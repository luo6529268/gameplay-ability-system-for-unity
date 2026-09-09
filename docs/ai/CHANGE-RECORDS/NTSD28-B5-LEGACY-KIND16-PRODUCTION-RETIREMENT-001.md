# NTSD28-B5-LEGACY-KIND16-PRODUCTION-RETIREMENT-001 — retire legacy itr.kind16

<!-- CHANGE-RECORD
id: NTSD28-B5-LEGACY-KIND16-PRODUCTION-RETIREMENT-001
status: VERIFIED
change-kind: TEST_FIRST_LEGACY_PRODUCTION_RETIREMENT
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5LegacyKind16RetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan resolve_relation_hit kind15 then unsupported fallthrough, with effect16 independently classified; EXE B1E13AE1, closure 39DDDA15.
evidence: REMAINING-FALLDAMAGEDIV-AUDIT-VERIFIED / AUTHORITY-NO-KIND16-DISPATCH-READ / EFFECT16-DISAMBIGUATED / TEST-FIRST-RED-855a8951bd8d445789c89db993d23891-4OF4-EXPECTED-FAIL / COMPILE-0 / FOCUSED-382df2f7b2fd44eba3c6031a853dab82-5OF5 / HITPLAN-0a8d625400e7423ab857255991981dd5-178OF178 / B5-7ddd31eb8f034d66a477182f7eefc9bf-117OF117 / EXACT96-BROAD-bd0f8318547445588949542629aaf7ab-675OF675 / SELFCHECK-PASS-20260905T150748Z / CONSOLE0 / SCENE-UNCHANGED / LEDGER260-229-PASS
-->

> 状态：`VERIFIED / KIND16_UNSUPPORTED / KIND15_EFFECT16_PRESERVED`

统一退役itr.kind16生产行为；kind15和effect16保持，不涉及content/armor/resource/audio实现扩张。

首个focused test job `855a8951bd8d445789c89db993d23891`完成4项并全部按预期失败，确认旧行为仍由
disposition、actual/shared character与generic weapon四条入口生产。代码搜索进一步发现具体
`LF2Weapon.Hit`与sequence writer-observation也引用合并的Kind15Or16，已在改生产脚本前纳入本Record。

## 实际改动

- disposition枚举拆为`Kind15`；`kind16`统一落入`Unsupported`。
- actual/shared character、actual/generic weapon不再消费`kind16`，无HP/统计/帧/rest/link/运动/RNG/sound副作用。
- hit-plan删除kind16 writer projection；writer observation只追踪kind15。
- `kind15`角色/武器分离和`kind0/effect16`damage分类均保留。

## 验证

- test-first：`855a8951bd8d445789c89db993d23891`，4/4按预期失败。
- 编译：0 error。
- focused：`382df2f7b2fd44eba3c6031a853dab82`，5/5。
- hit-plan：`0a8d625400e7423ab857255991981dd5`，178/178。
- B5集合：`7ddd31eb8f034d66a477182f7eefc9bf`，117/117。
- 精确NTSD28 broad：96 classes，`bd0f8318547445588949542629aaf7ab`，675/675。
- SelfCheck：2026-09-05T15:07:48Z PASS；Console清空后0 error。
- Scene：SHA `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、
  203477 bytes、mtime `2026-09-04T13:12:45.1526434Z`，不变。
- Change Ledger：260 records、229 governed code files，PASS；`git diff --check`仅既有CRLF警告。

## 未扩张边界

本包不处理armor catalog、effect eligibility/post-action、资源数值、audio/spark或B6/B8/B11职责。
