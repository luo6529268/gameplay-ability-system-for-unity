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
    public sealed class NTSD28Q06Kind2FrameLookupEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-KIND2-PICKUP-NATIVE-FRAME-LOOKUP-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> definitions = new();
        private static JObject[] rows;

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void PickupAndFollowingPhysicsMatchOriginal(BattleRuntimeProfile profile, bool shadow)
        {
            rows ??= File.ReadLines(Output + "native.jsonl").Select(JObject.Parse).ToArray();
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in rows)
            {
                var world = MakeWorld(row, profile, out var holder, out var target);
                try
                {
                    string label = "case " + row["index"];
                    foreach (var before in row["before"]) Compare(world, before, label + " before", differences);
                    ulong legacyBefore = world.Rng.CallCount;
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    if (shadow)
                    {
                        world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                        world.PostInteractionTickAll(1);
                        var diagnostic = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                        if (!diagnostic.CurrentTickPlanValid || diagnostic.ObservedWriterEffectCount != 1 || diagnostic.LastWriterEffectDifferenceMask != 0)
                            differences.Add(label + " shadow mismatch " + diagnostic.LastWriterEffectDifferenceMask);
                    }
                    else if (world.InteractionWriter.TryApplyPickup(holder, target, 2) != (bool)row["applied"])
                        differences.Add(label + " application differs");
                    foreach (var after in row["after"]) Compare(world, after, label + " pickup", differences);
                    holder.ApplyNativeFrameMotionForWorldPass();
                    holder.ExecuteNativePhysicsForWorldPass(1);
                    Compare(world, row["holderAfterPhysics"], label + " physics", differences);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    if (observer.Calls.Count != (int)row["nativeCalls"] || observer.CrtCalls != (int)row["crtCalls"] || world.Rng.CallCount != legacyBefore)
                        differences.Add(label + " RNG differs");
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + profile + "-" + shadow + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(1200));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        internal static void Compare(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            int slot = (int)expected["raw"]["slot"];
            var raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"].Single(e => (int)e["slot"] == slot);
            foreach (var field in Flatten(expected["raw"], ""))
            {
                if (NTSD28UnityEntityRawCapture.MissingBindings.Contains(field.Key)) continue;
                var value = raw.SelectToken(field.Key);
                bool numeric = field.Value.Type == JTokenType.Integer || field.Value.Type == JTokenType.Float;
                bool equal = numeric ? value != null && value.Type != JTokenType.Null && (double)value == (double)field.Value : JToken.DeepEquals(value, field.Value);
                if (!equal) differences.Add(label + " slot=" + slot + " " + field.Key + "=" + value + " expected " + field.Value);
            }
            var entity = world.FindEntityByRuntimeSlotForQuery(slot);
            if (entity.Runtime.LinkState != (int)expected["relation"] || entity.Runtime.PickupCount != (int)expected["count"] ||
                entity.Runtime.TargetSlotIndex != (int)expected["child"] || entity.Runtime.HolderStableId != (int)expected["parent"])
                differences.Add(label + " slot=" + slot + " relation fields differ");
            var descriptor = expected["descriptor"];
            var actual = entity.Frame.D;
            if (descriptor.Type == JTokenType.Null)
            {
                if (actual != null) differences.Add(label + " slot=" + slot + " unexpected descriptor");
            }
            else if (actual == null || actual.frameId != (int)descriptor["id"] || actual.state != (int)descriptor["state"] || actual.wait != (int)descriptor["wait"])
                differences.Add(label + " slot=" + slot + " native descriptor differs");
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

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity holder, out LF2Entity target, bool renderer = false)
        {
            int oid = (int)row["oid"], type = (int)row["type"];
            var a = Definition(row, true); var t = Definition(row, false);
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "holder.dat"), new ObjectDefinition(oid, type, "pickup.dat") }, id => id == 77 ? a : t);
            var pair = new LF2Entity[2];
            for (int i = 0; i < 2; i++)
            {
                int slot = i == 0 ? 0 : 50;
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = i + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = i == 0 ? 77 : oid, action = 0 }
                };
                var entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(entity, Is.Not.Null);
                pair[i] = entity;
                entity.Team = i + 1;
                entity.Runtime.SetPosition(i == 0 ? 100.25 : 110.25, -10, 200);
                entity.Runtime.SyncIntegerPosition();
                entity.Trans.SyncDirectFrameData(100, 0, i == 0 ? 23 : 24);
                entity.AttackingCounter = i == 0 ? 7 : 9;
                entity.Runtime.SetVelocity(i == 0 ? 2.5 : -2, -.5, .75);
                entity.Runtime.TargetSlotIndex = 0; entity.Runtime.HeldWeaponStableId = 0; entity.Runtime.HolderStableId = 0;
                if (i == 1) entity.Health.HP = entity.Health.HPBound = entity.Health.HP3 = (int)row["hp"];
            }
            holder = pair[0]; target = pair[1];
            holder.Runtime.PickupCount = 5;
            holder.Runtime.KeyJump = 1; holder.Runtime.PrevJump = 0;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(holder.Runtime.HitCandidateCount, Is.EqualTo(1), "pickup candidate");
            int action = (int)row["action"];
            target.WriteCurrentFrameId(action);
            target.Frame.D = target.FrameCache.GetNativeFrameDataById(action);
            if (target.Frame.D != null) target.Trans.SyncDirectFrameData(target.Frame.D.wait, target.Frame.D.next, 24);
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool holder)
        {
            int action = (int)row["action"], weaponAction = (int)row["weaponAction"];
            bool declared = (bool)row["declared"], holderDeclared = (bool)row["holderDeclared"];
            string key = holder + "/" + row["type"] + "/" + row["oid"] + "/" + action + "/" + declared + "/" + weaponAction + "/" + holderDeclared;
            if (definitions.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: NativePickupLookup\nweapon_hp: 17\n<bmp_end>\n<frame> 0 initial\nstate: " + (holder ? 0 : 1004) + " wait: 100 next: 0\n";
            if (holder) text += "itr:\nkind: 2 x: -20 y: -20 w: 60 h: 60\nitr_end:\n";
            else
            {
                text += "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
                if (action == 0 && declared) text += "wpoint:\nkind: 1 weaponact: " + weaponAction + "\nwpoint_end:\n";
            }
            text += "<frame_end>\n";
            if (!holder && declared && action > 0 && action <= 999)
                text += "<frame> " + action + " target\nstate: 1004 wait: 100 next: 0\nwpoint:\nkind: 1 weaponact: " + weaponAction + "\nwpoint_end:\n<frame_end>\n";
            if (holder && holderDeclared)
            {
                foreach (int id in new[] { 115, 116 }) text += "<frame> " + id + " holder\nstate: 0 wait: 100 next: 0 dvx: 557\n<frame_end>\n";
                if (weaponAction > 0 && weaponAction <= 999) text += "<frame> " + weaponAction + " override\nstate: 0 wait: 100 next: 0 dvx: 553\n<frame_end>\n";
            }
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData { type_sub = holder ? 0 : (int)row["type"], weapon_hp = 17,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties)) };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertToFrameData(frame));
            var wrapper = new LF2CharacterDataWrapper(holder ? 77 : (int)row["oid"], data);
            definitions.Add(key, wrapper);
            return wrapper;
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06Kind2FrameLookupPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_Kind2FrameLookupPlay.request";
        static NTSD28Q06Kind2FrameLookupPlayProbe() { EditorApplication.update += Poll; }

        private static bool Selected(JObject row)
        {
            int action = (int)row["action"], weapon = (int)row["weaponAction"];
            return (action == 998 && (weapon == 998 || weapon == 1000 || weapon == -998)) ||
                (action == 999 && weapon == 999) || (action == 1000 && weapon == 998);
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
                var selected = File.ReadLines(NTSD28Q06Kind2FrameLookupEditorTests.Output + "native.jsonl").Select(JObject.Parse).Where(Selected).ToArray();
                Assert.That(selected.Length, Is.EqualTo(200));
                foreach (bool renderer in new[] { false, true })
                foreach (bool shadow in new[] { false, true })
                foreach (var row in selected)
                {
                    var world = NTSD28Q06Kind2FrameLookupEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out var holder, out var target, renderer);
                    try
                    {
                        string label = "renderer=" + renderer + " shadow=" + shadow + " case=" + row["index"];
                        foreach (var before in row["before"]) NTSD28Q06Kind2FrameLookupEditorTests.Compare(world, before, label + " before", differences);
                        var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                        world.NativeRandom.SetDiagnosticCallObserver(observer);
                        ulong legacy = world.Rng.CallCount;
                        if (shadow)
                        {
                            world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                            world.PostInteractionTickAll(1);
                            var diagnostic = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                            Assert.That(diagnostic.CurrentTickPlanValid, Is.True);
                            Assert.That(diagnostic.ObservedWriterEffectCount, Is.EqualTo(1));
                            Assert.That(diagnostic.LastWriterEffectDifferenceMask, Is.Zero);
                        }
                        else Assert.That(world.InteractionWriter.TryApplyPickup(holder, target, 2), Is.True);
                        foreach (var after in row["after"]) NTSD28Q06Kind2FrameLookupEditorTests.Compare(world, after, label + " pickup", differences);
                        holder.ApplyNativeFrameMotionForWorldPass();
                        holder.ExecuteNativePhysicsForWorldPass(1);
                        NTSD28Q06Kind2FrameLookupEditorTests.Compare(world, row["holderAfterPhysics"], label + " physics", differences);
                        world.NativeRandom.SetDiagnosticCallObserver(null);
                        Assert.That(observer.Calls.Count, Is.EqualTo((int)row["nativeCalls"]));
                        Assert.That(observer.CrtCalls, Is.EqualTo((int)row["crtCalls"]));
                        Assert.That(world.Rng.CallCount, Is.EqualTo(legacy));
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
                Assert.That(cases, Is.EqualTo(800));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_Kind2FrameLookupPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, synthetic kind2 live-frame lookup/raw holder binding and following physics; both factories, direct/Shadow; full held lifecycle and images not certified."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
