<!-- TASK
id: NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001
status: IN_PROGRESS
-->

# NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001

## User requirement

The three controls in the `NTSD_Battle` Canvas represent the existing `NTSDInputConfig` actions `Attack`, `Jump` and `Defend`. The first implementation pass is limited to `BattleControlsView.cs` so the user can review the binding logic before scene references are finalized.

## Bounded scope

- Update only `Assets/NTSD/Scripts/UI/Battle/BattleControlsView.cs` and this package's governance records.
- Resolve the existing `Player_<playerId>` action map through `AppManager.InputModule`; do not instantiate a second `NTSDInputConfig`.
- Treat the current scene `Image` objects as explicit pointer surfaces and support press, release and pointer-exit transitions.
- Route the resolved action to the active human character's existing `CharacterInputModule`, preserving its tick-buffer path and Attack/Jump/Defend mapping.
- Keep `NTSD_Battle.unity`, `NTSDInputConfig.cs`, `CharacterInputModule.cs`, `BattleBootstrap.cs` and other scenes unchanged in this pass.

## Non-goals

- Do not add `Button` components or change the Canvas hierarchy in the scene during this script review pass.
- Do not synthesize `InputAction` callbacks or create another input asset.
- Do not change keyboard bindings, action names, simulation pass order, or runtime ownership.
- Do not connect HUD/Combo snapshot producers or TEngine `UIModule` in this package.

## Acceptance

- `BattleControlsView` has explicit Player ID and three action-surface references.
- The script resolves `Player_<playerId>/Attack`, `Jump` and `Defend` from the shared `InputModule` action map.
- Pointer down/up/exit writes the corresponding held state through the current human `CharacterInputModule`; releasing or disabling the view clears held state.
- The action map is not disabled by the view, because `CharacterInputModule` shares it with keyboard input.
- Static/generated-project compilation and Change Ledger validation pass.
- Unity Editor scene binding and real Battle Scene pointer Play remain pending until the user reviews this script and assigns the three Image references.

## Rollback

Revert only `BattleControlsView.cs` and this package's task/record/ledger/status entries. Preserve all existing scene, input asset and unrelated battle changes.
