# NTSD28-Q07-D024-HAN-MAPPED-BATTLE-PLAY-001

Status: `RUNTIME_PENDING / SCOPED_CANDIDATE_MATCH / PROJECT_MAP_Z_BOUNDARY_OPEN`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / D-024`.

User requirement: keep one shared conversion entrance for source-rule battle distances and DAT-local geometry. `SimulationWorld.SpatialProjection` already owns the 1333×730 to configured-view X/Z ratios and source/view position and delta mapping. This package validates its existing production consumers; it must not introduce another ratio or edit DAT values.

Authority and matched input: formal release EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `SimulationTickDriver28::step` and `BattleWorld28` candidate path. Existing paired formal trace starts Han OID726 at source X500/Z650 and Lee OID7 at source X520 (near) or X580 (far)/Z650. First action146 candidate is 1 near, 0 far. The old Unity Q07 probe used those source numbers directly as physical positions and is not an equivalent D-024 input.

Exact script scope: only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09HanEarthquakeBattlePlayProbeEditor.cs`. Add an opt-in request flag that maps formal source starts to Unity physical X/Z through `world.SpatialProjection.SourceToViewX/Z(..., 0)` and initializes source carriers to the original source positions. Old requests remain byte-compatible in behavior. Include actual source/physical starts and stage bounds in the new report, assert the mapped starts and capture the in-collector and post-tick first action146 branch. Run near/far with unique result names; do not overwrite old requests or diagnostics. No production, DAT, Scene, camera, map, mode, nonbattle or formal-source edits.

Validation: generated C# compile, original idle Editor script compile, two bounded original Battle Scene Play runs through the existing request bridge, match mapped source inputs and first candidate 1/0 with the formal paired control, check ordered shutdown/zero borrowers and protected file hashes, then validator/diff check. If map stage clamps mapped positions, retain the failure report and classify boundary domains before any production change. No full-case suite.

Rollback: inverse only the optional probe fields and mapped-start branch without touching existing dirty user work or historical results. Deleting any existing artifact requires separate authorization.

Result: original Editor mapped near/far first action146 completed-row candidate counts match formal 1/0, with ordered shutdown and zero borrowers. Both mapped Z650 physical positions exceed the project's live stage Z maximum 760, so this is a limited diagnostic match and not valid in-map natural acceptance. Exact inputs/results and unchanged protected hashes: [acceptance](../../../artifacts/diagnostics/NTSD28-Q07-D024-HAN-CANDIDATE-BRANCH-001/MAPPED-NEAR-FAR-ACCEPTANCE-20260929.md).
