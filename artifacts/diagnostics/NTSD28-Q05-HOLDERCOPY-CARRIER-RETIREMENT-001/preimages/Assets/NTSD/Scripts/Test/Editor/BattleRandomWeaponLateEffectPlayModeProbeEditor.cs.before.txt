#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Live/current-catalog and explicit-fixture S4 probe for natural random
    /// weapon drop, late transform chain, state9996 children, and exhaustion.
    /// Alignment contract: R8-LATEPLAY-001.
    /// </summary>
    public static class BattleRandomWeaponLateEffectPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Validation/R8/Run Random Weapon Late Effect Play Probe";
        private const string ResultRelativePath =
            "Temp/NTSD_R8_WP01C_06_RandomWeaponLateEffect.result.json";
        private const int TickTimeoutEditorUpdates = 1800;
        private const int DynamicSlotStart = 50;

        private static readonly List<LF2Entity> OwnedFixtures =
            new List<LF2Entity>(4);
        private static readonly List<LF2Entity> OwnedSpawnedEntities =
            new List<LF2Entity>(12);
        private static readonly List<PendingSoundEvent> BaselineSounds =
            new List<PendingSoundEvent>(16);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2ObjectPool objectPool;
        private static ProbeResult result;
        private static uint baselineRngState;
        private static ulong baselineRngCalls;
        private static bool previousPaused;
        private static bool dependenciesResolved;
        private static bool pauseRequested;
        private static bool baselineCaptured;
        private static bool running;
        private static int editorUpdates;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            ResetProbeState();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            result = new ProbeResult
            {
                status = "RUNNING",
            };
            running = true;
            EditorApplication.update += Observe;
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying)
            {
                Fail("Play Mode or production world ended before completion.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > TickTimeoutEditorUpdates)
            {
                Fail("Timed out waiting for a safe live-world boundary.");
                return;
            }

            if (!dependenciesResolved)
            {
                driver = SimulationTickDriver.Instance;
                world = driver?.World;
                objectPool = LF2ObjectPool.Instance;
                if (driver == null || world == null || objectPool == null ||
                    LF2ReferencePool.Instance == null ||
                    world.RuntimeDataCatalog?.IsReady != true)
                {
                    return;
                }

                previousPaused = driver.IsPaused;
                result.startTick = driver.CurrentTickIndex;
                result.workerWasActive =
                    driver.DedicatedSimulationWorkerActiveForDiagnostics;
                dependenciesResolved = true;
            }
            if (driver.DedicatedSimulationWorkerFailureForDiagnostics != null)
            {
                Fail(
                    "The production simulation worker failed: " +
                    driver.DedicatedSimulationWorkerFailureForDiagnostics);
                return;
            }

            if (!pauseRequested)
            {
                if (driver.CurrentTickIndex <= 0 || world.ObjectCount <= 0 ||
                    world.ClaimedRuntimeSlotCountForDiagnostics <= 0)
                {
                    return;
                }

                driver.SetPaused(true);
                pauseRequested = true;
                CaptureBaseline();
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;

            try
            {
                result.matrix = ExecuteMatrix();
                FinishSuccess();
            }
            catch (Exception exception)
            {
                Fail("Unhandled probe exception: " + exception);
            }
        }

        private static MatrixEvidence ExecuteMatrix()
        {
            NaturalRandomEvidence natural = ExecuteLiveNaturalRandom();
            LateChildrenEvidence liveLate = ExecuteLiveState9996();
            SyntheticChainEvidence synthetic = ExecuteSyntheticFullChain();
            ExhaustionEvidence exhaustion = ExecuteExhaustionFixture();
            return new MatrixEvidence
            {
                natural = natural,
                liveLate = liveLate,
                synthetic = synthetic,
                exhaustion = exhaustion,
            };
        }

        private static NaturalRandomEvidence ExecuteLiveNaturalRandom()
        {
            int weaponCount = CountCurrentRandomWeaponCandidates(world);
            Require(weaponCount < 4,
                $"The live scene already has {weaponCount} random-drop weapons; " +
                "a natural positive witness is not isolated.");

            int freeSlot = FindFirstFreeSlot(world);
            Require(freeSlot >= DynamicSlotStart,
                "The live world has no dynamic slot for natural random drop.");

            int seed = FindSeedForFirstRemainder(200, 0);
            var expectedRng = new DeterministicRng(seed);
            Require(expectedRng.NextInt(0, 200) == 0,
                "Natural random gate seed is invalid.");

            var candidates = new List<int>();
            var seen = new HashSet<int>();
            IReadOnlyList<ObjectDefinition> definitions =
                world.RuntimeDataCatalog.ObjectDefinitions;
            for (int index = 0; index < definitions.Count; index++)
            {
                ObjectDefinition definition = definitions[index];
                if (definition == null || !seen.Add(definition.id))
                    continue;
                int oid = definition.id;
                if (oid < 100 || oid >= 200)
                    continue;
                if (world.RuntimeDataCatalog.GetCharacterConfig(oid) == null)
                    continue;
                if (oid == 122 || oid == 123)
                {
                    if (expectedRng.NextInt(0, 2) == 0 ||
                        (world.BattleGameModeId >= 1 &&
                         world.BattleGameModeId <= 4))
                    {
                        continue;
                    }
                }
                candidates.Add(oid);
            }
            Require(candidates.Count > 0,
                "The current runtime catalog has no natural random-drop candidate.");

            int selectedIndex = expectedRng.NextInt(0, candidates.Count);
            int selectedOid = candidates[selectedIndex];
            BattleStageRuntimeState stage = world.Runtime?.Stage;
            int xMaxOverride = stage?.XMaxOverride ?? 0;
            int stageWidth = stage?.BaseStageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            int r1 = expectedRng.NextInt(0, 30);
            int xBase = xMaxOverride == 0 ? stageWidth - 60 : xMaxOverride - 60;
            int r2 = expectedRng.NextInt(0, 30);
            int r3 = expectedRng.NextInt(0, 30);
            int zBase = zMax - zMin - 60;
            int r4 = expectedRng.NextInt(0, 30);
            int expectedX = r1 * (xBase / 30) + r2 + 30;
            int expectedZ = r3 * (zBase / 30) + r4 + zMin + 30;

            world.Rng.Seed(seed);
            int beforeObjects = world.ObjectCount;
            world.RandomWeaponDropTickAll(driver.CurrentTickIndex + 600);
            LF2Entity spawned = world.FindEntityByRuntimeSlotForQuery(freeSlot);
            Require(spawned != null && world.ObjectCount == beforeObjects + 1,
                "Natural random drop did not materialize at the lowest free slot.");
            OwnedSpawnedEntities.Add(spawned);
            Require(
                spawned.ObjectId == selectedOid &&
                spawned.Frame.N == 0 &&
                spawned.Runtime.XInt == expectedX &&
                spawned.Runtime.YInt == -500 &&
                spawned.Runtime.ZInt == expectedZ &&
                Math.Abs(spawned.Runtime.Vx) < 0.0001 &&
                Math.Abs(spawned.Runtime.Vy) < 0.0001 &&
                Math.Abs(spawned.Runtime.Vz) < 0.0001,
                $"Natural random selected OID/slot/position/velocity mismatch: " +
                $"cppCandidateCount={candidates.Count},cppSelected={selectedOid}," +
                $"cppIndex={selectedIndex},cppPos=({expectedX},-500,{expectedZ})," +
                $"unityOid={spawned.ObjectId},unitySlot={spawned.Runtime.SlotIndex}," +
                $"unityFrame={spawned.Frame.N},unityPos=({spawned.Runtime.XInt}," +
                $"{spawned.Runtime.YInt},{spawned.Runtime.ZInt}),unityVel=(" +
                $"{spawned.Runtime.Vx:R},{spawned.Runtime.Vy:R}," +
                $"{spawned.Runtime.Vz:R}),cppCandidates=" +
                string.Join(",", candidates));
            Require(
                spawned.Health.HP == (selectedOid == 122 ? 200 : 500) &&
                spawned.Health.HPBound == 500 &&
                spawned.Health.HP3 == 500 &&
                spawned.Health.PP == 500 &&
                spawned.KillCount == -1 &&
                world.Rng.State == expectedRng.State &&
                world.Rng.CallCount == expectedRng.CallCount,
                "Natural random stats or RNG call ordering mismatch.");

            var evidence = new NaturalRandomEvidence
            {
                weaponCountBefore = weaponCount,
                candidateCount = candidates.Count,
                candidates = candidates.ToArray(),
                selectedIndex = selectedIndex,
                selectedOid = selectedOid,
                slot = freeSlot,
                x = spawned.Runtime.XInt,
                y = spawned.Runtime.YInt,
                z = spawned.Runtime.ZInt,
                rngCalls = world.Rng.CallCount,
                passed = true,
            };

            spawned.FreeEntityLikeExe();
            world.FlushPendingDestroyForDiagnostics();
            Require(world.FindEntityByRuntimeSlotForQuery(freeSlot) == null,
                "Natural random cleanup did not release its slot.");
            return evidence;
        }

        private static LateChildrenEvidence ExecuteLiveState9996()
        {
            Require(
                world.RuntimeDataCatalog.GetCharacterConfig(217)?.characterData != null &&
                world.RuntimeDataCatalog.GetCharacterConfig(218)?.characterData != null,
                "Current runtime catalog is missing OID217 or OID218.");

            LF2CharacterData spawnerData = BuildData(
                "R8C06_Live9996",
                LF2ObjectType.Character,
                9996,
                0);
            var spawner = RegisterOwned(new ProbeCharacter(
                "R8C06_Live9996",
                8400,
                spawnerData));
            spawner.SetPosition(100, -20, 200);
            spawner.AttackingCounter = 1;
            int spawnerSlot = spawner.Runtime.SlotIndex;
            int firstFree = FindFirstFreeSlot(world);
            Require(firstFree >= DynamicSlotStart,
                "The live world has no slot for state9996 children.");

            const uint seed = 0x13572468u;
            var expectedRng = new DeterministicRng(seed);
            ChildExpectation[] expected = new ChildExpectation[5];
            for (int index = 0; index < expected.Length; index++)
                expected[index] = NextChildExpectation(expectedRng, index, spawner);

            world.Rng.Seed(seed);
            int objectsBefore = world.ObjectCount;
            world.RunLateStateSpecialPreCollisionForSelfCheck(spawner);
            Require(world.ObjectCount == objectsBefore + 5 &&
                    world.Rng.CallCount == 34 &&
                    world.Rng.State == expectedRng.State,
                "Live state9996 did not create five children with 34 RNG calls.");

            var children = new ChildEvidence[5];
            for (int index = 0; index < children.Length; index++)
            {
                int slot = firstFree + index;
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(slot);
                Require(child != null,
                    $"Live state9996 child {index} missing at slot {slot}.");
                OwnedSpawnedEntities.Add(child);
                ChildExpectation item = expected[index];
                int expectedOid = index == 4 ? 218 : 217;
                Require(
                    child.ObjectId == expectedOid &&
                    child.SpawnerEntityIndex == spawnerSlot &&
                    child.Runtime.XInt == item.x &&
                    child.Runtime.YInt == item.y &&
                    child.Runtime.ZInt == item.z &&
                    Nearly(child.Runtime.Vx, item.vx) &&
                    Nearly(child.Runtime.Vy, item.vy) &&
                    Nearly(child.Runtime.Vz, item.vz) &&
                    child.Frame.N == item.frame &&
                    child.Runtime.Dir ==
                        (item.facing == 0 ? "right" : "left") &&
                    child.AttackExempt == 6 &&
                    child.Team == 0 && child.RelationTeam == 0 &&
                    child.HolderCopySlot == 99 && child.KillCount == -1,
                    $"Live state9996 child {index} field/RNG ordering mismatch.");
                children[index] = Child(child);
            }

            return new LateChildrenEvidence
            {
                spawnerSlot = spawnerSlot,
                firstChildSlot = firstFree,
                childCount = children.Length,
                rngCalls = world.Rng.CallCount,
                children = children,
                passed = true,
            };
        }

        private static SyntheticChainEvidence ExecuteSyntheticFullChain()
        {
            const int sourceOid = 782;
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            var definitions = new List<ObjectDefinition>();
            AddConfig(sourceOid, LF2ObjectType.Character, 9995, 0);
            AddConfig(50, LF2ObjectType.Character, 4900, 0);
            AddConfig(900, LF2ObjectType.Other, 8901, 0);
            AddConfig(901, LF2ObjectType.Character, 9996, 0);
            AddWeapon(217);
            AddWeapon(218);

            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            var fixtureWorld = new SimulationWorld(
                BattleRuntimeProfile.Authority400,
                SimulationWorld.AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                resolver);
            fixtureWorld.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(
                    oid,
                    out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            fixtureWorld.LogicReferencePool.Prewarm(
                LF2ObjectType.LightWeapon,
                8);
            fixtureWorld.LogicReferencePool.PrewarmTasks<OPointCreateTask>(2);
            fixtureWorld.SetLogicOnlyEntityMaterialization(true);

            var spawner = new ProbeCharacter(
                "R8C06_SyntheticChain",
                sourceOid,
                wrappers[sourceOid].characterData);
            spawner.SetRequiredRuntimeSlot(0);
            spawner.SetPosition(100, -20, 200);
            spawner.AttackingCounter = 1;
            fixtureWorld.Register(spawner);
            fixtureWorld.Rng.Seed(0x24681357u);
            fixtureWorld.RunLateStateSpecialPreCollisionForSelfCheck(spawner);

            Require(
                spawner.ObjectId == 901 &&
                spawner.Frame.N == 0 &&
                spawner.Frame.D?.state == 9996 &&
                spawner.Runtime.RenderPicOffset == 140 &&
                spawner.AttackingCounter == 1 &&
                fixtureWorld.ObjectCount == 6 &&
                fixtureWorld.Rng.CallCount == 34,
                "Synthetic 9995→4000→8000→9996 chain mismatch.");
            for (int index = 0; index < 5; index++)
            {
                LF2Entity child = fixtureWorld.FindEntityByRuntimeSlotForQuery(
                    DynamicSlotStart + index);
                Require(child?.ObjectId == (index == 4 ? 218 : 217),
                    $"Synthetic chain child {index} OID/slot mismatch.");
            }

            return new SyntheticChainEvidence
            {
                finalOid = spawner.ObjectId,
                finalState = spawner.Frame.D.state,
                renderPicOffset = spawner.Runtime.RenderPicOffset,
                childCount = fixtureWorld.ObjectCount - 1,
                rngCalls = fixtureWorld.Rng.CallCount,
                passed = true,
            };

            void AddConfig(
                int oid,
                LF2ObjectType type,
                int state,
                int weaponHp)
            {
                LF2CharacterData data = BuildData(
                    $"R8C06_Config_{oid}",
                    type,
                    state,
                    weaponHp);
                wrappers[oid] = new LF2CharacterDataWrapper(oid, data);
                definitions.Add(new ObjectDefinition(
                    oid,
                    (int)type,
                    "r8-c06-fixture.dat"));
            }

            void AddWeapon(int oid)
            {
                LF2CharacterData data = BuildWeaponData(
                    $"R8C06_Weapon_{oid}",
                    oid);
                wrappers[oid] = new LF2CharacterDataWrapper(oid, data);
                definitions.Add(new ObjectDefinition(
                    oid,
                    (int)LF2ObjectType.LightWeapon,
                    "r8-c06-fixture.dat"));
            }
        }

        private static ExhaustionEvidence ExecuteExhaustionFixture()
        {
            LF2CharacterData data = BuildData(
                "R8C06_Exhaustion",
                LF2ObjectType.Character,
                9996,
                0);
            var fixtureWorld = new SimulationWorld(
                BattleRuntimeProfile.Authority400,
                SimulationWorld.AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                new RuntimeCharacterConfigResolver());
            var spawner = new ProbeCharacter(
                "R8C06_ExhaustionSpawner",
                8500,
                data);
            spawner.SetRequiredRuntimeSlot(0);
            spawner.AttackingCounter = 1;
            fixtureWorld.Register(spawner);
            for (int slot = DynamicSlotStart;
                 slot < SimulationWorld.AuthorityRuntimeSlotCapacity;
                 slot++)
            {
                var blocker = new ProbeCharacter(
                    "R8C06_Blocker_" + slot,
                    8600 + slot,
                    data);
                blocker.SetRequiredRuntimeSlot(slot);
                fixtureWorld.Register(blocker);
                Require(blocker.Runtime.SlotIndex == slot,
                    $"Exhaustion fixture could not claim slot {slot}.");
            }

            int gateSeed = FindSeedForFirstRemainder(200, 0);
            fixtureWorld.Rng.Seed(gateSeed);
            int before = fixtureWorld.ObjectCount;
            fixtureWorld.RandomWeaponDropTickAll(1);
            Require(
                fixtureWorld.ObjectCount == before &&
                fixtureWorld.Rng.CallCount == 1,
                "Natural random exhaustion must stop after one gate RNG call.");
            ulong naturalCalls = fixtureWorld.Rng.CallCount;

            fixtureWorld.Rng.Seed(0x11223344u);
            fixtureWorld.RunLateStateSpecialPreCollisionForSelfCheck(spawner);
            Require(
                fixtureWorld.ObjectCount == before &&
                fixtureWorld.Rng.CallCount == 0,
                "state9996 exhaustion must stop before RNG and child creation.");

            return new ExhaustionEvidence
            {
                occupiedDynamicSlots =
                    SimulationWorld.AuthorityRuntimeSlotCapacity -
                    DynamicSlotStart,
                naturalRngCalls = naturalCalls,
                lateRngCalls = fixtureWorld.Rng.CallCount,
                spawned = fixtureWorld.ObjectCount - before,
                passed = true,
            };
        }

        private static ChildExpectation NextChildExpectation(
            DeterministicRng rng,
            int index,
            LF2Entity spawner)
        {
            var item = new ChildExpectation
            {
                x = spawner.Runtime.XInt + rng.NextInt(0, 7) - 3,
                y = spawner.Runtime.YInt + rng.NextInt(0, 7) - 9,
                z = spawner.Runtime.ZInt + 1,
                vy = -(rng.NextInt(0, 15) / 2) - 5.0,
            };
            if (index == 1 || index == 3)
                item.vz = -3.0 - rng.NextInt(0, 2);
            else if (index == 4)
                item.vz = 1.0;
            else
                item.vz = rng.NextInt(0, 2) + 3.0;

            if (index >= 4)
                item.vx = rng.NextInt(0, 7) - 3.0;
            else if (index >= 2)
                item.vx = rng.NextInt(0, 3) + 10.0;
            else
                item.vx = -10.0 - rng.NextInt(0, 3);
            item.frame = rng.NextInt(0, 4);
            item.facing = rng.NextInt(0, 2);
            return item;
        }

        private static int CountCurrentRandomWeaponCandidates(
            SimulationWorld activeWorld)
        {
            int count = 0;
            for (int slot = 0;
                 slot < activeWorld.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                LF2Entity entity = activeWorld.FindEntityByRuntimeSlotForQuery(slot);
                if (entity?.CountsAsRandomWeaponDropCandidate() == true)
                    count++;
            }
            return count;
        }

        private static int FindFirstFreeSlot(SimulationWorld activeWorld)
        {
            for (int slot = DynamicSlotStart;
                 slot < activeWorld.RuntimeSlotCapacityForDiagnostics;
                 slot++)
            {
                if (activeWorld.FindEntityByRuntimeSlotForQuery(slot) == null)
                    return slot;
            }
            return -1;
        }

        private static int FindSeedForFirstRemainder(
            int modulus,
            int expectedRemainder)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var rng = new DeterministicRng(seed);
                if (rng.NextInt(0, modulus) == expectedRemainder)
                    return seed;
            }
            throw new InvalidOperationException("No deterministic gate seed found.");
        }

        private static LF2CharacterData BuildData(
            string name,
            LF2ObjectType type,
            int state,
            int weaponHp)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)type,
                weapon_hp = weaponHp,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state),
                },
            };
        }

        private static LF2CharacterData BuildWeaponData(string name, int oid)
        {
            var frames = new List<LF2FrameData>(4);
            for (int frame = 0; frame < 4; frame++)
                frames.Add(Frame(frame, LF2States.WeaponInSky));
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)LF2ObjectType.LightWeapon,
                weapon_hp = 700 + oid,
                frames = frames,
            };
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = frameId,
                pic = frameId,
                centerx = 39,
                centery = 79,
            };
        }

        private static ChildEvidence Child(LF2Entity child)
        {
            uint generation = 0;
            if (child?.Runtime != null &&
                world.TryGetCurrentRuntimeHandleForDiagnostics(
                    child.Runtime.SlotIndex,
                    child,
                    out RuntimeEntityHandle handle))
            {
                generation = handle.Generation;
            }
            return new ChildEvidence
            {
                slot = child.Runtime.SlotIndex,
                oid = child.ObjectId,
                generation = generation,
                x = child.Runtime.XInt,
                y = child.Runtime.YInt,
                z = child.Runtime.ZInt,
                vx = child.Runtime.Vx,
                vy = child.Runtime.Vy,
                vz = child.Runtime.Vz,
                frame = child.Frame.N,
                facing = child.Runtime.IsFacingLeft ? 1 : 0,
                spawner = child.SpawnerEntityIndex,
            };
        }

        private static bool Nearly(double left, double right)
        {
            return Math.Abs(left - right) <= 0.0001;
        }

        private static T RegisterOwned<T>(T entity)
            where T : LF2Entity
        {
            world.Register(entity);
            Require(entity.Runtime?.SlotIndex >= 0,
                (entity?.Name ?? "entity") + " has no runtime slot.");
            OwnedFixtures.Add(entity);
            return entity;
        }

        private static void CaptureBaseline()
        {
            baselineCaptured = true;
            result.startTick = driver.CurrentTickIndex;
            result.baselineObjectCount = world.ObjectCount;
            result.baselineClaimedSlots =
                world.ClaimedRuntimeSlotCountForDiagnostics;
            result.baselineObjectPoolActive =
                objectPool.ActiveObjectCountForAcceptance;
            result.baselineLogicPoolActive = LF2ReferencePool.Instance.ActiveCount;
            baselineRngState = world.Rng.State;
            baselineRngCalls = world.Rng.CallCount;
            BaselineSounds.Clear();
            BaselineSounds.AddRange(world.PendingSounds);
        }

        private static void CleanupOwnedEntities()
        {
            if (world == null)
                return;
            for (int index = OwnedSpawnedEntities.Count - 1; index >= 0; index--)
            {
                LF2Entity entity = OwnedSpawnedEntities[index];
                try
                {
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        entity.FreeEntityLikeExe();
                }
                catch (Exception exception)
                {
                    AppendCleanupError(entity?.Name ?? "spawned", exception);
                }
            }
            OwnedSpawnedEntities.Clear();

            for (int index = OwnedFixtures.Count - 1; index >= 0; index--)
            {
                LF2Entity entity = OwnedFixtures[index];
                try
                {
                    if (entity?.Match == world && entity.Runtime?.SlotIndex >= 0)
                        world.Unregister(entity);
                }
                catch (Exception exception)
                {
                    AppendCleanupError(entity?.Name ?? "fixture", exception);
                }
            }
            OwnedFixtures.Clear();
            try
            {
                world.FlushPendingDestroyForDiagnostics();
            }
            catch (Exception exception)
            {
                AppendCleanupError("flush", exception);
            }
            world.PendingSounds.Clear();
            world.PendingSounds.AddRange(BaselineSounds);
            world.Rng.RestoreState(baselineRngState, baselineRngCalls);
        }

        private static void FinishSuccess()
        {
            result.status = "PASS";
            result.message =
                "Natural random, live state9996, synthetic full chain, and " +
                "exhaustion matrices passed.";
            result.endTick = driver.CurrentTickIndex;
            CleanupOwnedEntities();
            CaptureFinalState();
            Require(result.cleanupCompleted,
                "Probe cleanup did not restore live baseline: " +
                result.cleanupErrors);
            WriteResult(result);
            Debug.Log(
                $"[BattleRandomWeaponLateEffectPlayModeProbe] PASS: " +
                $"randomOid={result.matrix.natural.selectedOid}, " +
                $"lateChildren={result.matrix.liveLate.childCount}.");
            StopObservation();
        }

        private static void Fail(string message)
        {
            result ??= new ProbeResult();
            result.status = "FAIL";
            result.message = message;
            result.endTick = driver?.CurrentTickIndex ?? -1;
            CleanupOwnedEntities();
            CaptureFinalState();
            WriteResult(result);
            Debug.LogError(
                "[BattleRandomWeaponLateEffectPlayModeProbe] FAIL: " + message);
            StopObservation();
        }

        private static void CaptureFinalState()
        {
            if (result == null)
                return;
            result.finalObjectCount = world?.ObjectCount ?? -1;
            result.finalClaimedSlots =
                world?.ClaimedRuntimeSlotCountForDiagnostics ?? -1;
            result.finalObjectPoolActive =
                objectPool?.ActiveObjectCountForAcceptance ?? -1;
            result.finalLogicPoolActive =
                LF2ReferencePool.Instance?.ActiveCount ?? -1;
            result.rngRestored = world?.Rng != null &&
                world.Rng.State == baselineRngState &&
                world.Rng.CallCount == baselineRngCalls;
            result.pendingSoundsRestored =
                PendingSoundsEqual(world?.PendingSounds, BaselineSounds);
            result.cleanupCompleted =
                baselineCaptured &&
                string.IsNullOrEmpty(result.cleanupErrors) &&
                result.finalObjectCount == result.baselineObjectCount &&
                result.finalClaimedSlots == result.baselineClaimedSlots &&
                result.finalObjectPoolActive == result.baselineObjectPoolActive &&
                result.finalLogicPoolActive == result.baselineLogicPoolActive &&
                result.rngRestored && result.pendingSoundsRestored;
        }

        private static bool PendingSoundsEqual(
            IList<PendingSoundEvent> left,
            IList<PendingSoundEvent> right)
        {
            if (left == null || right == null || left.Count != right.Count)
                return false;
            for (int index = 0; index < left.Count; index++)
            {
                if (left[index].Cue != right[index].Cue ||
                    left[index].WorldX != right[index].WorldX ||
                    left[index].Tick != right[index].Tick)
                {
                    return false;
                }
            }
            return true;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void AppendCleanupError(string label, Exception exception)
        {
            if (result != null)
                result.cleanupErrors += label + ":" + exception.Message + ";";
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeResult
            {
                status = "FAIL",
                message = message,
                startTick = driver?.CurrentTickIndex ?? -1,
                endTick = driver?.CurrentTickIndex ?? -1,
            });
            Debug.LogError(
                "[BattleRandomWeaponLateEffectPlayModeProbe] FAIL: " + message);
        }

        private static void WriteResult(ProbeResult probeResult)
        {
            string path = Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                ResultRelativePath));
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            File.WriteAllText(path, JsonUtility.ToJson(probeResult, true));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (driver != null && EditorApplication.isPlaying)
                driver.SetPaused(previousPaused);
            running = false;
        }

        private static void ResetProbeState()
        {
            driver = null;
            world = null;
            objectPool = null;
            result = null;
            baselineRngState = 0;
            baselineRngCalls = 0;
            previousPaused = false;
            dependenciesResolved = false;
            pauseRequested = false;
            baselineCaptured = false;
            running = false;
            editorUpdates = 0;
            OwnedFixtures.Clear();
            OwnedSpawnedEntities.Clear();
            BaselineSounds.Clear();
        }

        private sealed class ProbeCharacter : LF2Character
        {
            public ProbeCharacter(
                string name,
                int objectId,
                LF2CharacterData data)
            {
                Name = name;
                ObjectId = objectId;
                FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
                ImmediateFrame(0);
                Runtime.SetPosition(0, 0, 0);
                Runtime.SyncIntegerPosition();
                SwitchDir("right");
                Health.HP = 100;
                Health.HPBound = 100;
                Health.HP3 = 100;
                KillCount = -1;
            }

            public void SetPosition(int x, int y, int z)
            {
                Runtime.SetPosition(x, y, z);
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }
        }

        private struct ChildExpectation
        {
            public int x;
            public int y;
            public int z;
            public double vx;
            public double vy;
            public double vz;
            public int frame;
            public int facing;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status = string.Empty;
            public string message = string.Empty;
            public int startTick;
            public int endTick;
            public bool workerWasActive;
            public int baselineObjectCount;
            public int baselineClaimedSlots;
            public int baselineObjectPoolActive;
            public int baselineLogicPoolActive;
            public int finalObjectCount;
            public int finalClaimedSlots;
            public int finalObjectPoolActive;
            public int finalLogicPoolActive;
            public bool rngRestored;
            public bool pendingSoundsRestored;
            public bool cleanupCompleted;
            public string cleanupErrors = string.Empty;
            public MatrixEvidence matrix;
        }

        [Serializable]
        private sealed class MatrixEvidence
        {
            public NaturalRandomEvidence natural;
            public LateChildrenEvidence liveLate;
            public SyntheticChainEvidence synthetic;
            public ExhaustionEvidence exhaustion;
        }

        [Serializable]
        private sealed class NaturalRandomEvidence
        {
            public int weaponCountBefore;
            public int candidateCount;
            public int[] candidates = Array.Empty<int>();
            public int selectedIndex;
            public int selectedOid;
            public int slot;
            public int x;
            public int y;
            public int z;
            public ulong rngCalls;
            public bool passed;
        }

        [Serializable]
        private sealed class LateChildrenEvidence
        {
            public int spawnerSlot;
            public int firstChildSlot;
            public int childCount;
            public ulong rngCalls;
            public ChildEvidence[] children = Array.Empty<ChildEvidence>();
            public bool passed;
        }

        [Serializable]
        private sealed class ChildEvidence
        {
            public int slot;
            public int oid;
            public uint generation;
            public int x;
            public int y;
            public int z;
            public double vx;
            public double vy;
            public double vz;
            public int frame;
            public int facing;
            public int spawner;
        }

        [Serializable]
        private sealed class SyntheticChainEvidence
        {
            public int finalOid;
            public int finalState;
            public int renderPicOffset;
            public int childCount;
            public ulong rngCalls;
            public bool passed;
        }

        [Serializable]
        private sealed class ExhaustionEvidence
        {
            public int occupiedDynamicSlots;
            public ulong naturalRngCalls;
            public ulong lateRngCalls;
            public int spawned;
            public bool passed;
        }
    }
}
#endif
