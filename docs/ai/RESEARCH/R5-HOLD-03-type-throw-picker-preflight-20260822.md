# R5-HOLD-03 — held throw `picker_idx` preflight

> 日期：2026-08-22  
> 状态：`COMPLETED (source preflight only)`  
> 差异：`D-HOLD-003`  
> 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live runtime。

## 结论

`D-HOLD-003` 是一个独立的单-writer最小包：C++ release 的两轮 held `dvx` throw 对 type `1/2/4/6` 都**不写**
child `picker_idx`；Unity real `LF2WeaponHeldStateResolver.ThrowHeldWeapon` 却在所有这些分支后无条件写
`PickerStableId=holder slot`。

正确修复是只移除该 Unity extra write，使所有 held throw 类型保留进入 helper 前已有的 `PickerStableId`。不得把
字段改写为 `-1`：C++ `Entity.reset()` 的默认值确实是`-1`，但 release throw branch 的精确语义是“保留”，且 C++ 后续
`frame_advance.cpp` 的 target selection 才可能写入该字段。

## C++ release source 合同

| 观察点 | 证据状态 | 规则 |
|---|---|---|
| 初始化 | `VERIFIED` | `include/game_world.h:216-258` 的 `Entity::reset()` 写 `picker_idx=-1`。 |
| pickup | `VERIFIED` | `collision.cpp:1032-1076` 的 regular kind-2 pickup只写target/link/holder/held-slot/pickup count；不写 child `picker_idx`。 |
| 第一轮 held throw | `VERIFIED` | `game_tick.cpp:1597-1630` 的 type1/4/6 和 type2 blocks分别写spawner/frame/velocity/link，但都没有`picker_idx` assignment。 |
| 第二轮 held throw | `VERIFIED` | `game_tick.cpp:1977-2006` 重复上述无`picker_idx` write的规则。 |
| release helper | `VERIFIED` | `game_tick.cpp:133-139` 的 `clear_released_held_slot`只清holder held-slot / throw guard。 |
| later consumer / legitimate writer | `VERIFIED` | release-listed `frame_advance.cpp:215-271,695-735` 用现有`picker_idx`做target选择，并只在该target logic写入新best slot。 |
| build participation | `VERIFIED` | `Makefile:17`列入`frame_advance.cpp`，`Makefile:32`列入`game_tick.cpp`。 |

未运行、构建、修改、复制或向 C++ authority 目录写入任何内容。

## Unity crosswalk

| Unity位置 | 当前行为 | 判定 |
|---|---|---|
| `LF2WeaponHeldStateResolver.Act:75-92` | type1/4/6与type2都落入 shared throw helper。 | 相同 extra writer覆盖全部四类type。 |
| `LF2WeaponHeldStateResolver.ThrowHeldWeapon:103-124` | 在release tick / link teardown前后无条件写`PickerStableId=holder slot`。 | 与C++两轮branch不一致。 |
| `LF2WeaponFrameLogicResolver:150-161,241-309` | hit_Fa=4直接读picker，hit_Fa=12将它当current target并可能re-scan。 | 字段是可观察target state，不是日志。 |
| `LF2WeaponBase:45-50,780-785` | `PickerStableId`明确映射runtime `picker_idx`，snapshot refresh不引入新值。 | 名称与C++字段的对应已在Unity本身注明。 |
| `LF2WeaponReleaseFlowResolver:23-56` 与 `LF2Weapon.OnThrown` | release只改release tick/link；OnThrown只初始化flight counter。 | 都不需要、也不应负责picker stamp。 |

## 最小夹具

扩展现有 `CheckWorldLevelRealWeaponStep12Contracts` / `RunWorldLevelRealWeaponStep12Case`：

1. type1、type4、type6和type2各自输入不同非默认 `PickerStableId` sentinel；
2. held `dvx` throw后每个字段都保持其输入值；
3. 继续保留R5-HOLD-001/002的FrameDelay、spawner、frame、velocity、link、release state断言；
4. 不运行实际 target scan，不修改`LF2WeaponFrameLogicResolver`；该reader只用于说明字段可观察性。

## 允许范围与停止条件

允许改动仅限 `LF2WeaponHeldStateResolver.cs` 与 `BattleRuntimeSelfCheck.cs`。

不得修改 `SpawnerEntityIndex`（R5-HOLD-002已独立闭环）、FrameDelay、ReleaseTick、PN/wait、random、OnThrown、
current target selection、CPoint/WeaponSync、held pass、slot/generation、input、AI、collision、render、DAT/scene或C++ authority。
如需改任何 target-selection reader、pickup、entity reset或当前项以外文件，停止并另建合同。
