#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28R06NaturalRevivalEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string EvidenceRoot =
            "artifacts/diagnostics/NTSD28-R06-NATURAL-REVIVAL-UNITY-001";
        private const string SourceRows =
            "artifacts/diagnostics/NTSD28-R06-NATURAL-REVIVAL-ENTRY-001/" +
            "source-run-02/natural_revival_source_ticks.csv";
        private static readonly string[] ComparedFields =
        {
            "actor_action", "actor_ko", "target_present", "target_hp",
            "target_action", "target_state", "target_lives", "target_phase",
            "target_hold"
        };
        private static readonly string[] OutputFields =
        {
            "tick", "actor_action", "actor_ko", "target_present", "target_hp",
            "target_action", "target_state", "target_lives", "target_phase",
            "target_hold", "target_x", "target_z", "target_physical_x",
            "target_physical_z", "target_source_initialized"
        };

        [Test]
        public void NaturalArmorLethalToOrdinaryRevival_MatchesPairedPlayableSource()
        {
            string scenarioPath = EvidenceRoot + "/scenario.json";
            var formal = ReadCsv(Path.GetFullPath(SourceRows));
            Assert.That(formal.Count, Is.EqualTo(70));
            var observed = new List<Dictionary<string, string>>(70);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, scenarioPath, BattleRuntimeProfile.Authority400, 70,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    Assert.That(world.RuntimeDataCatalog.IsReady, Is.True);
                    try
                    {
                        for (int tick = 1; tick <= 70; tick++)
                        {
                            Assert.That(driver.StepOneTick(inputs[tick - 1], true, false),
                                Is.True, "complete Driver tick=" + tick);
                            observed.Add(CaptureTick(world, tick));
                        }
                    }
                    finally
                    {
                        string outputDirectory = Path.GetFullPath(EvidenceRoot + "/unity");
                        Directory.CreateDirectory(outputDirectory);
                        string outputPath = Path.Combine(outputDirectory,
                            "natural-" + DateTime.UtcNow.ToString(
                                "yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) +
                            "-" + Guid.NewGuid().ToString("N") + ".csv");
                        var lines = new List<string>(observed.Count + 1)
                        {
                            string.Join(",", OutputFields)
                        };
                        lines.AddRange(observed.Select(row => string.Join(",",
                            OutputFields.Select(field => row[field]))));
                        File.WriteAllLines(outputPath, lines);
                        TestContext.Progress.WriteLine("R06 Unity raw: " + outputPath);
                    }

                    Assert.That(observed.Count, Is.EqualTo(70));
                    for (int index = 0; index < observed.Count; index++)
                    {
                        Dictionary<string, string> source = formal[index];
                        Dictionary<string, string> unity = observed[index];
                        Assert.That(unity["tick"], Is.EqualTo(source["tick"]));
                        foreach (string field in ComparedFields)
                        {
                            Assert.That(unity[field], Is.EqualTo(source[field]),
                                "first difference: tick=" + unity["tick"] +
                                " field=" + field + " formal=" + source[field] +
                                " unity=" + unity[field]);
                        }
                        AssertScaledPosition(source, unity, "x", 550, 2048.0 / 1333.0);
                        AssertScaledPosition(source, unity, "z", 542, 1152.0 / 730.0);
                    }
                    Assert.That(observed[15]["target_hp"], Is.EqualTo("-73"));
                    Assert.That(observed[26]["target_state"], Is.EqualTo("14"));
                    Assert.That(observed[53]["target_action"], Is.EqualTo("212"));
                    Assert.That(observed[53]["target_lives"], Is.EqualTo("1"));
                }, useProjectMode: true);
        }

        private static Dictionary<string, string> CaptureTick(SimulationWorld world, int tick)
        {
            LF2Entity actor = world.FindEntityByRuntimeSlotForQuery(0);
            LF2Entity target = world.FindEntityByRuntimeSlotForQuery(1);
            return new Dictionary<string, string>
            {
                ["tick"] = Number(tick),
                ["actor_action"] = Number(actor?.Frame.D?.frameId ?? -1),
                ["actor_ko"] = Number(actor?.Runtime.KnockoutCount358 ?? -1),
                ["target_present"] = target != null ? "1" : "0",
                ["target_hp"] = Number(target?.Health.HP ?? -1),
                ["target_action"] = Number(target?.Frame.D?.frameId ?? -1),
                ["target_state"] = Number(target == null ? -1 : (int)(target.Frame.D?.state ?? 0)),
                ["target_lives"] = Number(target?.HP2Orig ?? -1),
                ["target_phase"] = Number(target?.HitStun ?? -1),
                ["target_hold"] = Number(target?.FrameDelay ?? -1),
                ["target_x"] = Number(target == null ? -1 :
                    target.Runtime.SourceRulePositionInitialized
                        ? target.Runtime.SourceRuleXInt : target.Runtime.XInt),
                ["target_z"] = Number(target == null ? -1 :
                    target.Runtime.SourceRulePositionInitialized
                        ? target.Runtime.SourceRuleZInt : target.Runtime.ZInt),
                ["target_physical_x"] = Number(target?.Runtime.XInt ?? -1),
                ["target_physical_z"] = Number(target?.Runtime.ZInt ?? -1),
                ["target_source_initialized"] =
                    target?.Runtime.SourceRulePositionInitialized == true ? "1" : "0"
            };
        }

        private static List<Dictionary<string, string>> ReadCsv(string path)
        {
            string[] lines = File.ReadAllLines(path);
            string[] header = lines[0].Split(',');
            var result = new List<Dictionary<string, string>>(lines.Length - 1);
            foreach (string line in lines.Skip(1))
            {
                string[] values = line.Split(',');
                Assert.That(values.Length, Is.EqualTo(header.Length));
                var row = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int index = 0; index < header.Length; index++)
                    row.Add(header[index], values[index]);
                result.Add(row);
            }
            return result;
        }

        private static void AssertScaledPosition(
            Dictionary<string, string> source, Dictionary<string, string> unity,
            string axis, int initial, double scale)
        {
            int formalPosition = int.Parse(source["target_" + axis],
                CultureInfo.InvariantCulture);
            int physicalPosition = int.Parse(unity["target_physical_" + axis],
                CultureInfo.InvariantCulture);
            double expected = initial + (formalPosition - initial) * scale;
            Assert.That(Math.Abs(physicalPosition - expected), Is.LessThanOrEqualTo(1.5),
                "scaled position: tick=" + unity["tick"] + " axis=" + axis +
                " formal=" + formalPosition + " unity=" + physicalPosition);
        }

        private static string Number(int value) =>
            value.ToString(CultureInfo.InvariantCulture);
    }
}
#endif
