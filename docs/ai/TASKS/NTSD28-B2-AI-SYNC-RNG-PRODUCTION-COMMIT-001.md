# Task Contract — NTSD28-B2-AI-SYNC-RNG-PRODUCTION-COMMIT-001

> 状态：`FOCUSED_TEST_PASS / PRODUCTION_SYNC_COMMIT_READY / LEGACY_RNG_ISOLATED / JOINT_TRACE_PENDING`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

把已经覆盖40个可能 live ID 的 synchronized AI candidate 接入 production `IndexedCanonical`
事务：每个 AI 从 `world.NativeRandom` 捕获一个候选 cursor，只有最终被接受且通过预提交验证的
canonical witness 提交一次；Full oracle、shadow、失败候选和 fallback不得提交。accepted sync commit
不得再把 legacy `world.Rng`恢复为 witness 的占位状态。

## Authority 与架构边界

- Authority `step_main`内全部 `synchronized_next`共享 BattleWorld synchronized stream，按升序 AI
  producer实际执行顺序连续推进；CRT stream独立。
- Unity唯一允许提交边界是
  `SimulationAiDecisionModule.TryPrepareIndexedCanonical→ValidateIndexedCanonicalCommit→BattleAiInputWriter`。
- `SharedSnapshot.CopyOwnedFrom(IndexedSnapshot)`只为Full oracle复制同一 origin cursor；oracle witness
  仅比较，不提交。
- legacy fallback继续显式使用旧 `world.Rng`，但失败前的 synchronized candidate必须对
  `world.NativeRandom`零副作用。
- 同 generation的两个cursor也可能过期；commit必须同时验证table/generation和捕获时的
  counter/index/calls/last-site origin，禁止later stale overwrite。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Core/NTSD28NativeRandom.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiDecisionSnapshot.cs`
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleAiInputWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeRandomEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs`
- 本 Task、Change Record、Ledger、STATE、handoff、总表与 B2 AI RNG manifest

禁止修改 Config/DAT/Scene/Prefab/ProjectSettings、authority、AI site逻辑、native input producer/combo
action、B0 exporter或joint trace格式。

## 不变量

- capture/evaluate/oracle/validate失败均保持NativeRandom原状态；只有accepted writer提交。
- writer在写input/flow前先验证并提交cursor；unexpected stale/missing cursor hard-fail，不允许部分input写入后fallback。
- accepted synchronized path保持legacy `world.Rng` state/calls不变；legacy fallback仍可正常推进它。
- reusable snapshot每次普通Populate先清除旧cursor，只有indexed production捕获后显式重新设置。
- Full/Indexed oracle比较必须包含call-site ID以及bound/raw/value/order/count。

## 验收

- test-first取得：accepted path NativeRandom未推进/legacy RNG被错误改写、旧profile parity断言、
  oracle未推进、same-generation stale cursor被接受等预期红灯；
- focused覆盖accepted唯一提交、precommit失败discard+fallback、oracle不double-commit、shadow不commit、
  same-generation stale reject、snapshot stale cursor清除和zero-allocation；
- 全AI回归、compile0、SelfCheck、Console0、Ledger PASS；
- 不把本包扩大为native input producer迁移或B2 joint parity完成。

## 回滚

移除production cursor传递/提交，恢复IndexedCanonical使用legacy RNG；保留candidate helper时其生产入口重新
变为未连接。cursor origin字段可随包整体回滚。

## 结果

- production `IndexedCanonical`现在为每个AI捕获`NativeRandom` synchronized cursor；writer先验证并
  唯一提交accepted witness，再发布input/flow。same-generation stale origin被拒绝。
- accepted synchronized路径不再恢复legacy `world.Rng`；precommit failure仍完整回退legacy，且对
  NativeRandom零副作用。
- Full oracle复制同一origin cursor但不提交；DeepShadow/SharedShadow均有前后scalar不变断言；普通
  reusable snapshot populate会清除旧cursor。
- oracle严格比较call-site/bound/raw/value/order/count。
- test-first job `929951ee02b64d2895f7207e2eefbc9d`：81项中5个预期失败、76通过；最终
  `8b17731394bf42709cd42ab2ca5c794d` 81/81、全AI
  `ab5cb12975c441bbba899c34c26fea27` 362/362、B2 broad
  `5a432d977a0a4eeea8ec69c81fb40e9b` 234/234；补强后DeepShadow类
  `6419df7677354cc197c0c50b9257ce29` 69/69、NativeRandom类
  `b5305763fc1c4504a56cb290eb5a0dca` 12/12。
- Unity compile 0；2026-09-03 11:36:54 full SelfCheck PASS；清理后Console 0 error；
  Change Ledger 128 records / 76 governed code files PASS。
- 本包没有完成native input producer迁移或同seed/input/tick联合trace，B2仍为进行中。
