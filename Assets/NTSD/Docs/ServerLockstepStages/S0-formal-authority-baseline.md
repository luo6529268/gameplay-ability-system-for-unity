# S0 — Formal Authority Baseline

> Current status: `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED`
> Formal phase status: `NOT_VERIFIED`
> Status preserved from the total ledger on 2026-08-29.

## 1. Objective

Prove that one Server world and at least two Client worlds can run the same formal BattleKernel from the same StartBarrier and immutable input journal, producing the same deterministic result without any Socket, public network, prediction, or state overwrite.

## 2. Player-visible result

S0 itself adds no public networking. Its successful result is invisible but foundational: the same inputs must produce the same movement, attacks, collisions, RNG, objects, HP and events in every world. A mismatch must stop promotion and name the first differing tick/domain/slot rather than being hidden by a state correction.

## 3. Entry prerequisites and upstream handoff

- C++ release live runtime remains the only battle-rule authority.
- Unity single-player BattleKernel/tick path remains functional at fixed 30 Hz.
- Test construction must not make Transform, Animator, Scene, or presentation state authoritative.
- The Client freeze remains active unless the user separately authorizes a bounded Client package.

## 4. Data contracts and execution order

Required StartBarrier facts:

- SessionId and initial authority tick;
- protocol and policy version;
- rule/catalog/stage/build fingerprints;
- canonical roster and stable slot ownership;
- deterministic RNG initial state;
- formal-world factory identity.

Execution order:

```text
Create Server world + Client world A + Client world B
    ↓
Apply identical StartBarrier and seed
    ↓
For each authority tick, provide one identical immutable FrameInputSet
    ↓
Step the same formal Kernel exactly once in every world
    ↓
Compare aggregate checksum every tick
    ↓ mismatch only
Capture structured domain/slot/generation/RNG/event first difference
```

## 5. Decisions and evidence sources

- Global design: [`../server-lockstep-s0-s9-design.md`](../server-lockstep-s0-s9-design.md).
- Current evidence ledger: [`../server-lockstep-s0-s9-progress.md`](../server-lockstep-s0-s9-progress.md).
- Formal checksum witness gate: [`../../../../../NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S0-FORMAL-CHECKSUM-WITNESS-GATE-001.md).
- C++ tick/capture mapping remains evidence work; generic initial authority tick is not automatically C++ `world.game_tick`.
- Held-only and zero-carry product decisions belong to S1/S2 and do not close this formal S0 proof.

## 6. Solution and package inventory

| Package/evidence | Current package status | What it proves | What it does not prove |
|---|---|---|---|
| `S0-SERVER-BOOTSTRAP-001` | `FOCUSED_TEST_PASS` | Independent .NET solution, boundaries, commands and no-network host | Formal battle parity |
| `S0-SERVER-INMEMORY-AUTHORITY-001` | `FOCUSED_TEST_PASS` | Generic authority session/TestKernel/checksum mismatch mechanism | Real NTSD formal Kernel |
| `S0-SERVER-ROOM-JOURNAL-001` | `FOCUSED_TEST_PASS` | Stable roster, append-only journal, sequential room owner | Cross-world battle result |
| Existing Unity S0 focused fixture | screenshot 5/5 pass | Existing in-process fixture behavior | Required ten-domain/typed first difference |
| Existing lockstep fixture | screenshot 9/9 pass | Existing lockstep regressions | Formal S0 exit gate |
| `S0-WITNESS-001` | existing Client code-written state, frozen | Candidate witness implementation | Fresh compile/runtime/formal acceptance |

## 7. Boundaries and forbidden shortcuts

- No real transport, ACK/Jitter, public listener, prediction, Gateway, database, snapshot recovery or independent-process requirement in S0.
- Do not use a generic TestKernel as the formal BattleKernel.
- Do not require S5 cross-process evidence to close S0; S0 is same-process multi-world.
- Do not allocate a full structured diagnostic snapshot every tick; capture detail after aggregate mismatch.
- Do not overwrite Client state from Server to make checksums match.
- Do not mark S0 `VERIFIED` from compilation, self-check, one NUnit fixture, or static analysis alone.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Server bootstrap/build | Debug/Release Server solution and no-network host | Passed at package scope |
| Existing Client compile/self-check | Fresh Unity compile and `BattleRuntimeSelfCheck` | Prior evidence exists; Client now frozen |
| Existing focused tests | S0 and lockstep fixtures | Prior screenshot evidence exists |
| Formal world identity | Same formal Kernel/factory in 1 Server + 2 Client worlds | Pending |
| Journal parity | Same StartBarrier, seed and fixed input journal | Pending formal proof |
| Per-tick checksum | Continuous aggregate equality | Pending formal proof |
| First difference | tick/domain/slot/generation/RNG/event witness | Pending |
| C++ authority | Applicable live-path behavior and tick mapping | Pending where not already closed |
| 30 Hz budget | Formal multi-world test within bounded cost | Pending |

## 9. Failure disposition and return rule

On first mismatch, freeze the journal and witness, stop S0 promotion, and return to the formal Kernel/C++ alignment owner. Do not add protocol compensation in S1 or state overwrite in a Client adapter.

## 10. Exact exit gate

S0 becomes `VERIFIED` only when:

- one Server world and at least two Client worlds use the same formal Kernel;
- identical StartBarrier/seed/journal produces identical continuous per-tick checksums;
- a forced mismatch preserves the required typed first-difference witness;
- OfflineLocal policy and 30 Hz battle behavior are unchanged;
- required Client/formal runtime tests actually run and pass.

Until then the current status remains unchanged.

## 11. Handoff to S1

S1 may formally rely only on a verified SessionId, roster/SlotId ownership, initial tick, protocol/policy identity, immutable frame-input value and same-Kernel deterministic step boundary. Server-only S1 preimplementation does not waive this handoff gate.

## 12. Current blockers and next lawful action

- Formal same-Kernel multi-world witness is incomplete.
- Client source/import/compile/test/runtime actions are currently frozen.
- A future Client action requires a separately authorized Task/Change Record and must preserve existing user work.

## 13. Revision history

- 2026-08-29: dossier created by `GOVERNANCE-S0-S9-STAGE-DOSSIERS-001`; no status change.
