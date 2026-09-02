# SIMULATION-WORLD-MODULE-EXTRACTION-001 Task Contract

## Objective

Replace the historical `partial class SimulationWorld` implementation with explicit non-Mono child modules owned and orchestrated by `SimulationWorld`, following `Assets/NTSD/Docs/simulation-world-module-extraction-plan.md`.

## Authorized scope

- SimulationWorld composition and compatibility façade.
- Registry/runtime-slot/lifecycle module extraction.
- AI Input/Sensing/Decision aggregate extraction.
- PassPipeline and stable behavior-domain pass extraction.
- Removal of empty/historical SimulationWorld partial implementation files.
- Architecture, focused, parity/checksum, worker, shutdown and Play validation.

## Forbidden expansion

- No gameplay-rule, pass-order, 30 Hz, RNG, input-time or checksum changes.
- No Naruto DDA or unrelated SelfCheck repair.
- No Scene, DAT, Prefab, URP, Input Actions, Server or C++ changes.
- No broad asmdef/namespace/directory migration.
- No destructive Git operations or unrelated cleanup.

## Exit criteria

No `partial class SimulationWorld` remains; state ownership is explicit; World is the aggregate root and façade; related behavioral evidence matches the pre-change baseline; governance and unresolved external blockers are honestly recorded.

## Current status — 2026-09-02

- M1～M9 focused gates passed; M10 runtime/editor compile is 0 error.
- M10 AI regression is 158/158; worker/checksum/ordered-shutdown/architecture is
  35/35; three stale owner-path assertions are 3/3.
- Two clean `NTSD_Battle` Play/Stop cycles have no target cleanup warning and leave
  the Scene clean.
- Full EditMode executed all 1763 tests but is blocked by pre-existing/task-external
  position38, package-version, static-catalog and concurrent S0 WPoint baselines.
- Fresh full SelfCheck is blocked by a task-external central-render P4 feature-owner
  assertion. The forbidden-expansion rules prohibit repairing it in this Change.
- `SimulationWorld.cs` is 6040 lines. This exceeds the 2500-line architecture alarm;
  the retained aggregate-root/API/diagnostic/snapshot responsibilities are recorded
  in the Change Record. No partial declaration or historical partial file remains.
- Therefore the Change remains `IN_PROGRESS`; final acceptance requires the external
  baseline owners to close their independent Changes, followed by full-matrix and
  SelfCheck reruns.
