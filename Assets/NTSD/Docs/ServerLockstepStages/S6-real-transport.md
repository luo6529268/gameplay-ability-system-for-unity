# S6 — Real Transport Selection and Integration

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Transport selection: `DEFERRED`

## 1. Objective

Map the verified application protocol onto one real low-latency transport without allowing library behavior, packet boundaries, loss recovery or serialization to change authority-frame semantics.

## 2. Player-visible result

Players can connect on localhost/LAN and an explicitly authorized node with low-latency input/frame delivery. Ordinary loss can recover without duplicate actions or room-wide stalls. Transport failure has a stable connection disposition rather than corrupting battle history.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S5 `VERIFIED`.
- S5 hands off ServerHost artifact, protocol ABI, room endpoint, in-memory transport oracle and health/fault boundaries.
- Endpoint owner must explicitly authorize OS, address, ports, firewall/security group and credentials.

## 4. Transport/application boundary

Transport owns:

- byte send/receive;
- connection/channel mechanics;
- encryption handshake implementation where selected;
- MTU-safe datagram/stream delivery primitives.

Application protocol continues to own:

- session/sequence/TargetTick;
- idempotency/conflict/late disposition;
- ACK/confirmed/ready/gap;
- redundancy policy and authority history;
- deadline, missing-neutral and recovery trigger;
- policy activation and checksum witness.

## 5. Decisions and evidence sources

- `S-NET-004` transport selection remains pending evaluation.
- Mature public historical frame-sync evidence supports evaluating UDP with application-layer sequence/cache/redundancy, but does not select an NTSD library or parameter: [`../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md).
- GP-01 now freezes fixed 30 Hz/formed tick-delay semantics plus adaptive per-Client redundancy/retransmission/gap behavior; exact production counts/timing remain measured before transport binding.

## 6. Candidate evaluation

Candidates may include UDP plus a project-owned application protocol, KCP, ENet, LiteNetLib or another maintained option. Evaluate:

- Android/Windows support and maintenance;
- reliable/unreliable ordered channels;
- MTU, fragmentation and amplification behavior;
- congestion/pacing and head-of-line risk;
- encryption/authentication integration;
- allocations/CPU and 20-human packet rate;
- license, security history and operational tooling;
- exact equivalence with in-memory fixtures.

No candidate is currently selected.

## 7. Boundaries and forbidden shortcuts

- No public production rollout or nationwide matching in S6.
- No transport DTO/library type inside Battle.Protocol/Kernel.
- No changing 30 Hz, TargetTick, deadline or fill to accommodate a library.
- No raw IP fragmentation reliance without tested application framing.
- No credentials/tokens in source, logs or documents.
- No probing or operating an endpoint beyond explicit authorization.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Serialization ABI | versioned round trip/malformed/unknown fields | Not started |
| Localhost/LAN | same history as in-memory transport | Not started |
| Authorized node | same fixed script and checksum | Not started |
| MTU/fragmentation | boundaries, loss and reassembly | Not started |
| Loss/reorder/duplicate | application semantics unchanged | Not started |
| Security | identity/auth/replay/amplification review | Not started |
| Performance | packet rate, bandwidth, allocation, CPU | Not started |
| Health/shutdown | listener readiness and graceful stop | Not started |

## 9. Failure disposition and return rule

- Application semantic mismatch returns to S1/S2, not a Kernel patch.
- Snapshot/recovery mismatch returns to S3.
- Host endpoint/fault issue returns to S5.
- Library/MTU/congestion defect stays in S6 and is reproduced against the in-memory oracle.

## 10. Exact exit gate

S6 becomes `VERIFIED` only when one selected real transport passes localhost/LAN/authorized-node equivalence with in-memory authority history/checksum, MTU/loss/reorder/security/performance tests and stable lifecycle/health behavior.

## 11. Handoff to S7

Hand off versioned endpoint configuration, supported runtime matrix, packet/channel ABI, observability hooks, weak-network controls and a deployable but not yet publicly released ServerHost.

## 12. Current blockers and next lawful action

- S5 is not verified.
- No real transport or port is authorized by this dossier.
- Do not start transport selection from historical《王者荣耀》articles alone.

## 13. Revision history

- 2026-08-29: dossier created; transport remains deferred and unselected.
