# NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001

Status: VERIFIED_SCOPED_INSPECTOR_ENTRY. Parent: BATCH-04 / Q07. Authority: user D-023 formal DAT/character-image content, with explicit native background/mode DAT exceptions; current configured content root and Q07 old-image owner scan.

Observed: `CharacterAnimtorManager.RefreshAllData()` Inspector action unconditionally invokes legacy `ParseCharacterFrameConfigs` and `LoadCharacterSpritesAsync`, even when `GameConfig.BattleContentRuntimeRoot` selects the formal Logan root. Normal `LoadingPrewarmController` already uses `PrewarmConfiguredLoganContentAsync` for that root; explicit empty-root legacy loading remains a supported historical branch. The Inspector action is a battle-content authoring entry and is not a rule authority.

Exact script scope: `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs` (`RefreshAllData` only) and `Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCacheCallerEditorTests.cs` (one focused fixture case). Documentation scope: this Task, Change Record/Ledger, Q07 alignment, state/handoff and acceptance report. No DAT, image, Scene, Prefab, GameConfig, normal menu UI, GAS, or battle tick modification.

Implementation: in the Inspector delayed callback, if the configured formal root is nonempty, await the existing scoped formal prewarm and return; otherwise retain the old explicit parse/sprite path. Do not bypass the native content boundary or add a second candidate loader.

Acceptance: original Editor compile with zero new C# errors; focused test invokes the actual Inspector method with a selected tiny formal candidate and observes its published source key and formal object/image values, proving the old branch did not replace it; adjacent source caller tests remain green. Verify Scene saved hashes and `git diff --check`, run ledger validator. This only closes Inspector's default-root path; empty-root legacy and historical tests still block old-resource retirement.

Risk/rollback: formal prewarm rejects an active battle or noncurrent owner; that is the existing production boundary and should surface as an Editor error rather than loading old content silently. Roll back only this conditional branch and its focused test through the repository's approved revert process; do not reset unrelated work.

Acceptance result: original Editor compile completed with zero observed C# errors. Actual-button test first RED (`6e0333854715472dab260ba556765284`, expected formal key / actual null) with the original legacy-only button. After the conditional branch, exact button plus adjacent direct caller job `5b37372a4480401aa9a4fb0ccf27d8d7` passed 2/2; explicit legacy lazy-load job `f9fa73b6383a4f51be1cf102d4bc91be` passed 1/1. Saved Battle/Menu Scene hashes remained unchanged. Evidence and limits: `artifacts/diagnostics/NTSD28-Q07-INSPECTOR-FORMAL-REFRESH-001/ACCEPTANCE.md`.
