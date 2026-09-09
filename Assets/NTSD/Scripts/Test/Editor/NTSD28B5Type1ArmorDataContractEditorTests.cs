#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Reflection;

using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorDataContractEditorTests
    {
        [Test]
        public void FullArmorBlock_ParsesEveryAuthorityField()
        {
            LF2CharacterData data = Convert(@"
<bmp_begin>
<bmp_end>
<armor>
 sound1: data\085.wav sound2: data\002.wav
 type: 1 ratio: 5 decrease: 50 mp: 2 fall: -1 bdefend: 3 injury: 4
 spark: 5 hp: 6 recover: 7 facing: 8 action: 9 reserve: 10 delay: 11
 frame: 20 30 frame: 40 50
 state: 4 state: 5 kind: 1 kind: 6 id: 99 id: 100 effect: 2 effect: 20
<armor_end>
<frame> 0 test
 pic: 0
<frame_end>");
            object armor = ArmorAt(data, 0);

            AssertFields(armor,
                ("type", 1), ("ratio", 5), ("decrease", 50), ("mp", 2),
                ("fall", -1), ("bdefend", 3), ("injury", 4), ("spark", 5),
                ("hp", 6), ("recover", 7), ("facing", 8), ("action", 9),
                ("reserve", 10), ("delay", 11));
            Assert.That(Field<string>(armor, "sound1"), Is.EqualTo("data\\085.wav"));
            Assert.That(Field<string>(armor, "sound2"), Is.EqualTo("data\\002.wav"));
            Assert.That(IntList(armor, "states"), Is.EqualTo(new[] { 4, 5 }));
            Assert.That(IntList(armor, "kinds"), Is.EqualTo(new[] { 1, 6 }));
            Assert.That(IntList(armor, "ids"), Is.EqualTo(new[] { 99, 100 }));
            Assert.That(IntList(armor, "effects"), Is.EqualTo(new[] { 2, 20 }));
            IList ranges = ListField(armor, "frame_ranges");
            Assert.That(ranges.Count, Is.EqualTo(2));
            AssertFields(ranges[0], ("first", 20), ("last", 30));
            AssertFields(ranges[1], ("first", 40), ("last", 50));
        }

        [Test]
        public void TypePrecedenceAndLastWin_MatchAuthorityFieldBag()
        {
            LF2CharacterData data = Convert(@"
<bmp_begin><bmp_end>
<armor>
 ptype: 2 ptype: 3 ratio: 1 ratio: 7 sound1: a.wav sound1: b.wav
<armor_end>
<armor>
 ptype: 2 type: 1 type: 4
<armor_end>
<frame> 0 test pic: 0 <frame_end>");

            object fallback = ArmorAt(data, 0);
            AssertFields(fallback, ("type", 3), ("ratio", 7));
            Assert.That(Field<string>(fallback, "sound1"), Is.EqualTo("b.wav"));
            AssertFields(ArmorAt(data, 1), ("type", 4));
        }

        [Test]
        public void MissingAndMalformedValues_DefaultOrAreIgnored()
        {
            LF2CharacterData data = Convert(@"
<bmp_begin><bmp_end>
<armor>
 frame: nope frame: 2 nope state: nope kind: effect: nope
<armor_end>
<frame> 0 test pic: 0 <frame_end>");
            object armor = ArmorAt(data, 0);

            AssertFields(armor,
                ("type", 0), ("ratio", 0), ("decrease", 0), ("delay", 0));
            Assert.That(Field<string>(armor, "sound1"), Is.Null);
            Assert.That(ListField(armor, "frame_ranges").Count, Is.Zero);
            Assert.That(IntList(armor, "states"), Is.Empty);
            Assert.That(IntList(armor, "kinds"), Is.Empty);
            Assert.That(IntList(armor, "effects"), Is.Empty);
        }

        [Test]
        public void MultipleBlocks_PreserveOrderAndRepeatedApplyClearsStaleData()
        {
            var data = new LF2CharacterData();
            Apply(Parse(@"
<bmp_begin><bmp_end>
<armor> type: 1 <armor_end>
<armor> type: 2 <armor_end>
<frame> 0 test pic: 0 <frame_end>"), data);

            Assert.That(ArmorList(data).Count, Is.EqualTo(2));
            AssertFields(ArmorAt(data, 0), ("type", 1));
            AssertFields(ArmorAt(data, 1), ("type", 2));

            Apply(Parse(@"
<bmp_begin><bmp_end>
<frame> 0 test pic: 0 <frame_end>"), data);
            Assert.That(ArmorList(data).Count, Is.Zero);
        }

        [Test]
        public void DeepCopy_DoesNotShareMutableListsAndKeepsFingerprint()
        {
            object source = ArmorAt(Convert(@"
<bmp_begin><bmp_end>
<armor> type: 1 frame: 2 3 state: 4 sound1: a.wav <armor_end>
<frame> 0 test pic: 0 <frame_end>"), 0);
            MethodInfo copyMethod = source.GetType().GetMethod("DeepCopy");
            MethodInfo fingerprint = source.GetType().GetMethod("ComputeFingerprint64");
            Assert.That(copyMethod, Is.Not.Null);
            Assert.That(fingerprint, Is.Not.Null);
            object copy = copyMethod.Invoke(source, null);

            Assert.That(ListField(copy, "states"), Is.Not.SameAs(ListField(source, "states")));
            Assert.That(ListField(copy, "frame_ranges"),
                Is.Not.SameAs(ListField(source, "frame_ranges")));
            Assert.That(Fingerprint(fingerprint, copy),
                Is.EqualTo(Fingerprint(fingerprint, source)));

            ListField(copy, "states").Add(9);
            Assert.That(IntList(source, "states"), Is.EqualTo(new[] { 4 }));
            Assert.That(Fingerprint(fingerprint, copy),
                Is.Not.EqualTo(Fingerprint(fingerprint, source)));
        }

        [Test]
        public void Fingerprint_DistinguishesRangeOrderSoundNullAndScalar()
        {
            object baseline = ArmorAt(Convert(@"
<bmp_begin><bmp_end><armor> frame: 1 2 <armor_end>
<frame> 0 test pic: 0 <frame_end>"), 0);
            MethodInfo fingerprint = baseline.GetType().GetMethod("ComputeFingerprint64");
            Assert.That(fingerprint, Is.Not.Null);
            ulong baselineHash = Fingerprint(fingerprint, baseline);

            object scalar = baseline.GetType().GetMethod("DeepCopy").Invoke(baseline, null);
            scalar.GetType().GetField("ratio").SetValue(scalar, 1);
            Assert.That(Fingerprint(fingerprint, scalar), Is.Not.EqualTo(baselineHash));

            object sound = baseline.GetType().GetMethod("DeepCopy").Invoke(baseline, null);
            sound.GetType().GetField("sound1").SetValue(sound, string.Empty);
            Assert.That(Fingerprint(fingerprint, sound), Is.Not.EqualTo(baselineHash));

            object reversed = ArmorAt(Convert(@"
<bmp_begin><bmp_end><armor> frame: 2 1 <armor_end>
<frame> 0 test pic: 0 <frame_end>"), 0);
            Assert.That(Fingerprint(fingerprint, reversed), Is.Not.EqualTo(baselineHash));
        }

        [Test]
        public void WarmFingerprint_AllocatesNoManagedMemory()
        {
            var armor = new LF2ArmorData
            {
                type = 1,
                ratio = 5,
                decrease = 50,
                sound1 = "data\\085.wav",
            };
            armor.frame_ranges.Add(new LF2ArmorFrameRange
            {
                first = 20,
                last = 30,
            });
            armor.states.Add(4);
            armor.kinds.Add(1);

            _ = armor.ComputeFingerprint64();
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            ulong checksum = 1469598103934665603UL;
            for (int index = 0; index < 4096; index++)
            {
                checksum *= 1099511628211UL;
                checksum ^= armor.ComputeFingerprint64() + (ulong)index;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static LF2CharacterData Convert(string text)
        {
            Lf2DatFile dat = Parse(text);
            var data = new LF2CharacterData();
            Apply(dat, data);
            return data;
        }

        private static Lf2DatFile Parse(string text)
        {
            return new Lf2DatParserV2().Parse(text);
        }

        private static void Apply(Lf2DatFile dat, LF2CharacterData data)
        {
            MethodInfo method = typeof(Lf2DatConverter).GetMethod(
                "ApplyNativeArmorDefinitionData",
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { dat, data });
        }

        private static IList ArmorList(LF2CharacterData data)
        {
            FieldInfo field = typeof(LF2CharacterData).GetField("armors");
            Assert.That(field, Is.Not.Null);
            return (IList)field.GetValue(data);
        }

        private static object ArmorAt(LF2CharacterData data, int index)
        {
            return ArmorList(data)[index];
        }

        private static void AssertFields(
            object value,
            params (string Name, int Expected)[] fields)
        {
            foreach ((string name, int expected) in fields)
                Assert.That(Field<int>(value, name), Is.EqualTo(expected), name);
        }

        private static T Field<T>(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, name);
            return (T)field.GetValue(value);
        }

        private static IList ListField(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, name);
            return (IList)field.GetValue(value);
        }

        private static int[] IntList(object value, string name)
        {
            IList list = ListField(value, name);
            var result = new int[list.Count];
            for (int index = 0; index < list.Count; index++)
                result[index] = (int)list[index];
            return result;
        }

        private static ulong Fingerprint(MethodInfo method, object value)
        {
            return (ulong)method.Invoke(value, null);
        }
    }
}
#endif
