# NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001 — standard hit rest pure resolver

<!-- CHANGE-RECORD
id: NTSD28-B5-STANDARD-HIT-REST-PURE-RESOLVER-001
status: VERIFIED
change-kind: TEST_FIRST_PURE_CORE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleStandardHitRestResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestPureResolverEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::apply_standard_hit_rest at battle_world.cpp 3854-3920; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-20 / FOCUSED-21 / B5-HITPLAN-404 / EXACT-779 / SELFCHECK-PASS / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## 原状与边界

三类input carrier已ready，但Unity仍在各actual/HitPlan路径重复raw rest公式。本包只建立单一pure truth table，
不改调用者；production integration另包。

## 验收状态

- test-first red：job `4a1a81f590e947e29e46a08f085fc174`，20/20按预期失败；随后新增resolver及第21个zero-allocation case。
- implementation：新增`BattleStandardHitRestResolver`与只读结果结构，精确实现recover、双方definition effect、0..5 reduction、arest特例和native uint8 vrest语义；不读取或写入World/Unity对象。
- focused：job `74ae79b7f7244410ba1f816a4a079f03`，21/21通过。
- related：B5+HitPlan job `f39a6241b4f849b28b85c2157fcaedee`，404/404；exact NTSD28 job `c26607abe78c40fe9f45a59a3b1b7ad2`，779/779。
- compile：Runtime `2026-09-05T21:24:42.1263702Z`、Editor `2026-09-05T21:24:42.6024203Z`完成；filtered Console仅7条预期日志。
- SelfCheck：`2026-09-05T21:32:09Z` PASS。
- Scene：SHA-256 `D4266C6D0A802975B50E481E1D01662DC79AEF168C6E2DE14AE382396FDA583B`、长度205625不变。
- Ledger：validator PASS（278 records / 239 governed code files）。

## 实际文件与职责

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleStandardHitRestResolver.cs`：allocation-free single truth table。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestPureResolverEditorTests.cs`：21个边界与无分配断言。
- 两个对应`.meta`及本Record/Task/Ledger/STATE/handoff/总表/manifest。

## 未关闭项与回滚

actual `BattleDamageWriter`与`BattleEcsHitExecutionPlan`仍使用各自旧公式；下一包统一接线。回滚删除新增resolver与test及对应meta即可，不影响已验证carrier或当前生产行为。
