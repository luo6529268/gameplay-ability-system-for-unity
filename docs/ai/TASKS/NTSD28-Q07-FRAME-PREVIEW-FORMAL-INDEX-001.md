# NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001

Status: VERIFIED_SCOPED_PREVIEW_INDEX. Parent: BATCH-04 / Q07. Authority: D-023 formal DAT/character-image source; Q07 current old-image dynamic-owner scan and the user's requirement to preserve existing Unity framework and nonbattle features.

Observed: `CharacterFramePreviewWindow.OnEnable` always calls its private `LoadDataFile`, which reads old `Assets/NTSD/Config/data.txt` into the shared `GameDataManager` even when `GameConfig.BattleContentRuntimeRoot` selects formal Logan content. The window's frame/sprite display uses `CharacterAnimtorManager`; its guidance already asks users to refresh that manager, whose Inspector button now uses formal prewarm. This editor-only old-index read is a real content-owner seam, not a battle-rule authority.

Exact script paths: `Assets/NTSD/Scripts/Animation/Editor/CharacterFramePreviewWindow.cs` (`LoadDataFile` only) and `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07FramePreviewFormalIndexEditorTests.cs` (one formal-root/empty-root focused fixture). No Scene, GameConfig, DAT, image, normal menu UI, battle runtime or GAS modifications.

Implementation: when formal Logan content is configured, opening the preview must not inject the old object index into `GameDataManager`; the window continues to read already-published frames/sprites from `CharacterAnimtorManager`, and its existing refresh guidance now leads to formal prewarm. Empty-root historical authoring still explicitly loads the old index. No new parser or resource copy.

Acceptance: prove a pre-change RED by opening the window with formal root and an unloaded GameDataManager, then post-change formal-root no-old-index and empty-root old-index tests. Original Editor compile with zero new errors; focused tests plus current inspector test; saved Scene hashes stable; ledger validator and diff check. Window preview visual fidelity with formal images remains its separate existing Q07/Q09 gate.

Risk/rollback: a user who opens this editor window with formal root before any formal prewarm sees its existing unloaded-data guidance until they refresh the manager; this is preferable to displaying an obsolete index and follows the currently selected source. On failed validation, correct only the declared files; any rollback follows repository approval and preserves unrelated work.

Acceptance result: `artifacts/diagnostics/NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001/ACCEPTANCE.md`. After two fixture-binding failures were corrected, the pre-change window test exposed formal-root RED 1/2 (`c74a38f8fd9b45fb9d753537aeac8571`: old index loaded), while empty-root stayed GREEN. Post-change `2104a9446b7c4d08b01eca86cc2a4704` passed both 2/2; adjacent Inspector/legacy tests `e5299057e8d4401a9c6a9016dc1f6d6d` passed 2/2. Original Editor compiled, Battle Scene dirty false and both saved Scene hashes stable. Scope is only preview old-index selection, not old-resource retirement or formal-image visual parity.
