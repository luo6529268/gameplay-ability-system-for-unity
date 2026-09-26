#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q07HidanNaturalCatchRelationEditorTests
    {
        private const string Root =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string SourceEvidence =
            "artifacts/diagnostics/NTSD28-Q07-HIDAN-NATURAL-INPUT-REACHABILITY-001/";
        private const string UnityEvidence =
            "artifacts/diagnostics/NTSD28-Q07-HIDAN-NATURAL-UNITY-DRIVER-001/";

        [TestCase(580)]
        [TestCase(1200)]
        public void NaturalCatchRelationsMatchFormalRootAtEveryCompletedTick(int targetX)
        {
            string tracePath = SourceEvidence + "release-natural-x" + targetX + "-trace.jsonl";
            Assert.That(File.Exists(tracePath), Is.True, tracePath);
            var expected = File.ReadAllLines(tracePath)
                .Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 40)
                .ToDictionary(row => (int)row["tick"]);
            Assert.That(expected.Count, Is.EqualTo(40));

            string scenario = UnityEvidence + "unity-scenario-x" + targetX + ".json";
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                Root, scenario, BattleRuntimeProfile.Authority400, 40,
                (driver, inputs, identity) =>
                {
                    for (int tick = 1; tick <= 40; tick++)
                    {
                        Assert.That(driver.StepOneTick(inputs[tick - 1], true, false),
                            Is.True, "tick=" + tick);
                        var actor = driver.World.FindEntityByRuntimeSlotForQuery(0);
                        var target = driver.World.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(actor, Is.Not.Null, "actor tick=" + tick);
                        Assert.That(target, Is.Not.Null, "target tick=" + tick);

                        var entities = (JArray)expected[tick]["entities"];
                        var formalActor = entities.Children<JObject>()
                            .Single(row => (int)row["slot"] == 0);
                        var formalTarget = entities.Children<JObject>()
                            .Single(row => (int)row["slot"] == 1);
                        Assert.That(actor.Runtime.CaughtSlotIndex,
                            Is.EqualTo((int)formalActor["catchTargetSlot8C"]),
                            "actor catch target tick=" + tick + " targetX=" + targetX);
                        Assert.That(target.Runtime.CatchSourceSlot90,
                            Is.EqualTo((int)formalTarget["catchSourceSlot90"]),
                            "target catch source tick=" + tick + " targetX=" + targetX);
                    }
                }, useProjectMode: true);
        }
    }
}
#endif
