#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;

using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_Q09")]
    public sealed class NTSD28Q09KilltextModeInputEditorTests
    {
        [Test]
        public void ProjectModeAssetProjectsSelectedKilltextFields()
        {
            LoganModeKnockoutFeedInput input =
                LoganModeKnockoutFeedInput.FromProjectSnapshot(
                    ProjectBattleModeConfig.LoadDefault().Capture());

            Assert.That(input, Is.Not.Null);
            Assert.That(input.Enabled, Is.True);
            Assert.That(input.Respond, Is.EqualTo(5));
            Assert.That(input.LifetimeTicks, Is.EqualTo(70));
            Assert.That(input.RowSpacing, Is.EqualTo(40));
            Assert.That(input.Transparency, Is.EqualTo(1));
            Assert.That(input.ScreenTop, Is.EqualTo(132));
            Assert.That(input.ImageScreenLeft, Is.EqualTo(575));
            Assert.That(input.AttackerScreenLeft, Is.EqualTo(560));
            Assert.That(input.VictimScreenLeft, Is.EqualTo(650));
            Assert.That(input.AttackerTeamColor, Is.True);
            Assert.That(input.VictimTeamColor, Is.True);
            Assert.That(input.AttackerAppendCharacterName, Is.True);
            Assert.That(input.VictimAppendCharacterName, Is.True);
            Assert.That(input.AttackerRightAligned, Is.True);
            Assert.That(input.VictimRightAligned, Is.False);
            Assert.That(input.AllowedBattleModes, Is.EqualTo(new[] { 0, 1, 4 }));
            Assert.That(input.ExcludedVictimObjectIds, Is.Empty);
            Assert.That(input.TypeResourcePath(0), Is.EqualTo("sprite/kill/c.png"));
            Assert.That(input.TypeResourcePath(3), Is.EqualTo("sprite/kill/sk1.png"));
            Assert.That(input.StageTeam1DeathSoundPath, Is.EqualTo("data/m_ok.wav"));
            Assert.That(input.StageTeam5DeathSoundPath, Is.EqualTo("data/m_join.wav"));
        }

        [Test]
        public void MissingRecordDiffersFromBoundZeroAndKeepsRepeatedFilters()
        {
            Assert.That(LoganModeKnockoutFeedInput.Parse(
                "<combo> bound: 1 <combo_end>"), Is.Null);
            LoganModeKnockoutFeedInput input = LoganModeKnockoutFeedInput.Parse(
                "<bmp_begin> bound: 0 times: -1 mode: 0 mode: 4 id: 30 id: 31 <bmp_end>");
            Assert.That(input, Is.Not.Null);
            Assert.That(input.Enabled, Is.False);
            Assert.That(input.LifetimeTicks, Is.EqualTo(-1));
            Assert.That(input.AllowedBattleModes, Is.EqualTo(new[] { 0, 4 }));
            Assert.That(input.ExcludedVictimObjectIds, Is.EqualTo(new[] { 30, 31 }));
            Assert.That(input.TypeResourcePath(0), Is.Null);
        }

        [Test]
        public void MalformedRecordIsRejected()
        {
            Assert.Throws<InvalidDataException>(() =>
                LoganModeKnockoutFeedInput.Parse("<bmp_begin> times: 70"));
            Assert.Throws<InvalidDataException>(() =>
                LoganModeKnockoutFeedInput.Parse(
                    "<bmp_begin> times: wrong <bmp_end>"));
        }
    }
}
#endif
