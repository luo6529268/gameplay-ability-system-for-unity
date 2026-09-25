<!-- CHANGE-RECORD
id: NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001
status: RUNTIME_PENDING
change-kind: Q08_TRANSITION_HOST_TICK_ADMISSION
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationTickHostPolicyEditorTests.cs
authority: formal Logan GameSession28 transition skips combat tick driver and later old-battle input; Q08 core-freeze acceptance identifies unguarded Unity host tick and ApplyFrameInputSet
evidence: isolated Unity host RED0/1 then target1/1, adjacent25/25 and full SelfCheck PASS; routing and real battle pending
-->

# NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001

Created before scripts. Task: `docs/ai/TASKS/NTSD28-Q08-TRANSITION-HOST-TICK-ADMISSION-001.md`. Current driver `_tickIndex`, `_sparkRenderFrame` and `ApplyFrameInputSet` run before `NTSDBattleTickSystem.RunTick`; after core freezes on result transition, later driver invocations still apply input and compute checksum. `CanAdvanceTick` has no transition guard, and the explicit-frame `StepOneTickInternal(FrameInputSet, ...)` does not call `CanAdvanceTick`. Formal `GameSession28::step()` skips the combat driver on/after the transition boundary. Planned test first, then both admission paths return false for a nonzero transition before mutating old-world input or host tick. Preserve the emitted transition state for later AppManager routing and keep mode commands distinct. No new scene/menu/runtime route in this Task; the result screen may remain pending until that route is implemented. Verification and rollback as in Task.

Test-first: `SimulationTickHostPolicyEditorTests.NativeResultTransitionRejectsAutomaticExplicitAndPausedOldWorldTicks` checks normal automatic admission, then after canonical phase3/command202 checks private automatic gate, public explicit frame with `ignorePaused`, and paused F2 host-control path; it asserts unchanged host tick and applied frame input. Exact declared test file was edited before production. Isolated RED pending.

Prechange isolated Unity `UNITY-HOST-ADMISSION-RED.xml` compiled and failed the focused target 0/1 at automatic `CanAdvanceTick` expected false/actual true after phase3/command202. The explicit and F2 assertions were not reached in that RED; the postchange run must cover them.

Production: `SimulationTickDriver.CanAdvanceTick` now rejects nonzero native result transition before automatic/paused host admission. The explicit `StepOneTickInternal(FrameInputSet,...)` checks the same state before `_tickIndex`, `_sparkRenderFrame`, `ApplyFrameInputSet`, checksum or publication; it does not call `CanAdvanceTick`, so explicit Lockstep/Manual remains independent of input-provider readiness. No AppManager/menu route, mode value, pause/cadence, Scene or content changed. Focused postchange and SelfCheck pending; this alone is not full result-session alignment.

Postchange isolated Unity `UNITY-HOST-ADMISSION-POST.xml` 1/1 PASS, adjacent host/Q08/Results `UNITY-HOST-ADMISSION-ADJACENT.xml` 25/25 PASS, and `UNITY-HOST-ADMISSION-SELFCHECK.log` logs battle runtime PASS/editor completion. The preceding pending sentence is historical. `HOST-TICK-ADMISSION-ACCEPTANCE.md` states remaining AppManager routing and original real-battle limits. Current Record remains `RUNTIME_PENDING`, not Q08 complete.

Final local governance: `Tools/Validate-ChangeLedger.ps1` PASS 666 records/eight governed code diff files, `git diff --check` exit0, original Battle Scene SHA unchanged. No computer-use, Scene/content/nonbattle edit, deletion, commit or push.

2026-09-25 original Editor follow-up: the exact host admission focused test passed 1/1, failed0 in job `8500eef22ac443d68e0ea6daef54532a`; saved JSON and Scene hashes are in `HOST-TICK-ADMISSION-ACCEPTANCE.md`. No production or test code changed in this follow-up. `RUNTIME_PENDING` remains because natural battle transition and host command routing/Player behavior were not tested.
