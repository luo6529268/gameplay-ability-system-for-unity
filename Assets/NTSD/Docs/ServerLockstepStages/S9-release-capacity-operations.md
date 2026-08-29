# S9 — Release, Capacity, Compatibility and Operations

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Release authorization: `NOT_GRANTED`

## 1. Objective

Decide whether the complete Android/Windows/Server system is safe to release by running fresh deterministic, weak-network, recovery, cross-runtime, security, capacity, upgrade/downgrade and fault-drill matrices. S9 accepts no new feature implementation; failures return to the owning stage.

## 2. Player-visible result

- Published Android APK and Windows EXE connect only to compatible rooms.
- Matches remain deterministic and recover according to confirmed policy.
- Capacity overload produces admission/drain behavior instead of tick collapse.
- Upgrades do not mix incompatible battle rules in one room.
- Operational faults are detected, isolated and reported with player-safe outcomes.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S8 `VERIFIED` and fresh S0～S8 evidence.
- Release environment, distribution, signing, nodes, domains, certificates, secrets and operator ownership require explicit authorization.
- No historical Editor screenshot or one successful local run substitutes for release evidence.

## 4. Release evidence set

Required candidate bundle:

- versioned Protocol/Kernel/Client/Server artifacts and fingerprints;
- Android/Windows build and compatibility matrix;
- fixed deterministic journals, snapshots and recovery fixtures;
- real/public weak-network and long-session reports;
- room/node capacity model and measured budgets;
- database migration/backup/restore evidence where applicable;
- security, secrets, rate-limit and abuse review;
- dashboards, alerts, runbooks and rollback artifacts;
- upgrade/downgrade and old-room compatibility rules;
- failure drill results and unresolved-risk register.

## 5. Decisions and evidence sources

- Same room cannot mix protocol/rule/catalog/stage incompatible versions.
- Old builds may finish only explicitly compatible old rooms and cannot enter new incompatible rooms.
- Android is self-distributed APK; Windows is independent EXE; detailed signing/update/release channel still requires operational decisions.
- GP-01～GP-09 are confirmed; all must still be reflected in executable acceptance tests before release.

## 6. Validation waves

1. deterministic and C++ authority regression;
2. Server/Client build and cross-runtime parity;
3. snapshot/history/reconnect and desync recovery;
4. localhost/LAN/public weak-network matrices;
5. one-slow-client and 20-human performance;
6. multi-room/node capacity and isolation;
7. migration/backup/restore and upgrade/downgrade;
8. security/abuse/secrets review;
9. soak and fault drills;
10. release-candidate sign-off or return-to-stage decision.

## 7. Boundaries and forbidden shortcuts

- No new feature code in S9; return defects to S0～S8.
- No bypassing a failed gate because a release date is near.
- No changing protocol/version/fingerprint in place without supersede/compatibility evidence.
- No using Client state to repair Server authority.
- No publishing secrets, private logs or player data.
- No public release from partial `FOCUSED_TEST_PASS` packages.

## 8. Acceptance and test matrix

| Domain | Required evidence | Current result |
|---|---|---|
| S0 deterministic baseline | fresh formal multi-world proof | Not available |
| S1 protocol/history | frozen formal schema and dispositions | Not verified |
| S2 weak network | real/formal matrix and bounded catch-up | Not verified |
| S3 recovery | snapshot/history/desync/reconnect | Not started |
| S4 prediction decision | confirmed-only or controlled evidence | Not started |
| S5 independent Host | cross-runtime/actor/fault isolation | Not started |
| S6 transport | real equivalent transport/security | Not started |
| S7 public/long session | Android/Windows/public weak network | Not started |
| S8 control plane | identity/match/room/node/capacity/data | Not started |
| Release operations | soak/drill/upgrade/rollback/runbook | Not started |

## 9. Failure disposition and return rule

Every failure is classified by first owner:

- rule/checksum → S0;
- input/frame schema → S1;
- ACK/Jitter/slow Client → S2;
- snapshot/recovery → S3;
- prediction/events → S4;
- Host/actor/fault → S5;
- packet/transport → S6;
- real weak network/long session → S7;
- identity/match/capacity/data → S8.

S9 records and retests; it does not patch around the failure.

## 10. Exact exit gate

S9 and the Server roadmap become `VERIFIED/RELEASED` only when every required S0～S8 stage is verified, all release matrices are fresh and passing, no unresolved blocking risk remains, rollback/runbooks are operational, and the authorized release owner explicitly approves publication.

## 11. Handoff and final release output

Hand off signed artifacts, version/fingerprint matrix, deployment manifests, migrations, dashboards, alerts, runbooks, rollback plan, known limitations, capacity envelope and complete evidence index to operations and future maintenance.

## 12. Current blockers and next lawful action

- S0～S8 are not fully verified.
- No release environment or publication is authorized.
- S9 remains a future acceptance gate only.

## 13. Revision history

- 2026-08-29: dossier created; status remains `NOT_STARTED` and no release authorized.
