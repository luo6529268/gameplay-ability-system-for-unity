#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07HitFa5FullDriverEditorTests
    {
        private const string FormalRuntime =
            "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private const string Scenario =
            "artifacts/diagnostics/NTSD28-Q07-HITFA5-UNITY-DRIVER-001/scenario.json";
        private const string SourceRows =
            "artifacts/diagnostics/NTSD28-Q07-HITFA5-FRAME-COUNTER-FIRST-DIFF-001/run-02/";

        [Test]
        public void ExistingLiveHitFaRepresentativeRoutingRemainsValid()
        {
            MethodInfo check = typeof(BattleRuntimeSelfCheck).GetMethod(
                "CheckCurrentDatFrameLogicSharedRouting",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(check, Is.Not.Null);
            Assert.DoesNotThrow(() => check.Invoke(null, null));
        }

        [Test]
        public void ExplicitZeroVitalsReachPresentationPostInitAndTaskReset()
        {
            var child = new LF2SpecialAttack { ObjectId = 219 };
            var task = new OPointCreateTask
            {
                opoint = new ObjectPoint { oid = 219, kind = 0, action = 0 },
                useExplicitInitialVitals = true,
                initialHp = 0,
                initialMp = 0,
            };
            GameObject host = null;
            try
            {
                host = new GameObject("Q07ExplicitVitalsPostInit")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                host.SetActive(false);
                var factory = host.AddComponent<LF2ObjectPointFactory>();
                MethodInfo postInit = typeof(LF2ObjectPointFactory).GetMethod(
                    "PostInitLivingWithTask",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.That(postInit, Is.Not.Null);
                postInit.Invoke(factory,
                    new object[] { child, null, task.opoint, 3, 0f, false, task });
                Assert.That(child.Health.HP, Is.Zero);
                Assert.That(child.Health.HPBound, Is.Zero);
                Assert.That(child.Health.HP3, Is.Zero);
                Assert.That(child.Health.PP, Is.Zero);

                task.Clear();
                Assert.That(task.useExplicitInitialVitals, Is.False);
                Assert.That(task.initialHp, Is.Zero);
                Assert.That(task.initialMp, Is.Zero);
            }
            finally
            {
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
            }
        }

        [TestCase(1, "positive.csv")]
        [TestCase(3, "negative.csv")]
        public void IndexedHitFa5PreplacedController_MatchesPairedSourceCompleteSession(
            int group, string sourceFile)
        {
            string contentRoot = Path.GetFullPath("Assets/NTSD/Content/LoganRuntime");
            string sourceDat = Path.Combine(contentRoot, "decoded_dat/w/e.dat");
            Assert.That(File.Exists(sourceDat), Is.True, sourceDat);
            var definition = new LF2CharacterDataWrapper(219,
                CharacterAnimtorManager.BuildCharacterDataFromSource(
                    File.ReadAllText(sourceDat), sourceDat,
                    BattleContentSource.ForLoganRuntime(contentRoot)));
            string[][] expected = File.ReadAllLines(SourceRows + sourceFile)
                .Skip(1).Select(row => row.Split(',')).ToArray();
            Assert.That(expected.Length, Is.EqualTo(8));

            NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                FormalRuntime, Scenario, BattleRuntimeProfile.Authority400, 8,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    world.ConfigureFixedViewRunDistance(2048, 1152);
                    SetInitialCharacterPosition(world, 0, 251);
                    SetInitialCharacterPosition(world, 1, 1200);

                    var controller = new LF2SpecialAttack { ObjectId = 219 };
                    controller.FrameCache.Load(definition);
                    controller.ImmediateFrame(51);
                    controller.InitializeNativeDefinitionIdentityForSpawn();
                    controller.InitializeNativeArmorRuntimeFromCurrentDefinitionForSpawn();
                    controller.SetRequiredRuntimeSlot(20);
                    controller.RelationTeam = group;
                    controller.Team = group;
                    controller.OwnerEntityIndex = 0;
                    controller.Health.HP = 500;
                    controller.Runtime.HP = 500;
                    controller.Runtime.MP = 500;
                    controller.Runtime.PP = 500;
                    world.Register(controller);
                    controller.Runtime.SetPosition(100, 0, 542);
                    controller.Runtime.SyncIntegerPosition();
                    controller.Runtime.SetSourceRulePosition(100, 542);
                    controller.Runtime.SyncSourceRuleIntegerPosition();
                    Assert.That(controller.Frame.D?.hit_Fa, Is.EqualTo(5));
                    Assert.That(controller.Runtime.SlotIndex, Is.EqualTo(20));

                    for (int tick = 1; tick <= 8; tick++)
                    {
                        bool advanced;
                        try
                        {
                            advanced = driver.StepOneTick(inputs[tick - 1], true, false);
                        }
                        catch (Exception exception)
                        {
                            throw new InvalidOperationException(
                                "Driver first exception tick=" + tick +
                                ", group=" + group +
                                ", publishedEpochCurrent=" +
                                world.AiUnifiedSnapshotExecutionPublishedEpochIsCurrentForTests +
                                ", slot50Born=" +
                                (world.FindEntityByRuntimeSlotForQuery(50) != null) +
                                ", postCommitBreachCount=" +
                                world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                                exception);
                        }
                        Assert.That(advanced,
                            Is.True, "completed tick=" + tick);
                        Assert.That(
                            world.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics,
                            Is.Zero, "postcommit breach tick=" + tick);
                        string[] row = expected[tick - 1];
                        Assert.That(Parse(row[0]), Is.EqualTo(tick));
                        Assert.That(Parse(row[1]), Is.EqualTo(group));
                        Assert.That(world.FindEntityByRuntimeSlotForQuery(20) != null ? 1 : 0,
                            Is.EqualTo(Parse(row[2])), "controller tick=" + tick);
                        var ally = world.FindEntityByRuntimeSlotForQuery(0);
                        Assert.That(ally, Is.Not.Null, "ally tick=" + tick);
                        Assert.That(ally.Runtime.SourceRuleXInt, Is.EqualTo(Parse(row[3])),
                            "ally X tick=" + tick);
                        Assert.That(ally.Runtime.SourceRuleZInt, Is.EqualTo(Parse(row[4])),
                            "ally Z tick=" + tick);

                        var children = Enumerable.Range(50,
                                world.RuntimeSlotCapacityForDiagnostics - 50)
                            .Select(world.FindEntityByRuntimeSlotForQuery)
                            .Where(entity => entity != null && entity.ObjectId == 219)
                            .ToArray();
                        Assert.That(children.Length, Is.EqualTo(Parse(row[6])),
                            "children tick=" + tick);
                        if (children.Length == 0)
                            continue;
                        var child = children[0];
                        Assert.That(child.Runtime.SlotIndex, Is.EqualTo(Parse(row[7])),
                            "first child slot tick=" + tick);
                        Assert.That(row.Length, Is.EqualTo(15),
                            "paired source counter row shape tick=" + tick);
                        if (group == 1 && tick == 1)
                        {
                            Assert.That(child.Health.HP, Is.Zero,
                                "formal hit_Fa5 child birth HP tick=" + tick);
                        }
                        Assert.That(child.Trans.WaitCounter, Is.EqualTo(Parse(row[14])),
                            "first child action latch tick=" + tick);
                        Assert.That(child.AttackingCounter, Is.EqualTo(Parse(row[13])),
                            "first child frame counter tick=" + tick);
                        Assert.That(child.Frame.D?.frameId, Is.EqualTo(Parse(row[8])),
                            "first child action tick=" + tick);
                        Assert.That(child.Runtime.Vx,
                            Is.EqualTo(ParseDouble(row[11])).Within(0.000001),
                            "first child Vx before position tick=" + tick);
                        Assert.That(child.Runtime.SourceRuleX,
                            Is.EqualTo(ParseDouble(row[9])).Within(0.000001),
                            "first child source X tick=" + tick);
                        Assert.That(child.Runtime.SourceRuleZ,
                            Is.EqualTo(ParseDouble(row[10])).Within(0.000001),
                            "first child source Z tick=" + tick);
                        Assert.That(child.Runtime.ObjectAiTargetSlot3F8,
                            Is.EqualTo(Parse(row[12])),
                            "first child target tick=" + tick);
                    }
                }, useProjectMode: true);
        }

        private static void SetInitialCharacterPosition(
            SimulationWorld world, int slot, int x)
        {
            var entity = world.FindEntityByRuntimeSlotForQuery(slot);
            Assert.That(entity, Is.Not.Null, "initial slot=" + slot);
            entity.Runtime.SetPosition(x, 0, 542);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetSourceRulePosition(x, 542);
            entity.Runtime.SyncSourceRuleIntegerPosition();
        }

        private static int Parse(string value) =>
            int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);

        private static double ParseDouble(string value) =>
            double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}
#endif
