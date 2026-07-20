# NTSD battle parity trace architect review (2026-07-17)

## Scope and conclusion

Reviewed only the C# authority at `J:\QQFile\NTSD2.4\ntsd_release_C#` and the Unity parity infrastructure. Default `stage.dat` was not read or treated as a prerequisite.

The current `ntsd-battle-trace-v2` design is useful as a first-difference detector, but it is not yet a sound full-state parity certificate. An `equal` result from the current comparator would prove equality only for the two producers' reported hashes over their current projections. It would not prove that every authoritative battle field was observed, that the payload actually matches the reported hashes, or that bootstrap/input semantics were equivalent.

The sample scenario is expected to differ before meaningful skill logic is reached unless the P0 bootstrap/projection issues below are resolved. This is a valid diagnostic result and must not be hidden.

## P0 findings

### P0-1: Unity world projection conflates stage width with `BoundRight`

Authority bootstrap loads District `Bg.Width = 960`, while `BoundRight` remains the independent default `800`. The existing authority tick-1 trace confirms `runtime.stage.width = 960` and `runtime.stage.boundRight = 800`.

Unity `BattleParitySnapshot.ProjectWorldDomain()` emits both `width` and `boundRight` from `BattleStageRuntimeState.StageWidthPx`. Bootstrapping the proposed fixture with `SetSceneSnapshot(960, 450, 525, ...)` therefore emits `boundRight = 960`, guaranteeing a tick-1 `world` mismatch even when stage collision width is otherwise correct.

Required action: represent and bootstrap independent `BoundLeft`/`BoundRight` values, or project a genuine existing runtime field. Do not change District width to 800 to make the hash pass; spawn/clamp logic uses the 960 width.

### P0-2: first-tick battle-entry behavior is not equivalent

Authority `DirectBattleBootstrap.InitializeFromConfig()` sets `NeedClearInput = true`. `GameTick.Run()` advances global phase fields, clears `NeedClearInput`, calls `ClearBattleEntryInput`, and returns before input, frame advance, collision, random drop, late update, and results on tick 1.

Unity `NTSDBattleTickSystem.RunReleaseTick()` has no corresponding battle-entry early return. It advances flow and executes all frame/interaction/cleanup phases. Unity's world projection currently hardcodes `needClearInput = false`, so the missing state is concealed rather than compared.

Required action: bootstrap and consume the battle-entry gate with the same pass boundary. A runner-only skipped tick is not acceptable because the production tick behavior is what must be measured.

### P0-3: `VRest` canonical axes are transposed

Authority trace generation iterates `world.VRest[first, second]` and emits the first index as `attackerSlot` and the second as `victimSlot`. Authority battle code predominantly stores cooldown as `VRest[victimSlot, attackerSlot]`; the v2 labels are therefore historically inverted, but they define the current schema bytes.

Unity iterates actual attacker then actual victim and reads `victim.ItrRest.GetVrest(attacker)`, emitting `{ attackerSlot: attacker, victimSlot: victim }`. This transposes every non-symmetric vrest entry relative to the authority v2 trace. The same issue applies to full row-major output if rows are built by actual attacker.

Required action: either preserve v2 byte compatibility by projecting Unity rows in authority matrix order (documenting the legacy label inversion), or correct both producers and bump the schema. Do not silently change only one producer.

### P0-4: the slot/world projection has authoritative blind spots

Authority slots are reflection-projected from all public members of `NtsdEntityRuntime`. Unity manually substitutes constants or aliases for authoritative fields, including:

- `FrameWaitCounter` aliases `WaitCounter`; `SuppressJumpInit` and `JumpInitPending` are always false.
- `PickupCount`, `ReleaseTick`, and `StuckVictimSlot` are constants.
- hit-candidate arrays and `Mp2/Mp3/Mp4` are constants.
- `AbortRemainingHitPairs`, block flags, `Unk318`, `Unk31C`, `Unk324`, and `Unk33C` are constants.
- world pause/step flags, `NeedClearInput`, reserve state, and all result fields are constants.

Several of these fields change battle control flow. Equality while they remain hardcoded can be a false positive when Unity carries a divergent value that is not projected, and can be a false negative when Unity has an equivalent value under a different runtime field.

Required action: produce a field-contract table for every authority runtime/world member: exact Unity source, explicit proven derivation, or unsupported. Unsupported battle fields must block a full-parity claim. Constants are acceptable only with an invariant check that fails if the Unity state departs from the constant.

### P0-5: comparator trusts producer hashes without binding them to payload

`TraceCompareCommand` compares the reported hash strings only. It does not recompute `input/rng/world/slots/aRest/vRest/stats/events`, and even `--detail full` does not compare payloads when hashes happen to match. A stale hash, serialization bug, or incorrect Unity projection can therefore return `equal` for different tick payloads.

Required action: for certificate runs, require full traces and recompute every domain hash from a normative projection, then compare full payload nodes as well. Compact traces may remain a diagnostic format, but must not be the sole completion evidence. `overall` must be recomputed from the eight recomputed domain hashes.

### P0-6: bootstrap must mirror `DirectBattleBootstrap`, not generic Unity match setup

Authority bootstrap consumes exactly two RNG calls per successfully spawned active slot for initial X/Z, after the scenario seed is applied. The sample header proves seed `305419896` becomes state `3768712380` with call count `4`. Spawn order is player-slot order and runtime slots are allocated in authority order.

Generic Unity `ApplyMatchConfig()` resets/reseeds the world, refreshes stage data from the scene, can load/start stage campaigns, and writes `Match.Seed = scenario.seed`. Authority `DirectBattleBootstrap` leaves its projected runtime match seed at `0` in the current sample. Calling generic setup after fixture injection will therefore change RNG/stage/world bytes.

Required action: implement an explicit parity bootstrap adapter whose ordered effects are checked against `DirectBattleBootstrap`: reset, globals, roster, RNG draws, spawn, prime frame/position, battle stats, and runtime sync. No Unity scene callback or async load completion may consume battle RNG.

## P1 findings

### P1-1: canonical JSON implementations need a cross-runtime conformance suite

Authority hashes `System.Text.Json` output; Unity uses a custom writer. Ordinary integers and current doubles are likely compatible, but escaping and numeric edge cases are not proven. `System.Text.Json` escapes some non-ASCII and HTML-sensitive characters differently, while Unity emits printable ASCII directly and lowercase `\u` escapes. Unity also promotes `float` to `double` before `"R"` formatting, which can expose extra digits compared with serializing a float directly.

Add golden vectors generated by the authority serializer for control characters, non-ASCII text, quotes/backslashes, `<>&+'`, `-0`, float/double extrema, arrays, and sorted dictionaries. The Unity serializer must match byte-for-byte, not merely parse to equivalent JSON.

### P1-2: input packets are sparse updates, not a complete all-player snapshot

Both scenario providers include only listed players. In authority, an included player is polled before `GameTick`, while later authority input processing may poll zero input again depending on `HumanInputPolledExternally`; the existing sample trace shows tick-2 Left ending with `keyLeft = 0`, `prevLeft = 1`, and no leftward motion. Unity enqueues seven held-state events for an included player and consumes them during its post-cooldown pass. This is not automatically equivalent.

Treat the authority trace as definitive: verify press, hold, release, omitted-player, simultaneous buttons, and duplicate player entries. Reject duplicate `(tick, playerSlot)` entries during scenario validation or define deterministic last-wins behavior on both sides. The current authority provider preserves duplicate list order, so Unity must not silently deduplicate.

### P1-3: header comparison is incomplete

The comparator checks schema, manifest digest, projected scenario, and stage fixture only. It ignores `loadedChars`, `maxRuntimeSlots`, `rngAfterBootstrap`, `buttonMask`, and `detail`. These are part of the v2 header contract and should be compared (with an intentional compact/full policy for `detail`). At minimum, a bootstrap RNG mismatch must fail at the header before tick comparison.

### P1-4: scenario validation is insufficient

Authority validation does not reject duplicate player slots in roster, duplicate player inputs within a tick, out-of-range input player slots, invalid button bits, missing/nonexistent OIDs, invalid stage index, or active slots that fail to spawn. Such cases can produce traces whose scenario says one thing and world bootstrap does another.

Reject invalid scenarios symmetrically before either side runs and include the successfully bound roster mapping in the header.

### P1-5: manifest equality is a prerequisite, not proof of runtime parity

The existing data audit reports 137 authority OIDs, with only 34 equal, 66 different, and 37 missing Unity files; its battle-logic manifest differs. If the Unity runner deliberately loads external authority DAT files, its manifest must still be computed from Unity's resolved runtime data using exactly the same battle-logic projection. Copying the authority digest or hashing source bytes would defeat the parser/conversion check.

Manifest mismatch should stop the certificate comparison but still produce a separate data-difference report. It must not be bypassed to reach tick hashes.

## Acceptance evidence

A specific scenario may be described as "逐 tick 一致" only when all of the following fresh evidence exists:

1. Both traces identify `ntsd-battle-trace-v2` (or a deliberately bumped schema), the same validated scenario, explicit stage fixture state, button map, runtime slot count, loaded/bound roster, manifest, and post-bootstrap RNG state.
2. Unity's battle-logic manifest equals the authority manifest from independently resolved Unity runtime data. No default `stage.dat` is read when `stageFixture` is null.
3. Bootstrap audit matches spawn success/order, runtime-slot mapping, X/Z, HP/PP, initial frame/link state, and RNG state/call count before tick 1.
4. Every authority world/runtime field has a reviewed projection contract. No unverified hardcoded battle field remains in a domain used for a full-parity claim.
5. Full 400-slot, full ARest, and full 400x400 VRest payloads are emitted for certificate runs. The comparator independently recomputes all domain and overall hashes and deep-compares payloads.
6. Every tick index is contiguous and equal, both streams end together, and all domains match on every tick.
7. The scenario is long enough to cross the behavior under test. Six idle/movement ticks can certify only that exact six-tick bootstrap/input path, not all battle logic.
8. Focused scenarios cover at least entry tick, press/hold/release/combo input, AI RNG, movement/bounds, opoint lifecycle, hit/rest, held weapon/throw, catch/link, death/respawn/results, transform/current-DAT, and explicit stage fixture progression (when T8 is resumed).
9. Unity compile is 0 errors, `BattleRuntimeSelfCheck` passes, and the same concrete scenario is reproduced in Play Mode where presentation behavior is in scope.

Until these conditions hold, report results as "trace runner operational; first difference at ..." or "this scenario's observed domains match", not "all battle logic is fully aligned".
