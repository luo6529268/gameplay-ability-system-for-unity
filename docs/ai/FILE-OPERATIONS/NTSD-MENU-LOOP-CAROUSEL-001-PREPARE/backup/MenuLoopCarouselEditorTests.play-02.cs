#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NTSD.UI.Menu;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NTSD.Test.Editor
{
    public sealed class MenuLoopCarouselEditorTests
    {
        [TestCase(1)]
        [TestCase(-1)]
        public void ManyTurnsReturnToSameIndexAndContinuousVisibleOffsets(int direction)
        {
            var motion = new MenuCarouselMotion();
            motion.Reset(7, 0);
            for (int turn = 0; turn < 20; turn++)
            {
                for (int step = 0; step < 7; step++)
                {
                    motion.Move(direction);
                    for (int frame = 0; frame < 100; frame++)
                    {
                        double[] before = new double[7];
                        for (int i = 0; i < 7; i++) before[i] = motion.Offset(i);
                        motion.Advance(1.0 / 60.0, 0.085);
                        for (int i = 0; i < 7; i++)
                        {
                            double after = motion.Offset(i);
                            if (Math.Abs(before[i]) < 3 && Math.Abs(after) < 3)
                                Assert.Less(Math.Abs(after - before[i]), 0.3, "Visible item jumped");
                        }
                    }
                }
                Assert.AreEqual(0, motion.SelectedIndex);
                Assert.IsTrue(motion.IsSettled);
            }
        }

        [Test]
        public void DragAcrossManyTurnsSnapsAndIgnoresNavigationDuringDrag()
        {
            var motion = new MenuCarouselMotion();
            motion.Reset(7, 0);
            motion.BeginDrag();
            motion.Drag(700.4);
            motion.Move(1);
            Assert.AreEqual(0, motion.SelectedIndex);
            motion.EndDrag();
            for (int i = 0; i < 200; i++) motion.Advance(0.016, 0.085);
            Assert.AreEqual(0, motion.SelectedIndex);
            Assert.IsTrue(motion.IsSettled);
            motion.BeginDrag();
            motion.Drag(-700.7);
            motion.EndDrag();
            motion.FinishSnap();
            Assert.AreEqual(6, motion.SelectedIndex);
        }

        [Test]
        public void RapidQueuedInputPreservesEverySelectedIndex()
        {
            var motion = new MenuCarouselMotion();
            motion.Reset(7, 0);
            for (int i = 1; i <= 10001; i++)
            {
                motion.Move(1);
                Assert.AreEqual(i % 7, motion.SelectedIndex);
                motion.Advance(0.001, 0.085);
                Assert.That(motion.Position, Is.GreaterThanOrEqualTo(0).And.LessThan(7));
            }
            motion.FinishSnap();
            Assert.IsTrue(motion.IsSettled);
            motion.Reset(7, 0);
            Assert.AreEqual(0, motion.SelectedIndex);
            Assert.AreEqual(0, motion.Position);
        }

        [Test]
        public void SelectingLastFromFirstUsesAdjacentWrap()
        {
            var motion = new MenuCarouselMotion();
            motion.Reset(7, 0);
            motion.Select(6);
            Assert.AreEqual(-1, motion.Target);
            motion.FinishSnap();
            Assert.AreEqual(6, motion.SelectedIndex);
            Assert.AreEqual(0, motion.Offset(6));
        }

        [Test]
        public void PointerKeyboardAndWheelUseSingleListAndConfirmOnlyOnce()
        {
            using (var fixture = new Fixture())
            {
                int confirmations = 0;
                int confirmedIndex = -1;
                fixture.List.OnOptionConfirmed += index => { confirmations++; confirmedIndex = index; };
                fixture.List.OnNavigate(Vector2.up);
                Assert.AreEqual(6, fixture.List.CurrentIndex);
                fixture.Carousel.OnScroll(new PointerEventData(null) { scrollDelta = Vector2.down });
                Assert.AreEqual(0, fixture.List.CurrentIndex);
                fixture.Carousel.Click(3);
                Assert.AreEqual(3, fixture.List.CurrentIndex);
                Assert.AreEqual(0, confirmations);
                fixture.List.OnConfirm();
                fixture.Carousel.Click(3);
                fixture.List.OnConfirm();
                Assert.AreEqual(1, confirmations);
                Assert.AreEqual(3, confirmedIndex);
                fixture.List.OnFocusExit();
                fixture.Carousel.Navigate(1);
                Assert.AreEqual(3, fixture.List.CurrentIndex);
            }
        }

        [Test]
        public void DragSuppressesConfirmAndReleaseClickThenSettles()
        {
            using (var fixture = new Fixture())
            {
                int confirmations = 0;
                fixture.List.OnOptionConfirmed += _ => confirmations++;
                var pointer = new PointerEventData(null) { pointerId = 7, position = Vector2.zero };
                fixture.Carousel.OnBeginDrag(pointer);
                pointer.position = new Vector2(0, fixture.Carousel.ItemSpacing * 8.7f);
                fixture.Carousel.OnDrag(pointer);
                fixture.List.OnConfirm();
                Assert.AreEqual(0, confirmations);
                fixture.Carousel.OnEndDrag(pointer);
                fixture.Carousel.Click(fixture.List.CurrentIndex);
                fixture.List.OnConfirm();
                Assert.AreEqual(0, confirmations);
                Assert.IsFalse(pointer.eligibleForClick);
                fixture.Carousel.Motion.FinishSnap();
                Assert.AreEqual(2, fixture.List.CurrentIndex);
                Assert.IsTrue(fixture.Carousel.Motion.IsSettled);
            }
        }

        [Test]
        public void ReleaseRestoresOriginalLayoutAndReopenDoesNotDuplicateItems()
        {
            using (var fixture = new Fixture())
            {
                Assert.IsFalse(fixture.Scroll.enabled);
                Assert.IsFalse(fixture.Layout.enabled);
                fixture.Carousel.Navigate(5);
                Invoke(fixture.Carousel, "OnDisable");
                Assert.IsTrue(fixture.Scroll.enabled);
                Assert.IsTrue(fixture.Layout.enabled);
                Assert.AreEqual(fixture.OriginalContentPosition, fixture.Scroll.content.anchoredPosition);
                Invoke(fixture.List, "OnEnable");
                Invoke(fixture.Carousel, "OnEnable");
                Assert.AreEqual(0, fixture.Carousel.Motion.SelectedIndex);
                Assert.AreEqual(7, fixture.Scroll.content.childCount);
                Assert.AreEqual(7, fixture.Root.GetComponentsInChildren<MenuCarouselOptionPointer>().Length);
                Invoke(fixture.List, "OnDisable");
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator OriginalMenuScene_InputsSnapReopenAndConfirmOnce()
        {
            var entryScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(entryScene.path))
            {
                Assert.IsFalse(entryScene.isDirty, "Do not discard a dirty scene.");
                Assert.AreEqual(0, entryScene.rootCount, "Only replace the empty Test Runner scene.");
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Menu.unity");
            }
            Assert.AreEqual("Assets/NTSD/Scene/NTSD_Menu.unity",
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
            Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
            yield return new UnityEngine.TestTools.EnterPlayMode();
            for (int frame = 0; frame < 12; frame++) yield return null;
            Assert.AreEqual("Assets/NTSD/Scene/NTSD_Menu.unity",
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
            NTSD.UI.MenuUIController.Instance.ShowSelectGameMode();
            yield return null;
            var carousel = UnityEngine.Object.FindObjectOfType<MenuLoopCarousel>();
            Assert.IsNotNull(carousel);
            var list = carousel.GetComponentInParent<MenuOptionList>();
            Assert.AreSame(list, MenuFocusManager.Instance.Current);
            Assert.AreEqual(7, list.OptionCount);
            Assert.AreEqual(0, list.CurrentIndex);
            for (int direction = -1; direction <= 1; direction += 2)
            {
                for (int i = 0; i < 21; i++)
                    MenuFocusManager.Instance.Current.OnNavigate(direction > 0 ? Vector2.down : Vector2.up);
                Assert.AreEqual(0, list.CurrentIndex);
                yield return new WaitForSecondsRealtime(1f);
                Assert.IsTrue(carousel.Motion.IsSettled);
            }

            var eventSystem = EventSystem.current;
            Assert.IsNotNull(eventSystem);
            var pointer = new PointerEventData(eventSystem) { pointerId = -1, position = new Vector2(700, 500) };
            ExecuteEvents.Execute(carousel.gameObject, pointer, ExecuteEvents.beginDragHandler);
            Assert.IsTrue(carousel.IsDragging);
            pointer.position += new Vector2(0, 450);
            ExecuteEvents.Execute(carousel.gameObject, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(carousel.gameObject, pointer, ExecuteEvents.endDragHandler);
            Assert.IsFalse(pointer.eligibleForClick);
            yield return new WaitForSecondsRealtime(1f);
            Assert.IsTrue(carousel.Motion.IsSettled);
            float centerY = ((RectTransform)list.GetOption(list.CurrentIndex).transform).anchoredPosition.y;
            Assert.Less(Mathf.Abs(centerY), 0.05f);
            int beforeWheel = list.CurrentIndex;
            ExecuteEvents.Execute(carousel.gameObject,
                new PointerEventData(eventSystem) { scrollDelta = Vector2.down }, ExecuteEvents.scrollHandler);
            Assert.AreEqual((beforeWheel + 1) % 7, list.CurrentIndex);

            NTSD.UI.MenuUIController.Instance.ShowMainMenu();
            yield return null;
            NTSD.UI.MenuUIController.Instance.ShowSelectGameMode();
            yield return null;
            Assert.AreEqual(0, list.CurrentIndex);
            Assert.AreEqual(7, carousel.GetComponentsInChildren<MenuCarouselOptionPointer>().Length);
            Assert.AreSame(list, MenuFocusManager.Instance.Current);

            int confirmed = 0;
            Action<int> onConfirm = index => { Assert.AreEqual(1, index); confirmed++; };
            list.OnOptionConfirmed += onConfirm;
            try
            {
                // Index 1 is the existing Stage placeholder, so the real event has no scene/quit side effect.
                carousel.Click(1);
                Assert.AreEqual(0, confirmed);
                yield return new WaitForSecondsRealtime(1f);
                ExecuteEvents.Execute(list.GetOption(1).gameObject,
                    new PointerEventData(eventSystem), ExecuteEvents.pointerClickHandler);
                MenuFocusManager.Instance.Current.OnConfirm();
                Assert.AreEqual(1, confirmed);
            }
            finally
            {
                list.OnOptionConfirmed -= onConfirm;
            }
            NTSD.UI.MenuUIController.Instance.ShowMainMenu();
            yield return null;
            NTSD.UI.MenuUIController.Instance.ShowSelectGameMode();
            yield return null;
            string screenshot = "artifacts/diagnostics/NTSD-MENU-LOOP-CAROUSEL-001/menu-interaction-only.png";
            if (!System.IO.File.Exists(screenshot)) ScreenCapture.CaptureScreenshot(screenshot);
            yield return new WaitForSecondsRealtime(0.25f);
            Debug.Log("MENU_CAROUSEL_PLAY_PASS: 3 reverse/3 forward turns, event-interface drag/wheel, snap, reopen, click+confirm once. Visual matching and physical input unverified.");
            yield return new UnityEngine.TestTools.ExitPlayMode();
        }

        [UnityEngine.TestTools.UnityTearDown]
        public System.Collections.IEnumerator ExitOwnedTestPlayMode()
        {
            if (Application.isPlaying)
                yield return new UnityEngine.TestTools.ExitPlayMode();
        }

        private static void Invoke(object instance, string method)
        {
            instance.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
        }

        private sealed class Fixture : IDisposable
        {
            public GameObject Root;
            public MenuOptionList List;
            public UnityEngine.UI.ScrollRect Scroll;
            public UnityEngine.UI.VerticalLayoutGroup Layout;
            public MenuLoopCarousel Carousel;
            public Vector2 OriginalContentPosition = new Vector2(15, 20);

            public Fixture()
            {
                Root = new GameObject("CarouselFixture", typeof(RectTransform));
                List = Root.AddComponent<MenuOptionList>();
                var view = new GameObject("ModeList", typeof(RectTransform));
                view.transform.SetParent(Root.transform, false);
                var viewport = (RectTransform)view.transform;
                viewport.sizeDelta = new Vector2(900, 815);
                var content = new GameObject("Content", typeof(RectTransform));
                content.transform.SetParent(view.transform, false);
                var rect = (RectTransform)content.transform;
                rect.anchoredPosition = OriginalContentPosition;
                Scroll = view.AddComponent<UnityEngine.UI.ScrollRect>();
                Scroll.viewport = viewport;
                Scroll.content = rect;
                Layout = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                Layout.spacing = 77.43f;
                var options = new List<MenuOptionBase>();
                for (int i = 0; i < 7; i++)
                {
                    var item = new GameObject("Option" + i, typeof(RectTransform));
                    item.transform.SetParent(content.transform, false);
                    ((RectTransform)item.transform).sizeDelta = new Vector2(600, 50);
                    options.Add(item.AddComponent<MenuOptionBase>());
                }
                typeof(MenuOptionList).GetField("options", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(List, options);
                List.OnFocusEnter();
                Carousel = view.AddComponent<MenuLoopCarousel>();
                Assert.IsTrue(Carousel.Configure(List, Scroll));
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(Root);
            }
        }
    }
}
#endif
