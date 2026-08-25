#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiCharacterDecisionFullDispatcherEditorTests
    {
        private readonly AiCharacterDecisionModule module =
            new AiCharacterDecisionModule();
        private readonly AiCharacterDecisionContext context =
            new AiCharacterDecisionContext(0, 800);

        [Test]
        public void Position38_Oid52TargetState3ConsumesRand10AndWritesDja()
        {
            AiSensingSnapshot rows = CreateRows(52, 2, 0);
            rows.Pp[0] = 126;
            rows.X[1] = 100;
            int[] moduli = new int[8];
            int[] raw = new int[8];
            int[] values = new int[8];
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(
                16u,
                0,
                true,
                moduli,
                raw,
                values);

            bool matched = Evaluate(
                rows,
                0,
                1,
                3,
                ref input,
                ref random,
                out int position,
                out int rowVisits);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(38));
            Assert.That(input.ComboDja, Is.EqualTo(3));
            Assert.That(random.Calls, Is.EqualTo(1));
            Assert.That(random.DrawCount, Is.EqualTo(1));
            Assert.That(moduli[0], Is.EqualTo(10));
            Assert.That(raw[0], Is.EqualTo(90));
            Assert.That(values[0], Is.Zero);
            Assert.That(rowVisits, Is.Zero);
        }

        [Test]
        public void Position39_Oid51FrameWindowWritesAttackWithoutRng()
        {
            AiSensingSnapshot rows = CreateRows(51, 2, 0);
            rows.Frame[0] = 270;
            rows.Z[1] = 20;
            var input = new AiDecisionInputState { PrevAttack = 1 };
            var random = new AiDecisionRandomStream(9u, 4);

            bool matched = Evaluate(
                rows,
                0,
                1,
                0,
                ref input,
                ref random,
                out int position,
                out int rowVisits);

            Assert.That(matched, Is.True);
            Assert.That(position, Is.EqualTo(39));
            Assert.That(input.PrevAttack, Is.Zero);
            Assert.That(input.KeyAttack, Is.EqualTo(1));
            Assert.That(random.Calls, Is.EqualTo(4));
            Assert.That(rowVisits, Is.Zero);
        }

        [Test]
        public void GlobalScan_LowAuthoritySelfDoesNotReadExtendedSlots()
        {
            AiSensingSnapshot rows = CreateRows(1, 402, 0);
            ConfigurePosition21(rows, 0, 1, 401);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(21u, 0);

            bool matched = Evaluate(
                rows,
                0,
                1,
                0,
                ref input,
                ref random,
                out int position,
                out int rowVisits);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(input.ComboDdj, Is.Zero);
            Assert.That(rowVisits, Is.EqualTo(400));
        }

        [Test]
        public void GlobalScan_ExtendedSelfReadsFullUnityCapacity()
        {
            AiSensingSnapshot rows = CreateRows(1, 402, 400);
            ConfigurePosition21(rows, 400, 1, 401);
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(21u, 0);

            bool matched = Evaluate(
                rows,
                400,
                1,
                0,
                ref input,
                ref random,
                out int position,
                out int rowVisits);

            Assert.That(matched, Is.False);
            Assert.That(position, Is.Zero);
            Assert.That(input.ComboDdj, Is.EqualTo(3));
            Assert.That(rowVisits, Is.EqualTo(402));
        }

        [Test]
        public void WarmedFullDispatcherEvaluationAllocatesNoManagedMemory()
        {
            AiSensingSnapshot rows = CreateRows(51, 2, 0);
            rows.Frame[0] = 270;
            rows.Z[1] = 20;
            bool valid = true;
            for (int iteration = 0; iteration < 8; iteration++)
                valid &= EvaluatePosition39(rows);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 4096; iteration++)
                valid &= EvaluatePosition39(rows);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(valid, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private bool Evaluate(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int position,
            out int rowVisits)
        {
            return module.TryEvaluatePositions7Through39(
                rows,
                self,
                target,
                targetState,
                10000,
                false,
                in context,
                ref input,
                ref random,
                out position,
                out rowVisits);
        }

        private bool EvaluatePosition39(AiSensingSnapshot rows)
        {
            var input = new AiDecisionInputState();
            var random = new AiDecisionRandomStream(9u, 4);
            bool matched = Evaluate(
                rows,
                0,
                1,
                0,
                ref input,
                ref random,
                out int position,
                out int rowVisits);
            return matched &&
                   position == 39 &&
                   input.KeyAttack == 1 &&
                   random.Calls == 4 &&
                   rowVisits == 0;
        }

        private static void ConfigurePosition21(
            AiSensingSnapshot rows,
            int self,
            int target,
            int candidate)
        {
            rows.Pp[self] = 80;
            rows.Hp[self] = 200;
            rows.Hp[target] = 300;
            rows.Team[self] = 1;
            rows.Team[target] = 2;
            rows.Included[candidate] = true;
            rows.DataObjectType[candidate] = 0;
            rows.Team[candidate] = 1;
            rows.Hp[candidate] = 400;
            rows.X[candidate] = 401;
            rows.Z[candidate] = 0;
        }

        private static AiSensingSnapshot CreateRows(
            int selfObjectId,
            int capacity,
            int self)
        {
            var rows = new AiSensingSnapshot(capacity);
            rows.CapturedOccupancyEpoch = 1;
            rows.Included[self] = true;
            rows.Included[1] = true;
            rows.ObjectId[self] = selfObjectId;
            rows.ObjectId[1] = 100;
            rows.DataObjectType[self] = 0;
            rows.DataObjectType[1] = 0;
            rows.Hp[self] = 500;
            rows.Hp[1] = 500;
            rows.HpMax[self] = 500;
            rows.HpMax[1] = 500;
            rows.Pp[self] = 500;
            rows.Team[self] = 1;
            rows.Team[1] = 2;
            rows.Facing[self] = 0;
            return rows;
        }
    }
}
#endif
