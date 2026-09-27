<!-- CHANGE-RECORD
id: NTSD28-Q08-STATE12-KO-FULL-TICK-001
status: FOCUSED_TEST_PASS
change-kind: Q08_STATE12_KNOCKOUT_COMPLETE_TICK_DIAGNOSTIC
code-path: Tools/NTSD28Q08Diagnostics/state12_ko_full_tick_source_probe.cpp
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08State12KoFullTickEditorTests.cs
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; paired playable GameSession28/BattleWorld28 state12 contact and knockout event; indexed Naruto frame180
evidence: artifacts/diagnostics/NTSD28-Q08-STATE12-KO-FULL-TICK-001/ACCEPTANCE.md
-->

# NTSD28-Q08-STATE12-KO-FULL-TICK-001

Pre-change state: Q08 nonstandard KO writers passed original-Editor direct focused 7/7, but no formal same-state complete Session versus original-Editor complete Driver event comparison exists for state12 contact.

Planned result: collect a paired positive/HP50 control through formal `GameSession28::step` with indexed Naruto frame180, then compare one exact Unity complete Driver replay if the formal event is reached. Capture the original rows and first difference before any assertion or production decision. The test must check event time/order/ownership, HP/count and result timer. No authored production behavior changes are authorized by this diagnostic.

Owned symbols: new C++ diagnostic entry and helpers; new Editor NUnit fixture; existing `NTSD28UnityRawCaptureEditor` only for a strict three-tick Q08 scenario schema, participant gate and native RNG initialization. The latter addition is declared before editing the shared harness; do not touch other scenarios or production. Side effects are new diagnostics and test-only temporary World setup/teardown. Protect existing dirty source, four Scene/config hashes, DAT values, resource/Scene/nonbattle behavior.

Validation: formal build and positive/control output, original-Editor one-test compile/run if source gate succeeds, exact trace comparison, protected hashes, `git diff --check`, Change Ledger validator. Report `CODE_WRITTEN`, `COMPILE_PASS`, `FOCUSED_TEST_PASS` or a captured first difference only to the extent fresh evidence supports it. Rollback only reviewed additions in this Change ID; no cleaning unrelated work.

Actual change: added one read-only paired playable C++ diagnostic; the shared Editor raw-capture harness gained only a strict Q08 three-tick schema, participant check and formal native RNG reset; a new NUnit fixture/meta and two exact JSON scenarios compare formal positive/control complete ticks with Unity production Driver. No production writer, DAT, mode Asset, Scene, resource or nonbattle path changed. The first C++ link failed without `-municode`, corrected on rebuild. First Unity fixture compile failed CS0103 for its missing `NTSD.EditorTools` import; that test-only import was added before the successful original-Editor run. No formal/Unity gameplay first difference remained in the declared six-row, 16-column comparison.

Final validation: formal source positive/control each three ticks, source probe exit0; original Editor exact Q08 job `b72c11024a9442e48e45924443118ba8` 1/1 PASS; final Q08 plus adjacent Q07 strict-schema job `6f22b7b1d8cd49ba81758b033fd1b1ca` 2/2 PASS. Final source/Unity logical rows are identical in all six rows and 16 fields per row; byte hashes differ by line endings. The Editor DLL postdates changed Editor scripts. Protected four Scene/config hashes stable, `git diff --check` exit0, and Ledger validator exit0 (902 records/13 diff code files). The transient MCP bridge read timeouts were resolved by re-polling the same live job IDs. Natural Battle Play/Player, Q08 other producer branches and Q09/Q10 consumers are unverified; this Change ID is scoped to the state12 complete-tick gate.
