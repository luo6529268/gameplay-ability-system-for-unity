#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FusionSelfCheckOracleEditorTests
    {
        [TestCase("CheckOid5152MergeSuccessAndDormantIsolation")]
        [TestCase("CheckOid5152MergeCooldownOneTriggersSameTick")]
        [TestCase("CheckOid5152AuthorityGateMatrix")]
        [TestCase("CheckOid5152MirrorIdentityAndPresentation")]
        [TestCase("CheckOid5152SplitSuccessAndOddTruncate")]
        [TestCase("CheckOid5152SplitFailurePartialRecovery")]
        [TestCase("CheckOid5152DjaReleaseTriggersSameTickSplit")]
        public void ExistingFusionSelfCheckMatchesCurrentAuthority(string name)
        {
            var method = typeof(BattleRuntimeSelfCheck).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, name);
            try { method.Invoke(null, null); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }
    }
}
#endif
