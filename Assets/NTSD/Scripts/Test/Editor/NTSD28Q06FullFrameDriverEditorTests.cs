#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FullFrameDriverEditorTests
    {
        internal const string Witness = "artifacts/diagnostics/NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-SOURCE-WITNESS-001/native.jsonl";
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-FRAME-FULL-DRIVER-UNITY-001/";
        private static JObject[] rows;
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        private static IEnumerable<TestCaseData> Cases()
        {
            foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
            foreach (bool optimized in new[] { false, true })
            foreach (string group in new[] { "next", "high", "cost", "modifiers", "hold" })
                yield return new TestCaseData(profile, optimized, group);
        }

        [TestCaseSource(nameof(Cases))]
        public void FullDriverMatchesInitialAndThreeOriginalTicks(BattleRuntimeProfile profile, bool optimized, string group)
        {
            rows ??= File.ReadLines(Witness).Skip(1).Select(JObject.Parse).ToArray();
            var differences = new List<string>();
            int cases = 0, ticks = 0;
            ulong legacyCalls = 0;
            foreach (var row in rows.Where(r => (string)r["group"] == group))
            {
                using (var fixture = new Fixture(row, profile, optimized))
                {
                    string label = "case " + row["index"];
                    CompareEntity(fixture.World, row["initial"], label + " initial", differences);
                    var driver = new NTSDBattleTickSystem(fixture.World);
                    foreach (var expected in row["ticks"])
                    {
                        int tick = (int)expected["tick"];
                        var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                        fixture.World.NativeRandom.SetDiagnosticCallObserver(observer);
                        ulong before = fixture.World.Rng.CallCount;
                        driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                        fixture.World.NativeRandom.SetDiagnosticCallObserver(null);
                        ulong delta = fixture.World.Rng.CallCount - before;
                        legacyCalls += delta;
                        string stepLabel = label + " tick " + tick;
                        if (delta != 1) differences.Add(stepLabel + " C17 legacy calls=" + delta + " expected 1");
                        if (observer.CrtCalls != (int)expected["crtCalls"] || !JToken.DeepEquals(JArray.FromObject(observer.Calls), expected["calls"]))
                            differences.Add(stepLabel + " native RNG differs");
                        CompareEntity(fixture.World, expected["entity"], stepLabel, differences);
                        CompareSounds(fixture.World, expected["audio"], stepLabel, differences);
                        ticks++;
                    }
                }
                cases++;
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + optimized + "-" + group + ".json",
                JsonConvert.SerializeObject(new { cases, ticks, legacyCalls, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(group == "next" ? 168 : group == "high" ? 48 : group == "cost" ? 144 : group == "modifiers" ? 72 : 18));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        [TestCase(BattleRuntimeProfile.Authority400, false, false)]
        [TestCase(BattleRuntimeProfile.Authority400, false, true)]
        [TestCase(BattleRuntimeProfile.Authority400, true, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true, true)]
        public void HighAndEncodedStatesReplayAfterLocalOrFreshTransferredSnapshot(BattleRuntimeProfile profile, bool optimized, bool transfer)
        {
            rows ??= File.ReadLines(Witness).Skip(1).Select(JObject.Parse).ToArray();
            var selected = rows.Where(r =>
            {
                var c = r["input"];
                string group = (string)r["group"];
                return (int)c["type"] == 0 &&
                    ((group == "high" && (int)c["slot"] == 70 && (int)c["action"] == 998 && (int)c["declared"] == 0 && (int)c["next"] == 0) ||
                     (group == "cost" && (int)c["slot"] == 0 && (int)c["fallback"] == 1101 &&
                        (((int)c["hp"] == 5 && (int)c["mp"] == 9) || ((int)c["hp"] == 6 && (int)c["mp"] == 8))) ||
                     (group == "next" && (int)c["slot"] == 0 && (int)c["next"] == 1299 && (int)c["y"] == 0));
            }).ToArray();
            Assert.That(selected.Length, Is.EqualTo(4));
            var differences = new List<string>();
            foreach (var row in selected)
            {
                using (var source = new Fixture(row, profile, optimized))
                {
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var system = new NTSDBattleTickSystem(source.World);
                    system.RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    var snapshot = source.World.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(source.World.TryCaptureBattleStateSnapshot(identity, 1, snapshot), Is.True);
                    var checksums = new Dictionary<int, string>();
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        var input = new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>());
                        system.RunReleaseTick(tick, false, input);
                        checksums.Add(tick, source.World.CaptureLockstepChecksumSnapshot(tick, input).OverallChecksum);
                    }
                    Fixture target = transfer ? new Fixture(row, profile, optimized, spawn: false) : null;
                    try
                    {
                        var world = target?.World ?? source.World;
                        if (transfer) snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
                        Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                        var replay = new NTSDBattleTickSystem(world);
                        for (int tick = 2; tick <= 3; tick++)
                        {
                            var input = new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>());
                            replay.RunReleaseTick(tick, false, input);
                            string label = "replay " + row["index"] + " tick " + tick;
                            CompareEntity(world, row["ticks"][tick - 1]["entity"], label, differences);
                            CompareSounds(world, row["ticks"][tick - 1]["audio"], label, differences);
                            if (world.CaptureLockstepChecksumSnapshot(tick, input).OverallChecksum != checksums[tick])
                                differences.Add(label + " full checksum mismatch");
                        }
                    }
                    finally { target?.Dispose(); }
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "replay-" + profile + "-" + optimized + "-" + transfer + ".json",
                JsonConvert.SerializeObject(new { cases = selected.Length, differences }, Formatting.Indented));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
        }

        internal static void CompareEntity(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            var entities = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            bool absent = expected == null || expected.Type == JTokenType.Null;
            if (entities.Count() != (absent ? 0 : 1)) differences.Add(label + " entity count=" + entities.Count());
            if (absent) return;
            int slot = (int)expected["raw"]["slot"];
            var actual = entities.SingleOrDefault(e => (int)e["slot"] == slot);
            if (actual == null) { differences.Add(label + " missing source " + slot); return; }
            foreach (var field in Flatten(expected["raw"], ""))
            {
                if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                var value = actual.SelectToken(field.Key);
                bool number = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float;
                bool equal = number ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value : JToken.DeepEquals(value, field.Value);
                if (!equal) differences.Add(label + " " + field.Key + "=" + value + " expected " + field.Value);
            }
            var entity = world.FindEntityByRuntimeSlotForQuery(slot);
            if (entity.Runtime.NativeSoundActionLatch != (int)expected["soundLatch"]) differences.Add(label + " sound latch=" + entity.Runtime.NativeSoundActionLatch + " expected " + expected["soundLatch"]);
            if (entity.Runtime.InputHpConsumedTotal34C != (int)expected["hpConsumed"]) differences.Add(label + " HP consumed differs");
            if (entity.Runtime.InputMpConsumedTotal350 != (int)expected["mpConsumed"]) differences.Add(label + " MP consumed differs");
        }

        internal static void CompareSounds(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            if (world.PendingSounds.Count != expected.Count()) differences.Add(label + " audio count=" + world.PendingSounds.Count + " expected " + expected.Count());
            for (int i = 0; i < Math.Min(world.PendingSounds.Count, expected.Count()); i++)
            {
                var sound = world.PendingSounds[i];
                var native = expected[i];
                if ((int)native["source"] != 1 || (int)native["channel"] != -1 || sound.Cue != (string)native["path"] || sound.WorldX != (int)native["x"])
                    differences.Add(label + " audio " + i + "=" + sound.Cue + "@" + sound.WorldX + " expected " + native);
            }
        }

        private static IEnumerable<KeyValuePair<string, JToken>> Flatten(JToken token, string prefix)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                    foreach (var field in Flatten(property.Value, prefix.Length == 0 ? property.Name : prefix + "." + property.Name)) yield return field;
            }
            else yield return new KeyValuePair<string, JToken>(prefix, token);
        }

        internal sealed class Fixture : IDisposable
        {
            internal readonly SimulationWorld World;
            private readonly bool renderer;

            internal Fixture(JObject row, BattleRuntimeProfile profile, bool optimized, bool renderer = false, bool spawn = true)
            {
                this.renderer = renderer;
                var c = (JObject)row["input"];
                int I(string key) => (int)c[key];
                string key = string.Join("/", new[] { "type", "action", "declared", "next", "destHp", "destMp", "fallback", "recmp" }.Select(k => I(k)));
                if (!wrappers.TryGetValue(key, out var wrapper))
                {
                    var stats = new LoganDefinitionFieldSet(new[] { new KeyValuePair<string, string>("recmp", I("recmp").ToString(CultureInfo.InvariantCulture)) });
                    var bmp = new LoganDefinitionFieldSet(new[]
                    {
                        new KeyValuePair<string, string>("weapon_hp", "17"),
                        new KeyValuePair<string, string>("jump_height", "-9.5"),
                        new KeyValuePair<string, string>("jump_distance", "4.25"),
                        new KeyValuePair<string, string>("jump_distancez", "2.5")
                    });
                    var data = new LF2CharacterData
                    {
                        type_sub = I("type"), weapon_hp = 17, recmp = I("recmp"), jump_height = -9.5f, jump_distance = 4.25f, jump_distancez = 2.5f,
                        NativeMetadata = new LoganDefinitionMetadata(bmp, stats)
                    };
                    if (I("action") != 0) data.frames.Add(new LF2FrameData { frameId = 0, wait = 100, next = 0, UsesLoganFrameNumbers = true });
                    if (I("declared") != 0)
                    {
                        var current = new LF2FrameData { frameId = I("action"), wait = 0, next = I("next"), sound = "current.wav", UsesLoganFrameNumbers = true };
                        current.SealFrameSounds(new[] { "current.wav" });
                        data.frames.Add(current);
                    }
                    var destination = new LF2FrameData { frameId = 7, wait = 0, next = I("fallback"), hp = I("destHp"), mp = I("destMp"), sound = "destination.wav", UsesLoganFrameNumbers = true };
                    destination.SealFrameSounds(new[] { "destination.wav" });
                    data.frames.Add(destination);
                    wrapper = new LF2CharacterDataWrapper(77, data);
                    wrappers.Add(key, wrapper);
                }
                World = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
                World.SetLogicOnlyEntityMaterialization(!renderer);
                World.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, I("type"), "full-frame.dat") }, _ => wrapper);
                World.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(optimized ? BattleEcsCharacterFrameTickPassMode.DataOriented : BattleEcsCharacterFrameTickPassMode.Legacy);
                World.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(optimized ? BattleEcsCharacterFrameAdvancePassMode.DataOriented : BattleEcsCharacterFrameAdvancePassMode.Legacy);
                World.Runtime.FunctionKeys.ResetForBattle(I("local") != 0);
                if (!spawn) return;
                var task = new OPointCreateTask { targetWorld = World, requiredRuntimeSlot = I("slot"), dir = "right", nativeWeaponPieceSpawn = true, preserveActionZero = true, opoint = new ObjectPoint { oid = 77, action = 0 } };
                var entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : World.LogicEntityFactory.Create(task, out _);
                Assert.That(entity, Is.Not.Null, "Full frame source creation");
                entity.WriteCurrentFrameId(I("action"));
                entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(I("action"));
                entity.Frame.Prev = I("action");
                entity.Trans.SyncDirectFrameData(entity.Frame.D.wait, entity.Frame.D.next, I("action"));
                entity.SyncCollisionSnapshotToCurrentFrame();
                entity.Health.HP = I("hp"); entity.Health.PP = I("mp"); entity.Health.HPBound = 200;
                entity.Runtime.InputModeCostMultiplier30 = I("mode"); entity.Runtime.InputDoubleCost19C = I("doubled"); entity.Runtime.InputCostWaived1B4 = I("waived");
                entity.FrameDelay = I("hold");
                entity.Runtime.SetPosition(100.25, I("y"), 200.75);
                entity.Runtime.XInt = 100; entity.Runtime.YInt = I("y"); entity.Runtime.ZInt = 200;
                entity.Runtime.SetVelocity(1.25, I("y") == 0 ? 0 : -1.25, .75);
                World.NativeRandom.ResetFromSeed(42);
            }

            public void Dispose()
            {
                if (renderer)
                {
                    for (int slot = 0; slot < World.RuntimeSlotCapacityForDiagnostics; slot++)
                        World.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                }
                NTSD28Q06State18SpawnEditorTests.Shutdown(World);
            }
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06FullFrameDriverPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_FullFrameDriverPlay.request";

        static NTSD28Q06FullFrameDriverPlayProbe() { EditorApplication.update += Poll; }

        private static bool Selected(JObject row)
        {
            var c = row["input"];
            string group = (string)row["group"];
            int type = (int)c["type"], slot = (int)c["slot"];
            return (group == "next" && type == 0 && ((slot == 0 && (int)c["y"] == 0) ||
                    (slot == 70 && (int)c["y"] == -10 && new[] { -999, 999, 857, 998, 1101 }.Contains((int)c["next"])))) ||
                (group == "high" && type == 0 && slot == 70) ||
                (group == "cost" && type == 0 && slot == 0) ||
                (group == "hold" && type == 3 && slot == 70) ||
                (group == "modifiers" && ((int)c["recmp"] == 25 || (int)c["recmp"] == 150) &&
                    (int)c["mode"] == 200 && (int)c["doubled"] == 1 && (int)c["waived"] == 0 && (int)c["local"] == 1);
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            string status = "FAIL", error = null;
            int cases = 0, ticks = 0;
            var differences = new List<string>();
            try
            {
                var selected = File.ReadLines(NTSD28Q06FullFrameDriverEditorTests.Witness).Skip(1).Select(JObject.Parse).Where(Selected).ToArray();
                Assert.That(selected.Length, Is.EqualTo(56));
                foreach (bool renderer in new[] { false, true })
                foreach (bool optimized in new[] { false, true })
                foreach (var row in selected)
                {
                    using (var fixture = new NTSD28Q06FullFrameDriverEditorTests.Fixture(row, BattleRuntimeProfile.Authority400, optimized, renderer))
                    {
                        string label = "renderer=" + renderer + " optimized=" + optimized + " case=" + row["index"];
                        NTSD28Q06FullFrameDriverEditorTests.CompareEntity(fixture.World, row["initial"], label + " initial", differences);
                        var system = new NTSDBattleTickSystem(fixture.World);
                        foreach (var expected in row["ticks"])
                        {
                            int tick = (int)expected["tick"];
                            var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                            fixture.World.NativeRandom.SetDiagnosticCallObserver(observer);
                            system.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                            fixture.World.NativeRandom.SetDiagnosticCallObserver(null);
                            NTSD28Q06FullFrameDriverEditorTests.CompareEntity(fixture.World, expected["entity"], label + " tick " + tick, differences);
                            NTSD28Q06FullFrameDriverEditorTests.CompareSounds(fixture.World, expected["audio"], label + " tick " + tick, differences);
                            Assert.That(JToken.DeepEquals(JArray.FromObject(observer.Calls), expected["calls"]), Is.True, label + " RNG");
                            Assert.That(observer.CrtCalls, Is.EqualTo((int)expected["crtCalls"]));
                            ticks++;
                        }
                        Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
                        cases++;
                    }
                    Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                }
                Assert.That(cases, Is.EqualTo(224));
                Assert.That(ticks, Is.EqualTo(672));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_FullFrameDriverPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, ticks, differences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, isolated synthetic high-action/cost worlds; both actual factories and backends, complete three ticks; no image or physical-input certification."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
