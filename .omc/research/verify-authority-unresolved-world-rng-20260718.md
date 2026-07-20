# Authority-Unresolved Verification: World Slots and RNG

Date: 2026-07-18  
Scope: code-level battle runtime contracts only. No Play Mode or scene/presentation verification.

Authority: `J:\\QQFile\\NTSD2.4\\ntsd_release_C#`

## Result Summary

| ID | Scope | Result | Classification |
|---|---|---|---|
| `DEP.WORLD.01` | world slot allocation, VRest/ARest ownership and reset | Slot ranges and cooldown ownership are equivalent on reachable production paths. The stage-rest preservation boundary is already the separately tracked `F5` item; it is not a new unresolved difference. | **Close as equivalent/Unity-adapter; retain F5 separately** |
| `DEP.RNG.01` | RNG algorithm, seed ownership and call routing | The LCG algorithm and `%` range behavior match. Seed ownership/lifetime differs by design: C# uses one process-level static stream while Unity uses one stream per `SimulationWorld` and seeds it during world reset/configuration. | **Unity-adapter / policy-open**: retain the per-world stream for lockstep unless an authority-compatible process-stream policy is explicitly chosen |

## `DEP.WORLD.01`

### Authority contract

- `BattleCore/Simulation/SimulationWorld.cs` allocates `Objects` as a fixed 400-slot array and initializes every slot once.
- `SimulationWorld.Registry.cs:39-47` (`Spawn`) scans `Objects[0..399]` in ascending order and delegates to `SpawnAt` at the first inactive slot.
- `SimulationWorld.Registry.cs:50-79` (`SpawnAt`) calls `Entity.Reset()`, then writes identity, position and slot fields. `Entity.Reset()` resets only the entity runtime; world-level cooldown matrices are owned by `SimulationWorld`.
- `SimulationWorld.Registry.cs:81-92` (`FreeEntity`) resets the entity and preserves its slot index. The explicit world cooldown cleanup is `SimulationWorld.QueryAndLinks.cs:12-23` (`ResetCooldowns`), which clears `ARest[slot]` and both VRest row/column directions.
- Stage immediate spawning in `Simulation/GameTick.cs:2079-2147` scans slots `20..399` in ascending order. Frame/effect/opoint producers independently use their documented `50..399` dynamic range (for example `FrameAdvance.cs` dynamic scans and `GameTick.cs:1875+`).
- `SimulationWorld.Passes.cs:13-103` resets all world arrays, including `VRest`, `ARest`, battle-slot arrays, and every fixed entity slot.

### Unity mapping

- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:25-45` creates 400 raw runtime slots; `SimulationWorld.Registry.partial.cs:96-104` creates the world and RNG; the slot capacity is `MaxRuntimeSlots = 400`.
- `SimulationWorld.Registry.partial.cs:500-531` (`AllocateRuntimeSlot`) uses type semantics: ordinary entities start at slot `0`, dynamic entities start at `DynamicRuntimeSlotStart = 50`, and an explicit `RequiredRuntimeSlot` wins. This maps the authority's ordinary, opoint/effect and stage ranges.
- `SimulationWorld.StageWave.partial.cs:434-435` requests the first free slot in `20..399` for stage immediate entries, matching the authority stage range. `RestoreStageSpawnRestState` is called only for the StageSpawnAt semantic.
- `SimulationWorld.QueryAndLinks.partial.cs:17-45` (`ResetCooldownsForRuntimeSlot`) clears the reused occupant's ItrRest and removes the freed slot from every other entity's VRest map. This is the object/component form of clearing the authority row and column.
- `SimulationWorld.Registry.partial.cs:138-149` (`ResetRuntimeState`) resets registered objects, raw runtime slots, and world runtime state. `Register` at lines `334-357` resets a newly allocated slot's raw state and cooldowns for all non-stage registrations.
- `LF2Entity.UnregisterFromWorld` and `SimulationWorld.UnregisterImmediate` release the runtime slot; `LF2Entity.FreeEntityLikeExe` routes pooled renderers through the same unregister path.

### Determination

For ordinary registration, dynamic object/opoint registration, stage immediate spawn, and slot reuse, the production Unity path preserves the authority's slot ordering and VRest/ARest ownership. Therefore `DEP.WORLD.01` is no longer an unresolved contract.

The one deliberate lifecycle difference is stage-spawn rest capture/restore (`CaptureRawRestSlotState`/`RestoreStageSpawnRestState`). This was introduced for the separately tracked `F5` stage-spawn-rest contract and must remain recorded there; it should not be counted again as a second world-slot difference.

## `DEP.RNG.01`

### Algorithm equivalence

- Authority `BattleCore/Common/NtsdRng.cs:7-23`:
  - state update: `Seed = Seed * 0x343FDu + 0x269EC3u` (uint wraparound);
  - result: `(Seed >> 16) & 0x7FFF`;
  - `Srand` resets `Seed` and `CallCount`.
- Unity `Assets/NTSD/Scripts/Simulation/DeterministicRng.cs:7-63` implements the same uint LCG, the same 15-bit result and call counting. `NextInt(a,b)` uses `NextRaw() % (b-a)`, matching the authority call sites that use `Rand() % range`.

No algorithm or range arithmetic difference was found.

### Seed ownership/lifetime difference

- Authority RNG is `public static class NtsdRng`; every `NtsdRng.Rand()` call in `InputRuntime`, `CollisionCollect`, `HitResolve`, `FrameAdvance`, and `GameTick` advances the one process-level stream. The authority source contains the `Srand` definition but no in-repository call site; `ResetBattleRuntime` does not call `Srand` (`SimulationWorld.Passes.cs:13-16` and the complete reset body through line 103).
- Unity stores `public DeterministicRng Rng { get; private set; }` on each `SimulationWorld` (`SimulationWorld.Registry.partial.cs:89-104`). Entity calls route through `LF2Entity.BattleRandInt` (`LF2Entity.cs:793-801`) to `Match.Rng`; only an unregistered/no-driver object uses the static `FallbackRng`.
- Unity `SimulationWorld.ResetRuntimeState` explicitly calls `Rng.Seed(0x4E545344u)` (`SimulationWorld.Registry.partial.cs:138-149`). `SimulationTickDriver.ApplyMatchConfig` then seeds the same per-world stream with `config.seed` (`SimulationTickDriver.cs:263-284`). Thus reset/rematch introduces a deterministic seed boundary absent from the authority reset method, and two Unity worlds do not share RNG state while the authority static stream would be shared.
- Unity's `BattleParityTraceEditor` also explicitly seeds the world stream for trace scenarios; this is a test/trace adapter and does not remove the production ownership difference.

### Determination

`DEP.RNG.01` is an intentional Unity-adapter/policy-open boundary, not an unclassified production defect: per-call arithmetic matches authority, while Unity keeps ownership local to `SimulationWorld` to preserve deterministic lockstep isolation. A future authority-compatible process-stream policy would be a separate decision. No production code was changed during this verification.

## Evidence Boundary

This report only closes code-level contracts. It does not evaluate Naruto skills, renderer positions, sprite pivots, camera presentation, or any other Play Mode behavior. Those remain the user's separate verification scope.
