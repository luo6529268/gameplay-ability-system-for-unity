# R7-TEST-001 — AI shared-shadow static frame sentinel isolation

> 日期：2026-08-23
> 状态：`VERIFIED / TEST-ONLY`

## Goal

关闭 `D-TEST-001`：防止
`AiDecisionSoAShadowEditorTests.SharedShadow_BuildsOnceAndRefreshesLowSlotBeforeHighSlotEvaluation`
把 `LF2FrameCache` 的进程级 `EmptyFrame.state` 留为14，污染同域后续 R3-INP-01 self-check。

## Scope

- 在该单一测试内保存共享empty-frame sentinel的原始state；
- 修正`UnifiedAuthority_AscendingRefreshMakesLowVisibleToHighWithoutReverseEarlyVisibility`对前一测试
  static污染的隐藏依赖：使用现有character-input mutation override触发正式post-input full refresh；
- 保留现有shared-row post-legacy state14 mutation与全部断言；
- 使用`finally`恢复原值，即使断言失败也清理；
- 不修改production AI、frame cache、character input、OID7/8/51或scheduler。

## Authority / Evidence

- 这是test isolation治理，不以Unity测试定义C++ gameplay；
- fresh domain下该单测1/1 PASS后，同域`BattleRuntimeSelfCheck`于02:52:32稳定失败在R3-INP-01；
- 同类其余65 cases分组、其他decision classes、shadow 9/9、stress 254/254之后的同域self-check均可PASS；
- source chain：test hook写`entity.Frame.D.state=14`；该用例AI切到missing frame，`Frame.D`落到
  `LF2FrameCache.EmptyFrame`静态sentinel；无finally恢复。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs`
- 本Task/Record/Ledger/STATE/diff register/handoff与主计划。

## Deliverables

1. 单测本地try/finally sentinel restoration；
2. dependent unified-refresh fixture可在fresh domain独立通过且自行清理；
3. owner exact test后同域full self-check PASS；
4. 完整AI 286 matrix后同域full self-check PASS（无需domain reload作为隔离手段）；
5. compile、validator与fresh-domain最终self-check证据。

## Verification

- owner exact 1/1 PASS；
- owner后同域self-check PASS；
- `AiDecisionSoAShadowEditorTests` 66/66 PASS，随后同域self-check PASS；
- AI matrix 286/286 PASS，随后同域self-check PASS；
- dotnet/Unity compile 0 error；
- ledger/scoped diff PASS。

## Stop conditions

- sentinel没有被该测试修改，或恢复后仍复现同一污染；
- 修复需要改变production frame fallback、AI decision或pass顺序；
- first difference转移到另一production模块。

## Out of scope

- 修改`LF2FrameCache.EmptyFrame`设计；
- R7-BROAD-02、容量合同、Play Mode、C++ trace与R8。

## Result

- fresh-domain二分从AI 286矩阵缩到单一owner；owner pre-fix 1/1后same-domain self-check在02:52:32 FAIL；
- owner加入finally后exact 1/1，same-domain self-check于02:57:57 PASS；
- owner cleanup暴露dependent unified-refresh fixture的隐藏污染依赖；pre-fix fresh exact FAIL；
- dependent改用character-input mutation override并自清理后exact job
  `b8e926eb862a4c4a83ed3124180f3267` 1/1 PASS；
- class job `4a05c94370434bddbd1e2afc38425c9e` 66/66 PASS，same-domain self-check 03:03:54 PASS；
- 原始AI matrix job `c90b67cf6eb740dfb2ed2715f56dbaf4` 286/286 PASS，same-domain self-check
  03:06:15 PASS；
- final fresh-domain compile Console 0 error，03:07:32 self-check PASS；dotnet 0 error；production未修改。
