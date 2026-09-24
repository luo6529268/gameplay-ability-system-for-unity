#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07KindIdentityPublicationEditorTests
    {
        private const string FormalRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void FormalAndStagedSevenComponentIdentityMatchIndependentVector()
        {
            var formalNative = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(FormalRoot));
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            var formal = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(FormalRoot), mode);
            var staged = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(
                Path.GetFullPath("Assets/NTSD/Content/LoganRuntime")), mode);
            Assert.That(formal.KindInput.Catalog.Records.Count, Is.EqualTo(1));
            Assert.That(formal.KindInput.InputFingerprint,
                Is.EqualTo("B18E147AB26065B0668BB0ACCE9DF89C5352ABD6E467ABB89A4E01C3178AF5E0"));
            Assert.That(formal.KindInput.SemanticFingerprint,
                Is.EqualTo("48EE87992BACEC941C70E4176012D4AD4F060CAB7CA551EA8317555D40A703D5"));
            Assert.That(formalNative.ContentIdentity.BattleInputContractTag,
                Is.EqualTo("NTSD28_LOGAN_BATTLE_INPUTS_V3"));
            Assert.That(formalNative.ContentIdentity.RawDefinitionFingerprint,
                Is.EqualTo("E88C4CBAC231E6C6FF47A41EFC86D46B46F74B63A8E0F3ABD30ED37C5C05C36A"));
            Assert.That(formalNative.ContentIdentity.SemanticFingerprint,
                Is.EqualTo("B8B13894088DDE96D71771C9110FBD84E2C03AE712FE32222B99C5DFE8155A45"));
            Assert.That(formalNative.ContentIdentity.CatalogFingerprint.ToString("X16"),
                Is.EqualTo("96DE8D089438B1B8"));
            Assert.That(staged.ContentIdentity.SemanticFingerprint,
                Is.EqualTo(formal.ContentIdentity.SemanticFingerprint));
            var prior = LoganContentIdentity.FromBattleComponents(formalNative.DefinitionFingerprint,
                formalNative.FusionInput.InputFingerprint, formalNative.FusionInput.SemanticFingerprint,
                formalNative.ModeComboInput.InputFingerprint, formalNative.ModeComboInput.SemanticFingerprint);
            Assert.That(prior.BattleInputContractTag, Is.EqualTo("NTSD28_LOGAN_BATTLE_INPUTS_V2"));
            Assert.That(prior.SemanticFingerprint,
                Is.EqualTo("FF1218FF3FEB409FF6B2F8EDB1090591612B3D82D7FA91601E596D29CDF13DFB"));
        }

        [Test]
        public void KindOnlyFallbackAndSelectedBytesHaveDifferentIdentity()
        {
            string root = CreateRuntime();
            var source = BattleContentSource.ForLoganRuntime(root);
            var fallback = LoganVisualContentCandidate.Capture(source);
            Assert.That(fallback.ContentIdentity.BattleInputContractTag,
                Is.EqualTo("NTSD28_LOGAN_BATTLE_INPUTS_V3_KIND_ONLY"));
            Assert.That(fallback.Catalog.KindInput.UsesLockedFallback, Is.True);
            string path = Path.Combine(root, "decoded_dat/data/kind.dat");
            File.Copy(Path.GetFullPath("Assets/NTSD/Content/LoganRuntime/decoded_dat/data/kind.dat"), path);
            var selected = LoganVisualContentCandidate.Capture(source);
            Assert.That(selected.Catalog.KindInput.UsesLockedFallback, Is.False);
            Assert.That(selected.Catalog.KindInput.SemanticFingerprint,
                Is.EqualTo(fallback.Catalog.KindInput.SemanticFingerprint));
            Assert.That(selected.ContentIdentity.SemanticFingerprint,
                Is.Not.EqualTo(fallback.ContentIdentity.SemanticFingerprint));
            Assert.Throws<InvalidDataException>(() => fallback.AssertInputsCurrent());
            File.AppendAllText(path, "# changed bytes\n", new UTF8Encoding(false));
            Assert.Throws<InvalidDataException>(() => selected.AssertInputsCurrent());
        }

        [Test]
        public void PreparedWorldCatalogUsesCapturedKindAndIdentity()
        {
            var captured = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(FormalRoot));
            var definitions = captured.Entries.Select(entry =>
                new ObjectDefinition(entry.Id, entry.Type, entry.SourcePath)).ToArray();
            var prepared = new BattleRuntimeDataCatalog();
            prepared.Prepare(definitions, _ => null, default, captured);
            Assert.That(prepared.IsReady, Is.True);
            Assert.That(prepared.KindCatalog, Is.SameAs(captured.KindInput.Catalog));
            Assert.That(prepared.LoganContentIdentity, Is.SameAs(captured.ContentIdentity));
            Assert.That(prepared.KindCatalog.FindEffect(209)?.Frame, Is.EqualTo(40));
            definitions[0] = new ObjectDefinition(-1, definitions[0].type, "invalid.dat");
            Assert.Throws<ArgumentException>(() => prepared.Prepare(definitions, _ => null, default, captured));
            Assert.That(prepared.KindCatalog, Is.SameAs(captured.KindInput.Catalog));
        }

        private static string CreateRuntime()
        {
            string root = Path.GetFullPath("Temp/Q07KindIdentity/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat/data"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            Directory.CreateDirectory(Path.Combine(root, "vfs"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,56,0,a.dat,missing\n",
                new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                "<bmp_begin>\nname: kind\n<bmp_end>\n<frame> 0 standing\nstate: 0 wait: 1 next: 0\n<frame_end>\n",
                new UTF8Encoding(false));
            return root;
        }
    }
}
#endif
