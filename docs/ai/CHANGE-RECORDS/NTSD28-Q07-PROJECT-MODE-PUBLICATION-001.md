<!-- CHANGE-RECORD
id: NTSD28-Q07-PROJECT-MODE-PUBLICATION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LoganModeComboInput.cs
code-path: Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/LoganVisualContentCandidate.cs
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ProjectModePublicationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCacheCallerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B11SourceCallerPlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07StagedCandidateIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KindIdentityPublicationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ModeComboPublishedActivationEditorTests.cs
authority: 2026-09-24 user exclusion of native mode DAT and request for independent Unity ProjectBattleModeConfig.asset
evidence: artifacts/diagnostics/NTSD28-Q07-PROJECT-MODE-PUBLICATION-001/TASK-CONTRACT.md
-->

# NTSD28-Q07-PROJECT-MODE-PUBLICATION-001

Before code: production configured prewarm calls `LoganVisualContentCandidate.Capture(source)` on a worker. `LoganObjectCatalog.Read` then reads native `data/mode.dat` and its selected child; identity, combo first tick and KO feed/icon/audio receive these values. The new project Asset is present and focused-tested but not wired. This change passes an immutable main-thread snapshot into the worker and reuses the current published input shape while replacing native DAT provenance. Exact paths, expected effects, rollback and acceptance are in the Task Contract. No Scene, GameConfig or DAT data is authorized to change.

Actual production diff: `CharacterAnimtorManager` captures `ProjectBattleModeConfig.asset` on the main thread, compares its fingerprint before cache reuse and publication, and passes a plain immutable snapshot to worker candidate capture. `LoganObjectCatalog` selects the project snapshot factory rather than native `mode.dat`; content identity includes its fingerprint. `LoganVisualContentCandidate` recaptures with that same snapshot for freshness. The existing first-tick combo and KO/icon/audio consumers retain their published shape. The native parser remains only for explicit historical/diagnostic no-snapshot calls; after the user-approved reversible move, the original mode DATs and their metas are no longer in Assets. `NTSD28B11SourceCacheCallerEditorTests` now makes its candidate with the production snapshot; the formal staged Play probe now checks project Asset provenance instead of an obsolete fixed native-mode identity and allows the formal content path a real-time loading deadline.

Verification: original Editor compiled with 0 observed script errors. Initial new focused test used unsupported async Task NUnit signature and failed, then corrected to synchronous worker Task wait; final job `476eaf649cd04938ba403e144920e5e4` 1/1 PASS including first-tick tuple. Existing source caller tests first failed on obsolete direct-native candidate identity, then fixture was aligned to production and job `61c39b5cca0149b791911c58445d0834` 2/2 PASS. After moving all excluded DAT/script inputs out of Assets, job `e4f049c0112149b791911c58445d5f` 3/3 PASS. Fresh standard `Temp/NTSD_BattleRuntimeSelfCheck.result` PASS at 2026-09-24 08:45:57Z. Real Battle Scene Play with a temporary content root containing no native mode DAT: `q07-project-mode-direct-1.json` PASS, World4, same published keys, ordered shutdown, borrowers/survivors0. First formal staged Play timed out at the probe's old 1800-frame limit while still Preparing and is retained as FAIL; after a bounded 240-second real-time wait change, `q07-project-mode-formal-2.json` PASS. Final post-move formal staged Play `q07-project-mode-no-native-dat-1.json` PASS, World4, same keys, RuntimeMapCleared, stopped after two frames, borrowers/survivors0. Battle/Menu saved Scene SHA unchanged. `Tools/Validate-ChangeLedger.ps1` and `git diff --check` pass. This is `VERIFIED` for configured production mode-DAT replacement; native combo/KO event fidelity remains governed by their own Q08/Q09/Q10/Q12 gates and is not closed here.

Final adjacent fixture check after original mode DAT retirement: the three declared Q07 staged/formal identity fixtures now pass the project snapshot into both current candidates while retaining the native formal hash as a historical vector. The original Editor refreshed and compiled, then exact EditMode job `c82592c1ad694d3dbf5cb4e358ab36af` completed 3/3 PASS (staged candidate, seven-component identity, published mode combo). This closes the fixture migration in this Record; diagnostic no-snapshot calls and Q09/Q10 event-level tests remain separately scoped.
