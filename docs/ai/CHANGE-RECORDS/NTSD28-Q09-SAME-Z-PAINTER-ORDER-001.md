<!-- CHANGE-RECORD
id: NTSD28-Q09-SAME-Z-PAINTER-ORDER-001
status: RUNTIME_PENDING
change-kind: TEST_FIRST_BATTLE_PRESENTATION_ORDER
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationBeginFrameReuseEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Formal playable render_snapshot.cpp entity_commands equal-depth physical-slot-descending painter order and d3d11_renderer.cpp ordered consumption
evidence: docs/ai/TASKS/NTSD28-Q09-SAME-Z-PAINTER-ORDER-001.md
-->

# 同 Z painter order

Pre-change: the formal interleaved entity command stream sorts greater physical slot first at equal depth. Unity Stage legacy comparator, coordinator comparator and central radix path sort lesser slot first, so equal-Z occlusion is reversed. Native independent sprite/spark source arrays are not the painter stream. Scope, risks, acceptance and rollback are in the Task Contract. Change starts with test oracle then implementation. Existing fixed-camera and content exceptions remain.

Test-first step: the independent Editor reference now expects depth ascending and equal-depth physical slot descending. Added direct LegacyOnly/CentralShadowBuild tie and central comparison-fallback cases; updated the existing original Battle Scene liveness probe's same-Z oracle. The first raw bridge `run_tests` request accidentally ignored the supplied `testFilter` field and started the entire ~8,500-case EditMode suite; it is not a focused RED result. We stopped issuing additional tests and are tracking that live job before re-running with the correct field. Its unrelated early failures must not be attributed to this change.

Production written: Stage legacy comparator and coordinator comparison fallback now use descending physical slot at equal Z. The central allocation-free radix path still sorts signed Z ascending, then reverses only the equal-Z index runs; it rejects duplicate runtime slots to the deterministic comparison fallback. No frozen physical row, DAT, Scene, logic tick or independent native sprite/spark source array is changed. Original Editor compile/focused/runtime validation pending.

Post-change: original Editor compile 0 errors; focused 14/14 PASS, fresh BattleRuntimeSelfCheck PASS. Original Battle Scene Play probe entered but did not reach its old-DAT-dependent fixture before exit; P-04 pixel and formal EXE A/B remain pending. Evidence: artifacts/diagnostics/NTSD28-Q09-SAME-Z-PAINTER-ORDER-001/ACCEPTANCE.md.

Scope amendment before editing: full SelfCheck freshly failed in CheckCompactPresentationRenderSorting at its ascending-slot oracle. Update only compact/legacy painter rank expectations in BattleRuntimeSelfCheck.cs, then rerun. The original Battle Scene R07B Play probe reached tick 272 but waited for its old pending/free hit_Fa DAT fixture; stopping Play yielded FAIL with no P-04 observation, so P-04 runtime remains pending.

Final script scope: Stage/SimulationStageRenderModule.cs comparator; Presentation/BattlePresentationShadowBuild.cs comparator and equal-Z radix index runs; Editor/BattlePresentationBeginFrameReuseEditorTests.cs store-vs-painter reference and fast/fallback cases; Editor/BattleCentralLivenessIdentityVisibilityPlayModeProbeEditor.cs same-Z oracle; Test/BattleRuntimeSelfCheck.cs compact and legacy rank oracles. The existing SPARK publication delta in BattlePresentationShadowBuild.cs predates this package and is not attributed here.

Correction to the earlier test-first description: native snapshot capture/HitRecord owner traversal keeps its ascending physical-slot reference; only the separate painter reference expects descending slot at equal Z. First focused 11/14 found this test-oracle conflation, final focused 14/14 passed. Earlier pending validation sentences record intermediate states; final status is RUNTIME_PENDING as above.
