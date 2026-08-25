# S0-WITNESS-001 — 同进程多 world 首差 witness 与真实实体验收

<!-- CHANGE-RECORD
id: S0-WITNESS-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepAuthoritySession.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepChecksumWitness.cs
code-path: Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs
authority: User authorization on 2026-08-24; Assets/NTSD/Docs/server-lockstep-s0-s9-design.md §5 S0; C++ release live path remains the only battle-rule authority.
evidence: CODE-WRITTEN / USER-APPROVED-CLIENT-WITNESS-SCOPE / BASELINE-S0-FOCUSED-5-OF-5-PASS / BASELINE-EXISTING-LOCKSTEP-9-OF-9-PASS / SELFCHECK-PASS / COMPILE-PENDING
-->

> 创建日期：2026-08-24  
> 状态：`CODE_WRITTEN / COMPILE_PENDING`  
> 类型：S0 lockstep diagnostics / test-only validation adapter  
> 前置记录：`S0-INPROC-AUTHORITY-001`

## 1. 用户授权与目标

用户明确授权：只允许为 S0 checksum witness、first-difference 和真实实体 multi-world 测试修改 Client runtime/test 文件；必须先建立独立 Change Record；禁止 battle rules、30 Hz tick、Scene、资源、配置、S1 协议、Socket、数据库、transport 和公网功能。

本包目标是在现有同进程 1 server + 2 client world authority session 的**首次 aggregate checksum 分叉后**，保留可定位的诊断 witness，而不改变任何正常 tick 的模拟结果或热路径分配行为。

## 2. Authority 与当前事实

- S0 设计入口：`Assets/NTSD/Docs/server-lockstep-s0-s9-design.md` §5。S0 是同进程多 world；跨进程/跨 runtime 是 S5，不属于本包。
- 战斗语义 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live path。本包不改 battle pass、实体逻辑、输入语义或规则字段。
- 当前 `InProcessBattleKernelHost` 每 tick 以 `SimulationWorld.CaptureRuntimeChecksum64(...)` 记录 aggregate `ulong`；该路径必须继续保留为 fast path。
- 当前 `InProcessAuthorityDifference` 只记录 tick、replica、server/client input hash 与 aggregate state hash；无法记录 first differing domain、slot/generation、RNG state/call count 或 structured diagnostic snapshot。
- `BattleLockstepChecksumSnapshot` 已有九个命名 hashes：`input`、`metadata`、`rng`、`world`、`slots`、`aRest`、`vRest`、`stats`、`events`，以及 `overall`。把这十个 checksum 值映射为设计中的“十域”是本包待锁定的适配合同，不是新的 battle rule。
- 该 snapshot 构造 arrays/dictionaries/JSON/SHA strings，只能在首次 mismatch 后捕获；不得在每个 tick 捕获它。
- 当前基线证据：S0 focused EditMode fixture 5/5、existing `BattleLockstepSessionEditorTests` fixture 9/9、`BattleRuntimeSelfCheck=PASS`；均不等价于本包完成。

## 3. 允许范围

| 文件 | 允许职责 |
|---|---|
| `InProcessBattleKernelHost.cs` | 仅新增 mismatch-only structured lockstep snapshot capture 的内部入口；不改 tick 调用顺序、world setup、FrameInputSet 或 aggregate checksum fast path。 |
| `InProcessLockstepAuthoritySession.cs` | 仅在已有 first mismatch 分支构建并锁存 typed witness；正常一致 tick 不分配、不走诊断 capture。 |
| `InProcessLockstepChecksumWitness.cs` | 新增仅供 S0 authority session 使用的 domain-order、RNG、slot/generation 与 snapshot comparison value types/helpers。 |
| `InProcessLockstepAuthoritySessionEditorTests.cs` | 扩展 focused tests：RNG/domain witness、slot/generation witness、正常 path 不触发诊断、真实实体的 1 server + 2 clients journal 一致。 |
| 本 Record、Task、Ledger、State、Handoff、server progress | 记录实际范围、命令、证据、未验证项与回滚。 |

## 4. 明确禁止

- 不改 `SimulationWorld`、`NTSDBattleTickSystem`、battle pass、C++ 对齐逻辑、输入行为、tick 频率或 RNG 使用顺序。
- 不改 `BattleParitySnapshot.cs`，除非预检证明无替代方案；若发生该情况，先停止并追加新的批准/范围说明。
- 不改 Scene、Prefab、资源、DAT、ProjectSettings、Input Actions、UI 或 renderer。
- 不创建 S1 DTO、deadline、ACK、Jitter、Socket、transport、数据库、Server Host、Gateway、Matchmaker 或公网动作。
- 不把 aggregate-hash、编译或 focused tests 夸大为 S0 `VERIFIED`。

## 5. 实施合同

1. `CaptureRuntimeChecksum64` 保持每 tick 的唯一 normal-path checksum capture；一致帧不得构造 structured snapshot、JSON、dictionary、array 或 string witness。
2. 当已有 authority/client aggregate input 或 state hash 首次不一致时，先保留 existing tick/replica/hashes，再为 authority 和该 client 各捕获一个 `BattleLockstepChecksumSnapshot`。
3. 以固定顺序比较：`Input`、`Metadata`、`Rng`、`World`、`Slots`、`ARest`、`VRest`、`Stats`、`Events`、`Overall`；记录第一个不一致项。`Overall` 仅用于前九项均一致的异常 fallback，不能掩盖前一项的差异。
4. witness 必须无条件保留双方 RNG state/call count；当 `Slots` 首差存在时，必须记录首个不同 runtime slot 与双方 generation。无法安全得出 slot 时明确写为未知，不能伪造值。
5. witness 还必须保留 authority journal tick、client replica index、aggregate hashes 和双方 structured snapshot，以便离线诊断；随后维持既有 fail-closed 行为。
6. 真实实体 fixture 必须用三个独立 `SimulationWorld` 上的相同 test-only logic entity 设置和同 journal 驱动；不能改生产 asset/scene，且不得通过复制 Client state 让测试通过。
7. 若为实现上述任一项需要修改本 Record 未列出的 authored script，先停止并更新范围/授权；不能顺手扩张。

## 6. 验收标准

- Unity 脚本编译为 0 error，且本包新增文件被正确纳入 Editor fixture；
- 现有五项 S0 focused tests、existing lockstep 9 项和 `BattleRuntimeSelfCheck` 没有回归；
- 注入 RNG divergence 时，first witness 记录 tick、replica、`Rng` domain、双方 RNG state/call count 和两份 structured snapshot；
- 注入 slot/generation divergence 时，first witness 记录 `Slots` domain、首个 runtime slot、双方 generation 和 structured snapshots；
- 正常连续 journal 不捕获 diagnostic snapshots；预热后针对该 S0 session 的一致 fast path 不新增可观察的 managed allocation；
- 一个 test-only real-entity 的 server + 2 client worlds 使用相同 barrier/journal 连续 N 帧并保持 aggregate checksum 一致；
- `Tools/Validate-ChangeLedger.ps1` 至少针对本 Record 的 simulated path/metadata 一致；全仓既有 `BattleBackgroundPlatform*` 治理失败如仍存在，单独报告，不能收编或掩盖。

## 7. 风险与回滚

- structured snapshot 是诊断性分配路径；若它在一致 tick 触发，即视为本包失败并回退该接线。
- slot projection 的内部 shape 是当前 Unity implementation detail；只使用显式可审计的结构，不能用脆弱的字符串猜测或反射绕过类型边界。
- real-entity fixture 可能暴露既有 battle runtime 差异；记录 first witness，不以修改 battle rule 解决。
- 回滚仅删除本包新增 witness 文件、移除本包在两个 InProcess runtime 文件和 focused fixture的调用；保留本 Record、Task 与真实失败证据。不得回滚 `S0-INPROC-AUTHORITY-001`、其他用户改动或 Server 基础工程。

## 8. 当前实际状态

- 已实际修改 `InProcessBattleKernelHost.cs`、`InProcessLockstepAuthoritySession.cs` 和 `InProcessLockstepAuthoritySessionEditorTests.cs`，并新增 `InProcessLockstepChecksumWitness.cs`。没有修改 `SimulationWorld`、`BattleParitySnapshot`、battle pass、Scene、资源、配置、网络或 Server 工程。
- `InProcessBattleKernelHost` 的 structured snapshot capture 受 `currentTick` 边界检查保护，并仅由 first-mismatch branch 调用；其计数仅供 Editor fixture 确认一致 fast path 未走诊断 capture。
- 新 witness 固定比较 Input → Metadata → Rng → World → Slots → ARest → VRest → Stats → Events → Overall；它从 mismatch-only structured snapshot 中保留 RNG state/call count、首个不同 slot/generation 与双方 snapshots。
- S0 fixture新增 RNG witness、slot/generation reuse witness、real test-only character 的 1 server + 2 client 连续 journal 一致 case；尚未获得 Unity 编译或运行证据。
- 静态编译尝试：首次 `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 因受限用户目录的 `.NET` first-run sentinel 退出，未进入项目；随后使用进程级 `DOTNET_CLI_HOME` 重试。重试时生成的 `Assembly-CSharp.csproj` 尚未列入新增 `InProcessLockstepChecksumWitness.cs`，因而在两个已修改 InProcess 文件中报缺少该新类型的 `CS0246`。这只证明 Unity AssetDatabase 尚未导入新文件，**不是**对新 witness 源码的编译反证。
- 当前单实例 Unity 观察：新增 `.cs` 尚无 Unity 生成的 `.meta`，`Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 时间均早于代码写入。没有启动第二个 Editor，也没有手改 `.csproj` 或 `.meta`。
- 下一步：由当前已打开的 Unity Editor 执行一次普通 AssetDatabase Refresh/切回项目，使其导入新文件并生成 `.meta`；随后读取真实 Unity 编译结果，再运行本 S0 fixture、existing lockstep fixture 和 self-check，并如实记录结果。
