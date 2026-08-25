# R7-PERF-01 — retire stale PreInteraction cross-pass proof

## Goal

删除无法证明中间content未变化的PreInteraction cross-pass no-op fast path，使C++ T14等价点始终读取
object collision consume之后的current/Prev2 CPoint、link、target与held状态；保留同点whole-pass proof和
participant filtering。

## Scope

- `SimulationWorld.Passes.partial.cs`：
  - death-cleanup不再计算/发布cross-pass proof；
  - `PreInteractionTickAll`不再消费cross-pass proof；
  - 删除private proof storage/helper；
  - 保留public stress/report兼容toggle与last-used diagnostic，last-used固定为false；
  - 同点`TryProveWholePreInteractionPassNoOp`、three-pass writer与participant filters不改。
- `PreInteractionNoOpProofEditorTests.cs`：
  - neutral checkpoint改为断言同点proof、非cross-pass；
  - 新增checkpoint后同slot current kind2 content变化→frame212/Vy=-3/Y=-2与forced legacy等价；
  - 保留occupancy/non-neutral/0 B矩阵。

## Authority / evidence

- C++ release `Makefile:11-35`；
- `src/entity/game_tick.cpp:659-664,1818-1825`；
- `src/entity/cpoint.cpp:23-190`；
- `src/entity/weapon.cpp:13-107`；
- `RESEARCH/R7-PERF-01-preinteraction-cross-pass-proof-preflight-20260822.md`。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
- `Assets/NTSD/Scripts/Test/Editor/PreInteractionNoOpProofEditorTests.cs`。

## Unknowns

- 删除cross-pass cache后1000 AI的净耗时变化；正确性优先，性能只由fresh focused/压力数据判定；
- R1-WP02 C++ full trace仍BLOCKED；
- stress report中的旧cross-pass字段何时正式删除，不在本包。

## Deliverables

- production不再发布/消费stale cross-pass proof；
- same-point proof与fallback行为保持；
- stale-content/neutral/0 B focused tests；
- ledger/STATE/register/plan/handoff更新。

## Verification

1. `dotnet build Assembly-CSharp.csproj --no-restore --nologo --verbosity:minimal`；
2. fresh Unity compile，`error CS=0`；
3. focused `PreInteractionNoOpProofEditorTests`；
4. full `BattleRuntimeSelfCheck`；
5. warmed neutral 0 B；
6. validator与scoped diff check；
7. PlayMode/C++ trace未取得时最高`RUNTIME_PENDING`。

## Stop conditions

- 需要content epoch、pass reordering、CPoint/weapon writer修改；
- stale fixture不能由当前C++ T14 source闭合；
- 需要改stress schema、server/network、scene/DAT或C++；
- same-point proof/forced legacy结果不一致。

## Out of scope

R7-LATE-001、AI/Frame SoA、broadphase/quadtree、render worker、pool/capacity、R8 PlayMode、T8、C++ trace。

