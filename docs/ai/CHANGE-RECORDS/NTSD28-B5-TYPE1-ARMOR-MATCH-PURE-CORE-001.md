# NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001 — exact type1 armor matcher

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-MATCH-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleType1ArmorMatchResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorMatchPureCoreEditorTests.cs
authority: NTSD 2.8-Logan armor_resolution.cpp ArmorResolver28::match_type1 and armor_resolution.h; EXE B1E13AE1, closure 39DDDA15.
evidence: red 53a9b74ae9044c42af8c525c3b85e414 24/24; focused 88e8a44a505f4655b1df40fdf61c23e3 25/25; B5 91d5c2685e1b4c5b8af01d913c4e31c7 304/304; broad 1fcf49be49744c3380f1523c3950412d 780/780; SelfCheck PASS 2026-09-06T00:36:52Z; Console no C# error; Scene unchanged; Ledger PASS.
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## 原状与边界

typed armor definition已经可加载，但Unity没有type1 matcher；production只能走null-armor reduced-hit。
本包只实现matcher truth table和版本化invalid-state常量，不做activation cost、selected damage接线、runtime
armor HP/recovery初始化或正式content部署。回滚删除新resolver/test及meta。

## 验收状态

新增allocation-free matcher及typed decision/reason，逐分支保持Authority kind gate、facing、严格阈值、
effect/id bypass、inclusive frame range、state OR和2.8.3.3 invalid-state fallback顺序。

验证：red `53a9b74ae9044c42af8c525c3b85e414` 24/24；focused
`88e8a44a505f4655b1df40fdf61c23e3` 25/25；B5 `91d5c2685e1b4c5b8af01d913c4e31c7`
304/304；broad `1fcf49be49744c3380f1523c3950412d` 780/780；00:36:52Z SelfCheck PASS；
Console仅既有故意失败日志，Scene未变。activation/production/content仍未接。
