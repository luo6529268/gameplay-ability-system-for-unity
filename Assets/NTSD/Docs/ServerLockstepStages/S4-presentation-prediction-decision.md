# S4 — Presentation and Limited-Prediction Decision Gate

> **NTSD24_AUTHORITY_SUPERSEDED（2026-09-02）：** 本文所称未限定版本的 “C++ release” 形成于旧 NTSD 2.4 权威期，仅作为历史证据；不得据此定义当前战斗规则、pass、timing、slot、RNG、字段、生命周期、表现或“已对齐”状态。任何恢复先读 `docs/ai/CURRENT-AUTHORITY.md`；当前权威是 NTSD 2.8-Logan 正式 EXE 及其对应 playable 源码，旧结论一律 `REBASELINE_REQUIRED`。

> Current status: `NOT_STARTED`
> Formal phase status: `NOT_VERIFIED`
> Default direction: `CONFIRMED_ONLY UNTIL EVIDENCE`
> Status preserved from the total ledger on 2026-08-29.

## 1. Objective

Decide from measured evidence whether NTSD needs any limited local-player prediction beyond confirmed authority frames and presentation interpolation. Prediction is optional; rejecting it is a valid successful S4 result.

## 2. Player-visible result

- ConfirmedOnly: actions reflect authority timing consistently, with no rollback pops or duplicated effects.
- Presentation-only feedback: button/UI/animation anticipation may respond without claiming battle result.
- Limited prediction, only if approved: local movement/input feel may improve, but wrong predictions must reconcile without changing authoritative damage, collision, RNG or remote results.

## 3. Entry prerequisites and upstream handoff

- Formal entry gate remains S3 `VERIFIED`.
- S3 must provide reliable snapshot/history recovery, confirmed event cursor and bounded replay.
- GP-01 interpolation is not a license for authority prediction.

## 4. Candidate contracts and order

Candidates:

1. `ConfirmedOnly` — execute only continuous authority frames.
2. Immediate local presentation feedback — UI/animation only, no BattleWorld mutation.
3. Limited local-player movement/input prediction — versioned, bounded and reversible; no remote result prediction.

Any permitted prediction order:

```text
Capture local intent
    ↓
Optional presentation/predicted local state with explicit epoch
    ↓
Receive immutable authority frame
    ↓
Match or reconcile against authority
    ↓
Emit confirmed events once by cursor
```

## 5. Decisions and evidence sources

- `S-NET-003` remains pending measurement in the total ledger.
- Mature public frame-sync evidence records that large delayed playback hurts hand feel and full local ahead/correction can look floaty; it does not prove the correct NTSD choice: [`../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md`](../../../../../NTSD_Server/docs/ai/AUDITS/KING-OF-GLORY-PUBLIC-FRAME-SYNC-EVIDENCE-001.md).
- C++ release rules remain authoritative for input windows, hits, RNG and event order.

## 6. Solution/package inventory

No S4 implementation package exists. Required future work is an A/B evidence package comparing ConfirmedOnly, presentation feedback and any narrowly proposed prediction under the same fixed journals and network scripts.

## 7. Boundaries and forbidden shortcuts

- No global rollback/GGPO requirement.
- No prediction of remote-player damage, hit, HP, weapon, opoint, AI or RNG results.
- No rewrite of locked authority frames.
- No Transform/presentation state becoming logic truth.
- No duplicate sound/effect/stat event after reconciliation.
- No prediction implementation before S3 recovery evidence.

## 8. Acceptance and test matrix

| Layer | Required evidence | Current result |
|---|---|---|
| ConfirmedOnly baseline | measured latency/feel under fixed scripts | Not started |
| Presentation-only feedback | no logic writeback, no false confirmed event | Not started |
| Prediction benefit | statistically meaningful latency/UX gain | Not started |
| Wrong prediction | bounded correction and event dedupe | Not started |
| Restore/replay | snapshot/history remains exact | Blocked on S3 |
| C++ parity | no rule/order/RNG divergence | Not started |
| Android/Windows | input, visual and performance evidence | Not started |

## 9. Failure disposition and return rule

Any authority rewrite, RNG/world divergence, duplicated confirmed event, severe visual pull or insufficient measured benefit closes the prediction branch and returns S4 to `ConfirmedOnly`.

## 10. Exact exit gate

S4 succeeds when a documented, measured decision selects either:

- no prediction, retaining ConfirmedOnly plus interpolation; or
- a narrowly versioned prediction scope that passes recovery, checksum, event-cursor, C++ and performance gates.

## 11. Handoff to S5

Hand off an immutable authority/recovery contract and explicit statement of which presentation/prediction state is non-authoritative and excluded from Server Kernel serialization.

## 12. Current blockers and next lawful action

- S3 is not started/verified.
- No S4 source work is authorized.
- Continue treating prediction as deferred, not as a missing mandatory feature.

## 13. Revision history

- 2026-08-29: dossier created; status and ConfirmedOnly default preserved.
