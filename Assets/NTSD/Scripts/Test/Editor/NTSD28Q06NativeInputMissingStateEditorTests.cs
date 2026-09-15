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
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeInputMissingStateEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-INPUT-MISSING-STATE-ROUTING-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void SampledInputMatchesSourceOptionalState(BattleRuntimeProfile profile)
        {
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            var actual = new List<object>();
            int cases = 0;
            foreach (var row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = CreateWorld(row, profile, out var entity);
                try
                {
                    string label = "case " + row["index"];
                    Compare(world, entity, row["before"], label + " before", beforeDifferences);
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacy = world.Rng.CallCount;
                    new NTSD28InputTwoPassModule(world, null).ProcessNativeSampledState(entity);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    Compare(world, entity, row["after"], label + " after", differences);
                    if (world.Rng.CallCount != legacy || observer.CrtCalls != row["crtCalls"].Count())
                        differences.Add(label + " unexpected legacy/CRT calls");
                    var expectedCalls = row["synchronizedCalls"].Select(call => new long[]
                    {
                        (long)call["callSite"], (long)call["upperBound"], (long)call["result"],
                        (long)call["counterAfter"], (long)call["indexAfter"], (long)call["totalCalls"]
                    }).ToArray();
                    CompareJson(JArray.FromObject(expectedCalls), JArray.FromObject(observer.Calls), label + " RNG calls", differences);
                    actual.Add(new { index = (int)row["index"], raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1)),
                        input = CaptureInput(world), random = CaptureRandom(world), runAccumulator = entity.Runtime.AnimSub,
                        descriptorAvailable = entity.Frame.D != null });
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + ".json", JsonConvert.SerializeObject(new
            {
                cases, beforeDifferences, differences, actual,
                scope = "432 sampled type0 input endpoints; 47 bound raw fields, exact B2 input and RNG. Pending input is not sampled here; source action messages are diagnostic only."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(432));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(15)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(20)));
        }

        private static JObject CaptureInput(SimulationWorld world)
            => JObject.FromObject(((object[])ProjectInput.Invoke(null, new object[] { world }))[0]);

        private static JObject CaptureRandom(SimulationWorld world)
            => JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() }));

        private static void Compare(SimulationWorld world, LF2Entity entity, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, new JArray(expected["raw"].DeepClone()), label, differences);
            CompareJson(expected["input"], CaptureInput(world), label + " input", differences);
            CompareJson(expected["random"], CaptureRandom(world), label + " random", differences);
            if ((entity.Frame.D != null) != (bool)expected["available"])
                differences.Add(label + " descriptor availability differs");
            if (entity.Runtime.AnimSub != (int)expected["runAccumulator"])
                differences.Add(label + " run accumulator=" + entity.Runtime.AnimSub + " expected=" + expected["runAccumulator"]);
        }

        private static void CompareJson(JToken expected, JToken actual, string path, List<string> differences)
        {
            if (expected is JObject obj)
            {
                foreach (var property in obj.Properties()) CompareJson(property.Value, actual?[property.Name], path + "." + property.Name, differences);
            }
            else if (expected is JArray array)
            {
                if (actual is not JArray actualArray || actualArray.Count != array.Count)
                {
                    differences.Add(path + " array length differs");
                    return;
                }
                for (int index = 0; index < array.Count; index++) CompareJson(array[index], actualArray[index], path + "[" + index + "]", differences);
            }
            else if (!JToken.DeepEquals(expected, actual)) differences.Add(path + "=" + actual + " expected=" + expected);
        }

        private static SimulationWorld CreateWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity entity)
        {
            string dat = (string)row["dat"];
            if (!wrappers.TryGetValue(dat, out var wrapper))
            {
                var parsed = new Lf2DatParserV2().ParseLoganContent(dat);
                var data = new LF2CharacterData { type_sub = 0,
                    NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties)) };
                foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
                wrapper = new LF2CharacterDataWrapper(77, data);
                wrappers.Add(dat, wrapper);
            }
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "input-state.dat") }, id => wrapper);
            entity = world.LogicEntityFactory.Create(new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = 0, dir = "right", nativeWeaponPieceSpawn = true,
                preserveActionZero = true, opoint = new ObjectPoint { oid = 77, action = 0 }
            }, out _);
            Assert.That(entity, Is.Not.Null);
            entity.AiControlled = false;
            var before = row["before"];
            var raw = before["raw"];
            var r = entity.Runtime;
            entity.DirectWriteNativeRawFramePreserveWaitCounter((int)raw["frame"]["action"]);
            entity.Frame.Prev = (int)raw["frame"]["previousAction"];
            entity.Frame.Prev2 = (int)raw["frame"]["tickActionSnapshot"];
            entity.Frame.Prev2D = entity.FrameCache.GetNativeFrameDataById(entity.Frame.Prev2);
            r.PrevFrame2 = entity.Frame.Prev2;
            entity.Trans.SyncDirectFrameData(entity.Frame.D?.wait ?? 0, entity.Frame.D?.next ?? 0, (int)raw["frame"]["actionLatch"]);
            entity.AttackingCounter = (int)raw["frame"]["frameCounter"];
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
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            entity.RefreshRuntimeSnapshot();
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }
    }
}
#endif
