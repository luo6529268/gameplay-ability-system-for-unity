<!-- CHANGE-RECORD
id: NTSD28-Q07-FORMAL-CHARACTER-DEPLOYMENT-001
status: VERIFIED
change-kind: BATTLE_CONTENT_DEPLOYMENT_TEST
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs
authority: D-023 formal type0 catalog and referenced character images; Q07 old-resource migration
evidence: docs/ai/TASKS/NTSD28-Q07-FORMAL-CHARACTER-DEPLOYMENT-001.md
-->

# NTSD28-Q07-FORMAL-CHARACTER-DEPLOYMENT-001

Before: current test reads old `Config/data.txt`, demands 42 old character encrypted DATs, parses them with the historical decryptor, and demands old BMPs under `Sprite/Character`. The menu item invokes this same historical assertion. This conflicts with current D-023 content authority and prevents exact old-resource retirement.

Planned after: same Editor test/menu command validate current formal/staged type0 catalog rows, parsed frame configurations and referenced VFS images using existing source/catalog/converter APIs. Compare each type0 DAT and each declared selected image with the formal resource bytes, report precise ID/path first difference, and require nonempty type0/image sets. Keep production runtime and all resource files untouched.

Expected side effects: the test no longer requires obsolete 42-BMP deployment; it will detect missing or changed formal type0 DAT/image files in current staged content. No scene or runtime behavior changes. Reversion, if authorized, must target only this test file and preserve other worktree changes. Focused Editor, adjacent identity/PNG, diff and ledger checks are required before claiming verified.

Actual diff: only `CharacterAssetDeploymentEditorTests.cs` changed. The test/menu command now read the selected formal and staged Logan catalogs using the project mode snapshot; for 158 type0 rows they compare registry ID, source path, published folder and DAT SHA, parse through the existing converter, then compare each declared head/small/frame VFS image path and SHA-256, caching repeated image hashes. The old decryptor, 42 constant, data.txt regex, old path checks and menu result text were retired. No production or resource file changed under this ID.

Validation: original running Unity Editor refreshed/compiled; EditMode job `3bc6f024f2784e928d749118b17d4982` passed 3/3 (formal type0 deployment 14.27s, adjacent staged candidate identity, actual formal Naruto PNG GPU alpha). Active Battle Scene `isDirty=false`; saved Scene hashes stable. `git diff --check` and Change Ledger validator results are in the acceptance artifact. Historic `R8-CHARASSET-TEST-001` was valid for its old 42-BMP baseline, but its current-content test semantics are superseded by this ID; no old record was deleted.
