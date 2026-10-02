# NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001

Status: `VERIFIED_SCOPED_NEGATIVE`. Parent: `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / F02`. The direct-spawn catalog screen is closed; F02/Q07 remain open. [Report](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001/REPORT.md).

Authority and question: current formal EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and corresponding playable `ObjectSpawnPlanner28::plan_frame` plus `PhysicsIntegrator28::step`. Existing OID124 natural source case produced state1000 frames but none with `|Vx|>9`; the accepted F02 Unity fix has focused/full-tick evidence yet lacks a naturally reached formal-root weapon entry. Before another Play attempt, enumerate formal DAT OPoint entries that spawn type4/6 children directly into state1000 and record declared horizontal velocity and predecessor frames. This is a candidate screen, not proof that any entry is naturally reachable or that F02 runs.

Editable path and scope: extend only `Tools/NTSD28Q07Diagnostics/fast_weapon_natural_reachability_probe.cpp` with a separate `discover-fast` mode using the existing `ObjectDefinitionCatalog28` parser and a no-overwrite CSV. Keep old `discover`, `ground`, `jump`, `air20`, `air40`, `dei-air`, `kar433`, `kar434`, and `spawn` behavior unchanged. Do not modify official source, Unity scripts, DAT values, images, Scene, input or non-battle code.

Acceptance: compile against the same formal playable source closure with zero errors, run new mode twice to independent output directories, compare CSV hashes and counts, then inspect candidate rows. Explicitly distinguish declared OPoint `dvx` from physical-entry post-friction `Vx`, multi-spawn spread, landing override and natural player input reachability. Preserve existing outputs and inspect source diff plus Change Ledger validation. If no direct high-speed candidate exists, record the bounded negative and choose another F02 precondition rather than fabricate a runtime positive.

Rollback: supersede this diagnostic with a forward correction; do not delete old result folders, restore user files or alter the formal DAT.
