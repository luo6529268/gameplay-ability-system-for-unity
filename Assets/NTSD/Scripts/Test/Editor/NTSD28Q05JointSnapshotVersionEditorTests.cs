#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q05JointSnapshotVersionEditorTests
    {
        [TestCase("RosterResults")]
        [TestCase("StageSpawn")]
        [TestCase("RuntimeSlots")]
        [TestCase("EntityRuntime")]
        [TestCase("EntityBaseShell")]
        [TestCase("LivingShell")]
        [TestCase("CharacterShell")]
        [TestCase("WeaponShell")]
        [TestCase("SpecialOtherShell")]
        [TestCase("PendingEvents")]
        [TestCase("Rest")]
        public void EveryNestedPayloadHeaderMustStillMatchAtRestore(string domain)
        {
            var world = new SimulationWorld();
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            object payload = typeof(BattleStateSnapshotBuffer).GetProperty(domain).GetValue(snapshot);
            foreach (string name in new[] { "SchemaVersion", "ProtocolSchemaVersion", "IdentityFingerprint", "CapturedTick" })
            {
                PropertyInfo header = payload.GetType().GetProperty(name);
                object saved = header.GetValue(payload);
                object different = saved is ulong ? (object)((ulong)saved ^ 1UL) : (object)((int)saved + 1000);
                header.SetValue(payload, different);
                string before = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.False, domain + "." + name);
                Assert.That(failure, Is.EqualTo(BattleStateSnapshotRestoreFailure.InvalidSnapshot));
                Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(before));
                Assert.That(header.GetValue(payload), Is.EqualTo(different), "Rejected source must remain intact.");
                header.SetValue(payload, saved);
                Assert.That(snapshot.IsValid, Is.True);
            }
        }

        [TestCase(23)]
        [TestCase(24)]
        [TestCase(25)]
        public void OldChecksumHistoryIsRejectedBeforeReplayRestoresWorld(int oldVersion)
        {
            FieldInfo singleton = typeof(SimulationTickDriver).BaseType.GetField(
                "<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
            object previous = singleton.GetValue(null);
            singleton.SetValue(null, null);
            var host = new GameObject("Q05JointVersion") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var driver = host.AddComponent<SimulationTickDriver>();
                driver.RecreateWorld();
                driver.SetPaused(true);
                driver.ApplySettings(new LockstepSimulationSettings
                {
                    driveMode = SimulationDriveMode.Manual,
                    enableFrameChecksum = true,
                });
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var session = new BattleLockstepSession(driver, identity, 0, 8, 8);
                var snapshot = session.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(session.TryCaptureBattleStateSnapshot(snapshot), Is.True);
                var frame = new FrameInputSet(1, new[]
                {
                    new SimulationPlayerInput(2, SimulationInputButtons.None),
                    new SimulationPlayerInput(5, SimulationInputButtons.None),
                });
                Assert.That(session.TryAdvanceManual(frame, buildPresentation: false), Is.True);
                var versions = (int[])typeof(LockstepChecksumHistoryRing).GetField(
                    "checksumSchemaVersions", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(session.ChecksumHistory);
                versions[0] = oldVersion;
                ulong before = driver.World.CaptureRuntimeChecksum64(1, frame);
                Assert.That(session.TryRestoreAndReplay(snapshot), Is.False);
                Assert.That(session.LastReason, Is.EqualTo(LockstepProtocolReason.ReplayHistoryUnavailable));
                Assert.That(driver.CurrentTickIndex, Is.EqualTo(1));
                Assert.That(driver.World.CaptureRuntimeChecksum64(1, frame), Is.EqualTo(before));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                singleton.SetValue(null, previous);
            }
        }

        [TestCase(typeof(BattleWorldEntityRuntimeSnapshotBuffer), 15)]
        [TestCase(typeof(BattleStateSnapshotBuffer), 23)]
        [TestCase(typeof(BattleLockstepChecksumModule), 26)]
        [TestCase(typeof(BattleWorldCharacterShellSnapshotBuffer), 2)]
        [TestCase(typeof(BattleWorldEntityBaseShellSnapshotBuffer), 2)]
        public void JointVersionSetIsExplicit(Type type, int expected)
        {
            FieldInfo field = type.GetField("CurrentSchemaVersion",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetRawConstantValue(), Is.EqualTo(expected));
        }

        [TestCase("Aggregate", 20)]
        [TestCase("EntityRuntime", 12)]
        [TestCase("CharacterShell", 1)]
        [TestCase("EntityBaseShell", 1)]
        public void OldPayloadVersionIsRejectedBeforeWorldMutation(string domain, int oldVersion)
        {
            var world = new SimulationWorld();
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(3);
            world.Register(character);
            try
            {
                character.Runtime.HP = 217;
                character.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 19;
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                object payload = domain == "Aggregate" ? snapshot :
                    typeof(BattleStateSnapshotBuffer).GetProperty(domain).GetValue(snapshot);
                PropertyInfo version = payload.GetType().GetProperty("SchemaVersion");
                Assert.That(version, Is.Not.Null);
                version.SetValue(payload, oldVersion);
                character.Runtime.HP = 81;
                character.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 23;
                string before = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.False);
                Assert.That(failure, Is.Not.EqualTo(BattleStateSnapshotRestoreFailure.None));
                Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(before));
                Assert.That(character.Runtime.HP, Is.EqualTo(81));
                Assert.That(character.Runtime.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(23));
            }
            finally
            {
                character.UnregisterFromWorld();
                character.Reset();
            }
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void CurrentVersionRestoresClaimedAndRawStateWithSameChecksum(BattleRuntimeProfile profile)
        {
            var world = profile == BattleRuntimeProfile.Authority400 ? new SimulationWorld() :
                new SimulationWorld(profile, BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(3);
            world.Register(character);
            try
            {
                int rawSlot = world.RuntimeSlotCapacity - 1;
                var raw = world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot);
                character.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = 17;
                character.Runtime.SpawnerSlotIndex = 31;
                raw.ObjectAiExcludedGroupSourceSlot2F8 = 27;
                raw.InputHistory[4] = 53;
                world.Rng.Seed(424242);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                Assert.That(snapshot.SchemaVersion, Is.EqualTo(23));
                Assert.That(snapshot.EntityRuntime.SchemaVersion, Is.EqualTo(15));
                string expected = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
                ulong expectedFast = world.CaptureRuntimeChecksum64(0, FrameInputSet.Empty(0));
                character.Runtime.ObjectAiExcludedGroupSourceSlot2F8 = -1;
                character.Runtime.SpawnerSlotIndex = 9;
                raw.ObjectAiExcludedGroupSourceSlot2F8 = -1;
                raw.InputHistory[4] = 0;
                world.Rng.NextInt(0, 97);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                Assert.That(character.Runtime.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(17));
                Assert.That(character.Runtime.SpawnerSlotIndex, Is.EqualTo(31));
                raw = world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot);
                Assert.That(raw.ObjectAiExcludedGroupSourceSlot2F8, Is.EqualTo(27));
                Assert.That(raw.InputHistory[4], Is.EqualTo(53));
                Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(expected));
                Assert.That(world.CaptureRuntimeChecksum64(0, FrameInputSet.Empty(0)), Is.EqualTo(expectedFast));
                Assert.That(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion, Is.EqualTo(11));
            }
            finally
            {
                character.UnregisterFromWorld();
                character.Reset();
            }
        }
    }
}
#endif
