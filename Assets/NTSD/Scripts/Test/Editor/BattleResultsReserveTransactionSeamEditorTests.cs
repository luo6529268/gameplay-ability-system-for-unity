#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class BattleResultsReserveTransactionSeamEditorTests
    {
        [Test]
        public void DedicatedReserveTransactionOwnerAndMaterializerExist()
        {
            const string writerPath =
                "Assets/NTSD/Scripts/Simulation/Ecs/BattleResultsReserveHostWriter.cs";
            const string stagePath =
                "Assets/NTSD/Scripts/Simulation/SimulationStageWaveModule.cs";

            Assert.That(File.Exists(writerPath), Is.True,
                "A dedicated reserve transaction owner must exist before terminal integration.");

            string writer = File.ReadAllText(writerPath);
            string stage = File.ReadAllText(stagePath);
            Assert.That(writer, Does.Contain("class BattleResultsReserveHostWriter"));
            Assert.That(writer, Does.Contain("TrySpawnBeforeResults"));
            Assert.That(stage, Does.Contain("TrySpawnResultsReserveEntry"));
        }

        [Test]
        public void AuthorityTransactionContractsPass()
        {
            var expectedRestConflict = new Regex(
                @"\[SimulationWorld\] Runtime rest bind failed; registration rejected: .*");
            LogAssert.Expect(LogType.Error, expectedRestConflict);
            LogAssert.Expect(LogType.Error, expectedRestConflict);
            MethodInfo method = typeof(BattleRuntimeSelfCheck).GetMethod(
                "RunResultsReserveTransactionSeamChecksForEditor",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, null);
        }
    }
}
#endif
