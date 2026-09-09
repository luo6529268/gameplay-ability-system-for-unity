# Task Contract — NTSD28-B3-C05-NATIVE-TELEPORT-PRODUCTION-001

> 状态：`VERIFIED / TARGETED-STATE400-401-PLAY-PASS / NEXT-C06`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C05`
> 依赖：`NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001 / VERIFIED`

## 目标

在显式C04后、C06前建立只包含修复版权威state400/401的production C05 teleport：每tick升序slot扫描，
排除source自身，按current DAT type0/HP/battle-group选择最近敌或最远友，复制collision-Y、同步整数/精确坐标并
清三轴。旧`EarlyFrameAdvanceSpecialsAll`保留direct compatibility，但production不再运行其FrameToggle gate或
state500/501额外transform。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Simulation/Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/EarlyFrameAdvanceOptimizationEditorTests.cs`（仅修正proxy值比较器）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportPlayModeProbeEditor.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅旧production断言纠正）
- 本Task/Record、Ledger、STATE、handoff、manifest与总表。

不删除旧direct transform实现，不修改C04完整字段、C06 physics、Scene/Prefab/Config/资源或authority目录。

## 不变量

- production C05不看`FrameToggle`；state400/401每个完整tick均检查。
- candidate排除source，要求current data type0、HP>0；strict距离比较保留低slot tie。
- state400最近敌初值10000，state401最远友初值-1；relation使用已绑定的`RelationTeam` battle-group。
- 有target时Y取target collision reference（Unity载体`PS.groundY`），无target时取source自身；X/Z按整数镜像，
  再同步double坐标并清`Vx/Vy/Vz`。
- production不调用state500/501 transform；direct `EarlyFrameAdvanceSpecialsAll`原行为和测试保留。
- input-clear partial仍不进入C05；full occurrence数不因rename变化。

## 验收

1. test-first覆盖odd/even cadence、self exclusion、collision-Y、nearest/farthest、non-character source、
   no-target precise sync、legacy-extra isolation和phase位置。
2. compile0；focused、early/frame/worker相关回归、SelfCheck通过。
3. 真实kind0 no-op Play与state400/401定向production Play通过；Scene unchanged、Play退出、Console0。
4. 下一首差推进到C06 nested physics/Unity SerialTick。

## 回滚

恢复production调用combined EarlyFrameAdvance，移除native teleport seam与新tests；不回退C01～C04和OID包。
