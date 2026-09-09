# Task Contract — NTSD28-B6-CATCH-ADVANCE-SLOT-ORDER-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / SINGLE_ASCENDING_MIXED_PASS_REQUIRED / EXACT_CONSUMER_PACKAGE_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-OWNER-AUDIT-001 / VERIFIED`

> 2026-09-08 补充：`NTSD28-B6-CATCH-CONTROL-FLOW-FENCES-OWNER-AUDIT-001` 已证明 reciprocal
> mismatch 与 negative-decrease release 均须立即结束当前 slot；同一个后续 mixed/exact production
> 包还必须纠正 `HitCount`/`AttackingCounter` carrier 和旧 fallback throw/dircontrol SelfCheck 断言。

## 目标

冻结 Authority `advance_catch_relations()` 的单一 slot 升序混合分派语义，纠正 Unity 将 kind1 catcher 与
kind2 orphan validation 拆成两个全局 sweep 的顺序差异，并将后续 exact relation consumer 迁移与
`settle_catch_relations()` 的独立边界一起定义。

## Authority 顺序

- `advance_catch_relations()` 只执行一个 `slot=0..capacity-1` live loop。
- 每个 slot 先读取 `tick_action_snapshot`：若是 kind1 且 `motion_hold_timer>=0`，当场处理 reciprocal、
  decrease、输入 action、throw 与 dircontrol。
- 否则才读取该实体 current action；仅 current cpoint kind2 时，以该实体
  `catch_source_slot_90` 找 source，再检查 source current kind1 与 source
  `catch_target_slot_8c==caught slot`，失败才写 caught action212/Vy-3/Y<=-2。
- 这不是两个可交换阶段。较低 slot 的 kind2 target 会在较高 slot catcher 改 action/throw **之前**验证；
  较高 slot target 会在 catcher 之后验证。
- `settle_catch_relations()` 是 advance 完整结束后的第二个独立升序 pass；它读取双方 current action与同一
  exact reciprocal字段。该独立 pass 不能合并进 advance slot body。

## Unity 首差

`BattleInteractionPipeline.RunPreInteraction()` 当前顺序为：

1. 对 `participantScratch` 全体运行 `RunCpointCheckStep10()` → `BattleCpointWriter.RunKind1()`；
2. 再对全体运行 `RunCpointMismatchTailStep10()` → `RunKind2Validation()`；
3. 最后 live ascending 运行 `RunWeaponSyncHeldStep10()` → `SyncHeldCpoint()`。

因此 Unity 把所有 kind2 target 都当成“位于所有 catcher 之后”。若高槽 catcher 在第一 sweep 从 kind1
throw 到非-kind1 next，而低槽 target 仍进入 kind2 vaction，第二 sweep 会错误地立刻把低槽 target改为212；
Authority低槽 target已在 catcher之前验证通过，应保留该 vaction到下一 tick。高槽 target则两端都会写212。

此外三个 Unity consumer均以 compat `CatcherSlotIndex` 检查 source；Authority只读 exact
`CatchSourceSlot90`。在前置 producer 包完成 dual-write后，advance与settlement必须迁到exact字段；compat只
保留旧API/过渡镜像，不能继续裁决关系。

## 语料与可达边界

- Direction B 当前 Unity 的2条 kind1 throw CPoint只写 vaction180/181；29个当前 character定义没有相应
  kind2 frame交叉匹配，因此本顺序差异没有当前内容 witness。
- release 75条 kind1 throw CPoint 的 vaction 集合为85/102/132/180/181/183/701；其中 Hinata 与 Neji
  action343各有一条 `throwvx=5, vaction=132, next=344`，action344是state15、非kind1。
- release `data.txt` 的 type0 definitions 中有134个 definition/action132 kind2交叉匹配。因此低槽
  victim/高槽 Hinata或Neji catcher是正式内容可构造的规则 witness；精确玩家输入/技能进入action343的真实
  Play路径仍标 `RUNTIME_WITNESS_PENDING`，不把静态交叉匹配夸大成已复现。

## 后续 production 包

`NTSD28-B6-CATCH-EXACT-CONSUMER-AND-ADVANCE-ORDER-PRODUCTION-001` 必须在
`NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001` 与 entity-link lifecycle cleanup runtime绿灯之后：

- 将 advance 改为单个 slot 升序 mixed dispatch；writer提供“snapshot kind1 eligible / else current kind2”
  的单-slot原子入口，不能简单在同一 slot 无条件先RunKind1再RunKind2。
- `RunKind1` reciprocal、kind2 orphan与`SyncHeldCpoint` settlement统一读取 plain exact
  `CatchSourceSlot90`；不得把 `>=0x2000` attribution tag解码成catch relation。
- 保留 settlement 为 advance 后的独立完整 pass；保留三类 no-op proof/participant filter，但其cache/proof
  必须按新 mixed order重新证明，不能沿用旧“两 sweep 可交换”假设。
- compat `CatcherSlotIndex`继续由producer/lifecycle同步，直到独立compat retirement证明所有外部调用者已迁移。

## 验收矩阵

- synthetic低槽caught/高槽catcher：kind1 throw到非kind1 next + kind2 vaction，caught保持vaction；下一tick才
  按Authority orphan规则处理。
- synthetic低槽catcher/高槽caught：同一输入下高槽caught当tick写212；证明slot极性。
- slot0、extended high、相邻/非相邻、catcher=caught拒绝、missing source、mismatch reciprocal、encoded
  `0x2000+slot` source、stale generation/reuse。
- positive/zero/negative decrease、input-selected kind1→kind1、throw victim non-kind2、dircontrol，确认只改变
  应受slot顺序影响的case，RNG为0。
- current Unity vaction180/181 corpus guard输出不变；release action343/132 synthetic definition fixture命中。
- exact/compat故意设不同：全部正式consumer以exact裁决；checksum/parity/snapshot可见。
- legacy与optimized/no-op proof两路径、4096 warmed zero-allocation、compile、focused B6/NTSD28、SelfCheck与
  formal Hinata/Neji Play。Play前状态只能 `RUNTIME_PENDING`。

## 不变量 / 阻塞

- 不改 relation producer、CPoint action算法、throw资源、held WPoint、lifecycle cleanup、content/Scene/Authority。
- 不把release内容写进Direction B Config；134只是只读交叉匹配。
- 当前多个B6 production缺Unity runtime绿灯；本包保持held，本轮无脚本修改。

## 回滚

仅移除本治理记录；没有代码、content、Scene或Authority回滚。

## Current corpus correction（2026-09-08）

旧“2条throw / 29 character definitions”是单行漏计。projection中current kind1 throwvx有58条、type0
definitions为42；重新联结全部source vaction与type0 kind2 frame后仍为0 pairs，因此“mixed-order暂无current
content witness”结论保留。release witness与single mixed-pass owner不变。
