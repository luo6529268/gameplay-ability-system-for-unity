# NTSD28 B5 hit resource / armor / effect split

| 子项 | 当前前置 | 状态/下一步 |
|---|---|---|
| positive injury display steps | 4 runtime carriers已存在；位于local gate前 | VERIFIED |
| resource injury arithmetic | pure core与stats.attacking carrier已闭合；完整production仍等baseMax/mode/suppression | PURE_CORE_VERIFIED_PRODUCTION_BLOCKED |
| injury MP reward/drain/gain | pure transaction已闭合；mode 34/38、suppression缺失 | PURE_CORE_VERIFIED_CARRIER_REQUIRED |
| suppression 15C | carrier/schema闭合；authority child literal1继承/type0强制1尚未接 | CARRIER_VERIFIED_PRODUCER_B7 |
| world numeric rules | 1C默认0、34/38默认75/75；reset/snapshot/restore/checksum/parity闭合；selected-mode override未映射 | CARRIER_VERIFIED_MODE_OVERRIDE_B8_H |
| F6 local gate | FunctionKeys唯一gate；accepted active projection与registration inheritance已闭合 | PROJECTION_VERIFIED |
| resource attacker | `OwnerSlotIndex`两跳，negative/self terminal，missing next失败 | RESOLVER_VERIFIED |
| definition attacking/baseMax | stats.attacking typed carrier已闭合；MPMax仍有内容500/200差异 | ATTACKING_READY_BASEMAX_H_B11_BLOCKED |
| armor selection/reduced/break | data/runtime/selection/reduced/fallback/break已闭合；正式内容值仍归H/B11 | RULES_VERIFIED_CONTENT_H_B11 |
| unarmored HP consumption +0x34C | type0/1/2/3/4/5 actual+HitPlan按effective injury写；type6跳过 | VERIFIED |
| weapon strength self-cost | WPoint attacking线索有；formal strength schema未闭 | H_B11_BLOCKED |
| effect action/direct effects | owner/matrix已审计；eligibility/post-action/override/audio/spark需独立分包 | AUDIT_VERIFIED_SPLIT_REQUIRED |
| defend/weakness damage scale | type0..5 standard闭合；remaining FallDamageDiv owners已路由B4/B5/B6/B8/H | STANDARD_VERIFIED_REMAINING_ROUTED |

Production readiness结论：现有primitive已ready，但完整接入仍被definition attacking/baseMax、selected-mode
override、child suppression producer、selected armor与cpoint调用面阻塞；不得局部接入并以默认0替代缺失语义。

## 2026-09-06 readiness resume note

`stats.attacking` typed carrier与type1 armor-specific production现已闭合；旧readiness结论中的这两个阻塞已解除。
下一`NTSD28-B5-HIT-RESOURCE-PRODUCTION-READINESS-AUDIT-002`必须重新核对baseMax、selected-mode override、
child suppression producer、weapon-strength/cpoint与actual+HitPlan写面，不能直接沿用旧阻塞矩阵。

## Production readiness audit 002

`NTSD28-B5-HIT-RESOURCE-PRODUCTION-READINESS-AUDIT-002 / VERIFIED / GOVERNANCE_ONLY /
THREE_BLOCKERS_REMAIN / HP_CONSUMPTION_READY`确认stats/type1已解除；完整MP transaction仍等baseMax、mode
override、child suppression，cpoint/weapon caller仍独立。unarmored/type0 fallback的+0x34C写入可先实施。

## Unarmored HP consumption production

`NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001 / VERIFIED /
UNARMORED_HP_CONSUMPTION_ALIGNED / FULL_RESOURCE_BLOCKED`已把type0/1/2/3/4/5 standard actual+HitPlan
接到effective HP injury的`+0x34C`累计，type6保持跳过；red13/15、focused15、HitPlan184、B5 371、
broad836、SelfCheck/Console/Scene/diff/Ledger均通过。此结论不解除完整MP transaction的三项阻塞。
