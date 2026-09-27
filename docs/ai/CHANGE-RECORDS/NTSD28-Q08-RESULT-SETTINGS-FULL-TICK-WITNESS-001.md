<!-- CHANGE-RECORD
id: NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08CombatLethalPrecombatTimingEditorTests.cs
authority: root Logan EXE BattleFlow28 and GameSession28; user P-19/G-08 result-page exception; BATCH-04 Q08
evidence: docs/ai/TASKS/NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001.md
-->

# NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001

Before edit: `BattleResultsWriter.RunSettingsPhase` may apply result multipliers to active old-world characters; the direct-writer SelfCheck expects this, but no complete natural-KO tick path has measured the write while native timer remains below 144. `PendingHostAction` has no production consumer in current repo search; a simple removal would not prove next-match behavior.

Declared edit: add only one focused Editor test to the named existing fixture. Observe positive/negative complete-tick effects from natural overlap KO through Unity results-page settings; no runtime, asset, Scene, DAT or nonbattle change. Anticipated side effects are Editor test discovery/compilation and in-memory fixture Worlds only. Do not infer a formal EXE first difference from Unity-only result settings, which have no matching shipped host command. Acceptance and rollback are in the Task Contract.

After edit: the existing two-combatant complete-tick fixture now has one additional test. It first records both living groups, moves the victim into the existing lethal hit frame, lets the Unity result page activate naturally, navigates Settings cursor 2→1→0 through P1 pressed edges, then compares full-tick Attack/no-Attack cases. The production writer is unchanged. The observed native timer is 14 and transition remains 0; no-Attack leaves old actor `FallDamageDiv=0`, Attack changes it to 100. This proves current Unity runtime coupling before native transition, not a formal EXE first difference because the shipped formal host has no corresponding results-settings action.

Original Editor verification: PID 11944 compiled `Assembly-CSharp-Editor.dll` newer than the test source; `Editor.log` reports Tundra build success with warnings and no C# errors. The exact MCP EditMode job `ffc645602a7444d3aaf6348a44a3c007` succeeded 1/1 in 1.656 seconds; raw response is `artifacts/diagnostics/NTSD28-Q08-RESULT-SETTINGS-FULL-TICK-WITNESS-001/editor-job.json`. `get_editor_state` reported idle, non-Play, no compilation or running test; protected Battle/Menu/mode Asset hashes match Task baselines. The original Task requested XML, but MCP provides only structured job JSON; Task acceptance text now states the actual retained format, and no XML is claimed. `Tools/Validate-ChangeLedger.ps1` exit 0 (919 Records, 10 governed code files; pre-existing stale-declaration warnings), `git diff --check` exit 0.

Unverified and next owner: no natural physical keyboard Play or formal matching results-settings command; `PendingHostAction` still has no production consumer. Ordinary result command2 unloads Battle to the protected menu selection page; new Battle uses `CurrentMatchConfig`, sourced in production from `CharacterSelectionController.SetMatchConfig`, not old result fields. This test deliberately records current behavior and must be updated with a later guarded production fix. A separate production Task must keep Unity's result UI while moving next-match settings out of old-world combat state; it must test the actual next-match consumer rather than only suppressing this write. A plan that needs menu/selection changes is outside this goal's current nonbattle authorization. Q07/Q08/BATCH-04 and the total goal remain open.
