#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation;
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
        public void StagedCandidateMatchesFormalRuntime()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime"));
            Assert.That(Directory.Exists(stagedRoot), Is.True);
            Assert.That(Directory.Exists(FormalRoot), Is.True);

            LoganVisualContentCandidate formal = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(FormalRoot));
            LoganVisualContentCandidate staged = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(stagedRoot));

            Assert.That(formal.Catalog.Entries.Count, Is.EqualTo(330));
            Assert.That(staged.Catalog.Entries.Count, Is.EqualTo(330));
            // Q01 indexed 1,010 raw PNG references; the current parsed candidate selects 906.
            Assert.That(formal.Images.Count, Is.EqualTo(906));
            Assert.That(staged.Images.Count, Is.EqualTo(formal.Images.Count));
            Assert.That(staged.Catalog.DefinitionFingerprint, Is.EqualTo(formal.Catalog.DefinitionFingerprint));
            Assert.That(staged.Catalog.FusionInput.InputFingerprint, Is.EqualTo(formal.Catalog.FusionInput.InputFingerprint));
            Assert.That(staged.Catalog.FusionInput.SemanticFingerprint, Is.EqualTo(formal.Catalog.FusionInput.SemanticFingerprint));
            Assert.That(staged.ContentIdentity.RawDefinitionFingerprint, Is.EqualTo(formal.ContentIdentity.RawDefinitionFingerprint));
            Assert.That(staged.ContentIdentity.SemanticFingerprint, Is.EqualTo(formal.ContentIdentity.SemanticFingerprint));
            Assert.That(staged.VisualFingerprint, Is.EqualTo(formal.VisualFingerprint));
            Assert.That(staged.SourceCacheKey, Is.Not.EqualTo(formal.SourceCacheKey));
            Assert.DoesNotThrow(() => formal.AssertInputsCurrent());
            Assert.DoesNotThrow(() => staged.AssertInputsCurrent());
        }
    }
}
#endif
