#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q06FusionPersistentCarriersEditorTests
    {
        private static FieldInfo Field(object target, string name)
        {
            var field = target.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, "Missing persistent carrier: " + name);
            return field;
        }

        [TestCase("NativeAiProfileObjectId", -1, -7)]
        [TestCase("NativeDefinitionDropMode", 0, 3)]
        [TestCase("FusionDisplayTimer190", 0, 4500)]
        public void EntityCarrierCopiesAndResetsIndependently(string name, int initial, int changed)
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            var field = Field(source, name);
            Assert.That(field.GetValue(source), Is.EqualTo(initial));
            field.SetValue(source, changed);
            source.Unk338 = 12;
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(field.GetValue(destination), Is.EqualTo(changed));
            source.Reset();
            Assert.That(field.GetValue(source), Is.EqualTo(initial));
            Assert.That(field.GetValue(destination), Is.EqualTo(changed));
            Assert.That(destination.Unk338, Is.EqualTo(12));
        }

        [TestCase("FusionFirstFeatureGate4A8428")]
        [TestCase("FusionSecondFeatureGate4A842C")]
        public void GlobalFeatureDefaultsAndResetsWithoutRewritingEntityProjection(string name)
        {
            var runtime = new BattleRuntimeState();
            var entity = new NTSDEntityRuntime { FeatureGate4A8428 = true };
            var field = Field(runtime, name);
            Assert.That(field.GetValue(runtime), Is.False);
            field.SetValue(runtime, true);
            runtime.Reset();
            Assert.That(field.GetValue(runtime), Is.False);
            Assert.That(entity.FeatureGate4A8428, Is.True);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void AggregateRestorePreservesGlobalPairAndIndependentRawCarriers(bool first, bool second)
        {
            var world = new SimulationWorld();
            var identity = NTSD.Test.StrictDelayedInputBufferEditorTests.CreateIdentity();
            var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
            world.Runtime.FusionFirstFeatureGate4A8428 = first;
            world.Runtime.FusionSecondFeatureGate4A842C = second;
            var raw = world.GetRawRuntimeSlotState(3);
            raw.NativeAiProfileObjectId = -7;
            raw.NativeDefinitionDropMode = 3;
            raw.FusionDisplayTimer190 = 4500;
            raw.Unk338 = 12;
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            ulong expected = world.CaptureRuntimeChecksum64(0, input);
            world.Runtime.FusionFirstFeatureGate4A8428 = !first;
            world.Runtime.FusionSecondFeatureGate4A842C = !second;
            raw.NativeAiProfileObjectId = 6;
            raw.NativeDefinitionDropMode = 0;
            raw.FusionDisplayTimer190 = 0;
            Assert.That(world.CaptureRuntimeChecksum64(0, input), Is.Not.EqualTo(expected));
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
            Assert.That(world.Runtime.FusionFirstFeatureGate4A8428, Is.EqualTo(first));
            Assert.That(world.Runtime.FusionSecondFeatureGate4A842C, Is.EqualTo(second));
            Assert.That(raw.NativeAiProfileObjectId, Is.EqualTo(-7));
            Assert.That(raw.NativeDefinitionDropMode, Is.EqualTo(3));
            Assert.That(raw.FusionDisplayTimer190, Is.EqualTo(4500));
            Assert.That(raw.Unk338, Is.EqualTo(12));
            Assert.That(world.CaptureRuntimeChecksum64(0, input), Is.EqualTo(expected));
        }

        [TestCase("NativeAiProfileObjectId")]
        [TestCase("NativeDefinitionDropMode")]
        [TestCase("FusionDisplayTimer190")]
        [TestCase("FusionFirstFeatureGate4A8428")]
        [TestCase("FusionSecondFeatureGate4A842C")]
        public void EachNewCarrierIndependentlyChangesChecksum(string name)
        {
            var world = new SimulationWorld();
            var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
            object target = name.Contains("FeatureGate") ? (object)world.Runtime : world.GetRawRuntimeSlotState(3);
            ulong before = world.CaptureRuntimeChecksum64(0, input);
            Field(target, name).SetValue(target, name.Contains("FeatureGate") ? (object)true : 17);
            Assert.That(world.CaptureRuntimeChecksum64(0, input), Is.Not.EqualTo(before));
        }
    }
}
#endif
