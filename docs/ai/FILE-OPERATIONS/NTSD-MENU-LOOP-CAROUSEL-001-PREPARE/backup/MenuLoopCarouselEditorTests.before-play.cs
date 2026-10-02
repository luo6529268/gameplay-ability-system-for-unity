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
