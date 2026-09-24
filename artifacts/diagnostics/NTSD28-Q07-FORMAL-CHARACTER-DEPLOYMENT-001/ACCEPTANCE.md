# Q07 formal type0 character deployment acceptance

Scope: `Assets/NTSD/Scripts/Test/Editor/CharacterAssetDeploymentEditorTests.cs` only. No production, DAT, image, Scene, Prefab, mode Asset or nonbattle file changed under this package; no old resource deleted.

Authority and finding: the root formal `NTSD2.8-Logan.exe` SHA-256 is `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The formal and project-staged `catalog.csv` each have 158 `object/type0` rows; the previous test demanded 42 historical Unity DAT/BMP assets. The new test enumerates actual formal type0 rows and checks all selected staged DAT identities, parsed frame counts, declared head/small/frame image paths and image SHA-256 against the formal runtime, with a per-test hash cache. It uses the project mode Asset snapshot and does not consume original background or mode DAT.

Validation in the existing original Unity 2022.3.62f3 Editor on 2026-09-24:

- `refresh_unity(scope=all, force=true, compile=request)` returned idle after domain reload. No second Editor launched.
- EditMode job `3bc6f024f2784e928d749118b17d4982`: 3 total, 3 passed, 0 failed, 46.7 seconds. The focused `FormalTypeZeroCharacterDatAndImagesAreDeployed` passed in 14.27 seconds. Adjacent `StagedCandidateMatchesFormalRuntime` and `FormalNar_ProductionStageCatalogAndGpu_PreserveNativeAlpha` passed; the latter loaded formal `c/nar/nar.png` and reported Direct3D11 five-sample GPU match.
- The changed test has zero old `Config/Character`, `Sprite/Character`, `.bmp`, or 42-count references. The active `NTSD_Battle` Scene reported `isDirty=false`.
- Saved Scene SHA-256 unchanged: `NTSD_Battle.unity` = `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`; `NTSD_Menu.unity` = `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. `git diff --check` passed.
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` reported `Change ledger validation PASSED`, exit code 0; 782 records and 17 governed code files in the current workspace diff. Its unrelated historical no-current-diff warnings do not change this package's focused result.

Limit: this proves the selected current formal type0 content is deployed byte-for-byte where the parsed contract references it. It does not prove all old assets are unreachable, that every skill renders identically in Play/Player, or that Q07 is closed. The empty-root legacy loader and other old-content owners remain. No deletion is authorized by this test.
