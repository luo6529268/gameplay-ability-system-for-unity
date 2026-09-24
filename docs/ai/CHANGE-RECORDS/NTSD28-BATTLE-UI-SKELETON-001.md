<!-- CHANGE-RECORD
id: NTSD28-BATTLE-UI-SKELETON-001
status: SUPERSEDED
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleUiContracts.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleCharacterSlotView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleMainUIView.cs
authority: Current user request limited to NTSD_Battle UI skeleton; existing NTSD input/runtime ownership and TEngine UI lifecycle design are integration boundaries
evidence: docs/ai/TASKS/NTSD28-BATTLE-UI-SKELETON-001.md
-->

# NTSD28-BATTLE-UI-SKELETON-001

Pre-change: `NTSD_Battle` contains a serialized Canvas with static HUD, character-card and control-image objects, but no battle-specific UI controller or data-binding contract. `BattleBootstrap` currently has temporary Canvas enable/camera handling; this package must not expand that temporary ownership. The current repository does not contain TEngine source or a TEngine UPM dependency, so directly inheriting the new view from TEngine `UIWindow` would make the project depend on an unregistered external runtime before the user creates the UI Prefab.

Bounded implementation: add a small presentation-only contract, a reusable character-slot view, and a root battle UI view. The root accepts a context and reusable snapshot, applies static role data and dynamic HP/MP/input visual state to explicitly assigned references, and exposes explicit clear/validation methods. No scene, prefab, input action, DAT/content, battle runtime, `BattleBootstrap`, or other-scene change is included.

Expected side effects: none until a user-created `BattleMainUI` Prefab has these components attached and an external UIModule bridge calls `Bind`/`Refresh`. The new code must tolerate optional unassigned visual references while the user is constructing the UI.

Non-goals: importing all of TEngine, resource-module replacement, automatic hierarchy generation, direct character control from UI, raw `Input.GetKey` polling, touch input, HP/MP source discovery, or visual design decisions.

Acceptance: source review, current available compile path or exact dependency blocker, `git diff --check`, and `Tools/Validate-ChangeLedger.ps1`. Runtime UI acceptance remains pending the user-created Prefab and actual TEngine/UIRoot integration.

Rollback: review the exact three new script files and remove only those files if separately required; preserve unrelated dirty docs, diagnostics, scenes, assets and scripts.

## Actual change and validation

- Added `Assets/NTSD/Scripts/UI/Battle/BattleUiContracts.cs`: reusable eight-slot snapshot contract, `IBattleUiSnapshotSource`, and TEngine `UserData`-compatible `BattleUiContext`.
- Added `Assets/NTSD/Scripts/UI/Battle/BattleCharacterSlotView.cs`: explicit uGUI Image/TMP/GameObject references for role card static data, HP/MP bars, and held-input indicators.
- Added `Assets/NTSD/Scripts/UI/Battle/BattleMainUIView.cs`: root binding, refresh, clear, slot ordering, and missing-reference inspection. It does not read raw input or access `BattleBootstrap`.
- Added the Unity folder metadata for `Assets/NTSD/Scripts/UI/Battle/`.
- A temporary .NET compile using local Unity/TMP/uGUI type stubs passed (`STATIC_CSHARP_SYNTAX_PASS`). This validates C# syntax and the referenced member shapes only; it is not a Unity assembly compile.
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location)` passed. The validator emitted pre-existing warnings for older Records whose declared files are not in the current diff; the three UI files were covered by this Record.
- Unity assembly compilation, Prefab import, TEngine `UIModule` open/close, and `NTSD_Battle` Play Mode UI verification remain pending. Four Unity processes were already active, so no second Editor was started against the same Library.

Current status: `SUPERSEDED` / replaced by `NTSD28-BATTLE-UI-HUD-REWORK-001` after the user supplied the actual single-HUD scene structure.

## Superseded correction

The user reviewed the eight-slot abstraction and supplied a revised `NTSD_Battle` HUD layout with one character panel, one combo panel, and one action-control panel. This Record is superseded by `NTSD28-BATTLE-UI-HUD-REWORK-001`; the previous three scripts are retired from the active implementation and the new Record owns the replacement views.
