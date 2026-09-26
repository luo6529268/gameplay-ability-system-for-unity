# NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001

Status: `VERIFIED_SCOPED_SOURCE_SESSION`. BATCH-04/Q07 and D-024 remain open.

Authority identity was rechecked: formal root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. Indexed `w/e.dat` SHA-256 `178D2653E2BC4AFFF7B933DA2F28A87ECD6217CBD4F99291EA1B19F300704274` declares OID219/type3 frame51 `hit_Fa:5`. The paired playable `SimulationTickDriver28::step` calls `NativeAi28::step_non_character_hit_fa`; the diagnostic calls only `GameSession28::step` and does not directly call that AI branch.

The diagnostic initialized two formal Naruto OID2 combatants in mode0/background23 with seed `0x28A55A5A`, then preplaced formal OID219/type3/action51 at slot20, X100/Z542. Slot0's X251 and team1 make the positive group1 case; group3 has no living same-group character. Each case ran eight neutral complete Session ticks. `run-01/positive.csv` records controller removal and one child in slot50 at tick1, precise X103/Z542, Vx2, target slot0. At tick6 it records two live OID219 children; that later growth is observed, not independently characterized here. `run-01/negative.csv` records controller removal and zero children in all eight ticks. Probe exit was 0 (`positive_birth=1 negative_birth=0`).

The probe compiled with MinGW g++ 15.1.0, C++17, all 28 current core translation units plus playable `game_session.cpp` and `selection_flow.cpp`, with `-municode`; `compile-v2.log` has exit0. The initial `compile.log` failed because the new diagnostic used an incorrect `Entity28` type name; the scoped correction to `EntityState28` was compiled and run in the second invocation. Both logs are preserved.

Repository checks after this script change: `Tools/Validate-ChangeLedger.ps1` exit0 (its pre-existing unrelated warnings remain in `change-ledger-validation.log`), and `git -c core.safecrlf=false diff --check` exit0.

This is a controlled paired-source complete-Session fixture. The formal root EXE's current LFR input format cannot encode the extra preplaced OID219, so no root-EXE same-world or GPU parity is claimed. The prior Unity `NTSD28-USER-SOURCE-HITFA5-TARGET-VELOCITY-001` direct-frame focused tests do not cover the full Driver. The next independent Q07/D-024 gate is a governed Unity full-Driver fixture with the same source-rule positions, group, frame, owner and target, followed by a scoped first-difference comparison. No formal DAT value, Unity production code, Scene, image, mode Asset or nonbattle behavior was changed for this witness.
