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
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OpointMaterializerEditorTests
    {
        [Test]
        public void BirthTaskDeclaresExplicitSourceCoordinatePresence()
        {
            foreach (string name in new[] { "useSourceRulePosition", "sourceRuleX", "sourceRuleZ",
                "useInitialSourceRuleIntPosition", "initialSourceRuleX", "initialSourceRuleZ" })
            {
                Assert.That(typeof(OPointCreateTask).GetField(name), Is.Not.Null, name);
            }
        }

        [Test]
        public void Kind1RandomBirthXZ_UsesOneFixedViewRatioAfterNativeDraw()
        {
            var rawBase = CaptureKind1RandomBirth(false, false);
            var rawRandom = CaptureKind1RandomBirth(false, true);
            var viewBase = CaptureKind1RandomBirth(true, false);
            var viewRandom = CaptureKind1RandomBirth(true, true);
            double rawDelta = rawRandom.x - rawBase.xInt;
            double rawDepthDelta = rawRandom.z - rawBase.zInt;

            Assert.That(rawDelta, Is.Not.Zero);
            Assert.That(rawDepthDelta, Is.Not.Zero);
            Assert.That(rawRandom.calls - rawBase.calls, Is.EqualTo(4UL));
            Assert.That(viewRandom.calls - viewBase.calls, Is.EqualTo(4UL));
            Assert.That(viewRandom.calls, Is.EqualTo(rawRandom.calls));
            Assert.That(rawRandom.y, Is.EqualTo(rawBase.y));
            Assert.That(viewRandom.y, Is.EqualTo(viewBase.y));
            Assert.That(viewRandom.x - viewBase.xInt,
                Is.EqualTo(rawDelta * 2048.0 / 1333.0).Within(1e-9));
            Assert.That(viewRandom.z - viewBase.zInt,
                Is.EqualTo(rawDepthDelta * 1152.0 / 730.0).Within(1e-9));
            Assert.That(viewRandom.xInt, Is.EqualTo((int)viewRandom.x));
            Assert.That(viewRandom.zInt, Is.EqualTo((int)viewRandom.z));
        }

        [TestCase(false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(true, true, true)]
        public void Kind1RandomBirthUpdatesOnlyInitializedSourceHistory(bool initialized, bool configuredView, bool randomExtent)
        {
            var baseline = CaptureKind1RandomBirth(configuredView, false);
            JObject row = JObject.Parse(File.ReadLines(Source).First());
            if (randomExtent)
            {
                string original = (string)row["sourceDat"];
                string altered = original.Replace("centerx: 0 centery: 0 centerz: 0 framea: 0",
                    "centerx: 600 centery: 0 centerz: 80 framea: 0");
                Assert.That(altered, Is.Not.EqualTo(original));
                row["sourceDat"] = altered;
            }
            SimulationWorld world = CreateWorld(row);
            try
            {
                if (configuredView) world.ConfigureFixedViewRunDistance(2048, 1152);
                LF2Entity parent = world.FindEntityByRuntimeSlotForQuery(20);
                if (initialized)
                {
                    parent.Runtime.SetSourceRulePosition(-50.75, 120.75);
                    parent.Runtime.SyncSourceRuleIntegerPosition();
                }
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(child, Is.Not.Null);
                var calls = (JArray)observer.Capture()["synchronized"];
                int deltaX = 0;
                int deltaZ = 0;
                if (randomExtent)
                {
                    var randomCalls = calls.Where(call => (uint)call["callSite"] == 0x0044D3ABu ||
                        (uint)call["callSite"] == 0x0044D3B5u).ToArray();
                    Assert.That(randomCalls.Select(call => (uint)call["callSite"]),
                        Is.EqualTo(new[] { 0x0044D3ABu, 0x0044D3B5u, 0x0044D3ABu, 0x0044D3B5u }));
                    Assert.That(randomCalls.Select(call => (int)call["upperBound"]), Is.EqualTo(new[] { 600, 50, 80, 50 }));
                    deltaX = (int)randomCalls[0]["result"] * ((int)randomCalls[1]["result"] >= 25 ? -1 : 1);
                    deltaZ = (int)randomCalls[2]["result"] * ((int)randomCalls[3]["result"] >= 25 ? -1 : 1);
                    Assert.That(deltaX, Is.Not.Zero);
                    Assert.That(deltaZ, Is.Not.Zero);
                }
                Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls - baseline.calls,
                    Is.EqualTo(randomExtent ? 4UL : 0UL));
                Assert.That(child.Runtime.X, Is.EqualTo(baseline.xInt + deltaX * world.FixedViewRunDistanceScale).Within(1e-9));
                Assert.That(child.Runtime.Z, Is.EqualTo(baseline.zInt + deltaZ * world.FixedViewRunVerticalDistanceScale).Within(1e-9));
                Assert.That(child.Runtime.XInt, Is.EqualTo((int)child.Runtime.X));
                Assert.That(child.Runtime.ZInt, Is.EqualTo((int)child.Runtime.Z));
                Assert.That(child.Runtime.SourceRulePositionInitialized, Is.EqualTo(initialized));
                Assert.That(child.Runtime.SourceRuleX, Is.EqualTo(initialized ? -42 + deltaX : 0));
                Assert.That(child.Runtime.SourceRuleZ, Is.EqualTo(initialized ? 126 + deltaZ : 0));
                Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo((int)child.Runtime.SourceRuleX));
                Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo((int)child.Runtime.SourceRuleZ));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [Test]
        public void ZeroExtentKind1ReprojectsSourcePreciseFromIndependentIntegers()
        {
            JObject row = JObject.Parse(File.ReadLines(Source).First());
            SimulationWorld world = CreateWorld(row);
            try
            {
                LF2Entity parent = world.FindEntityByRuntimeSlotForQuery(20);
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(50);
                child.Runtime.SetSourceRulePosition(-12.75, 51.5);
                child.Runtime.SourceRuleXInt = -14;
                child.Runtime.SourceRuleZInt = 49;
                ulong callsBefore = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
                NTSD.Simulation.Ecs.BattleNativeOpointBirthWriter.InitializeBirth(child, new OPointCreateTask
                {
                    parent = parent, targetWorld = world,
                    opoint = BattleObjectPointValueAdapter.ToLegacyTask(parent.Frame.D.opoints[0]),
                });
                Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(callsBefore));
                Assert.That(child.Runtime.SourceRuleX, Is.EqualTo(-14));
                Assert.That(child.Runtime.SourceRuleZ, Is.EqualTo(49));
                Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo(-14));
                Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo(49));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExplicitSourceBirthPreservesIndependentIntegersAndClearsPooledTask(bool explicitIntegers)
        {
            var task = new OPointCreateTask
            {
                useDirectRuntimePosition = true, directX = 400.5, directY = -20, directZ = 300.75,
                useInitialRuntimeIntPosition = true, initialRuntimeX = 399, initialRuntimeY = -20, initialRuntimeZ = 299,
                useSourceRulePosition = true, sourceRuleX = -12.75, sourceRuleZ = 8.5,
                useInitialSourceRuleIntPosition = explicitIntegers, initialSourceRuleX = -13, initialSourceRuleZ = 7,
            };
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(task);
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(task);
            var entity = new LF2Character();
            entity.ApplyInitialRuntimePosition(task);
            Assert.That(entity.Runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(entity.Runtime.SourceRuleX, Is.EqualTo(-12.75));
            Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(9.5));
            Assert.That(entity.Runtime.SourceRuleXInt, Is.EqualTo(explicitIntegers ? -13 : -12));
            Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo(explicitIntegers ? 7 : 9));
            Assert.That(entity.Runtime.XInt, Is.EqualTo(399));
            Assert.That(entity.Runtime.Z, Is.EqualTo(301.75));
            Assert.That(entity.Runtime.ZInt, Is.EqualTo(299));
            task.Clear();
            Assert.That(task.useSourceRulePosition || task.useInitialSourceRuleIntPosition, Is.False);
            Assert.That(task.sourceRuleX, Is.Zero);
            Assert.That(task.sourceRuleZ, Is.Zero);
            Assert.That(task.initialSourceRuleX, Is.Zero);
            Assert.That(task.initialSourceRuleZ, Is.Zero);
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(task);
            var withoutHistory = new LF2Character();
            withoutHistory.ApplyInitialRuntimePosition(task);
            Assert.That(withoutHistory.Runtime.SourceRulePositionInitialized, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ActualLateMaterializerPublishesExplicitSourceBirth(bool component)
        {
            JObject row = JObject.Parse(File.ReadLines(Source).First());
            SimulationWorld world = CreateWorld(row);
            GameObject host = null;
            try
            {
                world.ConfigureFixedViewRunDistance(2048, 1152);
                LF2Entity parent = world.FindEntityByRuntimeSlotForQuery(20);
                parent.Runtime.SetSourceRulePosition(-50.75, 120.75);
                parent.Runtime.SyncSourceRuleIntegerPosition();
                if (component)
                {
                    host = new GameObject("SourceBirthMaterializer") { hideFlags = HideFlags.HideAndDontSave };
                    host.SetActive(false);
                    var factory = host.AddComponent<LF2ObjectPointFactory>();
                    typeof(LF2ObjectPointFactory).GetMethod("ProcessOpointSpawnCoreForStructuralWriter", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(factory, new object[] { parent });
                }
                else
                {
                    world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                }
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(child, Is.Not.Null);
                Assert.That(child.Runtime.SourceRulePositionInitialized, Is.True);
                Assert.That(child.Runtime.SourceRuleX, Is.Not.EqualTo(child.Runtime.X));
                Assert.That(child.Runtime.SourceRuleZ, Is.Not.EqualTo(child.Runtime.Z));
                Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo((int)child.Runtime.SourceRuleX));
                Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo((int)child.Runtime.SourceRuleZ));
            }
            finally
            {
                if (host != null) UnityEngine.Object.DestroyImmediate(host);
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        internal static object VerifySourceCoordinateRendererBirthForPlay()
        {
            Assert.That(Application.isPlaying, Is.True, "The pooled Renderer acceptance requires the real Play lifecycle.");
            JObject row = JObject.Parse(File.ReadLines(Source).First());
            SimulationWorld world = CreateWorld(row);
            try
            {
                world.SetLogicOnlyEntityMaterialization(false);
                world.ConfigureFixedViewRunDistance(2048, 1152);
                LF2Entity parent = world.FindEntityByRuntimeSlotForQuery(20);
                parent.Runtime.SetSourceRulePosition(-50.75, 120.75);
                parent.Runtime.SyncSourceRuleIntegerPosition();
                LF2FrameData frame = parent.Frame.D;
                BattleObjectPointValue op = frame.opoints[0];
                int relativeX = parent.Runtime.Dir == "right" ? op.X - frame.centerx : frame.centerx - op.X;
                double expectedSourceX = parent.Runtime.SourceRuleXInt + relativeX;
                double expectedSourceZ = parent.Runtime.SourceRuleZInt + op.Z + 1.0;
                double expectedBattleX = parent.Runtime.XInt + relativeX * world.FixedViewRunDistanceScale;
                double expectedBattleZ = parent.Runtime.ZInt + (op.Z + 1.0) * world.FixedViewRunVerticalDistanceScale;
                Assert.That(op.Kind, Is.EqualTo(1));
                Assert.That(op.CenterX, Is.Zero);
                Assert.That(op.CenterZ, Is.Zero);
                // Formal kind1 birth reprojects precise coordinates from integer mirrors even with zero random extent.
                expectedBattleX = (int)expectedBattleX;
                expectedBattleZ = (int)expectedBattleZ;
                var materializer = world.ResolveLateObjectPointStructuralMaterializerForModule();
                Assert.That(materializer, Is.InstanceOf<LF2ObjectPointFactory>());
                world.StructuralWriter.ProcessLateOpointSegment(materializer, parent, 1);
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(child, Is.Not.Null);
                Assert.That(child.Renderer, Is.Not.Null, "This acceptance requires the pooled Renderer materializer.");
                Assert.That(child.Runtime.SourceRulePositionInitialized, Is.True);
                Assert.That(child.Runtime.SourceRuleX, Is.EqualTo(expectedSourceX));
                Assert.That(child.Runtime.SourceRuleZ, Is.EqualTo(expectedSourceZ));
                Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo((int)expectedSourceX));
                Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo((int)expectedSourceZ));
                Assert.That(child.Runtime.X, Is.EqualTo(expectedBattleX).Within(1e-9));
                Assert.That(child.Runtime.Z, Is.EqualTo(expectedBattleZ).Within(1e-9));
                Assert.That(child.Runtime.XInt, Is.EqualTo((int)expectedBattleX));
                Assert.That(child.Runtime.ZInt, Is.EqualTo((int)expectedBattleZ));
                return new
                {
                    sourceX = child.Runtime.SourceRuleX,
                    sourceZ = child.Runtime.SourceRuleZ,
                    sourceXInt = child.Runtime.SourceRuleXInt,
                    sourceZInt = child.Runtime.SourceRuleZInt,
                    sourceInitialized = child.Runtime.SourceRulePositionInitialized,
                    battleX = child.Runtime.X,
                    battleZ = child.Runtime.Z,
                    battleXInt = child.Runtime.XInt,
                    battleZInt = child.Runtime.ZInt,
                    expectedSourceX,
                    expectedSourceZ,
                    expectedBattleX,
                    expectedBattleZ,
                    rendererPresent = child.Renderer != null,
                    materializer = materializer.GetType().FullName,
                    parentSourceXInt = parent.Runtime.SourceRuleXInt,
                    parentSourceZInt = parent.Runtime.SourceRuleZInt,
                };
            }
            finally
            {
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        [TestCase(false, false, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, false)]
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(true, true, true)]
        public void LateBirthKeepsParentSourceIntegerHistorySeparate(bool component, bool configuredView, bool left)
        {
            var world = new SimulationWorld();
            if (configuredView) world.ConfigureFixedViewRunDistance(2048, 1152);
            var parent = new LF2Character();
            parent.Runtime.SetPosition(401.75, -10, 302.75);
            parent.Runtime.SyncIntegerPosition();
            parent.Runtime.SetSourceRulePosition(100.75, 70.75);
            parent.Runtime.SyncSourceRuleIntegerPosition();
            parent.Runtime.Dir = left ? "left" : "right";
            var frame = new LF2FrameData { centerx = 5, centery = 2 };
            BattleObjectPointValue op = new ObjectPoint { x = 23, y = 4, z = -7 };
            Type type = component ? typeof(LF2ObjectPointFactory) :
                typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.BattleLogicObjectPointRuntime");
            MethodInfo configure = type.GetMethod("ConfigureLateOpointPosition", BindingFlags.Static | BindingFlags.NonPublic);
            var task = new OPointCreateTask { targetWorld = world };
            configure.Invoke(null, new object[] { task, parent, frame, op });
            LF2ObjectPointFactory.PrepareFinalRuntimePositionForCreation(task);
            var child = new LF2Character();
            child.ApplyInitialRuntimePosition(task);
            int deltaX = left ? -18 : 18;
            Assert.That(child.Runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(child.Runtime.SourceRuleX, Is.EqualTo(100 + deltaX));
            Assert.That(child.Runtime.SourceRuleZ, Is.EqualTo(64));
            Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo(100 + deltaX));
            Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo(64));
            Assert.That(child.Runtime.X, Is.EqualTo(401 + deltaX * world.FixedViewRunDistanceScale).Within(1e-9));
            Assert.That(child.Runtime.Z, Is.EqualTo(302 - 6 * world.FixedViewRunVerticalDistanceScale).Within(1e-9));
            task.Clear();
            task.targetWorld = world;
            parent.Runtime.SourceRulePositionInitialized = false;
            configure.Invoke(null, new object[] { task, parent, frame, op });
            Assert.That(task.useSourceRulePosition, Is.False);
        }

        private static (double x, int xInt, double y, double z, int zInt,
            ulong calls)
            CaptureKind1RandomBirth(bool configuredView, bool randomX)
        {
            JObject row = JObject.Parse(File.ReadLines(Source).First());
            Assert.That((string)row["name"], Is.EqualTo("kind1_type0_full"));
            if (randomX)
            {
                string original = (string)row["sourceDat"];
                string altered = original.Replace(
                    "centerx: 0 centery: 0 centerz: 0 framea: 0",
                    "centerx: 600 centery: 0 centerz: 80 framea: 0");
                Assert.That(altered, Is.Not.EqualTo(original));
                row["sourceDat"] = altered;
            }

            SimulationWorld world = CreateWorld(row);
            try
            {
                if (configuredView)
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                LF2Entity parent = world.FindEntityByRuntimeSlotForQuery(20);
                world.StructuralWriter.ProcessLateOpointSegment(
                    world.ResolveLateObjectPointStructuralMaterializerForModule(),
                    parent, 1);
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(50);
                Assert.That(child, Is.Not.Null);
                return (child.Runtime.X, child.Runtime.XInt, child.Runtime.Y,
                    child.Runtime.Z, child.Runtime.ZInt,
                    world.NativeRandom.CaptureScalarState().SynchronizedCalls);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _,
                    out string reason), Is.True, reason);
            }
        }

        private const string Source = "artifacts/diagnostics/NTSD28-Q06-OPOINT-MATERIALIZER-SOURCE-WITNESS-001/source-final/first.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static IEnumerable<TestCaseData> Cases()
        {
            for (int index = 0; index < 23; index++)
                foreach (bool component in new[] { false, true })
                    yield return new TestCaseData(index, component).SetName("OpointSource_" + index + "_" + (component ? "ComponentLateLogicMaterializer" : "WorldLogicLate"));
        }

        private const string BoundarySource = Output + "boundary-source/first.jsonl";
        private static IEnumerable<TestCaseData> BoundaryCases()
        {
            for (int index = 0; index < 6; index++)
                foreach (bool component in new[] { false, true })
                    yield return new TestCaseData(index, component).SetName("OpointBoundary_" + index + "_" + component);
        }

        [TestCaseSource(nameof(Cases))]
        public void ImmediateMatchesOriginalMaterializer(int index, bool component)
            => VerifyImmediate(index, component, Source, "12ACA9132D088999F1771B3A7170DFF56141708FD464581810D780B57F250FD7", "immediate-");

        [TestCaseSource(nameof(BoundaryCases))]
        public void BoundaryImmediateMatchesSource(int index, bool component)
            => VerifyImmediate(index, component, BoundarySource, "0B06159112E90AE615F20991CE666E67BF017186D8B4444DF28FC0F504B2A3B9", "boundary-");

        private static void VerifyImmediate(int index, bool component, string sourcePath, string expectedHash, string outputPrefix)
        {
            using (var input = File.OpenRead(sourcePath))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", ""), Is.EqualTo(expectedHash));
            var row = JObject.Parse(File.ReadAllLines(sourcePath)[index]);
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            SimulationWorld world = null;
            GameObject host = null;
            bool previousLogPolicy = LogAssert.ignoreFailingMessages;
            try
            {
                // Missing-definition diagnostics are expected; outcome/entity assertions remain strict.
                LogAssert.ignoreFailingMessages = true;
                world = CreateWorld(row);
                JObject before = Capture(world);
                CompareWorld(row["before"], before, "before", beforeDifferences);
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                var parent = world.FindEntityByRuntimeSlotForQuery(20);
                if (component)
                {
                    host = new GameObject("OpointMaterializerFixture") { hideFlags = HideFlags.HideAndDontSave };
                    host.SetActive(false);
                    var factory = host.AddComponent<LF2ObjectPointFactory>();
                    typeof(LF2ObjectPointFactory).GetMethod("ProcessOpointSpawnCoreForStructuralWriter", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(factory, new object[] { parent });
                }
                else
                    world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                JObject after = Capture(world);
                CompareWorld(row["after"], after, "after", differences);
                CompareJson(row["calls"], observer.Capture(), "calls", differences);
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + outputPrefix + index + "-" + component + ".json", JsonConvert.SerializeObject(new
                {
                    index, name = (string)row["name"], component,
                    scope = "Actual late caller, logic-only world; component route does not certify Renderer factory",
                    beforeDifferences, differences, before, after, calls = observer.Capture(),
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty, "Fixture initial state mismatch: " + string.Join("\n", beforeDifferences.Take(8)));
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(16)));
            }
            finally
            {
                if (world != null)
                {
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    world.BeginBattleShutdown();
                    Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                }
                if (host != null) UnityEngine.Object.DestroyImmediate(host);
                LogAssert.ignoreFailingMessages = previousLogPolicy;
            }
        }

        [TestCase(0)]
        [TestCase(15)]
        public void FollowingFullTickAndSnapshotReplayMatchSource(int index)
            => VerifyFollowing(index, Source, "following-");

        [TestCase(4)]
        [TestCase(5)]
        public void BoundaryFollowingAndReplayMatchSource(int index)
            => VerifyFollowing(index, BoundarySource, "boundary-following-");

        private static void VerifyFollowing(int index, string sourcePath, string prefix)
        {
            var row = JObject.Parse(File.ReadAllLines(sourcePath)[index]);
            var world = CreateWorld(row);
            try
            {
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                var parent = world.FindEntityByRuntimeSlotForQuery(20);
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                var immediateDifferences = new List<string>();
                CompareWorld(row["after"], Capture(world), "immediate", immediateDifferences);
                Assert.That(immediateDifferences, Is.Empty);
                // Source options have native AI disabled and zero controls for this full tick.
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    var entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                    if (entity != null) entity.AiControlled = false;
                }
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(observer);
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                var following = Capture(world);
                var differences = new List<string>();
                CompareWorld(row["following"], following, "following", differences);
                CompareJson(row["followingCalls"], observer.Capture(), "following.calls", differences);
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + prefix + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, differences, following, calls = observer.Capture(),
                    scope = "Actual full tick after helper birth, synthetic catalog, source native AI disabled; no counter compensation."
                }, Formatting.Indented));
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(30)));
                ulong checksum = world.CaptureRuntimeChecksum64(1, input);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                var replayObserver = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(replayObserver);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(replayObserver);
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                var replayDifferences = new List<string>();
                CompareWorld(following, Capture(world), "replay", replayDifferences);
                CompareJson(observer.Capture(), replayObserver.Capture(), "replay.calls", replayDifferences);
                Assert.That(replayDifferences, Is.Empty, string.Join("\n", replayDifferences.Take(30)));
                Assert.That(world.CaptureRuntimeChecksum64(1, input), Is.EqualTo(checksum));
                TestContext.WriteLine("Source following and full snapshot replay PASS; checksum=" + checksum);
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(null);
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        internal static void VerifyRendererBirthForPlay(int index)
        {
            bool boundary = index >= 100;
            var row = JObject.Parse(File.ReadAllLines(boundary ? BoundarySource : Source)[boundary ? index - 100 : index]);
            var world = CreateWorld(row);
            try
            {
                world.SetLogicOnlyEntityMaterialization(false);
                var beforeDifferences = new List<string>();
                CompareWorld(row["before"], Capture(world), "before", beforeDifferences);
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                var parent = world.FindEntityByRuntimeSlotForQuery(20);
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                var after = Capture(world);
                var differences = new List<string>();
                CompareWorld(row["after"], after, "after", differences);
                CompareJson(row["calls"], observer.Capture(), "calls", differences);
                foreach (var entity in (JArray)after["entities"])
                {
                    int slot = (int)entity["raw"]["slot"];
                    if (slot >= 50)
                        Assert.That(world.FindEntityByRuntimeSlotIncludingPending(slot)?.Renderer, Is.Not.Null, "Renderer slot " + slot);
                }
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "renderer-" + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, beforeDifferences, differences, after, calls = observer.Capture(),
                    scope = "True pooled Renderer children in isolated World; synthetic logic-only parent."
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty);
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(30)));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        private static SimulationWorld CreateWorld(JObject row)
        {
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                var definitions = new Dictionary<int, LF2CharacterDataWrapper>();
                var entries = new List<ObjectDefinition>();
                const string neutral = "<bmp_begin>\nname: Neutral\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
                Add(31980, 0, (string)row["sourceDat"]);
                if ((string)row["name"] != "missing_oid_stops_frame" && (int)row["oid"] > 0) Add((int)row["oid"], (int)row["type"], (string)row["targetDat"]);
                Add(778, 5, neutral);
                Add(31999, 5, neutral);
                world.PrepareRuntimeDataCatalogForBattle(entries.ToArray(), id => definitions.TryGetValue(id, out var data) ? data : null);
                var parent = Spawn(31980, 20);
                int freeSlots = (int)row["freeSlots"];
                if (freeSlots <= 2)
                    for (int slot = 50 + freeSlots; slot < world.RuntimeSlotCapacityForDiagnostics; slot++) Spawn(31999, slot);
                Restore(parent, row["before"]["entities"][0]);
                world.CharacterInputWriter.SynchronizeNativeExactAiStateFromRuntime(parent.Runtime);
                world.NativeRandom.ResetFromSeed(42);
                return world;

                void Add(int oid, int type, string dat)
                {
                    string root = Path.GetFullPath(Output + "fixture-runtime");
                    var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat, Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root));
                    data.type_sub = type;
                    definitions.Add(oid, new LF2CharacterDataWrapper(oid, data));
                    entries.Add(new ObjectDefinition(oid, type, "fixture-" + oid + ".dat"));
                }
                LF2Entity Spawn(int oid, int slot)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right", preserveActionZero = true,
                        opoint = new ObjectPoint { oid = oid, action = 0 }
                    };
                    var entity = world.LogicEntityFactory.Create(task, out var failure);
                    Assert.That(entity, Is.Not.Null, failure.ToString());
                    entity.AiControlled = false;
                    return entity;
                }
            }
            catch
            {
                world.BeginBattleShutdown();
                world.TryShutdownAndClearLogicState(out _, out _);
                throw;
            }
        }

        private static JObject Capture(SimulationWorld world)
        {
            var rawRows = (JArray)JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            var inputs = ((object[])ProjectInput.Invoke(null, new object[] { world })).Select(JObject.FromObject).ToArray();
            var entities = new JArray();
            int fillers = 0;
            foreach (JObject raw in rawRows)
            {
                if ((int)raw["identity"]["objectId"] == 31999) { fillers++; continue; }
                int slot = (int)raw["slot"];
                var e = world.FindEntityByRuntimeSlotIncludingPending(slot);
                var r = e.Runtime;
                entities.Add(new JObject
                {
                    ["raw"] = raw.DeepClone(), ["input"] = inputs.Single(item => (int)item["slot"] == slot),
                    ["extra"] = new JArray(r.HitStop, r.HitResourceInjuryDouble1A0, r.HitResourceSuppression15C,
                        r.OrdinaryCreditGate2F4, r.IncomingDamageScale340, r.HP2Orig, r.RespawnCount, r.HPOrig, r.ReviveVisualId184,
                        e.Trans.WaitCounter, e.Frame.Prev, r.AttackingCounter, r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208, r.WeaponFlightCounter),
                    ["links"] = new JArray(r.LinkState, r.HolderStableId, r.TargetSlotIndex),
                    ["position"] = new JArray(r.XInt, r.YInt, r.ZInt, r.X, r.Y, r.Z),
                    ["motion"] = new JArray(r.Vx, r.Vy, r.Vz),
                });
            }
            return new JObject { ["entities"] = entities, ["fillers"] = fillers,
                ["random"] = JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() })) };
        }

        private static void CompareWorld(JToken expected, JObject actual, string label, List<string> differences)
        {
            var actualRows = (JArray)actual["entities"];
            CompareJson(((JArray)expected["entities"]).Count, actualRows.Count, label + ".nonFillerCount", differences);
            foreach (JObject source in expected["entities"])
            {
                int slot = (int)source["raw"]["slot"];
                var found = actualRows.SingleOrDefault(item => (int)item["raw"]["slot"] == slot);
                if (found == null) { differences.Add(label + " missing slot " + slot); continue; }
                var trimmed = (JObject)source.DeepClone();
                foreach (string path in NTSD28UnityEntityRawCapture.MissingBindings)
                    (trimmed["raw"].SelectToken(path)?.Parent as JProperty)?.Remove();
                CompareJson(trimmed, found, label + ".slot" + slot, differences);
            }
            CompareJson(expected["random"], actual["random"], label + ".random", differences);
        }

        private static void CompareJson(JToken expected, JToken actual, string path, List<string> differences)
        {
            if (expected is JObject obj)
            {
                if (actual is not JObject other) { differences.Add(path + " object missing"); return; }
                foreach (var property in obj.Properties()) CompareJson(property.Value, other[property.Name], path + "." + property.Name, differences);
            }
            else if (expected is JArray array)
            {
                if (actual is not JArray other || other.Count != array.Count) { differences.Add(path + " array length differs"); return; }
                for (int i = 0; i < array.Count; i++) CompareJson(array[i], other[i], path + "[" + i + "]", differences);
            }
            else
            {
                bool numeric = expected != null && (expected.Type == JTokenType.Integer || expected.Type == JTokenType.Float);
                bool equal = numeric ? actual != null && (actual.Type == JTokenType.Integer || actual.Type == JTokenType.Float) && (double)expected == (double)actual : JToken.DeepEquals(expected, actual);
                if (!equal) differences.Add(path + "=" + actual + " expected=" + expected);
            }
        }

        private static void Restore(LF2Entity entity, JToken before)
        {
            var raw = before["raw"];
            var r = entity.Runtime;

            if (before["ordinaryCreditGate2F4"] != null)
            {
                r.OrdinaryCreditGate2F4 = (int)before["ordinaryCreditGate2F4"];
                r.InputHpConsumedTotal34C = (int)before["hpConsumed"];
                r.InputMpConsumedTotal350 = (int)before["mpConsumed"];
            }
            entity.DirectWriteNativeRawFramePreserveWaitCounter((int)raw["frame"]["action"]);
            entity.Frame.Prev = (int)raw["frame"]["previousAction"];
            entity.Frame.Prev2 = (int)raw["frame"]["tickActionSnapshot"];
            entity.Frame.Prev2D = entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev2);
            r.PrevFrame2 = entity.Frame.Prev2;
            entity.Trans.SyncDirectFrameData(entity.Frame.D?.wait ?? 0, entity.Frame.D?.next ?? 0, (int)raw["frame"]["actionLatch"]);
            entity.AttackingCounter = (int)raw["frame"]["frameCounter"];
            entity.SwitchDir((bool)raw["frame"]["facingLeft"] ? "left" : "right");
            r.SetVelocity((double)raw["motion"]["x"], (double)raw["motion"]["y"], (double)raw["motion"]["z"]);
            r.SetPosition((double)raw["position"]["preciseX"], (double)raw["position"]["preciseY"], (double)raw["position"]["preciseZ"]);
            r.XInt = (int)raw["position"]["x"]; r.YInt = (int)raw["position"]["y"]; r.ZInt = (int)raw["position"]["z"];
            r.AnimCounter = (int)raw["identity"]["controlSlot"];
            r.OwnerSlotIndex = (int)raw["identity"]["ownerSlot"];
            r.RelationTeam = (int)raw["identity"]["battleGroup"];
            r.Unk344 = (int)raw["identity"]["participantClass"];
            r.HP = (int)raw["vitals"]["currentHp"]; r.HPBound = (int)raw["vitals"]["effectiveMaxHp"]; r.HP3 = (int)raw["vitals"]["baseMaxHp"];
            r.PP = (int)raw["vitals"]["currentMp"]; r.MPMax = (int)raw["vitals"]["baseMaxMp"];
            r.HP2Orig = (int)raw["vitals"]["reviveLives"]; r.HPOrig = (int)raw["vitals"]["reviveNextLives"]; r.RespawnCount = (int)raw["vitals"]["reviveNextHp"];
            r.NativeRuntimeStateCode = (int)raw["combat"]["runtimeStateCode"];
            r.WeaponFlightCounter = (int)raw["combat"]["weaponHp"];
            r.RuntimeArmorHp118 = (int)raw["combat"]["runtimeArmorHp"];
            r.ArmorRecoveryTimer11C = (int)raw["combat"]["armorRecoveryTimer"];
            r.FrameDelay = (int)raw["combat"]["motionHoldTimer"];
            var input = before["input"]["input"];
            r.NativeInputProxy.Clear();
            string[] edgeKeys = { "attack", "jump", "defend", "right", "left", "up", "down" };
            int[] maskBits = { 2, 3, 1, 0, 4, 5, 6 };
            for (int index = 0; index < 7; index++)
            {
                r.NativeInputProxy.Current[index] = (byte)(((int)input["currentMask"] >> maskBits[index]) & 1);
                r.NativeInputProxy.Previous[index] = (byte)(((int)input["previousMask"] >> maskBits[index]) & 1);
                r.NativeInputProxy.EdgeWindow[index] = (byte)input["edgeWindow"][edgeKeys[index]];
                r.InputRemapIndices13C[index] = (byte)input["remapIndices"][index];
            }
            for (int index = 0; index < 10; index++) r.NativeInputProxy.ComboState[index] = (byte)input["comboState"][index];
            for (int index = 0; index < 5; index++) r.InputHistory[index + 1] = (int)input["keyHistory"][index];
            r.NativeInputProxy.DefendReentryCooldown = (byte)input["defendReentryCooldown"];
            r.NativeInputProxy.ProxyTail = (byte)input["proxyTail"];
            r.AnimSub = (int)input["runAccumulator"];
            r.InputLastAction144 = (int)input["lastAction"];
            r.InputRemapState138 = (int)input["remapState"];
            r.BoundState198 = (int)input["boundState"];
            r.InputGlobalRecordState20 = (int)input["globalRecordState"];
            r.HitStop = (int)raw["combat"]["renderPhase"];
            r.AttackExempt = (int)raw["combat"]["attackerRest"];
            r.CollisionYReference = (int)raw["combat"]["collisionYReference"];
            r.Fall = (int)raw["combat"]["hitReactionTimer"];
            r.Bdefend = (int)raw["combat"]["bdefendAccumulator"];
            r.SpecialHitLatch0EB = (bool)raw["combat"]["specialHitLatch0eb"];
            r.EnvironmentState320 = (int)raw["combat"]["environmentState"];
            r.EnvironmentSourceSlot160 = (int)raw["combat"]["environmentSourceSlot"];
            r.ObjectAiExcludedGroupSourceSlot2F8 = (int)raw["combat"]["objectAiExcludedGroupSourceSlot"];
            r.NativeLifecycleCode = (int)raw["lifecycle"]["code"];
            r.NativeLifecycleResolutionPending = (bool)raw["lifecycle"]["resolutionPending"];
            var extra = before["extra"];
            r.HitResourceInjuryDouble1A0 = (int)extra[1]; r.HitResourceSuppression15C = (int)extra[2];
            r.OrdinaryCreditGate2F4 = (int)extra[3]; r.IncomingDamageScale340 = (int)extra[4];
            r.ReviveVisualId184 = (int)extra[8];
            r.DisplayCurrentHp200 = (int)extra[12]; r.DisplayEffectiveMaxHp208 = (int)extra[13];
            r.LinkState = (int)before["links"][0]; r.HolderStableId = (int)before["links"][1]; r.TargetSlotIndex = (int)before["links"][2];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            entity.RefreshRuntimeSnapshot();
        }

        private sealed class Observer : INTSD28NativeRandomCallObserver
        {
            private readonly List<object> crt = new();
            private readonly List<object> synchronized = new();
            public void OnCrtNext(NTSD28NativeCrtCall call) => crt.Add(new { result = call.Result, stateAfter = call.StateAfter, totalCalls = call.TotalCalls });
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call) => synchronized.Add(new
            {
                callSite = call.CallSite, upperBound = call.UpperBound, result = call.Result,
                counterAfter = call.CounterAfter, indexAfter = call.IndexAfter, totalCalls = call.TotalCalls
            });
            internal JObject Capture() => JObject.FromObject(new { crt, synchronized });
        }
    }
}
#endif
