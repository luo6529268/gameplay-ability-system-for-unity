<!-- OPTIMIZATION-REVIEW-20261009 -->
# M-13 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | CPU / 低 |
| 当前状态 | 选定评估结束／不实施；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 仅真实cue热点新证据才重开；若实施沿用H-10容量合同 |
| 下一步与具体方向 | 现有评估未建立真实热点，冻结原算法；不因二次复杂度描述直接改 |
| 完成边界 | 评估结论复用；新候选须有收益且cue顺序/增益/retrigger不变 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

**历史状态更正：** 下方“尚未测量”属于原方案时点；选定热点资格评估已完成，未建立值得实施的真实热点，保留算法。

## 原方案与历史证据（原始正文保留）

> 2026-10-07 首阶段评估完成 / NO_IMPLEMENTATION_THIS_PHASE：第25批1000事件unique1/8/128 synthetic sink均值约0.28/2.1/21.6ms，比较1999/12464/182996；含voice替换，不是纯扫描成本或实际混战。H07唯一实际窗声音0，真实多cue热点未建立；不实施算法，voice64/Ordinal/tick/mix不改。四语义检查通过。详[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH25-BOUNDED-ASSESSMENTS-20261007/REPORT.md)，旧等待算法批准仍有效。

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
