#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NTSD.App;
using UnityEditor;
using UnityEngine.TestTools;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MoreMountains.Tools;
using NUnit.Framework;
using NTSD.Simulation;
using NTSD.Tools;
using NTSD.UI.Battle;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public sealed class BattleHudEventEditorTests
    {
        private static NTSDEntityRuntime Runtime(int slot = 0, int stableId = 11)
        {
            return new NTSDEntityRuntime { SlotIndex = slot, StableId = stableId, ObjectId = 7 };
        }

        [Test]
        public void SameDefaultInitializationStillPublishesFullBindingOnce()
        {
            var tracker = new BattleHudChangeTracker();
            var runtime = Runtime();
            runtime.HP = runtime.HPBound = runtime.HP3 = runtime.MP = runtime.MPMax = 500;
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            Assert.That(tracker.TryConsume(out var value), Is.True);
            Assert.That(value.Changes, Is.EqualTo(BattleHudChanges.All));
            Assert.That(value.Hp, Is.EqualTo(500));
            Assert.That(value.Mp, Is.EqualTo(500));
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            Assert.That(tracker.TryConsume(out _), Is.False);
        }

        [Test]
        public void HpChangesAndSameValueAssignmentsPreserveIndependentMasks()
        {
            var tracker = new BattleHudChangeTracker();
            var runtime = Runtime();
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            tracker.TryConsume(out _);
            runtime.HP = 123;
            Assert.That(tracker.TryConsume(out var value), Is.True);
            Assert.That(value.Changes, Is.EqualTo(BattleHudChanges.Hp));
            Assert.That(value.Hp, Is.EqualTo(123));
            runtime.HP = 123;
            Assert.That(tracker.TryConsume(out _), Is.False);
            runtime.HPBound = 200;
            runtime.HP3 = 450;
            Assert.That(tracker.TryConsume(out value), Is.True);
            Assert.That(value.Changes, Is.EqualTo(BattleHudChanges.HpBound | BattleHudChanges.HpMax));
            Assert.That(value.HpBound, Is.EqualTo(200));
            Assert.That(value.HpMax, Is.EqualTo(450));
        }

        [Test]
        public void MpAndMaximumChangesAreCapturedWithoutHpWrites()
        {
            var tracker = new BattleHudChangeTracker();
            var runtime = Runtime();
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            tracker.TryConsume(out _);
            runtime.MP = 125;
            runtime.MPMax = 250;
            Assert.That(tracker.TryConsume(out var value), Is.True);
            Assert.That(value.Changes, Is.EqualTo(BattleHudChanges.Mp | BattleHudChanges.MpMax));
            Assert.That(value.Mp, Is.EqualTo(125));
            Assert.That(value.MpMax, Is.EqualTo(250));
            runtime.MP = 125; runtime.MPMax = 250;
            Assert.That(tracker.TryConsume(out _), Is.False);
        }

        [Test]
        public void ReusedSlotRejectsOldGenerationAndDetachedRuntimeWrites()
        {
            var tracker = new BattleHudChangeTracker();
            var oldRuntime = Runtime();
            var oldHandle = new RuntimeEntityHandle(0, 1);
            tracker.Bind(0, oldRuntime, oldHandle);
            tracker.TryConsume(out _);
            tracker.Release(oldHandle);
            var next = Runtime(0, 22);
            tracker.Bind(0, next, new RuntimeEntityHandle(0, 2));
            Assert.That(tracker.TryConsume(out var value), Is.True);
            Assert.That(value.StableId, Is.EqualTo(22));
            oldRuntime.HP = 1;
            tracker.Capture(0, oldHandle, BattleHudChanges.Mp, 1);
            tracker.Release(oldHandle);
            Assert.That(tracker.TryConsume(out _), Is.False);
            next.MP = 88;
            Assert.That(tracker.TryConsume(out value), Is.True);
            Assert.That(value.Mp, Is.EqualTo(88));
        }

        [Test]
        public void RebindingObjectIdPublishesMetadataAndSelectionUsesFirstHumanSlot()
        {
            var tracker = new BattleHudChangeTracker();
            var second = Runtime(4, 44);
            tracker.Bind(3, second, new RuntimeEntityHandle(4, 1));
            tracker.TryConsume(out _);
            var first = Runtime(2, 22);
            tracker.Bind(1, first, new RuntimeEntityHandle(2, 1));
            Assert.That(tracker.TryConsume(out var value), Is.True);
            Assert.That(value.PlayerIndex, Is.EqualTo(1));
            first.ObjectId = 99;
            tracker.Bind(1, first, new RuntimeEntityHandle(2, 1));
            Assert.That(tracker.TryConsume(out value), Is.True);
            Assert.That(value.ObjectId, Is.EqualTo(99));
            Assert.That(value.Changes, Is.EqualTo(BattleHudChanges.All));
            second.HP = 300;
            Assert.That(tracker.TryConsume(out _), Is.False);
            tracker.UnbindPlayer(1);
            Assert.That(tracker.TryConsume(out value), Is.True);
            Assert.That(value.PlayerIndex, Is.EqualTo(3));
            Assert.That(value.Hp, Is.EqualTo(300));
        }

        [Test]
        public void StopDetachesProducerAndNewWorldSessionAcceptsReusedObject()
        {
            var tracker = new BattleHudChangeTracker();
            var runtime = Runtime();
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            tracker.TryConsume(out var before);
            tracker.Reset(stop: true);
            Assert.That(tracker.TryConsume(out var cleared), Is.True);
            Assert.That(cleared.IsVisible, Is.False);
            runtime.MP = 12;
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 2));
            Assert.That(tracker.TryConsume(out _), Is.False);
            var nextWorld = new BattleHudChangeTracker();
            nextWorld.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            Assert.That(nextWorld.TryConsume(out var next), Is.True);
            Assert.That(next.Session, Is.Not.EqualTo(before.Session));
            Assert.That(next.Mp, Is.EqualTo(12));
        }

        private sealed class Listener : MMEventListener<BattleHudChangedEvent>
        {
            internal readonly List<int> Threads = new List<int>();
            public void OnMMEvent(BattleHudChangedEvent value) => Threads.Add(Thread.CurrentThread.ManagedThreadId);
        }

        [Test]
        public void WorkerRecordsValuesButOnlyMainThreadDispatchCallsListeners()
        {
            var tracker = new BattleHudChangeTracker();
            var runtime = Runtime();
            tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
            tracker.TryConsume(out _);
            var listener = new Listener();
            listener.MMEventStartListening<BattleHudChangedEvent>();
            int mainThread = Thread.CurrentThread.ManagedThreadId;
            try
            {
                Task.Run(() => { runtime.HP = 211; runtime.MP = 97; }).GetAwaiter().GetResult();
                Assert.That(listener.Threads, Is.Empty);
                Assert.That(tracker.TryConsume(out var value), Is.True);
                MMEventManager.TriggerEvent(new BattleHudChangedEvent(value, "Role", null));
                Assert.That(listener.Threads, Is.EqualTo(new[] { mainThread }));
                Assert.That(value.Hp, Is.EqualTo(211));
                Assert.That(value.Mp, Is.EqualTo(97));
                runtime.MP = 5;
                Assert.That(value.Mp, Is.EqualTo(97), "Published payload must be a value snapshot.");
            }
            finally { listener.MMEventStopListening<BattleHudChangedEvent>(); }
        }

        [Test]
        public void LateEnableResyncDisableUnsubscribeAndReenableRestoresCurrentState()
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            SimulationTickDriver previous = SimulationTickDriver.Instance;
            var instance = typeof(SingletonBehaviour<SimulationTickDriver>).GetProperty("Instance");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            BattleHudView view = null;
            try
            {
                var host = new GameObject("HudEventHost");
                host.SetActive(false);
                SceneManager.MoveGameObjectToScene(host, scene);
                var driver = host.AddComponent<SimulationTickDriver>();
                instance.SetValue(null, driver);
                typeof(SimulationTickDriver).GetField("lifecycleState", flags)
                    .SetValue(driver, BattleRuntimeLifecycleState.Running);
                var tracker = new BattleHudChangeTracker();
                var runtime = Runtime();
                tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
                tracker.TryConsume(out var initial);
                var first = new BattleHudChangedEvent(initial, "Naruto", null);
                typeof(SimulationTickDriver).GetField("lastBattleHudEvent", flags).SetValue(driver, first);
                typeof(SimulationTickDriver).GetField("hasBattleHudEvent", flags).SetValue(driver, true);
                var root = new GameObject("Hud", typeof(RectTransform));
                root.SetActive(false);
                SceneManager.MoveGameObjectToScene(root, scene);
                view = root.AddComponent<BattleHudView>();
                var text = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
                text.transform.SetParent(root.transform, false);
                var label = text.GetComponent<TextMeshProUGUI>();
                typeof(BattleHudView).GetField("characterNameText", flags).SetValue(view, label);
                root.SetActive(true);
                typeof(BattleHudView).GetMethod("OnEnable", flags).Invoke(view, null);
                Assert.That(label.text, Is.EqualTo("Naruto"));
                root.SetActive(false);
                typeof(BattleHudView).GetMethod("OnDisable", flags).Invoke(view, null);
                Assert.That(label.text, Is.Empty);
                runtime.ObjectId = 8;
                tracker.Bind(0, runtime, new RuntimeEntityHandle(0, 1));
                tracker.TryConsume(out var bound);
                var next = new BattleHudChangedEvent(bound, "Sasuke", null);
                typeof(SimulationTickDriver).GetField("lastBattleHudEvent", flags).SetValue(driver, next);
                MMEventManager.TriggerEvent(next);
                Assert.That(label.text, Is.Empty);
                root.SetActive(true);
                typeof(BattleHudView).GetMethod("OnEnable", flags).Invoke(view, null);
                Assert.That(label.text, Is.EqualTo("Sasuke"));
                MMEventManager.TriggerEvent(first);
                Assert.That(label.text, Is.EqualTo("Sasuke"), "Old binding event must be ignored.");
                typeof(SimulationTickDriver).GetMethod("ClearPublishedBattleHud", flags).Invoke(driver, null);
                Assert.That(label.text, Is.Empty);
            }
            finally
            {
                if (view != null)
                    typeof(BattleHudView).GetMethod("OnDisable", flags).Invoke(view, null);
                EditorSceneManager.ClosePreviewScene(scene);
                instance.SetValue(null, previous);
            }
        }
    }
    public sealed class BattleHudScenePlayEditorTests
    {
        private const string Folder = "artifacts/diagnostics/NTSD-BATTLE-HUD-EVENTS-001/round2/";

        [UnityTest, Timeout(360000)]
        public IEnumerator BattleHudInitialDisableEnableShutdownAndReentry()
        {
            for (int cycle = 0; cycle < 2; cycle++)
            {
                EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
                yield return new EnterPlayMode();
                SimulationTickDriver driver = null;
                BattleHudChangedEvent state = default;
                for (int second = 0; second < 120; second++)
                {
                    driver = SimulationTickDriver.Instance;
                    if (driver != null && driver.TryGetCurrentBattleHud(out state))
                        break;
                    yield return new WaitForSecondsRealtime(1f);
                }
                Assert.That(driver, Is.Not.Null);
                Assert.That(driver.TryGetCurrentBattleHud(out state), Is.True, "No ready HUD binding.");
                yield return null;
                var view = Object.FindObjectOfType<BattleHudView>();
                Assert.That(view, Is.Not.Null);
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var label = (TMP_Text)typeof(BattleHudView).GetField("characterNameText", flags).GetValue(view);
                var hp = (UnityEngine.UI.Image)typeof(BattleHudView).GetField("hpImage", flags).GetValue(view);
                var mp = (UnityEngine.UI.Image)typeof(BattleHudView).GetField("mpImage", flags).GetValue(view);
                driver.TryGetCurrentBattleHud(out state);
                Assert.That(label.text, Is.Not.Empty);
                Assert.That(label.text, Is.EqualTo(state.DisplayName));
                Assert.That(hp.fillAmount, Is.EqualTo(state.Values.HpMax > 0 ? Mathf.Clamp01((float)state.Values.Hp / state.Values.HpMax) : 0f).Within(.001f));
                Assert.That(mp.fillAmount, Is.EqualTo(state.Values.MpMax > 0 ? Mathf.Clamp01((float)state.Values.Mp / state.Values.MpMax) : 0f).Within(.001f));
                File.AppendAllText(Folder + "play-evidence.txt", $"cycle={cycle} tick={driver.CurrentTickIndex} name={label.text} player={state.Values.PlayerIndex} slot={state.Values.Handle.Slot} generation={state.Values.Handle.Generation} session={state.Values.Session} worker={driver.DedicatedSimulationWorkerActiveForDiagnostics}\n");
                ScreenCapture.CaptureScreenshot(Folder + "hud-cycle-" + cycle + ".png");
                yield return new WaitForSecondsRealtime(.25f);
                view.enabled = false;
                Assert.That(label.text, Is.Empty);
                Assert.That(hp.fillAmount, Is.Zero);
                view.enabled = true;
                driver.TryGetCurrentBattleHud(out state);
                Assert.That(label.text, Is.EqualTo(state.DisplayName));
                Assert.That(AppManager.Instance.TryShutdownBattleRuntimeBeforeSceneDestroy(out _), Is.True);
                Assert.That(driver.LifecycleState, Is.EqualTo(BattleRuntimeLifecycleState.Stopped));
                Assert.That(label.text, Is.Empty);
                Assert.That(hp.fillAmount, Is.Zero);
                Assert.That(mp.fillAmount, Is.Zero);
                Assert.That(driver.TryGetCurrentBattleHud(out _), Is.False);
                File.AppendAllText(Folder + "play-evidence.txt", $"cycle={cycle} disable-enable=PASS shutdown-clear=PASS state={driver.LifecycleState}\n");
                yield return new ExitPlayMode();
            }
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (Application.isPlaying)
                yield return new ExitPlayMode();
            string original = SessionState.GetString("NTSD.HudEvents.OriginalScene", string.Empty);
            if (!string.IsNullOrEmpty(original) && !SceneManager.GetActiveScene().isDirty)
                EditorSceneManager.OpenScene(original);
            SessionState.EraseString("NTSD.HudEvents.OriginalScene");
        }
    }
}
#endif
