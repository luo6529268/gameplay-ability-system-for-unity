#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28ResidualSerialTailRehomeEditorTests
    {
        [Test]
        public void FullTick_RecordsC14ImmediatelyBeforeC15AndSerialAfterC20()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                101,
                buildPresentation: false);

            Assert.That(
                diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase typeZeroHit),
                Is.True);
            Assert.That(
                typeZeroHit,
                Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(16, out BattleTickPhase randomDrop),
                Is.True);
            Assert.That(randomDrop, Is.EqualTo(BattleTickPhase.RandomWeaponDrop));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(17, out BattleTickPhase nonTypeZeroHit),
                Is.True);
            Assert.That(nonTypeZeroHit, Is.EqualTo(BattleTickPhase.ObjectHitConsume));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(22, out BattleTickPhase secondHeld),
                Is.True);
            Assert.That(secondHeld, Is.EqualTo(BattleTickPhase.HeldProcess));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(23, out BattleTickPhase bounds),
                Is.True);
            Assert.That(bounds, Is.EqualTo(BattleTickPhase.PreFrameBounds));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(24, out BattleTickPhase impulse),
                Is.True);
            Assert.That(impulse, Is.EqualTo(BattleTickPhase.FramePostProcess));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(25, out BattleTickPhase resourceTick),
                Is.True);
            Assert.That(resourceTick, Is.EqualTo(BattleTickPhase.NativeResourceTick));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(26, out BattleTickPhase frameTick),
                Is.True);
            Assert.That(frameTick, Is.EqualTo(BattleTickPhase.NativeFrameTick));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(27, out BattleTickPhase c25Skeleton),
                Is.True);
            Assert.That(c25Skeleton, Is.EqualTo(BattleTickPhase.LateEntityUpdate));
            Assert.That(
                diagnostics.TryGetLastPhaseAt(28, out BattleTickPhase serialTailProxy),
                Is.True);
            Assert.That(serialTailProxy, Is.EqualTo(BattleTickPhase.FrameAdvance));
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        [Test]
        public void FullProductionTick_SerialTailObservesC14AndC15SideEffects()
        {
            var world = new SimulationWorld();
            world.Rng.Seed(0x5EEDu);
            world.ConfigureBattleHitExecutionPlanForDiagnostics(
                BattleHitExecutionPlanMode.Disabled);
            world.ForceLegacyEmptyCharacterHitConsumeForDiagnostics = true;
            SerialTailObserver observer = CreateObserver(world);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                102,
                buildPresentation: false);

            Assert.That(observer.HitCallerCount, Is.EqualTo(1));
            Assert.That(observer.PostSerialCount, Is.EqualTo(1));
            Assert.That(observer.HitCallerCountSeenInSerial, Is.EqualTo(1));
            Assert.That(observer.RngCallsSeenInSerial, Is.EqualTo(1));
            Assert.That(world.Rng.CallCount, Is.EqualTo(1));
        }

        private static SerialTailObserver CreateObserver(SimulationWorld world)
        {
            var observer = new SerialTailObserver();
            observer.ModuleInitialize();
            observer.Name = "ResidualSerialTailObserver";
            observer.ObjectId = 13410;
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

        private sealed class SerialTailObserver : LF2Character
        {
            internal int HitCallerCount { get; private set; }
            internal int PostSerialCount { get; private set; }
            internal int HitCallerCountSeenInSerial { get; private set; } = -1;
            internal ulong RngCallsSeenInSerial { get; private set; } = ulong.MaxValue;

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
                RngCallsSeenInSerial =
                    RegisteredWorldForSimulation?.Rng.CallCount ?? ulong.MaxValue;
            }
        }
    }
}
#endif
