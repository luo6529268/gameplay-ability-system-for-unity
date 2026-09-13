<!-- CHANGE-RECORD
id: NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001
status: FOCUSED_TEST_PASS
change-kind: REMOVE_RETIRED_HOLDERCOPY_CARRIERS
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterLateRuntimeModule.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Tasks/OPointCreateTask.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/Respawn/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/StageSpawnTaskConfigurator.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCollisionHitDamagePlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleDeathRespawnAiIntegerPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHeldWeaponLifecyclePlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRandomWeaponLateEffectPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind5LinkedParentSlotCorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceAttackerResolverEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardReducedKnockoutProducerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3KindCatalogTransformEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3LegacyHolderCopyWriterRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3TargetGenericContinuationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Type3WeaponFluteLegacyStatRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6EntityLinkLifecycleCleanupProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6HeldInjuryAccountingCoverProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6Kind2PickupAtomicProductionIntegrationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyGrabbedByRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyHolderCopyResidualRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointKind3ReleaseProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointTerminalStructuralProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05HolderCopyCarrierEditorTests.cs
authority: Q03 JOINT-FIELD-MATRIX / OPOINT-AND-HELD-DEPTH-CONTRACT; NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001 and RESIDUAL-RETIREMENT-PRODUCTION-001 VERIFIED, B5 type3 writer retirement VERIFIED; formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: RED_7_FAIL_4_PASS / FOCUSED_863_RECONCILED / SELFCHECK_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING
-->

# Q05 HolderCopy载体完整清理

准确36脚本（13生产/22旧测试与probe/1新测试）。所有当前production引用已核对为field/default/copy/reset、Entity包装、任务默认赋值、ECS数组/hash/比较、checksum/parity和HitPlan诊断。不存在行为reader；前包保留的真实linked-parent/owner/spawner/group/control仍是对应数据，不用它们重造HolderCopy别名。两条factory的相关行为此前退休，当前均不读task.holderCopySlot，不能为本包重写factory/initializer。

生产范围：NTSDEntityRuntime.HolderCopySlotIndex、LF2Entity.HolderCopySlot、Character/Weapon/Other默认、OPointCreateTask.holderCopySlot及Entity/CharacterLateRuntime/StageSpawnTaskConfigurator/Respawn的-1赋值；ECS links数组/capture/clear/compare/hash；checksum/parity；HitPlan WriterEffectSnapshot.TargetHolderCopySlot/capture/DifferenceMask bit33。bit33退休后保留空洞，不复用/重排32、34、35及其他位，反射执行相邻bit回归证明位置。没有新序列字段或独立版本晋升。

旧测试/诊断同步去掉人工holderCopy sentinel及默认值断言，保留全部有效HP/动作/速度/命中/关系/回收检查。只有退休占位参数的重复用例需合并或改为明确有效Owner独立性fixture；准确写明变化。G16 pair和旧death probe报告去掉holderCopy字段，不输出假99/-1；保留真正holder/owner字段。SelfCheck中ReleaseTick包已改的有效断言不覆盖。旧源码负向guard保留，原结构保留guard改为absence，旧fixture名如涉及sentinel同步其同文件runner。

Test-first：新增runtime/Entity/task/ECS/diagnostic absence、checksum/parity缺字段guard、实际WriterEffectSnapshot相邻mask 32/34/35、任务Clear真实owner/spawner/parent字段；先RED再生产。相关全类包括B5/B6/HitPlan/kind2/type3/held/OPoint task复用/snapshot/restore/ECS/checksum/完整SelfCheck；真实Goal20_R12 request进入Battle场景的G16 pickup/replacement与current OPoint fulltick/unregister验证。其他旧probe仅迁移，不冒称所有probe已逐一执行。

保留前包未提交差量；不修改Scene/InputActions/Gen/Plugins/外部包/资源/非战斗/Unity-GAS，33ms/3ms和十一阶段不变，无新增生命周期owner。禁止computer-use，只桥接/日志/结果。Scene旧SHA/Foot18既有缺失与新目录保持。

Q05同窗口12/20/23/1/1中间形状未发布；五类删除后仍需identity/双OPoint空队列guard、统一13/21/24/2/2、旧版本拒绝/新回放/Play，不能关闭父Q05/R13/R15或提前Q07。回滚须明确批准，按preimages字节/SHA仅撤销本包精确差量，不清理其他用户工作。

## 已写

RED11为7FAIL/4PASS（相邻mask三位与task有效Clear已通过），改前Goal20_R12真实拾取/替换/OPoint PASS已留证。准确36脚本已写；13生产仅载体/默认/序列/诊断删除，bit33保留退休空洞。旧测试专属sentinel改absence，真实关系保持；Kind5移除无效copy参数但3case仍在，六held用例改验独立Owner取99/-1/55不被持有接线覆盖。首轮变更在内存匹配遇到多行赋值时停止、未落盘，修正匹配后写入。待编译与剩余引用审查/focused/SelfCheck/Play。

代码审查补齐Character仅包围旧默认赋值的空if移除；_initializedFromOpoint其他用途继续按真实引用保留。本轮相关编译中间状态不作验收，最终重载后再取新证据。

中间编译3个SelfCheck语法错误：迁移赋值匹配误含==，造成旧复合断言截断。按preimages生成精确修正版并逐个验证当前等于本包初次变更，修复受影响测试文件、保留所有有效断言；未重写生产。修正清单在migration-assertion-correction.json。最终compile/focused仍待。

Focused863=859PASS/4FAIL，四旧统计预期由独立NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001处理；不改production。SelfCheck审查还移除三个原本只包围HolderCopy sentinel的if及专用HashSet，确保Team/RelationTeam仍逐tick验证，清除旧/99诊断描述；完整SelfCheck随后执行。

## 限定出口

首次863=859/4；独立NTSD28-Q05-RETIRED-HOLDER-STATS-FIXTURE-CORRECTION-001保留原失败并以26+1定向通过证据闭合四旧预期（Cpoint真实KO1保留）。完整SelfCheck请求10:34:09Z→新PASS10:34:58Z。真实Goal20_R12 Play tick5→9，两个pickup/replacement witness及current OPoint kind2/action279→213 childaction0 fulltick/unregister PASS；去除holderCopy诊断key后两witness前后完全一致。Scene旧SHA/dirtyfalse/root14、CS0、无新缺失，13production精确删载体检查通过。当前包36与前包合计45脚本范围一致，496 Records validator PASS。详细实际命令/边界/失败历史见artifact REPORT。下一NTSD28-Q05-CONTENT-IDENTITY-AND-CAPTURE-BOUNDARY-001，联合identity/version/replay仍待。
