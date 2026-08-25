# R8-CENTRALSEAL-001 — central presentation activation / battle allocation seal ordering

<!-- CHANGE-RECORD
id: R8-CENTRALSEAL-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/App/AppManager.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationInitializationEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleCentralFailClosedOwnershipPlayModeProbeEditor.cs
authority: UNITY-NATIVE-ADAPTER / USER-APPROVED-R8-WP01G-R07C-R01 / B-R8-R07C-01
evidence: COMPILE0 / FOCUSED20-PASS / SELFCHECK-PASS / NORMAL-PLAY-CAMERA-ENABLED-CONSOLE0 / R07C-PASS / COMBAT1000-0GC-PASS
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：render / initialization / performance / test

## 1. 状态与范围

- 当前状态：`VERIFIED / PRODUCTION REPAIR`
- 所属 Work Package：`R8-WP01G-R07C-R01`
- 只覆盖：战斗加载期间presentation activation、首次allocation seal和重复seal的Unity-native顺序；
- 不属于本次范围：C++ gameplay、R08、AI、T8、IL2CPP、Android、服务器；
- 关联 Change ID：`R8-CENTRALOWN-001`、`R8-PERFBOOT-001`、`R6-PRES-004`。

## 2. Authority / 需求依据

- 用户于2026-08-23明确批准`R8-WP01G-R07C-R01`并恢复总目标；
- R07C final Play观察到`BeginBattleAllocationSeal→PrepareBattleCapacity→Submission.PrepareCapacity`
  在active/published submission上抛出resize异常；
- C++ renderer只定义battle render success path，不定义Unity camera生命周期、capacity预热或managed allocation seal；
- 本Change属于已批准CentralOnly/URP/0GC Unity adapter修复，不能改变战斗logic truth。

## 3. Unity 原状与已确认差异

- `BattleBootstrap`没有Awake-time disable，场景序列化为enabled的world camera可在异步加载期间开始URP render；
- `BattleTestBootstrap`和`AppManager`是互斥入口，但都在加载/实体装配前段启用presentation，之后才调用
  `BeginBattleAllocationSeal`；
- `BeginBattleAllocationSeal`即使allocation gate/runtime capacity已经sealed，仍会先重复执行presentation capacity
  prepare；
- `BattleCentralSubmission.PrepareCapacity`正确拒绝published/leased submission resize；不得削弱该保护；
- 已确认production first difference：初始化顺序允许首次capacity prepare晚于中央publication。

### 3.1 执行中修正（2026-08-23）

- 第一版在`BattleBootstrap.Awake`调用`DisablePresentation`，能够阻断提前publication，但同时关闭world/UI
  Camera与Canvas；用户在真实Play观察到Camera被禁用。该副作用不属于批准目标，第一版方案立即停止采用；
- 复核确认`BattleCentralRenderSystem.ResetRuntime()`已是正式world replace/unbind/destroy边界使用的中央
  publication清退合同；首次`BeginBattleAllocationSeal`可在presentation capacity prepare之前复用该合同；
- 修正目标为Camera始终保持场景序列化启用状态，只在首次seal同步清退旧publication；重复seal继续严格no-op；
- R07C request探针必须等待allocation gate和runtime capacity均sealed后再运行，不能在异步初始化中抢先创建
  submission。

## 4. 计划改动

| 文件 | 类型 / 方法 | 改前职责 | 目标职责 |
|---|---|---|---|
| `Assets/NTSD/Scripts/App/BattleBootstrap.cs` | presentation lifecycle | 第一版新增Awake会关闭Camera/Canvas | 移除第一版Awake禁用；Camera保持场景序列化启用状态 |
| `Assets/NTSD/Scripts/App/AppManager.cs` | `InitializeBattleAsync` | 先EnablePresentation，后装配/封印 | 装配和BeginSeal完成后才EnablePresentation，再unpause |
| `Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs` | `Start` | 先EnablePresentation，后装配/封印 | 与正式AppManager同一顺序：装配、seal、EnablePresentation、unpause |
| `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs` | `BeginBattleAllocationSeal` | 首次seal可遇到旧publication；sealed状态仍重复prepare | 首次capacity prepare前清退旧central publication；双方均sealed时严格no-op |
| `Assets/NTSD/Scripts/Test/Editor/BattlePresentationInitializationEditorTests.cs` | 新focused test | 不存在 | 验证BattleBootstrap不会在Play初始化时自动关闭Camera/Canvas |
| `Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs` | focused test | 只覆盖首次seal | 验证published submission存在时首次seal安全清退并封印，之后重复seal严格no-op |
| `Assets/NTSD/Scripts/Test/Editor/BattleCentralFailClosedOwnershipPlayModeProbeEditor.cs` | request poller | request进入Play后立即执行 | 等待driver/world/camera及allocation/runtime seal全部就绪后消费request，并保留明确超时失败 |

## 5. 不可回退边界

- 不catch/吞掉resize异常，不删除submission lease/retire保护；
- 不取消预战capacity prepare、runtime seal、managed memory battle window或0GC门；
- 不回退Legacy；保留CentralOnly/Texture2DArray/dynamic Mesh/URP；
- 保持1.5×visual scale、fixed camera、扩展容量、30Hz/FrameInputSet、SoA/ECS、pool/worker；
- C++ authority只读；不改DAT、scene、material asset、gameplay或pass order。

## 6. 实际改动

| 文件 | 类型 / 方法 | 实际改动 | 预期副作用 |
|---|---|---|---|
| `BattleBootstrap.cs` | lifecycle correction | 移除执行中第一版Awake禁用 | Camera/Canvas保持场景序列化启用状态；该文件最终无净新增生命周期逻辑 |
| `AppManager.cs` | `InitializeBattleAsync` | 删除早期Enable；BeginSeal后、unpause前Enable | additive正式入口先预热/封印容量再开始表现 |
| `BattleTestBootstrap.cs` | `Start` | 缓存bootstrap；BeginSeal后、unpause前Enable | 直接场景入口与正式入口同序 |
| `SimulationTickDriver.cs` | `BeginBattleAllocationSeal` | 首次presentation capacity prepare前ResetRuntime；双方均sealed时立即return | 旧publication先retire，再安全预热/封印；重复seal严格no-op |
| `BattlePresentationInitializationEditorTests.cs` | 新test | fixture激活后验证world/UI Camera与Canvas仍enabled、world camera binding保留 | 仅EditMode测试 |
| `BattleSimulationWorkerBoundaryEditorTests.cs` | 新test | pre-seal publish→首次seal retire→post-seal republish→重复seal | 验证首次repair与strict no-op均不破坏generation/lease |
| `BattleCentralFailClosedOwnershipPlayModeProbeEditor.cs` | request poller | request等待driver/world/camera和双seal全部就绪后才执行 | 仅测试工具；避免probe在异步初始化中自己制造first difference |

执行中第一版Awake-disable已被同一Change内的Camera-preserving修正取代；最终代码与运行证据均已完成。

## 7. 验收与证据

| 层级 | 命令 / 场景 / 输入 | 实际结果 | 状态 |
|---|---|---|---|
| 编译 | Unity full asset refresh | 2026-08-23最终refresh，Console compiler error 0 | `PASS` |
| focused test | initialization + central seal + worker/central regression | job `4cd77be4f1664b329a1e6f3b8167cfc9`，20/20 | `PASS` |
| self-check | full BattleRuntimeSelfCheck | `2026-08-23 23:13:13 PASS` | `PASS` |
| normal Play | fresh NTSD_Battle bootstrap | 运行20秒；`ScenesCamera.enabled=true`；Console0；capacity exception 0 | `PASS` |
| R07C Play | current/stale/replacement | tick214 current、215/214 stale、215 replacement；gen216→217；三态4/4/1/1、259px、hash `AE3AFF1E932B491E`；checksum/cleanup PASS；Console0 | `PASS` |
| 1000/0GC | data-oriented Combat1000 performance smoke | 30 warmup+180 sample；1000 world entities/slots；Avg/P95/Max `19.121/21.687/23.805ms`；0 B/tick、0 collection；cleanup restored | `PASS` |
| C++ full trace | R1-WP02 | blocker保持 | `BLOCKED` |

## 8. 风险、回滚与未关闭项

- 执行中第一版Camera disable已废弃；最终实现不得自动关闭Camera/Canvas；
- `EnablePresentation`必须在unpause前发生，避免逻辑已跑而camera仍disabled；
- 重复seal只在allocation gate与runtime capacity均sealed时no-op；不掩盖partial-seal不一致；
- 第一版Awake disable已经撤回，后续不得以重新禁用Camera的方式修复publication顺序；
- 回滚方式：仅回退本Change文件并标记`ROLLED_BACK`，不得回退其他用户修改。

## 9. Git / 交接

- 修改前工作树存在大量用户/历史修改与未跟踪文件；本Change不清理、不覆盖、不回退；
- 实际diff范围：`AppManager`、`BattleTestBootstrap`、`SimulationTickDriver`，2个focused test、R07C test-only probe、1个meta及留痕文档；`BattleBootstrap`第一版改动已撤回且无净diff；
- 提交hash：未提交；
- validator：`Tools/Validate-ChangeLedger.ps1`，85 records / 103 governed code files，PASS；
- 优先阅读：R07C evidence、R07C-R01 Task/Handoff、本Record。
