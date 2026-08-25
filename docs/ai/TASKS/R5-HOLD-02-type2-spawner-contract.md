# R5-HOLD-02 — type-2 held throw `SpawnerEntityIndex` contract

> 建立日期：2026-08-22  
> 状态：`PLANNED`  
> 对应差异：`D-HOLD-002`  
> Change ID：`R5-HOLD-002`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:1597-1630,1977-2006`。

## Goal

使 Unity 真实 `LF2Weapon` 的 held `dvx` throw 与 C++ release writer 合同一致：type `1/4/6` 写入 holder
runtime slot 到 `SpawnerEntityIndex`，type `2` 不写该字段并保留进入 throw 前已有值。

## Scope

允许修改：

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

允许的实现仅为：给当前 private `ThrowHeldWeapon` 传入明确的 spawner-stamp 条件，且仅让 type `1/4/6` 为真。
type `2` 必须调用同一 release / velocity / state 流程，但不能写、清或重置 `SpawnerEntityIndex`。

## Required behavior

1. type `1/4/6` 继续在 `wp.dvx != 0` throw 后写 `SpawnerEntityIndex = holder slot`；
2. type `2` 在同类 throw 后保持其已有 `SpawnerEntityIndex`（包括 `-1` 或非默认值）；
3. type `2` 仍只使用已有的随机 frame、authoring velocity、link release、release tick、PickerStableId 与
   `WeaponThrowing` 行为；本包不改变后四项；
4. current held pass、两轮调用顺序、random call count、frame/PN/wait、FrameDelay、CPoint/WeaponSync、
   holder/child link cleanup 都不得改变；
5. 不修改、运行、构建、复制或向 C++ authority 写入任何内容。

## Evidence / acceptance

| 层级 | 验收 |
|---|---|
| S0 source | 重读 C++ 两轮 type `1/4/6` / type `2` blocks、`clear_released_held_slot` 与 `Makefile`；重读 Unity writer/reader。 |
| S1 focused fixture | 现有 real weapon held fixture扩展为：type `1/4/6` stamp holder slot；type `2` 保留一个非默认 preseed sentinel，并继续锁定已有 frame、velocity、link、state、FrameDelay 行为。 |
| S2 Unity | `Validate-ChangeLedger.ps1`、本包范围 diff check、Unity script compile `error CS=0`、full `BattleRuntimeSelfCheck` PASS。 |
| S3 honesty | 最高状态只能是 `RUNTIME_PENDING`；C++ trace、first-difference 与真实 Play Mode 继续待验。 |

## Explicitly excluded

- `D-HOLD-003`：共享 helper 的 `PickerStableId=holder slot` extra write；
- `D-HOLD-001` FrameDelay、ReleaseTick、PN/wait、random、OnThrown、holder / child link release；
- `LF2WeaponFrameLogicResolver` 或 `LF2Entity` 的 target selection reader 策略；
- valid held relation、CPoint/WeaponSync、scheduler、slot/generation、input、AI、collision、render、DAT、scene、
  resources、performance 和 C++ authority。

## Stop conditions

- 需要修改任何未列出文件，或需要改 `PickerStableId`、reader 策略、pickup origin / target selection；
- C++ source contract 无法以 release-listed writer 闭合；
- Unity compile / focused fixture / full self-check 失败；
- `R1-WP02` C++ trace变为可用时，不自动扩大本包，另建 trace验证记录。
