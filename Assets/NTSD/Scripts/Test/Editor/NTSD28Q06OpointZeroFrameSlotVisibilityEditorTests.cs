#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.EditorTools;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OpointZeroFrameSlotVisibilityEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q06-OPOINT-ZERO-FRAME-SLOT-VISIBILITY-001/";
        private const string Source = Root + "source-run1.jsonl";
        private const string SourceSha = "2DA583167F96523BF227DE0BB4388702AD7D09E02202FD044FECEE802E58CB01";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void FullTickBirthOrderMatchesFormalSource(int index)
        {
            using (var input = File.OpenRead(Source))
            using (var sha = SHA256.Create())
            {
                Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", ""),
                    Is.EqualTo(SourceSha));
            }

            JObject source = JObject.Parse(File.ReadAllLines(Source)[index]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                string root = Path.GetFullPath(Root + "fixture-runtime");
                var definitions = new Dictionary<int, LF2CharacterDataWrapper>();
                AddDefinition(779, 0, (string)source["parentDat"]);
                AddDefinition(780, 5, (string)source["childDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(779, 0, "birth-parent.dat"),
                    new ObjectDefinition(780, 5, "birth-child.dat")
                }, oid => definitions.TryGetValue(oid, out var definition) ? definition : null);

                int parentSlot = (int)source["parentSlot"];
                var parent = world.LogicEntityFactory.Create(new OPointCreateTask
                {
                    targetWorld = world,
                    requiredRuntimeSlot = parentSlot,
                    dir = "right",
                    preserveActionZero = true,
                    opoint = new ObjectPoint { oid = 779, action = 0 }
                }, out var failure);
                Assert.That(parent, Is.Not.Null, failure.ToString());
                parent.AiControlled = false;
                parent.Runtime.SetPosition(300, -20, 250);
                parent.Runtime.SyncIntegerPosition();
                parent.Health.HP = 500;
                parent.Health.HPBound = 500;
                parent.Health.HP3 = 500;
                parent.Health.PP = 500;
                parent.FrameDelay = (int)source["initialHold"];
                world.NativeRandom.ResetFromSeed(42);

                var beforeDifferences = Compare(source["before"], Capture(world), "before");
                var tickReports = new JArray();
                for (int tick = 0; tick < ((JArray)source["ticks"]).Count; tick++)
                {
                    var input = new FrameInputSet(tick + 1, Array.Empty<SimulationPlayerInput>());
                    new NTSDBattleTickSystem(world).RunReleaseTick(tick + 1, false, input);
                    JObject actual = Capture(world);
                    JToken expected = source["ticks"][tick]["world"];
                    var differences = Compare(expected, actual, "tick" + tick);
                    tickReports.Add(new JObject
                    {
                        ["tick"] = tick,
                        ["expectedSpawned"] = source["ticks"][tick]["spawned"].DeepClone(),
                        ["actualChildCount"] = ((JArray)actual["entities"]).Count(e => (int)e["oid"] == 780),
                        ["differences"] = new JArray(differences),
                        ["actual"] = actual
                    });
                }

                Directory.CreateDirectory(Root);
                File.WriteAllText(Root + "unity-" + index + ".json", new JObject
                {
                    ["case"] = source["name"].DeepClone(),
                    ["beforeDifferences"] = new JArray(beforeDifferences),
                    ["ticks"] = tickReports
                }.ToString(Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty,
                    "Source/Unity initial state differs: " + string.Join("; ", beforeDifferences));
                foreach (JObject tick in tickReports)
                {
                    var differences = ((JArray)tick["differences"]).Select(v => (string)v).ToArray();
                    Assert.That(differences, Is.Empty,
                        (string)source["name"] + " tick " + (int)tick["tick"] + ": " +
                        string.Join("; ", differences.Take(12)));
                }

                void AddDefinition(int oid, int type, string dat)
                {
                    var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                        Path.Combine(root, "decoded_dat", oid + ".dat"),
                        BattleContentSource.ForLoganRuntime(root));
                    data.type_sub = type;
                    definitions.Add(oid, new LF2CharacterDataWrapper(oid, data));
                }
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason),
                    Is.True, reason);
            }
        }

        [Test]
        public void ComponentMaterializerDoesNotSuppressHeldZeroCounter()
        {
            JObject source = JObject.Parse(File.ReadAllLines(Source)[3]);
            var world = new SimulationWorld();
            GameObject host = null;
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                string root = Path.GetFullPath(Root + "fixture-runtime");
                LF2CharacterDataWrapper Build(int oid, int type, string dat)
                {
                    var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                        Path.Combine(root, "decoded_dat", oid + ".dat"),
                        BattleContentSource.ForLoganRuntime(root));
                    data.type_sub = type;
                    return new LF2CharacterDataWrapper(oid, data);
                }
                var parentData = Build(779, 0, (string)source["parentDat"]);
                var childData = Build(780, 5, (string)source["childDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(779, 0, "birth-parent.dat"),
                    new ObjectDefinition(780, 5, "birth-child.dat")
                }, oid => oid == 779 ? parentData : oid == 780 ? childData : null);
                LF2Entity parent = world.LogicEntityFactory.Create(new OPointCreateTask
                {
                    targetWorld = world,
                    requiredRuntimeSlot = 20,
                    dir = "right",
                    preserveActionZero = true,
                    opoint = new ObjectPoint { oid = 779, action = 0 }
                }, out var failure);
                Assert.That(parent, Is.Not.Null, failure.ToString());
                parent.AiControlled = false;
                parent.FrameDelay = 1;
                Assert.That(parent.AttackingCounter, Is.Zero);
                host = new GameObject("OpointHeldComponentFixture")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                host.SetActive(false);
                var factory = host.AddComponent<LF2ObjectPointFactory>();
                typeof(LF2ObjectPointFactory).GetMethod(
                    "ProcessOpointSpawnCoreForStructuralWriter",
                    BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(factory, new object[] { parent });
                LF2Entity child = world.FindEntityByRuntimeSlotIncludingPending(50);
                Assert.That(child, Is.Not.Null);
                Assert.That(child.ObjectId, Is.EqualTo(780));
                Assert.That(child.AttackingCounter, Is.Zero,
                    "Direct component materialization precedes the high-slot frame pass.");
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason),
                    Is.True, reason);
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
            }
        }

        internal static void VerifyRendererHoldForPlay()
        {
            JObject source = JObject.Parse(File.ReadAllLines(Source)[3]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                string root = Path.GetFullPath(Root + "fixture-runtime");
                LF2CharacterDataWrapper Build(int oid, int type, string dat)
                {
                    var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                        Path.Combine(root, "decoded_dat", oid + ".dat"),
                        BattleContentSource.ForLoganRuntime(root));
                    data.type_sub = type;
                    return new LF2CharacterDataWrapper(oid, data);
                }
                var parentData = Build(779, 0, (string)source["parentDat"]);
                var childData = Build(780, 5, (string)source["childDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(779, 0, "birth-parent.dat"),
                    new ObjectDefinition(780, 5, "birth-child.dat")
                }, oid => oid == 779 ? parentData : oid == 780 ? childData : null);
                LF2Entity parent = world.LogicEntityFactory.Create(new OPointCreateTask
                {
                    targetWorld = world,
                    requiredRuntimeSlot = 20,
                    dir = "right",
                    preserveActionZero = true,
                    opoint = new ObjectPoint { oid = 779, action = 0 }
                }, out var failure);
                Assert.That(parent, Is.Not.Null, failure.ToString());
                parent.AiControlled = false;
                parent.Runtime.SetPosition(300, -20, 250);
                parent.Runtime.SyncIntegerPosition();
                parent.Health.HP = 500;
                parent.Health.HPBound = 500;
                parent.Health.HP3 = 500;
                parent.Health.PP = 500;
                parent.FrameDelay = 2;
                world.NativeRandom.ResetFromSeed(42);
                var before = Compare(source["before"], Capture(world), "before");
                Assert.That(before, Is.Empty, string.Join("; ", before));
                world.SetLogicOnlyEntityMaterialization(false);
                var reports = new JArray();
                for (int tick = 0; tick < 2; tick++)
                {
                    var input = new FrameInputSet(tick + 1,
                        Array.Empty<SimulationPlayerInput>());
                    new NTSDBattleTickSystem(world).RunReleaseTick(tick + 1, false, input);
                    JObject actual = Capture(world);
                    var differences = Compare(source["ticks"][tick]["world"],
                        actual, "renderer.tick" + tick);
                    foreach (JToken row in (JArray)actual["entities"])
                    {
                        if ((int)row["oid"] == 780)
                        {
                            int slot = (int)row["slot"];
                            Assert.That(world.FindEntityByRuntimeSlotIncludingPending(slot)?.Renderer,
                                Is.Not.Null, "Renderer child slot " + slot);
                        }
                    }
                    reports.Add(new JObject
                    {
                        ["tick"] = tick,
                        ["differences"] = new JArray(differences),
                        ["actual"] = actual
                    });
                    Assert.That(differences, Is.Empty,
                        string.Join("; ", differences.Take(12)));
                }
                File.WriteAllText(Root + "renderer-hold-play.json",
                    new JObject { ["beforeDifferences"] = new JArray(before),
                        ["ticks"] = reports }.ToString(Formatting.Indented));
            }
            finally
            {
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                }
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason),
                    Is.True, reason);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        private static JObject Capture(SimulationWorld world)
        {
            var entities = new JArray();
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                if (entity == null)
                    continue;
                int oid = entity.ObjectId;
                if (oid != 779 && oid != 780)
                    continue;
                entities.Add(new JObject
                {
                    ["slot"] = slot,
                    ["oid"] = oid,
                    ["action"] = entity.Frame?.N ?? -1,
                    ["counter"] = entity.AttackingCounter,
                    ["hold"] = entity.FrameDelay
                });
            }
            return new JObject { ["entities"] = entities };
        }

        private static List<string> Compare(JToken expected, JObject actual, string label)
        {
            var differences = new List<string>();
            var expectedEntities = (JArray)expected["entities"];
            var actualEntities = (JArray)actual["entities"];
            if (expectedEntities.Count != actualEntities.Count)
                differences.Add(label + ".entityCount=" + actualEntities.Count + " expected=" + expectedEntities.Count);
            foreach (JToken source in expectedEntities)
            {
                int slot = (int)source["slot"];
                JToken observed = actualEntities.FirstOrDefault(e => (int)e["slot"] == slot);
                if (observed == null)
                {
                    differences.Add(label + ".missingSlot=" + slot);
                    continue;
                }
                foreach (string field in new[] { "oid", "action", "counter", "hold" })
                {
                    int wanted = (int)source[field];
                    int got = (int)observed[field];
                    if (wanted != got)
                        differences.Add(label + ".slot" + slot + "." + field + "=" + got + " expected=" + wanted);
                }
            }
            return differences;
        }
    }
}
#endif
