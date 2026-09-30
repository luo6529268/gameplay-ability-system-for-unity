<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-PROJECT-STAGE-EDGE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and user project-map exception
evidence: docs/ai/TASKS/NTSD28-Q07-D024-PROJECT-STAGE-EDGE-PLAY-001.md
-->

# NTSD28-Q07-D024-PROJECT-STAGE-EDGE-PLAY-001

Status: `VERIFIED / DIAGNOSTIC_RED_ONLY`. Exact contract and exit are in the Task.

Before: the existing Han/Lee original Battle Scene probe has mapped Z650 and in-map Z400 candidate request paths. It captures Stage physical Z237..760 and source/physical entity positions, but has no independent first-tick project edge mode; source Z100/600 would otherwise enter the unrelated natural earthquake chain or candidate contract. All current dirty script content and request outputs must be preserved.

Plan: add one optional edge request file with a `stageEdgeOnly` flag, require source projection and source Z100 or Z600, stop after one full Driver tick and publish a separate diagnostic report. Reuse existing snapshot, map/camera unchanged assertion and ordered shutdown. This changes no production behavior. Potential side effects are Editor domain reload and a new report/request file only. Do not mutate Scene, DAT, map, camera, GameConfig or formal executable/source.

Acceptance: original Editor C# compile, two unique original Battle Scene edge reports with physical/source positions and snapshot values, explicit-source fixture control, zero borrowers, five protected SHA-256 values stable, `git diff --check` and `Tools/Validate-ChangeLedger.ps1` exit0. Unverified layers remain named. Rollback is the inverse patch limited to this probe branch; do not restore the entire already-dirty script.

Actual edit: the one existing Editor probe now has a separate `ProjectStageEdge` request file with required `stageEdgeOnly`, mapped source Z100/600 and Lee X520. It stops after one complete Driver tick, skips unrelated natural/candidate and PNG gates, and writes to the new result root; existing request selectors and report roots remain unchanged. The original Scene map/camera and ordered shutdown checks still run. No production script, Scene, DAT, map, GameConfig or formal source changed. Validation pending.

Verification: original Editor PID11944 bridge `refresh_unity` produced Tundra build success, zero C# errors and reloaded the test script. Independent Battle Scene Play reports `q07-project-stage-edge-near-z100-20260929-a.json` and `q07-project-stage-edge-far-z600-20260929-a.json` each completed one full Driver tick, original Stage physical Z237..760 unchanged, ordered shutdown true, borrowers0 and saved Scene hash unchanged. Near source100/view157.808 became source237/view374.005 instead of project near237; far source600/view946.849 remained outside project max760. Existing explicit-source `NTSD28SourceStageDepthEditorTests` first job failed to initialize before any test; second with initTimeout180000 succeeded8/8, job `525ee953c2b346a891ab0bb1128c17b1`. Five protected hashes, targeted `git diff --check` and Ledger validator stayed stable/exit0. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q07-D024-PROJECT-STAGE-EDGE-PLAY-001/ACCEPTANCE.md). Only the diagnostic is verified; parent stage-domain production repair remains open.
