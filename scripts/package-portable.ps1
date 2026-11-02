[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')][string]$Runtime,
    [Parameter(Mandatory)][ValidatePattern('^\d+\.\d+\.\d+$')][string]$Version,
    [string]$OutputRoot = 'artifacts/portable',
    [string]$Dotnet = 'dotnet'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$output = [IO.Path]::GetFullPath($OutputRoot)
$name = "UTAU-Editor-$Version-$Runtime"
$stage = Join-Path $output $name
$extension = if ($Runtime.StartsWith('win-')) { '.zip' } else { '.tar.gz' }
$archive = Join-Path $output ($name + $extension)

# Never package an old publish directory: it could contain maps, tokens or logs from a previous launch.
if ((Test-Path -LiteralPath $stage) -or (Test-Path -LiteralPath $archive)) {
    throw "Output already exists. Choose a fresh OutputRoot: $output"
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null

Push-Location $repo
try {
    & $Dotnet publish osu.Desktop/osu.Desktop.csproj -c Release -r $Runtime --self-contained true -o $stage `
        "-p:Version=$Version" '-p:AssemblyVersion=0.0.0.0' '-p:ContinuousIntegrationBuild=true' `
        '-p:DebugType=None' '-p:DebugSymbols=false' '-p:RunAnalyzers=false' `
        '-p:PublishSingleFile=false' '-p:PublishTrimmed=false' '-p:IncludeSourceRevisionInInformationalVersion=false'
    if ($LASTEXITCODE -ne 0) { throw 'dotnet publish failed.' }

    $dependencies = & $Dotnet list osu.Desktop/osu.Desktop.csproj package --include-transitive --format json --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Could not enumerate dependency versions.' }
    # NuGet reports absolute project paths; publish repository-relative paths only.
    $dependencies = ($dependencies -join "`n") | ConvertFrom-Json
    $dependencies.projects | ForEach-Object { $_.path = [IO.Path]::GetRelativePath($repo, $_.path).Replace('\', '/') }
    $dependencies | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath (Join-Path $stage 'dependencies.json') -Encoding utf8NoBOM

    foreach ($required in @('utau-editor.portable', 'osu!.dll', 'osu!.runtimeconfig.json', 'LICENCE', 'PORTABLE.md')) {
        if (!(Test-Path -LiteralPath (Join-Path $stage $required))) { throw "Missing package file: $required" }
    }

    # Check critical native assets, not just managed build success.
    $nativeNames = if ($Runtime.StartsWith('win-')) {
        @('osu!.exe', 'coreclr.dll', 'realm-wrappers.dll', 'bass.dll', 'SDL3.dll')
    } elseif ($Runtime.StartsWith('linux-')) {
        @('osu!', 'libcoreclr.so', 'librealm-wrappers.so', 'libbass.so', 'libSDL3.so')
    } else {
        @('osu!', 'libcoreclr.dylib', 'librealm-wrappers.dylib', 'libbass.dylib', 'libSDL3.dylib')
    }
    foreach ($native in $nativeNames) {
        if (!(Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object Name -EQ $native)) { throw "Missing native asset: $native" }
    }
    if ((Test-Path -LiteralPath (Join-Path $stage 'data')) -or
        (Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object { $_.Extension -in '.realm', '.log', '.pdb' -or $_.Name -in 'storage.ini', 'game.utau-editor.ini', 'framework.ini' })) {
        throw 'Unexpected user data or local debug symbols in package.'
    }

    foreach ($assembly in Get-ChildItem -LiteralPath $stage -Filter 'osu*.dll') {
        $bytes = [IO.File]::ReadAllBytes($assembly.FullName)
        foreach ($encoding in @([Text.Encoding]::UTF8, [Text.Encoding]::Unicode)) {
            if ($encoding.GetString($bytes).Contains($repo, [StringComparison]::OrdinalIgnoreCase)) {
                throw "Local checkout path embedded in $($assembly.Name). Do not distribute this package."
            }
        }
    }

    if (!$Runtime.StartsWith('win-')) {
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'launch.sh') -Destination (Join-Path $stage 'launch.sh')
        & chmod +x (Join-Path $stage 'osu!') (Join-Path $stage 'launch.sh')
        if ($LASTEXITCODE -ne 0) { throw 'Could not set executable permissions.' }
    }
    if ($Runtime.StartsWith('osx-')) {
        $arch = if ($Runtime -eq 'osx-arm64') { 'arm64' } else { 'x86_64' }
        foreach ($binary in Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object { $_.Extension -eq '.dylib' -or $_.Name -eq 'osu!' }) {
            & lipo -verify_arch $arch $binary.FullName
            if ($LASTEXITCODE -ne 0) { throw "Wrong architecture: $($binary.Name)" }
        }
    }

    if ($Runtime.StartsWith('win-')) {
        Compress-Archive -LiteralPath $stage -DestinationPath $archive -CompressionLevel Optimal
    } else {
        & tar -czf $archive -C $output $name
        if ($LASTEXITCODE -ne 0) { throw 'Archive creation failed.' }
    }
    $hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($archive))" | Set-Content -LiteralPath ($archive + '.sha256') -Encoding utf8NoBOM
    Write-Output "Created $archive"
} finally {
    Pop-Location
}
