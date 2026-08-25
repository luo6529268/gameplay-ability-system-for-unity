namespace NTSD.Simulation
{
    internal readonly struct AiCharacterDecisionContext
    {
        internal AiCharacterDecisionContext(int moveMode, int stageTargetX)
        {
            MoveMode = moveMode;
            StageTargetX = stageTargetX;
        }

        internal int MoveMode { get; }
        internal int StageTargetX { get; }
    }

    internal sealed class AiCharacterDecisionModule
    {
        internal bool TryEvaluatePositions7Through39(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            int nearestTargetDistance,
            bool sameZLane,
            in AiCharacterDecisionContext context,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int matchedPosition,
            out int rowVisits)
        {
            matchedPosition = 0;
            rowVisits = 0;
            if (!IsIncluded(rows, self) || !IsIncluded(rows, target))
                return false;

            if (TryEvaluatePositions7Through16(
                    rows,
                    self,
                    target,
                    targetState,
                    in context,
                    ref input,
                    ref random,
                    out matchedPosition))
            {
                return true;
            }

            int globalScanSlotCount = ResolveGlobalScanSlotCount(rows, self);
            if (TryEvaluatePositions17Through28(
                    rows,
                    self,
                    target,
                    targetState,
                    nearestTargetDistance,
                    globalScanSlotCount,
                    ref input,
                    ref random,
                    out matchedPosition,
                    out _,
                    out int positions17To28RowVisits))
            {
                rowVisits += positions17To28RowVisits;
                return true;
            }
            rowVisits += positions17To28RowVisits;

            if (TryEvaluatePositions29Through37(
                    rows,
                    self,
                    target,
                    nearestTargetDistance,
                    sameZLane,
                    20,
                    100,
                    ref input,
                    ref random,
                    out matchedPosition,
                    out _,
                    out _,
                    out int positions29To37RowVisits))
            {
                rowVisits += positions29To37RowVisits;
                return true;
            }
            rowVisits += positions29To37RowVisits;

            if (TryUpdateOid52GroupPreLabel591(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(38, out matchedPosition);
            }
            if (TryUpdateLabel591Group(rows, self, target, ref input, ref random))
                return Match(39, out matchedPosition);
            return false;
        }

        internal bool TryEvaluatePositions7Through16(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            in AiCharacterDecisionContext context,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int matchedPosition)
        {
            matchedPosition = 0;
            if (!IsIncluded(rows, self) || !IsIncluded(rows, target))
                return false;

            if (TryUpdateOid6(rows, self, target, ref input, ref random))
                return Match(7, out matchedPosition);
            if (TryUpdateOid7FrameKey(rows, self, target, targetState, ref input))
                return Match(8, out matchedPosition);
            if (TryUpdateOid7Close(rows, self, target, targetState, ref input, ref random))
                return Match(9, out matchedPosition);
            if (TryUpdateOid7Midfar(rows, self, target, targetState, ref input, ref random))
                return Match(10, out matchedPosition);
            if (TryUpdateOid7Facing(rows, self, target, targetState, ref input, ref random))
                return Match(11, out matchedPosition);
            if (TryUpdateOid7Frame255(
                    rows,
                    self,
                    target,
                    in context,
                    ref input))
            {
                return Match(12, out matchedPosition);
            }
            if (TryUpdateOid8(rows, self, target, targetState, ref input, ref random))
                return Match(13, out matchedPosition);
            if (TryUpdateOid11First(rows, self, target, ref input, ref random))
                return Match(14, out matchedPosition);

            ApplyOid11Frame290SideEffect(rows, self, target, ref input);
            if (TryUpdateOid11Dua(rows, self, target, targetState, ref input, ref random))
                return Match(16, out matchedPosition);
            return false;
        }

        private bool TryUpdateOid6(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            int oid = rows.ObjectId[self];
            if (oid != 6 && oid != 18)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 100 &&
                dx > 80 &&
                dx < 130 &&
                dz < 30 &&
                random.Rand(10) == 0)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDrj = 3;
                else
                    input.ComboDlj = 3;
                return true;
            }

            if (rows.Pp[self] > 100 &&
                dx < 45 &&
                dz < 5 &&
                random.Rand(3) == 0)
            {
                input.ComboDuj = 3;
                return true;
            }

            if (rows.State[self] == 9 && random.Rand(8) == 0)
            {
                input.KeyDefend = 1;
                input.PrevDefend = 0;
            }

            return false;
        }

        private bool TryUpdateOid7FrameKey(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input)
        {
            if (!IsOid7Group(rows.ObjectId[self]) ||
                rows.Frame[self] <= 267 ||
                rows.Frame[self] >= 283)
            {
                return false;
            }

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            bool facingTarget = IsFacingTarget(rows, self, target);
            if (targetState == 12 ||
                targetState == 11 ||
                dx > 150 ||
                dz > 25 ||
                facingTarget)
            {
                input.KeyAttack = 1;
                input.PrevAttack = 0;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid7Close(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid7Group(rows.ObjectId[self]) ||
                targetState == 18 ||
                targetState == 14 ||
                targetState == 12)
            {
                return false;
            }

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Hp[self] > 70 &&
                rows.Pp[self] > 320 &&
                (dx > 50 || dz > 10) &&
                dx < 85 &&
                rows.Hp[self] > rows.Hp[target] &&
                dz < 35 &&
                random.Rand(5) == 0)
            {
                input.ComboDuj = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid7Midfar(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid7Group(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            bool useLabel221 = false;
            if (targetState != 18 &&
                targetState != 14 &&
                targetState != 12 &&
                rows.Pp[self] > 200 &&
                dx > 100 &&
                dx < 370 &&
                dz < 60 &&
                random.Rand(20) == 0)
            {
                useLabel221 = true;
            }

            if (!useLabel221 && random.Rand(100) == 0 && dx > 240 && dx < 400)
                useLabel221 = true;
            if (!useLabel221)
                return false;

            if (rows.X[target] > rows.X[self])
                input.ComboDrj = 3;
            else
                input.ComboDlj = 3;
            return true;
        }

        private bool TryUpdateOid7Facing(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid7Group(rows.ObjectId[self]) ||
                targetState == 18 ||
                targetState == 14 ||
                targetState == 12)
            {
                return false;
            }

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            bool facingTarget = IsFacingTarget(rows, self, target);
            if (rows.Pp[self] > 200 &&
                dx > 60 &&
                dx < 280 &&
                dz < 60 &&
                random.Rand(15) == 0 &&
                facingTarget)
            {
                input.ComboDdj = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid7Frame255(
            AiSensingSnapshot rows,
            int self,
            int target,
            in AiCharacterDecisionContext context,
            ref AiDecisionInputState input)
        {
            if (!IsOid7Group(rows.ObjectId[self]) ||
                rows.Frame[self] < 255 ||
                rows.Frame[self] > 261)
            {
                return false;
            }

            bool attackReturn = false;
            if (rows.Facing[self] == 0)
            {
                attackReturn = rows.X[self] > rows.X[target] + 120 ||
                               rows.X[self] > context.StageTargetX - 30;
            }
            else if (rows.Facing[self] == 1)
            {
                attackReturn = rows.X[self] < rows.X[target] - 120 ||
                               rows.X[self] < 30;
            }

            attackReturn = attackReturn ||
                           Abs(rows.Z[target] - rows.Z[self]) > 70 ||
                           context.MoveMode == 1;
            if (attackReturn)
            {
                input.KeyAttack = 1;
                input.PrevAttack = 0;
                return true;
            }

            bool inHorizontalFacingLane = IsFacingTarget(rows, self, target);
            if ((!inHorizontalFacingLane && rows.Z[target] < rows.Z[self]) ||
                (inHorizontalFacingLane && rows.Z[target] > rows.Z[self]))
            {
                input.KeyDown = 1;
            }
            else
            {
                input.KeyUp = 1;
            }

            return false;
        }

        private bool TryUpdateOid8(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (rows.ObjectId[self] != 8)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (targetState != 13)
            {
                if (rows.Pp[self] > 200 &&
                    dx < 400 &&
                    dz < 170 &&
                    random.Rand(250) == 0)
                {
                    WriteHorizontalJump(rows, self, target, ref input);
                    return true;
                }

                if (targetState != 14 &&
                    rows.Pp[self] > 200 &&
                    dx > 60 &&
                    dx < 280 &&
                    dz < 65 &&
                    random.Rand(15) == 0)
                {
                    WriteHorizontalJump(rows, self, target, ref input);
                    return true;
                }
            }

            bool facingTarget = IsFacingTarget(rows, self, target);
            if (targetState != 14 &&
                rows.Pp[self] > 320 &&
                (dx > 50 || dz > 7 || targetState == 13) &&
                dx < 125 &&
                dz < 25 &&
                random.Rand(3) == 0 &&
                facingTarget)
            {
                input.ComboDuj = 3;
                return true;
            }

            if (random.Rand(50) == 0 &&
                rows.LinkState[self] == 0 &&
                rows.Pp[self] > 200 &&
                dx > 200 &&
                dz > 50)
            {
                input.ComboDdj = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid11First(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (rows.ObjectId[self] != 11)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 150 &&
                dx < 280 &&
                dz < 30 &&
                random.Rand(10) == 0)
            {
                if (rows.Facing[self] == 0 && rows.X[target] > rows.X[self])
                    input.ComboDda = 3;
                if (IsFacingTarget(rows, self, target))
                    return true;
            }

            return false;
        }

        private void ApplyOid11Frame290SideEffect(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input)
        {
            if (rows.ObjectId[self] == 11 &&
                rows.HitJ[self] == 290 &&
                rows.Y[target] < 0)
            {
                input.PrevDefend = 0;
                input.KeyDefend = 1;
            }
        }

        private bool TryUpdateOid11Dua(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (rows.ObjectId[self] != 11)
                return false;
            if (random.Rand(5) != 0 && targetState != 16 && targetState != 8)
                return false;

            int predictedDx = Abs(
                rows.X[target] + (int)rows.Vx[self] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (predictedDx < 100 &&
                dz < 7 &&
                rows.Pp[self] > 200 &&
                IsFacingTarget(rows, self, target))
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        internal bool TryEvaluatePositions17Through28(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            int nearestTargetDistance,
            int scanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int matchedPosition,
            out int position21SelectedSlot)
        {
            return TryEvaluatePositions17Through28(
                rows,
                self,
                target,
                targetState,
                nearestTargetDistance,
                scanSlotCount,
                ref input,
                ref random,
                out matchedPosition,
                out position21SelectedSlot,
                out _);
        }

        private bool TryEvaluatePositions17Through28(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            int nearestTargetDistance,
            int scanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int matchedPosition,
            out int position21SelectedSlot,
            out int rowVisits)
        {
            matchedPosition = 0;
            position21SelectedSlot = -1;
            rowVisits = 0;
            if (!IsIncluded(rows, self) || !IsIncluded(rows, target))
                return false;

            if (TryUpdateOid10Or1First(rows, self, target, ref input, ref random))
                return Match(17, out matchedPosition);
            if (TryUpdateOid10Or1Frame271(rows, self, target, targetState, ref input))
                return Match(18, out matchedPosition);
            if (TryUpdateOid10Or1PredictedDua(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(19, out matchedPosition);
            }
            if (TryUpdateOid10Or1Midrange(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(20, out matchedPosition);
            }

            position21SelectedSlot = ApplyOid10Or1HpTeamScanSideEffect(
                rows,
                self,
                target,
                scanSlotCount,
                ref input,
                ref random,
                out int position21RowVisits);
            rowVisits += position21RowVisits;
            ApplyOid10Or1HpAdvantageSideEffect(
                rows,
                self,
                target,
                ref input,
                ref random);

            if (TryUpdateOid9Or2PredictedDda(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(23, out matchedPosition);
            }
            if (TryUpdateOid9Or2Midfar(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(24, out matchedPosition);
            }
            if (TryUpdateOid9Or2NearestDua(
                    rows,
                    self,
                    nearestTargetDistance,
                    ref input,
                    ref random))
            {
                return Match(25, out matchedPosition);
            }
            if (TryUpdateOid32Or19Midfar(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(26, out matchedPosition);
            }
            if (TryUpdateOid32Or19Close(rows, self, target, ref input, ref random))
                return Match(27, out matchedPosition);
            if (TryUpdateOid33Or19Or16PredictedDua(
                    rows,
                    self,
                    target,
                    targetState,
                    ref input,
                    ref random))
            {
                return Match(28, out matchedPosition);
            }

            return false;
        }

        private bool TryUpdateOid10Or1First(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid10Or1(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 100 &&
                dx < 280 &&
                dz < 25 &&
                random.Rand(10) == 0)
            {
                if (rows.Facing[self] == 0 && rows.X[target] > rows.X[self])
                    input.ComboDda = 3;
                if (IsFacingTarget(rows, self, target))
                    return true;
            }

            return false;
        }

        private bool TryUpdateOid10Or1Frame271(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input)
        {
            if (!IsOid10Or1(rows.ObjectId[self]))
                return false;
            if (rows.Frame[self] == 271 && rows.Y[target] < 0 && targetState == 12)
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid10Or1PredictedDua(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid10Or1(rows.ObjectId[self]))
                return false;
            if (random.Rand(10) != 0 && targetState != 16 && targetState != 8)
                return false;

            int predictedDx = Abs(rows.X[target] + (int)rows.Vx[self] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (predictedDx < 80 && dz < 7 && IsFacingTarget(rows, self, target))
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid10Or1Midrange(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid10Or1(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] <= 200 || dx <= 60 || dx >= 280 || dz >= 65)
                return false;

            bool useCombo = random.Rand(15) == 0;
            if (!useCombo &&
                random.Rand(4) == 0 &&
                (targetState == 16 ||
                 targetState == 8 ||
                 (targetState == 12 && rows.Y[target] < -40)))
            {
                useCombo = true;
            }
            if (!useCombo)
                return false;

            WriteHorizontalJump(rows, self, target, ref input);
            return true;
        }

        private int ApplyOid10Or1HpTeamScanSideEffect(
            AiSensingSnapshot rows,
            int self,
            int target,
            int scanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int rowVisits)
        {
            rowVisits = 0;
            if (!IsOid10Or1(rows.ObjectId[self]))
                return -1;
            if (rows.Hp[self] >= 250 || rows.Hp[self] >= rows.Hp[target] + 50)
                return -1;
            if (random.Rand(20) != 0 || rows.Pp[self] <= 75)
                return -1;

            int count = scanSlotCount;
            if (count < 0)
                count = 0;
            if (count > rows.Capacity)
                count = rows.Capacity;

            int bestDistance = -1;
            int bestSlot = -1;
            for (int slot = 0; slot < count; slot++)
            {
                rowVisits++;
                if (slot == self ||
                    !rows.Included[slot] ||
                    rows.DataObjectType[slot] != 0 ||
                    rows.Team[slot] != rows.Team[self] ||
                    rows.Hp[slot] <= rows.Hp[target])
                {
                    continue;
                }

                int distance = Abs(rows.Z[slot] - rows.Z[self]) +
                               Abs(rows.X[slot] - rows.X[self]);
                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestSlot = slot;
                }
            }

            if (bestSlot != -1 && bestDistance > 300 && rows.LinkState[self] == 0)
                input.ComboDdj = 3;
            return bestSlot;
        }

        private void ApplyOid10Or1HpAdvantageSideEffect(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (IsOid10Or1(rows.ObjectId[self]) &&
                rows.Hp[self] > rows.Hp[target] &&
                random.Rand(70) == 0 &&
                rows.Pp[self] > 500)
            {
                input.ComboDuj = 3;
            }
        }

        private bool TryUpdateOid9Or2PredictedDda(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid9Or2(rows.ObjectId[self]))
                return false;
            if (random.Rand(10) != 0 && targetState != 16 && targetState != 8)
                return false;

            int predictedDx = Abs(rows.X[target] + (int)rows.Vx[self] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (predictedDx < 120 && dz < 7 && IsFacingTarget(rows, self, target))
            {
                input.ComboDda = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid9Or2Midfar(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid9Or2(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            bool pathA = targetState != 18 &&
                         targetState != 14 &&
                         targetState != 12 &&
                         rows.Pp[self] > 200 &&
                         dx > 75 &&
                         dx < 370 &&
                         dz < 60 &&
                         random.Rand(13) == 0;
            bool pathB = false;
            if (!pathA)
            {
                int modulus = rows.Hp[target] / 4 + 40;
                pathB = random.Rand(modulus) == 0 && dx > 150 && dx < 400;
            }
            if (!pathA && !pathB)
                return false;

            WriteHorizontalJump(rows, self, target, ref input);
            return true;
        }

        private bool TryUpdateOid9Or2NearestDua(
            AiSensingSnapshot rows,
            int self,
            int nearestTargetDistance,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid9Or2(rows.ObjectId[self]))
                return false;
            if (nearestTargetDistance < 10000 &&
                random.Rand(30) == 0 &&
                rows.Pp[self] > 150)
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid32Or19Midfar(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid32Or19(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            bool pathA = targetState != 18 &&
                         targetState != 14 &&
                         targetState != 12 &&
                         rows.Pp[self] > 200 &&
                         dx < 270 &&
                         dz < 60 &&
                         random.Rand(60) == 0;
            bool pathB = false;
            if (!pathA)
            {
                int modulus = rows.Hp[target] / 4 + 40;
                pathB = random.Rand(modulus) == 0 && dx > 150 && dx < 400;
            }
            if (!pathA && !pathB)
                return false;

            WriteHorizontalJump(rows, self, target, ref input);
            return true;
        }

        private bool TryUpdateOid32Or19Close(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid32Or19(rows.ObjectId[self]))
                return false;
            if (Abs(rows.X[target] - rows.X[self]) < 150 &&
                Abs(rows.Z[target] - rows.Z[self]) < 40 &&
                random.Rand(15) == 0)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDra = 3;
                else
                    input.ComboDla = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid33Or19Or16PredictedDua(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            int objectId = rows.ObjectId[self];
            if (objectId != 33 && objectId != 19 && objectId != 16)
                return false;
            if (random.Rand(5) != 0 && targetState != 16 && targetState != 8)
                return false;

            int predictedDx = Abs(rows.X[target] + (int)rows.Vx[self] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (predictedDx < 60 &&
                dz < 7 &&
                rows.Pp[self] > 150 &&
                IsFacingTarget(rows, self, target))
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        internal bool TryEvaluatePositions29Through37(
            AiSensingSnapshot rows,
            int self,
            int target,
            int nearestTargetDistance,
            bool sameZLane,
            int teammateScanSlotCount,
            int teamHelpScanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int matchedPosition,
            out int position30SelectedSlot,
            out int position34SelectedSlot)
        {
            return TryEvaluatePositions29Through37(
                rows,
                self,
                target,
                nearestTargetDistance,
                sameZLane,
                teammateScanSlotCount,
                teamHelpScanSlotCount,
                ref input,
                ref random,
                out matchedPosition,
                out position30SelectedSlot,
                out position34SelectedSlot,
                out _);
        }

        private bool TryEvaluatePositions29Through37(
            AiSensingSnapshot rows,
            int self,
            int target,
            int nearestTargetDistance,
            bool sameZLane,
            int teammateScanSlotCount,
            int teamHelpScanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int matchedPosition,
            out int position30SelectedSlot,
            out int position34SelectedSlot,
            out int rowVisits)
        {
            matchedPosition = 0;
            position30SelectedSlot = -1;
            position34SelectedSlot = -1;
            rowVisits = 0;
            if (!IsIncluded(rows, self) || !IsIncluded(rows, target))
                return false;

            if (TryUpdateOid34GroupLowHpDdj(rows, self, ref input, ref random))
                return Match(29, out matchedPosition);
            if (TryUpdateOid34GroupTeammateGuard(
                    rows,
                    self,
                    nearestTargetDistance,
                    sameZLane,
                    teammateScanSlotCount,
                    ref input,
                    out position30SelectedSlot,
                    out int position30RowVisits))
            {
                rowVisits += position30RowVisits;
                return Match(30, out matchedPosition);
            }
            rowVisits += position30RowVisits;
            if (TryUpdateLabel464Long(rows, self, target, ref input, ref random))
                return Match(31, out matchedPosition);
            if (TryUpdateLabel464CloseDda(rows, self, target, ref input, ref random))
                return Match(32, out matchedPosition);
            if (TryUpdateOid35Long(rows, self, target, ref input, ref random))
                return Match(33, out matchedPosition);
            if (TryUpdateOid36Or16TeamDuj(
                    rows,
                    self,
                    teamHelpScanSlotCount,
                    ref input,
                    ref random,
                    out position34SelectedSlot,
                    out int position34RowVisits))
            {
                rowVisits += position34RowVisits;
                return Match(34, out matchedPosition);
            }
            rowVisits += position34RowVisits;
            if (TryUpdateOid36Or16RangeDua(rows, self, target, ref input, ref random))
                return Match(35, out matchedPosition);
            if (TryUpdateOid38(rows, self, target, ref input, ref random))
                return Match(36, out matchedPosition);
            if (TryUpdateOid39Or10Close(rows, self, target, ref input, ref random))
                return Match(37, out matchedPosition);
            return false;
        }

        private bool TryUpdateOid34GroupLowHpDdj(
            AiSensingSnapshot rows,
            int self,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid34Group(rows.ObjectId[self]))
                return false;
            if (random.Rand(10) != 0 || rows.Pp[self] <= 350)
                return false;

            bool lowHp =
                (rows.Hp[self] < rows.HpMax[self] - 70 && rows.Hp[self] < 140) ||
                (rows.Hp[self] < (3 * rows.HpMax[self]) / 5 && rows.Hp[self] >= 140);
            if (!lowHp)
                return false;

            input.ComboDdj = 3;
            return true;
        }

        private bool TryUpdateOid34GroupTeammateGuard(
            AiSensingSnapshot rows,
            int self,
            int nearestTargetDistance,
            bool sameZLane,
            int scanSlotCount,
            ref AiDecisionInputState input,
            out int selectedSlot,
            out int rowVisits)
        {
            selectedSlot = -1;
            rowVisits = 0;
            if (!IsOid34Group(rows.ObjectId[self]))
                return false;
            if (rows.LinkState[self] != 0 && rows.Frame[self] >= 9)
                return false;

            bool hpWindow =
                (rows.Hp[self] >= rows.HpMax[self] - 70 || rows.Hp[self] >= 140) &&
                (rows.Hp[self] >= (3 * rows.HpMax[self]) / 5 || rows.Hp[self] < 140);
            if (!hpWindow || sameZLane)
                return false;

            int count = ClampScanCount(rows, scanSlotCount);
            for (int slot = 0; slot < count; slot++)
            {
                rowVisits++;
                if (slot == self ||
                    !rows.Included[slot] ||
                    rows.Team[slot] == 0 ||
                    rows.Team[slot] != rows.Team[self] ||
                    Abs(rows.X[slot] - rows.X[self]) >= 250 ||
                    Abs(rows.Z[slot] - rows.Z[self]) >= 60 ||
                    rows.Pp[self] <= 350)
                {
                    continue;
                }

                bool candidateLowHp =
                    (rows.Hp[slot] < rows.HpMax[slot] - 90 && rows.Hp[slot] < 140) ||
                    (rows.Hp[slot] < (3 * rows.HpMax[slot]) / 5 && rows.Hp[slot] >= 140);
                if (!candidateLowHp || rows.Hp[slot] <= 0)
                    continue;

                int candidateDistance = Abs(rows.X[slot] - rows.X[self]) +
                                        Abs(rows.Z[slot] - rows.Z[self]);
                if (candidateDistance >= nearestTargetDistance / 3)
                    continue;

                selectedSlot = slot;
                break;
            }
            if (selectedSlot < 0)
                return false;

            if (rows.X[selectedSlot] <= rows.X[self])
            {
                input.KeyRight = 0;
                input.KeyLeft = 1;
            }
            else
            {
                input.KeyRight = 1;
                input.KeyLeft = 0;
            }

            int teammateDx = Abs(rows.X[selectedSlot] - rows.X[self]);
            bool movementOnly =
                teammateDx >= 5 &&
                ((rows.X[selectedSlot] < rows.X[self] && rows.Facing[self] != 1) ||
                 (rows.X[selectedSlot] > rows.X[self] && rows.Facing[self] == 1));
            if (movementOnly)
                return true;

            input.ComboDuj = 3;
            return true;
        }

        private bool TryUpdateLabel464Long(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsLabel464Group(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (random.Rand(7) != 0 ||
                dx >= 500 ||
                dx <= 90 ||
                dz >= 4 ||
                rows.Pp[self] <= 150)
            {
                return false;
            }

            if (rows.Frame[target] != 263 && rows.Frame[target] != 264)
            {
                if (rows.X[target] <= rows.X[self])
                    input.ComboDla = 3;
                else
                    input.ComboDra = 3;
                return true;
            }

            input.PrevJump = 0;
            input.KeyJump = 1;
            return false;
        }

        private bool TryUpdateLabel464CloseDda(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsLabel464Group(rows.ObjectId[self]))
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (random.Rand(7) == 0 && dx < 100 && dz < 7 && rows.Pp[self] > 75)
            {
                input.ComboDda = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid35Long(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (rows.ObjectId[self] != 35)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (random.Rand(7) != 0 ||
                dx >= 650 ||
                dx <= 40 ||
                dz >= 4 ||
                rows.Pp[self] <= 120)
            {
                return false;
            }

            if (rows.X[target] <= rows.X[self])
                input.ComboDla = 3;
            else
                input.ComboDra = 3;
            return true;
        }

        private bool TryUpdateOid36Or16TeamDuj(
            AiSensingSnapshot rows,
            int self,
            int scanSlotCount,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random,
            out int selectedSlot,
            out int rowVisits)
        {
            selectedSlot = -1;
            rowVisits = 0;
            if (!IsOid36Or16(rows.ObjectId[self]))
                return false;
            if (rows.Pp[self] <= 200 || random.Rand(5) != 0)
                return false;

            int count = ClampScanCount(rows, scanSlotCount);
            for (int slot = 0; slot < count; slot++)
            {
                rowVisits++;
                if (!rows.Included[slot] ||
                    rows.DataObjectType[slot] != 0 ||
                    rows.Team[slot] != rows.Team[self])
                {
                    continue;
                }

                bool needsHelp =
                    rows.Hp[slot] < rows.HpMax[slot] - 200 ||
                    (rows.Hp[slot] < 200 && rows.Hp[slot] < rows.HpMax[slot] - 100);
                if (!needsHelp)
                    continue;

                selectedSlot = slot;
                input.ComboDuj = 3;
                return true;
            }

            return true;
        }

        private bool TryUpdateOid36Or16RangeDua(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (!IsOid36Or16(rows.ObjectId[self]))
                return false;
            if (rows.Pp[self] <= 260 || random.Rand(10) != 0)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (dx < 650 && dz < 240)
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid38(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            if (rows.ObjectId[self] != 38)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 150 &&
                random.Rand(5) == 0 &&
                dx < 250 &&
                dx > 130 &&
                dz < 10)
            {
                WriteHorizontalJump(rows, self, target, ref input);
                return true;
            }
            if (rows.Pp[self] > 200 && random.Rand(10) == 0 && dz < 10)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDra = 3;
                else
                    input.ComboDla = 3;
                return true;
            }
            if (rows.Pp[self] > 200 &&
                random.Rand(10) == 0 &&
                (dx > 200 || dz < 250))
            {
                input.ComboDuj = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid39Or10Close(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            int objectId = rows.ObjectId[self];
            if (objectId != 39 && objectId != 10)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 100 && random.Rand(3) == 0 && dx < 120)
            {
                if (IsFacingTarget(rows, self, target) && dz < 10)
                {
                    input.ComboDda = 3;
                    return true;
                }
            }
            if (rows.Pp[self] > 100 &&
                random.Rand(7) == 0 &&
                dx < 250 &&
                dz < 10)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDra = 3;
                else
                    input.ComboDla = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateOid52GroupPreLabel591(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            int objectId = rows.ObjectId[self];
            if (objectId != 52 && objectId != 1 && objectId != 2 && objectId != 21)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (targetState == 3 &&
                rows.Pp[self] > 125 &&
                random.Rand(10) == 0 &&
                dx < 120 &&
                dz < 10)
            {
                input.ComboDja = 3;
                return true;
            }
            if (rows.Pp[self] > 125 &&
                random.Rand(5) == 0 &&
                dx < 100 &&
                dz < 30)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDuj = 3;
                return true;
            }
            if (rows.Pp[self] > 125 &&
                random.Rand(14) == 0 &&
                dx < 700 &&
                dz < 150)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDra = 3;
                else
                    input.ComboDla = 3;
                return true;
            }
            if (rows.Pp[self] > 125 &&
                random.Rand(5) == 0 &&
                dz < 20)
            {
                WriteHorizontalJump(rows, self, target, ref input);
                return true;
            }

            bool predictedGate = random.Rand(5) == 0 || targetState == 16 || targetState == 8;
            int predictedDx = Abs(rows.X[target] + (int)rows.Vx[self] - rows.X[self]);
            if (predictedGate &&
                predictedDx < 100 &&
                dz < 7 &&
                rows.Pp[self] < 100 &&
                IsFacingTarget(rows, self, target))
            {
                input.ComboDua = 3;
                return true;
            }

            return false;
        }

        private bool TryUpdateLabel591Group(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream random)
        {
            int objectId = rows.ObjectId[self];
            if (objectId != 51 && objectId != 2 && objectId != 18 && objectId != 7)
                return false;

            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Frame[self] > 265 &&
                rows.Frame[self] < 280 &&
                (dz > 13 || rows.DataObjectType[target] != 0))
            {
                input.PrevAttack = 0;
                input.KeyAttack = 1;
                return true;
            }
            if (rows.Pp[self] > 300 && random.Rand(10) == 0 && dx < 300 && dz < 200)
            {
                input.ComboDuj = 3;
                return true;
            }
            if (rows.Pp[self] > 300 && random.Rand(10) == 0 && dx < 950)
            {
                input.ComboDua = 3;
                return true;
            }
            if (random.Rand(5) == 0 &&
                rows.Pp[self] > 250 &&
                dx < 1200 &&
                dx > 40 &&
                dz < 13)
            {
                WriteHorizontalJump(rows, self, target, ref input);
                return true;
            }

            return false;
        }

        private bool IsIncluded(AiSensingSnapshot rows, int slot)
        {
            return rows != null &&
                   slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot];
        }

        private bool IsOid7Group(int objectId)
        {
            return objectId == 7 || objectId == 4 || objectId == 10;
        }

        private bool IsOid10Or1(int objectId)
        {
            return objectId == 10 || objectId == 1;
        }

        private bool IsOid9Or2(int objectId)
        {
            return objectId == 9 || objectId == 2;
        }

        private bool IsOid32Or19(int objectId)
        {
            return objectId == 32 || objectId == 19;
        }

        private bool IsOid34Group(int objectId)
        {
            return objectId == 34 || objectId == 10 || objectId == 5 || objectId == 14;
        }

        private bool IsLabel464Group(int objectId)
        {
            return objectId == 50 ||
                   objectId == 4 ||
                   objectId == 18 ||
                   objectId == 7 ||
                   objectId == 21 ||
                   objectId == 5 ||
                   objectId == 14 ||
                   objectId == 17;
        }

        private bool IsOid36Or16(int objectId)
        {
            return objectId == 36 || objectId == 16;
        }

        private int ClampScanCount(AiSensingSnapshot rows, int scanSlotCount)
        {
            if (scanSlotCount < 0)
                return 0;
            return scanSlotCount < rows.Capacity ? scanSlotCount : rows.Capacity;
        }

        private int ResolveGlobalScanSlotCount(AiSensingSnapshot rows, int self)
        {
            if (self >= 400)
                return rows.Capacity;
            return rows.Capacity < 400 ? rows.Capacity : 400;
        }

        private bool IsFacingTarget(
            AiSensingSnapshot rows,
            int self,
            int target)
        {
            return (rows.Facing[self] == 0 && rows.X[self] < rows.X[target]) ||
                   (rows.Facing[self] == 1 && rows.X[self] > rows.X[target]);
        }

        private void WriteHorizontalJump(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input)
        {
            if (rows.X[target] > rows.X[self])
                input.ComboDrj = 3;
            else
                input.ComboDlj = 3;
        }

        private bool Match(int position, out int matchedPosition)
        {
            matchedPosition = position;
            return true;
        }

        private int Abs(int value)
        {
            return value < 0 ? -value : value;
        }
    }
}
