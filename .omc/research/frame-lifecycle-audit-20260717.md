# Frame / Lifecycle Static Parity Audit (2026-07-17)

## 1. Scope and authority

- Sole behavior authority: `J:\QQFile\NTSD2.4\ntsd_release_C#`.
- Authority entry points audited in full:
  - `src/BattleCore/Frame/FrameTick.cs` (4/4 methods).
  - `src/BattleCore/Frame/FrameAdvance.cs` (15/15 methods, including every `hit_Fa` branch 1..14 that exists in the file).
  - `src/BattleCore/Frame/FrameRuntime.cs`, `FrameTickRuntime.cs`, `FrameAdvanceRuntime.cs`, and `FrameTransistor.cs` (all methods).
  - Direct lifecycle dependencies: `Entity.Reset`, `SimulationWorld.Spawn/SpawnAt/FreeEntity`, `ResetCooldowns`, `ResetBattleRuntime`, and the relevant `GameTick` frame-logic/frame-advance/late-tail call boundaries.
- Unity mapping audited in full for the relevant methods in `LF2Entity`, `LF2Character`, `LF2LivingObject`, `LF2WeaponBase`, `LF2Weapon`, `LF2SpecialAttack`, `LF2OtherObject`, `LF2ObjectPointFactory`, `LF2ReferencePool`, `LF2ObjectPool`, `LF2ObjectRenderer`, `NTSDEntityRuntime`, and `SimulationWorld` registry/pass/query partials.
- `Physics.Update` is called by authority `FrameAdvance.Advance` at `FrameAdvance.cs:46`; this report verifies the call gate and Unity dispatch boundary. The internal `Physics.cs` branch audit belongs to the separate physics partition and is not duplicated here.
- T8 default `stage.dat` deployment is excluded. No missing default stage asset is counted as a lifecycle difference.

## 2. Authority order and Unity mapping

| Authority operation | Authority coordinate | Unity mapping | Static result |
|---|---|---|---|
| `FrameAdvance.FrameLogic` slot scan | `GameTick.cs:72-81`, `FrameAdvance.cs:49-215` | `NTSDBattleTickSystem.cs:42-44`, `SimulationWorld.Passes.partial.cs:662-675`, `LF2Entity.cs:1034-2070` | Main ordering is mapped; destruction/slot availability and some target/RNG gates differ (FL-02..FL-05). |
| `FrameAdvance.Advance` slot scan | `GameTick.cs:83-93`, `FrameAdvance.cs:13-47` | `SimulationWorld.Passes.partial.cs:340-369`, per-shell `SimTransit/SimTU`, shared frame-advance helpers | Throw guard, signed `FrameDelay`, negative link, cpoint kind 2, and current-DAT dispatch are present. Held weapon frame-logic gate differs before this pass (FL-01). |
| Late `FrameTick.Tick` | `GameTick.cs:1523-1593`, `FrameTick.cs:13-216` | `SimulationWorld.Passes.partial.cs:706-783`, `LF2Entity.RunCommonFrameTick` | Counter and wait/next skeleton is mapped; frame transition side effects and missing-frame semantics differ (FT-01..FT-04). |
| Late opoint publication | `GameTick.cs:1574`, `FrameTick.cs:233-457` | `SimulationWorld.Passes.partial.cs:762-777`, `LF2ObjectPointFactory.cs:132-267` | Same per-slot late boundary and higher-slot same-tick visibility; several initialization/reset contracts differ (OP-01..OP-05). |
| Free/reset/reuse | `SimulationWorld.Registry.cs:45-85`, `SimulationWorld.QueryAndLinks.cs:12-23` | `SimulationWorld.Registry.partial.cs:206-445`, renderer/reference pools | Fixed 400-slot model is emulated, but unregister ordering, cooldown-column reset, and whole-world reset differ (LC-01..LC-05). |

### Confirmed equivalent boundaries

1. `FrameAdvance.Advance` checks signed `FrameDelay` before link/cpoint and increments negative delay toward zero (`FrameAdvance.cs:21-31`; `LF2Entity.cs:4679-4696`).
2. Frame-advance clears runtime action and direction keys immediately before each entity (`GameTick.cs:89-91`; `SimulationWorld.Passes.partial.cs:348-356`).
3. Frame logic and late entity scans are ascending dynamic runtime-slot scans. A newly published higher slot can run later in the same pass, while a reused lower slot waits for the next tick (`GameTick.cs:73-80,1523-1530`; `SimulationWorld.QueryAndLinks.partial.cs:17-68`; `SimulationWorld.Passes.partial.cs:711-784`).
4. Multi-opoint spread, per-opoint grouping, `AttackExempt`, and pairwise 40-tick vrest are structurally equivalent (`FrameTick.cs:248-330`; `LF2ObjectPointFactory.cs:147-267`).
5. Opoint position, facing modes, `z + 1`, directional `vz`, oid 211 scaling, oid 5/52 override, kind 2 link setup, and parent relation inheritance are mapped (`FrameTick.cs:367-444`; `LF2ObjectPointFactory.cs:482-635`).
6. `FrameTick` counter order is mapped: `AttackExempt` before negative link, then hit stop/fall/hit-state/hit-confirm after the cpoint gate (`FrameTick.cs:24-57`; `LF2Entity.cs:5161-5186`).
7. `next=999`, signed `next` facing flip, state-14 exit hit stop, frame 212 jump velocity, PP spend/display, defend lock, and frame 202 hit stop have matching main branches (`FrameTick.cs:106-215`; `LF2Entity.cs:5227-5270`).
8. `hit_Fa` arithmetic for normal in-range, loaded-data cases is substantially mapped for cases 1, 2, 3, 4, 5, 7, 10, 11, 12, 13, and 14. The exceptions below concern publication, target validity, reset, or missing-resource boundaries rather than the listed velocity constants.

## 3. Confirmed differences

The following are source-proven differences. They do not require interpretation from an older implementation.

### FL-01: held weapons are excluded from frame logic in Unity

- Authority: every active non-character DAT entity with `HitFa > 0` enters `FrameAdvance.FrameLogic`; neither `EntityUsesFrameLogicPass` nor `FrameLogic` excludes `LinkState < 0` (`GameTick.cs:143-153`; `FrameAdvance.cs:49-85`).
- Unity: `LF2WeaponBase.SupportsFrameLogicBeforeAdvancePhase` additionally requires `GetRuntimeHolderEntity() == null` (`LF2WeaponBase.cs:503-507`).
- Effect: a held type 1/2/4/6 object with a nonzero `hit_Fa` skips tracking/spawn/frame-logic behavior in Unity but runs it in the authority. This is directly relevant to hand-attached opoint weapons.

### FL-02: frame-logic source death is not published at the same slot boundary

- Authority: cases 5/6/8/9/11/13 set `entity.Active = false` and decrement `ObjectCount` inside the current producer call (`FrameAdvance.cs:310-312,337-339,387-389,590-592,769-771,801-803,810-812,849-851`). The slot is immediately reusable by a later producer in the same ascending frame-logic pass.
- Unity: the same cases set `Runtime.PendingFlushDestroy = true` (`LF2Entity.cs:1262-1264,1354-1356,1587-1591,1594-1599,1632-1634,1645-1650,1697-1699,1759-1777`). The registry treats it as inactive for queries, but does not release/finalize it until `RunDeferredMutationEntityPass` exits and `FlushPendingEntityDestroy` runs (`SimulationWorld.Passes.partial.cs:19-40`; `SimulationWorld.Registry.partial.cs:328-345`).
- Effect: later same-pass spawns see different free-slot capacity and can select a different slot or fail only in Unity. `ObjectCount` also diverges during the observation window.

### FL-03: `hit_Fa=8` consumes RNG before the authority's data gate

- Authority: resolves oid 225 data once before the loop; missing data deactivates the source with zero spawn RNG calls (`FrameAdvance.cs:330-341`). Only after that does it draw four values per spawn (`FrameAdvance.cs:343-384`).
- Unity: `RunHitFa8FrameLogic` draws vx/vy/vz/target for every requested object before the factory resolves the object definition/config (`LF2Entity.cs:1194-1260`; `LF2ObjectPointFactory.cs:269-320`).
- Effect: absent/incompletely loaded oid 225 changes RNG state and can queue rejected or data-less objects in Unity.

### FL-04: `hit_Fa=6/9` consumes RNG before free-slot and loaded-data success

- Authority: for each target, it first finds a free slot, then (case 9) draws the oid, checks loaded `CharData`, and only then draws velocity values (`FrameAdvance.cs:689-752`).
- Unity: target/oid/velocity RNG is consumed while tasks are built; actual slot allocation and object data resolution happen later during the flush (`LF2Entity.cs:1267-1345`; `LF2ObjectPointFactory.cs:84-120,269-329`).
- Effect: full slots or missing oid 220/221/222 data advance RNG differently, and case 9 loop termination can differ because Unity counts queued tasks rather than successful authority publications.

### FL-05: saved frame-logic targets can resolve inactive/dormant occupants

- Authority: the normal saved target is `ResolveFrameLogicTarget`, which returns only when `world.Objects[slot].Active` (`FrameAdvance.cs:87-89,969-976`). Raw inactive-slot semantics are special handling in cases 4 and 7 only.
- Unity: `ResolveFrameLogicTargetByHitFa` falls back from the active query to `FindEntityByRuntimeSlotIncludingPending`, which also searches dormant/pending registry entries (`LF2Entity.cs:1865-1946`; `SimulationWorld.QueryAndLinks.partial.cs:131-165`).
- Effect: hitFa 1/2/3/11/12/14 can retain an inactive object with positive HP where the authority rescans or kills the source.

### FL-06: inactive-source fields are rewritten before Unity removal

- Authority: frame-logic spawn cases normally deactivate the source without forcing `Hp = 0`; case 11 only sets HP zero when no target exists (`FrameAdvance.cs:590-659`).
- Unity: cases 5/6/8/9/11/13 force HP zero and mark pending destroy (`LF2Entity.cs:1262-1264,1354-1356,1632-1634,1697-1699,1759-1777`).
- Effect: full-slot trace state and any same-pass observer of the dormant source differ even when the final visible object disappears in both implementations.

### FT-01: a wait/next transition executes extra Unity state/frame work immediately

- Authority: `FrameTick.Tick` uses `FrameRuntime.SetFrameImmediate`, which writes only `Frame` and `FrameWaitCounter = 0`; new-frame physics/state callbacks are not invoked there (`FrameRuntime.cs:12-16`; `FrameTick.cs:106-203`).
- Unity: `RunCommonFrameTick` calls virtual `OnFrameTickTransit` (`LF2Entity.cs:5227-5258`). Character transit immediately calls `FrameForce`, state exit/entry, `FrameEvent`, and sound (`LF2LivingObject.cs:282-337`); state exit clears local input state (`LF2Character.cs:1618-1621`). Special-attack transit calls `FrameEvent`, whose frame 15 branch immediately rewrites to frame 1000 (`LF2SpecialAttack.cs:91-100,201-225`).
- Effect: character frame velocity/state effects can occur one logical tick early, local input history can be cleared, and a special attack entering frame 15 does not remain on the authority frame.

### FT-02: missing target frames produce different current-frame state

- Authority: `SetFrameImmediate` always writes the requested integer. Callers then load that frame and return if it is missing (`FrameRuntime.cs:12-16`; `FrameTick.cs:128-139`).
- Unity: `SetFrameTickDirect` refuses to write when `FrameCache` lacks the frame (`LF2Entity.cs:5111-5123`), while virtual `OnFrameTransit` implementations write `Frame.N` before validating and can leave `Frame.D` pointing at the old frame (`LF2LivingObject.cs:282-304`; `LF2SpecialAttack.cs:201-213`; `LF2WeaponBase.cs:460-474`; `LF2OtherObject.ReleaseFlow.partial.cs:7-17`). `RunCommonFrameTick` only checks that `Frame.D` is non-null (`LF2Entity.cs:5249-5258`).
- Effect: Unity either silently stays on the old frame or creates a hybrid new frame ID/old frame data; authority exposes the requested frame with no data and stops that branch. Existing DAT audit differences make this more than a theoretical parser case.

### FT-03: some direct-frame helpers reset `Attacking` when the authority does not

- Authority: `FrameRuntime.SetFrameImmediate` and direct assignments in `FrameAdvance.FrameLogic` do not clear `Attacking`; only surrounding branches that explicitly assign it do so (`FrameRuntime.cs:12-16`; examples `FrameAdvance.cs:63,413,459-469,930,949`).
- Unity: general `ImmediateFrame` clears `AttackingCounter` (`LF2Entity.cs:886-900`; `LF2LivingObject.cs:484-496`), and weapon `SetFrameDirect` clears it (`LF2WeaponBase.cs:896-903`). The weapon boomerang/catch paths use that helper (`LF2WeaponFrameLogicResolver.cs:26-45,67-79`).
- Effect: opoint eligibility (`Attacking == 0`) and wait/next timing can move by a tick after weapon/frame transitions.

### FT-04: `FrameWaitCounter` is absent and conflated with the previous-frame ID

- Authority: `Frame`, `WaitCounter`, and `FrameWaitCounter` are distinct fields; immediate frame writes reset only `FrameWaitCounter` (`NtsdEntityRuntime.cs:117-140`; `FrameRuntime.cs:12-16`).
- Unity: `NTSDEntityRuntime` has `WaitCounter` but no separate `FrameWaitCounter`; `DirectWriteFrameImmediateWaitReset` resets the transistor's `WaitCounter` (`LF2Entity.cs:3665-3668`). The parity snapshot serializes `runtime.WaitCounter` as `frameWaitCounter` (`BattleParitySnapshot.cs:422`).
- Effect: a full-slot trace cannot match both authority fields, and any caller of the Unity “immediate wait reset” helper changes the wrong counter. The currently found non-test caller is stage spawn wiring, which remains outside this report's T8 result.

### OP-01: opoint frame-delay gate reads shell type instead of current DAT type

- Authority: delayed opoint suppression is based on `entity.CharData.ObjType == Character` (`FrameTick.cs:245-246`).
- Unity: it checks `spawner.ObjectType == 0`, which is the pooled subclass type (`LF2ObjectPointFactory.cs:132-145`), despite current-DAT routing being available through `GetCurrentDataObjectTypeForSimulation`.
- Effect: a transformed/shared-DAT shell can publish or suppress an opoint on the wrong tick.

### OP-02: object eligibility is definition-based rather than loaded combat-data based

- Authority: `SpawnFromOpoint` first calls `world.GetChar(op.Oid)` and returns null unless complete combat `CharData` is loaded (`FrameTick.cs:333-350`).
- Unity: `ProcessCreateObject` accepts `GameDataManager.GetObjectById`; missing `CharacterConfig` does not prevent registration, and several initializers tolerate a null wrapper/frame (`LF2ObjectPointFactory.cs:269-329`; `LF2WeaponBase.cs:801-844`; `LF2SpecialAttack.cs:946-977`).
- Effect: Unity can allocate a real slot/render object with null frame data where the authority publishes nothing.

### OP-03: ordinary opoint child holder index differs

- Authority: every opoint child is initialized with `HolderIdx = 0`; kind 2 later overwrites it with the spawner slot (`FrameTick.cs:352-363,422-430`).
- Unity: pooled reset/initialization leaves `Runtime.HolderStableId = -1` for an ordinary child and only writes it for kind 2 (`LF2WeaponBase.cs:412-442`; `LF2ObjectPointFactory.cs:529-545`).
- Effect: any state/interaction path that reads the holder field without first requiring a negative link sees slot 0 in the authority and “none” in Unity.

### OP-04: opoint weapon HP/PP contract is not the authority reset contract

- Authority: `entity.Reset()` supplies HP/HPMax/HP3/PP = 500; opoint spawn stores `WeaponHp` in `Unk31C` and changes HP/PP only for oid 5/52 (`FrameTick.cs:352-419`; `NtsdEntityRuntime.cs:277-306`).
- Unity: `LF2WeaponBase.Reset` zeroes HP/HPBound/HP3/MP/PP, then `InitializeHealth` restores only HP from `weapon_hp`; `LF2Weapon.OnHealthInitialized` separately puts `weapon_hp` into the flight/durability field (`LF2WeaponBase.cs:412-445,831-845`; `LF2Weapon.cs:60-63`).
- Effect: a normal opoint weapon starts with HP=`weapon_hp`, HPBound/HP3/PP=0 instead of 500/500/500/500, while durability is also `weapon_hp`.

### OP-05: slot reuse does not clear the incoming vrest column

- Authority: every opoint/frame-logic spawn calls `world.ResetCooldowns(slot)`, clearing `ARest[slot]`, the slot's whole vrest row, and every other row's entry for that slot (`FrameTick.cs:446`; `FrameAdvance.cs:307,384,587,756,847,912`; `SimulationWorld.QueryAndLinks.cs:12-23`).
- Unity: a new/reset entity clears only its own `LF2ItrRestTracker`; `ReleaseRuntimeSlot` only flips the occupancy bit, and no code removes the reused attacker-slot key from every other entity's tracker (`LF2ItrRestTracker.cs:31-35`; `SimulationWorld.Registry.partial.cs:409-416`).
- Effect: a new occupant can inherit an old occupant's attacker-side vrest in other victims and fail to hit until the stale cooldown expires.

### LC-01: character destruction loses its slot before unregistering

- Unity `LF2Character.OnTransitDestroy` calls `Destroy`, and `Destroy` calls `Reset`; `Runtime.Reset` sets `SlotIndex = -1` (`LF2Character.cs:915-963`; `NTSDEntityRuntime.cs:318-321`). Only afterward does renderer release call `LF2ObjectRenderer.ResetState`, which unregisters the logic object (`LF2ObjectRenderer.cs:195-200`; `LF2ObjectPool.cs:170-183`).
- `SimulationWorld.ReleaseRuntimeSlot` therefore receives `-1` and cannot clear the old `_runtimeSlotUsed[slot]` bit (`SimulationWorld.Registry.partial.cs:409-416`).
- Authority `FreeEntity(slot)` resets the fixed entity and preserves that slot as free (`SimulationWorld.Registry.cs:75-85`).
- Effect: each destroyed character can permanently consume one runtime slot until a world reconstruction.

### LC-02: generic free invokes effects/events that authority `FreeEntity` does not

- Authority invalid-frame/state cleanup calls `world.FreeEntity(slot)`, which only resets the fixed slot (`GameTick.cs:1530-1555`; `SimulationWorld.Registry.cs:75-85`).
- Unity `HandleLateFrameTickExit` and state-9998 cleanup call `FreeEntityLikeExe -> OnTransitDestroy` (`SimulationWorld.Passes.partial.cs:372-381,822-863`; `LF2Entity.cs:3644-3647`). That route invokes `DestroyEvent`; special attacks call `CreateBrokenEffect`, and weapons can queue broken sound (`LF2Entity.cs:944-959`; `LF2SpecialAttack.cs:83-109,194-197,860-864`; `LF2WeaponBase.cs:448-454,958-961`).
- Effect: invalid/state-cleanup frees can create extra combat objects or sound events in Unity.

### LC-03: whole-world runtime reset does not reset the entity registry

- Authority `ResetBattleRuntime` resets all 400 entities, all cooldown matrices, object count, tick/flow, and world state (`SimulationWorld.Passes.cs:12-96`).
- Unity `SimulationWorld.ResetRuntimeState` resets only `BattleRuntimeState`, RNG, and pending sounds (`SimulationWorld.Registry.partial.cs:102-108`). It does not clear buckets, `_runtimeSlotUsed`, pending unregister/destroy queues, or entity trackers.
- Effect: reusing the same `SimulationWorld` for a deterministic restart/rematch can retain entities, occupied slots, and cooldown state.

### LC-04: formal reset defaults differ

- Authority reset defaults include `KnockbackVx/Vy/Vz = 0.1` and `HolderCopy = 99` (`NtsdEntityRuntime.cs:85-102,160-195`).
- Unity generic runtime reset writes knockback accumulators to 0 and holder-copy slot to -1 (`NTSDEntityRuntime.cs:318-470`, specifically `:382,419-421`).
- Effect: a freshly reset/spawned object hit before the first postprocess can produce a different averaged knockback; full-slot trace defaults also differ unless synthesized by the trace writer. Some special merge/split paths manually compensate, but generic reset does not.

### LC-05: normal pooled character reset starts with a 10-tick delay

- Authority `Entity.Reset`/`SpawnAt` leaves `FrameDelay = 0` (`NtsdEntityRuntime.cs:117-140`; `SimulationWorld.Registry.cs:45-72`).
- Unity `LF2Character.Reset` performs `Runtime.Reset()` and then assigns `FrameDelay = 10`; normal `ModuleBind/Initialize` does not unconditionally restore it to zero (`LF2Character.cs:915-936,966-1018`). Opoint initialization does overwrite it from the task.
- Effect: a normal character obtained from the logic pool can skip frame advance/frame tick for ten ticks unless its caller performs an extra override.

## 4. Trace-required risks

These are real structural risks, but a production input/DAT trace is needed before calling the branch reachable in the deployed battle set.

1. **R-FL-01, non-heavy `WeaponState 1002/2000`:** authority applies the 1002 -> 2000 -> 3000 damping to every runtime weapon (`FrameAdvance.cs:66-81`); Unity applies it only when `LF2WeaponBase.IsHeavy` is true (`LF2WeaponFrameLogicResolver.cs:34-45`). Trace any non-heavy DAT that writes those states.
2. **R-FL-02, shell/current-DAT mismatch in boomerang prelogic:** authority tests runtime identity plus current `EntityType` (`FrameAdvance.cs:58-64`); Unity's special weapon prelogic exists only on the weapon subclass resolver. Trace identity-transform paths into type 4/6.
3. **R-FL-03, raw inactive case-4 side effect:** authority case 4 can write `CatchTimer = 100` into an inactive fixed slot after using its reset coordinates/HP (`FrameAdvance.cs:394-415`); Unity emulates raw coordinates but has no placeholder object on which to write the timer (`LF2Entity.cs:1361-1386`). This is definitely trace-visible if the geometry passes, but later spawn reset may erase it before active behavior.
4. **R-LC-01, character frame snapshot residue:** `LF2Character.Reset` does not clear `Frame.PN/Prev/Prev2/Prev2D` or `FrameCache`, while authority reset clears previous-frame fields. Opoint initialization overwrites them; normal pooled reuse does not overwrite every field (`LF2Character.cs:915-991`). Trace a despawned character reused as a normal player/reserve entity.
5. **R-LC-02, pending-unregister stable-id alias:** after a mid-pass unregister releases `SlotIndex`, `GetRuntimeSlotOrder` falls back to `StableId`; `FindEntityByRuntimeSlotIncludingDormant` can then expose the pending object under that numeric key (`SimulationWorld.Registry.partial.cs:60-65,257-271`; `SimulationWorld.QueryAndLinks.partial.cs:141-165`). A trace must show a stable ID colliding with a queried runtime slot.
6. **R-FT-01, event-driven state extras beyond frame 15:** weapon/character/special `OnFrameTransit` invokes local state events absent from `FrameTick.Tick`. Frame 15 is a confirmed concrete mismatch (FT-01); other state-specific extra writes require per-DAT transition traces.

## 5. Required differential scenarios

The minimum trace suite for this partition should include:

1. Held weapon with `hit_Fa > 0`, proving FL-01.
2. Two frame-logic producers near slot exhaustion, where the first self-deactivates and the second spawns, proving FL-02 and RNG/slot order.
3. `hit_Fa=8` with oid 225 unavailable, and `hit_Fa=9` with no free dynamic slot, comparing RNG call count and state.
4. Saved target pointing to a dormant/pending occupant for hitFa 1 and 12.
5. Character `wait/next` transition whose destination has nonzero `dvx/dvy/dvz`, and special attack transition into frame 15.
6. Transition to an absent frame ID, checking `Frame`, resolved frame data, attacking, sound, and cleanup tick.
7. Slot 50: victim records vrest for attacker 50; attacker is freed; a new attacker reuses 50; verify both vrest directions are zero before its first collision.
8. Ordinary kind-1 opoint weapon: assert holder index, HP, HPMax/HP3/PP, durability, frame/wait counters, and slot.
9. Destroy and respawn more than 350 opoint characters, checking `_runtimeSlotUsed` exhaustion and first-free slot.
10. Reset/replay on the same Unity `SimulationWorld`, checking 400-slot occupancy, rest state, RNG, tick, and object count.

## 6. Counts and status

- Fully audited authority methods in this partition: **25/25** (4 `FrameTick`, 15 `FrameAdvance`, 6 runtime/wrapper/transistor methods), plus the direct reset/registry/cooldown/world-reset methods listed in scope.
- Confirmed difference clusters: **20** (`FL-01..06`, `FT-01..04`, `OP-01..05`, `LC-01..05`).
- Additional trace-required risk clusters: **6**.
- Confirmed-equivalent boundary groups: **8**.
- This is a static audit result, not a runtime parity certificate. None of the confirmed differences in this report should be marked aligned until Unity compiles, focused self-checks pass, and the listed dual-end traces agree.

## 7. Final Unity SHA verification

The following SHA-256 values were read after the audit and after this report was written. Any later changed hash invalidates only the conclusions for that changed file and requires a targeted reread.

| Unity file | SHA-256 |
|---|---|
| `Animation/Character/FrameTransistor.cs` | `f1f9d6ef14fc87a23e83b6d4e910d395e8cedcf7238857dee95dba46641df1c8` |
| `Animation/Character/LF2ItrRestTracker.cs` | `3e8d402a73e2bcc04766e1969104aa944022b0e9cbc958eff64a79e0f944c23c` |
| `Animation/Character/LF2ObjectPointFactory.cs` | `d5193ff8b69b3412ef6cb222d3337aaefcb369d4bd2a99a9ff464d26c227bbee` |
| `Animation/LF2Objects/LF2Entity.cs` | `96736d966cbf535bdf76baffd62767fca08e988496f61360cf033ed76cd5aa6a` |
| `Animation/LF2Objects/LF2LivingObject.cs` | `48e46afd3701f8d1bfe536c1c0108972a3dd9a335da02929cad39a22b122994e` |
| `Animation/LF2Objects/LF2Character.cs` | `79fb778665e0ff58f5c2c29314d8cb66265fabf2e1d15f24eaee93bd3a2fb513` |
| `Animation/LF2Objects/LF2WeaponBase.cs` | `7428073b659d83b8b474cbf1fa65dec57e2bbc2b42a4f5531d3bc95fe663636d` |
| `Animation/LF2Objects/LF2Weapon.cs` | `41bb9c4024fbe7e7c02641a2e71458e4c3fbacd3736bbb4d78569e3a8a688a03` |
| `Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs` | `ead07751a82e66cc5fcf30c9c04c6e5e32fc3dbe79f13053cff236fc4af8dde5` |
| `Animation/LF2Objects/LF2SpecialAttack.cs` | `5c66cdedb5145bafaeeb6516cadfe2522920c7f8c544a53a5f564c6839534363` |
| `Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs` | `35a3d59b6873da25176d3b1a10d31e0c0ff0d1f6b89697277cfc830b452e2fd6` |
| `Animation/LF2Objects/LF2OtherObject.ReleaseFlow.partial.cs` | `610955b162700b02a0fcd73f4b33344ba9f6a2d9acb5c8b7517ab0ebce5078d1` |
| `Animation/LF2Objects/LF2ObjectRenderer.cs` | `9b0ad9f1dfe58b15780f2f570753f1b90dab0324b04e46f86c339cbdaa07ea2c` |
| `Animation/LF2Objects/LF2ReferencePool.cs` | `d2ceb57a31344f0da09650e8b5c3b74880140f01c7712c872aa3ea3601015a28` |
| `Animation/LF2ObjectPool.cs` | `615593fd47b9122215aade264edd0123e00b17fbc712b10813c2a0a38eaa7898` |
| `Simulation/NTSDEntityRuntime.cs` | `e7221acbff411f54838154913ebbda68bdbc24bce2ed9a9143ece22883c56d0c` |
| `Simulation/NTSDBattleTickSystem.cs` | `be6aa699906761b3996667feeb91d4c5229f7f8a7393c12594550fd0d739af9c` |
| `Simulation/SimulationWorld.Passes.partial.cs` | `a6a17ab6b1aae9937a103e90e8aea5c51a464c97252c5fad5a5d92036438d2bd` |
| `Simulation/SimulationWorld.Registry.partial.cs` | `9218388ecfa4e4e95f073b209fea85834c38e14b229b9882a6cbdf72ccd6bd7b` |
| `Simulation/SimulationWorld.QueryAndLinks.partial.cs` | `9c70b31ac462ef392f864e8409285c30e7c6a405d2ecd796700b3a31a4c6345e` |
| `Simulation/BattleParitySnapshot.cs` | `41b3571ac9959eaf5b1f9b09d98202b0b889d6f9d91a9272e64b25c4b7cbc08a` |
