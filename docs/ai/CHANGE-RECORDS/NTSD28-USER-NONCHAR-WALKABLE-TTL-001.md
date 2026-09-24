<!-- CHANGE-RECORD
id: NTSD28-USER-NONCHAR-WALKABLE-TTL-001
status: RUNTIME_PENDING
change-kind: D025_NONCHAR_WALKABLE_TTL
code-path: Assets/NTSD/Scripts/Simulation/Stage/BattleWalkableAreaSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NonCharacterWalkableTtlEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NonCharacterWalkablePlayProbeEditor.cs
authority: user D-025 walkable-area ten-second logic-time noncharacter cull; formal playable source for unaffected rules
evidence: docs/ai/TASKS/NTSD28-USER-NONCHAR-WALKABLE-TTL-001.md
-->

# NTSD28-USER-NONCHAR-WALKABLE-TTL-001

Created before any production script edits. The new user exception replaces only immediate noncharacter offstage X deletion; it must use the battle map walkable polygons and 10,000 ms of logical time (304 elapsed normal 33 ms ticks), leave characters and independent rules untouched, and preserve deterministic worker/snapshot/checksum behavior. Exact pre-change paths, acceptance, risk and rollback are in the Task Contract. Actual file/symbol changes, tests and remaining Play evidence follow after implementation.

2026-09-24 implementation: `SimulationStageRenderModule` captures enabled map polygon world vertices on the host before worker submit and publishes an immutable `BattleWalkableAreaSnapshot`; `SimulationWorld` exposes the pure query. `LF2Entity.ApplyPreFrameXBounds` retains type0 and OID122/123 position behavior, but routes all other types through a continuous outside-area timer and existing `FreeEntityLikeExe` at 304 elapsed ticks. `NTSDEntityRuntime` resets/copies the timer; runtime snapshot schema 18→19 and lockstep checksum schema 32→33 include it. The new Editor tests cover polygon union/edge, both PreFrame modes, 303/304 elapsed tick boundary, re-entry, type3/character, missing boundary, copy/reset, snapshot and checksum. The standard `BattleRuntimeSelfCheck.CheckPreFrameXBoundsMatrix` superseded assertions now verify no deletion without geometry and world removal at tick 305 after starting at tick 1.

Validation on the original project Editor: scripts refreshed/compiled without C# errors; focused `NTSD28NonCharacterWalkableTtlEditorTests` job `2b91ad635da04cb883341e479bae8805` 8/8 PASS; adjacent bounds/snapshot job `5c76e40ffd8b4b4d9f07947941ba82f9` 29/29 PASS and map/stage job `7b614222fa1949a2bda5b36c821273d0` 9/9 PASS. Fresh `Temp/NTSD_BattleRuntimeSelfCheck.result` at 2026-09-24 14:33 local is PASS after correcting the stale immediate-cull assertion; its preceding FAIL is preserved as the test-first contradiction. `Tools/Validate-ChangeLedger.ps1` PASSED (775 records). Task-scoped code `git diff --check` passed; whole-tree check only reports pre-existing/user Scene trailing whitespace at `NTSD_Battle.unity:2044-2045` and is not a D-025 code defect.

Remaining: actual `NTSD_Battle` Play map capture, worker tick, F5 cadence, exit/re-enter and observable object removal have not been witnessed for this package. Formal release immediate offstage cull intentionally differs under user D-025. Status remains `RUNTIME_PENDING`; do not claim full battle alignment. Snapshot/checksum schema changes require consumers to use the new schema, and no backwards-compatibility migration was introduced.
