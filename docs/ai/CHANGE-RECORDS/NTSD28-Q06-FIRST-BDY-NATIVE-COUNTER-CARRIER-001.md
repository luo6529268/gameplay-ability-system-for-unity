<!-- CHANGE-RECORD
id: NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001
status: VERIFIED
change-kind: FIRST_BDY_NATIVE_COUNTER_CARRIER
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleFirstBodyResponseWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5FirstBodyResponseAtomicProductionIntegrationEditorTests.cs
authority: Current playable encoded first BDY frame.frame_counter reset maps native AttackingCounter.
evidence: Parent source8 and binder-only RED case4/5/6 native counters7/8 instead of0.
-->

# NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001

IN_PROGRESS / MEASURED_RED_EXACT_CARRIER_CORRECTION.

Authority: formal EXE B1E13A…19033/current playable closure07CD47…778F; battle_world.cpp resolve_confirmed_unarmored_hit encoded first-BDY branch writes frame.frame_counter=0 only for decoded action<999. Simple1xxx/2xxx and skipped999 preserve counters. Unity native C25 frame advancement and NTSD28UnityEntityRawCapture map this field to Runtime.AttackingCounter. Runtime.FrameWaitCounter is a different carrier, not an alias.

Measured parent source8 double-run SHA98ca9c2a5ba911dc1fbd7d414974d2dfbcaa713fcd30d64ba559bd27810a97f8/222 independent checks. Parent initial RED then binder-only jobe2c124ca1f324d198ada7b4f87704268 leaves main8 before0/immediate5/following0 and smoke4 before0/immediate3/following0:encoded cases4/5/6 retain7/8 vs expected0. Thus this is independent of frame binding and does not require new source fixtures. Preserved parent initial-red and after-binders-counter-red evidence.

Exact changes:
- BattleFirstBodyResponseWriter.ApplyResponse: two ResetTarget/AttackerFrameCounter branches write AttackingCounter=0, replacing FrameWaitCounter writes; do not write both. Preserve conditions/order and parent native binders.
- BattleEcsHitExecutionPlan.CaptureFirstBodyResponseAttemptSnapshot: two counter capture sources change to Runtime.AttackingCounter; current independent projection conditions remain, no schema change.
- NTSD28B5FirstBodyResponseAtomicProductionIntegrationEditorTests: three native counter tests use correct carrier, add distinct legacy FrameWaitCounter sentinels to prove preservation; no other expected decisions weakened.

Existing parent fixture8 tests fullsource before/immediate/following through real candidate Character pass. Reuse parent sentinel999/simple cases and existing B5 focused suite. Once both packages stable, one relevant joint run, one SelfCheck, representative replay/Play/closure. No full character product or old B5 exhaustive reruns. Prior B5 decision/RNG/group/hold/manual-stats/abort evidence remains valid; append exact correction for old wrong counter capture/observation/oracle.

Side effects: actual native frame wait progression resets as source requires and can affect next-tick advance. No framework, renderer, resources, Scene, input ownership, timeline/schema or nonbattle edits. No new runtime manager/queue/pool/lifecycle ownership. Rollback only reviewed task-owned hunks preserving parent/user changes; no destructive Git operations. Parent remains IN_PROGRESS until this dependency and its original exits pass; fullQ06/goal unfinished.

Exact3 files now edited: two writer resets toAttackingCounter; two FirstBody attempt capture sources toRuntime.AttackingCounter; original three oracle tests assert nativecounter and independent legacycounter sentinels. No pure resolver/schema/order/binder changes in this ID. Compile refresh pending then parent8/smoke4+B5 focused joint.

Joint4f8d979073d1469abbe7b6a51e6e2868 19/19PASS(1.1330306s),主8/smoke4 before/after/following全部0；CS0。完整SelfCheck2026-09-21T04:53:09.6593817Z PASS。独立review生产4点/两capture/测试哨兵通过。下一仅parent fixture新增4代表sameWorld replay与Play8/Q05close；不重复SelfCheck/旧矩阵。

Final declared scope VERIFIED; artifacts/diagnostics/NTSD28-Q06-FIRST-BDY-NATIVE-COUNTER-CARRIER-001/ACCEPTANCE.md records exact final source/joint/SelfCheck/replay/Play/closure evidence and exclusions. Previous RED retained; totalQ06 not closed.
