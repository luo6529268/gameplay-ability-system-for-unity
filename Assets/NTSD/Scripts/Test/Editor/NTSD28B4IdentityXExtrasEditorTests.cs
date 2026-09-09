#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4IdentityXExtrasEditorTests
    {
        [TestCase((int)LF2ObjectType.ThrowWeapon, 900, 101, 0.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 120, 0, 2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 101, 0, -2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 900, 120, 2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 900, 101, -2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 900, 0, 0.0)]
        public void DerivedWeaponUsesIndependentRealAndAliasIdentityOperations(
            int objectType,
            int objectId,
            int alias,
            double expectedExtraX)
        {
            ProbeWeapon weapon = CreateWeapon(objectType, objectId, alias);
            weapon.Runtime.X = 0.0;
            weapon.Runtime.Vx = 10.0;

            weapon.InvokeFlightPhysics();

            Assert.That(weapon.Runtime.X, Is.EqualTo(expectedExtraX));
        }

        private static ProbeWeapon CreateWeapon(
            int objectType,
            int objectId,
            int alias)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponInSky,
                wait = 100,
                next = 0,
            };
            var data = new LF2CharacterData
            {
                name = "B4IdentityX",
                type_sub = alias,
                frames = new List<LF2FrameData> { frame },
            };
            var weapon = new ProbeWeapon { ObjectId = objectId };
            weapon.SetType(objectType);
            var wrapper = new LF2CharacterDataWrapper(objectId, data);
            weapon.FrameCache.Load(wrapper);
            weapon.SetRuntimeCharacterConfigResolverForSelfCheck(
                new RuntimeCharacterConfigResolver(id =>
                    id == objectId ? wrapper : null));
            weapon.ImmediateFrame(0);
            return weapon;
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void SetType(int objectType)
            {
                SetWeaponType(objectType);
            }

            internal void InvokeFlightPhysics()
            {
                base.WeaponFlightPhysics();
            }
        }
    }
}
#endif
