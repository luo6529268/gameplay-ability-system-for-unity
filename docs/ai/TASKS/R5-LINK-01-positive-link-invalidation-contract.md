# R5-LINK-01 — positive-link invalidation contract

> 建立日期：2026-08-22  
> 状态：`PLANNED`  
> 对应差异：`D-LINK-001`  
> Change ID：`R5-LINK-001`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1828-1845`。

## Goal

使 Unity positive-link invalidation 与 C++ release 一致：无效正向关系只使 holder `LinkState`归零，保持
`TargetSlotIndex` 与 `HeldWeaponStableId` 的既有值。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs`；
- `Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsPositiveLinkValidationPass.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`；
- `Assets/NTSD/Scripts/Test/Editor/BattleEcsPositiveLinkValidationPassEditorTests.cs`。

## Required behavior

1. positive `LinkState` + invalid target slot、inactive target、holder reciprocal mismatch都只写`LinkState=0`；
2. target/held slot字段在Legacy、ShadowCompare与DataOriented path中均保留；
3. reverse target fields继续不被positive holder invalidation修改；
4. valid positive link、slot顺序、generation guard、event ordering、zero allocation和positive-link index更新不变；
5. 不处理negative-link invalidation或任何R5其他差异。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | 复核C++ T11 source和Makefile参与性；不运行/写C++ authority。 |
| S1 | 更新existing self-check matrix与Editor Legacy/Shadow/DataOriented parity tests。 |
| S2 | ledger validator、diff check、Unity compile `error CS`=0、full self-check PASS、focused EditMode test PASS。 |
| S3 | `RUNTIME_PENDING` only; C++ trace / Play Mode remain pending. |

## Stop conditions / out of scope

如需修改negative held child、CPoint/weapon sync、held release、slot allocator、pass ordering、runtime defaults或C++ authority，停止并另建合同。T8、性能、服务器、render、C++ trace和Play Mode均不在本包范围。
