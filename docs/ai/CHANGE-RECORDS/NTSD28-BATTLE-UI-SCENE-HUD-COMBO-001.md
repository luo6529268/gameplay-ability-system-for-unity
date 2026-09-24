<!-- CHANGE-RECORD
id: NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001
status: IN_PROGRESS
change-kind: CODE_AND_SCENE_BINDING
code-path: Assets/NTSD/Scripts/UI/Battle/BattleHudView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleComboView.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleUiContracts.cs
code-path: Assets/NTSD/Scene/NTSD_Battle.unity
authority: Current user clarification that NTSD_Battle UI is scene-resident and default-visible; BattleControlsView logic is deferred to the user
evidence: docs/ai/TASKS/NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001.md
-->

# NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001

Pre-change: the previous UI package described the scene views as a staging point for a later TEngine `UIWindow` and left the ComboPanel references unassigned. The user clarified that the `NTSD_Battle` Canvas remains in the scene and is shown by default, so this UI does not need prefab loading or `UIModule` ownership.

Required change: keep the current scene-bound `MonoBehaviour` views for the resident HUD, make HUD/Combo visibility behavior explicit, and bind the existing ComboPanel Image children. `BattleControlsView` is intentionally outside the behavior change until the user provides its input rules.

Expected side effects: `BattleComboView` receives the serialized Image references for `ComboBg`, `ComboTemp` and `ComboArraw`. No hierarchy, sprite, anchor, camera or default scene activation changes are intended.

Non-goals: TEngine source or prefab integration, dynamic window lifecycle, button/input dispatch, BattleBootstrap changes, runtime snapshot source wiring, or other scene changes.

## Implementation and validation

Implementation is in progress. Validation results will be appended after the scoped script and scene changes are complete.

Rollback is limited to the files and three scene reference fields declared above.
