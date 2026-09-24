<!-- CHANGE-RECORD
id: NTSD28-USER-D024-NONCHAR-X-BOUNDARY-WITNESS-001
status: FOCUSED_TEST_PASS
change-kind: D024_NONCHAR_X_BOUNDARY_TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024NonCharacterXBoundaryWitnessEditorTests.cs
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable battle_world.cpp stage bounds
evidence: docs/ai/TASKS/NTSD28-USER-D024-NONCHAR-X-BOUNDARY-WITNESS-001.md
-->

# NTSD28-USER-D024-NONCHAR-X-BOUNDARY-WITNESS-001

Created before any test script edit. The test-only package characterizes the existing production `LF2Entity.ApplyPreFrameXBounds` route with width 800 and fixed coordinates; it does not settle the user's pending D-024 source-rule/physical cull policy. It must leave DAT, Scene, production scripts and nonbattle code untouched. Expected side effects are an Editor import/test run and a generated `.meta` only; no scene Play or asset save. Actual test/result, diff, verification and rollback evidence follow after execution.

Actual file: `Assets/NTSD/Scripts/Test/Editor/NTSD28D024NonCharacterXBoundaryWitnessEditorTests.cs` plus Unity-generated `.meta`. It directly invokes the production noncharacter X bounds method at width800 and factor1; no production behavior or scene was edited. Initial formal-expected focused job `cb6bd8d3a2134117b0cb3bada32fae32` failed all three cases: grounded OID150 X50 expected cull true/actual false; protected OID122 X50 expected100/actual50; X790 expected700/actual790. The retained diagnostic assertions explicitly characterize the defect and passed 3/3 in fresh job `3bdacb2565dd4a8bbd5e34cd01abf5fd`. `FOCUSED_TEST_PASS` applies only to the diagnostic witness, not NTSD parity.

Risk/dependency: the original Editor remains in non-Play Menu; this direct method witness does not cover full Driver timing, battle-mode-only OID exception, type3, physical/source choice, Scene Play or formal EXE observation. D-024 noncharacter destruction policy remains user-pending, Q07 paused. Rollback is the new test and generated `.meta` after provenance review; no DAT or other user work should be removed.

Final local checks: `Tools/Validate-ChangeLedger.ps1` exited 0 and reported `Change ledger validation PASSED`; `git -c core.safecrlf=false diff --check` exited 0. Unity imported the new test and generated its `.meta`; compilation returned idle and the current Editor log search found no `error CS`. The Menu Scene was modified externally during this task (its disk SHA changed to `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228` at 13:53 local); this diagnostic test did not write that Scene. Battle Scene disk SHA remains `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`.
