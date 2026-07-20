# C# Authority Interaction Ledger (2026-07-18)

## Scope and rules

- Sole authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`.
- This ledger was produced from the authority C# source only; no non-authority implementation or historical project is cited.
- Included: collision collection, hit consume/resolve, cpoint, held weapons/wpoint, opoint/object generation, battle-affecting stage runtime, arest/vrest, combat statistics, and sound-event emission.
- Excluded: deployment of a default `stage.dat` asset. Stage runtime rules remain included.
- ID format: `INT-<partition>-<method>.<branch>`. The method ID is the prefix before the final dot; `.E*` denotes an early return, `.B*` an ordered branch, `.R*` RNG, and `.S*` a state/lifecycle side effect.
- Line numbers refer to the authority source as read on 2026-07-18.

## Tick ownership and call order

| ID | Authority location | Caller -> callee | Ordered contract and observable writes |
|---|---|---|---|
| INT-PASS-001 | `src/BattleCore/Simulation/GameTick.cs:18-137`, `GameTick.Run` | tick driver -> all battle passes | Increments `GameTick`, input phase and frame counters; clears transient sound/pause state; input/cooldowns; frame logic; frame advance; post-frame state; cpoint; held sync; link validation; held step12; snapshots `PrevFrame2`; collects once; consumes Character attackers; random/F8 weapon drops; consumes non-Character attackers; preframe bounds; stage phase/spawn; frame postprocess; late entity update/opoint; mode2 drops; tail/results. `.E1` results state runs results only. `.E2` entry-input clear returns before combat. `.E3` step-wait returns after stage application but before late/tail/results.
| INT-PASS-002 | `src/BattleCore/Interaction/InteractionRuntimePasses.cs:16-54` | `GameTick.Run` -> facade -> runtime | Pure ordered facade: `RunCPoint`, `SyncHeldWeapons`, `RunHeldWeaponStep12`, `CollectCandidates`, `ResolveCharacterHits`, `ResolveObjectHits`; late entity update calls `ProcessOpointSpawn` at `GameTick.cs:1594`.
| INT-PASS-003 | `InteractionRuntimePasses.cs:36-49`; `CollisionCandidateCollector.cs:9-12`; `HitResolver.cs:9-17` | facade -> collector/resolver | Collection delegates to `CollisionCollect.CollectCandidates`; Character consume delegates to loop 1; object consume delegates to loop 2. No independent rules or state.
| INT-PASS-004 | `InteractionRuntimePasses.cs:16-34`; `WeaponPointRuntime.cs:10-23` | facade -> weapon-point facade -> `WeaponRuntime` | Force drop, held sync, and step12 are delegation-only wrappers.
| INT-PASS-005 | `InteractionRuntimePasses.cs:51-54`; `ObjectPointFactory.cs:11-14` | late update -> object factory -> `FrameTick.ProcessOpointSpawn` | Delegation-only opoint entry; generation truth is in `FrameTick`.

## Collision collection

### Top-level and pair gates

| ID | Authority location | Caller/callee | Preconditions, branches, fields, RNG/lifecycle |
|---|---|---|---|
| INT-COL-001 | `Interaction/CollisionCollect.cs:14-40`, `CollectCandidates` | `CollisionCandidateCollector.Collect` -> `DecrementPairVrest`, `CollectPair` | `.B1` first resets candidate carriers (`Mp`, arrays and related fields through `Entity.ResetHitCandidates`) for every active entity with `CharData`. `.B2` iterates unordered active pairs `i<j`; decrements both vrest directions once, then collects `i->j` and `j->i`. Deterministic slot order.
| INT-COL-002 | `CollisionCollect.cs:42-48`, `DecrementPairVrest` | `CollectCandidates` | Each positive `VRest[a,b]` and `VRest[b,a]` decrements independently; zero/negative unchanged.
| INT-COL-003 | `CollisionCollect.cs:50-136`, `CollectPair` | `CollectCandidates` -> pair/geometry/filter/record helpers | `.E1` pair gate false. `.E2` current/`PrevFrame2` attacker or victim frame missing. `.E3` current and snapshot itr/bdy lists must all be non-empty. `.E4` union generation fails. `.E5` coarse rectangles do not overlap. `.B1` per snapshot itr: oids 200/203/205/206/207/215/216 cannot hit oid209 except kind9. `.B2` kind3/8 require Character victim DAT. `.B3` state3005 kind8 lead-in may defer. `.B4` victim `HitStop!=0` blocks except kind8/14. `.B5-.B7` legacy group, kind5-holder-team, and kind0-effect filters. `.B8` strict Z: `-zHalf < delta < zHalf`, default half width 15. `.B9` first overlapping bdy supplies `hitBdyX`; candidate recording happens only after exact overlap.
| INT-COL-004 | `CollisionCollect.cs:138-160`, `PairAllowed` | `CollectPair` | `.E1` attacker `Residual.AttackExempt>0`. `.E2` `VRest[victimSlot,attackerSlot]>0`. `.E3` special current oid205 -> oid9 frame301 with all hit keys 999 and equal nonzero `Unk364` blocks. Otherwise allowed.

### Candidate selection

| ID | Authority location | Branch contract |
|---|---|---|
| INT-COL-005 | `CollisionCollect.cs:162-302`, `RecordCandidate` | Method-level order is fixed: initial reject -> nearest-path eligibility -> capacity -> state1004 gate -> kind-specific select -> append.
| INT-COL-005.B1 | `:173-204` | Initializes `reject=0`; victim state12 plus `fall<=40`, except kind10/11, sets reject2. `hitBdyX>=1000`, `vrest==0`, non-kind1/2/7 requires Character/current special attacker eligibility or a valid Character holder; otherwise reject2.
| INT-COL-005.B2 | `:206-240` | Nearest path is `vrest==0` and non-kind1/2/7. State1004 victim is allowed only for positive attacker DAT type with nonnegative link. Distance uses attacker X, but held objects use holder X; direct holder==victim forces 2000. If distance is smaller, or equal and RNG tie is even, writes `Mp2`, candidate slot/index 0, `Mp=1`, then returns.
| INT-COL-005.R1 | `:230` | Exactly one `NtsdRng.Rand()%2` is consumed only on equal nearest distance.
| INT-COL-005.E1 | `:242-250` | Candidate capacity `Mp>=HitCandidateMax` returns. For victim snapshot state1004, ordinary attackers reject unless kind2/7/10.
| INT-COL-005.B3 | `:253-265` | Kind1 competes by `victim.Mp3`; smaller or RNG-even tie updates `Mp3`, otherwise select2.
| INT-COL-005.R2 | `:257` | Exactly one RNG tie call for equal kind1 distance.
| INT-COL-005.B4 | `:267-270` | Kind4 with `WeaponCount!=0` selects; every non-kind1/2/7 selects unless already rejected.
| INT-COL-005.B5 | `:272-279` | Kind1 selects only if right/left held toward victim and victim current state16.
| INT-COL-005.B6 | `:281-287` | Kind2 selects light ground state1004 only with `LinkState==0`, `KeyJump==1`, `PrevJump==0`; heavy ground state2004 needs the same fresh jump but not the link-zero clause.
| INT-COL-005.B7 | `:289-293` | Kind7 selects state1004 on fresh jump.
| INT-COL-005.S1 | `:295-301` | Selected candidate appends at current `Mp`, stores victim slot and signed-byte itr index, increments `Mp`.

### Legacy filters and geometry

| ID | Authority location | Contract |
|---|---|---|
| INT-COL-006 | `CollisionCollect.cs:304-360`, `RejectLegacyKindGroupFilters` | Applies only kind `<4`, 6, Character-target kind9, 10, 11, 15, 16. `.B1` victim state13/10 bypasses rejection. `.B2` oid212 victim has paired-oid/frame-digit exceptions. `.B3` equal nonzero `Unk364`, except kind8, rejects unless attacker state18 with effect not21/22; Character attacker vs opposite-facing type3 rejects; Shuriken/IronBall/FlyingA/FlyingB victims are team-filter exceptions.
| INT-COL-007 | `CollisionCollect.cs:362-384`, `RejectLegacyKind0EffectFilters` | Rejects effect4 vs Character; effect20 vs non-Character or victim previous state18/19; effect21 vs previous state18/19; effect30 vs victim frame200-202; effect2 when attacker previous state19 and victim previous state18.
| INT-COL-008 | `CollisionCollect.cs:386-429`, `RejectLegacyKind5HolderTeamFilters` | Only kind5. Holder slot is `HolderIdx==-1 ? 0 : HolderIdx`; invalid/inactive/different or zero team does not reject. Same nonzero holder/victim `Unk364` rejects ordinary victim states/types, with oid212 paired-frame exception.
| INT-COL-009 | `CollisionCollect.cs:431-479`, `TryUnionItrRect` / `TryUnionBdyRect` | Empty lists fail. Unions min/max local rectangles; any release full-height bdy produces sentinel full-height union.
| INT-COL-010 | `CollisionCollect.cs:481-592`, itr/bdy world transforms | Facing0 uses `X-CenterX+local`; facing1 mirrors around `X+CenterX`. Full-height bdy maps Y to +/-1e9. Raw sentinel itr uses unchecked 32-bit three-term addition.
| INT-COL-011 | `CollisionCollect.cs:594-610`, `CollisionZ` / `Overlaps` | Type3 collision Z removes visual Z offset, else removes `(HitJ-50)` when positive; otherwise `ZInt`. Rectangle overlap is strict on both axes (touching edges do not hit).
| INT-COL-012 | `CollisionCollect.cs:612-628`, state3005/full-height helpers | State3005 kind8 defers if current frame has `HitFa`/opoint, or its distinct positive next frame does. Full-height bdy is `Y==int.MinValue && X<-100 && W>=900`.

## Hit consume and resolve

### Consume loop and dispatch

| ID | Authority location | Contract |
|---|---|---|
| INT-HIT-001 | `Interaction/HitResolve.cs:14-22`, `ResolveLoop1/2` | Loop1 consumes Character DAT attackers; loop2 consumes non-Character DAT attackers.
| INT-HIT-002 | `HitResolve.cs:24-65`, `ResolveCandidates` | Slot-order attacker scan. `.E1` inactive/no data/no candidates. `.E2` wrong DAT category for selected loop. `.E3` missing attacker `PrevFrame2`. Per pair: abort flag clears then breaks; invalid victim/itr slots, inactive victim, or current `VRest[victim,attacker]>0` skip. Uses itr from attacker snapshot frame and calls `ApplyCandidate`.
| INT-HIT-003 | `HitResolve.cs:67-117`, `ApplyCandidate` | Clones itr, preprocesses it, then dispatches: `.B0` kind0 victim oid300 special redirect and immediate return; `.B1` kind0/9 damage; `.B2` kind6 sets victim `HitConfirm=3`; `.B3` kind8 heal/teleport; `.B4` kind14 movement block flags; `.B5` kind15/16; `.B6` kind10/11; `.B7` kind1 grab; `.B8` kind3 grab; `.B9` kind2/7 pickup; default no-op.
| INT-HIT-004 | `HitResolve.cs:119-150`, `CloneItr` | Copies every data-driven itr field used by consume (`kind`, rect, dv, fall/bdefend/injury/rest/effect/attacking, catch/pick/throw values and zwidth), isolating preprocessing from source DAT.
| INT-HIT-005 | `HitResolve.cs:152-258`, `PreprocessCandidate` | `.B1` kind4 with weapon count becomes kind0 and reverses dvx when movement opposes facing. `.B2` kind0 hitting holder link2 may detach linked target, set vrest45/30, random frame0..5, `Vy=-1`. `.B3` held kind5 may copy holder wpoint-selected itr fields and become kind0. `.B4` IronBall halves dvx/dvy. `.B5` kind9 vs Character becomes kind0 and kills attacker; vs non-Character becomes kind0 only in state1002/2000.
| INT-HIT-005.R1 | `:184` | Link break chooses target frame `Rand()%6`.

### Damage families

| ID | Authority location | Ordered branches and writes |
|---|---|---|
| INT-HIT-006 | `HitResolve.cs:260-508`, `ApplyDamageCandidate` | Computes effective arest as 4 when `arest<4 && vrest==0`. `.E1` missing victim DAT.
| INT-HIT-006.B1 | `:269-304` | Kind9 records effect sound. Character victim kills attacker. Type3 victim records broken sound; state3005 -> frame40/hitconfirm2; otherwise copies team/holder, frame30, clears attacking/knockback/velocity, stores attacker slot in `AnimCounter`.
| INT-HIT-006.B2 | `:307-320` | Character alternate-hurt path resolves and records hit then returns. Kind0 FlyingB uses reaction-only damage + object tail + record then returns.
| INT-HIT-006.B3 | `:322-368` | Standard Character: lethal first-hit increments holder `KillStat` and indexed `KillStats`; subtracts HP and HPMax/3; increments victim/attacker combo stats and indexed `DamageStats`; death/effect4 forces fall80; hit/fall accumulation and force-fall rules.
| INT-HIT-006.B4 | `:370-442` | Fall thresholds in descending order: `>Fall` knockback/fall80; `>Heavy` frame226, airborne promotes knockback; `>Medium` frame222/224, airborne promotes; `>Light` frame220 or airborne 222/224. Sound then X knockback then state3000 tail. Knockback applies dvy or -7, clamps ground crossing to +12, selects frame180/186, and seeds linked target vrest45/30.
| INT-HIT-006.B5 | `:444-484` | Sets hit-state45, attacker delay3 unless negative, victim delay-3, attacker `AttackExempt=itrArest`, optional victim vrest, caught hurt frame, clears fall80 marker, mirrors delay to holder. Attacker state1002 randomizes frame0..15 and rebounds. Writes `ARest[attacker]=itrArest` last.
| INT-HIT-006.R1 | `:470` | State1002 attacker frame `Rand()%16`.
| INT-HIT-006.B6 | `:485-505` | Non-Character weapon families subtract durability (`Unk31C`) and `Bdefend==100` breaks it; object hurt tail applies. Kind0 additionally applies type/object tail and records hit.
| INT-HIT-007 | `HitResolve.cs:510-538`, `ApplyKind14` | Relative X/Z plus current/knockback velocity sets directional residual blockers: right/left thresholds 5, Z thresholds 2.
| INT-HIT-008 | `HitResolve.cs:540-615`, grab and alignment | Kind1/3 zero X velocity, face pair, select catching/caught actions, align cpoints, set reciprocal `CaughtIdx/CatcherIdx`, duration300, victim fall0. Kind1 seeds victim X/Y before align; kind3 seeds attacker X/Y. Alignment mirrors centers/cpoint X then half-corrects both X positions.
| INT-HIT-009 | `HitResolve.cs:617-625`, `ApplyKind8` | Sets victim `HealTimer=injury+1000`; attacker frame from `dvx`, copies victim X and Z+1.
| INT-HIT-010 | `HitResolve.cs:627-678`, `ShouldUseAlternateHurt` | Requires Character victim. Alternate path for oid37 hit-state<=15, oid6 hit-state<=1 with frame/state restrictions, oid52 hit-state<=15, unless heavy effects/attacker oid214/208. Also guard-like previous state7 with `bdefend<=60` and live victim when facing/dvx/specific oids satisfy blocked condition.
| INT-HIT-011 | `HitResolve.cs:680-827`, `ApplyAlternateDamage` | Requires Character. Scales injury by fall divisor then uses one tenth. Updates lethal/kill/combo/damage stats. Applies alternate sound/reaction and state3000 tail. Writes attacker `AttackExempt=(arest<4&&vrest==0?4:arest)`; vrest is `max(4,min(vrest,12))` when positive. Branches by victim oid/state/effect for frames, facing, reduced motion and fall behavior.
| INT-HIT-012 | `HitResolve.cs:829-980`, reaction/object hurt methods | `ApplyReactionOnlyDamage` performs reaction/rest/link/hit-state/delay without normal HP subtraction. `ApplyObjectHurtTail` sets weapon reaction by type/state, delay, attack-exempt, optional vrest and final ARest. Link break paths seed vrest45/30 and random released frame.
| INT-HIT-013 | `HitResolve.cs:982-995`, `ShouldForceFall80` | Forces fall80 for previous state13, previous2 state12, or Shuriken/IronBall/FlyingA/FlyingB victim DAT.
| INT-HIT-014 | `HitResolve.cs:997-1070`, `ApplyStandardDamageKnockbackX` | Branches by knockback, victim type, attacker/victim X/facing, effects22/23, and zero/nonzero dvx; includes Flying weapon velocity combination and directional sign rules.
| INT-HIT-015 | `HitResolve.cs:1071-1082`, oid100 tail | Only oid100 with negative link; multiplies knockback X by2.5, records SFX039, clamps nonzero magnitude to at least10.

### Sounds, hit records, object/type tails

| ID | Authority location | Contract |
|---|---|---|
| INT-HIT-016 | `HitResolve.cs:1084-1096`, `RecordDamageEffectSound` | Effect map 0..5 -> SFX001/002/006/010/011/004, default001; sound X is attacker X.
| INT-HIT-017 | `HitResolve.cs:1097-1130`, `RecordStandardHurtSounds` | Type3 attacker broken cue first; Character victim gets generic knockback/nonknockback cue; effect1 adds two extra cues; non-Character victim may add DAT `WeaponHitSound`.
| INT-HIT-018 | `HitResolve.cs:1132-1146`, alternate lead sound | Missing DAT returns. Type3 attacker uses broken sound and returns; otherwise oid37/6 victim uses SFX017, others SFX002.
| INT-HIT-019 | `HitResolve.cs:1148-1193`, `RecordKind0Hit` | Record owner is higher Z, tie higher slot. Capacity delegated to `CharacterPresentation.TryAddHitRecord`. Damage marker depends on effect1 and fall>60. Computes bounded hit point and consumes two `Rand()%9-4` offsets for record Z/X.
| INT-HIT-020 | `HitResolve.cs:1195-1219`, caught hurt frame | Requires not-fall80, victim previous2 cpoint kind2, valid reciprocal catcher. Selects front/back hurt action by facing if nonzero.
| INT-HIT-021 | `HitResolve.cs:1221-1290`, `ApplyKind0VictimObjectTail` | Type3 delegates special tail. Shuriken/Flying types set hitconfirm2, random frame0..15 and team; Flying also self-vrest30. IronBall sets holder/self vrest3 or19 based fall/effect, selects frame20 or random0..5. Attacker oid201 frees itself on Character victim; oid214 sets attacker HP0.
| INT-HIT-021.R1 | `:1244,:1253,:1276` | Random frame selection occurs only in the corresponding weapon-type branches.
| INT-HIT-022 | `HitResolve.cs:1292-1314`, oid300 special | Requires current and frame+6 bdys, current first bdy X>1000. Writes victim team1, frame=`bdy.X-1000`, delays 3/-3, and sets attacker abort-remaining-pairs flag.
| INT-HIT-023 | `HitResolve.cs:1316-1528`, type3 tail | Copies owner/team based on held state; clears motion; oid209/Karasu transformations may replace DAT/frame. Chooses frame20 vs30 by attacker DAT/link/effect2/20. Matching state3005/3006 pairs reset both entities and invert positive delay. Effect tail: 3/30 -> Character frame200 + SFX065; 5000..5999 drains PP; 6000..6999 selects frame; 2/21/22 -> Character frame203/facing + SFX068; effect20 same except previous state18; effect23 sound only.
| INT-HIT-024 | `HitResolve.cs:1530-1563`, state/type replacement helpers | `FrameStateIs` is current frame-state predicate. `IsKarasuOid` fixed oid set. `ReplaceWithActiveOid` chooses first active matching oid in slot order, assigns its DAT and writes wait/prev frame. `ReplaceEntityCharData` writes `CharData`, ids/types and weapon HP.
| INT-HIT-025 | `HitResolve.cs:1565-1624`, state2000/3000 tails | State2000 attacker moving toward victim scales Vx/Vz by0.4. Standard state3000 usually resets attacker to frame10, attacking0, Vx0, frame10 Dvz; oid209/Karasu exclusions skip. Alternate state3000 resets frame10/attacking/Vx only.

### Remaining kind dispatches and pickup

| ID | Authority location | Contract |
|---|---|---|
| INT-HIT-026 | `HitResolve.cs:1626-1733`, kind15/16 | Character victim: kind16 scales injury by fall divisor, updates lethal/kill/combo/damage stats, SFX065/frame200, optional vrest, linked-target release with random frame; kind15 additionally moves. Shuriken/Flying types except oid201/202 normalize frame and move; IronBall normalizes state2000 and moves with smaller Y step.
| INT-HIT-027 | `HitResolve.cs:1735-1754`, kind15 movement | Pushes X and Z by relative position, ensures `Y<=-2` and `Vy=-6`, then decrements Vy by type step while above -6; syncs knockback velocities.
| INT-HIT-028 | `HitResolve.cs:1756-1821`, kind10/11 | Kind11 rejects nonnegative `WeaponCount`. Character sets `WeaponCount=-20`; every 12 ticks outside step-wait increments holder combo11, always indexed damage11; damps X/Z by0.934579..., frame182, air-step3. Weapon types exclude oid201/202, normalize states and air-step3 or2.3.
| INT-HIT-029 | `HitResolve.cs:1823-1837`, air step | Same ground clamp as kind15; decrements Vy by supplied step only while above -6.
| INT-HIT-030 | `HitResolve.cs:1839-1850`, `RecordSound` | Empty cue returns. Otherwise appends `{Cue, WorldX, Tick=world.GameTick}` to `PendingSounds`; no playback side effect in simulation.
| INT-HIT-031 | `HitResolve.cs:1852-1930`, `ApplyPickupCandidate` | `.E1` kind7 requires attacker link0. `.B1` kind7 links generic1/-1, copies team/holder, increments pickup and held slot; oids120/124 -> link101; FlyingA ->4/-4; FlyingB ->6 if HP>0 else4 and clears durability when empty. `.B2` kind2 accepts only Shuriken/FlyingA/FlyingB/IronBall; selects attacker frame115 or116 and link1/4/6/2 with reciprocal negative link, then owner/link/pickup fields and attacking0. Other DAT type returns before writes.

## CPoint runtime

| ID | Authority location | Contract |
|---|---|---|
| INT-CP-001 | `Interaction/CPointRuntime.cs:14-18`, `Run` | Fixed order: kind1 pass, then kind2 validation.
| INT-CP-002 | `CPointRuntime.cs:20-46`, `SyncHeldCpoint` | Requires active attacker/current frame cpoint kind1 state9, valid `CaughtIdx`, reciprocal active victim current cpoint kind2; then syncs caught pair.
| INT-CP-003 | `CPointRuntime.cs:48-148`, `RunKind1Pass` | Active slot-order scan using attacker `PrevFrame2`. Missing/invalid caught pair immediately sends attacker frame0. State9 sync first. Positive `Decrease` subtracts duration; negative adds and, below0, releases to frames0/181, hit counts1, victim velocity +/-4,-3. Input action order: Aaction (`KeyJump && CdAttack`, direction gate), Taction (same plus any direction), Jaction (`KeyDefend && CdJump`), throw if `ThrowVx!=0`, then dircontrol facing logic.
| INT-CP-004 | `CPointRuntime.cs:150-185`, kind2 validation | Every current cpoint kind2 must have active reciprocal catcher current cpoint kind1; otherwise victim frame212, `Vy=-3`, and Y clamped to at most -2.
| INT-CP-005 | `CPointRuntime.cs:187-202`, cpoint action | Negative action flips attacker facing then uses absolute frame. Immediate attacker frame; victim action read from new attacker cpoint; immediate victim frame; both attacking0.
| INT-CP-006 | `CPointRuntime.cs:204-302`, caught sync | Hurtable/vaction frame gate first. Negative victim frame flips facing. Injury only when nonzero and attacker attacking0: positive scales by fall divisor, updates kill/HP/HPMax/combo, delays and attacking; negative executes `victim.Hp += injury` and `victim.HpMax += injury/3` exactly as written (therefore a negative value reduces both). Then aligns victim to attacker cpoint, applies cover remainder Z/Y offset and cover quotient facing, syncs doubles from ints.
| INT-CP-007 | `CPointRuntime.cs:304-341`, throw | Positive throw injury writes victim `WeaponCount`; `-1` swaps attacker DAT. Positions victim at attacker cpoint, advances attacker to current frame next and clears attacking, applies facing-signed throw Vx/Vy and directional Vz, sets victim frame/PrevFrame2 to vaction.
| INT-CP-008 | `CPointRuntime.cs:343-360`, DAT swap | Attacker stores old/new oids, adopts victim DAT, frame0. Every active entity with `KillCount==attacker.Slot` adopts same DAT in slot order.
| INT-CP-009 | `CPointRuntime.cs:362-369`, assign DAT | Writes data/id/entity type/runtime coarse type and weapon HP only.

## Weapon and wpoint runtime

| ID | Authority location | Contract |
|---|---|---|
| INT-WPN-001 | `Interaction/WeaponRuntime.cs:14-42`, `ForceDrop` | No held slot returns. Out-of-range clears held slot and throw guard. Inactive held clears held slot only. Valid pair clears holder links/target/held slot and held links/holder fields, halves held Vx.
| INT-WPN-002 | `WeaponRuntime.cs:44-69`, held sync | Active/data holder scan. Always runs cpoint held sync first. No held slot returns; out-of-range clears held slot and throw guard.
| INT-WPN-003 | `WeaponRuntime.cs:71-95`, held step12 | Scans active data objects with negative link. Invalid holder clears held link. Inactive/missing/nonreciprocal holder clears held link. Valid pair delegates.
| INT-WPN-004 | `WeaponRuntime.cs:97-213`, held pair | `.E1` missing data/frame. Consume path first and returns on fully consumed. Reads holder first wpoint/default; copies held frame/facing/delay, aligns held wpoint to holder wpoint and cover Z/Y. Held state12/10 releases and randomizes frame0..15 with one-third holder/reaction Vx. Nonzero wpoint dvx throws Shuriken/Flying types (frame40, owner slot, signed velocities) or IronBall (random0..5, delay1). Wpoint kind3 releases then randomizes frame0..5, Vx -3..3, Vy 0..-3, Vz -0.4..0.4.
| INT-WPN-004.R1 | `:161,190,208-211` | RNG calls occur only in release/type branches, in source order.
| INT-WPN-005 | `WeaponRuntime.cs:215-226`, consume dispatch | Requires holder state17 and held data; oid122 -> milk, oid123 -> beer, otherwise false.
| INT-WPN-006 | `WeaponRuntime.cs:228-256`, milk | HP<=0 false. Decrement held HP; every5 adds HPMax2/HP4 with HP3/HPMax clamps; every6 adds PP5 capped500. Only when HP reaches0 releases consumed item and returns true.
| INT-WPN-007 | `WeaponRuntime.cs:258-275`, beer | HP<=0 false. HP-=2, holder PP+=3 capped500, but child-owned held item with `KillCount>-1 && held.Pp>150` clamps holder PP150. On depletion releases and true.
| INT-WPN-008 | `WeaponRuntime.cs:277-285`, consumed release | Uses consume-specific unlink, held frame0/Vy-8/random Vx -3..3/durability0; holder frame0.
| INT-WPN-009 | `WeaponRuntime.cs:287-303`, release variants | Normal: both link0, release tick, clear holder slot. Consume variant additionally writes holder target0 and held holder0 before same release bookkeeping.
| INT-WPN-010 | `WeaponRuntime.cs:305-312`, clear slot | Clears held slot/throw guard only when holder points to this held slot.

## Opoint and object/slot lifecycle

| ID | Authority location | Contract |
|---|---|---|
| INT-OP-001 | `Frame/FrameTick.cs:233-331`, `ProcessOpointSpawn` | `.E1` missing data/frame. `.E2` first opoint missing/invalid or entity attacking. `.E3` Character with nonzero frame delay. For each valid opoint, `Facing>10` encodes count=`Facing/10`, facing mode=`%10`; each spawn delegates. Successful spawn increments `ObjectCount` in caller. Multi-spawn spreads Vz and counter-adjusts Vx. Type3 state3003 seeds bilateral vrest10 with linked entity. Multiple children receive symmetric spacing `AttackExempt` and pairwise vrest40.
| INT-OP-002 | `FrameTick.cs:333-458`, `SpawnFromOpoint` | `.E1` missing target DAT. `.E2` no free slot in `[50,MaxObjects)`. Resets chosen slot, publishes active identity/slot/team/link defaults/frame and transform; facing modes0 same,1 opposite, default0. Position is spawner-facing cpoint math; Z=spawner Z+1; signed dvx. Character DAT child inherits owner via `KillCount`, hitstop and AI flag. oid5/52 fixed HP10/PP5. Opoint kind2 creates immediate holder link. State3000/1002/3006 child reads spawner up/down for Vz, oid211 quarters it. Clears full arest/vrest row/column and input cooldowns before return.
| INT-OP-003 | `Simulation/SimulationWorld.Registry.cs:39-46`, `Spawn` | First inactive slot in ascending full pool; delegates `SpawnAt`; no slot returns null.
| INT-OP-004 | `SimulationWorld.Registry.cs:50-73`, `SpawnAt` | Invalid slot null. Resets then activates/publishes slot and base identity/position/data/type/durability/wait/previous frames; increments `ObjectCount`. Missing DAT is allowed and yields defaults.
| INT-OP-005 | `SimulationWorld.Registry.cs:82-85`, `FreeEntity` | Invalid or inactive returns. Resets entity, restores stable slot number, decrements positive `ObjectCount`. Rest matrices are not cleared here.
| INT-OP-006 | `SimulationWorld.QueryAndLinks.cs:12-23`, `ResetCooldowns` | Valid slot clears `ARest[slot]` and both VRest row and column.
| INT-OP-007 | `Simulation/GameTick.cs:616-677`, natural weapon drop | Counts every active non-Character DAT object; count>=4 returns without RNG. Otherwise consumes `Rand()%200` and proceeds only on zero. Uses first free slot50+, filters loaded oids100..199; oids122/123 each consume a `%2` gate and are disallowed in game modes1..4. Chooses candidate, then consumes four `%30` calls for X/Z. Calls `SpawnWeaponDrop` after Character hit loop and before object hit loop.
| INT-OP-008 | `GameTick.cs:679-725`, F8 weapon drop | Requires `F8Pressed`, then clears it even if no spawn. Enumerates loaded oids100..199 in numeric order; modes1..4 exclude 122/123; oid122 consumes a `%2` gate. Requires candidate and free slot50+, chooses random oid, consumes four `%30` position calls, clamps X to `[30,stageWidth-30]`, then spawns. Runs between natural drop and object hit loop.
| INT-OP-009 | `GameTick.cs:727-771`, mode2 weapon drop | Requires `GameMode2==1`. Enumerates loaded order100..199; oid122 consumes `%2` gate. For every candidate in order, takes next free slot50+ until exhausted and consumes four `%30` calls per spawn. Runs after late entity update and before postframe tail.
| INT-OP-010 | `GameTick.cs:773-797`, `SpawnWeaponDrop` | Missing DAT returns. Resets/publishes selected slot, initializes runtime identity, places at supplied X/Z and Y=-500, zeros velocity, oid122 HP200, increments `ObjectCount`, clears ARest and both VRest axes.
| INT-OP-011 | `GameTick.cs:1753-1782`, state transition effect selector | Active/data only. Leaving previous state13 or frame200 invokes branch1. Previous state18/19: leaving those states makes count7; remaining in them consumes `%4` and makes count1 only on zero; positive count invokes branch2. Uses DAT oid999.
| INT-OP-012 | `GameTick.cs:1784-1820`, transition branch1 | Missing oid999 DAT returns. Queues SFX066, then up to15 children using first free slot50+. Each child resets/publishes identity, copies source integer position/Z, consumes four RNG calls for Y, X, Vy, Vx, selects deterministic frame bands by child index, increments `ObjectCount`, clears rest axes.
| INT-OP-013 | `GameTick.cs:1822-1851`, transition branch2 | Missing DAT returns. Spawns requested count until slots exhaust; each child consumes four RNG calls (the final `%1` is still a call and always yields0), copies source position, uses Vy=-1 and frame140, increments count and clears rests.
| INT-OP-014 | `GameTick.cs:1853-1875`, effect slot/rest helpers | Free effect slot is first inactive slot50+. Reset helper clears ARest and both VRest axes; invalid slot returns.

## Stage rules affecting battle simulation

| ID | Authority location | Contract |
|---|---|---|
| INT-STG-001 | `Simulation/GameTick.cs:2016-2031`, current phase | First campaign whose id matches `StageSeriesIdx`; invalid wave index returns null; otherwise current phase.
| INT-STG-002 | `GameTick.cs:2033-2054`, wave eligibility/advance | Matching campaign only; no phase after last; wave `-1` or `waveReady` can advance. Advance increments wave exactly once after gate.
| INT-STG-003 | `GameTick.cs:2056-2128`, immediate entry slot | Spawn HP defaults500. Reject id<0, no free slot `[20,MaxObjects)`, missing DAT, or failed `SpawnAt`. Spawn uses team2. Stage Z defaults min180 and max min+1; X bound from override/background/800. Character-init semantics set hitstop20/team key2; holder copy=self slot. X=-1000 chooses random left/right offstage plus random 0..299; explicit X adds random0..299. Z random in range. oid122 HP200; frame/action, HP trio, PP500 and facing are set.
| INT-STG-003.R1 | `:2102-2112` | RNG order: side coin, side offset (or explicit-X offset), then Z offset.
| INT-STG-004 | `GameTick.cs:2130-2151`, spawn factor | Counts active Character DAT entities only in slots0..19; oid51 counts twice total, oid52 three times total.
| INT-STG-005 | `GameTick.cs:2153-2160`, reset runtime | Resets runtime wave to -1 and clears target/entry/spawned/slot lists.
| INT-STG-006 | `GameTick.cs:2162-2204`, ensure positive runtime | Existing matching wave/list sizes returns. Rebuilds one 40-slot array per spawn. Negative id or nonpositive ratio disabled. `entryCount=int(factor*ratio)` clamped0..40; target=`int(times*ratio*factor)` clamped>=0.
| INT-STG-007 | `GameTick.cs:2206-2252`, refill positive spawns | Requires matching wave and list size. Validates tracked slots by active/matching oid. Fills empty producer positions until per-entry target total, stopping on spawn failure; increments spawned totals.
| INT-STG-008 | `GameTick.cs:2254-2273`, cleared predicate | For every nonnegative spawn id, any active matching oid in slots20+ means not cleared.
| INT-STG-009 | `GameTick.cs:2275-2295`, producer initialized predicate | Immediate means ratio<=0; positive means ratio>0. Requires corresponding applied-wave marker for every present producer class.
| INT-STG-010 | `GameTick.cs:2297-2328`, phase advance | Requires valid progression, game mode1/2, nonnegative wave, current phase, producers initialized, spawns cleared, and next wave eligibility. On advance applies positive next bound to X/camera max, resets both applied markers and runtime spawn state.
| INT-STG-011 | `GameTick.cs:2330-2392`, current wave spawns | Same progression/mode/wave gates. Null phase resets runtime. Immediate entries (ratio<=0) apply once per wave; marker advances only if any spawned or phase empty. Builds positive runtime from factor; deferred positive entries apply once, store slots/increment totals; marker advances when positive producer seen or phase empty. Finally refills vacancies.
| INT-STG-012 | `GameTick.cs:113-124` | Phase advance and spawn execution occur after both hit loops and preframe bounds, before optional step-wait early return and before late opoint processing.

## ARest, VRest, statistics, and sound events

| ID | Authority location | Contract |
|---|---|---|
| INT-RST-001 | `Simulation/SimulationWorld.cs:29-32` | `VRest[victimSlot,attackerSlot]` is pair cooldown; `ARest[attackerSlot]` is attacker cooldown. Collector decrements pair VRest; `GameTick.RunCooldownsTick` decrements positive ARest at `GameTick.cs:1265-1275`.
| INT-RST-002 | `HitResolve.cs:266,448-450,483` | Standard damage writes residual `AttackExempt` and world ARest from effective arest; positive itr vrest writes victim/attacker pair. Effective arest is forced to4 only when arest<4 and vrest0.
| INT-RST-003 | `HitResolve.cs:799-803` | Alternate hurt uses the same attack-exempt rule; positive vrest becomes `vrest>4 ? min(vrest,12) : 4`.
| INT-RST-004 | `HitResolve.cs:182-183,436-437,939-940,1693-1697` | Link releases use fixed 45/30 pair cooldowns; exact matrix orientation follows each branch and is not interchangeable.
| INT-RST-005 | `FrameTick.cs:285-286,326-327` | Opoint linkage writes bilateral vrest10; sibling multi-spawn writes bilateral40.
| INT-RST-006 | `SimulationWorld.QueryAndLinks.cs:12-23`; `GameTick.cs:1864-1875`; `FrameTick.cs:446` | New/reused generated slots must clear ARest and both VRest axes. `FreeEntity` itself does not do this; spawn paths must call reset explicitly.
| INT-STAT-001 | `HitResolve.cs:326-358,699-732,1644-1678` | Standard, alternate, and kind16 lethal paths increment holder `KillStat` and indexed world `KillStats`; damage updates victim/attacker combo and indexed `DamageStats`. Conditions include live pre-hit victim and `KillCount==-1` ownership semantics.
| INT-STAT-002 | `CPointRuntime.cs:217-256` | Cpoint positive injury updates lethal holder kill stat and combo counters but does not write world indexed damage/kill arrays in this method.
| INT-STAT-003 | `HitResolve.cs:1762-1782` | Kind10/11 Character path adds combo11 every 12 ticks outside step-wait and indexed damage11 each consume.
| INT-SND-001 | `Simulation/GameTick.cs:31-41` | `PendingSounds` is cleared at the start of every tick; all later additions are per-tick simulation events.
| INT-SND-002 | `HitResolve.cs:1839-1850`; `FrameTick.cs:218-230`; `GameTick.cs:519-526` | Sound helpers append cue, world X and current tick only. Blank hit/frame cues are suppressed where checked. No renderer/audio playback occurs here.
| INT-SND-003 | `HitResolve.cs:1084-1147,1492,1511,1521,1526,1678`; `GameTick.cs:1682-1694,1809-1814`; `Frame/Physics.cs:294,326,349,381,438-449` | Data-driven and fixed combat cues include hit effect, generic hurt, elemental, weapon hit/broken/drop, transformation and landing/ground effects. Queue order follows call order and is observable when multiple cues are emitted.

## Coverage count

| Partition | Authority methods inventoried | Ledger method IDs | Explicit branch/RNG/side-effect IDs | Status |
|---|---:|---:|---:|---|
| Pass facades | 15 | 5 | 6 | Closed (delegation-only methods grouped by identical contract) |
| CollisionCollect | 21 | 12 | 27 | Closed |
| HitResolve | 40 | 31 | 42 | Closed at method and semantic branch-family level |
| CPointRuntime | 9 | 9 | 22 | Closed |
| WeaponRuntime | 12 | 10 | 25 | Closed |
| Opoint/object generation/registry/rest reset | 18 relevant | 14 | 39 | Closed |
| Stage battle runtime | 12 relevant | 12 | 31 | Closed; asset deployment excluded |
| Rest/stats/sound cross-cutting | cross-method | 12 | 20 | Closed |
| **Total** | **127 directly relevant methods** | **105 stable method/contract IDs** | **212 explicit branch/RNG/side-effect identities** | **No authority source write performed** |

Notes on counting:

- Tiny facade methods with identical delegation semantics are grouped under one stable method/contract ID, but all 15 declarations were inspected.
- `HitResolve` switch cases and large damage/effect functions use stable branch-family IDs; every source `kind`, `effect`, weapon-type, link, rest, lifecycle and RNG branch is represented in its containing family.
- `GameTick` has additional input/results/frame logic outside this partition; only its interaction, stage, rest/stat/sound, drop and lifecycle methods are counted here.

## Unresolved symbols and cross-partition dependencies

These are not guessed. They require another authority-ledger partition or a dedicated follow-up trace.

| Dependency ID | Symbol/location | Why it matters / current boundary |
|---|---|---|
| INT-DEP-001 | `Entity.ResetHitCandidates`, `Entity.Reset` | Candidate carrier defaults and complete pool-reset field list are owned by entity/runtime inventory. This ledger records every call site and publication order.
| INT-DEP-002 | `FrameRuntime.SetFrameImmediate` | Cpoint action/release uses immediate frame changes. Exact wait/next/previous-frame writes belong to frame-runtime inventory.
| INT-DEP-003 | `FrameTickRuntime.Tick` (`GameTick.cs:1530`) | Late lifecycle and frame sound/opoint ordering around the tick body belongs to frame inventory; direct opoint entry and later destruction check were traced here.
| INT-DEP-004 | `CharacterPresentation.TryAddHitRecord` | Hit-record capacity/replacement policy is presentation-runtime owned. HitResolve owner selection, values and RNG are closed here.
| INT-DEP-005 | `ObjTypeRules` / `WeaponType` constants | Category predicates and numeric enum definitions are framework/data-contract owned; all interaction call sites and category branches are recorded.
| INT-DEP-006 | `StageCampaignData`, `StagePhaseData`, `StageSpawnData` parser/population | Stage runtime consumption is closed; stage data parsing and default asset deployment are outside this partition. Default `stage.dat` deployment remains excluded.
| INT-DEP-007 | `NtsdRng.Rand` implementation and seeding | All interaction RNG call sites and call order are inventoried. Algorithm/seed ownership belongs to common/runtime inventory.
| INT-DEP-008 | `Physics.Update` and landing/drop sound branches | This ledger lists sound/lifecycle dependency sites, but full physics integration is owned by frame/physics inventory.

## Cross-partition handoff summary

1. Frame inventory must preserve the exact `PrevFrame2` snapshot boundary: cpoint/held sync occur before snapshot; collect and both hit loops read snapshot itr/bdy while several filters read current frames.
2. Entity/framework inventory must preserve fixed slots and reset-before-publication. Opoint uses slots50+, stage uses20+, ordinary spawn searches the full pool; rest matrices require explicit clearing on reuse.
3. Input inventory must preserve fresh jump as `KeyJump==1 && PrevJump==0`; it directly selects kind2/7 pickup candidates.
4. Data inventory must preserve all itr/cpoint/wpoint/opoint fields without normalization. They are direct branch inputs, frame selectors, velocities, cooldowns, effects and link modes.
5. Stage parsing may adapt file loading, but runtime `ratio/times/bound/x/y/act/hp/id` semantics and RNG call order above are authoritative. Default `stage.dat` deployment is not required by this ledger.
