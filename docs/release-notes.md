# UTAU Editor 0.1.0

Initial public snapshot of the personal osu!lazer editor fork. See the [English README](https://github.com/DouyaBula/UTAU-Editor-Public#readme) or [中文介绍](https://github.com/DouyaBula/UTAU-Editor-Public/blob/UTAU_Editor/README.zh-CN.md) for the complete feature overview.

Highlights: Classic/Pen curve editing and duration-preserving SV; three slider generation/animation toolboxes; Pattern Gallery; Hitsound/MIDI tools; global timing offset; star rating and summary-timeline strain; configurable visuals, grid, canvas and sidebar workflow.

Download the archive matching your operating system and CPU. Extract fully to a writable folder; start `osu!.exe` (Windows) or `./launch.sh` (Linux/macOS). .NET is included. Keep `utau-editor.portable` beside the executable; user data is stored under `data/`. Use `.osz` / `.osk` imports, not the official client's live database. Back up before upgrading. Builds are unsigned; macOS is not notarised. Linux requires desktop/graphics/audio system libraries. Check `SHA256SUMS.txt` for archive integrity.

The optional pose companion source ZIP requires Python/dependencies; models and videos are not bundled. Its Windows setup/run scripts are included.

This is an unofficial, non-commercial personal project with no ppy support. Significant LLM-assisted code has focused tests but no comprehensive independent audit. Keep backups and verify exports in the intended client. Known limitations and dependency warnings are documented in the maintainer notes. Build/native-load checks do not imply full manual validation on every platform.

---

首个公开快照，整合此前所有自定义编辑功能。下载匹配系统和 CPU 的便携包，完整解压后运行；数据在 `data/`，请勿与官方客户端共用数据库。更新前备份，跨客户端使用 `.osz`／`.osk` 导入。独立姿态工具源码为可选附件，不含模型／视频，需要 Python 环境。

非官方个人分享、未签名、无官方支持；macOS 未公证，Linux 仍需系统运行库。各平台构建及原生库检查通过后才进入本草稿，但不等同于所有平台都完成了人工交互测试。
