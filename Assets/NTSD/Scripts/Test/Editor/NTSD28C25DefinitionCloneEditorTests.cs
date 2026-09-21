#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25DefinitionCloneEditorTests
    {
        private const uint CloneSeed = 0x13572468u;

        [Test]
        public void C25a_AnyDataType_UsesSourceNextAndResetsNativeFrameFields()
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            LF2CharacterData sourceData = Data(
                "C25a_Source",
                LF2ObjectType.Other,
                Frame(5, 8007, 3));
            LF2CharacterData targetData = Data(
                "C25a_Target",
                LF2ObjectType.SpecialAttack,
                Frame(0, 100, 0),
                Frame(3, 301, 3));
            wrappers.Add(9000, new LF2CharacterDataWrapper(9000, sourceData));
            wrappers.Add(7, new LF2CharacterDataWrapper(7, targetData));
            SimulationWorld world = CreateWorld(wrappers, 9000, 7);
            LF2Character source = CreateCharacter(world, 9000, sourceData, 60, 5);
            source.AttackingCounter = 9;
            source.Runtime.WeaponFlightCounter = 321;
            source.Runtime.RenderPicOffset = 140;
            source.Frame.Prev2 = 88;
            source.Runtime.PrevFrame2 = 88;

            world.LateEntityUpdateAll(1);

            Assert.That(source.ObjectId, Is.EqualTo(7));
            Assert.That(
                source.GetCurrentDataObjectTypeForSimulation(),
                Is.EqualTo((int)LF2ObjectType.SpecialAttack));
            Assert.That(source.Frame.N, Is.EqualTo(3));
            Assert.That(source.Frame.D, Is.SameAs(targetData.frames[1]));
            Assert.That(source.AttackingCounter, Is.Zero);
            Assert.That(source.Trans.WaitCounter, Is.EqualTo(3));
            Assert.That(source.Frame.Prev2, Is.EqualTo(3));
            Assert.That(source.Runtime.PrevFrame2, Is.EqualTo(3));
            Assert.That(source.Runtime.RenderPicOffset, Is.Zero);
            Assert.That(source.Runtime.WeaponFlightCounter, Is.EqualTo(321));
        }

        [TestCase(9995, 50)]
        [TestCase(4007, 7)]
        public void C25a_LegacySpecialStates_AreProductionNoOps(
            int state,
            int legacyTargetOid)
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            LF2CharacterData sourceData = Data(
                "C25a_Legacy_Source",
                LF2ObjectType.Character,
                Frame(5, state, 0));
            LF2CharacterData targetData = Data(
                "C25a_Legacy_Target",
                LF2ObjectType.Character,
                Frame(0, 100, 0));
            wrappers.Add(9000, new LF2CharacterDataWrapper(9000, sourceData));
            wrappers.Add(
                legacyTargetOid,
                new LF2CharacterDataWrapper(legacyTargetOid, targetData));
            SimulationWorld world = CreateWorld(
                wrappers,
                9000,
                legacyTargetOid);
            LF2Character source = CreateCharacter(world, 9000, sourceData, 60, 5);

            world.LateEntityUpdateAll(1);

            Assert.That(source.ObjectId, Is.EqualTo(9000));
            Assert.That(source.Frame.N, Is.EqualTo(5));
            Assert.That(source.Frame.D, Is.SameAs(sourceData.frames[0]));
        }

        [Test]
        public void C25a_MissingTargetAction_IsAtomicNoOp()
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            LF2CharacterData sourceData = Data(
                "C25a_Atomic_Source",
                LF2ObjectType.Character,
                Frame(5, 8007, 3));
            LF2CharacterData targetData = Data(
                "C25a_Atomic_Target",
                LF2ObjectType.SpecialAttack,
                Frame(0, 100, 0));
            wrappers.Add(9000, new LF2CharacterDataWrapper(9000, sourceData));
            wrappers.Add(7, new LF2CharacterDataWrapper(7, targetData));
            SimulationWorld world = CreateWorld(wrappers, 9000, 7);
            LF2Character source = CreateCharacter(world, 9000, sourceData, 60, 5);
            source.AttackingCounter = 9;
            source.Runtime.RenderPicOffset = 77;
            source.Runtime.WeaponFlightCounter = 321;

            world.LateEntityUpdateAll(1);

            Assert.That(source.ObjectId, Is.EqualTo(9000));
            Assert.That(source.Frame.N, Is.EqualTo(5));
            Assert.That(source.Frame.D, Is.SameAs(sourceData.frames[0]));
            Assert.That(source.AttackingCounter, Is.EqualTo(9));
            Assert.That(source.Runtime.RenderPicOffset, Is.EqualTo(77));
            Assert.That(source.Runtime.WeaponFlightCounter, Is.EqualTo(321));
        }

        [Test]
        public void C25a_MissingTargetDefinition_IsAtomicNoOp()
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            LF2CharacterData sourceData = Data(
                "C25a_MissingDefinition_Source",
                LF2ObjectType.Character,
                Frame(5, 8007, 3));
            LF2CharacterData targetData = Data(
                "C25a_MissingDefinition_Target",
                LF2ObjectType.SpecialAttack,
                Frame(3, 301, 3));
            wrappers.Add(9000, new LF2CharacterDataWrapper(9000, sourceData));
            wrappers.Add(7, new LF2CharacterDataWrapper(7, targetData));
            SimulationWorld world = CreateWorld(wrappers, 9000);
            LF2Character source = CreateCharacter(world, 9000, sourceData, 60, 5);
            source.AttackingCounter = 9;
            source.Runtime.RenderPicOffset = 77;

            world.LateEntityUpdateAll(1);

            Assert.That(source.ObjectId, Is.EqualTo(9000));
            Assert.That(source.Frame.N, Is.EqualTo(5));
            Assert.That(source.Frame.D, Is.SameAs(sourceData.frames[0]));
            Assert.That(source.AttackingCounter, Is.EqualTo(9));
            Assert.That(source.Runtime.RenderPicOffset, Is.EqualTo(77));
        }

        [TestCase(999)]
        [TestCase(1000)]
        public void C25a_SourceNextAtOrAbove999_ResetsTargetActionZero(
            int sourceNext)
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            LF2CharacterData sourceData = Data(
                "C25a_ResetAction_Source",
                LF2ObjectType.Character,
                Frame(5, 8007, sourceNext));
            LF2CharacterData targetData = Data(
                "C25a_ResetAction_Target",
                LF2ObjectType.SpecialAttack,
                Frame(0, 301, 0));
            wrappers.Add(9000, new LF2CharacterDataWrapper(9000, sourceData));
            wrappers.Add(7, new LF2CharacterDataWrapper(7, targetData));
            SimulationWorld world = CreateWorld(wrappers, 9000, 7);
            LF2Character source = CreateCharacter(world, 9000, sourceData, 60, 5);

            world.LateEntityUpdateAll(1);

            Assert.That(source.ObjectId, Is.EqualTo(7));
            Assert.That(source.Frame.N, Is.Zero);
            Assert.That(source.Frame.D, Is.SameAs(targetData.frames[0]));
            Assert.That(source.Trans.WaitCounter, Is.Zero);
            Assert.That(source.Runtime.PrevFrame2, Is.Zero);
        }

        [Test]
        public void C25b_State9996_UsesExactSynchronizedCallsAndNativeBirthDefaults()
        {
            SimulationWorld world = CreateCloneWorld(
                include217: true,
                include218: true,
                runtimeSlotCapacity: SimulationWorld.AuthorityRuntimeSlotCapacity,
                spawnerSlot: 60,
                out LF2Character spawner);
            var observer = new SynchronizedRecorder();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            uint legacyState = world.Rng.State;
            ulong legacyCalls = world.Rng.CallCount;
            var expectedRandom = new NTSD28NativeRandom(CloneSeed);

            world.LateEntityUpdateAll(1);

            Assert.That(observer.Calls, Has.Count.EqualTo(34));
            Assert.That(world.Rng.State, Is.EqualTo(legacyState));
            Assert.That(world.Rng.CallCount, Is.EqualTo(legacyCalls));
            int callIndex = 0;
            for (int spawnIndex = 0; spawnIndex < 5; spawnIndex++)
            {
                int expectedX = spawner.Runtime.XInt +
                    Next(expectedRandom, observer, ref callIndex, 0x0041F792u, 7) - 3;
                int expectedY = spawner.Runtime.YInt +
                    Next(expectedRandom, observer, ref callIndex, 0x0041F7B6u, 7) - 9;
                int expectedZ = spawner.Runtime.ZInt + 1;
                double expectedVy = -(
                    Next(expectedRandom, observer, ref callIndex, 0x0041F818u, 15) / 2) - 5.0;
                double expectedVz;
                if (spawnIndex == 1 || spawnIndex == 3)
                {
                    expectedVz = -3.0 - Next(
                        expectedRandom,
                        observer,
                        ref callIndex,
                        0x0041F87Fu,
                        2);
                }
                else if (spawnIndex == 4)
                {
                    expectedVz = 1.0;
                }
                else
                {
                    expectedVz = Next(
                        expectedRandom,
                        observer,
                        ref callIndex,
                        0x0041F8A9u,
                        2) + 3.0;
                }

                double expectedVx;
                if (spawnIndex >= 4)
                {
                    expectedVx = Next(
                        expectedRandom,
                        observer,
                        ref callIndex,
                        0x0041F92Eu,
                        7) - 3.0;
                }
                else if (spawnIndex >= 2)
                {
                    expectedVx = Next(
                        expectedRandom,
                        observer,
                        ref callIndex,
                        0x0041F908u,
                        3) + 10.0;
                }
                else
                {
                    expectedVx = -10.0 - Next(
                        expectedRandom,
                        observer,
                        ref callIndex,
                        0x0041F8DDu,
                        3);
                }

                int expectedFrame = Next(
                    expectedRandom,
                    observer,
                    ref callIndex,
                    0x0041F955u,
                    4);
                int expectedFacing = Next(
                    expectedRandom,
                    observer,
                    ref callIndex,
                    0x0041F96Bu,
                    2);
                LF2Entity child = world.FindEntityByRuntimeSlotIncludingPending(
                    50 + spawnIndex);
                int expectedOid = spawnIndex == 4 ? 218 : 217;
                Assert.That(child, Is.Not.Null);
                Assert.That(child.ObjectId, Is.EqualTo(expectedOid));
                Assert.That(child.Runtime.XInt, Is.EqualTo(expectedX));
                Assert.That(child.Runtime.YInt, Is.EqualTo(expectedY));
                Assert.That(child.Runtime.ZInt, Is.EqualTo(expectedZ));
                Assert.That(child.Runtime.Vx, Is.EqualTo(expectedVx));
                Assert.That(child.Runtime.Vy, Is.EqualTo(expectedVy));
                Assert.That(child.Runtime.Vz, Is.EqualTo(expectedVz));
                Assert.That(child.Frame.N, Is.EqualTo(expectedFrame));
                Assert.That(
                    child.Runtime.Dir,
                    Is.EqualTo(expectedFacing == 0 ? "right" : "left"));
                Assert.That(child.Health.HP, Is.EqualTo(500));
                Assert.That(child.Health.HPBound, Is.EqualTo(500));
                Assert.That(child.Health.HP3, Is.EqualTo(500));
                Assert.That(child.Health.PP, Is.EqualTo(500));
                Assert.That(child.SpawnerEntityIndex, Is.EqualTo(-1));
                Assert.That(child.OwnerId, Is.EqualTo(-1));
                Assert.That(child.OwnerEntityIndex, Is.EqualTo(-1));
                Assert.That(child.RelationOwnerSlot, Is.EqualTo(-1));
                Assert.That(child.Team, Is.Zero);
                Assert.That(child.RelationTeam, Is.Zero);
                Assert.That(child.AttackExempt, Is.EqualTo(6));
                Assert.That(
                    child.Runtime.WeaponFlightCounter,
                    Is.EqualTo(700 + expectedOid));
            }

            Assert.That(callIndex, Is.EqualTo(34));
        }

        [Test]
        public void C25b_MissingDefinitionsSkipRandom_FullCapacityConsumesAllTuples()
        {
            SimulationWorld missingWorld = CreateCloneWorld(
                include217: false,
                include218: false,
                runtimeSlotCapacity: SimulationWorld.AuthorityRuntimeSlotCapacity,
                spawnerSlot: 60,
                out _);
            NTSD28NativeRandomScalarState missingBefore =
                missingWorld.NativeRandom.CaptureScalarState();

            missingWorld.LateEntityUpdateAll(1);

            NTSD28NativeRandomScalarState missingAfter =
                missingWorld.NativeRandom.CaptureScalarState();
            Assert.That(
                missingAfter.SynchronizedCalls,
                Is.EqualTo(missingBefore.SynchronizedCalls));
            Assert.That(missingWorld.ObjectCount, Is.EqualTo(1));

            SimulationWorld fullWorld = CreateCloneWorld(
                include217: true,
                include218: true,
                runtimeSlotCapacity: 50,
                spawnerSlot: 0,
                out _);
            NTSD28NativeRandomScalarState fullBefore =
                fullWorld.NativeRandom.CaptureScalarState();

            fullWorld.LateEntityUpdateAll(1);

            NTSD28NativeRandomScalarState fullAfter =
                fullWorld.NativeRandom.CaptureScalarState();
            Assert.That(
                fullAfter.SynchronizedCalls,
                Is.EqualTo(fullBefore.SynchronizedCalls + 34ul));
            Assert.That(fullWorld.ObjectCount, Is.EqualTo(1));
        }

        [TestCase(49, 5)]
        [TestCase(60, 6)]
        public void C25b_NewbornSlotVisibility_FollowsDynamicAscendingCursor(
            int spawnerSlot,
            int expectedAttackerRest)
        {
            SimulationWorld world = CreateCloneWorld(
                include217: true,
                include218: true,
                runtimeSlotCapacity: SimulationWorld.AuthorityRuntimeSlotCapacity,
                spawnerSlot: spawnerSlot,
                out _);

            world.LateEntityUpdateAll(1);

            for (int spawnIndex = 0; spawnIndex < 5; spawnIndex++)
            {
                LF2Entity child = world.FindEntityByRuntimeSlotIncludingPending(
                    50 + spawnIndex);
                Assert.That(child, Is.Not.Null);
                Assert.That(child.AttackExempt, Is.EqualTo(expectedAttackerRest));
            }
        }

        private static SimulationWorld CreateCloneWorld(
            bool include217,
            bool include218,
            int runtimeSlotCapacity,
            int spawnerSlot,
            out LF2Character spawner)
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            var definitions = new List<ObjectDefinition>();
            LF2CharacterData sourceData = Data(
                "C25b_Source",
                LF2ObjectType.Character,
                Frame(0, 9996, 0));
            wrappers.Add(9000, new LF2CharacterDataWrapper(9000, sourceData));
            definitions.Add(new ObjectDefinition(
                9000,
                (int)LF2ObjectType.Character,
                "c25b-source.dat"));
            if (include217)
                AddWeapon(217);
            if (include218)
                AddWeapon(218);

            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            BattleRuntimeProfile profile =
                runtimeSlotCapacity == SimulationWorld.AuthorityRuntimeSlotCapacity
                    ? BattleRuntimeProfile.Authority400
                    : BattleRuntimeProfile.MobileExtended;
            var world = new SimulationWorld(
                profile,
                runtimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                resolver);
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            world.LogicReferencePool.Prewarm(LF2ObjectType.LightWeapon, 8);
            world.LogicReferencePool.PrewarmTasks<OPointCreateTask>(2);
            world.SetLogicOnlyEntityMaterialization(true);
            world.NativeRandom.ResetFromSeed(CloneSeed);
            spawner = CreateCharacter(world, 9000, sourceData, spawnerSlot, 0);
            spawner.Runtime.SetPosition(100.75, -20.25, 200.5);
            spawner.Runtime.SyncIntegerPosition();
            spawner.AttackingCounter = 1;
            return world;

            void AddWeapon(int oid)
            {
                LF2CharacterData data = Data(
                    $"C25b_Weapon_{oid}",
                    LF2ObjectType.LightWeapon,
                    Frame(0, LF2States.WeaponInSky, 0),
                    Frame(1, LF2States.WeaponInSky, 1),
                    Frame(2, LF2States.WeaponInSky, 2),
                    Frame(3, LF2States.WeaponInSky, 3));
                data.weapon_hp = 700 + oid;
                wrappers.Add(oid, new LF2CharacterDataWrapper(oid, data));
                definitions.Add(new ObjectDefinition(
                    oid,
                    (int)LF2ObjectType.LightWeapon,
                    $"c25b-{oid}.dat"));
            }
        }

        private static SimulationWorld CreateWorld(
            Dictionary<int, LF2CharacterDataWrapper> wrappers,
            params int[] definitionOids)
        {
            var definitions = new List<ObjectDefinition>();
            for (int index = 0; index < definitionOids.Length; index++)
            {
                int oid = definitionOids[index];
                LF2CharacterDataWrapper wrapper = wrappers[oid];
                definitions.Add(new ObjectDefinition(
                    oid,
                    wrapper.characterData.type_sub,
                    $"c25a-{oid}.dat"));
            }

            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            var world = new SimulationWorld(
                BattleRuntimeProfile.Authority400,
                SimulationWorld.AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                resolver);
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            return world;
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int oid,
            LF2CharacterData data,
            int runtimeSlot,
            int frameId)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = data.name;
            character.ObjectId = oid;
            character.FrameCache.Load(new LF2CharacterDataWrapper(oid, data));
            character.Initialize(500, 500);
            character.Frame.N = frameId;
            character.Frame.PN = frameId;
            character.Frame.D = character.FrameCache.GetFrameDataById(frameId);
            character.Frame.Prev = frameId;
            character.Frame.Prev2 = frameId;
            character.Frame.Prev2D = character.Frame.D;
            character.Trans.SyncDirectFrameData(
                character.Frame.D.wait,
                character.Frame.D.next,
                27);
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Runtime.SuppressLateFrameTickUntilTick = 100;
            character.RefreshRuntimeSnapshot();
            world.Register(character);
            return character;
        }

        private static LF2CharacterData Data(
            string name,
            LF2ObjectType type,
            params LF2FrameData[] frames)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)type,
                frames = new List<LF2FrameData>(frames),
            };
        }

        private static LF2FrameData Frame(int frameId, int state, int next)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = next,
                centerx = 39,
                centery = 79,
            };
        }

        private static int Next(
            NTSD28NativeRandom expected,
            SynchronizedRecorder actual,
            ref int index,
            uint callSite,
            int upperBound)
        {
            Assert.That(actual.Calls[index].CallSite, Is.EqualTo(callSite));
            Assert.That(actual.Calls[index].UpperBound, Is.EqualTo(upperBound));
            int result = expected.SynchronizedNext(callSite, upperBound);
            Assert.That(actual.Calls[index].Result, Is.EqualTo(result));
            index++;
            return result;
        }

        private sealed class SynchronizedRecorder : INTSD28NativeRandomCallObserver
        {
            internal readonly List<NTSD28NativeSynchronizedCall> Calls = new();

            public void OnCrtNext(NTSD28NativeCrtCall call)
            {
            }

            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                Calls.Add(call);
            }
        }
    }
}
#endif
