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
    public sealed class NTSD28Q06Kind3FrameLookupEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-KIND3-CATCH-NATIVE-FRAME-LOOKUP-001/";
        private static JObject[] rows;
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void NativePairLookupAndAtomicWritesMatchOriginal(BattleRuntimeProfile profile, bool shadowPipeline)
        {
            rows ??= File.ReadLines(Output + "native.jsonl").Select(JObject.Parse).ToArray();
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in rows)
            {
                var world = MakeWorld(row, profile, out var attacker, out var target);
                try
                {
                    ComparePair(world, row["before"], "case " + cases + " before", differences);
                    var itr = attacker.Frame.D.itrs[0];
                    ulong legacyBefore = world.Rng.CallCount;
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    if (shadowPipeline)
                    {
                        world.CaptureCollisionFrameSnapshotsAll();
                        world.CollectCollisionCandidatesAll();
                        if (attacker.Runtime.HitCandidateCount != 1) differences.Add("case " + cases + " candidate count=" + attacker.Runtime.HitCandidateCount);
                        world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                        world.PostInteractionTickAll(1);
                        var diagnostics = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                        if (!diagnostics.CurrentTickPlanValid || diagnostics.ObservedWriterEffectCount != 1 || diagnostics.LastWriterEffectDifferenceMask != 0)
                            differences.Add("case " + cases + " shadow plan/effect mismatch=" + diagnostics.LastWriterEffectDifferenceMask);
                    }
                    else
                    {
                        bool applied = world.InteractionWriter.TryApplyGrab(attacker, target, itr, 3);
                        if (applied != (bool)row["applied"]) differences.Add("case " + cases + " applied=" + applied);
                    }
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    if (world.Rng.CallCount != legacyBefore || observer.Calls.Count != 0 || observer.CrtCalls != 0)
                        differences.Add("case " + cases + " unexpected RNG");
                    ComparePair(world, row["after"], "case " + cases + " after", differences);
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + profile + "-" + shadowPipeline + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(800));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        internal static void ComparePair(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            var raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            if (raw.Count() != 2) differences.Add(label + " entity count=" + raw.Count());
            foreach (var item in expected)
            {
                int slot = (int)item["raw"]["slot"];
                var actual = raw.SingleOrDefault(e => (int)e["slot"] == slot);
                if (actual == null) { differences.Add(label + " missing slot " + slot); continue; }
                foreach (var field in Flatten(item["raw"], ""))
                {
                    if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                    var value = actual.SelectToken(field.Key);
                    bool number = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float;
                    bool equal = number ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value : JToken.DeepEquals(value, field.Value);
                    if (!equal) differences.Add(label + " slot " + slot + " " + field.Key + "=" + value + " expected " + field.Value);
                }
                var entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity.Runtime.CaughtSlotIndex != (int)item["catchTarget"] || entity.Runtime.CatchSourceSlot90 != (int)item["catchSource"] || entity.Runtime.CaughtDuration != (int)item["timeout"])
                    differences.Add(label + " slot " + slot + " relation fields differ");
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

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity attacker, out LF2Entity target, bool renderer = false)
        {
            var definitions = new[] { Definition(row, true), Definition(row, false) };
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "catcher.dat"), new ObjectDefinition(78, 0, "caught.dat") }, id => definitions[id - 77]);
            var pair = new LF2Entity[2];
            for (int slot = 0; slot < 2; slot++)
            {
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                };
                var entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(entity, Is.Not.Null, "Kind3 source creation");
                pair[slot] = entity;
                entity.Team = slot + 1;
                int x = slot == 0 ? ((bool)row["reversed"] ? 110 : 100) : ((bool)row["reversed"] ? 100 : 110);
                entity.Runtime.SetPosition(x + (slot == 0 ? .25 : .75), slot == 0 ? -5.5 : -6.25, 200);
                entity.Runtime.XInt = x; entity.Runtime.YInt = slot == 0 ? -5 : -6; entity.Runtime.ZInt = 200;
                entity.Trans.SyncDirectFrameData(100, 0, slot == 0 ? 11 : 12);
                entity.AttackingCounter = slot == 0 ? 7 : 9;
                entity.Runtime.SetVelocity(slot == 0 ? -7.25 : 3.25, slot == 0 ? 8.5 : -4.5, slot == 0 ? 9.75 : -5.75);
            }
            attacker = pair[0]; target = pair[1];
            attacker.Runtime.CaughtSlotIndex = 77;
            attacker.Runtime.CaughtDuration = 88;
            target.Runtime.CatchSourceSlot90 = 66;
            target.Runtime.CatcherSlotIndex = 66;
            target.Runtime.Fall = 44;
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool attacker)
        {
            int catching = (int)row["catching"], caught = (int)row["caught"];
            bool declared = (bool)row[attacker ? "attackerDeclared" : "targetDeclared"];
            string key = attacker + "/" + catching + "/" + caught + "/" + declared;
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: NativeCatchLookup\n<bmp_end>\n<frame> 0 initial\nstate: 0 wait: 100 next: 0\n";
            text += attacker ? "itr:\nkind: 3 x: -20 y: -20 w: 60 h: 60 catchingact: " + catching + " caughtact: " + caught + " respond: 73\nitr_end:\n" : "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
            text += "<frame_end>\n";
            int action = Math.Abs(attacker ? catching : caught);
            if (declared && action > 0 && action <= 999)
                text += "<frame> " + action + " relation\nstate: " + (attacker ? 9 : 10) + " wait: 100 next: 0 centerx: " + (attacker ? 3 : 5) + " centery: " + (attacker ? 7 : 11) + "\ncpoint:\nkind: " + (attacker ? 1 : 2) + " x: " + (attacker ? 13 : 17) + " y: 19\ncpoint_end:\n<frame_end>\n";
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
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06Kind3FrameLookupPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_Kind3FrameLookupPlay.request";

        static NTSD28Q06Kind3FrameLookupPlayProbe() { EditorApplication.update += Poll; }

        private static bool Selected(JObject row)
        {
            int a = (int)row["catching"], t = (int)row["caught"];
            return (a == t && new[] { 998, -998, 999, -999, 1000 }.Contains(a)) ||
                (a == 0 && t == 998) || (a == 998 && t == 0) || (a == 99 && t == 857) ||
                (a == 1000 && t == 998) || (a == 998 && t == 1000);
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
            int cases = 0;
            var differences = new List<string>();
            try
            {
                var selected = File.ReadLines("artifacts/diagnostics/NTSD28-Q06-KIND3-CATCH-NATIVE-FRAME-LOOKUP-001/native.jsonl").Select(JObject.Parse).Where(Selected).ToArray();
                Assert.That(selected.Length, Is.EqualTo(80));
                foreach (bool renderer in new[] { false, true })
                foreach (bool shadow in new[] { false, true })
                foreach (var row in selected)
                {
                    var world = NTSD28Q06Kind3FrameLookupEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out var attacker, out var target, renderer);
                    try
                    {
                        string label = "renderer=" + renderer + " shadow=" + shadow + " case=" + row["index"];
                        NTSD28Q06Kind3FrameLookupEditorTests.ComparePair(world, row["before"], label + " before", differences);
                        var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                        world.NativeRandom.SetDiagnosticCallObserver(observer);
                        ulong before = world.Rng.CallCount;
                        if (shadow)
                        {
                            world.CaptureCollisionFrameSnapshotsAll();
                            world.CollectCollisionCandidatesAll();
                            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
                            world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                            world.PostInteractionTickAll(1);
                            var diagnostics = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                            Assert.That(diagnostics.CurrentTickPlanValid, Is.True);
                            Assert.That(diagnostics.ObservedWriterEffectCount, Is.EqualTo(1));
                            Assert.That(diagnostics.LastWriterEffectDifferenceMask, Is.Zero);
                        }
                        else Assert.That(world.InteractionWriter.TryApplyGrab(attacker, target, attacker.Frame.D.itrs[0], 3), Is.EqualTo((bool)row["applied"]));
                        world.NativeRandom.SetDiagnosticCallObserver(null);
                        Assert.That(observer.Calls, Is.Empty);
                        Assert.That(observer.CrtCalls, Is.Zero);
                        Assert.That(world.Rng.CallCount, Is.EqualTo(before));
                        NTSD28Q06Kind3FrameLookupEditorTests.ComparePair(world, row["after"], label + " after", differences);
                        Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
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
                Assert.That(cases, Is.EqualTo(320));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_Kind3FrameLookupPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, synthetic kind3 pair admission/binding only, both factories and actual ShadowCompare; later CPoint/throw/graphics not certified."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
