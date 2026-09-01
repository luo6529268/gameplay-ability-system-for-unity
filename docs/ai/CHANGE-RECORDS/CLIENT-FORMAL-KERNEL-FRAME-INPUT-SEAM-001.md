# CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001 — Unity FrameInput relocation seam

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Input/FrameInputSet.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/LocalFrameInputSource.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/FrameInputSetPreallocation.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/FrameInputDenseTraceBuilder.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleSimulationWorkerBoundary.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepReplayJournal.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/StrictDelayedInputBuffer.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepFrameHistoryRing.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FrameInputSetSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/LocalFrameInputProviderEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/LockstepFrameHistoryRingEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 2026-08-30 exact user authorization; C++ release held-action evidence; Server Cut B topology/concrete-tick audits.
evidence: FRAME_INPUT_SEAM_READY / UNITY_COMPILE_PASS / SEAM_4_OF_4 / RELATED_44_OF_44 / S0_8_OF_8 / LOCKSTEP_9_OF_9 / SELFCHECK_PASS / WARMED_ZERO_B / GOVERNANCE_CLOSED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 状态：`FOCUSED_TEST_PASS / FRAME_INPUT_SEAM_READY / GOVERNANCE_CLOSED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 改前事实

- `FrameInputSet.cs`同时定义public canonical values、Client capture interface、preallocated mutable backing和dense diagnostic timeline。
- 现有21个`ResetPreallocated`调用与worker/history warmed 0 B测试约束了可复用路径。
- Public normal constructor、button bits、player sequence、edge bytes、FNV hash和所有consumer type names必须保持。
- `FrameInputSet`尚不在shared package；本包不执行source move。

## 2. 预期职责

- `FrameInputSet.cs`：仅平台无关public value/hash/order合同。
- `LocalFrameInputSource.cs`：Client physical-held capture adapter interface。
- `FrameInputSetPreallocation.cs`：Client-only reusable frame/storage/reset seam。
- `FrameInputDenseTraceBuilder.cs`：Client diagnostic sparse→dense helper。
- Runtime/lockstep call sites仅改可复用实例的构造方式，不改frame内容或消费顺序。

## 3. 不可回退边界

RNG Cut A、30 Hz、held-only产品决议、现有InputAction映射、StartBarrier/TargetTick/InputDelay和所有无关用户diff均不可由本包回退或修改。

## 4. 验证计划

执行Task中的focused/compile/SelfCheck/S0/lockstep矩阵，并以Client Ledger validator和双仓库diff检查收口。任何新asmdef/manifest需求必须先回写本Record。

## 5. 当前结果

- Task/Record已在任何Client脚本修改前建立。
- Public value/hash/order合同保留在`FrameInputSet.cs`；capture、preallocation、dense trace已分别移到三个Client owner文件。
- 可复用runtime/lockstep字段改为显式Client reusable adapter；consumer参数/返回类型仍为`FrameInputSet`。
- 新focused test冻结七bit、player/edge order、golden hash、immutable/reusable等价、dense held carry与warmed allocation。
- Package manifest/lock和asmdef无需改动，因为本包不执行shared-source move。
- 首次 Unity refresh 暴露 3 个 test-only `CS1061`：`BattleSimulationWorkerBoundaryEditorTests.cs` 使用 `Simulation.FrameInputSet` 全限定名，扩展方法不在查找范围。本包只在该已声明测试文件内改为显式 `Simulation.FrameInputSetPreallocation.ResetPreallocated(...)`；第二次 refresh 后以及全部测试后的最终 Console 查询均为 `error CS=0`。
- FrameInput seam focused job `6089d3694527487a8aff01f8cf347257` 为 `4/4 passed, 0 failed/skipped`，覆盖 exact seven bits、golden hash `0x25B94F895B464DCB`、immutable/reusable 等价、immutable reset fail-closed、dense held carry 与 warmed `0 B`。
- 相关回归 job `a5723463beba46e684dbdd2d75ea02ed` 为 `44/44 passed, 0 failed/skipped`，覆盖 `LocalFrameInputProviderEditorTests`、worker boundary、history ring、strict buffer、replay journal 与 checksum；既有 hot-path allocation 仍为 `0 B`。
- S0 witness job `f945e8a369a048b4b21761f43a946c94` 为 `8/8 passed`；existing lockstep job `138335c1ae6e416dbee3078b3d5bb6a4` 为 `9/9 passed`，均为 0 failed/skipped。
- `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-08-30 11:32:54 +08:00` fresh 写入 `PASS`。菜单调用的 MCP 请求发生超时，但结果文件已由 Editor 完整写入；重启的仅是 stdio proxy，未关闭或重启 Unity Editor，随后最终 Console `error CS` 查询成功并为 0。
- Client `Tools/Validate-ChangeLedger.ps1` 通过：`Records 110 / governed code diff 16`；本包所有源码 diff 都由本 Record 覆盖。双仓库 scoped diff 与 `FrameInputSet.cs` BCL-only purity scan 通过。
- Server Debug/Release 十项目均 `0 warnings / 0 errors`，Protocol/BattleHost/Architecture/Integration 与 .NET shared RNG direct consumer 仍通过；`KernelAbstractionsAssemblyMarker.IsFormalBattleKernelImplemented` 保持 `false`，package 中无 `bin/obj` 污染。
- 实际没有修改 package manifest/lock 或 asmdef；本包在 source move 之前闭合。S0/S5 仍非 `VERIFIED`，下一 shared-owner move 不由本证据授权。
