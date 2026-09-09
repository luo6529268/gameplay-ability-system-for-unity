# NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_NEGATIVE_RELATION_PRESERVE
code-path: Assets/NTSD/Scripts/Simulation/Passes/Interaction/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationQueryAndLinkModuleEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: NTSD 2.8-Logan BattleWorld28::settle_held_refill_objects invalid parent/reciprocal branch; battle_world.cpp EB37E8EC; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-2-OF-2 / FOCUSED-7-OF-7 / B6-CATEGORY-22-OF-22 / RELATED-47-OF-47 / BROAD-56-OF-62-WITH-6-UNRELATED-NATIVEINPUTPROXY / TARGETED-PLAY-7-CASES / BUILDS-0-ERROR / SELFCHECK-BLOCKED-UNRELATED-HELD-ACCOUNTING / CONSOLE-0-ERROR / SCENE-BASELINE-RESTORED / DIAGNOSTIC-PRESERVE
-->

> 状态：`VERIFIED / RED_2_OF_2 / FOCUSED_7_OF_7 / B6_CATEGORY_22_OF_22 / RELATED_47_OF_47 / BROAD_56_OF_62_6_UNRELATED_NATIVE_INPUT_PROXY / TARGETED_PLAY_7_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_HELD_ACCOUNTING / CONSOLE_0_ERROR / SCENE_BASELINE_RESTORED / DIAGNOSTIC_PRESERVE`

## 实施结果

- `SimulationQueryAndLinkModule.HeldObjectProcessAll()`现在每次C09/C20扫描都重置last-pass计数；negative child
  的parent out-of-range、inactive或reciprocal target mismatch只增加累计/last-pass失败计数并continue，
  不再写`LinkState`或调用`RefreshRuntimeSnapshot()`。
- diagnostic sink存在时写`negative-held-validation/link-validation`事件，记录child slot、Link、parent slot、
  observed parent target、generation epoch、`preserved` outcome及三种reason；sink关闭时不materialize事件。
- `SimulationWorld`只暴露只读累计与last-pass诊断属性。没有新增snapshot/checksum carrier或运行规则状态。
- positive-link validation未修改；前置lifecycle cleanup后的正常parent removal/reuse把child置为0/0，因此不会
  进入本invalid分支，也不会产生ABA或失败计数。

## 验证记录

- behavioral RED job `cb3304b568644633b230d6e1a1ffbb41`：`0/2`，两项实际都得到旧
  `LinkState=0`，预期分别为`-1/-2`。
- GREEN focused job `bde6acc7c08b40ec85289e07919cf75c`：`7/7`。覆盖C09→C20、out-of-range、
  inactive、mismatch、slot0、extended slot511、lifecycle cleanup/reuse、双方sentinel/RNG、三类trace reason
  与sink-off warmed 0B。
- 最终B6 category job `e6477884bb45498c8f68ad1525bd627b`：`22/22`。
- clean adjacent job `118517a12f7944968151f45e04b29ebb`：`47/47`，覆盖positive-link、pending
  destroy、slot lifecycle、pooled reuse与structural witness。
- 首次较宽job `472606481f5b4fa68fe7b9831e6c297a`：`56/62`；仅6项失败均来自既有
  `PreInteractionNoOpProofEditorTests`把两个内容相同但不同实例的`NativeInputProxy`作对象引用相等比较，
  与本包held/link字段无关。本包未修改该独立夹具。
- 真实`NTSD_Battle` Play runner为`7` cases通过：missing/mismatch preserve、slot0/high、lifecycle、trace、
  RNG零变化与warmed sink-off 0B均通过。退出Play后Console error=0。
- fresh Runtime/Editor `dotnet build --no-restore`均0 error，既有warning为47/129；Unity 2022.3.62f3
  程序集刷新后Console无编译错误。
- full `BattleRuntimeSelfCheck`已跨过更新后的negative-held检查，下一首差仍是独立held-CPoint injury legacy
  accounting (`BattleRuntimeSelfCheck.cs:11403`)。
- compile/domain reload曾把用户Scene中三个Capsule active1自动保存为0；已只恢复这三个精确值，并以进入
  包前SHA-256 `89AA621640A3818F2C7CD307836C50D72CAB84B6D4F2B6DB333FF7CBF525A673`、
  长度216762确认。最终Scene `isDirty=false`、rootCount16；本包无Scene交付。
- `git diff --check`通过；`Tools/Validate-ChangeLedger.ps1`通过（423 records / 356 governed code files）。

## 未关闭项

下一严格包为`NTSD28-B6-CATCH-RELATION-EXACT-FIELDS-PRODUCTION-001`；之后依次是mixed catch advance、
settlement vaction preflight与held accounting。positive-link retirement、carrier/schema和content继续独立。

## 回滚

反向恢复本Change的invalid handler、diagnostics和测试；不得回退前置lifecycle cleanup或用户文件。
