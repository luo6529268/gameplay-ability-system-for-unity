# NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-KIND2-PICKUP-RELATION-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan kind2 candidate gate and resolve_special_relation_hit holder/object branch with weapon_throw table; Unity BattleInteractionWriter, HitPlan and legacy pickup paths; EXE B1E13AE1, closure 39DDDA15.
evidence: exact type1/2/4/6 relation/action/owner/group/count/WPoint tail order frozen; runtime weapon_throw table 120/124; Unity kind2 misses OID120 promotion, target OwnerSlot, conditional relation count and WPoint/unsupported tail while kind7 incorrectly picks up; corpus kind2/kind7 current 1/0 and release 375/0; indexed candidate-state frames current17/release53 all supported types and no WPoint, current formal Naruto clone frame65 to OID120 frame64 witness; carrier+system rules, pure core and atomic actual/HitPlan/legacy packages defined; production held; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / CURRENT_OID120_WITNESS / KIND7_DORMANT_RETIREMENT / PRODUCTION_HELD`

Authority kind2按type建立relation，type1 OID120因`weapon_throw={120,124}`写holder101/child-1，并写target
OwnerSlot；+35C仅在旧relation0时加1，最后统一reset frame counter并读target WPoint覆盖action。Unity kind2漏这些
语义，却把120/124和无按键pickup错误放在两端无语料的kind7分支。

current/release kind2 ITR为1/375、kind7均0；current Naruto clone frame65→OID120 frame64是正式首差，17/53个
indexed candidate-state frames均无WPoint。后续carrier/system rules→pure core→atomic actual/HitPlan/legacy三包；
runtime栈未清，本轮无脚本、content或Scene修改。

## CORRECTION（2026-09-08）

Direction-B kind2/target corpus数字与“唯一”措辞由
`NTSD28-B6-KIND2-PICKUP-CORPUS-CORRECTION-001`纠正；本记录的Authority规则、Unity首差、owner与三包拆分
继续有效。current正确值为117 kind2 tuples / 39 definitions / 31 supported target frames；release375/53不变。
