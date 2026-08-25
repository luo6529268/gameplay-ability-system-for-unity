# HANDOFF — R3-INP-02 F1/F2 step gate / entry-clear separation

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> 当前脚本 Change Record：`R3-INP-002 / RUNTIME_PENDING`；static、Unity compile 与 F1/F2/entry-clear self-check 已通过。`R3-INP-001` 保持 `RUNTIME_PENDING`。

## 已确认的最小事实

- C++ `game_tick(...)` 每 tick 清 step gate；F2 mode 2 在同 tick 变成 gate 1/mode 1；
  wait state跳过完整 post-cooldown input callback，但仍通过 OID、frame、interaction、preframe/stage/render，
  render 后才 early return；
- Unity 已有并会 snapshot/checksum `BattleStepMode` / `BattleStepGate`，且恢复、held 和 recovery
  的局部消费者已读取 wait predicate；但 `NTSDBattleTickSystem` 没有 producer/early-return owner；
- Unity `NeedClearInput` 是 bootstrap 清输入 marker，不是 C++ F1/F2。它的既有 M1→clear→return
  契约必须保留；
- C++ `g_dword_449048` 非零调试解锁与 Unity physical F1/F2 binding 尚未闭合，不能偷偷混入 default
  core gate 修复。

## 已拆分的后续范围

| 后续包 | 覆盖 | 原因 |
|---|---|---|
| `R3-INP-02` | 仅 D-SCHED-010 default F1/F2 step gate | 与 scheduler/render-after-return 边界相关，可复用现有 Flow 字段。 |
| `R3-HOLD-INP-01` | D-INP-001 negative-link input | 依赖 held/caught / R5 relation，不应与 gate 混改。 |
| `R3-AI-LIFE-01` | D-INP-002 HP=0 / respawn AI caller | 依赖 death/respawn writer，不应与 gate 混改。 |
| `R3-INP-03` | D-INP-003～006 packet / P1-P2 / target / physical binding | 需要 journal / Play Mode / user binding evidence。 |

## 推荐下一步

`R3-INP-002` 已完成 source / compile / focused self-check，状态为 `RUNTIME_PENDING`。下一脚本包应按
`D-010` 转入 `R3-HOLD-INP-01` 的只读 relation/input preflight，不能因本包 PASS 宣称 physical F1/F2、
debug-unlock、negative-link 或 dead-AI input 已对齐。
