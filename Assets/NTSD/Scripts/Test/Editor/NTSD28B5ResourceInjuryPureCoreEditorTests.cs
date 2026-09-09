#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ResourceInjuryPureCoreEditorTests
    {
        [TestCase(7, 1, 0, 0, 14)]
        [TestCase(10, 0, 200, 300, 20)]
        [TestCase(1, 0, 0, 149, 1)]
        [TestCase(1, 0, 0, 150, 2)]
        [TestCase(-1, 0, 150, 0, -1)]
        [TestCase(25, 1, 150, 0, 75)]
        [TestCase(0, 1, 150, 0, 0)]
        public void ResolvesNativeDoublePriorityAndRounding(
            int injury,
            int injuryDouble,
            int definitionAttacking,
            int activeModePercent,
            int expected)
        {
            int actual = BattleDamageWriter.ResolveNativeHitResourceInjury(
                injury,
                injuryDouble,
                definitionAttacking,
                activeModePercent);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void InjuryDouble_PreservesLowThirtyTwoBits()
        {
            int actual = BattleDamageWriter.ResolveNativeHitResourceInjury(
                int.MaxValue,
                1,
                0,
                0);

            Assert.That(actual, Is.EqualTo(-2));
        }

        [Test]
        public void MultiplierProduct_PreservesLowThirtyTwoBitsBeforeDivision()
        {
            int actual = BattleDamageWriter.ResolveNativeHitResourceInjury(
                int.MaxValue,
                0,
                2,
                0);

            Assert.That(actual, Is.Zero);
        }
    }
}
#endif
