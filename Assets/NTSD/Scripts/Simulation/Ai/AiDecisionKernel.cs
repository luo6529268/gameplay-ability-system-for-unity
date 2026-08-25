using System;

namespace NTSD.Simulation
{
    public static class AiDecisionKernel
    {
        private struct Context
        {
            public int Difficulty;
            public int Rand3;
            public int Rand5;
            public int Rand15;
            public int Rand20;
            public int MoveMode;
            public int StageTargetX;
            public int InputPhase;
        }

        public static bool TryEvaluate(
            AiDecisionSnapshot snapshot,
            ref AiDecisionWitness witness)
        {
            return TryEvaluate(snapshot, AiDecisionEvaluationPolicy.FullScan, ref witness);
        }

        public static bool TryEvaluate(
            AiDecisionSnapshot snapshot,
            AiDecisionEvaluationPolicy policy,
            ref AiDecisionWitness witness)
        {
            return TryEvaluate(snapshot, policy, true, ref witness);
        }

        public static bool TryEvaluate(
            AiDecisionSnapshot snapshot,
            AiDecisionEvaluationPolicy policy,
            bool captureRngTrace,
            ref AiDecisionWitness witness)
        {
            return TryEvaluate(
                snapshot,
                policy,
                captureRngTrace,
                null,
                ref witness);
        }

        internal static bool TryEvaluate(
            AiDecisionSnapshot snapshot,
            AiDecisionEvaluationPolicy policy,
            bool captureRngTrace,
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiDecisionWitness witness)
        {
            if (snapshot == null)
            {
                witness = default;
                witness.Availability = AiDecisionAvailability.SnapshotMissing;
                return false;
            }

            return TryEvaluateCore(
                snapshot,
                in snapshot.Input,
                policy,
                captureRngTrace,
                diagnostics,
                ref witness);
        }

        internal static bool TryEvaluateCanonicalInput(
            AiDecisionSnapshot snapshot,
            in AiDecisionInputState canonicalInput,
            AiDecisionEvaluationPolicy policy,
            bool captureRngTrace,
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiDecisionWitness witness)
        {
            return TryEvaluateCore(
                snapshot,
                in canonicalInput,
                policy,
                captureRngTrace,
                diagnostics,
                ref witness);
        }

        private static bool TryEvaluateCore(
            AiDecisionSnapshot snapshot,
            in AiDecisionInputState ownedInput,
            AiDecisionEvaluationPolicy policy,
            bool captureRngTrace,
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiDecisionWitness witness)
        {
            witness = default;
            if (snapshot == null || snapshot.Rows == null)
            {
                witness.Availability = AiDecisionAvailability.SnapshotMissing;
                return false;
            }

            AiSensingSnapshot rows = snapshot.Rows;
            witness.SelfSlot = snapshot.SelfSlot;
            witness.SelfGeneration = snapshot.SelfGeneration;
            witness.SelfStableId = snapshot.SelfStableId;
            witness.OccupancyEpoch = snapshot.OccupancyEpoch;
            snapshot.RngTraceCount = 0;
            snapshot.RngTraceOverflow = false;

            int self = snapshot.SelfSlot;
            if (self < 0 || self >= rows.Capacity)
            {
                return RejectBeforeEvaluation(
                    snapshot,
                    in ownedInput,
                    AiDecisionAvailability.SelfSlotInvalid,
                    ref witness);
            }
            if (!rows.Included[self])
            {
                return RejectBeforeEvaluation(
                    snapshot,
                    in ownedInput,
                    AiDecisionAvailability.SelfNotIncluded,
                    ref witness);
            }
            if (snapshot.OccupancyEpoch != rows.CapturedOccupancyEpoch)
            {
                return RejectBeforeEvaluation(
                    snapshot,
                    in ownedInput,
                    AiDecisionAvailability.EpochMismatch,
                    ref witness);
            }
            if (snapshot.SelfGeneration == 0 ||
                rows.Generation[self] != snapshot.SelfGeneration)
            {
                return RejectBeforeEvaluation(
                    snapshot,
                    in ownedInput,
                    AiDecisionAvailability.GenerationMismatch,
                    ref witness);
            }
            if (rows.Identity[self] != snapshot.SelfStableId)
            {
                return RejectBeforeEvaluation(
                    snapshot,
                    in ownedInput,
                    AiDecisionAvailability.StableIdMismatch,
                    ref witness);
            }
            if (policy == AiDecisionEvaluationPolicy.Indexed &&
                !AiSensingKernel.AreIndexesReady(rows))
            {
                return RejectBeforeEvaluation(
                    snapshot,
                    in ownedInput,
                    AiDecisionAvailability.IndexesNotReady,
                    ref witness);
            }

            witness.Availability = AiDecisionAvailability.Available;
            AiDecisionInputState input = ownedInput;
            AiDecisionWorldState world = snapshot.World;
            var rng = new AiDecisionRandomStream(
                snapshot.RngState,
                snapshot.RngCalls,
                captureRngTrace,
                snapshot.RngTraceModuli,
                snapshot.RngTraceRaw,
                snapshot.RngTraceValues);

            // Alignment contract R3-AI-LIFE-001: C++ prepare_ai_input has no
            // self-HP early return; later death/respawn passes own that lifecycle.
            if (input.Unk3FC > -1000)
            {
                Context coordinate = CreateCoordinateContext(world);
                RollAndClear(ref input);
                MoveTowardCoordinate(rows, self, coordinate, ref input, ref rng);
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.Coordinate;
                Publish(ref witness, input, world, rng);
                return true;
            }

            Context ai = CreateContext(rows, self, ref world, ref witness);
            AiSensingNearestResult nearest;
            bool measureIndexedSensing =
                policy == AiDecisionEvaluationPolicy.Indexed &&
                diagnostics != null &&
                diagnostics.Enabled;
            bool nearestAvailable;
            if (measureIndexedSensing)
            {
                diagnostics.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalNearestSearch);
                try
                {
                    nearestAvailable = AiSensingKernel.TryFindNearest(
                        rows,
                        self,
                        ai.InputPhase,
                        policy,
                        out nearest);
                }
                finally
                {
                    diagnostics.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalNearestSearch);
                }
                diagnostics.RecordPhaseCall(
                    BattleAiInputDetailPhase.IndexedCanonicalNearestSearch);
                diagnostics.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.IndexedCanonicalNearestSearch,
                    nearest.GroundRowVisits + nearest.AirRowVisits);
            }
            else
            {
                nearestAvailable = AiSensingKernel.TryFindNearest(
                    rows,
                    self,
                    ai.InputPhase,
                    policy,
                    out nearest);
            }
            if (!nearestAvailable)
            {
                witness.Availability = AiDecisionAvailability.SelfNotIncluded;
                Publish(ref witness, input, world, rng);
                return false;
            }

            witness.RowVisits += nearest.GroundRowVisits + nearest.AirRowVisits;
            int selected = nearest.SelectedSlot;
            int bestDistance = nearest.BestDist;
            bool sameZLane = nearest.SameZLane;
            witness.InitialSelectedSlot = selected;
            witness.InitialBestDistance = bestDistance;

            int savedTarget = input.Unk360;
            witness.RowVisits++;
            if (IsLivingCharacter(rows, savedTarget))
            {
                if (rng.Rand(30) > 0)
                    selected = savedTarget;
                else
                    input.Unk360 = selected;
            }
            else
            {
                input.Unk360 = selected;
            }
            witness.CachedSelectedSlot = selected;

            if (selected < 0)
            {
                RollAndClear(ref input);
                PostNoTargetFallback(rows, self, savedTarget, ai, ref input);
                ApplyInputEdges(ref input);
                witness.FinalSelectedSlot = selected;
                witness.Exit = AiDecisionExit.NoTarget;
                Publish(ref witness, input, world, rng);
                return true;
            }

            bool specialProximity = false;
            bool specialLeft = false;
            bool specialRight = false;
            bool specialUp = false;
            bool specialDown = false;
            int specialBestDistance = 10000;
            int specialFlags = 0;
            AiSensingSpecialResult special;
            bool specialAvailable;
            if (measureIndexedSensing)
            {
                diagnostics.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalSpecialSearch);
                try
                {
                    specialAvailable = AiSensingKernel.TryScanSpecial(
                        rows,
                        self,
                        ai.InputPhase,
                        selected,
                        bestDistance,
                        sameZLane,
                        policy,
                        out special);
                }
                finally
                {
                    diagnostics.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalSpecialSearch);
                }
                diagnostics.RecordPhaseCall(
                    BattleAiInputDetailPhase.IndexedCanonicalSpecialSearch);
                diagnostics.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.IndexedCanonicalSpecialSearch,
                    special.SlotVisits);
            }
            else
            {
                specialAvailable = AiSensingKernel.TryScanSpecial(
                    rows,
                    self,
                    ai.InputPhase,
                    selected,
                    bestDistance,
                    sameZLane,
                    policy,
                    out special);
            }
            if (specialAvailable)
            {
                selected = special.SelectedSlot;
                specialBestDistance = special.BestDist;
                specialFlags = special.Flags;
                specialProximity = (specialFlags & AiSensingKernel.SpecialProximity) != 0;
                specialLeft = (specialFlags & AiSensingKernel.SpecialLeft) != 0;
                specialRight = (specialFlags & AiSensingKernel.SpecialRight) != 0;
                specialUp = (specialFlags & AiSensingKernel.SpecialUp) != 0;
                specialDown = (specialFlags & AiSensingKernel.SpecialDown) != 0;
                witness.RowVisits += special.SlotVisits;
            }
            else if (policy == AiDecisionEvaluationPolicy.Indexed)
            {
                witness.Availability = AiDecisionAvailability.IndexesNotReady;
                Publish(ref witness, input, world, rng);
                return false;
            }

            witness.SpecialBestDistance = specialBestDistance;
            witness.SpecialFlags = specialFlags;
            witness.FinalSelectedSlot = selected;
            input.Unk360 = selected;
            RollAndClear(ref input);
            witness.RowVisits++;
            if (!IsIncluded(rows, selected))
            {
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.TargetMissing;
                Publish(ref witness, input, world, rng);
                return true;
            }

            int target = selected;
            int selfState = rows.State[self];
            int targetState = rows.State[target];
            int targetOid = rows.ObjectId[target];

            if (rows.X[target] > rows.X[self] && rows.Facing[self] == 1)
                input.KeyRight = 1;
            if (rows.X[target] < rows.X[self] && rows.Facing[self] == 0)
                input.KeyLeft = 1;
            if (selfState == 2)
            {
                if (rows.Facing[self] == 1)
                    input.KeyRight = 1;
                else
                    input.KeyLeft = 1;
            }

            if (rng.Rand(ai.Rand5 + 8) == 0 && input.HasBoundaryBlock)
            {
                input.PrevJump = 0;
                input.KeyJump = 1;
            }

            if (PreUpdateTarget3000(rows, self, target, selfState, targetState, ai, ref input, ref rng))
            {
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.TargetState3000;
                Publish(ref witness, input, world, rng);
                return true;
            }

            if (input.HasInputHistoryGate && rows.LinkState[self] > 0)
            {
                int held = rows.TargetSlot[self];
                witness.RowVisits++;
                if (IsIncluded(rows, held) &&
                    (rows.ObjectId[held] == 0x7A || rows.ObjectId[held] == 0x7B))
                {
                    input.PrevJump = 0;
                    input.KeyJump = 1;
                    ApplyInputEdges(ref input);
                    witness.Exit = AiDecisionExit.HeldSpecial;
                    Publish(ref witness, input, world, rng);
                    return true;
                }
            }

            bool coordinateAllowsSpecial =
                !input.HasInputHistoryGate ||
                PostCacheCoordinateAllowsSpecial(rows, self, ref input);
            if (coordinateAllowsSpecial && (targetState == 0x3EC || targetState == 0x7D4))
            {
                if (input.HasInputHistoryGate &&
                    (Abs(rows.Z[self] - rows.Z[target]) > 150 ||
                     Abs(rows.X[self] - rows.X[target]) > 240) &&
                    targetOid != 0x7A &&
                    targetOid != 0x7B)
                {
                    ApplyInputEdges(ref input);
                    witness.Exit = AiDecisionExit.SpecialTarget;
                    Publish(ref witness, input, world, rng);
                    return true;
                }

                MoveTowardTarget(rows, self, target, ai, selfState, ref input, ref rng);
                if (Abs(rows.Z[target] - rows.Z[self]) <= 3 &&
                    Abs(rows.X[target] - rows.X[self]) <= 6)
                {
                    input.PrevJump = 0;
                    input.KeyJump = 1;
                }
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.SpecialTarget;
                Publish(ref witness, input, world, rng);
                return true;
            }

            if (targetState == 14 || Abs(rows.Y[target]) > 2)
            {
                if (rows.X[target] > ai.StageTargetX - 30)
                {
                    input.KeyLeft = 1;
                    input.PrevLeft = 0;
                    ApplyInputEdges(ref input);
                    witness.Exit = AiDecisionExit.AirTarget;
                    Publish(ref witness, input, world, rng);
                    return true;
                }
                if (rows.X[target] < 30)
                {
                    input.KeyRight = 1;
                    input.PrevRight = 0;
                    ApplyInputEdges(ref input);
                    witness.Exit = AiDecisionExit.AirTarget;
                    Publish(ref witness, input, world, rng);
                    return true;
                }
                if (Abs(rows.Z[target] - rows.Z[self]) <= 45 ||
                    Abs(rows.X[target] - rows.X[self]) <= 350)
                {
                    if (rows.X[target] > rows.X[self])
                    {
                        input.KeyLeft = 1;
                        if (rng.Rand(ai.Rand20 + 35) == 0)
                            input.PrevLeft = 0;
                    }
                    else
                    {
                        input.KeyRight = 1;
                        if (rng.Rand(ai.Rand20 + 35) == 0)
                            input.PrevRight = 0;
                    }
                    if (rows.Z[target] < rows.Z[self] ||
                        rows.Z[target] < world.StageZMin + 10)
                        input.KeyDown = 1;
                    else
                        input.KeyUp = 1;
                }
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.AirTarget;
                Publish(ref witness, input, world, rng);
                return true;
            }

            bool c8Allowed =
                (input.HasInputHistoryGate &&
                 (Abs(rows.Z[self] - rows.Z[target]) > 150 ||
                  Abs(rows.X[self] - rows.X[target]) > 240)) ||
                (targetState != 14 && Abs(rows.Y[target]) <= 2);
            if (c8Allowed && targetOid == 0xC8)
            {
                if (rows.X[target] > rows.X[self] + 7)
                    input.KeyRight = 1;
                else if (rows.X[target] < rows.X[self] - 7)
                    input.KeyLeft = 1;
                if (rows.Z[target] > rows.Z[self] + 2)
                    input.KeyDown = 1;
                else if (rows.Z[target] < rows.Z[self] - 2)
                    input.KeyUp = 1;
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.C8Target;
                Publish(ref witness, input, world, rng);
                return true;
            }

            if (rng.Rand(ai.Rand5 + 1) == 0)
            {
                int characterDecisionPosition = 0;
                if (UpdateFirstDecision(
                        rows,
                        self,
                        target,
                        bestDistance,
                        specialProximity,
                        ref input,
                        ref rng))
                {
                    characterDecisionPosition = 1;
                }
                else if (UpdateTeammateGuardDecision(
                             rows,
                             self,
                             bestDistance,
                             sameZLane,
                             ref input,
                             ref witness))
                {
                    characterDecisionPosition = 2;
                }
                else if (UpdateOid1ComboDecision(
                             rows,
                             self,
                             target,
                             targetState,
                             ref input,
                             ref rng))
                {
                    characterDecisionPosition = 3;
                }
                else if (UpdateCloseOid1Decision(rows, self, target, ref input, ref rng))
                {
                    characterDecisionPosition = 4;
                }
                else if (UpdateOid4ComboDecision(rows, self, target, ref input, ref rng))
                {
                    characterDecisionPosition = 5;
                }
                else if (UpdateOid5ComboDecision(rows, self, target, ref input, ref rng))
                {
                    characterDecisionPosition = 6;
                }
                else
                {
                    var characterContext = new AiCharacterDecisionContext(
                        ai.MoveMode,
                        ai.StageTargetX);
                    if (snapshot.CharacterDecisionModule.TryEvaluatePositions7Through39(
                            rows,
                            self,
                            target,
                            targetState,
                            bestDistance,
                            sameZLane,
                            in characterContext,
                            ref input,
                            ref rng,
                            out int matchedPosition,
                            out int characterRowVisits))
                    {
                        characterDecisionPosition = matchedPosition;
                    }
                    witness.RowVisits += characterRowVisits;
                }

                if (characterDecisionPosition != 0)
                {
                    witness.CharacterDecisionPosition = characterDecisionPosition;
                    ApplyInputEdges(ref input);
                    witness.Exit = AiDecisionExit.FirstDecision;
                    Publish(ref witness, input, world, rng);
                    return true;
                }
            }

            bool closeOrFree =
                !input.HasInputHistoryGate ||
                (Abs(rows.Z[self] - rows.Z[target]) <= 150 &&
                 Abs(rows.X[self] - rows.X[target]) <= 240);
            int selfOid = rows.ObjectId[self];
            bool widePath = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if (!widePath)
            {
                bool targetPressure =
                    rows.Hp[target] > rows.Hp[self] * 2 ||
                    (rows.Hp[self] <= 100 && rows.Hp3[self] > 100);
                widePath = targetPressure &&
                           ai.InputPhase == 1 &&
                           rows.DataObjectType[target] == 0 &&
                           self >= 20 &&
                           rows.Team[self] != 5;
            }

            if (closeOrFree)
            {
                if ((specialRight || ai.MoveMode == 1) &&
                    selfState == 2 &&
                    rows.Facing[self] == 0)
                    input.KeyLeft = 1;
                if (specialLeft && selfState == 2 && rows.Facing[self] == 1)
                    input.KeyRight = 1;
                int threshold = widePath ? 170 : 60;
                int near = widePath ? 150 : 0;
                if (selfState != 19)
                {
                    if ((rows.X[target] > rows.X[self] + threshold ||
                         ((rows.X[target] > rows.X[self] + near ||
                           (selfState == 7 && rows.X[target] > rows.X[self])) &&
                          rows.Facing[self] == 1)) &&
                        !specialRight &&
                        ((widePath && ai.MoveMode == 0) ||
                         (!widePath && (ai.MoveMode == 0 || rows.Facing[self] == 1))))
                    {
                        input.KeyRight = 1;
                        if (rng.Rand(ai.Rand20 + 35) == 0)
                            input.PrevRight = 0;
                    }
                    if ((rows.X[target] < rows.X[self] - threshold ||
                         ((rows.X[target] < rows.X[self] - near ||
                           (selfState == 7 && rows.X[target] < rows.X[self])) &&
                          rows.Facing[self] == 0)) &&
                        !specialLeft)
                    {
                        input.KeyLeft = 1;
                        if (rng.Rand(ai.Rand20 + 35) == 0)
                            input.PrevLeft = 0;
                    }
                    if (((rows.Z[target] > rows.Z[self] + 3 && !specialProximity) ||
                         ((specialRight || specialLeft) && specialUp)) &&
                        !specialDown)
                        input.KeyDown = 1;
                    if (((rows.Z[target] < rows.Z[self] - 3 && !specialProximity) ||
                         ((specialRight || specialLeft) && specialDown)) &&
                        !specialUp)
                        input.KeyUp = 1;
                }
            }

            if (rows.LinkState[self] > 0 &&
                !ProcessHeld(rows, self, target, ai, selfState, targetState,
                    sameZLane, specialProximity, world, ref input, ref rng, ref witness))
            {
                ApplyInputEdges(ref input);
                witness.Exit = AiDecisionExit.HeldDecision;
                Publish(ref witness, input, world, rng);
                return true;
            }

            if (rng.Rand(ai.Difficulty * 7 + 10) == 0 &&
                (targetState == 3 || targetState / 100 == 3) &&
                Abs(rows.Z[target] - rows.Z[self]) < 9 &&
                ((rows.Facing[target] == 0 && rows.X[target] < rows.X[self]) ||
                 (rows.Facing[target] == 1 && rows.X[target] > rows.X[self])))
                input.KeyAttack = 1;
            if (closeOrFree &&
                rng.Rand(2 * (ai.Rand5 + 10)) < 3 &&
                rng.Rand(20) < 3 &&
                targetState != 14)
                input.KeyDefend = 1;
            bool selfGroup = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if ((!selfGroup || targetState == 16) &&
                Abs(rows.X[target] - 2 * (int)rows.Vx[self] - rows.X[self]) < 50 &&
                Abs(rows.Z[target] - rows.Z[self]) < 5 &&
                rng.Rand(ai.Rand3 + 3) == 0 &&
                targetState != 14)
                input.KeyJump = 1;

            ProcessSubCallerPrewrite(rows, self, target, ai, selfState, targetState,
                ref input, ref rng);
            ProcessSubPressurePrewrite(rows, self, target, ai, selfState, targetState,
                ref input, ref rng);
            ProcessSubHelper(rows, self, target, ai, targetState, specialLeft, specialRight,
                ref input, ref rng);
            ApplyInputEdges(ref input);
            witness.Exit = AiDecisionExit.Complete;
            Publish(ref witness, input, world, rng);
            return true;
        }

        public static void ApplyInputEdges(ref AiDecisionInputState input)
        {
            if (input.PrevRight == 0 && input.KeyRight == 1)
            {
                input.CdRight = 5;
                PushHistory(ref input, 6);
            }
            if (input.PrevLeft == 0 && input.KeyLeft == 1)
            {
                input.CdLeft = 5;
                PushHistory(ref input, 4);
            }
            if (input.PrevUp == 0 && input.KeyUp == 1)
            {
                input.CdUp = 5;
                PushHistory(ref input, 8);
            }
            if (input.PrevDown == 0 && input.KeyDown == 1)
            {
                input.CdDown = 5;
                PushHistory(ref input, 2);
            }
            if (input.PrevAttack == 0 && input.KeyAttack == 1)
            {
                input.CdDefend = 5;
                PushHistory(ref input, 9);
            }
            if (input.PrevDefend == 0 && input.KeyDefend == 1)
            {
                input.CdJump = 5;
                PushHistory(ref input, 0);
            }
            if (input.PrevJump == 0 && input.KeyJump == 1)
            {
                input.CdAttack = 5;
                PushHistory(ref input, 5);
            }
        }

        private static Context CreateCoordinateContext(in AiDecisionWorldState world)
        {
            return new Context
            {
                Difficulty = world.FlowAiDifficulty,
                Rand3 = world.FlowRand3,
                Rand5 = world.FlowRand5,
                Rand15 = world.FlowRand15,
                Rand20 = world.FlowRand20,
                MoveMode = world.FlowMoveMode,
                StageTargetX = world.FlowStageTargetX,
                InputPhase = world.InputPhase,
            };
        }

        private static Context CreateContext(
            AiSensingSnapshot rows,
            int self,
            ref AiDecisionWorldState world,
            ref AiDecisionWitness witness)
        {
            int difficulty = world.Difficulty;
            bool forceZero = world.AiPhaseGate == 1;
            if (!forceZero && world.InputPhase == 1 && rows.Team[self] != 5)
                forceZero = self < 20 || rows.ObjectId[self] < 30;
            if (forceZero || difficulty < 0)
                difficulty = 0;
            var ai = new Context
            {
                Difficulty = difficulty,
                Rand3 = difficulty * 3,
                Rand5 = difficulty * 5,
                Rand15 = difficulty * 15,
                Rand20 = difficulty * 20,
                InputPhase = world.InputPhase,
                StageTargetX = world.StageTargetX,
            };
            UpdateMoveMode(rows, self, ref ai, ref witness);
            world.FlowAiDifficulty = ai.Difficulty;
            world.FlowRand3 = ai.Rand3;
            world.FlowRand5 = ai.Rand5;
            world.FlowRand15 = ai.Rand15;
            world.FlowRand20 = ai.Rand20;
            world.FlowMoveMode = ai.MoveMode;
            world.FlowStageTargetX = ai.StageTargetX;
            return ai;
        }

        private static void UpdateMoveMode(
            AiSensingSnapshot rows,
            int self,
            ref Context ai,
            ref AiDecisionWitness witness)
        {
            if (ai.InputPhase != 1 || rows.Team[self] == 5)
                return;
            int rightmostX = -1;
            int rightmostZ = 0;
            int count = Math.Min(10, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                witness.RowVisits++;
                if (slot == self ||
                    !rows.Included[slot] ||
                    rows.DataObjectType[slot] != 0 ||
                    rows.Hp[slot] <= 0)
                    continue;
                if (rows.X[slot] > rightmostX)
                {
                    rightmostX = rows.X[slot];
                    rightmostZ = rows.Z[slot];
                }
            }
            if (rightmostX < 0)
                return;
            if (rows.X[self] > rightmostX &&
                rows.X[self] + Abs(rows.Z[self] - rightmostZ) / 2 - rightmostX > 200)
                ai.MoveMode = 1;
            if (rows.X[self] > rightmostX + 400)
                ai.MoveMode = 2;
        }

        private static void PostNoTargetFallback(
            AiSensingSnapshot rows,
            int self,
            int savedTarget,
            Context ai,
            ref AiDecisionInputState input)
        {
            if (IsIncluded(rows, savedTarget))
            {
                bool close = !input.HasInputHistoryGate ||
                             (Abs(rows.Z[self] - rows.Z[savedTarget]) <= 150 &&
                              Abs(rows.X[self] - rows.X[savedTarget]) <= 240);
                if (close && ai.MoveMode == 1)
                    input.KeyLeft = 1;
            }
            int oid = rows.ObjectId[self];
            int frame = rows.Frame[self];
            if ((oid == 7 && frame >= 255 && frame <= 261) ||
                (oid == 9 && frame >= 280 && frame <= 290) ||
                (oid == 32 && frame >= 240 && frame <= 245))
                input.KeyAttack = 1;
        }

        private static void RollAndClear(ref AiDecisionInputState input)
        {
            input.PrevUp = input.KeyUp;
            input.PrevDown = input.KeyDown;
            input.PrevLeft = input.KeyLeft;
            input.PrevRight = input.KeyRight;
            input.PrevJump = input.KeyJump;
            input.PrevDefend = input.KeyDefend;
            input.PrevAttack = input.KeyAttack;
            input.KeyUp = 0;
            input.KeyDown = 0;
            input.KeyLeft = 0;
            input.KeyRight = 0;
            input.KeyAttack = 0;
            input.KeyJump = 0;
            input.KeyDefend = 0;
        }

        private static void MoveTowardCoordinate(
            AiSensingSnapshot rows,
            int self,
            Context ai,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            if (input.Unk3FC <= -1000 || input.Unk400 <= -1000)
                return;
            if (rows.X[self] > input.Unk3FC + 6)
            {
                input.KeyLeft = 1;
                if (rows.X[self] > input.Unk3FC + 250 && rng.Rand(ai.Rand3 + 3) == 0)
                    input.PrevLeft = 0;
                if (rows.X[self] < input.Unk3FC + 100 && rows.State[self] == 2 && rows.Facing[self] == 1)
                    input.KeyRight = 1;
            }
            else if (rows.X[self] < input.Unk3FC - 6)
            {
                input.KeyRight = 1;
                if (rows.X[self] < input.Unk3FC - 250 && rng.Rand(ai.Rand3 + 3) == 0)
                    input.PrevRight = 0;
                if (rows.X[self] > input.Unk3FC - 100 && rows.State[self] == 2 && rows.Facing[self] == 0)
                    input.KeyLeft = 1;
            }
            if (rows.Z[self] < input.Unk400 - 3)
                input.KeyDown = 1;
            else if (rows.Z[self] > input.Unk400 + 3)
                input.KeyUp = 1;
            if ((input.BoundaryFlags & 3) != 0)
            {
                input.PrevJump = 0;
                input.KeyJump = 1;
            }
            if (Abs(input.Unk400 - rows.Z[self]) <= 90 &&
                Abs(input.Unk3FC - rows.X[self]) <= 90)
            {
                input.Unk3FC = -1000;
                input.Unk400 = -1000;
            }
        }

        private static void MoveTowardTarget(
            AiSensingSnapshot rows,
            int self,
            int target,
            Context ai,
            int selfState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            if (rows.X[self] > rows.X[target] + 6)
            {
                input.KeyLeft = 1;
                if (rows.X[self] > rows.X[target] + 250 && rng.Rand(ai.Rand3 + 3) == 0)
                    input.PrevLeft = 0;
                if (rows.X[self] < rows.X[target] + 100 && selfState == 2 && rows.Facing[self] == 1)
                    input.KeyRight = 1;
            }
            else if (rows.X[self] < rows.X[target] - 6)
            {
                if (ai.MoveMode == 0)
                    input.KeyRight = 1;
                if (rows.X[self] < rows.X[target] - 250 &&
                    rng.Rand(ai.Rand3 + 3) == 0 &&
                    ai.MoveMode == 0)
                    input.PrevRight = 0;
                if (rows.X[self] > rows.X[target] - 100 && selfState == 2 && rows.Facing[self] == 0)
                    input.KeyLeft = 1;
            }
            if (rows.Z[self] < rows.Z[target] - 3)
                input.KeyDown = 1;
            else if (rows.Z[self] > rows.Z[target] + 3)
                input.KeyUp = 1;
        }

        private static bool PostCacheCoordinateAllowsSpecial(
            AiSensingSnapshot rows,
            int self,
            ref AiDecisionInputState input)
        {
            if (input.Unk3FC <= -1000)
                return true;
            if (Abs(input.Unk400 - rows.Z[self]) > 90 ||
                Abs(input.Unk3FC - rows.X[self]) > 90)
                return false;
            input.Unk3FC = -1000;
            input.Unk400 = -1000;
            return true;
        }

        private static bool PreUpdateTarget3000(
            AiSensingSnapshot rows,
            int self,
            int target,
            int selfState,
            int targetState,
            Context ai,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            if (targetState != 3000)
                return false;
            bool randomGate = ai.Rand3 <= 0 || rng.Rand(ai.Rand3) == 0;
            if (selfState != 7 && randomGate &&
                ((rows.X[target] > rows.X[self] && rows.X[target] < rows.X[self] + 200 && rows.Vx[target] < 0.0) ||
                 (rows.X[target] < rows.X[self] && rows.X[target] > rows.X[self] - 200 && rows.Vx[target] > 0.0)))
            {
                input.PrevAttack = 0;
                input.KeyAttack = 1;
            }
            if (rows.X[target] > rows.X[self] && rows.Facing[self] == 1)
                input.KeyRight = 1;
            if (rows.X[target] < rows.X[self] && rows.Facing[self] == 0)
                input.KeyLeft = 1;
            return true;
        }

        private static bool UpdateFirstDecision(
            AiSensingSnapshot rows,
            int self,
            int target,
            int nearestTargetDistance,
            bool specialProximity,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            int oid = rows.ObjectId[self];
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21)
                return false;
            if (rng.Rand(10) == 0 &&
                rows.Pp[self] > 85 &&
                ((rows.Hp[self] < rows.HpMax[self] - 70 && rows.Hp[self] < 450) ||
                 (rows.Hp[self] < (3 * rows.HpMax[self]) / 5 && rows.Hp[self] >= 140)))
            {
                input.ComboDdj = 3;
                return true;
            }
            if (nearestTargetDistance < 10000 && rng.Rand(30) == 0 && rows.Pp[self] > 250)
            {
                input.ComboDua = 3;
                return true;
            }
            int targetOid = rows.ObjectId[target];
            bool split =
                targetOid == 2 || targetOid == 9 || targetOid == 10 ||
                targetOid == 11 || targetOid == 33 || targetOid == 34;
            int maxDx = split ? 500 : 250;
            int targetPpMin = split ? 220 : 170;
            if (rng.Rand(15) == 0 &&
                Abs(rows.X[target] - rows.X[self]) > 100 &&
                Abs(rows.X[target] - rows.X[self]) < maxDx &&
                Abs(rows.Z[target] - rows.Z[self]) < 30 &&
                rows.Pp[self] > 100 &&
                rows.Pp[target] > targetPpMin &&
                !specialProximity)
            {
                if (rows.X[target] <= rows.X[self])
                    input.ComboDlj = 3;
                else
                    input.ComboDrj = 3;
                return true;
            }
            return false;
        }

        private static bool UpdateTeammateGuardDecision(
            AiSensingSnapshot rows,
            int self,
            int nearestTargetDistance,
            bool sameZLane,
            ref AiDecisionInputState input,
            ref AiDecisionWitness witness)
        {
            int oid = rows.ObjectId[self];
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21)
                return false;
            if (rows.LinkState[self] != 0 && rows.Frame[self] >= 9)
                return false;
            bool hpWindow =
                (rows.Hp[self] >= rows.HpMax[self] - 70 || rows.Hp[self] >= 140) &&
                (rows.Hp[self] >= (3 * rows.HpMax[self]) / 5 || rows.Hp[self] < 140);
            if (!hpWindow || sameZLane)
                return false;
            int count = Math.Min(20, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                witness.RowVisits++;
                if (!rows.Included[slot] ||
                    slot == self ||
                    rows.Team[slot] == 0 ||
                    rows.Team[slot] != rows.Team[self] ||
                    Abs(rows.X[slot] - rows.X[self]) >= 250 ||
                    Abs(rows.Z[slot] - rows.Z[self]) >= 60 ||
                    rows.Pp[self] <= 350)
                    continue;
                bool lowHp =
                    (rows.Hp[slot] < rows.HpMax[slot] - 90 && rows.Hp[slot] < 140) ||
                    (rows.Hp[slot] < (3 * rows.HpMax[slot]) / 5 && rows.Hp[slot] >= 140);
                int distance = Abs(rows.X[slot] - rows.X[self]) + Abs(rows.Z[slot] - rows.Z[self]);
                if (!lowHp || rows.Hp[slot] <= 0 || distance >= nearestTargetDistance / 3)
                    continue;
                int deltaX = rows.X[slot] - rows.X[self];
                if (deltaX > 0 && rows.Facing[self] == 1 && Abs(deltaX) >= 5)
                {
                    input.KeyRight = 1;
                    input.KeyLeft = 0;
                    return true;
                }
                if (deltaX < 0 && rows.Facing[self] != 1 && Abs(deltaX) >= 5)
                {
                    input.KeyRight = 0;
                    input.KeyLeft = 1;
                    return true;
                }
                input.ComboDuj = 3;
                return true;
            }
            return false;
        }

        private static bool UpdateOid1ComboDecision(
            AiSensingSnapshot rows,
            int self,
            int target,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            int oid = rows.ObjectId[self];
            if (oid != 1 && oid != 21 && oid != 17)
                return false;
            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Frame[self] >= 260 && rows.Frame[self] <= 289 && dx < 100 && dz < 7)
                return false;
            if (rng.Rand(7) == 0 && dx < 150 && dz < 8 && rows.Pp[self] > 150 &&
                ((rng.Rand(10) == 0 && targetState != 3) ||
                 (rng.Rand(3) > 0 && (targetState == 16 || targetState == 8 || targetState == 11))))
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDrj = 3;
                else
                    input.ComboDlj = 3;
                return true;
            }
            if (rng.Rand(7) == 0 && dx < 100 && dz < 7 && rows.Pp[self] > 75)
            {
                if (rows.Pp[self] <= 150 ||
                    ((rng.Rand(10) != 0 || targetState == 3) &&
                     (rng.Rand(3) <= 0 || targetState != 16)))
                {
                    input.ComboDda = 3;
                    return true;
                }
                if (rows.X[target] <= rows.X[self])
                    input.ComboDlj = 3;
                else
                    input.ComboDrj = 3;
                return true;
            }
            return false;
        }

        private static bool UpdateCloseOid1Decision(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            int oid = rows.ObjectId[self];
            if (oid != 1 && oid != 21 && oid != 17)
                return false;
            if (rows.Frame[self] < 260 || rows.Frame[self] > 289 ||
                Abs(rows.X[target] - rows.X[self]) >= 100 ||
                Abs(rows.Z[target] - rows.Z[self]) >= 7)
                return false;
            if ((rows.Y[target] == 0 && rows.Y[self] == 0 && rng.Rand(3) == 0) ||
                (rows.Y[target] < 0 && rows.Y[self] < 0 && rng.Rand(7) == 0))
            {
                input.KeyJump = 1;
                input.PrevJump = 0;
                return true;
            }
            if ((rows.Y[target] >= 0 || rng.Rand(5) != 0) && rng.Rand(30) != 0)
                return true;
            bool targetRight = rows.X[target] > rows.X[self];
            bool targetLeft = rows.X[target] < rows.X[self];
            if ((targetRight && rows.Facing[self] == 0) ||
                (targetLeft && rows.Facing[self] == 1))
                input.KeyDefend = 1;
            input.PrevDefend = 0;
            return true;
        }

        private static bool UpdateOid4ComboDecision(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            int oid = rows.ObjectId[self];
            if (oid != 4 && oid != 10 && oid != 19)
                return false;
            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 360 && dx < 100 && dz < 70 && rng.Rand(rows.Hp[self] / 5 + 10) == 0)
            {
                input.ComboDuj = 3;
                return true;
            }
            if (rng.Rand(45) == 0 && dx > 100 && dx < 550 && dz < 20 && rows.Pp[self] > 170)
            {
                if (rows.X[target] <= rows.X[self])
                    input.ComboDlj = 3;
                else
                    input.ComboDrj = 3;
                return true;
            }
            if (rng.Rand(30) == 0 && rows.Pp[self] > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                bool facing =
                    (rows.Facing[self] == 0 && rows.X[self] < rows.X[target]) ||
                    (rows.Facing[self] == 1 && rows.X[self] > rows.X[target]);
                if (facing)
                {
                    input.ComboDja = 3;
                    return true;
                }
            }
            return false;
        }

        private static bool UpdateOid5ComboDecision(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            int oid = rows.ObjectId[self];
            if (oid != 5 && oid != 19)
                return false;
            int dx = Abs(rows.X[target] - rows.X[self]);
            int dz = Abs(rows.Z[target] - rows.Z[self]);
            if (rows.Pp[self] > 450 && dx > 100 && dz > 50 && rng.Rand(3) == 0)
            {
                if (rng.Rand(2) != 0)
                    input.ComboDdj = 3;
                else
                    input.ComboDuj = 3;
                return true;
            }
            if (rows.Pp[self] > 70 && dx > 100 && dx < 160 && dz < 8 && rng.Rand(10) == 0)
            {
                if (rows.X[target] > rows.X[self])
                    input.ComboDrj = 3;
                else
                    input.ComboDlj = 3;
                return true;
            }
            if (rng.Rand(30) == 0 && rows.Pp[self] > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                if (rows.Facing[self] == 0 && rows.X[self] < rows.X[target])
                {
                    input.ComboDra = 3;
                    return true;
                }
                if (rows.Facing[self] == 1 && rows.X[self] > rows.X[target])
                {
                    input.ComboDla = 3;
                    return true;
                }
            }
            return false;
        }

        private static bool ProcessHeld(
            AiSensingSnapshot rows,
            int self,
            int target,
            Context ai,
            int selfState,
            int targetState,
            bool sameZLane,
            bool specialProximity,
            in AiDecisionWorldState world,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng,
            ref AiDecisionWitness witness)
        {
            if (rng.Rand(ai.Rand3 + 1) > 0)
                return false;
            int heldSlot = rows.TargetSlot[self];
            if (heldSlot < 0 || heldSlot >= rows.Capacity)
                return true;
            int heldOid = IsIncluded(rows, heldSlot) ? rows.ObjectId[heldSlot] : -1;
            bool lineCover = HasHeldLineCover(rows, self, target, ref witness);
            if (selfState == 2 && rng.Rand(ai.Rand3 + 5) == 0)
            {
                if (lineCover)
                    input.KeyDefend = 1;
                else
                    input.KeyJump = 1;
            }

            int vxTwice = 2 * (int)rows.Vx[self];
            if (heldOid == 100 || heldOid == 101 || heldOid == 120 || heldOid == 121 || heldOid == 124)
            {
                if (Abs(rows.X[target] - vxTwice - rows.X[self]) < 10000 &&
                    Abs(rows.Z[target] - rows.Z[self]) < 6 &&
                    rng.Rand(ai.Rand3 + 3) == 0 &&
                    targetState != 14)
                    input.KeyJump = 1;
                if (heldOid == 124 && rng.Rand(ai.Rand15 + 30) == 0)
                    input.KeyJump = 1;
                if (rng.Rand(ai.Rand3 + 5) == 0)
                {
                    bool close =
                        !input.HasInputHistoryGate ||
                        (Abs(rows.Z[self] - rows.Z[target]) <= 150 &&
                         Abs(rows.X[self] - rows.X[target]) <= 240);
                    if (close && Abs(rows.X[target] - rows.X[self]) < 600 &&
                        Abs(rows.Z[target] - rows.Z[self]) < 20)
                    {
                        if (rows.X[target] > rows.X[self] && ai.MoveMode == 0)
                        {
                            input.KeyRight = 1;
                            input.PrevRight = 0;
                        }
                        if (rows.X[target] < rows.X[self])
                        {
                            input.KeyLeft = 1;
                            input.PrevLeft = 0;
                        }
                    }
                }
            }
            if ((heldOid == 150 || heldOid == 151) &&
                !lineCover &&
                Abs(rows.X[target] - vxTwice - rows.X[self]) < 5000 &&
                Abs(rows.Z[target] - rows.Z[self]) < 10 &&
                rng.Rand(ai.Rand5 + 7) == 0 &&
                targetState != 14)
                input.KeyJump = 1;
            if (heldOid != 122 && heldOid != 123)
                return true;

            input.KeyAttack = 0;
            input.KeyJump = 0;
            input.KeyDefend = 0;
            input.KeyUp = 0;
            input.KeyDown = 0;
            input.KeyLeft = 0;
            input.KeyRight = 0;
            if (selfState == 17 && sameZLane && !specialProximity && rows.HitStop[self] != 0)
            {
                input.KeyAttack = 1;
                return false;
            }
            if (input.HasInputHistoryGate &&
                (Abs(rows.Z[self] - rows.Z[target]) > 150 ||
                 Abs(rows.X[self] - rows.X[target]) > 240))
                return false;
            if (rows.Z[target] < world.StageZMin + 30)
                input.KeyDown = 1;
            else if (rows.Z[target] < world.StageZMax - 30)
                input.KeyUp = 1;
            else if (rows.Z[target] > rows.Z[self])
                input.KeyUp = 1;
            else
                input.KeyDown = 1;

            if (rows.X[target] < 400 && rows.X[self] < 200)
            {
                input.KeyRight = 1;
                if (rng.Rand(ai.Rand3 + 7) == 0)
                    input.PrevRight = 0;
                if (rng.Rand(ai.Rand3 + 5) == 0 && selfState == 2)
                    input.KeyDefend = 1;
                return false;
            }
            if (rows.X[target] > ai.StageTargetX - 400 && rows.X[self] > ai.StageTargetX - 200)
            {
                input.KeyLeft = 1;
                if (rng.Rand(ai.Rand3 + 7) == 0)
                    input.PrevLeft = 0;
                if (rng.Rand(ai.Rand3 + 5) == 0 && selfState == 2)
                    input.KeyDefend = 1;
                return false;
            }
            if (Abs(rows.X[target] - rows.X[self]) < 350 &&
                Abs(rows.Z[target] - rows.Z[self]) < 70)
            {
                if (rows.X[target] > rows.X[self])
                {
                    input.KeyLeft = 1;
                    if (rng.Rand(ai.Rand3 + 4) == 0)
                        input.PrevLeft = 0;
                }
                if (rows.X[target] <= rows.X[self])
                {
                    input.KeyRight = 1;
                    if (rng.Rand(ai.Rand3 + 4) == 0)
                        input.PrevRight = 0;
                }
                return false;
            }
            if (selfState == 2)
            {
                if (rows.Facing[self] == 0)
                    input.KeyLeft = 1;
                if (rows.Facing[self] == 1)
                    input.KeyRight = 1;
                return false;
            }
            if (rng.Rand(5) != 0)
                return false;
            if (specialProximity ||
                (rows.ObjectId[self] != 2 && rows.ObjectId[self] != 34) ||
                rows.Pp[self] <= 150 ||
                rng.Rand(ai.Rand3 + 3) <= 0)
            {
                input.KeyJump = 1;
                return false;
            }
            if (rows.X[target] > rows.X[self])
                input.ComboDrj = 3;
            else
                input.ComboDlj = 3;
            return true;
        }

        private static bool HasHeldLineCover(
            AiSensingSnapshot rows,
            int self,
            int target,
            ref AiDecisionWitness witness)
        {
            bool lineCover = false;
            int count = Math.Min(20, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                witness.RowVisits++;
                if (!rows.Included[slot] ||
                    slot == self ||
                    rows.Team[slot] == 0 ||
                    rows.Team[target] != rows.Team[self] ||
                    rows.Hp[slot] <= 0 ||
                    rows.State[slot] == 14 ||
                    Abs(rows.Y[slot]) > 2)
                    continue;
                if (Abs(rows.Z[slot] - rows.Z[self]) < 15 &&
                    ((rows.X[self] < rows.X[slot] && rows.X[slot] < rows.X[target]) ||
                     (rows.X[target] < rows.X[slot] && rows.X[slot] < rows.X[self])))
                    lineCover = true;
            }
            return lineCover;
        }

        private static void ProcessSubCallerPrewrite(
            AiSensingSnapshot rows,
            int self,
            int target,
            Context ai,
            int selfState,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            bool specialOid = IsSpecialOidForSubGate(rows.ObjectId[self]);
            if (rows.LinkState[self] == 0 &&
                targetState == 16 &&
                specialOid &&
                Abs(rows.X[target] - 2 * (int)rows.Vx[self] - rows.X[self]) < 350 &&
                Abs(rows.Z[target] - rows.Z[self]) < 5 &&
                rng.Rand(ai.Rand3 + 3) == 0)
            {
                if ((rows.X[target] > rows.X[self] && rows.Facing[self] == 0) ||
                    (rows.X[target] <= rows.X[self] && rows.Facing[self] == 1))
                    input.KeyJump = 1;
            }
            if (rows.LinkState[self] != 0 || targetState == 16 || !specialOid)
                return;
            bool closeTrigger =
                rows.X[target] - rows.X[self] < 100 &&
                Abs(rows.Z[target] - rows.Z[self]) < 80 &&
                rng.Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger && selfState != 7)
            {
                if (Abs(rows.X[target] - 2 * (int)rows.Vx[self] - rows.X[self]) < 300 &&
                    Abs(rows.Z[target] - rows.Z[self]) < 5 &&
                    rng.Rand(ai.Rand3 + 3) == 0 &&
                    targetState != 14 &&
                    ((rows.X[target] > rows.X[self] && rows.Facing[self] == 0) ||
                     (rows.X[target] <= rows.X[self] && rows.Facing[self] == 1)))
                    input.KeyJump = 1;
            }
            else if (selfState != 7)
            {
                bool closeWindow =
                    !input.HasInputHistoryGate ||
                    (Abs(rows.Z[self] - rows.Z[target]) <= 150 &&
                     Abs(rows.X[self] - rows.X[target]) <= 240);
                ApplyPressureRetreat(rows, self, target, ai, closeWindow, ref input);
                if (closeWindow && rng.Rand(17) == 0)
                    input.KeyDefend = 1;
            }
        }

        private static void ProcessSubPressurePrewrite(
            AiSensingSnapshot rows,
            int self,
            int target,
            Context ai,
            int selfState,
            int targetState,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            bool specialOid = IsSpecialOidForSubGate(rows.ObjectId[self]);
            if (targetState != 16 && specialOid && rows.LinkState[self] == 0)
                return;
            bool pressure =
                rows.Hp[target] > rows.Hp[self] * 2 ||
                (rows.Hp[self] <= 100 && rows.Hp3[self] > 100);
            if (!pressure ||
                ai.InputPhase != 1 ||
                rows.DataObjectType[target] != 0 ||
                self < 20 ||
                rows.Team[self] == 5)
                return;
            bool closeTrigger =
                rows.X[target] - rows.X[self] < 100 &&
                Abs(rows.Z[target] - rows.Z[self]) < 80 &&
                rng.Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger || selfState == 7)
                return;
            bool closeWindow =
                !input.HasInputHistoryGate ||
                (Abs(rows.Z[self] - rows.Z[target]) <= 150 &&
                 Abs(rows.X[self] - rows.X[target]) <= 240);
            ApplyPressureRetreat(rows, self, target, ai, closeWindow, ref input);
            if (closeWindow && rng.Rand(17) == 0)
                input.KeyDefend = 1;
        }

        private static void ApplyPressureRetreat(
            AiSensingSnapshot rows,
            int self,
            int target,
            Context ai,
            bool closeWindow,
            ref AiDecisionInputState input)
        {
            if (!closeWindow)
                return;
            if ((rows.X[target] < 250 || rows.X[target] < rows.X[self]) &&
                rows.X[target] <= ai.StageTargetX - 250)
            {
                input.KeyRight = 1;
                input.PrevRight = 0;
            }
            else if (rows.X[target] > ai.StageTargetX - 250 || rows.X[target] > rows.X[self])
            {
                input.KeyLeft = 1;
                input.PrevLeft = 0;
            }
        }

        private static void ProcessSubHelper(
            AiSensingSnapshot rows,
            int self,
            int target,
            Context ai,
            int targetState,
            bool specialLeft,
            bool specialRight,
            ref AiDecisionInputState input,
            ref AiDecisionRandomStream rng)
        {
            int oid = rows.ObjectId[self];
            int predictedTargetX = rows.X[target] + 2 * (int)rows.Vx[target];
            if (rows.Pp[self] < 150)
                input.ComboDja = 3;
            if (Abs(rows.X[target] - 2 * (int)rows.Vx[self] - rows.X[self]) < 80 &&
                Abs(rows.Z[target] - rows.Z[self]) < 5 &&
                rng.Rand(ai.Rand3 + 3) == 0 &&
                targetState != 14)
                input.KeyJump = 1;
            if ((specialLeft && rows.X[target] > rows.X[self]) ||
                (specialRight && rows.X[target] < rows.X[self]))
                return;
            if (rng.Rand(ai.Rand3 + 1) != 0)
                return;
            int predictedDelta = Abs(predictedTargetX - rows.X[self]);
            if (IsProcessSubOidGroup(oid) &&
                predictedDelta > 100 &&
                predictedDelta < 900 &&
                Abs(rows.Z[target] - rows.Z[self]) < 5 &&
                rng.Rand(ai.Rand3 + 10) == 0 &&
                targetState != 14)
                input.KeyAttack = 1;
            bool facing =
                (rows.Facing[self] == 0 && rows.X[target] > rows.X[self]) ||
                (rows.Facing[self] == 1 && rows.X[target] < rows.X[self]);
            if (IsProcessSubOidGroup(oid) &&
                predictedDelta > 90 &&
                facing &&
                (rows.Frame[self] == 110 || rows.Frame[self] >= 235) &&
                Abs(rows.Z[target] - rows.Z[self]) < 13 &&
                targetState != 14)
            {
                input.PrevRight = 0;
                input.PrevLeft = 0;
                input.PrevJump = 0;
                if (rows.X[target] <= rows.X[self])
                    input.KeyLeft = 1;
                else
                    input.KeyRight = 1;
                if (oid != 34 || rng.Rand(2) != 0)
                    input.KeyJump = 1;
                else
                    input.KeyDefend = 1;
            }
            if (oid == 1 &&
                predictedDelta > 100 &&
                predictedDelta < 300 &&
                Abs(rows.Z[target] - rows.Z[self]) < 5 &&
                rng.Rand(ai.Rand5 + 10) == 0 &&
                targetState != 14)
                input.KeyAttack = 1;
            if (oid == 1 &&
                predictedDelta > 90 &&
                facing &&
                (rows.Frame[self] == 110 || rows.Frame[self] >= 235) &&
                Abs(rows.Z[target] - rows.Z[self]) < 7 &&
                targetState != 14)
            {
                input.PrevRight = 0;
                input.PrevLeft = 0;
                input.PrevJump = 0;
                if (rows.X[target] <= rows.X[self])
                    input.KeyLeft = 1;
                else
                    input.KeyRight = 1;
                input.KeyJump = 1;
            }
        }

        private static void PushHistory(ref AiDecisionInputState input, int key)
        {
            input.History1 = input.History2;
            input.History2 = input.History3;
            input.History3 = input.History4;
            input.History4 = input.History5;
            input.History5 = key;
        }

        private static void Publish(
            ref AiDecisionWitness witness,
            in AiDecisionInputState input,
            in AiDecisionWorldState world,
            in AiDecisionRandomStream rng)
        {
            witness.Input = input;
            witness.World = world;
            witness.RngState = rng.State;
            witness.RngCalls = rng.Calls;
            witness.RngOrderHash = rng.OrderHash;
            witness.RngDrawCount = rng.DrawCount;
            witness.RngTraceOverflow = rng.TraceOverflow;
            // The snapshot owns the preallocated order trace scratch used by shadow comparison.
            // Publishing only advances primitive cursors; no per-decision arrays are created.
        }

        private static bool RejectBeforeEvaluation(
            AiDecisionSnapshot snapshot,
            in AiDecisionInputState ownedInput,
            AiDecisionAvailability availability,
            ref AiDecisionWitness witness)
        {
            witness.Availability = availability;
            witness.Input = ownedInput;
            witness.World = snapshot.World;
            witness.RngState = snapshot.RngState;
            witness.RngCalls = snapshot.RngCalls;
            witness.RngOrderHash = AiDecisionRandomStream.HashOffset;
            return false;
        }

        private static bool IsIncluded(AiSensingSnapshot rows, int slot)
        {
            return slot >= 0 && slot < rows.Capacity && rows.Included[slot];
        }

        private static bool IsLivingCharacter(AiSensingSnapshot rows, int slot)
        {
            return IsIncluded(rows, slot) && rows.Hp[slot] > 0 && rows.DataObjectType[slot] == 0;
        }

        private static bool IsProcessSubOidGroup(int oid)
        {
            return oid <= 29 || oid == 33 || oid == 34;
        }

        private static bool IsSpecialOidForSubGate(int oid)
        {
            return oid == 18 || oid == 5 || oid == 31 || oid == 36;
        }

        private static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }
    }
}
