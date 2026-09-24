<!-- CHANGE-RECORD
id: NTSD28-Q09-SMALLB-PUBLICATION-001
status: SUPERSEDED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/UI/CharacterUIResourceManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11VisualContentCandidateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedPublicationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KillIconInputIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09WordsInputIdentityEditorTests.cs
authority: Formal playable HUD smallb-first fallback and D-023 formal character images
evidence: docs/ai/TASKS/NTSD28-Q09-SMALLB-PUBLICATION-001.md
-->

# NTSD28-Q09-SMALLB-PUBLICATION-001

Pre-change: formal Q01 indexed image audit has 1,010 distinct pictures, but current Unity candidate captures 906 file/head/small pictures and omits 104 distinct smallb pictures. Native metadata already retains the original smallb field. The project Battle Scene currently holds a static Naruto small portrait, not the formal smallb byte content; HUD consumer is a later package. This change adds smallb candidate identity, decode, atomic UI-resource publication and fallback without changing selection UI or Scene. Exact paths, invariants, validation and rollback are in the Task Contract. Existing worktree edits are preserved.

Expected side effects: candidate visual fingerprint changes when smallb bytes change; formal/staged identity remains equal, candidate image count becomes 1,010. Additional portrait Sprite/Texture ownership must retire with existing publication. No combat logic, DAT, Scene or menu action changes.

Post-change: `LoganVisualContentCandidate` now includes smallb paths in its verified input set; `CharacterAnimtorManager` decodes them into publication-owned Sprites and publishes the separate battle-portrait field through `CharacterUIResourceManager`. Existing head/small UI fields remain unchanged; absent smallb falls back to small. Five existing focused fixtures were updated to the 1,010-image identity and independent project mode Asset provenance. One new synthetic stale-smallb hash case and a 330-object with/without-smallb publication assertion were added. Actual code paths are exactly the eight header paths; no Scene, DAT or resource edit.

Original Editor compile/console 0 errors; fresh focused jobs 5/5 (`80a2799be5d0460a94b28876aa9e5f51`), 330-object publication/recycle 1/1 (`5e53239b5cd64879bd8821b21beb2881`), candidate class final 8/8 (`5759979d39f3409d9eb2dbd850056050`). Earlier path-comparison test failure, old Temp fixture missing failure and 0-test job are retained in `artifacts/diagnostics/NTSD28-Q09-SMALLB-PUBLICATION-001/ACCEPTANCE.md`; all were resolved without production changes. Ledger validator PASS, diff check PASS, saved Battle/Menu Scene SHA unchanged. Status is limited `FOCUSED_TEST_PASS`: no HUD screen consumer or Play pixel parity yet.

2026-09-24 supersede correction: the above tests were valid at the time, but this package interpreted an explicitly excluded domain as required. Alignment §0.2/§1.2/P-17 and `docs/ai/DECISIONS.md` establish complete native character HUD as `USER_EXCLUDED`. The added smallb candidate/decode/publication API is being surgically withdrawn by `NTSD28-Q09-SMALLB-EXCEPTION-CORRECTION-001`, preserving the pre-existing content and unrelated worktree. The historical evidence remains recorded; this Record is `SUPERSEDED`, not a live Q09 acceptance gate.
