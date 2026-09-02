# S5 — Shared Formal Kernel and Independent Server Host

> **NTSD24_AUTHORITY_SUPERSEDED（2026-09-02）：** 本文所称未限定版本的 “C++ release” 形成于旧 NTSD 2.4 权威期，仅作为历史证据；不得据此定义当前战斗规则、pass、timing、slot、RNG、字段、生命周期、表现或“已对齐”状态。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前权威是 NTSD 2.8-Logan 正式 EXE 及其对应 playable 源码，旧结论一律 `REBASELINE_REQUIRED`。

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Existing independent .NET bootstrap is preimplementation, not S5 completion.

## 1. Objective

Prove that one uniquely owned formal BattleKernel and Protocol can run in Unity Client runtimes and an independent headless/.NET Server Host with identical journal, snapshot, restore/replay and checksum behavior, while each room has a real isolated single-writer execution owner.

## 2. Player-visible result

Players connect to a Server that calculates the same battle rules as the Client without depending on Unity Scene/presentation. A failure in one room does not corrupt or terminate unrelated rooms. Misconfiguration fails before accepting a match.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S4 `VERIFIED`.
- S0～S4 must hand off formal Kernel, frozen Protocol, snapshot/history/recovery schema and prediction exclusion/contract.
- Existing no-network `NTSD.Server.Hosting` bootstrap and `SequentialSingleWriter` label are reusable evidence only.

## 4. Module and operation contracts

Required one-way boundaries:

```text
Battle.Protocol
    ↓
Battle.Kernel
    ↓
ClientAdapter      BattleHost
                        ↓
                 Transport adapter later
```

Room operation order must be explicitly versioned, for example:

```text
admit input / connection lifecycle command
    ↓
deadline resolution
    ↓
lock authority frame
    ↓
atomic Kernel step + checksum + journal commit
    ↓
publish progress/frame
```

The exact exception/rollback/fault contract must be defined before implementation; this example is not itself approval.

## 5. Decisions and evidence sources

- Kernel/room fault prerequisite: [`../../../../../NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S5-KERNEL-ROOM-FAULT-BOUNDARY-PREREQUISITE-001.md).
- Single-writer actor prerequisite: [`../../../../../NTSD_Server/docs/ai/AUDITS/S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S5-SINGLE-WRITER-ROOM-ACTOR-PREREQUISITE-001.md).
- GP-08 product disposition was confirmed on 2026-08-29: when atomic room-tick completion cannot be proved, the room faults, stops input/tick progress, records immutable first-fault evidence, and the match is invalid with no player loss; unrelated rooms and the Host continue. Automatic room recovery remains forbidden until S3/S5 prove snapshot, atomic commit/rollback and injected-fault recovery. Public King of Glory evidence supports modular fault shielding and limited blast radius but does not disclose this exact NTSD player result: [`../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-SERVER-FAULT-ISOLATION-EVIDENCE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-SERVER-FAULT-ISOLATION-EVIDENCE-001.md).
- GP-09 product behavior was confirmed on 2026-08-29: online battle does not pause; individual surrender/exit is an idempotent room/session lifecycle command, follows missing-neutral and the existing 30-second ownership barrier to Server AI, does not end the whole match, and remains reconnectable through GP-06. `GP09-ORIGINAL-ONESHOT-CONSUMER-MAPPING-001` later proved the historical masks are shared F1～F9/feature events and F4 exits the shared battle; no historical bit is the new individual lifecycle command.
- `GP09-INDIVIDUAL-DEPARTURE-COMMAND-CONTRACT-001` freezes command-vs-input ordering: whichever direct operation is admitted first determines preservation/rejection at the future participation tick. The first Server package may test sequential direct calls only; it must not claim an actor, mailbox, thread safety or atomic S5 commit.
- C++ release remains the only battle-rule authority; no Server-specific rules fork.

## 6. Solution and package inventory

Existing preimplementation:

- independent `.NET 10.0.400` solution and project boundaries;
- no-network bootstrap, strong configuration/NodeId and liveness/readiness facts;
- Protocol/BattleHost generic owners and architecture tests;
- synchronous in-memory session/room/assembler/adapter fixtures.
- `com.ntsd.battle-kernel` exists as a real UPM/.NET package and Cut A has one shared deterministic RNG source consumed by Unity and .NET; this is foundation evidence only.
- the FrameInput relocation seam and shared-owner Cut B are focused-test ready: the platform-independent public value/hash now has one Server-owned physical source consumed by Unity and .NET, while Client capture/preallocation/dense trace remain outside.
- `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SHARED-OWNER-001` is focused-test ready as a pre-S5 foundation cut; it does not enter S5 or change the formal marker.
- `CLIENT-CPP-STAGE-SPAWN-REST-ALIGNMENT-001` is focused-test ready as a pre-Cut-C Client authority correction; it does not enter S5, implement the Host, or change the formal marker.

Missing:

- formal Kernel implementation marker/assembly;
- complete formal Kernel package/ABI consumable by Unity and independent Server Host (RNG Cut A and FrameInput Cut B are shared-owned; StageSpawn correction and the Client-owned Cut C slot/lifecycle seam are focused ready, but the Cut C production shared-owner move remains gated, followed by Cut D～G);
- real room actor/mailbox/scheduler/backpressure/lifecycle;
- atomic tick commit and fault witness;
- room/process isolation and injected-fault tests;
- cross-runtime snapshot/replay/checksum matrix.

## 7. Boundaries and forbidden shortcuts

- Do not copy a second BattleKernel into Server.
- Do not treat `SequentialSingleWriter` metadata as an actor/thread-safety guarantee.
- Do not add an ad-hoc global `lock` or queue without operation order/backpressure/fault contract.
- Do not broadly catch Kernel exceptions, retry a tick or remove journal entries without atomic recovery evidence.
- Do not bind a real transport/public listener; that is S6.
- Do not use UnityEngine, Scene, Transform, Animator or resources in formal Kernel/Protocol.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Project boundaries | no Unity/Host/transport leak into Protocol/Kernel | Generic architecture tests pass |
| Formal Kernel marker | real shared implementation | `false` / missing |
| Same package/runtime | Unity Mono/IL2CPP and .NET Server consume same owner | RNG Cut A and FrameInput Cut B direct/locked-artifact/Unity evidence passed; complete formal Kernel and platform/runtime matrix pending |
| Room actor | ordered mailbox, lifecycle, backpressure | Not started |
| Atomic commit | world/journal/checksum/progress commit or stable fault | Not started |
| Fault isolation | one room failure leaves other rooms/Host healthy | Not started |
| Cross-runtime parity | fixed journal/snapshot/replay/checksum | Not started |
| Clean command chain | restore/build/test/run-local/health | Bootstrap evidence exists; formal Host pending |

## 9. Failure disposition and return rule

- Battle divergence returns to S0.
- Protocol/history/recovery divergence returns to S1～S3.
- Prediction-only issue returns to S4.
- Host/actor/atomicity/isolation issue remains S5 and blocks transport entry.

## 10. Exact exit gate

S5 becomes `VERIFIED` only when a uniquely owned formal Kernel runs identically in-process and independent-process across supported runtimes, room operation order and atomic fault behavior are proven, configuration fails closed, health is accurate, and one room fault cannot terminate unrelated rooms or the Host.

## 11. Handoff to S6

Hand off an independent ServerHost artifact, frozen protocol ABI, formal Kernel package, room endpoint abstraction, lifecycle/fault contract, health/readiness and deterministic in-memory transport oracle.

## 12. Current blockers and next lawful action

- S4 and all prior formal phases are not verified.
- Formal Kernel, actor and atomic-fault decisions are absent.
- Current independent Server remains no-network preimplementation.
- Shared RNG Cut A and the FrameInput relocation seam are ready, but neither enters S5 nor proves shared FrameInput ownership, the complete Kernel, actor, atomic commit, snapshot/replay, fault isolation or independent Host parity.

## 13. Revision history

- 2026-08-29: dossier created; existing bootstrap explicitly separated from S5 formal completion.
- 2026-08-29: King of Glory public module-isolation evidence linked for GP-08; the user then confirmed room fault, invalid/no-loss match disposition, first-fault retention and Host isolation. S5 status remains unchanged because atomic/injected-fault implementation evidence does not yet exist.
- 2026-08-29: GP-09 online no-pause and individual surrender/exit lifecycle were frozen as product behavior; implementation remained pending.
- 2026-08-30: the original one-shot consumer audit closed Mask A/B mapping and proved F4 is shared battle exit rather than individual departure. S5 remains NOT_STARTED because room actor/command ordering/atomicity/source evidence is still absent.
- 2026-08-30: the individual-departure Server contract froze the future actor ordering boundary and selected a direct-call admission package. It does not implement the S5 actor or change S5 status.
- 2026-08-30: The direct-call individual-departure admission package passed sequential ordering tests. This is not actor/mailbox/thread-safety/atomic-commit evidence, so S5 remains NOT_STARTED/NOT_VERIFIED.
- 2026-08-30: The caller-timed departure witness package passed direct sequential tests. Admission-time registration is not yet an actor-owned atomic room operation, so S5 remains unchanged.
- 2026-08-30: The single-slot ownership request package appends causality and ownership in one direct method chain, but AI step/checksum/commit and actor atomicity are absent; S5 remains NOT_STARTED/NOT_VERIFIED.
- 2026-08-30: `GOVERNANCE-S0-FORMAL-KERNEL-NEXT-PACKAGE-SELECTION-001` began a no-source dependency/phase audit after S0 witness focused closure. It does not enter S5, change the formal marker or waive the S4 entry gate.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-DETERMINISTIC-RNG-SHARED-OWNER-001` established the Server-owned UPM/.NET package and shared RNG source with dual-consumer evidence. S5 remains NOT_STARTED/NOT_VERIFIED because the full Kernel, Host/actor, atomicity, recovery and isolation gates are untouched.
- 2026-08-30: `CLIENT-FORMAL-KERNEL-FRAME-INPUT-SEAM-001` separated relocatable FrameInput values from Client-only helpers and passed focused/regression/allocation evidence. It did not move the value source or prove a .NET consumer, so S5 remains NOT_STARTED/NOT_VERIFIED and the shared-owner move needs a new exact authorization.
