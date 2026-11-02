# UTAU_Editor v3 更新日志

发布日期：2026-06-07

本文记录 `UTAU_Editor v2` 更新日志之后新增的编辑器功能。

## Approach rate 双值切换

- 新增 osu! compose 主编辑界面的 approach rate 快速切换功能。
- 新增默认快捷键 `Ctrl+Shift+Y`，可在按键设置中重新绑定。
- 新增两个可配置 AR 值：
  - `Approach rate toggle value A`，默认 `9.5`。
  - `Approach rate toggle value B`，默认 `6.5`。
- 切换逻辑：
  - 当前 AR 等于 A 时切换到 B。
  - 当前 AR 不等于 A 时切换到 A。
  - 切换范围限制在 `0.0` 到 `10.0`。
- 切换后会更新所有 hit object，并保存一次编辑器状态。

## 左侧 Sidebar 的 UTAU EDITOR 功能组

- 在 osu! compose 左侧 Sidebar 顶部新增 `UTAU EDITOR` 分组。
- `UTAU EDITOR` 分组当前包含 `Approach Rate` 功能按钮。
- 该分组通过 base `HitObjectComposer.CreateLeftToolboxGroups()` hook 在左侧 Sidebar 初始构建时插入，放在左侧 Sidebar 第一位，位于原有 `toolbox`、`toggles`、`bank` 之前。
- `Approach Rate` 按钮行为：
  - 点击按钮立即执行一次 AR 双值切换。
  - 同时打开或关闭 AR 配置 popover。
- 新增 `ApproachRateTogglePopover`：
  - 显示当前 AR。
  - 提供 `Toggle approach rate` 按钮。
  - 提供 `Value A` / `Value B` 两个 `FormSliderBar<float>` 配置控件。
  - 两个 slider 直接绑定到已有配置项，修改会持久化。

## 顶栏菜单整理

- 顶栏 `UTAU_Editor > Workflow` 中的 AR 相关条目已停用。
- 停用的条目包括：
  - `Toggle approach rate`
  - `Approach rate toggle value A`
  - `Approach rate toggle value B`
- 这些条目没有删除，而是以注释形式保留在代码中，便于日后回退或对照。
- `Nudge distance` 和 `Fixed-duration slider SV step` 等其他 workflow 设置仍保留在顶栏菜单中。

## UI 复用接口

- 扩展了 `EditorToolButton`，支持传入额外的按钮点击动作。
- 现在同一个 toolbox button 可以同时：
  - 执行即时动作。
  - 切换 popover。
- 新增 `UTAUEditorToolboxGroup.AddFeatureButton()`，作为后续 UTAU_Editor 自定义功能继续加入左侧 Sidebar 的复用入口。

## 相关文件

- `osu.Game/Screens/Edit/EditorApproachRateToggle.cs`
- `osu.Game.Rulesets.Osu/Edit/ApproachRateTogglePopover.cs`
- `osu.Game.Rulesets.Osu/Edit/UTAUEditorToolboxGroup.cs`
- `osu.Game.Rulesets.Osu/Edit/OsuHitObjectComposer.cs`
- `osu.Game/Screens/Edit/Components/EditorToolButton.cs`
- `osu.Game/Screens/Edit/Editor.cs`
- `osu.Game/Configuration/OsuConfigManager.cs`
- `osu.Game/Input/Bindings/GlobalActionContainer.cs`
- `osu.Game/Localisation/EditorStrings.cs`
- `osu.Game/Localisation/GlobalActionKeyBindingStrings.cs`

## 验证状态

- 本轮未运行 build。
- 按当前开发约定，编译与运行验证由用户手动完成。
- 已做静态检查：`git diff --check`，仅出现仓库已有的 CRLF 提示。
