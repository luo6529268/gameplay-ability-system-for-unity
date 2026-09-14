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
    public sealed class NTSD28Q06WeaponPieceAdmissionEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001/native.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001/";

        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameTickPassMode.Legacy, 0)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 0)]
        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameTickPassMode.Legacy, 1)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 1)]
        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameTickPassMode.DataOriented, 1)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.DataOriented, 1)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.Legacy, 2)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameTickPassMode.DataOriented, 2)]
        public void OriginalAdmissionAndSlotEndpointsMatch(BattleRuntimeProfile profile,
            BattleEcsCharacterFrameTickPassMode mode, int phase)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (string line in File.ReadLines(Witness))
            {
                var row = JObject.Parse(line);
                if ((int)row["phase"] != (phase == 2 ? 1 : phase) ||
                    (profile == BattleRuntimeProfile.Authority400 && (int)row["source"] >= 400)) continue;
                if (phase == 2 && ((int)row["target"] != 0 || (int)row["freeSlots"] != -1 ||
                    (int)row["parentOid"] != 888 || (int)row["variants"] != 1 || (int)row["present"] != 1)) continue;
                SimulationWorld world = MakeWorld(row, profile, out LF2Entity source);
                try
                {
                    world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
                    var observer = new Observer();
                    world.NativeRandom.ResetFromSeed(42);
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    if (phase == 0) source.TryRunLatePostOpointCleanupPhase();
                    else if (phase == 2) new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                    else
                    {
                        world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                        world.LateEntityUpdateAll(1);
                    }
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    string label = "case " + cases + " oid=" + row["target"] + " type=" + row["type"] +
                        " act=" + row["action"] + " source=" + row["source"];
                    if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"]))
                        differences.Add(label + " random calls");
                    if ((world.FindEntityByRuntimeSlotForQuery((int)row["source"]) != null) != (bool)row["sourceAlive"])
                        differences.Add(label + " parent lifecycle");
                    var raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
                    int target = (int)row["target"] == -1 ? 999 : (int)row["target"];
                    int expectedAlive = row["events"].Count(e => e["entity"] is JObject);
                    int actualAlive = raw.Count(e => (int)e["identity"]["objectId"] == target);
                    if (actualAlive != expectedAlive) differences.Add(label + " alive " + actualAlive + " expected " + expectedAlive);
                    foreach (var ev in row["events"].Where(e => (int)e["slot"] < 1000))
                    {
                        var actual = raw.FirstOrDefault(e => (int)e["slot"] == (int)ev["slot"]);
                        var expected = ev["entity"];
                        if (expected is not JObject)
                        {
                            if (actual != null) differences.Add(label + " terminal slot retained " + ev["slot"]);
                            continue;
                        }
                        if (actual == null) { differences.Add(label + " missing slot " + ev["slot"]); continue; }
                        foreach (var field in Flatten(expected, ""))
                        {
                            if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                            var value = actual.SelectToken(field.Key);
                            bool equal = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float
                                ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value
                                : JToken.DeepEquals(value, field.Value);
                            if (!equal) differences.Add(label + " slot " + ev["slot"] + " " + field.Key + "=" + value + " expected " + field.Value);
                        }
                        var entity = world.FindEntityByRuntimeSlotForQuery((int)ev["slot"]);
                        if (entity.Frame.D?.frameId != entity.Frame.N) differences.Add(label + " frame descriptor");
                    }
                    if (world.LogicReferencePool.AvailableCreateTaskCount != 1)
                        differences.Add(label + " task not returned");
                }
                finally { Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + profile + "-" + mode + "-" + phase + ".json",
                JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(phase == 2 ? 254 : profile == BattleRuntimeProfile.Authority400 ? 648 : 900));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(15)));
        }

        [TestCase(false, 0, 0, false)]
        [TestCase(true, 0, 0, true)]
        [TestCase(true, 777, -1, false)]
        [TestCase(true, 777, 999, false)]
        [TestCase(true, 777, 1000, false)]
        public void FactoryAdmissionPrecedesPoolBorrow(bool native, int oid, int action, bool expected)
        {
            var row = DefaultRow();
            row["target"] = oid;
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out _);
            try
            {
                int before = world.LogicReferencePool.ActiveCount;
                var task = new OPointCreateTask
                {
                    targetWorld = world, opoint = new ObjectPoint { oid = oid, action = action, kind = 1 },
                    nativeWeaponPieceSpawn = native, preserveActionZero = true, dir = "right", requiredRuntimeSlot = 50,
                };
                LF2Entity result = null;
                Assert.DoesNotThrow(() => result = world.LogicEntityFactory.Create(task, out _));
                Assert.That(result != null, Is.EqualTo(expected));
                Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(before + (expected ? 1 : 0)));
            }
            finally { Shutdown(world); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExhaustedPoolsDoNotLeakTasksOrPublishEntities(bool exhaustTasks)
        {
            var world = MakeWorld(DefaultRow(), BattleRuntimeProfile.Authority400, out LF2Entity source);
            OPointCreateTask held = null;
            try
            {
                var pool = world.LogicReferencePool;
                if (exhaustTasks) held = pool.Fetch<OPointCreateTask>();
                pool.SealBattleCapacity();
                world.NativeRandom.ResetFromSeed(42);
                source.TryRunLatePostOpointCleanupPhase();
                Assert.That(world.FindEntityByRuntimeSlotForQuery(50), Is.Null);
                Assert.That(pool.ActiveCount, Is.Zero);
                Assert.That(pool.AvailableCreateTaskCount, Is.EqualTo(exhaustTasks ? 0 : 1));
                Assert.That(exhaustTasks ? pool.RejectedTaskFetchCount : pool.RejectedLogicObjectFetchCount, Is.EqualTo(2));
                Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(10));
                if (held != null) { pool.Recycle(held); held = null; }
                var recycled = pool.Fetch<OPointCreateTask>();
                Assert.That(recycled.nativeWeaponPieceSpawn, Is.False);
                Assert.That(recycled.targetWorld, Is.Null);
                pool.Recycle(recycled);
            }
            finally
            {
                if (held != null) world.LogicReferencePool.Recycle(held);
                Shutdown(world);
            }
        }

        private static JObject DefaultRow() => new JObject
        {
            ["target"] = 0, ["type"] = 3, ["action"] = 0, ["declared999"] = 0,
            ["present"] = 1, ["source"] = 20, ["freeSlots"] = -1, ["variants"] = 1, ["parentOid"] = 888,
        };

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity source)
        {
            int I(string key) => (int)row[key];
            string parentDat = "<bmp_begin>\nname: Parent weapon_hp: 10\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n<weapon_piece>\nteam: 1\n";
            for (int variant = 0; variant < I("variants"); variant++)
                parentDat += "piece: 1\n" + (variant == 0 ? "amount: 2 " : "") + "oid: " + I("target") + " act: " + I("action") + " framea: 0 dvx: 0 dvy: 0 dvz: 0\npiece_end:\n";
            parentDat += "<weapon_piece_end>\n";
            string childDat = "<bmp_begin>\nname: Child weapon_hp: 17\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
            if (I("declared999") != 0) childDat += "<frame> 999 terminal\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
            var parent = Wrapper(I("parentOid"), 1, parentDat);
            var child = Wrapper(I("target") == -1 ? 999 : I("target"), I("type"), childDat);
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper> { [I("parentOid")] = parent };
            if (I("present") != 0) wrappers[child.characterId] = child;
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(wrappers.Select(p => new ObjectDefinition(p.Key, p.Value.characterData.type_sub, "admission.dat")).ToArray(), id => wrappers[id]);
            world.LogicReferencePool.PrewarmTasks<OPointCreateTask>(1);
            source = new LF2Weapon { ObjectId = I("parentOid") };
            source.FrameCache.Load(parent);
            source.Frame.D = source.FrameCache.GetNativeFrameDataById(0);
            source.Trans.SyncDirectFrameData(100, 0, 0);
            source.SetRequiredRuntimeSlot(I("source"));
            world.Register(source);
            source.Health.HP = 500; source.Health.HPBound = 500; source.Health.HP3 = 500; source.Health.PP = 500;
            source.Runtime.SetPosition(100, -20, 200); source.Runtime.SyncIntegerPosition();
            source.Runtime.WeaponFlightCounter = -1;
            source.OwnerEntityIndex = 17; source.RelationTeam = 9;
            if (I("freeSlots") >= 0)
            {
                int remaining = I("freeSlots");
                for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    if (slot == I("source")) continue;
                    if (remaining-- > 0) continue;
                    var blocker = new LF2Weapon { ObjectId = 4444 };
                    blocker.SetWeaponType(4);
                    blocker.FrameCache.Load(child);
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
            var dat = new Lf2DatParserV2().ParseLoganContent(text);
            var bmp = LoganDefinitionMetadata.CopyFields(dat.Bmp.Properties);
            var data = new LF2CharacterData
            {
                type_sub = type, weapon_hp = bmp.Int32OrDefault("weapon_hp", 0),
                NativeMetadata = new LoganDefinitionMetadata(bmp, LoganDefinitionMetadata.CopyFields(dat.LoganStats?.Properties), null,
                    dat.LoganWeaponPiece == null ? null : new LoganWeaponPieceDefinition(dat.LoganWeaponPiece)),
            };
            foreach (var frame in dat.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            return new LF2CharacterDataWrapper(oid, data);
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

        private static void Shutdown(SimulationWorld world)
        {
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }

        private sealed class Observer : INTSD28NativeRandomCallObserver
        {
            internal readonly List<long[]> Calls = new List<long[]>();
            public void OnCrtNext(NTSD28NativeCrtCall call) { }
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                Calls.Add(new[] { (long)call.CallSite, call.UpperBound, call.Result, call.CounterAfter, call.IndexAfter, (long)call.TotalCalls });
            }
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06WeaponPieceAdmissionPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_PieceAdmissionPlay.request";
        private const string Result = "Temp/NTSD28_Q06_PieceAdmissionPlay.result.json";

        static NTSD28Q06WeaponPieceAdmissionPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var vectors = new List<object>();
            string status = "FAIL", error = null;
            int before = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            try
            {
                var rows = File.ReadLines("artifacts/diagnostics/NTSD28-Q06-WEAPON-PIECE-ADMISSION-SOURCE-WITNESS-001/native.jsonl")
                    .Select(JObject.Parse).Where(r => (int)r["phase"] == 1 && (int)r["target"] == 0 &&
                        (int)r["source"] < 100 && (int)r["parentOid"] == 888 && (int)r["freeSlots"] == -1 &&
                        (int)r["present"] == 1 && (int)r["variants"] == 1 &&
                        ((int)r["action"] == 0 || (int)r["action"] == 998 || (int)r["action"] == 999)).ToArray();
                foreach (bool logicOnly in new[] { true, false })
                foreach (var row in rows)
                {
                    var world = NTSD28Q06WeaponPieceAdmissionEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out _);
                    try
                    {
                        world.SetLogicOnlyEntityMaterialization(logicOnly);
                        world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(BattleEcsCharacterFrameTickPassMode.DataOriented);
                        if ((int)row["type"] == 0 && (int)row["source"] == 20 &&
                            (int)row["action"] == 0 && (int)row["declared999"] == 0)
                        {
                            foreach (int rejectedAction in new[] { -1, 999, 1000, 0 })
                            {
                                var rejectedTask = new OPointCreateTask
                                {
                                    targetWorld = world, opoint = new ObjectPoint { oid = 0, action = rejectedAction, kind = 1 },
                                    nativeWeaponPieceSpawn = rejectedAction != 0, preserveActionZero = true,
                                    dir = "right", requiredRuntimeSlot = 50,
                                };
                                var rejected = logicOnly ? world.LogicEntityFactory.Create(rejectedTask, out _) :
                                    LF2ObjectPointFactory.Instance.CreateObjectImmediate(rejectedTask);
                                Assert.That(rejected, Is.Null);
                                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
                                Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(before));
                            }
                        }
                        world.NativeRandom.ResetFromSeed(42);
                        new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                        int alive = 0;
                        foreach (var ev in row["events"].Where(e => (int)e["slot"] < 1000))
                        {
                            var entity = world.FindEntityByRuntimeSlotForQuery((int)ev["slot"]);
                            Assert.That(entity != null, Is.EqualTo(ev["entity"] is JObject));
                            if (entity == null) continue;
                            alive++;
                            Assert.That(entity.ObjectId, Is.Zero);
                            Assert.That(entity.Frame.N, Is.EqualTo((int)ev["entity"]["frame"]["action"]));
                            Assert.That(entity.Frame.D.frameId, Is.EqualTo(entity.Frame.N));
                            Assert.That(entity.AttackingCounter, Is.EqualTo((int)ev["entity"]["frame"]["frameCounter"]));
                            Assert.That(entity.Health.HP, Is.EqualTo(500));
                            Assert.That(entity.Renderer != null, Is.EqualTo(!logicOnly));
                        }
                        Assert.That(world.FindEntityByRuntimeSlotForQuery((int)row["source"]), Is.Null);
                        Assert.That(world.LogicReferencePool.AvailableCreateTaskCount, Is.EqualTo(1));
                        vectors.Add(new { logicOnly, type = (int)row["type"], action = (int)row["action"],
                            declared999 = (int)row["declared999"], source = (int)row["source"], alive });
                    }
                    finally
                    {
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                            world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                        world.BeginBattleShutdown();
                        Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                        Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
                        Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(before));
                    }
                }
                var exhaustedWorld = NTSD28Q06WeaponPieceAdmissionEditorTests.MakeWorld(rows[0], BattleRuntimeProfile.Authority400, out LF2Entity exhaustedSource);
                var borrowed = new List<LF2ObjectRenderer>();
                var rendererPool = LF2ObjectPool.Instance;
                bool originallySealed = rendererPool.IsBattleCapacitySealed;
                int availableBefore = rendererPool.AvailableObjectCountForAcceptance;
                long rejectedBefore = rendererPool.RejectedObjectFetchCount;
                try
                {
                    exhaustedWorld.SetLogicOnlyEntityMaterialization(false);
                    for (int index = 0; index < availableBefore; index++)
                    {
                        rendererPool.Get(out var renderer);
                        Assert.That(renderer, Is.Not.Null);
                        borrowed.Add(renderer);
                    }
                    rendererPool.SealBattleCapacity();
                    exhaustedWorld.NativeRandom.ResetFromSeed(42);
                    exhaustedSource.TryRunLatePostOpointCleanupPhase();
                    Assert.That(exhaustedWorld.FindEntityByRuntimeSlotForQuery(50), Is.Null);
                    Assert.That(exhaustedWorld.LogicReferencePool.ActiveCount, Is.Zero);
                    Assert.That(exhaustedWorld.LogicReferencePool.AvailableCreateTaskCount, Is.EqualTo(1));
                    Assert.That(rendererPool.RejectedObjectFetchCount - rejectedBefore, Is.EqualTo(2));
                    Assert.That(exhaustedWorld.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(10));
                    vectors.Add(new { rendererPoolExhausted = true, rejected = 2, tasksReturned = 1 });
                }
                finally
                {
                    foreach (var renderer in borrowed) rendererPool.Release(renderer);
                    if (!originallySealed) rendererPool.UnsealBattleCapacity();
                    exhaustedWorld.BeginBattleShutdown();
                    Assert.That(exhaustedWorld.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                    Assert.That(rendererPool.ActiveObjectCountForAcceptance, Is.EqualTo(before));
                    Assert.That(rendererPool.AvailableObjectCountForAcceptance, Is.EqualTo(availableBefore));
                }
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status, error, vectors, rendererBorrowersBefore = before,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Play, isolated DAT fixture worlds, both production materializers/full NTSDBattleTickSystem, types0..6 and OID0 high/low slots. Not formal sprites or physical inputs.",
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
