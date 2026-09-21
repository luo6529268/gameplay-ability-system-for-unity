using System.Collections.Generic;
using NativeLinkedActionField = NTSD.Simulation.BattleNativeLinkedWeaponActionField;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;

namespace NTSD.Simulation.Ecs
{
    internal enum NTSD28NativeActionFailure : byte
    {
        None = 0,
        InvalidEntity = 1,
        ActionLocked = 2,
        UnrepresentableMagnitude = 3,
        SourceFrameMissing = 4,
        ResourceRejected = 5,
        ActionWriteFailed = 6,
    }

    internal readonly struct NTSD28NativeActionAttempt
    {
        internal NTSD28NativeActionAttempt(
            bool attempted,
            bool applied,
            bool usedFallback,
            int requestedAction,
            int resolvedAction,
            int mpCost,
            int hpCost,
            int effectiveMaxHpCost,
            NTSD28NativeActionFailure failure)
        {
            Attempted = attempted;
            Applied = applied;
            UsedFallback = usedFallback;
            RequestedAction = requestedAction;
            ResolvedAction = resolvedAction;
            MpCost = mpCost;
            HpCost = hpCost;
            EffectiveMaxHpCost = effectiveMaxHpCost;
            Failure = failure;
        }

        internal bool Attempted { get; }
        internal bool Applied { get; }
        internal bool UsedFallback { get; }
        internal int RequestedAction { get; }
        internal int ResolvedAction { get; }
        internal int MpCost { get; }
        internal int HpCost { get; }
        internal int EffectiveMaxHpCost { get; }
        internal NTSD28NativeActionFailure Failure { get; }
    }

    internal readonly struct NTSD28NativeComboActionResult
    {
        internal NTSD28NativeComboActionResult(
            bool selected,
            bool attempted,
            bool applied,
            bool consumed,
            NTSD28NativeComboField field,
            NTSD28NativeActionAttempt action)
        {
            Selected = selected;
            Attempted = attempted;
            Applied = applied;
            Consumed = consumed;
            Field = field;
            Action = action;
        }

        internal bool Selected { get; }
        internal bool Attempted { get; }
        internal bool Applied { get; }
        internal bool Consumed { get; }
        internal NTSD28NativeComboField Field { get; }
        internal NTSD28NativeActionAttempt Action { get; }
    }

    internal readonly struct NTSD28NativeFieldRoutingResult
    {
        internal NTSD28NativeFieldRoutingResult(
            int attemptCount,
            int appliedCount)
        {
            AttemptCount = attemptCount;
            AppliedCount = appliedCount;
        }

        internal int AttemptCount { get; }
        internal int AppliedCount { get; }
    }

    /// <summary>
    /// Owns the world-bound character action transaction entered after input
    /// resolution. The compatibility implementations still live on the entity
    /// adapters while U6 migrates their storage, but production callers cross
    /// this single composition boundary before mutating frame, facing, motion,
    /// HP/PP or action statistics.
    /// </summary>
    internal sealed class BattleCharacterActionWriter
    {
        private const int KeyDepthUp = 0;
        private const int KeyDepthDown = 1;
        private const int KeyLeft = 2;
        private const int KeyRight = 3;
        private const int KeyAttack = 4;
        private const int KeyJump = 5;
        private const int KeyDefend = 6;

        private const int EdgeAttack = 0;
        private const int EdgeJump = 1;
        private const int EdgeDefend = 2;
        private const int EdgeRight = 3;
        private const int EdgeLeft = 4;
        private const int EdgeDepthUp = 5;
        private const int EdgeDepthDown = 6;

        private readonly LF2CharacterActionResolver releaseInputResolver =
            new LF2CharacterActionResolver();

        private enum NativeInputFrameField : byte
        {
            HitAttack,
            HitDefend,
            HitJump,
            HoldAttack,
            HoldDefend,
            HoldJump,
            HitForward,
            HitBack,
            HitDepthUp,
            HitDepthDown,
            HoldForward,
            HoldBack,
            HoldDepthUp,
            HoldDepthDown,
        }

        internal bool TryCharacterDatInputFrameJump(
            LF2Entity character,
            int frameId)
        {
            return ApplyNativeInputAction(character, frameId).Applied;
        }

        internal bool ProcessReleaseInput(LF2Character character)
        {
            if (character == null)
                return false;

            return releaseInputResolver.ProcessReleaseInput(character);
        }

        internal NTSD28NativeActionAttempt ApplyNativeInputAction(
            LF2Entity character,
            int requestedAction)
        {
            return ApplyNativeInputActionCore(character, requestedAction);
        }

        internal static NTSD28NativeActionAttempt ApplyNativeInputActionCore(
            LF2Entity character,
            int requestedAction)
        {
            NTSDEntityRuntime runtime = character?.Runtime;
            if (runtime == null || character.Frame == null || character.Health == null)
            {
                return Result(
                    requestedAction,
                    requestedAction,
                    failure: NTSD28NativeActionFailure.InvalidEntity);
            }
            if (runtime.InputActionLock130 > 0)
            {
                return Result(
                    requestedAction,
                    requestedAction,
                    failure: NTSD28NativeActionFailure.ActionLocked);
            }
            if (requestedAction == int.MinValue)
            {
                return Result(
                    requestedAction,
                    requestedAction,
                    failure: NTSD28NativeActionFailure.UnrepresentableMagnitude);
            }

            int targetAction = requestedAction < 0
                ? -requestedAction
                : requestedAction;
            if (targetAction == 999)
                targetAction = 0;
            LF2FrameData sourceFrame = character.FrameCache?.GetNativeFrameDataById(targetAction);
            if (sourceFrame == null)
            {
                return Result(
                    requestedAction,
                    targetAction,
                    failure: NTSD28NativeActionFailure.SourceFrameMissing);
            }
            int encodedState = sourceFrame.state;
            if (encodedState >= 1000000 && encodedState <= 1999999)
            {
                int payload = encodedState - 1000000;
                if (character.Health.HP > payload / 1000)
                    targetAction = payload % 1000;
            }
            else if (encodedState >= 2000000 && encodedState <= 2999999)
            {
                int payload = encodedState - 2000000;
                if (character.Health.PP <= payload / 1000)
                    targetAction = payload % 1000;
            }

            int mpCost = 0;
            int hpCost = 0;
            int effectiveMaxHpCost = 0;
            if (sourceFrame.mp != 0 && runtime.InputLocalResourceEnabled49D034)
            {
                mpCost = AdjustNativeMpCost(
                    character,
                    sourceFrame.mp % 1000);
                hpCost = (sourceFrame.mp / 1000) * 10 + sourceFrame.hp;
                effectiveMaxHpCost = sourceFrame.hp / 3;
                if (character.Health.PP < mpCost || character.Health.HP <= hpCost)
                {
                    int fallback = runtime.InputSpecialGate194;
                    if (fallback < 1)
                    {
                        fallback = character.FrameCache?.Wrapper?
                            .characterData?.caughtact ?? 0;
                    }
                    if (fallback < 1)
                        fallback = runtime.InputModeFallbackActionB8;
                    if (fallback < 1)
                    {
                        return Result(
                            requestedAction,
                            targetAction,
                            mpCost,
                            hpCost,
                            effectiveMaxHpCost,
                            failure: NTSD28NativeActionFailure.ResourceRejected);
                    }

                    if (!character.WriteNativeInputActionUnchecked(fallback))
                    {
                        return Result(
                            requestedAction,
                            fallback,
                            mpCost,
                            hpCost,
                            effectiveMaxHpCost,
                            failure: NTSD28NativeActionFailure.ActionWriteFailed);
                    }
                    if (requestedAction < 1)
                        FlipFacing(character);
                    return Result(
                        requestedAction,
                        fallback,
                        mpCost,
                        hpCost,
                        effectiveMaxHpCost,
                        applied: true,
                        usedFallback: true);
                }

                character.Health.PP -= mpCost;
                runtime.InputMpConsumedTotal350 += mpCost;
                character.Health.HP -= hpCost;
                runtime.InputHpConsumedTotal34C += hpCost;
                if (sourceFrame.hp != 0)
                    character.Health.HPBound -= effectiveMaxHpCost;
            }

            if (!character.WriteNativeInputActionUnchecked(targetAction))
            {
                return Result(
                    requestedAction,
                    targetAction,
                    mpCost,
                    hpCost,
                    effectiveMaxHpCost,
                    failure: NTSD28NativeActionFailure.ActionWriteFailed);
            }
            runtime.InputLastAction144 = targetAction;
            if (requestedAction < 1)
                FlipFacing(character);
            return Result(
                requestedAction,
                targetAction,
                mpCost,
                hpCost,
                effectiveMaxHpCost,
                applied: true);
        }

        internal NTSD28NativeComboActionResult RouteNativeComboAction(
            LF2Entity character)
        {
            NTSDEntityRuntime runtime = character?.Runtime;
            if (runtime == null ||
                !NTSD28NativeComboRouteSelector.TrySelect(
                    character.Frame?.D,
                    runtime,
                    out NTSD28NativeComboRouteDecision decision))
            {
                return default;
            }

            if (decision.HasRequestedAction && decision.HasHorizontalFacing)
            {
                character.SwitchDir(decision.FacingLeft ? "left" : "right");
            }

            if (decision.Field == NTSD28NativeComboField.HitJa)
                return RouteNativeHitJa(character, decision);

            if (!decision.HasRequestedAction)
            {
                return new NTSD28NativeComboActionResult(
                    true,
                    false,
                    false,
                    false,
                    decision.Field,
                    default);
            }

            NTSD28NativeActionAttempt action = ApplyNativeInputAction(
                character,
                decision.RequestedAction);
            if (action.Applied)
                runtime.AttackingCounter = 0;
            NTSD28NativeComboRouteSelector.ConsumeAttempt(runtime);
            return new NTSD28NativeComboActionResult(
                true,
                true,
                action.Applied,
                true,
                decision.Field,
                action);
        }

        internal NTSD28NativeFieldRoutingResult RouteNativeThreeButtonFields(
            LF2Entity character)
        {
            NTSD28InputProxyBlock input = character?.Runtime?.NativeInputProxy;
            if (input == null || !input.HasCanonicalStorage)
                return default;

            int attemptCount = 0;
            int appliedCount = 0;
            int attack = input.EdgeWindow[EdgeAttack];
            int jump = input.EdgeWindow[EdgeJump];
            int defend = input.EdgeWindow[EdgeDefend];

            if (attack > jump && attack > defend &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HitAttack,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.EdgeWindow[EdgeAttack] = 0;
            }
            if (defend > attack && defend > jump &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HitDefend,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.EdgeWindow[EdgeDefend] = 0;
            }
            if (jump > attack && jump > defend &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HitJump,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.EdgeWindow[EdgeJump] = 0;
            }

            int heldAttack = input.Previous[KeyAttack] != 0 ? 1 : 0;
            int heldJump = input.Previous[KeyJump] != 0 ? 1 : 0;
            int heldDefend = input.Previous[KeyDefend] != 0 ? 1 : 0;
            if (heldAttack > jump && heldAttack > defend &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HoldAttack,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.Previous[KeyAttack] = 0;
            }
            if (heldDefend > attack && heldDefend > jump &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HoldDefend,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.Previous[KeyDefend] = 0;
            }
            if (heldJump > attack && heldJump > defend &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HoldJump,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.Previous[KeyJump] = 0;
            }

            return new NTSD28NativeFieldRoutingResult(
                attemptCount,
                appliedCount);
        }

        internal NTSD28NativeFieldRoutingResult RouteNativeDirectionFields(
            LF2Entity character)
        {
            NTSD28InputProxyBlock input = character?.Runtime?.NativeInputProxy;
            if (input == null || !input.HasCanonicalStorage)
                return default;

            int attemptCount = 0;
            int appliedCount = 0;
            int left = input.EdgeWindow[EdgeLeft];
            int right = input.EdgeWindow[EdgeRight];
            bool facingLeft = character.Runtime.Dir == "left";
            if (!facingLeft)
            {
                if (right > left &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HitForward,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.EdgeWindow[EdgeRight] = 0;
                }
                if (left > right &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HitBack,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.EdgeWindow[EdgeLeft] = 0;
                }
            }
            else
            {
                if (left > right &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HitForward,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.EdgeWindow[EdgeLeft] = 0;
                }
                if (right > left &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HitBack,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.EdgeWindow[EdgeRight] = 0;
                }
            }

            if (input.EdgeWindow[EdgeDepthUp] >
                (input.Previous[KeyDepthUp] != 0 ? 1 : 0) &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HitDepthUp,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.Previous[KeyDepthUp] = 0;
            }
            if (input.EdgeWindow[EdgeDepthDown] >
                (input.Previous[KeyDepthDown] != 0 ? 1 : 0) &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HitDepthDown,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.EdgeWindow[EdgeDepthDown] = 0;
            }

            int heldLeft = input.Previous[KeyLeft] != 0 ? 1 : 0;
            int heldRight = input.Previous[KeyRight] != 0 ? 1 : 0;
            facingLeft = character.Runtime.Dir == "left";
            if (!facingLeft)
            {
                if (heldRight > heldLeft &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HoldForward,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.Previous[KeyRight] = 0;
                }
                if (heldLeft > heldRight &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HoldBack,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.Previous[KeyLeft] = 0;
                }
            }
            else
            {
                if (heldLeft > heldRight &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HoldForward,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.Previous[KeyLeft] = 0;
                }
                if (heldRight > heldLeft &&
                    TryApplyNativeFrameField(
                        character,
                        NativeInputFrameField.HoldBack,
                        ref attemptCount,
                        ref appliedCount))
                {
                    input.Previous[KeyRight] = 0;
                }
            }

            if (input.EdgeWindow[EdgeDepthUp] <
                (input.Previous[KeyDepthUp] != 0 ? 1 : 0) &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HoldDepthUp,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.EdgeWindow[EdgeDepthUp] = 0;
            }
            if (input.EdgeWindow[EdgeDepthDown] <
                (input.Previous[KeyDepthDown] != 0 ? 1 : 0) &&
                TryApplyNativeFrameField(
                    character,
                    NativeInputFrameField.HoldDepthDown,
                    ref attemptCount,
                    ref appliedCount))
            {
                input.Previous[KeyDepthDown] = 0;
            }

            return new NTSD28NativeFieldRoutingResult(
                attemptCount,
                appliedCount);
        }

        internal bool RouteNativeGroundBuiltins(LF2Entity character)
        {
            NTSDEntityRuntime runtime = character?.Runtime;
            NTSD28InputProxyBlock input = runtime?.NativeInputProxy;
            LF2FrameData frame = character?.Frame?.D;
            if (runtime == null || input == null ||
                !input.HasCanonicalStorage || frame == null)
            {
                return false;
            }

            int state = ReadNativeInputFrameState(frame);
            bool action110 = character.Frame.N == 110;
            bool specialGroundState = state == 19 || state == 301;
            if (!action110 && !specialGroundState &&
                state != 0 && state != 1 && state != 2)
            {
                return false;
            }

            DecayNativeRunAccumulator(runtime);
            bool left = input.Current[KeyLeft] != 0;
            bool right = input.Current[KeyRight] != 0;
            bool up = input.Current[KeyDepthUp] != 0;
            bool down = input.Current[KeyDepthDown] != 0;

            if (action110)
            {
                if (right)
                    character.SwitchDir("right");
                if (left)
                    character.SwitchDir("left");
            }

            LF2CharacterData data = character.FrameCache?.Wrapper?.characterData;
            if (specialGroundState &&
                character.PS != null &&
                runtime.Y == character.PS.groundY)
            {
                double depthSpeed = data?.running_speedz ?? 0.0;
                if (up && !down)
                    runtime.Vz = -depthSpeed;
                else if (down && !up)
                    runtime.Vz = depthSpeed;
            }

            if (state == 0 || state == 1)
            {
                LF2CharacterData linkedData = ResolveNativeLinkedCharacterData(
                    character);
                if (runtime.LinkState == 2)
                {
                    RouteNativeHeavyStanding(
                        character,
                        data,
                        linkedData,
                        left,
                        right,
                        up,
                        down);
                }
                else
                {
                    RouteNativeStanding(
                        character,
                        data,
                        linkedData,
                        left,
                        right,
                        up,
                        down);
                }
            }
            else if (state == 2)
            {
                LF2CharacterData linkedData = ResolveNativeLinkedCharacterData(
                    character);
                if (runtime.LinkState == 2)
                {
                    RouteNativeHeavyRunning(
                        character,
                        data,
                        linkedData,
                        left,
                        right,
                        up,
                        down);
                }
                else
                {
                    RouteNativeRunning(
                        character,
                        data,
                        linkedData,
                        left,
                        right,
                        up,
                        down);
                }
            }

            return true;
        }

        internal bool RouteNativeAirDashRedirectBuiltins(LF2Entity character)
        {
            NTSDEntityRuntime runtime = character?.Runtime;
            NTSD28InputProxyBlock input = runtime?.NativeInputProxy;
            LF2FrameData frame = character?.Frame?.D;
            if (runtime == null || input == null ||
                !input.HasCanonicalStorage || frame == null)
            {
                return false;
            }

            int action = character.Frame.N;
            int state = ReadNativeInputFrameState(frame);
            // Alignment contract: NTSD28-Q06-NATIVE-INPUT-MISSING-STATE-ROUTING-001.
            // Ground owns its handled states; this fallback consumes the remaining frame prelude once.
            DecayNativeRunAccumulator(runtime);
            bool owned = action == 215 || action == 182 || action == 188 ||
                         state == 4 || state == 5 ||
                         state == 85 || state == 86;
            if (!owned)
                return false;

            bool left = input.Current[KeyLeft] != 0;
            bool right = input.Current[KeyRight] != 0;
            bool up = input.Current[KeyDepthUp] != 0;
            bool down = input.Current[KeyDepthDown] != 0;
            LF2CharacterData data = character.FrameCache?.Wrapper?.characterData;

            if (action == 215)
            {
                RouteNativeAction215(
                    character,
                    data,
                    left,
                    right,
                    up,
                    down);
                frame = character.Frame?.D;
                if (frame == null)
                    return true;
                state = ReadNativeInputFrameState(frame);
            }

            action = character.Frame.N;
            if (action == 182 || action == 188)
            {
                RouteNativeRowingRedirect(character, data);
                return true;
            }

            if (state == 85 || state == 86)
            {
                OrientNativeExclusiveHorizontal(character, left, right);
                bool movingForward = IsNativeMovingForward(runtime);
                if (state == 85 && character.Frame.N != 0 && movingForward)
                {
                    AssignNativeDirectAction(
                        character,
                        character.Frame.N + 1,
                        false);
                }
                return true;
            }

            if (state == 5)
            {
                RouteNativeState5(
                    character,
                    left,
                    right,
                    up,
                    down);
                return true;
            }

            if (state == 4 && character.PS != null &&
                runtime.Y < character.PS.groundY)
            {
                RouteNativeState4(
                    character,
                    left,
                    right,
                    up,
                    down);
            }
            return true;
        }

        private static void RouteNativeAction215(
            LF2Entity character,
            LF2CharacterData data,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            if (input.Current[KeyDefend] != 0 &&
                IsNativeBuffered(input, EdgeDefend))
            {
                AssignNativeDirectAction(character, 102, false);
            }

            double dashZ = data?.dash_distancez ?? 0.0;
            if (input.Current[KeyJump] != 0 &&
                IsNativeBuffered(input, EdgeJump))
            {
                double dashX = data?.dash_distance ?? 0.0;
                double dashY = data?.dash_height ?? 0.0;
                const double directionEpsilon = 0.001;
                bool selected = false;
                int action = character.Frame.N;
                bool facingLeft = runtime.Dir == "left";
                if (right || runtime.Vx > directionEpsilon)
                {
                    action = 213 + (facingLeft ? 1 : 0);
                    runtime.Vx = dashX;
                    selected = true;
                }
                if (left || runtime.Vx < -directionEpsilon)
                {
                    action = 214 - (facingLeft ? 1 : 0);
                    runtime.Vx = -dashX;
                    selected = true;
                }
                if (selected)
                {
                    runtime.Vy = dashY;
                    AssignNativeDirectAction(character, action, false);
                }
            }

            if (up && !down)
                runtime.Vz = -dashZ;
            else if (down && !up)
                runtime.Vz = dashZ;
        }

        private static void RouteNativeRowingRedirect(
            LF2Entity character,
            LF2CharacterData data)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            int currentAction = character.Frame.N;
            if ((currentAction != 182 && currentAction != 188) ||
                runtime.EnvironmentState320 < 0 ||
                input.Current[KeyJump] == 0 ||
                !IsNativeBuffered(input, EdgeJump) ||
                character.Health == null || character.Health.HP <= 0 ||
                input.ComboState[8] == 1)
            {
                return;
            }

            int costAction = currentAction == 182 ? 100 : 108;
            LF2FrameData costFrame = character.FrameCache?
                .GetNativeFrameDataById(costAction);
            if (costFrame == null)
                return;

            int mpCost = costFrame.mp;
            if (mpCost != 0)
            {
                if (character.Health.PP < mpCost)
                    return;
                character.Health.PP -= mpCost;
            }

            bool facingLeft = runtime.Dir == "left";
            double priorX = runtime.Vx;
            int action = ((!facingLeft && priorX <= 0.0) ||
                          (facingLeft && priorX > 0.0))
                ? 100
                : 108;
            if (character.FrameCache?.HasNativeFrame(action) != true)
                return;

            AssignNativeDirectAction(character, action, true);
            double rowingHeight = data?.rowing_height ?? 0.0;
            double rowingDistance = data?.rowing_distance ?? 0.0;
            if (runtime.Vy > rowingHeight)
                runtime.Vy = rowingHeight;
            if (priorX >= 1.0 || priorX <= -1.0)
                runtime.Vx = priorX <= 0.0 ? -rowingDistance : rowingDistance;
            else
                runtime.Vx = facingLeft ? rowingDistance : -rowingDistance;
        }

        private void RouteNativeState5(
            LF2Entity character,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            OrientNativeExclusiveHorizontal(character, left, right);

            bool facingLeft = runtime.Dir == "left";
            if (!facingLeft)
            {
                if (runtime.Vx < 0.0 && character.Frame.N != 217)
                    AssignNativeDirectAction(character, 214, false);
                if (runtime.Vx > 0.0 && character.Frame.N != 216)
                    AssignNativeDirectAction(character, 213, false);
            }
            else
            {
                if (runtime.Vx > 0.0 && character.Frame.N != 217)
                    AssignNativeDirectAction(character, 214, false);
                if (runtime.Vx < 0.0 && character.Frame.N != 216)
                    AssignNativeDirectAction(character, 213, false);
            }

            if (!IsNativeMovingForward(runtime) ||
                input.Current[KeyAttack] == 0)
            {
                return;
            }

            int interaction = runtime.LinkState;
            if (interaction == 0)
            {
                AssignNativeGatedResourceAction(character, 90, false);
                return;
            }

            bool anyDirection = left || right || up || down;
            bool allDirections = left && right && up && down;
            bool linkedFamily = interaction % 100 == 1;
            bool throwFamily =
                (interaction == 4 && anyDirection && !allDirections) ||
                (interaction == 6 && anyDirection);
            if (!linkedFamily && !throwFamily)
                return;

            LF2CharacterData linkedData = ResolveNativeLinkedCharacterData(
                character);
            bool skyThrow = interaction == 4 || interaction == 6;
            int action = NativeLinkedAction(
                linkedData,
                skyThrow
                    ? NativeLinkedActionField.SkyLightThrow
                    : NativeLinkedActionField.JumpAttack,
                skyThrow ? 52 : 40);
            AssignNativeDirectAction(character, action, true);
            runtime.Vy -= 1.0;
        }

        private void RouteNativeState4(
            LF2Entity character,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            OrientNativeExclusiveHorizontal(character, left, right);
            if (!IsNativeBuffered(input, EdgeAttack))
                return;

            int interaction = runtime.LinkState;
            if (interaction == 0)
            {
                AssignNativeClampedResourceAction(character, 80, true);
                return;
            }

            bool hasDirection = left || right || up || down;
            LF2CharacterData linkedData = ResolveNativeLinkedCharacterData(
                character);
            int action;
            if (interaction % 100 == 1 && !hasDirection)
            {
                action = NativeLinkedAction(
                    linkedData,
                    NativeLinkedActionField.JumpAttack,
                    30);
            }
            else if (interaction % 100 == 1 ||
                     interaction == 4 || interaction == 6)
            {
                action = NativeLinkedAction(
                    linkedData,
                    NativeLinkedActionField.SkyLightThrow,
                    52);
            }
            else
            {
                return;
            }
            AssignNativeDirectAction(character, action, true);
        }

        private static void OrientNativeExclusiveHorizontal(
            LF2Entity character,
            bool left,
            bool right)
        {
            if (right && !left)
                character.SwitchDir("right");
            else if (left && !right)
                character.SwitchDir("left");
        }

        private static bool IsNativeMovingForward(NTSDEntityRuntime runtime)
        {
            return runtime.Dir == "left"
                ? runtime.Vx < 0.0
                : runtime.Vx > 0.0;
        }

        private void RouteNativeStanding(
            LF2Entity character,
            LF2CharacterData data,
            LF2CharacterData linkedData,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            double walkX = data?.walking_speed ?? 0.0;
            double walkZ = data?.walking_speedz ?? 0.0;
            int walkRate = data?.walking_frame_rate ?? 1;
            bool walking = false;
            if (right && !left)
            {
                if (runtime.Dir == "left")
                    runtime.AnimSub = 0;
                character.SwitchDir("right");
                runtime.Vx = walkX;
                if (input.Previous[KeyRight] == 0)
                    runtime.AnimSub += 10;
                walking = true;
            }
            else if (left && !right)
            {
                if (runtime.Dir != "left")
                    runtime.AnimSub = 0;
                character.SwitchDir("left");
                runtime.Vx = -walkX;
                if (input.Previous[KeyLeft] == 0)
                    runtime.AnimSub -= 10;
                walking = true;
            }
            if (up && !down)
            {
                runtime.Vz = -walkZ;
                walking = true;
            }
            else if (down && !up)
            {
                runtime.Vz = walkZ;
                walking = true;
            }
            ScaleNativeDiagonalX(runtime, up != down, 1.4);

            if (walking)
            {
                bool running = runtime.AnimSub > 10 || runtime.AnimSub < -10;
                if (running)
                {
                    runtime.AnimSub = 0;
                    runtime.AnimCounter = 0;
                    AssignNativeDirectAction(character, 9, false);
                    bool standingActionPending =
                        (input.Current[KeyAttack] != 0 &&
                         IsNativeBuffered(input, EdgeAttack)) ||
                        (input.Current[KeyJump] != 0 &&
                         IsNativeBuffered(input, EdgeJump)) ||
                        (input.Current[KeyDefend] != 0 &&
                         IsNativeBuffered(input, EdgeDefend) &&
                         input.DefendReentryCooldown == 0);
                    if (character.Frame.N == 9 && !standingActionPending)
                    {
                        RouteNativeGroundBuiltins(character);
                        return;
                    }
                }
                else
                {
                    AssignNativeDirectAction(
                        character,
                        NativeMovementAction(
                            data?.walking_frames,
                            5,
                            6,
                            walkRate,
                            runtime),
                        false);
                }
            }

            if (input.Current[KeyAttack] != 0 &&
                IsNativeBuffered(input, EdgeAttack))
            {
                RouteNativeStandingAttack(
                    character,
                    linkedData,
                    left || right || up || down);
            }
            if (input.Current[KeyJump] != 0 &&
                IsNativeBuffered(input, EdgeJump))
            {
                runtime.AnimSub = 0;
                AssignNativeDirectAction(character, 210, true);
            }
            if (input.Current[KeyDefend] != 0 &&
                IsNativeBuffered(input, EdgeDefend) &&
                input.DefendReentryCooldown == 0)
            {
                runtime.AnimSub = 0;
                AssignNativeDirectAction(character, 110, true);
            }
        }

        private void RouteNativeHeavyStanding(
            LF2Entity character,
            LF2CharacterData data,
            LF2CharacterData linkedData,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            double walkX = data?.heavy_walking_speed ?? 0.0;
            double walkZ = data?.heavy_walking_speedz ?? 0.0;
            int walkRate = data?.heavy_walking_frames?.Count > 0
                ? data.heavy_walking_frames.Count
                : data?.walking_frame_rate ?? 1;
            if (character.Frame.N < 12)
                AssignNativeDirectAction(character, 12, false);

            bool walking = false;
            if (right && !left)
            {
                if (runtime.Dir == "left")
                    runtime.AnimSub = 0;
                character.SwitchDir("right");
                runtime.Vx = walkX;
                if (input.Previous[KeyRight] == 0)
                    runtime.AnimSub += 10;
                walking = true;
            }
            else if (left && !right)
            {
                if (runtime.Dir != "left")
                    runtime.AnimSub = 0;
                character.SwitchDir("left");
                runtime.Vx = -walkX;
                if (input.Previous[KeyLeft] == 0)
                    runtime.AnimSub -= 10;
                walking = true;
            }
            if (up && !down)
            {
                runtime.Vz = -walkZ;
                walking = true;
            }
            else if (down && !up)
            {
                runtime.Vz = walkZ;
                walking = true;
            }
            ScaleNativeDiagonalX(runtime, up != down, 1.4);

            if (walking)
            {
                bool running = runtime.AnimSub > 10 || runtime.AnimSub < -10;
                if (running)
                {
                    runtime.AnimSub = 0;
                    runtime.AnimCounter = 0;
                    AssignNativeDirectAction(character, 16, false);
                    if (character.Frame.N == 16)
                    {
                        RouteNativeGroundBuiltins(character);
                        return;
                    }
                }
                else
                {
                    AssignNativeDirectAction(
                        character,
                        NativeMovementAction(
                            data?.heavy_walking_frames,
                            12,
                            6,
                            walkRate,
                            runtime),
                        false);
                }
            }

            if (IsNativeBuffered(input, EdgeAttack))
            {
                runtime.AnimCounter = 0;
                AssignNativeDirectAction(
                    character,
                    NativeLinkedAction(
                        linkedData,
                        NativeLinkedActionField.HeavyThrow,
                        50),
                    true);
            }
            if (IsNativeBuffered(input, EdgeJump))
            {
                runtime.AnimCounter = 0;
                AssignNativeDirectAction(character, 210, true);
            }
            if (IsNativeBuffered(input, EdgeDefend) &&
                input.DefendReentryCooldown == 0)
            {
                runtime.AnimCounter = 0;
                AssignNativeDirectAction(character, 110, true);
            }
        }

        private void RouteNativeRunning(
            LF2Entity character,
            LF2CharacterData data,
            LF2CharacterData linkedData,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            double runX = data?.running_speed ?? 0.0;
            double runZ = data?.running_speedz ?? 0.0;
            int runAction = NativeMovementAction(
                data?.running_frames,
                9,
                4,
                data?.running_frame_rate ?? 1,
                runtime);
            if (runtime.Dir == "left")
            {
                runtime.Vx = -runX;
                if (right)
                    runAction = 218;
            }
            else
            {
                runtime.Vx = runX;
                if (left)
                    runAction = 218;
            }
            if (up && !down)
                runtime.Vz = -runZ;
            else if (down && !up)
                runtime.Vz = runZ;
            ScaleNativeDiagonalX(runtime, up != down, 1.2);
            AssignNativeDirectAction(character, runAction, false);

            if (IsNativeBuffered(input, EdgeAttack))
            {
                int interaction = runtime.LinkState;
                int action = 85;
                if (interaction % 100 == 1)
                {
                    if (!left && !right && !up && !down)
                    {
                        action = NativeLinkedAction(
                            linkedData,
                            NativeLinkedActionField.RunAttack,
                            35);
                    }
                    else
                    {
                        action = NativeLinkedAction(
                            linkedData,
                            NativeLinkedActionField.LightThrow,
                            45);
                    }
                }
                else if (interaction == 4 || interaction == 6)
                {
                    bool neutral = !left && !right && !up && !down;
                    action = interaction == 6 && neutral
                        ? NativeLinkedAction(
                            linkedData,
                            NativeLinkedActionField.WeaponDrink,
                            55)
                        : NativeLinkedAction(
                            linkedData,
                            NativeLinkedActionField.LightThrow,
                            45);
                }

                if (interaction == 0)
                    AssignNativeGatedResourceAction(character, action, false);
                else
                    AssignNativeDirectAction(character, action, false);
            }
            if (IsNativeBuffered(input, EdgeJump))
            {
                ApplyNativeRunningJumpMotion(character, data, up, down);
                runtime.AnimSub = 0;
                AssignNativeDirectAction(character, 213, false);
            }
            if (IsNativeBuffered(input, EdgeDefend))
                AssignNativeDirectAction(character, 102, false);
        }

        private void RouteNativeHeavyRunning(
            LF2Entity character,
            LF2CharacterData data,
            LF2CharacterData linkedData,
            bool left,
            bool right,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            NTSD28InputProxyBlock input = runtime.NativeInputProxy;
            double runX = data?.heavy_running_speed ?? 0.0;
            double runZ = data?.heavy_running_speedz ?? 0.0;
            int runRate = data?.heavy_running_frames?.Count > 0
                ? data.heavy_running_frames.Count
                : data?.running_frame_rate ?? 1;
            if (right && !left)
            {
                character.SwitchDir("right");
                runtime.Vx = runX;
            }
            else if (left && !right)
            {
                character.SwitchDir("left");
                runtime.Vx = -runX;
            }
            if (up && !down)
                runtime.Vz = -runZ;
            else if (down && !up)
                runtime.Vz = runZ;
            ScaleNativeDiagonalX(runtime, up != down, 1.2);
            if ((left != right) || (up != down))
            {
                AssignNativeDirectAction(
                    character,
                    NativeMovementAction(
                        data?.heavy_running_frames,
                        16,
                        4,
                        runRate,
                        runtime),
                    false);
            }

            if (IsNativeBuffered(input, EdgeAttack))
            {
                AssignNativeDirectAction(
                    character,
                    NativeLinkedAction(
                        linkedData,
                        NativeLinkedActionField.RunHeavyThrow,
                        50),
                    false);
            }
            if (IsNativeBuffered(input, EdgeJump))
            {
                ApplyNativeRunningJumpMotion(character, data, up, down);
                runtime.AnimSub = 0;
                AssignNativeDirectAction(character, 213, false);
            }
            if (IsNativeBuffered(input, EdgeDefend))
                AssignNativeDirectAction(character, 102, false);
        }

        private void RouteNativeStandingAttack(
            LF2Entity character,
            LF2CharacterData linkedData,
            bool hasDirection)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            runtime.AnimSub = 0;
            runtime.AttackingCounter = 0;
            int interaction = runtime.LinkState;
            if (interaction == 0)
            {
                if (runtime.HitConfirmEa > 0)
                {
                    AssignNativeDirectAction(character, 70, false);
                    return;
                }

                int selection = character.RegisteredWorldForSimulation?
                    .NativeRandom?.SynchronizedNext(0x82u, 2) ?? 0;
                AssignNativeClampedResourceAction(
                    character,
                    (selection + 12) * 5,
                    false);
                return;
            }

            int action;
            if (interaction == 101 && hasDirection)
            {
                action = NativeLinkedAction(
                    linkedData,
                    NativeLinkedActionField.LightThrow,
                    45);
            }
            else if (interaction % 100 == 1)
            {
                uint callSite = interaction == 101 ? 0x83u : 0x84u;
                int selection = character.RegisteredWorldForSimulation?
                    .NativeRandom?.SynchronizedNext(callSite, 2) ?? 0;
                action = selection == 0
                    ? NativeLinkedAction(
                        linkedData,
                        NativeLinkedActionField.NormalAttack1,
                        20)
                    : NativeLinkedAction(
                        linkedData,
                        NativeLinkedActionField.NormalAttack2,
                        25);
            }
            else if (interaction == 4 ||
                     (interaction == 6 && hasDirection))
            {
                action = NativeLinkedAction(
                    linkedData,
                    NativeLinkedActionField.LightThrow,
                    45);
            }
            else if (interaction == 6)
            {
                action = NativeLinkedAction(
                    linkedData,
                    NativeLinkedActionField.WeaponDrink,
                    55);
            }
            else
            {
                return;
            }

            AssignNativeDirectAction(character, action, false);
        }

        private static void ApplyNativeRunningJumpMotion(
            LF2Entity character,
            LF2CharacterData data,
            bool up,
            bool down)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            double dashX = data?.dash_distance ?? 0.0;
            runtime.Vx = runtime.Dir == "left" ? -dashX : dashX;
            runtime.Vy = data?.dash_height ?? 0.0;
            double dashZ = data?.dash_distancez ?? 0.0;
            if (up && !down)
                runtime.Vz = -dashZ;
            else if (down && !up)
                runtime.Vz = dashZ;
        }

        private static LF2CharacterData ResolveNativeLinkedCharacterData(
            LF2Entity character)
        {
            NTSDEntityRuntime runtime = character?.Runtime;
            SimulationWorld world = character?.RegisteredWorldForSimulation;
            if (runtime == null || world == null ||
                runtime.LinkState <= 0 || runtime.TargetSlotIndex < 0)
            {
                return null;
            }

            LF2Entity linked = world.FindEntityByRuntimeSlotForQuery(
                runtime.TargetSlotIndex);
            return linked?.FrameCache?.Wrapper?.characterData;
        }

        private static int NativeLinkedAction(
            LF2CharacterData linkedData,
            NativeLinkedActionField field,
            int fallback)
        {
            return BattleNativeLinkedWeaponActionResolver.Resolve(linkedData, field, fallback);
        }

        private static int NativeMovementAction(
            List<int> sequence,
            int fallbackBase,
            int fallbackPhases,
            int rate,
            NTSDEntityRuntime runtime)
        {
            if (rate < 1)
                rate = 1;
            int count = sequence?.Count ?? 0;
            int phases = count > 0 ? count : fallbackPhases;
            int period = phases * rate;
            runtime.AnimCounter = (runtime.AnimCounter + 1) % period;
            int phase = runtime.AnimCounter / rate;
            if (count > 0)
                return sequence[phase];
            if (fallbackPhases == 6 && phase > 3)
                phase = 6 - phase;
            if (fallbackPhases == 4 && phase > 2)
                phase -= 2;
            return fallbackBase + phase;
        }

        private static int ReadNativeInputFrameState(LF2FrameData frame)
        {
            if (!frame.UsesLoganFrameNumbers)
                return frame.state;

            return frame.rawProperties != null &&
                   frame.rawProperties.TryGetValue("state", out string value) &&
                   LoganNumericDecoder.TryParseInt32(value, out int state)
                ? state
                : -1;
        }

        private static void DecayNativeRunAccumulator(
            NTSDEntityRuntime runtime)
        {
            if (runtime.AnimSub > 0)
                runtime.AnimSub--;
            else if (runtime.AnimSub < 0)
                runtime.AnimSub++;
        }

        private static void ScaleNativeDiagonalX(
            NTSDEntityRuntime runtime,
            bool hasExclusiveDepth,
            double divisor)
        {
            if (hasExclusiveDepth)
                runtime.Vx /= divisor;
        }

        private static bool IsNativeBuffered(
            NTSD28InputProxyBlock input,
            int edgeIndex)
        {
            return input.EdgeWindow[edgeIndex] > 0;
        }

        private static void AssignNativeDirectAction(
            LF2Entity character,
            int action,
            bool restartAction)
        {
            character.WriteNativeInputActionUnchecked(action);
            if (restartAction)
                character.Runtime.AttackingCounter = 0;
        }

        private static void AssignNativeClampedResourceAction(
            LF2Entity character,
            int action,
            bool restartAction)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime.InputLocalResourceEnabled49D034 &&
                character.Health != null)
            {
                int mpCost = NativeBuiltinMpCost(character, action);
                if (character.Health.PP < mpCost)
                {
                    character.Health.PP = 0;
                }
                else
                {
                    character.Health.PP -= mpCost;
                    runtime.InputMpConsumedTotal350 += mpCost;
                }
            }
            AssignNativeDirectAction(character, action, restartAction);
        }

        private static bool AssignNativeGatedResourceAction(
            LF2Entity character,
            int action,
            bool restartAction)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime.InputLocalResourceEnabled49D034 &&
                character.Health != null)
            {
                int mpCost = NativeBuiltinMpCost(character, action);
                if (character.Health.PP < mpCost)
                    return false;
                character.Health.PP -= mpCost;
                runtime.InputMpConsumedTotal350 += mpCost;
            }
            AssignNativeDirectAction(character, action, restartAction);
            return true;
        }

        private static int NativeBuiltinMpCost(
            LF2Entity character,
            int action)
        {
            int rawCost = character.FrameCache?.GetNativeFrameDataById(action)?.mp ?? 0;
            return AdjustNativeMpCost(character, rawCost);
        }

        private NTSD28NativeComboActionResult RouteNativeHitJa(
            LF2Entity character,
            NTSD28NativeComboRouteDecision decision)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            int useAi = runtime.NativeAiProfileObjectId;
            bool specialFamily = character.ObjectId == 6 || useAi == 6;
            if (specialFamily &&
                decision.RequestedAction == 300 &&
                character.Health?.HP > 177 &&
                !runtime.FeatureGate4A8428)
            {
                runtime.AttackingCounter = 0;
                NTSD28NativeComboRouteSelector.ConsumeAttempt(runtime);
                return new NTSD28NativeComboActionResult(
                    true,
                    true,
                    false,
                    true,
                    decision.Field,
                    default);
            }

            if (specialFamily &&
                (decision.RequestedAction == 0 ||
                 runtime.InputLinkedDefinitionId324 != -1))
            {
                if (runtime.InputSpecialGate194 == 1 || runtime.Unk328 != 1)
                {
                    return new NTSD28NativeComboActionResult(
                        true,
                        false,
                        false,
                        false,
                        decision.Field,
                        default);
                }

                runtime.Unk338 = 0;
                runtime.AttackingCounter = 0;
                NTSD28NativeComboRouteSelector.ConsumeAttempt(runtime);
                return new NTSD28NativeComboActionResult(
                    true,
                    true,
                    false,
                    true,
                    decision.Field,
                    default);
            }

            if (!decision.HasRequestedAction)
            {
                return new NTSD28NativeComboActionResult(
                    true,
                    false,
                    false,
                    false,
                    decision.Field,
                    default);
            }

            NTSD28NativeActionAttempt action = ApplyNativeInputAction(
                character,
                decision.RequestedAction);
            if (action.Applied)
                runtime.AttackingCounter = 0;
            NTSD28NativeComboRouteSelector.ConsumeAttempt(runtime);
            return new NTSD28NativeComboActionResult(
                true,
                true,
                action.Applied,
                true,
                decision.Field,
                action);
        }

        internal static int AdjustNativeMpCost(
            LF2Entity character,
            int rawCost)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime.InputCostWaived1B4 != 0)
                return 0;

            int adjusted = rawCost;
            int multiplier = character.FrameCache?.Wrapper?
                .characterData?.recmp ?? 0;
            if (multiplier < 1)
                multiplier = runtime.InputModeCostMultiplier30;
            if (multiplier > 0 && multiplier != 100)
                adjusted = (adjusted * multiplier) / 100;
            if (runtime.InputDoubleCost19C > 0)
                adjusted *= 2;
            return adjusted;
        }

        private bool TryApplyNativeFrameField(
            LF2Entity character,
            NativeInputFrameField field,
            ref int attemptCount,
            ref int appliedCount)
        {
            LF2FrameData frame = character?.Frame?.D;
            if (frame == null)
                return false;

            int requestedAction = ReadNativeFrameField(frame, field);
            if (requestedAction == 0)
                return false;

            NTSD28NativeActionAttempt action = ApplyNativeInputAction(
                character,
                requestedAction);
            attemptCount++;
            if (action.Applied)
                appliedCount++;
            return true;
        }

        private static int ReadNativeFrameField(
            LF2FrameData frame,
            NativeInputFrameField field)
        {
            return field switch
            {
                NativeInputFrameField.HitAttack => frame.hit_a,
                NativeInputFrameField.HitDefend => frame.hit_d,
                NativeInputFrameField.HitJump => frame.hit_j,
                NativeInputFrameField.HoldAttack => frame.hold_a,
                NativeInputFrameField.HoldDefend => frame.hold_d,
                NativeInputFrameField.HoldJump => frame.hold_j,
                NativeInputFrameField.HitForward => frame.hit_f,
                NativeInputFrameField.HitBack => frame.hit_b,
                NativeInputFrameField.HitDepthUp => frame.hit_uz,
                NativeInputFrameField.HitDepthDown => frame.hit_dz,
                NativeInputFrameField.HoldForward => frame.hold_f,
                NativeInputFrameField.HoldBack => frame.hold_b,
                NativeInputFrameField.HoldDepthUp => frame.hold_uz,
                NativeInputFrameField.HoldDepthDown => frame.hold_dz,
                _ => 0,
            };
        }

        private static void FlipFacing(LF2Entity character)
        {
            character.SwitchDir(
                character.Runtime.Dir == "left" ? "right" : "left");
        }

        private static NTSD28NativeActionAttempt Result(
            int requestedAction,
            int resolvedAction,
            int mpCost = 0,
            int hpCost = 0,
            int effectiveMaxHpCost = 0,
            bool applied = false,
            bool usedFallback = false,
            NTSD28NativeActionFailure failure = NTSD28NativeActionFailure.None)
        {
            return new NTSD28NativeActionAttempt(
                true,
                applied,
                usedFallback,
                requestedAction,
                resolvedAction,
                mpCost,
                hpCost,
                effectiveMaxHpCost,
                failure);
        }

        internal bool TryApplyExactCharacterFrameVelocityTail(
            LF2Character character)
        {
            if (character == null || character.GetType() != typeof(LF2Character))
                return false;

            LF2FrameData frame = character?.Frame?.D;
            NTSDEntityRuntime runtime = character?.Runtime;
            if (frame == null || runtime == null)
                return true;

            double vx = runtime.Vx;
            ApplyAxisVelocity(frame.dvx, ref vx, runtime.Dir == "left" ? -1 : 1);
            runtime.Vx = vx;

            if (frame.dvy > 500)
                runtime.Vy = frame.dvy - 550;
            else if (frame.dvy != 0)
                runtime.Vy += frame.dvy;

            if (frame.dvz > 500)
            {
                runtime.Vz = frame.dvz - 550;
                return true;
            }

            if (frame.dvz == 0)
                return true;

            if (runtime.KeyUp != 0 && runtime.CdUp >= runtime.CdDown)
                runtime.Vz = -frame.dvz;
            if (runtime.KeyDown != 0 && runtime.CdDown >= runtime.CdUp)
                runtime.Vz = frame.dvz;
            return true;
        }

        private void ApplyAxisVelocity(
            int value,
            ref double velocity,
            int direction)
        {
            if (value > 500)
            {
                velocity = value - 550;
                return;
            }

            if (value == 0)
                return;

            double target = value * direction;
            if (value > 0)
            {
                if (direction >= 0)
                {
                    if (velocity < target)
                        velocity = target;
                }
                else if (velocity > target)
                {
                    velocity = target;
                }

                return;
            }

            if (direction >= 0)
            {
                if (velocity > target)
                    velocity = target;
            }
            else if (velocity < target)
            {
                velocity = target;
            }
        }
    }
}
