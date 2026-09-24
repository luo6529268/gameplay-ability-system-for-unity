#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeWeaponPieceEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-WEAPON-PIECE-SOURCE-WITNESS-001/";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001/";

        [TestCase(false, false, 0)]
        [TestCase(false, true, 0)]
        [TestCase(true, false, 0)]
        [TestCase(true, true, 0)]
        [TestCase(false, false, 93)]
        [TestCase(false, true, 93)]
        [TestCase(true, false, 93)]
        [TestCase(true, true, 93)]
        public void FragmentBirthPreservesIndependentSourceIntegerHistory(bool initialized, bool configuredView, int witnessIndex)
        {
            var row = JObject.Parse(File.ReadLines(Witness + "synthetic.jsonl").Skip(witnessIndex).First());
            var input = row["input"];
            int I(string key) => (int)input[key];
            var parent = MakeWrapper(I("oid"), I("type"), ParentDat(I("block"), I("team")));
            const string childDat = "<bmp_begin>\nweapon_hp: 17\n<bmp_end>\n" +
                "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [I("oid")] = parent,
                [999] = MakeWrapper(999, I("childType"), childDat),
                [777] = MakeWrapper(777, I("childType"), childDat),
            };
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView) world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(wrappers.Select(pair =>
                new ObjectDefinition(pair.Key, pair.Value.characterData.type_sub, "piece-test.dat")).ToArray(), id => wrappers[id]);
            try
            {
                var source = RegisterParent(world, parent, input);
                if (initialized)
                {
                    source.Runtime.SetSourceRulePosition(-100.75, 87.5);
                    source.Runtime.SourceRuleXInt = -102;
                    source.Runtime.SourceRuleZInt = 85;
                }
                int countBefore = world.ObjectCount;
                var observer = new Observer();
                world.NativeRandom.ResetFromSeed((uint)input["seed"]);
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                Assert.That(source.TryRunLatePostOpointCleanupPhase(), Is.True);
                Assert.That(JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"]), Is.True);
                Assert.That(observer.CrtCalls, Is.Zero);
                var random = world.NativeRandom.CaptureScalarState();
                Assert.That(random.CrtState, Is.EqualTo((uint)row["rng"]["crtAfter"]));
                Assert.That(random.SynchronizedCalls, Is.EqualTo((ulong)row["rng"]["syncCalls"]));
                Assert.That(random.SynchronizedIndex, Is.EqualTo((int)row["rng"]["index"]));
                Assert.That(random.SynchronizedCounter, Is.EqualTo((int)row["rng"]["counter"]));
                Assert.That(random.SynchronizedTableHash, Is.EqualTo((ulong)row["rng"]["tableHash"]));
                var births = row["events"].Where(e => e["entity"] is JObject).ToArray();
                Assert.That(births, Is.Not.Empty);
                Assert.That(world.ObjectCount - countBefore, Is.EqualTo(births.Length));
                foreach (var birth in births)
                {
                    var child = world.FindEntityByRuntimeSlotForQuery((int)birth["slot"]);
                    Assert.That(child, Is.Not.Null);
                    var position = birth["entity"]["position"];
                    int dx = (int)position["x"] - 100;
                    int dz = (int)position["z"] - 200;
                    double battleX = 100 + dx * world.FixedViewRunDistanceScale;
                    double battleZ = 200 + dz * world.FixedViewRunVerticalDistanceScale;
                    Assert.That(child.Runtime.X, Is.EqualTo(battleX).Within(1e-10));
                    Assert.That(child.Runtime.Z, Is.EqualTo(battleZ).Within(1e-10));
                    Assert.That(child.Runtime.XInt, Is.EqualTo((int)Math.Round(battleX, MidpointRounding.ToEven)));
                    Assert.That(child.Runtime.ZInt, Is.EqualTo((int)Math.Round(battleZ, MidpointRounding.ToEven)));
                    Assert.That(child.Runtime.SourceRulePositionInitialized, Is.EqualTo(initialized));
                    Assert.That(child.Runtime.SourceRuleX, Is.EqualTo(initialized ? -102 + dx : 0));
                    Assert.That(child.Runtime.SourceRuleZ, Is.EqualTo(initialized ? 85 + dz : 0));
                    Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo(initialized ? -102 + dx : 0));
                    Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo(initialized ? 85 + dz : 0));
                }
                Assert.That(source.Runtime.SourceRulePositionInitialized, Is.EqualTo(initialized));
                Assert.That(source.Runtime.SourceRuleX, Is.EqualTo(initialized ? -100.75 : 0));
                Assert.That(source.Runtime.SourceRuleZ, Is.EqualTo(initialized ? 87.5 : 0));
                Assert.That(source.Runtime.SourceRuleXInt, Is.EqualTo(initialized ? -102 : 0));
                Assert.That(source.Runtime.SourceRuleZInt, Is.EqualTo(initialized ? 85 : 0));
                Assert.That(source.Runtime.X, Is.EqualTo(100.25));
                Assert.That(source.Runtime.Z, Is.EqualTo(200.75));
                Assert.That(source.Runtime.XInt, Is.EqualTo(100));
                Assert.That(source.Runtime.ZInt, Is.EqualTo(200));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [TestCase(false, 0, 50)]
        [TestCase(true, 0, 50)]
        [TestCase(false, 93, 50)]
        [TestCase(true, 93, 50)]
        public void FragmentBirthRelativeXZUsesViewRatio(
            bool configuredView,
            int witnessIndex,
            int fragmentSlot)
        {
            var row = JObject.Parse(File.ReadLines(Witness + "synthetic.jsonl")
                .Skip(witnessIndex).First());
            var input = row["input"];
            int I(string key) => (int)input[key];
            var parent = MakeWrapper(I("oid"), I("type"), ParentDat(I("block"), I("team")));
            const string childDat = "<bmp_begin>\nweapon_hp: 17\n<bmp_end>\n" +
                "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>
            {
                [I("oid")] = parent,
                [999] = MakeWrapper(999, I("childType"), childDat),
                [777] = MakeWrapper(777, I("childType"), childDat),
            };
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(wrappers.Select(pair =>
                new ObjectDefinition(pair.Key, pair.Value.characterData.type_sub,
                    "piece-test.dat")).ToArray(), id => wrappers[id]);
            try
            {
                var source = RegisterParent(world, parent, input);
                world.NativeRandom.ResetFromSeed((uint)input["seed"]);
                Assert.That(source.TryRunLatePostOpointCleanupPhase(), Is.True);
                var fragment = world.FindEntityByRuntimeSlotForQuery(fragmentSlot);
                Assert.That(fragment, Is.Not.Null);
                var expectedEvent = row["events"].First(e =>
                    (int)e["slot"] == fragmentSlot && e["entity"] is JObject);
                var formalPosition = expectedEvent["entity"]["position"];
                double expectedX = source.Runtime.XInt +
                    ((double)formalPosition["x"] - source.Runtime.XInt) *
                    (configuredView ? 2048.0 / 1333.0 : 1.0);
                double expectedZ = source.Runtime.ZInt +
                    ((double)formalPosition["z"] - source.Runtime.ZInt) *
                    (configuredView ? 1152.0 / 730.0 : 1.0);
                Assert.That(fragment.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
                Assert.That(fragment.Runtime.XInt,
                    Is.EqualTo((int)Math.Round(expectedX, MidpointRounding.ToEven)));
                Assert.That(fragment.Runtime.Z, Is.EqualTo(expectedZ).Within(0.000001));
                Assert.That(fragment.Runtime.ZInt,
                    Is.EqualTo((int)Math.Round(expectedZ, MidpointRounding.ToEven)));
                Assert.That(fragment.Runtime.YInt, Is.EqualTo((int)formalPosition["y"]));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason),
                    Is.True, reason);
            }
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void OriginalTwoStageVectorsMatchFullBirthsAndRandom(BattleRuntimeProfile profile)
        {
            var differences = new List<string>();
            int cases = 0, births = 0;
            foreach (string line in File.ReadLines(Witness + "synthetic.jsonl"))
            {
                var row = JObject.Parse(line);
                var input = row["input"];
                int I(string key) => (int)input[key];
                var parent = MakeWrapper(I("oid"), I("type"), ParentDat(I("block"), I("team")));
                string childDat = "<bmp_begin>\nweapon_hp: 17\n<bmp_end>\n";
                if (I("stats") != 0)
                    childDat += "<stats> ohp: 25 omp: 50" + (I("maxMp") == -999 ? "" : " max_mp: " + I("maxMp")) + " <stats_end>\n";
                childDat += "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
                var wrappers = new Dictionary<int, LF2CharacterDataWrapper> { [I("oid")] = parent };
                if (I("targets") >= 1) wrappers[999] = MakeWrapper(999, I("childType"), childDat);
                if (I("targets") >= 2) wrappers[777] = MakeWrapper(777, I("childType"), childDat);
                var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
                world.SetLogicOnlyEntityMaterialization(true);
                world.PrepareRuntimeDataCatalogForBattle(wrappers.Select(pair =>
                    new ObjectDefinition(pair.Key, pair.Value.characterData.type_sub, "piece-test.dat")).ToArray(), id => wrappers[id]);
                try
                {
                    LF2Entity source = RegisterParent(world, parent, input);
                    if (I("freeSlots") >= 0)
                    {
                        int remaining = I("freeSlots");
                        for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                        {
                            if (slot == I("sourceSlot")) continue;
                            if (remaining-- > 0) continue;
                            var blocker = new LF2Character { ObjectId = I("oid") };
                            blocker.FrameCache.Load(parent);
                            blocker.SetRequiredRuntimeSlot(slot);
                            world.Register(blocker);
                        }
                    }
                    births += Compare(world, source, row, differences, "case " + cases);
                }
                finally
                {
                    world.BeginBattleShutdown();
                    Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                }
                cases++;
            }
            File.WriteAllText(Output + profile + "-birth-comparison.json", JsonConvert.SerializeObject(new { cases, births, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(157));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
            Assert.That(births, Is.EqualTo(762));
        }

        [Test]
        public void PooledTaskClearsNativeBirthFlag()
        {
            var task = new OPointCreateTask { nativeWeaponPieceSpawn = true };
            task.Clear();
            Assert.That(task.nativeWeaponPieceSpawn, Is.False);
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ActualLoganPiecesMatchOriginalFunctions(BattleRuntimeProfile profile)
        {
            var differences = new List<string>();
            int births = 0;
            NTSD.EditorTools.NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime",
                NTSD.EditorTools.NTSD28UnityRawCaptureEditor.DefaultScenario, profile, 3, (driver, inputs, identity) =>
            {
                Assert.That(driver.World.RuntimeDataCatalog.LoganContentIdentity.ObjectDefinitionFingerprint,
                    Is.EqualTo("4EFE1D2A6A51C20742EA839CC5EAC2BA0D09EE9E4A5888E77C8AC35D4AA0C58C"));
                Assert.That(identity.CatalogFingerprint.ToString("X16"), Is.EqualTo("96DE8D089438B1B8"));
                var catalog = driver.World.RuntimeDataCatalog;
                foreach (string line in File.ReadLines(Witness + "formal.jsonl"))
                {
                    var row = JObject.Parse(line);
                    var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
                    world.SetLogicOnlyEntityMaterialization(true);
                    world.PrepareRuntimeDataCatalogForBattle(catalog.ObjectDefinitions, catalog.GetCharacterConfig);
                    try
                    {
                        var source = RegisterParent(world, catalog.GetCharacterConfig((int)row["input"]["oid"]), row["input"]);
                        births += Compare(world, source, row, differences, "formal " + source.ObjectId);
                    }
                    finally
                    {
                        world.BeginBattleShutdown();
                        Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                    }
                }
            });
            File.WriteAllText(Output + profile + "-formal-comparison.json", JsonConvert.SerializeObject(new { births, differences }, Formatting.Indented));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
            Assert.That(births, Is.EqualTo(50));
        }

        private static LF2Entity RegisterParent(SimulationWorld world, LF2CharacterDataWrapper wrapper, JToken input)
        {
            var source = new LF2Character { ObjectId = (int)input["oid"] };
            source.FrameCache.Load(wrapper);
            source.SetRequiredRuntimeSlot((int)input["sourceSlot"]);
            world.Register(source);
            source.WriteCurrentFrameId((int)input["pending"] != 0 ? 1000 : 0);
            source.Frame.D = source.FrameCache.GetNativeFrameDataById(source.Frame.N);
            source.Runtime.X = 100.25; source.Runtime.Y = -20.5; source.Runtime.Z = 200.75;
            source.Runtime.XInt = 100; source.Runtime.YInt = -20; source.Runtime.ZInt = 200;
            source.OwnerEntityIndex = 17; source.RelationTeam = 9;
            source.Runtime.Dir = (int)input["facing"] != 0 ? "left" : "right";
            source.Runtime.LinkState = (int)input["link"];
            source.Runtime.WeaponFlightCounter = (int)input["weaponHp"];
            source.Runtime.NativeLifecycleResolutionPending = (int)input["pending"] != 0;
            source.Runtime.NativeLifecycleCode = (int)input["pending"] != 0 ? 1000 : 0;
            return source;
        }

        private static int Compare(SimulationWorld world, LF2Entity source, JObject row, List<string> differences, string label)
        {
            var observer = new Observer();
            world.NativeRandom.ResetFromSeed((uint)row["input"]["seed"]);
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            int before = world.ObjectCount;
            bool triggered = source.TryRunLatePostOpointCleanupPhase();
            world.NativeRandom.SetDiagnosticCallObserver(null);
            if (triggered != (bool)row["summary"]["triggered"]) differences.Add(label + " break gate");
            if (source.Runtime.NativeLifecycleResolutionPending != (bool)row["summary"]["pending"] ||
                source.Runtime.NativeLifecycleCode != (int)row["summary"]["code"] ||
                source.Runtime.WeaponFlightCounter != (int)row["summary"]["weaponHp"])
                differences.Add(label + " source pending/code/weaponHp");
            if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"])) differences.Add(label + " random per-call sequence");
            if (observer.CrtCalls != 0) differences.Add(label + " unexpected CRT call");
            var random = world.NativeRandom.CaptureScalarState();
            if (random.CrtState != (uint)row["rng"]["crtAfter"] || random.SynchronizedCalls != (ulong)row["rng"]["syncCalls"] ||
                random.SynchronizedIndex != (int)row["rng"]["index"] || random.SynchronizedCounter != (int)row["rng"]["counter"] ||
                random.SynchronizedTableHash != (ulong)row["rng"]["tableHash"]) differences.Add(label + " final random state");
            int expectedBirths = row["events"].Count(e => e["entity"] is JObject);
            if (world.ObjectCount - before != expectedBirths) differences.Add(label + " birth count: " + (world.ObjectCount - before) + " expected " + expectedBirths);
            var entities = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            foreach (var ev in row["events"].Where(e => e["entity"] is JObject))
            {
                var expected = ev["entity"];
                var actual = entities.FirstOrDefault(e => (int)e["slot"] == (int)ev["slot"]);
                if (actual == null) { differences.Add(label + " missing slot " + ev["slot"]); continue; }
                foreach (var field in Flatten(expected, ""))
                {
                    if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                    var value = actual.SelectToken(field.Key);
                    bool equal = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float
                        ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value
                        : JToken.DeepEquals(value, field.Value);
                    if (!equal) differences.Add(label + " slot " + ev["slot"] + " " + field.Key + ": " + value + " expected " + field.Value);
                }
                var entity = world.FindEntityByRuntimeSlotForQuery((int)ev["slot"]);
                if (entity.Runtime.NativeSoundActionLatch != (int)ev["soundLatch"]) differences.Add(label + " sound latch");
                if (entity.Frame.D?.frameId != (int)ev["action"]) differences.Add(label + " native frame descriptor");
            }
            int birthCount = world.ObjectCount - before;
            if (world.PendingSounds.Count != (int)row["summary"]["audioCount"]) differences.Add(label + " broken sound count");
            source.ResolveNativeC25LifecycleForWorldPass();
            if ((source.Runtime.SlotIndex >= 0) != (bool)row["afterLifecycle"]["sourceAlive"])
                differences.Add(label + " source lifecycle");
            return birthCount;
        }

        private static IEnumerable<KeyValuePair<string, JToken>> Flatten(JToken token, string prefix)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                    foreach (var value in Flatten(property.Value, prefix.Length == 0 ? property.Name : prefix + "." + property.Name))
                        yield return value;
            }
            else yield return new KeyValuePair<string, JToken>(prefix, token);
        }

        private static LF2CharacterDataWrapper MakeWrapper(int oid, int type, string text)
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(text);
            var bmp = LoganDefinitionMetadata.CopyFields(dat.Bmp.Properties);
            var stats = LoganDefinitionMetadata.CopyFields(dat.LoganStats?.Properties);
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = bmp.Int32OrDefault("weapon_hp", 0), weapon_broken_sound = text.Contains("break.wav") ? "break.wav" : null,
                NativeMetadata = new LoganDefinitionMetadata(bmp, stats, null,
                    dat.LoganWeaponPiece == null ? null : new LoganWeaponPieceDefinition(dat.LoganWeaponPiece)),
            };
            foreach (var frame in dat.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            return new LF2CharacterDataWrapper(oid, data);
        }

        private static string ParentDat(int block, int team)
        {
            string text = "<bmp_begin>\nweapon_hp: 99 weapon_broken_sound: break.wav\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
            if (block == 0) return text;
            text += "<weapon_piece>\nteam: " + team + "\npiece: 1\namount: 2 oid: -1 act: 10 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n";
            if (block == 2) text += "piece: 1\noid: 777 act: 80 framea: 4 dvx: 6 dvy: -8 dvz: 4\npiece_end:\npiece: 2\namount: 1 oid: 777 act: 109 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n";
            return text + "<weapon_piece_end>\n";
        }

        private sealed class Observer : INTSD28NativeRandomCallObserver
        {
            internal readonly List<long[]> Calls = new List<long[]>();
            internal int CrtCalls;
            public void OnCrtNext(NTSD28NativeCrtCall call) { CrtCalls++; }
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                Calls.Add(new[] { (long)call.CallSite, call.UpperBound, call.Result, call.CounterAfter, call.IndexAfter, (long)call.TotalCalls });
            }
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06NativeWeaponPiecePlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_WeaponPiecePlay.request";
        private const string Result = "Temp/NTSD28_Q06_WeaponPiecePlay.result.json";
        static NTSD28Q06NativeWeaponPiecePlayProbe() { EditorApplication.update += Poll; }

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
                Assert.That(world.RuntimeDataCatalog.GetCharacterConfig(100), Is.Not.Null);
                Assert.That(world.RuntimeDataCatalog.GetCharacterConfig(999), Is.Not.Null);
                Assert.That(world.RuntimeDataCatalog.GetCharacterConfig(100).characterData.NativeMetadata?.WeaponPiece, Is.Null,
                    "This Scene probe declares legacy OID100 with only the builtin family.");
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, saved), Is.True);
                var input = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                bool originalLogicOnly = world.UsesLogicOnlyEntityMaterialization;
                var originalMode = world.BattleEcsCharacterFrameTickPassModeForDiagnostics;
                var originalSlots = new HashSet<int>();
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    if (world.FindEntityByRuntimeSlotIncludingPending(slot) != null) originalSlots.Add(slot);
                try
                {
                    for (int route = 0; route < 2; route++)
                    for (int broken = 0; broken < 2; broken++)
                    {
                        world.SetLogicOnlyEntityMaterialization(route == 0);
                        var existing = new HashSet<int>();
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                            if (world.FindEntityByRuntimeSlotForQuery(slot) != null) existing.Add(slot);
                        var task = new OPointCreateTask
                        {
                            targetWorld = world, opoint = new ObjectPoint { oid = 100, action = 0, kind = 1 },
                            dir = "right", preserveActionZero = true, nativeWeaponPieceSpawn = true,
                            useDirectRuntimePosition = true, directX = 200, directY = -20, directZ = 200,
                            useInitialRuntimeIntPosition = true, initialRuntimeX = 200, initialRuntimeY = -20, initialRuntimeZ = 200,
                            skipPostInitZOffset = true,
                        };
                        var source = route == 0 ? world.LogicEntityFactory.Create(task, out _) : LF2ObjectPointFactory.Instance.CreateObjectImmediate(task);
                        Assert.That(source, Is.Not.Null);
                        int sourceSlot = source.Runtime.SlotIndex;
                        source.Runtime.WeaponFlightCounter = broken != 0 ? -1 : 1;
                        if (broken == 0)
                        {
                            source.WriteCurrentFrameId(1000);
                            source.Frame.D = null;
                        }
                        world.LateEntityUpdateAll(report.tick);
                        Assert.That(world.FindEntityByRuntimeSlotForQuery(sourceSlot), Is.Null);
                        int pieces = 0;
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                        {
                            if (existing.Contains(slot)) continue;
                            var piece = world.FindEntityByRuntimeSlotForQuery(slot);
                            if (piece == null) continue;
                            Assert.That(piece.ObjectId, Is.EqualTo(999));
                            Assert.That(piece.Health.HP, Is.EqualTo(500));
                            Assert.That(piece.Renderer != null, Is.EqualTo(route == 1));
                            pieces++;
                        }
                        Assert.That(pieces, Is.EqualTo(broken != 0 ? 5 : 0));
                        report.vectors.Add(new { mode = originalMode.ToString(), route, broken, pieces });
                        RecycleProbeEntities(world, originalSlots);
                        world.SetLogicOnlyEntityMaterialization(originalLogicOnly);
                        Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                        Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum, Is.EqualTo(checksum));
                    }
                }
                finally
                {
                    RecycleProbeEntities(world, originalSlots);
                    world.SetLogicOnlyEntityMaterialization(originalLogicOnly);
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    report.objectsAfter = world.ObjectCount;
                    report.restored = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum == checksum;
                }
                Assert.That(report.objectsAfter, Is.EqualTo(report.objectsBefore));
                Assert.That(report.restored, Is.True);
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }

        private static void RecycleProbeEntities(SimulationWorld world, HashSet<int> originalSlots)
        {
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                if (!originalSlots.Contains(slot)) world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Legacy-content real Scene: full World C25 Late pass, two materializers/current frame backend, broken versus healthy terminal; per-vector checksum restore. Not full input/physics tick or formal sprite parity.";
            public int tick, objectsBefore, objectsAfter;
            public bool restored;
            public List<object> vectors = new List<object>();
        }
    }
}
#endif
