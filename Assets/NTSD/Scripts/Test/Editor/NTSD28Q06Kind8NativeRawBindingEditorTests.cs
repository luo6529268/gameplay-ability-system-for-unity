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
    public sealed class NTSD28Q06Kind8NativeRawBindingEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-KIND8-NATIVE-RAW-BINDING-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void Kind8AndFollowingTickMatchSource(BattleRuntimeProfile profile)
        {
            RunMatrix(profile, false);
        }

        internal static void RunMatrix(BattleRuntimeProfile profile, bool renderer)
        {
            var beforeDifferences = new List<string>();
            var immediateDifferences = new List<string>();
            var followingDifferences = new List<string>();
            var actual = new List<object>();
            int cases = 0;
            int borrowers = renderer ? LF2ObjectPool.Instance.ActiveObjectCountForAcceptance : 0;
            foreach (var row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = CreateWorld(row, profile, renderer);
                try
                {
                    string label = "case " + row["index"];
                    Compare(world, row["before"], label + " before", beforeDifferences);
                    var attacker = world.FindEntityByRuntimeSlotForQuery(0);
                    var target = world.FindEntityByRuntimeSlotForQuery(70);
                    var observer = new Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacyCalls = world.Rng.CallCount;
                    bool applied = BattleKind8ControlRelationWriter.TryApply(world, attacker, target, attacker.Frame.D.itrs[0]);
                    if (applied != (bool)row["applied"]) immediateDifferences.Add(label + " applied differs");
                    Compare(world, row["after"], label + " kind8", immediateDifferences);
                    CompareJson(row["calls"], observer.Capture(), label + " calls", immediateDifferences);
                    if (world.Rng.CallCount != legacyCalls) immediateDifferences.Add(label + " legacy RNG changed");
                    var after = Capture(world);
                    observer = new Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    Compare(world, row["following"], label + " following", followingDifferences);
                    CompareJson(row["followingCalls"], observer.Capture(), label + " following calls", followingDifferences);
                    actual.Add(new { index = (int)row["index"], applied, after, following = Capture(world) });
                    cases++;
                }
                finally
                {
                    Shutdown(world, renderer);
                    if (renderer) Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + (renderer ? "-renderer" : "") + ".json", JsonConvert.SerializeObject(new
            {
                cases, renderer, beforeDifferences, immediateDifferences, followingDifferences, actual,
                scope = "Source134 canonical kind8 writer plus following complete tick; existing three unbound raw fields excluded. Renderer factory only when requested; no formal image parity claim."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(134));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(12)));
            Assert.That(immediateDifferences, Is.Empty, string.Join("\n", immediateDifferences.Take(12)));
            Assert.That(followingDifferences, Is.Empty, string.Join("\n", followingDifferences.Take(12)));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void Kind8SurvivesLocalSnapshotReplay(BattleRuntimeProfile profile)
        {
            int cases = 0;
            foreach (var row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = CreateWorld(row, profile, false);
                try
                {
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                    var cursor = world.NativeRandom.CaptureSynchronizedCursor();
                    string initial = Capture(world).ToString(Formatting.None);
                    string[] expected = ExecuteKind8AndTwoTicks(world);
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    Assert.That(world.NativeRandom.CanCommitSynchronizedCursor(cursor), Is.False);
                    Assert.That(Capture(world).ToString(Formatting.None), Is.EqualTo(initial), "initial case " + row["index"]);
                    Assert.That(ExecuteKind8AndTwoTicks(world), Is.EqualTo(expected), "replay case " + row["index"]);
                    cases++;
                }
                finally { Shutdown(world, false); }
            }
            Assert.That(cases, Is.EqualTo(134));
            File.WriteAllText(Output + "replay-" + profile + ".json", JsonConvert.SerializeObject(new
            {
                cases, replayedTicks = cases * 2,
                scope = "Same-world kind8 transaction plus two complete ticks; not cross-world or renderer validation."
            }, Formatting.Indented));
        }

        private static string[] ExecuteKind8AndTwoTicks(SimulationWorld world)
        {
            var attacker = world.FindEntityByRuntimeSlotForQuery(0);
            var target = world.FindEntityByRuntimeSlotForQuery(70);
            BattleKind8ControlRelationWriter.TryApply(world, attacker, target, attacker.Frame.D.itrs[0]);
            var result = new List<string> { Capture(world).ToString(Formatting.None) };
            var driver = new NTSDBattleTickSystem(world);
            for (int tick = 1; tick <= 2; tick++)
            {
                driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                result.Add(Capture(world).ToString(Formatting.None));
            }
            return result.ToArray();
        }

        private static JObject Capture(SimulationWorld world)
        {
            var inputRows = ((object[])ProjectInput.Invoke(null, new object[] { world })).Select(JObject.FromObject).ToArray();
            var entities = new JArray();
            foreach (int slot in new[] { 0, 70 })
            {
                var e = world.FindEntityByRuntimeSlotForQuery(slot);
                if (e == null) { entities.Add(JValue.CreateNull()); continue; }
                var frame = e.Frame.D;
                var snapshot = e.Frame.Prev2D;
                entities.Add(new JObject
                {
                    ["healTimer"] = e.Runtime.HealTimer,
                    ["ordinaryCreditGate2F4"] = e.Runtime.OrdinaryCreditGate2F4,
                    ["hpConsumed"] = e.Runtime.InputHpConsumedTotal34C,
                    ["mpConsumed"] = e.Runtime.InputMpConsumedTotal350,
                    ["link"] = e.Runtime.LinkState, ["parent"] = e.Runtime.HolderStableId, ["child"] = e.Runtime.TargetSlotIndex,
                    ["available"] = frame != null, ["state"] = frame?.state ?? 0, ["wait"] = frame?.wait ?? 0, ["next"] = frame?.next ?? 0,
                    ["snapshotAvailable"] = snapshot != null, ["snapshotState"] = snapshot?.state ?? 0,
                    ["input"] = inputRows.Single(row => (int)row["slot"] == slot)
                });
            }
            return new JObject
            {
                ["entities"] = entities,
                ["random"] = JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() })),
                ["raw"] = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))
            };
        }

        private static void Compare(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            var rows = (JArray)expected["entities"];
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world,
                new JArray(rows.Where(e => e.Type != JTokenType.Null).Select(e => e["raw"].DeepClone())), label, differences);
            var actual = Capture(world);
            var extras = (JArray)rows.DeepClone();
            foreach (var row in extras.OfType<JObject>()) row.Remove("raw");
            CompareJson(extras, actual["entities"], label + " entities", differences);
            CompareJson(expected["random"], actual["random"], label + " RNG", differences);
        }

        private static SimulationWorld CreateWorld(JObject row, BattleRuntimeProfile profile, bool renderer, bool genericShell = false)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            try
            {
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.SetLogicOnlyEntityMaterialization(!renderer);
                world.Runtime.Stage.StageWidthPx = (int)row["config"]["stageWidth"];
                world.Runtime.Stage.BaseStageWidthPx = (int)row["config"]["stageWidth"];
                world.Runtime.Stage.XMaxOverride = 0;
                world.Runtime.Stage.ZMin = (int)row["config"]["stageNear"];
                world.Runtime.Stage.ZMax = (int)row["config"]["stageFar"];
                world.Runtime.NativeWorldClock.ResourcePhase12 = 0;
                int childOid = (int?)row["params"]["childOid"] ?? 78;
                bool refill = (bool?)row["params"]["refillMode"] == true;
                var definitions = new[] { Definition(77, (string)row["attackerDat"], refill), Definition(childOid, (string)row["targetDat"], refill) };
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "held-parent.dat"), new ObjectDefinition(childOid, 0, "held-child.dat")
                }, id => id == 77 ? definitions[0] : id == childOid ? definitions[1] : null);
                var entities = new LF2Entity[2];
                for (int index = 0; index < 2; index++)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = index == 0 ? 0 : 70, dir = "right", nativeWeaponPieceSpawn = true,
                        preserveActionZero = true, relationTeam = 0, opoint = new ObjectPoint { oid = index == 0 ? 77 : childOid, action = 10 }
                    };
                    if (genericShell && index == 1)
                    {
                        Assert.That(renderer, Is.False);
                        var child = new LF2OtherObject { ObjectId = childOid };
                        child.FrameCache.Load(definitions[1]);
                        child.SetRequiredRuntimeSlot(70);
                        world.Register(child);
                        entities[index] = child;
                        Assert.That(world.FindEntityByRuntimeSlotForQuery(70), Is.TypeOf<LF2OtherObject>());
                    }
                    else
                    {
                        entities[index] = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                    }
                    Assert.That(entities[index], Is.Not.Null);
                    entities[index].AiControlled = false;
                }
                // The second spawn may clear old slot links; restore both sides only after all creations.
                for (int index = 0; index < 2; index++) Restore(entities[index], row["before"]["entities"][index]);
                world.NativeRandom.ResetFromSeed((uint)row["params"]["seed"]);
                return world;
            }
            catch { Shutdown(world, renderer); throw; }
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
            r.HealTimer = (int)before["healTimer"];
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
            r.LinkState = (int)before["link"];
            r.HolderStableId = (int)before["parent"];
            r.TargetSlotIndex = (int)before["child"];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            entity.RefreshRuntimeSnapshot();
        }

        private static void CompareJson(JToken expected, JToken actual, string path, List<string> differences)
        {
            if (expected is JObject obj)
            {
                if (actual is not JObject actualObject)
                {
                    differences.Add(path + " object differs: " + actual);
                    return;
                }
                foreach (var property in obj.Properties()) CompareJson(property.Value, actualObject[property.Name], path + "." + property.Name, differences);
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

        private static void Shutdown(SimulationWorld world, bool renderer)
        {
            world.NativeRandom.SetDiagnosticCallObserver(null);
            if (renderer)
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
            NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
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

    [InitializeOnLoad]
    internal static class NTSD28Q06Kind8PlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_Kind8Play.request";
        private const string Result = "Temp/NTSD28_Q06_Kind8Play.result.json";
        static NTSD28Q06Kind8PlayProbe() { EditorApplication.update += Poll; }

        public static void StartPlayAcceptance()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += StartPlayAcceptance;
                return;
            }
            try
            {
                Assert.That(EditorApplication.isPlayingOrWillChangePlaymode, Is.False);
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty, Is.False);
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/NTSD/Scene/NTSD_Battle.unity");
                File.WriteAllText(Request, "run");
                EditorApplication.isPlaying = true;
            }
            catch (Exception exception)
            {
                File.WriteAllText(Result, JsonConvert.SerializeObject(new { status = "START_FAILED", error = exception.ToString() }, Formatting.Indented));
                throw;
            }
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            string checksum = world.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum;
            var errors = new List<string>();
            int passedCases = 0;
            foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
                foreach (bool renderer in new[] { false, true })
                {
                    try
                    {
                        NTSD28Q06Kind8NativeRawBindingEditorTests.RunMatrix(profile, renderer);
                        passedCases += 134;
                    }
                    catch (Exception exception) { errors.Add(profile + "/" + renderer + ": " + exception); }
                }
            bool unchanged = world.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum;
            int borrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            string status = errors.Count == 0 && passedCases == 536 && unchanged && borrowersAfter == borrowers ? "PASS" : "FAIL";
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status, errors, passedCases, sceneChecksumUnchanged = unchanged,
                rendererBorrowersBefore = borrowers, rendererBorrowersAfter = borrowersAfter,
                scope = "Real Scene preserved; synthetic source134 in both profiles/factories. Physical-key and image parity not claimed; separate Q05 shutdown result required."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
            File.WriteAllText("Temp/NTSD28_Q05_ReplayPlay.request", "run");
        }
    }
}
#endif
