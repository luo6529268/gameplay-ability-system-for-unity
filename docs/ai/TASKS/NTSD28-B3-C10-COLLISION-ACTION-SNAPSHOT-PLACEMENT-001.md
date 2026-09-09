# Task Contract — NTSD28-B3-C10-COLLISION-ACTION-SNAPSHOT-PLACEMENT-001

> 状态：`VERIFIED / C10-SNAPSHOT-PLACEMENT / C11-REST-NOT-EARLY / TARGETED-PLAY-PASS`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C10`
> 依赖：`NTSD28-B3-C09-HELD-REFILL-PLACEMENT-001 / VERIFIED`

## 目标

把Unity碰撞动作快照从serial remainder后移动到C09后、serial前，对应authority C10 `snapshot_actions()`。
现有`CaptureCollisionFrameSnapshotsAll`还混合C11 attacker-rest prelude；本包必须拆出snapshot-only production入口，
并保持rest prelude在serial后、candidate前。只闭合字段冻结与placement，不实现C11 geometry算法。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（仅更新相邻C10断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`（仅更新相邻C10断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs`（仅更新相邻C10断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs`（仅相邻C10断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28CooldownWriterExtractionEditorTests.cs`（仅direct兼容断言必要调整）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧phase断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改snapshot字段定义/消费者、candidate geometry、pair-vrest算法、hit/catch、Scene/Prefab/Config/资源或authority目录。

## 不变量

- production顺序固定为C09 HeldProcess#1→C10 snapshot-only→serial remainder→C11 rest/candidate prelude。
- C10只冻结每个eligible live entity当前action到collision previous action；不得顺带执行rest clear。
- public `CaptureCollisionFrameSnapshotsAll`保持direct兼容：rest prelude+snapshot。
- full occurrence仍31；input-clear partial仍4。
- 下一结构首差应推进到C11 candidate/pass-shape相对serial remainder。

## 验收

1. test-first证明serial内frame变更不污染已冻结C10 action，并证明rest prelude未随C10提前。
2. compile0；C04～C10、collision snapshot/rest/frame/worker/snapshot回归和SelfCheck通过。
3. 真实Play定向probe通过；cleanup、Scene unchanged、Play退出、Console0。

## 回滚

恢复serial后combined `CaptureCollisionFrameSnapshotsAll` production调用，移除snapshot-only seam及测试；不回退C01～C09。

## 执行结果

- test-first `2/3`预期失败：冻结值为serial改后的5而不是0，phase 10为FrameAdvance；rest-not-early保护项已通过。
- 新增snapshot-only入口；public combined direct入口仍为rest prelude+snapshot。production改为C09→C10 snapshot→serial→rest prelude。
- C10 focused `3/3` PASS，job `9f551edced964e278da247eda45fa1bd`。C04～C10首次扩大因C06～C08旧相邻
  断言为29/32；更新后final `32/32` PASS，job `90de21fc13fd426280ba6368b12eb35b`。
- cooldown/collision snapshot/frame/worker/state snapshot `59/59` PASS，job
  `6871e19d9b7a497cb3e7118f1a9aee54`；Unity scripts compile error 0；完整SelfCheck
  `2026-09-05 02:51:04 +08:00` PASS。
- 真实Play tick6：当前frame5、C10 collision snapshot与Runtime镜像均0；serial观察AttackExempt3，C11后最终0；
  cleanup PASS。结果SHA-256 `D96A619A06CE542210F6BF75B002C90F3FB92FF65C22CB703252DB6222D54FA0`。
- Scene SHA保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  Play已退出、Console error 0；full/partial仍31/4。下一结构首差C11 candidate/pass-shape相对serial remainder。

> 后续修正：`NTSD28-B3-C11-CANDIDATE-BUILD-PLACEMENT-001`已将C11 rest/pair/candidate transaction整体
> 移到serial前；本节“rest暂留serial后”只代表C10关闭当时的阶段性状态，不再是当前production顺序。
