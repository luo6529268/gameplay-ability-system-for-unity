# R5-HOLD-02 — type-2 held throw `spawner_slot` preflight

> 日期：2026-08-22  
> 状态：`COMPLETED (source preflight only)`  
> 差异：`D-HOLD-002`  
> 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live `src/entity/game_tick.cpp::game_tick(...)`。

## 结论

`D-HOLD-002` 可以作为独立、单 writer 的最小修复包处理。

在 C++ release 的两轮 negative-held scan 中，当前 held DAT `obj_type` 为 `1`、`4` 或 `6` 的 `dvx` throw
明确写 `e_core.spawner_slot = holder slot`；当前 DAT `obj_type == 2` 的相邻 branch 只写随机 frame、速度、link
release 和可选 Z 速度，没有 `spawner_slot` 写入。Unity 真实 `LF2Weapon` 的共享 `ThrowHeldWeapon` helper 在两种
branch 后都会无条件写 `SpawnerEntityIndex = holder slot`，因此错误地改写了 type-2 的字段。

最小修复应当只让 type `1/4/6` 调用带 `stampSpawnerSlot=true` 的现有 throw helper，让 type `2` 调用同一 helper
但保留进入分支前的 `SpawnerEntityIndex`。**不能**把 type-2 字段强制重置为 `-1`：权威分支的精确语义是“不写”，
而不是“写 -1”。

## C++ source 合同

| 观察点 | 证据状态 | 规则 |
|---|---|---|
| 第一轮 type 1/4/6 throw | `VERIFIED` | `game_tick.cpp:1597-1619` 在 `wp.dvx != 0` 后写 `e_core.spawner_slot=hi`，写 frame 40、速度、双方 link release 和可选 `vz`。 |
| 第一轮 type 2 throw | `VERIFIED` | `game_tick.cpp:1621-1630` 只写 `frame=rand%6`、速度、双方 link release 和可选 `vz`；该 block 不写 `spawner_slot`。 |
| 第二轮 type 1/4/6 throw | `VERIFIED` | `game_tick.cpp:1977-1997` 重复 type 1/4/6 的 `spawner_slot=hi` 写入。 |
| 第二轮 type 2 throw | `VERIFIED` | `game_tick.cpp:1999-2006` 重复 type-2 branch，仍不写 `spawner_slot`。 |
| helper side effect | `VERIFIED` | `game_tick.cpp:133-139` 的 `clear_released_held_slot` 只清 holder 的 held-slot / throw guard；它不写 child `spawner_slot` 或 `picker_idx`。 |
| release participation | `VERIFIED` | `Makefile:32` 列入 `src/entity/game_tick.cpp`；本结论不依赖修改、构建或运行 C++ executable。 |

`game_world.h:152,247` 和普通 child spawn/reset 路径可见默认 `spawner_slot=-1`，但它们不改变 type-2 throw 的
“保持已有值”合同。所有 C++ authority 文件在本次均只读。

## Unity writer / reader crosswalk

| Unity位置 | 当前行为 | 与 C++ 的关系 |
|---|---|---|
| `LF2WeaponHeldStateResolver.Act:75-92` | type `1/4/6` 与 type `2` 都进入同一个 `ThrowHeldWeapon`。 | writer dispatch 已分支，适合最小化传递 stamp 标志。 |
| `LF2WeaponHeldStateResolver.ThrowHeldWeapon:103-123` | 无条件 `weapon.SpawnerEntityIndex = holder.Runtime.SlotIndex`。 | type `1/4/6` 正确；type `2` 为 extra write。 |
| `LF2WeaponFrameLogicResolver:241-309` | hit_Fa=12 的 target 保留 / 扫描以 `SpawnerEntityIndex` 推导 holder team 并过滤候选。 | 字段可影响后续可观察目标选择。 |
| `LF2Entity.ResolveFrameLogicTargetByHitFa:2310-2377` | 通用 current-DAT hit_Fa target 逻辑也把该字段作为 holder-team filter。 | 字段不是纯诊断数据。 |
| `NTSDEntityRuntime.SpawnerSlotIndex`、checksum / ECS mirror | runtime field会进入 parity snapshot、checksum 与 SoA mirror。 | 本修复预期改变 type-2 的真实 runtime state；不改这些 reader。 |

C++ 的 `frame_advance.cpp` 也有 `spawner_slot` target-filter consumer（静态读取位于约 227、696 行），但当前
`Makefile` 对该翻译单元的直接 build participation 没有在本预检中独立闭合。因此它只能作为**影响方向的 INFERRED
补充**，不作为本包的 release-authority 验收来源。真正的 writer 合同已由 release-listed `game_tick.cpp` 闭合。

## 新发现但必须独立记录的差异

同一 Unity helper 在 release 后还写：

```csharp
weapon.PickerStableId = holder.Runtime?.SlotIndex ?? -1;
```

两轮 C++ type-2 held throw block与 `clear_released_held_slot` 都未写 `picker_idx`。C++ `picker_idx` 在后续
frame logic 中会参与 target selection；Unity 同样将它用于 hit_Fa=4 / hit_Fa=12。因此这是一项独立的
`D-HOLD-003`：**已静态发现，尚未完成其完整 source consumer / pickup-origin contract，不允许合入 D-HOLD-002。**

## 最小实现与夹具设计

允许修改仅限：

1. `LF2WeaponHeldStateResolver.cs`：为 private throw helper 增加显式“是否写 spawner”参数；
2. `BattleRuntimeSelfCheck.cs`：扩展现有 real `LF2Weapon` held type test。

夹具至少锁定：

1. type `1/4/6`：throw 后 `SpawnerEntityIndex == holder runtime slot`；
2. type `2`：以非默认 sentinel 预置 `SpawnerEntityIndex`，throw 后仍保持 sentinel；这验证“不写”而不是“写 -1”；
3. type `2` 继续满足现有随机 frame 范围、速度、link release、throwing state、FrameDelay 保持；
4. fixture 不断言、删除或修复 `PickerStableId`，该字段归 `D-HOLD-003`；
5. C++ trace 与真实 Play Mode 继续为 `RUNTIME_PENDING` 层，不得以 source/fixture 宣称 C++ runtime 完整一致。

## 非范围 / 停止条件

- 不改 `PickerStableId`（`D-HOLD-003`）、ReleaseTick、FrameDelay（`D-HOLD-001`）、PN/wait/random、link release、
  held pass顺序、CPoint/WeaponSync、opoint、slot/generation、input、AI、collision、render、DAT、scene或资源；
- 不改 C++ authority、不运行或构建 C++ executable；
- 若实现需要触及 `LF2WeaponFrameLogicResolver`、`LF2Entity` 的 consumer 策略，或需要确定 type-2 asset 的真实
  hit_Fa / pickup 起源，停止并建立独立合同；
- `R1-WP02` 的 C++ full trace 仍 `BLOCKED`，不以本预检替代。
