# Q08 knockout event producer coverage audit

## 2026-09-27 current-state correction

Status: `FIVE_FORMAL_CALL_SITES_MAPPED / PRODUCER_SUBGATES_SCOPED_PASS / Q08_OPEN`. The original `STATIC_GAP_CONFIRMED` table below is the **2026-09-22 pre-implementation snapshot**, not the current production backlog. The formal root EXE SHA-256 was rechecked as `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; `battle_world.cpp` and `game_session.cpp` are in the paired playable build closure. A current source search still finds five `BattleWorld28::record_native_knockout` call sites: state-12/18 environmental floor contact (1898), negative-environment recovery (2262), held CPoint injury (6133), and unarmored/selected-armor standard hits (6688/7123). The current Unity callers are respectively `LF2Entity`, `BattleNegativeEnvironmentRecoveryWriter`, `BattleCpointWriter`, and the shared `BattleDamageWriter` standard-hit helper. All four Unity producer families now call `SimulationWorld.RecordNativeKnockout` or its standard-hit wrapper; each producer owns its single credited KO increment before the shared event append. The shared writer itself does not increment KO count, including when the source slot is absent.

The current evidence is deliberately narrower than full Q08 completion. The three nonstandard producer families have original-Editor focused 7/7 (`NTSD28-Q08-NONSTANDARD-KNOCKOUT-EVENT-PRODUCERS-001`), and each has a separate paired-playable Session versus original-Editor complete-tick positive/negative gate: [`STATE12-KO-FULL-TICK-001`](../NTSD28-Q08-STATE12-KO-FULL-TICK-001/ACCEPTANCE.md), [`NEGATIVE-ENV-KO-FULL-TICK-001`](../NTSD28-Q08-NEGATIVE-ENV-KO-FULL-TICK-001/ACCEPTANCE.md), and [`HELD-CPOINT-KO-FULL-TICK-001`](../NTSD28-Q08-HELD-CPOINT-KO-FULL-TICK-001/ACCEPTANCE.md). The standard-hit family has a physical Naruto J natural-KO original-Battle-Scene witness across direct rematch, plus existing focused standard/reduced producer checks, but this topology audit does not turn those selected cases into an all-combination certificate. A natural KO event also survived through configured `time+70` and expired at `time+71` in both Battle Scene Worlds ([lifetime Play](../NTSD28-Q08-NATURAL-KO-FEED-LIFETIME-PLAY-001/ACCEPTANCE.md)). Q09 visible rows, Q10 sound, any untested standard-hit variant, and the rest of Q08 mode/result/event exit remain open.

Disposition: do not re-implement the three historical “Missing append” rows or repeat their already passed producer cases merely to clear this stale table. Reopen a producer only on a new reachable caller, a first difference in its declared fields/order/count, or a shared-writer change. The next independent BATCH-04 work remains the formal-reachable result/event and project-mode battle conditions documented in the alignment plan. This correction is a current-source and existing-evidence reconciliation; it adds no Editor/EXE runtime result and changes no production script, DAT, asset, Scene or nonbattle behavior.

Date: 2026-09-22. Status: `STATIC_GAP_CONFIRMED`. This is a read-only source audit; no new Unity runtime or formal-EXE trace was run.

## Finding

The formal playable `BattleWorld28::record_native_knockout` has five source call sites, not only the two standard-hit branches covered by `NTSD28-Q08-NATIVE-KNOCKOUT-EVENT-STATE-001`. The helper increments the credited entity's `knockout_count_358` and appends one event containing the current sequence, victim/source/credit slots, source type (if resolvable), and the source's four-owner slot (if resolvable). Its tail prune removes expired records from the newest end only.

| Formal `battle_world.cpp` call | Trigger and source slot | Current Unity counterpart | Current event coverage |
| --- | --- | --- | --- |
| 1898 | State 12/18 effective floor-contact environment damage; source is victim `environment_source_slot_160`, or victim slot if negative | `LF2Entity.ApplyCurrentDatType0State1218EnvironmentDamage` at 5685; existing lethal count at 5730-5735 | Missing append |
| 2262 | Negative environment recovery damage; source is `impact_source_slot_164`, or invalid maximum slot if negative | `BattleNegativeEnvironmentRecoveryWriter.Apply` at 23; existing lethal count at 46-52 | Missing append |
| 6133 | Held CPoint injury; source and credit are the resolved catcher owner (or local character catcher) | `BattleCpointWriter.ApplyHeldInjury` at 293; existing lethal count at 329-337 | Missing append |
| 6688 | Unarmored standard hit, pre-HP subtraction | `BattleDamageWriter.ApplyNativeStandardHitKnockout` | Appended by current Q08 package; runtime pending |
| 7123 | Selected-armor standard hit, pre-HP subtraction | Same Unity helper | Appended by current Q08 package; runtime pending |

The three missing Unity paths already increment their own KO count. Their event append must occur beside that increment before HP subtraction, without a second increment. The event writer must accept a source slot that does not resolve to an entity: formal negative-environment impact uses the maximum slot sentinel, still increments a valid credit and appends an event with absent source type/four-owner. Reusing the standard-hit helper by fabricating a physical attacker would be incorrect.

## Next bounded change and acceptance

Create a separate pre-edit Task Contract and Change Record for these three existing producer paths plus the smallest shared `SimulationWorld` event writer/API change. Preserve each existing lethal gate, count, damage, owner/credit and pass timing. Add focused source/Unity cases for source fallback, missing impact source, catcher owner fallback, and no-event ineligible gates; compare event order, fields, one count increment and pre-HP timing with formal same-seed output. Reuse the existing Q08 list/snapshot/checksum/prune state. The current Q08 standard-hit package remains `CODE_WRITTEN`, not `VERIFIED`; original Editor compile, NUnit, SelfCheck, Play, and full-tick trace are still pending. Q09 presentation and Q10 audio remain downstream.

This audit does not authorize Scene, Prefab, nonbattle UI, resource deletion or another Unity project.
