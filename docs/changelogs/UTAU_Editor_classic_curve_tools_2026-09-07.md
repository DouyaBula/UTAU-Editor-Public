# Classic 控制点曲线工具（2026-09-07）

本轮在传统 Slider 控制点模式中加入了与常见矢量绘图软件相似的曲线修饰工具。所有操作直接写回 osu 原生 `SliderPath.ControlPoints`，不会引入新的 `.osu` 数据格式。

本轮收尾时移除了实验性的 **Add bump** 操作（包括实现、菜单入口、`Alt+U` 快捷键和测试），避免保留不符合预期的曲线变形行为。

## 新增操作

- **Round corners**：对内部 uninherited anchor 执行圆角；纯 Bézier 锚点沿用高质量锚点算法，Bézier、Perfect Curve、Linear、B-spline 以及它们的混合段均可在局部进行操作。
- **Bevel corners**：对内部 uninherited anchor 执行倒角。每个新边界只保留一个 typed 点，不再生成同位置的多余 inherited 点。
- **Straighten segments**：将选中 anchor 的出段中间控制点拉直到首尾连线。
- **Smooth / Symmetric handles**：只处理选中的 uninherited anchor；仅移动真正的 inherited handles。缺少 handle 时自动插入，不会把另一个 anchor 当 handle；Perfect Curve 的定义点只参与切线计算，不被误改。
- **Retract handles**：删除选中 anchor 两侧的可编辑 inherited handles；Perfect Curve 的必要定义点和 Catmull knots 会保留。
- **Align / Distribute / Mirror X / Y**：仅对选中的 uninherited anchors 进行排列变换。
- **Simplify linear segment**：删除选中 Linear 出段中近似共线的 inherited 点（`Alt+Q`）。
- **Classic live symmetry**：选中单个 Slider A 后显示临时的对称 Slider B 预览；编辑 A 时 B 实时跟随，可选择垂直或水平轴。预览不会写入 beatmap，也不会产生 undo 记录。

本次修复还重建了拓扑变更后的控制点视图。`ReplaceRange`/`Reset` 会同步替换可视控制点，避免第一次倒角后继续操作仍指向旧对象而表现为“点击无效”。

## 入口和快捷键

曲线面板重新分为 `Curve workflow`、`Shortcuts & gestures` 和 `Contour operations` 三个可折叠分组。编辑模式、Keep duration、Curvature pen、live symmetry 及对称轴集中在侧栏；顶栏 `UTAU_Editor -> Workflow` 只保留圆角/倒角 trim 参数，避免重复配置入口。

选中控制点后右键打开 **Classic curve tools**：

- `Alt+R` / `Alt+B`：圆角 / 倒角
- `Alt+L`：拉直
- `Alt+S` / `Alt+Y`：平滑 / 对称控制柄
- `Alt+H`：收回控制柄
- `Alt+X` / `Alt+V`：X / Y 对齐
- `Alt+Shift+X` / `Alt+Shift+V`：X / Y 均匀分布
- `Alt+Q`：简化 Linear 出段
- 顶栏 `UTAU_Editor -> Workflow`：切换 live symmetry、选择对称轴及其它曲线工作流参数

混合路径不会被静默重写为单一曲线类型。需要局部改变类型时，只在操作范围插入明确的原生段边界；其余 Catmull、B-Spline、Perfect Curve、Linear 段及其继承点保持可读、可导出。继承点被选中时 classic 工具会明确不显示，避免点击后无变化。

## 时长与兼容性

- 遵循现有 `Keep duration` 设置；classic 拖动在启用时持续采用实际几何长度，SV 超出原生范围时操作原子回滚。
- 保持传统控制点和路径类型的原生存储方式。
- 移除已被统一 Keep duration 流程取代的 `Use full native path length` 旧操作入口及实现。
- 对称预览只在路径版本、位置、堆叠或缩放变化时重绘，避免编辑过程中的额外重复采样。

## 验证

- `AnchorPathTest`（含 classic 操作、混合路径、handle 生命周期及 Linear 简化工具）：30/30 通过。
- classic live symmetry 选中预览、默认直线点创建、拖动生成控制柄：相关场景通过。
- Desktop 项目构建：0 警告、0 错误。
