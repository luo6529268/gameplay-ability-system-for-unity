using NTSD.Animation;

namespace NTSD.Simulation
{
    internal enum NTSD28NativeComboField : byte
    {
        None = 0,
        HitFa = 1,
        HitFj = 2,
        HitUa = 3,
        HitUj = 4,
        HitDa = 5,
        HitDj = 6,
        HitAj = 7,
        HitAd = 8,
        HitJa = 9,
        HitJd = 10,
    }

    internal readonly struct NTSD28NativeComboRouteDecision
    {
        internal NTSD28NativeComboRouteDecision(
            NTSD28NativeComboField field,
            int requestedAction,
            bool hasHorizontalFacing,
            bool facingLeft)
        {
            Field = field;
            RequestedAction = requestedAction;
            HasHorizontalFacing = hasHorizontalFacing;
            FacingLeft = facingLeft;
        }

        internal NTSD28NativeComboField Field { get; }
        internal int RequestedAction { get; }
        internal bool HasHorizontalFacing { get; }
        internal bool FacingLeft { get; }
        internal bool HasRequestedAction => RequestedAction != 0;
    }

    internal static class NTSD28NativeComboRouteSelector
    {
        internal static bool TrySelect(
            LF2FrameData frame,
            NTSDEntityRuntime runtime,
            out NTSD28NativeComboRouteDecision decision)
        {
            decision = default;
            NTSD28InputProxyBlock input = runtime?.NativeInputProxy;
            if (frame == null || input == null || !input.HasCanonicalStorage)
                return false;

            byte[] combo = input.ComboState;
            if (combo[0] >= 4)
            {
                decision = new NTSD28NativeComboRouteDecision(
                    NTSD28NativeComboField.HitFa,
                    frame.hit_Fa,
                    true,
                    combo[0] - 4 != 0);
                return true;
            }
            if (combo[1] >= 4)
            {
                decision = new NTSD28NativeComboRouteDecision(
                    NTSD28NativeComboField.HitFj,
                    frame.hit_Fj,
                    true,
                    combo[1] - 4 != 0);
                return true;
            }
            if (combo[2] == 3)
                return Select(NTSD28NativeComboField.HitUa, frame.hit_Ua, out decision);
            if (combo[3] == 3)
                return Select(NTSD28NativeComboField.HitUj, frame.hit_Uj, out decision);
            if (combo[4] == 3)
                return Select(NTSD28NativeComboField.HitDa, frame.hit_Da, out decision);
            if (combo[5] == 3)
                return Select(NTSD28NativeComboField.HitDj, frame.hit_Dj, out decision);
            if (combo[6] == 1)
                return Select(NTSD28NativeComboField.HitAj, frame.hit_aj, out decision);
            if (combo[7] == 1)
                return Select(NTSD28NativeComboField.HitAd, frame.hit_ad, out decision);
            if (combo[8] == 1)
                return Select(NTSD28NativeComboField.HitJa, frame.hit_ja, out decision);
            if (combo[9] == 1)
                return Select(NTSD28NativeComboField.HitJd, frame.hit_jd, out decision);
            return false;
        }

        internal static void ConsumeAttempt(NTSDEntityRuntime runtime)
        {
            NTSD28NativeComboStateMachine.ClearComboAttempt(runtime);
        }

        private static bool Select(
            NTSD28NativeComboField field,
            int requestedAction,
            out NTSD28NativeComboRouteDecision decision)
        {
            decision = new NTSD28NativeComboRouteDecision(
                field,
                requestedAction,
                false,
                false);
            return true;
        }
    }
}
