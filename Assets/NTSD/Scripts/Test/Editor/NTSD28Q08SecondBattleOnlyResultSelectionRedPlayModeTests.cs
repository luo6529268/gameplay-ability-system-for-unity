#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.Reflection;
using NTSD.App;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q08SecondBattleOnlyResultSelectionRedPlayModeTests
    {
        [UnityTest]
        public IEnumerator SecondResultWithDirectOwnerRequiresLogicalSelectionState()
        {
            yield return new EnterPlayMode();
            Assert.That(AppManager.Instance, Is.Null);
            Assert.That(SimulationTickDriver.Instance, Is.Null);

            Scene battleScene = SceneManager.CreateScene("NTSD_Battle");
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
            GameObject appObject = new GameObject("__Q08_DirectApp");
            appObject.SetActive(false);
            SceneManager.MoveGameObjectToScene(appObject, battleScene);
            AppManager app = appObject.AddComponent<AppManager>();
            SetPrivate(app, "gameConfig", config);
            appObject.SetActive(true);

            GameObject driverObject = new GameObject("__Q08_DirectDriver");
            SceneManager.MoveGameObjectToScene(driverObject, battleScene);
            SimulationTickDriver driver =
                driverObject.AddComponent<SimulationTickDriver>();
            driver.SetPaused(false);

            GameObject hostObject = new GameObject("__Q08_DirectOwner");
            SceneManager.MoveGameObjectToScene(hostObject, battleScene);
            BattleTestBootstrap host = hostObject.AddComponent<BattleTestBootstrap>();
            SetPrivate(host, "ownsDirectBattle", true);
            SetPrivate(host, "firstDirectRematchCompleted", true);
            driver.RegisterBattleOnlyResultHost(host);

            SimulationWorld world = driver.World;
            int frozenTick = driver.CurrentTickIndex;
            world.Runtime.Results.NativeResultPhase = 3;
            world.Runtime.Results.NativeResultOutputTimer = 350;
            world.Runtime.Results.NativeTransitionState = 2;
            for (int frame = 0; frame < 3; frame++)
                yield return null;

            Assert.That(driver.World, Is.SameAs(world));
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(frozenTick));
            Assert.That(world.Runtime.Results.NativeTransitionState,
                Is.EqualTo(1),
                "A second result needs ordinary upper-selection state even without a Menu Scene.");
        }

        [UnityTest]
        [Timeout(180000)]
        public IEnumerator SecondResultEntersOrdinaryUpperSelectionInSameWorld()
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
            SimulationWorld firstWorld = driver.World;
            firstWorld.Runtime.Results.NativeResultPhase = 3;
            firstWorld.Runtime.Results.NativeResultOutputTimer = 350;
            firstWorld.Runtime.Results.NativeTransitionState = 2;
            Assert.That(host.TryHandleFirstBattleOnlyResult(driver), Is.True);

            driver.SetPaused(true);
            SimulationWorld secondWorld = driver.World;
            Assert.That(secondWorld, Is.Not.SameAs(firstWorld));
            Assert.That(secondWorld.ObjectCount, Is.GreaterThan(0));
            int frozenTick = driver.CurrentTickIndex;
            secondWorld.Runtime.Results.NativeResultPhase = 3;
            secondWorld.Runtime.Results.NativeResultOutputTimer = 350;
            secondWorld.Runtime.Results.NativeTransitionState = 2;
            for (int frame = 0; frame < 5; frame++)
                yield return null;

            Assert.That(driver.World, Is.SameAs(secondWorld),
                "The second result must not trigger a second direct rematch.");
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(frozenTick));
            Assert.That(secondWorld.Runtime.Results.NativeTransitionState,
                Is.EqualTo(1),
                "The following upper-state step did not enter ordinary selection.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }

        private static void SetPrivate(object owner, string fieldName, object value)
        {
            FieldInfo field = owner.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(owner, value);
        }
    }
}
#endif
