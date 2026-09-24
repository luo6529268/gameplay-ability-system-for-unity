#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07BackgroundModeInputEditorTests
    {
        private const string FormalRoot =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void FormalAndStagedFilesResolveSameOrderedBattleRecords()
        {
            var formal = LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(FormalRoot));
            var staged = LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(
                    Path.GetFullPath("Assets/NTSD/Content/LoganRuntime")));

            Assert.That(staged, Is.Not.Null);
            Assert.That(staged.Groups.Count, Is.EqualTo(32));
            Assert.That(staged.RecordCount, Is.EqualTo(96));
            Assert.That(staged.InputFingerprint, Is.EqualTo(formal.InputFingerprint));
            Assert.That(staged.SemanticFingerprint, Is.EqualTo(formal.SemanticFingerprint));
            staged.AssertInputsCurrent();

            Assert.That(staged.TryGetRecord(1, 0, out var normal), Is.True);
            Assert.That(normal.WeaponDrop, Is.EqualTo(2));
            Assert.That(normal.RegenHp, Is.EqualTo(1));
            Assert.That(normal.RegenMp, Is.EqualTo(1));
            Assert.That(normal.GainMpAttacker, Is.EqualTo(75));
            Assert.That(normal.GainMpVictim, Is.EqualTo(75));
            Assert.That(normal.Type, Is.EqualTo(1));

            Assert.That(staged.TryGetRecord(1, 1, out var casual), Is.True);
            Assert.That(casual.WeaponDrop, Is.Zero);
            Assert.That(casual.RegenHp, Is.Zero);
            Assert.That(casual.RegenMp, Is.Zero);
            Assert.That(casual.GainMpAttacker, Is.Zero);
            Assert.That(casual.Type, Is.EqualTo(1));

            Assert.That(staged.TryGetRecord(1, 2, out var practice), Is.True);
            Assert.That(practice.WeaponDrop, Is.EqualTo(2));
            Assert.That(practice.RegenHp, Is.EqualTo(1));
            Assert.That(practice.RegenMp, Is.Zero);
            Assert.That(staged.TryGetRecord(100, 0, out var random), Is.True);
            Assert.That(random.RegenMp, Is.EqualTo(normal.RegenMp));
            Assert.That(staged.TryGetRecord(107, 2, out var story), Is.True);
            Assert.That(story.Type, Is.EqualTo(practice.Type));
            Assert.That(staged.TryGetRecord(1, 3, out _), Is.False);
            Assert.That(staged.TryGetRecord(999, 0, out _), Is.False);
        }

        [Test]
        public void PresentParentRequiresEveryReferencedChild()
        {
            string root = CreateRuntime();
            Write(root, "data/bg_mode.dat",
                "<bg_information> bg: 3 file: data\\bg\\missing.dat <bg_information_end>");
            Assert.Throws<InvalidDataException>(() => LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(root)));
        }

        [Test]
        public void DuplicateIdAndEscapingChildAreRejected()
        {
            string root = CreateRuntime();
            Write(root, "data/bg_mode.dat",
                "<bg_information> bg: 3 file: data\\bg\\one.dat <bg_information_end> " +
                "<bg_information> bg: 3 file: data\\bg\\one.dat <bg_information_end>");
            Assert.Throws<InvalidDataException>(() => LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(root)));

            Write(root, "data/bg_mode.dat",
                "<bg_information> bg: 3 file: ../../outside.dat <bg_information_end>");
            Assert.Throws<ArgumentException>(() => LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(root)));
        }

        [Test]
        public void CapturedBytesBecomeStaleAndRecordDefaultsRemainZero()
        {
            string root = CreateRuntime();
            Write(root, "data/bg_mode.dat",
                "<bg_information> bg: 7 file: data\\bg\\one.dat <bg_information_end>");
            Write(root, "data/bg/one.dat",
                "<mode> name: test id: 9 id: 10 regen_hp: 1 <mode_end>");
            var input = LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(root));
            Assert.That(input.TryGetRecord(7, 0, out var record), Is.True);
            Assert.That(record.DropCandidateIds, Is.EqualTo(new[] { 9, 10 }));
            Assert.That(record.RegenHp, Is.EqualTo(1));
            Assert.That(record.RegenMp, Is.Zero);
            input.AssertInputsCurrent();

            File.AppendAllText(Path.Combine(root, "decoded_dat/data/bg/one.dat"), "\n");
            Assert.Throws<InvalidDataException>(() => input.AssertInputsCurrent());
            var changed = LoganBackgroundModeInput.Capture(
                BattleContentSource.ForLoganRuntime(root));
            Assert.That(changed.InputFingerprint, Is.Not.EqualTo(input.InputFingerprint));
            Assert.That(changed.SemanticFingerprint, Is.EqualTo(input.SemanticFingerprint));
        }

        private static string CreateRuntime()
        {
            string root = Path.GetFullPath("Temp/Q07BackgroundModeInput/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat", "data"));
            return root;
        }

        private static void Write(string root, string relative, string value)
        {
            string path = Path.Combine(root, "decoded_dat", relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, value);
        }
    }
}
#endif
