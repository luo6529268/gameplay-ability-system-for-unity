#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

using NTSD.Test;
using NUnit.Framework;

namespace NTSD.EditorTests
{
    public sealed class SimulationWorldExtractedPassModuleEditorTests
    {
        [TestCase("CheckOid5152MergeSuccessAndDormantIsolation")]
        [TestCase("CheckOid5152MergeCooldownOneTriggersSameTick")]
        [TestCase("CheckOid5152AuthorityGateMatrix")]
        [TestCase("CheckOid5152MirrorIdentityAndPresentation")]
        [TestCase("CheckOid5152SplitSuccessAndOddTruncate")]
        [TestCase("CheckOid5152SplitFailurePartialRecovery")]
        [TestCase("CheckOid5152DjaReleaseTriggersSameTickSplit")]
        public void Oid5152Module_PreservesExistingSelfCheck(string methodName)
        {
            InvokeExistingSelfCheck(methodName);
        }

        [TestCase("CheckRespawnPassWithoutStoredCount")]
        [TestCase("CheckRespawnReadsPhysicsTailIntegerCoordinates")]
        [TestCase("CheckRespawnPassFreeEntityGate")]
        [TestCase("CheckRespawnPassWithStoredCountAndEffectSpawn")]
        public void RespawnModule_PreservesExistingSelfCheck(string methodName)
        {
            InvokeExistingSelfCheck(methodName);
        }

        [TestCase("CheckRandomWeaponDropAuthorityContract")]
        [TestCase("CheckFrameLifecycleAuthorityBatchContracts")]
        [TestCase("CheckGameTickInputLifetimeBoundaries")]
        public void PassPipeline_PreservesRngAndPassOrderSelfCheck(
            string methodName)
        {
            InvokeExistingSelfCheck(methodName);
        }

        private static void InvokeExistingSelfCheck(string methodName)
        {
            MethodInfo method = typeof(BattleRuntimeSelfCheck).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null,
                $"BattleRuntimeSelfCheck must retain focused check {methodName}.");

            try
            {
                method.Invoke(null, null);
            }
            catch (TargetInvocationException exception)
                when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw new InvalidOperationException(
                    "Unreachable after rethrowing the self-check failure.");
            }
        }
    }

}
#endif
