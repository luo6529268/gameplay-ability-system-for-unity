# Task Contract — NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / POST_HIT_SCOPE_ONLY / CPOINT_THROW_SETTLEMENT_ROUTED`

> 2026-09-08 纠正：本合同实际只审计 post-hit catch/settlement 子链。更早的 C09
> held-refill/WPoint 入口由 `NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001`
> 补审；本合同的 throw 结论不回退，但不再称为整个 B6 的全局首差。
>
> 2026-09-08 schema口径补充：本合同所称4019条是2.8 release decoded完整CPoint corpus；
> Direction-B当前33条中有1条`aaction`与1条负`taction`。`NTSD28-B6-CPOINT-27-SCALAR-SCHEMA-OWNER-AUDIT-001`
> 已按两端分别计数，并发现release三条`drain=600`；不得再把release无A/T外推为当前内容无A/T。
> 依赖：`NTSD28-B5-EXIT-GATE-AUDIT-001 / VERIFIED`

## 目标

从Authority hit消费后的catch relation advance、catch settlement、held refill、stage settlement与horizontal/
depth impulse finalizer开始，逐调用链对照Unity 4.8抓取/持有/武器/CPoint路径，选择B6第一个可独立实施的
production差异或前置carrier/owner审计。

## 范围与边界

- 只读`simulation_tick_driver.cpp`、`battle_world.cpp`的catch/held/cpoint/impulse调用链与Unity
  `BattleCpointWriter`、interaction/held passes、runtime carriers、HitPlan及既有记录。
- 复核B5后置的caughtact combo producer和cpoint hit-resource caller，但不提前接线；它们必须消费B6真实
  settlement/injury event。
- 不改C#、Scene、Prefab、Config、资源、ProjectSettings或Authority；不重开已验证B5 ordinary combo/expiry。

## 验收

- 冻结正式pass顺序、active-slot/live mutation边界、字段读写和事件结构。
- 定位Unity首个production-reachable差异，确认既有carrier与依赖，给出单一下一包。
- 同步Ledger/STATE/handoff/总表；未知行为保持待确认。

## 结论

- Authority 的 post-hit 顺序已闭合为 `advance_catch_relations` →
  `settle_catch_relations` → caughtact combo producer → stage depth clamp →
  held refill → stage settlement → horizontal/depth impulse finalizer。
- Unity 的 kind-1 advance 已覆盖关系失配、`decrease`、现有 A/T/J 输入、throw 和
  `dircontrol`，随后才在 held sync 中执行 victim action、injury 与位置同步；这两个 owner
  不能合并成一次无序重写。
- Authority 输入动作还定义 D/UZ/DZ/F/B 固定覆盖顺序，但当前正式 4019 条 CPoint corpus
  中 A/J/D/T/F/B/UZ/DZ 均无显式记录，所以它是规则/数据契约缺口，不是本轮可证明的首个
  正式内容可达差异；归入后续 CPoint contract/H 内容切换共同关闭。
- 第一个正式内容可达差异位于 throw tail：Authority 75 条非零 `throwvx`，其中 6 条同时
  具有最终正 `throwinjury`；正值先执行原生命中资源事务，再写 caught
  `EnvironmentState320=throwinjury` 与 `EnvironmentSourceSlot160=self`。Unity 却写
  `victim.WeaponCount`，且无该资源事务；无独占 depth 输入时 Unity 还把 Vz 强制清零，
  Authority 保留原 Vz。
- 下一唯一包：`NTSD28-B6-CPOINT-THROW-OWNER-AUDIT-001`，先冻结字段、writer、HitPlan、
  resource helper 与测试责任，再拆 test-first production 包。

## 回滚

仅移除治理记录；没有行为、内容或Scene回滚。

## Current corpus correction（2026-09-08）

Direction-B current CPoint不是33/A-T各1，而是1426/A-T各9；current throwvx58、positive throwinjury48。
release75与本记录的Authority throw规则保持；全局B6入口纠正及后续owner仍有效。详见multiline总correction。
