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
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeDisplayEditorTests
    {
        [Test]
        public void DisplayMatches980OriginalFunctionVectors()
        {
            var advance = Resolve();
            int count = 0;
            foreach (string line in File.ReadLines("artifacts/diagnostics/NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001/display.tsv").Skip(1))
            {
                int[] v = line.Split('\t').Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                var r = new NTSDEntityRuntime();
                r.PendingFlushDestroy = v[1] != 0;
                r.Frame = v[2] == 0 ? 9999 : 0;
                r.DisplayScore1F0 = r.DisplayDamageTotal1F8 = r.DisplayCurrentHp200 = r.DisplayEffectiveMaxHp208 = v[3];
                r.InputScoreTotal348 = r.InputHpConsumedTotal34C = r.HP = r.HPBound = v[4];
                r.DisplayScoreStep1F4 = r.DisplayDamageStep1FC = r.DisplayCurrentHpStep204 = r.DisplayEffectiveMaxHpStep20C = v[5];
                advance(r);
                Assert.That(Values(r), Is.EqualTo(v.Skip(9).Take(4).ToArray()), "first: " + line);
                advance(r);
                Assert.That(Values(r), Is.EqualTo(v.Skip(13).Take(4).ToArray()), "second: " + line);
                Assert.That(new[] { r.HP, r.HPBound, r.InputScoreTotal348, r.InputHpConsumedTotal34C }, Is.EqualTo(new[] { 10, 10, 10, 10 }));
                count++;
            }
            Assert.That(count, Is.EqualTo(980));
        }

        [Test]
        public void IndependentTargetsAndStepsRemainSeparate()
        {
            var r = new NTSDEntityRuntime
            {
                DisplayScore1F0 = 2, InputScoreTotal348 = 100, DisplayScoreStep1F4 = 3,
                DisplayDamageTotal1F8 = 7, InputHpConsumedTotal34C = 80, DisplayDamageStep1FC = 11,
                DisplayCurrentHp200 = 90, HP = 12, DisplayCurrentHpStep204 = 13,
                DisplayEffectiveMaxHp208 = 120, HPBound = 17, DisplayEffectiveMaxHpStep20C = 19,
                PP = 123,
            };
            Resolve()(r);
            Assert.That(Values(r), Is.EqualTo(new[] { 5, 18, 77, 101 }));
            Assert.That(new[] { r.HP, r.HPBound, r.PP, r.InputScoreTotal348, r.InputHpConsumedTotal34C }, Is.EqualTo(new[] { 12, 17, 123, 100, 80 }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ProductionRunsAfterHpRecoveryBeforeFrame(bool legacy)
        {
            var world = new SimulationWorld();
            var entity = Create(world, 0, 0, 7);
            world.ForceLegacyLateCommonNoOpGatesForDiagnostics = legacy;
            try
            {
                entity.Runtime.DisplayCurrentHp200 = 0;
                entity.Health.HP = 100; entity.Health.HPBound = 200;
                world.Runtime.NativeWorldClock.ResourcePhase12 = 0;
                world.Runtime.NativeWorldClock.ResourcePhase3 = 1;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.SawFrame, Is.True);
                Assert.That(entity.HpSeenAtFrame, Is.EqualTo(107));
                Assert.That(entity.DisplaySeenAtFrame, Is.EqualTo(107));
                Assert.That(entity.Runtime.DisplayCurrentHp200, Is.EqualTo(107));
                Assert.That(entity.Health.HP, Is.EqualTo(42), "Frame mutation must not retroactively rewrite display.");
            }
            finally { Stop(world); }
        }

        [TestCase(0, false)]
        [TestCase(3, false)]
        [TestCase(5, false)]
        [TestCase(0, true)]
        public void OccupiedPendingSlotGetsDisplayWithoutOtherTail(int type, bool missingFrame)
        {
            var world = new SimulationWorld();
            var entity = Create(world, 0, type, 0);
            try
            {
                if (missingFrame) entity.Runtime.Frame = 9999;
                entity.Runtime.PendingFlushDestroy = true;
                entity.Runtime.DisplayScore1F0 = 9;
                entity.Runtime.InputScoreTotal348 = 10;
                entity.Runtime.DisplayScoreStep1F4 = 4;
                entity.Runtime.FullRestoreTimer1B0 = 3;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.Runtime.DisplayScore1F0, Is.EqualTo(13));
                Assert.That(entity.SawFrame, Is.False);
                Assert.That(entity.Runtime.FullRestoreTimer1B0, Is.EqualTo(3));
            }
            finally { Stop(world); }
        }

        [Test]
        public void DormantShellIsNotRevivedByDisplay()
        {
            var world = new SimulationWorld();
            var entity = Create(world, 0, 0, 0);
            try
            {
                entity.Runtime.OidMergeDormant = true;
                entity.Runtime.DisplayScore1F0 = 9;
                entity.Runtime.InputScoreTotal348 = 10;
                entity.Runtime.DisplayScoreStep1F4 = 4;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.Runtime.DisplayScore1F0, Is.EqualTo(9));
                Assert.That(entity.SawFrame, Is.False);
            }
            finally { Stop(world); }
        }

        [Test]
        public void WarmDisplayDoesNotAllocate()
        {
            var advance = Resolve();
            var runtime = new NTSDEntityRuntime();
            for (int i = 0; i < 16; i++) advance(runtime);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++) advance(runtime);
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
        }

        [TestCase(3, false)]
        [TestCase(5, true)]
        public void DisplayDoesNotRequireCharacterTypeOrValidFrame(int type, bool missingFrame)
        {
            var world = new SimulationWorld();
            var entity = Create(world, 0, type, 0);
            try
            {
                entity.Health.HP = 100;
                entity.Runtime.DisplayCurrentHp200 = 0;
                if (missingFrame) entity.Runtime.Frame = 9999;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.DisplaySeenAtFrame, Is.EqualTo(100));
            }
            finally { Stop(world); }
        }

        [Test]
        public void PendingUnregisterAndEmptySlotsAreNotDisplayCandidates()
        {
            var world = new SimulationWorld();
            var entity = Create(world, 0, 0, 0);
            try
            {
                Assert.That(world.FindEntityByRuntimeSlotForNativeDisplay(1), Is.Null);
                world.BeginDeferredEntityMutationPass();
                world.Unregister(entity);
                Assert.That(world.FindEntityByRuntimeSlotForNativeDisplay(0), Is.Null);
                world.EndLateEntityMutationTickingForModule();
                world.FlushLateEntityPendingMutationsForModule();
                Assert.That(world.FindEntityByRuntimeSlotForNativeDisplay(0), Is.Null);
            }
            finally
            {
                world.EndLateEntityMutationTickingForModule();
                Stop(world);
            }
        }

        private static Action<NTSDEntityRuntime> Resolve()
        {
            Type type = typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.Ecs.BattleNativeDisplayWriter");
            MethodInfo method = type?.GetMethod("Advance", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "C25 display owner missing.");
            return (Action<NTSDEntityRuntime>)Delegate.CreateDelegate(typeof(Action<NTSDEntityRuntime>), method);
        }

        private static int[] Values(NTSDEntityRuntime r) => new[]
        {
            r.DisplayScore1F0, r.DisplayDamageTotal1F8, r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208,
        };

        private static FrameProbe Create(SimulationWorld world, int slot, int type, int chp)
        {
            var frame = new LF2FrameData { frameId = 0, state = 0, wait = 100, next = 0, chp = chp };
            var data = new LF2CharacterData { frames = new List<LF2FrameData> { frame } };
            var entity = new FrameProbe { ObjectId = 31992, DataType = type };
            entity.ModuleInitialize();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(31992, data));
            entity.Frame.D = frame; entity.Runtime.Frame = 0;
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            return entity;
        }

        private static void Stop(SimulationWorld world)
        {
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }

        private sealed class FrameProbe : LF2Character
        {
            internal int DataType, HpSeenAtFrame, DisplaySeenAtFrame;
            internal bool SawFrame;
            public override int GetCurrentDataObjectTypeForSimulation() => DataType;
            public override void SimFrameTick(int tickIndex)
            {
                SawFrame = true;
                HpSeenAtFrame = Health.HP;
                DisplaySeenAtFrame = Runtime.DisplayCurrentHp200;
                Health.HP = 42;
            }
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06DisplayPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_DisplayPlay.request";
        private const string Result = "Temp/NTSD28_Q06_DisplayPlay.result.json";
        static NTSD28Q06DisplayPlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var report = new Report { tickBefore = driver.CurrentTickIndex, objectsBefore = world.ObjectCount };
            try
            {
                var entity = world.FindEntityByRuntimeSlotForQuery(0);
                Assert.That(entity?.Renderer, Is.Not.Null);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tickBefore, saved), Is.True);
                var checkInput = new FrameInputSet(report.tickBefore, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tickBefore, checkInput).OverallChecksum;
                try
                {
                    var r = entity.Runtime;
                    r.HP = r.HPBound = r.HP3 = 500;
                    r.EnvironmentState320 = 0;
                    r.InputScoreTotal348 = 10; r.InputHpConsumedTotal34C = 30;
                    r.DisplayScore1F0 = 9; r.DisplayScoreStep1F4 = 4;
                    r.DisplayDamageTotal1F8 = 20; r.DisplayDamageStep1FC = 6;
                    r.DisplayCurrentHp200 = 502; r.DisplayCurrentHpStep204 = 3;
                    r.DisplayEffectiveMaxHp208 = 503; r.DisplayEffectiveMaxHpStep20C = 5;
                    for (int tick = 1; tick <= 2; tick++)
                    {
                        var input = new FrameInputSet(report.tickBefore + tick, new[]
                        {
                            new SimulationPlayerInput(0, SimulationInputButtons.None),
                            new SimulationPlayerInput(1, SimulationInputButtons.None),
                        });
                        Assert.That(driver.StepOneTick(input, true, true), Is.True);
                        int[] values = { r.DisplayScore1F0, r.DisplayDamageTotal1F8, r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208 };
                        if (tick == 1) report.first = values; else report.second = values;
                        Assert.That(values, Is.EqualTo(tick == 1 ? new[] { 13, 26, 499, 498 } : new[] { 10, 32, 500, 500 }));
                        Assert.That(new[] { r.HP, r.HPBound, r.InputScoreTotal348, r.InputHpConsumedTotal34C }, Is.EqualTo(new[] { 500, 500, 10, 30 }));
                    }
                }
                finally
                {
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tickBefore, checkInput).OverallChecksum, Is.EqualTo(checksum));
                    report.restored = true;
                }
                report.objectsAfter = world.ObjectCount;
                Assert.That(report.objectsAfter, Is.EqualTo(report.objectsBefore));
                report.status = "PASS";
            }
            catch (Exception error) { report.status = "FAIL"; report.error = error.ToString(); }
            File.WriteAllText(Result, JsonConvert.SerializeObject(report, Formatting.Indented));
            File.WriteAllText(Request, "done");
        }

        private sealed class Report
        {
            public string status, error;
            public string scope = "Current legacy-content real Scene, two injected neutral production ticks; four display values and preserved sources, full checksum restored. Birth initialization and post-display are pending.";
            public int tickBefore, objectsBefore, objectsAfter;
            public int[] first, second;
            public bool restored;
        }
    }
}
#endif
