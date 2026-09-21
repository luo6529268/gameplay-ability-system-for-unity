#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q06FusionPreparedWorldCatalogEditorTests
    {
        private static LoganObjectCatalog Input()
        {
            string root = Path.GetFullPath("Temp/Q06PreparedFusion/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat/data"));
            Directory.CreateDirectory(Path.Combine(root, "vfs"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,7,0,a.dat,missing\n", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"), "<bmp_begin>\nname: prepared\n<bmp_end>\n", new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "decoded_dat/data/fusion.dat"),
                "<fusion_begin>\nfusion: 1\nid1: 7\nid2: 8\nid3: 51\nhp: 177\nfusion_end:\n<fusion_end>\n", new UTF8Encoding(false));
            return LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(root));
        }

        private static object Read(BattleRuntimeDataCatalog target, string name)
        {
            var property = target.GetType().GetProperty(name);
            Assert.That(property, Is.Not.Null, "Missing prepared fusion property: " + name);
            return property.GetValue(target);
        }

        private static void Prepare(BattleRuntimeDataCatalog target, LoganObjectCatalog input, bool mismatch = false,
            Func<int, LF2CharacterDataWrapper> resolver = null)
        {
            var method = typeof(BattleRuntimeDataCatalog).GetMethod("Prepare");
            Assert.That(method.GetParameters().Length, Is.EqualTo(4), "Missing captured Logan catalog preparation argument");
            var definition = new ObjectDefinition(mismatch ? 8 : 7, 0, "a.dat");
            Func<int, LF2CharacterDataWrapper> resolve = resolver ?? (id => new LF2CharacterDataWrapper(id, new LF2CharacterData()));
            try { method.Invoke(target, new object[] { new[] { definition }, resolve, default(BattleHitRecordLifecycleCatalog), input }); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        [Test]
        public void PreparedWorldOwnsCapturedTableAndCompositeIdentityWithoutLaterFileReads()
        {
            var input = Input();
            var catalog = new BattleRuntimeDataCatalog();
            Prepare(catalog, input);
            catalog.Seal();
            Assert.That(Read(catalog, "FusionCatalog"), Is.SameAs(input.FusionInput.Catalog));
            Assert.That(Read(catalog, "LoganContentIdentity"), Is.SameAs(input.ContentIdentity));
            File.WriteAllText(input.FusionInput.SelectedPath, "invalid replacement");
            Assert.That(((LoganFusionCatalog)Read(catalog, "FusionCatalog")).Records.Single().Hp, Is.EqualTo(177));
            Assert.That(catalog.IsSealedForBattle, Is.True);
        }

        [Test]
        public void SealedReplacementRejectsBeforeChangingFrozenReferences()
        {
            var input = Input();
            var catalog = new BattleRuntimeDataCatalog();
            Prepare(catalog, input);
            catalog.Seal();
            Assert.Throws<InvalidOperationException>(() => Prepare(catalog, null));
            Assert.That(Read(catalog, "LoganContentIdentity"), Is.SameAs(input.ContentIdentity));
        }

        [Test]
        public void ExplicitLegacyPreparationClearsPreviousLoganTableAndIdentity()
        {
            var catalog = new BattleRuntimeDataCatalog();
            Prepare(catalog, Input());
            catalog.Seal();
            catalog.Unseal();
            Prepare(catalog, null);
            Assert.That(Read(catalog, "FusionCatalog"), Is.Null);
            Assert.That(Read(catalog, "LoganContentIdentity"), Is.Null);
            Assert.That(catalog.IsReady, Is.True);
        }

        [Test]
        public void WrongObjectCatalogRejectsWithoutMutatingPreviouslyPreparedState()
        {
            var input = Input();
            var catalog = new BattleRuntimeDataCatalog();
            Prepare(catalog, input);
            int generation = catalog.Generation;
            object definition = catalog.GetObjectDefinition(7);
            Assert.Throws<ArgumentException>(() => Prepare(catalog, input, true));
            Assert.That(catalog.Generation, Is.EqualTo(generation));
            Assert.That(catalog.GetObjectDefinition(7), Is.SameAs(definition));
            Assert.That(Read(catalog, "LoganContentIdentity"), Is.SameAs(input.ContentIdentity));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void InvalidResolverLeavesPreviousCatalogUntouched(bool throws)
        {
            var input = Input();
            var catalog = new BattleRuntimeDataCatalog();
            Prepare(catalog, input);
            var wrapper = catalog.GetCharacterConfig(7);
            int generation = catalog.Generation;
            if (throws)
                Assert.Throws<IOException>(() => Prepare(catalog, input, resolver: _ => throw new IOException("fixture")));
            else
                Assert.Throws<ArgumentException>(() => Prepare(catalog, input,
                    resolver: _ => new LF2CharacterDataWrapper(8, new LF2CharacterData())));
            Assert.That(catalog.GetCharacterConfig(7), Is.SameAs(wrapper));
            Assert.That(catalog.Generation, Is.EqualTo(generation));
            Assert.That(Read(catalog, "FusionCatalog"), Is.SameAs(input.FusionInput.Catalog));
        }

        [Test]
        public void WorldPreparationForwardsSameFrozenCatalogAndSealsIt()
        {
            var input = Input();
            var world = new SimulationWorld();
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(7, 0, "a.dat") },
                id => new LF2CharacterDataWrapper(id, new LF2CharacterData()), loganCatalog: input);
            Assert.That(world.RuntimeDataCatalog.FusionCatalog, Is.SameAs(input.FusionInput.Catalog));
            Assert.That(world.RuntimeDataCatalog.LoganContentIdentity, Is.SameAs(input.ContentIdentity));
            Assert.That(world.RuntimeDataCatalog.IsSealedForBattle, Is.True);
        }
    }
}
#endif
