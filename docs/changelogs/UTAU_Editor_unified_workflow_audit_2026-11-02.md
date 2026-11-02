# UTAU Editor — 统一交互与维护优化

日期：2026-11-02。状态：实现与定向自动验证完成，用户实机测试未发现问题并批准提交。本记录为公开初始快照之前的功能演进说明。

本轮围绕曲线、生成工具、Gallery、Hitsound、难度显示、公共面板和撤销生命周期检查并改造。以下是实际修改范围，不代表所有历史分支、极端参数或第三方输入均已穷尽验证。

## 交互统一

- 七类可取消工具统一固定底部 Apply／Cancel：Polygon、Toolbox 1/2/3、Random perturb、Slider velocity、Convert to stream。内容滚动不再带走确认／取消入口。
- Toolbox 1/2 的预览和参数区在窄窗口自动上下排列；参数行、分类按钮、Gallery 上方分组和 Toolbox 3 参数行按可读宽度换行。
- 生成结果、SV 超限与 JSON 读取错误改用可换行文字，不再塞进固定高度单行。
- Hitsound 面板按 Quick edits／Export reference／Import & merge 分页；后者再分 Mania columns 与 MIDI notes。共享匹配容差、清空选项显式说明；清空选项提示会影响全图未匹配对象。
- Slider curves 按 Editing settings／Contour operations／Shortcuts & gestures 分页；Difficulty display 优先呈现更新入口，再分曲线与整图星数设置。
- Toolbox 1/2、Polygon 打开时锁定插入时间，通过 `Use timeline cursor` 显式重定位。调参不再从滚动后的 clock 重新决定插入点。
- Gallery 导入改用工具内文件浏览器，不再依赖不可用的系统文件选择。选择 JSON 后合并，界面说明同 ID 替换规则。
- 保留熟悉的工具名称与侧栏位置，没有把参数重复复制进顶栏。设置面板即时生效、精确变换关闭时提交的既有语义保留，不强行套用生成器的 Cancel 逻辑。

## 预览生命周期与撤销

- 抽出 `EditorPreviewSession`／`EditorPreviewPopover`，统一一次性 Begin／Commit／Cancel，关闭先失效后回滚，重复关闭不会重复 EndChange。
- 所有排队预览写入先检查 session。解决关闭工具后旧回调仍可能重新添加物件、重开编辑事务的问题。
- 处理容器加载阶段初始 PopOut，避免还没显示就把 session 取消。
- 预览移除与重建合并为一次物件处理；取消除恢复物件，还恢复间接受影响的 combo、break 与 beatmap version。
- 精确移动／旋转／缩放只有实际开始操作后才结束事务。
- Toolbox 1/2 使用工具持有的节拍 bindable 副本；难度曲线释放共享事件订阅；Toolbox 3 关闭后不再处理排队导入。

## 计算与算法

- **难度**：编辑器只需要最终星数和局部 strain，却曾计算每个物件前缀的累计 difficulty attributes。新增单次 `CalculateWithTimeline`，不生成这些未使用的中间结果；官方 Calculate／CalculateTimed 保持原样。最终属性与官方计算的四组对照通过，包括真实谱面 fixture。仍仅手动／保存触发，不恢复实时重算。
- **难度快照**：结果携带同次快照结束时间；更改曲线采样间隔时不再混用当前谱面的新结束时间与旧曲线数据。
- **跨 BPM 拍域**：抽出 `BeatTimeline`。Toolbox 1/2 的帧时长和空拍按各段 BPM 推进；Gallery 保存与插入共用此算法，不再各自维护一份。
- **Gallery 时长**：要求超出原生范围的 SV 时在插入前拒绝，避免 clamp 后悄悄改变节奏；设置倍率后立即更新实际 velocity。新增多 BPM、repeat、空拍与 slider 尾部占拍回归。
- **姿态序列**：生成每帧前先设置当前采样时间，再取该时刻 BPM，避免使用上一帧时间。
- **Hitsound**：把逐来源全量扫目标改为二分定位＋容差窗口扫描；保留同距顺序、一对一资格及目标类型优先规则。无效容差在清空音效前拒绝。10 万目标的窄窗口测试通过时间访问次数约束，不依赖机器耗时。
- **曲线命中**：缓存 Bézier 段的屏幕空间采样，按模型与仿射变换失效，减少鼠标移动反复求样本／坐标转换；保留原子拓扑替换及每帧合并拖拽更新。
- **轮廓偏移**：补充有限值与范围验证。

## 数据与维护

- Gallery 无法读取或格式过新时报告只读错误，保留原文件，不能以空库覆盖。新增／删除／导入先写候选集合，写入成功再替换内存状态；使用统计写失败则恢复统计字段。
- Gallery 导出也采用安全文件写入；加载规范化去除重复 ID。插入成功与使用统计保存失败分别提示，避免误导重复插入。
- Toolbox 1 复用既有插值、easing、交换、复制与单帧禁用控件，删除重复实现。
- 删除无现行调用的 `EditorClassicSymmetryAxisMenuItem`、`EditorEasingMenuItem`，以及旧 Convert-to-stream preset 实现和失效注释。旧配置键不随意改类型或重排；历史可从 Git 恢复。
- 外部 pose companion 取消旧播放定时回调，避免快速暂停／播放叠加循环；识别完成后预览实际完成文件，不读取可能已被修改的路径输入框。未更改模型、虚拟环境或用户输出。
- 建立 [统一文档入口](../README.md)、[使用指南](../editor-guide.md)、[维护指南](../maintainer-guide.md)。退役导出算法保留研究归档；私人交接材料不包含在公开快照中。
- 原生谱面格式和默认 compatibility 导出未改变；未迁移真实用户数据库或配置。

## 验证证据

使用 .NET 10、Debug、定向过滤，构建关闭 analyzers；没有运行全量测试，也没有使用 computer-use。

| 验证组 | 结果 | 主要覆盖 |
| --- | --- | --- |
| 核心定向回归 | 49 通过，0 失败／跳过 | 预览事务、拍域、音效匹配、面板、侧栏排序、SV 继承、全局 offset、底部时间轴 |
| osu! 定向回归 | 79 通过，0 失败／跳过 | AnchorPath、Gallery、难度采样与最终属性、舞蹈生成、实际生成工具开关与撤销、关键 Keep duration 交互 |
| 最后 UI 补测 | 19 通过，0 失败／跳过 | 包含新增长状态文字换行，及已有布局／事务用例的重跑 |
| 最后生成工具补测 | 8 通过，0 失败／跳过 | 实际四个 Generate 面板打开／取消，锁定时间 Apply，单步撤销；含测试场景通用检查 |
| Python 配套工具单元测试 | 5 通过 | 骨架映射、缺失关节统计、输入／输出约束；不包含 Tk GUI 自动操作 |
| Desktop Debug 构建 | 通过，0 错误 | 保留现有 MessagePack 3.1.8 的 NU1902 警告 |

后两组 C# 补测包含前两组的重复用例，不将执行次数简单相加作为独立覆盖数量。本地 TRX 分别为 `editor-audit-final-core.trx`、`editor-audit-final-ruleset.trx`、`editor-audit-ui-final.trx`、`editor-audit-tools-final.trx`，位于对应项目的 TestResults；复跑命令见维护指南。

性能收益依据移除重复工作、限定访问范围和缓存结构；没有测量或宣称具体 FPS／毫秒提升。自动布局与点击检查不等于人工观感验收。

## 已知边界与后续事项

1. **快捷键数据库兼容**：自定义 GlobalAction 数字 ID 与官方共库仍可能冲突。需要单独的迁移／隔离设计，本轮没有重排枚举或修改用户 Realm。
2. **Snap all 跨 BPM**：`SnapAllHitObjectsToCurrentDivisor` 的现行 SV 比例计算未显式覆盖 slider 头跨 timing 点；另有超限 clamp 和事务 finally 边界待验证。本轮没有修改该批量算法，不能宣称此边界已解决。
3. **超大输入与完整外部链路**：pose JSON 读取及生成仍在更新线程；所有生成形状的极端参数、完整 MIDI tempo-map／DAW 往返、Gallery 并发外部写入与任意手改元数据未穷尽验证。
4. **活动预览时保存**：长事务与保存的产品语义仍需单独明确；目前建议先 Apply／Cancel 再保存。关闭 editor 等异步销毁路径不在基类 Dispose 中强行回写谱面。
5. **依赖警告**：MessagePack NU1902 原有警告未在本轮升级处理。

## 建议人工验收

- 缩小窗口后使用 Hitsound 的两类导入页；打开文件选择再返回，确认工具不消失、底部控件可点击。
- Toolbox 1/2 调参、滚动时间轴，再 Apply／Cancel；确认插入点不漂移、Cancel 无残留、Apply 一次撤销。
- 多 BPM 谱面上预览动画和插入含空拍、repeat slider 的 Gallery pattern，检查节奏及超限反馈。
- Classic／Pen 的拖拽、增删点与缩放保持时长；手动／保存刷新难度，确认撤销不会触发难度重算。
- 外部姿态预览连续暂停／播放，完成识别后检查自动加载的文件是否正确。
