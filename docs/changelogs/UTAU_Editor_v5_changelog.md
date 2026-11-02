# UTAU_Editor v5 更新日志

发布日期：2026-06-15

本文记录 `UTAU_Editor v4` 之后完成的全部功能更新，重点列出实际行为、UI 入口、配置入口和核心代码位置。

## 内置 UTAU Editor 默认配置

- 将当前个人使用的常用配置、ruleset 设置和快捷键写入程序默认值。
- UTAU Editor 继续使用独立配置文件 `osu!.utau-editor.ini`，避免与官方 osu!lazer 配置相互覆盖。
- 普通 osu! 配置由 `UtauEditorOsuConfigManager` 提供默认值。
- Framework 配置首次应用后，通过 `UtauEditorFrameworkDefaultsApplied` 标记，避免每次启动重复覆盖。
- Ruleset 设置和快捷键采用内置默认值；已有的持久化用户配置仍由对应配置系统管理。

### 通用与编辑器默认值

| 配置 | 默认值 |
|---|---:|
| 显示转换谱面 | `false` |
| 选歌排序 | `DateAdded` |
| Cursor rotation | `false` |
| 24 小时时间格式 | `false` |
| 首次运行引导 | `false` |
| UI scale | `1.1` |
| Editor background dim | `0` |
| Waveform opacity | `0.75` |
| Selection outline thickness | `2` |
| Selection box outline thickness | `2` |
| Selection hit circle / slider / box / control colour | `05f4fd` |
| Selection control button size | `15` |
| Selection scale handle size | `6` |
| Selection rotation handle size | `12` |
| Slider control line colour | `ff5a5a` |
| Slider control point size | `8` |
| Slider control point hit area | `20` |
| Slider tail arc colour | `05f4fd` |
| Slider tail arc thickness | `2` |
| Convert to stream acceleration | `-0.1` |
| Random perturb mode | `Square` |
| Random perturb strength | `8.5` |
| Random perturb seed | `4713` |
| Approach rate value A / B | `6.0` / `9.2` |
| Scale / rotation origin | `SelectionCentre` |
| Timeline show breaks | `false` |
| Contract sidebars | `true` |

### Framework 首次默认值

| 配置 | 默认值 |
|---|---:|
| Confine mouse | `Never` |
| Universal volume | `100%` |
| Music volume | `99%` |
| Effect volume | `0%` |
| Frame sync | `Unlimited` |
| Window mode | `Windowed` |
| Locale | `en` |

### Ruleset 默认值

- osu!：
  - Slider snaking out：关闭。
  - Cursor trail：关闭。
  - Replay click markers：开启。
  - Replay cursor path：开启。
  - Replay analysis display length：`400`。
- osu!mania：
  - Scroll speed：`17.0`。
  - 4K 默认按键：`Z`、`X`、`.`、`/`。
- osu!standard 默认按键：
  - Left button：`X`。
  - Right button：`C`。
  - Smoke：无默认按键。
- osu!taiko 默认按键：
  - Left rim：`Z` / `C`。
  - Left centre：`X`。
  - Right centre：`.`。
  - Right rim：`/`。

### Editor 默认快捷键调整

| 动作 | 默认快捷键 |
|---|---|
| Verify mode | 无 |
| Fixed-duration slider SV increase | `Ctrl+Shift+Up` |
| Fixed-duration slider SV decrease | `Ctrl+Shift+Down` |
| Toggle approach rate | `Ctrl+Shift+A` |
| Canvas zoom in | `Ctrl+Shift+MouseWheelLeft` |
| Canvas zoom out | `Ctrl+Shift+MouseWheelRight` |
| Canvas zoom reset | `Ctrl+Shift+MouseMiddle` |
| Pan editor canvas | `MouseMiddle` |

### 核心代码入口

- `osu.Game/Configuration/UtauEditorOsuConfigManager.cs`
  - UTAU Editor 通用和编辑器默认配置。
- `osu.Game/OsuGameBase.cs`
  - 独立 config manager 接入。
  - Framework 首次默认值应用和完成标记。
- `osu.Game/Input/Bindings/GlobalActionContainer.cs`
  - Editor 默认快捷键。
- `osu.Game.Rulesets.Osu/Configuration/OsuRulesetConfigManager.cs`
  - osu! ruleset 默认设置。
- `osu.Game.Rulesets.Mania/Configuration/ManiaRulesetConfigManager.cs`
  - mania 默认滚速。
- `osu.Game.Rulesets.Osu/OsuRuleset.cs`
  - osu!standard 默认按键。
- `osu.Game.Rulesets.Taiko/TaikoRuleset.cs`
  - taiko 默认按键。
- `osu.Game.Rulesets.Mania/SingleStageVariantGenerator.cs`
  - mania 4K 默认按键。

## Slider velocity

- 新增 slider 变速工具，在保持 slider 总时长的同时改变沿原路径移动的速度分布。
- 工具会重新采样原 slider 路径，并在 slider body 内加入短距离往返的隐藏路径。
- 新路径增加的总长度与 SV multiplier 同比例变化，因此 slider 时长保持不变。
- 生成后的 slider 可以表现为加速、减速、先加速后减速、波动、弹跳等速度曲线。

### UI 入口

- 选中单个 slider。
- 打开右键菜单。
- 选择 `Slider velocity...`。
- 当前没有默认快捷键。
- 初版曾提供左侧 Sidebar 按钮；最终已移除该重复入口，仅保留 slider 右键菜单。

### 速度曲线

| Curve | 行为 |
|---|---|
| `Linear` | 保持恒定速度分布 |
| `Accelerate` | 从慢到快 |
| `Decelerate` | 从快到慢 |
| `Accelerate then decelerate` | 中段最快 |
| `Decelerate then accelerate` | 中段最慢 |
| `Speed wave` | 周期性速度波动 |
| `Bounce` | 带衰减的弹跳变化 |
| `Elastic` | 弹性振荡 |
| `Back` | 带回拉感的前后变化 |

### 参数

| 参数 | 默认值 | 范围 | 用途 |
|---|---:|---:|---|
| Curve | `AccelerateThenDecelerate` | 9 种曲线 | 选择速度分布 |
| Intensity | `100%` | `0–100%` | 在恒速和目标曲线之间混合 |
| Fixed SV multiplier | `2x` | `1–10x` | 提供增加隐藏路径所需的速度和长度空间 |
| Power | `3` | `1–8` | 控制加速、减速、Bounce 曲线强度 |
| Frequency | `3` | `1–8` | 控制 Wave、Bounce、Elastic 的振荡次数 |
| Overshoot | `35%` | `0–100%` | 控制 Wave 振幅以及 Elastic / Back 的过冲 |
| Curve samples | `96` | `24–256` | 控制对原路径和速度曲线的采样精度 |
| Maximum hidden segment | `6px` | `1–12px` | 限制每段隐藏往返路径的长度 |

### 预览与提交

- 修改任意参数会实时恢复原 slider 状态并重新生成预览。
- 调整参数时 popover 会临时降低到约 `22%` 不透明度，减少视野遮挡。
- 结果区域显示：
  - 实际 SV multiplier。
  - 实际应用的 Intensity。
  - 生成后的控制点总数。
- 如果请求的 Intensity 无法在当前 SV multiplier 下实现，工具会自动降低实际 Intensity。
- 对原 SV 不高于 `10x` 的 slider，生成后的实际 slider SV 不会超过 `10x`。
- 控制点数量上限为 `12000`；无法在限制内生成时会禁用 `OK`。
- 点击 `OK` 提交修改。
- 点击 `Cancel` 或直接关闭 popover 会恢复原路径和原 SV。
- 整个预览过程使用 editor change transaction，可作为一次编辑操作处理。

### 实现说明

- 输出路径使用线性控制点重新构建。
- `Curve samples` 越高，对原路径跟随越准确，但会增加控制点数量。
- `Maximum hidden segment` 越小，隐藏往返更短、更容易留在 slider body 内，但也会增加控制点数量。
- Intensity 是否能达到目标值同时受以下因素影响：
  - 原 slider 长度和形状。
  - Fixed SV multiplier。
  - Curve 类型。
  - Power / Frequency / Overshoot。
  - Curve samples。

### 配置入口

配置定义位于 `osu.Game/Configuration/OsuConfigManager.cs`：

- `EditorSliderVelocityCurve`
- `EditorSliderVelocityIntensity`
- `EditorSliderVelocityMultiplier`
- `EditorSliderVelocityPower`
- `EditorSliderVelocityFrequency`
- `EditorSliderVelocityOvershoot`
- `EditorSliderVelocitySampleCount`
- `EditorSliderVelocityMaxDetourLength`

UTAU Editor 内置默认值同步定义于：

- `osu.Game/Configuration/UtauEditorOsuConfigManager.cs`

### 核心代码入口

- `osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/SliderSelectionBlueprint.cs`
  - Slider 右键菜单入口和 popover 创建。
- `osu.Game.Rulesets.Osu/Edit/SliderVelocityPopover.cs`
  - 参数 UI、实时预览、提交与回滚。
- `osu.Game.Rulesets.Osu/Edit/SliderVelocityTransformer.cs`
  - 速度曲线计算、路径采样、隐藏往返路径生成和 Intensity 自适应。
- `osu.Game.Rulesets.Osu/Edit/UTAUEditorToolboxGroup.cs`
  - Slider velocity Sidebar 入口已从此处移除。

## 物件出界摆放与移动

- 新增可随时开启或关闭的 playfield 边界限制。
- 默认保持官方行为，不允许将物件放置或移动到 playfield 外。
- 开启后，可在编辑器主画布范围内的 playfield 外部放置和操作物件。
- 开关状态通过 UTAU Editor 配置持久化。

### UI 入口

- 左侧 Sidebar：`UTAU EDITOR > Out-of-bounds placement`。
- 按钮高亮时允许出界摆放和移动。
- 再次点击后恢复 playfield 边界限制。
- 该功能没有额外 popover 或快捷键入口。

### 支持的操作

开启后，以下操作允许产生场外坐标：

- 在 playfield 外开始放置 hit circle。
- 在 playfield 外开始放置 slider，并继续编辑场外路径。
- 使用鼠标拖动已选物件到 playfield 外。
- 使用 `Ctrl+方向键` 将已选物件微移到 playfield 外。
- 使用 `Transform > Move` 精确移动物件到 playfield 外。
- 使用非居中原点执行 Flip，并保留产生的场外位置。
- 使用 Polygon generation 生成包含场外顶点的图案。
- 使用位置网格吸附时保留场外吸附坐标。
- 使用距离吸附时保留场外吸附结果。

关闭后，上述操作恢复原始边界行为：

- 新物件只能从 playfield 内开始放置。
- 拖动和键盘微移后的选择会被校正回 playfield。
- 精确移动重新使用基于当前选择范围计算的坐标边界。
- Flip、Polygon generation 和吸附结果重新执行边界检查。

### 配置入口

配置定义位于 `osu.Game/Configuration/OsuConfigManager.cs`：

- `OsuSetting.EditorAllowOutOfBoundsPlacement`
  - 默认值：`false`。
  - `true`：允许出界摆放和移动。
  - `false`：启用官方 playfield 边界限制。

UTAU Editor 内置默认值同步定义于：

- `osu.Game/Configuration/UtauEditorOsuConfigManager.cs`

### 核心代码入口

- `osu.Game.Rulesets.Osu/Edit/UTAUEditorToolboxGroup.cs`
  - 创建 Sidebar 开关并连接持久化配置。
- `osu.Game.Rulesets.Osu/Edit/OsuHitObjectComposer.cs`
  - 定义允许摆放物件的屏幕区域。
  - 控制位置网格和距离吸附的边界校正。
- `osu.Game.Rulesets.Osu/Edit/Blueprints/HitCircles/HitCirclePlacementBlueprint.cs`
  - 接收 playfield 外的 hit circle 放置输入。
- `osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/SliderPlacementBlueprint.cs`
  - 允许 slider 从 playfield 外开始放置。
- `osu.Game.Rulesets.Osu/Edit/OsuSelectionHandler.cs`
  - 控制拖动、键盘微移和 Flip 的边界校正。
- `osu.Game.Rulesets.Osu/Edit/PreciseMovementPopover.cs`
  - 控制精确移动的 X/Y 可输入范围。
- `osu.Game.Rulesets.Osu/Edit/PolygonGenerationPopover.cs`
  - 控制场外多边形顶点是否阻止生成。
- `osu.Game/Localisation/EditorStrings.cs`
  - Sidebar 按钮文本。

## 验证状态

- Slider velocity 已完成编译问题和配置类型问题修复。
- 已由用户确认编辑器可以正常放置 playfield 边界外物件。
- 已执行静态补丁检查。
- 按当前开发约定，本轮更新日志编写未运行 build 或自动化测试。
