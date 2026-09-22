<!-- CHANGE-RECORD
id: NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001
status: RUNTIME_PENDING
change-kind: Q08_NATIVE_RESULT_CONTINUE_HELD_INPUT
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs
authority: formal Logan GameSession28 effective-combatant held input before BattleFlow28 step and timer144-to350; Q08 RESULT-CONTINUE-INPUT-AUDIT-001
evidence: isolated Unity RED 0/2 then focused 10/10, adjacent scene 3/3, SelfCheck log PASS; real battle and Q08 parent pending
-->

# NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001

Correction (2026-09-22): the approved P-19/G-08 visual exceptions remain. The parent follow-up is timer101 logical record and timer350 transition/combat stop, not changing the Unity result page to display at101. Earlier “101 UI consumer” language is superseded only on that point.

Task: `docs/ai/TASKS/NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001.md`. Created before scripts. Current Unity native carrier accepts no frame argument and therefore cannot make the formal >=144 held Attack/Jump shortcut. The old UI's P1/P2 pressed-edge route has different ownership and timing.

Planned exact change: pass the same-tick frame to the precombat native result producer; scan active configured roster `PlayerSlot` entries and current `Buttons` for Attack/Jump, including a third participant, without requiring the entity to remain alive. Do not use physical slot index or postcombat Results input. Keep result carrier snapshot/checksum schema unchanged because the existing timer/phase/transition fields already store the outcome. Unknown/absent frame is neutral. Avoid reading a stale previous-tick frame.

Expected side effects: native result timer can jump 144->350 under participant held input; mode transition follows the existing carrier's 350 branch. No content, Scene, nonbattle, framework or old Results UI behavior change. Acceptance and rollback in Task. Actual diff, tests, limitations and remaining parent Q08 gaps must be appended immediately after script edits.

Actual code written: `NTSD28Q08BattleFlowRedProbeEditorTests` adds the 143/144 held shortcut and active-third/inactive/pressed-only cases; isolated Unity initial RED XML is `artifacts/diagnostics/NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001/UNITY-RED.xml`, 0/2 PASS with expected 350 versus actual 144. `NTSDBattleTickSystem.RunTick` passes only same-tick frame to `SimulationWorld.AdvanceNativeBattleResultsBeforeCombat`; `BattleResultsOutcomeHostWriter` uses active roster `PlayerSlot` and held Buttons to set timer350 after increment, then existing phase/transition handling. No schema, old UI or reserve edit. Current status remains `IN_PROGRESS` until focused postchange compile/test and limitation review. Real Play, native full state projection, 101 UI consumer and phase3 combat skip remain Q08 parent or this Task acceptance pending.

Postchange: independent Unity focused 10/10 PASS, adjacent result scene seam 3/3 PASS, zero compiler errors, full isolated SelfCheck log PASS. Original Battle Scene SHA unchanged. Evidence and limitations in `artifacts/diagnostics/NTSD28-Q08-RESULT-CONTINUE-HELD-INPUT-001/ACCEPTANCE-PENDING.md`. The preceding `IN_PROGRESS` sentence records the intermediate code-written state; current status `RUNTIME_PENDING` because representative original Player battle/close-reenter and Q08 parent UI/phase3 integration remain unverified. Rollback only these three production call-chain hunks and the two test cases under repository approval rules; preserve user work.
