# UTAU_Editor v2 更新日志

发布日期：2026-06-07

本文记录自 `UTAU_Editor v1` 更新日志之后新增的编辑器功能。

## Convert to stream 参数面板

- 将 osu! 模式 slider 右键菜单中的 `Convert to stream` 从预设曲线选择改为参数面板。
- `Ctrl+Shift+F` 现在会打开同一个 `Convert to stream...` 参数面板，而不是直接执行预设转换。
- 新增 `Acceleration` 参数，用于控制转换后 note 在 slider 路径上的非等距分布强度。
- 新增 `Normal` / `Mirror` 模式：
  - `Normal` 下正向 acceleration 对应 ease in，负向 acceleration 对应 ease out。
  - `Mirror` 下正向 acceleration 对应 ease in & out，负向 acceleration 对应 ease out & in。
- 调整参数时会实时生成转换后的 circle 预览。
- 点击 `OK` 后才正式删除原 slider，并保留预览生成的 circles。
- 点击 `Cancel` 或关闭面板时会移除预览 circles，并保留原 slider。
- 预览期间原 slider 会临时隐藏，只显示转换后的 circles，便于观察最终 pattern。
- 调整 `Acceleration` 时，参数面板会短时间大幅降低不透明度，减少面板遮挡对预览判断的影响。
- 新增配置项用于保存上次使用的 `Acceleration` 与 `Normal/Mirror` 模式。
- 旧的 preset-based easing 菜单入口已从菜单中停用，相关代码保留为注释/辅助代码，便于后续对照或回退。

## 随机扰动选中物件

- 新增 osu! 模式选中物件的随机位置扰动功能。
- 选中一个或多个可移动物件后，右键菜单新增 `Random perturb...`。
- 新增快捷键 `Ctrl+Shift+J`，可直接打开随机扰动参数面板。
- 新增扰动类型：
  - `Circle`：在圆形范围内随机扰动。
  - `Square`：在方形范围内随机扰动。
  - `Horizontal`：只扰动 X 轴。
  - `Vertical`：只扰动 Y 轴。
- 新增 `Strength` 参数，用于控制扰动强度，单位为 px。
- 新增 `Seed` 参数，用于固定或切换随机结果。
- 扰动结果按 seed 与物件顺序确定；调整 strength 时随机方向保持稳定，不会每帧重新随机。
- 调整扰动类型、强度或 seed 时，会实时预览选中物件的新位置。
- 点击 `OK` 后保留当前扰动结果，并作为一次编辑变更提交。
- 点击 `Cancel` 或关闭面板时，会恢复所有选中物件的原始位置。
- 预览期间会刷新 selection box，并重新应用 osu! stacking，使选择框和堆叠显示尽量跟随当前预览状态。
- 调整 `Strength` 或 `Seed` 时，参数面板会短时间降低不透明度，减少遮挡。
- 新增配置项保存随机扰动的类型、强度与 seed。

## 修正与实现细节

- 修复随机扰动 `Seed` 配置的 bindable 类型不匹配问题，当前以 float 配置保存，并在生成随机数时取整使用。
- 为新面板补齐必要的 popover、hotkey、editor change handler 相关引用。
- 新增功能均复用 osu!lazer 现有 `OsuPopover`、`FormSliderBar`、`EditorRadioButtonCollection` 和 `RoundedButton` UI 组件。
- 新增功能沿用实时预览 + `OK` 提交 + `Cancel` 回滚的交互模式。

## 更新日志维护策略

- 从本版本开始，每轮新增功能默认创建新的更新日志文件。
- 后续不再默认追加旧更新日志，除非明确要求修改某个已有日志文件。
