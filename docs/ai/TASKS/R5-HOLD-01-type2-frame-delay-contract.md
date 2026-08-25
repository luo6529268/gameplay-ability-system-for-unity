# R5-HOLD-01 — type-2 held throw frame-delay contract

> 建立日期：2026-08-22  
> 状态：`PLANNED`  
> 对应差异：`D-HOLD-001`  
> Change ID：`R5-HOLD-001`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1527-1535,1621-1630,1924-1932,1999-2006`。

## Goal

使Unity type-2 held throw与C++ release一致：child保留在同步阶段得到的holder `FrameDelay`，而不是被type-2
throw branch强制改为`1`。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleHeldObjectWriter.cs`；
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

## Required behavior

1. generic current-DAT type2与真实`LF2WeaponBase.WeaponType==2`都先保留既有holder delay copy；
2. type2 branch仍写random frame、authoring velocity、link clear和throwing state；
3. first/second held pass顺序、random call count、frame/PN/wait writer、spawner、release tick、link/held cleanup均不改；
4. holder delay为0、正数或负数时，type2 throw不额外覆盖为1；
5. 不修改C++ authority。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | 重读C++两轮sync/throw source和Makefile；重读两个Unity writer；不运行/写C++ authority。 |
| S1 | 现有generic和real type2 held fixture都改为验证非零holder delay保持，并继续锁定frame范围、velocity、link/state。 |
| S2 | ledger validator、R5范围diff check、Unity compile `error CS=0`、full self-check PASS。 |
| S3 | 只可标`RUNTIME_PENDING`；C++ trace和真实Play Mode继续待验。 |

## Stop conditions / out of scope

如需要改`SpawnerEntityIndex`、ReleaseTick、CPoint、pass order、valid held relation、slot/generation、
其它weapon type、C++ authority或任意非列明路径，停止并另建合同。`D-HOLD-002`必须独立处理。
