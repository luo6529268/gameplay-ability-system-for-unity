#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class LateRuntimeSnapshotBoundaryEditorTests
    {
        [Test]
        public void NormalWorldDefaultsToConsolidatedFinal()
        {
            var world = new SimulationWorld();
            Assert.That(
                world.LateRuntimeSnapshotModeForDiagnostics,
                Is.EqualTo(BattleLateRuntimeSnapshotMode.ConsolidatedFinal));
        }

        [Test]
        public void RecoveryHpZeroStillReachesFrameAndDeath()
        {
            int[] result = Capture(mode: 0);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.EqualTo(1));
            Assert.That(result[6], Is.EqualTo(1));
            Assert.That(result[7], Is.EqualTo(1));
            Assert.That(result[8], Is.Zero);
            Assert.That(result[9], Is.EqualTo(1));
            Assert.That(result[10], Is.Zero);
        }

        [Test]
        public void SuppressedFrameTickUsesSinglePostBranchSnapshot()
        {
            int[] result = Capture(mode: 1);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.EqualTo(1));
            Assert.That(result[7], Is.Zero);
        }

        [Test]
        public void CompletedCleanupKeepsContinueBoundaryWithoutSnapshot()
        {
            int[] result = Capture(mode: 2);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.Zero);
            Assert.That(result[11], Is.EqualTo(1));
            Assert.That(result[12], Is.Zero);
        }

        [Test]
        public void DepletedWeaponStillPlainFreesWithoutCleanupSnapshot()
        {
            int[] result = Capture(mode: 3);
            AssertRemovedStages(result);
            Assert.That(result[3], Is.EqualTo(1));
            Assert.That(result[4], Is.EqualTo(1));
            Assert.That(result[5], Is.Zero);
            Assert.That(result[13], Is.Zero);
            Assert.That(result[14], Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(1)]
        public void ConsolidatedActivePathKeepsOnlyFinalTailSnapshot(int scenario)
        {
            int[] result = Capture(
                scenario,
                BattleLateRuntimeSnapshotMode.ConsolidatedFinal);

            Assert.That(result[3], Is.Zero);
            Assert.That(result[4], Is.Zero);
            Assert.That(result[5], Is.EqualTo(1));
            Assert.That(result[15], Is.Zero);
            Assert.That(result[16], Is.Zero);
        }

        [TestCase(2)]
        [TestCase(3)]
        public void ConsolidatedCleanupExitPathsTakeNoLateSnapshot(int scenario)
        {
            int[] result = Capture(
                scenario,
                BattleLateRuntimeSnapshotMode.ConsolidatedFinal);

            Assert.That(result[3], Is.Zero);
            Assert.That(result[4], Is.Zero);
            Assert.That(result[5], Is.Zero);
            Assert.That(result[15], Is.Zero);
            Assert.That(result[16], Is.Zero);
        }

        [TestCase(4)]
        [TestCase(5)]
        public void ConsolidatedFrameExitPublishesResetFrameImmediately(int scenario)
        {
            int[] result = Capture(
                scenario,
                BattleLateRuntimeSnapshotMode.ConsolidatedFinal);

            Assert.That(result[3], Is.Zero);
            Assert.That(result[4], Is.Zero);
            Assert.That(result[5], Is.Zero);
            Assert.That(result[15], Is.EqualTo(1));
            Assert.That(result[17], Is.Zero);
        }

        [Test]
        public void ConsolidatedRecoveryPreservesHpObservationAtLaterPhases()
        {
            int[] legacy = Capture(
                0,
                BattleLateRuntimeSnapshotMode.LegacyThree);
            int[] consolidated = Capture(
                0,
                BattleLateRuntimeSnapshotMode.ConsolidatedFinal);

            Assert.That(consolidated[8], Is.EqualTo(legacy[8]));
            Assert.That(consolidated[10], Is.EqualTo(legacy[10]));
            Assert.That(consolidated[8], Is.Zero);
            Assert.That(consolidated[10], Is.Zero);
        }

        [Test]
        public void StressRequestDefaultsToConsolidatedAndRetainsLegacyOracle()
        {
            var missingRequest = new ProductionEntityStressRequest
            {
                action = "smoke",
            };
            var emptyRequest = new ProductionEntityStressRequest
            {
                action = "smoke",
                lateRuntimeSnapshotMode = string.Empty,
            };
            var legacyRequest = new ProductionEntityStressRequest
            {
                action = "smoke",
                lateRuntimeSnapshotMode = "legacy-three",
            };

            ProductionEntityStressConfig missing =
                ProductionEntityStressConfig.FromRequest(missingRequest, ".");
            ProductionEntityStressConfig empty =
                ProductionEntityStressConfig.FromRequest(emptyRequest, ".");
            ProductionEntityStressConfig legacy =
                ProductionEntityStressConfig.FromRequest(legacyRequest, ".");

            Assert.That(
                missing.LateRuntimeSnapshotMode,
                Is.EqualTo(BattleLateRuntimeSnapshotMode.ConsolidatedFinal));
            Assert.That(
                empty.LateRuntimeSnapshotMode,
                Is.EqualTo(BattleLateRuntimeSnapshotMode.ConsolidatedFinal));
            Assert.That(
                legacy.LateRuntimeSnapshotMode,
                Is.EqualTo(BattleLateRuntimeSnapshotMode.LegacyThree));
        }

        [Test]
        public void StressFactoryRequestsDefaultToConsolidatedFinal()
        {
            ProductionEntityStressRequest menuRequest =
                ProductionEntityStressWindow.CreateDefaultRequest(
                    "dispersed",
                    "Temp/late-snapshot-default.json");
            ProductionEntityStressRequest aiSmokeRequest =
                ProductionEntityStressWindow
                    .CreateDispersedAiSimulationOnlySmokeRequest(
                        1000,
                        "Temp/late-snapshot-ai-smoke.json");

            Assert.That(
                menuRequest.lateRuntimeSnapshotMode,
                Is.EqualTo("consolidated-final"));
            Assert.That(
                aiSmokeRequest.lateRuntimeSnapshotMode,
                Is.EqualTo("consolidated-final"));
        }

        [TestCase("legacy")]
        [TestCase("legacy-three")]
        public void StressRequestRetainsExplicitLegacyAliases(string mode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                lateRuntimeSnapshotMode = mode,
            };

            ProductionEntityStressConfig config =
                ProductionEntityStressConfig.FromRequest(request, ".");

            Assert.That(
                config.LateRuntimeSnapshotMode,
                Is.EqualTo(BattleLateRuntimeSnapshotMode.LegacyThree));
        }

        private static void AssertRemovedStages(int[] result)
        {
            Assert.That(result[0], Is.Zero);
            Assert.That(result[1], Is.Zero);
            Assert.That(result[2], Is.Zero);
        }

        private static int[] Capture(int mode)
        {
            var world = new SimulationWorld();
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "CaptureLateRuntimeSnapshotBoundaryForSelfCheck",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (int[])method.Invoke(world, new object[] { mode });
        }

        private static int[] Capture(
            int mode,
            BattleLateRuntimeSnapshotMode snapshotMode)
        {
            var world = new SimulationWorld();
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "CaptureLateRuntimeSnapshotBoundaryForModeForSelfCheck",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (int[])method.Invoke(
                world,
                new object[] { mode, (int)snapshotMode });
        }
    }
}
#endif
