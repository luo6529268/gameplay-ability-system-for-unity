#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingAcceptanceEditorTests
    {
        [Test]
        public void Config_NormalizesProjectRelativeOutputAndRejectsInvalidRanges()
        {
            string root = Path.GetFullPath(Path.Combine("Temp", "P8-C-ConfigRoot"));
            var request = new BattleRenderingAcceptanceRequest
            {
                outputDirectory = "evidence",
                imageSize = 128,
                exerciseLivePool = false,
                livePoolExtraCount = 2,
            };

            BattleRenderingAcceptanceConfig config =
                BattleRenderingAcceptanceConfig.FromRequest(request, root);

            Assert.That(config.OutputDirectory, Is.EqualTo(Path.GetFullPath(Path.Combine(root, "evidence"))));
            Assert.That(config.ImageSize, Is.EqualTo(128));
            Assert.That(config.ExerciseLivePool, Is.False);
            Assert.That(config.LivePoolExtraCount, Is.EqualTo(2));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BattleRenderingAcceptanceConfig("Temp/P8-C", 32, false, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new BattleRenderingAcceptanceConfig("Temp/P8-C", 128, false, 0));
        }

        [Test]
        public void FullMatrix_IsDeterministicAndWritesNonEmptyLegacyCentralEvidence()
        {
            string output = Path.GetFullPath(Path.Combine("Temp", "P8-C-EditModeTest"));
            var config = new BattleRenderingAcceptanceConfig(output, 256, false, 1);

            BattleRenderingAcceptanceReport report =
                BattleRenderingAcceptanceHarness.Run(config);

            Assert.That(report.passed, Is.True, report.ToJson());
            Assert.That(report.generationReuse.passed, Is.True);
            Assert.That(report.generationReuse.sourceCount, Is.EqualTo(1000));
            Assert.That(report.generationReuse.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.isolatedPoolExpansion.sourceCount, Is.EqualTo(33));
            Assert.That(report.isolatedPoolExpansion.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.livePoolExpansion.available, Is.False);
            Assert.That(report.atlasArrayAndOrderedPages.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.transparentResourceInterleave.segmentCount, Is.EqualTo(3));
            Assert.That(report.categoryOcclusionOrder.resolvedCount, Is.EqualTo(4));
            Assert.That(report.categoryOcclusionOrder.nonTransparentPixels, Is.GreaterThan(0));
            Assert.That(report.chunkBoundaries.sourceCount, Is.EqualTo(4097));
            Assert.That(report.chunkBoundaries.chunkCount, Is.EqualTo(2));
            Assert.That(report.missingResourceFailClosed.nonTransparentPixels, Is.EqualTo(0));
            Assert.That(report.legacyCentralPixelParity.sourceCount, Is.GreaterThan(0));
            Assert.That(report.legacyCentralPixelParity.nonTransparentPixels, Is.GreaterThan(0));

            string firstJson = report.ToJson();
            string secondJson = report.ToJson();
            Assert.That(secondJson, Is.EqualTo(firstJson));
            StringAssert.Contains("ntsd-battle-rendering-acceptance-v1", firstJson);
            StringAssert.Contains("synthetic fixture only", report.syntheticFixtureEvidenceScope);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ReportFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.LegacyFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.CentralFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ParityDiffFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.GenerationReuseFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.IsolatedExpansionFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ArrayFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.OrderedPagesFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.AtlasDiffFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.InterleaveFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.CategoryOcclusionFileName);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.Chunk4097FileName);
        }

        [Test]
        public void RequestedProductionPath_FailsExplicitlyOutsidePlayModeAndWritesReport()
        {
            string output = Path.GetFullPath(Path.Combine("Temp", "P8-C-RequestedProductionUnavailable"));
            var config = new BattleRenderingAcceptanceConfig(output, 128, true, 1);

            BattleRenderingAcceptanceReport report =
                BattleRenderingAcceptanceHarness.Run(config);

            Assert.That(report.livePoolRequested, Is.True);
            Assert.That(report.passed, Is.False);
            Assert.That(report.livePoolExpansion.available, Is.False);
            Assert.That(report.livePoolExpansion.passed, Is.False);
            StringAssert.Contains("requested but unavailable", report.livePoolExpansion.evidence);
            Assert.That(report.productionCatalogPixelParity.available, Is.False);
            Assert.That(report.productionCatalogPixelParity.passed, Is.False);
            StringAssert.Contains("requested but unavailable", report.productionCatalogPixelParity.evidence);
            AssertArtifact(output, BattleRenderingAcceptanceHarness.ReportFileName);
        }

        [Test]
        public void ProductionBackendCounts_SumIndependentPerResourceBuilds()
        {
            var aggregate = new BattleRenderingAcceptanceCase();
            var character = new BattleRenderingProductionResourceEvidence
            {
                segmentCount = 1,
                chunkCount = 1,
            };
            var weapon = new BattleRenderingProductionResourceEvidence
            {
                segmentCount = 1,
                chunkCount = 1,
            };

            BattleRenderingAcceptanceHarness.AggregateProductionBackendCounts(
                aggregate,
                character,
                weapon);

            Assert.That(aggregate.segmentCount, Is.EqualTo(2));
            Assert.That(aggregate.chunkCount, Is.EqualTo(2));
            Assert.That(aggregate.segmentCount, Is.GreaterThan(0));
            Assert.That(aggregate.chunkCount, Is.GreaterThan(0));
        }

        [Test]
        public void PlayModeRequest_DoesNotExecuteBeforePlayAndWarmsUpOnceEntered()
        {
            var request = new BattleRenderingAcceptanceRequest
            {
                exerciseLivePool = true,
                enterPlayMode = true,
                playModeWarmupFrames = 3,
                exitPlayModeAfterRun = true,
            };

            Assert.That(
                BattleRenderingAcceptanceRequestProcessor.DecideRequestAction(
                    request,
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    completedWarmupFrames: 0),
                Is.EqualTo(BattleRenderingAcceptanceRequestProcessor.RequestAction.EnterPlayMode));
            Assert.That(
                BattleRenderingAcceptanceRequestProcessor.DecideRequestAction(
                    request,
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: true,
                    completedWarmupFrames: 0),
                Is.EqualTo(BattleRenderingAcceptanceRequestProcessor.RequestAction.WaitForPlayMode));
            Assert.That(
                BattleRenderingAcceptanceRequestProcessor.DecideRequestAction(
                    request,
                    isPlaying: true,
                    isPlayingOrWillChangePlaymode: true,
                    completedWarmupFrames: 2),
                Is.EqualTo(BattleRenderingAcceptanceRequestProcessor.RequestAction.WarmUp));
            Assert.That(
                BattleRenderingAcceptanceRequestProcessor.DecideRequestAction(
                    request,
                    isPlaying: true,
                    isPlayingOrWillChangePlaymode: true,
                    completedWarmupFrames: 3),
                Is.EqualTo(BattleRenderingAcceptanceRequestProcessor.RequestAction.Execute));

            request.playModeWarmupFrames = 0;
            Assert.That(
                BattleRenderingAcceptanceRequestProcessor.GetRequiredWarmupFrames(request),
                Is.EqualTo(BattleRenderingAcceptanceRequestProcessor.DefaultPlayModeWarmupFrames));
        }

        [Test]
        public void LegacyRequest_StillExecutesImmediatelyWithoutPlayModeTransition()
        {
            var request = new BattleRenderingAcceptanceRequest
            {
                exerciseLivePool = true,
                enterPlayMode = false,
            };

            Assert.That(
                BattleRenderingAcceptanceRequestProcessor.DecideRequestAction(
                    request,
                    isPlaying: false,
                    isPlayingOrWillChangePlaymode: false,
                    completedWarmupFrames: 0),
                Is.EqualTo(BattleRenderingAcceptanceRequestProcessor.RequestAction.Execute));
        }

        private static void AssertArtifact(string directory, string name)
        {
            string path = Path.Combine(directory, name);
            Assert.That(File.Exists(path), Is.True, path);
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0), path);
        }
    }
}
#endif
