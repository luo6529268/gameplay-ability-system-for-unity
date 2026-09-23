<!-- CHANGE-RECORD
id: NTSD28-USER-CORE-MOTION-OUTPUT-RATIO-001
status: FOCUSED_TEST_PASS
change-kind: USER_APPROVED_ALL_ENTITY_MOTION_OUTPUT_RATIO_CORE_SLICE
code-path: Assets/NTSD/Scripts/Animation/Character/CharacterMechanics.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterActionResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterStateResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameAdvancePass.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCharacterActionWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28FixedViewRunRatioEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0GroundBuiltinsEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeType0AirDashRedirectBuiltinsEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CanonicalCharacterPhysicsTailEditorTests.cs
authority: user D-024 all-battle-entity screen-travel-fraction decision; formal physics_integrator.cpp integrates raw motion before one-unit friction
evidence: artifacts/diagnostics/NTSD28-USER-ALL-ENTITY-MOTION-RATIO-001/INITIAL-INVENTORY.md
-->

# NTSD28-USER-CORE-MOTION-OUTPUT-RATIO-001

Status `FOCUSED_TEST_PASS / RUNTIME_PENDING`, created before script modifications. User explicitly forbids DAT data changes without a new request. Exact Task contract: `docs/ai/TASKS/NTSD28-USER-CORE-MOTION-OUTPUT-RATIO-001.md`.

Before: run/dash source writers multiply stored velocity on configured Worlds, and both character/non-character shared physics integrates unscaled velocity. Consequently the current run slice changes friction output and future velocity readers. Formal `PhysicsIntegrator28::step` first adds raw `motion.x/z` to precise position and later damps stored motion by one. Unity has the same order, so the ratio exception belongs at the final position delta.

After intended: all declared run/dash writers store raw motion; shared character and non-character X/Z integration applies a world-owned one-time position-delta multiplier while friction, gravity, action timing and DAT bytes remain raw. Two direct pre-integration non-character X/Z adds use the same world factor. Y, spawn/held/teleport and geometry need later declared packages. Expected side effects: actual world X/Z positions and thus collision/AI/stage arrival change under the user-approved fixed-view exception; logical velocity and friction remain formal. No other module or Scene changes.

Validation planned: original Editor compile; configured full-tick character and non-character position/velocity focused tests; current run/dash suites; DAT status and Scene SHA; Ledger validator and diff check. Runtime Play and formal EXE visible ratio remain pending until separately recorded. Rollback by inverse patch of this Change's exact modifications only.

Implemented: `CharacterMechanicsContext` now carries immutable X/Z multipliers with identity defaults. `StepBattleLogic`, `StepNonCharacterBattleLogic` and `WeaponDynamics` multiply only integrated X/Z position deltas after existing block decisions, before raw-velocity friction. The three character production callers and two non-character callers pass the World factors; the shared non-character identity-X and type-3 hit-j-Z direct additions use their respective factors. Legacy/shared/Native run, heavy-run and dash writers have been restored to raw authored velocity on all declared paths; the early-scaling helpers in `LF2Entity` were removed. Y altitude, spawn/held/teleport/other direct movement remain unscaled and are not covered by this Change. Existing Q07 edits inside `SimulationWorld.cs` and unrelated diagnostics in the dirty tree were preserved.

Focused original Unity Editor results after refresh/domain reload: `NTSD28FixedViewRunRatioEditorTests` 4/4 PASS (job `8723c1370ff44b969065ee7eb7f56528`), Native air/dash builtins 11/11 PASS (`181e65c0e238498f9279e4de8a8852ce`), ground builtins 13/13 PASS (`fc40ff3878ff46739995b56cbed8b75a`), default-scale shared type1 reference 4/4 PASS (`39d4832b64664acd9f40de92c081ba4a`), and default-scale type0 physics core 5/5 PASS (`48693125fbdf4224a370b43b53c7d130`). An earlier incorrectly namespaced shared-type1 filter yielded zero tests (job `23f44909b2f649bf8d3de9d88476493e`); the corrected 4/4 job above is the evidence. The new mechanics tests show configured 18 X / 3.3 Z and -12 X / -4.5 Z gain the target position factors while stored post-friction motion remains 17 / 2.3 and -11 / -3.5, respectively. These are focused mechanics steps, not a full SimulationTickDriver or real Play proof. Stage collision, attached objects, non-integrator writers, Y and alternate aspects remain open.

Validation amendment before editing the Q06 fixture: add the declared test path solely to run a configured-world production physics pass in both Legacy and Native/ECS modes, asserting resulting position and stored raw velocity. This keeps the earlier fixture definitions and all default-scale source cases unchanged. Do not report it as a complete Driver tick or Battle Scene Play witness.

Production pass test chronology: first original-Editor run `d4850b5526b94689b4e62ad3e517d043` failed because the new fixture expected attacking counter zero; the live state correctly retained nine. After correcting only that fixture expectation and refreshing the original Editor, the new test passed 1/1 (`c8464a55ea2a4f1d8a36c1af2ea4e962`) and its complete adjacent class passed 3/3 (`73faaf0f543d44ca93f8ecb51c7fb63a`). It asserts configured X/Z travel and unchanged raw post-friction speed through both Legacy and Native/ECS production physics owners. This is not a full release core tick.

Next test-only amendment before edit: the same declared Q06 fixture may add one `NTSDBattleTickSystem.RunReleaseTick` configured-world case, retaining the earlier production physics-pass test. A later pass might change the terminal entity state, so preserve any real first difference and adjust only after tracing ownership.

Core tick chronology: first configured `RunReleaseTick` job `e0b61ae535594293a28c72e96b971f85` reached the stage-boundary pass and clamped fixture Z=100 to the scene minimum 180, proving that physics-only Z=100 was not a valid full-tick sample. Moving only the fixture start to legal Z=250 let the position assertion pass; job `8d690d49ca584e188773abb51e8de384` then exposed that later core phases increment the attack counter 9→10. After recording that core-tick expectation, job `b3766fafe2164ff1a9184a49dbfc5c8c` exposed the expected resource-phase increment 0→1, which was separately asserted only for full tick. The final original-Editor core-tick job `87a7cee42fff4f4d97010c02e07a1096` passed 1/1 and the whole adjacent class job `6973793e87de426d845b8650062edb56` passed 4/4. The original physics-only check still asserts its phase remains unchanged. This is a complete `NTSDBattleTickSystem` core tick on a synthetic World, not a live `SimulationTickDriver`/Battle Scene Play.

Before next test edit: extend only the declared Q06 fixture with one production-materialized non-character type-1 weapon case under configured scales. Run its actual non-character physics pass, checking position and raw motion. Keep weapon frame/landing semantics and authored DAT fixture raw; this is an additional caller witness, not all-entity completion.

The configured type-1 weapon test passed 1/1 in the original Editor (job `e5279a84494743079e17e45da450389c`). It materializes a synthetic weapon through the production logic factory and runs the production non-character physics pass; X/Z displacement follows configured viewport ratios while raw Vx/Vz stay unchanged in this airborne sample. The earlier synthetic full core tick passed 1/1 (`87a7cee42fff4f4d97010c02e07a1096`), and its then-adjacent class passed 4/4 (`6973793e87de426d845b8650062edb56`). No real Battle Scene or formal EXE ratio comparison follows from either test.

An attempt to run the whole adjacent class passed an unsupported `filter` field to the local test bridge; the bridge ignored it and started the full EditMode catalog (job `5192eb9e273741398e88a56379e3e9ff`, 8269 discovered). Its unrelated failures are not attributed to this Change. A cancellation attempt through the bridge did not establish terminal status; do not cite that broad run as a focused result or start concurrent tests before the current job is terminal.
