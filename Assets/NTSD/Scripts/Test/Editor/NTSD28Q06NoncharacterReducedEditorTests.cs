#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NoncharacterReducedEditorTests
    {
        internal const string SourceRoot = "artifacts/diagnostics/NTSD28-Q06-NONCHARACTER-REDUCED-SOURCE-WITNESS-001/owner987/";
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-NONCHARACTER-REDUCED-HIT-TRANSACTION-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void FullReducedTransactionAndGuardsMatch(BattleRuntimeProfile profile, bool shadow)
        {
            var before = new List<string>();
            var differences = new List<string>();
            int cases = 0;
            foreach (var row in File.ReadLines(SourceRoot + "first.jsonl").Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile, out var entities);
                try
                {
                    RunCase(world, entities, row, shadow, before, differences);
                    cases++;
                }
                finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + shadow + ".json",
                JsonConvert.SerializeObject(new { cases, before, differences, sourceOnlyExtra = "yResolved" }, Formatting.Indented));
            var validation = JObject.Parse(File.ReadAllText(SourceRoot + "validation.json"));
            Assert.That(cases, Is.EqualTo((int)validation["cases"]));
            Assert.That(cases, Is.EqualTo(987));
            Assert.That(before, Is.Empty, string.Join("\n", before.Take(10)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(10)));
        }

        [Test]
        public void NativeCandidateRouteRestoresAfterNestedException()
        {
            var row = JObject.Parse(File.ReadLines(SourceRoot + "first.jsonl").First());
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var entities);
            try
            {
                var attacker = entities[0];
                Assert.That(attacker.NativeHitRoute.HasValue, Is.False);
                var outer = new BattleOrdinaryCharacterDamageRoute(
                    BattleOrdinaryCharacterDamageRouteKind.UnarmoredType1BrokenFallback, null, 7, default, default);
                using (attacker.BeginNativeHitCandidate(5, outer))
                {
                    Assert.That(attacker.NativeHitRoute.Value.Kind, Is.EqualTo(outer.Kind));
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        using var inner = attacker.BeginNativeHitCandidate(7);
                        Assert.That(attacker.NativeHitRoute.HasValue, Is.False);
                        Assert.That(attacker.ResolveNativeHitCandidateIndex(null), Is.EqualTo(7));
                        throw new InvalidOperationException("scope unwind");
                    });
                    Assert.That(attacker.NativeHitRoute.Value.Kind, Is.EqualTo(outer.Kind));
                    Assert.That(attacker.ResolveNativeHitCandidateIndex(null), Is.EqualTo(5));
                }
                Assert.That(attacker.NativeHitRoute.HasValue, Is.False);
                Assert.That(attacker.ResolveNativeHitCandidateIndex(null), Is.EqualTo(0));
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity[] entities, bool renderer = false)
        {
            var raw = row["before"]["raw"];
            var definitions = new LF2CharacterDataWrapper[3];
            for (int slot = 0; slot < 3; slot++)
            {
                string dat = (string)row["params"]["dat"][slot];
                int type = (int)raw[slot]["identity"]["objectType"];
                string key = slot + "/" + type + "/" + dat;
                if (!wrappers.TryGetValue(key, out var wrapper))
                {
                    var parsed = new Lf2DatParserV2().ParseLoganContent(dat);
                    var data = new LF2CharacterData
                    {
                        type_sub = type,
                        weapon_hp = 23,
                        weapon_broken_sound = "",
                        weapon_hit_sound = "",
                        NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties),
                            LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
                    };
                    foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
                    Lf2DatConverter.ApplyNativeArmorDefinitionData(parsed, data, true);
                    wrapper = new LF2CharacterDataWrapper((int)raw[slot]["identity"]["objectId"], data);
                    wrappers.Add(key, wrapper);
                }
                definitions[slot] = wrapper;
            }
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(Enumerable.Range(0, 3).Select(slot =>
                new ObjectDefinition(77 + slot, (int)raw[slot]["identity"]["objectType"], "matched-" + slot + ".dat")).ToArray(),
                id => definitions[id - 77]);
            entities = new LF2Entity[3];
            for (int slot = 0; slot < 3; slot++)
            {
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                };
                var e = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(e, Is.Not.Null);
                entities[slot] = e;
                e.Team = slot + 1;
                e.Runtime.SetPosition(slot == 2 ? 10000 : 100 + slot * 10, 0, 200);
                e.Runtime.SyncIntegerPosition();
            }
            entities[0].DirectWriteNativeRawFramePreserveWaitCounter((int)row["params"]["candidateAttackerAction"]);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(entities[0].Runtime.HitCandidateCount, Is.EqualTo(1));
            for (int slot = 0; slot < 3; slot++)
            {
                var e = entities[slot];
                var state = raw[slot];
                var extra = row["before"]["extra"][slot];
                e.DirectWriteNativeRawFramePreserveWaitCounter((int)state["frame"]["action"]);
                e.Frame.Prev = (int)state["frame"]["previousAction"];
                e.Frame.Prev2 = (int)state["frame"]["tickActionSnapshot"];
                e.Frame.Prev2D = e.FrameCache.GetNativeFrameDataById(e.Frame.Prev2);
                e.Runtime.PrevFrame2 = e.Frame.Prev2;
                e.AttackingCounter = (int)state["frame"]["frameCounter"];
                e.Trans.SyncDirectFrameData(e.Frame.D.wait, e.Frame.D.next, (int)state["frame"]["actionLatch"]);
                e.SwitchDir((bool)state["frame"]["facingLeft"] ? "left" : "right");
                e.Runtime.SetPosition((double)state["position"]["preciseX"], (double)state["position"]["preciseY"], (double)state["position"]["preciseZ"]);
                e.Runtime.XInt = (int)state["position"]["x"];
                e.Runtime.YInt = (int)state["position"]["y"];
                e.Runtime.ZInt = (int)state["position"]["z"];
                e.Runtime.SetVelocity((double)state["motion"]["x"], (double)state["motion"]["y"], (double)state["motion"]["z"]);
                e.Health.HP = (int)state["vitals"]["currentHp"];
                e.Health.HPBound = (int)state["vitals"]["effectiveMaxHp"];
                e.Health.HP3 = (int)state["vitals"]["baseMaxHp"];
                e.Health.PP = (int)state["vitals"]["currentMp"];
                e.Health.MaxMP = (int)state["vitals"]["baseMaxMp"];
                e.FrameDelay = (int)state["combat"]["motionHoldTimer"];
                e.FallCounter = (int)state["combat"]["hitReactionTimer"];
                e.Runtime.Bdefend = (int)state["combat"]["bdefendAccumulator"];
                e.Runtime.RuntimeArmorHp118 = (int)state["combat"]["runtimeArmorHp"];
                e.Runtime.OwnerSlotIndex = (int)state["identity"]["ownerSlot"];
                e.Runtime.SpecialHitLatch0EB = (bool)state["combat"]["specialHitLatch0eb"];
                e.Runtime.CollisionYReference = (int)state["combat"]["collisionYReference"];
                e.HitCount = (int)extra["count"];
                e.KnockbackVx = (double)extra["x"];
                e.KnockbackVy = (double)extra["y"];
                e.KnockbackVz = (double)extra["z"];
                e.Runtime.LinkState = (int)extra["link"];
                e.Runtime.HolderStableId = (int)extra["parent"];
                e.Runtime.TargetSlotIndex = (int)extra["child"];
                e.Runtime.InputHpConsumedTotal34C = (int)extra["hpConsumed"];
                e.Runtime.InputMpConsumedTotal350 = (int)extra["mpConsumed"];
                e.Runtime.InputScoreTotal348 = (int)extra["score"];
                e.Runtime.Kind4SourceCount92 = (int)extra["kind4SourceCount"];
                e.Runtime.WeakTimer12C = (int)extra["weak"];
                e.Runtime.IncomingDamageScale340 = (int)extra["incomingScale"];
                e.Runtime.StatusDx1C0 = (int)extra["statusDx"];
                e.Runtime.StatusDy1C4 = (int)extra["statusDy"];
                e.Runtime.StatusDz1C8 = (int)extra["statusDz"];
                e.Runtime.StatusGain1CC = (int)extra["statusGain"];
                e.Runtime.StatusHitFacing1D0 = (int)extra["statusFacing"];
                e.Runtime.StatusPickedAction1D4 = (int)extra["statusPicked"];
                e.Runtime.StatusPickingAction1D8 = (int)extra["statusPicking"];
                for (int target = 0; target < 3; target++) e.ItrRest.SetVrest(target, (int)row["before"]["rest"][slot][target]);
            }
            world.NativeRandom.ResetFromSeed((uint)row["params"]["seed"]);
            return world;
        }

        internal static void RunCase(SimulationWorld world, LF2Entity[] entities, JObject row, bool shadow, List<string> before, List<string> differences)
        {
            string label = "case " + row["index"];
            CompareState(world, entities, row["before"], label + " before", before);
            var observer = new Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            ulong legacy = world.Rng.CallCount;
            if (shadow)
            {
                world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                world.CaptureBattleHitExecutionPlanPass(1, BattleHitExecutionPass.Character);
                Assert.That(world.BeginBattleHitExecutionPlanLegacyObservation(1, BattleHitExecutionPass.Character), Is.True);
            }
            try { NTSD28Q06PrearmorFeedbackEditorTests.ConsumeRecordedCandidate(entities[0]); }
            finally { if (shadow) world.EndBattleHitExecutionPlanLegacyObservation(); }
            world.NativeRandom.SetDiagnosticCallObserver(null);
            if (shadow)
            {
                var plan = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                bool writerExpected = (int)row["status"] == 0;
                if (writerExpected && plan.ObservedWriterEffectCount != 1) differences.Add(label + " writer observations=" + plan.ObservedWriterEffectCount);
                if (!plan.CurrentTickPlanValid || plan.LastWriterEffectDifferenceMask != 0 || plan.LastConsumeEffectsDifferenceMask != 0)
                    differences.Add(label + " Shadow=" + JsonConvert.SerializeObject(plan));
            }
            CompareState(world, entities, row["after"], label + " after", differences);
            if (!JToken.DeepEquals(JArray.FromObject(observer.Crt), row["crt"])) differences.Add(label + " CRT calls differ");
            if (!JToken.DeepEquals(JArray.FromObject(observer.Native), row["native"])) differences.Add(label + " native calls differ");
            if (world.Rng.CallCount != legacy) differences.Add(label + " legacy calls=" + (world.Rng.CallCount - legacy));
            if (world.PendingSounds.Count != row["audio"].Count()) differences.Add(label + " audio count differs");
            for (int i = 0; i < Math.Min(world.PendingSounds.Count, row["audio"].Count()); i++)
                if (world.PendingSounds[i].Cue != (string)row["audio"][i]["path"] || world.PendingSounds[i].WorldX != (int)row["audio"][i]["x"])
                    differences.Add(label + " audio event differs");
            foreach (var entity in entities) entity.FrameDelay = 0;
            world.FramePostProcessAll();
            CompareState(world, entities, row["afterReleasedHoldFinalize"], label + " finalized", differences);
        }

        private static void CompareState(SimulationWorld world, LF2Entity[] entities, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, expected["raw"], label, differences);
            var extra = new JArray();
            var rests = new JArray();
            var sparks = new JArray();
            foreach (var e in entities)
            {
                var r = e.Runtime;
                extra.Add(JObject.FromObject(new
                {
                    count = e.HitCount, x = e.KnockbackVx, y = e.KnockbackVy, z = e.KnockbackVz,
                    hpConsumed = r.InputHpConsumedTotal34C, mpConsumed = r.InputMpConsumedTotal350, score = r.InputScoreTotal348,
                    link = r.LinkState, parent = r.HolderStableId, child = r.TargetSlotIndex, kind4SourceCount = r.Kind4SourceCount92,
                    weak = r.WeakTimer12C, incomingScale = r.IncomingDamageScale340,
                    statusDx = r.StatusDx1C0, statusDy = r.StatusDy1C4, statusDz = r.StatusDz1C8, statusGain = r.StatusGain1CC,
                    statusFacing = r.StatusHitFacing1D0, statusPicked = r.StatusPickedAction1D4, statusPicking = r.StatusPickingAction1D8
                }));
                rests.Add(new JArray(Enumerable.Range(0, 3).Select(slot => world.GetRawRestVrest(r.SlotIndex, slot))));
                sparks.Add(new JArray(Enumerable.Range(0, e.HitRecordCount).Select(i => new JObject
                {
                    ["host"] = r.SlotIndex, ["id"] = e.GetHitRecordAge(i), ["x"] = e.GetHitRecordX(i), ["y"] = e.GetHitRecordZ(i)
                })));
            }
            var expectedExtra = (JArray)expected["extra"].DeepClone();
            foreach (JObject e in expectedExtra)
            {
                e.Remove("yResolved");
                foreach (string axis in new[] { "x", "y", "z" }) e[axis] = (double)e[axis];
            }
            if (!JToken.DeepEquals(extra, expectedExtra)) differences.Add(label + " extra=" + extra.ToString(Formatting.None) + " expected=" + expectedExtra.ToString(Formatting.None));
            if (!JToken.DeepEquals(rests, expected["rest"])) differences.Add(label + " rest differs");
            if (!JToken.DeepEquals(sparks, expected["sparks"])) differences.Add(label + " sparks differ");
            var random = world.NativeRandom.CaptureScalarState();
            var rng = expected["rng"];
            string hash = (string)rng["synchronized"]["tableHash64"];
            ulong tableHash = ulong.Parse(hash.Replace("0x", "").Replace("0X", ""), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            if (random.CrtState != (uint)rng["crt"]["state"] || random.CrtCalls != (ulong)rng["crt"]["totalCalls"] ||
                random.SynchronizedCounter != (int)rng["synchronized"]["counter"] || random.SynchronizedIndex != (int)rng["synchronized"]["index"] ||
                random.SynchronizedCalls != (ulong)rng["synchronized"]["totalCalls"] || random.LastSynchronizedCallSite != (uint)rng["synchronized"]["lastCallSite"] ||
                random.SynchronizedTableHash != tableHash) differences.Add(label + " random state differs");
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
}
#endif
