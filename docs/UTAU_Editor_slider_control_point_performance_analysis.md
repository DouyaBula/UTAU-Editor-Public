# Slider Control Point Performance Analysis

> 历史热点调查。当前已有控制点 LOD、拖拽按帧合并和命中采样缓存；以下性能描述不等于现版本实测结论。现行入口和约定见[使用指南](editor-guide.md)与[维护指南](maintainer-guide.md)。

日期：2026-06-16

## 结论

当前卡顿主要出现在选中态的控制点可视化和输入命中逻辑，而不是移动、缩放等几何计算本身。

当 slider 包含几千个 control points 时，单独选中该 slider 会创建几千个 `PathControlPointPiece` drawable。每个 piece 都是一个可交互的 `CompositeDrawable`，包含多个子 drawable、配置绑定、hover / drag / tooltip / selection 状态和命中检测。除此之外，选中态还会绘制一条包含所有 control points 的控制线，并在若干输入路径中线性遍历所有 control point piece。

因此，几何数据本身可以在移动/缩放时较快处理，但一旦进入“单个 slider 选中并显示所有控制点”的 UI 模式，渲染树、输入树和 per-control-point 状态更新会迅速膨胀，导致 editor 卡顿甚至无响应。

## 触发条件

最容易触发卡顿的条件：

- 单个 slider 拥有大量 control points，例如 `2000-3000+`。
- 该 slider 是当前唯一选中的物体。
- editor 进入 control point 详细编辑模式。
- 鼠标在 playfield 上移动、hover、右键菜单、拖拽或缩放。

多选时问题较轻，是因为当前代码有意只在单选 slider 时显示详细 control point visualiser。

相关代码：

- `osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/SliderSelectionBlueprint.cs`
  - `updateVisualDefinition()`
  - 单选时创建 `PathControlPointVisualiser<Slider>`。
- `osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/Components/PathControlPointVisualiser.cs`
  - 为每个 `PathControlPoint` 创建 `PathControlPointPiece`。

## 关键代码路径

### 1. 单选 slider 时创建完整 control point visualiser

`SliderSelectionBlueprint.updateVisualDefinition()` 中有明确逻辑：

- 如果 slider 被选中。
- 且当前选中物体数量小于 `2`。
- 则创建 `PathControlPointVisualiser<Slider>`。

这意味着单选一个大 control-point slider 会立即进入最重的可视化模式。

代码入口：

- `SliderSelectionBlueprint.cs`
  - `updateVisualDefinition()`
  - `AddInternal(ControlPointVisualiser = new PathControlPointVisualiser<Slider>(HitObject, true) ...)`

### 2. 每个 control point 创建一个独立 drawable

`PathControlPointVisualiser.onControlPointsChanged()` 在收到 control point add 事件时，对每个点执行：

```csharp
Pieces.Add(new PathControlPointPiece<T>(hitObject, point).With(...));
```

因此 `3000` 个 control points 会创建 `3000` 个 `PathControlPointPiece`。

每个 `PathControlPointPiece` 内部又包含：

- `inputCircle`
- `circle`
- `markerRing`

也就是说，几千个 control points 实际上会扩展成上万个 drawable 级别的节点，并且全部参与生命周期、布局、绘制、输入和状态更新。

相关文件：

- `PathControlPointVisualiser.cs`
  - `Pieces = new Container<PathControlPointPiece<T>>`
  - `onControlPointsChanged()`
- `PathControlPointPiece.cs`
  - `InternalChildren = new[] { inputCircle, circle, markerRing }`

### 3. 每个 control point piece 都绑定大量配置和状态

`PathControlPointPiece.LoadComplete()` 中，每个 piece 都绑定并监听多组配置：

- hit object position
- hit object scale
- stack height
- selected state
- control point base colour
- inherited colour
- Catmull colour
- Bezier colour
- Linear colour
- Perfect curve colour
- B-spline degree 1/2/3/4 colour
- control point size
- control point hit area size

每个绑定变化都会触发 `updateMarkerDisplay()`。

在几千个 points 下，这会形成很大的绑定回调和状态更新成本。即使配置不频繁变化，初始化时也会产生大量绑定对象和初始回调。

相关文件：

- `PathControlPointPiece.cs`
  - `load(OsuConfigManager config)`
  - `LoadComplete()`
  - `updateMarkerDisplay()`

### 4. 鼠标输入命中会线性遍历所有 control point piece

`SliderSelectionBlueprint.ReceivePositionalInputAt()` 在 `ControlPointVisualiser != null` 时会执行：

```csharp
foreach (var p in ControlPointVisualiser.Pieces)
{
    if (p.ReceivePositionalInputAt(screenSpacePos))
        return true;
}
```

这意味着鼠标移动、hover、click、context menu 等输入路径会对几千个 piece 做线性扫描。

`PathControlPointPiece.ReceivePositionalInputAt()` 又会调用不可见的 `inputCircle.ReceivePositionalInputAt()`，用于扩大控制点命中区域。

这解释了为什么进入选中态后 editor 可能变得无响应：不是一次计算慢，而是每次输入命中都可能扫全量 control point drawable。

相关文件：

- `SliderSelectionBlueprint.cs`
  - `ReceivePositionalInputAt()`
- `PathControlPointPiece.cs`
  - `ReceivePositionalInputAt()`

### 5. SelectionQuad 会合并所有 control point piece 的屏幕包围盒

`SliderSelectionBlueprint.SelectionQuad` 在存在 `ControlPointVisualiser` 时会遍历每个 piece：

```csharp
foreach (var piece in ControlPointVisualiser.Pieces)
    result = RectangleF.Union(result, piece.ScreenSpaceDrawQuad.AABBFloat);
```

这个路径本身是 O(n)。如果 selection quad 在布局、选择框、transform handle 或其他 UI 刷新中频繁访问，会进一步放大高点数 slider 的开销。

相关文件：

- `SliderSelectionBlueprint.cs`
  - `SelectionQuad`

### 6. 控制线也会重建全部顶点

`PathControlPointConnection<T>` 继承 `SmoothPath`，用于绘制控制点之间的连接线。

当 hit object position、path version 或 stack height 变化时，会执行：

```csharp
ClearVertices();

foreach (var controlPoint in hitObject.Path.ControlPoints)
    AddVertex(controlPoint.Position);
```

这条线本身比几千个 interactive drawable 更轻，但仍然是 O(n) 的动态路径更新。拖动、缩放、路径版本变化时会额外消耗 CPU/GPU 侧资源。

相关文件：

- `PathControlPointConnection.cs`
  - `updateConnectingPath()`

## 为什么移动/缩放看起来更快

移动/缩放的核心逻辑主要是数组级别的 control point 坐标批处理。

例如 `OsuSelectionScaleHandler.scaleSlider()` 直接遍历 slider path control points：

```csharp
for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
{
    slider.Path.ControlPoints[i].Position = ...;
    slider.Path.ControlPoints[i].Type = ...;
}
```

这是 O(n) 的数据计算，几千个点并不一定不可接受。

但选中显示时不是只处理一个数组，而是：

- 创建几千个 UI drawable。
- 每个 drawable 有多个子节点。
- 每个 drawable 有多个 bindable 监听。
- 每个 drawable 可以参与 hover、click、drag、tooltip 和 selection。
- 每次输入命中可能线性扫描全部 drawable。
- selection bounds 也可能线性合并全部 drawable。

因此，实际瓶颈更像是 UI/渲染/输入树规模问题，而不是单纯几何计算问题。

## 次要风险点

### ScreenSpaceSnapPoints

`SliderSelectionBlueprint.ScreenSpaceAdditionalNodes` 会枚举 slider control points，并转换为 screen-space snap nodes。

移动开始时 `BlueprintContainer.prepareSelectionMovement()` 会读取 selected blueprint 的 `ScreenSpaceSnapPoints`。

这条路径通常不是持续每帧运行，但对几千个 LINEAR control points 的 slider，开始移动时仍会有一次较重的 O(n) 转换。

相关文件：

- `SliderSelectionBlueprint.cs`
  - `ScreenSpaceAdditionalNodes`
  - `getScreenSpaceControlPointNodes()`
- `BlueprintContainer.cs`
  - `prepareSelectionMovement()`

### Slider body 本体不是首要嫌疑

`SliderBodyPiece.UpdateFrom()` 只有在 path version 变化时才调用 `body.SetVertices(hitObject.Path.CalculatedPath)`。

这说明 slider body 本体路径更新也有成本，但它不像 control point visualiser 那样为每个 control point 创建可交互 drawable。因此，本问题的首要优化目标不应是 slider body，而应是选中态 control point 可视化。

相关文件：

- `SliderBodyPiece.cs`
  - `UpdateFrom()`

## 优化方向建议

### 方案 A：高 control point 数量时自动降级详细控制点显示

这是最直接、风险最低的优化方向。

建议行为：

- 当 `ControlPoints.Count <= threshold` 时，保持当前完整编辑体验。
- 当 `ControlPoints.Count > threshold` 时，默认进入 lightweight visualisation：
  - 不创建每个 `PathControlPointPiece`。
  - 只显示 slider body、head、tail 和必要的简化控制线。
  - 结果栏或 tooltip 提示 control points 过多，详细点编辑已折叠。
  - 提供显式按钮或快捷入口进入“详细控制点编辑模式”。

建议初始 threshold：

- 保守值：`512`
- 宽松值：`1024`

优点：

- 修改范围较小。
- 能立即避免几千个 drawable 的输入/渲染负担。
- 不影响普通 slider 编辑。

缺点：

- 高点数 slider 默认不能逐点编辑。
- 需要补一个显式入口给需要逐点编辑的用户。

### 方案 B：批量绘制 control point markers，避免一点一个 CompositeDrawable

将几千个控制点 marker 改成一个或少数几个批量 drawable。

思路：

- 用一个自定义 drawable 批量绘制所有 marker。
- 按控制点类型分组或用顶点颜色绘制。
- 只为当前 hovered / selected 的少量点创建真实交互 piece。

优点：

- 可以保留高点数下的可视化密度。
- 渲染树节点数量从 O(n) 降到 O(1) 或少量常数。

缺点：

- 实现复杂度中高。
- 需要重新实现 hover/selection 的最近点拾取。
- 需要仔细处理颜色、大小、选中环、tooltip 和 drag。

### 方案 C：用空间索引或最近点查询替代全量命中扫描

当前命中测试是逐 piece 扫描。可以改为：

- control point 坐标变更时构建空间索引。
- 鼠标命中时只查询附近 control points。
- 或者使用屏幕空间最近点距离判断，而不是遍历每个 drawable 的 `ReceivePositionalInputAt()`。

优点：

- 能明显改善鼠标移动、hover、点击和右键菜单响应。
- 可以与方案 B 配合。

缺点：

- 如果仍保留几千个 drawable，渲染树开销仍然存在。
- 单独实施只能缓解输入卡顿，不能完全解决 drawable 创建和渲染成本。

### 方案 D：缓存 SelectionQuad 和 ScreenSpaceSnapPoints

当前高点数 slider 的 selection bounds 和 snap nodes 可能重复计算。

可以按以下条件缓存：

- path version
- hit object position
- stack height
- scale
- selected visualiser 是否存在

优点：

- 风险较低。
- 对 transform handle、drag start 和 selection UI 有帮助。

缺点：

- 不是主瓶颈。
- 不能解决 per-control-point drawable 数量过多的问题。

### 方案 E：控制线降采样或隐藏

`PathControlPointConnection` 可在高点数时：

- 降采样顶点。
- 仅显示首尾或每 N 个控制点。
- 或在 lightweight mode 中隐藏。

优点：

- 实现简单。
- 对拖动/缩放时的动态路径更新有帮助。

缺点：

- 控制线不是最大瓶颈。
- 单独做收益有限。

## 推荐优化顺序

1. 先做高点数 lightweight mode。
   - 这是最可能快速解决无响应的方案。
   - 避免创建几千个 `PathControlPointPiece` 是核心。

2. 增加显式 `Show all control points` 或 `Detailed control point edit` 入口。
   - 防止用户完全失去逐点编辑能力。
   - 可以在弹窗中提示高点数会影响性能。

3. 对 input hit testing 做最近点查询。
   - 如果后续仍希望高点数下可编辑控制点，需要避免每次鼠标输入线性扫全部 piece。

4. 再考虑批量 marker renderer。
   - 这是长期更完整的方案，但实现复杂度高于 lightweight mode。

5. 最后再考虑路径生成阶段的 control point 简化。
   - 它能降低数据规模，但不是本次卡顿的根因。
   - 用户可能有意保留高密度路径，不应强制破坏原始 slider。

## 建议的第一版优化规格

可以先实现一个非常保守的第一版：

- 新增阈值常量，例如：

```csharp
private const int detailed_control_point_limit = 1024;
```

- 在 `SliderSelectionBlueprint.updateVisualDefinition()` 中判断：

```csharp
if (HitObject.Path.ControlPoints.Count <= detailed_control_point_limit)
    create detailed PathControlPointVisualiser;
else
    use lightweight mode;
```

- lightweight mode 行为：
  - 保留 `SliderBodyPiece`、head overlay、tail overlay。
  - 不创建 `PathControlPointVisualiser`。
  - 可选：显示一条降采样控制线。
  - 右键菜单仍保留 `Slider velocity...` 等 slider 级工具。
  - 不提供逐点删除、split、curve type 改动，除非用户显式打开详细模式。

如果需要保持可编辑性，可以补充：

- Sidebar / context menu 项：`Show all control points`。
- 或配置项：`EditorDetailedSliderControlPointLimit`。
- 或临时按住某个修饰键时启用详细模式。

## 需要验证的点

实施优化后建议重点验证：

- 选中 `3000+` control points slider 时 editor 是否仍可流畅响应。
- 鼠标移动时是否不再无响应。
- 普通低点数 slider 的逐点编辑体验是否不变。
- 多选 slider 行为是否不变。
- 右键菜单是否仍可打开。
- Slider velocity、Shape animation 生成出的高点数 slider 是否能安全选择和移动。
- 详细模式入口是否能恢复完整 control point 编辑能力。

## 最终判断

本问题的主因是渲染和输入 UI 对象规模，而不是路径几何计算。

最关键的性能成本来自：

1. `PathControlPointVisualiser` 为每个 control point 创建 `PathControlPointPiece`。
2. `PathControlPointPiece` 是包含多个子 drawable 和大量 bindable 的交互对象。
3. `SliderSelectionBlueprint.ReceivePositionalInputAt()` 对所有 point piece 做线性命中检测。
4. `SelectionQuad` 和控制线更新在高点数下也存在 O(n) 成本。

优先优化方向应是高点数 slider 的选中态可视化降级，而不是先改移动/缩放算法。
