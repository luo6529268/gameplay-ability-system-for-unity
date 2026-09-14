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
    public sealed class NTSD28Q06State18SpawnEditorTests
    {
        internal const string Witness = "artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001/native.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-TRANSACTION-001/";
        private static readonly Dictionary<(int, int, string), LF2CharacterDataWrapper> Definitions = new();
        private static IReadOnlyList<JObject> sourceVectors;

        private static IEnumerable<TestCaseData> MatrixCases()
        {
            foreach (var setting in new[]
            {
                (BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameTickPassMode.Legacy, 0),
                (BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 0),
                (BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 1),
                (BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.DataOriented, 1),
            })
                for (int chunk = 0; chunk < 12; chunk++)
                    yield return new TestCaseData(setting.Item1, setting.Item2, setting.Item3, chunk);
        }

        [TestCaseSource(nameof(MatrixCases))]
        public void BirthAndFullDriverMatchOriginal(BattleRuntimeProfile profile, BattleEcsCharacterFrameTickPassMode mode, int phase, int chunk)
        {
            int cases = 0, eligibleIndex = 0;
            var differences = new List<string>();
            var legacyCallCounts = new List<ulong>();
            sourceVectors ??= File.ReadLines(Witness).Select(JObject.Parse).ToArray();
            foreach (var row in sourceVectors)
            {
                if ((int)row["phase"] != phase || (int)row["delay"] != 0 ||
                    (profile == BattleRuntimeProfile.Authority400 && (int)row["source"] >= 400)) continue;
                if (eligibleIndex++ / 64 != chunk) continue;
                var world = MakeWorld(row, profile, out LF2Entity source);
                try
                {
                    world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
                    var observer = new Observer();
                    world.NativeRandom.ResetFromSeed((uint)row["seed"]);
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacyBefore = world.Rng.CallCount;
                    if (phase == 0)
                    {
                        source.RunNativeC25State18BrokenWeaponParticles();
                        world.ResolveLateObjectPointStructuralMaterializerForModule().FlushTasks();
                    }
                    else new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    string label = "case " + (eligibleIndex - 1) + " prev=" + row["previous"] + " current=" + row["current"] +
                        " source=" + row["source"] + " seed=" + row["seed"] + " composite=" + row["composite"];
                    // Alignment contract: NTSD28-Q06-STATE18-RNG-EXCEPTION-FIXTURE-001.
                    ulong legacyCalls = world.Rng.CallCount - legacyBefore;
                    ulong expectedLegacyCalls = phase == 1 && (int)row["freeSlots"] < 0 ? 1ul : 0ul;
                    legacyCallCounts.Add(legacyCalls);
                    if (legacyCalls != expectedLegacyCalls) differences.Add(label + " legacy RNG=" + legacyCalls + " expected " + expectedLegacyCalls);
                    if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"])) differences.Add(label + " native RNG sequence");
                    if (observer.CrtCalls != (int)row["crtCalls"]) differences.Add(label + " CRT calls");
                    CompareChildren(world, row, label, differences);
                    if ((world.FindEntityByRuntimeSlotForQuery((int)row["source"]) != null) != (row["sourceAfter"] is JObject))
                        differences.Add(label + " source lifetime");
                    if (world.LogicReferencePool.AvailableCreateTaskCount != 24) differences.Add(label + " task pool not restored");
                }
                finally { Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + profile + "-" + mode + "-" + phase + "-chunk" + chunk + ".json", JsonConvert.SerializeObject(new { cases, differences, legacyCallCounts }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(Math.Min(64, (profile == BattleRuntimeProfile.Authority400 ? 750 : 757) - chunk * 64)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(14)));
        }

        [TestCase(BattleRuntimeProfile.Authority400, -1, 1ul)]
        [TestCase(BattleRuntimeProfile.Authority400, 0, 0ul)]
        [TestCase(BattleRuntimeProfile.Authority400, 1, 0ul)]
        [TestCase(BattleRuntimeProfile.Authority400, 3, 0ul)]
        [TestCase(BattleRuntimeProfile.MobileExtended, -1, 1ul)]
        [TestCase(BattleRuntimeProfile.MobileExtended, 0, 0ul)]
        [TestCase(BattleRuntimeProfile.MobileExtended, 1, 0ul)]
        [TestCase(BattleRuntimeProfile.MobileExtended, 3, 0ul)]
        public void IsolatedC17ExceptionHasBoundedRngAndNoEntitySideEffects(BattleRuntimeProfile profile, int freeSlots, ulong expectedCalls)
        {
            var row = File.ReadLines(Witness).Select(JObject.Parse).First(r =>
                (int)r["phase"] == 1 && (int)r["delay"] == 0 && (int)r["source"] < 400 && (int)r["freeSlots"] == freeSlots);
            var world = MakeWorld(row, profile, out _);
            try
            {
                string before = NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);
                ulong legacyBefore = world.Rng.CallCount;
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.RandomWeaponDropTickAll(1);
                world.NativeRandom.SetDiagnosticCallObserver(null);
                ulong calls = world.Rng.CallCount - legacyBefore;
                bool entitiesUnchanged = before == NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1);
                File.WriteAllText(Output + "c17-" + profile + "-" + freeSlots + ".json",
                    JsonConvert.SerializeObject(new { calls, entitiesUnchanged, nativeCalls = observer.Calls.Count }, Formatting.Indented));
                Assert.That(calls, Is.EqualTo(expectedCalls));
                Assert.That(entitiesUnchanged, Is.True);
                Assert.That(observer.Calls, Is.Empty);
            }
            finally { Shutdown(world); }
        }

        internal static void CompareChildren(SimulationWorld world, JObject row, string label, List<string> differences)
        {
            var actual = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"]
                .Where(e => (int)e["identity"]["objectId"] == 999 || (int)e["identity"]["objectId"] == 777).ToArray();
            if (actual.Length != row["children"].Count()) differences.Add(label + " children=" + actual.Length + " expected " + row["children"].Count());
            foreach (var child in row["children"])
            {
                var expected = child["raw"];
                var found = actual.FirstOrDefault(e => (int)e["slot"] == (int)expected["slot"]);
                if (found == null) { differences.Add(label + " missing slot " + expected["slot"]); continue; }
                foreach (var field in Flatten(expected, ""))
                {
                    if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                    var value = found.SelectToken(field.Key);
                    bool equal = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float
                        ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value
                        : JToken.DeepEquals(value, field.Value);
                    if (!equal) differences.Add(label + " slot " + expected["slot"] + " " + field.Key + "=" + value + " expected " + field.Value);
                }
                var entity = world.FindEntityByRuntimeSlotForQuery((int)expected["slot"]);
                if (entity.Runtime.NativeSoundActionLatch != (int)child["soundLatch"]) differences.Add(label + " sound latch");
            }
        }

        [Test]
        public void FormalLogan999MatchesBirthAndFullDriver()
        {
            var differences = new List<string>();
            int cases = 0;
            NTSD.EditorTools.NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime",
                NTSD.EditorTools.NTSD28UnityRawCaptureEditor.DefaultScenario, BattleRuntimeProfile.Authority400, 3,
                (driver, inputs, identity) =>
                {
                    Assert.That(identity.CatalogFingerprint.ToString("X16"), Is.EqualTo("3900ECBC509557DB"));
                    var catalog = driver.World.RuntimeDataCatalog;
                    var formal = catalog.ObjectDefinitions.ToDictionary(d => d.id, d => catalog.GetCharacterConfig(d.id));
                    var formalTypes = catalog.ObjectDefinitions.ToDictionary(d => d.id, d => d.type);
                    foreach (var mode in new[] { BattleEcsCharacterFrameTickPassMode.Legacy, BattleEcsCharacterFrameTickPassMode.DataOriented })
                    foreach (string line in File.ReadLines("artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001/formal.jsonl"))
                    {
                        var row = JObject.Parse(line);
                        var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var source, formal, formalTypes);
                        try
                        {
                            world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
                            var observer = new Observer();
                            world.NativeRandom.ResetFromSeed((uint)row["seed"]);
                            world.NativeRandom.SetDiagnosticCallObserver(observer);
                            if ((int)row["phase"] == 0) source.RunNativeC25State18BrokenWeaponParticles();
                            else new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                            world.NativeRandom.SetDiagnosticCallObserver(null);
                            CompareChildren(world, row, "formal " + cases, differences);
                            if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"])) differences.Add("formal " + cases + " RNG");
                        }
                        finally { Shutdown(world); }
                        cases++;
                    }
                });
            File.WriteAllText(Output + "formal.json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(192));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        [Test]
        public void ExplicitDelayMatchesOriginalWithoutAddingAHostProducer()
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (string line in File.ReadLines(Witness))
            {
                var row = JObject.Parse(line);
                if ((int)row["phase"] != 0 || (int)row["delay"] == 0) continue;
                var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var source);
                try
                {
                    var observer = new Observer();
                    world.NativeRandom.ResetFromSeed((uint)row["seed"]);
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    BattleNativeState18ParticleWriter.Materialize(source, (int)row["delay"]);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    CompareChildren(world, row, "delay " + cases, differences);
                    if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"])) differences.Add("delay RNG " + cases);
                }
                finally { Shutdown(world); }
                cases++;
            }
            Assert.That(cases, Is.EqualTo(18));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void NativePresenceDistinguishesReservedFusionFromPendingLifecycle(bool reservedFusion)
        {
            var row = JObject.Parse(File.ReadLines(Witness).First(s => s.Contains("\"previous\":18,\"current\":0") && s.Contains("\"present\":1,\"freeSlots\":-1")));
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var source);
            try
            {
                var occupant = new LF2Weapon { ObjectId = 888 };
                occupant.FrameCache.Load(source.FrameCache.Wrapper);
                occupant.SetRequiredRuntimeSlot(50); world.Register(occupant);
                occupant.Runtime.OidMergeDormant = reservedFusion;
                occupant.Runtime.NativeLifecycleResolutionPending = !reservedFusion;
                occupant.Runtime.NativeLifecycleCode = reservedFusion ? 0 : 1101;
                world.NativeRandom.ResetFromSeed(42);
                bool spawned = source.RunNativeC25State18BrokenWeaponParticles();
                Assert.That(spawned, Is.EqualTo(!reservedFusion));
                Assert.That(world.FindEntityByRuntimeSlotIncludingPending(50), Is.SameAs(occupant));
                Assert.That(world.FindEntityByRuntimeSlotForQuery(51) != null, Is.EqualTo(!reservedFusion));
                Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(reservedFusion ? 4 : 28));
                Assert.That(world.LogicReferencePool.AvailableCreateTaskCount, Is.EqualTo(24));
                Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(reservedFusion ? 0 : 7));
            }
            finally { Shutdown(world); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PoolFailureReturnsTaskAndStopsAfterOneTuple(bool exhaustTasks)
        {
            var row = JObject.Parse(File.ReadLines(Witness).First(s => s.Contains("\"previous\":18,\"current\":0") && s.Contains("\"present\":1,\"freeSlots\":-1")));
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var source);
            var held = new List<OPointCreateTask>();
            try
            {
                var pool = world.LogicReferencePool;
                if (exhaustTasks) for (int i = 0; i < 24; i++) held.Add(pool.Fetch<OPointCreateTask>());
                pool.SealBattleCapacity();
                world.NativeRandom.ResetFromSeed(42);
                Assert.That(source.RunNativeC25State18BrokenWeaponParticles(), Is.False);
                Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(4));
                Assert.That(pool.ActiveCount, Is.Zero);
                Assert.That(pool.AvailableCreateTaskCount, Is.EqualTo(exhaustTasks ? 0 : 24));
                Assert.That(exhaustTasks ? pool.RejectedTaskFetchCount : pool.RejectedLogicObjectFetchCount, Is.EqualTo(1));
            }
            finally
            {
                foreach (var task in held) world.LogicReferencePool.Recycle(task);
                Shutdown(world);
            }
        }

        [Test]
        public void StoppingRejectsBeforeRandomOrSpawn()
        {
            var row = JObject.Parse(File.ReadLines(Witness).First(s => s.Contains("\"previous\":18,\"current\":0") && s.Contains("\"present\":1,\"freeSlots\":-1")));
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var source);
            try
            {
                world.NativeRandom.ResetFromSeed(42);
                world.BeginBattleShutdown();
                Assert.That(source.RunNativeC25State18BrokenWeaponParticles(), Is.False);
                Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.Zero);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
            finally { Shutdown(world); }
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity source,
            IDictionary<int, LF2CharacterDataWrapper> formal = null, IReadOnlyDictionary<int, int> formalTypes = null)
        {
            int I(string key) => (int)row[key];
            bool composite = I("composite") != 0;
            int oid = composite ? 151 : 888;
            string text = "<bmp_begin>\nname: Parent weapon_hp: 17\n<bmp_end>\n<frame> 0 current\nstate: " + I("current") +
                " wait: " + (composite ? 0 : 100) + " next: 0\n";
            if (composite) text += "opoint:\nkind: 1 oid: 777 action: 0\nopoint_end:\n";
            text += "<frame_end>\n<frame> 1 previous\nstate: " + I("previous") + " wait: 100 next: 1\n<frame_end>\n";
            if (I("declared999") != 0) text += "<frame> 999 declared_terminal\nstate: " + I("current") + " wait: 100 next: 0\n<frame_end>\n";
            if (composite) text += "<weapon_piece>\nteam: 1\npiece: 1\namount: 1 oid: 777 act: 0 framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n<weapon_piece_end>\n";
            const string childText = "<bmp_begin>\nname: Particle weapon_hp: 17\n<bmp_end>\n<stats> ohp: 25 omp: 50 max_mp: 700 <stats_end>\n" +
                "<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n<frame> 140 particle\nstate: 0 wait: 100 next: 140\n<frame_end>\n";
            var parent = Wrapper(oid, 1, text);
            var child = formal == null ? Wrapper(999, I("type"), childText) : formal[999];
            var wrappers = formal == null ? new Dictionary<int, LF2CharacterDataWrapper>() : new Dictionary<int, LF2CharacterDataWrapper>(formal);
            wrappers[oid] = parent;
            if (I("present") != 0) wrappers[999] = child;
            if (composite) wrappers[777] = formal == null ? Wrapper(777, 3, childText) : new LF2CharacterDataWrapper(777, child.characterData);
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(wrappers.Select(p => new ObjectDefinition(p.Key,
                p.Key == oid ? 1 : p.Key == 777 && composite ? 3 : formalTypes != null ? formalTypes[p.Key] : p.Value.characterData.type_sub,
                "state18.dat")).ToArray(), id => wrappers[id]);
            world.LogicReferencePool.PrewarmTasks<OPointCreateTask>(24);
            source = new LF2Weapon { ObjectId = oid };
            source.FrameCache.Load(parent);
            source.WriteCurrentFrameId(I("action"));
            source.Frame.D = source.FrameCache.GetNativeFrameDataById(I("action"));
            source.Frame.PN = 0; source.Frame.Prev = 1;
            source.Trans.SyncDirectFrameData(source.Frame.D?.wait ?? 0, source.Frame.D?.next ?? 0, 0);
            source.SetRequiredRuntimeSlot(I("source")); world.Register(source);
            source.Health.HP = 500; source.Health.HPBound = 500; source.Health.HP3 = 500; source.Health.PP = 500;
            source.Runtime.SetPosition(100.25, -20.5, 200.75);
            source.Runtime.XInt = 100; source.Runtime.YInt = -20; source.Runtime.ZInt = 200;
            source.Runtime.SetVelocity(3.25, -1.25, 0.75); source.SwitchDir("left");
            source.Runtime.WeaponFlightCounter = composite ? -1 : 17;
            source.OwnerEntityIndex = 17; source.RelationTeam = 9;
            source.Runtime.NativeLifecycleResolutionPending = I("pending") != 0;
            source.Runtime.NativeLifecycleCode = I("pending") != 0 ? 1101 : 0;
            if (I("freeSlots") >= 0)
            {
                int remaining = I("freeSlots");
                for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    if (slot == I("source")) continue;
                    if (remaining-- > 0) continue;
                    var blocker = new LF2Weapon { ObjectId = 4444 };
                    blocker.SetWeaponType(4); blocker.FrameCache.Load(child);
                    blocker.Frame.D = blocker.FrameCache.GetNativeFrameDataById(0);
                    blocker.Trans.SyncDirectFrameData(100, 0, 0);
                    blocker.Runtime.WeaponFlightCounter = 17;
                    blocker.SetRequiredRuntimeSlot(slot); world.Register(blocker);
                }
            }
            return world;
        }

        private static LF2CharacterDataWrapper Wrapper(int oid, int type, string text)
        {
            if (Definitions.TryGetValue((oid, type, text), out var cached)) return cached;
            var dat = new Lf2DatParserV2().ParseLoganContent(text);
            var bmp = LoganDefinitionMetadata.CopyFields(dat.Bmp.Properties);
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = bmp.Int32OrDefault("weapon_hp", 0),
                NativeMetadata = new LoganDefinitionMetadata(bmp, LoganDefinitionMetadata.CopyFields(dat.LoganStats?.Properties), null,
                    dat.LoganWeaponPiece == null ? null : new LoganWeaponPieceDefinition(dat.LoganWeaponPiece)),
            };
            foreach (var frame in dat.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            var result = new LF2CharacterDataWrapper(oid, data);
            Definitions.Add((oid, type, text), result);
            return result;
        }

        private static IEnumerable<KeyValuePair<string, JToken>> Flatten(JToken token, string prefix)
        {
            if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                    foreach (var value in Flatten(property.Value, prefix.Length == 0 ? property.Name : prefix + "." + property.Name)) yield return value;
            }
            else yield return new KeyValuePair<string, JToken>(prefix, token);
        }

        internal static void Shutdown(SimulationWorld world)
        {
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }

        internal sealed class Observer : INTSD28NativeRandomCallObserver
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
    internal static class NTSD28Q06State18SpawnPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_State18Play.request";
        private const string Result = "Temp/NTSD28_Q06_State18Play.result.json";

        static NTSD28Q06State18SpawnPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            string status = "FAIL", error = null;
            var results = new List<object>();
            var observations = new List<object>();
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            int availableBefore = LF2ObjectPool.Instance.AvailableObjectCountForAcceptance;
            long rejectedBefore = LF2ObjectPool.Instance.RejectedObjectFetchCount;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            try
            {
                var rendererPool = LF2ObjectPool.Instance;
                bool sealedBefore = rendererPool.IsBattleCapacitySealed;
                if (sealedBefore) rendererPool.UnsealBattleCapacity();
                try { rendererPool.PrepareObjectCapacityImmediateForDiagnostics(borrowers + 24); }
                finally { if (sealedBefore) rendererPool.SealBattleCapacity(); }
                var catalog = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(
                    "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime"));
                var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
                var types = catalog.Entries.ToDictionary(e => e.Id, e => e.Type);
                Assert.That(catalog.Entries.Count, Is.EqualTo(330));
                foreach (bool logicOnly in new[] { true, false })
                foreach (string line in File.ReadLines("artifacts/diagnostics/NTSD28-Q06-C25L-STATE18-SPAWN-SOURCE-WITNESS-001/formal.jsonl"))
                {
                    var row = JObject.Parse(line);
                    if ((int)row["phase"] != 1) continue;
                    var world = NTSD28Q06State18SpawnEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out _, configs, types);
                    try
                    {
                        world.SetLogicOnlyEntityMaterialization(logicOnly);
                        world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(BattleEcsCharacterFrameTickPassMode.DataOriented);
                        var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                        world.NativeRandom.ResetFromSeed((uint)row["seed"]);
                        world.NativeRandom.SetDiagnosticCallObserver(observer);
                        new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                        world.NativeRandom.SetDiagnosticCallObserver(null);
                        observations.Add(new
                        {
                            vector = results.Count, logicOnly,
                            nativeCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                            expectedCalls = (int)row["syncCalls"],
                            rendererActive = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                            rendererAvailable = LF2ObjectPool.Instance.AvailableObjectCountForAcceptance,
                            rendererRejected = LF2ObjectPool.Instance.RejectedObjectFetchCount - rejectedBefore,
                        });
                        var differences = new List<string>();
                        NTSD28Q06State18SpawnEditorTests.CompareChildren(world, row, "play " + results.Count, differences);
                        foreach (var child in row["children"])
                        {
                            var entity = world.FindEntityByRuntimeSlotForQuery((int)child["raw"]["slot"]);
                            if (entity != null && !ReferenceEquals(entity.FrameCache.Wrapper, world.RuntimeDataCatalog.GetCharacterConfig(entity.ObjectId)))
                                differences.Add("World definition binding differs for slot " + entity.Runtime.SlotIndex);
                        }
                        Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(8)));
                        Assert.That(JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"]), Is.True, "native RNG");
                        foreach (var child in row["children"])
                            Assert.That(world.FindEntityByRuntimeSlotForQuery((int)child["raw"]["slot"]).Renderer != null, Is.EqualTo(!logicOnly));
                        results.Add(new { logicOnly, source = (int)row["source"], current = (int)row["current"],
                            seed = (int)row["seed"], composite = (int)row["composite"], particles = (int)row["spawned"] });
                    }
                    finally
                    {
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                            world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                        NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                        Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
                        Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                    }
                }
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status, error, results, observations, rendererAvailableBefore = availableBefore, rendererBorrowersBefore = borrowers,
                rendererFixtureTarget = borrowers + 24,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Play, formal Logan999 DAT in isolated fixture worlds, both factories and full tick. Scene remains old content; sprites and physical inputs not certified.",
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
