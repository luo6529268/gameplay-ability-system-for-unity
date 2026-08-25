using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NTSD.DatParser;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class CharacterAssetDeploymentEditorTests
    {
        private const string DatPassword = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";
        private const int ExpectedTypeZeroCharacterCount = 42;

        [Test]
        [Category("CharacterAssetDeployment")]
        public void TypeZeroCharacterCatalogDecryptsParsesAndResolvesDeclaredBitmaps()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            Assert.That(projectRoot, Is.Not.Null.And.Not.Empty);

            string dataPath = Path.Combine(projectRoot, "Assets", "NTSD", "Config", "data.txt");
            Assert.That(File.Exists(dataPath), Is.True, $"data.txt missing: {dataPath}");

            var typeZeroEntries = ReadTypeZeroEntries(dataPath);
            Assert.That(typeZeroEntries.Count, Is.EqualTo(ExpectedTypeZeroCharacterCount));

            int bitmapReferenceCount = 0;
            foreach (KeyValuePair<int, string> entry in typeZeroEntries)
            {
                Assert.That(
                    entry.Value.StartsWith("Assets/NTSD/Config/Character/", StringComparison.OrdinalIgnoreCase),
                    Is.True,
                    $"type:0 oid {entry.Key} must resolve through the Character DAT catalog: {entry.Value}");

                string datPath = Path.Combine(projectRoot, entry.Value.Replace('/', Path.DirectorySeparatorChar));
                Assert.That(File.Exists(datPath), Is.True, $"type:0 oid {entry.Key} DAT missing: {entry.Value}");

                string datText = Lf2DatDecryptor.DecryptFile(datPath, DatPassword);
                Assert.That(datText, Is.Not.Null.And.Not.Empty, $"type:0 oid {entry.Key} DAT decrypt returned empty");

                Lf2DatFile datFile = new Lf2DatParserV2().Parse(datText, datPath);
                Assert.That(datFile, Is.Not.Null, $"type:0 oid {entry.Key} DAT parse returned null");
                Assert.That(datFile.Frames, Is.Not.Null.And.Not.Empty, $"type:0 oid {entry.Key} DAT has no frames");

                foreach (string bitmapPath in ReadBitmapPaths(datText, entry.Key))
                {
                    bitmapReferenceCount++;
                    Assert.That(
                        bitmapPath.StartsWith("Assets/NTSD/Sprite/Character/", StringComparison.OrdinalIgnoreCase),
                        Is.True,
                        $"type:0 oid {entry.Key} bitmap must use the Character sprite catalog: {bitmapPath}");

                    string absoluteBitmapPath = Path.Combine(
                        projectRoot,
                        bitmapPath.Replace('/', Path.DirectorySeparatorChar));
                    Assert.That(
                        File.Exists(absoluteBitmapPath),
                        Is.True,
                        $"type:0 oid {entry.Key} bitmap missing: {bitmapPath}");
                }
            }

            Assert.That(bitmapReferenceCount, Is.GreaterThan(0));
        }

        [MenuItem("Tools/NTSD/Tests/Verify Type0 Character Asset Deployment")]
        private static void VerifyTypeZeroCharacterAssetDeployment()
        {
            new CharacterAssetDeploymentEditorTests()
                .TypeZeroCharacterCatalogDecryptsParsesAndResolvesDeclaredBitmaps();
            Debug.Log("[CharacterAssetDeployment] type:0 DAT/BMP deployment contract passed.");
        }

        private static List<KeyValuePair<int, string>> ReadTypeZeroEntries(string dataPath)
        {
            var entries = new List<KeyValuePair<int, string>>();
            foreach (string line in File.ReadLines(dataPath))
            {
                Match match = Regex.Match(
                    line,
                    @"^\s*id:\s*(?<id>\d+)\s+type:\s*0\s+file:\s*(?<path>\S+\.dat)",
                    RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    entries.Add(new KeyValuePair<int, string>(
                        int.Parse(match.Groups["id"].Value),
                        match.Groups["path"].Value));
                }
            }

            return entries;
        }

        private static IEnumerable<string> ReadBitmapPaths(string datText, int oid)
        {
            Match bitmapBlock = Regex.Match(
                datText,
                @"<bmp_begin>(?<content>.*?)<bmp_end>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Assert.That(bitmapBlock.Success, Is.True, $"type:0 oid {oid} has no bmp block");

            foreach (string line in bitmapBlock.Groups["content"].Value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                Match headOrSmall = Regex.Match(
                    line,
                    @"^\s*(?:head|small)\s*:\s*(?<path>\S+\.bmp)\s*$",
                    RegexOptions.IgnoreCase);
                if (headOrSmall.Success)
                {
                    yield return headOrSmall.Groups["path"].Value;
                    continue;
                }

                Match frameSheet = Regex.Match(
                    line,
                    @"^\s*file\([^)]*\)\s*:\s*(?<path>\S+\.bmp)\b",
                    RegexOptions.IgnoreCase);
                if (frameSheet.Success)
                    yield return frameSheet.Groups["path"].Value;
            }
        }
    }
}
