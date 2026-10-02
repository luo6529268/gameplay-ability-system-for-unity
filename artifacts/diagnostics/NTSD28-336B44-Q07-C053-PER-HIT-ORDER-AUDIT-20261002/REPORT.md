# Q07/C053 自然双命中逐候选顺序复核

状态：`VERIFIED_SELECTED_CANDIDATE_ORDER / PER_HIT_ACTION_PENDING`。本报告只复用已保存的正式源码和原 Unity Battle Scene 证据；没有重跑、改动战斗脚本、DAT、资源或场景。

权威仍是根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 playable live path。三名受控初始角色为 slot0/2 OID65、action511、源 X580，slot1 OID702、action553、源 X500，seed682973786、mode0、difficulty0；后续输入中性。875/808 是正式资源经完整 tick 的 OPoint 自然生成，但初始动作不是物理按键选出。

| 相对 tick7 的同条件观测 | 正式 playable 源码 | 原 Unity Battle Scene |
| --- | --- | --- |
| 攻击者与目标槽位顺序 | `51:0:2:156;52:0:2:156`，两条 applied/effect2，均记录目标 post-hit action156 | 两条 plan entry 按 slot51→slot52、target slot50 排列，两个 observed disposition 均为 `Damage` |
| 命中执行观测 | 源码 `oid808_applied_uj=2` | `observedDispositionCount=2`、`observedConsumeEffectsCount=2`、`observedWriterEffectCount=2`；两条 consume 预测/实际指纹分别一致 |
| tick 后子体 | OID808 action156、锁存156、HP450 | slot50 action156、锁存156、HP450；两个攻击者各自 victim-rest10 |
| 计划诊断 | 源码双跑 CSV 逐字节同 SHA | `hitPlanValid=true`、mismatch/failure 0、consume/first-body/writer/lifecycle 差异掩码均 0 |

原件：[正式源码 CSV](../NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/source-run-01.csv) 的 X580/580/tick7 行；[Unity Scene 结果](../NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/ank580-ank580-jira500-shadow-sound-green-01.json) 的 `ticks.relativeTick=7` 行；[132 个战斗字段及 42 个待播音效字段对照](../NTSD28-336B44-Q07-C053-SHADOW-SOUND-COORDINATE-001/source-unity-comparison.json)。

可确认的新增边界是：同一个自然双攻击样本的**候选顺序、两次 Damage 决策及消费观测**与正式源码的两条 applied 命中相容，选定 tick 末态也一致。Unity 现有 entry view 只导出 disposition、consume 指纹及汇总 writer 计数，**没有逐 entry 导出目标 post-hit action、HP 或锁存动作**；因此不能从最终 action156/HP450、writer 零差倒推出「两次各自 Uj156」已被直接观测。正式根 CLI 也不能覆盖第三初始槽的 action511，不能把不等价根回放算作三人同条件证书。下一出口应在现有 hit plan 只读诊断记录每次 writer 后的目标 action/锁存/HP，再在同一原 Scene 样本与源码两条记录逐项比较；物理键自然选招、完整 World 与整场验收另列，不反复重跑已有末态案例。

C053、Q07、总目标均继续开放。
