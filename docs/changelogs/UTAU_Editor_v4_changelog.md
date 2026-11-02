# UTAU_Editor v4 更新日志

发布日期：2026-06-11

本文记录 `UTAU_Editor v3` 之后新增的画布视图控制和物件缩放能力，重点列出后续维护所需的 UI、配置与代码入口。

## 编辑器画布缩放

- osu! compose 编辑器支持整体缩放画布视图。
- 缩放统一作用于 ruleset playfield、辅助网格和 blueprint，确保显示、放置、选择及控制点操作使用同一套坐标变换。
- 画布缩放仅改变编辑视图，不修改 hit object 位置、slider 控制点或 beatmap 数据。
- 默认缩放范围为 `10%` 到 `200%`。

### UI 入口

- 左侧 Sidebar：`UTAU EDITOR > Canvas zoom`。
- `Canvas zoom` popover 提供：
  - 当前画布缩放比例。
  - 快捷缩放步长。
  - Reset 使用的缩放比例。
  - `Zoom out`、`Reset`、`Zoom in` 按钮。
- `Reset` 会同时恢复配置的缩放比例并将画布平移位置归中。

### 快捷键入口

- `Ctrl+Plus`：放大画布。
- `Ctrl+Minus`：缩小画布。
- `Ctrl+0`：恢复 Reset 缩放比例并将画布归中。
- 以上动作位于按键设置的 `Editor` 分类，可重新绑定。

### 配置入口

配置定义位于 `osu.Game/Configuration/OsuConfigManager.cs`：

- `EditorCanvasZoom`
  - 默认 `1.0`。
  - 范围 `0.1` 到 `2.0`。
- `EditorCanvasZoomStep`
  - 默认 `0.1`。
  - 范围 `0.01` 到 `0.5`。
- `EditorCanvasZoomResetValue`
  - 默认 `1.0`。
  - 范围 `0.1` 到 `2.0`。

## 编辑器画布平移

- osu! compose 编辑器支持按住指定按键并拖动鼠标，自由平移画布视图。
- 默认绑定为鼠标中键。
- 平移由 Editor 顶层全局按键动作处理，不依赖 playfield、物件 blueprint 或 slider 控制点接收拖拽事件，因此可在选择和放置状态下使用。
- 平移偏移只属于当前编辑器视图，不写入 beatmap 数据。

### 快捷键入口

- 按键设置路径：`Settings > Input > Keyboard > Editor > Pan editor canvas`。
- 默认绑定：鼠标中键。
- 对应全局动作：`GlobalAction.EditorPanCanvas`。

### 交互变更

- 鼠标中键不再执行快速删除，以避免与画布平移冲突。
- 快速删除继续使用 `Shift+右键`。

## 允许缩放物件到 Playfield 外

- osu! ruleset 的选择缩放不再按 playfield 边界裁剪最大缩放倍数。
- 以下入口共享同一行为：
  - 拖动 selection box 的 scale handle。
  - `Transform > Scale` 精确缩放 popover。
  - 其他调用 `OsuSelectionScaleHandler` 的缩放操作。
- 多选图案、hit circle 和 slider 可被缩放到 playfield 外。
- 单个 slider 缩放仍保留路径有效性检查。
- 缩放仍保留最小正数限制，避免拖动 scale handle 穿过原点后意外翻转图案。
- `Transform > Scale` 的数值范围不再因当前选择靠近 playfield 边界而缩小；当前 UI 最大值仍为 `10x`。

## 核心代码入口

### 画布视图

- `osu.Game/Rulesets/Edit/HitObjectComposer.cs`
  - 将画布缩放和平移应用到 `PlayfieldContentContainer`。
  - 该容器统一承载 ruleset、网格和 blueprint。
- `osu.Game/Screens/Edit/Editor.cs`
  - 处理画布缩放快捷键。
  - 处理 `EditorPanCanvas` 的按下、释放和鼠标位移。
  - 保存当前会话的 `CanvasPanOffset`。
- `osu.Game.Rulesets.Osu/Edit/CanvasZoomPopover.cs`
  - 画布缩放配置和按钮 UI。
- `osu.Game.Rulesets.Osu/Edit/UTAUEditorToolboxGroup.cs`
  - 左侧 `Canvas zoom` 功能入口。

### 按键与文本

- `osu.Game/Input/Bindings/GlobalActionContainer.cs`
  - 画布缩放、重置和平移动作及默认绑定。
- `osu.Game/Localisation/GlobalActionKeyBindingStrings.cs`
  - 按键设置中的动作名称。
- `osu.Game/Localisation/EditorStrings.cs`
  - Canvas zoom popover 的 UI 文本。

### 物件缩放

- `osu.Game.Rulesets.Osu/Edit/OsuSelectionScaleHandler.cs`
  - 统一取消 playfield 边界对缩放倍数的限制。
  - 保留轴向约束、防翻转约束和 slider 路径有效性检查。
- `osu.Game.Rulesets.Osu/Edit/PreciseScalePopover.cs`
  - `Transform > Scale` 的数值 UI。
  - 通过共享 scale handler 获取可用缩放范围。

### 中键输入兼容

- `osu.Game/Screens/Edit/Compose/Components/SelectionHandler.cs`
  - 快速删除改为仅使用 `Shift+右键`。
- `osu.Game/Screens/Edit/Compose/Components/BlueprintContainer.cs`
  - 中键不参与选择、框选或物件拖动。
- `osu.Game/Rulesets/Edit/PlacementBlueprint.cs`
  - 放置过程中允许中键输入透传给画布平移动作。

## 验证覆盖

- 画布缩放后仍可正确选择和放置物件。
- 画布平移只改变屏幕位置，不修改 hit object 数据。
- 多选物件可缩放到 playfield 边界外。
- 鼠标中键不再删除物件，`Shift+右键` 快速删除仍保留。
- 本轮未运行完整 build；已执行 `git diff --check`，仅有 CRLF 提示。
