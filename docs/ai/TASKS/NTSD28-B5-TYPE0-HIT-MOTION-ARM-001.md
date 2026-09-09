# Task Contract — NTSD28-B5-TYPE0-HIT-MOTION-ARM-001

> 状态：`VERIFIED / TYPE0_HIT_MOTION_ARM_ALIGNED / OTHER_TYPES_DEFERRED`
> 依赖：`NTSD28-B5-TYPE0-ENCODED-STATUS-001 / VERIFIED`

## 目标

在 `BattleDamageWriter.ApplyStandardCharacterDamage` 的 type-0 confirmed unarmored
路径接入权威 `arm_native_unarmored_hit_motion`，保持其 bypass、写入顺序和 pending-Z
累加语义。

## Authority 合同

- authority：当前正式 NTSD 2.8-Logan `battle_world.cpp` 的
  `arm_native_unarmored_hit_motion`，该文件进入 playable build closure。
- 调用位置：unarmored reaction 之后、ordinary horizontal response 之前。
- bypass：`Fall == 80 && Vx > -5 && Vx < 5 && itr.dvx == 0`，边界 `-5/+5`
  不 bypass；bypass 不写任何 arm 字段。
- armed：`KnockbackVz += itr.dvz`；写 attacker facing、`dx/dy/dz`，强制 `gain=1`；
  `pickedact/pickingact` 的零值默认分别为 `191/185`。
- 本包只接 type 0；join/mimic immediate side effects、其他 target type 与 B8 event 后置。

## 不变量

- 使用逻辑 runtime，不读取/写回 Transform、Animator 或渲染状态。
- 不改 Config、DAT、Scene、Authority、资源或 snapshot schema。
- 不改变 encoded status RNG 次数与顺序；armed 写入发生在 encoded producer 之后。
- 保持现有命中 rest、伤害、音效和水平响应职责。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type0HitMotionArmEditorTests.cs`

## 验收

- test-first compile red。
- focused：strict bypass、`-5/+5` 边界、其他 gate、累加/覆写/default/explicit、
  production order。
- 相关 B5/hit/RNG、精确 NTSD28 broad、BattleRuntimeSelfCheck。
- Unity compile 0 error、Console 0 error、Scene hash/mtime/size unchanged、Change Ledger PASS。

## 回滚

移除 arm helper、production 调用和 focused test；保留既有 carrier 与 encoded producer。

## 验证结论

- test-first fresh compile red：5个预期 `CS0117`。
- fresh compile 0 error；focused `a4887c8f3e654ad09ccc5d165739eeea` 10/10。
- carrier/consumer/hit related `943c18ddb29d44e0a57c82a343209706` 34/34。
- 精确 NTSD28 broad `9982f92979224752ba45eb77db7a09bd` 577/577。
- BattleRuntimeSelfCheck `2026-09-05T11:00:36Z` PASS；Scene unchanged。
- 首轮focused唯一失败是夹具误断言既有致死HP必须钳0；收窄为`HP <= 0`后通过，
  未改变伤害逻辑。
