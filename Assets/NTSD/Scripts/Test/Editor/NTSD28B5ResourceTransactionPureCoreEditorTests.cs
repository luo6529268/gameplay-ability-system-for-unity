#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ResourceTransactionPureCoreEditorTests
    {
        [Test]
        public void LocalModeDisabled_PreservesBothParticipants()
        {
            NTSDEntityRuntime attacker = CreateTypeZero(mp: 40);
            NTSDEntityRuntime target = CreateTypeZero(mp: 50);
            attacker.InputMpConsumedTotal350 = 3;
            target.InputMpConsumedTotal350 = 4;

            BattleDamageWriter.ApplyNativeHitResourceTransaction(
                attacker, target, 20, 0, 10, -10, false, 50, 50, 500);

            Assert.That(attacker.MP, Is.EqualTo(40));
            Assert.That(target.MP, Is.EqualTo(50));
            Assert.That(attacker.InputMpConsumedTotal350, Is.EqualTo(3));
            Assert.That(target.InputMpConsumedTotal350, Is.EqualTo(4));
        }

        [Test]
        public void InjuryRewards_RunBeforeDrainAndNegativeGainCosts()
        {
            NTSDEntityRuntime attacker = CreateTypeZero(mp: 40);
            NTSDEntityRuntime target = CreateTypeZero(mp: 40);

            BattleDamageWriter.ApplyNativeHitResourceTransaction(
                attacker, target, 20, 0, 50, -50, true, 50, 50, 500);

            Assert.That(attacker.MP, Is.Zero);
            Assert.That(target.MP, Is.Zero);
            Assert.That(attacker.InputMpConsumedTotal350, Is.EqualTo(50));
            Assert.That(target.InputMpConsumedTotal350, Is.EqualTo(50));
        }

        [Test]
        public void SuppressionOne_DisablesOnlyInjuryRewards()
        {
            NTSDEntityRuntime attacker = CreateTypeZero(mp: 40);
            NTSDEntityRuntime target = CreateTypeZero(mp: 40);

            BattleDamageWriter.ApplyNativeHitResourceTransaction(
                attacker, target, 20, 1, 10, -10, true, 50, 50, 500);

            Assert.That(attacker.MP, Is.EqualTo(30));
            Assert.That(target.MP, Is.EqualTo(30));
            Assert.That(attacker.InputMpConsumedTotal350, Is.EqualTo(10));
            Assert.That(target.InputMpConsumedTotal350, Is.EqualTo(10));
        }

        [Test]
        public void NonTypeZeroParticipants_KeepIndependentDrainAndGainGates()
        {
            var attacker = new NTSDEntityRuntime { ObjType = 3, MP = 40 };
            NTSDEntityRuntime target = CreateTypeZero(mp: 40);

            BattleDamageWriter.ApplyNativeHitResourceTransaction(
                attacker, target, 20, 0, 10, -10, true, 50, 50, 500);

            Assert.That(attacker.MP, Is.EqualTo(40));
            Assert.That(attacker.InputMpConsumedTotal350, Is.Zero);
            Assert.That(target.MP, Is.EqualTo(30));
            Assert.That(target.InputMpConsumedTotal350, Is.EqualTo(10));
        }

        [TestCase(10, 100)]
        [TestCase(11, 90)]
        public void PositiveGain_RequiresResultWithinBaseMaximum(
            int gain,
            int expectedMp)
        {
            NTSDEntityRuntime attacker = CreateTypeZero(mp: 90);
            var target = new NTSDEntityRuntime { ObjType = 3, MP = 0 };

            BattleDamageWriter.ApplyNativeHitResourceTransaction(
                attacker, target, 0, 0, 0, gain, true, 0, 0, 100);

            Assert.That(attacker.MP, Is.EqualTo(expectedMp));
        }

        [Test]
        public void UnaffordableDrainAndNegativeGain_AreAllOrNothing()
        {
            NTSDEntityRuntime attacker = CreateTypeZero(mp: 9);
            NTSDEntityRuntime target = CreateTypeZero(mp: 9);

            BattleDamageWriter.ApplyNativeHitResourceTransaction(
                attacker, target, 0, 0, 10, -10, true, 0, 0, 500);

            Assert.That(attacker.MP, Is.EqualTo(9));
            Assert.That(target.MP, Is.EqualTo(9));
            Assert.That(attacker.InputMpConsumedTotal350, Is.Zero);
            Assert.That(target.InputMpConsumedTotal350, Is.Zero);
        }

        private static NTSDEntityRuntime CreateTypeZero(int mp)
        {
            return new NTSDEntityRuntime
            {
                ObjType = 0,
                MP = mp,
            };
        }
    }
}
#endif
