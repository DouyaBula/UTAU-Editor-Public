# UTAU_Editor v6 更新日志

发布日期：2026-06-15

本文记录 `UTAU_Editor v5` 之后新增的参数化 Shape slider 生成功能，重点说明最终功能行为、UI 入口、参数、配置项和代码入口。

## 参数化 Shape Slider 生成器

- 新增特殊形状 slider 生成工具。
- 支持多边形、常见数学曲线和分形图案。
- 生成结果默认归一化到约 `260 x 260` 的区域，并放置在 playfield 中心附近，之后可继续使用编辑器缩放、旋转和移动。
- 参数变化会实时重建预览。
- 点击 `Create` 提交生成结果。
- 点击 `Cancel` 或关闭 popover 会移除预览并回滚本次 editor change transaction。
- 单次生成最多保留 `8000` 个控制点，避免产生无上限增长的 beatmap 数据。

### UI 入口

- 左侧 Sidebar：`Generate > Shape slider`。
- 当前没有默认快捷键。
- Popover 使用左右双栏布局：
  - 左侧选择 Shape。
  - 右侧调整当前 Shape 所需参数。
  - 底部显示控制点数量和计算后的 slider 路径长度。

### Shape 分类与选择

左侧 Shape 选择区分为四个组：

- `POLYGON`
- `CURVES`
- `FRACTALS`
- `3D`

四个组使用可折叠分类按钮：

- 默认展开上次应用的 Shape 所属分类。
- 点击其他分类会收起当前分类并展开目标分类。
- 再次点击当前分类可将其折叠。
- 分类内部使用双列按钮阵列，并可独立滚动。
- 当前 Shape 按钮会高亮显示。
- 较长名称提供 tooltip。

## 支持的 Shape

当前共提供 `52` 种 Shape：`1` 种多边形、`29` 种数学曲线、`21` 种分形和 `1` 种参数化 3D 投影生成器。

### Polygon

- Polygon

参数：

- `Polygon vertices`：`3-32`

### Curves

- Circle
- Ellipse
- Sine wave
- Cosine wave
- Parabola
- Cubic curve
- Semicubical parabola
- Cardioid
- Heart curve
- Lissajous curve
- Rose curve
- Bernoulli lemniscate
- Astroid
- Cycloid
- Epicycloid
- Hypocycloid
- Epitrochoid
- Hypotrochoid
- Archimedean spiral
- Logarithmic spiral
- Hyperbolic spiral
- Fermat spiral
- Euler spiral
- Butterfly curve
- Cochleoid
- Circle involute
- Catenary
- Witch of Agnesi
- Superformula

曲线根据类型动态使用以下参数：

- `Sampling resolution`
- `Cycles`
- `X frequency`
- `Y frequency`
- `Petals`
- `Cusps`
- `Lobes`
- `Turns`
- `Pen offset`
- `Symmetry`
- `Roundness`

离散参数会自动使用整数步长；连续参数使用 `0.1` 步长。

### Fractals

- Koch curve
- Koch snowflake
- Dragon curve
- Sierpinski arrowhead
- Hilbert curve
- Gosper curve
- Levy C curve
- Menger carpet
- Barnsley fern
- Mandelbrot set
- Julia set
- Burning Ship
- Newton fractal
- Peano curve
- Moore curve
- Minkowski sausage
- Minkowski island
- Terdragon curve
- Cesaro curve
- Fractal tree
- Binary tree

分形根据类型动态使用以下参数：

- `Iterations / depth`：递归或 L-system 展开深度。
- `Sampling resolution`：集合轮廓、吸引子或数值边界的采样精度。
- `Julia constant real`
- `Julia constant imaginary`

`Menger carpet` 是适合二维 slider 表示的 Menger sponge 平面版本。

### 3D Projections

- Regular polyhedron

支持五种正多面体：

| Faces | Polyhedron |
|---:|---|
| `4` | Tetrahedron |
| `6` | Cube |
| `8` | Octahedron |
| `12` | Dodecahedron |
| `20` | Icosahedron |

参数：

- `Faces`：自动吸附到 `4 / 6 / 8 / 12 / 20`。
- `Quaternion X / Y / Z / W`：控制物体旋转；生成前自动归一化，全零值按 identity quaternion 处理。
- `Perspective`：`0-100%`；`0%` 为正交投影，增大后增强远近透视效果。
- `Head position`：沿闭合的投影路径循环移动 slider head。

实现会自动推导正多面体的棱，并生成覆盖全部棱且回到起点的连续路径。部分棱会被反向重走，以保证单个 slider 能连续显示完整 wireframe。

## 路径与 Stable 兼容策略

- 数学曲线、集合边界和吸引子主要使用离散采样生成。
- L-system 和递归曲线直接输出折线路径。
- slider head 对应第一个控制点，并显式使用 `Linear`。
- 后续控制点全部使用 inherited path type。
- 不生成 B-spline 控制点，避免 stable 不支持 B-spline 时在导出过程中增加大量替代点。
- 自动 Bezier 拟合未作为通用方案使用，因为它会引入拟合误差、分段复杂度和不同 Shape 之间不一致的结果。
- `Sampling resolution` 范围为 `8-180`。
- 对普通数学曲线，最低分辨率约生成 `48` 个采样点。

## Slider Head Position

闭合路径提供 `Head position` 参数：

- 范围：`0-100%`
- 按实际累计路径长度移动，而不是按控制点索引移动。
- `0%` 和 `100%` 表示同一位置。
- 移动 head 时重新排列闭合路径控制点，不改变图形轮廓。

支持该参数的主要 Shape：

- Polygon
- Circle / Ellipse
- Cardioid / Heart
- Lissajous / Rose / Lemniscate / Astroid
- Epicycloid / Hypocycloid / Epitrochoid / Hypotrochoid
- Butterfly / Superformula
- Koch snowflake
- Minkowski island
- Moore curve
- Mandelbrot / Julia / Burning Ship

开放曲线不显示该参数，避免为移动 head 强行增加闭合连接段。

## 面板透明度

- 右侧参数区顶部提供 `Panel opacity`。
- 范围：`15-100%`
- 步长：`5%`
- 调整后立即改变整个 popover body 的透明度。
- 参数调整不再自动降低面板透明度。
- 透明度设置独立持久化，关闭或取消生成也不会恢复默认值。

## 参数持久化

成功点击 `Create` 后保存以下生成参数：

- Shape
- Polygon vertices
- Iterations / depth
- Sampling resolution
- Julia constant real
- Julia constant imaginary
- Primary curve parameter
- Secondary curve parameter
- Head position
- Polyhedron faces
- Quaternion X / Y / Z / W
- Perspective

下次打开 `Shape slider` 时恢复上次成功应用的参数。

点击 `Cancel` 或直接关闭 popover 不会保存生成参数。`Panel opacity` 属于 UI 设置，会在调整时立即保存。

### 配置入口

配置定义位于：

- `osu.Game/Configuration/OsuConfigManager.cs`

配置项：

- `EditorParametricSliderShape`
- `EditorParametricSliderVertices`
- `EditorParametricSliderIterations`
- `EditorParametricSliderResolution`
- `EditorParametricSliderJuliaReal`
- `EditorParametricSliderJuliaImaginary`
- `EditorParametricSliderPrimaryParameter`
- `EditorParametricSliderSecondaryParameter`
- `EditorParametricSliderHeadPosition`
- `EditorParametricSliderPanelOpacity`
- `EditorParametricSliderPolyhedronFaces`
- `EditorParametricSliderQuaternionX`
- `EditorParametricSliderQuaternionY`
- `EditorParametricSliderQuaternionZ`
- `EditorParametricSliderQuaternionW`
- `EditorParametricSliderPerspective`

## 核心代码入口

- `osu.Game.Rulesets.Osu/Edit/GenerateToolboxGroup.cs`
  - `Generate > Shape slider` 按钮入口。
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerationPopover.cs`
  - 双栏 UI、分类折叠、参数显示、实时预览、透明度、配置加载与保存、提交和回滚。
- `osu.Game.Rulesets.Osu/Edit/ParametricSliderGenerator.cs`
  - Shape 枚举和分类。
  - 数学曲线采样。
  - L-system、递归分形、逃逸时间集合轮廓和 Newton basin 边界生成。
  - 正多面体顶点、棱推导、四元数旋转、透视投影和闭合 edge-covering path。
  - 路径归一化、控制点数量限制和闭合路径 head 重排。
- `osu.Game/Configuration/OsuConfigManager.cs`
  - Shape slider 参数和面板透明度的默认值与持久化配置项。

## 验证状态

- 初版参数化 slider 生成器曾完成 `osu.Game.Rulesets.Osu` 最小 build：`0 warning / 0 error`。
- 已修复 popover 加载阶段禁用 bindable 导致的运行时异常。
- 已修复配置整数参数与 `BindableFloat` 类型不匹配导致的运行时异常。
- 已修复通过普通 `Bindable<double>` 访问 `Precision` 导致的编译错误。
- 后续新增的大量曲线、分形、折叠阵列和手动透明度已执行静态检查。
- 按当前开发约定，最终扩展版本尚未运行 build 或客户端测试。
