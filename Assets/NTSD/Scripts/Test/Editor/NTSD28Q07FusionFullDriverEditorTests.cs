#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07FusionFullDriverEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string EvidenceRoot =
            "artifacts/diagnostics/NTSD28-Q07-FUSION-FULL-DRIVER-FIRST-DIFF-001";
        private static readonly string[] ComparedFields =
        {
            "active", "suspended", "oid", "action", "state", "hp", "mp",
            "x", "y", "z", "precise_x", "precise_y", "precise_z",
            "vx", "vy", "vz", "timer338", "gate328", "display190",
            "partner32c", "primary330", "partner334", "frame_counter",
            "active_count", "crt_state", "crt_calls", "sync_counter",
            "sync_index", "sync_calls", "sync_table_hash"
        };
        private static readonly string[] FloatFields =
        {
            "precise_x", "precise_y", "precise_z", "vx", "vy", "vz"
        };
        private static readonly string[] OutputFields =
        {
            "tick", "slot", "active", "suspended", "oid", "action", "state",
            "hp", "mp", "x", "y", "z", "precise_x", "precise_y",
            "precise_z", "vx", "vy", "vz", "timer338", "gate328",
            "display190", "partner32c", "primary330", "partner334",
            "frame_counter", "active_count", "crt_state", "crt_calls",
            "sync_counter", "sync_index", "sync_calls", "sync_table_hash",
            "view_x", "view_z", "source_initialized"
        };

        [TestCase("positive", 304)]
        [TestCase("negative", 315)]
        public void FormalFusionRow0_CompleteDriverMatchesPairedSession(
            string caseName, int primaryX)
        {
            string sourcePath = Path.GetFullPath(
                EvidenceRoot + "/formal/" + caseName + "-v3.csv");
            string scenarioPath = EvidenceRoot + "/" + caseName + "-scenario.json";
            var expected = ReadCsv(sourcePath);
            Assert.That(expected.Count, Is.EqualTo(8), sourcePath);
            var observed = new List<Dictionary<string, string>>(8);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, scenarioPath, BattleRuntimeProfile.Authority400, 3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    Assert.That(world.RuntimeDataCatalog.IsReady, Is.True);
                    LoganFusionCatalog fusion = world.RuntimeDataCatalog.FusionCatalog;
                    Assert.That(fusion, Is.Not.Null);
                    Assert.That(fusion.SourceAvailable && fusion.IsValid, Is.True);
                    Assert.That(fusion.Records.Count, Is.GreaterThan(0));
                    LoganFusionRecord row = fusion.Records[0];
                    Assert.That(new[] { row.Id1, row.Id2, row.Id3, row.Hp,
                            row.State, row.Action, row.Decrease },
                        Is.EqualTo(new[] { 7, 8, 51, 177, 2, 290, 4500 }));
                    Assert.That(world.RuntimeDataCatalog.GetObjectDefinition(51),
                        Is.Not.Null, "formal fused definition must be prepared");

                    LF2Entity primary = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity partner = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(primary, Is.Not.Null);
                    Assert.That(partner, Is.Not.Null);
                    Assert.That(primary.ObjectId, Is.EqualTo(7));
                    Assert.That(partner.ObjectId, Is.EqualTo(8));
                    primary.Runtime.SetSourceRulePosition(primaryX, 600);
                    partner.Runtime.SetSourceRulePosition(300, 600);
                    primary.Runtime.SyncSourceRuleIntegerPosition();
                    partner.Runtime.SyncSourceRuleIntegerPosition();
                    primary.Runtime.SetVelocity(20, 0, 0);
                    partner.Runtime.SetVelocity(0, 0, 0);
                    foreach (LF2Entity entity in new[] { primary, partner })
                    {
                        entity.Runtime.Unk328 = 0;
                        entity.Runtime.Unk330 = -1;
                        entity.Runtime.Unk334 = -1;
                    }
                    Assert.That(primary.Runtime.XInt, Is.EqualTo(primaryX));
                    Assert.That(partner.Runtime.XInt, Is.EqualTo(300));
                    Assert.That(primary.Runtime.SourceRulePositionInitialized, Is.True);
                    Assert.That(partner.Runtime.SourceRulePositionInitialized, Is.True);

                    try
                    {
                        CaptureTick(world, observed, 0);
                        for (int tick = 1; tick <= 3; tick++)
                        {
                            Assert.That(driver.StepOneTick(inputs[tick - 1], true, false),
                                Is.True, "complete Driver tick=" + tick);
                            CaptureTick(world, observed, tick);
                        }
                    }
                    finally
                    {
                        string outputDirectory = Path.GetFullPath(
                            EvidenceRoot + "/unity");
                        Directory.CreateDirectory(outputDirectory);
                        string outputPath = Path.Combine(outputDirectory,
                            caseName + "-" + DateTime.UtcNow.ToString(
                                "yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) +
                            "-" + Guid.NewGuid().ToString("N") + ".csv");
                        var lines = new List<string>(observed.Count + 1)
                        {
                            string.Join(",", OutputFields)
                        };
                        lines.AddRange(observed.Select(value => string.Join(",",
                            OutputFields.Select(field => value.TryGetValue(field,
                                out string text) ? text : string.Empty))));
                        File.WriteAllLines(outputPath, lines);
                        TestContext.Progress.WriteLine("Q07 fusion Unity raw: " + outputPath);
                    }
                    Compare(expected, observed, caseName);
                }, useProjectMode: true);
        }

        [Test]
        public void FormalFusionRow1_UndeclaredActionUsesNativeZeroFrame()
        {
            const string scenarioPath =
                "artifacts/diagnostics/NTSD28-Q07-FUSION-MISSING-FRAME-GATE-001/row1-scenario.json";
            const string outputRoot =
                "artifacts/diagnostics/NTSD28-Q07-FUSION-MISSING-FRAME-GATE-001/unity";
            var observed = new List<Dictionary<string, string>>(8);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, scenarioPath, BattleRuntimeProfile.Authority400, 3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    LF2Entity primary = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity partner = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(primary?.ObjectId, Is.EqualTo(10));
                    Assert.That(partner?.ObjectId, Is.EqualTo(11));
                    primary.Runtime.SetSourceRulePosition(304, 600);
                    partner.Runtime.SetSourceRulePosition(300, 600);
                    primary.Runtime.SyncSourceRuleIntegerPosition();
                    partner.Runtime.SyncSourceRuleIntegerPosition();
                    primary.Runtime.SetVelocity(20, 0, 0);
                    partner.Runtime.SetVelocity(0, 0, 0);
                    foreach (LF2Entity entity in new[] { primary, partner })
                    {
                        entity.Runtime.Unk328 = 0;
                        entity.Runtime.Unk330 = -1;
                        entity.Runtime.Unk334 = -1;
                    }

                    try
                    {
                        CaptureTick(world, observed, 0);
                        for (int tick = 1; tick <= 3; tick++)
                        {
                            Assert.That(driver.StepOneTick(inputs[tick - 1], true, false),
                                Is.True, "complete Driver tick=" + tick);
                            CaptureTick(world, observed, tick);
                        }
                    }
                    finally
                    {
                        string outputDirectory = Path.GetFullPath(outputRoot);
                        Directory.CreateDirectory(outputDirectory);
                        string outputPath = Path.Combine(outputDirectory,
                            "row1-" + DateTime.UtcNow.ToString(
                                "yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) +
                            "-" + Guid.NewGuid().ToString("N") + ".csv");
                        var lines = new List<string>(observed.Count + 1)
                        {
                            string.Join(",", OutputFields)
                        };
                        lines.AddRange(observed.Select(value => string.Join(",",
                            OutputFields.Select(field => value.TryGetValue(field,
                                out string text) ? text : string.Empty))));
                        File.WriteAllLines(outputPath, lines);
                        TestContext.Progress.WriteLine("Q07 fusion row1 Unity raw: " + outputPath);
                    }

                    Assert.That(observed.Count, Is.EqualTo(8));
                    for (int tick = 1; tick <= 3; tick++)
                    {
                        Dictionary<string, string> merged = observed[tick * 2];
                        Dictionary<string, string> dormant = observed[tick * 2 + 1];
                        Assert.That(merged["oid"], Is.EqualTo("52"), "tick=" + tick);
                        Assert.That(merged["action"], Is.EqualTo("310"), "tick=" + tick);
                        Assert.That(merged["x"], Is.EqualTo("299"), "tick=" + tick);
                        Assert.That(merged["source_initialized"], Is.EqualTo("1"));
                        Assert.That(dormant["active"], Is.EqualTo("0"));
                        Assert.That(dormant["suspended"], Is.EqualTo("1"));
                    }
                }, useProjectMode: true);
        }

        private static void CaptureTick(SimulationWorld world,
            List<Dictionary<string, string>> rows, int tick)
        {
            int activeCount = Enumerable.Range(0,
                    world.RuntimeSlotCapacityForDiagnostics)
                .Count(slot => world.FindEntityByRuntimeSlotForQuery(slot) != null);
            var rng = world.NativeRandom.CaptureScalarState();
            for (int slot = 0; slot < 2; slot++)
            {
                LF2Entity active = world.FindEntityByRuntimeSlotForQuery(slot);
                LF2Entity includingDormant =
                    world.FindEntityByRuntimeSlotIncludingDormant(slot);
                var row = new Dictionary<string, string>
                {
                    ["tick"] = Number(tick),
                    ["slot"] = Number(slot),
                    ["active"] = active != null ? "1" : "0",
                    ["suspended"] = active == null && includingDormant != null &&
                        includingDormant.Runtime.OidMergeDormant ? "1" : "0",
                    ["active_count"] = Number(activeCount),
                    ["crt_state"] = Number(rng.CrtState),
                    ["crt_calls"] = Number(rng.CrtCalls),
                    ["sync_counter"] = Number(rng.SynchronizedCounter),
                    ["sync_index"] = Number(rng.SynchronizedIndex),
                    ["sync_calls"] = Number(rng.SynchronizedCalls),
                    ["sync_table_hash"] = Number(rng.SynchronizedTableHash)
                };
                if (active != null)
                {
                    row["oid"] = Number(active.ObjectId);
                    row["action"] = Number(active.Frame.D?.frameId ?? -1);
                    row["state"] = Number((int)(active.Frame.D?.state ?? 0));
                    row["hp"] = Number(active.Health.HP);
                    row["mp"] = Number(active.Runtime.MP);
                    row["x"] = Number(active.Runtime.SourceRuleXInt);
                    row["y"] = Number(active.Runtime.YInt);
                    row["z"] = Number(active.Runtime.SourceRuleZInt);
                    row["precise_x"] = Number(active.Runtime.SourceRuleX);
                    row["precise_y"] = Number(active.Runtime.Y);
                    row["precise_z"] = Number(active.Runtime.SourceRuleZ);
                    row["vx"] = Number(active.Runtime.Vx);
                    row["vy"] = Number(active.Runtime.Vy);
                    row["vz"] = Number(active.Runtime.Vz);
                    row["timer338"] = Number(active.Runtime.Unk338);
                    row["gate328"] = Number(active.Runtime.Unk328);
                    row["display190"] = Number(active.Runtime.FusionDisplayTimer190);
                    row["partner32c"] = Number(active.Runtime.Unk32C);
                    row["primary330"] = Number(active.Runtime.Unk330);
                    row["partner334"] = Number(active.Runtime.Unk334);
                    row["frame_counter"] = Number(active.AttackingCounter);
                    row["view_x"] = Number(active.Runtime.X);
                    row["view_z"] = Number(active.Runtime.Z);
                    row["source_initialized"] =
                        active.Runtime.SourceRulePositionInitialized ? "1" : "0";
                }
                rows.Add(row);
            }
        }

        private static Dictionary<string, Dictionary<string, string>> ReadCsv(
            string path)
        {
            string[] lines = File.ReadAllLines(path);
            Assert.That(lines.Length, Is.EqualTo(9), path);
            string[] header = lines[0].Split(',');
            var rows = new Dictionary<string, Dictionary<string, string>>();
            foreach (string line in lines.Skip(1))
            {
                string[] values = line.Split(',');
                Assert.That(values.Length, Is.EqualTo(header.Length), line);
                var row = new Dictionary<string, string>();
                for (int index = 0; index < header.Length; index++)
                    row.Add(header[index], values[index]);
                rows.Add(row["tick"] + ":" + row["slot"], row);
            }
            return rows;
        }

        private static void Compare(
            Dictionary<string, Dictionary<string, string>> expected,
            List<Dictionary<string, string>> observed, string caseName)
        {
            Assert.That(observed.Count, Is.EqualTo(8));
            foreach (Dictionary<string, string> actual in observed)
            {
                string key = actual["tick"] + ":" + actual["slot"];
                Assert.That(expected.TryGetValue(key, out var formal), Is.True, key);
                foreach (string field in ComparedFields)
                {
                    string sourceValue = formal[field];
                    if (string.IsNullOrEmpty(sourceValue))
                        continue;
                    Assert.That(actual.TryGetValue(field, out string unityValue),
                        Is.True, caseName + " " + key + " " + field);
                    if (FloatFields.Contains(field))
                        Assert.That(ParseDouble(unityValue),
                            Is.EqualTo(ParseDouble(sourceValue)).Within(0.000001),
                            caseName + " " + key + " " + field);
                    else
                        Assert.That(unityValue, Is.EqualTo(sourceValue),
                            caseName + " " + key + " " + field);
                }
            }
        }

        private static double ParseDouble(string value) =>
            double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);

        private static string Number(int value) =>
            value.ToString(CultureInfo.InvariantCulture);

        private static string Number(uint value) =>
            value.ToString(CultureInfo.InvariantCulture);

        private static string Number(ulong value) =>
            value.ToString(CultureInfo.InvariantCulture);

        private static string Number(double value) =>
            value.ToString("R", CultureInfo.InvariantCulture);
    }
}
#endif
