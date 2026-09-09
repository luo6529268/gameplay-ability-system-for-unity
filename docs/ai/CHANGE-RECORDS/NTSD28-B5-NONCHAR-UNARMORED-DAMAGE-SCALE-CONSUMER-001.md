# NTSD28-B5-NONCHAR-UNARMORED-DAMAGE-SCALE-CONSUMER-001 — non-character unarmored scale consumer

<!-- CHANGE-RECORD
id: NTSD28-B5-NONCHAR-UNARMORED-DAMAGE-SCALE-CONSUMER-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_DAMAGE_CONSUMER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonCharUnarmoredDamageScaleEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: NTSD 2.8-Logan standard unarmored target type1..5 +340 then attacker weakness damage block and type6 skip; EXE B1E13AE1, closure 39DDDA15.
evidence: NONCHAR-DAMAGE-SCALE-OWNER-AUDIT-VERIFIED / TEST-FIRST-RUNTIME-RED-X5-TYPE6-GREEN / COMPILE0 / FOCUSED6 / RELATED290 / NTSD28-BROAD670 / FIRST-SELFCHECK-R4-HIT03-LEGACY-FIXTURE-CORRECTED / SELFCHECK-PASS-2026-09-05T14:37:47Z / CONSOLE0 / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / TYPE1_5_UNARMORED_SCALE_WEAK_ALIGNED / TYPE6_SKIP_VERIFIED`

迁weapon/type3/type5 normal及hit-plan projection；不触碰其他FallDamageDiv owners、producer、armor、
effect或resource。

test-first focused 5 red/1 green：weapon/type3/5旧damage owner被准确捕获，type6 skip基线通过。
首次SelfCheck按预期捕获R4-HIT-03旧`FallDamageDiv`fixture；只把已迁standard weapon夹具改为
`IncomingDamageScale340`，type6冲突值和其他未迁owner保持。

- weapon type1/2/4与type3/5 normal的HP/HPBound/combo/stat已迁`+340 -> weak/2`。
- raw weapon durability/display/status顺序保留；type6 damage/display skip保留；hit-plan projection同步。
- focused6、related290、精确NTSD28 broad670、14:37:47Z SelfCheck、Console/Scene/Ledger均通过。
