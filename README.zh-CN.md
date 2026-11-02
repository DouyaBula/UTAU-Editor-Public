# UTAU Editor

[English](README.md) · **简体中文**

基于 [osu!lazer](https://github.com/ppy/osu) 的非官方个人编辑器，面向实验性制谱、slider 图案和动画制作。仅作个人分享，与 ppy 及 osu! 官方团队无隶属或支持关系。

自定义功能大量借助 LLM 开发，经过定向自动测试与个人使用验收，但未经全面的独立代码审查。编辑重要谱面前请备份。

[下载便携版](https://github.com/DouyaBula/UTAU-Editor-Public/releases) · [使用指南](docs/editor-guide.md) · [构建与发布](docs/releasing.md) · [反馈问题](https://github.com/DouyaBula/UTAU-Editor-Public/issues)

## 相比原版新增了什么？

新增功能主要服务于 **osu!standard 编辑器**，保留 lazer 原有编辑框架。下表概括本分支的额外能力与行为调整。

| 领域 | 新增功能 |
| --- | --- |
| **Slider 绘制** | Classic 控制点与纯 Bézier 的 Pen 锚点／方向手柄模式随时切换（`Alt+P`）。单击画直线，拖动创建手柄，可选 Curvature pen 自动平滑。尖角／平滑／对称节点、独立手柄编辑与收回、增删节点、细分、开闭路径、反向、镜像、对齐和分布。 |
| **Classic 曲线** | 对混合路径进行倒角、圆角、平滑／对称／收回手柄等操作，区分段落起点与 inherited 控制点；线性段简化、可调拐角大小、当前 slider 的实时镜像预览，重新提供 Catmull 类型。 |
| **轮廓运算** | 对采样后的闭合 Bézier 轮廓预览联集、交集、差集、异或、偏移／描边轮廓，再输出普通 slider。 |
| **时长与 SV** | 增删／拖动控制点或缩放时，保留完整路径并自动补偿 SV，保持时长；新 slider 继承选中／之前 slider 的 SV，也可手动覆盖。定时长 SV／缩放快捷键，以及支持加减速、easing、折返参数的变速 slider 工具。 |
| **Slider Toolbox** | 参数化几何曲线、波形、螺旋、分形、3D 投影与数学符号；符号序列、起点／路径类型、缩放／旋转／偏移、首尾参数插值和 easing；按 snap 配置每帧时长与空拍的 slider 动画序列。 |
| **Slider Toolbox 2** | 向量场流线、噪声、隐式轮廓、图／迷宫路径、吸引子、笔触语法、程序化 mask 轮廓与节拍驱动曲线；可复现 seed、采样和平滑，以及同一套动画流程。 |
| **Slider Toolbox 3** | 将姿态 JSON 映射为“跳舞”的 slider，每个采样火柴人对应一根原生 slider。锁定起止时间、按源帧数调采样密度、设置姿态间空拍与定位／缩放；支持拖入或浏览导入。独立的 [Python 配套工具](tools/pose-slider-companion/README.md) 负责视频识别和骨架预览。 |
| **Pattern Gallery** | 保存单个 slider 或一组物件，多列预览、搜索／标签／排序，跨图复用。跨 BPM 保持物件占拍、间隔、空拍、形状与 slider 尾部时刻，并反算 SV；库 JSON 导入／导出，SV 超限会拒绝插入。 |
| **选区与变换** | 可选原点的精确移动／旋转／缩放、可调微移距离、可选越界放置、带 seed 的随机扰动、加速／镜像 Convert to stream、在时间轴当前位置插点／分割及 tangent glitch。 |
| **Timing 与难度** | 全局 offset 一次平移当前难度的 timing/effect line、物件、break、bookmark、preview time，支持撤销。显示原生整图星数及下方总览时间轴上的连续局部 strain 曲线；手动／保存更新，可调位置、透明度、高度和渲染层级。 |
| **Hitsound** | 跨难度复制／合并音效、slider 尾音静音、批量音量与 5% 静音样本；导出 MIDI、音符／mania 参考难度；按映射及时间容差导入 MIDI 和 mania 列音效。 |
| **画布与外观** | 画布缩放／平移、可调网格透明度、slider body 开关（默认显示）；自定义选择描边与颜色、选框按钮／手柄、控制点大小与命中范围、控制线和 tail arc。密集 slider 的粗略／详细显示、简化击打／选择特效、细分背景变暗比例。 |
| **工作流** | 侧栏组排序并持久化、双值 AR 快切、可配置吸附目标、更低的 tick rate 范围和制谱默认配置；限宽限高的可滚动面板、任务分页、响应式布局，以及预览工具固定的 Apply／Cancel 操作栏。 |

### 从哪里打开

- **左侧栏 → UTAU Editor：**画布、AR、Slider detail／curves、Difficulty display、Hitsound tools。
- **右侧栏 → Generate：**Polygon、Slider Toolbox 1/2/3、Pattern Gallery；**Transform／GRID：**精确变换和网格。
- **顶栏 → UTAU_Editor：**外观、吸附、工作流参数、侧栏排序；**Timing 页面：**Global offset。
- **物件／节点右键菜单：**局部曲线修饰、转串、扰动、SV 和分割操作。

生成预览使用 **Apply／Cancel** 并支持撤销；保存前先结束预览。形状工具打开时锁定插入点，点击 **Use timeline cursor** 才重新定位。难度计算仅在手动请求或保存时执行，避免每次编辑都后台重算。

## 便携版

Release 工作流面向 Windows x64／ARM64、Linux x64／ARM64、macOS Intel／Apple Silicon 构建。下载对应资产，完整解压到可写目录：Windows 运行 `osu!.exe`，Linux／macOS 运行 `./launch.sh`。无需另装 .NET。这是未签名的个人构建，macOS 未公证；Linux 仍需系统图形、音频和桌面运行库。

保留可执行文件旁的 `utau-editor.portable`。谱面、皮肤、快捷键、编辑器配置和 Gallery 默认位于包内 **`data/`**；framework 设置可能也在可执行文件旁。不会使用官方自动更新或抢占文件关联。更新时先关闭两版程序，解压新包，再迁移 `data/` 和可选的 `framework.ini`，详见[便携使用说明](docs/portable.md)。

从已有客户端复制内容请使用 `.osz`／`.osk` 导入。**不要把数据目录设为官方端正在使用的目录，也不要复制其 Realm 数据库**，两者的数据库版本和快捷键 ID 可能不同。姿态识别配套工具为可选项，需要单独安装 Python 和依赖。

## 兼容性与范围

产物仍为原生 osu! 谱面，使用默认兼容 `.osz` 导出；Gallery 和姿态 JSON 是辅助数据，不是新谱面格式。Pen 仅用于纯 Bézier，其他及混合路径使用 Classic。密集图案、极端 SV 和 stable 的整数坐标转换可能影响效果，请在目标客户端检查导出结果。已退役的 “slider optimization” 导出不包含在内。

本项目用于编辑和视觉实验，不作为官方在线游戏客户端的受支持替代品，也不保证生成谱面满足排名要求或在各客户端显示完全相同。

## 构建与同步上游

安装 `global.json` 指定的 SDK（**.NET 10**，IDE 需支持 C# 14）：

```sh
dotnet run --project osu.Desktop -c Debug
# 自包含便携构建示例：
dotnet publish osu.Desktop -c Release -r win-x64 --self-contained true -o artifacts/portable
```

公开历史保留官方提交，将定制编辑器作为一个初始快照引入，因此后续仍能正常 `git fetch upstream`／`git merge upstream/master`。详见[发布与上游维护](docs/releasing.md)；[维护指南](docs/maintainer-guide.md)说明公共 UI、预览／撤销及定向测试。

## 致谢与许可

基于 [osu!lazer](https://github.com/ppy/osu) 和 [osu!framework](https://github.com/ppy/osu-framework)。Pattern Gallery 和 strain 可视化参考了 [Mapping Tools](https://github.com/OliBomby/Mapping_Tools) 与 [MapsetVerifier](https://github.com/Naxesss/MapsetVerifier) 的思路，并针对本编辑器重新设计交互。

代码保留上游 [MIT 许可](LICENCE)。资源、字体与原生依赖各有许可，见[第三方说明](THIRD_PARTY_NOTICES.md)；其中 osu!resources 大部分内容为 **CC BY-NC 4.0**，osu!／ppy 品牌不包含在代码许可内。本项目仅作非商业分享。
