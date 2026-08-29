# CODEX-CURRENT-HANDOFF

> **当前授权/活跃包（2026-08-29）：** `S0-WITNESS-001 / COMPILE_PASS / FRESH_SELFCHECK_PASS / TEST_RUNNER_7_PLUS_9_PENDING`。Current assemblies postdate witness source，fresh self-check于15:56:23Z为PASS；当前S0 fixture是7项、existing lockstep是9项。Computer Use未获Unity控制批准，需UI控制授权或用户手动运行。Scene/地图/背景用户改动、Input Actions、30 Hz、battle rules、transport与recovery不在范围。

> 生成日期：2026-08-24  
> 最后更新：2026-08-29
> 用途：将旧 Codex 任务 `01a02f58-c229-7830-a50b-7406c1d7d061` 最近三天有效事实迁移到当前持续目标；后续不依赖旧会话。  
> 证据口径：本文件区分“已观察/已验证”“用户明确决定”“推断/待验证”。它不取代 C++ release 的 battle authority，也不把历史 self-check 写成完整 C++ 对齐证书。
> 最近实测更新：2026-08-24，`S0-INPROC-AUTHORITY-001` 已取得 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING` 的 validation-only 证据；本更新不修改 Unity Client，也不把 S0 写成 `VERIFIED`。

## 1. 当前结论

- **当前 Client 仓库**是 `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，当前分支为 `NTSD_2.4_C++`（HEAD `2c53f1eb`）；Unity 项目版本为 `2022.3.62f3`。根工作树已有大量用户/历史修改和未跟踪文件，不能清理、回退、批量格式化或顺手提交。
- **当前主线仍是“独立服务端优先”**：`S0-SERVER-BOOTSTRAP-001`、`S0-SERVER-INMEMORY-AUTHORITY-001`、`S0-SERVER-ROOM-JOURNAL-001`、S1 authority-frame/adapter/deadline packages 与 S2 Server-only preimplementation 已完成各自范围。用户已重新明确：先继续 Server 实现，Client 只暂停其自身源码/导入/编译/测试，不暂停 S0～S9 总目标。
- **最新 held-only Server 结果（2026-08-29，优先于下方旧 pending/active 表述）：** 用户指定的 Server `NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md` 已作为 `S-PROTO-001` 与 human missing carry 决议完整执行。`S1-SERVER-FORMAL-FRAME-INPUT-CONTRACT-001` 已关闭为 `FOCUSED_TEST_PASS / SERVER_HUMAN_AUTHORITY_INPUT_READY / CLIENT_PAUSED / S1-S2-PREIMPLEMENTATION`：原版稀疏 bit、deep-immutable held-only submission、Android 1 / Windows 1～2 / room 20 human ownership、稳定 1/2/8/20 聚合、all-released baseline、Server locked edge、deadline neutral 与 held carry 0 均通过 test-first、focused、Debug/Release 十项目 `0/0`、full Server tests、no-network local host 和治理校验。没有执行任何 Client 动作，也未实现 formal Kernel AI/state hash、numeric grace/deadline/delay、ownership transfer、snapshot/history recovery、wire/transport 或 S1/S2 `VERIFIED`。
- **当前逐项产品决策（2026-08-29）：** Server `DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md` 是单项确认清单；当前 `GP-01` 已按公开成熟帧同步资料修订为“固定30Hz与既有TargetTick/InputDelay语义，按每个Client动态调整冗余/补发，严格连续补帧、有界追帧、严重落后snapshot recovery、表现插值不反写逻辑、deadline后故障human slot neutral”，状态仍为`USER_CONFIRMATION_PENDING`。证据与不可照抄边界见 Server `AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md`；这不是源码授权。
- **阶段治理结构（2026-08-29）：** `GOVERNANCE-S0-S9-STAGE-DOSSIERS-001` 已关闭为`FOCUSED_TEST_PASS / STAGE_DOSSIERS_READY / GOVERNANCE_CLOSED / PHASE_STATUS_FROZEN`。总设计保留跨阶段不变量；`server-lockstep-s0-s9-progress.md`是状态总账；[`ServerLockstepStages/README.md`](ServerLockstepStages/README.md)索引十份固定模板阶段档案；package Task Contract 与 Change Record 继续分别拥有包范围和实际改动证据。该治理包只调整文档与必要`.meta`，所有阶段状态保持原样，不能因档案创建而晋升`VERIFIED`。
- **最新 Server-first 同步（2026-08-25，优先于本文件下方旧的“最近完成”历史叙述）：** `S0-SERVER-BOOTSTRAP-NODE-IDENTITY-001 / FOCUSED_TEST_PASS / SERVER_BOOTSTRAP_NODE_IDENTITY_READY / S0_SERVER_FIRST_CORRECTION / CLIENT_PAUSED` 已使有效 `NodeId` 成为 no-network bootstrap/health 输出事实；`S1-SERVER-POLICY-VERSION-VALUE-001 / FOCUSED_TEST_PASS / SERVER_POLICY_VERSION_VALUE_READY / S1_SERVER_FIRST_PREIMPLEMENTATION / CLIENT_PAUSED` 已将用户确认 Model B/C1 的既有 policy identity 收束为 Protocol/BattleHost 强类型。后者保留`InputSubmission`无 policy field、activation journal/cursor/ack、next-tick `ServerProgress`和acknowledged-prefix gap/ready边界，绝不改变`TargetTick`/`InputDelayFrames`语义。两包均完成test-first、focused、Debug/Release十项目`0/0`、full Server tests、no-network local host、declared-path audit与Server final workflow/Ledger`31 / 51`；均不代表formal S0/S1/S2 `VERIFIED`，不授权 Client/wire/transport/snapshot/recovery/rebarrier/missing-input/battle-rule工作。
- **当前范围校正（优先于本文件后方的历史 Client validation 授权记录）：** 目前不得修改、导入、编译、测试、self-check 或回滚 Unity Client。S2 已完成的 Server-only protocol-owner 覆盖为 sequence/conflict、deadline boundary、ACK/confirmed range、redundancy、bounded ready buffer、inbound/downlink logical disorder 和 gap response；但 S2 正式关闭仍需真实 Client 连续消费、单客户端黑洞/极端抖动矩阵及用户批准的 grace/neutral/recovery 行为。`KernelAbstractionsAssemblyMarker.IsFormalBattleKernelImplemented=false`，所以也不得用 generic/TestKernel snapshot 冒充 S3/S5 formal Kernel 进展。这是阶段/范围门槛，不是总目标暂停。
- **当前 formal 输入边界（2026-08-29）：** held-only、Server-derived edge、all-released baseline 与 deadline zero-carry neutral 已由上述 Server-only 包落实，不得再次询问。仍待的是 Client capture/wire、formal Kernel AI/state hash、authority-frame-to-world tick mapping、ownership barrier 和 recovery；这些必须另建明确包，不能反写成当前包已完成。
- **前一完成 Server 输入值包（2026-08-25）：** `S1-SERVER-HUMAN-FRAME-INPUT-VALUE-001 / FOCUSED_TEST_PASS / SERVER_HUMAN_FRAME_INPUT_VALUE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION / EXHAUSTIVE-REGRESSION-HARDENED`。上位设计与C++ release `InputHandler::poll`/`battle_bootstrap.cpp`共同支持并已实现不可变七action human held/pressed/released value、edge derivation/order、equality/hash与Server Protocol tests。test-first missing-type red evidence、全`128 × 128 = 16,384` pair regression、Debug/Release `0/0`、focused/full Server tests、no-network local host、declared-source content audit与final Ledger`22 / 72`均有证据。它不改Client、不绑定capture/tick mapping、不选择missing-input、AI、Kernel、transport/recovery或S1验证。
- **最近完成 Server policy 包（2026-08-25）：** `S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001 / FOCUSED_TEST_PASS / SERVER_POLICY_ACTIVATION_SCHEDULE_READY / CLIENT-PAUSED / S1-PREIMPLEMENTATION`。用户确认Model B：`InputSubmission`不带per-submission `PolicyVersion`；Server room/session以target authority tick解析append-only future-effective activation schedule，并把resolved version写入现有immutable locked envelope history。已覆盖activation exact boundary、已接受future `TargetTick`不重算、locked history不回写、schedule ordering/terminal/contract mismatch拒绝以及room adapter顺序执行不变。test-first red、Debug/Release十项目`0/0`、full Server tests、no-network host、declared-source audit与Ledger`23 / 74`均通过。它不改Client、transport、battle rules、30 Hz、missing-input、InputDelayFrames、rebarrier、cross-version recovery或S1验证。
- **C++ battle-entry input 前置结论（2026-08-25，只读）：** Server [`S1-FORMAL-INPUT-BOOTSTRAP-CAPTURE-BOUNDARY-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-FORMAL-INPUT-BOOTSTRAP-CAPTURE-BOUNDARY-001.md) 证明battle bootstrap及首callback会清除key/prev/cooldown/history：tick 1的`poll`结果不会进入normal `apply_input`，normal human input消费只在更晚的`world.game_tick > 1` callback发生。因此all-released是C++ pre-history事实，但不能拿它推断generic `InitialAuthorityTick`、Client capture或StartBarrier formal mapping。
- **S1 policy binding 已确认且有 Server-only 证据（2026-08-25）：** Server [`PENDING-S1-POLICY-VERSION-BINDING-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-POLICY-VERSION-BINDING-001.md) 记录的Model B已由用户确认并由[`S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001`](../../../../NTSD_Server/docs/ai/CHANGE-RECORDS/S1-SERVER-POLICY-ACTIVATION-SCHEDULE-001.md)实现：不向`InputSubmission`追加policy field，StartBarrier initial policy加Server-owned future-effective activation schedule按target authority tick解析，resolved policy写入immutable envelope history。已形成`TargetTick`或`InputDelayFrames`语义不得通过该包改变；需要变化须另建rebarrier/versioned contract。Client capture/wire、cross-boundary redundancy、reconnect/replay和stale disposition仍未定也未实现。
- **Cross-policy history consumer 门槛（2026-08-25，只读）：** Model B schedule已使Server authority history可含多个resolved policy version；但现有gap responder、ready buffer、ACK tracker仍exact-match initial contract policy。于是later-policy progress/envelope/gap range的canonical delivery、ACK、ready、reconnect/replay与failure witness尚未定义。详见[`S2-S3-CROSS-POLICY-HISTORY-CONSUMER-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S2-S3-CROSS-POLICY-HISTORY-CONSUMER-PREREQUISITE-001.md)。不得通过放宽guard、加per-submission field、Client/transport/recovery或rebarrier代码绕过；需要新的版本化合同和独立Change Record。
- **已确认的 cross-policy C1 合同：** [`PENDING-S2-S3-CROSS-POLICY-HISTORY-DELIVERY-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S2-S3-CROSS-POLICY-HISTORY-DELIVERY-001.md) 现记录用户确认的activation journal/cursor独立immutable history fact、per-envelope resolved witness，以及仅在receiving side已确认journal prefix后允许mixed-policy frame range。它不授权Client、wire、transport或recovery实现。
- **最近 C1 Server 结果（2026-08-25）：** 用户授权的[`S2-SERVER-CROSS-POLICY-ACTIVATION-JOURNAL-001`](../../../../NTSD_Server/docs/ai/CHANGE-RECORDS/S2-SERVER-CROSS-POLICY-ACTIVATION-JOURNAL-001.md) 已在其Server Protocol/BattleHost与Server tests范围关闭为`FOCUSED_TEST_PASS / SERVER_CROSS_POLICY_JOURNAL_READY / CLIENT_PAUSED / S2-PREIMPLEMENTATION`：activation journal cursor/ack、`ServerProgress` next-tick policy语义和确认prefix保护的gap/ready边界已实现，Debug/Release十项目`0/0`、full Server tests、no-network host、C1 source audit与final Ledger `24 / 78`均通过。不得修改Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input规则。
- **当前执行门禁（2026-08-29）：** 当前无 ACTIVE/READY Server 源码包。下一精确 gate 是 order 2 的 formal Kernel AI ownership/state-hash 与实测 numeric short-grace/deadline/delay 合同；重连仍依赖 S3 snapshot/history recovery。此状态不是 Server test 失败，也不授权 placeholder AI、hidden default、Client/wire/transport 或 generic recovery。
- **最新选包结论（2026-08-25）：** Server [`S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S1-SERVER-FIRST-NEXT-SOURCE-AUDIT-001.md) 已完成。SessionId、NodeId与当前已实现的Model B/C1 PolicyVersion边界均已收束；当前无下一个可直接开工的Server源码包。该结论不是目标暂停或泛化授权请求：最早触发为`S-PROTO-001`的明确输入edge/capture合同，独立触发为`S-NET-001/002`，其余为已记录的Client/formal-Kernel/S3/S5 gate。收到命名决定后，应只更新相应Server queue row为READY并立即建立Record，不再重复索取“允许继续Server”的确认。
- **Server generic terminal-tick boundary（2026-08-25，只读）：** [`S1-S2-TERMINAL-AUTHORITY-TICK-BOUNDARY-AUDIT-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-S2-TERMINAL-AUTHORITY-TICK-BOUNDARY-AUDIT-001.md) 确认当前generic contract把`long.MaxValue`限定为terminal next cursor或exact empty ready range，未发现新的独立源码缺陷；当前工作树再次取得Debug `0/0`、Release Server self-hosted chain、no-network local host与Ledger`21 / 70`证据。它不等同于C++ tick mapping、history/recovery、formal Kernel、Client runtime、transport或阶段`VERIFIED`。
- **C++ release snapshot/RNG 前置结论（2026-08-25，只读）：** release `Makefile` 确认 live source set 包含 `simulation_tick_driver.cpp`、`game_tick.cpp` 和 `input_handler.cpp`。`InputHandler::snapshot()` 仅复制上一帧按键，`snapshot_phase210_table()` 仅保存结算/UI 表，并非已确认的 BattleWorld snapshot；`g_ntsd_rand_seed` 的 LCG 又在 input、game tick、collision、frame advance 中被广泛消费。因此未来 formal snapshot/recovery 必须覆盖 battle-world state、slot/generation、event cursor 与精确 RNG seed/call ordering，不能以 generic/TestKernel frame history 替代。本结论不授权实现 snapshot。
- **字段级前置清单：** Server 侧 [`S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md) 记录必须 capture/restore 的 C++ release domain、明确排除项和 future implementation gate；它是分析文档，不是 snapshot 已实现的证据。
- **restore 顺序补充：** `spawn_at` 会 reset slot cooldown 行/列；因此 `s_arest`/`s_vrest` 和外部 battle globals 只能在所有 stable slot rehydrate 后恢复。详见同一 Server audit，仍不是 snapshot 已实现的证据。
- **全阶段证据矩阵：** Server 侧 [`S0-S9-FORMAL-READINESS-MATRIX-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-S9-FORMAL-READINESS-MATRIX-001.md) 将 S0-S9 的需求、当前证据、未闭合门槛和合法下一步逐项列出；它不把 Server-only 测试扩展成阶段 `VERIFIED`。
- **最近完成的 Server correction 是 `S2-SERVER-READY-BUFFER-HORIZON-001 / FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED / S2-CORRECTION`**：它让 generic `InMemoryAuthorityFrameReadyBuffer` 必须接收 caller-supplied、nonnegative future-envelope horizon；zero合法且无production jitter/delay default。non-late far envelope会在duplicate/conflict/capacity mutation前以 appended `RejectedFutureTickLimit` fail closed，避免远future tick占满有限buffer并拒绝near contiguous frame。invalid/zero/exact/over-limit/no-mutation/near-capacity/moving-window/disorder regressions通过；Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `19 / 70`和fixed-string scoped audit均通过。它不实现actual Client buffer、transport、ACK/retransmit、weak-network、history/recovery、battle rules、formal Kernel或S2 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-AUTHORITY-TICK-RANGE-001 / FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED / S1-CORRECTION`**：它定义 Protocol-owned addressable authority-frame tick range为`[0, long.MaxValue - 1]`，将`long.MaxValue`保留为final legal frame后的terminal next cursor。contract/barrier/input/envelope/frame/deadline均拒绝terminal frame fact；assembler在terminal cursor以`AuthorityTickExhausted`于任何missing-input/history mutation前fail closed，direct `InMemoryAuthorityRoom.TryAdvance(...)`也在frame construction前以`InMemoryAuthorityFrameRejection.AuthorityTickExhausted`返回`false`。final legal session/direct-room/journal/ready-buffer/progress/ACK cursor与terminal no-second-kernel/no-policy-fill/no-journal-append regressions通过；Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `18 / 70`和expanded fixed-string scoped audit均通过。它不宣称C++ `world.game_tick`同range，不选择30 Hz、battle rules、Client、transport、missing-input、formal Kernel或snapshot/recovery，更不是S1/S2 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-FUTURE-TARGET-BOUND-001 / FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED / S1-CORRECTION`**：它让 generic `InMemoryAuthorityFrameAssembler` 和其 room adapter 都必须接收 caller-supplied、nonnegative future-target distance；zero合法且无production `InputDelayFrames` default。越界 target 会在 sequence/pending mutation 前以 appended `RejectedTargetBeyondFutureLimit` fail closed，比较使用 `TargetTick - NextAuthorityTick`，避免 `next + limit` overflow。negative/zero/exact/over-limit/no-mutation/sequence reuse/moving-bound/near-terminal/adapter与既有fixtures均通过；Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `17 / 70`和fixed-string scoped audit均通过。它不选择 production delay、deadline/missing-input policy、raw packet/MTU/bandwidth、Client、transport、battle rules、formal Kernel或snapshot/recovery，更不是S1/S2 `VERIFIED`。
- **最新 Server 前置审计是 `S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001`（只读、非实现包）**：`submissionsBySequence` 会在对应frame lock后继续保留，但 ACK、redundancy和client-reported confirmed cursor都没有定义安全retirement proof。因此不得把 future-target cap误称为完整sequence-memory cap，也不得擅自加锁帧删除、LRU/count cap、sequence reset、new disposition、reconnect或snapshot逻辑；先取得 lifecycle/replay/overload的版本化协议/产品决定。
- **最新 S3 history 前置审计是 `S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001`（只读、非实现包）**：generic `lockedFrames` 与room journal无限append，gap responder的单次response cap不是retention cap，且其索引假定完整initial-prefix仍可用。不得把它们称为`FrameHistoryRing`或S3 recovery，也不得擅自truncate、引入hidden ring、history-expired error、snapshot/replay或Client恢复；先取得formal Kernel、retained range/snapshot base和recovery disposition合同。
- **最新 S1 protocol-evolution 前置审计是 `S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001`（只读、非实现包）**：`AuthorityFrameProtocolVersion=1`当前只是in-memory marker；exact-match与append-only source enum不是wire ABI/旧端兼容证明。不得私自bump version、添加serializer/capability/unknown fallback或Client/transport upgrade行为；先取得S5/S6的version meaning、bump rules、admission/upgrade/replay supersede合同和real serialization matrix。
- **前一项 Server correction 是 `S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001 / FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED`**：它使`InMemoryRedundantSubmissionIngress`拥有positive、caller-supplied actual-entry cap；oversized matching window在任何assembler delegation/状态变更前以`RejectedServerEntryLimit` fail closed，at-cap window保持原有顺序/outcomes。Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `16 / 70`和fixed-string scoped audit均通过。它不选择production redundancy count、raw packet/MTU/bandwidth policy、Client、transport、deadline/missing-input policy、battle rules、formal Kernel或snapshot/recovery，更不是S2 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-MISSING-INPUT-PROVENANCE-001 / FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED`**：它让 `AuthorityFrameInputSource` / `MissingInputFillReason` 只能构造六种一致、已知的 provenance pair，拒绝 cross-labelled 或 unknown enum；immutable envelope 与 generic missing-policy resolution使用同一个 Protocol owner。Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `15 / 70`和fixed-string scoped audit均通过。它没有选择任何 payload、grace、neutral、AI、disconnect/reconnect或产品规则，不实现 Client、battle rules、formal Kernel、snapshot/recovery、transport或数据库，更不是S0/S1 `VERIFIED`。
- **前一项 Server correction 是 `S1-SERVER-INITIAL-AUTHORITY-TICK-001 / FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED`**：它修复了合法 non-zero `AuthorityFrameProtocolContract.InitialAuthorityTick` 与 existing StartBarrier/session/journal 的零起点矛盾。StartBarrier 现保存 validated immutable tick origin，session/journal从同一 origin 起步，adapter在构造前拒绝 protocol/barrier mismatch；negative tick、non-zero direct session、room+journal、adapter以及mismatch fail-closed fixtures通过。Debug/Release各10项目`0 warnings / 0 errors`、focused/full Server tests、`SequentialSingleWriter` no-network local run、final Ledger `14 / 70`和scoped audit均通过。它不实现 Client、battle rules、formal Kernel、snapshot/recovery、transport、数据库或缺失输入产品规则，更不是S0/S1 `VERIFIED`。
- **C++ tick-identity 前置结论（只读）**：C++ `reset_battle_runtime()` 将 `world.game_tick`/`input_phase`/`g_frame_toggle` 归零，而 `step_one_tick -> game_tick` 会在所有 battle passes 前递增/切换它们。因此 Server `InitialAuthorityTick` 只是 generic authority-history identity，不能自动等同于 C++ world tick；future formal schema必须显式验证 `authorityFrameTick`、`worldCompletedTick` 和 `nextAuthorityFrameTick` 的关系。该审计不实现 snapshot/recovery，不解除 Client freeze，也不改变S0～S9验证状态。
- **S1 policy-version input-binding gate（只读）**：设计明确 session-wide policy version 和 future effective tick，但没有规定它必须位于每个 `InputSubmission`。当前 contract/envelope/progress/gap有 version，submission/redundancy window没有；应先决定 per-submission binding 或 session/connection binding，以及旧version window在activation tick前后的处置。详见 Server [`S1-POLICY-VERSION-INPUT-BOUNDARY-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-POLICY-VERSION-INPUT-BOUNDARY-001.md)。在决定前不得私自新增DTO字段、stale-policy rejection、connection state、Client或transport行为。
- **formal checksum first-difference gate（只读）**：current generic Server kernel仅能返回aggregate checksum，mismatch没有domain、slot/generation、RNG或event cursor；它不能证明S0/S3所需ten-domain witness。future formal Kernel必须在同一completed tick boundary保留版本化domain list和first difference；C++ runtime views只是inventory线索。详见 Server [`S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md)。不得以TestKernel/aggregate包装或Unity表现状态伪造此门槛。
- **前一项 S2 correction 是 `S2-SERVER-DISORDER-ACTION-VALIDATION-001 / FOCUSED_TEST_PASS / SERVER_DISORDER_ACTION_VALIDATION_READY / CLIENT-PAUSED`**：两个公开 disorder instruction constructor 的未知 enum fail-closed guard与聚焦fixtures已写入；Debug/Release各10项目`0 warnings / 0 errors`、inbound/downlink invalid-action fixtures及既有合法行为、`SequentialSingleWriter` no-network local run、final Ledger `13 / 70`和scoped audit均通过。它只修复非法 enum，不改变正常 Deliver/Drop、delivery order、budget、deadline、ACK、ready/gap、battle state 或网络范围；不是S2 `VERIFIED`。
- **最近完成的 Server 包是 `S2-SERVER-INMEMORY-SUBMISSION-DISORDER-001 / FOCUSED_TEST_PASS / SERVER_INBOUND_DISORDER_READY / CLIENT-PAUSED / S2-PREIMPLEMENTATION`**：它向既有redundancy ingress预编排投递或丢弃完整input windows的harness与聚焦fixtures已写入，覆盖inbound logical delay/drop/duplicate/reorder；Debug/Release各10项目`0 warnings / 0 errors`、BattleHost inbound-disorder checks、`SequentialSingleWriter` no-network local run、final Ledger `12 / 70`和scoped audit均通过。下一步仅可只读审计下一项Server-only包，先建新的Change Record再写源码；本包不能触发deadline/lock或选择`MissingInputPolicy`，不实现Client、packet/serialization/transport/retransmit/Jitter/weak-network runtime、snapshot/recovery、battle rules、数据库或公网，更不是S2 `VERIFIED`。
- **最近完成的 Server 包是 `S2-SERVER-AUTHORITY-FRAME-GAP-001 / FOCUSED_TEST_PASS / SERVER_GAP_RESPONDER_READY / CLIENT-PAUSED / S2-PREIMPLEMENTATION`**：missing authority-frame request、从现有assembler locked history取得有界顺序切片的Server responder与聚焦fixtures已写入；Debug/Release各10项目`0 warnings / 0 errors`、Protocol gap-request与BattleHost gap-responder checks、`SequentialSingleWriter` no-network local run、final Ledger `11 / 68`和scoped audit均通过；它也不是S2 `VERIFIED`。
- **最近完成的 Server 包是 `S2-SERVER-INPUT-REDUNDANCY-001 / FOCUSED_TEST_PASS / SERVER_INPUT_REDUNDANCY_READY / CLIENT-PAUSED / S2-PREIMPLEMENTATION`**：current+unconfirmed完整`InputSubmission` window、ordered ingress与聚焦测试已写入；首次`netstandard2.1` guard兼容性错误已受限修复，Debug/Release各10项目`0 warnings / 0 errors`、Protocol redundancy-window与BattleHost redundancy-ingress checks、`SequentialSingleWriter` no-network local run、final Ledger `10 / 64`和scoped audit均通过；它也不是S2 `VERIFIED`。
- **当前 Server-only 范围与下一步**：不实现真实 Socket、数据库、正式 BattleKernel、Gateway、Matchmaker 或公网部署；也不应继续堆叠 TestKernel 来冒充 formal S0。最近完成源码包为`S2-SERVER-ACK-READY-GAP-TICK-RANGE-001 / FOCUSED_TEST_PASS / SERVER_ACK_READY_GAP_TICK_RANGE_READY / CLIENT-PAUSED / S2-CORRECTION`：`FrameAck`、gap request和non-empty ready range不再接受terminal `long.MaxValue`作为authority-frame fact，`ServerProgress`也只接受精确successor；terminal empty ready-range仍合法。Debug/Release `0/0`、focused/full Server tests、no-network `SequentialSingleWriter` run、declared-source audit与final Ledger`21 / 70`均通过。它不选择Client、transport、ACK/ready/gap retention、payload、policy、Kernel、snapshot或S2验证状态。当前没有活跃Server源码包；下一步仅可做前置审计或先建立新的独立Server Change Record。`CLIENT_INTEGRATION_REQUIRED` 仍是 formal S0 关闭门槛，但不是恢复任何 Client 动作的授权，也不是 S1/S0 `VERIFIED`。
- **Server 侧没有隐藏的 S0 编码余量**：`NTSD.Battle.Kernel.Abstractions` 明确标记 formal battle kernel 尚未实现；新增 `IBattleKernel`、snapshot/restore 或共享 runtime adapter 是设计中的 S5 shared-Kernel/独立进程工作，不能用它替代当前 S0 Client 的十域 witness 缺口。
- **已观察的当前环境事实**：`I:\GitHub\Unity_GAS\NTSD_Server` 已有独立 Git 仓库、`NTSD.Server.sln`、.NET 10 工程与 Server 自己的 Ledger/State/Handoff/Change Record；`dotnet --version` 在 `global.json` 下解析为 `10.0.400`。旧任务关于 sibling root 未热重载和 .NET 10 缺失的内容均是已解除的历史环境事实。
- **当前没有 S0 bootstrap 的硬环境 blocker**。`CLIENT_INTEGRATION_REQUIRED` 已获 validation-only 批准：fresh assembly/Editor.log compile evidence、self-check PASS、S0 focused NUnit 5/5 PASS 与 existing lockstep fixture 9/9 PASS 已取得；用户现已明确授权 `S0-WITNESS-001` 的最小 Client runtime/test 实现范围。C++ full-trace 观察链路仍是独立未解边界。
- **Unity 内的 S0 多 world 代码已取得 focused 验证，但仍未完成验收**：`S0-INPROC-AUTHORITY-001` 为 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`。现有 Editor 于17:07:33完成 self-check `PASS`；用户 Test Runner 截图显示 S0 focused NUnit 5/5 pass / 0 fail，`BattleLockstepSessionEditorTests` 可见九项全绿。真实实体的 in-process multi-world journal 与十域 witness 仍待，且不得修改代码或把它标为 `VERIFIED`；跨进程/跨 runtime 一致性是后续 S5，而不是 S0 gate。现有 `BattleLockstepChecksumSnapshot` 可在 mismatch 后提供九个命名 hash 加 overall 的 structured diagnostic，但其分配型 capture 不能置入每 tick hot path，typed slot/generation witness 仍缺。
- **战斗规则唯一 authority 不变**：`J:\QQFile\NTSD2.4\ntsd_release` 中实际进入 `ntsd_new.exe` release build 的 C++ live path。Unity/C#、历史 C# release、旧 self-check、性能报告和旧 Play Mode 都只能用于实现、回归或定位，不能裁决规则。
- **C++→Unity 重新对齐主计划没有取消，只是当前被服务器优先顺序覆盖**。R1 静态 source inventory 已完成；`R1-WP02` 的只读自动 full trace 仍 `BLOCKED`。`D-SCHED-009 + D-RENDER-002` 已取得 Unity joint S4 证据，但仍缺 C++ full trace，且 R07B、R07C、R08 未开始。
- **HFR 不是当前实施主线**：`high-frame-rate-presentation-plan.md` 仍为 `PLANNED`，HFR-00～HFR-09 都未开始。战斗逻辑继续固定 **30 Hz**；60/120 Hz 仅能是 presentation sampling/interpolation，绝不能改变 DAT、tick、输入、碰撞、AI、opoint、RNG 或逻辑真值。
- **Web cadence 实验是独立诊断，不是 Unity HFR 或 C++ release parity 证书**：`WEB-CADENCE-001` 已 build、focused test、Native HTTP 生命周期验证，但仍 `RUNTIME_PENDING`，因为 Canvas 人工三栏视觉验收未完成。

- **S5 kernel / room exception boundary（2026-08-25，只读、非实现包）：** Server generic `IInMemoryAuthorityKernel.Advance(...)` 与 caller-owned missing-input policy没有异常、原子性或回滚合同；`InMemoryAuthorityRoom`会先append journal再推进kernel，session/adapter也不catch。因而一次throw可能留下locked/journaled frame而没有formal completed tick，不能安全以catch/retry/journal removal/generic rollback/fault logging“修复”。详见 Server [`S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md)。必须先在formal Kernel/S5 Host范围定义atomic commit、fault witness、room/process isolation与snapshot/recovery，当前不改变任何Client或Server源码。

- **S1 input-payload immutability boundary（2026-08-25，只读、非实现包）：** generic Server 的`ReadOnlyCollection`/record只保证collection structure；`InputSubmission<TInput>`、slot input、pending/locked/journal/ready owners与missing-policy结果都会直接保留opaque `TInput`，源码已明确将value/deep-copy semantics留给future formal input-contract owner。因此不能把“不可变frame”误写为payload deep immutability，也不能以reflection clone/default identity copy/JSON序列化自行解决。详见 Server [`S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md)。必须先在formal Kernel/S1输入范围定义canonical value、capture boundary、equality/hash/serialization、missing-policy关系与mutable-alias regression；本审计不修改任何Client或Server源码。

- **S1 formal FrameInputSet shape boundary（2026-08-25，只读、非实现包）：** `ntsd_new.exe` release `Makefile`纳入`input_handler.cpp`与`game_tick.cpp`；live input basis是right/left/up/down/attack/jump/defend七个logical action，poll会从held state派生prev/rising edge/history/cooldown，AI则由world/input_phase/RNG在kernel内写入同一domain。SDL/Unity key binding、`InputHandler::snapshot()`、prev/history/cooldown/AI与post-`apply_input` state都不是raw Client intent。详见 Server [`S1-FORMAL-FRAME-INPUT-SHAPE-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S1-FORMAL-FRAME-INPUT-SHAPE-PREREQUISITE-001.md)。formal action value、capture/edge derivation、human/AI slot ownership、tick mapping与real-world replay仍待正式scope；本审计不修改任何Client或Server源码。
- **Formal AI/state-hash Client gate（2026-08-29，已授权）：** 用户已授权必要Client源码并恢复S0～S9；当前先执行`S0-WITNESS-001` fresh validation。future `CLIENT-FORMAL-BATTLE-KERNEL-SHARED-OWNER-001`为`USER_AUTHORIZED / PACKAGE_NOT_STARTED`，仍须独立Task/Change；不能把AI放进Client submission、generic missing policy或复制Server实现。Scene/资源/Input Actions不在当前包。

- **S5 single-writer room actor boundary（2026-08-25，只读、非实现包）：** 当前Server的`SequentialSingleWriter`只是`SequentialRoomExecutionBoundary`/`LocalBootstrapHost`输出的bootstrap metadata；没有运行中的room actor、mailbox、queue、scheduler或并发顺序证明，generic in-memory owners也未声明thread-safe。详见 Server [`S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md)。不能以临时`lock`/queue决定input/deadline/advance/fault顺序；formal S5 Host必须先定义operation order、backpressure、lifecycle、commit与fault isolation。此审计不改Client或Server源码。

- **C1 后的 S3 recovery 门槛（2026-08-25，只读）：** Server [`S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md`](../../../../NTSD_Server/docs/ai/AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md) 已按完成的 C1 更新：activation journal 与每帧 resolved policy witness 现仅为进程内事实，没有 retained-base/epoch、persistence/serializer、snapshot/restore 或 reconnect/recovery disposition。未来恢复合同必须绑定 initial policy、连续 activation prefix、retained envelope range、snapshot tick/checksum 与 target replay tick，并明确 receiver activation-ack 在 restore/reconnect 后是恢复、失效还是重新确认。它不是新源码授权；不得据此修改Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input。

- **缺失输入产品决策门槛（2026-08-25，只读）：** Server [`PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md`](../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md) 已将 S-NET-001/002 收敛为用户必须确认的模式、deadline/grace/max-missing、transient canonical input、persistent neutral/AI/disconnect、policy refusal/fault 与 reconnect/recovery 语义。现有 deadline/provenance 机制不等于选择任何 payload 或产品规则；不得修改Client、wire、transport、snapshot/recovery、rebarrier、InputDelay或missing-input源码。

- **持久执行工作流（2026-08-25）：** 后续 Codex 不得依赖本聊天或旧 session 选择工作；必须先读 Server [`S0-S9-EXECUTION-WORKFLOW.md`](../../../../NTSD_Server/docs/ai/S0-S9-EXECUTION-WORKFLOW.md) 与 [`S0-S9-NEXT-PACKAGE-QUEUE.md`](../../../../NTSD_Server/docs/ai/S0-S9-NEXT-PACKAGE-QUEUE.md)，从最早 READY row 执行。局部 GATED/DEFERRED 不等于总目标暂停；只有 queue 没有 READY/ACTIVE row 时才报告准确的外部 gate，不能反复索要泛化范围确认。

- **持久工作流验证（2026-08-25）：** Server GOVERNANCE-S0-S9-EXECUTION-WORKFLOW-001 已在其治理范围关闭；[`Validate-S0S9ExecutionWorkflow.ps1`](../../../../NTSD_Server/scripts/Validate-S0S9ExecutionWorkflow.ps1) 会校验 workflow/queue 锚点、queue 状态、最多一个 ACTIVE row 和 no-READY 声明。任何后续选包或交接更新后必须运行它，并同时运行 Change Ledger validator；它不验证 battle correctness，也不授权任何 Client 动作。

## 2. 当前主线任务

当前不是泛泛地“做联机”或“重写 Unity 战斗”，而是按用户确定的顺序推进：

```text
独立 Server bootstrap + generic authority-session TestKernel（Server-only 已完成）
    ↓
CLIENT_INTEGRATION_REQUIRED（validation-only 已批准；focused NUnit 5/5 与 existing lockstep 9/9 已通过）
    ↓
S0-WITNESS-001（用户授权 / CODE_WRITTEN；仅 checksum witness、first-difference、test-only real-entity multi-world；等待 Unity 编译）
    ↓
SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING
    ↓
只有用户重新批准后，恢复 Unity adapter、S0 同进程多 world 十域 checksum 验收；跨进程/跨 runtime 验收留待 S5
```

物理边界：

```text
I:\GitHub\Unity_GAS\
├─ gameplay-ability-system-for-unity\   # Unity Client；S0-WITNESS具名范围已激活
└─ NTSD_Server\                         # 独立 Server 根；Git/.NET 10 solution 已建立
```

S0～S9 的含义不可混淆：S0～S5 先建立“能正确运行一局权威 battle session”的核心；S6～S7 才接真实 transport/公网弱网；S8～S9 才涉及 Gateway、Auth、Matchmaker、Room Allocator、多房间、容量与多地域。当前不提前实现后半段。

## 3. 权威文档

后续执行前，按下表顺序读取与当前操作相关的文档。更具体路径的规则优先于泛化说明；任何与根 `AGENTS.md` 冲突的历史文字都以根规则为准。

| 优先级 | 文档 | 用途与阅读规则 |
|---:|---|---|
| 1 | `AGENTS.md` | 全项目安全、C++ authority、30 Hz、验证、Git、Change Record 和 Client 冻结边界。任何战斗规则先追 C++ release live path。 |
| 2 | `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md`（本文） | 当前主线、近三天用户决定、当前环境复核与可执行续接顺序；不替代行为 authority。 |
| 3 | `Assets/NTSD/Docs/server-lockstep-s0-s9-progress.md` | S0～S9 当前进度、Resume Card、开放决策、问题台账。其“旧任务无法写 sibling root”的环境描述为历史记录；目录/权限以当前任务实测为准。 |
| 4 | `Assets/NTSD/Docs/server-lockstep-s0-s9-design.md` | S0～S9 设计、输入/传输分层、single-slow-client 合同、修复流程与关闭标准。详细设计以它为准。 |
| 5 | `docs/ai/STATE.md` | 全项目长期状态和活跃 Change ID；阅读其日期与覆盖语句。里面旧沙箱路径描述同样不能覆盖当前会话的实际 writable roots。 |
| 6 | `I:\GitHub\Unity_GAS\NTSD_Server\docs\ai\CURRENT-HANDOFF.md`、`STATE.md`、`CHANGE-LEDGER.md` 与最新 `TASKS/CHANGE-RECORDS/S2-SERVER-READY-BUFFER-HORIZON-001.md` | 最近 Server-only 证据、formal Client gate、精确范围、命令与回滚合同。先读它们，不能凭旧 bootstrap 指令重做工程。 |
| 7 | `docs/ai/CHANGE-RECORDS/S0-INPROC-AUTHORITY-001.md` 与 `docs/ai/CHANGE-LEDGER.md` | 冻结的 Unity S0 代码范围和治理总账；只读确认，不得在没有新批准时借此恢复 Unity 验证。 |
| 8 | `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` | C++→Unity R1～R8 的当前执行顺序、证据分级与 R1-WP02 full-trace blocker。 |
| 9 | `Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md` | 战斗对齐、性能、CentralOnly、容量和已做工作包的细节交接；其中的历史最终措辞受根 `AGENTS.md` 的 C++ authority override 约束。 |
| 10 | `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md` 与 `Assets/NTSD/Docs/BATTLE_RUNTIME_VERIFICATION.md` | 历史差异/测试索引和自检证据。可复用具体 regression，但不能仅凭这些文件宣称 C++ release 已对齐。 |
| 11 | `Assets/NTSD/Docs/high-frame-rate-presentation-plan.md` | HFR 唯一实施计划。当前仅方案；除非用户明确恢复 HFR，不能从此文跳入 Shader/Mesh 修改。 |
| 12 | `Assets/NTSD/Docs/unified-battle-lockstep-ecs-server-architecture-plan.md` | 架构总览与阶段依赖；S0～S9 的日常设计/进度分别以下表第 3、4 项为准。`future-server-lockstep-architecture.md` 仅作历史背景。 |

## 4. 最近 3 天有效上下文

### 4.1 读取边界

本次只读取了旧任务的 2026-08-23 至 2026-08-24 记录。其下一页直接回到 2026-08-20，因此未把 8 月 20 日及更早的完整讨论搬入本文件；只在这些近三天记录引用到的权威文档中提取了必要定调。

### 4.2 用户已明确决定

1. **服务端优先、冻结 Client**（2026-08-23/24）

   - 暂不继续修改 Unity Client；用户现已仅批准既有 S0 的读/编译/focused test/`BattleRuntimeSelfCheck`，不批准 Client 源码、Scene、资源或配置改动。
   - 已经写入的 Unity S0 多 world 代码保留，不回滚、不删除；当前状态为 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`。
   - `CLIENT_INTEGRATION_REQUIRED` 已记录并获得 validation-only 批准；若后续需要修改 Client，先建立独立 Change Record，再请求/记录相应实现范围。

2. **独立服务端根目录**

   - 服务端固定为 `I:\GitHub\Unity_GAS\NTSD_Server`，作为 Unity Client 的兄弟目录，而非 `Assets/`、`Tools/` 或 Client 根的临时目录。
   - 服务端应拥有独立 Git、solution、SDK、依赖、配置、测试、部署与自己的治理记录；不得为了绕开目录/权限问题把 server 代码暂写到 Unity/Tools/Temp 再迁移。

3. **技术路线和阶段划分**

   - ServerHost/Gateway 采用 **C# + .NET**；共享的未来 `Protocol` / `Kernel` 边界必须保持 Unity 可消费的 `netstandard2.1` 约束，独立 Server Host 基线为 **.NET 10 LTS**。
   - C++ release live runtime 继续定义 battle rules；不另外复制一套 C++ server battle logic，也不让 .NET ServerHost 重写伤害、技能、碰撞、RNG、对象生命周期或 pass 顺序。
   - S1～S3 先冻结应用层协议语义；S6 才评估 UDP/KCP/ENet/LiteNetLib 等实际 transport。不得提前把某一个 transport 库耦合进 BattleKernel。
   - 控制面将来是 HTTPS/TLS；战斗数据面是低延迟、应用层有 sequence/ACK/redundancy/deadline/jitter 语义的通道。正常客户端只提交离散输入，不上传 Transform、HP、命中、伤害、武器或技能结果作为权威状态。

4. **单慢客户端的硬定调**

   - 不采用“每帧无限等待所有玩家”的 pure wait lockstep。
   - 应采用输入延迟、deadline、不可改写的 authority frame、缺失输入原因、ACK、冗余、Jitter Buffer、恢复与长期缺失状态机。
   - deadline 后的迟到输入不能改历史；短缺包不能伪造 pressed/released/J/K/L/组合边沿；长期缺失的降级只影响该玩家，健康玩家持续跟随服务器。
   - PvP 长期缺失后的 neutral/托管/结局仍是 `S-NET-001 / PENDING_PRODUCT_RULE`，不得凭经验擅自写死。

5. **公网和多地域的边界**

   - 用户确认未来可以使用两个候选公网 IP：`129.204.124.151`、`124.71.139.127`；它们仅是 S6/S7 的获授权测试候选，尚未对其扫描、登录、部署或改安全组。
   - 进入 S6 前仍须由资源所有者确认资源类型、region、OS、CPU/内存/带宽、SSH/RDP/控制台访问、可开的 TCP/UDP 端口、外部测试授权与长期使用条件。
   - 一局 battle 永远只运行在一台权威 Battle Server 上；多地域只能把不同房间分配到不同节点，不能把同一 BattleWorld 拆到两个地区一起推进。

### 4.3 当前代码与验证状态

| 主题 | 已观察事实 / 最新记录 | 不能据此声称 |
|---|---|---|
| Unity S0 多 world | 既有五个 S0 文件已读；fresh script assemblies 晚于其 source，Editor.log 无匹配 C# compile error，`BattleRuntimeSelfCheck` 于17:07:33 PASS，用户 Test Runner 截图为 S0 focused NUnit 5/5 pass / 0 fail，以及 existing lockstep fixture 9/9 pass；Record 为 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`。现有 S0 fixture 是 48 tick logic-only / aggregate-hash evidence。 | 真实实体的 same-process server+two-client same-Kernel journal、十域 first-difference witness 或 S0 `VERIFIED`。跨进程/跨 runtime 一致性属于 S5。 |
| S0 syntax unblock | Record 追加说明：两处 switch 解析括号修正后，force-all 脚本刷新曾得到 Editor DLL 更新与 Console `error=0`。 | S0 自身所有 acceptance 或 runtime 测试已通过。 |
| Server bootstrap | `S0-SERVER-BOOTSTRAP-001` 已在独立 Server Git 仓库达到 `FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`：bootstrap 两次、Debug/Release build、四项自托管测试、架构边界、Ledger validator 和 no-network local run 均通过。 | formal BattleKernel、authority frames、transport、数据库、Unity 集成、跨端 checksum，或 S0 `VERIFIED`。 |
| Server authority-session | `S0-SERVER-INMEMORY-AUTHORITY-001` 已达到 `FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`：generic frame/barrier/session、96 帧 TestKernel journal、Debug/Release build、四项 tests、no-network run、Ledger/static audit 均通过。 | formal NTSD BattleKernel、Unity multi-world、十域 checksum、S0 `VERIFIED` 或 S1。 |
| Server initial tick origin | `S1-SERVER-INITIAL-AUTHORITY-TICK-001` 已达到 `FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED`：StartBarrier/session/journal 与 protocol contract 可在同一个合法 non-zero authority tick 起步，mismatch 在 kernel step 前 fail closed；Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `14 / 70` 和 scoped audit已通过。 | formal snapshot/recovery、formal Kernel、Client integration、C++ battle alignment、real transport，或 S0/S1 `VERIFIED`。 |
| Server missing-input provenance | `S1-SERVER-MISSING-INPUT-PROVENANCE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED`：source/fill-reason 仅允许六种一致、已知 pair；immutable envelope 与 generic policy resolution 均 fail closed。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `15 / 70` 与 fixed-string audit已通过。 | 任何 missing-input payload、grace、neutral/carry、AI、disconnect/reconnect产品行为、formal Kernel、Client integration，或 S0/S1 `VERIFIED`。 |
| Server redundancy ingress capacity | `S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001` 已达到 `FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED`：ingress-own actual-entry cap拒绝oversize window且zero mutation，at-cap window保留原有顺序/outcomes。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `16 / 70` 与 fixed-string audit已通过。 | production count、raw packet/MTU/bandwidth cap、Client resend、transport、deadline/missing-input policy、formal Kernel/recovery，或 S2 `VERIFIED`。 |
| Server future-target admission bound | `S1-SERVER-FUTURE-TARGET-BOUND-001` 已达到 `FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED`：generic assembler/room adapter要求caller-supplied nonnegative bound，zero合法；exact boundary可接受，over-limit target在managed sequence/pending mutation前以稳定disposition拒绝。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `17 / 70` 与 fixed-string audit已通过。 | production `InputDelayFrames`/default、deadline、missing-input policy、raw packet/MTU、Client、transport、formal Kernel/recovery，或 S1/S2 `VERIFIED`。 |
| Server authority-tick numeric range | `S1-SERVER-AUTHORITY-TICK-RANGE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED`：`long.MaxValue`不再是可推进frame tick，只作为final legal tick后的terminal next cursor；terminal fact、assembler lock和direct room call均fail closed。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `18 / 70` 与 expanded fixed-string audit已通过。 | C++ `world.game_tick` mapping、30 Hz/battle语义、Client、transport、missing-input、formal Kernel/recovery，或 S1/S2 `VERIFIED`。 |
| Server client-known confirmed tick range | `S1-SERVER-CONFIRMED-TICK-RANGE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_CONFIRMED_TICK_RANGE_READY / CLIENT-PAUSED`：`InputSubmission.ClientKnownConfirmedAuthorityTick`只接受既有`-1` sentinel或Protocol addressable tick；terminal `long.MaxValue`在DTO构造前fail closed。`-1`、final addressable与terminal regressions，Debug/Release 0 error、focused/full Server chain、no-network run、declared-source audit与Ledger `20 / 70`均通过。 | reported cursor与target/current tick关系、ACK/retransmit/retention、Client、transport、payload、policy、formal Kernel/recovery，或 S1 `VERIFIED`。 |
| Server ACK / ready / gap tick range | `S2-SERVER-ACK-READY-GAP-TICK-RANGE-001` 已达到 `FOCUSED_TEST_PASS / SERVER_ACK_READY_GAP_TICK_RANGE_READY / CLIENT-PAUSED`：ACK/gap/non-empty ready range只能表示addressable frame fact；progress必须是exact successor；terminal empty ready range继续合法。final-addressable/terminal/empty-range/successor regressions、Debug/Release 0 error、focused/full Server chain、no-network run、declared-source audit与Ledger `21 / 70`均通过。 | real Client ACK/ready/gap flow、retransmit/retention/recovery、transport、payload、policy、formal Kernel/recovery，或 S2 `VERIFIED`。 |
| Server ready-buffer future horizon | `S2-SERVER-READY-BUFFER-HORIZON-001` 已达到 `FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED`：far envelope不能先占尽buffer count并排挤near contiguous frame；exact horizon保留正常行为。Debug/Release 0 error、focused/full self-hosted chain、no-network run、Ledger `19 / 70` 与 fixed-string audit已通过。 | production jitter/delay、actual Client buffer、transport/ACK/retransmit、weak-network runtime、history/recovery，或 S2 `VERIFIED`。 |
| Server client-sequence retention | `S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001` 只读审计完成：accepted sequence map跨锁帧保留，现有 ACK/冗余/reported cursor不能作为safe eviction floor。 | sequence lifecycle/rollover/reconnect、idempotency horizon、retirement proof、post-expiry disposition/witness、snapshot/replay、capacity/overload的版本化决定；未决定前不得写eviction或count refusal。 |
| Server authority-history retention | `S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001` 只读审计完成：locked envelope/journal list无限增长，当前gap索引需要完整initial-prefix，单次gap response cap不是history cap。 | formal Kernel、retained range/snapshot base、history-expired/recovery disposition、ACK/sequence relationship、bounded capacity与real Server/Client restore-replay evidence；未决定前不得写generic ring/truncation/recovery。 |
| Server protocol-version evolution | `S1-PROTOCOL-VERSION-EVOLUTION-PREREQUISITE-001` 只读审计完成：version `1`是in-memory marker，当前不存在wire codec/ABI、capability negotiation、unknown-disposition或rolling upgrade模型。 | S5/S6版本语义、compatibility/bump规则、session admission、wire header/codec、upgrade/downgrade、replay/schema supersede与real serialized peer fixtures；未决定前不得bump或加compatibility路径。 |
| C++→Unity 对齐 | R1 static source inventory 完成；R2 pass 包仍 `RUNTIME_PENDING`；R07A 的 `D-SCHED-009 + D-RENDER-002` 获 Unity joint S4 Play/automatic evidence，结论是 `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。 | 整个 battle runtime、R7/R8 或 C++ release full trace 已闭环。 |
| R7 performance | `R7-PERF-001` 的 fresh compile、focused `15/15`、warmed `0 B`、full self-check 已有记录，但仍 `RUNTIME_PENDING`，缺真实 battle Play Mode 和 C++ runtime trace。 | 1000 AI 已稳定达到 30 Hz，或性能/对齐已最终认证。 |
| 当前 Unity self-check | 现有 Editor request 已被消费，`Temp/NTSD_BattleRuntimeSelfCheck.result` 于17:07:33 写入 `PASS`；Editor.log 同时记录“自检完成”。 | self-check 覆盖 S0 focused NUnit、formal multi-world 或 C++/Server alignment。 |
| HFR | HFR-00～HFR-09 均 `NOT_STARTED`；`RenderAlpha` 未接入中央 Mesh/Shaders，中央顶点无 previous-position。 | Unity 已有 60/120 Hz presentation support。 |
| Web cadence | `WEB-CADENCE-001`：build、focused `48/48`、Native `open → 16-tick preview → close`、只读 `403` 均有证据；全量 npm 为 `392 passed / 2 existing unrelated failures / 1 skipped`。 | Canvas 三栏人工视觉已验收，或它证明 Unity/C++ release gameplay parity。 |

> **Server S5 异常边界前置结论（2026-08-25，只读）：** 当前 generic kernel/policy 的throw路径不是已实现的 room fault/recovery 行为；journal 与world的提交原子性、fault witness、隔离与恢复均尚未定义。因此任何局部catch/retry/rollback/日志实现都会越过formal Kernel/S5 Host gate，不能被写成已完成的Server实现或S5进展。

> **Server S5 single-writer 前置结论（2026-08-25，只读）：** `SequentialSingleWriter`目前只在bootstrap health metadata中出现，并不证明线程安全、room actor、队列顺序、backpressure或多room isolation。future S5 Host必须把submit/deadline/lock/advance/ACK/ready/gap/fault等操作置于一个明确的deterministic admission order，并用formal Kernel/Client证据验证；在此之前不得以`lock`/background task/queue伪造完成。

> **Server S1 输入 payload 前置结论（2026-08-25，只读）：** 目前已验证的是frame/window的结构不可变，不是任意引用型`TInput`的deep snapshot。正式输入值、capture时点、canonical equality/hash/serialization与missing-policy payload必须与formal Kernel/Client/C++ release-live input evidence共同定义；在此之前不能增设通用clone或把当前Server-only测试称为不可变payload证据。

> **Server S1 formal input shape 前置结论（2026-08-25，只读）：** C++ release live path已确认七个logical held action及由runtime派生的prev/edge/history/cooldown；AI由world/input_phase/RNG在kernel内生成。未来`FrameInputSet`必须定义player input capture/edge contract与human/AI ownership，不能上传SDL/Unity binding、input history、cooldown、AI或post-input state，也不能将`InputHandler::snapshot()`当作battle snapshot。

### 4.4 性能、HFR 与 battle alignment 的固定结论

- 逻辑 tick 恒为 **30 Hz**。`SimulationTickDriver -> NTSDBattleTickSystem -> SimulationWorld`、`FrameInputSet`、slot/generation、SoA/ECS、pool、CentralOnly、Texture2DArray、动态 Mesh/URP 与 battle-time 0-GC 目标是必须保留的 Unity 边界；不能为了对齐、服务器或平滑显示回退到 Transform/Animator/Legacy `SpriteRenderer` 作为战斗真值。
- 既有性能文档的最近可用结论是：1000 AI 的稳定 30 Hz gate 仍没有被证明关闭；不要把 catch-up 限制、单个 focused `0 B` 或平均 FPS 当作容量验收。后续性能报告必须报告 tick P50/P95/P99、GC、backlog/dropped tick、实体容量和真实 profile。
- HFR 的 v1 只能做“一个逻辑 tick 延迟的 previous/current presentation interpolation”；出生/销毁、slot/generation/lineage 变化、frame/pic/facing、hit/opoint/overlay 等结构性事件应保持离散，异常时 fail closed 回到 current-only。
- `R1-WP02` 自动化 C++ full trace 是额外的定位/比较 blocker，不阻断已经有 C++ source contract 的最小 Unity 工作包；但没有它或同等级的 C++ runtime evidence 时，所有相关结果都必须保留在 `RUNTIME_PENDING`/相应层级。

## 5. 已完成事项

### 5.1 已实现且已有相应验证

- `R8-WP01G-R07A`：`D-SCHED-009` 与 `D-RENDER-002` 的 Unity joint S4 证据已完成到可用证据上限。Record/Task 记载 actual collision/hit → frozen publication → same-tick writeback → central materialization → Late idempotence 与 next-tick RNG/lifecycle 的 joint 证据，且有 fresh compile、focused suites、full self-check、Play probe、Console0、ledger 的记录。**仍缺 C++ full trace；不可扩大成完整 battle/C++ verified。**
- `R7-PERF-001`：已移除 stale PreInteraction cross-pass proof；既有记录为 compile0、focused `15/15`、warmed `0 B` 和 full self-check PASS。当前状态仍为 `RUNTIME_PENDING`，不是完整 runtime 对齐。
- `WEB-CADENCE-001`：独立只读 render-cadence 入口、纯 presentation sampler、只读 server flag、专用 launcher 与 focused/HTTP 生命周期验证已完成；默认 DAT 编辑器、Unity、C++、DAT 和资源未改。
- 服务器设计/治理层：`server-lockstep-s0-s9-design.md`、`server-lockstep-s0-s9-progress.md`、`S0-SERVER-BOOTSTRAP-001` Task Contract 和该冻结 Unity S0 Change Record 已建立。
- `S0-SERVER-BOOTSTRAP-001`：独立 Server Git/.NET 10 solution、模块边界、Server Ledger/State/Handoff/Record、bootstrap/build/test/run-local、架构检查和 no-network local health skeleton 已实际完成并验证；状态为 `FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`。
- `S0-SERVER-INMEMORY-AUTHORITY-001`：generic immutable frame、StartBarrier、authority-first session、replica checksum witness 和 tests 内 TestKernel 已实际完成；Debug/Release 0 error、四项 tests、no-network run、Ledger/static audit 已验证；状态为 `FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`。
- `S1-SERVER-INITIAL-AUTHORITY-TICK-001`：Server generic StartBarrier/session/journal/protocol initial-tick alignment与non-zero/mismatch regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`14 / 70`和scoped audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_INITIAL_AUTHORITY_TICK_READY / CLIENT-PAUSED`。
- `S1-SERVER-MISSING-INPUT-PROVENANCE-001`：Server protocol provenance pair validator、immutable envelope/resolution guard与legal/mismatched/unknown regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`15 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_MISSING_INPUT_PROVENANCE_READY / CLIENT-PAUSED`。它不选择任何 missing-input 产品行为。
- `S2-SERVER-REDUNDANCY-INGRESS-CAPACITY-001`：Server-owned redundancy actual-entry cap、oversized-window no-mutation rejection与at-cap/disorder regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`16 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_REDUNDANCY_INGRESS_CAPACITY_READY / CLIENT-PAUSED`。它不选择production count、wire/MTU或任何missing-input产品行为。
- `S1-SERVER-FUTURE-TARGET-BOUND-001`：Server generic future-target admission bound、adapter propagation、stable over-limit disposition与negative/zero/exact/no-mutation/near-terminal regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`17 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_FUTURE_TARGET_BOUND_READY / CLIENT-PAUSED`。它不选择production delay/default、deadline/missing-input产品行为、Client、transport或任何S1/S2验证状态。
- `S1-SERVER-AUTHORITY-TICK-RANGE-001`：Server generic authority tick range、terminal fact rejection、terminal lock rejection与final session/room/journal/ready-buffer/progress/ACK regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`18 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_AUTHORITY_TICK_RANGE_READY / CLIENT-PAUSED`。它不定义C++ tick mapping、battle/30 Hz、Client、transport或任何S1/S2验证状态。
- `S2-SERVER-READY-BUFFER-HORIZON-001`：Server generic ready-buffer future horizon、stable far-envelope rejection与invalid/zero/exact/no-mutation/near-capacity/moving-window/disorder regressions已实际完成；Debug/Release 0 error、focused/full self-hosted Server tests、no-network run、Ledger`19 / 70`和fixed-string audit已验证；状态为 `FOCUSED_TEST_PASS / SERVER_READY_BUFFER_HORIZON_READY / CLIENT-PAUSED`。它不选择production jitter/delay、不实现Client、transport、weak-network或任何S2验证状态。
- `S0-INPROC-AUTHORITY-001` validation-only：fresh Unity assembly/Editor.log compile evidence 与 `BattleRuntimeSelfCheck=PASS` 已实际获得；没有修改 Client 源码、场景、资源或配置。

### 5.2 已实现但未运行时验收 / 明确冻结

- `S0-INPROC-AUTHORITY-001`：Unity 同进程 server + 两个 client world 的骨架及五项 Editor tests 已写；self-check 与 S0-focused NUnit 5/5 已通过，但既有 lockstep tests、真实多 world journal 和 C++/runtime 证据仍缺。当前只允许验证，禁止 Client 代码修改。
- 所有要从当前 Unity battle alignment 延伸到真实 C++ release trace、真实 Play Mode、真实 DAT/scene 的包，除有明确 task evidence 外，仍应按各自 Change Record 的 `RUNTIME_PENDING` 处理。
- `WEB-CADENCE-001` 的 Canvas 人工视觉三栏验收仍待；全量 npm 的两项历史 `main.ts` 静态正则失败没有被本包修复。

### 5.3 仅分析/设计完成

- S0～S9 的分阶段设计、协议职责、slow-client 降级原则、恢复/快照/ACK/Jitter 方向、服务端模块边界与 S5/S8/S9 的责任划分。
- HFR 的 HFR-00～HFR-09 计划和 HFR Off/On 不改逻辑的验收矩阵。
- 多地域、Gateway、Matchmaker、Room Allocator、容量调度的后期架构说明；未写对应生产代码。

### 5.4 已废弃、不得继续或不得误用

- 不再使用“C# release / Unity self-check 是最终 battle authority”的旧口径；唯一裁决是 C++ release live path。
- 不采用“一个慢客户端无限拖住整局”的网络模型。
- 不在 S0～S5 之前绑定真实 transport、写 Socket/数据库/公网 listener，或把 TestKernel 称为正式 NTSD BattleKernel。
- 不把 Web cadence 诊断、HFR 计划、历史 self-check、性能 0-B 结论当成 C++ full-trace/真实 Play Mode 战斗认证。
- T8 默认 `stage.dat` 资产部署继续按用户决定暂缓；不要为测试变绿私自生成或加入默认资产。

## 6. 当前阻塞

> **2026-08-29 优先更新：** held-only 与 missing-input carry 已解除，不再是阻塞。当前 Server 硬 gate 是 formal Kernel AI ownership/state hash、实测 numeric short-grace/deadline/delay，以及后续 S3 snapshot/history recovery；下表中更早的 Client/trace/Web 项仅保留其各自历史范围。

| 优先级 | 阻塞 | 已观察原因 | 影响范围 | 清除后的下一步 |
|---:|---|---|---|---|
| 已解除 | Server bootstrap 环境前置 | 当前 Server 根的 `global.json` 解析 `.NET SDK 10.0.400`；独立 Git/workspace 已存在。 | `S0-SERVER-BOOTSTRAP-001` 已不再被 SDK/目录/sandbox 阻塞。 | 后续 Server 扩展另建 Change Record；不自动扩大为 S0 battle verification。 |
| 已解除 | Existing lockstep regression runner | 用户在现有 Editor 的 EditMode Test Runner 实际运行 `BattleLockstepSessionEditorTests`；筛选出的九项均通过。 | S0 focused NUnit 5/5 与 existing lockstep 9/9 已具会话证据。 | 下一步不再是运行该 fixture，而是取得 Client 源码修改授权后，为十域 witness 建立独立 Change Record。 |
| P1 | C++ release 自动 full trace 观察通道未解决 | `R1-WP02` 保持 `BLOCKED`；没有已确认的只读、可重复、覆盖 full schema 的观察方式。 | 不能取得 C++ full-trace/comparator 证书；不阻断已闭合的 C++ source contract 与最小 Unity work package。 | 仅在获得已有的无 authority 写入观察方式后再继续；严禁 instrumentation、hook、patch、注入、重建或新增 trace sink。 |
| 已部分解除 | 当前 Unity fresh verification | fresh assemblies/Editor.log compile scan、`BattleRuntimeSelfCheck=PASS`、用户提供的 S0 focused NUnit 5/5 与 existing lockstep 9/9 截图已取得。 | 仍不能证明 formal multi-world witness、真实实体 runtime 或 C++/Server alignment。 | 取得 Client 源码修改授权后建立 witness 独立 Change Record；不直接修改现有 S0 文件。 |
| P2 | 工作树高度脏且治理文档本身未提交 | `git status` 显示大量脚本、资源、场景、工具和 Docs 修改/未跟踪项；`docs/ai/` 也处于未跟踪状态。 | broad build、提交、回滚和大范围 diff 很容易误触用户工作。 | 每次只按 Task/Record scoped diff；不 `reset`/`clean`/`restore`，不提交未审查文件。 |
| P3 | Web cadence 最后视觉验收 | 自动/HTTP 证据齐全，但当前没有浏览器 Canvas 人工观察证据。 | 仅影响 `WEB-CADENCE-001` 的最终 runtime 级别，不影响 Server bootstrap。 | 用户或后续任务在实际浏览器选择有位移的技能，观察 30/60/120 三栏并记录结果。 |

## 7. 下一步执行顺序

> **2026-08-29 优先更新：** 用户已授权必要Client源码并恢复S0～S9。当前最早包为Server `S0-FORMAL-MULTIWORLD-WITNESS-VALIDATION-001` / Client `S0-WITNESS-001`；先用现有Editor取得fresh compile、5/5、9/9和self-check。不要重做bootstrap、不要询问held-only、不要扩张到Scene/资源/Input Actions/transport/recovery。

> **2026-08-25 执行更新：** 下方 bootstrap 步骤是历史完成记录，**不得重做**。Client 冻结仍然有效，但不会暂停 Server-first 总目标；S1/S2 的 Server-only packages以及最近`S2-SERVER-ACK-READY-GAP-TICK-RANGE-001`已各自在独立 .NET 范围通过。当前没有活跃 Server source package：先阅读 `NTSD_Server/docs/ai/CURRENT-HANDOFF.md`、`STATE.md`、Ledger、最新`TASKS/CHANGE-RECORDS/S2-SERVER-ACK-READY-GAP-TICK-RANGE-001.md`以及`AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md`、`AUDITS/S1-CLIENT-SEQUENCE-RETENTION-PREREQUISITE-001.md`、`AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md`、`AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`，随后只做前置审计或创建有明确缺口的新 Server-only Change Record。不得修改任何 Client 源码，也不得用 generic/TestKernel绕过formal Kernel、snapshot/recovery、transport或产品规则门槛。

> **补充读取门槛：** 在下一项Server源码包前，还必须阅读 `NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`；除非已获得formal Kernel/S5 Host的atomic commit、fault witness、isolation与recovery合同，不得以局部异常处理绕过它。

> **补充读取门槛：** 在下一项触及`TInput`、submission、locked envelope、missing-policy payload、journal/replay或ready-buffer value semantics的Server源码包前，必须先阅读 `NTSD_Server/docs/ai/AUDITS/S1-SERVER-INPUT-PAYLOAD-IMMUTABILITY-PREREQUISITE-001.md`；未获得formal input-contract范围时不得添加通用deep clone、reflection copy或ad-hoc serializer。

每次新包仍须更新对应 Task Contract、Change Record、Ledger、`docs/ai/STATE.md` 和 server progress；不要只在聊天中宣布状态。

1. **补齐 .NET 10 SDK 前置条件（用户动作）。**

   - 不自动安装 SDK、不改 PATH 或 profile。
   - 验收：在任意 shell 中 `dotnet --list-sdks` 出现 `10.0.*`；随后在 Server 根的 future `global.json` 约束下 `dotnet --version` 解析为受支持的 10.0 SDK。

2. **恢复 `S0-SERVER-BOOTSTRAP-001`，但先做只读/治理 preflight。**

   - 重新阅读本文、第 3 节文档、Task Contract、Server 根目录及 `git status`；确认 `NTSD_Server` 不含用户文件或先吸收其已有内容。
   - 在写入第一个服务器脚本之前，**在 `NTSD_Server/docs/ai/CHANGE-RECORDS/` 创建真正的 `S0-SERVER-BOOTSTRAP-001` Change Record**，并建立该 Server 仓库自己的 Ledger/State；不要在 Unity Ledger 伪造一个外部路径 Record。
   - 验收：Change Record 明确覆盖首包文件、authority/需求、模块边界、验证与回滚；Unity Client scoped diff 没有被此包新增修改。

3. **仅实现 Server 工程骨架。**

   - 创建独立 `.git`、`global.json`、`Directory.Build.props`、`Directory.Packages.props`、`NTSD.Server.sln`、`README.md`、`AGENTS.md`、`src/`、`tests/`、`scripts/`、`config/`、`deploy/` 与 `docs/ai/`，目录严格遵守 S0 Task Contract。
   - `Protocol` / `Kernel.Abstractions` 无 Unity、Host、DB、transport 依赖；`BattleHost` 只拥有 room 的顺序执行边界；不创建无 owner 的 `Common` 项目；TestKernel 必须显式测试专用。
   - 验收：`scripts/bootstrap.ps1` 可重复运行且 fail-fast；architecture tests 能拒绝禁止的项目引用；没有 Socket、DB、真实 battle rule 或 Unity Client diff。

4. **完成纯 .NET build/test/run 链。**

   - 运行 Task Contract 指定的 `scripts/build.ps1 -Configuration Release`、`scripts/test.ps1 -Configuration Release` 和最小 `run-local`/health 验证；执行 Server 侧 Ledger validator 或等价检查。
   - 验收：Release build/test 为成功退出；无 `bin/obj/TestResults/logs/secrets` 被纳入 Git；状态最多可到 `SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`，不是 S0 `VERIFIED`。

5. **CLIENT_INTEGRATION_REQUIRED 已建立，等待恢复 Client 工作的批准。**

   - Server progress 与 Server Handoff 已列出 Client files、formal Kernel 共享边界、纯 Server TestKernel 的不足、预期 checksum/fixture、风险与回滚。
   - 验收：得到用户新的明确批准后，才继续冻结的 `S0-INPROC-AUTHORITY-001` focused test、`BattleRuntimeSelfCheck`、同 journal 的 server + two-client world、十域 witness 和真实运行时验证；跨进程/跨 runtime checksum 另按 S5 处理。

6. **不要并行自动恢复非服务器主线。**

   - battle alignment：当前仅保留各 Change Record 的 `RUNTIME_PENDING` 事实；在用户恢复该主线后，从指定的 R8/repair Task、C++ source contract 与最小 scoped validation 继续。
   - HFR：用户明确批准后才新建 HFR-00 Change Record，从 baseline/feature gate 开始，不能直接改 shader/mesh。
   - Web cadence：只有要关闭其 `RUNTIME_PENDING` 时再做浏览器 Canvas 人工视觉验收；不能替代 Unity HFR。

## 8. 技术定调与禁止事项

### 8.1 Battle authority、tick 与 Unity 实现边界

- 规则只由 C++ release live path 定义：从 `J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp` 的 live `game_tick(...)` 及正式 build 参与的 frame/physics/collision/hit/weapon/cpoint/input/renderer 调用链追踪。C# release 只作历史移植辅助。
- 保持固定 **30 Hz**。Unity `Update`/`FixedUpdate`/`LateUpdate` 不定义 battle rule；tick 内不能用 `Time.deltaTime`/`Time.fixedDeltaTime` 决定战斗结果。
- Transform、Animator、Camera、SpriteRenderer、Mesh、URP 只读逻辑/表现快照，绝不反写 position、velocity、frame、HP/PP、link/holder/target、input、RNG 或碰撞真值。
- 每个 gameplay/Server adapter 改动必须拥有闭合 C++ authority、Unity mapping、focused check、必要 Play Mode 和与风险相称的 C++/server checksum evidence；不能把静态阅读、编译0、self-check、单一测试或历史 Pass 外推为“已对齐”。

### 8.2 Server/lockstep 定调

- 同一 `FrameInputSet` 应是单机、回放、Client 和 Server 的共同逻辑入口；Server 只组装/锁定 authority frames，不能另写 gameplay。
- 正常网络只同步 input/ack/checksum/recovery data；不以 Client Transform 或状态包为真相，也不以客户端本地 snapshot 作为 authority restore。
- 同一 BattleWorld 一个顺序、单写者 tick owner；多个房间才可按房间并行。不得为了性能把同一局 battle passes 随意并发。
- 不让慢客户端无限阻塞健康客户端；但具体 input delay、deadline、grace 和 PvP 长缺失产品规则仍待 `S-NET-*` 证据/用户决定。
- 在 S6 前不得选择/耦合实际 transport；在 S8 前不得实现 Gateway、Auth、Matchmaker、Room Allocator、多地域调度、数据库或消息队列；无授权不得探测或操作公网 IP。

### 8.3 HFR、表现与性能定调

- HFR 只影响 presentation sampling；HFR Off/On 的 logic checksum、RNG、slot/generation、frame、HP/PP、事件、command identity/order 必须一致。
- 不能以提高显示帧率来改 30 Hz DAT wait、碰撞、输入窗口、AI、hit、opoint、随机数或对象生灭时序。
- CentralOnly、Texture2DArray、中央 Mesh、动态 quad、URP、slot/generation/pool、MobileExtended 1000 active 与 DesktopExtended 无固定产品 active cap 不能被对齐/性能修复回退。
- 性能验收必须看预热后的 0 B hot path、P50/P95/P99、GC、backlog、entities、draw、Mesh build，而不是仅平均 FPS 或一次 catch-up 行为。

### 8.4 资源、验证、Git 与安全

- `T8` 默认 `stage.dat` 资产部署继续暂缓。不要为测试加入/生成默认资产；需要 stage 的测试显式使用 fixture 或报告前置条件。
- 未经用户批准不修改 `Assets/NTSD/Scripts/Gen/`、`Assets/Plugins/`、C++ authority、Git hooks/config 或公网环境；不 push。
- 不 `git reset --hard`、`git restore`、`git clean`、删除/覆盖用户文件，也不通过换目录/Temp/Tools 绕开 Server 根和 Client 冻结边界。
- 本次当前线程先完成交接迁移，随后完成独立 Server bootstrap；实际运行了 Server bootstrap/build/test/run-local 与 Server Ledger validation。全程没有运行 Unity、EditMode、Play Mode、SelfCheck、C++ trace、浏览器视觉验收或公网操作，也没有修改 Unity Client 代码。

## 9. 关键文件与入口

| 路径 / 命令 | 作用 | 当前使用注意事项 |
|---|---|---|
| `J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp` → `game_tick(...)` | Battle C++ release live authority 入口。 | 继续追 frame advance、physics、collision_collect/collision/hit、weapon/cpoint、input、renderer；确认 Makefile/release participation。 |
| `Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs` | Unity 30 Hz 逻辑帧外层入口。 | Client 当前冻结；不得让渲染帧反写 tick。 |
| `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs` 与 `SimulationWorld*.cs` | Unity battle pass、world state 和逻辑真值。 | 对齐时按 C++ pass/source contract，不按旧 C# 语义猜测。 |
| `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessBattleKernelHost.cs` | 每个 S0 in-process replica 的 `SimulationWorld + NTSDBattleTickSystem` owner。 | 已读/编译证据已取；没有修改，focused/runtime仍待。 |
| `Assets/NTSD/Scripts/Simulation/Lockstep/InProcessLockstepAuthoritySession.cs` | server → clients 的 authority journal、推进和 first-difference 捕获。 | 已读/编译证据已取；没有修改，focused/runtime仍待。 |
| `Assets/NTSD/Scripts/Simulation/Lockstep/LockstepStartBarrier.cs` / `LockstepSessionIdentity.cs` | S0 session identity、barrier fingerprint、canonical slots。 | identity exposure 安全调整已随 S0 写入；不要对外暴露可变数组。 |
| `Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs` | S0 focused Editor tests。 | 五项 NUnit 独立 fixture；当前未运行，因为项目被现有 Editor 锁定且无远程 runner。 |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 与 `.../Editor/BattleRuntimeSelfCheckEditor.cs` | Unity battle runtime 自检入口。 | 仅在解除 Client 冻结后使用；当前 result 文件不存在，历史 PASS 必须按日期引用。 |
| `Tools/Validate-ChangeLedger.ps1` | 检查工作树的脚本 diff 是否被 Change Record 覆盖。 | 任何含脚本改动的交付/提交前必须跑；文档迁移本身未触发。 |
| `I:\GitHub\Unity_GAS\NTSD_Server` | 独立 Server 根。 | `main` Git 仓库与 `.NET 10` solution 已建立；实际证据见其 `docs/ai/` 下的 Server Record/Ledger/State/Handoff。 |
| `I:\GitHub\Unity_GAS\NTSD_Server\src\NTSD.Server.BattleHost\InMemory\InMemoryAuthoritySession.cs` | Server-only generic authority-first/fail-closed 调度容器。 | 不是 formal BattleKernel，也不定义 S1 protocol 或战斗规则。 |
| `I:\GitHub\Unity_GAS\NTSD_Server\tests\NTSD.Server.BattleHost.Tests\InMemoryAuthoritySessionTests.cs` | 96 帧 TestKernel journal 与 reject/mismatch matrix。 | 已通过；只能证明容器行为。 |
| `I:\GitHub\Unity_GAS\NTSD_Server\docs\ai\CURRENT-HANDOFF.md` | Server 当前 resume card 与 `CLIENT_INTEGRATION_REQUIRED` 范围。 | 在任何 Server 或 Client 下一步前优先阅读。 |
| `docs/ai/CHANGE-RECORDS/S0-INPROC-AUTHORITY-001.md` | 冻结 Client S0 的真实改动和未验证项。 | 是事实来源，不是继续 Client 工作的授权。 |
| `docs/ai/CHANGE-RECORDS/WEB-CADENCE-001.md` | 独立 Web presentation diagnostic 的范围与证明。 | 不是 battle authority / HFR runtime certificate。 |
| `dotnet --list-sdks` / `dotnet --version` | Server SDK preflight。 | 当前 `global.json` 下已解析 `.NET SDK 10.0.400`；仍不得用 8/9 临时顶替。 |
| `& $env:UNITY_EXE -batchmode ... -runTests -testPlatform EditMode ...` | Unity EditMode 测试命令模板。 | 只在用户解除 Client 冻结且确认不与现有 Editor 争用 Library 后执行；实际 Editor 路径以 `ProjectSettings/ProjectVersion.txt` 和本机安装为准。 |

## 10. 给下一个 Codex 的启动指令

```text
请先阅读 Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md、根 AGENTS.md，以及其中第 3 节列出的当前主线文档；随后必须阅读 I:GitHubUnity_GASNTSD_ServerdocsaiS0-S9-EXECUTION-WORKFLOW.md 和 S0-S9-NEXT-PACKAGE-QUEUE.md。不要依赖旧会话上下文。

然后在 I:GitHubUnity_GASNTSD_Server 运行 scripts/Validate-S0S9ExecutionWorkflow.ps1；仅从最早 READY queue row 选择工作。局部 GATED/DEFERRED 只阻断该包，不能替代整个目标的进度判断。

当前主线是 server-first：Server bootstrap、generic authority-session/room、S1 authority-frame/adapter/deadline/initial-tick alignment 与 S2 Server-only preimplementation 均已在各自范围通过。先只读确认 I:\GitHub\Unity_GAS\NTSD_Server、其 `docs/ai/CURRENT-HANDOFF.md`、`STATE.md`、Ledger、最新 Change Record、`dotnet --version` 和 `git status`；不要重做 bootstrap，也不要把 Server-only TestKernel 写成 formal BattleKernel。

当前 formal S0 的 validation-only 门已经获批：现有 Unity S0 已取得 compile evidence 与 `BattleRuntimeSelfCheck=PASS`。focused NUnit 仍需在安全单实例 Editor 中运行；在用户明确允许修改前，仍不得改 Unity Client 源码/Scene/资源/配置，也不得实现 S1 protocol/DTO、deadline/ACK/Jitter、Socket、数据库、真实 transport、Gateway、Matchmaker 或公网操作。

所有 battle rules 继续以 J:\QQFile\NTSD2.4\ntsd_release 的 C++ release live path 为唯一 authority；不能把旧 C#、self-check、性能报告、Web cadence 或 HFR 计划写成 C++ 对齐证书。
```

## 11. 本次迁移自检

- 已读取旧任务的近三天页面；未复制 2026-08-20 及更早聊天全文。
- 已保留近三天用户明确的 server-first、Client freeze、独立目录、.NET 10、网络/公网边界和持续留痕决定。
- 已用当前工作区重新核验 Server 目录、Git 状态、SDK、Unity 版本、SelfCheck result 是否存在及 HFR/Server/Alignment 文档状态。
- 已将旧任务的“sandbox 未热重载”写为历史事实，而不是当前未验证的 blocker；当前仍以 .NET 10 缺失为明确 blocker。
- 未把 `CODE_WRITTEN`、历史 compile/self-check、focused test、Web 自动验证或设计文档写成完整 runtime/C++/Play Mode 完成。
- 交接迁移子任务仅新增 Client handoff 及其 `.meta`；同一线程后续在独立 `NTSD_Server` 仓库创建了 Server bootstrap 文件和验证链。没有修改 Unity battle/runtime、scene、asset、C++ authority、Git 配置或公网资源。
