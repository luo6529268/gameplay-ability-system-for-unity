<!-- CHANGE-RECORD
id: NTSD28-Q07-OID32-UNITY-RECT-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: FOCUSED_EDITOR_DIAGNOSTIC_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid32BoundarySpriteEditorTests.cs
authority: formal OID32 action95 source/root EXE trace and staged/formal byte-identical hun.dat/hun.png
evidence: original Editor category job fe2dd9581b3b4f2791644341f2baf33e 1/1; actual formal staged pic54 present/pic64 absent
-->

# NTSD28-Q07-OID32-UNITY-RECT-WITNESS-001

Pre-edit state: original Unity Editor PID11944 is idle, not in Play, with the project Battle Scene active. Formal source/root EXE tick0–8 has action95/pic64 and sprite commands; Unity actual indexed-rect result for parsed OID32 is not yet measured. A read-only MCP `execute_code` attempt failed in its temporary compiler (`mono.exe` command-line length) before running code; no Unity test or production conclusion follows from that. Add only the Task-declared focused diagnostic test and meta. Expected side effect is Unity script import/recompile and one EditMode result; no Scene, DAT, PNG, production or nonbattle mutation. Validate focused test, compile, Scene SHA, Ledger and diff. The diagnostic expectation describes current behavior and must not be mistaken for parity acceptance. Rollback follows the Task.

Implementation and focused result: added only the declared Editor test; Unity generated the declared `.meta`. The original Editor recompiled, and category job `fe2dd9581b3b4f2791644341f2baf33e` selected exactly one test, 1/1 PASS. It used actual formal staged OID32 catalog data and decoded PNG dimensions; production `BuildIndexedSpriteRects` yielded valid pic54 and null pic64. The first two test jobs selected zero methods before import and are not counted as PASS. Menu/Battle disk Scene SHA remained unchanged. This confirms the current rect-builder boundary only. Actual SpriteCatalog publication, entity Play render, formal GUI GPU pixels and natural action95 reachability are not tested. Full evidence: `artifacts/diagnostics/NTSD28-Q07-OID32-FRAME95-RELEASE-VISIBILITY-001/UNITY-RECT-ACCEPTANCE-20260925.md`. The test asserts the observed omission and must be updated/retired if a generic parity fix changes that behavior; no production/DAT/PNG/Scene/nonbattle edit.

Governance correction: first `Tools/Validate-ChangeLedger.ps1` run failed because this Record listed the Unity-generated `.meta` under `code-path`; the validator governs scripts, not `.meta`. The `.meta` remains explicitly declared in the Task/body, and its erroneous metadata line was removed. Second run exited0, 829 Records and 25 governed code files covered. Both validator logs are retained; `git diff --check` exited0. No test outcome was changed by the Record correction.
