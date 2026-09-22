# Q08 timer-101 consumer audit

Status: `READ_ONLY_SOURCE_AND_UNITY_CALLER_AUDIT / Q08_OPEN` (2026-09-22). No runtime, Scene, asset, or script change was made for this audit. The formal root EXE SHA-256 was freshly checked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`.

## Current playable path

- `source/ntsd28_playable/scripts/build.ps1` includes `src/simulation/battle_flow.cpp` and `src/game_session.cpp` in its playable compilation inputs. `GameSession28::step()` calls `battle_flow_.step()` before the combat world update, then returns early at the terminal transition.
- `source/ntsd28_core/src/simulation/battle_flow.cpp`, `BattleFlow28::step()`: at timer 101 the step changes phase to `result_visible` and sets `result_record_created = (timer_ == 101)`. This flag is a one-step output, while phase/timer/outcome are the continuing state.
- An exact C++ source search found `result_record_created` consumers only in the scenario serializer and `battle_flow_tests.cpp`; it found no read in `GameSession28`. The playable `GameSession28::snapshot()` instead sets `native_scoreboard_visible` for mode 0 when `battle_flow_step_.phase == result_visible`, and reads the winner from `battle_flow_step_.outcome`. Its battle-tick score source stops incrementing when the flow step timer reaches 100. This is a source observation, not an EXE visual capture.

## Current Unity boundary

- `BattleResultsOutcomeHostWriter.AdvanceNativeFlowBeforeCombat()` already stores `NativeResultOutputTimer`, `NativeResultPhase`, `NativeLivingGroupMask` and the later transition. `BattleParitySnapshot` and `BattleLockstepChecksumModule` include these native state fields.
- The separate Unity result page can activate at legacy `BattleEndPhase >= 11`. Existing P-19/G-08 exceptions allow the Unity page's own presentation. Its timing cannot define the native battle phase, combat input, or transition.

## Decision for the next Q08 child

Do not treat a newly allocated persistent Unity “result record” object as an independent required parity output solely because the core step has a transient `result_record_created` flag. The current playable live consumer evidence supports checking the timer-101 phase/outcome state and any actually consumed render handoff; preserve the approved Unity result-page presentation exception. Keep timer-80 end signal, >=144 held continue, timer-350 transition, host routing, combat freeze, source-result visible behavior, and original-Editor/Player runtime acceptance open. If later formal EXE evidence identifies a separate lasting record or scoreboard side effect outside the inspected live path, revise this boundary before implementation.
