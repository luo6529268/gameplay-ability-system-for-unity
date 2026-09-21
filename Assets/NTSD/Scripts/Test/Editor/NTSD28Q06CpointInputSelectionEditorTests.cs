#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Reflection;
using NTSD.EditorTools;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Input;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06CpointInputSelectionEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q06-CPOINT-INPUT-ACTION-SELECTION-001/";

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void SelectionPassMatchesSource(BattleRuntimeProfile profile)
            => RunSelectionMatrix(profile, false);

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void FollowingSelectionMatchesSource(BattleRuntimeProfile profile)
            => RunSelectionMatrix(profile, true);

        [TestCase(1)]
        [TestCase(4)]
        [TestCase(247)]
        [TestCase(260)]
        [TestCase(331)]
        [TestCase(369)]
        public void SelectionSnapshotReplayRepresentative(int index)
        {
            var row = JObject.Parse(File.ReadLines(Root + "source/first.jsonl")
                .Where(line => !string.IsNullOrWhiteSpace(line)).ElementAt(index));
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400);
            try
            {
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                var checksums = new ulong[2];
                var differences = new List<string>();
                for (int pass = 0; pass < 2; pass++)
                {
                    if (pass == 1)
                        Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    CompareState(world, row["before"], "replay initial " + pass, differences);
                    for (int slot = 0; slot < 2; slot++)
                        world.FindEntityByRuntimeSlotForQuery(slot)?.RunCpointAdvanceStep10();
                    CompareState(world, row["after"], "replay immediate " + pass, differences);
                    var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                    ulong immediate = world.CaptureRuntimeChecksum64(0, new FrameInputSet(0, Array.Empty<SimulationPlayerInput>()));
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                    CompareState(world, row["following"], "replay following " + pass, differences);
                    ulong following = world.CaptureRuntimeChecksum64(1, input);
                    if (pass == 0) { checksums[0] = immediate; checksums[1] = following; }
                    else
                    {
                        Assert.That(immediate, Is.EqualTo(checksums[0]));
                        Assert.That(following, Is.EqualTo(checksums[1]));
                    }
                }
                File.WriteAllText(Root + "replay-" + index + ".json", new JObject
                {
                    ["index"] = index, ["differences"] = new JArray(differences),
                    ["immediateChecksum"] = checksums[0].ToString("X16"),
                    ["followingChecksum"] = checksums[1].ToString("X16")
                }.ToString());
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(15)));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        private static void RunSelectionMatrix(BattleRuntimeProfile profile, bool following)
        {
            string path = Root + "source/first.jsonl";
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""),
                    Is.EqualTo("9ED8699D6AF26C12F1C6E6C3776955A0AABC58C90E41048C7999EFCBBE63ADDE"));
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile);
                try
                {
                    CompareState(world, row["before"], "case " + row["index"] + " before", beforeDifferences);
                    for (int slot = 0; slot < 2; slot++)
                        world.FindEntityByRuntimeSlotForQuery(slot)?.RunCpointAdvanceStep10();
                    CompareState(world, row["after"], "case " + row["index"] + " after", differences);
                    if (following)
                    {
                        new NTSDBattleTickSystem(world).RunReleaseTick(1, false,
                            new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                        CompareState(world, row["following"], "case " + row["index"] + " following", differences);
                    }
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            File.WriteAllText(Root + (following ? "following-" : "immediate-") + profile + ".json", new JObject
            {
                ["cases"] = cases, ["beforeDifferences"] = new JArray(beforeDifferences),
                ["differences"] = new JArray(differences),
                ["scope"] = "Raw47, descriptor/catch/link, full B2 and RNG; following flag selects actual complete tick. Replay and Play pending."
            }.ToString());
            Assert.That(cases, Is.EqualTo(370));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(12)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        internal static void VerifyRendererForPlay(int index)
        {
            var row = JObject.Parse(File.ReadLines(Root + "source/first.jsonl")
                .Where(line => !string.IsNullOrWhiteSpace(line)).ElementAt(index));
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, true);
            try
            {
                var differences = new List<string>();
                CompareState(world, row["before"], "renderer before", differences);
                for (int slot = 0; slot < 2; slot++)
                    world.FindEntityByRuntimeSlotForQuery(slot)?.RunCpointAdvanceStep10();
                CompareState(world, row["after"], "renderer immediate", differences);
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false,
                    new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                CompareState(world, row["following"], "renderer following", differences);
                File.WriteAllText(Root + "renderer-" + index + ".json", new JObject
                {
                    ["index"] = index, ["differences"] = new JArray(differences)
                }.ToString());
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(15)));
            }
            finally { Shutdown(world, true); }
        }

        private static void Shutdown(SimulationWorld world, bool renderer)
        {
            if (renderer)
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
            NTSD28Q06State18SpawnEditorTests.Shutdown(world);
        }

        private static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, bool renderer = false)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            try
            {
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.SetLogicOnlyEntityMaterialization(!renderer);
                var definitions = new[] { Definition(77, (string)row["catcherDat"]), Definition(78, (string)row["victimDat"]) };
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "catcher.dat"), new ObjectDefinition(78, 0, "victim.dat")
                }, id => id >= 77 && id <= 78 ? definitions[id - 77] : null);
                for (int slot = 0; slot < 2; slot++)
                {
                    var before = row["before"]["entities"][slot];
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                        relationTeam = 0, preserveActionZero = true,
                        opoint = new ObjectPoint { oid = 77 + slot, action = (int)before["raw"]["frame"]["action"] }
                    };
                    BattleLogicEntityCreationFailure failure = BattleLogicEntityCreationFailure.None;
                    var entity = renderer ? LF2ObjectPointFactory.Instance.MaterializeObjectForStructuralWriter(task)
                        : world.LogicEntityFactory.Create(task, out failure);
                    if (renderer) Assert.That(entity?.Renderer, Is.Not.Null);
                    Assert.That(entity, Is.Not.Null, failure.ToString());
                    entity.AiControlled = false;
                    RestoreBefore(entity, before);
                }
                var proxy = world.FindEntityByRuntimeSlotForQuery(0).Runtime.NativeInputProxy;
                string[] names = { "attack", "jump", "defend", "right", "left", "up", "down" };
                int[] sampleIndices = { 4, 5, 6, 3, 2, 0, 1 };
                for (int key = 0; key < names.Length; key++)
                {
                    proxy.Current[sampleIndices[key]] = (byte)row["params"]["current"][names[key]];
                    proxy.Previous[sampleIndices[key]] = (byte)row["params"]["previous"][names[key]];
                    proxy.EdgeWindow[key] = (byte)row["params"]["edge"][names[key]];
                }
                NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(world.FindEntityByRuntimeSlotForQuery(0).Runtime);
                for (int slot = 0; slot < 2; slot++)
                    BindHumanInput(world, (LF2Character)world.FindEntityByRuntimeSlotForQuery(slot), slot);
                world.Runtime.Roster.ActiveSlotCount = 2;
                world.Runtime.FunctionKeys.ResetForBattle(true);
                world.Runtime.Stage.StageWidthPx = 800;
                world.Runtime.Stage.BaseStageWidthPx = 800;
                world.Runtime.Stage.ZMin = 180;
                world.Runtime.Stage.ZMax = 350;
                world.NativeRandom.ResetFromSeed(42);
                return world;
            }
            catch
            {
                Shutdown(world, renderer);
                throw;
            }
        }

        private static void BindHumanInput(SimulationWorld world, LF2Character entity, int slotIndex)
        {
            entity.Controller = new EmptyController();
            entity.InputState.SyncFromRuntime(entity.Runtime);
            var slot = world.Runtime.Roster.Slots[slotIndex];
            slot.Active = true;
            slot.IsHuman = true;
            slot.CharacterId = entity.ObjectId;
            slot.Team = entity.Team;
            slot.InputId = slotIndex;
            slot.AiId = -1;
            slot.RuntimeSlotIndex = entity.Runtime.SlotIndex;
            slot.StableId = entity.Runtime.StableId;
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

        private static LF2CharacterDataWrapper Definition(int oid, string dat)
        {
            string root = Path.GetFullPath(Root + "fixture-runtime");
            return new LF2CharacterDataWrapper(oid, CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root)));
        }

        private static void CompareState(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            Compare(world, expected["entities"], label, differences);
            var inputMethod = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
            var randomMethod = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
            var inputs = JArray.FromObject(inputMethod.Invoke(null, new object[] { world }));
            var expectedInputs = new JArray(expected["entities"].Where(e => e.Type != JTokenType.Null).Select(e => e["input"].DeepClone()));
            CompareJson(expectedInputs, inputs, label + " input", differences);
            CompareJson(expected["random"], JObject.FromObject(randomMethod.Invoke(null,
                new object[] { world.NativeRandom.CaptureScalarState() })), label + " random", differences);
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
                for (int i = 0; i < array.Count; i++) CompareJson(array[i], actualArray[i], path + "[" + i + "]", differences);
            }
            else if (!JToken.DeepEquals(expected, actual)) differences.Add(path + "=" + actual + " expected=" + expected);
        }

        private static void Compare(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world,
                new JArray(expected.Where(e => e.Type != JTokenType.Null).Select(e => e["raw"].DeepClone())), label, differences);
            for (int slot = 0; slot < 2; slot++)
            {
                var entity = world.FindEntityByRuntimeSlotForQuery(slot);
                bool present = expected[slot].Type != JTokenType.Null;
                if ((entity != null) != present) differences.Add(label + " slot " + slot + " lifetime differs");
                if (entity == null || !present) continue;
                var frame = entity.Frame.D;
                var snapshot = entity.Frame.Prev2D;
                var actual = new JObject
                {
                    ["available"] = frame != null, ["state"] = frame?.state ?? 0,
                    ["wait"] = frame?.wait ?? 0, ["next"] = frame?.next ?? 0,
                    ["snapshotAvailable"] = snapshot != null, ["snapshotState"] = snapshot?.state ?? 0,
                    ["catchTarget"] = entity.CaughtSlotIndex, ["catchSource"] = entity.Runtime.CatchSourceSlot90,
                    ["timeout"] = entity.Runtime.CaughtDuration,
                    ["link"] = entity.Runtime.LinkState, ["parent"] = entity.Runtime.HolderStableId,
                    ["child"] = entity.Runtime.TargetSlotIndex
                };
                foreach (var property in actual.Properties())
                    if (!JToken.DeepEquals(property.Value, expected[slot][property.Name]))
                        differences.Add(label + " slot " + slot + " " + property.Name + "=" + property.Value + " expected " + expected[slot][property.Name]);
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
            r.LinkState = (int)before["link"];
            r.HolderStableId = (int)before["parent"];
            r.TargetSlotIndex = (int)before["child"];
            var input = before["input"]["input"];
            for (int i = 0; i < 5; i++) r.InputHistory[i + 1] = (int)input["keyHistory"][i];
            for (int i = 0; i < 7; i++) r.InputRemapIndices13C[i] = (byte)input["remapIndices"][i];
            r.AnimSub = (int)input["runAccumulator"];
            r.InputLastAction144 = (int)input["lastAction"];
            r.InputRemapState138 = (int)input["remapState"];
            r.BoundState198 = (int)input["boundState"];
            r.InputGlobalRecordState20 = (int)input["globalRecordState"];
            r.EnvironmentState320 = 0;
            r.EnvironmentSourceSlot160 = -1;
            e.RefreshRuntimeSnapshot();
        }

    }
}
#endif
