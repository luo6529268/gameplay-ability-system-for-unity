#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q08BattleOnlyRematchRedPlayModeTests
    {
        [UnityTest]
        [Timeout(180000)]
        public IEnumerator FirstDirectBattleResultRecreatesPlayableBattle()
        {
            EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
            yield return new EnterPlayMode();

            SimulationTickDriver driver = null;
            for (int second = 0; second < 120; second++)
            {
                driver = SimulationTickDriver.Instance;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World != null && driver.World.ObjectCount > 0)
                {
                    break;
                }

                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(driver, Is.Not.Null, "Direct Battle bootstrap did not create a driver.");
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Running));
            Assert.That(driver.World, Is.Not.Null);
            Assert.That(driver.World.ObjectCount, Is.GreaterThan(0));

            SimulationWorld oldWorld = driver.World;
            oldWorld.Runtime.Results.NativeResultPhase = 3;
            oldWorld.Runtime.Results.NativeResultOutputTimer = 350;
            oldWorld.Runtime.Results.NativeTransitionState = 2;

            for (int second = 0; second < 15 &&
                 ReferenceEquals(driver.World, oldWorld); second++)
            {
                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(driver.World, Is.Not.SameAs(oldWorld),
                "The first battle-only result did not recreate its World.");
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Running));
            Assert.That(driver.World.ObjectCount, Is.GreaterThan(0),
                "A new empty World is not a playable rematch.");
            Assert.That(driver.World.Runtime.Roster.ActiveSlotCount,
                Is.GreaterThan(0), "The direct roster was not recreated.");

            int rematchTick = driver.CurrentTickIndex;
            for (int second = 0; second < 3 &&
                 driver.CurrentTickIndex <= rematchTick; second++)
            {
                yield return new WaitForSecondsRealtime(1f);
            }
            Assert.That(driver.CurrentTickIndex, Is.GreaterThan(rematchTick),
                "The recreated battle did not resume its host tick.");
        }

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator FirstRematchPreservesRandomAndInputPhaseAfterOrderedClose()
        {
            EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
            yield return new EnterPlayMode();

            SimulationTickDriver driver = null;
            for (int second = 0; second < 120; second++)
            {
                driver = SimulationTickDriver.Instance;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World != null && driver.World.ObjectCount > 0)
                    break;
                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(driver, Is.Not.Null);
            Assert.That(driver.World, Is.Not.Null);
            Assert.That(driver.World.ObjectCount, Is.GreaterThan(0));
            BattleTestBootstrap host =
                Object.FindObjectOfType<BattleTestBootstrap>(true);
            Assert.That(host, Is.Not.Null);

            driver.SetPaused(true);
            SimulationWorld oldWorld = driver.World;
            oldWorld.Runtime.Flow.InputPhase = 1;
            NTSD28NativeRandomState randomBefore =
                oldWorld.NativeRandom.CaptureState();
            uint localRngBefore = oldWorld.Rng.State;
            oldWorld.Runtime.Results.NativeResultPhase = 3;
            oldWorld.Runtime.Results.NativeResultOutputTimer = 350;
            oldWorld.Runtime.Results.NativeTransitionState = 2;

            Assert.That(host.TryHandleFirstBattleOnlyResult(driver), Is.True);
            driver.SetPaused(true);

            SimulationWorld nextWorld = driver.World;
            Assert.That(nextWorld, Is.Not.SameAs(oldWorld));
            Assert.That(oldWorld.ObjectCount, Is.EqualTo(0));
            Assert.That(nextWorld.ObjectCount, Is.GreaterThan(0));
            Assert.That(nextWorld.Runtime.Roster.ActiveSlotCount, Is.GreaterThan(0));
            Assert.That(nextWorld.InputPhase, Is.EqualTo(1));
            Assert.That(nextWorld.Rng.State, Is.EqualTo(localRngBefore));

            NTSD28NativeRandomState randomAfter =
                nextWorld.NativeRandom.CaptureState();
            Assert.That(randomAfter.CrtState, Is.EqualTo(randomBefore.CrtState));
            Assert.That(randomAfter.CrtCalls, Is.EqualTo(randomBefore.CrtCalls));
            Assert.That(randomAfter.Synchronized.Calls,
                Is.EqualTo(randomBefore.Synchronized.Calls + 1));
            Assert.That(randomAfter.Synchronized.LastCallSite,
                Is.EqualTo(0x004021E0u));
            Assert.That(randomAfter.Synchronized.Table,
                Is.EqualTo(randomBefore.Synchronized.Table));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }
    }
}
#endif
