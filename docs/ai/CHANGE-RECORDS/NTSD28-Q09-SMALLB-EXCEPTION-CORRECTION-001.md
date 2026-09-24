<!-- CHANGE-RECORD
id: NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/UI/CharacterUIResourceManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11VisualContentCandidateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedPublicationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KillIconInputIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09WordsInputIdentityEditorTests.cs
authority: User-approved complete native HUD exclusion in alignment P-17 and decisions ledger
evidence: docs/ai/TASKS/NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001.md
-->

# NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001

Pre-change: smallb files are 1,010/1,010 deployed with formal bytes, but previous package added runtime input/decode/publication for an excluded native HUD. Project `HeadImg` and user `HUDBg x30` are unchanged. Re-establish the user scope by surgically removing only that package's smallb-only code and restoring 906-image candidate assertions. Keep independently valid test use of `ProjectBattleModeConfig` because native mode DAT remains excluded. Exact scope, invariants, validation and rollback are in Task Contract; current dirty tree remains protected.

Post-change: Removed only the superseded smallb candidate, prewarm, publication and portrait API additions. Production candidate remains the 906 file/head/small image union. The 104 separate smallb images remain byte-identical on disk, but have no production HUD consumer. Retained the independent ProjectBattleModeConfig snapshot changes in tests because both original mode DATs are user-excluded. No DAT, image, Scene, Prefab or ProjectSettings edits were made by this correction. The original Battle HUD, HeadImg and user HUDBg x30 remain unchanged.

Validation: original Unity Editor refresh and Console showed zero compile errors; focused EditMode job 52c98c44e9c74c49a07162c70faca838 passed 5/5, including 330-object publication/recycle and adjacent kill-icon/WORDS checks; candidate job 27a96589503945deac8a62efc9947165 passed 7/7. Battle Scene SHA-256 9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39 and Menu Scene SHA-256 3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228 match pre-correction baselines. `Tools/Validate-ChangeLedger.ps1` passed (784 Records, 21 current code diffs covered), and `git -c core.safecrlf=false diff --check` passed. See `artifacts/diagnostics/NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001/ACCEPTANCE.md`.

Limit: this verifies the scope correction and focused regressions, not full Q09 or full battle parity. No native HUD Play/pixel comparison was pursued because that consumer is user-excluded. Rollback is a review of this package's exact code diff against the superseded package, preserving all independent worktree changes; no destructive Git operation was used.
