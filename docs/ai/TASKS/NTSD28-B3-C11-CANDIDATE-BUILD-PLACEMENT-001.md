# Task Contract — NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001

> 状态：`VERIFIED / C11-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHM-PENDING-B5`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C11`
> 依赖：`NTSD28-B3-C10-COLLISION-ACTION-SNAPSHOT-PLACEMENT-001 / VERIFIED`

## 目标

把Unity现有C11 candidate transaction的三个步骤——attacker-rest prelude、PairVRest、CandidateCollect——
整体从serial remainder后移动到C10后、serial前。本包只闭合placement和transaction连续性，不改候选几何、
rest递减/阻断、平台关系或kind分类算法。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（仅相邻C11断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`（仅相邻C11断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs`（仅相邻C11断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs`（仅相邻C11断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs`（仅相邻C11断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementPlayModeProbeEditor.cs`（仅更新C11 rest时点断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧phase断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改`BruteForceSceneQuery`、RuntimeRestStore、candidate geometry、rest数值、platform/kind规则、C12 fusion、
Scene/Prefab/Config/资源或authority目录。

## 不变量

- production连续顺序为C10 snapshot→C11 rest prelude→PairVRest→CandidateCollect→serial remainder。
- C11三个步骤之间不得插入serial、presentation或其他全局pass。
- direct入口语义不变；full occurrence仍31，input-clear partial仍4。
- 算法等价性仍归B5；下一结构首差推进到C12 fusion相对serial remainder。

## 验收

1. test-first证明serial观察到C11 rest/pair已完成，并验证phase连续性。
2. compile0；C04～C11、candidate/rest/collision/frame/worker回归和SelfCheck通过。
3. 真实Play定向probe通过；cleanup、Scene unchanged、Play退出、Console0。

## 回滚

恢复C10→serial→rest/PairVRest/CandidateCollect顺序及相邻断言；不回退C01～C10。

## 执行结果

- test-first `2/2`预期失败：serial仍观察rest3，phase 11仍为FrameAdvance。
- production已调整为C10→rest prelude→PairVRest→CandidateCollect→serial；full/partial保持31/4。
- C11 focused `2/2` PASS，job `306a2a4fc9c14d6e921d566ef1f1d3b2`；C04～C11/actual
  `34/34` PASS，job `cd80786752674c78be22af533f966e33`；rest/collision/empty-candidate/hit-witness
  `28/28` PASS，job `1fe8fe71354b4093abc2f390322ffad8`。
- Unity scripts compile error 0；完整SelfCheck `2026-09-05 03:08:13 +08:00` PASS。
- 真实Play tick6：serial观察AttackExempt/Arest均0，PairVRest visit=5，cleanup PASS；结果SHA-256
  `3C43ADC9E2FA7D012D2AFDEE27F7C39010828C8A9C566FC90572D5E1493D0C5F`。
- Scene SHA保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  Play已退出、Console error 0；下一结构首差为C12 fusion相对serial remainder。
