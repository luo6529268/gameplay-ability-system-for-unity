# R7-CAP-01 — pool / slot allocator / dynamic capacity inventory

> 日期：2026-08-22  
> 状态：`INVENTORIED / D-CAP-001 OPEN / NO CODE CHANGE`

## Goal

确认C++ lowest-free/generation/lifecycle合同在Unity扩展profile中是否保持，并审计pool/seal是否真正满足
Mobile 1000与Desktop dynamic/no-hard-cap交付边界。

## Authority / Evidence

- C++ `include/game_world.h`、`frame_advance.cpp`、`game_tick.cpp`、`collision.cpp`；
- Unity `RuntimeSlotAllocator`、`RuntimeSlotTable`、`SimulationWorld.Registry.partial.cs`、
  `SimulationRuntimeCapacityModule`、`BattleRuntimeAllocationGate`、`BattleLogicReferencePool`、`LF2ObjectPool`；
- focused job `4cc1de5fb20b49609ee0824cd64c4af4` 44/44 PASS；
- `RESEARCH/R7-CAP-01-pool-slot-dynamic-capacity-recertification-20260822.md`。

## Result

- Authority400/Mobile、lowest-free、generation、pending release、pool family/reset与0 B合同通过当前自动矩阵；
- `PoolMaxSize=200`不是硬上限；
- `D-CAP-001`：DesktopExtended seal后拒绝增长，默认Windows准备512后实际存在battle-time hard cap；
- fresh-domain Unity Console为0条error/warning，2026-08-22 22:45:05 full self-check PASS；
- 当前未修改代码，且不能在未固定zero-GC/desktop capacity策略前直接实施。

## Future repair WPs

1. `R7-CAP-01A`：只做桌面容量/0 B/admission合同决策与验收矩阵；
2. `R7-CAP-01B`：按已批准合同实现preflight reservation、controlled growth或deterministic fault；
3. `R7-CAP-01C`：Windows Player >512 opoint/slot reuse/pool/central visibility与GC验收。

## Out of scope / stop

不允许在本inventory中提高常量、解除seal、允许tick内new、修改C++或改变Mobile 1000合同。
