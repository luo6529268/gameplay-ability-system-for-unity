#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering.Editor;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class BattleBruteProductionAdmissionEditorTests
    {
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string FormalSha = "336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3";
        private const string ScenarioRoot = "artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/";
        private const string RootTrace = "artifacts/diagnostics/NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001/";
        private const string Output = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH40-BRUTE-PRODUCTION-ADMISSION-20261007/";
        private const string Kind5Output = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH46-KIND5-ADMISSION-20261007/";

        [Test]
        public void OrdinaryBruteDefaultsEnableAdmittedFastPath()
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            Assert.That(query.EnableEmptyItrPairGuardForDiagnostics, Is.True);
            Assert.That(query.EnableBruteEmptyItrRosterForDiagnostics, Is.True);
            Assert.That(query.EnableBruteExactCacheForDiagnostics, Is.True);
            Assert.That(query.EnableBruteGeometryFirstForDiagnostics, Is.True);
            Assert.That(query.FormalCollectorMode, Is.EqualTo(CollisionFormalCollectorMode.Configured));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ProductionRequestDoesNotChangeFrozenWorkloadOrEnableCandidate(int index)
        {
            MethodInfo build = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteProductionRequest", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(build, Is.Not.Null, "ordinary production request entry is missing");
            var actual = (ProductionEntityStressRequest)build.Invoke(null, new object[] { index });
            var original = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            Assert.That(actual.outputPath, Does.Contain("NTSD-OPTIMIZATION-BATCH40-BRUTE-PRODUCTION-ADMISSION-20261007"));
            original.outputPath = actual.outputPath;
            Assert.That(UnityEngine.JsonUtility.ToJson(actual),
                Is.EqualTo(UnityEngine.JsonUtility.ToJson(original)));
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void OffAndOnPreserveApplicableFormalRootTraceAndEveryTick(
            string side, string trace, string expectedSha)
        {
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] legacy = Run(scenario, false, false, ProductionEntityStressMode.Combat1000, 12, native);
            TickRow[] optimized = Run(scenario, true, false, ProductionEntityStressMode.Combat1000, 12, native);
            CompareEveryTick(legacy, optimized);
            SaveNew(side + "-parity.json", new { legacy, optimized, rootSelectedFieldChecksPerPath = 132 });
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void ThousandCanonicalAiFullDriverPreservesEveryTick(ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] legacy = Run(scenario, false, true, mode, 32, null);
            TickRow[] optimized = Run(scenario, true, true, mode, 32, null);
            CompareEveryTick(legacy, optimized);
            SaveNew(mode + "-parity.json", new { legacy, optimized, performanceEvidence = false });
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void Kind5PresencePreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] baseline = Run(scenario, true, false, ProductionEntityStressMode.Combat1000,
                12, native, kind5Presence: false);
            TickRow[] candidate = Run(scenario, true, false, ProductionEntityStressMode.Combat1000,
                12, native, kind5Presence: true);
            CompareEveryTick(baseline, candidate);
            SaveNew(side + "-kind5-parity.json", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" },
                performanceEvidence = false,
            }, Kind5Output);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void Kind5PresenceThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] baseline = Run(scenario, true, true, mode, 32, null, kind5Presence: false);
            TickRow[] candidate = Run(scenario, true, true, mode, 32, null, kind5Presence: true);
            CompareEveryTick(baseline, candidate);
            Assert.That(candidate.Any(row => row.kind5ScansSkipped > 0), Is.True,
                "the presence cache must skip a real scan before qualification can pass");
            SaveNew(mode + "-kind5-parity.json", new { baseline, candidate, performanceEvidence = false },
                Kind5Output);
        }

        private static TickRow[] Run(string scenario, bool enabled, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool? kind5Presence = null)
        {
            var result = new List<TickRow>(ticks);
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                RuntimeRoot, scenario, thousand ? BattleRuntimeProfile.MobileExtended : BattleRuntimeProfile.Authority400,
                ticks, (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    var query = (BruteForceSceneQuery)world.SceneQuery;
                    query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                    SetCandidate(query, enabled);
                    if (kind5Presence.HasValue)
                    {
                        Assert.That(enabled, Is.True, "both paths must retain the admitted production mechanisms");
                        Assert.That(query.EnableBruteKind5PresenceForDiagnostics, Is.False,
                            "qualification must not promote the production default");
                        Assert.That(query.EnableBruteBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteKind5PresenceForDiagnostics = kind5Presence.Value;
                    }
                    if (thousand)
                        PrepareThousandAi(world, mode);
                    ulong before = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                    for (int tick = 1; tick <= ticks; tick++)
                    {
                        Assert.That(driver.StepOneTick(inputs[tick - 1], true, false), Is.True,
                            "completed tick=" + tick + ", fast=" + enabled);
                        if (native != null)
                            CompareRoot(world, native[tick - 1], tick);
                        Assert.That(world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                            Is.Zero, "AI post-commit tick=" + tick);
                        object completeHashes = thousand
                            ? world.CaptureExtendedChecksumSnapshot(tick, inputs[tick - 1]).Hashes
                            : world.CaptureParityFrameSnapshot(tick, inputs[tick - 1],
                                includeFullDomains: true).Hashes;
                        var lockstep = world.CaptureLockstepChecksumSnapshot(tick, inputs[tick - 1]);
                        result.Add(new TickRow
                        {
                            tick = tick,
                            entities = world.ObjectCount,
                            extendedHashes = JsonConvert.SerializeObject(completeHashes),
                            lockstepHashes = JsonConvert.SerializeObject(lockstep.Hashes),
                            nativeCalls = world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                            legacyCalls = world.Rng.CallCount,
                            cacheApplied = query.LastBruteExactCacheAppliedForDiagnostics,
                            fallback = query.LastBruteExactCacheFallbackForDiagnostics,
                            kind5PresenceApplied = query.LastBruteKind5PresenceAppliedForDiagnostics,
                            kind5ScansSkipped = query.LastBruteKind5ScanSkippedForDiagnostics,
                        });
                        if (kind5Presence.HasValue)
                        {
                            Assert.That(query.LastBruteKind5PresenceAppliedForDiagnostics,
                                Is.EqualTo(kind5Presence.Value), "kind5 application tick=" + tick);
                        }
                        if (enabled)
                        {
                            Assert.That(query.LastBruteExactCacheAppliedForDiagnostics, Is.True,
                                "prepared cache must be exercised tick=" + tick);
                            Assert.That(query.LastBruteExactCacheFallbackForDiagnostics, Is.False,
                                "admission may not pass without testing the candidate tick=" + tick);
                        }
                    }
                    if (thousand)
                    {
                        Assert.That(world.AiUnifiedSnapshotExecutionBuildCountForDiagnostics +
                            world.AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics, Is.GreaterThan(0));
                        Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls,
                            Is.GreaterThan(before), "canonical AI RNG must execute");
                    }
                }, useProjectMode: true);
            return result.ToArray();
        }

        private static void PrepareThousandAi(SimulationWorld world, ProductionEntityStressMode mode)
        {
            Assert.That(world.AiExecutionProfile, Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            for (int index = 0; index < 1000; index++)
            {
                LF2Entity entity = index < 2 ? world.FindEntityByRuntimeSlotForQuery(index) :
                    world.LogicEntityFactory.Create(new OPointCreateTask
                    {
                        targetWorld = world,
                        requiredRuntimeSlot = index,
                        dir = (index & 1) == 0 ? "right" : "left",
                        nativeWeaponPieceSpawn = true,
                        relationTeam = (index & 1) + 1,
                        team = (index & 1) + 1,
                        useExplicitRelationIdentity = true,
                        preserveActionZero = true,
                        opoint = new ObjectPoint { oid = 2, action = 0 },
                    }, out _);
                Assert.That(entity, Is.TypeOf<LF2Character>(), "roster index=" + index);
                var character = (LF2Character)entity;
                character.AiControlled = true;
                character.Team = (index & 1) + 1;
                character.RelationTeam = character.Team;
                character.ImmediateFrame(0);
                UnityEngine.Vector3 position = ProductionEntityStressRunner.BuildSpawnPosition(mode, index, 1000);
                character.Runtime.SetPosition(position.x, 0, position.z);
                character.Runtime.SyncIntegerPosition();
                character.Runtime.SetSourceRulePosition(position.x, position.z);
                character.Runtime.SyncSourceRuleIntegerPosition();
            }
            Assert.That(world.ObjectCount, Is.EqualTo(1000));
            Assert.That(Enumerable.Range(0, 1000).Select(world.FindEntityByRuntimeSlotForQuery)
                .OfType<LF2Character>().Count(value => value.AiControlled), Is.EqualTo(1000));
        }

        private static void CompareRoot(SimulationWorld world, JObject native, int tick)
        {
            foreach (JObject expected in native["entities"])
            {
                var actual = world.FindEntityByRuntimeSlotForQuery((int)expected["slot"]);
                Assert.That(actual, Is.Not.Null, "root slot tick=" + tick);
                Assert.That(actual.ObjectId, Is.EqualTo((int)expected["oid"]), "root oid tick=" + tick);
                Assert.That(actual.Frame.D?.frameId, Is.EqualTo((int)expected["action"]), "root action tick=" + tick);
                Assert.That(actual.Health.HP, Is.EqualTo((int)expected["hp"]), "root HP tick=" + tick);
                Assert.That(actual.Runtime.Vx, Is.EqualTo((double)expected["vx"]).Within(0.000001),
                    "root Vx printed precision tick=" + tick);
            }
        }

        private static void CompareEveryTick(TickRow[] legacy, TickRow[] optimized)
        {
            Assert.That(optimized.Length, Is.EqualTo(legacy.Length));
            for (int index = 0; index < legacy.Length; index++)
            {
                TickRow a = legacy[index], b = optimized[index];
                string label = "first-difference tick=" + a.tick;
                Assert.That(b.tick, Is.EqualTo(a.tick), label);
                Assert.That(b.entities, Is.EqualTo(a.entities), label + " entities");
                Assert.That(b.extendedHashes, Is.EqualTo(a.extendedHashes), label + " extended domains");
                Assert.That(b.lockstepHashes, Is.EqualTo(a.lockstepHashes), label + " lockstep domains");
                Assert.That(b.nativeCalls, Is.EqualTo(a.nativeCalls), label + " native RNG");
                Assert.That(b.legacyCalls, Is.EqualTo(a.legacyCalls), label + " legacy RNG");
            }
        }

        private static void SetCandidate(BruteForceSceneQuery query, bool enabled)
        {
            query.EnableEmptyItrPairGuardForDiagnostics = enabled;
            query.EnableBruteEmptyItrRosterForDiagnostics = enabled;
            query.EnableBruteExactCacheForDiagnostics = enabled;
            query.EnableBruteGeometryFirstForDiagnostics = enabled;
        }

        private static string HashFile(string path)
        {
            using var hash = SHA256.Create();
            using var stream = File.OpenRead(path);
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }

        private static void SaveNew(string name, object value, string output = Output)
        {
            Directory.CreateDirectory(output);
            string path = output + Path.GetFileNameWithoutExtension(name) + "-" +
                Guid.NewGuid().ToString("N") + ".json";
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private sealed class TickRow
        {
            public int tick;
            public int entities;
            public string extendedHashes;
            public string lockstepHashes;
            public ulong nativeCalls;
            public ulong legacyCalls;
            public bool cacheApplied;
            public bool fallback;
            public bool kind5PresenceApplied;
            public long kind5ScansSkipped;
        }
    }
}
#endif
