#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C11CandidateBuildPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_SerialRemainderObservesC11RestAndPairPreludeCompleted()
        {
            var world = new SimulationWorld();
            C11Observer probe = CreateProbe(world, 50, 11100);
            probe.AttackExempt = 3;
            probe.ItrRest.Arest = 3;

            new NTSDBattleTickSystem(world).RunReleaseTick(61, buildPresentation: false);

            Assert.That(probe.PostSerialCount, Is.EqualTo(1));
            Assert.That(probe.ObservedAttackExemptInSerial, Is.Zero);
            Assert.That(probe.ObservedArestInSerial, Is.Zero);
            Assert.That(probe.ObservedPairVisitCountInSerial, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void FullTick_RecordsC11TransactionAfterC10BeforeSerial()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(62, buildPresentation: false);

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

        private static C11Observer CreateProbe(
            SimulationWorld world,
            int slot,
            int objectId)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            var data = new LF2CharacterData
            {
                name = "C11_" + objectId,
                type_sub = (int)LF2ObjectType.Other,
                frames = new List<LF2FrameData> { frame },
            };
            var probe = new C11Observer(world);
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            return probe;
        }

        private sealed class C11Observer : LF2OtherObject
        {
            private readonly SimulationWorld world;

            internal C11Observer(SimulationWorld world)
            {
                this.world = world;
            }

            internal int PostSerialCount { get; private set; }
            internal int ObservedAttackExemptInSerial { get; private set; }
            internal int ObservedArestInSerial { get; private set; }
            internal int ObservedPairVisitCountInSerial { get; private set; }

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
                ObservedPairVisitCountInSerial =
                    world.LastCollisionPairVRestEligibilityVisitCount;
            }
        }
    }
}
#endif
