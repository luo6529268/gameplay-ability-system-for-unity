#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;

using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5DefinitionAttackingCarrierEditorTests
    {
        [Test]
        public void StatsAttacking_ParsesSignedValue()
        {
            LF2CharacterData data = Apply(@"
<bmp_begin><bmp_end>
<stats> attacking: -25 <stats_end>
<frame> 0 test pic: 0 <frame_end>");

            Assert.That(Attacking(data), Is.EqualTo(-25));
        }

        [Test]
        public void RepeatedStatsAttacking_UsesLastDeclaration()
        {
            LF2CharacterData data = Apply(@"
<bmp_begin><bmp_end>
<stats> attacking: 25 attacking: 77 <stats_end>
<frame> 0 test pic: 0 <frame_end>");

            Assert.That(Attacking(data), Is.EqualTo(77));
        }

        [Test]
        public void RepeatedApplyWithoutStats_ClearsStaleValue()
        {
            var data = new LF2CharacterData();
            Apply(@"
<bmp_begin><bmp_end>
<stats> attacking: 25 <stats_end>
<frame> 0 test pic: 0 <frame_end>", data);
            Assert.That(Attacking(data), Is.EqualTo(25));

            Apply(@"
<bmp_begin><bmp_end>
<frame> 0 test pic: 0 <frame_end>", data);
            Assert.That(Attacking(data), Is.Zero);
        }

        [Test]
        public void WpointAttacking_DoesNotPopulateDefinitionCarrier()
        {
            LF2CharacterData data = Apply(@"
<bmp_begin><bmp_end>
<frame> 0 test
 pic: 0
 wpoint: kind: 1 attacking: 99 wpoint_end:
<frame_end>");

            Assert.That(Attacking(data), Is.Zero);
        }

        private static LF2CharacterData Apply(string text)
        {
            var data = new LF2CharacterData();
            Apply(text, data);
            return data;
        }

        private static void Apply(string text, LF2CharacterData data)
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text);
            Lf2DatConverter.ApplyNativeInputDefinitionData(dat, data);
        }

        private static int Attacking(LF2CharacterData data)
        {
            FieldInfo field = typeof(LF2CharacterData).GetField(
                "definition_attacking",
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (int)field.GetValue(data);
        }
    }
}
#endif
