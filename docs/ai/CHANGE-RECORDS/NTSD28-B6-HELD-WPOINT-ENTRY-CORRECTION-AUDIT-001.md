# NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-WPOINT-ENTRY-CORRECTION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan SimulationTickDriver28 C09/C20 held-refill placement and BattleWorld28::settle_held_refill_objects; EXE B1E13AE1, closure 39DDDA15.
evidence: global B6 scan corrected to include pre-geometry C09; Unity held-refill/WPoint path compared end-to-end; an immediate OID122/123 exhaustion enters before shared WPoint and writes wrong Vy/Vz, while OID123 nonpositive entry and +0x2F4 cap also differ; release/current WPoint distributions retained for the subsequent kind3 route; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / GLOBAL_B6_ORDER_CORRECTED / HELD_REFILL_MP_EXHAUSTION_ROUTED`

## 纠正结论

- Authority C09 第一次 held-refill/WPoint settlement 在 C08 depth clamp 之后、collision action
  snapshot 和所有 hit/catch 之前。Unity placement 已由 B3 验证，但 B3 record 明确将
  refill/WPoint/release/RNG 行为留给 B6。
- 因此 `NTSD28-B6-ENTRY-CATCH-SETTLEMENT-AUDIT-001` 的 throw 结论只是
  **post-hit catch 子链首差**，不是整个 B6 的全局首差。既有 throw 代码仍保留为有效
  子链修正，其 runtime 状态与许可阻塞不变。

## 最早可达差异

- 在 shared WPoint tail 之前，OID122 HP-refill 与 OID123 MP-refill 均可在当次扫描将
  child HP 降到 `<1`并立即进入 exhaustion。Authority 随机draw一次 `[0,7)` 写
  child Vx，写 Vy=0，保留入场 Vz，再重置双方action/counter/relation与child weapon HP。
  Unity虽draw相同Vx，却写 Vy=-8 并清 Vz=0。当前 content 明确包含data.txt
  OID122/123，因此以 child HP=1/2 即可在C09可达；这比kind-3 WPoint tail更早。
- OID123 还有两个同一前置分支差异：Authority 对入场 HP<=0 仍执行`HP-=2`、
  holder MP+3与exhaustion，Unity却提前return；Authority以child
  `OrdinaryCreditGate2F4 >= 0 && child.MP > 150` 将 **child MP** 截到150，Unity以
  legacy `KillCount` 为gate且误写 **holder MP**。

- Authority 有效 reciprocal negative child 在普通 refill/WPoint pose 后，对 parent WPoint
  `kind == 3` 固定消耗4个 synchronized RNG：顺序为 frame `[0,6)`、X `[0,7)`、
  Y `[0,4)`、Z `[0,5)`。四个draw始终消耗；每轴 authored `dv* != 0` 时使用 authored
  值，否则使用对应 random，Z 的 random 值是整数 `-2..2`。
- Unity `BattleHeldObjectWriter.DropRandomly()` 也消耗相同4个 draw，但始终忽略
  authored `Dvx/Dvy/Dvz`，且把 Z 写成 `(roll - 2) * 0.2`。在 authored dv 全为0的
  当前内容中，frame/X/Y 一致，第一可观察差异是非零 Z roll 的幅度（每次
  deterministic invocation 有4/5的 roll 值会分歧）。
- release decoded corpus：38163 条 WPoint，2744 条 kind-3，其中2739条 authored dv全零、
  5条至少一轴非零。当前 Direction-B Unity Config：312 条 WPoint，12 条 kind-3，
  全部 authored dv为零。两个corpus的 WPoint cover 都只有0/1，因此合成 cover=2
  pose 差异不是正式内容首差。
- refill OID122 的HPBound/baseMax clamp仍受B11/H authoritative baseMax边界影响；下一包只改
  不依赖baseMax的OID123 MP gate和两类exhaustion tail，不宣称整个refill已对齐。
- invalid reciprocal、terminal weapon action、cover=2、DVX release source/+2F8 仍是后续 B6
  审计项，未被写成已对齐。

## 下一步

`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-OWNER-AUDIT-001 / VERIFIED / GOVERNANCE_ONLY / ACTUAL_ONLY_PACKAGE_DEFINED`
已冻结真正的下一 actual/test owner。`NTSD28-B6-WPOINT-KIND3-RELEASE-OWNER-AUDIT-001`
仍是后续有效 owner，但不再称为全局下一包。CPoint throw 代码仍无 Unity runtime 绿灯，
因此当前两个 production 候选都不叠加新行为修改。

## CORPUS CORRECTION（2026-09-08）

入口/早期refill结论保留；current WPoint312/kind3-12由全量7995、holder-kind3 770与authored overlap1替换，
terminal28成为current reachable后继。

## HELD RELATION DOMAIN CORRECTION（2026-09-08）

OPoint kind2 producer补入后，完整current kind3/terminal为811/33；原770/28保留为pickup子集。
全局C09入口与refill首差路由不变。
