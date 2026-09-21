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
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OpointMaterializerEditorTests
    {
        private const string Source = "artifacts/diagnostics/NTSD28-Q06-OPOINT-MATERIALIZER-SOURCE-WITNESS-001/source-final/first.jsonl";
        private const string Output = "artifacts/diagnostics/NTSD28-Q06-OPOINT-MATERIALIZER-TRANSACTION-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static IEnumerable<TestCaseData> Cases()
        {
            for (int index = 0; index < 23; index++)
                foreach (bool component in new[] { false, true })
                    yield return new TestCaseData(index, component).SetName("OpointSource_" + index + "_" + (component ? "ComponentLateLogicMaterializer" : "WorldLogicLate"));
        }

        private const string BoundarySource = Output + "boundary-source/first.jsonl";
        private static IEnumerable<TestCaseData> BoundaryCases()
        {
            for (int index = 0; index < 6; index++)
                foreach (bool component in new[] { false, true })
                    yield return new TestCaseData(index, component).SetName("OpointBoundary_" + index + "_" + component);
        }

        [TestCaseSource(nameof(Cases))]
        public void ImmediateMatchesOriginalMaterializer(int index, bool component)
            => VerifyImmediate(index, component, Source, "12ACA9132D088999F1771B3A7170DFF56141708FD464581810D780B57F250FD7", "immediate-");

        [TestCaseSource(nameof(BoundaryCases))]
        public void BoundaryImmediateMatchesSource(int index, bool component)
            => VerifyImmediate(index, component, BoundarySource, "0B06159112E90AE615F20991CE666E67BF017186D8B4444DF28FC0F504B2A3B9", "boundary-");

        private static void VerifyImmediate(int index, bool component, string sourcePath, string expectedHash, string outputPrefix)
        {
            using (var input = File.OpenRead(sourcePath))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", ""), Is.EqualTo(expectedHash));
            var row = JObject.Parse(File.ReadAllLines(sourcePath)[index]);
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            SimulationWorld world = null;
            GameObject host = null;
            bool previousLogPolicy = LogAssert.ignoreFailingMessages;
            try
            {
                // Missing-definition diagnostics are expected; outcome/entity assertions remain strict.
                LogAssert.ignoreFailingMessages = true;
                world = CreateWorld(row);
                JObject before = Capture(world);
                CompareWorld(row["before"], before, "before", beforeDifferences);
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                var parent = world.FindEntityByRuntimeSlotForQuery(20);
                if (component)
                {
                    host = new GameObject("OpointMaterializerFixture") { hideFlags = HideFlags.HideAndDontSave };
                    host.SetActive(false);
                    var factory = host.AddComponent<LF2ObjectPointFactory>();
                    typeof(LF2ObjectPointFactory).GetMethod("ProcessOpointSpawnCoreForStructuralWriter", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(factory, new object[] { parent });
                }
                else
                    world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                JObject after = Capture(world);
                CompareWorld(row["after"], after, "after", differences);
                CompareJson(row["calls"], observer.Capture(), "calls", differences);
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + outputPrefix + index + "-" + component + ".json", JsonConvert.SerializeObject(new
                {
                    index, name = (string)row["name"], component,
                    scope = "Actual late caller, logic-only world; component route does not certify Renderer factory",
                    beforeDifferences, differences, before, after, calls = observer.Capture(),
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty, "Fixture initial state mismatch: " + string.Join("\n", beforeDifferences.Take(8)));
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(16)));
            }
            finally
            {
                if (world != null)
                {
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    world.BeginBattleShutdown();
                    Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                }
                if (host != null) UnityEngine.Object.DestroyImmediate(host);
                LogAssert.ignoreFailingMessages = previousLogPolicy;
            }
        }

        [TestCase(0)]
        [TestCase(15)]
        public void FollowingFullTickAndSnapshotReplayMatchSource(int index)
            => VerifyFollowing(index, Source, "following-");

        [TestCase(4)]
        [TestCase(5)]
        public void BoundaryFollowingAndReplayMatchSource(int index)
            => VerifyFollowing(index, BoundarySource, "boundary-following-");

        private static void VerifyFollowing(int index, string sourcePath, string prefix)
        {
            var row = JObject.Parse(File.ReadAllLines(sourcePath)[index]);
            var world = CreateWorld(row);
            try
            {
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                var parent = world.FindEntityByRuntimeSlotForQuery(20);
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                var immediateDifferences = new List<string>();
                CompareWorld(row["after"], Capture(world), "immediate", immediateDifferences);
                Assert.That(immediateDifferences, Is.Empty);
                // Source options have native AI disabled and zero controls for this full tick.
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    var entity = world.FindEntityByRuntimeSlotIncludingPending(slot);
                    if (entity != null) entity.AiControlled = false;
                }
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(observer);
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                var following = Capture(world);
                var differences = new List<string>();
                CompareWorld(row["following"], following, "following", differences);
                CompareJson(row["followingCalls"], observer.Capture(), "following.calls", differences);
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + prefix + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, differences, following, calls = observer.Capture(),
                    scope = "Actual full tick after helper birth, synthetic catalog, source native AI disabled; no counter compensation."
                }, Formatting.Indented));
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(30)));
                ulong checksum = world.CaptureRuntimeChecksum64(1, input);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                var replayObserver = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(replayObserver);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(replayObserver);
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                var replayDifferences = new List<string>();
                CompareWorld(following, Capture(world), "replay", replayDifferences);
                CompareJson(observer.Capture(), replayObserver.Capture(), "replay.calls", replayDifferences);
                Assert.That(replayDifferences, Is.Empty, string.Join("\n", replayDifferences.Take(30)));
                Assert.That(world.CaptureRuntimeChecksum64(1, input), Is.EqualTo(checksum));
                TestContext.WriteLine("Source following and full snapshot replay PASS; checksum=" + checksum);
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(null);
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        internal static void VerifyRendererBirthForPlay(int index)
        {
            bool boundary = index >= 100;
            var row = JObject.Parse(File.ReadAllLines(boundary ? BoundarySource : Source)[boundary ? index - 100 : index]);
            var world = CreateWorld(row);
            try
            {
                world.SetLogicOnlyEntityMaterialization(false);
                var beforeDifferences = new List<string>();
                CompareWorld(row["before"], Capture(world), "before", beforeDifferences);
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                var parent = world.FindEntityByRuntimeSlotForQuery(20);
                world.StructuralWriter.ProcessLateOpointSegment(world.ResolveLateObjectPointStructuralMaterializerForModule(), parent, 1);
                var after = Capture(world);
                var differences = new List<string>();
                CompareWorld(row["after"], after, "after", differences);
                CompareJson(row["calls"], observer.Capture(), "calls", differences);
                foreach (var entity in (JArray)after["entities"])
                {
                    int slot = (int)entity["raw"]["slot"];
                    if (slot >= 50)
                        Assert.That(world.FindEntityByRuntimeSlotIncludingPending(slot)?.Renderer, Is.Not.Null, "Renderer slot " + slot);
                }
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "renderer-" + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, beforeDifferences, differences, after, calls = observer.Capture(),
                    scope = "True pooled Renderer children in isolated World; synthetic logic-only parent."
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty);
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(30)));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        private static SimulationWorld CreateWorld(JObject row)
        {
            var world = new SimulationWorld();
            try
            {
                world.SetLogicOnlyEntityMaterialization(true);
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                var definitions = new Dictionary<int, LF2CharacterDataWrapper>();
                var entries = new List<ObjectDefinition>();
                const string neutral = "<bmp_begin>\nname: Neutral\n<bmp_end>\n<frame> 0 idle\nstate: 0 wait: 100 next: 0\n<frame_end>\n";
                Add(31980, 0, (string)row["sourceDat"]);
                if ((string)row["name"] != "missing_oid_stops_frame" && (int)row["oid"] > 0) Add((int)row["oid"], (int)row["type"], (string)row["targetDat"]);
                Add(778, 5, neutral);
                Add(31999, 5, neutral);
                world.PrepareRuntimeDataCatalogForBattle(entries.ToArray(), id => definitions.TryGetValue(id, out var data) ? data : null);
                var parent = Spawn(31980, 20);
                int freeSlots = (int)row["freeSlots"];
                if (freeSlots <= 2)
                    for (int slot = 50 + freeSlots; slot < world.RuntimeSlotCapacityForDiagnostics; slot++) Spawn(31999, slot);
                Restore(parent, row["before"]["entities"][0]);
                world.CharacterInputWriter.SynchronizeNativeExactAiStateFromRuntime(parent.Runtime);
                world.NativeRandom.ResetFromSeed(42);
                return world;

                void Add(int oid, int type, string dat)
                {
                    string root = Path.GetFullPath(Output + "fixture-runtime");
                    var data = CharacterAnimtorManager.BuildCharacterDataFromSource(dat, Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root));
                    data.type_sub = type;
                    definitions.Add(oid, new LF2CharacterDataWrapper(oid, data));
                    entries.Add(new ObjectDefinition(oid, type, "fixture-" + oid + ".dat"));
                }
                LF2Entity Spawn(int oid, int slot)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right", preserveActionZero = true,
                        opoint = new ObjectPoint { oid = oid, action = 0 }
                    };
                    var entity = world.LogicEntityFactory.Create(task, out var failure);
                    Assert.That(entity, Is.Not.Null, failure.ToString());
                    entity.AiControlled = false;
                    return entity;
                }
            }
            catch
            {
                world.BeginBattleShutdown();
                world.TryShutdownAndClearLogicState(out _, out _);
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
                    ["raw"] = raw.DeepClone(), ["input"] = inputs.Single(item => (int)item["slot"] == slot),
                    ["extra"] = new JArray(r.HitStop, r.HitResourceInjuryDouble1A0, r.HitResourceSuppression15C,
                        r.OrdinaryCreditGate2F4, r.IncomingDamageScale340, r.HP2Orig, r.RespawnCount, r.HPOrig, r.ReviveVisualId184,
                        e.Trans.WaitCounter, e.Frame.Prev, r.AttackingCounter, r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208, r.WeaponFlightCounter),
                    ["links"] = new JArray(r.LinkState, r.HolderStableId, r.TargetSlotIndex),
                    ["position"] = new JArray(r.XInt, r.YInt, r.ZInt, r.X, r.Y, r.Z),
                    ["motion"] = new JArray(r.Vx, r.Vy, r.Vz),
                });
            }
            return new JObject { ["entities"] = entities, ["fillers"] = fillers,
                ["random"] = JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() })) };
        }

        private static void CompareWorld(JToken expected, JObject actual, string label, List<string> differences)
        {
            var actualRows = (JArray)actual["entities"];
            CompareJson(((JArray)expected["entities"]).Count, actualRows.Count, label + ".nonFillerCount", differences);
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
            var extra = before["extra"];
            r.HitResourceInjuryDouble1A0 = (int)extra[1]; r.HitResourceSuppression15C = (int)extra[2];
            r.OrdinaryCreditGate2F4 = (int)extra[3]; r.IncomingDamageScale340 = (int)extra[4];
            r.ReviveVisualId184 = (int)extra[8];
            r.DisplayCurrentHp200 = (int)extra[12]; r.DisplayEffectiveMaxHp208 = (int)extra[13];
            r.LinkState = (int)before["links"][0]; r.HolderStableId = (int)before["links"][1]; r.TargetSlotIndex = (int)before["links"][2];
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
