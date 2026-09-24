#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_Q09")]
    public sealed class NTSD28Q09WordsInputIdentityEditorTests
    {
        private const string FormalRoot =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void FormalAndStagedCandidateCaptureSameWordsInputs()
        {
            string stagedRoot = Path.GetFullPath(Path.Combine(
                Application.dataPath, "NTSD/Content/LoganRuntime"));
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            var formal = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(FormalRoot), mode);
            var staged = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(stagedRoot), mode);

            Assert.That(formal.Images.Count, Is.EqualTo(906));
            Assert.That(staged.Images.Count, Is.EqualTo(906));
            Assert.That(formal.WordsInput, Is.Not.Null);
            Assert.That(staged.WordsInput, Is.Not.Null);
            Assert.That(staged.WordsInput.InputFingerprint,
                Is.EqualTo(formal.WordsInput.InputFingerprint));
            Assert.That(staged.VisualFingerprint, Is.EqualTo(formal.VisualFingerprint));
            Assert.That(staged.WordsInput.Images.Count, Is.EqualTo(6));
            Assert.DoesNotThrow(formal.AssertInputsCurrent);
            Assert.DoesNotThrow(staged.AssertInputsCurrent);
        }

        [Test]
        public void NativeWordsInputTracksSelectionBytesAndPresence()
        {
            string root = Path.Combine(Path.GetTempPath(),
                "NTSD28-Q09-WordsInput-" + Guid.NewGuid().ToString("N"));
            try
            {
                var source = BattleContentSource.ForLoganRuntime(root);
                Assert.That(LoganVisualContentCandidate.NativeWordsInput.Capture(source), Is.Null);

                string datDirectory = Path.Combine(source.DatRoot, "data");
                Directory.CreateDirectory(datDirectory);
                string resourcePath = Path.Combine(datDirectory, "resource.dat");
                string[] rows = new string[22];
                for (int index = 0; index < rows.Length; index++)
                    rows[index] = "pic: sprite/UI/" +
                        (index < 16 ? "unused" + index : "WORDS" + (index - 16)) + ".png";
                File.WriteAllText(resourcePath,
                    "<bmp_begin>\n" + string.Join("\n", rows) + "\n<bmp_end>\n",
                    new UTF8Encoding(false));

                string imageDirectory = Path.Combine(source.ImageRoot, "sprite", "UI");
                Directory.CreateDirectory(imageDirectory);
                for (int index = 0; index < 6; index++)
                    File.WriteAllBytes(Path.Combine(imageDirectory, "WORDS" + index + ".png"),
                        new byte[] { (byte)index });

                var first = LoganVisualContentCandidate.NativeWordsInput.Capture(source);
                Assert.That(first.Images.Count, Is.EqualTo(6));
                Assert.That(Path.GetFileName(first.Images[0].Path), Is.EqualTo("WORDS0.png"));
                Assert.That(Path.GetFileName(first.Images[5].Path), Is.EqualTo("WORDS5.png"));
                Assert.That(LoganVisualContentCandidate.NativeWordsInput.Capture(source).InputFingerprint,
                    Is.EqualTo(first.InputFingerprint));

                File.WriteAllBytes(Path.Combine(imageDirectory, "WORDS3.png"), new byte[] { 99 });
                string changedImageFingerprint =
                    LoganVisualContentCandidate.NativeWordsInput.Capture(source).InputFingerprint;
                Assert.That(changedImageFingerprint,
                    Is.Not.EqualTo(first.InputFingerprint));

                File.AppendAllText(resourcePath, "changed nonselected bytes\n");
                var changedDat = LoganVisualContentCandidate.NativeWordsInput.Capture(source);
                Assert.That(changedDat.InputFingerprint,
                    Is.Not.EqualTo(changedImageFingerprint));

                rows[16] = "pic: ../../outside.png";
                File.WriteAllText(resourcePath,
                    "<bmp_begin>\n" + string.Join("\n", rows) + "\n<bmp_end>\n",
                    new UTF8Encoding(false));
                Assert.Throws<ArgumentException>(() =>
                    LoganVisualContentCandidate.NativeWordsInput.Capture(source));

                File.Delete(resourcePath);
                Assert.That(LoganVisualContentCandidate.NativeWordsInput.Capture(source), Is.Null);
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
