# Unity Interaction Mapping Ledger (2026-07-18)

## Basis and status vocabulary

- Authority input: `.omc/research/csharp-authority-interaction-ledger-20260718.md`, read in full before this scan.
- Target input: current production and self-check source under `Assets/NTSD/Scripts/`; no previous audit status was used as evidence.
- `equivalent`: same simulation preconditions, branch order, field writes, RNG order, lifecycle result and observable event.
- `Unity-adapter`: engine/pool/CLR/task representation differs, while the runtime result and ordering map to the authority contract.
- `confirmed-difference`: current production-reachable result differs from the authority contract.
- `missing`: no target implementation was found for the authority contract.
- `authority-unresolved`: authority ledger explicitly deferred the symbol, so target parity cannot be decided in this partition.
- Test evidence below refers to `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`. The fresh full result available during mapping is `Temp/NTSD_BattleRuntimeSelfCheck.result = PASS` (2026-07-18 01:07:52.834).

## Shared target anchors

| Anchor | Current target location | Responsibility |
|---|---|---|
| U-PASS | `Simulation/NTSDBattleTickSystem.cs:17-209`; `Simulation/SimulationWorld.Passes.partial.cs:712-1098` | Fixed pass order, candidate boundary, split Character/object consume, late opoint and cleanup.
| U-COL | `Animation/Character/BruteForceSceneQuery.cs:236-1700` | Frozen step6 carrier, pair/filter/select/geometry/rest rules and consume-time revalidation.
| U-HIT-C | `Animation/LF2Objects/LF2CharacterHitResolver.cs:34-760` | Concrete `LF2Character` hit settlement.
| U-HIT-D | `Animation/LF2Objects/LF2CharacterDatHitResolver.cs:7-1328` | Shared current Character-DAT settlement, common runtime helpers and alternate damage.
| U-HIT-O | `Animation/LF2Objects/LF2WeaponBase.cs:227-878`; `LF2Weapon.cs:369-580`; `LF2SpecialAttack.cs:504-985` | Non-Character hit settlement and consume side effects.
| U-INT-C | `Animation/LF2Objects/LF2CharacterInteractionResolver.cs:34-456`; `LF2CharacterDatInteractionResolver.cs:25-240` | Character/current Character-DAT candidate consume, grab and pickup.
| U-INT-O | `Animation/LF2Objects/LF2WeaponInteractionResolver.cs:20-164` | Non-Character candidate consume.
| U-CP | `Animation/LF2Objects/LF2CharacterCatchResolver.cs:79-244`; `LF2Entity.cs` cpoint step10 entry | Cpoint action, injury, alignment and throw.
| U-WPN | `Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:19-428`; `LF2CharacterWeaponLinkResolver.cs:22-466`; `LF2WeaponReleaseFlowResolver.cs:16-61` | Held-object validation, pose, throw/drop, drink and unlink.
| U-OP | `Animation/Character/LF2ObjectPointFactory.cs:69-688`; `Simulation/SimulationWorld.Registry.partial.cs:303-623`; `SimulationWorld.Passes.partial.cs:1098-1370` | Task/pool-based object generation, runtime slots, drops and deferred lifecycle.
| U-STG | `Simulation/SimulationWorld.StageWave.partial.cs:15-653` | Stage phase, producer state, fixed-slot spawn and factory/direct Character adapter.
| U-RST | `Animation/Character/LF2ItrRestTracker.cs:31-136`; `Simulation/SimulationWorld.QueryAndLinks.partial.cs:17-39`; `SimulationWorld.Passes.partial.cs:962-1006` | Entity rest view plus raw runtime-slot matrix and ticking/reset.
| U-SND | `Simulation/SimulationWorld.cs:8-35`; hit/entity resolvers | Per-tick queued sound event `{Cue,WorldX,Tick}`.

## 105 authority contract mappings

### Pass contracts (5/5)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-PASS-001 | U-PASS; `NTSDBattleTickSystem.RunReleaseTick/RunFrameAdvancePhase/RunInteractionPhase/RunPresentationAndCleanupPhase` | Unity-adapter | Same collect -> Character consume -> random drop -> object consume -> stage -> late opoint order. Unity decomposes the monolithic call and flushes deferred mutations. Tests `3087-3161`, `7373-7806`, `10711-11100`, `13957-14355`.
| INT-PASS-002 | U-PASS world pass methods | Unity-adapter | Facade is replaced by direct `SimulationWorld` pass methods; no rule branch added. Same order proven by `NTSDBattleTickSystem.cs:32-76`.
| INT-PASS-003 | `CollectCollisionCandidatesAll`, `PostInteractionTickAll`, `ObjectInteractionTickAll` at `SimulationWorld.Passes.partial.cs:728-1037` | equivalent | Frozen sequence is consumed only by current DAT Character post loop or non-Character object loop. Tests `5002-5280`, `8664-8809`.
| INT-PASS-004 | `HeldObjectProcessAll` and resolver anchors U-WPN | Unity-adapter | Cpoint sync and held step12 are object/resolver calls instead of authority facades; field/order assertions `2547-3216`, `5451-5550`.
| INT-PASS-005 | `LateEntityUpdateAll` -> entity late update -> U-OP | Unity-adapter | Queue/flush and pooled presentation are adapters; logical spawn remains at late boundary. Tests `7373-7806`, `9959-10159`.

### Collision contracts (12/12)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-COL-001 | `BruteForceSceneQuery.CollectCollisionCandidates:236-294` | Unity-adapter | Clears carriers, scans slot-sorted active pairs, decrements both vrest directions, collects both directions. Pending-destroy suppression adapts deferred object teardown. Tests `8155-8809`, candidate cap/order `8664-8736`.
| INT-COL-002 | `DecrementPairVrest:360-371`; `LF2ItrRestTracker.TickVrestForAttacker` | Unity-adapter | Raw slot matrix and tracker projection decrement the same positive pair values. Rest-domain checksum tests `350-366`.
| INT-COL-003 | `CollectCandidatesForPair:382-430`; `ItrAllowed:1016-1104`; geometry `1221-1700` | Unity-adapter | Current/snapshot frame split, kind/oid/hitstop filters, strict Z and exact bdy overlap are present. Runtime current-DAT resolution and zero-width geometry are representation adapters. Tests `7880-8799`.
| INT-COL-004 | `CandidateCollectionPairAllowed:897-915`; `IsBlockedReleasePair:917-940` | Unity-adapter | AttackExempt, victim-side vrest, and current oid205/oid9 frame301 gate match. Current DAT lookup replaces direct `CharData`. Tests `8303-8375`.
| INT-COL-005 | `TryRecordReleaseCandidate:432-477`; `AcceptReleaseSelectFlagCandidate:676-715` | equivalent | Reject -> nearest -> cap -> state1004 -> kind select -> append order preserved. Tests `8155-8300`, `8664-8736`.
| INT-COL-006 | `ItrAllowed:1050-1095`; `RunsKindGroupFilters:1139-1148` | equivalent | Kind group, state13/10, oid212 and same-team exceptions match. Frame-source tests `8422-8469`.
| INT-COL-007 | `Kind0EffectAllowed:1168-1193` | equivalent | Effects4/20/21/30/2 and previous-frame state gates match. Tests within `CheckCollisionAudit3Contracts:8155-8469`.
| INT-COL-008 | `Kind5Allowed:1150-1166`; holder slot `1195-1201` | Unity-adapter | Same implicit slot0, team/type/oid212 rules; runtime-slot lookup adapts references. Tests `8163-8227`, held kind5 `2582-2707`.
| INT-COL-009 | `TryUnionItrRect:1401-1449`; `TryUnionBodyRect:1450-1502` | equivalent | Empty-list failure, min/max union and full-height propagation match. Tests `8079-8230`.
| INT-COL-010 | `ItrWorldRect/BodyWorldRect/LocalRectWorldRect/ItrWorldRectExeRaw:1524-1619` | Unity-adapter | Same integer mirroring/sentinel math; runtime integer position is used instead of transform truth. Tests `8079-8153`, geometry matrix `8231-8242`.
| INT-COL-011 | `CollisionZInt:1517-1523`; `Overlap:1694-1700` | equivalent | Type3 visual offset/HitJ correction and strict edge overlap match. Tests `8079-8153`.
| INT-COL-012 | `DeferState3005Kind8LeadIn:1120-1131`; `BodyIsReleaseFullHeight:1620-1624` | equivalent | Active-frame/next-frame opoint/hitFa defer and full-height signature match. Tests `8422-8469`.

Collision branch identity accounting: all 27 authority branch/RNG/side-effect identities map through the rows above. Nearest tie RNG and kind1 tie RNG use world deterministic RNG in `TryRecordNearestPathCandidate:479-526` and `AcceptReleaseKind1Nearest:790-810`; fresh-jump kind2/7 gates are `828-853`; append/cap/order is asserted at `8664-8736`.

### Hit contracts (31/31)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-HIT-001 | U-INT-C, U-INT-O; `SimulationWorld.Passes.partial.cs:1008-1037` | Unity-adapter | DAT category, not CLR class, selects post/object loop. Tests `5002-5280`, including missing-definition fallback.
| INT-HIT-002 | `LF2CharacterInteractionResolver.TryConsumeUnifiedStep7CandidateSequence:34-134`; `LF2CharacterDatInteractionResolver:55-118`; `LF2WeaponInteractionResolver:36-83` | Unity-adapter | Slot-carrier consume, itr-index validation, current occupant resolution and current vrest gate match. Object references are re-resolved by frozen slot. Tests `8664-8809`.
| INT-HIT-003 | interaction dispatch methods plus U-HIT-C/U-HIT-D/U-HIT-O | Unity-adapter | All authority kind cases dispatch; class-specific resolvers are an implementation split. Tests `4153-4795`, `5556-5900`, `13201-13389`.
| INT-HIT-004 | `BruteForceSceneQuery.ResolveRuntimeItrForPair:528-674` | equivalent | Shallow copy plus every preprocess-consumed field prevents authored itr mutation. Kind9 test `7270-7310`; held kind5 `2582-2707`.
| INT-HIT-005 | `ResolveRuntimeItrForPair:528-674`; consume effects `LF2Entity.cs:4202-4220` | Unity-adapter | Kind4, held kind5, IronBall halves, kind9 conversion/HP zero and linked release are preserved; resolver split separates returned itr from consume side effects. Tests `3950-3972`, `2582-2707`, `7260-7310`.
| INT-HIT-006 | U-HIT-C `34-492`; U-HIT-D `88-1324`; U-HIT-O | Unity-adapter | Standard/alternate/object families, threshold order, rests, link break and delay are present for concrete and shared current Character DAT shells. Tests `3757-4197`, `4620-4795`, `5556-5900`.
| INT-HIT-007 | `LF2Entity.ApplyKind14DirectionalBlockFrom:876-908`; shared resolver `962-982`; weapon hit `LF2Weapon.cs:369-383` | equivalent | X/Z thresholds and velocity-sign blocker writes match. Tests `CheckHitResolveSpecialKindContracts:4631-4695`.
| INT-HIT-008 | `LF2CharacterInteractionResolver.TryApplyKind1Grab/TryApplyKind3Grab/AlignKind3GrabPair:177-287` | Unity-adapter | Current Character-DAT target allows non-Character CLR shell; raw frame/link/half-align semantics match. Tests `4201-4271`, `4797-4900`.
| INT-HIT-009 | U-HIT-C/U-HIT-D kind8 branches | equivalent | Heal timer, raw frame=`dvx`, X/Z and integer mirrors match. Tests `4696-4742`.
| INT-HIT-010 | `LF2CharacterDatHitResolver.ShouldUseAlternateHurt:226-281` and concrete counterpart in `LF2CharacterHitResolver` | equivalent | oid37/6/52, heavy effects and guard-like gates match. Tests `7080-7188`.
| INT-HIT-011 | `LF2AlternateDamageResolver.ApplyAlternateDamage:283-394` | Unity-adapter | Same reduced injury/stat/rest clamp; shared helper used by concrete/current-DAT paths. Tests `7080-7270`, vrest `7189-7270`.
| INT-HIT-012 | U-HIT-O weapon/special hit tails; U-HIT-D reaction-only paths | Unity-adapter | HP-free reaction and weapon/object reaction/rest/link tails are distributed by target CLR/current DAT. Tests `5556-5900`.
| INT-HIT-013 | U-HIT-C/U-HIT-D `HitFall` helpers | equivalent | Previous state13, previous2 state12 and weapon-type forced knockdown represented. Tests `3757-4150`.
| INT-HIT-014 | `LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX:65-130`; concrete hit equivalent | Unity-adapter | Central current-DAT helper is used by shared shells; direction/effect/type branches match. Tests `3884-3972`, `4153-4197`.
| INT-HIT-015 | `LF2CharacterDatHitResolver.ApplyOid100KnockbackTail:132-143`; concrete equivalent | equivalent | Negative-link oid100 multiplier, SFX039 and min magnitude match. Covered by hit authority batch `3757-4197`.
| INT-HIT-016 | `LF2CharacterDatHitResolver.RecordDamageEffectSound:151-167`; entity sound helpers | equivalent | Effect0..5/default map and attacker X match. Sound tests `5374-5450`, `5556-5800`.
| INT-HIT-017 | `RecordStandardHurtSounds:169-213`; weapon DAT sound fields | equivalent | Broken -> generic/elemental -> victim hit cue order matches. Explicit order tests `5745-5780`.
| INT-HIT-018 | `LF2AlternateDamageResolver.RecordLeadSound:396-413` | equivalent | Missing data, type3 broken cue, oid37/6 SFX017 else SFX002 match. Tests `7080-7188`.
| INT-HIT-019 | `LF2Entity.RecordKind0Hit:478-548` | Unity-adapter | Same owner ordering, 10-slot capacity, hit position and two RNG offsets; presentation arrays live on entity module. Tests `3685-3756`.
| INT-HIT-020 | `LF2CharacterDatHitResolver.ApplyCaughtVictimHurtFrame:1264-1287`; concrete resolver equivalent | Unity-adapter | Reciprocal runtime-slot catcher lookup replaces direct array; front/back action matches. Tests `1476-2296`, `4797-4900`.
| INT-HIT-021 | U-HIT-O `LF2Weapon.Hit:369-580`, `LF2SpecialAttack.Hit:535-985`, rest matrix helpers | Unity-adapter | Type3/Shuriken/Flying/IronBall tails and self/holder rest are split by object type. Tests `5556-5900`.
| INT-HIT-022 | `LF2HitResolveRuntimeData.ShouldAbortRemainingHitPairsAfterOid300Redirect:44-63` plus consume loops | equivalent | Current/frame+6 geometry redirect and abort-after-success are preserved. Hit authority batch `4153-4197`.
| INT-HIT-023 | `LF2SpecialAttack.cs` type3 post-effect/tail `535-985`; shared hit post-effect `1289-1328` | Unity-adapter | Current DAT identity/replacement and frame/effect/sound tails map across pooled types. Tests `4631-4695`, `5451-5550`, `5829-5900`.
| INT-HIT-024 | `LF2Entity` current DAT identity helpers `4015-4050`; resolver transform helpers | Unity-adapter | Wrapper/current definition lookup replaces direct `CharData`; deterministic active/runtime-slot lookup retained. Tests `5002-5280`, `11744-11945`.
| INT-HIT-025 | `LF2CharacterDatHitResolver.DampenState2000Attacker/ApplyState3000Tail:474-514`; concrete equivalents | equivalent | Directional 0.4 damping and state3000 reset/exclusions match. Tests `7080-7188`.
| INT-HIT-026 | U-HIT-C/U-HIT-D kind15/16 branches and `ReleaseHeldTargetOnKind16` | Unity-adapter | Character/shared shell stats, movement, vrest link release and weapon-type routing match. Tests `13201-13389`.
| INT-HIT-027 | `LF2CharacterDatHitResolver.ApplyAirStep:1033-1048`; concrete whirlwind helpers | equivalent | Relative X/Z, Y=-2/Vy=-6 and 3.0/2.3 steps match. Tests `13201-13265`.
| INT-HIT-028 | U-HIT-C/U-HIT-D kind10/11 branches | Unity-adapter | Current Character DAT route, `WeaponCount=-20`, period12 combo and damage11, type exclusions and damping match. Tests `4743-4795`.
| INT-HIT-029 | shared/concrete air-step helpers | equivalent | Ground clamp and conditional Vy decrement match. Tests `4743-4795`, `13201-13265`.
| INT-HIT-030 | `SimulationWorld.QueueSound:23-34` | equivalent | Blank suppression at callers; event contains cue/X/current tick. Tests `5374-5450`, sound order `5745-5780`.
| INT-HIT-031 | `LF2CharacterDatInteractionResolver.TryApplyCurrentDatPickupCandidate:145-230`; concrete pickup `LF2CharacterInteractionResolver:376-427` | Unity-adapter | Current DAT, not CLR, selects weapon category; link101/current oid and HP-empty drink branches match. Tests `4201-4525`, including formal collect/wrong-loop/correct-loop and negative type3.

Hit branch identity accounting: all 42 authority branch/RNG/side-effect identities map through the 31 rows. The concrete and shared Character-DAT snapshots are explicitly compared at `BattleRuntimeSelfCheck.cs:4153-4197`, `5053-5368`, and kind16 at `13266-13389`; weapon and special tails are checked at `5556-5900`. No mapped authority hit branch is `missing` or `confirmed-difference` in current source.

### CPoint contracts (9/9)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-CP-001 | `SimulationWorld.PreInteractionTickAll:1038-1097`; entity cpoint step10 | Unity-adapter | Kind1 action pass and kind2 validation execute before candidate snapshot. Tests `3087-3161`.
| INT-CP-002 | `LF2CharacterWeaponLinkResolver.RunWeaponSyncHeldStep10:457-466`; cpoint sync entry | Unity-adapter | Runtime-slot reciprocal links and current cpoints replace array indices. Tests `1892-1955`, `3087-3161`.
| INT-CP-003 | `LF2CharacterCatchResolver.RunCpointActionSelectionStep10/ApplyCpointThrowStep10/ApplyCpointDirControlStep10:79-157`; entity state checks | Unity-adapter | Duration, escape, A/T/J ordering, throw and dircontrol match. Tests `1476-2296`.
| INT-CP-004 | state-being-caught validation and preinteraction pass | equivalent | Invalid reciprocal pair falls to frame212/Vy-3/Y clamp. Tests `2141-2224`.
| INT-CP-005 | `ApplyCpointActionStep10:108-116` | Unity-adapter | Signed action and immediate frame/wait reset use explicit raw-write helpers. Tests `1811-1955`.
| INT-CP-006 | `ApplyCpointHeldInjuryStep10:159-200`; `SyncCpointHeldPositionStep10:201-244` | Unity-adapter | Exact signed injury, holder-only stats, cover/facing/position and integer mirrors match. Tests `2225-2296`.
| INT-CP-007 | `ApplyCpointThrowStep10:117-140` and entity transform helper | Unity-adapter | Throw injury, transform `-1`, next/action raw frames and signed velocities match. Tests `1591-1631`, `1956-2086`.
| INT-CP-008 | transform throw path and child current-DAT rebinding | Unity-adapter | Current DAT wrapper replaces direct `CharData` pointer; child owner scan remains runtime-slot ordered. Tests `1956-2086`, transform routing `11744-11945`.
| INT-CP-009 | `LF2Entity.TryApplyRuntimeIdentity` and current DAT helpers | Unity-adapter | Writes wrapper/id/category/durability and preserves CLR presentation shell. Tests `1956-2086`, `5002-5280`.

Cpoint branch identity accounting: all 22 authority identities are covered by the rows above and the focused cpoint suite `1476-2296`.

### Held weapon/wpoint contracts (10/10)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-WPN-001 | `LF2CharacterWeaponLinkResolver.ForceReleaseHeldObjectReference:363-374`; release-flow resolver | Unity-adapter | Runtime-slot links and pooled object references replace array entries; valid/invalid link cleanup and Vx damping are tested `2547-3216`.
| INT-WPN-002 | `SimulationWorld.HeldObjectProcessAll:96-129`; `RunWeaponSyncHeldStep10` | Unity-adapter | Slot-sorted holders, cpoint first, stale slot cleanup. Tests `3087-3161`.
| INT-WPN-003 | `LF2WeaponHeldStateResolver.RunStep12:255-315`; world held pass | Unity-adapter | Negative-link held scan and reciprocal validation use runtime slots. Tests `2764-3086`, `7579-7806`.
| INT-WPN-004 | `LF2WeaponHeldStateResolver:255-428`; `LF2CharacterWeaponLinkResolver.ReleaseHeldObjectByWPoint:168-361` | Unity-adapter | Consume first, pose/cover, damage release, wpoint throw and kind3 RNG match; double runtime positions adapt float presentation. Tests `2547-3216`, `5451-5550`.
| INT-WPN-005 | `LF2WeaponHeldStateResolver.ProcessDrinkConsumption:166-234` | Unity-adapter | State17 and oid122/123 dispatch match. Tests `2870-3006`.
| INT-WPN-006 | milk branch `166-206` | equivalent | HP decrement, modulo5 HP and modulo6 PP/clamps match. Tests `2870-3006`.
| INT-WPN-007 | beer branch `207-234` | equivalent | HP-2, PP+3/caps and child clamp match. Tests `2870-3006`.
| INT-WPN-008 | consumed release in held-state/release-flow resolvers | Unity-adapter | Frame0, Vy-8, RNG Vx, durability0 and holder frame0 match. Tests `2870-3006`.
| INT-WPN-009 | `LF2WeaponReleaseFlowResolver:16-61` | Unity-adapter | Normal and consume-specific runtime fields match; rendering parent/layer cleanup is additional adapter state. Tests `2764-3086`.
| INT-WPN-010 | `ClearLinks:418-428`; character link clear helpers | Unity-adapter | Clears only matching holder slot/reference and throw guard equivalents. Tests `2547-3216`.

Held branch identity accounting: all 25 authority identities map through the ten rows; RNG/cover/frame-delay invariants are asserted at `2547-3216` and `5451-5550`.

### Opoint/object lifecycle contracts (14/14)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-OP-001 | `LF2ObjectPointFactory.ProcessOpointSpawnAlignedToCpp/ProcessOneLateOpoint:168-273` | Unity-adapter | Frame/current-DAT gates, facing-count encoding, spread, state3003 vrest and sibling exempt/rest match; spawn is a task/pool adapter. Tests `7373-7806`, `10007-10159`.
| INT-OP-002 | `LF2ObjectPointFactory.ProcessCreateObject/PostInitLiving:274-582` | Unity-adapter | Required slot50+, reset/register, identity, transform, owner, oid5/52, kind2 link, directional Vz and rest reset match. Tests `7507-7806`, `10050-10159`, `13645-13875`.
| INT-OP-003 | `SimulationWorld.Register/AllocateRuntimeSlot:303-550` | Unity-adapter | First permitted free fixed runtime slot; CLR instance may preexist. Tests `7507-7806`, full-slot rejection `7680-7702`.
| INT-OP-004 | factory create + `Register` | Unity-adapter | Pool reset and registration publish identity/slot before use; no-data spawn is rejected because target requires loaded wrapper/definition, matching opoint authority E1. Tests `7610-7702`, `10050-10095`.
| INT-OP-005 | `Unregister`, `FlushPendingEntityDestroy`, `FreeEntityLikeExe` at Registry `359-464` | Unity-adapter | Deferred teardown hides entity immediately and finalizes at pass boundary; count is computed from active registry. Tests `9259-10159`, `10200-10290`.
| INT-OP-006 | `ResetCooldownsForRuntimeSlot:QueryAndLinks:17-39` | Unity-adapter | Clears tracker occupant and raw matrix row/column. Tests `10050-10159`.
| INT-OP-007 | `SimulationWorld.RandomWeaponDropTickAll:1098-1231` | Unity-adapter | Count/gate/candidate/RNG/slot/drop initialization match through pooled factory. Tests `13957-14062`.
| INT-OP-008 | same method F8 branch | Unity-adapter | Press clear, numeric enumeration, mode/oid gates, clamped X and RNG order match. Tests `13957-14062`.
| INT-OP-009 | `Mode2RandomWeaponDropTailAll/SpawnMode2RandomWeapons:1232-1370` | Unity-adapter | Loaded-order candidates, per-candidate four RNG calls and slot exhaustion match. Tests `13957-14062`.
| INT-OP-010 | random-drop factory task creation `1145-1210` | Unity-adapter | Required slot, direct runtime coordinates, Y=-500, zero velocity, oid122 HP and rest reset match. Tests `13957-14062`.
| INT-OP-011 | `LF2Entity` transition selector around `3670-3890` | Unity-adapter | State-exit/count/%4 selection maps; spawn semantic tags transition-only objects. Tests `7373-7558`, transition precision `11179-11220`.
| INT-OP-012 | transition branch1 task loop | Unity-adapter | SFX066, up to15, four RNG calls/frame bands/slot reset map; task queue/presentation is adapter. Tests `7373-7558`.
| INT-OP-013 | transition branch2 task loop | Unity-adapter | Count loop, four RNG calls including `%1`, frame140 and cleanup match. Tests `7373-7558`.
| INT-OP-014 | `FindFirstFreeRuntimeSlot`, reset helpers, factory required slot | Unity-adapter | Fixed high-slot policy and both rest domains match. Tests `7507-7806`, `10050-10159`.

Object-generation branch identity accounting: all 39 authority identities map through the fourteen rows. Factory/pool/task behavior is intentionally classified as adapter, not as extra battle logic.

### Stage contracts (12/12)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-STG-001 | `SimulationWorld.StageProgressionCurrentPhase:86-106` | equivalent | First matching series and wave range behavior match.
| INT-STG-002 | `StageProgressionCanAdvanceWave/AdvanceWave:107-136` | equivalent | Last-phase and `-1 || ready` gates match.
| INT-STG-003 | `SpawnStageImmediateEntrySlot:429-507` | Unity-adapter | Required slot20+, data gates, bounds, RNG order, HP/PP/team/facing contract match; factory/direct Character creation adapts CLR/pool constraints. Tests `14063-14355`.
| INT-STG-004 | `StageSpawnEntryFactor:140-160` | equivalent | Slots0..19 and oid51/52 weights match. Test `14063-14162`.
| INT-STG-005 | `ResetStageSpawnRuntime:161-169` | equivalent | Wave and all producer lists reset.
| INT-STG-006 | `EnsureCurrentWavePositiveStageRuntime:170-218` | equivalent | Shape reuse, arrays40, ratio/factor count and clamping match.
| INT-STG-007 | `RefillCurrentWavePositiveStageSpawns:219-267` | Unity-adapter | Slot validation/refill/total match; entity resolution uses runtime registry. Tests `14329-14380`.
| INT-STG-008 | `CurrentWaveStageSpawnsCleared:268-302` | Unity-adapter | Active matching oid in slots20+ blocks; registry query replaces array access.
| INT-STG-009 | `CurrentWaveStageSpawnProducersInitialized:303-330` | equivalent | Immediate/positive markers match.
| INT-STG-010 | `ApplyCurrentWavePhaseAdvance:331-360` | equivalent | All gates, bound/camera update and reset order match. Tests `14163-14328`.
| INT-STG-011 | `ApplyCurrentWaveImmediateStageSpawns:361-428` | Unity-adapter | Immediate/deferred markers and refill match; spawn uses factory/direct adapter. Tests `14163-14380`.
| INT-STG-012 | `NTSDBattleTickSystem.RunPresentationAndCleanupPhase:67-76` | Unity-adapter | Stage remains after both interaction loops and before late opoint; render dispatch is inserted as presentation-only adapter. Tests `10711-11100`, `14163-14380`.

Stage branch identity accounting: all 31 authority identities map through the rows above. Default stage asset deployment is not used as test evidence; fixtures drive runtime rules.

### Rest/stat/sound contracts (12/12)

| Authority ID | Target mapping | Status | Branch/result evidence and tests |
|---|---|---|---|
| INT-RST-001 | U-RST; `SimulationWorld.VrestTickAll:962-972` | Unity-adapter | Per-entity tracker plus raw matrix project the authority dimensions; positive cooldown ticks match. Tests `350-366`, `3617-3626`.
| INT-RST-002 | `LF2Entity.ResolveArestCooldown/ApplyItrRestAfterHit:800-818`; hit resolvers | Unity-adapter | Effective arest and victim pair vrest match in both domains. Tests `3757-4197`, weapon rest `5698-5740`.
| INT-RST-003 | `LF2AlternateDamageResolver.ApplyAlternateDamage:283-394` | equivalent | Positive vrest clamp4..12 and attack exempt match. Tests `7189-7310`.
| INT-RST-004 | kind16/link-release helpers in U-HIT-C/U-HIT-D | Unity-adapter | Runtime-slot orientation 45/30 preserved. Test `13266-13389`.
| INT-RST-005 | `LF2ObjectPointFactory.ApplyState3003LinkedVrest/ApplyMultiSpawnExemptAndVrest:239-273,599-610` | Unity-adapter | Bilateral10 and sibling40 use runtime slots. Tests `7736-7780`.
| INT-RST-006 | `ResetCooldownsForRuntimeSlot:17-39`; registration/reset paths | Unity-adapter | Both tracker and raw row/column clear on reuse; unregister alone defers cleanup. Tests `10050-10159`, reset `10162-10290`.
| INT-STAT-001 | hit resolvers and `BattleRuntimeState.KillStats/DamageStats:357-358` | Unity-adapter | Entity-local and world arrays match for standard/alternate/kind16. Tests `3757-4197`, `7080-7270`, `13266-13389`.
| INT-STAT-002 | `LF2CharacterCatchResolver.ApplyCpointHeldInjuryStep10:159-200` | equivalent | Holder/local combos only; world arrays unchanged. Test `2225-2296`.
| INT-STAT-003 | kind10/11 shared/concrete hit branches | equivalent | Period12 combo11 and indexed damage11 match. Test `4743-4795`.
| INT-SND-001 | `NTSDBattleTickSystem.RunReleaseTick:21-25`; `SimulationWorld.PendingSounds` | equivalent | Queue clears once at tick start before battle effects. Tests `5374-5450`.
| INT-SND-002 | `SimulationWorld.QueueSound:23-34`; entity frame sound helpers | Unity-adapter | Same event fields; playback remains presentation-side. Tests `5374-5450`.
| INT-SND-003 | hit/entity/physics sound callers | Unity-adapter | Cue selection and insertion order match; asset cue normalization is presentation adapter. Tests `5374-5450`, `5745-5780`, cleanup sound `7373-7460`.

Cross-cutting branch identity accounting: all 20 authority rest/stat/sound identities map through these rows.

## Status totals

| Status | Authority contract IDs | Authority semantic identities |
|---|---:|---:|
| equivalent | 35 | inherited by the branch identities described in those 35 contract rows |
| Unity-adapter | 70 | inherited by the branch identities described in those 70 contract rows |
| confirmed-difference | 0 | 0 |
| missing | 0 | 0 |
| authority-unresolved | 0 of the 105 mapped contracts | 0 of the 212 mapped identities |
| **Total** | **105** | **212 accounted identities** |

The authority ledger groups its 212 semantic identities inside 105 stable contract IDs rather than assigning every identity a standalone base ID. Each branch/RNG/side-effect identity therefore inherits the status of its explicit contract row above; the partition accounting paragraphs enumerate all 212. The counts apply only to the interaction authority ledger and are not a whole-battle parity certificate.

## Authority-unresolved dependency mapping

| Dependency ID | Target location | Status | Current conclusion |
|---|---|---|---|
| INT-DEP-001 | `LF2Entity.Reset`, `NTSDEntityRuntime.Reset`, registry reset | authority-unresolved | Target reset/reuse is extensively tested, but complete parity belongs to entity/framework authority inventory.
| INT-DEP-002 | `LF2Entity.DirectWriteFrameImmediateWaitReset`, signed immediate helpers | authority-unresolved | Target call sites are mapped; exact frame-runtime parity belongs to frame inventory.
| INT-DEP-003 | `SimulationWorld.LateEntityUpdateAll`; entity TU/frame tick | authority-unresolved | Opoint boundary is mapped; complete frame tick is outside this ledger.
| INT-DEP-004 | `LF2Entity.RecordKind0Hit`, `LF2HitCountersModule` | authority-unresolved | Capacity/value behavior is tested, but authority presentation helper ownership was deferred.
| INT-DEP-005 | `LF2Entity.ResolveCurrentDataObjectType`, `LF2ObjectType` | authority-unresolved | Interaction usage is mapped; numeric/category contract belongs to framework inventory.
| INT-DEP-006 | `BattleStageCampaignLoader`, stage data models | authority-unresolved | Runtime consumption is mapped; parser/deployment is outside scope.
| INT-DEP-007 | world deterministic RNG implementation | authority-unresolved | Every interaction call site/order is mapped; RNG algorithm/seed belongs to framework inventory.
| INT-DEP-008 | entity physics/landing resolvers | authority-unresolved | Interaction sound/lifecycle call sites are mapped; full physics parity belongs to frame inventory.

## Reverse scan: production target branches absent from this authority ledger

Only branches with a production call path are classified as reachable. Test-only hooks are separated.

| Target-only ID | Target branch and location | Reachability evidence | Classification | Effect on authority result |
|---|---|---|---|---|
| UONLY-INT-001 | Fixed runtime slot + `StableId` dual identity, registry sorting, required slots; `SimulationWorld.Registry.partial.cs:303-623` | Every production `Register` allocates a runtime slot; opoint/stage tasks set required slots. | adapter | Stable CLR identity is presentation/pool identity; battle links/candidates use fixed runtime slots. Tests `7507-7806`, `8664-8809` prove slot-order and same-slot reuse.
| UONLY-INT-002 | Deferred unregister/`PendingFlushDestroy`; Registry `359-480`, pass-finally flushes | Production `Unregister` during `_ticking` releases slot and queues final teardown; all deferred passes use this path. | adapter | Hides an authority-freed entity immediately while delaying GameObject/pool teardown. Tests `9259-10290` and candidate filters prove no extra consume.
| UONLY-INT-003 | Candidate stores slot and resolves current same-slot occupant; `SceneQueryHit.ResolveCurrentTarget`, consumers | Production frozen carriers can outlive an occupant within the tick; consume calls `ResolveCurrentTarget`. | adapter | Preserves authority array-slot semantics despite CLR references. Test `8738-8809` proves newborn is consumed, stale object is untouched.
| UONLY-INT-004 | Current DAT type/oid resolution independent of CLR shell; `LF2Entity.cs:4015-4050` | Every collect/consume route calls the resolver; pooled shells may change current wrapper. | adapter | Recreates authority `CharData` identity. Tests `5002-5280`, `11744-11945` include missing definitions and transformed shells.
| UONLY-INT-005 | `IsPureTransitionSmoke`; `BruteForceSceneQuery.cs:977-1004` | Production transition effects set `Runtime.SpawnSemantic=TransitionEffect`; collector calls the gate at `903`. | adapter | Prevents presentation-only pooled oid999 effects from entering battle queries. Production-data classifier `7880-8077` asserts no valid resolved geometry is suppressed.
| UONLY-INT-006 | `PS==null` / missing renderer-position guards in scene query | Every query checks `PS`; fully initialized production entities bind it, but a rejected/partial pooled object could lack it. | unreachable after valid production registration | No valid registered combat entity reaches this branch; factory rejects failed creations before publication (`LF2ObjectPointFactory.cs:274-370`).
| UONLY-INT-007 | Immediate scene-query overloads outside frozen step6; `BruteForceSceneQuery.cs:29-234,320-359` | Production calls exist in `LF2Weapon.cs:301` (impact body probe) and `:713` (weapon-strength body probe). | adapter (cross-partition physics/wpoint) | These probes implement authority physics/weapon body-volume operations outside this ledger's formal step6 consumer. Formal interaction consumers never fall back to immediate query (`TryGetCollisionCandidateSequence:296-312`).
| UONLY-INT-008 | Queue + flush opoint tasks and pooled renderer/logic allocation; factory `69-167,274-497` | Production entity opoint modules enqueue; frame/late passes flush at explicit boundaries. | adapter | Changes allocation mechanics only. Required runtime slots, direct runtime position/velocity and flush timing preserve authority publication. Tests `7373-7806`, `9959-10159`.
| UONLY-INT-009 | Stage factory spawn then direct Character fallback; `SimulationWorld.StageWave.partial.cs:470-653` | `SpawnStageImmediateEntrySlot` always tries factory; missing pool/factory can reach direct fallback for Character DAT. | adapter | Both paths require the same runtime slot and reapply the same runtime contract. Tests `14063-14355` cover strict slot/type paths.
| UONLY-INT-010 | Render dispatch inserted between stage and frame postprocess; `NTSDBattleTickSystem.cs:67-76` | Every completed production tick calls it. | adapter | Presentation snapshot only; simulation writes continue after it and render state does not feed collision truth. GameTick ordering tests `10711-11100`.
| UONLY-INT-011 | Dual rest storage: entity `LF2ItrRestTracker` plus raw runtime-slot matrices | Registration and every hit/rest operation synchronize both domains. | adapter | Supports CLR object ownership/checksum while retaining authority matrix orientation. Tests `350-366`, `3617-3626`, `10050-10159`.
| UONLY-INT-012 | Pool/render parent/layer cleanup on held release | Production weapon release resolvers clear renderer parent/layer in addition to logical links. | adapter | Presentation-only cleanup; logical link, velocity, frame and rest assertions remain authority-mapped (`2547-3216`).
| UONLY-INT-013 | `RespawnEffectSpawnOverride`; `SimulationWorld.Passes.partial.cs:17,544-546` | Only assigned by `BattleRuntimeSelfCheck.cs:13131-13197`; no production assignment found by repository-wide reference scan. | unreachable in production | Test seam only; default null branch cannot alter production battle behavior.
| UONLY-INT-014 | Direct `CreateObjectImmediate` public API | Production callers: stage, random drops, transition/respawn and entity late effects; tests also call it directly. | adapter | Synchronous creation is required where authority publishes within the same pass; task fields preserve slot/position/RNG results.
| UONLY-INT-015 | Fallback from runtime slot to `StableId` in `LF2ObjectPointFactory.GetRuntimeSlotOrStableId:593-597` | Called during post-init ownership; valid registered entities already have a slot. | unreachable for successfully published production entities | Fallback prevents null diagnostics on rejected/partial objects; successful opoint tests assert slot-based ownership `7736-7806`.

Reverse-scan result: no production-reachable target-only branch in the requested interaction surface is currently classified as a behavioral `difference`. Twelve are representation/timing adapters, three are unreachable in valid production state or test-only. This does not close authority dependencies outside the interaction ledger.

## Verification and residual risk

- Static source mapping covers all 105 authority contract IDs and accounts for all 212 semantic branch/RNG/side-effect identities by partition totals.
- Focused self-check coverage exists for every partition, and the fresh aggregate result is `PASS`.
- `git diff --check` must be run on this ledger before handoff.
- Residual risk is limited to the eight `authority-unresolved` cross-partition dependencies and real-scene validation not represented by the self-check fixtures. No claim of complete combat parity follows from this mapping alone.
