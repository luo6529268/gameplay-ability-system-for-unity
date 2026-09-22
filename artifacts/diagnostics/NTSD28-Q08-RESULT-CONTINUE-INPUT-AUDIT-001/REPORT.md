# Q08 result-continue input: source and Unity seam audit

Status: `SOURCE_AND_UNITY_CALLER_AUDITED / IMPLEMENTATION_PENDING` (2026-09-22). This is a read-only audit of the current `81cacc3e` worktree, not an acceptance result.

## Formal playable rule

`source/ntsd28_playable/src/game_session.cpp` lines 2731-2742 computes `result_continue_requested` immediately before `BattleFlow28::step`, by iterating `effective_combatants(config_)` and OR-ing the **held** Attack or Jump byte in `slot_inputs_[combatant.slot]`. It checks configured combatant slots, not only P1/P2, living entities, or a just-pressed edge. `set_input` at lines 2447-2450 supplies that slot array. A slot outside its capacity is skipped. A paired `story_mission_id` and `story_child_stage_id` bypasses ordinary BattleFlow entirely; a direct mode-1 fight without that pair does not.

`source/ntsd28_core/src/simulation/battle_flow.cpp` lines 80-119 increments the timer first, then if the incremented timer is at least 144 and the request is true, sets it to 350. This emits result-complete and the mode transition in the **same** step, with stored timer reset to zero. An input before 144 does not advance completion. Phase 3 on subsequent calls does not reclassify or increment. `game_session.cpp` lines 3228-3237 then skips the combat driver while the transition state is nonzero. The formal source fixture and double-run result timing are already recorded in `NTSD28-Q08-RESULT-GROUP-CARRIER-AUDIT-001`.

## Current Unity seam

`LocalSimulationFrameInputProvider.GetFrameInput` in `SimulationTickDriver.cs` lines 102-142 captures `SimulationPlayerInput.Buttons` as held state and derives `PressedButtons` separately. `SimulationFrameInputModule.TryCaptureLocalFrameInput` lines 115-144 captures active human roster entries; its `ApplyFrameInputSet` can also pass explicit AI-slot Buttons to the native AI input pipeline. Both the local driver (`SimulationTickDriver.cs` line 588) and in-process lockstep host (`InProcessBattleKernelHost.cs` line 143) apply the frame before calling the battle tick. `NTSDBattleTickSystem.RunTick` has the frame argument available at lines 292-320 when it calls `AdvanceNativeBattleResultsBeforeCombat`.

The native carrier currently takes no frame argument (`BattleResultsOutcomeHostWriter.AdvanceNativeFlowBeforeCombat`) and increments through 144 without the formal shortcut. The separate old `BattleResultsWriter.RunActiveTick` collects only P1/P2 `PressedButtons` after the world tick (lines 22-33, 431-449). Reusing that old result-page input would change both the accepted slots and held/edge timing.

## Exact next implementation boundary

Before changing scripts, create a new Task/Change Record for only the result-continue route. Pass the *current tick's* frame from `NTSDBattleTickSystem` through `SimulationWorld` to the native carrier; use held `Buttons` and the configured active roster mapping, including explicit AI-slot input when supplied. Resolve how roster player slot maps to formal effective combatant physical slot (including CPU 10/11) before coding; do not equate `PlayerSlot` with `RuntimeSlotIndex` or use the old UI's P1/P2 aggregation. Keep the existing mode-4 reserve and old result-page paths outside this change. Confirm whether a frame with no entries represents neutral held input; do not reuse the previous tick's frame.

Focused RED/PASS cases: timer 142 with held Attack stays at 143; timer 143 with held Attack or Jump completes at 350 that tick; held input from an effective slot beyond P1/P2 is accepted; unrelated/nonparticipant slot input is ignored; no input progresses naturally; full tick at 350 skips further combat on the transition path. Include same-seed source/Unity projection, snapshot/checksum on shortcut and one representative real battle/close/re-enter. Current Q08 native carrier and Q08 parent remain open until these and the separate 101-tick result-page/host ownership checks pass. No files were deleted or replaced by this audit.
