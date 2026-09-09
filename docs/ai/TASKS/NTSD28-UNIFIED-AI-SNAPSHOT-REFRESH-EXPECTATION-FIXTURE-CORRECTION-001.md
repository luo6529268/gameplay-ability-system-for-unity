# NTSD28-UNIFIED-AI-SNAPSHOT-REFRESH-EXPECTATION-FIXTURE-CORRECTION-001 — Task Contract

> 2026-09-09 Goal6 Part A / VERIFIED / TEST_ONLY；本合同修改前建立，实际证据见同ID Record。

需求：用户明确授权仅修正具名测试RefreshCount期望，并完成一次单测与一次完整ProductionEntityStress压力组复验。
GLM已独立复核并由用户要求直接采用：Goal5六计数1/1/1/2/1/1050，world==report，无roll-forward/失败/重复构建。
证据文件Temp/Goal5_UnifiedAiSnapshot_Counters.json；历史取证Record为NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001。
SimulationAiDecisionModule.RefreshUnifiedExecutionRowAfterCharacterInput的1699 skip与1813正常递增互斥，每次进入只+1。
SimulationWorld wrapper入口2756、2936、2979（AfterProducer）；本夹具slot0一次CharacterInputAll合法经过producer后和输入路由尾两次刷新。

## 精确范围与不变量

唯一脚本：Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs。
唯一符号：UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport。
仅RefreshCount断言Is.EqualTo(1)改为Is.EqualTo(2)，前加一行两阶段语义注释。
其他断言、Goal5 dump、fixture、cleanup及production所有字节保留。
附属文件仅本Task/Record、docs/ai/CHANGE-LEDGER.md、docs/ai/STATE.md、Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md与Temp产物。
无runtime模块/计数/World/RNG/持久数据修改，无生命周期或有序关闭副作用。
当前NTSD2.8-Logan正式Authority不变。Part B独立零文件改动，只读结论仅在最终报告，不并入本Record。

## 验收、风险与回滚

指定instance gameplay-ability-system-for-unity@b1b02287，2022.3.62f3 / NTSD_Battle；不进Play，不启动第二实例。
先testNames具名单测一次，必须PASS；随后groupNames ProductionEntityStress完整压力组一次，记录总数。
若单测仍失败或压力组出现新无关失败，立即停止，不修不重跑；其他硬停止依用户Goal6原文。
两套Assembly dotnet build均0 error、Tools/Validate-ChangeLedger.ps1通过，Scene SHA保持
D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11。
最终VERIFIED仅关闭该期望修正与stress复验；无Play或整个战斗对齐结论。
风险：压力组可能揭示其他失败；只报告。既有dump在本次获批运行中写原Temp路径，运行前在Temp保存Goal5证据副本。
回滚需要用户明确批准；仅撤销本包断言/注释和治理增量，保留全部既有工作。无不可逆运行行为或外部部署。
完成后GOAL7_USER_HOLD，不继续其他任务。

验收结果：具名单测一次1/1，完整group一次280/280（main256/timing17/capacity7），两套build均0 error。VERIFIED仅关闭本夹具期望与stress复验。
