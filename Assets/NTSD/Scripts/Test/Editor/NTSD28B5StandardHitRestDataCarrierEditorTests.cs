#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5StandardHitRestDataCarrierEditorTests
    {
        [Test]
        public void InteractionRecover_ParsesAndCopiesAsTypedField()
        {
            FieldInfo field = typeof(InteractionArea).GetField("recover");
            Assert.That(field, Is.Not.Null);
            InteractionArea parsed = ParseFirstItr(@"
<frame> 0 test
  itr:
    kind: 0 recover: 3 injury: 1
  itr_end:
<frame_end>");
            Assert.That(field.GetValue(parsed), Is.EqualTo(3));

            var copy = new InteractionArea();
            copy.CopyFrom(parsed);
            Assert.That(field.GetValue(copy), Is.EqualTo(3));
            Assert.That(new InteractionArea().GetType().GetField("recover")
                .GetValue(new InteractionArea()), Is.Zero);

            Type projectionType = typeof(BattleEcsHitExecutionPlan)
                .GetNestedType("ItrProjection", BindingFlags.NonPublic);
            object projection = Activator.CreateInstance(
                projectionType,
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic,
                null,
                new object[] { parsed },
                null);
            Assert.That(projectionType.GetField(
                    "Recover",
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic).GetValue(projection),
                Is.EqualTo(3));

            MethodInfo fingerprint = typeof(BattleEcsHitExecutionPlan).GetMethod(
                "Fingerprint",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(InteractionArea) },
                null);
            ulong baseline = (ulong)fingerprint.Invoke(
                null,
                new object[] { new InteractionArea() });
            ulong changed = (ulong)fingerprint.Invoke(null, new object[] { parsed });
            Assert.That(changed, Is.Not.EqualTo(baseline));
        }

        [Test]
        public void DefinitionEffect_ReadsOnlyBmpProperty()
        {
            FieldInfo field = typeof(LF2CharacterData).GetField(
                "definition_effect");
            Assert.That(field, Is.Not.Null);
            var datFile = new Lf2DatFile { Bmp = new Lf2BmpSection() };
            datFile.Bmp.AddProperty(new Lf2DatProperty("effect", "4"));
            var data = new LF2CharacterData();

            Lf2DatConverter.ApplyNativeInputDefinitionData(datFile, data);

            Assert.That(field.GetValue(data), Is.EqualTo(4));
            Assert.That(field.GetValue(new LF2CharacterData()), Is.Zero);
        }

        [Test]
        public void WorldTimingReduction_ClampsResetsSnapshotsAndRestores()
        {
            FieldInfo rootField = typeof(BattleRuntimeState).GetField(
                "NativeStandardHitRest");
            Assert.That(rootField, Is.Not.Null);
            var world = new SimulationWorld();
            object state = rootField.GetValue(world.Runtime);
            Assert.That(state, Is.Not.Null);
            MethodInfo set = state.GetType().GetMethod(
                "SetTimingReduction4A9FF4");
            PropertyInfo value = state.GetType().GetProperty(
                "TimingReduction4A9FF4");
            Assert.That(set, Is.Not.Null);
            Assert.That(value, Is.Not.Null);

            set.Invoke(state, new object[] { 99 });
            Assert.That(value.GetValue(state), Is.EqualTo(5));
            set.Invoke(state, new object[] { -8 });
            Assert.That(value.GetValue(state), Is.Zero);
            set.Invoke(state, new object[] { 3 });
            var snapshot = new BattleWorldCoreScalarSnapshot(
                world,
                StrictDelayedInputBufferEditorTests.CreateIdentity());
            PropertyInfo snapshotDomain = typeof(BattleWorldCoreScalarSnapshot)
                .GetProperty("StandardHitRest");
            Assert.That(snapshotDomain, Is.Not.Null);
            object captured = snapshotDomain.GetValue(snapshot);
            Assert.That(
                captured.GetType().GetProperty("TimingReduction4A9FF4")
                    .GetValue(captured),
                Is.EqualTo(3));

            world.Runtime.Reset();
            Assert.That(value.GetValue(rootField.GetValue(world.Runtime)),
                Is.Zero);
        }

        [Test]
        public void TimingReduction_ChangesChecksumAndFullParity()
        {
            var world = new SimulationWorld();
            FieldInfo rootField = typeof(BattleRuntimeState).GetField(
                "NativeStandardHitRest");
            Assert.That(rootField, Is.Not.Null);
            object state = rootField.GetValue(world.Runtime);
            ulong before = world.CaptureRuntimeChecksum64(0, null);

            state.GetType().GetMethod("SetTimingReduction4A9FF4")
                .Invoke(state, new object[] { 4 });
            ulong after = world.CaptureRuntimeChecksum64(0, null);
            string parity = world.CaptureParityFrameSnapshot(0)
                .ToJson(full: true);

            Assert.That(after, Is.Not.EqualTo(before));
            Assert.That(parity,
                Does.Contain("\"timingReduction4A9FF4\":4"));
        }

        [Test]
        public void WorldSchemas_AdvanceForStandardHitRestDomain()
        {
            Assert.That(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion,
                Is.EqualTo(11));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(20));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(23));
        }

        private static InteractionArea ParseFirstItr(string text)
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text);
            return Lf2DatConverter.ConvertToFrameData(dat.Frames[0]).itrs[0];
        }
    }
}
#endif
