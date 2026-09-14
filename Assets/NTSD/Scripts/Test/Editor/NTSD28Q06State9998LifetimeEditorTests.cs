#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06State9998LifetimeEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-STATE9998-SOURCE-DRIVER-WITNESS-001/native.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-STATE9998-LEGACY-CLEANUP-RETIREMENT-001/";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void LateAndSerialLifetimeMatchesOriginalDriver(int type)
        {
            var rows = File.ReadLines(Witness).Select(JObject.Parse).Where(r => (int)r["type"] == type).ToArray();
            var failures = new List<string>();
            int cases = 0;
            foreach (var group in rows.GroupBy(r => string.Join("/", new[] { "state", "enter", "hp", "y", "slot" }.Select(k => (int)r[k]))))
            {
                var first = group.First();
                int state = (int)first["state"], enter = (int)first["enter"];
                var data = new LF2CharacterData();
                data.frames.Add(new LF2FrameData { frameId = 0, state = enter == 0 ? state : 0, wait = enter == 0 ? 100 : 0, next = enter });
                data.frames.Add(new LF2FrameData { frameId = 1, state = state, wait = 100, next = 1 });
                var wrapper = new LF2CharacterDataWrapper(777, data);
                var world = new SimulationWorld();
                world.SetLogicOnlyEntityMaterialization(true);
                world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(777, type, "state9998-fixture.dat") }, _ => wrapper);
                try
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, opoint = new ObjectPoint { oid = 777, kind = 1, action = 0 },
                        requiredRuntimeSlot = (int)first["slot"], preserveActionZero = true, dir = "right",
                        useDirectRuntimePosition = true, directX = 100, directY = (int)first["y"], directZ = 100,
                        useInitialRuntimeIntPosition = true, initialRuntimeX = 100, initialRuntimeY = (int)first["y"], initialRuntimeZ = 100,
                        skipPostInitZOffset = true,
                    };
                    var entity = world.LogicEntityFactory.Create(task, out var failure);
                    Assert.That(entity, Is.Not.Null, type + "/" + group.Key + " " + failure);
                    entity.Health.HP = (int)first["hp"];
                    entity.Health.HPBound = (int)first["hp"];
                    entity.Health.HP3 = (int)first["hp"];
                    entity.Runtime.HP2Orig = 1;
                    entity.Runtime.HPOrig = 0;
                    entity.Runtime.RespawnCount = 0;
                    entity.Runtime.DisplayCurrentHp200 = (int)first["hp"];
                    entity.Runtime.DisplayEffectiveMaxHp208 = (int)first["hp"];
                    world.NativeRandom.ResetFromSeed(42);
                    foreach (var row in group)
                    {
                        int tick = (int)row["tick"];
                        world.LateEntityUpdateAll(tick);
                        int actionBeforeSerial = entity.Frame.N;
                        world.SerialTickAll(tick);
                        var actual = world.FindEntityByRuntimeSlotForQuery((int)row["slot"]);
                        var expected = row["entity"];
                        if (actual == null)
                        {
                            failures.Add(group.Key + " tick" + tick + " removed; original alive");
                            break;
                        }
                        if (actual.Frame.N != (int)expected["frame"]["action"] || actual.Frame.D?.state != (int)expected["frame"]["frameState"])
                            failures.Add(group.Key + " tick" + tick + " action/state: " + actual.Frame.N + "/" + actual.Frame.D?.state + " beforeSerial=" + actionBeforeSerial);
                    }
                }
                finally
                {
                    world.BeginBattleShutdown();
                    Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                }
                cases++;
            }
            File.WriteAllText(Output + "type" + type + ".json", JsonConvert.SerializeObject(new { cases, failures }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(32));
            Assert.That(failures, Is.Empty, string.Join("\n", failures.Take(8)));
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06State9998PlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_State9998Play.request";
        private const string Result = "Temp/NTSD28_Q06_State9998Play.result.json";
        static NTSD28Q06State9998PlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var report = new Report { tick = driver.CurrentTickIndex, before = world.ObjectCount };
            try
            {
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, saved), Is.True);
                var input = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                var entity = world.FindEntityByRuntimeSlotForQuery(0);
                Assert.That(entity?.Renderer, Is.Not.Null);
                try
                {
                    // The serial sweep consumed the current descriptor; keep shared DAT/cache data immutable.
                    entity.Frame.D = new LF2FrameData { frameId = entity.Frame.N, state = 9998, wait = 100, next = entity.Frame.N };
                    world.SerialTickAll(report.tick);
                    report.stateAfterSerial = entity.Frame.D?.state ?? -1;
                    report.survived = ReferenceEquals(world.FindEntityByRuntimeSlotForQuery(0), entity);
                    Assert.That(report.stateAfterSerial, Is.EqualTo(9998));
                    Assert.That(report.survived, Is.True);
                    Assert.That(world.ObjectCount, Is.EqualTo(report.before));
                }
                finally
                {
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    report.restored = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum == checksum;
                    report.after = world.ObjectCount;
                }
                Assert.That(report.restored, Is.True);
                Assert.That(report.after, Is.EqualTo(report.before));
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Legacy-content real Scene, current descriptor state9998 fed to actual SerialTickAll; no shared DAT mutation, checksum restore. Does not certify full input/physics tick or HP0 frame behavior.";
            public int tick, before, after, stateAfterSerial;
            public bool survived, restored;
        }
    }
}
#endif
