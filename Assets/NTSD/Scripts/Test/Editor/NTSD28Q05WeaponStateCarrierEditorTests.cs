#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05WeaponStateCarrierEditorTests
    {
        [Test]
        public void RetiredWeaponStateHasNoRuntimeCarrier()
        {
            Assert.That(typeof(NTSDEntityRuntime).GetMember("WeaponState",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), Is.Empty);
        }

        [TestCase("Simulation/Ecs/Core/BattleEcsWorld.cs", "runtime.WeaponState")]
        [TestCase("Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs", "runtime.WeaponState")]
        [TestCase("Simulation/Lockstep/Checksum/BattleParitySnapshot.cs", "\"weaponState\"")]
        public void ProjectionOmitsRetiredCarrier(string path, string expression)
        {
            string source = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "NTSD/Scripts", path));
            Assert.That(source.Contains(expression), Is.False, path + ": " + expression);
        }

        [TestCase(1002)]
        [TestCase(2000)]
        public void WeaponStateResolverReadsActualFrame(int state)
        {
            var weapon = new LF2Weapon { ObjectId = 124 };
            var data = new LF2CharacterData
            {
                type_sub = 4,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData { frameId = 0, state = state, wait = 100, next = 0 },
                    new LF2FrameData { frameId = 40, state = 1000, wait = 100, next = 40 },
                },
            };
            weapon.FrameCache.Load(new LF2CharacterDataWrapper(124, data));
            weapon.ImmediateFrame(0);
            Assert.That(weapon.GetResolvedWeaponStateForExternalUse(), Is.EqualTo(state));
            weapon.ImmediateFrame(40);
            Assert.That(weapon.GetResolvedWeaponStateForExternalUse(), Is.EqualTo(1000));
        }
    }
}
#endif
