#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06FrameMotionTailEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q06-FRAME-MOTION-TAIL-001/";

        [TestCase(false)]
        [TestCase(true)]
        public void FormalOid736Frame120DirectDepthUsesViewRatio(bool configuredView)
        {
            const string formalPath =
                "Assets/NTSD/Content/LoganRuntime/decoded_dat/a/wal/wal.dat";
            string dat = File.ReadAllText(formalPath);
            var definition = Definition(736, dat);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(736, 3, "a/wal/wal.dat")
            }, id => id == 736 ? definition : null);
            try
            {
                var entity = new LF2Weapon { ObjectId = 736 };
                entity.FrameCache.Load(definition);
                var frame = entity.FrameCache.GetNativeFrameDataById(120);
                Assert.That(frame, Is.Not.Null);
                Assert.That(frame.dz, Is.EqualTo(-2));
                Assert.That(frame.dvz, Is.EqualTo(550));
                entity.Frame.D = frame;
                entity.Trans.SyncDirectFrameData(frame.wait, frame.next, 120);
                entity.SetRequiredRuntimeSlot(20);
                world.Register(entity);
                entity.Runtime.ZInt = 250;
                entity.Runtime.Z = 250;
                entity.Runtime.SetSourceRulePosition(-12.75, 31.25);
                entity.Runtime.SourceRuleXInt = -14;
                entity.Runtime.SourceRuleZInt = 29;
                entity.Runtime.Vz = 0;
                entity.Runtime.DelayTimer134 = 0;
                entity.ApplyNativeFrameMotionForWorldPass();

                double expectedZ = 250 - 2.0 *
                    (configuredView ? 1152.0 / 730.0 : 1.0);
                Assert.That(entity.Runtime.Z, Is.EqualTo(expectedZ).Within(0.000001));
                Assert.That(entity.Runtime.ZInt,
                    Is.EqualTo((int)Math.Round(expectedZ, MidpointRounding.ToEven)));
                Assert.That(entity.Runtime.SourceRuleX, Is.EqualTo(-12.75));
                Assert.That(entity.Runtime.SourceRuleXInt, Is.EqualTo(-14));
                Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(27));
                Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo(27));
                Assert.That(entity.Runtime.Vz, Is.Zero);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var reason),
                    Is.True, reason);
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DirectFrameDxUsesViewRatioAndFacing(
            bool configuredView,
            bool faceLeft)
        {
            var data = new LF2CharacterData { type_sub = 1 };
            data.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0,
                dx = 4, dvx = 550,
            });
            var definition = new LF2CharacterDataWrapper(888, data);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(888, 1, "direct-frame.dat")
            }, id => id == 888 ? definition : null);
            try
            {
                var entity = new LF2Weapon { ObjectId = 888 };
                entity.FrameCache.Load(definition);
                entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(0);
                entity.Trans.SyncDirectFrameData(100, 0, 0);
                entity.SetRequiredRuntimeSlot(20);
                world.Register(entity);
                entity.Runtime.XInt = 200;
                entity.Runtime.X = 200;
                entity.Runtime.SetSourceRulePosition(-12.75, 31.25);
                entity.Runtime.SourceRuleXInt = -14;
                entity.Runtime.SourceRuleZInt = 29;
                entity.Runtime.Vx = 0;
                entity.SwitchDir(faceLeft ? "left" : "right");
                entity.ApplyNativeFrameMotionForWorldPass();

                double expectedX = 200 + (faceLeft ? -4.0 : 4.0) *
                    (configuredView ? 2048.0 / 1333.0 : 1.0);
                Assert.That(entity.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
                Assert.That(entity.Runtime.XInt,
                    Is.EqualTo((int)Math.Round(expectedX, MidpointRounding.ToEven)));
                Assert.That(entity.Runtime.SourceRuleX,
                    Is.EqualTo(faceLeft ? -18 : -10));
                Assert.That(entity.Runtime.SourceRuleXInt,
                    Is.EqualTo(faceLeft ? -18 : -10));
                Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(31.25));
                Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo(29));
                Assert.That(entity.Runtime.Vx, Is.Zero);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var reason),
                    Is.True, reason);
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void LinkedPlatformCarryUsesViewRatioAndFacing(
            bool configuredView,
            bool faceLeft)
        {
            var platformData = new LF2CharacterData { type_sub = 3 };
            platformData.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0,
                state = 3003, dvx = 4, dvy = 550, dvz = 2,
            });
            var riderData = new LF2CharacterData { type_sub = 3 };
            riderData.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0,
                state = 3003, dvx = 550, dvy = 550, dvz = 550,
            });
            var platformDefinition = new LF2CharacterDataWrapper(887, platformData);
            var riderDefinition = new LF2CharacterDataWrapper(888, riderData);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(887, 3, "platform-carry.dat"),
                new ObjectDefinition(888, 3, "rider-carry.dat")
            }, id => id == 887 ? platformDefinition : id == 888 ? riderDefinition : null);
            try
            {
                var platform = new LF2Weapon { ObjectId = 887 };
                platform.FrameCache.Load(platformDefinition);
                platform.Frame.D = platform.FrameCache.GetNativeFrameDataById(0);
                platform.Trans.SyncDirectFrameData(100, 0, 0);
                platform.SetRequiredRuntimeSlot(20);
                world.Register(platform);
                platform.SwitchDir(faceLeft ? "left" : "right");

                var rider = new LF2Weapon { ObjectId = 888 };
                rider.FrameCache.Load(riderDefinition);
                rider.Frame.D = rider.FrameCache.GetNativeFrameDataById(0);
                rider.Trans.SyncDirectFrameData(100, 0, 0);
                rider.SetRequiredRuntimeSlot(21);
                world.Register(rider);
                rider.Runtime.SetPosition(200, -10, 250);
                rider.Runtime.SyncIntegerPosition();
                rider.Runtime.SetSourceRulePosition(-12.75, 31.25);
                rider.Runtime.SourceRuleXInt = -14;
                rider.Runtime.SourceRuleZInt = 29;
                rider.Runtime.CollisionYReference = -10;
                rider.Runtime.PlatformSourceSlotF4 = 20;
                rider.ApplyNativeFrameMotionForWorldPass();

                double expectedX = 200 + (faceLeft ? -4.0 : 4.0) *
                    (configuredView ? 2048.0 / 1333.0 : 1.0);
                double expectedZ = 250 + 2.0 *
                    (configuredView ? 1152.0 / 730.0 : 1.0);
                Assert.That(rider.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
                Assert.That(rider.Runtime.XInt,
                    Is.EqualTo((int)Math.Round(expectedX, MidpointRounding.ToEven)));
                Assert.That(rider.Runtime.Z, Is.EqualTo(expectedZ).Within(0.000001));
                Assert.That(rider.Runtime.ZInt,
                    Is.EqualTo((int)Math.Round(expectedZ, MidpointRounding.ToEven)));
                Assert.That(rider.Runtime.SourceRuleX,
                    Is.EqualTo(faceLeft ? -18 : -10));
                Assert.That(rider.Runtime.SourceRuleXInt,
                    Is.EqualTo(faceLeft ? -18 : -10));
                Assert.That(rider.Runtime.SourceRuleZ, Is.EqualTo(31));
                Assert.That(rider.Runtime.SourceRuleZInt, Is.EqualTo(31));
                Assert.That(rider.Runtime.Y, Is.EqualTo(-10));
                Assert.That(rider.Runtime.CollisionYReference, Is.EqualTo(-10));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var reason),
                    Is.True, reason);
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void PlatformThenDelayedDirectFrameUsesIndependentSourceIntegerBase(
            bool configuredView,
            bool initializedSource)
        {
            var platformData = new LF2CharacterData { type_sub = 3 };
            platformData.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0,
                state = 3003, dvx = 3, dvy = 550, dvz = 1,
            });
            var riderData = new LF2CharacterData { type_sub = 3 };
            riderData.frames.Add(new LF2FrameData
            {
                frameId = 0, wait = 100, next = 0,
                state = 3003, dvx = 550, dvy = 550, dvz = 550,
                dx = 4.5, dz = -2.5,
            });
            var platformDefinition = new LF2CharacterDataWrapper(887, platformData);
            var riderDefinition = new LF2CharacterDataWrapper(888, riderData);
            var world = new SimulationWorld();
            world.SetLogicOnlyEntityMaterialization(true);
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(887, 3, "platform-carry.dat"),
                new ObjectDefinition(888, 3, "rider-carry.dat")
            }, id => id == 887 ? platformDefinition : id == 888 ? riderDefinition : null);
            try
            {
                var platform = new LF2Weapon { ObjectId = 887 };
                platform.FrameCache.Load(platformDefinition);
                platform.Frame.D = platform.FrameCache.GetNativeFrameDataById(0);
                platform.Trans.SyncDirectFrameData(100, 0, 0);
                platform.SetRequiredRuntimeSlot(20);
                world.Register(platform);

                var rider = new LF2Weapon { ObjectId = 888 };
                rider.FrameCache.Load(riderDefinition);
                rider.Frame.D = rider.FrameCache.GetNativeFrameDataById(0);
                rider.Trans.SyncDirectFrameData(100, 0, 0);
                rider.SetRequiredRuntimeSlot(21);
                world.Register(rider);
                rider.Runtime.SetPosition(200, -10, 250);
                rider.Runtime.SyncIntegerPosition();
                rider.Runtime.CollisionYReference = -10;
                rider.Runtime.PlatformSourceSlotF4 = 20;
                rider.Runtime.DelayTimer134 = 1;
                if (initializedSource)
                {
                    rider.Runtime.SetSourceRulePosition(-12.75, 31.25);
                    rider.Runtime.SourceRuleXInt = -14;
                    rider.Runtime.SourceRuleZInt = 29;
                }

                rider.ApplyNativeFrameMotionForWorldPass();

                double scaleX = configuredView ? 2048.0 / 1333.0 : 1.0;
                double scaleZ = configuredView ? 1152.0 / 730.0 : 1.0;
                double physicalX = Math.Round(200 + 3.0 * scaleX,
                    MidpointRounding.ToEven) + 4.5 * 0.25 * scaleX;
                double physicalZ = Math.Round(250 + 1.0 * scaleZ,
                    MidpointRounding.ToEven) - 2.5 * 0.25 * scaleZ;
                Assert.That(rider.Runtime.X, Is.EqualTo(physicalX).Within(0.000001));
                Assert.That(rider.Runtime.XInt,
                    Is.EqualTo((int)Math.Round(physicalX, MidpointRounding.ToEven)));
                Assert.That(rider.Runtime.Z, Is.EqualTo(physicalZ).Within(0.000001));
                Assert.That(rider.Runtime.ZInt,
                    Is.EqualTo((int)Math.Round(physicalZ, MidpointRounding.ToEven)));
                Assert.That(rider.Runtime.SourceRulePositionInitialized,
                    Is.EqualTo(initializedSource));
                Assert.That(rider.Runtime.SourceRuleX,
                    Is.EqualTo(initializedSource ? -9.875 : 0));
                Assert.That(rider.Runtime.SourceRuleXInt,
                    Is.EqualTo(initializedSource ? -10 : 0));
                Assert.That(rider.Runtime.SourceRuleZ,
                    Is.EqualTo(initializedSource ? 29.375 : 0));
                Assert.That(rider.Runtime.SourceRuleZInt,
                    Is.EqualTo(initializedSource ? 29 : 0));
                Assert.That(rider.Runtime.Vx, Is.Zero);
                Assert.That(rider.Runtime.Vz, Is.Zero);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var reason),
                    Is.True, reason);
            }
        }

        private static IEnumerable<TestCaseData> Cases()
        {
            for (int index = 0; index < 12; index++)
                yield return new TestCaseData(index);
        }

        [TestCaseSource(nameof(Cases))]
        public void FrameMotionMatchesSource(int index)
            => Verify(index, false);

        [TestCase(1)]
        [TestCase(7)]
        [TestCase(9)]
        public void FollowingFullTickAndReplayMatchSource(int index)
            => Verify(index, true);

        internal static void VerifyRendererForPlay(int index) => Verify(index, false, true);

        private static void Verify(int index, bool following, bool renderer = false)
        {
            string source = Root + (following ? "source-following/first.jsonl" : "source/first.jsonl");
            using (var input = File.OpenRead(source))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", ""),
                    Is.EqualTo(following ? "4F9E34435EF26A181C608998450AD5C0F594F704F363F5DC6C16F66684E1B9A1"
                        : "2AD148A4AA3B0241DD866957C51189A7AC20A50C17F9AE0D9579D66B973F4C2F"));
            var row = JObject.Parse(File.ReadAllLines(source)[index]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(!renderer);
                if (following)
                    world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                var target = Definition(31981, (string)row["dat"]);
                var platform = Definition(31980, (string)row["platformDat"]);
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(31981, 0, "tail.dat"),
                    new ObjectDefinition(31980, 3, "platform.dat")
                }, id => id == 31981 ? target : id == 31980 ? platform : null);
                var entity = Spawn(31981, 21);
                if ((bool)row["linked"])
                    Spawn(31980, 20);
                var r = entity.Runtime;
                r.XInt = 100; r.YInt = -10; r.ZInt = 250;
                r.X = 100.75; r.Y = -10.25; r.Z = 250.5;
                r.Vx = 4; r.Vy = -3; r.Vz = 2;
                r.CollisionYReference = -10;
                r.DelayTimer134 = (int)row["before"]["delay"];
                r.NativeLifecycleResolutionPending = (bool)row["pending"];
                r.PlatformSourceSlotF4 = (bool)row["linked"] || (bool)row["missingLink"] ? 20 : 0;
                entity.SwitchDir((bool)row["left"] ? "left" : "right");
                r.KeyUp = (byte)((int)row["depth"] < 0 ? 1 : 0);
                r.KeyDown = (byte)((int)row["depth"] > 0 ? 1 : 0);
                var before = Capture(entity);
                AssertValues(before, row["before"], "before");
                entity.ApplyNativeFrameMotionForWorldPass();
                var after = Capture(entity);
                File.WriteAllText(Root + "unity-" + index + ".json", new JObject
                {
                    ["index"] = index, ["before"] = before, ["after"] = after,
                    ["expected"] = row["after"].DeepClone()
                }.ToString());
                AssertValues(after, row["after"], (string)row["name"]);
                if (following)
                {
                    world.NativeRandom.ResetFromSeed(42);
                    world.Runtime.Stage.StageWidthPx = 800;
                    world.Runtime.Stage.BaseStageWidthPx = 800;
                    world.Runtime.Stage.ZMin = 180;
                    world.Runtime.Stage.ZMax = 350;
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                    var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                    var observed = Capture(world.FindEntityByRuntimeSlotForQuery(21));
                    File.WriteAllText(Root + "following-" + index + ".json", new JObject
                    {
                        ["observed"] = observed, ["expected"] = row["following"].DeepClone()
                    }.ToString());
                    AssertValues(observed, row["following"], "following");
                    ulong checksum = world.CaptureRuntimeChecksum64(1, input);
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                    AssertValues(Capture(world.FindEntityByRuntimeSlotForQuery(21)), observed, "replay");
                    Assert.That(world.CaptureRuntimeChecksum64(1, input), Is.EqualTo(checksum));
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
                    var result = renderer ? LF2ObjectPointFactory.Instance.MaterializeObjectForStructuralWriter(task)
                        : world.LogicEntityFactory.Create(task, out failure);
                    if (renderer) Assert.That(result?.Renderer, Is.Not.Null);
                    Assert.That(result, Is.Not.Null, failure.ToString());
                    result.AiControlled = false;
                    return result;
                }
            }
            finally
            {
                if (renderer)
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                        world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out var failure), Is.True, failure);
            }
        }

        private static LF2CharacterDataWrapper Definition(int oid, string dat)
        {
            string root = Path.GetFullPath(Root + "fixture-runtime");
            return new LF2CharacterDataWrapper(oid, CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root)));
        }

        private static JObject Capture(LF2Entity entity)
        {
            var r = entity.Runtime;
            return new JObject
            {
                ["position"] = new JArray(r.XInt, r.YInt, r.ZInt, r.X, r.Y, r.Z),
                ["motion"] = new JArray(r.Vx, r.Vy, r.Vz),
                ["reference"] = r.CollisionYReference, ["delay"] = r.DelayTimer134
            };
        }

        private static void AssertValues(JToken actual, JToken expected, string phase)
        {
            foreach (string key in new[] { "position", "motion" })
                for (int axis = 0; axis < ((JArray)expected[key]).Count; axis++)
                    Assert.That((double)actual[key][axis], Is.EqualTo((double)expected[key][axis]), phase + "/" + key + "/" + axis);
            foreach (string key in new[] { "reference", "delay" })
                Assert.That((int)actual[key], Is.EqualTo((int)expected[key]), phase + "/" + key);
        }
    }
}
#endif
