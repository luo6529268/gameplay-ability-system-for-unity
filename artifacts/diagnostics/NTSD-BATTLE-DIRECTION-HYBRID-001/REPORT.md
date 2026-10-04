# BattleControls click / joystick / hybrid

Original saved NTSD_Battle Play PASS259 assertions (includes actual Driver StepOneTick checks), selected NarutoOID2/InputId1. Actual Graphics raycast -> production gesture -> registered CharacterInputModule -> local frame provider -> native direction/action. Right x800.7465->849.9108 in8ticks; hybrid11screen pixel jitter stays click, threshold crossing replaces right with left, center remains latched/neutral; ClickOnly/JoystickOnly eight sectors/up release; foreign finger ignored; direction plus attack/jump/defend frames60/210/110; keyboard/UI OR, mode change and disable/re-enable clear/replay. Screenshot original-direction.png captured real GameView and inspected; knob visibly clamped upper-right. Postplay Scene clean. Independent115assertions additionally cover thresholds at Canvas0.5/1/2, deadzone/angular hysteresis, opposing inputs, focus/pause/cancel, InputId2 slot0 and InputId2->1 rebind/removal. Synthetic pointer events through actual components, controlled paused/manual Driver ticks; not physical touchscreen/OS mouse acceptance, full BattleRuntimeSelfCheck or formal EXE parity. No DAT/native phase/action mapping/Combo/PP/ripple/font changes.

Implementation:
- BattleDirectionControl.cs: event-driven single-pointer uGUI adapter on existing BattleControls root, serialized ControBg/ControPoint refs. Public SetMode(BattleDirectionMode), Mode, CancelGesture(), BindPlayer/UnbindPlayer. Independent selected-human immutable HUD InputId/session/handle subscription and current snapshot replay. No player1/first-AI fallback.
- InputModule.cs: TrySetMoveInput routes through same registered player sink as actions.
- CharacterInputModule.cs: separate device/UI direction contributions, OR each held bit before unchanged tick buffer/native boundary; device-only analog/deadzone retained. Opposing bits remain available to native arbitration; legacy vector cancels opposed components. Action states untouched.
- NTSD_Battle.unity: one added component; all authored transforms/sprites/other UI retained.

Defaults / behavior:
- Hybrid default,12screen px threshold (strictly greater switches); Canvas scale independent. Initial ring press outputs fixed click sector; center within0.45radius starts neutral. ClickOnly ignores drag, holds initial direction until release/cancel. JoystickOnly starts fixed-center joystick immediately.
- Joystick clamps knob to40local units for existing180ring/100knob. Deadzone0.2travel with0.05entry/exit hysteresis, angular hysteresis5degrees around eight22.5degree half-sectors. Out-of-ring initial press rejected; owned drag outside clamps, no second-finger takeover.
- Release/end drag/cancel/disable/application focus loss/application pause/rebinding/mode change clears UI source and restores authored knob center.

Validation:
- Managed runtime0errors/36warnings; Editor0errors/275warnings.
- isolated-result.txt PASS115, isolated-final-harness.cs.txt and isolated-unity.log.
- result.json PASS259, postplay.json dirtyfalse, original-direction.png.
- First isolated raycast failed before graphics registered; harness waits a frame. Temporary original Editor probe first had float/double compile mismatch; fixed to double. First original press failed without focused GameView after preload; final focused run passed, production guard unchanged. All failures retained.
- Temporary probe source/meta backed up and removed; final clean Editor/compiler/ledger results appended after cleanup.

Scope limits: user-requested UI/input adapter only. Application pause tested; no new F1 gameplay-pause policy. Tests use synthetic pointer events, real native input path and controlled ticks, not physical touch-device/manual gesture testing. Full BattleRuntimeSelfCheck and formal EXE parity not run; no battle-rule alignment claim.

Final cleanup complete: original Editor idle/nonplaying/noncompiling, active NTSD_Battle isDirty=false, zero compiler-error entries; final-editor-state.json/final-scene-state.json/final-compiler-errors.json. Scoped diff-check and Tools/Validate-ChangeLedger.ps1 exit0; Scene final SHA stable. Temporary diagnostic script/meta absent, backups retained. No remaining implementation blocker; physical touchscreen/manual OS gesture acceptance not run.
