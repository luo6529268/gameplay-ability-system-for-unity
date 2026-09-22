#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("NTSD28Q08BattleFlowRedProbe")]
    public sealed class NTSD28Q08BattleFlowRedProbeEditorTests
    {
        [Test]
        public void ZeroHpWithOneNativeLifeStillStartsCurrentTerminalGuard()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            Register(world, 0, 1);
            var second = Register(world, 1, 2);

            world.UpdateBattleResultsFlow();
            Assert.That(world.Runtime.Results.HadBoth, Is.True);
            second.Health.HP = 0;
            second.HP2Orig = 1;
            world.UpdateBattleResultsFlow();

            Assert.That(world.Runtime.Results.BattleEndPhase, Is.EqualTo(1),
                "HP0 with only one native life must remain terminal at this boundary.");
        }

        [Test]
        public void FullTickObservesZeroHpWithTwoNativeLivesBeforeCombatTail()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            Register(world, 0, 1);
            var second = Register(world, 1, 2);
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(1, buildPresentation: false);
            Assert.That(world.Runtime.Results.HadBoth, Is.True,
                "Fixture must establish two living groups after the first full tick.");

            second.Health.HP = 0;
            second.HP2Orig = 2;
            tickSystem.RunReleaseTick(2, buildPresentation: false);

            Assert.That(world.Runtime.Results.BattleEndPhase, Is.Zero,
                "Native pre-combat BattleFlow28 keeps the revive2 group alive.");
        }

        [Test]
        public void ZeroHpWithTwoNativeLivesKeepsTwoGroupsAlive()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            var first = Register(world, 0, 1);
            var second = Register(world, 1, 2);

            world.UpdateBattleResultsFlow();
            Assert.That(world.Runtime.Results.HadBoth, Is.True,
                "Fixture must first establish two living groups.");
            Assert.That(world.Runtime.Results.BattleEndPhase, Is.Zero);

            second.Health.HP = 0;
            second.HP2Orig = 2;
            world.UpdateBattleResultsFlow();

            Assert.That(world.Runtime.Results.BattleEndPhase, Is.Zero,
                "Formal battle_flow_tests: HP0 with revive_lives_30c=2 remains living, timer=0.");
        }

        [Test]
        public void ModeOneDirectBattleStillStartsTerminalGuard()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            Register(world, 0, 1);
            var second = Register(world, 1, 2);
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(1, buildPresentation: false);
            Assert.That(world.StageProgressionValid, Is.False);
            Assert.That(world.Runtime.Results.HadBoth, Is.True);
            second.Health.HP = 0;
            second.HP2Orig = 1;
            tickSystem.RunReleaseTick(2, buildPresentation: false);

            Assert.That(world.Runtime.Results.BattleEndPhase, Is.EqualTo(1),
                "Mode 1 alone must not suppress the direct battle result path.");
        }

        [Test]
        public void ModeOneConfiguredStageWithoutStorySelectorKeepsOrdinaryGuard()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            var campaigns = BattleStageCampaignLoader.ParseText(
                "<stage> id: 12\n<phase> bound: 900\n<phase_end>\n<stage_end>\n");
            Assert.That(world.ConfigureStageCampaigns(campaigns, 12, -1), Is.True);
            Assert.That(world.StageProgressionValid, Is.True);
            Assert.That(world.StartInitialStageWave(), Is.True);
            Register(world, 0, 1);
            var second = Register(world, 1, 2);
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(1, buildPresentation: false);
            Assert.That(world.Runtime.Results.HadBoth, Is.True);
            second.Health.HP = 0;
            second.HP2Orig = 1;
            tickSystem.RunReleaseTick(2, buildPresentation: false);

            Assert.That(world.Runtime.Results.BattleEndPhase, Is.EqualTo(1),
                "A campaign alone is not the formal paired story mission/child selection.");
        }

        [Test]
        public void NativePrecombatResultLatchesFirstTerminalGroupsAcrossRevival()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            Register(world, 0, 1);
            var second = Register(world, 1, 2);
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(1, buildPresentation: false);
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.Zero);
            Assert.That(world.Runtime.Results.NativeLivingGroupMask,
                Is.EqualTo((1UL << 1) | (1UL << 2)));

            second.Health.HP = 0;
            second.HP2Orig = 1;
            tickSystem.RunReleaseTick(2, buildPresentation: false);
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.EqualTo(1));
            Assert.That(world.Runtime.Results.NativeLivingGroupMask, Is.EqualTo(1UL << 1));

            second.Health.HP = 500;
            tickSystem.RunReleaseTick(3, buildPresentation: false);
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.EqualTo(2));
            Assert.That(world.Runtime.Results.NativeLivingGroupMask, Is.EqualTo(1UL << 1));
        }

        [Test]
        public void NativePrecombatResultRetainsThirdGroupAndExcludesInvalidGroups()
        {
            var threeGroupWorld = new SimulationWorld();
            Register(threeGroupWorld, 0, 1);
            Register(threeGroupWorld, 1, 2);
            Register(threeGroupWorld, 2, 3);
            new NTSDBattleTickSystem(threeGroupWorld).RunReleaseTick(
                1, buildPresentation: false);
            Assert.That(threeGroupWorld.Runtime.Results.NativeResultTimer, Is.Zero);
            Assert.That(threeGroupWorld.Runtime.Results.NativeLivingGroupMask,
                Is.EqualTo((1UL << 1) | (1UL << 2) | (1UL << 3)));

            var invalidWorld = new SimulationWorld();
            Register(invalidWorld, 0, 1);
            Register(invalidWorld, 1, 0);
            Register(invalidWorld, 2, 5);
            Register(invalidWorld, 3, 40);
            new NTSDBattleTickSystem(invalidWorld).RunReleaseTick(
                1, buildPresentation: false);
            Assert.That(invalidWorld.Runtime.Results.NativeResultTimer, Is.EqualTo(1));
            Assert.That(invalidWorld.Runtime.Results.NativeLivingGroupMask,
                Is.EqualTo(1UL << 1));
        }

        [Test]
        public void NativeResultTimerEmitsSourceMilestonesAndClearsStored350()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            Register(world, 0, 1);
            var tickSystem = new NTSDBattleTickSystem(world);
            for (int tick = 1; tick <= 350; tick++)
            {
                tickSystem.RunReleaseTick(tick, buildPresentation: false);
                if (tick != 79 && tick != 80 && tick != 100 &&
                    tick != 101 && tick != 349 && tick != 350)
                {
                    continue;
                }

                BattleResultsRuntimeState results = world.Runtime.Results;
                Assert.That(results.NativeResultOutputTimer, Is.EqualTo(tick));
                Assert.That(results.NativeResultPhase,
                    Is.EqualTo(tick < 80 ? 0 : tick < 101 ? 1 : tick < 350 ? 2 : 3));
            }

            Assert.That(world.Runtime.Results.NativeResultTimer, Is.Zero);
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(2));
        }

        [Test]
        public void NativeResultContinueUsesHeldAttackAt144NotEarlier()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 1;
            Register(world, 0, 1);
            world.Runtime.Roster.Slots[0].Active = true;
            world.Runtime.Results.NativeResultTimer = 142;
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(1, false, new FrameInputSet(1, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.Attack),
            }));
            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(143));
            Assert.That(world.Runtime.Results.NativeResultPhase, Is.EqualTo(2));

            tickSystem.RunReleaseTick(2, false, new FrameInputSet(2, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.Attack),
            }));
            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(350));
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.Zero);
            Assert.That(world.Runtime.Results.NativeResultPhase, Is.EqualTo(3));
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(2));
        }

        [Test]
        public void NativeResultContinueAcceptsThirdParticipantButNotInactiveOrPressedOnly()
        {
            var activeWorld = new SimulationWorld();
            Register(activeWorld, 0, 1);
            activeWorld.Runtime.Roster.Slots[2].Active = true;
            activeWorld.Runtime.Results.NativeResultTimer = 143;
            new NTSDBattleTickSystem(activeWorld).RunReleaseTick(1, false,
                new FrameInputSet(1, new[]
                {
                    new SimulationPlayerInput(2, SimulationInputButtons.Jump),
                }));
            Assert.That(activeWorld.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(350));

            var inactiveWorld = new SimulationWorld();
            Register(inactiveWorld, 0, 1);
            inactiveWorld.Runtime.Results.NativeResultTimer = 143;
            new NTSDBattleTickSystem(inactiveWorld).RunReleaseTick(1, false,
                new FrameInputSet(1, new[]
                {
                    new SimulationPlayerInput(2, SimulationInputButtons.Attack),
                }));
            Assert.That(inactiveWorld.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(144));

            var edgeWorld = new SimulationWorld();
            Register(edgeWorld, 0, 1);
            edgeWorld.Runtime.Roster.Slots[0].Active = true;
            edgeWorld.Runtime.Results.NativeResultTimer = 143;
            new NTSDBattleTickSystem(edgeWorld).RunReleaseTick(1, false,
                new FrameInputSet(1, new[]
                {
                    new SimulationPlayerInput(
                        0,
                        SimulationInputButtons.None,
                        SimulationInputButtons.Attack),
                }));
            Assert.That(edgeWorld.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(144));
        }

        [Test]
        public void ModeFourTransitionStopsCombatWorldOn350AndFollowingTick()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 4;
            Register(world, 0, 1);
            var victim = Register(world, 1, 2);
            victim.Health.HP = 0;
            victim.HP2Orig = 1;
            var tickSystem = new NTSDBattleTickSystem(world);

            for (int tick = 1; tick <= 349; tick++)
                tickSystem.RunReleaseTick(tick, buildPresentation: false);

            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(349));
            ulong frameSequenceBefore = world.Runtime.NativeWorldClock.FrameSequence;
            tickSystem.RunReleaseTick(350, buildPresentation: false);
            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(350));
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(202));
            Assert.That(world.Runtime.NativeWorldClock.FrameSequence,
                Is.EqualTo(frameSequenceBefore),
                "Formal GameSession skips the combat driver on the transition tick.");

            tickSystem.RunReleaseTick(351, buildPresentation: false);
            Assert.That(world.Runtime.NativeWorldClock.FrameSequence,
                Is.EqualTo(frameSequenceBefore),
                "The following upper-state tick must leave the combat world frozen.");
        }

        [Test]
        public void OrdinaryTransitionChangesCommandTwoToOneWithoutAdvancingOldCombatWorld()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 0;
            Register(world, 0, 1);
            var victim = Register(world, 1, 2);
            victim.Health.HP = 0;
            victim.HP2Orig = 1;
            world.Runtime.Results.NativeResultTimer = 349;
            var tickSystem = new NTSDBattleTickSystem(world);
            ulong frameSequenceBefore = world.Runtime.NativeWorldClock.FrameSequence;

            tickSystem.RunReleaseTick(350, buildPresentation: false);
            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(350));
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(2));
            Assert.That(world.Runtime.NativeWorldClock.FrameSequence,
                Is.EqualTo(frameSequenceBefore));

            tickSystem.RunReleaseTick(351, buildPresentation: false);
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(1));
            Assert.That(world.Runtime.NativeWorldClock.FrameSequence,
                Is.EqualTo(frameSequenceBefore));
        }

        [Test]
        public void TransitionTickDoesNotRunOldResultsSettingsWriter()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 4;
            Register(world, 0, 1);
            var victim = Register(world, 1, 2);
            victim.Health.HP = 0;
            victim.HP2Orig = 1;
            world.Runtime.Results.NativeResultTimer = 349;
            world.Runtime.Results.Phase = 202;
            world.Runtime.Results.SettingsCursor = 0;
            var tickSystem = new NTSDBattleTickSystem(world);

            tickSystem.RunReleaseTick(350, false, new FrameInputSet(350, new[]
            {
                new SimulationPlayerInput(
                    1,
                    SimulationInputButtons.Attack,
                    SimulationInputButtons.Attack),
            }));

            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(202));
            Assert.That(world.Runtime.Results.PendingHostAction,
                Is.EqualTo(BattleResultsRuntimeState.HostActionNone),
                "The old Results settings writer must not edit the frozen battle after transition.");
            Assert.That(world.Runtime.Results.Phase, Is.EqualTo(202));
        }

        private static FlowEntity Register(SimulationWorld world, int slot, int group)
        {
            var entity = new FlowEntity();
            entity.Name = "Q08Flow" + slot;
            entity.ObjectId = 900 + slot;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                entity.ObjectId,
                new LF2CharacterData
                {
                    name = entity.Name,
                    frames = new List<LF2FrameData>
                    {
                        new LF2FrameData
                        {
                            frameId = 0,
                            state = 0,
                            wait = 3,
                            next = 0,
                            centerx = 39,
                            centery = 79,
                        },
                    },
                }));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.PN = 0;
            entity.Frame.N = 0;
            entity.RelationTeam = group;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private sealed class FlowEntity : LF2Entity
        {
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

            public FlowEntity()
            {
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
            }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;

            public override void Reset() { }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }
    }
}
#endif
