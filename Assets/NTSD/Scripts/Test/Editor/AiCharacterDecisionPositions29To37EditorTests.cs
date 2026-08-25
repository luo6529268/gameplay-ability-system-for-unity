#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiCharacterDecisionPositions29To37EditorTests
    {
        private readonly AiCharacterDecisionModule module =
            new AiCharacterDecisionModule();

        [TestCase(34)]
        [TestCase(10)]
        [TestCase(5)]
        [TestCase(14)]
        public void Position29_OidGroupWritesLowHpDdj(int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 351;
            rows.Hp[0] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(29));
            Assert.That(input.ComboDdj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position29_ConsumesRand10BeforePpCheck()
        {
            AiSensingSnapshot rows = CreateRows(34);
            rows.Pp[0] = 350;
            rows.Hp[0] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(input.ComboDdj, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position30_SelectsFirstEligibleSlotWithoutObjectTypeFilter()
        {
            AiSensingSnapshot rows = CreateRows(34, 4);
            ConfigurePosition30Self(rows);
            IncludeCandidate(rows, 2, 1, 100, 500, 100, 0, 7);
            IncludeCandidate(rows, 3, 1, 50, 500, -50, 0, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                1000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out int selectedSlot,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(30));
            Assert.That(selectedSlot, Is.EqualTo(2));
            Assert.That(input.KeyRight, Is.EqualTo(1));
            Assert.That(input.ComboDuj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position30_MovementOnlyStillReturnsTrueWithoutDuj()
        {
            AiSensingSnapshot rows = CreateRows(34, 3);
            ConfigurePosition30Self(rows);
            rows.Facing[0] = 1;
            IncludeCandidate(rows, 2, 1, 100, 500, 100, 0, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                1000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out int selectedSlot,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(30));
            Assert.That(selectedSlot, Is.EqualTo(2));
            Assert.That(input.KeyRight, Is.EqualTo(1));
            Assert.That(input.ComboDuj, Is.Zero);
        }

        [Test]
        public void Position30_HonorsExplicitFirstTwentySlotBoundary()
        {
            AiSensingSnapshot rows = CreateRows(34, 21);
            ConfigurePosition30Self(rows);
            IncludeCandidate(rows, 20, 1, 100, 500, 100, 0, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                1000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out int selectedSlot,
                out _);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(selectedSlot, Is.EqualTo(-1));
            Assert.That(input.KeyRight, Is.Zero);
        }

        [TestCase(50, 1)]
        [TestCase(4, 1)]
        [TestCase(18, 1)]
        [TestCase(7, 1)]
        [TestCase(21, 1)]
        [TestCase(5, 2)]
        [TestCase(14, 2)]
        [TestCase(17, 1)]
        public void Position31_Label464GroupWritesDirectionalAttack(
            int objectId,
            int expectedCalls)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 151;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(12u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                true,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(31));
            Assert.That(input.ComboDra, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(expectedCalls));
        }

        [Test]
        public void Position31_Frame263JumpSideEffectContinuesIntoPosition32()
        {
            AiSensingSnapshot rows = CreateRows(50);
            rows.Pp[0] = 151;
            rows.X[1] = 95;
            rows.Frame[1] = 263;
            var input = new AiDecisionInputState { PrevJump = 1 };
            var random = new AiDecisionRandomStream(12u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(32));
            Assert.That(input.KeyJump, Is.EqualTo(1));
            Assert.That(input.PrevJump, Is.Zero);
            Assert.That(input.ComboDda, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position33_Oid35WritesDirectionalAttack()
        {
            AiSensingSnapshot rows = CreateRows(35);
            rows.Pp[0] = 121;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(12u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(33));
            Assert.That(input.ComboDra, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [TestCase(36)]
        [TestCase(16)]
        public void Position34_GateHitWithoutCandidateStillReturnsTrueAndBlocks35(int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 261;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(2u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out int selectedSlot);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(34));
            Assert.That(selectedSlot, Is.EqualTo(-1));
            Assert.That(input.ComboDuj, Is.Zero);
            Assert.That(input.ComboDua, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position34_HonorsFirstHundredBoundaryAndSelectsSlot99()
        {
            AiSensingSnapshot rows = CreateRows(36, 101);
            rows.Pp[0] = 201;
            IncludeCandidate(rows, 99, 1, 100, 500, 0, 0, 0);
            IncludeCandidate(rows, 100, 1, 50, 500, 0, 0, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(2u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out int selectedSlot);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(34));
            Assert.That(selectedSlot, Is.EqualTo(99));
            Assert.That(input.ComboDuj, Is.EqualTo(3));
        }

        [Test]
        public void Position34_IncludesSelfWhenSelfNeedsHelp()
        {
            AiSensingSnapshot rows = CreateRows(36);
            rows.Pp[0] = 201;
            rows.Hp[0] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(2u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out int selectedSlot);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(34));
            Assert.That(selectedSlot, Is.Zero);
            Assert.That(input.ComboDuj, Is.EqualTo(3));
        }

        [Test]
        public void Position35_OnlyRunsAfterPosition34RandomGateFails()
        {
            AiSensingSnapshot rows = CreateRows(36);
            rows.Pp[0] = 261;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(8u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(35));
            Assert.That(input.ComboDua, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position36_FirstBranchUsesRand5()
        {
            AiSensingSnapshot rows = CreateRows(38);
            rows.Pp[0] = 151;
            rows.X[1] = 150;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(2u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(36));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position36_SecondBranchRunsAfterRand5Fails()
        {
            AiSensingSnapshot rows = CreateRows(38);
            rows.Pp[0] = 201;
            rows.X[1] = 50;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(8u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(36));
            Assert.That(input.ComboDra, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position36_ThirdBranchPreservesThreeDrawOrder()
        {
            AiSensingSnapshot rows = CreateRows(38);
            rows.Pp[0] = 201;
            rows.X[1] = 250;
            rows.Z[1] = 20;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(23u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(36));
            Assert.That(input.ComboDuj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(3));
        }

        [Test]
        public void Position37_FirstBranchUsesRand3()
        {
            AiSensingSnapshot rows = CreateRows(39);
            rows.Pp[0] = 101;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(3u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(37));
            Assert.That(input.ComboDda, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position37_FailedFacingAfterRand3ContinuesToRand7()
        {
            AiSensingSnapshot rows = CreateRows(39);
            rows.Pp[0] = 101;
            rows.X[1] = 100;
            rows.Facing[0] = 1;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(3u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(37));
            Assert.That(input.ComboDda, Is.Zero);
            Assert.That(input.ComboDra, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position37_Oid10ObservesPosition29RandBeforeRand3()
        {
            AiSensingSnapshot rows = CreateRows(10);
            rows.Pp[0] = 101;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out _,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(37));
            Assert.That(input.ComboDda, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void UnrelatedOidSkipsPositions29Through37WithoutRngOrWrites()
        {
            AiSensingSnapshot rows = CreateRows(12);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 9);

            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out int selected30,
                out int selected34);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(selected30, Is.EqualTo(-1));
            Assert.That(selected34, Is.EqualTo(-1));
            Assert.That(random.Calls, Is.EqualTo(9));
            Assert.That(input.ComboDuj, Is.Zero);
        }

        [Test]
        public void WarmedPosition34HundredSlotScanAllocatesNoManagedMemory()
        {
            AiSensingSnapshot rows = CreateRows(36, 100);
            rows.Pp[0] = 201;
            bool valid = true;
            for (int iteration = 0; iteration < 8; iteration++)
                valid &= EvaluatePosition34(rows);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 4096; iteration++)
                valid &= EvaluatePosition34(rows);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(valid, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private bool Evaluate(
            AiSensingSnapshot rows,
            int nearestTargetDistance,
            bool sameZLane,
            int teammateScanSlotCount,
            int teamHelpScanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int position,
            out int selected30,
            out int selected34)
        {
            return module.TryEvaluatePositions29Through37(
                rows,
                0,
                1,
                nearestTargetDistance,
                sameZLane,
                teammateScanSlotCount,
                teamHelpScanSlotCount,
                ref input,
                ref random,
                out position,
                out selected30,
                out selected34);
        }

        private bool EvaluatePosition34(AiSensingSnapshot rows)
        {
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(2u, 0);
            bool matched = Evaluate(
                rows,
                10000,
                false,
                20,
                100,
                ref input,
                ref random,
                out int position,
                out int selected30,
                out int selected34);
            return matched &&
                   position == 34 &&
                   selected30 == -1 &&
                   selected34 == -1 &&
                   input.ComboDuj == 0 &&
                   random.Calls == 1;
        }

        private static void ConfigurePosition30Self(AiSensingSnapshot rows)
        {
            rows.Pp[0] = 500;
            rows.Hp[0] = 500;
            rows.HpMax[0] = 500;
        }

        private static void IncludeCandidate(
            AiSensingSnapshot rows,
            int slot,
            int team,
            int hp,
            int hpMax,
            int x,
            int z,
            int dataObjectType)
        {
            rows.Included[slot] = true;
            rows.DataObjectType[slot] = dataObjectType;
            rows.Team[slot] = team;
            rows.Hp[slot] = hp;
            rows.HpMax[slot] = hpMax;
            rows.X[slot] = x;
            rows.Z[slot] = z;
        }

        private static AiSensingSnapshot CreateRows(int selfObjectId, int capacity = 2)
        {
            var rows = new AiSensingSnapshot(capacity);
            rows.CapturedOccupancyEpoch = 1;
            rows.Included[0] = true;
            rows.Included[1] = true;
            rows.ObjectId[0] = selfObjectId;
            rows.ObjectId[1] = 100;
            rows.DataObjectType[0] = 0;
            rows.DataObjectType[1] = 0;
            rows.Hp[0] = 500;
            rows.Hp[1] = 500;
            rows.HpMax[0] = 500;
            rows.HpMax[1] = 500;
            rows.Team[0] = 1;
            rows.Team[1] = 2;
            rows.Facing[0] = 0;
            return rows;
        }
    }
}
#endif
