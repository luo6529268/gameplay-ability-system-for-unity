# Task Contract — NTSD28-B5-NONCHAR-DAMAGE-SCALE-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NONCHAR_MIGRATION_SEAM_DEFINED`

## 目标

只读判断Unity `FallDamageDiv`是否可作为2.8 `IncomingDamageScale340`的别名，并闭合weapon/type3
standard unarmored damage的迁移边界。

## Authority 结论

- Entity只有`incoming_damage_scale_340`作为实际伤害除数；正值执行`damage*100/value`。
- `mode_damage_scale_percent`默认100，只在definition/fusion切换时与positive `stats.defend`合成为
  `+340`；当前playable live path未发现把结果页倍率另写为独立伤害除数的调用。
- 普通playable combatant创建不会凭结果页状态生成另一除数；LFR只直接恢复已录制的`+340`。
- unarmored standard hit对type0..5共享`+340 -> attack-effect source weak/2`；type6跳过damage block。
- raw ITR injury仍独立供display step、weapon durability/native attacking injury等消费者使用。

## Unity 结论

- `BattleResultsWriter.ApplyFallDamage`把结果表倍率写入`FallDamageDiv`；该字段同时被weapon/type3、
  alternate/kind16、cpoint、environment/recovery等旧路径消费。
- `FallDamageDiv`与`IncomingDamageScale340`同时存在于runtime/snapshot/checksum，字段来源和生命周期
  不同，不能继续称为同一2.8字段或双重叠加。
- type0 standard已只读`IncomingDamageScale340`；weapon normal与type3 normal仍读`FallDamageDiv`且不读
  attacker weakness，是当前可独立修复首差。

## 实施拆分

1. `NTSD28-B5-NONCHAR-UNARMORED-DAMAGE-SCALE-CONSUMER-001`：weapon/type3 normal只消费
   `IncomingDamageScale340 -> attacker WeakTimer12C/2`；durability/display/status顺序保持。
2. B4/B6/B8分别审计environment/recovery、cpoint、ResultsWriter及剩余alternate/kind16 consumers；
   在各owner迁移前不删除`FallDamageDiv` carrier。
3. H/B11补`stats.defend`producer，B7补child inheritance；当前不伪造内容值。

## 修改边界与验证

本包只修改治理文档；不改C#、Config、Scene、Prefab、Authority或运行行为。Authority live writer/readers、
Unity全部`FallDamageDiv/+340`读写者与下一迁移seam已静态闭合。
