# Non-character early stage-depth pass correction

Status: `FOCUSED_TEST_PASS / RUNTIME_PENDING`, 2026-09-24. Change ID: `NTSD28-USER-NONCHAR-EARLY-STAGE-Z-001`. This is a battle-only pass-order fix discovered during the D-024 non-perceptual writer audit. The formal EXE SHA-256 was verified as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; adjacent `source/README_SOURCE.md` declares the playable source, and its build script includes `battle_world.cpp` and `simulation_tick_driver.cpp`.

Paired playable `SimulationTickDriver28::step` invokes `BattleWorld28::clamp_type0_stage_depth` before geometry and again after hits. The function's name is misleading: it loops every active entity, clamps type0 precise Z to `[near,far]`, other entity types to `[near-1,far+1]`, and refreshes integer Z even when precise Z stays in range. Unity `NTSDBattleTickSystem` invoked its matching `ClampCharacterZToStageBounds` twice at the corresponding phases, but default `BattleEcsCharacterStageZPass.IsEligible` and the Legacy oracle admitted only characters. Non-character Z was instead clamped later by `PreFrameBounds`. Thus a held/collision/hit phase could read an out-of-range non-character Z.

| Original-project Editor job | Scope | Outcome |
| --- | --- | --- |
| `da79757f77b140d4939946d45af63fa8` | Test-first actual registered World, first StageZ pass | RED as predicted: type1 weapon expected Z351 but remained Z500. |
| `7463df56318e47dc8ea0465508be5086` | Corrected DataOriented, Legacy, ShadowCompare and formerly stale bounds test | GREEN 4/4. |
| `6d4d1ee15b2e40fd8f623242963b3180` | Final strengthened StageZ and StageBounds test classes plus two-view WPoint held regression | GREEN 16/16, 0 failed/skipped. |

The production pass now admits every active registered entity and applies type0 margin0 versus non-type0 margin1 in both DataOriented and Legacy/ShadowCompare paths. It still writes only precise Z and integer Z; no X/Y, DAT value, physical view factor, pass position or RNG change. Focused tests cover registered character type0, weapon type1 and special type3 at far350, in-range non-character precise Z200.75 with integer mirror200, pending-destroy/dormant exclusion, and a repeated pass. The earlier test expecting non-character Z500 after the first pass was corrected because it froze the contradicted Unity behavior.

This proves the selected pass's registered-World behavior and adjacent test compatibility. It does not prove a complete same-input full Driver trace, collision/hit downstream parity, real Battle Scene Play or formal EXE observable result. The D-024 source-rule coordinate carrier, OID219 and fusion first differences and weapon-component WPoint pose remain open. Q07 stays paused.
