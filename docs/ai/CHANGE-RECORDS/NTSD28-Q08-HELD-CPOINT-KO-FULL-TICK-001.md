<!-- CHANGE-RECORD
id: NTSD28-Q08-HELD-CPOINT-KO-FULL-TICK-001
status: FOCUSED_TEST_PASS
change-kind: Q08_HELD_CPOINT_KNOCKOUT_COMPLETE_TICK_DIAGNOSTIC
code-path: Tools/NTSD28Q08Diagnostics/held_cpoint_ko_full_tick_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08HeldCpointKoFullTickEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; paired playable GameSession28 and BattleWorld28 held CPoint settlement
evidence: artifacts/diagnostics/NTSD28-Q08-HELD-CPOINT-KO-FULL-TICK-001/ACCEPTANCE.md
-->

# NTSD28-Q08-HELD-CPOINT-KO-FULL-TICK-001

Pre-change: held CPoint injury has direct-writer and knockout-event focused evidence, but no paired formal complete Session versus original-Editor complete Driver trace. The source order places settlement and potential knockout before `begin_frame_tick`, so reusing a later producer's event-time expectation would be wrong.

Planned result: use formal indexed Naruto frame260/132 and reciprocal relation for HP30/HP100 positive/control complete ticks, then compare Unity with the same initial state if the formal gate is observed. Record a first difference without speculative production edits. No DAT, Scene, mode Asset, old asset, menu, renderer or nonbattle modification is authorized.

Owned symbols: new C++ paired source probe; shared `NTSD28UnityRawCaptureEditor` only exact Q08 held-CPoint schema, participant guard and native RNG setup; new focused NUnit fixture and `.meta`. Diagnostic output stays under the matching artifact folder. Expected side effects are test-only World setup and teardown plus new evidence files. Rollback applies only to this Change ID's reviewed additions; preserve existing dirty work.

Validation plan: playable-closure C++ compile and run, original-Editor compile/focused comparison when source gate succeeds, adjacent strict-schema check only if shared harness changed, four protected hashes, `git diff --check`, Change Ledger validator. Status may advance only with actual evidence. Q08 aggregate and the total goal remain open.

Actual code paths: new `held_cpoint_ko_full_tick_source_probe.cpp` compiled with the formal 28 core + 2 playable sources and ran both three-tick cases, exit0; `NTSD28UnityRawCaptureEditor.ValidateScenario` exact Q08 schema/participant gate and `ConfigureWorldAndRoster` formal native RNG initialization; new `NTSD28Q08HeldCpointKoFullTickEditorTests` plus unique `.meta`. The test-only timeout carrier was corrected from unrelated `CatchTimer` to `CaughtDuration` after the first retained RED observation. No production code or other protected surface changed.

Final scoped validation: original Editor exact held-CPoint job `3c0ecdeafe56459cbe24b9a2ba46fc7f` 1/1 PASS and adjacent negative-environment shared-schema job `f7312e3611cc4771a6749eac021389f6` 1/1 PASS; positive/control each three complete rows ×20 fields match formal source logically. Earlier test-carrier false difference and unrelated MCP `NetworkStream` log failure are retained in ACCEPTANCE. Four protected hashes and GUID uniqueness were checked; `git diff --check` exit0 and Change Ledger validator PASS (904 Records / 17 governed code files). No natural Battle Play/Player, all-definition, downstream Q09/Q10 or Q08 aggregate certificate follows.
