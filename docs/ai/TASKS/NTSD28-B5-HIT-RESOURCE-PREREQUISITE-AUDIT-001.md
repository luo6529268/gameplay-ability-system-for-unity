# Task Contract — NTSD28-B5-HIT-RESOURCE-PREREQUISITE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / RESOURCE_SPLIT_DEFINED`

## 目标

只读闭合 `apply_native_hit_resource_transfer` 及相邻 armor/weapon-strength/effect 链的当前
可实施边界，避免在缺少内容/模式合同的情况下把旧Unity资源规则误当新权威。

## 已观察结论

### 可立即独立实施

- positive injury首先写四个display interpolation step：score/damage/current-HP=`injury/10`，
  effective-max-HP=`injury/20`；该写入发生在local-resource/F6 gate之前。
- 四个runtime carrier、reset/copy/snapshot/checksum已存在。

### 可做pure core、production仍需前置

- resource injury：attacker injury-double后，definition `stats.attacking`优先，否则mode
  `attacking percent`，有余数>=50向上取整。
- type0↔type0 injury MP reward、target drain、resource-attacker gain均为整数/all-or-nothing规则。
- 当前已有MP/MPMax、consumed totals与injury-double；但缺definition stats.attacking正式模型、
  mode 1C/34/38 rules、hit-resource suppression 15C及resource-attacker完整归属接线。
- F6 session state已有`HitResourceEnabled`，尚未接入hit resource production gate；现有entity
  `InputLocalResourceEnabled49D034`不能在无证据时冒充唯一world gate。

### 必须继续独立处理

- armor：authority正式内容有18个armor block，而当前Unity正式内容为0；programmatic HP/recovery
  core已有，但selection/reduced-hit/break仍受H/B11内容/schema策略约束。
- held weapon strength decrease/selfinjury依赖holder WPoint attacking与weapon-strength正式schema。
- effect-action override、direct effect、damage defend/weakness scale、resource-attacker attribution、
  armor/reduced分支应分别建包，不能与display step合并。

## 实施顺序

1. `NTSD28-B5-HIT-DISPLAY-STEP-PRODUCER-001`：四字段、positive-only、local gate之外。
2. hit-resource rules/carrier与pure arithmetic core。
3. F6/world gate、resource-attacker attribution与production transaction。
4. effect/damage scaling；armor与weapon strength等待H/B11正式schema/content边界。

## 验证

Authority resource function/call order与Unity runtime/session/data owners已静态闭合；本包无脚本、
Config、Scene或Authority修改。
