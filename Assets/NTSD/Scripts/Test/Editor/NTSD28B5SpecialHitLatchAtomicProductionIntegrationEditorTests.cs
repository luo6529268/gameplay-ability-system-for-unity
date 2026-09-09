#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5SpecialHitLatchAtomicProductionIntegrationEditorTests
    {
        [Test]
        public void SharedConsumer_UsesDedicatedLatchInsteadOfHitConfirm2()
        {
            string source = ReadProjectSource(
                "NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs");

            StringAssert.Contains(
                "attacker.Runtime.SpecialHitLatch0EB &&",
                source);
            StringAssert.DoesNotContain(
                "attacker.HitConfirm2 != 0 &&",
                source);
        }

        [Test]
        public void Type3Producers_WriteDedicatedLatchAndPreserveWeaponHitConfirm2Writers()
        {
            string source = ReadProjectSource(
                "NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs");

            Assert.That(
                Count(source, ".Runtime.SpecialHitLatch0EB = true;"),
                Is.EqualTo(4));
            Assert.That(
                Count(source, ".HitConfirm2 = 1;"),
                Is.EqualTo(3),
                "The three ordinary weapon/object confirmation writes remain independent.");
        }

        [Test]
        public void HitPlan_ProjectsAndComparesDedicatedLatchWithoutChangingObjectProjection()
        {
            string source = ReadProjectSource(
                "NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs");

            StringAssert.Contains(
                "TargetSpecialHitLatch0EB = target?.Runtime?.SpecialHitLatch0EB ?? false",
                source);
            Assert.That(
                Count(source, "projection.TargetSpecialHitLatch0EB = true;"),
                Is.EqualTo(5));
            Assert.That(
                Count(source, "projection.TargetHitConfirm2 = 1;"),
                Is.EqualTo(1),
                "The ordinary object damage projection must keep HitConfirm2.");
            StringAssert.Contains(
                "expected.TargetSpecialHitLatch0EB != actual.TargetSpecialHitLatch0EB",
                source);
        }

        [Test]
        public void PerTickClearBoundaries_PreserveDedicatedLatchAndClearHitConfirm2()
        {
            var entity = new LF2Character();
            entity.HitConfirm2 = 7;
            entity.Runtime.SpecialHitLatch0EB = true;

            entity.ClearHitCandidateCarriers();

            Assert.That(entity.HitConfirm2, Is.Zero);
            Assert.That(entity.Runtime.SpecialHitLatch0EB, Is.True);

            entity.HitConfirm2 = 7;
            var pass = new BattleEcsCharacterPostFrameTailPass();
            pass.SetMode(BattleEcsCharacterPostFrameTailPassMode.DataOriented);

            Assert.That(pass.TryExecute(entity), Is.True);
            Assert.That(entity.HitConfirm2, Is.Zero);
            Assert.That(entity.Runtime.SpecialHitLatch0EB, Is.True);
        }

        private static string ReadProjectSource(string relativePath)
        {
            return File.ReadAllText(Path.Combine(Application.dataPath, relativePath));
        }

        private static int Count(string source, string value)
        {
            int count = 0;
            int offset = 0;
            while ((offset = source.IndexOf(value, offset, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                offset += value.Length;
            }

            return count;
        }
    }
}
#endif
