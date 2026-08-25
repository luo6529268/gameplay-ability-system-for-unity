# R3-COMBO-01 — staggered combo-state persistence

> 日期：2026-08-23
> Change ID：`R3-COMBO-001`
> 状态：`VERIFIED / SOURCE + UNITY RUNTIME`

## Goal

使Unity九组combo wrapper像C++ release一样逐wrapper、逐逻辑tick即时持久化状态，从而恢复真实顺序输入
（例如Naruto物理L→S→K）的多tick技能触发，同时保留现有帧同步、worker与0-GC边界。

## Scope

- `BattleCharacterInputActionResolver.ApplyComboFrameInput`的九combo字段ownership；
- 修正`BattleRuntimeSelfCheck`中把transactional discard当权威的陈旧断言；
- 新增focused Editor matrix，优先独立覆盖跨tick状态1/2/3与early branches；
- 更新D-INP-010、Ledger、STATE与handoff。

## Authority / evidence

- `J:\QQFile\NTSD2.4\ntsd_release\include\input_handler.h:9-16`；
- `src/input/input_handler.cpp:1555-1609,2758-2859`；
- `Makefile:35` release参与性；
- `RESEARCH/R3-COMBO-01-staggered-combo-state-persistence-preflight-20260823.md`；
- 用户current-worktree Play Mode组合键失败报告。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleCharacterInputActionResolver.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleCharacterInputComboPersistenceEditorTests.cs`（如需新增）
- 对应`.meta`、Change Record、Ledger、STATE、diff register与handoff。

## Unknowns

- real Naruto DAT在当前场景的frame271→递归opoint完整表现仍属R8-WP01C，不能由本包focused test替代；
- missing target/guard/Unk328的后续direct action frame是否需要更新旧test具体期望，须按C++逐branch重算；
- full C++ runtime trace不可用。

## Deliverables

1. combo state不再依赖local transaction统一commit；
2. 八方向combo和DJA每一步/interrupt/trigger状态与C++ source一致；
3. 陈旧negative staggered Naruto断言改为source-derived positive sequence；
4. compile、focused、full self-check、real Play Mode证据分别记录。

## Verification

- static source crosswalk；
- exact focused Editor tests；
- full `BattleRuntimeSelfCheck`；
- full EditMode相关input regression；
- `NTSD_Battle`真实物理L→S→K及至少一组L→方向→J；
- validator与scoped diff；
- C++ full trace仍明确BLOCKED。

## Stop conditions

- 修复需要改变C++ wrapper顺序、combo window、physical mapping或pass order；
- first mismatch进入opoint/lifecycle/render等R8-WP01C/D；
- 需要修改C++ authority、DAT或长期架构；
- focused修复不能独立验收。

## Out of scope

- FrameInputSet/worker输入journal重构；
- AI decision、collision/hit、opoint具体技能、render、T8、Android、服务器。

## Blocker

`B-R3-COMBO-001-01`已解除：用户于2026-08-23明确回复“同意修改，继续处理”，批准实施
`R3-COMBO-001`。本包现仅可按既定最小范围修改resolver与source-conflicting测试；若first mismatch进入
opoint/lifecycle/render、physical mapping或pass order，仍须按Stop conditions停止并另立工作包。
