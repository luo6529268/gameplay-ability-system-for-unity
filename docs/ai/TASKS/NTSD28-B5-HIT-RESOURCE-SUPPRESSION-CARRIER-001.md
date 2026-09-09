# Task Contract — NTSD28-B5-HIT-RESOURCE-SUPPRESSION-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / PRODUCER_DEFERRED`
> 依赖：`NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001 / VERIFIED`

## 目标

新增`HitResourceSuppression15C`确定性实体载体，闭合default/full-reset、canonical copy、entity
snapshot、checksum与full parity；本包不接child producer。

## Authority 合同

- spawn/default为0。
- hit-resource reward仅在physical attack-effect source该值不等于literal 1时允许。
- special child传播/强制1属于B7/C25b producer，当前只建carrier。
- `ResetInputState`不得清除；完整`Reset`清0。

## Schema

- entity runtime snapshot `8 -> 9`。
- aggregate battle snapshot `12 -> 13`。
- lockstep checksum `15 -> 16`。
- full parity的`nativeReactionStatus`新增`hitResourceSuppression15C`；B0 raw 48-field合同不改。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- 新focused test及现有schema断言测试。

## 验收

test-first compile red；生命周期/copy/snapshot/checksum/parity/schema/zero-allocation；相关snapshot/
checksum/B5、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除carrier和序列化/checksum/parity写入，schema恢复8/12/15，移除focused test。

## 验证结论

- test-first fresh compile red：12个预期`CS1061/CS0117`。
- fresh compile 0 error；focused `701eecdfd8ff4b069516914cf3bb12d5` 6/6。
- schema/snapshot/checksum related `21015094ea1445afbae067cb5601bd52` 69/69。
- 精确NTSD28 broad `9db7870ab74844bd9aa9d5ffee7b3a44` 638/638。
- BattleRuntimeSelfCheck `2026-09-05T12:49:59Z` PASS；Scene unchanged。
- child继承/type0强制1 producer仍归B7/C25b。
