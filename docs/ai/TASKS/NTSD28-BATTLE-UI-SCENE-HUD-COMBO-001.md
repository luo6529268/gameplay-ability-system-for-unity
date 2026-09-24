<!-- TASK
id: NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001
status: IN_PROGRESS
-->

# NTSD28-BATTLE-UI-SCENE-HUD-COMBO-001

## User requirement

The `NTSD_Battle` Canvas is a scene-resident UI that is enabled by default. It will not be converted into a TEngine-loaded prefab. The current task is to finish the scene-bound `BattleHudView` and `BattleComboView`; the user will provide the implementation rules for `BattleControlsView` separately after this task.

## Bounded scope

- Keep `NTSD_Battle` Canvas, hierarchy, layout, camera and default-visible behavior in the scene.
- Treat `BattleHudView` and `BattleComboView` as lightweight scene presentation components, not TEngine `UIWindow` classes.
- Preserve the existing single HUD and ComboPanel contract.
- Bind `ComboBg`, `ComboTemp` and `ComboArraw` to the existing `BattleComboView` component in the scene.
- Make state visibility explicit when a runtime snapshot is applied; a null state must not unexpectedly hide the default scene UI.
- Leave `BattleControlsView` behavior and scene references unchanged until the user supplies its input implementation logic.

## Non-goals

- Do not create or migrate a `BattleMainUI` prefab.
- Do not import TEngine or route this scene-resident Canvas through `UIModule`.
- Do not connect buttons to `CharacterInputModule`.
- Do not modify `BattleBootstrap`, battle runtime ownership, input sampling, DAT/content, or other scenes.

## Acceptance

- `BattleHudView` and `BattleComboView` remain valid scene `MonoBehaviour` presentation components.
- The scene's existing HUD bindings remain intact and ComboPanel's three Image references resolve to the current child Image components.
- `BattleControlsView` has no new behavior or guessed input semantics.
- Static C# validation, focused scene-reference validation and `Tools/Validate-ChangeLedger.ps1` pass.
- Unity assembly compile and Battle Scene Play remain pending if the active Editor cannot be safely used.

## Rollback

Review and revert only the scoped view-script changes, the three ComboPanel serialized reference lines and this task's governance records. Preserve all unrelated user scene edits, diagnostics, battle scripts and documentation.
