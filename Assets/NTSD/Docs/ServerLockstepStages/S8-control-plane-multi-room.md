# S8 — Control Plane, Multi-Room, Capacity and Region Allocation

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Product/security/resource authorization: `PENDING`

## 1. Objective

Build the control plane that authenticates or identifies players as approved by product scope, matches compatible players, allocates one room to one healthy Battle Server, enforces capacity/region/admission policy and persists non-tick lifecycle facts without entering BattleKernel authority.

## 2. Player-visible result

- Compatible Android/Windows players can form or match into one room.
- A room uses one protocol/rule/catalog/stage/build contract and one authority Server.
- New rooms avoid unhealthy/full nodes.
- Failure of a node stops new allocation and produces explicit room outcome/recovery behavior.
- Matchmaking considers mainland latency/region and party constraints without splitting one BattleWorld across regions.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S7 `VERIFIED`.
- S7 hands off endpoint health, supported builds, per-room resource envelope, network/region measurements and recovery metrics.
- Product/security decisions must define identity requirements, tokens, party/match rules, data retention and operational ownership.

## 4. Component and data contracts

```text
Gateway / connection admission
    ↓
Auth or approved no-account identity boundary
    ↓
Party / Matchmaker
    ↓
Room Allocator
    ↓
Battle Server node admission
    ↓
StartBarrier roster/slot ownership
```

Persistent control-plane facts may include player/session/party/match/room/node/outcome/migration records. Running BattleWorld, per-tick inputs, ACK/Jitter, history/snapshot hot rings, AI decisions and battle results remain in the Battle Server runtime, not per-tick SQL.

## 5. Decisions and evidence sources

- `S-NET-006` nationwide/region/party matching policy remains pending.
- User currently says no account is required; identity/security/reconnect token behavior still needs a versioned contract before implementation.
- `S-DATA-001/002` database/cache/object-store/message-queue decisions remain pending evaluation.
- One room binds to one authority region/node; multi-region distributes different rooms only.

## 6. Solution/package inventory

No S8 implementation package exists. Future bounded packages should separately own:

- identity/token and security model;
- node registration/health/capacity;
- room allocation/admission;
- party/match compatibility and region selection;
- PostgreSQL schema/migrations/outbox if approved;
- multi-room load/isolation;
- node-failure and new-room redistribution;
- operational/admin authorization.

## 7. Boundaries and forbidden shortcuts

- Control plane cannot modify battle inputs, HP, hits, RNG, locked frames or checksums.
- Do not query a database every tick.
- Do not share one BattleWorld across regions/nodes.
- Do not invent an account system contrary to the approved no-account product scope.
- Do not store secrets/tokens in source or logs.
- Do not publicly release before S9.
- Do not claim hot room migration without snapshot/transport/node-failure evidence.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| Identity/security | token/replay/expiry/authorization contract | Not started |
| Compatibility | protocol/rule/catalog/stage/build admission | Not started |
| Match/party | deterministic product rules and fairness | Not started |
| Region | latency/party/capacity selection | Not started |
| Node health | liveness/readiness/admission/drain | Bootstrap facts exist; control plane absent |
| Multi-room | isolation, capacity and fault containment | Not started |
| Persistence | migrations, idempotency, backup/restore | Not started |
| Node failure | stop allocation and explicit room handling | Not started |
| Security/load | rate limit, abuse, secrets and capacity tests | Not started |

## 9. Failure disposition and return rule

- Battle/runtime/network correctness returns to S0～S7.
- Identity/matching/allocation/persistence/capacity defect remains S8.
- A control-plane failure must not rewrite running room truth; isolate admission and preserve audit evidence.

## 10. Exact exit gate

S8 becomes `VERIFIED` only when compatible players are securely admitted and allocated to healthy nodes, multi-room capacity/isolation and node-failure matrices pass, approved persistence is migration/restore safe, region policy is measured, and the control plane cannot mutate BattleWorld truth.

## 11. Handoff to S9

Hand off release candidate topology, identity/match/allocation policy, node/room capacity limits, schema/migration version, dashboards/alerts, failure drills and supported platform/region matrix.

## 12. Current blockers and next lawful action

- S7 is not verified.
- Product identity/match/region/security/data decisions and resources are not approved.
- No S8 source package is READY.

## 13. Revision history

- 2026-08-29: dossier created; status remains `NOT_STARTED`.
