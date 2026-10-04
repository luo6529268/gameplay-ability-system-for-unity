<!-- CHANGE-RECORD
id: NTSD-BATTLE-DIRECTION-HYBRID-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleDirectionControl.cs
code-path: Assets/NTSD/Scripts/App/InputModule.cs
code-path: Assets/NTSD/Scripts/Input/CharacterInputModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/DirectionSceneValidationTemporary.cs
authority: User requests BattleControls click/joystick/hybrid direction UI
evidence: artifacts/diagnostics/NTSD-BATTLE-DIRECTION-HYBRID-001/before.json
-->
# NTSD-BATTLE-DIRECTION-HYBRID-001
Existing BattleControls only has authored directional ring and knob; MMTouchJoystick publishes each Update and lacks owning-pointer hybrid threshold. Add event-driven uGUI gesture adapter, same immutable HUD InputId/session binding, same registered player input sink; separate device/UI direction contributions and OR bits before existing tick buffer. Preserve device-only analog value/deadzone; opposing sources retain both native held bits and cancel only legacy display vector. No simulation/pass/action mapping/DAT/Combo/PP changes.

Modes public ClickOnly/JoystickOnly/Hybrid, default Hybrid. Click holds initial eight-sector direction; screen-pixel movement exceeding configurable threshold latches joystick until release; joystick uses fixed ring center, clamped knob, radial deadzone/hysteresis and angular hysteresis. Center press neutral, ring outside circle rejected. One pointer owns gesture, foreign down/drag/up ignored. Pointer up/end drag/cancel/disable/focus loss/pause/rebind/mode change clear UI source and restore authored knob center. No per-frame input polling. Lifecycle release only through existing input sink; no manager or shutdown order change.

Acceptance: fresh compile, real Unity pointer events/three modes/threshold/jitter/8sectors/deadzone/hysteresis/multitouch/action independence/keyboard OR/lifecycle/rebind; original Scene hookup and native directional movement when approved. No broad alignment claim. Rollback exact backups and remove only introduced files with prior audit.

Three declared production scripts written. Input event merge preserves device-only deadzone/analog; gesture subscribes same immutable selected-human binding independently of action view. Scene untouched; compile/tests pending.

User confirms current Play may stop and saved Scene may be wired/tested. Original Editor was already stopped, Scene clean. Add temporary DirectionSceneValidationTemporary Editor probe for exact component hookup and original Scene runtime assertions. Isolated first harness failed raycast immediately after creating Graphics; retain failure and wait a rendering cycle before raycast.

Isolated Unity115 assertions PASS. Original Editor probe first compile failed CS0266 (probe treated authoritative double position as float); corrected probe variable to double, failure retained. Runtime DLL was fresh but Editor acceptance not passed yet.

Runtime managed compile0errors/36warnings. Independent real Unity Play harness PASS115: authored ring/knob dimensions+sprites, actual EventSystem/production gesture/input module/local provider/native input. Canvas scales0.5/1/2, thresholds11/12/13screen px, all8sectors, deadzone/angle hysteresis/clamp, pointer ownership, action concurrency, keyboard/UI OR/opposed bits, focus/pause/cancel/disable/replay/mode/rebind/removal. Synthetic data fixture. First failure was graphics raycast before a render cycle; corrected fixture waits, final passes. Original Scene saved component1941467046 only with ring1527051355 and knob2042893407; no transform/resource/other UI changes. OriginalScene runtime probe underway.

Original first Play bound correct InputId1 and raycast, stopped at immediate press guard (11assertions passed). Result retained original-first-failure.json; no runtime pass claimed. Probe now reacquires actual OS/GameView focus after long resource preload and records first-press focus/position diagnostics without bypassing production focus guard.

Original saved NTSD_Battle Play PASS259 assertions (includes actual Driver StepOneTick checks), selected NarutoOID2/InputId1. Actual Graphics raycast -> production gesture -> registered CharacterInputModule -> local frame provider -> native direction/action. Right x800.7465->849.9108 in8ticks; hybrid11screen pixel jitter stays click, threshold crossing replaces right with left, center remains latched/neutral; ClickOnly/JoystickOnly eight sectors/up release; foreign finger ignored; direction plus attack/jump/defend frames60/210/110; keyboard/UI OR, mode change and disable/re-enable clear/replay. Screenshot original-direction.png captured real GameView and inspected; knob visibly clamped upper-right. Postplay Scene clean. Independent115assertions additionally cover thresholds at Canvas0.5/1/2, deadzone/angular hysteresis, opposing inputs, focus/pause/cancel, InputId2 slot0 and InputId2->1 rebind/removal. Synthetic pointer events through actual components, controlled paused/manual Driver ticks; not physical touchscreen/OS mouse acceptance, full BattleRuntimeSelfCheck or formal EXE parity. No DAT/native phase/action mapping/Combo/PP/ripple/font changes.
First original press failure resolved by reacquiring OS/GameView focus after resource prewarm; production focus guard retained. Temporary diagnostic source/meta removed with exact backup/hash audit. Final cleanup Editor refresh pending confirmation.

Final cleanup complete: original Editor idle/nonplaying/noncompiling, active NTSD_Battle isDirty=false, zero compiler-error entries; final-editor-state.json/final-scene-state.json/final-compiler-errors.json. Scoped diff-check and Tools/Validate-ChangeLedger.ps1 exit0; Scene final SHA stable. Temporary diagnostic script/meta absent, backups retained. No remaining implementation blocker; physical touchscreen/manual OS gesture acceptance not run.
