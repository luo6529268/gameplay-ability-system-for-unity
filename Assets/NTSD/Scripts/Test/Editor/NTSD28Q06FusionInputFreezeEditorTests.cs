#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NTSD.Animation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FusionInputFreezeEditorTests
    {
        private static readonly string Decoded = Path.GetFullPath("Temp/FusionInputFreeze.Virtual/decoded");
        private static readonly string Extracted = Path.GetFullPath("Temp/FusionInputFreeze.Virtual/extracted");
        private static string Text(int hp) => "<fusion_begin>\nfusion: 1\nhp: " + hp + "\nfusion_end:\n<fusion_end>\n";
        private static string[] Paths => new[]
        {
            Path.Combine(Decoded, "data", "fusion.dat"),
            Path.Combine(Extracted, "data", "fusion.dat"),
            Path.Combine(Extracted, "dat", "data", "fusion.dat"),
            Path.Combine(Extracted, "assets", "data", "fusion.dat"),
            Path.Combine(Extracted, "NTSD2.8", "data", "fusion.dat"),
        };

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void SelectsFirstAvailableFileInNativeOrder(int firstAvailable)
        {
            var files = new Dictionary<string, byte[]>();
            for (int index = firstAvailable; index < Paths.Length; index++)
                files.Add(Paths[index], Encoding.UTF8.GetBytes(Text(20 + index)));
            var reads = new List<string>();
            var input = LoganFusionCatalogInput.Capture(Decoded, Extracted, path =>
            {
                reads.Add(path);
                return files.TryGetValue(path, out var bytes) ? bytes : null;
            });
            Assert.That(input.SelectedPath, Is.EqualTo(Paths[firstAvailable]));
            Assert.That(input.UsesLockedFallback, Is.False);
            Assert.That(input.Catalog.Records[0].Hp, Is.EqualTo(20 + firstAvailable));
            Assert.That(reads, Is.EqualTo(Paths.Take(firstAvailable + 1)));
        }

        [TestCase("")]
        [TestCase("invalid")]
        [TestCase("\ufeff<fusion_begin>\n<fusion_end>")]
        public void PresentInvalidFirstFileDoesNotFallThrough(string bad)
        {
            var reads = new List<string>();
            Assert.Throws<InvalidDataException>(() => LoganFusionCatalogInput.Capture(Decoded, Extracted, path =>
            {
                reads.Add(path);
                return Encoding.UTF8.GetBytes(path == Paths[0] ? bad : Text(99));
            }));
            Assert.That(reads, Is.EqualTo(new[] { Paths[0] }));
        }

        [Test]
        public void AbsentFilesUseExactNativeFallbackAndDetectLaterAppearance()
        {
            byte[] appeared = null;
            var input = LoganFusionCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[0] ? appeared : null);
            Assert.That(input.UsesLockedFallback, Is.True);
            Assert.That(input.Catalog.IsValid, Is.True);
            Assert.That(input.Catalog.Records.Count, Is.EqualTo(2));
            Assert.That(input.Catalog.Records.Select(r => r.Id3), Is.EqualTo(new[] { 51, 52 }));
            input.AssertInputsCurrent();
            appeared = Encoding.UTF8.GetBytes(Text(9));
            Assert.Throws<InvalidOperationException>(() => input.AssertInputsCurrent());
        }

        [Test]
        public void HigherPriorityAppearanceInvalidatesSelectedSourceEvenForSameBytes()
        {
            bool higherAppeared = false;
            byte[] bytes = Encoding.UTF8.GetBytes(Text(9));
            var input = LoganFusionCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[1] || higherAppeared && path == Paths[0] ? bytes : null);
            input.AssertInputsCurrent();
            higherAppeared = true;
            Assert.Throws<InvalidOperationException>(() => input.AssertInputsCurrent());
        }

        [Test]
        public void FrozenParsedValuesSurviveBackingBytesMutationButFreshnessRejectsIt()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Text(17));
            var input = LoganFusionCatalogInput.Capture(Decoded, Extracted, path => path == Paths[0] ? bytes : null);
            byte[] changed = Encoding.UTF8.GetBytes(Text(18));
            Array.Copy(changed, bytes, bytes.Length);
            Assert.That(input.Catalog.Records[0].Hp, Is.EqualTo(17));
            Assert.Throws<InvalidOperationException>(() => input.AssertInputsCurrent());
        }

        [Test]
        public void SemanticIdentityIgnoresCommentsButRawIdentityTracksThem()
        {
            LoganFusionCatalogInput Capture(string text) => LoganFusionCatalogInput.Capture(Decoded, Extracted,
                path => path == Paths[0] ? Encoding.UTF8.GetBytes(text) : null);
            var plain = Capture(Text(17));
            var commented = Capture("# comment\n" + Text(17));
            var changed = Capture(Text(18));
            Assert.That(commented.SemanticFingerprint, Is.EqualTo(plain.SemanticFingerprint));
            Assert.That(commented.InputFingerprint, Is.Not.EqualTo(plain.InputFingerprint));
            Assert.That(changed.SemanticFingerprint, Is.Not.EqualTo(plain.SemanticFingerprint));
        }

        [Test]
        public void FormalFileMatchesNativeFallbackSemanticsAndKeepsDifferentInputIdentity()
        {
            const string root = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
            var actual = LoganFusionCatalogInput.Capture(Path.Combine(root, "decoded_dat"), Extracted);
            var fallback = LoganFusionCatalogInput.Capture(Decoded, Extracted, _ => null);
            Assert.That(actual.UsesLockedFallback, Is.False);
            Assert.That(actual.Catalog.Records.Count, Is.EqualTo(2));
            Assert.That(actual.SemanticFingerprint, Is.EqualTo(fallback.SemanticFingerprint));
            Assert.That(actual.InputFingerprint, Is.Not.EqualTo(fallback.InputFingerprint));
            actual.AssertInputsCurrent();
        }

        [Test]
        public void OptionalDecodedRootDoesNotInventAnAmbientSearchPath()
        {
            var reads = new List<string>();
            var input = LoganFusionCatalogInput.Capture(null, Extracted, path =>
            {
                reads.Add(path);
                return path == Paths[1] ? Encoding.UTF8.GetBytes(Text(23)) : null;
            });
            Assert.That(reads, Is.EqualTo(new[] { Paths[1] }));
            Assert.That(input.Catalog.Records[0].Hp, Is.EqualTo(23));
        }
    }
}
#endif
