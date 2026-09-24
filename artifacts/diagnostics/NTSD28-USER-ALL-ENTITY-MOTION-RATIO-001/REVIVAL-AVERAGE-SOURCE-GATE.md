# D-024 ordinary revival average: source-coordinate gate

2026-09-24 read-only paired caller audit. This is a follow-on to, and deliberately separate from, the bounded OID998/action6 continuation-effect birth package. No DAT or gameplay change is authorized by this note alone.

Formal playable closure: `ntsd28_playable/scripts/build.ps1` includes `battle_world.cpp` and `simulation_tick_driver.cpp`; `BattleWorld28::advance_native_revivals` in `ntsd28_core/src/simulation/battle_world.cpp` around 8484–8507 loops same-group active type-0 peers, adds each **integer** `position.x/z`, then checks `sum_x != 0 && peers > 0`. Only that branch consumes synchronized RNG sites `0x90` and `0x91` and writes the revived entity's **precise** X/Z as integer average plus raw random offset. The integer X/Z mirrors are not immediately synchronized in this transaction. `SimulationTickDriver28::step` calls this pass before the continuation-effect materialization.

Unity production counterpart: `BattleRespawnModule.ApplyRespawnWithoutStoredCount` in `Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs` around 78–138 sums current physical `Runtime.XInt/ZInt`, uses that sum as the RNG gate, writes current physical precise X/Z from physical peer average plus D-024 scaled random offset, and leaves current integer mirrors unchanged. It presently does not advance independent source-rule precise X/Z. Prior ratio tests only checked physical offsets when both coordinate domains coincided.

First-difference candidates requiring exact RED witnesses:

| Source peer integer sum X | Physical peer integer sum X | Formal action | Current Unity action |
| ---: | ---: | --- | --- |
| 0 | nonzero | No position change and no RNG | Reposition and consume two RNG calls |
| nonzero | 0 | Reposition and consume two RNG calls | No position change and no RNG |
| nonzero | different nonzero | Source precise average differs | Only physical average exists |

The future writer must use one formal source-domain gate and one pair of RNG draws when source history is complete. Reuse those raw draws in both domains: source precise = source integer peer average + raw offset; physical precise = physical integer peer average + offset scaled once by the approved viewport ratio. Preserve source and physical integer mirrors until their established sync point. If source history is absent for any eligible peer, do not silently invent source history by dividing physical coordinates; classify that incomplete-carrier path explicitly before activating rule readers. Include negative integer averages, `sum_x==0`, ignored non-type0/different-group peers, RNG state, snapshot and a real participant path in focused acceptance. Avoid editing DAT values or changing the fixed camera.

Status: `SOURCE_UNITY_CALLER_AUDIT / NO_PRODUCTION_FIX`. The current OID998 effect-birth package does not close this gate. Q07 remains paused behind D-024.

Dependency: live participant peer positions must first have source-rule continuous motion and the formal integer-sync boundary; otherwise a synthetic source sum test can expose the branch defect but cannot certify a real play sequence. Do not activate this reader against stale birth-only source history.
