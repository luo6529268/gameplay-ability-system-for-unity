# Task Contract — NTSD28-B5-WORLD-RESOURCE-RULES-F6-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / WORLD_RULES_SPLIT_DEFINED`

## 目标

只读确定world hit-resource rules、F6 gate和entity trace projection的唯一owner及实施顺序。

## Authority 结论

- world rules由`BattleWorld28`持有：active-mode attacking percent默认0、local gate默认true、
  attacker/target injury-MP percent由playable config默认75/75。
- battle创建时host一次性投影rules；F6 accepted toggle后同tick更新world local gate，并把当前gate
  投影给所有active entity的`input_local_resource_enabled_49d034`。
- 任意后续spawn从world gate继承entity trace字段；entity字段只用于输入/action及trace consumer，
  hit-resource transaction必须读world gate。
- selected background/mode record可覆盖1C/34/38，但Unity当前`GameModeConfig`没有对应字段；
  非普通模式映射保持B8/H，不在本包猜测。

## Unity owner结论

- `BattleRuntimeState.FunctionKeys.HitResourceEnabled`已是唯一F6 session gate，已有reset/snapshot/checksum。
- 不新增第二个local bool；hit transaction直接读FunctionKeys。
- 新建world numeric rules carrier，仅保存1C/34/38，默认0/75/75；进入core scalar snapshot、restore、
  checksum和full parity。
- `SimulationTickDriver.ApplyPendingBattleFunctionKeyCommandsForTick`在dispatch前后比较gate；仅真实
  change时调用world projection。
- Registry admission在实体成为active时写入当前FunctionKeys gate；不得依赖以后F6才修正。

## 实施拆分

1. `NTSD28-B5-WORLD-HIT-RESOURCE-RULES-CARRIER-001`：numeric carrier及snapshot/checksum/parity。
2. `NTSD28-B5-F6-RESOURCE-GATE-PROJECTION-001`：accepted toggle active projection + registration inheritance。
3. B11/H提供stats.attacking/baseMax/mode override后，接production transaction与真实trace。

## 验证

Authority GameSession bootstrap/F6 projection/spawn inheritance、BattleWorld defaults和Unity FunctionKeys/
driver/registry/snapshot owners均已静态闭合。本包无脚本、Config、Scene或Authority修改。
