#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    [Category("BattleResultsOutcomeHostWriterSeam")]
    public sealed class BattleResultsOutcomeHostWriterSeamEditorTests
    {
        [Test]
        public void TerminalObservationHasDedicatedSingleWriterSource()
        {
            string simulationPath = Path.Combine(
                Application.dataPath,
                "NTSD",
                "Scripts",
                "Simulation");
            string ecsPath = Path.Combine(simulationPath, "Ecs");
            string outcomePath = Path.Combine(
                ecsPath,
                "BattleResultsOutcomeHostWriter.cs");
            string navigationPath = Path.Combine(ecsPath, "BattleResultsWriter.cs");
            string worldPath = Path.Combine(simulationPath, "SimulationWorld.cs");
            string registryPath = Path.Combine(
                simulationPath,
                "SimulationWorld.Registry.partial.cs");

            Assert.That(
                File.Exists(outcomePath),
                Is.True,
                "The completed-tick outcome host writer source does not exist yet.");

            string outcome = File.ReadAllText(outcomePath);
            string navigation = File.ReadAllText(navigationPath);
            string world = File.ReadAllText(worldPath);
            string registry = File.ReadAllText(registryPath);
            Assert.That(
                outcome,
                Does.Contain("internal sealed class BattleResultsOutcomeHostWriter"));
            Assert.That(outcome, Does.Contain("UpdateSummaryActivation"));
            Assert.That(navigation, Does.Not.Contain("UpdateSummaryActivation"));
            Assert.That(
                world,
                Does.Contain("battleResultsOutcomeHostWriter.UpdateSummaryActivation();"));
            Assert.That(
                registry,
                Does.Contain("battleResultsOutcomeHostWriter ="));
            Assert.That(
                registry,
                Does.Contain("new BattleResultsOutcomeHostWriter(this);"));
        }

        [Test]
        public void ExistingActiveResultsGateRemainsUnchanged()
        {
            var world = new SimulationWorld();
            BattleResultsRuntimeState results = world.Runtime.Results;
            world.Runtime.Match.BattleGameModeId = 2;
            results.Phase = 200;
            results.HadBoth = true;
            results.BattleEndPhase = 7;
            results.PendingWinner = 1;

            world.UpdateBattleResultsFlow();

            Assert.That(results.HadBoth, Is.True);
            Assert.That(results.BattleEndPhase, Is.EqualTo(7));
            Assert.That(results.PendingWinner, Is.EqualTo(1));
        }
    }
}
#endif
