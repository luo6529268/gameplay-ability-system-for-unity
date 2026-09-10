#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6Kind2PickupPureTransactionProductionEditorTests
    {
        [TestCase(1, 120, 1, 101, -1, 115)]
        [TestCase(1, 124, 1, 101, -1, 115)]
        [TestCase(1, 100, 1, 1, -1, 115)]
        [TestCase(2, 150, 1, 2, -2, 116)]
        [TestCase(4, 124, 1, 4, -4, 115)]
        [TestCase(6, 123, 1, 6, -6, 115)]
        [TestCase(6, 123, 0, 4, -4, 115)]
        [TestCase(6, 123, -1, 4, -4, 115)]
        public void RelationTypes_ProduceExactOrderedWrites(int type, int oid, int hp, int holderRelation, int childRelation, int action)
        {
            BattlePickupTransactionPlan plan = Build(type, oid, hp);
            Assert.That(plan.Outcome, Is.EqualTo(BattlePickupTransactionOutcome.RelationEstablished));
            var expected = new List<string>();
            if (type == 6 && hp <= 0) expected.Add("SetTargetWeaponHp:0");
            expected.AddRange(new[] { "SetHolderRelationCount:6", $"SetHolderRelation:{holderRelation}",
                $"SetTargetRelation:{childRelation}", "SetHolderLinkedChildSlot:9", "SetTargetLinkedParentSlot:37",
                "SetTargetOwnerSlot:37", "SetTargetBattleGroup:7", $"SetHolderAction:{action}", "SetHolderFrameCounter:0" });
            CollectionAssert.AreEqual(expected, Writes(plan));
            Assert.That(plan.Applied && plan.RelationEstablished, Is.True);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(101)]
        [TestCase(-1)]
        public void ExistingRelation_DoesNotIncrementOrRepresentOldChildCleanup(int oldRelation)
        {
            var plan = Build(2, 150, 1, oldRelation: oldRelation);
            Assert.That(plan.RelationEstablished, Is.True);
            Assert.That(Writes(plan).Any(s => s.StartsWith("SetHolderRelationCount:")), Is.False);
            Assert.That(plan.OperationCount, Is.EqualTo(8));
            CollectionAssert.DoesNotContain(Enum.GetNames(typeof(BattlePickupWriteKind)), "Free");
            CollectionAssert.DoesNotContain(Enum.GetNames(typeof(BattlePickupWriteKind)), "UnlinkOldChild");
        }

        [TestCase(1, 0)]
        [TestCase(1, 333)]
        [TestCase(1, -888)]
        [TestCase(1, 1000)]
        [TestCase(3, 0)]
        [TestCase(3, 333)]
        [TestCase(3, -888)]
        [TestCase(3, 1000)]
        public void TargetWpoint_IsLiteralLastWriteEvenWithoutRelation(int type, int action)
        {
            var plan = Build(type, 100, 1, action: action);
            Assert.That(plan.Applied, Is.True);
            Assert.That(plan.RelationEstablished, Is.EqualTo(type == 1));
            string[] writes = Writes(plan);
            Assert.That(writes[writes.Length - (action == 0 ? 1 : 2)], Is.EqualTo("SetHolderFrameCounter:0"));
            if (action != 0) Assert.That(writes.Last(), Is.EqualTo("SetHolderAction:" + action));
            if (type == 3) Assert.That(plan.OperationCount, Is.EqualTo(action == 0 ? 1 : 2));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void MissingTargetFrame_StillResetsCounterButIgnoresStaleWpoint(int type)
        {
            var plan = Build(type, 100, 1, action: -888, framePresent: false);
            Assert.That(plan.Applied, Is.True);
            Assert.That(Writes(plan).Last(), Is.EqualTo("SetHolderFrameCounter:0"));
            Assert.That(Writes(plan).Contains("SetHolderAction:-888"), Is.False);
        }

        [TestCase(0)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(99)]
        public void UnsupportedTargetType_IsAppliedTailOnly(int type)
        {
            var plan = Build(type, 100, 1, action: 333);
            Assert.That(plan.Outcome, Is.EqualTo(BattlePickupTransactionOutcome.AppliedWithoutRelation));
            CollectionAssert.AreEqual(new[] { "SetHolderFrameCounter:0", "SetHolderAction:333" }, Writes(plan));
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        [TestCase(false, true)]
        public void UnsupportedRuleAdmission_HasNoTailOrPartialWrites(bool audited, bool empty)
        {
            var rules = BattleLockedPickupWeaponThrowRules.FromAuditedTable(audited, empty ? Array.Empty<int>() : new[] { 120, 124 });
            var plan = BattlePickupTransactionPlan.Create(Input(6, 123, 0, 0, -888, true), rules);
            Assert.That(plan.Outcome, Is.EqualTo(BattlePickupTransactionOutcome.Unsupported));
            Assert.That(plan.Applied, Is.False);
            Assert.That(plan.OperationCount, Is.Zero);
        }

        [Test]
        public void WarmedPlans_DoNotAllocate()
        {
            var input = Input(6, 123, 0, 0, -888, true);
            for (int i = 0; i < 32; i++) _ = BattlePickupTransactionPlan.Create(input, BattleLockedPickupWeaponThrowRules.Locked);
            long before = GC.GetAllocatedBytesForCurrentThread();
            int sum = 0;
            for (int i = 0; i < 4096; i++)
            {
                var plan = BattlePickupTransactionPlan.Create(input, BattleLockedPickupWeaponThrowRules.Locked);
                sum += plan.OperationCount;
                for (int j = 0; j < plan.OperationCount; j++) _ = plan.GetOperation(j);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            Assert.That(sum, Is.EqualTo(4096 * 11));
        }

        private static BattlePickupTransactionInput Input(int type, int oid, int hp, int oldRelation, int action, bool framePresent)
            => new BattlePickupTransactionInput(type, oid, hp, 31, 37, 9, 7, oldRelation, 5, 77, 8, framePresent, action);
        private static BattlePickupTransactionPlan Build(int type, int oid, int hp, int oldRelation = 0, int action = 0, bool framePresent = true)
            => BattlePickupTransactionPlan.Create(Input(type, oid, hp, oldRelation, action, framePresent), BattleLockedPickupWeaponThrowRules.Locked);
        private static string[] Writes(BattlePickupTransactionPlan plan)
        {
            var result = new string[plan.OperationCount];
            for (int i = 0; i < result.Length; i++) { var op = plan.GetOperation(i); result[i] = op.Kind + ":" + op.Value; }
            return result;
        }
    }
}
#endif
