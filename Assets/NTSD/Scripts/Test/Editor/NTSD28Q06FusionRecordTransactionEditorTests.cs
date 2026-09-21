#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
    public sealed class NTSD28Q06FusionRecordTransactionEditorTests
    {
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-FUSION-RECORD-TRANSACTION-001/";
        private const string Source = "artifacts/diagnostics/NTSD28-Q06-FUSION-CATALOG-TRANSACTION-WITNESS-001/source/first.jsonl";
        private const string FormalFusion = @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime\decoded_dat\data\fusion.dat";
        private static readonly MethodInfo ProjectInput = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectExactInputEntities", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ProjectRandom = typeof(NTSD28UnityRawCaptureEditor).GetMethod("ProjectInitialNativeRandom", BindingFlags.Static | BindingFlags.NonPublic);
        // These are reported explicitly, never removed from the source artifact or promoted to raw bindings.
        private static readonly string[] UnboundExtras = { "previousX", "previousY", "previousZ", "opointLatch" };

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void RecordMergeAndControlledDefuseMatchSource(int index)
        {
            RunImmediate(index, BattleRuntimeProfile.Authority400, false);
        }

        internal static void RunImmediate(int index, BattleRuntimeProfile profile, bool renderer)
        {
            JObject row = File.ReadLines(Source).Select(JObject.Parse).Single(value => (int)value["index"] == index);
            var beforeDifferences = new List<string>();
            var mergeDifferences = new List<string>();
            var splitBeforeDifferences = new List<string>();
            var splitDifferences = new List<string>();
            var world = CreateWorld(row, profile, renderer);
            try
            {
                Compare(world, row["beforeMerge"], "beforeMerge", beforeDifferences);
                JObject before = Capture(world);
                LF2Entity retainedPartner = world.FindEntityByRuntimeSlotIncludingDormant(1);
                ulong legacyCalls = world.Rng.CallCount;
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.Oid5152FusionScanAll(0);
                Compare(world, row["afterMerge"], "afterMerge", mergeDifferences);
                CompareJson(row["mergeCalls"], observer.Capture(), "mergeCalls", mergeDifferences);
                if (world.Rng.CallCount != legacyCalls) mergeDifferences.Add("legacy RNG changed");
                JObject merged = Capture(world);
                if (!ReferenceEquals(world.FindEntityByRuntimeSlotIncludingDormant(1), retainedPartner))
                    mergeDifferences.Add("Unity dormant adapter did not retain the original partner instance");
                JObject beforeDefuse = null;
                JObject afterDefuse = null;
                string rejectionBefore = null;
                string rejectionAfter = null;
                if (row["beforeDefuse"].Type != JTokenType.Null)
                {
                    // Same explicit test intervention as C++; not natural timer-expiry evidence.
                    world.FindEntityByRuntimeSlotIncludingDormant(0).Runtime.Unk338 = 0;
                    if (index == 3) Prepare(world, row, true);
                    beforeDefuse = Capture(world);
                    Compare(world, row["beforeDefuse"], "beforeDefuse", splitBeforeDifferences);
                    rejectionBefore = CaptureIncludingDormant(world).ToString(Formatting.None);
                    observer = new Observer();
                    world.NativeRandom.SetDiagnosticCallObserver(observer);
                    world.Oid5152FusionScanAll(0);
                    Compare(world, row["afterDefuse"], "afterDefuse", splitDifferences);
                    CompareJson(row["defuseCalls"], observer.Capture(), "defuseCalls", splitDifferences);
                    if (world.Rng.CallCount != legacyCalls) splitDifferences.Add("legacy RNG changed");
                    afterDefuse = Capture(world);
                    if (!ReferenceEquals(world.FindEntityByRuntimeSlotIncludingDormant(1), retainedPartner))
                        splitDifferences.Add("Unity split did not restore the retained original partner instance");
                    rejectionAfter = CaptureIncludingDormant(world).ToString(Formatting.None);
                    if (index == 3 && rejectionBefore != rejectionAfter)
                        splitDifferences.Add("missing original partner definition must reject without modifying active OR dormant supported state");
                }
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "case-" + index + "-immediate" + (renderer || profile != BattleRuntimeProfile.Authority400 ? "-" + profile + "-renderer" + renderer : "") + ".json", JsonConvert.SerializeObject(new
                {
                    index, profile, renderer, beforeDifferences, mergeDifferences, splitBeforeDifferences, splitDifferences,
                    before, merged, beforeDefuse, afterDefuse, rejectionBefore, rejectionAfter,
                    unsupportedExtras = UnboundExtras, rawMissingBindings = NTSD28UnityEntityRawCapture.MissingBindings,
                    followingStatus = "NOT_RUN_SOURCE_FOLLOWING_RETAINED_FOR_LATER_INTEGRATION",
                    scope = "Synthetic matching object catalog plus exact formal fusion bytes; current source4 immediate public states. Native suspended slot represented by retained dormant Unity shell. No claim native slot reuse or hidden native suspended bytes."
                }, Formatting.Indented));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences));
                Assert.That(mergeDifferences, Is.Empty, string.Join("\n", mergeDifferences));
                Assert.That(splitBeforeDifferences, Is.Empty, string.Join("\n", splitBeforeDifferences));
                Assert.That(splitDifferences, Is.Empty, string.Join("\n", splitDifferences));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                Shutdown(world, renderer);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        public void DefusedPairFollowingTickMatchesSource(int index)
        {
            JObject row = File.ReadLines(Source).Select(JObject.Parse).Single(value => (int)value["index"] == index);
            var world = CreateWorld(row);
            try
            {
                world.Oid5152FusionScanAll(0);
                world.FindEntityByRuntimeSlotIncludingDormant(0).Runtime.Unk338 = 0;
                world.Oid5152FusionScanAll(0);
                var before = new List<string>();
                Compare(world, row["afterDefuse"], "afterDefuse", before);
                Assert.That(before, Is.Empty, string.Join("\n", before));
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                world.ApplyFrameInputSet(new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                var differences = new List<string>();
                Compare(world, row["following"], "following", differences);
                CompareJson(row["followingCalls"], observer.Capture(), "followingCalls", differences);
                Directory.CreateDirectory(Output);
                File.WriteAllText(Output + "case-" + index + "-following.json", JsonConvert.SerializeObject(new
                {
                    index, beforeDifferences = before, differences, actual = Capture(world),
                    unsupportedExtras = UnboundExtras, rawMissingBindings = NTSD28UnityEntityRawCapture.MissingBindings
                }, Formatting.Indented));
                Assert.That(differences, Is.Empty, string.Join("\n", differences));
            }
            finally
            {
                world.NativeRandom.SetDiagnosticCallObserver(null);
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            }
        }

        [TestCase(0, false)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(1, true)]
        public void BeforeMergeAndDormantSnapshotsReplaySameTransaction(int index, bool mergedSnapshot)
        {
            JObject row = File.ReadLines(Source).Select(JObject.Parse).Single(value => (int)value["index"] == index);
            var world = CreateWorld(row);
            try
            {
                var identity = world.RuntimeDataCatalog.LoganContentIdentity.CreateLocalValidationSessionIdentity(
                    0xF051UL, 42, 1, new[] { 0, 1 });
                LF2Entity partner = world.FindEntityByRuntimeSlotIncludingDormant(1);
                if (mergedSnapshot) world.Oid5152FusionScanAll(0);
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                void Finish()
                {
                    if (!mergedSnapshot) world.Oid5152FusionScanAll(0);
                    world.FindEntityByRuntimeSlotIncludingDormant(0).Runtime.Unk338 = 0;
                    world.Oid5152FusionScanAll(0);
                    world.ApplyFrameInputSet(input);
                    new NTSDBattleTickSystem(world).RunReleaseTick(1, false);
                }
                Finish();
                ulong checksum = world.CaptureRuntimeChecksum64(1, input);
                JObject first = Capture(world, true);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                Assert.That(world.FindEntityByRuntimeSlotIncludingDormant(1), Is.SameAs(partner));
                Assert.That(partner.Runtime.OidMergeDormant, Is.EqualTo(mergedSnapshot));
                Finish();
                Assert.That(world.CaptureRuntimeChecksum64(1, input), Is.EqualTo(checksum));
                var differences = new List<string>();
                CompareJson(first, Capture(world, true), "replay", differences);
                Assert.That(differences, Is.Empty, string.Join("\n", differences));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [Test]
        public void SuspendedPartnerKeepsReservedSlotAndRejectsNewRequiredSlotBirth()
        {
            JObject row = File.ReadLines(Source).Select(JObject.Parse).Single(value => (int)value["index"] == 0);
            var world = CreateWorld(row);
            try
            {
                var partner = world.FindEntityByRuntimeSlotIncludingDormant(1);
                world.Oid5152FusionScanAll(0);
                int borrowers = world.LogicReferencePool.ActiveCount;
                Assert.That(world.FindEntityByRuntimeSlotForQuery(1), Is.Null);
                var attempted = world.LogicEntityFactory.Create(new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = 1, dir = "right", preserveActionZero = true,
                    opoint = new ObjectPoint { oid = 7, action = 20, kind = 1 }
                }, out var failure);
                Assert.That(attempted, Is.Null);
                Assert.That(failure, Is.EqualTo(BattleLogicEntityCreationFailure.RuntimeSlotRejected));
                Assert.That(world.FindEntityByRuntimeSlotIncludingDormant(1), Is.SameAs(partner));
                Assert.That(partner.Runtime.OidMergeDormant, Is.True);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.EqualTo(borrowers));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        internal static SimulationWorld CreateWorld(JObject row, BattleRuntimeProfile profile = BattleRuntimeProfile.Authority400, bool renderer = false)
        {
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            try
            {
                world.SetLogicOnlyEntityMaterialization(!renderer);
                world.Runtime.Stage.StageWidthPx = (int)row["config"]["stageWidth"];
                world.Runtime.Stage.BaseStageWidthPx = (int)row["config"]["stageWidth"];
                world.Runtime.Stage.ZMin = (int)row["config"]["stageNear"];
                world.Runtime.Stage.ZMax = (int)row["config"]["stageFar"];
                Prepare(world, row, false);
                for (int slot = 0; slot < 2; slot++)
                {
                    var task = new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                        preserveActionZero = true, relationTeam = 3,
                        opoint = new ObjectPoint { oid = (int)row["definitions"][slot]["oid"], action = slot == 0 ? 20 : 10 }
                    };
                    LF2Entity entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                    Assert.That(entity, Is.Not.Null);
                    entity.AiControlled = false;
                    Restore(entity, row["beforeMerge"]["entities"][slot]);
                }
                world.NativeRandom.ResetFromSeed((uint)row["params"]["seed"]);
                return world;
            }
            catch { Shutdown(world, renderer); throw; }
        }

        private static void Shutdown(SimulationWorld world, bool renderer)
        {
            if (renderer)
                for (int slot = 0; slot < world.RuntimeSlotCapacity; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
            NTSD28Q06State18SpawnEditorTests.Shutdown(world);
        }

        private static void Prepare(SimulationWorld world, JObject row, bool omitPartner)
        {
            string root = Path.GetFullPath("Temp/Q06FusionRecord/" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat/data"));
            Directory.CreateDirectory(Path.Combine(root, "vfs"));
            var csv = new StringBuilder("registry_section,registry_index,id,type,source_path,published_folder\n");
            int registry = 0;
            foreach (JObject definition in row["definitions"])
            {
                int id = (int)definition["oid"];
                if (omitPartner && id == (int)row["definitions"][1]["oid"]) continue;
                csv.Append("object,").Append(registry++).Append(',').Append(id).Append(",0,data/").Append(id).Append(".dat,missing-").Append(id).Append('\n');
                File.WriteAllText(Path.Combine(root, "decoded_dat/data", id + ".dat"), (string)definition["dat"], new UTF8Encoding(false));
            }
            File.WriteAllText(Path.Combine(root, "catalog.csv"), csv.ToString(), new UTF8Encoding(false));
            byte[] fusionBytes = File.ReadAllBytes(FormalFusion);
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(fusionBytes)).Replace("-", ""),
                    Is.EqualTo("E0D7BF92F222C63C04D6728ECD423369F0604D77FF12DF958CE4AE76FEBA5FEE"), "Source4 formal fusion input changed");
            File.WriteAllBytes(Path.Combine(root, "decoded_dat/data/fusion.dat"), fusionBytes);
            var catalog = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(root));
            var wrappers = catalog.Entries.ToDictionary(entry => entry.Id, entry => new LF2CharacterDataWrapper(entry.Id,
                CharacterAnimtorManager.BuildCharacterDataFromSource(entry.DatText, entry.DatPath, catalog.Source)));
            var definitions = catalog.Entries.Select(entry => new ObjectDefinition(entry.Id, entry.Type, entry.SourcePath)).ToArray();
            if (world.RuntimeDataCatalog.IsSealedForBattle) world.UnsealRuntimeDataCatalog();
            world.PrepareRuntimeDataCatalogForBattle(definitions, id => wrappers.TryGetValue(id, out var value) ? value : null, loganCatalog: catalog);
            Assert.That(world.RuntimeDataCatalog.LoganContentIdentity, Is.SameAs(catalog.ContentIdentity));
        }

        internal static JObject Capture(SimulationWorld world, bool includeDormant = false)
        {
            var raw = (JArray)JObject.Parse(NTSD28UnityEntityRawCapture.CaptureTickJson(world, 1))["entities"];
            var inputs = ((object[])ProjectInput.Invoke(null, new object[] { world })).Select(JObject.FromObject).ToArray();
            var entities = new JArray();
            for (int slot = 0; slot < 2; slot++)
            {
                var e = world.FindEntityByRuntimeSlotIncludingDormant(slot);
                if (e == null || !includeDormant && e.Runtime.OidMergeDormant) { entities.Add(JValue.CreateNull()); continue; }
                var r = e.Runtime;
                var frame = e.Frame.D;
                var snapshot = e.GetCollisionFrameData();
                entities.Add(new JObject
                {
                    ["raw"] = raw.Single(value => (int)value["slot"] == slot).DeepClone(),
                    ["available"] = frame != null, ["state"] = frame?.state ?? 0, ["wait"] = frame?.wait ?? 0, ["next"] = frame?.next ?? 0,
                    ["snapshotAvailable"] = snapshot != null, ["snapshotState"] = snapshot?.state ?? 0,
                    ["environment"] = r.EnvironmentState320, ["environmentSource"] = r.EnvironmentSourceSlot160,
                    ["catchSource"] = r.CatchSourceSlot90, ["impactSource"] = r.ImpactSourceSlot164,
                    ["pendingCount"] = e.HitCount, ["pendingX"] = r.KnockbackVx, ["pendingY"] = r.KnockbackVy, ["pendingZ"] = r.KnockbackVz,
                    ["fusionTimer"] = r.Unk338, ["fusionDisplayTimer"] = r.FusionDisplayTimer190,
                    ["fusionPartnerSlot"] = r.Unk32C, ["fusionPrimaryId"] = r.Unk330, ["fusionPartnerId"] = r.Unk334,
                    ["gate328"] = r.Unk328, ["gate194"] = r.InputSpecialGate194,
                    ["soundLatch"] = r.NativeSoundActionLatch, ["aiProfile"] = r.NativeAiProfileObjectId,
                    ["dropMode"] = r.NativeDefinitionDropMode, ["incomingScale"] = r.IncomingDamageScale340,
                    ["modeScale"] = r.ModeDamageScalePercent, ["reviveVisual318"] = r.RenderPicOffset,
                    ["hpConsumed"] = r.InputHpConsumedTotal34C, ["mpConsumed"] = r.InputMpConsumedTotal350,
                    ["score"] = r.InputScoreTotal348,
                    ["input"] = inputs.SingleOrDefault(value => (int)value["slot"] == slot)?.DeepClone()
                });
            }
            return new JObject { ["entities"] = entities,
                ["random"] = JObject.FromObject(ProjectRandom.Invoke(null, new object[] { world.NativeRandom.CaptureScalarState() })) };
        }

        private static JObject CaptureIncludingDormant(SimulationWorld world) => Capture(world, true);

        internal static void Compare(SimulationWorld world, JToken expected, string label, List<string> differences)
        {
            JToken filtered = expected.DeepClone();
            foreach (JObject entity in filtered["entities"].OfType<JObject>())
            {
                foreach (string key in UnboundExtras) entity.Remove(key);
                foreach (string path in NTSD28UnityEntityRawCapture.MissingBindings)
                    ((JProperty)entity["raw"].SelectToken(path)?.Parent)?.Remove();
            }
            CompareJson(filtered, Capture(world), label, differences);
        }

        internal static void CompareJson(JToken expected, JToken actual, string path, List<string> differences)
        {
            if (expected is JObject obj)
            {
                if (actual is not JObject actualObject) { differences.Add(path + " expected object, actual=" + actual); return; }
                foreach (var property in obj.Properties()) CompareJson(property.Value, actualObject[property.Name], path + "." + property.Name, differences);
            }
            else if (expected is JArray array)
            {
                if (actual is not JArray values || values.Count != array.Count) { differences.Add(path + " array length differs"); return; }
                for (int i = 0; i < array.Count; i++) CompareJson(array[i], values[i], path + "[" + i + "]", differences);
            }
            else
            {
                bool numeric = expected.Type == JTokenType.Integer || expected.Type == JTokenType.Float;
                bool actualNumeric = actual != null && (actual.Type == JTokenType.Integer || actual.Type == JTokenType.Float);
                if (!(numeric && actualNumeric ? expected.Value<double>() == actual.Value<double>() : JToken.DeepEquals(expected, actual)))
                    differences.Add(path + "=" + actual + " expected=" + expected);
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
            if (before["link"] != null) r.LinkState = (int)before["link"];
            if (before["parent"] != null) r.HolderStableId = (int)before["parent"];
            if (before["child"] != null) r.TargetSlotIndex = (int)before["child"];
            r.CatchSourceSlot90 = (int)before["catchSource"]; r.ImpactSourceSlot164 = (int)before["impactSource"];
            entity.HitCount = (int)before["pendingCount"];
            r.KnockbackVx = (double)before["pendingX"]; r.KnockbackVy = (double)before["pendingY"]; r.KnockbackVz = (double)before["pendingZ"];
            NTSD28NativeComboStateMachine.ProjectExactStateToLegacy(r);
            r.Unk338 = (int)before["fusionTimer"];
            r.FusionDisplayTimer190 = (int)before["fusionDisplayTimer"];
            r.Unk32C = (int)before["fusionPartnerSlot"];
            r.Unk330 = (int)before["fusionPrimaryId"];
            r.Unk334 = (int)before["fusionPartnerId"];
            r.Unk328 = (int)before["gate328"];
            r.InputSpecialGate194 = (int)before["gate194"];
            r.NativeSoundActionLatch = (int)before["soundLatch"];
            r.NativeAiProfileObjectId = (int)before["aiProfile"];
            r.NativeDefinitionDropMode = (int)before["dropMode"];
            r.IncomingDamageScale340 = (int)before["incomingScale"];
            r.ModeDamageScalePercent = (int)before["modeScale"];
            r.RenderPicOffset = (int)before["reviveVisual318"];
            r.InputHpConsumedTotal34C = (int)before["hpConsumed"];
            r.InputMpConsumedTotal350 = (int)before["mpConsumed"];
            r.InputScoreTotal348 = (int)before["score"];
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

#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
namespace NTSD.Test
{
    [InitializeOnLoad]
    internal static class NTSD28Q06FusionPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_FusionPlay.request";
        private const string Result = "Temp/NTSD28_Q06_FusionPlay.result.json";
        static NTSD28Q06FusionPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var scene = driver?.World;
            if (scene == null || driver.CurrentTickIndex < 5 || !scene.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            ulong checksum = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input);
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var errors = new List<string>();
            int passed = 0;
            foreach (var profile in new[] { BattleRuntimeProfile.Authority400, BattleRuntimeProfile.MobileExtended })
            {
                bool renderer = profile == BattleRuntimeProfile.Authority400;
                for (int index = 0; index < (renderer ? 4 : 2); index++)
                {
                    try
                    {
                        NTSD28Q06FusionRecordTransactionEditorTests.RunImmediate(index, profile, renderer);
                        passed++;
                    }
                    catch (Exception error) { errors.Add(profile + "/" + index + ": " + error); }
                }
            }
            bool unchanged = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input) == checksum;
            int after = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status = errors.Count == 0 && passed == 6 && unchanged && borrowers == after ? "PASS" : "FAIL",
                passedCases = passed, errors, sceneChecksumUnchanged = unchanged,
                rendererBorrowersBefore = borrowers, rendererBorrowersAfter = after,
                scope = "Real Play isolated source4 transactions with pooled renderers; Mobile logic2. No physical-key/pixel parity claim; Q05 closure separate."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
            File.WriteAllText("Temp/NTSD28_Q05_ReplayPlay.request", "run");
        }
    }
}
#endif
