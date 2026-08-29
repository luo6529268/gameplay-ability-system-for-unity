# NTSD S0～S9 服务器权威帧同步：进度、证据与问题台账

> **总账入口（2026-08-29）：** 本文是 S0～S9 当前状态、阻塞和下一步的总账；详细阶段方案统一从 [`ServerLockstepStages/README.md`](ServerLockstepStages/README.md) 进入。Task Contract 只定义一个实现包，Change Record 只记录一次实际改动；Audit/Decision 继续是证据和用户决议的唯一来源。阶段档案创建未改变下表任何状态。

| 阶段 | 当前状态（原样保留） | 阶段档案 | 当前正式门槛 |
|---|---|---|---|
| S0 | `WITNESS_CODE_WRITTEN / VALIDATION_ACTIVE / NOT_VERIFIED` | [S0](ServerLockstepStages/S0-formal-authority-baseline.md) | fresh compile、当前 focused 7/7、existing 9/9、self-check 与 formal classification |
| S1 | `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS` | [S1](ServerLockstepStages/S1-authority-input-protocol.md) | S0 `VERIFIED`、formal Kernel/tick mapping、Client capture/wire |
| S2 | `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS` | [S2](ServerLockstepStages/S2-weak-network-frame-delivery.md) | S1 `VERIFIED`、GP decisions、真实Client消费与弱网矩阵 |
| S3 | `NOT_STARTED` | [S3](ServerLockstepStages/S3-snapshot-history-recovery.md) | S2 `VERIFIED`、formal snapshot/history/recovery contract |
| S4 | `NOT_STARTED` | [S4](ServerLockstepStages/S4-presentation-prediction-decision.md) | S3 `VERIFIED`；预测不是必做功能 |
| S5 | `NOT_STARTED` | [S5](ServerLockstepStages/S5-shared-kernel-independent-host.md) | S4 `VERIFIED`、shared formal Kernel、actor/atomic fault contract |
| S6 | `NOT_STARTED` | [S6](ServerLockstepStages/S6-real-transport.md) | S5 `VERIFIED` 与授权 endpoint/security 环境 |
| S7 | `NOT_STARTED` | [S7](ServerLockstepStages/S7-public-weak-network-runtime.md) | S6 `VERIFIED` 与授权公网/移动测试矩阵 |
| S8 | `NOT_STARTED` | [S8](ServerLockstepStages/S8-control-plane-multi-room.md) | S7 `VERIFIED` 与产品/安全/资源决策 |
| S9 | `NOT_STARTED` | [S9](ServerLockstepStages/S9-release-capacity-operations.md) | S0～S8新鲜证据与发布授权 |

> **最新输入决议与实现结果（2026-08-29，优先于下方所有 pending 表述）：** 用户指定 Server [`NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md) 及其 SHA-256 已核对的完整原版报告为 `S-PROTO-001` 和 human missing carry 决议。正式合同是 Client held-only、Server从previous/current locked held派生edge、首帧all-released、AI无Client owner；中央Server在deadline将缺失human解析为neutral，held carry上限0。`S1-SERVER-FORMAL-FRAME-INPUT-CONTRACT-001`已在Server Protocol/BattleHost/tests范围关闭为`FOCUSED_TEST_PASS / SERVER_HUMAN_AUTHORITY_INPUT_READY / CLIENT_PAUSED / S1-S2-PREIMPLEMENTATION`；test-first、focused、Debug/Release十项目`0/0`、full Server tests、no-network local host和治理校验通过。不改Client/wire/transport/formal Kernel/recovery，不选择numeric grace/delay/deadline，也不晋升S1/S2 VERIFIED。
> **最新 Server-first 状态同步（2026-08-25，优先于下方旧的“最近完成”表述）：** `S0-SERVER-BOOTSTRAP-NODE-IDENTITY-001 = FOCUSED_TEST_PASS / SERVER_BOOTSTRAP_NODE_IDENTITY_READY / S0_SERVER_FIRST_CORRECTION / CLIENT_PAUSED` 与 `S1-SERVER-POLICY-VERSION-VALUE-001 = FOCUSED_TEST_PASS / SERVER_POLICY_VERSION_VALUE_READY / S1_SERVER_FIRST_PREIMPLEMENTATION / CLIENT_PAUSED` 已关闭。前者使 Protocol-owned `NodeId` 成为本地 bootstrap/health 的有效身份事实；后者使既有 Model B/C1 的 `PolicyVersion` 成为 Protocol/BattleHost 强类型，并保留 `InputSubmission` 不含 policy field、activation/journal/ACK/ready 行为及已形成 `TargetTick`/`InputDelayFrames` 语义不变。两包均有 test-first、focused、Debug/Release 十项目 `0/0`、full Server tests、no-network local host、declared-path audit 和最终 Server workflow/Ledger 证据；它们均不闭合 formal S0/S1/S2，不授权 Client、wire、transport、snapshot/recovery、rebarrier、missing-input 或 battle-rule 修改。最新 Server 选择审计为 [`S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md)：当前无活跃/READY 源码包，但 Server-first 总目标仍 active；只等待命名的 `S-PROTO-001`、`S-NET-001/002` 或 Client/formal-Kernel/S3/S5 gate，而非再次请求泛化 Server 授权。
> 状态：最近完成`S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001 = FOCUSED_TEST_PASS / SERVER_POLICY_ACTIVATION_SCHEDULE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION`；用户已确认Model B，Server room/session以target authority tick解析future-effective policy activation并把resolved version写入immutable locked envelope history，`InputSubmission`仍不带per-submission `PolicyVersion`。它不选择capture、missing-input、AI、Kernel、Client、wire、InputDelay、rebarrier或recovery行为。前置 S1 tick/future bound、S2 redundancy-ingress capacity、disorder/gap/redundancy/ready buffer/ACK 与其他packages也均为 focused-test ready。本台账只追踪服务器阶段；它不把单机 U0～U9、C++→Unity 重新对齐、T8 默认 `stage.dat` 或 Android 真机任务误写为服务器已完成。  
> 最新 C1 状态（2026-08-25，优先于上句“最近完成”历史措辞）：`S2-SERVER-CROSS-POLICY-ACTIVATION-JOURNAL-001 = FOCUSED_TEST_PASS / SERVER_CROSS_POLICY_JOURNAL_READY / CLIENT_PAUSED / S2-PREIMPLEMENTATION`。用户确认的Server-only C1实现activation journal独立cursor/ack、next-tick resolved `ServerProgress.PolicyVersion` 与acknowledged-prefix cross-policy gap/ready guard；test-first red、Debug/Release十项目`0/0`、full Server tests、no-network local host、declared C1 audit和final Ledger`24 / 78`均通过。它不闭合S2/S3，且不授权Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input工作。  
> 缺失输入与网络时间决策状态（2026-08-29，取代此前 pending 表述）：Server [`PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md) 已记录 held carry=0、deadline missing-neutral、PvP/PvE完整30秒后formal-Kernel AI barrier、比赛继续、GP-06可重连，以及约1秒short-grace测试候选；GP-01又确认固定30Hz与本局tick/delay语义、按Client动态冗余/补发/gap、连续消费、有界追帧和表现隔离。formal AI/ownership/recovery和production timing数值仍待实现或实测；这些确认不授权任何 Client 或 Server 源码。
> ownership barrier 接线门禁（2026-08-29）：Server [`S1-S2-OWNERSHIP-BARRIER-PENDING-SUBMISSION-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-S2-OWNERSHIP-BARRIER-PENDING-SUBMISSION-PREREQUISITE-001.md) 已确认当前 human assembler 会按静态 connection slot set 预收 future TargetTick，且会把所有缺失 canonical slot 填成 human neutral；因此不能直接接入独立 ownership schedule。后续必须一起版本化 target-tick ownership admission、已接受输入保留、Windows 双本地玩家动态 slot subset、formal AI step 与 frame/checksum 原子提交；禁止丢弃输入、让Client提交AI输入或把AI-owned tick伪装成missing-neutral。该审计没有修改Client或Server源码。
> connection liveness Server 结果（2026-08-29）：`S2-SERVER-CONNECTION-LIVENESS-WITNESS-001 = FOCUSED_TEST_PASS / SERVER_CONNECTION_LIVENESS_WITNESS_READY / CLIENT_PAUSED / PRODUCTION_CLOCK_BARRIER_RECOVERY_PENDING / S1_S2_PREIMPLEMENTATION`。Protocol/BattleHost 现有 caller-supplied monotonic milliseconds、explicit versioned timeout、heartbeat sequence/idempotence/stale、exact `elapsed >= timeout` 与 immutable first witness；test-first 18个预期`CS0246`、Debug/Release十项目`0/0`、full Server tests、no-network Host和治理检查通过。它不是production clock/transport、ownership activation、AI、Client、recovery或S1/S2 VERIFIED。
> ownership-aware human admission Server 结果（2026-08-29）：`S1-SERVER-OWNERSHIP-AWARE-HUMAN-ADMISSION-001 = FOCUSED_TEST_PASS / SERVER_OWNERSHIP_AWARE_HUMAN_ADMISSION_READY / CLIENT_PAUSED / FORMAL_AI_CLOCK_RECOVERY_PENDING / S1_S2_PREIMPLEMENTATION`。Server现按TargetTick校验Android/Windows真实human-owned slot subset；human→AI barrier与已接受future input重叠时不改schedule/journal并保留first witness；AI-owned next tick返回`FormalAiKernelRequired`，不写neutral、不删pending、不推进tick/history。Debug/Release十项目`0/0`、full Server tests、no-network Host和治理检查通过；Client/formal AI/clock/transport/recovery仍未实现，S1/S2状态不变。
> timeout→ownership causality Server 结果（2026-08-29）：`S2-SERVER-TIMEOUT-OWNERSHIP-REQUEST-001 = FOCUSED_TEST_PASS / SERVER_TIMEOUT_OWNERSHIP_CAUSALITY_READY / CLIENT_PAUSED / TICK_PLANNER_FORMAL_AI_RECOVERY_PENDING / S1_S2_PREIMPLEMENTATION`。只有当前tracker实际保留的timeout witness可请求caller明确给出的future tick；Server自动覆盖该connection当时仍human-owned的全部slot，Windows不会漏第二名本地玩家；已AI slot跳过，overlap不追加，same witness/same tick幂等，different tick冲突，accepted request追加contiguous causal journal。它不选择tick、不执行AI、不改Client、不实现production clock/transport/recovery；S1/S2状态不变。
> earliest-safe ownership barrier Server 结果（2026-08-29）：`S2-SERVER-EARLIEST-SAFE-OWNERSHIP-BARRIER-001 = FOCUSED_TEST_PASS / SERVER_EARLIEST_SAFE_OWNERSHIP_BARRIER_READY / CLIENT_PAUSED / FORMAL_AI_CLOCK_RECOVERY_PENDING / S1_S2_PREIMPLEMENTATION`。同一assembler调用内选择`max(NextAuthorityTick, last ownership activation successor, timed-out connection相关accepted pending TargetTick successor)`并立即走guarded schedule；其他connection pending不延迟，terminal activation/pending无successor时拒绝且不溢出。它不把30秒换算tick、不改TargetTick/InputDelay、不执行AI、不改Client、不实现production clock/transport/recovery；S1/S2状态不变。
> Hosting monotonic clock Server 结果（2026-08-29）：`S2-SERVER-HOSTING-MONOTONIC-CLOCK-001 = FOCUSED_TEST_PASS / SERVER_TIMEPROVIDER_MONOTONIC_ADAPTER_READY / CLIENT_PAUSED / SUSPEND_TRANSPORT_MEASUREMENT_PENDING / S1_S2_PREIMPLEMENTATION`。Hosting以.NET 10 injected `TimeProvider.GetTimestamp/GetElapsedTime`产生process-local `MonotonicMilliseconds`，System factory使用`TimeProvider.System`；frequency、sub-ms floor、equal/forward、regression no-mutation、frequency change和system smoke通过。未接heartbeat transport/timer/room/AI/Client/recovery，官方未统一保证的OS/VM suspend语义仍待实测；S1/S2状态不变。
> connection grace classification Server 结果（2026-08-29）：`S2-SERVER-CONNECTION-GRACE-CLASSIFICATION-001 = FOCUSED_TEST_PASS / SERVER_CONNECTION_GRACE_CLASSIFICATION_READY / CLIENT_PAUSED / PRODUCTION_MEASUREMENT_PENDING / S1_S2_PREIMPLEMENTATION`。Versioned policy显式携带short grace与full heartbeat timeout；Server从同一last-seen timeline派生Healthy/Reconnecting/TimedOut。1,000/30,000ms只作为focused fixture，精确999/1000/29999/30000边界、递增heartbeat恢复、duplicate/stale不续命、legacy timeout兼容通过；只有完整timeout生成first witness。没有Client、transport、ownership、AI、input或recovery副作用，S2仍非VERIFIED。
> S2 exit-gate 对账（2026-08-29）：Server [`S2-EXIT-GATE-COVERAGE-RECONCILIATION-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S2-EXIT-GATE-COVERAGE-RECONCILIATION-001.md) 逐项核对当前源码/测试。`DequeueReady(maxFrames)`、contiguous ready range和两个disorder pump已证明Server oracle有界且不跨洞；它们不是实际Client Jitter/catch-up。真实catch-up owner属于未来`C-P2/C-P6`并须Android/Windows实测；TestKernel replicas不能冒充healthy Clients；OfflineLocal也未由Server证据覆盖。因此当前没有新的Server-only catch-up/Jitter源码包READY，不改Client/Server源码，不改变S2状态。
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
| 持续目标 | Server-first 持续推进；每个Server包必须声明`ClientImpact=NONE/AUDIT_REQUIRED/VERIFY_ONLY/MODIFY_REQUIRED`。Client门禁只阻塞相关包/阶段关闭，不暂停独立Server-only READY工作。 |
| 当前服务器阶段 | `S0` formal close 仍待；已完成多项 `S1/S2` Server-only preimplementation，绝不将其标记成阶段 `VERIFIED` |
| 阶段状态 | `S0_SERVER_ROOM_JOURNAL_READY / S0_FORMAL_CLIENT_PROOF_DEFERRED / S1_S2_PREIMPLEMENTATION` |
| 当前 Work Package | `S0-FORMAL-MULTIWORLD-WITNESS-VALIDATION-001 / ACTIVE`；Client compile/self-check已过，TestRunner pending。 |
| 当前 Change ID | Server orchestration `S0-FORMAL-MULTIWORLD-WITNESS-VALIDATION-001`；Client `S0-WITNESS-001 / COMPILE_PASS / FRESH_SELFCHECK_PASS / TEST_RUNNER_7_PLUS_9_PENDING`。 |
| 下一项允许动作 | Queue目前无READY源码包。下一次只恢复具名门禁：获批的formal shared-Kernel AI/state-hash Client包、后续authenticated transport/platform timing measurement，或满足前置合同后的S3 snapshot/history/recovery；不得再询问held-only/GP-01～GP-09，不得写hidden default、placeholder AI或未经授权Client/wire/transport。 |
| 当前外部阻塞 | S0 formal close 的 Client multi-world/ten-domain proof deferred；S2 formal close仍需真实 Client 连续消费、authenticated heartbeat与平台实测、单客户端黑洞/极端抖动矩阵、formal AI和recovery；S3/S5 又需 formal Kernel（当前 marker 为 false）。C++ release 只读审计确认其 `InputHandler::snapshot()` 是按键快照、`snapshot_phase210_table()` 是 UI/结算表，且 live RNG 为跨 input/game tick/collision/frame advance 的 global LCG，因此 formal snapshot 不能由 generic frame list 替代；字段级 inventory 见 `NTSD_Server/docs/ai/AUDITS/S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md`。此外，sequence/history retention仍需版本化决定，而`S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001`确认`ProtocolVersion=1`尚非有协商/ABI/rolling-upgrade/replay-supersede合同；三者均需formal/product/transport范围后才能实施。这些限制阶段 verified claim 和新源码范围，但不暂停 Server-first goal。C++ full-trace 观察链路仍为独立范围边界。 |
| 不允许做的事 | 不修改/编译/测试 Client；不接公网、不把 TestKernel 当正式 battle Kernel、不自行选择 neutral/carry/AI/断线规则、不宣称 S0/S1 `VERIFIED` |
| 最新服务器验证 | `S2-SERVER-CONNECTION-GRACE-CLASSIFICATION-001` test-first red为21个预期missing-API diagnostics；实现后Debug/Release十项目均`0 warnings / 0 errors`，Protocol/BattleHost/Architecture/Integration全通过；no-network Host仍为`NodeId=local-node / BootstrapReady / Liveness=True / Readiness=True / NetworkListenerStarted=False`；workflow `27/0/0/3/7`、Ledger `41/71`、ClientImpact矩阵`41/41`与formal stages`10/10`通过。它不是Client/runtime/C++ battle对齐或S0/S1/S2 `VERIFIED`。 |

持续目标的Client门禁：每个Server包先声明`ClientImpact`。若为`AUDIT_REQUIRED`，仅执行具名只读Client审计；若为`VERIFY_ONLY`或`MODIFY_REQUIRED`，先在本台账新增`CLIENT_INTEGRATION_REQUIRED`，列出文件/接口、原因、Server证据、最窄验收和回滚边界，再暂停该包并等待用户明确批准。该门禁不得暂停无关的Server-only READY包；不得借“共享代码”“顺手验证”或“只有一行”绕过授权边界。

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
| `S-NET-001` | `PRODUCT_BEHAVIOR_CONFIRMED / ZERO_CARRY_NEUTRAL / FORMAL_LIFECYCLE_PENDING` | deadline missing human立即neutral且held carry=0；PvP与合作PvE均在完整30秒前保持human ownership，30秒后于versioned barrier由formal Kernel AI接管，比赛继续；玩家之后仍可经GP-06 recovery在future barrier取回 | S1/S2/S3/S5 | 当前包只覆盖neutral；AI ownership、30秒barrier、reconnect/recovery仍需formal Kernel与S3/S5实现证据 |
| `S-NET-002` | `PRODUCT_DIRECTION_CONFIRMED / NUMERIC_MEASUREMENT_PENDING` | 固定30Hz与本局tick/delay语义，动态冗余/补发/gap、连续消费、有界追帧和表现隔离已由GP-01确认；held carry=0；short grace约1秒仅为测试候选；`InputDelayFrames`、deadline、production grace、冗余/gap/catch-up/history等数值仍由versioned配置与S2/公网测量决定 | S2 | Android/Windows内存弱网与中国大陆真实公网测量；禁止hidden default |
| `S-NET-003` | `PENDING_MEASUREMENT` | 是否实施有限本地预测 | S4 | S3 恢复闭环、错预测率、体验与成本 A/B |
| `S-NET-004` | `PENDING_EVALUATION` | 实际 battle transport 选型 | S6 | 移动端、MTU、拥塞、许可证、维护和协议等价测试 |
| `S-NET-005` | `PENDING_DEPLOYMENT` | 首批公网节点的 region、OS、端口、安全组和部署方式 | S6 前 | 资源所有者授权与实际环境清单 |
| `S-NET-006` | `PENDING_PRODUCT_RULE` | 全国匹配的地区优先、跨区兜底与队伍策略 | S8 | 用户规则、网络质量和容量数据 |
| `S-PROTO-001` | `CONFIRMED / SERVER_FOCUSED_TEST_PASS / CLIENT_WIRE_DEFERRED` | 原版和用户决议：human submission为owned-slot完整held，locked held唯一真值，Server/formal Kernel派生edge，首帧all-released，AI无Client owner；Client edge非mandatory canonical truth | Server-only held/ownership/edge/zero-carry范围已关闭；formal Kernel/Client/wire仍待 | `NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001`、完整原版报告、`S1-SERVER-FORMAL-FRAME-INPUT-CONTRACT-001` |
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
- 历史 focused NUnit：用户曾在现有 Editor 的 EditMode Test Runner 搜索 `InProcessLockstepAuthoritySessionEditorTests` 并运行当时的五项；截图为 5/5 pass、0 fail。该 fixture 现已扩展为 7 项，历史截图不能代替本次 7/7 fresh evidence；self-check 不调用该 Fixture，且项目下没有持久 TestResults，所以截图仅是历史会话证据。
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
- C++ source terms只能帮助确认当前发现的 local input path没有给出Server网络缺失策略；它不替代 C++ trace 或用户产品决定。当时 `S-NET-001`/`S-NET-002`仍为 pending；该历史状态已被 2026-08-29 的 held/zero-carry 与 GP-01、GP-03～GP-06 决议 supersede，当前以第4节决策表和文末最新记录为准。本包仍不是 S1 `VERIFIED`，也没有任何 Unity Client动作。

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
- 当时的清除方式是确认`S-PROTO-001`、提供`S-NET-001/002`产品规则，或授权明确的Client/formal Kernel/S5 Host范围；held/zero-carry 与 GP-01～GP-09 产品方向现已由后续决议关闭。当前仍须以 queue/最新决策重新选取具名源码包；production timing实测、formal Kernel/S3/S5及Client范围各自保持独立门禁，随后先建立独立Task Contract和Change Record。`S-PROTO-002`仅在需要扩展为Client/wire/rebarrier/cross-version recovery时才需要新的版本化合同。

### 2026-08-29：原版在线 held-only / zero-carry 决议与 Server 包启动

- 用户指定 `NTSD_Server/docs/ai/AUDITS/NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md` 为 `S-PROTO-001` 和 missing-input carry 的正式决议，不再询问 held-only。
- 完整报告 `NTSD28_ORIGINAL_ONLINE_LOCKSTEP_REPORT.md` 已逐行读取；SHA-256 为 `D356EB8D9C555593134A887A5E0EE41BF636A54F22D6592ECC3EFFEAC627AEBD`，与审计声明一致。
- 原版事实：八槽逐Tick wire只传current held；previous/current edge由消费端派生；同Tick屏障缺帧时阻塞/失败终止，没有carry、neutral、AI takeover或recovery。
- 当前产品适配：中央Server、Android一human slot、Windows最多两human slots、room最多20 humans；deadline missing为neutral，held carry=0；formal AI/ownership barrier和snapshot/history reconnect保留后续包。
- `S1-SERVER-FORMAL-FRAME-INPUT-CONTRACT-001` 已完成并关闭为`FOCUSED_TEST_PASS / SERVER_HUMAN_AUTHORITY_INPUT_READY / CLIENT-PAUSED / S1-S2-PREIMPLEMENTATION`：原版bit、deep-immutable held-only submission、Android 1 / Windows 1～2 / room 20 human ownership、稳定1/2/8/20聚合、all-released baseline、locked edge与deadline zero-carry neutral均有聚焦证据；Debug/Release十项目`0 warnings / 0 errors`、full Server tests、no-network local host、workflow/Ledger与diff检查通过。
- 未实现且不得误标完成：formal Kernel AI/state hash、PvP/PvE ownership barrier、numeric short grace/deadline/delay、Client capture/wire、snapshot/history recovery、transport、room actor/fault atomicity及S1/S2 `VERIFIED`。

### 2026-08-29：GP-01 公开成熟帧同步证据与方案修订

- `NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md` 保存了2017年腾讯TGDC/腾讯云等公开资料的可信度、历史方案和未知边界：固定频率输入序列、UDP应用层补发、Server内存帧历史、下行历史帧冗余、严格连续消费、有界追帧、逻辑/表现分离、插值、hash/录像/网络指标；不把历史公开约15Hz、5v5、前三帧冗余或2026当前未知实现照抄为NTSD事实。
- `GP-01` 已从“对局中可能在LowLatency/Balanced/WeakNetwork间切换”的易误解方向修订为：30Hz、已形成TargetTick/InputDelay语义和deadline rule保持固定；每个Client可按ACK/丢包/jitter动态调整上/下行冗余与补发；Client严格补齐连续frame后有界追帧，严重落后走snapshot+history；表现插值不反写逻辑；deadline后故障human slot仍zero-carry neutral，健康玩家继续。
- 已记录三点：不把NTSD逻辑改成15Hz；`P103`携带历史`F100..F103`不是预知未来；下行缺`F3`先由后续冗余/gap补齐再执行`F4`，上行迟到`F3`只能在deadline前采用。用户已于2026-08-29确认GP-01修订推荐方案；production数值仍须实测，且确认未授权源码。

### 2026-08-29：GP-02 极短真实点击 Client capture 合同确认

- 用户确认 GP-02 推荐方案：攻/跳/防和八方向数字键的有效极短 press 至少进入一个 30 Hz held tick；对角方向在同一 tick 原子提交双 bit；虚拟摇杆使用 deadzone/hysteresis/八方向量化，快速 flick只保存一个稳定方向，不逐个 latch 旋转中间扇区。
- Android 多点触控、pointer ownership、cancel/失焦/后台清理、30/60/90/120Hz 表现帧率下相同 capture trace，以及 Android/Windows canonical held 等价性进入后续 Client 验收。Server held-only、Server-derived edge、zero-carry neutral、30Hz和battle rules不变。
- 独立 Client 投影记录为`NTSD_Server/docs/ai/DECISIONS/ONLINE-GAMEPLAY-CLIENT-ADJUSTMENT-REGISTER-001.md / CONFIRMED_CLIENT_IMPACT / IMPLEMENTATION_DEFERRED`。该确认不授权 Client 源码、Scene、Input Actions、wire或transport修改，不改变S1及后续阶段状态。

### 2026-08-29：GP-03 / GP-04 / GP-05 跨模式 30 秒后 Server AI 接管决议

- 用户确认实时PvP与合作PvE都只在完整30秒 heartbeat timeout后由AI接管；30秒前保持human ownership并对每个deadline missing使用zero-carry neutral，short grace结束本身不触发任何模式的AI。
- 30秒到达后，掉线human slot必须在明确且可审计的ownership barrier切为formal-Kernel `ServerAiOwner`，比赛继续；同一tick不得由Client和AI共同拥有。PvP不因timeout直接判负/移除，PvE不因timeout直接失败，最终都按正常模式战斗规则结束。
- GP-04和GP-05据此关闭为CONFIRMED；旧的“PvP默认不由AI接管/30秒直接判负或移除”和“PvE short grace后立即AI接管”提案均被取代。仍待GP-06接管后human取回；short-grace生产值继续实测，input-silence timeout保留独立policy。该决议不授权formal AI、ownership journal/barrier、Client、wire、transport或recovery源码，也不改变S2/S3/S5阶段状态。
- 用户随后确认GP-03无异议：第一版以约1秒作为显式short-grace测试候选，只将“网络不稳定”分类为正式重连/断线状态，不改变两种模式统一的30秒AI接管时点；生产值仍需Android/Windows及大陆移动网络实测。GP-03关闭为CONFIRMED，但不授权源码或阶段晋升。
- 用户确认GP-06：实时PvP与合作PvE玩家即使晚于30秒、formal Server AI已经接管，只要对局仍在继续且identity可安全认证，仍允许重新连接；旧Client world必须丢弃，完成Server snapshot + contiguous authority history + checksum后，才可在future ownership barrier从AI手中取回。AI在recovery期间继续，同一tick不得双owner。GP-06关闭为CONFIRMED；S3实现、retention、identity与运行时验收仍NOT_STARTED/待证据。
- GP-07新增`NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-DESYNC-DISPOSITION-EVIDENCE-001.md`：历史王者公开分享与腾讯通用专利确认周期hash、mismatch通知、录像/日志保存、限定窗口函数参数对比、自动化/体验服定位；没有公开线上玩家的恢复次数、snapshot重同步或断开处置。NTSD“首次权威恢复一次、恢复后仍错则断开”仍是待用户确认的自有方案，不得冒充王者事实或据此授权S3源码。
- 用户随后确认GP-07推荐方案：首次checksum mismatch保存first-difference证据，暂停目标Client human input并执行一次Server snapshot + contiguous authority history recovery；成功后future ownership barrier交回，同一次权威恢复后仍mismatch则断开当前Client session，继续允许按GP-06重新连接。Server/健康Client继续，禁止Client覆盖Server、无限恢复和单次mismatch自动认定作弊。GP-07关闭为CONFIRMED；S3仍NOT_STARTED。
- GP-08新增`NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-SERVER-FAULT-ISOLATION-EVIDENCE-001.md`：历史王者公开架构支持大厅/PvP/Proxy/房间等模块分离、故障模块自动屏蔽、在线扩容和限制影响面，可借鉴“单room故障不拖Host”；没有公开单局tick半执行后的rollback、迁移、恢复或玩家结果。用户随后确认NTSD自有处置：无法证明atomic completion时room立即fault并停止输入/tick，本局无效且不判任何玩家负，保存不可覆盖的first-fault witness，其他room与Host继续；在S3/S5完成snapshot、atomic commit/rollback和injected-fault证据前禁止自动恢复。GP-08关闭为CONFIRMED，但S5仍NOT_STARTED。
- GP-09产品表现已确认：PvP与合作PvE均不支持普通玩家暂停authority world；“投降/主动退出”均为个人离场，不结束整局或团队对局，使用可靠、版本化、幂等room/session command，并复用missing-neutral、完整30秒后ownership barrier切给Server AI及GP-06可重连取回。原版global/function one-shot mask live consumer仍待只读审计，但它是工程证据项，不再要求用户选择未知bit。该记录不授权room command或Client/Server源码。
- 用户已确认GP-01修订推荐方案，因此GP-01～GP-09产品表现现已全部冻结。GP-01的production InputDelay/deadline/redundancy/gap/catch-up/history数值仍须Android/Windows、中国大陆移动网络及1/2/8/20 human矩阵实测；全部GP确认不自动冻结数值、不授权源码，也不改变S0～S9阶段状态。
- `NTSD_Server/docs/ai/DECISIONS/ONLINE-GAMEPLAY-CLIENT-ADJUSTMENT-REGISTER-001.md` 已完成全部GP的Client影响整合：去重为capture、连续authority-frame消费、连接/ownership生命周期、统一recovery、room disposition、表现与诊断owners，并按preflight→capture→frame consumer→ownership→recovery→room fault→presentation拆成未来候选包。该映射是文档结果，真实路径preflight、Client授权和所有源码包仍为PENDING/NONE READY。
- `GOVERNANCE-SERVER-CLIENT-IMPACT-TRACEABILITY-001`已完成：`NTSD_Server/docs/ai/TRACEABILITY/SERVER-MODULE-TO-CLIENT-IMPACT-MATRIX-001.md` 精确映射Ledger `34/34`、formal S0～S9 `10/10`，missing/extra/duplicate `0/0/0`、broken links `0`，并区分已关闭Server包自身的ClientImpact和下游formal Client gate。该矩阵防止遗漏Client同步，也禁止把一个Client gate泛化为整个Server-first目标暂停；它不授权源码或改变阶段状态。
- `S1-SERVER-OWNERSHIP-ACTIVATION-SCHEDULE-001`已Server-focused关闭：新增immutable initial human/ServerAI owners、atomic future-effective multi-slot transitions、idempotence/first-conflict与contiguous ownership journal；test-first `CS0246`、Debug/Release `0/0`、full Server tests、no-network Host和content boundary通过。它不含30秒timer、AI input/provider/state hash、assembler、Client、wire、transport或recovery；S1/S2仍非VERIFIED。
- `CLIENT_INTEGRATION_REQUIRED-FORMAL-KERNEL-AI-STATE-HASH-001`原只读审计已由2026-08-29用户授权更新：必要Client源码可在具名Task/Change下修改。当前先恢复`S0-WITNESS-001`验证；future formal shared-Kernel owner为`USER_AUTHORIZED / PACKAGE_NOT_STARTED`。Server formal marker仍false，禁止generic missing policy/TestKernel或复制Server AI。
- Heartbeat时钟只读审计确认：30秒AI接管是已冻结产品结果，但当前没有versioned monotonic clock owner、session/heartbeat sequence、suspend/discontinuity语义、first-timeout witness，或不覆盖既有future TargetTick的timeout→authority barrier映射；现有logical frame deadline不能充当wall-clock heartbeat。因此没有新的timer源码包READY，不得写DateTime/Stopwatch/Thread.Sleep或hidden clock default。
- `S2-SERVER-CONNECTION-GRACE-CLASSIFICATION-001`已Server-focused关闭：policy显式要求`shortGrace < heartbeatTimeout`且无hidden default；tracker从同一Server last-seen timeline派生Healthy/Reconnecting/TimedOut，Reconnecting不生成witness、不改变ownership/AI/input/history。Test-first 21个预期missing-API diagnostics、Debug/Release十项目`0/0`、full Server tests、no-network Host通过。1秒仍是测试候选，production值、真实transport/suspend、Client C-P3、formal AI和S3 recovery继续保持门禁。

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
