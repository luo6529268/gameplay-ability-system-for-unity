<!-- CHANGE-RECORD
id: NTSD28-USER-RASENGAN-WINDOW-PARITY-001
status: FOCUSED_TEST_PASS
change-kind: USER_REPORTED_RASENGAN_WINDOW_DIAGNOSTIC
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UserRasenganWindowEditorTests.cs
authority: formal NTSD 2.8-Logan playable/core source and user Naruto physical-input report
evidence: artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/REPORT.md
-->

# NTSD28-USER-RASENGAN-WINDOW-PARITY-001

Status `FOCUSED_TEST_PASS / PHYSICAL_PLAY_PENDING`; Task: `docs/ai/TASKS/NTSD28-USER-RASENGAN-WINDOW-PARITY-001.md`.

Before: full Unity 241-to-253 action sequence and attack sampling window have not been captured; isolated action-253 J success is insufficient. The formal source-model 32-tick sweep in Temp indicates a tick-25/tick-26 boundary, but does not prove formal EXE play or Unity full-window parity.

Intended after: one Editor-only NUnit diagnostic invokes the existing formal-content World/Manual Driver helper for 32 ticks, supplies canonical physical J at specified ticks, and writes per-tick raw diagnostic rows. It must not change battle runtime, authored DAT, any project Scene, camera, input binding or nonbattle owner.

Expected side effects: one new test file and optional Unity-generated `.meta`; Temp diagnostic output and Editor test results. No gameplay effect. Acceptance: original Editor compile and focused test, explicit source-model comparison, unchanged DAT/Scene status, Ledger validator. Real physical-key Play, formal EXE visible timing and any production fix remain outside this diagnostic Change. Rollback is a reviewed inverse edit of this new test and this Change's ledger/state/handoff entries, preserving concurrent user work.

Implemented only the declared Editor test. Its first focused run executed 3 actual cases and failed on tick 24 due to an uninitialized EditMode `LF2ObjectPool`. The fixture was corrected using the existing pool initialization pattern. Its second run generated all 32 rows for each of 3 cases and failed only because a redundant explicit shutdown asserted `Stopped` where the unsealed diagnostic driver returned `Failed`; the test now relies on the helper's own disposal and zero-object/slot/borrower check. The final script compiled in the original Editor but a final NUnit rerun is pending because the local bridge's `get_editor_state` timestamp stopped advancing after that compilation. Keep state `IN_PROGRESS`, do not call it focused-test pass.

The complete 3-case source-model/Unity raw comparison found Naruto action, frame counter and MP identical across all 32 ticks. Two successful transformation cases had all entity rows identical; the too-late case first differed only in generated OID 434 motion X at completed tick 30. Exact scope and next gates are in `artifacts/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/FULL-WINDOW-COMPARISON.md`. Formal EXE visible and real physical Play remain unverified. No battle production, DAT, Scene or camera code changed.

The helper materializes its diagnostic roster before recording the driver's shutdown service owners. After the first 32-tick results exposed that test-only teardown limitation (`unity-renderers-remained-without-an-object-pool-owner`), the new test prepares the existing pool/factory and records their exact private driver owner fields solely for cleanup. It does not alter runtime production behavior. The isolated rerun job `da074b0b1add445e95a6cb62bfac7a80` passed 1/1; the final full class job `c8aa5cebb03e4879aa90beee82ec67f1` passed 3/3. Each final run reached `AwaitingRuntimeMapCleanup` at `ObjectPoolQuiesced`, with objects/slots/borrowers 0; the helper completes final map cleanup in its scope disposal. The earlier failed jobs remain historical fixture evidence. Final `Tools/Validate-ChangeLedger.ps1` passed (716 records, this sole governed code diff covered), original Battle Scene SHA stayed `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`, and DAT/Scene git status was empty. A subsequent original-Editor Play entry never reached a fresh stable Play state, so no physical-key result was claimed; the Editor was returned to fresh idle Edit Mode.
