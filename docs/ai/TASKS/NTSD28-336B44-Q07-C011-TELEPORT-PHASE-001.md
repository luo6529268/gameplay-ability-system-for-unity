# NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001

Status: `RUNTIME_PENDING`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001`, BATCH-04/Q07, new 336B44 C011/R04. Original-Editor full-tick RED 2/2, GREEN plus direct control 3/3, and original Battle Scene four-tick Play PASS. Formal-root same-state and natural selected-content integration remain open. [Evidence](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C011-TELEPORT-PHASE-001/REPORT.md).

Authority: selected formal root EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and its playable `source/ntsd28_core/src/simulation/simulation_tick_driver.cpp::SimulationTickDriver28::step` plus `battle_world.cpp::advance_teleport_phase_4a0bd8`. The source flips a zero-initialized phase every battle tick, and calls state-400/401 teleport after frame motion only when the new phase is zero: first tick skips, second tick executes. Root EXE same-state observation remains a separate gate.

Observed Unity first-difference candidate: `SimulationWorld.AdvanceBattleFlowTick` already flips `Runtime.Flow.FrameToggle` from zero before frame motion; the canonical `NTSDBattleTickSystem.NativeTeleport` currently calls `world.NativeTeleportAll()` on every full tick. The direct world teleport method is a mechanism API and its direct tests are not a tick cadence oracle. The old C05 Play probe expected state400 teleport on tick 1 and state401 on tick 2; that is previous-version evidence requiring rebaseline.

Owned scripts: `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`, `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs`, and `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportPlayModeProbeEditor.cs` if the old probe is rerun. No DAT, resource, Scene, input, menu, GAS framework, or stage-map edit.

Plan and acceptance: add a two-tick full battle-pass test first, with state400 and a live enemy at a fixed X. It must show no teleport after initial phase 1 and one teleport after phase 0. Run exact RED in the original Editor. Gate only the canonical tick call with the existing phase, keeping `NativeTeleportAll()` direct use and state500 unrelated paths intact. Run exact GREEN, any old conflicting C05 test, and a bounded original Battle Scene Play probe if safely reachable. Verify formal-root same-state before `VERIFIED`; otherwise use `RUNTIME_PENDING`. Record test results and validate Ledger and scoped diff.

Rollback: review and invert only owned hunks after checking concurrent changes; no `git restore`, reset, clean, DAT changes, or user-file cleanup.
