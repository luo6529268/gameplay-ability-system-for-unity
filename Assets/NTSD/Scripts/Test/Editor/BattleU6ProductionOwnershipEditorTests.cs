#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering.Editor;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleU6ProductionOwnershipEditorTests
    {
        private static readonly Type[] BattleEntityTypes =
        {
            typeof(LF2Entity),
            typeof(LF2LivingObject),
            typeof(LF2Character),
            typeof(LF2WeaponBase),
            typeof(LF2Weapon),
            typeof(LF2SpecialAttack),
            typeof(LF2OtherObject),
        };

        [Test]
        public void ProductionPassDefaults_MatchPromotedOwnershipAndMeasuredOracles()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsCooldownPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCooldownPassMode.DataOriented));
            Assert.That(
                world.BattleEcsCharacterStageZPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterStageZPassMode.DataOriented));
            Assert.That(
                world.BattleEcsCharacterPreFrameBoundsPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented));
            Assert.That(
                world.BattleEcsCharacterFrameAdvancePassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterFrameAdvancePassMode.DataOriented));
            Assert.That(
                world.BattleEcsCharacterRecoveryPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterRecoveryPassMode.DataOriented));
            Assert.That(
                world.BattleEcsCharacterFrameTickPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterFrameTickPassMode.DataOriented));
            Assert.That(
                world.BattleEcsCharacterInputPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterInputPassMode.DataOriented));
            Assert.That(
                world.BattleEcsPositiveLinkValidationPassModeForDiagnostics,
                Is.EqualTo(BattleEcsPositiveLinkValidationPassMode.DataOriented));

            Assert.That(
                world.BattleEcsFramePostProcessPassModeForDiagnostics,
                Is.EqualTo(BattleEcsFramePostProcessPassMode.Legacy),
                "The measured DataOriented candidate regressed P95 and remains an oracle.");
            Assert.That(
                world.BattleEcsCharacterPostFrameTailPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterPostFrameTailPassMode.Legacy),
                "The measured candidate did not pass its production performance gate.");
        }

        [Test]
        public void BattleEntityTypes_ArePureCSharpAndDeclareNoUnityPerEntityLoop()
        {
            string[] forbiddenMessages = { "Update", "FixedUpdate", "LateUpdate" };

            for (int typeIndex = 0; typeIndex < BattleEntityTypes.Length; typeIndex++)
            {
                Type type = BattleEntityTypes[typeIndex];
                Assert.That(
                    typeof(MonoBehaviour).IsAssignableFrom(type),
                    Is.False,
                    type.FullName + " must remain a pure C# battle shell.");

                for (int messageIndex = 0;
                     messageIndex < forbiddenMessages.Length;
                     messageIndex++)
                {
                    MethodInfo method = type.GetMethod(
                        forbiddenMessages[messageIndex],
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly);
                    Assert.That(
                        method,
                        Is.Null,
                        type.FullName + " must not own a Unity per-entity loop named " +
                        forbiddenMessages[messageIndex] + ".");
                }
            }
        }

        [Test]
        public void ProductionOwnershipInventory_EnumeratesEveryExitDispositionOnce()
        {
            var inventory = new BattleProductionOwnershipInventory();
            int domainCount =
                Enum.GetValues(typeof(BattleProductionOwnershipDomain)).Length;
            var seen = new bool[domainCount];
            int canonicalCount = 0;
            int retainedOracleCount = 0;
            int compatibilityShellCount = 0;

            Assert.That(inventory.Count, Is.EqualTo(seen.Length));
            for (int i = 0; i < inventory.Count; i++)
            {
                BattleProductionOwnershipEntry entry = inventory.GetEntry(i);
                int domainIndex = (int)entry.Domain - 1;
                Assert.That(domainIndex, Is.InRange(0, seen.Length - 1));
                Assert.That(seen[domainIndex], Is.False, entry.Domain.ToString());
                seen[domainIndex] = true;
                switch (entry.Disposition)
                {
                    case BattleProductionOwnershipDisposition.WorldCanonical:
                        canonicalCount++;
                        Assert.That(
                            entry.Reason,
                            Is.EqualTo(
                                BattleProductionOwnershipReason
                                    .CanonicalWorldStoreAndWriter));
                        break;
                    case BattleProductionOwnershipDisposition.RetainedMeasuredOracle:
                        retainedOracleCount++;
                        Assert.That(
                            entry.Reason,
                            Is.EqualTo(
                                    BattleProductionOwnershipReason
                                        .PerformanceGateRejectedCandidate)
                                .Or.EqualTo(
                                    BattleProductionOwnershipReason
                                        .ParityGateRejectedCandidate));
                        break;
                    case BattleProductionOwnershipDisposition.UnityCompatibilityShell:
                        compatibilityShellCount++;
                        Assert.That(
                            entry.Reason,
                            Is.EqualTo(
                                BattleProductionOwnershipReason
                                    .UnityHostOrDerivedCompatibility));
                        break;
                    default:
                        Assert.Fail("Unknown U6 ownership disposition: " + entry.Disposition);
                        break;
                }
            }

            Assert.That(
                canonicalCount,
                Is.EqualTo(BattleProductionOwnershipInventory.ExpectedCanonicalOwnerCount));
            Assert.That(
                retainedOracleCount,
                Is.EqualTo(
                    BattleProductionOwnershipInventory
                        .ExpectedRetainedMeasuredOracleCount));
            Assert.That(
                compatibilityShellCount,
                Is.EqualTo(
                    BattleProductionOwnershipInventory
                        .ExpectedUnityCompatibilityShellCount));
        }

        [Test]
        public void ProductionOwnershipConfiguration_AcceptsPromotedProductionProfile()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.DataOrientedCanonical);
            var inventory = new BattleProductionOwnershipInventory();

            BattleProductionOwnershipConfiguration result = inventory.Evaluate(world);

            Assert.That(result.Passed, Is.True, result.Failure.ToString());
            Assert.That(
                result.CanonicalOwnerCount,
                Is.EqualTo(BattleProductionOwnershipInventory.ExpectedCanonicalOwnerCount));
            Assert.That(
                result.RetainedMeasuredOracleCount,
                Is.EqualTo(
                    BattleProductionOwnershipInventory
                        .ExpectedRetainedMeasuredOracleCount));
            Assert.That(
                result.UnityCompatibilityShellCount,
                Is.EqualTo(
                    BattleProductionOwnershipInventory
                        .ExpectedUnityCompatibilityShellCount));
        }

        [Test]
        public void ProductionOwnershipConfiguration_RejectsLegacyAiProfile()
        {
            var world = new SimulationWorld();
            var inventory = new BattleProductionOwnershipInventory();

            BattleProductionOwnershipConfiguration result = inventory.Evaluate(world);

            Assert.That(result.Passed, Is.False);
            Assert.That(
                result.Failure,
                Is.EqualTo(BattleProductionOwnershipFailure.AiExecutionProfile));
        }

        [Test]
        public void Request_PreservesExplicitU6ProductionOwnershipGate()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "combat1000",
                aiExecutionProfile = "data-oriented-canonical",
                requireU6ProductionOwnershipAudit = true,
            };

            ProductionEntityStressConfig config =
                ProductionEntityStressConfig.FromRequest(request, Environment.CurrentDirectory);

            Assert.That(config.RequireU6ProductionOwnershipAudit, Is.True);
        }
    }
}
#endif
