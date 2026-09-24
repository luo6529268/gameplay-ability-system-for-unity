# D-024 coordinate-history contract: design gate

Status: `DESIGN_GATE / IMPLEMENTATION_NOT_STARTED` (2026-09-24). This document scopes the next battle-only implementation work. It does not change the user's full-background camera decision, DAT bytes, stage assets, or nonbattle framework.

2026-09-24 status correction: the heading above is the original design checkpoint, not the current implementation state. `NTSD28-USER-SOURCE-COORDINATE-STATE-001` and `...-JSON-CHECKSUM-001` established the versioned state/snapshot/checksum carrier; birth packages for late OPoint/stage, kind1 random, participant, state18/19, state9996 and weapon fragments have focused tests, and the bounded continuation OID998/action6 effect birth is `CODE_WRITTEN / COMPILE_PENDING`. Gameplay source-rule readers, continuous motion, source-domain boundary flags and ordinary revival average remain open. See `D024-CURRENT-GATE.md` and the individual Task/Change Records; do not re-run the design phase as if no implementation existed.

The later `POSITION-WRITER-OWNERSHIP-GATE.md` adds exact active writer/caller, alias, snapshot and unresolved-boundary ownership. The second confirmed configured-view first difference is fusion eligibility: original Editor formal-row production scan merges at raw gap40 but rejects at current battle gap50. State9996 child relative X/Z birth has since received a focused outlet fix; neither event changes the need for the general history carrier.

2026-09-24 additional reader gate: the focused `hit_Fa=7` correction now reads an addressable inactive slot's retained raw runtime XInt/ZInt when no active target exists. Under D-024, that raw slot is still a separate coordinate-history owner: its source-rule reference values must survive occupant removal, reset/reuse, canonical raw-slot snapshot and checksum. The official OID875 empty-slot test and full SelfCheck pass in the present factor-one fixture; no formal EXE empty-slot direct observation or configured-view retained-slot history test exists. Do not copy a live target's battle coordinate into a raw-slot source-rule reference during release, and do not mark the shared carrier complete from the factor-one self-check.

## Why a scalar fix cannot close the defect

The original Editor produced two registered-World OID219 behavior5 cases with the same current integer target-child X gap `224`:

| History | Current Unity gap | Paired playable source-rule gap | Required raw child Vx | Current Unity Vx |
| --- | ---: | ---: | ---: | ---: |
| Initial spawn gap224 | 224 | 224 | 4 | 4 |
| Initial gap151, then one formal48px target movement | 224 | 199 | 3 | 4 |

The formal `NativeAi28::step_non_character_hit_fa` behavior5 computes integer gap `/50`; the current Unity producer reads only the present `Runtime.XInt` gap. Job `d248af5583144fa4822db084fa43cee3` passed focused 11/11 and asserted both histories, including the moved target's precise-to-integer sync. Identical current inputs require different correct outputs. Any stateless multiplier/divider of the present gap, including a producer-side inverse view factor, fails at least one history.

## Required coordinate domains

1. **Battle position** remains the D-024 actual Unity runtime position. Character and object X/Z travel is increased at the approved motion outputs to match the formal screen fraction in the fixed full-background view. Collision, AI sensing, stage interaction and presentation currently consume this position; changing those consumers requires its own evidence, not an incidental rewrite.
2. **Source-rule reference position** (or a mathematically equivalent history carrier) represents where the same entity would be under the formal raw-pixel motion rules and original absolute birth placement. Source-derived formulas that must retain formal timing can read this domain. It is not a render Transform or a camera offset.
3. Both domains need precise and integer semantics where the formal source uses them. The reference integer conversion must follow the playable source's own truncation/synchronization order before behavior5's integer `/50`; do not round a scaled World gap as a substitute for history.
4. The transform is per axis and view configuration. The known Training sample is X `2048/1333`, projected Z `1152/730`; Y/floor remains unresolved. Default diagnostic World factor1 must collapse to the same position in both domains. Other aspects/maps need separately measured view spans.

The user's approved D-024 exception allows the actual Unity battle position to travel farther per tick, so collision, hit reach and stage-edge contact can intentionally differ from the formal raw-position scene. The source-rule reference cannot simply reuse battle boundary flags: a battle body can reach an unchanged absolute stage edge before its formal raw counterpart would. The implementation must explicitly define and test whether a source-rule reference motion step follows formal virtual bounds while physical collision follows battle bounds. This hybrid choice is a D-024 consequence to document; it must not be hidden in an inverse multiplier or claimed as unrestricted full battle equivalence.

## Atomic writer inventory required before production edits

Every reachable position writer must declare whether it performs raw absolute placement, raw source-relative placement, scaled physical travel, target-relative snap, attachment pose, stage clamp or presentation-only adjustment. The current census is only lexical. The next inventory must trace both Unity caller and paired playable field-write order for at least:

- entity birth, raw stage/wave/drop placement, immediate and late OPoint, weapon fragments, transition effects, clone and revival;
- character/non-character integrated X/Z, frame direct `dx/dz`, weapon identity extra X, type3 precise-Z, linked-platform carriage and any physics fast path;
- state400/401 teleport, cpoint/wpoint held positioning and throw, stage X/Z bounds, respawn and destruction;
- direct assignment paths found by `rg` but not yet classified, including AI/interaction writes.

Each writer must update both domains in the same transaction, or be proved presentation-only. A reference position that is refreshed by dividing the current battle position is invalid after mixed absolute birth and scaled movement. The existing `BattleHeldObjectWriter` and `BattleCpointWriter` use DAT-local frame anchors; these require a pose/renderer test before any coordinate scaling.

## State and replay gates

`NTSDEntityRuntime` currently stores only one `X/Y/Z` precise position plus integer mirrors. `TryCopyCanonicalStateTo` copies them into lockstep snapshots, and `Reset` clears them for reuse. `BattleLockstepChecksumModule.AppendEntityRuntime` hashes the current position. A new history carrier therefore needs explicit birth/reset, canonical snapshot copy/restore, checksum and parity trace/version handling, object-pool reuse, world shutdown, and deterministic seed/tick checks. Do not introduce a temporary unsnapshotted field in the behavior5 producer.

## Smallest valid acceptance path

First write a complete caller/writer ownership table and bounded Task/Change records for the shared coordinate carrier and its writers. Then use the original Editor to prove: static gap151 and224, moved gap224, left/right and at least one non-character mover; same-input same-tick formal/Unity reference-coordinate trace; Battle Scene Play of the representative case; snapshot/restore/replay/checksum and pool reset; no DAT/Scene/camera/nonbattle diff. Run focused tests for newly touched writer categories, broadening only if a cross-pass change reveals a new first difference. Finally repeat at a second view/aspect or explicitly keep that factor outside the completed claim.

This design gate does not by itself close behavior5, D-024 or Q07. The current eleven passing EditMode tests include tests that characterize a known defect.
