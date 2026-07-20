# Unity framework mapping independent verification (2026-07-18)

## 1. Scope and verdict

- Authority: `J:\QQFile\NTSD2.4\ntsd_release_C#` only.
- Audited inputs: all 172 unique `FW-*` IDs in `csharp-authority-framework-ledger-20260718.md`, their 172 Unity mappings in `unity-framework-mapping-ledger-20260718.md`, and a reverse scan of the Unity production framework paths named by that mapping.
- This is a read-only source audit. No production code, test, or main alignment document was changed.
- Unity-native `GameObject`, `Transform`, renderer, pool, scene lifecycle, fixed-world camera, and DAT-loading carriers were accepted when they do not change battle runtime truth.
- Default `stage.dat` deployment remains deferred. Stage runtime code is still in scope and can be verified with an in-memory fixture.

The mapping ledger is structurally complete (172/172 IDs, no duplicate or missing ID), but its status summary overstates differences. The corrected ID classification is:

| Status | Mapping ledger | Verified |
|---|---:|---:|
| equivalent | 64 | 64 |
| Unity-adapter | 51 | 57 |
| confirmed-difference | 21 | 13 |
| authority-unresolved | 8 | 8 |
| scope-excluded | 28 | 30 |
| total | 172 | 172 |

Eight IDs must be downgraded: `FW-BS-002`, `FW-BS-003`, `FW-BS-004`, `FW-BS-005`, `FW-WR-003`, and `FW-WR-006` to `Unity-adapter`; `FW-WR-006-E` and `FW-LC-009` to `scope-excluded`. The remaining 13 difference IDs collapse to seven independent root causes, not five.

## 2. Verdict on the ledger's five root clusters

| Ledger cluster | Verdict | Production reachability |
|---|---|---|
| C1 bootstrap/registry/roster/initial prime | **Partially true; must split.** Four real roots exist: initial stage-wave advance, roster slot/team mapping, initial spawn position/RNG consumption, and initial `HitStop`/velocity prime. The loader rejection, seed-source API, failed-parse registry residue, background fallback, and generic CLR pool shape are adapters or invalid-resource boundaries. | The four real roots are reachable in ordinary battle setup; stage-wave is conditional on campaign data. |
| C2 whole-battle reset/RNG continuation | **Rejected for the current scope.** `ResetRuntimeState()` reseeds to a constant, but `ApplyMatchConfig()` overwrites it with the match seed before any RNG consumption (`SimulationTickDriver.cs:268-280`). The remaining divergence is new-battle/rebootstrap/rematch continuation, which is an explicitly excluded host boundary. | Not reachable during an already-running ordinary battle. |
| C3 stage spawn inherits generic cooldown reset | **Confirmed.** Unity `Register()` resets the allocated slot's rest state for every entity (`SimulationWorld.Registry.partial.cs:330-350`); authority `SpawnAt()` does not (`SimulationWorld.Registry.cs:50-78`). Stage spawn uses that generic Unity registration (`SimulationWorld.StageWave.partial.cs:429-480,600-652`). | Reachable when a later stage spawn reuses a slot carrying old ARest/VRest. An unused clean slot masks the bug. |
| C4 results-active still executes normal passes | **Confirmed.** Authority increments the header, clears transient globals, then returns through results-only logic (`GameTick.cs:32-50`). Unity has no `Results.IsActive` gate and continues frame, collision, stage, late, and tail passes (`NTSDBattleTickSystem.cs:17-30,32-77`). | Reachable on the tick after `ActivateSummary`; Unity summary activation itself is production wired. |
| C5 hit-candidate carrier cleanup one tick late | **Confirmed as runtime-state parity, lower gameplay severity.** Authority tail calls `ClearHitCandidateCarriers()` (`GameTick.cs:1897-1956`), which resets transient scratch, `HitConfirm2`, and abort state (`Entity.cs:151-156`). Unity tail does not clear it (`SimulationWorld.Passes.partial.cs:890-939`); `HitConfirm2` is cleared at the next candidate collection (`BruteForceSceneQuery.cs:236-254`). | Reachable after weapon/special hit paths set `HitConfirm2`. No current Unity gameplay reader was found between ticks, so the proven impact is runtime/checksum state, not a presently demonstrated move outcome. |

## 3. Confirmed deduplicated roots

### F1. Unity advances the initial stage wave while authority remains at `-1`

- Authority bootstrap writes `WaveIdx=-1` (`DirectBattleBootstrap.cs:62-73`). Its normal stage-advance helper returns while `WaveIdx<0` (`GameTick.cs:2317-2357`), so the formal chain never enters wave 0.
- Unity configures `-1` and immediately calls `StartInitialStageWave()` (`SimulationTickDriver.cs:285-292`); that method increments to 0 (`SimulationWorld.StageWave.partial.cs:69-83`).
- IDs: `FW-BS-008`; the same root is also referenced by `FW-LC-004`.
- Fix boundary: remove the extra production bootstrap advance. Do not add a default `stage.dat`; verify with an in-memory campaign.

### F2. Unity compresses the eight roster slots and stores a non-normalized team

- Authority preserves every original index in its fixed eight-slot config and maps team `0` to `10 + slotIndex` (`DirectBattleBootstrap.cs:27,34-35,85-104,126-132`).
- Unity skips inactive entries and writes active players to a sequential `writeIndex` (`BattleRuntimeState.cs:203-226`). `AppManager` normalizes independent team `-1` to `10+i` for the entity (`AppManager.cs:183-208`), while the roster retains raw `-1`.
- The mismatch is not merely cosmetic: roster binding requires `candidate.Team == rosterSlot.Team` (`SimulationWorld.FrameInput.partial.cs:180-190`). Independent-team players therefore cannot take the normal unbound roster match path; compressed holes also change the public player-slot contract.
- ID: `FW-BS-008-B1`.
- Fix boundary: preserve original 0..7 indices and normalize roster team with the same original-index rule used for the entity.

### F3. Initial character spawn position and RNG consumption differ

- Authority consumes two RNG calls per valid initial character and writes `x=width/4 + Rand()%(width/2)`, `z=Rand()%range+zMin` (`DirectBattleBootstrap.cs:106-133`).
- Unity reads scene spawn transforms and consumes no battle RNG (`AppManager.cs:176-224`). This changes both initial X/Z and every downstream RNG result.
- ID: `FW-BS-008-B2`.
- This is not the accepted fixed-world camera adapter: scene position is written into simulation truth (`lf2.PS.x/z`).
- Fix boundary: use authority RNG and stage runtime bounds for logical X/Z; a scene transform may remain a presentation locator only.

### F4. Initial character `HitStop` and velocity prime are missing

- Authority initializes `HitStop=75`, `Vx=Vz=0.1`, and `Vy=0` (`DirectBattleBootstrap.cs:224-242`).
- Unity runtime reset uses `HitStop=0`, `Vx=Vy=Vz=0` (`NTSDEntityRuntime.cs:393-426`), and `LF2Character.Initialize()` only initializes health/input/rest (`LF2Character.cs:1035-1048`).
- `HitStop` is battle-significant: authority candidate collection rejects ordinary hits while victim `HitStop != 0` (`CollisionCollect.cs:96-104`), and frame tick decrements it (`FrameTick.cs:52-53`).
- ID: `FW-BS-009`.
- Narrowing: difficulty HP bonus is a no-op for the normal reset value because it is capped by `Hp3=500`; RespawnCount and input cooldowns are already zero. Only the missing non-default prime is confirmed here.

### F5. Stage spawn incorrectly clears reused-slot ARest/VRest

- Evidence and reachability are in C3 above.
- IDs: `FW-WR-005`, `FW-TK-028`, `FW-H-050`, `FW-H-059`, `FW-LC-004`.
- Fix boundary: separate logical registration from rest reset and select the reset policy by spawn semantic. Keep reset behavior on authority paths that explicitly clear cooldowns; preserve it on stage `SpawnAt`.

### F6. Results-active lacks the authority early return

- Evidence and reachability are in C4 above.
- IDs: `FW-TK-002`, `FW-END-002`.
- Fix boundary: after the tick header and results input observation boundary, execute only the in-scope results state update and return before cooldown/frame/collision/stage/late/tail.

### F7. Post-frame transient carrier cleanup occurs at the next collect

- Evidence and reachability are in C5 above.
- IDs: `FW-TK-034`, `FW-H-042`.
- Fix boundary: clear `HitConfirm2` at entity post-frame tail. Unity's local abort bool and candidate-cache representation may remain adapters; do not introduce unused authority storage solely for structural similarity.

## 4. False positives and accepted adapters

| Item | Verified classification | Reason |
|---|---|---|
| `loadedChars<=0` startup rejection (`FW-BS-002`) | Unity-adapter | Resource/bootstrap failure handling, not a valid loaded battle's rule. Unity manager/pool failure behavior should be tested separately but does not establish ordinary combat divergence. |
| environment/system-tick seed source (`FW-BS-003`) | Unity-adapter | `MatchConfig.seed` is a host input carrier. With the same seed, the mechanism is equivalent; the real RNG divergence is F3's missing spawn consumption. |
| failed DAT parse leaves allocated registry entry (`FW-BS-004`, `FW-WR-003`) | Unity-adapter | Invalid-resource loader residue and explicitly accepted DAT-reading adaptation. It is not reached by a valid loaded character path. |
| background fallback/direct two-player convenience chain (`FW-BS-005`) | Unity-adapter | Host/resource carrier. Logical bounds may come from Unity scene configuration. Scene spawn position is separately rejected in F3 because it writes battle truth. |
| generic reset/pool shape (`FW-WR-006`) | Unity-adapter | Raw 400 slots plus pooled CLR shells are allowed. No additional in-battle result difference was established beyond the separately listed roots. |
| reset RNG continuation (`FW-WR-006-E`, `FW-LC-009`) | scope-excluded | Only survives after `ApplyMatchConfig` at new-battle/rematch/rebootstrap boundaries; host rematch is excluded. Reclassify if that scope is restored. |
| scene polygon walkability reverse finding | unreachable/dormant adapter | `IsGroundPointWalkable` is captured (`LF2Character.cs:898-905`, `LF2Entity.cs:4980-4995`), but `CharacterMechanics.Step()` never reads `ctx.isPointWalkable` (`CharacterMechanics.cs:147-213`). It currently cannot roll back or block movement. |
| fixed-world camera | accepted adapter | User-specified; authority `CameraX` has no in-scope ordinary battle consumer in this framework inventory. |
| pending destroy, dormant merge shell, `Suppress*UntilTick` | accepted adapter, still test-sensitive | They model Unity object lifetime and same-pass visibility. No additional mismatch was proven in the framework audit; interaction/frame ledgers must continue to own their per-spawn timing checks. |

## 5. Reverse Unity-only audit

The reverse scan covered the production-only branches listed in section 10 of the mapping ledger plus their callers. It found no additional framework root beyond F1-F7.

- Driver pause, accumulator/backlog, GameObject pooling, pending-destroy slot release, dormant merge shell, direct stage factory fallback, and renderer `LateUpdate` are Unity adapters.
- Fixed-world camera and ordinary audio are accepted/excluded by scope.
- `UnbindWorld/RecreateWorld`, full snapshot apply/rollback, host rematch, F1/F8, step-wait, and Mode2 debug behavior remain excluded.
- Scene walkability must be removed from the confirmed-difference backlog unless a future production change starts calling the callback from mechanics.

The eight `FW-X-*` handoff IDs remain `authority-unresolved` in this framework report by design; they must be closed by the frame/input, interaction, and stage-parser inventories, not silently upgraded here.

## 6. Required verification after fixes

1. Bootstrap fixture: eight config slots with holes and an independent team; assert original roster indices, normalized teams, entity binding, human polling, `HitStop=75`, `Vx/Vz=0.1`, authority X/Z, and exact RNG call count.
2. Stage fixture without default `stage.dat`: assert pre-first-tick `WaveIdx=-1`; separately force a later stage spawn into a reused slot with nonzero ARest and both VRest directions, and assert they remain unchanged.
3. Results fixture: activate summary, run one tick, and assert header tick advances while frame, position, HP/PP, rest, collision, opoint, stage state, and late/tail fields do not.
4. Carrier fixture: cause weapon and special hit paths to set `HitConfirm2`; assert it is visible during the same interaction phase and zero before post-tick checksum/observer capture.
5. Run fresh Unity compile and full `BattleRuntimeSelfCheck`; then run Play Mode with a normal two-player setup and an independent-team/hole roster setup. Static mapping alone is not completion evidence.

## 7. Completion boundary

This report verifies the framework ledger only. It proves that the existing `21 confirmed-difference / 5 roots` conclusion is not reliable as written and replaces it with `13 difference IDs / 7 roots`. It does not claim whole-battle alignment: the eight handoff IDs and the independent frame/input and interaction audits must be integrated, fixes must be implemented, and fresh Unity runtime evidence must pass before any completion claim.
