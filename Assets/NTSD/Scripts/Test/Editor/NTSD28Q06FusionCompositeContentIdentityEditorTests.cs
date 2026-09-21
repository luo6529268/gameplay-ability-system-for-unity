#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.EditorTools;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q06FusionCompositeContentIdentityEditorTests
    {
        private const string Objects = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        private const string Input = "202122232425262728292A2B2C2D2E2F303132333435363738393A3B3C3D3E3F";
        private const string Semantic = "404142434445464748494A4B4C4D4E4F505152535455565758595A5B5C5D5E5F";
        private const string FormalRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void IndependentByteVectorMatchesAllStages()
        {
            var identity = LoganContentIdentity.FromBattleComponents(Objects.ToLowerInvariant(), Input, Semantic);
            Assert.That(identity.ObjectDefinitionFingerprint, Is.EqualTo(Objects));
            Assert.That(identity.FusionInputFingerprint, Is.EqualTo(Input));
            Assert.That(identity.FusionSemanticFingerprint, Is.EqualTo(Semantic));
            Assert.That(identity.RawDefinitionFingerprint, Is.EqualTo("C32F2109ABE09F390E5D13936B32E174AD704D64B1B56A9516D573BCF3BE55EF"));
            Assert.That(identity.SemanticFingerprint, Is.EqualTo("68EE16B9C9BCAB61B44C040C057C7686E642662F25DA2B0999FD7359729D0111"));
            Assert.That(identity.CatalogFingerprint, Is.EqualTo(0x61ABBCC9B916EE68UL));
            Assert.That(identity.CreateLocalValidationSessionIdentity(1, 2, 3, new[] { 0 }).CatalogFingerprint,
                Is.EqualTo(identity.CatalogFingerprint));
        }

        [TestCase(0, null)]
        [TestCase(1, "")]
        [TestCase(2, "bad")]
        [TestCase(0, "Z00102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F")]
        [TestCase(1, "Z00102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F")]
        [TestCase(2, "Z00102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F")]
        public void InvalidComponentCannotCreateCurrentIdentity(int index, string value)
        {
            var values = new[] { Objects, Input, Semantic };
            values[index] = value;
            Assert.Throws<ArgumentException>(() => LoganContentIdentity.FromBattleComponents(values[0], values[1], values[2]));
        }

        [Test]
        public void HistoricalAndTagOnlyIdentitiesCannotCreateCurrentSessions()
        {
            var old = LoganContentIdentity.FromDefinitionFingerprint(Objects);
            Assert.That(old.DecodeContractTag, Is.EqualTo("NTSD28_LOGAN_DAT_SEMANTICS_V2"));
            Assert.Throws<InvalidOperationException>(() => old.CreateLocalValidationSessionIdentity(1, 2, 3, new[] { 0 }));
            var tagOnly = LoganContentIdentity.ForDecodeContract(Objects, LoganContentIdentity.CurrentDecodeContractTag);
            Assert.Throws<InvalidOperationException>(() => tagOnly.CreateLocalValidationSessionIdentity(1, 2, 3, new[] { 0 }));
        }

        private static string CreateRuntime()
        {
            string root = Path.GetFullPath("Temp/Q06FusionComposite/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat/data"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            Directory.CreateDirectory(Path.Combine(root, "vfs"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,56,0,a.dat,missing\n", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                "<bmp_begin>\nname: composite\n<bmp_end>\n<frame> 0 standing\nstate: 0 wait: 1 next: 0\n<frame_end>\n", new UTF8Encoding(false));
            return root;
        }

        private static string Fusion(int hp) => "<fusion_begin>\nfusion: 1\nhp: " + hp + "\nfusion_end:\n<fusion_end>\n";

        [Test]
        public void FusionOnlyAndCommentOnlyChangesInvalidateCacheWithStableObjects()
        {
            string root = CreateRuntime();
            string path = Path.Combine(root, "decoded_dat/data/fusion.dat");
            File.WriteAllText(path, Fusion(17), new UTF8Encoding(false));
            var first = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            File.WriteAllText(path, Fusion(18), new UTF8Encoding(false));
            var changed = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            Assert.That(changed.Catalog.DefinitionFingerprint, Is.EqualTo(first.Catalog.DefinitionFingerprint));
            Assert.That(changed.VisualFingerprint, Is.EqualTo(first.VisualFingerprint));
            Assert.That(changed.ContentIdentity.FusionSemanticFingerprint, Is.Not.EqualTo(first.ContentIdentity.FusionSemanticFingerprint));
            Assert.That(changed.SourceCacheKey, Is.Not.EqualTo(first.SourceCacheKey));
            Assert.Throws<InvalidDataException>(() => first.AssertInputsCurrent());
            File.AppendAllText(path, "# comment only\n", new UTF8Encoding(false));
            var commented = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            Assert.That(commented.ContentIdentity.FusionSemanticFingerprint, Is.EqualTo(changed.ContentIdentity.FusionSemanticFingerprint));
            Assert.That(commented.ContentIdentity.FusionInputFingerprint, Is.Not.EqualTo(changed.ContentIdentity.FusionInputFingerprint));
            Assert.That(commented.ContentIdentity.RawDefinitionFingerprint, Is.Not.EqualTo(changed.ContentIdentity.RawDefinitionFingerprint));
            Assert.That(commented.SourceCacheKey, Is.Not.EqualTo(changed.SourceCacheKey));
            Assert.Throws<InvalidDataException>(() => changed.AssertInputsCurrent());
        }

        [Test]
        public void SameByteHigherPrioritySelectionInvalidatesFrozenCandidate()
        {
            string root = CreateRuntime();
            File.WriteAllText(Path.Combine(root, "data/fusion.dat"), Fusion(17), new UTF8Encoding(false));
            var first = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            File.WriteAllText(Path.Combine(root, "decoded_dat/data/fusion.dat"), Fusion(17), new UTF8Encoding(false));
            var second = LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
            Assert.That(second.SourceCacheKey, Is.EqualTo(first.SourceCacheKey));
            Assert.Throws<InvalidDataException>(() => first.AssertInputsCurrent());
        }

        [Test]
        public void FormalCatalogHeaderMatchesIndependentVectorAndFileDiffersFromFallback()
        {
            var catalog = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(FormalRoot));
            var header = NTSD28TraceContentIdentity.FromLoganCatalog(catalog);
            Assert.That(catalog.Entries.Count, Is.EqualTo(330));
            Assert.That(header["objectDefinitionSha256"], Is.EqualTo("4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C"));
            Assert.That(header["fusionInputSha256"], Is.EqualTo("28E1809EDE9C18E49629E6CBEC20CBD11FCB6E0175DF9DEE6ED90CE542886D5F"));
            Assert.That(header["fusionSemanticSha256"], Is.EqualTo("81CA495386950C3F8D5F00B43A62934410D4F6F738CD7B720610241E88F8D369"));
            Assert.That(header["rawDefinitionSha256"], Is.EqualTo("3A7FF5A15521B9766FC35BBF04B8FA9D3F4BDEA5C5D0045A578BA8B523C37CC4"));
            Assert.That(header["semanticSha256"], Is.EqualTo("FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147"));
            Assert.That(header["catalogFingerprint64"], Is.EqualTo("0FEFD4B968D618FD"));
            Assert.That(header["scope"], Is.EqualTo("catalog-object-fusion-definitions"));
            var fallback = LoganFusionCatalogInput.Capture(null, "Temp/virtual", _ => null);
            Assert.That(fallback.SemanticFingerprint, Is.EqualTo(catalog.FusionInput.SemanticFingerprint));
            var other = LoganContentIdentity.FromBattleComponents(catalog.DefinitionFingerprint,
                fallback.InputFingerprint, fallback.SemanticFingerprint);
            Assert.That(other.SemanticFingerprint, Is.Not.EqualTo(catalog.ContentIdentity.SemanticFingerprint));
            File.WriteAllText("Temp/Q06FusionComposite-formal-header.json", JsonConvert.SerializeObject(header, Formatting.Indented));
        }
    }
}
#endif
