# Task Contract — NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001

> 状态：`VERIFIED / C02-C03-PRODUCTION-PLACED / REAL-PLAY-PASS`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C02-C03`
> 依赖：`NTSD28-B3-PRODUCER-SCAN-COOLDOWN-BOUNDARY-AUDIT-001 / VERIFIED`

## 目标

关闭C01后的首个production顺序差：在正常同步与worker tick中，把human controller poll、升序live-slot的
non-character positive `hit_Fa`与character producer/sample、第二遍proxy/action route全部放到Cooldown前；
后续旧FrameLogic phase不再重复执行non-character `hit_Fa`。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C02C03ProductionPlacementEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅更新被新权威顺序取代的`FW-FLOW-01`断言）

不修改Cooldown内部writer、frame/physics算法、OID51/52 maintenance、collision/hit、Scene/Prefab/Config、资源、
ProjectSettings、Packages或authority目录。

## 不变量

- 保留`CharacterInputAll`作为既有character-only direct/test入口；production使用新入口，避免静默扩大历史调用者。
- production第一遍按runtime slot动态升序：non-character `hit_Fa`和character producer在同一scan中交错；第二遍
  只做character proxy/route。
- hit_Fa每tick最多执行一次；高slot newborn可见，pending-destroy跳过；同/低slot立即复用差异继续披露给B7。
- `NeedClearInput` partial、step-wait与results gate的执行/返回条件不变，只允许其已执行phase相对Cooldown的位置调整。
- 同步与worker共享`NTSDBattleTickSystem.RunTick`，不得分叉实现。

## 验收

1. test-first把full/partial actual sequence更新为新顺序，并新增production入口non-character执行一次、direct
   `CharacterInputAll`仍不执行non-character的断言；旧代码预期红灯。
2. full tick首序列为`BattleFlow→NativeSparkAdvance→HumanInput→CharacterInput→Cooldown`，不再出现
   `FrameLogic`；partial保持6 occurrences且HumanInput在Cooldown前。
3. focused、character-input/AI相关回归、worker相关回归、compile、SelfCheck通过；运行时行为改变需补真实Play
   或明确RUNTIME_PENDING。
4. 更新Ledger、STATE、handoff、总表与B3 manifest；下一首差如实记录。

## 回滚

恢复TickSystem原phase位置、production调用回`CharacterInputAll`，移除新增production wrapper/test；不回退C01、
57项contract或用户晋升的新权威。
