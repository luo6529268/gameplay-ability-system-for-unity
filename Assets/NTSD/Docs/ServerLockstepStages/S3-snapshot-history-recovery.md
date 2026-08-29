# S3 — Snapshot, History, Desync and Recovery

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Status preserved from the total ledger on 2026-08-29.

## 1. Objective

Create a formal Server-owned recovery timeline so a severely lagging, desynchronized, reconnecting or future observing Client can restore from an authoritative snapshot plus continuous authority history without rewinding the Server or healthy Clients.

## 2. Player-visible result

- A reconnecting player sees an explicit recovery state, not an old local world pretending to continue.
- Healthy players continue while one Client restores.
- A first desync can produce diagnostic/recovery behavior rather than silent teleport/state overwrite.
- Recovery finishes only when the Client reaches the current authority tick and checksum agrees.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S2 `VERIFIED`.
- S2 must hand off continuous frame sequence, confirmed/ready cursors, missing/disconnect state, recovery trigger and bounded gap threshold.
- A real formal BattleKernel snapshot/factory/restore owner must exist; generic frame lists are not snapshots.

## 4. Data contracts and recovery order

Required owners:

```text
FrameHistoryRing
SnapshotRing
ChecksumHistory
RetainedBase / Epoch
SnapshotSchemaVersion
RecoveryPackage
RecoveryDisposition
```

Recovery package minimum:

- protocol/session/policy and activation-journal prefix;
- snapshot tick and checksum;
- deterministic battle-world state;
- stable slot/generation and all external cooldown matrices;
- prior held, edge/input windows and history;
- exact RNG state/cursor;
- event cursor and deterministic statistics;
- contiguous authority-frame range after snapshot;
- target authority tick.

Order:

```text
Freeze recovery target and retained range
    ↓
Restore authoritative snapshot at safe completed-tick boundary
    ↓
Restore stable slots/generations, then external aRest/vRest matrices
    ↓
Restore RNG/input/event cursors
    ↓
Replay contiguous authority frames to target tick
    ↓
Compare checksum and activation-journal acknowledgement
    ↓
Only then enter Ready / ownership-barrier eligibility
```

## 5. Decisions and evidence sources

- Snapshot field/safe-point prerequisite: [`../../../../../NTSD_Server/docs/ai/AUDITS/S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S3-FORMAL-SNAPSHOT-PREREQUISITE-001.md).
- History-retention prerequisite: [`../../../../../NTSD_Server/docs/ai/AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/S3-AUTHORITY-HISTORY-RETENTION-PREREQUISITE-001.md).
- Cross-policy recovery boundary remains linked to the C1 activation journal and acknowledged-prefix contract.
- GP-06 and GP-07 product behavior are confirmed in [`../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md): players may reconnect after the 30-second AI takeover, but old local state is discarded and human ownership returns only after snapshot/history/checksum success plus a future barrier; the first checksum mismatch gets one authority recovery attempt, and mismatch after that same recovery disconnects the current Client session without overriding Server truth.
- GP-07 public reference audit: [`../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-DESYNC-DISPOSITION-EVIDENCE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-DESYNC-DISPOSITION-EVIDENCE-001.md). Historical King of Glory/Tencent evidence confirms periodic hash plus replay/log/first-difference diagnostics, but does not disclose the online player's recovery/retry/disconnect disposition.

## 6. Solution and package inventory

Current reusable evidence:

- immutable in-memory authority frames and room journal;
- gap responses and confirmed/ready cursors;
- policy activation journal and envelope policy witness;
- generic checksum mismatch mechanisms;
- C++ field/RNG/safe-point prerequisite audits.

Not implemented:

- formal BattleWorld snapshot schema/factory;
- retained-base/epoch and bounded rings;
- serializer/persistence contract;
- restore/replay state machine;
- history-expired disposition;
- reconnect/observer package;
- real Client recovery and ownership return.

## 7. Boundaries and forbidden shortcuts

- Do not call an unbounded `List` a history ring.
- Do not truncate current history until retained-base, gap and recovery semantics are versioned.
- Do not use `InputHandler::snapshot()` or UI/result tables as a BattleWorld snapshot.
- Do not restore external cooldown matrices before stable-slot rehydration.
- Do not capture snapshots in the middle of a C++-equivalent tick.
- Do not trust Client local snapshot as authority.
- Do not let recovery overwrite or rewind healthy worlds.
- Do not hide checksum mismatch with state correction.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Schema inventory | every deterministic domain and exclusion named | Analysis exists |
| Snapshot round trip | capture → mutate → restore exact world | Not started |
| Replay | snapshot + contiguous frames reaches exact target | Not started |
| Ten-domain checksum | Server/Client equality after restore/replay | Not started |
| Slot/generation | first typed mismatch and stable rehydration | Not started |
| RNG | exact seed/cursor/call ordering restored | Not started |
| Policy activation | initial policy + activation prefix + ACK semantics | C1 mechanism exists; recovery pending |
| History retention | bounded range and history-expired result | Not started |
| Client reconnect | old world rejected, recovery UI/state, barrier return | Not started |
| Healthy continuity | recovery Client does not rewind/stall healthy worlds | Not started |

## 9. Failure disposition and return rule

- Snapshot/restore field mismatch returns to S3 schema/factory owner.
- Battle rule divergence returns to S0/formal Kernel.
- Missing/gap/ready trigger defects return to S2.
- Cross-process serializer difference later returns to S5.

## 10. Exact exit gate

S3 becomes `VERIFIED` only when authoritative snapshot → mutation/desync → restore → contiguous history replay produces identical formal Server/Client ten-domain checksum, slot/generation, RNG, input history and event cursor; recovery affects only the target Client and all failure dispositions are stable and replayable.

## 11. Handoff to S4/S5

Hand off frozen snapshot/history/checksum/recovery schema, retained-range contract, factory/restore order, recovery state machine, activation-prefix behavior and confirmed event cursor.

## 12. Current blockers and next lawful action

- S2 is not formally verified.
- Formal Kernel snapshot/restore owner is absent.
- GP-06/GP-07 product behavior is confirmed; formal recovery implementation and retention/timing evidence remain pending.
- No S3 source package is READY.

## 13. Revision history

- 2026-08-29: dossier created from existing prerequisite audits; status remains `NOT_STARTED`.
- 2026-08-29: GP-06 post-30-second reconnect and future-barrier return linked as confirmed; S3 implementation remains `NOT_STARTED`.
- 2026-08-29: GP-07 King of Glory/Tencent public detection/diagnostic evidence linked; exact NTSD player disposition remains pending and S3 status unchanged.
- 2026-08-29: GP-07 confirmed one authority recovery attempt followed by current-session disconnect on repeat mismatch; S3 implementation remains `NOT_STARTED`.
