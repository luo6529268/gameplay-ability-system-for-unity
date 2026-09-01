#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleResultsSceneHostTickAlignmentEditorTests
    {
        [Test]
        public void ResultsActiveTickRunsFullWorldTailWithoutEntityHumanPoll()
        {
            var world = new SimulationWorld();
            world.Runtime.Results.Phase = 200;
            world.SetInitStatsRequest(1);
            world.SetNeedClearInput(true);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(
                world.InitStatsRequest,
                Is.Zero,
                "C++ SceneState::RESULTS still completes the world post-frame tail.");
            Assert.That(
                world.Runtime.Flow.HumanInputPolledExternally,
                Is.False,
                "C++ Results passes a null post-cooldown battle-entity input callback.");
            Assert.That(
                world.NeedClearInput,
                Is.True,
                "A null Results entity-input callback must not consume the battle-entry clear request.");
        }

        [Test]
        public void ResultsHostUsesExplicitP1P2PressedEdgesAfterWorldTick()
        {
            MethodInfo overload = typeof(NTSDBattleTickSystem).GetMethod(
                nameof(NTSDBattleTickSystem.RunReleaseTick),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[] { typeof(int), typeof(bool), typeof(FrameInputSet) },
                modifiers: null);
            Assert.That(
                overload,
                Is.Not.Null,
                "Results host input must be supplied explicitly without polling entity input.");

            var world = new SimulationWorld();
            world.Runtime.Results.Phase = 200;
            world.Runtime.Results.Cursor = 6;
            var tickSystem = new NTSDBattleTickSystem(world);
            var frame = new FrameInputSet(1, new[]
            {
                new SimulationPlayerInput(
                    2,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.Attack),
                new SimulationPlayerInput(
                    1,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.Attack),
            });

            overload.Invoke(tickSystem, new object[] { 1, false, frame });

            Assert.That(world.Runtime.Results.Phase, Is.EqualTo(202));
            Assert.That(world.Runtime.Results.SettingsCursor, Is.EqualTo(2));
            Assert.That(world.Runtime.Flow.HumanInputPolledExternally, Is.False);
        }

        [Test]
        public void ResultsHostDoesNotRetriggerFromHeldOnlyOrNonP1P2PressedInput()
        {
            MethodInfo overload = typeof(NTSDBattleTickSystem).GetMethod(
                nameof(NTSDBattleTickSystem.RunReleaseTick),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[] { typeof(int), typeof(bool), typeof(FrameInputSet) },
                modifiers: null);
            Assert.That(overload, Is.Not.Null);

            var world = new SimulationWorld();
            world.Runtime.Results.Phase = 200;
            world.Runtime.Results.Cursor = 6;
            var tickSystem = new NTSDBattleTickSystem(world);
            var frame = new FrameInputSet(1, new[]
            {
                new SimulationPlayerInput(
                    0,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.None),
                new SimulationPlayerInput(
                    2,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.Attack),
            });

            overload.Invoke(tickSystem, new object[] { 1, false, frame });

            Assert.That(world.Runtime.Results.Phase, Is.EqualTo(200));
            Assert.That(world.Runtime.Results.Cursor, Is.EqualTo(6));
        }
    }
}
#endif
