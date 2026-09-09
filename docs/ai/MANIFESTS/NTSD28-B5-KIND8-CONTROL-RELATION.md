# NTSD28 B5 kind-8 control relation

| 层 | Authority | Unity 当前 | 状态 |
|---|---|---|---|
| target type | `bdefend`: 0..6 exact；7=1/2/4/6；8=all | kind8与kind3一起仅允许type0 | CONFIRMED_DIFFERENCE |
| relation | `respond`: 0 unconditional；1 same group；2 different；3 same group+owner；4再要求owner=mode | candidate无对应classifier | CONFIRMED_DIFFERENCE |
| defensive consume gate | consumer重复同一classifier | actual/HitPlan未复核 | CONFIRMED_DIFFERENCE |
| heal | injury非0时写`injury+1000` | 无条件写 | CONFIRMED_DIFFERENCE |
| MP | caughtact非0直接加current MP | 缺失 | CONFIRMED_DIFFERENCE |
| action | dvx非999时写attacker action | 无条件写dvx | CONFIRMED_DIFFERENCE |
| position | dvy归一化到-1/0/1/2；按mode同步precise X/Y/Z；int保持 | 固定同步X/Z且立即写int | CONFIRMED_DIFFERENCE |
| owner split | single native relation consumer | character/shared/special/weapon与HitPlan多入口 | ATOMIC_INTEGRATION_REQUIRED |

实施顺序：pure eligibility core → production-owner/write-surface audit → candidate+consumer actual+HitPlan原子接线。
respond-4所需battle mode必须读取已验证的world carrier，不得把local menu mode或默认0混入。

`NTSD28-B5-KIND8-ELIGIBILITY-PURE-CORE-001 / VERIFIED / PURE_CORE_READY /
PRODUCTION_UNCONNECTED`已闭合完整selector真值表与4096次zero-allocation；下一production owner/write-surface audit。

`NTSD28-B5-KIND8-PRODUCTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY /
ATOMIC_INTEGRATION_READY`确认所有字段已被HitPlan snapshot观察；下一同包接candidate、shared actual与HitPlan。

`NTSD28-B5-KIND8-ATOMIC-PRODUCTION-INTEGRATION-001 / VERIFIED /
KIND8_CONTROL_RELATION_ALIGNED`已原子接三层；conditional heal/PP/action、dvy precise轴选择与int保持均闭合。
red14、focused14、HitPlan184、collision258、B5 420、clean broad885、SelfCheck/Console/Scene/Ledger通过。

`NTSD28-B5-KIND8-EXIT-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY /
KIND8_SPECIFIC_FAMILY_EXIT_READY`确认无第二production owner或kind8-specific首差；后续回到其他candidate family。
