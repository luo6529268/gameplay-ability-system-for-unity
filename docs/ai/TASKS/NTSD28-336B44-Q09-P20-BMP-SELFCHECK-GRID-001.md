# NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001

Status: `FOCUSED_TEST_PASS / FULL_SELFCHECK_NEXT_FAILURE`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / Q09 / P-20`; the Q07/F02 full-self-check gate is the immediate trigger.

The original Editor full `BattleRuntimeSelfCheck` fails in `CheckBattleSpritePrewarmTransactionContracts` before reaching the F02 weapon branch. Its three synthetic `.bmp` sheets expect legacy declared-range indexing, but their direct calls to `BuildIndexedSpriteRects` omit the `allowBeyondGridCapacity: true` argument introduced by `NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001`. The production BMP loader already passes `true`; formal Logan PNGs use the capacity guard. The failing full-run result is preserved in the F02 diagnostics folder.

Own only the three BMP fixture call sites in `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` plus this package's governance/evidence files. Preserve the Logan PNG guard, production loading, all DAT/image bytes, Scenes, configuration, and nonbattle code. Acceptance: the synthetic BMP contract no longer fails, original Editor compilation has zero C# errors, full self-check result is recorded honestly, the existing P-20 focused test still passes, and Ledger/diff and protected asset hashes are checked. If a later full-self-check assertion fails, report that next first failure rather than claim full PASS. Rollback is only the three fixture argument additions after review of the current dirty file.

Observed: original Editor P-20 exact category 2/2 PASS; fresh full self-check reached `R3-AI-LIFE-01` after the BMP assertion. See the diagnostics report. Full self-check remains FAIL; Q07/F02 and Q09 remain open.
