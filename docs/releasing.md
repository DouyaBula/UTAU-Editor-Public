# Releases and upstream maintenance

## Make a portable package locally

Install .NET 10 (see `global.json`) and PowerShell 7. From the repository root:

```powershell
./scripts/package-portable.ps1 -Runtime win-x64 -Version 0.1.1
dotnet run --project scripts/PortableSmokeTests -c Release -- artifacts/portable/UTAU-Editor-0.1.1-win-x64
```

Use a **new output directory** for each run (`-OutputRoot artifacts/try-2`). The script refuses to reuse an existing staging folder/archive: previously launched binaries may contain private maps or account data. It publishes self-contained, untrimmed, multi-file builds, checks critical native assets, includes notices/dependency versions, then archives with a SHA-256 sidecar. PDBs and absolute project paths are excluded. Do not zip your working installation or add a `data/` directory to a release.

Supported RIDs in the script are `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`. Build Unix archives on Unix so executable permissions are preserved. macOS native libraries are checked for the requested architecture. Graphics/audio functionality still needs a manual check on the target system.

`PortableSmokeTests` loads the actual published assemblies and critical native libraries without opening the application window. It checks the portable storage override and manual updater in a freshly generated temporary directory. It does not read the developer's library and does not claim a complete GUI test.

## GitHub Release

The intended public repository is `DouyaBula/UTAU-Editor-Public`, with `UTAU_Editor` as its default branch. The earlier development repository stays private and is not the release destination. Check `git remote -v` before pushing; in a private maintenance clone, use a separate public remote instead of repointing the old one.

The **Portable release** workflow builds all six desktop variants on matching GitHub-hosted runners with the latest stable .NET 10 SDK/runtime patch allowed by `global.json`. ARM runners may have availability/plan restrictions, particularly in private repositories. No developer signing key or external package-publishing secret is required; only the release job gets `contents: write`.

Two entry points:

1. **Actions → Portable release → Run workflow** builds downloadable workflow artifacts only. Choose an `X.Y.Z` version.
2. Push a new version tag such as `utau-editor-v0.1.1` **on the reviewed public branch**. When all jobs pass, it creates a **draft** Release containing ZIP/TAR.GZ packages, `SHA256SUMS.txt` and an optional pose-companion source ZIP. Inspect it and click **Publish release** yourself. Enabling Actions does not replay an older tag push; do not delete/recreate a published tag to trigger a build.

```sh
git tag -a utau-editor-v0.1.1 -m "UTAU Editor 0.1.1"
git push origin refs/tags/utau-editor-v0.1.1
```

Before tagging, update `docs/release-notes.md` and the default project version. Choose commit/tag identity suitable for a public repository; Git stores names and email addresses in both. Tag pushes are deliberate publication actions, not part of normal local builds. Rerun failed jobs rather than reusing an existing published version/tag. An existing Release is not silently overwritten by this workflow.

This is informal distribution: no installer, official updater, code-signing certificate or Apple notarisation is configured. See [portable usage](portable.md) for the resulting user workflow. OS libraries and compatible drivers remain required.

## Keep official upstream mergeable

The public initial custom snapshot has the last merged official commit, `676f3b01f2`, as its parent. All official history stays reachable; private custom development commits are not parents of that snapshot. This is a squash of the custom changes, **not an unrelated orphan repository**.

For future updates, start from a clean public-development branch and make a temporary integration branch:

```sh
git remote add upstream https://github.com/ppy/osu.git  # once
git fetch upstream
git switch -c codex/upstream-sync
git merge upstream/master
```

Resolve custom-feature conflicts, build and run the relevant regressions, then test the editor. Pay particular attention to persisted `GlobalAction` values, Realm migrations, curve topology/Keep duration, preview transactions, storage isolation and release workflow changes. Do not restore upstream deployment credentials/workflows or its automatic updater. Merge the reviewed integration branch back into the public branch.

Do not merge the old private custom branch into the public branch: that would republish its ancestry. Keep old history in an offline bundle/private clone for reference. Subsequent custom work should branch from the new public snapshot.

## First-publication privacy checklist

Rewriting a Git branch removes old commits from that branch's visible ancestry, **not necessarily from GitHub caches, pull-request refs, forks, Actions logs, artifacts, releases or other copies**. For the strongest separation, publish the prepared snapshot to a new empty public repository and keep the old repository private. If reusing a private repository, review all refs and server-side artifacts before changing visibility; GitHub Support may be required for actual sensitive-data removal. Rotate any exposed credentials—history rewriting is not revocation.

Before publication, confirm repository destination, public author/committer identity, default branch, version/tag and whether the prepared tree is approved. Push only the intended branch/tag, never `--mirror` or all local backup refs. Keep the offline backup outside the repository. Do not claim all past copies have been erased.

## 中文速览

- 本地运行 `scripts/package-portable.ps1` 即可生成自包含压缩包；输出目录必须是新的，不能把使用过的客户端目录直接打包。
- Actions 手动运行只产出构建附件；推送 `utau-editor-vX.Y.Z` 标签后，全部平台成功才创建 **Release 草稿**，检查后再手动发布。
- 当前提供六个桌面架构的流程，跨平台构建／原生库检查不等于所有系统的完整实机验收。
- 公开首提交接在最后合并的官方提交后面，保留共同祖先；未来正常 fetch／merge 官方。不要再合并旧私有开发分支，否则旧历史也会重新进入公开库。
- 隐私优先建议新建空公开库承接干净历史；覆盖旧分支不保证 GitHub 缓存、PR、Actions 日志等历史副本消失。发布前确认公开提交身份和仓库去向。
