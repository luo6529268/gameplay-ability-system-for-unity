#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiDecisionKernelEditorTests
    {
        [Test]
        public void CharacterInputWriter_SeparatesFullAndProgressTransactions()
        {
            var world = new SimulationWorld();
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.KeyRight = 9;
            runtime.PrevLeft = 8;
            AiDecisionInputState input = new AiDecisionInputState
            {
                CdAttack = 1,
                CdJump = 2,
                CdDefend = 3,
                CdDefendLock = 4,
                CdRight = 5,
                CdLeft = 6,
                CdUp = 7,
                CdDown = 8,
                ComboDra = 1,
                ComboDla = 2,
                ComboDua = 3,
                ComboDda = 4,
                ComboDrj = 5,
                ComboDlj = 6,
                ComboDuj = 7,
                ComboDdj = 8,
                ComboDja = 9,
                PrevUp = 1,
                PrevDown = 2,
                PrevLeft = 3,
                PrevRight = 4,
                PrevJump = 5,
                PrevDefend = 6,
                PrevAttack = 7,
                KeyUp = 7,
                KeyDown = 6,
                KeyLeft = 5,
                KeyRight = 4,
                KeyAttack = 3,
                KeyJump = 2,
                KeyDefend = 1,
            };

            world.CharacterInputWriter.CommitProgressState(runtime, input);

            Assert.That(runtime.KeyRight, Is.EqualTo(9));
            Assert.That(runtime.PrevLeft, Is.EqualTo(8));
            Assert.That(
                new[]
                {
                    runtime.CdAttack, runtime.CdJump, runtime.CdDefend,
                    runtime.CdDefendLock, runtime.CdRight, runtime.CdLeft,
                    runtime.CdUp, runtime.CdDown,
                },
                Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.That(runtime.ComboDja, Is.EqualTo(9));

            world.CharacterInputWriter.CommitFullState(runtime, input);

            Assert.That(
                new[]
                {
                    runtime.PrevUp, runtime.PrevDown, runtime.PrevLeft,
                    runtime.PrevRight, runtime.PrevJump, runtime.PrevDefend,
                    runtime.PrevAttack, runtime.KeyUp, runtime.KeyDown,
                    runtime.KeyLeft, runtime.KeyRight, runtime.KeyAttack,
                    runtime.KeyJump, runtime.KeyDefend,
                },
                Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 7, 6, 5, 4, 3, 2, 1 }));
        }

        [Test]
        public void CharacterInputWriter_OwnsInputLifecycleTransactions()
        {
            var world = new SimulationWorld();
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.KeyRight = 1;
            runtime.KeyAttack = 1;

            world.CharacterInputWriter.RollAndClearCurrentKeys(runtime);

            Assert.That(runtime.PrevRight, Is.EqualTo(1));
            Assert.That(runtime.PrevAttack, Is.EqualTo(1));
            Assert.That(runtime.KeyRight, Is.EqualTo(0));
            Assert.That(runtime.KeyAttack, Is.EqualTo(0));

            runtime.PrevRight = 0;
            runtime.PrevDefend = 0;
            runtime.KeyRight = 1;
            runtime.KeyDefend = 1;
            world.CharacterInputWriter.ApplyInputEdges(runtime);

            Assert.That(runtime.CdRight, Is.EqualTo(5));
            Assert.That(runtime.CdJump, Is.EqualTo(5));
            Assert.That(runtime.InputHistory, Is.EqualTo(new[] { 0, 0, 0, 0, 6, 0 }));

            world.CharacterInputWriter.SetInputHistoryGate(runtime, true);
            world.CharacterInputWriter.ClearInputHistoryTail(runtime);
            Assert.That(runtime.InputHistory, Is.EqualTo(new[] { 1, 0, 0, 0, 0, 0 }));

            world.CharacterInputWriter.SetDefendLock(runtime, 4);
            world.CharacterInputWriter.ResetInputState(runtime);
            Assert.That(runtime.CdDefendLock, Is.EqualTo(0));
            Assert.That(runtime.InputHistory, Is.EqualTo(new[] { 0, 0, 0, 0, 0, 0 }));
            Assert.That(runtime.KeyRight, Is.EqualTo(0));
            Assert.That(runtime.PrevRight, Is.EqualTo(0));
        }

        [Test]
        public void AiInputWriter_CommitsDecisionInputFlowAndRngAsOneTransaction()
        {
            var world = new SimulationWorld();
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            AiDecisionWitness witness = new AiDecisionWitness
            {
                Input = new AiDecisionInputState
                {
                    History0 = 1,
                    History1 = 2,
                    History2 = 3,
                    History3 = 4,
                    History4 = 5,
                    History5 = 6,
                    CdAttack = 1,
                    CdJump = 2,
                    CdDefend = 3,
                    CdDefendLock = 4,
                    CdRight = 5,
                    CdLeft = 6,
                    CdUp = 7,
                    CdDown = 8,
                    ComboDra = 1,
                    ComboDla = 2,
                    ComboDua = 3,
                    ComboDda = 4,
                    ComboDrj = 5,
                    ComboDlj = 6,
                    ComboDuj = 7,
                    ComboDdj = 8,
                    ComboDja = 9,
                    PrevUp = 1,
                    PrevDown = 2,
                    PrevLeft = 3,
                    PrevRight = 4,
                    PrevJump = 5,
                    PrevDefend = 6,
                    PrevAttack = 7,
                    KeyUp = 7,
                    KeyDown = 6,
                    KeyLeft = 5,
                    KeyRight = 4,
                    KeyAttack = 3,
                    KeyJump = 2,
                    KeyDefend = 1,
                    Unk360 = 17,
                    Unk3FC = 271,
                    Unk400 = 314,
                },
                World = new AiDecisionWorldState
                {
                    FlowAiDifficulty = 2,
                    FlowRand3 = 6,
                    FlowRand5 = 10,
                    FlowRand15 = 30,
                    FlowRand20 = 40,
                    FlowMoveMode = 1,
                    FlowStageTargetX = 777,
                },
                RngState = 0xA11CEu,
                RngCalls = 29,
            };

            world.AiInputWriter.CommitIndexedCanonicalDecision(runtime, witness);

            Assert.That(runtime.InputHistory, Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
            Assert.That(
                new[]
                {
                    runtime.CdAttack, runtime.CdJump, runtime.CdDefend,
                    runtime.CdDefendLock, runtime.CdRight, runtime.CdLeft,
                    runtime.CdUp, runtime.CdDown,
                },
                Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));
            Assert.That(
                new[]
                {
                    runtime.ComboDra, runtime.ComboDla, runtime.ComboDua,
                    runtime.ComboDda, runtime.ComboDrj, runtime.ComboDlj,
                    runtime.ComboDuj, runtime.ComboDdj, runtime.ComboDja,
                },
                Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            Assert.That(
                new[]
                {
                    runtime.PrevUp, runtime.PrevDown, runtime.PrevLeft,
                    runtime.PrevRight, runtime.PrevJump, runtime.PrevDefend,
                    runtime.PrevAttack, runtime.KeyUp, runtime.KeyDown,
                    runtime.KeyLeft, runtime.KeyRight, runtime.KeyAttack,
                    runtime.KeyJump, runtime.KeyDefend,
                },
                Is.EqualTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 7, 6, 5, 4, 3, 2, 1 }));
            Assert.That(
                new[] { runtime.Unk360, runtime.Unk3FC, runtime.Unk400 },
                Is.EqualTo(new[] { 17, 271, 314 }));
            Assert.That(world.Runtime.Flow.AiDifficulty, Is.EqualTo(2));
            Assert.That(world.Runtime.Flow.AiMoveMode, Is.EqualTo(1));
            Assert.That(world.Runtime.Flow.AiStageTargetX, Is.EqualTo(777));
            Assert.That(world.Rng.State, Is.EqualTo(0xA11CEu));
            Assert.That(world.Rng.CallCount, Is.EqualTo(29));
        }

        [Test]
        public void CoordinateBranch_UsesPriorSharedFlowAndPreservesIt()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(2, 41);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 7, 0, 0, 0, 2);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.Input.Unk3FC = 400;
            snapshot.Input.Unk400 = 0;
            snapshot.World.FlowAiDifficulty = 2;
            snapshot.World.FlowRand3 = 6;
            snapshot.World.FlowRand5 = 10;
            snapshot.World.FlowRand15 = 30;
            snapshot.World.FlowRand20 = 40;
            snapshot.World.FlowMoveMode = 1;
            snapshot.World.FlowStageTargetX = 777;
            snapshot.RngState = 0x1234u;

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(witness.Availability, Is.EqualTo(AiDecisionAvailability.Available));
            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.Coordinate));
            Assert.That(witness.Input.KeyRight, Is.EqualTo(1));
            Assert.That(witness.World.FlowAiDifficulty, Is.EqualTo(2));
            Assert.That(witness.World.FlowRand3, Is.EqualTo(6));
            Assert.That(witness.World.FlowMoveMode, Is.EqualTo(1));
            Assert.That(witness.World.FlowStageTargetX, Is.EqualTo(777));
            Assert.That(witness.RngCalls, Is.EqualTo(1));
        }

        [Test]
        public void CanonicalInputOverload_ConsumesOwnedRowWithoutSnapshotCopy()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(2, 42);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 7, 0, 0, 0, 2);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.Input.Unk3FC = -1000;
            snapshot.World.FlowAiDifficulty = 2;
            snapshot.World.FlowRand3 = 6;
            snapshot.World.FlowRand5 = 10;
            snapshot.World.FlowRand15 = 30;
            snapshot.World.FlowRand20 = 40;
            snapshot.World.FlowMoveMode = 1;
            snapshot.World.FlowStageTargetX = 777;
            snapshot.RngState = 0x1234u;
            AiDecisionInputState canonicalInput = snapshot.Input;
            canonicalInput.Unk3FC = 400;
            canonicalInput.Unk400 = 0;

            AiDecisionWitness witness = default;
            Assert.That(
                AiDecisionKernel.TryEvaluateCanonicalInput(
                    snapshot,
                    in canonicalInput,
                    AiDecisionEvaluationPolicy.FullScan,
                    true,
                    null,
                    ref witness),
                Is.True);

            Assert.That(witness.Exit, Is.EqualTo(AiDecisionExit.Coordinate));
            Assert.That(witness.Input.Unk3FC, Is.EqualTo(400));
            Assert.That(snapshot.Input.Unk3FC, Is.EqualTo(-1000));
        }

        [Test]
        public void OwnedInputMode_DefaultsToSnapshotCopyAndRequiresResetBoundary()
        {
            var world = new SimulationWorld();
            Assert.That(
                world.AiDecisionOwnedInputModeForDiagnostics,
                Is.EqualTo(AiDecisionOwnedInputMode.SnapshotCopy));

            world.ConfigureAiDecisionOwnedInputModeForDiagnostics(
                AiDecisionOwnedInputMode.CanonicalStoreDirect);
            Assert.That(
                world.AiDecisionOwnedInputModeForDiagnostics,
                Is.EqualTo(AiDecisionOwnedInputMode.CanonicalStoreDirect));
        }

        [Test]
        public void Decision_SeesEarlierSlotPostComboStateAndSameTeamSummary()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(4, 52);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 1, 10, 0, 0, 14);
            snapshot.Rows.Frame[0] = 6;
            snapshot.Rows.Hp[0] = 80;
            snapshot.Rows.HpMax[0] = 500;
            SetCharacter(snapshot.Rows, 1, 1001, 2, 1, 0, 0, 0, 9);
            SetCharacter(snapshot.Rows, 2, 1002, 3, 2, 50, 0, 0, 0);
            snapshot.SelfSlot = 1;
            snapshot.SelfGeneration = 2;
            snapshot.SelfStableId = 1001;
            snapshot.World.InputPhase = 2;
            snapshot.RngState = 0x51A0u;

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            Assert.That(witness.InitialSelectedSlot, Is.EqualTo(2),
                "the later AI must reject the earlier slot's post-combo state 14 row");
            Assert.That(witness.Input.Unk360, Is.EqualTo(2));
            Assert.That(witness.RowVisits, Is.GreaterThan(0));
        }

        [Test]
        public void InvalidSelfGenerationOrEpoch_IsUnavailableWithoutRngUse()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(2, 63);
            SetCharacter(snapshot.Rows, 0, 1000, 4, 1, 0, 0, 0, 0);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 3;
            snapshot.SelfStableId = 1000;
            snapshot.RngState = 0x99u;
            snapshot.RngCalls = 17;

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.False);
            Assert.That(witness.Availability, Is.EqualTo(AiDecisionAvailability.GenerationMismatch));
            Assert.That(witness.RngState, Is.EqualTo(0x99u));
            Assert.That(witness.RngCalls, Is.EqualTo(17));

            snapshot.SelfGeneration = 4;
            snapshot.OccupancyEpoch = 64;
            witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.False);
            Assert.That(witness.Availability, Is.EqualTo(AiDecisionAvailability.EpochMismatch));
            Assert.That(witness.RngCalls, Is.EqualTo(17));
        }

        [Test]
        public void IndexedPolicy_IndexesNotReadyFailsClosedWhileDefaultRemainsFullScan()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(2, 64);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 1, 0, 0, 0, 9);
            SetCharacter(snapshot.Rows, 1, 1001, 2, 2, 30, 0, 0, 0);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.World.InputPhase = 2;

            AiDecisionWitness full = default;
            AiDecisionWitness indexed = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref full), Is.True);
            Assert.That(AiDecisionKernel.TryEvaluate(
                snapshot,
                AiDecisionEvaluationPolicy.Indexed,
                ref indexed), Is.False);
            Assert.That(indexed.Availability,
                Is.EqualTo(AiDecisionAvailability.IndexesNotReady));
        }

        [Test]
        public void IndexedTeamSummary_ExcludingSelfHandlesUniqueAndRepeatedMinimum()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(3, 65);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 1, 0, 0, 0, 9);
            SetCharacter(snapshot.Rows, 1, 1001, 2, 1, 30, 0, 0, 0);
            SetCharacter(snapshot.Rows, 2, 1002, 3, 1, 60, 0, 0, 0);
            snapshot.Rows.Hp[0] = 100;
            snapshot.Rows.Hp[1] = 200;
            snapshot.Rows.Hp[2] = 300;
            snapshot.Rows.TeamSummariesReady = true;
            snapshot.Rows.TeamSummaryCount = 1;
            snapshot.Rows.TeamSummaries[0] = new AiSensingTeamSummary
            {
                Team = 1,
                Count = 3,
                MinHp = 100,
                MinCount = 1,
                SecondMinHp = 200,
            };

            Assert.That(AiSensingKernel.TryGetSameTeamSummaryExcludingSelf(
                snapshot.Rows, 0, 1, out int count, out int minHp), Is.True);
            Assert.That(count, Is.EqualTo(2));
            Assert.That(minHp, Is.EqualTo(200));

            snapshot.Rows.Hp[1] = 100;
            snapshot.Rows.TeamSummaries[0] = new AiSensingTeamSummary
            {
                Team = 1,
                Count = 3,
                MinHp = 100,
                MinCount = 2,
                SecondMinHp = 300,
            };
            Assert.That(AiSensingKernel.TryGetSameTeamSummaryExcludingSelf(
                snapshot.Rows, 0, 1, out count, out minHp), Is.True);
            Assert.That(count, Is.EqualTo(2));
            Assert.That(minHp, Is.EqualTo(100));
        }

        [Test]
        public void InputEdges_PreserveAuthorityCrossMappingAndHistoryOrder()
        {
            AiDecisionInputState input = default;
            input.KeyUp = 1;
            input.KeyDown = 1;
            input.KeyLeft = 1;
            input.KeyRight = 1;
            input.KeyAttack = 1;
            input.KeyDefend = 1;
            input.KeyJump = 1;

            AiDecisionKernel.ApplyInputEdges(ref input);

            Assert.That(input.CdRight, Is.EqualTo(5));
            Assert.That(input.CdLeft, Is.EqualTo(5));
            Assert.That(input.CdUp, Is.EqualTo(5));
            Assert.That(input.CdDown, Is.EqualTo(5));
            Assert.That(input.CdDefend, Is.EqualTo(5), "attack edge maps to CdDefend");
            Assert.That(input.CdJump, Is.EqualTo(5), "defend edge maps to CdJump");
            Assert.That(input.CdAttack, Is.EqualTo(5), "jump edge maps to CdAttack");
            Assert.That(input.History1, Is.EqualTo(8));
            Assert.That(input.History2, Is.EqualTo(2));
            Assert.That(input.History3, Is.EqualTo(9));
            Assert.That(input.History4, Is.EqualTo(0));
            Assert.That(input.History5, Is.EqualTo(5));
        }

        [Test]
        public void RngClone_RecordsExactCountStateAndModulusOrder()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(1, 77);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 7, 0, 0, 0, 0);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.Input.Unk3FC = 400;
            snapshot.Input.Unk400 = 0;
            snapshot.World.FlowRand3 = 6;
            snapshot.RngState = 0x1234u;
            snapshot.RngCalls = 9;

            AiDecisionWitness witness = default;
            Assert.That(AiDecisionKernel.TryEvaluate(snapshot, ref witness), Is.True);

            uint expectedState = unchecked(0x1234u * 0x343FDu + 0x269EC3u);
            int expectedRaw = (int)((expectedState >> 16) & 0x7FFFu);
            Assert.That(witness.RngState, Is.EqualTo(expectedState));
            Assert.That(witness.RngCalls, Is.EqualTo(10));
            Assert.That(witness.RngOrderHash, Is.EqualTo(HashDraw(
                1469598103934665603UL,
                9,
                expectedRaw,
                expectedRaw % 9)));
        }

        [Test]
        public void WarmedKernel_128EvaluationsAllocateZeroBytes()
        {
            AiDecisionSnapshot snapshot = CreateSnapshot(2, 88);
            SetCharacter(snapshot.Rows, 0, 1000, 1, 1, 0, 0, 0, 9);
            SetCharacter(snapshot.Rows, 1, 1001, 2, 2, 30, 0, 0, 0);
            snapshot.SelfSlot = 0;
            snapshot.SelfGeneration = 1;
            snapshot.SelfStableId = 1000;
            snapshot.World.InputPhase = 2;
            snapshot.RngState = 0xA11CEu;

            AiDecisionWitness witness = default;
            for (int index = 0; index < 32; index++)
                AiDecisionKernel.TryEvaluate(snapshot, ref witness);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 128; index++)
                AiDecisionKernel.TryEvaluate(snapshot, ref witness);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        private static AiDecisionSnapshot CreateSnapshot(int capacity, ulong epoch)
        {
            var snapshot = new AiDecisionSnapshot(capacity);
            snapshot.Reset(epoch);
            snapshot.World.Difficulty = 1;
            snapshot.World.StageTargetX = 800;
            snapshot.World.StageZMin = 180;
            snapshot.World.StageZMax = 350;
            snapshot.Input.Unk360 = -1;
            snapshot.Input.Unk3FC = -1000;
            snapshot.Input.Unk400 = -1000;
            for (int slot = 0; slot < capacity; slot++)
            {
                snapshot.Rows.CoordinateTargetX[slot] = -1000;
                snapshot.Rows.KillCount[slot] = -1;
            }
            return snapshot;
        }

        private static void SetCharacter(
            AiSensingSnapshot rows,
            int slot,
            int stableId,
            uint generation,
            int team,
            int x,
            int y,
            int z,
            int state)
        {
            rows.Included[slot] = true;
            rows.Generation[slot] = generation;
            rows.Identity[slot] = stableId;
            rows.DataObjectType[slot] = 0;
            rows.Team[slot] = team;
            rows.X[slot] = x;
            rows.Y[slot] = y;
            rows.Z[slot] = z;
            rows.State[slot] = state;
            rows.Hp[slot] = 500;
            rows.Hp3[slot] = 500;
            rows.HpMax[slot] = 500;
            rows.Pp[slot] = 500;
            rows.CoordinateTargetX[slot] = -1000;
            rows.KillCount[slot] = -1;
        }

        private static ulong HashDraw(ulong hash, int modulus, int raw, int value)
        {
            unchecked
            {
                hash ^= (uint)modulus;
                hash *= 1099511628211UL;
                hash ^= (uint)raw;
                hash *= 1099511628211UL;
                hash ^= (uint)value;
                hash *= 1099511628211UL;
                return hash;
            }
        }
    }
}
#endif
