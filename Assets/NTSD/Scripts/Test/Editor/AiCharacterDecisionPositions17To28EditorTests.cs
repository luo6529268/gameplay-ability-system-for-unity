#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiCharacterDecisionPositions17To28EditorTests
    {
        private readonly AiCharacterDecisionModule module =
            new AiCharacterDecisionModule();

        [TestCase(10)]
        [TestCase(1)]
        public void Position17_RightFacingWritesDdaAndReturns(int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 101;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(17));
            Assert.That(input.ComboDda, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position17_LeftFacingReturnsWithoutDdaLikeCpp()
        {
            AiSensingSnapshot rows = CreateRows(10);
            rows.Pp[0] = 101;
            rows.X[0] = 100;
            rows.X[1] = 0;
            rows.Facing[0] = 1;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(17));
            Assert.That(input.ComboDda, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position18_Frame271WritesDuaWithoutRng()
        {
            AiSensingSnapshot rows = CreateRows(1);
            rows.Frame[0] = 271;
            rows.Y[1] = -1;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 4);

            bool matched = Evaluate(
                rows,
                12,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(18));
            Assert.That(input.ComboDua, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(4));
        }

        [TestCase(16)]
        [TestCase(8)]
        public void Position19_StateBypassStillConsumesRand10(int targetState)
        {
            AiSensingSnapshot rows = CreateRows(10);
            rows.X[1] = 300;
            rows.Vx[0] = -250.0;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                targetState,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(19));
            Assert.That(input.ComboDua, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position20_Rand15SuccessSkipsRand4()
        {
            AiSensingSnapshot rows = CreateRows(10);
            rows.Pp[0] = 201;
            rows.X[1] = 100;
            rows.Z[1] = 30;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(8u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(20));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position20_FallbackConsumesRand4BeforeTargetState()
        {
            AiSensingSnapshot rows = CreateRows(10);
            rows.Pp[0] = 201;
            rows.X[1] = 100;
            rows.Z[1] = 30;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(10u, 0);

            bool matched = Evaluate(
                rows,
                16,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(20));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(3));
        }

        [Test]
        public void Positions21And22ApplyBothSideEffectsThenContinue()
        {
            AiSensingSnapshot rows = CreateRows(10, 4);
            rows.Pp[0] = 501;
            rows.Hp[0] = 200;
            rows.Hp[1] = 199;
            rows.Z[1] = 30;
            IncludeCandidate(rows, 2, 1, 300, 400, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(1460u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                4,
                ref input,
                ref random,
                out int position,
                out int selectedSlot);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(selectedSlot, Is.EqualTo(2));
            Assert.That(input.ComboDdj, Is.EqualTo(3));
            Assert.That(input.ComboDuj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(3));
        }

        [Test]
        public void Position21SelectsStrictFarthestAndKeepsFirstTie()
        {
            AiSensingSnapshot rows = CreateRows(10, 5);
            ConfigurePosition21(rows);
            IncludeCandidate(rows, 2, 1, 301, 100, 0);
            IncludeCandidate(rows, 3, 1, 301, 400, 0);
            IncludeCandidate(rows, 4, 1, 301, -400, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(21u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                5,
                ref input,
                ref random,
                out _,
                out int selectedSlot);

            Assert.That(matched, Is.False);
            Assert.That(selectedSlot, Is.EqualTo(3));
            Assert.That(input.ComboDdj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position21HonorsExplicitFourHundredSlotAuthorityBoundary()
        {
            AiSensingSnapshot rows = CreateRows(10, 401);
            ConfigurePosition21(rows);
            IncludeCandidate(rows, 400, 1, 301, 600, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(21u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                400,
                ref input,
                ref random,
                out _,
                out int selectedSlot);

            Assert.That(matched, Is.False);
            Assert.That(selectedSlot, Is.EqualTo(-1));
            Assert.That(input.ComboDdj, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position21FiltersInactiveTypeTeamAndHpBeforeDistance()
        {
            AiSensingSnapshot rows = CreateRows(10, 7);
            ConfigurePosition21(rows);
            IncludeCandidate(rows, 2, 1, 301, 350, 0);
            IncludeCandidate(rows, 3, 2, 999, 900, 0);
            IncludeCandidate(rows, 4, 1, 300, 800, 0);
            IncludeCandidate(rows, 5, 1, 999, 700, 0);
            rows.DataObjectType[5] = 1;
            rows.X[6] = 1000;
            rows.Hp[6] = 999;
            rows.Team[6] = 1;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(21u, 0);

            Evaluate(
                rows,
                0,
                10000,
                7,
                ref input,
                ref random,
                out _,
                out int selectedSlot);

            Assert.That(selectedSlot, Is.EqualTo(2));
            Assert.That(input.ComboDdj, Is.EqualTo(3));
        }

        [TestCase(9)]
        [TestCase(2)]
        public void Position23_StateBypassStillConsumesRand10(int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.X[1] = 300;
            rows.Vx[0] = -200.0;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                16,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(23));
            Assert.That(input.ComboDda, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position24_PathAUsesRand13AfterPredictedRand10()
        {
            AiSensingSnapshot rows = CreateRows(9);
            rows.Pp[0] = 201;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(35u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(24));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position24_PathBConsumesDynamicModulusBeforeXRange()
        {
            AiSensingSnapshot rows = CreateRows(9);
            rows.Pp[0] = 201;
            rows.Hp[1] = 400;
            rows.X[1] = 200;
            int[] moduli = new int[4];
            int[] rawValues = new int[4];
            int[] values = new int[4];
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(
                21u,
                0,
                true,
                moduli,
                rawValues,
                values);

            bool matched = Evaluate(
                rows,
                12,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(24));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
            Assert.That(moduli[0], Is.EqualTo(10));
            Assert.That(moduli[1], Is.EqualTo(140));
        }

        [Test]
        public void Position25_ConsumesRand30BeforePpCheck()
        {
            AiSensingSnapshot rows = CreateRows(9);
            rows.Pp[0] = 151;
            rows.Hp[1] = 400;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(46u, 0);

            bool matched = Evaluate(
                rows,
                12,
                9999,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(25));
            Assert.That(input.ComboDua, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(3));
        }

        [TestCase(32)]
        [TestCase(19)]
        public void Position26_PathAUsesRand60(int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 201;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(25u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(26));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position27_FollowsDynamicPathBRngAndWritesDra()
        {
            AiSensingSnapshot rows = CreateRows(32);
            rows.Hp[1] = 400;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(8u, 0);

            bool matched = Evaluate(
                rows,
                12,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(27));
            Assert.That(input.ComboDra, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [TestCase(33, 1)]
        [TestCase(19, 2)]
        [TestCase(16, 1)]
        public void Position28_StateBypassStillConsumesRand5(
            int objectId,
            int expectedCalls)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 151;
            rows.X[1] = 300;
            rows.Vx[0] = -250.0;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = Evaluate(
                rows,
                16,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out _);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(28));
            Assert.That(input.ComboDua, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(expectedCalls));
        }

        [Test]
        public void Position17EarlyReturnPreventsPositions21And22SideEffects()
        {
            AiSensingSnapshot rows = CreateRows(10, 3);
            rows.Pp[0] = 501;
            rows.Hp[0] = 200;
            rows.Hp[1] = 199;
            rows.X[1] = 100;
            IncludeCandidate(rows, 2, 1, 300, 400, 0);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                3,
                ref input,
                ref random,
                out int position,
                out int selectedSlot);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(17));
            Assert.That(selectedSlot, Is.EqualTo(-1));
            Assert.That(input.ComboDdj, Is.Zero);
            Assert.That(input.ComboDuj, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void UnrelatedOidSkipsPositions17Through28WithoutRngOrWrites()
        {
            AiSensingSnapshot rows = CreateRows(12);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 9);

            bool matched = Evaluate(
                rows,
                0,
                10000,
                2,
                ref input,
                ref random,
                out int position,
                out int selectedSlot);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(selectedSlot, Is.EqualTo(-1));
            Assert.That(random.Calls, Is.EqualTo(9));
            Assert.That(input.ComboDda, Is.Zero);
            Assert.That(input.ComboDua, Is.Zero);
        }

        [Test]
        public void WarmedPosition21AuthorityScanAllocatesNoManagedMemory()
        {
            AiSensingSnapshot rows = CreateRows(10, 400);
            ConfigurePosition21(rows);
            IncludeCandidate(rows, 399, 1, 301, 400, 0);
            bool valid = true;
            for (int iteration = 0; iteration < 8; iteration++)
                valid &= EvaluatePosition21(rows);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 4096; iteration++)
                valid &= EvaluatePosition21(rows);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(valid, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private bool Evaluate(
            AiSensingSnapshot rows,
            int targetState,
            int nearestTargetDistance,
            int scanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int position,
            out int selectedSlot)
        {
            return module.TryEvaluatePositions17Through28(
                rows,
                0,
                1,
                targetState,
                nearestTargetDistance,
                scanSlotCount,
                ref input,
                ref random,
                out position,
                out selectedSlot);
        }

        private bool EvaluatePosition21(AiSensingSnapshot rows)
        {
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(21u, 0);
            bool matched = Evaluate(
                rows,
                0,
                10000,
                400,
                ref input,
                ref random,
                out int position,
                out int selectedSlot);
            return !matched &&
                   position == 0 &&
                   selectedSlot == 399 &&
                   input.ComboDdj == 3 &&
                   random.Calls == 2;
        }

        private static void ConfigurePosition21(AiSensingSnapshot rows)
        {
            rows.Pp[0] = 80;
            rows.Hp[0] = 200;
            rows.Hp[1] = 300;
            rows.Z[1] = 30;
        }

        private static void IncludeCandidate(
            AiSensingSnapshot rows,
            int slot,
            int team,
            int hp,
            int x,
            int z)
        {
            rows.Included[slot] = true;
            rows.DataObjectType[slot] = 0;
            rows.Team[slot] = team;
            rows.Hp[slot] = hp;
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
