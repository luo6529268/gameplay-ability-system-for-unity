#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05ReleaseTickCarrierEditorTests
    {
        [Test]
        public void RetiredReleaseTickHasNoRuntimeCarrier()
        {
            Assert.That(typeof(NTSDEntityRuntime).GetMember("ReleaseTick",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), Is.Empty);
        }

        [TestCase(typeof(LF2WeaponBase), "ReleaseHeldWeaponRuntimeInternal", 1)]
        [TestCase(typeof(LF2WeaponReleaseFlowResolver), "ReleaseHeldWeaponRuntime", 1)]
        [TestCase(typeof(BattleHeldObjectWriter), "ClearLinks", 2)]
        public void ReleaseEntryHasOnlyEffectiveEntityParameters(Type type, string methodName, int count)
        {
            MethodInfo method = type.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.GetParameters().Length, Is.EqualTo(count));
            Assert.That(method.GetParameters().All(p => p.ParameterType == typeof(LF2Entity)), Is.True);
        }

        [TestCase("Simulation/Ecs/Core/BattleEcsWorld.cs", "runtime.ReleaseTick")]
        [TestCase("Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs", "runtime?.ReleaseTick")]
        [TestCase("Simulation/Lockstep/Checksum/BattleParitySnapshot.cs", "\"releaseTick\"")]
        public void CurrentCanonicalProjectionOmitsRetiredCarrier(string path, string retiredExpression)
        {
            string source = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "NTSD/Scripts", path));
            Assert.That(source.Contains(retiredExpression), Is.False, path + ": " + retiredExpression);
        }
    }
}
#endif
