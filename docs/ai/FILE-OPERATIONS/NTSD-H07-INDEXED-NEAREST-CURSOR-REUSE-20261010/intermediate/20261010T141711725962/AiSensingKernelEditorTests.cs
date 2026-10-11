#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class AiSensingKernelEditorTests
    {
        private delegate bool CursorQuery(AiSensingSnapshot rows, int self, int phase,
            AiDecisionEvaluationPolicy policy, out AiSensingNearestResult result);

        private static CursorQuery BindCursorCandidate()
        {
            var method = typeof(AiSensingKernel).GetMethod("TryFindNearestCursorReuseForDiagnostics",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "The isolated candidate entry must exist.");
            return (CursorQuery)Delegate.CreateDelegate(typeof(CursorQuery), method);
        }

        [Test]
        public void CursorReuse_EntryExists()
        {
            Assert.That(BindCursorCandidate(), Is.Not.Null);
        }

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
            snapshot.OwnerSlot[0] = -1;
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
        public void Special_OwnerSlotAloneGuardsOid122And123()
        {
            AiSensingSnapshot snapshot = CreateSnapshot(21, 1237);
            SetCharacter(snapshot, 0, 1, 0, 0, 0);
            SetCharacter(snapshot, 1, 2, 80, 0, 0);
            SetSpecial(snapshot, 20, 122, 10);

            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot, 0, 2, 1, 100, false, true, out AiSensingSpecialResult result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(20));

            snapshot.OwnerSlot[0] = 0;
            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot, 0, 2, 1, 100, false, true, out result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(1));
            Assert.That(result.Flags & AiSensingKernel.SpecialGuard7A, Is.Not.Zero);
            Assert.That(result.Flags & AiSensingKernel.SpecialGuard7B, Is.Not.Zero);

            snapshot.OwnerSlot[0] = -1;
            snapshot.ObjectId[20] = 123;
            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot, 0, 2, 1, 100, false, true, out result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(20));

            snapshot.OwnerSlot[0] = 37;
            Assert.That(AiSensingKernel.TryScanSpecial(
                snapshot, 0, 2, 1, 100, false, true, out result), Is.True);
            Assert.That(result.SelectedSlot, Is.EqualTo(1));
            Assert.That(result.Flags & AiSensingKernel.SpecialGuard7B, Is.Not.Zero);
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

        private static int CursorAbs(int value) => value < 0 ? -value : value;

        private static void BuildCursorIndexes(AiSensingSnapshot rows)
        {
            var ground = new System.Collections.Generic.List<int>();
            var air = new System.Collections.Generic.List<int>();
            var teams = new System.Collections.Generic.SortedSet<int>();
            rows.SpecialSlotCount = 0;
            Array.Clear(rows.SpecialScanMember, 0, rows.Capacity);
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!rows.Included[slot]) continue;
                int oid = rows.ObjectId[slot];
                if (slot >= 20 && (oid / 100 == 1 || oid == 200 || oid == 211 || oid == 212 || oid == 213))
                {
                    rows.SpecialScanMember[slot] = true;
                    rows.SpecialSlots[rows.SpecialSlotCount++] = slot;
                }
                if (rows.Hp[slot] <= 0) continue;
                if (rows.DataObjectType[slot] == 0) teams.Add(rows.Team[slot]);
                if (rows.State[slot] == 14 || CursorAbs(rows.HitStop[slot]) > 2) air.Add(slot);
                else if (rows.DataObjectType[slot] == 0 || rows.State[slot] == 3000) ground.Add(slot);
            }
            Comparison<int> order = (a, b) =>
            {
                int comparison = rows.Team[a].CompareTo(rows.Team[b]);
                if (comparison == 0) comparison = rows.X[a].CompareTo(rows.X[b]);
                return comparison == 0 ? a.CompareTo(b) : comparison;
            };
            ground.Sort(order);
            air.Sort(order);
            rows.GroundRoleSlotCount = ground.Count;
            rows.AirRoleSlotCount = air.Count;
            rows.GroundRoleTeamSummaryCount = FillCursorSpans(rows, ground, rows.GroundRoleSlotsByX, rows.GroundRoleTeamSummaries);
            rows.AirRoleTeamSummaryCount = FillCursorSpans(rows, air, rows.AirRoleSlotsByX, rows.AirRoleTeamSummaries);
            rows.TeamSummaryCount = 0;
            foreach (int team in teams)
            {
                int count = 0, min = int.MaxValue, second = int.MaxValue, minCount = 0;
                for (int slot = 0; slot < rows.Capacity; slot++)
                {
                    if (!rows.Included[slot] || rows.DataObjectType[slot] != 0 ||
                        rows.Hp[slot] <= 0 || rows.Team[slot] != team) continue;
                    count++;
                    int hp = rows.Hp[slot];
                    if (hp < min) { second = min; min = hp; minCount = 1; }
                    else if (hp == min) minCount++;
                    else if (hp < second) second = hp;
                }
                rows.TeamSummaries[rows.TeamSummaryCount++] = new AiSensingTeamSummary
                {
                    Team = team, Count = count, MinHp = min, MinCount = minCount, SecondMinHp = second
                };
            }
            rows.RoleIndexesReady = rows.SpecialIndexReady = rows.TeamSummariesReady = true;
            Assert.That(AiSensingKernel.ValidateIndexedContract(rows), Is.True, "Cold fixture must have complete indexes.");
        }

        private static int FillCursorSpans(AiSensingSnapshot rows, System.Collections.Generic.List<int> source,
            int[] slots, AiSensingRoleTeamSummary[] summaries)
        {
            int count = 0;
            for (int index = 0; index < source.Count;)
            {
                int start = index, team = rows.Team[source[index]];
                while (index < source.Count && rows.Team[source[index]] == team)
                {
                    slots[index] = source[index];
                    index++;
                }
                summaries[count++] = new AiSensingRoleTeamSummary { Team = team, Start = start, Count = index - start };
            }
            return count;
        }

        private static void CompareCursorResult(AiSensingNearestResult expected, AiSensingNearestResult actual, bool visits)
        {
            Assert.That(actual.SelectedSlot, Is.EqualTo(expected.SelectedSlot));
            Assert.That(actual.BestDist, Is.EqualTo(expected.BestDist));
            Assert.That(actual.SameZLane, Is.EqualTo(expected.SameZLane));
            Assert.That(actual.CapturedOccupancyEpoch, Is.EqualTo(expected.CapturedOccupancyEpoch));
            Assert.That(actual.SelectedGeneration, Is.EqualTo(expected.SelectedGeneration));
            Assert.That(actual.SelectedIdentity, Is.EqualTo(expected.SelectedIdentity));
            if (visits)
            {
                Assert.That(actual.GroundRowVisits, Is.EqualTo(expected.GroundRowVisits));
                Assert.That(actual.AirRowVisits, Is.EqualTo(expected.AirRowVisits));
            }
        }

        private static void CompareCursorQuery(CursorQuery candidate, AiSensingSnapshot rows, int self, int phase, bool linear)
        {
            bool oldAvailable = AiSensingKernel.TryFindNearest(rows, self, phase, AiDecisionEvaluationPolicy.Indexed, out var old);
            bool newAvailable = candidate(rows, self, phase, AiDecisionEvaluationPolicy.Indexed, out var result);
            Assert.That(newAvailable, Is.EqualTo(oldAvailable));
            CompareCursorResult(old, result, true);
            if (linear)
            {
                bool fullAvailable = AiSensingKernel.TryFindNearest(rows, self, phase, AiDecisionEvaluationPolicy.FullScan, out var full);
                Assert.That(newAvailable, Is.EqualTo(fullAvailable));
                CompareCursorResult(full, result, false);
            }
        }

        private static void CompareCursorSnapshot(CursorQuery candidate, AiSensingSnapshot rows, bool linear)
        {
            for (int self = 0; self < rows.Capacity; self++)
                foreach (int phase in new[] { 1, 2, 4 })
                    CompareCursorQuery(candidate, rows, self, phase, linear);
        }

        [Test]
        public void CursorReuse_ExplicitBoundariesAndEligibilityMatchOriginal()
        {
            var candidate = BindCursorCandidate();
            var rows = CreateSnapshot(40, 9191);
            int[] x = { 0, -10001, -10000, -9999, -250, -249, -10, 0, 0, 10, 249, 250, 9999, 10000, 10001 };
            int[] z = { -40, -39, -15, -14, 0, 14, 15, 39, 40 };
            double[] velocity = { -1, 0, 0.001, 0.002, 1, double.NaN };
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                SetCharacter(rows, slot, new[] { -1, 0, 1, 2, 5 }[slot % 5],
                    x[slot % x.Length], z[slot % z.Length], slot % 4 == 0 ? 14 : 0);
                rows.HitStop[slot] = new[] { -3, -2, 0, 2, 3 }[slot % 5];
                if (slot % 7 == 0) rows.Hp[slot] = 0;
                if (slot >= 20)
                {
                    rows.DataObjectType[slot] = 1;
                    rows.State[slot] = slot % 2 == 0 ? 3000 : 0;
                    rows.Vx[slot] = velocity[slot % velocity.Length];
                }
            }
            BuildCursorIndexes(rows);
            CompareCursorSnapshot(candidate, rows, true);
            for (int step = 0; step < 6; step++)
            {
                rows.X[0] = step % 2 == 0 ? -20000 : 20000; // Exhaust either side.
                rows.State[0] = step % 3 == 0 ? 9 : 0;
                rows.Hp[0] = step % 2 == 0 ? 0 : 500; // Self HP is not an admission guard.
                rows.X[21] = rows.X[0];
                rows.HitStop[22] = step - 3;
                rows.CapturedOccupancyEpoch++;
                rows.Generation[23]++;
                rows.Identity[23]++;
                BuildCursorIndexes(rows);
                CompareCursorSnapshot(candidate, rows, true);
            }
        }

        [Test]
        public void CursorReuse_TieGroundFactsStrictLimitsAndState9()
        {
            var candidate = BindCursorCandidate();
            var rows = CreateSnapshot(6, 92);
            SetCharacter(rows, 0, 1, 0, 0, 0);
            SetCharacter(rows, 1, 2, 10, 0, 0);
            SetCharacter(rows, 2, 2, -10, 0, 0);
            SetCharacter(rows, 3, 2, 249, 39, 14);
            BuildCursorIndexes(rows);
            CompareCursorQuery(candidate, rows, 0, 2, true);
            candidate(rows, 0, 2, AiDecisionEvaluationPolicy.Indexed, out var result);
            Assert.That(result.SelectedSlot, Is.EqualTo(3));
            Assert.That(result.BestDist, Is.EqualTo(10));
            Assert.That(result.SameZLane, Is.True);
            rows.State[0] = 9;
            candidate(rows, 0, 2, AiDecisionEvaluationPolicy.Indexed, out result);
            Assert.That(result.SelectedSlot, Is.EqualTo(1));
            rows.State[0] = 0;
            rows.Z[3] = 40;
            CompareCursorQuery(candidate, rows, 0, 2, true);
            candidate(rows, 0, 2, AiDecisionEvaluationPolicy.Indexed, out result);
            Assert.That(result.SelectedSlot, Is.EqualTo(1));
            rows.Z[3] = 0; rows.X[3] = 250;
            BuildCursorIndexes(rows);
            CompareCursorQuery(candidate, rows, 0, 2, true);
            rows.Included[1] = rows.Included[2] = rows.Included[3] = false;
            SetCharacter(rows, 4, 2, 10000, 0, 0);
            BuildCursorIndexes(rows);
            candidate(rows, 0, 2, AiDecisionEvaluationPolicy.Indexed, out result);
            Assert.That(result.SelectedSlot, Is.EqualTo(-1));
            Assert.That(result.BestDist, Is.EqualTo(10000));
            CompareCursorSnapshot(candidate, rows, true);
        }

        [Test]
        public void CursorReuse_ReadinessEmptyIndexAndNonIndexedDelegation()
        {
            var candidate = BindCursorCandidate();
            CompareCursorQuery(candidate, null, 0, 2, false);
            var rows = CreateSnapshot(3, 93);
            SetCharacter(rows, 0, 1, 0, 0, 0);
            SetCharacter(rows, 1, 2, 30, 0, 0);
            CompareCursorQuery(candidate, rows, 0, 2, false);
            BuildCursorIndexes(rows);
            CompareCursorQuery(candidate, rows, -1, 2, true);
            CompareCursorQuery(candidate, rows, 3, 2, true);
            CompareCursorQuery(candidate, rows, 2, 2, true);
            rows.CoordinateTargetX[0] = -999;
            CompareCursorQuery(candidate, rows, 0, 2, true);
            rows.CoordinateTargetX[0] = -1000;
            foreach (AiDecisionEvaluationPolicy policy in new[] { AiDecisionEvaluationPolicy.FullScan, (AiDecisionEvaluationPolicy)77 })
            {
                bool a = AiSensingKernel.TryFindNearest(rows, 0, 2, policy, out var expected);
                bool b = candidate(rows, 0, 2, policy, out var actual);
                Assert.That(b, Is.EqualTo(a)); CompareCursorResult(expected, actual, true);
            }
            // Deliberate existing readiness contract: ready empty indexes never fall back to linear.
            rows.GroundRoleTeamSummaryCount = rows.AirRoleTeamSummaryCount = 0;
            rows.GroundRoleSlotCount = rows.AirRoleSlotCount = 0;
            CompareCursorQuery(candidate, rows, 0, 2, false);
            candidate(rows, 0, 2, AiDecisionEvaluationPolicy.Indexed, out var empty);
            Assert.That(empty.SelectedSlot, Is.EqualTo(-1));
            Assert.That(empty.GroundRowVisits + empty.AirRowVisits, Is.Zero);
        }

        [Test]
        public void CursorReuse_SeededSnapshotsAndIntercallMutationMatchAllFields()
        {
            var candidate = BindCursorCandidate();
            var random = new Random(20261010);
            int comparisons = 0;
            int[] capacities = { 1, 2, 8, 20, 32, 64 };
            int[] coordinates = { -10001, -10000, -250, -249, -1, 0, 1, 249, 250, 9999, 10000 };
            int[] teams = { -1, 0, 1, 2, 5, 6 };
            int[] states = { 0, 9, 14, 3000 };
            double[] velocities = { -1, 0, 0.001, 0.002, 1, double.NaN };
            for (int fixture = 0; fixture < 256; fixture++)
            {
                var rows = CreateSnapshot(capacities[fixture % capacities.Length], (ulong)(fixture + 100));
                for (int slot = 0; slot < rows.Capacity; slot++)
                {
                    SetCharacter(rows, slot, teams[random.Next(teams.Length)], coordinates[random.Next(coordinates.Length)],
                        random.Next(-40, 41), states[random.Next(states.Length)]);
                    rows.Included[slot] = random.Next(8) != 0;
                    rows.Hp[slot] = random.Next(3) == 0 ? 0 : random.Next(1, 501);
                    rows.DataObjectType[slot] = random.Next(2);
                    rows.HitStop[slot] = random.Next(-3, 4);
                    rows.Vx[slot] = velocities[random.Next(velocities.Length)];
                }
                BuildCursorIndexes(rows);
                CompareCursorSnapshot(candidate, rows, true);
                comparisons += rows.Capacity * 3;
                rows.X[0] = random.Next(-1000, 1001);
                rows.State[0] = states[random.Next(states.Length)];
                rows.Team[0] = 5; rows.Generation[0]++; rows.CapturedOccupancyEpoch++;
                BuildCursorIndexes(rows);
                CompareCursorQuery(candidate, rows, 0, 1, true); comparisons++;
            }
            TestContext.WriteLine("C# indexed/candidate and normal FullScan payload comparisons: " + comparisons);
        }

        [Test]
        public void CursorReuse_IntExtremesPreserveOriginalIndexedArithmetic()
        {
            var candidate = BindCursorCandidate();
            var rows = CreateSnapshot(16, 94);
            int[] x = { int.MinValue, int.MaxValue, -1, 0, 1, -10000, 10000, 249 };
            for (int i = 0; i < 16; i++)
            {
                SetCharacter(rows, i, i % 3, x[i % 8], x[(i + 3) % 8], i % 4 == 0 ? 14 : 0);
                rows.HitStop[i] = i % 5 == 0 ? int.MinValue : 0;
            }
            BuildCursorIndexes(rows);
            CompareCursorSnapshot(candidate, rows, false);
        }

        private static AiSensingSnapshot CursorCostSnapshot(int spacing)
        {
            var rows = CreateSnapshot(1000, 20261010);
            var random = new Random(20261010);
            for (int slot = 0; slot < 1000; slot++)
            {
                SetCharacter(rows, slot, slot % 4, (slot % 50 - 25) * spacing + random.Next(-3, 4),
                    (slot / 50 - 10) * 4, slot % 11 == 0 ? 14 : 0);
                rows.HitStop[slot] = slot % 17 == 0 ? 3 : 0;
            }
            BuildCursorIndexes(rows);
            return rows;
        }

        private static long CursorQueryRound(CursorQuery query, AiSensingSnapshot rows)
        {
            long aggregate = 0;
            for (int self = 0; self < 1000; self++)
            {
                bool available = query(rows, self, self % 3 == 0 ? 1 : 2, AiDecisionEvaluationPolicy.Indexed, out var result);
                aggregate += result.SelectedSlot + result.BestDist + result.GroundRowVisits + result.AirRowVisits + (available ? 1 : 0);
            }
            return aggregate;
        }

        [TestCase(120)]
        [TestCase(12)]
        public void CursorReuse_WarmedQueriesBothPathsZeroBytesAndFixedArrays(int spacing)
        {
            var rows = CursorCostSnapshot(spacing);
            CursorQuery original = AiSensingKernel.TryFindNearest;
            var candidate = BindCursorCandidate();
            CompareCursorSnapshot(candidate, rows, true);
            string beforeState = Newtonsoft.Json.JsonConvert.SerializeObject(rows);
            foreach (CursorQuery query in new[] { original, candidate })
            {
                long sum = 0;
                for (int warm = 0; warm < 8; warm++) sum += CursorQueryRound(query, rows);
                var x = rows.X; var ground = rows.GroundRoleSlotsByX; var air = rows.AirRoleSlotsByX;
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int round = 0; round < 32; round++) sum += CursorQueryRound(query, rows);
                long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(bytes, Is.Zero);
                Assert.That(rows.Capacity, Is.EqualTo(1000));
                Assert.That(rows.X, Is.SameAs(x)); Assert.That(rows.GroundRoleSlotsByX, Is.SameAs(ground)); Assert.That(rows.AirRoleSlotsByX, Is.SameAs(air));
                Assert.That(Newtonsoft.Json.JsonConvert.SerializeObject(rows), Is.EqualTo(beforeState));
                Assert.That(sum, Is.Not.Zero);
            }
        }

        private static AiSensingSnapshot CreateSnapshot(int capacity, ulong epoch)
        {
            var snapshot = new AiSensingSnapshot(capacity);
            snapshot.Reset(epoch);
            for (int slot = 0; slot < capacity; slot++)
            {
                snapshot.CoordinateTargetX[slot] = -1000;
                snapshot.OwnerSlot[slot] = -1;
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
            snapshot.OwnerSlot[slot] = -1;
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
            snapshot.OwnerSlot[slot] = -1;
        }
    }
}
#endif
