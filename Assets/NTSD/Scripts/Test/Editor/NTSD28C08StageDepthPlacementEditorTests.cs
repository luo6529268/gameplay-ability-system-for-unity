#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C08StageDepthPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_SerialRemainderObservesFirstDepthClamp()
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 180, 350, 0, 0);
            StageObserverCharacter probe = CreateProbe(world, 0, 10800, 500.0);

            new NTSDBattleTickSystem(world).RunReleaseTick(31, buildPresentation: false);

            Assert.That(probe.PostSerialCount, Is.EqualTo(1));
            Assert.That(probe.ObservedZInSerial, Is.EqualTo(350.0));
            Assert.That(probe.Runtime.Z, Is.EqualTo(350.0));
            Assert.That(probe.Runtime.ZInt, Is.EqualTo(350));
        }

        [Test]
        public void FullTick_RecordsStageBoundsBeforeFirstHeldAndSerialAndKeepsSecondOccurrence()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(32, buildPresentation: false);

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

            int stageBoundsOccurrences = 0;
            for (int index = 0; index < diagnostics.LastPhaseSequenceCount; index++)
            {
                if (diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase phase) &&
                    phase == BattleTickPhase.StageBounds)
                {
                    stageBoundsOccurrences++;
                }
            }

            Assert.That(stageBoundsOccurrences, Is.EqualTo(2));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        private static StageObserverCharacter CreateProbe(
            SimulationWorld world,
            int slot,
            int objectId,
            double z)
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
                name = "C08_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData> { frame },
            };
            var probe = new StageObserverCharacter();
            probe.ModuleInitialize();
            probe.Name = data.name;
            probe.ObjectId = objectId;
            probe.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.Initialize(500, 500);
            probe.PS.z = z;
            probe.Runtime.SetPosition(0.0, 0.0, z);
            probe.Runtime.SyncIntegerPosition();
            probe.SetRequiredRuntimeSlot(slot);
            world.Register(probe);
            probe.PS.z = z;
            probe.Runtime.SetPosition(0.0, 0.0, z);
            probe.Runtime.SyncIntegerPosition();
            return probe;
        }

        private sealed class StageObserverCharacter : LF2Character
        {
            internal int PostSerialCount { get; private set; }
            internal double ObservedZInSerial { get; private set; }

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                return true;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedZInSerial = Runtime.Z;
            }
        }
    }
}
#endif
