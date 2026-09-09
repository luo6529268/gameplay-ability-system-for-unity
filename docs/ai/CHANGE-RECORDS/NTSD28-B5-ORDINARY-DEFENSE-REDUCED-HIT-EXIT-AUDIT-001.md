# NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-ORDINARY-DEFENSE-REDUCED-HIT-EXIT-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan defense_resolution.cpp, damage_resolution.cpp and battle_world.cpp 7023-7269; EXE B1E13AE1, closure 39DDDA15.
evidence: CALLER-AUDIT / LEGACY-HEURISTIC-ABSENT / INTEGRATION-RED3-FOCUSED3-B5-272-HITPLAN184-BROAD748-SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NULL_ARMOR_FAMILY_EXIT_READY`

actual两入口、HitPlan四个selection/plan consumer及actual/projection各一组damage/rest pure owner已闭合；旧OID37/6/52、
Prev2 selection与FallDamageDiv/weak damage均不在production null-armor路径。残留Prev2只服务defend-break reaction tail。

本退出仅覆盖ordinary defense与selected armor=null；type1 armor model/activation/runtime/content仍未实现。下一
`NTSD28-B5-TYPE1-ARMOR-DATA-CONTRACT-001`，只补typed model/parser/copy/fingerprint，不部署content。
