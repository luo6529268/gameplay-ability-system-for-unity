# S7 — Public Weak-Network Runtime and Long-Session Validation

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Public runtime authorization: `NOT_GRANTED_BY_THIS_DOSSIER`

## 1. Objective

Validate the real transport and formal Server/Client system under authorized public/mobile networks, device stalls, foreground/background transitions, disconnect/reconnect and long sessions, while ensuring one slow Client never indefinitely blocks healthy players.

## 2. Player-visible result

- Android and Windows players can complete real PvP under normal mainland network conditions.
- Short fluctuation is absorbed or isolated to the affected player.
- Weak-network UI, neutral/grace/AI/recovery behavior matches confirmed product decisions.
- Network switching or Client restart can recover without continuing an old world.
- Long sessions do not accumulate unbounded memory, latency or catch-up work.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S6 `VERIFIED`.
- S6 hands off a real transport, deployable endpoint, fixed ABI, observability and in-memory-equivalent behavior.
- Public node, test accounts/devices, addresses, ports, security groups and test windows require explicit owner authorization.

## 4. Test scenarios and data order

Required real scenarios:

- baseline RTT/jitter/loss on Windows LAN, Android Wi-Fi and Android mobile data;
- variable high/low jitter, isolated drop, burst loss, duplicate/reorder where transport permits;
- one Client input blackhole while heartbeat survives;
- downlink frame gap and bounded catch-up;
- short/long disconnect, Wi-Fi↔mobile switch, Android background/foreground;
- Client process crash/restart and snapshot/history recovery;
- 1/2/8/20 human mixes and Windows two-local-player ownership;
- long-running room/history/sequence retention and memory budget;
- Server tick overload separated from Client network faults.

Every run preserves packet/sequence/ACK/deadline/authority/checksum/recovery witnesses without logging sensitive payloads at production information level.

## 5. Decisions and evidence sources

- GP-01～GP-08 gameplay decisions are single-sourced in [`../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md`](../../../../../NTSD_Server/docs/ai/DECISIONS/PENDING-ONLINE-GAMEPLAY-POLICY-CONFIRMATIONS-001.md).
- User already specified mainland players, Android APK, Windows EXE, initial local public Server intent and target latency expectations; exact authorized test environment remains external-state dependent.
- No public-source article overrides NTSD formal authority/history rules.

## 6. Solution/package inventory

No S7 implementation/verification package exists. Future packages should separate:

1. authorized-node deployment fixture;
2. Windows/Android connection smoke;
3. deterministic public weak-network matrix;
4. background/network-switch/restart recovery;
5. long-session retention/performance soak;
6. product-result validation for PvP/PvE lifecycle.

## 7. Boundaries and forbidden shortcuts

- No Gateway/Auth/Matchmaker/Room Allocator implementation; those are S8.
- No unapproved IP scan, login, firewall or cloud mutation.
- No changing battle rules, tick, history or checksum to make public tests pass.
- No calling one successful ping/Editor run public weak-network verification.
- No hiding a slow Server tick as a Client network fault.
- No unbounded packet logs containing tokens, IP inventories or player privacy.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Windows↔Server | real authorized endpoint battle session | Not started |
| Android↔Server | Wi-Fi and mobile data session | Not started |
| Cross-platform room | same protocol/fingerprint/build contract | Not started |
| Variable network | P50/P95/P99 RTT/jitter/loss and gameplay witness | Not started |
| One slow Client | healthy players continue; affected slot follows policy | Not started |
| Background/switch | recovery, no old-world continuation | Not started |
| Long disconnect | PvP/PvE confirmed result and barrier | Not started |
| Long session | bounded memory/history/sequence and no GC/tick spikes | Not started |
| 20 humans | 30 Hz Server and network budget | Not started |

## 9. Failure disposition and return rule

- Protocol/ready/gap/catch-up defect returns to S2.
- Snapshot/recovery/checksum defect returns to S3.
- Host/transport/MTU defect returns to S5/S6.
- Product behavior mismatch returns to the applicable GP Decision before code.

## 10. Exact exit gate

S7 becomes `VERIFIED` only when authorized real Windows/Android/public-network matrices show that normal, variable, slow, disconnected, backgrounded and recovering Clients follow the frozen policy; healthy Clients never wait indefinitely; formal checksum/history remains identical; and long-session performance stays within budget.

## 11. Handoff to S8

Hand off measured node/network/capacity/recovery metrics, endpoint health contract, supported builds/platforms, admission limits and proven per-room resource envelope.

## 12. Current blockers and next lawful action

- S6 is not started/verified.
- GP decisions and real transport are incomplete.
- No endpoint operation is authorized by this documentation package.

## 13. Revision history

- 2026-08-29: dossier created; status remains `NOT_STARTED`.
