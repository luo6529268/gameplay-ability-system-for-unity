# NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001

<!-- CHANGE-RECORD
id: NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_FILTER
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5CandidateEffectTypeProductionFilterEditorTests.cs
authority: NTSD 2.8-Logan hit_candidates.cpp candidate filter and documented consumer second call; EXE B1E13AE1, closure 39DDDA15.
evidence: valid red e8767d08a7074e5c9d81b381d5bd5e75 failed 6/15 exactly on five rejected matrix cases plus missing runner gate; focused 70bdf534fd6341e5b43c298aaabedbb0 15/15, HitPlan d1149a8995794c27b023d375ae870953 184/184, B5 3f3a0c18eef64e6487035f1804ac0f49 461/461, broad de83a62ac2aa4bafbe878785fae554b4 926/926, 05:13:45Z SelfCheck PASS, Console expected7, Scene D4266C6D unchanged, Ledger 309/270 PASS.
-->

> 状态：`VERIFIED / CANDIDATE_AND_CONSUMER_FILTER_ALIGNED`

只接candidate/shared defensive filter，不改变effect consumer副作用。候选入口在 `ItrAllowedCore` 统一过滤；shared runner在runtime ITR替换完成后、disposition和writer之前再次过滤。回滚为移除两处resolver调用与本包测试。

实现与权威顺序闭合：candidate几何前共享过滤，并在runtime ITR替换后、任何disposition/writer之前二次过滤。
