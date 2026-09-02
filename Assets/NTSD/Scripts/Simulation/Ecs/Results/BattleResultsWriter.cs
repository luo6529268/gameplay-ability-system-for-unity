using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns the deterministic battle-results state machine. The Unity host may
    /// present these values, but it must not advance or rewrite them.
    /// </summary>
    internal sealed class BattleResultsWriter
    {
        private readonly SimulationWorld world;

        internal BattleResultsWriter(SimulationWorld world)
        {
            this.world = world;
        }

        internal void RunActiveTick(FrameInputSet frameInput)
        {
            BattleRuntimeState battle = world.Runtime;
            BattleResultsRuntimeState results = battle?.Results;
            if (results == null || !results.IsActive)
                return;

            SimulationInputButtons pressed =
                CollectP1P2PressedButtons(frameInput);
            bool goLeft = Pressed(pressed, SimulationInputButtons.Left);
            bool goRight = Pressed(pressed, SimulationInputButtons.Right);
            bool goUp = Pressed(pressed, SimulationInputButtons.Up);
            bool goDown = Pressed(pressed, SimulationInputButtons.Down);
            bool goAttack = Pressed(pressed, SimulationInputButtons.Attack);
            bool goJump = Pressed(pressed, SimulationInputButtons.Jump);
            bool goDefend = Pressed(pressed, SimulationInputButtons.Defend);

            if (results.Phase == 201)
            {
                results.Phase = 202;
                results.SettingsCursor = 2;
                world.QueueSound("SFX_004", 400);
            }
            else if (results.Phase >= 210 && results.Phase < 220)
            {
                RunPhase210(
                    results,
                    goLeft,
                    goRight,
                    goUp,
                    goDown,
                    goAttack,
                    goJump);
            }
            else if (results.Phase == 202)
            {
                RunSettingsPhase(results, goLeft, goRight, goAttack, goJump);
            }
            else
            {
                RunSummaryPhase(
                    results,
                    goLeft,
                    goRight,
                    goUp,
                    goDown,
                    goAttack,
                    goJump,
                    goDefend);
            }

            results.Timer++;
            world.SetMode2Request(0);
        }

        private void RunPhase210(
            BattleResultsRuntimeState results,
            bool goLeft,
            bool goRight,
            bool goUp,
            bool goDown,
            bool goAttack,
            bool goJump)
        {
            int editCursor = results.TableCursor;
            bool tableEditAttack = goAttack && editCursor >= 0 && editCursor < 10;
            if (goJump)
            {
                results.RestorePhase210Table();
                results.Phase = 200;
                world.QueueSound("SFX_006", 400);
                return;
            }

            if (tableEditAttack)
                results.ApplyPhase210TableAttack(editCursor);

            if (results.TableCursor >= 0 && results.TableCursor < 10)
            {
                if (goLeft)
                    results.TableCursor--;
                if (goRight)
                    results.TableCursor++;
                if (results.TableCursor < 0)
                    results.TableCursor = 10;
            }
            else if (results.TableCursor == 10)
            {
                if (goDown)
                    results.TableCursor++;
                if (goLeft)
                    results.TableCursor--;
                if (goRight)
                    results.TableCursor = 0;
            }
            else if (results.TableCursor == 11)
            {
                if (goLeft)
                    results.TableCursor -= 2;
                if (goRight)
                    results.TableCursor = 0;
                if (goUp)
                    results.TableCursor--;
            }

            if (!tableEditAttack &&
                goAttack &&
                (results.TableCursor == 10 || results.TableCursor == 11))
            {
                if (results.TableCursor == 11)
                    results.RestorePhase210Table();
                results.Phase = 200;
                world.QueueSound(
                    results.TableCursor == 10 ? "SFX_004" : "SFX_006",
                    400);
            }
        }

        private void RunSettingsPhase(
            BattleResultsRuntimeState results,
            bool goLeft,
            bool goRight,
            bool goAttack,
            bool goJump)
        {
            if (goLeft)
            {
                results.SettingsCursor = (results.SettingsCursor - 1 + 6) % 6;
                world.QueueSound("SFX_002", 400);
            }

            if (goRight)
            {
                results.SettingsCursor = (results.SettingsCursor + 1) % 6;
                world.QueueSound("SFX_002", 400);
            }

            if (goJump)
            {
                results.Phase = 200;
                world.QueueSound("SFX_006", 400);
                return;
            }

            if (!goAttack)
                return;

            results.CommitResultTableValues();
            SyncReserveOwner(results);
            world.QueueSound("SFX_002", 400);
            switch (results.SettingsCursor)
            {
                case 0:
                    ApplyFallDamage(results);
                    results.ResetLiveGuard();
                    results.PendingHostAction =
                        BattleResultsRuntimeState.HostActionRematch;
                    break;
                case 1:
                case 5:
                    results.ResetLiveGuard();
                    results.PendingHostAction =
                        BattleResultsRuntimeState.HostActionBootstrapDirect;
                    break;
                case 2:
                    results.Phase = 201;
                    break;
                case 3:
                    AdvanceStageSelection();
                    break;
                case 4:
                    world.Runtime.Match.Difficulty--;
                    if (world.Runtime.Match.Difficulty < 0)
                        world.Runtime.Match.Difficulty = 2;
                    break;
            }
        }

        private void RunSummaryPhase(
            BattleResultsRuntimeState results,
            bool goLeft,
            bool goRight,
            bool goUp,
            bool goDown,
            bool goAttack,
            bool goJump,
            bool goDefend)
        {
            if (goLeft)
            {
                results.Cursor = (results.Cursor - 1 + 7) % 7;
                world.QueueSound("SFX_002", 400);
            }
            else if (goRight)
            {
                results.Cursor = (results.Cursor + 1) % 7;
                world.QueueSound("SFX_002", 400);
            }

            if (results.Cursor == 0)
            {
                if (goUp || goDown)
                    results.TableSide = 1 - results.TableSide;

                int side = results.TableSide;
                if (goAttack)
                {
                    results.ResultMultiplier[side] += 50;
                    if (results.ResultMultiplier[side] > 300)
                        results.ResultMultiplier[side] = 100;
                }
                if (goJump)
                {
                    results.ResultMultiplier[side] -= 50;
                    if (results.ResultMultiplier[side] < 100)
                        results.ResultMultiplier[side] = 300;
                }
            }

            if (results.Cursor > 0 && results.Cursor < 5)
            {
                int subMax = results.Cursor <= 2 ? 5 : 4;
                if (goDown)
                {
                    results.ResultSubcursor++;
                    if (results.ResultSubcursor > subMax)
                    {
                        results.ResultSubcursor = 0;
                        results.TableSide = 1 - results.TableSide;
                    }
                }
                if (goUp)
                {
                    if (results.ResultSubcursor > subMax)
                        results.ResultSubcursor = subMax;
                    results.ResultSubcursor--;
                    if (results.ResultSubcursor < 0)
                    {
                        results.ResultSubcursor = subMax;
                        results.TableSide = 1 - results.TableSide;
                    }
                }
            }

            if (results.Cursor > 0 &&
                results.Cursor < 5 &&
                (goAttack || goJump || goDefend))
            {
                EditResultValue(results, goAttack, goJump, goDefend);
            }

            if (results.Cursor == 5 && (goUp || goDown))
                results.TableSide = 1 - results.TableSide;

            if (results.Cursor == 5 && goAttack)
            {
                results.Phase = 210;
                results.TableCursor = 10;
                results.SnapshotPhase210Table();
                world.QueueSound("SFX_002", 400);
            }
            else if (results.Cursor == 6 && goAttack)
            {
                results.Phase = 202;
                results.SettingsCursor = 2;
                world.QueueSound("SFX_001", 400);
            }

            if (results.Cursor == 6 && goUp)
            {
                results.Cursor = 5;
                results.TableSide = 0;
            }
            else if (results.Cursor == 6 && goDown)
            {
                results.Cursor = 5;
                results.TableSide = 1;
            }
            if (results.Cursor == 6 && goJump)
            {
                results.Phase = 200;
                world.QueueSound("SFX_006", 400);
            }
        }

        private static void EditResultValue(
            BattleResultsRuntimeState results,
            bool goAttack,
            bool goJump,
            bool goDefend)
        {
            int side = results.TableSide;
            results.ResultSelectedTroop[side] = -1;
            results.ResultSelectedIcon[side] = -1;
            int maxValue = results.Cursor == 1 || results.Cursor == 3 ? 10 : 30;
            int currentValue;
            if (results.Cursor == 1)
            {
                currentValue =
                    results.ResultRow1Values[side, results.ResultSubcursor];
            }
            else if (results.Cursor == 2)
            {
                currentValue =
                    results.ResultRow2Values[side, results.ResultSubcursor];
            }
            else
            {
                int col = results.ResultSubcursor >= 4
                    ? 10
                    : 6 + results.ResultSubcursor;
                currentValue = results.Cursor == 3
                    ? results.ResultRow1Values[side, col]
                    : results.ResultRow2Values[side, col];
            }

            if (goAttack)
                currentValue++;
            if (goDefend)
            {
                currentValue += 5;
                if (currentValue > maxValue && currentValue < maxValue + 5)
                    currentValue = maxValue;
            }
            if (goJump)
                currentValue--;
            if (currentValue < 0)
                currentValue = maxValue;
            if (currentValue > maxValue)
                currentValue = 0;

            if (results.Cursor == 1)
            {
                results.ResultRow1Values[side, results.ResultSubcursor] = currentValue;
            }
            else if (results.Cursor == 2)
            {
                results.ResultRow2Values[side, results.ResultSubcursor] = currentValue;
            }
            else
            {
                int col = results.ResultSubcursor >= 4
                    ? 10
                    : 6 + results.ResultSubcursor;
                if (results.Cursor == 3)
                    results.ResultRow1Values[side, col] = currentValue;
                else
                    results.ResultRow2Values[side, col] = currentValue;
            }
        }

        private void SyncReserveOwner(BattleResultsRuntimeState results)
        {
            BattleRuntimeState battle = world.Runtime;
            battle.ReserveOwnerValid = battle.Match.BattleGameModeId == 4;
            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < 11; col++)
                {
                    battle.ReserveCommittedTotal[side, col] =
                        results.ResultCommittedTotal[side, col];
                    battle.ReserveCommittedHp[side, col] =
                        results.ResultCommittedHp[side, col];
                }
            }
        }

        private void ApplyFallDamage(BattleResultsRuntimeState results)
        {
            foreach (LF2Entity entity in world.ActiveEntitiesByRuntimeSlotForModule)
            {
                if (entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
                {
                    continue;
                }

                entity.FallDamageDiv =
                    results.FallDamageDivForTeam(entity.RelationTeam);
            }
        }

        private void AdvanceStageSelection()
        {
            BattleMatchRuntimeState match = world.Runtime.Match;
            if (match.StageIdx == 0x64)
            {
                match.StageIdx = 0x63;
                match.RandomStage = 0;
                return;
            }

            if (match.StageIdx == 0x63)
            {
                match.StageIdx = 0;
                match.RandomStage = 0;
                return;
            }

            match.StageIdx++;
            if (match.RuntimeStageCount > 0 &&
                match.StageIdx == match.RuntimeStageCount)
            {
                match.StageIdx = 0x64;
                match.RandomStage = 1;
            }
            else
            {
                match.RandomStage = 0;
            }
        }

        private static SimulationInputButtons CollectP1P2PressedButtons(
            FrameInputSet frameInput)
        {
            // Alignment contract: CLIENT-CPP-RESULTS-SCENE-HOST-TICK-ALIGNMENT-001.
            // C++ ORs P1/P2 host just-pressed edges after the world tick.
            if (frameInput?.Players == null)
                return SimulationInputButtons.None;

            SimulationInputButtons pressed = SimulationInputButtons.None;
            for (int index = 0; index < frameInput.Players.Count; index++)
            {
                SimulationPlayerInput player = frameInput.Players[index];
                if (player.PlayerSlot < 0 || player.PlayerSlot > 1)
                    continue;

                pressed |= player.PressedButtons;
            }

            return pressed;
        }

        private static bool Pressed(
            SimulationInputButtons pressed,
            SimulationInputButtons button)
        {
            return (pressed & button) != 0;
        }
    }
}
