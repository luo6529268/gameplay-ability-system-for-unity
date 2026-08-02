#if UNITY_EDITOR
using System;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleAiExecutionProfileEditorTests
    {
        [Test]
        public void Resolver_CommandLineOverridesConfigAndDefaultIsLegacyOnEveryPlatform()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            try
            {
                config.BattleAiExecutionProfileName = "data-oriented-canonical";
                Assert.That(
                    BattleAiExecutionProfileProductionSource.Resolve(
                        config,
                        new[]
                        {
                            BattleAiExecutionProfileResolver.ProfileArgument,
                            "legacy",
                        }),
                    Is.EqualTo(BattleAiExecutionProfile.LegacyCanonical));
                Assert.That(
                    BattleAiExecutionProfileProductionSource.Resolve(config, Array.Empty<string>()),
                    Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
                config.BattleAiExecutionProfileName = string.Empty;
                Assert.That(
                    BattleAiExecutionProfileProductionSource.Resolve(config, Array.Empty<string>()),
                    Is.EqualTo(BattleAiExecutionProfile.LegacyCanonical));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void World_AtomicProfilesMapToCoherentTriplesAndCanSwitchWhileEmpty()
        {
            var world = new SimulationWorld();

            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            Assert.That(world.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Assert.That(world.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.IndexedCanonical));
            Assert.That(world.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.UnifiedAuthority));
            Assert.That(world.AiDecisionShadowMode, Is.EqualTo(AiDecisionShadowMode.Disabled));
            Assert.That(world.AiUnifiedSnapshotShadowMode,
                Is.EqualTo(AiUnifiedSnapshotShadowMode.Disabled));
            Assert.That(world.AiDecisionIndexedCanonicalFullOracleSampleInterval, Is.Zero);

            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.LegacyCanonical);
            Assert.That(world.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.LegacyCanonical));
            Assert.That(world.AiSensingMode, Is.EqualTo(AiSensingMode.LegacyAiSensing));
            Assert.That(world.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.Legacy));
            Assert.That(world.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.LegacySeparate));
        }

        [Test]
        public void World_RejectsUnknownAndPostRegistrationProfileChanges()
        {
            var world = new SimulationWorld();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                world.ConfigureAiExecutionProfile((BattleAiExecutionProfile)99));

            var registered = new RegisteredStub();
            world.Register(registered);
            try
            {
                InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                    world.ConfigureAiExecutionProfile(
                        BattleAiExecutionProfile.DataOrientedCanonical));
                Assert.That(exception.Message, Does.Contain("before entities are registered"));
            }
            finally
            {
                world.Unregister(registered);
            }
        }

        [Test]
        public void StressRequest_ProductionProfileMapsAtomicallyWithoutLegacyOptIn()
        {
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                new ProductionEntityStressRequest
                {
                    action = "dispersed1000",
                    entityCount = 1000,
                    aiExecutionProfile = "data-oriented-canonical",
                    outputPath = "Temp/ai-profile-data-oriented.json",
                },
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            Assert.That(config.UsesLegacyAiConfigurationCompatibility, Is.False);
            Assert.That(config.AllowUnsafeAiSoACandidate, Is.False);
            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
            Assert.That(config.AiDecisionExecutionMode,
                Is.EqualTo(AiDecisionExecutionMode.IndexedCanonical));
            Assert.That(config.AiUnifiedSnapshotExecutionMode,
                Is.EqualTo(AiUnifiedSnapshotExecutionMode.UnifiedAuthority));
        }

        [Test]
        public void StressRequest_OldCandidateJsonRemainsExplicitCompatibilityOnly()
        {
            ProductionEntityStressRequest request = JsonUtility.FromJson<ProductionEntityStressRequest>(
                "{\"action\":\"smoke\",\"aiSensingMode\":\"candidate\"," +
                "\"allowUnsafeAiSoACandidate\":true," +
                "\"outputPath\":\"Temp/legacy-ai-request.json\"}");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.UsesLegacyAiConfigurationCompatibility, Is.True);
            Assert.That(config.AiExecutionProfile,
                Is.EqualTo(BattleAiExecutionProfile.DataOrientedCanonical));
            Assert.That(config.AiSensingMode, Is.EqualTo(AiSensingMode.SoAAiSensing));
        }

        [Test]
        public void StressFingerprint_ProfileChangesImplementationButNotWorkloadIdentity()
        {
            ProductionEntityStressConfig legacy = ProductionEntityStressConfig.FromRequest(
                CreateProfileRequest("legacy"),
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressConfig dataOriented = ProductionEntityStressConfig.FromRequest(
                CreateProfileRequest("data-oriented-canonical"),
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(
                ProductionEntityStressFingerprint.BuildWorkload(dataOriented),
                Is.EqualTo(ProductionEntityStressFingerprint.BuildWorkload(legacy)));
            Assert.That(
                ProductionEntityStressFingerprint.BuildImplementationConfig(dataOriented),
                Is.Not.EqualTo(ProductionEntityStressFingerprint.BuildImplementationConfig(legacy)));
        }

        private static ProductionEntityStressRequest CreateProfileRequest(string profile)
        {
            return new ProductionEntityStressRequest
            {
                action = "dispersed1000",
                entityCount = 1000,
                warmupTicks = 30,
                sampleTicks = 60,
                seed = 0x4E545344u,
                simulationOnly = true,
                aiExecutionProfile = profile,
                outputPath = "Temp/ai-profile-fingerprint.json",
            };
        }

        private sealed class RegisteredStub : ISimObject
        {
            public int SimOrder => 0;
            public int StableId => 1;
        }
    }
}
#endif
