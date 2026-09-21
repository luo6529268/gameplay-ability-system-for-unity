# NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001

IN_PROGRESS / FOCUSED_TEST_FIRST.

## Requirement and observed failure

Parent NTSD28-Q06-LOCKED-KIND-NATIVE-FRAME40-ADMISSION-001 must replay from a snapshot taken before a legitimate DAT200→213 definition transition. Existing local-shell replay job eb4986b07a5d4f4abc247d7f6e345405 FAIL1/1, EntityIdentityMismatch, is the measured RED. The source2 formal-kind witness proves the forward definition transition; restoring Unity local snapshots is a Unity adaptation requirement, not a newly invented C++ battle rule. Formal authority remains EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable closure07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F.

## Current implementation and proposed boundary

BattleStateSnapshotRestore.ValidateBattleStateSnapshotRestore preflight compares local shell StableId, EntityKind, current DAT id/type. The DAT id comparison rejects legitimate mutation. HasSnapshotFrameData then reads the current local FrameCache; LF2Entity.TryRestoreBaseShellForSnapshot also resolves saved current/collision frame IDs against that cache. Removing only the identity check is therefore incorrect. Existing factory materialization can use RuntimeDataCatalog.GetCharacterConfig(expected.CurrentDataObjectId), but replacing retained shells would alter renderer ownership and is not this task's chosen exit.

Investigate and implement a prepared expected-definition binding for the same local shell only after full preflight succeeds, before restoring frame descriptors. Validate definition, wrapper identity/type, required native frames and shell compatibility before mutating anything. Preserve exact same StableId, EntityKind, local shell and Renderer references. Do not relax cross-World/allocation-epoch policy, wire/schema, network recovery, renderer topology or ordered shutdown. Character-controller/weapon-data side effects and FrameCache.Load behavior require audit before deciding the final production helper. No generic lifecycle redesign or async resource loading.

## Exact prospective ownership

- Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs: preflight identity/data resolution, HasSnapshotFrameData, prepared binding before base-shell restore. Production edits await the remaining binding-side-effect audit.
- Assets/NTSD/Scripts/Test/Editor/BattleStateSnapshotRestoreEditorTests.cs: focused retained-shell mutable-definition restore and failed-preflight nonmutation checks, preserving existing StableId rejection.
- Existing parent fixture NTSD28Q06LockedKindNativeFrame40AdmissionEditorTests.cs is reused unchanged for original replay; any required test edit must first be explicitly added to this record.

No other script paths are authorized by this record. No Scene, Prefab, DAT, sprite, asset importer, package, framework or nonbattle changes. If LF2Entity/factory/cache edits prove necessary, amend the exact scope before editing. Snapshot content/header/schema remains unchanged.

## Acceptance and verification budget

1. Confirm current RED remains the original pre-transform retained-shell replay, not an easier substitute.
2. Focused valid same-shell 200→213→200 restore must restore expected DAT wrapper, current/collision descriptors, complete saved battle state and deterministic continuation.
3. Reject StableId/entity-kind mismatch, unavailable or inconsistent catalog definition/type, unavailable required frame, and late-slot validation failure without changing earlier entities, world topology, render bindings or pool borrowers. Preserve existing pure-value restore behavior.
4. Compile0 errors, relevant snapshot/identity tests plus parent source2 replay. Reuse parent source/captured/Play evidence where untouched. Only after production package stabilizes run one related joint regression and one SelfCheck, with representative retained-renderer Play/closure if affected. Do not rerun all characters/scenarios for test-only edits.
5. Independent review of preflight nonmutation, binding order and existing ownership invariants before VERIFIED.

## Risks, rollback and current status

Main risks are resolving saved frames from the wrong DAT, resetting mutable runtime while loading definitions, and retaining stale controller/cache/presentation references. No new runtime manager, queue, worker or pool is introduced; existing shutdown contract unchanged. Rollback consists only of reviewed edits owned by this Change ID, preserving all pre-existing user and task work; no automatic Git reset/restore/delete. Parent remains IN_PROGRESS until its original replay exit succeeds. Q06 incomplete, Q07 not migrated, total goal ACTIVE.

At creation only read-only diagnosis and documentation have occurred. No implementation, compilation or new Unity tests were run for this dependency.

## Binding audit update — 2026-09-21

Observed LF2FrameCache.Load clears the existing frame table, assigns Wrapper, fills authored native frame entries and invokes ILF2FrameCacheObserver.OnFrameCacheIdentityChanged. LF2Entity observer calls PublishIdentityMetadataForSimulation: invalidates data-type tick cache, writes Runtime.ObjType/EntityType, then registeredWorld.IdentityWriter.SyncFromEntity. Thus Load is a mutation and must not execute in ValidateBattleStateSnapshotRestore. The getter RuntimeDataCatalog.GetCharacterConfig only looks up the prepared dictionary; use it for preflight resolution without singleton/resource fallback. LF2LivingObject._FrameDataWrapper is already a FrameCache.Wrapper getter, not a second mutable wrapper field.

TryRestoreBattleStateSnapshot currently validates the complete snapshot first, unbinds rest, restores topology, copies raw/entity runtime, then restores base/living/character shells. Candidate binding placement is after saved runtime copy and before TryRestoreBaseShellForSnapshot, with explicit validation of identity metadata consistency. FrameCache observer may publish metadata, so verify final registry values against saved identity and preserve retained renderer ownership; no claim of implementation is made yet. Character-specific controller and weapon cache compatibility remains to be checked before broadening beyond the evidenced type3 transition. Do not silently narrow the overall mutable-definition recovery requirement to one passing case.

Existing IdentityMismatchFailsBeforeMutatingWorld mutates StableId, then checks X and AiRand15 remain unchanged on rejection. Retain this guard. Add a later-slot invalid-definition case to prove the proposed preflight never binds an earlier valid transformed entity before all checks pass.

Documentation verification: Tools/Validate-ChangeLedger.ps1 PASS609 records/40 governed code files; git diff --check passed (existing LF/CRLF advisory only). No dependency scripts changed or new Unity tests executed.

Pre-edit test declaration: existing BattleStateSnapshotRestoreEditorTests will add same-type DAT identity restore representatives for type0/type3, exact wrapper/current/collision/checksum checks and a later-slot StableId rejection proving earlier transformed entity is unchanged. Production still unchanged; original parent replay remains required.

Focused after-binding job cfffb63e14c74ea0b4f5eb2d92715aa5: XML confirms13/13PASS,3.866682s, including original parent LockedKindRepresentativesSurviveLocalSnapshotReplay. Compiled Editor idle and read_console error CS returned0. Evidence after-binding-fix/results.xml; initial RED remains focused-red/results.xml. Parent replay is now measured PASS, but dependency/parent remain IN_PROGRESS pending rejection hardening and representative renderer verification.

Independent review agrees Load placement and rejects ModuleBind/weapon initialization (would reset input/rest/HP). Required remaining before package closure: expected-definition/config identity consistency rejection; saved descriptor unavailable in expected DAT rejection; saved raw/entity ObjType/EntityType consistency check before observer can overwrite invalid payload; representative retained-renderer restore. Saved999 exists only in original DAT in the new success test, already proving expected-DAT descriptor selection. Late-slot StableId rejection already passes. LF2CharacterData has no independent top-level DAT type carrier found; type authority is ObjectDefinition plus entity/snapshot metadata, do not invent a field.

Next precise code-read: BattleWorldEntityRuntimeSnapshotBuffer stores private entityRuntimes/rawRuntimes. Determine a nonmutating metadata validation method and declare that exact additional file in this record BEFORE editing; do not allocate scratch runtime per slot or use reflection in production. Add only necessary corruption tests to existing test file. Cross-type and weapon subclass coverage remain explicit unclosed follow-up; no broad verification claim. Do not run full SelfCheck yet because implementation may change; related13 evidence remains valid until affected by edits. No Play/full SelfCheck executed in this turn.

Pre-edit expanded exact scope: BattleWorldEntityRuntimeSnapshot.cs adds internal read-only HasConsistentDataIdentity(runtimeSlot, expectedDataType) over both existing private canonical buffers, no allocations or schema changes. Restore caller invokes only on changed-DAT retained-shell path, before any mutation. Tests add corrupt entity/raw type, mismatched config id, expected999 unavailable and missing expected characterData. Existing stable-id and success representatives retained. First run rejection cases before helper implementation to measure any concealed invalid-payload acceptance.

Raw ownership correction applied: HasConsistentEntityDataType now checks entity payload only. Test case3 seeds independent raw type6/ObjType1 before capture, preserves it exactly while entity type3 is restored; case2 still rejects invalid entity types. Independent reviewer explicitly withdrew prior raw-equality advice and confirmed RuntimeSlotTable/capture/Q05 ownership evidence. See joint-interrupted/OBSERVATION.md. Current corrected code CODE_WRITTEN, not yet compiled or tested: old Editor gone, fresh lock is actually held by a process (read-exclusive share violation), unity status empty. No second Editor started, no lock deletion. Next poll current bridge/project readiness, not lost job recreation while project is unavailable.

Pre-edit renderer validation extension: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06LockedKindNativeFrame40AdmissionEditorTests.cs only. Extract existing two-case local replay into shared RunSnapshotReplay(profile, mode, renderer); existing EditMode test calls false unchanged. Play probe accepts separate run-restore request, runs only Authority/DataOriented retained-renderer two-case replay instead of repeating old4-case matrix. Assert original entity/Renderer/LogicObject retained, ForceRefreshPresentation after restore, source capture and replay same, Scene checksum/borrower counts unchanged, then existing Q05 close. This validates renderer binding lifecycle/refresh path, not visual pixel equality; assets still preQ07. Preserve old run request semantics. No new production paths.

Final declared-scope acceptance: VERIFIED. See artifacts/diagnostics/NTSD28-Q06-SNAPSHOT-MUTABLE-DAT-IDENTITY-RESTORE-001/ACCEPTANCE.md for exact evidence, raw-domain correction and scope exclusions. Earlier IN_PROGRESS/RED observations remain historical. Q06 and total goal are not complete.
