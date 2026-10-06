#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralPresentationDeferralEditorTests
    {
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        private static readonly Action<SimulationWorld, bool> Present = BindPresent();
        private SimulationWorld world;
        private GameObject cameraObject;
        private Camera camera;
        private Camera previousCamera;
        private BattleRenderFeature feature;
        private Material material;
        private int materializedVersion;

        [SetUp]
        public void SetUp()
        {
            BattleCentralRenderSystem.ResetRuntime();
            previousCamera = NTSDRenderSpace.WorldCamera;
            cameraObject = new GameObject("Central Deferral Test Camera") { hideFlags = HideFlags.HideAndDontSave };
            camera = cameraObject.AddComponent<Camera>();
            UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            Assert.That(cameraData.scriptableRenderer, Is.Not.Null, "Requires this project's configured URP renderer.");
            NTSDRenderSpace.BindWorldCamera(camera);
            Shader shader = Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName);
            Assert.That(shader, Is.Not.Null);
            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
            feature.hideFlags = HideFlags.HideAndDontSave;
            feature.Configure(material, BattleCentralDrawMode.OrderedChunks);
            feature.SetActive(true);
            typeof(BattleCentralRenderSystem).GetMethod("RecordFeatureCameraAvailability", StaticFlags)
                .Invoke(null, new object[] { feature, cameraData.scriptableRenderer, camera, CameraRenderType.Base });
            world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.BattlePresentation.BeginFrame(world, 701);
            BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
            BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
            materializedVersion = ReadInt("pendingPublicationVersion");
            SetField("lastMaterializedPublicationVersion", materializedVersion);
            SetField("lastBuiltDisplayAlpha", 1d);
            SetField("lastResolvedDisplayAlpha", -1d);
            SetField("lastMaterializedUnityFrame", -1);
        }

        [TearDown]
        public void TearDown()
        {
            BattleCentralRenderSystem.ResetRuntime();
            world?.ResetRuntimeState();
            NTSDRenderSpace.ClearBoundWorldCamera(camera);
            NTSDRenderSpace.BindWorldCamera(previousCamera);
            if (feature != null)
                UnityEngine.Object.DestroyImmediate(feature);
            if (material != null)
                UnityEngine.Object.DestroyImmediate(material);
            if (cameraObject != null)
                UnityEngine.Object.DestroyImmediate(cameraObject);
        }

        [Test]
        public void WarmInteractiveCentralHost_QueuesWithoutReplacingCapturedGeometry()
        {
            BattlePixelFramePlan before = BattleCentralRenderSystem.CurrentPixelFramePlan;
            world.BattlePresentation.BeginFrame(world, 702);
            ulong checksum = world.CaptureRuntimeChecksum64(702, null);
            Present(world, true);
            BattlePixelFramePlan after = BattleCentralRenderSystem.CurrentPixelFramePlan;
            Assert.That(after.Generation, Is.EqualTo(before.Generation));
            Assert.That(after.CapturedFrame, Is.SameAs(before.CapturedFrame));
            Assert.That(after.Submission, Is.SameAs(before.Submission));
            Assert.That(ReadInt("pendingPublishedTick"), Is.EqualTo(702));
            Assert.That(ReadInt("lastMaterializedPublicationVersion"), Is.EqualTo(materializedVersion));
            Assert.That(ReadInt("pendingPublicationVersion"), Is.GreaterThan(materializedVersion));
            Assert.That(world.BattlePresentation.PublishedFrame.CommandsMaterialized, Is.False);
            Assert.That(world.BattlePresentation.PublishedFrame.PresentationOrderMaterialized, Is.False);
            Assert.That(GetField("lastResolvedDisplayAlpha"), Is.EqualTo(1d));
            Assert.That(world.CaptureRuntimeChecksum64(702, null), Is.EqualTo(checksum));
        }

        [Test]
        public void PendingPublication_RejectsOldLeaseUntilCameraMaterializesLatest()
        {
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            bool acquiredOld = BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(camera, CameraRenderType.Base,
                CameraType.Game, false, out BattleCentralSubmission.BattleCentralSubmissionLease oldLease);
            oldLease.Dispose();
            Assert.That(acquiredOld, Is.False);
            // Reuses the existing editor-only publication fixture, not a production bypass.
            BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(12345);
            Assert.That(plan.SimulationTick, Is.EqualTo(702));
            Assert.That(ReadInt("lastMaterializedPublicationVersion"), Is.EqualTo(ReadInt("pendingPublicationVersion")));
            bool acquiredLatest = BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(camera, CameraRenderType.Base,
                CameraType.Game, false, out BattleCentralSubmission.BattleCentralSubmissionLease lease);
            lease.Dispose();
            Assert.That(acquiredLatest, Is.True);
        }

        [Test]
        public void ExplicitFlush_StillConsumesPendingPublicationImmediately()
        {
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            AssertConsumed();
        }

        [Test]
        public void NonInteractiveHost_StillFlushes()
        {
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, false);
            AssertConsumed();
        }

        [Test]
        public void ThirtyFpsHost_StillFlushes()
        {
            typeof(SimulationWorld).GetMethod("ConfigureBattlePresentationDisplayPolicy",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(world, new object[] { 30, 0.033f });
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            AssertConsumed();
        }

        [TestCase(BattlePresentationBackendMode.LegacyOnly)]
        [TestCase(BattlePresentationBackendMode.CentralShadowBuild)]
        public void NonCentralOnlyHost_StillFlushes(BattlePresentationBackendMode mode)
        {
            world.SetBattlePresentationBackend(mode);
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            AssertConsumed();
        }

        [Test]
        public void DisabledFeature_StillFlushesAndDoesNotClaimReady()
        {
            feature.SetActive(false);
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            AssertConsumed();
            Assert.That(BattleCentralRenderSystem.CurrentPixelFramePlan.IsStale, Is.True);
        }

        [Test]
        public void MissingMaterial_StillFlushes()
        {
            feature.Configure(null, BattleCentralDrawMode.OrderedChunks);
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            AssertConsumed();
        }

        [Test]
        public void DisabledCamera_StillFlushes()
        {
            camera.enabled = false;
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            AssertConsumed();
        }

        [Test]
        public void MissingRecentRouteObservation_StillFlushes()
        {
            SetField("observedUnityFrame", -1);
            world.BattlePresentation.BeginFrame(world, 702);
            Present(world, true);
            AssertConsumed();
        }

        [Test]
        public void StalePlan_StillFlushes()
        {
            feature.SetActive(false);
            world.BattlePresentation.BeginFrame(world, 702);
            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            Assert.That(BattleCentralRenderSystem.CurrentPixelFramePlan.IsStale, Is.True);
            feature.SetActive(true);
            world.BattlePresentation.BeginFrame(world, 703);
            Present(world, true);
            AssertConsumed();
        }

        [Test]
        public void ColdOrChangedWorld_StillFlushes()
        {
            var nextWorld = new SimulationWorld();
            try
            {
                nextWorld.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
                nextWorld.BattlePresentation.BeginFrame(nextWorld, 801);
                Present(nextWorld, true);
                AssertConsumed();
                Assert.That(BattleCentralRenderSystem.CurrentPixelFramePlan.World, Is.SameAs(nextWorld));
            }
            finally
            {
                nextWorld.ResetRuntimeState();
            }
        }

        [Test]
        public void WarmHost_AllocatedBytesRemainZeroAfterWarmup()
        {
            world.BattlePresentation.BeginFrame(world, 702);
            for (int index = 0; index < 16; index++)
                Present(world, true);
            long begin = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 64; index++)
                Present(world, true);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - begin;
            Assert.That(allocated, Is.Zero);
            Assert.That(ReadInt("lastMaterializedPublicationVersion"), Is.EqualTo(materializedVersion));
        }

        private static Action<SimulationWorld, bool> BindPresent()
        {
            MethodInfo method = typeof(BattleCentralRenderSystem).GetMethod("PresentLatestPublishedFrame", StaticFlags);
            if (method != null)
                return (Action<SimulationWorld, bool>)method.CreateDelegate(typeof(Action<SimulationWorld, bool>));
            // Test-first: exercise the actual pre-change production Flush rather than fail compilation.
            return (value, interactive) => BattleCentralRenderSystem.FlushLatestPublishedFrame(value);
        }

        private static object GetField(string name) =>
            typeof(BattleCentralRenderSystem).GetField(name, StaticFlags).GetValue(null);

        private static int ReadInt(string name) => (int)GetField(name);

        private static void SetField(string name, object value) =>
            typeof(BattleCentralRenderSystem).GetField(name, StaticFlags).SetValue(null, value);

        private static void AssertConsumed() =>
            Assert.That(ReadInt("lastMaterializedPublicationVersion"), Is.EqualTo(ReadInt("pendingPublicationVersion")));
    }
}
#endif
