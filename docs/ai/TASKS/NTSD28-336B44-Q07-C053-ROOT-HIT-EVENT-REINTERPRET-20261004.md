# NTSD28-336B44-Q07-C053-ROOT-HIT-EVENT-REINTERPRET-20261004

状态：`VERIFIED_SCOPED_ROOT_HIT_EVENT_OBSERVATION`；C053/Q07/总目标仍开放。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q07/C053。

目的：核对现存正式根336B44的 C053 自然双 producer LFR 回放 trace 是否实际导出逐 hit 事件。旧报告声称根 trace 不公开逐 hit，但当前原件第7 tick 有 `events.kind=hit`，因此先做只读结构化对照，纠正文档可能过窄的证据边界。正式根程序与其对应 playable 源码、正式 DAT 为权威；Unity仍是实现目标。

准确输入：`artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-PRODUCER-001/source-run-y0/ank610-y0-jira500.csv`、`ank610-root-v2-trace.jsonl`、`ank610-root-v2-report.json` 和对应正式根身份。核对每 tick 的 root hit 序列、slot51→50 的 applied/candidate/HP 以及源码 `oid808_hit_sequence`、action、latch；不得把根 trace 未给出的 effect/Uj 直接当作根观测，也不把单 Uj 样本升级为双 Uj 阳性。

范围与保护：仅新增本包分析 JSON/报告及当前进度文档的追加更正，不改工具源码、战斗脚本、DAT、图片、Unity Scene/Asset、正式 EXE 或非战斗代码；不重复运行已有40tick LFR或原Scene。验收为可复算的结构化比较、当前正式 EXE SHA 复核、明确剩余的双命中/Unity逐hit/物理键门。若旧报告有错，只追加更正并链接原件，不删除或覆盖旧事实。失败则保留原件与原因，不提升 C053/Q07 状态。

结果：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-ROOT-HIT-EVENT-REINTERPRET-20261004/REPORT.md)；正式源CSV与根trace的slot51→50命中计数40/40 tick相同，唯一阳性tick7根事件`applied/candidate0/HP25`，目标后态OID808/action156/HP475；源事件另有`effect2/Uj156`，根事件本身不导出这两字段。纠正旧“根不公开逐hit”的过窄说法，不宣称双Uj或整个C053完成。
