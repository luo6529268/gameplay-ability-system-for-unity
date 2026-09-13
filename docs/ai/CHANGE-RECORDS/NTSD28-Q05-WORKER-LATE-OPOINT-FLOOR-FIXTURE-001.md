<!-- CHANGE-RECORD
id: NTSD28-Q05-WORKER-LATE-OPOINT-FLOOR-FIXTURE-001
status: VERIFIED
change-kind: TEST_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
authority: Current formal playable physics_integrator.cpp type0 contact clamp; BattleLogicObjectPointRuntime.ConfigureLateOpointPosition.
evidence: related-first-174-PASS-1-FAIL.xml; prechange.json
-->

# Worker late OPoint floor夹具修正

状态 IN_PROGRESS / TEST_ONLY。边界guard相关首次175项中174PASS/1FAIL，旧worker fulltick fixture在type0 parent Y40、floor0情况下仍期待child Y45。正式physics_integrator.cpp接触分支把type0 precise_y置effective_floor_y；当前BattleLogicObjectPointRuntime.ConfigureLateOpointPosition用当前parentY-centery20+opointY25，因此child应5。保留原parent起点40，增加parent落地0断言并改child5；不改生产运动/出生逻辑。

准确一个测试脚本，符号DedicatedWorkerFullTickMaterializesLateOpointWithoutUnityFactory。风险是误把行为变化当旧预期，故已对照正式playable physics和实际出生consumer、保留原FAIL。验收该测试及worker相关回归、SelfCheck；rollback须批准按preimage差量恢复，保持其余用户/既有修改。不改资源/Scene/版本/非战斗，禁止computer-use。

## 验证出口

VERIFIED_TEST_ONLY：最终guard相关187/187含本类/该精确用例通过，完整SelfCheck PASS。原175中的1FAIL保留于guard artifact；只增加parent0与child5两断言变动，未更改生产运动或spawn。
