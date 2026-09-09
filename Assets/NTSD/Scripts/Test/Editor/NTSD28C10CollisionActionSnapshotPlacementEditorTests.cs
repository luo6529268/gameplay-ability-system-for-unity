#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C10CollisionActionSnapshotPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_FreezesActionBeforeSerialRemainderChangesCurrentFrame()
        {
            var world = new SimulationWorld();
            SnapshotObserver probe = CreateProbe(world, 50, 11000);

            new NTSDBattleTickSystem(world).RunReleaseTick(51, buildPresentation: false);

            Assert.That(probe.PostSerialCount, Is.EqualTo(1));
            Assert.That(probe.Frame.N, Is.EqualTo(5));
            Assert.That(probe.Frame.Prev2, Is.EqualTo(0));
            Assert.That(probe.Runtime.PrevFrame2, Is.EqualTo(0));
        }

        [Test]
        public void FullProductionTick_SerialRemainderObservesC11RestPreludeCompleted()
        {
            var world = new SimulationWorld();
            SnapshotObserver probe = CreateProbe(world, 50, 11001);
            probe.AttackExempt = 3;
            probe.ItrRest.Arest = 3;

            new NTSDBattleTickSystem(world).RunReleaseTick(52, buildPresentation: false);

            Assert.That(probe.ObservedAttackExemptInSerial, Is.Zero);
            Assert.That(probe.ObservedArestInSerial, Is.Zero);
            Assert.That(probe.AttackExempt, Is.Zero);
            Assert.That(probe.ItrRest.Arest, Is.Zero);
        }

        [Test]
        public void FullTick_RecordsC10AfterC09BeforeSerial()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(53, buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(9, out BattleTickPhase held), Is.True);
            Assert.That(held, Is.EqualTo(BattleTickPhase.HeldProcess));
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

        private static SnapshotObserver CreateProbe(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var data = new LF2CharacterData
            {
                name = "C10_" + objectId,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData>
                {
                    Frame(0),
                    Frame(5),
                },
            };
            var probe = new SnapshotObserver();
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.Runtime.Frame = 0;
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            return probe;
        }

        private static LF2FrameData Frame(int id)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = 0,
                wait = 100,
                next = id,
                itrs = new List<InteractionArea>(),
            };
        }

        private sealed class SnapshotObserver : LF2OtherObject
        {
            internal int PostSerialCount { get; private set; }
            internal int ObservedAttackExemptInSerial { get; private set; }
            internal int ObservedArestInSerial { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedAttackExemptInSerial = AttackExempt;
                ObservedArestInSerial = ItrRest.Arest;
                Frame.N = 5;
                Frame.D = FrameCache.GetFrameDataById(5);
                Runtime.Frame = 5;
            }
        }
    }
}
#endif
