#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NUnit.Framework;

using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28BattleActualPhaseSequenceEditorTests
    {
        private static readonly BattleTickPhase[] CurrentFullTickSequence =
        {
            BattleTickPhase.BattleFlow,
            BattleTickPhase.NativeSparkAdvance,
            BattleTickPhase.HumanInput,
            BattleTickPhase.CharacterInput,
            BattleTickPhase.FrameMotion,
            BattleTickPhase.NativeTeleport,
            BattleTickPhase.NestedPhysics,
            BattleTickPhase.Revival,
            BattleTickPhase.StageBounds,
            BattleTickPhase.HeldProcess,
            BattleTickPhase.CollisionSnapshot,
            BattleTickPhase.PairVRest,
            BattleTickPhase.CandidateCollect,
            BattleTickPhase.RuntimeMaintenance,
            BattleTickPhase.ActiveWeaponCount,
            BattleTickPhase.CharacterHitConsumePostInteraction,
            BattleTickPhase.RandomWeaponDrop,
            BattleTickPhase.ObjectHitConsume,
            BattleTickPhase.CandidateConsumptionEnd,
            BattleTickPhase.PreInteraction,
            BattleTickPhase.StageBounds,
            BattleTickPhase.HeldProcess,
            BattleTickPhase.PreFrameBounds,
            BattleTickPhase.FramePostProcess,
            BattleTickPhase.NativeResourceTick,
            BattleTickPhase.NativeFrameTick,
            BattleTickPhase.LateEntityUpdate,
            BattleTickPhase.FrameAdvance,
            BattleTickPhase.Stage,
            BattleTickPhase.RandomWeaponDropTail,
            BattleTickPhase.EntityPostFrameTail,
            BattleTickPhase.BattleResults,
            BattleTickPhase.RenderDispatch,
        };

        [Test]
        public void DisabledRecorder_DoesNotCapturePhaseOccurrences()
        {
            var diagnostics = new BattleTickPhaseDiagnostics();

            diagnostics.BeginTick(1);
            diagnostics.BeginPhase(BattleTickPhase.BattleFlow);
            diagnostics.EndPhase(BattleTickPhase.BattleFlow);
            diagnostics.EndTick();

            Assert.That(diagnostics.LastPhaseSequenceCount, Is.Zero);
            Assert.That(diagnostics.LastPhaseSequenceOverflowed, Is.False);
            Assert.That(diagnostics.TryGetLastPhaseAt(0, out _), Is.False);
        }

        [Test]
        public void FullEmptyWorldTick_CapturesCurrentThirtyThreeOccurrenceSequence()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(
                diagnostics.LastPhaseSequenceCount,
                Is.EqualTo(CurrentFullTickSequence.Length));
            Assert.That(diagnostics.LastPhaseSequenceOverflowed, Is.False);
            for (int index = 0; index < CurrentFullTickSequence.Length; index++)
            {
                Assert.That(diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase actual), Is.True);
                Assert.That(actual, Is.EqualTo(CurrentFullTickSequence[index]));
            }

            world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
        }

        [Test]
        public void FullTick_C25SkeletonPrecedesLegacySerialAndRemainingDifferenceIsC25Behavior()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(
                NTSD28BattlePassOrder.GetAt(6).Id,
                Is.EqualTo(NTSD28BattlePassId.CoreInputPhaseAdvance));
            Assert.That(
                NTSD28BattlePassOrder.GetAt(7).Id,
                Is.EqualTo(NTSD28BattlePassId.CoreSparkAdvance));
            Assert.That(
                NTSD28BattlePassOrder.GetAt(8).Id,
                Is.EqualTo(NTSD28BattlePassId.CoreProducerSampleScan));
            Assert.That(
                NTSD28BattlePassOrder.GetAt(9).Id,
                Is.EqualTo(NTSD28BattlePassId.CoreProxyAndInputRouteScan));
            Assert.That(
                NTSD28BattlePassOrder.GetAt(10).Id,
                Is.EqualTo(NTSD28BattlePassId.CoreFrameMotion));
            Assert.That(
                NTSD28BattlePassOrder.GetAt(11).Id,
                Is.EqualTo(NTSD28BattlePassId.CoreTeleport));
            Assert.That(diagnostics.TryGetLastPhaseAt(0, out BattleTickPhase first), Is.True);
            Assert.That(first, Is.EqualTo(BattleTickPhase.BattleFlow));
            Assert.That(diagnostics.TryGetLastPhaseAt(1, out BattleTickPhase second), Is.True);
            Assert.That(second, Is.EqualTo(BattleTickPhase.NativeSparkAdvance));
            Assert.That(diagnostics.TryGetLastPhaseAt(2, out BattleTickPhase third), Is.True);
            Assert.That(third, Is.EqualTo(BattleTickPhase.HumanInput));
            Assert.That(diagnostics.TryGetLastPhaseAt(3, out BattleTickPhase fourth), Is.True);
            Assert.That(fourth, Is.EqualTo(BattleTickPhase.CharacterInput));
            Assert.That(diagnostics.TryGetLastPhaseAt(4, out BattleTickPhase fifth), Is.True);
            Assert.That(fifth, Is.EqualTo(BattleTickPhase.FrameMotion));
            Assert.That(diagnostics.TryGetLastPhaseAt(5, out BattleTickPhase sixth), Is.True);
            Assert.That(sixth, Is.EqualTo(BattleTickPhase.NativeTeleport));
            Assert.That(diagnostics.TryGetLastPhaseAt(6, out BattleTickPhase seventh), Is.True);
            Assert.That(seventh, Is.EqualTo(BattleTickPhase.NestedPhysics));
            Assert.That(diagnostics.TryGetLastPhaseAt(7, out BattleTickPhase eighth), Is.True);
            Assert.That(eighth, Is.EqualTo(BattleTickPhase.Revival));
            Assert.That(diagnostics.TryGetLastPhaseAt(8, out BattleTickPhase ninth), Is.True);
            Assert.That(ninth, Is.EqualTo(BattleTickPhase.StageBounds));
            Assert.That(diagnostics.TryGetLastPhaseAt(9, out BattleTickPhase tenth), Is.True);
            Assert.That(tenth, Is.EqualTo(BattleTickPhase.HeldProcess));
            Assert.That(diagnostics.TryGetLastPhaseAt(10, out BattleTickPhase eleventh), Is.True);
            Assert.That(eleventh, Is.EqualTo(BattleTickPhase.CollisionSnapshot));
            Assert.That(diagnostics.TryGetLastPhaseAt(11, out BattleTickPhase twelfth), Is.True);
            Assert.That(twelfth, Is.EqualTo(BattleTickPhase.PairVRest));
            Assert.That(diagnostics.TryGetLastPhaseAt(12, out BattleTickPhase candidate), Is.True);
            Assert.That(candidate, Is.EqualTo(BattleTickPhase.CandidateCollect));
            Assert.That(diagnostics.TryGetLastPhaseAt(13, out BattleTickPhase fusion), Is.True);
            Assert.That(fusion, Is.EqualTo(BattleTickPhase.RuntimeMaintenance));
            Assert.That(diagnostics.TryGetLastPhaseAt(14, out BattleTickPhase weaponCount), Is.True);
            Assert.That(weaponCount, Is.EqualTo(BattleTickPhase.ActiveWeaponCount));
            Assert.That(diagnostics.TryGetLastPhaseAt(15, out BattleTickPhase hit), Is.True);
            Assert.That(hit, Is.EqualTo(BattleTickPhase.CharacterHitConsumePostInteraction));
            Assert.That(diagnostics.TryGetLastPhaseAt(16, out BattleTickPhase drop), Is.True);
            Assert.That(drop, Is.EqualTo(BattleTickPhase.RandomWeaponDrop));
            Assert.That(diagnostics.TryGetLastPhaseAt(17, out BattleTickPhase objectHit), Is.True);
            Assert.That(objectHit, Is.EqualTo(BattleTickPhase.ObjectHitConsume));
            Assert.That(diagnostics.TryGetLastPhaseAt(21, out BattleTickPhase held), Is.True);
            Assert.That(held, Is.EqualTo(BattleTickPhase.HeldProcess));
            Assert.That(diagnostics.TryGetLastPhaseAt(22, out BattleTickPhase bounds), Is.True);
            Assert.That(bounds, Is.EqualTo(BattleTickPhase.PreFrameBounds));
            Assert.That(diagnostics.TryGetLastPhaseAt(23, out BattleTickPhase impulse), Is.True);
            Assert.That(impulse, Is.EqualTo(BattleTickPhase.FramePostProcess));
            Assert.That(diagnostics.TryGetLastPhaseAt(24, out BattleTickPhase resource), Is.True);
            Assert.That(resource, Is.EqualTo(BattleTickPhase.NativeResourceTick));
            Assert.That(diagnostics.TryGetLastPhaseAt(25, out BattleTickPhase frame), Is.True);
            Assert.That(frame, Is.EqualTo(BattleTickPhase.NativeFrameTick));
            Assert.That(diagnostics.TryGetLastPhaseAt(26, out BattleTickPhase c25), Is.True);
            Assert.That(c25, Is.EqualTo(BattleTickPhase.LateEntityUpdate));
            Assert.That(diagnostics.TryGetLastPhaseAt(27, out BattleTickPhase serial), Is.True);
            Assert.That(serial, Is.EqualTo(BattleTickPhase.FrameAdvance));

            world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
        }

        [Test]
        public void InputClearPartialTick_CapturesOnlyTheFourExecutedOccurrences()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            world.SetNeedClearInput(true);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            BattleTickPhase[] expected =
            {
                BattleTickPhase.BattleFlow,
                BattleTickPhase.NativeSparkAdvance,
                BattleTickPhase.HumanInput,
                BattleTickPhase.InputClear,
            };
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase actual), Is.True);
                Assert.That(actual, Is.EqualTo(expected[index]));
            }

            world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
        }

        [Test]
        public void Recorder_PreservesFirstSixtyFourOccurrencesAndMarksOverflow()
        {
            var diagnostics = new BattleTickPhaseDiagnostics();
            diagnostics.SetEnabled(true);
            diagnostics.BeginTick(7);

            for (int index = 0; index < 70; index++)
            {
                BattleTickPhase phase = (BattleTickPhase)(
                    index % BattleTickPhaseDiagnostics.PhaseCount);
                diagnostics.BeginPhase(phase);
                diagnostics.EndPhase(phase);
            }
            diagnostics.EndTick();

            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(64));
            Assert.That(diagnostics.LastPhaseSequenceOverflowed, Is.True);
            for (int index = 0; index < 64; index++)
            {
                Assert.That(diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase actual), Is.True);
                Assert.That(
                    actual,
                    Is.EqualTo((BattleTickPhase)(
                        index % BattleTickPhaseDiagnostics.PhaseCount)));
            }
            Assert.That(diagnostics.TryGetLastPhaseAt(64, out _), Is.False);
        }

        [Test]
        public void SequenceQuery_RemainsAllocationFreeAfterWarmup()
        {
            var diagnostics = new BattleTickPhaseDiagnostics();
            diagnostics.SetEnabled(true);
            diagnostics.BeginTick(9);
            diagnostics.BeginPhase(BattleTickPhase.BattleFlow);
            diagnostics.EndPhase(BattleTickPhase.BattleFlow);
            diagnostics.EndTick();
            _ = diagnostics.TryGetLastPhaseAt(0, out _);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                if (diagnostics.TryGetLastPhaseAt(0, out BattleTickPhase phase))
                    checksum ^= (int)phase + index;
                checksum ^= diagnostics.LastPhaseSequenceCount << 8;
                if (diagnostics.LastPhaseSequenceOverflowed)
                    checksum ^= 1 << 20;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(checksum, Is.Not.EqualTo(int.MinValue));
        }
    }
}
#endif
