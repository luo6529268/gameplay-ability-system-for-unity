#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Tools;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5F6ResourceGateProjectionEditorTests
    {
        [Test]
        public void AcceptedF6_ProjectsBothDirectionsAtDispatchBoundary()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            SimulationWorld world = driver.World;
            LF2Character first = RegisterCharacter(world, 3, 7);
            LF2Character second = RegisterCharacter(world, 4, 8);
            second.Runtime.InputLocalResourceEnabled49D034 = false;

            driver.QueueNativeFunctionKeyForDiagnostics(
                NTSD28NativeFunctionKey.F6);
            InvokeFunctionKeyDispatch(driver);

            Assert.That(world.Runtime.FunctionKeys.HitResourceEnabled, Is.False);
            Assert.That(first.Runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(second.Runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(
                world.GetRawRuntimeSlotState(3)
                    .InputLocalResourceEnabled49D034,
                Is.False);
            Assert.That(
                world.GetRawRuntimeSlotState(4)
                    .InputLocalResourceEnabled49D034,
                Is.False);

            driver.QueueNativeFunctionKeyForDiagnostics(
                NTSD28NativeFunctionKey.F6);
            InvokeFunctionKeyDispatch(driver);

            Assert.That(world.Runtime.FunctionKeys.HitResourceEnabled, Is.True);
            Assert.That(first.Runtime.InputLocalResourceEnabled49D034, Is.True);
            Assert.That(second.Runtime.InputLocalResourceEnabled49D034, Is.True);
        }

        [Test]
        public void RejectedLockedF6_DoesNotProjectUnchangedGate()
        {
            using var scope = new DriverScope();
            SimulationTickDriver driver = scope.Driver;
            SimulationWorld world = driver.World;
            LF2Character entity = RegisterCharacter(world, 3, 7);
            entity.Runtime.InputLocalResourceEnabled49D034 = false;
            world.GetRawRuntimeSlotState(3)
                .InputLocalResourceEnabled49D034 = false;

            driver.QueueNativeFunctionKeyForDiagnostics(
                NTSD28NativeFunctionKey.F3);
            InvokeFunctionKeyDispatch(driver);
            Assert.That(world.Runtime.FunctionKeys.LockState, Is.EqualTo(2));

            driver.QueueNativeFunctionKeyForDiagnostics(
                NTSD28NativeFunctionKey.F6);
            InvokeFunctionKeyDispatch(driver);

            Assert.That(world.Runtime.FunctionKeys.HitResourceEnabled, Is.True);
            Assert.That(world.Runtime.FunctionKeys.F6EventCount, Is.Zero);
            Assert.That(entity.Runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(
                world.GetRawRuntimeSlotState(3)
                    .InputLocalResourceEnabled49D034,
                Is.False);
        }

        [Test]
        public void SuccessfulRegistration_InheritsCurrentWorldGateInBothMirrors()
        {
            var world = new SimulationWorld();
            world.Runtime.FunctionKeys.RestoreForSnapshot(
                0,
                false,
                1,
                0,
                0,
                0,
                false,
                NTSD28NativeFunctionKeyPendingObjectCommand.None,
                0,
                0x10);
            LF2Character entity = RegisterCharacter(world, 3, 7);

            Assert.That(entity.Runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(
                world.GetRawRuntimeSlotState(3)
                    .InputLocalResourceEnabled49D034,
                Is.False);
        }

        [Test]
        public void Projection_ExcludesUnregisteredEntity()
        {
            var world = new SimulationWorld();
            LF2Character entity = RegisterCharacter(world, 3, 7);
            world.Unregister(entity);
            entity.Runtime.InputLocalResourceEnabled49D034 = true;

            world.ProjectNativeHitResourceGateToActiveEntities(false);

            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(-1));
            Assert.That(entity.Runtime.InputLocalResourceEnabled49D034, Is.True);
        }

        [Test]
        public void WarmProjection_DoesNotAllocate()
        {
            var world = new SimulationWorld();
            LF2Character first = RegisterCharacter(world, 3, 7);
            LF2Character second = RegisterCharacter(world, 4, 8);
            world.ProjectNativeHitResourceGateToActiveEntities(false);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                world.ProjectNativeHitResourceGateToActiveEntities(
                    (index & 1) == 0);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(first.Runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(second.Runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(allocated, Is.Zero);
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int runtimeSlot,
            int objectId)
        {
            var entity = new LF2Character { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(runtimeSlot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(runtimeSlot));
            return entity;
        }

        private static void InvokeFunctionKeyDispatch(
            SimulationTickDriver driver)
        {
            MethodInfo method = typeof(SimulationTickDriver).GetMethod(
                "ApplyPendingBattleFunctionKeyCommandsForTick",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(driver, null);
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
                host = new GameObject("__NTSD28_F6ResourceGateProjectionTest")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                SetInstance(Driver);
                Driver.RecreateWorld();
                Driver.SetFrameInputProvider(null);
                Driver.SetPaused(false);
                Driver.SetPaused(true);
            }

            internal SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                if (Driver != null)
                {
                    BattleRuntimeShutdownReport report =
                        Driver.ShutdownBattleRuntime();
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
