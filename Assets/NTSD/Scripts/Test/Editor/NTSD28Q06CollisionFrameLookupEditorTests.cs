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
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06CollisionFrameLookupEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-COLLISION-FRAME-UNITY-001/";
        internal const string Source = "artifacts/diagnostics/NTSD28-Q06-COLLISION-FRAME-SOURCE-WITNESS-001/expanded/first.jsonl";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void CollisionSnapshotAndCatchMatchOriginal(BattleRuntimeProfile profile, bool roleAware)
        {
            Directory.CreateDirectory(Output);
            var differences = new List<string>();
            var candidateDifferences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile, roleAware, out var attacker, out var target);
                try { RunCase(world, attacker, target, row, differences, candidateDifferences); }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + profile + "-" + roleAware + ".json", JsonConvert.SerializeObject(new
            {
                cases, differences, candidateDifferences,
                scope = "Original endpoint contract including explicitly diagnostic separated current/snapshot; full-driver reachability is separate."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(336));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
            Assert.That(candidateDifferences, Is.Empty, string.Join("\n", candidateDifferences.Take(12)));
        }

        internal static void RunCase(SimulationWorld world, LF2Entity attacker, LF2Entity target, JObject row,
            List<string> differences, List<string> candidateDifferences)
        {
            string label = "case " + row["index"];
            NTSD28Q06Kind3FrameLookupEditorTests.ComparePair(world, row["before"], label + " before", differences);
            foreach (var entity in new[] { attacker, target })
            {
                int slot = entity.Runtime.SlotIndex;
                var expected = row["before"][slot];
                var frame = entity.GetCollisionFrameData();
                var native = entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev2);
                if ((frame != null) != (bool)expected["snapshotAvailable"] || !ReferenceEquals(frame, native))
                    differences.Add(label + " slot " + slot + " collision descriptor identity/availability");
                if ((frame?.centerx ?? 0) != (int)expected["snapshotCenterX"])
                    differences.Add(label + " slot " + slot + " snapshot centerx=" + frame?.centerx);
            }
            ulong legacy = world.Rng.CallCount;
            var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            world.CollectCollisionCandidatesAll();
            if (attacker.Runtime.HitCandidateCount != (int)row["candidates"])
                candidateDifferences.Add(label + " current=" + row["current"] + " snapshot=" + row["snapshot"] +
                    " count=" + attacker.Runtime.HitCandidateCount + " expected=" + row["candidates"]);
            NTSD28Q06Kind3FrameLookupEditorTests.ComparePair(world, row["afterCollection"], label + " collection", differences);
            foreach (var entity in new[] { attacker, target })
            {
                if (world.CpointWriter.ShouldRunKind1Advance(entity)) world.CpointWriter.RunKind1(world, entity);
                else world.CpointWriter.RunKind2Validation(world, entity);
            }
            NTSD28Q06Kind3FrameLookupEditorTests.ComparePair(world, row["afterCatch"], label + " catch", differences);
            world.NativeRandom.SetDiagnosticCallObserver(null);
            if (world.Rng.CallCount != legacy || observer.Calls.Count != 0 || observer.CrtCalls != 0)
                differences.Add(label + " RNG changed");
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, bool roleAware,
            out LF2Entity attacker, out LF2Entity target, bool renderer = false)
        {
            var definitions = new[] { Definition(row, true, false), Definition(row, false, false) };
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000,
                roleAware ? CollisionBroadphaseBackend.LooseQuadtree : CollisionBroadphaseBackend.BruteForce);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "collision-a.dat"), new ObjectDefinition(78, 0, "collision-t.dat") }, id => definitions[id - 77]);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.ForceRoleAwareTreeForDiagnostics = roleAware;
            var pair = new LF2Entity[2];
            for (int slot = 0; slot < 2; slot++)
            {
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                };
                var entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(entity, Is.Not.Null);
                pair[slot] = entity;
                entity.Team = slot + 1;
                entity.Runtime.SetPosition(100 + slot * 10, -5, 200);
                entity.Runtime.SyncIntegerPosition();
                entity.DirectWriteNativeRawFramePreserveWaitCounter(slot == 0 || (bool)row["both"] ? (int)row["snapshot"] : 0);
            }
            world.CaptureCollisionFrameSnapshotsAll();
            for (int slot = 0; slot < 2; slot++)
            {
                var entity = pair[slot];
                if ((int)row["replaceSide"] == slot + 1) entity.FrameCache.Load(Definition(row, slot == 0, true));
                entity.DirectWriteNativeRawFramePreserveWaitCounter((int)row["current"]);
                entity.Trans.SyncWaitCounterFrame(11 + slot);
                entity.AttackingCounter = 7 + slot;
            }
            attacker = pair[0]; target = pair[1];
            attacker.Runtime.CaughtSlotIndex = 1;
            attacker.Runtime.CaughtDuration = 80;
            target.Runtime.CatchSourceSlot90 = 0;
            target.Runtime.CatcherSlotIndex = 0;
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool attacker, bool replacement)
        {
            int snapshot = (int)row["snapshot"];
            bool declared = (bool)row["declared"];
            string key = attacker + "/" + snapshot + "/" + declared + "/" + replacement;
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: NativeCollisionLookup\n<bmp_end>\n";
            void Append(int action)
            {
                text += "<frame> " + action + " collision\nstate: 0 wait: 100 next: 0 centerx: " + (replacement && !attacker ? 500 : 0) + " centery: 0\n";
                text += attacker ? "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 1\nitr_end:\n" : "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
                text += "cpoint:\nkind: " + (attacker ? 1 : 2) + " decrease: " + (replacement ? 5 : 2) + " hurtable: 1\ncpoint_end:\n<frame_end>\n";
            }
            Append(0); Append(10);
            if (declared && snapshot > 0 && snapshot != 10 && snapshot <= 999) Append(snapshot);
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
    internal static class NTSD28Q06CollisionFrameLookupPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_CollisionFrameLookupPlay.request";

        static NTSD28Q06CollisionFrameLookupPlayProbe() { EditorApplication.update += Poll; }

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
            var candidateDifferences = new List<string>();
            try
            {
                var selected = File.ReadLines(NTSD28Q06CollisionFrameLookupEditorTests.Source).Select(JObject.Parse)
                    .Where(row => (int)row["index"] >= 252 && (bool)row["both"]).ToArray();
                Assert.That(selected.Length, Is.EqualTo(42));
                foreach (bool renderer in new[] { false, true })
                foreach (bool roleAware in new[] { false, true })
                foreach (var row in selected)
                {
                    var world = NTSD28Q06CollisionFrameLookupEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400,
                        roleAware, out var attacker, out var target, renderer);
                    try
                    {
                        NTSD28Q06CollisionFrameLookupEditorTests.RunCase(world, attacker, target, row, differences, candidateDifferences);
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
                Assert.That(candidateDifferences, Is.Empty, string.Join("\n", candidateDifferences.Take(10)));
                Assert.That(cases, Is.EqualTo(168));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_CollisionFrameLookupPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, candidateDifferences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, synthetic equal current/snapshot endpoints, both factories and query backends; separated-current candidate differences remain open."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
