#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiSensingKernelEditorTests
    {
        [Test]
        public void Nearest_PreservesGroundTieAirOverrideAndGroundFacts()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(8, 91);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 10, 0, 0);
            SetCharacter(snapshot, 2, 2, -10, 0, 0);
            SetCharacter(snapshot, 3, 2, 4, 10, 14);

            Assert.That(AiSensingKernel.TryFindNearest(snapshot, 0, 2, out AiSensingNearestResult result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(3), "An eligible air role overrides the ground selection.");
            Assert.That(result.BestDist, Is.EqualTo(10), "The authority keeps the ground best distance.");
            Assert.That(result.SameZLane, Is.True, "sameZLane is captured before the air override.");
            Assert.That(result.CapturedOccupancyEpoch, Is.EqualTo(91));
            Assert.That(result.SelectedGeneration, Is.EqualTo(4));
            Assert.That(result.SelectedIdentity, Is.EqualTo(1003));

            snapshot.State[0] = 9;
            Assert.That(AiSensingKernel.TryFindNearest(snapshot, 0, 2, out result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(1), "State 9 skips the air scan.");
        }

        [Test]
        public void Nearest_UsesStrictAirBoundaries()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(6, 7);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 30, 0, 0);
            SetCharacter(snapshot, 2, 2, 1, 40, 14);
            SetCharacter(snapshot, 3, 2, 250, 1, 14);

            Assert.That(AiSensingKernel.TryFindNearest(snapshot, 0, 2, out AiSensingNearestResult result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(1));

            snapshot.Z[2] = 39;
            Assert.That(AiSensingKernel.TryFindNearest(snapshot, 0, 2, out result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(2));
        }

        [Test]
        public void Special_IndexedSlot20PlusScanPreservesAscendingLateOverrideAndHandle()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(24, 1234);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            snapshot.Hp[0] = 100;
            snapshot.Hp3[0] = 500;
            snapshot.HpMax[0] = 500;
            snapshot.KillCount[0] = -1;
            SetCharacter(snapshot, 1, 1, 40, 30, 0);
            SetSpecial(snapshot, 20, 0x7A, 10);
            SetSpecial(snapshot, 21, 0x7A, 20);
            snapshot.SpecialSlots[0] = 20;
            snapshot.SpecialSlots[1] = 21;
            snapshot.SpecialSlotCount = 2;
            snapshot.SpecialIndexReady = true;
            snapshot.TeamSummariesReady = true;
            snapshot.TeamSummaryCount = 1;
            snapshot.TeamSummaries[0] = new AiSensingTeamSummary
            {
                Team = 1,
                Count = 2,
                MinHp = 100,
                MinCount = 1,
                SecondMinHp = 500,
            };

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot, 0, 4, 1, 70, false, false, out AiSensingSpecialResult result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(21), "The later ascending slot keeps the authority override.");
            Assert.That(result.CapturedOccupancyEpoch, Is.EqualTo(1234));
            Assert.That(result.SelectedGeneration, Is.EqualTo(22));
            Assert.That(result.SelectedIdentity, Is.EqualTo(1021));
            Assert.That(result.SlotVisits, Is.EqualTo(2));
            Assert.That(result.Flags & AiSensingKernel.SpecialForce7AGround, Is.Not.Zero);
        }

        [Test]
        public void Special_IndexedPreservesAscending100_199_C8_D3_D4_D5OrderAndSelection()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(26, 1235);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            snapshot.Hp[0] = 100;
            snapshot.Hp3[0] = 500;
            snapshot.HpMax[0] = 500;
            SetCharacter(snapshot, 1, 2, 100, 50, 0);
            SetSpecial(snapshot, 20, 100, 60);
            snapshot.DataObjectType[20] = 1;
            SetSpecial(snapshot, 21, 199, 55);
            SetSpecial(snapshot, 22, 0xC8, 70);
            snapshot.Frame[22] = 0;
            SetSpecial(snapshot, 23, 0xD3, 60);
            snapshot.State[23] = 0x12;
            snapshot.Z[23] = 30;
            SetSpecial(snapshot, 24, 0xD4, -130);
            snapshot.Frame[24] = 160;
            SetSpecial(snapshot, 25, 0xD5, 40);
            for (int index = 0; index < 6; index++)
                snapshot.SpecialSlots[index] = 20 + index;
            snapshot.SpecialSlotCount = 6;
            snapshot.SpecialIndexReady = true;
            snapshot.TeamSummariesReady = true;

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot,
                0,
                2,
                1,
                100,
                false,
                AiDecisionEvaluationPolicy.Indexed,
                out AiSensingSpecialResult indexed), Is.True);
            Assert.That(indexed.SelectedSlot, Is.EqualTo(25));
            Assert.That(indexed.SlotVisits, Is.EqualTo(6));
            Assert.That(indexed.Flags & AiSensingKernel.SpecialDown, Is.Not.Zero);
            Assert.That(indexed.Flags & AiSensingKernel.SpecialLeft, Is.Not.Zero);

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot,
                0,
                2,
                1,
                100,
                false,
                AiDecisionEvaluationPolicy.FullScan,
                out AiSensingSpecialResult full), Is.True);
            Assert.That(full.SelectedSlot, Is.EqualTo(indexed.SelectedSlot));
            Assert.That(full.Flags, Is.EqualTo(indexed.Flags));
        }

        [Test]
        public void Special_IndexedReadyButMissingSelfTeamSummaryFailsClosed()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(24, 1236);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 1, 40, 30, 0);
            SetSpecial(snapshot, 20, 0x7A, 10);
            snapshot.SpecialSlots[0] = 20;
            snapshot.SpecialSlotCount = 1;
            snapshot.SpecialIndexReady = true;
            snapshot.TeamSummariesReady = true;

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot,
                0,
                4,
                1,
                70,
                false,
                AiDecisionEvaluationPolicy.Indexed,
                out _), Is.False);
        }

        [Test]
        public void QuerySteadyState_DoesNotReplaceSnapshotArrays()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(4, 55);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 10, 0, 0);
            bool[] included = snapshot.Included;
            int[] x = snapshot.X;
            int[] specialSlots = snapshot.SpecialSlots;

            for (int index = 0; index < 100; index++)
                Assert.That(AiSensingKernel.TryFindNearest(snapshot, 0, 2, out _), Is.True);

            Assert.That(snapshot.Included, Is.SameAs(included));
            Assert.That(snapshot.X, Is.SameAs(x));
            Assert.That(snapshot.SpecialSlots, Is.SameAs(specialSlots));
        }

        [Test]
        public void Special_EmptyIndexedFeatureAtCapacity1000AvoidsFullRangeWalk()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(1000, 56);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 30, 0, 0);
            snapshot.SpecialIndexReady = true;
            snapshot.TeamSummariesReady = true;

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot,
                0,
                2,
                1,
                30,
                true,
                AiDecisionEvaluationPolicy.FullScan,
                out AiSensingSpecialResult full), Is.True);
            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot,
                0,
                2,
                1,
                30,
                true,
                AiDecisionEvaluationPolicy.Indexed,
                out AiSensingSpecialResult indexed), Is.True);

            Assert.That(full.SlotVisits, Is.EqualTo(980));
            Assert.That(indexed.SlotVisits, Is.Zero);
            Assert.That(indexed.SelectedSlot, Is.EqualTo(full.SelectedSlot));
            Assert.That(indexed.Flags, Is.EqualTo(full.Flags));
        }

        [Test]
        public void Nearest_ReadyEmptyIndexDoesNotFallBackToLinearRows()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(3, 57);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 30, 0, 0);
            snapshot.RoleIndexesReady = true;

            Assert.That(AiSensingKernel.TryFindNearest(
                snapshot,
                0,
                2,
                AiDecisionEvaluationPolicy.FullScan,
                out AiSensingNearestResult full), Is.True);
            Assert.That(AiSensingKernel.TryFindNearest(
                snapshot,
                0,
                2,
                AiDecisionEvaluationPolicy.Indexed,
                out AiSensingNearestResult indexed), Is.True);
            Assert.That(full.SelectedSlot, Is.EqualTo(1));
            Assert.That(indexed.SelectedSlot, Is.EqualTo(-1));
            Assert.That(indexed.GroundRowVisits, Is.Zero);
        }

        [Test]
        public void IndexedContract_ReverseEnumerationRejectsOmittedSpecialAndRoleRows()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(24, 58);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 30, 0, 0);
            SetSpecial(snapshot, 20, 100, 60);
            snapshot.DataObjectType[20] = 1;
            snapshot.SpecialSlots[0] = 20;
            snapshot.SpecialSlotCount = 1;
            snapshot.GroundRoleSlotsByX[0] = 0;
            snapshot.GroundRoleSlotsByX[1] = 1;
            snapshot.GroundRoleSlotCount = 2;
            snapshot.GroundRoleTeamSummaries[0] = new AiSensingRoleTeamSummary
            {
                Team = 1,
                Start = 0,
                Count = 1,
            };
            snapshot.GroundRoleTeamSummaries[1] = new AiSensingRoleTeamSummary
            {
                Team = 2,
                Start = 1,
                Count = 1,
            };
            snapshot.GroundRoleTeamSummaryCount = 2;
            snapshot.TeamSummaries[0] = new AiSensingTeamSummary
            {
                Team = 1,
                Count = 1,
                MinHp = 500,
                MinCount = 1,
                SecondMinHp = int.MaxValue,
            };
            snapshot.TeamSummaries[1] = new AiSensingTeamSummary
            {
                Team = 2,
                Count = 1,
                MinHp = 500,
                MinCount = 1,
                SecondMinHp = int.MaxValue,
            };
            snapshot.TeamSummaryCount = 2;
            snapshot.SpecialIndexReady = true;
            snapshot.RoleIndexesReady = true;
            snapshot.TeamSummariesReady = true;

            Assert.That(AiSensingKernel.ValidateIndexedContract(snapshot), Is.True);

            snapshot.GroundRoleSlotCount = 1;
            snapshot.GroundRoleTeamSummaryCount = 1;
            Assert.That(AiSensingKernel.ValidateIndexedContract(snapshot), Is.False,
                "a role row omitted from the forward index must be found by reverse enumeration");

            snapshot.GroundRoleSlotCount = 2;
            snapshot.GroundRoleTeamSummaryCount = 2;
            snapshot.SpecialScanMember[20] = false;
            snapshot.SpecialSlotCount = 0;
            Assert.That(AiSensingKernel.ValidateIndexedContract(snapshot), Is.False,
                "special membership must be recomputed from the captured object id");
        }

        private static AiSensingSnapshot CreateSnapshot(int capacity, ulong epoch)
        {
            var snapshot = new AiSensingSnapshot(capacity);
            snapshot.Reset(epoch);
            for (int slot = 0; slot < capacity; slot++)
            {
                snapshot.CoordinateTargetX[slot] = -1000;
                snapshot.KillCount[slot] = -1;
            }
            return snapshot;
        }

        private static void SetCharacter(
            AiSensingSnapshot snapshot,
            int slot,
            int team,
            int x,
            int z,
            int state)
        {
            snapshot.Included[slot] = true;
            snapshot.Generation[slot] = (uint)(slot + 1);
            snapshot.Identity[slot] = slot + 1000;
            snapshot.DataObjectType[slot] = 0;
            snapshot.Team[slot] = team;
            snapshot.X[slot] = x;
            snapshot.Z[slot] = z;
            snapshot.State[slot] = state;
            snapshot.Hp[slot] = 500;
            snapshot.Hp3[slot] = 500;
            snapshot.HpMax[slot] = 500;
            snapshot.CoordinateTargetX[slot] = -1000;
            snapshot.KillCount[slot] = -1;
        }

        private static void SetSpecial(AiSensingSnapshot snapshot, int slot, int objectId, int x)
        {
            snapshot.Included[slot] = true;
            snapshot.SpecialScanMember[slot] = true;
            snapshot.Generation[slot] = (uint)(slot + 1);
            snapshot.Identity[slot] = slot + 1000;
            snapshot.ObjectId[slot] = objectId;
            snapshot.X[slot] = x;
            snapshot.State[slot] = 0x3EC;
            snapshot.Hp[slot] = 1;
            snapshot.Hp3[slot] = 1;
            snapshot.HpMax[slot] = 1;
            snapshot.KillCount[slot] = -1;
        }
    }
}
#endif
