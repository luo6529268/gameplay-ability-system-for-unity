# Task Contract — NTSD28-B3-C06-NESTED-PHYSICS-PRODUCTION-001

> 状态：`VERIFIED / C06-PRODUCTION-OWNER / TARGETED-PLAY-PASS / NEXT-C07`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C06`
> 依赖：`NTSD28-B3-C05-NATIVE-TELEPORT-PRODUCTION-001 / VERIFIED`

## 目标

在production C05后建立显式C06升序live-slot事务：每个slot先运行该实体当前已有的物理/落地入口，随后立刻按
修复版权威`FUN_004154E0`规则处理死亡current-DAT type0实体，再进入下一slot。normalizer只把
`effective_max_hp/current_mp`对应的Unity `HPBound/PP`写0；不得改`HP/HP3/MP/MPMax/PPMax`。

同时从后续`SerialTickAll` production路径移除重复物理owner；现有无参数/direct `SerialTickAll`保持兼容。
`LF2SpecialAttack`的state-entry/state15/death非物理尾仍留在后续serial入口，本包不把C25行为提前到C06。

## 允许代码路径

- `Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs`
- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs`（新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsPlayModeProbeEditor.cs`（如定向Play需要，新增及meta）
- `Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportPlayModeProbeEditor.cs`（仅改用新C06 test seam冻结后续物理）
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`（仅既有production顺序断言必要修正）
- 本Task/Record、Ledger、STATE、handoff、B3 manifest与总表。

不修改物理公式、gravity/landing常量、C04完整字段、C07 revival、C25 frame/state算法、Scene/Prefab/Config/资源或
authority目录。

## 不变量

- production顺序固定为C04全barrier→C05全barrier→C06逐slot嵌套`physics→normalize`。
- normalizer按current DAT type判断，不按CLR壳类型；仅`HP<=0`的type0适用。
- `HPBound=0`、`PP=0`；`HP`保留原负/零值，`HP3/MP/MPMax/PPMax`不变。
- 低slot normalize必须在高slot physics开始前可见；本tickC06以后才致死的实体要到下一tick C06归一化。
- production每实体物理只运行一次；direct `SerialTickAll`保留旧单入口兼容。
- input-clear partial不进入C06；本包不宣称C06内部物理算法已完成B4/B10对齐。

## 验收

1. test-first覆盖phase位置、逐slot嵌套可见性、type0死亡字段矩阵、living/non-type0排除、next-tick归一化、
   production single-owner与direct兼容。
2. compile0；focused及frame/physics/worker/snapshot相关回归、SelfCheck通过。
3. 真实kind0 Play和死亡type0定向production Play通过；cleanup、Scene unchanged、Play退出、Console0。
4. actual phase首差推进到C07 revival或后续尚未迁移边界，并如实记录完整C06物理算法仍归B4/B10。

## 回滚

恢复production `SerialTickAll`直接运行原SimTransit/SimTU，移除显式C06 phase/owner seam与新tests；不回退
C01～C05、cooldown或OID分包。

## 执行结果

- red compile捕获2项缺失override seam；实现后compile error 0。
- focused首轮5/6，唯一失败是synthetic CLR壳未走current-DAT catalog；夹具改为显式current-DAT override后
  `6/6` PASS，job `cbfbaf3636254388aedef9a02a6fd0b2`。
- C04～C06与actual phase `22/22`、NTSD28组`92/92`、frame/runtime`23/23`、worker`20/20`、
  physics/bounds/sound/weapon snapshot`22/22`均PASS。
- 完整SelfCheck `2026-09-05 01:38:19 +08:00` PASS。
- C06定向Play通过：高slot 51在自身physics入口观察到低slot 50已经`HPBound=0/PP=0`；低slot最终
  `HP=-5/HP3=500/PPMax=480/MP=17/MPMax=19`，cleanup PASS。结果SHA-256
  `50BA937F5FF8F983C7136D8B60156E92D823906181D0C629B949D8E04820D38B`。
- C05定向Play在新C06后复跑PASS；结果SHA-256
  `8064DFCED92494BBDAEF23342499890DE9ECEA6DE890851DB86CAA1D1B778C7B`。
- Scene SHA-256保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，
  Play退出，最终Console error 0；actual full/partial occurrence为31/4，下一结构首差C07 revival对serial remainder。
