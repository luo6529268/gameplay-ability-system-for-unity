# NTSD S0～S9 服务器权威帧同步：进度、证据与问题台账

> **最新 Server-first 状态同步（2026-08-25，优先于下方旧的“最近完成”表述）：** `S0-SERVER-BOOTSTRAP-NODE-IDENTITY-001 = FOCUSED_TEST_PASS / SERVER_BOOTSTRAP_NODE_IDENTITY_READY / S0_SERVER_FIRST_CORRECTION / CLIENT_PAUSED` 与 `S1-SERVER-POLICY-VERSION-VALUE-001 = FOCUSED_TEST_PASS / SERVER_POLICY_VERSION_VALUE_READY / S1_SERVER_FIRST_PREIMPLEMENTATION / CLIENT_PAUSED` 已关闭。前者使 Protocol-owned `NodeId` 成为本地 bootstrap/health 的有效身份事实；后者使既有 Model B/C1 的 `PolicyVersion` 成为 Protocol/BattleHost 强类型，并保留 `InputSubmission` 不含 policy field、activation/journal/ACK/ready 行为及已形成 `TargetTick`/`InputDelayFrames` 语义不变。两包均有 test-first、focused、Debug/Release 十项目 `0/0`、full Server tests、no-network local host、declared-path audit 和最终 Server workflow/Ledger 证据；它们均不闭合 formal S0/S1/S2，不授权 Client、wire、transport、snapshot/recovery、rebarrier、missing-input 或 battle-rule 修改。最新 Server 选择审计为 [`S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md)：当前无活跃/READY 源码包，但 Server-first 总目标仍 active；只等待命名的 `S-PROTO-001`、`S-NET-001/002` 或 Client/formal-Kernel/S3/S5 gate，而非再次请求泛化 Server 授权。
> 状态：最近完成`S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001 = FOCUSED_TEST_PASS / SERVER_POLICY_ACTIVATION_SCHEDULE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION`；用户已确认Model B，Server room/session以target authority tick解析future-effective policy activation并把resolved version写入immutable locked envelope history，`InputSubmission`仍不带per-submission `PolicyVersion`。它不选择capture、missing-input、AI、Kernel、Client、wire、InputDelay、rebarrier或recovery行为。前置 S1 tick/future bound、S2 redundancy-ingress capacity、disorder/gap/redundancy/ready buffer/ACK 与其他packages也均为 focused-test ready。本台账只追踪服务器阶段；它不把单机 U0～U9、C++→Unity 重新对齐、T8 默认 `stage.dat` 或 Android 真机任务误写为服务器已完成。  
> 最新 C1 状态（2026-08-25，优先于上句“最近完成”历史措辞）：`S2-SERVER-CROSS-POLICY-ACTIVATION-JOURNAL-001 = FOCUSED_TEST_PASS / SERVER_CROSS_POLICY_JOURNAL_READY / CLIENT_PAUSED / S2-PREIMPLEMENTATION`。用户确认的Server-only C1实现activation journal独立cursor/ack、next-tick resolved `ServerProgress.PolicyVersion` 与acknowledged-prefix cross-policy gap/ready guard；test-first red、Debug/Release十项目`0/0`、full Server tests、no-network local host、declared C1 audit和final Ledger`24 / 78`均通过。它不闭合S2/S3，且不授权Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input工作。  
> 缺失输入决策状态（2026-08-25，只读）：Server [`PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md) 现明确 S-NET-001/002 仍需用户确认；现有 logical deadline、caller-owned policy interface 和 source/reason provenance 只证明机制，不选择 grace/max-missing、payload、neutral/carry、AI、disconnect/reconnect 或 mode 行为。该卡不授权任何 Client 或 Server 源码。  
> 持久执行流程（2026-08-25）：Server [`S0-S9-EXECUTION-WORKFLOW.md`](../../../../NTSD_Server/docs/ai/S0-S9-EXECUTION-WORKFLOW.md) 与 [`S0-S9-NEXT-PACKAGE-QUEUE.md`](../../../../NTSD_Server/docs/ai/S0-S9-NEXT-PACKAGE-QUEUE.md) 是未来会话的唯一选包入口。每次先选最早 READY row；局部 GATED/DEFERRED 只阻断对应包，不得使整个 Server-first 目标失忆或被泛化暂停。  
> 持久流程验证（2026-08-25）：Server GOVERNANCE-S0-S9-EXECUTION-WORKFLOW-001 已写入并验证只读 [`Validate-S0S9ExecutionWorkflow.ps1`](../../../../NTSD_Server/scripts/Validate-S0S9ExecutionWorkflow.ps1)。它检查 queue/anchor 一致性、最多一个 ACTIVE 和 no-READY 自洽性；每次 queue/交接更新后必须运行。它不代表任何 battle 或阶段 VERIFIED 证据。  
> 详细设计合同：[server-lockstep-s0-s9-design.md](server-lockstep-s0-s9-design.md)。  
> 上位总览：[unified-battle-lockstep-ecs-server-architecture-plan.md](unified-battle-lockstep-ecs-server-architecture-plan.md)。  
> Server 证据矩阵：`NTSD_Server/docs/ai/AUDITS/S0-S9-FORMAL-READINESS-MATRIX-001.md`（只读分析，不等于阶段验证）。  
> S1 输入 payload 前置审计：`NTSD_Server/docs/ai/AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md` 已确认现有generic frame/window只保证结构不可变，opaque `TInput`的value/deep-copy semantics仍是formal input-contract gate；不授权通用 clone/copy/serializer。  
> S1 formal FrameInputSet shape 前置审计：`NTSD_Server/docs/ai/AUDITS/S1-FORMAL-FRAME-INPUT-SHAPE-PREREQUISITE-001.md` 已确认C++ release-live input是七个logical action加runtime-derived edge/history/cooldown和Kernel-owned AI；不授权把SDL/Unity binding、AI、history或post-input state写成Client payload。  
> S5 异常边界前置审计：`NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md` 已确认当前 generic kernel/policy throw 没有可安全推断的 journal/world 原子提交或恢复语义；不授权局部 catch/retry/rollback。  
> S5 single-writer room actor 前置审计：`NTSD_Server/docs/ai/AUDITS/S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md` 已确认`SequentialSingleWriter`目前只是bootstrap metadata，尚无room actor/mailbox/queue或并发顺序证据；不授权通过临时lock/queue跳过S5 Host合同。  
> 最近完成 Server 包：`S2-SERVER-ACK-READY-GAP-TICK-RANGE-001 / FOCUSED_TEST_PASS / SERVER_ACK_READY_GAP_TICK_RANGE_READY / CLIENT-PAUSED / S2-CORRECTION`；它将 ACK/gap/non-empty ready-range的frame fact收紧为addressable tick，要求ServerProgress精确successor，并保留terminal empty ready range。Debug/Release `0/0`、focused/full Server tests、no-network local run、declared-source audit与final Ledger`21 / 70`均通过；不改Client、transport、retention、policy、payload、Kernel或阶段验证。  
> 最后更新：2026-08-25。

## 0. Resume Card：上下文压缩后的唯一恢复入口

任何新对话、上下文压缩、执行者切换或中断恢复，都必须先以本卡片和工作树为准，不能依据聊天摘要、记忆、旧 comment 或“上次好像做到这里”的推测继续。

| 字段 | 当前值 |
|---|---|
| 持续目标 | Server-first 持续推进；Client 仅暂停，不等于总目标暂停 |
| 当前服务器阶段 | `S0` formal close 仍待；同时进行用户目标授权的 `S1` Server-only preimplementation，绝不标记阶段 `VERIFIED` |
| 阶段状态 | `S0_SERVER_ROOM_JOURNAL_READY / S0_FORMAL_CLIENT_PROOF_DEFERRED / S1_PREIMPLEMENTATION` |
| 当前 Work Package | 无源码包活跃；最近完成为`S1-SERVER-POLICY-VERSION-VALUE-001 / FOCUSED_TEST_PASS / SERVER_POLICY_VERSION_VALUE_READY / S1_SERVER_FIRST_PREIMPLEMENTATION / CLIENT_PAUSED`。它将既有 Model B/C1 policy identity 收束为 Protocol/BattleHost 强类型，同时保留 `InputSubmission` 无 policy field、activation journal/cursor/ack、next-tick `ServerProgress` 和 acknowledged-prefix gap/ready 行为。test-first、focused、Debug/Release十项目`0/0`、full Server tests、no-network host、declared-path audit和final workflow/Ledger`31 / 51`均通过；不是formal S1/S2闭合。 |
| 当前 Change ID | 无活跃 Server Change ID；最近关闭为`S1-SERVER-POLICY-VERSION-VALUE-001`，此前`S0-SERVER-BOOTSTRAP-NODE-IDENTITY-001`也已关闭。Client `S0-WITNESS-001` 保持既有 `CODE_WRITTEN / COMPILE_PENDING`，用户当前暂停其任何动作。 |
| 下一项允许动作 | [`S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md) 已确认当前没有可凭空扩展的 Server-only 源码包。下一触发是用户确认/修订 formal input `S-PROTO-001`（最早队列行），独立地确认`S-NET-001/002`，或取得 Client/formal-Kernel/S3/S5 的命名 gate；收到后直接将对应 queue row 标为 READY、建立独立 Task/Change Record 并实施，不再询问泛化 Server 范围。 |
| 当前外部阻塞 | S0 formal close 的 Client multi-world/ten-domain proof deferred；S2 formal close仍需真实 Client 连续消费、单客户端黑洞/极端抖动矩阵和用户批准的 grace/neutral/recovery 行为；S3/S5 又需 formal Kernel（当前 marker 为 false）。C++ release 只读审计确认其 `InputHandler::snapshot()` 是按键快照、`snapshot_phase210_table()` 是 UI/结算表，且 live RNG 为跨 input/game tick/collision/frame advance 的 global LCG，因此 formal snapshot 不能由 generic frame list 替代；字段级 inventory 见 `NTSD_Server/docs/ai/AUDITS/S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md`。此外，sequence/history retention仍需版本化决定，而`S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001`确认`ProtocolVersion=1`尚非有协商/ABI/rolling-upgrade/replay-supersede合同；三者均需formal/product/transport范围后才能实施。这些限制阶段 verified claim 和新源码范围，但不暂停 Server-first goal。C++ full-trace 观察链路仍为独立范围边界 |
| 不允许做的事 | 不修改/编译/测试 Client；不接公网、不把 TestKernel 当正式 battle Kernel、不自行选择 neutral/carry/AI/断线规则、不宣称 S0/S1 `VERIFIED` |
| 最新服务器验证 | 除既有 S0 room/journal、S1/S2 generic Server-only 覆盖外，`S0-SERVER-BOOTSTRAP-NODE-IDENTITY-001` 已使本地 host 输出`NodeId=local-node / BootstrapReady / Liveness=True / Readiness=True / NetworkListenerStarted=False`；`S1-SERVER-POLICY-VERSION-VALUE-001` 已通过 Model B/C1 Protocol/BattleHost 回归。两包的 Debug/Release 十项目均为`0 warnings / 0 errors`，Protocol/BattleHost/Architecture/Integration self-hosted Server tests、no-network local run、source audit 和最终 workflow/Ledger`31 / 51`均通过。它们都不是 Client/runtime/C++ battle 对齐或 S0/S1/S2 `VERIFIED`。 |

持续目标的强制暂停条件：若下一项实现必须修改、编译、运行或验证 Unity Client，先在本台账新增 `CLIENT_INTEGRATION_REQUIRED` 条目，列出需要修改的文件、接口、原因、服务器侧已有证据和不修改客户端无法继续的具体门槛；随后停止实施并等待用户明确批准。不得借“共享代码”“顺手验证”或“只有一行”绕过该边界。

### 0.1 每次恢复时必须执行的读取顺序

```text
1. 读取根 AGENTS.md 与适用子目录规则
2. 读取 docs/ai/STATE.md，确认全项目当前主线和活跃 Change ID
3. 读取本文件的 Resume Card、阶段总览、开放决策和问题台账
4. 读取 server-lockstep-s0-s9-design.md 中当前阶段及其前置阶段
5. 若 Current Change ID 非空，完整读取对应 docs/ai/CHANGE-RECORDS/<ChangeId>.md
6. 运行 git status 与 scoped git diff，确认工作树真实状态
7. 若存在脚本改动，运行 Tools/Validate-ChangeLedger.ps1
8. 只执行 Resume Card 中写明的“下一项允许动作”
```

如果任一文档、Change Record、Ledger 与工作树互相矛盾：

```text
工作树和实际测试结果优先于聊天摘要
    ↓
将阶段状态降到不夸大的最近真实状态
    ↓
追加 correction / issue 条目
    ↓
重新建立最小验证，不得直接继续后续阶段
```

### 0.2 原子状态更新顺序

服务器实施期间，每个实际代码包都必须遵守以下原子检查点。任何中断都只能发生在已写入的检查点之后，后续执行者据此恢复。

```text
批准实施
    ↓
Change Record = PLANNED
    ↓
Ledger + 本 Resume Card 写入 Work Package / Change ID / 下一步
    ↓
开始修改代码
    ↓
立即将 Record 更新为 CODE_WRITTEN，并列出实际改动文件与未验证项
    ↓
编译 / focused test / runtime / 公网验证
    ↓
每获得一层真实证据，更新 Change Record、Ledger、进度条目和 Resume Card
    ↓
只有全部关闭证据齐全，阶段才能从 RUNTIME_PENDING 升至 VERIFIED
```

不得把多次代码修改攒到对话结束再一次性补写记录；否则上下文压缩、崩溃或人员切换后无法判断哪些代码已经写入、哪些测试尚未执行。

### 0.3 中断、崩溃或上下文丢失时的保守规则

| 恢复时观察到的事实 | 必须采取的动作 |
|---|---|
| 工作树有脚本 diff，但 Change Record 仍是 `PLANNED` | 停止继续修改；先审查 diff，补充实际文件和状态为 `CODE_WRITTEN`，再决定验证 |
| Change Record 是 `CODE_WRITTEN`，没有测试证据 | 不得声称编译或行为正确；从最窄编译/聚焦测试重新开始 |
| 测试结果存在，但路径、程序集时间或版本不清楚 | 视为证据不足，刷新/重建后重跑相关门禁 |
| 进度文档写 `VERIFIED`，但工作树/Record/证据缺失 | 立即降级为最近可证明状态并追加 correction，不伪造完成 |
| 当前阶段与前置阶段状态冲突 | 回退到最早未验证的阶段；后继阶段不得继续 |
| 发现新增用户改动或未知文件 | 视为用户工作，保留不动；将服务器任务收窄到可隔离范围或请求方向 |

## 1. 状态定义

| 状态 | 含义 |
|---|---|
| `NOT_STARTED` | 只有设计，不存在该阶段代码或执行证据 |
| `DESIGN_READY` | 设计、输入输出、验收和故障路径已冻结，等待实施 |
| `IMPLEMENTING` | 已建立该阶段的实际 Work Package / Change Record，正在实现 |
| `CODE_WRITTEN` | 代码存在，尚未完成编译或最窄门禁 |
| `COMPILE_PASS` | 编译通过，尚未完成协议/运行时/跨端证据 |
| `FOCUSED_TEST_PASS` | 聚焦自动门禁通过，尚未完成目标 runtime 或公网验证 |
| `RUNTIME_PENDING` | 代码与自动门禁通过，但缺 Player、Server、C++ trace 或公网证据 |
| `BLOCKED` | 已记录重复出现的外部阻塞，不能安全继续 |
| `VERIFIED` | 本阶段设计合同要求的最新证据齐全 |
| `SUPERSEDED` | 已被后续版本化设计或实现替代，保留历史 |

`VERIFIED` 不等于 S0～S9 全部完成；只有所有前置阶段也均为 `VERIFIED`，后继阶段才可开始正式验收。

## 2. 服务器阶段总览

| 阶段 | 当前状态 | 解决的问题 | 当前阻塞 | 进入条件 | 关闭证据 |
|---|---|---|---|---|---|
| S0 | `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED` | 同进程权威 server/client 同核 | TestKernel 不能替代 formal Kernel；same-process ten-domain witness 和 C++ evidence仍缺；跨进程/跨 runtime 是 S5 | 获得 Client 代码批准后建立 witness 独立 Change Record，再继续 formal S0 integration | 多 world journal/checksum 一致 |
| S1 | `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS` | 应用层输入、锁帧、deadline、幂等 | 依赖 S0；当前还缺 formal input payload value/capture/equality/hash/serialization contract，以及`S-PROTO-001` edge ownership确认 | S0 `VERIFIED` | authority frame history 完整一致 |
| S2 | `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS` | ACK、冗余、Jitter、单慢客户端不拖全局 | 依赖 S1；Server-only ACK/ready/gap/redundancy/disorder containers已各自通过，仍非真实Client弱网证据 | S1 `VERIFIED` | 内存弱网矩阵通过 |
| S3 | `NOT_STARTED` | server snapshot、desync、重连恢复 | 依赖 S2 | S2 `VERIFIED` | restore/replay 十域一致 |
| S4 | `NOT_STARTED` | 是否需要有限预测/回滚 | 依赖 S3 | S3 `VERIFIED` | 预测拒绝或受控通过 |
| S5 | `NOT_STARTED` | 独立 .NET Server Host | 依赖 S4；当前还缺 formal Kernel atomic step、journal commit/recovery、versioned fault witness 与 room/process isolation 合同 | S4 `VERIFIED` | 跨进程/跨 runtime 一致 |
| S6 | `NOT_STARTED` | 真实 transport 选择与接入 | 依赖 S5、获授权环境 | S5 `VERIFIED` | 真实 transport 与内存协议等价 |
| S7 | `NOT_STARTED` | 公网弱网、断线、重连、长时稳定 | 依赖 S6、获授权公网节点 | S6 `VERIFIED` | 单慢客户端不拖全局的公网证据 |
| S8 | `NOT_STARTED` | Gateway、匹配、房间调度、多房间、多地域 | 依赖 S7 | S7 `VERIFIED` | 控制面、容量、节点故障矩阵通过 |
| S9 | `NOT_STARTED` | 发布级完整验收 | 依赖 S8 | S8 `VERIFIED` | 确定性、弱网、容量、升级与故障矩阵完成 |

## 3. 当前事实与边界

- 当前没有正式 `Battle.ServerHost`、Gateway、Auth、Matchmaker、Room Allocator、Socket server 或真实 transport 的实现证据；现有 `NTSD.Server.Hosting` 仅是 no-network bootstrap host。
- 独立服务端代码根目录固定为 `I:\GitHub\Unity_GAS\NTSD_Server`，其 .NET solution 已按 Server-first 顺序提前建立。S0～S4 的 formal shared-Kernel/cross-world 合同仍必须在 Unity 与 Server 共同验证，不能由该提前目录创建替代。
- 2026-08-24 用户调整执行顺序：先完成独立服务端脚本，Unity Client 改动冻结；独立 Server solution 可以提前建立，但 S0～S5 原有跨端退出门槛不因此自动完成。
- S0～S5 应先在本机完成，不需要公网 IP、第二台电脑或云服务器。
- 已获授权的腾讯公网地址 `129.204.124.151` 和华为云公网地址 `124.71.139.127` 仅登记为未来 S6/S7 可用测试环境候选；未对它们进行端口扫描、登录、部署或配置修改。
- 是否使用、使用哪台、区域、操作系统、资源配置、端口、安全组、访问方式与长期授权仍应在进入 S6 前由资源所有者确认。
- 一局战斗只能绑定一个权威 Battle Server；多地域只能分配不同房间，不能跨地区共同推进同一 BattleWorld。

## 4. 开放设计决策

| 决策 ID | 状态 | 决策内容 | 决定时机 | 证据要求 |
|---|---|---|---|---|
| `S-NET-001` | `PENDING_PRODUCT_RULE` | PvP 持续缺失输入后采用 neutral、托管还是按模式结束 | S1 前 | C++ input trace + 用户产品规则 |
| `S-NET-002` | `PENDING_MEASUREMENT` | `InputDelayFrames`、deadline、grace 与最大缺失阈值 | S2 | 内存弱网与真实公网测量 |
| `S-NET-003` | `PENDING_MEASUREMENT` | 是否实施有限本地预测 | S4 | S3 恢复闭环、错预测率、体验与成本 A/B |
| `S-NET-004` | `PENDING_EVALUATION` | 实际 battle transport 选型 | S6 | 移动端、MTU、拥塞、许可证、维护和协议等价测试 |
| `S-NET-005` | `PENDING_DEPLOYMENT` | 首批公网节点的 region、OS、端口、安全组和部署方式 | S6 前 | 资源所有者授权与实际环境清单 |
| `S-NET-006` | `PENDING_PRODUCT_RULE` | 全国匹配的地区优先、跨区兜底与队伍策略 | S8 | 用户规则、网络质量和容量数据 |
| `S-PROTO-001` | `PARTIAL_SERVER_VALUE_READY / PENDING_FORMAL_CAPTURE_WIRE_DECISION` | formal `FrameInputSet` 的edge ownership：Server immutable human triple已实现，仍待决定Client capture/wire、malformed witness disposition与end-to-end formal integration | S1 formal input源码包前 | `S1-SERVER-HUMAN-FRAME-INPUT-VALUE-001` Server evidence；C++ release `InputHandler::poll` trace、上位设计和full Client/world contract；提案见`NTSD_Server/docs/ai/DECISIONS/PENDING-S1-FRAME-INPUT-EDGE-OWNERSHIP-001.md` |
| `S-PROTO-002` | `CONFIRMED / SERVER_SCHEDULE_FOCUSED_TEST_PASS` | 已确认 Model B：不加per-submission field，StartBarrier initial policy加Server session/authority-history future-effective activation schedule | 后续Client/wire、cross-version recovery或delay/rebarrier合同前 | `S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001`：test-first red、Debug/Release十项目`0/0`、full self-hosted tests、no-network host、declared-source audit与Ledger`23 / 74`；不等于Client/transport/recovery或S1验证 |
| `S-SRV-001` | `DESIGN_READY` | Server solution 的模块边界、强类型 ID、单向依赖与无 Common 杂物项目原则 | S5 | 模块依赖检查、编译和 architecture tests |
| `S-SRV-002` | `DESIGN_READY` | 统一配置、结构化日志、错误边界、health/readiness 和命令化本地运行 | S5 | 干净环境命令链、错误配置 fail-fast、异常隔离测试 |
| `S-DATA-001` | `PENDING_EVALUATION` | PostgreSQL 主库、local DB、migration、outbox、备份与恢复策略 | S8 | schema review、migration、幂等和故障恢复测试 |
| `S-DATA-002` | `PENDING_EVALUATION` | Redis/对象存储/消息队列是否有必要及其真相边界 | S8/S9 | 容量、延迟、恢复和运维证据 |
| `S-OPS-001` | `DESIGN_READY` | bootstrap/build/test/run-local/migrate/publish 命令与最小运行说明 | S5/S9 | CI 实际执行、干净环境复现、Unity Client 连接说明 |

## 5. 问题台账

当前没有未解决的 Server-only implementation failure；S0 formal close 目前由 `CLIENT_INTEGRATION_REQUIRED` 门禁暂停，不应被误写为 Server 测试失败或 S0 `VERIFIED`。

未来每个问题必须追加一条，格式如下：

```text
### S<阶段>-ISSUE-<三位序号>：简短标题

- 日期：
- 状态：OPEN / MITIGATED / FIXED / SUPERSEDED / BLOCKED
- 分类：RULE_TRACE / PROTOCOL / JITTER / RECOVERY / RUNTIME / CAPACITY / CONTROL_PLANE
- 影响：哪些 session、player slot、region 或 runtime
- 最小复现：journal / packet fixture / snapshot / 命令
- first difference：tick、domain、slot/generation、RNG 或 sequence
- 临时隔离：禁用路径、停止房间分配、回退到哪个已验证路径
- 根因：仅在证据闭合后填写
- 修复：关联 Change ID、代码 owner、协议/schema 版本
- 验证：实际命令、结果、未验证项
- 回滚：如何安全撤销或关闭新路径
```

## 6. 每次更新的最低要求

### 设计变更

- 先更新 `server-lockstep-s0-s9-design.md` 的相应合同；
- 如改变协议/schema/policy，新增 version 和 supersede 条目；
- 不删除旧的已执行事实，只追加 correction。

### 脚本或服务器代码变更

- 在修改前建立 `docs/ai/CHANGE-RECORDS/<ChangeId>.md` 与 `docs/ai/CHANGE-LEDGER.md` 条目；
- 同时在本台账关联阶段下新增进度条目和 Change ID 链接；
- 修改后运行 `Tools/Validate-ChangeLedger.ps1`，再运行该阶段最窄验证；
- 不以“编译通过”替代 C++ trace、server/client checksum、Player 或公网验收。

### 公网或部署操作

- 记录节点 owner 的明确授权、region、OS、配置、开放端口和安全组；不得在没有授权时扫描、登录、部署或改变网络配置；
- 不在文档记录密码、私钥、token 或完整敏感日志；
- 每个公网测试保存脱敏后的 endpoint ID、时间范围、版本和网络条件，而非凭聊天记忆判断。

## 7. 阶段进度条目

### 2026-08-24：S0 实施获批并建立首个 Work Package

- 状态：`IMPLEMENTING`。
- Work Package / Change ID：`S0-INPROC-AUTHORITY-001`。
- 已完成：目录边界定为 `I:\GitHub\Unity_GAS\NTSD_Server`；S0 Task Contract、Change Record、Ledger 与 Resume Card 已建立。
- 已写入：同进程 StartBarrier、无表现 Kernel Host、一个 server + 两个 clients authority session 和5项 focused tests。
- 未做：尚未编译/运行 S0 runtime；未创建独立 Server solution；未进入 S1～S9。

### 2026-08-24：执行顺序调整为服务器优先

- 用户要求：暂不继续修改客户端，先专注服务器脚本，服务器稳定后再回头处理客户端。
- 处置：`S0-INPROC-AUTHORITY-001` 保持 `CODE_WRITTEN` 并冻结，不继续 Unity 编译/focused/self-check；不能把它写成完成或回滚。
- 新顺序：先在 `I:\GitHub\Unity_GAS\NTSD_Server` 建立独立 solution、服务器模块与纯 .NET 测试闭环；服务器阶段可记录 `SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`，但原 S0～S9 的 `VERIFIED` 门槛不变。
- 当前阻塞：目标兄弟目录不在本任务的可写 workspace roots 中；加入工作区后创建新的服务器专用 Task Contract/Change Record。

### 2026-08-24：服务器执行计划设为持续目标

- 目标状态：`ACTIVE`。
- 目标线程：`01a02f58-c229-7830-a50b-7406c1d7d061`。
- 目标范围：只推进 `I:\GitHub\Unity_GAS\NTSD_Server` 的服务器 solution、协议、权威帧、room tick、journal/checksum、配置日志、错误隔离、health、构建测试运行脚本及后续服务器侧 S0～S9 工作。
- 留痕合同：每个服务器脚本包必须在修改前建立 Task Contract、Change Record 与 Ledger 条目；每获得编译、测试、运行或集成证据后同步更新本台账和 `docs/ai/STATE.md`。
- 暂停合同：需要修改或验证 Unity Client 时，先记录 `CLIENT_INTEGRATION_REQUIRED`，再暂停并等待用户批准；客户端未接入时最多标记 `SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`。

### 2026-08-24：S0 Server Bootstrap 预实施合同完成

- Work Package / Change ID：`S0-SERVER-BOOTSTRAP-001 / PLANNED`。
- SDK 决策：`.NET 10 LTS`，`global.json=10.0.100 + latestFeature`；不以本机现有但即将结束支持的 .NET 8/9 作为新服务端基线。
- 本机证据：`dotnet --list-sdks` 只有 3.1、8.0、9.0；.NET 10 尚未安装。
- 目录证据：`Test-Path I:\GitHub\Unity_GAS\NTSD_Server = False`；父目录存在，但目标不在当前可写 workspace roots。
- 已完成：预实施 Task Contract 和 Resume Card 已建立；服务器文件尚未写入，因此没有伪建跨仓库 Change Record，Unity Client 未继续修改或验证。
- 下一步：用户创建/加入目标工作区并安装 .NET 10 SDK后，直接创建独立 solution、项目单向依赖与 build/test/run 脚本。

### 2026-08-24：环境阻塞三轮复核后正式封存

- 目标状态：`BLOCKED`，不是完成、取消或范围缩减。
- 连续复核：三轮均得到 `Test-Path I:\GitHub\Unity_GAS\NTSD_Server = False`；当前 workspace roots 不包含目标路径；`dotnet --list-sdks` 均只有 3.1/8.0/9.0，无 .NET 10。
- 无替代写入：不得把服务器代码临时写进 Unity Client 仓库、`Tools/`、Temp 或其他路径后再搬运；这会违反用户指定的代码根与客户端冻结边界。
- 恢复条件：目标目录存在并加入可写 workspace，且 `dotnet --list-sdks` 出现 10.0 SDK。
- 恢复动作：读取本 Resume Card 和 `S0-SERVER-BOOTSTRAP-001` Task Contract，在 Server 仓库首个脚本改动前创建独立 Change Record/Ledger，然后建立 solution。

### 2026-08-24：用户级 writable root 配置已写入，但当前任务沙箱未重载

- 用户已将 `I:\GitHub\Unity_GAS\NTSD_Server` 加入 `C:\Users\Logan\.codex\config.toml` 的 `sandbox_workspace_write.writable_roots`；读取结果与目标路径一致。
- 目标目录当前存在且为空，但在本任务中尝试创建 `docs/ai`、`src`、`tests`、`scripts`、`config`、`deploy` 以及 `.git` 时均返回 `Access to the path ... is denied`。
- 结论：当前任务仍使用创建时注入的旧 writable roots；配置没有动态重载到此沙箱。没有任何 Server 子目录、文件或 Git 仓库被创建。
- 恢复条件更新：重启 Codex Desktop 或重新打开本任务/本地环境，使新的 user-level config 生成新的 sandbox；恢复后先做一次单文件/目录 write probe，再开始 Server bootstrap。

### 2026-08-24：服务器设计拆分完成

- 状态：`DESIGN_READY`，不是 `IMPLEMENTING`。
- 内容：从上位统一方案中分离 S0～S9 的详细设计、单慢客户端不阻塞合同、协议/传输分层、问题修复流程、阶段验收与进度台账。
- 未做：未创建 ServerHost、未创建网络协议代码、未选择 transport、未连接公网 IP、未运行 Unity/Server 测试。
- 下一步：仅在用户明确批准进入 S0 后，建立 S0 的独立 Work Package、Task Contract 和 Change Record，再开始代码实施。

### 2026-08-24：S0 Server Bootstrap 实际完成（覆盖上方旧环境阻塞记录）

- Server 仓库：`I:\GitHub\Unity_GAS\NTSD_Server`，`main` / initial commit `cd37fd8`，原有 `.gitattributes` 保留。
- SDK：`global.json=10.0.100 + latestFeature` 在本机解析为 `.NET SDK 10.0.400`；Server scripts 使用进程级临时 CLI/NuGet 路径，未修改系统 PATH、profile 或 SDK。
- 实现：独立 solution、六个 source projects、四个 test executables、`bootstrap/build/test/run-local`、Server Ledger/State/Handoff/Record、架构依赖验证和 no-network local host 已建立。
- 验证：bootstrap 两次通过；Debug/Release build 均 `0 warnings / 0 errors`；四项测试及 Server Ledger validator 通过；local run 输出 `BootstrapReady` / `NetworkListenerStarted=False`；生成的 `bin/obj` 已被 `.gitignore` 忽略。
- 修正历史：初次 NUnit restore 的 NuGet `NU1301` SSL/authentication 失败未被绕过；S0 测试改为无第三方包的 .NET 10 self-hosted executables。首次 build `CA1822` 和 Ledger PowerShell 插值错误均以最小代码修正后重跑通过。
- 结论：`S0-SERVER-BOOTSTRAP-001 = FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`。这不是 authority-frame、Unity multi-world、跨端 checksum 或 S0 `VERIFIED`；Unity Client 未修改、编译、测试或验证。

### 2026-08-24：Server-first S0 authority-session 子包完成

- Work Package / Change ID：`S0-SERVER-INMEMORY-AUTHORITY-001 / FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`。
- 实现：generic immutable frame、StartBarrier、authority-first sequence、replica checksum witness、fault fail-closed；合成 deterministic `TestKernel` 只在 tests。
- 验证：Debug/Release 10 projects 均 `0 warnings / 0 errors`；四个自托管测试、96 帧一致 journal、first mismatch/post-fault、no-network local run、Ledger 与 static audit 通过。
- 仍禁止：正式 S1 protocol/DTO、deadline/ACK/Jitter、socket/transport、snapshot/recovery、battle rules、Unity Client 修改或验收。
- 结论：formal S0 现需要 `CLIENT_INTEGRATION_REQUIRED`。TestKernel 不能取代同一正式 Kernel 的 1 Server + 2 Unity Client worlds / 同 seed+journal / 十域 checksum 证据，故 S0 不是 `VERIFIED`。

### 2026-08-24：CLIENT_INTEGRATION_REQUIRED — formal S0 close gate

- 状态：`WAITING_FOR_EXPLICIT_USER_APPROVAL`；本条不授权任何 Unity 修改、编译、测试或运行。
- 最小候选范围：`Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs`、`InProcessLockstepAuthoritySession.cs`、`LockstepStartBarrier.cs`、`LockstepSessionIdentity.cs`、`Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs` 与现有 `BattleRuntimeSelfCheck` 入口。
- 原因：现有 Server TestKernel 只证明容器/调度，不是 formal NTSD BattleKernel；S0 关闭证据必须是相同 formal Kernel、相同 StartBarrier/seed/roster/journal、1 Server + 至少 2 Unity client worlds 的重复 N 帧十域 checksum。
- 批准后顺序：先只读/compile/focused 验证既有 Unity S0；如需改 Unity source，先创建独立 Client Change Record；保留 first differing tick/domain/slot/generation/RNG，不能用 Client state 覆盖或提前进入 S1。

### 2026-08-24：S0 Client validation-only 首轮结果

- 用户授权：允许既有 Unity S0 的只读、编译、focused test 与 `BattleRuntimeSelfCheck`，明确禁止 Client 源码、Scene、资源和配置修改。
- 安全边界：项目已由 Unity 2022.3.62f3 占用，`Temp/UnityLockfile` 存在；没有启动第二个 Unity Editor。
- 编译证据：`Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 于17:06:41/17:06:42 刷新，晚于 S0 source；Editor.log 的窄 error scan 未匹配 `error CS*`、`Scripts have compiler errors` 或 `Compilation failed`。
- self-check：通过 `Temp/NTSD_BattleRuntimeSelfCheck.request` 触发现有 Editor；它自行消费请求，并在17:07:33写入 `PASS`。Editor.log 记录 `BattleRuntimeSelfCheckEditor` “自检完成”。
- focused NUnit：用户在现有 Editor 的 EditMode Test Runner 搜索 `InProcessLockstepAuthoritySessionEditorTests` 并运行五项；截图为 5/5 pass、0 fail。self-check 不调用该 Fixture，且项目下没有持久 TestResults，所以截图是会话证据。
- Ledger：`Tools/Validate-ChangeLedger.ps1` 因三个非 S0 的 `BattleBackgroundPlatform*` 未记录 authored diff 与 `CAMERA-PLATFORM-BACKGROUND-001` 声明不一致而失败；没有改动或收编这些外部文件。S0 validator PASS 仍待其 owner 处理。
- 当前状态：`FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`。不能从此推导同一正式 Kernel 的 Server+Client 多 world 十域 checksum 或 S0 `VERIFIED`。

### 2026-08-24：Existing lockstep regression user-run evidence

- 用户在现有 Unity EditMode Test Runner 搜索 `BattleLockstepSessionEditorTests`。筛选列表中九项测试均为绿色；与该 fixture 的九个 `[Test]` 方法一一对应。
- 面板同时显示 90 passed / 0 failed；因没有持久 TestResults XML，且该总数可能覆盖更宽范围，本台账只将其记录为用户提供的该 fixture **9/9 pass** 会话证据。
- 这关闭了 S0 Task Contract 的 existing lockstep regression 项；不闭合真实实体 multi-world、十域 first-difference witness、C++ authority 或 S0 `VERIFIED`。

### 2026-08-24 18:14 +08:00：Server 当前工作树回归验证

- 复跑 `dotnet --version`、`scripts/build.ps1 -Configuration Debug`、`scripts/test.ps1 -Configuration Release` 和 `scripts/run-local.ps1 -Configuration Release`；SDK 为 `10.0.400`，Debug 为 `0 warnings / 0 errors`，四个自托管测试程序与 Server Ledger（`Records: 2 / Governed files: 37`）均通过。
- 本地 Host 再次报告 `ExecutionModel=SequentialSingleWriter` 与 `NetworkListenerStarted=False`；没有启动 Socket、transport、数据库或公网操作，也没有修改或运行 Unity Client。
- 这只刷新 Server-only 证据，不替代 existing lockstep regression、同一正式 Kernel 的 Server+两 Client world 十域 checksum、C++ release authority 对照，亦不允许进入 S1。

### 2026-08-24：S0 stage-boundary 与 witness coverage 只读审计

- `server-lockstep-s0-s9-design.md` §5 的 S0 关闭条件是同进程 server world + 两个 client world、同 seed/journal 的连续 N 帧十域 checksum；独立进程 / 跨 runtime 一致性归 S5，不能继续混写为 S0 gate。
- 已观察：focused fixture实际覆盖 48 tick、两次重复、1 server + 2 client logic-only worlds，但 `InProcessBattleKernelHost` 只记录 aggregate `CaptureRuntimeChecksum64`；`InProcessAuthorityDifference` 仅有 tick、replica index 与 input/state aggregate hashes。
- 因此 current code 不能留下 design 所要求的 first differing domain、slot/generation、RNG witness。进一步只读确认：`BattleLockstepChecksumSnapshot` 有 input/metadata/rng/world/slots/aRest/vRest/stats/events 九个命名 hash 与 overall；把它作为十个 checksum 值是待正式确认的合理推断。该 snapshot 会构造 arrays/dictionaries/JSON/SHA strings，故只能在 aggregate mismatch 后用于诊断，不能替代每 tick 的 0-allocation `CaptureRuntimeChecksum64`。未来仍需以固定域比较和 typed slot/generation witness 接线；需要独立 Client Change Record，本审计不授权任何 Client 改动。
- Server-side no-go audit：`NTSD.Battle.Kernel.Abstractions` 仅有 `IsFormalBattleKernelImplemented = false` marker，`InMemoryAuthoritySession` 仍是 generic container/TestKernel 边界。把 formal `IBattleKernel`、snapshot/restore 或 shared runtime adapter 写进 Server 将提前进入 S5；它不是当前 S0 Client witness 缺口的合法替代方案。

### 2026-08-24：持续 Server-first 目标恢复与 S1 authority-frame 预实现

- 用户明确纠正：Client 暂停只约束 Client 子包，不能暂停 Server S0～S9 持续目标；因此在不执行 Unity 动作的前提下，建立 `S1-SERVER-AUTHORITY-FRAME-001`。
- 实现：`NTSD.Battle.Protocol` 现有 versioned `AuthorityFrameProtocolContract`、`InputSubmission<TInput>`、`AuthoritativeFrameEnvelope<TInput>`、`FrameAck`、`ServerProgress`、stable disposition/conflict witness；`NTSD.Server.BattleHost` 现有 future-target in-memory assembler、canonical first-valid-wins、idempotence、slot/sequence conflict witness、explicit deadline trigger 和 caller-owned missing-input policy interface。
- 约束：不定义 `TInput` 的 held/pressed/released 语义，不选择 neutral/carry/AI/disconnect；无 policy 时 deadline fail closed，test-only explicit policy 才能给出 input/source/reason。没有写 Client、battle rule、wire serialization、Socket/transport、ACK/Jitter、DB 或公网。
- 验证：Debug/Release 均为 10 项目 `0 warnings / 0 errors`；Protocol/BattleHost/Architecture/Integration 四个自托管 executables、no-network local host、Ledger `4 / 47` 和 scoped audit 均通过。
- 状态：`FOCUSED_TEST_PASS / SERVER_AUTHORITY_FRAME_PREIMPLEMENTATION_READY`；S0 formal Client proof、formal Kernel/adapter、S1 phase `VERIFIED`、actual ACK/Jitter/weak network 仍未完成。

### 2026-08-24：S1 locked-envelope → sequential-room adapter

- 实现：`InMemoryAuthorityFrameRoomAdapter<TInput, TChecksum>` 先验证 protocol contract 与 StartBarrier 的 session/policy/roster/initial tick，再 lock envelope、canonical map 到 room；lock failure 不推进 room，room mismatch 后保留 locked envelope+journal、锁存 execution fault，并拒绝后续 submit/advance。
- 不做：不创建 battle rule/Kernel/Client、实际 deadline clock、wire/Socket/transport、ACK/Jitter、weak network、snapshot/recovery、DB 或公网；missing policy 保持 caller-owned。
- 验证：final fixture 覆盖 full lock、not-ready no advance、explicit deadline policy、session/policy/roster/initial-tick mismatch、checksum mismatch/post-fault；Debug/Release 10 项目 `0/0`、四个 self-hosted executables、no-network local run、Ledger `5 / 49` 与 scoped audit均通过。
- 状态：`FOCUSED_TEST_PASS / SERVER_FRAME_ROOM_ADAPTER_READY`；仍不是 S0/S1 verified 或跨端/battle authority证据。

### 2026-08-24：S1 prearranged logical deadline lifecycle

- 实现：`AuthorityFrameDeadlineSchedule` 冻结连续 `(target tick, deadline logical server tick)` entries；`InMemoryAuthorityFrameDeadlineLifecycle` 用显式单调 logical tick 驱动 adapter。full roster 可提前 lock，缺 slot 到 deadline 后才交给 caller policy。
- fail-closed：pre-deadline missing、deadline no/refusing policy、clock regression、schedule exhaustion、adapter fault 均不发明 input、不跳 frame、不推进 room；没有引入 DateTime/Stopwatch/Thread.Sleep/Unity time。
- 验证：schedule contract copy/continuous/deadline ordering、policy/no-policy、clock regression、contract mismatch、schedule exhaustion、adapter fault fixtures；Debug/Release 10 项目 `0/0`、四个 self-hosted executables、no-network local run、Ledger `6 / 53` 与 scoped audit均通过。
- 状态：`FOCUSED_TEST_PASS / SERVER_DEADLINE_LIFECYCLE_READY`；InputDelay 实测公式、真实30Hz driver、ACK/Jitter/weak network、formal Kernel/Client与S0/S1 verified均未完成。

### 2026-08-24：S2 in-memory ACK confirmation and global ready range

- 实现：`InMemoryFrameAckTracker` 以 `ServerProgress` 锁定 published authority bound，维护canonical slots的highest contiguous cursor、first rejected ACK witness和global minimum confirmed/ready range；untrusted bootstrap ACK version由Server owner稳定拒绝。
- fail-closed：wrong protocol/session/slot、regression、future cursor均不改状态；ready range从initial tick连续到所有slot最小cursor，可合法为空。
- 不做：packet、retransmit、Jitter/client ready buffer、weak network、Client、battle/Kernel、DB/公网。
- 验证：Debug/Release 10 项目 `0/0`、ACK cursor/range fixtures与四个self-hosted executables、no-network local run、Ledger `7 / 56` 和scoped audit均通过；状态 `FOCUSED_TEST_PASS / SERVER_ACK_CONFIRMATION_READY`，不是 S2 verified。

### 2026-08-24：S2 generic contiguous ready buffer and bounded dequeue

- 实现：`InMemoryAuthorityFrameReadyBuffer<TInput>` 接收 contract-checked immutable envelopes；future/out-of-order帧可缓存但不跨洞，same tick identical幂等、different内容留first conflict，late/capacity/wrong facts稳定拒绝；`DequeueReady(maxFrames)`每次严格bounded。
- 不做：实际Client buffer、packet/transport/Jitter/retransmit/weak-network、battle rules/Kernel、DB/公网。
- 验证：out-of-order hole、duplicate/conflict/late/capacity、contract/roster和bounded dequeue fixtures；Debug/Release10项目`0/0`、四个self-hosted executables、no-network local run、Ledger`8 / 58`与scoped audit均通过；状态`FOCUSED_TEST_PASS / SERVER_READY_BUFFER_READY`，不是S2 verified。

### 2026-08-25：S1 initial authority-tick container correction

- Work Package / Change ID：`S1-SERVER-INITIAL-AUTHORITY-TICK-001 / FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED / S1-CORRECTION`。
- 观察到的 defect：`AuthorityFrameProtocolContract.InitialAuthorityTick` 合法地允许非零值，且 `AuthorityFrameDeadlineSchedule` 已按其连续索引；但既有 generic StartBarrier/session/journal 固定从零开始，因此一个 session/policy/roster 对齐、但起点非零的 protocol contract 会在 room adapter 之前/构造中无法与 room 一致。
- 实现：StartBarrier 现持有 validated immutable `InitialAuthorityTick`（默认零保持已有调用兼容）；session 与 journal 从该 origin 起步；adapter 在构造 mutable owners 前显式要求 protocol 与 StartBarrier origin 相等。
- 回归：negative tick、direct non-zero session、non-zero room+journal、aligned non-zero adapter execution，以及 mismatch 在 kernel 执行前 fail closed 均由 self-hosted BattleHost fixtures 覆盖。
- 证据：Debug/Release各10项目`0 warnings / 0 errors`；focused BattleHost与完整 Protocol/BattleHost/Architecture/Integration chain通过；`run-local`仍报告`Status=BootstrapReady`、`ExecutionModel=SequentialSingleWriter`、`NetworkListenerStarted=False`；Ledger`Records: 14 / Governed files: 70`与scoped source audit通过。
- 不意味着：formal BattleKernel、snapshot/recovery、Client integration、C++ battle alignment、真实transport、missing-input policy或S0/S1 `VERIFIED`。Unity Client没有被修改、编译、测试或验证。

### 2026-08-25：C++ release ↔ Server tick-identity 前置审计（只读）

- 直接读取 C++ release live path：`reset_battle_runtime()` 将 `world.game_tick`、`input_phase`、`g_frame_toggle` 归零；`BattleTickScheduler::step_one_tick()` 直接委派 `game_tick()`；后者在全部 live passes 前先递增/切换这些控制值。
- 结论：最新 `S1-SERVER-INITIAL-AUTHORITY-TICK-001` 只修复 generic Server authority-history owners 的一致起点，不能把 `InitialAuthorityTick == 0` 解释成 C++ reset state、pre-step world tick 或 first completed world tick。
- future formal Kernel/snapshot schema 必须显式、版本化并以 real-world restore/replay 验证 `authorityFrameTick`、`worldCompletedTick`、`nextAuthorityFrameTick` 三者的关系；不得从 list index 或 generic TestKernel 推断。
- 本条是 `ANALYSIS_COMPLETE / NO_CODE / CLIENT-PAUSED`，不创建 snapshot/recovery、Client action、transport或新的产品规则，也不改变任一 S0～S9 `VERIFIED` 状态。

### 2026-08-25：S1 missing-input provenance pair correction

- Work Package / Change ID：`S1-SERVER-MISSING-INPUT-PROVENANCE-001 / FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED / S1-CORRECTION`。
- 观察到的 defect：现有 public `AuthorityFrameInputSource` 与 `MissingInputFillReason` 只做 broad missing/non-missing validation，可构造 cross-labelled pair（例如 persistent source + transient reason）或 unknown enum；这会污染 immutable authority-history 的审计标签。
- 实现：`NTSD.Battle.Protocol` 现在唯一拥有 six-pair taxonomy validator；immutable slot input与generic `MissingInputResolution<TInput>` 都使用它，real/idempotent仍只接受`None`。不为任何 source 选择 payload、grace、neutral、AI、disconnect/reconnect或模式行为。
- 回归与证据：six legal pairs、mismatched/unknown pair、既有transient deadline test均通过；Debug/Release各10项目`0 warnings / 0 errors`；focused Protocol/BattleHost与完整自托管chain通过；`run-local`仍为`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`；Ledger`15 / 70`和fixed-string scoped audit通过。
- C++ source terms只能帮助确认当前发现的 local input path没有给出Server网络缺失策略；它不替代 C++ trace 或用户产品决定。`S-NET-001`/`S-NET-002`继续 pending，本包不是 S1 `VERIFIED`，也没有任何 Unity Client动作。

### 2026-08-25：S1 policy-version input-binding gate（只读）

- 设计要求 StartBarrier 固化 policy version，future policy update 必须广播 version 和 future effective tick；但 S1 `InputSubmission` 字段清单没有明确 per-submission policy version。
- 当前 Server 的 contract/envelope/progress/gap request 包含 policy version，而 `InputSubmission` / redundancy window不包含。因此它是待决的 binding model，不是已证实可直接改代码的 defect。
- 可行模型包括每个 submission/window 显式携带 version，或已验证 session/connection binding 在 authority history 中保留可恢复 witness；两者都必须决定 activation tick 时旧输入、冗余窗口、target tick与replay/reconnect的 fail-closed 行为。
- 详见 `NTSD_Server/docs/ai/AUDITS/S1-POLICY-VERSION-INPUT-BOUNDARY-001.md`。在用户/协议 owner 决定前，不创建 DTO/stale-policy rejection/connection-state源码包，不触碰Client/transport，也不改变S1 `VERIFIED`状态。

### 2026-08-25：formal checksum first-difference witness gate（只读）

- 当前 Server `IInMemoryAuthorityKernel<TInput,TChecksum>` 只返回一个 aggregate，mismatch也只有tick/replica/两个 aggregate；它只能证明generic调度 fail-closed，不能证明S0/S3所需的ten-domain first difference。
- future formal witness必须在同一completed tick boundary保留版本化domain list、first differing domain、stable slot/generation、RNG witness、event cursor、authority input history与world/replica identity；C++ core/character/weapon/special/effect及world battle/stage/roster/camera views只作为field inventory线索，不是十域schema。
- 详见`NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md`。不得通过TestKernel、多aggregate包装、反射复制或Unity表现状态伪造该证据；它需要formal Kernel与冻结中的Client integration，不能标记S0/S3 `VERIFIED`。

### 2026-08-25：S1 Server-owned future-target admission bound correction

- Work Package / Change ID：`S1-SERVER-FUTURE-TARGET-BOUND-001 / FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED / S1-CORRECTION`。
- 观察到的 defect：assembler 在 protocol/session/slot/initial/late checks 后，对每个 future `TargetTick` 无上限地写入 managed pending/sequence state；既有 redundancy-window count、ready buffer 或 delivery budget 都不能约束该 owner。
- 实现：assembler 与 room adapter 现在都要求 caller-supplied、nonnegative `MaxFutureAuthorityTicks`；zero合法且不引入 production delay/default。`TargetTick - NextAuthorityTick` 以inclusive distance在 sequence/pending mutation 前判定，越界返回 appended `RejectedTargetBeyondFutureLimit`。该减法不依赖可能 overflow 的`next + limit`。
- 证据：negative/zero/exact/over-limit/no-mutation/sequence reuse/moving-bound/near-terminal/adapter 与既有 deadline/gap/redundancy/disorder fixtures通过；Debug/Release各10项目`0 warnings / 0 errors`，focused/full self-hosted chain、`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`、Ledger`17 / 70`与fixed-string audit通过。
- 边界：它不选择 `InputDelayFrames`、deadline、missing-input 产品规则、raw packet/MTU/bandwidth、Client、transport、battle rules、formal Kernel、snapshot/recovery或S1/S2 `VERIFIED`。

### 2026-08-25：S1 authority-tick numeric range correction

- Work Package / Change ID：`S1-SERVER-AUTHORITY-TICK-RANGE-001 / FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED / S1-CORRECTION`。
- 观察到的 defect：`long.MaxValue`此前可作为initial/frame/input/envelope/deadline target，且assembler/session/journal/ready-buffer会用`++`推进next cursor；最终frame会令cursor静默回绕为负数，进而破坏progress/ACK/deadline/gap的一致性。
- 实现：Protocol-owned addressable range现在为`[0, long.MaxValue - 1]`；`long.MaxValue`只保留为final legal tick之后的terminal next cursor。contract/barrier/input/envelope/frame/deadline拒绝terminal fact；assembler在terminal cursor于任何missing-policy/history mutation前返回`AuthorityTickExhausted`。
- 证据：terminal fact rejection、final legal session/direct-room/journal/assembler/ready-buffer/progress/ACK cursor和terminal assembler/direct-room no-second-kernel/no-policy-fill/no-journal-append regressions通过；Debug/Release各10项目`0 warnings / 0 errors`，focused/full self-hosted chain、`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`、Ledger`18 / 70`与expanded fixed-string audit通过。
- 边界：它不表示C++ `world.game_tick`同样使用该range，不改变30 Hz、battle rules、Client、transport、missing-input、snapshot/recovery或S1/S2 `VERIFIED`。

### 2026-08-25：S2 ready-buffer future-horizon correction

- Work Package / Change ID：`S2-SERVER-READY-BUFFER-HORIZON-001 / FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED / S2-CORRECTION`。
- 观察到的 defect：`MaxBufferedFrames`仅限制buffer条数，不限制envelope相对`NextReadyTick`的距离；少量极远tick可占满容量，导致实际能填补连续hole的near/current frame被`RejectedCapacity`。
- 实现：ready buffer现在要求caller-supplied、nonnegative `MaxFutureAuthorityTicks`；zero合法且无production jitter/delay default。non-late envelope以`Envelope.Tick - NextReadyTick`的inclusive distance在duplicate/conflict/capacity mutation前判定，越界返回 appended `RejectedFutureTickLimit`。
- 证据：invalid/zero/exact/over-limit/no-mutation/near-capacity/moving-window和既有disorder regressions通过；Debug/Release各10项目`0 warnings / 0 errors`，focused/full self-hosted chain、`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`、Ledger`19 / 70`与fixed-string audit通过。
- 边界：它不是actual Client jitter buffer、packet/transport、ACK/retransmit、weak-network runtime、snapshot/history/recovery或S2 `VERIFIED`。

### 2026-08-25：S1 client-sequence retention prerequisite audit

- 观察到的事实：`submissionsBySequence` 在 accepted input 对应 frame lock 后仍保留；new future-target bound只限制 pending future tick，不能回收已锁序列事实。
- 不能擅自修复的原因：client-reported confirmed progress 不是Server-owned proof，redundancy window没有sequence lifecycle，ACK tracker也没有 input-sequence-to-ACK retirement mapping；按锁帧、reported cursor、LRU/count cap删除或拒绝，都会选择 delayed duplicate、conflicting reuse、reconnect、snapshot/replay或long-session overload的行为。
- 结论：`NTSD_Server/docs/ai/AUDITS/S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001.md` 已记录此为版本化协议/产品前置条件。下一源码包前必须定义 sequence lifecycle、idempotency horizon、retirement proof、post-expiry disposition/witness、recovery state和overload处置；本审计不创建Client/transport/snapshot/reconnect代码。

### 2026-08-25：S3 authority-history retention prerequisite audit

- 观察到的事实：generic assembler `lockedFrames`与room journal从initial tick起无限append；gap responder的`maxFramesPerRequest`只限制一次响应，当前索引仍假定从`InitialAuthorityTick`到最新tick的完整前缀都在内存中。
- 不能擅自修复的原因：prefix eviction会使现有gap index失效，并必须选择最早可恢复tick、snapshot/checksum base、ACK与history关系、history-expired disposition、落后客户端的gap/snapshot/replay/disconnect路径及正式Kernel恢复证据。
- 结论：`NTSD_Server/docs/ai/AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md` 已记录此为S3 formal history/recovery前置条件。当前list只可作为generic Server-focused audit history，不得标为`FrameHistoryRing`、snapshot或S3已实现。

### 2026-08-25：S1 protocol-version evolution prerequisite audit

- 观察到的事实：当前`AuthorityFrameProtocolVersion=1`会被in-memory owners精确校验，最近也追加了多个稳定disposition enum；但工程不存在实际codec、message header/capability negotiation、unknown-disposition receiver behavior、rolling upgrade/downgrade或replay schema supersede模型。
- 不能擅自修复的原因：无论bump marker、保持marker、添加unknown fallback、per-message version还是session capability，都会选择future wire ABI、Client retry/disconnect/upgrade与S5/S6部署行为。
- 结论：`NTSD_Server/docs/ai/AUDITS/S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001.md` 已记录这不是当前generic source defect。不得将append-only source enum误称wire compatibility；待S5/S6获授权后以real serialized compatibility fixtures验证。

### 2026-08-25：S2 Server-owned redundancy ingress capacity correction

- Work Package / Change ID：`S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001 / FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED / S2-CORRECTION`。
- 观察到的 defect：window creator声明的`MaxEntries`不能约束Server ingress；matching window会被逐项委托assembler，接收端没有独立entry-count cap。
- 实现：ingress现在要求positive、caller-supplied `MaxEntriesPerWindow`；actual frozen count超限时返回`RejectedServerEntryLimit`、zero outcomes，并在任何per-entry delegation/assembler mutation前失败。at-cap window保留原有顺序和outcomes。
- 证据：invalid/at-cap/oversize zero-mutation与既有inbound disorder回归通过；Debug/Release各10项目`0 warnings / 0 errors`，focused/full self-hosted chain、`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`、Ledger`16 / 70`与fixed-string audit通过。
- 不意味着：production redundancy count、raw packet allocation/MTU/bandwidth protection、Client resend、transport、deadline/missing-input policy、snapshot/recovery或S2 `VERIFIED`。Client没有任何动作。

### 2026-08-25：S1 formal `FrameInputSet` edge ownership 决策提案（只读）

- 已观察：上位设计要求`InputSubmission`包含完整`held/pressed/released`；C++ release `InputHandler::poll`则先roll `key_* -> prev_*`、写入七个held action，再按固定顺序派生rising edge/history/cooldown，AI亦由world/input phase/RNG在Kernel内生成。
- 提案：human slot以locked `held`为唯一battle truth；若Client依设计提交edge masks，则Server/formal Kernel以preceding locked held重算`pressed = held & ~previousHeld`、`released = previousHeld & ~held`，Client edge只作一致性witness，不能覆盖Kernel。AI不能由Client提交。
- 状态：`ANALYSIS_COMPLETE / PARTIAL_NON_PRODUCT_VALUE_IMPLEMENTED / SERVER-FIRST / CLIENT-PAUSED / FORMAL_CAPTURE_AND_WIRE_DECISION_REMAINING`。`S1-SERVER-HUMAN-FRAME-INPUT-VALUE-001`已完成immutable seven-action triple、edge derivation/order、equality/hash和Server Protocol tests；`S-PROTO-001`仍只阻塞Client capture/wire/disposition与full formal world integration，不暂停Server-first总目标，也未触碰Client、transport、missing-input产品规则或S0～S2验证状态。
- 详情：`NTSD_Server/docs/ai/DECISIONS/PENDING-S1-FRAME-INPUT-EDGE-OWNERSHIP-001.md`。

### 2026-08-25：S1/S2 generic terminal authority-tick boundary audit 与 Server 回归重验（只读）

- 审计：`NTSD_Server/docs/ai/AUDITS/S1-S2-TERMINAL-AUTHORITY-TICK-BOUNDARY-AUDIT-001.md` 已审阅range、protocol facts、assembler、room/session/journal、adapter、ready buffer、ACK与gap。`long.MaxValue`仍只可作为terminal next cursor或精确empty ready-range的first cursor；没有发现可在当前未决协议/恢复范围内独立修复的Server source defect。
- 新鲜Server证据：`.NET SDK 10.0.400`；Debug 10项目`0 warnings / 0 errors`；Release Protocol/BattleHost/Architecture/Integration self-hosted chain通过；本地运行仍为`BootstrapReady / SequentialSingleWriter / NetworkListenerStarted=False`；Ledger`Records: 21 / Governed files: 70`通过。
- 边界：该结果只证明generic integer-safety/container boundary，不能定义C++ `world.game_tick`、history retention/recovery、terminal session lifecycle、formal Kernel、Client、transport或S0～S2 `VERIFIED`。

### 2026-08-25：S1 immutable human frame-input value（Server-only）

- Work Package / Change ID：`S1-SERVER-HUMAN-FRAME-INPUT-VALUE-001 / FOCUSED_TEST_PASS / SERVER_HUMAN_FRAME_INPUT_VALUE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION`。
- 实现：`NTSD.Battle.Protocol/HumanFrameInput.cs`新增七logical human action mask、immutable held/pressed/released triple、right→left→up→down→attack→defend→jump的release rising-edge order、structural mask validation、pure prior/current held edge derivation、prior-held consistency predicate与value equality/hash。它可作为`InputSubmission<HumanFrameInput>`的值，但没有修改generic `InputSubmission<TInput>`或任何mutable owner。
- Test-first 与验证：新Protocol fixture/runner先写，missing type的`CS0103`/`CS0246` expected red evidence已记录；实现后focused Protocol executable通过。Debug/Release各10项目`0 warnings / 0 errors`、完整self-hosted Server chain、no-network local run、declared-source content audit和final Ledger`Records: 22 / Governed files: 72`均通过。
- 加固：同一fixture现穷举`128 × 128 = 16,384` legal prior/current held pairs，且验证unknown prior mask在derivation/witness validation前fail fast；Debug/Release、完整Server chain、no-network local run、content audit与final Ledger`22 / 72`均重新通过。
- 不意味着：formal capture/tick mapping、StartBarrier baseline、AI slot generation、Client edge wire/disposition、missing-input policy、policy binding、serializer/wire ABI、snapshot/recovery、Client runtime或S1 `VERIFIED`。Unity Client没有任何动作。

### 2026-08-25：S1 C++ input bootstrap/capture boundary audit（只读）

- C++ release evidence：`init_battle_from_config`清零七个`key_*`与`prev_*`并设置`need_clear_input`；首个`step_one_tick -> game_tick`先递增`world.game_tick`/切换`input_phase`再进入callback。callback可先`poll`，但会检测flag、再次清零key/prev/cooldown/history并return；normal `apply_input`又只在`world.game_tick > 1`时运行。
- 结论：all-released是可引用的C++ battle-entry pre-history事实，支持`HumanFrameInput`的纯值基线；但不能把generic `InitialAuthorityTick`或Client first packet直接等同C++ reset/tick1/tick2，formal capture/StartBarrier/tick mapping仍待定义。
- 详情：`NTSD_Server/docs/ai/AUDITS/S1-FORMAL-INPUT-BOOTSTRAP-CAPTURE-BOUNDARY-001.md`。本审计未改Client、Server源码、30 Hz、policy、Kernel、transport或阶段验证状态。

### 2026-08-25：S1 policy-version session/authority-history binding（用户确认的 Server-only 实现）

- 已观察：设计要求StartBarrier固化session-wide policy并以future effective tick更新，但S1 `InputSubmission`字段表没有`PolicyVersion`；现有envelope/progress/gap持有policy，而submission/window没有。
- 用户确认：`S-PROTO-002`采用Model B——StartBarrier initial policy、Server-owned future-effective activation schedule，按target authority tick resolve policy并把resolved version写入immutable authority history；不新增per-submission field。活跃session update不得改变已经形成的`TargetTick`或`InputDelayFrames`语义，除非另建rebarrier/versioned contract。
- 已实现（Server-only）：`S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001 / FOCUSED_TEST_PASS`新增immutable activation fact与append-only schedule；assembler在lock time把resolved policy写入existing envelope history，room adapter仅委托该Server操作。已覆盖exact activation boundary、pre-accepted future target保持、locked-history不回写、ordering/terminal/contract mismatch拒绝及无schedule不变。Debug/Release十项目`0/0`、full self-hosted tests、no-network local host、declared-source audit、Ledger`23 / 74`均通过。
- 仍未授权：Client/wire broadcast、stale/malformed disposition、connection state、cross-boundary redundancy、gap/ready/ACK/recovery、replay/reconnect、InputDelay变更或rebarrier；不能把本包写成S1 `VERIFIED`。
- 详情：`NTSD_Server/docs/ai/DECISIONS/PENDING-S1-POLICY-VERSION-BINDING-001.md`与`CHANGE-RECORDS/S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001.md`。

### 2026-08-25：cross-policy authority-history consumer 前置审计（只读）

- 已观察：Model B Server schedule 可在immutable envelope history中产生initial policy前缀和later policy后缀；assembler的`ServerProgress`按next authority tick解析policy。
- 已观察：现有gap responder只接受initial contract policy的request，却可能返回later-policy envelope；ready buffer拒绝later-policy envelope；ACK tracker拒绝later-policy progress。当前没有定义gap range跨activation、ready接收、ACK cursor、Client observable effective-tick、reconnect/replay或stable disposition。
- 状态：`ANALYSIS_COMPLETE / NO_CODE / VERSIONED-CROSS-POLICY-CONTRACT-REQUIRED`。不能通过放宽静态guard、加policy field、隐藏connection switch、Client/transport/recovery代码或rebarrier绕过。详情：`NTSD_Server/docs/ai/AUDITS/S2-S3-CROSS-POLICY-HISTORY-CONSUMER-PREREQUISITE-001.md`。
- 待确认提案：`NTSD_Server/docs/ai/DECISIONS/PENDING-S2-S3-CROSS-POLICY-HISTORY-DELIVERY-001.md` 推荐 C1（独立acknowledged activation journal + per-envelope witness），但其progress含义、mixed range、activation ack、recovery evidence、delay/rebarrier边界和首包范围均仍待用户协议确认。

### 2026-08-25：Server-first 后续源码门禁

- `S-PROTO-002`已确认并由独立Server-only包关闭；它不再是当前阻塞。cross-policy history consumer合同现是该决策的后续边界；formal input capture/wire/tick mapping、missing-input、sequence/history/recovery与S5 actor/fault仍各自需要未批准的协议/产品/Client/formal Kernel合同；S6+ transport/public/control-plane亦未授权。
- 状态：无活跃源码包。这不是S0～S9完成、不是Server构建/测试失败，也不是允许用placeholder DTO、queue、lock、recovery或Client操作绕过的暂停。
- 清除方式：用户确认或修订`S-PROTO-001`、提供`S-NET-001/002`产品规则，或授权明确的Client/formal Kernel/S5 Host范围；随后先建立独立Task Contract和Change Record。`S-PROTO-002`仅在需要扩展为Client/wire/rebarrier/cross-version recovery时才需要新的版本化合同。

## 8. 留痕完整性自检

每次准备报告“继续推进”“阶段完成”“可以进入下一阶段”或准备提交时，都应按以下检查：

```text
[ ] Resume Card 的当前阶段、Change ID、下一步和阻塞是否与工作树一致？
[ ] 当前阶段的实际证据是否已追加到本台账？
[ ] 每个脚本 diff 是否有 Change Record 和 Ledger 覆盖？
[ ] Change Record 是否列明 authority、实际文件、验证、未验证项和回滚？
[ ] 是否执行了 Tools/Validate-ChangeLedger.ps1？
[ ] 是否把 C++ release trace、Unity/Server checksum、Player/公网结果明确区分？
[ ] 是否避免把“代码已写”写成“阶段 VERIFIED”？
[ ] 是否把新的故障写成独立 issue，而不是埋在聊天记录里？
```

若任意一项为否，阶段保持原状态或降级，不能仅因对话压缩、时间过去或下一项工作看起来更有趣而跳过。
