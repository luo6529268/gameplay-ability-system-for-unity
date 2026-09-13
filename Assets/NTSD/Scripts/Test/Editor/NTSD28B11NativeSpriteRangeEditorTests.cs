#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.DatParser;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28B11NativeSpriteRangeEditorTests
    {
        private const string RuntimeRoot = @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime";
        private const string Sheets = "<bmp_begin>\nfile(20-19): first.png w: 2 h: 3 row: 2 col: 1\nfile(900-900): second.png w: 2 h: 3 row: 1 col: 2\n<bmp_end>";

        private static Lf2DatFile ParseNative(string text)
        {
            MethodInfo method = typeof(Lf2DatParserV2).GetMethod("ParseLoganContent");
            Assert.That(method, Is.Not.Null, "Explicit native parser admission must exist.");
            return (Lf2DatFile)method.Invoke(new Lf2DatParserV2(), new object[] { text, null });
        }

        private static List<SpriteFileInfo> Project(Lf2DatFile dat, BattleContentSource source)
        {
            MethodInfo method = typeof(CharacterAnimtorManager).GetMethod("BuildSpriteFilesForSource", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Source-specific range projection must exist.");
            return (List<SpriteFileInfo>)method.Invoke(null, new object[] { dat, Application.dataPath, source });
        }

        private static LF2CharacterData Build(string text, string path, BattleContentSource source)
        {
            MethodInfo method = typeof(CharacterAnimtorManager).GetMethod("BuildCharacterDataFromSource", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Native parser and builder must be paired.");
            return (LF2CharacterData)method.Invoke(null, new object[] { text, path, source });
        }

        [Test]
        public void NativeRanges_PreserveDeclarations_AndAccumulateCapacity()
        {
            Lf2DatFile dat = ParseNative(Sheets);
            var files = Project(dat, BattleContentSource.ForLoganRuntime(RuntimeRoot));
            Assert.That(dat.Bmp.Files.Count, Is.EqualTo(2));
            Assert.That(dat.Bmp.Files[0].StartIndex, Is.EqualTo(20));
            Assert.That(dat.Bmp.Files[0].EndIndex, Is.EqualTo(19));
            Assert.That(dat.Bmp.Files[1].StartIndex, Is.EqualTo(900));
            Assert.That(new[] { files[0].startFrame, files[0].endFrame, files[1].startFrame, files[1].endFrame }, Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void LegacyRangeAndReflectionEntry_ArePreserved()
        {
            var dat = new Lf2DatParserV2().Parse(Sheets);
            Assert.That(dat.Bmp.Files.Count, Is.EqualTo(1));
            var files = Project(dat, null);
            Assert.That(files[0].startFrame, Is.EqualTo(900));
            Assert.That(files[0].endFrame, Is.EqualTo(900));
            Assert.That(typeof(CharacterAnimtorManager).GetMethod("BuildCharacterDataFromDat", BindingFlags.Instance | BindingFlags.NonPublic), Is.Not.Null);
        }

        [Test]
        public void SignedEnd_ZeroCapacity_AndNegativeProduct_KeepNativeOffsets()
        {
            var dat = ParseNative("<bmp_begin>\nfile(0--1): zero.png row: 0 col: 7\nfile(2-1): neg.png row: -2 col: 3\nfile(7-6): pos.png row: -2 col: -3\n<bmp_end>");
            var files = Project(dat, BattleContentSource.ForLoganRuntime(RuntimeRoot));
            Assert.That(files.Count, Is.EqualTo(3));
            Assert.That(new[] { files[0].startFrame, files[0].endFrame, files[1].startFrame, files[1].endFrame, files[2].startFrame, files[2].endFrame }, Is.EqualTo(new[] { 0, -1, 0, -1, 0, 5 }));
        }

        [Test]
        public void MalformedNativeDeclarations_AreRejectedWithoutInventingSheets()
        {
            var dat = ParseNative("<bmp_begin>\nfile(2-x): bad.png row: 1 col: 1\nfile(-2-3): negative.png row: 1 col: 1\nfile(2--x): invalid.png row: 1 col: 1\nfile(3): valid.png row: 1 col: 1\n<bmp_end>");
            Assert.That(dat.Bmp.Files.Count, Is.EqualTo(1));
            Assert.That(dat.Bmp.Files[0].StartIndex, Is.EqualTo(3));
        }

        [Test]
        public void Admission_ResolvesNativeHeads_WithoutChangingLegacyDeclaredRanges()
        {
            const string text = "<bmp_begin>\nhead: c/head.png\nsmall: c/small.png\nfile(30-31): c/body.png w: 2 h: 3 row: 2 col: 1\n<bmp_end>";
            var nativeSource = BattleContentSource.ForLoganRuntime(RuntimeRoot);
            var native = Build(text, nativeSource.ResolveDatPath("c/body.dat"), nativeSource);
            Assert.That(native.head, Is.EqualTo(nativeSource.ResolveImagePath("c/head.png", null)));
            Assert.That(native.small, Is.EqualTo(nativeSource.ResolveImagePath("c/small.png", null)));
            var legacySource = BattleContentSource.ForUnityProject(Directory.GetParent(Application.dataPath).FullName);
            var legacy = Build(text, legacySource.ResolveDatPath("body.dat"), legacySource);
            Assert.That(legacy.files[0].startFrame, Is.EqualTo(30));
            Assert.That(legacy.files[0].endFrame, Is.EqualTo(31));
        }

        [Test]
        public void OverflowingCapacity_IsRejected()
        {
            var dat = ParseNative("<bmp_begin>\nfile(0-1): huge.png row: 2147483647 col: 2\n<bmp_end>");
            Assert.Throws<OverflowException>(() => CharacterAnimtorManager.BuildSpriteFilesForSource(
                dat, Application.dataPath, BattleContentSource.ForLoganRuntime(RuntimeRoot)));
        }

        [Test]
        public void NativeAdmission_FeedsActualCatalogWithNativeSheetAndRect()
        {
            var source = BattleContentSource.ForLoganRuntime(RuntimeRoot);
            var data = Build(Sheets, Path.Combine(RuntimeRoot, "decoded_dat", "fixture.dat"), source);
            var first = new Texture2D(6, 4);
            var second = new Texture2D(3, 8);
            Sprite a = null;
            Sprite b = null;
            try
            {
                a = Sprite.Create(first, new Rect(0, 0, 6, 4), Vector2.zero);
                b = Sprite.Create(second, new Rect(0, 0, 3, 8), Vector2.zero);
                var catalog = CharacterAnimtorManager.BuildBattleSpriteCatalog(
                    new Dictionary<int, LF2CharacterDataWrapper> { [56] = new LF2CharacterDataWrapper(56, data) },
                    new Dictionary<int, List<Sprite>> { [56] = new List<Sprite> { a, a, b, b } });
                Assert.That(catalog.Count, Is.EqualTo(4));
                Assert.That(catalog.TryGet(56, 0, out BattleSpriteEntry entry0), Is.True);
                Assert.That(catalog.TryGet(56, 2, out BattleSpriteEntry entry2), Is.True);
                Assert.That(entry0.SharedTexture, Is.SameAs(first));
                Assert.That(entry2.SharedTexture, Is.SameAs(second));
                Assert.That(entry0.PixelRect, Is.EqualTo(new Rect(0, 1, 2, 3)));
                Assert.That(entry2.PixelRect, Is.EqualTo(new Rect(0, 5, 2, 3)));
                Assert.That(entry2.SourceSheetPath, Is.EqualTo(source.ResolveImagePath("second.png", Application.dataPath)));
                Assert.That(catalog.TryGet(56, 900, out _), Is.False);
            }
            finally
            {
                if (a != null) UnityEngine.Object.DestroyImmediate(a);
                if (b != null) UnityEngine.Object.DestroyImmediate(b);
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(second);
            }
        }

        [TestCase("c/jug/jugo.dat")]
        [TestCase("c/jug/jugoCS2.dat")]
        [TestCase("c/shis/shis.dat")]
        [TestCase("a/tra/tra.dat")]
        public void FormalDat_UsesPairedNativeAdmission(string relative)
        {
            var source = BattleContentSource.ForLoganRuntime(RuntimeRoot);
            string path = source.ResolveDatPath(relative);
            Assert.That(File.Exists(path), Is.True, path);
            var data = Build(File.ReadAllText(path), path, source);
            Assert.That(data.files.Count, Is.GreaterThan(0));
            Assert.That(data.files[0].startFrame, Is.Zero);
            for (int i = 1; i < data.files.Count; i++)
                Assert.That(data.files[i].startFrame, Is.EqualTo(data.files[i - 1].endFrame + 1));
        }

        [Serializable] private sealed class NativeDocument { public string path; public bool parseSuccess; public NativeSprite[] sprites; }
        [Serializable] private sealed class NativeSprite
        {
            public string path;
            public int width, height, row, col, declaredFirst, declaredLast, effectiveFirst, effectiveLast;
        }

        [Test]
        public void AllFormalDatSheets_MatchFrozenNativeSourceCapture()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string capture = Path.Combine(root, "artifacts/diagnostics/NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content.jsonl");
            using (var hash = SHA256.Create())
            using (var stream = File.OpenRead(capture))
                Assert.That(BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", ""), Is.EqualTo("5F5C6C34CE907BB6D7855B8A1E69D410B3CD577807F202332B8A0DA30ED0146F"), "Frozen native capture identity");
            var source = BattleContentSource.ForLoganRuntime(RuntimeRoot);
            int documents = 0;
            int sheets = 0;
            int rejected = 0;
            foreach (string line in File.ReadLines(capture))
            {
                var expected = JsonUtility.FromJson<NativeDocument>(line);
                string text = File.ReadAllText(source.ResolveDatPath(expected.path));
                documents++;
                if (!expected.parseSuccess)
                {
                    var exception = Assert.Throws<TargetInvocationException>(() => ParseNative(text), expected.path);
                    Assert.That(exception.InnerException, Is.TypeOf<FormatException>(), expected.path);
                    rejected++;
                    continue;
                }
                var dat = ParseNative(text);
                var actual = Project(dat, source);
                Assert.That(actual.Count, Is.EqualTo(expected.sprites.Length), expected.path);
                for (int i = 0; i < actual.Count; i++)
                {
                    var a = actual[i];
                    var e = expected.sprites[i];
                    Assert.That(new[] { a.startFrame, a.endFrame, a.width, a.height, a.row, a.col, dat.Bmp.Files[i].StartIndex, dat.Bmp.Files[i].EndIndex },
                        Is.EqualTo(new[] { e.effectiveFirst, e.effectiveLast, e.width, e.height, e.row, e.col, e.declaredFirst, e.declaredLast }), expected.path + " sheet " + i);
                    Assert.That(a.filePath, Is.EqualTo(source.ResolveImagePath(e.path, Application.dataPath)), expected.path);
                    sheets++;
                }
            }
            Assert.That(documents, Is.EqualTo(405));
            Assert.That(rejected, Is.EqualTo(3));
            TestContext.WriteLine("Native projection matched: " + documents + " DAT / " + sheets + " sheets.");
        }
    }
}
#endif
