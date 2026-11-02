# Stable Slider Export Optimizer

> 已退役功能的历史设计，2026-11-02 归档。`For compatibility with slider optimization (.osz)` 及其实现已移除；以下 API、菜单与测试路径是历史记录，不是现行功能。默认 compatibility 导出未因本轮优化而改变。当前文档见[统一入口](../README.md)。

本文档说明当前 `For compatibility with slider optimization (.osz)` 导出路径中的滑条优化器逻辑。该优化器仍是实验性功能，尤其是变速滑条部分；默认 `For compatibility (.osz)` 导出路径不会启用它。

## 目标

osu!stable 的 legacy `.osu` 格式不保留滑条 control point 的浮点坐标。lazer 中高密度采样生成的滑条在导出到 stable 时会被量化到整数坐标，过密的相邻点反而可能形成可见锯齿或重复点。

优化器的目标是：

- 面向 `Slider Toolbox` 生成的高密度采样滑条。
- 在 stable 整数坐标约束下减少 control point 数量。
- 尽量保持滑条可见形状和 `ExpectedDistance` 对应的视觉长度。
- 避免导出后出现重复 inherited control point 位置。
- 保留默认兼容导出逻辑，优化导出只通过单独菜单入口触发。

## UI 入口

- `File > Export > For compatibility with slider optimization (.osz)`

默认入口仍然是：

- `File > Export > For compatibility (.osz)`

默认入口不启用优化器。

## 代码入口

- `osu.Game/Screens/Edit/Editor.cs`
  - 新增优化导出菜单项。
- `osu.Game/Beatmaps/BeatmapManager.cs`
  - `ExportLegacyWithSliderOptimization(...)`
  - 使用带优化器的 legacy exporter。
- `osu.Game/Database/LegacyBeatmapExporter.cs`
  - `optimizeSliderPaths` 为 `true` 时调用 `StableSliderPathOptimizer.TryOptimize(...)`。
- `osu.Game/Database/StableSliderPathOptimizer.cs`
  - 当前优化器主体。
- `osu.Game.Tests/Beatmaps/IO/StableSliderPathOptimizerTest.cs`
  - 优化器相关测试入口。

## 总体流程

优化器只处理 control point 数量足够高的滑条。

```text
SliderPath
  -> control point 数量检查
  -> 尝试识别并处理变速滑条
  -> 如果不是变速滑条，则进入普通高密度路径优化
  -> 成功时替换 SliderPath.ControlPoints
  -> 失败时返回 false，由 legacy exporter 继续使用默认导出逻辑
```

当前阈值：

- `minimum_control_points = 128`

少于该数量的滑条不会进入优化器。

## 坐标量化顺序

优化器的关键原则是：面向 stable 输出的几何判断应在整数坐标空间中进行。

原因是 stable 导出最终会把 control point 坐标写成整数。如果先在浮点坐标中简化，再交给导出器量化，简化结果可能在 stable 中变成另一条路径，尤其是高密度曲线和变速滑条更容易出现锯齿、重复点或长度偏差。

因此普通路径优化会先执行：

```text
CalculatedPath float samples
  -> MathF.Round(x/y)
  -> 移除相邻重复整数点
  -> 在整数点序列上计算累计长度和简化误差
```

## 普通高密度路径优化

普通路径分支用于未识别为变速滑条的高密度 slider。

### 输入

- `path.CalculatedPath`
  - 这是 lazer 计算后的实际路径采样。
- `path.ExpectedDistance`
  - 如果存在，用作目标视觉长度。
  - 如果不存在，使用量化后样本的累计长度。

### 简化算法

当前逻辑是 Ramer-Douglas-Peucker 风格的递归简化，但有两个重要调整：

- 在整数坐标空间中计算误差。
- 长距离段会按累计长度中点拆分，避免单段过长。

使用的主要参数：

- `maximum_segment_length = 16`
- `error_attempts = 1.25, 1, 0.75, 0.5, 0.35, 0.2, 0.1`

优化器会从较宽松的误差开始尝试，生成候选 control point 序列，然后用 `SliderPath` 重新计算候选路径长度。

### 重复点处理

stable 中重复 inherited control point 位置可能被解释成异常的 uninherited point，视觉上会表现为红点或路径异常。

因此候选点会经过 `makePositionsDistinct(...)`：

- 普通重复点会优先沿局部切线方向寻找邻近可用整数坐标。
- 找不到时才使用小范围网格邻居。
- 如果尾点闭合回到起点，则允许最后一个点与第一个点重合。

### 接受条件

候选路径会与目标长度比较：

- 严格接受：
  - 绝对误差不超过 `2 px`，或
  - 相对误差不超过 `1.5%`
- 兜底接受：
  - 比默认量化路径的长度误差更小，或
  - 相对误差不超过 `4%`

如果没有任何候选满足条件，优化器返回 `false`，导出器继续默认逻辑。

### 输出

普通路径优化成功后会把路径写成单段 linear control point 序列：

```text
first control point: PathType.LINEAR
other control points: inherited/null
```

这是因为优化器目标是 stable 可读的几何拟合，而不是保留原始 parametric shape 的语义。

## 变速滑条优化

这里的“变速滑条”指右键菜单 `Slider velocity` 功能生成的高密度路径。它通常通过 control point 密度或局部折返距离来表达不同区域的视觉速度。

当前变速分支的设计目标不是完全复刻速度曲线，而是优先保持：

1. 可见形状。
2. head / tail 位置。
3. 视觉长度。
4. 大致速度趋势。

速度精度低于视觉保真优先级。

### 识别条件

当前只处理单段 linear slider：

- 第一个 control point 允许为 `Linear` 或空。
- 后续 control point 必须为 inherited/null。
- control point 数量必须达到高密度阈值。

识别时会扫描形如：

```text
A -> hidden branch point -> A
```

的局部折返结构。若折返后量化位置回到当前点，则认为这是速度处理留下的隐藏 travel distance。

触发变速分支的条件：

- 隐藏折返总距离至少 `16 px`，或
- 折返分支数量至少 `8`

### 重建思路

变速分支不是直接对源 slider A 的所有坐标做普通重采样，而是尝试抽象出两层信息：

- 可见路径 B：
  - 由源路径中的可见 anchor 位置构成。
  - 使用量化后的整数坐标作为 stable 输出基础。
- travel distance 分布：
  - 每段 anchor 的 `OutgoingDistance` 记录源路径在该段中实际走过的距离。
  - 这包含可见直连距离和隐藏折返距离。

简化后，优化器在可见路径 B 的相邻保留点之间重新构造 stable-compatible 的局部 travel distance。

### 切线方向折返

为避免产生垂直方向扰动导致锯齿，当前变速分支的补偿折返限制在局部切线方向上：

```text
start
  -> start + tangent * forward
  -> start + tangent * returnOffset
  -> end
```

不主动生成法线方向的 zig-zag 抖动。

使用的主要参数：

- `maximum_local_detour_radius = 16`
- `maximum_velocity_control_points = 12000`

如果无法在局部找到不重复的整数点来表达额外 travel distance，则该候选会失败。

### 输出长度

变速优化成功时会设置：

```text
path.ExpectedDistance.Value = targetDistance
```

这样导出的 legacy slider 尽量保留源 slider 的目标视觉长度，而不是让重建后的整数 control point 序列重新定义长度。

## 已知限制

变速滑条优化仍然不是通用解。

主要原因：

- stable 坐标只能是整数，很多 sub-pixel 级的速度处理信息在导出前后不可逆。
- 高密度变速滑条可能同时把“形状”和“速度”编码在 control point 分布中，优化器很难完全区分二者。
- 某些局部折返在 stable 量化后会坍缩成重复点，既不能原样保留，也不一定能找到视觉无损的替代表达。
- 当前算法优先保留形状和长度，因此速度曲线只能保证大体趋势。

如果优化器无法构造满足长度和重复点约束的候选，会返回 `false`，该 slider 会退回默认 legacy 导出逻辑。

## 维护建议

后续继续改进时应保持以下原则：

- 默认 `For compatibility (.osz)` 不应启用优化器。
- stable 输出相关几何判断应尽量在整数坐标空间中完成。
- 不要重新加入对尖角、拐角、曲率奇点的特殊识别；`Slider Toolbox` 生成逻辑本质上是高密度采样，形状识别容易产生误判。
- 变速补偿不应使用随机或法线方向扰动，除非有额外的视觉误差验证。
- 任何候选替换都应和默认导出路径比较长度误差。
- 如果优化器对某类 slider 不可靠，应只让该类 slider 局部回退，而不是影响默认导出入口。
