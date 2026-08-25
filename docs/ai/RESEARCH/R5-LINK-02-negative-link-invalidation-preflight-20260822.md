# R5-LINK-02 — negative-link invalidation preflight

> 日期：2026-08-22  
> 状态：`COMPLETED (source preflight only)`  
> 差异：`D-LINK-002`  
> 行为 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp`。

## 结论

`D-LINK-002`可以作为一个独立、最小的R5修复包处理。C++ release 的两轮 negative-held scan 在关系无效时
只写 child `link_state=0`；Unity共用的`HeldObjectProcessAll`还把 child `HolderStableId`写为`-1`。

该字段是Unity对C++ `holder_idx`的当前映射。移除这一项extra clear不会移动pass、不改变有效held行为、
不触及正link、CPoint、release、weapon act、slot allocator或pool；同一共用函数会自然同时覆盖已存在的第一轮和
第二轮held scan。

## C++ release source 合同

| 观察点 | 证据 | 规则 |
|---|---|---|
| 第一轮negative-held | `game_tick.cpp:1441-1457` | 升序处理active且`link_state<0`的child；`holder_idx`越界、holder inactive或holder `target_idx != child slot`时，只写child `link_state=0`。 |
| 第二轮negative-held | `game_tick.cpp:1860-1872` | 同样的升序条件与invalid relation分支；同样只写child `link_state=0`。 |
| 例外分支 | `game_tick.cpp:1485-1496`、`1512-1522`、`1571-1640`及第二轮对应分支 | 有效held对象进入饮料、state10/12、wpoint throw或kind3 release时会写双方link/holder/slot等字段；这些不是本差异的invalid relation branch。 |
| build参与性 | `Makefile:32` | `src/entity/game_tick.cpp`参与release构建。 |

`R1-SOURCE-005-cpp-cpoint-held-link-opoint-lifecycle-contract.md:22,46-47,80`已记录同一source结论。
未运行、修改、构建、复制或向C++ authority目录写入任何内容。

## Unity current mapping

| Unity位置 | 当前行为 | 与C++差异 |
|---|---|---|
| `SimulationQueryAndLinkModule.HeldObjectProcessAll:39-61` | 按runtime slot升序处理active且`LinkState<0`的child；invalid holder或target mismatch时写`LinkState=0`并写`HolderStableId=-1`。 | 多写了`HolderStableId=-1`。 |
| `NTSDBattleTickSystem:450-457` | 同一`HeldObjectProcessAll`被first/second held pass各调用一次。 | 只需要改共用invalid branch；不改调度。 |
| `BattleHeldObjectWriter.RunStep12` | 只在关系有效时处理实际held/throw/release行为。 | 不在本包范围。 |

## 最小测试合同

1. child `LinkState<0`且`HolderStableId`越界：一次held pass后仅`LinkState=0`，保留原holder slot；
2. child指向active holder但holder `TargetSlotIndex`不指向child：同样只清child `LinkState`，保留child holder slot和holder字段；
3. 对同一无效child再次运行shared held process：已清为0的child不再被处理，保留的holder field不能被第二轮意外清空；
4. valid negative link和所有`RunStep12`/throw/release行为不在本包中变更；现有R2双held pass检查继续作为回归。

## 明确不做

- 不改`D-SCHED-004`的双pass顺序；
- 不改`D-LINK-001`、正link、CPoint/WeaponSync、抓取、held release、type2 throw、opoint或slot lifecycle；
- 不改`BattleHeldObjectWriter`、AI、输入、collision、render、资源、场景或C++ authority；
- 不将static source结论升级为C++ runtime trace或Play Mode证据。

## 证据状态

- **VERIFIED (source)**：C++两个invalid negative link分支只写`link_state=0`。
- **VERIFIED (Unity static)**：Unity共用invalid branch额外清`HolderStableId`，并被两轮pass调用。
- **UNKNOWN**：C++ full trace与真实战斗场景的观察证据仍不可用；R1-WP02保持BLOCKED。
