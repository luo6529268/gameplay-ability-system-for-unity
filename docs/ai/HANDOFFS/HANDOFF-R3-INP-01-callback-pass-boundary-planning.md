# HANDOFF — R3-INP-01 callback / OID pass boundary

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> 脚本 Change Record：`R3-INP-001 / RUNTIME_PENDING`；local static order、Unity compile 和 request self-check 已通过，仍缺 R3 joint fixture / Play Mode / C++ trace。

## 已执行的最小子流程

`R3-INP-01` 只处理 `D-SCHED-005`：正常逻辑 tick 中已将 Unity OID 7/8/51 maintenance 移至
完整 human/AI character input callback 之后。

### 已核实的 source order

```text
C++ game_tick:
cooldown → permitted post_cooldown_input callback
         → P1/P2 poll → all active character AI prepare/apply_input
         → OID 7/8/51 maintenance

Unity normal path after R3-INP-001:
cooldown → human input → CharacterInput → OID maintenance → EarlyFrameAdvance

Unity entry-clear branch retained:
OID maintenance → NeedClearInput clear → return
```

source 坐标已写入 `TASKS/R3-INP-01-callback-pass-boundary.md`。本包已修改两份 Unity script，并新增
一个 scheduler-driven OID7/8 fixture；没有运行或修改 C++，没有运行 Unity Play Mode。local static
check、UnityMCP scripts refresh/compile（filtered `error CS`=0）及 request self-check（`PASS`）均已取得。
full Console 仍包含 UnityMCP disposed-object 与两条预期 runtime-rest negative fixture error，不作为
compiler / self-check failure 隐瞒或误报。

## 不可混入的工作

- `NeedClearInput` 与 C++ F1/F2 gate 的分离：`R3-INP-02`；
- negative held/caught input：后续 R3/R5 relation 包；
- dead/respawn AI：后续 R3 输入/帧推进包；
- packet edge、physical W/S/A/D/J/K/L binding：R3-INP-03/用户 Play Mode；
- OID maintenance 自身 identity、slot、pool、lifecycle、CPoint/held/collision 公式：不属于本包。

## 执行条件

根据 `D-009`，后续已批准的计划内包可继续推进；但 `R3-INP-001` 自身最多只能声明
`RUNTIME_PENDING`，不得借由本次 PASS 宣称 C++ runtime / Play Mode 完整对齐。不得在没有独立 Record
的情况下混入 `NeedClearInput`、F1/F2、AI decision、FrameInputSet、physical input 或 OID formula 改动。

## 已移交的下一包

`R3-INP-02` 的 source/preflight / Task Contract 已建立：
`TASKS/R3-INP-02-step-gate-vs-entry-clear.md`。它只处理 default F1/F2 step gate 与 entry-clear
分离；D-INP-001、D-INP-002、packet / physical binding 已由 D-010 拆为后续独立包。
