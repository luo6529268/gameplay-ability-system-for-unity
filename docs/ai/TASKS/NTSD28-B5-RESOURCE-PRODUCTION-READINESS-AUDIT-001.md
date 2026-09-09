# Task Contract — NTSD28-B5-RESOURCE-PRODUCTION-READINESS-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / PRODUCTION_DEPENDENCIES_ROUTED`
> 依赖：resource pure core、attacker resolver、world rules carrier、F6 projection均`VERIFIED`

## 目标

只读闭合hit-resource transaction的全部authority调用面、Unity插入位置和剩余依赖，判断是否可以
立即连接production，避免以默认0或Unity旧字段伪造正式语义。

## Authority 结论

- standard unarmored与selected-armor命中均在HP/credit mutation后调用同一个resource helper；type6跳过
  整个damage/resource block。
- helper先按raw injury写四个display step，再以attack-effect source的definition `stats.attacking`优先、
  world active-mode `1C`回退计算resource injury；随后按world local gate执行75/75 reward、target drain、
  resource-attacker gain。
- physical attacker与resource attacker不是同一概念；后者按`OwnerSlotIndex`最多两跳解析。
- cpoint held injury与throw injury也调用同一helper，分别属于B6 catch/held生产面。
- selected armor仍调用resource helper，但明确不运行unarmored statuses/join链。

## Unity readiness结论

已具备：pure injury/transaction、两跳resolver、suppression carrier、world `1C/34/38` carrier、唯一F6
gate与active/registration projection。

尚未具备且不能猜测：

1. `LF2CharacterData`/正式definition model没有`stats.attacking`；归H/B11 content schema。
2. `Runtime.MPMax`虽存在，但Unity初始化/Config尚不能证明等同definition `stats.max_mp`的
   `base_max_mp`；归H/B11。
3. selected-mode对`1C/34/38`覆盖没有Unity mode-record mapping；归B8/H。
4. special child的suppression继承/type0强制1 producer尚未接；归B7/C25b。
5. selected armor正式schema/selection/reduced mutation未闭合；归B5+H/B11。
6. cpoint held/throw调用面归B6。

## 决策

当前不得只在standard hit局部接入transaction，否则会把缺失definition值当0、错误使用MPMax，并在
child/armor/cpoint/mode路径产生不一致。production package保持`BLOCKED_BY_ROUTED_DEPENDENCIES`，但B5
可继续处理不依赖这些字段的damage-scale/effect等独立差异。

## 修改边界与验证

本包只修改治理文档；不改C#、Config、Scene、Prefab、Authority或运行行为。Authority四类调用面与
Unity data model/production owner均已静态闭合。
