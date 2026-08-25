#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiCharacterDecisionModuleEditorTests
    {
        private readonly AiCharacterDecisionModule module =
            new AiCharacterDecisionModule();
        private readonly AiCharacterDecisionContext context =
            new AiCharacterDecisionContext(0, 800);

        [TestCase(6)]
        [TestCase(18)]
        public void Position7_Oid6GroupWritesHorizontalJumpAndReturns(
            int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Pp[0] = 101;
            rows.X[1] = 100;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(7));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position7_State9DefendIsSideEffectWithoutEarlyReturn()
        {
            AiSensingSnapshot rows = CreateRows(6);
            rows.State[0] = 9;
            var input = new AiDecisionInputState { PrevDefend = 1 };
            var random = new AiDecisionRandomStream(3u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(input.KeyDefend, Is.EqualTo(1));
            Assert.That(input.PrevDefend, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [TestCase(7)]
        [TestCase(4)]
        [TestCase(10)]
        public void Position8_Oid7GroupFrameWindowWritesAttackWithoutRng(
            int objectId)
        {
            AiSensingSnapshot rows = CreateRows(objectId);
            rows.Frame[0] = 270;
            var input = new AiDecisionInputState { PrevAttack = 1 };
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                12,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(8));
            Assert.That(input.KeyAttack, Is.EqualTo(1));
            Assert.That(input.PrevAttack, Is.Zero);
            Assert.That(random.Calls, Is.Zero);
        }

        [Test]
        public void Position9_Oid7CloseUsesOneRand5AndWritesDuj()
        {
            AiSensingSnapshot rows = CreateRows(7);
            rows.Pp[0] = 321;
            rows.Hp[0] = 500;
            rows.Hp[1] = 400;
            rows.X[1] = 60;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(2u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(9));
            Assert.That(input.ComboDuj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position10_Oid7MidfarUsesRand20BeforeRand100Fallback()
        {
            AiSensingSnapshot rows = CreateRows(7);
            rows.Pp[0] = 201;
            rows.X[1] = 150;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(19u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(10));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position11_Oid7FacingObservesPriorRand100ThenRand15()
        {
            AiSensingSnapshot rows = CreateRows(7);
            rows.Pp[0] = 201;
            rows.X[1] = 70;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(8u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(11));
            Assert.That(input.ComboDdj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(2));
        }

        [Test]
        public void Position12_Oid7Frame255AttackReturnsAfterPriorRand100()
        {
            AiSensingSnapshot rows = CreateRows(7);
            rows.Frame[0] = 255;
            rows.X[0] = 700;
            rows.X[1] = 500;
            var input = new AiDecisionInputState { PrevAttack = 1 };
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(12));
            Assert.That(input.KeyAttack, Is.EqualTo(1));
            Assert.That(input.PrevAttack, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position12_Oid7Frame255DirectionWriteContinuesWithoutMatch()
        {
            AiSensingSnapshot rows = CreateRows(7);
            rows.Frame[0] = 255;
            rows.X[0] = 100;
            rows.X[1] = 150;
            rows.Z[1] = 10;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(input.KeyDown, Is.EqualTo(1));
            Assert.That(input.KeyUp, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position13_Oid8FirstGateUsesRand250()
        {
            AiSensingSnapshot rows = CreateRows(8);
            rows.Pp[0] = 201;
            rows.X[1] = 300;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(65u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(13));
            Assert.That(input.ComboDrj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position13_Oid8FinalGatePreservesConditionalThreeDrawOrder()
        {
            AiSensingSnapshot rows = CreateRows(8);
            rows.Pp[0] = 321;
            rows.X[1] = 250;
            rows.Z[1] = 60;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(27u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(13));
            Assert.That(input.ComboDdj, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(3));
        }

        [Test]
        public void Position14_Oid11EarlyReturnPreventsPosition15SideEffect()
        {
            AiSensingSnapshot rows = CreateRows(11);
            rows.Pp[0] = 151;
            rows.X[1] = 100;
            rows.Y[1] = -1;
            rows.HitJ[0] = 290;
            var input = new AiDecisionInputState { PrevDefend = 1 };
            var random = new AiDecisionRandomStream(16u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(14));
            Assert.That(input.ComboDda, Is.EqualTo(3));
            Assert.That(input.KeyDefend, Is.Zero);
            Assert.That(input.PrevDefend, Is.EqualTo(1));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void Position15_Oid11WritesDefendThenContinuesToFailedPosition16()
        {
            AiSensingSnapshot rows = CreateRows(11);
            rows.Y[1] = -1;
            rows.HitJ[0] = 290;
            var input = new AiDecisionInputState { PrevDefend = 1 };
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(input.KeyDefend, Is.EqualTo(1));
            Assert.That(input.PrevDefend, Is.Zero);
            Assert.That(input.ComboDua, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [TestCase(16)]
        [TestCase(8)]
        public void Position16_Oid11StateBypassStillConsumesRand5(
            int targetState)
        {
            AiSensingSnapshot rows = CreateRows(11);
            rows.Pp[0] = 201;
            rows.X[1] = 300;
            rows.Vx[0] = -250.0;
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 0);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                targetState,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(16));
            Assert.That(input.ComboDua, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
        }

        [Test]
        public void UnrelatedOidSkipsPositions7Through16WithoutRngOrWrites()
        {
            AiSensingSnapshot rows = CreateRows(12);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(0u, 9);

            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(random.Calls, Is.EqualTo(9));
            Assert.That(input.ComboDrj, Is.Zero);
            Assert.That(input.KeyAttack, Is.Zero);
        }

        [Test]
        public void WarmedPosition7ModuleEvaluationAllocatesNoManagedMemory()
        {
            AiSensingSnapshot rows = CreateRows(6);
            rows.Pp[0] = 101;
            rows.X[1] = 100;
            bool allMatched = true;
            for (int iteration = 0; iteration < 8; iteration++)
                allMatched &= EvaluatePosition7(rows);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 4096; iteration++)
                allMatched &= EvaluatePosition7(rows);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allMatched, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private bool EvaluatePosition7(AiSensingSnapshot rows)
        {
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(16u, 0);
            bool matched = module.TryEvaluatePositions7Through16(
                rows,
                0,
                1,
                0,
                in context,
                ref input,
                ref random,
                out int position);
            return matched &&
                   position == 7 &&
                   input.ComboDrj == 3 &&
                   random.Calls == 1;
        }

        private static AiSensingSnapshot CreateRows(int selfObjectId)
        {
            var rows = new AiSensingSnapshot(2);
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
