<!-- CHANGE-RECORD
id: NTSD-MENU-FONT-3500-REBIND-001
status: PLANNED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/MenuCarouselVisualEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/MenuFont3500MigrationEditor.cs
authority: User 2026-10-03 explicitly requests replacing all GameObjects using the 0480 SDF font asset with the 3500 SDF font asset
evidence: docs/ai/TASKS/NTSD-MENU-FONT-3500-REBIND-001.md
-->

# NTSD-MENU-FONT-3500-REBIND-001

Pre-change: Assets/NTSD/Scene/NTSD_Menu.unity is already changed by other work and has one existing 3500 label. Its current SHA is E58B80DE9F008C488CE32A322B7B8FCBFF406303423D360C0C23FA8D48C668E9. It has 25 old-font TMP labels, 21 old-font material references and one old-font input field global font reference. New 3500 SDF asset is untracked and currently contains only four glyphs; source TTF has broader Chinese coverage. The old 0480 SDF asset is modified, and its TTF/.meta are already deleted in the worktree, outside this task.

Intended code: MenuFont3500MigrationEditor exposes a guarded one-shot Editor MenuItem. It resolves the exact two font paths and active clean NTSD_Menu scene; collects the old-font labels and matching input fields; first checks/populates the new font atlas for supported current label characters, then switches only affected labels to the target font/default material while preserving scene-local styled materials by rebinding their atlas. It writes only the target SDF via SaveAssetIfDirty and NTSD_Menu via SaveScene. It refuses an unexpected reference count or scene state. MenuCarouselVisualEditorTests.NewLabel changes its test font GUID to the target GUID. No runtime code outside the existing scene asset changes.

Side effects: persistent target SDF glyph table/atlas expansion and existing Menu scene TMP/input-field/material references. The one pre-existing 3500 label remains bound. Scene-local material color/outline configuration is preserved. Latin digits/fullwidth parentheses absent from target TTF are handled by existing fallback paths where available; acceptance verifies actual display and reports gaps.

Acceptance: new font GUID is assigned to all 26 intended TMP labels and input field; no old font GUID remains in serialized scene/prefab/component references, and no other scene/font asset is modified by this task. Editor compile 0 error, focused Menu tests and actual Menu Play render/exit. Exact before/after SHA, reference counts, fallback glyph status and Unity Editor outcome recorded.

Rollback: use the exact pre-change backups and SHA in artifacts/diagnostics/NTSD-MENU-FONT-3500-REBIND-001/prechange-manifest.json only after checking current content has not changed further; preserve the user's already changed Menu scene and target asset. Do not restore HEAD or old 0480 assets. Shared governance documents require forward correction, not whole-file backup replacement.

Implementation and verification results will be appended.
