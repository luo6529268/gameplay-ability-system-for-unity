#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q08DirectBattleResultHostPlayModeTests
    {
        [UnityTest]
        [Timeout(180000)]
        public IEnumerator DirectBattleSceneUsesRematchOwnerWithoutMenuScene()
        {
            EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
            yield return new EnterPlayMode();

            SimulationTickDriver driver = null;
            for (int second = 0; second < 120; second++)
            {
                driver = SimulationTickDriver.Instance;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World != null && driver.World.ObjectCount > 0 &&
                    AppManager.Instance != null)
                    break;
                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(driver, Is.Not.Null);
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Running));
            Assert.That(driver.World, Is.Not.Null);
            Assert.That(driver.World.ObjectCount, Is.GreaterThan(0));
            Assert.That(AppManager.Instance, Is.Not.Null);
            Assert.That(AppManager.Instance.gameObject.name,
                Is.EqualTo("AppManager [TestBootstrap]"));
            Assert.That(AppManager.Instance.State, Is.EqualTo(AppFlowState.MenuMain));
            Assert.That(SceneManager.GetSceneByName("NTSD_Menu").isLoaded, Is.False);

            SimulationWorld oldWorld = driver.World;
            driver.World.Runtime.Results.NativeResultPhase = 3;
            driver.World.Runtime.Results.NativeResultOutputTimer = 350;
            driver.World.Runtime.Results.NativeTransitionState = 2;
            for (int second = 0; second < 15 &&
                 ReferenceEquals(driver.World, oldWorld); second++)
            {
                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(driver.World, Is.Not.SameAs(oldWorld));
            Assert.That(driver.World.ObjectCount, Is.GreaterThan(0));
            Assert.That(driver.LifecycleState,
                Is.EqualTo(BattleRuntimeLifecycleState.Running));
            Assert.That(SceneManager.GetSceneByName("NTSD_Battle").isLoaded, Is.True);
            Assert.That(AppManager.Instance.State, Is.EqualTo(AppFlowState.MenuMain));
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
