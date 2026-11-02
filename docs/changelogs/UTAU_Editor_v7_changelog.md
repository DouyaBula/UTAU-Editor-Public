# UTAU_Editor v7 更新日志

发布日期：2026-06-16

本文记录 `UTAU_Editor v6` 之后新增的 Shape slider 序列生成功能，重点说明功能行为、UI 入口、参数语义、配置项和代码入口。

## Shape Slider Animation

新增 Shape slider 序列生成器，用于制作由多条 slider 组成的逐帧动画。

- 使用一个固定的 Shape 类型生成整段序列。
- 每个可用 Shape 参数均提供 `Start` 和 `End` 值。
- Start 与 End 相同时，该参数在序列中保持不变。
- Start 与 End 不同时，该参数按照所选运动曲线逐帧插值。
- 可同时改变多个参数，例如形状参数、head 位置、3D 旋转和整体大小。
- 每个生成结果都是独立 slider，并按顺序放置在时间轴上。
- 参数变化会实时重建整段序列预览。
- 点击 `Create sequence` 提交全部 slider。
- 点击 `Cancel` 或关闭 popover 会移除预览并回滚本次 editor change transaction。

### UI 入口

- 左侧 Sidebar：`Generate > Shape animation`。
- 当前没有默认快捷键。

Popover 延续 `Shape slider` 生成器的 UI 结构：

- 左侧使用 `POLYGON / CURVES / FRACTALS / 3D` 分类按钮。
- 每个分类可折叠，并使用可滚动的双列 Shape 按钮阵列。
- 当前 Shape 高亮显示，较长名称提供 tooltip。
- 右侧只显示当前 Shape 实际使用的参数。
- 每个参数在同一行并排显示 `Start` 和 `End`。
- 底部集中显示运动曲线、帧数、单帧时长和帧间隔。
- `Panel opacity` 可手动调整整个 popover 的透明度。

## 动画参数

### Shape 参数插值

序列支持对当前 Shape 使用的一个或多个参数同时插值：

- Polygon vertices
- Iterations / depth
- Sampling resolution
- Primary curve parameter
- Secondary curve parameter
- Julia constant real / imaginary
- Head position
- Polyhedron faces
- Quaternion X / Y / Z / W
- Perspective
- Scale

整数和离散参数会在插值后取整。

正多面体面数会继续吸附到：

- `4`
- `6`
- `8`
- `12`
- `20`

四元数使用归一化插值，并在起止四元数方向相反时自动选择较短旋转方向。

### Motion Curve

`Motion curve` 控制 Start 到 End 的逐帧插值进度。

支持以下预设：

| Curve | 行为 |
|---|---|
| `Linear` | 匀速变化 |
| `Ease in` | 前段较慢，后段加速 |
| `Ease out` | 前段较快，后段减速 |
| `Ease in / out` | 起止较慢，中段较快 |
| `Smooth step` | 平滑的起止过渡 |

第一帧始终使用 Start 值，最后一帧始终使用 End 值。

### Frames

- 表示序列中生成的 slider 数量，即动画帧数。
- 范围：`2-64`。
- 默认值：`8`。

### Frame Duration

`Frame duration` 使用当前编辑器 snap divisor 作为单位。

- `1 snap` 表示当前 snap divisor 下的一个网格单位。
- 实际时长计算为：

```text
TimingPoint.BeatLength / current BeatDivisor * FrameDurationSnaps
```

- 范围：`1-64 snaps`。
- 默认值：`1 snap`。
- 切换编辑器 snap divisor 时，序列预览会立即重新计算。
- 结果栏同时显示 snap 数和对应毫秒时长。

例如：

| 当前 Snap Divisor | `1 snap` 对应时长 |
|---:|---:|
| `1/4` | `0.25 beat` |
| `1/8` | `0.125 beat` |
| `1/16` | `0.0625 beat` |

### Frame Gap

`Frame gap` 表示上一帧 slider tail 到下一帧 slider head 之间的空白时间。

- 单位：beats。
- 范围：`0-4 beats`。
- 步长：`1/16 beat`。
- 默认值：`0.25 beats`。
- 设置为 `0` 时，相邻 slider 首尾相接。

下一帧开始时间的计算方式为：

```text
NextStartTime = CurrentStartTime + FrameDuration + FrameGap
```

总序列时长会计入 `Frames - 1` 个 Frame Gap。

### Scale

所有 Shape 都提供：

- `Start scale`
- `End scale`

参数行为：

- 范围：`10%-400%`。
- 默认值：`100% / 100%`。
- 按 Motion curve 逐帧插值。
- 以 osu! playfield 中心 `(256, 192)` 为缩放中心。
- 只改变 Shape 的整体尺寸，不额外改变序列基准位置。

Scale 会直接修改每一帧的 slider 控制点坐标，不是仅对预览 drawable 进行视觉缩放。

## 等时长与 SV 处理

不同动画帧可能具有不同路径长度。生成器会为每一帧独立计算 `SliderVelocityMultiplier`，确保所有 slider 都满足用户指定的相同 Frame Duration。

计算基于：

- 当前帧的路径长度。
- 当前帧开始位置对应的 timing point。
- Beatmap slider multiplier。
- 用户指定的 Frame Duration。

因此以下变化不会改变每帧在时间轴上的占用时长：

- Shape 参数变化。
- Scale 变化。
- Head position 变化。
- 3D 旋转和透视变化。

osu! slider SV 的有效范围为 `0.1x-10x`。如果某一帧为了满足目标时长需要超出该范围：

- 不生成无效序列。
- 禁用 `Create sequence`。
- 在结果栏显示所需 SV。
- 用户需要调整 Frame Duration 或 Scale。

## 实时预览与事务

- 打开 popover 后开始一个 editor change transaction。
- 任意参数变化只调度一次批量预览重建。
- 重建时先移除旧序列，再生成完整的新序列。
- 只有全部帧都成功生成后，才会将候选序列加入 beatmap。
- 第一帧继承当前 New Combo 状态，后续帧不自动开启 New Combo。
- `Create sequence` 将整段序列作为一次编辑操作提交。
- `Cancel` 或关闭 popover 会删除全部预览 slider 并结束事务。

结果栏显示：

- 帧数。
- 每帧 snap 数。
- 每帧实际毫秒时长。
- Frame Gap。
- 序列总时长。

## 参数持久化

成功点击 `Create sequence` 后保存当前参数。下次打开工具时恢复上次应用状态。

与普通 `Shape slider` 共用的 Start 配置：

- `EditorParametricSliderShape`
- `EditorParametricSliderVertices`
- `EditorParametricSliderIterations`
- `EditorParametricSliderResolution`
- `EditorParametricSliderJuliaReal`
- `EditorParametricSliderJuliaImaginary`
- `EditorParametricSliderPrimaryParameter`
- `EditorParametricSliderSecondaryParameter`
- `EditorParametricSliderHeadPosition`
- `EditorParametricSliderPolyhedronFaces`
- `EditorParametricSliderQuaternionX`
- `EditorParametricSliderQuaternionY`
- `EditorParametricSliderQuaternionZ`
- `EditorParametricSliderQuaternionW`
- `EditorParametricSliderPerspective`

Animation 全局配置：

- `EditorParametricSliderAnimationFrames`
- `EditorParametricSliderAnimationFrameDurationSnaps`
- `EditorParametricSliderAnimationFrameGap`
- `EditorParametricSliderAnimationEasing`
- `EditorParametricSliderAnimationStartScale`
- `EditorParametricSliderAnimationEndScale`

Animation End 配置：

- `EditorParametricSliderAnimationEndVertices`
- `EditorParametricSliderAnimationEndIterations`
- `EditorParametricSliderAnimationEndResolution`
- `EditorParametricSliderAnimationEndJuliaReal`
- `EditorParametricSliderAnimationEndJuliaImaginary`
- `EditorParametricSliderAnimationEndPrimaryParameter`
- `EditorParametricSliderAnimationEndSecondaryParameter`
- `EditorParametricSliderAnimationEndHeadPosition`
- `EditorParametricSliderAnimationEndPolyhedronFaces`
- `EditorParametricSliderAnimationEndQuaternionX`
- `EditorParametricSliderAnimationEndQuaternionY`
- `EditorParametricSliderAnimationEndQuaternionZ`
- `EditorParametricSliderAnimationEndQuaternionW`
- `EditorParametricSliderAnimationEndPerspective`

面板透明度继续共用：

- `EditorParametricSliderPanelOpacity`

配置默认值和枚举定义位于：

- `osu.Game/Configuration/OsuConfigManager.cs`

## 核心代码入口

- `osu.Game.Rulesets.Osu/Edit/GenerateToolboxGroup.cs`
  - `Generate > Shape animation` Sidebar 按钮入口。
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderAnimationPopover.cs`
  - 分类折叠和 Shape 网格。
  - Start / End 参数 UI。
  - Motion curve 和插值。
  - Frame Duration、Frame Gap 和时间轴排布。
  - Scale 变换。
  - 四元数插值。
  - 每帧 SV 计算和有效范围检查。
  - 实时批量预览、提交、回滚和配置保存。
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerator.cs`
  - 复用 v6 Shape slider 的 Shape 枚举、参数元数据和路径生成。
  - 复用闭合路径 Head position 重排逻辑。
- `osu.Game/Configuration/OsuConfigManager.cs`
  - Animation 参数默认值和持久化配置项。

## 验证状态

- 初版 Shape animation 序列生成器完成过 `osu.Game.Rulesets.Osu` 最小 build：`0 warning / 0 error`。
- 初版 build 后继续完成了以下 UI 和参数调整：
  - 改用 Shape slider 同款分类折叠和双列 Shape 网格。
  - 参数改为紧凑的 Start / End 成对布局。
  - 新增 Frame Gap。
  - Frame Duration 改为当前 snap divisor 单位。
  - 新增 Start / End Scale。
- 根据开发约定，上述最终调整未再次运行 build 或客户端测试。
