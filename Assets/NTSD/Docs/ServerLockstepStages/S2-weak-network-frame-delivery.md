# S2 — Weak-Network Frame Delivery and Slow-Client Isolation

> Current status: `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS`
> Formal phase status: `NOT_VERIFIED`
> Status preserved from the total ledger on 2026-08-29.

## 1. Objective

Ensure delay, jitter, drop, duplication, reordering, short blackout and one slow Client cannot lose or duplicate canonical actions, create frame holes, rewrite history, produce unbounded catch-up, or stop the Server and healthy Clients indefinitely.

## 2. Player-visible result

- Normal fluctuation is absorbed through redundancy/retransmission without visible rule changes.
- If one human input misses the authority deadline, only that slot becomes neutral for the tick; other players continue.
- A Client missing downlink frames shows weak-network/catch-up behavior and recovers in sequence; it does not force the room to rewind.
- Logic remains 30 Hz; presentation may interpolate without changing battle truth.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S1 `VERIFIED`.
- S1 must hand off frozen input/frame schema, sequence/target/deadline semantics, policy/provenance and immutable history.
- S3 snapshot recovery is not available as an S2 implementation shortcut; S2 only defines when recovery becomes necessary.

## 4. Data contracts and execution order

Relevant application facts:

- current plus unconfirmed input redundancy window;
- FrameAck and ServerProgress;
- activation-journal cursor/ack;
- contiguous ready range;
- gap request/response;
- locked authority-frame history;
- per-slot source/fill reason;
- bounded future admission and catch-up budget.

Revised GP-01 pipeline:

```text
Client capture at fixed 30 Hz contract
    ↓
Uplink current + unconfirmed held submissions
    ↓ adaptive redundancy only; target meaning fixed
Server admission / deadline / immutable lock / Kernel step
    ↓
Downlink current + recent immutable authority frames
    ↓ adaptive redundancy / app resend
Client strict contiguous ready range
    ├─ small gap: recover from redundancy
    ├─ unresolved gap: gap request
    ├─ small lag: bounded catch-up
    └─ excessive/history-expired: signal S3 recovery requirement
Presentation interpolates snapshots without writeback
```

## 5. Decisions and evidence sources

- GP-01 fixed-timeline/adaptive-network-protection contract and all gameplay confirmations: [`../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md). GP-01 was confirmed on 2026-08-29; production numeric parameters remain measured rather than product-pending.
- Public mature frame-sync evidence and non-copy boundaries: [`../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md).
- Missing-input product contract: [`../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-S2-MISSING-INPUT-PRODUCT-CONTRACT-001.md).
- Current-worktree exit-gate reconciliation: [`../../../../../NTSD_Server/docs/ai/AUDITS/S2-EXIT-GATE-COVERAGE-RECONCILIATION-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S2-EXIT-GATE-COVERAGE-RECONCILIATION-001.md). It distinguishes the bounded Server oracle from real Client Jitter/catch-up and slow-client evidence.
- Held-only/zero-carry facts are confirmed. PvP and cooperative PvE both keep human ownership plus missing neutral before the 30-second heartbeat timeout, then change the disconnected slot to formal-Kernel Server AI at an explicit ownership barrier and continue the match. GP-03 confirms approximately one second as the first explicit short-grace test candidate; the production grace value remains measured. GP-06 allows post-takeover reconnect and human reclaim only after authoritative recovery/checksum plus a future barrier; implementation and retention thresholds remain pending/measured.

## 6. Solution and package inventory

Existing Server-only preimplementation packages cover:

- ACK confirmation and global confirmed range;
- ready-buffer contiguous consumption and future horizon;
- input redundancy windows and bounded ingress capacity;
- authority-frame gap request/response;
- deterministic in-memory inbound/downlink disorder harnesses;
- disorder action validation;
- cross-policy activation journal and acknowledged-prefix ready/gap guard;
- zero-carry missing-neutral Server assembly.
- future-effective human↔ServerAI ownership activation schedule/journal.
- caller-timed monotonic heartbeat sequence/timeout state and immutable first witness; no production clock or transport.
- target-tick dynamic human subset, accepted-future-input overlap witness and formal-Kernel-required AI-owned lock boundary.
- tracker-recorded timeout→explicit connection-wide ownership activation request and contiguous causal journal.
- same-call earliest-safe barrier selection over execution cursor, activation tail and relevant pending input successor.
- Hosting-owned .NET 10 TimeProvider process-local monotonic millisecond adapter; suspend/transport evidence pending.
- explicit versioned short-grace classification from the same last-seen timeline; 1,000 ms is a focused test candidate, while only the full timeout creates a witness.

Missing formal packages include:

- measured and versioned GP-01 production timing/redundancy parameters;
- real Client redundancy/ACK/gap/ready/catch-up consumer;
- formal-Kernel AI input/state-hash integration, production timer and full ownership mode lifecycle;
- fixed slow-client/blackhole matrix over real formal worlds;
- measured Android/Windows catch-up and 20-human bandwidth/CPU budgets.

## 7. Boundaries and forbidden shortcuts

- Do not change battle logic to 15 Hz; historical 15 packets/s is not an NTSD rule.
- Do not dynamically reinterpret formed TargetTick/InputDelayFrames because ping changes.
- Do not use a large playback buffer as a hidden permanent latency increase.
- Do not allow Client to execute `F4` before missing `F3`.
- Do not add unbounded catch-up loops.
- Do not let presentation interpolation write logic position/HP/input/RNG.
- Do not select UDP/library/MTU/encryption in S2; real transport is S6.
- Do not implement snapshot restore in S2; recovery belongs to S3.
- Do not treat in-memory disorder harnesses as real Client/public weak-network proof.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| ACK/confirmed | monotonic, bounded, stable rejection | Server focused pass |
| Redundancy | immutable ordered windows, idempotent ingress, bounded entries | Server focused pass |
| Ready/gap | strict contiguous range, bounded gap response, policy-prefix guard | Server focused pass |
| In-memory disorder | delay/drop/duplicate/reorder reproducible | Server focused pass |
| Missing deadline | neutral, zero carry, no fabricated press | Server focused pass |
| Ownership barrier schedule | atomic future human↔ServerAI owner journal | Server focused pass; timer/AI input/Client/recovery pending |
| Connection grace classification | exact Healthy/Reconnecting/TimedOut boundary; no pre-timeout witness or ownership side effect | Server focused pass; production value/transport/Client pending |
| GP-01 product contract | fixed logic + adaptive per-Client network protection | Confirmed; production numeric measurement pending |
| Real Client consume | strict sequence/redundancy/gap/ACK | Pending/frozen |
| Slow-client matrix | one blackhole/extreme jitter does not stop healthy worlds | Pending formal evidence |
| Bounded dequeue primitive | explicit positive `maxFrames`, continuous prefix only, later ready frames retained | Server focused pass; no production value selected |
| Production Client catch-up | measured logic/presentation budget, no unbounded loop/GC spike | Pending Client/formal measurement |
| 20-human budget | 30 Hz aggregate/network/Kernel/presentation metrics | Pending |
| OfflineLocal | unchanged single-player tick behavior | Pending formal regression at phase close |

## 9. Failure disposition and return rule

- Canonical input/frame defect returns to S1.
- Battle checksum divergence returns to S0/formal Kernel.
- Gap too old or lag too large transitions to an explicit S3 recovery requirement; do not invent a hidden ring/snapshot.
- Actual packet/MTU/library defect returns to S6 after S5, without changing S1/S2 semantics.

## 10. Exact exit gate

S2 becomes `VERIFIED` only after S1 is verified and real formal Server/Client worlds pass a deterministic matrix covering delay, jitter, drop, duplicate, reorder, short blackout, input blackhole and bounded catch-up, with no lost/duplicated skill edge, no healthy-client indefinite stall, no history rewrite and no unbounded workload.

## 11. Handoff to S3

S2 must hand off continuous authority history semantics, per-client confirmed/ready cursor, gap boundary, missing/grace/disconnect state machine, recovery trigger, retained-range requirement and measurable catch-up threshold.

## 12. Current blockers and next lawful action

- GP-01 product direction is confirmed: fixed 30 Hz and formed tick/delay semantics, per-Client adaptive redundancy/retransmission/gap, strict contiguous consumption, bounded catch-up, recovery threshold and presentation isolation. Exact production InputDelay/deadline/redundancy/gap/catch-up/history values remain measured. GP-03 confirms the one-second short-grace test candidate, GP-04/GP-05 confirm the shared PvP/PvE 30-second Server-AI transition, and GP-06 confirms post-takeover reconnect/reclaim eligibility; measured production grace and S3 implementation/retention evidence remain pending.
- Formal S1 and Client consumption are not verified.
- Formal Kernel AI ownership and S3 recovery owners do not yet exist.
- The explicit GP-03 short-grace Server classifier is focused-pass complete. Current Queue order 2 remains gated by formal S1/real Client, authenticated transport/platform evidence, formal AI/recovery and measured production parameters.
- Current-worktree reconciliation found no independent Server-only catch-up/Jitter source package: the reusable ready/disorder oracle is already bounded, while the remaining owner is real Client consumption/presentation plus formal measurement and S3 recovery thresholds.

## 13. Revision history

- 2026-08-29: dossier created with GP-01/public frame-sync evidence links; phase status unchanged.
- 2026-08-29: PvP 30-second Server-AI ownership transition recorded as a partial GP-03/GP-04 decision; no lifecycle source authorization and phase status unchanged.
- 2026-08-29: GP-04/GP-05 closed the shared PvP/PvE 30-second Server-AI takeover product rule; implementation remains gated and phase status unchanged.
- 2026-08-29: GP-03 confirmed approximately one second as a non-production short-grace test candidate; implementation remains gated and phase status unchanged.
- 2026-08-29: GP-06 confirmed reconnect after 30-second AI takeover with authoritative recovery and future-barrier return; implementation remains S3-gated and phase status unchanged.
- 2026-08-29: GP-01 fixed-timeline/adaptive-network-protection direction confirmed; exact production numeric parameters, real Client consumption and weak-network evidence remain pending, so S2 phase status is unchanged.
- 2026-08-29: Server-only ownership activation schedule/journal passed without timer, AI input/provider, assembler, Client or recovery integration; S2 phase status is unchanged.
- 2026-08-29: Server-only caller-timed connection liveness and immutable first-timeout witness passed; production clock/transport, ownership barrier, formal AI, recovery and S2 phase status remain pending.
- 2026-08-29: Server-only ownership-aware human admission passed with overlap no-mutation and `FormalAiKernelRequired`; formal AI/Client/clock/transport/recovery and S2 phase status remain pending.
- 2026-08-29: Server-only recorded timeout→connection-wide ownership request/journal passed; automatic safe-tick planner, formal AI/Client/production clock/transport/recovery and S2 phase status remain pending.
- 2026-08-29: Server-only earliest-safe barrier planner passed with unrelated-pending exclusion and terminal fail-closed; formal AI/Client/production clock/transport/recovery and S2 phase status remain pending.
- 2026-08-29: Hosting TimeProvider monotonic adapter passed frequency/floor/regression/System smoke without transport/timer/room action; authenticated heartbeat and OS/VM suspend evidence remain pending, S2 phase status unchanged.
- 2026-08-29: Explicit versioned Healthy/Reconnecting/TimedOut classification passed exact 1,000/30,000 ms fixture boundaries and heartbeat recovery without pre-timeout witness, ownership, AI, input or Client effects; production grace remains measured and S2 phase status is unchanged.
- 2026-08-29: Exit-gate reconciliation mapped ready/disorder/deadline/liveness evidence to the upstream S2 requirements. Bounded Server dequeue is focused-pass evidence, but production Client catch-up, formal slow-client worlds and OfflineLocal remain pending; no source or phase status changed.
