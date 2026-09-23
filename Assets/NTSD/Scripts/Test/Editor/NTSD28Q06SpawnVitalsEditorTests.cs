#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q06SpawnVitalsEditorTests
    {
        private const string Artifact = "artifacts/diagnostics/NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001/";

        [Test]
        public void BirthValuesMatchOriginalMaterializer()
        {
            var apply = Resolve();
            int count = 0;
            foreach (string line in File.ReadLines(Artifact + "native-vitals.tsv").Skip(1))
            {
                int[] v = line.Split('\t').Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                var child = new LF2Character { ObjectId = v[0] };
                child.FrameCache.Load(Wrapper(v[0], v[1], v[5], v[6], v[7] != 0, v[8]));
                child.Health.HP = -37; child.Health.PP = -39;
                child.Runtime.DisplayScore1F0 = 9; child.Runtime.DisplayCurrentHpStep204 = 8;
                apply(child, new ObjectPoint { oid = v[0], hp = v[3], mp = v[4], action = 0, kind = 1 });
                Assert.That(Values(child), Is.EqualTo(v.Skip(9).ToArray()), line);
                count++;
            }
            Assert.That(count, Is.EqualTo(3716));
        }

        [TestCase(false, 5)]
        [TestCase(true, 5)]
        [TestCase(false, 77)]
        [TestCase(true, 77)]
        public void BothPostInitCallersApplyExplicitVitalsAndPercentages(bool presentation, int oid)
        {
            var child = new LF2Character { ObjectId = oid };
            child.FrameCache.Load(Wrapper(oid, 0, 50, 150, true, 0));
            var point = new ObjectPoint { oid = oid, hp = 101, mp = 103, action = 0, kind = 1 };
            GameObject host = null;
            try
            {
                if (presentation)
                {
                    host = new GameObject("SpawnVitalsPostInitFixture") { hideFlags = HideFlags.HideAndDontSave };
                    host.SetActive(false);
                    var factory = host.AddComponent<LF2ObjectPointFactory>();
                    typeof(LF2ObjectPointFactory).GetMethod("PostInitLiving", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(factory, new object[] { child, null, point, 0, 0f, false });
                }
                else BattleLogicEntityFactory.PostInitLiving(child, null, point, 0, 0, false);
                Assert.That(Values(child), Is.EqualTo(new[] { 50, 50, 50, 154, 0, 50, 50, 0, 0, 0, 0, 0, 0 }));
            }
            finally { if (host != null) UnityEngine.Object.DestroyImmediate(host); }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void RealLogicFactoryInitializesBeforeReturningAndReusesPool(int type)
        {
            var wrapper = Wrapper(31990, type, 50, 150, false, 0);
            var resolver = new RuntimeCharacterConfigResolver(_ => wrapper);
            var world = new SimulationWorld(resolver);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31990, type, "birth-vitals.dat") }, _ => wrapper);
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    var point = new ObjectPoint { oid = 31990, kind = 1, action = 0, hp = i == 0 ? 101 : 201, mp = 103 };
                    var task = new OPointCreateTask { targetWorld = world, opoint = point, dir = "right", preserveActionZero = true };
                    LF2Entity child = world.LogicEntityFactory.Create(task, out var failure);
                    Assert.That(child, Is.Not.Null, failure.ToString());
                    int hp = i == 0 ? 50 : 100;
                    Assert.That(Values(child), Is.EqualTo(new[] { hp, hp, hp, 154, 154, hp, hp, 0, 0, 0, 0, 0, 0 }));
                    child.Runtime.DisplayScore1F0 = 77; child.Runtime.DisplayDamageStep1FC = 33;
                    child.FreeEntityLikeExe();
                    Assert.That(world.ObjectCount, Is.Zero);
                }
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [Test]
        public void WarmBirthWriterDoesNotAllocateOrOverwriteUnrelatedMirrors()
        {
            var apply = Resolve();
            var child = new LF2Character { ObjectId = 77 };
            child.FrameCache.Load(Wrapper(77, 0, 50, 150, false, 0));
            child.Health.MP = 91; child.Health.MaxPP = 92; child.Health.PPBound = 93;
            child.Runtime.InputScoreTotal348 = 94;
            var point = new ObjectPoint { oid = 77, hp = 101, mp = 103 };
            for (int i = 0; i < 16; i++) apply(child, point);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++) apply(child, point);
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
            Assert.That(new[] { child.Health.MP, child.Health.MaxPP, child.Health.PPBound, child.Runtime.InputScoreTotal348 }, Is.EqualTo(new[] { 91, 92, 93, 94 }));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ActualLoganDefinitionsMatch329NativeBirths(BattleRuntimeProfile profile)
        {
            var apply = Resolve();
            int count = 0;
            NTSD.EditorTools.NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime",
                NTSD.EditorTools.NTSD28UnityRawCaptureEditor.DefaultScenario, profile, 3, (driver, inputs, identity) =>
            {
                Assert.That(identity.CatalogFingerprint.ToString("X16"), Is.EqualTo("96DE8D089438B1B8"));
                foreach (string line in File.ReadLines(Artifact + "native-actual.tsv").Skip(1))
                {
                    int[] v = line.Split('\t').Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                    var wrapper = driver.World.RuntimeDataCatalog.GetCharacterConfig(v[0]);
                    Assert.That(wrapper, Is.Not.Null);
                    var child = new LF2Character { ObjectId = v[0] };
                    child.FrameCache.Load(wrapper);
                    apply(child, new ObjectPoint { oid = v[0], action = v[2], hp = v[3], mp = v[4], kind = 1 });
                    Assert.That(Values(child), Is.EqualTo(v.Skip(9).ToArray()), "Actual Logan oid=" + v[0]);
                    count++;
                }
            });
            Assert.That(count, Is.EqualTo(329), "OID0 is intentionally not materialized by native OPoint.");
            File.WriteAllText(Artifact + profile + "-actual-births.json", JsonConvert.SerializeObject(new { profile, count, catalog = "96DE8D089438B1B8" }));
        }

        private static Action<LF2Entity, ObjectPoint> Resolve()
        {
            Type type = typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.Ecs.BattleSpawnVitalsWriter");
            MethodInfo method = type?.GetMethod("Apply", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Native OPoint birth resource writer missing.");
            return (Action<LF2Entity, ObjectPoint>)Delegate.CreateDelegate(typeof(Action<LF2Entity, ObjectPoint>), method);
        }

        internal static int[] Values(LF2Entity entity)
        {
            var r = entity.Runtime;
            return new[] { r.HP, r.HPBound, r.HP3, r.PP, r.MPMax, r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208,
                r.DisplayScore1F0, r.DisplayDamageTotal1F8, r.DisplayScoreStep1F4, r.DisplayDamageStep1FC, r.DisplayCurrentHpStep204, r.DisplayEffectiveMaxHpStep20C };
        }

        internal static LF2CharacterDataWrapper Wrapper(int oid, int type, int ohp, int omp, bool maxPresent, int maxMp)
        {
            var fields = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("ohp", ohp.ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>("omp", omp.ToString(CultureInfo.InvariantCulture)),
            };
            if (maxPresent) fields.Add(new KeyValuePair<string, string>("max_mp", maxMp.ToString(CultureInfo.InvariantCulture)));
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = 17,
                NativeMetadata = new LoganDefinitionMetadata(new LoganDefinitionFieldSet(Array.Empty<KeyValuePair<string, string>>()), new LoganDefinitionFieldSet(fields)),
            };
            data.frames.Add(new LF2FrameData { frameId = 0, state = 0, wait = 100, next = 0 });
            return new LF2CharacterDataWrapper(oid, data);
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06SpawnVitalsPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_SpawnVitalsPlay.request";
        private const string Result = "Temp/NTSD28_Q06_SpawnVitalsPlay.result.json";
        static NTSD28Q06SpawnVitalsPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var report = new Report { tick = driver.CurrentTickIndex, objectsBefore = world.ObjectCount };
            try
            {
                var parent = world.FindEntityByRuntimeSlotForQuery(0);
                Assert.That(parent?.Renderer, Is.Not.Null);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, saved), Is.True);
                var checkInput = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tick, checkInput).OverallChecksum;
                report.originalLogicOnly = world.UsesLogicOnlyEntityMaterialization;
                LF2Entity child = null;
                try
                {
                    for (int route = 0; route < 2; route++)
                    for (int birth = 0; birth < 2; birth++)
                    {
                        world.SetLogicOnlyEntityMaterialization(route == 0);
                        int hp = birth == 0 ? 101 : 201;
                        var point = new ObjectPoint { oid = parent.ObjectId, kind = 1, action = parent.Runtime.Frame, hp = hp, mp = 103 };
                        var task = new OPointCreateTask { targetWorld = world, parent = parent, opoint = point, dir = "right", preserveActionZero = true };
                        child = route == 0
                            ? world.LogicEntityFactory.Create(task, out _)
                            : LF2ObjectPointFactory.Instance.CreateObjectImmediate(task);
                        Assert.That(child, Is.Not.Null, "Full materializer route=" + route);
                        Assert.That(child.Renderer != null, Is.EqualTo(route == 1));
                        var stats = child.FrameCache.Wrapper.characterData.NativeMetadata?.Stats;
                        Assert.That(stats?.Int32OrDefault("ohp", 0) ?? 0, Is.Zero, "Current-content fixture must have no HP percentage.");
                        Assert.That(stats?.Int32OrDefault("omp", 0) ?? 0, Is.Zero, "Current-content fixture must have no MP percentage.");
                        int maxMp = stats?.Int32OrDefault("max_mp", 103) ?? 103;
                        var values = NTSD28Q06SpawnVitalsEditorTests.Values(child);
                        Assert.That(values, Is.EqualTo(new[] { hp, hp, hp, 103, maxMp, hp, hp, 0, 0, 0, 0, 0, 0 }));
                        report.births.Add(new Birth { route = route, ordinal = birth, slot = child.Runtime.SlotIndex, values = values });
                        child.FreeEntityLikeExe();
                        child = null;
                    }
                }
                finally
                {
                    try { child?.FreeEntityLikeExe(); }
                    finally { world.SetLogicOnlyEntityMaterialization(report.originalLogicOnly); }
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, checkInput).OverallChecksum, Is.EqualTo(checksum));
                    report.restoredLogicOnly = world.UsesLogicOnlyEntityMaterialization;
                    Assert.That(report.restoredLogicOnly, Is.EqualTo(report.originalLogicOnly));
                    report.restored = true;
                }
                report.objectsAfter = world.ObjectCount;
                Assert.That(report.objectsAfter, Is.EqualTo(report.objectsBefore));
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Current legacy-content real Scene; paused idle materialization mode selected per route and restored; full logic and renderer materializers each spawn twice, canonical birth values checked before any tick, children released and World checksum restored. No natural skill or formal sprite parity claim.";
            public int tick, objectsBefore, objectsAfter;
            public bool restored, originalLogicOnly, restoredLogicOnly;
            public List<Birth> births = new List<Birth>();
        }
        private sealed class Birth
        {
            public int route, ordinal, slot;
            public int[] values;
        }
    }
}
#endif
