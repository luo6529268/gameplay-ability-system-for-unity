# NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001

Status: VERIFIED_SCOPED_SCENE_TICK. Parent: BATCH-04/Q07; follow-up to NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001. Original Unity project only.

Authority: formal NTSD 2.8-Logan EXE SHA-256 B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable `battle_world_tests.cpp::test_world_applies_locked_kind_catalog_type3_bound_respond_transform` uses type-3 OID213 attacking OID206, selected `kind.dat` effect209/frame40, and checks transferred identity, team/owner, action/latch/history. The staged selected `data/kind.dat` SHA-256 is 39E30DF8D86A5FC374B26C80BE358A6503B2C3096D8681C586478D73A0900011.

Current gap: original Editor compilation and focused B5/ECS tests pass, but no controlled kind-dependent full tick in the saved Battle Scene's live World. The default formal 330-object catalog has no OID209, so ordinary roster Play is not a valid positive witness.

Exact script path: `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07KindDatLiveSceneTickProbeEditor.cs` only. Add a request-file Play-only probe in the existing original Editor; no production script, Scene, Prefab, ProjectSettings, content asset, or nonbattle path. Temporary test entities may be registered only in Play World, and must be removed or destroyed on exit. Never write them into the saved Scene.

Acceptance: preflight original saved Battle Scene and idle Editor; verify the live World has a valid selected kind record effect209/frame40 with bound213/respond206; pause live driver, register controlled type-3 OID213/OID206 at free runtime slots, run exactly one full production driver tick, record candidate/transform outputs and first failure, then exit Play. Confirm Scene hashes and dirty state unchanged, request terminal, Editor back in EditMode, and no remaining test entities. A direct writer call alone does not satisfy this task. Narrow compile/test and Change Ledger validation only; this does not claim physical input, ordinary catalog reachability, Player, EXE pixel or full Q07 parity.

Rollback: remove only this new diagnostic script and its meta after explicit approval required by AGENTS.md for deletion; existing dirty files are protected. The request/result files are diagnostics, not project content.

Result: original Editor PID173216 compiled the probe and entered the saved Battle Scene. The live selected catalog contained one effect209/frame40 bound213/respond206 record; production driver full tick 5→6 changed temporary target OID206 to OID213/type3, action/history40, team4/owner7, special-hit latch true and attacker definition reference. After unregister, World object count stayed 4 and pool borrowers stayed 2; Editor returned idle EditMode and both Scene SHA-256 values stayed unchanged. See `artifacts/diagnostics/NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001/result.json`. This is a controlled synthetic pair, not a natural roster skill or formal EXE visual witness.
