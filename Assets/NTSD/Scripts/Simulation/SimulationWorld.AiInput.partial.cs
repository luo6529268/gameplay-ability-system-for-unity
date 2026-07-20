using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public partial class SimulationWorld
    {
        private readonly LF2Entity[] aiInputSlots;

        private struct AiInputContext
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

        private void BuildAiInputSlotSnapshot()
        {
            Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
            GetAllEntities(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                int slot = entity?.Runtime?.SlotIndex ?? -1;
                if (slot >= 0 && slot < aiInputSlots.Length && IsActiveForCurrentPass(entity))
                    aiInputSlots[slot] = entity;
            }
            _entityScratch.Clear();
        }

        private void ClearAiInputSlotSnapshot()
        {
            Array.Clear(aiInputSlots, 0, aiInputSlots.Length);
        }

        internal void PrepareAiInputBasic(LF2Entity self, int tickIndex)
        {
            if (self?.Runtime == null || self.Runtime.HP <= 0)
                return;

            NTSDEntityRuntime input = self.Runtime;
            if (input.Unk3FC > -1000)
            {
                RollAndClearAiKeys(input);
                MoveTowardCoordinate(self, CreateCoordinateAiInputContext());
                input.ApplyInputEdges();
                return;
            }

            AiInputContext ai = CreateAiInputContext(self, tickIndex);

            int selectedSlot = FindNearestAiTargetSlot(self, ai, out int bestDist, out bool sameZLane);
            int savedTargetSlot = input.Unk360;
            LF2Entity cached = AiAt(savedTargetSlot);
            if (IsLivingCharacterDat(cached) && Rand(30) > 0)
                selectedSlot = savedTargetSlot;
            else
                input.Unk360 = selectedSlot;

            if (selectedSlot < 0)
            {
                RollAndClearAiKeys(input);
                AiPostNoTargetFallback(self, cached, ai);
                input.ApplyInputEdges();
                return;
            }

            int selectedBeforeSpecialScan = selectedSlot;
            bool specialObjectProximity = false;
            bool specialLeft = false;
            bool specialRight = false;
            bool specialUp = false;
            bool specialDown = false;
            bool specialGuard7A = false;
            bool specialGuard7B = false;
            bool specialForce7AGround = false;
            bool specialC8ThreatSeen = false;
            bool specialPostSelectionSeen = false;
            int specialBestDist = 10000;

            if (ai.InputPhase == 1 || ai.InputPhase == 4)
            {
                int selfTeam = Team(self);
                if (selfTeam != 5)
                {
                    specialForce7AGround = true;
                    if (Hp(self) > (4 * Hp3(self)) / 5 || Hp(self) > Hp3(self) - 130)
                        specialForce7AGround = false;
                    if (Hp(self) > 430 || Hp(self) > Hp3(self) - 130)
                        specialGuard7A = true;

                    int sameTeamCount = 0;
                    for (int i = 0; i < aiInputSlots.Length; i++)
                    {
                        LF2Entity teammate = AiAt(i);
                        if (teammate == null || teammate == self || !IsLivingCharacterDat(teammate) || Team(teammate) != selfTeam)
                            continue;
                        if (Hp(teammate) < Hp(self)) specialForce7AGround = false;
                        if (Hp(teammate) < Hp(self) - 200) specialGuard7A = true;
                        sameTeamCount++;
                    }
                    if (sameTeamCount == 0) specialForce7AGround = false;
                }
            }

            if (self.Runtime.KillCount > -1) { specialGuard7A = true; specialGuard7B = true; }
            if (Pp(self) > 250) specialGuard7B = true;
            if (ai.InputPhase == 1 && Team(self) == 1) specialGuard7B = true;
            if (Slot(self) >= 20 && ai.InputPhase == 4) specialGuard7B = true;

            for (int i = 20; i < aiInputSlots.Length; i++)
            {
                LF2Entity obj = AiAt(i);
                if (obj == null) continue;
                int objOid = obj.ObjectId;
                int objState = State(obj);
                if (objOid == 0xC8)
                {
                    int frameGroup = Frame(obj) / 10;
                    bool threat = frameGroup == 6 && Team(obj) != Team(self);
                    if (!threat && frameGroup == 5)
                    {
                        bool lowHpWindow = (Hp(self) >= Hp3(self) - 70 || Hp(self) >= Hp3(self) - 200) &&
                                           (Hp(self) >= (3 * Hp3(self)) / 5 || Hp(self) < Hp3(self) - 200);
                        threat = (self.ObjectId == 2 || self.ObjectId == 34) && lowHpWindow && Team(obj) == Team(self);
                    }
                    if (threat) specialC8ThreatSeen = true;
                    if (threat && Abs(Z(obj) - Z(self)) < 25 && Abs(X(obj) - X(self)) < 150)
                    {
                        specialObjectProximity = true;
                        if (Abs(Z(obj) - Z(self)) < 20)
                        {
                            if (Abs(X(obj) - X(self)) < 180)
                            {
                                if (Z(obj) <= Z(self)) specialUp = true; else specialDown = true;
                            }
                            if (X(obj) <= X(self)) specialLeft = true; else specialRight = true;
                        }
                    }
                }

                if ((objOid == 0xD3 && objState == 0x12) || (objOid == 0xD4 && Frame(obj) >= 150 && Frame(obj) <= 170))
                {
                    if (Abs(X(obj) - X(self)) < 80)
                    {
                        if (Z(obj) > Z(self) + 20) specialDown = true;
                        else if (Z(obj) < Z(self) - 20) specialUp = true;
                    }
                    if (Abs(Z(obj) - Z(self)) < 20)
                    {
                        if (X(obj) > X(self) + 100) specialRight = true;
                        else if (X(obj) < X(self) - 100) specialLeft = true;
                    }
                }

                if (!specialPostSelectionSeen && !specialC8ThreatSeen && !sameZLane && input.LinkState == 0)
                {
                    int dist = Distance(self, obj);
                    bool oidCandidate = objOid / 100 == 1 || objOid == 0xD5;
                    bool guarded = (objOid == 0x7A && specialGuard7A) || (objOid == 0x7B && specialGuard7B) ||
                                   (input.HasInputHistoryGate() && objOid != 0x7A);
                    if (dist < 2 * bestDist && dist < specialBestDist && oidCandidate && !guarded &&
                        obj.Runtime.LinkState == 0 && (objState == 0x3EC || objState == 0x7D4))
                    {
                        selectedSlot = i;
                        specialBestDist = dist;
                    }
                }

                if (objOid == 0xC8 && Frame(obj) / 10 == 5 && Abs(X(obj) - X(self)) < 300 &&
                    Abs(Z(obj) - Z(self)) < 90 && Team(obj) == Team(self))
                {
                    bool pressure = (Hp(self) < HpMax(self) - 70 && Hp(self) < 140) ||
                                    (Hp(self) < (3 * HpMax(self)) / 5 && Hp(self) >= 140);
                    if (pressure) selectedSlot = i;
                    specialPostSelectionSeen = true;
                }

                if (specialForce7AGround && objOid == 0x7A && objState == 0x3EC && input.LinkState == 0)
                {
                    selectedSlot = i;
                    specialPostSelectionSeen = true;
                }
            }

            if (specialC8ThreatSeen) selectedSlot = selectedBeforeSpecialScan;
            input.Unk360 = selectedSlot;
            RollAndClearAiKeys(input);
            LF2Entity target = AiAt(selectedSlot);
            if (target == null) { input.ApplyInputEdges(); return; }
            int selfState = State(self);
            int targetState = State(target);
            int targetOid = target.ObjectId;

            if (X(target) > X(self) && Facing(self) == 1) input.KeyRight = 1;
            if (X(target) < X(self) && Facing(self) == 0) input.KeyLeft = 1;
            if (selfState == 2) { if (Facing(self) == 1) input.KeyRight = 1; else input.KeyLeft = 1; }

            int blockRoll = Rand(ai.Rand5 + 8);
            if (blockRoll == 0 && (input.ZBoundNegative || input.ZBoundPositive || input.XBoundNegative || input.XBoundPositive))
            { input.PrevJump = 0; input.KeyJump = 1; }

            if (AiPreUpdateTarget3000SideEffect(self, target, selfState, targetState, ai)) { input.ApplyInputEdges(); return; }

            if (input.HasInputHistoryGate() && input.LinkState > 0)
            {
                LF2Entity held = AiAt(input.TargetSlotIndex);
                if (held != null && (held.ObjectId == 0x7A || held.ObjectId == 0x7B))
                { input.PrevJump = 0; input.KeyJump = 1; input.ApplyInputEdges(); return; }
            }

            bool coordinateAllowsSpecial = !input.HasInputHistoryGate() || AiPostCacheCoordinateAllowsSpecial(self);
            if (coordinateAllowsSpecial && (targetState == 0x3EC || targetState == 0x7D4))
            {
                if (input.HasInputHistoryGate() && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240) &&
                    targetOid != 0x7A && targetOid != 0x7B) { input.ApplyInputEdges(); return; }
                MoveTowardTarget(self, target, ai, selfState);
                if (Abs(Z(target) - Z(self)) <= 3 && Abs(X(target) - X(self)) <= 6) { input.PrevJump = 0; input.KeyJump = 1; }
                input.ApplyInputEdges(); return;
            }

            if (targetState == 14 || Abs(Y(target)) > 2)
            {
                if (X(target) > ai.StageTargetX - 30) { input.KeyLeft = 1; input.PrevLeft = 0; input.ApplyInputEdges(); return; }
                if (X(target) < 30) { input.KeyRight = 1; input.PrevRight = 0; input.ApplyInputEdges(); return; }
                if (Abs(Z(target) - Z(self)) <= 45 || Abs(X(target) - X(self)) <= 350)
                {
                    if (X(target) > X(self)) { input.KeyLeft = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    else { input.KeyRight = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if (Z(target) < Z(self) || Z(target) < StageZMin + 10) input.KeyDown = 1; else input.KeyUp = 1;
                }
                input.ApplyInputEdges(); return;
            }

            bool c8Allowed = (input.HasInputHistoryGate() && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) ||
                             (targetState != 14 && Abs(Y(target)) <= 2);
            if (c8Allowed && targetOid == 0xC8)
            {
                if (X(target) > X(self) + 7) input.KeyRight = 1; else if (X(target) < X(self) - 7) input.KeyLeft = 1;
                if (Z(target) > Z(self) + 2) input.KeyDown = 1; else if (Z(target) < Z(self) - 2) input.KeyUp = 1;
                input.ApplyInputEdges(); return;
            }

            if (Rand(ai.Rand5 + 1) == 0)
            {
                if (AiUpdateFirstDecision(self, target, bestDist, specialObjectProximity) ||
                    AiUpdateTeammateGuardDecision(self, ai, bestDist, sameZLane) ||
                    AiUpdateOid1ComboDecision(self, target, targetState) ||
                    AiUpdateCloseOid1Decision(self, target) ||
                    AiUpdateOid4ComboDecision(self, target) ||
                    AiUpdateOid5ComboDecision(self, target))
                { input.ApplyInputEdges(); return; }
            }

            if (AiUpdateOid33_19_16PredictedDuaDecision(self, target, targetState) ||
                AiUpdateOid52_1_2_21PreLabel591Decision(self, target, targetState) ||
                AiUpdateLabel591Oid51_2_18_7Decision(self, target))
            { input.ApplyInputEdges(); return; }

            bool closeOrFree = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
            int selfOid = self.ObjectId;
            bool widePath = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if (!widePath)
            {
                bool targetPressure = Hp(target) > Hp(self) * 2 || (Hp(self) <= 100 && Hp3(self) > 100);
                widePath = targetPressure && ai.InputPhase == 1 && IsCharacterDat(target) && Slot(self) >= 20 && Team(self) != 5;
            }

            if (closeOrFree)
            {
                if ((specialRight || ai.MoveMode == 1) && selfState == 2 && Facing(self) == 0) input.KeyLeft = 1;
                if (specialLeft && selfState == 2 && Facing(self) == 1) input.KeyRight = 1;
                int threshold = widePath ? 170 : 60;
                int near = widePath ? 150 : 0;
                if (selfState != 19)
                {
                    if ((X(target) > X(self) + threshold || ((X(target) > X(self) + near || (selfState == 7 && X(target) > X(self))) && Facing(self) == 1)) &&
                        !specialRight && ((widePath && ai.MoveMode == 0) || (!widePath && (ai.MoveMode == 0 || Facing(self) == 1))))
                    { input.KeyRight = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if ((X(target) < X(self) - threshold || ((X(target) < X(self) - near || (selfState == 7 && X(target) < X(self))) && Facing(self) == 0)) && !specialLeft)
                    { input.KeyLeft = 1; if (Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    if (((Z(target) > Z(self) + 3 && !specialObjectProximity) || ((specialRight || specialLeft) && specialUp)) && !specialDown) input.KeyDown = 1;
                    if (((Z(target) < Z(self) - 3 && !specialObjectProximity) || ((specialRight || specialLeft) && specialDown)) && !specialUp) input.KeyUp = 1;
                }
            }

            if (input.LinkState > 0 && !AiProcessHelper(self, target, ai, selfState, targetState, sameZLane, specialObjectProximity))
            { input.ApplyInputEdges(); return; }

            if (Rand(ai.Difficulty * 7 + 10) == 0 && (targetState == 3 || targetState / 100 == 3) &&
                Abs(Z(target) - Z(self)) < 9 && ((Facing(target) == 0 && X(target) < X(self)) || (Facing(target) == 1 && X(target) > X(self))))
                input.KeyAttack = 1;
            if (closeOrFree && Rand(2 * (ai.Rand5 + 10)) < 3 && Rand(20) < 3 && targetState != 14) input.KeyDefend = 1;
            bool selfGroup = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if ((!selfGroup || targetState == 16) && Abs(X(target) - 2 * (int)self.Runtime.Vx - X(self)) < 50 &&
                Abs(Z(target) - Z(self)) < 5 && Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;

            AiProcessSubCallerPrewrite(self, target, ai, selfState, targetState);
            AiProcessSubLabel435PressurePrewrite(self, target, ai, selfState, targetState);
            AiProcessSubHelper(self, target, ai, targetState, specialLeft, specialRight);
            input.ApplyInputEdges();
        }

        private AiInputContext CreateAiInputContext(LF2Entity self, int tickIndex)
        {
            int inputPhase = InputPhase;
            int difficulty = Difficulty;
            bool forceZero = AiPhaseGate == 1;
            if (!forceZero && inputPhase == 1 && Team(self) != 5)
                forceZero = Slot(self) < 20 || self.ObjectId < 30;
            if (forceZero || difficulty < 0) difficulty = 0;
            AiInputContext ai = new AiInputContext
            {
                Difficulty = difficulty,
                Rand3 = difficulty * 3,
                Rand5 = difficulty * 5,
                Rand15 = difficulty * 15,
                Rand20 = difficulty * 20,
                InputPhase = inputPhase,
                StageTargetX = Runtime?.Stage?.XMaxOverride > 0 ? Runtime.Stage.XMaxOverride : (Runtime?.Stage?.StageWidthPx ?? 800),
            };
            AiUpdateMoveModeScan(self, ref ai);
            if (Runtime?.Flow != null)
            {
                Runtime.Flow.AiDifficulty = ai.Difficulty;
                Runtime.Flow.AiRand3 = ai.Rand3;
                Runtime.Flow.AiRand5 = ai.Rand5;
                Runtime.Flow.AiRand15 = ai.Rand15;
                Runtime.Flow.AiRand20 = ai.Rand20;
                Runtime.Flow.AiMoveMode = ai.MoveMode;
                Runtime.Flow.AiStageTargetX = ai.StageTargetX;
            }
            return ai;
        }

        private AiInputContext CreateCoordinateAiInputContext()
        {
            BattleFlowRuntimeState flow = Runtime?.Flow;
            return new AiInputContext
            {
                Difficulty = flow?.AiDifficulty ?? 0,
                Rand3 = flow?.AiRand3 ?? 0,
                Rand5 = flow?.AiRand5 ?? 0,
                Rand15 = flow?.AiRand15 ?? 0,
                Rand20 = flow?.AiRand20 ?? 0,
                MoveMode = flow?.AiMoveMode ?? 0,
                StageTargetX = flow?.AiStageTargetX ?? (Runtime?.Stage?.StageWidthPx ?? 800),
                InputPhase = InputPhase,
            };
        }

        private int StageZMin => Runtime?.Stage?.ZMin ?? 180;
        private int StageZMax => Runtime?.Stage?.ZMax ?? 350;
        private int Rand(int modulus) => Rng.NextRaw() % Math.Max(1, modulus);
        private LF2Entity AiAt(int slot) => slot >= 0 && slot < aiInputSlots.Length ? aiInputSlots[slot] : null;
        private static int X(LF2Entity e) => e.Runtime.XInt;
        private static int Y(LF2Entity e) => e.Runtime.YInt;
        private static int Z(LF2Entity e) => e.Runtime.ZInt;
        private static int Hp(LF2Entity e) => e.Runtime.HP;
        private static int Hp3(LF2Entity e) => e.Runtime.HP3;
        private static int HpMax(LF2Entity e) => e.Runtime.HPBound;
        private static int Pp(LF2Entity e) => e.Runtime.PP;
        private static int Team(LF2Entity e) => e.Runtime.RelationTeam;
        private static int Slot(LF2Entity e) => e.Runtime.SlotIndex;
        private static int Frame(LF2Entity e) => e.Runtime.Frame;
        private static int State(LF2Entity e) => e.GetState();
        private static int Facing(LF2Entity e) => e.Runtime.Dir == "left" ? 1 : 0;
        private static int Abs(int value) => Math.Abs(value);
        private static int Distance(LF2Entity a, LF2Entity b) => Abs(X(b) - X(a)) + Abs(Z(b) - Z(a));
        private static bool IsCharacterDat(LF2Entity e) => e != null && e.GetCurrentDataObjectTypeForSimulation() == 0;
        private static bool IsLivingCharacterDat(LF2Entity e) => IsCharacterDat(e) && Hp(e) > 0;

        private int FindNearestAiTargetSlot(LF2Entity self, AiInputContext ai, out int bestDist, out bool sameZLane)
        {
            int selected = -1;
            bestDist = 10000;
            for (int i = 0; i < aiInputSlots.Length; i++)
            {
                LF2Entity candidate = AiAt(i);
                if (candidate == null || candidate == self)
                    continue;
                int state = State(candidate);
                if (!IsCharacterDat(candidate))
                {
                    if (state != 3000) continue;
                    if (X(candidate) > X(self)) { if (!(candidate.Runtime.Vx < 0.001)) continue; }
                    else if (X(candidate) < X(self)) { if (!(candidate.Runtime.Vx > 0.001)) continue; }
                    else continue;
                }
                if (!TeamCandidateAllowed(self, candidate, ai.InputPhase) || Hp(candidate) <= 0 || state == 14 || Abs(Y(candidate)) > 2)
                    continue;
                int dist = Distance(self, candidate);
                if (dist < bestDist) { bestDist = dist; selected = i; }
            }

            sameZLane = selected >= 0 && Abs(Z(AiAt(selected)) - Z(self)) < 15;
            if (State(self) != 9)
            {
                int bestAirDist = 10000;
                for (int i = 0; i < aiInputSlots.Length; i++)
                {
                    LF2Entity candidate = AiAt(i);
                    if (candidate == null || candidate == self || !TeamCandidateAllowed(self, candidate, ai.InputPhase) || Hp(candidate) <= 0)
                        continue;
                    int state = State(candidate);
                    if (state != 14 && Abs(Y(candidate)) <= 2) continue;
                    int dist = Distance(self, candidate);
                    if (dist >= bestAirDist || Abs(Z(candidate) - Z(self)) >= 40 || Abs(X(candidate) - X(self)) >= 250) continue;
                    bestAirDist = dist;
                    selected = i;
                }
            }
            return selected;
        }

        private static bool TeamCandidateAllowed(LF2Entity self, LF2Entity candidate, int inputPhase)
        {
            if (Team(candidate) != Team(self))
            {
                if (inputPhase != 1) return true;
                if (Team(self) == 5) return true;
            }
            if (Team(candidate) != 5) return false;
            if (inputPhase != 1) return false;
            return Team(candidate) != Team(self);
        }

        private void AiUpdateMoveModeScan(LF2Entity self, ref AiInputContext ai)
        {
            if (ai.InputPhase != 1 || Team(self) == 5) return;
            int rightmostX = -1;
            int rightmostZ = 0;
            for (int i = 0; i < 10; i++)
            {
                LF2Entity candidate = AiAt(i);
                if (candidate == null || candidate == self || !IsLivingCharacterDat(candidate)) continue;
                if (X(candidate) > rightmostX) { rightmostX = X(candidate); rightmostZ = Z(candidate); }
            }
            if (rightmostX < 0) return;
            if (X(self) > rightmostX && X(self) + Abs(Z(self) - rightmostZ) / 2 - rightmostX > 200) ai.MoveMode = 1;
            if (X(self) > rightmostX + 400) ai.MoveMode = 2;
        }

        private void AiPostNoTargetFallback(LF2Entity self, LF2Entity savedTarget, AiInputContext ai)
        {
            if (savedTarget != null)
            {
                bool close = !self.Runtime.HasInputHistoryGate() || (Abs(Z(self) - Z(savedTarget)) <= 150 && Abs(X(self) - X(savedTarget)) <= 240);
                if (close && ai.MoveMode == 1) self.Runtime.KeyLeft = 1;
            }
            if ((self.ObjectId == 7 && Frame(self) >= 255 && Frame(self) <= 261) ||
                (self.ObjectId == 9 && Frame(self) >= 280 && Frame(self) <= 290) ||
                (self.ObjectId == 32 && Frame(self) >= 240 && Frame(self) <= 245))
                self.Runtime.KeyAttack = 1;
        }

        private static void RollAndClearAiKeys(NTSDEntityRuntime input)
        {
            input.RollInputFromCurrent();
            input.ClearDirectionalInputKeys();
            input.ClearActionInputKeys();
        }

        private void MoveTowardCoordinate(LF2Entity self, AiInputContext ai)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (input.Unk3FC <= -1000 || input.Unk400 <= -1000) return;
            if (X(self) > input.Unk3FC + 6)
            {
                input.KeyLeft = 1;
                if (X(self) > input.Unk3FC + 250 && Rand(ai.Rand3 + 3) == 0) input.PrevLeft = 0;
                if (X(self) < input.Unk3FC + 100 && State(self) == 2 && Facing(self) == 1) input.KeyRight = 1;
            }
            else if (X(self) < input.Unk3FC - 6)
            {
                input.KeyRight = 1;
                if (X(self) < input.Unk3FC - 250 && Rand(ai.Rand3 + 3) == 0) input.PrevRight = 0;
                if (X(self) > input.Unk3FC - 100 && State(self) == 2 && Facing(self) == 0) input.KeyLeft = 1;
            }
            if (Z(self) < input.Unk400 - 3) input.KeyDown = 1;
            else if (Z(self) > input.Unk400 + 3) input.KeyUp = 1;
            if (input.XBoundPositive || input.XBoundNegative) { input.PrevJump = 0; input.KeyJump = 1; }
            if (Abs(input.Unk400 - Z(self)) <= 90 && Abs(input.Unk3FC - X(self)) <= 90)
            { input.Unk3FC = -1000; input.Unk400 = -1000; }
        }

        private void MoveTowardTarget(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (X(self) > X(target) + 6)
            {
                input.KeyLeft = 1;
                if (X(self) > X(target) + 250 && Rand(ai.Rand3 + 3) == 0) input.PrevLeft = 0;
                if (X(self) < X(target) + 100 && selfState == 2 && Facing(self) == 1) input.KeyRight = 1;
            }
            else if (X(self) < X(target) - 6)
            {
                if (ai.MoveMode == 0) input.KeyRight = 1;
                if (X(self) < X(target) - 250 && Rand(ai.Rand3 + 3) == 0 && ai.MoveMode == 0) input.PrevRight = 0;
                if (X(self) > X(target) - 100 && selfState == 2 && Facing(self) == 0) input.KeyLeft = 1;
            }
            if (Z(self) < Z(target) - 3) input.KeyDown = 1;
            else if (Z(self) > Z(target) + 3) input.KeyUp = 1;
        }

        private static bool AiPostCacheCoordinateAllowsSpecial(LF2Entity self)
        {
            NTSDEntityRuntime r = self.Runtime;
            if (r.Unk3FC <= -1000) return true;
            if (Abs(r.Unk400 - Z(self)) > 90 || Abs(r.Unk3FC - X(self)) > 90) return false;
            r.Unk3FC = -1000; r.Unk400 = -1000;
            return true;
        }

        private bool AiPreUpdateTarget3000SideEffect(LF2Entity self, LF2Entity target, int selfState, int targetState, AiInputContext ai)
        {
            if (targetState != 3000) return false;
            bool randomGate = ai.Rand3 <= 0 || Rand(ai.Rand3) == 0;
            if (selfState != 7 && randomGate &&
                ((X(target) > X(self) && X(target) < X(self) + 200 && target.Runtime.Vx < 0.0) ||
                 (X(target) < X(self) && X(target) > X(self) - 200 && target.Runtime.Vx > 0.0)))
            { self.Runtime.PrevAttack = 0; self.Runtime.KeyAttack = 1; }
            if (X(target) > X(self) && Facing(self) == 1) self.Runtime.KeyRight = 1;
            if (X(target) < X(self) && Facing(self) == 0) self.Runtime.KeyLeft = 1;
            return true;
        }

        private bool AiUpdateOid33_19_16PredictedDuaDecision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = self.ObjectId;
            if (oid != 33 && oid != 19 && oid != 16) return false;
            if (Rand(5) != 0 && targetState != 16 && targetState != 8) return false;
            bool facing = (Facing(self) == 0 && X(self) < X(target)) || (Facing(self) == 1 && X(self) > X(target));
            if (Abs(X(target) + (int)self.Runtime.Vx - X(self)) < 60 && Abs(Z(target) - Z(self)) < 7 && Pp(self) > 150 && facing)
            { self.Runtime.ComboDua = 3; return true; }
            return false;
        }

        private bool AiUpdateOid52_1_2_21PreLabel591Decision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = self.ObjectId;
            if (oid != 52 && oid != 1 && oid != 2 && oid != 21) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (targetState == 3 && Pp(self) > 125 && Rand(10) == 0 && dx < 120 && dz < 10)
            { self.Runtime.ComboDja = 3; return true; }
            if (Pp(self) > 125 && Rand(5) == 0 && dx < 100 && dz < 30)
            { if (X(target) > X(self)) self.Runtime.ComboDuj = 3; return true; }
            if (Pp(self) > 125 && Rand(14) == 0 && dx < 700 && dz < 150)
            { if (X(target) > X(self)) self.Runtime.ComboDra = 3; else self.Runtime.ComboDla = 3; return true; }
            if (Pp(self) > 125 && Rand(5) == 0 && dz < 20)
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            bool predictedGate = Rand(5) == 0 || targetState == 16 || targetState == 8;
            bool facing = (Facing(self) == 0 && X(self) < X(target)) || (Facing(self) == 1 && X(self) > X(target));
            if (predictedGate && Abs(X(target) + (int)self.Runtime.Vx - X(self)) < 100 && dz < 7 && Pp(self) < 100 && facing)
            { self.Runtime.ComboDua = 3; return true; }
            return false;
        }

        private bool AiUpdateLabel591Oid51_2_18_7Decision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 51 && oid != 2 && oid != 18 && oid != 7) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Frame(self) > 265 && Frame(self) < 280 && (dz > 13 || !IsCharacterDat(target)))
            { self.Runtime.PrevAttack = 0; self.Runtime.KeyAttack = 1; return true; }
            if (Pp(self) > 300 && Rand(10) == 0 && dx < 300 && dz < 200) { self.Runtime.ComboDuj = 3; return true; }
            if (Pp(self) > 300 && Rand(10) == 0 && dx < 950) { self.Runtime.ComboDua = 3; return true; }
            if (Rand(5) == 0 && Pp(self) > 250 && dx < 1200 && dx > 40 && dz < 13)
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            return false;
        }

        private bool AiUpdateFirstDecision(LF2Entity self, LF2Entity target, int nearestTargetDist, bool specialObjectProximity)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (Rand(10) == 0 && Pp(self) > 85 &&
                ((Hp(self) < HpMax(self) - 70 && Hp(self) < 450) || (Hp(self) < (3 * HpMax(self)) / 5 && Hp(self) >= 140)))
            { self.Runtime.ComboDdj = 3; return true; }
            if (nearestTargetDist < 10000 && Rand(30) == 0 && Pp(self) > 250) { self.Runtime.ComboDua = 3; return true; }
            int targetOid = target.ObjectId;
            bool split = targetOid == 2 || targetOid == 9 || targetOid == 10 || targetOid == 11 || targetOid == 33 || targetOid == 34;
            int maxDx = split ? 500 : 250;
            int targetPpMin = split ? 220 : 170;
            if (Rand(15) == 0 && Abs(X(target) - X(self)) > 100 && Abs(X(target) - X(self)) < maxDx &&
                Abs(Z(target) - Z(self)) < 30 && Pp(self) > 100 && Pp(target) > targetPpMin && !specialObjectProximity)
            { if (X(target) <= X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3; return true; }
            return false;
        }

        private bool AiUpdateTeammateGuardDecision(LF2Entity self, AiInputContext ai, int nearestTargetDist, bool sameZLane)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (self.Runtime.LinkState != 0 && Frame(self) >= 9) return false;
            bool hpWindow = (Hp(self) >= HpMax(self) - 70 || Hp(self) >= 140) &&
                            (Hp(self) >= (3 * HpMax(self)) / 5 || Hp(self) < 140);
            if (!hpWindow || sameZLane) return false;
            for (int i = 0; i < 20; i++)
            {
                LF2Entity cand = AiAt(i);
                if (cand == null || cand == self || Team(cand) == 0 || Team(cand) != Team(self) ||
                    Abs(X(cand) - X(self)) >= 250 || Abs(Z(cand) - Z(self)) >= 60 || Pp(self) <= 350)
                    continue;
                bool lowHp = (Hp(cand) < HpMax(cand) - 90 && Hp(cand) < 140) ||
                             (Hp(cand) < (3 * HpMax(cand)) / 5 && Hp(cand) >= 140);
                if (!lowHp || Hp(cand) <= 0 || Distance(self, cand) >= nearestTargetDist / 3) continue;
                if (X(cand) > X(self) && Facing(self) == 1 && Abs(X(cand) - X(self)) >= 5)
                { self.Runtime.KeyRight = 1; self.Runtime.KeyLeft = 0; return true; }
                if (X(cand) < X(self) && Facing(self) != 1 && Abs(X(cand) - X(self)) >= 5)
                { self.Runtime.KeyRight = 0; self.Runtime.KeyLeft = 1; return true; }
                self.Runtime.ComboDuj = 3; return true;
            }
            return false;
        }

        private bool AiUpdateOid1ComboDecision(LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 21 && oid != 17) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Frame(self) >= 260 && Frame(self) <= 289 && dx < 100 && dz < 7) return false;
            if (Rand(7) == 0 && dx < 150 && dz < 8 && Pp(self) > 150 &&
                ((Rand(10) == 0 && targetState != 3) || (Rand(3) > 0 && (targetState == 16 || targetState == 8 || targetState == 11))))
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            if (Rand(7) == 0 && dx < 100 && dz < 7 && Pp(self) > 75)
            {
                if (Pp(self) <= 150 || ((Rand(10) != 0 || targetState == 3) && (Rand(3) <= 0 || targetState != 16)))
                { self.Runtime.ComboDda = 3; return true; }
                if (X(target) <= X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3;
                return true;
            }
            return false;
        }

        private bool AiUpdateCloseOid1Decision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 1 && oid != 21 && oid != 17) return false;
            if (Frame(self) < 260 || Frame(self) > 289 || Abs(X(target) - X(self)) >= 100 || Abs(Z(target) - Z(self)) >= 7) return false;
            if ((Y(target) == 0 && Y(self) == 0 && Rand(3) == 0) || (Y(target) < 0 && Y(self) < 0 && Rand(7) == 0))
            { self.Runtime.KeyJump = 1; self.Runtime.PrevJump = 0; return true; }
            if ((Y(target) >= 0 || Rand(5) != 0) && Rand(30) != 0) return true;
            bool targetRight = X(target) > X(self);
            bool targetLeft = X(target) < X(self);
            if ((targetRight && Facing(self) == 0) || (targetLeft && Facing(self) == 1)) self.Runtime.KeyDefend = 1;
            self.Runtime.PrevDefend = 0;
            return true;
        }

        private bool AiUpdateOid4ComboDecision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 4 && oid != 10 && oid != 19) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Pp(self) > 360 && dx < 100 && dz < 70 && Rand(Hp(self) / 5 + 10) == 0)
            { self.Runtime.ComboDuj = 3; return true; }
            if (Rand(45) == 0 && dx > 100 && dx < 550 && dz < 20 && Pp(self) > 170)
            { if (X(target) <= X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3; return true; }
            if (Rand(30) == 0 && Pp(self) > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                bool facing = (Facing(self) == 0 && X(self) < X(target)) || (Facing(self) == 1 && X(self) > X(target));
                if (facing) { self.Runtime.ComboDja = 3; return true; }
            }
            return false;
        }

        private bool AiUpdateOid5ComboDecision(LF2Entity self, LF2Entity target)
        {
            int oid = self.ObjectId;
            if (oid != 5 && oid != 19) return false;
            int dx = Abs(X(target) - X(self));
            int dz = Abs(Z(target) - Z(self));
            if (Pp(self) > 450 && dx > 100 && dz > 50 && Rand(3) == 0)
            { if (Rand(2) != 0) self.Runtime.ComboDdj = 3; else self.Runtime.ComboDuj = 3; return true; }
            if (Pp(self) > 70 && dx > 100 && dx < 160 && dz < 8 && Rand(10) == 0)
            { if (X(target) > X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            if (Rand(30) == 0 && Pp(self) > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                if (Facing(self) == 0 && X(self) < X(target)) { self.Runtime.ComboDra = 3; return true; }
                if (Facing(self) == 1 && X(self) > X(target)) { self.Runtime.ComboDla = 3; return true; }
            }
            return false;
        }

        private static bool AiProcessSubOidGroup(int oid) => oid <= 29 || oid == 33 || oid == 34;
        private static bool AiSpecialOidForSubGate(int oid) => oid == 18 || oid == 5 || oid == 31 || oid == 36;

        private void AiProcessSubHelper(LF2Entity self, LF2Entity target, AiInputContext ai, int targetState, bool specialLeft, bool specialRight)
        {
            NTSDEntityRuntime input = self.Runtime;
            int oid = self.ObjectId;
            int predictedTargetX = X(target) + 2 * (int)target.Runtime.Vx;
            if (Pp(self) < 150) input.ComboDja = 3;
            if (Abs(X(target) - 2 * (int)self.Runtime.Vx - X(self)) < 80 && Abs(Z(target) - Z(self)) < 5 &&
                Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;
            if ((specialLeft && X(target) > X(self)) || (specialRight && X(target) < X(self))) return;
            if (Rand(ai.Rand3 + 1) != 0) return;
            int predictedDelta = Abs(predictedTargetX - X(self));
            if (AiProcessSubOidGroup(oid) && predictedDelta > 100 && predictedDelta < 900 && Abs(Z(target) - Z(self)) < 5 &&
                Rand(ai.Rand3 + 10) == 0 && targetState != 14) input.KeyAttack = 1;
            bool facing = (Facing(self) == 0 && X(target) > X(self)) || (Facing(self) == 1 && X(target) < X(self));
            if (AiProcessSubOidGroup(oid) && predictedDelta > 90 && facing && (Frame(self) == 110 || Frame(self) >= 235) &&
                Abs(Z(target) - Z(self)) < 13 && targetState != 14)
            {
                input.PrevRight = input.PrevLeft = input.PrevJump = 0;
                if (X(target) <= X(self)) input.KeyLeft = 1; else input.KeyRight = 1;
                if (oid != 34 || Rand(2) != 0) input.KeyJump = 1; else input.KeyDefend = 1;
            }
            if (oid == 1 && predictedDelta > 100 && predictedDelta < 300 && Abs(Z(target) - Z(self)) < 5 &&
                Rand(ai.Rand5 + 10) == 0 && targetState != 14) input.KeyAttack = 1;
            if (oid == 1 && predictedDelta > 90 && facing && (Frame(self) == 110 || Frame(self) >= 235) &&
                Abs(Z(target) - Z(self)) < 7 && targetState != 14)
            {
                input.PrevRight = input.PrevLeft = input.PrevJump = 0;
                if (X(target) <= X(self)) input.KeyLeft = 1; else input.KeyRight = 1;
                input.KeyJump = 1;
            }
        }

        private void AiProcessSubCallerPrewrite(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(self.ObjectId);
            if (input.LinkState == 0 && targetState == 16 && specialOid &&
                Abs(X(target) - 2 * (int)input.Vx - X(self)) < 350 && Abs(Z(target) - Z(self)) < 5 && Rand(ai.Rand3 + 3) == 0)
            {
                if ((X(target) > X(self) && Facing(self) == 0) || (X(target) <= X(self) && Facing(self) == 1)) input.KeyJump = 1;
            }
            if (input.LinkState != 0 || targetState == 16 || !specialOid) return;
            bool closeTrigger = X(target) - X(self) < 100 && Abs(Z(target) - Z(self)) < 80 && Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger && selfState != 7)
            {
                if (Abs(X(target) - 2 * (int)input.Vx - X(self)) < 300 && Abs(Z(target) - Z(self)) < 5 &&
                    Rand(ai.Rand3 + 3) == 0 && targetState != 14 &&
                    ((X(target) > X(self) && Facing(self) == 0) || (X(target) <= X(self) && Facing(self) == 1))) input.KeyJump = 1;
            }
            else if (selfState != 7)
            {
                bool closeWindow = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
                ApplyPressureRetreat(self, target, ai, closeWindow);
                if (closeWindow && Rand(17) == 0) input.KeyDefend = 1;
            }
        }

        private void AiProcessSubLabel435PressurePrewrite(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(self.ObjectId);
            if (targetState != 16 && specialOid && input.LinkState == 0) return;
            bool pressure = Hp(target) > Hp(self) * 2 || (Hp(self) <= 100 && Hp3(self) > 100);
            if (!pressure || ai.InputPhase != 1 || !IsCharacterDat(target) || Slot(self) < 20 || Team(self) == 5) return;
            bool closeTrigger = X(target) - X(self) < 100 && Abs(Z(target) - Z(self)) < 80 && Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger || selfState == 7) return;
            bool closeWindow = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
            ApplyPressureRetreat(self, target, ai, closeWindow);
            if (closeWindow && Rand(17) == 0) input.KeyDefend = 1;
        }

        private static void ApplyPressureRetreat(LF2Entity self, LF2Entity target, AiInputContext ai, bool closeWindow)
        {
            if (!closeWindow) return;
            if ((X(target) < 250 || X(target) < X(self)) && X(target) <= ai.StageTargetX - 250)
            { self.Runtime.KeyRight = 1; self.Runtime.PrevRight = 0; }
            else if (X(target) > ai.StageTargetX - 250 || X(target) > X(self))
            { self.Runtime.KeyLeft = 1; self.Runtime.PrevLeft = 0; }
        }

        private bool AiProcessHelper(LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState, bool sameZLane, bool specialObjectProximity)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (Rand(ai.Rand3 + 1) > 0) return false;
            int heldSlot = input.TargetSlotIndex;
            if (heldSlot < 0 || heldSlot >= aiInputSlots.Length) return true;
            LF2Entity held = AiAt(heldSlot);
            int heldOid = held != null ? held.ObjectId : -1;
            bool lineCover = false;
            for (int i = 0; i < 20; i++)
            {
                LF2Entity cand = AiAt(i);
                if (cand == null || cand == self || Team(cand) == 0 || Team(target) != Team(self) || Hp(cand) <= 0 ||
                    State(cand) == 14 || Abs(Y(cand)) > 2) continue;
                if (Abs(Z(cand) - Z(self)) < 15 && ((X(self) < X(cand) && X(cand) < X(target)) || (X(target) < X(cand) && X(cand) < X(self))))
                    lineCover = true;
            }
            if (selfState == 2 && Rand(ai.Rand3 + 5) == 0)
            { if (lineCover) input.KeyDefend = 1; else input.KeyJump = 1; }

            int vxTwice = 2 * (int)input.Vx;
            if (heldOid == 100 || heldOid == 101 || heldOid == 120 || heldOid == 121 || heldOid == 124)
            {
                if (Abs(X(target) - vxTwice - X(self)) < 10000 && Abs(Z(target) - Z(self)) < 6 && Rand(ai.Rand3 + 3) == 0 && targetState != 14)
                    input.KeyJump = 1;
                if (heldOid == 124 && Rand(ai.Rand15 + 30) == 0) input.KeyJump = 1;
                if (Rand(ai.Rand3 + 5) == 0)
                {
                    bool close = !input.HasInputHistoryGate() || (Abs(Z(self) - Z(target)) <= 150 && Abs(X(self) - X(target)) <= 240);
                    if (close && Abs(X(target) - X(self)) < 600 && Abs(Z(target) - Z(self)) < 20)
                    {
                        if (X(target) > X(self) && ai.MoveMode == 0) { input.KeyRight = 1; input.PrevRight = 0; }
                        if (X(target) < X(self)) { input.KeyLeft = 1; input.PrevLeft = 0; }
                    }
                }
            }
            if ((heldOid == 150 || heldOid == 151) && !lineCover && Abs(X(target) - vxTwice - X(self)) < 5000 &&
                Abs(Z(target) - Z(self)) < 10 && Rand(ai.Rand5 + 7) == 0 && targetState != 14) input.KeyJump = 1;
            if (heldOid != 122 && heldOid != 123) return true;

            input.ClearActionInputKeys(); input.ClearDirectionalInputKeys();
            if (selfState == 17 && sameZLane && !specialObjectProximity && input.HitStop != 0)
            { input.KeyAttack = 1; return false; }
            if (input.HasInputHistoryGate() && (Abs(Z(self) - Z(target)) > 150 || Abs(X(self) - X(target)) > 240)) return false;
            if (Z(target) < StageZMin + 30) input.KeyDown = 1;
            else if (Z(target) < StageZMax - 30) input.KeyUp = 1;
            else if (Z(target) > Z(self)) input.KeyUp = 1;
            else input.KeyDown = 1;

            if (X(target) < 400 && X(self) < 200)
            {
                input.KeyRight = 1;
                if (Rand(ai.Rand3 + 7) == 0) input.PrevRight = 0;
                if (Rand(ai.Rand3 + 5) == 0 && selfState == 2) input.KeyDefend = 1;
                return false;
            }
            if (X(target) > ai.StageTargetX - 400 && X(self) > ai.StageTargetX - 200)
            {
                input.KeyLeft = 1;
                if (Rand(ai.Rand3 + 7) == 0) input.PrevLeft = 0;
                if (Rand(ai.Rand3 + 5) == 0 && selfState == 2) input.KeyDefend = 1;
                return false;
            }
            if (Abs(X(target) - X(self)) < 350 && Abs(Z(target) - Z(self)) < 70)
            {
                if (X(target) > X(self)) { input.KeyLeft = 1; if (Rand(ai.Rand3 + 4) == 0) input.PrevLeft = 0; }
                if (X(target) <= X(self)) { input.KeyRight = 1; if (Rand(ai.Rand3 + 4) == 0) input.PrevRight = 0; }
                return false;
            }
            if (selfState == 2)
            { if (Facing(self) == 0) input.KeyLeft = 1; if (Facing(self) == 1) input.KeyRight = 1; return false; }
            if (Rand(5) != 0) return false;
            if (specialObjectProximity || (self.ObjectId != 2 && self.ObjectId != 34) || Pp(self) <= 150 || Rand(ai.Rand3 + 3) <= 0)
            { input.KeyJump = 1; return false; }
            if (X(target) > X(self)) input.ComboDrj = 3; else input.ComboDlj = 3;
            return true;
        }
    }
}
