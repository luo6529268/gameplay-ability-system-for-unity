<!-- CHANGE-RECORD
id: NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001
status: IN_PROGRESS
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleControlsView.cs
authority: Current user instruction to bind the three NTSD_Battle controls to NTSDInputConfig Attack, Jump and Defend
evidence: docs/ai/TASKS/NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001.md
-->

# NTSD28-BATTLE-UI-CONTROLS-ACTION-BINDING-001

Pre-change: `BattleControlsView` only applied a visual held-state snapshot and explicitly did not write to `CharacterInputModule`. The three scene objects named `AttackBtn`, `JumpBtn` and `DefendBtn` currently contain `Image` components rather than Unity `Button` components, and their view references are not serialized yet. The existing input owner is `CharacterInputModule`, which consumes the shared `AppManager.InputModule` action map and enqueues tick-aligned input.

Required change: add a reviewable scene-view bridge that resolves the shared `Player_<playerId>` map and its `Attack`, `Jump` and `Defend` actions, listens to pointer press/release on explicit Image surfaces, resolves the active human controller for that input ID, and forwards the held-state transition through the same `CharacterInputModule` setters used by the generated InputAction callbacks.

Expected side effects: `BattleControlsView` will add a small runtime pointer relay to each assigned Image surface. It will not own or disable the shared action map, create another `NTSDInputConfig`, or modify scene serialization in this pass.

Non-goals: scene hierarchy or component changes, generated input code changes, keyboard binding changes, `BattleBootstrap`, TEngine UI lifecycle, battle simulation pass changes, or runtime Play verification.

## Implementation and validation

Implementation is in progress. The final record will list the exact method responsibilities, generated-project build result, focused source checks, and any remaining Unity scene/runtime evidence.

## Rollback

Rollback is limited to `Assets/NTSD/Scripts/UI/Battle/BattleControlsView.cs` and this Change Record's governance entries.
