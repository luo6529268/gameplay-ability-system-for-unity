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
    public sealed class NTSD28Q06NativeAiAliasTickEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-NATIVE-AI-PERSISTED-ALIAS-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(16)]
        [TestCase(17)]
        public void ActualAiTwoTicksMatchNativeSamplingAndRng(int index)
        {
            RunTwoTicks(index, false);
        }

        internal static void RunTwoTicks(int index, bool renderer)
        {
            var row = JObject.Parse(File.ReadAllLines(Output + "source/first.jsonl")[index]);
            var world = CreateWorld(row, BattleRuntimeProfile.Authority400,
                BattleEcsCharacterFrameAdvancePassMode.DataOriented, renderer);
            var beforeDifferences = new List<string>();
            var firstDifferences = new List<string>();
            var secondDifferences = new List<string>();
            try
            {
                Compare(world, row["before"], "before", beforeDifferences);
                var initial = Capture(world);
                var driver = new NTSDBattleTickSystem(world);
                var firstObserver = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(firstObserver);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(firstObserver);
                driver.RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                Compare(world, row["after"], "first", firstDifferences);
                CompareJson(row["calls"], firstObserver.Capture(), "first.calls", firstDifferences);
                var first = Capture(world);
                var secondObserver = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(secondObserver);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(secondObserver);
                driver.RunReleaseTick(2, false, new FrameInputSet(2, Array.Empty<SimulationPlayerInput>()));
                Compare(world, row["later"], "second", secondDifferences);
                CompareJson(row["laterCalls"], secondObserver.Capture(), "second.calls", secondDifferences);
                File.WriteAllText(Output + "world-tick-" + index + ".json", JsonConvert.SerializeObject(new
                {
                    index, beforeDifferences, firstDifferences, secondDifferences,
                    initial, first, second = Capture(world), firstCalls = firstObserver.Capture(), secondCalls = secondObserver.Capture(),
                    scope = "Synthetic source16/17, real World AI producer/sampler and two ticks; previousXYZ and native derived level scalars are source-only."
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(15)));
                Assert.That(firstDifferences, Is.Empty, string.Join("\n", firstDifferences.Take(15)));
                Assert.That(secondDifferences, Is.Empty, string.Join("\n", secondDifferences.Take(15)));
            }
            finally { Shutdown(world, renderer); }
        }

        [Test]
        public void AliasOnlySnapshotRestoreRecapturesRowsAndReplaysTwoTicks()
        {
            var row = JObject.Parse(File.ReadAllLines(Output + "source/first.jsonl")[16]);
            var world = CreateWorld(row, BattleRuntimeProfile.Authority400,
                BattleEcsCharacterFrameAdvancePassMode.DataOriented, false);
            try
            {
                var character = (LF2Character)world.FindEntityByRuntimeSlotForQuery(0);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                string initial = Capture(world).ToString(Formatting.None);
                var oldCursor = world.NativeRandom.CaptureSynchronizedCursor();
                character.Runtime.NativeAiProfileObjectId = 0;
                AssertRowAlias(world, character, 0);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                Assert.That(world.NativeRandom.CanCommitSynchronizedCursor(oldCursor), Is.False);
                Assert.That(Capture(world).ToString(Formatting.None), Is.EqualTo(initial));
                AssertRowAlias(world, character, -1);
                string[] expected = ExecuteTwoTicks(world);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out failure), Is.True, failure.ToString());
                AssertRowAlias(world, character, -1);
                Assert.That(ExecuteTwoTicks(world), Is.EqualTo(expected));
            }
            finally { Shutdown(world, false); }
        }

        private static void AssertRowAlias(SimulationWorld world, LF2Character character, int alias)
        {
            var snapshot = new AiDecisionSnapshot(world.RuntimeSlotCapacityForDiagnostics);
            Assert.That(world.CaptureAiDecisionShadowSnapshotForModule(character, snapshot), Is.EqualTo(AiDecisionAvailability.Available));
            Assert.That(snapshot.Rows.NativeAiProfileObjectId[0], Is.EqualTo(alias));
            Assert.That(snapshot.Rows.ObjectId[0], Is.EqualTo(77));
        }

        private static string[] ExecuteTwoTicks(SimulationWorld world)
        {
            var captured = new List<string>();
            var driver = new NTSDBattleTickSystem(world);
            for (int tick = 1; tick <= 2; tick++)
            {
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.SetAcceptedAiRandomTraceObserverForDiagnostics(observer);
                driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                captured.Add(Capture(world).ToString(Formatting.None));
                captured.Add(observer.Capture().ToString(Formatting.None));
            }
            return captured.ToArray();
        }

        private static JObject Capture(SimulationWorld world)
        {
            var inputRows = ((object[])ProjectInput.Invoke(null, new object[] { world })).Select(JObject.FromObject).ToArray();
            var entities = new JArray();
            foreach (int slot in new[] { 0, 1 })
            {
                var e = world.FindEntityByRuntimeSlotForQuery(slot);
                if (e == null) { entities.Add(JValue.CreateNull()); continue; }
                var frame = e.Frame.D;
                var snapshot = e.GetCollisionFrameData();
                entities.Add(new JObject
                {
                    ["alias"] = e.Runtime.NativeAiProfileObjectId,
                    ["pending"] = new JArray(e.Runtime.KeyUp, e.Runtime.KeyDown, e.Runtime.KeyLeft, e.Runtime.KeyRight, e.Runtime.KeyJump, e.Runtime.KeyDefend, e.Runtime.KeyAttack),
                    ["catchSource"] = e.Runtime.CatchSourceSlot90, ["impactSource"] = e.Runtime.ImpactSourceSlot164,
                    ["pendingCount"] = e.HitCount,
                    ["pendingX"] = e.Runtime.KnockbackVx, ["pendingY"] = e.Runtime.KnockbackVy, ["pendingZ"] = e.Runtime.KnockbackVz,
                    ["environment"] = e.Runtime.EnvironmentState320,
                    ["environmentSource"] = e.Runtime.EnvironmentSourceSlot160,
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
            foreach (var row in extras.OfType<JObject>())
            {
                row.Remove("raw");
                row.Remove("previousX");
                row.Remove("previousY");
                row.Remove("previousZ");
                row.Remove("level618");
                row.Remove("level61c");
            }
            CompareJson(extras, actual["entities"], label + " entities", differences);
            CompareJson(expected["random"], actual["random"], label + " RNG", differences);
        }

        private static SimulationWorld CreateWorld(JObject row, BattleRuntimeProfile profile, BattleEcsCharacterFrameAdvancePassMode mode, bool renderer)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            try
            {
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(mode);
                world.SetLogicOnlyEntityMaterialization(!renderer);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.XMaxOverride = 0;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                var definitions = new[] { Definition(77, (string)row["dat"][0]), Definition(31981, (string)row["dat"][1]) };
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "ai-subject.dat"), new ObjectDefinition(31981, 0, "ai-target.dat")
                }, id => id == 77 ? definitions[0] : id == 31981 ? definitions[1] : null);
                int[] slots = { 0, 1 };
                var entities = new LF2Entity[slots.Length];
                for (int i = 0; i < slots.Length; i++)
                {
                    int oid = slots[i] == 0 ? 77 : 31981;
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slots[i], dir = "right", nativeWeaponPieceSpawn = true,
                        preserveActionZero = true, relationTeam = 0, opoint = new ObjectPoint { oid = oid, action = 110 }
                    };
                    entities[i] = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                    Assert.That(entities[i], Is.Not.Null);
                    entities[i].AiControlled = false;
                }
                for (int i = 0; i < slots.Length; i++) Restore(entities[i], row["before"]["entities"][i]);
                var subject = (LF2Character)entities[0];
                subject.Controller = new EmptyController();
                subject.AiControlled = true;
                world.Runtime.Match.Difficulty = 0;
                // Spawn bound the canonical store before the synthetic initial state was restored.
                foreach (LF2Entity entity in entities)
                    Assert.That(world.CharacterInputWriter.SynchronizeNativeExactAiStateFromRuntime(entity.Runtime), Is.True);
                Assert.That(world.CharacterInputWriter.TryCaptureCanonicalState(subject.Runtime, out var initialInput), Is.True);
                Assert.That(initialInput.KeyUp, Is.EqualTo(1));
                world.Runtime.Flow.AiDifficulty = 0;
                world.Runtime.FunctionKeys.ResetForBattle(true);
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
            r.CatchSourceSlot90 = (int)before["catchSource"]; r.ImpactSourceSlot164 = (int)before["impactSource"];
            entity.HitCount = (int)before["pendingCount"];
            r.NativeAiProfileObjectId = (int)before["alias"];
            r.KnockbackVx = (double)before["pendingX"]; r.KnockbackVy = (double)before["pendingY"]; r.KnockbackVz = (double)before["pendingZ"];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            var pending = before["pending"];
            r.KeyUp = (byte)pending[0]; r.KeyDown = (byte)pending[1];
            r.KeyLeft = (byte)pending[2]; r.KeyRight = (byte)pending[3];
            r.KeyJump = (byte)pending[4]; r.KeyDefend = (byte)pending[5]; r.KeyAttack = (byte)pending[6];

            entity.RefreshRuntimeSnapshot();
        }

        private static void CompareJson(JToken expected, JToken actual, string path, List<string> differences)
        {
            if ((path.EndsWith(".pendingX", StringComparison.Ordinal) ||
                 path.EndsWith(".pendingY", StringComparison.Ordinal) ||
                 path.EndsWith(".pendingZ", StringComparison.Ordinal)) &&
                expected is JValue && actual is JValue &&
                (expected.Type == JTokenType.Integer || expected.Type == JTokenType.Float) &&
                (actual.Type == JTokenType.Integer || actual.Type == JTokenType.Float))
            {
                if (expected.Value<double>() != actual.Value<double>())
                    differences.Add(path + "=" + actual + " expected=" + expected);
                return;
            }
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
            world.SetAcceptedAiRandomTraceObserverForDiagnostics(null);
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
        private sealed class EmptyController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsDefend => false;
            bool ILF2Controller.IsJump => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId) { }
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06NativeAiAliasPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_NativeAiAliasPlay.request";
        private const string Result = "Temp/NTSD28_Q06_NativeAiAliasPlay.result.json";

        static NTSD28Q06NativeAiAliasPlayProbe()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run")
                return;
            var driver = SimulationTickDriver.Instance;
            var scene = driver?.World;
            if (scene == null || driver.CurrentTickIndex < 5 || !scene.IsBattleSnapshotBoundaryReady)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            File.WriteAllText(Request, "running");
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            ulong checksum = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input);
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var errors = new List<string>();
            int passed = 0;
            foreach (int index in new[] { 16, 17 })
            {
                try
                {
                    NTSD28Q06NativeAiAliasTickEditorTests.RunTwoTicks(index, true);
                    passed++;
                }
                catch (Exception error)
                {
                    errors.Add(index + ": " + error);
                }
            }
            bool unchanged = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input) == checksum;
            int after = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status = errors.Count == 0 && passed == 2 && unchanged && borrowers == after ? "PASS" : "FAIL",
                passedCases = passed,
                errors,
                sceneChecksumUnchanged = unchanged,
                rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = after,
                scope = "Real Play source16/17 two ticks each with pooled renderers; synthetic data, no physical-key or pixel parity claim."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
            File.WriteAllText("Temp/NTSD28_Q05_ReplayPlay.request", "run");
        }
    }
}
#endif
