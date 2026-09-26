<!-- CHANGE-RECORD
id: NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001
status: VERIFIED
change-kind: Q07_D024_HITFA5_PAIRED_PLAYABLE_COMPLETE_SESSION_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/hitfa5_full_session_source_probe.cpp
authority: formal root NTSD2.8-Logan.exe and paired playable GameSession28::step NativeAi28::step_non_character_hit_fa, indexed w/e.dat frame51
evidence: prior NTSD28-USER-SOURCE-HITFA5-TARGET-VELOCITY-001 focused Editor 8/8 but no complete Driver witness
-->

# NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001

Implementation: Added only `Tools/NTSD28Q07Diagnostics/hitfa5_full_session_source_probe.cpp`. It uses indexed formal OID219 frame51 in a two-combatant `GameSession28`, injects a controlled controller at slot20, and records eight complete steps for matching-group and nonmatching-group controls in fresh CSV outputs. Unity production, DAT, images, Scene, configs and nonbattle code are unchanged by this probe.

Post-change evidence: first MinGW compile failed on probe-only wrong `Entity28` type; corrected to `EntityState28`, second compile against all 28 current core TUs plus playable `game_session.cpp`/`selection_flow.cpp` exit0. `run-01` source Session positive/negative exit0: matching group birthed one OID219 child at tick1 (X103/Z542, Vx2, target0), unmatched group birthed none through tick8. Exact inputs/rows and limits in `artifacts/diagnostics/NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001/REPORT.md`. This verifies only this diagnostic exit. Unity complete Driver, root EXE same-world and pixels remain pending under separate governed work. Rollback scope is the new diagnostic only, subject to repository deletion rules.

Pre-change: indexed formal OID219/type3 `w/e.dat` frame51 has `hit_Fa:5`; its paired playable source spawns one OID219 child per living same-group character through the ordinary `GameSession28::step` pass and removes the controller. Unity's source-domain quotient and child source carrier were previously corrected in direct method tests only. No saved complete-Session same-world trace covers this path. New C++ diagnostic script, expected positive/negative controls, exact outputs, non-goals, validation and rollback are declared in the Task Contract before script edit. Existing source, Unity production, DAT, images, Scene, configs and nonbattle code are excluded.
