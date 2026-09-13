#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05TypedFrameEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001";
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string FrameMembers = "frameId pic state cover wait next dvx dvy dvz centerx centery mp hp hit_a hit_d hit_j hit_g hit_Fj hit_Fa hit_Da hit_Ua hit_ja hit_aj hit_ad hit_jd hit_Dj hit_Uj hit_f hit_b hit_uz hit_dz hold_a hold_d hold_j hold_f hold_b hold_uz hold_dz centerz chp cmp primaryBodyKindForEffectSuppression primaryBodyRespondForHitResponse";
        private const string ItrMembers = "kind x y w h dvx dvy fall arest vrest respond effect drain spark recover dbdefend bdefend injury zwidth z dvz sound cover caughtact catchingact pickedact pickingact delay poison confus weak manacle join mimic bound facing dx dy dz gain hasGeometry";
        private const string CatchMembers = "Kind X Y Injury Cover Vaction Aaction Jaction Daction Taction Faction Baction Uzaction Dzaction ThrowVx ThrowVy Hurtable FrontHurtAct BackHurtAct Decrease DirControl ThrowInjury ThrowVz Z Recover Drain Gain";
        private const string ObjectMembers = "Kind X Y Z Action Dvx Dvy Dvz Oid Facing Hp Mp Team Reserve Effect Pic CenterX CenterY CenterZ FrameA Attacking Join JoinReserve JoinPic";
        private const string StrengthMembers = "dvx dvy fall arest vrest respond effect drain spark recover dbdefend bdefend injury zwidth z dvz sound cover caughtact";
        private static readonly Dictionary<string, string[]> MemberNames = new Dictionary<string, string[]>();
        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> Members = new Dictionary<Type, Dictionary<string, MemberInfo>>();

        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (string profile in new[] { "fixtures", "corpus" })
            {
                foreach (string line in File.ReadAllLines(Path.Combine(Root, "native-" + profile + "-index.tsv")))
                {
                    string[] c = line.Split('\t');
                    yield return new TestCaseData(profile, c[0], c[1] == "1", c[3]).SetName("NativeTypedFrame_" + profile + "_" + c[0]);
                }
            }
        }

        [TestCaseSource(nameof(Cases))]
        public void CompleteTypedProjectionMatchesNative(string profile, string file, bool accepted, string expectedHash)
        {
            string path = Path.Combine(profile == "corpus" ? RuntimeRoot : Path.Combine(Root, "fixtures"), file);
            string text = File.ReadAllText(path);
            if (!accepted)
            {
                Assert.Throws<FormatException>(() => new Lf2DatParserV2().ParseLoganContent(text, path));
                return;
            }
            var dat = new Lf2DatParserV2().ParseLoganContent(text, path);
            MethodInfo method = typeof(Lf2DatConverter).GetMethod("ConvertLoganFrameData", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "explicit native typed frame entry");
            var convert = (Func<Lf2FrameBlock, LF2FrameData>)Delegate.CreateDelegate(typeof(Func<Lf2FrameBlock, LF2FrameData>), method);
            PropertyInfo profileProperty = typeof(LF2FrameData).GetProperty("UsesLoganFrameNumbers");
            Assert.That(profileProperty, Is.Not.Null);
            var output = new StringBuilder();
            for (int f = 0; f < dat.Frames.Count; f++)
            {
                LF2FrameData frame = convert(dat.Frames[f]);
                Assert.That(profileProperty.GetValue(frame), Is.True);
                Row(output, file, f, "frame", 0, frame, FrameMembers);
                Row(output, file, f, "motion", 0, frame, "nativeDvx nativeDvy nativeDvz dx dy dz");
                TextRow(output, file, f, "name", 0, frame.frameName ?? "");
                var soundsProperty = typeof(LF2FrameData).GetProperty("FrameSounds");
                Assert.That(soundsProperty, Is.Not.Null, "ordered native sound declarations");
                var sounds = (IReadOnlyList<string>)soundsProperty.GetValue(frame);
                for (int i = 0; i < sounds.Count; i++) TextRow(output, file, f, "sound", i, sounds[i]);
                for (int i = 0; i < frame.bodies.Count; i++) Row(output, file, f, "bdy", i, frame.bodies[i], "X Y W H ZWidth HasGeometry");
                for (int i = 0; i < frame.itrs.Count; i++) Row(output, file, f, "itr", i, frame.itrs[i], ItrMembers);
                for (int i = 0; i < frame.CatchPoints.Count; i++) Row(output, file, f, "cpoint", i, frame.CatchPoints[i], CatchMembers);
                for (int i = 0; i < frame.opoints.Count; i++) Row(output, file, f, "opoint", i, frame.opoints[i], ObjectMembers);
                for (int i = 0; i < frame.FormalWeaponPoints.Count; i++) Row(output, file, f, "wpoint", i, frame.FormalWeaponPoints[i], "Kind X Y WeaponAct Attacking Cover Dvx Dvy Dvz");
                for (int i = 0; i < frame.BloodPoints.Count; i++) Row(output, file, f, "bpoint", i, frame.BloodPoints[i], "X Y");
                if (frame.CatchPoints.Count > 0)
                    Assert.That(BattleCatchPointValueAdapter.FromLegacy(frame.cpoint), Is.EqualTo(frame.CatchPoints[0]));
                if (frame.opoints.Count > 0) Assert.That(frame.opoint.Value, Is.EqualTo(frame.opoints[0]));
            }
            MethodInfo strengthDecoder = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder")
                .GetMethod("WeaponStrength", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var strength in dat.LoganWeaponStrengthRows)
            {
                var value = strengthDecoder.Invoke(null, new object[] { strength.Index, strength.Properties });
                Row(output, file, -1, "strength", strength.Index, value, StrengthMembers);
            }
            string actual = output.ToString();
            string hash;
            using (var sha = SHA256.Create()) hash = Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(actual)));
            if (hash != expectedHash)
            {
                string difference = Path.Combine(Root, "differences", profile, file + ".actual.tsv");
                Directory.CreateDirectory(Path.GetDirectoryName(difference));
                File.WriteAllText(difference, actual, new UTF8Encoding(false));
            }
            Assert.That(hash, Is.EqualTo(expectedHash), "complete typed source projection: " + file);
        }

        [Test]
        public void NativeBinary64DecoderMatchesAllWitnessValues()
        {
            MethodInfo method = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganNumericDecoder")
                .GetMethod("ParseFiniteFloat64OrZero", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            var decode = (Func<string, double>)Delegate.CreateDelegate(typeof(Func<string, double>), method);
            string[] inputs = File.ReadAllLines(Path.Combine(Root, "numbers64-input.hex"));
            string[] expected = File.ReadAllLines(Path.Combine(Root, "numbers64-native.tsv"));
            Assert.That(expected.Length, Is.EqualTo(inputs.Length));
            for (int i = 0; i < inputs.Length; i++)
            {
                byte[] bytes = new byte[inputs[i].Length / 2];
                for (int j = 0; j < bytes.Length; j++) bytes[j] = byte.Parse(inputs[i].Substring(j * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                long bits = long.Parse(expected[i].Split('\t')[1], CultureInfo.InvariantCulture);
                Assert.That(BitConverter.DoubleToInt64Bits(decode(Encoding.UTF8.GetString(bytes))), Is.EqualTo(bits), "number64 row " + i);
            }
        }

        [TestCase("c/ank/ank.dat")]
        [TestCase("c/hir/hir.dat")]
        [TestCase("c/min/min.dat")]
        [TestCase("c/min/sag.dat")]
        [TestCase("c/nar/nar.dat")]
        [TestCase("c/ank/ssnk.dat")]
        public void PreviouslyRejectedFormalDefinitionsBuild(string file)
        {
            string path = Path.Combine(RuntimeRoot, "decoded_dat", file);
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(File.ReadAllText(path), path,
                BattleContentSource.ForLoganRuntime(RuntimeRoot));
            Assert.That(data.frames.Count, Is.GreaterThan(0));
        }

        [Test]
        public void LegacyFrameDefaultsAndPermissiveIntegerRemainExplicit()
        {
            var frame = new Lf2FrameBlock();
            frame.AddProperty(new Lf2DatProperty("DVX", "+17tail"));
            var data = Lf2DatConverter.ConvertToFrameData(frame);
            Assert.That(data.wait, Is.EqualTo(1));
            Assert.That(data.dvx, Is.EqualTo(17));
            PropertyInfo profile = typeof(LF2FrameData).GetProperty("UsesLoganFrameNumbers");
            Assert.That(profile, Is.Not.Null);
            Assert.That(profile.GetValue(data), Is.False);
        }

        private static void Row(StringBuilder output, string file, int frame, string kind, int index, object value, string names)
        {
            Begin(output, file, frame, kind, index);
            if (!MemberNames.TryGetValue(names, out string[] selected)) MemberNames[names] = selected = names.Split(' ');
            Type type = value.GetType();
            if (!Members.TryGetValue(type, out var members))
            {
                members = new Dictionary<string, MemberInfo>();
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance)) members[field.Name] = field;
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)) members[property.Name] = property;
                Members[type] = members;
            }
            foreach (string name in selected)
            {
                Assert.That(members.ContainsKey(name), Is.True, type.Name + "." + name);
                object item = members[name] is FieldInfo field ? field.GetValue(value) : ((PropertyInfo)members[name]).GetValue(value);
                if (item is float number) item = BitConverter.ToInt32(BitConverter.GetBytes(number), 0);
                if (item is double wideNumber) item = BitConverter.DoubleToInt64Bits(wideNumber);
                if (item is bool flag) item = flag ? 1 : 0;
                if (item is int[] pair) item = pair.Length == 0 ? 0 : pair[0];
                output.Append('\t').Append(Convert.ToString(item, CultureInfo.InvariantCulture));
            }
            output.Append('\n');
        }

        private static void Begin(StringBuilder output, string file, int frame, string kind, int index)
        {
            output.Append(file).Append('\t').Append(frame.ToString(CultureInfo.InvariantCulture)).Append('\t').Append(kind)
                .Append('\t').Append(index.ToString(CultureInfo.InvariantCulture));
        }

        private static void TextRow(StringBuilder output, string file, int frame, string kind, int index, string value)
        {
            Begin(output, file, frame, kind, index);
            output.Append('\t').Append(Hex(Encoding.UTF8.GetBytes(value ?? ""))).Append('\n');
        }

        private static string Hex(byte[] bytes)
        {
            var text = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) text.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            return text.ToString();
        }
    }
}
#endif
