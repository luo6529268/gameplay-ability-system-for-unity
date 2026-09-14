#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeActionDataCarrierEditorTests
    {
        private static readonly byte[] IdentityRemap =
        {
            0, 1, 2, 3, 4, 5, 6,
        };

        [Test]
        public void RuntimeDefaults_InputOnlyResetPreservesCarriers_FullResetRestoresDefaults()
        {
            var runtime = new NTSDEntityRuntime();

            AssertCarrierDefaults(runtime);
            FillCarriers(runtime, 10);
            byte[] expectedRemap = (byte[])runtime.InputRemapIndices13C.Clone();

            runtime.ResetInputState();

            Assert.That(runtime.InputActionLock130, Is.EqualTo(11));
            Assert.That(runtime.InputLastAction144, Is.EqualTo(12));
            Assert.That(runtime.InputRemapState138, Is.EqualTo(13));
            Assert.That(runtime.InputRemapIndices13C, Is.EqualTo(expectedRemap));
            Assert.That(runtime.BoundState198, Is.EqualTo(14));
            Assert.That(runtime.InputGlobalRecordState20, Is.EqualTo(15));
            Assert.That(runtime.InputModeCostMultiplier30, Is.EqualTo(16));
            Assert.That(runtime.InputDoubleCost19C, Is.EqualTo(17));
            Assert.That(runtime.InputCostWaived1B4, Is.EqualTo(18));
            Assert.That(runtime.InputSpecialGate194, Is.EqualTo(19));
            Assert.That(runtime.InputModeFallbackActionB8, Is.EqualTo(20));
            Assert.That(runtime.InputLocalResourceEnabled49D034, Is.False);
            Assert.That(runtime.InputHpConsumedTotal34C, Is.EqualTo(21));
            Assert.That(runtime.InputMpConsumedTotal350, Is.EqualTo(22));
            Assert.That(runtime.FeatureGate4A8428, Is.True);
            Assert.That(runtime.InputLinkedDefinitionId324, Is.EqualTo(23));

            runtime.Reset();

            AssertCarrierDefaults(runtime);
        }

        [Test]
        public void CanonicalCopy_PreservesEveryCarrierAndDoesNotAliasRemap()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            FillCarriers(source, 30);

            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);

            AssertCarriersEqual(source, destination);
            Assert.That(destination.InputRemapIndices13C,
                Is.Not.SameAs(source.InputRemapIndices13C));
            byte captured = destination.InputRemapIndices13C[0];
            source.InputRemapIndices13C[0]++;
            Assert.That(destination.InputRemapIndices13C[0], Is.EqualTo(captured));
        }

        [Test]
        public void CanonicalCopy_InvalidRemapStorageFailsClosed()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            source.InputRemapIndices13C = null;

            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.False);

            source = new NTSDEntityRuntime();
            destination.InputRemapIndices13C = new byte[6];
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.False);
        }

        [Test]
        public void EntityRuntimeSnapshot_RoundTripsEveryCarrierWithoutAliasing()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            FillCarriers(entity.Runtime, 50);
            byte[] expectedRemap = (byte[])entity.Runtime.InputRemapIndices13C.Clone();
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                17), Is.True);
            entity.Runtime.InputRemapIndices13C[0] = byte.MaxValue;
            entity.Runtime.Reset();
            Assert.That(snapshot.TryCopyEntityRuntime(3, entity.Runtime), Is.True);

            Assert.That(entity.Runtime.InputActionLock130, Is.EqualTo(51));
            Assert.That(entity.Runtime.InputLinkedDefinitionId324, Is.EqualTo(63));
            Assert.That(entity.Runtime.InputRemapIndices13C, Is.EqualTo(expectedRemap));
            Assert.That(entity.Runtime.InputRemapIndices13C,
                Is.Not.SameAs(expectedRemap));
            Assert.That(snapshot.SchemaVersion, Is.EqualTo(13));
        }

        [Test]
        public void EntityRuntimeSnapshot_InvalidRemapStorageFailsClosed()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            entity.Runtime.InputRemapIndices13C = new byte[6];
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                17), Is.False);
            Assert.That(snapshot.SchemaVersion, Is.Zero);
            Assert.That(snapshot.EntityRuntimeCount, Is.Zero);
        }

        [Test]
        public void RuntimeChecksum_TracksEveryScalarAndEveryRemapByte()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            NTSDEntityRuntime runtime = entity.Runtime;
            ulong previous = world.CaptureRuntimeChecksum64(0, null);

            runtime.InputActionLock130 = 1;
            AssertChecksumChanged(world, ref previous);
            runtime.InputLastAction144 = 2;
            AssertChecksumChanged(world, ref previous);
            runtime.InputRemapState138 = 3;
            AssertChecksumChanged(world, ref previous);
            for (int index = 0; index < runtime.InputRemapIndices13C.Length; index++)
            {
                runtime.InputRemapIndices13C[index] =
                    (byte)(runtime.InputRemapIndices13C[index] + 10);
                AssertChecksumChanged(world, ref previous);
            }
            runtime.BoundState198 = 4;
            AssertChecksumChanged(world, ref previous);
            runtime.InputGlobalRecordState20 = 5;
            AssertChecksumChanged(world, ref previous);
            runtime.InputModeCostMultiplier30 = 6;
            AssertChecksumChanged(world, ref previous);
            runtime.InputDoubleCost19C = 7;
            AssertChecksumChanged(world, ref previous);
            runtime.InputCostWaived1B4 = 8;
            AssertChecksumChanged(world, ref previous);
            runtime.InputSpecialGate194 = 9;
            AssertChecksumChanged(world, ref previous);
            runtime.InputModeFallbackActionB8 = 10;
            AssertChecksumChanged(world, ref previous);
            runtime.InputLocalResourceEnabled49D034 = false;
            AssertChecksumChanged(world, ref previous);
            runtime.InputHpConsumedTotal34C = 11;
            AssertChecksumChanged(world, ref previous);
            runtime.InputMpConsumedTotal350 = 12;
            AssertChecksumChanged(world, ref previous);
            runtime.FeatureGate4A8428 = true;
            AssertChecksumChanged(world, ref previous);
            runtime.InputLinkedDefinitionId324 = 13;
            AssertChecksumChanged(world, ref previous);
        }

        [Test]
        public void WarmCanonicalCopyAndChecksum_AllocateZeroManagedBytes()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            FillCarriers(source, 70);
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            FillCarriers(entity.Runtime, 70);
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            _ = world.CaptureRuntimeChecksum64(0, null);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool copied = true;
            ulong checksum = 0UL;
            for (int index = 0; index < 4096; index++)
            {
                copied &= source.TryCopyCanonicalStateTo(destination);
                checksum ^= world.CaptureRuntimeChecksum64(index, null);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(copied, Is.True);
            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void FrameConverter_PreservesHpDirectActionAndHoldDirectionFields()
        {
            var frame = new Lf2FrameBlock { FrameIndex = 27, FrameName = "action" };
            AddProperties(
                frame,
                ("hp", "101"),
                ("hit_f", "102"),
                ("hit_b", "103"),
                ("hit_uz", "104"),
                ("hit_dz", "105"),
                ("hold_a", "106"),
                ("hold_d", "107"),
                ("hold_j", "108"),
                ("hold_f", "109"),
                ("hold_b", "110"),
                ("hold_uz", "111"),
                ("hold_dz", "112"));

            LF2FrameData actual = Lf2DatConverter.ConvertToFrameData(frame);

            Assert.That(actual.hp, Is.EqualTo(101));
            Assert.That(actual.hit_f, Is.EqualTo(102));
            Assert.That(actual.hit_b, Is.EqualTo(103));
            Assert.That(actual.hit_uz, Is.EqualTo(104));
            Assert.That(actual.hit_dz, Is.EqualTo(105));
            Assert.That(actual.hold_a, Is.EqualTo(106));
            Assert.That(actual.hold_d, Is.EqualTo(107));
            Assert.That(actual.hold_j, Is.EqualTo(108));
            Assert.That(actual.hold_f, Is.EqualTo(109));
            Assert.That(actual.hold_b, Is.EqualTo(110));
            Assert.That(actual.hold_uz, Is.EqualTo(111));
            Assert.That(actual.hold_dz, Is.EqualTo(112));
        }

        [Test]
        public void DefinitionConverter_PreservesOnlyDefinitionLevelNativeInputFields()
        {
            var datFile = new Lf2DatFile { Bmp = new Lf2BmpSection() };
            datFile.Bmp.AddProperty(new Lf2DatProperty("use_ai", "7"));
            datFile.AddProperty(new Lf2DatProperty("recmp", "999"));
            datFile.AddProperty(new Lf2DatProperty("caughtact", "998"));
            var unrelated = new Lf2DatBlock { Name = "itr" };
            unrelated.AddProperty(new Lf2DatProperty("caughtact", "997"));
            datFile.Blocks.Add(unrelated);
            var stats = new Lf2DatBlock { Name = "stats" };
            stats.AddProperty(new Lf2DatProperty("recmp", "71"));
            stats.AddProperty(new Lf2DatProperty("caughtact", "72"));
            datFile.Blocks.Add(stats);
            var data = new LF2CharacterData();

            Lf2DatConverter.ApplyNativeInputDefinitionData(datFile, data);

            Assert.That(data.use_ai, Is.EqualTo(7));
            Assert.That(data.recmp, Is.EqualTo(71));
            Assert.That(data.caughtact, Is.EqualTo(72));
        }

        [Test]
        public void SnapshotAndChecksumSchemas_AreNativeActionCarrierVersions()
        {
            Assert.That(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(13));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(21));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(24));
        }

        private static void FillCarriers(NTSDEntityRuntime runtime, int seed)
        {
            runtime.InputActionLock130 = seed + 1;
            runtime.InputLastAction144 = seed + 2;
            runtime.InputRemapState138 = seed + 3;
            for (int index = 0; index < runtime.InputRemapIndices13C.Length; index++)
                runtime.InputRemapIndices13C[index] = (byte)(seed + index + 1);
            runtime.BoundState198 = seed + 4;
            runtime.InputGlobalRecordState20 = seed + 5;
            runtime.InputModeCostMultiplier30 = seed + 6;
            runtime.InputDoubleCost19C = seed + 7;
            runtime.InputCostWaived1B4 = seed + 8;
            runtime.InputSpecialGate194 = seed + 9;
            runtime.InputModeFallbackActionB8 = seed + 10;
            runtime.InputLocalResourceEnabled49D034 = false;
            runtime.InputHpConsumedTotal34C = seed + 11;
            runtime.InputMpConsumedTotal350 = seed + 12;
            runtime.FeatureGate4A8428 = true;
            runtime.InputLinkedDefinitionId324 = seed + 13;
        }

        private static void AssertCarrierDefaults(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.InputActionLock130, Is.Zero);
            Assert.That(runtime.InputLastAction144, Is.Zero);
            Assert.That(runtime.InputRemapState138, Is.Zero);
            Assert.That(runtime.InputRemapIndices13C, Is.EqualTo(IdentityRemap));
            Assert.That(runtime.BoundState198, Is.Zero);
            Assert.That(runtime.InputGlobalRecordState20, Is.Zero);
            Assert.That(runtime.InputModeCostMultiplier30, Is.Zero);
            Assert.That(runtime.InputDoubleCost19C, Is.Zero);
            Assert.That(runtime.InputCostWaived1B4, Is.Zero);
            Assert.That(runtime.InputSpecialGate194, Is.Zero);
            Assert.That(runtime.InputModeFallbackActionB8, Is.Zero);
            Assert.That(runtime.InputLocalResourceEnabled49D034, Is.True);
            Assert.That(runtime.InputHpConsumedTotal34C, Is.Zero);
            Assert.That(runtime.InputMpConsumedTotal350, Is.Zero);
            Assert.That(runtime.FeatureGate4A8428, Is.False);
            Assert.That(runtime.InputLinkedDefinitionId324, Is.EqualTo(-1));
        }

        private static void AssertCarriersEqual(
            NTSDEntityRuntime expected,
            NTSDEntityRuntime actual)
        {
            Assert.That(actual.InputActionLock130,
                Is.EqualTo(expected.InputActionLock130));
            Assert.That(actual.InputLastAction144,
                Is.EqualTo(expected.InputLastAction144));
            Assert.That(actual.InputRemapState138,
                Is.EqualTo(expected.InputRemapState138));
            Assert.That(actual.InputRemapIndices13C,
                Is.EqualTo(expected.InputRemapIndices13C));
            Assert.That(actual.BoundState198,
                Is.EqualTo(expected.BoundState198));
            Assert.That(actual.InputGlobalRecordState20,
                Is.EqualTo(expected.InputGlobalRecordState20));
            Assert.That(actual.InputModeCostMultiplier30,
                Is.EqualTo(expected.InputModeCostMultiplier30));
            Assert.That(actual.InputDoubleCost19C,
                Is.EqualTo(expected.InputDoubleCost19C));
            Assert.That(actual.InputCostWaived1B4,
                Is.EqualTo(expected.InputCostWaived1B4));
            Assert.That(actual.InputSpecialGate194,
                Is.EqualTo(expected.InputSpecialGate194));
            Assert.That(actual.InputModeFallbackActionB8,
                Is.EqualTo(expected.InputModeFallbackActionB8));
            Assert.That(actual.InputLocalResourceEnabled49D034,
                Is.EqualTo(expected.InputLocalResourceEnabled49D034));
            Assert.That(actual.InputHpConsumedTotal34C,
                Is.EqualTo(expected.InputHpConsumedTotal34C));
            Assert.That(actual.InputMpConsumedTotal350,
                Is.EqualTo(expected.InputMpConsumedTotal350));
            Assert.That(actual.FeatureGate4A8428,
                Is.EqualTo(expected.FeatureGate4A8428));
            Assert.That(actual.InputLinkedDefinitionId324,
                Is.EqualTo(expected.InputLinkedDefinitionId324));
        }

        private static void AssertChecksumChanged(
            SimulationWorld world,
            ref ulong previous)
        {
            ulong current = world.CaptureRuntimeChecksum64(0, null);
            Assert.That(current, Is.Not.EqualTo(previous));
            previous = current;
        }

        private static void AddProperties(
            Lf2FrameBlock frame,
            params (string key, string value)[] properties)
        {
            for (int index = 0; index < properties.Length; index++)
            {
                frame.AddProperty(new Lf2DatProperty(
                    properties[index].key,
                    properties[index].value));
            }
        }
    }
}
#endif
