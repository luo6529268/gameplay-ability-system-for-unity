#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Test
{
    public sealed class NTSD28Q06OrdinaryStageDisplayBirthEditorTests
    {
        private const int Oid = 31983;

        [TestCase(false)]
        [TestCase(true)]
        public void OrdinaryInitializePublishesFinalHpAndClearsAllDisplaySteps(bool dirty)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            if (dirty) SetMarkers(character.Runtime);
            character.Initialize(137, 500);
            AssertBirth(character.Runtime, 137);
        }

        [Test]
        public void ReusedPooledCharacterInitializeDoesNotRetainDisplayMarkers()
        {
            var world = CreateWorld();
            try
            {
                var first = Spawn(world, 20);
                SetMarkers(first.Runtime);
                first.FreeEntityLikeExe();
                var reused = Spawn(world, 20);
                Assert.That(reused, Is.SameAs(first), "The fixture must exercise the returned pooled shell.");
                ((LF2Character)reused).Initialize(137, 500);
                AssertBirth(reused.Runtime, 137);
            }
            finally { Shutdown(world); }
        }

        [TestCase(false, 0)]
        [TestCase(true, 0)]
        [TestCase(true, 3)]
        public void ActualStageBirthUsesFinalHpRatherThanFactoryVitals(bool factory, int type)
        {
            var world = CreateWorld(type);
            try
            {
                if (factory)
                {
                    var baseline = Spawn(world, 21);
                    Assert.That(baseline.Runtime.HP, Is.Not.EqualTo(137), "OPoint birth HP must differ from the Stage override.");
                    baseline.FreeEntityLikeExe();
                }
                var spawn = new BattleStageSpawnValue(id: Oid, act: 0, hp: 137,
                    times: 1, x: 300, y: 0, ratio: 0.0, join: 0);
                var module = typeof(SimulationWorld).GetField("stageWaveModule", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
                var method = module.GetType().GetMethod(factory ? "TrySpawnStageEntityWithFactory" : "TrySpawnStageCharacterDirect",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);
                var entity = (LF2Entity)method.Invoke(module, new object[] { spawn, 300, 0, 250, "right", 137, 20 });
                Assert.That(entity, Is.Not.Null);
                Assert.That(world.FindEntityByRuntimeSlotForQuery(20), Is.SameAs(entity));
                Assert.That(entity.Runtime.HP, Is.EqualTo(137));
                AssertBirth(entity.Runtime, 137);
            }
            finally { Shutdown(world); }
        }

        [Test]
        public void ResultsReserveBirthUsesItsFinalHp()
        {
            var world = CreateWorld();
            try
            {
                Assert.That(world.TrySpawnResultsReserveEntry(Oid, 0, 137, 20), Is.True);
                var entity = world.FindEntityByRuntimeSlotForQuery(20);
                Assert.That(entity, Is.Not.Null);
                Assert.That(entity.Runtime.HP, Is.EqualTo(137));
                AssertBirth(entity.Runtime, 137);
            }
            finally { Shutdown(world); }
        }

        [Test]
        public void NewShellSnapshotRestorePreservesAllEightDisplayMarkers()
        {
            var world = CreateWorld();
            try
            {
                var original = Spawn(world, 20);
                SetMarkers(original.Runtime);
                int[] expected = Capture(original.Runtime);
                var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
                var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
                Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
                snapshot.RuntimeSlots.ClearLocalEntityShellsForTransfer();
                original.FreeEntityLikeExe();
                Assert.That(world.FindEntityByRuntimeSlotForQuery(20), Is.Null,
                    "Restore must reconstruct an absent shell; a pooled CLR instance may legitimately be reused.");
                Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out var failure), Is.True, failure.ToString());
                var restored = world.FindEntityByRuntimeSlotForQuery(20);
                Assert.That(restored, Is.Not.Null);
                Assert.That(Capture(restored.Runtime), Is.EqualTo(expected));
            }
            finally { Shutdown(world); }
        }

        internal static void VerifyRendererBirthForPlay(bool stage)
        {
            var world = CreateWorld(stage ? 3 : 0, true);
            try
            {
                LF2Entity entity;
                if (stage)
                {
                    var spawn = new BattleStageSpawnValue(id: Oid, act: 0, hp: 137,
                        times: 1, x: 300, y: 0, ratio: 0.0, join: 0);
                    var module = typeof(SimulationWorld).GetField("stageWaveModule",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(world);
                    entity = (LF2Entity)module.GetType().GetMethod("TrySpawnStageEntityWithFactory",
                        BindingFlags.Instance | BindingFlags.NonPublic).Invoke(module,
                            new object[] { spawn, 300, 0, 250, "right", 137, 20 });
                }
                else
                {
                    entity = LF2ObjectPointFactory.Instance.CreateObjectImmediate(new OPointCreateTask
                    {
                        targetWorld = world, requiredRuntimeSlot = 20, preserveActionZero = true,
                        useDirectRuntimePosition = true, skipPostInitZOffset = true, dir = "right",
                        opoint = new ObjectPoint { oid = Oid, kind = 0, action = 0 }
                    });
                    Assert.That(entity, Is.Not.Null);
                    ((LF2Character)entity).Initialize(137, 500);
                }
                Assert.That(entity, Is.Not.Null);
                Assert.That(entity.Renderer, Is.Not.Null);
                Assert.That(world.FindEntityByRuntimeSlotForQuery(20), Is.SameAs(entity));
                AssertBirth(entity.Runtime, 137);
            }
            finally
            {
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    world.FindEntityByRuntimeSlotIncludingPending(slot)?.FreeEntityLikeExe();
                Shutdown(world);
            }
        }

        private static SimulationWorld CreateWorld(int type = 0, bool renderer = false)
        {
            var data = new LF2CharacterData
            {
                name = "OrdinaryStageDisplayBirth",
                type_sub = type,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData { frameId = 0, state = 0, wait = 1000, next = 0 }
                }
            };
            var wrapper = new LF2CharacterDataWrapper(Oid, data);
            var world = new SimulationWorld(new RuntimeCharacterConfigResolver(id => id == Oid ? wrapper : null));
            world.SetLogicOnlyEntityMaterialization(!renderer);
            world.PrepareRuntimeDataCatalogForBattle(new[] { new ObjectDefinition(Oid, type, "display-birth-fixture.dat") },
                id => id == Oid ? wrapper : null);
            world.Runtime.Stage.SetSceneSnapshot(800, 180, 350, 0, 0);
            return world;
        }

        private static LF2Entity Spawn(SimulationWorld world, int slot)
        {
            var task = new OPointCreateTask
            {
                targetWorld = world, requiredRuntimeSlot = slot, preserveActionZero = true,
                useDirectRuntimePosition = true, skipPostInitZOffset = true, dir = "right",
                opoint = new ObjectPoint { oid = Oid, kind = 0, action = 0 }
            };
            var entity = world.LogicEntityFactory.Create(task, out var failure);
            Assert.That(entity, Is.Not.Null, failure.ToString());
            return entity;
        }

        private static void SetMarkers(NTSDEntityRuntime runtime)
        {
            runtime.DisplayScore1F0 = 101;
            runtime.DisplayScoreStep1F4 = 7;
            runtime.DisplayDamageTotal1F8 = 103;
            runtime.DisplayDamageStep1FC = 9;
            runtime.DisplayCurrentHp200 = 105;
            runtime.DisplayCurrentHpStep204 = 11;
            runtime.DisplayEffectiveMaxHp208 = 107;
            runtime.DisplayEffectiveMaxHpStep20C = 13;
        }

        private static int[] Capture(NTSDEntityRuntime runtime) => new[]
        {
            runtime.DisplayScore1F0, runtime.DisplayScoreStep1F4,
            runtime.DisplayDamageTotal1F8, runtime.DisplayDamageStep1FC,
            runtime.DisplayCurrentHp200, runtime.DisplayCurrentHpStep204,
            runtime.DisplayEffectiveMaxHp208, runtime.DisplayEffectiveMaxHpStep20C
        };

        private static void AssertBirth(NTSDEntityRuntime runtime, int hp)
        {
            Assert.That(Capture(runtime), Is.EqualTo(new[] { 0, 0, 0, 0, hp, 0, hp, 0 }),
                "Native spawn_at sets both HP displays from final request.hp; cumulative displays and all four steps start at zero.");
        }

        private static void Shutdown(SimulationWorld world)
        {
            NTSD28Q06State18SpawnEditorTests.Shutdown(world);
            Assert.That(world.LogicReferencePool.ActiveCount, Is.Zero);
        }
    }
    [InitializeOnLoad]
    internal static class NTSD28Q06OrdinaryStageDisplayBirthPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_DisplayBirthPlay.request";
        private const string Result = "Temp/NTSD28_Q06_DisplayBirthPlay.result.json";

        static NTSD28Q06OrdinaryStageDisplayBirthPlayProbe()
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
            foreach (bool stage in new[] { false, true })
            {
                try
                {
                    NTSD28Q06OrdinaryStageDisplayBirthEditorTests.VerifyRendererBirthForPlay(stage);
                    passed++;
                }
                catch (Exception error)
                {
                    errors.Add("stage=" + stage + ": " + error);
                }
            }
            bool unchanged = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input) == checksum;
            int after = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status = errors.Count == 0 && passed == 2 && unchanged && borrowers == after ? "PASS" : "FAIL",
                passedCases = passed, errors, sceneChecksumUnchanged = unchanged,
                rendererBorrowersBefore = borrowers, rendererBorrowersAfter = after,
                scope = "Real Play pooled-renderer ordinary/Stage type3 finalHP137 display birth. No full Stage or formal image parity claim."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
            File.WriteAllText("Temp/NTSD28_Q05_ReplayPlay.request", "run");
        }
    }
}
#endif
