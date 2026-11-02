# UTAU Editor 维护与开发指南

更新：2026-11-03。功能入口见[使用指南](editor-guide.md)，验证与剩余事项见[优化记录](changelogs/UTAU_Editor_unified_workflow_audit_2026-11-02.md)。发布与上游同步见[发布指南](releasing.md)。

## 环境与工作约定

- 主要分支 `UTAU_Editor`；`origin` 是定制仓库，`upstream` 是 `ppy/osu`。已合并官方基线 `676f3b01f2`。
- `global.json` 要求 .NET 10.0.100，项目使用 C# 14；IDE 也必须支持该 SDK。
- 按修改范围运行构建与定向测试；人工检查交互观感。不要拿真实谱面、配置或 Realm 作为测试夹具。
- 先检查工作树并保留现有改动。涉及数据库迁移、快捷键数字 ID、导出语义的修改应单独设计。
- 不把旧实现留成大片注释；历史追溯使用 Git。新增行为同步更新指南，版本变化另写 changelog。

## 代码职责

| 范围 | 主要入口 | 职责边界 |
| --- | --- | --- |
| Editor 顶层 | `osu.Game/Screens/Edit/Editor.cs` | 顶栏、保存、快捷键与编辑页面协调 |
| Compose 与侧栏 | `osu.Game/Rulesets/Edit/HitObjectComposer.cs`；`osu.Game.Rulesets.Osu/Edit/OsuHitObjectComposer.cs` | 公共框架／osu 扩展；不要在加载后整体重建侧栏 children |
| 侧栏顺序 | `EditorToolboxOrder`、`EditorToolboxOrderDialog` | 稳定组 ID、真实 layout position、配置绑定；不用 Depth 当排序 |
| 公共工具 UI | `osu.Game/Screens/Edit/Components/EditorTool*.cs`、`EditorPopover.cs` | 受限面板、任务页、响应式布局、时间锁定、反馈、操作栏 |
| 预览事务 | `EditorPreviewSession`、`EditorPreviewPopover` | 一次 Begin/End、关闭状态、回滚间接副作用；具体几何由工具负责 |
| 曲线与时长 | `Edit/Curves/`；`OsuSelectionScaleHandler` | 锚点模型、Classic 混合路径、原生拓扑、duration/SV 补偿 |
| 生成算法 | `ParametricSliderGenerator`、`NoiseSliderGenerator`、`DancePoseSliderGenerator` | 从输入参数生成路径；不把 UI 状态混进算法 |
| Gallery | `Edit/PatternGallery/` | Operations 转换物件；Store 管库写入；Popover 管交互 |
| 拍域计算 | `osu.Game/Beatmaps/Timing/BeatTimeline.cs` | 跨 BPM 的时间／拍数换算，供生成器和 Gallery 共用 |
| Hitsound | `EditorHitsound*`、`HitsoundTimeMatcher` | 音效处理、格式转换、已排序时间窗口匹配 |
| 难度 | `DifficultyCalculator.CalculateWithTimeline`、`DifficultyTimelineSampleBuilder`、`DifficultyRatingTimelineOverlay` | 单次计算、采样、总览时间轴渲染分离 |
| 配置 | `OsuConfigManager`、`UtauEditorOsuConfigManager` | 默认类型、范围、绑定与 `game.utau-editor.ini` |
| 全局 offset | `Screens/Edit/Timing/GlobalOffsetAdjustment.cs` | 预检后统一平移；UI 不自行分批挪物件 |
| 外部识别 | `tools/pose-slider-companion/` | 视频到标准 pose JSON；编辑器只读取并映射 slider |

表内无完整路径的 osu 专用类位于 `osu.Game.Rulesets.Osu/Edit/` 或其子目录；通用编辑类位于 `osu.Game/Screens/Edit/`。

## 一致的 UI 规则

纯显示／工作流参数留在已有设置位置；依赖选区的动作放右键或相关工具面板；生成工具继续集中在右侧 Generate。不要为同一参数新建第二套 bindable 或第二个设置入口。曲线的 round/bevel trim fraction 保留顶栏位置。

新工具从现有公共组件组合：

| 组件 | 使用方式 |
| --- | --- |
| `EditorPopover` | 提供一个 root；以实际宿主坐标限宽限高，负责滚动与嵌套 popover host |
| `EditorToolPages` | 按工作任务切页，保留页内控件及数值；只提供非空、标题唯一的页面列表 |
| `EditorToolControlRow` | 按最小控件宽度自动换行；不要固定放很多半宽控件 |
| `EditorToolSplitPane` | 宽屏并排、窄屏上下；可滚动的参数区由调用方提供 |
| `EditorToolTimeAnchor` | 打开捕获吸附时间，按钮显式重新捕获；不要把预览时间绑定到每帧 clock |
| `EditorToolStatus` | 内容驱动高度的错误／结果文字，不用固定一行裁掉原因 |
| `EditorToolActionRow` | 作为 root 直接子项，由基类抽出到固定 footer；仅用于可提交／取消的预览 |

不要再把文件选择依赖于本机不一定可用的系统 dialog。工具内使用现有 `FormFileSelector`／`OsuFileSelector`，并让子弹窗使用最近的宿主。初始化或清空文件不等于“用户关闭工具”。

osu.Framework 的可见容器在加载隐藏状态时可能先调用 `PopOut`；此时不能提前关闭预览事务。相对尺寸按钮必须显式设置 `Size = Vector2.One`，不能把 RoundedButton 默认 40px 高度当作相对高度。

## 预览、撤销与数据一致性

`IEditorChangeHandler` 管用户可见撤销事务，`EditorBeatmap.BeginChange/EndChange` 管批量物件处理；二者不是可随意互换的开关。

可取消工具继承 `EditorPreviewPopover`：

1. 每个预览写入入口先调用 `BeginPreview()`，失败即返回，包括 Scheduler 回调。仅检查面板是否曾加载不能防止关闭后的写入。
2. 在脱离谱面的对象上计算、校验完整候选结果；通过后再替换。`ReplacePreviewObjects` 把旧预览移除和新预览添加合成一次批处理。
3. Apply 先保证当前输入有有效结果，再 `CommitPreview()`。Cancel／PopOut 由基类关闭 session，再调用工具的 `RestorePreview()`。
4. 工具只负责恢复自身物件或几何。基类另恢复预览间接影响的原有 combo、break、beatmap version；先 flush 物件处理再恢复 break，防止后续生成覆盖。

`EditorPreviewSession` 是一次性对象，Commit／Cancel 幂等；关闭标志先于回滚设定，防止重入和排队回调重新插入。`EndChange` 用 finally 平衡。不要在异步 Dispose 中无条件修改谱面来补救生命周期。

精确变换沿用变换 handler，不强行转为可取消工具；只在对应操作确实开始后提交。外部事件订阅在 Dispose 解除；共享 bindable 尽量使用工具持有的 bound copy，避免已隐藏的工具持续响应。

保存发生在活动预览期间的交互尚未统一；使用指南要求先 Apply／Cancel 再保存。新增相关行为必须明确事务与保存快照边界。

## 曲线、SV 与时间约束

- `.osu` 只保存原生控制点、ExpectedDistance、SV 等数据；不要引入只在定制端可读的谱面格式。Gallery 与 pose JSON 是外部工作流数据，不替代谱面格式。
- 纯 Bézier 的锚点模型经 `AnchorEditSession` 转回原生点。更新拓扑使用原子替换，不能 `Clear()` 后逐点 Add，让中间空路径被输入／可视化读取。
- `SliderDurationPreserver` 是 Classic 与缩放的共用入口；Pen 也必须维持“完整路径＋目标 duration＋重新求 SV”的一致性。修改倍率后及时刷新实际 velocity，不能只改 bindable 等下一次 defaults 才生效。
- `ExpectedDistance` 与 `CalculatedPath` 的语义要以现有类为准；后者已经包含长度裁切／延伸，不要把它误判为原始未裁切路径。
- 新增控制点、拖拽、类型切换、缩放分别检查是否再次执行 SnapTo，避免把 Keep duration 结果覆盖。已有回归覆盖纯 Bézier 插点、删除、两点 Perfect Curve 与缩放。
- 保留按帧合并 pointer 写入。命中测试采样缓存依赖模型对象和 screen-space 仿射变换，路径、缩放、旋转、移动变化都必须失效。
- `BeatTimeline.BeatsBetween` 积分各 timing 段；`TimeAfterBeats` 从起点推进非负拍数。用于声称“保持占拍数”的功能，不使用一个初始 BPM 贯穿整个序列。
- Gallery 在临时对象上校验目标 SV；超限拒绝，而不是 clamp 后静默改变节奏。UI 应明确哪个参数可以调整。

## 性能与持久化

难度计算保留手动／保存触发。主线程只建立稳定谱面快照，后台 `CalculateWithTimeline` 单次处理 skills 并计算最终属性，generation 与 cancellation 防止旧结果覆盖新请求；对应结束时间跟计算结果一起保存。不要重新调用逐物件累计属性接口只为取得最后一项。

Hitsound 目标先按时间稳定排序，`HitsoundTimeMatcher` 二分定位窗口，只扫描容差范围。保持一对一匹配、同距取更早项、MIDI 特定目标优先等现有规则。任何清空操作之前先验证容差等输入。

Gallery 写入采用“候选集合 → 安全写文件 → 替换内存状态”；解析失败／未来格式进入只读错误状态，不能当空库继续保存。物件插入与使用统计写入是两个结果，错误提示必须区分，不能让用户误以为没插入而重复点击。分页只反序列化当前可见卡片。

JSON 序列化沿用仓库 serializer，包括 `snake_case` 属性名；测试应使用实际 serializer，避免手写不存在的 `FormatVersion` 字段。外部库格式兼容不等于所有任意手工修改数据都已深度验证。

配置 `GetBindable<T>()` 的类型必须与默认值一致。SliderBar 的数值 bindable 必须有有效 MinValue/MaxValue。不要为了 UI 显示两位小数而截断模型里的精确 SV。

`GlobalAction` 的数字 ID 持久化在 Realm。自定义成员与官方数字 ID 存在差异。**禁止通过简单重排枚举来“整理代码”**；上游同步涉及新增 action 时要明确 ID 兼容和迁移。公开桌面包默认使用独立数据目录和 IPC，但用户手动指向官方数据目录仍有风险；不自动迁移官方 Realm。

## 有限、可复现的验证

从仓库根目录执行，先 `dotnet restore osu.Desktop.slnf`，使用满足 `global.json` 的 SDK。

```powershell
dotnet build osu.Desktop/osu.Desktop.csproj --no-restore -v quiet -p:RunAnalyzers=false

dotnet test osu.Game.Tests/osu.Game.Tests.csproj --no-restore -v quiet -p:RunAnalyzers=false --filter 'FullyQualifiedName~EditorPreviewSessionTest|FullyQualifiedName~BeatTimelineTest|FullyQualifiedName~HitsoundTimeMatcherTest|FullyQualifiedName~TestSceneEditorPopoverLayout|FullyQualifiedName~EditorToolboxOrderTest|FullyQualifiedName~TestSceneSliderVelocityInheritance|FullyQualifiedName~GlobalOffsetAdjustmentTest|FullyQualifiedName~TestSceneEditorSummaryTimeline'

dotnet test osu.Game.Rulesets.Osu.Tests/osu.Game.Rulesets.Osu.Tests.csproj --no-restore -v quiet -p:RunAnalyzers=false --filter 'FullyQualifiedName~AnchorPathTest|FullyQualifiedName~PatternGalleryTest|FullyQualifiedName~DifficultyTimelineSampleBuilderTest|FullyQualifiedName~DancePoseSliderGeneratorTest|FullyQualifiedName~TimelineFinalAttributesMatchOfficialCalculation|FullyQualifiedName~TestSceneEditorToolboxPreview|FullyQualifiedName~TestAnchor|FullyQualifiedName~TestDeletingControlPointKeepsDuration|FullyQualifiedName~TestAddingBezierControlPointKeepsDuration'

python -m unittest discover -s tools/pose-slider-companion/tests -v
```

修改单个区域时缩小 filter；不要为了“全面”反复全量执行。Headless UI 测试通过真实 drawable/input 检查点击、关闭、滚动、事务，不代替人工审美与长时间使用。时间性能改进优先用算法调用数／分配数约束测试，不用易波动的绝对耗时断言。

外部工具源位于 `tools/pose-slider-companion`。独立安装时不要覆盖 venv、模型、视频或用户输出；这些内容不应纳入版本管理。打包脚本和发布原生依赖检查位于 `scripts/`。

## 文档维护

现行入口为 [docs/README](README.md)、本指南和使用指南。`archive` 保留算法研究，`changelogs` 保留验证历史，不代表现行入口。私人交接材料不随公开快照发布。新功能说明实现状态和限制；不能用“已全面验证”代替具体覆盖范围。
