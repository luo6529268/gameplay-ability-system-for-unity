#if UNITY_EDITOR
using System.IO;
using System.Reflection;

using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class BattleResultsReserveTerminalIntegrationEditorTests
    {
        [Test]
        public void OutcomeWriterUsesPersistentAuthorityDomainAndReserveBeforeGuard()
        {
            const string outcomePath =
                "Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs";
            string outcome = File.ReadAllText(outcomePath);

            Assert.That(outcome, Does.Not.Contain("BattleGameModeId != 1"));
            Assert.That(outcome, Does.Not.Contain("battle.Roster"));
            Assert.That(outcome, Does.Contain("AuthorityRuntimeSlotCapacity"));
            Assert.That(
                outcome,
                Does.Contain("TrySpawnBattleResultsReserveBeforeResults"));
        }

        [Test]
        public void AuthorityTerminalIntegrationContractsPass()
        {
            MethodInfo method = typeof(BattleRuntimeSelfCheck).GetMethod(
                "RunResultsReserveTerminalIntegrationChecksForEditor",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, null);
        }
    }
}
#endif
