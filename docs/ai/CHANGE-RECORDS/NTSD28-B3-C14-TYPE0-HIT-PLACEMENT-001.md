# NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001 — C14 type-zero hit caller placement

<!-- CHANGE-RECORD
id: NTSD28-B3-C14-TYPE0-HIT-PLACEMENT-001
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
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C14TypeZeroHitPlacementPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/W06CollisionHitResolveWitnessEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan SimulationTickDriver28 C14 first ascending caller loop invokes the shared hit consumer only for current definition type zero immediately after C13; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TEST-FIRST-RED-2 / UNITY-COMPILE-0 / FOCUSED-2-2 / C04-C14-ACTUAL-42-42 / HIT-RELATED-196-196 / SELFCHECK-PASS-2026-09-05T04:33:34+08 / TARGETED-PLAY-PASS / RESULT-SHA256-3695117EDDB5C81334A7D0ADD3DE74851DC25FD03F8C47E3DD100CDE373C6A95 / HIT-ALGORITHM-PRESERVED / SCENE-UNCHANGED / CONSOLE-0 / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C14-PLACEMENT / TARGETED-PLAY-PASS / HIT-ALGORITHM-PRESERVED`

## 改前事实与计划

- Authority C14在C13后立即按升序attacker slot消费current definition type0 candidates。
- Unity现有`PostInteractionTickAll`已经按current DAT type0选择caller，但位于serial remainder后。
- serial remainder对普通type0 production shell没有业务写入；当前实际写入者是type3 special state/death tail与最终state9998 cleanup。把C14前移会恢复Authority相对顺序，不需要复制consumer。
- 先用phase与serial observation建立red，再只移动现有caller；C16与drop保持原位。

## 实际修改

- `RunFrameAdvancePhase`现在按C12→C13→C14→serial执行；`RunInteractionPhase`不再重复type0 caller，只保留random-drop例外→C16 object hit→后续interaction。
- `BattleTickPhase.CharacterHitConsumePostInteraction` occurrence从serial后移到serial前；full/partial仍32/4。
- C06～C13与actual sequence相邻断言同步；新增C14 focused/Play probe。
- 旧`W06CollisionHitResolveWitnessEditorTests`的私有`RunInteractionPhase`测试从“character→object”更正为“只object”，并断言character execution count为0、object仍观察到drop RNG。完整production tick的C14顺序由新测试独立覆盖。
- 未修改`BattleInteractionPipeline`、`BattleEcsHitExecutionPlan`、candidate store、hit resolver/writer、random-drop实现或entity算法。

## 验证

| 层级 | 证据 | 结果 |
|---|---|---|
| test-first | job `3228a7e5791f46ff9fc7fbc34d8aed56` | `0/2`，serial count0；phase15 FrameAdvance |
| Unity compile | final refresh；Console精确过滤`error CS` | `0` |
| focused | job `bf02a6f05e9a4a55b21fefa1e90bcaa0` | `2/2 PASS` |
| C04～C14/actual | job `b2fdbe2272bc4aa08b2a2c63c26c929b` | `42/42 PASS` |
| hit related | job `7f31c1396d52446fadc579c0cf3b9e9a` | `196/196 PASS` |
| SelfCheck | `Temp/NTSD_BattleRuntimeSelfCheck.result`，2026-09-05 04:33:34 +08 | `PASS` |
| targeted Play | tick6 serial/final hit executions=3；phase14～18正确；cleanup | `PASS` |
| Play artifact | `Temp/NTSD28_B3_C14_TypeZeroHitPlacement.result.json` | SHA-256 `3695117EDDB5C81334A7D0ADD3DE74851DC25FD03F8C47E3DD100CDE373C6A95` |
| Scene/Console | Scene SHA前后`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`；目标Play后error查询 | unchanged / `0` |

## 失败与修正留痕

- 首次hit相关回归job `b470740a873c4767929e9a825d054681`为195/196；唯一失败是旧W06测试直接调用后半`RunInteractionPhase`仍期待已迁出的character caller。
- Task/Record先补入该测试路径，再把断言修正为“不重复C14；drop RNG后仍执行C16 object”。final 196/196通过；production未为旧测试恢复重复caller。

## 结论与后续

C14 caller placement已闭合，但这不等于B5 hit body、termination、armor或damage已经对齐。当前实际顺序为C13→C14→serial→C15例外→C16；下一步先审计并拆分serial中type3 state/death与state9998 cleanup的权威归属，不修改用户保留的C15本体。

> 后续状态（2026-09-05）：`NTSD28-B3-RESIDUAL-SERIAL-TAIL-REHOME-001 / VERIFIED`已把serial caller从C14/C15之间原样后移到current C20后；本Record的C14算法与验证事实保持有效，但上述“当前实际顺序”只代表本Record关闭时快照，不得用于恢复最新顺序。
