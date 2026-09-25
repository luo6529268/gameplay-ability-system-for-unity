<!-- CHANGE-RECORD
id: NTSD28-Q08-NATIVE-TRANSITION-COMBAT-FREEZE-001
status: RUNTIME_PENDING
change-kind: Q08_NATIVE_TRANSITION_COMBAT_FREEZE
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
authority: formal Logan GameSession28 precombat BattleFlow transition skips tick_driver; mode commands 2 28 128 202 remain distinct
evidence: source fixtures ordinary/loop/mode4 double-run; isolated Unity RED0/2 then transition3/3, combined17/17 and full SelfCheck PASS; host and real battle pending
-->

# NTSD28-Q08-NATIVE-TRANSITION-COMBAT-FREEZE-001

Created before scripts. Task: `docs/ai/TASKS/NTSD28-Q08-NATIVE-TRANSITION-COMBAT-FREEZE-001.md`. Current Unity `NTSDBattleTickSystem.RunTick` advances `AdvanceBattleFlowTick` and all combat passes after `AdvanceNativeBattleResultsBeforeCombat` emits timer350/transition. The archived full-tick mode4 RED measured FrameSequence349→350 on the transition tick while formal GameSession retained its combat tick. The old Unity Results page postworld writer can also mutate the old battle World after activation; its presentation is approved, these combat mutations are not.

Planned: move the native classifier before Unity combat-flow header work, return successful host-step completion when it emits or retains a nonzero transition, and skip the old combat passes and UI result writer after that boundary. Preserve the phase3 ordinary command2→1 advancement in the native carrier and distinct 28/128/202 values. Focused tests cover existing mode4 transition/following, ordinary transition/following, and old Results writer admission. The subsequent AppManager/SimulationTickDriver route is a separate dependency and must not be silently treated as done. Expected side effects: no combat pass/world-clock/old-Results-writer mutation on/after350; host tick index, checksum and `ApplyFrameInputSet` currently occur before this core method and require a separate host-boundary review. Rollback/validation as in Task; actual diff and evidence will be appended after code.

Test-first update: added ordinary command2→1 with old-world clock frozen and active old Results settings pressed-input denial to the declared Q08 Editor test class. The previous mode4 349→350→next test already had a measured first-difference RED. New tests await isolated Unity prechange execution before the core edit; no production code changed under this Record yet.

Prechange: independent Unity `UNITY-ADDITIONAL-FREEZE-RED.xml` compiled and failed both new targets 0/2 at expected old-world FrameSequence0 versus actual1 for ordinary transition, and old Results `PendingHostAction` expected0 versus actual1 for mode4 transition. This adds distinct ordinary and old-page first differences to the archived mode4 349→350 RED.

Production change: in `NTSDBattleTickSystem.RunTick`, the native result classifier now runs before Unity combat-flow header/projection, spark, input, frame, collision, lifecycle, Results UI writer and world-clock passes. A nonzero transition returns `FullReturn` immediately, preserving worker completion semantics; the same phase3 carrier may convert ordinary command2→1 on a later call. Combat ECS shadow refresh is skipped on these noncombat calls. All no-transition combat passes retain their relative order. The driver still applies its frame input and increments host tick before this core call, and no menu route has been implemented. Isolated focused postchange test/compilation and SelfCheck remain pending; do not promote this to full Q08 acceptance.

Postchange: isolated Unity target `UNITY-TRANSITION-CORE-POST.xml` 3/3 PASS, combined Q08 result carrier and Results input `UNITY-TRANSITION-CORE-ADJACENT.xml` 17/17 PASS, full `UNITY-TRANSITION-CORE-SELFCHECK.log` logs battle runtime PASS/editor completion. The preceding “pending” sentence is historical code-written state. Formal root EXE/source SHA and original Scene SHA were rechecked unchanged. See `TRANSITION-COMBAT-FREEZE-ACCEPTANCE.md` for measured scope and remaining host/real battle gates. Current `RUNTIME_PENDING` is not Q08 parent closure.

Final local governance: `Tools/Validate-ChangeLedger.ps1` PASS 665 records/six governed code diff files; `git diff --check` exit0. Evidence is archived in `TRANSITION-CORE-LEDGER.log`. No commit, push, Scene edit, resource deletion or nonbattle edit.

2026-09-25 original-project follow-up: unchanged production and test code ran in original Editor job `0a164411360a4854a799026c81256e40`; the three scoped transition-core tests passed 3/3, failed0. The full result and Scene hashes are in `TRANSITION-COMBAT-FREEZE-ACCEPTANCE.md` and its linked JSON. The preceding pre-host comments remain historical: `NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001` later guarded old-world driver admission. This Record remains `RUNTIME_PENDING` because natural KO/transition, command routing, Player and formal EXE-visible comparison were not verified by these EditMode tests.
