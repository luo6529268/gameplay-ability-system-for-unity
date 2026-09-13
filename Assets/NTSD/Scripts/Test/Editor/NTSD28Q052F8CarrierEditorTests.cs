#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q052F8CarrierEditorTests
    {
        private const string FieldName = "ObjectAiExcludedGroupSourceSlot2F8";
        private static FieldInfo Carrier()
        {
            var field = typeof(NTSDEntityRuntime).GetField(FieldName);
            Assert.That(field, Is.Not.Null, "Native +2F8 needs an independent carrier.");
            Assert.That(field.FieldType, Is.EqualTo(typeof(int)));
            return field;
        }
        private static void Set(NTSDEntityRuntime runtime, int value) => Carrier().SetValue(runtime, value);
        private static int Get(NTSDEntityRuntime runtime) => (int)Carrier().GetValue(runtime);

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(399)]
        [TestCase(767)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void SignedSlotCopiesAndResetsWithoutAliasingOwners(int slot)
        {
            var source = new NTSDEntityRuntime { SpawnerSlotIndex = 19, OwnerSlotIndex = 20, OwnerStableId = 21, RelationOwnerSlotIndex = 22 };
            var destination = new NTSDEntityRuntime();
            Assert.That(Get(source), Is.EqualTo(-1));
            Set(source, slot);
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(Get(destination), Is.EqualTo(slot));
            Assert.That(new[] { destination.SpawnerSlotIndex, destination.OwnerSlotIndex, destination.OwnerStableId, destination.RelationOwnerSlotIndex }, Is.EqualTo(new[] { 19, 20, 21, 22 }));
            source.Reset();
            Assert.That(Get(source), Is.EqualTo(-1));
            Assert.That(Get(destination), Is.EqualTo(slot));
            destination.Reset();
            Assert.That(Get(destination), Is.EqualTo(-1));
        }

        [Test]
        public void EcsCapturesValidatesFingerprintsAndClearsIndependentSlot()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            var ecs = new BattleEcsWorld(new BattleEcsCapacityProfile(BattleRuntimeProfile.Authority400, world.RuntimeSlotCapacity));
            FieldInfo field = ecs.Identity.GetType().GetField(FieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            var slots = (int[])field.GetValue(ecs.Identity);
            Assert.That(slots.All(value => value == -1), Is.True);
            Set(entity.Runtime, 7);
            ecs.CaptureSlot(3, true, 1, entity);
            Assert.That(slots[3], Is.EqualTo(7));
            Assert.That(ecs.MatchesCanonicalSlot(3, true, 1, entity, out _), Is.True);
            var fingerprint = BattleRuntimeFingerprint.Compute(entity.Runtime);
            Set(entity.Runtime, 8);
            Assert.That(ecs.MatchesCanonicalSlot(3, true, 1, entity, out BattleEcsShadowMismatchKind mismatch), Is.False);
            Assert.That(mismatch, Is.EqualTo(BattleEcsShadowMismatchKind.Identity));
            Assert.That(BattleRuntimeFingerprint.Compute(entity.Runtime), Is.Not.EqualTo(fingerprint));
            ecs.CaptureSlot(3, true, 1, entity);
            Assert.That(slots[3], Is.EqualTo(8));
            ecs.CaptureSlot(3, false, 2, null);
            Assert.That(slots[3], Is.EqualTo(-1));
            entity.Runtime.Reset();
            ecs.CaptureSlot(3, true, 3, entity);
            Assert.That(slots[3], Is.EqualTo(-1));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ClaimedAndRawSlotsCaptureAndRestoreIndependently(BattleRuntimeProfile profile)
        {
            var world = CreateWorld(profile);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            int rawSlot = world.RuntimeSlotCapacity - 1;
            var raw = world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot);
            Set(entity.Runtime, 9);
            Set(raw, 37);
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            Set(entity.Runtime, 90);
            Set(raw, 370);
            var copiedEntity = new NTSDEntityRuntime();
            var copiedRaw = new NTSDEntityRuntime();
            Assert.That(snapshot.EntityRuntime.TryCopyEntityRuntime(3, copiedEntity), Is.True);
            Assert.That(snapshot.EntityRuntime.TryCopyRawRuntime(rawSlot, copiedRaw), Is.True);
            Assert.That(Get(copiedEntity), Is.EqualTo(9));
            Assert.That(Get(copiedRaw), Is.EqualTo(37));
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out BattleStateSnapshotRestoreFailure failure), Is.True, failure.ToString());
            Assert.That(Get(entity.Runtime), Is.EqualTo(9));
            Assert.That(Get(world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot)), Is.EqualTo(37));
            Assert.That(entity.Runtime.SpawnerSlotIndex, Is.EqualTo(-1));
            Assert.That(entity.Runtime.OwnerSlotIndex, Is.EqualTo(-1));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ClaimedAndRawValuesAffectChecksumAndDiagnosticProjection(BattleRuntimeProfile profile)
        {
            var world = CreateWorld(profile);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            var initial = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
            Set(entity.Runtime, 17);
            var claimed = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
            Assert.That(claimed, Is.Not.EqualTo(initial));
            Set(world.RuntimeSlotTableForModules.GetRawRuntime(world.RuntimeSlotCapacity - 1), 29);
            Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.Not.EqualTo(claimed));
            string json = profile == BattleRuntimeProfile.Authority400
                ? world.CaptureParityFrameSnapshot(0).ToJson()
                : world.CaptureExtendedChecksumSnapshot(0).ToJson();
            int[] slots = JToken.Parse(json).SelectTokens("$..objectAiExcludedGroupSourceSlot2F8").Values<int>().ToArray();
            Assert.That(slots, Does.Contain(17));
            // The extended diagnostic exposes raw payloads; Authority400 uses its existing entity projection.
            if (profile == BattleRuntimeProfile.MobileExtended)
                Assert.That(slots, Does.Contain(29));
        }

        private static SimulationWorld CreateWorld(BattleRuntimeProfile profile)
        {
            return profile == BattleRuntimeProfile.Authority400 ? new SimulationWorld()
                : new SimulationWorld(profile, BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
        }
    }
}
#endif
