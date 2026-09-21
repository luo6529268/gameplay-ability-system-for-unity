#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06NativePostDisplayResourceEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q06-DISPLAY-POST-DISPLAY-SOURCE-WITNESS-001/post.tsv";
        private const string WitnessSha = "4C8CC6013FF44CB2613C5AC2F42B5BF056522B4C564829B9D539BD2B49D8B644";
        private delegate void AdvancePost(LF2Entity entity, int mode, bool hasStageBounds, int width, int near, int far);

        [Test]
        public void PostDisplayMatches2379OriginalFunctionVectors()
        {
            AdvancePost advance = ResolvePost();
            Action<NTSDEntityRuntime> display = ResolveDisplay();
            using (var stream = File.OpenRead(Witness))
            using (var sha = SHA256.Create())
                Assert.That(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""), Is.EqualTo(WitnessSha));

            var differences = new List<string>();
            int count = 0;
            foreach (string line in File.ReadLines(Witness).Skip(1))
            {
                double[] row = line.Split('\t').Select(x => double.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                Assert.That(row.Length, Is.EqualTo(36));
                var entity = Create(row);
                display(entity.Runtime);
                var preserved = CapturePreserved(entity);
                LF2FrameData descriptor = entity.Frame.D;
                advance(entity, (int)row[11], row[15] != 0, row[15] == 2 ? 0 : 101,
                    row[15] == 2 ? 10 : -9, row[15] == 2 ? -9 : 10);
                double[] actual = CaptureOutput(entity);
                int differenceBefore = differences.Count;
                for (int column = 0; column < actual.Length; column++)
                {
                    if (actual[column] != row[column + 16])
                        differences.Add($"row {count}, output column {column + 16}: {actual[column]:R} != {row[column + 16]:R}");
                }
                ComparePreserved(preserved, CapturePreserved(entity), count, differences);
                if (!ReferenceEquals(descriptor, entity.Frame.D))
                    differences.Add($"row {count}: post-display changed cached frame descriptor before frame entry");
                if (entity.Runtime.Vx != 0 || entity.Runtime.Vy != 0 || entity.Runtime.Vz != 0)
                    differences.Add($"row {count}: original zero motion changed");
                TestContext.WriteLine(JsonConvert.SerializeObject(new
                {
                    index = count, input = row.Take(16), actual, expected = row.Skip(16).Take(16),
                    sourceDiagnosticPassCounters = row.Skip(32).Take(4),
                    nonTargetFieldsUnchanged = differences.Count == differenceBefore,
                    differences = differences.Skip(differenceBefore).ToArray(),
                }));
                count++;
            }
            TestContext.WriteLine($"sourceVectorsExecuted={count}; differences={differences.Count}; source pass counters are diagnostic only, not Unity persistent fields.");
            Assert.That(count, Is.EqualTo(2379));
            Assert.That(differences, Is.Empty, string.Join("\n", differences.Take(30)));
        }

        [Test]
        public void SourceCodeDerived405ClearsNonzeroMotionOnlyWithValidBounds()
        {
            AdvancePost advance = ResolvePost();
            foreach (int stage in new[] { 0, 1, 2 })
            {
                var row = DefaultRow();
                row[2] = 405;
                var entity = Create(row);
                entity.Runtime.SetVelocity(3.25, -4.5, 6.75);
                advance(entity, 0, stage != 0, stage == 2 ? 0 : 101, stage == 2 ? 10 : -9, stage == 2 ? -9 : 10);
                var r = entity.Runtime;
                Assert.That(new[] { r.Vx, r.Vy, r.Vz }, Is.EqualTo(stage == 1 ? new double[] { 0, 0, 0 } : new[] { 3.25, -4.5, 6.75 }));
                Assert.That(new[] { r.XInt, r.YInt, r.ZInt }, Is.EqualTo(stage == 1 ? new[] { 50, -7, 0 } : new[] { 11, -7, 13 }));
                Assert.That(new[] { r.X, r.Y, r.Z }, Is.EqualTo(stage == 1 ? new double[] { 50, -7, 0 } : new[] { 11.25, -7.5, 13.75 }));
                TestContext.WriteLine(JsonConvert.SerializeObject(new
                {
                    evidence = "source-code-derived: battle_world.cpp previous_state 405; original post.tsv vectors have zero motion",
                    stage, actual = CaptureOutput(entity), motion = new[] { r.Vx, r.Vy, r.Vz },
                }));
            }
        }

        [Test]
        public void ProductionDisplayReadsPreLimitAndFrameSeesRestoreBeforeTimerDecrement()
        {
            var row = DefaultRow();
            row[4] = 501; row[5] = 501; row[10] = 1;
            var world = new SimulationWorld();
            var entity = Create(row, world);
            try
            {
                world.LateEntityUpdateAll(1);
                Assert.That(entity.SawFrame, Is.True);
                Assert.That(entity.SeenAtFrame, Is.EqualTo(new[] { 500, 500, 501, 501, 1, 0, 0, 29 }),
                    "Frame must observe the post transaction; display must retain its pre-limit observation and timer must still be 1.");
                Assert.That(entity.Runtime.FullRestoreTimer1B0, Is.Zero, "Existing subsequent C25 timer owner decrements 1 to 0.");
                Assert.That(entity.Runtime.DisplayCurrentHp200, Is.EqualTo(501));
                Assert.That(entity.Runtime.HP, Is.EqualTo(42), "Frame observer mutation must not rewrite the earlier display.");
                TestContext.WriteLine(JsonConvert.SerializeObject(new { entity.SeenAtFrame, timerAfterTail = entity.Runtime.FullRestoreTimer1B0 }));
            }
            finally { Stop(world); }
        }

        [TestCase(0, 100, 200, 37, 9)]
        [TestCase(2, 500, 500, 0, 0)]
        [TestCase(3, 500, 500, 0, 0)]
        public void ProductionUsesExistingWorldModeCarrierWithZeroTimer(int mode, int hp, int bound, int consumed, int ko)
        {
            var world = new SimulationWorld();
            var entity = Create(DefaultRow(), world);
            try
            {
                world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = mode;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.SawFrame, Is.True);
                Assert.That(entity.SeenAtFrame, Is.EqualTo(new[] { hp, bound, 100, 200, 0, consumed, ko, 29 }));
                Assert.That(entity.Runtime.FullRestoreTimer1B0, Is.Zero);
                TestContext.WriteLine(JsonConvert.SerializeObject(new { mode, entity.SeenAtFrame }));
            }
            finally { Stop(world); }
        }

        [Test]
        public void ProductionFrameBodyConsumesRawActionAndCommitsHistory()
        {
            var world = CreateProductionWorld(out var entity);
            try
            {
                Assert.That(entity.GetType(), Is.EqualTo(typeof(LF2Character)), "No frame observer override is used.");
                world.LateEntityUpdateAll(1);
                AssertProductionResult(entity);
            }
            finally { Stop(world); }
        }

        [Test]
        public void ProductionPostTransactionSnapshotRestoreReplaysSameResultAndChecksum()
        {
            var world = CreateProductionWorld(out var entity);
            try
            {
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                var input = new FrameInputSet(1, Array.Empty<SimulationPlayerInput>());
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                double[] initial = CaptureProductionResult(entity);
                ulong initialChecksum = world.CaptureRuntimeChecksum64(0, new FrameInputSet(0, Array.Empty<SimulationPlayerInput>()));
                world.LateEntityUpdateAll(1);
                AssertProductionResult(entity);
                double[] expected = CaptureProductionResult(entity);
                ulong expectedChecksum = world.CaptureRuntimeChecksum64(1, input);
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                entity = (LF2Character)world.FindEntityByRuntimeSlotForQuery(20);
                Assert.That(entity, Is.Not.Null);
                Assert.That(CaptureProductionResult(entity), Is.EqualTo(initial));
                Assert.That(world.CaptureRuntimeChecksum64(0, new FrameInputSet(0, Array.Empty<SimulationPlayerInput>())), Is.EqualTo(initialChecksum));
                world.LateEntityUpdateAll(1);
                AssertProductionResult(entity);
                Assert.That(CaptureProductionResult(entity), Is.EqualTo(expected));
                Assert.That(world.CaptureRuntimeChecksum64(1, input), Is.EqualTo(expectedChecksum));
                TestContext.WriteLine(JsonConvert.SerializeObject(new
                {
                    scope = "Production catalog/factory shell, pre-Late snapshot, identical empty input and Late pass replay; not a full host tick.",
                    initialChecksum, expectedChecksum, expected,
                }));
            }
            finally { Stop(world); }
        }

        private static SimulationWorld CreateProductionWorld(out LF2Character entity, bool renderer = false)
        {
            const int oid = 31984;
            var parsed = new Lf2DatParserV2().ParseLoganContent(
                "<bmp_begin>\nname: PostDisplayContinuation\nframe_0mp: 900\n<bmp_end>\n" +
                "<stats> max_mp: 300 <stats_end>\n" +
                "<frame> 0 entry\npic: 0 state: 0 wait: 100 next: 0\n<frame_end>\n" +
                "<frame> 800 previous\npic: 0 state: 405 wait: 100 next: 800\n<frame_end>\n" +
                "<frame> 900 depleted\npic: 0 state: 7005 wait: 100 next: 900\n<frame_end>\n");
            var data = new LF2CharacterData
            {
                name = "PostDisplayContinuation", type_sub = 0,
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties),
                    LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties)),
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            var wrapper = new LF2CharacterDataWrapper(oid, data);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(id => id == oid ? wrapper : null));
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(oid, 0, "post-display-continuation-fixture.dat") },
                id => id == oid ? wrapper : null);
            world.Runtime.Stage.SetSceneSnapshot(101, -9, 10, 0, 0);
            world.Runtime.NativeWorldClock.ResourcePhase12 = 1;
            world.Runtime.NativeWorldClock.ResourcePhase3 = 1;
            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = 2;
            var task = new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = 20, preserveActionZero = true,
                useDirectRuntimePosition = true, skipPostInitZOffset = true, dir = "right",
                opoint = new ObjectPoint { oid = oid, kind = 0, action = 0 }
            };
            if (renderer)
            {
                entity = (LF2Character)LF2ObjectPointFactory.Instance.CreateObjectImmediate(task);
                Assert.That(entity, Is.Not.Null);
                Assert.That(entity.Renderer, Is.Not.Null);
            }
            else
            {
                entity = (LF2Character)world.LogicEntityFactory.Create(task, out var failure);
                Assert.That(entity, Is.Not.Null, failure.ToString());
            }
            var r = entity.Runtime;
            entity.Frame.Prev = 800;
            entity.Frame.Prev2 = 45;
            entity.Frame.Prev2D = entity.FrameCache.GetNativeFrameDataById(45);
            r.PrevFrame2 = 45;
            entity.Trans.SyncDirectFrameData(100, 0, 0);
            r.AttackingCounter = 17;
            r.HP = 1; r.HPBound = 0; r.HP3 = 500; r.PP = 400;
            r.OrdinaryCreditGate2F4 = -1; r.FullRestoreTimer1B0 = 1;
            r.InputHpConsumedTotal34C = 37; r.KnockoutCount358 = 9;
            r.NativeComputerState1B8 = 9;
            r.SetPosition(11.25, 0, 13.75);
            r.XInt = 11; r.YInt = 0; r.ZInt = 13;
            r.SetVelocity(3.25, -4.5, 6.75);
            return world;
        }

        private static double[] CaptureProductionResult(LF2Entity entity)
        {
            var r = entity.Runtime;
            return CaptureOutput(entity).Concat(new double[]
            {
                entity.Frame.D?.frameId ?? -1, entity.Frame.Prev, entity.Frame.Prev2, r.PrevFrame2,
                entity.Trans.WaitCounter, r.AttackingCounter, r.FullRestoreTimer1B0,
                r.NativeComputerState1B8, r.Vx, r.Vy, r.Vz,
            }).ToArray();
        }

        private static void AssertProductionResult(LF2Entity entity)
        {
            var r = entity.Runtime;
            Assert.That(new[] { r.Frame, entity.Frame.D.frameId, entity.Frame.Prev, entity.Trans.WaitCounter },
                Is.EqualTo(new[] { 900, 900, 900, 900 }));
            Assert.That(new[] { r.AttackingCounter, r.FullRestoreTimer1B0, r.NativeComputerState1B8 }, Is.EqualTo(new[] { 1, 0, 8 }),
                "Slot20 is outside computer refresh slots0..9; the existing timer tail decrements 9 to 8.");
            Assert.That(new[] { entity.Frame.Prev2, r.PrevFrame2 }, Is.EqualTo(new[] { 45, 45 }));
            Assert.That(new[] { r.HP, r.HPBound, r.PP, r.InputHpConsumedTotal34C, r.KnockoutCount358 },
                Is.EqualTo(new[] { 500, 500, 300, 0, 0 }));
            Assert.That(new[] { r.X, r.Y, r.Z, r.Vx, r.Vy, r.Vz }, Is.EqualTo(new double[] { 50, 0, 0, 0, 0, 0 }));
        }

        internal static void VerifyProductionForPlay()
        {
            var world = CreateProductionWorld(out var entity, true);
            try
            {
                world.LateEntityUpdateAll(1);
                AssertProductionResult(entity);
                Assert.That(entity.Renderer, Is.Not.Null);
            }
            finally
            {
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                Stop(world);
                Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
            }
        }

        private static AdvancePost ResolvePost()
        {
            Type type = typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.Ecs.BattleNativePostDisplayResourceWriter");
            MethodInfo method = type?.GetMethod("Advance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(LF2Entity), typeof(int), typeof(bool), typeof(int), typeof(int), typeof(int) }, null);
            Assert.That(method, Is.Not.Null,
                "missingowner: BattleNativePostDisplayResourceWriter.Advance(LF2Entity,int,bool,int,int,int); sourceVectorsExecuted=0. This is owner-resolution RED, not 2379 executed vector mismatches.");
            return (AdvancePost)Delegate.CreateDelegate(typeof(AdvancePost), method);
        }

        private static Action<NTSDEntityRuntime> ResolveDisplay()
        {
            Type type = typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.Ecs.BattleNativeDisplayWriter");
            MethodInfo method = type?.GetMethod("Advance", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (Action<NTSDEntityRuntime>)Delegate.CreateDelegate(typeof(Action<NTSDEntityRuntime>), method);
        }

        private static double[] DefaultRow() => new double[]
        {
            0, 0, 0, 0, 100, 200, 500, 300, 500, -1, 0, 0, 0, 0, 1, 0,
        };

        [TestCase(false, 500)]
        [TestCase(true, 100)]
        public void LocalPendingSlotUsesIndependentNativePendingGate(bool nativePending, int expectedHp)
        {
            var world = new SimulationWorld();
            var entity = Create(DefaultRow(), world);
            try
            {
                entity.Runtime.PendingFlushDestroy = true;
                entity.Runtime.NativeLifecycleResolutionPending = nativePending;
                entity.Runtime.FullRestoreTimer1B0 = 1;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.Runtime.HP, Is.EqualTo(expectedHp));
                Assert.That(entity.Runtime.DisplayCurrentHp200, Is.EqualTo(100));
                Assert.That(entity.Runtime.FullRestoreTimer1B0, Is.EqualTo(1));
                Assert.That(entity.SawFrame, Is.False);
            }
            finally { Stop(world); }
        }

        [TestCase(0, 7005, 5)]
        [TestCase(7005, 0, 9)]
        public void ComputerRefreshReadsActionWrittenByPost(int entryState, int depletedState, int expected)
        {
            var row = DefaultRow();
            row[1] = entryState; row[3] = 900; row[4] = 1; row[5] = 0;
            var world = new SimulationWorld();
            var entity = Create(row, world, depletedState);
            try
            {
                entity.Runtime.NativeComputerState1B8 = 9;
                world.LateEntityUpdateAll(1);
                Assert.That(entity.SawFrame, Is.True);
                Assert.That(entity.ComputerSeenAtFrame, Is.EqualTo(expected));
                Assert.That(entity.Runtime.Frame, Is.EqualTo(900));
            }
            finally { Stop(world); }
        }

        private static FrameProbe Create(double[] row, SimulationWorld world = null, int depletedState = 63)
        {
            int I(int index) => (int)row[index];
            string text = $"<bmp_begin>\nname: DisplayPostWitness\nframe_0mp: {I(3)}\n<bmp_end>\n" +
                $"<stats> max_mp: {I(8)} <stats_end>\n" +
                $"<frame> {I(0)} current\npic: 0 state: {I(1)} wait: 100 next: {I(0)}\n<frame_end>\n" +
                $"<frame> 800 previous\npic: 0 state: {I(2)} wait: 100 next: 800\n<frame_end>\n" +
                $"<frame> 900 depleted\npic: 0 state: {depletedState} wait: 100 next: 900\n<frame_end>\n";
            var parsed = new Lf2DatParserV2().ParseLoganContent(text);
            var data = new LF2CharacterData
            {
                type_sub = I(12),
                NativeMetadata = new LoganDefinitionMetadata(LoganDefinitionMetadata.CopyFields(parsed.Bmp.Properties),
                    LoganDefinitionMetadata.CopyFields(parsed.LoganStats?.Properties)),
            };
            foreach (var frame in parsed.Frames) data.frames.Add(Lf2DatConverter.ConvertLoganFrameData(frame));
            var entity = new FrameProbe { ObjectId = 77, DataType = I(12) };
            entity.ModuleInitialize();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(77, data));
            if (world != null)
            {
                entity.SetRequiredRuntimeSlot(0);
                world.Register(entity);
                world.Runtime.NativeWorldClock.ResourcePhase12 = 1;
                world.Runtime.NativeWorldClock.ResourcePhase3 = 1;
            }
            var r = entity.Runtime;
            r.Frame = I(14) == 0 ? 9999 : I(0);
            entity.Frame.D = entity.FrameCache.GetNativeFrameDataById(r.Frame);
            Assert.That(entity.Frame.D != null, Is.EqualTo(I(14) != 0), "No cached-frame fallback may turn action 9999 into a valid descriptor.");
            entity.Frame.Prev = 800;
            entity.Frame.Prev2 = 45;
            entity.Frame.Prev2D = entity.FrameCache.GetNativeFrameDataById(45);
            r.PrevFrame2 = 45;
            entity.Trans.SyncDirectFrameData(100, I(0), I(0));
            r.AttackingCounter = 17;
            r.HP = I(4); r.HPBound = I(5); r.HP3 = I(6); r.PP = I(7);
            r.OrdinaryCreditGate2F4 = I(9); r.FullRestoreTimer1B0 = I(10);
            r.NativeLifecycleResolutionPending = I(13) != 0;
            r.InputHpConsumedTotal34C = 37; r.KnockoutCount358 = 9;
            r.HP2Orig = 7; r.RelationTeam = 8;
            r.SetPosition(11.25, -7.5, 13.75);
            r.XInt = 11; r.YInt = -7; r.ZInt = 13;
            r.SetVelocity(0, 0, 0);
            r.DisplayCurrentHp200 = r.DisplayEffectiveMaxHp208 = 0;
            r.InputScoreTotal348 = 29; r.InputMpConsumedTotal350 = 31;
            return entity;
        }

        private static double[] CaptureOutput(LF2Entity entity)
        {
            var r = entity.Runtime;
            return new double[]
            {
                r.Frame, r.HP, r.HPBound, r.PP, r.InputHpConsumedTotal34C, r.KnockoutCount358,
                r.HP2Orig, r.RelationTeam, r.XInt, r.YInt, r.ZInt, r.X, r.Y, r.Z,
                r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208,
            };
        }

        private static Dictionary<string, object> CapturePreserved(LF2Entity entity)
        {
            var allowed = new HashSet<string>
            {
                "InputHpConsumedTotal34C", "KnockoutCount358", "HP2Orig", "X", "Y", "Z", "Vy", "Vz",
            };
            var fields = typeof(NTSDEntityRuntime).GetFields(BindingFlags.Public | BindingFlags.Instance);
            var values = fields.Where(f => (f.FieldType.IsPrimitive || f.FieldType.IsEnum || f.FieldType == typeof(string)) && !allowed.Contains(f.Name))
                .ToDictionary(f => f.Name, f => f.GetValue(entity.Runtime));
            values.Add("HP3", entity.Runtime.HP3);
            values.Add("Frame.Prev", entity.Frame.Prev);
            values.Add("Frame.Prev2", entity.Frame.Prev2);
            values.Add("Trans.WaitCounter", entity.Trans.WaitCounter);
            values.Add("PendingFlushDestroy", entity.Runtime.PendingFlushDestroy);
            values.Add("Dir", entity.Runtime.Dir);
            values.Add("LinkState", entity.Runtime.LinkState);
            values.Add("HitStop", entity.Runtime.HitStop);
            return values;
        }

        private static void ComparePreserved(Dictionary<string, object> before, Dictionary<string, object> after, int index, List<string> differences)
        {
            foreach (var pair in before)
                if (!Equals(pair.Value, after[pair.Key])) differences.Add($"row {index}: non-target {pair.Key} changed from {pair.Value} to {after[pair.Key]}");
        }

        private static void Stop(SimulationWorld world)
        {
            world.BeginBattleShutdown();
            Assert.That(world.TryShutdownAndClearLogicState(out _, out string reason), Is.True, reason);
        }

        private sealed class FrameProbe : LF2Character
        {
            internal int DataType;
            internal bool SawFrame;
            internal int[] SeenAtFrame;
            internal int ComputerSeenAtFrame;
            public override int GetCurrentDataObjectTypeForSimulation() => DataType;
            public override void SimFrameTick(int tickIndex)
            {
                SawFrame = true;
                var r = Runtime;
                ComputerSeenAtFrame = r.NativeComputerState1B8;
                SeenAtFrame = new[] { r.HP, r.HPBound, r.DisplayCurrentHp200, r.DisplayEffectiveMaxHp208,
                    r.FullRestoreTimer1B0, r.InputHpConsumedTotal34C, r.KnockoutCount358, r.InputScoreTotal348 };
                r.HP = 42;
            }
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06NativePostDisplayResourcePlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_PostDisplayPlay.request";
        private const string Result = "Temp/NTSD28_Q06_PostDisplayPlay.result.json";

        static NTSD28Q06NativePostDisplayResourcePlayProbe()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run")
                return;
            var driver = SimulationTickDriver.Instance;
            var scene = driver?.World;
            if (scene == null || driver.CurrentTickIndex < 5 || !scene.IsBattleSnapshotBoundaryReady)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            File.WriteAllText(Request, "running");
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            ulong checksum = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input);
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var errors = new List<string>();
            int passed = 0;
            try
            {
                NTSD28Q06NativePostDisplayResourceEditorTests.VerifyProductionForPlay();
                passed++;
            }
            catch (Exception error)
            {
                errors.Add(error.ToString());
            }
            bool unchanged = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input) == checksum;
            int after = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status = errors.Count == 0 && passed == 1 && unchanged && borrowers == after ? "PASS" : "FAIL",
                passedCases = passed, errors, sceneChecksumUnchanged = unchanged,
                rendererBorrowersBefore = borrowers, rendererBorrowersAfter = after,
                scope = "Real Play pooled Renderer and production Late post-resource/frame continuation with synthetic catalog. Not formal DAT or image parity."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
            File.WriteAllText("Temp/NTSD28_Q05_ReplayPlay.request", "run");
        }
    }
}
#endif
