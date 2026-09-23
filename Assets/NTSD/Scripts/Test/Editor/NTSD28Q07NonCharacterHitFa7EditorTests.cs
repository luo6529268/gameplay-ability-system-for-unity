#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07NonCharacterHitFa7EditorTests
    {
        private const string RuntimeRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string KindScenario =
            "artifacts/diagnostics/NTSD28-R15-KIND-DEPENDENT-SOURCE-PREFLIGHT-001/kind213-action176-to-206-action0-candidate.json";
        private const string FullTickWitnessRoot =
            "artifacts/diagnostics/NTSD28-Q07-HITFA7-TARGET-FULLTICK-WITNESS-001";

        [Test]
        public void MissingPreassignedTargetDoesNotBirthAnExtraClone()
        {
            string output = ProjectPath(
                "Temp/NTSD28UnityTrace/FocusedTests/q07-hitfa7-missing-target.raw.jsonl");
            NTSD28UnityRawCaptureEditor.RunLoganScenarioForTests(
                ProjectPath(RuntimeRoot), ProjectPath(KindScenario), output);

            TickEnvelope tick = JsonUtility.FromJson<TickEnvelope>(
                File.ReadAllLines(output)[1]);
            Assert.That(tick.completedTick, Is.EqualTo(1));
            Assert.That(tick.entities.Select(entity => entity.slot),
                Is.EqualTo(new[] { 1 }));
            Assert.That(tick.entities[0].identity.objectId, Is.EqualTo(213));
            Assert.That(tick.entities[0].frame.action, Is.EqualTo(40));
        }

        [Test]
        public void RealOid875PreassignedTargetUsesOneVerticalStepAndNoClone()
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(
                BattleContentSource.ForLoganRuntime(ProjectPath(RuntimeRoot)));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            Assert.That(configs[875].characterData.frames.Any(frame =>
                frame.frameId == 55 && frame.hit_Fa == 7), Is.True);

            var world = new SimulationWorld();
            var references = new BattleLogicReferencePool();
            world.BindLogicReferencePool(references);
            world.PrepareRuntimeDataCatalogForBattle(
                catalog.Entries.Select(entry =>
                    new ObjectDefinition(entry.Id, entry.Type, entry.DatPath)).ToArray(),
                id => configs.TryGetValue(id, out LF2CharacterDataWrapper wrapper)
                    ? wrapper : null,
                loganCatalog: catalog);
            world.SetLogicOnlyEntityMaterialization(true);

            var target = new LF2Character { ObjectId = 99 };
            target.ModuleInitialize();
            target.SetRequiredRuntimeSlot(0);
            target.ModuleBind(configs[99], 99, world);
            target.Initialize(500, 500);
            target.Team = 2;
            target.Runtime.SetPosition(700, 0, 120);
            target.Runtime.SyncIntegerPosition();

            var subject = new LF2SpecialAttack { ObjectId = 875 };
            subject.FrameCache.Load(configs[875]);
            subject.ImmediateFrame(55);
            subject.SetRequiredRuntimeSlot(50);
            subject.Team = 1;
            subject.Health.HP = 100;
            subject.Runtime.SetPosition(400, -30, 100);
            subject.Runtime.SetVelocity(0, 3.8, 0);
            subject.Runtime.SyncIntegerPosition();
            subject.ObjectAiTargetSlot3F8 = 0;
            world.Register(subject);

            subject.RunFrameLogicBeforeAdvance();

            Assert.That(world.ObjectCount, Is.EqualTo(2));
            Assert.That(subject.Frame.N, Is.EqualTo(55));
            Assert.That(subject.Runtime.Vx, Is.EqualTo(1.4));
            Assert.That(subject.Runtime.Vy, Is.EqualTo(4.2));
            Assert.That(subject.Runtime.Vz, Is.EqualTo(0.4));
            Assert.That(subject.Runtime.Y, Is.EqualTo(-25.8));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-30));

            CharacterMechanics.StepNonCharacterBattleLogic(subject.Runtime, 0.5);
            Assert.That(subject.Runtime.NativePreviousY104, Is.EqualTo(-30));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-30));
            Assert.That(subject.Runtime.Y, Is.EqualTo(-21.6));

            subject.Runtime.SetPosition(400, -30, 100);
            subject.Runtime.SetVelocity(14, 4.2, 2.2);
            subject.Runtime.SyncIntegerPosition();
            subject.RunFrameLogicBeforeAdvance();
            Assert.That(subject.Frame.N, Is.EqualTo(55));
            Assert.That(subject.Runtime.Vx, Is.EqualTo(14));
            Assert.That(subject.Runtime.Vz, Is.EqualTo(2.2));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-30));

            subject.Runtime.SetPosition(400, -20, 100);
            subject.Runtime.SetVelocity(20, 4.2, 3);
            subject.Runtime.SyncIntegerPosition();
            subject.RunFrameLogicBeforeAdvance();
            Assert.That(subject.Frame.N, Is.EqualTo(60));
            Assert.That(subject.Runtime.Y, Is.EqualTo(-15.8));
            Assert.That(subject.Runtime.YInt, Is.EqualTo(-20));
            Assert.That(subject.Runtime.Vx, Is.Zero);
            Assert.That(subject.Runtime.Vy, Is.Zero);
            Assert.That(subject.Runtime.Vz, Is.Zero);
            Assert.That(subject.Runtime.IsFacingLeft, Is.True);
            Assert.That(world.ObjectCount, Is.EqualTo(2));
        }

        [Test]
        public void RealOid875PreassignedTargetMatchesSourceModelFullTicks()
        {
            string sourcePath = ProjectPath(
                FullTickWitnessRoot + "/source-stage23-witness.jsonl");
            string output = ProjectPath(
                "Temp/NTSD28UnityTrace/FocusedTests/q07-hitfa7-target-fulltick.unity.jsonl");
            var sourceRows = File.ReadAllLines(sourcePath)
                .Select(JObject.Parse).ToArray();
            Assert.That(sourceRows.Length, Is.EqualTo(4));

            var unityRows = new List<string>();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                ProjectPath(RuntimeRoot),
                ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                BattleRuntimeProfile.Authority400,
                3,
                (driver, inputs, identity) =>
                {
                    SimulationWorld world = driver.World;
                    var target = world.FindEntityByRuntimeSlotForQuery(0);
                    var subject = world.FindEntityByRuntimeSlotForQuery(1);
                    Assert.That(target?.ObjectId, Is.EqualTo(99));
                    Assert.That(subject?.ObjectId, Is.EqualTo(875));
                    Assert.That(subject?.Frame.N, Is.EqualTo(55));
                    subject.ObjectAiTargetSlot3F8 = 0;
                    subject.Runtime.SetVelocity(0.0, 3.8, 0.0);
                    unityRows.Add(CaptureFullTickRow(world, subject, target, 0));
                    for (int tick = 1; tick <= 2; tick++)
                    {
                        Assert.That(driver.StepOneTick(
                            inputs[tick - 1], ignorePaused: true,
                            buildPresentation: false), Is.True);
                        target = world.FindEntityByRuntimeSlotForQuery(0);
                        subject = world.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(subject, Is.Not.Null);
                        unityRows.Add(CaptureFullTickRow(
                            world, subject, target, tick));
                    }
                });
            }
            catch (Exception exception)
            {
                File.WriteAllLines(output, unityRows);
                File.WriteAllText(output + ".failure.txt", exception.ToString());
                throw;
            }

            File.WriteAllLines(output, unityRows);
            Assert.That(unityRows.Count, Is.EqualTo(3));
            for (int tick = 0; tick < unityRows.Count; tick++)
            {
                JObject unity = JObject.Parse(unityRows[tick]);
                foreach (string field in new[]
                {
                    "completedTick", "targetOid", "subjectOid", "targetSlot",
                    "action", "integerY", "previousY", "entityCount"
                })
                {
                    Assert.That(unity[field].Value<int>(),
                        Is.EqualTo(sourceRows[tick][field].Value<int>()),
                        $"tick {tick}, field {field}");
                }
                foreach (string field in new[]
                {
                    "motionX", "motionY", "motionZ", "preciseY"
                })
                {
                    Assert.That(unity[field].Value<double>(),
                        Is.EqualTo(sourceRows[tick][field].Value<double>())
                            .Within(1e-9),
                        $"tick {tick}, field {field}");
                }
            }
        }

        [Test]
        public void RealOid875PreassignedTargetTick3MaterializesOpoint()
        {
            string output = ProjectPath(
                "Temp/NTSD28UnityTrace/FocusedTests/q07-hitfa7-tick3.unity.jsonl");
            string sourcePath = ProjectPath(
                FullTickWitnessRoot + "/source-stage23-witness.jsonl");
            var sourceRows = File.ReadAllLines(sourcePath)
                .Select(JObject.Parse).ToArray();
            Assert.That(sourceRows.Length, Is.EqualTo(4));

            var unityRows = new List<string>();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            try
            {
                NTSD28UnityRawCaptureEditor.WithLoganScenarioForReplayTests(
                    ProjectPath(RuntimeRoot),
                    ProjectPath(FullTickWitnessRoot + "/scenario.json"),
                    BattleRuntimeProfile.Authority400,
                    3,
                    (driver, inputs, identity) =>
                    {
                        SimulationWorld world = driver.World;
                        var target = world.FindEntityByRuntimeSlotForQuery(0);
                        var subject = world.FindEntityByRuntimeSlotForQuery(1);
                        Assert.That(target?.ObjectId, Is.EqualTo(99));
                        Assert.That(subject?.ObjectId, Is.EqualTo(875));
                        Assert.That(subject?.Frame.N, Is.EqualTo(55));
                        LF2ObjectPool pool = LF2ObjectPool.Instance;
                        if (!pool.IsRuntimeStateValidForAcceptance)
                        {
                            typeof(LF2ObjectPool).GetMethod(
                                "Awake",
                                System.Reflection.BindingFlags.Instance |
                                System.Reflection.BindingFlags.NonPublic)
                                ?.Invoke(pool, null);
                        }
                        Assert.That(pool.IsRuntimeStateValidForAcceptance,
                            Is.True);
                        driver.BeginBattleAllocationSeal();
                        world.SetLogicOnlyEntityMaterialization(true);
                        Assert.That(world.UsesLogicOnlyEntityMaterialization, Is.True);
                        subject.ObjectAiTargetSlot3F8 = 0;
                        subject.Runtime.SetVelocity(0.0, 3.8, 0.0);
                        unityRows.Add(CaptureFullTickRow(world, subject, target, 0));

                        for (int tick = 1; tick <= 3; tick++)
                        {
                            File.WriteAllText(output + ".preconditions.json",
                                new JObject
                                {
                                    ["nextTick"] = tick,
                                    ["worldLogicOnly"] =
                                        world.UsesLogicOnlyEntityMaterialization,
                                    ["subjectRegisteredToWorld"] =
                                        ReferenceEquals(subject.Match, world),
                                    ["subjectAtSlot1"] = ReferenceEquals(
                                        world.FindEntityByRuntimeSlotForQuery(1),
                                        subject),
                                    ["subjectObjectId"] = subject.ObjectId,
                                    ["entityCount"] = world.ObjectCount,
                                }.ToString(Newtonsoft.Json.Formatting.Indented));
                            Assert.That(driver.StepOneTick(
                                inputs[tick - 1], ignorePaused: true,
                                buildPresentation: false), Is.True);
                            target = world.FindEntityByRuntimeSlotForQuery(0);
                            subject = world.FindEntityByRuntimeSlotForQuery(1);
                            Assert.That(subject, Is.Not.Null);
                            unityRows.Add(CaptureFullTickRow(
                                world, subject, target, tick));
                        }
                        BattleRuntimeShutdownReport shutdown =
                            driver.ShutdownBattleRuntime();
                        File.WriteAllText(output + ".shutdown.json",
                            new JObject
                            {
                                ["status"] = shutdown.Status.ToString(),
                                ["completedStage"] =
                                    shutdown.CompletedStage.ToString(),
                                ["failureReason"] = shutdown.FailureReason,
                                ["remainingWorldObjects"] =
                                    shutdown.RemainingWorldObjects,
                                ["remainingRuntimeSlots"] =
                                    shutdown.RemainingRuntimeSlots,
                                ["remainingPoolBorrowers"] =
                                    shutdown.RemainingPoolBorrowers,
                            }.ToString(Newtonsoft.Json.Formatting.Indented));
                    });
            }
            catch (Exception exception)
            {
                File.WriteAllLines(output, unityRows);
                File.WriteAllText(output + ".failure.txt", exception.ToString());
                throw;
            }

            File.WriteAllLines(output, unityRows);
            Assert.That(unityRows.Count, Is.EqualTo(sourceRows.Length));
            JObject unityTick3 = JObject.Parse(unityRows[3]);
            JObject sourceTick3 = sourceRows[3];
            foreach (string field in new[]
            {
                "completedTick", "targetOid", "subjectOid", "targetSlot",
                "action", "integerY", "previousY", "entityCount"
            })
            {
                Assert.That(unityTick3[field].Value<int>(),
                    Is.EqualTo(sourceTick3[field].Value<int>()),
                    $"tick 3, field {field}");
            }
            foreach (string field in new[]
            {
                "motionX", "motionY", "motionZ", "preciseY"
            })
            {
                Assert.That(unityTick3[field].Value<double>(),
                    Is.EqualTo(sourceTick3[field].Value<double>())
                        .Within(1e-9),
                    $"tick 3, field {field}");
            }
        }

        private static string CaptureFullTickRow(
            SimulationWorld world, LF2Entity subject, LF2Entity target,
            int completedTick)
        {
            return new JObject
            {
                ["completedTick"] = completedTick,
                ["targetOid"] = target?.ObjectId ?? -1,
                ["subjectOid"] = subject.ObjectId,
                ["targetSlot"] = subject.ObjectAiTargetSlot3F8,
                ["action"] = subject.Frame.N,
                ["motionX"] = subject.Runtime.Vx,
                ["motionY"] = subject.Runtime.Vy,
                ["motionZ"] = subject.Runtime.Vz,
                ["integerY"] = subject.Runtime.YInt,
                ["preciseY"] = subject.Runtime.Y,
                ["previousY"] = subject.Runtime.NativePreviousY104,
                ["entityCount"] = world.ObjectCount,
            }.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static string ProjectPath(string path)
        {
            return Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
        }

        [Serializable]
        private sealed class TickEnvelope
        {
            public int completedTick;
            public EntityEnvelope[] entities;
        }

        [Serializable]
        private sealed class EntityEnvelope
        {
            public int slot;
            public IdentityEnvelope identity;
            public FrameEnvelope frame;
        }

        [Serializable]
        private sealed class IdentityEnvelope
        {
            public int objectId;
        }

        [Serializable]
        private sealed class FrameEnvelope
        {
            public int action;
        }
    }
}
#endif
