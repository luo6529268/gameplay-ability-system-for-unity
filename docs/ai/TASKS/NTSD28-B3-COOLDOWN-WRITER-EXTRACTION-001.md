# Task Contract — NTSD28-B3-COOLDOWN-WRITER-EXTRACTION-001

> 状态：`VERIFIED / EARLY-COOLDOWN-REMOVED / C11-C25J-OWNERS / REAL-PLAY-PASS`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C04-FIRST-DIFF`
> 依赖：`NTSD28-B3-C02-C03-PRODUCTION-PLACEMENT-001 / VERIFIED`

## 目标

移除C03后的Unity早期`Cooldown` production phase，把其两个writer交还新权威实际owner：

1. current-frame无Itr或state1001 parent WPoint停止attacking时清attacker rest，移入C11 candidate prelude；
2. frame/state处理后的attacker-rest decrement继续由late frame tick的canonical `AttackExempt` writer完成，并在
   同一点同步Unity兼容镜像`ItrRest.Arest`。

完成后full tick不再出现Cooldown occurrence；下一首差预期为`CoreFrameMotion / RuntimeMaintenance`。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28CooldownWriterExtractionEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28NativeSparkC01IntegrationEditorTests.cs`（仅更新C01后下一phase断言）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅受新owner影响的rest顺序断言）

不删除`BattleEcsCooldownPass`兼容/diagnostic实现，不修改hit damage、frame算法、OID maintenance、Scene/Prefab/
Config/资源/ProjectSettings/Packages或authority目录。

## 不变量

- `AttackExempt`是authority attacker_rest的canonical Unity绑定；`ItrRest.Arest`只作为既有consumer兼容镜像，
  production candidate与tail边界后必须相等。
- clear发生在collision snapshot/candidate geometry前，不在input前；decrement发生在每entity frame tick后。
- frame tick因hold/relation/suppress gate未递减时，镜像不得自行递减。
- direct `BattleEcsCooldownPass`测试仍可覆盖legacy/shadow/data-oriented seam，但production不再调用。
- partial input-clear不进入frame/candidate，因此不应偷偷递减rest。

## 验收

1. test-first：actual full/partial count、candidate clear双字段、late-frame mirror sync在旧代码上失败。
2. compile0；focused、cooldown/frame/collision/worker相关回归、SelfCheck通过。
3. 真实Play kind0 probe通过；Scene unchanged，Play退出，Console0。
4. 更新manifest/Ledger/STATE/handoff/总表和下一首差。

## 回滚

恢复TickSystem早期Cooldown调用，移除candidate/tail镜像owner与新tests；不回退C01/C02/C03或新权威。
