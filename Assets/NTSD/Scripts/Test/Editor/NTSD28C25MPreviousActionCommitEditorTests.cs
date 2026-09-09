#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28C25MPreviousActionCommitEditorTests
    {
        [Test]
        public void PlacementCommitsAfterC25LBeforeCleanupAndRunsHealingAfterVirtualTail()
        {
            string source = File.ReadAllText(Path.Combine(
                UnityEngine.Application.dataPath,
                "NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs"));
            int state18 = source.IndexOf(
                "obj.RunNativeC25State18BrokenWeaponParticles()",
                StringComparison.Ordinal);
            int commit = source.IndexOf(
                "obj.MirrorLatePrevFrame();",
                StringComparison.Ordinal);
            int cleanup = source.IndexOf(
                "obj.TryRunLatePostOpointCleanupPhase();",
                StringComparison.Ordinal);
            int virtualTail = source.IndexOf(
                "obj.RunLateTailAfterNativePreviousActionCommit(",
                StringComparison.Ordinal);
            int healing = source.IndexOf(
                "AdvanceNativeHealing(obj);",
                StringComparison.Ordinal);

            Assert.That(commit, Is.GreaterThan(state18));
            Assert.That(cleanup, Is.GreaterThan(commit));
            Assert.That(virtualTail, Is.GreaterThan(cleanup));
            Assert.That(healing, Is.GreaterThan(virtualTail));
        }

        [Test]
        public void ProductionVirtualTailSeesCommittedFieldAndExplicitOldPreviousActionContext()
        {
            var world = new SimulationWorld();
            ProbeCharacter probe = CreateProbe(world);

            world.LateEntityUpdateAll(1);

            Assert.That(probe.TailCallCount, Is.EqualTo(1));
            Assert.That(probe.ObservedFramePrevious, Is.Zero);
            Assert.That(probe.ObservedContextPrevious, Is.EqualTo(1));
            Assert.That(probe.Frame.Prev, Is.Zero);
            Assert.That(
                probe.ResolveLateTransitionPreviousActionForWorldPass(),
                Is.Zero);
        }

        [Test]
        public void TransientPreviousActionContextIsClearedWhenVirtualTailThrows()
        {
            var probe = new ProbeCharacter
            {
                ThrowFromTail = true,
            };
            probe.ModuleInitialize();
            probe.Frame.N = 0;
            probe.Frame.Prev = 0;

            Assert.Throws<InvalidOperationException>(() =>
                probe.RunLateTailAfterNativePreviousActionCommit(7));
            Assert.That(
                probe.ResolveLateTransitionPreviousActionForWorldPass(),
                Is.Zero);
        }

        private static ProbeCharacter CreateProbe(SimulationWorld world)
        {
            var data = new LF2CharacterData
            {
                name = "C25mPreviousAction",
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 100,
                        next = 0,
                        centerx = 39,
                        centery = 79,
                    },
                    new LF2FrameData
                    {
                        frameId = 1,
                        state = LF2States.Burning,
                        wait = 100,
                        next = 1,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            var probe = new ProbeCharacter();
            probe.ModuleInitialize();
            probe.ObjectId = 8200;
            probe.FrameCache.Load(
                new LF2CharacterDataWrapper(probe.ObjectId, data));
            probe.Initialize(500, 500);
            probe.Frame.N = 0;
            probe.Frame.PN = 0;
            probe.Frame.Prev = 1;
            probe.Frame.D = probe.FrameCache.GetFrameDataById(0);
            probe.Trans.SyncDirectFrameData(100, 0, 0);
            probe.SetRequiredRuntimeSlot(50);
            probe.Runtime.SuppressLateFrameTickUntilTick = 2;
            world.Register(probe);
            return probe;
        }

        private sealed class ProbeCharacter : LF2Character
        {
            internal int TailCallCount { get; private set; }
            internal int ObservedFramePrevious { get; private set; }
            internal int ObservedContextPrevious { get; private set; }
            internal bool ThrowFromTail { get; set; }

            internal override void RunLateTailBeforePrevFrame()
            {
                TailCallCount++;
                ObservedFramePrevious = Frame?.Prev ?? -1;
                ObservedContextPrevious =
                    ResolveLateTransitionPreviousActionForWorldPass();
                if (ThrowFromTail)
                    throw new InvalidOperationException("C25m probe");
            }
        }
    }
}
#endif
