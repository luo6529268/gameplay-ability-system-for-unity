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
    public sealed class NTSD28Q06StandardHitFall80PreservationEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001/";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();
        private static readonly int[] RepresentativeIndices = { 0, 1, 2 };

        [TestCase(BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameAdvancePassMode.DataOriented)]
        public void StandardHitAndFollowingTickMatchSource(BattleRuntimeProfile profile, BattleEcsCharacterFrameAdvancePassMode mode)
        {
            RunMatrix(profile, mode, false);
        }

        internal static void RunMatrix(BattleRuntimeProfile profile, BattleEcsCharacterFrameAdvancePassMode mode, bool renderer)
        {
            var beforeDifferences = new List<string>();
            var immediateDifferences = new List<string>();
            var followingDifferences = new List<string>();
            var actual = new List<object>();
            int cases = 0;
            int borrowers = renderer ? LF2ObjectPool.Instance.ActiveObjectCountForAcceptance : 0;
            int[] smoke = RepresentativeIndices;
            var rows = File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse);
            if (profile != BattleRuntimeProfile.Authority400 || renderer)
                rows = rows.Where(row => smoke.Contains((int)row["index"]));
            foreach (var row in rows)
            {
                var world = CreateWorld(row, profile, mode, renderer);
                try
                {
                    string label = "case " + row["index"];
                    Compare(world, row["before"], label + " before", beforeDifferences);
                    var observer = new Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacyCalls = world.Rng.CallCount;
                    var attacker = world.FindEntityByRuntimeSlotForQuery(0);
                    var target = world.FindEntityByRuntimeSlotForQuery(70);
                    bool applied = world.DamageWriter.ApplyStandardCharacterDamage(world, attacker, target, ((LF2Character)target).HitCounters, attacker.Frame.D.itrs[0]);
                    if (!applied) immediateDifferences.Add(label + " applied differs");
                    Compare(world, row["after"], label + " effect", immediateDifferences);
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
                    actual.Add(new { index = (int)row["index"], after, following = Capture(world) });
                    cases++;
                }
                finally
                {
                    Shutdown(world, renderer);
                    if (renderer) Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                }
            }
            File.WriteAllText(Output + profile + "-" + mode + (renderer ? "-renderer" : "") + ".json", JsonConvert.SerializeObject(new
            {
                cases, mode, renderer, beforeDifferences, immediateDifferences, followingDifferences, actual,
                sourceOnlyExtras = new[] { "previousX", "previousY", "previousZ" },
                scope = "Actual unarmored effect transaction and following full tick; native raw47 mapped fields plus descriptor/B2/RNG. Source previousXYZ have no asserted Unity carrier; not platform or image parity."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(profile == BattleRuntimeProfile.Authority400 && !renderer ? 3 : 3));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(12)));
            Assert.That(immediateDifferences, Is.Empty, string.Join("\n", immediateDifferences.Take(12)));
            Assert.That(followingDifferences, Is.Empty, string.Join("\n", followingDifferences.Take(12)));
        }

        [TestCase(BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(BattleHitExecutionPlanMode.DataOriented)]
        public void CapturedHitPlanPreservesSourceReactionTimer(BattleHitExecutionPlanMode mode)
        {
            RunCapturedHitPlan(mode, Output + "source/first.jsonl");
        }

        [TestCase(BattleHitExecutionPlanMode.ShadowCompare)]
        [TestCase(BattleHitExecutionPlanMode.DataOriented)]
        public void NonzeroVerticalProjectionMatchesSource(BattleHitExecutionPlanMode mode)
        {
            RunCapturedHitPlan(mode, "artifacts/diagnostics/NTSD28-Q06-STANDARD-HIT-PENDING-Y-PROJECTION-001/source/first.jsonl");
        }

        internal static void RunCapturedHitPlan(BattleHitExecutionPlanMode mode, string sourceFile, bool renderer = false)
        {
            int borrowers = renderer ? LF2ObjectPool.Instance.ActiveObjectCountForAcceptance : 0;
            foreach (var row in File.ReadLines(sourceFile).Select(JObject.Parse))
            {
                var world = CreateWorld(row, BattleRuntimeProfile.Authority400, BattleEcsCharacterFrameAdvancePassMode.DataOriented, renderer);
                try
                {
                    world.ConfigureBattleHitExecutionPlanForDiagnostics(mode);
                    var attacker = world.FindEntityByRuntimeSlotForQuery(0);
                    var target = world.FindEntityByRuntimeSlotForQuery(70);
                    var query = (BruteForceSceneQuery)world.SceneQuery;
                    query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
                    world.CaptureCollisionFrameSnapshotsAll();
                    world.CollectCollisionCandidatesAll();
                    Assert.That(query.TryGetCollisionCandidateRange(attacker, out CollisionCandidateRange range), Is.True);
                    Assert.That(range.Count, Is.EqualTo(1));
                    world.PostInteractionTickAll(0);
                    var diagnostics = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                    if (!EditorApplication.isPlaying)
                        TestContext.WriteLine(JsonConvert.SerializeObject(new { index = row["index"], diagnostics, hp = target.Runtime.HP, fall = target.Runtime.Fall }));
                    Assert.That(diagnostics.FailureCount, Is.Zero);
                    Assert.That(diagnostics.ObservationMismatchCount, Is.Zero);
                    if (mode == BattleHitExecutionPlanMode.ShadowCompare)
                        Assert.That(diagnostics.ObservedWriterEffectCount, Is.GreaterThan(0));
                    Assert.That(target.Runtime.KnockbackVy, Is.EqualTo((double)row["after"]["entities"][1]["pendingY"]), "pendingY case " + row["index"]);
                    Assert.That(target.Runtime.HP, Is.EqualTo((int)row["after"]["entities"][1]["raw"]["vitals"]["currentHp"]));
                    Assert.That(target.Runtime.Fall, Is.EqualTo((int)row["after"]["entities"][1]["raw"]["combat"]["hitReactionTimer"]), "case " + row["index"]);
                }
                finally
                {
                    Shutdown(world, renderer);
                    if (renderer) Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                }
            }
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
                    ["catchSource"] = e.Runtime.CatchSourceSlot90, ["impactSource"] = e.Runtime.ImpactSourceSlot164,
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
                world.Runtime.Stage.StageWidthPx = (int)row["config"]["stageWidth"];
                world.Runtime.Stage.BaseStageWidthPx = (int)row["config"]["stageWidth"];
                world.Runtime.Stage.XMaxOverride = 0;
                world.Runtime.Stage.ZMin = (int)row["config"]["stageNear"];
                world.Runtime.Stage.ZMax = (int)row["config"]["stageFar"];
                var definitions = new[] { Definition(77, (string)row["attackerDat"]), Definition(78, (string)row["targetDat"]) };
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "effect-attacker.dat"), new ObjectDefinition(78, 0, "effect-target.dat")
                }, id => id == 77 ? definitions[0] : id == 78 ? definitions[1] : null);
                int[] slots = { 0, 70 };
                var entities = new LF2Entity[slots.Length];
                for (int i = 0; i < slots.Length; i++)
                {
                    int oid = slots[i] == 0 ? 77 : 78;
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slots[i], dir = "right", nativeWeaponPieceSpawn = true,
                        preserveActionZero = true, relationTeam = 0, opoint = new ObjectPoint { oid = oid, action = 10 }
                    };
                    entities[i] = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                    Assert.That(entities[i], Is.Not.Null);
                    entities[i].AiControlled = false;
                }
                for (int i = 0; i < slots.Length; i++) Restore(entities[i], row["before"]["entities"][i]);
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
            r.KnockbackVx = (double)before["pendingX"]; r.KnockbackVy = (double)before["pendingY"]; r.KnockbackVz = (double)before["pendingZ"];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
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

}
#endif
