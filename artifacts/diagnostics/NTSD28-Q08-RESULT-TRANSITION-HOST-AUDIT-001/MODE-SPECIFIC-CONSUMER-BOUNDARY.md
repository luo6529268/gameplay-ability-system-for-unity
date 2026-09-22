# Q08 mode 2/3/4 result-transition consumer boundary (2026-09-22)

Status: `VERIFIED_STATIC_BOUNDARY_ONLY`. This audit changes no runtime or resource file.

## Authority and observed path

- Formal root `NTSD2.8-Logan.exe` SHA-256 was freshly checked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. `source/README_SOURCE.md` identifies the corresponding source, and `ntsd28_playable/scripts/build.ps1 -Target playable` includes `game_session.cpp` and core `battle_flow.cpp`.
- `BattleFlow28::step` assigns transition command 28, 128 or 202 for battle mode 2, 3 or 4 at result timer 350, resets the timer, then keeps the command unchanged in its `result_complete` branch. Only ordinary command 2 becomes command 1 and sets `upper_scene_entered` on the next call (`battle_flow.cpp` lines 49-63, 105-121).
- The inspected playable `GameSession28::step` calls that flow before combat. Its `upper_scene_entered` branch handles ordinary selection or explicit battle-scene-only rematch (`game_session.cpp` lines 2730-2782). Its later `transition_state != 0` guard clears pending combat input and returns before the world tick (`game_session.cpp` lines 3228-3240). A scoped search of playable `src`/`include` found no mode-specific `28/128/202` consumer; the only `upper_scene_entered` consumer is in `game_session.cpp`. This is evidence about the inspected playable source closure, not proof that a native helper outside that closure has no behavior in every original-game context.
- Unity `BattleResultsOutcomeHostWriter` emits the same three commands at timer 350. `SimulationTickDriver.TryDispatchOrdinaryResultTransition` dispatches command 2 only; `CanAdvanceTick` rejects nonzero commands, preserving the old battle world. This is a static correspondence for command production and freeze, not a completed mode-specific frontend.

## Consequence for alignment

The mode-2/3/4 timer-350 command and old-world freeze have a bounded source/Unity contract. Do not route 28, 128 or 202 through Unity's ordinary selection or battle-only rematch just to clear the frozen state. Mode-specific result pages, mission continuation, setting actions and further transitions remain `AUTHORITY_SURFACE_PENDING`; obtain a reachable formal-release behavior path or an explicit user exception before implementing them. The prior mode-4 complete-session source witness covers the frozen World after 350, but does not prove the unavailable frontend behavior. The newly added in-combat lethal-timing Unity test is separate and remains `COMPILE_PENDING` in the original Editor.
