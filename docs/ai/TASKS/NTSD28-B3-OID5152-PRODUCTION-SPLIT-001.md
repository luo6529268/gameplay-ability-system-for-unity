# Task Contract — NTSD28-B3-OID5152-PRODUCTION-SPLIT-001

> 状态：`VERIFIED / C12-FUSION / C25H-TIMER / REAL-PLAY-PASS`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C12+C25h`
> 依赖：`NTSD28-B3-OID5152-MAINTENANCE-PLACEMENT-AUDIT-001 / VERIFIED`

## 目标

把 Unity 生产路径中合并的 OID 7/8/51 maintenance 拆回修复版权威的两个 owner：

1. C12 在 candidate build 后、type-0 hit consume 前执行 fusion scan，并读取本 tick 尚未递减的
   `Unk338`；
2. C25h 在每个 live slot 的 frame step 后递减正值 `Unk338`；
3. `Oid5152RuntimeMaintenanceAll` 继续保留原“先递减、再 merge/split”的 direct compatibility 语义，
   不再由 production tick 调用。

完成后 input-clear partial 不再推进 fusion 或 `Unk338`，full tick 的下一项首差应回到
`CoreFrameMotion / EarlyFrameAdvance`。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Oid5152ProductionSplitEditorTests.cs`（新增及 meta）
- `Assets/NTSD/Scripts/Test/Editor/BattleOid5152MergeSplitPlayModeProbeEditor.cs`（仅更新生产时点断言）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅在既有 production 顺序断言需要纠正时）
- 本 Task/Record、Ledger、STATE、handoff、B3 manifest 与对齐总表。

不修改 merge/split 算法、正式内容、Scene/Prefab/Config/资源/ProjectSettings/Packages 或 authority 目录。

## 不变量

- C12 fusion scan 遍历 slot 0..19，保留现有 mutation、dormant、stable handle 与 reset 语义。
- C12 不递减 `Unk338`：timer=1 的实体本 tick 不得 merge/split，C25h 后变为 0，下 tick C12 才触发。
- C12 新 merge 写 4500 后，同 tick C25h 写 4499；C12 split 写 900 后，同 tick C25h 写 899。
- C25h 必须是 per-slot、frame 后 writer；slot 没进入该 tail 时不得推进。
- input-clear partial 不进入 C12/C25h；direct compatibility 入口的既有 timer=1 同调用触发与最终
  4500/900 断言保持不变。
- 不改变 Unity Slot 容量用户例外，也不扩大到 FrameMotion、完整 C25 tail 或 fusion 数据策略。

## 验收

1. test-first 红灯覆盖 C12 位置、partial 不推进、timer=1 延后一 tick、C25h frame 后递减和 direct 兼容。
2. Unity compile 0；focused、OID/lifecycle/phase/worker 相关 tests 与 SelfCheck 通过。
3. 真实 OID 7/8/51 Play probe 按新 production 时点通过；Scene unchanged、Play 退出、Console 0。
4. Ledger validator 与 scoped `git diff --check` 通过；更新所有恢复入口及下一首差。

## 回滚

恢复 production 的 combined maintenance 调用，移除 C12 fusion 与 C25h timer seam 和新 tests；不回退
新权威、C01～C03 或 cooldown writer extraction。
