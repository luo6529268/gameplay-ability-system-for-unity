#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.Text;
using NTSD.Animation.LF2Objects;
using NTSD.Game;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q08DirectBattleNaturalKoTwoCyclePlayModeTests
    {
        [UnityTest]
        [Timeout(240000)]
        public IEnumerator PhysicalPunchKoDrivesOneRematchThenSameWorldSelection()
        {
            EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
            yield return new EnterPlayMode();

            SimulationTickDriver driver = null;
            for (int second = 0; second < 120; second++)
            {
                driver = SimulationTickDriver.Instance;
                if (driver != null &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World != null && driver.World.ObjectCount > 0 &&
                    driver.CurrentTickIndex >= 2)
                    break;
                yield return new WaitForSecondsRealtime(1f);
            }

            Assert.That(driver, Is.Not.Null);
            Assert.That(driver.World, Is.Not.Null);
            Assert.That(driver.World.Runtime.Roster.ActiveSlotCount, Is.GreaterThanOrEqualTo(2));
            Keyboard keyboard = Keyboard.current;
            Assert.That(keyboard, Is.Not.Null);

            SimulationWorld firstWorld = driver.World;
            AdvanceToNaturalResult(driver, firstWorld, keyboard);
            for (int frame = 0; frame < 10 &&
                 ReferenceEquals(driver.World, firstWorld); frame++)
                yield return null;

            Assert.That(driver.World, Is.Not.SameAs(firstWorld),
                "The first real KO did not cause the one direct rematch.");
            Assert.That(driver.World.ObjectCount, Is.GreaterThan(0));
            Assert.That(driver.World.Runtime.Roster.ActiveSlotCount, Is.GreaterThanOrEqualTo(2));
            Assert.That(firstWorld.ObjectCount, Is.Zero,
                "The first World was not cleared by ordered rematch shutdown.");

            SimulationWorld secondWorld = driver.World;
            int rematchTick = driver.CurrentTickIndex;
            for (int second = 0; second < 5 &&
                 driver.CurrentTickIndex <= rematchTick; second++)
                yield return new WaitForSecondsRealtime(1f);
            Assert.That(driver.CurrentTickIndex, Is.GreaterThan(rematchTick),
                "The recreated battle did not resume before its second collision fixture.");
            AdvanceToNaturalResult(driver, secondWorld, keyboard);
            int frozenTick = driver.CurrentTickIndex;
            for (int frame = 0; frame < 5; frame++)
                yield return null;

            Assert.That(driver.World, Is.SameAs(secondWorld),
                "The second real KO must not create another direct rematch.");
            Assert.That(driver.CurrentTickIndex, Is.EqualTo(frozenTick));
            Assert.That(secondWorld.Runtime.Results.NativeTransitionState, Is.EqualTo(1),
                "The second result did not enter logical upper selection.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }
            if (Application.isPlaying)
                yield return new ExitPlayMode();
        }

        private static void AdvanceToNaturalResult(
            SimulationTickDriver driver, SimulationWorld world, Keyboard keyboard)
        {
            driver.SetPaused(true);
            Assert.That(driver.World, Is.SameAs(world));
            LF2Character attacker = world.FindEntityByRuntimeSlotForQuery(0) as LF2Character;
            LF2Character victim = world.FindEntityByRuntimeSlotForQuery(1) as LF2Character;
            CharacterInputModule input = attacker?.Controller as CharacterInputModule;
            Assert.That(attacker?.Runtime.SourceRulePositionInitialized, Is.True,
                "Direct Battle attacker lacks source-rule birth position.");
            Assert.That(victim?.Runtime.SourceRulePositionInitialized, Is.True,
                "Direct Battle victim lacks source-rule birth position.");
            Assert.That(attacker?.ObjectId, Is.EqualTo(2));
            Assert.That(victim?.ObjectId, Is.EqualTo(2));
            Assert.That(attacker.RelationTeam, Is.EqualTo(1));
            Assert.That(victim.RelationTeam, Is.EqualTo(2));
            Assert.That(input?.AttackAction?.enabled, Is.True);
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.Zero);
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.Zero);

            int direction = attacker.Runtime.IsFacingLeft ? -1 : 1;
            victim.Runtime.SetPosition(attacker.Runtime.XInt + direction * 40,
                attacker.Runtime.YInt, attacker.Runtime.ZInt);
            victim.Runtime.SyncIntegerPosition();
            victim.Health.HP = 10;
            victim.Health.HPBound = 10;
            victim.Health.HP3 = 10;
            victim.HP2Orig = 1;
            victim.HitStun = 0;
            victim.AttackExempt = 0;
            victim.Runtime.LinkState = 0;
            victim.ItrRest.Reset();
            victim.RefreshRuntimeSnapshot();

            bool physicalAttackSeen = false;
            bool authoredPunchSeen = false;
            bool knockoutSeen = false;
            var trace = new StringBuilder(4096);
            trace.Append("mode=").Append(world.BattleGameModeId)
                .Append(" inputPhase=").Append(world.InputPhase)
                .Append(" initialA=").Append(attacker.Runtime.XInt).Append(',')
                .Append(attacker.Runtime.YInt).Append(',')
                .Append(attacker.Runtime.ZInt).Append(" facingLeft=")
                .Append(attacker.Runtime.IsFacingLeft)
                .Append(" initialV=").Append(victim.Runtime.XInt).Append(',')
                .Append(victim.Runtime.YInt).Append(',')
                .Append(victim.Runtime.ZInt).AppendLine();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.J));
                InputSystem.Update();
                for (int count = 0; count < 40; count++)
                {
                    Assert.That(driver.StepOneTick(ignorePaused: true,
                        buildPresentation: false), Is.True);
                    if (count == 1)
                    {
                        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                        InputSystem.Update();
                    }

                    FrameInputSet applied = driver.LastAppliedFrameInput;
                    if (applied?.Players != null)
                    {
                        foreach (SimulationPlayerInput player in applied.Players)
                        {
                            if (player.PlayerSlot == 0 &&
                                (player.Buttons & SimulationInputButtons.Jump) != 0)
                                physicalAttackSeen = true;
                        }
                    }
                    authoredPunchSeen |= attacker.Frame.N == 60 ||
                        attacker.Frame.N == 62 || attacker.Frame.N == 65 ||
                        attacker.Frame.N == 513;
                    trace.Append("tick=").Append(driver.CurrentTickIndex)
                        .Append(" aFrame=").Append(attacker.Frame.N)
                        .Append(" aX=").Append(attacker.Runtime.XInt)
                        .Append(" aZ=").Append(attacker.Runtime.ZInt)
                        .Append(" aItr=").Append(attacker.Frame.D?.itrs.Count ?? -1)
                        .Append(" vFrame=").Append(victim.Frame.N)
                        .Append(" vX=").Append(victim.Runtime.XInt)
                        .Append(" vZ=").Append(victim.Runtime.ZInt)
                        .Append(" vBdy=").Append(victim.Frame.D?.bodies.Count ?? -1)
                        .Append(" vHP=").Append(victim.Health.HP)
                        .AppendLine();
                    if (victim.Health.HP > 0)
                        continue;

                    int eventTime = unchecked((int)(world.NativeFrameSequence - 1UL));
                    foreach (NativeKnockoutEvent knockout in world.NativeKnockoutEvents)
                    {
                        if (knockout.VictimSlot == victim.Runtime.SlotIndex &&
                            knockout.BattleTimeTick == eventTime &&
                            (knockout.CreditSlot == attacker.Runtime.SlotIndex ||
                             knockout.FourOwnerSlot == attacker.Runtime.SlotIndex))
                            knockoutSeen = true;
                    }
                    break;
                }
            }
            finally
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }

            Assert.That(physicalAttackSeen, Is.True,
                "The P1 physical J edge did not enter the canonical tick input.");
            Assert.That(authoredPunchSeen, Is.True,
                "The P1 attack edge did not select the authored Naruto punch.\n" + trace);
            Assert.That(victim.Health.HP, Is.LessThanOrEqualTo(0),
                "The live Naruto attack did not knock out the opposing participant.\n" + trace);
            Assert.That(knockoutSeen, Is.True);
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.Zero);
            Assert.That(driver.StepOneTick(ignorePaused: true,
                buildPresentation: false), Is.True);
            Assert.That(world.Runtime.Results.NativeResultTimer, Is.EqualTo(1));
            for (int timer = 2; timer <= 350; timer++)
            {
                Assert.That(driver.StepOneTick(ignorePaused: true,
                    buildPresentation: false), Is.True,
                    "Natural result timer stopped at " + timer);
            }
            Assert.That(world.Runtime.Results.NativeResultOutputTimer, Is.EqualTo(350));
            Assert.That(world.Runtime.Results.NativeResultPhase, Is.EqualTo(3));
            Assert.That(world.Runtime.Results.NativeTransitionState, Is.EqualTo(2));
        }
    }
}
#endif
