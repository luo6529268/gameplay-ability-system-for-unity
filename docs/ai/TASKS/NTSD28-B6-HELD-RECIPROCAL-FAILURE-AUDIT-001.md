# Task Contract — NTSD28-B6-HELD-RECIPROCAL-FAILURE-AUDIT-001

> 状态：`SUPERSEDED / REACHABILITY_RESOLVED_BY_NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`
> 依赖：`NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001 / VERIFIED`

## 目标

继续闭合C09/C20 held traversal前置，比较negative child的parent slot/reciprocal失败处理，
并把规则差异与正常lifecycle可达性分开记录。

## 已观察事实

- Authority `BattleWorld28::settle_held_refill_objects()`只扫描`interaction_state < 0`的active
  child。parent slot越界、parent不存在或`parent.linked_child_slot != child_slot`时：
  - `pass.success=false`并追加diagnostic；
  - `continue`到下一child；
  - 不修改child/parent relation、action、motion、RNG或lifecycle。
- Unity `SimulationQueryAndLinkModule.HeldObjectProcessAll()`在holder不存在或target slot不匹配时，
  立即将`held.Runtime.LinkState=0`并刷新snapshot，然后continue。因此同一入场状态的
  checksum/下一次C20可见关系不同。
- Unity approved extended slot profile属于用户例外；后续实现应使用active world logical capacity，
  不能机械硬编码native 1000来破坏已批准容量模型。

## 可达性边界

- 合成invalid parent/mismatch可直接证明规则差异，但不等于正常battle已必然产生该状态。
- 需要沿holder despawn、slot reuse、pending destroy、consume/DVX/kind3 release、world reset和
  ordered shutdown逐一证明是否可能让negative child跨到下一C09/C20。当前没有足够fresh runtime
  证据，因此标记`DYNAMIC_REACHABILITY_PENDING`，不得称正式场景已复现。
- terminal WPoint与cover=2在current/release corpus均为0；当前Direction-B的29个type1/2/4/6
  object中，16个WPoint-referenced action没有任何state12/18 frame。它们不是本差异的可达性证据，
  后续仍按规则/未来H单独审计。

## 后续 owner / 验收

先做`NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`，只读列出全部relation
writer/holder removal/slot reuse顺序并跑可用的trace；只有证明production可达或决定按规则无条件
闭合后，才建立actual package修改`SimulationQueryAndLinkModule`。

actual若获准，focused至少覆盖missing parent、mismatch、slot0、extended high slot、slot reuse、
C09→C20两次可见性、RNG零消耗与双方sentinel；不得顺手改变positive-link validation或shutdown。

## 不变量

- 不改kind3、DVX、refill、capacity、content、Scene或lifecycle顺序。
- 当前runtime stack未清，本轮不修改脚本。

## 回滚

仅移除治理记录；没有代码、内容或Scene回滚。

## 后续结果（2026-09-08）

Authority despawn与Unity registry release/reuse链已证明正常production存在清理时点差异及ABA窗口；
动态可达性不再pending。以后继
`NTSD28-B6-HELD-RECIPROCAL-LIFECYCLE-REACHABILITY-AUDIT-001`为准。
