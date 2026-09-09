#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Linq;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeType0BuiltinDataSeamsEditorTests
    {
        [Test]
        public void Parser_PreservesInlineAndMultilineMovementSequences()
        {
            Lf2DatFile dat = Parse(
                "walking_frame: 4 5 6 5 8 walking_frame_end:\n" +
                "running_frame: 3\n650 651 652\nrunning_frame_end:\n" +
                "heavy_walking_frame: 3\n12 13 14\nheavy_walking_frame_end:\n" +
                "heavy_running_frame: 4 16 17 18 528 heavy_running_frame_end:\n");

            AssertSequence(dat, "walking_frame", 5, 6, 5, 8);
            AssertSequence(dat, "running_frame", 650, 651, 652);
            AssertSequence(dat, "heavy_walking_frame", 12, 13, 14);
            AssertSequence(dat, "heavy_running_frame", 16, 17, 18, 528);
        }

        [Test]
        public void Parser_SequenceCountStopsBeforeFollowingBmpProperty()
        {
            Lf2DatFile dat = Parse(
                "running_frame: 3 650 651 652 running_frame_end:\n" +
                "running_speed 18.5\n");

            AssertSequence(dat, "running_frame", 650, 651, 652);
            Lf2DatProperty speed = dat.Bmp.Properties.Single(
                property => property.Key == "running_speed");
            Assert.That(speed.Value, Is.EqualTo("18.5"));
        }

        [Test]
        public void Converter_CopiesFourMovementSequencesWithoutReordering()
        {
            Lf2DatFile dat = Parse(
                "walking_frame: 4 5 6 5 8 walking_frame_end:\n" +
                "running_frame: 3 650 651 652 running_frame_end:\n" +
                "heavy_walking_frame: 2 12 13 heavy_walking_frame_end:\n" +
                "heavy_running_frame: 2 16 528 heavy_running_frame_end:\n");
            var data = new LF2CharacterData();

            Lf2DatConverter.ApplyNativeInputDefinitionData(dat, data);

            Assert.That(data.walking_frames, Is.EqualTo(new[] { 5, 6, 5, 8 }));
            Assert.That(data.running_frames, Is.EqualTo(new[] { 650, 651, 652 }));
            Assert.That(data.heavy_walking_frames, Is.EqualTo(new[] { 12, 13 }));
            Assert.That(data.heavy_running_frames, Is.EqualTo(new[] { 16, 528 }));
        }

        [Test]
        public void Converter_CopiesAllNativeLinkedActionSelectors()
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(
                "<bmp_begin>\n<bmp_end>\n" +
                "<stats> normal_attack1: 120 normal_attack2: 121 " +
                "light_throw: 145 weapon_drink: 155 heavy_throw: 150 " +
                "run_heavy_throw: 160 run_attack: 135 jump_attack: 130 " +
                "sky_light_throw: 152 <stats_end>\n");
            var data = new LF2CharacterData();

            Lf2DatConverter.ApplyNativeInputDefinitionData(dat, data);

            Assert.That(data.normal_attack1, Is.EqualTo(120));
            Assert.That(data.normal_attack2, Is.EqualTo(121));
            Assert.That(data.light_throw, Is.EqualTo(145));
            Assert.That(data.weapon_drink, Is.EqualTo(155));
            Assert.That(data.heavy_throw, Is.EqualTo(150));
            Assert.That(data.run_heavy_throw, Is.EqualTo(160));
            Assert.That(data.run_attack, Is.EqualTo(135));
            Assert.That(data.jump_attack, Is.EqualTo(130));
            Assert.That(data.sky_light_throw, Is.EqualTo(152));
        }

        [Test]
        public void MissingNativeBuiltinData_DefaultsToZeroAndEmpty()
        {
            var first = new LF2CharacterData();
            var second = new LF2CharacterData();

            Assert.That(first.walking_frames, Is.Empty);
            Assert.That(first.running_frames, Is.Empty);
            Assert.That(first.heavy_walking_frames, Is.Empty);
            Assert.That(first.heavy_running_frames, Is.Empty);
            Assert.That(first.normal_attack1, Is.Zero);
            Assert.That(first.normal_attack2, Is.Zero);
            Assert.That(first.light_throw, Is.Zero);
            Assert.That(first.weapon_drink, Is.Zero);
            Assert.That(first.heavy_throw, Is.Zero);
            Assert.That(first.run_heavy_throw, Is.Zero);
            Assert.That(first.run_attack, Is.Zero);
            Assert.That(first.jump_attack, Is.Zero);
            Assert.That(first.sky_light_throw, Is.Zero);
            Assert.That(second.walking_frames, Is.Not.SameAs(first.walking_frames));
            Assert.That(second.running_frames, Is.Not.SameAs(first.running_frames));
            Assert.That(second.heavy_walking_frames,
                Is.Not.SameAs(first.heavy_walking_frames));
            Assert.That(second.heavy_running_frames,
                Is.Not.SameAs(first.heavy_running_frames));
        }

        [Test]
        public void RepeatedConversion_ReplacesMovementSequencesInsteadOfAppending()
        {
            var data = new LF2CharacterData();
            Lf2DatConverter.ApplyNativeInputDefinitionData(
                Parse("running_frame: 3 9 10 11 running_frame_end:\n"),
                data);

            Lf2DatConverter.ApplyNativeInputDefinitionData(
                Parse("running_frame: 2 519 520 running_frame_end:\n"),
                data);

            Assert.That(data.running_frames, Is.EqualTo(new[] { 519, 520 }));
        }

        private static Lf2DatFile Parse(string bmpBody)
        {
            return new Lf2DatParserV2().Parse(
                "<bmp_begin>\n" + bmpBody + "<bmp_end>\n");
        }

        private static void AssertSequence(
            Lf2DatFile dat,
            string name,
            params int[] expected)
        {
            Lf2BmpFrameSequence sequence = dat.Bmp.FrameSequences.Single(
                candidate => candidate.Name == name);
            Assert.That(sequence.Actions, Is.EqualTo(expected));
        }
    }
}
#endif
