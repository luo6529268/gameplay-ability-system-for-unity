# Task Contract — NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ACTUAL_ONLY_PACKAGE_DEFINED / HP_BASEMAX_DEFERRED`
> 依赖：`NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001 / VERIFIED`

## 目标

冻结 C09/C20 OID123 MP refill gate/nonpositive-entry 与 OID122/123 exhaustion motion 的唯一
actual owner、精确写入顺序、RNG 边界与测试责任，不提前解决 OID122 的
authoritative baseMax 依赖。

## Owner 矩阵

| 责任 | owner | 结论 |
|---|---|---|
| two-pass traversal | `NTSDBattleTickSystem` + `SimulationQueryAndLinkModule` | C09/C20 placement 与次数不改。 |
| refill classification | `LF2WeaponHeldStateResolver.ProcessDrinkConsumption()` 的 type_sub 122/123 | 当前 Direction-B content 与data.txt均映射到type6；本包不引入release system.dat或改内容权威。 |
| OID123 MP arithmetic | 同上 | 移除HP<=0早退；每次`child HP -= 2`、`holder MP=min(MP+3,500)`。 |
| OID123 cap | 同上 | 仅以child `OrdinaryCreditGate2F4 >= 0 && child MP > 150`写child MP=150；不读`KillCount`、不写holder=150。 |
| shared exhaustion | 同上 + 现有 release helper | HP<1后重置双方action/counter/relation/weaponHP；只消耗一个`[0,7)` RNG写Vx，写Vy=0并保留Vz。 |
| OID122 baseMax | `HPBound/HP/HP3` legacy path + B11/H | 本包只修共享exhaustion tail；`effective_max_hp` 相对authoritative `base_max_hp`的clamp继续后置。 |
| persistent state | 现有 HP/MP/frame/counter/link/Vx/Vy/Vz/weaponHP carriers | 已入copy/checksum/parity，无schema变更。 |

## Focused 验收矩阵

- OID122 HP=1 与 OID123 HP=2：当次耗尽，RNG delta=1，Vx精确、Vy=0、Vz sentinel保留，
  双方action/counter/relation和child weapon HP精确重置。
- OID123 HP=0与负值：仍执行HP-=2、holder MP+3、cap判定和exhaustion，不走Unity早退。
- gate -1：child MP sentinel保留；gate 0：child MP>150被截到150，holder只增3而不被写150。
- 非耗尽 OID123：不消耗exhaustion RNG，后续WPoint path仍执行。
- 更新 SelfCheck 中consume/release sentinel，并以 C09/C20 Play probe验证第一次耗尽后
  第二次不再消费、Scene unchanged、Console 0。

## 不变量

- 不改 HP-refill baseMax clamp、kind-3 WPoint、DVX release、cover pose、terminal action、
  invalid reciprocal、content、Scene 或 system table。
- 不将 `KillCount`、`HolderCopySlot`、`SpawnerSlotIndex` 继续当作 +0x2F4 或 refill carrier。

## 下一 production 候选

`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`。在 CPoint throw 的 Unity runtime 绿灯恢复前，
仅保留为 planned candidate。

## 回滚

仅移除治理记录；没有行为、内容或 Scene 回滚。
