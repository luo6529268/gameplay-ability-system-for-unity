# Task Contract — NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / GLOBAL_B6_ORDER_CORRECTED / HELD_REFILL_MP_EXHAUSTION_ROUTED`
> 依赖：`NTSD28-B3-C09-HELD-REFILL-PLACEMENT-001 / VERIFIED / BEHAVIOR_PENDING_B6`

## 目标

纠正 B6 入口审计只从 post-hit catch 子链开始的范围遗漏，按 Authority 完整 tick
顺序重新检查 C09 第一次 `settle_held_refill_objects()`，并选出整个 B6 最早的
当前内容可达行为差异。

## 范围与不变量

- Authority：`SimulationTickDriver28::step()` C08→C09→geometry 顺序、
  `BattleWorld28::settle_held_refill_objects()` 全函数、system DAT refill 表、正式 WPoint corpus。
- Unity：`NTSDBattleTickSystem` 两次 HeldProcess placement、`SimulationQueryAndLinkModule`、
  `BattleHeldObjectWriter`、`LF2WeaponHeldStateResolver`、WPoint formal value 与旧测试。
- 只读审计；不修改 C#、Config、Scene、Prefab、资源、ProjectSettings 或 Authority。
- 既有 CPoint throw 修正仍是有效的 post-hit 子链修正；本纠正不回退该代码，
  只撤销它作为“全局 B6 首差”的表述。

## 验收

- 证明 C09 早于所有 hit/catch，且 Unity 实际仍调用未经 2.8 行为闭合的 held writer。
- 从 refill、reciprocal relation、WPoint pose/release、RNG 中选出最早的正式内容可达差异。
- 测量 release 与当前 Direction-B Unity content 的 WPoint 字段分布，不把合成 fixture
  冒充正式可达性。

## 回滚

仅移除治理纠正记录；没有行为、内容或 Scene 回滚。

## Current corpus correction（2026-09-08）

入口顺序与refill首差结论保留；current WPoint312/kind3-12的语料附注由总correction替换为全量7995、
holder-primary kind3=770且authored-DV overlap=1。terminal28也已成为current reachable后继。

## Held relation producer-domain correction（2026-09-08）

完整ITR+OPoint held union把current kind3/terminal进一步修正为811/33；原770/28仅是pickup子集。
入口顺序、refill优先与Rock Lee authored-DV witness不变。
