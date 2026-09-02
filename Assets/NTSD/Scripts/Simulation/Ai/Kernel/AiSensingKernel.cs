namespace NTSD.Simulation
{
    public struct AiSensingNearestResult
    {
        public int SelectedSlot;
        public int BestDist;
        public bool SameZLane;
        public ulong CapturedOccupancyEpoch;
        public uint SelectedGeneration;
        public int SelectedIdentity;
        public int GroundRowVisits;
        public int AirRowVisits;
    }

    public struct AiSensingSpecialResult
    {
        public int SelectedSlot;
        public int BestDist;
        public bool SameZLane;
        public ulong CapturedOccupancyEpoch;
        public uint SelectedGeneration;
        public int SelectedIdentity;
        public int Flags;
        public int SlotVisits;
    }

    public static class AiSensingKernel
    {
        public const int SpecialProximity = 1 << 0;
        public const int SpecialLeft = 1 << 1;
        public const int SpecialRight = 1 << 2;
        public const int SpecialUp = 1 << 3;
        public const int SpecialDown = 1 << 4;
        public const int SpecialGuard7A = 1 << 5;
        public const int SpecialGuard7B = 1 << 6;
        public const int SpecialForce7AGround = 1 << 7;
        public const int SpecialC8ThreatSeen = 1 << 8;
        public const int SpecialPostSelectionSeen = 1 << 9;

        public static bool TryFindNearest(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            out AiSensingNearestResult result)
        {
            bool useIndexed = rows != null &&
                              (rows.GroundRoleTeamSummaryCount > 0 ||
                               rows.AirRoleTeamSummaryCount > 0);
            return TryFindNearestCore(rows, selfSlot, inputPhase, useIndexed, false, out result);
        }

        public static bool TryFindNearest(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            AiDecisionEvaluationPolicy policy,
            out AiSensingNearestResult result)
        {
            return TryFindNearestCore(
                rows,
                selfSlot,
                inputPhase,
                policy == AiDecisionEvaluationPolicy.Indexed,
                true,
                out result);
        }

        private static bool TryFindNearestCore(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            bool useIndexed,
            bool requireReady,
            out AiSensingNearestResult result)
        {
            result = default;
            if (useIndexed && requireReady && (rows == null || !rows.RoleIndexesReady))
                return false;
            // Alignment contract R3-AI-LIFE-001: C++ prepare_ai_input does not
            // reject the active self by HP before its no-target roll/clear path.
            if (!IsIncluded(rows, selfSlot) ||
                rows.CoordinateTargetX[selfSlot] > -1000)
            {
                return false;
            }

            int selectedSlot;
            int bestDist;
            if (useIndexed)
                selectedSlot = FindIndexedGround(rows, selfSlot, inputPhase, ref result, out bestDist);
            else
                selectedSlot = FindLinearGround(rows, selfSlot, inputPhase, ref result, out bestDist);

            bool sameZLane = selectedSlot >= 0 &&
                             Abs(rows.Z[selectedSlot] - rows.Z[selfSlot]) < 15;
            if (rows.State[selfSlot] != 9)
            {
                int airSlot = useIndexed
                    ? FindIndexedAir(rows, selfSlot, inputPhase, ref result)
                    : FindLinearAir(rows, selfSlot, inputPhase, ref result);
                if (airSlot >= 0)
                    selectedSlot = airSlot;
            }

            result.SelectedSlot = selectedSlot;
            result.BestDist = bestDist;
            result.SameZLane = sameZLane;
            CaptureHandle(rows, selectedSlot, ref result.SelectedGeneration, ref result.SelectedIdentity);
            result.CapturedOccupancyEpoch = rows.CapturedOccupancyEpoch;
            return true;
        }

        public static bool TryScanSpecial(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            int initialSelectedSlot,
            int nearestBestDist,
            bool sameZLane,
            bool forceFullScan,
            out AiSensingSpecialResult result)
        {
            return TryScanSpecial(
                rows,
                selfSlot,
                inputPhase,
                initialSelectedSlot,
                nearestBestDist,
                sameZLane,
                forceFullScan
                    ? AiDecisionEvaluationPolicy.FullScan
                    : AiDecisionEvaluationPolicy.Indexed,
                out result);
        }

        public static bool TryScanSpecial(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            int initialSelectedSlot,
            int nearestBestDist,
            bool sameZLane,
            AiDecisionEvaluationPolicy policy,
            out AiSensingSpecialResult result)
        {
            result = default;
            bool useIndexed = policy == AiDecisionEvaluationPolicy.Indexed;
            if (useIndexed &&
                (rows == null || !rows.SpecialIndexReady || !rows.TeamSummariesReady))
            {
                return false;
            }
            if (!IsIncluded(rows, selfSlot) || !IsIncluded(rows, initialSelectedSlot))
                return false;

            int selectedSlot = initialSelectedSlot;
            int selectedBeforeScan = selectedSlot;
            bool proximity = false;
            bool left = false;
            bool right = false;
            bool up = false;
            bool down = false;
            bool guard7A = false;
            bool guard7B = false;
            bool force7AGround = false;
            bool c8ThreatSeen = false;
            bool postSelectionSeen = false;
            int specialBestDist = 10000;
            int selfTeam = rows.Team[selfSlot];

            if ((inputPhase == 1 || inputPhase == 4) && selfTeam != 5)
            {
                force7AGround = true;
                if (rows.Hp[selfSlot] > (4 * rows.Hp3[selfSlot]) / 5 ||
                    rows.Hp[selfSlot] > rows.Hp3[selfSlot] - 130)
                    force7AGround = false;
                if (rows.Hp[selfSlot] > 430 ||
                    rows.Hp[selfSlot] > rows.Hp3[selfSlot] - 130)
                    guard7A = true;

                if (useIndexed)
                {
                    if (!TryGetSameTeamSummaryExcludingSelf(
                        rows,
                        selfSlot,
                        selfTeam,
                        out int sameTeamCount,
                        out int sameTeamMinHp))
                    {
                        return false;
                    }
                    ApplySameTeamGuard(
                        rows,
                        selfSlot,
                        sameTeamCount,
                        sameTeamMinHp,
                        ref force7AGround,
                        ref guard7A);
                }
                else
                {
                    GetSameTeamSummaryExcludingSelf(rows, selfSlot, selfTeam,
                        out int sameTeamCount, out int sameTeamMinHp);
                    ApplySameTeamGuard(
                        rows,
                        selfSlot,
                        sameTeamCount,
                        sameTeamMinHp,
                        ref force7AGround,
                        ref guard7A);
                }
            }

            if (rows.KillCount[selfSlot] > -1) { guard7A = true; guard7B = true; }
            if (rows.Pp[selfSlot] > 250) guard7B = true;
            if (inputPhase == 1 && selfTeam == 1) guard7B = true;
            if (selfSlot >= 20 && inputPhase == 4) guard7B = true;

            int scanCount = useIndexed ? rows.SpecialSlotCount : Max(0, rows.Capacity - 20);
            for (int scanIndex = 0; scanIndex < scanCount; scanIndex++)
            {
                int slot = useIndexed ? rows.SpecialSlots[scanIndex] : scanIndex + 20;
                result.SlotVisits++;
                if (!IsIncluded(rows, slot))
                    continue;

                int objectId = rows.ObjectId[slot];
                int state = rows.State[slot];
                if (objectId == 0xC8)
                {
                    int frameGroup = rows.Frame[slot] / 10;
                    bool threat = frameGroup == 6 && rows.Team[slot] != selfTeam;
                    if (!threat && frameGroup == 5)
                    {
                        bool lowHpWindow =
                            (rows.Hp[selfSlot] >= rows.Hp3[selfSlot] - 70 ||
                             rows.Hp[selfSlot] >= rows.Hp3[selfSlot] - 200) &&
                            (rows.Hp[selfSlot] >= (3 * rows.Hp3[selfSlot]) / 5 ||
                             rows.Hp[selfSlot] < rows.Hp3[selfSlot] - 200);
                        threat = (rows.ObjectId[selfSlot] == 2 || rows.ObjectId[selfSlot] == 34) &&
                                 lowHpWindow && rows.Team[slot] == selfTeam;
                    }
                    if (threat) c8ThreatSeen = true;
                    if (threat && Abs(rows.Z[slot] - rows.Z[selfSlot]) < 25 &&
                        Abs(rows.X[slot] - rows.X[selfSlot]) < 150)
                    {
                        proximity = true;
                        if (Abs(rows.Z[slot] - rows.Z[selfSlot]) < 20)
                        {
                            if (Abs(rows.X[slot] - rows.X[selfSlot]) < 180)
                            {
                                if (rows.Z[slot] <= rows.Z[selfSlot]) up = true;
                                else down = true;
                            }
                            if (rows.X[slot] <= rows.X[selfSlot]) left = true;
                            else right = true;
                        }
                    }
                }

                if ((objectId == 0xD3 && state == 0x12) ||
                    (objectId == 0xD4 && rows.Frame[slot] >= 150 && rows.Frame[slot] <= 170))
                {
                    if (Abs(rows.X[slot] - rows.X[selfSlot]) < 80)
                    {
                        if (rows.Z[slot] > rows.Z[selfSlot] + 20) down = true;
                        else if (rows.Z[slot] < rows.Z[selfSlot] - 20) up = true;
                    }
                    if (Abs(rows.Z[slot] - rows.Z[selfSlot]) < 20)
                    {
                        if (rows.X[slot] > rows.X[selfSlot] + 100) right = true;
                        else if (rows.X[slot] < rows.X[selfSlot] - 100) left = true;
                    }
                }

                if (!postSelectionSeen && !c8ThreatSeen && !sameZLane && rows.LinkState[selfSlot] == 0)
                {
                    int distance = Distance(rows, selfSlot, slot);
                    bool objectIdCandidate = objectId / 100 == 1 || objectId == 0xD5;
                    bool guarded = (objectId == 0x7A && guard7A) ||
                                   (objectId == 0x7B && guard7B) ||
                                   (rows.InputHistoryGate[selfSlot] && objectId != 0x7A);
                    if (distance < 2 * nearestBestDist && distance < specialBestDist &&
                        objectIdCandidate && !guarded && rows.LinkState[slot] == 0 &&
                        (state == 0x3EC || state == 0x7D4))
                    {
                        selectedSlot = slot;
                        specialBestDist = distance;
                    }
                }

                if (objectId == 0xC8 && rows.Frame[slot] / 10 == 5 &&
                    Abs(rows.X[slot] - rows.X[selfSlot]) < 300 &&
                    Abs(rows.Z[slot] - rows.Z[selfSlot]) < 90 && rows.Team[slot] == selfTeam)
                {
                    bool pressure =
                        (rows.Hp[selfSlot] < rows.HpMax[selfSlot] - 70 && rows.Hp[selfSlot] < 140) ||
                        (rows.Hp[selfSlot] < (3 * rows.HpMax[selfSlot]) / 5 && rows.Hp[selfSlot] >= 140);
                    if (pressure) selectedSlot = slot;
                    postSelectionSeen = true;
                }

                if (force7AGround && objectId == 0x7A && state == 0x3EC && rows.LinkState[selfSlot] == 0)
                {
                    selectedSlot = slot;
                    postSelectionSeen = true;
                }
            }

            if (c8ThreatSeen) selectedSlot = selectedBeforeScan;
            result.SelectedSlot = selectedSlot;
            result.BestDist = specialBestDist;
            result.SameZLane = sameZLane;
            result.CapturedOccupancyEpoch = rows.CapturedOccupancyEpoch;
            CaptureHandle(rows, selectedSlot, ref result.SelectedGeneration, ref result.SelectedIdentity);
            result.Flags = PackFlags(proximity, left, right, up, down, guard7A, guard7B,
                force7AGround, c8ThreatSeen, postSelectionSeen);
            return true;
        }

        private static bool IsIncluded(AiSensingSnapshot rows, int slot)
        {
            return rows != null && slot >= 0 && slot < rows.Capacity && rows.Included[slot];
        }

        private static int FindLinearGround(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            ref AiSensingNearestResult result,
            out int bestDist)
        {
            int selectedSlot = -1;
            bestDist = 10000;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!IsGroundTarget(rows, selfSlot, slot, inputPhase))
                    continue;
                result.GroundRowVisits++;
                int distance = Distance(rows, selfSlot, slot);
                if (distance < bestDist)
                {
                    bestDist = distance;
                    selectedSlot = slot;
                }
            }
            return selectedSlot;
        }

        private static int FindIndexedGround(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            ref AiSensingNearestResult result,
            out int bestDist)
        {
            int selectedSlot = -1;
            bestDist = 10000;
            int selfX = rows.X[selfSlot];
            for (int summaryIndex = 0; summaryIndex < rows.GroundRoleTeamSummaryCount; summaryIndex++)
            {
                AiSensingRoleTeamSummary summary = rows.GroundRoleTeamSummaries[summaryIndex];
                if (summary.Count <= 0 || !TeamAllowed(rows.Team[selfSlot], summary.Team, inputPhase))
                    continue;
                int spanEnd = summary.Start + summary.Count;
                int left = LowerBound(rows, rows.GroundRoleSlotsByX, summary.Start, summary.Count, selfX) - 1;
                int right = left + 1;
                while (left >= summary.Start || right < spanEnd)
                {
                    int leftDx = left >= summary.Start
                        ? Abs(rows.X[rows.GroundRoleSlotsByX[left]] - selfX)
                        : int.MaxValue;
                    int rightDx = right < spanEnd
                        ? Abs(rows.X[rows.GroundRoleSlotsByX[right]] - selfX)
                        : int.MaxValue;
                    if (leftDx > bestDist && rightDx > bestDist)
                        break;
                    int slot = leftDx <= rightDx
                        ? rows.GroundRoleSlotsByX[left--]
                        : rows.GroundRoleSlotsByX[right++];
                    result.GroundRowVisits++;
                    if (!IsGroundTarget(rows, selfSlot, slot, inputPhase))
                        continue;
                    int distance = Distance(rows, selfSlot, slot);
                    if (distance < bestDist ||
                        (distance == bestDist && selectedSlot >= 0 && slot < selectedSlot))
                    {
                        bestDist = distance;
                        selectedSlot = slot;
                    }
                }
            }
            return selectedSlot;
        }

        private static int FindLinearAir(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            ref AiSensingNearestResult result)
        {
            int selectedSlot = -1;
            int bestDist = 10000;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!IsAirTarget(rows, selfSlot, slot, inputPhase))
                    continue;
                result.AirRowVisits++;
                int zDistance = Abs(rows.Z[slot] - rows.Z[selfSlot]);
                int xDistance = Abs(rows.X[slot] - rows.X[selfSlot]);
                int distance = zDistance + xDistance;
                if (distance >= bestDist || zDistance >= 40 || xDistance >= 250)
                    continue;
                bestDist = distance;
                selectedSlot = slot;
            }
            return selectedSlot;
        }

        private static int FindIndexedAir(
            AiSensingSnapshot rows,
            int selfSlot,
            int inputPhase,
            ref AiSensingNearestResult result)
        {
            int selectedSlot = -1;
            int bestDist = 10000;
            int selfX = rows.X[selfSlot];
            for (int summaryIndex = 0; summaryIndex < rows.AirRoleTeamSummaryCount; summaryIndex++)
            {
                AiSensingRoleTeamSummary summary = rows.AirRoleTeamSummaries[summaryIndex];
                if (summary.Count <= 0 || !TeamAllowed(rows.Team[selfSlot], summary.Team, inputPhase))
                    continue;
                int spanEnd = summary.Start + summary.Count;
                int left = LowerBound(rows, rows.AirRoleSlotsByX, summary.Start, summary.Count, selfX) - 1;
                int right = left + 1;
                while (left >= summary.Start || right < spanEnd)
                {
                    int leftDx = left >= summary.Start
                        ? Abs(rows.X[rows.AirRoleSlotsByX[left]] - selfX)
                        : int.MaxValue;
                    int rightDx = right < spanEnd
                        ? Abs(rows.X[rows.AirRoleSlotsByX[right]] - selfX)
                        : int.MaxValue;
                    int maximumRelevantDx = bestDist < 249 ? bestDist : 249;
                    if (leftDx > maximumRelevantDx && rightDx > maximumRelevantDx)
                        break;
                    int slot = leftDx <= rightDx
                        ? rows.AirRoleSlotsByX[left--]
                        : rows.AirRoleSlotsByX[right++];
                    result.AirRowVisits++;
                    if (!IsAirTarget(rows, selfSlot, slot, inputPhase))
                        continue;
                    int zDistance = Abs(rows.Z[slot] - rows.Z[selfSlot]);
                    int xDistance = Abs(rows.X[slot] - selfX);
                    int distance = zDistance + xDistance;
                    if (zDistance >= 40 || xDistance >= 250)
                        continue;
                    if (distance < bestDist ||
                        (distance == bestDist && selectedSlot >= 0 && slot < selectedSlot))
                    {
                        bestDist = distance;
                        selectedSlot = slot;
                    }
                }
            }
            return selectedSlot;
        }

        private static int LowerBound(
            AiSensingSnapshot rows,
            int[] slots,
            int start,
            int count,
            int x)
        {
            int lower = start;
            int upper = start + count;
            while (lower < upper)
            {
                int middle = lower + ((upper - lower) >> 1);
                if (rows.X[slots[middle]] < x) lower = middle + 1;
                else upper = middle;
            }
            return lower;
        }

        private static bool IsGroundTarget(AiSensingSnapshot rows, int self, int slot, int phase)
        {
            if (slot == self || !IsIncluded(rows, slot)) return false;
            int state = rows.State[slot];
            if (rows.DataObjectType[slot] != 0)
            {
                if (state != 3000) return false;
                if (rows.X[slot] > rows.X[self]) { if (!(rows.Vx[slot] < 0.001)) return false; }
                else if (rows.X[slot] < rows.X[self]) { if (!(rows.Vx[slot] > 0.001)) return false; }
                else return false;
            }
            return TeamAllowed(rows.Team[self], rows.Team[slot], phase) &&
                   rows.Hp[slot] > 0 && state != 14 && Abs(rows.Y[slot]) <= 2;
        }

        private static bool IsAirTarget(AiSensingSnapshot rows, int self, int slot, int phase)
        {
            return slot != self && IsIncluded(rows, slot) &&
                   TeamAllowed(rows.Team[self], rows.Team[slot], phase) && rows.Hp[slot] > 0 &&
                   (rows.State[slot] == 14 || Abs(rows.Y[slot]) > 2);
        }

        private static bool TeamAllowed(int selfTeam, int candidateTeam, int phase)
        {
            if (candidateTeam != selfTeam)
            {
                if (phase != 1) return true;
                if (selfTeam == 5) return true;
            }
            if (candidateTeam != 5) return false;
            if (phase != 1) return false;
            return candidateTeam != selfTeam;
        }

        private static void GetSameTeamSummaryExcludingSelf(
            AiSensingSnapshot rows, int selfSlot, int selfTeam, out int count, out int minHp)
        {
            count = 0;
            minHp = int.MaxValue;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (slot == selfSlot || !IsIncluded(rows, slot) ||
                    rows.DataObjectType[slot] != 0 || rows.Hp[slot] <= 0 || rows.Team[slot] != selfTeam)
                    continue;
                count++;
                if (rows.Hp[slot] < minHp) minHp = rows.Hp[slot];
            }
        }

        public static bool TryGetSameTeamSummaryExcludingSelf(
            AiSensingSnapshot rows,
            int selfSlot,
            int selfTeam,
            out int count,
            out int minHp)
        {
            count = 0;
            minHp = int.MaxValue;
            if (rows == null || !rows.TeamSummariesReady ||
                selfSlot < 0 || selfSlot >= rows.Capacity)
            {
                return false;
            }

            for (int index = 0; index < rows.TeamSummaryCount; index++)
            {
                AiSensingTeamSummary summary = rows.TeamSummaries[index];
                if (summary.Team != selfTeam)
                    continue;
                if (summary.Count <= 0 || summary.MinCount <= 0 ||
                    summary.MinCount > summary.Count)
                {
                    return false;
                }

                count = summary.Count;
                minHp = summary.MinHp;
                if (IsLivingCharacter(rows, selfSlot) && rows.Team[selfSlot] == selfTeam)
                {
                    count--;
                    if (rows.Hp[selfSlot] == summary.MinHp && summary.MinCount == 1)
                        minHp = summary.SecondMinHp;
                }
                if (count <= 0)
                {
                    count = 0;
                    minHp = int.MaxValue;
                }
                return true;
            }
            return !IsLivingCharacter(rows, selfSlot) || rows.Team[selfSlot] != selfTeam;
        }

        public static bool ValidateIndexedContract(AiSensingSnapshot rows)
        {
            if (!AreIndexesReady(rows))
            {
                return false;
            }
            if (rows.SpecialSlotCount < 0 || rows.SpecialSlotCount > rows.Capacity ||
                rows.GroundRoleSlotCount < 0 || rows.GroundRoleSlotCount > rows.Capacity ||
                rows.AirRoleSlotCount < 0 || rows.AirRoleSlotCount > rows.Capacity ||
                rows.GroundRoleTeamSummaryCount < 0 ||
                rows.GroundRoleTeamSummaryCount > rows.Capacity ||
                rows.AirRoleTeamSummaryCount < 0 ||
                rows.AirRoleTeamSummaryCount > rows.Capacity ||
                rows.TeamSummaryCount < 0 || rows.TeamSummaryCount > rows.Capacity)
            {
                return false;
            }

            int specialIndex = 0;
            for (int slot = 20; slot < rows.Capacity; slot++)
            {
                bool expectedMember = IsIncluded(rows, slot) &&
                                      IsSpecialScanObjectId(rows.ObjectId[slot]);
                if (rows.SpecialScanMember[slot] != expectedMember)
                    return false;
                if (!expectedMember)
                    continue;
                if (specialIndex >= rows.SpecialSlotCount ||
                    rows.SpecialSlots[specialIndex] != slot)
                {
                    return false;
                }
                specialIndex++;
            }
            return specialIndex == rows.SpecialSlotCount &&
                   ValidateRoleIndex(
                       rows,
                       rows.GroundRoleSlotsByX,
                       rows.GroundRoleSlotCount,
                       rows.GroundRoleTeamSummaries,
                       rows.GroundRoleTeamSummaryCount,
                       true) &&
                   ValidateRoleIndex(
                       rows,
                       rows.AirRoleSlotsByX,
                       rows.AirRoleSlotCount,
                       rows.AirRoleTeamSummaries,
                       rows.AirRoleTeamSummaryCount,
                       false) &&
                   ValidateTeamSummaries(rows);
        }

        public static bool AreIndexesReady(AiSensingSnapshot rows)
        {
            return rows != null && rows.SpecialIndexReady &&
                   rows.RoleIndexesReady && rows.TeamSummariesReady;
        }

        private static bool ValidateRoleIndex(
            AiSensingSnapshot rows,
            int[] slots,
            int slotCount,
            AiSensingRoleTeamSummary[] summaries,
            int summaryCount,
            bool ground)
        {
            int reverseMemberCount = 0;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (ground ? IsGroundRoleMember(rows, slot) : IsAirRoleMember(rows, slot))
                    reverseMemberCount++;
            }
            if (reverseMemberCount != slotCount)
                return false;

            int previousSlot = -1;
            for (int index = 0; index < slotCount; index++)
            {
                int slot = slots[index];
                bool roleMember = ground
                    ? IsGroundRoleMember(rows, slot)
                    : IsAirRoleMember(rows, slot);
                if (!roleMember || slot == previousSlot)
                    return false;
                if (index > 0)
                {
                    int previousTeam = rows.Team[previousSlot];
                    int currentTeam = rows.Team[slot];
                    if (previousTeam > currentTeam ||
                        (previousTeam == currentTeam &&
                         (rows.X[previousSlot] > rows.X[slot] ||
                          (rows.X[previousSlot] == rows.X[slot] && previousSlot > slot))))
                    {
                        return false;
                    }
                }
                previousSlot = slot;
            }

            int expectedStart = 0;
            for (int index = 0; index < summaryCount; index++)
            {
                AiSensingRoleTeamSummary summary = summaries[index];
                if (summary.Start != expectedStart || summary.Count <= 0 ||
                    summary.Start + summary.Count > slotCount ||
                    rows.Team[slots[summary.Start]] != summary.Team)
                {
                    return false;
                }
                expectedStart += summary.Count;
            }
            return expectedStart == slotCount;
        }

        private static bool ValidateTeamSummaries(AiSensingSnapshot rows)
        {
            int totalLivingCharacters = 0;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (IsLivingCharacter(rows, slot))
                    totalLivingCharacters++;
            }

            int summarizedCharacters = 0;
            for (int index = 0; index < rows.TeamSummaryCount; index++)
            {
                AiSensingTeamSummary summary = rows.TeamSummaries[index];
                for (int prior = 0; prior < index; prior++)
                {
                    if (rows.TeamSummaries[prior].Team == summary.Team)
                        return false;
                }

                int count = 0;
                int minHp = int.MaxValue;
                int minCount = 0;
                int secondMinHp = int.MaxValue;
                for (int slot = 0; slot < rows.Capacity; slot++)
                {
                    if (!IsLivingCharacter(rows, slot) || rows.Team[slot] != summary.Team)
                        continue;
                    count++;
                    int hp = rows.Hp[slot];
                    if (hp < minHp)
                    {
                        secondMinHp = minHp;
                        minHp = hp;
                        minCount = 1;
                    }
                    else if (hp == minHp)
                    {
                        minCount++;
                    }
                    else if (hp < secondMinHp)
                    {
                        secondMinHp = hp;
                    }
                }
                if (count <= 0 || summary.Count != count ||
                    summary.MinHp != minHp || summary.MinCount != minCount ||
                    summary.SecondMinHp != secondMinHp)
                {
                    return false;
                }
                summarizedCharacters += count;
            }
            return summarizedCharacters == totalLivingCharacters;
        }

        private static bool IsGroundRoleMember(AiSensingSnapshot rows, int slot)
        {
            if (!IsIncluded(rows, slot))
                return false;
            int state = rows.State[slot];
            return rows.Hp[slot] > 0 && state != 14 && Abs(rows.Y[slot]) <= 2 &&
                   (rows.DataObjectType[slot] == 0 || state == 3000);
        }

        private static bool IsAirRoleMember(AiSensingSnapshot rows, int slot)
        {
            return IsIncluded(rows, slot) && rows.Hp[slot] > 0 &&
                   (rows.State[slot] == 14 || Abs(rows.Y[slot]) > 2);
        }

        private static bool IsSpecialScanObjectId(int objectId)
        {
            return objectId / 100 == 1 || objectId == 0xC8 ||
                   objectId == 0xD3 || objectId == 0xD4 || objectId == 0xD5;
        }

        private static void ApplySameTeamGuard(
            AiSensingSnapshot rows,
            int selfSlot,
            int sameTeamCount,
            int sameTeamMinHp,
            ref bool force7AGround,
            ref bool guard7A)
        {
            if (sameTeamMinHp < rows.Hp[selfSlot]) force7AGround = false;
            if (sameTeamMinHp < rows.Hp[selfSlot] - 200) guard7A = true;
            if (sameTeamCount == 0) force7AGround = false;
        }

        private static bool IsLivingCharacter(AiSensingSnapshot rows, int slot)
        {
            return IsIncluded(rows, slot) &&
                   rows.DataObjectType[slot] == 0 &&
                   rows.Hp[slot] > 0;
        }

        private static void CaptureHandle(AiSensingSnapshot rows, int slot, ref uint generation, ref int identity)
        {
            if (!IsIncluded(rows, slot)) return;
            generation = rows.Generation[slot];
            identity = rows.Identity[slot];
        }

        private static int PackFlags(bool proximity, bool left, bool right, bool up, bool down,
            bool guard7A, bool guard7B, bool force7A, bool c8, bool post)
        {
            int flags = 0;
            if (proximity) flags |= SpecialProximity;
            if (left) flags |= SpecialLeft;
            if (right) flags |= SpecialRight;
            if (up) flags |= SpecialUp;
            if (down) flags |= SpecialDown;
            if (guard7A) flags |= SpecialGuard7A;
            if (guard7B) flags |= SpecialGuard7B;
            if (force7A) flags |= SpecialForce7AGround;
            if (c8) flags |= SpecialC8ThreatSeen;
            if (post) flags |= SpecialPostSelectionSeen;
            return flags;
        }

        private static int Distance(AiSensingSnapshot rows, int first, int second)
        {
            return Abs(rows.X[second] - rows.X[first]) + Abs(rows.Z[second] - rows.Z[first]);
        }

        private static int Abs(int value) => value < 0 ? -value : value;
        private static int Max(int first, int second) => first > second ? first : second;
    }
}
