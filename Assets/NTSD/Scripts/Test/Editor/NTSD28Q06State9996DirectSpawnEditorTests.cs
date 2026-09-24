#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.EditorTools;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06State9996DirectSpawnEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-STATE9996-DIRECT-SPAWN-TRANSACTION-001/";
        private const string SourceFinal = Output + "source-final/first.jsonl";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NativeDirectSpawnImmediateMatchesSource(int index)
        {
            var row = JObject.Parse(File.ReadAllLines(SourceFinal)[index]);
            var beforeDifferences = new List<string>();
            var immediateDifferences = new List<string>();
            var world = CreateWorld(row);
            try
            {
                var before = Capture(world);
                Compare(row["before"], before, "before", beforeDifferences);
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                InvokeNativeCloneBoundary(world);
                var after = Capture(world);
                Compare(row["after"], after, "immediate", immediateDifferences);
                CompareJson(row["calls"], observer.Capture(), "calls", immediateDifferences);
                Directory.CreateDirectory(Output + "unity-immediate");
                File.WriteAllText(Output + "unity-immediate/case-" + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, name = (string)row["name"], beforeDifferences, immediateDifferences,
                    before, after, calls = observer.Capture(), sourceResult = row["result"],
                    sourceCapacity = (int)row["config"]["capacity"], unityCapacity = world.RuntimeSlotCapacityForDiagnostics,
                    capacityNormalized = index == 6 || index == 7,
                    requestedDynamicFreeSlots = (int)row["params"]["freeSlots"],
                    scope = "Synthetic immediate private native boundary only; pending case probes boundary, not outer caller reachability. Encoded 8M is unresolved no-op control. Capacity fillers excluded from tuple parity; noncapacity counts exact. No following, renderer or EXE claim."
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(30)));
                Assert.That(immediateDifferences, Is.Empty, string.Join("\n", immediateDifferences.Take(40)));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        [TestCase(1333)]
        [TestCase(2048)]
        public void State9996ChildAfterParentMotion_PreservesFormalScreenFraction(
            int referenceWidth)
        {
            var row = JObject.Parse(File.ReadAllLines(SourceFinal)[0]);
            var world = CreateWorld(row);
            try
            {
                world.ConfigureFixedViewRunDistance(referenceWidth,
                    referenceWidth == 1333 ? 730 : 1152);
                LF2Entity parent = world.FindEntityByRuntimeSlotIncludingPending(0);
                Assert.That(parent.Runtime.XInt, Is.EqualTo(300));
                Assert.That((int)row["after"]["entities"][1]["raw"]["position"]["x"] - 300,
                    Is.EqualTo(-3));

                parent.Runtime.SetVelocity(20, 0, 0);
                new CharacterMechanics().StepBattleLogic(
                    new CharacterMechanicsContext(parent.Runtime, null, 0f, 0f, 0.0,
                        world.FixedViewRunDistanceScale,
                        world.FixedViewRunVerticalDistanceScale));
                parent.Runtime.XInt = (int)parent.Runtime.X;
                Assert.That(parent.Runtime.XInt,
                    Is.EqualTo(300 + (int)(20 * world.FixedViewRunDistanceScale)));

                InvokeNativeCloneBoundary(world);
                for (int spawnIndex = 0; spawnIndex < 5; spawnIndex++)
                {
                    LF2Entity child = world.FindEntityByRuntimeSlotIncludingPending(50 + spawnIndex);
                    Assert.That(child, Is.Not.Null);
                    Assert.That(child.ObjectId, Is.EqualTo(spawnIndex == 4 ? 218 : 217));
                    int formalChildOffset = (int)row["after"]["entities"][spawnIndex + 1]
                        ["raw"]["position"]["x"] - 300;
                    double expectedX = parent.Runtime.XInt +
                        formalChildOffset * world.FixedViewRunDistanceScale;
                    double expectedZ = parent.Runtime.ZInt +
                        world.FixedViewRunVerticalDistanceScale;
                    Assert.That(child.Runtime.X,
                        Is.EqualTo(expectedX).Within(1e-10));
                    Assert.That(child.Runtime.XInt, Is.EqualTo((int)expectedX));
                    Assert.That(child.Runtime.Z,
                        Is.EqualTo(expectedZ).Within(1e-10));
                    Assert.That(child.Runtime.ZInt, Is.EqualTo((int)expectedZ));
                    Assert.That((child.Runtime.X - parent.Runtime.XInt) / referenceWidth,
                        Is.EqualTo(formalChildOffset / 1333.0).Within(1e-12));
                    Assert.That((child.Runtime.Z - parent.Runtime.ZInt) /
                        (referenceWidth == 1333 ? 730.0 : 1152.0),
                        Is.EqualTo(1.0 / 730.0).Within(1e-12));
                }
            }
            finally
            {
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void FiveChildBirthUsesIndependentSourceIntegerPosition(bool initialized, bool configuredView)
        {
            var row = JObject.Parse(File.ReadAllLines(SourceFinal)[0]);
            var world = CreateWorld(row);
            try
            {
                if (configuredView) world.ConfigureFixedViewRunDistance(2048, 1152);
                LF2Entity parent = world.FindEntityByRuntimeSlotIncludingPending(0);
                if (initialized)
                {
                    parent.Runtime.SetSourceRulePosition(-100.75, 87.5);
                    parent.Runtime.SourceRuleXInt = -102;
                    parent.Runtime.SourceRuleZInt = 85;
                }
                int countBefore = world.ObjectCount;
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                InvokeNativeCloneBoundary(world);
                Assert.That(JToken.DeepEquals(row["calls"], observer.Capture()), Is.True, "Formal RNG sequence");
                var randomState = JObject.FromObject(ProjectRandom.Invoke(null,
                    new object[] { world.NativeRandom.CaptureScalarState() }));
                Assert.That(JToken.DeepEquals(row["after"]["random"], randomState), Is.True, "Formal final RNG state");
                Assert.That(world.ObjectCount - countBefore, Is.EqualTo(5));
                for (int i = 0; i < 5; i++)
                {
                    LF2Entity child = world.FindEntityByRuntimeSlotIncludingPending(50 + i);
                    Assert.That(child, Is.Not.Null);
                    Assert.That(child.ObjectId, Is.EqualTo(i == 4 ? 218 : 217));
                    int dx = (int)row["after"]["entities"][i + 1]["raw"]["position"]["x"] - 300;
                    double battleX = parent.Runtime.XInt + dx * world.FixedViewRunDistanceScale;
                    double battleZ = parent.Runtime.ZInt + world.FixedViewRunVerticalDistanceScale;
                    Assert.That(child.Runtime.X, Is.EqualTo(battleX).Within(1e-10));
                    Assert.That(child.Runtime.XInt, Is.EqualTo((int)battleX));
                    Assert.That(child.Runtime.Z, Is.EqualTo(battleZ).Within(1e-10));
                    Assert.That(child.Runtime.ZInt, Is.EqualTo((int)battleZ));
                    Assert.That(child.Runtime.SourceRulePositionInitialized, Is.EqualTo(initialized));
                    Assert.That(child.Runtime.SourceRuleX, Is.EqualTo(initialized ? -102 + dx : 0));
                    Assert.That(child.Runtime.SourceRuleZ, Is.EqualTo(initialized ? 86 : 0));
                    Assert.That(child.Runtime.SourceRuleXInt, Is.EqualTo(initialized ? -102 + dx : 0));
                    Assert.That(child.Runtime.SourceRuleZInt, Is.EqualTo(initialized ? 86 : 0));
                }
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        [TestCase(0)]
        [TestCase(2)]
        public void NativeDirectSpawnFollowingTickMatchesSource(int index)
        {
            VerifyContinuation(index, false);
        }

        internal static void VerifyRendererBirthForPlay(int index)
        {
            Assert.That(index == 0 || index == 2, Is.True);
            VerifyContinuation(index, true);
        }

        private static void VerifyContinuation(int index, bool renderer)
        {
            var row = JObject.Parse(File.ReadAllLines(SourceFinal)[index]);
            var beforeDifferences = new List<string>();
            var immediateDifferences = new List<string>();
            var followingDifferences = new List<string>();
            var world = CreateWorld(row);
            try
            {
                if (renderer) world.SetLogicOnlyEntityMaterialization(false);
                var before = Capture(world);
                Compare(row["before"], before, "before", beforeDifferences);
                var immediateObserver = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(immediateObserver);
                InvokeNativeCloneBoundary(world);
                var immediate = Capture(world);
                Compare(row["after"], immediate, "immediate", immediateDifferences);
                CompareJson(row["calls"], immediateObserver.Capture(), "immediate.calls", immediateDifferences);
                var externalControlSlots = new List<int>();
                JObject following = null;
                JObject followingCalls = null;
                if (renderer)
                {
                    for (int slot = 50; slot < 55; slot++)
                        Assert.That(world.FindEntityByRuntimeSlotIncludingPending(slot)?.Renderer, Is.Not.Null,
                            "Renderer materialization at slot " + slot);
                }
                else
                {
                    // Match source native_ai=false and zero controls once before the full tick.
                    // Counter1 is preserved, so this tick can legitimately generate another five clones.
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    {
                        var entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                        if (entity == null) continue;
                        entity.AiControlled = false;
                        externalControlSlots.Add(slot);
                    }
                    var observer = new Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    world.SetAcceptedAiRandomTraceObserverForDiagnostics(observer);
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false,
                        new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    following = Capture(world);
                    followingCalls = observer.Capture();
                    Compare(row["following"], following, "following", followingDifferences);
                    CompareJson(row["followingCalls"], followingCalls, "following.calls", followingDifferences);
                }
                string folder = Output + (renderer ? "unity-renderer" : "unity-following");
                Directory.CreateDirectory(folder);
                File.WriteAllText(folder + "/case-" + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, renderer, beforeDifferences, immediateDifferences, followingDifferences,
                    before, immediate, following, immediateCalls = immediateObserver.Capture(), followingCalls,
                    externalControlSlots, sourceNativeAi = false,
                    scope = renderer
                        ? "Synthetic parent remains logic-only; five children use real Renderer materialization. Immediate only."
                        : "Synthetic helper then one full tick, source counter1 unchanged. One-time external ownership for pre-tick entities and empty input set. No second tick or AI-control equivalence claim for new mid-tick entities. Raw missing bindings remain source-only."
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(30)));
                Assert.That(immediateDifferences, Is.Empty, string.Join("\n", immediateDifferences.Take(40)));
                Assert.That(followingDifferences, Is.Empty, string.Join("\n", followingDifferences.Take(50)));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(null);
                if (renderer)
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                        world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        private static void InvokeNativeCloneBoundary(SimulationWorld world)
        {
            var pipeline = typeof(SimulationWorld).GetField("passPipeline", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
            var module = pipeline.GetType().GetProperty("LateEntityLifecycle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pipeline);
            var spawn = module.GetType().GetMethod("SpawnState9996Children", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(spawn, Is.Not.Null);
            spawn.Invoke(module, new object[] { world.FindEntityByRuntimeSlotIncludingPending(0), true });
        }

        [Test]
        public void RecycledCloneTaskPreservesOrdinaryOpointBirth()
        {
            var row = JObject.Parse(File.ReadAllLines(SourceFinal)[0]);
            var world = CreateWorld(row);
            OPointCreateTask task = null;
            try
            {
                var ordinary = world.LogicEntityFactory.Create(new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = 50, dir = "right",
                    preserveActionZero = true, opoint = new ObjectPoint { oid = 217, action = 0 }
                }, out _);
                Assert.That(ordinary, Is.Not.Null);
                task = world.LogicReferencePool.Fetch<OPointCreateTask>();
                task.nativeState9996CloneSpawn = true;
                world.LogicReferencePool.Recycle(task);
                var returnedTask = task;
                task = null;
                Assert.That(returnedTask.nativeState9996CloneSpawn, Is.False, "Recycle must clear clone semantics.");
                task = world.LogicReferencePool.Fetch<OPointCreateTask>();
                Assert.That(task, Is.SameAs(returnedTask), "Exercise the actual returned task.");
                task.targetWorld = world;
                task.requiredRuntimeSlot = 51;
                task.dir = "right";
                task.preserveActionZero = true;
                task.opoint = new ObjectPoint { oid = 217, action = 0 };
                var reused = world.LogicEntityFactory.Create(task, out _);
                Assert.That(reused, Is.Not.Null);
                Assert.That(ordinary.Health.HP, Is.Not.EqualTo(500), "Fixture must distinguish ordinary OPoint scaling from direct clone birth.");
                Assert.That(reused.Health.HP, Is.EqualTo(ordinary.Health.HP));
                Assert.That(reused.Health.PP, Is.EqualTo(ordinary.Health.PP));
                Assert.That(reused.Health.HPBound, Is.EqualTo(ordinary.Health.HPBound));
                Assert.That(reused.Health.HP3, Is.EqualTo(ordinary.Health.HP3));
                Assert.That(reused.Runtime.DisplayCurrentHp200, Is.EqualTo(ordinary.Runtime.DisplayCurrentHp200));
                Assert.That(reused.Runtime.DisplayEffectiveMaxHp208, Is.EqualTo(ordinary.Runtime.DisplayEffectiveMaxHp208));
            }
            finally
            {
                if (task != null)
                    world.LogicReferencePool.Recycle(task);
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        private static SimulationWorld CreateWorld(JObject row)
        {
            var world = new SimulationWorld(BattleRuntimeProfile.Authority400, 400);
            try
            {
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.SetLogicOnlyEntityMaterialization(true);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                var definitions = new Dictionary<int, LF2CharacterDataWrapper>();
                var entries = new List<ObjectDefinition>();
                Add(31980, (int)row["params"]["sourceType"], (string)row["sourceDat"]);
                foreach (int oid in new[] { 217, 218 })
                    if (oid != (int)row["params"]["missingOid"])
                        Add(oid, (int)row["params"]["targetType"], (string)row["targetDat"]);
                Add(31999, 5, "<bmp_begin>\nname: Filler\n<bmp_end>\n<frame> 0 filler\nstate: 0 wait: 100 next: 0\n<frame_end>\n");
                world.PrepareRuntimeDataCatalogForBattle(entries.ToArray(), id => definitions.TryGetValue(id, out var data) ? data : null);
                var parent = Spawn(31980, 0, 20);
                int freeSlots = (int)row["params"]["freeSlots"];
                if (freeSlots <= 2)
                {
                    for (int slot = 50 + freeSlots; slot < world.RuntimeSlotCapacityForDiagnostics; slot++) Spawn(31999, slot, 0);
                    Assert.That(Enumerable.Range(50, world.RuntimeSlotCapacityForDiagnostics - 50)
                        .Count(slot => world.FindEntityByRuntimeSlotIncludingPending(slot) == null), Is.EqualTo(freeSlots));
                }
                Restore(parent, row["before"]["entities"][0]);
                world.CharacterInputWriter.SynchronizeNativeExactAiStateFromRuntime(parent.Runtime);
                world.NativeRandom.ResetFromSeed((uint)row["params"]["seed"]);
                return world;

                void Add(int oid, int type, string dat)
                {
                    definitions.Add(oid, Definition(oid, dat));
                    entries.Add(new ObjectDefinition(oid, type, "clone-" + oid + ".dat"));
                }
                LF2Entity Spawn(int oid, int slot, int action)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right", preserveActionZero = true,
                        opoint = new ObjectPoint { oid = oid, action = action }
                    };
                    var entity = world.LogicEntityFactory.Create(task, out _);
                    Assert.That(entity, Is.Not.Null, "fixture spawn " + oid + "/" + slot);
                    entity.AiControlled = false;
                    return entity;
                }
            }
            catch
            {
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                throw;
            }
        }

        private static JObject Capture(SimulationWorld world)
        {
            var rawRows = (JArray)JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            var inputs = ((object[])ProjectInput.Invoke(null, new object[] { world })).Select(JObject.FromObject).ToArray();
            var entities = new JArray();
            int fillers = 0;
            foreach (JObject raw in rawRows)
            {
                if ((int)raw["identity"]["objectId"] == 31999) { fillers++; continue; }
                int slot = (int)raw["slot"];
                var e = world.FindEntityByRuntimeSlotIncludingPending(slot);
                var r = e.Runtime;
                entities.Add(new JObject
                {
                    ["raw"] = raw.DeepClone(), ["available"] = e.Frame.D != null,
                    ["state"] = e.Frame.D?.state ?? 0, ["wait"] = e.Frame.D?.wait ?? 0, ["next"] = e.Frame.D?.next ?? 0,
                    ["aiProfile"] = r.NativeAiProfileObjectId, ["dropMode"] = r.NativeDefinitionDropMode,
                    ["incomingScale"] = r.IncomingDamageScale340, ["modeScale"] = r.ModeDamageScalePercent,
                    ["pendingCount"] = e.HitCount, ["pending"] = new JArray(r.KnockbackVx, r.KnockbackVy, r.KnockbackVz),
                    ["display"] = new JArray(r.DisplayScore1F0, r.DisplayScoreStep1F4, r.DisplayDamageTotal1F8,
                        r.DisplayDamageStep1FC, r.DisplayCurrentHp200, r.DisplayCurrentHpStep204,
                        r.DisplayEffectiveMaxHp208, r.DisplayEffectiveMaxHpStep20C),
                    ["input"] = inputs.Single(item => (int)item["slot"] == slot)
                });
            }
            return new JObject
            {
                ["activeCount"] = rawRows.Count, ["fillerCount"] = fillers, ["entities"] = entities,
                ["random"] = JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() }))
            };
        }

        private static void Compare(JToken expected, JObject actual, string label, List<string> differences)
        {
            // Fillers model only relative dynamic capacity; their count differs by profile.
            CompareJson((int)expected["activeCount"] - (int)expected["fillerCount"],
                (int)actual["activeCount"] - (int)actual["fillerCount"], label + ".nonFillerCount", differences);
            if ((int)expected["fillerCount"] == 0)
            {
                CompareJson(expected["activeCount"], actual["activeCount"], label + ".activeCount", differences);
                CompareJson(expected["fillerCount"], actual["fillerCount"], label + ".fillerCount", differences);
            }
            var actualRows = (JArray)actual["entities"];
            foreach (JObject source in expected["entities"])
            {
                int slot = (int)source["raw"]["slot"];
                var found = actualRows.SingleOrDefault(item => (int)item["raw"]["slot"] == slot);
                if (found == null) { differences.Add(label + " missing slot " + slot); continue; }
                var trimmed = (JObject)source.DeepClone();
                foreach (string path in NTSD28UnityEntityRawCapture.MissingBindings)
                    (trimmed["raw"].SelectToken(path)?.Parent as JProperty)?.Remove();
                CompareJson(trimmed, found, label + ".slot" + slot, differences);
            }
            CompareJson(expected["random"], actual["random"], label + ".random", differences);
        }

        private static void CompareJson(JToken expected, JToken actual, string path, List<string> differences)
        {
            if (expected is JObject obj)
            {
                if (actual is not JObject other) { differences.Add(path + " object missing"); return; }
                foreach (var property in obj.Properties()) CompareJson(property.Value, other[property.Name], path + "." + property.Name, differences);
            }
            else if (expected is JArray array)
            {
                if (actual is not JArray other || other.Count != array.Count) { differences.Add(path + " array length differs"); return; }
                for (int i = 0; i < array.Count; i++) CompareJson(array[i], other[i], path + "[" + i + "]", differences);
            }
            else
            {
                bool numeric = expected != null && (expected.Type == JTokenType.Integer || expected.Type == JTokenType.Float);
                bool equal = numeric ? actual != null && (actual.Type == JTokenType.Integer || actual.Type == JTokenType.Float) && (double)expected == (double)actual : JToken.DeepEquals(expected, actual);
                if (!equal) differences.Add(path + "=" + actual + " expected=" + expected);
            }
        }

        private static LF2CharacterDataWrapper Definition(int id, string dat, bool sourceLoaderDefaults = false)
        {
            string key = id + "/" + sourceLoaderDefaults + "/" + dat;
            if (wrappers.TryGetValue(key, out var wrapper)) return wrapper;
            string root = Path.GetFullPath(Output + "fixture-runtime");
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", id + ".dat"), BattleContentSource.ForLoganRuntime(root));
            if (sourceLoaderDefaults && data.type_sub == 0) data.type_sub = id;
            wrapper = new LF2CharacterDataWrapper(id, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }

        private static void Restore(LF2Entity entity, JToken before)
        {
            var raw = before["raw"];
            var r = entity.Runtime;

            if (before["ordinaryCreditGate2F4"] != null)
            {
                r.OrdinaryCreditGate2F4 = (int)before["ordinaryCreditGate2F4"];
                r.InputHpConsumedTotal34C = (int)before["hpConsumed"];
                r.InputMpConsumedTotal350 = (int)before["mpConsumed"];
            }
            entity.DirectWriteNativeRawFramePreserveWaitCounter((int)raw["frame"]["action"]);
            entity.Frame.Prev = (int)raw["frame"]["previousAction"];
            entity.Frame.Prev2 = (int)raw["frame"]["tickActionSnapshot"];
            entity.Frame.Prev2D = entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev2);
            r.PrevFrame2 = entity.Frame.Prev2;
            entity.Trans.SyncDirectFrameData(entity.Frame.D?.wait ?? 0, entity.Frame.D?.next ?? 0, (int)raw["frame"]["actionLatch"]);
            entity.AttackingCounter = (int)raw["frame"]["frameCounter"];
            entity.SwitchDir((bool)raw["frame"]["facingLeft"] ? "left" : "right");
            r.SetVelocity((double)raw["motion"]["x"], (double)raw["motion"]["y"], (double)raw["motion"]["z"]);
            r.SetPosition((double)raw["position"]["preciseX"], (double)raw["position"]["preciseY"], (double)raw["position"]["preciseZ"]);
            r.XInt = (int)raw["position"]["x"]; r.YInt = (int)raw["position"]["y"]; r.ZInt = (int)raw["position"]["z"];
            r.AnimCounter = (int)raw["identity"]["controlSlot"];
            r.OwnerSlotIndex = (int)raw["identity"]["ownerSlot"];
            r.RelationTeam = (int)raw["identity"]["battleGroup"];
            r.Unk344 = (int)raw["identity"]["participantClass"];
            r.HP = (int)raw["vitals"]["currentHp"]; r.HPBound = (int)raw["vitals"]["effectiveMaxHp"]; r.HP3 = (int)raw["vitals"]["baseMaxHp"];
            r.PP = (int)raw["vitals"]["currentMp"]; r.MPMax = (int)raw["vitals"]["baseMaxMp"];
            r.HP2Orig = (int)raw["vitals"]["reviveLives"]; r.HPOrig = (int)raw["vitals"]["reviveNextLives"]; r.RespawnCount = (int)raw["vitals"]["reviveNextHp"];
            r.NativeRuntimeStateCode = (int)raw["combat"]["runtimeStateCode"];
            r.WeaponFlightCounter = (int)raw["combat"]["weaponHp"];
            r.RuntimeArmorHp118 = (int)raw["combat"]["runtimeArmorHp"];
            r.ArmorRecoveryTimer11C = (int)raw["combat"]["armorRecoveryTimer"];
            r.FrameDelay = (int)raw["combat"]["motionHoldTimer"];
            var input = before["input"]["input"];
            r.NativeInputProxy.Clear();
            string[] edgeKeys = { "attack", "jump", "defend", "right", "left", "up", "down" };
            int[] maskBits = { 2, 3, 1, 0, 4, 5, 6 };
            for (int index = 0; index < 7; index++)
            {
                r.NativeInputProxy.Current[index] = (byte)(((int)input["currentMask"] >> maskBits[index]) & 1);
                r.NativeInputProxy.Previous[index] = (byte)(((int)input["previousMask"] >> maskBits[index]) & 1);
                r.NativeInputProxy.EdgeWindow[index] = (byte)input["edgeWindow"][edgeKeys[index]];
                r.InputRemapIndices13C[index] = (byte)input["remapIndices"][index];
            }
            for (int index = 0; index < 10; index++) r.NativeInputProxy.ComboState[index] = (byte)input["comboState"][index];
            for (int index = 0; index < 5; index++) r.InputHistory[index + 1] = (int)input["keyHistory"][index];
            r.NativeInputProxy.DefendReentryCooldown = (byte)input["defendReentryCooldown"];
            r.NativeInputProxy.ProxyTail = (byte)input["proxyTail"];
            r.AnimSub = (int)input["runAccumulator"];
            r.InputLastAction144 = (int)input["lastAction"];
            r.InputRemapState138 = (int)input["remapState"];
            r.BoundState198 = (int)input["boundState"];
            r.InputGlobalRecordState20 = (int)input["globalRecordState"];
            r.HitStop = (int)raw["combat"]["renderPhase"];
            r.AttackExempt = (int)raw["combat"]["attackerRest"];
            r.CollisionYReference = (int)raw["combat"]["collisionYReference"];
            r.Fall = (int)raw["combat"]["hitReactionTimer"];
            r.Bdefend = (int)raw["combat"]["bdefendAccumulator"];
            r.SpecialHitLatch0EB = (bool)raw["combat"]["specialHitLatch0eb"];
            r.EnvironmentState320 = (int)raw["combat"]["environmentState"];
            r.EnvironmentSourceSlot160 = (int)raw["combat"]["environmentSourceSlot"];
            r.ObjectAiExcludedGroupSourceSlot2F8 = (int)raw["combat"]["objectAiExcludedGroupSourceSlot"];
            r.NativeLifecycleCode = (int)raw["lifecycle"]["code"];
            r.NativeLifecycleResolutionPending = (bool)raw["lifecycle"]["resolutionPending"];
            if (before["link"] != null) r.LinkState = (int)before["link"];
            if (before["parent"] != null) r.HolderStableId = (int)before["parent"];
            if (before["child"] != null) r.TargetSlotIndex = (int)before["child"];
            entity.HitCount = (int)before["pendingCount"];
            r.KnockbackVx = (double)before["pending"][0]; r.KnockbackVy = (double)before["pending"][1]; r.KnockbackVz = (double)before["pending"][2];
            r.NativeAiProfileObjectId = (int)before["aiProfile"];
            r.NativeDefinitionDropMode = (int)before["dropMode"];
            r.IncomingDamageScale340 = (int)before["incomingScale"];
            r.ModeDamageScalePercent = (int)before["modeScale"];
            r.DisplayScore1F0 = (int)before["display"][0];
            r.DisplayScoreStep1F4 = (int)before["display"][1];
            r.DisplayDamageTotal1F8 = (int)before["display"][2];
            r.DisplayDamageStep1FC = (int)before["display"][3];
            r.DisplayCurrentHp200 = (int)before["display"][4];
            r.DisplayCurrentHpStep204 = (int)before["display"][5];
            r.DisplayEffectiveMaxHp208 = (int)before["display"][6];
            r.DisplayEffectiveMaxHpStep20C = (int)before["display"][7];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            entity.RefreshRuntimeSnapshot();
        }

        private sealed class Observer : INTSD28NativeRandomCallObserver
        {
            private readonly List<object> crt = new();
            private readonly List<object> synchronized = new();
            public void OnCrtNext(NTSD28NativeCrtCall call) => crt.Add(new { result = call.Result, stateAfter = call.StateAfter, totalCalls = call.TotalCalls });
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call) => synchronized.Add(new
            {
                callSite = call.CallSite, upperBound = call.UpperBound, result = call.Result,
                counterAfter = call.CounterAfter, indexAfter = call.IndexAfter, totalCalls = call.TotalCalls
            });
            internal JObject Capture() => JObject.FromObject(new { crt, synchronized });
        }
    }

}
#endif
