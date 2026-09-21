<!-- CHANGE-RECORD
id: NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001
status: VERIFIED
change-kind: FUSION_PERSISTENT_CARRIERS
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldEntityRuntimeSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleWorldCoreScalarSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Snapshot/BattleStateSnapshotRestore.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionPersistentCarriersEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05TraceIdentityEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Tools/NTSD28Parity/TraceContentIdentity.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeComboActionTransactionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionBirthIdentityEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionFeatureProjectionEditorTests.cs
authority: Current formal source entity birth and fusion writers, Host feature globals; exact producer audit.
evidence: InitialRED5; storage14/14 and related7PASS, Parity84/84+19/19, fresh native/Unityheader; STORAGE-STAGE-RESULT.md; producers pending.
-->

# Fusion persistent carrier integration

IN_PROGRESS. Parent FUSION-CATALOG-TRANSACTION-WITNESS-001; authority and producer table artifacts/diagnostics/NTSD28-Q06-FUSION-CARRIER-PRODUCER-AUDIT-001/CARRIER-PRODUCER-MATRIX.md. Existing prepared catalog verified. Total goal remains ACTIVE.

Initial exact12 files cover storage/snapshot/schema stage, not birth/consumer activation. Before birth/consumer edits append their exact paths to this same Record and task. Do not declare this whole integration verified merely because copy tests pass.

Entity additions NativeAiProfileObjectId (-1 constructor/reset; explicit birth later sets bmp use_ai default0), NativeDefinitionDropMode(default/reset0), FusionDisplayTimer190(default/reset0). Canonical copy covers both entity and independent raw-slot snapshots. No automatic initialization on FrameCache identity observer, registration, snapshot DAT reload or ordinary transform. Existing RenderPicOffset remains318. World additions FusionFirstFeatureGate4A8428/FusionSecondFeatureGate4A842C false/resetfalse in BattleRuntimeState, core snapshot/restore, checksum. Do not conflate existing entity FeatureGate with World state or project from getters.

Version contract coordinated entity16/aggregate24/checksum27/worldCore12, shells unchanged2/2. Native diagnostic content schema fields and Parity schemas/current identity tests follow; content V3 hashing bytes unchanged. Historical evidence untouched; rejection controls verify old nested schemas. Raw47/3 unchanged. New fields unused by current fusion until explicit later transaction implementation.

Validation initial reflective field tests RED then copy/reset/global snapshot/checksum focused; reuse existing generic entity/raw snapshot and aggregate restore tests if needed. No full unrelated combat tests per edit. Native capture rebuild/header verification once stable; publication hash remains same, schema changes. Initial staged progress must be clearly reported if producer/consumer or final runtime evidence not finished.

Persistent managed fields require no new lifecycle owner; pool Reset and existing World Reset plus ordered shutdown retained. Rollback scoped hunks only, preserve user work, no Scene/resources/nonbattle/Server/menu/network changes. Follow-up exact birth sites (character ModuleBind, weapon/special/other initialization) and input alias consumer require predeclared amendment and evidence for preservation. Host feature key sequence/projection timing is a separate pending behavioral seam, not implicitly implemented here.

Storage/schema code written. Initial jobca8b60a42a67417a975915ec1cb1aa51:21 cases,9 checksum probes failed because new test mistakenly queried separate diagnostic canonical JSON checksum rather than production CaptureRuntimeChecksum64. All initial field/reset cases, generic full entity/raw canonical copy and Q05 trace checks passed. Original XML preserved initial-checksum-probe-results.xml; fixture corrected to actual hot-path64 checksum, production checksum implementation unchanged. Diagnostic JSON projection is a separate partial diagnostic contract, not certified by this package. Birth/input/global projection still pending; do not mark VERIFIED.

# Storage stage evidence — parent integration remains open

IN_PROGRESS / STORAGE_SCHEMA_FOCUSED_PASS_PRODUCERS_PENDING.

Three entity carriers and two World globals now have defaults, canonical copy/reset, core snapshot capture/restore and actual hot-path64 checksum coverage. Current schemas entity16/aggregate24/checksum27/core12; character/base shells2/2. V3 content hash math unchanged. No birth/fusion/input/global projection behavior activated.

Actual evidence:
- Initial reflective5 RED jobe75d61d4351d4b24aa0ee676ceeefe13, missing fields; red-results.xml.
- Initial joint21 jobca8b60a42a67417a975915ec1cb1aa51:12 PASS/9 FAIL. Nine new checksum probes used partial diagnostic JSON rather than production64 checksum. Failure XML retained initial-checksum-probe-results.xml. Generic full entity+raw canonical snapshot case and Q05 trace6 passed here; no production changes since then.
- Fixture corrected to CaptureRuntimeChecksum64; job130432bfbcb54709bbec6bbf83fb5684 new14/14 PASS,0.9476202s. storage-focused-results.xml includes each new field hash sensitivity and all4 global boolean combinations with raw snapshot restore. Prior passing unrelated cases not rerun merely for fixture correction.
- Unity refreshed and completed compile/domain reload, error-CS query0 before first joint. No new production compile changes after it.
- Parity new old-schema controls RED84/3FAIL then84/84 PASS; raw19/19 PASS. Reports preserved.
- Native runner fresh build and3tick formal canonical roots capture. native-build/build-manifest.json, native-formal.jsonl, native-vector-check.json9 checksPASS; binary8F7FB285E850578FB615866C66D49E3D90316CE07EF17F50BA3243CD20825E87. SOURCE_MODEL_DIAGNOSTIC_ONLY, not formal EXE recording.
- Fresh native and Unity content headers exactly equal including16/24/27 schemas (joint-header-check.json); O/F/S/C/M and projection remain previous independent vector. Parity validates native source model3ticks6entityrecords, certificatefalse.
- Independent read-only reviewer confirmed storage/copy/reset/restore/hash and legitimacy of corrected test entry; no definite omission found.
- Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6 preserved. No Scene/resources/nonbattle/Server edits, computer-use, commit or push. No fullSelfCheck/Play sweep repeated for storage staging.

Still required before this integration can be VERIFIED: explicit birth AIalias/drop initialization, preservation on ordinary DAT rebind and snapshot restore, input alias consumer, explicit global configuration and exact projection timing (not getters), and corresponding representative producer tests. Fusion display190 assignment belongs to full record-driven fusion transaction. Must amend exact ownership before new script edits. Existing diagnostic canonical JSON and raw47/3 are not promoted by hot-path checksum27; track diagnostics separately.

Next exact birth investigation: shared helper in LF2Entity plus LF2Character.ModuleBind, weapon/special/other InitializeFrame; snapshot CreateSnapshotShell currently calls ModuleBind(initializeNativeArmorRuntime:false). Do not silently repurpose armor-only flag: use explicit spawn identity behavior and preserve restored fields. BattleSpawnVitalsWriter is OPoint-only and insufficient for all entry paths. Current RouteNativeHitJa reads wrapper.use_ai and should move to persisted alias only once birth writers are ready. Global pair config/projection sources documented in prior producer audit. Parent fusion transaction and Q06 incomplete; Q07 not migrated; goalACTIVE.

## Exact birth/input amendment before edits

Add nine declared paths (total21). LF2Entity.InitializeNativeDefinitionIdentityForSpawn reads actual current definition native bmp use_ai/drop(default0), with manual-wrapper use_ai fallback if metadata absent. It changes only alias/drop, not display190 or other state. Character ModuleBind gains independent optional initializeNativeDefinitionIdentity=true, leaving armor flag semantics intact; snapshot shell caller explicitly passes false for both. Weapon/special/other call helper immediately after their birth InitializeFrame Load. General FrameCache observer/ordinary DAT rebind/snapshot restore do not write fields. Input RouteNativeHitJa consumes persisted alias. Old alias fixture explicitly sets persistent field after changing its synthetic wrapper; no authority changes. New tests use real logic factory class representatives0/1/3/5 with pool reuse and altered subsequent DAT, plus character ModuleBind/restore exception. Existing class-equivalent weapon types2/4/6 reuse path evidence rather than repeating character roster. Source spawn1262/1263 and input_routing482 are requirements. Stage/roster share ModuleBind caller, distinguish that static caller proof from actual stage scene acceptance. Full global projection/feature configuration and fusion transaction remain pending.

# Birth and input stage result

IN_PROGRESS / BIRTH_INPUT_FOCUSED_PASS_GLOBAL_PROJECTION_PENDING. Same persistent carrier package,21 exact declared script paths after9-path amendment. Full fusion behavior not closed.

Implemented helper LF2Entity.InitializeNativeDefinitionIdentityForSpawn: only alias/drop from native BMP with default0; manual-wrapper alias fallback when metadata absent. Character ModuleBind separate optional initializeNativeDefinitionIdentity, snapshot caller passesfalse independently of armorflag. Weapon/special/other birth frame initialization calls helper. Common FrameCache observer and same-shell DAT reload unchanged. Input RouteNativeHitJa uses persisted alias, preserving ObjectId6 alternative and all prior action gates.

Evidence:
- Actual four class birth paths0/1/3/5 initially RED4 alias-1 vs31, joba0821e34c77f407da281f32278693696; birth-red-results.xml.
- Final joint18/18 job13e9bdc8a1264cc385db1123cfc43b6d,7.0622695s: newbirth7 (four class paths/poolreuse/rebind preservation, ModuleBindbirth vs explicitrestorefalse2, missing-native/manualwrapper1); persistedalias-vs-currentDAT input2; oldspecialfamily1; mutableDATrestore/atomicity8. birth-focused-results.xml.
- Additional directly affected snapshot shell/freed entity replay2/2 job1a0df8ce8660436a8454dde9e7b2441a,0.5140155s; birth-snapshot-shell-results.xml. Existing whole snapshots/restores checked; no claim this exhausts all runtime profiles/epoch cases.
- Independent finalbirth diff review found no material issue. Reviewer confirmed noncharacter snapshot Init values are subsequently replaced by canonical copy, same-shell restore does not invokehelper, and independent character flag preserves snapshot binding semantics.
- Current Unity compiled after production/test changes and successfully ran tests. Scope includes shared code used by renderer births; four representative test cases use actual logic factory. Stage/roster reuse ModuleBind by source/caller proof, not newly measured fullstage scene or physical input proof.
- Earlier storage14/14, generic entity/raw snapshot and schema traces, Parity84/19, native header evidence reused; these underlying storage/schema files unchanged in birthstage. No repeated fullSelfCheck/Play sweep. No Scene/resource/nonbattle/Server writes, no computer-use/commit/push.

Next: sameTask explicit World globalfeature configuration and correct first-global→entity projection (init/acceptedtoggle/Host preclassification/poststory predriver). Must declare exact additional code paths before edits. Unity raw scenario currently lacks source same-named fields; add config projection without menu or per-getter fallback. Read shared actual Host/worker tick entry before deciding placement so Manual/Local/worker paths agree; do not just overwrite arbitrary entity fixtures. Then complete record-driven fusion transaction/source4 and targeted runtime acceptance. Display190 assignment still pending fusion implementation; NativeAI alias consumer audit still open. Q06 incomplete/Q07 unmigrated/goalACTIVE.

## Feature configuration/projection amendment before edits

Add exact5 paths (total26). Existing SimulationRegistryModule owns allocation-free slot iteration: project first global flag to committed entity and its occupied-slot raw input carrier, skip OidMergeDormant (native suspended optional is absent), preserve unoccupied raw slots and second global flag. World exposes internal ConfigureFusionFeatureGates(first,second) explicit boundary and internal ProjectFusionFeatureGateToActiveEntities(). Configure sets globals and projects currently present entities; registration does not inherit feature flag (native midtick births retain default until Host projection). Existing common NTSDBattleTickSystem.RunTick calls projection once before BattleFlow; both main and dedicated paths share it. This places projection before existing core input, not in getters. Current Unity stage progression still has separate documented late-placement backlog; do not move stage passes here or claim native pre-story Host order fullyclosed. Initialization rawscenario same source boolean names populated and Configure called after roster spawn. No menu/feature-key scanning implementation or cross-scene toggle persistence implied.

Focused RED via reflective API then explicit four combinations/reversiblefalse, dormant/empty-raw exclusion, latebirth-before-nextprojection, main/worker common tick, and scenario config bridge. Preserve previously verified immutable content/storage/birth behavior; no schema re-bump, network or resource edits. This stage does not activate fusion record consumer. Global core snapshot already carries two flags. Future accepted Host key command should use explicit boundary after upstream admission is implemented; not authorized to guess key sequences now.

# Explicit feature config and battle projection stage

IN_PROGRESS / FEATURE_CONFIG_CORE_PROJECTION_PASS_FULL_FUSION_PENDING.

Five newly declared paths (total26) implement World.ConfigureFusionFeatureGates(first,second), registry projection to committed non-dormant entity and corresponding occupied raw gate, and common NTSDBattleTickSystem pre-BattleFlow call for main/worker. No singleton/IO or new manager, no registration-time inheritance. Scenario bools exactly fusionFirstFeatureGate4A8428/fusionSecondFeatureGate4A842C and configure runs after roster creation.

Evidence: initial7 RED job4d20e0166d324fd7beb504172a2a1555, feature-red-results.xml; final11/11 job553b98f94db849928070495c94152d5e,11.8773786s, feature-focused-results.xml. New10: four flag combinations and reverse, main/worker core entry2, dormant/empty raw exclusion, postprojection birth defaultfalse then actual nextticktrue, same-named scenario parsing/config2. Plus existing formal capture manager restoration1. Current source/build schema unchanged; prior data/birth/schema proofs reused. Dedicated worker test invokes shared worker tick entry directly, not a newly launched worker OS thread. No fullSelfCheck/Play sweep.

Independent scope review confirmed sourceinit2272/acceptedkey2490/preclassification2721/poststory3242. Do not add HP/lifecycle/link filters absent from source. Raw projection is one occupied input carrier only; no claim raw slot is an entity runtime mirror. Dormant raw and empty raw untouched.

Unclosed behavioral dependencies: source post-story second projection requires existing stage/Host placement audit; current Unity stage remains later, not moved in this task. Complete native feature-key admission/toggling UI and cross-scene persistence not implemented. Existing Host stopping contract prevents normal post-exit ticks; no menu changes. Thus core config/projection scopedPASS does not certify full formal Host behavior. Fusion current hardcoded consumer still not replaced, display190 write pending fusion, NativeAI alias read revisit remains. Keep carrier Task IN_PROGRESS until full integration evidence; goal/Q06 incomplete, Q07 unmigrated.

Next concrete implementation: parent fusion source4 witness is ready and prepared table/identity/entityfields/explicitglobalpair now exist. Create exact record-driven fusion transaction Task and Unity source4 RED, then replace whole merge/split transaction: record ordering, native action validation, gate/cover/HP semantics, atomic preflight, definition publication and suspended partner preservation. Preserve already verified C12/C25h placement/timer decrement. Do not stop at getter replacement or resetpartner. Carry Host-key/poststory obligations explicitly into later Q06/Host acceptance rather than pretending closed.

## Final scoped acceptance

VERIFIED / STORAGE_BIRTH_INPUT_EXPLICIT_CORE_FEATURE_PROJECTION.
Evidence: artifacts/diagnostics/NTSD28-Q06-FUSION-PERSISTENT-CARRIERS-001/ACCEPTANCE.md. Previous IN_PROGRESS checkpoints above are history; deferred Host keys/poststory/NativeAI and raw/diagnostic limitations remain explicitly open in shared acceptance.
