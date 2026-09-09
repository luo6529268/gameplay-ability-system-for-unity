#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using System.Threading;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Tools;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28NativeFunctionKeyProductionIntegrationEditorTests
    {
        [Test]
        public void PhysicalLatch_EmitsRisingSessionBitsAndReleaseRearms()
        {
            var latch = new NTSD28NativeFunctionKeyPhysicalLatch();
            ushort sessionMask = Mask(
                NTSD28NativeFunctionKey.F3,
                NTSD28NativeFunctionKey.F6,
                NTSD28NativeFunctionKey.F7,
                NTSD28NativeFunctionKey.F8,
                NTSD28NativeFunctionKey.F9);

            latch.CapturePressedMaskForDiagnostics(
                sessionMask,
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            Assert.That(
                latch.TryConsumeOneShotHandoff(
                    out byte firstEvents,
                    out bool firstLeave,
                    out NTSD28NativeFunctionKeyMaintenanceCommand firstMaintenance),
                Is.True);
            Assert.That(firstEvents, Is.EqualTo(0xF4));
            Assert.That(firstLeave, Is.False);
            Assert.That(
                firstMaintenance,
                Is.EqualTo(NTSD28NativeFunctionKeyMaintenanceCommand.None));

            latch.CapturePressedMaskForDiagnostics(
                sessionMask,
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            Assert.That(latch.TryConsumeOneShotHandoff(out _, out _, out _), Is.False);

            latch.CapturePressedMaskForDiagnostics(
                0,
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            latch.CapturePressedMaskForDiagnostics(
                NTSD28NativeFunctionKeyPhysicalLatch.KeyMask(
                    NTSD28NativeFunctionKey.F7),
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            Assert.That(
                latch.TryConsumeOneShotHandoff(
                    out byte rearmedEvents,
                    out _,
                    out _),
                Is.True);
            Assert.That(rearmedEvents, Is.EqualTo(0x20));
        }

        [Test]
        public void PhysicalLatch_RoutesF4MaintenanceAndContinuousF12Priority()
        {
            var latch = new NTSD28NativeFunctionKeyPhysicalLatch();
            ushort pressed = Mask(
                NTSD28NativeFunctionKey.F4,
                NTSD28NativeFunctionKey.F9,
                NTSD28NativeFunctionKey.F10,
                NTSD28NativeFunctionKey.F11,
                NTSD28NativeFunctionKey.F12);

            latch.CapturePressedMaskForDiagnostics(
                pressed,
                true,
                new NTSD28NativeFunctionKeyRouteContext(false, false, true, false));

            Assert.That(
                latch.TryConsumeOneShotHandoff(
                    out byte events,
                    out bool leave,
                    out NTSD28NativeFunctionKeyMaintenanceCommand maintenance),
                Is.True);
            Assert.That(events, Is.Zero);
            Assert.That(leave, Is.False,
                "F4 remains battle-context gated while maintenance bypasses context.");
            Assert.That(
                maintenance,
                Is.EqualTo(
                    NTSD28NativeFunctionKeyMaintenanceCommand.DiscardPendingRecording));
            Assert.That(
                latch.CurrentContinuousHostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.VolumeUp));

            latch.CapturePressedMaskForDiagnostics(
                0,
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            latch.CapturePressedMaskForDiagnostics(
                NTSD28NativeFunctionKeyPhysicalLatch.KeyMask(
                    NTSD28NativeFunctionKey.F4),
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            Assert.That(
                latch.TryConsumeOneShotHandoff(out _, out bool allowedLeave, out _),
                Is.True);
            Assert.That(allowedLeave, Is.True);
            Assert.That(
                latch.CurrentContinuousHostCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.None));
        }

        [Test]
        public void PhysicalLatch_AppliesLockAndDelayOnlyToTheirAuthorityScopes()
        {
            var latch = new NTSD28NativeFunctionKeyPhysicalLatch();
            ushort pressed = Mask(
                NTSD28NativeFunctionKey.F6,
                NTSD28NativeFunctionKey.F7,
                NTSD28NativeFunctionKey.F8,
                NTSD28NativeFunctionKey.F9);

            latch.CapturePressedMaskForDiagnostics(
                pressed,
                false,
                new NTSD28NativeFunctionKeyRouteContext(true, true, true, false));
            Assert.That(latch.TryConsumeOneShotHandoff(out _, out _, out _), Is.False);

            latch.CapturePressedMaskForDiagnostics(
                0,
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            latch.CapturePressedMaskForDiagnostics(
                pressed,
                false,
                new NTSD28NativeFunctionKeyRouteContext(true, true, false, false));
            Assert.That(
                latch.TryConsumeOneShotHandoff(out byte events, out _, out _),
                Is.True);
            Assert.That(events, Is.EqualTo(0x30),
                "F6/F7 ignore delay while F8/F9 require a clear delay.");
        }

        [Test]
        public void PhysicalLatch_RemainsAllocationFreeAfterWarmup()
        {
            var latch = new NTSD28NativeFunctionKeyPhysicalLatch();
            ushort pressed = (ushort)(
                NTSD28NativeFunctionKeyPhysicalLatch.KeyMask(
                    NTSD28NativeFunctionKey.F7) |
                NTSD28NativeFunctionKeyPhysicalLatch.KeyMask(
                    NTSD28NativeFunctionKey.F11));
            latch.CapturePressedMaskForDiagnostics(
                pressed,
                false,
                NTSD28NativeFunctionKeyRouteContext.Allowed);
            latch.TryConsumeOneShotHandoff(out _, out _, out _);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                ushort mask = (index & 1) == 0 ? (ushort)0 : pressed;
                latch.CapturePressedMaskForDiagnostics(
                    mask,
                    false,
                    NTSD28NativeFunctionKeyRouteContext.Allowed);
                if (latch.TryConsumeOneShotHandoff(
                        out byte events,
                        out _,
                        out _))
                {
                    checksum += events;
                }
                checksum += (int)latch.CurrentContinuousHostCommand;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(checksum, Is.Not.Zero);
        }

        [Test]
        public void DriverNativeDiagnostics_ReuseB1HostOwner()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            driver.SetPaused(false);

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F1);
            driver.ProcessHostControlCommandsForDiagnostics();
            Assert.That(driver.IsPaused, Is.True);

            int beforeStep = driver.CurrentTickIndex;
            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F2);
            Assert.That(driver.ProcessHostControlCommandsForDiagnostics(), Is.True);
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(beforeStep + 1));

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F5);
            driver.ProcessHostControlCommandsForDiagnostics();
            Assert.That(driver.IsFastMode, Is.True);
        }

        [Test]
        public void DriverTick_DispatchesF7ExactlyOnceWithoutLegacyEffect()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            driver.SetPaused(true);
            NTSD28NativeFunctionKeySessionState state =
                driver.World.Runtime.FunctionKeys;

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F7);
            Assert.That(driver.StepOneTick(true, false), Is.True);

            Assert.That(state.F7EventCount, Is.EqualTo(1U));
            Assert.That(state.LastAcceptedEventByte, Is.EqualTo(0x20));
            Assert.That(state.PendingFullMp, Is.True);
            Assert.That(driver.World.InitStatsRequest, Is.Zero,
                "Formal F7 must not route into the legacy all-stats request.");

            Assert.That(driver.StepOneTick(true, false), Is.True);
            Assert.That(state.F7EventCount, Is.EqualTo(1U));
            Assert.That(state.LastAcceptedEventByte, Is.Zero,
                "The next empty tick must clear the accepted event byte once.");
        }

        [Test]
        public void DriverTick_PreservesFixedF8F9AndF3LockDispatch()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            driver.SetPaused(true);
            NTSD28NativeFunctionKeySessionState state =
                driver.World.Runtime.FunctionKeys;

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F9);
            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F8);
            Assert.That(driver.StepOneTick(true, false), Is.True);
            Assert.That(state.LastAcceptedEventByte, Is.EqualTo(0xC0));
            Assert.That(
                state.PendingObjectCommand,
                Is.EqualTo(NTSD28NativeFunctionKeyPendingObjectCommand.TerminateObjects));
            Assert.That(driver.World.Mode2Request, Is.Zero,
                "Formal F8/F9 must not route into the legacy mode2 request.");

            state.ResetForBattle();
            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F6);
            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F3);
            Assert.That(driver.StepOneTick(true, false), Is.True);
            Assert.That(state.LockState, Is.EqualTo(2));
            Assert.That(state.LastAcceptedEventByte, Is.EqualTo(0x04));
            Assert.That(state.F6EventCount, Is.Zero);
        }

        [Test]
        public void DriverTick_RechecksSessionMainStateAtTickBoundary()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            driver.SetPaused(true);
            driver.World.Runtime.Match.LocalGameModeId = 1;

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F7);
            Assert.That(driver.StepOneTick(true, false), Is.True);

            Assert.That(driver.World.Runtime.FunctionKeys.F7EventCount, Is.Zero);
            Assert.That(driver.World.Runtime.FunctionKeys.LastAcceptedEventByte, Is.Zero);
        }

        [Test]
        public void DriverNativeDiagnostics_ExposeTypedDeferredHostHandoffs()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F4);
            Assert.That(driver.NativeFunctionKeyLeaveBattleRequestedForDiagnostics,
                Is.True);
            Assert.That(driver.ConsumeNativeFunctionKeyLeaveBattleRequestForDiagnostics(),
                Is.True);
            Assert.That(driver.ConsumeNativeFunctionKeyLeaveBattleRequestForDiagnostics(),
                Is.False);

            driver.QueueNativeFunctionKeyForDiagnostics(
                NTSD28NativeFunctionKey.F10,
                false,
                true);
            Assert.That(
                driver.ConsumeNativeFunctionKeyMaintenanceCommandForDiagnostics(),
                Is.EqualTo(
                    NTSD28NativeFunctionKeyMaintenanceCommand.DiscardPendingRecording));

            driver.QueueNativeFunctionKeyForDiagnostics(NTSD28NativeFunctionKey.F11);
            Assert.That(
                driver.NativeFunctionKeyContinuousHostCommandForDiagnostics,
                Is.EqualTo(NTSD28NativeFunctionKeyHostCommand.VolumeDown));
        }

        [Test]
        public void LegacyDiagnosticQueue_DoesNotEnterNativeSessionCarrier()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            driver.SetPaused(true);

            driver.QueueBattleFunctionKeyCommandsForDiagnostics(
                BattleFunctionKeyCommand.InitializeStats |
                BattleFunctionKeyCommand.SpawnAllWeapons);
            Assert.That(driver.StepOneTick(true, false), Is.True);

            Assert.That(driver.World.Runtime.FunctionKeys.F7EventCount, Is.Zero);
            Assert.That(driver.World.Runtime.FunctionKeys.F8EventCount, Is.Zero);
            Assert.That(BattleTestBootstrap.NativeFunctionKeysOwnBattle(driver), Is.True,
                "The battle bootstrap must not reuse formal F6/F7 as movement debug keys.");
        }

        [Test]
        public void DedicatedWorkerTick_DispatchesNativeSessionByteExactlyOnce()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            SimulationWorld world = driver.World;
            driver.SetPaused(true);
            var characterData = new LF2CharacterData();
            characterData.frames.Add(new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 0,
                wait = 1,
                next = 0,
            });
            var wrapper = new LF2CharacterDataWrapper(31997, characterData);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        31997,
                        (int)LF2ObjectType.Other,
                        "function-key-worker.dat"),
                },
                id => id == 31997 ? wrapper : null);
            driver.SetFrameInputProvider(new EmptyFrameInputProvider());

            bool sealStarted = false;
            try
            {
                driver.BeginBattleAllocationSeal();
                sealStarted = true;
                Assert.That(
                    driver.DedicatedSimulationWorkerActiveForDiagnostics,
                    Is.True,
                    driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics);
                MethodInfo consumeMethod = typeof(SimulationTickDriver).GetMethod(
                    "ConsumeDedicatedSimulationWorkerPublication",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(consumeMethod, Is.Not.Null);

                driver.QueueNativeFunctionKeyForDiagnostics(
                    NTSD28NativeFunctionKey.F7);
                Assert.That(
                    driver.TryScheduleDedicatedSimulationWorkerTickForDiagnostics(false),
                    Is.True,
                    driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics);
                Assert.That(
                    SpinWait.SpinUntil(
                        () =>
                        {
                            consumeMethod.Invoke(driver, null);
                            return driver.CurrentTickIndex == 1 ||
                                   driver.DedicatedSimulationWorkerFailureForDiagnostics != null;
                        },
                        2000),
                    Is.True);
                Assert.That(driver.DedicatedSimulationWorkerFailureForDiagnostics, Is.Null);
                Assert.That(world.Runtime.FunctionKeys.F7EventCount, Is.EqualTo(1U));
                Assert.That(
                    world.Runtime.FunctionKeys.LastAcceptedEventByte,
                    Is.EqualTo(0x20));
                Assert.That(
                    SpinWait.SpinUntil(
                        () => !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics ||
                              driver.DedicatedSimulationWorkerFailureForDiagnostics != null,
                        2000),
                    Is.True);
                Assert.That(world.Runtime.FunctionKeys.F7EventCount, Is.EqualTo(1U));
            }
            finally
            {
                if (sealStarted)
                    driver.EndBattleAllocationSeal();
            }
        }

        private static ushort Mask(params NTSD28NativeFunctionKey[] keys)
        {
            ushort mask = 0;
            for (int index = 0; index < keys.Length; index++)
                mask |= NTSD28NativeFunctionKeyPhysicalLatch.KeyMask(keys[index]);
            return mask;
        }

        private sealed class EmptyFrameInputProvider : ISimulationFrameInputProvider
        {
            private readonly FrameInputSet frame =
                FrameInputSetPreallocation.CreateReusable();

            public bool IsFrameInputReady(int tickIndex) => true;

            public FrameInputSet GetFrameInput(int tickIndex)
            {
                FrameInputSetPreallocation.ResetPreallocated(frame, tickIndex, null);
                return frame;
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
                host = new GameObject("__NTSD28_FunctionKeyIntegrationTest")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                SetInstance(Driver);
                Driver.RecreateWorld();
                Driver.SetFrameInputProvider(null);
                Driver.SetPaused(false);
            }

            internal SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                if (Driver != null)
                {
                    BattleRuntimeShutdownReport report = Driver.ShutdownBattleRuntime();
                    if (report.Status != BattleRuntimeShutdownStatus.Failed)
                        Driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true);
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
