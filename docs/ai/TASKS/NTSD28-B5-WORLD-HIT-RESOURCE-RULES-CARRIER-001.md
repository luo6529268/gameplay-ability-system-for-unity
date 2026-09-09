# Task Contract — NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001

> 状态：`VERIFIED / CARRIER_READY / F6_PROJECTION_DEFERRED`
> 依赖：`NTSD28-B5-WORLD-RESOURCE-RULES-F6-AUDIT-001 / VERIFIED`

## 目标

新增 world-owned hit-resource numeric rules carrier，保存 active-mode attacking percent `1C`、
attacker injury-MP percent `34` 与 target injury-MP percent `38`，并闭合 reset、core scalar
snapshot、完整 restore、checksum 与 full parity。

## Authority 合同

- playable 普通配置默认值为 `0 / 75 / 75`。
- 三项是 world/session 数值规则，不属于 entity runtime，也不重复保存 F6 local bool。
- F6 local gate继续由 `BattleRuntimeState.FunctionKeys.HitResourceEnabled`唯一持有。
- selected-mode record 对 `1C/34/38` 的覆盖归 B8/H；本包不得猜测 Unity mode mapping。
- 本包只建立确定性 carrier；F6 active-entity projection、registration inheritance及实际 hit-resource
  production transaction均不接入。

## Schema

- world core scalar snapshot `6 -> 7`。
- aggregate battle snapshot `13 -> 14`。
- lockstep checksum `16 -> 17`。
- entity runtime snapshot保持 `9`；B0 raw entity schema不变。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs`
- `Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs`
- 新 focused test、完整 restore test及现有 schema 断言测试。

## 验收

test-first compile red；默认值/reset/不可变capture/full restore/checksum/full parity/schema/
warm capture allocation；相关snapshot/checksum、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

已取得 fresh compile red：24个预期 `CS1061`，均为尚不存在的
`BattleRuntimeState.NativeHitResourceRules`或`BattleWorldCoreScalarSnapshot.HitResourceRules`。

## 验证结论

- fresh compile 0 error。
- focused snapshot/restore/carrier `00c10176507a4d4d9a0748d96b29ed9d`：16/16。
- related schema/snapshot/checksum `6102e87e598b4910bc68b25b4b21ab73`：59/59。
- 精确92-class NTSD28 broad `a4349bdea8fe4c2387286cbb2d689805`：650/650。
- BattleRuntimeSelfCheck `2026-09-05T13:29:18Z` PASS；清理其预期negative-path日志后Console 0 error。
- Scene SHA `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、
  203477 bytes、mtime unchanged；Change Ledger 252 records/221 governed code files PASS。
- F6 active projection与registration inheritance仍归下一独立包；mode override/production仍后置。

## 回滚

移除numeric carrier及snapshot/checksum/parity写入，schema恢复6/13/16，移除focused test并恢复
既有schema断言。本包不触碰Config、Scene、Prefab、Authority或production hit transaction。
