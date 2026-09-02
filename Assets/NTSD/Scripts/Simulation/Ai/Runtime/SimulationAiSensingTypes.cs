using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public enum AiSensingMode
    {
        LegacyAiSensing = 0,
        SoAShadowAiSensing = 1,
        SoAAiSensing = 2,
    }

    public enum AiSoASensingShadowMismatchKind
    {
        None = 0,
        ShadowPurity = 1,
        InitialNearest = 2,
        CachedSelection = 3,
        PostSpecialSelection = 4,
    }

    public struct AiSoASensingShadowMismatch
    {
        public AiSoASensingShadowMismatchKind Kind;
        public int SelfSlot;
        public int ExpectedSelection;
        public int ActualSelection;
        public int ExpectedValue;
        public int ActualValue;
        public int ExpectedFlags;
        public int ActualFlags;
    }

}


