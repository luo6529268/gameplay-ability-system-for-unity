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
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06WeaponReactionEditorTests
    {
        internal const string Source = "artifacts/diagnostics/NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001/first.jsonl";
        internal const string Output = "artifacts/diagnostics/NTSD28-Q06-UNARMORED-WEAPON-REACTION-001/";
        private static readonly Dictionary<string, LF2CharacterDataWrapper> wrappers = new();

        [TestCase("CheckAudit7IronBallPreprocessContracts")]
        [TestCase("CheckAudit4ArchitectDefectContracts")]
        [TestCase("CheckAudit7HitConfirmCarrierTail")]
        [TestCase("CheckWeaponVictimRawFrameWriterContract")]
        [TestCase("CheckWeaponAttackerRawFrameAndOrderingContract")]
        [TestCase("CheckWeaponTailIdentityTimingContract")]
        public void NativeWeaponLegacySelfCheckContract(string method)
        {
            var check = typeof(BattleRuntimeSelfCheck).GetMethod(method,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.That(check, Is.Not.Null);
            try { check.Invoke(null, null); }
            catch (System.Reflection.TargetInvocationException exception)
            {
                Assert.Fail(exception.InnerException?.ToString() ?? exception.ToString());
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void State2000HorizontalDirectionUsesIntegerPositions(bool shadow)
        {
            var row = File.ReadLines(Source).Select(JObject.Parse).First(value => (int)value["index"] == 36);
            var world = MakeWorld(row, BattleRuntimeProfile.Authority400, out var pair);
            try
            {
                pair[0].Runtime.SetPosition(100.1, 0, 200);
                pair[1].Runtime.SetPosition(100.9, 0, 200);
                pair[0].Runtime.SyncIntegerPosition();
                pair[1].Runtime.SyncIntegerPosition();
                Assert.That(pair[0].Runtime.XInt, Is.EqualTo(pair[1].Runtime.XInt));
                if (shadow)
                {
                    world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
                    world.CaptureBattleHitExecutionPlanPass(1, BattleHitExecutionPass.Character);
                    Assert.That(world.BeginBattleHitExecutionPlanLegacyObservation(1, BattleHitExecutionPass.Character), Is.True);
                }
                try { NTSD28Q06PrearmorFeedbackEditorTests.ConsumeRecordedCandidate(pair[0]); }
                finally { if (shadow) world.EndBattleHitExecutionPlanLegacyObservation(); }
                Assert.That(pair[1].KnockbackVx, Is.EqualTo(-5.0), "Native horizontal context stores integer X; equal positions take the negative branch.");
                if (shadow)
                {
                    var plan = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                    Assert.That(plan.CurrentTickPlanValid, Is.True);
                    Assert.That(plan.ObservedWriterEffectCount, Is.EqualTo(1));
                    Assert.That(plan.LastWriterEffectDifferenceMask, Is.Zero);
                }
            }
            finally { NTSD28Q06State18SpawnEditorTests.Shutdown(world); }
        }

        [TestCase(BattleRuntimeProfile.Authority400, false)]
        [TestCase(BattleRuntimeProfile.MobileExtended, false)]
        [TestCase(BattleRuntimeProfile.Authority400, true)]
        [TestCase(BattleRuntimeProfile.MobileExtended, true)]
        public void NativeWeaponReactionAndFinalizeMatch(BattleRuntimeProfile profile, bool shadow)
        {
            int cases = 0;
            var before = new List<string>();
            var differences = new List<string>();
            foreach (var row in File.ReadLines(Source).Select(JObject.Parse))
            {
                var world = MakeWorld(row, profile, out var pair);
                try
                {
                    RunCase(world, pair, row, shadow, before, differences);
                    cases++;
                }
                finally
                {
                    NTSD28Q06State18SpawnEditorTests.Shutdown(world);
                }
            }
            Directory.CreateDirectory(Output);
            File.WriteAllText(Output + profile + "-" + shadow + ".json", JsonConvert.SerializeObject(new { cases, before, differences }, Formatting.Indented));
            Assert.That(cases, Is.EqualTo(2100));
            Assert.That(before, Is.Empty, string.Join("\n", before.Take(12)));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(12)));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void WeaponStateSurvivesLocalSnapshotReplay(BattleRuntimeProfile profile)
        {
            var rows = File.ReadLines(Source).Select(JObject.Parse)
                .Where(row => (int)row["attackerState"] == 1002 && (int)row["attackerType"] == 0)
                .GroupBy(row => (int)row["type"]).Select(group => group.First()).ToArray();
            Assert.That(rows.Length, Is.EqualTo(4));
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
            var rest = new int[2, 2];
            for (int target = 0; target < 2; target++)
                for (int attacker = 0; attacker < 2; attacker++) rest[target, attacker] = world.GetRawRestVrest(target, attacker);
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
                    entity.HitCount, entity.KnockbackVx, entity.KnockbackVy, entity.KnockbackVz,
                    entity.Runtime.LinkState, entity.Runtime.HolderStableId, entity.Runtime.TargetSlotIndex,
                    records = Enumerable.Range(0, entity.HitRecordCount).Select(i => new[] { entity.GetHitRecordAge(i), entity.GetHitRecordX(i), entity.GetHitRecordZ(i) }).ToArray()
                }).ToArray()
            });
        }

        internal static void RunCase(SimulationWorld world, LF2Entity[] pair, JObject row, bool shadow, List<string> before, List<string> differences)
        {
            string label = "case " + row["index"];
            CompareState(world, pair, row["before"], label + " before", before);
            var observer = new Observer();
            world.NativeRandom.SetDiagnosticCallObserver(observer);
            ulong legacy = world.Rng.CallCount;
            if (shadow)
                world.ConfigureBattleHitExecutionPlanForDiagnostics(BattleHitExecutionPlanMode.ShadowCompare);
            if (shadow)
            {
                var pass = pair[0].SupportsPostInteractionPhase() ? BattleHitExecutionPass.Character : BattleHitExecutionPass.Object;
                world.CaptureBattleHitExecutionPlanPass(1, pass);
                Assert.That(world.BeginBattleHitExecutionPlanLegacyObservation(1, pass), Is.True);
                try { NTSD28Q06PrearmorFeedbackEditorTests.ConsumeRecordedCandidate(pair[0]); }
                finally { world.EndBattleHitExecutionPlanLegacyObservation(); }
            }
            else
                NTSD28Q06PrearmorFeedbackEditorTests.ConsumeRecordedCandidate(pair[0]);
            if (shadow)
            {
                var plan = world.BattleHitExecutionPlanDiagnosticsForDiagnostics;
                if (plan.ObservedWriterEffectCount != 1)
                    differences.Add(label + " writer observation count=" + plan.ObservedWriterEffectCount);
                if (!plan.CurrentTickPlanValid || plan.LastConsumeEffectsDifferenceMask != 0 || plan.LastWriterEffectDifferenceMask != 0)
                    differences.Add(label + " Shadow=" + JsonConvert.SerializeObject(plan));
                if (world.HitExecutionPlanForInteractionModule.NativePreludeObservationCountForDiagnostics != 1)
                    differences.Add(label + " native prelude observation differs");
            }
            world.NativeRandom.SetDiagnosticCallObserver(null);
            CompareState(world, pair, row["after"], label + " after", differences);
            if (!JToken.DeepEquals(JArray.FromObject(observer.Crt), row["crt"])) differences.Add(label + " CRT differs");
            if (!JToken.DeepEquals(JArray.FromObject(observer.Native), row["native"])) differences.Add(label + " native calls differ");
            if (world.Rng.CallCount != legacy) differences.Add(label + " legacy calls=" + (world.Rng.CallCount - legacy));
            if (world.PendingSounds.Count != row["audio"].Count()) differences.Add(label + " audio count=" + world.PendingSounds.Count);
            for (int i = 0; i < Math.Min(world.PendingSounds.Count, row["audio"].Count()); i++)
            {
                var sound = world.PendingSounds[i];
                if (sound.Cue != (string)row["audio"][i]["path"] || sound.WorldX != (int)row["audio"][i]["x"])
                    differences.Add(label + " audio " + i + "=" + sound.Cue + "@" + sound.WorldX);
            }
            pair[0].FrameDelay = 0;
            pair[1].FrameDelay = 0;
            world.FramePostProcessAll();
            CompareState(world, pair, row["afterReleasedHoldFinalize"], label + " finalized", differences);
        }

        private static void CompareState(SimulationWorld world, LF2Entity[] pair, JToken expected, string label, List<string> differences)
        {
            NTSD28Q06CollisionQualificationEditorTests.CompareRaw(world, expected["raw"], label, differences);
            var extra = new JArray();
            var rest = new JArray();
            var sparks = new JArray();
            for (int slot = 0; slot < 2; slot++)
            {
                var e = pair[slot];
                extra.Add(new JObject
                {
                    ["count"] = e.HitCount, ["x"] = e.KnockbackVx, ["y"] = e.KnockbackVy, ["z"] = e.KnockbackVz,
                    ["hpConsumed"] = e.Runtime.InputHpConsumedTotal34C, ["mpConsumed"] = e.Runtime.InputMpConsumedTotal350,
                    ["score"] = e.Runtime.InputScoreTotal348, ["link"] = e.Runtime.LinkState,
                    ["parent"] = e.Runtime.HolderStableId, ["child"] = e.Runtime.TargetSlotIndex
                });
                rest.Add(new JArray(world.GetRawRestVrest(slot, 0), world.GetRawRestVrest(slot, 1)));
                var events = new JArray();
                for (int i = 0; i < e.HitRecordCount; i++)
                    events.Add(new JObject { ["host"] = slot, ["id"] = e.GetHitRecordAge(i), ["x"] = e.GetHitRecordX(i), ["y"] = e.GetHitRecordZ(i) });
                sparks.Add(events);
            }
            var expectedExtra = (JArray)expected["extra"].DeepClone();
            foreach (var entity in expectedExtra)
                foreach (string axis in new[] { "x", "y", "z" }) entity[axis] = (double)entity[axis];
            foreach (var item in new[] { ("extra", extra, (JToken)expectedExtra), ("rest", rest, expected["rest"]), ("sparks", sparks, expected["sparks"]) })
                if (!JToken.DeepEquals(item.Item2, item.Item3))
                    differences.Add(label + " " + item.Item1 + "=" + item.Item2.ToString(Formatting.None) + " expected=" + item.Item3.ToString(Formatting.None));
        }

        internal static SimulationWorld MakeWorld(JObject row, BattleRuntimeProfile profile, out LF2Entity[] pair, bool renderer = false)
        {
            var wrappersForCase = new[] { Definition(row, true), Definition(row, false) };
            var world = new SimulationWorld(profile, profile == BattleRuntimeProfile.Authority400 ? 400 : 1000);
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[]
            {
                new ObjectDefinition(77, (int)row["attackerType"], "weapon-reaction-a.dat"),
                new ObjectDefinition(78, (int)row["type"], "weapon-reaction-t.dat")
            }, id => wrappersForCase[id - 77]);
            pair = new LF2Entity[2];
            for (int slot = 0; slot < 2; slot++)
            {
                var task = new OPointCreateTask
                {
                    targetWorld = world, requiredRuntimeSlot = slot, dir = "right", nativeWeaponPieceSpawn = true,
                    relationTeam = slot + 1, preserveActionZero = true, opoint = new ObjectPoint { oid = 77 + slot, action = 0 }
                };
                pair[slot] = renderer ? LF2ObjectPointFactory.Instance.CreateObjectImmediate(task) : world.LogicEntityFactory.Create(task, out _);
                Assert.That(pair[slot], Is.Not.Null);
                pair[slot].Team = slot + 1;
                pair[slot].Health.HP = slot == 0 ? 500 : (int)row["hp"];
                pair[slot].Health.HPBound = pair[slot].Health.HP;
                pair[slot].Health.HP3 = pair[slot].Health.HP;
                pair[slot].Runtime.SetPosition(100 + slot * 10, 0, 200);
                pair[slot].Runtime.SyncIntegerPosition();
            }
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(pair[0].Runtime.HitCandidateCount, Is.EqualTo(1));
            for (int slot = 0; slot < 2; slot++)
            {
                pair[slot].SwitchDir((int)row[slot == 0 ? "attackerFacing" : "targetFacing"] == 0 ? "right" : "left");
                pair[slot].AttackingCounter = slot == 0 ? 5 : 7;
                pair[slot].Trans.SyncDirectFrameData(pair[slot].Frame.D.wait, pair[slot].Frame.D.next, slot == 0 ? 13 : 17);
                pair[slot].Runtime.LinkState = 0;
                pair[slot].Runtime.HolderStableId = 0;
                pair[slot].Runtime.TargetSlotIndex = 0;
            }
            pair[0].Runtime.SetVelocity(4, 6, 5);
            pair[1].Runtime.SetVelocity((double)row["vx"], 2.5, -3);
            pair[0].HitCount = 1; pair[0].KnockbackVx = 1; pair[0].KnockbackVy = 2; pair[0].KnockbackVz = 3;
            pair[1].HitCount = 2; pair[1].KnockbackVx = (double)row["px"]; pair[1].KnockbackVy = (double)row["py"]; pair[1].KnockbackVz = 1.25;
            pair[1].Runtime.SetPosition((int)row["targetX"], (int)row["targetY"], 200);
            pair[1].Runtime.SyncIntegerPosition();
            pair[1].Runtime.Bdefend = 17;
            pair[1].FallCounter = 29;
            pair[1].Runtime.IncomingDamageScale340 = (int)row["scale"];
            pair[0].Runtime.WeakTimer12C = (int)row["weak"];
            world.NativeRandom.ResetFromSeed(42);
            return world;
        }

        private static LF2CharacterDataWrapper Definition(JObject row, bool attacker)
        {
            string key = attacker + "/" + string.Join("/", new[] { "type", "attackerType", "attackerState", "cover", "injury", "fall", "dvx", "dvy", "bdefend" }.Select(k => row[k]));
            if (wrappers.TryGetValue(key, out var cached)) return cached;
            string text = "<bmp_begin>\nname: WeaponReaction\nweapon_hp: 20\n";
            if (attacker && (int)row["attackerType"] == 3) text += "weapon_broken_sound: weapon_reaction_attacker.wav\n";
            if (!attacker) text += "weapon_hit_sound: weapon_reaction_target.wav\n";
            text += "<bmp_end>\n<frame> 0 active\nstate: " + (attacker ? (int)row["attackerState"] : 0) +
                " wait: 100 next: 0 cover: " + row["cover"] + " centerx: 3 centery: 5\n";
            if (attacker) text += "itr:\nkind: 0 x: -20 y: -20 w: 60 h: 60 injury: " + row["injury"] +
                " fall: " + row["fall"] + " dvx: " + row["dvx"] + " dvy: " + row["dvy"] + " vrest: 1 bdefend: " + row["bdefend"] + "\nitr_end:\n";
            else text += "bdy:\nkind: 0 x: -20 y: -20 w: 60 h: 60\nbdy_end:\n";
            text += "<frame_end>\n<frame> 10 response\nstate: 0 wait: 100 next: 0 dvx: 7\n<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData
            {
                type_sub = (int)row[attacker ? "attackerType" : "type"], weapon_hp = 20,
                weapon_broken_sound = attacker && (int)row["attackerType"] == 3 ? "weapon_reaction_attacker.wav" : null,
                weapon_hit_sound = attacker ? null : "weapon_reaction_target.wav",
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties), LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties))
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            var wrapper = new LF2CharacterDataWrapper(attacker ? 77 : 78, data);
            wrappers.Add(key, wrapper);
            return wrapper;
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
    internal static class NTSD28Q06WeaponReactionPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_WeaponReactionPlay.request";

        static NTSD28Q06WeaponReactionPlayProbe() { EditorApplication.update += Poll; }

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
                var selected = File.ReadLines(NTSD28Q06WeaponReactionEditorTests.Source).Select(JObject.Parse)
                    .ToArray();
                Assert.That(selected.Length, Is.EqualTo(2100));
                foreach (bool renderer in new[] { false, true })
                foreach (bool shadow in new[] { false, true })
                foreach (var row in selected)
                {
                    var world = NTSD28Q06WeaponReactionEditorTests.MakeWorld(row, BattleRuntimeProfile.Authority400, out var entities, renderer);
                    try
                    {
                        NTSD28Q06WeaponReactionEditorTests.RunCase(world, entities, row, shadow, beforeDifferences, differences);
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
                Assert.That(cases, Is.EqualTo(8400));
                Assert.That(beforeDifferences, Is.Empty, string.Join("\n", beforeDifferences.Take(10)));
                Assert.That(sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum, Is.EqualTo(checksum));
                status = "PASS";
            }
            catch (Exception exception) { error = exception.ToString(); }
            File.WriteAllText("Temp/NTSD28_Q06_WeaponReactionPlay.result.json", JsonConvert.SerializeObject(new
            {
                status, error, cases, differences, beforeDifferences, rendererBorrowersBefore = borrowers,
                rendererBorrowersAfter = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance,
                sceneChecksumUnchanged = sceneWorld.CaptureLockstepChecksumSnapshot(driver.CurrentTickIndex, input).OverallChecksum == checksum,
                scope = "Real Scene, source2100 synthetic weapon transactions through both factories and direct/Shadow. This does not validate physical keys or visual asset alignment."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }
    }
}
#endif
