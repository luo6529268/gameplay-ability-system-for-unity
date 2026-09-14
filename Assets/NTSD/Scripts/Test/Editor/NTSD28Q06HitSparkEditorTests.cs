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
using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06HitSparkEditorTests
    {
        internal const string Source = "artifacts/diagnostics/NTSD28-Q06-HIT-SPARK-SOURCE-WITNESS-001/first.jsonl";
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-HIT-SPARK-UNITY-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();
        private static readonly MethodInfo consume = typeof(BattleHitCandidateSequenceRunner)
            .GetMethod("TryConsumeCandidate", BindingFlags.Static | BindingFlags.NonPublic);

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void SparkRecordsAndRandomReplayAfterLocalSnapshot(BattleRuntimeProfile profile)
        {
            var rows = File.ReadLines(Source).Select(JObject.Parse)
                .Where(r => (int)r["route"] != 1 && (int)r["spark"] == 0 && (int)r["missing"] == 0)
                .GroupBy(r => (int)r["route"] + "/" + (int)r["itrIndex"]).Select(g => g.First()).ToArray();
            Assert.That(rows.Length, Is.EqualTo(4));
            foreach (var row in rows)
            {
                var world = MakeWorld(row, profile, false, out var attacker, out var target);
                try
                {
                    var differences = new List<string>();
                    RunEndpoint(world, attacker, target, row, differences);
                    Assert.That(differences, Is.Empty);
                    var driver = new NTSDBattleTickSystem(world);
                    driver.RunReleaseTick(1, false, new FrameInputSet(1, Array.Empty<SimulationPlayerInput>()));
                    var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                    var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                    Assert.That(world.TryCaptureBattleStateSnapshot(identity, 1, snapshot), Is.True);
                    var checksums = new List<string>();
                    var records = new List<JArray>();
                    var crtStates = new List<NTSD28NativeRandomScalarState>();
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        var input = new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>());
                        driver.RunReleaseTick(tick, false, input);
                        checksums.Add(world.CaptureLockstepChecksumSnapshot(tick, input).OverallChecksum);
                        records.Add(CaptureRecords(attacker, target));
                        crtStates.Add(world.NativeRandom.CaptureScalarState());
                    }
                    Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    for (int tick = 2; tick <= 3; tick++)
                    {
                        var input = new FrameInputSet(tick, Array.Empty<SimulationPlayerInput>());
                        driver.RunReleaseTick(tick, false, input);
                        Assert.That(world.CaptureLockstepChecksumSnapshot(tick, input).OverallChecksum, Is.EqualTo(checksums[tick - 2]));
                        Assert.That(JToken.DeepEquals(CaptureRecords(attacker, target), records[tick - 2]), Is.True);
                        var crt = world.NativeRandom.CaptureScalarState();
                        Assert.That(crt.CrtState, Is.EqualTo(crtStates[tick - 2].CrtState));
                        Assert.That(crt.CrtCalls, Is.EqualTo(crtStates[tick - 2].CrtCalls));
                        Assert.That(attacker.ResolveNativeHitCandidateIndex(new InteractionArea()), Is.Zero);
                    }
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void ActualCharacterRoutesUseOriginalIndexWithCopiedItr(BattleRuntimeProfile profile)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse).Where(r => (int)r["route"] != 1 && (int)r["missing"] == 0))
            {
                var world = MakeWorld(row, profile, false, out var attacker, out var target);
                try
                {
                    RunActual(world, attacker, target, row, differences);
                    cases++;
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + "routes-" + profile + ".json", JsonConvert.SerializeObject(new { cases, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(282));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        internal static void RunActual(SimulationWorld world, LF2Entity attacker, LF2Entity target, JObject row, List<string> differences)
        {
            var observer = new Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            var consumer = new Consumer(attacker);
            DispatchCandidate(world, attacker, consumer, (int)row["itrIndex"]);
            Assert.That(consumer.Dispatches, Is.EqualTo(1));
            Assert.That(attacker.ResolveNativeHitCandidateIndex(new InteractionArea()), Is.Zero);
            CompareRecords(attacker, target, row["after"], "actual " + row["index"], differences);
            if (!JToken.DeepEquals(JArray.FromObject(observer.Crt), row["crt"])) differences.Add("actual " + row["index"] + " CRT differs");
            world.NativeRandom.SetDiagnosticCallObserver(null);
            if (observer.Native != (int)row["nativeCalls"]) differences.Add("actual " + row["index"] + " synchronized calls differ");
        }

        [Test]
        public void CandidateScopeRestoresAfterNestedAndThrowingDispatch()
        {
            var row = File.ReadLines(Source).Select(JObject.Parse).First(r => (int)r["route"] == 0 && (int)r["itrIndex"] == 2 && (int)r["spark"] == 0);
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, false, out var attacker, out _);
            try
            {
                var copy = new InteractionArea();
                attacker.CurrentItrIndex = 17;
                using (attacker.BeginNativeHitCandidate(1))
                {
                    foreach (bool before in new[] { false, true })
                    {
                        var consumer = new Consumer(attacker) { ThrowBefore = before, ThrowDuring = !before };
                        Assert.Throws<TargetInvocationException>(() => DispatchCandidate(world, attacker, consumer, 2));
                        Assert.That(attacker.ResolveNativeHitCandidateIndex(copy), Is.EqualTo(1));
                    }
                }
                Assert.That(attacker.ResolveNativeHitCandidateIndex(copy), Is.Zero);
                Assert.That(attacker.CurrentItrIndex, Is.EqualTo(17));
            }
            finally
            {
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            }
        }

        [Test]
        public void NonKindZeroDoesNotWriteOrConsumeRandom()
        {
            var row = File.ReadLines(Source).Select(JObject.Parse).First(r => (int)r["route"] == 0 && (int)r["spark"] == 0);
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, false, out var attacker, out var target);
            try
            {
                var observer = new Observer();
                world.NativeRandom.SetDiagnosticCallObserver(observer);
                foreach (int kind in new[] { -1, 1, 3, 6, 8, 9, 14 })
                    BattleNativeHitSparkWriter.Append(world, attacker, target, new InteractionArea { kind = kind }, 2, null, false, true);
                Assert.That(attacker.HitRecordCount + target.HitRecordCount, Is.Zero);
                Assert.That(observer.Crt, Is.Empty);
                Assert.That(observer.Native, Is.Zero);
            }
            finally
            {
                NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            }
        }

        private static void DispatchCandidate(SimulationWorld world, LF2Entity attacker, Consumer consumer, int index)
        {
            Assert.That(world.SceneQuery.TryGetCollisionCandidateRange(attacker, out var range), Is.True);
            Assert.That(range.TryGet(index, out var candidate), Is.True);
            Assert.That(candidate.ItrIndex, Is.EqualTo(index));
            consume.Invoke(null, new object[] { consumer, attacker.GetCollisionFrameData(), world.ItrKindService, candidate });
        }

        private sealed class Consumer : IBattleHitCandidateConsumer
        {
            public LF2Entity Attacker { get; }
            internal int Dispatches;
            internal bool ThrowBefore;
            internal bool ThrowDuring;
            internal Consumer(LF2Entity attacker) { Attacker = attacker; }
            public void ApplyConsumeEffects(in SceneQueryHit hit) { }
            public void BeforeDispatch(int itrIndex)
            {
                Assert.That(Attacker.ResolveNativeHitCandidateIndex(new InteractionArea()), Is.EqualTo(itrIndex));
                if (ThrowBefore) throw new InvalidOperationException("scope-before");
            }
            public bool Dispatch(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
            {
                Dispatches++;
                if (ThrowDuring) throw new InvalidOperationException("scope-dispatch");
                var copy = itr.ShallowCopy();
                return Attacker.Match.DamageWriter.TryApplyCurrentDatTargetHit(Attacker.Match, Attacker, target, copy, default);
            }
        }

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        public void OriginalSparkArraysAndCrtTraceMatch(BattleRuntimeProfile profile, bool renderer)
        {
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile, renderer, out var attacker, out var target);
                try
                {
                    RunEndpoint(world, attacker, target, row, differences);
                    cases++;
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + renderer + ".json", JsonConvert.SerializeObject(
                new { cases, differences, scope = "Spark endpoint only; feedback damage route is a separate task." }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(438));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        internal static void RunEndpoint(SimulationWorld world, LF2Entity attacker, LF2Entity target, JObject row, List<string> differences)
        {
            string label = "case " + row["index"];
            CompareRecords(attacker, target, row["before"], label + " before", differences);
            var observer = new Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            var itr = attacker.FrameCache.GetNativeFrameDataById(0).itrs[(int)row["itrIndex"]];
            int route = (int)row["route"];
            var armor = target.FrameCache.Wrapper.characterData.armors.FirstOrDefault();
            BattleNativeHitSparkWriter.Append(world, attacker, target, itr, (int)row["itrIndex"], armor, route == 2, route == 0);
            world.NativeRandom.SetDiagnosticCallObserver(null);
            CompareRecords(attacker, target, row["after"], label + " after", differences);
            if (!JToken.DeepEquals(JArray.FromObject(observer.Crt), row["crt"])) differences.Add(label + " CRT values/state/count differ");
            if (observer.Native != (int)row["nativeCalls"]) differences.Add(label + " synchronized calls differ");
        }

        private static void CompareRecords(LF2Entity attacker, LF2Entity target, JToken expected, string label, List<string> differences)
        {
            var actual = CaptureRecords(attacker, target);
            if (!JToken.DeepEquals(actual, expected)) differences.Add(label + " records=" + actual.ToString(Formatting.None) + " expected=" + expected.ToString(Formatting.None));
        }

        private static JArray CaptureRecords(LF2Entity attacker, LF2Entity target)
        {
            var actual = new JArray();
            foreach (var entity in new[] { attacker, target }.OrderBy(e => e.Runtime.SlotIndex))
            {
                var events = new JArray();
                for (int i = 0; i < entity.HitRecordCount; i++)
                    events.Add(new JObject { ["host"] = entity.Runtime.SlotIndex, ["id"] = entity.GetHitRecordAge(i), ["x"] = entity.GetHitRecordX(i), ["y"] = entity.GetHitRecordZ(i) });
                actual.Add(new JObject { ["slot"] = entity.Runtime.SlotIndex, ["events"] = events });
            }
            return actual;
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, bool renderer, out LF2Entity attacker, out LF2Entity target)
        {
            var definitions = new[] { Definition(row, true), Definition(row, false) };
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(77, 0, "spark-a.dat"), new ObjectDefinition(78, (int)row["route"] == 1 ? 5 : 0, "spark-t.dat")
            }, id => definitions[id - 77]);
            var pair = new LF2Entity[2];
            int a = (int)row["reverse"] == 0 ? 0 : 1;
            for (int role = 0; role < 2; role++)
            {
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = role == 0 ? a : 1 - a,
                    dir = "right", nativeWeaponPieceSpawn = true, relationTeam = role + 1,
                    preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + role, action = 0 }
                };
                pair[role] = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(pair[role], Is.Not.Null);
                pair[role].Runtime.SetPosition(100 + role * 10, 0, 200);
                pair[role].Runtime.SyncIntegerPosition();
            }
            attacker = pair[0]; target = pair[1];
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(3));
            attacker.DirectWriteNativeRawFramePreserveWaitCounter(10);
            target.DirectWriteNativeRawFramePreserveWaitCounter(10);
            attacker.SwitchDir((int)row["facing"] == 0 ? "right" : "left");
            int geometry = (int)row["geometry"];
            attacker.Runtime.SetPosition(-11, -5, geometry == 1 ? 21 : geometry == 2 ? 19 : 20);
            target.Runtime.SetPosition(geometry == 3 ? -20 : -4, -6, 20);
            foreach (var entity in pair)
            {
                entity.Runtime.SyncIntegerPosition();
                int count = entity.Runtime.SlotIndex == (int)row["expectedHost"] ? (int)row["fill"] : (int)row["otherFill"];
                for (int i = 0; i < count; i++) entity.AddHitRecord(70 + i, -100 - i, -200 - i);
            }
            if ((int)row["missing"] == 1) attacker.Frame.Prev2 = 1000;
            if ((int)row["missing"] == 2) target.Frame.Prev2 = 1000;
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool attacker)
        {
            string key = attacker + "/" + string.Join("/", new[] { "route", "effect", "spark", "fall", "armorSpark", "cover" }.Select(k => row[k]));
            if (wrappers.TryGetValue(key, out var existing)) return existing;
            int route = (int)row["route"];
            string text = "<bmp_begin>\nname: SparkTransaction\n<bmp_end>\n";
            if (!attacker && route != 0) text += "<armor>\ntype: " + (route == 1 ? 0 : 1) +
                " ratio: 15 decrease: 50 mp: 0 fall: -1 bdefend: -1 injury: -1 delay: -1 state: 4 spark: " + row["armorSpark"] + "\n<armor_end>\n";
            foreach (int action in new[] { 0, 10 })
            {
                text += "<frame> " + action + " spark\nstate: " + (!attacker && route == 2 ? 4 : 0) +
                    " wait: 100 next: 0 centerx: " + (action == 0 ? 3 : 31) + " centery: " + (action == 0 ? 5 : 41) + "\n";
                if (attacker)
                    for (int i = 0; i < 3; i++) text += "itr:\nkind: 0 x: -8 y: -3 w: 17 h: 5 vrest: 1 injury: 1 fall: " + row["fall"] +
                        " effect: " + row["effect"] + " spark: " + row["spark"] + " cover: " + row["cover"] + "\nitr_end:\n";
                else text += "bdy:\nkind: 0 x: -999 y: -999 w: 2000 h: 2000\nbdy_end:\n";
                text += "<frame_end>\n";
            }
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData { type_sub = !attacker && route == 1 ? 5 : 0 };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            Lf2DatConverter.ApplyNativeArmorDefinitionData(parsed, data, true);
            var wrapper = new LF2CharacterDataWrapper(attacker ? 77 : 78, data);
            wrappers.Add(key, wrapper);
            return wrapper;
        }

        internal sealed class Observer : INTSD28NativeRandomCallObserver
        {
            internal readonly List<object> Crt = new();
            internal int Native;
            public void OnCrtNext(NTSD28NativeCrtCall call) => Crt.Add(new { result = call.Result, stateAfter = call.StateAfter, totalCalls = call.TotalCalls });
            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call) => Native++;
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06HitSparkPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_HitSparkPlay.request";

        static NTSD28Q06HitSparkPlayProbe() { EditorApplication.update += Poll; }

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
            try
            {
                var selected = File.ReadLines(NTSD28Q06HitSparkEditorTests.Source).Select(JObject.Parse).ToArray();
                Assert.That(selected.Length, Is.EqualTo(438));
                foreach (bool renderer in new[] { false, true })
                foreach (var row in selected)
                foreach (bool actual in (int)row["route"] != 1 && (int)row["missing"] == 0
                    ? new[] { false, true } : new[] { false })
                {
                    var world = NTSD28Q06HitSparkEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400,
                        renderer, out var attacker, out var target);
                    try
                    {
                        if (actual)
                            NTSD28Q06HitSparkEditorTests.RunActual(world, attacker, target, row, differences);
                        else
                            NTSD28Q06HitSparkEditorTests.RunEndpoint(world, attacker, target, row, differences);
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
                Assert.That(cases, Is.EqualTo(1440));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_HitSparkPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, both factories: source438 endpoints and source282 actual character routes per factory; feedback damage route remains separate."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
