<!-- CHANGE-RECORD
id: NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIXTURE_CORRECTION_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardReducedKnockoutProducerEditorTests.cs
authority: NTSD28-B6-LEGACY-HOLDERCOPY-RESIDUAL-RETIREMENT-PRODUCTION-001 VERIFIED explicitly retires four HolderCopy-derived damage stats calls; NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001 VERIFIED; preserve native OrdinaryCreditGate2F4/KnockoutCount358 contract.
evidence: VERIFIED_TEST_ONLY / FIRST_863_859_PASS_4_FAIL / RETRY_27_26_PASS_1_FAIL / FINAL_TARGETED_1_PASS / RECONCILED
-->

# 旧HolderCopy统计测试修正

父HolderCopy carrier清理的863测试中4FAIL（2类）仍要求旧holder.ComboCountAtk/KillStat写入或要求source包含holder.KillStat++。该production行为在Goal20已VERIFIED退休，不能为旧测试重建。BattleDamageWriter本包没有改动；HitPlan本包仅删除HolderCopy诊断字段及bit33，统计消费主体未变。此处不声称曾实跑当前修改前的两类全测试。

准确两测试文件，仅标准/简化/cpoint旧holder额外统计预期10/1改0、方法名称更新为不计旧holder统计并同步同文件runner；damage gate source计数5→3（此前两旧stats gate删除），其他HitPlan gate4/1及真实KO生产断言保持。旧要求holder.KillStat++ sourceguard改不存在，真实KnockoutCount358和owner链/拒绝矩阵保持。

先记录父失败和此preimages，再改；只复验两类，保留首次859/4并按实际case列表核对关闭4失败，不重复整批863。生产/资源/Scene/框架/非战斗/版本保持，禁止computer-use。测试only VERIFIED不代表父Q05全对齐。回滚须批准，只本两文件精确差量，保留前包HolderCopy载体迁移。

首次两类复验27=26PASS/1FAIL：Cpoint helper参数虽旧名expectedHolderKills，实际早已断言attacker.KnockoutCount358，而非holder.KillStat。纠正本次误改，保留Cpoint gate -1时nativeKO=1，改名expectedKnockouts和组合测试名避免再误读。只standard/reduced旧holder stats为0；cpoint原native score/KO/holder无写断言全部保持，production不改。首次复验失败保留，最窄失败用例复验后核对27类结果与原863关闭映射。

限定出口：两类27例已由26PASS+最窄1PASS闭合，父原四FAIL逐项映射至通过证据；原失败均保留。实际命令/结果/Cpoint误名纠正边界见artifact REPORT。没有production/资源/框架改动，父SelfCheck/Play仍独立。
