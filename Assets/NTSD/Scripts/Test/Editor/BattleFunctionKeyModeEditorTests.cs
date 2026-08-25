#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleFunctionKeyModeEditorTests
    {
        [Test]
        public void GameConfig_UsesExactModeRuleAndDefaultsToDeny()
        {
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            try
            {
                config.BattleFunctionKeyModeRules = new[]
                {
                    new BattleFunctionKeyModeRule
                    {
                        gameModeId = 0,
                        battleGameModeId = 1,
                        enableF7 = true,
                        enableF8 = true,
                        enableF9 = false,
                    },
                };

                Assert.That(
                    config.ResolveBattleFunctionKeyCommands(0, 1),
                    Is.EqualTo(
                        BattleFunctionKeyCommand.InitializeStats |
                        BattleFunctionKeyCommand.SpawnAllWeapons));
                Assert.That(
                    config.ResolveBattleFunctionKeyCommands(1, 1),
                    Is.EqualTo(BattleFunctionKeyCommand.None));
                Assert.That(
                    config.ResolveBattleFunctionKeyCommands(0, 2),
                    Is.EqualTo(BattleFunctionKeyCommand.None));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void Latch_FoldsF7ParityAndKeepsLatestF8F9Request()
        {
            var latch = new BattleFunctionKeyInputLatch();
            latch.QueueForDiagnostics(BattleFunctionKeyCommand.InitializeStats);
            latch.QueueForDiagnostics(BattleFunctionKeyCommand.InitializeStats);
            Assert.That(latch.TryConsume(out _, out _), Is.False);

            latch.QueueForDiagnostics(
                BattleFunctionKeyCommand.InitializeStats |
                BattleFunctionKeyCommand.SpawnAllWeapons);
            latch.QueueForDiagnostics(BattleFunctionKeyCommand.ClearWeaponPicker);
            Assert.That(
                latch.TryConsume(out bool toggleInitializeStats, out int mode2Request),
                Is.True);
            Assert.That(toggleInitializeStats, Is.True);
            Assert.That(mode2Request, Is.EqualTo(2));
            Assert.That(latch.HasPendingRequest, Is.False);
        }

        [Test]
        public void EntityPostFrameTail_AppliesAndClearsF7RequestAtAuthorityBoundary()
        {
            var world = new SimulationWorld();
            var entity = new LF2OtherObject { ObjectId = 999 };
            entity.SetRequiredRuntimeSlot(50);
            world.Register(entity);
            entity.Health.HP3 = 10;
            entity.Health.HPBound = 20;
            entity.Health.HP = 15;
            entity.Health.PP = 7;
            world.SetBattleExitCountdown(91);
            world.ToggleInitStatsRequest();

            Assert.That(world.InitStatsRequest, Is.EqualTo(1));
            Assert.That(world.Runtime.Flow.BattleExitCountdown, Is.Zero);
            world.EntityPostFrameTailAll(1);
            Assert.That(entity.Health.HP3, Is.EqualTo(500));
            Assert.That(entity.Health.HPBound, Is.EqualTo(500));
            Assert.That(entity.Health.HP, Is.EqualTo(500));
            Assert.That(entity.Health.PP, Is.EqualTo(500));

            world.ClearFunctionKeyRequestsAfterPostFrameTail();
            Assert.That(world.InitStatsRequest, Is.Zero);
            Assert.That(world.Mode2Request, Is.Zero);
            world.Unregister(entity);
        }

        [Test]
        public void FlowRequest_IsTrackedByChecksumAndCoreSnapshot()
        {
            var world = new SimulationWorld();
            var input = FrameInputSet.Empty(0);
            ulong before = world.CaptureRuntimeChecksum64(0, input);
            world.SetInitStatsRequest(1);
            ulong after = world.CaptureRuntimeChecksum64(0, input);
            Assert.That(after, Is.Not.EqualTo(before));

            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldCoreScalarSnapshot(world, identity);
            Assert.That(snapshot.Flow.InitStatsRequest, Is.EqualTo(1));
            Assert.That(
                snapshot.SchemaVersion,
                Is.EqualTo(BattleWorldCoreScalarSnapshot.CurrentSchemaVersion));
        }
    }
}
#endif
