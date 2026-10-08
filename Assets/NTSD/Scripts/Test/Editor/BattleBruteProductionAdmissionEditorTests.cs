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
        private const string EligibilityOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH49-BRUTE-ELIGIBILITY-REUSE-20261007/driver-qualification-01/";
        private const string CombinedOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH51-COMBINED-CACHE-ADMISSION-20261008/driver-qualification-01/";
        private const string CoarseEnvelopeOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH59-COARSE-ENVELOPE-DRIVER-20261008/driver-qualification-01/";
        private const string EnvelopeBindingOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH62-ENVELOPE-BINDING-DRIVER-20261008/driver-qualification-01/";

        private const string EnvelopeBindingEligibilityOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH64-ENVELOPE-BINDING-ELIGIBILITY-20261008/driver-qualification-01/";

        private const string OrdinalPacketDriverOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH71-BRUTE-ORDINAL-PACKET-DRIVER-20261008/driver-qualification-01/";

        private const string DepthRejectDriverOutput = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH76-BRUTE-DEPTH-DRIVER-20261008/driver-qualification-01/";

        [Test]
        public void DepthRejectDriver_DefaultOffAndExplicitQualificationOnly()
        {
            RequireDepthRejectDriver();
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            Assert.That(query.EnableBruteDepthRejectForDiagnostics, Is.False);
            Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, Is.False);
            Assert.That(query.EnableBruteCoarseProofReuseForDiagnostics, Is.False);
            Assert.That(query.EnableBruteEnvelopeBranchTimingForDiagnostics, Is.False);
            CoarseEnvelopeQualificationDoesNotPromoteAnyCandidateDefault();
            OrdinaryBruteDefaultsEnableAdmittedFastPath();
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void DepthRejectDriver_PreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            RequireDepthRejectDriver();
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] candidate = InvokeDepthRejectDriver(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, true);
            TickRow[] baseline = InvokeDepthRejectDriver(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, false);
            CompareDepthRejectDriverRows(baseline, candidate, false);
            SaveNew(side + "-depth-reject-parity.json", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" }, performanceEvidence = false,
            }, DepthRejectDriverOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void DepthRejectDriver_ThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            RequireDepthRejectDriver();
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] candidate = InvokeDepthRejectDriver(scenario, true, mode, 32, null, true);
            TickRow[] baseline = InvokeDepthRejectDriver(scenario, true, mode, 32, null, false);
            bool requireDepthProof = mode == ProductionEntityStressMode.Combat1000;
            CompareDepthRejectDriverRows(baseline, candidate, true, requireDepthProof);
            SaveNew(mode + "-depth-reject-parity.json",
                new
                {
                    baseline, candidate, requireDepthProof,
                    depthRejectCoverage = candidate.Any(row => row.depthRejects > 0)
                        ? "COVERED" : "NO_REJECT_COVERAGE",
                    performanceEvidence = false,
                }, DepthRejectDriverOutput);
        }

        private static MethodInfo RequireDepthRejectDriver()
        {
            MethodInfo method = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "RunDepthRejectDriver", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "the explicit depth reject Driver qualification entry is missing");
            return method;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DepthRejectDriver_CoverageContractKeepsParityGuards(bool requireProof)
        {
            MethodInfo compare = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "CompareDepthRejectDriverRows", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(compare, Is.Not.Null);
            Assert.That(compare.GetParameters().Length, Is.EqualTo(4),
                "branch coverage must be an explicit requirement, separate from every-tick parity");
            var baseline = new[] { new TickRow
            {
                tick = 1, entities = 1000, actualAi = 1000,
                extendedHashes = "same-comparer-fixture", lockstepHashes = "same-comparer-fixture",
                nativeScalarState = "same-comparer-fixture",
            } };
            var candidate = new[] { new TickRow
            {
                tick = 1, entities = 1000, actualAi = 1000, depthRejectApplied = true,
                extendedHashes = "same-comparer-fixture", lockstepHashes = "same-comparer-fixture",
                nativeScalarState = "same-comparer-fixture",
            } };
            Action invoke = () => compare.Invoke(null, new object[] { baseline, candidate, true, requireProof });
            if (requireProof)
                Assert.That(Assert.Throws<TargetInvocationException>(() => invoke()).InnerException,
                    Is.TypeOf<AssertionException>());
            else
                Assert.DoesNotThrow(() => invoke());
            candidate[0].depthRejects = 1;
            Assert.DoesNotThrow(() => invoke());
            candidate[0].extendedHashes = "changed-comparer-fixture";
            Assert.That(Assert.Throws<TargetInvocationException>(() => invoke()).InnerException,
                Is.TypeOf<AssertionException>());
            candidate[0].extendedHashes = baseline[0].extendedHashes;
            candidate[0].nativeScalarState = "changed-comparer-fixture";
            Assert.That(Assert.Throws<TargetInvocationException>(() => invoke()).InnerException,
                Is.TypeOf<AssertionException>());
            candidate[0].nativeScalarState = baseline[0].nativeScalarState;
            candidate[0].actualAi = 999;
            Assert.That(Assert.Throws<TargetInvocationException>(() => invoke()).InnerException,
                Is.TypeOf<AssertionException>());
        }

        private static TickRow[] InvokeDepthRejectDriver(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            return (TickRow[])RequireDepthRejectDriver().Invoke(null,
                new object[] { scenario, thousand, mode, ticks, native, enabled });
        }

        private static void CompareDepthRejectDriverRows(TickRow[] baseline, TickRow[] candidate, bool thousand,
            bool requireDepthProof = false)
        {
            CompareEveryTick(baseline, candidate);
            for (int i = 0; i < baseline.Length; i++)
            {
                JObject a = JObject.FromObject(baseline[i]), b = JObject.FromObject(candidate[i]);
                string label = "depth reject first-difference tick=" + (i + 1);
                Assert.That((string)a["nativeScalarState"], Is.Not.Null.And.Not.Empty, label);
                Assert.That((string)b["nativeScalarState"], Is.EqualTo((string)a["nativeScalarState"]), label);
                Assert.That((uint)b["legacyScalarState"], Is.EqualTo((uint)a["legacyScalarState"]), label);
                Assert.That((long)b["bruteExactDirections"], Is.EqualTo((long)a["bruteExactDirections"]), label);
                Assert.That((int)b["actualAi"], Is.EqualTo((int)a["actualAi"]), label);
                Assert.That((bool)a["depthRejectApplied"], Is.False, label);
                Assert.That((long)a["depthRejects"], Is.Zero, label);
                Assert.That((bool)b["depthRejectApplied"], Is.True, label);
                if (thousand)
                    Assert.That((int)b["actualAi"], Is.EqualTo(1000), label);
            }
            if (requireDepthProof)
                Assert.That(candidate.Any(row => (long)JObject.FromObject(row)["depthRejects"] > 0),
                    Is.True, "canonical AI must exercise an actual depth negative proof");
        }

        [Test]
        public void OrdinalPacketDriver_DefaultOffAndExplicitQualificationOnly()
        {
            RequireOrdinalPacketDriver();
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, Is.False);
            CoarseEnvelopeQualificationDoesNotPromoteAnyCandidateDefault();
            OrdinaryBruteDefaultsEnableAdmittedFastPath();
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void OrdinalPacketDriver_PreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            RequireOrdinalPacketDriver();
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] candidate = InvokeOrdinalPacketDriver(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, true);
            TickRow[] baseline = InvokeOrdinalPacketDriver(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, false);
            CompareOrdinalPacketDriverRows(baseline, candidate, false);
            SaveNew(side + "-ordinal-packet-parity.json", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" }, performanceEvidence = false,
            }, OrdinalPacketDriverOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void OrdinalPacketDriver_ThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            RequireOrdinalPacketDriver();
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] candidate = InvokeOrdinalPacketDriver(scenario, true, mode, 32, null, true);
            TickRow[] baseline = InvokeOrdinalPacketDriver(scenario, true, mode, 32, null, false);
            CompareOrdinalPacketDriverRows(baseline, candidate, true);
            SaveNew(mode + "-ordinal-packet-parity.json",
                new { baseline, candidate, performanceEvidence = false }, OrdinalPacketDriverOutput);
        }

        private static MethodInfo RequireOrdinalPacketDriver()
        {
            MethodInfo method = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "RunOrdinalPacketDriver", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "the explicit ordinal packet Driver qualification entry is missing");
            return method;
        }

        private static TickRow[] InvokeOrdinalPacketDriver(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            return (TickRow[])RequireOrdinalPacketDriver().Invoke(null,
                new object[] { scenario, thousand, mode, ticks, native, enabled });
        }

        private static void CompareOrdinalPacketDriverRows(TickRow[] baseline, TickRow[] candidate, bool thousand)
        {
            CompareEveryTick(baseline, candidate);
            for (int i = 0; i < baseline.Length; i++)
            {
                JObject a = JObject.FromObject(baseline[i]), b = JObject.FromObject(candidate[i]);
                string label = "ordinal packet first-difference tick=" + (i + 1);
                Assert.That((string)a["nativeScalarState"], Is.Not.Null.And.Not.Empty, label);
                Assert.That((string)b["nativeScalarState"], Is.EqualTo((string)a["nativeScalarState"]), label);
                Assert.That((uint)b["legacyScalarState"], Is.EqualTo((uint)a["legacyScalarState"]), label);
                Assert.That((long)b["bruteExactDirections"], Is.EqualTo((long)a["bruteExactDirections"]), label);
                Assert.That((long)b["bruteGeometryRejects"], Is.EqualTo((long)a["bruteGeometryRejects"]), label);
                Assert.That((int)b["actualAi"], Is.EqualTo((int)a["actualAi"]), label);
                Assert.That((bool)a["ordinalPacketApplied"], Is.False, label);
                Assert.That((long)a["ordinalPacketRejects"], Is.Zero, label);
                Assert.That((long)a["ordinalPacketProofs"], Is.Zero, label);
                Assert.That((bool)b["ordinalPacketApplied"], Is.True, label);
                Assert.That((bool)b["ordinalPacketFallback"], Is.False, label);
                if (thousand)
                    Assert.That((int)b["actualAi"], Is.EqualTo(1000), label);
            }
            if (thousand)
            {
                Assert.That(candidate.Any(row => (long)JObject.FromObject(row)["ordinalPacketRejects"] > 0),
                    Is.True, "canonical AI must exercise an actual packet negative proof");
                Assert.That(candidate.Any(row => (long)JObject.FromObject(row)["ordinalPacketProofs"] > 0), Is.True);
            }
        }

        [TestCase("far", 1)]
        [TestCase("edge", 1)]
        [TestCase("near", 0)]
        [TestCase("wide", 0)]
        [TestCase("negative", 1)]
        [TestCase("extreme", 1)]
        [TestCase("inverted", 0)]
        [TestCase("no-body", 1)]
        [TestCase("no-attack", 0)]
        [TestCase("mixed-block", 0)]
        public void OrdinalPacketProbe_ConservativeBothDirectionProof(string scenario, int rejectedPairs)
        {
            var rows = new JArray();
            for (int index = 0; index < (scenario == "mixed-block" ? 18 : 17); index++)
                rows.Add(new JObject { ["active"] = false });
            int firstX = scenario == "negative" ? -100 : scenario == "extreme" ? int.MaxValue - 4 : 0;
            int secondX = scenario == "negative" ? -200 : scenario == "extreme" ? int.MinValue :
                scenario == "edge" ? 4 : scenario == "near" || scenario == "inverted" ? 2 : 100;
            rows[0] = MakeOrdinalProbeRow(firstX, firstX + 4);
            rows[16] = MakeOrdinalProbeRow(secondX, secondX + 4);
            if (scenario == "wide")
                rows[16]["attack"] = new JArray(-1, 0, 104, 4);
            if (scenario == "inverted")
            {
                rows[16]["attack"] = new JArray(3, 0, 1, 4);
                rows[16]["body"] = new JArray(3, 0, 1, 4);
            }
            if (scenario == "no-body" || scenario == "no-attack")
                foreach (int index in new[] { 0, 16 })
                    rows[index][scenario == "no-body" ? "hasBody" : "hasItr"] = false;
            if (scenario == "mixed-block")
                rows[17] = MakeOrdinalProbeRow(2, 6);
            JObject result = JObject.FromObject(InvokeOrdinalProbe("AnalyzeOrdinalPacketGeometry", rows));
            Assert.That((long)result["packetRejectedPairs"], Is.EqualTo(rejectedPairs));
            Assert.That((long)result["falseRejectedDirections"], Is.Zero);
            Assert.That((int)result["packetWidth"], Is.EqualTo(16));
            if (scenario == "mixed-block")
                Assert.That((long)result["scalarRejectedDirections"], Is.GreaterThan(0),
                    "a wide packet must retain individually rejectable pairs when its union may overlap");
        }

        [Test]
        public void OrdinalPacketProbe_EmptyInputAndMissingCacheFailClosed()
        {
            JObject result = JObject.FromObject(InvokeOrdinalProbe("AnalyzeOrdinalPacketGeometry", new JArray()));
            Assert.That((long)result["directionChecks"], Is.Zero);
            TargetInvocationException nullFailure = Assert.Throws<TargetInvocationException>(() =>
                InvokeOrdinalProbe("AnalyzeOrdinalPacketGeometry", (object)null));
            Assert.That(nullFailure.InnerException, Is.TypeOf<ArgumentNullException>());
            MethodInfo capture = RequireOrdinalProbe("CaptureOrdinalPacketGeometry");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            TargetInvocationException failure = Assert.Throws<TargetInvocationException>(() =>
                capture.Invoke(null, new object[] { query }));
            Assert.That(failure.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void OrdinalPacketProbe_DoesNotPromoteAnyProductionDefault()
        {
            RequireOrdinalProbe("AnalyzeOrdinalPacketGeometry");
            CoarseEnvelopeQualificationDoesNotPromoteAnyCandidateDefault();
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void OrdinalPacketProbe_CanonicalThousandAiReadOnlyCoverage(ProductionEntityStressMode mode)
        {
            JObject result = JObject.FromObject(InvokeOrdinalProbe("RunOrdinalPacketCoverageProbe", mode));
            Assert.That((int)result["sampleCount"], Is.EqualTo(32));
            Assert.That((int)result["minimumActualAi"], Is.EqualTo(1000));
            Assert.That((long)result["falseRejectedDirections"], Is.Zero);
            Assert.That((int)result["observedMutationCount"], Is.Zero);
            Assert.That((bool)result["performanceEvidence"], Is.False);
        }

        private static JObject MakeOrdinalProbeRow(int x1, int x2)
        {
            return new JObject
            {
                ["active"] = true, ["hasItr"] = true, ["hasAttack"] = true, ["hasBody"] = true,
                ["baseAllowed"] = true, ["attackBaseAllowed"] = true,
                ["attack"] = new JArray(x1, 0, x2, 4), ["body"] = new JArray(x1, 0, x2, 4),
            };
        }

        private static MethodInfo RequireOrdinalProbe(string name)
        {
            MethodInfo method = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "the test-only ordinal packet qualification entry is missing: " + name);
            return method;
        }

        private static object InvokeOrdinalProbe(string name, params object[] arguments)
        {
            return RequireOrdinalProbe(name).Invoke(null, arguments);
        }

        private static object RunOrdinalPacketCoverageProbe(ProductionEntityStressMode mode)
        {
            const string output = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH69-BRUTE-ORDINAL-PACKET-FEASIBILITY-20261008/probe-01/";
            var samples = new List<OrdinalPacketCoverage>(32);
            int minimumAi = int.MaxValue;
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(RuntimeRoot,
                ScenarioRoot + "unity-scenario-right-x550.json", BattleRuntimeProfile.MobileExtended, 32,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    var query = (BruteForceSceneQuery)world.SceneQuery;
                    query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                    SetCandidate(query, true);
                    PrepareThousandAi(world, mode);
                    for (int tick = 1; tick <= 32; tick++)
                    {
                        Assert.That(driver.StepOneTick(inputs[tick - 1], true, false), Is.True);
                        Assert.That(world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics, Is.Zero);
                        int actualAi = Enumerable.Range(0, 1000).Select(world.FindEntityByRuntimeSlotForQuery)
                            .OfType<LF2Character>().Count(character => character.AiControlled);
                        minimumAi = Math.Min(minimumAi, actualAi);
                        Assert.That(actualAi, Is.EqualTo(1000));
                        string before = JsonConvert.SerializeObject(new
                        {
                            extended = world.CaptureExtendedChecksumSnapshot(tick, inputs[tick - 1]).Hashes,
                            lockstep = world.CaptureLockstepChecksumSnapshot(tick, inputs[tick - 1]).Hashes,
                            native = world.NativeRandom.CaptureScalarState(), legacy = world.Rng.CallCount,
                            query.LastBruteExactCacheDirectionCountForDiagnostics,
                            query.LastBruteGeometryFirstRejectCountForDiagnostics,
                        });
                        OrdinalPacketCoverage sample = AnalyzeOrdinalPacketGeometry(CaptureOrdinalPacketGeometry(query));
                        sample.tick = tick;
                        samples.Add(sample);
                        string after = JsonConvert.SerializeObject(new
                        {
                            extended = world.CaptureExtendedChecksumSnapshot(tick, inputs[tick - 1]).Hashes,
                            lockstep = world.CaptureLockstepChecksumSnapshot(tick, inputs[tick - 1]).Hashes,
                            native = world.NativeRandom.CaptureScalarState(), legacy = world.Rng.CallCount,
                            query.LastBruteExactCacheDirectionCountForDiagnostics,
                            query.LastBruteGeometryFirstRejectCountForDiagnostics,
                        });
                        Assert.That(after, Is.EqualTo(before), "read-only packet analysis tick=" + tick);
                        Assert.That(sample.falseRejectedDirections, Is.Zero);
                        Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                            query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics ||
                            query.EnableBruteKind5PresenceForDiagnostics || query.EnableBruteCoarseDispatchForDiagnostics ||
                            query.EnableBruteBranchTimingForDiagnostics, Is.False, "probe must not activate another candidate");
                    }
                }, useProjectMode: true);
            long directions = samples.Sum(sample => sample.directionChecks);
            long rejected = samples.Sum(sample => sample.packetRejectedDirections);
            long idealTests = directions - rejected + 2 * samples.Sum(sample => sample.packetChecks);
            double rejectedFraction = directions == 0 ? 0 : (double)rejected / directions;
            double idealReduction = directions == 0 ? 0 : 1.0 - (double)idealTests / directions;
            var result = new
            {
                mode = mode.ToString(), sampleCount = samples.Count, minimumActualAi = minimumAi,
                directionChecks = directions, packetRejectedDirections = rejected, packetRejectedFraction = rejectedFraction,
                idealEnvelopeTests = idealTests, idealEnvelopeTestReductionFraction = idealReduction,
                falseRejectedDirections = samples.Sum(sample => sample.falseRejectedDirections), observedMutationCount = 0,
                baseGatedRejectedDirections = samples.Sum(sample => sample.baseGatedRejectedDirections),
                thresholdQualified = rejectedFraction >= 0.25 && idealReduction >= 0.20,
                performanceEvidence = false, bindingCleanupRemoved = false, packetWidth = 16, samples,
            };
            SaveNew(mode + "-packet-coverage.json", result, output);
            return result;
        }

        private static JArray CaptureOrdinalPacketGeometry(BruteForceSceneQuery query)
        {
            if (!query.LastBruteExactCacheAppliedForDiagnostics || query.LastBruteExactCacheFallbackForDiagnostics)
                throw new InvalidOperationException("packet analysis requires this collection's complete exact cache");
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            object buffer = typeof(BruteForceSceneQuery).GetField("_roleFormalParticipants", flags).GetValue(query);
            Array items = (Array)buffer.GetType().GetField("items", flags).GetValue(buffer);
            int count = (int)buffer.GetType().GetProperty("Count", flags).GetValue(buffer);
            var itrs = (System.Collections.IList)typeof(BruteForceSceneQuery)
                .GetField("_roleFormalExactItrRects", flags).GetValue(query);
            var rows = new JArray();
            for (int ordinal = 0; ordinal < count; ordinal++)
            {
                object participant = items.GetValue(ordinal);
                Type type = participant.GetType();
                object Read(string name) => type.GetProperty(name, flags).GetValue(participant);
                if (Read("Entity") == null)
                {
                    rows.Add(new JObject { ["active"] = false });
                    continue;
                }
                Assert.That((bool)Read("HasExactCommonCache") && (bool)Read("HasExactAttackCache") &&
                    (bool)Read("HasExactBodyCache"), Is.True, "complete participant ordinal=" + ordinal);
                bool hasAttack = (bool)Read("HasOrdinaryItrUnion");
                OrdinalProbeRect attack = hasAttack ? ReadOrdinalProbeRect(Read("OrdinaryItrUnionWorld")) : default;
                int start = (int)Read("ExactItrRectOffset"), length = (int)Read("ExactItrRectCount");
                Assert.That(start >= 0 && length >= 0 && start <= itrs.Count && length <= itrs.Count - start, Is.True);
                for (int index = start; index < start + length; index++)
                {
                    object entry = itrs[index];
                    Type entryType = entry.GetType();
                    var itr = (InteractionArea)entryType.GetProperty("Itr", flags).GetValue(entry);
                    if (itr.kind != 5)
                        continue;
                    OrdinalProbeRect rect = ReadOrdinalProbeRect(entryType.GetProperty("WorldRect", flags).GetValue(entry));
                    attack = hasAttack ? UnionOrdinalProbeRect(attack, rect) : rect;
                    hasAttack = true;
                }
                rows.Add(new JObject
                {
                    ["active"] = true, ["hasItr"] = (int)Read("CollisionItrCount") > 0, ["hasAttack"] = hasAttack,
                    ["hasBody"] = (bool)Read("HasCollisionReleaseBody") && (bool)Read("HasBodyUnion"),
                    ["baseAllowed"] = (bool)Read("PairCollectionBaseAllowed"),
                    ["attackBaseAllowed"] = (bool)Read("AttackPairCollectionBaseAllowed"),
                    ["attack"] = new JArray(attack.x1, attack.y1, attack.x2, attack.y2),
                    ["body"] = JArray.FromObject(ReadOrdinalProbeRect(Read("BodyUnionWorld")).Coordinates),
                });
            }
            return rows;
        }

        private static OrdinalProbeRect ReadOrdinalProbeRect(object value)
        {
            Type type = value.GetType();
            return new OrdinalProbeRect((int)type.GetField("X1").GetValue(value),
                (int)type.GetField("Y1").GetValue(value), (int)type.GetField("X2").GetValue(value),
                (int)type.GetField("Y2").GetValue(value));
        }

        private static OrdinalPacketCoverage AnalyzeOrdinalPacketGeometry(JArray input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            const int width = 16;
            var rows = new OrdinalProbeGeometry[input.Count];
            var packets = new OrdinalProbeGeometry[(input.Count + width - 1) / width];
            for (int index = 0; index < input.Count; index++)
            {
                if (!(bool)input[index]["active"])
                    continue;
                var row = new OrdinalProbeGeometry
                {
                    active = true, hasItr = (bool)input[index]["hasItr"], hasAttack = (bool)input[index]["hasAttack"],
                    hasBody = (bool)input[index]["hasBody"], baseAllowed = (bool)input[index]["baseAllowed"],
                    attackBaseAllowed = (bool)input[index]["attackBaseAllowed"],
                    attack = OrdinalProbeRect.FromArray((JArray)input[index]["attack"]),
                    body = OrdinalProbeRect.FromArray((JArray)input[index]["body"]),
                };
                rows[index] = row;
                ref OrdinalProbeGeometry packet = ref packets[index / width];
                if (row.hasItr && row.hasAttack)
                {
                    packet.attack = packet.hasAttack ? UnionOrdinalProbeRect(packet.attack, row.attack) : row.attack;
                    packet.hasAttack = true;
                }
                if (row.hasBody)
                {
                    packet.body = packet.hasBody ? UnionOrdinalProbeRect(packet.body, row.body) : row.body;
                    packet.hasBody = true;
                }
                packet.hasItr |= row.hasItr;
            }
            var result = new OrdinalPacketCoverage { participantCount = rows.Length, packetWidth = width };
            for (int i = 0; i < rows.Length; i++)
            {
                OrdinalProbeGeometry first = rows[i];
                if (!first.active)
                    continue;
                for (int blockStart = i + 1; blockStart < rows.Length;)
                {
                    int blockEnd = Math.Min(rows.Length, (blockStart / width + 1) * width);
                    OrdinalProbeGeometry packet = packets[blockStart / width];
                    bool reject = false;
                    if (blockStart % width == 0 && (first.hasItr || packet.hasItr))
                    {
                        result.packetChecks++;
                        reject = !MayOrdinalProbeOverlap(first, packet) && !MayOrdinalProbeOverlap(packet, first);
                    }
                    for (int j = blockStart; j < blockEnd; j++)
                    {
                        OrdinalProbeGeometry second = rows[j];
                        if (!second.active || !first.hasItr && !second.hasItr)
                            continue;
                        result.logicalPairs++;
                        bool forward = MayOrdinalProbeOverlap(first, second), reverse = MayOrdinalProbeOverlap(second, first);
                        int directions = (first.hasItr ? 1 : 0) + (second.hasItr ? 1 : 0);
                        result.directionChecks += directions;
                        result.scalarRejectedDirections += (first.hasItr && !forward ? 1 : 0) +
                            (second.hasItr && !reverse ? 1 : 0);
                        if (!reject)
                            continue;
                        result.packetRejectedPairs++;
                        result.packetRejectedDirections += directions;
                        result.falseRejectedDirections += (forward ? 1 : 0) + (reverse ? 1 : 0);
                        result.baseGatedRejectedDirections +=
                            (first.hasItr && first.attackBaseAllowed && second.baseAllowed ? 1 : 0) +
                            (second.hasItr && second.attackBaseAllowed && first.baseAllowed ? 1 : 0);
                    }
                    blockStart = blockEnd;
                }
            }
            return result;
        }

        private static bool MayOrdinalProbeOverlap(OrdinalProbeGeometry attacker, OrdinalProbeGeometry target)
        {
            OrdinalProbeRect a = attacker.attack, b = target.body;
            return attacker.hasItr && attacker.hasAttack && target.hasBody &&
                a.x1 < b.x2 && a.x2 > b.x1 && a.y1 < b.y2 && a.y2 > b.y1;
        }

        private static OrdinalProbeRect UnionOrdinalProbeRect(OrdinalProbeRect a, OrdinalProbeRect b)
        {
            return new OrdinalProbeRect(Math.Min(a.x1, b.x1), Math.Min(a.y1, b.y1),
                Math.Max(a.x2, b.x2), Math.Max(a.y2, b.y2));
        }

        private readonly struct OrdinalProbeRect
        {
            public readonly int x1, y1, x2, y2;
            public OrdinalProbeRect(int x1, int y1, int x2, int y2)
            {
                this.x1 = x1; this.y1 = y1; this.x2 = x2; this.y2 = y2;
            }
            public int[] Coordinates => new[] { x1, y1, x2, y2 };
            public static OrdinalProbeRect FromArray(JArray value)
            {
                if (value == null || value.Count != 4)
                    throw new InvalidOperationException("packet geometry requires four exact integer coordinates");
                return new OrdinalProbeRect((int)value[0], (int)value[1], (int)value[2], (int)value[3]);
            }
        }

        private struct OrdinalProbeGeometry
        {
            public bool active, hasItr, hasAttack, hasBody, baseAllowed, attackBaseAllowed;
            public OrdinalProbeRect attack, body;
        }

        private sealed class OrdinalPacketCoverage
        {
            public int tick, participantCount, packetWidth;
            public long logicalPairs, directionChecks, scalarRejectedDirections, packetChecks;
            public long packetRejectedPairs, packetRejectedDirections, falseRejectedDirections, baseGatedRejectedDirections;
        }

        [Test]
        public void EnvelopeBindingEligibilityQualificationDoesNotPromoteDefaults()
        {
            Assert.That(typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "RunEnvelopeBindingEligibility", BindingFlags.Static | BindingFlags.NonPublic), Is.Not.Null,
                "the explicit three-mechanism Driver qualification entry is missing");
            ParameterInfo composition = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "Run", BindingFlags.Static | BindingFlags.NonPublic).GetParameters().Last();
            Assert.That(composition.Name, Is.EqualTo("composeEnvelopeBindingEligibility"));
            Assert.That(composition.DefaultValue, Is.EqualTo(false));
            CoarseEnvelopeQualificationDoesNotPromoteAnyCandidateDefault();
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void EnvelopeBindingEligibilityPreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] candidate = InvokeEnvelopeBindingEligibilityRun(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, true);
            TickRow[] baseline = InvokeEnvelopeBindingEligibilityRun(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, false);
            CompareEveryTick(baseline, candidate);
            SaveNew(side + "-envelope-binding-eligibility-parity", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" },
                baselineFlags = "envelope=true,rejectedBinding=true,eligibility=false",
                candidateFlags = "envelope=true,rejectedBinding=true,eligibility=true",
                performanceEvidence = false,
            }, EnvelopeBindingEligibilityOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void EnvelopeBindingEligibilityThousandCanonicalAiPreservesEveryTick(
            ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] candidate = InvokeEnvelopeBindingEligibilityRun(scenario, true, mode, 32, null, true);
            TickRow[] baseline = InvokeEnvelopeBindingEligibilityRun(scenario, true, mode, 32, null, false);
            CompareEveryTick(baseline, candidate);
            SaveNew(mode + "-envelope-binding-eligibility-parity", new
            {
                baseline, candidate,
                baselineFlags = "envelope=true,rejectedBinding=true,eligibility=false",
                candidateFlags = "envelope=true,rejectedBinding=true,eligibility=true",
                performanceEvidence = false,
            }, EnvelopeBindingEligibilityOutput);
        }

        private static TickRow[] InvokeEnvelopeBindingEligibilityRun(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            MethodInfo run = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "RunEnvelopeBindingEligibility", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(run, Is.Not.Null, "the explicit three-mechanism Driver qualification entry is missing");
            return (TickRow[])run.Invoke(null, new object[]
                { scenario, thousand, mode, ticks, native, enabled });
        }

        [Test]
        public void EnvelopeBindingQualificationDoesNotPromoteDefaults()
        {
            Assert.That(typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "RunEnvelopeBinding", BindingFlags.Static | BindingFlags.NonPublic), Is.Not.Null,
                "the composed Driver qualification entry is missing");
            CoarseEnvelopeQualificationDoesNotPromoteAnyCandidateDefault();
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void EnvelopeBindingPreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] candidate = InvokeEnvelopeBindingRun(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, true);
            TickRow[] baseline = InvokeEnvelopeBindingRun(scenario, false,
                ProductionEntityStressMode.Combat1000, 12, native, false);
            CompareEveryTick(baseline, candidate);
            SaveNew(side + "-envelope-binding-parity", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" },
                baselineFlags = "envelope=true,rejectedBinding=false",
                candidateFlags = "envelope=true,rejectedBinding=true",
                performanceEvidence = false,
            }, EnvelopeBindingOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void EnvelopeBindingThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] candidate = InvokeEnvelopeBindingRun(scenario, true, mode, 32, null, true);
            TickRow[] baseline = InvokeEnvelopeBindingRun(scenario, true, mode, 32, null, false);
            CompareEveryTick(baseline, candidate);
            SaveNew(mode + "-envelope-binding-parity", new
            {
                baseline, candidate,
                baselineFlags = "envelope=true,rejectedBinding=false",
                candidateFlags = "envelope=true,rejectedBinding=true",
                performanceEvidence = false,
            }, EnvelopeBindingOutput);
        }

        private static TickRow[] InvokeEnvelopeBindingRun(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            MethodInfo run = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "RunEnvelopeBinding", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(run, Is.Not.Null, "the composed Driver qualification entry is missing");
            return (TickRow[])run.Invoke(null, new object[]
                { scenario, thousand, mode, ticks, native, enabled });
        }

        [TestCase(0L, 0L, "COUNTER_UNAVAILABLE", true)]
        [TestCase(1L, 4128L, "COUNTER_UNAVAILABLE", true)]
        [TestCase(0L, 4128L, "CALIBRATED_MEASUREMENT_REQUIRED", false)]
        public void CoarseEnvelopeBudgetCounterGateFailsClosed(
            long emptyBytes, long positiveBytes, string expectedStatus, bool expectedUnavailable)
        {
            JObject result = JObject.FromObject(ClassifyCoarseEnvelopeArrayCounter(emptyBytes, positiveBytes));
            Assert.That(result["status"].Value<string>(), Is.EqualTo(expectedStatus));
            Assert.That(result["counterUnavailable"].Value<bool>(), Is.EqualTo(expectedUnavailable));
            Assert.That(result["byteBudgetCertified"].Value<bool>(), Is.False);
            Assert.That(result["strideBytes"].Type, Is.EqualTo(JTokenType.Null));
            Assert.That(result["steadyBytes"].Type, Is.EqualTo(JTokenType.Null));
            Assert.That(result["transitionBytes"].Type, Is.EqualTo(JTokenType.Null));
        }

        [Test]
        public void CoarseEnvelopeBudgetTracksPreparedOwnerAndTransition()
        {
            JObject result = InvokeCoarseEnvelopeBudgetProbe("InspectCoarseEnvelopeBufferOwner");
            Assert.That(result["participantBufferOwnerCount"].Value<int>(), Is.EqualTo(1));
            Assert.That(result["participantArrayFieldCount"].Value<int>(), Is.EqualTo(1));
            Assert.That(result["initialCapacity"].Value<int>(), Is.EqualTo(128));
            Assert.That(result["preparedCapacity"].Value<int>(), Is.EqualTo(1050));
            Assert.That(result["countAfterFullBuild"].Value<int>(), Is.EqualTo(1000));
            Assert.That(result["countAfterShrunkBuild"].Value<int>(), Is.EqualTo(900));
            Assert.That(result["arrayReusedAfterPrepareToggleBuild"].Value<bool>(), Is.True);
            Assert.That(result["oldNewTransitionRecordCapacity"].Value<int>(), Is.EqualTo(1178));
            Assert.That(result["includesEntireWorld"].Value<bool>(), Is.False);
            SaveNew("owner-budget", result,
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH61-COARSE-ENVELOPE-BUDGET-20261008/budget-01/");
        }


        private static object ClassifyCoarseEnvelopeArrayCounter(long emptyBytes, long positiveBytes)
        {
            bool unavailable = emptyBytes != 0 || positiveBytes < 4096;
            return new
            {
                status = unavailable ? "COUNTER_UNAVAILABLE" : "CALIBRATED_MEASUREMENT_REQUIRED",
                counterUnavailable = unavailable,
                byteBudgetCertified = false,
                strideBytes = (long?)null,
                steadyBytes = (long?)null,
                transitionBytes = (long?)null
            };
        }

        private static object InspectCoarseEnvelopeBufferOwner()
        {
            const BindingFlags instanceFields = BindingFlags.Instance | BindingFlags.NonPublic;
            var world = new SimulationWorld(BattleRuntimeProfile.MobileExtended, 1050);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            Type participantType = typeof(BruteForceSceneQuery).Assembly.GetType(
                "NTSD.Animation.RoleAwareFormalParticipant", true);
            Type bufferType = typeof(BruteForceSceneQuery).Assembly.GetType(
                "NTSD.Animation.RoleAwareFormalParticipantBuffer", true);
            FieldInfo[] owners = typeof(BruteForceSceneQuery).GetFields(instanceFields)
                .Where(field => field.FieldType == bufferType).ToArray();
            FieldInfo[] arrays = bufferType.GetFields(instanceFields)
                .Where(field => field.FieldType == participantType.MakeArrayType()).ToArray();
            Assert.That(owners.Length, Is.EqualTo(1));
            Assert.That(arrays.Length, Is.EqualTo(1));
            object owner = owners[0].GetValue(query);
            Array initial = (Array)arrays[0].GetValue(owner);
            MethodInfo ensure = bufferType.GetMethod("EnsureCapacity", instanceFields);
            MethodInfo begin = bufferType.GetMethod("BeginBuild", instanceFields);
            MethodInfo add = bufferType.GetMethod("Add", instanceFields);
            MethodInfo complete = bufferType.GetMethod("CompleteBuild", instanceFields);
            PropertyInfo count = bufferType.GetProperty("Count", instanceFields);
            ensure.Invoke(owner, new object[] { 1050 });
            Array prepared = (Array)arrays[0].GetValue(owner);
            bool reused = true;
            bool original = query.EnableBruteCoarseEnvelopeForDiagnostics;
            int fullCount;
            int shrunkCount;
            try
            {
                query.EnableBruteCoarseEnvelopeForDiagnostics = true;
                reused &= ReferenceEquals(prepared, arrays[0].GetValue(owner));
                query.EnableBruteCoarseEnvelopeForDiagnostics = false;
                ensure.Invoke(owner, new object[] { 1050 });
                reused &= ReferenceEquals(prepared, arrays[0].GetValue(owner));
                object[] argument = { Activator.CreateInstance(participantType) };
                begin.Invoke(owner, null);
                for (int index = 0; index < 1000; index++)
                    add.Invoke(owner, argument);
                complete.Invoke(owner, null);
                fullCount = (int)count.GetValue(owner);
                reused &= ReferenceEquals(prepared, arrays[0].GetValue(owner));
                begin.Invoke(owner, null);
                for (int index = 0; index < 900; index++)
                    add.Invoke(owner, argument);
                complete.Invoke(owner, null);
                shrunkCount = (int)count.GetValue(owner);
                reused &= ReferenceEquals(prepared, arrays[0].GetValue(owner));
            }
            finally
            {
                query.EnableBruteCoarseEnvelopeForDiagnostics = original;
            }
            GC.KeepAlive(initial);
            return new
            {
                schema = "ntsd/coarse-envelope-prepared-owner-budget@1",
                scope = "cold actual owner path, not live Battle slot readback or a new 0GC measurement",
                participantBufferOwnerCount = owners.Length,
                participantArrayFieldCount = arrays.Length,
                initialCapacity = initial.Length,
                preparedCapacity = prepared.Length,
                countAfterFullBuild = fullCount,
                countAfterShrunkBuild = shrunkCount,
                arrayReusedAfterPrepareToggleBuild = reused,
                oldNewTransitionRecordCapacity = initial.Length + prepared.Length,
                transitionModel = "resize old/new arrays may coexist until old array is no longer retained and collected",
                instanceGpuLeaseStagingOwnedByThisBuffer = false,
                includesEntireWorld = false,
                productionCandidateDefault = original
            };
        }

        private static JObject InvokeCoarseEnvelopeBudgetProbe(string name)
        {
            MethodInfo probe = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(probe, Is.Not.Null, "cold budget probe is missing");
            return JObject.FromObject(probe.Invoke(null, null));
        }

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

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void EligibilityReusePreservesApplicableFormalRootAndEveryTick(
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
                12, native, eligibilityReuse: false);
            TickRow[] candidate = Run(scenario, true, false, ProductionEntityStressMode.Combat1000,
                12, native, eligibilityReuse: true);
            CompareEveryTick(baseline, candidate);
            Assert.That(baseline.All(row => !row.eligibilityReuseApplied), Is.True);
            Assert.That(candidate.All(row => row.eligibilityReuseApplied), Is.True,
                "every candidate tick must actually exercise cached eligibility");
            SaveNew(side + "-eligibility-parity.json", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" },
                performanceEvidence = false,
            }, EligibilityOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void EligibilityReuseThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] baseline = Run(scenario, true, true, mode, 32, null, eligibilityReuse: false);
            TickRow[] candidate = Run(scenario, true, true, mode, 32, null, eligibilityReuse: true);
            CompareEveryTick(baseline, candidate);
            Assert.That(baseline.All(row => !row.eligibilityReuseApplied), Is.True);
            Assert.That(candidate.All(row => row.eligibilityReuseApplied), Is.True,
                "every canonical AI candidate tick must exercise cached eligibility");
            SaveNew(mode + "-eligibility-parity.json",
                new { baseline, candidate, performanceEvidence = false }, EligibilityOutput);
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void CombinedCachePreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] candidate = Run(scenario, true, false, ProductionEntityStressMode.Combat1000,
                12, native, kind5Presence: true, eligibilityReuse: true);
            TickRow[] baseline = Run(scenario, true, false, ProductionEntityStressMode.Combat1000,
                12, native, kind5Presence: false, eligibilityReuse: true);
            CompareEveryTick(baseline, candidate);
            Assert.That(baseline.All(row => row.eligibilityReuseApplied && !row.kind5PresenceApplied), Is.True);
            Assert.That(candidate.All(row => row.eligibilityReuseApplied && row.kind5PresenceApplied), Is.True);
            SaveNew(side + "-combined-parity.json", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" },
                baselineFlags = "eligibility=true,kind5=false",
                candidateFlags = "eligibility=true,kind5=true",
                performanceEvidence = false,
            }, CombinedOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void CombinedCacheThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] candidate = Run(scenario, true, true, mode, 32, null,
                kind5Presence: true, eligibilityReuse: true);
            TickRow[] baseline = Run(scenario, true, true, mode, 32, null,
                kind5Presence: false, eligibilityReuse: true);
            CompareEveryTick(baseline, candidate);
            Assert.That(baseline.All(row => row.eligibilityReuseApplied && !row.kind5PresenceApplied), Is.True);
            Assert.That(candidate.All(row => row.eligibilityReuseApplied && row.kind5PresenceApplied), Is.True);
            Assert.That(candidate.Any(row => row.kind5ScansSkipped > 0), Is.True,
                "the composed candidate must skip an actual kind5 scan");
            SaveNew(mode + "-combined-parity.json", new
            {
                baseline, candidate,
                baselineFlags = "eligibility=true,kind5=false",
                candidateFlags = "eligibility=true,kind5=true",
                performanceEvidence = false,
            }, CombinedOutput);
        }

        [Test]
        public void CoarseEnvelopeQualificationDoesNotPromoteAnyCandidateDefault()
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, Is.False);
            Assert.That(query.EnableBruteKind5PresenceForDiagnostics, Is.False);
            Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, Is.False);
            Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, Is.False);
            Assert.That(query.EnableBruteCoarseDispatchForDiagnostics, Is.False);
            Assert.That(query.EnableBruteBranchTimingForDiagnostics, Is.False);
        }

        [TestCase("right-x550", "right-x550-root-v1-trace.jsonl",
            "AA327E22F85000294699EA689795E90B02EFF38194954294CE41024D1D415FBE")]
        [TestCase("left-x350", "left-x350-root-v2-trace.jsonl",
            "CF2701700E543AB7C37B132D1D1A0337DB1814C6440846FC8F16C6EAAD5BB9B2")]
        public void CoarseEnvelopePreservesApplicableFormalRootAndEveryTick(
            string side, string trace, string expectedSha)
        {
            Assert.That(HashFile(Path.GetFullPath(Path.Combine(RuntimeRoot, "../../NTSD2.8-Logan.exe"))),
                Is.EqualTo(FormalSha));
            Assert.That(HashFile(RootTrace + trace), Is.EqualTo(expectedSha));
            JObject[] native = File.ReadLines(RootTrace + trace).Select(JObject.Parse)
                .Where(row => (int)row["tick"] >= 1 && (int)row["tick"] <= 12).ToArray();
            Assert.That(native.Length, Is.EqualTo(12));
            string scenario = ScenarioRoot + "unity-scenario-" + side + ".json";
            TickRow[] candidate = RunCoarseEnvelope(scenario, false, ProductionEntityStressMode.Combat1000,
                12, native, true);
            TickRow[] baseline = RunCoarseEnvelope(scenario, false, ProductionEntityStressMode.Combat1000,
                12, native, false);
            CompareEveryTick(baseline, candidate);
            AssertCoarseEnvelopeApplication(baseline, candidate);
            SaveNew(side + "-envelope-parity.json", new
            {
                baseline, candidate, formalRootSha = FormalSha, traceSha = expectedSha,
                rootFields = new[] { "oid", "action", "hp", "vx" },
                performanceEvidence = false,
            }, CoarseEnvelopeOutput);
        }

        [TestCase(ProductionEntityStressMode.Dispersed1000)]
        [TestCase(ProductionEntityStressMode.Combat1000)]
        public void CoarseEnvelopeThousandCanonicalAiPreservesEveryTick(ProductionEntityStressMode mode)
        {
            string scenario = ScenarioRoot + "unity-scenario-right-x550.json";
            TickRow[] candidate = RunCoarseEnvelope(scenario, true, mode, 32, null, true);
            TickRow[] baseline = RunCoarseEnvelope(scenario, true, mode, 32, null, false);
            CompareEveryTick(baseline, candidate);
            AssertCoarseEnvelopeApplication(baseline, candidate);
            Assert.That(candidate.Any(row => row.coarseEnvelopeDirections > 0), Is.True,
                "the envelope must be exercised by the canonical AI workload");
            Assert.That(candidate.Any(row => row.coarseEnvelopeRejects > 0), Is.True,
                "the envelope must reject an actual direction before qualification can pass");
            SaveNew(mode + "-envelope-parity.json",
                new { baseline, candidate, performanceEvidence = false }, CoarseEnvelopeOutput);
        }

        private static TickRow[] RunCoarseEnvelope(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            MethodInfo run = typeof(BattleBruteProductionAdmissionEditorTests).GetMethod(
                "Run", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(run, Is.Not.Null);
            Assert.That(run.GetParameters().Any(parameter => parameter.Name == "coarseEnvelope"), Is.True,
                "the full Driver fixture must explicitly integrate the envelope candidate");
            return (TickRow[])run.Invoke(null, new object[]
                { scenario, true, thousand, mode, ticks, native, null, null, enabled, null, false });
        }

        private static void AssertCoarseEnvelopeApplication(TickRow[] baseline, TickRow[] candidate)
        {
            Assert.That(baseline.All(row => !row.coarseEnvelopeEnabled &&
                row.coarseEnvelopeDirections == 0 && row.coarseEnvelopeRejects == 0), Is.True);
            Assert.That(candidate.All(row => row.coarseEnvelopeEnabled), Is.True);
        }

        private static TickRow[] RunEnvelopeBinding(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            TickRow[] rows = Run(scenario, true, thousand, mode, ticks, native,
                coarseEnvelope: true, rejectedBindingReuse: enabled);
            AssertEnvelopeBindingApplication(rows, enabled, thousand);
            return rows;
        }

        private static void AssertEnvelopeBindingApplication(TickRow[] rows, bool enabled, bool thousand)
        {
            Assert.That(rows.All(row => row.coarseEnvelopeEnabled &&
                row.cacheApplied && !row.fallback && !row.kind5PresenceApplied &&
                !row.eligibilityReuseApplied && row.bindingReuseApplied == enabled), Is.True,
                "each tick must retain the envelope baseline and apply only the requested binding reuse");
            if (!enabled)
            {
                Assert.That(rows.All(row => row.bindingFirstProbes == 0 && row.bindingReuses == 0), Is.True,
                    "OFF must not use the binding reuse counters");
            }
            if (thousand)
            {
                Assert.That(rows.Any(row => row.coarseEnvelopeDirections > 0), Is.True);
                Assert.That(rows.Any(row => row.coarseEnvelopeRejects > 0), Is.True,
                    "the canonical AI fixture must exercise a real envelope rejection");
                if (enabled)
                {
                    Assert.That(rows.Any(row => row.bindingFirstProbes > 0), Is.True,
                        "the composed candidate must execute the original first binding probe");
                    Assert.That(rows.Any(row => row.bindingReuses > 0), Is.True,
                        "the composed candidate must reuse an actual rejected-binding probe");
                }
            }
        }

        private static TickRow[] RunEnvelopeBindingEligibility(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            TickRow[] rows = Run(scenario, true, thousand, mode, ticks, native,
                eligibilityReuse: enabled, coarseEnvelope: true, rejectedBindingReuse: true,
                composeEnvelopeBindingEligibility: true);
            Assert.That(rows.All(row => row.coarseEnvelopeEnabled && row.bindingReuseApplied &&
                row.eligibilityReuseApplied == enabled && row.cacheApplied && !row.fallback &&
                !row.kind5PresenceApplied), Is.True,
                "every tick must apply only the explicit eligibility increment on the envelope-binding baseline");
            if (thousand)
            {
                Assert.That(rows.Any(row => row.coarseEnvelopeDirections > 0 && row.coarseEnvelopeRejects > 0),
                    Is.True, "canonical AI must exercise envelope rejection");
                Assert.That(rows.Any(row => row.bindingFirstProbes > 0 && row.bindingReuses > 0),
                    Is.True, "both paths must execute the first binding probe and actual reuse");
            }
            return rows;
        }

        private static TickRow[] RunOrdinalPacketDriver(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            TickRow[] rows = Run(scenario, true, thousand, mode, ticks, native, ordinalPacket: enabled);
            Assert.That(rows.All(row => row.ordinalPacketApplied == enabled && !row.ordinalPacketFallback &&
                row.cacheApplied && !row.fallback && !row.kind5PresenceApplied &&
                !row.eligibilityReuseApplied && !row.coarseEnvelopeEnabled && !row.bindingReuseApplied), Is.True,
                "only the explicitly requested ordinal packet mode may change on the admitted baseline");
            return rows;
        }

        private static TickRow[] RunDepthRejectDriver(string scenario, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native, bool enabled)
        {
            TickRow[] rows = Run(scenario, true, thousand, mode, ticks, native, depthReject: enabled);
            Assert.That(rows.All(row => row.depthRejectApplied == enabled && row.cacheApplied &&
                !row.fallback && !row.ordinalPacketApplied && !row.kind5PresenceApplied &&
                !row.eligibilityReuseApplied && !row.coarseEnvelopeEnabled && !row.bindingReuseApplied), Is.True,
                "only the explicitly requested depth mode may change on the admitted baseline");
            return rows;
        }

        private static object[] ReadDepthRejectPreparedArrays(BruteForceSceneQuery query)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var arrays = new object[3];
            object owner = typeof(BruteForceSceneQuery).GetField("_roleFormalParticipants", flags).GetValue(query);
            arrays[0] = owner.GetType().GetField("items", flags).GetValue(owner);
            for (int i = 1; i < arrays.Length; i++)
            {
                object list = typeof(BruteForceSceneQuery).GetField(
                    i == 1 ? "_roleFormalExactItrRects" : "_roleFormalExactBodyRects", flags).GetValue(query);
                arrays[i] = list.GetType().GetField("_items", flags).GetValue(list);
            }
            return arrays;
        }

        private static TickRow[] Run(string scenario, bool enabled, bool thousand,
            ProductionEntityStressMode mode, int ticks, JObject[] native,
            bool? kind5Presence = null, bool? eligibilityReuse = null, bool? coarseEnvelope = null,
            bool? rejectedBindingReuse = null, bool? ordinalPacket = null, bool? depthReject = null,
            bool composeEnvelopeBindingEligibility = false)
        {
            Assert.That(!composeEnvelopeBindingEligibility || enabled && !kind5Presence.HasValue &&
                eligibilityReuse.HasValue && coarseEnvelope == true && rejectedBindingReuse == true, Is.True,
                "three-mechanism composition requires its explicit mode and exact baseline flags");
            var result = new List<TickRow>(ticks);
            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                RuntimeRoot, scenario, thousand ? BattleRuntimeProfile.MobileExtended : BattleRuntimeProfile.Authority400,
                ticks, (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    var query = (BruteForceSceneQuery)world.SceneQuery;
                    query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                    SetCandidate(query, enabled);
                    if (composeEnvelopeBindingEligibility)
                    {
                        Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                            query.EnableBruteRejectedBindingReuseForDiagnostics ||
                            query.EnableBruteEligibilityReuseForDiagnostics ||
                            query.EnableBruteKind5PresenceForDiagnostics ||
                            query.EnableBruteCoarseDispatchForDiagnostics ||
                            query.EnableBruteBranchTimingForDiagnostics, Is.False,
                            "explicit composition must not promote or inherit candidate defaults");
                        query.EnableBruteCoarseEnvelopeForDiagnostics = true;
                        query.EnableBruteRejectedBindingReuseForDiagnostics = true;
                        query.EnableBruteEligibilityReuseForDiagnostics = eligibilityReuse.Value;
                    }
                    if (!composeEnvelopeBindingEligibility && kind5Presence.HasValue)
                    {
                        Assert.That(enabled, Is.True, "both paths must retain the admitted production mechanisms");
                        Assert.That(query.EnableBruteKind5PresenceForDiagnostics, Is.False,
                            "qualification must not promote the production default");
                        Assert.That(query.EnableBruteBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteKind5PresenceForDiagnostics = kind5Presence.Value;
                    }
                    if (!composeEnvelopeBindingEligibility && eligibilityReuse.HasValue)
                    {
                        Assert.That(enabled, Is.True, "both paths must retain the admitted production mechanisms");
                        Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, Is.False,
                            "qualification must not promote the production default");
                        Assert.That(query.EnableBruteKind5PresenceForDiagnostics,
                            Is.EqualTo(kind5Presence ?? false),
                            "only an explicitly requested kind5 cache may be composed with eligibility reuse");
                        Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteEligibilityReuseForDiagnostics = eligibilityReuse.Value;
                    }
                    if (!composeEnvelopeBindingEligibility && coarseEnvelope.HasValue)
                    {
                        Assert.That(enabled, Is.True, "both paths must retain the admitted production mechanisms");
                        Assert.That(kind5Presence.HasValue || eligibilityReuse.HasValue, Is.False,
                            "envelope qualification must not compose other unadmitted candidates");
                        Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, Is.False,
                            "qualification must not promote the production default");
                        Assert.That(query.EnableBruteKind5PresenceForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteCoarseDispatchForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteCoarseEnvelopeForDiagnostics = coarseEnvelope.Value;
                    }
                    if (!composeEnvelopeBindingEligibility && rejectedBindingReuse.HasValue)
                    {
                        Assert.That(enabled, Is.True);
                        Assert.That(coarseEnvelope, Is.EqualTo(true),
                            "binding composition requires the explicit envelope baseline");
                        Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, Is.False,
                            "qualification must not promote the production default");
                        Assert.That(query.EnableBruteKind5PresenceForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteCoarseDispatchForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteRejectedBindingReuseForDiagnostics = rejectedBindingReuse.Value;
                    }
                    if (ordinalPacket.HasValue)
                    {
                        Assert.That(enabled && !composeEnvelopeBindingEligibility && !kind5Presence.HasValue &&
                            !eligibilityReuse.HasValue && !coarseEnvelope.HasValue && !rejectedBindingReuse.HasValue,
                            Is.True, "ordinal packet qualification may not inherit untested compositions");
                        Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteKind5PresenceForDiagnostics ||
                            query.EnableBruteEligibilityReuseForDiagnostics || query.EnableBruteCoarseEnvelopeForDiagnostics ||
                            query.EnableBruteCoarseProofReuseForDiagnostics || query.EnableBruteRejectedBindingReuseForDiagnostics ||
                            query.EnableBruteCoarseDispatchForDiagnostics || query.EnableBruteBranchTimingForDiagnostics ||
                            query.EnableBruteEnvelopeBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteOrdinalPacketForDiagnostics = ordinalPacket.Value;
                    }
                    if (depthReject.HasValue)
                    {
                        Assert.That(enabled && !composeEnvelopeBindingEligibility && !kind5Presence.HasValue &&
                            !eligibilityReuse.HasValue && !coarseEnvelope.HasValue && !rejectedBindingReuse.HasValue &&
                            !ordinalPacket.HasValue, Is.True,
                            "depth qualification may not inherit untested compositions");
                        Assert.That(query.EnableBruteDepthRejectForDiagnostics, Is.False);
                        Assert.That(query.EnableBruteOrdinalPacketForDiagnostics || query.EnableBruteKind5PresenceForDiagnostics ||
                            query.EnableBruteEligibilityReuseForDiagnostics || query.EnableBruteCoarseEnvelopeForDiagnostics ||
                            query.EnableBruteCoarseProofReuseForDiagnostics || query.EnableBruteRejectedBindingReuseForDiagnostics ||
                            query.EnableBruteCoarseDispatchForDiagnostics || query.EnableBruteBranchTimingForDiagnostics ||
                            query.EnableBruteEnvelopeBranchTimingForDiagnostics, Is.False);
                        query.EnableBruteDepthRejectForDiagnostics = depthReject.Value;
                    }
                    if (thousand)
                        PrepareThousandAi(world, mode);
                    object[] depthArrays = depthReject.HasValue ? ReadDepthRejectPreparedArrays(query) : null;
                    int[] depthCapacities = depthReject.HasValue ? new[]
                    {
                        query.BruteExactParticipantCapacityForDiagnostics,
                        query.BruteExactItrCapacityForDiagnostics,
                        query.BruteExactBodyCapacityForDiagnostics,
                    } : null;
                    FieldInfo packetArrayField = ordinalPacket.HasValue ? typeof(BruteForceSceneQuery).GetField(
                        "_bruteOrdinalPackets", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                    object packetArray = packetArrayField?.GetValue(query);
                    int packetCapacity = query.BruteOrdinalPacketCapacityForDiagnostics;
                    if (ordinalPacket.HasValue)
                    {
                        Assert.That(packetArrayField, Is.Not.Null);
                        Assert.That(packetArray, Is.Not.Null);
                        Assert.That(packetCapacity, Is.GreaterThanOrEqualTo((world.ObjectCount + 15) / 16));
                    }
                    ulong before = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                    for (int tick = 1; tick <= ticks; tick++)
                    {
                        Assert.That(driver.StepOneTick(inputs[tick - 1], true, false), Is.True,
                            "completed tick=" + tick + ", fast=" + enabled);
                        if (native != null)
                            CompareRoot(world, native[tick - 1], tick);
                        Assert.That(world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                            Is.Zero, "AI post-commit tick=" + tick);
                        if (ordinalPacket.HasValue)
                        {
                            Assert.That(packetArrayField.GetValue(query), Is.SameAs(packetArray), "packet array tick=" + tick);
                            Assert.That(query.BruteOrdinalPacketCapacityForDiagnostics, Is.EqualTo(packetCapacity));
                        }
                        if (depthReject.HasValue)
                        {
                            object[] actualArrays = ReadDepthRejectPreparedArrays(query);
                            for (int index = 0; index < depthArrays.Length; index++)
                                Assert.That(actualArrays[index], Is.SameAs(depthArrays[index]),
                                    "depth prepared array=" + index + ", tick=" + tick);
                            Assert.That(query.BruteExactParticipantCapacityForDiagnostics, Is.EqualTo(depthCapacities[0]));
                            Assert.That(query.BruteExactItrCapacityForDiagnostics, Is.EqualTo(depthCapacities[1]));
                            Assert.That(query.BruteExactBodyCapacityForDiagnostics, Is.EqualTo(depthCapacities[2]));
                            Assert.That(query.LastBruteDepthRejectAppliedForDiagnostics, Is.EqualTo(depthReject.Value));
                        }
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
                            eligibilityReuseApplied = query.LastBruteEligibilityReuseAppliedForDiagnostics,
                            coarseEnvelopeEnabled = query.EnableBruteCoarseEnvelopeForDiagnostics,
                            coarseEnvelopeDirections = query.LastBruteCoarseEnvelopeDirectionCountForDiagnostics,
                            coarseEnvelopeRejects = query.LastBruteCoarseEnvelopeRejectCountForDiagnostics,
                            bindingReuseApplied = query.LastBruteRejectedBindingReuseAppliedForDiagnostics,
                            bindingFirstProbes = query.LastBruteRejectedBindingProbeCountForDiagnostics,
                            bindingReuses = query.LastBruteRejectedBindingReuseCountForDiagnostics,
                            ordinalPacketApplied = ordinalPacket.HasValue && query.LastBruteOrdinalPacketAppliedForDiagnostics,
                            ordinalPacketFallback = ordinalPacket.HasValue && query.LastBruteOrdinalPacketFallbackForDiagnostics,
                            ordinalPacketRejects = ordinalPacket.HasValue ? query.LastBruteOrdinalPacketRejectedDirectionCountForDiagnostics : 0,
                            ordinalPacketProofs = ordinalPacket.HasValue ? query.LastBruteOrdinalPacketProofCountForDiagnostics : 0,
                            nativeScalarState = ordinalPacket.HasValue || depthReject.HasValue
                                ? JsonConvert.SerializeObject(world.NativeRandom.CaptureScalarState()) : null,
                            legacyScalarState = ordinalPacket.HasValue || depthReject.HasValue ? world.Rng.State : 0,
                            bruteExactDirections = ordinalPacket.HasValue || depthReject.HasValue
                                ? query.LastBruteExactCacheDirectionCountForDiagnostics : 0,
                            bruteGeometryRejects = ordinalPacket.HasValue || depthReject.HasValue
                                ? query.LastBruteGeometryFirstRejectCountForDiagnostics : 0,
                            actualAi = ordinalPacket.HasValue || depthReject.HasValue
                                ? Enumerable.Range(0, 1000).Select(world.FindEntityByRuntimeSlotForQuery)
                                .OfType<LF2Character>().Count(character => character.AiControlled) : 0,
                            depthRejectApplied = depthReject.HasValue && query.LastBruteDepthRejectAppliedForDiagnostics,
                            depthRejects = depthReject.HasValue ? query.LastBruteDepthRejectedDirectionCountForDiagnostics : 0,
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
            public bool eligibilityReuseApplied;
            public bool coarseEnvelopeEnabled;
            public long coarseEnvelopeDirections;
            public long coarseEnvelopeRejects;
            public bool bindingReuseApplied;
            public long bindingFirstProbes;
            public long bindingReuses;
            public bool ordinalPacketApplied;
            public bool ordinalPacketFallback;
            public long ordinalPacketRejects;
            public long ordinalPacketProofs;
            public string nativeScalarState;
            public uint legacyScalarState;
            public long bruteExactDirections;
            public long bruteGeometryRejects;
            public int actualAi;
            public bool depthRejectApplied;
            public long depthRejects;
        }
    }
}
#endif
