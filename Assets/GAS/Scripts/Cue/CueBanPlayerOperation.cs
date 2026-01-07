using GAS.Runtime;
using MoreMountains.TopDownEngine;
using UnityEngine;

namespace GAS.Cue
{
    public class CueBanPlayerOperation:GameplayCueDurational
    {
        public override GameplayCueDurationalSpec CreateSpec(GameplayCueParameters parameters)
        {
            return new CueBanPlayerOperationSpec(this, parameters);
        }
    }

    public class CueBanPlayerOperationSpec : GameplayCueDurationalSpec<CueBanPlayerOperation>
    {

        public CueBanPlayerOperationSpec(CueBanPlayerOperation cue, GameplayCueParameters parameters) : base(cue, parameters)
        {

        }

        public override void OnAdd()
        {

        }

        public override void OnRemove()
        {
 
        }

        public override void OnGameplayEffectActivate()
        {
        }

        public override void OnGameplayEffectDeactivate()
        {
        }

        public override void OnTick()
        {
        }
    }
}