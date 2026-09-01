#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("BattleResultsActivationResetAlignment")]
    public sealed class BattleResultsActivationResetAlignmentEditorTests
    {
        [Test]
        public void ActivationCommitsWinnerAndResetsResultTableToCanonicalDefaults()
        {
            var results = new BattleResultsRuntimeState
            {
                Phase = 17,
                Cursor = 3,
                SettingsCursor = 5,
                TableCursor = 4,
                TableSide = 1,
                ResultSubcursor = 5,
                Timer = 77,
                Winner = -1,
                PendingHostAction = BattleResultsRuntimeState.HostActionRematch,
                ResultTableSavedTop = 8,
                ResultTableSavedBottom = 9,
            };
            results.ResultMultiplier[0] = 250;
            results.ResultRow1Values[0, 0] = 99;
            results.ResultRow2Values[0, 0] = 88;
            results.ResultCommittedTotal[0, 0] = 77;
            results.ResultCommittedHp[0, 0] = 66;
            results.ResultSelectedTroop[0] = 5;
            results.ResultSelectedIcon[0] = 4;
            results.ResultTableTop[0] = 3;
            results.ResultTableBottom[0] = 2;
            results.ResultBackupRow1Values[0, 0] = 55;
            results.ResultBackupRow2Values[0, 0] = 44;

            results.ActivateSummary(1, 2, 11, 22);

            Assert.That(results.Phase, Is.EqualTo(200));
            Assert.That(results.Cursor, Is.EqualTo(6));
            Assert.That(results.SettingsCursor, Is.EqualTo(2));
            Assert.That(results.TableCursor, Is.EqualTo(10));
            Assert.That(results.TableSide, Is.Zero);
            Assert.That(results.ResultSubcursor, Is.EqualTo(2));
            Assert.That(results.Timer, Is.Zero);
            Assert.That(results.Winner, Is.EqualTo(1));
            Assert.That(results.ResultMultiplier[0], Is.EqualTo(100));
            Assert.That(results.ResultRow1Values[0, 0], Is.EqualTo(30));
            Assert.That(results.ResultRow2Values[0, 0], Is.EqualTo(10));
            Assert.That(results.ResultCommittedTotal[0, 0], Is.Zero);
            Assert.That(results.ResultCommittedHp[0, 0], Is.Zero);
            Assert.That(results.ResultSelectedTroop[0], Is.EqualTo(-1));
            Assert.That(results.ResultSelectedIcon[0], Is.EqualTo(-1));
            Assert.That(results.ResultTableTop[0], Is.EqualTo(-1));
            Assert.That(results.ResultTableBottom[0], Is.EqualTo(-1));
            Assert.That(results.ResultTableSavedTop, Is.EqualTo(-1));
            Assert.That(results.ResultTableSavedBottom, Is.EqualTo(-1));
            Assert.That(results.ResultBackupRow1Values[0, 0], Is.Zero);
            Assert.That(results.ResultBackupRow2Values[0, 0], Is.Zero);
            Assert.That(
                results.PendingHostAction,
                Is.EqualTo(BattleResultsRuntimeState.HostActionRematch));
        }

        [Test]
        public void ActivationClearsTerminalLiveGuardAfterCommittingWinner()
        {
            var results = new BattleResultsRuntimeState
            {
                HadBoth = true,
                BattleEndPhase = 11,
                PendingWinner = 1,
                TeamCount = 2,
            };
            results.TeamIds[0] = 11;
            results.TeamIds[1] = 22;

            results.ActivateSummary(1, 2, 11, 22);

            Assert.That(results.Winner, Is.EqualTo(1));
            Assert.That(results.IsActive, Is.True);
            Assert.That(results.HadBoth, Is.False);
            Assert.That(results.BattleEndPhase, Is.Zero);
            Assert.That(results.PendingWinner, Is.EqualTo(-2));
            Assert.That(results.TeamCount, Is.Zero);
            Assert.That(results.TeamIds[0], Is.EqualTo(-1));
            Assert.That(results.TeamIds[1], Is.EqualTo(-1));
        }
    }
}
#endif
