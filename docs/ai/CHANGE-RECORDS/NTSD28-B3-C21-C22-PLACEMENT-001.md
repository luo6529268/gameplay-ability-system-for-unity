# NTSD28-B3-C21-C22-PLACEMENT-001 — C21/C22 owner placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C21-C22-PLACEMENT-001
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
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomeEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C21 settle_ordinary_stage_bounds then C22 finalize_horizontal_hit_impulses before C23/C24/C25; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TEST-FIRST-RED-2 / UNITY-COMPILE-0 / FOCUSED-2-2 / C04-C22-ACTUAL-46-46 / RELATED-21-21 / SELFCHECK-PASS-2026-09-05T05:28:56+08 / TARGETED-PLAY-PASS / RESULT-SHA256-2E3A6570AB02B834320B86864F8F1A207E6F3DB9D8B81F220A6901469379AF8A / PLACEMENT-ONLY / SCENE-UNCHANGED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C21-C22-PLACEMENT / TARGETED-PLAY-PASS / ALGORITHMS-PENDING-B4-B5-B8`

## 改前事实

- Authority C21最终stage settlement后立即C22 horizontal impulse finalizer，随后才进入resource/frame/per-slot tail。
- Unity `PreFrameBounds`覆盖当前主要边界writer，但缺少部分mode/dynamic字段；`FramePostProcess`覆盖累计knockback提交，但公式尚未B5重证。
- 两个owner当前均在临时serial之后，C22还在RenderDispatch/step-wait gate之后；`Stage`是独立波次逻辑。

## 计划

先用phase与serial可见性建立red，再只移动现有caller到current C20与serial之间；算法与Stage不动。

## 实际修改

- `PreFrameBounds`和`FramePostProcess`从`RunPresentationAndCleanupPhase`移入`RunInteractionPhase`的current C20之后、serial proxy之前。
- actual sequence为current C20→C21 candidate→C22 owner→serial proxy→Stage→Render；full/partial仍32/4。
- F1 step-wait仍在Render后返回，但现在不会跳过C22；C25/late tail仍被冻结。
- 未修改bounds/impulse/ECS算法，也未移动`CurrentWaveStage`。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| test-first | job `8cf7d4da5ba44f7292f3aa415562c0a4` | `0/2`，serial X=-50；phase23 serial |
| Unity compile | final refresh；`error CS`过滤 | `0` |
| focused | job `40d3edd5bf0c44be994d10ec3986b06f` | `2/2 PASS` |
| C04～C22/actual | job `2dbc3cb87816474a9a4badada6189c64` | `46/46 PASS` |
| bounds/impulse related | job `5578f4ce5f0b469caffebe7997fdcd24` | `21/21 PASS` |
| SelfCheck | 2026-09-05 05:28:56 +08 | `PASS` |
| targeted Play | tick6，X=-100、Vx10、accumulators0，phase23～27正确 | `PASS` |
| artifact | `Temp/NTSD28_B3_C21_C22_Placement.result.json` | SHA-256 `2E3A6570AB02B834320B86864F8F1A207E6F3DB9D8B81F220A6901469379AF8A` |
| Scene/Console | SHA/mtime unchanged；Play exited | `0 error` |

## 失败与修正留痕

- 首次SelfCheck为FAIL：旧`R3-INP-02`断言要求F1 wait在C22前返回。Authority C22必须在C23/C24/C25前完成，因此测试改为要求HitCount清零、Vx提交，同时C25h/late仍冻结；最终SelfCheck PASS。生产代码未为旧断言回退C22。

## 结论与后续

C21/C22相对placement已闭合，但不等于bounds/dynamic-stage或impulse公式完成对齐。下一结构首差是Authority C23 begin native resource tick、C24 begin frame tick与C25逐slot tail在Unity仍没有统一生产owner；临时serial只覆盖其中极小残留。

> 后续状态（2026-09-05）：`NTSD28-B3-C23-C24-WORLD-CLOCK-001 / VERIFIED`已建立C23/C24 single owners及snapshot/checksum闭包；下一首差只剩C25 nested tail。
