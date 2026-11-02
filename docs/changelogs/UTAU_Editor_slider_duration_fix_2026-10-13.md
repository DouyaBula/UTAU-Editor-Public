# UTAU Editor — Slider 时长保持修复（2026-10-13）

## Slider Curves

- 统一控制点编辑、控制点删除、曲线类型切换、经典曲线工具和缩放的 Keep duration 处理：使用编辑后的原始路径长度更新 `ExpectedDistance` 与 SV，并立即刷新 slider velocity，避免旧吸附长度覆盖编辑结果。
- 修复经典模式通过 Ctrl+单击或当前时间入口增加控制点时仍无条件执行 `SnapTo`、导致 duration 变短的问题。关闭 Keep duration 时保留原有吸附行为。
- 在只有两个控制点的路径上切换为 Perfect Curve 时，自动插入一个继承型中间点，使两点路径即可创建圆弧。
- 编辑结果长度无效或所需 SV 超出允许范围时回滚几何修改，避免破坏原 slider。

## 验证

- Keep duration 定向回归测试：4/4 通过，覆盖 Bézier 插点、控制点删除、两点 Perfect Curve 和缩放。
- `TestSceneSliderSelectionBlueprint`：22/22 通过。
- `TestScenePathControlPointVisualiser`：14/14 通过。
- 未运行全量测试。
