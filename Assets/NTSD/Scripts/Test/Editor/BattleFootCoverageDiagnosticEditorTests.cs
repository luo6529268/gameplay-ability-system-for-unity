#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class BattleFootCoverageDiagnosticEditorTests
    {
        [TestCase(false, false, false, 0, 0, "NO_AUTHORING_AND_SELF_FLAG_UNOBSERVED")]
        [TestCase(false, false, false, 1, 0, "NO_LOADED_AUTHORING")]
        [TestCase(true, false, true, 1, 0, "RUNTIME_DISABLED")]
        [TestCase(true, true, false, 1, 0, "RUNTIME_SPRITE_UNAVAILABLE")]
        [TestCase(true, true, true, 0, 0, "SELF_FLAG_UNOBSERVED")]
        [TestCase(true, true, true, 1, 0, "FOOT_BACKEND_EMPTY")]
        [TestCase(true, true, true, 1, 1, "ACTIVITY_OBSERVED_NOT_CERTIFIED")]
        public void Observations_KeepIndependentPrerequisitesAndDoNotCertifyPerformance(
            bool authoringAvailable, bool runtimeEnabled, bool runtimeSpriteAvailable,
            int selfCommands, int activeMarkers, string expected)
        {
            Assert.That(BattleCentralProductionWindowSceneProbeEditor.ClassifyFootCoverage(
                authoringAvailable, runtimeEnabled, runtimeSpriteAvailable, selfCommands, activeMarkers),
                Is.EqualTo(expected));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void NegativeCounts_AreRejected(int selfCommands, int activeMarkers)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleCentralProductionWindowSceneProbeEditor.ClassifyFootCoverage(
                    true, true, true, selfCommands, activeMarkers));
        }
    }
}
#endif
