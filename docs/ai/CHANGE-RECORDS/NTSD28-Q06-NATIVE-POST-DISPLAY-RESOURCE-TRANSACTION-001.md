<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION-001
status: VERIFIED
change-kind: NATIVE_POST_DISPLAY_RESOURCE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativePostDisplayResourceWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativePostDisplayResourceEditorTests.cs
authority: Current formal playable BattleWorld28.advance_native_resources_post_display_range battle_world.cpp2389-2550 and C25 display then post then computer/frame; formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033, closure07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F.
evidence: Reuse immutable post.tsv2379 SHA4C8CC6013FF44CB2613C5AC2F42B5BF056522B4C564829B9D539BD2B49D8B644; display parent and birth dependencies VERIFIED; current production missing post owner. Test-first pending.
-->

# Post-display resource transaction

IN_PROGRESS / SOURCE_REUSE_AND_FOCUSED_RED_FIRST. Exact three paths declared before script edits. Task same ID defines scope/authority and R05/R07/R10 dependencies. Source2379 capture build manifest matches current identities, no source rebuild merely for repeat.

Unity before: late display then RefreshNativeComputerState/frame, no dedicated post-display transaction. Existing pre-display HP/MP and display writers are closed responsibilities, not to be rewritten. New stateless BattleNativePostDisplayResourceWriter.Advance(LF2Entity entity,int selectedModeFullRestoreGate18,bool hasStageBounds,int width,int zNear,int zFar) will implement entire ordered source contract; Late module calls once after display before computer/frame. Guard current DAT type0, current native descriptor valid, nonpending and actual lifecycle eligibility. Read previous078 against currentdefinition, not cached Frame.D or tick snapshot. frame_0mp writes only action, saved current descriptor controls restore; preserve action latch/entry until existing frame step.

State62..66 resource writes;405 integer stagecenter preserves integerY and syncs preciseXYZ then clears velocities;4000..4999 lives and3640..3645 group; creditGate-1/state!=63/timer>0 or mode2/3 restores baseHP and clears accumulated selfdamage/KO only; upper HP/maxMP bounds only, no lowerclamp/no HP<=effectiveMax normalization. Display has already read pre-limit values. Native source pass counters are diagnostic, not persistent gameplay fields; no schema fields added to copy them.

Production selectedMode18 uses explicitly named current default0 as existing GameSession selected_mode_hit_group_gate_18 default; selected records remain Q08, no invented mutable world mode or full-mode claim. Tests pass explicit mode values for original2379 matrix. Stage valid width>0 and orderednear/far from actual source definition; no stage.dat deployment. Need405 nonzero-motion focused evidence and trace assumptions recorded.

Validation: measured RED through2379-vector harness and actual late-loop representative, then smallest affected focused cases including raw-action-only/savedcurrentdescriptor/timer1/displayprelimit/invalid andtype/pending/405/poolreset or existingfields snapshot. Stablepackage once relevantjoint, SelfCheck, actualPlay/orderedclose; reuse unchanged previous evidence. Test count is not fullgame alignment. No newqueue/manager/cache runtimeowner; statelesswriter and existing pass obey 11-phase shutdown. No computer-use, nonbattle/Scene/resources/GAS/Mono edits, no persistent schema change. Preserve dirtytree and userHUDBg x30.

Rollback: only reviewed hunks from these exactpaths after user approval where required, no reset/restore/delete or edits to other ongoing changes. Before adding sourcefixture/newPlayprobe or widening ownership, amend exactcodepaths. Tests may be added before writer exists using reflection to record missingowner RED; distinguish it from actual per-vector mismatch counts. Do not claim2379 executed if resolution failed first.

## Pre-production correction: reuse existing mode18 carrier

Further exact authority chain: game_session.cpp2148 set_active_mode_hit_group_gate_18(config_.selected_mode_hit_group_gate_18);4165 options.resource_rules.selected_mode_full_restore_gate_18 = same config value. game_session.h327 default0. Unity Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 already exists with snapshot/checksum/raw projection; NTSD28-B5-WORLD-HIT-GROUP-MODE-GATE-CARRIER-001 explicitly reserves full-restore consumer. Therefore supersede above initial constant-only proposal BEFORE production edits: Late passes this existing current Worldcarrier into writer. Do not add a new field or schema, and do not ignore mode2/3 on an explicitly configured World. Complete selected-mode content producer remains Q08. Add productionmode2/3 focused coverage, not merely direct writer parameter tests. Current StageBounds28.valid is width>0 && z_near<=z_far.

## Pre-production slot qualification correction

Independent source/query audit: source entity(slot) only empty/bounds gate; post_range rereads slots then NativeLifecycleResolutionPending/type/currentdescriptor. Unity FindCurrent excludes PendingFlushDestroy, an independent local field (setter does not set NativeLifecycleResolutionPending; RunHitFa13FrameLogic has local-only writes). Hence supersede normal-only insertion proposal: each of four AdvanceNativeDisplay exits (3early/1normal) must then independently requery current slot via existing FindEntityByRuntimeSlotForNativeDisplay and call postwriter; preserve all original continues and frame eligibility. No relaxation of global IsActiveForCurrentPass. Writer must not reject localPendingFlushDestroy by name or reuse RecoveryStatusWriter.CanEnterNativeResource with extra exclusions. Existing display query excludes retired pendingUnregister/dormant, retains actual occupied localpending. Focused tests separate localpending/nativepending; actual callers for every early exit not yet claimed. This still uses the same declared LateModule path and no World/schema edits.

Measured RED job6eda2f8a183f4ad8b4e7e9486b542e66: actual6/1PASS5FAIL. Missingowner vector+405 tests each sourceVectorsExecuted0; productionprelimitframe501expected500 and modes2/3HP100expected500 are actualbehaviorRED. mode0 controlPASS. XML red-6eda2f8a archived. CompileCS0 beforehand. Proceed exact production writer+fourlate sites; do not misreport missingowner as2379 mismatches.

First implemented focused job554bbc2a actual6/5PASS1FAIL. 2379 vectors actually ran; onlyrow2351 differed due fixture setting source lifecycle_pending on localPendingFlushDestroy instead ofNativeLifecycleResolutionPending. Correct input mapping, not production gate. Independentreview also found adjacentRefreshNativeComputerState reads cachedFrame.D after newraw-action-only write; source simulation_tick_driver1003 re-reads currentnativeaction. Already-declaredLateModule symbol now explicitly included before its edit; add two actualcallerRED cases old0->new7005 andold7005->new0. Preserve writer saved-entry descriptor contract, no otherAI logic changes. Add local/nativepending productionpair independently.

## Focused result 2026-09-21 / job8866ce8ef4664d36a46df67d28a28876

Actual 10 tests, 10 passed, 0 failed, duration7.5187117s. Archived focused-8866ce8e.xml confirms sourceVectorsExecuted=2379; differences=0. Includes source-derived nonzero405, display/prelimit/timer ordering, mode0/2/3, independent local/native pending, and both post-action computer refresh directions. Computer RED bc4a28fc preserved; production now reads current Runtime.Frame native descriptor, without rebinding cached entry descriptor. Independent static review PASS, no new production issue. Earlier missing-owner and single-vector fixture errors remain historical evidence, not erased.

Scope remains FOCUSED_TEST_PASS, not VERIFIED. Remaining minimum: one real production frame-body continuation (existing FrameProbe only observes ordering), one combined snapshot/replay representative, then stable related joint/fullSelfCheck/realPlay/ordered close. No full-role retest. No new persistent fields or schema. Current run did not run fullSelfCheck or Play.

## Pre-edit focused continuation and replay coverage

Within the already declared test path only, add ProductionFrameBodyConsumesRawActionAndCommitsHistory and ProductionPostTransactionSnapshotRestoreReplaysSameResultAndChecksum. Use an exact LF2Character from the production logic factory and prepared catalog, no SimFrameTick override. Exercise frame_0mp -> 900, state7005, entry latch/counter mismatch, timer1 and previous405 stage-center transaction. Source frame_machine resets changed-action counter then increments to1; C25 later commits previous078. The replay representative captures pre-Late production snapshot, executes identical empty-input Late, restores and repeats, compares result and runtime checksum. No new production edits or standalone Unity run by worker; root owns compile and focused execution. Existing 10/2379 evidence stays unchanged.

Worker wrote the two declared continuation/replay tests and CreateProductionWorld/CaptureProductionResult/AssertProductionResult helpers. The factory helper supports optional renderer materialization for later root-owned Play reuse, but no Play polling entry was added. Final computer state assertion is4: refresh7005 writes5 and existing Late timer owner then decrements. No Unity compile/test was run by worker; root must execute the two new methods. Source-code expectation only until measured results exist.

Root pre-run review correction: production fixture uses slot20, outside native computer refresh slots0..9. Its existing computer9 must decay to8; do not expect refresh5/decay4. Keep slot20 and source-correct assertion; low-slot refresh is already covered by two passing tests. No production edit.

## Real frame/replay and related joint measured

jobbeec5066b5ac49b3ab13f72b71d554a7: 6/6 PASS, 0.9138499s. Two new real factory/native frame body and snapshot/replay representatives plus display-before-frame canonical/legacy two cases and computer/body timer two cases. Restored initial and replay result/checksum both match. Archive frame-replay-joint-beec5066.xml. Fresh domain reload completed, error CS0. This is real production Late, not full host tick or formal EXE recording.

Pre-edit Play acceptance scope (same declared test file): add NTSD28Q06NativePostDisplayResourcePlayProbe request consumer and VerifyProductionForPlay helper. Reuse production fixture with pooled Renderer, one real Late pass and existing assertions; recycle entities in finally before ordered shutdown, prove zero local borrowers. Poll only when existing scene is ready/paused, record scene checksum and global borrowers unchanged, then existing Q05 shutdown request. No scene save, new manager, production state, or schema. A diagnostic Editor-only poll exists only for acceptance; result explicitly synthetic catalog. Source prior2379 and joint results remain reusable after this test-only addition. Stable fullSelfCheck requested once after production stable; its result pending.

## Scoped final acceptance

# Scoped acceptance — post-display resource transaction

VERIFIED for the declared post-display transaction, not Q06 or overall battle alignment.

- Production paths: BattleNativePostDisplayResourceWriter.cs; BattleLateEntityLifecycleModule.cs. Tests: NTSD28Q06NativePostDisplayResourceEditorTests.cs. No new persistent schema or owner.
- Formal source identity/closure and original2379 SHA remain those in PRECHANGE-AUDIT and Change Record. Reused original source capture; no claim of formal EXE recording.
- focused-8866ce8e.xml:10/10 PASS, sourceVectorsExecuted=2379/differences=0. Includes current/previous descriptor distinction, resource/position gates and independent local/native pending. Nonzero405 is source-code-derived, original vectors have zero motion.
- frame-replay-joint-beec5066.xml:6/6 PASS; exact production LF2Character frame body, snapshot restore result/checksum and selected display/computer/timer neighbours. Slot20 computer9->8; separate slot0 tests prove refresh. No full host replay claim.
- selfcheck-093756.result: PASS,2026-09-21T09:37:56Z. One full run after production stable. Subsequent edits only add the Play acceptance helper/probe; refreshed compile has0 CS errors.
- play-093941.json:PASS,1 actual pooled Renderer/production Late representative with synthetic catalog; scene checksum unchanged and borrowers2->2.
- close-093942.json:PASS,real scene snapshot4->4; final World/slots/logic/render borrowers all0; Stopped held two frames.
- Editor returned idle/nonPlay; Scene dirtyfalse/root14 and SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6 unchanged.
- Independent read-only review PASS/GO-WITH-NOTES. Diagnostic setup failures before helper try have a cleanup limitation; current setup/Play succeeded and no borrowers leaked. No production closure defect found.
- Ledger623 records/117 governed diff files and git diff --check PASS. Counts cover dirty tree, not this three-file scope alone.

Preserve initial missing-owner RED, pending-column fixture correction, real computer-read RED and subsequent passing evidence. No production rule was relaxed for green. Full formal content/mode producer/Q07-Q08, three raw binding gaps and Q06 remaining responsibilities stay open. No resource replacement/deletion or nonbattle change occurred in this package.
