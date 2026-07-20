# GameTick/Physics + Weapon Hit Final Review (2026-07-17)

## Scope and authority

- Sole authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`.
- A) Re-reviewed the previous `GT-04`, `GT-07`, `PH-02` blockers against the current production code and focused self-checks.
- B) Re-reviewed Weapon `C-23..C-29` against `HitResolve.ApplyObjectHurtTail`, `ApplyKind0VictimObjectTail`, `ApplyStandardDamageKnockbackX`, sound recording, and encoded-effect dispatch.
- No C++, disassembly, pseudocode, or historical implementation was used. No production code was edited by this review.

## Findings (severity order)

### P1 - C-26/C-27: weapon vertical clamp still uses floating Y instead of authority integer Y

Authority `HitResolve.cs:924-925` clamps only when `(int)(victim.KnockbackVy + victim.YInt) > 0`. The current `LF2Weapon.ApplyHitEffects` uses `if (KnockbackVy + Runtime.Y > 0f)`, at `LF2Weapon.cs:538`.

The two predicates differ for fractional positions/knockback, e.g. `YInt == 0`, `Y = 0.25`, `KnockbackVy = 0.1`: authority keeps the value while Unity writes `12`. The existing C26 assertion (`fall=40`, initial KnockbackVy=0) only proves that low-fall IronBall does not add authored/default vertical impulse; it does not exercise the integer-boundary clamp. This remains a production tail divergence affecting C26/C27.

### P1 - C-27: attacker state3000 tail omits authority frame10 Vz write

Authority `ApplyState3000Tail` at `HitResolve.cs:1585-1615` sets attacker frame 10, `Attacking=0`, `Vx=0`, and copies `frame10.Dvz` into `Vz`.

`LF2Weapon.ApplyAttackerResponse` at `LF2Weapon.cs:663-669` handles the corresponding projectile/state3000 response by switching to frame 10, clearing `AttackingCounter`, and setting `Runtime.Vx=0`, but never assigns `Runtime.Vz` from frame 10 data. A weapon victim hit by a state3000 attacker therefore leaves stale Z velocity. The C27 focused matrix uses standing attackers only and cannot detect it.

### P2 - Current-DAT type3 weapon shell is not a complete type3 object tail

The ordinary `LF2Weapon.ApplyHitEffects` branches by `GetCurrentDataObjectTypeForSimulation`, which is correct for types 1/2/4/6. If a weapon CLR shell currently carries DAT type3, it executes the generic weapon path but `ApplyKind0VictimObjectTail` has no type3 branch (`LF2Weapon.cs:584-629`). Authority always follows object hurt tail and then `ApplyKind0Type3Tail` for current DAT type3 (`HitResolve.cs:1236-1241`).

This is adjacent to C-29 (encoded effect is intentionally not consumed for ordinary weapons) and the broader current-DAT contract. The existing current-DAT test only transforms a CLR throw shell to current type1, not type3. It must remain an explicit residual until the type3 current-DAT path is either routed to the SpecialAttack resolver or tested against the authority type3 tail.

## A) GameTick/Physics blocker re-check

### GT-04: PASS

`SimulationWorld.RunEarlyState501Specials` now selects the child frame with `child.Runtime.YInt < 0 ? 212 : 0` (`SimulationWorld.Passes.partial.cs:654-658`), matching authority `GameTick.cs:1088`. The focused fixture places the child at `Y=-0.5`, verifies `YInt==0`, frame 0, source-slot `KillCount` matching, and identity publication. It also rejects the stable-ID-only child. No remaining GT-04 blocker found.

### GT-07: PASS at the confirmed-difference level

The base cleanup now accepts every current-DAT non-character type (`LF2Entity.cs:3582-3593`), exactly matching authority `ObjTypeRules.IsWeaponDat(datObjType != 0)` plus `GameTick.cs:1598-1603`. The real `LF2WeaponBase` override delegates to the base and only suppresses the old broken-fragment path (`LF2WeaponBase.cs:534-545`); it no longer adds holder/destroyability gates or fragments. The expanded fixture covers current DAT types 1..6, real CLR weapon instances, held depleted weapons without link clearing, and current character exclusion. The previous self-check assertion requiring five fragments has been replaced with the authority expectation of no queued fragments. No remaining GT-07 blocker found.

### PH-02: PASS for the reviewed boundary

`CharacterMechanics.Step` now marks `caughtGroundResolve` before clamping (`CharacterMechanics.cs:178-187`), preserving positive Y and skipping `landed` for cpoint kind2. The real-character and shared current-character-DAT fixtures cover positive Y after vertical movement. The remaining exact `-0.0001`/`+0.0001` and weapon old-Vy gates are also covered. No remaining PH-02 blocker found in this review.

## A) 21-item GameTick/Physics verdict

| ID group | Verdict |
|---|---|
| `GT-01..15` | PASS at confirmed-difference/source level after current fixes |
| `PH-01..06` | PASS at confirmed-difference/source level after current fixes |

This is not a full runtime certificate: R-GP risks, DAT manifest parity, Play Mode, and same-seed dual-end trace remain open.

## B) Weapon C-23..C-29 verdict

| ID | Verdict | Review evidence |
|---|---|---|
| C-23 | PASS | `LF2Weapon.Hit` no longer rejects a held victim globally; candidate-side held/link validation remains separate, and the focused held victim fixture reaches the hurt path. |
| C-24 | PASS | No oid201/202 or heavy state2000/2004 rejection remains in the ordinary weapon kind0 path. Current type is read from DAT, and focused light/heavy cases exercise the removed gates. |
| C-25 | PASS for unconverted kind9 | Candidate preparation converts kind9 only for Character or victim state1002/2000; an unconverted kind9 passed directly to `LF2Weapon.Hit` returns without durability, sound, or ordinary tail side effects. |
| C-26 | **FAIL** | Low-fall IronBall correctly skips dvy/default -7, but vertical clamp still uses floating `Runtime.Y` instead of authority `YInt`. The boundary case is untested. |
| C-27 | **FAIL** | Authored victim vrest, type2 holder/self rest, type4/6 attacker self rest, resolved arest, and current-DAT dispatch are fixed and tested; state3000 attacker Vz and integer-Y clamp remain divergent. |
| C-28 | PASS at covered sound-chain level | Type1/2/4 effect cue precedes attacker type3 broken cue and victim hit sound; type6 omits the damage-effect cue. Focused tests verify order/count. |
| C-29 | PASS for ordinary weapon victims | `LF2Weapon.ApplyHitEffects` no longer consumes encoded 5000/6000 effects; focused test verifies PP/frame are unchanged. Current-DAT type3 remains the adjacent residual listed above. |

## Remaining risks and evidence gaps

1. **Weapon boundary/state-tail coverage:** add C26/C27 fixtures for fractional `Runtime.Y` plus nonzero `KnockbackVy`, and attacker state3000 with a frame10 nonzero `dvz`; include `Runtime.Vz` in the snapshot.
2. **Current-DAT type3 path:** run a CLR weapon shell with current DAT type3 through candidate collection and kind0/kind9, and compare `ApplyObjectHurtTail` plus `ApplyKind0Type3Tail` fields, encoded effects, identity, and sound.
3. **Global runtime certificate:** `FrameWaitCounter` mapping, deployed DAT-manifest differences, and dormant/pending multi-tick consumers still need same-seed comparator evidence. No Play Mode test of held weapon, weapon-layer ordering, or Naruto skill chain was performed here.

## Conclusion

- Previous GameTick/Physics blockers: **cleared at confirmed-difference/source level; `GT-01..15` and `PH-01..06` PASS**.
- Weapon batch: **C-23/24/25/28/29 PASS; C-26 and C-27 FAIL** due the two P1 tail differences above.
- Therefore neither the overall battle runtime nor the Weapon batch can receive a full Architect/runtime certificate yet. Fresh compile/full self-check evidence, Play Mode reproduction, and same-seed dual-end trace remain required.
