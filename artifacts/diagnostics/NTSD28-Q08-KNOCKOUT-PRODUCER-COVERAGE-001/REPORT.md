# Q08 knockout event producer coverage audit

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
