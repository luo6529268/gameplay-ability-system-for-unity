# NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001 — type3 target generic continuation

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE3-TARGET-GENERIC-CONTINUATION-001
status: VERIFIED
change-kind: TEST_FIRST_AUTHORITY_BEHAVIOR_PORT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan resolve_relation_hit type3 generic continuation at 0x0042F72B..0x0042FC78; EXE B1E13AE1, closure 39DDDA15.
evidence: PREREQUISITE-AUDIT-VERIFIED / RUNTIME-CARRIERS-READY / TEST-FIRST-a9c51fe81f1f42db97855f208b251133-7FAIL-4PASS / COMPILE0 / FOCUSED-35a8c525a12e4526829de33274fb2c32-11OF11 / HITPLAN-fa3927cd838344d9ab414a2338e45e31-182OF182 / B5-HITPLAN-49e59345777646cfbe5c70598cf3330c-359OF359 / EXACT100-BROAD-30500081d6a3436b88dfd18481b6cf60-735OF735 / SELFCHECK-PASS-20260905T182912Z / FILTERED-ERROR-CS0 / EXPECTED-TEST-ERROR-LOGS7 / LEDGER268-233-PASS / SCENE-D4266C6D-UNCHANGED
-->

> 状态：`VERIFIED / TYPE3_TARGET_GENERIC_CONTINUATION_ALIGNED`

## Authority 与 Unity 原状

Authority只在response frame存在且state!=3005时执行generic transaction；按active parent决定归属源，复制
battle_group/owner_slot/control_slot，清pending total但保留count，再选hit_Fj/30或hit_Uj/20。

Unity原先固定20/30，只复制team/HolderCopySlot，并清Runtime XYZ与AttackingCounter；hit-plan同样缺TargetOwnerSlot。
locked kind candidate的旧OID route保持后置，不在本Change宣称对齐。

## 验收状态

- `ApplyNativeType3TargetGenericContinuation` 已按 direct/active negative-link parent 解析关系源，并原子写入
  `RelationTeam`、`OwnerSlotIndex`、`AnimCounter`、`HitConfirm2`；`HolderCopySlot` 不再被误作 owner。
- state3005 精确跳过；state3006 保持可达。目标当前 response frame 在 action 写入前读取，按 attacker type/link/effect
  选择 `hit_Fj`/`hit_Uj`，且仅零值回退 30/20。
- 只清 `KnockbackVx/Vy/Vz`，保留 `HitCount` 与 `Runtime.Vx/Vy/Vz`；action 写入后清
  `AttackingCounter`。actual 与 HitPlan 增加 `TargetOwnerSlot` 后结果一致。
- state1002/2000 的 kind9 在候选预处理阶段转成 kind0，因此走 generic continuation；真正未转换 kind9 与
  locked kind/OID transform 仍保持原分支并明确后置。
- red `a9c51fe81f1f42db97855f208b251133`：11项中7 fail/4 pass；final focused
  `35a8c525a12e4526829de33274fb2c32` 11/11；HitPlan
  `fa3927cd838344d9ab414a2338e45e31` 182/182；B5+HitPlan
  `49e59345777646cfbe5c70598cf3330c` 359/359；exact100 broad
  `30500081d6a3436b88dfd18481b6cf60` 735/735。
- fresh compile：`Assembly-CSharp.dll` 2026-09-05T18:27:56Z、`Assembly-CSharp-Editor.dll`
  2026-09-05T18:23:23Z；SelfCheck 2026-09-05T18:29:12Z PASS；filtered `error CS`=0。
- Console 7条 error-type 均为既有注册回滚/绑定拒绝预期测试日志；Ledger 268 records / 233 governed code files
  PASS；Scene `D4266C6D...583B`、长度205625及mtime保持并发基线不变。

## 2026-09-08 correction

原exact group/owner/control、impulse、action和motion证据保留；但
`BattleDamageWriter.CopyRelation()`和HitPlan仍额外传播旧`HolderCopySlot`，Authority type3
transaction没有此写入。因此whole-continuation声明已收窄，待
`NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001`删除actual/HitPlan extra write并重跑验收。

## 2026-09-09 correction closure

`NTSD28-B5-TYPE3-LEGACY-HOLDERCOPY-WRITER-RETIREMENT-001`已按Authority退休actual一处与HitPlan三处
extra HolderCopy write，并取得RED0/3、focused4/4、related284/284、双build 0 error和真实Play/Scene证据。
因此本记录原先收窄的whole-continuation声明恢复；不扩展为legacy stats、carrier或schema结论。
