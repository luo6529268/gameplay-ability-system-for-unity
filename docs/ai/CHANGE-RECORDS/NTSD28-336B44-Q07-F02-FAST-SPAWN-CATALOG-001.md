<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fast_weapon_natural_reachability_probe.cpp
authority: 336B44 formal playable ObjectSpawnPlanner28 and PhysicsIntegrator28 plus Q07/F02 natural-entry gap
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001.md
-->

# NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001

Before change: the existing standalone C++ probe can screen selected natural GameSession chains and has an unrelated airborne OPoint catalog mode, but has no formal catalog screen for F02 type4/6 state1000 spawn velocity. The new mode will only read official parsed definitions and write a separate no-overwrite diagnostic CSV. Existing modes and output files are protected.

Expected side effects: one diagnostic executable compiled under `artifacts/diagnostics/NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001/`, two independently named CSV directories, plus documentation. No production or content effect. Acceptance and forward-correction rollback are specified in the Task. Compile/dual-run results and limitations will be appended after the script change.

After change: the only code diff is the new `discover_fast_spawn_routes` mode and its dispatch in the declared path. Existing modes retain their branches. It reads parsed formal definitions, writes a no-overwrite CSV, and bounds initial X speed using the playable planner's multi-spawn spread. The 330-definition catalog yields 13 type-4/6 state-1000 direct OPoints, all child OID422 and all initial `|Vx|` bounds at most 5; none meets strict `>9`. This is a bounded negative for birth, not global F02 reachability. [Evidence](../../../artifacts/diagnostics/NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001/REPORT.md).

Validation: exact `g++` command in artifact `compile-argv.txt` exited 0 and `compile-output.txt` has no diagnostics. Corrected independent `discover-fast` runs each exited 0, each produced 13 rows, and both CSV SHA-256 values equal `9C5C41B46B188FC8F23C1E14D26FCB59CB41B59D39162E0C04D779D0ADDBEDD6`. Two prior wrong-root exit-4 invocations are preserved. `git diff --check` passed for this package and the scoped Change Ledger validator passed. Whole-worktree validator failed only on the unrelated pre-existing `Assets/NTSD/Scripts/UI/UIButton.cs` diff without record; no file outside this task's code path was altered to conceal it. Original Unity Editor/Scene, formal root LFR, and natural F02 Play were not run by this catalog screen.

Rollback: retain this diagnostic and result as versioned evidence; a future correction should supersede it and preserve both CSVs. Do not delete content or change DAT values.
