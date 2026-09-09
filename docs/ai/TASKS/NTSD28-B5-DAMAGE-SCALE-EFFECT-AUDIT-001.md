# Task Contract — NTSD28-B5-DAMAGE-SCALE-EFFECT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / DAMAGE_AND_EFFECT_SPLIT_DEFINED`

## 目标

只读核验standard hit中damage scale、weakness与effect家族的权威顺序和Unity owners，确定可独立实施的
首差，不把armor/content/effect-action混成一个大包。

## Authority 结论

- unarmored HP injury严格先经过target `incoming_damage_scale_340`：正值时执行32位`injury*100`
  后signed除法；再仅当attack-effect source `weak_timer_12c>0`时整数除2。
- selected-armor分支先由armor helper得到reduced HP/MP damage，再只应用target `+340`；明确不应用
  attacker weakness。
- `stats.defend`与mode damage percent生产`+340`，definition/fusion/spawn/child继承属于H/B11+B7/B8；
  但`+340`与weak carrier本身已经进入reset/copy/snapshot/checksum/parity。
- kind0 post-effect action独立发生：effect 3/30按previous-state!=13写action200；effect 2/21/22及
  effect20 previous-state!=18写action203，并按pending X符号写朝向。
- effect 8..16另有target-type matrix、latched-frame首个BDY kind50/52、state602/603、definition
  property2/3与caughtact sentinel suppression，再决定catchingact/caughtact override。
- effect eligibility gates更早位于candidate/consumer边界；audio与spark又是后续独立副作用，不得与
  HP scale一起迁移。

## Unity 结论

- `ApplyStandardVitalAndStatWrites`仍直接消费raw injury；weapon路径只使用旧`FallDamageDiv`；type3
  normal路径也直接消费raw injury。三者均没有按2.8执行`+340 -> attacker weak`。
- `ApplyType3EffectTail`只挂在special-object路径，不能覆盖共同kind0 post-effect/override矩阵；同时
  仍包含2.8权威未确认的5000..5999 PP与6000..6999 direct-action扩展，不能据旧实现晋升。
- Hit execution plan已有大量legacy effect observation/projection，但它们是旧路径兼容设施，不等于
  当前2.8 effect family已经对齐。

## 实施拆分

1. `NTSD28-B5-UNARMORED-DAMAGE-SCALE-CONSUMER-001`：只实现无分配32位scale+weak kernel，并接
   当前unarmored standard target type paths；不接armor/effect/producer。
2. H/B11+B7/B8：补`stats.defend`与mode/child producer。
3. selected-armor consumer随正式armor package。
4. effect eligibility、post-effect action、8..16 override、audio/spark分别独立审计/实施；5000/6000
   扩展在得到当前权威或用户例外前不得作为正式2.8行为。

## 修改边界与验证

本包只修改治理文档；不改C#、Config、Scene、Prefab、Authority或运行行为。Authority helper/order与
Unity三条damage owner、effect owner已静态闭合。
