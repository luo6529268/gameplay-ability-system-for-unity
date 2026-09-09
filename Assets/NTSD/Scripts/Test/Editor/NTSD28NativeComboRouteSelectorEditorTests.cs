#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeComboRouteSelectorEditorTests
    {
        [Test]
        public void Parser_PreservesNativeAjAdAndJdFields()
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(@"
<frame> 0 native
  pic: 0 state: 0 wait: 1 next: 0 hit_aj: 145 hit_ad: 360 hit_jd: 630
<frame_end>");

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(dat.Frames[0]);

            Assert.That(frame.hit_aj, Is.EqualTo(145));
            Assert.That(frame.hit_ad, Is.EqualTo(360));
            Assert.That(frame.hit_jd, Is.EqualTo(630));
            Assert.That(frame.Hit["aj"], Is.EqualTo(145));
            Assert.That(frame.Hit["ad"], Is.EqualTo(360));
            Assert.That(frame.Hit["jd"], Is.EqualTo(630));
        }

        [TestCase(0, 4, (int)NTSD28NativeComboField.HitFa, 101, true, false)]
        [TestCase(0, 5, (int)NTSD28NativeComboField.HitFa, 101, true, true)]
        [TestCase(1, 4, (int)NTSD28NativeComboField.HitFj, 102, true, false)]
        [TestCase(1, 5, (int)NTSD28NativeComboField.HitFj, 102, true, true)]
        [TestCase(2, 3, (int)NTSD28NativeComboField.HitUa, 103, false, false)]
        [TestCase(3, 3, (int)NTSD28NativeComboField.HitUj, 104, false, false)]
        [TestCase(4, 3, (int)NTSD28NativeComboField.HitDa, 105, false, false)]
        [TestCase(5, 3, (int)NTSD28NativeComboField.HitDj, 106, false, false)]
        [TestCase(6, 1, (int)NTSD28NativeComboField.HitAj, 107, false, false)]
        [TestCase(7, 1, (int)NTSD28NativeComboField.HitAd, 108, false, false)]
        [TestCase(8, 1, (int)NTSD28NativeComboField.HitJa, 109, false, false)]
        [TestCase(9, 1, (int)NTSD28NativeComboField.HitJd, 110, false, false)]
        public void EachCompletedNativeCombo_SelectsExactField(
            int comboIndex,
            int comboState,
            int expectedField,
            int expectedAction,
            bool expectedHasFacing,
            bool expectedFacingLeft)
        {
            NTSDEntityRuntime runtime = CreateRuntime();
            runtime.NativeInputProxy.ComboState[comboIndex] = (byte)comboState;
            LF2FrameData frame = CreateFrame();

            bool selected = NTSD28NativeComboRouteSelector.TrySelect(
                frame,
                runtime,
                out NTSD28NativeComboRouteDecision decision);

            Assert.That(selected, Is.True);
            Assert.That((int)decision.Field, Is.EqualTo(expectedField));
            Assert.That(decision.RequestedAction, Is.EqualTo(expectedAction));
            Assert.That(decision.HasHorizontalFacing, Is.EqualTo(expectedHasFacing));
            Assert.That(decision.FacingLeft, Is.EqualTo(expectedFacingLeft));
        }

        [Test]
        public void AllCompletedCombos_SelectFirstNativePriorityWithoutMutation()
        {
            NTSDEntityRuntime runtime = CreateRuntime();
            byte[] combo = runtime.NativeInputProxy.ComboState;
            combo[0] = combo[1] = 4;
            for (int index = 2; index < 6; index++)
                combo[index] = 3;
            for (int index = 6; index < combo.Length; index++)
                combo[index] = 1;
            byte[] before = combo.ToArray();

            Assert.That(NTSD28NativeComboRouteSelector.TrySelect(
                CreateFrame(), runtime, out NTSD28NativeComboRouteDecision decision), Is.True);

            Assert.That(decision.Field, Is.EqualTo(NTSD28NativeComboField.HitFa));
            Assert.That(decision.RequestedAction, Is.EqualTo(101));
            Assert.That(combo, Is.EqualTo(before));
        }

        [Test]
        public void CompletedZeroField_SelectsButDoesNotImplyAttemptOrFacingMutation()
        {
            NTSDEntityRuntime runtime = CreateRuntime();
            runtime.NativeInputProxy.ComboState[0] = 5;
            LF2FrameData frame = CreateFrame();
            frame.hit_Fa = 0;

            Assert.That(NTSD28NativeComboRouteSelector.TrySelect(
                frame, runtime, out NTSD28NativeComboRouteDecision decision), Is.True);

            Assert.That(decision.Field, Is.EqualTo(NTSD28NativeComboField.HitFa));
            Assert.That(decision.RequestedAction, Is.Zero);
            Assert.That(decision.HasHorizontalFacing, Is.True);
            Assert.That(decision.HasRequestedAction, Is.False);
            Assert.That(runtime.NativeInputProxy.ComboState[0], Is.EqualTo(5));
        }

        [Test]
        public void ConsumeAttempt_UsesExactNativeClearBoundary()
        {
            NTSDEntityRuntime runtime = CreateRuntime();
            for (int index = 0; index < 7; index++)
            {
                runtime.NativeInputProxy.EdgeWindow[index] = (byte)(10 + index);
                runtime.NativeInputProxy.Previous[index] = 1;
                runtime.NativeInputProxy.Current[index] = 1;
            }
            runtime.NativeInputProxy.DefendReentryCooldown = 9;
            for (int index = 0; index < 10; index++)
                runtime.NativeInputProxy.ComboState[index] = 1;
            runtime.NativeInputProxy.ProxyTail = 0x5A;
            for (int index = 1; index < runtime.InputHistory.Length; index++)
                runtime.InputHistory[index] = index;

            NTSD28NativeComboRouteSelector.ConsumeAttempt(runtime);

            Assert.That(runtime.NativeInputProxy.EdgeWindow.Take(6), Is.All.Zero);
            Assert.That(runtime.NativeInputProxy.EdgeWindow[6], Is.EqualTo(16));
            Assert.That(runtime.NativeInputProxy.DefendReentryCooldown, Is.Zero);
            Assert.That(runtime.NativeInputProxy.ComboState, Is.All.Zero);
            Assert.That(runtime.InputHistory.Skip(1), Is.All.EqualTo(-1));
            Assert.That(runtime.NativeInputProxy.Current, Is.All.EqualTo((byte)1));
            Assert.That(runtime.NativeInputProxy.Previous, Is.All.EqualTo((byte)1));
            Assert.That(runtime.NativeInputProxy.ProxyTail, Is.EqualTo(0x5A));
        }

        [Test]
        public void WarmSelection_AllocatesZeroManagedBytes()
        {
            NTSDEntityRuntime runtime = CreateRuntime();
            runtime.NativeInputProxy.ComboState[6] = 1;
            LF2FrameData frame = CreateFrame();
            NTSD28NativeComboRouteSelector.TrySelect(frame, runtime, out _);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
                NTSD28NativeComboRouteSelector.TrySelect(frame, runtime, out _);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static NTSDEntityRuntime CreateRuntime()
        {
            var runtime = new NTSDEntityRuntime();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(runtime);
            return runtime;
        }

        private static LF2FrameData CreateFrame()
        {
            return new LF2FrameData
            {
                hit_Fa = 101,
                hit_Fj = 102,
                hit_Ua = 103,
                hit_Uj = 104,
                hit_Da = 105,
                hit_Dj = 106,
                hit_aj = 107,
                hit_ad = 108,
                hit_ja = 109,
                hit_jd = 110,
            };
        }
    }
}
#endif
