<!-- CHANGE-RECORD
id: NTSD28-Q08-NATURAL-KO-PLAY-EVENT-FIELDS-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08DirectBattleNaturalKoTwoCyclePlayModeTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 Naruto punch trace and paired playable battle_world.cpp record_native_knockout
evidence: docs/ai/TASKS/NTSD28-Q08-NATURAL-KO-PLAY-EVENT-FIELDS-001.md
-->

# NTSD28-Q08-NATURAL-KO-PLAY-EVENT-FIELDS-001

Before script edit: prior original Battle Scene physical-J two-cycle Play test proves natural Naruto punch KO, configured event lifetime, result and rematch, but only checks that some event credits the attacker. Formal root trace plus existing EditMode full Driver test provide exact six-field reference. The requested edit is a single helper assertion extension; no production or resource change. The Task records the exact scope, side effects, failure classification, acceptance and rollback. Protected Scene/Asset baselines must remain unchanged; Q07 collision strategy and other Q08 gates are separate.

2026-09-29 code written: the existing `AdvanceToNaturalResult` helper now requires original Scene slots0/1 and captures the first naturally credited KO event. It asserts event time at its actual tick plus type0/fourOwner0/victim1/source0/credit0, in both invocations of the existing two-cycle Play test. No input schedule, result path, event production, other test, DAT, Scene, Asset or nonbattle code changed. Original Editor import/Play is pending; no runtime conclusion yet.

2026-09-29 first original-Editor run: exact single test job `9e235a383c394510896aca61d2d91459` entered/exited Play and wrote terminal `Temp/Goal18_LastTestResults.xml` at 03:44:01. It failed after 238.9 seconds on a Unity TestRunner unhandled MCP bridge disposed-stream error during domain reload, not on a named KO field assertion. The raw XML is preserved under this package's diagnostic folder, SHA-256 `EEEAB539CD0F6C5568C30DA9BED12FFBB9764836F462BC0C17C68A589C3B72E0`. The Editor was independently idle/non-Play; the persisted job still reported running with no progress, so the documented `run_tests clear_stuck` marked that orphan failed after the terminal XML was saved. This is a failed validation, not gameplay PASS. Before another script edit, the Task extends only this test's timeout from 240 to 420 seconds because the first run ended at the old limit; the same gameplay assertions stay. Retry once without MCP polling during Play reload, then inspect terminal XML and Editor state.

2026-09-29 scoped Play result: exact single original-Editor retry job `53f81a2e515a4e478a455e8d18429717` wrote terminal XML with one named test passed, zero failed, in 278.05 seconds. It ran the existing two-cycle physical J natural Naruto KO helper twice, so each World checked the six formal-reference event fields along with prior lifetime/rematch/result assertions. The only retry code edit was test timeout 240000→420000; no production/input/DAT/Scene/Asset/nonbattle change. The original Editor returned idle/non-Play, all four protected SHA values remained stable, and the plugin's persisted orphan was cleared after archiving the PASS XML. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q08-NATURAL-KO-PLAY-EVENT-FIELDS-001/ACCEPTANCE.md). This closes the selected natural Play field gate only; root whole-state same-seed, other modes and Q08/BATCH-04 remain open.

Final governance: Change Ledger validator exited 0 (995 records / 26 governed diff code files); `git -c core.safecrlf=false diff --check` exited 0. No additional character matrix or production test was run after the passing single Play case.
