using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiOrdinaryNonHeldRandomEditorTests
    {
        [TestCase(0, 200, 0x1Cu)]
        [TestCase(0, -200, 0x1Du)]
        [TestCase(1, 200, 0x1Au)]
        [TestCase(1, -200, 0x1Bu)]
        public void MovementSite_DependsOnStaticBattleModeLowHpSpacing(
            int battleMode,
            int targetX,
            uint expectedSite)
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX);
            rows.Hp[20] = 100;
            rows.Hp3[20] = 500;
            rows.Hp[0] = 400;
            AiDecisionInputState input = default;
            var owner = CreateOwnerWithTable(1);
            var sites = new uint[4];
            var rng = CreateStream(owner, sites);

            AiDecisionKernel.ProcessNativeOrdinaryMovement(
                rows,
                self: 20,
                target: 0,
                battleMode,
                rand20: 40,
                behaviorRange: true,
                threatOnRight: false,
                threatOnLeft: false,
                threatPositiveZ: false,
                threatNegativeZ: false,
                closeObstruction: false,
                ref input,
                ref rng);

            Assert.That(rng.DrawCount, Is.EqualTo(1));
            Assert.That(sites[0], Is.EqualTo(expectedSite));
        }

        [Test]
        public void CommonTail_OneFRejectsWithoutConsuming20()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX: 500);
            rows.Z[0] = 100;
            AiDecisionInputState input = default;
            var sites = new uint[8];
            var rng = CreateStream(CreateOwnerWithTable(1, 5), sites);

            AiDecisionKernel.ProcessNativeOrdinaryTail(
                rows, 20, 0, rand3: 6, rand5: 10, battleMode: 0,
                stageTargetX: 800, behaviorRange: true,
                threatOnRight: true, threatOnLeft: false,
                ref input, ref rng);

            AssertSites(sites, rng.DrawCount, 0x1Eu, 0x1Fu);
        }

        [Test]
        public void CommonTail_OneFPassConsumes20BeforeLaterSites()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX: 50);
            AiDecisionInputState input = default;
            var sites = new uint[16];
            var rng = CreateStream(
                CreateOwnerWithTable(1, 0, 18, 5, 4, 1),
                sites);

            AiDecisionKernel.ProcessNativeOrdinaryTail(
                rows, 20, 0, rand3: 6, rand5: 10, battleMode: 0,
                stageTargetX: 800, behaviorRange: true,
                threatOnRight: false, threatOnLeft: false,
                ref input, ref rng);

            AssertSites(
                sites,
                rng.DrawCount,
                0x1Eu,
                0x1Fu,
                0x20u,
                0x21u,
                0x37u,
                0x38u);
            Assert.That(input.KeyDefend, Is.EqualTo(1),
                "legacy KeyDefend maps to native jump");
            Assert.That(input.KeyJump, Is.EqualTo(1),
                "legacy KeyJump maps to native attack");
        }

        [Test]
        public void LowHpContinuation_ConsumesSingle26ThenConditional27()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX: 50);
            rows.Hp[20] = 100;
            rows.Hp3[20] = 500;
            rows.Hp[0] = 400;
            rows.State[20] = 2;
            AiDecisionInputState input = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(7, 15), sites);

            AiDecisionKernel.ProcessNativeLowHpContinuation(
                rows, 20, 0, battleMode: 1, rand3: 6,
                stageTargetX: 800, behaviorRange: true,
                ref input, ref rng);

            AssertSites(sites, rng.DrawCount, 0x26u, 0x27u);
            Assert.That(input.KeyDefend, Is.EqualTo(1),
                "0x27 zero result raises native jump");
        }

        [Test]
        public void LowHpContinuation_SelfState7StopsBefore27()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX: 50);
            rows.Hp[20] = 100;
            rows.Hp3[20] = 500;
            rows.Hp[0] = 400;
            rows.State[20] = 7;
            AiDecisionInputState input = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(7), sites);

            AiDecisionKernel.ProcessNativeLowHpContinuation(
                rows, 20, 0, battleMode: 1, rand3: 6,
                stageTargetX: 800, behaviorRange: true,
                ref input, ref rng);

            AssertSites(sites, rng.DrawCount, 0x26u);
        }

        [Test]
        public void ProfileFamily34_Consumes38Then39Then3A()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX: 120);
            rows.Frame[20] = 110;
            rows.Facing[20] = 0;
            AiDecisionInputState input = default;
            var sites = new uint[8];
            var rng = CreateStream(CreateOwnerWithTable(6, 1, 1), sites);

            AiDecisionKernel.ProcessNativeProfiledCombat(
                rows, 20, 0, rand3: 6, rand5: 10,
                threatOnRight: false, threatOnLeft: false,
                ref input, ref rng);

            AssertSites(sites, rng.DrawCount, 0x38u, 0x39u, 0x3Au);
            Assert.That(input.KeyDefend, Is.EqualTo(1),
                "oid34 0x3A zero raises native jump");
            Assert.That(input.KeyJump, Is.Zero);
        }

        [Test]
        public void ProfileOid1_Consumes3BAndAppliesFinalAttackChase()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 1, targetX: 120);
            rows.Frame[20] = 110;
            rows.Facing[20] = 0;
            AiDecisionInputState input = default;
            var sites = new uint[8];
            var rng = CreateStream(CreateOwnerWithTable(6, 18), sites);

            AiDecisionKernel.ProcessNativeProfiledCombat(
                rows, 20, 0, rand3: 6, rand5: 10,
                threatOnRight: false, threatOnLeft: false,
                ref input, ref rng);

            AssertSites(sites, rng.DrawCount, 0x38u, 0x3Bu);
            Assert.That(input.KeyAttack, Is.EqualTo(1),
                "0x3B zero raises native defend");
            Assert.That(input.KeyJump, Is.EqualTo(1),
                "oid1 final chase raises native attack");
        }

        [Test]
        public void ProfileOid5_IsNotNativeFamilyAndConsumesNo39()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 5, targetX: 120);
            AiDecisionInputState input = default;
            var sites = new uint[4];
            var rng = CreateStream(CreateOwnerWithTable(6), sites);

            AiDecisionKernel.ProcessNativeProfiledCombat(
                rows, 20, 0, rand3: 6, rand5: 10,
                threatOnRight: false, threatOnLeft: false,
                ref input, ref rng);

            AssertSites(sites, rng.DrawCount, 0x38u);
        }

        [Test]
        public void SynchronizedContext_UsesBattleModeNotCadencePhase()
        {
            AiDecisionSnapshot modeOne = CreateFullSnapshot(
                battleMode: 1,
                cadencePhase: 0,
                difficulty: 2);
            var owner = CreateOwnerWithTable(1);
            modeOne.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());
            AiDecisionWitness witness = default;

            Assert.That(AiDecisionKernel.TryEvaluate(modeOne, ref witness), Is.True);
            Assert.That(modeOne.RngTraceCallSites[0], Is.EqualTo(0x14u));
            Assert.That(modeOne.RngTraceModuli[0], Is.EqualTo(8),
                "mode-one local AI must zero effective level even on cadence phase zero");

            AiDecisionSnapshot ordinary = CreateFullSnapshot(
                battleMode: 0,
                cadencePhase: 1,
                difficulty: 2);
            ordinary.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());
            Assert.That(AiDecisionKernel.TryEvaluate(ordinary, ref witness), Is.True);
            Assert.That(ordinary.RngTraceModuli[0], Is.EqualTo(18),
                "cadence phase one must not masquerade as battle mode one");
        }

        [Test]
        public void ProductionWorldSnapshot_CapturesBattleModeSeparatelyFromCadence()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 4;
            world.Runtime.Flow.InputPhase = 1;
            var method = typeof(SimulationWorld).GetMethod(
                "CaptureAiDecisionWorldState",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            var captured = (AiDecisionWorldState)method.Invoke(world, null);
            Assert.That(captured.BattleMode, Is.EqualTo(4));
            Assert.That(captured.InputPhase, Is.EqualTo(1));

            AiDecisionWorldState differentMode = captured;
            differentMode.BattleMode = 0;
            Assert.That(
                SimulationAiDecisionModule.WorldEquals(captured, differentMode),
                Is.False);
        }

        [Test]
        public void FullCandidate_NonHeldPathReachesCompleteWithNativeOrder()
        {
            const ulong epoch = 79UL;
            var snapshot = new AiDecisionSnapshot(21);
            snapshot.Reset(epoch);
            SetRow(snapshot.Rows, 20, oid: 34, state: 0, x: 0, team: 1);
            snapshot.Rows.Facing[20] = 1;
            SetRow(snapshot.Rows, 0, oid: 2, state: 0, x: 50, team: 2);
            snapshot.SelfSlot = 20;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1020;
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
            var owner = CreateOwnerWithTable(1, 1, 1, 1, 1, 1, 1, 1);
            snapshot.SetSynchronizedRngCursor(owner.CaptureSynchronizedCursor());

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.Complete));
            AssertSites(
                snapshot.RngTraceCallSites,
                witness.RngDrawCount,
                0x14u,
                0x3Cu,
                0x1Cu,
                0x1Eu,
                0x1Fu,
                0x21u,
                0x37u,
                0x38u);
        }

        [Test]
        public void WarmOrdinaryNonHeldHelpers_AllocateZeroManagedBytes()
        {
            AiSensingSnapshot rows = CreateRows(selfOid: 34, targetX: 50);
            var owner = new NTSD28NativeRandom(0x2468ACE0u);
            AiDecisionInputState input = default;
            for (int index = 0; index < 16; index++)
                EvaluateAndCommit(owner, rows, ref input);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool committed = true;
            for (int index = 0; index < 4096; index++)
                committed &= EvaluateAndCommit(owner, rows, ref input);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(committed, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static bool EvaluateAndCommit(
            NTSD28NativeRandom owner,
            AiSensingSnapshot rows,
            ref AiDecisionInputState input)
        {
            var rng = new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor());
            AiDecisionKernel.ProcessNativeOrdinaryMovement(
                rows, 20, 0, battleMode: 0, rand20: 40,
                behaviorRange: true, threatOnRight: false,
                threatOnLeft: false, threatPositiveZ: false,
                threatNegativeZ: false, closeObstruction: false,
                ref input, ref rng);
            AiDecisionKernel.ProcessNativeOrdinaryTail(
                rows, 20, 0, rand3: 6, rand5: 10,
                battleMode: 0, stageTargetX: 800,
                behaviorRange: true, threatOnRight: false,
                threatOnLeft: false, ref input, ref rng);
            return owner.TryCommitSynchronizedCursor(
                rng.CaptureSynchronizedCursor());
        }

        private static AiDecisionSnapshot CreateFullSnapshot(
            int battleMode,
            int cadencePhase,
            int difficulty)
        {
            const ulong epoch = 73UL;
            var snapshot = new AiDecisionSnapshot(21);
            snapshot.Reset(epoch);
            SetRow(snapshot.Rows, 0, oid: 1, state: 7, x: 0, team: 1);
            SetRow(snapshot.Rows, 1, oid: 2, state: 3000, x: 50, team: 5);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.OccupancyEpoch = epoch;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            snapshot.World.Difficulty = difficulty;
            snapshot.World.BattleMode = battleMode;
            snapshot.World.InputPhase = cadencePhase;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = -100;
            snapshot.World.StageZMax = 100;
            return snapshot;
        }

        private static AiSensingSnapshot CreateRows(int selfOid, int targetX)
        {
            var rows = new AiSensingSnapshot(21);
            rows.Reset(41UL);
            SetRow(rows, 20, selfOid, state: 0, x: 0, team: 1);
            SetRow(rows, 0, oid: 2, state: 0, x: targetX, team: 2);
            return rows;
        }

        private static void SetRow(
            AiSensingSnapshot rows,
            int slot,
            int oid,
            int state,
            int x,
            int team)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = 1;
            rows.Identity[slot] = 1000 + slot;
            rows.ObjectId[slot] = oid;
            rows.DataObjectType[slot] = 0;
            rows.State[slot] = state;
            rows.X[slot] = x;
            rows.Y[slot] = 0;
            rows.Z[slot] = 0;
            rows.Vx[slot] = 0.0;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = 300;
            rows.Team[slot] = team;
            rows.Facing[slot] = 0;
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
