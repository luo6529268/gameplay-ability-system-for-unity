# NTSD28-B3-C12-FUSION-BARRIER-PLACEMENT-001 — C12 fusion barrier placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C12-FUSION-BARRIER-PLACEMENT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C12 advance_native_fusions immediately after C11 before C13; existing Unity OID5152 fusion owner proven by NTSD28-B3-OID5152-PRODUCTION-SPLIT-001; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TEST-FIRST-RED-2 / UNITY-COMPILE-0 / FOCUSED-2-2 / C04-C12-ACTUAL-36-36 / OID-C12-6-6 / SELFCHECK-PASS-2026-09-05T03:47:14+08 / TARGETED-PLAY-PASS / RESULT-SHA256-FFAD6915C6C9B5C81C05A0C60093367BD64D267D74103458313DA1B7257504DA / SCENE-UNCHANGED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C12-PLACEMENT / TARGETED-PLAY-PASS / EXISTING-ALGORITHM-PRESERVED`

## 改前事实与计划

- C12 owner本身已经验证，但C11前移后production仍为C11→serial→C12。
- 先用phase和serial观察融合结果建立red tests，再只移动C12 barrier；运行回归、SelfCheck和真实Play。

## 实际修改

- `NTSDBattleTickSystem.RunFrameAdvancePhase`现在固定为C10 snapshot→C11 rest/pair-vrest/candidate→C12 fusion→serial remainder。
- actual phase sequence及C06～C11相邻顺序断言同步到RuntimeMaintenance index 13、FrameAdvance index 14；full/partial occurrence仍为31/4。
- 新增C12 focused test与真实Play probe。测试使用完整synthetic running frame 9～12，避免native route读取缺失frame造成伪失败。
- 未修改`BattleOid5152RuntimeModule`、runtime resolver、OID内容、4500/900 timer、C25h reaction timer writer、Scene、Prefab、Config或authority目录。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| test-first | 旧顺序下serial观察OID7、phase index 13观察FrameAdvance | `2 failures` |
| Unity compile | final script refresh；Console精确过滤`error CS` | `0` |
| focused | job `8255c8bbeeca46bfb5184d5fdf1f6e9a` | `2/2 PASS` |
| C04～C12/actual | job `4c0e61b7920c4b419a74dde7a285ff29` | `36/36 PASS` |
| OID split+C12 | job `0b5eaa7c57214ac0a559a704c783a5f6` | `6/6 PASS` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-05 03:47:14 +08 | `PASS` |
| targeted Play | tick6：route frame10→9/state2；C12 OID7/8→51/frame290；serial看到51；timer4499；partner dormant；cleanup | `PASS` |
| Play artifact | `Temp/NTSD28_B3_C12_FusionBarrierPlacement.result.json` | SHA-256 `FFAD6915C6C9B5C81C05A0C60093367BD64D267D74103458313DA1B7257504DA` |
| Scene/Console | Scene SHA前后`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`；最终目标Play后error查询 | unchanged / `0` |

## 失败与修正留痕

- 初始真实Play中synthetic wrapper仅有frame0/10。production native running route合法选择frame9，但fixture没有该帧，导致C12前`Frame.N=9`、`Frame.D`无有效state，融合未发生。
- 增加producer/route/physics/collision/serial观察点确认变化发生在C03 native route内部，而不是C04～C12的生产顺序错误。
- fixture补齐frame9～12为state2后，focused与两次最终Play均通过；生产输入与fusion算法没有为测试让路。

## 结论与后续

C12 placement已闭合；C12算法继续由既有OID production split证据约束，具体collision/fusion规则仍归B5/B7。下一结构首差推进到C13 active weapon count相对serial remainder。
