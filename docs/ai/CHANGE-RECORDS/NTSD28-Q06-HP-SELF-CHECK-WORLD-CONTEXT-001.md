<!-- CHANGE-RECORD
id: NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001
status: VERIFIED
change-kind: TEST_FIXTURE_WORLD_CONTEXT
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Q06 World-owned resourcePhase12 contract and actual HP source witness; existing SelfCheck current-DAT dispatch purpose.
evidence: parent HP134 PASS; fresh SelfCheck GT-06 failure with deliberately unregistered CreateCharacter fixtures
-->

# GT-06自检补真实World阶段上下文

IN_PROGRESS / TEST_ONLY。仅BattleRuntimeSelfCheck.CheckGameTickCurrentDatDispatchMatrix中的GT-06块。CreateCharacter全局测试工具明确不注册World；原GT-06直接传tick12调用资源入口，HP改为正式World phase后无World即无phase，旧夹具因此不再满足入口条件。不得在生产中恢复host tick fallback，也不得改全局CreateCharacter造成所有测试额外注册。

在GT-06范围内建立私有SimulationWorld(runtimeCharacterConfigs)，注册realCharacter/sharedCharacter/reverseType3三者，显式phase12=0/phase3=1。保持原ApplyRecoveryFixture、HP101/MP0及非角色不恢复的所有断言；reverseType3同样获得有效World，确保不被“缺World”假阳性遮蔽类型测试。finally逐一注销并按既有logic shutdown清空私有World，不触碰Scene、全局Driver或正式pool。无新生产生命周期、字段或schema。

回滚须批准仅GT-06块差量。验证完整SelfCheck、HP/MP相关focused及账本；原失败归档。本修正不能宣称整个HP事务或Q06已完成，父真实Play和范围证据继续。

## 实际改动

已仅在GT-06块注册三个夹具到私有World，phase12=0/phase3=1；finally注销并通过既有logic shutdown清理。原恢复和反向类型断言保持。编译CS0；正在运行新鲜完整SelfCheck，结果待回写。

## 最终限定出口（2026-09-14）

VERIFIED / TEST_ONLY：原GT-06断言保持，私有World上下文修正后完整SelfCheck PASS；父134回归和实际HP Play/有序关闭全0通过。

详细证据和命令见 artifacts/diagnostics/NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001/REPORT.md。用户HUDBg x30/Scene bcd1047b…保留，保护清单无新增缺失；父Q06、Q08模式投影及正式资源迁移仍未完成。初始失败和中间状态为历史，不覆盖本出口。

账本最终验证PASS（510 records/10 governed code diff），详见父artifact/ledger-final.txt；历史Record非当前diff警告未清理。
