#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingDevicePolicyEditorTests
    {
        private const long MiB = 1024L * 1024L;

        [Test]
        public void PlatformResolver_UsesDesktopBudgetForEditorAndStandalone()
        {
            BattleRenderingPlatformCategory editor =
                BattleRenderingPlatformPolicy.ResolvePlatformCategory(
                    true, false, false, false);
            BattleRenderingPlatformCategory standalone =
                BattleRenderingPlatformPolicy.ResolvePlatformCategory(
                    false, true, false, false);

            Assert.That(editor, Is.EqualTo(BattleRenderingPlatformCategory.Desktop));
            Assert.That(standalone, Is.EqualTo(BattleRenderingPlatformCategory.Desktop));
            Assert.That(
                BattleRenderingPlatformPolicy.ResolveDefaultAtlasMemoryBudgetBytes(editor),
                Is.EqualTo(512L * MiB));
            Assert.That(
                BattleRenderingPlatformPolicy.ResolveDefaultAtlasMemoryBudgetBytes(standalone),
                Is.EqualTo(512L * MiB));
        }

        [Test]
        public void PlatformResolver_UsesMobileBudgetForAndroidAndIos()
        {
            BattleRenderingPlatformCategory android =
                BattleRenderingPlatformPolicy.ResolvePlatformCategory(
                    false, false, true, false);
            BattleRenderingPlatformCategory ios =
                BattleRenderingPlatformPolicy.ResolvePlatformCategory(
                    false, false, false, true);

            Assert.That(android, Is.EqualTo(BattleRenderingPlatformCategory.Mobile));
            Assert.That(ios, Is.EqualTo(BattleRenderingPlatformCategory.Mobile));
            Assert.That(
                BattleRenderingPlatformPolicy.ResolveDefaultAtlasMemoryBudgetBytes(android),
                Is.EqualTo(256L * MiB));
            Assert.That(
                BattleRenderingPlatformPolicy.ResolveDefaultAtlasMemoryBudgetBytes(ios),
                Is.EqualTo(256L * MiB));
        }

        [Test]
        public void FromSystem_ExplicitBudgetOverridesPlatformDefault()
        {
            const long explicitBudget = 123L * MiB;

            BattleRenderingDeviceCapabilities capabilities =
                BattleRenderingDeviceCapabilities.FromSystem(explicitBudget);

            Assert.That(capabilities.AtlasMemoryBudgetBytes, Is.EqualTo(explicitBudget));
        }

        [Test]
        public void FourHundredEightyMiBPlan_IsAcceptedOnDesktopAndRejectedOnMobile()
        {
            const int pageCount = 30;
            long estimatedBytes = BattleAtlasDiagnosticInputs.EstimateAtlasBytes(pageCount);
            Assert.That(estimatedBytes, Is.EqualTo(480L * MiB));

            BattleAtlasArrayDecision desktop = CreateCapabilities(
                    BattleRenderingPlatformCategory.Desktop)
                .ToAtlasCapabilityPolicy()
                .EvaluateArray(pageCount);
            BattleAtlasArrayDecision mobile = CreateCapabilities(
                    BattleRenderingPlatformCategory.Mobile)
                .ToAtlasCapabilityPolicy()
                .EvaluateArray(pageCount);

            Assert.That(desktop.UseTextureArray, Is.True);
            Assert.That(mobile.UseTextureArray, Is.False);
            StringAssert.Contains("budget", mobile.Reason);
            StringAssert.Contains((256L * MiB).ToString(), mobile.Reason);
        }

        [Test]
        public void DiagnosticReport_ReportsActualResolvedBudgetBytes()
        {
            BattleRenderingDeviceCapabilities capabilities = CreateCapabilities(
                BattleRenderingPlatformCategory.Desktop);
            BattleAtlasPolicyDecision atlasDecision = BattleRenderingPolicyResolver.ResolveAtlas(
                capabilities,
                30,
                new string[0],
                nameof(BattleAtlasPolicyMode.Auto));
            var atlas = new BattleAtlasDiagnosticInputs(
                capabilities,
                atlasDecision,
                30,
                BattleAtlasDiagnosticInputs.EstimateAtlasBytes(30),
                BattleSpriteCentralBindingMode.AtlasTextureArray,
                string.Empty);
            var draw = new BattleDrawPolicyDecision(
                BattleDrawPolicyMode.OrderedChunks,
                BattleCentralDrawMode.OrderedChunks,
                string.Empty);
            var report = new BattleRenderingDiagnosticReport(
                atlas,
                draw,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                BattlePresentationBackendMode.CentralOnly,
                BattlePresentationBackendMode.CentralOnly);

            StringAssert.Contains(
                "\"atlasMemoryBudgetBytes\":536870912",
                report.ToJson());
        }

        private static BattleRenderingDeviceCapabilities CreateCapabilities(
            BattleRenderingPlatformCategory category)
        {
            return new BattleRenderingDeviceCapabilities(
                "Injected GPU",
                "Injected Device",
                "Injected API",
                true,
                4096,
                128,
                true,
                true,
                BattleRenderingPlatformPolicy.ResolveDefaultAtlasMemoryBudgetBytes(category));
        }
    }
}
#endif
