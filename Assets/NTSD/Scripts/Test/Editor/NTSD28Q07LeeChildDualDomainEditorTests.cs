#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07LeeChildDualDomainEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string ScenarioPath =
            "artifacts/diagnostics/NTSD28-Q07-LEE-JL-UNITY-RAW-FIRST-DIFF-001/lee-jl-formal-45.json";
        private const string NativePath =
            "artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-DUAL-DOMAIN-001/formal-oid204-ticks6-13.csv";
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD28-Q07-LEE-CHILD-DUAL-DOMAIN-001/unity";
        private const double XFactor = 2048.0 / 1333.0;
        private const double ZFactor = 1152.0 / 730.0;

        [Test]
        public void FormalLeeChildren_KeepSourceTraceAndScalePhysicalTravel()
        {
            Dictionary<string, NativeRow> expected = ReadNativeRows();
            var observed = new List<string>(41)
            {
                "tick,slot,oid,action,source_x,source_z,source_x_int,source_z_int,view_x,view_z,source_initialized"
            };
            var birthViewX = new Dictionary<int, double>(5);
            var birthViewZ = new Dictionary<int, double>(5);

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, ScenarioPath, BattleRuntimeProfile.Authority400, 45,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    for (int slot = 0; slot < 2; slot++)
                    {
                        LF2Entity participant = world.FindEntityByRuntimeSlotForQuery(slot);
                        Assert.That(participant, Is.Not.Null, "participant slot=" + slot);
                        participant.Runtime.SetSourceRulePosition(
                            participant.Runtime.X, participant.Runtime.Z);
                        participant.Runtime.SyncSourceRuleIntegerPosition();
                    }

                    try
                    {
                        for (int tick = 1; tick <= 45; tick++)
                        {
                            Assert.That(driver.StepOneTick(inputs[tick - 1], true, false),
                                Is.True, "complete Driver tick=" + tick);
                            if (tick < 6 || tick > 13)
                                continue;

                            for (int slot = 51; slot <= 55; slot++)
                            {
                                NativeRow native = expected[tick + ":" + slot];
                                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(slot);
                                Assert.That(child, Is.Not.Null,
                                    "formal OID204 occupied tick=" + tick + " slot=" + slot);
                                var runtime = child.Runtime;
                                observed.Add(string.Join(",", new[]
                                {
                                    tick.ToString(CultureInfo.InvariantCulture),
                                    slot.ToString(CultureInfo.InvariantCulture),
                                    child.ObjectId.ToString(CultureInfo.InvariantCulture),
                                    (child.Frame.D?.frameId ?? -1).ToString(CultureInfo.InvariantCulture),
                                    runtime.SourceRuleX.ToString("R", CultureInfo.InvariantCulture),
                                    runtime.SourceRuleZ.ToString("R", CultureInfo.InvariantCulture),
                                    runtime.SourceRuleXInt.ToString(CultureInfo.InvariantCulture),
                                    runtime.SourceRuleZInt.ToString(CultureInfo.InvariantCulture),
                                    runtime.X.ToString("R", CultureInfo.InvariantCulture),
                                    runtime.Z.ToString("R", CultureInfo.InvariantCulture),
                                    runtime.SourceRulePositionInitialized ? "1" : "0"
                                }));

                                Assert.That(child.ObjectId, Is.EqualTo(native.Oid),
                                    "tick=" + tick + " slot=" + slot);
                                Assert.That(runtime.SourceRulePositionInitialized, Is.True,
                                    "tick=" + tick + " slot=" + slot);
                                Assert.That(child.Frame.D?.frameId ?? -1, Is.EqualTo(native.Action),
                                    "action tick=" + tick + " slot=" + slot);
                                Assert.That(runtime.SourceRuleX,
                                    Is.EqualTo(native.PreciseX).Within(1e-6),
                                    "source X tick=" + tick + " slot=" + slot);
                                Assert.That(runtime.SourceRuleZ,
                                    Is.EqualTo(native.PreciseZ).Within(1e-6),
                                    "source Z tick=" + tick + " slot=" + slot);
                                Assert.That(runtime.SourceRuleXInt, Is.EqualTo(native.X),
                                    "source X integer tick=" + tick + " slot=" + slot);
                                Assert.That(runtime.SourceRuleZInt, Is.EqualTo(native.Z),
                                    "source Z integer tick=" + tick + " slot=" + slot);

                                if (tick == 6)
                                {
                                    birthViewX.Add(slot, runtime.X);
                                    birthViewZ.Add(slot, runtime.Z);
                                }
                                else
                                {
                                    NativeRow birth = expected["6:" + slot];
                                    Assert.That(runtime.X - birthViewX[slot],
                                        Is.EqualTo((native.PreciseX - birth.PreciseX) * XFactor)
                                            .Within(1e-6),
                                        "physical X travel tick=" + tick + " slot=" + slot);
                                    Assert.That(runtime.Z - birthViewZ[slot],
                                        Is.EqualTo((native.PreciseZ - birth.PreciseZ) * ZFactor)
                                            .Within(1e-6),
                                        "physical Z travel tick=" + tick + " slot=" + slot);
                                }
                            }
                        }
                    }
                    finally
                    {
                        string outputDirectory = Path.GetFullPath(OutputRoot);
                        Directory.CreateDirectory(outputDirectory);
                        string outputPath = Path.Combine(outputDirectory,
                            "configured-view-" + DateTime.UtcNow.ToString(
                                "yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) +
                            "-" + Guid.NewGuid().ToString("N") + ".csv");
                        File.WriteAllLines(outputPath, observed);
                        TestContext.Progress.WriteLine(
                            "Q07 Lee child dual-domain raw: " + outputPath);
                    }
                }, useProjectMode: true);
            Assert.That(observed.Count, Is.EqualTo(41));
        }

        private static Dictionary<string, NativeRow> ReadNativeRows()
        {
            string[] lines = File.ReadAllLines(Path.GetFullPath(NativePath));
            Assert.That(lines.Length, Is.EqualTo(41));
            var rows = new Dictionary<string, NativeRow>(40);
            for (int index = 1; index < lines.Length; index++)
            {
                string[] fields = lines[index].Split(',');
                Assert.That(fields.Length, Is.EqualTo(8), "native row=" + index);
                var row = new NativeRow
                {
                    Tick = int.Parse(fields[0], CultureInfo.InvariantCulture),
                    Slot = int.Parse(fields[1], CultureInfo.InvariantCulture),
                    Oid = int.Parse(fields[2], CultureInfo.InvariantCulture),
                    Action = int.Parse(fields[3], CultureInfo.InvariantCulture),
                    PreciseX = double.Parse(fields[4], CultureInfo.InvariantCulture),
                    PreciseZ = double.Parse(fields[5], CultureInfo.InvariantCulture),
                    X = int.Parse(fields[6], CultureInfo.InvariantCulture),
                    Z = int.Parse(fields[7], CultureInfo.InvariantCulture)
                };
                Assert.That(row.Oid, Is.EqualTo(204));
                rows.Add(row.Tick + ":" + row.Slot, row);
            }
            return rows;
        }

        private sealed class NativeRow
        {
            public int Tick;
            public int Slot;
            public int Oid;
            public int Action;
            public double PreciseX;
            public double PreciseZ;
            public int X;
            public int Z;
        }
    }
}
#endif
