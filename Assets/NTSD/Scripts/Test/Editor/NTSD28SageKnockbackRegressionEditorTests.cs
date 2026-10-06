#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NTSD.Animation.LF2Objects;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SageKnockbackRegressionEditorTests
    {
        internal const string OutputRoot =
            "artifacts/diagnostics/NTSD28-336B44-SAGE-P2-REGRESSION-20261006/";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";

        [TestCase(false)]
        [TestCase(true)]
        public void ReadySageCloneCompletesDefinitionTransition(bool projected)
        {
            RunSage(projected, false);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NaturalSagePreparationAndDefenseCompletesDefinitionTransition(bool projected)
        {
            RunSage(projected, true);
        }

        [Test]
        public void NaturalP2SageRecallMatchesFormalModeZeroRejection()
        {
            RunSage(true, true, 1);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DelayedFirstSageRecallAfterMovingAwayCompletesDefinitionTransition(bool projected)
        {
            RunSage(projected, true, recallDelay: 60);
        }

        private static void RunSage(bool projected, bool natural, int playerSlot = 0, int recallDelay = 0)
        {
            var rows = new List<object>();
            bool transformed = false;
            bool cloneCreated = false;
            bool readyClone = false;
            int phase = 0;
            int phaseTick = 0;
            int preparedTick = 0;
            string scenario = OutputRoot +
                (playerSlot == 1 ? "p2-natural-sage-scenario.json" :
                    natural ? "natural-sage-scenario.json" : "ready-sage-scenario.json");
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    ContentRoot, scenario, BattleRuntimeProfile.Authority400, 3,
                    (driver, unused, identity) =>
                    {
                        SimulationWorld world = driver.World;
                        Assert.That(world.BattleGameModeId, Is.Zero,
                            "This diagnostic requires the formal mode0 initial condition.");
                        world.Runtime.FunctionKeys.ResetForBattle(true);
                        world.Runtime.Flow.FrameToggle = 0;
                        world.Runtime.Flow.InputPhase = 0;
                        if (projected)
                        {
                            world.ConfigureFixedViewRunDistance(2048, 1152);
                            var initialEntities = new List<LF2Entity>();
                            world.GetAllEntities(initialEntities);
                            foreach (LF2Entity entity in initialEntities)
                            {
                                if (recallDelay > 0)
                                    entity.Runtime.Z = 450;
                                entity.Runtime.SourceRuleX = entity.Runtime.X;
                                entity.Runtime.SourceRuleZ = entity.Runtime.Z;
                                entity.Runtime.SourceRulePositionInitialized = true;
                                entity.Runtime.X = world.SpatialProjection.SourceToViewX(entity.Runtime.X);
                                entity.Runtime.Z = world.SpatialProjection.SourceToViewZ(entity.Runtime.Z);
                                entity.Runtime.SyncIntegerPosition();
                                entity.Runtime.SyncSourceRuleIntegerPosition();
                            }
                        }
                        LF2Entity naruto = world.FindEntityByRuntimeSlotForQuery(playerSlot);
                        Assert.That(naruto.Runtime.OwnerSlotIndex, Is.EqualTo(playerSlot));
                        var entities = new List<LF2Entity>();
                        for (int tick = 1; tick <= (playerSlot == 1 ? 191 : natural ? 600 : 40); tick++)
                        {
                            SimulationInputButtons buttons = SimulationInputButtons.None;
                            if (natural)
                            {
                                if (phase == 0)
                                    buttons = SkillButton(tick);
                                else if (phase == 1)
                                {
                                    if (tick - phaseTick <= 1)
                                        buttons = SimulationInputButtons.Defend;
                                    else
                                        phase = 2;
                                }
                                else if (phase == 3)
                                    buttons = SkillButton(tick - phaseTick);
                                else if (phase == 2 && preparedTick > 0 && recallDelay > 0)
                                {
                                    int sinceReady = tick - preparedTick;
                                    if (sinceReady <= 24)
                                        buttons = SimulationInputButtons.Down;
                                    else if (sinceReady > recallDelay)
                                    {
                                        phase = 3;
                                        phaseTick = tick - 1;
                                        buttons = SkillButton(1);
                                    }
                                }
                                else if (phase == 4)
                                {
                                    if (tick - phaseTick <= 1)
                                        buttons = SimulationInputButtons.Attack;
                                    else
                                        phase = 5;
                                }
                                if (phase == 0 && naruto.Frame.N == 414)
                                {
                                    buttons = SimulationInputButtons.Defend;
                                    phase = 1;
                                    phaseTick = tick;
                                }
                                if (phase == 3 && naruto.Frame.N == 414)
                                {
                                    buttons = SimulationInputButtons.Attack;
                                    phase = 4;
                                    phaseTick = tick;
                                }
                            }
                            Assert.That(driver.StepOneTick(new FrameInputSet(tick,
                                new[]
                                {
                                    new SimulationPlayerInput(0,
                                        playerSlot == 0 ? buttons : SimulationInputButtons.None),
                                    new SimulationPlayerInput(1,
                                        playerSlot == 1 ? buttons : SimulationInputButtons.None)
                                }), ignorePaused: true, buildPresentation: false), Is.True);
                            world.GetAllEntities(entities);
                            LF2Entity clone = entities.FirstOrDefault(value =>
                                value.ObjectId == 96 && value.Runtime.SlotIndex >= 20);
                            cloneCreated |= clone != null;
                            if (natural && phase == 2 && clone != null &&
                                (clone.Frame.N == 323 || clone.Frame.N == 326 ||
                                 clone.Frame.N == 327 || clone.Frame.N == 328 ||
                                 clone.Frame.N == 329))
                            {
                                readyClone = true;
                                if (preparedTick == 0)
                                    preparedTick = tick;
                                if (recallDelay == 0)
                                {
                                    phase = 3;
                                    phaseTick = tick;
                                }
                            }
                            rows.Add(new
                            {
                                tick, phase, buttons = (int)buttons,
                                actors = entities.Select(value => new
                                {
                                    slot = value.Runtime.SlotIndex,
                                    oid = value.ObjectId,
                                    action = value.Frame.N,
                                    state = value.Frame.D?.state ?? -1,
                                    counter = value.Runtime.AttackingCounter,
                                    owner = value.Runtime.OwnerSlotIndex,
                                    x = value.Runtime.SourceRuleX,
                                    y = value.Runtime.Y,
                                    z = value.Runtime.SourceRuleZ,
                                    hp = value.Runtime.HP,
                                    mp = value.Runtime.MP
                                }).ToArray()
                            });
                            if (naruto.ObjectId == 99)
                            {
                                transformed = true;
                                break;
                            }
                        }
                    }, useProjectMode: true);
            }
            catch (Exception exception)
            {
                WriteNew("exception", new { projected, natural, playerSlot, error = exception.ToString() });
                throw;
            }
            finally
            {
                WriteNew((recallDelay > 0 ? "delayed-first" : playerSlot == 1 ? "natural-p2" : natural ? "natural" : "ready") + "-" + projected,
                    new { projected, natural, playerSlot, recallDelay, transformed, cloneCreated, readyClone,
                        scope = "Current approved DAT through complete Unity Driver; legacy scenario schema identity is a fixture token only. Formal EXE comparison is separate.", rows });
            }
            if (natural)
            {
                Assert.That(cloneCreated, Is.True, "Authored input did not create Sage clone OID96.");
                Assert.That(readyClone, Is.True, "Sage clone did not finish authored preparation.");
            }
            Assert.That(transformed, Is.EqualTo(playerSlot == 0),
                "Sage outcome differs from the formal mode0 witness for this player; inspect the retained tick trace.");
        }

        internal static SimulationInputButtons SkillButton(int offset)
        {
            // The existing discrete input contract names D/A/J as Attack/Jump/Defend.
            return offset >= 1 && offset <= 2 ? SimulationInputButtons.Attack :
                offset >= 3 && offset <= 4 ? SimulationInputButtons.Up :
                offset >= 5 && offset <= 6 ? SimulationInputButtons.Jump :
                SimulationInputButtons.None;
        }

        internal static string WriteNew(string label, object value)
        {
            string path = OutputRoot + label + "-" + Guid.NewGuid().ToString("N") + ".json";
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
            return path;
        }
    }
}
#endif
