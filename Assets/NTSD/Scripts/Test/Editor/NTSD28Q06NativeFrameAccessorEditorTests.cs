#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativeFrameAccessorEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001/native-zero.tsv";

        [Test]
        public void NativeAccessorMatchesValidOriginalDocuments()
        {
            var get = Resolve();
            int compared = 0;
            foreach (int[] v in Rows())
            {
                if (v[2] == 0) continue;
                var cache = new LF2FrameCache();
                cache.Load(Wrapper(v[0]));
                var frame = get(cache, v[1]);
                Assert.That(frame != null, Is.EqualTo(v[4] != 0), "declaration/lookup=" + v[0] + "/" + v[1]);
                if (frame != null)
                    Assert.That(new[] { frame.frameId, frame.wait, frame.next, frame.state, frame.pic, frame.chp, frame.cmp }, Is.EqualTo(v.Skip(5).Take(7).ToArray()));
                compared++;
            }
            Assert.That(compared, Is.EqualTo(63));
        }

        [Test]
        public void HpAndMpUseTheSameNativeLookupAsOriginalResourcePass()
        {
            int compared = 0;
            foreach (int[] v in Rows())
            {
                if (v[2] == 0) continue;
                var entity = new LF2Character { ObjectId = 77 };
                entity.FrameCache.Load(Wrapper(v[0]));
                entity.Runtime.Frame = v[1];
                entity.Health.HP = 100; entity.Health.HPBound = 200; entity.Health.HP3 = 500; entity.Health.PP = 100;
                entity.Runtime.OrdinaryCreditGate2F4 = -1;
                BattleRecoveryStatusWriter.ApplyHpRecovery(entity, 0, 0);
                BattleRecoveryStatusWriter.ApplyMpRecovery(entity, 0, 0, true);
                Assert.That(new[] { entity.Health.HP, entity.Health.HPBound, entity.Health.PP }, Is.EqualTo(v.Skip(13).ToArray()), "declaration/lookup=" + v[0] + "/" + v[1]);
                compared++;
            }
            Assert.That(compared, Is.EqualTo(63));
        }

        [TestCase(false, 7)]
        [TestCase(true, 7)]
        [TestCase(false, 857)]
        [TestCase(true, 857)]
        [TestCase(false, 998)]
        [TestCase(true, 998)]
        public void BothProductionRecoveryCallersAcceptImplicitFrames(bool optimized, int frameId)
        {
            var world = new SimulationWorld();
            var wrapper = Wrapper(0);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(77, 0, "zero-frame.dat") }, _ => wrapper);
            var entity = new LF2Character { ObjectId = 77 };
            entity.FrameCache.Load(wrapper);
            entity.Frame.D = wrapper.characterData.frames[0];
            entity.Runtime.Frame = frameId;
            entity.Health.HP = 100; entity.Health.HPBound = 200; entity.Health.PP = 100;
            world.Register(entity);
            entity.Runtime.Frame = frameId;
            try
            {
                world.Runtime.NativeWorldClock.ResourcePhase12 = 0;
                world.Runtime.NativeWorldClock.ResourcePhase3 = 0;
                if (optimized) new BattleEcsCharacterRecoveryPass(world).Execute(entity, 12);
                else entity.RunPreCollisionRecoveryPhase(12);
                Assert.That(new[] { entity.Health.HP, entity.Health.HPBound, entity.Health.PP }, Is.EqualTo(new[] { 101, 200, 100 }));
            }
            finally
            {
                world.BeginBattleShutdown();
                Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
            }
        }

        [Test]
        public void LegacyQueriesRetainTheirExistingDeclaredRangeAndDefault()
        {
            var get = Resolve();
            var cache = new LF2FrameCache();
            var wrapper = Wrapper(999);
            cache.Load(wrapper);
            Assert.That(LF2FrameCache.MaxFrameIdExclusive, Is.EqualTo(857));
            Assert.That(cache.HasFrame(7), Is.False);
            Assert.That(cache.GetFrameDataById(7).wait, Is.EqualTo(1));
            Assert.That(cache.GetFrameDataById(7).frameId, Is.Zero);
            Assert.That(cache.GetFrameDataById(999), Is.Null);
            Assert.That(cache.HasFrame(999), Is.False);
            Assert.That(cache.GetFirstFrameByState(14), Is.EqualTo(-1));
            Assert.That(get(cache, 999), Is.SameAs(wrapper.characterData.frames[0]));
            Assert.That(get(cache, 7).frameId, Is.EqualTo(7));
            Assert.That(get(cache, 7).wait, Is.Zero);
        }

        [Test]
        public void NativeZeroTemplatesArePerIdAndSharedOnlyAsDefinitionData()
        {
            var get = Resolve();
            var a = new LF2FrameCache(); var b = new LF2FrameCache();
            Assert.That(get(a, 0), Is.Null, "Unbound cache is not an empty loaded definition.");
            a.Load(Wrapper(-999)); b.Load(Wrapper(-999));
            for (int id = 0; id < 999; id++)
            {
                var frame = get(a, id);
                Assert.That(frame, Is.SameAs(get(b, id)));
                Assert.That(frame.frameId, Is.EqualTo(id));
                Assert.That(new[] { frame.wait, frame.next, frame.pic, frame.state, frame.chp, frame.cmp }, Is.EqualTo(new int[6]));
                Assert.That(frame.UsesLoganFrameNumbers, Is.True);
                Assert.That(frame.rawProperties.Count + frame.bodies.Count + frame.itrs.Count + frame.wpoints.Count + frame.opoints.Count + frame.FrameSounds.Count, Is.Zero);
                Assert.That(frame.cpoint, Is.Null); Assert.That(frame.bpoint, Is.Null); Assert.That(frame.opoint, Is.Null);
            }
            Assert.That(get(a, 7), Is.Not.SameAs(get(a, 8)));
            Assert.That(get(a, 999), Is.Null);
            a.Clear(); Assert.That(get(a, 7), Is.Null);
            a.Load(Wrapper(7)); Assert.That(get(a, 7).wait, Is.EqualTo(2));
            a.Load(Wrapper(-999)); Assert.That(get(a, 7).wait, Is.Zero);
        }

        [Test]
        public void WarmNativeReadsAllocateNothing()
        {
            var get = Resolve(); var cache = new LF2FrameCache(); cache.Load(Wrapper(-999));
            for (int i = 0; i < 16; i++) get(cache, 998);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++) get(cache, 998);
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.Zero);
        }

        private static Func<LF2FrameCache, int, LF2FrameData> Resolve()
        {
            var method = typeof(LF2FrameCache).GetMethod("GetNativeFrameDataById", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Native zero-frame accessor missing.");
            return (Func<LF2FrameCache, int, LF2FrameData>)Delegate.CreateDelegate(typeof(Func<LF2FrameCache, int, LF2FrameData>), method);
        }

        private static System.Collections.Generic.IEnumerable<int[]> Rows() => File.ReadLines(Witness).Skip(1)
            .Select(line => line.Split('\t').Select(value => int.Parse(value, CultureInfo.InvariantCulture)).ToArray());

        private static LF2CharacterDataWrapper Wrapper(int declared)
        {
            var data = new LF2CharacterData();
            if (declared >= 0 && declared <= 999)
                data.frames.Add(new LF2FrameData { frameId = declared, pic = 29, state = 14, wait = 2, next = 0, chp = 7, cmp = 5 });
            return new LF2CharacterDataWrapper(77, data);
        }
    }

    [InitializeOnLoad]
    internal static class NTSD28Q06NativeFramePlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_NativeFramePlay.request";
        private const string Result = "Temp/NTSD28_Q06_NativeFramePlay.result.json";
        static NTSD28Q06NativeFramePlayProbe() { EditorApplication.update += Poll; }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run") return;
            var driver = SimulationTickDriver.Instance;
            var world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 || !world.IsBattleSnapshotBoundaryReady) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            File.WriteAllText(Request, "running");
            var report = new Report { tick = driver.CurrentTickIndex, objectsBefore = world.ObjectCount };
            try
            {
                var entity = world.FindEntityByRuntimeSlotForQuery(0);
                Assert.That(entity?.Renderer, Is.Not.Null);
                Assert.That(entity.GetType(), Is.EqualTo(typeof(LF2Character)));
                Assert.That(entity.FrameCache.Wrapper.characterData.NativeMetadata?.HasStatsRecord == true, Is.False,
                    "This current-content fixture uses the native no-stats HP branch.");
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var saved = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, report.tick, saved), Is.True);
                var checkInput = new FrameInputSet(report.tick, Array.Empty<SimulationPlayerInput>());
                string checksum = world.CaptureLockstepChecksumSnapshot(report.tick, checkInput).OverallChecksum;
                try
                {
                    foreach (bool optimized in new[] { false, true })
                    foreach (int action in new[] { 857, 998, 999 })
                    {
                        var native = entity.FrameCache.GetNativeFrameDataById(action);
                        Assert.That(entity.FrameCache.GetFrameDataById(action), Is.Null, "Legacy boundary is preserved.");
                        if (action == 999) Assert.That(native, Is.Null);
                        else
                        {
                            Assert.That(native.frameId, Is.EqualTo(action));
                            Assert.That(new[] { native.wait, native.chp, native.cmp }, Is.EqualTo(new int[3]));
                        }
                        entity.Runtime.Frame = action;
                        entity.Health.HP = 100; entity.Health.HPBound = 200; entity.Health.HP3 = 500; entity.Health.PP = 100;
                        entity.Runtime.WeakTimer12C = 0; entity.Runtime.HpRegenDouble1AC = 0;
                        entity.Runtime.EffectiveMaxRegenDouble1A8 = 0; entity.Runtime.MpRegenBonusTimer1A4 = 0;
                        entity.Runtime.EnvironmentState320 = 0;
                        world.Runtime.NativeWorldClock.ResourcePhase12 = 0;
                        world.Runtime.NativeWorldClock.ResourcePhase3 = 0;
                        if (optimized) new BattleEcsCharacterRecoveryPass(world).Execute(entity, report.tick);
                        else entity.RunPreCollisionRecoveryPhase(report.tick);
                        int expectedHp = action == 999 ? 100 : 101;
                        Assert.That(new[] { entity.Health.HP, entity.Health.HPBound, entity.Health.PP }, Is.EqualTo(new[] { expectedHp, 200, 100 }));
                        report.rows.Add(new Row { optimized = optimized, action = action, hp = entity.Health.HP, mp = entity.Health.PP });
                    }
                }
                finally
                {
                    Assert.That(driver.TryRestoreBattleStateSnapshot(identity, saved, out var failure), Is.True, failure.ToString());
                    Assert.That(world.CaptureLockstepChecksumSnapshot(report.tick, checkInput).OverallChecksum, Is.EqualTo(checksum));
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
            public string scope = "Real current-content Scene, paused resource owners only at injected857/998/999; old frame/input/movement readers are not migrated or exercised as a full high-action tick. Complete World checksum restored.";
            public int tick, objectsBefore, objectsAfter;
            public bool restored;
            public List<Row> rows = new List<Row>();
        }
        private sealed class Row
        {
            public bool optimized;
            public int action, hp, mp;
        }
    }
}
#endif
