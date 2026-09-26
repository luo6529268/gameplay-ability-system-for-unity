<!-- TASK
id: NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001
status: RUNTIME_PENDING
-->

# NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001

## User requirement

The three controls in the `NTSD_Battle` Canvas use a reusable custom `NTSDButton : Button`, allowing different presentation effects to be attached later without moving visual logic into `BattleControlsView`. The player ID is assigned by an external battle-flow owner after scene entry; only then may the view resolve `Player_<id>` through `AppManager.Instance.InputModule.GetActionMapByPlayerID(playerId)`.

## Bounded scope

- Update `App/InputModule.cs`, `Input/CharacterInputModule.cs`, add `UI/NTSDButton.cs`, update `UI/Battle/BattleControlsView.cs` and this package's governance records.
- Route typed held-state changes by the externally assigned player ID through the existing `AppManager.InputModule`; do not instantiate a second `NTSDInputConfig`.
- Bind three `NTSDButton` components and support press, release and pointer-exit transitions through the custom button state event.
- Register the bound `CharacterInputModule` under its assigned input ID, preserving its tick-buffer path and Attack/Jump/Defend mapping.
- Keep `NTSD_Battle.unity`, generated `NTSDInputConfig.cs`, `BattleBootstrap.cs` and other scenes unchanged in this pass.

## Non-goals

- Do not change the Canvas hierarchy or add the missing Button components in the scene during this script review pass; the user will perform scene binding.
- Do not synthesize `InputAction` callbacks or create another input asset.
- Do not change keyboard bindings, action names, simulation pass order, or runtime ownership.
- Do not connect HUD/Combo snapshot producers or TEngine `UIModule` in this package.

## Acceptance

- `BattleControlsView` has three `NTSDButton` references and no serialized/default player ID.
- An external owner assigns the player ID through a public binding method; that method resolves `Player_<id>/Attack`, `Jump` and `Defend` through `GetActionMapByPlayerID` before input can be submitted.
- Pointer down/up/exit reaches the registered `CharacterInputModule`; releasing or disabling the view clears held state.
- The view does not read, enable or disable the shared action map; `CharacterInputModule` keeps its existing keyboard/InputSystem ownership.
- Static/generated-project compilation and Change Ledger validation pass.
- Unity Editor scene binding and real Battle Scene pointer Play remain pending until the user reviews this script and assigns the three Image references.

## Current evidence

- `NTSDButton : Button` publishes `PressedStateChanged` for pointer down, pointer up, pointer exit and disable cleanup. It exposes `OnPressedStateChanged` as the presentation extension point, without embedding a Shader, scale, sound or HUD policy.
- `BattleControlsView` binds three `NTSDButton` references, subscribes without adding runtime components, and retains the external `BindPlayer(int)` -> `GetActionMapByPlayerID` contract.
- Unity imported the new script and generated its `.meta`; the final `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly` completed with 0 errors and 53 project warnings.
- Scene component assignment, the external battle-flow caller for `BindPlayer`, and real pointer Play validation remain pending.

## Rollback

Revert only the four declared scripts and this package's task/record/ledger/status entries. Preserve all existing scene, generated input asset and unrelated battle changes.
