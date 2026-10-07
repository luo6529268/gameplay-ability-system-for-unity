#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Spatial;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleOptimizationAdmissionAssessmentEditorTests
    {
        private const uint CollectionSeed = 0x41C64E6Du;
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH25-BOUNDED-ASSESSMENTS-20261007/run-01";
        private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        [Serializable]
        private sealed class Observation
        {
            public string item;
            public string workload;
            public string backend;
            public string scope;
            public int replicate;
            public int participants;
            public int events;
            public int uniqueCues;
            public int warmup;
            public int samples;
            public double[] elapsedMs;
            public long[] allocatedBytes;
            public bool candidateSequenceParity;
            public int pairCount;
            public int fallbackParticipants;
            public bool aborted;
            public int directTicks;
            public int nestedTicks;
            public int sweepTicks;
            public int treeTicks;
            public long directComparisons;
            public long sweepChecks;
            public long ordinalComparisons;
            public long playCountDelta;
            public long rejectedCueDelta;
            public long voiceDropDelta;
            public int voiceCapacity;
            public int imageCount;
            public int existingImages;
            public int missingImages;
            public long imageBytesLowerBoundPerAssert;
            public string visualFingerprint;
            public string admission;
            public int knownLiveAllocationBytes;
            public bool allocationCounterResponded;
        }

        [Test]
        public void AllocationCounterPositiveControl()
        {
            const int bytes = 1024 * 1024;
            byte[] live = null;
            var row = new Observation
            {
                item = "EVIDENCE_CONTROL", workload = "KnownLiveManagedAllocation", samples = 1,
                knownLiveAllocationBytes = bytes,
                scope = "Positive control for current Editor GC.GetAllocatedBytesForCurrentThread only"
            };
            Measure(() =>
            {
                live = new byte[bytes];
                live[0] = 1;
                live[bytes - 1] = 2;
                GC.KeepAlive(live);
            }, null, row);
            Assert.That(live.Length, Is.EqualTo(bytes));
            row.allocationCounterResponded = row.allocatedBytes[0] >= bytes;
            row.admission = row.allocationCounterResponded ? "COUNTER_RESPONDED" :
                "COUNTER_NOT_VALIDATED_FOR_ZERO_GC_CERTIFICATION";
            Save("Allocation-Counter-Control", row);
            GC.KeepAlive(live);
        }

        [TestCase("Dispersed")]
        [TestCase("Combat")]
        [TestCase("Concentrated")]
        [TestCase("OPointBurst-shaped")]
        [Timeout(120000)]
        public void CollisionFourShapesCandidateParityAndBoundedCost(string workload)
        {
            Type fixtureType = typeof(NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests);
            MethodInfo makeFrame = fixtureType.GetMethod("MakeFrame", PrivateStatic);
            MethodInfo create = fixtureType.GetMethod("CreateCharacter", PrivateStatic);
            MethodInfo register = fixtureType.GetMethod("Register", PrivateStatic);
            MethodInfo run = fixtureType.GetMethods(PrivateStatic).Single(method =>
                method.Name == "RunCollection" && method.GetParameters().Length == 5);
            MethodInfo equal = fixtureType.GetMethod("AssertRunsEqual", PrivateStatic);
            var itr = new InteractionArea
            {
                kind = 0, vrest = 1,
                x = workload == "Dispersed" ? 10000 : -16,
                y = -5, w = workload == "Dispersed" ? 10 : 32,
                h = 10, zwidth = 15
            };
            LF2FrameData frame = (LF2FrameData)makeFrame.Invoke(null, new object[]
            {
                itr, new BodyBox { kind = 0, x = 0, y = -5, w = 10, h = 10 }
            });
            var world = new SimulationWorld(
                BattleRuntimeProfile.DesktopExtended, 1100, CollisionBroadphaseBackend.LooseQuadtree);
            var participants = new LF2Entity[1000];
            try
            {
                for (int slot = 0; slot < participants.Length; slot++)
                {
                    bool child = workload == "OPointBurst-shaped" && slot >= 760;
                    LF2Character entity = (LF2Character)create.Invoke(null, new object[]
                    {
                        "BoundedCollector_" + workload + "_" + slot, 1, frame, child
                    });
                    int x;
                    int z;
                    if (workload == "Dispersed")
                    {
                        x = slot * 20;
                        z = 0;
                    }
                    else
                    {
                        int columns = workload == "Combat" ? 50 :
                            workload == "Concentrated" ? 20 : 40;
                        int xSpacing = workload == "Combat" ? 20 :
                            workload == "Concentrated" ? 1 : 16;
                        int zSpacing = workload == "Combat" ? 30 :
                            workload == "Concentrated" ? 1 : 20;
                        x = (slot % columns) * xSpacing;
                        z = (slot / columns) * zSpacing;
                    }
                    register.Invoke(null, new object[] { world, entity, slot, slot < 500 ? 1 : 2, x });
                    entity.Runtime.SetPosition(x, 0, z);
                    entity.Runtime.SyncIntegerPosition();
                    participants[slot] = entity;
                }
                var query = (BruteForceSceneQuery)world.SceneQuery;
                query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                object brute = run.Invoke(null, new object[]
                {
                    world, query, CollisionFormalCollectorMode.ForceBruteForce, CollectionSeed, participants
                });
                foreach (string backend in new[] { "BruteForce", "RoleAwareDirectAdaptive", "RoleAwareTree" })
                {
                    query.ForceRoleAwareTreeForDiagnostics = backend == "RoleAwareTree";
                    query.ForceRoleAwareDirectForDiagnostics = backend == "RoleAwareDirectAdaptive";
                    query.ForceRoleAwareNestedDirectForDiagnostics = false;
                    query.ForceRoleAwareSweepDirectForDiagnostics = false;
                    CollisionFormalCollectorMode mode = backend == "BruteForce" ?
                        CollisionFormalCollectorMode.ForceBruteForce : CollisionFormalCollectorMode.ForceRoleAware;
                    object actual = run.Invoke(null, new object[] { world, query, mode, CollectionSeed, participants });
                    equal.Invoke(null, new[] { brute, actual, null });
                    query.FormalCollectorMode = mode;
                    Action collect = () =>
                    {
                        world.CaptureCollisionFrameSnapshotsAll();
                        world.CollectCollisionCandidatesAll();
                        world.EndCollisionCandidateConsumption();
                    };
                    Action seed = () => world.Rng.Seed(CollectionSeed);
                    Warm(collect, seed, 4);
                    for (int replicate = 1; replicate <= 2; replicate++)
                    {
                        var row = new Observation
                        {
                            item = "H-06", workload = workload, backend = backend, replicate = replicate,
                            participants = participants.Length, warmup = 4, samples = 8,
                            scope = "Editor collector only; no AI/full tick/actual OPoint/native authority",
                            candidateSequenceParity = true,
                            admission = "NO_DEFAULT_PROMOTION / AUTHORITY_AND_FULL_TICK_EVIDENCE_PENDING"
                        };
                        Measure(collect, seed, row);
                        row.pairCount = query.LastFormalPairCountForDiagnostics;
                        row.fallbackParticipants = query.LastFormalFallbackParticipantCountForDiagnostics;
                        row.aborted = query.LastFormalCollectionAbortedForDiagnostics;
                        row.directTicks = query.LastRoleAwareDirectTickCountForDiagnostics;
                        row.nestedTicks = query.LastRoleAwareNestedDirectTickCountForDiagnostics;
                        row.sweepTicks = query.LastRoleAwareSweepDirectTickCountForDiagnostics;
                        row.treeTicks = query.LastRoleAwareTreeTickCountForDiagnostics;
                        row.directComparisons = query.LastRoleAwareDirectComparisonCountForDiagnostics;
                        row.sweepChecks = query.LastRoleAwareSweepFullOverlapCheckCountForDiagnostics;
                        Save(workload + "-" + backend + "-" + replicate, row);
                    }
                }
            }
            finally
            {
                foreach (LF2Entity entity in participants)
                {
                    if (entity != null)
                        world.Unregister(entity);
                }
                Assert.That(world.ObjectCount, Is.Zero, "Independent collector fixture objects retained.");
                Assert.That(world.ClaimedRuntimeSlotCountForDiagnostics, Is.Zero);
            }
        }

        [TestCase(1)]
        [TestCase(8)]
        [TestCase(128)]
        [Timeout(120000)]
        public void SoundThreeDistributionsCurrentSinkCost(int uniqueCues)
        {
            Type fixtureType = typeof(NTSD28OriginalCommonAudioEditorTests)
                .GetNestedType("Fixture", BindingFlags.NonPublic);
            object fixture = Activator.CreateInstance(fixtureType, true);
            try
            {
                var player = (NTSDSoundPlayer)fixtureType.GetProperty(
                    "Player", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fixture);
                MethodInfo prepare = fixtureType.GetMethod(
                    "Prepare", BindingFlags.Instance | BindingFlags.NonPublic);
                string vfsRoot = Path.GetFullPath(Path.Combine(
                    Application.dataPath, "NTSD/Content/LoganRuntime/vfs"));
                string[] cues = Directory.GetFiles(vfsRoot, "*.wav", SearchOption.AllDirectories)
                    .OrderBy(path => path, StringComparer.Ordinal).Take(uniqueCues)
                    .Select(path => path.Substring(vfsRoot.Length + 1).Replace('/', '\\')).ToArray();
                Assert.That(cues.Length, Is.EqualTo(uniqueCues));
                foreach (string cue in cues)
                    Assert.That(prepare.Invoke(fixture, new object[] { cue, true }), Is.Not.Null);
                var sounds = new PendingSoundEvent[1000];
                for (int index = 0; index < sounds.Length; index++)
                    sounds[index] = new PendingSoundEvent(cues[index % uniqueCues], 500 + index % 201, 1);
                Action present = () => player.PresentSounds(sounds);
                Warm(present, null, 4);
                for (int replicate = 1; replicate <= 2; replicate++)
                {
                    long plays = player.PooledOneShotPlayCountForDiagnostics;
                    long rejected = player.RejectedUnpreparedCueCountForDiagnostics;
                    long drops = player.OneShotVoiceLimitDropCountForDiagnostics;
                    var row = new Observation
                    {
                        item = "M-13", workload = "SameTick1000", backend = "CanonicalPresentSounds",
                        replicate = replicate, events = sounds.Length, uniqueCues = uniqueCues,
                        warmup = 4, samples = 16,
                        scope = "Synthetic silent clips with formal cue identities; actual sink; not real battle audio",
                        ordinalComparisons = CountCanonicalComparisons(sounds),
                        admission = "NO_IMPLEMENTATION_THIS_PHASE / REAL_MULTI_CUE_HOTSPOT_NOT_ESTABLISHED"
                    };
                    Measure(present, null, row);
                    row.playCountDelta = player.PooledOneShotPlayCountForDiagnostics - plays;
                    row.rejectedCueDelta = player.RejectedUnpreparedCueCountForDiagnostics - rejected;
                    row.voiceDropDelta = player.OneShotVoiceLimitDropCountForDiagnostics - drops;
                    row.voiceCapacity = player.OneShotVoiceCountForDiagnostics;
                    Save("Sound-" + uniqueCues + "-" + replicate, row);
                    Assert.That(row.rejectedCueDelta, Is.Zero);
                    Assert.That(row.playCountDelta, Is.EqualTo(row.samples * uniqueCues));
                }
            }
            finally
            {
                ((IDisposable)fixture).Dispose();
            }
        }

        [Test]
        [Timeout(120000)]
        public void ContentFreshnessCurrentInputsBoundedWarmCacheCost()
        {
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(
                "Assets/NTSD/Config/GameConfig/GameConfig.asset");
            Assert.That(config, Is.Not.Null);
            BattleContentSource source = BattleContentSource.ForLoganRuntime(
                Path.GetFullPath(Path.Combine(Application.dataPath, "..", config.BattleContentRuntimeRoot)));
            ProjectBattleModeConfig.Snapshot mode = ProjectBattleModeConfig.LoadDefault().Capture();
            LoganVisualContentCandidate candidate = null;
            var capture = new Observation
            {
                item = "M-14", workload = "CurrentMutableInputs", backend = "CaptureIncludingAssert",
                replicate = 0, samples = 1, scope = "Current Editor/cache state; not cold startup or physical IO",
                admission = "DEPENDENCY_NOT_READY / KEEP_ALL_FRESHNESS_GUARDS"
            };
            Measure(() => candidate = LoganVisualContentCandidate.Capture(source, mode), null, capture);
            capture.imageCount = candidate.Images.Count;
            capture.visualFingerprint = candidate.VisualFingerprint;
            foreach (LoganVisualContentCandidate.ImageInput image in candidate.Images)
            {
                if (File.Exists(image.Path))
                {
                    capture.existingImages++;
                    capture.imageBytesLowerBoundPerAssert += new FileInfo(image.Path).Length;
                }
                else
                    capture.missingImages++;
            }
            Save("Content-Capture", capture);
            for (int replicate = 1; replicate <= 2; replicate++)
            {
                var row = new Observation
                {
                    item = "M-14", workload = "CurrentMutableInputs", backend = "AssertInputsCurrent",
                    replicate = replicate, samples = 1, imageCount = capture.imageCount,
                    existingImages = capture.existingImages, missingImages = capture.missingImages,
                    imageBytesLowerBoundPerAssert = capture.imageBytesLowerBoundPerAssert,
                    visualFingerprint = candidate.VisualFingerprint, scope = capture.scope,
                    admission = capture.admission
                };
                Measure(candidate.AssertInputsCurrent, null, row);
                Save("Content-Assert-" + replicate, row);
            }
        }

        private static void Warm(Action work, Action before, int count)
        {
            for (int index = 0; index < count; index++)
            {
                before?.Invoke();
                work();
            }
        }

        private static void Measure(Action work, Action before, Observation row)
        {
            row.elapsedMs = new double[row.samples];
            row.allocatedBytes = new long[row.samples];
            for (int index = 0; index < row.samples; index++)
            {
                before?.Invoke();
                long allocated = GC.GetAllocatedBytesForCurrentThread();
                long start = System.Diagnostics.Stopwatch.GetTimestamp();
                work();
                long stop = System.Diagnostics.Stopwatch.GetTimestamp();
                row.allocatedBytes[index] = GC.GetAllocatedBytesForCurrentThread() - allocated;
                row.elapsedMs[index] = (stop - start) * 1000d / System.Diagnostics.Stopwatch.Frequency;
            }
        }

        private static long CountCanonicalComparisons(PendingSoundEvent[] sounds)
        {
            long count = 0;
            for (int index = 0; index < sounds.Length; index++)
            {
                bool consumed = false;
                for (int previous = 0; previous < index; previous++)
                {
                    count++;
                    if (string.Equals(sounds[previous].Cue, sounds[index].Cue, StringComparison.Ordinal))
                    {
                        consumed = true;
                        break;
                    }
                }
                if (consumed)
                    continue;
                for (int contribution = index; contribution < sounds.Length; contribution++)
                    count++;
            }
            return count;
        }

        private static void Save(string name, Observation row)
        {
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", OutputRoot));
            Directory.CreateDirectory(root);
            using (var stream = new FileStream(Path.Combine(root, name + ".json"),
                FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(row, true));
        }
    }
}
#endif

