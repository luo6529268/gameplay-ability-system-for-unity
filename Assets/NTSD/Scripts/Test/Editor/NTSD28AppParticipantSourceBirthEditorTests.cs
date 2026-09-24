#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28AppParticipantSourceBirthEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void ParticipantPlacementInitializesRawSourceAndOverwritesPriorHistory(bool configuredView)
        {
            var world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            var participant = new LF2Character();
            participant.ModuleInitialize();
            participant.SetRequiredRuntimeSlot(0);
            world.Register(participant);
            try
            {
                Assert.That(participant.Runtime.SourceRulePositionInitialized, Is.False);
                PlaceAndAssert(participant, world, 301.75, 220);
                PlaceAndAssert(participant, world, -42.75, 290);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        private static void PlaceAndAssert(LF2Character participant, SimulationWorld world, double spawnX, int spawnZ)
        {
            participant.PS.x = spawnX;
            participant.PS.y = 0;
            participant.PS.z = spawnZ;
            participant.PS.vx = 0.1;
            participant.PS.vy = 0;
            participant.PS.vz = 0.1;
            participant.HitStun = 75;
            uint rngBefore = world.Rng.State;
            ulong nativeCallsBefore = world.NativeRandom.CaptureScalarState().SynchronizedCalls;
            AppManager.SyncParticipantBirthPosition(participant, spawnX, spawnZ);
            Assert.That(participant.Runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(participant.Runtime.SourceRuleX, Is.EqualTo(spawnX));
            Assert.That(participant.Runtime.SourceRuleZ, Is.EqualTo(spawnZ));
            Assert.That(participant.Runtime.SourceRuleXInt, Is.EqualTo((int)spawnX));
            Assert.That(participant.Runtime.SourceRuleZInt, Is.EqualTo(spawnZ));
            Assert.That(participant.Runtime.X, Is.EqualTo(spawnX));
            Assert.That(participant.Runtime.Z, Is.EqualTo(spawnZ));
            Assert.That(participant.Runtime.XInt, Is.EqualTo((int)spawnX));
            Assert.That(participant.Runtime.ZInt, Is.EqualTo(spawnZ));
            Assert.That(participant.PS.vx, Is.EqualTo(0.1));
            Assert.That(participant.PS.vy, Is.Zero);
            Assert.That(participant.PS.vz, Is.EqualTo(0.1));
            Assert.That(participant.HitStun, Is.EqualTo(75));
            Assert.That(world.Rng.State, Is.EqualTo(rngBefore));
            Assert.That(world.NativeRandom.CaptureScalarState().SynchronizedCalls, Is.EqualTo(nativeCallsBefore));
        }
    }
}
#endif
