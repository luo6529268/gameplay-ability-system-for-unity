# Q09/P-08 root LFR initial-HP entry audit — 2026-09-29

Status: `READ_ONLY / SAME_INITIAL_STATE_LFR_NOT_AVAILABLE_FOR_SELECTED_CASE`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q09 / P-08`.

The selected paired natural-hit/WARP case starts Ita at current HP 180 and base HP 500 (`Tools/NTSD28Q09Diagnostics/ita_natural_bleed_warp_probe.cpp`). The formal root EXE remains SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`.

The declared playable `GameSessionLfr28::snapshot_entity` writes `entity.base_max_hp` into the physical slot's `base_hp` field (`source/ntsd28_playable/src/game_session_lfr.cpp:134-151`). `GameSessionLfrPlayback28` reads that field and sets **both** `combatant.hp` and `combatant.base_hp` to it (`:681`, `:702-703`). The root `main.cpp` playback arguments include slot action/facing/MP overrides (`:751-767`) but no current-HP override. Therefore replaying an LFR made from this paired case would initialize Ita at HP 500, not 180. A root report or trace from that LFR would not be the same-initial-state validation of the selected natural bleed/three-pixel case. No LFR or root playback was generated here.

Next useful route: construct a separately named natural case whose **initial current HP equals base HP**, first verify in the paired playable full Session that ordinary input reaches damage below the mark threshold while Ita survives and returns to a bpoint frame, then record LFR and run the unchanged root EXE. Keep the HP180 paired WARP evidence as its own scoped case; do not silently compare a changed-HP root run against it. If no such natural case reaches the mark, retain P-08 root same-state as open and use another expressly supported root entry rather than editing authority or DAT.

This was a source and CLI contract audit only. It did not change the formal EXE/source, Unity production, DAT, images, Scene, tests or result files. It does not change the P-08/Q09/BATCH-05 or Q07/D-024 status.
