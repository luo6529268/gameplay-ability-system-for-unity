using NTSD.GAS;
using System;
using UnityEngine;

namespace GAS.Runtime
{
    [CreateAssetMenu(fileName = "UniversalFrameDriven", menuName = "GAS/Ability/UniversalFrameDriven")]
    public class AAUniversalFrameDriven : AbilityAsset
    {
        public override Type AbilityType()
        {
            return typeof(UniversalFrameDrivenAbility);
        }
    }
}