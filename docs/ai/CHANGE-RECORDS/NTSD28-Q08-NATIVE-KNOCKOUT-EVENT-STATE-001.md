<!-- CHANGE-RECORD
id: NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationBattleBufferModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldPendingEventSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q08NativeKnockoutEventStateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardReducedKnockoutProducerEditorTests.cs
authority: formal Logan playable battle_world.cpp record_native_knockout and prune, game_session.cpp post-core step, render_snapshot.cpp lifetime
evidence: artifacts/diagnostics/NTSD28-Q08-Q09-KNOCKOUT-FEED-CHAIN-AUDIT-001/REPORT.md
-->

# NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001

Created before any script modification for this package. Status `IN_PROGRESS`: no code, compile, test or runtime claim yet. The precise current-state finding is that Unity credits `KnockoutCount358` before lethal HP mutation but does not retain the formal per-KO record. The record is future Q09 presentation and Q10 sound input; aggregate count cannot reconstruct its source type, independent owners or time.

The [Task Contract](../../../artifacts/diagnostics/NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001/TASK-CONTRACT.md) specifies authority, candidate code paths, safety boundaries, expected producer/store/snapshot/checksum/reset behavior, focused validation and rollback. The record's `code-path` list must be corrected before editing if the full call chain requires other files. The pre-change review found snapshot schema 26, pending-event buffer schema 1 and lockstep checksum schema 29; versioned state changes must be explicit, with no old snapshot silently accepted. Capacity policy: prewarm at least one value record per runtime slot in the battle preparation window; after that, allow measured list growth if sustained events exceed capacity. `BattleRuntimeAllocationGate` seals covered pools/tasks, and `BattleManagedMemoryBoundary` reports allocations; an arbitrary event cap would change gameplay. No zero-allocation claim is made for an unbounded stress sequence.

Expected side effects: new Q08 battle-only event truth and versioned replay/checksum; no change to lethal HP, score or the already-correct KO counter, no Q09 renderer or Q10 audio implementation, no nonbattle/Scene/Prefab or framework rewrite. Eleven-stage ordered shutdown remains intact. No actual file changes or validation results yet. Rollback is limited to this package's reviewed diffs; preserve all pre-existing dirty files and staged content.

Pre-edit path correction: the native Session tail prune belongs in existing `NTSDBattleTickSystem.RunPresentationAndCleanupPhase` after `BattleResultsFlow` and before `RenderDispatch`; add this exact path to the Record and Task. The formal selected `#killtext` uses `times:70`; until Q09 publishes a parsed mode config, this package uses that captured value only for the active default mode and must mark other modes/content variants pending. The event carrier remains battle-only.

Pre-edit schema check: `BattleParitySnapshot` has a frozen Authority400 v3 domain that must keep its existing event payload. Only Unity extended/lockstep domains gain KO events and advance to v2. `BattleRuntimeSelfCheck` has a literal extended-v1 schema assertion, so this exact test path is added before editing; its expectation must advance with the real schema, with no old assertion silently skipped.

Pre-edit producer-test correction: the existing B5 focused fixture constructs direct standard/armor lethal hits and exact owner chains. Extend that same fixture to assert this package's new event without duplicating its test setup; add its exact test path before editing. A separate Q08 focused test owns tail/snapshot/checksum state.

2026-09-22 code written: `SimulationBattleBufferModule` now owns a value-record list prewarmed to at least runtime-slot count, with no count cap and native newest-tail-only pruning. `SimulationWorld` publishes read-only event access, captures the redirected source, distinct credit and four-owner slots, source object type and current logic tick, and clears the list on reset; `BattleDamageWriter` calls that writer beside its **existing single** pre-HP KO count increment. `NTSDBattleTickSystem` prunes after result flow and before render dispatch using the selected formal mode child's captured `times:70` pending Q09's data-driven handoff. Pending-event snapshot schema advances 1→2, aggregate battle snapshot 26→27, lockstep checksum 29→30, and Unity extended/lockstep parity schemas v1→v2. Frozen Authority400 v3 parity still uses its unchanged event projection. New focused Editor tests cover newest-tail expiry, snapshot copy/restore/checksum/reset, and existing B5 hit fixtures now assert event fields and rejected gates. SelfCheck's exact extended-schema expectation advances to v2. No Scene, Prefab, menu, result UI, Q09 renderer, Q10 sound, old resource or nonbattle code was changed by this package.

Validation: `git diff --check` for the touched scripts returned 0; new test meta GUID occurs once under `Assets`. Offline original-project `dotnet msbuild Assembly-CSharp-Editor.csproj -t:Build` returned 0 with a Temp-only **absolute** `CustomAfterMicrosoftCommonTargets` that adds the new test. `-getItem:Compile` confirmed that file as an evaluated Editor input and the offline DLL contains the test type; runtime DLL contains `NativeKnockoutEvent`. The earlier relative-target build also returned 0 but **omitted** the new test and is not acceptance evidence. Neither offline build updates the live Editor assemblies or runs NUnit. Original Editor PID 33236 is alive; the separate Q09 WORDS TestRunner request remains consumed without a result, so no competing Q08 TestRunner request was queued. Original Editor compile, focused NUnit, SelfCheck, full-tick native comparison, Play and exit/re-entry are pending. Status `CODE_WRITTEN`, not verified.

Review limits: the formal record can grow under sustained events; excess allocations are reported by the existing managed-memory boundary and gameplay records are retained. Snapshot restore may need to grow destination list capacity on an excess record count. The hard-coded captured 70 lifetime must be replaced by published formal mode input in Q09; this package does not claim all mode/content variants. Versioned checksum/schema changes require R15 trace comparator and any fixed-vector fixtures to be audited against same-version inputs. Rollback is limited to this Record's reviewed diffs, preserving prior dirty work.
