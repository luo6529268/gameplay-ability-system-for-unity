#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05HolderCopyCarrierEditorTests
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [TestCase(typeof(NTSDEntityRuntime), "HolderCopySlotIndex")]
        [TestCase(typeof(LF2Entity), "HolderCopySlot")]
        [TestCase(typeof(OPointCreateTask), "holderCopySlot")]
        [TestCase(typeof(BattleEcsLinkStore), "HolderCopySlot")]
        public void RetiredCarrierIsAbsent(Type type, string member)
        {
            Assert.That(type.GetMember(member, Members), Is.Empty);
        }

        [Test]
        public void HitDiagnosticHasNoRetiredProjection()
        {
            Type snapshot = typeof(BattleEcsHitExecutionPlan).GetNestedType("WriterEffectSnapshot", BindingFlags.NonPublic);
            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.GetField("TargetHolderCopySlot", Members), Is.Null);
        }

        [TestCase("Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs", "runtime.HolderCopySlotIndex")]
        [TestCase("Simulation/Lockstep/Checksum/BattleParitySnapshot.cs", "\"holderCopy\"")]
        public void ProjectionOmitsRetiredField(string path, string expression)
        {
            string source = File.ReadAllText(Path.Combine(UnityEngine.Application.dataPath, "NTSD/Scripts", path));
            Assert.That(source.Contains(expression), Is.False, path);
        }

        [TestCase("TargetHolderSlot", 32)]
        [TestCase("TargetRelationTeam", 34)]
        [TestCase("TargetWeaponFlightCounter", 35)]
        public void EffectiveDiagnosticBitsKeepTheirPositions(string field, int bit)
        {
            Type snapshot = typeof(BattleEcsHitExecutionPlan).GetNestedType("WriterEffectSnapshot", BindingFlags.NonPublic);
            object before = Activator.CreateInstance(snapshot);
            object after = Activator.CreateInstance(snapshot);
            snapshot.GetField(field, Members).SetValue(after, 1);
            MethodInfo difference = typeof(BattleEcsHitExecutionPlan).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(m => m.Name == "DifferenceMask" && m.GetParameters()[0].ParameterType.GetElementType() == snapshot);
            Assert.That((ulong)difference.Invoke(null, new[] { before, after }), Is.EqualTo(1UL << bit));
        }

        [Test]
        public void TaskClearResetsRealOwnerAndSpawnRelations()
        {
            var task = new OPointCreateTask
            {
                parent = new LF2Character(), targetWorld = new SimulationWorld(),
                ownerEntityIndex = 11, spawnerEntityIndex = 23, trackedTargetSlot = 31,
                relationTeam = 7, useExplicitRelationIdentity = true, inheritParentRelation = true,
            };
            task.Clear();
            Assert.That(task.parent, Is.Null);
            Assert.That(task.targetWorld, Is.Null);
            Assert.That(task.ownerEntityIndex, Is.EqualTo(-1));
            Assert.That(task.spawnerEntityIndex, Is.EqualTo(-1));
            Assert.That(task.trackedTargetSlot, Is.EqualTo(-1));
            Assert.That(task.relationTeam, Is.Zero);
            Assert.That(task.useExplicitRelationIdentity || task.inheritParentRelation, Is.False);
        }
    }
}
#endif
