# NTSD S0～S9 服务器权威帧同步：执行设计与故障修复合同

> 状态：`DESIGN_READY`；尚未开始任何服务器、Socket、Gateway、Matchmaker 或公网部署代码。  
> 建立日期：2026-08-24。  
> 上位总览：[unified-battle-lockstep-ecs-server-architecture-plan.md](unified-battle-lockstep-ecs-server-architecture-plan.md)。  
> 进度、证据、阻塞与问题处置只写入：[server-lockstep-s0-s9-progress.md](server-lockstep-s0-s9-progress.md)。  
> C++ release live authority：`J:\QQFile\NTSD2.4\ntsd_release` 中参与 release 构建并运行到 `ntsd_new.exe` 的 live path。  
> `ntsd_release_C#` 仅用于历史移植意图、命名与交叉检查，不定义战斗规则。

> **能力包粒度治理补充（2026-08-31）：** 本文件继续拥有跨阶段架构、不变量、阶段顺序和退回规则；执行粒度由 Server 侧 `GOVERNANCE-S0-S9-CAPABILITY-PACKAGE-CONSOLIDATION-001` 重整为"可观察能力级工作包"（每阶段约 3～5 个；S0 为内容闭合 → 内容模型集成 → 正式 Kernel 组装 → multi-world 退出证据）。字段/Frame/OID/路径级明细作为能力包内部验收矩阵或冻结证据保留，不再作为长期 Queue 节点；历史已完成包与证据是不可重做的 prerequisite evidence。验证采用三级层级：内部检查点通过≠能力包完成，能力包完成≠阶段 `VERIFIED`，S0 退出证据不完整时不得进入正式 S1 `VERIFIED`。本补充不改变本文任何阶段边界、进入门槛、退出证据或禁止项。

## 1. 本文的用途与不可突破边界

本文是 S0～S9 的唯一详细设计合同。上位统一方案只保留总览、单机阶段和跨计划边界；本文件定义服务器阶段的每一步要解决什么问题、采用什么方案、失败时如何处置、以什么证据关闭。

> **阶段档案治理补充（2026-08-29）：** 本文继续拥有跨阶段架构、不变量、阶段顺序和退回规则；[`ServerLockstepStages/README.md`](ServerLockstepStages/README.md) 及 S0～S9 十份固定模板阶段档案承载逐阶段目标、玩家表现、数据顺序、Decision/Audit链接、实现包、边界、验收矩阵、退出条件和下一阶段移交。阶段档案是本设计的派生展开，不能覆盖或放宽本文；当前状态总账仍由 [`server-lockstep-s0-s9-progress.md`](server-lockstep-s0-s9-progress.md) 持有。

阶段入口：

| 阶段 | 独立阶段文档 |
|---|---|
| S0 | [`ServerLockstepStages/S0-formal-authority-baseline.md`](ServerLockstepStages/S0-formal-authority-baseline.md) |
| S1 | [`ServerLockstepStages/S1-authority-input-protocol.md`](ServerLockstepStages/S1-authority-input-protocol.md) |
| S2 | [`ServerLockstepStages/S2-weak-network-frame-delivery.md`](ServerLockstepStages/S2-weak-network-frame-delivery.md) |
| S3 | [`ServerLockstepStages/S3-snapshot-history-recovery.md`](ServerLockstepStages/S3-snapshot-history-recovery.md) |
| S4 | [`ServerLockstepStages/S4-presentation-prediction-decision.md`](ServerLockstepStages/S4-presentation-prediction-decision.md) |
| S5 | [`ServerLockstepStages/S5-shared-kernel-independent-host.md`](ServerLockstepStages/S5-shared-kernel-independent-host.md) |
| S6 | [`ServerLockstepStages/S6-real-transport.md`](ServerLockstepStages/S6-real-transport.md) |
| S7 | [`ServerLockstepStages/S7-public-weak-network-runtime.md`](ServerLockstepStages/S7-public-weak-network-runtime.md) |
| S8 | [`ServerLockstepStages/S8-control-plane-multi-room.md`](ServerLockstepStages/S8-control-plane-multi-room.md) |
| S9 | [`ServerLockstepStages/S9-release-capacity-operations.md`](ServerLockstepStages/S9-release-capacity-operations.md) |

服务器阶段的目标不是另写一套游戏，而是让同一套已经由 C++ release live runtime 约束的 C# `BattleKernel` 在 Unity Client 与独立 Server Host 上接受同一份 `FrameInputSet`、得到同一战斗结果。

以下边界始终成立：

- 一局战斗只存在一个权威 `BattleWorld`、一个权威 region/node、一个固定 30 Hz tick owner；多地域部署只分配不同对局，不能把同一局拆到多个地区共同计算。
- 客户端正常战斗只提交输入意图；不得上传位置、伤害、命中、HP、武器状态、opoint 结果或 AI 决策作为权威结果。
- `FrameInputSet`、历史帧、snapshot、checksum、RNG、slot/generation 与事件游标是恢复和审计事实；Unity Transform、GameObject、Renderer、动画和音频不是服务器真相。
- 已锁定的权威帧不可被迟到输入、客户端预测、重连、transport 重发或运维操作改写。
- 每个房间内部必须顺序单写；不同房间可在明确隔离后并行，但不能并发修改同一个 `BattleWorld`。
- 不使用 `DateTime.Now + Thread.Sleep + while` 作为权威战斗规则时钟；不使用每 tick 深拷贝全部引用型 Component 的快照；不把可靠 UDP 当作 ACK/Jitter/幂等协议本身。
- C++ release trace 与 Unity/Server trace 出现分叉时，立即停止该路径晋升；不能用状态包覆盖、修改历史帧或性能收益掩盖分叉。

## 2. 组件边界

### 2.0 物理目录与 solution 边界

独立服务端代码固定放在 `I:\GitHub\Unity_GAS\NTSD_Server`，与 Unity Client 仓库 `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 保持兄弟目录关系。根据用户 2026-08-24 的执行顺序调整，可以现在先创建独立 Server solution 并完成服务器内部闭环；S5 仍是它接入真实共享 BattleKernel、跨 runtime 一致并成为正式独立 ServerHost 的验收门，而不再被解释为“到 S5 才允许创建目录”。两个仓库不得复制维护两套正式 BattleKernel 或协议源码；S5 必须以版本化、Unity 可消费的共享程序集或包建立唯一源码所有权。

```text
I:\GitHub\Unity_GAS\
  gameplay-ability-system-for-unity/
    Assets/                       Unity Client；当前冻结，后续恢复跨端适配与验收
    Tools/                        诊断、转换、一次性/开发工具
    docs/                         跨阶段 Change Ledger、状态和治理记录
    Assets/NTSD/Docs/             服务器设计与进度的唯一上位合同
  NTSD_Server/                    S5 起唯一的独立生产服务端代码根目录
    NTSD.Server.sln
    global.json                   独立锁定 Server SDK，不干扰 Unity
    Directory.Build.props
    Directory.Packages.props
    src/
    tests/
    scripts/
    deploy/
    config/
    docs/
    README.md
```

创建 `I:\GitHub\Unity_GAS\NTSD_Server` 时，应将其初始化为独立 Git 仓库，并在其中建立自己的 `AGENTS.md`、`docs/ai/CHANGE-LEDGER.md`、Change Records、CI 与发布标签。Unity 仓库中的本设计和进度台账仍是跨仓库阶段事实的唯一上位入口，每个服务器 Work Package 必须在这里记录对应的 Server commit hash、包版本与未来 Unity 消费版本。不得把 Server solution 放入 `Assets/`，也不得把生产 ServerHost 伪装成 `Tools/` 下的诊断项目。

#### 2.0.1 服务器优先、客户端延后执行边界

当前执行顺序固定为：先停止修改 Unity Client，优先完成 `NTSD_Server` 内的服务器项目结构、协议/权威帧模型、房间顺序 tick owner、journal/checksum、配置日志、错误边界和纯 .NET 自动测试；待服务器内部闭环稳定后，再回到 Unity Client 建立共享包消费、客户端 adapter 和 server/client 同帧对照。

服务器优先阶段可以使用明确标注为 `TestKernel` 的确定性测试内核验证调度、锁帧、幂等、history 和错误边界，但它不能冒充正式 NTSD BattleKernel，也不能定义任何战斗结果。以下状态必须分开记录：

- `SERVER_CODE_READY`：服务器自身可以构建、测试和运行，但尚未接入 Unity/C++ 对齐后的正式战斗 Kernel；
- `CLIENT_INTEGRATION_PENDING`：Unity Client adapter、共享包和多 world checksum 尚未验收；
- `VERIFIED`：只有原阶段要求的 Server/Client/Kernel 对照全部通过后才能使用。

当前已经写入 Unity 仓库的 `S0-INPROC-AUTHORITY-001` 文件保持 `CODE_WRITTEN` 并冻结，不继续修改或用测试结果晋升；除非用户后续明确恢复客户端阶段，不得把服务器框架问题通过修改 Unity gameplay/client 代码解决。

```text
Battle.Protocol
    版本化 envelope、输入、权威帧、ACK、checksum、snapshot/recovery DTO

Battle.Kernel
    StepOneTick(FrameInputSet)、deterministic state、snapshot、restore、checksum

Unity Client Adapter
    本地输入采样、NetworkFrameBuffer、confirmed presentation、Unity 表现

Battle.ServerHost
    BattleRoom、权威帧组装、server simulation、history、snapshot、恢复

Gateway/Auth/Match/Room Allocator
    身份、队列、地区、容量、连接 token、房间分配；不拥有 BattleKernel

Transport Adapter
    实际收发字节；不拥有 tick、ACK 语义、Jitter Buffer 或 BattleWorld
```

共享程序集须维持 Unity 可使用的 API 边界；独立 Server Host 可以使用现代 .NET 运行时。BattleKernel 与协议层不得依赖 `UnityEngine`、GameObject、Transform、Renderer、场景、资源加载或具体网络库类型。

### 2.1 商业级服务端模块边界

服务器采用“模块化单体优先、按职责拆项目”的结构：先保持一个可重复构建、测试和部署的 solution，只有负载、故障域或团队边界有明确证据时才拆成独立服务。不得先以微服务数量冒充商业级质量，也不得创建无约束的 `Common` 杂物项目。

```text
NTSD_Server/src/
  NTSD.Battle.Protocol/       版本化 DTO、二进制布局、schema、错误码
  NTSD.Battle.Kernel/         StepOneTick、snapshot、restore、checksum；无 Unity/DB/网络依赖
  NTSD.Server.BattleHost/     BattleRoom、frame assembler、history、recovery、room lifecycle
  NTSD.Server.Transport/      IFrameTransport 与具体 transport adapter；不拥有战斗语义
  NTSD.Server.ControlPlane/   Gateway、Auth、Player、Matchmaker、Room Allocator
  NTSD.Server.Persistence/    数据库仓储、migration、outbox；不进入 tick 热路径
  NTSD.Server.Observability/  结构化日志、metrics、tracing、health/readiness
  NTSD.Server.Hosting/        Composition Root、配置绑定、DI、进程边界、命令入口

NTSD_Server/tests/
  NTSD.Battle.Kernel.Tests/
  NTSD.Server.Protocol.Tests/
  NTSD.Server.IntegrationTests/
  NTSD.Server.NetworkTests/

NTSD_Server/scripts/          bootstrap、build、test、run-local、publish、migration
NTSD_Server/deploy/           示例配置、容器/服务定义、环境说明；不保存 secrets
NTSD_Server/config/           无 secret 的配置模板
NTSD_Server/docs/             本地运行、客户端连接、部署、故障与运维说明
```

模块依赖必须单向：`Protocol` 与 `Kernel` 不引用 Host/DB/Transport；`BattleHost` 不引用 Unity；`ControlPlane` 只能分配房间和连接 token，不能写 BattleWorld；`Persistence` 通过接口提供数据，不让业务层泄漏 ORM 类型。跨模块身份使用明确的强类型值对象，例如 `PlayerId`、`SessionId`、`RoomId`、`NodeId`、`TickId`，不能长期以无约束的 `string`/`int` 混用。

### 2.2 数据库与持久化边界

推荐方向是在 S8 采用 **PostgreSQL 作为主关系数据库**；本地开发可用临时 PostgreSQL 容器，SQLite 只可作为不等价的本地工具/测试替代，不能作为生产一致性结论。Redis、消息队列、对象存储都属于未来可选能力，必须由测量或业务需求证明，不提前成为真相来源。

数据库负责持久化控制面与审计数据：

| 数据类别 | 是否写数据库 | 说明 |
|---|---:|---|
| 账号/外部身份、玩家资料、权限 | 是 | 凭据只保存安全哈希或外部身份引用，绝不保存明文密码/token |
| 登录 session、refresh token、封禁/审计 | 是 | token 保存哈希、过期和撤销信息 |
| 匹配 ticket、房间分配、节点注册 | 是或受控缓存 | 关系型数据是持久事实；短期队列/缓存必须可重建 |
| 对局摘要、结算、replay metadata、版本/配置审计 | 是 | 大 replay/snapshot 文件可放对象存储，数据库只保存元数据和校验 |
| 正在运行的 BattleWorld、ACK、Jitter、FrameHistory、SnapshotRing | 否 | 必须在房间内存和固定 ring 中；每 tick 访问数据库会破坏确定性、延迟和零 GC |
| 逐帧移动、位置、HP、命中、opoint、AI 决策 | 否 | 由权威 BattleKernel 计算，不能做逐 tick SQL 状态同步 |

持久化写入采用“战斗结束/明确检查点 -> outbox -> 异步持久化”的方式。数据库短暂不可用时，正在运行的房间不能因为写日志或写结算而崩溃；应记录可重试 outbox、将节点标记为降级，并按控制面策略停止新房间分配。数据库恢复后重试必须幂等。

数据库 schema 采用受版本控制的 migration。开发环境可以使用 CLI 更新；生产环境只能使用审查过的 SQL script 或 migration bundle，不能让每一个 Battle Server 进程启动时竞争性地自动改生产 schema。连接串、账号、密码、token 与云密钥不得写入 Git、普通 `appsettings.json` 或日志。

### 2.3 配置、日志、错误处理与运行契约

**配置**：使用 `appsettings.json`、环境专用配置、环境变量和受控 secret provider 的分层；定义强类型 Options，并在启动时执行格式、范围、依赖和安全校验。无效端口、缺失数据库连接、空 node id、非法容量、未知 region 或生产环境 secret 缺失时，Host 必须 fail-fast 并输出脱敏错误，不能带着半配置运行。

**日志与指标**：使用统一的结构化日志抽象，所有关键事件携带 `traceId`、`nodeId`、`region`、`roomId`、`sessionId`、`playerId`、`tick` 和 `protocolVersion` 等必要关联字段。连接、登录、开始/结束对局、进入/离开房间、恢复、节点 admission、异常与安全拒绝记录为结构化事件。移动同步、每 tick 输入和 packet 明细在生产环境不能逐条 `Information` 写日志；它们只允许进入采样 `Debug/Trace`、受限 packet witness 或 metrics，以避免日志反过来造成卡顿和费用失控。

**错误处理**：协议解析、参数校验、身份校验、限流、DB/transport 异常和房间异常必须有明确边界。非法请求只能得到稳定错误码或断开该连接，不能崩溃 Host；单房间异常必须隔离该 room 并保留恢复/审计证据，不能杀死所有房间；进程级未捕获异常须记录 fatal 证据并由外部 supervisor 重启。日志中不得输出密码、完整 token、私钥、敏感连接串或完整玩家隐私数据。

**可重复运行**：每次发布必须有固定 SDK/依赖版本、可重放的 build/test 命令、配置模板、数据库 migration 入口和健康检查。Health 分为 liveness 与 readiness：进程存活不等于可接收新房间；数据库、配置、节点注册、关键依赖和 room admission 未就绪时只能报告 not-ready。

## 3. 单慢客户端不得无限阻塞全局

NTSD 不采用“第 N 帧无限等待全部玩家输入”的纯等待式 lockstep。默认网络策略是“服务器权威帧 + 固定输入延迟 + frame deadline + 缺失输入降级 + snapshot 恢复”。

```text
客户端采样输入
    ↓ target = future server tick
InputSubmission（当前输入 + 未确认冗余输入）
    ↓
服务器在 frame deadline 前收集
    ├─ 合法输入到达：写入该 player slot
    └─ 输入缺失：写入明确 FillReason，并按 MissingInputPolicy 填充
    ↓
锁定不可变 AuthoritativeFrameEnvelope
    ↓
服务器与全部健康客户端推进同一 FrameInputSet
```

### 3.1 会话级策略

`StartBarrier` 固化并版本化以下 session-wide 参数：

| 参数 | 含义 | 当前状态 |
|---|---|---|
| `InputDelayFrames` | 客户端输入作用到未来权威帧的帧数 | S1/S2 测量后确定 |
| `FrameDeadline` | 服务器锁定某权威帧的时间边界 | S1/S2 测量后确定 |
| `GraceFrames` | 连接/恢复仍处于 grace 的上限；不得解释成战斗 held carry | 数值仍待 S2 测量和版本化配置 |
| `MaxMissingFrames` | 转入托管/断线处置前的连续缺失上限；human held carry 已固定为 0 | 数值仍待 S2 测量和模式配置 |
| `MissingInputPolicy` | 短暂缺失、持续缺失、重连与模式差异的规则版本 | S1 定义、逐模式确认 |

这些参数不能由单个客户端即时改变。若未来需要调整，只能由服务器广播新的 policy version，并指定一个全体客户端都能观察到的未来生效 tick。

### 3.2 缺失输入不允许伪造技能边沿

每个权威帧的每个 player slot 都必须标记输入来源：

```text
RealInput
DuplicateIdempotentInput
TransientMissingFill
PersistentMissingNeutral
ModeApprovedAiTakeover
Disconnected
```

任何缺失填充都不能凭空生成新的 `pressed` 边沿。用户于 2026-08-29 以原版在线证据确认：human deadline missing 的 held carry 上限为 0，当前 Tick 解析为 neutral；neutral 相对上一 locked held 可以派生一次正常 `released`，但后续 neutral 不得重复。连接 grace、PvP 结果、PvE AI ownership barrier 和 recovery 仍必须分别满足：

1. C++ release live `input_handler.cpp` 及关联 tick trace 已确认对 held/pressed/released 的真实消费语义；
2. 用户已确认相应模式的产品规则；
3. S2 弱网矩阵证明不会伪造攻击、卡住按键或让健康玩家无限等待。

在上述条件未满足前，相关选项必须标记为 `PENDING_PRODUCT_RULE`，不得由实现者自行选择。

### 3.3 长期慢客户端与服务器自身慢的区别

| 问题 | 允许影响范围 | 处置 |
|---|---|---|
| 单客户端丢包、抖动、设备卡顿 | 该客户端 | ACK/冗余、grace、neutral、恢复、重连 |
| 单客户端长期断线 | 该客户端及该模式规定的对局结果 | 服务器 snapshot + history recovery；必要时按模式结束/托管 |
| Battle Server tick 超预算 | 整个房间 | 容量告警、停止分配新房间、扩容或恢复；不得误报为单客户端问题 |

## 4. S0～S9 阶段边界矩阵

阶段不是“完成上一项后顺手扩张任何相邻功能”的许可证。每个阶段只解决本表列出的问题；发现问题属于更早层时，必须回退到对应阶段建立最小复现和修复，不能在后层增加补丁掩盖根因。

| 阶段 | 允许实施 | 明确不做 / 延后 | 进入门槛 | 退出门槛与移交 |
|---|---|---|---|---|
| S0 | 内存 loopback、server/client 多 world、StartBarrier、完整不可变 authority frame、同 journal/checksum 对照 | Socket、真实协议字节编码、ACK/Jitter、真实弱网、登录、匹配、snapshot recovery、预测 | 用户批准 S0；单机 Kernel/快照基础仍可运行 | 多 world 固定 journal 一致；向 S1 移交 session identity、roster、policy version 和 authority frame 事实 |
| S1 | transport-agnostic DTO、输入排序、future target tick、deadline、锁帧、去重、冲突/迟到错误码、FillReason | 真实收发、丢包模拟、ACK/retransmit、snapshot/recovery、预测、Gateway | S0 `VERIFIED` | 同一输入/policy 必得同一 authority history；向 S2 移交冻结消息 schema、锁帧/缺失策略和错误码 |
| S2 | 内存弱网、ACK、冗余、Jitter Buffer、ready range、有界追帧、单慢客户端降级矩阵 | 真实公网、真实网络库、server snapshot 恢复、客户端预测、房间调度 | S1 `VERIFIED` | 单慢客户端不使健康客户端无限停帧；向 S3 移交连续 authority history、ACK/confirmed cursor 与缺失状态机 |
| S3 | server snapshot、history、checksum witness、desync、重连/观战恢复的内存闭环 | 真实 Socket、跨进程部署、预测、登录/匹配、热迁服承诺 | S2 `VERIFIED` | restore + replay 十域一致，恢复客户端不回退健康客户端；向 S4/S5 移交版本化 schema、factory、恢复包合同 |
| S4 | `ConfirmedOnly` 与有限预测 A/B、确认事件去重、是否实施预测的决策 | 全局预测、远端结果预测、迟到输入改锁定帧、为了“完整”强行实现 GGPO | S3 `VERIFIED` | 记录“拒绝预测”或受控预测证据；向 S5 移交不可变 authority/recovery 合同 |
| S5 | `Battle.Protocol`/`Battle.Kernel`/`ClientAdapter`/`ServerHost` 分层、独立 headless/.NET 进程、跨 runtime 对照 | 公网监听、具体 transport 绑定、Gateway/Match、生产部署 | S4 `VERIFIED` | 进程内/跨进程/跨 runtime journal 与 checksum 一致；向 S6 移交独立 ServerHost 发布产物与协议 ABI |
| S6 | 评估并接入一个真实 transport、localhost/LAN/获授权节点连通、MTU/分片/加密接线验证 | 大规模公开测试、全国匹配、多地域调度、把库类型渗入 Kernel | S5 `VERIFIED`；公网节点授权、OS/端口/安全组信息齐全 | 真实 transport 与内存 transport 的 authority history 等价；向 S7 移交可部署 endpoint、版本和观测接口 |
| S7 | 真实公网弱网、慢客户端、断线、重连、前后台、网络切换、长时稳定 | 登录体系、公开匹配、自动多地域扩容、无感热迁服承诺 | S6 `VERIFIED`；获授权测试节点和测试客户端可用 | 健康客户端不被单慢客户端无限阻塞；向 S8 移交节点健康、网络质量、容量和恢复指标 |
| S8 | Gateway/Auth、Matchmaker、Room Allocator、节点注册、房间 admission、容量/区域策略、多房间和首批多地域 | 未经 S9 验收即公开发布；跨区域拆分同一 BattleWorld；控制面改写战斗结果 | S7 `VERIFIED`；控制面资源与运营规则已批准 | 节点失效、新房间重分配、权限、容量和区域矩阵通过；向 S9 移交发布候选、监控与演练报告 |
| S9 | 正式 Player/headless 验收、soak、故障演练、版本兼容、升级/降级、发布判定 | 新功能开发、临时绕过失败门禁、用单次 Editor 结果代替发布证据 | S8 `VERIFIED` | 所有矩阵新鲜通过；否则按问题归属退回 S0～S8，不得宣布服务器阶段完成 |

### 4.1 跨阶段问题的退回规则

```text
发现 BattleKernel / C++ trace 分叉
    → 回到 S0 或对应的 Kernel Change Record，不在 S1+ 协议层补偿

发现 frame 锁定、重复、迟到或 FillReason 错误
    → 回到 S1，不在 S2/S6 transport 层掩盖

发现慢客户端拖停健康客户端、ACK/ready range/追帧异常
    → 回到 S2，不以增加 input delay 或预测临时掩盖

发现 restore/replay/checksum 分叉
    → 回到 S3，不以状态同步覆盖修复

发现预测表现或事件重复
    → 回到 S4，默认关闭预测分支

发现跨进程/跨 runtime 差异
    → 回到 S5，不为 ServerHost 复制第二套战斗逻辑

发现实际公网 packet/MTU/分片问题
    → 回到 S6，保持 S1～S3 协议语义不变

发现真实网络恢复、慢客户端或长局问题
    → 回到 S7，保留 packet/sequence/deadline witness

发现匹配、房间分配、节点容量或地区策略问题
    → 回到 S8，禁止控制面修改 BattleWorld 结果

发现发布矩阵失败
    → 按根因回退到 S0～S8；S9 只验收，不接收功能性补丁
```

## 5. S0～S9 阶段设计

### S0：同进程权威服务器骨架

**解决的问题**：先证明“服务器世界”和“多个客户端世界”可以共用同一 Kernel，而不是先被 Socket、云部署或网络库复杂度掩盖问题。

**方案**：

- 以显式内存 loopback 创建一个 server `BattleWorld` 与至少两个 client `BattleWorld`；
- `StartBarrier` 固化 session identity、C++-aligned rule/catalog/stage fingerprint、seed、roster、canonical player slot 与 policy version；
- 所有 world 只经 `StepOneTick(FrameInputSet)` 推进；每个 server tick 都生成完整不可变权威帧，包括空帧；
- 只使用预编排输入脚本，不引入真实网络或墙钟调参。

**失败时怎么修复**：若 server/client checksum 分叉，冻结该输入 journal、记录 first differing tick/domain/slot/generation/RNG，先回到 C++ release trace 与 BattleKernel owner；禁止用 client 状态覆盖 server，禁止进入 S1。

**关闭证据**：同 seed、同 journal、重复运行下 server 与全部 client 连续 N 帧十域 checksum 一致；单机 host policy 未被改写。

### S1：应用层权威帧协议与组装器

**解决的问题**：让真实或模拟 transport 无法改变输入排序、deadline、锁帧和幂等语义。

**方案**：

- 定义 versioned、transport-agnostic 的 `InputSubmission`、`AuthoritativeFrameEnvelope`、`FrameAck`、`ServerProgress` 与稳定错误码；
- human `InputSubmission` 包含 session、connection/owned slots、client sequence、future target tick、每个 owned slot 的完整 held mask及已确认服务器进度；locked held 是唯一 canonical human input，Server/formal Kernel 从前一 locked held 派生 pressed/released并写入不可变 authority history；Client edge若未来保留只可作可选诊断 witness；
- 服务器按 `(session, target tick, canonical player slot)` 去重、排序、校验身份和 deadline；同一键的第一次合法内容胜出，冲突内容保留 witness 并拒绝覆盖；
- deadline 到达后无条件锁定完整帧，每 slot 带 `InputSource/FillReason`；迟到输入只能被确定性拒绝或接受到尚未锁定的未来 target，永不回填历史帧；
- protocol、RPC、Unity 类型与具体网络库不得进入 BattleKernel 或 `FrameInputSet`。

**失败时怎么修复**：重复、冲突、迟到或错误 roster 必须产出稳定错误码与最小 witness；先补协议 fixture，再修 owner layer；不能通过放宽锁帧或覆盖先到输入让测试变绿。

**关闭证据**：乱序、重复、冲突、迟到、空输入、roster 变更、单 slot 缺失在同一 policy version 下生成相同 authority frame history。

### S2：内存弱网、ACK 与 Jitter Buffer 状态机

**解决的问题**：短暂网络问题不应导致技能丢失、重复消费或“一人卡全员卡”。

**方案**：

- 内存 transport 可重复注入延迟、抖动、丢包、重复、乱序、短断流；
- 应用层维护 sequence、ACK、冗余输入窗口、连续 ready-frame 区间、缺帧请求、confirmed tick 和有界追帧；
- 客户端只消费连续 ready 的权威帧，不能跨洞、不能以后到包改已锁定帧；
- 加入一名客户端输入黑洞/极端抖动的固定矩阵：服务器和健康客户端按 deadline 前进，故障客户端单独进入 grace/neutral/recovery；
- `OfflineLocal` 不因网络策略改变既定的单次外层 Update 自动推进边界。

**失败时怎么修复**：若健康客户端停帧，优先检查 deadline/ready-window/ACK/填充状态机；若技能边沿丢失，检查输入冗余、target tick、canonical edge 和 C++ trace；若出现无限积压，限制 catch-up 并输出 queue/deadline witness，而不是增加无界 while。

**关闭证据**：deadline 前合法攻击/技能边沿不丢失、不重复消费；单慢客户端不使健康客户端无限停帧；无墙钟驱动的爆发式多 tick 和无界积压。

### S3：服务器权威快照、desync 与恢复闭环

**解决的问题**：客户端严重落后、checksum 分叉、断线重连和观战加入不能破坏已确认时间线。

**方案**：

- 服务器维护统一 schema/session 生命周期的 `FrameHistoryRing`、`SnapshotRing` 与 `ChecksumHistory`；
- 客户端周期报告 checksum；服务器同核结果为权威，mismatch 写入 witness 与显式状态机；
- 恢复包由服务器 snapshot、snapshot tick/checksum、连续权威帧和目标 tick 构成；客户端本地快照仅为非权威缓存提示；
- 故障客户端只恢复自身到当前 authority tick，健康客户端不回退已确认帧。

**失败时怎么修复**：restore/replay 不一致时停止恢复晋升，保留 snapshot、frame range、schema、domain hash 与 first difference；先修 snapshot schema/factory/restore owner，不允许“正常 tick 状态包覆盖”掩盖分叉。

**关闭证据**：snapshot -> mutate/desync -> restore -> history replay 后，server/client 十域 hash、slot/generation、RNG、输入历史和事件游标一致；恢复期间健康客户端持续推进。

### S4：预测与回滚决策门

**解决的问题**：不把“更灵敏的表现”误做成高风险的全局逻辑预测。

**方案**：比较 `ConfirmedOnly`、本地即时表现反馈、输入回显与有限本地玩家预测的延迟、错预测率、回滚成本和用户体验收益。默认保持 `ConfirmedOnly`；仅当 S3 恢复窗口、C++ trace、事件去重和性能预算均证明安全时才引入有限预测。

**失败时怎么修复**：任何预测导致权威帧回写、远端结果提前确认、声音/特效重复、RNG/slot 分叉或健康客户端等待慢客户端，立即关闭预测分支并回到 `ConfirmedOnly`；不把预测当作必须完成的功能。

**关闭证据**：要么以实测证明不需要预测并关闭，要么有限预测通过 snapshot/replay、确认事件游标和 C++-aligned trace 门禁。

### S5：共享程序集与独立进程门禁

**解决的问题**：证明 Battle Server 不依赖 Unity Editor、场景或表现对象。

**方案**：按 2.1 的单向模块边界拆分 `Battle.Protocol`、`Battle.Kernel`、`ClientAdapter`、`BattleHost`、`Transport`、`Observability` 与 `Hosting`；Server Host 使用 headless/.NET 进程，复用同一 schema、factory、checksum、snapshot 与 `StepOneTick`。本阶段建立统一结构化日志、强类型启动配置/Validate-on-start、错误边界、liveness/readiness health endpoint、固定依赖/SDK 版本和最少可运行命令，但不接入生产数据库或公网控制面。

**失败时怎么修复**：若进程内与独立进程不同，优先检查序列化默认值、字节序、schema、随机种子、文化区、浮点/整数域与 Unity 残留依赖；禁止为服务器单独复制战斗规则。

**关闭证据**：进程内与独立进程、Mono/IL2CPP/Server runtime 的固定 journal、snapshot、restore/replay 和 checksum 一致；`restore -> build -> test -> run-local -> health` 的命令链在干净环境可重复执行，错误配置 fail-fast，单个异常请求不导致 Host 退出。

### S6：真实 transport 选择与接入

**解决的问题**：将已验证的应用层协议映射到真实公网收发，而不让网络库篡改逻辑语义。

**方案**：S0～S5 全部关闭后，按移动端支持、可靠/不可靠通道、MTU、分片、拥塞、加密、维护状态、许可证与部署能力评估 UDP/KCP/ENet/LiteNetLib 或其他实现。控制面可用 HTTPS/TLS；战斗数据面以低延迟 transport 为主，但 ACK/Jitter/锁帧仍在应用层。

**失败时怎么修复**：transport 问题先以同一 authority journal 与内存 transport 对照，定位为字节收发、分片、MTU、拥塞或协议适配；不得修改 BattleKernel 以适应某个库。需要更换库时复用 S1～S3 消息合同与弱网矩阵。

**关闭证据**：localhost/局域网/获授权公网节点上的真实 transport 与内存 transport 在同一输入/弱网脚本下得到相同 authority history 与 checksum witness。

### S7：真实弱网、断线重连与长时稳定性

**解决的问题**：验证理论协议在真实公网、移动网络和设备卡顿时仍遵守“单慢客户端不拖全局”。

**方案**：在获授权公网节点上测试延迟、抖动、丢包、重复、乱序、短断流、长断线、前后台、网络切换、客户端进程重启和长局历史截断；持续记录 fault slot、grace/neutral/recovery 时刻、confirmed tick、带宽、P50/P95/P99 和恢复时长。

**失败时怎么修复**：真实网络与内存矩阵不一致时，不把问题归咎于“网络不可控”；保存 packet/sequence/ack/deadline trace，先用可重放 packet fixture 复现，再修 transport adapter 或 protocol state machine。客户端无法恢复时只影响该客户端，不能停止健康客户端推进。

**关闭证据**：单持续慢客户端、输入黑洞、设备卡顿、短/长断线均不让健康客户端无限等待；BattleKernel/协议热路径无非预期分配与 Gen0/1/2 collection。

### S8：控制面、多房间、容量、安全与多地域

**解决的问题**：让不同对局能够安全、可观测地分配到不同节点和地区，而不把登录/匹配塞进 BattleKernel。

**方案**：建立 `Gateway/Auth -> Matchmaker -> Room Allocator -> Battle Server`，并引入 2.2 的 PostgreSQL 持久化边界、版本化 migration、幂等 outbox、玩家/会话/房间/对局/节点数据模型。控制面只处理身份、连接 token、队列、地区/延迟策略、roster、容量与房间分配；Battle Server 只运行分配给自己的房间。初期允许单地域获授权公网节点，后续按延迟、容量与故障域增加 region pool，客户端始终先连接稳定 Gateway 域名。

**失败时怎么修复**：节点超预算、room admission 错误、token 越权或区域选择不公平时，冻结新房间分配、保留 node/room/session 证据并在 control-plane 修复；不迁移正在进行的权威 BattleWorld 来掩盖错误。新对局可重分配，进行中对局按 S3/S7 恢复策略处理。

**关闭证据**：多房间常规/极限负载、单房间高实体负载、节点失效、新房间重分配、权限和审计矩阵均通过；数据库 migration、连接失败降级、outbox 幂等重试、敏感配置保护、玩家/房间参数校验、结构化日志关联查询均通过；server tick 超预算与单客户端缺失输入使用不同指标和不同处置策略。

### S9：最终验收与发布门

**解决的问题**：避免把局部网络测试、Editor 成功或单个 checksum 当成生产可发布结论。

**方案**：执行确定性、协议兼容、弱网、恢复、长时 soak、进程崩溃、容量、升级/降级和多地域矩阵；使用目标 Player/headless 构建，不用 Editor 或 simulation-only 结果替代。发布候选必须提供版本锁定的依赖安装、build、test、local run、health、migration、publish 命令以及最小运行说明，CI 必须实际执行其中的 build/test/migration dry-run 路径。

**失败时怎么修复**：任何一项失败都不能发布该版本。将问题登记为独立 issue，附 input journal、authority frame history、snapshot、checksum/domain first difference、网络/资源指标和最小复现；回到所属 S 阶段修复，重新执行该阶段及所有后继阶段的必要门禁。

**关闭证据**：Windows Mono/IL2CPP、Server runtime 与用户提供的 Android 真机证据满足同一固定 journal、snapshot、restore/replay 和 checksum 合同；持续慢客户端、短断流、长断线、服务器超预算四类故障均有明确处置与报告。运行说明可让新环境按命令安装依赖、编译、迁移、启动 ServerHost、查询 health，并让 Unity Client 连接到指定 local/public endpoint；T8 默认 `stage.dat` 仍按项目边界排除。

## 6. 商业级质量门与交付物

“商业级”不是一个口头标签。服务器阶段不得在缺少下列交付物时声称达到商业可维护质量：

| 质量项 | 最低合同 | 阶段 |
|---|---|---|
| 清晰结构 | 2.1 的模块边界、单向依赖、强类型 ID、无 Unity/DB/transport 泄漏到 Kernel | S5 |
| 类型与参数约束 | protocol schema version、最大长度、范围验证、stable error code、启动 Options 校验 | S1/S5 |
| 统一日志与指标 | 关联字段、级别、采样、高频日志限制、fatal/room exception 证据 | S5/S8 |
| 配置与 secrets | 示例配置、环境覆盖、secret 不入库、启动 fail-fast | S5 |
| 异常隔离 | 请求、connection、room、process 四层错误边界；健康检查与 supervisor 接线 | S5/S7 |
| 数据库 | PostgreSQL schema、migration、事务/约束、outbox、幂等、备份/恢复和最小权限 | S8/S9 |
| 重复构建运行 | 固定 SDK/依赖、`bootstrap`、`build`、`test`、`run-local`、`publish`、`migrate` 命令 | S5/S9 |
| 最小运行说明 | 本地 ServerHost 启动、health、日志位置、Unity Client endpoint、测试账号/开发模式说明 | S5/S9 |
| 部署与扩展 | node registration、容量 admission、多房间、region、灰度/回滚、故障演练 | S8/S9 |

计划中的命令接口如下；实际文件和参数必须在 S5 创建后由 CI 运行验证，不能只在文档中示例化：

```text
NTSD_Server/scripts/bootstrap.ps1 / bootstrap.sh   安装/验证 SDK 与依赖
NTSD_Server/scripts/build.ps1     / build.sh       构建 Server solution
NTSD_Server/scripts/test.ps1      / test.sh        运行 Server/Protocol/Integration tests
NTSD_Server/scripts/run-local.ps1 / run-local.sh   启动本地依赖与 ServerHost
NTSD_Server/scripts/migrate.ps1   / migrate.sh     开发 migration 或受控部署 migration
NTSD_Server/scripts/publish.ps1   / publish.sh     生成部署产物
```

本地运行说明至少必须包含：前置 SDK、依赖启动方式、配置模板、数据库准备、启动命令、`/health` 或等价健康端点、日志位置、Unity Client 指向 `127.0.0.1` 或获授权 endpoint 的步骤、停止与清理方式。生产说明还必须分开写明 migration 审核、secret 注入、端口/安全组、备份、回滚和节点滚动升级。

## 7. 问题发现后的统一修复流程

任何 S0～S9 问题必须按以下顺序处理，不能边猜边改、不能把异常吞掉。

```text
发现异常
    ↓
冻结证据与最小复现
    ↓
分类：规则 / 协议 / 弱网 / 恢复 / runtime / 容量 / 控制面
    ↓
停止该路径晋升，保留 last-known-good 路径
    ↓
建立 Issue 条目与（如涉及脚本）Change Record
    ↓
最小 owner-layer 修复
    ↓
运行本阶段门禁 + 所有受影响后继门禁
    ↓
追加记录证据、风险、回滚与状态
```

### 7.1 必须冻结的证据

| 问题类型 | 最小证据包 |
|---|---|
| C++/Unity/Server 战斗分叉 | C++ release trace、input journal、first differing tick/domain、RNG、slot/generation、authority frame history |
| 输入/协议错误 | envelope、session/policy version、sequence、ACK、deadline、InputSource/FillReason、稳定错误码 |
| 弱网/Jitter 问题 | 注入脚本或 packet trace、ready range、confirmed tick、queue depth、catch-up 计数 |
| snapshot/replay 错误 | snapshot schema/checksum/frame、history range、restore/replay checksum、factory witness |
| runtime/进程差异 | runtime version、平台、序列化字节、culture、seed、assembly hash、first difference |
| 容量/卡顿 | room/node id、tick P50/P95/P99、CPU、内存、GC、队列、带宽、活动房间和实体数 |
| 控制面错误 | token/roster/version（脱敏后）、region decision、node admission、room allocation 证据 |

### 7.2 修复纪律

- 战斗规则疑问先回到 C++ release live path 与同 tick trace；C# 与 Unity 现状不能裁决规则。
- 已锁定 authority frame、已确认 checksum witness 和原始网络包不得被测试修复覆盖或删除。
- 先修拥有该状态的唯一 owner；不得在客户端、服务器、表现层各加一个补丁形成三套真相。
- 代码改动前，按根 `AGENTS.md` 建立 `docs/ai/CHANGE-RECORDS/<ChangeId>.md` 与 Ledger 条目；脚本改动后运行 `Tools/Validate-ChangeLedger.ps1`，并在本文件对应阶段的进度条目中链接 Change ID。
- 若新路径与 old path 不同，先保留 old path 作为可诊断 fallback；只有 authority trace、自动门禁和目标 runtime 都通过后才移除。
- 未确认的 C++ held/pressed/released 或模式产品规则必须写为 `PENDING_PRODUCT_RULE`，不得伪造为“通常游戏都这样”。

## 8. 进度与留痕规则

本文件只在“设计合同改变”时更新。以下内容不得混入本文件的阶段正文：每次命令输出、临时问题、单次性能数字、Change Record 全文和日常工作日志。

这些内容写入 [server-lockstep-s0-s9-progress.md](server-lockstep-s0-s9-progress.md)：

- 当前阶段状态与下一步；
- 已获得的编译、测试、Player、C++ trace 和公网证据；
- 阻塞项、故障 ID、处理状态和风险；
- 每个实际脚本 Change ID 的链接；
- 版本化 policy/协议/schema 的生效与 supersede 关系。

进度记录采用追加式事实：旧结论错误时新增 correction/supersede 条目，不删除旧事实。任何阶段只有满足本文件“关闭证据”并在进度文档记录新鲜证据后才能标记完成。
