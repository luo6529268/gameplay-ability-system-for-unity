#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06PlatformTransactionEditorTests
    {
        private const string Source = "artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-SOURCE-WITNESS-001/source-final/first.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-PLATFORM-TRANSACTION-001/";

        [TestCase(false, false, 525, 650, true, true, 0)]
        [TestCase(true, false, 525, 650, true, true, 0)]
        [TestCase(false, true, 475, 650, true, true, 0)]
        [TestCase(true, true, 475, 650, true, true, 0)]
        [TestCase(true, false, 527, 650, false, true, 0)]
        [TestCase(true, true, 473, 650, false, true, 0)]
        [TestCase(false, false, 510, 664, true, true, 0)]
        [TestCase(true, false, 510, 664, true, true, 0)]
        [TestCase(true, false, 510, 665, false, true, 0)]
        [TestCase(true, false, 525, 650, true, false, 0)]
        [TestCase(true, false, 510, 666, true, false, 2)]
        public void D024PlatformPointGateUsesSourceGeometry(
            bool configuredView,
            bool facingLeft,
            int targetSourceX,
            int targetSourceZ,
            bool shouldArm,
            bool useSourceCarrier,
            int itrOffsetZ)
        {
            var world = new SimulationWorld();
            try
            {
                if (configuredView)
                    world.ConfigureFixedViewRunDistance(2048, 1152);

                var platformFrame = new LF2FrameData
                {
                    frameId = 0,
                    state = 3003,
                    wait = 100,
                    next = 0,
                    centerx = 39,
                };
                platformFrame.itrs.Add(new InteractionArea
                {
                    kind = 30,
                    x = 40,
                    w = 25,
                    z = itrOffsetZ,
                    zwidth = 15,
                    PlatformDvy = 0,
                });
                var riderFrame = new LF2FrameData
                {
                    frameId = 0,
                    state = LF2States.Standing,
                    wait = 100,
                    next = 0,
                };
                var platformData = new LF2CharacterData
                {
                    name = "D024Platform",
                    type_sub = 3,
                    frames = new List<LF2FrameData> { platformFrame },
                };
                var riderData = new LF2CharacterData
                {
                    name = "D024Rider",
                    type_sub = 0,
                    frames = new List<LF2FrameData> { riderFrame },
                };

                var platform = new LF2SpecialAttack
                {
                    Name = platformData.name,
                    ObjectId = 31980,
                };
                platform.FrameCache.Load(new LF2CharacterDataWrapper(31980, platformData));
                platform.Frame.D = platform.FrameCache.GetFrameDataById(0);
                platform.Frame.N = 0;
                platform.Frame.Prev2 = 0;
                platform.SetRequiredRuntimeSlot(20);
                world.Register(platform);

                var rider = new LF2Character
                {
                    Name = riderData.name,
                    ObjectId = 31981,
                };
                rider.ModuleInitialize();
                rider.FrameCache.Load(new LF2CharacterDataWrapper(31981, riderData));
                rider.Frame.D = rider.FrameCache.GetFrameDataById(0);
                rider.Frame.N = 0;
                rider.Frame.Prev2 = 0;
                rider.Initialize(500, 500);
                rider.SetRequiredRuntimeSlot(21);
                world.Register(rider);

                platform.SwitchDir(facingLeft ? "left" : "right");
                SetProjectedPlatformPosition(world, platform, 500, -20, 650);
                SetProjectedPlatformPosition(world, rider, targetSourceX, -10, targetSourceZ);
                if (!useSourceCarrier)
                {
                    platform.Runtime.SourceRulePositionInitialized = false;
                    rider.Runtime.SourceRulePositionInitialized = false;
                }
                platform.Runtime.NativePreviousY104 = 0;
                rider.Runtime.NativePreviousY104 = -10;
                rider.Runtime.CollisionYReference = 0;
                rider.Runtime.PlatformSourceSlotF4 = 0;
                rider.Runtime.RenderShadowOffset10C = 0;

                Assert.That(rider.GetCurrentDataObjectTypeForSimulation(), Is.EqualTo(0));
                world.CaptureCollisionFrameSnapshotsAll();
                world.CollectCollisionCandidatesAll();
                Assert.That(rider.Runtime.CollisionYReference,
                    Is.EqualTo(shouldArm ? -20 : 0),
                    $"view={configuredView} left={facingLeft} X={targetSourceX} Z={targetSourceZ}");
                Assert.That(rider.Runtime.PlatformSourceSlotF4,
                    Is.EqualTo(shouldArm ? platform.Runtime.SlotIndex : 0));
                world.EndCollisionCandidateConsumption();
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var failure), Is.True, failure);
            }
        }

        private static void SetProjectedPlatformPosition(
            SimulationWorld world,
            LF2Entity entity,
            int sourceX,
            int y,
            int sourceZ)
        {
            entity.Runtime.SetPosition(
                world.SpatialProjection.SourceToViewX(sourceX, 0.0), y,
                world.SpatialProjection.SourceToViewZ(sourceZ, 0.0));
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetSourceRulePosition(sourceX, sourceZ);
            entity.Runtime.SyncSourceRuleIntegerPosition();
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void CompleteTicksAndReplayMatchSource(int index)
        {
            string path = Output + "source-fulltick-final/first.jsonl";
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""),
                    Is.EqualTo("3079EE34B013170CDDFAA0738CBC5E070A56826C9BF80921057963207442139F"));
            var lines = new List<string>();
            foreach (string line in File.ReadAllLines(path))
                if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
            var row = JObject.Parse(lines[index]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                var source = Definition(31980, (string)row["sourceDat"]);
                var target = Definition(31981, (string)row["targetDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(31980, 3, "platform.dat"),
                    new ObjectDefinition(31981, 0, "rider.dat")
                }, id => id == 31980 ? source : id == 31981 ? target : null);
                var platform = Spawn(31980, 20);
                var rider = Spawn(31981, 21);
                InitializeFullTickEntity(platform, row["platformBefore"]);
                InitializeFullTickEntity(rider, row["before"]);
                platform.SwitchDir((bool)row["params"]["left"] ? "left" : "right");
                AssertFullTickEntity(platform, row["platformBefore"], "initial platform");
                AssertFullTickEntity(rider, row["before"], "initial rider");
                world.NativeRandom.ResetFromSeed(42);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                var checksums = new ulong[2];
                for (int pass = 0; pass < 2; pass++)
                {
                    if (pass == 1)
                        Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    for (int tick = 1; tick <= 2; tick++)
                    {
                        var input = new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>());
                        new NTSDBattleTickSystem(world).RunReleaseTick(tick, false, input);
                        rider = world.FindEntityByRuntimeSlotForQuery(21);
                        platform = world.FindEntityByRuntimeSlotForQuery(20);
                        File.WriteAllText(Output + "fulltick-" + index + "-" + pass + "-" + tick + ".json", new JObject
                        {
                            ["rider"] = Capture(rider), ["platform"] = Capture(platform),
                            ["expected"] = row["ticks"][tick - 1].DeepClone()
                        }.ToString());
                        AssertFullTickEntity(rider, row["ticks"][tick - 1]["rider"], "rider tick " + tick);
                        AssertFullTickEntity(platform, row["ticks"][tick - 1]["platform"], "platform tick " + tick);
                        ulong checksum = world.CaptureRuntimeChecksum64(tick, input);
                        if (pass == 0) checksums[tick - 1] = checksum;
                        else Assert.That(checksum, Is.EqualTo(checksums[tick - 1]), "replay tick " + tick);
                    }
                }

                LF2Entity Spawn(int oid, int slot)
                {
                    var entity = world.LogicEntityFactory.Create(new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right",
                        preserveActionZero = true, relationTeam = 1,
                        opoint = new ObjectPoint { oid = oid, action = 0 }
                    }, out var failure);
                    Assert.That(entity, Is.Not.Null, failure.ToString());
                    entity.AiControlled = false;
                    return entity;
                }
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var failure), Is.True, failure);
            }
        }

        private static void InitializeFullTickEntity(LF2Entity entity, JToken state)
        {
            var r = entity.Runtime;
            r.XInt = (int)state["position"][0]; r.YInt = (int)state["position"][1]; r.ZInt = (int)state["position"][2];
            r.X = (double)state["position"][3]; r.Y = (double)state["position"][4]; r.Z = (double)state["position"][5];
            r.CollisionYReference = (int)state["reference"];
            r.PlatformSourceSlotF4 = (int)state["platformSlot"];
            r.RenderShadowOffset10C = (int)state["shadow"];
            r.NativePreviousY104 = (int)state["previousY"];
        }

        private static void AssertFullTickEntity(LF2Entity entity, JToken expected, string phase)
        {
            var actual = Capture(entity);
            AssertPosition(actual["position"], expected["position"], phase);
            foreach (string key in new[] { "reference", "platformSlot", "shadow", "previousY" })
                Assert.That((int)actual[key], Is.EqualTo((int)expected[key]), phase + "/" + key);
        }

        private static IEnumerable<TestCaseData> CandidateCases()
        {
            for (int index = 0; index < 21; index++)
                foreach (bool brute in new[] { false, true })
                    yield return new TestCaseData(index, brute);
        }

        [TestCaseSource(nameof(CandidateCases))]
        public void CandidatePositionAndReferenceMatchSource(int index, bool bruteForce)
            => VerifyTransaction(index, bruteForce, false);

        private static IEnumerable<TestCaseData> MotionCases()
        {
            for (int index = 0; index < 21; index++)
                yield return new TestCaseData(index);
        }

        [TestCaseSource(nameof(MotionCases))]
        public void LinkedMotionMatchesSource(int index)
            => VerifyTransaction(index, false, true);

        [TestCase(false, "none")]
        [TestCase(true, "none")]
        [TestCase(false, "hold")]
        [TestCase(true, "hold")]
        [TestCase(false, "link")]
        [TestCase(true, "link")]
        [TestCase(false, "pending")]
        [TestCase(true, "pending")]
        public void PhysicsHistoryMatchesSource(bool worldEntry, string skip)
            => VerifyTransaction(20, false, true, true, worldEntry, skip);

        internal static void VerifyRendererForPlay(int index)
            => VerifyTransaction(index, false, true, index == 20, false, "none", true);

        private static void VerifyTransaction(int index, bool bruteForce, bool motion, bool physics = false, bool worldEntry = false, string skip = "none", bool renderer = false)
        {
            using (var input = File.OpenRead(Source))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", ""),
                    Is.EqualTo("9671B8D3E3BA1D48E58735EBDBE729BD291154435C8C21320DBB32799D77F9B2"));

            var row = JObject.Parse(File.ReadAllLines(Source)[index]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(!renderer);
                var source = Definition(31980, (string)row["sourceDat"]);
                var target = Definition(31981, (string)row["targetDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(31980, 3, "platform.dat"),
                    new ObjectDefinition(31981, (int)row["targetType"], "rider.dat")
                }, id => id == 31980 ? source : id == 31981 ? target : null);
                var platform = Spawn(31980, (int)row["params"]["sourceSlot"]);
                var rider = Spawn(31981, 21);
                platform.Runtime.SetPosition(100, -20, 250);
                platform.Runtime.SyncIntegerPosition();
                platform.Runtime.NativePreviousY104 = (int)row["params"]["previousSourceY"];
                platform.Runtime.RelationTeam = (int)row["params"]["sourceGroup"];
                platform.SwitchDir((bool)row["params"]["left"] ? "left" : "right");
                if ((int)row["params"]["extraY"] != 0)
                {
                    var extra = Spawn(31980, (int)row["params"]["extraSlot"]);
                    extra.Runtime.SetPosition(100, (int)row["params"]["extraY"], 250);
                    extra.Runtime.SyncIntegerPosition();
                    extra.Runtime.NativePreviousY104 = 0;
                    extra.Runtime.RelationTeam = (int)row["params"]["sourceGroup"];
                }
                rider.Runtime.SetPosition((double)row["params"]["targetX"], -10, (double)row["params"]["targetZ"]);
                rider.Runtime.SyncIntegerPosition();
                rider.Runtime.CollisionYReference = -999;
                rider.Runtime.PlatformSourceSlotF4 = 99;
                rider.Runtime.RenderShadowOffset10C = -999;
                rider.Runtime.NativePreviousY104 = -10;
                rider.Runtime.RelationTeam = (int)row["params"]["targetGroup"];
                world.CaptureCollisionFrameSnapshotsAll();
                if ((bool)row["params"]["splitFrame"])
                    platform.DirectWriteNativeRawFramePreserveWaitCounter(1);
                var query = (BruteForceSceneQuery)world.SceneQuery;
                if (bruteForce)
                    query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                JObject before = Capture(rider);
                AssertPosition(before["position"], row["before"]["position"], "Initial position");
                Assert.That((int)before["reference"], Is.EqualTo((int)row["before"]["reference"]));
                Assert.That(platform.GetCollisionFrameData().state, Is.EqualTo(3003));
                Assert.That(platform.Frame.D.itrs.Count, Is.EqualTo(1));
                world.CollectCollisionCandidatesAll();
                JObject after = Capture(rider);
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "candidate-" + index + "-" + bruteForce + ".json",
                    JsonConvert.SerializeObject(new
                    {
                        index, bruteForce, before, after,
                        expected = row["afterCandidate"],
                        scope = "Actual candidate state; source previousXYZ X/Z and following motion/fulltick not certified"
                    }, Formatting.Indented));
                {
                    AssertPosition(after["position"], row["afterCandidate"]["position"], "After candidate");
                    foreach (string key in new[] { "reference", "platformSlot", "shadow", "previousY" })
                        Assert.That((int)after[key], Is.EqualTo((int)row["afterCandidate"][key]), key);
                }

                if (motion)
                {
                    if ((bool)row["params"]["removeSource"])
                    {
                        int sourceSlot = platform.Runtime.SlotIndex;
                        world.StructuralWriter.Free(platform);
                        world.FlushPendingDestroyForDiagnostics();
                        Assert.That(world.FindEntityByRuntimeSlotForQuery(sourceSlot), Is.Null);
                    }
                    rider.ApplyNativeFrameMotionForWorldPass();
                    JObject moved = Capture(rider);
                    File.WriteAllText(Output + "motion-" + index + ".json", JsonConvert.SerializeObject(new
                    {
                        index, afterCandidate = after, afterMotion = moved, expected = row["afterMotion"],
                        scope = "Actual rider motion entry; no fulltick or native diagnostic success parity"
                    }, Formatting.Indented));
                    AssertPosition(moved["position"], row["afterMotion"]["position"], "After linked motion");
                    foreach (string key in new[] { "reference", "platformSlot", "shadow", "previousY" })
                        Assert.That((int)moved[key], Is.EqualTo((int)row["afterMotion"][key]), key);
                }

                if (physics)
                {
                    if (skip == "hold") rider.Runtime.FrameDelay = 2;
                    if (skip == "link") rider.Runtime.LinkState = -1;
                    if (skip == "pending") rider.Runtime.NativeLifecycleResolutionPending = true;
                    if (worldEntry)
                    {
                        world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(
                            NTSD.Simulation.Ecs.BattleEcsCharacterFrameAdvancePassMode.DataOriented);
                        world.NativePhysicsAndDeadCharacterResourceNormalizeAll(1);
                        if (skip != "pending")
                            Assert.That(world.BattleEcsCharacterFrameAdvancePassDiagnosticsForDiagnostics.ExactCharacterCount,
                                Is.GreaterThan(0), "Actual ECS character entry");
                    }
                    else
                        rider.ExecuteNativePhysicsForWorldPass(1);
                    JToken expectedPhysics = row[skip == "none" ? "afterPhysics" : "afterMotion"];
                    JObject integrated = Capture(rider);
                    File.WriteAllText(Output + "physics-" + index + "-" + worldEntry + "-" + skip + ".json", JsonConvert.SerializeObject(new
                    {
                        index, worldEntry, skip, afterPhysics = integrated, expected = expectedPhysics,
                        scope = "Actual rider physics entry; X/Z history and fulltick not certified"
                    }, Formatting.Indented));
                    AssertPosition(integrated["position"], expectedPhysics["position"], "After physics");
                    foreach (string key in new[] { "reference", "platformSlot", "shadow", "previousY" })
                        Assert.That((int)integrated[key], Is.EqualTo((int)expectedPhysics[key]), key);
                }

                LF2Entity Spawn(int oid, int slot)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right",
                        preserveActionZero = true, relationTeam = 1,
                        opoint = new ObjectPoint { oid = oid, action = 0 }
                    };
                    BattleLogicEntityCreationFailure failure = BattleLogicEntityCreationFailure.None;
                    var entity = renderer ? LF2ObjectPointFactory.Instance.MaterializeObjectForStructuralWriter(task)
                        : world.LogicEntityFactory.Create(task, out failure);
                    if (renderer) Assert.That(entity?.Renderer, Is.Not.Null);
                    Assert.That(entity, Is.Not.Null, failure.ToString());
                    entity.AiControlled = false;
                    return entity;
                }
            }
            finally
            {
                if (renderer)
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                        world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var shutdownFailure), Is.True, shutdownFailure);
            }
        }

        [TestCase("PlatformSourceSlotF4")]
        [TestCase("RenderShadowOffset10C")]
        [TestCase("NativePreviousY104")]
        public void PlatformStateCopiesResetsAndRestoresWithChecksum(string name)
        {
            FieldInfo field = typeof(NTSDEntityRuntime).GetField(name);
            Assert.That(field, Is.Not.Null, "Missing native platform carrier " + name);
            var source = new NTSDEntityRuntime();
            var copy = new NTSDEntityRuntime();
            Assert.That(field.GetValue(source), Is.EqualTo(0));
            field.SetValue(source, -19);
            Assert.That(source.TryCopyCanonicalStateTo(copy), Is.True);
            source.Reset();
            Assert.That(field.GetValue(source), Is.EqualTo(0));
            Assert.That(field.GetValue(copy), Is.EqualTo(-19));

            var world = new SimulationWorld();
            try
            {
                var raw = world.GetRawRuntimeSlotState(3);
                var input = new FrameInputSet(0, Array.Empty<SimulationPlayerInput>());
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                field.SetValue(raw, -19);
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                ulong before = world.CaptureRuntimeChecksum64(0, input);
                field.SetValue(raw, 7);
                Assert.That(world.CaptureRuntimeChecksum64(0, input), Is.Not.EqualTo(before));
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                Assert.That(field.GetValue(raw), Is.EqualTo(-19));
                Assert.That(world.CaptureRuntimeChecksum64(0, input), Is.EqualTo(before));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var shutdownFailure), Is.True, shutdownFailure);
            }
        }

        private static LF2CharacterDataWrapper Definition(int oid, string dat)
        {
            string root = Path.GetFullPath(Output + "fixture-runtime");
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root));
            return new LF2CharacterDataWrapper(oid, data);
        }

        private static void AssertPosition(JToken actual, JToken expected, string phase)
        {
            for (int axis = 0; axis < 6; axis++)
                Assert.That((double)actual[axis], Is.EqualTo((double)expected[axis]), phase + " axis " + axis);
        }

        private static JObject Capture(LF2Entity entity)
        {
            var r = entity.Runtime;
            return new JObject
            {
                ["position"] = new JArray(r.XInt, r.YInt, r.ZInt, r.X, r.Y, r.Z),
                ["reference"] = r.CollisionYReference,
                ["platformSlot"] = r.PlatformSourceSlotF4,
                ["shadow"] = r.RenderShadowOffset10C,
                ["previousY"] = r.NativePreviousY104
            };
        }
    }
}
#endif
