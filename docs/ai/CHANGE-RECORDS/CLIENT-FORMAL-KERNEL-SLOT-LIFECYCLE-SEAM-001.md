# CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001 — Cut C Slot/Lifecycle Seam

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SEAM-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/RuntimeSlotLifecycleState.cs
code-path: Assets/NTSD/Scripts/Simulation/RuntimeSlotTable.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationQueryAndLinkModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RuntimeSlotLifecycleSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleParityStructuralWitnessEditorTests.cs
authority: User exact authorization dated 2026-08-30; C++ release live Authority400 and closed Cut C identity/golden-journal/StageSpawn prerequisites.
evidence: USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SLOT_LIFECYCLE_SEAM_READY / GOVERNANCE_CLOSED / CUT_C_SOURCE_MOVE_FORBIDDEN / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> 创建日期：2026-08-30
> 当前状态：`FOCUSED_TEST_PASS / SLOT_LIFECYCLE_SEAM_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / S0_NOT_VERIFIED`

## 1. 改前事实

- `RuntimeSlotTable`同时持有BCL allocator/Generation与LF2Entity/raw/page/snapshot adapter。
- structural buffer当前自行按allocate event计数LifecycleEpoch，不是canonical state。
- registration先claim，再做rest/stable-id；回滚可推进Generation但不能形成成功allocationEpoch。
- StageSpawn rest clear/lease-conflict rollback已经单独focused关闭。

## 2. 计划职责

- 新BCL state拥有provisional claim、required side-effect readiness、commit、rollback/release、本地Generation和allocationEpoch。
- RuntimeSlotTable只适配entity/raw/page并保持现有public behavior。
- Registry在rest和注册检查后commit，然后才发布成功allocate event。
- structural witness读取canonical epoch，不自行衍生。

## 3. 验收与回滚

完全遵守配对Task Contract。Cut C source move、snapshot/recovery、formal AI、marker和S0/S5晋升均不在本包。

## 4. 验证记录

- 双仓库事前Task/Change Record：已在脚本修改前建立。
- Test-first failure：已取得红灯。`.NET Debug`仅因尚不存在的`RuntimeSlotLifecycleState.cs`产生`CS2001`；通过现有Unity MCP Stdio heartbeat端口`6400`强制refresh/compile得到18条预期`CS0246/CS1061`，全部指向新seam/ticket/identity/AllocationEpoch API，旧`Assembly-CSharp.dll`未被替换。
- Runtime实现：已在声明范围内完成。新`RuntimeSlotLifecycleState`拥有provisional/side-effect/commit/rollback/release、本地Generation和per-slot allocationEpoch；`RuntimeSlotTable`保留LF2Entity/raw/page adapter，Registry只在rest和stable-id检查后commit，structural buffer消费canonical epoch。
- 首次related regression：`BattleParityStructuralWitnessEditorTests.W04`失败，原因是stage diagnostic entity没有`ItrRest`，既有StageSpawn helper将其误拒绝而未执行C++全局cooldown clear。已在修改前将`SimulationQueryAndLinkModule.cs`纳入范围；有tracker的lease语义保持不变。
- First-difference修正：无`ItrRest` tracker的实体只执行C++全局slot rest clear；有tracker继续走acquire-first atomic `TryResetAndBind`。W04复跑`1/1`，最终StageSpawn/rest为`2/2`。
- Unity compile：最终MCP refresh后当前`Assembly-CSharp.dll`生成，Console C# error为0。
- Unity证据：seam `5/5`（`609630fb8dfe4894893835838c0564d9`）、StageSpawn/rest `2/2`（`f3de9e717663465fa8a16088642df67f`）、structural `4/4`（`b1de9045ddc947fa8699656e54f5ecc1`）、occupancy `1/1`、slot snapshot `3/3`、state restore `9/9`、same-tick `8/8`、pending-destroy `7/7`、S0 `8/8`（`9c64c69ba75448db955515842fb51dbf`）、lockstep `9/9`（`db05ff8fed7f4e5da5da26e5100a623c`）全部0 failed/skipped。2026-08-30 16:53:57 fresh `BattleRuntimeSelfCheck=PASS`。
- 性能/合同：seam suite锁定Authority400 bands、failed-side-effect不可重试且不增epoch、commit exactly once、rollback/release不增epoch、stale handle、same-slot reuse和fresh reset；warmed 1,024 cycles为`0 B`。
- .NET/Server：linked-source test-only consumer Debug/Release均打印`NTSD Battle Kernel RNG, FrameInput and slot-lifecycle seam consumers PASSED.`；Server solution Debug/Release各`0 warnings / 0 errors`，Protocol/BattleHost/Architecture/Integration suites双配置通过，Release no-network Host为`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`。
- 治理：pre-close Client Ledger `114 records / 32 governed code files`通过；Server Ledger `66 / 97`通过；pre-close workflow `51 rows / ACTIVE 1 / READY 0 / GATED 3 / DEFERRED 6`通过；scoped diff/whitespace检查通过。未改manifest/lock/asmdef/version、Scene、资源、Input Actions、tick、battle rule、transport、recovery schema、formal AI或marker。
- Final post-close治理：Client Ledger再次`114 / 32`通过；Server Ledger `66 / 97`通过；workflow与exact ClientImpact在`52 rows / ACTIVE 0 / READY 0 / GATED 4 / DEFERRED 6`通过。Queue `0bc`只被记录为下一门禁，尚未授权。

## 5. 实际文件与职责

- Runtime：新增`RuntimeSlotLifecycleState.cs`；修改`RuntimeSlotTable.cs`、`SimulationWorld.Registry.partial.cs`、`SimulationQueryAndLinkModule.cs`和`BattleParitySnapshot.cs`。
- Client evidence：新增`RuntimeSlotLifecycleSeamEditorTests.cs`并更新`BattleRuntimeSelfCheck.cs`。声明的既有`BattleParityStructuralWitnessEditorTests.cs`只用于回归运行，没有源码diff。
- Server test-only consumer：`NTSD.Battle.Kernel.Tests.csproj`、`Program.cs`、`RuntimeSlotLifecycleSeamVerifier.cs`；它们不构成production source move。

## 6. 未关闭项

- 本包没有扩张snapshot/recovery schema；formal `allocationEpoch` snapshot/restore属于未来S3合同，当前未验证。
- allocator/handle/lifecycle production source仍归Client；后续`CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001`必须另获精确授权并先建Task/Change。
- Formal AI、完整Kernel/world/pass owner、C++ completed-tick/domain/event mapping、atomic frame commit、snapshot/recovery和marker promotion仍待；S0/S5保持NOT_VERIFIED。
