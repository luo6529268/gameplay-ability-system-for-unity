#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25LState18ParticleOwnerEditorTests
    {
        [Test]
        public void PlacementIsAfterFrameZeroOpointAndBeforePreviousActionCommitAndCleanup()
        {
            string source = File.ReadAllText(Path.Combine(
                UnityEngine.Application.dataPath,
                "NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs"));
            int opoint = source.IndexOf(
                "world.StructuralWriter.ProcessLateOpointSegment(",
                System.StringComparison.Ordinal);
            int state18 = source.IndexOf(
                "obj.RunNativeC25State18BrokenWeaponParticles()",
                System.StringComparison.Ordinal);
            int cleanup = source.IndexOf(
                "obj.TryRunLatePostOpointCleanupPhase();",
                System.StringComparison.Ordinal);
            int commit = source.IndexOf(
                "obj.MirrorLatePrevFrame();",
                System.StringComparison.Ordinal);

            Assert.That(opoint, Is.GreaterThanOrEqualTo(0));
            Assert.That(state18, Is.GreaterThan(opoint));
            Assert.That(commit, Is.GreaterThan(state18));
            Assert.That(cleanup, Is.GreaterThan(commit));
        }

        [Test]
        public void DecisionKernelMatchesApplicabilityDelayAndLeavingCounts()
        {
            Assert.That(
                BattleNativeState18ParticleKernel.ResolvePreRollCount(0, 0, 0),
                Is.Zero);
            Assert.That(
                BattleNativeState18ParticleKernel.ResolvePreRollCount(18, 0, 0),
                Is.EqualTo(7));
            Assert.That(
                BattleNativeState18ParticleKernel.ResolvePreRollCount(19, 18, 0),
                Is.EqualTo(-1));
            Assert.That(
                BattleNativeState18ParticleKernel.ResolvePreRollCount(18, 19, 2),
                Is.Zero);
        }

        [Test]
        public void SustainedStateConsumesOnlySelectionRollWhenEffectResourceIsUnavailable()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(
                world,
                previousState: 18,
                currentState: 18);
            ulong before = world.NativeRandom.CaptureScalarState().SynchronizedCalls;

            world.LateEntityUpdateAll(1);

            Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls - before, Is.EqualTo(1));
            Assert.That(character.Frame.Prev, Is.EqualTo(character.Frame.N));
        }

        [Test]
        public void LeavingStateUsesSevenPlanWithoutSelectionRollAndStillCommitsPreviousAction()
        {
            var world = new SimulationWorld();
            LF2Character character = CreateCharacter(
                world,
                previousState: 18,
                currentState: LF2States.Standing);
            ulong before = world.NativeRandom.CaptureScalarState().SynchronizedCalls;

            world.LateEntityUpdateAll(1);

            Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls - before, Is.Zero);
            Assert.That(character.Frame.Prev, Is.EqualTo(0));
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int previousState,
            int currentState)
        {
            var data = new LF2CharacterData
            {
                name = "C25lState18",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = currentState,
                        wait = 100,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                    },
                    new LF2FrameData
                    {
                        frameId = 1,
                        state = previousState,
                        wait = 100,
                        next = 1,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = 8100;
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(character.ObjectId, data));
            character.Initialize(500, 500);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Frame.Prev = 1;
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Trans.SyncDirectFrameData(100, 0, 0);
            character.SetRequiredRuntimeSlot(50);
            character.Runtime.SuppressLateFrameTickUntilTick = 2;
            world.Register(character);
            return character;
        }
    }
}
#endif
