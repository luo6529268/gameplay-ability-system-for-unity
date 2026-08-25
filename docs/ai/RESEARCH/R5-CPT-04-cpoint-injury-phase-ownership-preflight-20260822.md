# D-CPT-004 — CPoint injury phase ownership / duplicate opportunity 静态预检

> 日期：2026-08-22  
> 状态：`STATIC CONFIRMED / NO CODE WRITTEN`  
> Authority：`J:/QQFile/NTSD2.4/ntsd_release` 的 release live source。  
> 关联：`D-CPT-002` 的 global stats writer 不能在本问题关闭前独立实施。

## C++ release 的 phase ownership

1. `game_tick.cpp:659-664` 的 step10 固定顺序是：
   `run_cpoint_runtime_pass(world)` → `weapon_sync_runtime_pass(world)`。
2. `cpoint.cpp:23-190` 处理 prev-frame kind1 relation、decrease、action、throw和dircontrol；
   该函数没有 CPoint injury、HP、combo、global stats或held position writer。
3. `weapon.cpp:13-20` 对 active entity 扫描 `weapon_sync_held`；其
   `collision_check2_cpoint:22-107` 是 current-frame state9 CPoint 的唯一 injury / vaction /
   held-position writer，injury仅在 `50-75` 执行。

因此，C++ 的 CPoint action 先决定 current frame；之后 weapon-sync 是否还满足 current frame state9 /
CPoint relation，才决定是否发生一次 held injury 与 position sync。

## Unity 当前交叉路径

| Unity 路径 | 当前行为 | C++ 对照 | 判定 |
|---|---|---|---|
| `BattleCpointWriter.RunKind1:41-42` | 在 prev-frame CPoint pass 内直接调用 `SyncCaughtByCpoint`。 | `cpoint.cpp:23-190` 没有 held injury / position sync。 | extra early writer。 |
| `SyncCaughtByCpoint:191-209` | 写 victim vaction，随后 `ApplyHeldInjury`，再同步 held position。 | 应只属于 `weapon.cpp:42-107`。 | phase ownership 错位。 |
| `RunActionSelection / ApplyAction:143-182` | action 可在早期 injury 后把 attacker attacking 清回 0。 | C++ action 在 weapon-sync injury 前发生。 | 可能恢复第二次 injury 条件。 |
| `SyncHeldCpoint:106-135` | 之后 current-frame state9 再调用同一 `SyncCaughtByCpoint`。 | 对应 C++ `weapon_sync_runtime_pass`。 | 同 tick 第二个 writer opportunity。 |
| `PreInteractionTickAll:2280,2305,2339` | 完整扫描依次执行 CPoint、mismatch、weapon sync。 | 与 C++ 外层顺序相同。 | 同一 Unity tick 可实际到达两个机会。 |

## 最短静态 witness

当 attacker 的 prev-frame 是 state9/kind1、当前 action frame仍为 state9/kind1，且：

1. first `RunKind1` 的 `SyncCaughtByCpoint` 以 `AttackingCounter=0` 先写 injury；
2. `aaction` / `taction` / `jaction` 选择后 `ApplyAction` 将 attacker attacking 清为 0；
3. later `SyncHeldCpoint` 看见 action 后仍为 current state9/kind1，再满足同一 injury 条件；
4. Unity 就有第二次 injury / stat 写入机会。

反方向，若 action 后 frame 不再是 state9，C++ weapon sync不会 injury，但 Unity early writer已经伤害。
这不是仅靠 `AttackingCounter` “通常挡住一次”即可视为等价的情况。

## 对 D-CPT-002 的影响

`D-CPT-002` 的 C++ global `g_kill_stats` / `g_damage_stats` 写入归属 current weapon-sync injury。
如果现在直接在 Unity `ApplyHeldInjury` 补 stats：

- non-action path可能只写一次，但时点提前；
- action+state9 path可得到第二次 stats opportunity；
- action→non-state9 path会在 C++ 没有 weapon-sync injury时提前写 stats。

所以 `D-CPT-002` 不能作为“只补两个数组”的独立 code change 继续。先关闭本项的 phase ownership，
再以最终唯一 injury writer实现 stats。

## 后续独立 Work Package 建议

预定 `R5-CPT-004`（尚未建立 Change Record）：

- Goal：令 `RunKind1` 只承担 C++ `cpoint.cpp` 的 relation/decrease/action/throw/dircontrol；
  令 current held vaction/injury/position只在 `SyncHeldCpoint` 对应 C++ `weapon.cpp` 执行一次。
- Likely files：`BattleCpointWriter.cs`、`BattleRuntimeSelfCheck.cs`。
- Required fixture：同一 `PreInteractionTickAll` 内的
  1) no-action state9 injury once，
  2) action→state9 injury once且发生在 action 后，
  3) action→non-state9 no injury，
  4) vaction/position/attacking/frame-delay/link preservation。
- Out of scope：D-CPT-002 global stats、D-CPT-003 reciprocal mismatch、pass order、held/link、
  collision、input、render、C++ authority。

未创建 Task Contract / Change Record 前不得改脚本。
