# NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001 — ordinary defense pure resolver

<!-- CHANGE-RECORD
id: NTSD28-B5-ORDINARY-DEFENSE-PURE-RESOLVER-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleOrdinaryDefenseResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryDefensePureResolverEditorTests.cs
authority: NTSD 2.8-Logan defense_resolution.cpp DefenseResolver28::match_ordinary and locked system defend OID 822; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-13 / FOCUSED-14 / B5-HITPLAN-439 / NTSD28-BROAD-720 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## 原状

Unity `LF2AlternateDamageResolver`仍以旧OID37/6/52、Prev2 state与bdefend<=60启发式选择alternate；缺current
state70/75、spark/dbdefend、effect61前门与formal system OID822。先建立single pure truth table，不接production。

## 实际实现与验证

- 新增`BattleOrdinaryDefenseResolver`及三态result，按Authority顺序实现inactive/applies/bypassed与
  `UsedTwoWayDefenseObjectId`，OID822只在state7其余触发均未命中时消费。
- red job `19f8d99805724c41a0f03150838012ba` 13/13；focused job
  `b072776e7b6c4bcf8c73107a108fa8d7` 14/14，含4096 zero-allocation。
- B5+HitPlan `efa692df393742fb9a3f29fcee406a02` 439/439；NTSD28 broad
  `55766df0360b4fb18cadbe25a0a99bf4` 720/720；22:41:19Z SelfCheck PASS。
- Runtime/Editor compile `2026-09-05T22:34:43Z`；filtered CS0；Scene unchanged；Ledger283/244 PASS。

production/HitPlan未接。下一reduced-hit damage pure core；回滚删除resolver/test与两个meta即可。
