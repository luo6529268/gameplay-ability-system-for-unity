# Task Contract — NTSD28-B5-REMAINING-FALLDAMAGEDIV-CONSUMER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / REMAINING_OWNERS_ROUTED`

## 目标与结论

在type0..5 standard damage迁移后，审计全部剩余`FallDamageDiv`读写者并路由到真实阶段。

- `ApplyAlternateDamage`及hit-plan projection属于旧guard/alternate hurt分支；需随selected armor/defense
  重建，归B5 armor + H/B11，不能只换除数字段。
- Unity `ApplyKind16`把`itr.kind=16`解释为角色伤害/扣蓝帧；当前2.8 world relation consumer没有
  kind16分支，只有kind15 separation。源码中的`case 16`属于`effect=16`目标类型矩阵，不是itr.kind。
  该路由是独立可移除首差。
- `BattleCpointWriter`的held damage归B6 catch/held，权威使用caught `+340`。
- frozen landing、landing weapon-count、negative weapon-count recovery及其legacy duplicate归B4/B7
  lifecycle清理；2.8对应路径使用`+340`或独立固定公式。
- `BattleResultsWriter.ApplyFallDamage`是旧results owner，归B8；当前正式playable未发现把结果页倍率
  写成独立战斗除数的live path。
- `FallDamageDiv` carrier、snapshot/checksum/parity只能在上述全部owner迁移后删除。

## 下一步

`NTSD28-B5-LEGACY-KIND16-PRODUCTION-RETIREMENT-001`：使kind16在candidate disposition、hit-plan
projection及direct character/weapon dispatch中统一unsupported/no mutation，同时保留authority kind15。

## 修改边界与验证

本包只修改治理文档；不改C#、Config、Scene、Prefab、Authority或运行行为。
