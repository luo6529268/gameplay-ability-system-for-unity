#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleWorldStageSpawnSnapshotEditorTests
    {
        [Test]
        public void BootstrapCapacityAndCaptureOwnStageRuntimeValues()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            ConfigureCampaign(scope.Driver.World, 2);
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldStageSpawnSnapshotBuffer destination =
                session.CreateStageSpawnSnapshotBufferForBootstrap();
            BattleRuntimeState runtime = scope.Driver.World.Runtime;
            PopulateRuntime(runtime, 2);

            Assert.That(destination, Is.Not.Null);
            Assert.That(destination.EntryCapacity, Is.EqualTo(2));
            Assert.That(
                session.TryCaptureWorldStageSpawnSnapshot(destination),
                Is.True);

            runtime.StageSpawnRuntimeWave = 99;
            runtime.StageSpawnRuntimeTargetTotal[1] = 0;
            runtime.StageSpawnRuntimeSlots[1][39] = -1;

            Assert.That(destination.SchemaVersion,
                Is.EqualTo(BattleWorldStageSpawnSnapshotBuffer.CurrentSchemaVersion));
            Assert.That(destination.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(destination.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(destination.CapturedTick, Is.EqualTo(0));
            Assert.That(destination.RuntimeWave, Is.EqualTo(4));
            Assert.That(destination.ActiveEntryCount, Is.EqualTo(2));
            Assert.That(destination.GetTargetTotal(1), Is.EqualTo(101));
            Assert.That(destination.GetEntryCount(1), Is.EqualTo(11));
            Assert.That(destination.GetSpawnedTotal(1), Is.EqualTo(21));
            Assert.That(destination.GetRuntimeSlot(1, 39), Is.EqualTo(1039));
        }

        [Test]
        public void CapacityOverflowFailsWithoutPartiallyOverwritingDestination()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            ConfigureCampaign(scope.Driver.World, 1);
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldStageSpawnSnapshotBuffer destination =
                session.CreateStageSpawnSnapshotBufferForBootstrap();
            BattleRuntimeState runtime = scope.Driver.World.Runtime;
            PopulateRuntime(runtime, 1);
            Assert.That(
                session.TryCaptureWorldStageSpawnSnapshot(destination),
                Is.True);

            runtime.StageSpawnRuntimeWave = 8;
            AddRuntimeEntry(runtime, 1);

            Assert.That(
                session.TryCaptureWorldStageSpawnSnapshot(destination),
                Is.False);
            Assert.That(destination.RuntimeWave, Is.EqualTo(4));
            Assert.That(destination.ActiveEntryCount, Is.EqualTo(1));
            Assert.That(destination.GetTargetTotal(0), Is.EqualTo(100));
        }

        [Test]
        public void WarmStageSpawnCaptureDoesNotAllocate()
        {
            using var scope = new DriverScope();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            ConfigureCampaign(scope.Driver.World, 3);
            var session = new BattleLockstepSession(scope.Driver, identity, 0, 8, 8);
            BattleWorldStageSpawnSnapshotBuffer destination =
                session.CreateStageSpawnSnapshotBufferForBootstrap();
            PopulateRuntime(scope.Driver.World.Runtime, 3);
            Assert.That(
                session.TryCaptureWorldStageSpawnSnapshot(destination),
                Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                if (!session.TryCaptureWorldStageSpawnSnapshot(destination))
                {
                    Assert.Fail($"Stage-spawn capture failed at {index}.");
                }
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static void ConfigureCampaign(SimulationWorld world, int spawnCount)
        {
            var phase = new BattleStagePhaseData();
            for (int index = 0; index < spawnCount; index++)
            {
                phase.Spawns.Add(new BattleStageSpawnData
                {
                    Id = index + 1,
                });
            }

            var campaign = new BattleStageCampaignData
            {
                Id = 7,
            };
            campaign.Phases.Add(phase);
            world.ConfigureStageCampaigns(
                new List<BattleStageCampaignData> { campaign },
                7,
                0);
        }

        private static void PopulateRuntime(BattleRuntimeState runtime, int count)
        {
            runtime.StageSpawnRuntimeWave = 4;
            runtime.StageSpawnRuntimeTargetTotal.Clear();
            runtime.StageSpawnRuntimeEntryCount.Clear();
            runtime.StageSpawnRuntimeSpawnedTotal.Clear();
            runtime.StageSpawnRuntimeSlots.Clear();
            for (int index = 0; index < count; index++)
            {
                AddRuntimeEntry(runtime, index);
            }
        }

        private static void AddRuntimeEntry(BattleRuntimeState runtime, int index)
        {
            runtime.StageSpawnRuntimeTargetTotal.Add(100 + index);
            runtime.StageSpawnRuntimeEntryCount.Add(10 + index);
            runtime.StageSpawnRuntimeSpawnedTotal.Add(20 + index);
            var slots = new int[StageSpawnRuntimeBufferPool.SlotsPerSpawnEntry];
            for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
            {
                slots[slotIndex] = index * 1000 + slotIndex;
            }
            runtime.StageSpawnRuntimeSlots.Add(slots);
        }

        private sealed class DriverScope : IDisposable
        {
            private readonly FieldInfo instanceField;
            private readonly SimulationTickDriver previous;
            private readonly GameObject host;

            public DriverScope()
            {
                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
                instanceField = typeof(SimulationTickDriver).BaseType.GetField(
                    "<Instance>k__BackingField",
                    flags);
                Assert.That(instanceField, Is.Not.Null);
                previous = instanceField.GetValue(null) as SimulationTickDriver;
                instanceField.SetValue(null, null);
                host = new GameObject("BattleWorldStageSpawnSnapshotTests")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                Driver.RecreateWorld();
                Driver.SetPaused(true);
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(host);
                instanceField.SetValue(null, previous);
            }
        }
    }
}
#endif
