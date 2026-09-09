#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NUnit.Framework;

using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25NestedTailSkeletonEditorTests
    {
        [Test]
        public void FullTick_PlacesC25SkeletonImmediatelyAfterC24_AndRendersCompletedTail()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(PhaseAt(diagnostics, 25),
                Is.EqualTo(BattleTickPhase.NativeResourceTick));
            Assert.That(PhaseAt(diagnostics, 26),
                Is.EqualTo(BattleTickPhase.NativeFrameTick));
            Assert.That(PhaseAt(diagnostics, 27),
                Is.EqualTo(BattleTickPhase.LateEntityUpdate),
                "C25 must begin immediately after C24 on the normal full-return path.");
            Assert.That(PhaseAt(diagnostics, 28),
                Is.EqualTo(BattleTickPhase.FrameAdvance),
                "The unclassified legacy serial remainder must stay outside and after C25.");
            Assert.That(PhaseAt(diagnostics, 29),
                Is.EqualTo(BattleTickPhase.Stage));
            Assert.That(PhaseAt(diagnostics, 30),
                Is.EqualTo(BattleTickPhase.RandomWeaponDropTail));
            Assert.That(PhaseAt(diagnostics, 31),
                Is.EqualTo(BattleTickPhase.EntityPostFrameTail));
            Assert.That(PhaseAt(diagnostics, 32),
                Is.EqualTo(BattleTickPhase.BattleResults));
            Assert.That(PhaseAt(diagnostics, 33),
                Is.EqualTo(BattleTickPhase.RenderDispatch),
                "Presentation must freeze only after the normal tick tail and Results host writer.");
            Assert.That(diagnostics.LastPhaseSequenceCount, Is.EqualTo(34));
        }

        [Test]
        public void StepWaitTick_PreservesLegacySkipOfC25AndPostTail()
        {
            var world = new SimulationWorld();
            world.Runtime.Flow.BattleStepMode = 1;
            BattleTickPhaseDiagnostics diagnostics =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

            new NTSDBattleTickSystem(world).RunReleaseTick(
                2,
                buildPresentation: false);

            Assert.That(Contains(diagnostics, BattleTickPhase.LateEntityUpdate),
                Is.False);
            Assert.That(Contains(diagnostics, BattleTickPhase.RandomWeaponDropTail),
                Is.False);
            Assert.That(Contains(diagnostics, BattleTickPhase.EntityPostFrameTail),
                Is.False);
            Assert.That(Contains(diagnostics, BattleTickPhase.BattleResults),
                Is.False);
            Assert.That(Contains(diagnostics, BattleTickPhase.FrameAdvance),
                Is.True);
            Assert.That(Contains(diagnostics, BattleTickPhase.Stage), Is.True);
            Assert.That(Contains(diagnostics, BattleTickPhase.RenderDispatch), Is.True);
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

        private static bool Contains(
            BattleTickPhaseDiagnostics diagnostics,
            BattleTickPhase expected)
        {
            for (int index = 0;
                 index < diagnostics.LastPhaseSequenceCount;
                 index++)
            {
                if (diagnostics.TryGetLastPhaseAt(
                        index,
                        out BattleTickPhase actual) &&
                    actual == expected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif

