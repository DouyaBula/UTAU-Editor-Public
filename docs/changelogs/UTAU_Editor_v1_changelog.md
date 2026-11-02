# UTAU_Editor v1 更新日志

发布日期：2026-06-06

本文记录当前 UTAU_Editor 分支相对官方 osu!lazer 编辑器实现的第一版自定义功能。

## 编辑器视觉简化

- 移除了 osu! edit 内部分选择相关的 ring 扩散动画，使物件选择反馈更安静。
- 移除了物件击打后的扩散视觉效果，减少 mapping 时的视觉干扰。
- 精简了部分默认皮肤层面的闪烁/扩散反馈，包括 kiai flash 等会影响编辑观察的效果。

## 物件选择描边

- 新增物件选择描边宽度配置，用于调整 osu! 模式下 circle、slider head、slider tail、slider body 等选中描边的粗细。
- 选择描边显示方式调整为更接近内描边，减少描边向物件外侧扩张。
- 新增独立的选择框描边宽度配置，选择框粗细不再被物件描边宽度强制同步。
- 新增选择描边颜色配置：
  - hit circle / slider head / slider tail 选择描边颜色。
  - slider body 选择描边颜色。
- 新增选择框颜色配置。
- 新增选择框下方控制按钮与控制柄颜色配置。

## 选择框控制项

- 新增选择框下方按钮大小配置。
- 新增选择框缩放控制柄大小配置。
- 新增选择框旋转控制柄大小配置。
- 旋转控制柄的外扩位置会随旋转控制柄大小同步调整，避免大小变化后控件位置不协调。
- 选择控制项图标前景色会根据配置的背景颜色自动选择黑色或白色，以保持可读性。

## 滑条路径编辑

- 新增滑条控制点显示大小配置。
- 新增滑条控制点可点击范围配置，使可点击范围可以与实际显示大小分离，方便在控制点显示较小时仍然容易选中。
- 新增滑条控制线颜色配置。
- 新增滑条控制线粗细配置。
- 滑条控制点颜色按类型拆分为独立配置：
  - Inherit。
  - Linear。
  - Bezier。
  - Catmull。
  - Perfect curve。
  - B-Spline degree 1。
  - B-Spline degree 2。
  - B-Spline degree 3。
  - B-Spline degree 4+。
- Catmull 曲线类型重新加入控制点右键菜单和放置滑条时的类型循环。
- 新增 slider tail 半圆弧颜色配置。
- 新增 slider tail 半圆弧粗细配置。

## 快捷操作与工作流

- 改进 slider 右键菜单中的 `Convert to stream` 功能。
- `Convert to stream` 现在支持按 easing 曲线生成非等距 note：note 时间仍按当前 beat divisor 对齐，note 在 slider 路径上的采样进度会按曲线重新分布。
- 新增 `Convert to stream curve` 配置项，可在 `UTAU_Editor > Workflow` 中选择默认曲线。
- slider 右键菜单新增 `Convert to stream with curve` 子菜单，可临时选择任意曲线直接转换。
- 曲线列表按 `Linear`、`Ease in`、`Ease out`、`Ease in & out`、`Ease out & in`、`Other` 分组，并尽量暴露 osu!framework 当前提供的全部 easing 曲线。
- 新增等时长 slider SV 调整快捷键：
  - `Ctrl+Alt+Up`：增加选中 slider 的 slider velocity，同时按 `newSV / oldSV` 等比放大 slider 路径，使 slider 在时间轴上的持续时长保持不变。
  - `Ctrl+Alt+Down`：减少选中 slider 的 slider velocity，同时按 `newSV / oldSV` 等比缩小 slider 路径，使 slider 在时间轴上的持续时长保持不变。
- 等时长 slider 调整使用加算 SV step，而不是直接把 SV 乘以倍率。例如 SV 为 `1.2`、step 为 `0.1` 时，上调后 SV 为 `1.3`。
- 新增 `Fixed-duration slider SV step` 配置项，默认 `0.1 SV`。
- 等时长 slider 调整允许 slider head / tail 超出 playfield bounds，仅保留 slider 路径长度有效性检查。
- 新增 `Nudge distance` 配置项，用于调整选中物件时 `Ctrl+方向键` 的空间平移距离。
- `Ctrl+方向键` 默认平移距离由 `1px` 改为 `5px`，并可在菜单中配置。

## 颜色预设

- 编辑器自定义颜色菜单新增多种常用预设色。
- 预设色包括黄色、橙色、红色、粉色、紫色、蓝色、青色、绿色、浅灰色、白色等。
- 选择相关颜色、滑条控制点颜色、控制线颜色和 tail 半圆弧颜色均可通过菜单分别配置。

## 菜单结构

- 新增顶层 `UTAU_Editor` 菜单，用于集中管理本分支新增的自定义功能。
- 已将选择描边、选择框、滑条路径视觉、等时长 SV step、nudge distance 等新增功能从 `View` 菜单移出。
- `UTAU_Editor` 菜单当前分为：
  - `Workflow`：放置 nudge distance 与 fixed-duration slider SV step 等操作流相关配置。
  - `Selection visuals`：放置选择描边、选择框、控制按钮/控制柄大小和选择颜色配置。
  - `Slider path visuals`：放置滑条控制点、控制线和 tail arc 相关配置。
- `View` 菜单保留背景变暗、storyboard、hit markers、timeline、auto seek、limited distance snap、contract sidebars 等更接近原始编辑器视图/开关属性的条目。

## 背景变暗

- Background dim 菜单新增的中间档位现在可以正常生效。
- Background dim 配置步进从 25% 调整为 1%，支持更细的背景变暗比例。

## 配置隔离

- 新增 UTAU_Editor 专用配置管理器。
- UTAU_Editor 分支现在使用独立的 `.utau-editor.ini` 保存新增编辑器配置。
- 官方客户端和 UTAU_Editor 仍然可以共用谱面、皮肤、数据库和资源 storage，但官方客户端自动更新或启动后，不会再覆盖 UTAU_Editor 的自定义编辑器配置。

## 更新日志位置

- 更新日志从仓库根目录移动到 `docs/changelogs/`，避免根目录继续堆放项目说明文件。

## 实现原则

- 尽量保留原始代码作为注释，方便后续对照、回退或继续修改。
- 新增功能优先接入 osu!lazer 现有配置系统、菜单系统和快捷键系统。
- 编辑器视觉自定义与 mapping 工作流配置集中放入 `UTAU_Editor` 菜单，便于 mapper 按用途快速查找。
