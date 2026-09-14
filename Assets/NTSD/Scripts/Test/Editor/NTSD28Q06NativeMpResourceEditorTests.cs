#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeMpResourceEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001/native-mp.tsv";

        [Test]
        public void CompleteMpTransactionMatchesNativeMemberFunctionMatrix()
        {
            var apply = ResolveTransaction();
            Assert.That(File.Exists(Witness), Is.True, "Build and execute the original-source witness first.");
            int count = 0;
            foreach (string line in File.ReadLines(Witness).Skip(1))
            {
                int[] v = line.Split('\t').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                var entity = new ProbeCharacter { ObjectId = v[0], DataType = v[1] };
                Configure(entity, v[0], v[4], v[5] != 0, v[6], v[7]);
                entity.Runtime.HP = v[2];
                entity.Runtime.PP = v[3];
                entity.Runtime.InputDoubleCost19C = v[8];
                entity.Runtime.WeakTimer12C = v[9];
                entity.Runtime.MpRegenBonusTimer1A4 = v[10];
                entity.Runtime.HitStop = v[11];
                entity.Runtime.OrdinaryCreditGate2F4 = v[12];
                entity.Runtime.Frame = v[16] == 0 ? 9999 : 0;
                entity.Runtime.PendingFlushDestroy = v[17] != 0;
                apply(entity, v[15], v[13], v[14] != 0);
                Assert.That(entity.Runtime.PP, Is.EqualTo(v[18]), "Native vector " + count + ": " + line);
                Assert.That(entity.Runtime.HP, Is.EqualTo(v[2]), "MP transaction must not apply the HP phase.");
                count++;
            }
            Assert.That(count, Is.GreaterThan(1000));
            TestContext.WriteLine("Compared native MP vectors: " + count);
        }

        [TestCase(false, 0, 1, 0, 7, 100, 107)]
        [TestCase(true, 0, 1, 0, 7, 100, 107)]
        [TestCase(false, 1, 3, 0, 7, 100, 100)]
        [TestCase(true, 1, 3, 0, 7, 100, 100)]
        [TestCase(false, 0, 1, -3, 0, 500, 498)]
        [TestCase(true, 0, 1, -3, 0, 500, 498)]
        [TestCase(false, 0, 3, 0, 0, 200, 200)]
        [TestCase(true, 0, 3, 0, 0, 200, 200)]
        public void BothProductionCallersUseWorldPhaseAndCompleteRules(bool optimized, int phase, int tick,
            int regen, int cmp, int before, int expected)
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 31992 };
            var wrapper = Configure(entity, 31992, regen, true, 0, cmp);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31992, 0, "mp-fixture.dat") }, _ => wrapper);
            world.Register(entity);
            try
            {
                world.Runtime.NativeWorldClock.ResourcePhase3 = phase;
                world.Runtime.NativeWorldClock.ResourcePhase12 = 1;
                world.Runtime.Flow.BattleStepMode = 1;
                world.Runtime.Flow.BattleStepGate = 0;
                entity.Runtime.PP = before;
                entity.Runtime.HP = 500;
                if (optimized) new BattleEcsCharacterRecoveryPass(world).Execute(entity, tick);
                else entity.RunPreCollisionRecoveryPhase(tick);
                Assert.That(entity.Runtime.PP, Is.EqualTo(expected));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [Test]
        public void CurrentDefinitionFrameWinsOverStaleDisplayFrameAndMissingFrameSkips()
        {
            var apply = ResolveTransaction();
            var entity = new ProbeCharacter();
            Configure(entity, 77, -1, true, 0, 7);
            entity.Frame.D = new LF2FrameData { frameId = 55, cmp = 100 };
            entity.Runtime.PP = 100;
            apply(entity, 0, 1, true);
            Assert.That(entity.Runtime.PP, Is.EqualTo(107));
            entity.Runtime.Frame = 9999; // An undeclared 0..998 action is a valid native zero frame.
            apply(entity, 0, 0, true);
            Assert.That(entity.Runtime.PP, Is.EqualTo(107));
        }

        [Test]
        public void WarmMpTransactionDoesNotAllocate()
        {
            var apply = ResolveTransaction();
            var entity = new ProbeCharacter();
            Configure(entity, 77, -3, true, 1, 5);
            for (int i = 0; i < 16; i++) apply(entity, 0, 0, true);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++) apply(entity, 0, 0, true);
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
        }

        [TestCase(BattleEcsCharacterRecoveryPassMode.Legacy)]
        [TestCase(BattleEcsCharacterRecoveryPassMode.DataOriented)]
        public void AllowedRegenFamilyConsumesLastBonusBeforeTimerTail(BattleEcsCharacterRecoveryPassMode mode)
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 31992 };
            var wrapper = Configure(entity, 31992, -7, true, 0, 0);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(31992, 0, "mp-bonus.dat") }, _ => wrapper);
            world.ConfigureBattleEcsCharacterRecoveryPassForDiagnostics(mode);
            world.Register(entity);
            try
            {
                world.Runtime.NativeWorldClock.ResourcePhase3 = 0;
                world.Runtime.NativeWorldClock.ResourcePhase12 = 3;
                entity.Runtime.HP = 400;
                entity.Runtime.PP = 100;
                entity.Runtime.MpRegenBonusTimer1A4 = 1;
                world.LateEntityUpdateAll(3);
                Assert.That(entity.Runtime.PP, Is.EqualTo(103));
                Assert.That(entity.Runtime.MpRegenBonusTimer1A4, Is.Zero);
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        private static Action<LF2Entity, int, int, bool> ResolveTransaction()
        {
            MethodInfo method = typeof(BattleRecoveryStatusWriter).GetMethod("ApplyMpRecovery",
                BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] { typeof(LF2Entity), typeof(int), typeof(int), typeof(bool) }, null);
            Assert.That(method, Is.Not.Null, "The complete immutable-input MP transaction is missing.");
            return (Action<LF2Entity, int, int, bool>)Delegate.CreateDelegate(typeof(Action<LF2Entity, int, int, bool>), method);
        }

        private static LF2CharacterDataWrapper Configure(LF2Entity entity, int oid, int regen, bool hasStats, int bound, int cmp)
        {
            var fields = new List<KeyValuePair<string, string>>();
            if (hasStats)
            {
                fields.Add(new KeyValuePair<string, string>("regen_mp", regen.ToString(CultureInfo.InvariantCulture)));
                fields.Add(new KeyValuePair<string, string>("bound", bound.ToString(CultureInfo.InvariantCulture)));
                fields.Add(new KeyValuePair<string, string>("max_mp", "500"));
            }
            var data = new LF2CharacterData
            {
                NativeMetadata = new LoganDefinitionMetadata(new LoganDefinitionFieldSet(Array.Empty<KeyValuePair<string, string>>()),
                    new LoganDefinitionFieldSet(fields)),
            };
            var frame = new LF2FrameData { frameId = 0, wait = 100, next = 0, cmp = cmp };
            data.frames.Add(frame);
            var wrapper = new LF2CharacterDataWrapper(oid, data);
            entity.FrameCache.Load(wrapper);
            entity.Runtime.Frame = 0;
            entity.Frame.D = frame;
            return wrapper;
        }

        private sealed class ProbeCharacter : LF2Character
        {
            internal int DataType;
            public override int GetCurrentDataObjectTypeForSimulation() => DataType;
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06MpPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_MpPlay.request";
        private const string Result = "Temp/NTSD28_Q06_MpPlay.result.json";
        static NTSD28Q06MpPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused && world.NativeResourcePhase3 != 2) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var report = new Report { tickBefore = driver.CurrentTickIndex, objectsBefore = world.ObjectCount };
            try
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(0);
                Assert.That(entity?.Renderer, Is.Not.Null);
                report.objectId = LF2Entity.ResolveCurrentDataObjectId(entity);
                Assert.That(entity.FrameCache.Wrapper.characterData.NativeMetadata?.Stats.Int32OrDefault("regen_mp", 0) ?? 0, Is.Zero);
                Assert.That(entity.FrameCache.GetFrameDataById(entity.Runtime.Frame).cmp, Is.Zero);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tickBefore, snapshot), Is.True);
                var checksumInput = new FrameInputSet(report.tickBefore, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tickBefore, checksumInput).OverallChecksum;
                try
                {
                    entity.Runtime.HP = 500;
                    entity.Runtime.PP = 200;
                    entity.Runtime.WeakTimer12C = 0;
                    entity.Runtime.MpRegenBonusTimer1A4 = 0;
                    entity.Runtime.OrdinaryCreditGate2F4 = -1;
                    entity.Runtime.HitStop = 0;
                    var frame = new FrameInputSet(report.tickBefore + 1, new[]
                    {
                        new SimulationPlayerInput(0, SimulationInputButtons.None),
                        new SimulationPlayerInput(1, SimulationInputButtons.None),
                    });
                    Assert.That(driver.StepOneTick(frame, true, true), Is.True);
                    report.phaseAfterTick = world.NativeResourcePhase3;
                    Assert.That(report.phaseAfterTick, Is.Zero, "Probe must observe a real MP cadence tick.");
                    report.defaultModeMp = entity.Runtime.PP;
                    Assert.That(report.defaultModeMp, Is.EqualTo(200));
                    BattleRecoveryStatusWriter.ApplyMpRecovery(entity, 0, 0, true);
                    report.allowedModeMp = entity.Runtime.PP;
                    Assert.That(report.allowedModeMp, Is.EqualTo(201));
                    entity.Runtime.WeakTimer12C = 1;
                    BattleRecoveryStatusWriter.ApplyMpRecovery(entity, 0, 0, true);
                    report.weakSuppressedMp = entity.Runtime.PP;
                    Assert.That(report.weakSuppressedMp, Is.EqualTo(201));
                }
                finally
                {
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tickBefore, checksumInput).OverallChecksum, Is.EqualTo(checksum));
                    report.restored = true;
                }
                report.objectsAfter = world.ObjectCount;
                Assert.That(report.objectsAfter, Is.EqualTo(report.objectsBefore));
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
            // The existing Q05 replay/ordered-shutdown probe is requested separately after this result is inspected.
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Real current-content Scene: one injected neutral production tick, explicit mode0/weak checks, full state restored; no physical-input or formal-Logan-visual claim.";
            public int tickBefore, objectId, objectsBefore, objectsAfter, phaseAfterTick;
            public int defaultModeMp, allowedModeMp, weakSuppressedMp;
            public bool restored;
        }
    }
}
#endif
