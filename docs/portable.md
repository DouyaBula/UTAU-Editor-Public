# Portable UTAU Editor / 便携使用

Download only the asset matching your operating system and CPU. Extract the **entire** archive into a writable folder. Do not run inside the ZIP. .NET is included; graphics drivers and operating-system libraries are not.

| Package | Start | Notes |
| --- | --- | --- |
| `win-x64` / `win-arm64` | `osu!.exe` | Windows 10/11; unsigned personal build. |
| `linux-x64` / `linux-arm64` | `./launch.sh` | Modern glibc desktop Linux, built on Ubuntu 24.04; graphics, ICU, OpenSSL, X11/Wayland and audio libraries must be installed. |
| `osx-x64` / `osx-arm64` | `./launch.sh` in Terminal | Intel / Apple Silicon, respectively; a terminal-launch distribution, not a notarised `.app` installer. Use a macOS release supported by .NET 10. |

macOS may block downloaded unsigned executables. Review the release source/checksum and use the per-app **Privacy & Security → Open Anyway** controls if you trust it. Do not disable system-wide protections. Native ARM64/x64 packages are built and smoke-checked on matching CI runners; a successful build is not full graphics/audio testing on every machine.

## Data and updates

`utau-editor.portable` activates local storage. Keep it next to the executable. The default layout is:

```text
UTAU-Editor-<version>-<platform>/
  osu!.exe or osu!
  utau-editor.portable
  framework.ini             # may appear after starting
  data/                     # created on first run
    client.realm            # maps, skins, bindings database
    files/                  # imported resources
    game.utau-editor.ini    # editor settings
    pattern-gallery/        # reusable pattern library
    exports/
```

The first launch intentionally starts with a separate library. Import `.osz` beatmaps and `.osk` skins by dragging them onto this window. Import Gallery JSON from its panel. **Do not reuse an official client's `storage.ini` / Realm database or share its live data folder.** Game settings allow an explicit storage override; if you set one, the application is no longer fully portable and uses that chosen directory instead.

For an update: close both versions, back up `data/`, extract the new package, and copy/move your `data/` and optional `framework.ini` into it. Retain a backup before trying a newer database version; do not downgrade the same live database. No automatic updater runs. Nothing is registered as the system `.osz` / `osu://` handler.

## 中文说明

完整解压匹配系统和 CPU 的压缩包到可写目录。Windows 双击 `osu!.exe`；Linux／macOS 在终端运行 `./launch.sh`。无需单独安装 .NET，但 Linux 的桌面／图形／音频等系统依赖仍须具备。macOS 包未签名公证，不是 `.app` 安装器；只在确认来源可信后使用系统针对单个程序的放行入口。

`utau-editor.portable` 和程序放在一起，首次运行创建独立 `data/`，所以初始没有旧客户端的谱面和皮肤是正常的。通过 `.osz`／`.osk` 导入，Gallery 使用其 JSON 导入。不要复制官方端的 Realm 或指向其正在使用的数据目录。若主动在设置中指定其他数据目录，数据会存到指定位置，便携性相应改变。

升级先关闭两版程序、备份 `data/`，再把它和可选的 `framework.ini` 移到新包中。请保留升级前备份，不对同一数据库直接降级。程序不会自动更新或抢占官方文件关联。
