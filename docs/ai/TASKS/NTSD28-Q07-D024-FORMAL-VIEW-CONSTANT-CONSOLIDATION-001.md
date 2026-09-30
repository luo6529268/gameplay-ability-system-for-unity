# NTSD28-Q07-D024-FORMAL-VIEW-CONSTANT-CONSOLIDATION-001

Status: `COMPILE_PASS / STATIC_VERIFIED`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / D-024`.

User request (2026-09-29): centralize existing battle distance conversions so later changes have one entry. The immediately preceding `NTSD28-Q07-D024-UNIFIED-SPATIAL-PROJECTION-001` introduced `BattleSpatialProjection` and passed original-Editor focused tests 5/5. A production search found the only remaining literal formal viewport dimensions outside that value in `Assets/NTSD/Scripts/App/GameConfig.cs` (battle defaults) and `Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs` (two absent-Asset fallbacks). This package changes only those two script paths to read the central constants, without changing serialized `GameConfig.asset` or any configured/absent-Asset numeric result.

Acceptance: both references use `BattleSpatialProjection.FormalViewWidthPx/HeightPx`; production scan finds no other formal 1333/730 numeric literals except descriptive text; original Editor compile has zero C# errors; focused projection tests stay passed (rerun only if behavior or test changes); `Tools/Validate-ChangeLedger.ps1`, `git diff --check`, protected Scene/config hash check. No DAT/PNG/Scene/camera/map/mode, no nonbattle flow. Q07 remains open until birth/collision production wiring and paired Play.

Risk: a namespace or source-order compile error. Rollback: inverse patch of exactly the two declared references, preserving existing dirty work.
