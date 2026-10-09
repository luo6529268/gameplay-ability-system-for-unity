#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class H06CollectorPreparationEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD-H06-PROFILER-RESET-100-PREP-20261009/";
        private const string Marker = "H06.Preparation100.Collect";
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;

        [Test]
        [Explicit("User-authorized discard of retained Profiler memory history only.")]
        public void ClearAuthorizedProfilerHistory()
        {
            RequireIdle();
            string root = NewOutput("reset");
            JObject before = Snapshot();
            int first = ProfilerDriver.firstFrameIndex;
            int last = ProfilerDriver.lastFrameIndex;
            Save(root, "before.json", new { before, first, last,
                authorization = "2026-10-09 current user authorizes old Profiler cache discard; no disk evidence deletion",
                limitation = "Retained history has no complete export; discarded samples cannot be restored." });
            ProfilerDriver.ClearAllFrames();
            JObject after = Snapshot();
            Save(root, "history-reset.json", new { before, after, firstBefore = first, lastBefore = last,
                firstAfter = ProfilerDriver.firstFrameIndex, lastAfter = ProfilerDriver.lastFrameIndex,
                settingsEqual = JToken.DeepEquals(before, after), diskFilesDeleted = new string[0] });
            Assert.That(ProfilerDriver.firstFrameIndex, Is.LessThan(0));
            Assert.That(ProfilerDriver.lastFrameIndex, Is.LessThan(0));
            Assert.That(JToken.DeepEquals(before, after), Is.True);
            RequireIdle();
            TestContext.WriteLine(root);
        }

        [UnityTest]
        [Explicit("100-object allocation localization only; never a thousand-object admission test.")]
        [Timeout(60000)]
        public IEnumerator Capture100AllocationCallsites()
        {
            RequireIdle();
            Assert.That(ProfilerDriver.firstFrameIndex, Is.LessThan(0), "Run the authorized cache reset first.");
            string root = NewOutput("capture-100");
            object savedSettings = ReadSettings();
            JObject before = Snapshot();
            Save(root, "settings-before.json", before);
            SimulationWorld world = null;
            var participants = new LF2Entity[100];
            BattleScopedGcAllocationRecorder recorder = null;
            var captured = new List<object>();
            var errors = new List<string>();
            Application.LogCallback handler = (message, trace, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    errors.Add(message);
                }
            };
            int markers = 0;
            try
            {
                world = CreateFixture(participants);
                var query = (BruteForceSceneQuery)world.SceneQuery;
                query.PrepareBattleCapacity(participants.Length, 3, 2);
                for (int warm = 0; warm < 4; warm++)
                {
                    Collect(world);
                }
                world.RuntimeCapacity.Seal();
                long rejectedBefore = world.RuntimeCapacity.RejectedGrowthCount;
                recorder = new BattleScopedGcAllocationRecorder();
                var calibrationBefore = recorder.Calibrate();
                var preflight = new BattleScopedGcAllocationRecorder.ScopeResult[3];
                for (int sample = 0; sample < preflight.Length; sample++)
                {
                    world.Rng.Seed(1314149188u);
                    world.CaptureCollisionFrameSnapshotsAll();
                    Assert.That(recorder.Begin(), Is.True);
                    try
                    {
                        world.CollectCollisionCandidatesAll();
                    }
                    finally
                    {
                        preflight[sample] = recorder.End();
                    }
                    world.EndCollisionCandidateConsumption();
                }
                var calibrationAfter = recorder.Calibrate();
                Save(root, "preflight.json", new { scope = "100-object collector only; no Driver or performance certification",
                    count = participants.Length, ordinary = 76, specialAttack = 24, teamOne = 50, teamTwo = 50,
                    types = participants.GroupBy(x => x.GetType().Name).ToDictionary(x => x.Key, x => x.Count()),
                    layout = "original 40 columns, 16 X spacing, 20 Z spacing; scaled role/team cutoffs",
                    recorder.Unit, calibrationBefore, calibrationAfter, preflight,
                    pairs = query.LastFormalPairCountForDiagnostics,
                    actualMode = query.LastFormalCollectorModeForDiagnostics,
                    fallback = query.LastFormalFallbackParticipantCountForDiagnostics,
                    aborted = query.LastFormalCollectionAbortedForDiagnostics,
                    rejectedBefore, rejectedAfter = world.RuntimeCapacity.RejectedGrowthCount });
                Assert.That(calibrationBefore.passed && calibrationAfter.passed, Is.True);
                Assert.That(preflight.All(x => x.valid && x.calibratedBefore), Is.True);
                Assert.That(preflight.Any(x => x.allocationEvents > 0), Is.True,
                    "Not reproduced at 100; retain evidence and do not claim the thousand-object failure is fixed.");
                Assert.That(query.LastFormalCollectorModeForDiagnostics,
                    Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
                Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
                Assert.That(world.RuntimeCapacity.RejectedGrowthCount, Is.EqualTo(rejectedBefore));

                Application.logMessageReceived += handler;
                foreach (ProfilerArea area in Enum.GetValues(typeof(ProfilerArea)))
                {
                    ProfilerDriver.SetAreaEnabled(area, area == ProfilerArea.CPU);
                }
                ProfilerDriver.profileEditor = true;
                Profiler.enableBinaryLog = false;
                Profiler.enableAllocationCallstacks = true;
                ProfilerDriver.enabled = true;
                Profiler.enabled = true;
                yield return null;
                for (int sample = 0; sample < 3; sample++)
                {
                    world.Rng.Seed(1314149188u);
                    world.CaptureCollisionFrameSnapshotsAll();
                    Profiler.BeginSample(Marker);
                    try
                    {
                        world.CollectCollisionCandidatesAll();
                    }
                    finally
                    {
                        Profiler.EndSample();
                    }
                    world.EndCollisionCandidateConsumption();
                    yield return null;
                }
                yield return null;
                Profiler.enabled = false;
                ProfilerDriver.enabled = false;
                markers = ExportCallsites(captured);
                Save(root, "callsites.json", new { scope = "instrumented 100-object localization; no performance or admission claim",
                    marker = Marker, markers, captured, errors });
                string raw = Path.Combine(root, "capture.raw");
                Assert.That(File.Exists(raw), Is.False);
                ProfilerDriver.SaveProfile(raw);
                Save(root, "export.json", new { raw, exists = File.Exists(raw),
                    bytes = File.Exists(raw) ? new FileInfo(raw).Length : 0, errors });
                Assert.That(errors, Is.Empty);
                Assert.That(markers, Is.EqualTo(3));
                Assert.That(captured.Count, Is.GreaterThan(0));
                Assert.That(File.Exists(raw) && new FileInfo(raw).Length > 0, Is.True);
            }
            finally
            {
                Profiler.enabled = false;
                ProfilerDriver.enabled = false;
                Application.logMessageReceived -= handler;
                if (recorder != null)
                {
                    recorder.Dispose();
                }
                try
                {
                    if (world != null)
                    {
                        world.RuntimeCapacity.Unseal();
                        foreach (LF2Entity participant in participants)
                        {
                            if (participant != null)
                            {
                                world.Unregister(participant);
                            }
                        }
                    }
                }
                finally
                {
                    RestoreSettings(savedSettings);
                    var areas = (JObject)before["areas"];
                    foreach (JProperty property in areas.Properties())
                    {
                        ProfilerDriver.SetAreaEnabled((ProfilerArea)Enum.Parse(typeof(ProfilerArea), property.Name),
                            (bool)property.Value);
                    }
                    JObject after = Snapshot();
                    Save(root, "cleanup.json", new { before, after, settingsEqual = JToken.DeepEquals(before, after),
                        objects = world == null ? 0 : world.ObjectCount,
                        slots = world == null ? 0 : world.ClaimedRuntimeSlotCountForDiagnostics,
                        markers, allocationSamples = captured.Count, errors });
                    Assert.That(JToken.DeepEquals(before, after), Is.True, "Restore this run's exact known settings.");
                }
                Assert.That(world == null || world.ObjectCount == 0, Is.True);
                Assert.That(world == null || world.ClaimedRuntimeSlotCountForDiagnostics == 0, Is.True);
                RequireIdle();
                TestContext.WriteLine(root);
            }
        }

        private static SimulationWorld CreateFixture(LF2Entity[] participants)
        {
            Type fixture = typeof(NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests);
            var frame = (LF2FrameData)fixture.GetMethod("MakeFrame", PrivateStatic).Invoke(null, new object[]
            {
                new InteractionArea { kind = 0, vrest = 1, x = -16, y = -5, w = 32, h = 10, zwidth = 15 },
                new BodyBox { kind = 0, x = 0, y = -5, w = 10, h = 10 },
            });
            MethodInfo create = fixture.GetMethod("CreateCharacter", PrivateStatic);
            MethodInfo register = fixture.GetMethod("Register", PrivateStatic);
            var world = new SimulationWorld(BattleRuntimeProfile.DesktopExtended, 1100,
                CollisionBroadphaseBackend.LooseQuadtree);
            try
            {
                for (int slot = 0; slot < participants.Length; slot++)
                {
                    var entity = (LF2Character)create.Invoke(null, new object[]
                    {
                        "H06Prep100_" + slot, 1, frame, slot >= 76,
                    });
                    participants[slot] = entity;
                    int x = slot % 40 * 16;
                    int z = slot / 40 * 20;
                    register.Invoke(null, new object[] { world, entity, slot, slot < 50 ? 1 : 2, x });
                    entity.Runtime.SetPosition(x, 0, z);
                    entity.Runtime.SyncIntegerPosition();
                }
                return world;
            }
            catch
            {
                foreach (LF2Entity participant in participants)
                {
                    if (participant != null)
                    {
                        world.Unregister(participant);
                    }
                }
                throw;
            }
        }

        private static void Collect(SimulationWorld world)
        {
            world.Rng.Seed(1314149188u);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            world.EndCollisionCandidateConsumption();
        }

        private static int ExportCallsites(List<object> samples)
        {
            int markers = 0;
            int scanned = 0;
            int last = ProfilerDriver.lastFrameIndex;
            var addresses = new List<ulong>();
            for (int frame = ProfilerDriver.firstFrameIndex; frame >= 0 && frame <= last;
                frame = ProfilerDriver.GetNextFrameIndex(frame))
            {
                Assert.That(++scanned, Is.LessThanOrEqualTo(30), "bounded short capture");
                using (var view = ProfilerDriver.GetRawFrameDataView(frame, 0))
                {
                    if (view.valid)
                    {
                        for (int index = 0; index < view.sampleCount; index++)
                        {
                            if (view.GetSampleName(index) != Marker)
                            {
                                continue;
                            }
                            markers++;
                            int end = index + view.GetSampleChildrenCountRecursive(index);
                            for (int child = index + 1; child <= end; child++)
                            {
                                if (view.GetSampleName(child) != "GC.Alloc")
                                {
                                    continue;
                                }
                                addresses.Clear();
                                view.GetSampleCallstack(child, addresses);
                                var stack = new List<object>();
                                foreach (ulong address in addresses)
                                {
                                    var method = view.ResolveMethodInfo(address);
                                    stack.Add(new { method.methodName, method.sourceFileName, method.sourceFileLine });
                                }
                                samples.Add(new { frame, child,
                                    bytes = view.GetSampleMetadataCount(child) > 0 ? view.GetSampleMetadataAsLong(child, 0) : -1,
                                    stack });
                            }
                        }
                    }
                }
                if (frame == last)
                {
                    break;
                }
            }
            return markers;
        }

        private static void RequireIdle()
        {
            Assert.That(EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating, Is.False);
            BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
            Assert.That(CollisionBroadphaseBackendResolver.Resolve(null, null),
                Is.EqualTo(CollisionBroadphaseBackend.BruteForce));
        }

        private static object ReadSettings()
        {
            return typeof(BattleOptimizationCpuGcCaptureEditor).GetMethod("ReadSettings", PrivateStatic).Invoke(null, null);
        }

        private static void RestoreSettings(object settings)
        {
            typeof(BattleOptimizationCpuGcCaptureEditor).GetMethod("RestoreSettings", PrivateStatic)
                .Invoke(null, new[] { settings });
        }

        private static JObject Snapshot()
        {
            var areas = new JObject();
            foreach (ProfilerArea area in Enum.GetValues(typeof(ProfilerArea)))
            {
                areas[area.ToString()] = ProfilerDriver.IsAreaEnabled(area);
            }
            return new JObject { ["settings"] = JToken.FromObject(ReadSettings()),
                ["gpu"] = ProfilerDriver.profileGPU, ["areas"] = areas };
        }

        private static string NewOutput(string name)
        {
            string path = Path.GetFullPath(Output + name + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        private static void Save(string root, string name, object value)
        {
            using (var stream = new FileStream(Path.Combine(root, name), FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
            }
        }
    }
}
#endif
