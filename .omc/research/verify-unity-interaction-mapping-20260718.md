# Independent Verification: Unity Interaction Mapping (2026-07-18)

## Verdict

The 105 base authority contract IDs are set-complete: the authority ledger and Unity mapping ledger each contain the same 105 unique `INT-*` base IDs (PASS 5, COL 12, HIT 31, CP 9, WPN 10, OP 14, STG 12, RST 6, STAT 3, SND 3). The advertised status count also mechanically equals 35 `equivalent` plus 70 `Unity-adapter`.

That set equality does **not** validate the mapping result. Two production-reachable normal-battle differences were found below. Therefore the mapping ledger's totals `confirmed-difference = 0`, `missing = 0`, and "212 accounted identities" are not valid as a parity conclusion.

## Findings

### P1 - IronBall preprocessing is applied to DAT type 6 (Drink) instead of DAT type 2 (IronBall/HeavyWeapon)

- Authority: `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\HitResolve.cs:235-239` halves the cloned itr `Dvx` and `Dvy` only when `victim.CharData.ObjType == WeaponType.IronBall`. `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Common\NtsdConstants.cs:27-35` defines `IronBall = 2` and `FlyingB = 6`.
- Unity: `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs:632-642` checks `LF2ObjectType.Drink` (enum value 6) before dividing `itr.dvx` and `itr.dvy`. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectType.cs:18-34` defines `HeavyWeapon = 2` and `Drink = 6`.
- Production reachability: every Character/current-Character-DAT/object consume loop calls `ResolveRuntimeItrForPair` before dispatch (`LF2CharacterInteractionResolver.cs:52-70`, `LF2CharacterDatInteractionResolver.cs:81-99`, `LF2WeaponInteractionResolver.cs:54-71`). Both type-2 heavy weapons and type-6 drinks are production DAT categories.
- Result: type-2 victims receive unhalved knockback input, while type-6 victims are incorrectly halved. This changes hit reaction/velocity and may change subsequent frame and collision results.
- Ledger impact: `INT-HIT-005` is a `confirmed-difference`, not `Unity-adapter`. Its statement that the IronBall half-scaling is preserved is contradicted by source.
- Minimal correction/test: change the gate to current DAT `HeavyWeapon`/value 2. Add one type-2 and one type-6 preprocess case asserting the runtime clone's `dvx/dvy`, the authored itr remains unchanged, and RNG state is unchanged.

### P1 - Late opoint spawn uses the spawner's floating presentation/runtime position instead of the authority integer spawn coordinates

- Authority: `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Frame\FrameTick.cs:381-394` computes `spawnXInt` and `spawnYInt` from `spawner.XInt/YInt`, then writes both child `X/Y` and `XInt/YInt` from those integers. Only Z preserves the spawner's floating value (`:385,389-390`).
- Unity: `Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs:190-202,229-236` builds `task.pos` from `spawner.PS.x/PS.y` and invokes `ProcessCreateObject` without setting `useDirectRuntimePosition` or integer X/Y. Weapon initialization then calls `SetPos(task.pos.x, task.pos.y, task.z)` at `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs:781-787`; special attacks likewise copy `task.pos` at `LF2SpecialAttack.cs:1268-1280`.
- Production reachability: `SimulationWorld.Passes.partial.cs:740-818` calls `ProcessOpointSpawnAlignedToCpp` in the normal late-entity pass, and authored frame opoints enter this method directly.
- Result: whenever a spawner has a fractional X or Y, the child retains that fraction instead of being placed at the authority integer coordinate. Even where truncation initially yields the same `XInt/YInt`, logical X/Y and next-tick physics differ; when adding the opoint offset crosses zero, the integer coordinate can differ immediately as well.
- Ledger impact: `INT-OP-002` is a `confirmed-difference`, not `Unity-adapter`. Its claim of direct runtime position parity is false for the late opoint path. `INT-OP-001` also inherits this incorrect spawn result.
- Minimal correction/test: compute late-opoint child X/Y from `Runtime.XInt/YInt`, pass them through the direct runtime/int-position task contract, and keep child Z as `spawner.Runtime.Z + 1`. Add positive, negative, and cross-zero fractional-spawner cases, checking X/Y doubles, XInt/YInt, Z, velocity, slot, and next-tick snapshot.

### P2 - The 212 semantic-identity completeness claim is not independently auditable

- Mechanical evidence: the authority ledger contains 105 unique base IDs but only 22 explicitly named sub-identity table rows matching `.B*`, `.E*`, `.R*`, or `.S*`. The totals 6/27/42/22/25/39/31/20 are prose partition totals which sum to 212; the remaining identities do not have stable individual IDs or individual Unity mappings.
- Consequence: set equality proves only the 105 grouped contracts. It cannot prove that every branch, RNG call, and side effect inside a large family such as `INT-HIT-006`, `INT-HIT-023`, `INT-OP-002`, or `INT-STG-003` is carried. The two P1 findings demonstrate the failure mode: a row can exist and be marked adapter while a contained field/type branch is wrong.
- Required evidence: assign a stable identity to each counted branch/RNG/side effect, with authority line, Unity line, status, and focused assertion/trace. Until then, `212 accounted identities` should be stated as an author estimate, not verified coverage.

### P2 - Some mapping evidence cites dead target code rather than the production route

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs:154-159` routes every current Character-DAT victim to `LF2CharacterDatHitResolver.TryResolveHit`.
- Repository-wide references to `LF2CharacterHitResolver` find construction only (`LF2Character.cs:56,101`) and no production call to its `ResolveHit`; its approximately 760-line settlement implementation is currently unreachable.
- The mapping ledger nevertheless defines U-HIT-C as `LF2CharacterHitResolver.cs:34-760` and repeatedly cites "concrete counterpart" and concrete/shared comparisons as production mapping evidence (`INT-HIT-006`, `010`, `013-015`, `025-029`). The actual production proof must be grounded in `LF2CharacterDatHitResolver`, not in the dead resolver or tests that invoke it directly.
- This does not by itself establish another runtime difference, because the shared resolver is active, but it weakens the claimed 31/31 hit proof and should be corrected in the ledger/reverse scan.

### P2 - Target-only reverse classification is incomplete/misstated

- `SimulationWorld.Passes.partial.cs:1232-1255` contains a `Mode2Request == 2` branch that writes every weapon candidate's `WeaponFlightCounter = -1`. No non-test production setter was found; classify it as Unity-only, production-unreachable in the current repository. It is absent from UONLY-INT-001..015.
- UONLY-INT-011 says Unity maintains an entity tracker "plus raw runtime-slot matrices". No raw ARest/VRest matrix exists in production Unity source; `LF2ItrRestTracker.cs:19-20` is the storage, and `SimulationWorld.QueryAndLinks.partial.cs:17-39` projects row/column clearing by walking trackers. This can still represent the authority matrix, but the stated dual-storage evidence is factually wrong.
- `SimulationWorld.cs:23-38` queues the authority-shaped sound event and directly calls `SoundPlayer.PlaySfx`. This is a production-reachable Unity-only presentation adapter and should be listed as such rather than described as playback occurring outside `QueueSound`. No simulation/RNG write was found, so it is not counted as a battle-logic difference.

## Scope-excluded ledger classification errors

Per the task scope, debug step-wait, F8 weapon drop, `RunMode2RandomWeaponDrop`, and `InitStats` are not production battle backlog. They should therefore not be used toward the in-scope 105/212 closure:

- Authority `GameTick.cs:114-125` has the debug step-wait early return; Unity `NTSDBattleTickSystem.cs:67-76` has no equivalent gate. This is `scope-excluded`, not an in-scope adapter under `INT-PASS-001`/`INT-STG-012`.
- Authority `GameTick.cs:679-725` implements F8 drop between Character and object hit loops; production Unity has no F8 runtime branch (only `Assets/NTSD/Scripts/Test/WeaponSpawner.cs`). `INT-OP-008` is `scope-excluded`, not mapped by `RandomWeaponDropTickAll`.
- Authority `GameTick.cs:727-797` iterates mode2 candidates in `LoadedOidOrder` and leaves the reset spawn frame at 0. Unity `SimulationWorld.Passes.partial.cs:1263-1354` enumerates numeric OIDs and selects a flight frame. `INT-OP-009/010` are scope-excluded differences, not equivalent adapters.

These rows still matter to ledger arithmetic: either remove them from the in-scope totals or mark them explicitly `scope-excluded`; otherwise 105/212 overstates the requested interaction closure.

## Partition Sampling Result

- Collision collect: pair order, bidirectional vrest decrement, nearest/kind1 tie RNG sites, strict overlap, Z width, state3005/kind8 defer, and legacy kind filters were source-checked. No additional confirmed normal-production difference was found. The type-2/type-6 preprocess defect occurs after collection.
- Hit resolve: consume slot re-resolution, dispatch kinds, standard/alternate stats, kind14, kind10/11, kind15/16, hit-record RNG, type3 tails, and link-release rests were sampled. The confirmed defect is the IronBall type gate above. Dead concrete resolver evidence remains a verification gap.
- CPoint: kind1 then kind2 ordering, double cpoint sync boundary, signed actions, injury/stat writes, throw, and identity replacement were sampled with no additional confirmed difference.
- Weapon/wpoint: slot-order held scan, consume-before-pose, release, drink branches, kind3 RNG order, cover, and throw categories were sampled with no additional confirmed difference.
- Opoint/lifecycle: first-free high slot, child identity/link/rest reset, multi-spawn spread/rest, transition RNG, and publication timing were sampled. The confirmed coordinate defect is above.
- Stage: phase/producer order, factor, slot range, spawn RNG order, HP/team/facing, refill, and phase advance were sampled with no additional confirmed difference.
- Rest/stats/sound: vrest orientation, arest rule, reset-on-reuse, kill/damage stat conditions, and queue order were sampled. The storage/playback descriptions need correction as noted, but no additional combat-state difference was established.

## Residual Verification Boundary

This was a read-only static audit. No code, tests, main documents, Unity compilation, self-check, or Play Mode scenario was changed/run. The existing aggregate self-check PASS cannot close the two P1 gaps because its IronBall preprocess coverage is absent and its late-opoint fixtures use integer source positions. Cross-partition authority dependencies INT-DEP-001..008 and real-scene data reachability remain open; no whole-battle parity conclusion is made.
