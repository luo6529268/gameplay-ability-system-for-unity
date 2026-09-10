#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using NUnit.Framework;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6NativeImpactPureCoreEditorTests
    {
        private static readonly int[] LockedIds = { 201, 202 };

        [Test, Combinatorial]
        public void KindTypeMatrix([Values(10, 11, 17, 18)] int kind,
            [Values(0, 1, 2, 3, 4, 6)] int type)
        {
            var input = Input(type);
            input.Kind = kind;
            bool accepted = type != 3 && (kind != 17 || type == 0) && (kind != 18 || type != 0);
            Check(input, accepted);
        }

        [Test, Combinatorial]
        public void Kind11EnvironmentGate([Values(0, 1, 2, 4, 6)] int type,
            [Values(-1, 0, 1)] int environment)
        {
            var input = Input(type);
            input.Kind = 11;
            input.Environment = environment;
            Check(input, environment < 0);
        }

        [Test, Combinatorial]
        public void OwnerPreflightIsCharacterOnly([Values(0, 2)] int type,
            [Values(false, true)] bool firstValid, [Values(false, true)] bool creditValid)
        {
            var input = Input(type);
            input.FirstOwnerValid = firstValid;
            input.CreditOwnerValid = creditValid;
            Check(input, type != 0 || (firstValid && creditValid));
        }

        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        [TestCase(0, 0, true)]
        [TestCase(399, 399, true)]
        public void OwnerPhysicalSlots(int first, int credit, bool accepted)
        {
            var input = Input(0);
            input.FirstOwnerSlot = first;
            input.CreditSlot = credit;
            input.AttackerSlot = first;
            Check(input, accepted);
        }

        [Test, Combinatorial]
        public void RuleAndRespondLiterals([Values(47, 0, -9)] int rule,
            [Values(0, 99, -12)] int respond)
        {
            var input = Input(0);
            input.Rule94 = rule;
            input.Respond = respond;
            Check(input, true);
        }

        [Test, Combinatorial]
        public void ObjectImmunityGate([Values(1, 2, 4, 6)] int type,
            [Values(0, 1, 2, 3, 4)] int tableCase)
        {
            var input = Input(type);
            if (tableCase == 0) input.TargetObjectId = 201;
            if (tableCase == 1) input.TargetObjectId = 202;
            if (tableCase == 2) input.ImmuneObjectIds = Array.Empty<int>();
            if (tableCase == 3) input.ImmunityAudited = false;
            Check(input, type == 2 || tableCase == 4);
        }

        [Test, Combinatorial]
        public void ObjectActionPreservation([Values(1, 2, 4, 6)] int type,
            [Values(0, 1000, 2000)] int state)
        {
            var input = Input(type);
            input.TargetState = state;
            Check(input, true);
        }

        [Test, Combinatorial]
        public void IntegerPlaneAndVerticalThreshold([Values(0, 1, 2, 4, 6)] int type,
            [Values(-3, -2, 0)] int integerY, [Values(-7.0, -6.0, -5.9)] double vy)
        {
            var input = Input(type);
            input.YInt = integerY;
            input.Y = integerY - 0.875;
            input.Vy = vy;
            Check(input, true);
        }

        [TestCase(0.0)]
        [TestCase(-0.0)]
        [TestCase(1.0)]
        [TestCase(-13.7)]
        [TestCase(123456789.123)]
        public void DivisionHasNativeDoubleBitPattern(double velocity)
        {
            var input = Input(0);
            input.Vx = velocity;
            input.Vz = -velocity;
            Check(input, true);
        }

        [TestCase(0)]
        [TestCase(9)]
        [TestCase(15)]
        [TestCase(-1)]
        public void NonImpactHasNoWrites(int kind)
        {
            var input = Input(0);
            input.Kind = kind;
            Check(input, false);
        }

        [Test]
        public void WarmedResolveAndOrderedReadAllocateZeroBytes()
        {
            var input = Input(0);
            int count = 0;
            for (int i = 0; i < 128; i++)
            {
                var warm = BattleNativeImpactResolver.Resolve(in input);
                for (int j = 0; j < warm.OperationCount; j++) count += (int)warm.GetOperation(j).Kind;
            }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1024; i++)
            {
                var plan = BattleNativeImpactResolver.Resolve(in input);
                for (int j = 0; j < plan.OperationCount; j++) count += (int)plan.GetOperation(j).Kind;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(count, Is.GreaterThan(0));
            Assert.That(allocated, Is.Zero);
        }

        private static BattleNativeImpactInput Input(int type)
        {
            return new BattleNativeImpactInput
            {
                Kind = 10, TargetType = type, TargetState = 19, TargetAction = -45,
                TargetObjectId = 123, Environment = -9, CatchSource = 0x2345, ImpactSource = 77,
                FirstOwnerValid = true, CreditOwnerValid = true,
                FirstOwnerSlot = 0, CreditSlot = 399, AttackerSlot = 19,
                Rule94 = 20, ImmunityAudited = true, ImmuneObjectIds = LockedIds,
                YInt = -3, Y = -3.875, Vx = 13.7, Vy = -5.9, Vz = -17.3,
                PendingX = 71, PendingY = 72, PendingZ = 73,
            };
        }

        private static void Check(BattleNativeImpactInput input, bool accepted)
        {
            BattleNativeImpactInput before = input;
            var plan = BattleNativeImpactResolver.Resolve(in input);
            Assert.That(plan.Applied, Is.EqualTo(accepted));
            Assert.That(input, Is.EqualTo(before), "pure input remains unchanged");
            Assert.That(plan.RngDrawCount, Is.Zero);
            if (!accepted)
            {
                Assert.That(plan.OperationCount, Is.Zero, "rejection is an atomic zero-write plan");
                return;
            }

            var expected = new List<BattleNativeImpactOperation>();
            if (input.TargetType == 0)
            {
                Add(expected, BattleNativeImpactWriteKind.Environment, -(input.Rule94 > 0 ? input.Rule94 : 20));
                Add(expected, BattleNativeImpactWriteKind.CatchSource, 0x2000 + input.CreditSlot);
                Add(expected, BattleNativeImpactWriteKind.ImpactSource, input.AttackerSlot);
            }
            else if (input.TargetState != (input.TargetType == 2 ? 2000 : 1000))
            {
                Add(expected, BattleNativeImpactWriteKind.Action, 0);
            }
            Add(expected, BattleNativeImpactWriteKind.Vx, input.Vx / 1.07);
            Add(expected, BattleNativeImpactWriteKind.Vz, input.Vz / 1.07);
            Add(expected, BattleNativeImpactWriteKind.PendingX, input.Vx / 1.07);
            Add(expected, BattleNativeImpactWriteKind.PendingZ, input.Vz / 1.07);
            if (input.TargetType == 0)
                Add(expected, BattleNativeImpactWriteKind.Action, input.Respond == 0 ? 182 : input.Respond);
            if (input.YInt >= -2)
            {
                Add(expected, BattleNativeImpactWriteKind.YInt, -2);
                Add(expected, BattleNativeImpactWriteKind.Y, -2.0);
                Add(expected, BattleNativeImpactWriteKind.Vy, -6.0);
            }
            else if (input.Vy > -6.0)
            {
                double vy = input.Vy - (input.TargetType == 0 ? 3.0 : 2.3);
                Add(expected, BattleNativeImpactWriteKind.Vy, vy);
                Add(expected, BattleNativeImpactWriteKind.PendingY, vy);
            }
            Assert.That(plan.OperationCount, Is.EqualTo(expected.Count));
            for (int i = 0; i < expected.Count; i++)
            {
                var op = plan.GetOperation(i);
                Assert.That(op.Kind, Is.EqualTo(expected[i].Kind), "operation " + i);
                Assert.That(BitConverter.DoubleToInt64Bits(op.Value),
                    Is.EqualTo(BitConverter.DoubleToInt64Bits(expected[i].Value)), "value bits " + i);
            }
            Assert.Throws<ArgumentOutOfRangeException>(() => plan.GetOperation(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => plan.GetOperation(plan.OperationCount));
        }

        private static void Add(List<BattleNativeImpactOperation> ops, BattleNativeImpactWriteKind kind, double value)
        {
            ops.Add(new BattleNativeImpactOperation(kind, value));
        }
    }
}
#endif
