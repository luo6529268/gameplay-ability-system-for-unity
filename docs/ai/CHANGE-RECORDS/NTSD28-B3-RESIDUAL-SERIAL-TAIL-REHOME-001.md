# NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001 — residual serial tail rehome

<!-- CHANGE-RECORD
id: NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001
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
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28ResidualSerialTailRehomePlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C14-C25 ordering; C14 is immediately followed by C15, while frame/state/lifecycle work belongs to the later per-slot tail; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TEST-FIRST-RED-2 / UNITY-COMPILE-0 / FOCUSED-2-2 / C04-C14-ACTUAL-44-44 / RELATED-289-289 / SELFCHECK-PASS-2026-09-05T05:07:12+08 / TARGETED-PLAY-PASS / RESULT-SHA256-E3A0B326AE09E08AE1835DD6A3F632C054395D653D3691BA23B71C5763829D58 / C15-USER-EXCEPTION-PRESERVED / SERIAL-BODY-PRESERVED / SCENE-UNCHANGED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C14-C15-ADJACENCY / SERIAL-AFTER-C20 / TARGETED-PLAY-PASS / BODY-PRESERVED`

## 改前事实与计划

- 当前生产顺序为C13→C14→`SerialTickAll` remainder→C15例外→C16。
- remainder只剩type3 state/death、runtime snapshot和全局state9998 cleanup；普通实体无业务writer。
- Authority在C14后立即执行C15，再执行C16～C20；frame/state/lifecycle属于后续C25逐slot tail。
- 先写phase与可见性red，再只移动现有caller到C20后；算法本体保持不变。

## 实际修改

- 从`RunFrameAdvancePhase`移除了`FrameAdvanceAll` occurrence。
- 在`RunInteractionPhase`完成第二次held后执行同一个`FrameAdvanceAll`，作为临时C25 tail proxy。
- actual sequence现在为C13→C14→C15→C16→current C17～C20→serial proxy；full/partial仍32/4。
- 更新C06～C14 serial sentinel和actual sequence断言；新增focused/Play probe。
- 未修改`SerialTickAll`、`LF2SpecialAttack`、`CleanupState9998Entities`、hit consumer、random-drop、catch/held/stage算法。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| test-first | job `9df9b1f1d5f84e7294f64c0e2c60eda4` | `0/2`，phase16仍serial；serial RNG=0 |
| Unity compile | final refresh；Console精确过滤`error CS` | `0` |
| focused | job `3325a61e0a5341798d362687554af4f3` | `2/2 PASS` |
| C04～C14/actual | job `152c405251a04838bdeb598a4ff1e652` | `44/44 PASS` |
| frame/type3/state9998/lifecycle related | job `86dc1dfbdf9245889db3f32275f0ea0f` | `289/289 PASS` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-05 05:07:12 +08 | `PASS` |
| targeted Play | tick6；hit3；RNG5→6且serial=6；phase15/16/17/22/23正确；cleanup | `PASS` |
| Play artifact | `Temp/NTSD28_B3_ResidualSerialTailRehome.result.json` | SHA-256 `E3A0B326AE09E08AE1835DD6A3F632C054395D653D3691BA23B71C5763829D58` |
| Scene/Console | Scene SHA/mtime不变；清空后第二次Play；已退出Play | unchanged / `0 error` |

## 验证噪声与修正留痕

- 初次新增测试使用`long`接收authority RNG的`ulong`计数，产生CS0019；只修正测试夹具类型后才取得正式red。
- 两次调用Unity test接口时误用了不受支持的`testFilter`/`filter`字段，分别启动了1980项既有全量套件；其25项失败含资源/结构/性能等既有问题，且新增测试因第一次运行期间编译或编译错误未进入该1980项清单。直接读取本机MCP实现后改用`testNames`/`groupNames`；本Record只采用上表精确相关job，不把误触发全量结果写成当前包通过或失败。

## 结论与后续

C14与用户例外C15已直接相邻，C16紧随C15；serial remainder在current C20后才执行，且本体未改。它仍只是临时C25 proxy：Authority C21/C22/C23/C24/C25以及C25逐slot动态事务尚未形成，state9998全局清理也未被误报为等价。下一步先核对C21 stage settlement与C22 horizontal impulse owner，再继续把serial职责拆入C25。

> 后续状态（2026-09-05）：`NTSD28-B3-C21-C22-PLACEMENT-001 / VERIFIED`已把C21候选与C22 owner插入current C20和serial proxy之间；serial本体未改，下一首差为C23/C24/C25。
