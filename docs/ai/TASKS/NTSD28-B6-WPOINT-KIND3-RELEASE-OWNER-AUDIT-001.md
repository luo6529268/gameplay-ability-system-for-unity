# Task Contract — NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001

> 状态：`SUPERSEDED / CORRECTED_BY_NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001`
> 依赖：`NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001 / VERIFIED`

## 目标

冻结 WPoint kind-3 release 的唯一 actual 写入点、RNG draw 顺序、authored/random 选择、
link/release 边界及 focused/SelfCheck/Play 证据责任。

## Owner 矩阵

| 责任 | owner | 结论 |
|---|---|---|
| C09/C20 traversal | `NTSDBattleTickSystem` + `SimulationQueryAndLinkModule.HeldObjectProcessAll()` | placement 已验证，不改顺序或次数。 |
| kind-3 gate | `BattleHeldObjectWriter.RunStep12()` | 完成 common pose/release 前置后以 `holderWPoint.Kind == 3` 进入唯一 tail。 |
| actual RNG/速度 | `BattleHeldObjectWriter.DropRandomly()` | 唯一 production 改动点；需传入 `BattleWeaponPointValue`，固定先draw 6/7/4/5，再按每轴 authored-nonzero 选择。 |
| random stream | `LF2Entity.BattleRandInt()` | 现有 synchronized stream 和4次消耗已具备，不新建 RNG。 |
| relation release | `DropRandomly()` 现有 `ReleaseHeldWeaponRuntimeInternal/ClearLinks` | 保留既有 release tick/link cleanup，不扩大到 exhaustion 或 DVX release。 |
| persistent state | 现有 frame/Vx/Vy/Vz/link/releaseTick carriers | checksum/parity 已覆盖，无 schema 变更。 |
| focused evidence | 新 Editor test | 固定seed后断言draw数/顺序结果、零dv的整数Vz、非零dv override，覆盖 real `LF2WeaponBase` 与generic held entity。 |
| existing assertions | `BattleRuntimeSelfCheck` kind-3 held sections | 移除`Vz -0.4..0.4`旧期望，改为精确seeded结果或Authority整数范围。 |
| runtime evidence | C09/C20 Play probe | 证明held relation、kind-3 release、RNG delta=4、pose/velocity/link cleanup和Scene unchanged。 |

## 不变量

- 不改 C09/C20 placement、refill OID 122/123 算术、DVX release、terminal action、cover pose、
  child/parent relation carrier、content 或 Scene。
- 四个 RNG draw 必须无条件消耗；不允许 authored nonzero 轴省略 draw。
- X/Y/Z 各自选择 authored 或 random，不使用 Unity 物理缩放、`Time.deltaTime`或
  表现层数值。

## 后续 production 候选

`NTSD28-B6-WPOINT-KIND3-RELEASE-PRODUCTION-001`。但它现在明确排在
`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`之后。前置 throw package 受 Unity
Licensing 阻塞时只保留 planned candidate，不写行为代码。

## 回滚

仅移除治理记录；没有行为、内容或 Scene 回滚。

## 后续纠正（2026-09-08）

本记录的RNG/final-write结论仍可作历史证据，但“唯一production改动点是`DropRandomly()`”不完整：
Unity real/generic路径均会在DVX后提前return，而Authority仍继续独立kind-3 tail。权威后继为
`NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-CORRECTION-001`，不得再按本记录直接实施。
