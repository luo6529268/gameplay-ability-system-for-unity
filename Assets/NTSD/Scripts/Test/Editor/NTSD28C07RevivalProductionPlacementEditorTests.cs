#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C07RevivalProductionPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_RunsRevivalOnceBeforeSerialRemainder()
        {
            var world = new SimulationWorld();
            LF2Character host = CreateHost(world, 50, 10700);
            SerialVisibilityProbe spawned = null;
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((activeWorld, parent) =>
            {
                spawnCount++;
                spawned = CreateSpawned(activeWorld, 51, 998);
                return spawned;
            });

            new NTSDBattleTickSystem(world).RunReleaseTick(21, buildPresentation: false);

            Assert.That(spawnCount, Is.EqualTo(1));
            Assert.That(spawned, Is.Not.Null);
            Assert.That(spawned.PostNativePhysicsSerialCount, Is.EqualTo(1));
            Assert.That(spawned.ObservedNativePhysicsCompleted, Is.False,
                "C07 newborn must not retroactively run the completed C06 transaction.");
        }

        [Test]
        public void DirectDeathCleanupEntry_RemainsCompatible()
        {
            var world = new SimulationWorld();
            LF2Character host = CreateHost(world, 50, 10701);
            int spawnCount = 0;
            world.SetRespawnEffectSpawnOverrideForSelfCheck((_, __) =>
            {
                spawnCount++;
                return host;
            });

            world.PostFrameAdvanceDeathCleanupAll(22);

            Assert.That(spawnCount, Is.EqualTo(1));
            Assert.That(host.Frame.N, Is.EqualTo(219));
            Assert.That(host.Health.HP, Is.EqualTo(80));
        }

        [Test]
        public void FullTick_RecordsRevivalAfterNestedPhysicsBeforeSerialRemainder()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(23, buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(6, out BattleTickPhase physics), Is.True);
            Assert.That(physics, Is.EqualTo(BattleTickPhase.NestedPhysics));
            Assert.That(diagnostics.TryGetLastPhaseAt(7, out BattleTickPhase revival), Is.True);
            Assert.That(revival, Is.EqualTo(BattleTickPhase.Revival));
            Assert.That(diagnostics.TryGetLastPhaseAt(8, out BattleTickPhase firstClamp), Is.True);
            Assert.That(firstClamp, Is.EqualTo(BattleTickPhase.StageBounds));
            Assert.That(diagnostics.TryGetLastPhaseAt(9, out BattleTickPhase firstHeld), Is.True);
            Assert.That(firstHeld, Is.EqualTo(BattleTickPhase.HeldProcess));
            Assert.That(diagnostics.TryGetLastPhaseAt(10, out BattleTickPhase snapshot), Is.True);
            Assert.That(snapshot, Is.EqualTo(BattleTickPhase.CollisionSnapshot));
            Assert.That(diagnostics.TryGetLastPhaseAt(11, out BattleTickPhase pairVRest), Is.True);
            Assert.That(pairVRest, Is.EqualTo(BattleTickPhase.PairVRest));
            Assert.That(diagnostics.TryGetLastPhaseAt(12, out BattleTickPhase candidate), Is.True);
            Assert.That(candidate, Is.EqualTo(BattleTickPhase.CandidateCollect));
            Assert.That(diagnostics.TryGetLastPhaseAt(13, out BattleTickPhase fusion), Is.True);
            Assert.That(fusion, Is.EqualTo(BattleTickPhase.RuntimeMaintenance));
            Assert.That(diagnostics.TryGetLastPhaseAt(14, out BattleTickPhase weaponCount), Is.True);
            Assert.That(weaponCount, Is.EqualTo(BattleTickPhase.ActiveWeaponCount));
            Assert.That(diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase hit), Is.True);
            Assert.That(hit, Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(diagnostics.TryGetLastPhaseAt(28, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        private static LF2Character CreateHost(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(0, LF2States.Lying),
                Frame(219, 0),
            };
            var data = new LF2CharacterData
            {
                name = "C07Host_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = frames,
            };
            var host = new LF2Character();
            host.ModuleInitialize();
            Bind(host, slot, objectId, data, 0);
            host.Initialize(500, 500);
            host.Health.HP = 0;
            host.Health.HPBound = 10;
            host.Health.HP3 = 10;
            host.Health.PP = 77;
            host.HPOrig = 6;
            host.HP2Orig = 1;
            host.RespawnCount = 80;
            host.HitStun = 2;
            world.Register(host);
            return host;
        }

        private static SerialVisibilityProbe CreateSpawned(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "C07Spawned_" + objectId,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { Frame(0, 0) },
            };
            var spawned = new SerialVisibilityProbe();
            Bind(spawned, slot, objectId, data, 0);
            world.Register(spawned);
            return spawned;
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
                itrs = new List<InteractionArea>(),
            };
        }

        private static void Bind(
            LF2Entity entity,
            int slot,
            int objectId,
            LF2CharacterData data,
            int frameId)
        {
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.N = frameId;
            entity.Frame.PN = frameId;
            entity.Frame.D = entity.FrameCache.GetFrameDataById(frameId);
            entity.SetRequiredRuntimeSlot(slot);
        }

        private sealed class SerialVisibilityProbe : LF2OtherObject
        {
            internal int PostNativePhysicsSerialCount { get; private set; }
            internal bool ObservedNativePhysicsCompleted { get; private set; }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostNativePhysicsSerialCount++;
                ObservedNativePhysicsCompleted = nativePhysicsCompleted;
            }
        }
    }
}
#endif
