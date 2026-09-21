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
