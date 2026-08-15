using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal struct BattleCharacterInputActionState
    {
        internal byte CdAttack;
        internal byte CdJump;
        internal byte CdDefend;
        internal byte CdDefendLock;
        internal byte CdRight;
        internal byte CdLeft;
        internal byte CdUp;
        internal byte CdDown;
        internal byte ComboDra;
        internal byte ComboDla;
        internal byte ComboDua;
        internal byte ComboDda;
        internal byte ComboDrj;
        internal byte ComboDlj;
        internal byte ComboDuj;
        internal byte ComboDdj;
        internal byte ComboDja;

        internal bool ContentEquals(in BattleCharacterInputActionState other)
        {
            return CdAttack == other.CdAttack &&
                   CdJump == other.CdJump &&
                   CdDefend == other.CdDefend &&
                   CdDefendLock == other.CdDefendLock &&
                   CdRight == other.CdRight &&
                   CdLeft == other.CdLeft &&
                   CdUp == other.CdUp &&
                   CdDown == other.CdDown &&
                   ComboDra == other.ComboDra &&
                   ComboDla == other.ComboDla &&
                   ComboDua == other.ComboDua &&
                   ComboDda == other.ComboDda &&
                   ComboDrj == other.ComboDrj &&
                   ComboDlj == other.ComboDlj &&
                   ComboDuj == other.ComboDuj &&
                   ComboDdj == other.ComboDdj &&
                   ComboDja == other.ComboDja;
        }
    }

    /// <summary>
    /// Resolves combo and direct-frame input against a value snapshot. The
    /// resolver owns no entity-local state; callers publish the resulting
    /// progress through <see cref="BattleCharacterInputWriter"/>.
    /// </summary>
    internal readonly struct BattleCharacterInputActionResolver
    {
        internal bool ApplyFrameInput(
            LF2Entity character,
            ref BattleCharacterInputActionState input,
            BattleAiInputDetailDiagnostics diagnostics = null)
        {
            if (character == null || character.Frame?.D == null)
                return false;

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.ActionComboDirectResolve);
            bool result;
            try
            {
                result = ApplyComboFrameInput(character, ref input);
                result |= ApplyDirectFrameInput(character, ref input);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.ActionComboDirectResolve);
            }

            if (character is LF2Character realCharacter)
            {
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.ActionReleaseResolve);
                try
                {
                    result |= realCharacter.ProcessReleaseInput();
                }
                finally
                {
                    diagnostics?.EndPhase(BattleAiInputDetailPhase.ActionReleaseResolve);
                }
            }
            return result;
        }

        internal bool ApplyFrameInputFromRuntimeProgress(
            LF2Entity character,
            BattleCharacterInputWriter writer,
            BattleAiInputDetailDiagnostics diagnostics = null)
        {
            NTSDEntityRuntime runtime = character?.Runtime;
            if (runtime == null || writer == null)
                return false;

            BattleCharacterInputActionState input;
            bool capturedCanonical;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.ActionProgressCapture);
            try
            {
                capturedCanonical = writer.TryCaptureProgressState(runtime, out input);
                if (!capturedCanonical)
                    input = CaptureProgress(runtime);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.ActionProgressCapture);
            }
            BattleCharacterInputActionState original = input;
            bool result = ApplyFrameInput(character, ref input, diagnostics);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.ActionProgressCommit);
            try
            {
                writer.CommitResolvedProgressState(
                    runtime,
                    original,
                    input,
                    capturedCanonical);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.ActionProgressCommit);
            }
            return result;
        }

        private bool ApplyComboFrameInput(
            LF2Entity character,
            ref BattleCharacterInputActionState input)
        {
            if (input.CdDefend != 5 &&
                input.ComboDra == 0 &&
                input.ComboDla == 0 &&
                input.ComboDua == 0 &&
                input.ComboDda == 0 &&
                input.ComboDrj == 0 &&
                input.ComboDlj == 0 &&
                input.ComboDuj == 0 &&
                input.ComboDdj == 0 &&
                input.ComboDja == 0)
            {
                return false;
            }

            byte comboDra = input.ComboDra;
            byte comboDla = input.ComboDla;
            byte comboDua = input.ComboDua;
            byte comboDda = input.ComboDda;
            byte comboDrj = input.ComboDrj;
            byte comboDlj = input.ComboDlj;
            byte comboDuj = input.ComboDuj;
            byte comboDdj = input.ComboDdj;
            byte comboDja = input.ComboDja;
            bool result = false;

            result |= RunCombo(
                character,
                ref comboDra,
                input.CdRight,
                ComboMode.Right,
                input.CdAttack,
                ComboMode.Attack,
                character.Frame.D.hit_Fa,
                "right",
                ref input);
            result |= RunCombo(
                character,
                ref comboDla,
                input.CdLeft,
                ComboMode.Left,
                input.CdAttack,
                ComboMode.Attack,
                character.Frame.D.hit_Fa,
                "left",
                ref input);
            result |= RunCombo(
                character,
                ref comboDua,
                input.CdUp,
                ComboMode.Up,
                input.CdAttack,
                ComboMode.Attack,
                character.Frame.D.hit_Ua,
                null,
                ref input);
            result |= RunCombo(
                character,
                ref comboDda,
                input.CdDown,
                ComboMode.Down,
                input.CdAttack,
                ComboMode.Attack,
                character.Frame.D.hit_Da,
                null,
                ref input);
            result |= RunCombo(
                character,
                ref comboDrj,
                input.CdRight,
                ComboMode.Right,
                input.CdJump,
                ComboMode.Jump,
                character.Frame.D.hit_Fj,
                "right",
                ref input);
            result |= RunCombo(
                character,
                ref comboDlj,
                input.CdLeft,
                ComboMode.Left,
                input.CdJump,
                ComboMode.Jump,
                character.Frame.D.hit_Fj,
                "left",
                ref input);
            result |= RunCombo(
                character,
                ref comboDuj,
                input.CdUp,
                ComboMode.Up,
                input.CdJump,
                ComboMode.Jump,
                character.Frame.D.hit_Uj,
                null,
                ref input);
            result |= RunCombo(
                character,
                ref comboDdj,
                input.CdDown,
                ComboMode.Down,
                input.CdJump,
                ComboMode.Jump,
                character.Frame.D.hit_Dj,
                null,
                ref input);

            bool djaAdvanced = false;
            AdvanceCombo(
                ref comboDja,
                input.CdJump,
                ComboMode.Jump,
                input.CdAttack,
                ref djaAdvanced,
                in input);
            if (character.Frame?.D == null || comboDja != 3)
                return result;

            int targetFrame = character.Frame.D.hit_ja;
            if (character.ShouldHoldCharacterDatDjaInputGuard(targetFrame))
                return result;

            if (targetFrame != 0 && character.CanEnterCharacterDatInputFrameJump())
            {
                bool jumped = character.TryCharacterDatInputFrameJump(targetFrame);
                comboDja = 0;
                if (jumped)
                    ClearActionAndDirectionCooldowns(ref input);
                return true;
            }

            if (character.Runtime?.Unk328 == 1)
            {
                character.Runtime.Unk338 = 0;
                return result;
            }

            if (ComboInterrupted(ComboMode.Attack, djaAdvanced, in input))
                comboDja = 0;

            input.ComboDra = comboDra;
            input.ComboDla = comboDla;
            input.ComboDua = comboDua;
            input.ComboDda = comboDda;
            input.ComboDrj = comboDrj;
            input.ComboDlj = comboDlj;
            input.ComboDuj = comboDuj;
            input.ComboDdj = comboDdj;
            input.ComboDja = comboDja;
            return result;
        }

        private bool RunCombo(
            LF2Entity character,
            ref byte comboState,
            byte step2Cooldown,
            ComboMode step2Mode,
            byte step3Cooldown,
            ComboMode finalMode,
            int targetFrame,
            string facing,
            ref BattleCharacterInputActionState input)
        {
            bool advanced = false;
            AdvanceCombo(
                ref comboState,
                step2Cooldown,
                step2Mode,
                step3Cooldown,
                ref advanced,
                in input);
            if (comboState != 3)
                return false;

            if (targetFrame != 0 && character.Runtime.LinkState != 2)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(targetFrame);
                if (!string.IsNullOrEmpty(facing))
                    character.SwitchDir(facing);
                comboState = 0;
                if (jumped)
                    ClearActionAndDirectionCooldowns(ref input);
                return true;
            }

            if (ComboInterrupted(finalMode, advanced, in input))
                comboState = 0;
            return false;
        }

        private void AdvanceCombo(
            ref byte comboState,
            byte step2Cooldown,
            ComboMode step2Mode,
            byte step3Cooldown,
            ref bool advanced,
            in BattleCharacterInputActionState input)
        {
            advanced = false;
            if (comboState == 0 && input.CdDefend == 5)
            {
                comboState = 1;
                advanced = true;
            }

            if (comboState == 1)
            {
                if (step2Cooldown == 5)
                {
                    comboState = 2;
                    advanced = true;
                }
                else if (ComboInterrupted(ComboMode.Defend, advanced, in input))
                {
                    comboState = 0;
                }
            }

            if (comboState == 2)
            {
                if (step3Cooldown == 5)
                {
                    comboState = 3;
                    advanced = true;
                }
                else if (ComboInterrupted(step2Mode, advanced, in input))
                {
                    comboState = 0;
                }
            }
        }

        private bool ApplyDirectFrameInput(
            LF2Entity character,
            ref BattleCharacterInputActionState input)
        {
            if (character.Frame?.D == null)
                return false;

            if (character.Frame.D.hit_a != 0 &&
                input.CdAttack > input.CdDefend &&
                input.CdAttack > input.CdJump)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(
                    character.Frame.D.hit_a);
                if (jumped)
                    ClearActionAndDirectionCooldowns(ref input);
                input.CdAttack = 0;
                return true;
            }

            if (character.Frame.D.hit_d != 0 &&
                input.CdDefend > input.CdAttack &&
                input.CdDefend > input.CdJump)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(
                    character.Frame.D.hit_d);
                if (jumped)
                    ClearActionAndDirectionCooldowns(ref input);
                input.CdDefend = 0;
                return true;
            }

            if (character.Frame.D.hit_j != 0 &&
                input.CdJump > input.CdAttack &&
                input.CdJump > input.CdDefend)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(
                    character.Frame.D.hit_j);
                if (jumped)
                    ClearActionAndDirectionCooldowns(ref input);
                input.CdJump = 0;
                return true;
            }

            return false;
        }

        private bool ComboInterrupted(
            ComboMode mode,
            bool advancedThisWrapper,
            in BattleCharacterInputActionState input)
        {
            if (!advancedThisWrapper)
            {
                return IsFreshAttackOrJumpOrDefend(in input) ||
                       IsFreshDirection(in input);
            }

            return mode switch
            {
                ComboMode.Up =>
                    IsFreshAttackOrJumpOrDefend(in input) ||
                    input.CdLeft == 5 || input.CdDown == 5 || input.CdRight == 5,
                ComboMode.Down =>
                    IsFreshAttackOrJumpOrDefend(in input) ||
                    input.CdLeft == 5 || input.CdUp == 5 || input.CdRight == 5,
                ComboMode.Left =>
                    IsFreshAttackOrJumpOrDefend(in input) ||
                    input.CdUp == 5 || input.CdDown == 5 || input.CdRight == 5,
                ComboMode.Right =>
                    IsFreshAttackOrJumpOrDefend(in input) ||
                    input.CdLeft == 5 || input.CdUp == 5 || input.CdDown == 5,
                ComboMode.Defend =>
                    input.CdAttack == 5 || input.CdJump == 5 ||
                    IsFreshDirection(in input),
                ComboMode.Jump =>
                    input.CdAttack == 5 || input.CdDefend == 5 ||
                    IsFreshDirection(in input),
                ComboMode.Attack =>
                    input.CdJump == 5 || input.CdDefend == 5 ||
                    IsFreshDirection(in input),
                _ => false,
            };
        }

        private bool IsFreshAttackOrJumpOrDefend(
            in BattleCharacterInputActionState input)
        {
            return input.CdAttack == 5 ||
                   input.CdJump == 5 ||
                   input.CdDefend == 5;
        }

        private bool IsFreshDirection(in BattleCharacterInputActionState input)
        {
            return input.CdLeft == 5 ||
                   input.CdUp == 5 ||
                   input.CdDown == 5 ||
                   input.CdRight == 5;
        }

        private void ClearActionAndDirectionCooldowns(
            ref BattleCharacterInputActionState input)
        {
            input.CdRight = 0;
            input.CdLeft = 0;
            input.CdUp = 0;
            input.CdDown = 0;
            input.CdAttack = 0;
            input.CdJump = 0;
            input.CdDefend = 0;
        }

        private BattleCharacterInputActionState CaptureProgress(NTSDEntityRuntime runtime)
        {
            return new BattleCharacterInputActionState
            {
                CdAttack = runtime.CdAttack,
                CdJump = runtime.CdJump,
                CdDefend = runtime.CdDefend,
                CdDefendLock = runtime.CdDefendLock,
                CdRight = runtime.CdRight,
                CdLeft = runtime.CdLeft,
                CdUp = runtime.CdUp,
                CdDown = runtime.CdDown,
                ComboDra = runtime.ComboDra,
                ComboDla = runtime.ComboDla,
                ComboDua = runtime.ComboDua,
                ComboDda = runtime.ComboDda,
                ComboDrj = runtime.ComboDrj,
                ComboDlj = runtime.ComboDlj,
                ComboDuj = runtime.ComboDuj,
                ComboDdj = runtime.ComboDdj,
                ComboDja = runtime.ComboDja,
            };
        }

        private enum ComboMode
        {
            Up,
            Down,
            Left,
            Right,
            Defend,
            Jump,
            Attack,
        }
    }
}
