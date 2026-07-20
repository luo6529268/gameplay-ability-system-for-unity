# GameTick / Physics Final Architect Review (2026-07-17)

## Scope

- Sole authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`.
- Re-reviewed all 21 confirmed differences from `game-tick-physics-audit-20260717.md`: `GT-01..15` and `PH-01..06`.
- Compared current Unity production paths and focused `BattleRuntimeSelfCheck` assertions. No C++, disassembly, pseudocode, or historical implementation was used as evidence.
- No production file was edited by this review.

## Findings (severity order)

### P1 - GT-07 still diverges in non-character late cleanup, and an existing test enforces non-authority behavior

Authority `GameTick.RunLateEntityUpdate` at `GameTick.cs:1598-1603` treats every current-DAT non-character object as weapon data (`ObjTypeRules.IsWeaponDat` is `datObjType != 0`), and when `Unk31C < 0` it performs exactly this sequence: set `Unk31C=0`, queue the configured broken sound, free the slot, return.

The centralized Unity base path is still narrower:

- `LF2Entity.TryRunLatePostOpointCleanupPhase` at `LF2Entity.cs:3582-3601` accepts only type 1/2/4/6. It excludes current-DAT type3 `SpecialAttack` and type5 `Other`, although both satisfy the authority `IsWeaponDat` predicate.
- Real `LF2WeaponBase` instances do not call the centralized base implementation. `LF2WeaponBase.cs:534-561` adds `holder == null`, `IsWeaponDestroyable()`, and `_lateBreakEffectsHandled` gates, creates broken effects, clears holder state, and only then marks pending destroy.
- Authority has no five-fragment generation in this branch. `BattleRuntimeSelfCheck.cs:5559-5565` instead requires five fragments from a real oid100 weapon and describes them as coming from a different baseline. That assertion is contrary to the sole C# authority and can make a regression appear correct.

The GT-07 focused matrix only checks a shared light-weapon DAT and a weapon CLR shell transformed to non-weapon data. It omits shared type3/type5 negative `Unk31C`, a held depleted real weapon, and a real weapon whose subclass gate rejects cleanup. GT-07 fails.

### P1 - PH-02 clamps character Y before the authority cpoint-kind2 early return

Authority `Physics.ApplyGroundResolve` obtains the current frame and returns immediately for `frame.Cpoints[0].Kind == 2` before the `newY <= 0.0001` check and before any ground clamp. Therefore a character with `newY > 0.0001` on a cpoint-kind2 frame retains that positive Y for this tick.

Unity `CharacterMechanics.Step` at `CharacterMechanics.cs:178-184` computes `landed = runtime.Y > 0.0001` and immediately writes `runtime.Y = 0.0`. The cpoint-kind2 exclusion is only applied later in `LF2CharacterDamageStateResolver.HandleLandingEvent:102-106`; it is too late to preserve Y. The same mechanics path is used by real characters and shared current-character DAT shells.

The PH-02 focused checks cover exact epsilon boundaries and weapon old-Vy gates, but no cpoint-kind2 frame. PH-02 fails.

### P1 - GT-04 child transform uses floating Y instead of authority YInt

Authority `RunState501Pass` at `GameTick.cs:1074-1088` selects the child frame with `target.YInt < 0 ? 212 : 0`.

Unity `RunEarlyState501Specials` correctly restricts children by source runtime slot and publishes current-DAT identity, but line 657 selects with `child.PS.y < 0f ? 212 : 0`. These predicates differ for fractional values such as `Y=-0.5`, where C# integer truncation yields `YInt=0` and frame 0 while Unity chooses frame 212.

The GT-04 focused test verifies slot ownership and identity fields but does not place the child at a fractional negative Y boundary. GT-04 fails.

## Per-ID verdict

| ID | Verdict | Evidence and test quality |
|---|---|---|
| GT-01 | PASS | NeedClearInput is consumed after the authority pre-clear phases; current, previous, cooldown/combo/history input is reset only for current character DAT; the rest of the tick returns. Real/shared/non-character cases are covered. |
| GT-02 | PASS | Every active slot clears only current directional/action keys immediately before its serial frame advance; previous keys are preserved. Real/shared/generic slot probes cover the boundary. |
| GT-03 | PASS at source-contract level | Counts all active current-DAT non-characters; preserves parsed data.txt order; special 122/123 RNG precedes mode exclusion; gate/full-slot/candidate/position RNG order matches; strict slot starts at 50; frame 0, position/velocity, oid122 HP rule, default stats, and vrest reset are implemented. Focused tests cover order, count, RNG miss, full slots, special gates, ordinary spawn and stats. Deployed DAT-manifest equality remains a trace prerequisite, not closed by this test. |
| GT-04 | **FAIL** | Slot ownership and identity are fixed, but fractional child Y uses `PS.y` instead of `YInt` for frame 212/0 selection. |
| GT-05 | PASS | Preframe Z dispatch is centralized by current DAT for character, type3 logic-Z, and other non-character bounds, with ZInt synchronization. The cross-shell matrix covers both directions. |
| GT-06 | PASS | Recovery eligibility is centralized by current DAT. Real character, shared character-DAT shell, and reverse character-shell/non-character-DAT cases are covered. |
| GT-07 | **FAIL** | Central base excludes type3/type5; real weapon override adds gates and five-fragment behavior absent from authority. Focused GT-07 coverage misses these branches and another self-check enforces the extra fragments. |
| GT-08 | PASS | Relay is checked before generic frame >=400 release; `GetAllEntities` now filters inactive/dormant/pending objects; owner and active children receive relay, owner resets to frame0 and remains allocated. Real/shared/current-DAT tests cover representative frame groups. |
| GT-09 | PASS | Late entry into state9998 survives the current late pass and is released by next tick's post-frame-advance cleanup. Focused two-tick lifecycle test is not self-referential. |
| GT-10 | PASS | State9995/4000/8000 identity transforms raw-write frame and preserve PN/Attacking/wait state; state8000 adds HitStop 140. Current-DAT identity is refreshed in the same tick. |
| GT-11 | PASS | The production state9996 spawn branch is removed. Focused test seeds Attacking and verifies RNG state/call count and ObjectCount remain unchanged. |
| GT-12 | PASS | Transition calculations and task runtime position/velocity fields remain double through immediate factory creation. Focused test uses non-float-representable values and checks the created production entity. |
| GT-13 | PASS | The extra generic HP/death cleanup tail is absent. Focused tests prove HP-zero non-character and delayed dead state14 entities remain allocated. |
| GT-14 | PASS at source-contract level | Stage spawn reserves the first free strict slot in 20..399 before RNG, rejects slot collision without fallback, preserves ordinary dynamic slot 50, consumes no position RNG when full, and applies stage identity/stats contract. Character spawn and full-slot cases are covered; Play/trace must still exercise production non-character factory spawn. |
| GT-15 | PASS | Factor scans only slots 0..19, filters `IsActiveForCurrentPass`, requires current character DAT, and weights oid51/52 as 2/3. Test includes dormant, pending-destroy, and slot20 exclusions. |
| PH-01 | PASS | Polygon walkability callback is no longer consumed by mechanics. Only block flags suppress axis movement, and focused tests use a rejecting callback to prove it cannot roll back movement. |
| PH-02 | **FAIL** | Double epsilon and weapon old-Vy gates are largely fixed, but cpoint-kind2 character Y is clamped before its authority early return. No focused test covers it. |
| PH-03 | PASS | `0.2` and `0.3333333333333333` remain double on native/shared character and weapon paths. Precision tests use values that distinguish float contamination. |
| PH-04 | PASS | Shared state12/18/state13 and real LF2Character state12/18/state13 cases preserve authority negative HP/HPBound behavior. The prior actual-path omission is now covered. |
| PH-05 | PASS | oid999 ground resolve requires an actual `newY > 0.0001` crossing; exact-ground Y=0 does not switch to frame101. |
| PH-06 | PASS | Landing branches do not overwrite `Runtime.WeaponState`; the transformed weapon matrix preserves sentinel values across light/heavy/throw/drink paths. |

## Risk status

The three original report risks are not sufficient to block the three confirmed failures above, but they also prevent a full GameTick/Physics certificate after those failures are fixed:

1. **R-GP-01 remains open, partially implemented.** Unity now has `NTSDEntityRuntime.FrameWaitCounter` and the parity snapshot reads it, but production writes are not comprehensively mapped to all authority `FrameWaitCounter` transitions. Most direct frame helpers still synchronize `FrameTransistor.WaitCounter` without maintaining the independent field. Field existence alone does not close the contract.
2. **R-GP-02 remains open.** `CharacterMechanics.Step:173` still gates ground friction on `mass > 0`, while authority friction has no mass predicate. No full deployed/future spec proof excludes mass 0.
3. **R-GP-03 is reduced but remains a trace/audit risk.** `GetAllEntities` and reviewed stage/pass consumers now filter dormant/pending entities, and GT-15 is fixed. Explicit `FindEntityByRuntimeSlotIncludingDormant` consumers still exist for roster, merge, snapshot, and cleanup adaptation. A complete consumer audit plus merge-to-split multi-tick trace is still required before certifying the dormant model globally.

## Conclusion

- Confirmed-difference closure: **18 PASS / 3 FAIL**.
- Blocking IDs: **GT-04, GT-07, PH-02**.
- Therefore `game-tick-physics-audit-20260717.md` does **not** receive Architect PASS in the current code state.
- A passing compile/full self-check cannot override these findings because the focused suite omits the distinguishing GT-04 and PH-02 inputs and contains a GT-07 assertion that enforces behavior absent from the authority C# source.
- After repair, fresh Unity compile, full self-check, production Play Mode coverage, DAT-manifest-compatible same-seed trace, and first-difference comparator results remain required. T8 default `stage.dat` deployment remains deferred and was not used as a failure here.
