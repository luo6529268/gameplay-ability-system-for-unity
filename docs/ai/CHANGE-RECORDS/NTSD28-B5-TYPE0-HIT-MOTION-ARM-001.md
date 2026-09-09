# NTSD28-B5-TYPE0-HIT-MOTION-ARM-001 — type0 unarmored hit-motion arm

<!-- CHANGE-RECORD
id: NTSD28-B5-TYPE0-HIT-MOTION-ARM-001
status: VERIFIED
change-kind: TEST_FIRST_HIT_MOTION_PRODUCER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0HitMotionArmEditorTests.cs
authority: NTSD 2.8-Logan arm_native_unarmored_hit_motion in confirmed unarmored hit live path; EXE B1E13AE1, closure 39DDDA15.
evidence: AUTHORITY-SOURCE-AND-CALL-ORDER-READ / UNITY-CARRIER-OWNER-READ / TEST-FIRST-COMPILE-RED-CS0117-X5 / COMPILE0 / FOCUSED10 / RELATED34 / NTSD28-BROAD577 / SELFCHECK-PASS-2026-09-05T11:00:36Z / SCENE-UNCHANGED
-->

> 状态：`VERIFIED / TYPE0_HIT_MOTION_ARM_ALIGNED / OTHER_TYPES_DEFERRED`

## 改前事实

- type0 encoded status producer 已在 damage/resource 后、reaction 前运行。
- Unity 尚未实现紧随 reaction 后的 unarmored hit-motion arm。
- `Fall`、逻辑 `Vx`、`KnockbackVz` 及七个 status-motion carrier 已存在并纳入确定性状态。

## 预期实施

- 新增无分配的 pure arm helper，精确实现 strict bypass 与 armed writes。
- 在 `ApplyStandardCharacterDamage` 的 reaction 后、horizontal response 前调用。
- 添加 focused Editor tests；不修改内容、Scene、Authority 或其他 target type。

## 验证状态

focused test 已先写入，fresh compile 得到5个预期 `CS0117` 缺
`ArmNativeUnarmoredHitMotion`。最小生产实现后compile 0 error；首轮focused仅有测试夹具
把既有致死HP误断言为必须钳0（实际为-5），已收窄为本包所需的`HP <= 0`，未改伤害行为。

- pure helper精确实现`Fall==80 && -5<Vx<5 && dvx==0`的strict bypass；`-5/+5`
  边界仍arm。
- armed路径累加`KnockbackVz`，覆盖attacker facing、dx/dy/dz、gain=1和默认/显式动作。
- production调用位于reaction与hurt sound之后、horizontal response之前；encoded producer先于arm，
  production test验证arm覆盖encoded gain，death stabilization验证bypass保留encoded/旧值。
- fresh compile 0 error；focused `a4887c8f3e654ad09ccc5d165739eeea` 10/10；
  related `943c18ddb29d44e0a57c82a343209706` 34/34；精确NTSD28 broad
  `9982f92979224752ba45eb77db7a09bd` 577/577。
- BattleRuntimeSelfCheck `2026-09-05T11:00:36Z` PASS；Scene SHA
  `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、203477 bytes、
  mtime unchanged。其他target types、join/mimic immediate side effects与B8 event仍后置。
