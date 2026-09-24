using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class CharacterAssetDeploymentEditorTests
    {
        private const string FormalRoot = @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime";

        [Test]
        [Category("CharacterAssetDeployment")]
        public void FormalTypeZeroCharacterDatAndImagesAreDeployed()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime"));
            Assert.That(Directory.Exists(FormalRoot), Is.True, $"formal runtime missing: {FormalRoot}");
            Assert.That(Directory.Exists(stagedRoot), Is.True, $"staged runtime missing: {stagedRoot}");

            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            LoganObjectCatalog formal = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(FormalRoot), mode);
            LoganObjectCatalog staged = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(stagedRoot), mode);
            LoganObjectCatalog.Entry[] formalCharacters = formal.Entries.Where(entry => entry.Type == 0).ToArray();
            Dictionary<int, LoganObjectCatalog.Entry> stagedCharacters = staged.Entries
                .Where(entry => entry.Type == 0).ToDictionary(entry => entry.Id);
            Assert.That(formalCharacters.Length, Is.EqualTo(158), "formal catalog type-0 count changed");
            Assert.That(stagedCharacters.Count, Is.EqualTo(formalCharacters.Length), "staged type-0 catalog is incomplete");

            Dictionary<int, LF2CharacterDataWrapper> formalConfigs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(formal);
            Dictionary<int, LF2CharacterDataWrapper> stagedConfigs =
                CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(staged);
            var imageHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int imageReferenceCount = 0;
            foreach (LoganObjectCatalog.Entry entry in formalCharacters)
            {
                Assert.That(stagedCharacters.TryGetValue(entry.Id, out LoganObjectCatalog.Entry deployed), Is.True,
                    $"type:0 oid {entry.Id} missing from staged catalog");
                Assert.That(deployed.RegistryIndex, Is.EqualTo(entry.RegistryIndex), $"oid {entry.Id} registry index");
                Assert.That(deployed.SourcePath, Is.EqualTo(entry.SourcePath), $"oid {entry.Id} source path");
                Assert.That(deployed.PublishedFolder, Is.EqualTo(entry.PublishedFolder), $"oid {entry.Id} folder");
                Assert.That(deployed.DatSha256, Is.EqualTo(entry.DatSha256), $"oid {entry.Id} DAT bytes");

                Assert.That(formalConfigs.TryGetValue(entry.Id, out LF2CharacterDataWrapper formalConfig), Is.True);
                Assert.That(stagedConfigs.TryGetValue(entry.Id, out LF2CharacterDataWrapper stagedConfig), Is.True);
                Assert.That(stagedConfig.characterData.frames.Count,
                    Is.EqualTo(formalConfig.characterData.frames.Count), $"oid {entry.Id} parsed frame count");

                string[] formalImages = DeclaredImages(formalConfig, formal.Source);
                string[] stagedImages = DeclaredImages(stagedConfig, staged.Source);
                Assert.That(stagedImages, Is.EqualTo(formalImages), $"oid {entry.Id} declared image paths");
                foreach (string relativeImage in formalImages)
                {
                    imageReferenceCount++;
                    string formalImage = formal.Source.ResolveImagePath(relativeImage, null);
                    string stagedImage = staged.Source.ResolveImagePath(relativeImage, null);
                    Assert.That(File.Exists(formalImage), Is.True, $"oid {entry.Id} formal image: {relativeImage}");
                    Assert.That(File.Exists(stagedImage), Is.True, $"oid {entry.Id} staged image: {relativeImage}");
                    Assert.That(FileHash(stagedImage, imageHashes),
                        Is.EqualTo(FileHash(formalImage, imageHashes)),
                        $"oid {entry.Id} image bytes: {relativeImage}");
                }
            }
            Assert.That(imageReferenceCount, Is.GreaterThan(0));
        }

        [MenuItem("Tools/NTSD/Tests/Verify Formal Type0 Character Asset Deployment")]
        private static void VerifyFormalTypeZeroCharacterAssetDeployment()
        {
            new CharacterAssetDeploymentEditorTests()
                .FormalTypeZeroCharacterDatAndImagesAreDeployed();
            Debug.Log("[CharacterAssetDeployment] formal type:0 DAT/image deployment contract passed.");
        }

        private static string[] DeclaredImages(LF2CharacterDataWrapper wrapper, BattleContentSource source)
        {
            var paths = new List<string>();
            foreach (SpriteFileInfo file in wrapper.characterData.files)
                paths.Add(file.filePath);
            if (!string.IsNullOrEmpty(wrapper.characterData.head))
                paths.Add(wrapper.characterData.head);
            if (!string.IsNullOrEmpty(wrapper.characterData.small))
                paths.Add(wrapper.characterData.small);
            return paths.Select(path => Path.GetRelativePath(source.ImageRoot, path).Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.Ordinal).ToArray();
        }

        private static string FileHash(string path, Dictionary<string, string> cache)
        {
            if (cache.TryGetValue(path, out string hash))
                return hash;
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
            cache.Add(path, hash);
            return hash;
        }
    }
}
