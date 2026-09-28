<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-HAN-FAR-CANDIDATE-PLAY-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs
authority: formal NTSD2.8-Logan root EXE far X580 candidate tick and paired playable collision order
evidence: docs/ai/TASKS/NTSD28-Q07-D024-HAN-FAR-CANDIDATE-PLAY-001.md
-->

# NTSD28-Q07-D024-HAN-FAR-CANDIDATE-PLAY-001

Pre-edit `PLANNED`. The Task declares the formal far source, existing near Unity result, the optional request/report-only Editor script path (`Request`, `Report`, `Run`), risk, no-overwrite Play acceptance and rollback. No shared production collision/DAT/Scene/nonbattle code may be changed; current Q07 collision-domain user choice stays pending. The already passed near probe is not rerun. Existing Q09 and Q07 default X520 semantics must remain stable.

2026-09-28 `CODE_WRITTEN`: only the declared optional Editor probe script changed. `Request` gained `leeStartX` (zero/default still X520), `Poll` limits explicit X to 520 or 580, `Report` records the effective X, and `Run` passes it to the existing `CreateCharacter` call. No other input, collector, collision, DAT, Scene or production path changed. Generated Editor compile and one original Battle Play still pending; no far runtime outcome claimed. Rollback exactly these optional fields/guard/argument after reviewing the shared dirty tree.

Pre-run request protection correction: the existing fixed candidate request file contains a prior X520 run record with `requested=false`. Before running X580, the Task was tightened to route only that new far request through its own Temp path and preserve the old file unchanged. This is still the same declared Editor test script; a second original Editor refresh/compile is required before Play. No prior request or result was overwritten.

Follow-up `CODE_WRITTEN`: the same Editor script now adds `FarCandidateRequestPath`, reads it after the legacy candidate path so a requested far run selects its own file, validates X580 and Q07 mode, and uses `currentRequestFile` for consumed-writeback so the old X520 request remains untouched. `Reset` clears that path; consumed Q07 JSON retains `leeStartX`. The existing Q09 and Q07 default request paths and old result files remain as before. This protection edit has not yet been compiled by the original Editor or run; no far outcome claimed.

2026-09-28 `VERIFIED` for this test-only task: generated Editor build 0 errors/191 warnings; same original Editor imported the script and ran one clean Battle Scene X580 Play. In-collection live Han candidate cache=1, complete collision action146 row candidate=1/catch, whereas formal root far candidate=0/no first-tick catch. Raw result `OBSERVED_CACHE_LIMIT` because the predicate view is after collection, not a per-return trace; see `artifacts/diagnostics/NTSD28-Q07-D024-HAN-FAR-CANDIDATE-PLAY-001/ACCEPTANCE.md` and immutable raw JSON SHA `D5ECEAB90B136DC48EC2E3DE47E27B42B0C5544CF2A002BDA4885A90BA1CF8BC`. New request consumed, old X520 request SHA unchanged. Ordered shutdown complete/borrowers0; original Editor idle/non-Play/clean; Battle, Menu, Input Actions and mode Asset hashes stable. The actual changed code is solely `NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs` optional request selection, X parameter/report and consumed-path reset. No production/DAT/Scene/nonbattle change. Q07/BATCH-04 still open; collision domain awaits the user's pending answer. Rollback remains the task's exact optional probe additions after reviewing the dirty worktree, without touching existing reports or unrelated edits.

Post-run governance: `Tools/Validate-ChangeLedger.ps1` exited 0, `Change ledger validation PASSED` with 951 records and 18 governed code files in the dirty diff; this task's optional Editor script is covered. Full output is `artifacts/diagnostics/NTSD28-Q07-D024-HAN-FAR-CANDIDATE-PLAY-001/ledger-validation.txt`; its historical-record warnings do not fail validation. `git -c core.safecrlf=false diff --check` exited 0. No full self-check was rerun because this task changed only an optional diagnostic and did not alter production logic.
