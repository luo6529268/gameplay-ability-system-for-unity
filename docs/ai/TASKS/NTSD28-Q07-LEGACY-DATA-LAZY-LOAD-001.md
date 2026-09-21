# NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001

Status: VERIFIED_LAZY_LOADING_ONLY. Parent: BATCH-04 / Q07. Authority: D-023 formal Logan DAT and character-image content; current formal candidate publication and retained empty-root legacy path.

Observed before change: `GameDataManager.InitializeSingleton` reads old `Assets/NTSD/Config/data.txt` unconditionally. Direct Battle bootstrap may establish `GameConfig` only in `Start`, after other `Awake` calls, so checking the configured root in the manager's initialization is insufficient. Formal prewarm publishes the Logan object catalog explicitly; empty-root `CharacterAnimtorManager.ParseCharacterFrameConfigs` calls `LoadDataFile(fullDataPath)` explicitly. The old file has no background rows; Q08 owns the separate formal background/result rule.

Exact write scope: remove only the implicit `LoadDataFile()` call from `Assets/NTSD/Scripts/Animation/GameDataManager.cs`; add one focused Editor test of lazy initialization and explicit legacy loading. Update this Task, its Change Record, Ledger, current handoff and Q07 alignment notes. Do not change Scene, GameConfig, legacy resources, nonbattle UI or GAS. No deletion.

Acceptance: Editor compilation has zero new C# errors; focused test proves new manager initialization leaves data unloaded and explicit legacy file loading fills the object table; formal serialized-root Play/Player prewarm still publishes all 330 objects and closes cleanly. Inspect current Editor state before reuse and do not start a second instance on the same Library. Keep the Q08 background-count finding open; this change does not claim formal result-stage parity.

Risk/rollback: a caller depending on an implicit old object table before prewarm may observe no definitions. Search current callers, run focused paths, and restore only this one call if a valid required caller is found; never reset unrelated work. The old file stays present for explicit legacy/editor/test consumers.

Acceptance result: focused lazy-load 1/1 and full formal publication 1/1 passed; serialized-root menu Play passed actual prewarm/World4/ordered close. The final base-only override removal was freshly compiled and focused-tested; full publication/Play were run immediately before that no-op cleanup. See `artifacts/diagnostics/NTSD28-Q07-LEGACY-DATA-LAZY-LOAD-001/ACCEPTANCE.md`. Q07 aggregate and Q08 remain open.
