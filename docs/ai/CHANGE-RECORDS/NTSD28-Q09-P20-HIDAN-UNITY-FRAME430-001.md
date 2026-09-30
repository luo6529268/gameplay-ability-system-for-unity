<!-- CHANGE-RECORD
id: NTSD28-Q09-P20-HIDAN-UNITY-FRAME430-001
status: FOCUSED_TEST_PASS
change-kind: Q09_P20_HIDAN_NATURAL_FRAME430_UNITY_RAW_CAPTURE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired natural frame430 tick16 root LFR evidence
evidence: formal paired/root 30-tick natural input selected 120 of 120 equal; Unity old Hidan schema fixed to unrelated catch schedule
-->

# NTSD28-Q09-P20-HIDAN-UNITY-FRAME430-001

Status before script: `IN_PROGRESS`. [Task and precise exit](../TASKS/NTSD28-Q09-P20-HIDAN-UNITY-FRAME430-001.md).

Current code has a reusable JSON-driven full Driver capture and a strictly validated older Hidan catch schema. It cannot accept the new formal six-row combo fixture without a separate schema. The file is already dirty with other governed changes, which must remain intact.

Planned side effects: new Q09-only diagnostic schema/capture fixtures; no runtime producer, presentation behavior, formal data or nonbattle changes. Code path, rollback and validation are declared in the Task.

Actual code: `NTSD28UnityRawCaptureEditor` adds `Q09HidanFrame430ScenarioSchema`, admits it to formal Logan scope, locks 30 ticks/seed/mode/stage/difficulty/Hidan participants, and requires exactly physical J0–1/K8–9/L+D+J12–13. Existing catch schema and Q08/R06 dirty hunks stay intact. Added one JSON scenario and new raw/input-RNG/domain/result artifacts only. Original Editor PID11944 compile refresh succeeded after transient domain-reload MCP disconnect; same original instance recovered idle/nonPlay and consumed one request. Result PASS; Unity versus root selected actor action/currentMP/inputPhase/combo0 120/120 equal, frame430 first tick16. Four protected asset SHA values unchanged. See [acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-UNITY-FRAME430-001/ACCEPTANCE.md).

Final status `FOCUSED_TEST_PASS` for this original Editor full-Driver logic gate only. Original Battle Scene natural rendered body, formal root GPU pixel and full P-20/Q09 remain open. No production/DAT/PNG/Scene/nonbattle change. Final `Tools/Validate-ChangeLedger.ps1` PASS (1003 records/34 governed diff files; [log](../../../artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-UNITY-FRAME430-001/ledger-validation.txt)); `git -c core.safecrlf=false diff --check` exit0.
