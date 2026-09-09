# NTSD28-UNIFIED-AI-SNAPSHOT-REFRESH-EXPECTATION-FIXTURE-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-UNIFIED-AI-SNAPSHOT-REFRESH-EXPECTATION-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_ONLY_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
authority: 用户2026-09-09 Goal6 PartA明确授权及GLM已确认的双阶段刷新语义；当前NTSD2.8-Logan battle authority不变。
evidence: Single test job389cbc5b1f2a4730a0690d261619c395 PASS1/1; one groupNames ProductionEntityStress jobd484fc9db61a4f8d8ffb66a62b0f5f3d PASS280/280 (main256 + timing17 + capacity7). Runtime22/Editor104 warnings, both0 errors. Only assertion1-to-2 and one comment changed; remaining bytes preserved. Scope-only VERIFIED.
-->

完整事前合同：[Task](../TASKS/NTSD28-UNIFIED-AI-SNAPSHOT-REFRESH-EXPECTATION-FIXTURE-CORRECTION-001.md)。
原状：具名测试RefreshCount期望1，与已证实的producer后及输入路由尾两次刷新冲突。
改后职责：仅断言期望2并说明两阶段语义；原所有其他断言和dump保留。
来源：Temp/Goal5_UnifiedAiSnapshot_Counters.json与NTSD28-UNIFIED-AI-SNAPSHOT-STRESS-COUNTER-EVIDENCE-001。
源码证据：SimulationAiDecisionModule.cs:1699/1813互斥递增点，SimulationWorld.cs:2756/2936/2979三个wrapper入口。
实际脚本范围、副作用、不变量、验收和回滚均按Task；只登记本轮两行增量，不认领整个文件历史。
实际修改：仅该具名测试RefreshCount的Is.EqualTo(1)改2并加一行producer后/输入路由尾注释；字节逆替换等于修改前全文件，其他字节未改。Part B不写文件，结论不归本包。

## 最终验证

- 原SHA：E40B5799A78BEACF975D8ED37040C1D8EFCF5EA67DC89B25334B09EDDD117F00；改后SHA：4278221ED4EC1401882BBC3FE3E5871B42BC6C9C80994D9605F8E36E91F21600。
- 指定instance gameplay-ability-system-for-unity@b1b02287，版本2022.3.62f3，NTSD_Battle，Scene dirty=false。脚本reload期间未发送run_tests；ready后仅启动一次single与一次group，不进入Play、不启动第二实例。
- testNames完整名NTSD.Animation.Rendering.Editor.ProductionEntityStressEditorTests.UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport，job389cbc5b1f2a4730a0690d261619c395，1 total/1 passed/0 failed/0 skipped。
- 随后groupNames=[ProductionEntityStress]，jobd484fc9db61a4f8d8ffb66a62b0f5f3d，280 total/280 passed/0 failed/0 skipped，6.7602892s。明细main256、DetailTiming17、CapacityPressure7全部通过，不能误报为仅256项。查询结果未重跑测试。
- Temp/Goal6_UnifiedAiSnapshot_Started.json、Goal6_UnifiedAiSnapshot_Result.json、Goal6_ProductionEntityStress_Started.json、Goal6_ProductionEntityStress_Result.json、Goal6_ProductionEntityStress_Details.json保存本次实际响应。
- Goal5原计数文件在运行前逐字节备份为Temp/Goal6_Goal5Counters_Baseline.json；原dump代码保留，获批single/group运行仍写原Temp路径。
- dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，22 warnings/0 errors，26.89s。
- dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，104 warnings/0 errors，21.71s。
- Scene SHA基线：D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11；最终hash和validator另附。
- 无新runtime所有者或shutdown阶段，无production修改。本包不代表full SelfCheck或完整战斗对齐，停止等待Goal7。

当前状态：VERIFIED / TEST_FIXTURE_ONLY / SINGLE_1_OF_1 / STRESS_280_OF_280 / CORE_STRESS_256_OF_256 / BUILDS_0_ERROR / SCENE_UNCHANGED / GOAL7_USER_HOLD

最终审计：Tools/Validate-ChangeLedger.ps1实际PASS，435 Records / 374 governed code files / 531 warnings，exit0；日志Temp/Goal6_ChangeLedger_Validation.log。Scene最终SHA与上述指定基线完全一致，get_active再次确认dirty=false/root13。
与本轮开始前逐路径SHA基线比较，Temp之外仅6个授权文件变化：唯一测试脚本、本Task/Record、Ledger、STATE、对齐总表；BattleRuntimeSelfCheck.cs及所有production字节未变。
