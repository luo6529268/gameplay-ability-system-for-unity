<!-- CHANGE-RECORD
id: NTSD28-Q08-ARMOR-KO-RESULT-FLOW-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: root Logan EXE BattleFlow28 and GameSession28; BATCH-04 Q08; exact HP3 selected-armor KO fixture
evidence: docs/ai/TASKS/NTSD28-Q08-ARMOR-KO-RESULT-FLOW-001.md
-->

# NTSD28-Q08-ARMOR-KO-RESULT-FLOW-001

Before edit: the Q08 HP3 selected-armor full-tick fixture has 1,700 equal mapped entity fields and matching first KO event, but the Unity raw/KO/domain exporter does not publish `BattleResultsRuntimeState` at the post-tick boundary. Formal root trace already has per-tick `battleFlow` with timer, phase, winner and transition state. Existing ordinary KO result Play witnesses use a different Naruto scenario; they cannot certify this exact selected-armor fatal hit's result handoff.

Declared change: add an optional result-flow output path to the existing diagnostic request/exporter and gate it to the exact Q08 HP3 schema. Write one immutable header and one post-complete-tick row for result timer/output timer/phase/transition/living group mask using the existing canonical JSON writer. Do not change runtime result rules or legacy export behavior. Potential side effects: one Editor recompilation and one new diagnostic output file; no production, content, Scene, ProjectSettings, mode Asset or nonbattle mutation.

Validation: fresh original Editor compile and exact 26-tick export, formal root comparison with first difference, ledger validator/diff check and three protected hashes. Risk is mistaking post-tick `NativeResultTimer` for formal `timer` on transition tick; compare both output and stored timer with documented mapping. Rollback is only this optional diagnostic hunk, preserving other current work.

After edit: the request and private exporter accept an optional `resultFlowOutputPath`, reject conflict with the request result path or other output files, and restrict capture to the existing strict HP3 Q08 schema. The new sidecar writes a header and five native-result carrier fields after each successful Manual complete Driver tick. Old callers keep default-null behavior. No production result logic, content, Scene or nonbattle script was edited. Original Editor compile/capture and formal comparison remain pending.

After validation: original Editor script/assembly timestamps show the new exporter was compiled, console check returned no C# compilation errors, and one strict HP3 Manual complete-Driver request returned `PASS`. The new sidecar has header + all exact ticks 1–26. Comparison with the unchanged root EXE's initial-action-110 trace gives 26×5=130/130 mapped result-flow values equal: precombat fatal tick16 timer0/mask groups1+2, next tick17 timer1/winner1. `artifacts/diagnostics/NTSD28-Q08-ARMOR-KO-RESULT-FLOW-001/ACCEPTANCE.md` gives output paths, mapping and limits. Protected hash, Change Ledger and diff checks follow below; no production result fix was needed for this bounded route.

Final check: the optional-output run's 26 raw entity rows equal the previous HP3 run's 26 rows. Change Ledger validator exit0, `git diff --check` exit0; original Editor idle/non-Play/compile false. Battle/Menu/ProjectBattleModeConfig SHA-256 remained unchanged (exact hashes in the acceptance report). No broader Q08 phase or aggregate batch completion is claimed.
