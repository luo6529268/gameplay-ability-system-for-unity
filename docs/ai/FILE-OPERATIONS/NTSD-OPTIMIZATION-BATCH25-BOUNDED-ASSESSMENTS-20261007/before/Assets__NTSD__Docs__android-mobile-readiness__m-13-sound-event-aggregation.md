# M-13 同 Tick 声音聚合重复扫描优化方案

> 优先级：中
> 状态：`OPEN / HOTSPOT_CANDIDATE / PROFILING_REQUIRED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 主登记表：[优化风险登记表](../android-mobile-readiness-priority-risk-register.md)
> 共同合同与启动门：[本轮复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)
> 本文为待批方案；下面的测试全部是未来验收条件，本轮未执行。

## 当前事实与成本假设

[已验证-代码] `NTSDSoundPlayer.PresentSounds(:278-313)`按Tick分组，
对每条事件扫描此前cue，再对首次cue扫描剩余贡献；大量互异cue时最坏接近二次复杂度。
同cue重复时不必达到该最坏情况；本项是热点候选，尚未实测为瓶颈。

## 待批解决方案

1. 先统计每tick事件数、唯一cue数、比较次数、聚合耗时和voice/retrigger计数。
2. 如成本成立，复用预分配cue-id/索引表、generation标记和首次出现顺序数组，
   以有界单遍或等价分组减少重复扫描；输入仍只读，不改Core声音事件。
3. 保持Ordinal cue相等语义、Tick边界、首次cue消费顺序、每事件原来的声道计算、
   整数累加/溢出语义、同cue voice重触发和原voice上限/淘汰行为。
4. 容量在加载时按保守闭包预热并seal；超限遵循获批拒绝合同，不静默丢事件、不热扩容。
   新缓存纳入H-10预算和11阶段关闭owner声明。

## 验收与未来测试

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 聚合等价 | 空/单/重复/互异cue、连续多个tick | 输出顺序、声道mix、触发次数与canonical一致 |
| 数值边界 | 增益/位置边界、大量重复事件 | 与既有整数及声道语义一致，first difference即停 |
| 性能 | 1000实体混战、同cue与多cue压力 | 同指纹比较次数/耗时改善，完整呈现热路径0GC |
| 容量 | 上限/上限+1、未预热cue | 无扩容/部分吞事件，明确reason |
| 生命周期 | 退出/重进/旧generation | 无缓存污染、旧voice、资源残留 |

模拟checksum/声音事件序列不变。无热点收益或等价性失败则不采用，保留现有canonical聚合。
2026-10-06登记；尚未批准算法、未测量、未改声音行为。
