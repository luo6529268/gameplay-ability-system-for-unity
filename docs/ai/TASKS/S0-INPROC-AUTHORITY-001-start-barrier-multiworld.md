# Task Contract — S0-INPROC-AUTHORITY-001

> 状态：`FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`  
> 所属阶段：服务器帧同步 `S0`  
> 建立日期：2026-08-24

## 1. 目标

在现有 Unity 进程内建立一个权威 server world 与至少两个 client world，三者只消费同一份 canonical `FrameInputSet` 并通过同一个 `NTSDBattleTickSystem` 推进。固定 seed、session identity、rule/catalog/stage fingerprint、roster 与 policy version；连续输入 journal 下逐 tick 比较 checksum，并在第一处分叉后 fail closed。

## 2. 允许修改

- `Assets/NTSD/Scripts/Simulation/Lockstep/` 下新增 S0 启动屏障、无表现 Kernel Host 与多 world 权威会话；
- `LockstepSessionIdentity.cs` 只允许封闭 canonical player slot 数组的可变暴露；
- `Assets/NTSD/Scripts/Test/Editor/` 下新增 S0 聚焦测试；
- 本 Task Contract、Change Record、Ledger、STATE 与服务器进度文档。

## 3. 明确不做

- 不创建 `I:\GitHub\Unity_GAS\NTSD_Server` solution；独立进程属于 S5；
- 不接 Socket、序列化字节、ACK、Jitter Buffer、snapshot recovery、预测、Gateway、匹配、数据库或公网；
- 不修改 C++ release 对齐的 pass 顺序、战斗规则、DAT、资源、场景、表现和对象池；
- 不引入 `partial`、新的全局 singleton、每 tick 容器分配或第二套战斗逻辑。

## 4. 实施合同

1. `StartBarrier` 在启动前复制并冻结 identity、canonical roster、rule fingerprint 与 policy version。
2. 每个 Kernel Host 独占一个 `SimulationWorld` 和 `NTSDBattleTickSystem`；不依赖 `SimulationTickDriver` singleton、GameObject、Transform 或 Renderer。
3. authority session 先把调用者输入复制进预分配 history cell，再按 server → clients 的固定顺序消费；调用者后续修改不能改变已锁定帧。
4. tick 必须连续；wrong tick、非 canonical roster、barrier mismatch、journal 容量不足或任意 checksum 分叉均在推进前或首差处锁死会话。
5. 战斗 tick 内不得 `new` frame/list/dictionary；所有 history、journal、player storage 在 session/host 构造期预分配。

## 5. 验收

- Unity 编译 0 error；
- S0 focused EditMode tests：一个 server + 两个 clients，固定脚本连续运行并逐 tick checksum 一致；
- 同 journal 重复运行得到相同 checksum history；
- source frame 后续修改不改变 authority history；
- wrong tick、barrier mismatch 与注入 world 差异均 fail closed，且记录 first difference；
- `BattleLockstepSessionEditorTests` 等既有 lockstep tests 继续通过；
- `BattleRuntimeSelfCheck` 通过；
- `Tools/Validate-ChangeLedger.ps1` 通过。

## 5.1 2026-08-24 validation-only 实际证据

- 用户仅授权读取、编译、运行既有 focused tests / `BattleRuntimeSelfCheck`，没有授权任何 Client 源码、场景、资源或配置修改。
- 已运行 Editor 刷新后的 `Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 时间为 17:06:41 / 17:06:42，晚于 S0 源码；Editor.log 的窄扫描没有发现 `error CS*`、`Scripts have compiler errors` 或 `Compilation failed`。
- `BattleRuntimeSelfCheck` 已经由 request-file 机制执行并于 17:07:33 返回 `PASS`。
- S0 focused NUnit 已在现有 Editor 的 EditMode Test Runner 中由用户运行：五项 `InProcessLockstepAuthoritySessionEditorTests` 全绿，5/5 pass、0 fail；当前没有持久 TestResults，截图是会话证据。
- `BattleLockstepSessionEditorTests` 随后也由用户在同一 EditMode Test Runner 运行；筛选出的九项方法全绿。截图同时显示 90 passed / 0 failed，但只将可见 fixture 记为 9/9 pass，会话证据不扩大为全局测试声明。
- 只读 coverage audit：`BattleLockstepSessionEditorTests` 的 9 项只保护单个 `SimulationTickDriver` 的 input/journal/replay/checksum regression；当前 S0 fixture 虽已跑 48 tick 的 1 server + 2 client logic-only worlds，但只比较 aggregate `CaptureRuntimeChecksum64`。现有 `BattleLockstepChecksumSnapshot` 有九个命名 domain hashes（input/metadata/rng/world/slots/aRest/vRest/stats/events）及 overall；把它作为“十个 checksum 值”是待正式确认的合理推断。它是分配型诊断 capture，未来只能 aggregate mismatch 后使用。`InProcessAuthorityDifference` 仍未保存 first differing domain、slot/generation 或 RNG witness，且需要 typed slot/generation 方案。该 Client-code gap 需要新 Change Record 与用户授权；跨进程/跨 runtime 一致性属于 S5，而非 S0 退出门槛。
- 只读 discovery 检查已确认：`Assembly-CSharp-Editor.csproj` 含 `UNITY_INCLUDE_TESTS` 且显式编入该 Fixture，Unity Test Framework 已安装；先检查 Test Runner 的 EditMode / `Assembly-CSharp-Editor` 筛选，不因当前 UI 不可见而擅自改测试代码或 assembly 配置。
- `Tools/Validate-ChangeLedger.ps1` 当前因三个与 S0 无关的 `BattleBackgroundPlatform*` 未记录脚本 diff 而失败；不得在本 Task 中修复、收编或掩盖这些用户/其他 Change 工作。S0 ledger PASS 仍为外部治理缺口。

## 6. 回滚

删除本 Change ID 新增的 S0 runtime/test 文件，并恢复 `LockstepSessionIdentity` 的本次只读暴露调整；保留本 Task Contract、Change Record 和失败证据。不得回滚其他既存战斗、ECS、渲染、输入或 R8 用户改动。
