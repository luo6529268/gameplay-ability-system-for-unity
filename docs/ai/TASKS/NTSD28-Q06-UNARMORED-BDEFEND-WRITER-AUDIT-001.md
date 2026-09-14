> FIELD_FAMILY_MAP_RECORDED，原256见证已验证。必须按整个Bdefend字段家族处理：非角色首type0不写45，signed reduced add不能clamp；C25h现有owner正确。下一BDEFEND-FIELD-FAMILY-UNITY-001，完整边界见新source REPORT；不是只补unarmored单行。

# 无护甲命中 Bdefend 字段归属

READY_SOURCE_WRITER_AND_FIELD_MAP。当前collision qualification完整driver两例×四组的独立首差：target slot2 combat.bdefendAccumulator=0，原45；HP499、抓取动作10/998、tick snapshot0及其余raw一致。原输入/结果在COLLISION-QUALIFICATION-SOURCE-WITNESS-001/first.jsonl末两行，Unity在COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001/after-final-gate。

已确认不是DAT默认值：combat_records.cpp.value_or_zero/interaction缺失bdefend读0；battle_world.cpp约6743原无护甲/首armor type0流程直接 `target->bdefend_accumulator = 45`。不要把夹具DAT补成bdefend45或改变converter默认值以消除差异。

Unity raw capture映射为Runtime.Bdefend。BattleDamageWriter无护甲相关路径仍写victim.HitStateCount或HitCounters.SetHitStateCount(45)；LF2HitCountersModule分别绑定Bdefend与HitStateCount，不能按名称认为它们同一个源字段。HitPlan多个Project分支也只改TargetHitStateCount，因此Shadow mask0未证明原值正确。

先追Native bdefend全部写入/读取/自然恢复与当前命中分支、Unity两个字段真实消费者及copy/checksum，再准确Task子Record：actual/plan字段一起闭合，覆盖两factory和后续tick恢复。不能全局删除/重命名HitStateCount或把所有45改成Bdefend；defended/armor/weapon分支须按原各自合同区分。无新字段/schema/框架改造，无资源/Scene/非战斗/Server修改，禁止computer-use。

本项优先于HIT-SPARK-TRANSACTION-AUDIT-001；两项都验证后回当前qualification完整driver失败，父collision不得提前标VERIFIED。
