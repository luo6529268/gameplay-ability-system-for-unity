# Task Contract — NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TWO_PRODUCTION_PACKAGE_SPLIT_DEFINED / FULL_RESOURCE_DEFERRED`
> 依赖：`NTSD28-B6-POST-THROW-SETTLEMENT-AUDIT-001 / VERIFIED`

## 目标

只读冻结正 held CPoint injury 的 Authority/Unity owner：resource/display 前导、incoming-damage
缩放、lethal gate、直接 owner/self credit、canonical HP/score/KO 计数、cover hold timer，以及
settlement 完成后的 caughtact combo event 边界。

## 范围

- Authority：`battle_world.cpp::settle_catch_relations()`、`record_native_knockout()`、
  `simulation_tick_driver.cpp::produce_caughtact_combo_hits()` 与 decoded CPoint corpus。
- Unity：`BattleCpointWriter.ApplyHeldInjury()`、`BattleDamageWriter` display/resource helper、
  `NTSDEntityRuntime` canonical carriers、`BattleInteractionPipeline.RunPreInteraction()`、native combo producer、
  checksum/parity 与现有 CPoint/SelfCheck/Play probe。
- 只读审计；不改 C#、Config、Scene、Prefab、资源、ProjectSettings 或 Authority。

## 不变量

- held damage 只使用 `IncomingDamageScale340`，不复用 legacy `FallDamageDiv`。
- credit 只解析 catcher 的一层 `OwnerSlotIndex`；missing owner 且 catcher DAT type-0 才回退
  catcher self。不复用 resource attacker 的最多两层 owner 规则，不读 `HolderCopySlot`。
- lethal gate 只读 caught `OrdinaryCreditGate2F4 == -1`；不使用 `KillCount`。
- 不写 `KillStat`、`world.KillStats`、`ComboCountVic`、`ComboCountAtk` 或
  `world.DamageStats`。
- full MP resource transaction 仍受 B7 child suppression、B8 selected-mode override 与 B11/H
  authoritative baseMax 阻塞；本 owner 不授权以 `MPMax`、default 或近似值接线。
- caughtact combo 必须消费整个 held settlement 完成后的 applied-event 集合，不能在
  `ApplyHeldInjury()` 中逐实体立即生产而改变 Authority 顺序。

## Owner 结论

| 责任 | 当前 owner | 结论 |
|---|---|---|
| held actual writer | `BattleCpointWriter.ApplyHeldInjury()` | 第一 production package 的唯一行为写入点。 |
| raw injury/cover | `BattleCatchPointValue` | 当前实施所需字段已存在，无 persistent schema 变更。 |
| display lead | `BattleDamageWriter.ResolveNativeHitResourceAttacker()` + `ApplyNativeHitDisplaySteps()` | 只复用可独立精确的 display 子集；不调用完整 MP transaction。 |
| damage scaling | `NTSDEntityRuntime.IncomingDamageScale340` | 正值时 `raw * 100 / scale`，否则保留 raw。 |
| score/KO credit | 新建 writer-local direct-credit helper | 一层 owner，missing 且 catcher type-0 回退 self；score 与 lethal KO 共用同一 credit。 |
| canonical writes | `HP/HPBound`, `InputHpConsumedTotal34C`, `InputScoreTotal348`, `KnockoutCount358` | 都已有 carrier/copy/checksum/parity，无需扩展 snapshot schema。 |
| cover timers | `attacker.AttackingCounter`, `attacker.FrameDelay`, `victim.FrameDelay` | 严格实现 cover 1/2/3 排除分支；当前两个 corpus 可直接覆盖 0/1/10/11。 |
| negative injury | writer fail-closed branch | release 与当前 Unity corpus 都不可达；不保留无 Authority 支持的负值 healing 逻辑。 |
| caughtact event | `BattleInteractionPipeline` post-held-settlement boundary + transient world/module scratch | 第二 production package；不与 accounting 写入混包。 |
| caughtact producer | 现有 `BattleNativeComboOrdinaryProducer` | 复用 B5 已验证的 one-hop non-type0 source 语义，但只在 formal combo tuple 启用时生效。 |
| KO event feed | 当前 Unity 仅有 `KnockoutCount358` carrier，无等价 world event/feed | 本 accounting package 只能闭合 canonical counter；event publication 需在后续独立 world/session owner 审计中连接，不写 legacy global stats 代替。 |

## 实施顺序与验收

1. `NTSD28-B6-HELD-INJURY-ACCOUNTING-COVER-PRODUCTION-001`：focused RED 先覆盖 raw/scale、
   owner/self/missing credit、lethal/nonlethal/gate/already-dead、canonical counters、legacy-stat sentinel、
   cover 0/1/2/3/10/11，以及负 injury fail-closed；再修正 actual writer、SelfCheck 与抓取 Play probe。
2. `NTSD28-B6-HELD-INJURY-CAUGHTACT-EVENT-PRODUCTION-001`：增加 applied held-injury 事件
   聚合和 post-settlement producer，覆盖 facing/type/owner/formal-tuple gate 与多事件顺序。

第一包是唯一下一 production 候选；但在前置 throw package 仍因 Unity Licensing 未获得
focused/SelfCheck/Play 绿灯时，暂不叠加新行为修改。

## 回滚

仅移除治理记录；没有行为、内容或 Scene 回滚。

## Current corpus correction（2026-09-08）

current kind1 positive held injury不是9条而是223条，cover分布0:205、1:15、11:3。canonical accounting、
cover与caughtact两包owner保留；focused/Play不得继续使用旧小样本完成。详见multiline总correction。
