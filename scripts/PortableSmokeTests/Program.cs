// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

// Load the published assemblies and native libraries, not a project-reference rebuild.
// No game window, live configuration, account or user library is opened.
string package = Path.GetFullPath(args[0]);
var resolver = new AssemblyDependencyResolver(Path.Combine(package, "osu!.dll"));
AssemblyLoadContext.Default.Resolving += (_, name) => resolver.ResolveAssemblyToPath(name) is { } path
    ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
AssemblyLoadContext.Default.ResolvingUnmanagedDll += (_, name) => resolver.ResolveUnmanagedDllToPath(name) is { } path
    ? NativeLibrary.Load(path) : IntPtr.Zero;

string scratch = Path.Combine(Path.GetTempPath(), "utau-portable-check-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(scratch);
try
{
    foreach (string name in OperatingSystem.IsWindows()
                 ? new[] { "realm-wrappers.dll", "bass.dll", "SDL3.dll" }
                 : OperatingSystem.IsMacOS()
                     ? new[] { "librealm-wrappers.dylib", "libbass.dylib", "libSDL3.dylib" }
                     : new[] { "librealm-wrappers.so", "libbass.so", "libSDL3.so" })
    {
        string[] matches = Directory.GetFiles(package, name, SearchOption.AllDirectories);
        if (matches.Length != 1) throw new InvalidOperationException($"Expected one {name}, found {matches.Length}.");
        IntPtr handle = NativeLibrary.Load(matches[0]);
        NativeLibrary.Free(handle);
    }

    var desktop = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(package, "osu!.dll"));
    foreach (var attribute in desktop.GetCustomAttributes<AssemblyMetadataAttribute>())
    {
        if (attribute.Key == "Sentry.ProjectDirectory") throw new InvalidOperationException("Local build path metadata remains in the package.");
    }
    var gameType = desktop.GetType("osu.Desktop.OsuGameDesktop", true)!;
    var game = Activator.CreateInstance(gameType, new object?[] { null })!;
    var updater = gameType.GetMethod("CreateUpdateManager", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, null)!;
    if (updater.GetType().FullName != "osu.Game.Updater.UpdateManager") throw new InvalidOperationException("Official updater is still enabled.");
    ((IDisposable)updater).Dispose();

    // Program's marker must be checked beside its own assembly, not the working directory or smoke runner.
    var portable = (bool)desktop.GetType("osu.Desktop.Program", true)!.GetProperty("IsPortableInstallation")!.GetValue(null)!;
    if (!portable) throw new InvalidOperationException("Portable marker was not detected.");

    var framework = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(package, "osu.Framework.dll"));
    var storageType = framework.GetType("osu.Framework.Platform.NativeStorage", true)!;
    // A stale root-level redirect must not be inherited by the nested portable data store.
    File.WriteAllText(Path.Combine(scratch, "storage.ini"), "FullPath = " + Path.Combine(scratch, "must-not-use"));
    var rootStorage = Activator.CreateInstance(storageType, new object?[] { scratch, null })!;
    var storage = gameType.GetMethod("CreateStorage", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, new[] { null, rootStorage })!;
    var fullPath = (string)storage.GetType().GetMethod("GetFullPath")!.Invoke(storage, new object[] { ".", false })!;
    if (Path.TrimEndingDirectorySeparator(fullPath) != Path.Combine(scratch, "data"))
        throw new InvalidOperationException("Portable data did not resolve under the package's data directory.");
    var gameAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(package, "osu.Game.dll"));
    var config = (IDisposable)Activator.CreateInstance(gameAssembly.GetType("osu.Game.Configuration.UtauEditorOsuConfigManager", true)!, storage)!;
    config.Dispose();
    if (!File.Exists(Path.Combine(scratch, "data", "game.utau-editor.ini")))
        throw new InvalidOperationException("Editor configuration was not written to portable data.");

    var realm = (IDisposable)Activator.CreateInstance(gameAssembly.GetType("osu.Game.Database.RealmAccess", true)!,
        new object?[] { storage, "client.realm", null })!;
    realm.Dispose();
    if (!File.Exists(Path.Combine(scratch, "data", "client.realm")))
        throw new InvalidOperationException("Database was not created in portable data.");
    // No host/DI load occurred. OsuGame's Dispose expects loaded services; let this
    // short-lived verifier exit instead of invoking a partial game lifecycle.
    Console.WriteLine("PASS: portable data/config/database, manual updates, published assemblies and native libraries.");
}
finally
{
    // Only the freshly generated scratch directory above can be removed.
    if (Path.GetDirectoryName(scratch) != Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())) ||
        !Path.GetFileName(scratch).StartsWith("utau-portable-check-", StringComparison.Ordinal))
        throw new InvalidOperationException("Unexpected scratch path; refusing cleanup.");
    Directory.Delete(scratch, true);
}
