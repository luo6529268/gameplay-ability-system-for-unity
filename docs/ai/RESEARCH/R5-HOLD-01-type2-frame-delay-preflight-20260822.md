# R5-HOLD-01 — type-2 held throw frame-delay preflight

> 日期：2026-08-22  
> 状态：`COMPLETED (source preflight only)`  
> 差异：`D-HOLD-001`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp`。

## 结论

`D-HOLD-001`可作为一个独立的双-writer最小包处理。C++两轮negative-held pass都在进入throw分支前写
child `frame_delay = holder.frame_delay`，而type-2 throw分支本身没有再写该字段。Unity通用held writer和真实
`LF2WeaponBase` resolver都先镜像holder delay、再额外覆盖为`1`，属于同一个C++语义偏差。

正确修复是只删除这两个type-2 branch的`FrameDelay=1`，使先前复制的holder值保留。不得借此处理type-2
`SpawnerEntityIndex`，它是独立的`D-HOLD-002`。

## C++ release source 合同

| 观察点 | source | 规则 |
|---|---|---|
| 第一轮同步 | `game_tick.cpp:1527-1535` | 从holder current wpoint同步child frame/facing，并写`e_core.frame_delay = holder_core.frame_delay`。 |
| 第一轮type-2 throw | `game_tick.cpp:1621-1630` | 写`frame=rand%6`、`vx/vy`、双方link清理、可选`vz`；没有`frame_delay`写入。 |
| 第二轮同步 | `game_tick.cpp:1924-1932` | 与第一轮同样复制holder `frame_delay`。 |
| 第二轮type-2 throw | `game_tick.cpp:1999-2006` | 与第一轮同样没有`frame_delay`写入。 |
| 消费者 | `game_tick.cpp:671` | `frame_delay != 0`会影响frame postprocess；因此该字段保留语义是可观察的，不能用固定1替代。 |
| build参与性 | `Makefile:32` | `src/entity/game_tick.cpp`参与release构建。 |

未运行、修改、构建、复制或向C++ authority目录写入任何内容。

## Unity writer crosswalk

| Unity位置 | 当前顺序 | 偏差 |
|---|---|---|
| `BattleHeldObjectWriter.RunStep12:57-85` | `SyncHeldFrameAndPosition`先写`held.FrameDelay = holder.FrameDelay`；generic current type2 branch随后写随机frame并`held.FrameDelay=1`。 | 覆盖了C++应保留的holder值。 |
| `LF2WeaponHeldStateResolver.Act:59-92` | 真实weapon先写`weapon.FrameDelay = holder.FrameDelay`；`weaponType==2` branch随后写随机frame并`weapon.FrameDelay=1`。 | 覆盖了C++应保留的holder值。 |
| `NTSDBattleTickSystem:450-457` | shared held process由first/second held pass各调用一次。 | 不改调度；两个writer的本地修复自然适用两轮。 |

现有`BattleRuntimeSelfCheck.CheckGenericHeldStep12ContinuationContracts`已包含generic type2与真实weapon type2
throw夹具，但当前错误断言两者的`FrameDelay == 1`。它们可扩展为holder delay保持夹具，无需新增玩法路径。

## 最小验收合同

1. generic current type2 child：holder `FrameDelay=7`、type2 `dvx` throw后child仍为7；
2. real `LF2Weapon` type2：相同holder delay保持为7；
3. 两种路径仍保持随机frame范围、authoring velocity、link clear和weapon throwing state；
4. 不断言或修改`SpawnerEntityIndex`、ReleaseTick、Frame/PN/wait以外的独立契约；
5. C++ trace / Play Mode仍为后续证据，不提升本包等级。

## 非范围

- `D-HOLD-002` type2 spawner write；
- `D-LINK-*`、valid held relation、drink、state10/12、kind3 release、CPoint/WeaponSync、opoint、slot lifecycle；
- scheduler、input、collision、render、性能、DAT/scene/resource和C++ authority。
