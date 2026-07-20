# Audit 5 Batch 1 Architect Review (2026-07-17)

## Scope and authority

- Sole authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`.
- Reviewed GameTick/Physics batch: `GT-01`, `GT-02`, `PH-03`, `PH-04`, `PH-05`, `PH-06`.
- Reviewed Hit batch: `C-07`, `C-09` through `C-14`, `C-16`, `C-17`.
- Read-only review of the current Unity production code and focused `BattleRuntimeSelfCheck` coverage. No C++, disassembly, pseudocode, or historical implementation was used.
- A fresh full self-check PASS is accepted as evidence that the existing assertions execute, but it cannot close cases omitted by those assertions.

## Findings (severity order)

### P1 - PH-04 is still divergent for real `LF2Character` instances

Authority `Physics.cs:387-389` subtracts landing damage directly from both `Hp` and `HpMax`, and the state-13 high-speed landing branch at `Physics.cs:242-250` subtracts HP without clamping. Negative values remain observable until later passes.

The shared/current-DAT shell was fixed in `LF2Entity.ApplySharedCharacterDatLandingWeaponCountDamage` and its state-13 path, but the production real-character route still clamps:

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDamageStateResolver.cs:243-246` clamps state-13 landing HP to zero.
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDamageStateResolver.cs:269-274` clamps state-12/18 landing HP and HPBound to zero.

The focused PH-04 checks at `BattleRuntimeSelfCheck.cs:7143-7187` instantiate only `SelfCheckCharacterDatShell`; they never execute `LF2CharacterDamageStateResolver`. This is a production-path omission, not merely a missing edge assertion. PH-04 therefore fails architect verification.

### P1 - C-12 is still divergent in the real-character airborne reaction branch

Authority `HitResolve.cs:378-404` uses `victim.YInt < 0` to promote heavy/medium/light reactions while airborne. The shared Character-DAT resolver now uses `GetRuntimeYInt() < 0`, but the actual `LF2CharacterHitResolver.HitFall` still uses `_character.PS.vy < 0` at lines 612, 623, and 633.

These predicates are not equivalent. A character above ground while descending (`YInt < 0`, `Vy >= 0`) must take the authority airborne reaction; Unity does not. A grounded/crossing character with negative `Vy` can take the Unity airborne reaction when authority does not.

`CheckStandardCharacterDamageAlignmentContracts` uses a grounded fixture and compares actual/shared snapshots only for that one state. It cannot detect this branch divergence. C-12 therefore fails architect verification.

### P1 - C-12 standard hit still clears `Attacking` where authority preserves it

Authority standard Character damage assigns reaction frame IDs directly (`HitResolve.cs:381`, `391`, `401`, `425-428`) and does not clear `victim.Attacking` in this branch.

Both Unity resolvers select reaction frames through `ImmediateFrame`, which clears `AttackingCounter` in `LF2Entity.cs:892-900` / `LF2LivingObject.cs:491-496`. In addition, both hit resolvers explicitly clear `AttackingCounter` on non-knockdown accepted hits (`LF2CharacterHitResolver.cs:454-456`, `LF2CharacterDatHitResolver.cs:866-867`). Thus both knockdown and non-knockdown standard reactions can change an extra runtime field absent from the authority sequence.

The standard-damage snapshot in `BattleRuntimeSelfCheck.cs:3890-3958` does not capture `AttackingCounter`, so actual/shared equality can make this shared error look correct. This belongs to the C-12 reaction contract and prevents that ID from passing.

## Per-ID verdict

| ID | Verdict | Review evidence |
|---|---|---|
| GT-01 | PASS (static + focused self-check) | `NeedClearInput` is consumed after cooldown/input/OID maintenance, character-DAT current/previous/runtime input is reset, and the whole remaining tick returns. Test covers real character, shared Character-DAT shell, non-character exclusion, and no frame advance. |
| GT-02 | PASS (static + focused self-check) | `SimulationWorld.SerialTickAll:348-359` clears current action/directional keys immediately before each active slot's `SimTransit`/`SimTU`, preserving previous keys. |
| PH-03 | PASS (static + focused self-check) | Weapon extra-X factor and character landing thirds are double; checks exercise high-precision current-DAT paths. No target-ID regression found. |
| PH-04 | **FAIL** | Shared shell fixed, real `LF2CharacterDamageStateResolver` still clamps negative HP/HPBound. |
| PH-05 | PASS (static + focused self-check) | oid999 default landing is guarded by a real downward ground crossing; exact-ground fixture remains frame 0. |
| PH-06 | PASS (static + focused self-check) | Current-DAT landing branches no longer overwrite `Runtime.WeaponState`; matrix preserves sentinel values for all reviewed weapon classes. |
| C-07 | PASS (static + focused self-check) | kind4 with non-positive WeaponCount is ignored; positive WeaponCount clones/converts the candidate without mutating source DAT and applies the authority direction flip. |
| C-09 | PASS (static + focused self-check) | Heavy-weapon attacker no longer halves injury/fall/dvx/dvy in standard Character damage. |
| C-10 | PASS (static + focused self-check) | Standard injury is raw integer injury, HPBound uses integer `/3`, and PP is unchanged for both actual/shared fixtures. |
| C-11 | PASS (static + focused self-check) | Kill/combo/world kill/world damage fields follow holder-copy and victim-stat indices for covered standard hits. |
| C-12 | **FAIL** | Actual path still uses `Vy` instead of `YInt`; both paths clear an extra `Attacking` field. Existing snapshots omit both distinguishing cases. |
| C-13 | PASS for final knockback-X/order/frame selection | state2000/effect22/effect23 resolution occurs before final 180/186 selection. This does not waive the C-12 frame-write side effect above. |
| C-14 | PASS (static + focused self-check) | Both paths clamp the vertical result to positive `12` with the authority cast boundary. |
| C-16 | PASS (static + focused self-check) | Negative-link holder receives the attacker's final FrameDelay, not the victim delay. |
| C-17 | PASS (static + focused self-check) | Entity residual `AttackExempt` and the separate Unity world-arest mapping (`attacker.ItrRest.Arest`) are both written; vrest remains separate. |

## Verification conclusion

- GameTick/Physics batch: **FAIL overall** because PH-04 remains production-divergent.
- Hit batch: **FAIL overall** because C-12 remains production-divergent.
- Existing fresh compile/full-self-check evidence is useful but insufficient: the passing assertions omit the real-character PH-04 route, the `YInt` versus `Vy` C-12 boundary, and standard-hit `Attacking` preservation.
- Even after these findings are fixed, Play Mode and same-seed dual-end trace remain required. The currently reviewed focused checks are not a Play Mode or trace certificate.

## Required focused coverage before re-review

1. Run PH-04 with a real `LF2Character` for state12/18 WeaponCount landing and state13 high-speed landing; assert negative HP/HPBound exactly as authority.
2. Run C-12 actual and shared targets in both discriminating states: `YInt < 0, Vy >= 0` and `YInt >= 0, Vy < 0`.
3. Seed nonzero `AttackingCounter` before standard light, heavy, and knockdown hits; assert the authority field remains unchanged unless a separate authority branch explicitly clears it.
4. Re-run fresh Unity compile, full self-check, a target Play Mode hit sequence, and the same-seed trace comparator.
