<!-- CHANGE-RECORD
id: NTSD28-Q07-D024-UNIFIED-SPATIAL-PROJECTION-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/BattleSpatialProjection.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024UnifiedSpatialProjectionEditorTests.cs
authority: user D-024 2026-09-29 unified proportional battle spatial conversion and formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033
evidence: docs/ai/TASKS/NTSD28-Q07-D024-UNIFIED-SPATIAL-PROJECTION-001.md; artifacts/diagnostics/NTSD28-Q07-D024-COLLISION-GEOMETRY-DOMAIN-AUDIT-001/UNIFIED-SPATIAL-PROJECTION-20260929.md
-->

# NTSD28-Q07-D024-UNIFIED-SPATIAL-PROJECTION-001

Status: `FOCUSED_TEST_PASS / PRODUCTION_CONSUMERS_PENDING`.

Original state: `SimulationWorld.ConfigureFixedViewRunDistance` calculates X/Z scale from 1333×730; 18 production files read the two scale properties, but no shared source↔physical anchor conversion exists. Han/Lee raw initial X520/X580 Unity cases cannot be called same-distance after D-024 movement scaling, and local ITR/BDY remains raw around physical centers. Existing user dirty files and all protected scenes/assets remain in place.

Planned change: introduce immutable `BattleSpatialProjection` with formal viewport identity and configured factors, explicit common-anchor source↔physical X/Z, and delta mapping. Keep the two `SimulationWorld` scale getters as aliases so existing readers do not change behavior; add focused Editor tests of the value and World bridge. No birth, collision, presentation, DAT, Scene, camera, map, mode, nonbattle or formal source change in this package. No hidden state or new lifecycle owner; value is recomputed by existing World configuration and reset with a new World.

Expected side effect: one common API for subsequent birth/collision packages; current Play behavior and checksums should remain identical. Risk is an accidental changed default or undersized-viewport factor, so tests cover identity and configured values. Rollback and validation are in the Task; actual script paths, commands, failures, and evidence will be appended immediately after edit.

Actual script change (2026-09-29): added `Core/BattleSpatialProjection.cs` (and its Unity meta) with immutable horizontal/depth scales, X/Z delta mapping, and explicit shared-anchor source↔view position methods. `SimulationWorld.ConfigureFixedViewRunDistance` now constructs that value; the two prior public scale properties are read-only aliases. Added `NTSD28D024UnifiedSpatialProjectionEditorTests.cs` (and meta) for World defaults/configuration, three-anchor Han/Lee near/far geometry math, negative X and Z roundtrip. No birth/collision consumer was changed. Initial targeted `git diff --check` passed; generated C# compile, original Editor compile/focused test, Scene hashes and Ledger validator are pending. This is not the Q07 collision fix.

Validation (2026-09-29): original Editor PID 11944, `NTSD_Battle.unity`, refreshed via local `refresh_unity(force/all/compile=request)` while idle. `Editor.log` reports `*** Tundra build success`, both Assembly-CSharp and Editor assemblies copied, 0 CS errors; pre-existing warnings remain. The original Editor `run_tests(EditMode, groupNames=[NTSD.Test.Editor.NTSD28D024UnifiedSpatialProjectionEditorTests])` job `c628c672d9884858a1eb693b7842dc1c` completed `Passed`: total 5, passed 5, failed 0, skipped 0. `Tools/Validate-ChangeLedger.ps1` returned `Change ledger validation PASSED` (1013 records, 42 governed code files in current dirty diff); focused `git diff --check` passed. Scene SHA-256: Battle `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`; both Git-clean. The new projection alone changes no production collision/birth semantics, so Q07/D-024 and the parent goal remain open.
