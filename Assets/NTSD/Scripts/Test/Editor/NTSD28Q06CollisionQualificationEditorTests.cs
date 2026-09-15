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
    public sealed class NTSD28Q06CollisionQualificationEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-COLLISION-CURRENT-SNAPSHOT-QUALIFICATION-001/";
        internal const string Source = "artifacts/diagnostics/NTSD28-Q06-COLLISION-QUALIFICATION-SOURCE-WITNESS-001/first.jsonl";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void DriverCatchFrameChangePreservesQueuedAttack(BattleRuntimeProfile profile, bool optimized)
        {
            var differences = new List<string>();
            var plans = new List<object>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse).Where(r => (string)r["kind"] == "driver"))
            {
                int caughtAction = (int)row["caughtAction"];
                var definitions = Enumerable.Range(0, 3).Select(slot => DriverDefinition(slot, caughtAction)).ToArray();
                var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
                world.SetLogicOnlyEntityMaterialization(true);
                world.PrepareRuntimeDataCatalogForBattle(Enumerable.Range(0, 3).Select(slot => new ObjectDefinition(77 + slot, 0, "qualification-driver.dat")).ToArray(),
                    id => definitions[id - 77]);
                world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(optimized ? BattleEcsCharacterFrameTickPassMode.DataOriented : BattleEcsCharacterFrameTickPassMode.Legacy);
                world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(optimized ? BattleEcsCharacterFrameAdvancePassMode.DataOriented : BattleEcsCharacterFrameAdvancePassMode.Legacy);
                world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                world.Runtime.FunctionKeys.ResetForBattle(true);
                try
                {
                    for (int slot = 0; slot < 3; slot++)
                    {
                        var task = new OPointCreateTask
                        {
                            targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                            relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                        };
                        var entity = world.LogicEntityFactory.Create(task, out _);
                        Assert.That(entity, Is.Not.Null);
                        entity.Team = slot + 1;
                        entity.Runtime.SetPosition(slot == 2 ? 130 : 100 + slot * 10, 0, 200);
                        entity.Runtime.SyncIntegerPosition();
                    }
                    world.NativeRandom.ResetFromSeed(42);
                    string label = "caughtAction " + caughtAction;
                    CompareRaw(world, row["before"], label + " before", differences);
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacy = world.Rng.CallCount;
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    CompareRaw(world, row["after"], label + " after", differences);
                    if (world.Rng.CallCount - legacy != 1 || observer.Calls.Count != (int)row["nativeCalls"] || observer.CrtCalls != (int)row["crtCalls"])
                        differences.Add(label + " RNG counts legacy=" + (world.Rng.CallCount - legacy) +
                            " native=" + observer.Calls.Count + " CRT=" + observer.CrtCalls + " expected CRT=" + row["crtCalls"]);
                    var plan = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                    plans.Add(new { caughtAction, plan.CurrentTickPlanValid, plan.ObservedWriterEffectCount, plan.LastWriterEffectDifferenceMask });
                    if (plan.LastWriterEffectDifferenceMask != 0) differences.Add(label + " Shadow writer difference=" + plan.LastWriterEffectDifferenceMask);
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "driver-" + profile + "-" + optimized + ".json", JsonConvert.SerializeObject(new { cases, differences, plans }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(2));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(15)));
        }

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void OriginalQualificationAndFrozenConsumerMatch(BattleRuntimeProfile profile, bool roleAware)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse).Where(r => (string)r["kind"] == "qualification"))
            {
                var world = MakeWorld(row, profile, roleAware, out var attacker, out var target);
                try { RunCase(world, attacker, target, row, differences); }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + roleAware + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(480));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(15)));
        }

        internal static void RunCase(SimulationWorld world, LF2Entity attacker, LF2Entity target, JObject row, List<string> differences)
        {
            string label = "case " + row["index"];
            CompareRaw(world, row["before"], label + " before", differences);
            ulong legacy = world.Rng.CallCount;
            var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            world.CollectCollisionCandidatesAll();
            if (attacker.Runtime.HitCandidateCount != (int)row["candidates"])
                differences.Add(label + " candidates=" + attacker.Runtime.HitCandidateCount + " expected " + row["candidates"]);
            BattleHitCandidatePairSnapshot frozen = BattleHitCandidatePairSnapshotFactory.Capture(attacker, target, world);
            if (row["pair"] is JObject expected)
            {
                ComparePair(frozen, expected, label + " direct pair", differences);
                if (world.SceneQuery.TryGetCollisionCandidateRange(attacker, out var range) && range.Count == 1 && range.TryGet(0, out var hit))
                {
                    frozen = hit.PairSnapshot;
                    ComparePair(frozen, expected, label + " captured pair", differences);
                }
                else differences.Add(label + " missing captured pair");
                bool eligible = BruteForceSceneQuery.RuntimeConsumeItrAllowed(attacker, attacker.GetCollisionFrameData().itrs[0], target, in frozen);
                if (eligible != ((int)row["eligibility"] == 0)) differences.Add(label + " initial eligibility=" + eligible);
            }
            CompareRaw(world, row["afterCollection"], label + " collection", differences);
            attacker.DirectWriteNativeRawFramePreserveWaitCounter(1000);
            target.DirectWriteNativeRawFramePreserveWaitCounter(1000);
            if (row["pair"] is JObject)
            {
                bool eligible = BruteForceSceneQuery.RuntimeConsumeItrAllowed(attacker, attacker.GetCollisionFrameData().itrs[0], target, in frozen);
                if (eligible != ((int)row["afterCurrentMissingEligibility"] == 0)) differences.Add(label + " missing-current eligibility=" + eligible);
            }
            CompareRaw(world, row["afterCurrentMissing"], label + " missing-current", differences);
            world.NativeRandom.SetDiagnosticCallObserver(null);
            if (world.Rng.CallCount != legacy || observer.Calls.Count != 0 || observer.CrtCalls != 0) differences.Add(label + " RNG changed");
        }

        private static void ComparePair(BattleHitCandidatePairSnapshot actual, JObject expected, string label, List<string> differences)
        {
            var value = new JObject
            {
                ["valid"] = actual.Valid, ["attackerAction"] = actual.AttackerAction, ["targetAction"] = actual.TargetAction,
                ["attackerCurrentState"] = actual.AttackerCurrentState, ["targetCurrentState"] = actual.TargetCurrentState,
                ["attackerPreviousState"] = actual.AttackerPreviousState, ["targetPreviousState"] = actual.TargetPreviousState,
                ["attackerTickState"] = actual.AttackerTickState, ["targetTickState"] = actual.TargetTickState,
                ["attackerGroup"] = actual.AttackerBattleGroup, ["targetGroup"] = actual.TargetBattleGroup
            };
            foreach (var field in expected.Properties())
                if (!JToken.DeepEquals(field.Value, value[field.Name])) differences.Add(label + " " + field.Name + "=" + value[field.Name] + " expected " + field.Value);
        }

        internal static void CompareRaw(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            var actual = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            if (actual.Count() != expected.Count()) differences.Add(label + " entity count=" + actual.Count());
            foreach (JObject entity in expected)
            {
                int slot = (int)entity["slot"];
                var result = actual.SingleOrDefault(e => (int)e["slot"] == slot);
                if (result == null) { differences.Add(label + " missing slot=" + slot); continue; }
                foreach (var value in entity.Descendants().OfType<JValue>())
                {
                    string path = value.Path.Substring(entity.Path.Length).TrimStart('.');
                    if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(path)) continue;
                    var found = result.SelectToken(path);
                    bool numeric = value.Type == JTokenType.Integer || value.Type == JTokenType.Float;
                    bool equal = numeric ? found != null && found.Type != JTokenType.Null && (double)value == (double)found : JToken.DeepEquals(value, found);
                    if (!equal) differences.Add(label + " slot " + slot + " " + path + "=" + found + " expected " + value);
                }
            }
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, bool roleAware,
            out LF2Entity attacker, out LF2Entity target, bool renderer = false)
        {
            var definitions = new[] { Definition(row, true), Definition(row, false) };
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000,
                roleAware ? CollisionBroadphaseBackend.LooseQuadtree : CollisionBroadphaseBackend.BruteForce);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "qualification-a.dat"), new ObjectDefinition(78, 0, "qualification-t.dat") }, id => definitions[id - 77]);
            ((BruteForceSceneQuery)world.SceneQuery).ForceRoleAwareTreeForDiagnostics = roleAware;
            var pair = new LF2Entity[2];
            for (int slot = 0; slot < 2; slot++)
            {
                int group = slot == 0 || (bool)row["same"] ? 1 : 2;
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = group, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                };
                var entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(entity, Is.Not.Null);
                pair[slot] = entity;
                entity.Team = group;
                entity.Runtime.SetPosition(100 + slot * 10, 0, 200);
                entity.Runtime.SyncIntegerPosition();
            }
            world.CaptureCollisionFrameSnapshotsAll();
            attacker = pair[0]; target = pair[1];
            attacker.DirectWriteNativeRawFramePreserveWaitCounter((int)row["a"]);
            target.DirectWriteNativeRawFramePreserveWaitCounter((int)row["t"]);
            attacker.Frame.Prev = (int)row["pa"];
            target.Frame.Prev = (int)row["pt"];
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool attacker)
        {
            int effect = (int)row["effect"];
            bool declared = (bool)row["declared999"];
            string key = attacker + "/" + effect + "/" + declared;
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: CollisionQualification\n<bmp_end>\n<frame> 0 geometry\nstate: 0 wait: 100 next: 0\n";
            text += attacker ? "itr:\nkind: 0 effect: " + effect + " x: -20 y: -20 w: 60 h: 60 vrest: 1 injury: 1\nitr_end:\n" :
                "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
            text += "<frame_end>\n<frame> 10 state18\nstate: 18 wait: 100 next: 0\n<frame_end>\n<frame> 998 state13\nstate: 13 wait: 100 next: 0\n<frame_end>\n";
            if (declared) text += "<frame> 999 state19\nstate: 19 wait: 100 next: 0\n<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData
            {
                type_sub = 0,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            var wrapper = new LF2CharacterDataWrapper(attacker ? 77 : 78, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }

        private static LF2CharacterDataWrapper DriverDefinition(int slot, int caughtAction)
        {
            string key = "driver/" + slot + "/" + caughtAction;
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: CollisionQualificationDriver\n<bmp_end>\n<frame> 0 initial\nstate: 0 wait: 100 next: 0\n";
            if (slot == 0) text += "itr:\nkind: 3 x: -5 y: -20 w: 20 h: 40 catchingact: 10 caughtact: " + caughtAction + " respond: 73\nitr_end:\n";
            if (slot == 1) text += "itr:\nkind: 0 x: 15 y: -20 w: 10 h: 40 injury: 1 fall: 0 vrest: 1\nitr_end:\n";
            if (slot != 0) text += "bdy:\nkind: 0 x: -5 y: -20 w: 10 h: 40\nbdy_end:\n";
            text += "<frame_end>\n";
            if (slot < 2) text += "<frame> " + (slot == 0 ? 10 : caughtAction) + " caught\nstate: " + (slot == 0 ? 9 : 10) +
                " wait: 100 next: 0\ncpoint:\nkind: " + (slot == 0 ? 1 : 2) + " x: 10 y: 0 hurtable: 1\ncpoint_end:\n<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData
            {
                type_sub = 0,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            var wrapper = new LF2CharacterDataWrapper(77 + slot, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06CollisionQualificationPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_CollisionQualificationPlay.request";

        static NTSD28Q06CollisionQualificationPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request)) return;
            string command = File.ReadAllText(Request).Trim();
            if (command != "run" && command != "run-driver") return;
            bool driverOnly = command == "run-driver";
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            string status = "FAIL", error = null;
            int cases = 0;
            var differences = new List<string>();
            try
            {
                if (driverOnly)
                {
                    var tests = new NTSD28Q06CollisionQualificationEditorTests();
                    foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
                    foreach (bool optimized in new[] { false, true })
                    {
                        tests.DriverCatchFrameChangePreservesQueuedAttack(profile, optimized);
                        cases += 2;
                        Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                    }
                }
                else
                {
                    var selected = File.ReadLines(NTSD28Q06CollisionQualificationEditorTests.Source).Select(JObject.Parse)
                        .Where(row => (string)row["kind"] == "qualification" &&
                            (((int)row["pa"] == 0 && (int)row["pt"] == 0) || ((int)row["a"] == 0 && (int)row["t"] == 0))).ToArray();
                    Assert.That(selected.Length, Is.EqualTo(120));
                    foreach (bool renderer in new[] { false, true })
                    foreach (bool roleAware in new[] { false, true })
                    foreach (var row in selected)
                    {
                        var world = NTSD28Q06CollisionQualificationEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400,
                            roleAware, out var attacker, out var target, renderer);
                        try
                        {
                            NTSD28Q06CollisionQualificationEditorTests.RunCase(world, attacker, target, row, differences);
                            cases++;
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
                    Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));

                }
                Assert.That(cases, Is.EqualTo(driverOnly ? 8 : 480));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_CollisionQualificationPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, driverOnly, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = driverOnly ? "Real Scene context, two source driver cases across both profiles/frame modes; isolated logic worlds, scene checksum unchanged. No physical input or visual asset claim." : "Real Scene, synthetic qualification/frozen classification endpoints, both factories/query modes."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
