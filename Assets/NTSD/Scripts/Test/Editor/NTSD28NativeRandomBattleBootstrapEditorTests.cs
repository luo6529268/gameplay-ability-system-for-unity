#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeRandomBattleBootstrapEditorTests
    {
        private const uint ScenarioSeed = 682973786u;

        [Test]
        public void DirectBattleReset_MatchesAuthorityRandomBgmPreDrawVector()
        {
            var random = new NTSD28NativeRandom();

            random.ResetForDirectBattle(ScenarioSeed);

            NTSD28NativeRandomScalarState state = random.CaptureScalarState();
            Assert.That(state.CrtState, Is.EqualTo(1758127634u));
            Assert.That(state.CrtCalls, Is.EqualTo(3000UL));
            Assert.That(state.TableSeed, Is.EqualTo(ScenarioSeed));
            Assert.That(state.SynchronizedCounter, Is.EqualTo(1));
            Assert.That(state.SynchronizedIndex, Is.EqualTo(1));
            Assert.That(state.SynchronizedCalls, Is.EqualTo(1UL));
            Assert.That(
                state.LastSynchronizedCallSite,
                Is.EqualTo(0x004021E0u));
            Assert.That(
                state.SynchronizedTableHash,
                Is.EqualTo(0xA1BA1B90EA55796DUL));
        }

        [Test]
        public void DirectBattleReset_ReplaysFreshTransactionWhileGenericResetStaysPure()
        {
            var random = new NTSD28NativeRandom();
            random.ResetForDirectBattle(ScenarioSeed);
            NTSD28NativeRandomScalarState first = random.CaptureScalarState();
            random.SynchronizedNext(0x82u, 2);

            random.ResetForDirectBattle(ScenarioSeed);

            NTSD28NativeRandomScalarState replayed = random.CaptureScalarState();
            AssertStateEqual(first, replayed);

            random.ResetFromSeed(ScenarioSeed);
            NTSD28NativeRandomScalarState generic = random.CaptureScalarState();
            Assert.That(generic.SynchronizedCounter, Is.Zero);
            Assert.That(generic.SynchronizedIndex, Is.Zero);
            Assert.That(generic.SynchronizedCalls, Is.Zero);
            Assert.That(generic.LastSynchronizedCallSite, Is.Zero);
        }

        private static void AssertStateEqual(
            NTSD28NativeRandomScalarState expected,
            NTSD28NativeRandomScalarState actual)
        {
            Assert.That(actual.CrtState, Is.EqualTo(expected.CrtState));
            Assert.That(actual.CrtCalls, Is.EqualTo(expected.CrtCalls));
            Assert.That(actual.TableSeed, Is.EqualTo(expected.TableSeed));
            Assert.That(
                actual.SynchronizedCounter,
                Is.EqualTo(expected.SynchronizedCounter));
            Assert.That(
                actual.SynchronizedIndex,
                Is.EqualTo(expected.SynchronizedIndex));
            Assert.That(
                actual.SynchronizedCalls,
                Is.EqualTo(expected.SynchronizedCalls));
            Assert.That(
                actual.LastSynchronizedCallSite,
                Is.EqualTo(expected.LastSynchronizedCallSite));
            Assert.That(
                actual.SynchronizedTableHash,
                Is.EqualTo(expected.SynchronizedTableHash));
        }
    }
}
#endif
