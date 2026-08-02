#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using System.Text.RegularExpressions;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.Rendering.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class PlayDomainReloadPoolLifecycleEditorTests
    {
        [Test]
        public void OverlayDestroy_DoesNotResolveOrAutoCreatePool()
        {
            FieldInfo singletonField = typeof(MMSingleton<LF2ObjectPool>).GetField(
                "_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(singletonField, Is.Not.Null);

            LF2ObjectPool original = singletonField.GetValue(null) as LF2ObjectPool;
            int poolCountBefore = CountScenePools();
            GameObject overlayHost = null;
            try
            {
                singletonField.SetValue(null, null);
                Assert.That(LF2ObjectPool.TryGetInstance(), Is.Null);

                overlayHost = new GameObject("OverlayTeardown_NoPoolLookup");
                overlayHost.AddComponent<BattleEntityOverlayRenderer>();
                UnityEngine.Object.DestroyImmediate(overlayHost);
                overlayHost = null;

                Assert.That(LF2ObjectPool.TryGetInstance(), Is.Null,
                    "overlay teardown must use a non-creating singleton lookup");
                Assert.That(CountScenePools(), Is.EqualTo(poolCountBefore),
                    "overlay teardown must not create LF2ObjectPool_AutoCreated");
            }
            finally
            {
                if (overlayHost != null)
                    UnityEngine.Object.DestroyImmediate(overlayHost);
                LF2ObjectPool unexpected = LF2ObjectPool.TryGetInstance();
                if (unexpected != null && unexpected != original &&
                    unexpected.name == "LF2ObjectPool_AutoCreated")
                {
                    UnityEngine.Object.DestroyImmediate(unexpected.gameObject);
                }
                singletonField.SetValue(null, original);
            }
        }

        [Test]
        public void PoolUpdate_InvalidManagedStateLogsOnceDisablesAndKeepsAcceptanceSafe()
        {
            var host = new GameObject("LF2ObjectPool_InvalidState_Test");
            try
            {
                LF2ObjectPool pool = host.AddComponent<LF2ObjectPool>();
                SetPrivateField(pool, "_availableObjects", null);
                SetPrivateField(pool, "_activeObjects", null);
                SetPrivateField(pool, "_releaseTimeMap", null);
                SetPrivateField(pool, "_spritePool", null);
                MethodInfo update = typeof(LF2ObjectPool).GetMethod(
                    "Update",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(update, Is.Not.Null);

                LogAssert.Expect(
                    LogType.Error,
                    new Regex("LF2ObjectPool.*managed runtime state was invalidated"));
                update.Invoke(pool, null);
                update.Invoke(pool, null);

                Assert.That(pool.enabled, Is.False);
                Assert.That(pool.IsRuntimeStateValidForAcceptance, Is.False);
                Assert.That(pool.AvailableObjectCountForAcceptance, Is.Zero);
                Assert.That(pool.ActiveObjectCountForAcceptance, Is.Zero);
                Assert.That(pool.GetPoolStatus(), Does.Contain("invalidated"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [TestCase(false, true, false, false, false, false, 0,
            (int)ProductionEntityStressPlayRestartDecision.None)]
        [TestCase(true, false, true, true, false, false, 0,
            (int)ProductionEntityStressPlayRestartDecision.None)]
        [TestCase(true, true, false, false, false, false, 0,
            (int)ProductionEntityStressPlayRestartDecision.WaitForInitialServices)]
        [TestCase(true, true, false, true, false, false, 0,
            (int)ProductionEntityStressPlayRestartDecision.RestartPlayMode)]
        [TestCase(true, true, true, true, true, true, 1,
            (int)ProductionEntityStressPlayRestartDecision.RecordHealthyRuntime)]
        [TestCase(true, true, true, true, false, true, 1,
            (int)ProductionEntityStressPlayRestartDecision.WaitForRestartTransition)]
        [TestCase(true, true, true, true, false, false, 0,
            (int)ProductionEntityStressPlayRestartDecision.RestartPlayMode)]
        [TestCase(true, true, false, false, false, false, 1,
            (int)ProductionEntityStressPlayRestartDecision.RetryLimitExceeded)]
        public void RestartPolicy_IsBoundedAndStateDriven(
            bool pendingStartRequest,
            bool isPlaying,
            bool managedRuntimeWasValid,
            bool managedRuntimeExpected,
            bool managedRuntimeIsValid,
            bool restartTransitionPending,
            int restartCount,
            int expected)
        {
            Assert.That(
                (int)ProductionEntityStressRequestProcessor.EvaluatePlayRestartDecision(
                    pendingStartRequest,
                    isPlaying,
                    managedRuntimeWasValid,
                    managedRuntimeExpected,
                    managedRuntimeIsValid,
                    restartTransitionPending,
                    restartCount),
                Is.EqualTo(expected));
        }

        private static int CountScenePools()
        {
            LF2ObjectPool[] pools = Resources.FindObjectsOfTypeAll<LF2ObjectPool>();
            int count = 0;
            for (int index = 0; index < pools.Length; index++)
            {
                LF2ObjectPool pool = pools[index];
                if (pool != null && pool.gameObject.scene.IsValid())
                    count++;
            }
            return count;
        }

        private static void SetPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            field.SetValue(target, value);
        }
    }
}
#endif
