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
    [Category("NTSD28_Q09")]
    public sealed class NTSD28Q09KillIconInputIdentityEditorTests
    {
        private const string FormalRoot =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void FormalAndStagedCandidateCaptureSameKillIcons()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            var formal = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(FormalRoot), mode);
            var staged = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(stagedRoot), mode);

            Assert.That(formal.KillIconInput, Is.Not.Null);
            Assert.That(staged.KillIconInput, Is.Not.Null);
            Assert.That(staged.KillIconInput.InputFingerprint,
                Is.EqualTo(formal.KillIconInput.InputFingerprint));
            Assert.That(staged.VisualFingerprint, Is.EqualTo(formal.VisualFingerprint));
            Assert.That(staged.Images.Count, Is.EqualTo(906));
            Assert.That(staged.KillIconInput.Images.Count, Is.EqualTo(7));
            for (int type = 0; type < 7; type++)
                Assert.That(staged.KillIconInput.Images[type], Is.Not.Null);
            Assert.DoesNotThrow(staged.AssertInputsCurrent);
        }

        [Test]
        public void NativeKillIconInputTracksSelectedBytesPresenceAndContainment()
        {
            string root = Path.Combine(Path.GetTempPath(),
                "NTSD28-Q09-KillIcon-" + Guid.NewGuid().ToString("N"));
            try
            {
                BattleContentSource source = BattleContentSource.ForLoganRuntime(root);
                Assert.That(LoganVisualContentCandidate.NativeKillIconInput.Capture(
                    source, null), Is.Null);
                var feed = LoganModeKnockoutFeedInput.Parse(
                    "<bmp_begin> pic_type0: sprite/kill/c.png " +
                    "pic_type1: sprite/kill/c.png " +
                    "pic_type2: sprite/kill/sk1.png <bmp_end>");
                string directory = Path.Combine(source.ImageRoot, "sprite", "kill");
                Directory.CreateDirectory(directory);
                string common = Path.Combine(directory, "c.png");
                string missing = Path.Combine(directory, "sk1.png");
                File.WriteAllBytes(common, new byte[] { 1 });

                var first = LoganVisualContentCandidate.NativeKillIconInput.Capture(source, feed);
                Assert.That(first.Images.Count, Is.EqualTo(7));
                Assert.That(first.Images[0].Sha256, Is.EqualTo(first.Images[1].Sha256));
                Assert.That(first.Images[2], Is.Null);
                Assert.That(first.Images[6], Is.Null);

                File.WriteAllBytes(common, new byte[] { 2 });
                var changed = LoganVisualContentCandidate.NativeKillIconInput.Capture(source, feed);
                Assert.That(changed.InputFingerprint, Is.Not.EqualTo(first.InputFingerprint));

                File.WriteAllBytes(missing, new byte[] { 3 });
                var appeared = LoganVisualContentCandidate.NativeKillIconInput.Capture(source, feed);
                Assert.That(appeared.Images[2], Is.Not.Null);
                Assert.That(appeared.InputFingerprint, Is.Not.EqualTo(changed.InputFingerprint));

                File.Delete(missing);
                Assert.That(LoganVisualContentCandidate.NativeKillIconInput.Capture(
                    source, feed).InputFingerprint, Is.EqualTo(changed.InputFingerprint));

                var escape = LoganModeKnockoutFeedInput.Parse(
                    "<bmp_begin> pic_type0: ../../outside.png <bmp_end>");
                Assert.Throws<ArgumentException>(() =>
                    LoganVisualContentCandidate.NativeKillIconInput.Capture(source, escape));
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
        }
    }
}
#endif
