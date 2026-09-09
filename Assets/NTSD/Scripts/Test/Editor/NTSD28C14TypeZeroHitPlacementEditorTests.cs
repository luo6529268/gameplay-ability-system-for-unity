#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C14TypeZeroHitPlacementEditorTests
    {
        [Test]
        public void FullProductionTick_SerialTailStillObservesCompletedC14Caller()
        {
            var world = new SimulationWorld();
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.Disabled);
            world.ForceLegacyEmptyCharacterHitConsumeForDiagnostics = true;
            TypeZeroObserver observer = CreateObserver(world);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                91,
                buildPresentation: false);

            Assert.That(observer.HitCallerCount, Is.EqualTo(1));
            Assert.That(observer.PostSerialCount, Is.EqualTo(1));
            Assert.That(observer.HitCallerCountSeenInSerial, Is.EqualTo(1));
            Assert.That(
                world.LastCharacterHitConsumeExecutedCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void FullTick_RecordsC14AfterCountImmediatelyBeforeC15()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                92,
                buildPresentation: false);

            Assert.That(diagnostics.TryGetLastPhaseAt(14, out BattleTickPhase count), Is.True);
            Assert.That(count, Is.EqualTo(BattleTickPhase.ActiveWeaponCount));
            Assert.That(diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase typeZeroHit), Is.True);
            Assert.That(typeZeroHit, Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(diagnostics.TryGetLastPhaseAt(16, out BattleTickPhase randomDrop), Is.True);
            Assert.That(randomDrop, Is.EqualTo(BattleTickPhase.RandomWeaponDrop));
            Assert.That(diagnostics.TryGetLastPhaseAt(17, out BattleTickPhase nonTypeZeroHit), Is.True);
            Assert.That(nonTypeZeroHit, Is.EqualTo(BattleTickPhase.ObjectHitConsume));
            Assert.That(diagnostics.TryGetLastPhaseAt(28, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        private static TypeZeroObserver CreateObserver(SimulationWorld world)
        {
            var observer = new TypeZeroObserver();
            observer.ModuleInitialize();
            observer.Name = "C14_TypeZeroObserver";
            observer.ObjectId = 13400;
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

        private sealed class TypeZeroObserver : LF2Character
        {
            internal int HitCallerCount { get; private set; }
            internal int PostSerialCount { get; private set; }
            internal int HitCallerCountSeenInSerial { get; private set; } = -1;

            public override int GetCurrentDataObjectTypeForSimulation()
                => (int)LF2ObjectType.Character;

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

            public override void SimPostInteraction(int tickIndex)
            {
                HitCallerCount++;
            }

            internal override void RunPostNativePhysicsSerialForWorldPass(
                int tickIndex,
                bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                HitCallerCountSeenInSerial = HitCallerCount;
            }
        }
    }
}
#endif
