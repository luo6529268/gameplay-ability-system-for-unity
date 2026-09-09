# Task Contract — NTSD28-B3-LEGACY-STATE501-TRANSFORM-RETIREMENT-001

> 状态：`VERIFIED / RED_0_OF_1 / PRODUCTION_BRANCH_REMOVED / FOCUSED_1_OF_1 / EARLY_M2_11_OF_11 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED`
> 依赖：`NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001 / VERIFIED`

## 目标

原子退休Unity early-frame中Authority不存在的`state==501` self/owned-child definition mutation。501实体与任何
`KillCount` marker child经过early-frame pass后必须保持identity、definition、frame与runtime不变。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/EarlyFrameAdvanceOptimizationEditorTests.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- `Assets/NTSD/Scripts/Test/Editor/BattleRuntimeSelfCheckEditor.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B3State501RetirementEditorTests.cs`及`.meta`
- 本Task/Change、Ledger、STATE、handoff与总表。

## 不变量

- state500、C05 native teleport、8000..8999 definition transition行为与diagnostics不变。
- 不改CPoint `throwinjury==-1` transform、11xx/12xx lifecycle、`LF2States.RudolfTransform`常量、
  `TransformOriginalObjectId/TransformTargetObjectId`、KillCount carrier或snapshot/schema。
- 不改content、Scene、Prefab、ProjectSettings、Authority、RNG或pass顺序。

## Test-first与验收

1. RED：state501 self带有效replacement、slot-marker child、stable-id-marker child、dead child与missing replacement；
   断言early-frame后所有identity/definition/frame/runtime均不变，且fast/forced-legacy一致。
2. 删除state501 handle list、validation、dispatch和mutation方法；保持state500 handle fast path。
3. 修正旧focused/SelfCheck夹具，不再把Unity-only mutation当基线；运行目标focused、early-frame相关回归、build、
   SelfCheck和目标Play。若full SelfCheck仍被无关CPoint阻塞，须明确记录。
4. Scene dirty/hash、Console error与Change Ledger validator均须检查。

## 回滚

反向恢复本Change删除的state501 early-frame dispatch/mutation及对应旧夹具；不得回退state500、teleport或其他任务修改。

## 实施与验证（2026-09-09）

- 先把既有fast/forced-legacy state501 fixture改为逐实体保存canonical runtime、definition wrapper、ObjectId与
  frame并要求执行后全部不变；新增专项请求/Play runner。02:06:08 +08 RED为`0/1`，首差明确为child
  `ObjectId expected 31 / actual 9000`，证明测试穿过旧生产mutation。
- production只从`BattleEarlyFrameAdvanceModule`删除`state501Handles`、capacity/clear/validation、fast/fallback/
  forced-legacy dispatch及`RunState501Special(s)`变身体。state500 handle path、native teleport和模块外逻辑未改。
- 旧`EarlyFrameAdvanceOptimizationEditorTests`现在要求fast与forced-legacy两组共10个实体的canonical runtime
  全字段、definition引用、ObjectId与frame保持不变；SelfCheck GT-04也改为拒绝self/child identity/definition mutation。
- fresh builds：`Assembly-CSharp.csproj`为0 error/47 warning，`Assembly-CSharp-Editor.csproj`为
  0 error/104 warning。
- 02:10:23 +08 Unity专项focused `1/1`通过；02:11:05 +08 M2 focused为`11/11`
  （architecture=4、early=6、flow=1），同时保护state500、teleport与模块边界。
- 02:11:48 +08真实`NTSD_Battle` Play直接执行同一专项并通过：fast/legacy=true、entitiesUnchanged=10。
  Play前后active Scene均dirty=false/root=14，文件SHA-256保持
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`；清空后的Console error=0。
- 02:12:48 +08 fresh full SelfCheck仍提前停在独立既有CPoint mode0 victim-Vz断言，尚未到GT-04；
  不把该阻塞写成通过，也不影响已直接执行的专项/M2/Play证据。
- production grep确认early-frame module已无`state501Handles`、`RunState501Special`或501判断；CPoint
  `PropagateCpointThrowTransformToOwnedObjects`仍保留，11xx/12xx package未触碰。
- 本包只关闭early-frame state501 transform；不声明完整B3或full parity。严格下一包为
  `NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001`。
