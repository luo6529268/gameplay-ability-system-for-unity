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
        public void ResultsPageVisibilityDoesNotSuppressCombatHumanInput()
        {
            var inactiveWorld = new SimulationWorld();
            var activeWorld = new SimulationWorld();
            activeWorld.Runtime.Results.Phase = 200;

            new NTSDBattleTickSystem(inactiveWorld).RunReleaseTick(1, false);
            new NTSDBattleTickSystem(activeWorld).RunReleaseTick(1, false);

            Assert.That(inactiveWorld.Runtime.Flow.HumanInputPolledExternally, Is.True);
            Assert.That(activeWorld.Runtime.Results.IsActive, Is.True);
            Assert.That(activeWorld.Runtime.Flow.HumanInputPolledExternally, Is.True,
                "Unity result-page visibility must not suppress pre-transition combat input.");
        }

        [Test]
        public void ResultsActiveTickConsumesBattleEntryClearThenRunsWorldTail()
        {
            var world = new SimulationWorld();
            world.Runtime.Results.Phase = 200;
            world.SetInitStatsRequest(1);
            world.SetNeedClearInput(true);

            new NTSDBattleTickSystem(world).RunReleaseTick(
                1,
                buildPresentation: false);

            Assert.That(world.Runtime.Flow.HumanInputPolledExternally, Is.True);
            Assert.That(world.NeedClearInput, Is.False,
                "The battle-entry clear request belongs to combat, regardless of result-page visibility.");
            Assert.That(world.InitStatsRequest, Is.EqualTo(1),
                "The battle-entry clear gate returns before the world tail on this tick.");

            new NTSDBattleTickSystem(world).RunReleaseTick(
                2,
                buildPresentation: false);
            Assert.That(
                world.InitStatsRequest,
                Is.Zero,
                "The next pre-transition combat tick completes the world post-frame tail.");
            Assert.That(
                world.Runtime.Flow.HumanInputPolledExternally,
                Is.True,
                "Result-page visibility does not suppress participant input.");
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
            Assert.That(world.Runtime.Flow.HumanInputPolledExternally, Is.True);
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
