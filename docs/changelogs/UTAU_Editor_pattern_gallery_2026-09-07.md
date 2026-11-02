# Pattern Gallery（2026-09-07）

在右侧栏 `GENERATE` 中新增 **Pattern Gallery**。它参考 Mapping Tools 的 gallery 思路，但按 lazer editor 的对象、撤销和存储模型重新实现。

## 功能

- 保存当前选中的一个 Slider 或一组 osu! hit objects，并为其设置名称和标签。
- gallery 数据跨 beatmap、跨编辑会话保存在 app storage 的 `pattern-gallery/gallery.json` 中，不写入任何 `.osu` 文件。
- 以 editor 自身的 typed clipboard 序列化保存对象，因此 Slider 路径、重复次数、样本等原生属性可以一并复用。
- 搜索名称和标签；可按最近使用、名称或使用次数排序；卡片使用带 Slider body、头尾和 repeat 标记的轻量 osu! 风格缩略图。
- 缩略图会根据物件密度自适应缩放标记、降低路径采样并对密集的普通物件做均匀抽样，避免大量物件挤成不可读的色块。
- `Gallery data` 面板支持导出便携 JSON，以及从外部 JSON 合并导入；导入按 pattern id 去重，不会影响其它 pattern。
- 将任意卡片插入当前光标位置；默认把所有物件起点和 Slider tail 重新吸附到目标 beatmap 的 timing，并自动调整 Slider velocity 以保持 tail 对齐。
- 插入会生成全新的原生 hit objects，现有物件不会被静默覆盖，并作为一次完整 undo 操作处理。

## 设计取舍

本实现没有照搬 stable 工具的独立 `.osu` 文件集合、外部编辑器读取和复杂 timing/SV 覆盖矩阵。lazer editor 已经拥有可靠的 typed clipboard 与 beatmap change transaction，因此 gallery 只保存对象快照和轻量元数据，把时间偏移、beat snap、undo 和当前 beatmap生命周期交给 editor 处理，减少跨文件复用时的隐式改写。

入口和面板分为上方的 `Capture selection` / `Find & insert` 双栏，以及下方全宽的 `Saved patterns` 区域，保存/搜索/插入路径保持一致且不会污染当前 beatmap。`Saved patterns` 使用紧凑的双列卡片和分页浏览，避免 gallery 面板遮挡编辑器其它区域；分页会随搜索结果自动更新。

工具组排序复用了每个组原有的折叠按钮：单击仍然折叠/展开，按住并拖动时重新排列左右侧栏组，并将顺序写入编辑器配置。侧栏组在规则集动态加载完成后会再次恢复已保存顺序，确保离开并重新进入 editor 后仍保持用户布局。
