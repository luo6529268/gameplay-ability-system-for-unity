<!-- CHANGE-RECORD
id: NTSD-OPT-H07-HARNESS-RECOVERY-026
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: user explicit continue H07 beyond repair-count limit 2026-10-07; formal336 and simulation unchanged
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH26-H07-CONTINUE-20261007/REPORT.md
-->
# 第26批 H07 诊断生命周期与统计闭包恢复

最新终态2026-10-07T05:39:43+08:00：Suite PARTIAL / DONE，3个窗口结束，后三个formal未运行。首个Dispersed正式120+1800、534次活动AI观察min1000，但内存边界collection=[1,1,1]使ZeroGcGateFailed/harnessValidity=false，完整证书不成立。窄tick自身collections0/容量critical delta0；不能仅凭boundary全局collection归因选定表现代码，也不放宽或删除原失败。实际logic平均459.8893/P95696.6069ms，CandidateCollect平均426.5499ms/92.7505%，只是brute诊断，非性能达标。teardown.restored=true且最终十一阶段关闭objects/slots/borrowers0、Scene SHA同、原Menu clean/非Play/idle；StoppedWithResidue状态名不证明本例有活跃清理残留。Record继续RUNTIME_PENDING，Goal active；下一可靠采样Task27尚未改C#/执行，不再声称当前Suite活跃，不机械重复全部原失败窗。最终原件见final-suite-checkpoint.json和windows-01/suite-result.json。

后继事实：Combat smoke也完成120+180，actualAI/baseRoster min1000、harness/workloadValid=true、StoppedCleanly/restored；logic mean431.0003/P95691.91995ms未达预算。Dispersed formal-01已推进38warm，2/6终态，Suite仍运行，不扩/缩矩阵或重启。共享Temp终态按事前授权写PASS本批路径，原副本同；原178-path保护manifest包含该输出，raw唯一差异保留并明确分类，非写域177项无其它变化，详classification原件。新代码/source范围未增加，最终runtime门未闭合。

最新实测：首个Dispersed smoke120+180真实activeAI1000窗口valid=true/StoppedCleanly、cleanup零残留；build300/refresh600000/read300000符合修正前置。Combat已实际推进，旧quiesced入口拒单未复现。logic mean467.5241/P95706.75826ms，CandidateCollect mean429.1872ms（91.8%），仍未达性能预算；原brute模式不改，0GC UNKNOWN保留。1/6窗口完成，Record仍RUNTIME_PENDING，不升VERIFIED，整个Suite关闭/场景恢复待后续。

原Editor已实际进入Battle Play，六request在新windows-01创建；Dispersed smoke已从预热12推进112/120，暂无terminal。后续只观察当前live run，不重启、不编辑脚本/refresh打断，不将初始report.json Starting快照与live progress混淆。本Record RUNTIME_PENDING，87/87聚焦事实保持；H07性能未完。

GREEN-02：原Unity 2022.3.62f3 Editor job c8f10c9591b94e47aa97cacff008e30c完整终态succeeded，87/87 Passed、0failed/0skipped，实际duration2.5966177s；green-02-focused-tests.json保存全部87具名结果。新Preparing正反、refresh正反、旧capacity与authority/rollback/Shadow门均通过。原Menu非Play/idle，无编译或测试；ChangeLedger1322记录/8代码路径PASS、0errors（4277历史warnings），diff-check exit0。178保护/10备份SHA同，HEAD8107196b保持。下一仅冻结六窗，仍未有新性能达标或0GC证据。

GREEN-01实际：ba55216cfdd94abcbe2e57022b1955ac完成87项，progress仅一失败：双consumer Shadow JSON断言硬编码118000仍属旧59pass口径，report已有120000。原件green-01-focused-tests.json（长失败message仅前1600字符，明确截断）；只补为120000，业务断言及负例不放宽，同批准确路径不扩展，随后复验相同固定矩阵。桥接终态result为null，9330仍仅discovered，不据此称完整NUnit结果已落盘。

Test-first追加：Suite内新4 preparing生命周期与4双refresh正反case，复用既有owner fixture和合成authority report；生产修复尚未写，先运行这8项确认RED，不读Q06排序body。已有10副本/178保护与terminal副本已保存并hash一致。原Editor已核对Menu idle、非Play、无编译/测试。

RED实际：cd633246110940c7b251942e8a9260f0 执行8项，6预期失败/2通过（9330仅discovered）；原件red-focused-tests.json。4准备helper未实现、双refresh正例被拒、旧单refresh错误被接受，生产未修改。首次发送测试时domainreload6401短断未创建作业，恢复原Editor后仅上述实际作业，不重启Editor。

同一统计闭包重扫：当前World RunCharacterInputPasses只跳tick<=0，Driver已完成第一步为tick1；旧gate仍假设第一步跳过。故统一快照expected pass应等于成功logicTicksExecuted，而不是减1（旧smoke build300/read300000是具体反例）。准确路径不扩展，只同步原helper/golden/rollback合成字段，聚焦补入受影响UnifiedAiSnapshotShadow门；不修改AI模拟或输入规则，不按实际build计数反推期望，防止循环验证。

脚本前 PLANNED；用户明确次数到限仍继续，诊断缺陷修复不等于性能达标。Task冻结三个准确代码路径、六request、测试、7200s观察截止与旧GC证据限制。第24批失败原件保留；累计H07修复3轮，本批将记第4轮，不归零。

现状：Configure新World未Prepare服务，RecreateWorld正常Shutdown会留下pool quiesced/拒单，下一prepare因此失败；AI producer与input-tail各refresh一次，已有实际单实体测试2/1，诊断terminal gate仍错误期待refresh/read同数。只修diagnostic，使证据回到当前调用链，不改规则/默认/权威。

副作用：已有Editor压力runner/缓存/报告成本；合法Preparing重新接单，不绕过Stopping，不变十一阶段顺序，无新增模块。完整热路径0GC尚待可靠采样，不以旧API0B认证。关闭仍已有runner cleanup及Suite最终有序Shutdown/Scene保护。

验证尚未运行；风险为错误放宽统计gate、准备时owner捕获、长窗测量干扰，正负聚焦与原固定真实窗检验。备份/恢复来源见Operation before；任何精确恢复先授权，无reset/checkout/clean/删除。
