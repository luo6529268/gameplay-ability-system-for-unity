<!-- CHANGE-RECORD
id: NTSD28-Q06-C25-POISON-PHASE-FIXTURE-001
status: VERIFIED
change-kind: TEST_FIXTURE_CONTRACT_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FHJTimerOwnerEditorTests.cs
authority: Q05 validated joint schema13/21/24 and Q06 native World-phase HP transaction; existing C25 isolated test responsibilities.
evidence: joint regression5a7a3ed17dce44c49a0d3dece7815fd0 observed old fixture failures; correction not yet written
-->

# C25旧夹具合同修正

Poison专项夹具补非HP恢复phase12=1/phase3=1；原毒伤扣除/计时/统计和最终HP断言不变，仅C25h_PoisonUsesPostDecrementCadenceAndNativeDamageRule。新World默认phase0，Q06 HP已由host tick改World phase，旧测试让恢复+1混入毒伤验证，导致三例193/150/150实际194/151/151。不得改全局CreateCharacter或生产恢复。

准确一测试文件；无生产副作用/manager/schema改动。先保留原失败XML，原job结束后修正，运行当前类focused及父SelfCheck/Play，账本validator。回滚须批准仅本测试差量，不恢复旧生产合同。父DISPLAY-PROGRESSION仍IN_PROGRESS，禁止computer-use/非战斗/Scene/资源改动。

已在原job结束后按声明修改该单一测试方法，原181项结果177PASS/4FAIL已归档。生产四脚本保持；当前编译/定向复验待。

最终TEST_ONLY结果：job169f777ca0a64f1ca6497c38ccb90e76定向6/6 PASS（5 poison +1 schema），完整SelfCheck新鲜PASS。原181中的4失败逐项闭合，原assert语义保持，父生产没有为这些失败改动。focused-final.xml与父SelfCheck-pass.result为证据。

账本最终PASS：514 records/17 governed code diff；日志在父DISPLAY-PROGRESSION artifact/ledger-final.txt。历史Record不在当前diff的警告保留，不是本次失败。
