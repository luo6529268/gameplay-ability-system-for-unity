# NTSD28-B3-C05-NATIVE-TELEPORT-PRODUCTION-001 — C05原生teleport production

<!-- CHANGE-RECORD
id: NTSD28-B3-C05-NATIVE-TELEPORT-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/EarlyFrameAdvance/BattleEarlyFrameAdvanceModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/EarlyFrameAdvanceOptimizationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C05NativeTeleportPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan C05 resolve_native_teleport_state after C04 and before C06, EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-7-COMPILE-ERRORS / STATE400-401-ONLY / FRAME-TOGGLE-REMOVED-FROM-PRODUCTION / SELF-EXCLUDED / COLLISION-Y-AND-PRECISE-SYNC / STATE500-501-PRODUCTION-ISOLATED / DIRECT-COMPAT-PRESERVED / UNITY-COMPILE-0 / FOCUSED-11-OF-11-JOB-4DAC5147 / EARLY-PROXY-VALUE-COMPARER-CORRECTED / RELATED-209-OF-209-JOB-F817AF0D / SELFCHECK-2026-09-05T01-01-47-PASS / REAL-PLAY-KIND0-NOOP-PASS / NOOP-RESULT-SHA-FE98D718 / TARGETED-STATE400-401-PRODUCTION-PLAY-PASS / TARGETED-RESULT-SHA-02016B53 / CLEANUP-PASS / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-30-PARTIAL-4 / NEXT-C06 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / TARGETED-STATE400-401-PLAY-PASS / NEXT-C06`

## 改前事实

production调用combined EarlyFrameAdvance：teleport受FrameToggle隔tickgate，未严格排除self，Y固定0；同一入口还
运行当前playable无对应的state500/501 transform。

## 验证

- test-first：7条预期缺失`NativeTeleport` phase/seam编译错误；实现后Unity compile0。
- focused state400/401与actual phase `11/11` PASS，job`4dac5147173a4f21a0fc966292858791`。
- 扩大回归首次4项失败均为旧EarlyFrame comparer对`NativeInputProxyBlock`做引用相等；改为正式0x21-byte
  值比较后，early/direct state500/501、C04/C05、frame/input/OID/worker相关`209/209` PASS，job
  `f817af0d2dcd499ebe2c06296309af0c`。
- 完整SelfCheck `2026-09-05 01:01:47 +08:00` PASS。
- 真实kind0非teleport Play tick3～6 PASS，证明C05在普通战斗为no-op；结果SHA-256
  `FE98D71880EFE4E0F1CC6FBAE4E02360F096780C81C7C9DA8426E3A7CE1016F8`。Scene unchanged、
  Play退出、Console0。
- state400/401定向probe经完整production `SimulationTickDriver`两tick通过：state400从FrameToggle=0入口得到
  `(180,-31,131)`且三轴清零；state401在隔离battle-group内选中最远同队者，目标经C04边界后的Z为446，
  结果为`(560,-29,447)`且三轴清零。`startTick=1/endTick=2`、临时slot 50/51、cleanup PASS。
- probe开发期间先后暴露两处test-only假设：用`LF2OtherObject`伪装type0不会进入character候选；把目标Z=500
  写死为C05输入忽略了更早C04边界会夹到446。均只修正夹具，production代码未因失败改动。
- 最终结果SHA-256 `02016B53240EFE3A195063DB66AF14A18F282BBFB358B9F31ECF17D3934C8818`；
  Scene SHA-256保持`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`，Play退出，
  最终Console error 0。

## 未关闭边界

- C06 nested physics与完整C04仍是后续包。
