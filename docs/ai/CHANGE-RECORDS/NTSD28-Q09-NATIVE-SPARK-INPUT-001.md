<!-- CHANGE-RECORD
id: NTSD28-Q09-NATIVE-SPARK-INPUT-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11VisualContentCandidateEditorTests.cs
authority: NTSD 2.8-Logan formal playable spark resource and system geometry, BATCH-05 Q09
evidence: docs/ai/TASKS/NTSD28-Q09-NATIVE-SPARK-INPUT-001.md
-->

# NTSD28-Q09-NATIVE-SPARK-INPUT-001

Pre-change: formal spark resource and dimensions are not captured in `LoganVisualContentCandidate` identity; the common publisher remains legacy SPARK.bmp. User-approved full native HUD exclusion is unrelated; global hit spark is in-scope battle presentation. Exact scripts, invariants, validation and rollback are in the Task Contract. No DAT or image bytes will be changed.

Post-change: `NativeSparkInput` captures resource index 43 through the existing resource-table tokenizer, resolves its VFS PNG, scans `system.dat` scalar fields for positive `spark_w`/`spark_h`, and fingerprints both raw DAT inputs plus selected path, PNG SHA and dimensions. The parent candidate includes that fingerprint in V4 visual/source-cache identity and recaptures it at the publication freshness gate. Both inputs absent in minimal historical fixtures retain the prior optional candidate behavior; one missing input or invalid dimension fails closed. The original 906 file/head/small image list remains unchanged. No publication, renderer, logic lifecycle, DAT or image bytes were edited.

Validation: original Unity Editor refresh and Console yielded 0 errors. Candidate class job `27f49f7658644e7cb13abf648d2dc993` passed 8/8; after adding a resource-index selection mutation, exact test job `53f3aafaae7e4d6f9f57546f69d6d864` passed 1/1. Staged/formal candidate identity job `4325246fb0434cb3a36a1af3ec5f33f7` passed 1/1; an initial wrong-namespace request returned 0/0 and is not treated as evidence. Formal candidate asserted SPARK path, 99x79 and PNG SHA 15D8843E0CE87FF63F46DFF7170D30C23BAEA0F2799434B26717AADFD5EC881B. Battle/Menu Scene SHA stayed at 9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39 / 3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228. `Tools/Validate-ChangeLedger.ps1` passed, 785 Records and 21 code files covered; `git -c core.safecrlf=false diff --check` passed. See acceptance report under the same diagnostic ID.

Limit: this closes only immutable formal SPARK input identity. Formal PNG decode/publication, raw-ID geometry, legacy and central render consumers, 30/60/120 presentation and original-EXE pixel parity remain Q09/R14/R17 work. Rollback requires review of only this package's two-script diff while retaining unrelated worktree changes.
