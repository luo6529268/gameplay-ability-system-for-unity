# NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-CPOINT-KIND2-HURT-ACTION-CONSUMER-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan playable CatchPointRecord28 consumers and ordinary hit writers; Unity LF2HitResolveRuntimeData, BattleDamageWriter, BattleEcsHitExecutionPlan, legacy character hit resolvers and old alignment tests; EXE B1E13AE1, closure 39DDDA15.
evidence: playable closure has no CPoint front/back hurt-action consumer outside decoder; Unity shared helper plus two BattleDamageWriter calls, HitPlan projection, legacy DAT resolver and dead fallback inventoried; current 4 and release 398 front/back rows are all kind2 with no explicit injury/cover; current OID300 criminal catching341/caught130 can pair with OID33 Naruto clone action130 front132/back131, providing static third-hit witness; one atomic actual+shadow+legacy retirement package and focused matrix defined; runtime witness pending; production held; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / UNITY_ONLY_HURT_OVERRIDE_CONFIRMED / ATOMIC_ACTUAL_SHADOW_LEGACY_RETIREMENT_DEFINED / CURRENT_CORPUS_WITNESS / PRODUCTION_HELD`

当前2.8 playable只解码CPoint front/back hurt fields，从未在普通命中后消费它们；Unity则通过旧alias在
BattleDamageWriter、HitPlan、legacy DAT resolver和dead fallback中覆盖target action。Direction-B已有
OID300 criminal→OID33 Naruto clone action130的有效关系组合，第三方nonknockdown hit可让Unity额外写132/131。

后续单一production包必须同时退休default actual、shadow与legacy调用并替换旧5-test alias action断言；不得改读
FrontHurt/BackHurt来保留无Authority行为。converter alias和19→27 schema留给随后versioned schema包；当前
runtime栈未清，本轮无脚本、content或Scene修改。

## CORPUS CORRECTION（2026-09-08）

current front/back witness由4扩大为170，kind3 relation也非OID300唯一。consumer absence与atomic retirement
owner不变；完整修订见`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`。
