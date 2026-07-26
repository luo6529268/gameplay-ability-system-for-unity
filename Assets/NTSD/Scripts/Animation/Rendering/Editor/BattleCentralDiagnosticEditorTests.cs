#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCentralDiagnosticEditorTests
    {
        [Test]
        public void Serialize_IsDeterministicAndContainsEntityAndRenderingReport()
        {
            BattleCentralEntityDiagnostic entity =
                BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                    null,
                    17,
                    BattleRenderCommandType.Shadow);
            var capabilities = new BattleRenderingDeviceCapabilities(
                "Test GPU",
                "Test Device",
                "Test API",
                true,
                4096,
                256,
                true,
                true,
                64L * 1024L * 1024L);
            BattleAtlasPolicyDecision atlasDecision = BattleRenderingPolicyResolver.ResolveAtlas(
                capabilities,
                1,
                new string[0],
                nameof(BattleAtlasPolicyMode.TextureArray));
            var atlas = new BattleAtlasDiagnosticInputs(
                capabilities,
                atlasDecision,
                1,
                BattleAtlasDiagnosticInputs.EstimateAtlasBytes(1),
                BattleSpriteCentralBindingMode.AtlasTextureArray,
                "deterministic test");
            var draw = new BattleDrawPolicyDecision(
                BattleDrawPolicyMode.OrderedChunks,
                BattleCentralDrawMode.OrderedChunks,
                string.Empty);
            var report = new BattleRenderingDiagnosticReport(
                atlas,
                draw,
                4,
                3,
                1,
                0,
                1,
                2,
                2,
                BattlePresentationBackendMode.CentralOnly,
                BattlePresentationBackendMode.CentralOnly,
                2,
                generation: 9,
                buildTick: 31,
                simulationTick: 33,
                displayTick: 31,
                isStale: true,
                refusalReason: "test refusal",
                submissionBuildCurrent: true,
                unsupportedRenderStateCount: 1,
                firstUnresolvedCommandIndex: 3,
                firstUnresolvedCommandType: BattleRenderCommandType.Entity,
                firstUnresolvedStatus: BattleCentralResourceStatus.UnsupportedRenderState);

            string first = BattleCentralDiagnosticExporter.Serialize(
                17,
                BattleRenderCommandType.Shadow,
                entity,
                report);
            string second = BattleCentralDiagnosticExporter.Serialize(
                17,
                BattleRenderCommandType.Shadow,
                entity,
                report);

            Assert.That(second, Is.EqualTo(first));
            StringAssert.StartsWith(
                "{\"schema\":\"ntsd-central-render-diagnostic-v1\",\"requestedRuntimeSlot\":17",
                first);
            StringAssert.Contains("\"requestedCommandType\":\"Shadow\"", first);
            StringAssert.Contains("\"entityDiagnostic\":{", first);
            StringAssert.Contains("\"atlasPageIndex\":-1", first);
            StringAssert.Contains("\"frameId\":-1", first);
            StringAssert.Contains("\"reason\":\"InvalidRuntimeHandle\"", first);
            StringAssert.Contains("\"renderingReportAvailable\":true", first);
            StringAssert.Contains("\"renderingReport\":{\"requestedAtlasMode\":", first);
            StringAssert.Contains("\"generation\":9", first);
            StringAssert.Contains("\"buildTick\":31", first);
            StringAssert.Contains("\"simulationTick\":33", first);
            StringAssert.Contains("\"displayTick\":31", first);
            StringAssert.Contains("\"firstUnresolvedCommandIndex\":3", first);
            StringAssert.Contains("\"firstUnresolvedStatus\":\"UnsupportedRenderState\"", first);
            StringAssert.Contains("\"refusalReason\":\"test refusal\"", first);

            string withoutReport = BattleCentralDiagnosticExporter.Serialize(
                17,
                BattleRenderCommandType.Shadow,
                entity,
                null);
            StringAssert.Contains("\"renderingReportAvailable\":false", withoutReport);
            StringAssert.EndsWith("\"renderingReport\":null}", withoutReport);
        }
    }
}
#endif
