# Anchor / Pen 曲线模式（2026-09-07，功能冻结版）

本 feature 已完成开发并冻结实现。传统控制点模式仍保留，Anchor / Pen 模式作为可切换的纯 Bézier 编辑方式提供；最终实机验收由用户自行进行。

## 入口及使用

侧栏 UTAU Toolbox 将模式开关、钢笔选项、节点/轮廓工具合并到一个 **Slider curves** 面板，默认关闭，不替换传统编辑器。

- 开启后，用原来的 Slider 工具点击添加锚点，按住拖动拉出方向手柄；右键或 Enter 完成，Backspace 撤回最后一个锚点，Escape 取消。点击首点闭合（至少三个锚点）。切换模式取消尚未完成的放置，不改变已提交对象。
- 创建过程中默认单击只添加无句柄的直线锚点；只有按住并拖动新点时才生成方向手柄。显式打开 Curvature pen 后仍可选择点击自动平滑；C 在 pen 创建时切换该选项（不触发播放/暂停），L 保留给编辑器；Shift 将手柄/新线段限制到 15° 方向，Alt 独立调整手柄；Ctrl-drag 可移动最后一个锚点（包括初始点）。
- 单选 slider 后，直接拖锚点、手柄或曲线段。Ctrl-click 多选节点，Shift 拖节点限制水平/垂直方向；双击曲线或 Ctrl 点击曲线精确插点；双击节点切换尖角/自动平滑。
- 鼠标悬停曲线时，Ctrl 点击多选；Alt+方向键微移 1px，Alt+Shift 为 10px；不截获全局 Ctrl+A 和普通方向键。Escape 清除节点选择。Delete 保留开放曲线至少两个点、闭合曲线至少三个点，不因节点删不掉而误删整个 slider。
- 锚点右键：尖角、平滑、对称、自动平滑；收起手柄；拉直选中节点间的段；圆角/倒角；中点插入；删除；X/Y 对齐与均匀分布；闭合/开放；反向；水平/垂直翻转；实时镜像。
- 实时镜像是**已有节点配对**约束：按当前轴寻找最近的未配对镜像节点，拖动时未选中的伙伴跟随；位于轴上的节点保持自身配对。黄色轴线显示镜像轴；不是任意曲线自动生成另一半。曲线段拖拽不受此约束。

## 轮廓工具

Curve tools 支持 Union、Intersection、Difference、Xor、Offset、StrokeOutline 的小窗预览，再 Apply/Cancel。

- 布尔和偏移要求实际可见路径闭合。Difference 从最早的 slider 中依次减去其余 slider。
- 描边转轮廓允许开放路径，正值参数是半宽，采用圆角/圆端帽；闭合描边使用闭合接合。
- 输出为普通线性 slider；多个外轮廓/孔洞分别生成独立 slider，不会偷偷用连接线把不同轮廓串起来。
- 默认保留源对象；可选择 Apply 时替换。预览后源几何/位置/时间改变会拒绝提交，要求重新预览。
- 输出沿时间轴顺序排列，从最早输入开始，按 beat divisor 向后排列；继承首对象的 SV 和基础 samples，不保留原始 repeat/node hitsound 结构。可整体撤销。
- 输入限制 16 个 slider，每条最多 20,000 采样点；输出最多 128 个轮廓/20,000 点。新锚点交互最多 256 个锚点，传统高密度控制点模式保留。

## 兼容性和数据策略

`.osu` 不新增字段，不定义专有 beatmap 格式。锚点/手柄转换为原生分段 Bézier 控制点；节点约束保存在弱引用的内存会话缓存中，以原生控制点快照校验有效性。重新打开地图后可恢复几何，约束类型从几何推断，**不会持久化 AutoSmooth 意图和实时镜像设置**。

切换模式只读，不隐式转换。当前 Anchor mode 只接管纯 Bézier slider（包括直线手柄收起后的 Bézier 段）；圆弧、Catmull、BSpline 和其它类型保留传统控制点编辑，不会被隐藏或偷偷转换。

原生路径可能因 ExpectedDistance 被截短或延长。新模式显示底层完整锚点，实际 slider 仍按原长度显示；首次几何编辑会采用编辑后完整曲线。面板另提供 Use full native path length，明确提示可能改变时长。创建提交时会再次执行官方距离吸附；创建过程中的 C、Ctrl-drag 也不会改变这一算法。

默认 Keep duration 通过调整 slider SV 保持时长，超过原生 0.1x–10x 范围则拒绝该次编辑。关闭后保持 SV、允许时长改变。钢笔新建与传统控制点放置一致，预览和完成时都会把完整几何长度送入现有 distance-snap 算法；起点和每次点击都会重新经过现有位置/距离吸附系统，避免使用滞后的 hover 坐标。

性能修复：拖动 pen 手柄期间不再每个鼠标事件重建 nested hitobjects、写入 beatmap 或创建历史快照；输入事件按帧合并，结束拖动时一次性重建和提交。预览曲线 drawable 复用，预览标记复用已提交节点，橡皮筋移动只更新单个游标标记；原生控制点拓扑稳定时复用控制点对象，避免反复分配和清空路径。

输入路由修复：经典控制点可视化器在 Anchor / pen 模式下不仅透明，而且会关闭整个 positional-input 子树和快捷键处理，避免点击手柄时被经典控制点抢走事件、追加控制点或切换编辑语义。选中/悬停滑条时，C 和 Alt 方向键优先交给滑条编辑；Alt 本身仍交给 composer，以便整体移动时启用 distance snap；bookmark/音量等同键位动作也会被拦截。由于全局容器可能先把 Alt+方向键转换为 bookmark/音量 action，Anchor overlay 现在也会在 action 层执行一次对应的锚点微移，避免“事件被消费但锚点不动”。

后续效率补强：单 slider 缩放也遵循 Keep duration，按官方距离 snap 后自动补偿 SV，超出 SV 合法范围时原子回滚。统一面板显示完整快捷键表，Alt+P 在经典控制点与锚点编辑间快速切换；节点类型、手柄、几何、对齐/分布、闭合/反向、镜像和 Live symmetry 均有可见快捷键。Shift+右键命中方向手柄时直接收回该手柄，不删除锚点或整条 slider。

## 最终交互修正（2026-09-07）

- Pen 创建默认关闭 `Curvature pen` 时，单击只创建无控制柄的直线锚点；只有按住并拖动新点才会生成方向手柄。
- `Curvature pen` 仍作为显式可选功能保留；打开后点击自动平滑的旧行为不变。
- Round / Bevel 生成的拐角只保留朝向拐角内部的控制柄，外侧控制柄清零；Bevel 仍生成纯直线倒角。
- Slider 缩放遵循 `Keep duration`，并在 SV 边界超限时原子回滚。
- 传统模式 / Pen 模式、节点操作、对齐分布、镜像、实时对称、手柄操作和删除手柄均提供稳定入口与快捷键。

## 冻结验证

- 增量 Anchor / Pen 回归：26/26 通过（AnchorPath 24 项、默认直线点击创建、拖动生成手柄）。
- Desktop 构建：0 警告、0 错误。
- 未运行全量测试；保留用户实机验收检查交互、撤销/重做、保存重开与导出回读。

本轮回归修复：锚点拖动不再通过 `ControlPoints.Clear()` 再 `AddRange()` 更新路径。原实现会让选区蓝图看到一个瞬时空路径，立刻销毁 Anchor overlay 并重建传统控制点，从而表现为“点击手柄后新增大量控制点”。同拓扑编辑现在原地更新控制点对象；插入/删除使用一次性 `ReplaceRange()`，整个编辑事件始终看到最终拓扑。预览写回也使用同一套原子替换逻辑。

stable 只接受整数控制点，曲线导出本来就有量化误差。导出仍使用原来的官方 `for compatibility` 路径，不再启用旧的 slider optimization 分支，也不在导出时额外改写控制点。

## 参考与实现边界

参考成熟工具的交互，不照搬填充矢量图的数据模型：

- [Adobe Illustrator 钢笔](https://helpx.adobe.com/uk/illustrator/using/tool-techniques/pen-tool.html)：点击/拖动、锚点与方向手柄。
- [Inkscape 节点类型](https://inkscape-manuals.readthedocs.io/en/latest/node-types.html)：尖角、平滑、对称、自动平滑。
- [Inkscape 节点操作](https://inkscape-manuals.readthedocs.io/en/latest/node-operations.html)：节点选择、插入、拉直等高频操作。
- [Inkscape 布尔操作](https://inkscape-manuals.readthedocs.io/en/latest/boolean-operations.html)：闭合轮廓运算；osu slider 只承载路径，不承载填充或孔洞语义。
- [Clipper2 布尔](https://angusj.com/clipper2/Docs/Units/Clipper/Functions/BooleanOp.htm)与[偏移](https://www.angusj.com/clipper2/Docs/Units/Clipper/Functions/InflatePaths.htm)：引入 Clipper2 2.0.0，使用成熟多边形算法。操作对象是曲线采样轮廓，不是解析 Bézier 布尔。

本轮没有实现：套索/框选节点、跨 slider 焊接、持久化约束、解析曲线布尔、带误差承诺的曲线拟合。这些不冒充已交付功能。

## 历史验证记录与用户验收

用户随后授权构建与自动测试，并明确实机验收由用户自行进行；不再使用 computer-use。

实际结果：

- `dotnet build osu.Desktop/osu.Desktop.csproj -v:q` 成功，0 警告、0 错误。
- `AnchorPathTest`：24/24 通过。包含原生往返、多段 Bézier 原生结构保持、原子拓扑替换/控制点对象复用、精确插点、节点约束、轮廓布尔、时长保持/SV 保持、SV 越界原子拒绝、移动头部坐标归一化、撤销/重做及重复只读转换不产生编辑记录。
- 必要编辑器场景：`TestSceneSliderSelectionBlueprint` 12/12 通过；placement 相关场景 35/36 通过，唯一失败是既有的 `TestManualPathTypeControlViaKeyboard`（官方控制点类型快捷键基线回归），不涉及 Anchor / pen 路径。
- `LegacyBeatmapExporterTest / LegacyBeatmapEncoderTest / LegacyBeatmapDecoderTest / BeatmapEditorChangeHandlerTest`：270/270 通过。
- 规则集全量 1271 项：1231 通过、30 跳过、10 项既有基线失败（Slider stream conversion 5 项、PathControlPoint/placement 类型快捷键 2 项、Replay recording 2 项、skin fallback 1 项）；均未涉及 AnchorPath/pen 代码，需另行处理基线环境/官方测试回归。
- 首次编译暴露的 Clipper2 BooleanOp 参数顺序、IList samples 不支持 AddRange 两处问题已修复。
- tracked/untracked 文件的 whitespace 检查通过。

旧的 slider optimization 导出入口、`StableSliderPathOptimizer` 实现和对应测试已移除；BeatmapManager 现在只保留默认兼容导出器。完整 TRX 记录位于 `artifacts/anchor-validation`（Git 忽略的本地验收产物）。

建议用户实机检查：模式反复切换、拖头/尾/手柄、缩放/堆叠、吸附、撤销/重做、切换选择时事务收尾、布尔空结果/孔洞/源变化、保存重开和 stable 导出回读。特别复测既有 freehand、SV 工具、高密度 LOD 和默认兼容导出。自动测试通过不代替这些交互验收。
