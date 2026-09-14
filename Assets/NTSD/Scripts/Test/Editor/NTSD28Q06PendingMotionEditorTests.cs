#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06PendingMotionEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-PENDING-PRE-C25-MOTION-GUARD-001/";

        [TestCase(BattleEcsCharacterFrameAdvancePassMode.Legacy)]
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.DataOriented)]
        public void PendingBlocksMotionAndPhysicsButKeepsIndependentDeadNormalization(BattleEcsCharacterFrameAdvancePassMode mode)
        {
            var differences = new List<string>();
            int cases = 0;
            for (int type = 0; type <= 6; type++)
            foreach (int delay in new[] { -3, 0, 3 })
            foreach (int hp in new[] { 0, 500 })
            {
                var world = MakeWorld(type, out var entity);
                try
                {
                    world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(mode);
                    entity.Runtime.NativeLifecycleResolutionPending = true;
                    entity.Runtime.NativeLifecycleCode = 1101;
                    entity.FrameDelay = delay;
                    entity.Health.HP = hp;
                    world.NativeFrameMotionAll();
                    world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                    world.SerialTickAll(1, true, true);
                    if (entity.Runtime.X != 100.25 || entity.Runtime.Y != -20.5 || entity.Runtime.Z != 200.75 ||
                        entity.Runtime.XInt != 100 || entity.Runtime.YInt != -20 || entity.Runtime.ZInt != 200 ||
                        entity.Runtime.Vx != 3.25 || entity.Runtime.Vy != -1.25 || entity.Runtime.Vz != .75 || entity.FrameDelay != delay)
                        differences.Add(type + "/" + delay + "/" + hp + " moved or consumed hold");
                    if (entity.Health.HPBound != (type == 0 && hp == 0 ? 0 : 500) || entity.Health.PP != (type == 0 && hp == 0 ? 0 : 200))
                        differences.Add(type + "/" + delay + "/" + hp + " dead normalization");
                    if (!entity.Runtime.NativeLifecycleResolutionPending || entity.Runtime.NativeLifecycleCode != 1101)
                        differences.Add("pending lifecycle changed before C25");
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + "pending-" + mode + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(42));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
        }

        [TestCase(BattleEcsCharacterFrameAdvancePassMode.Legacy)]
        [TestCase(BattleEcsCharacterFrameAdvancePassMode.DataOriented)]
        public void NonPendingStillAppliesMotionAndAdvancesHold(BattleEcsCharacterFrameAdvancePassMode mode)
        {
            var world = MakeWorld(0, out var entity);
            try
            {
                world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(mode);
                entity.FrameDelay = 3;
                world.NativeFrameMotionAll();
                Assert.That(entity.Runtime.Vx, Is.Not.EqualTo(3.25));
                world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                Assert.That(entity.FrameDelay, Is.EqualTo(2));
                Assert.That(entity.Runtime.X, Is.EqualTo(100.25));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [TestCase(BattleEcsCharacterFrameTickPassMode.Legacy)]
        [TestCase(BattleEcsCharacterFrameTickPassMode.DataOriented)]
        public void PendingFullDriverParticlesMatchOriginal(BattleEcsCharacterFrameTickPassMode mode)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (string line in File.ReadLines(NTSD28Q06State18SpawnEditorTests.Witness))
            {
                var row = JObject.Parse(line);
                if ((int)row["phase"] != 1 || (int)row["pending"] != 1) continue;
                var world = NTSD28Q06State18SpawnEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out _);
                try
                {
                    world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.ResetFromSeed((uint)row["seed"]);
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    NTSD28Q06State18SpawnEditorTests.CompareChildren(world, row, "pending " + cases, differences);
                    var actualSource = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"]
                        .SingleOrDefault(e => (int)e["slot"] == (int)row["source"]);
                    foreach (string field in new[] { "position", "motion", "combat.runtimeStateCode", "lifecycle",
                        "frame.action", "frame.actionLatch", "frame.previousAction", "frame.tickActionSnapshot" })
                    {
                        if (!JToken.DeepEquals(actualSource?.SelectToken(field), row["sourceAfter"].SelectToken(field)))
                            differences.Add("pending source " + cases + " " + field + "=" + actualSource?.SelectToken(field) + " expected " + row["sourceAfter"].SelectToken(field));
                    }
                    if (!JToken.DeepEquals(JArray.FromObject(observer.Calls), row["calls"])) differences.Add("native RNG " + cases);
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
                cases++;
            }
            File.WriteAllText(Output + "full-" + mode + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(6));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
        }

        private static SimulationWorld MakeWorld(int type, out LF2Entity entity)
        {
            var data = new LF2CharacterData { type_sub = type };
            data.frames.Add(new LF2FrameData { frameId = 0, state = 0, wait = 100, next = 0, dvx = 557, dvy = 558, dvz = 559 });
            var wrapper = new LF2CharacterDataWrapper(888, data);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(888, type, "pending-motion.dat") }, _ => wrapper);
            entity = type == 0 ? new LF2Character() : type == 3 ? new LF2SpecialAttack() : type == 5 ? new LF2OtherObject() : new LF2Weapon();
            if (entity is LF2Weapon weapon) weapon.SetWeaponType(type);
            entity.ObjectId = 888;
            entity.FrameCache.Load(wrapper);
            entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(0);
            entity.Trans.SyncDirectFrameData(100, 0, 0);
            entity.SetRequiredRuntimeSlot(20); world.Register(entity);
            entity.Runtime.SetPosition(100.25, -20.5, 200.75);
            entity.Runtime.XInt = 100; entity.Runtime.YInt = -20; entity.Runtime.ZInt = 200;
            entity.Runtime.SetVelocity(3.25, -1.25, .75);
            entity.Health.HP = 500; entity.Health.HPBound = 500; entity.Health.PP = 200;
            return world;
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06PendingMotionPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_PendingMotionPlay.request";

        static NTSD28Q06PendingMotionPlayProbe()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = world.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            string status = "FAIL", error = null;
            try
            {
                var tests = new NTSD28Q06PendingMotionEditorTests();
                tests.PendingFullDriverParticlesMatchOriginal(BattleEcsCharacterFrameTickPassMode.Legacy);
                tests.PendingFullDriverParticlesMatchOriginal(BattleEcsCharacterFrameTickPassMode.DataOriented);
                Assert.That(world.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_PendingMotionPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases = 12, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = world.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Play, isolated logic-only pending fixtures; full driver and both frame backends. No image or physical-input certification."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
