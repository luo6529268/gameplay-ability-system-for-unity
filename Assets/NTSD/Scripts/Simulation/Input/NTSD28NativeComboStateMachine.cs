namespace NTSD.Simulation
{
    internal static class NTSD28NativeComboStateMachine
    {
        private const int DepthUp = 0;
        private const int DepthDown = 1;
        private const int Left = 2;
        private const int Right = 3;
        private const int Attack = 4;
        private const int Jump = 5;
        private const int Defend = 6;

        private const int EdgeAttack = 0;
        private const int EdgeJump = 1;
        private const int EdgeDefend = 2;
        private const int EdgeRight = 3;
        private const int EdgeLeft = 4;
        private const int EdgeUp = 5;
        private const int EdgeDown = 6;

        internal static void InitializeNativeHistory(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            if (runtime.InputHistory == null || runtime.InputHistory.Length != 6)
                runtime.InputHistory = new int[6];
            for (int index = 1; index < runtime.InputHistory.Length; index++)
                runtime.InputHistory[index] = -1;
        }

        internal static uint ProcessSampledInput(
            NTSDEntityRuntime runtime,
            bool suppressJumpEdge = false)
        {
            if (!HasCanonicalState(runtime))
                return 0;

            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            if (runtime.ObjType == 0 && runtime.HP <= 0)
            {
                input.Clear();
                InitializeNativeHistory(runtime);
                return 0;
            }

            for (int index = 0; index < input.EdgeWindow.Length; index++)
            {
                if (input.EdgeWindow[index] > 0)
                    input.EdgeWindow[index]--;
            }
            if (input.DefendReentryCooldown > 0)
                input.DefendReentryCooldown--;

            // Alignment contract: NTSD28-B2-NATIVE-COMBO-BRIDGE-001.
            uint mask = 0;
            mask = ProcessRisingEdge(runtime, Right, suppressJumpEdge, mask);
            mask = ProcessRisingEdge(runtime, Left, suppressJumpEdge, mask);
            mask = ProcessRisingEdge(runtime, DepthUp, suppressJumpEdge, mask);
            mask = ProcessRisingEdge(runtime, DepthDown, suppressJumpEdge, mask);
            mask = ProcessRisingEdge(runtime, Defend, suppressJumpEdge, mask);
            mask = ProcessRisingEdge(runtime, Jump, suppressJumpEdge, mask);
            mask = ProcessRisingEdge(runtime, Attack, suppressJumpEdge, mask);
            AdvanceCombos(runtime, mask);
            return mask;
        }

        internal static void ProjectExactStateToLegacy(NTSDEntityRuntime runtime)
        {
            if (!HasCanonicalState(runtime))
                return;

            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            runtime.KeyUp = input.Current[DepthUp];
            runtime.KeyDown = input.Current[DepthDown];
            runtime.KeyLeft = input.Current[Left];
            runtime.KeyRight = input.Current[Right];
            runtime.KeyJump = input.Current[Attack];
            runtime.KeyDefend = input.Current[Jump];
            runtime.KeyAttack = input.Current[Defend];

            runtime.PrevUp = input.Previous[DepthUp];
            runtime.PrevDown = input.Previous[DepthDown];
            runtime.PrevLeft = input.Previous[Left];
            runtime.PrevRight = input.Previous[Right];
            runtime.PrevJump = input.Previous[Attack];
            runtime.PrevDefend = input.Previous[Jump];
            runtime.PrevAttack = input.Previous[Defend];

            runtime.CdAttack = input.EdgeWindow[EdgeAttack];
            runtime.CdJump = input.EdgeWindow[EdgeJump];
            runtime.CdDefend = input.EdgeWindow[EdgeDefend];
            runtime.CdRight = input.EdgeWindow[EdgeRight];
            runtime.CdLeft = input.EdgeWindow[EdgeLeft];
            runtime.CdUp = input.EdgeWindow[EdgeUp];
            runtime.CdDown = input.EdgeWindow[EdgeDown];
            runtime.CdDefendLock = input.DefendReentryCooldown;

            byte[] combo = input.ComboState;
            runtime.ComboDra = combo[0] == 4 ? (byte)3 : (byte)0;
            runtime.ComboDla = combo[0] >= 5 ? (byte)3 : (byte)0;
            runtime.ComboDrj = combo[1] == 4 ? (byte)3 : (byte)0;
            runtime.ComboDlj = combo[1] >= 5 ? (byte)3 : (byte)0;
            runtime.ComboDua = combo[2] == 3 ? (byte)3 : (byte)0;
            runtime.ComboDuj = combo[3] == 3 ? (byte)3 : (byte)0;
            runtime.ComboDda = combo[4] == 3 ? (byte)3 : (byte)0;
            runtime.ComboDdj = combo[5] == 3 ? (byte)3 : (byte)0;
            runtime.ComboDja = combo[8] == 1 ? (byte)3 : (byte)0;
        }

        internal static void ClearComboAttempt(NTSDEntityRuntime runtime)
        {
            if (!HasCanonicalState(runtime))
                return;

            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            for (int index = EdgeAttack; index <= EdgeUp; index++)
                input.EdgeWindow[index] = 0;
            input.DefendReentryCooldown = 0;
            for (int index = 0; index < input.ComboState.Length; index++)
                input.ComboState[index] = 0;
            for (int index = 1; index < runtime.InputHistory.Length; index++)
                runtime.InputHistory[index] = -1;
        }

        private static bool HasCanonicalState(NTSDEntityRuntime runtime)
        {
            return runtime != null &&
                   runtime.InputHistory != null &&
                   runtime.InputHistory.Length == 6 &&
                   runtime.NativeInputProxy != null &&
                   runtime.NativeInputProxy.HasCanonicalStorage;
        }

        private static uint ProcessRisingEdge(
            NTSDEntityRuntime runtime,
            int key,
            bool suppressJumpEdge,
            uint mask)
        {
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            if (input.Previous[key] != 0 || input.Current[key] == 0 ||
                (key == Jump && suppressJumpEdge))
            {
                return mask;
            }

            input.EdgeWindow[EdgeIndex(key)] = 5;
            PushHistory(runtime.InputHistory, HistoryCode(key));
            return mask | Bit(key);
        }

        private static void AdvanceCombos(NTSDEntityRuntime runtime, uint mask)
        {
            if (mask == 0)
                return;

            byte[] combo = runtime.NativeInputProxy.ComboState;
            if ((mask & Bit(Defend)) != 0)
            {
                for (int index = 0; index < 6; index++)
                    combo[index] = 1;
            }

            AdvanceHorizontal(runtime.NativeInputProxy, 0, Attack, mask);
            AdvanceHorizontal(runtime.NativeInputProxy, 1, Jump, mask);
            AdvanceDepth(runtime.NativeInputProxy, 2, DepthUp, Attack, mask);
            AdvanceDepth(runtime.NativeInputProxy, 3, DepthUp, Jump, mask);
            AdvanceDepth(runtime.NativeInputProxy, 4, DepthDown, Attack, mask);
            AdvanceDepth(runtime.NativeInputProxy, 5, DepthDown, Jump, mask);

            int[] history = runtime.InputHistory;
            combo[6] = history[4] == 5 && history[5] == 0 ? (byte)1 : (byte)0;
            if (combo[6] != 0)
            {
                SelectCombo(combo, 6);
                return;
            }

            combo[7] = history[4] == 5 && history[5] == 9 ? (byte)1 : (byte)0;
            if (combo[7] != 0)
                return;

            combo[9] = history[4] == 0 && history[5] == 9 ? (byte)1 : (byte)0;
            if (combo[9] != 0)
                return;

            if (history[3] != 9)
            {
                combo[8] = 0;
                combo[9] = 0;
                runtime.NativeInputProxy.ProxyTail = 0;
                return;
            }

            combo[8] = history[4] == 0 && history[5] == 5 ? (byte)1 : (byte)0;
            if (combo[8] != 0)
                SelectCombo(combo, 8);
        }

        private static void AdvanceHorizontal(
            NTSD28InputProxyBlock input,
            int comboIndex,
            int terminal,
            uint mask)
        {
            byte state = input.ComboState[comboIndex];
            if (state == 0)
                return;

            bool directionAdvanced = false;
            uint horizontal = mask & (Bit(Left) | Bit(Right));
            if (state == 1)
            {
                bool exactlyOne = horizontal == Bit(Left) || horizontal == Bit(Right);
                uint allowed = horizontal | (mask & Bit(terminal)) | (mask & Bit(Defend));
                if (exactlyOne && allowed == mask)
                {
                    state = horizontal == Bit(Left) ? (byte)3 : (byte)2;
                    directionAdvanced = true;
                }
                else if ((mask & ~(Bit(Defend) | Bit(terminal))) != 0)
                {
                    state = 0;
                }

                input.ComboState[comboIndex] = state;
                if (!directionAdvanced || (mask & Bit(terminal)) == 0)
                    return;
            }

            if (state == 2 || state == 3)
            {
                uint acceptedDirection = state == 2 ? Bit(Right) : Bit(Left);
                uint allowed = Bit(terminal) |
                               (mask & Bit(Defend)) |
                               (directionAdvanced ? acceptedDirection : 0u);
                if ((mask & Bit(terminal)) != 0 && allowed == mask)
                {
                    state = (byte)(state + 2);
                    input.ComboState[comboIndex] = state;
                    SelectCombo(input.ComboState, comboIndex);
                }
                else if ((mask & ~Bit(Defend)) != 0)
                {
                    input.ComboState[comboIndex] = 0;
                }
            }
        }

        private static void AdvanceDepth(
            NTSD28InputProxyBlock input,
            int comboIndex,
            int direction,
            int terminal,
            uint mask)
        {
            byte state = input.ComboState[comboIndex];
            if (state == 0)
                return;

            bool directionAdvanced = false;
            if (state == 1)
            {
                uint allowed = Bit(direction) |
                               (mask & Bit(terminal)) |
                               (mask & Bit(Defend));
                if ((mask & Bit(direction)) != 0 && allowed == mask)
                {
                    state = 2;
                    directionAdvanced = true;
                }
                else if ((mask & ~(Bit(Defend) | Bit(terminal))) != 0)
                {
                    state = 0;
                }

                input.ComboState[comboIndex] = state;
                if (!directionAdvanced || (mask & Bit(terminal)) == 0)
                    return;
            }

            if (state == 2)
            {
                uint allowed = Bit(terminal) |
                               (mask & Bit(Defend)) |
                               (directionAdvanced ? Bit(direction) : 0u);
                if ((mask & Bit(terminal)) != 0 && allowed == mask)
                {
                    input.ComboState[comboIndex] = 3;
                    SelectCombo(input.ComboState, comboIndex);
                }
                else if ((mask & ~Bit(Defend)) != 0)
                {
                    input.ComboState[comboIndex] = 0;
                }
            }
        }

        private static void SelectCombo(byte[] combo, int selected)
        {
            for (int index = 0; index < combo.Length; index++)
            {
                if (index != selected)
                    combo[index] = 0;
            }
        }

        private static void PushHistory(int[] history, int code)
        {
            history[1] = history[2];
            history[2] = history[3];
            history[3] = history[4];
            history[4] = history[5];
            history[5] = code;
        }

        private static int EdgeIndex(int key)
        {
            return key switch
            {
                Attack => EdgeAttack,
                Jump => EdgeJump,
                Defend => EdgeDefend,
                Right => EdgeRight,
                Left => EdgeLeft,
                DepthUp => EdgeUp,
                DepthDown => EdgeDown,
                _ => EdgeDown,
            };
        }

        private static int HistoryCode(int key)
        {
            return key switch
            {
                Jump => 0,
                DepthDown => 2,
                Left => 4,
                Attack => 5,
                Right => 6,
                DepthUp => 8,
                Defend => 9,
                _ => -1,
            };
        }

        private static uint Bit(int key)
        {
            return 1u << key;
        }
    }
}
