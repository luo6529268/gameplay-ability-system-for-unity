using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiHeldRandomEditorTests
    {
        private const int Self = 20;
        private const int Target = 0;
        private const int Held = 1;

        [Test]
        public void InvalidLinkedSlot_DoesNotConsume28()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 150, targetX: 200);
            rows.TargetSlot[Self] = -1;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var rng = CreateStream(CreateOwnerWithTable(1), new uint[2]);

            bool continues = Evaluate(
                rows, sameZLane: false, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.True);
            Assert.That(rng.DrawCount, Is.Zero);
        }

        [Test]
        public void Initial28Positive_StopsOuterImmediately()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 150, targetX: 200);
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(0), sites);

            bool continues = Evaluate(
                rows, sameZLane: false, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.False);
            AssertSites(sites, rng.DrawCount, 0x28u);
        }

        [Test]
        public void EnemyTargetWithSubjectGroupBlocker_29RaisesNativeJump()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 150, targetX: 200);
            rows.State[Self] = 2;
            SetRow(rows, 2, oid: 3, state: 0, x: 100, z: 0, team: 1);
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(6, 9), sites);

            bool continues = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.True);
            AssertSites(sites, rng.DrawCount, 0x28u, 0x29u);
            Assert.That(input.KeyDefend, Is.EqualTo(1),
                "legacy KeyDefend maps to native jump");
            Assert.That(input.KeyJump, Is.Zero,
                "the enemy target is not the blocker owner");
        }

        [TestCase(114, true)]
        [TestCase(115, false)]
        public void OrdinaryWeapon_2AUsesStrict115PredictedRange(
            int targetX,
            bool consumes2A)
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 100, targetX);
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[5];
            var rng = CreateStream(CreateOwnerWithTable(6, 7, 0), sites);

            Assert.That(Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness), Is.True);

            if (consumes2A)
            {
                AssertSites(sites, rng.DrawCount, 0x28u, 0x2Au, 0x2Cu);
                Assert.That(input.KeyJump, Is.EqualTo(1),
                    "legacy KeyJump maps to native attack");
            }
            else
            {
                AssertSites(sites, rng.DrawCount, 0x28u, 0x2Cu);
                Assert.That(input.KeyJump, Is.Zero);
            }
        }

        [Test]
        public void Oid124_Consumes2BBetween2AAnd2C()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 124, targetX: 200);
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[5];
            var rng = CreateStream(CreateOwnerWithTable(6, 58, 0), sites);

            Assert.That(Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness), Is.True);

            AssertSites(sites, rng.DrawCount, 0x28u, 0x2Bu, 0x2Cu);
            Assert.That(input.KeyJump, Is.EqualTo(1));
        }

        [TestCase(299, true)]
        [TestCase(300, false)]
        public void HeavyWeapon_2DUsesStrict300By6Range(
            int targetX,
            bool consumes2D)
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 150, targetX);
            rows.Z[Target] = 5;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(6, 15), sites);

            Assert.That(Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness), Is.True);

            if (consumes2D)
            {
                AssertSites(sites, rng.DrawCount, 0x28u, 0x2Du);
                Assert.That(input.KeyJump, Is.EqualTo(1));
            }
            else
            {
                AssertSites(sites, rng.DrawCount, 0x28u);
                Assert.That(input.KeyJump, Is.Zero);
            }
        }

        [Test]
        public void WeaponRunLeftBoundary_Consumes2EThen2FAndStops()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 122, targetX: 300);
            rows.X[Self] = 100;
            rows.State[Self] = 2;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[6];
            var rng = CreateStream(CreateOwnerWithTable(6, 0, 10, 7), sites);

            bool continues = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.False);
            AssertSites(sites, rng.DrawCount, 0x28u, 0x29u, 0x2Eu, 0x2Fu);
            Assert.That(input.KeyRight, Is.EqualTo(1));
            Assert.That(input.KeyDefend, Is.EqualTo(1),
                "0x2F state2 result raises native jump");
        }

        [Test]
        public void WeaponRunRightBoundary_Consumes30Then31EvenOutsideState2()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 123, targetX: 500);
            rows.X[Self] = 700;
            rows.State[Self] = 0;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[5];
            var rng = CreateStream(CreateOwnerWithTable(6, 11, 8), sites);

            bool continues = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.False);
            AssertSites(sites, rng.DrawCount, 0x28u, 0x30u, 0x31u);
            Assert.That(input.KeyLeft, Is.EqualTo(1));
            Assert.That(input.KeyDefend, Is.Zero);
        }

        [TestCase(500, 0x32u, 1, 0)]
        [TestCase(300, 0x33u, 0, 1)]
        public void WeaponRunCloseBranch_ConsumesDirectionalSiteAndStops(
            int targetX,
            uint expectedSite,
            int expectedLeft,
            int expectedRight)
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 122, targetX);
            rows.X[Self] = 400;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(6, 8), sites);

            bool continues = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.False);
            AssertSites(sites, rng.DrawCount, 0x28u, expectedSite);
            Assert.That(input.KeyLeft, Is.EqualTo(expectedLeft));
            Assert.That(input.KeyRight, Is.EqualTo(expectedRight));
        }

        [TestCase(800, 3, 0)]
        [TestCase(0, 0, 3)]
        public void WeaponRunFinalBranch_35ArmsNativeComboIndex4Or5(
            int targetX,
            int expectedDda,
            int expectedDdj)
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 122, targetX);
            rows.X[Self] = 400;
            rows.State[Self] = 0;
            rows.Pp[Self] = 300;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[5];
            var rng = CreateStream(CreateOwnerWithTable(6, 3, 0), sites);

            bool continues = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.True);
            AssertSites(sites, rng.DrawCount, 0x28u, 0x34u, 0x35u);
            Assert.That(input.ComboDda, Is.EqualTo(expectedDda));
            Assert.That(input.ComboDdj, Is.EqualTo(expectedDdj));
            Assert.That(input.ComboDrj, Is.Zero);
            Assert.That(input.ComboDlj, Is.Zero);
        }

        [Test]
        public void WeaponRunDirectAttackShortCircuit_DoesNotConsume35()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 5, heldOid: 122, targetX: 800);
            rows.X[Self] = 400;
            rows.State[Self] = 0;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(6, 3), sites);

            Assert.That(Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness), Is.True);

            AssertSites(sites, rng.DrawCount, 0x28u, 0x34u);
            Assert.That(input.KeyJump, Is.EqualTo(1),
                "direct branch raises native attack");
        }

        [Test]
        public void WeaponRun35Zero_SelectsDirectAttackAfterConsumption()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 122, targetX: 800);
            rows.X[Self] = 400;
            rows.State[Self] = 0;
            rows.Pp[Self] = 300;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[5];
            var rng = CreateStream(CreateOwnerWithTable(6, 3, 6), sites);

            Assert.That(Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness), Is.True);

            AssertSites(sites, rng.DrawCount, 0x28u, 0x34u, 0x35u);
            Assert.That(input.KeyJump, Is.EqualTo(1));
            Assert.That(input.ComboDda, Is.Zero);
            Assert.That(input.ComboDdj, Is.Zero);
        }

        [Test]
        public void WeaponRunState17_UsesHitStopNotPhysicalY()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 122, targetX: 500);
            rows.State[Self] = 17;
            rows.Y[Self] = 0;
            rows.HitStop[Self] = 1;
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            var sites = new uint[3];
            var rng = CreateStream(CreateOwnerWithTable(6), sites);

            bool continues = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);

            Assert.That(continues, Is.False);
            AssertSites(sites, rng.DrawCount, 0x28u);
            Assert.That(input.KeyAttack, Is.EqualTo(1),
                "legacy KeyAttack maps to native defend");
        }

        [Test]
        public void FullCandidate_ValidHeldPathUses28AndNativeEarlyReturn()
        {
            const ulong epoch = 97UL;
            var snapshot = new AiDecisionSnapshot(21);
            snapshot.Reset(epoch);
            SetRow(snapshot.Rows, Self, oid: 34, state: 0, x: 0, z: 0, team: 1);
            SetRow(snapshot.Rows, Target, oid: 2, state: 0, x: 200, z: 0, team: 2);
            SetRow(snapshot.Rows, Held, oid: 900, state: 0, x: 0, z: 0, team: 0);
            snapshot.Rows.DataObjectType[Held] = 1;
            snapshot.Rows.LinkState[Self] = 1;
            snapshot.Rows.TargetSlot[Self] = Held;
            snapshot.SelfSlot = Self;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 2020;
            snapshot.OccupancyEpoch = epoch;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            snapshot.World.Difficulty = 2;
            snapshot.World.BattleMode = 0;
            snapshot.World.InputPhase = 1;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = -100;
            snapshot.World.StageZMax = 100;
            var owner = CreateOwnerWithTable(0, 0, 0, 0);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.HeldDecision));
            AssertSites(
                snapshot.RngTraceCallSites,
                witness.RngDrawCount,
                0x14u,
                0x3Cu,
                0x1Cu,
                0x28u);
        }

        [Test]
        public void WarmNativeHeldEvaluationCommit_AllocatesZeroManagedBytes()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, heldOid: 122, targetX: 800);
            rows.X[Self] = 400;
            rows.State[Self] = 0;
            var owner = new NTSD28NativeRandom(0x1234ABCDu);
            AiDecisionInputState input = default;
            AiDecisionWitness witness = default;
            for (int index = 0; index < 16; index++)
                EvaluateAndCommit(owner, rows, ref input, ref witness);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool committed = true;
            for (int index = 0; index < 4096; index++)
                committed &= EvaluateAndCommit(owner, rows, ref input, ref witness);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(committed, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static bool EvaluateAndCommit(
            NTSD28NativeRandom owner,
            AiSensingSnapshot rows,
            ref AiDecisionInputState input,
            ref AiDecisionWitness witness)
        {
            var rng = new AiDecisionRandomStream(owner.CaptureSynchronizedCursor());
            _ = Evaluate(
                rows, sameZLane: true, closeObstruction: false,
                ref input, ref rng, ref witness);
            return owner.TryCommitSynchronizedCursor(rng.CaptureSynchronizedCursor());
        }

        private static bool Evaluate(
            AiSensingSnapshot rows,
            bool sameZLane,
            bool closeObstruction,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng,
            ref AiDecisionWitness witness)
        {
            AiDecisionWorldState world = CreateWorld();
            return AiDecisionKernel.ProcessNativeHeld(
                rows,
                Self,
                Target,
                rand3: 6,
                rand5: 10,
                rand15: 30,
                sameZLane,
                closeObstruction,
                in world,
                ref input,
                ref rng,
                ref witness);
        }

        private static AiDecisionWorldState CreateWorld()
        {
            return new AiDecisionWorldState
            {
                StageTargetX = 800,
                StageZMin = -100,
                StageZMax = 100,
            };
        }

        private static AiSensingSnapshot CreateRows(
            int selfOid,
            int heldOid,
            int targetX)
        {
            var rows = new AiSensingSnapshot(21);
            rows.Reset(89UL);
            SetRow(rows, Self, selfOid, state: 0, x: 0, z: 0, team: 1);
            SetRow(rows, Target, oid: 2, state: 0, x: targetX, z: 0, team: 2);
            SetRow(rows, Held, heldOid, state: 0, x: 0, z: 0, team: 0);
            rows.DataObjectType[Held] = 1;
            rows.LinkState[Self] = 1;
            rows.TargetSlot[Self] = Held;
            return rows;
        }

        private static void SetRow(
            AiSensingSnapshot rows,
            int slot,
            int oid,
            int state,
            int x,
            int z,
            int team)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = 1;
            rows.Identity[slot] = 2000 + slot;
            rows.ObjectId[slot] = oid;
            rows.DataObjectType[slot] = 0;
            rows.State[slot] = state;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = z;
            rows.Vx[slot] = 0.0;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = 300;
            rows.Team[slot] = team;
            rows.Facing[slot] = 0;
            rows.TargetSlot[slot] = -1;
            rows.CachedTargetSlot[slot] = -1;
            rows.CoordinateTargetX[slot] = -1000;
        }

        private static AiDecisionRandomStream CreateStream(
            NTSD28NativeRandom owner,
            uint[] sites)
        {
            return new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor(),
                captureTrace: true,
                callSites: sites,
                moduli: new int[sites.Length],
                rawValues: new int[sites.Length],
                values: new int[sites.Length]);
        }

        private static NTSD28NativeRandom CreateOwnerWithTable(
            params byte[] leadingValues)
        {
            var state = new NTSD28SynchronizedRandomState();
            for (int index = 0; index < leadingValues.Length; index++)
                state.Table[index + 1] = leadingValues[index];
            var owner = new NTSD28NativeRandom(1u);
            owner.RestoreSynchronized(state);
            return owner;
        }

        private static void AssertSites(
            uint[] actual,
            int actualCount,
            params uint[] expected)
        {
            Assert.That(actualCount, Is.EqualTo(expected.Length));
            for (int index = 0; index < expected.Length; index++)
                Assert.That(actual[index], Is.EqualTo(expected[index]), $"site[{index}]");
        }
    }
}
