#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public sealed class NTSD28Q06DeadCharacterPreludeEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-DEAD-CHARACTER-FRAME-AND-HELD-SOURCE-WITNESS-001/native.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-C25-EXTRA-DEATH-PRELUDE-RETIREMENT-001/";

        [Test]
        public void DeadActionSelfCheckUsesActualLatePass()
        {
            var method = typeof(BattleRuntimeSelfCheck).GetMethod("CheckDeadCharacterLatePassDoesNotInjectBounce",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, null);
        }

        [TestCase(false, BattleEcsCharacterFrameTickPassMode.Legacy)]
        [TestCase(false, BattleEcsCharacterFrameTickPassMode.DataOriented)]
        [TestCase(true, BattleEcsCharacterFrameTickPassMode.Legacy)]
        [TestCase(true, BattleEcsCharacterFrameTickPassMode.DataOriented)]
        public void C25DoesNotInjectDeathMotionOrHeldRelease(bool sharedShell, BattleEcsCharacterFrameTickPassMode mode)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (string line in File.ReadLines(Witness))
            {
                JObject row = JObject.Parse(line);
                if ((int)row["phase"] != 0) continue;
                int action = (int)row["action"], state = (int)row["state"], relation = (int)row["relation"];
                var data = new LF2CharacterData();
                data.frames.Add(new LF2FrameData { frameId = action, state = state, wait = 100, next = 0 });
                var wrapper = new LF2CharacterDataWrapper(777, data);
                var childData = new LF2CharacterData { weapon_hp = 99 };
                childData.frames.Add(new LF2FrameData { frameId = 0, state = 1001, wait = 100, next = 0 });
                var childWrapper = new LF2CharacterDataWrapper(888, childData);
                var world = new SimulationWorld();
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(mode);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(777, 0, "dead-parent.dat"),
                    new ObjectDefinition(888, relation == 2 ? 2 : 1, "held-child.dat"),
                }, oid => oid == 777 ? wrapper : childWrapper);
                LF2Entity entity = sharedShell ? new LF2OtherObject() : new LF2Character();
                try
                {
                    entity.ObjectId = 777;
                    entity.FrameCache.Load(wrapper);
                    entity.SetRequiredRuntimeSlot(0);
                    world.Register(entity);
                    entity.WriteCurrentFrameId(action);
                    entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(action);
                    entity.Trans.SyncDirectFrameData(100, 0, action);
                    entity.Frame.Prev = action;
                    entity.SyncCollisionSnapshotToCurrentFrame();
                    var before = row["before"]["raw"];
                    entity.Health.HP = (int)row["hp"];
                    entity.Health.HPBound = 500; entity.Health.HP3 = 500; entity.Health.PP = 500;
                    entity.Runtime.HP2Orig = 1;
                    entity.Runtime.SetPosition((double)before["position"]["preciseX"], (double)before["position"]["preciseY"], (double)before["position"]["preciseZ"]);
                    entity.Runtime.SyncIntegerPosition();
                    entity.Runtime.SetVelocity((double)before["motion"]["x"], (double)before["motion"]["y"], (double)before["motion"]["z"]);
                    entity.HitStun = (int)row["render"];
                    LF2Weapon child = null;
                    if (relation != 0)
                    {
                        child = new LF2Weapon { ObjectId = 888 };
                        child.SetWeaponType(relation == 2 ? 2 : 1);
                        child.FrameCache.Load(childWrapper);
                        child.Frame.D = child.FrameCache.GetNativeFrameDataById(0);
                        child.SetRequiredRuntimeSlot(70);
                        world.Register(child);
                        child.Health.HP = 500;
                        child.Runtime.WeaponFlightCounter = 99;
                        entity.Runtime.LinkState = relation == 2 ? 2 : 1;
                        entity.Runtime.TargetSlotIndex = 70;
                        entity.Runtime.HeldWeaponStableId = 70;
                        child.Runtime.LinkState = relation == 2 ? -2 : -1;
                        child.Runtime.HolderStableId = 0;
                    }
                    var randomBefore = world.NativeRandom.CaptureScalarState();
                    uint legacyState = world.Rng.State;
                    ulong legacyCalls = world.Rng.CallCount;
                    world.LateEntityUpdateAll(1);
                    var expected = row["after"]["raw"];
                    bool same = entity.Frame.N == (int)expected["frame"]["action"] &&
                        entity.Runtime.X == (double)expected["position"]["preciseX"] &&
                        entity.Runtime.Y == (double)expected["position"]["preciseY"] &&
                        entity.Runtime.Z == (double)expected["position"]["preciseZ"] &&
                        entity.Runtime.Vx == (double)expected["motion"]["x"] &&
                        entity.Runtime.Vy == (double)expected["motion"]["y"] &&
                        entity.Runtime.Vz == (double)expected["motion"]["z"] &&
                        entity.Runtime.LinkState == (int)row["after"]["link"];
                    if (child != null)
                        same &= entity.Runtime.ResolveActiveHeldSlotIndex() == 70 && child.Runtime.HolderStableId == 0 &&
                            child.Runtime.LinkState == (int)row["heldAfter"]["link"];
                    var randomAfter = world.NativeRandom.CaptureScalarState();
                    same &= randomAfter.CrtCalls == randomBefore.CrtCalls && randomAfter.SynchronizedCalls == randomBefore.SynchronizedCalls &&
                        world.Rng.State == legacyState && world.Rng.CallCount == legacyCalls;
                    if (!same) differences.Add("case " + cases + " input=" + action + "/" + state + "/" + row["hp"] +
                        "/" + row["motion"] + "/" + relation + " actual=" + entity.Frame.N + "/y" + entity.Runtime.Y +
                        "/vy" + entity.Runtime.Vy + "/link" + entity.Runtime.LinkState);
                }
                finally
                {
                    world.BeginBattleShutdown();
                    Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                }
                cases++;
            }
            File.WriteAllText(Output + sharedShell + "-" + mode + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(3240));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(8)));
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06DeadPreludePlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_DeadPreludePlay.request";
        private const string Result = "Temp/NTSD28_Q06_DeadPreludePlay.result.json";
        static NTSD28Q06DeadPreludePlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var report = new Report { tick = driver.CurrentTickIndex, before = world.ObjectCount };
            try
            {
                var parent = world.FindEntityByRuntimeSlotForQuery(0) as LF2Character;
                Assert.That(parent?.Renderer, Is.Not.Null);
                var frame = parent.FrameCache.GetNativeFrameDataById(0);
                Assert.That(frame != null && (frame.wait > 0 || frame.next == 0), Is.True, "Standing fixture must not advance on its first step.");
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, saved), Is.True);
                var input = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum;
                bool originalMode = world.UsesLogicOnlyEntityMaterialization;
                LF2Entity child = null;
                try
                {
                    world.SetLogicOnlyEntityMaterialization(true);
                    child = world.ResolveObjectPointFactoryForSimulation().CreateObjectImmediate(new OPointCreateTask
                    {
                        targetWorld = world, parent = parent, opoint = new ObjectPoint { oid = 100, kind = 2, action = 0 },
                        dir = "right", preserveActionZero = true, releaseOpointSpawn = true, inheritParentRelation = true,
                        useDirectRuntimePosition = true, directX = parent.Runtime.X, directY = 0, directZ = parent.Runtime.Z,
                        skipPostInitZOffset = true,
                    });
                    Assert.That(child, Is.Not.Null);
                    int childSlot = child.Runtime.SlotIndex;
                    Assert.That(parent.Runtime.LinkState, Is.EqualTo(1));
                    Assert.That(child.Runtime.LinkState, Is.EqualTo(-1));
                    parent.WriteCurrentFrameId(0);
                    parent.Frame.D = frame;
                    parent.Trans.SyncDirectFrameData(frame.wait, frame.next, 0);
                    parent.AttackingCounter = 0;
                    parent.Health.HP = 0;
                    parent.HitStun = 0;
                    parent.Runtime.Y = 0; parent.Runtime.YInt = 0;
                    parent.Runtime.SetVelocity(0, 0, 0);
                    parent.KnockbackVy = 0;
                    world.LateEntityUpdateAll(report.tick);
                    report.frame = parent.Frame.N; report.y = parent.Runtime.Y; report.vy = parent.Runtime.Vy;
                    report.parentLink = parent.Runtime.LinkState; report.childLink = child.Runtime.LinkState;
                    Assert.That(report.frame, Is.Zero);
                    Assert.That(report.y, Is.Zero);
                    Assert.That(report.vy, Is.Zero);
                    Assert.That(parent.Runtime.ResolveActiveHeldSlotIndex(), Is.EqualTo(childSlot));
                    Assert.That(report.parentLink, Is.EqualTo(1));
                    Assert.That(report.childLink, Is.EqualTo(-1));
                    Assert.That(child.Runtime.HolderStableId, Is.Zero);
                    report.survived = ReferenceEquals(world.FindEntityByRuntimeSlotForQuery(0), parent);
                    Assert.That(report.survived, Is.True);
                }
                finally
                {
                    if (child?.Runtime?.SlotIndex >= 0) child.FreeEntityLikeExe();
                    world.SetLogicOnlyEntityMaterialization(originalMode);
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    report.restored = world.CaptureLockstepChecksumSnapshot(report.tick, input).OverallChecksum == checksum;
                    report.after = world.ObjectCount;
                }
                Assert.That(report.restored, Is.True);
                Assert.That(report.after, Is.EqualTo(report.before));
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Legacy-content real Scene, HP0 standing holder plus actual kind2 child, complete C25 Late pass only; no artificial bounce/drop, child recycled, original checksum restored. Not a new-hit/full-physics/formal-image certificate.";
            public int tick, before, after, frame, parentLink, childLink;
            public double y, vy;
            public bool survived, restored;
        }
    }
}
#endif
