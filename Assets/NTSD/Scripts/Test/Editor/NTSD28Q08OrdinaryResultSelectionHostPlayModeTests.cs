#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.Reflection;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q08OrdinaryResultSelectionHostPlayModeTests
    {
        private Scene menuScene;
        private Scene battleScene;
        private GameObject appHost;
        private GameConfig config;
        private SimulationTickDriver driver;

        [UnityTest]
        public IEnumerator OrdinaryTransitionReturnsToExistingCharacterSelection()
        {
            yield return new EnterPlayMode();
            CreateHosts(true, 2, out GameObject selectionView);

            AppManager app = AppManager.Instance;
            for (int frame = 0; frame < 40 &&
                 app.State != AppFlowState.MenuSelectCharacter; frame++)
            {
                yield return null;
            }

            Assert.That(app.State, Is.EqualTo(AppFlowState.MenuSelectCharacter),
                "The ordinary native result command must reach the existing Unity selection host.");
            Assert.That(SceneManager.GetSceneByName("NTSD_Battle").isLoaded, Is.False);
            Assert.That(selectionView.activeSelf, Is.True);
            Assert.That(driver == null, Is.True,
                "The old battle driver must be destroyed with its unloaded scene.");
            Assert.That(SimulationTickDriver.Instance, Is.Null);
        }

        [UnityTest]
        public IEnumerator ModeFourCommandDoesNotUseOrdinarySelectionRoute()
        {
            yield return new EnterPlayMode();
            CreateHosts(true, 202, out GameObject selectionView);
            int initialTick = driver.CurrentTickIndex;
            for (int frame = 0; frame < 5; frame++)
                yield return null;

            Assert.That(AppManager.Instance.State, Is.EqualTo(AppFlowState.BattleRunning));
            Assert.That(battleScene.isLoaded, Is.True);
            Assert.That(selectionView.activeSelf, Is.False);
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(initialTick));
            Assert.That(driver.World.Runtime.Results.NativeTransitionState, Is.EqualTo(202));
        }

        [UnityTest]
        public IEnumerator OrdinaryCommandWithoutMenuRemainsPending()
        {
            yield return new EnterPlayMode();
            CreateHosts(false, 2, out _);
            int initialTick = driver.CurrentTickIndex;
            for (int frame = 0; frame < 5; frame++)
                yield return null;

            Assert.That(AppManager.Instance.State, Is.EqualTo(AppFlowState.BattleRunning));
            Assert.That(battleScene.isLoaded, Is.True);
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(initialTick));
            Assert.That(driver.World.Runtime.Results.NativeTransitionState, Is.EqualTo(2));
        }

        private void CreateHosts(bool includeMenu, int transitionState,
            out GameObject selectionView)
        {
            Assert.That(AppManager.Instance, Is.Null);
            Assert.That(SimulationTickDriver.Instance, Is.Null);
            if (includeMenu)
                menuScene = SceneManager.CreateScene("NTSD_Menu");
            battleScene = SceneManager.CreateScene("NTSD_Battle");

            MenuUIController menuUi = null;
            selectionView = null;
            if (includeMenu)
            {
                var menuHost = new GameObject("__Q08_MenuUi");
                SceneManager.MoveGameObjectToScene(menuHost, menuScene);
                menuUi = menuHost.AddComponent<MenuUIController>();
                selectionView = new GameObject("__Q08_CharacterSelectionView");
                SceneManager.MoveGameObjectToScene(selectionView, menuScene);
                selectionView.SetActive(false);
                SetPrivate(menuUi, "selectCharacter", selectionView);
            }

            config = ScriptableObject.CreateInstance<GameConfig>();
            appHost = new GameObject("__Q08_AppManager");
            appHost.SetActive(false);
            SceneManager.MoveGameObjectToScene(appHost,
                includeMenu ? menuScene : battleScene);
            AppManager app = appHost.AddComponent<AppManager>();
            SetPrivate(app, "gameConfig", config);
            SetPrivate(app, "menuUi", menuUi);
            appHost.SetActive(true);

            var driverHost = new GameObject("__Q08_SimulationDriver");
            SceneManager.MoveGameObjectToScene(driverHost, battleScene);
            driver = driverHost.AddComponent<SimulationTickDriver>();
            driver.SetPaused(false);
            app.SetBattlePaused(false);
            driver.World.Runtime.Results.NativeResultPhase = 3;
            driver.World.Runtime.Results.NativeResultOutputTimer = 350;
            driver.World.Runtime.Results.NativeTransitionState = transitionState;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (driver != null &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Running)
            {
                BattleRuntimeShutdownReport report = driver.ShutdownBattleRuntime();
                if (report.Status != BattleRuntimeShutdownStatus.Failed)
                    driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true);
            }

            if (appHost != null)
                Object.Destroy(appHost);
            if (battleScene.IsValid() && battleScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(battleScene);
            if (menuScene.IsValid() && menuScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(menuScene);
            if (config != null)
                Object.Destroy(config);
            yield return null;
            yield return new ExitPlayMode();
        }

        private static void SetPrivate(object owner, string name, object value)
        {
            FieldInfo field = owner.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            field.SetValue(owner, value);
        }
    }
}
#endif
