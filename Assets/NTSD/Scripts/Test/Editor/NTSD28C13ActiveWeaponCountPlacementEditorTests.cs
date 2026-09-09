#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C13ActiveWeaponCountPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_C13CountsOnlyTypes1246AndSerialObservesSnapshot()
        {
            var world = new SimulationWorld();
            CountObserver observer = Create<CountObserver>(world, 13000, 0, 0);
            Create<PassiveEntity>(world, 13001, 1, 20);
            Create<PassiveEntity>(world, 13002, 2, 21);
            Create<PassiveEntity>(world, 13003, 3, 22);
            Create<PassiveEntity>(world, 13004, 4, 23);
            Create<PassiveEntity>(world, 13005, 5, 24);
            Create<PassiveEntity>(world, 13006, 6, 25);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                81,
                buildPresentation: false);

            Assert.That(observer.PostSerialCount, Is.EqualTo(1));
            Assert.That(observer.ObservedCountInSerial, Is.EqualTo(4));
            Assert.That(
                world.ActiveWeaponObjectCountBeforeHitsForDiagnostics,
                Is.EqualTo(4));
            Assert.That(
                world.ActiveWeaponObjectCountCapturedTickForDiagnostics,
                Is.EqualTo(81));
        }

        [Test]
        public void FullTick_RecordsC13AfterFusionBeforeSerial()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                82,
                buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(13, out BattleTickPhase fusion), Is.True);
            Assert.That(fusion, Is.EqualTo(BattleTickPhase.RuntimeMaintenance));
            Assert.That(diagnostics.TryGetLastPhaseAt(14, out BattleTickPhase count), Is.True);
            Assert.That(count, Is.EqualTo(BattleTickPhase.ActiveWeaponCount));
            Assert.That(diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase hit), Is.True);
            Assert.That(hit, Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(diagnostics.TryGetLastPhaseAt(28, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        [Test]
        public void C13Snapshot_DoesNotReplaceAcceptedRandomDropCandidateGate()
        {
            var world = new SimulationWorld();
            for (int index = 0; index < 4; index++)
                Create<PassiveEntity>(world, 13100 + index, 5, 20 + index);

            world.CaptureActiveWeaponObjectCountBeforeHits(83);
            Assert.That(
                world.ActiveWeaponObjectCountBeforeHitsForDiagnostics,
                Is.Zero);

            ulong callsBefore = world.Rng.CallCount;
            world.RandomWeaponDropTickAll(83);

            Assert.That(world.Rng.CallCount, Is.EqualTo(callsBefore),
                "The user-accepted Unity random-drop path must retain its existing all-non-character gate.");
        }

        [Test]
        public void C13Snapshot_Warmed4096CapturesAllocateZeroBytes()
        {
            var world = new SimulationWorld();
            for (int type = 0; type <= 6; type++)
                Create<PassiveEntity>(world, 13300 + type, type, 20 + type);

            world.CaptureActiveWeaponObjectCountBeforeHits(1);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 2; tick < 4098; tick++)
                world.CaptureActiveWeaponObjectCountBeforeHits(tick);
            long allocated =
                GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(
                world.ActiveWeaponObjectCountBeforeHitsForDiagnostics,
                Is.EqualTo(4));
        }

        private static T Create<T>(
            SimulationWorld world,
            int oid,
            int dataType,
            int slot)
            where T : TypedCharacter, new()
        {
            var entity = new T();
            entity.ModuleInitialize();
            entity.Name = "C13_" + oid;
            entity.ObjectId = oid;
            entity.CurrentDataType = dataType;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                oid,
                new LF2CharacterData
                {
                    name = entity.Name,
                    type_sub = dataType,
                    frames = new List<LF2FrameData> { frame },
                }));
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.D = frame;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private abstract class TypedCharacter : LF2Character
        {
            internal int CurrentDataType { get; set; }

            public override int GetCurrentDataObjectTypeForSimulation()
                => CurrentDataType;
        }

        private sealed class CountObserver : TypedCharacter
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedCountInSerial { get; private set; } = -1;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
                => true;

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedCountInSerial =
                    Match?.ActiveWeaponObjectCountBeforeHitsForDiagnostics ?? -1;
            }
        }

        private sealed class PassiveEntity : TypedCharacter
        {
            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
                => true;
        }
    }
}
#endif
