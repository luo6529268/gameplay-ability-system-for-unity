#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_Q07")]
    public sealed class NTSD28Q07StagedCandidateIdentityEditorTests
    {
        private const string FormalRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void StagedCandidateHasSameObjectDatAndImageBytesModuloCrLf()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime"));
            Assert.That(Directory.Exists(stagedRoot), Is.True);
            Assert.That(Directory.Exists(FormalRoot), Is.True);

            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            LoganVisualContentCandidate formal = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(FormalRoot), mode);
            LoganVisualContentCandidate staged = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(stagedRoot), mode);

            Assert.That(formal.Catalog.Entries.Count, Is.EqualTo(330));
            Assert.That(staged.Catalog.Entries.Count, Is.EqualTo(330));
            // Native HUD smallb images are deployed but excluded from this production candidate.
            Assert.That(formal.Images.Count, Is.EqualTo(906));
            Assert.That(staged.Images.Count, Is.EqualTo(formal.Images.Count));
            Assert.That(staged.Catalog.FusionInput.UsesLockedFallback,
                Is.EqualTo(formal.Catalog.FusionInput.UsesLockedFallback));
            if (!formal.Catalog.FusionInput.UsesLockedFallback)
            {
                Assert.That(NormalizeCrLf(File.ReadAllBytes(staged.Catalog.FusionInput.SelectedPath)),
                    Is.EqualTo(NormalizeCrLf(File.ReadAllBytes(formal.Catalog.FusionInput.SelectedPath))),
                    "Fusion DAT content differs beyond CRLF");
            }
            Assert.That(staged.Catalog.FusionInput.SemanticFingerprint, Is.EqualTo(formal.Catalog.FusionInput.SemanticFingerprint));
            Assert.That(staged.ContentIdentity.DecodeContractTag, Is.EqualTo(formal.ContentIdentity.DecodeContractTag));
            for (int index = 0; index < formal.Catalog.Entries.Count; index++)
            {
                LoganObjectCatalog.Entry expected = formal.Catalog.Entries[index];
                LoganObjectCatalog.Entry actual = staged.Catalog.Entries[index];
                Assert.That(actual.RegistryIndex, Is.EqualTo(expected.RegistryIndex));
                Assert.That(actual.Id, Is.EqualTo(expected.Id));
                Assert.That(actual.Type, Is.EqualTo(expected.Type));
                Assert.That(actual.SourcePath, Is.EqualTo(expected.SourcePath));
                Assert.That(NormalizeCrLf(File.ReadAllBytes(actual.DatPath)),
                    Is.EqualTo(NormalizeCrLf(File.ReadAllBytes(expected.DatPath))),
                    "DAT content differs beyond CRLF for OID " + expected.Id);
            }
            for (int index = 0; index < formal.Images.Count; index++)
            {
                LoganVisualContentCandidate.ImageInput expected = formal.Images[index];
                LoganVisualContentCandidate.ImageInput actual = staged.Images[index];
                string expectedPath = Path.GetRelativePath(formal.Catalog.Source.ImageRoot,
                    expected.Path).Replace('\\', '/');
                string actualPath = Path.GetRelativePath(staged.Catalog.Source.ImageRoot,
                    actual.Path).Replace('\\', '/');
                Assert.That(actualPath, Is.EqualTo(expectedPath));
                Assert.That(actual.Sha256, Is.EqualTo(expected.Sha256),
                    "Image content differs for " + expectedPath);
            }
            Assert.That(staged.SourceCacheKey, Is.Not.EqualTo(formal.SourceCacheKey));
            Assert.DoesNotThrow(() => formal.AssertInputsCurrent());
            Assert.DoesNotThrow(() => staged.AssertInputsCurrent());
        }

        private static byte[] NormalizeCrLf(byte[] source)
        {
            using (var normalized = new MemoryStream(source.Length))
            {
                for (int index = 0; index < source.Length; index++)
                {
                    if (source[index] == '\r' && index + 1 < source.Length &&
                        source[index + 1] == '\n') continue;
                    normalized.WriteByte(source[index]);
                }
                return normalized.ToArray();
            }
        }
    }
}
#endif
