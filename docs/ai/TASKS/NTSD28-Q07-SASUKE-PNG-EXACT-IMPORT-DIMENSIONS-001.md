# NTSD28-Q07-SASUKE-PNG-EXACT-IMPORT-DIMENSIONS-001

Status: VERIFIED_EXACT_IMPORT_DIMENSIONS_ONLY. Dependency for `NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001`.

Observed RED: focused Unity EditMode job `e11d6c4c95ed40a39a837b14cc20bf5a` failed because `AssetDatabase.LoadAssetAtPath<Texture2D>` returns width 1024 for formal `sasu.png` whose raw PNG width is 799. The current meta has `nPOTScale: 1`. Parent preview rebinding must not use importer-resized dimensions to calculate authored 79×79 frame0 geometry. Failure evidence: `artifacts/diagnostics/NTSD28-Q07-SASUKE-EDITOR-PREVIEW-FORMAL-IMAGE-001/IMPORTER-RED.json`.

Exact write scope: only `Assets/NTSD/Content/LoganRuntime/vfs/c/sasu/sasu.png.meta`, change `nPOTScale` from `1` to `0` so the authoring preview sees the formal raw dimensions. Preserve meta GUID `b5d608d7fe42c474ea0f2bb728a122a2`, all other importer fields and PNG bytes. Do not modify other formal or old images, code, Scene, GameConfig, HUD or Menu. Prechange meta SHA-256: `5AE12859AC73499E3E0164464FB771096A84D7CC95E3E4F5EA381EF3E1874EF6`.

Validation: Unity reimport and focused `FormalSasukePng_ImportedSizeAndFirstCellMatchRawData` must show 799×960 imported/raw and rect (0,881,79,79); formal PNG SHA remains `2AEB4FF667A9EE63E60EE0E866E4700C780974746D47E231CAEF1EAFCAD7C694`; Scene hashes unchanged. If Unity still resizes or another rendering difference appears, report it and do not widen this Task. The parent preview Task resumes only after this contract passes.

Rollback: exact meta field reversal only under repository restore approval rules; no reset/clean. No computer-use.

Result: only `nPOTScale: 1→0` changed in the named meta. After Unity refresh, focused EditMode job `3fc9eefefb1142ba9dccfce8b23ef253` passed 1/1, establishing imported/raw 799×960, formal pic0 rect (0,881,79,79) and nonempty opaque/transparent cells. RED and PASS JSON are preserved beside the parent acceptance artifacts. This closes the importer dependency only; the preview scripts and Scene still require their own validation.
