# UTAU Editor 文档

现行文档更新于 **2026-11-03**。[English overview](../README.md) · [中文功能总览](../README.zh-CN.md)。

## 从这里开始

| 需要了解什么 | 阅读入口 |
| --- | --- |
| 功能在哪里、如何操作、哪些设置会保存 | [编辑器使用指南](editor-guide.md) |
| 代码职责、预览与撤销、时间换算、如何扩展与测试 | [维护与开发指南](maintainer-guide.md) |
| 本轮具体修复、验证结果、仍需处理的事项 | [2026-11-02 优化记录](changelogs/UTAU_Editor_unified_workflow_audit_2026-11-02.md) |
| 舞蹈视频识别、JSON 交换格式与外部预览 | [Slider Toolbox 3 配套工具](../tools/pose-slider-companion/README.md) |
| 便携包数据、更新与平台说明 | [便携使用](portable.md) |
| 构建 Release、同步官方上游 | [发布与维护](releasing.md) |
| 首次公开版本准备与验证范围 | [2026-11-03 发布准备记录](changelogs/UTAU_Editor_public_release_2026-11-03.md) |

使用指南说明当前交互；维护指南说明代码约定。更新日志记录每轮变化，不再作为查找当前功能的唯一入口。

## 专项参考

这些材料保留算法背景或可用资源，不承担现行交接职责：

- [数学符号输入表](../.docs/slider-toolbox-symbol-reference.md)与[字体图例](../.docs/slider_symbols/README.md)。
- [Toolbox 2 过程生成原理](../.docs/slider_toolbox_2_procedural_generation_report.md)：初版研究记录；现行时间模型与 UI 以使用指南为准。
- [高控制点 slider 性能分析](UTAU_Editor_slider_control_point_performance_analysis.md)：历史问题分析，当前已有 LOD 与命中采样缓存。
- [兼容导出行为分析](../.docs/原版编辑器导出行为.md)：历史调查，本轮未改变默认兼容导出算法。

## 历史记录

- [历次更新日志](changelogs/)保留各轮功能、修复和当时的验证结果。
- [已退役功能研究](archive/README.md)只用于追溯，不应重新实施其中已废弃的设计。
- 私人开发交接材料不包含在公开快照中。
- `.docs/` 中仍有符号图片等实际资源，因此保留目录并统一从本页索引。

已移除的 slider optimization 导出、旧单独 shape generator、`use full native length` 面板、bump 装饰不属于现行功能。不要依据旧日志恢复这些入口。
