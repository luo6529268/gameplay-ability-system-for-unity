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
    public sealed class NTSD28Q06Type5UnarmoredEditorTests
    {
        internal const string Source = "artifacts/diagnostics/NTSD28-Q06-TYPE5-UNARMORED-SOURCE-WITNESS-001/first.jsonl";
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-TYPE5-UNARMORED-UNITY-001/";

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity[] pair, bool renderer = false)
            => NTSD28Q06WeaponReactionEditorTests.MakeWorld(row, profile, out pair, renderer);

        internal static void RunCase(SimulationWorld world, LF2Entity[] pair, JObject row, bool shadow, List<string> before, List<string> differences)
            => NTSD28Q06WeaponReactionEditorTests.RunCase(world, pair, row, shadow, before, differences);

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void Type5StateSurvivesLocalSnapshotReplay(BattleRuntimeProfile profile)
        {
            var sourceRows = File.ReadLines(Source).Select(JObject.Parse).ToArray();
            var rows = sourceRows.Where(row => (string)row["group"] == "tiers" && (int)row["targetY"] == 0 && (int)row["targetFacing"] == 0)
                .GroupBy(row => (int)row["after"]["raw"][1]["combat"]["hitReactionTimer"])
                .Select(group => group.First()).ToList();
            rows.Add(sourceRows.First(row => (int)row["attackerState"] == 1002));
            Assert.That(rows.Count, Is.EqualTo(7));
            foreach (var row in rows)
            {
                var world = MakeWorld(row, profile, out var entities);
                try
                {
                    var before = new List<string>();
                    var differences = new List<string>();
                    RunCase(world, entities, row, false, before, differences);
                    Assert.That(before, Is.Empty);
                    Assert.That(differences, Is.Empty);
                    world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                    var driver = new NTSDBattleTickSystem(world);
                    driver.RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    var staleCursor = world.NativeRandom.CaptureSynchronizedCursor();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 1, snapshot), Is.True);
                    var signatures = new List<string>();
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                        signatures.Add(ReplaySignature(world, entities, tick));
                    }
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    Assert.That(world.NativeRandom.CanCommitSynchronizedCursor(staleCursor), Is.False);
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                        Assert.That(ReplaySignature(world, entities, tick), Is.EqualTo(signatures[tick - 2]));
                        Assert.That(world.BattleHitExecutionPlanDiagnosticsForDiagnostics.CurrentTickPlanValid, Is.True);
                    }
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
        }

        private static string ReplaySignature(SimulationWorld world, LF2Entity[] entities, int tick)
        {
            var rest = new int[2, 2];
            for (int target = 0; target < 2; target++)
                for (int attacker = 0; attacker < 2; attacker++) rest[target, attacker] = world.GetRawRestVrest(target, attacker);
            var random = world.NativeRandom.CaptureScalarState();
            return JsonConvert.SerializeObject(new
            {
                checksum = world.CaptureLockstepChecksumSnapshot(tick, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>())).OverallChecksum,
                rest,
                random = new
                {
                    random.CrtState, random.CrtCalls, random.TableSeed, random.SynchronizedCounter, random.SynchronizedIndex,
                    random.SynchronizedCalls, random.LastSynchronizedCallSite, random.SynchronizedTableHash
                },
                entities = entities.Select(entity => new
                {
                    entity.HitCount, entity.KnockbackVx, entity.KnockbackVy, entity.KnockbackVz,
                    entity.Runtime.LinkState, entity.Runtime.HolderStableId, entity.Runtime.TargetSlotIndex,
                    records = Enumerable.Range(0, entity.HitRecordCount).Select(i => new[] { entity.GetHitRecordAge(i), entity.GetHitRecordX(i), entity.GetHitRecordZ(i) }).ToArray()
                }).ToArray()
            });
        }

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void Type5CompleteTransactionMatchesSource(BattleRuntimeProfile profile, bool shadow)
        {
            var before = new List<string>();
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse))
            {
                var world = NTSD28Q06WeaponReactionEditorTests.MakeWorld(row, profile, out var pair);
                try
                {
                    NTSD28Q06WeaponReactionEditorTests.RunCase(world, pair, row, shadow, before, differences);
                    cases++;
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + shadow + ".json",
                JsonConvert.SerializeObject(new { cases, before, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(585));
            Assert.That(before, Is.Empty, string.Join("\n", before.Take(12)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06Type5UnarmoredPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_Type5UnarmoredPlay.request";

        static NTSD28Q06Type5UnarmoredPlayProbe() { EditorApplication.update += Poll; }

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
            var beforeDifferences = new List<string>();
            try
            {
                var selected = File.ReadLines(NTSD28Q06Type5UnarmoredEditorTests.Source).Select(JObject.Parse)
                    .ToArray();
                Assert.That(selected.Length, Is.EqualTo(585));
                foreach (bool renderer in new[] { false, true })
                foreach (bool shadow in new[] { false, true })
                foreach (var row in selected)
                {
                    var world = NTSD28Q06Type5UnarmoredEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out var entities, renderer);
                    try
                    {
                        NTSD28Q06Type5UnarmoredEditorTests.RunCase(world, entities, row, shadow, beforeDifferences, differences);
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
                Assert.That(cases, Is.EqualTo(2340));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(10)));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_Type5UnarmoredPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, beforeDifferences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, source585 synthetic type5 transactions through both factories and direct/Shadow. This does not validate physical keys or visual asset alignment."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
