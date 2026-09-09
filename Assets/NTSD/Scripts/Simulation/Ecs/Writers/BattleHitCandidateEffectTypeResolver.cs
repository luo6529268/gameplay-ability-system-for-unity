namespace NTSD.Simulation.Ecs
{
    internal static class BattleHitCandidateEffectTypeResolver
    {
        internal static bool Accepts(int effect, int targetObjectType)
        {
            switch (effect)
            {
                case 13:
                    return targetObjectType == 0;
                case 14:
                    return targetObjectType == 3;
                case 15:
                    return targetObjectType == 0 || targetObjectType == 3;
                case 16:
                    return targetObjectType == 1 ||
                           targetObjectType == 2 ||
                           targetObjectType == 3 ||
                           targetObjectType == 4 ||
                           targetObjectType == 6;
                default:
                    return true;
            }
        }
    }
}
