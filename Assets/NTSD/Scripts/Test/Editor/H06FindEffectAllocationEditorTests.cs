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
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class H06FindEffectAllocationEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/";
        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        [Test]
        public void FindEffectPreservesLookupSemantics()
        {
            var first = Record(7);
            var duplicate = Record(7);
            var last = Record(int.MaxValue);
            foreach (bool available in new[] { false, true })
            foreach (var records in new[] { Array.Empty<LoganKindRecord>(), new[] { first, duplicate, last },
                new[] { Record(int.MinValue), Record(-1), Record(0) }, new[] { first, null, last } })
            {
                var catalog = (LoganKindCatalog)Activator.CreateInstance(typeof(LoganKindCatalog),
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new object[] { available, records, Array.Empty<LoganKindDiagnostic>() }, null);
                foreach (int effect in new[] { int.MinValue, -1, 0, 7, 8, int.MaxValue })
                {
                    LoganKindRecord expected = null;
                    Exception error = null;
                    try
                    {
                        foreach (LoganKindRecord item in catalog.Records)
                        {
                            if (item.Effect == effect) { expected = item; break; }
                        }
                    }
                    catch (NullReferenceException ex) { error = ex; }
                    if (error != null) Assert.Throws<NullReferenceException>(() => catalog.FindEffect(effect));
                    else Assert.That(catalog.FindEffect(effect), Is.SameAs(expected));
                }
            }
        }

        private static LoganKindRecord Record(int effect)
        {
            return (LoganKindRecord)Activator.CreateInstance(typeof(LoganKindRecord),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new object[] { effect, 1, Array.Empty<int>(), Array.Empty<int>(), 1 }, null);
        }

        [Test, Explicit("Immutable pre-repair 100-object baseline.")]
        public void Capture100BeforeRepair() { Run100(false); }

        [Test, Explicit("Compare the same 100-object fixture and calibrated collector allocations.")]
        public void Verify100AfterRepair() { Run100(true); }

        private static object Preparation(string method, params object[] args)
        {
            return typeof(H06CollectorPreparationEditorTests).GetMethod(method, PrivateStatic).Invoke(null, args);
        }

        private static void Run100(bool repaired)
        {
            Preparation("RequireIdle");
            var settings = (JObject)Preparation("Snapshot");
            var participants = new LF2Entity[100];
            SimulationWorld world = null;
            BattleScopedGcAllocationRecorder recorder = null;
            string phase = repaired ? "after" : "before";
            try
            {
                world = (SimulationWorld)Preparation("CreateFixture", new object[] { participants });
                var query = (BruteForceSceneQuery)world.SceneQuery;
                query.PrepareBattleCapacity(100, 3, 2);
                for (int warm = 0; warm < 4; warm++) Preparation("Collect", world);
                world.RuntimeCapacity.Seal();
                long rejected = world.RuntimeCapacity.RejectedGrowthCount;
                recorder = new BattleScopedGcAllocationRecorder();
                var calibrationBefore = recorder.Calibrate();
                var scopes = new BattleScopedGcAllocationRecorder.ScopeResult[3];
                var rows = new List<object>();
                for (int sample = 0; sample < 3; sample++)
                {
                    world.Rng.Seed(1314149188u);
                    world.CaptureCollisionFrameSnapshotsAll();
                    bool began = recorder.Begin();
                    try { world.CollectCollisionCandidatesAll(); }
                    finally { scopes[sample] = recorder.End(); }
                    Assert.That(began, Is.True);
                    rows.Add(CaptureResult(world, query));
                    world.EndCollisionCandidateConsumption();
                }
                var calibrationAfter = recorder.Calibrate();
                var result = JObject.FromObject(new { rows, scopes, calibrationBefore, calibrationAfter,
                    recorder.Unit, rejectedBefore = rejected, rejectedAfter = world.RuntimeCapacity.RejectedGrowthCount,
                    scope = "same preparation 100 objects; four warm, three collector-only samples; observations outside scope" });
                Save(phase + "-100.json", result);
                Assert.That(calibrationBefore.passed && calibrationAfter.passed, Is.True);
                Assert.That(scopes.All(x => x.valid && x.calibratedBefore), Is.True);
                Assert.That(scopes.All(x => x.allocationEvents == (repaired ? 0 : 22)), Is.True);
                Assert.That(world.RuntimeCapacity.RejectedGrowthCount, Is.EqualTo(rejected));
                Assert.That(query.LastFormalCollectorModeForDiagnostics, Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
                Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
                Assert.That(query.LastFormalFallbackParticipantCountForDiagnostics, Is.Zero);
                Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(97));
                if (repaired)
                {
                    var baseline = JObject.Parse(File.ReadAllText(Output + "before-100.json"));
                    Assert.That(JToken.DeepEquals(baseline["rows"], result["rows"]), Is.True,
                        "ordered candidates, payload, handles, checksum and both RNG states must match");
                }
            }
            finally
            {
                recorder?.Dispose();
                if (world != null)
                {
                    world.RuntimeCapacity.Unseal();
                    foreach (var entity in participants) if (entity != null) world.Unregister(entity);
                }
                var after = (JObject)Preparation("Snapshot");
                Save(phase + "-cleanup.json", new { settings, after, settingsEqual = JToken.DeepEquals(settings, after),
                    objects = world?.ObjectCount ?? 0, slots = world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0 });
                Assert.That(JToken.DeepEquals(settings, after), Is.True);
                Assert.That(world == null || world.ObjectCount == 0, Is.True);
                Assert.That(world == null || world.ClaimedRuntimeSlotCountForDiagnostics == 0, Is.True);
                Preparation("RequireIdle");
            }
        }

        private static object CaptureResult(SimulationWorld world, BruteForceSceneQuery query)
        {
            var cache = (Dictionary<LF2Entity, List<SceneQueryHit>>)typeof(BruteForceSceneQuery)
                .GetField("_candidateCache", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(query);
            var payload = new List<object>();
            int count = 0;
            for (int slot = 0; slot < world.MaxRuntimeSlotsForServices; slot++)
            {
                var entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null || !cache.TryGetValue(entity, out var sequence)) continue;
                Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(slot, entity, out var attacker), Is.True);
                var hits = new CollisionCandidateStoreEntry[sequence.Count];
                for (int i = 0; i < sequence.Count; i++)
                {
                    var hit = sequence[i];
                    Assert.That(world.TryGetCurrentRuntimeHandleForDiagnostics(hit.TargetSlot, hit.Target, out var target), Is.True);
                    hits[i] = new CollisionCandidateStoreEntry(hit.TargetSlot, target, hit.BodyX, hit.ItrIndex,
                        hit.RuntimeItr, hit.ZeroAttackerHpOnConsume, hit.ReleaseHeavyHeldTargetOnConsume, hit.PairSnapshot);
                }
                count += hits.Length;
                payload.Add(new { attacker, hits });
            }
            Assert.That(count, Is.GreaterThan(0), "Observe actual published candidate lists.");
            var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
            return new { payload, count, input, checksum = world.CaptureExtendedChecksumSnapshot(0, input).Hashes,
                native = world.NativeRandom.CaptureScalarState(), legacy = world.Rng.State, legacyCalls = world.Rng.CallCount,
                objects = world.ObjectCount, actualMode = query.LastFormalCollectorModeForDiagnostics,
                pairs = query.LastFormalPairCountForDiagnostics, fallback = query.LastFormalFallbackParticipantCountForDiagnostics,
                aborted = query.LastFormalCollectionAbortedForDiagnostics };
        }

        private static void Save(string name, object value)
        {
            using (var stream = new FileStream(Output + name, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream)) writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }
    }
}
#endif
