#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C21C22PlacementEditorTests
    {
        [Test]
        public void FullTick_RecordsC21AndC22BeforeClocksAndC25Skeleton()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                111,
                buildPresentation: false);

            Assert.That(PhaseAt(diagnostics, 22), Is.EqualTo(BattleTickPhase.HeldProcess));
            Assert.That(PhaseAt(diagnostics, 23), Is.EqualTo(BattleTickPhase.PreFrameBounds));
            Assert.That(PhaseAt(diagnostics, 24), Is.EqualTo(BattleTickPhase.FramePostProcess));
            Assert.That(PhaseAt(diagnostics, 25), Is.EqualTo(BattleTickPhase.NativeResourceTick));
            Assert.That(PhaseAt(diagnostics, 26), Is.EqualTo(BattleTickPhase.NativeFrameTick));
            Assert.That(PhaseAt(diagnostics, 27), Is.EqualTo(BattleTickPhase.LateEntityUpdate));
            Assert.That(PhaseAt(diagnostics, 28), Is.EqualTo(BattleTickPhase.FrameAdvance));
            Assert.That(PhaseAt(diagnostics, 29), Is.EqualTo(BattleTickPhase.Stage));
            Assert.That(PhaseAt(diagnostics, 33), Is.EqualTo(BattleTickPhase.RenderDispatch));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        [Test]
        public void FullProductionTick_SerialObservesBoundaryAndImpulseFinalizers()
        {
            var world = new SimulationWorld();
            C21C22Observer observer = CreateObserver(world);
            observer.Runtime.X = -50.0;
            observer.Runtime.Vx = 0.0;
            observer.HitCount = 1;
            observer.KnockbackVx = 10.0;

            new NTSDBattleTickSystem(world).RunReleaseTick(
                112,
                buildPresentation: false);

            Assert.That(observer.PostSerialCount, Is.EqualTo(1));
            Assert.That(observer.XSeenInSerial, Is.EqualTo(0.0));
            Assert.That(observer.VxSeenInSerial, Is.EqualTo(10.0));
            Assert.That(observer.HitCountSeenInSerial, Is.Zero);
            Assert.That(observer.KnockbackVxSeenInSerial, Is.Zero);
        }

        private static BattleTickPhase PhaseAt(
            BattleTickPhaseDiagnostics diagnostics,
            int index)
        {
            Assert.That(
                diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase phase),
                Is.True);
            return phase;
        }

        private static C21C22Observer CreateObserver(SimulationWorld world)
        {
            var observer = new C21C22Observer();
            observer.ModuleInitialize();
            observer.Name = "C21C22Observer";
            observer.ObjectId = 13420;
            observer.RelationTeam = 0;
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 100,
                next = 0,
                itrs = new List<InteractionArea>(),
            };
            observer.FrameCache.Load(new LF2CharacterDataWrapper(
                observer.ObjectId,
                new LF2CharacterData
                {
                    name = observer.Name,
                    frames = new List<LF2FrameData> { frame },
                }));
            observer.Frame.N = 0;
            observer.Frame.PN = 0;
            observer.Frame.D = frame;
            observer.Initialize(500, 500);
            observer.SetRequiredRuntimeSlot(0);
            world.Register(observer);
            return observer;
        }

        private sealed class C21C22Observer : LF2Character
        {
            internal int PostSerialCount { get; private set; }
            internal double XSeenInSerial { get; private set; } = double.NaN;
            internal double VxSeenInSerial { get; private set; } = double.NaN;
            internal int HitCountSeenInSerial { get; private set; } = -1;
            internal double KnockbackVxSeenInSerial { get; private set; } = double.NaN;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
                => true;

            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(
                int tickIndex)
            {
            }

            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(
                int tickIndex,
                bool applyFrameMotionTail = true)
            {
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                XSeenInSerial = Runtime.X;
                VxSeenInSerial = Runtime.Vx;
                HitCountSeenInSerial = HitCount;
                KnockbackVxSeenInSerial = KnockbackVx;
            }
        }
    }
}
#endif
