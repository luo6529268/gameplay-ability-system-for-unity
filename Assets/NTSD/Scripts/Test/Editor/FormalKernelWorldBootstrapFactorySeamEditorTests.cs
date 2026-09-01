#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class FormalKernelWorldBootstrapFactorySeamEditorTests
    {
        [Test]
        public void CreateWorldUsesExactBarrierSettings()
        {
            LockstepStartBarrier barrier = CreateBarrier();

            SimulationWorld world =
                InProcessBattleWorldBootstrap.CreateWorldForBarrier(barrier);

            Assert.That(world, Is.Not.Null);
            Assert.That(world.ObjectCount, Is.Zero);
            Assert.That(world.ClaimedRuntimeSlotCountForServices, Is.Zero);
            Assert.That(world.RuntimeProfileForServices,
                Is.EqualTo(barrier.WorldSettings.Profile));
            Assert.That(world.MaxRuntimeSlotsForServices,
                Is.EqualTo(barrier.WorldSettings.InitialRuntimeSlotCapacity));
            Assert.That(world.CollisionBroadphaseForServices,
                Is.EqualTo(barrier.WorldSettings.CollisionBroadphase));
        }

        [Test]
        public void PrepareWorldAppliesExactLogicOnlySeedAndCanonicalRoster()
        {
            LockstepStartBarrier barrier = CreateBarrier();
            SimulationWorld world =
                InProcessBattleWorldBootstrap.CreateWorldForBarrier(barrier);

            InProcessBattleWorldBootstrap.PrepareWorldForHost(barrier, world);

            Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.True);
            Assert.That(world.Rng.State, Is.EqualTo(barrier.Identity.Seed));
            Assert.That(world.Rng.CallCount, Is.Zero);
            Assert.That(world.Runtime.Match.Seed,
                Is.EqualTo(unchecked((int)barrier.Identity.Seed)));
            Assert.That(world.Runtime.Roster.ActiveSlotCount,
                Is.EqualTo(barrier.PlayerCount));
            for (int index = 0; index < barrier.PlayerCount; index++)
            {
                int playerSlot = barrier.CanonicalPlayerSlots[index];
                BattleSlotRuntimeState slot = world.Runtime.Roster.Slots[playerSlot];
                Assert.That(slot.Active, Is.True, $"slot={playerSlot}");
                Assert.That(slot.IsHuman, Is.True, $"slot={playerSlot}");
                Assert.That(slot.Team, Is.EqualTo(playerSlot + 1), $"slot={playerSlot}");
                Assert.That(slot.InputId, Is.EqualTo(playerSlot + 1), $"slot={playerSlot}");
            }
        }

        [Test]
        public void PrepareWorldRejectsDirtyAndMismatchedWorlds()
        {
            LockstepStartBarrier barrier = CreateBarrier();
            SimulationWorld dirty =
                InProcessBattleWorldBootstrap.CreateWorldForBarrier(barrier);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(20);
            dirty.Register(entity);

            Assert.Throws<ArgumentException>(() =>
                InProcessBattleWorldBootstrap.PrepareWorldForHost(barrier, dirty));

            CollisionBroadphaseBackend mismatchedBroadphase =
                barrier.WorldSettings.CollisionBroadphase ==
                CollisionBroadphaseBackend.BruteForce
                    ? CollisionBroadphaseBackend.LooseQuadtree
                    : CollisionBroadphaseBackend.BruteForce;
            var mismatched = new SimulationWorld(
                barrier.WorldSettings.Profile,
                barrier.WorldSettings.InitialRuntimeSlotCapacity,
                mismatchedBroadphase);
            Assert.Throws<ArgumentException>(() =>
                InProcessBattleWorldBootstrap.PrepareWorldForHost(barrier, mismatched));
        }

        [Test]
        public void HostDelegatesToPreparedWorldBootstrap()
        {
            LockstepStartBarrier barrier = CreateBarrier();

            var host = new InProcessBattleKernelHost(barrier, 0, 4);

            Assert.That(host.WorldForDiagnostics.UsesLogicOnlyEntityMaterialization,
                Is.True);
            Assert.That(host.WorldForDiagnostics.Rng.State,
                Is.EqualTo(barrier.Identity.Seed));
            Assert.That(host.WorldForDiagnostics.Runtime.Roster.ActiveSlotCount,
                Is.EqualTo(barrier.PlayerCount));
            Assert.That(host.CurrentTick, Is.Zero);
            Assert.That(host.Journal.Count, Is.Zero);
        }

        private static LockstepStartBarrier CreateBarrier()
        {
            var identity = new LockstepSessionIdentity(
                LockstepSessionIdentity.CurrentSchemaVersion,
                sessionId: 0x51000004UL,
                seed: 0x51A7u,
                catalogFingerprint: 0xCA7A10UL,
                stageFingerprint: 0x57A6EUL,
                playerSlots: new[] { 5, 2 });
            return new LockstepStartBarrier(
                identity,
                ruleFingerprint: 0xC0DE0001UL,
                policyVersion: 1,
                BattleRuntimeProfilePolicy.Create(BattleRuntimeProfile.Authority400));
        }
    }
}
#endif
