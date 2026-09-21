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

        private static void VerifyTransaction(int index, bool bruteForce, bool motion)
        {
            using (var input = File.OpenRead(Source))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", ""),
                    Is.EqualTo("9671B8D3E3BA1D48E58735EBDBE729BD291154435C8C21320DBB32799D77F9B2"));

            var row = JObject.Parse(File.ReadAllLines(Source)[index]);
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
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
