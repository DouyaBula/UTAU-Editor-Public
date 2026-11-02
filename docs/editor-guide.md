# UTAU Editor 使用指南

更新：2026-11-02。本文描述已获用户验收的统一交互改造。新旧差异见[本轮更新记录](changelogs/UTAU_Editor_unified_workflow_audit_2026-11-02.md)。

## 功能入口

侧栏组可以排序；这里的“左／右”指组所在侧，实际上下顺序依个人设置而定。

| 工作内容 | 入口 | 作用 |
| --- | --- | --- |
| 画布导航、AR、slider 显示细节 | 左侧 `UTAU Editor` | Canvas zoom、Approach Rate、Slider detail、越界放置 |
| 曲线编辑模式与辅助操作 | 左侧 `UTAU Editor → Slider curves`；锚点／控制点右键菜单 | Classic／Pen、Keep duration、节点处理、轮廓运算 |
| 难度显示 | 左侧 `UTAU Editor → Difficulty display` | 手动更新、保存时更新、下方总览时间轴的 strain 曲线、整图星数 |
| 音效工作流 | 左侧 `UTAU Editor → Hitsound tools` | 快速编辑、参考导出、MIDI／mania 导入合并 |
| 形状与动画生成 | 右侧 `Generate` | Polygon、Slider Toolbox、Slider Toolbox 2、Slider Toolbox 3 |
| 可复用物件库 | 右侧 `Generate → Pattern Gallery` | 保存选区、跨图插入、库导入／导出 |
| 精确变换 | 右侧 `Transform` | 移动、旋转、缩放 |
| 网格 | 右侧 `GRID` | 网格类型、间距、偏移、Opacity |
| 整张难度平移 | `Timing` 页面右侧 `Global offset...` | 同步移动 timing、物件及时间标记 |
| 全局显示与步长 | 顶栏 `UTAU_Editor` | Slider body、选择／控制点外观、吸附参数、round/bevel 大小、侧栏顺序 |
| 选区／slider 局部操作 | 物件右键菜单 | Convert to stream、Random perturb、SV 处理、插点／分割等 |

## 面板操作约定

长面板按当前可用空间限宽限高，通过滚动访问内容。Toolbox 1/2 的预览与参数栏在窄窗口上下排列；参数行按可读宽度换行。长错误信息会换行，避免关键原因被截断。工具内部打开文件选择器时，外层工具保持打开。

Polygon、Toolbox 1/2/3、Random perturb、Slider velocity、Convert to stream 共用固定在底部的 **Apply／Cancel**。调参会更新临时预览；Apply 提交为一次可撤销编辑；Cancel 或关闭面板撤回预览。结束预览后，排队中的旧更新不能再次写入谱面。关闭工具前应完成 Apply 或 Cancel，再保存谱面。

设置类面板即时生效，不增加多余的确认按钮。精确移动／旋转／缩放沿用现有变换操作，关闭时结束当前编辑；它们与可取消的生成预览不同。Hitsound 的执行按钮、Gallery 的 Save／Insert／Import 也各自即时执行，查看按钮旁的说明即可判断作用范围。

多个任务共用一个面板时，顶部 `Task` 选择当前任务；切换任务不丢失已填参数。没有把相同参数再次复制到顶栏。

## Slider 曲线

在 `Slider curves → Editing settings` 调整模式。开启 `Unified curve controls` 后，纯 Bézier 使用锚点与方向手柄；混合路径及其它类型仍使用 Classic 控制点。`Alt+P` 快速切换。编辑结果仍是原生 slider 数据，不需要专用 `.osu` 格式。

普通 Pen 单击放直线节点，按住拖动才建立手柄。`Curvature pen` 是独立选项，打开后单击节点会自动平滑。`Shift+右键` 点一个手柄只移除该手柄；节点工具和快捷键说明在 `Shortcuts & gestures` 页及右键菜单中。

`Keep duration when editing` 同时服务 Classic 与 Pen：以编辑后的完整路径重新计算 SV，保持时长；包含增删控制点和缩放。无法形成有效路径、或所需 SV 超出允许范围时，操作应保留原几何并提示，不能用截短路径伪装成成功。新放置 slider 的默认 SV 根据当前选中或其之前的 slider 继承，用户主动指定默认 SV 时尊重手动覆盖。

Classic 的节点处理区分 segment 起点与 inherited 控制点，支持混合路径。Round／Bevel 的裁切比例位于顶栏 `UTAU_Editor → Workflow → Round/bevel trim fraction`；只保留拐角内侧需要的手柄。`Classic live symmetry` 是随当前单选 slider 更新的镜像视觉预览，不会自动插入或合并第二个谱面物件。

`Contour operations` 针对选中的纯 Bézier 闭合轮廓执行采样布尔／偏移运算。先 Preview，确认后 Apply；可选择是否替换来源。结果为普通 slider，孔洞作为独立轮廓输出。源物件发生变化后必须重新预览。

高控制点 slider 可在 `Slider detail` 中使用粗略显示，避免创建大量可交互节点；需要逐点编辑时再启用详细模式。

## 生成工具

Slider Toolbox 生成公式、形状、符号等曲线；Slider Toolbox 2 生成向量场、噪声、隐式轮廓、图路径、吸引子等曲线。`Frames = 1` 表示单根 slider，更多帧表示参数插值动画；符号 Sequence 模式按输入 token 生成。

Toolbox 1/2 和 Polygon 打开时捕获当前吸附位置。`Insert at (ms)` 保持锁定，移动时间轴只改变观察位置；需要改变插入点时点击 `Use timeline cursor`。帧 duration 和 gap 用当前 snap divisor 表达，跨 BPM 变化按各段拍长推进。显示的 SV 超限提示需要通过时长或形状大小解决。

Slider Toolbox 3 的操作顺序：

1. 在[外部配套工具](../tools/pose-slider-companion/README.md)识别视频，先预览识别后的火柴人，再导出 pose JSON。
2. 在 Toolbox 3 拖入 JSON，或点击 `Drop pose JSON here / Browse…` 使用内置文件浏览器。
3. 在时间轴定位开始，点击 `Lock start at cursor`；定位结束，再点击 `Lock end at cursor`。起止值在当前 editor 会话中保留，滚动时间轴不会自动修改。
4. 按原始帧数设置 `Sample density`，用 `Gap between sliders (snaps)` 设置空拍，再调整姿态定位、缩放与偏移。
5. 检查预览、slider 数量和 SV 范围，再 Apply。每个采样姿态映射为一根原生 Linear slider；编辑器不执行视频识别。

## Pattern Gallery

上方包含选区保存和查找／插入设置，下方是分页的多列库。输入名称和可选标签，点击 `Save selected objects` 保存当前选区；浏览卡片后点击 Insert，在当前时间轴位置插入新物件。

开启 `Snap objects and slider tails` 时，新条目按保存的拍域信息保持物件间隔、空拍与 slider 占拍数。插入到不同 BPM 的谱面时，形状和 repeat 保留，用目标 timing 重新计算 SV；若 SV 超限，整次插入在改动谱面前拒绝。关闭此选项则沿用原始毫秒间隔。无拍域信息的旧条目走目标谱面逐物件／尾部吸附兼容路径。

`Gallery data` 中选择 JSON 会立即合并：相同 ID 的条目替换，其他条目保留。Export 将便携文件写到当前数据目录的 `pattern-gallery/gallery-export.json`，面板显示实际路径。日常库保存在同目录的 `gallery.json`；库操作独立于谱面撤销，分享或批量导入前可先导出一份。

库文件无法解析或版本过新时，面板报告路径并停止写入，避免用空库覆盖原文件。保存失败不先删除内存中的旧数据。物件插入成功但使用次数保存失败，会明确区分这两个结果。

## Hitsound

`Task` 将工作分为三个页面：

- `Quick edits`：静音全部／所选 slider 的尾音、设置所选物件音量、保留已有 5% 静音样本。
- `Export reference`：MIDI、音符参考难度、mania 多列参考难度导出。
- `Import / merge`：选择 `Mania columns` 或 `MIDI notes`，设置映射／文件后执行。

导入页面的 matching tolerance 和 clear-existing 同时作用于两种来源。**Clear-existing 会清除全图现有音效，包括没匹配上的物件**，不是仅清理已匹配对象；谱面撤销可恢复。匹配保留各工具已有的 head／tail 优先规则。

## Timing 与难度显示

替换音频后若需要整体挪动，使用 `Timing → Global offset...` 输入有符号的整数毫秒。正数后移，负数前移；当前难度的 timing/effect points、物件、break、bookmark、已设置的 preview time 一起移动，并支持一次撤销。它不修改音频、其它难度、video 或 storyboard。Timing 可为负，物件或标记移至负时间则整次拒绝。

难度计算仅由手动 Update 与启用后的保存事件触发，不随拖拽、增删或撤销实时重算。整图星数复用原生计算；曲线显示的是各段 strain，而不是逐步累计星数。`Difficulty display` 中可设置下方总览时间轴曲线的可见性、前后层级、透明度、高度和采样间隔，以及整图星数位置。默认曲线在 kiai 等标记上方。

## 配置与数据

自定义配置使用 `game.utau-editor.ini`；Gallery 使用独立 JSON；最终谱面仍使用原生数据与默认兼容导出。专用 `slider optimization` 导出已移除。

公开桌面便携包默认将配置、谱面、皮肤和快捷键数据库放在包内 `data/`，另有独立 IPC，不会自动接管官方数据。手动把两端指向同一目录仍有数据库版本和 action ID 冲突风险；通过 `.osz`／`.osk` 和 Gallery JSON 导入内容，不直接复制官方 Realm。更新及目录说明见[便携使用](portable.md)。
