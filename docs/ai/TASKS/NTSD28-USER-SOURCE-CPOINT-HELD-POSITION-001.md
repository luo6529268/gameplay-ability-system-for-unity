# NTSD28-USER-SOURCE-CPOINT-HELD-POSITION-001

Status 2026-09-24: `IN_PROGRESS / SOURCE_FIRST`; parent D-024, Q07 paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, playable `BattleWorld28::settle_catch_relations` in `source/ntsd28_core/src/simulation/battle_world.cpp:6170-6203`, and user D-024. The formula reads catcher integer X/Z and frame-local CPoint/center offsets, writes caught integer X/Z and matching precise X/Z in the same settlement. For `cpoint.z==0`, cover remainder chooses Z±1; nonzero Z is a separate physical-output discrepancy requiring its own full transaction.

Unity pre-change: active `BattleCpointWriter.SyncHeldPosition` rewrites caught physical X/Z using the catcher physical integer position and raw local offsets, then syncs physical integers; initialized source-rule X/Z remain stale. A prior two-view production witness showed raw local offset 25 at both camera sizes.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs` source-only held X/Z placement for initialized catcher/caught carriers; `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointSettlementRemainderEditorTests.cs` focused two-view and incomplete-carrier assertions. Preserve current physical output, cover direction, pose, DAT, Scene, ProjectSettings and nonbattle code. Nonzero CPoint Z physical behavior and any Y/direction mismatch must be handled by a separate parity package.

Acceptance: same formal raw X/Z after moved catcher at factor1/configured full view, source integer mirror, no invented history if one carrier absent, existing settlement regression, original Editor compile and focused NUnit. Full Driver/Play/EXE pending.

Rollback: review and reverse only the declared script hunks, retaining pre-existing dirty work.
