<!-- CHANGE-RECORD
id: NTSD28-Q07-HIT-PROJECTION-RESOURCE-MOTION-SYNC-001
status: FOCUSED_TEST_PASS
change-kind: HIT_SHADOW_PROJECTION_RESOURCE_AND_HALF_DVX_SYNC
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: formal root NTSD2.8-Logan.exe; paired battle_world.cpp hit resource transaction and hit_response.cpp reduced horizontal response
evidence: after two production fixes original Editor reduced ShadowCompare existing cases fail 2/2 with writer mask 0x20000000000000; projection omits resource settlement and integer-divides odd ground dvx
-->

# NTSD28-Q07-HIT-PROJECTION-RESOURCE-MOTION-SYNC-001

Pre-change: `ProjectStandardCharacterDamageWriterEffect` and `ProjectAlternateCharacterDamageWriterEffect` subtract HP and credit but do not project the shared type-0 resource transfer now present in production. The alternate projection uses `int halfDvx = resolvedItr.dvx / 2` while the production writer/formal rule use floating half. The stored `WriterEffectSnapshot` already carries resource owner handle/type/current PP/max/consumed and target PP/consumed. This package will mirror the transaction on these existing carriers, not alter gameplay rules or DAT values. The existing reduced ShadowCompare test's two failing cases constitute the initial RED; an odd-`dvx` case must be added before implementation.

No nonbattle changes. Expected side effect is correct ShadowCompare evidence and any decision that relies on its valid/invalid flag; production raw state should be unchanged. Rollback is exact-hunk after provenance review. Acceptance and broader open Q07 gates are in the Task Contract.

Actual change: `BattleEcsHitExecutionPlan` now projects the existing shared type-0 resource transaction after standard/reduced HP credit using its already-captured resource owner/target PP, consumed counters, active mode rules and owner-target alias; the reduced ground half-`dvx` projection uses `double` `/ 2.0`. A new opt-in Editor test exercises ordinary-defense reduced hit with odd `dvx=-5`, owner/target current PP below max, and asserts the ShadowCompare field difference mask and plan validity are zero. No production writer, DAT, Scene, image, config or nonbattle source was changed by this package.

Test sequence: after the production fixes, original Editor existing reduced ShadowCompare two cases failed 2/2 with writer mask `0x20000000000000` (job `eb4d108e6e0c4cbdacefa5dead4c74ab`). A temporary attempt to extend the existing method's signature caused CS7036 in a different existing test caller; it was repaired to compile and produced RED 3/3, with the odd case mask `0x20200000000000` (job `6155fc68c3894b889472ea1a85df4c40`). The method was then returned to its old signature and a new focused test added. After projection code, original Editor compilation had zero errors. New test job `06a8d15c637c49529663935f073df0cc` passed 1/1, including `CurrentTickPlanValid=true`, writer mask0 and pending-X -2.4. A neighboring 21-case run `7843cb4886ea46c684e0694dacfdd7d8` completed with all 21 tests failing on later historical value assertions, but each passed its earlier `CurrentTickPlanValid=true` and writer-mask-zero assertions; this whole run is **not** green. The expected historical HitStateCount and bdefend values were not edited without source-backed evidence. Detailed record: `artifacts/diagnostics/NTSD28-Q07-SASUKE-ARMOR-TARGET-FIRST-DIFF-001/PROJECTION-SYNC-ACCEPTANCE-20260926.md`.

Result: `FOCUSED_TEST_PASS / SCOPED_SHADOW_FIELD_PARITY`, with the adjacent 21-case full-test failure explicitly open. Do not infer complete Q07, natural Play or formal EXE GPU parity from this diagnostic projection.
