<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001
status: VERIFIED
change-kind: STATE9996_DIRECT_SPAWN_TRANSACTION
code-path: Tools/NTSD28AuthorityTrace/state9996_direct_spawn_transaction_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_state9996_direct_spawn_transaction_witness.py
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9996DirectSpawnEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Tasks/OPointCreateTask.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicEntityFactory.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeDirectSpawnWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeWeaponPieceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25DefinitionCloneEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06State9996DirectSpawnPlayProbe.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current playable BattleWorld28.materialize_special_state_clones and spawn_transient/spawn_at, formal closure07CD47; see Task.
evidence: ACCEPTANCE.md source12/813; immediate/shared20 PASS, following/C25 six PASS, fullSelfCheck09:06:05Z, realPlay2 and orderedclose09:07:01Z PASS; independent exit review PASS with explicit scope.
-->

# NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001

IN_PROGRESS / SOURCE_WITNESS_FIRST. Exact initial two diagnostic paths only, no Unity changes yet. Requirements/scope/validation/rollback in same-ID Task.

Observed before: Unity SpawnState9996Children early capacity break, HasAuthoredFrame(0..3), normalOPoint materialization followed by HP/HPBound/HP3/PP10. Current source uses 500 requestHP/MP without OPoint scaling, native descriptor lookup, random tuple before spawn_transient capacity refusal and continues remaining attempts. Older clone record closure39DDDA15 differs from current07CD47; preserve old evidence but establish fresh current source results before implementation.

Source witness outputs are diagnostic-only, not formal EXE capture. New tool script methods/fields and actual commands/results must be appended after writing. No Unity schema/lifecycle/manager/resource changes authorized by this initial record. Parent display/Q06/goal active, Q07 not migrated.

Source stage complete: 12 actual public-API cases, final build exit0, repeated163239 bytes SHA090B9773FCDB0C755D0E458CC719F97246702A9DFC006E2F4426D53AAA1D09F6. Final inputs are source-final/first.jsonl, not earlier source/ intermediate. BinaryBCE6BE776C63AF5BE12E72EC5FF2C19C9139EA4F144BF390C017100A4EEDA863; formal hashes unchanged. Independent813 checks PASS, root rerun PASS. Authored/implicit type0/3 all5+34calls; missing217 1+6calls/missing218 4+28; partial2/full0 still34; non-type0/pending/counter guards0; encoded8M explicitly unresolved. HP/MP500 ignoringohp20/omp30, maxMP731/weapon37/armor23recover17/alias34/drop2/display500; defend140 does not publish incomingScale (actual0). Following normal cases actually generate another5 (immediate helper did not advance source counter), captured but not independently modeled; options AI defaultsfalse declared, no counter compensation.

Pre-Unity-fixture amendment: exact new NTSD28Q06State9996DirectSpawnEditorTests.cs declared. Obtain immediate before/after source12 RED through real native producer and bothfactory wrappers as feasible, assert available dynamic slots/source event-derivedborn IDs and RNG. Source capacity1000 versus Unity existingAuthority400/Mobile1050 must be explicit; capacity cases compare same available-slot boundary, not equal full-world occupancy. Fillers carry no claimed gameplay parity beyond capacity. Source encoded8M is unresolved no-op control, not recovered Unity rule. Do not alter production until measured RED. Existing supported raw47/3 and persistent schemas unchanged.

## Measured RED and pre-production amendment (2026-09-21 08:44 UTC)

Job3056fb54de1d44819905403b46ba9144 completed actual12 / passed3 / failed9, duration1.321563s. Evidence unity-red-3056fb54/results.xml and immutable copied case0..11 JSON. All12 beforeDifferences empty. Failed0..7,9; passed8,10,11. Differences: HP/MP10 vs500, display100 vs500, native initial action/history, pending velocity0.1 vs0, implicit frame rejection, capacity2 consumes14 vs34 / full consumes0 vs34, pending boundary wrongly generates5. Pending is private producer boundary, not claim outer caller reaches it. No all-role/Play/fullSelfCheck was run for this RED.

Exact six production paths now declared above before editing. Add dedicated transient nativeState9996CloneSpawn, reset in Clear; keep weapon-piece OID0 admission independent. Extract existing generic native direct birth/admission to BattleNativeDirectSpawnWriter with original piece wrapper delegating, reuse display birth writer. Native clone uses both existing factories to bypass ordinary OPoint percentages, native current-frame admission and source pending guard; native capacity/factory refusal continues after required random draws. Legacy branch and ordinary OPoint/piece RNG/scheduling remain unchanged. Check measured pending motion against source reset, avoid blindly clearing unrelated state. No persistent carrier/schema or new lifecycle owner. Validation: focused source12, transient flag reset/ordinary reuse, shared initializer representatives, then stable package runtime gates. Rollback only this Change's reviewed hunks, preserve all preexisting dirty work. This amendment authorizes implementation, not VERIFIED status.

Production written / first focused verification: compile refresh succeeded, idle/reloaded, read_console errorCS0. Jobaa060e8c1173492998b4ffb6ba3caad2 actual19: clone12 + taskreuse1 + factoryadmission5 PASS, one piece representative failed before its behavior body at obsolete catalog projection3900ECBC509557DB, actual currentV3 0FEFD4B968D618FD. Archive unity-first-aa060e8c/results.xml and case JSON, preserve failure. Shared direct birth additionally initializes alias/drop, armor on all types, pending XYZ/count and scales; applies to piece/state18 wrappers too and needs representative regression. No following/renderer/fullSelfCheck yet.

Pre-test-oracle amendment: exact NTSD28Q06NativeWeaponPieceEditorTests.cs declared before edit. Update only ActualLoganPiecesMatchOriginalFunctions content-identity precondition: require formal object SHA4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C and current V3 projection0FEFD4B968D618FD established by FUSION-COMPOSITE-CONTENT-IDENTITY-001. Do not alter source vectors, birth/RNG expectations, thresholds or factory behavior. This is a stale identity assertion from before accepted composite schema, not evidence the piece behavior passed. Re-run only failed representative after correcting it; reuse unchanged18 passing checks.

## Focused checkpoint


- Native source-final/first.jsonl unchanged:12 cases,813 independently validated assertions, double SHA090B9773FCDB0C755D0E458CC719F97246702A9DFC006E2F4426D53AAA1D09F6.
- Unity initial RED job3056fb54de1d44819905403b46ba9144:12 actual,3 passed9 failed; all initial state comparisons matched. Immutable unity-red-3056fb54 XML/cases/summary.
- First post-change jobaa060e8c1173492998b4ffb6ba3caad2:19 actual,18 passed:12 full immediate cases,task pool reuse1,existing factory admission5. One piece test failed at obsolete pre-V3 catalog identity before behavior. Archived unity-first-aa060e8c/results.xml and all immediate cases. Reuse18 passes; no production change after this run.
- Corrected only that precondition, requiring current formal object SHA4EFE1D...C58C plus composite projection0FEFD4B968D618FD. Initial edit accessed property on LockstepSessionIdentity and produced CS1061; corrected to driver.World.RuntimeDataCatalog.LoganContentIdentity.ObjectDefinitionFingerprint. Fresh reload/read_console errorCS0 thereafter. Do not count the failed compile as a behavior failure or erase it.
- Follow-up job15cac58e7018401cbd5a9bd99e160f73:2/2 PASS,16.812760s. Exact tests ActualLoganPiecesMatchOriginalFunctions(Authority400) and BirthAndFullDriverMatchOriginal(Authority400,Legacy,0,0). Archived unity-shared-15cac58e/results.xml. This is selected shared-birth coverage, not all piece/state18 matrices.
- Combined currently applicable focused evidence20 PASS; no rerun of unchanged18. This turn no fullSelfCheck or Play.
- Independent fixture and production read-only reviews PASS within scope. Shared writer adds armor/alias/scales/pending initialization to old piece wrapper, not just a mechanical extraction; source spawn_at supports these defaults. Ordinary OPoint math unchanged; task reset reuse validates no semantic leakage.
- Ledger PASS622 records/113 governed changed scripts (whole worktree counts), diff--check exit0. Scene fileSHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6; live scene dirtyfalse/root14. No Scene/resources edits.

## Remaining work / exact next step
1. New fixture currently only private native immediate producer and logic-only factory. Add source following cases0/2 carefully: source captured helper then full driver, so sourcecounter1 legitimately repeats5 births. Same experiment/input/AI controls; no counter2 compensation. Preserve full supported raw/RNG/display/metadata comparisons, declare any added exact paths before scripts.
2. Verify actual renderer factory with pooled representative; new clone marker admission does not grant piece OID0. Existing source12 includes both type0/type3 and implicit frames.
3. Existing NTSD28C25DefinitionCloneEditorTests native expectationsHP10 and capacitynoRNG refer older closure39DDDA15; update only affected native oracle under predeclared exact path, preserve legacy GT11 selfchecks and successful-order/slotcursor tests. Do not label expected stale failures regression.
4. Once stable related package run relevant joint gate + once fullSelfCheck + representative Play/ordered close. No all-role rerun per edit. Keep existing fullWorld schema/raw47+3 unchanged.
5. Only after complete exit close clone package, return DISPLAY-PROGRESSION parent then existing POST-DISPLAY-RESOURCE transaction. Q06 remains incomplete/Q07 formal DAT/images not migrated.

Capacity proof only relative free dynamic boundary(native1000 vsUnity400); filler payloads excluded. Pending is private boundary, not outer caller reachability; encoded8M unresolved remains refusal control. No formal EXE gameplay recording claimed.

## Pre-edit native C25 oracle and following amendment

Exact NTSD28C25DefinitionCloneEditorTests.cs added before edit: C25b native birth four HP/MP10 expectations become500, full-capacity SynchronizedCalls unchanged becomes +34, rename that combined test to accurately distinguish missing-definition skip and full-capacity draws. Authority is measured source-final12 under current07CD47; retain missing-definition zero calls, positions/velocity/facing, actual slot traversal and all C25a/legacy tests. New fixture already declared: extend with following0/2 source driver results, preserve counter1 and source nativeAI=false control semantics, no per-tick compensation. No production change planned by this amendment.

Pre-edit declare separate State9996DirectSpawnPlayProbe.cs: existingEditor request-file harness pattern, realPlay paused snapshot boundary, isolatedWorld type0/type3 clone birth via actual Renderer factory; check scenechecksum and global poolborrowers unchanged, finally normal isolatedWorld cleanup, queue existingQ05 ordered closure. No scene save/input assets/resource modifications; only test entry, no new production lifecycle owner. Existing fixture declares VerifyRendererBirthForPlay helper.

Pre-edit selfcheck native oracle correction: full run2026-09-21 09:04:12UTC failed early at CheckNativeC25DefinitionAndCloneProductionContract line26020, still HP/MP10. Actual LateEntityUpdateAll nativecaller, distinct from legacy RunLateStateSpecialPreCollisionForSelfCheck. Exact BattleRuntimeSelfCheck.cs path now declared; update only four native500 resource constants, preserve all other statements/legacy routines. Initial failure archived selfcheck-initial-native-oracle-fail.txt. Run full again because initial invocation terminated before remaining gates, not due mere test-only edit. Following+oldC25 jobd28640d2 actual6/6PASS1.0076664s, following0/2 initial/immediate/followingdiff0,11entities+34calls; archived.

## Final scoped acceptance

VERIFIED / STATE9996_DIRECT_SPAWN_TRANSACTION. Exact13 declared scripts. Full final evidence and retained failures: artifacts/diagnostics/NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001/ACCEPTANCE.md. Independent limited exit review PASS / GO-WITH-NOTES: no major blocker; retain single-following-tick, relative capacity, syntheticparent, diagnostic-source and missingraw/encoded8M limitations. No overallQ06/Q07 completion.
