#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Tools;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class SimulationTickHostPolicyEditorTests
    {
        [Test]
        public void OfflineLocal_UsesExactCadenceAndDrainsAtMostTwoIntervalsPerUpdate()
        {
            Assert.That(
                SimulationConstants.SIM_DT,
                Is.EqualTo(0.033f).Within(0.0000001f));
            Assert.That(
                SimulationConstants.FAST_SIM_DT,
                Is.EqualTo(0.003f).Within(0.0000001f));
            var settings = new LockstepSimulationSettings
            {
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
            };
            settings.Normalize();
            var policy = new OfflineLocalTickPolicy();

            policy.BeginUpdate(SimulationConstants.SIM_DT * 4f, settings);

            Assert.That(policy.ShouldAttemptAutomaticTick(0, settings), Is.True);
            Assert.That(
                policy.ShouldBuildPresentationForNextTick(0, settings),
                Is.True);
            policy.CommitAutomaticTick();
            Assert.That(
                policy.Accumulator,
                Is.EqualTo(SimulationConstants.SIM_DT).Within(0.0000001f));
            Assert.That(policy.ShouldAttemptAutomaticTick(1, settings), Is.True);
            Assert.That(
                policy.ShouldBuildPresentationForNextTick(1, settings),
                Is.True);
            policy.CommitAutomaticTick();
            Assert.That(policy.Accumulator, Is.Zero.Within(0.0000001f));
            Assert.That(policy.ShouldAttemptAutomaticTick(2, settings), Is.False);

            policy.BeginUpdate(0f, settings);
            Assert.That(
                policy.ShouldAttemptAutomaticTick(0, settings),
                Is.False,
                "two active intervals were drained without unbounded catch-up");
        }

        [Test]
        public void OfflineLocal_CapsWallClockDebtAtTwoActiveIntervals()
        {
            var settings = new LockstepSimulationSettings
            {
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
            };
            settings.Normalize();
            var policy = new OfflineLocalTickPolicy();

            policy.BeginUpdate(SimulationConstants.SIM_DT * 20f, settings);

            Assert.That(
                policy.Accumulator,
                Is.EqualTo(SimulationConstants.SIM_DT * 2f)
                    .Within(0.0000001f));
        }

        [Test]
        public void OfflineLocal_CadenceChangeClearsDebtAndSameModePreservesIt()
        {
            var settings = new LockstepSimulationSettings();
            var policy = new OfflineLocalTickPolicy();
            policy.BeginUpdate(SimulationConstants.SIM_DT, settings);

            Assert.That(
                policy.SetCadenceMode(SimulationHostCadenceMode.Fast),
                Is.True);
            Assert.That(policy.Accumulator, Is.Zero);
            Assert.That(
                policy.ActiveIntervalSeconds,
                Is.EqualTo(0.003f).Within(0.0000001f));

            policy.BeginUpdate(0.02f, settings);
            Assert.That(
                policy.Accumulator,
                Is.EqualTo(0.006f).Within(0.0000001f));
            Assert.That(
                policy.SetCadenceMode(SimulationHostCadenceMode.Fast),
                Is.False);
            Assert.That(
                policy.Accumulator,
                Is.EqualTo(0.006f).Within(0.0000001f));

            Assert.That(
                policy.SetCadenceMode(SimulationHostCadenceMode.Normal),
                Is.True);
            Assert.That(policy.Accumulator, Is.Zero);
        }

        [Test]
        public void HostControl_AppliesF1ThenF2ThenF5WithPausedOnlySingleStep()
        {
            SimulationHostControlTransition runningF2 =
                SimulationHostControl.Apply(
                    SimulationHostControlCommand.SingleStep,
                    new SimulationHostControlState(
                        paused: false,
                        SimulationHostCadenceMode.Normal));
            Assert.That(runningF2.RequestSingleStep, Is.False);
            Assert.That(runningF2.State.Paused, Is.False);

            SimulationHostControlTransition pausedF2 =
                SimulationHostControl.Apply(
                    SimulationHostControlCommand.SingleStep,
                    new SimulationHostControlState(
                        paused: true,
                        SimulationHostCadenceMode.Normal));
            Assert.That(pausedF2.RequestSingleStep, Is.True);
            Assert.That(pausedF2.State.Paused, Is.True);

            SimulationHostControlTransition folded =
                SimulationHostControl.Apply(
                    SimulationHostControlCommand.TogglePause |
                    SimulationHostControlCommand.SingleStep |
                    SimulationHostControlCommand.ToggleFastMode,
                    new SimulationHostControlState(
                        paused: false,
                        SimulationHostCadenceMode.Normal));
            Assert.That(folded.State.Paused, Is.True);
            Assert.That(folded.RequestSingleStep, Is.True);
            Assert.That(folded.State.CadenceMode, Is.EqualTo(
                SimulationHostCadenceMode.Fast));
            Assert.That(folded.CadenceChanged, Is.True);
        }

        [Test]
        public void HostPhysicalEdgeLatch_EmitsOnlyFalseToTrueAndReleaseRearms()
        {
            var latch = new SimulationHostControlPhysicalEdgeLatch();

            Assert.That(
                latch.Capture(f1Pressed: false, f2Pressed: false, f5Pressed: false),
                Is.EqualTo(SimulationHostControlCommand.None));
            Assert.That(
                latch.Capture(f1Pressed: true, f2Pressed: true, f5Pressed: true),
                Is.EqualTo(
                    SimulationHostControlCommand.TogglePause |
                    SimulationHostControlCommand.SingleStep |
                    SimulationHostControlCommand.ToggleFastMode));
            Assert.That(
                latch.Capture(f1Pressed: true, f2Pressed: true, f5Pressed: true),
                Is.EqualTo(SimulationHostControlCommand.None),
                "held keys must not repeat");
            Assert.That(
                latch.Capture(f1Pressed: false, f2Pressed: false, f5Pressed: false),
                Is.EqualTo(SimulationHostControlCommand.None));
            Assert.That(
                latch.Capture(f1Pressed: true, f2Pressed: false, f5Pressed: true),
                Is.EqualTo(
                    SimulationHostControlCommand.TogglePause |
                    SimulationHostControlCommand.ToggleFastMode),
                "release must rearm each key independently");

            latch.Clear();
            Assert.That(
                latch.Capture(f1Pressed: true, f2Pressed: true, f5Pressed: true),
                Is.EqualTo(
                    SimulationHostControlCommand.TogglePause |
                    SimulationHostControlCommand.SingleStep |
                    SimulationHostControlCommand.ToggleFastMode),
                "clear must discard held state across lifecycle boundaries");
        }

        [Test]
        public void DriverHostControl_RunningF2IsDroppedAndPausedF2AdvancesExactlyOnce()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            driver.SetPaused(false);
            Assert.That(
                driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Running));
            int initialTick = driver.CurrentTickIndex;

            driver.QueueHostControlCommandsForDiagnostics(
                SimulationHostControlCommand.SingleStep);
            Assert.That(
                driver.ProcessHostControlCommandsForDiagnostics(),
                Is.False);
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(initialTick));

            driver.QueueHostControlCommandsForDiagnostics(
                SimulationHostControlCommand.TogglePause);
            Assert.That(
                driver.ProcessHostControlCommandsForDiagnostics(),
                Is.False);
            Assert.That(driver.IsPaused, Is.True);
            Assert.That(driver.RemainingAccumulatorTime, Is.Zero);

            driver.QueueHostControlCommandsForDiagnostics(
                SimulationHostControlCommand.SingleStep);
            bool advanced = driver.ProcessHostControlCommandsForDiagnostics();
            Assert.That(
                advanced,
                Is.True,
                $"paused F2 must advance exactly one production tick; " +
                $"lifecycle={driver.LifecycleState}, paused={driver.IsPaused}, " +
                $"drive={driver.Settings.driveMode}, worldNull={driver.World == null}, " +
                $"reason={driver.HostControlLastFailureReasonForDiagnostics}");
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(initialTick + 1));
            Assert.That(driver.IsPaused, Is.True);
            Assert.That(driver.RemainingAccumulatorTime, Is.Zero);
            Assert.That(
                driver.ProcessHostControlCommandsForDiagnostics(),
                Is.False);
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(initialTick + 1));

            driver.QueueHostControlCommandsForDiagnostics(
                SimulationHostControlCommand.ToggleFastMode);
            driver.ProcessHostControlCommandsForDiagnostics();
            Assert.That(
                driver.IsFastMode,
                Is.True,
                "F5 must toggle LocalFreeRun into fast cadence");
            Assert.That(
                driver.ActiveHostIntervalSeconds,
                Is.EqualTo(0.003f).Within(0.0000001f));

            driver.QueueHostControlCommandsForDiagnostics(
                SimulationHostControlCommand.TogglePause);
            driver.ProcessHostControlCommandsForDiagnostics();
            Assert.That(driver.IsPaused, Is.False);
        }

        [Test]
        public void NativeResultTransitionRejectsAutomaticExplicitAndPausedOldWorldTicks()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            int nextTick = driver.CurrentTickIndex + 1;
            MethodInfo canAdvance = typeof(SimulationTickDriver).GetMethod(
                "CanAdvanceTick",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(canAdvance, Is.Not.Null);
            Assert.That(canAdvance.Invoke(driver, new object[] { nextTick }), Is.True,
                "An active battle must still admit its next host tick.");

            SimulationWorld world = driver.World;
            FrameInputSet previousInput = world.CurrentAppliedFrameInputForResults;
            world.Runtime.Results.NativeResultPhase = 3;
            world.Runtime.Results.NativeTransitionState = 202;

            Assert.That(canAdvance.Invoke(driver, new object[] { nextTick }), Is.False,
                "Automatic ticks must stop after the native result transition.");
            Assert.That(driver.StepOneTick(
                new FrameInputSet(nextTick, Array.Empty<SimulationPlayerInput>()),
                ignorePaused: true,
                buildPresentation: false), Is.False,
                "An explicit Manual/Lockstep frame must not enter the old battle World.");
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(nextTick - 1));
            Assert.That(world.CurrentAppliedFrameInputForResults, Is.SameAs(previousInput));

            driver.SetPaused(true);
            driver.QueueHostControlCommandsForDiagnostics(
                SimulationHostControlCommand.SingleStep);
            Assert.That(driver.ProcessHostControlCommandsForDiagnostics(), Is.False,
                "Paused F2 cannot bypass a completed battle result transition.");
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(nextTick - 1));
        }

        [Test]
        public void ManualAndNetworkPolicies_NeverConsumeWallClockAutomatically()
        {
            var settings = new LockstepSimulationSettings();
            SimulationTickHostPolicy[] policies =
            {
                new ManualReplayTickPolicy(),
                new NetworkLockstepTickPolicy(),
            };

            foreach (SimulationTickHostPolicy policy in policies)
            {
                policy.BeginUpdate(10f, settings);
                Assert.That(policy.UsesWallClock, Is.False);
                Assert.That(policy.Accumulator, Is.Zero);
                Assert.That(
                    policy.ShouldAttemptAutomaticTick(0, settings),
                    Is.False);
                Assert.That(
                    policy.ShouldBuildPresentationForNextTick(0, settings),
                    Is.True);
            }
        }

        private sealed class DriverScope : IDisposable
        {
            private static readonly PropertyInfo InstanceProperty =
                typeof(SingletonBehaviour<SimulationTickDriver>).GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static);

            private readonly SimulationTickDriver previousInstance;
            private readonly GameObject host;

            internal DriverScope()
            {
                previousInstance = SimulationTickDriver.Instance;
                host = new GameObject("__NTSD28_HostControlTest")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                SetInstance(Driver);
                Driver.RecreateWorld();
                Driver.SetFrameInputProvider(null);
            }

            internal SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                if (Driver != null)
                {
                    BattleRuntimeShutdownReport report =
                        Driver.ShutdownBattleRuntime();
                    if (report.Status != BattleRuntimeShutdownStatus.Failed)
                    {
                        Driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true);
                    }
                }
                SetInstance(null);
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
                SetInstance(previousInstance);
            }

            private static void SetInstance(SimulationTickDriver value)
            {
                MethodInfo setter = InstanceProperty?.GetSetMethod(true);
                if (setter == null)
                {
                    throw new MissingMethodException(
                        "SimulationTickDriver singleton setter was not found.");
                }
                setter.Invoke(null, new object[] { value });
            }
        }
    }
}
#endif
