#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q06CpointSettlementRemainderEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q06-CPOINT-SETTLEMENT-REMAINDER-FRAME-BINDING-001/";

        [TestCase(1333, 730, 348.0, 348, 373)]
        [TestCase(2048, 1152, 373.7464366091523, 373, 398)]
        public void MovedCatcherHeldCPointPose_KeepsSourceLocalAnchorAtBothViews(
            int viewWidth,
            int viewHeight,
            double expectedCatcherX,
            int expectedCatcherXInt,
            int expectedCaughtXInt)
        {
            JObject row = JObject.Parse(File.ReadLines(Root + "source/first.jsonl").Skip(1).First());
            Assert.That((int)row["index"], Is.EqualTo(1));
            Assert.That((int)row["mode"], Is.Zero);
            Assert.That((int)row["after"][1]["raw"]["position"]["x"], Is.EqualTo(325));
            var world = new SimulationWorld(BattleRuntimeProfile.Authority400, 400);
            try
            {
                world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                world.SetLogicOnlyEntityMaterialization(true);
                var definitions = new[]
                {
                    Definition(77, (string)row["catcherDat"]),
                    Definition(78, (string)row["victimDat"])
                };
                world.PrepareRuntimeDataCatalogForBattle(new[]
                {
                    new ObjectDefinition(77, 0, "catcher.dat"),
                    new ObjectDefinition(78, 0, "victim.dat")
                }, id => id >= 77 && id <= 78 ? definitions[id - 77] : null);
                for (int slot = 0; slot < 2; slot++)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world,
                        requiredRuntimeSlot = slot,
                        nativeWeaponPieceSpawn = true,
                        dir = "right",
                        relationTeam = 0,
                        preserveActionZero = true,
                        opoint = new ObjectPoint
                        {
                            oid = 77 + slot,
                            action = (int)row["before"][slot]["raw"]["frame"]["action"]
                        }
                    };
                    LF2Entity entity = world.LogicEntityFactory.Create(task, out _);
                    Assert.That(entity, Is.Not.Null);
                    entity.AiControlled = false;
                    RestoreBefore(entity, row["before"][slot]);
                }

                world.ConfigureFixedViewRunDistance(viewWidth, viewHeight);
                LF2Entity catcher = world.FindEntityByRuntimeSlotForQuery(0);
                LF2Entity caught = world.FindEntityByRuntimeSlotForQuery(1);
                catcher.Runtime.SetSourceRulePosition(catcher.Runtime.XInt, catcher.Runtime.ZInt);
                caught.Runtime.SetSourceRulePosition(caught.Runtime.XInt, caught.Runtime.ZInt);
                catcher.Runtime.SyncSourceRuleIntegerPosition();
                caught.Runtime.SyncSourceRuleIntegerPosition();
                Assert.That(catcher.Runtime.XInt, Is.EqualTo(300));
                catcher.Runtime.SetVelocity(48, 0, 0);
                new CharacterMechanics().StepBattleLogic(
                    new CharacterMechanicsContext(catcher.Runtime, null, 0f, 0f, 0.0,
                        world.FixedViewRunDistanceScale,
                        world.FixedViewRunVerticalDistanceScale));
                Assert.That(catcher.Runtime.X, Is.EqualTo(expectedCatcherX).Within(1e-10));
                catcher.Runtime.SyncIntegerPosition();
                Assert.That(catcher.Runtime.XInt, Is.EqualTo(expectedCatcherXInt));

                catcher.RunWeaponSyncHeldStep10();
                Assert.That(caught.Runtime.XInt, Is.EqualTo(expectedCaughtXInt));
                Assert.That(caught.Runtime.XInt - catcher.Runtime.XInt, Is.EqualTo(25));
                Assert.That(caught.Runtime.YInt, Is.EqualTo(4));
                Assert.That(caught.Runtime.ZInt, Is.EqualTo(249));
                Assert.That(catcher.Runtime.SourceRuleXInt, Is.EqualTo(348));
                Assert.That(caught.Runtime.SourceRuleXInt, Is.EqualTo(373));
                Assert.That(caught.Runtime.SourceRuleX, Is.EqualTo(373));
                Assert.That(caught.Runtime.SourceRuleZInt, Is.EqualTo(249));
            }
            finally
            {
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            }
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void RemainingPassMatchesSource(BattleRuntimeProfile profile)
            => Run(profile, null);

        [TestCase(5)]
        [TestCase(9)]
        [TestCase(22)]
        public void FollowingSnapshotReplayMatchesSource(int index)
            => Run(BattleRuntimeProfile.Authority400, index);

        internal static void VerifyRendererForPlay(int index)
            => Run(BattleRuntimeProfile.Authority400, index, true);

        private static void Run(BattleRuntimeProfile profile, int? followingIndex, bool renderer = false)
        {
            string path = Root + (followingIndex.HasValue ? "source-following/first.jsonl" : "source/first.jsonl");
            if (!followingIndex.HasValue)
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", ""),
                    Is.EqualTo("B4674A11191FF10F09B31FEA4E0E26566F42986BDB3440695D8E3E41FCF8193F"));
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(path).Select(JObject.Parse))
            {
                if (followingIndex.HasValue && (int)row["index"] != followingIndex.Value) continue;
                var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
                try
                {
                    world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
                    world.SetLogicOnlyEntityMaterialization(!renderer);
                    var defs = new[] { Definition(77, (string)row["catcherDat"]), Definition(78, (string)row["victimDat"]) };
                    world.PrepareRuntimeDataCatalogForBattle(new[] {
                        new ObjectDefinition(77, 0, "catcher.dat"), new ObjectDefinition(78, 0, "victim.dat")
                    }, id => id >= 77 && id <= 78 ? defs[id - 77] : null);
                    for (int slot = 0; slot < 2; slot++)
                    {
                        var initial = row["before"][slot];
                        var task = new OPointCreateTask {
                            targetWorld = world, requiredRuntimeSlot = slot, nativeWeaponPieceSpawn = true,
                            dir = "right", relationTeam = 0, preserveActionZero = true,
                            opoint = new ObjectPoint { oid = 77 + slot, action = (int)initial["raw"]["frame"]["action"] }
                        };
                        var e = renderer ? LF2ObjectPointFactory.Instance.MaterializeObjectForStructuralWriter(task)
                            : world.LogicEntityFactory.Create(task, out _);
                        if (renderer) Assert.That(e?.Renderer, Is.Not.Null);
                        Assert.That(e, Is.Not.Null);
                        e.AiControlled = false;
                        RestoreBefore(e, initial);
                    }
                    world.NativeRandom.ResetFromSeed(42);
                    world.Runtime.FunctionKeys.ResetForBattle(true);
                    world.Runtime.Stage.StageWidthPx = world.Runtime.Stage.BaseStageWidthPx = 800;
                    world.Runtime.Stage.ZMin = 180; world.Runtime.Stage.ZMax = 350;
                    string label = "case " + row["index"];
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    if (followingIndex.HasValue && !renderer) Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                    ulong firstChecksum = 0;
                    for (int repeat = 0; repeat < (followingIndex.HasValue && !renderer ? 2 : 1); repeat++)
                    {
                        if (repeat == 1) Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                        Compare(world, row["before"], label + " before", beforeDifferences);
                        for (int slot = 0; slot < 2; slot++)
                        {
                            var e = world.FindEntityByRuntimeSlotForQuery(slot);
                            if ((int)row["mode"] == 0) e.RunWeaponSyncHeldStep10();
                            else e.RunCpointAdvanceStep10();
                        }
                        Compare(world, row["after"], label + " after", differences);
                        if (followingIndex.HasValue)
                        {
                            var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                            new NTSDBattleTickSystem(world).RunReleaseTick(1, false, input);
                            Compare(world, row["following"], label + " following", differences);
                            ulong checksum = world.CaptureRuntimeChecksum64(1, input);
                            if (repeat == 0) firstChecksum = checksum;
                            else Assert.That(checksum, Is.EqualTo(firstChecksum));
                        }
                    }
                    cases++;
                }
                finally
                {
                    if (renderer)
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                            world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            File.WriteAllText(Root + (followingIndex.HasValue ? (renderer ? "renderer-" : "following-") + followingIndex.Value : "unity-" + profile) + ".json", new JObject {
                ["cases"] = cases, ["beforeDifferences"] = JArray.FromObject(beforeDifferences),
                ["differences"] = JArray.FromObject(differences)
            }.ToString());
            Assert.That(cases, Is.EqualTo(followingIndex.HasValue ? 1 : 24));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(10)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        private static LF2CharacterDataWrapper Definition(int oid, string dat)
        {
            string root = Path.GetFullPath(Root + "fixture-runtime");
            return new LF2CharacterDataWrapper(oid, CharacterAnimtorManager.BuildCharacterDataFromSource(dat,
                Path.Combine(root, "decoded_dat", oid + ".dat"), BattleContentSource.ForLoganRuntime(root)));
        }

        private static void Compare(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world,
                new JArray(expected.Select(e => e["raw"].DeepClone())), label, differences);
            for (int slot = 0; slot < 2; slot++)
            {
                var e = world.FindEntityByRuntimeSlotForQuery(slot);
                var f = e.Frame.D;
                var row = expected[slot];
                if ((f != null) != (bool)row["available"]) differences.Add(label + " slot " + slot + " available");
                if (f != null && (f.wait != (int)row["wait"] || f.next != (int)row["next"]))
                    differences.Add(label + " slot " + slot + " descriptor wait/next=" + f.wait + "/" + f.next);
                if (e.CaughtSlotIndex != (int)row["target"] || e.Runtime.CatchSourceSlot90 != (int)row["source"] ||
                    e.Runtime.CaughtDuration != (int)row["timeout"]) differences.Add(label + " slot " + slot + " relation");
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
            e.CaughtSlotIndex = (int)before["target"];
            r.CatchSourceSlot90 = (int)before["source"];
            e.CatcherSlotIndex = (int)before["source"];
            r.CaughtDuration = (int)before["timeout"];
            r.PlatformSourceSlotF4 = (int)raw["combat"]["platformSourceSlot"];
            r.EnvironmentState320 = (int)raw["combat"]["environmentState"];
            r.EnvironmentSourceSlot160 = (int)raw["combat"]["environmentSourceSlot"];
            e.RefreshRuntimeSnapshot();
        }
    }
}
#endif
