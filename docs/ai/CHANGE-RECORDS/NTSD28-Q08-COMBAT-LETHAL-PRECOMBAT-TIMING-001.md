<!-- CHANGE-RECORD
id: NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08CombatLethalPrecombatTimingEditorTests.cs
authority: formal Logan playable GameSession28 precombat BattleFlow28 step and complete-session lethal tick12/timer0 tick13/timer1 witness
evidence: NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001/TIMING-FIRST-DIFFERENCE.md and existing Unity Q08 direct-HP full-tick cases
-->

# NTSD28-Q08-COMBAT-LETHAL-PRECOMBAT-TIMING-001

Created before the script edit. Before: Unity Q08 native result tests cover direct HP changes before a full tick, but do not prove that a hit consumed inside that tick leaves the precombat result timer at zero until the following tick. The new test will use two type-0 groups and the existing production collision/hit pass inside `NTSDBattleTickSystem.RunReleaseTick`; it will check the actual lethal HP write and both timer observations. No production logic or resource/Scene/nonbattle path will change. Expected side effects are test-only logic entity allocation and a Unity-generated `.meta`. Acceptance, limits and rollback are in the Task. Record actual code, compile, focused result and any failure here after the edit.

Actual edit: added the one declared test script only. It uses `NTSDBattleTickSystem.RunReleaseTick` with overlapping minimal type-0 itr/bdy entities (injury30 versus HP20) and checks in-tick HP death followed by next-tick native result timer1 and latched group1. No production code, Scene or asset was edited. `git diff --check` on declared paths exited0. At writing, original Editor `Assembly-CSharp-Editor.dll` timestamp 12:23:44 predates this script at 12:54:51; Unity compile and test are pending, and a failing fixture must not be treated as a production defect until collision setup is confirmed. Existing source/full-driver and previous SelfCheck evidence are reused without broad rerun.

Validation blocker (2026-09-22): the original Editor PID 33236 is live, but its Assembly-CSharp-Editor.dll timestamp remains earlier than the new test source. The project-local Unity MCP CLI reports no connected instance. Attempting to start its local HTTP helper with `Start-Process -WindowStyle Hidden` was rejected by automatic command policy with only `blocked by policy`; no helper was started and no alternate shell/launcher retry was made. Unity compilation and the focused test have not run. Ledger validator passed (679 records, 20 governed diff code files), `git diff --check` exited0, and the Battle Scene SHA-256 stayed `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`.
