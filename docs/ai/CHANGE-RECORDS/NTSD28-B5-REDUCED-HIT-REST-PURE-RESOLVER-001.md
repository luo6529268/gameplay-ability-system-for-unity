# NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001 — reduced rest pure resolver

<!-- CHANGE-RECORD
id: NTSD28-B5-REDUCED-HIT-REST-PURE-RESOLVER-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleReducedHitRestResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedHitRestPureResolverEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::apply_reduced_hit_rest at battle_world.cpp 3923-3993; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-13 / FOCUSED-14 / B5-HITPLAN-425 / NTSD28-BROAD-706 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## 原状与边界

Unity alternate actual/HitPlan复制固定3/-5与direct rest公式，缺definition-effect、world reduction、packed delay和
native byte wrap。先建立single pure truth table，不接旧selection或生产路径。

## 验收状态

- red：job `5fdb184bea1047f1b23118ea90df6719`，13/13按预期失败。
- focused：job `ea5b0283b3ff4cb286d641037d26ee1f`，14/14通过，含warm4096 zero-allocation。
- related：B5+HitPlan job `1cce3b0c00b448caa62d55a9b744d4d2` 425/425；NTSD28 broad job `9222d93c92f64cd9abb3a36571381d8c` 706/706。
- compile：Runtime/Editor DLL `2026-09-05T22:20:29Z`；filtered Console 7条均为预期rest-binding自检日志，无CS诊断。
- SelfCheck：`2026-09-05T22:27:22Z` PASS。
- Scene：SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、长度205625、mtime `2026-09-05T15:44:52.7794120Z`不变。

## 实际实现与边界

- 新增`BattleReducedHitRestResolver`及只读result：default/null、delay -1、definition-effect/reduction、signed packed delay、frame-counter clear、4/12 arest与native-byte vrest全部显式。
- 新增14个case，覆盖正/零/负packed delay、256/258 wrap及无分配。
- production、HitPlan、defense selection、armor model/content均未接；回滚删除resolver/test及两个meta即可。
