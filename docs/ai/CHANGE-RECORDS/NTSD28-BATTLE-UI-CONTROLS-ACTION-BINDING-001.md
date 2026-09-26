<!-- CHANGE-RECORD
id: NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/App/InputModule.cs
code-path: Assets/NTSD/Scripts/Input/CharacterInputModule.cs
code-path: Assets/NTSD/Scripts/UI/NTSDButton.cs
code-path: Assets/NTSD/Scripts/UI/Battle/BattleControlsView.cs
authority: Current user instruction to bind the three NTSD_Battle controls to NTSDInputConfig Attack, Jump and Defend
evidence: docs/ai/TASKS/NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001.md
-->

# NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001

Pre-change: `BattleControlsView` only applied a visual held-state snapshot and explicitly did not write to `CharacterInputModule`. The three scene objects named `AttackBtn`, `JumpBtn` and `DefendBtn` currently contain `Image` components rather than Unity `Button` components, and their view references are not serialized yet. The existing input owner is `CharacterInputModule`, which consumes the shared `AppManager.InputModule` action map and enqueues tick-aligned input.

Required change: introduce a lightweight reusable `NTSDButton : Button` interaction component for differing future button effects, bind three `NTSDButton` controls, accept the player ID from an external battle-flow owner after scene entry, then resolve `Player_<id>` through `AppManager.Instance.InputModule.GetActionMapByPlayerID(playerId)` and bind exactly Attack, Jump and Defend. Keep held-state routing in the existing InputModule/CharacterInputModule path without reading SimulationWorld, roster slots or character entities.

Expected side effects: `NTSDButton` publishes press-state changes from its normal uGUI pointer overrides and exposes a future presentation extension point. `BattleControlsView` subscribes to the three assigned custom buttons and no longer adds runtime relay components. `CharacterInputModule` keeps its existing registration lifecycle. No shader effect implementation, second `NTSDInputConfig`, fixed Inspector player ID or scene serialization change is included in this pass.

Non-goals: scene hierarchy or component changes, generated input code changes, keyboard binding changes, `BattleBootstrap`, TEngine UI lifecycle, battle simulation pass changes, or runtime Play verification.

## Implementation and validation

- Added `NTSDButton : UnityEngine.UI.Button`. It owns pointer pressed-state publication and clears the state on pointer up, pointer exit and disable. Its protected `OnPressedStateChanged(bool)` hook allows later presentation implementations without coupling them to battle input routing.
- Changed `BattleControlsView` to serialize three `NTSDButton` controls and subscribe/unsubscribe to `PressedStateChanged`. The temporary runtime `BattleControlPointerRelay` and `AddComponent` path were removed.
- Preserved the externally supplied player identity: `BindPlayer(int)` resolves the shared `InputActionMap` with `AppManager.Instance.InputModule.GetActionMapByPlayerID(assignedPlayerId)` before mapping Attack, Jump and Defend.
- Preserved the existing `InputModule` -> registered `CharacterInputModule` -> tick input buffer path, including separate device/UI held state.
- The existing Unity Editor imported `NTSDButton.cs`, generated GUID `ee105cf15d6ac00458745a9a03e08e06`, and refreshed `Assembly-CSharp.csproj`.
- Final `dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly`: 0 errors, 53 project warnings. A new member-hiding warning observed during import was removed by renaming the state property to `IsPointerPressed`.
- Pending: assign `NTSDButton` on the three scene controls, connect the external battle-flow owner to `BindPlayer`, and run real `NTSD_Battle` pointer hold/release/keyboard-coexistence Play validation.

## Rollback

Rollback is limited to the four declared scripts and this Change Record's governance entries.
