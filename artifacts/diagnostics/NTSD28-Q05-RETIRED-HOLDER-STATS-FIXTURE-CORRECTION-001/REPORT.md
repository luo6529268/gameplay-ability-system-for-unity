# 两份旧Holder统计测试修正

VERIFIED_TEST_ONLY。准确两测试文件，无production修改。父HolderCopy carrier包首次863为859PASS/4FAIL，四失败涉及标准/简化伤害旧holder额外stats、旧source5个gate和要求holder.KillStat++仍存在。Goal20 VERIFIED的HolderCopy residual退休已删除这些写入，当前BattleDamageWriter未被本包改动。

标准/简化伤害的旧holder.ComboCountAtk/KillStat预期改0；source gate5→3（其余HitPlan4/1保持）；Knockout测试要求legacy holder写入改为不存在，所有真实native KO/score/owner链/拒绝矩阵保持。

首次两类复验27=26PASS/1FAIL。该1FAIL发现Cpoint helper旧参数expectedHolderKills实际已检查attacker.KnockoutCount358，本次误改为0；修正回原native gate -1→KO1、gate0→KO0，参数更名expectedKnockouts并更正组合测试名称，原attacker score10/holder stats0检查保留。最后仅该1case复验PASS，不修改production让测试迁就。

实际job：0973d964fde14077896c7b1dda8d428d（27=26/1），8a46cf58cfde409b817d78d19af0e63e（1PASS）；XML在first-retry-FAIL.xml/final-targeted.xml。父focused-reconciliation.json逐项映射原4FAIL至最终新/同名通过用例。863不同用例均有通过证据，不能声称新鲜单次863全绿；未重复整批。

用户禁止computer-use，全部使用现有Editor桥接/日志/结果。父SelfCheck/Play单独闭合；此记录不代表Q05/总目标完成。回滚需批准，仅两测试精确差量，保留父HolderCopy清理。496条记录及累计45脚本范围由父最终validator核验。
