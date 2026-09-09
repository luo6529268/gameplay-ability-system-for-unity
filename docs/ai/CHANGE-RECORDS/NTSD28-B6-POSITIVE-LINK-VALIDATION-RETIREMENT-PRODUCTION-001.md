# NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001

<!-- CHANGE-RECORD
id: NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001
status: VERIFIED
change-kind: TEST_FIRST_UNITY_ONLY_POSITIVE_LINK_PASS_RETIREMENT
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleProductionOwnershipInventory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsPositiveLinkValidationPass.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/ProductionEntityStressHarness.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressWindow.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsPositiveLinkValidationPassEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6PositiveLinkValidationRetirementProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleGrabCpointLinkPlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleParityStructuralWitnessEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleParityTraceEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleU6ProductionOwnershipEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C09HeldRefillPlacementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/ProductionEntityStressCapacityPressureEditorTests.cs
authority: NTSD 2.8-Logan SimulationTickDriver28 post-catch sequence and BattleWorld28 clear_entity_links lifecycle; EXE B1E13AE1, closure 39DDDA15.
evidence: RED_4_FAIL_2_PASS_OF_6; focused 6/6; related 33/33; B6 103/103; NTSD28 229/229; stress fixture 255/256 with one unrelated AI-report failure; Unity and dotnet builds 0 error; full SelfCheck advanced beyond retired pass and stopped at unrelated stale R2 CPoint-sync fixture; real Battle grab Play PASS with three-step post-catch sequence and positive mismatch preserve; console 0 error; Scene byte identity unchanged.
-->

> 状态：`VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 / C09_RELATED_9_OF_9 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 / STRESS_255_OF_256_1_UNRELATED_AI_REPORT / REAL_BATTLE_GRAB_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC / CONSOLE_0_ERROR / SCENE_UNCHANGED / PHASE_33 / NO_POSITIVE_EVENT / LEDGER_PASS_429_373`

## 改前事实

- Authority post-catch无独立positive relation validation；Unity正式tick却有`HeldLinkValidation`，
  invalid时只清holder `LinkState`并保留其余正反向字段，且可发额外structural event。
- lifecycle cleanup后normal release/reuse已原子清理，因此继续保留该pass只会制造synthetic mismatch首差；
  fresh full SelfCheck当前正停在inactive-target旧半清理断言。
- stress mode/report、U6 inventory、W07与phase tests仍把这条Unity-only规则当作production owner。

## 预定实现

- 先新增focused RED，冻结phase/surface、半清理/event、lifecycle、stress/U6与0B边界。
- 原子移除正式phase与world owner，强制遗留entry no-op；同步退休stress/U6/parity旧witness和测试。
- negative invalid-preserve、AI projection和lifecycle cleanup保持独立且必须回归。

## 实际实现

- `NTSDBattleTickSystem`已从正式post-catch路径移除`HeldLinkValidation` occurrence；
  `BattleTickPhase`后续值连续前移，`Count`从34改为33。完整空World tick现记录33个occurrence，
  `PreInteraction -> StageBounds -> HeldProcess`直接相邻。
- `SimulationWorld`不再持有、构造、reset、配置或restore positive-link pass；
  `ValidateHeldLinksAll(int)`仅作为带`[Obsolete]`的allocation-free空兼容入口保留，不写runtime、
  不刷新snapshot、不发structural event。旧pass源文件未删除，其`Execute()`被硬锁为no-op。
- stress request/config/formatter/menu/report/setup/teardown/U6 counters已移除该mode及全部
  requested/effective/restored/run/visit/kept/cleared/mismatch surface。U6 domain/failure/entry已移除，
  canonical owner数由9改为8。
- W07现验证兼容入口无event、lifecycle unregister原子清关系；真实grab probe的顺序已改为
  `first-held -> cpoint-weapon-sync -> second-held`，synthetic positive mismatch保持全字段。
- lifecycle writer、negative held诊断的`Action=link-validation`、AI relation projection、
  relation store/generation、snapshot/checksum/restore均未修改。positive bitmap/index保留为无production
  consumer的兼容诊断存储，没有物理删除源文件或扩展本包到relation store重构。

## 验证记录

- 2026-09-09 Unity EditMode job `8bebbd2c698540a68ab0748e1d2bc635`：
  `NTSD28B6PositiveLinkValidationRetirementProductionEditorTests` 精确 RED 为
  `4 fail / 2 pass / 6 total`。失败分别冻结phase/World owner surface、synthetic mismatch
  `LinkState 7 -> 0`、stress request/config/report surface及U6 canonical owner；通过项确认
  warmed 4096调用当前0B及lifecycle unregister原子清理基线。执行RED时生产脚本仍未修改。
- Unity脚本刷新后Console编译错误为0；`dotnet build Assembly-CSharp.csproj --no-restore`
  与`Assembly-CSharp-Editor.csproj --no-restore`均exit 0、0 error（既有warning保留）。
- Unity EditMode GREEN job `9cf0f048014e45049e589ab2a1c82262`：新focused `6/6`。
- Unity EditMode related job `7b8e526781204f5fb717888532fe2f8e`：旧兼容入口、actual phase、
  U6、lifecycle、W07与capacity pressure合计`33/33`。
- Unity EditMode B6 job `919cd62af8dc4bb5863ff130b3af3683`：`103/103`；NTSD28 job
  `0e4a70101adb49e3b5f45a7cfdeaba0a`：`229/229`。
- `ProductionEntityStressEditorTests` job `b0c6df97b43a4fcc8f875d4bbe2b91d2`为
  `255 pass / 1 fail / 256 total`；唯一失败
  `UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport`是独立既有AI-report断言
  `expected 1 / actual 2`，本包phase/stress相关测试通过。
- fresh full `BattleRuntimeSelfCheck`已越过退休前阻塞的positive-link matrix，后停在独立旧夹具
  `R2-SCHED-001: T14 CPoint sync must still execute after candidate consumption`；该夹具只填compat
  catch slots而当前catch exact-field production要求exact relation，未作为本包失败修补或扩范围。
- 目标Unity 2022实例stable Play重跑抓取探针为PASS：三条pass序列不再包含positive validation，
  `linkResidue.positiveLinkAfter=5`，negative relation两次held扫描均保持；退出Play后Console 0 error。
- `Assets/NTSD/Scene/NTSD_Battle.unity`前后均为SHA-256
  `D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`、209891 bytes、
  UTC last-write `2026-09-09T06:33:50.7338880Z`。
- `Tools/Validate-ChangeLedger.ps1`通过：`Records=429 / Governed code files=373`；
  本包脚本scoped `git diff --check` exit 0，新focused文件尾随空白为0。
- 后续恢复held-refill runtime package时，Unity job `91cebcc8fe354f1f888e5ed33b529ebd`
  暴露一项本包直接遗漏：7-case held-refill均通过，但
  `NTSD28C09HeldRefillPlacementEditorTests.FullTick_...`仍按删除前phase occurrence索引读取，
  `expected FrameAdvance / actual Stage`。因此本包撤回VERIFIED，先同步该test-only索引后重验；
  production实现本身没有在此结果中失败。
- 仅修正C09 test-only occurrence索引`28 -> 27`与count`34 -> 33`后，Unity job
  `edb6022909c5414499e9a5c2364c4c76`为`9/9`（held-refill 7项、C09 placement 2项）。
  因生产脚本没有再变，前述focused/related/B6/NTSD28/Play/build证据仍适用，本包恢复VERIFIED。

## 回滚

反向恢复本Change的phase/world/stress/U6/parity接线与测试，不回退前置B6行为。
