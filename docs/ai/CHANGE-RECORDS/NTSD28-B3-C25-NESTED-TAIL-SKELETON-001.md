# NTSD28-B3-C25-NESTED-TAIL-SKELETON-001 — C25 production skeleton

<!-- CHANGE-RECORD
id: NTSD28-B3-C25-NESTED-TAIL-SKELETON-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25NestedTailSkeletonEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25NestedTailSkeletonPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C21C22PlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C23C24WorldClockEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomeEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C06NestedPhysicsProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C07RevivalProductionPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C08StageDepthPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C10CollisionActionSnapshotPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C11CandidateBuildPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C12FusionBarrierPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C13ActiveWeaponCountPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C23/C24 then C25 dynamic ascending live-slot tail; GameSession snapshot observes completed world; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-RED-1-OF-2 / UNITY-COMPILE-0 / FOCUSED-2-2 / PLACEMENT-W05-WORKER-51-51 / PRESENTATION-RELATED-14-14 / NTSD28-BROAD-123-123 / SELFCHECK-PASS-2026-09-05T06:38:37+08 / TARGETED-PLAY-PASS / RESULT-SHA256-8AE8AB883FEF85617ABD43854B98D46A5EFC17F2C7D81D78CE2286A90430D2F5 / SCENE-UNCHANGED / PLAY-EXITED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C25-SINGLE-PRODUCTION-ENTRY / COMPLETED-TICK-RENDER / TARGETED-PLAY-PASS / C25A-P-BEHAVIOR-PENDING`

## 改前事实

- production C24后先运行legacy `FrameAdvance/SerialTickAll`，随后Stage和Render，之后才进入动态late-slot loop。
- normal Render冻结的world未包含late、post-tail和Results写入；这与Authority completed-world snapshot边界相反。
- late loop本身已有动态cursor与高/低slot测试基础，本包只重建顶层placement，不修改a～p内部writer。

## 实际修改

- 正常tick在C24后先进入`LateEntityUpdate`作为C25 single production entry；legacy serial remainder显式后置。
- Stage、random tail、entity post-tail、Results完成后再Render，published frame现在标记同一completed tick。
- step-wait旧路径仍执行legacy serial→Stage→Render后返回，不执行C25/post-tail/results；本包没有扩义。
- 更新C06～C24 placement断言与actual sequence；新增focused与真实Play probe。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| test-first | job `69abe6d0d3ff42648a010562ba7b5908` | `1 FAIL / 1 PASS`，旧index27=`FrameAdvance` |
| focused final | job `cc2a8c85af744b2783edc89dc8dd671e` | `2/2 PASS` |
| placement/W05/worker | job `f634d9b90b5249c8b2a552d19b7e305e` | `51/51 PASS` |
| presentation/results/OID | job `39c7c76d7689422c8eefbe6a7eeaf483` | `14/14 PASS` |
| NTSD28 broad | job `f56031d148764c5bb5fe42bbf32945c9` | `123/123 PASS` |
| SelfCheck | 2026-09-05 06:38:37 +08 | `PASS` |
| targeted Play | tick6，phase25～33与publishedTick6 | `PASS` |
| artifact | `Temp/NTSD28_B3_C25_NestedTailSkeleton.result.json` | SHA `8AE8AB88...D2F5` |
| compile/Scene/Console | compile0；Scene SHA/mtime unchanged；Play exited | `PASS / 0 error` |

## 边界

这只是C25生产入口和顶层placement完成。legacy serial仍是C25后的可见残留；`BattleLateEntityLifecycleModule`内部仍未按a～p精确重排，post-tail仍是global scan。下一包从C25a definition transition与C25b special clone开始。
