# Task Contract — NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / ATOMIC_ACTUAL_AND_SHADOW_PACKAGE_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-OWNER-AUDIT-001 / VERIFIED`

## 目标

冻结 Authority `resolve_special_relation_hit()` → `resolve_kind3_catch_relation()` 建立抓取关系时的
完整原子写集合、Unity exact/compat 字段、DAT 语料活跃度、actual/HitPlan owner 与 focused 验收矩阵；
只定义后续 production 包，不在当前 Unity runtime 验收栈未清时继续叠加行为代码。

## Authority 调用链与顺序

- 正式 playable closure 内 `BattleWorld28::resolve_special_relation_hit()` 在 special-hit latch 与既有
  candidate/ITR identity 检查后，将 `interaction.kind == 3` 分派到
  `BattleWorld28::resolve_kind3_catch_relation()`。
- resolver 重读 `tick_action_snapshot` 对应的 ITR，并要求 source line/interaction index 仍与 frozen
  candidate 一致；kind、target 或 ITR 不可用时不写关系。
- `catchingact`/`caughtact` 各取 DAT 的第一个整数；缺失为 0。负数先翻转对应实体 facing，再取绝对值。
- 两侧 action frame 必须都存在；任一不存在时，整个 relation writer 在任何 facing、frame、坐标、关系或
  timeout 写入前返回 unsupported。
- 成功分支依次确定双方 facing，按两个 action frame 的 center 与首 cpoint X 计算 precise X、以 centerY
  差计算 precise Y，再把旧 target X 与新 anchor X 的一半差同时加到双方 X；双方水平 motion 清零。
- 最后写双方 action、`catch_target_slot_8c`、`catch_source_slot_90`、
  `catch_timeout_94 = respond == 0 ? 300 : respond`，并将 target `hit_reaction_timer` 清零。

## Unity 现状与字段映射

| Authority | Unity exact/primary | 当前状态 |
|---|---|---|
| `catch_target_slot_8c` | `attacker.Runtime.CaughtSlotIndex` | actual 已写；HitPlan 已投影。 |
| `catch_source_slot_90` | `target.Runtime.CatchSourceSlot90` | **actual 未写；HitPlan snapshot/projection/diff 均未覆盖。** |
| relation source compatibility | `target.Runtime.CatcherSlotIndex` | 当前 cpoint settlement 使用的 legacy mirror；后续包继续 dual-write，不冒充 exact。 |
| `catch_timeout_94` | `attacker.Runtime.CaughtDuration` | actual/HitPlan 均硬编码 300；只对当前语料等价。 |
| `hit_reaction_timer` | `target.Runtime.Fall` / `FallCounter` | actual/HitPlan 已清零。 |
| precise/action/facing/motion | `Runtime.X/Y/XInt/Vx`、frame、Dir | 正值 action 与当前 geometry 公式已有对应实现；缺少完整双-frame preflight 与 signed action 原子语义。 |

当前 `BattleInteractionWriter.TryApplyGrab()` 是 character/DAT/weapon/special-attack 四个 shared consumer 的
共同 actual owner；`BattleEcsHitExecutionPlan.ProjectGrabWriterEffect()` 是 shadow owner。旧
`LF2CharacterInteractionResolver.HandlePreInteractionKind()` 不是 unified dispatch 的生产 owner，不得重新启用或
并行修补。

## 语料测量

以逐 ITR block、first-integer 口径读取 Direction B 当前 Unity DAT 与 2.8 release decoded DAT：

| Corpus | kind3 | `respond != 0` | picking/picked 非零 | 缺 catching/caught | 负 catching | 负 caught | 零 catching/caught |
|---|---:|---:|---:|---:|---:|---:|---:|
| Unity `Assets/NTSD/Config` | 1 | 0 | 0 | 0 | 0 | 0 | 0/0 |
| release `resources/runtime/decoded_dat` | 548 | 0 | 0 | 0 | 0 | 0 | 0/0 |

当前 Unity 唯一 kind3 是 `Assets/NTSD/Config/chars/criminal.dat` frame 340：
`catchingact 341 341 / caughtact 130 130`。所以 signed action、missing/default action 与非零 respond 是
必须保留的 schema/runtime 规则，但不是当前或 release 语料首差。`CatchSourceSlot90` 缺 producer 则在任何
成功 kind3 relation 都发生，并且该字段已经被 kind4/resource attribution、raw snapshot/checksum/parity 消费，
属于当前内容可达的首个 exact-field 差异。

## 后续 production 包

建立 `NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001`，原子修改：

1. `BattleInteractionWriter.TryApplyGrab()`：先解码 first action 与 signed facing，在任何写入前预检双方 frame；
   成功时写 `target.Runtime.CatchSourceSlot90 = attacker slot`，继续同步 compat
   `CatcherSlotIndex`，并以 `respond == 0 ? 300 : respond` 写 duration。
2. `BattleEcsHitExecutionPlan.WriterEffectSnapshot`、capture、projection 与 diff：增加 target exact catch-source，
   使用同一 action/preflight/signed/timeout 规则；不能让 shadow 继续只比较 compat mirror。
3. 不把 exact field 替换成 `CatcherSlotIndex`，不在本包迁移后续 cpoint settlement consumer；后者待 relation
   lifecycle cleanup 与 runtime 证据闭合后另行处理。

## 验收矩阵

- 当前 formal criminal frame340 positive actions：slot0/high slot、双方 frame/facing/X/Y/Vx、exact+compat relation、
  duration300、target Fall0；actual 与 HitPlan mask 一致。
- synthetic signed catching、signed caught、双方均 signed；只翻对应 facing，action 使用绝对值，且无额外 RNG。
- `respond=0/1/300/-1` 精确写 timeout；禁止用 state-entry 300 覆盖 writer 结果。
- catching frame missing、caught frame missing、两者 missing：返回 false/unsupported，双方全部字段 bitwise 不变。
- 既有 special-hit latch、candidate order、vrest、group/effect/target-type filters、kind1/pickup 行为保持。
- exact `CatchSourceSlot90` 经 snapshot/checksum/parity 可见；compat cpoint relation仍可继续运行。
- warmed 4096 次 writer/projection 零 managed allocation；ordered shutdown 与 lifecycle cleanup 不新增服务或 publication。
- Unity compile、focused test、B6/NTSD28 regression、BattleRuntimeSelfCheck、formal criminal Play 与 same-tick
  attribution witness；在这些运行证据完成前不得标记 `VERIFIED`。

## 不变量 / 阻塞

- 不改 candidate collection/selection、special-hit latch、hit-group eligibility、kind1/pickup、cpoint settlement、
  lifecycle cleanup、invalid handler、content/Scene/Prefab/importer 或 Authority C++。
- 不把 release 548 条内容覆盖到 Direction B Unity Config；这里只读计数。
- 当前 throw 与 refill production 仍缺 Unity Test Runner/SelfCheck/Play 绿灯，kind3/WPoint 与 lifecycle production
  也已明确 held；本包保持 `PRODUCTION_HELD`，不新增脚本 diff。

## 回滚

仅移除本治理记录与状态摘要；没有代码、content、Scene 或 Authority 回滚。

## Current corpus correction（2026-09-08）

current kind3 ITR由1纠正为249，全部有catching/caught pair且respond0；missing exact CatchSourceSlot90的current
reachability显著扩大。atomic actual+shadow owner与compat dual-write结论不变，测试矩阵以multiline总correction为准。
