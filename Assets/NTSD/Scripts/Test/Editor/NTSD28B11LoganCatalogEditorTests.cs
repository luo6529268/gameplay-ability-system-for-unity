#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NTSD.Animation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28B11LoganCatalogEditorTests
    {
        private const string AuthorityRuntime = @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime";
        private const string Header = "registry_section,registry_index,id,type,source_path,published_folder";
        private const string Dat = "<bmp_begin>\nname: Test\nfile(20-19): c/test.png w: 2 h: 3 row: 2 col: 1\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n";
        private string root;
        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        [SetUp]
        public void CreateOwnedRuntime()
        {
            root = Path.Combine(ProjectRoot, "Temp/NTSD28Catalog/fixtures", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
        }

        private void WriteCatalog(string rows, string header = Header)
        {
            File.WriteAllText(Path.Combine(root, "catalog.csv"), header + "\r\n" + rows, new UTF8Encoding(true));
        }

        private void WriteDat(string relative, string text = Dat)
        {
            string path = Path.Combine(root, "decoded_dat", relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text, new UTF8Encoding(false));
        }

        private static object Read(string runtime)
        {
            return LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(runtime));
        }

        private static object Property(object value, string name) => value.GetType().GetProperty(name).GetValue(value);
        private static object[] Entries(object catalog) => ((IEnumerable)Property(catalog, "Entries")).Cast<object>().ToArray();

        private static Dictionary<int, LF2CharacterDataWrapper> Build(object catalog)
        {
            return CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog((LoganObjectCatalog)catalog);
        }

        [Serializable] private sealed class NativeEntry { public int registryIndex, id, type; public string sourcePath, publishedFolder, datPath; }
        [Serializable] private sealed class NativeResult { public bool success; public int objectRows, entriesLoaded; public NativeEntry[] entries; }

        private static NativeResult Probe(string runtime)
        {
            string executable = Path.Combine(ProjectRoot, "Temp/NTSD28Catalog/NativeCatalogProbe.exe");
            Assert.That(File.Exists(executable), Is.True, "Build Tools/NTSD28Catalog/Build-NativeCatalogProbe.ps1 first.");
            var start = new ProcessStartInfo(executable, "\"" + runtime + "\"")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using (var process = Process.Start(start))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                Assert.That(process.WaitForExit(30000), Is.True, "Native probe did not exit.");
                Assert.That(process.ExitCode, Is.Zero, error);
                return JsonUtility.FromJson<NativeResult>(output);
            }
        }

        [Test]
        public void NativeCsvQuotedBomAndRegistryOrder_MatchActualCatalog()
        {
            WriteDat("c,first.dat");
            WriteDat("second.dat");
            WriteCatalog("object,9,901,0,second.dat,missing2\r\nbackground,0,8,0,ignored,ignored\r\nobject,2,-7,-2,\"c,first.dat\",missing1\r\n");
            var native = Probe(root);
            Assert.That(native.success, Is.True);
            object catalog = Read(root);
            var entries = Entries(catalog);
            Assert.That(entries.Length, Is.EqualTo(2));
            for (int i = 0; i < entries.Length; i++)
            {
                Assert.That(Property(entries[i], "RegistryIndex"), Is.EqualTo(native.entries[i].registryIndex));
                Assert.That(Property(entries[i], "Id"), Is.EqualTo(native.entries[i].id));
                Assert.That(Property(entries[i], "Type"), Is.EqualTo(native.entries[i].type));
                Assert.That(Property(entries[i], "DatPath"), Is.EqualTo(Path.GetFullPath(native.entries[i].datPath)));
            }
        }

        [TestCase("object,0,1,0,a.dat,missing\nobject,1,1,0,a.dat,missing\n")]
        [TestCase("object,0,1,0,a.dat,missing\nobject,0,2,0,a.dat,missing\n")]
        [TestCase("object,+1,1,0,a.dat,missing\n")]
        [TestCase("object, 1,1,0,a.dat,missing\n")]
        [TestCase("object,-1,1,0,a.dat,missing\n")]
        [TestCase("object,0,1,0,a.dat,\n")]
        public void InvalidRequiredValues_RejectLikeNative(string rows)
        {
            WriteDat("a.dat");
            WriteCatalog(rows);
            Assert.That(Probe(root).success, Is.False);
            Assert.Throws<InvalidDataException>(() => Read(root));
        }

        [Test]
        public void MissingRequiredHeader_RejectsLikeNative()
        {
            WriteCatalog("object,0,1,0,a.dat\n", "registry_section,registry_index,id,type,source_path");
            Assert.That(Probe(root).success, Is.False);
            Assert.Throws<InvalidDataException>(() => Read(root));
        }

        [Test]
        public void PublishedFolderHasPriority_AndMultipleTopLevelDatRejects()
        {
            WriteDat("a.dat");
            WriteCatalog("object,0,1,0,a.dat,published\n");
            string published = Path.Combine(root, "published");
            Directory.CreateDirectory(published);
            File.WriteAllText(Path.Combine(published, "chosen.DAT"), Dat);
            var native = Probe(root);
            Assert.That(native.success, Is.True);
            Assert.That(Property(Entries(Read(root))[0], "DatPath"), Is.EqualTo(Path.GetFullPath(native.entries[0].datPath)));
            File.WriteAllText(Path.Combine(published, "second.dat"), Dat);
            Assert.That(Probe(root).success, Is.False);
            Assert.Throws<InvalidDataException>(() => Read(root));
        }

        [Test]
        public void MissingDatAndEscapingSource_AreRejected()
        {
            WriteCatalog("object,0,1,0,missing.dat,no-folder\n");
            Assert.That(Probe(root).success, Is.False);
            Assert.Throws<InvalidDataException>(() => Read(root));
            WriteCatalog("object,0,1,0,../outside.dat,no-folder\n");
            Assert.That(Probe(root).success, Is.False);
            Assert.Throws<InvalidDataException>(() => Read(root));
        }

        [Test]
        public void DefinitionIdentity_ChangesWithDat_AndRetainsCapturedText()
        {
            WriteDat("a.dat");
            WriteCatalog("object,0,1,0,a.dat,no-folder\n");
            object first = Read(root);
            string originalText = (string)Property(Entries(first)[0], "DatText");
            Assert.That(Property(first, "DefinitionFingerprint"), Is.EqualTo(Property(Read(root), "DefinitionFingerprint")));
            WriteDat("a.dat", Dat.Replace("wait: 1", "wait: 2"));
            object second = Read(root);
            Assert.That(Property(first, "DefinitionFingerprint"), Is.Not.EqualTo(Property(second, "DefinitionFingerprint")));
            Assert.That(Property(first, "SourceCacheKey"), Is.Not.EqualTo(Property(second, "SourceCacheKey")));
            Assert.That(Property(Entries(first)[0], "DatText"), Is.EqualTo(originalText));
        }

        [Test]
        public void IdenticalDefinitionsInDifferentRoots_DoNotShareCacheKey()
        {
            WriteDat("a.dat");
            WriteCatalog("object,0,1,0,a.dat,no-folder\n");
            object first = Read(root);
            root = Path.Combine(ProjectRoot, "Temp/NTSD28Catalog/fixtures", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            WriteDat("a.dat");
            WriteCatalog("object,0,1,0,a.dat,no-folder\n");
            object second = Read(root);
            Assert.That(Property(first, "DefinitionFingerprint"), Is.EqualTo(Property(second, "DefinitionFingerprint")));
            Assert.That(Property(first, "SourceCacheKey"), Is.Not.EqualTo(Property(second, "SourceCacheKey")));
            Assert.That(((IList)Property(second, "Entries")).IsReadOnly, Is.True);
        }

        [Test]
        public void ValidCandidate_IsBuiltAsOneSource_WithNativeRanges()
        {
            WriteDat("a.dat");
            WriteDat("b.dat");
            WriteCatalog("object,5,19,0,a.dat,no-folder\nobject,0,23,3,b.dat,no-folder2\n");
            var configs = Build(Read(root));
            Assert.That(configs.Count, Is.EqualTo(2));
            Assert.That(configs[19].characterData.type_sub, Is.EqualTo(19));
            Assert.That(configs[23].characterData.files[0].startFrame, Is.Zero);
            Assert.That(configs[19].characterData.files[0].filePath, Does.StartWith(Path.GetFullPath(Path.Combine(root, "vfs"))));
        }

        [Test]
        public void Formal330Catalog_MatchesFreshNativeRuntimeLoad()
        {
            var native = Probe(AuthorityRuntime);
            Assert.That(native.success, Is.True);
            Assert.That(native.entriesLoaded, Is.EqualTo(330));
            var entries = Entries(Read(AuthorityRuntime));
            Assert.That(entries.Length, Is.EqualTo(330));
            for (int i = 0; i < entries.Length; i++)
            {
                var e = native.entries[i];
                Assert.That(Property(entries[i], "RegistryIndex"), Is.EqualTo(e.registryIndex));
                Assert.That(Property(entries[i], "Id"), Is.EqualTo(e.id));
                Assert.That(Property(entries[i], "Type"), Is.EqualTo(e.type));
                Assert.That(Property(entries[i], "SourcePath"), Is.EqualTo(e.sourcePath));
                Assert.That(Property(entries[i], "PublishedFolder"), Is.EqualTo(e.publishedFolder));
                Assert.That(Property(entries[i], "DatPath"), Is.EqualTo(Path.GetFullPath(e.datPath)));
            }
        }

        [Test]
        public void FormalCandidate_BuildsAll330DefinitionsWithNativeFrames()
        {
            var catalog = Read(AuthorityRuntime);
            var configs = Build(catalog);
            Assert.That(configs.Count, Is.EqualTo(330));
            foreach (object entry in Entries(catalog))
            {
                int id = (int)Property(entry, "Id");
                Assert.That(configs.ContainsKey(id), Is.True, "object " + id);
                Assert.That(configs[id].characterData, Is.Not.Null);
                Assert.That(configs[id].characterData.frames.All(frame => frame.UsesLoganFrameNumbers), Is.True, "object " + id);
            }
        }

        [Test]
        public void InvalidNativeFrame_PreventsTheWholeCandidateFromReturning()
        {
            WriteDat("a.dat");
            WriteDat("b.dat", Dat + "<frame> 0 duplicate\n<frame_end>\n");
            WriteCatalog("object,0,19,0,a.dat,one\nobject,1,23,0,b.dat,two\n");
            var error = Assert.Throws<AggregateException>(() => Build(Read(root)));
            Assert.That(error.InnerExceptions.Count, Is.EqualTo(1));
            Assert.That(error.InnerExceptions[0].Message, Does.Contain("b.dat"));
        }
    }
}
#endif
