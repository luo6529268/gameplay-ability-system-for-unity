#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleMaterializationCountersEditorTests
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            BattleCentralRenderSystem.ResetRuntime();
        }

        [TearDown]
        public void TearDown()
        {
            BattleCentralRenderSystem.ResetRuntime();
        }

        [Test]
        public void FirstRequest_IsPublicationChanged()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            Assert.That(RecordRequest(diagnostics, new SimulationWorld(), 1, 0.25).ToString(),
                Is.EqualTo("PublicationChanged"));
            Assert.That(Count(diagnostics, "MaterializationRequestCount"), Is.EqualTo(1));
            Assert.That(Count(diagnostics, "PublicationChangedRequestCount"), Is.EqualTo(1));
        }

        [Test]
        public void SamePublication_ChangedAlpha_IsAlphaChanged()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            RecordRequest(diagnostics, world, 7, 0.25);
            Assert.That(RecordRequest(diagnostics, world, 7, 0.5).ToString(), Is.EqualTo("AlphaChanged"));
            Assert.That(Count(diagnostics, "AlphaChangedRequestCount"), Is.EqualTo(1));
        }

        [Test]
        public void SamePublication_SameAlpha_IsRepeated()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            RecordRequest(diagnostics, world, 7, 1.0);
            Assert.That(RecordRequest(diagnostics, world, 7, 1.0).ToString(), Is.EqualTo("Repeated"));
            Assert.That(Count(diagnostics, "RepeatedSampleRequestCount"), Is.EqualTo(1));
        }

        [Test]
        public void PublicationAndAlphaChanged_PublicationTakesPrecedence()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            RecordRequest(diagnostics, world, 7, 0.8);
            Assert.That(RecordRequest(diagnostics, world, 8, 0.1).ToString(), Is.EqualTo("PublicationChanged"));
            Assert.That(Count(diagnostics, "AlphaChangedRequestCount"), Is.Zero);
        }

        [Test]
        public void DifferentWorld_EqualVersionAndAlpha_IsPublicationChanged()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            RecordRequest(diagnostics, new SimulationWorld(), 7, 0.25);
            Assert.That(RecordRequest(diagnostics, new SimulationWorld(), 7, 0.25).ToString(),
                Is.EqualTo("PublicationChanged"));
            Assert.That(Count(diagnostics, "PublicationChangedRequestCount"), Is.EqualTo(2));
        }

        [Test]
        public void VersionWrap_IsPublicationChanged_NotTickComparison()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            RecordRequest(diagnostics, world, int.MaxValue, 1.0);
            Assert.That(RecordRequest(diagnostics, world, 1, 1.0).ToString(), Is.EqualTo("PublicationChanged"));
        }

        [Test]
        public void AlphaComparison_UsesExactExistingSample_NoEpsilon()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            RecordRequest(diagnostics, world, 1, 0.5);
            Assert.That(RecordRequest(diagnostics, world, 1, 0.5000000000000001).ToString(),
                Is.EqualTo("AlphaChanged"));
        }

        [Test]
        public void RequestBaseline_IsPreviousObservedRequest_NotSuccessfulBuild()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            RecordRequest(diagnostics, world, 1, 0.1);
            RecordRequest(diagnostics, world, 1, 0.2);
            Assert.That(RecordRequest(diagnostics, world, 1, 0.2).ToString(), Is.EqualTo("Repeated"));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.Zero);
        }

        [Test]
        public void RequestAndAttemptTotals_AreSeparateAndEqualTheirCategorySums()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            object publication = RecordRequest(diagnostics, world, 1, 0.1);
            object alpha = RecordRequest(diagnostics, world, 1, 0.2);
            object repeated = RecordRequest(diagnostics, world, 1, 0.2);
            RecordRequest(diagnostics, world, 2, 0.2);
            RecordAttempt(diagnostics, publication);
            RecordAttempt(diagnostics, alpha);
            RecordAttempt(diagnostics, repeated);
            Assert.That(Count(diagnostics, "MaterializationRequestCount"), Is.EqualTo(4));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.EqualTo(3));
            Assert.That(Count(diagnostics, "PublicationChangedRequestCount") +
                Count(diagnostics, "AlphaChangedRequestCount") + Count(diagnostics, "RepeatedSampleRequestCount"),
                Is.EqualTo(Count(diagnostics, "MaterializationRequestCount")));
            Assert.That(Count(diagnostics, "PublicationChangedAttemptCount"), Is.EqualTo(1));
            Assert.That(Count(diagnostics, "AlphaChangedAttemptCount"), Is.EqualTo(1));
            Assert.That(Count(diagnostics, "RepeatedSampleAttemptCount"), Is.EqualTo(1));
        }

        [Test]
        public void NoneAttempt_IsNotCounted()
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            MethodInfo request = RequestMethod();
            RecordAttempt(diagnostics, Enum.Parse(request.ReturnType, "None"));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.Zero);
        }

        [Test]
        public void EmptyQueuedWorld_IsNotARequest()
        {
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(10);
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "MaterializationRequestCount"), Is.Zero);
        }

        [Test]
        public void SameUnityFrame_ReusesWithoutAttempt_EvenWhenNewPublicationArrives()
        {
            var world = CreateWorld(BattlePresentationBackendMode.CentralOnly);
            Publish(world, 10);
            SetStatic("lastMaterializedUnityFrame", 700);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(700);
            Publish(world, 11);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(700);
            var diagnostics = BattleCentralRenderSystem.Diagnostics;
            Assert.That(Count(diagnostics, "PublicationChangedRequestCount"), Is.EqualTo(2));
            Assert.That(Count(diagnostics, "SameUnityFrameReuseCount"), Is.EqualTo(2));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.Zero);
        }

        [Test]
        public void SameSample_ReusesWithoutAttempt_AndCountsRepeatedRequests()
        {
            var world = CreateWorld(BattlePresentationBackendMode.CentralOnly);
            Publish(world, 10);
            SetStatic("lastMaterializedPublicationVersion", GetStatic("pendingPublicationVersion"));
            SetStatic("lastBuiltDisplayAlpha", 1.0);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(701);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(702);
            var diagnostics = BattleCentralRenderSystem.Diagnostics;
            Assert.That(Count(diagnostics, "SameSampleReuseCount"), Is.EqualTo(2));
            Assert.That(Count(diagnostics, "RepeatedSampleRequestCount"), Is.EqualTo(1));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.Zero);
        }

        [Test]
        public void BusyGate_IsCounted_WithoutAttemptOrBuild()
        {
            var world = CreateWorld(BattlePresentationBackendMode.CentralOnly);
            Publish(world, 10);
            SetStatic("materializationInProgress", 1);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(703);
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "ReentrantSkipCount"), Is.EqualTo(1));
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "MaterializationAttemptCount"), Is.Zero);
        }

        [Test]
        public void ForcedCachedPlan_CountsAttempts_NotActualMeshUploads()
        {
            var world = CreateWorld(BattlePresentationBackendMode.CentralOnly);
            Publish(world, 10);
            var plan = new BattlePixelFramePlan(world, world.BattlePresentation.PublishedFrame,
                BattlePresentationBackendMode.CentralOnly, BattlePixelFrameOwner.Central,
                10, 10, 100, false, "synthetic cached-plan gate only", null);
            world.PublishPixelFramePlan(plan);
            SetStatic("publishedPlanWorld", world);
            SetStatic("publishedPlanGeneration", 100);
            SetStatic("lastBuiltDisplayAlpha", 1.0);
            long uploadBefore = BattleCentralRenderSystem.MeshBackend.Diagnostics.VertexUploadCallCount;
            BattlePixelFramePlan first = BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            BattlePixelFramePlan second = BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            var diagnostics = BattleCentralRenderSystem.Diagnostics;
            Assert.That(first.Generation, Is.EqualTo(100));
            Assert.That(second.Generation, Is.EqualTo(100));
            Assert.That(first.UsesCentralPixels, Is.False, "Synthetic plan is not a real rendered submission.");
            Assert.That(Count(diagnostics, "MaterializationRequestCount"), Is.EqualTo(2));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.EqualTo(2));
            Assert.That(Count(diagnostics, "PublicationChangedAttemptCount"), Is.EqualTo(1));
            Assert.That(Count(diagnostics, "RepeatedSampleAttemptCount"), Is.EqualTo(1));
            Assert.That(BattleCentralRenderSystem.MeshBackend.Diagnostics.VertexUploadCallCount,
                Is.EqualTo(uploadBefore));
        }

        [Test]
        public void LegacyMaterialization_DoesNotEnterCentralRequestCounters()
        {
            var world = CreateWorld(BattlePresentationBackendMode.LegacyOnly);
            Publish(world, 10);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            Assert.That(plan.IsValid, Is.True);
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "MaterializationRequestCount"), Is.Zero);
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "MaterializationAttemptCount"), Is.Zero);
        }

        [Test]
        public void ExpectedWorldMismatch_DoesNotObserveQueuedWorld()
        {
            var world = CreateWorld(BattlePresentationBackendMode.CentralOnly);
            Publish(world, 10);
            MethodInfo materialize = typeof(BattleCentralRenderSystem).GetMethod(
                "MaterializeLatestPublishedFrame", StaticFlags);
            materialize.Invoke(null, new object[] { 704, false, new SimulationWorld(), false });
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "MaterializationRequestCount"), Is.Zero);
        }

        [Test]
        public void PerFrameDiagnosticReset_DoesNotClearCumulativeCounters()
        {
            RecordRequest(BattleCentralRenderSystem.Diagnostics, new SimulationWorld(), 1, 0.5);
            typeof(BattleCentralRenderSystem).GetMethod("ResetPerFrameDiagnostics", StaticFlags)
                .Invoke(null, new object[] { BattlePresentationBackendMode.CentralOnly, false });
            Assert.That(Count(BattleCentralRenderSystem.Diagnostics, "MaterializationRequestCount"), Is.EqualTo(1));
        }

        [Test]
        public void RuntimeReset_ClearsCountersAndObservedWorld()
        {
            var world = CreateWorld(BattlePresentationBackendMode.CentralOnly);
            Publish(world, 10);
            SetStatic("lastMaterializedUnityFrame", 705);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(705);
            var diagnostics = BattleCentralRenderSystem.Diagnostics;
            object kind = RecordRequest(diagnostics, world, 7, 0.5);
            RecordAttempt(diagnostics, kind);
            BattleCentralRenderSystem.ResetRuntime();
            string[] counters =
            {
                "MaterializationRequestCount", "PublicationChangedRequestCount",
                "AlphaChangedRequestCount", "RepeatedSampleRequestCount",
                "MaterializationAttemptCount", "PublicationChangedAttemptCount",
                "AlphaChangedAttemptCount", "RepeatedSampleAttemptCount",
                "SameUnityFrameReuseCount", "SameSampleReuseCount", "ReentrantSkipCount",
            };
            foreach (string counter in counters)
                Assert.That(Count(diagnostics, counter), Is.Zero, counter);
            FieldInfo observedWorld = typeof(BattleCentralRuntimeDiagnostics).GetField(
                "lastObservedWorld", InstanceFlags);
            Assert.That(observedWorld, Is.Not.Null);
            Assert.That(observedWorld.GetValue(diagnostics), Is.Null);
            Assert.That(RecordRequest(diagnostics, world, 7, 0.5).ToString(), Is.EqualTo("PublicationChanged"));
        }

        [Test]
        public void ScalarRequestAndAttemptHelpers_AfterPreparation_AllocateZeroManagedBytes()
        {
            MethodInfo request = RequestMethod();
            typeof(BattleMaterializationCountersEditorTests).GetMethod("CheckScalarGc", StaticFlags)
                .MakeGenericMethod(request.ReturnType).Invoke(null, new object[] { request });
        }

        private static void CheckScalarGc<TKind>(MethodInfo request)
        {
            var diagnostics = new BattleCentralRuntimeDiagnostics();
            var world = new SimulationWorld();
            var observe = (Func<SimulationWorld, int, double, TKind>)Delegate.CreateDelegate(
                typeof(Func<SimulationWorld, int, double, TKind>), diagnostics, request);
            MethodInfo attemptMethod = typeof(BattleCentralRuntimeDiagnostics).GetMethod(
                "RecordMaterializationAttempt", InstanceFlags);
            Assert.That(attemptMethod, Is.Not.Null);
            var attempt = (Action<TKind>)Delegate.CreateDelegate(typeof(Action<TKind>), diagnostics, attemptMethod);
            for (int index = 0; index < 64; index++)
                attempt(observe(world, index / 3 + 1, index % 3 == 0 ? 0.0 : 1.0));
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 512; index++)
                attempt(observe(world, index / 3 + 100, index % 3 == 0 ? 0.0 : 1.0));
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero, "Only new cached scalar helpers are measured, not the full render path.");
            Assert.That(Count(diagnostics, "MaterializationRequestCount"), Is.EqualTo(576));
            Assert.That(Count(diagnostics, "MaterializationAttemptCount"), Is.EqualTo(576));
        }

        private static SimulationWorld CreateWorld(BattlePresentationBackendMode mode)
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(mode);
            return world;
        }

        private static void Publish(SimulationWorld world, int tick)
        {
            world.BattlePresentation.BeginFrame(world, tick);
            BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
        }

        private static MethodInfo RequestMethod()
        {
            MethodInfo method = typeof(BattleCentralRuntimeDiagnostics).GetMethod(
                "RecordMaterializationRequest", InstanceFlags);
            Assert.That(method, Is.Not.Null, "Central request classification helper is missing.");
            return method;
        }

        private static object RecordRequest(BattleCentralRuntimeDiagnostics diagnostics,
            SimulationWorld world, int version, double alpha)
        {
            return RequestMethod().Invoke(diagnostics, new object[] { world, version, alpha });
        }

        private static void RecordAttempt(BattleCentralRuntimeDiagnostics diagnostics, object kind)
        {
            MethodInfo method = typeof(BattleCentralRuntimeDiagnostics).GetMethod(
                "RecordMaterializationAttempt", InstanceFlags);
            Assert.That(method, Is.Not.Null);
            method.Invoke(diagnostics, new[] { kind });
        }

        private static long Count(BattleCentralRuntimeDiagnostics diagnostics, string name)
        {
            PropertyInfo property = typeof(BattleCentralRuntimeDiagnostics).GetProperty(name, InstanceFlags);
            Assert.That(property, Is.Not.Null, "Missing diagnostic counter: " + name);
            return Convert.ToInt64(property.GetValue(diagnostics));
        }

        private static void SetStatic(string name, object value)
        {
            FieldInfo field = typeof(BattleCentralRenderSystem).GetField(name, StaticFlags);
            Assert.That(field, Is.Not.Null, name);
            field.SetValue(null, value);
        }

        private static object GetStatic(string name)
        {
            FieldInfo field = typeof(BattleCentralRenderSystem).GetField(name, StaticFlags);
            Assert.That(field, Is.Not.Null, name);
            return field.GetValue(null);
        }
    }
}
#endif
