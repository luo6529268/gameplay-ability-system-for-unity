#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q07KindCatalogInputEditorTests
    {
        private static readonly string Decoded = Path.GetFullPath("Temp/KindInputFreeze.Virtual/decoded");
        private static readonly string Extracted = Path.GetFullPath("Temp/KindInputFreeze.Virtual/extracted");
        private static string[] Paths => new[]
        {
            Path.Combine(Decoded, "data", "kind.dat"),
            Path.Combine(Extracted, "data", "kind.dat"),
            Path.Combine(Extracted, "dat", "data", "kind.dat"),
            Path.Combine(Extracted, "assets", "data", "kind.dat"),
            Path.Combine(Extracted, "NTSD2.8", "data", "kind.dat")
        };

        private static string Text(int effect = 209) =>
            "<kind>\neffect: " + effect + "\nframe: 40\nbound: 3\n" +
            "id: 8\nid: 209\nid: 213\nbound_end:\nrespond: 7\n" +
            "id: 200\nid: 203\nid: 205\nid: 206\nid: 207\nid: 215\nid: 216\n" +
            "respond_end:\n<kind_end>\n";

        [Test]
        public void StagedFormalFileMatchesNativeFallbackSemantics()
        {
            const string sha = "39E30DF8D86A5FC374B26C80BE358A6503B2C3096D8681C586478D73A0900011";
            string root = Path.GetFullPath("Assets/NTSD/Content/LoganRuntime");
            string path = Path.Combine(root, "decoded_dat", "data", "kind.dat");
            byte[] bytes = File.ReadAllBytes(path);
            using var hash = SHA256.Create();
            Assert.That(BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty),
                Is.EqualTo(sha));

            LoganKindCatalogInput selected = LoganKindCatalogInput.Capture(
                Path.Combine(root, "decoded_dat"), root);
            LoganKindCatalogInput fallback = LoganKindCatalogInput.Capture(Decoded, Extracted, _ => null);
            Assert.That(selected.SelectedPath, Is.EqualTo(path));
            Assert.That(selected.UsesLockedFallback, Is.False);
            Assert.That(selected.Catalog.IsValid, Is.True);
            Assert.That(selected.Catalog.Records.Count, Is.EqualTo(1));
            LoganKindRecord record = selected.Catalog.Records[0];
            Assert.That(record.Effect, Is.EqualTo(209));
            Assert.That(record.Frame, Is.EqualTo(40));
            Assert.That(record.BoundIds, Is.EqualTo(new[] { 8, 209, 213 }));
            Assert.That(record.RespondIds, Is.EqualTo(new[] { 200, 203, 205, 206, 207, 215, 216 }));
            Assert.That(record.Binds(8) && !record.Binds(200) && record.RespondsTo(216), Is.True);
            Assert.That(selected.SemanticFingerprint, Is.EqualTo(fallback.SemanticFingerprint));
            Assert.That(selected.InputFingerprint, Is.Not.EqualTo(fallback.InputFingerprint));
            selected.AssertInputsCurrent();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void SelectsFirstRegularFileInNativeOrder(int firstAvailable)
        {
            var files = new Dictionary<string, byte[]>();
            for (int index = firstAvailable; index < Paths.Length; index++)
                files.Add(Paths[index], Encoding.UTF8.GetBytes(Text(300 + index)));
            var reads = new List<string>();
            LoganKindCatalogInput input = LoganKindCatalogInput.Capture(Decoded, Extracted, path =>
            {
                reads.Add(path);
                return files.TryGetValue(path, out byte[] bytes) ? bytes : null;
            });
            Assert.That(input.SelectedPath, Is.EqualTo(Paths[firstAvailable]));
            Assert.That(input.Catalog.Records[0].Effect, Is.EqualTo(300 + firstAvailable));
            Assert.That(reads, Is.EqualTo(Paths.Take(firstAvailable + 1)));
        }

        [Test]
        public void InvalidSelectedFileDoesNotFallThroughToLowerPriority()
        {
            var reads = new List<string>();
            Assert.Throws<InvalidDataException>(() => LoganKindCatalogInput.Capture(Decoded, Extracted,
                path =>
                {
                    reads.Add(path);
                    return Encoding.UTF8.GetBytes(path == Paths[0]
                        ? "<kind>\nbound: 2\nid: 8\nbound_end:\n<kind_end>\n"
                        : Text(999));
                }));
            Assert.That(reads, Is.EqualTo(new[] { Paths[0] }));
        }

        [Test]
        public void MissingFileFallbackDetectsLaterAppearance()
        {
            byte[] appeared = null;
            LoganKindCatalogInput input = LoganKindCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[0] ? appeared : null);
            Assert.That(input.UsesLockedFallback, Is.True);
            Assert.That(input.Catalog.Records[0].Effect, Is.EqualTo(209));
            input.AssertInputsCurrent();
            appeared = Encoding.UTF8.GetBytes(Text());
            Assert.Throws<InvalidOperationException>(() => input.AssertInputsCurrent());
        }

        [Test]
        public void CaptureFreezesValuesAndRejectsChangedBytesOrHigherPriorityPath()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Text(317));
            bool higherAppeared = false;
            LoganKindCatalogInput input = LoganKindCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[1] || higherAppeared && path == Paths[0] ? bytes : null);
            Assert.That(input.Catalog.Records[0].Effect, Is.EqualTo(317));
            input.AssertInputsCurrent();
            byte[] changed = Encoding.UTF8.GetBytes(Text(318));
            Array.Copy(changed, bytes, bytes.Length);
            Assert.That(input.Catalog.Records[0].Effect, Is.EqualTo(317));
            Assert.Throws<InvalidOperationException>(() => input.AssertInputsCurrent());
            Array.Copy(Encoding.UTF8.GetBytes(Text(317)), bytes, bytes.Length);
            higherAppeared = true;
            Assert.Throws<InvalidOperationException>(() => input.AssertInputsCurrent());
        }

        [Test]
        public void NativeGrammarKeepsLastWriteDefaultsFirstEffectAndWarnings()
        {
            LoganKindCatalog catalog = LoganKindCatalogParser.ParseText(
                "outside\n<kind>\neffect: 1\neffect: 2\nunknown: x\n" +
                "bound: 0\nbound_end:\nrespond: 1\nid: -3\nrespond_end:\n<kind_end>\n" +
                "<kind>\neffect: 2\nframe: 80\n<kind_end>\n");
            Assert.That(catalog.IsValid, Is.True);
            Assert.That(catalog.Records.Count, Is.EqualTo(2));
            Assert.That(catalog.Records[0].Frame, Is.Zero);
            Assert.That(catalog.Records[0].RespondIds, Is.EqualTo(new[] { -3 }));
            Assert.That(catalog.FindEffect(2), Is.SameAs(catalog.Records[0]));
            Assert.That(catalog.Diagnostics.All(d => d.Severity == LoganKindDiagnosticSeverity.Warning), Is.True);
        }

        [Test]
        public void EmptyTextIsNativeValidButMalformedCountsAndBomFail()
        {
            Assert.That(LoganKindCatalogParser.ParseText(string.Empty).IsValid, Is.True);
            Assert.That(LoganKindCatalogParser.ParseText(
                "<kind>\nrespond: 1\nid: 1\nid: 2\nrespond_end:\n<kind_end>\n").IsValid,
                Is.False);
            Assert.That(LoganKindCatalogParser.ParseText(
                "<kind>\nbound: 2\nid: 8\nbound_end:\n<kind_end>\n").IsValid,
                Is.False);
            Assert.That(LoganKindCatalogParser.ParseText("\ufeff<kind>\n<kind_end>\n").IsValid,
                Is.False);
            Assert.That(LoganKindCatalogParser.ParseText("<kind>\n").IsValid, Is.False);
        }

        [TestCase("-2147483648", int.MinValue)]
        [TestCase("2147483647", int.MaxValue)]
        [TestCase("-0", 0)]
        [TestCase("0007", 7)]
        public void NativeStrictSignedIntegerBoundariesAreAccepted(string value, int expected)
        {
            LoganKindCatalog catalog = LoganKindCatalogParser.ParseText(
                "<kind>\neffect: " + value + "\n<kind_end>\n");
            Assert.That(catalog.IsValid, Is.True);
            Assert.That(catalog.Records[0].Effect, Is.EqualTo(expected));
        }

        [TestCase("+7")]
        [TestCase("1.0")]
        [TestCase("1x")]
        [TestCase("2147483648")]
        [TestCase("-2147483649")]
        [TestCase("")]
        public void InvalidNumericFieldCannotBeSilentlyCoerced(string value)
        {
            LoganKindCatalog catalog = LoganKindCatalogParser.ParseText(
                "<kind>\neffect: 9\neffect: " + value + "\n<kind_end>\n");
            Assert.That(catalog.IsValid, Is.False);
            Assert.That(catalog.Records[0].Effect, Is.EqualTo(9));
            Assert.That(catalog.Diagnostics.Any(d => d.Severity == LoganKindDiagnosticSeverity.Error),
                Is.True);
        }

        [Test]
        public void PresentEmptyFileStaysSelectedInsteadOfUsingFallback()
        {
            LoganKindCatalogInput input = LoganKindCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[0] ? Array.Empty<byte>() : null);
            Assert.That(input.UsesLockedFallback, Is.False);
            Assert.That(input.SelectedPath, Is.EqualTo(Paths[0]));
            Assert.That(input.Catalog.IsValid, Is.True);
            Assert.That(input.Catalog.Records, Is.Empty);
        }

        [Test]
        public void NativeRecordLimitKeepsFirstHundredAndReportsError()
        {
            string text = string.Concat(Enumerable.Range(0, 101)
                .Select(index => "<kind>\neffect: " + index + "\n<kind_end>\n"));
            LoganKindCatalog catalog = LoganKindCatalogParser.ParseText(text);
            Assert.That(catalog.IsValid, Is.False);
            Assert.That(catalog.Records.Count, Is.EqualTo(LoganKindCatalog.NativeRecordLimit));
            Assert.That(catalog.Records[99].Effect, Is.EqualTo(99));
            Assert.That(catalog.Diagnostics.Any(d => d.Severity == LoganKindDiagnosticSeverity.Error),
                Is.True);
        }

        [Test]
        public void InputFingerprintTracksCommentsButSemanticFingerprintDoesNot()
        {
            LoganKindCatalogInput Capture(string text) => LoganKindCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[0] ? Encoding.UTF8.GetBytes(text) : null);
            LoganKindCatalogInput plain = Capture(Text());
            LoganKindCatalogInput commented = Capture("# ignored\n" + Text());
            LoganKindCatalogInput changed = Capture(Text(210));
            Assert.That(commented.SemanticFingerprint, Is.EqualTo(plain.SemanticFingerprint));
            Assert.That(commented.InputFingerprint, Is.Not.EqualTo(plain.InputFingerprint));
            Assert.That(changed.SemanticFingerprint, Is.Not.EqualTo(plain.SemanticFingerprint));
        }
    }
}
#endif
