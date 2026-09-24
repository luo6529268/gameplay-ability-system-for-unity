<!-- CHANGE-RECORD
id: NTSD28-Q07-PROJECT-MODE-CONFIG-ASSET-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/ProjectBattleModeConfig.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07ProjectBattleModeConfigEditorTests.cs
authority: 2026-09-24 user scope correction and request for independent Unity mode ScriptableObject
evidence: artifacts/diagnostics/NTSD28-Q07-PROJECT-MODE-CONFIG-ASSET-001/TASK-CONTRACT.md
-->

# NTSD28-Q07-PROJECT-MODE-CONFIG-ASSET-001

Before code: native `data/mode.dat` and `data/mode/ntsd.dat` currently feed `LoganModeComboInput`, content identity and battle combo/KO consumers. `GameModeConfig` contains only IDs; no project-owned asset represents the currently consumed mode values. The user wants a new ScriptableObject asset independent of GameConfig. This package establishes its schema and initial serialized asset, without switching production callers. Exact scripts are declared in the header; the new asset will live at `Assets/NTSD/Resources/ProjectBattleModeConfig.asset` with a stable GUID. Expected side effects are only new script/test/asset files and metas, no Scene or battle behavior change. Acceptance and rollback are in the Task Contract. The next package owns production detachment and must not treat this schema package as completion.

Actual diff: added `ProjectBattleModeConfig.cs`, a Unity `Resources` ScriptableObject asset and metas, plus one focused Editor test. The asset serializes only the currently consumed combo/KO fields, initialized to the current project-visible baseline so creating it alone does not change behavior; the user can edit its values later. `Capture` validates the seven image slots, copies arrays to a plain immutable snapshot and hashes every field deterministically. The test verifies serialized lookup, field values and clone isolation. No production caller currently reads the new asset. Original Editor compile, focused test and Scene hash validation pending; status `CODE_WRITTEN`.

2026-09-24 acceptance correction: the prior paragraph describes the state immediately after asset creation. Original Editor imported and compiled it; exact focused job `1fbeb914d2e94be580db8c92b60a879f` 1/1 PASS, and after original-DAT move job `e4f049c0112149b791911c58445d5f` included it in 3/3 PASS. Production now uses the asset through `NTSD28-Q07-PROJECT-MODE-PUBLICATION-001`; formal-content Play without the original mode DAT passed in `q07-project-mode-no-native-dat-1.json`. Scene disk SHA remained Battle `9409F2BCFE3E657A6C3C88A7527045CC384D50AAACC99197D53AACA38F3B3A39`, Menu `3B0F58AA88BEC495AA999D014CB2779E935B21F0374826357B4DC64AE5B80228`. This Record is `VERIFIED` only for the asset schema and independent snapshot, not all mode behaviors.
