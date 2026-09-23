<!-- CHANGE-RECORD
id: NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001
status: FOCUSED_TEST_PASS
change-kind: BATTLE_DAT_KIND_CONSUMERS
code-path: Assets/NTSD/Scripts/Animation/LoganKindCatalogInput.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeDataCatalog.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3KindCatalogTransformEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: formal NTSD 2.8-Logan playable hit_candidates.cpp and battle_world.cpp active KindCatalog28; D-023 kind.dat
evidence: original Editor RED job 784fac29d1bf4c75aa6def869f99b541, 3 target failures and 6 existing passes; final focused jobs 677ea6b88bd64e7a8e908efcde27b623 10/10 and 83767199f64545ee90229de2169c699d 2/2 PASS
-->

# NTSD28-Q07-KIND-DAT-COMBAT-CONSUMERS-001

Before: selected kind table is V3-identified and prepared, but collision candidate and type3 transform (including ECS projection) still use old one-record literals. See the Task and `artifacts/diagnostics/NTSD28-Q07-KIND-DAT-READER-AND-IDENTITY-CONTRACT-AUDIT-001/REPORT.md` for exact native call order and current first difference.

After target: immutable World-or-locked-fallback table shared by candidate and transform; no literal OID/frame decision remains in these two consumers. Preserve actual source ordering, geometry pass, field writes and ECS projected state.

Expected side effects: modified formal kind DAT can change candidate acceptance and type3 transform/frame, while the staged formal one-record file keeps current locked-table behavior except the required target-type3 gate. Empty valid selected catalog means no table-specific reject/transform; no fallback silently replaces an explicitly selected valid empty table. Existing nonbattle paths and saved Scenes are unchanged.

Validation plan: original Editor compile; focused RED/PASS tests on altered kind tables, type0/3 target difference, kind9, first record and zero frame; existing B5 type3 tests; targeted Battle Scene smoke where feasible; `Tools/Validate-ChangeLedger.ps1` and `git diff --check`. Report source/runtime limits separately. Rollback as in Task; no cleanup, restore or deletion authorized.

RED: original Editor recompiled the declared B5 test file and ran job `784fac29d1bf4c75aa6def869f99b541`. Six existing cases passed; three new/changed cases failed on the current production code: candidate method has three parameters rather than target-type+catalog five; selected modified catalog does not cause the expected transform. This is the expected pre-change first difference, not a production regression conclusion. No production script was edited before this RED.

Implemented: `LoganKindCatalogInput` retains one parsed locked fallback; `BattleRuntimeDataCatalog.BattleKindTableRules` resolves prepared World catalog or that fallback and applies native candidate/transform record order. `BruteForceSceneQuery` now supplies target type and catalog to the candidate gate. `BattleDamageWriter` selects the bound/respond record and its frame (zero becomes 40), transfers definition/owner/team/ID and records raw action even when the destination frame has no data; `BattleEcsHitExecutionPlan` projects the same selected action. The two declared Editor test files cover selected/empty/fallback tables, first record, kind9, type0/type3 gate, raw missing-frame action, and ShadowCompare frame40/frame33. No Scene, Prefab, ProjectSettings, asset or nonbattle code changed in this package.

Validation: original project Editor compiled after each final source edit. B5 focused job `677ea6b88bd64e7a8e908efcde27b623` succeeded 10/10; ECS exact-name job `83767199f64545ee90229de2169c699d` succeeded 2/2. A preliminary missing-frame test using ID 241 failed its own precondition because native zero-initialized frames cover 0..998; corrected ID 999 produced a genuine RED at job `a0d4cd5458f54704bea928eaecf98d56` (target OID remained 201), then passed in final 10/10. A wrong-namespace ECS invocation `acdd30eb9a954939943572c741b17d17` ran 0 tests and is not counted. `Tools/Validate-ChangeLedger.ps1` passed (698 Records, 57 governed changed code files), `git diff --check` exit 0; Menu/Battle saved Scene SHA-256 remained `6CF124A17F692325CED5774C3C10C3324AA047439BC6C3EB08856188A78AB4B1` / `9E7B8A91ADD396D8A3674915BB5AC9A03B8D1A2EC817BA3B12F135D03EBA0AC0`. No real Scene kind-dependent pair or formal native same-seed trace has been run for this package; default formal catalog lacks OID209, so these remain Q07/R15 gates. Status is scoped focused-test pass, not complete battle parity.
