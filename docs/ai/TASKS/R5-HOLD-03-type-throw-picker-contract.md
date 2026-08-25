# R5-HOLD-03 — held throw picker preservation contract

> 建立日期：2026-08-22  
> 状态：`PLANNED`  
> 对应差异：`D-HOLD-003`  
> Change ID：`R5-HOLD-003`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1597-1630,1977-2006`；
> consumer / writer context：`src/entity/frame_advance.cpp:215-271,695-735`。

## Goal

使 Unity real held `dvx` throw 的 `PickerStableId` 与 C++ `picker_idx` writer 合同一致：type `1/2/4/6` throw 不写该字段，
保留进入 throw helper 前已有值；后续target selection保留自己的既有 writer 权限。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

允许的唯一 production change：删除 shared `ThrowHeldWeapon` 中无条件的
`weapon.PickerStableId = holder.Runtime?.SlotIndex ?? -1;`。

## Required behavior

1. type `1`、`2`、`4`、`6` 的 held `dvx` throw 后都保持预先存在的 `PickerStableId`；
2. 不把保留语义实现为强制 `-1` reset；
3. R5-HOLD-001 的 FrameDelay、R5-HOLD-002 的 SpawnerEntityIndex、frame随机/40、速度、link release、
   ReleaseTick、OnThrown、PN/wait和随机调用数量均不改变；
4. `LF2WeaponFrameLogicResolver` 和任何 generic reader / target selection writer不得修改；
5. 不修改、运行、构建、复制或向 C++ authority 写入任何内容。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | 重读C++ reset、pickup、两轮held branch、release helper与`frame_advance` target writer；确认Makefile参与性。 |
| S1 focused fixture | existing real held fixture为type1/2/4/6设置各异的非默认 picker sentinel，throw后逐个断言保持；保留R5-HOLD-001/002断言。 |
| S2 Unity | ledger validator、R5范围diff check、Unity script compile `error CS=0`、full self-check PASS。 |
| S3 honesty | 最高状态只能是`RUNTIME_PENDING`；C++ trace、same-scene first-difference和真实 Play Mode继续待验。 |

## Out of scope / stop conditions

- 不改 `SpawnerEntityIndex`、FrameDelay、ReleaseTick、target reader / selection、pickup、reset或任何其它R5链路；
- 不改 C++ authority；
- 如需修改未列出文件、发现 type-specific C++ picker writer，或Unity compile / fixture / self-check失败，停止并记录新差异。
