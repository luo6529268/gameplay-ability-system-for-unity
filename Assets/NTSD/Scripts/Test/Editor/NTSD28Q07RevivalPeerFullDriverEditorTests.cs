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
    public sealed class NTSD28Q07RevivalPeerFullDriverEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string EvidenceRoot =
            "artifacts/diagnostics/NTSD28-Q07-REVIVAL-PEER-FULL-DRIVER-001";
        private static readonly string[] ComparedFields =
        {
            "active", "oid", "action", "state", "hp", "base_hp", "effective_hp",
            "mp", "lives", "render_phase", "hold_timer", "frame_counter",
            "x", "y", "z", "precise_x", "precise_y", "precise_z",
            "vx", "vy", "vz", "active_count", "crt_state", "crt_calls",
            "sync_counter", "sync_index", "sync_calls", "sync_last_site",
            "sync_table_hash"
        };
        private static readonly string[] FloatFields =
        {
            "precise_x", "precise_y", "precise_z", "vx", "vy", "vz"
        };
        private static readonly string[] OutputFields =
        {
            "tick", "slot", "active", "oid", "action", "state", "hp",
            "base_hp", "effective_hp", "mp", "lives", "render_phase",
            "hold_timer", "frame_counter", "x", "y", "z", "precise_x",
            "precise_y", "precise_z", "vx", "vy", "vz", "active_count",
            "crt_state", "crt_calls", "sync_counter", "sync_index",
            "sync_calls", "sync_last_site", "sync_table_hash", "runtime_mp", "view_x",
            "view_z", "view_x_int", "view_z_int", "source_initialized"
        };

        [TestCase("positive", 100, 150)]
        [TestCase("negative", 0, 50)]
        public void LeeOrdinaryRevival_CompleteDriverMatchesPairedSession(
            string caseName, int peerSourceX, int peerViewX)
        {
            string sourcePath = Path.GetFullPath(
                EvidenceRoot + "/formal/" + caseName + ".csv");
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
                    LF2Entity dead = world.FindEntityByRuntimeSlotForQuery(0);
                    LF2Entity peer = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(dead, Is.Not.Null);
                    Assert.That(peer, Is.Not.Null);
                    Assert.That(dead.ObjectId, Is.EqualTo(7));
                    Assert.That(peer.ObjectId, Is.EqualTo(7));
                    Assert.That(dead.Frame.D?.state, Is.EqualTo(14));

                    dead.Health.HP = 0;
                    dead.Health.HPBound = 500;
                    dead.Health.HP3 = 500;
                    dead.Runtime.MP = 77;
                    dead.HP2Orig = 2;
                    dead.HitStun = 2;
                    dead.FrameDelay = 3;
                    dead.AttackingCounter = 0;
                    dead.Runtime.SetSourceRulePosition(50, 600);
                    dead.Runtime.SyncSourceRuleIntegerPosition();
                    peer.Runtime.SetSourceRulePosition(peerSourceX, 600);
                    peer.Runtime.SyncSourceRuleIntegerPosition();
                    peer.Runtime.X = peer.Runtime.XInt = peerViewX;
                    peer.PS.x = peerViewX;
                    dead.RefreshRuntimeSnapshot();
                    peer.RefreshRuntimeSnapshot();
                    Assert.That(dead.Runtime.SourceRulePositionInitialized, Is.True);
                    Assert.That(peer.Runtime.SourceRulePositionInitialized, Is.True);
                    Assert.That(peer.Runtime.SourceRuleXInt, Is.EqualTo(peerSourceX));
                    Assert.That(peer.Runtime.XInt, Is.EqualTo(peerViewX));

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
                                out string fieldValue) ? fieldValue : string.Empty))));
                        File.WriteAllLines(outputPath, lines);
                        TestContext.Progress.WriteLine("Q07 revival Unity raw: " + outputPath);
                    }
                    Compare(expected, observed, caseName);
                    var revived = observed.Single(row =>
                        row["tick"] == "1" && row["slot"] == "0");
                    if (caseName == "positive")
                    {
                        double rawX = ParseDouble(expected["1:0"]["precise_x"]) -
                            peerSourceX;
                        double rawZ = ParseDouble(expected["1:0"]["precise_z"]) - 600;
                        Assert.That(ParseDouble(revived["view_x"]),
                            Is.EqualTo(peerViewX + rawX * 2048.0 / 1333.0)
                                .Within(0.000001));
                        Assert.That(ParseDouble(revived["view_z"]),
                            Is.EqualTo(600 + rawZ * 1152.0 / 730.0)
                                .Within(0.000001));
                    }
                    else
                    {
                        Assert.That(ParseDouble(revived["view_x"]),
                            Is.EqualTo(50).Within(0.000001));
                        Assert.That(ParseDouble(revived["view_z"]),
                            Is.EqualTo(600).Within(0.000001));
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
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                var row = new Dictionary<string, string>
                {
                    ["tick"] = Number(tick),
                    ["slot"] = Number(slot),
                    ["active"] = entity != null ? "1" : "0",
                    ["active_count"] = Number(activeCount),
                    ["crt_state"] = Number(rng.CrtState),
                    ["crt_calls"] = Number(rng.CrtCalls),
                    ["sync_counter"] = Number(rng.SynchronizedCounter),
                    ["sync_index"] = Number(rng.SynchronizedIndex),
                    ["sync_calls"] = Number(rng.SynchronizedCalls),
                    ["sync_last_site"] = Number(rng.LastSynchronizedCallSite),
                    ["sync_table_hash"] = Number(rng.SynchronizedTableHash)
                };
                if (entity != null)
                {
                    row["oid"] = Number(entity.ObjectId);
                    row["action"] = Number(entity.Frame.D?.frameId ?? -1);
                    row["state"] = Number((int)(entity.Frame.D?.state ?? 0));
                    row["hp"] = Number(entity.Health.HP);
                    row["base_hp"] = Number(entity.Health.HP3);
                    row["effective_hp"] = Number(entity.Health.HPBound);
                    row["mp"] = Number(entity.Health.PP);
                    row["runtime_mp"] = Number(entity.Runtime.MP);
                    row["lives"] = Number(entity.HP2Orig);
                    row["render_phase"] = Number(entity.HitStun);
                    row["hold_timer"] = Number(entity.FrameDelay);
                    row["frame_counter"] = Number(entity.AttackingCounter);
                    row["x"] = Number(entity.Runtime.SourceRuleXInt);
                    row["y"] = Number(entity.Runtime.YInt);
                    row["z"] = Number(entity.Runtime.SourceRuleZInt);
                    row["precise_x"] = Number(entity.Runtime.SourceRuleX);
                    row["precise_y"] = Number(entity.Runtime.Y);
                    row["precise_z"] = Number(entity.Runtime.SourceRuleZ);
                    row["vx"] = Number(entity.Runtime.Vx);
                    row["vy"] = Number(entity.Runtime.Vy);
                    row["vz"] = Number(entity.Runtime.Vz);
                    row["view_x"] = Number(entity.Runtime.X);
                    row["view_z"] = Number(entity.Runtime.Z);
                    row["view_x_int"] = Number(entity.Runtime.XInt);
                    row["view_z_int"] = Number(entity.Runtime.ZInt);
                    row["source_initialized"] =
                        entity.Runtime.SourceRulePositionInitialized ? "1" : "0";
                }
                rows.Add(row);
            }
        }

        private static Dictionary<string, Dictionary<string, string>> ReadCsv(string path)
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
