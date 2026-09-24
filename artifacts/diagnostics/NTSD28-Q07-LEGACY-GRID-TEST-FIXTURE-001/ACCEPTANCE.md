# Q07 legacy-grid test fixture acceptance

Scope: `Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleSpriteGridSeparatorEditorTests.cs` only. Old Naruto/Sasuke BMP paths were replaced by an in-memory 800×560 grid/gutter fixture. Production legacy BMP and formal PNG paths were not changed. No resource was deleted.

Observed in the existing original Unity 2022.3.62f3 Editor on 2026-09-24:

- `refresh_unity(scope=all, force=true, compile=request)` returned to idle after domain reload. No second Editor was launched.
- EditMode job `91c3e05b90844d64ae160d2c24bd8003`: total 3, passed 3, failed 0. Tests: `LegacySheet_DeclaredGuttersBecomeTransparentWithoutChangingFrameContent`; `GridClear_DoesNotApplyGlobalGreenKeyInsideSpriteContent`; `FormalNar_ProductionStageCatalogAndGpu_PreserveNativeAlpha`. The latter loaded formal `c/nar/nar.png` and reported Direct3D11 GPU match with five samples.
- `rg` in the changed test found no `naruto_0.bmp` or `sasuke_0.bmp` literal.
- Saved Scene SHA-256: `NTSD_Battle.unity` = `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`; `NTSD_Menu.unity` = `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. Both equal the prior saved baseline. `git diff --check` passed.
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` reported `Change ledger validation PASSED`, exit code 0 (781 records; unrelated historical no-current-diff warnings remain).

The earlier 383-image reference CSV predates this fixture change. It must not be read as current zero-reference proof. The synthetic fixture retains generic gutter algorithm checks but no longer checks bit-for-bit content in the two old BMPs. Formal PNG content has its separate fresh production/GPU test. Empty-root legacy loading and other old-resource owners remain Q07 work. This package does not authorize deletion or close Q07.
