#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeComboStateMachineEditorTests
    {
        [Test]
        public void AllRisingEdges_UseNativeOrderAndFiveEntryHistory()
        {
            var runtime = CreateRuntime();
            for (int index = 0; index < runtime.NativeInputProxy.EdgeWindow.Length; index++)
                runtime.NativeInputProxy.EdgeWindow[index] = 2;
            runtime.NativeInputProxy.DefendReentryCooldown = 2;
            SetCurrent(runtime, 0, 1, 2, 3, 4, 5, 6);

            uint mask = NTSD28NativeComboStateMachine.ProcessSampledInput(runtime);

            Assert.That(mask, Is.EqualTo(0x7Fu));
            Assert.That(runtime.NativeInputProxy.EdgeWindow,
                Is.All.EqualTo((byte)5));
            Assert.That(runtime.NativeInputProxy.DefendReentryCooldown,
                Is.EqualTo(1));
            Assert.That(runtime.InputHistory.Skip(1),
                Is.EqualTo(new[] { 8, 2, 9, 0, 5 }));
        }

        [TestCase(3, 4, 0, 4)]
        [TestCase(2, 4, 0, 5)]
        [TestCase(3, 5, 1, 4)]
        [TestCase(2, 5, 1, 5)]
        [TestCase(0, 4, 2, 3)]
        [TestCase(0, 5, 3, 3)]
        [TestCase(1, 4, 4, 3)]
        [TestCase(1, 5, 5, 3)]
        public void DirectionAndTerminal_MayCompleteInOneNativeSample(
            int direction,
            int terminal,
            int combo,
            int expectedState)
        {
            var runtime = CreateRuntime();
            SetCurrent(runtime, 6, direction, terminal);

            NTSD28NativeComboStateMachine.ProcessSampledInput(runtime);

            Assert.That(runtime.NativeInputProxy.ComboState[combo],
                Is.EqualTo(expectedState));
            Assert.That(runtime.NativeInputProxy.ComboState
                .Where((_, index) => index != combo), Is.All.Zero);
        }

        [Test]
        public void EarlyTerminal_DoesNotCancelArmedDirectionalCombo()
        {
            var runtime = CreateRuntime();
            Sample(runtime, 6);
            Sample(runtime, 4);
            Assert.That(runtime.NativeInputProxy.ComboState[0], Is.EqualTo(1));

            Sample(runtime);
            Sample(runtime, 3);
            Assert.That(runtime.NativeInputProxy.ComboState[0], Is.EqualTo(2));

            Sample(runtime);
            Sample(runtime, 4);
            Assert.That(runtime.NativeInputProxy.ComboState[0], Is.EqualTo(4));
        }

        [Test]
        public void SuppressedJumpEdge_LeavesSamplesIntactWithoutHistoryOrWindow()
        {
            var runtime = CreateRuntime();
            SetCurrent(runtime, 5);

            uint mask = NTSD28NativeComboStateMachine.ProcessSampledInput(
                runtime,
                suppressJumpEdge: true);

            Assert.That(mask, Is.Zero);
            Assert.That(runtime.NativeInputProxy.Current[5], Is.EqualTo(1));
            Assert.That(runtime.NativeInputProxy.Previous[5], Is.Zero);
            Assert.That(runtime.NativeInputProxy.EdgeWindow[1], Is.Zero);
            Assert.That(runtime.InputHistory.Skip(1), Is.All.EqualTo(-1));
        }

        [TestCase(new int[] { 4, 5 }, 6)]
        [TestCase(new int[] { 4, 6 }, 7)]
        [TestCase(new int[] { 6, 5, 4 }, 8)]
        [TestCase(new int[] { 5, 6 }, 9)]
        public void HistoryCombos_UseNativeCodesAndPriority(int[] keys, int combo)
        {
            var runtime = CreateRuntime();
            for (int index = 0; index < keys.Length; index++)
            {
                Sample(runtime, keys[index]);
                if (index + 1 < keys.Length)
                    Sample(runtime);
            }

            Assert.That(runtime.NativeInputProxy.ComboState[combo], Is.EqualTo(1));
        }

        [Test]
        public void JThenL_PreservesLaterProxiedComboState()
        {
            var runtime = CreateRuntime();
            Sample(runtime, 4);
            Sample(runtime);
            runtime.NativeInputProxy.ComboState[9] = 1;

            Sample(runtime, 6);

            Assert.That(runtime.NativeInputProxy.ComboState[7], Is.EqualTo(1));
            Assert.That(runtime.NativeInputProxy.ComboState[9], Is.EqualTo(1));
        }

        [Test]
        public void HistoryMiss_ClearsLkjKldStateAndProxyTail()
        {
            var runtime = CreateRuntime();
            runtime.NativeInputProxy.ComboState[8] = 1;
            runtime.NativeInputProxy.ComboState[9] = 1;
            runtime.NativeInputProxy.ProxyTail = 9;

            Sample(runtime, 3);

            Assert.That(runtime.NativeInputProxy.ComboState[8], Is.Zero);
            Assert.That(runtime.NativeInputProxy.ComboState[9], Is.Zero);
            Assert.That(runtime.NativeInputProxy.ProxyTail, Is.Zero);
        }

        [Test]
        public void ClearComboAttempt_PreservesOnlyNativeDownEdgeAndProxyTail()
        {
            var runtime = CreateRuntime();
            for (int index = 0; index < 7; index++)
            {
                runtime.NativeInputProxy.EdgeWindow[index] = (byte)(10 + index);
                runtime.NativeInputProxy.Previous[index] = 1;
                runtime.NativeInputProxy.Current[index] = 1;
            }
            runtime.NativeInputProxy.DefendReentryCooldown = 7;
            for (int index = 0; index < 10; index++)
                runtime.NativeInputProxy.ComboState[index] = 1;
            runtime.NativeInputProxy.ProxyTail = 8;
            for (int index = 1; index < 6; index++)
                runtime.InputHistory[index] = index;

            NTSD28NativeComboStateMachine.ClearComboAttempt(runtime);

            Assert.That(runtime.NativeInputProxy.EdgeWindow.Take(6), Is.All.Zero);
            Assert.That(runtime.NativeInputProxy.EdgeWindow[6], Is.EqualTo(16));
            Assert.That(runtime.NativeInputProxy.DefendReentryCooldown, Is.Zero);
            Assert.That(runtime.NativeInputProxy.ComboState, Is.All.Zero);
            Assert.That(runtime.InputHistory.Skip(1), Is.All.EqualTo(-1));
            Assert.That(runtime.NativeInputProxy.Previous, Is.All.EqualTo((byte)1));
            Assert.That(runtime.NativeInputProxy.Current, Is.All.EqualTo((byte)1));
            Assert.That(runtime.NativeInputProxy.ProxyTail, Is.EqualTo(8));
        }

        [Test]
        public void LegacyProjection_MapsOnlyProvenCurrentEdgeAndComboFields()
        {
            var runtime = CreateRuntime();
            byte[] current = runtime.NativeInputProxy.Current;
            current[0] = current[2] = current[4] = current[6] = 1;
            byte[] previous = runtime.NativeInputProxy.Previous;
            previous[1] = previous[3] = previous[5] = 1;
            for (int index = 0; index < 7; index++)
                runtime.NativeInputProxy.EdgeWindow[index] = (byte)(10 + index);
            runtime.NativeInputProxy.DefendReentryCooldown = 17;
            byte[] combo = runtime.NativeInputProxy.ComboState;
            combo[0] = 4;
            combo[1] = 5;
            combo[2] = combo[3] = combo[4] = combo[5] = 3;
            combo[6] = combo[7] = combo[8] = combo[9] = 1;

            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(runtime);

            Assert.That(new[]
            {
                runtime.KeyUp, runtime.KeyDown, runtime.KeyLeft, runtime.KeyRight,
                runtime.KeyJump, runtime.KeyDefend, runtime.KeyAttack,
            }, Is.EqualTo(new byte[] { 1, 0, 1, 0, 1, 0, 1 }));
            Assert.That(new[]
            {
                runtime.PrevUp, runtime.PrevDown, runtime.PrevLeft, runtime.PrevRight,
                runtime.PrevJump, runtime.PrevDefend, runtime.PrevAttack,
            }, Is.EqualTo(new byte[] { 0, 1, 0, 1, 0, 1, 0 }));
            Assert.That(new[]
            {
                runtime.CdAttack, runtime.CdJump, runtime.CdDefend,
                runtime.CdRight, runtime.CdLeft, runtime.CdUp, runtime.CdDown,
                runtime.CdDefendLock,
            }, Is.EqualTo(new byte[] { 10, 11, 12, 13, 14, 15, 16, 17 }));
            Assert.That(new[]
            {
                runtime.ComboDra, runtime.ComboDla,
                runtime.ComboDrj, runtime.ComboDlj,
                runtime.ComboDua, runtime.ComboDuj,
                runtime.ComboDda, runtime.ComboDdj, runtime.ComboDja,
            }, Is.EqualTo(new byte[] { 3, 0, 0, 3, 3, 3, 3, 3, 3 }));
        }

        [Test]
        public void LegacyProjection_DoesNotAliasNativeAjAdOrJd()
        {
            var runtime = CreateRuntime();
            runtime.NativeInputProxy.ComboState[6] = 1;
            runtime.NativeInputProxy.ComboState[7] = 1;
            runtime.NativeInputProxy.ComboState[9] = 1;

            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(runtime);

            Assert.That(new[]
            {
                runtime.ComboDra, runtime.ComboDla,
                runtime.ComboDrj, runtime.ComboDlj,
                runtime.ComboDua, runtime.ComboDuj,
                runtime.ComboDda, runtime.ComboDdj, runtime.ComboDja,
            }, Is.All.Zero);
        }

        [Test]
        public void WarmProcessAndProjection_AllocateZeroManagedBytes()
        {
            var runtime = CreateRuntime();
            NTSD28NativeComboStateMachine.ProcessSampledInput(runtime);
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(runtime);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                NTSD28NativeComboStateMachine.ProcessSampledInput(runtime);
                NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(runtime);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static NTSDEntityRuntime CreateRuntime()
        {
            var runtime = new NTSDEntityRuntime();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(runtime);
            return runtime;
        }

        private static void Sample(NTSDEntityRuntime runtime, params int[] keys)
        {
            Array.Copy(runtime.NativeInputProxy.Current,
                runtime.NativeInputProxy.Previous,
                NTSD28InputProxyBlock.InputKeyCount);
            SetCurrent(runtime, keys);
            NTSD28NativeComboStateMachine.ProcessSampledInput(runtime);
        }

        private static void SetCurrent(NTSDEntityRuntime runtime, params int[] keys)
        {
            Array.Clear(runtime.NativeInputProxy.Current,
                0,
                runtime.NativeInputProxy.Current.Length);
            for (int index = 0; index < keys.Length; index++)
                runtime.NativeInputProxy.Current[keys[index]] = 1;
        }
    }
}
#endif
