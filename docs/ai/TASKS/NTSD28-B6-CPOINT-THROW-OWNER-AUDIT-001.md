# Task Contract — NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_STAGE_SPLIT_DEFINED / FULL_RESOURCE_DEFERRED`
> 依赖：`NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001 / VERIFIED`

## 目标

只读冻结 CPoint throw settlement 的完整 Authority/Unity owner：正式字段与 last-write 语义、
kind-1 throw gate、资源 attacker/transaction、environment injury/source、位置/动作/速度写入、
无 depth 输入的 Vz 保留、actual/HitPlan/trace 与 focused test 责任，并给出不交叉污染 held
settlement 的最小 test-first 实施顺序。

## 范围

- Authority：`combat_records.*`、`battle_world.cpp::advance_catch_relations()`、对应 tests 与正式
  decoded CPoint corpus。
- Unity：`BattleCatchPointValue*`、`Lf2DatConverter`、`BattleCpointWriter`、B5 resource helper、
  `NTSDEntityRuntime` environment carriers、HitPlan、现有 CPoint tests/self-check。
- 只读审计；不改 C#、Config、Scene、Prefab、资源、ProjectSettings 或 Authority。

## 不变量

- 不把 `WeaponCount` 继续当作 throw injury/environment carrier。
- 不把无 depth 输入解释为 Vz=0；只在 up/down 恰一项为真时覆盖。
- 不重复实现 B5 已验证的资源运算；必须复用其 pure core/attacker resolver。
- 不提前处理 held injury、cover sync、caughtact combo 或 H 内容替换。
- actual 与 HitPlan/trace 的字段与顺序必须在同一实施序列闭合。

## 验收

- 输出完整字段/调用者/写入者/测试矩阵。
- 说明 formal CPoint contract 是否需要先扩展 `drain/gain`，并区分当前 corpus 可达字段。
- 选择单一下一 test-first 包及可回滚路径。

## Owner 结论

| 责任 | 当前 owner | 结论 |
|---|---|---|
| throw gate/动作/位置/速度 | `BattleCpointWriter.ApplyThrow()` | 唯一 actual 写入点；在 kind-1 advance 内原位修正。 |
| raw throw fields | 现有 `BattleCatchPointValue`/legacy `CatchPoint` | 本首差所需 `ThrowVx/Vy/Vz/ThrowInjury` 已有；无需先改 schema。 |
| display lead | `BattleDamageWriter.ApplyNativeHitDisplaySteps()` | 可在 B6 复用；仅需先确认 resource attacker 能解析。 |
| MP resource transaction | B5 pure helpers + B7/B8/B11/H | 算法虽已验证，但 production 仍受 child suppression、selected-mode override 与 authoritative baseMax 阻塞；B6 不得提前接线。 |
| environment injury/source | `NTSDEntityRuntime.EnvironmentState320/EnvironmentSourceSlot160` | carrier、snapshot、checksum 已存在；source 必须写 caught 自身 slot。 |
| legacy `WeaponCount` | `LF2Entity.WeaponCount` | throw tail 不得写；测试必须以 sentinel 证明保持。 |
| parity/checksum | `BattleParitySnapshot`、`BattleLockstepChecksumModule` | MP、MP consumed、display steps、environment、Vz、WeaponCount 已覆盖，无 schema 变更。 |
| HitPlan | `BattleEcsHitExecutionPlan` | CPoint advance 位于 hit candidate plan 之后，不应伪造 HitPlan branch。 |
| 回归证据 | focused Editor test + `BattleRuntimeSelfCheck` + 抓取 Play probe | 更新现有错误断言；正式 Play probe 保持后置运行。 |

正式 75 条 throw 中 `drain/gain` 均为 0，但 injury MP reward 仍读取 selected-mode rules、child
suppression 与 authoritative `base_max_mp`。因此 B6 只先实现可独立精确关闭的 display lead、
environment/self-source、WeaponCount exclusion 与 Vz；完整 MP transaction 继续由 B7/B8/B11/H
依赖闭合后回接。完整 CPoint 的 `drain`（正式另有 3 条）、F/B/UZ/DZ 等 schema 扩展在后续
held/data-contract 包单独处理。

下一唯一包：`NTSD28-B6-CPOINT-THROW-ENVIRONMENT-VZ-PRODUCTION-001`。

## 回滚

仅移除治理记录；没有行为、内容或 Scene 回滚。
