# Task Contract — NTSD28-B3-BUGFIXED-PASS-CONTRACT-REBASELINE-001

> 状态：`FOCUSED_TEST_PASS / 57-CHECKPOINT / PRODUCTION-UNCONNECTED`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3`
> 依赖：`GOVERNANCE-NTSD28-AUTHORITY-PROMOTION-002`

## 目标

以用户确认的Bug修复版EXE `B1E13AE1...9033`、82-file playable closure `39DDDA15...6109`及
75-file capture子闭包 `07CD47A0...778F`重新建立
immutable battle pass contract。修正旧52-checkpoint合同中已被新版规则取代的内容，避免后续生产重排继续
朝旧权威对齐。

## Authority变化

- physics loop内每个slot在`step_physics`后立即执行dead-character resource normalize，再到下一slot；不是一个
  可拆成两个全局scan的顺序。
- hit消费是type-0升序caller loop、random-drop、non-type-0升序caller loop三段，不是单一unified pass。
- horizontal impulse在第二次depth clamp、held refill和stage settlement之后。
- nested live-slot tail扩展为16步：definition、special clone、resource pre-display、display、resource post-display、
  computer state、frame、reaction、armor、rest、frame-zero opoint、state18 particle、previous action、weapon pieces、
  lifecycle、healing。
- combo expiry仍在整个nested tail后，Session post与completed snapshot handoff保持其后。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28BattlePassOrder.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattlePassOrderEditorTests.cs`
- 若只为索引断言适配，可修改
  `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`

不修改TickSystem实际执行顺序、World行为、Scene/Prefab/Config、资源或authority目录。

## 验收

1. 先把focused tests更新为新版57-checkpoint、physics nested pair、type-separated hit、impulse placement与16-step tail，
   在旧contract上取得预期失败。
2. 最小修改immutable contract使focused通过；所有ID与descriptor保持稳定索引映射和allocation-free query。
3. 运行编译、focused、相关actual-sequence tests和完整SelfCheck；如需要Play必须保持Scene unchanged。
4. 更新B3 manifest、总表、Ledger、STATE、handoff与Change Record，明确旧52项证据被本包supersede。

## 回滚

仅回滚本包三条允许代码路径中的contract/test修改，并恢复对应文档状态；不得恢复旧artifact为权威。
