# Task Contract — NTSD28-B3-C07-REVIVAL-PRODUCTION-PLACEMENT-001

> 状态：`VERIFIED / C07-PLACEMENT / HIGH-SLOT-DOWNSTREAM-VISIBLE / BEHAVIOR-PENDING-B4-B7-B9`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C07`
> 依赖：`NTSD28-B3-C06-NESTED-PHYSICS-PRODUCTION-001 / VERIFIED`

## 目标

把Unity现有`BattleRespawnModule.RunPostFrameAdvanceDeathCleanup`的production唯一调用从serial remainder后移动到
C06完整结束后、serial remainder/geometry前，并将phase语义改为`Revival`。本包只闭合C07 placement、single owner
和新生高slot对后续阶段可见性；现有复活分支算法原样保留，不能据此宣称C07行为完全对齐。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`（仅调用名/兼容seam必要调整）
- `Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs`（仅调用名/兼容seam必要调整）
- `Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs`（仅production placement观测seam必要调整）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（仅更新相邻C07 phase断言）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalPlacementPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧phase/production断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改revival gate/数值/RNG/floor公式、主角色terminal策略、continuation controller/group、OID998内容、C08～C26、
Scene/Prefab/Config/资源或authority目录。

## 不变量

- production顺序为C06完整结束→C07 Revival→现有serial remainder；不得保留旧serial后第二次调用。
- direct `PostFrameAdvanceDeathCleanupAll`兼容入口可保留，但production只调用一次。
- C07产生的高slot对象在后续serial与geometry阶段可见；现有deferred mutation差异仍归B7。
- full occurrence用`Revival`替换旧`DeathCleanup`，总数仍31；input-clear partial仍为4。
- 现有Unity复活算法与正式C07之间的行为差异继续明确标为B4/B7/B9 pending。

## 验收

1. test-first覆盖phase顺序、production single owner、旧位置不再调用、direct兼容及高slot后续可见。
2. compile0；C06/respawn/frame/worker/snapshot相关回归和SelfCheck通过。
3. 真实Play复活定向probe通过；cleanup、Scene unchanged、Play退出、Console0。
4. actual phase首差推进为C08 stage-depth clamp对serial remainder。

## 回滚

恢复旧`FrameAdvance→DeathCleanup`production调用和phase名，移除C07 placement tests；不回退C01～C06。

## 执行结果

- red compile：缺失`BattleTickPhase.Revival` 1项；实现后compile error 0。
- focused C07 `3/3` PASS，job `b834f881579c4c8d8cb068ceb1b88154`。
- 首轮C04～C07/phase为24/25，唯一失败是C06旧相邻断言仍要求C06后直接serial；按新权威更新为
  C06→C07→serial后，final `25/25` PASS，job `6c8e2febf98b441da0b844568a5570f2`。
- 首次扩大43项中6个`PreInteractionNoOpProof`失败均为既有`NativeInputProxyBlock`引用相等比较器；与C07
  无关且未扩大范围修改。W05 lifecycle+worker单独重跑`28/28` PASS，job
  `50a4f99ebeac4b3797ef9327691f92d2`。
- 完整SelfCheck `2026-09-05 01:59:07 +08:00` PASS。
- 真实production Play：C07调用1次，slot50 continuation生成slot51；新生slot同tick进入serial remainder 1次，
  且未补跑C06，cleanup PASS。结果SHA-256
  `185F9CBD01B50A0DBBF240E0690BE1318D0C6B7AC0B5FEEFE875D2406EBBF6B1`。
- Scene SHA-256保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  Play退出，最终Console error 0；full/partial仍31/4，下一结构首差C08 stage-depth clamp对serial remainder。
