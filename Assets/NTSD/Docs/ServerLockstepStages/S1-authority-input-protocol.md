# S1 — Authority Input Protocol and Frame Assembly

> Current status: `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS`
> Formal phase status: `NOT_VERIFIED`
> Status preserved from the total ledger on 2026-08-29.

## 1. Objective

Freeze a transport-agnostic, versioned input/authority-frame contract so packet arrival order, duplication, lateness or a future transport cannot change battle input meaning or immutable authority history.

## 2. Player-visible result

The same physical action is assigned to one explicit future authority tick, consumed once, and never moved to another tick by retransmission. Unauthorized or conflicting input cannot control another player. A late input cannot retroactively create an attack in an already executed frame.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S0 `VERIFIED`.
- Server-only preimplementation may continue to supply reusable DTO/container evidence but cannot mark S1 formal complete.
- C++ release defines held/edge/history consumption and 30 Hz battle behavior.

## 4. Data contracts and execution order

Canonical human submission:

```text
ProtocolVersion
SessionId
ConnectionId
ClientSequence
TargetAuthorityTick
ClientKnownConfirmedAuthorityTick
OwnedSlotHeld[] sorted by SlotId
```

`InputSubmission` carries no per-submission `PolicyVersion`. Server resolves policy from the future-effective activation schedule at the target authority tick.

Execution order:

```text
Capture complete held for every connection-owned human slot
    ↓
Validate protocol/session/connection/slot set/target horizon
    ↓
Apply sequence idempotency and conflict witness before mutation
    ↓
Collect until authority deadline
    ↓
Resolve missing human slot according to confirmed policy
    ↓
Sort all slot inputs by stable SlotId
    ↓
Derive pressed/released from previous/current locked held
    ↓
Lock immutable AuthoritativeFrameEnvelope
    ↓
Step formal Kernel exactly once
```

## 5. Confirmed decisions and unique sources

- Original held-only/current-held bits, all-released baseline, Server-derived edges, Android-one/Windows-two/twenty-human ownership, and zero-carry deadline neutral: [`../../../../../NTSD_Server/docs/ai/AUDITS/NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/NTSD28-ORIGINAL-ONLINE-LOCKSTEP-EVIDENCE-001.md).
- User/product decision record: [`../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-FRAME-INPUT-EDGE-OWNERSHIP-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-S1-FRAME-INPUT-EDGE-OWNERSHIP-001.md).
- Model B policy binding and C1 activation-journal boundary remain in their unique Decision/Record sources.
- GP-01 is a revised proposal awaiting confirmation, not a confirmed S1 fact: [`../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md).
- GP-02 Client capture behavior is product-confirmed: discrete action/digital-direction short presses produce at least one 30 Hz held tick, diagonals are atomic, and virtual-stick quick flicks use stable eight-way quantization without latching intermediate rotation sectors. The unique Decision and deferred Client-impact record remain [`PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md) and [`ONLINE-GAMEPLAY-CLIENT-ADJUSTMENT-REGISTER-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/ONLINE-GAMEPLAY-CLIENT-ADJUSTMENT-REGISTER-001.md); confirmation does not authorize Client source work or close S1.

## 6. Solution and package inventory

Existing Server-only evidence includes:

- authority-frame DTO/assembler, immutable envelopes and lock results;
- frame-to-room adapter and deadline lifecycle;
- initial/future/terminal/confirmed tick bounds;
- provenance validation;
- SessionId and PolicyVersion strong values;
- human-frame value and exhaustive edge derivation;
- policy activation schedule and C1 activation journal/ACK-ready boundary;
- formal held-only connection/ownership/zero-carry assembler package.
- future-effective atomic human↔ServerAI ownership activation schedule/journal; no AI input/timer/Client integration.
- caller-timed monotonic connection liveness and immutable first-timeout witness; no production clock/transport/barrier activation.
- target-tick dynamic human slot subset admission, accepted-future-input overlap rejection and AI-owned lock `FormalAiKernelRequired`.
- tracker-recorded timeout witness to caller-explicit connection-wide ownership request and causal journal.
- same-call earliest safe barrier selection over next tick, activation tail and relevant pending TargetTick successor.
- GP-09 individual-departure admission is Server-focused ready: per-slot input participation is separate from ownership/liveness, accepted future input is preserved and Windows two-to-one submission passes. No Client/wire/AI/recovery action is included.
- GP-09 full-duration witness is also Server-focused ready: it binds only the retained participation entry to caller monotonic time and does not alter input, ownership or AI.

Most recent applicable package:

```text
S1-SERVER-INDIVIDUAL-DEPARTURE-ADMISSION-001
FOCUSED_TEST_PASS / SERVER_INDIVIDUAL_DEPARTURE_ADMISSION_READY
```

This is reusable preimplementation, not formal S1 closure.

## 7. Boundaries and forbidden shortcuts

- No Unity physical capture, wire codec, Socket, transport, snapshot/recovery, prediction, room actor or public deployment in S1.
- No AI input from Client; formal AI remains Kernel-owned.
- No unknown/default action bits, mutable payload aliases, late rewrite or conflicting sequence replacement.
- No policy field added to submission and no change to formed TargetTick/InputDelay semantics without versioned rebarrier.
- No production numeric delay/deadline/grace hidden default.
- No S1 `VERIFIED` claim from generic Server tests.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Protocol values/immutability | Original bits, held-only deep value, strong IDs | Server focused pass |
| Ownership | Android 1, Windows 1～2, room max 20, unauthorized reject | Server focused pass |
| Idempotency/conflict | same payload idempotent, conflict stable/no mutation | Server focused pass |
| Frame lock/order | 1/2/8/20 arrival-order independent SlotId history | Server focused pass |
| Deadline fill | missing neutral, zero carry, one release/no press | Server focused pass |
| Policy history | resolved policy and activation journal witness | Server focused pass |
| Ownership history | initial owner plus future human↔ServerAI barriers | Server focused pass; AI input/timer/Client pending |
| Formal Kernel mapping | authority tick to C++ completed-world/input phase | Pending |
| Client capture | real Android/Windows capture and short-tap contract | Product contract confirmed; implementation/runtime evidence pending and Client frozen |
| Cross-world history | same formal worlds produce same authority history/checksum | Pending S0/S1 formal proof |
| Wire serialization | frozen ABI/malformed/unknown-version matrix | Deferred to formal wire/transport scope |

## 9. Failure disposition and return rule

- Input/edge disagreement returns to S1 contract owner; preserve first conflict before mutation.
- Battle result divergence returns to S0/formal Kernel, not protocol compensation.
- ACK/Jitter/slow-client behavior belongs to S2.
- Snapshot/history recovery belongs to S3.

## 10. Exact exit gate

S1 becomes `VERIFIED` only after S0 is verified and the same formal Server/Client worlds prove that frozen schema, capture/tick mapping, deadline/lock/idempotency/dispositions and authority history are identical under the formal contract. Server preimplementation alone is insufficient.

## 11. Handoff to S2

S1 must hand off frozen message/value schema, canonical equality, sequence identity, future target/deadline rules, source/fill provenance, locked history, policy activation witness and stable dispositions.

## 12. Current blockers and next lawful action

- GP-01～GP-09 product behavior is confirmed; production numeric parameters remain measured.
- GP-02 product behavior is confirmed, but formal Kernel/tick mapping and actual Client capture/wire/runtime evidence remain missing.
- S0 formal exit gate is not closed.
- Do not create a production timing/transport package until the applicable Decision and formal dependency are ready.

## 13. Revision history

- 2026-08-29: dossier created; current held-only Server evidence reconciled; phase status unchanged.
- 2026-08-29: GP-02 product contract linked as confirmed; Client implementation remains deferred and phase status unchanged.
- 2026-08-29: Server-only ownership activation schedule/journal passed; formal AI input/state hash, timing, assembler/Client/recovery integration and phase status remain pending.
- 2026-08-29: Server-only caller-timed connection liveness/first-timeout witness passed; production clock/transport, safe barrier admission, formal AI, recovery and phase status remain pending.
- 2026-08-29: Server-only target-tick ownership-aware human admission and AI-owned lock fail-closed passed; formal AI/frame/checksum, Client, clock/transport, recovery and phase status remain pending.
- 2026-08-29: Server-only recorded timeout→connection-wide ownership causality/journal passed; automatic tick planner, formal AI, Client, production clock/transport, recovery and phase status remain pending.
- 2026-08-29: Server-only earliest-safe ownership barrier planner passed without elapsed-time conversion or input mutation; formal AI, Client, production clock/transport, recovery and phase status remain pending.
- 2026-08-30: GP-09 Server command contract closed; per-slot participation/Windows subset/accepted-input preservation are frozen and the first Server-only admission package was selected. Formal S1 status remains unchanged.
- 2026-08-30: The selected individual-departure admission package passed test-first, accepted-input, Windows subset, idempotency/conflict, terminal, Debug/Release/full Server and no-network Host evidence. It does not close formal S1.
- 2026-08-30: The slot-specific full-duration witness package passed without changing input or ownership. It is S2 preimplementation and does not close formal S1.
