# Task Contract — NTSD28-B5-NONTYPE0-ENCODED-JOIN-001

> 状态：`VERIFIED / TYPE1_5_ENCODED_JOIN_ALIGNED / TYPE6_SKIP_VERIFIED`
> 依赖：`NTSD28-B5-OTHER-TARGET-PRODUCER-AUDIT-001 / VERIFIED`

## 目标

在 confirmed unarmored standard-hit 的现有 Unity owners 中，为target type1/2/3/4/5接入已验证
的encoded status与join即时副作用；精确保持type6跳过该tail。

## Authority 合同

- type1/2/3/4/5：damage/resource之后执行`apply_confirmed_input_statuses`，再执行join/mimic；
  非type0下mimic gate必定不激活，但encoded mimic counter仍写入。
- type6：跳过整个damage/resource/status/join/mimic/weapon-strength tail，不消费status RNG。
- weapon owner：`ApplyWeaponDamage`；type3/5 owner：
  `ApplySpecialAttackDamage -> ApplySpecialObjectHurtTail`。

## 不变量

- 复用既有producer/helper，不复制编码/RNG或join算法。
- 不在本包接arm；arm紧随本包之后单独实施。
- 不改Config/DAT/Scene/Authority/资源/snapshot schema或C25 expiry owner。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5NonType0EncodedJoinEditorTests.cs`

## 验收

test-first red；覆盖type1/2/4 weapon、type3/5 special/other、type6 zero-RNG skip、non-type0
mimic gate；相关B2/B3/B5、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除两个owner中的producer/helper调用和focused test；保留既有type0实现。

## 验证结论

- test-first behavior red 11/11；type6一条越界group断言已在实现前移除，未改生产行为。
- fresh compile 0 error；focused `c47b7c21482543288e0ea27e5f26f0c6` 11/11。
- B2/B3/B5与完整hit related `7a299fb7133b466c807eaf15f8b94fe2` 233/233。
- 精确NTSD28 broad `66c9e6db907d4828801608b82a86df8f` 598/598。
- BattleRuntimeSelfCheck `2026-09-05T11:30:59Z` PASS；Scene unchanged。
