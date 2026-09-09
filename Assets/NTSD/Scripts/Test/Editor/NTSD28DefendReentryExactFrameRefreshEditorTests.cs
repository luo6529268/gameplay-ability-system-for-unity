#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28DefendReentryExactFrameRefreshEditorTests
    {
        [TestCase((byte)0)]
        [TestCase((byte)3)]
        [TestCase(byte.MaxValue)]
        public void SetDefendLock_MirrorsValueIntoLegacyAndExactCarriers(
            byte value)
        {
            var world = new SimulationWorld();
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.CdDefendLock = value == byte.MaxValue
                ? (byte)0
                : byte.MaxValue;
            runtime.NativeInputProxy.DefendReentryCooldown = value == 0
                ? byte.MaxValue
                : (byte)0;

            world.CharacterInputWriter.SetDefendLock(runtime, value);

            Assert.That(runtime.CdDefendLock, Is.EqualTo(value));
            Assert.That(
                runtime.NativeInputProxy.DefendReentryCooldown,
                Is.EqualTo(value));
        }
    }
}
#endif
