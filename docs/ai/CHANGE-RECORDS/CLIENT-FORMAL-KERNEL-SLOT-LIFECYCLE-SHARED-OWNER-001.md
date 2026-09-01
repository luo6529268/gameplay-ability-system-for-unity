# CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001 — Cut C Shared Slot/Lifecycle Owner

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/RuntimeSlotAllocator.cs
code-path: Assets/NTSD/Scripts/Simulation/RuntimeEntityHandle.cs
code-path: Assets/NTSD/Scripts/Simulation/RuntimeSlotLifecycleState.cs
authority: User exact named authorization dated 2026-08-30; Server single-owner package topology; closed Cut C identity/golden-journal/StageSpawn/seam prerequisites.
evidence: USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / SINGLE_OWNER_GUID_PASS / PACKAGE_0_3_0_DIRECT_AND_LOCKED_ARTIFACT_PASS / UNITY_COMPILE_0 / UNITY_PACKAGE_1_1 / UNITY_SLOT_RELATED_33_33 / SELFCHECK_PASS / S0_8_8 / LOCKSTEP_9_9 / SERVER_DEBUG_RELEASE_FULL_NO_NETWORK_PASS / FORMAL_MARKER_FALSE / GOVERNANCE_CLOSED / S0_NOT_VERIFIED
-->

> 当前状态：`FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / GOVERNANCE_CLOSED / USER_AUTHORIZED / CLIENT_INTEGRATION_REQUIRED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## 1. 改前事实

- 三份BCL-only production source当前仍位于Client，并保有GUID `3f48ed724a794e0a99073634df7ae654`、`35a1af57f8e14b27a120ff236130d1ab`、`92265319af00462f90e1a367b8ec3584`。
- Client消费方位于predefined `Assembly-CSharp`；Server package Core asmdef已经auto-referenced/no-engine，因此预期无需修改`RuntimeSlotTable`或任何caller。
- 唯一已识别assembly-boundary问题是`UNITY_INCLUDE_TESTS`下的internal self-check setter；计划只在shared source中提升其可见性，不改行为。
- Server direct test当前仍以sibling-relative路径链接Client source；source move后必须删除该隐藏owner。
- 当前package为`0.2.0`；按既有release tuple规则，向后兼容新增三种public Core type应统一升级到`0.3.0`。

## 2. 计划改动

完全遵守配对Task Contract。只删除原Client三份`.cs/.meta`路径，因为相同内容/GUID将先出现在Server-owned package。禁止修改`RuntimeSlotTable`、Registry、Rest、structural witness、Client tests或manifest/lock/asmdef，除非精确compile first-difference证明metadata必要且先补记录。

## 3. 验证记录

- 双仓库Task/Change：已在任何source/project/test脚本修改前建立。
- Queue/Ledger/State/Handoff/matrix activation：待写入后运行pre-code validator。
- Pre-move failing ownership/consumer evidence：已取得红灯。Static owner check精确报告三个Client owner仍存在、三个package owner缺失；移除test-only sibling links并让Core project引用预定package路径后，.NET Debug只产生三个预期`CS2001`。新增package Unity vector test触发live Editor compile后只产生三个预期`CS0246 RuntimeSlotLifecycleState`；未修改任何冻结Client adapter。
- Source move/API：三份物理源码/GUID已移动且原Client路径不存在。首次Unity compile精确产生五个existing RuntimeSlotTable调用的七条accessibility错误（`InvalidateLocalLeaseForWorldReset`、`BeginTopologyRestore`、`SetLocalGenerationForTopologyRestore`、`TryRestoreCommittedClaim`、`CompleteTopologyRestore`）及一条级联`CS0165`。修改shared source前已在Task/Record追加：只允许这五个符号与预定test setter从internal变public，RuntimeSlotTable及方法体保持冻结。
- .NET evidence：package-owned source的direct Debug/Release consumers通过；独立local feed以exact `[0.3.0]`强制restore后再locked-mode restore并运行通过。Core artifact SHA-256为`3009082B0DD9C492CE9A0E5FEC9AAA8F7C626047650E7F1B4CD6EC12D26B4756`，Abstractions artifact为`B28B427AB8B04FFDB723ABD6EB0DC6C3D6E48F94F950B71DAC71992F358D9768`。
- Cross-consumer evidence：48-line vector SHA-256 `22F25272BCD5E4616AFB92B50A6E080E546B6AA53A11DAB96647387F1C4381B7`；Unity和.NET均验证assembly owner、版本、journal与lifecycle transition。
- Unity evidence：C# compiler error count `0`；package vector job `739939483bbd4ae29c6a6d24ba8e2078`=`1/1`；seam job `43a493d9642e44049aaa29afdcbcf95a`=`5/5`；related slot/lifecycle jobs合计`33/33`；S0 job `2c9d6470992d49af9b1f1e3c7fbf256f`=`8/8`；lockstep job `c5b50cb7332c40eb876ae21d9f72840e`=`9/9`；fresh SelfCheck于`2026-08-30T17:46:42.9494184+08:00`为`PASS`。
- Server evidence：Debug/Release solution build均`0 warnings / 0 errors`；Protocol/BattleHost/Architecture/Integration suites双配置通过；Release no-network host输出`BootstrapReady`、liveness/readiness true、`SequentialSingleWriter`和`NetworkListenerStarted=False`。
- Boundary evidence：owner `3/3`、GUID `3/3`、vector hash/line、package `0.3.0`、package-local `bin/obj=0`均通过；formal marker仍false且无diff。RuntimeSlotTable、Registry、Rest、battle/tick、Scene/resource/Input Actions、transport、recovery、S1 wire均未在本包修改。
- Final governance evidence：workflow `53 / ACTIVE0 / READY1 / GATED3 / DEFERRED6`且Queue `0bd`为first READY；Server Ledger `67/101`、Client Ledger `115/33`、matrix exact set `67/67`与formal stages `10/10`通过；双仓库`git diff --check`仅有line-ending warnings。

## 4. 未关闭边界

本包不实现formal snapshot/recovery、formal AI、world/pass owner、C++ completed-tick/domain/event mapping或marker promotion；S0/S5保持NOT_VERIFIED。下一Queue只允许只读Cut D rest/checksum projection审计，不能继承本包授权修改Client。
