# Task Contract — NTSD28-B5-RESOURCE-CARRIER-ATTRIBUTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / CARRIER_ATTRIBUTION_SPLIT_DEFINED`

## 目标

只读确定hit-resource production所需carrier、world rules/F6 gate和resource-attacker解析，禁止
把Unity的stable/holder字段或冻结内容差异误接为权威值。

## Authority 与 Unity 映射

### Resource attacker

- authority从physical attacker slot开始，沿`owner_slot`最多两跳。
- `owner_slot < 0`或self-owner是合法terminal；若声明的next slot失活/越界则解析失败，不退回当前节点。
- Unity精确字段是已验证的`NTSDEntityRuntime.OwnerSlotIndex`；不得使用`OwnerStableId`、
  `HolderStableId`、`HolderCopySlotIndex`、`RelationOwnerSlotIndex`或Spawner字段。
- `SimulationWorld.FindEntityByRuntimeSlotForQuery`可作为当前slot解析入口。

### Entity carrier

- `hit_resource_suppression_15c`当前完全缺失。
- authority spawn默认0；special child materialize会继承parent的literal 1，且type0 child强制1。
- hit-resource读取physical attack-effect source的该字段，不读取最终resource attacker。

### World rules / F6

- authority world字段：active-mode attacking percent默认0；local gate默认true；attacker/target
  injury-MP百分比由playable config默认75/75并可由selected mode覆盖。
- Unity `Runtime.FunctionKeys.HitResourceEnabled`已具备reset/snapshot/checksum与F6 toggle，但尚未接
  hit-resource transaction world gate。
- Unity entity `InputLocalResourceEnabled49D034`已存在，但不能代替world transaction gate；
  后续仍需在bootstrap/F6/spawn投影该值，保证trace字段一致。

### B11/H依赖

- definition `<stats> attacking`当前parser block存在，但`LF2CharacterData`与converter未保留该字段。
- base max MP的候选是`Runtime.MPMax`，但真实双端基线仍有Unity500/authority200内容差异。
- 以上两项都受B11/H内容/schema策略约束，当前不得用默认常量掩盖。

## 后续拆分

1. `NTSD28-B5-HIT-RESOURCE-SUPPRESSION-CARRIER-001`：carrier reset/copy/snapshot/checksum/parity；
   producer仍归B7/C25b。
2. `NTSD28-B5-RESOURCE-ATTACKER-RESOLVER-001`：两跳physical owner解析与缺slot失败。
3. world hit-resource rules carrier与F6投影：需与B8 mode/bootstrap边界共同设计。
4. B11/H闭合stats.attacking和baseMax后，才能接production transaction并跑真实trace。

## 验证

Authority resolver/spawn/F6 projection/header defaults、Unity B0 owner binding、FunctionKey state、runtime
carrier与parser/model均已静态核对。本包无脚本、Config、Scene或Authority修改。
