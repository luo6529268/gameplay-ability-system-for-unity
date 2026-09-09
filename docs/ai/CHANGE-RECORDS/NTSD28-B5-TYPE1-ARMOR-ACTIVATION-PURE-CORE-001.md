# NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001 — exact armor activation cost

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE1-ARMOR-ACTIVATION-PURE-CORE-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleType1ArmorActivationResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type1ArmorActivationPureCoreEditorTests.cs
authority: NTSD 2.8-Logan armor_resolution.cpp ArmorResolver28::activation_cost and armor_resolution.h; EXE B1E13AE1, closure 39DDDA15.
evidence: red ad14998dedc7447683f2d909bb6ff5dc 11/11; focused 1d7f7267437449c28765c382793a8182 12/12; B5 0c471469bbc748ed97c55a9bb3e69ab3 316/316; broad 9b7705c6d72947e69a95e47b0b1dde61 792/792; SelfCheck PASS 2026-09-06T00:49:44Z; Console no C# error; Scene unchanged; Ledger PASS.
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## 原状与边界

type1 matcher已验证，但Unity尚无Authority activation cost/availability纯核心。现有runtime armor HP/recovery
carrier不等于激活逻辑。本包只建立纯结果，不做实体写回、production integration或content部署。
回滚删除新resolver/test及meta。

## 验收状态

新增allocation-free activation result/resolver，按Authority保持MP分支优先、decrease→mp两级64位
absolute-or-percent换算、最低1、exact MP boundary、runtime armor HP缺失与`<= effectiveInjury`破甲`-1`。

验证：red `ad14998dedc7447683f2d909bb6ff5dc` 11/11；focused
`1d7f7267437449c28765c382793a8182` 12/12；B5 `0c471469bbc748ed97c55a9bb3e69ab3`
316/316；broad `9b7705c6d72947e69a95e47b0b1dde61` 792/792；00:49:44Z SelfCheck PASS；
Console仅既有故意失败日志，Scene未变。production/content仍未接。
