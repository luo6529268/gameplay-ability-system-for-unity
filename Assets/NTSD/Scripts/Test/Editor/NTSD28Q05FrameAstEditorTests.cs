#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05FrameAstEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001";
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (string profile in new[] { "fixtures", "corpus" })
            {
                foreach (string line in File.ReadAllLines(Path.Combine(Root, "native-" + profile + "-index.tsv")))
                {
                    string[] c = line.Split('\t');
                    yield return new TestCaseData(profile, c[0], c[1] == "1", int.Parse(c[2]), c[3])
                        .SetName("NativeFrameAst_" + profile + "_" + c[0]);
                }
            }
        }

        [TestCaseSource(nameof(Cases))]
        public void FrameAstMatchesNative(string profile, string file, bool accepted, int count, string expectedHash)
        {
            string path = Path.Combine(profile == "corpus" ? RuntimeRoot : Path.Combine(Root, "fixtures"), file);
            string text = File.ReadAllText(path);
            if (!accepted)
            {
                Assert.Throws<FormatException>(() => new Lf2DatParserV2().ParseLoganContent(text, path));
                return;
            }
            Lf2DatFile dat = new Lf2DatParserV2().ParseLoganContent(text, path);
            Assert.That(dat.Frames.Count, Is.EqualTo(count), file);
            var output = new StringBuilder();
            for (int i = 0; i < dat.Frames.Count; i++)
            {
                var frame = dat.Frames[i];
                Row(output, file, "frame", i, frame.FrameIndex, "", frame.FrameName ?? "");
                foreach (var field in frame.Properties)
                    Row(output, file, "field", i, -1, field.Key, field.Value);
                for (int j = 0; j < frame.SubBlocks.Count; j++)
                {
                    var block = frame.SubBlocks[j];
                    Row(output, file, "block", i, j, block.Name, "");
                    foreach (var field in block.Properties)
                        Row(output, file, "field", i, j, field.Key, field.Value);
                }
            }
            string actual = output.ToString();
            string actualHash;
            using (var sha = SHA256.Create())
                actualHash = Hex(sha.ComputeHash(Encoding.UTF8.GetBytes(actual)));
            if (actualHash != expectedHash)
            {
                string outputPath = Path.Combine(Root, "differences", profile, file + ".actual.tsv");
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllText(outputPath, actual, new UTF8Encoding(false));
            }
            Assert.That(actualHash, Is.EqualTo(expectedHash), file + " complete ordered AST; see native TSV and differences artifact");
        }

        [Test]
        public void FormalSsnkBuildsAfterNativeLexerIgnoresNumericFieldName()
        {
            string path = Path.Combine(RuntimeRoot, "decoded_dat/c/ank/ssnk.dat");
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(File.ReadAllText(path), path,
                BattleContentSource.ForLoganRuntime(RuntimeRoot));
            Assert.That(data.frames.Count, Is.GreaterThan(0));
            var frame = data.frames.Find(value => value.frameId == 428);
            Assert.That(frame, Is.Not.Null);
            Assert.That(frame.wpoints.Count, Is.GreaterThan(0));
            Assert.That(frame.wpoints[0].rawProperties.ContainsKey("7"), Is.False);
        }

        [Test]
        public void LegacyParserStillAcceptsItsExistingDuplicateFrameSyntax()
        {
            string text = File.ReadAllText(Path.Combine(Root, "fixtures", "duplicate.dat"));
            Assert.That(new Lf2DatParserV2().Parse(text).Frames.Count, Is.EqualTo(2));
        }

        private static void Row(StringBuilder output, string file, string type, int frame, int block, string key, string value)
        {
            output.Append(file).Append('\t').Append(type).Append('\t').Append(frame.ToString(CultureInfo.InvariantCulture))
                .Append('\t').Append(block.ToString(CultureInfo.InvariantCulture)).Append('\t').Append(key).Append('\t')
                .Append(Hex(Encoding.UTF8.GetBytes(value ?? ""))).Append('\n');
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
