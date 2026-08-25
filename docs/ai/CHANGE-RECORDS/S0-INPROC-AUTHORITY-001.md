# S0-INPROC-AUTHORITY-001 — 同进程权威多 world 骨架

<!-- CHANGE-RECORD
id: S0-INPROC-AUTHORITY-001
status: FOCUSED_TEST_PASS
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepAuthoritySession.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepSessionIdentity.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/LockstepStartBarrier.cs
code-path: Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs
authority: User-approved server-lockstep-s0-s9-design.md S0 contract; C++ release tick order remains unchanged
evidence: SOURCE-REVIEWED / CODE-WRITTEN / USER-APPROVED-VALIDATION-ONLY / FRESH-ASSEMBLY-COMPILE-EVIDENCE / SELFCHECK-PASS / S0-FOCUSED-NUNIT-5-OF-5-PASS / EXISTING-LOCKSTEP-9-OF-9-PASS / WITNESS-IMPLEMENTATION-REQUIRED / RUNTIME-PENDING
-->

> 创建日期：2026-08-24  
> 最后更新：2026-08-24  
> 类型：battle / lockstep / test

## 1. 状态与范围

- 当前状态：`FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`
- 用户执行边界：用户现已明确仅授权既有 Unity S0 的只读、编译、focused test 与 `BattleRuntimeSelfCheck`；**不授权 Client 源码、场景、资源或配置修改**。
- 所属 Work Package：`S0-INPROC-AUTHORITY-001`
- 不属于本次范围：独立 Server solution、Socket、S1～S9、数据库、公网、战斗规则和表现改动
- 关联 Change ID：无

## 2. Authority / 需求依据

- 用户明确批准按 `server-lockstep-s0-s9-design.md` 执行，并将未来独立服务端根目录定为 `I:\GitHub\Unity_GAS\NTSD_Server`。
- S0 需求：同进程一个 server world + 至少两个 client world，共享同一 Kernel、输入 journal 与 checksum 合同。
- C++ release live path 仍唯一决定战斗 pass 与规则；本包不改变这些行为。
- Evidence 等级：用户需求 `VERIFIED`；S0 运行证据 `PENDING`。

## 3. Unity 原状与已确认差异

- `BattleLockstepSession` 已覆盖单个 `SimulationTickDriver` 的 delayed input、journal、history、checksum 与 snapshot 基础。
- 现有测试只顺序创建单 driver，没有同时存在的 server/client 多 world 权威会话。
- `SimulationTickDriver` 是 Unity singleton，不能作为 S0 多 world Kernel owner。
- `LockstepSessionIdentity.CanonicalPlayerSlots` 以 `IReadOnlyList<int>` 暴露底层数组，调用者可通过运行时 cast 获得数组并修改。
- 目标：新增直接拥有 `SimulationWorld + NTSDBattleTickSystem` 的实例边界，并由预分配 authority history 锁定输入。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `LockstepSessionIdentity.cs` | canonical slot exposure | 暴露底层数组对象 | 暴露不可变只读视图 |
| `LockstepStartBarrier.cs` | new | 无 | 冻结 identity、roster、rule/policy/world settings |
| `InProcessBattleKernelHost.cs` | new | 无 | 独占 world/tick system，消费 canonical frame 并记录 checksum |
| `InProcessLockstepAuthoritySession.cs` | new | 无 | server→clients 固定推进、authority history、first difference |
| `InProcessLockstepAuthoritySessionEditorTests.cs` | new | 无 | S0 多 world、重放、不可变与 fail-closed 证据 |

## 5. 不可回退边界

- 不改变 C++ release live pass 顺序或任何 gameplay writer。
- 维持固定 30 Hz 与逐 tick `FrameInputSet`；S0 不读取墙钟。
- 不改变现有 LocalFreeRun/Manual/LockstepBuffered host policy。
- 不修改 CentralOnly、Texture2DArray、动态 Mesh、Transform 或音频表现。
- 不使用 `partial`，不创建新的全局 static state。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `LockstepSessionIdentity.cs` | `CanonicalPlayerSlots` | 以 `Array.AsReadOnly` 封闭底层数组 | 启动期增加一个只读视图对象；不改变 hash/排序 |
| `LockstepStartBarrier.cs` | `LockstepStartBarrier` | 冻结 identity、rule/policy、world settings 与 roster slots，计算 barrier fingerprint | 只在 session 启动期分配 |
| `InProcessBattleKernelHost.cs` | `InProcessBattleKernelHost` | 每副本独占 logic-only world/tick system、journal/history/checksum，连续 tick fail closed | 不依赖 Unity driver singleton；不构建表现 |
| `InProcessLockstepAuthoritySession.cs` | `TryAdvance` | 先复制 authority frame，再固定 server→clients 推进并锁存首差 | authority journal 满、非 canonical 或 checksum 分叉后永久 fault |
| `InProcessLockstepAuthoritySessionEditorTests.cs` | 5 项 S0 tests | 覆盖 1 server + 2 clients、重复 journal、源数组变更、wrong tick、barrier mismatch 与 checksum 首差 | Editor-only |

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | 已运行 Editor 的 `Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 刷新、Editor.log compile-error scan | 两个程序集于 2026-08-24 17:06:41/17:06:42 刷新，晚于全部 S0 source；Editor.log 未匹配 `error CS*`、`Scripts have compiler errors` 或 `Compilation failed`。 | `COMPILE_PASS` |
| focused test | S0 `InProcessLockstepAuthoritySessionEditorTests` | 用户在现有 Unity Test Runner 的 EditMode 手动运行；截图显示五项方法全绿、右上角 5 passed / 0 failed。没有持久 TestResults XML，因此保留截图作为会话证据。 | `PASS / 5 OF 5` |
| self-check | `BattleRuntimeSelfCheck` | 已由现有 Editor request-file 入口实际执行；`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 2026-08-24 17:07:33 写入 `PASS`，Editor.log 记录“自检完成”。 | `PASS` |
| C++ authority 对照 | 本包不改 battle rule；仍需确认 tick host 未分叉 | 尚未运行 | `PENDING` |
| 运行时 | 同进程 server + 2 clients 连续 journal | focused NUnit 已在 logic-only empty-world 范围内运行 48 tick、重复两次并通过；真实实体/场景 runtime 与十域 checksum witness 仍未运行。 | `PARTIAL / PENDING` |

## 8. 风险、回滚与未关闭项

- 风险：`SimulationWorld` 内仍可能存在未显式隔离的 Unity/global dependency；聚焦测试必须捕获而不能用 mock 掩盖。
- 风险：空 world checksum 一致不足以证明真实实体闭环；本包至少建立结构闭环，后续 S0 fixture 是否需要生产实体按证据决定。
- 未关闭项：真实实体的 in-process multi-world journal、十域 first-difference witness 与 C++ authority 对照。`BattleLockstepSessionEditorTests` 已有 9/9 screenshot evidence。跨进程/跨 runtime 等价是 S5 门槛，不是 S0 退出条件。
- 受限原因：当前 Unity Editor 持有项目 `Temp/UnityLockfile`，没有本线程可连接的 Test Runner 接口；遵守“不启动第二个写同一 Library 的 Editor”规则，未以 batchmode、新脚本或 UI automation 绕过。
- 已做但不等价：focused NUnit 5/5、existing lockstep 9/9、self-check PASS 和 fresh compile evidence 不替代正式 S0 in-process same-Kernel 十域 checksum 验收；跨进程/跨 runtime 验收属于 S5。
- 回滚方式：按 Task Contract 第 6 节，仅撤销本 Change ID 文件和 identity 只读视图调整。

## 9. Git / 交接

- 修改前工作树基线：工作树存在大量用户/历史脚本、资源、场景、文档和未跟踪文件；本包不回退、不移动、不清理它们。
- 实际 diff 范围：本 Record 第 6 节列出的四个 runtime/identity 文件、一个 Editor test 及其 `.meta`，以及配套治理文档。
- 提交 hash：未提交。
- `Tools/Validate-ChangeLedger.ps1`：待运行。
- 交接需优先阅读：本 Record、S0 Task Contract、服务器 progress Resume Card。

## 10. 2026-08-24 syntax-only compile unblock

- R8-AIROWGEN-001 fresh compile暴露本Change的Editor test两条CS0019：`tick % 4 switch`与`tick % 3 switch`
  被C#解析为`tick % (constant switch ...)`，右操作数为`SimulationInputButtons`；
- 仅允许增加显式括号`(tick % 4) switch`、`(tick % 3) switch`，不改变输入周期、期望枚举、S0 runtime或HOLD状态；
- 此修正只恢复全项目编译能力，不恢复S0 focused/self-check/multi-world验收，不晋升本Change状态；
- 已按合同只增加两处括号；当前R8 force-all中Editor DLL更新且Console全部error=0，语法错误清零；S0其余验收
  继续保持PENDING/HOLD，本Change状态不晋升。

## 11. 2026-08-24 validation-only evidence

- 用户明确允许只读、编译和运行既有 Unity S0 focused test / `BattleRuntimeSelfCheck`，同时明确禁止修改 Client 代码。
- 已确认项目由运行中的 Unity 2022.3.62f3 占用：`Temp/UnityLockfile` 存在，`Library/EditorInstance.json` 指向该 Editor；没有启动第二个实例。
- 现有 `BattleRuntimeSelfCheckEditor` request-file 被写入并由 Editor 自行消费/删除；结果文件写入 `PASS`，而 Editor.log 在 `BattleRuntimeSelfCheckEditor:RunAndWriteResult` 后记录“自检完成”。
- focused NUnit tests 是五个独立 `[Test]`：连续 48 tick journal repeat、authority-frame copy、wrong tick、barrier mismatch 与 injected checksum mismatch。self-check 源码不直接调用它们，且项目下没有新鲜 TestResults 文件。
- 本次验证没有改动本 Record 所列 Client code、Scene、资源、配置或 C++ authority；下一步只有在用户关闭/释放现有 Editor 供单实例 batch runner 使用，或在现有 Editor 手动运行该 Test Fixture 并提供结果后，才可补齐 focused evidence。
- 最终只读回查时，`Assembly-CSharp.dll` 与 `Assembly-CSharp-Editor.dll` 已进一步刷新到 17:10:28；Editor.log compile-error 窄扫描仍为 0 match，self-check result 仍是17:07:33的 `PASS`。这强化 compile/self-check 证据，但不改变 focused NUnit 尚未运行的结论。

### 2026-08-24 NUnit discoverability read-only check

- `Assembly-CSharp-Editor.csproj` 明确同时列出 `UNITY_INCLUDE_TESTS` 和 `Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs`；`com.unity.test-framework 1.1.33` 已安装，且文件没有本地 asmdef 隔离。
- 因而 test class 不可见不能归因为该源码 `#if UNITY_EDITOR && UNITY_INCLUDE_TESTS` 未满足或 Test Framework 缺失。当前只知道它在现有 Test Runner UI 中未被找到；可能是 EditMode/assembly 筛选或 discovery cache，尚无可验证的具体根因。
- 不为猜测修复而改 Test Assembly、asmdef、define 或 Client source。首先应在 Test Runner 的 EditMode 视图，以 `InProcessLockstepAuthoritySession`（不含 namespace）搜索，并检查 `Assembly-CSharp-Editor`；若仍不可见，需单实例 batch runner 产生可审计结果。

### 2026-08-24 S0 focused NUnit user-run evidence

- 用户提供 Unity Test Runner EditMode 截图：搜索 `InProcessLockstepAuthoritySessionEditorTests` 后，`AuthorityJournalOwnsFrameStorageBeforeWorldsConsumeIt`、`FirstChecksumDifferenceLatchesReplicaAndTick`、`MismatchedStartBarrierIsRejectedBeforeTickZero`、`ServerAndTwoClientsConsumeTheSameContinuousAuthorityJournal` 与 `WrongTickFailsBeforeAnyWorldAdvances` 均显示绿色通过。
- 截图右上角显示 `5` 个通过、`0` 个失败；这覆盖本 Record 的 S0 focused fixture 全部五项。
- 本线程对项目目录作只读检查时未发现 TestResults XML、`Library/TestRunner` 输出或对应 Editor.log 文字结果。因此将该截图如实记为用户提供的 Test Runner UI 证据，不伪称存在持久结果文件。
- 本次仍未改动 Client code、Scene、资源、配置、C++ authority 或 Test Assembly 配置。formal S0 in-process same-Kernel world、十域 checksum 和 C++ authority evidence仍未完成；既有 lockstep regression 已有 9/9 screenshot evidence，跨进程/跨 runtime evidence 属于后续 S5。

### 2026-08-24 Change Ledger external result

- 已运行 `Tools/Validate-ChangeLedger.ps1`；结果为 `FAILED`，但三个 error 都是本 Change 范围外的未记录 authored script diff：`Assets/NTSD/Scripts/App/BattleBackgroundPlatformSelector.cs`、`Assets/NTSD/Scripts/App/Editor/BattleBackgroundPlatformAssetEditor.cs`、`Assets/NTSD/Scripts/Test/Editor/BattleBackgroundPlatformPresentationEditorTests.cs`。
- Validator 同时提示 `CAMERA-PLATFORM-BACKGROUND-001` 的声明与当前 diff 不一致。该 Record/脚本不是本 S0 validation-only 授权范围；未修改、未清理、未补写其 Record，也不把该全仓库治理失败归因于 S0。
- 因此本 Change 的 compile/self-check 证据保持有效，但 `Tools/Validate-ChangeLedger.ps1 PASS` 仍不能作为当前 S0 交付证据；需要由该 background 平台 Change 的 owner 单独恢复治理一致性。

### 2026-08-24 Read-only S0 acceptance-coverage audit

- 上位设计 `server-lockstep-s0-s9-design.md` §5 将 S0 定义为同进程内的 server `BattleWorld` + 至少两个 client `BattleWorld`；独立进程 / 跨 runtime 一致性明确是 S5 的关闭证据，不能再把它列为 S0 的退出门槛。
- `BattleLockstepSessionEditorTests` 的 9 个既有测试覆盖单个 `SimulationTickDriver` 的 input delay、journal/replay、canonical tick checksum 与 presentation isolation；它是 S0 的回归保护，但不是 server + two-client world 的多 world 或十域证明。
- S0 focused fixture 已实际运行 48 tick、两次重复、1 server + 2 client logic-only worlds。它用 `CaptureRuntimeChecksum64` 的单一 aggregate hash 比较一致性；注入 RNG 差异的测试只能锁存 tick、replica index 和两个 aggregate hash。
- 已观察到 `BattleLockstepChecksumSnapshot.Hashes` 已有九个命名 hash：`input`、`metadata`、`rng`、`world`、`slots`、`aRest`、`vRest`、`stats`、`events`，另有 `overall` 聚合值。把这十个 checksum 值视为上位设计“十域”的实际映射是合理**推断**，但当前 S0 contract 尚未正式命名它。该 capture 源码会构造 arrays/dictionaries/JSON/SHA strings，因此只能是 mismatch 后的诊断路径，不能替代每 tick 的 0-allocation aggregate checksum。
- 当前 `InProcessBattleKernelHost` / `InProcessLockstepAuthoritySession` 不调用该 structured snapshot，且不保存 design-required 的 first differing domain、slot/generation 或 RNG witness。最小未来策略应是继续每 tick 比较 `CaptureRuntimeChecksum64`，仅在 aggregate mismatch 后为 server/replica 捕获 structured snapshots、以固定域顺序比较 `Hashes`，并新增 typed slot/generation witness；`BattleParityFrameSnapshot` 有 Authority400-only slot commitments，generic lockstep snapshot 没有 typed first-slot commitment。该 Client-code gap 不能从 aggregate-hash PASS 推导 S0 closed。
- 本段仅记录证据缺口；当前用户禁止 Client 源码修改。任何修复必须先有新的 Client Change Record、用户授权和 30 Hz/no-allocation 评审。

### 2026-08-24 Existing lockstep user-run evidence

- 用户提供 Unity Test Runner 的 EditMode 截图，筛选条件为 `BattleLockstepSessionEditorTests`。可见的九项方法均为绿色：`BufferedAndManualUseSameExplicitDriverTransaction`、`DelayTwoRequiresExplicitNeutralBootstrapAndTargetsExactFutureTick`、`DelayZeroTargetsNextSimulationTickWithoutBootstrapPackets`、`DriverFactoryConsumesConfiguredInputDelay`、`LockstepBufferedProviderNullAndLocalFallbackProviderBothFailClosed`、`MissingPacketAndWrongTickNeverAdvanceDriver`、`PresentationPublicationDoesNotChangeCanonicalTickChecksum`、`ReplayingTheSameJournalProducesIdenticalPerTickChecksums`、`ResetClearsPendingInputAndJournalCursor`。
- 截图面板同时显示 `90` passed / `0` failed。因为该数值可能覆盖更广的当前 Test Runner 运行，且项目未发现持久 TestResults XML，所以本 Record 仅把筛选后的此 fixture 记为用户提供的 **9/9 pass** 会话证据。
- 这关闭了本 Task Contract 的既有 lockstep regression 验收项；它仍不证明真实实体的 S0 multi-world journal、十域 first-difference witness、C++ authority 或 S0 `VERIFIED`。
