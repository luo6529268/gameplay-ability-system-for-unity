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
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeInputActionCostEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-INPUT-ACTION-COST-FRAME-READERS-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameAdvancePassMode.Legacy)]
        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameAdvancePassMode.DataOriented)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameAdvancePassMode.Legacy)]
        [TestCase(BattleRuntimeProfile.MobileExtended, BattleEcsCharacterFrameAdvancePassMode.DataOriented)]
        public void FollowingTickPreservesNativeCostAndLifetime(BattleRuntimeProfile profile, BattleEcsCharacterFrameAdvancePassMode physicsMode)
        {
            RunFollowingTickMatrix(profile, physicsMode, false);
        }

        internal void RunFollowingTickMatrix(BattleRuntimeProfile profile, BattleEcsCharacterFrameAdvancePassMode physicsMode, bool renderer)
        {
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            var actual = new List<object>();
            int cases = 0;
            foreach (var row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = CreateWorld(row, profile, out var entity, renderer);
                world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(physicsMode);
                try
                {
                    string label = "case " + row["index"];
                    Compare(world, entity, row["before"], label + " before", beforeDifferences);
                    new NTSD28InputTwoPassModule(world, null).ProcessNativeSampledState(entity);
                    Compare(world, entity, row["after"], label + " input", beforeDifferences);
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false,
                        new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    var survivor = world.FindEntityByRuntimeSlotForQuery(0);
                    bool expectedPresent = row["following"].Type != JTokenType.Null;
                    if ((survivor != null) != expectedPresent) differences.Add(label + " lifetime mismatch");
                    if (expectedPresent && survivor != null) Compare(world, survivor, row["following"], label + " following", differences);
                    else if (!expectedPresent)
                        NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, new JArray(), label + " following", differences);
                    CompareJson(row["followingRandom"], CaptureRandom(world), label + " following RNG", differences);
                    if (observer.CrtCalls != row["followingCrtCalls"].Count()) differences.Add(label + " following CRT count differs");
                    var expectedCalls = row["followingSynchronizedCalls"].Select(call => new long[]
                    {
                        (long)call["callSite"], (long)call["upperBound"], (long)call["result"],
                        (long)call["counterAfter"], (long)call["indexAfter"], (long)call["totalCalls"]
                    }).ToArray();
                    CompareJson(JArray.FromObject(expectedCalls), JArray.FromObject(observer.Calls), label + " following calls", differences);
                    actual.Add(new
                    {
                        index = (int)row["index"], raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1)),
                        random = CaptureRandom(world), input = survivor == null ? null : CaptureInput(world),
                        resources = survivor == null ? null : CaptureResources(survivor)
                    });
                    cases++;
                }
                finally { ShutdownWorld(world, renderer); }
            }
            File.WriteAllText(Output + "following-" + profile + "-" + physicsMode + (renderer ? "-renderer" : "") + ".json", JsonConvert.SerializeObject(new
            {
                cases, beforeDifferences, differences, actual,
                scope = "Same synthetic input transaction followed by one complete tick; actual native resource/input/RNG/lifetime. Formal content and physical keys are separate."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(151));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(12)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(20)));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void NativeActionCostPathsMatchSource(BattleRuntimeProfile profile)
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
                        input = CaptureInput(world), random = CaptureRandom(world), resources = CaptureResources(entity), runAccumulator = entity.Runtime.AnimSub,
                        descriptorAvailable = entity.Frame.D != null });
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + ".json", JsonConvert.SerializeObject(new
            {
                cases, beforeDifferences, differences, actual,
                scope = "Declared action-field, encoded redirect, fallback, rowing and builtin input transactions; raw/B2/RNG/resources. Pending input and source action messages are diagnostic only; synthetic content."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(151));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(15)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(20)));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void NativeCostSurvivesLocalSnapshotReplay(BattleRuntimeProfile profile)
        {
            int cases = 0;
            foreach (var row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = CreateWorld(row, profile, out _);
                try
                {
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                    var cursor = world.NativeRandom.CaptureSynchronizedCursor();
                    string initial = ReplaySignature(world, 0);
                    string[] expected = ExecuteInputAndTwoTicks(world);
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    Assert.That(world.NativeRandom.CanCommitSynchronizedCursor(cursor), Is.False);
                    Assert.That(ReplaySignature(world, 0), Is.EqualTo(initial), "initial case " + row["index"]);
                    Assert.That(ExecuteInputAndTwoTicks(world), Is.EqualTo(expected), "replay case " + row["index"]);
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            Assert.That(cases, Is.EqualTo(151));
            File.WriteAllText(Output + "replay-" + profile + ".json", JsonConvert.SerializeObject(new
            {
                cases, replayedTicks = cases * 2, snapshotTick = 0,
                scope = "Same-world before-input snapshot; actual cost transaction plus two complete ticks. No cross-world or deployed-content claim."
            }, Formatting.Indented));
        }

        private static string[] ExecuteInputAndTwoTicks(SimulationWorld world)
        {
            new NTSD28InputTwoPassModule(world, null).ProcessNativeSampledState(world.FindEntityByRuntimeSlotForQuery(0));
            var result = new List<string> { ReplaySignature(world, 0) };
            var driver = new NTSDBattleTickSystem(world);
            for (int tick = 1; tick <= 2; tick++)
            {
                driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                result.Add(ReplaySignature(world, tick));
            }
            return result.ToArray();
        }

        private static string ReplaySignature(SimulationWorld world, int tick)
        {
            var entity = world.FindEntityByRuntimeSlotForQuery(0);
            return JsonConvert.SerializeObject(new
            {
                tick,
                raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"],
                input = ProjectInput.Invoke(null, new object[] { world }),
                resources = entity == null ? null : CaptureResources(entity),
                random = CaptureRandom(world)
            });
        }

        private static JObject CaptureInput(SimulationWorld world)
            => JObject.FromObject(((object[])ProjectInput.Invoke(null, new object[] { world }))[0]);

        private static JObject CaptureRandom(SimulationWorld world)
            => JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() }));

        private static JObject CaptureResources(LF2Entity entity)
        {
            var r = entity.Runtime;
            return new JObject
            {
                ["local"] = r.InputLocalResourceEnabled49D034, ["gate194"] = r.InputSpecialGate194,
                ["modeFallback"] = r.InputModeFallbackActionB8, ["lastAction"] = r.InputLastAction144,
                ["hpConsumed"] = r.InputHpConsumedTotal34C, ["mpConsumed"] = r.InputMpConsumedTotal350
            };
        }

        private static void Compare(SimulationWorld world, LF2Entity entity, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, new JArray(expected["raw"].DeepClone()), label, differences);
            CompareJson(expected["input"], CaptureInput(world), label + " input", differences);
            CompareJson(expected["random"], CaptureRandom(world), label + " random", differences);
            CompareJson(expected["resources"], CaptureResources(entity), label + " resources", differences);
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

        private static void ShutdownWorld(SimulationWorld world, bool renderer)
        {
            if (renderer)
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
            NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
        }

        private static SimulationWorld CreateWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity entity, bool renderer = false)
        {
            string dat = (string)row["dat"];
            if (!wrappers.TryGetValue(dat, out var wrapper))
            {
                string runtimeRoot = Path.GetFullPath(Output + "fixture-runtime");
                var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                    Path.Combine(runtimeRoot, "decoded_dat", "input-cost.dat"), BattleContentSource.ForLoganRuntime(runtimeRoot));
                wrapper = new LF2CharacterDataWrapper(77, data);
                wrappers.Add(dat, wrapper);
            }
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            // Match the source following-tick stage bounds, including its zero left edge.
            world.Runtime.Stage.StageWidthPx = 800;
            world.Runtime.Stage.BaseStageWidthPx = 800;
            world.Runtime.Stage.XMaxOverride = 0;
            world.Runtime.Stage.ZMin = -10000;
            world.Runtime.Stage.ZMax = 10000;
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "input-state.dat") }, id => wrapper);
            var task = new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = 0, dir = "right", nativeWeaponPieceSpawn = true,
                preserveActionZero = true, opoint = new ObjectPoint { oid = 77, action = 0 }
            };
            entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
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
            var resources = before["resources"];
            r.InputLocalResourceEnabled49D034 = (bool)resources["local"];
            r.InputSpecialGate194 = (int)resources["gate194"];
            r.InputModeFallbackActionB8 = (int)resources["modeFallback"];
            r.InputHpConsumedTotal34C = (int)resources["hpConsumed"];
            r.InputMpConsumedTotal350 = (int)resources["mpConsumed"];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            entity.RefreshRuntimeSnapshot();
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06NativeCostPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_NativeCostPlay.request";
        static NTSD28Q06NativeCostPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var sceneWorld = driver?.World;
            if (sceneWorld == null || driver.CurrentTickIndex < 5 || !sceneWorld.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            string status = "FAIL", error = null;
            int cases = 0;
            try
            {
                var fixture = new NTSD28Q06NativeInputActionCostEditorTests();
                foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
                    foreach (var mode in new[] { BattleEcsCharacterFrameAdvancePassMode.Legacy, BattleEcsCharacterFrameAdvancePassMode.DataOriented })
                        foreach (bool renderer in new[] { false, true })
                        {
                            fixture.RunFollowingTickMatrix(profile, mode, renderer);
                            cases += 151;
                            Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                        }
                Assert.That(cases, Is.EqualTo(1208));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_NativeCostPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene paused and preserved; source151 synthetic input and full following tick, two profiles, two physics paths, both factories. No physical-key or image claim; separate ordered shutdown required."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
