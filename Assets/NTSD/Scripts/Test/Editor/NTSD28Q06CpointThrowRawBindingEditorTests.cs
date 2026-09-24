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
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06CpointThrowRawBindingEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-CPOINT-THROW-NATIVE-RAW-BINDING-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(1333, 730, 148.0, 148, 159)]
        [TestCase(2048, 1152, 173.7464366091523, 173, 184)]
        public void MovedCatcherThrowPosition_KeepsSourceLocalAnchorAtBothViews(
            int viewWidth,
            int viewHeight,
            double expectedCatcherX,
            int expectedCatcherXInt,
            int expectedCaughtXInt)
        {
            JObject row = JObject.Parse(File.ReadLines(Output + "source/first.jsonl").First());
            Assert.That((int)row["index"], Is.Zero);
            Assert.That((int)row["thrown"], Is.EqualTo(1));
            Assert.That((int)row["after"][1]["raw"]["position"]["x"], Is.EqualTo(111));
            SimulationWorld world = MakeWorld(row, BattleRuntimeProfile.Authority400,
                out LF2Entity[] entities);
            try
            {
                world.ConfigureFixedViewRunDistance(viewWidth, viewHeight);
                LF2Entity catcher = entities[0];
                LF2Entity caught = entities[1];
                Assert.That(catcher.Runtime.XInt, Is.EqualTo(100));
                catcher.Runtime.SetVelocity(48, 0, 0);
                new CharacterMechanics().StepBattleLogic(
                    new CharacterMechanicsContext(catcher.Runtime, null, 0f, 0f, 0.0,
                        world.FixedViewRunDistanceScale,
                        world.FixedViewRunVerticalDistanceScale));
                Assert.That(catcher.Runtime.X,
                    Is.EqualTo(expectedCatcherX).Within(1e-10));
                catcher.Runtime.SyncIntegerPosition();
                Assert.That(catcher.Runtime.XInt, Is.EqualTo(expectedCatcherXInt));

                catcher.RunCpointAdvanceStep10();
                Assert.That(caught.Runtime.XInt, Is.EqualTo(expectedCaughtXInt));
                Assert.That(caught.Runtime.XInt - catcher.Runtime.XInt, Is.EqualTo(11));
                Assert.That(caught.Runtime.YInt, Is.EqualTo(-24));
                Assert.That(caught.Runtime.ZInt, Is.EqualTo(200));
                Assert.That(caught.Runtime.Vx, Is.EqualTo(1.5));
                Assert.That(caught.Runtime.Vy, Is.EqualTo(-2.25));
            }
            finally
            {
                ShutdownWorld(world, false);
            }
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void FollowingFullTickMatchesNativeLifetime(BattleRuntimeProfile profile)
        {
            RunFollowingFullTickMatrix(profile, false);
        }

        internal static int RunFollowingFullTickMatrix(BattleRuntimeProfile profile, bool renderer)
        {
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            var actual = new List<object>();
            int cases = 0;
            foreach (JObject row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile, out var entities, true, renderer);
                try
                {
                    string label = "case " + row["index"];
                    Compare(world, entities, row["before"], label + " before", beforeDifferences);
                    foreach (var entity in entities) entity.RunCpointAdvanceStep10();
                    Compare(world, entities, row["after"], label + " immediate", beforeDifferences);
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    var inputStages = new List<object>();
                    object InputStage(string phase, LF2Entity e) => new
                    {
                        phase, slot = e.Runtime.SlotIndex, action = e.Frame.N,
                        available = e.Frame.D != null, wait = e.Trans.Wait, next = e.Trans.Next,
                        e.Runtime.KeyJump, e.Runtime.CdAttack,
                        current = e.Runtime.NativeInputProxy.Current[4], edge = e.Runtime.NativeInputProxy.EdgeWindow[0],
                        world.NeedClearInput, world.InputPhase, world.AiExecutionProfile
                    };
                    inputStages.Add(InputStage("beforeDriver", entities[0]));
                    world.SetCharacterInputProducerPassMutationOverrideForSelfCheck((w, e) => inputStages.Add(InputStage("producer", e)));
                    world.SetCharacterInputPassMutationOverrideForSelfCheck((w, e) => inputStages.Add(InputStage("route", e)));
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false,
                        new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    world.SetCharacterInputProducerPassMutationOverrideForSelfCheck(null);
                    world.SetCharacterInputPassMutationOverrideForSelfCheck(null);
                    var expected = row["nextTick"];
                    NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world,
                        new JArray(expected.Where(e => e.Type != JTokenType.Null).Select(e => e["raw"].DeepClone())),
                        label + " nextTick", differences);
                    for (int slot = 0; slot < entities.Length; slot++)
                    {
                        var entity = world.FindEntityByRuntimeSlotForQuery(slot);
                        bool present = expected[slot].Type != JTokenType.Null;
                        Check((entity != null) == present, label + " slot " + slot + " lifetime differs", differences);
                        if (entity == null || !present) continue;
                        var extra = Extra(entity);
                        foreach (var property in ((JObject)expected[slot]).Properties().Where(p => p.Name != "raw"))
                            Check(JToken.DeepEquals(property.Value, extra[property.Name]),
                                label + " slot " + slot + " " + property.Name + "=" + extra[property.Name] + " expected " + property.Value, differences);
                    }
                    actual.Add(new { index = (int)row["index"], inputStages, raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1)),
                        extra = Enumerable.Range(0, 3).Select(slot => world.FindEntityByRuntimeSlotForQuery(slot) is LF2Entity e ? Extra(e) : null).ToArray() });
                    cases++;
                }
                finally { ShutdownWorld(world, renderer); }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "following-" + profile + (renderer ? "-renderer" : "") + ".json", JsonConvert.SerializeObject(new
            {
                cases, beforeDifferences, differences, actual,
                scope = "One complete tick after the declared synthetic throw; physical input, Play and replay are separate."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(392));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(12)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(20)));
            return cases;
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ImmediateThrowMatchesNativeRawBinding(BattleRuntimeProfile profile)
        {
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            var actual = new List<object>();
            int cases = 0;
            foreach (JObject row in File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile, out var entities);
                try
                {
                    string label = "case " + row["index"];
                    Compare(world, entities, row["before"], label + " before", beforeDifferences);
                    var definitions = entities.Select(e => e.FrameCache.Wrapper).ToArray();
                    var observer = new NTSD28Q06State18SpawnEditorTests.Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    ulong legacy = world.Rng.CallCount;
                    for (int slot = 0; slot < entities.Length; slot++)
                        entities[slot].RunCpointAdvanceStep10();
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                    Compare(world, entities, row["after"], label + " after", differences);
                    bool preserved = entities.Select((e, slot) => ReferenceEquals(e.FrameCache.Wrapper, definitions[slot])).All(v => v);
                    Check(preserved == (bool)row["definitionPreserved"], label + " definition identity changed", differences);
                    Check(observer.Calls.Count == (int)row["nativeCalls"] && observer.CrtCalls == (int)row["crtCalls"] && world.Rng.CallCount == legacy,
                        label + " RNG calls changed", differences);
                    for (int slot = 0; slot < entities.Length; slot++)
                    {
                        var expected = row["after"][slot];
                        bool available = (bool)expected["available"];
                        // Null native descriptors leave cached transistor wait/next unchanged.
                        int wait = available ? (int)expected["wait"] : (bool)row["select"] ? (slot == 0 ? 23 : 0) : (int)row["before"][slot]["wait"];
                        int next = available ? (int)expected["next"] : (bool)row["select"] ? (slot == 0 ? 102 : 0) : (int)row["before"][slot]["next"];
                        Check(entities[slot].Trans.Wait == wait && entities[slot].Trans.Next == next,
                            label + " slot " + slot + " transistor=" + entities[slot].Trans.Wait + "/" + entities[slot].Trans.Next + " expected " + wait + "/" + next, differences);
                    }
                    actual.Add(new { index = (int)row["index"], definitionPreserved = preserved,
                        raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1)),
                        extra = entities.Select(Extra).ToArray() });
                    cases++;
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "immediate-" + profile + ".json", JsonConvert.SerializeObject(new
            {
                cases, beforeDifferences, differences, actual,
                missingBindings = NTSD28UnityEntityRawCapture.MissingBindings,
                scope = "Immediate CPoint slot loop only; full next tick, Play and replay remain pending. Synthetic Logan DAT, not deployed content."
            }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(392));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(20)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(20)));
        }

        private static void ShutdownWorld(SimulationWorld world, bool renderer)
        {
            if (renderer)
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
            NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ThrowSurvivesLocalSnapshotReplay(BattleRuntimeProfile profile)
        {
            var rows = File.ReadLines(Output + "source/first.jsonl").Select(JObject.Parse)
                .Where(row => (int)row["next"] == (int)row["vaction"] &&
                    (bool)row["nextDeclared"] == (bool)row["victimDeclared"]).ToArray();
            Assert.That(rows.Length, Is.EqualTo(56));
            int cases = 0;
            foreach (var row in rows)
            {
                var world = MakeWorld(row, profile, out _, true);
                try
                {
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.IsBattleSnapshotBoundaryReady, Is.True);
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True, "before-throw tick 0 capture");
                    var staleCursor = world.NativeRandom.CaptureSynchronizedCursor();
                    string initial = ReplaySignature(world, 0);
                    var expected = ExecuteThrowAndTwoTicks(world);
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    Assert.That(world.NativeRandom.CanCommitSynchronizedCursor(staleCursor), Is.False);
                    Assert.That(ReplaySignature(world, 0), Is.EqualTo(initial), "restored before throw case " + row["index"]);
                    Assert.That(ExecuteThrowAndTwoTicks(world), Is.EqualTo(expected), "replay case " + row["index"]);
                    cases++;
                }
                finally { ShutdownWorld(world, false); }
            }
            File.WriteAllText(Output + "replay-" + profile + ".json", JsonConvert.SerializeObject(new
            {
                cases, replayedTicks = cases * 2, snapshotTick = 0,
                scope = "Same-world before-throw snapshot; diagonal source rows, actual CPoint slot loop and two complete ticks. Raw, extra, B2 inputs and native RNG scalar state; generation excluded and stale cursor rejected."
            }, Formatting.Indented));
        }

        private static string[] ExecuteThrowAndTwoTicks(SimulationWorld world)
        {
            for (int slot = 0; slot < 3; slot++)
                world.FindEntityByRuntimeSlotForQuery(slot)?.RunCpointAdvanceStep10();
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
            var random = world.NativeRandom.CaptureScalarState();
            var input = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
            return JsonConvert.SerializeObject(new
            {
                tick,
                raw = JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"],
                extra = Enumerable.Range(0, 3).Select(slot => world.FindEntityByRuntimeSlotForQuery(slot) is LF2Entity e ? Extra(e) : null).ToArray(),
                input = input.Invoke(null, new object[] { world }),
                random = new
                {
                    random.CrtState, random.CrtCalls, random.TableSeed, random.SynchronizedCounter,
                    random.SynchronizedIndex, random.SynchronizedCalls, random.LastSynchronizedCallSite,
                    random.SynchronizedTableHash
                }
            });
        }

        private static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity[] entities, bool nativeInput = false, bool renderer = false)
        {
            var definitions = new[] { Definition(row, false), Definition(row, true) };
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            try
            {
                if (nativeInput) world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.SetLogicOnlyEntityMaterialization(!renderer);
                world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "catcher.dat"), new ObjectDefinition(78, 0, "victim.dat") },
                    id => definitions[id - 77]);
                world.SetRuntimeCharacterConfigResolverForSelfCheck(id => id == 78 ? definitions[1] : null);
                entities = new LF2Entity[3];
                for (int slot = 0; slot < entities.Length; slot++)
                {
                    var before = row["before"][slot];
                    var raw = before["raw"];
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                        relationTeam = 0, preserveActionZero = true,
                        opoint = new ObjectPoint { oid = (int)raw["identity"]["objectId"], action = (int)raw["frame"]["action"] }
                    };
                    var e = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                    Assert.That(e, Is.Not.Null);
                    entities[slot] = e;
                    e.AiControlled = false;
                    RestoreBefore(e, before);
                }
                entities[0].Runtime.KeyJump = (bool)row["select"] ? (byte)1 : (byte)0;
                entities[0].Runtime.CdAttack = (bool)row["select"] ? (byte)5 : (byte)0;
                entities[0].Runtime.NativeInputProxy.Current[4] = entities[0].Runtime.KeyJump;
                entities[0].Runtime.NativeInputProxy.EdgeWindow[0] = entities[0].Runtime.CdAttack;
                entities[2].KillCount = 0;
                world.NativeRandom.ResetFromSeed(42);
                return world;
            }
            catch
            {
                ShutdownWorld(world, renderer);
                throw;
            }
        }

        private static void RestoreBefore(LF2Entity e, JToken before)
        {
            var raw = before["raw"];
            var frame = raw["frame"];
            var r = e.Runtime;
            e.DirectWriteNativeRawFramePreserveWaitCounter((int)frame["action"]);
            e.Frame.Prev = (int)frame["previousAction"];
            e.Frame.Prev2 = (int)frame["tickActionSnapshot"];
            e.Frame.Prev2D = e.FrameCache.GetNativeFrameDataById(e.Frame.Prev2);
            r.PrevFrame2 = e.Frame.Prev2;
            e.AttackingCounter = (int)frame["frameCounter"];
            e.Trans.SyncDirectFrameData((int)before["wait"], (int)before["next"], (int)frame["actionLatch"]);
            e.SwitchDir((bool)frame["facingLeft"] ? "left" : "right");
            r.SetPosition((double)raw["position"]["preciseX"], (double)raw["position"]["preciseY"], (double)raw["position"]["preciseZ"]);
            r.XInt = (int)raw["position"]["x"];
            r.YInt = (int)raw["position"]["y"];
            r.ZInt = (int)raw["position"]["z"];
            r.SetVelocity((double)raw["motion"]["x"], (double)raw["motion"]["y"], (double)raw["motion"]["z"]);
            r.AnimCounter = (int)raw["identity"]["controlSlot"];
            r.OwnerSlotIndex = (int)raw["identity"]["ownerSlot"];
            r.RelationTeam = (int)raw["identity"]["battleGroup"];
            r.Unk344 = (int)raw["identity"]["participantClass"];
            r.HP = (int)raw["vitals"]["currentHp"];
            r.HPBound = (int)raw["vitals"]["effectiveMaxHp"];
            r.HP3 = (int)raw["vitals"]["baseMaxHp"];
            r.PP = (int)raw["vitals"]["currentMp"];
            r.MPMax = (int)raw["vitals"]["baseMaxMp"];
            r.HP2Orig = (int)raw["vitals"]["reviveLives"];
            r.HPOrig = (int)raw["vitals"]["reviveNextLives"];
            r.RespawnCount = (int)raw["vitals"]["reviveNextHp"];
            r.NativeRuntimeStateCode = (int)raw["combat"]["runtimeStateCode"];
            r.HitStop = (int)raw["combat"]["renderPhase"];
            r.AttackExempt = (int)raw["combat"]["attackerRest"];
            r.CollisionYReference = (int)raw["combat"]["collisionYReference"];
            r.Fall = (int)raw["combat"]["hitReactionTimer"];
            r.Bdefend = (int)raw["combat"]["bdefendAccumulator"];
            r.RuntimeArmorHp118 = (int)raw["combat"]["runtimeArmorHp"];
            r.ArmorRecoveryTimer11C = (int)raw["combat"]["armorRecoveryTimer"];
            r.FrameDelay = (int)raw["combat"]["motionHoldTimer"];
            r.WeaponFlightCounter = (int)raw["combat"]["weaponHp"];
            r.SpecialHitLatch0EB = (bool)raw["combat"]["specialHitLatch0eb"];
            r.ObjectAiExcludedGroupSourceSlot2F8 = (int)raw["combat"]["objectAiExcludedGroupSourceSlot"];
            r.NativeLifecycleCode = (int)raw["lifecycle"]["code"];
            r.NativeLifecycleResolutionPending = (bool)raw["lifecycle"]["resolutionPending"];
            e.CaughtSlotIndex = (int)before["catchTarget"];
            r.CaughtDuration = before["timeout"] != null ? (int)before["timeout"] : r.SlotIndex == 0 ? 300 : 0;
            r.CatchSourceSlot90 = (int)before["catchSource"];
            e.CatcherSlotIndex = (int)before["catchSource"];
            r.EnvironmentState320 = (int)before["environment"];
            r.EnvironmentSourceSlot160 = (int)before["environmentSource"];
            e.RefreshRuntimeSnapshot();
        }

        private static JObject Extra(LF2Entity e)
        {
            return new JObject
            {
                ["action"] = e.Frame.N, ["snapshot"] = e.Frame.Prev2, ["counter"] = e.AttackingCounter,
                ["latch"] = e.Trans.WaitCounter, ["available"] = e.Frame.D != null,
                ["wait"] = e.Frame.D?.wait ?? 0, ["next"] = e.Frame.D?.next ?? 0,
                ["catchTarget"] = e.CaughtSlotIndex, ["catchSource"] = e.Runtime.CatchSourceSlot90,
                ["timeout"] = e.Runtime.CaughtDuration,
                ["environment"] = e.Runtime.EnvironmentState320, ["environmentSource"] = e.Runtime.EnvironmentSourceSlot160,
                ["transWait"] = e.Trans.Wait, ["transNext"] = e.Trans.Next
            };
        }

        private static void Compare(SimulationWorld world, LF2Entity[] entities, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, new JArray(expected.Select(e => e["raw"].DeepClone())), label, differences);
            for (int slot = 0; slot < entities.Length; slot++)
            {
                var e = entities[slot];
                var result = Extra(e);
                foreach (var property in ((JObject)expected[slot]).Properties().Where(p => p.Name != "raw"))
                    Check(JToken.DeepEquals(property.Value, result[property.Name]), label + " slot " + slot + " " + property.Name + "=" + result[property.Name] + " expected " + property.Value, differences);
                Check(ReferenceEquals(e.Frame.D, e.FrameCache.GetNativeFrameDataById((int)expected[slot]["action"])), label + " slot " + slot + " current descriptor identity", differences);
                Check(ReferenceEquals(e.Frame.Prev2D, e.FrameCache.GetNativeFrameDataById((int)expected[slot]["snapshot"])), label + " slot " + slot + " snapshot descriptor identity", differences);
            }
        }

        private static void Check(bool condition, string message, List<string> differences)
        {
            if (!condition) differences.Add(message);
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool victim)
        {
            int destination = (int)row[victim ? "vaction" : "next"];
            bool declared = (bool)row[victim ? "victimDeclared" : "nextDeclared"];
            string dat = "<bmp_begin>\nname: ThrowRawBinding\nweapon_hp: 17\n<bmp_end>\n";
            if (victim)
                dat += "<frame> 130 relation\nstate: 10 wait: 100 next: 130 centerx: 39 centery: 79\ncpoint:\nkind: 2\ncpoint_end:\n<frame_end>\n";
            else
                dat += "<frame> 100 relation\nstate: 9 wait: 100 next: " + destination + " centerx: 39 centery: 79\ncpoint:\nkind: 1 x: 50 y: 60 vaction: " + row["vaction"] +
                    " aaction: 101 throwvx: 1.5 throwvy: -2.25 throwvz: 3 throwinjury: " + row["injury"] + "\ncpoint_end:\n<frame_end>\n" +
                    "<frame> 101 selected\nstate: 9 wait: 23 next: 102 centerx: 399 centery: 799\ncpoint:\nkind: 1 vaction: 131\ncpoint_end:\n<frame_end>\n";
            if (declared)
                dat += "<frame> " + destination + " destination\nstate: 0 wait: 37 next: 0 centerx: 3 centery: 7\n<frame_end>\n";
            string key = victim + "/" + dat;
            if (wrappers.TryGetValue(key, out var wrapper)) return wrapper;
            var parsed = new Lf2DatParserV2().ParseLoganContent(dat);
            var data = new LF2CharacterData
            {
                name = "ThrowRawBinding", type_sub = 0, weapon_hp = 17,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties),
                    LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            Lf2DatConverter.ApplyNativeArmorDefinitionData(parsed, data, true);
            wrapper = new LF2CharacterDataWrapper(victim ? 78 : 77, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06CpointThrowPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_CpointThrowPlay.request";

        static NTSD28Q06CpointThrowPlayProbe() { EditorApplication.update += Poll; }

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
            int throwCases = 0, inputCases = 0;
            try
            {
                foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
                {
                    foreach (bool renderer in new[] { false, true })
                    {
                        throwCases += NTSD28Q06CpointThrowRawBindingEditorTests.RunFollowingFullTickMatrix(profile, renderer);
                        Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                    }
                    new NTSD28Q06NativeInputMissingStateEditorTests().SampledInputMatchesSourceOptionalState(profile);
                    inputCases += 432;
                }
                Assert.That(throwCases, Is.EqualTo(1568));
                Assert.That(inputCases, Is.EqualTo(864));
                Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_CpointThrowPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, throwCases, inputCases, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene paused at stable boundary; source392 synthetic throws plus complete following tick in two profiles and both factories; source432 sampled input endpoints in two logic worlds. No physical-key or image-alignment claim. Separate Q05 shutdown required."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
