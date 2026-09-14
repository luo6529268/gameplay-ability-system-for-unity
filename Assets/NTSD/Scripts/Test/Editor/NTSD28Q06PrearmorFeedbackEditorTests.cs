#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06PrearmorFeedbackEditorTests
    {
        internal const string Source = "artifacts/diagnostics/NTSD28-Q06-PREARMOR-FEEDBACK-SOURCE-WITNESS-001/first.jsonl";
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-NONCHARACTER-ARMOR-FEEDBACK-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void NativePreludeFeedbackAndEarlyReturnOrderMatch(BattleRuntimeProfile profile, bool shadow)
            => RunMatrix(profile, shadow, false);

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void NativeEarlyReturnTransactionsMatchSource(BattleRuntimeProfile profile, bool shadow)
            => RunMatrix(profile, shadow, true);

        internal static bool IsEarlyReturnCase(JObject row)
            => (int)row["status"] != 0 || (string)row["message"] == "native selected-armor non-character feedback-only tail applied";

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void PreludeStateSurvivesLocalSnapshotReplay(BattleRuntimeProfile profile)
        {
            var rows = File.ReadLines(Source).Select(JObject.Parse).Where(row =>
                ((int)row["type"] == 2 || (int)row["type"] == 5) && (int)row["armor"] == 0 &&
                ((int)row["relation"] == 1 || (int)row["relation"] == 4) &&
                ((int)row["gate"] == 1 || (int)row["gate"] == 2) && (int)row["rest"] == 5 &&
                (int)row["body"] == 0 && !(bool)row["defense"]).ToArray();
            Assert.That(rows.Length, Is.EqualTo(8));
            foreach (var row in rows)
            {
                var world = MakeWorld(row, profile, out var entities);
                try
                {
                    var before = new List<string>();
                    var differences = new List<string>();
                    RunCase(world, entities, row, false, before, differences);
                    Assert.That(before, Is.Empty);
                    Assert.That(differences, Is.Empty);
                    world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                    var driver = new NTSDBattleTickSystem(world);
                    driver.RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    var staleCursor = world.NativeRandom.CaptureSynchronizedCursor();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 1, snapshot), Is.True);
                    var signatures = new List<string>();
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                        signatures.Add(ReplaySignature(world, entities, tick));
                    }
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    Assert.That(world.NativeRandom.CanCommitSynchronizedCursor(staleCursor), Is.False);
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        driver.RunReleaseTick(tick, false, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>()));
                        Assert.That(ReplaySignature(world, entities, tick), Is.EqualTo(signatures[tick - 2]));
                        Assert.That(world.BattleHitExecutionPlanDiagnosticsForDiagnostics.CurrentTickPlanValid, Is.True);
                    }
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
        }

        private static string ReplaySignature(SimulationWorld world, LF2Entity[] entities, int tick)
        {
            var rest = new int[3, 3];
            for (int target = 0; target < 3; target++)
                for (int attacker = 0; attacker < 3; attacker++) rest[target, attacker] = world.GetRawRestVrest(target, attacker);
            var random = world.NativeRandom.CaptureScalarState();
            return JsonConvert.SerializeObject(new
            {
                checksum = world.CaptureLockstepChecksumSnapshot(tick, new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>())).OverallChecksum,
                rest,
                random = new
                {
                    random.CrtState, random.CrtCalls, random.TableSeed, random.SynchronizedCounter, random.SynchronizedIndex,
                    random.SynchronizedCalls, random.LastSynchronizedCallSite, random.SynchronizedTableHash
                },
                entities = entities.Select(entity => new
                {
                    entity.Runtime.LinkState, entity.Runtime.HolderStableId, entity.Runtime.TargetSlotIndex,
                    records = Enumerable.Range(0, entity.HitRecordCount).Select(i => new[] { entity.GetHitRecordAge(i), entity.GetHitRecordX(i), entity.GetHitRecordZ(i) }).ToArray()
                }).ToArray()
            });
        }

        private static void RunMatrix(BattleRuntimeProfile profile, bool shadow, bool earlyOnly)
        {
            int cases = 0;
            var beforeDifferences = new List<string>();
            var differences = new List<string>();
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse).Where(row => !earlyOnly || IsEarlyReturnCase(row)))
            {
                var world = MakeWorld(row, profile, out var entities);
                try
                {
                    RunCase(world, entities, row, shadow, beforeDifferences, differences);
                    cases++;
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + shadow + (earlyOnly ? "-early" : "") + ".json", JsonConvert.SerializeObject(new { cases, beforeDifferences, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(earlyOnly ? 684 : 984));
            Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(12)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        internal static void RunCase(SimulationWorld world, LF2Entity[] entities, JObject row, bool shadow,
            List<string> beforeDifferences, List<string> differences)
        {
            string label = "case " + row["index"];
            CompareState(world, entities, row["before"], label + " before", beforeDifferences);
            var observer = new Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            ulong legacyBefore = world.Rng.CallCount;
            if (shadow)
            {
                world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                world.PostInteractionTickAll(1);
                var plan = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                if ((string)row["message"] == "native selected-armor non-character feedback-only tail applied" && plan.ObservedWriterEffectCount != 1)
                    differences.Add(label + " feedback writer observation count differs");
                if (world.HitExecutionPlanForInteractionModule.NativePreludeObservationCountForDiagnostics != 1)
                    differences.Add(label + " Native prelude observation count differs");
                if (!plan.CurrentTickPlanValid || plan.LastConsumeEffectsDifferenceMask != 0 || plan.LastWriterEffectDifferenceMask != 0)
                    differences.Add(label + " Shadow=" + JsonConvert.SerializeObject(plan));
            }
            else
            {
                ConsumeRecordedCandidate(entities[0]);
            }
            world.NativeRandom.SetDiagnosticCallObserver(null);
            CompareState(world, entities, row["after"], label + " after", differences);
            if (!JToken.DeepEquals(JArray.FromObject(observer.Crt), row["crt"])) differences.Add(label + " CRT differs");
            if (!JToken.DeepEquals(JArray.FromObject(observer.Native), row["native"])) differences.Add(label + " synchronized calls differ");
            if (world.Rng.CallCount != legacyBefore) differences.Add(label + " legacy RNG calls=" + (world.Rng.CallCount - legacyBefore));
        }

        internal static bool ConsumeRecordedCandidate(LF2Entity attacker)
            => BattleHitCandidateSequenceRunner.TryConsume(new Consumer(attacker));

        private static void CompareState(SimulationWorld world, LF2Entity[] entities, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, expected["raw"], label, differences);
            var links = new JArray();
            var rest = new JArray();
            var sparks = new JArray();
            for (int slot = 0; slot < 3; slot++)
            {
                var entity = entities[slot];
                links.Add(new JObject { ["state"] = entity.Runtime.LinkState, ["parent"] = entity.Runtime.HolderStableId, ["child"] = entity.Runtime.TargetSlotIndex });
                var restRow = new JArray();
                for (int attacker = 0; attacker < 3; attacker++) restRow.Add(world.GetRawRestVrest(slot, attacker));
                rest.Add(restRow);
                var events = new JArray();
                for (int i = 0; i < entity.HitRecordCount; i++)
                    events.Add(new JObject { ["host"] = slot, ["id"] = entity.GetHitRecordAge(i), ["x"] = entity.GetHitRecordX(i), ["y"] = entity.GetHitRecordZ(i) });
                sparks.Add(events);
            }
            foreach (var pair in new[] { ("links", links), ("rest", rest), ("sparks", sparks) })
                if (!JToken.DeepEquals(pair.Item2, expected[pair.Item1]))
                    differences.Add(label + " " + pair.Item1 + "=" + pair.Item2.ToString(Formatting.None) + " expected=" + expected[pair.Item1].ToString(Formatting.None));
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity[] entities, bool renderer = false)
        {
            var data = Enumerable.Range(0, 3).Select(role => Definition(row, role)).ToArray();
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(77, 0, "prearmor-a.dat"), new ObjectDefinition(78, (int)row["type"], "prearmor-t.dat"),
                new ObjectDefinition(79, 0, "prearmor-child.dat")
            }, id => data[id - 77]);
            entities = new LF2Entity[3];
            for (int slot = 0; slot < 3; slot++)
            {
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                };
                var entity = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(entity, Is.Not.Null);
                entities[slot] = entity;
                entity.Team = slot + 1;
                entity.Runtime.SetPosition(slot == 2 ? 10000 : 100 + slot * 10, 0, 200);
                entity.Runtime.SyncIntegerPosition();
            }
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(entities[0].Runtime.HitCandidateCount, Is.EqualTo(1));
            var target = entities[1];
            var child = entities[2];
            target.Runtime.Vy = 4.25;
            child.Runtime.Vy = 7.5;
            target.Runtime.Bdefend = (int)row["bdefend"];
            target.Runtime.RuntimeArmorHp118 = 3;
            foreach (var entity in entities)
            {
                entity.Runtime.LinkState = 0;
                entity.Runtime.HolderStableId = 0;
                entity.Runtime.TargetSlotIndex = 0;
            }
            int relation = (int)row["relation"];
            if (relation != 0)
            {
                target.Runtime.TargetSlotIndex = 2;
                child.Runtime.HolderStableId = relation == 3 ? -1 : 1;
                target.Runtime.LinkState = relation == 2 ? 1 : relation == 4 ? 0 : 2;
                child.Runtime.LinkState = relation == 2 ? -1 : relation == 4 ? 0 : -2;
            }
            if ((bool)row["defense"]) target.SwitchDir("left");
            if ((int)row["gate"] == 2) target.Trans.SyncDirectFrameData(target.Frame.D.wait, target.Frame.D.next, 20);
            if ((int)row["body"] != 0) target.DirectWriteNativeRawFramePreserveWaitCounter(30);
            target.ItrRest.SetVrest(0, (int)row["rest"]);
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, int role)
        {
            string key = role + "/" + string.Join("/", new[] { "type", "armor", "gate", "body", "defense" }.Select(k => row[k]));
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            int armor = (int)row["armor"], gate = (int)row["gate"];
            string text = "<bmp_begin>\nname: PrearmorFeedback\nweapon_hp: 20\n<bmp_end>\n";
            if (role == 1 && armor >= 0) text += "<armor>\ntype: " + armor +
                " ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4 spark: 199\n<armor_end>\n";
            text += "<frame> 0 active\nstate: " + (role == 1 && (bool)row["defense"] ? 7 : role == 1 && armor == 1 ? 4 : 0) +
                " wait: 100 next: 0 centerx: 3 centery: 5\n";
            if (role == 0) text += "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: 1 fall: 0 vrest: 1 bdefend: 35 effect: " +
                (gate == 1 ? 12 : 0) + (gate == 1 ? " caughtact: -3" : "") + "\nitr_end:\n";
            if (role == 1) text += "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
            text += "<frame_end>\n<frame> 20 latched\nstate: 0 wait: 50 next: 0\n";
            if (role == 1) text += "bdy:\nkind: 50 x: 0 y: 0 w: 1 h: 1\nbdy_end:\n";
            text += "<frame_end>\n<frame> 30 current_body\nstate: 0 wait: 80 next: 0\n";
            if (role == 1) text += "bdy:\nkind: " + row["body"] + " x: -20 y: -20 w: 60 h: 60 respond: -1\nbdy_end:\n";
            text += "<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData
            {
                type_sub = role == 1 ? (int)row["type"] : 0, weapon_hp = 20,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            Lf2DatConverter.ApplyNativeArmorDefinitionData(parsed, data, true);
            var wrapper = new LF2CharacterDataWrapper(77 + role, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }

        private sealed class Consumer : IBattleHitCandidateConsumer
        {
            public LF2Entity Attacker { get; }
            internal Consumer(LF2Entity attacker) { Attacker = attacker; }
            public void BeforeDispatch(int itrIndex) { }
            public void ApplyConsumeEffects(in SceneQueryHit hit) => Attacker.ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(hit);
            public bool Dispatch(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
                => Attacker.Match.DamageWriter.TryApplyCurrentDatTargetHit(Attacker.Match, Attacker, target, itr.ShallowCopy(), default);
        }

        private sealed class Observer : INTSD28NativeRandomCallObserver
        {
            internal readonly List<object> Crt = new();
            internal readonly List<object> Native = new();
            public void OnCrtNext(NTSD28NativeCrtCall call) => Crt.Add(new { result = call.Result, stateAfter = call.StateAfter, totalCalls = call.TotalCalls });
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call) => Native.Add(new
            {
                callSite = call.CallSite, upperBound = call.UpperBound, result = call.Result,
                counterAfter = call.CounterAfter, indexAfter = call.IndexAfter, totalCalls = call.TotalCalls
            });
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06PrearmorFeedbackPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_PrearmorFeedbackPlay.request";

        static NTSD28Q06PrearmorFeedbackPlayProbe() { EditorApplication.update += Poll; }

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
            var differences = new List<string>();
            var beforeDifferences = new List<string>();
            try
            {
                var selected = File.ReadLines(NTSD28Q06PrearmorFeedbackEditorTests.Source).Select(JObject.Parse)
                    .Where(NTSD28Q06PrearmorFeedbackEditorTests.IsEarlyReturnCase).ToArray();
                Assert.That(selected.Length, Is.EqualTo(684));
                foreach (bool renderer in new[] { false, true })
                foreach (bool shadow in new[] { false, true })
                foreach (var row in selected)
                {
                    var world = NTSD28Q06PrearmorFeedbackEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out var entities, renderer);
                    try
                    {
                        NTSD28Q06PrearmorFeedbackEditorTests.RunCase(world, entities, row, shadow, beforeDifferences, differences);
                        cases++;
                    }
                    finally
                    {
                        for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                            world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                        NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                        Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
                        Assert.That(LF2ObjectPool.Instance.ActiveObjectCountForAcceptance, Is.EqualTo(borrowers));
                    }
                }
                Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
                Assert.That(cases, Is.EqualTo(2736));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(10)));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_PrearmorFeedbackPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, beforeDifferences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, source684 early return/prelude/feedback contracts through both factories and direct/Shadow; full984 damage differences remain open."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
