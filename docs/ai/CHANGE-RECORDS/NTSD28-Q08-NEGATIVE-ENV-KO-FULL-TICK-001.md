<!-- CHANGE-RECORD
id: NTSD28-Q08-NEGATIVE-ENV-KO-FULL-TICK-001
status: FOCUSED_TEST_PASS
change-kind: Q08_NEGATIVE_ENV_KNOCKOUT_COMPLETE_TICK_DIAGNOSTIC
code-path: Tools/NTSD28Q08Diagnostics/negative_environment_ko_full_tick_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08NegativeEnvironmentKoFullTickEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; paired playable GameSession28 and BattleWorld28 negative environment resource pass
evidence: artifacts/diagnostics/NTSD28-Q08-NEGATIVE-ENV-KO-FULL-TICK-001/ACCEPTANCE.md
-->

# NTSD28-Q08-NEGATIVE-ENV-KO-FULL-TICK-001

Pre-change: the existing direct-writer Q08 test checks negative environment lethal/missing-source event fields, but no formal same-state full Session versus original-Editor full Driver timing trace exists. The preceding state12 full-tick test covers a different physics producer and is not rerun for this Task.

Planned result: one 13-tick paired positive HP5 / nonlethal HP50 source probe across resource phase12, followed by an exact original-Editor complete Driver comparison if the native branch is reachable. Record raw rows before assertions and preserve any first difference. No production gameplay behavior is authorized to change in this diagnostic.

Owned symbols: new C++ source probe; existing `NTSD28UnityRawCaptureEditor` only strict Q08 negative-environment schema, participant validation and formal RNG initialization; new NUnit Editor fixture and its `.meta`. Side effects are diagnostic files and temporary test-only World setup/teardown. No production, DAT values, Scene, project mode, renderer, menu or nonbattle edits. Rollback only reviewed additions of this Change ID, preserving all prior dirty work.

Validation: formal playable-closure build and source output, original-Editor exact test if source gate succeeds, closest shared-schema adjacent check only if necessary, four protected hashes, `git diff --check` and `Tools/Validate-ChangeLedger.ps1`. Track evidence honestly through `CODE_WRITTEN`, `COMPILE_PASS`, `FOCUSED_TEST_PASS` or a precise captured first difference; Q08 aggregate remains open.

Actual files/symbols: new paired source probe `run_case`/entry; `NTSD28UnityRawCaptureEditor.ValidateScenario` strict 13-tick Q08 schema and participant gate plus `ConfigureWorldAndRoster` formal RNG selection; new Editor fixture `NegativeEnvironmentKoAndNonlethalControlMatchFormalResourcePhase`, `.meta`, two exact scenarios and raw/evidence files. No production code, DAT value, mode Asset, Scene, old resource or nonbattle path changed. The source probe compiled against 28 core + 2 playable translation units and ran both 13-tick cases, exit0. Its phase column is derived from the formal initial/increment call chain, while Unity records its direct `NativeResourcePhase12` value.

Final scoped verification: original Editor filtered negative-environment job `edf47b46348842ccb2bb2ba43f3f3d43` 1/1 PASS and adjacent unchanged state12 strict-schema job `031491dafde7457f90435a033534827e` 1/1 PASS; 26 complete-tick logical rows ×17 columns match formal source including tick12 missing-source1000 KO and tick13 result timer. Editor DLL postdates test/harness. Four protected hashes and unique GUID checked; final `git diff --check` and Ledger validator results are in ACCEPTANCE. Natural Play/Player, held CPoint, Q09/Q10 consumers and Q08 total exit remain unverified; no broader completion claim follows.
