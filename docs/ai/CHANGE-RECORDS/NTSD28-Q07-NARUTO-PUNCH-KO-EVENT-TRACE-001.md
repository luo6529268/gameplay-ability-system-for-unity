<!-- CHANGE-RECORD
id: NTSD28-Q07-NARUTO-PUNCH-KO-EVENT-TRACE-001
status: VERIFIED
change-kind: Q07_NARUTO_PUNCH_SAME_INITIAL_KO_EVENT_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
authority: formal root NTSD2.8-Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable Naruto frame513 hit/KO trace
evidence: docs/ai/TASKS/NTSD28-Q07-NARUTO-PUNCH-KO-EVENT-TRACE-001.md; artifacts/diagnostics/NTSD28-Q07-NARUTO-PUNCH-KO-EVENT-TRACE-001/ACCEPTANCE.md
-->

# NTSD28-Q07-NARUTO-PUNCH-KO-EVENT-TRACE-001

Pre-change: the original-Editor exact two-Naruto fixture has 30/30 matching occupied slots, 1560/1560 mapped entity fields and 1140/1140 input values with the root formal LFR trace. The formal tick8 frame513 hit generated KO source/victim/credit/four-owner 0/1/0/0. The Unity exporter does not currently emit the production event list; Q08 separately proved physical J and attributed KO under a different initial state, so it cannot close this same-state field comparison.

Declared code path/symbols: `NTSD28UnityRawCaptureEditor.CaptureRequest`, `PollRequest`, `RunAndWriteResult`, `RunScenario`, and optional event JSON builder. Add one opt-in KO output path, collision guard and per-completed-tick read of `SimulationWorld.NativeKnockoutEvents`. No producer, consumer, game rules, DAT, PNG, Scene, ProjectSettings, input asset, Q09 dirty work or nonbattle script change. Expected side effect is one new diagnostic output plus Editor recompile; no changes to existing outputs when option omitted. Exact verification and rollback are in the Task.

Post-change code written: in the declared Editor file only, `CaptureRequest` now accepts optional `knockoutOutputPath`; request/result routing and `RunScenario` reject a path collision or existing KO output, create only the requested new file, and serialize the production event list after each completed Driver tick. `BuildKnockoutTickJson` records event count and battle tick, source type, source/victim/credit/four-owner slots. This is opt-in and leaves existing raw/domain/input-RNG formats unchanged. New branch is restricted to the exact Naruto schema. Compilation, actual Editor request, field comparison, old-output regression, lifecycle and governance checks are pending; status is `CODE_WRITTEN`, not verified.

Post-validation: existing original Editor assembly refreshed after the script modification; Console error query returned zero. One unique 30-tick exact Naruto request returned `PASS`, and raw/domain/input-RNG tick payloads each matched the preserved run2 30/30. The new event stream proves the first KO at completed tick 8, matching formal birth and five attribution/type fields; stored battle time differs 8 (Unity) versus 7 (formal). `comparison.json` and `ACCEPTANCE.md` retain the machine result and source explanation. Original Editor is idle EditMode; Menu/Battle/GameConfig disk hashes and the old three tick track payloads remained unchanged. The diagnostic is VERIFIED; this status does not close Q07 or authorize a production timestamp adjustment. Governance/diff validation is recorded below.

Governance: `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <repo>` exited 0 with 854 Records and seven governed code files covered; `git diff --check` exited 0. The exact logs are under this diagnostic's artifact directory. Later production clock work is tracked under its separate Change ID.
