# NTSD Server Lockstep Stage Dossiers

> Status: `ACTIVE_INDEX / PHASE_STATUS_SOURCE_LINKED`
> Last reconciled: 2026-08-29
> Governance: `GOVERNANCE-S0-S9-STAGE-DOSSIERS-001`

## 1. Purpose

This directory is the stage-by-stage working set for the NTSD S0～S9 Server roadmap. It supports one-stage-at-a-time review without turning chats, package Records, or a derived queue into a second roadmap.

Document hierarchy:

1. [`../server-lockstep-s0-s9-design.md`](../server-lockstep-s0-s9-design.md) — global architecture, invariants, phase order, and cross-stage return rules.
2. [`../server-lockstep-s0-s9-progress.md`](../server-lockstep-s0-s9-progress.md) — total ledger and current evidence/status projection.
3. This index and the ten stage dossiers — detailed objective, player-visible result, data/order, decisions, packages, boundaries, tests, exit gate, and next-stage handoff.
4. [`../../../../../NTSD_Server/docs/ai/AUDITS/`](../../../../../NTSD_Server/docs/ai/AUDITS/) — observed evidence and unknown boundaries.
5. [`../../../../../NTSD_Server/docs/ai/DECISIONS/`](../../../../../NTSD_Server/docs/ai/DECISIONS/) — unique user/product decision sources.
6. [`../../../../../NTSD_Server/docs/ai/TASKS/`](../../../../../NTSD_Server/docs/ai/TASKS/) — one bounded implementation package scope.
7. [`../../../../../NTSD_Server/docs/ai/CHANGE-RECORDS/`](../../../../../NTSD_Server/docs/ai/CHANGE-RECORDS/) — actual changed files, verification, risk, and rollback.

If a dossier conflicts with the global design or total ledger, stop and open a governance correction. Do not silently choose whichever text permits more implementation.

## 2. Total ledger

| Stage | Current status — unchanged by dossier creation | Dossier | Current hard gate |
|---|---|---|---|
| S0 | `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED` | [S0](S0-formal-authority-baseline.md) | Formal same-Kernel multi-world ten-domain/first-difference proof remains incomplete; Client currently frozen |
| S1 | `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS` | [S1](S1-authority-input-protocol.md) | S0 `VERIFIED`, formal Kernel/tick mapping, Client capture/wire integration |
| S2 | `FORMAL_NOT_STARTED / SERVER_PREIMPLEMENTATION_EXISTS` | [S2](S2-weak-network-frame-delivery.md) | S1 `VERIFIED`, GP product/timing decisions, real Client consumption and weak-network matrix |
| S3 | `NOT_STARTED` | [S3](S3-snapshot-history-recovery.md) | S2 `VERIFIED`, formal snapshot schema/retention/recovery contract |
| S4 | `NOT_STARTED` | [S4](S4-presentation-prediction-decision.md) | S3 `VERIFIED`; prediction remains a decision, not a required feature |
| S5 | `NOT_STARTED` | [S5](S5-shared-kernel-independent-host.md) | S4 `VERIFIED`, shared formal Kernel, atomic commit/fault/isolation contract |
| S6 | `NOT_STARTED` | [S6](S6-real-transport.md) | S5 `VERIFIED` and explicitly authorized endpoint/security environment |
| S7 | `NOT_STARTED` | [S7](S7-public-weak-network-runtime.md) | S6 `VERIFIED` and authorized public/mobile test matrix |
| S8 | `NOT_STARTED` | [S8](S8-control-plane-multi-room.md) | S7 `VERIFIED`, product/security/resource authorization |
| S9 | `NOT_STARTED` | [S9](S9-release-capacity-operations.md) | S0～S8 fresh evidence and release environment authorization |

## 3. One-stage-at-a-time workflow

```text
Open total ledger
    ↓
Open current stage dossier
    ↓
Resolve exactly one linked Decision or evidence gap
    ↓
Create one bounded Task Contract
    ↓
Create one Change Record before governed edits
    ↓
Test first, implement, verify
    ↓
Write evidence back to dossier and total ledger
    ↓
Mark phase VERIFIED only when every dossier exit criterion passes
```

Package `FOCUSED_TEST_PASS` is not phase `VERIFIED`. A later-stage Server-only package may exist as preimplementation, but it cannot erase an earlier formal phase gate.

## 4. Single-source rules

- **Capability-package granularity (2026-08-31, `GOVERNANCE-S0-S9-CAPABILITY-PACKAGE-CONSOLIDATION-001`):** stage work advances through roughly 3～5 observable-capability packages per phase (S0: content closure → content-model integration → formal kernel assembly → multiworld exit witness). Field/Frame/OID/path-level details live inside a capability package as acceptance matrices or frozen evidence, not as long-term queue rows; historical packages and evidence remain prerequisite evidence. Internal checkpoint pass ≠ capability close ≠ phase `VERIFIED`.
- A user decision is recorded once in `DECISIONS/`; dossiers link its status and impact.
- An external/C++ observation is recorded once in `AUDITS/`; dossiers do not promote inference to fact.
- Task Contracts define planned package scope; Change Records define actual mutation/evidence.
- The total ledger records current status and navigation, not duplicate implementation narratives.
- Historical documents are preserved; corrections append supersede/clarification instead of deleting old facts.
