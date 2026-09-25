<!-- CHANGE-RECORD
id: NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_SCENE_BINDING
code-path: Assets/NTSD/Scripts/UI/Battle/BattleHudView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleComboView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleUiContracts.cs
authority: Current user clarification that NTSD_Battle UI is scene-resident and default-visible; BattleControlsView logic is deferred to the user
evidence: docs/ai/TASKS/NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001.md
-->

# NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001

Pre-change: the previous UI package described the scene views as a staging point for a later TEngine `UIWindow` and left the ComboPanel references unassigned. The user clarified that the `NTSD_Battle` Canvas remains in the scene and is shown by default, so this UI does not need prefab loading or `UIModule` ownership.

Required change: keep the current scene-bound `MonoBehaviour` views for the resident HUD, make HUD/Combo visibility behavior explicit, and bind the existing ComboPanel Image children. `BattleControlsView` is intentionally outside the behavior change until the user provides its input rules.

Expected side effects: `BattleComboView` receives the serialized Image references for `ComboBg`, `ComboTemp` and `ComboArraw`. No hierarchy, sprite, anchor, camera or default scene activation changes are intended.

Non-goals: TEngine source or prefab integration, dynamic window lifecycle, button/input dispatch, BattleBootstrap changes, runtime snapshot source wiring, or other scene changes.

## Implementation and validation

- `BattleHudView` remains a scene-bound `MonoBehaviour`; null snapshots leave the default scene UI unchanged, while an applied state explicitly controls the root visibility and updates the bound head/HP/preview/MP controls.
- `BattleComboView` remains a scene-bound `MonoBehaviour`; null snapshots leave the default scene UI unchanged, while an applied state explicitly controls the ComboPanel visibility and optional text.
- `BattleUiContext` documentation now distinguishes future dynamic TEngine UI from the current scene-resident HUD.
- `NTSD_Battle.unity` binds `comboBackgroundImage` to Image fileID `278066489` (`ComboBg`), `comboContentImage` to Image fileID `521647144` (`ComboTemp`), and `comboArrowImage` to Image fileID `833747954` (`ComboArraw`).
- `BattleControlsView.cs` and its scene references were not changed.
- `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly` passed with 0 errors and 65 warnings.
- Focused scene-reference check passed as `SCENE_HUD_COMBO_BINDINGS_PASS`.
- `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location)` passed; pre-existing historical-record warnings remain.
- Unity Editor import, actual Canvas rendering and Battle Scene Play remain pending.

Rollback is limited to the files and three scene reference fields declared above.
