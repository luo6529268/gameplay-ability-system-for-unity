#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralLatestFrameMaterializationEditorTests
    {
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
        public void CentralSubmission_PrepareCapacity_PrewarmsFrozenFrameStorage()
        {
            const int entityCapacity = 1000;
            int hitRecordCapacity = checked(
                entityCapacity *
                NTSD.Animation.LF2Objects.LF2Entity.MaxHitRecordSlots);
            MethodInfo calculateCommandCapacity =
                typeof(BattlePresentationCoordinator).GetMethod(
                    "CalculateMaximumCommandCapacity",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(calculateCommandCapacity, Is.Not.Null);
            int commandCapacity = (int)calculateCommandCapacity.Invoke(
                null,
                new object[] { entityCapacity });
            var meshBackend = new BattleDynamicMeshBackend();
            var footMarkerBackend = new BattleFootMarkerBatchBackend();
            var healthBackend = new BattleHealthBarBatchBackend();
            ConstructorInfo submissionConstructor =
                typeof(BattleCentralSubmission).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleDynamicMeshBackend),
                        typeof(BattleFootMarkerBatchBackend),
                        typeof(BattleHealthBarBatchBackend),
                    },
                    null);
            Assert.That(submissionConstructor, Is.Not.Null);
            var submission = (BattleCentralSubmission)submissionConstructor.Invoke(
                new object[] { meshBackend, footMarkerBackend, healthBackend });
            MethodInfo prepareCapacity = typeof(BattleCentralSubmission).GetMethod(
                "PrepareCapacity",
                BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo captureFrame = typeof(BattleCentralSubmission).GetMethod(
                "CaptureFrame",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(prepareCapacity, Is.Not.Null);
            Assert.That(captureFrame, Is.Not.Null);

            prepareCapacity.Invoke(
                submission,
                new object[]
                {
                    entityCapacity,
                    hitRecordCapacity,
                    commandCapacity,
                });
            var source = new BattlePresentationFrame();
            var captured = (BattlePresentationFrame)captureFrame.Invoke(
                submission,
                new object[] { source, null });

            Assert.That(captured.EntityCapacity, Is.GreaterThanOrEqualTo(entityCapacity));
            Assert.That(captured.HitRecordCapacity, Is.GreaterThanOrEqualTo(hitRecordCapacity));
            Assert.That(captured.CommandCapacity, Is.GreaterThanOrEqualTo(commandCapacity));
            meshBackend.Dispose();
            footMarkerBackend.Dispose();
            healthBackend.Dispose();
        }

        [Test]
        public void CatchUpPublications_MaterializeOnlyLatestSnapshot()
        {
            var world = CreateLegacyWorld();

            Publish(world, 101);
            Publish(world, 102);
            Publish(world, 103);

            BattlePixelFramePlan plan =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(700);

            Assert.That(plan.IsValid, Is.True);
            Assert.That(plan.SimulationTick, Is.EqualTo(103));
            Assert.That(plan.DisplayTick, Is.EqualTo(103));
            Assert.That(
                BattleCentralRenderSystem.LastMaterializedPublishedTickForDiagnostics,
                Is.EqualTo(103));
        }

        [Test]
        public void CentralOnly_CapturesBeforeMaterializingCommands()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);

            world.BattlePresentation.BeginFrame(world, 91);
            BattlePresentationFrame captured = world.BattlePresentation.PublishedFrame;

            Assert.That(captured, Is.Not.Null);
            Assert.That(captured.TickIndex, Is.EqualTo(91));
            Assert.That(captured.PresentationOrderMaterialized, Is.False,
                "the logic observation point must not sort presentation data");
            Assert.That(captured.CommandsMaterialized, Is.False,
                "the logic observation point must publish data without building render commands");

            BattlePixelFramePlan plan =
                BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);

            Assert.That(plan.IsValid, Is.True);
            Assert.That(plan.CapturedFrame, Is.Not.Null);
            Assert.That(plan.CapturedFrame.PresentationOrderMaterialized, Is.True);
            Assert.That(plan.CapturedFrame.CommandsMaterialized, Is.True);
            Assert.That(captured.CommandsMaterialized, Is.False,
                "the immutable logic publication must not be mutated by the presentation host");
        }

        [Test]
        public void CentralIntermediateTick_SkipsPublicationAndRendererShellLatePass()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);

            world.RenderDispatchAll(101);
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            int lateUpdateCount = world.LateRendererUpdateInvocationCountForDiagnostics;
            long bypassCount = world.CentralOnlyRendererShellBypassCountForDiagnostics;

            world.RenderDispatchAll(102, buildPresentation: false);

            Assert.That(world.BattlePresentation.PublishedFrame, Is.SameAs(published));
            Assert.That(world.BattlePresentation.PublishedFrame.TickIndex, Is.EqualTo(101));
            Assert.That(
                world.LateRendererUpdateInvocationCountForDiagnostics,
                Is.EqualTo(lateUpdateCount));
            Assert.That(
                world.CentralOnlyRendererShellBypassCountForDiagnostics,
                Is.EqualTo(bypassCount + 1));

            world.RenderDispatchAll(103, buildPresentation: true);

            Assert.That(world.BattlePresentation.PublishedFrame, Is.Not.SameAs(published));
            Assert.That(world.BattlePresentation.PublishedFrame.TickIndex, Is.EqualTo(103));
            Assert.That(
                world.LateRendererUpdateInvocationCountForDiagnostics,
                Is.EqualTo(lateUpdateCount));
            Assert.That(
                world.CentralOnlyRendererShellBypassCountForDiagnostics,
                Is.EqualTo(bypassCount + 2));
        }

        [TestCase(BattlePresentationBackendMode.LegacyOnly)]
        [TestCase(BattlePresentationBackendMode.CentralShadowBuild)]
        public void NonCentralOnlyModes_IgnorePresentationSuppression(
            BattlePresentationBackendMode mode)
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(mode);

            world.RenderDispatchAll(201, buildPresentation: false);

            Assert.That(world.BattlePresentation.PublishedFrame, Is.Not.Null);
            Assert.That(world.BattlePresentation.PublishedFrame.TickIndex, Is.EqualTo(201));
            Assert.That(world.LateRendererUpdateInvocationCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void SuppressedCatchUp_FinalSnapshotMatchesSingleFinalBuild()
        {
            var catchUpWorld = new SimulationWorld();
            catchUpWorld.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            catchUpWorld.RenderDispatchAll(301, buildPresentation: false);
            catchUpWorld.RenderDispatchAll(302, buildPresentation: false);
            catchUpWorld.RenderDispatchAll(303, buildPresentation: false);
            catchUpWorld.RenderDispatchAll(304, buildPresentation: true);
            BattlePresentationFrame catchUpFrame =
                catchUpWorld.BattlePresentation.PublishedFrame;

            BattleCentralRenderSystem.ResetRuntime();
            var directWorld = new SimulationWorld();
            directWorld.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            directWorld.RenderDispatchAll(304);
            BattlePresentationFrame directFrame =
                directWorld.BattlePresentation.PublishedFrame;

            Assert.That(catchUpFrame.TickIndex, Is.EqualTo(directFrame.TickIndex));
            Assert.That(catchUpFrame.EntityCount, Is.EqualTo(directFrame.EntityCount));
            Assert.That(catchUpFrame.HitRecordCount, Is.EqualTo(directFrame.HitRecordCount));
            Assert.That(catchUpFrame.CommandCount, Is.EqualTo(directFrame.CommandCount));
            Assert.That(
                catchUpWorld.LateRendererUpdateInvocationCountForDiagnostics,
                Is.Zero);
            Assert.That(
                directWorld.LateRendererUpdateInvocationCountForDiagnostics,
                Is.Zero);
            Assert.That(
                catchUpWorld.CentralOnlyRendererShellBypassCountForDiagnostics,
                Is.EqualTo(4));
            Assert.That(
                directWorld.CentralOnlyRendererShellBypassCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void SameUnityFrame_MaterializesAtMostOnce_AndNextFrameCatchesUp()
        {
            var world = CreateLegacyWorld();
            Publish(world, 201);

            BattlePixelFramePlan first =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(800);
            int firstGeneration = first.Generation;

            Publish(world, 202);
            BattlePixelFramePlan repeated =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(800);

            Assert.That(repeated.Generation, Is.EqualTo(firstGeneration));
            Assert.That(repeated.SimulationTick, Is.EqualTo(201));
            Assert.That(
                BattleCentralRenderSystem.PendingPublishedTickForDiagnostics,
                Is.EqualTo(202));

            BattlePixelFramePlan nextFrame =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(801);
            Assert.That(nextFrame.Generation, Is.Not.EqualTo(firstGeneration));
            Assert.That(nextFrame.SimulationTick, Is.EqualTo(202));
        }

        [Test]
        public void RepeatedCameraSubmissionWithoutNewPublication_DoesNotRebuild()
        {
            var world = CreateLegacyWorld();
            Publish(world, 301);

            BattlePixelFramePlan first =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(900);
            BattlePixelFramePlan sameFrame =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(900);
            BattlePixelFramePlan laterFrame =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(901);

            Assert.That(sameFrame.Generation, Is.EqualTo(first.Generation));
            Assert.That(laterFrame.Generation, Is.EqualTo(first.Generation));
            Assert.That(laterFrame.SimulationTick, Is.EqualTo(301));
        }

        [Test]
        public void ExplicitFlush_BypassesUnityFrameGateAndMaterializesLatestSnapshot()
        {
            var world = CreateLegacyWorld();
            Publish(world, 401);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1000);

            Publish(world, 402);
            BattlePixelFramePlan flushed =
                BattleCentralRenderSystem.FlushLatestPublishedFrame(world);

            Assert.That(flushed.IsValid, Is.True);
            Assert.That(flushed.SimulationTick, Is.EqualTo(402));
            Assert.That(flushed.DisplayTick, Is.EqualTo(402));
        }

        [Test]
        public void ResetRuntime_DropsPendingWorldAndPreventsStaleMaterialization()
        {
            var world = CreateLegacyWorld();
            Publish(world, 501);

            BattleCentralRenderSystem.ResetRuntime();
            BattlePixelFramePlan plan =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1100);

            Assert.That(plan.IsValid, Is.False);
            Assert.That(
                BattleCentralRenderSystem.PendingPublishedTickForDiagnostics,
                Is.EqualTo(-1));
            Assert.That(
                BattleCentralRenderSystem.LastMaterializedPublishedTickForDiagnostics,
                Is.EqualTo(-1));
        }

        [Test]
        public void WorldSwitchAfterMaterialization_DoesNotSubmitPreviousWorldInSameUnityFrame()
        {
            var firstWorld = CreateLegacyWorld();
            Publish(firstWorld, 601);
            BattlePixelFramePlan first =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1200);
            Assert.That(first.World, Is.SameAs(firstWorld));

            var nextWorld = CreateLegacyWorld();
            Publish(nextWorld, 1);
            BattlePixelFramePlan blockedSameFrame =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1200);

            Assert.That(blockedSameFrame.IsValid, Is.False);
            Assert.That(firstWorld.CurrentPixelFramePlan.IsValid, Is.False);

            BattlePixelFramePlan nextFrame =
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1201);
            Assert.That(nextFrame.World, Is.SameAs(nextWorld));
            Assert.That(nextFrame.SimulationTick, Is.EqualTo(1));
        }

        [Test]
        public void CamerasCannotAcquireOldSubmissionUntilLatestPublicationIsMaterialized()
        {
            Camera previousWorldCamera = NTSDRenderSpace.WorldCamera;
            GameObject worldCameraObject = null;
            GameObject sceneViewCameraObject = null;
            GameObject otherCameraObject = null;
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            try
            {
                worldCameraObject = new GameObject("LatestFrameGate_WorldCamera");
                Camera worldCamera = worldCameraObject.AddComponent<Camera>();
                sceneViewCameraObject = new GameObject("LatestFrameGate_SceneViewCamera");
                Camera sceneViewCamera = sceneViewCameraObject.AddComponent<Camera>();
                otherCameraObject = new GameObject("LatestFrameGate_OtherCamera");
                Camera otherCamera = otherCameraObject.AddComponent<Camera>();
                NTSDRenderSpace.BindWorldCamera(worldCamera);

                world.BattlePresentation.BeginFrame(world, 701);
                BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1300);
                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        worldCamera,
                        CameraRenderType.Base,
                        worldCamera.cameraType,
                        false,
                        out BattleCentralSubmission.BattleCentralSubmissionLease initialLease),
                    Is.True);
                initialLease.Dispose();

                world.BattlePresentation.BeginFrame(world, 702);
                BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);

                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        worldCamera,
                        CameraRenderType.Base,
                        worldCamera.cameraType,
                        false,
                        out _),
                    Is.False,
                    "the real world camera must not acquire the previous submission while a newer publication is pending");
                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        sceneViewCamera,
                        CameraRenderType.Base,
                        CameraType.SceneView,
                        true,
                        out _),
                    Is.False,
                    "a SceneView camera arriving before the world camera must not acquire stale pixels");
                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        otherCamera,
                        CameraRenderType.Base,
                        otherCamera.cameraType,
                        false,
                        out _),
                    Is.False,
                    "an unrelated camera arriving before the world camera must not acquire stale pixels");

                BattlePixelFramePlan latest =
                    BattleCentralRenderSystem.PublishReadyCentralPlanForSelfCheck(world);
                Assert.That(latest.SimulationTick, Is.EqualTo(702));
                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        worldCamera,
                        CameraRenderType.Base,
                        worldCamera.cameraType,
                        false,
                        out _),
                    Is.False,
                    "publishing geometry alone must not bypass the display-frame materialization gate");

                BattleCentralRenderSystem.MaterializeLatestPublishedFrameForSelfCheck(1301);
                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        sceneViewCamera,
                        CameraRenderType.Base,
                        CameraType.SceneView,
                        true,
                        out BattleCentralSubmission.BattleCentralSubmissionLease sceneViewLease),
                    Is.True,
                    "a Play Mode SceneView may acquire only after the world-camera materialization gate has accepted the latest publication");
                Assert.That(sceneViewLease.TickIndex, Is.EqualTo(702));
                sceneViewLease.Dispose();
                Assert.That(
                    BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                        worldCamera,
                        CameraRenderType.Base,
                        worldCamera.cameraType,
                        false,
                        out BattleCentralSubmission.BattleCentralSubmissionLease latestLease),
                    Is.True);
                Assert.That(latestLease.TickIndex, Is.EqualTo(702));
                latestLease.Dispose();
            }
            finally
            {
                NTSDRenderSpace.ClearBoundWorldCamera(
                    worldCameraObject != null ? worldCameraObject.GetComponent<Camera>() : null);
                if (previousWorldCamera != null)
                    NTSDRenderSpace.BindWorldCamera(previousWorldCamera);
                if (worldCameraObject != null)
                    Object.DestroyImmediate(worldCameraObject);
                if (sceneViewCameraObject != null)
                    Object.DestroyImmediate(sceneViewCameraObject);
                if (otherCameraObject != null)
                    Object.DestroyImmediate(otherCameraObject);
                world.ResetRuntimeState();
            }
        }

        private static SimulationWorld CreateLegacyWorld()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.LegacyOnly);
            return world;
        }

        private static void Publish(SimulationWorld world, int tickIndex)
        {
            world.BattlePresentation.BeginFrame(world, tickIndex);
            BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
        }
    }
}
#endif
