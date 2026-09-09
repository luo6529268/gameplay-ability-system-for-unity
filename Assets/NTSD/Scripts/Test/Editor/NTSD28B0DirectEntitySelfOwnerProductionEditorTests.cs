#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B0")]
    public sealed class NTSD28B0DirectEntitySelfOwnerProductionEditorTests
    {
        [TestCase(0)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(19)]
        public void DirectParticipantPreparation_PublishesSelfOwnerInFirstActiveSlotSnapshot(int slot)
        {
            var world = new SimulationWorld();
            var entity = new LF2Character();

            Assert.That(
                BattleMatchConfigRuntimeAdapter.PrepareDirectParticipantRegistration(entity, slot),
                Is.True);
            Assert.That(entity.RequiredRuntimeSlot, Is.EqualTo(slot));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(slot));

            world.Register(entity);

            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(slot));
            AssertActiveSlotOwnerSnapshot(world, slot, entity, slot);
        }

        [TestCase(-1)]
        [TestCase(20)]
        [TestCase(int.MaxValue)]
        public void DirectParticipantPreparation_InvalidSlotPreservesSentinel(int slot)
        {
            var entity = new LF2Character();

            Assert.That(
                BattleMatchConfigRuntimeAdapter.PrepareDirectParticipantRegistration(entity, slot),
                Is.False);
            Assert.That(entity.RequiredRuntimeSlot, Is.EqualTo(-1));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(-1));
        }

        [Test]
        public void DirectParticipantPreparation_ResetPreservesSentinel()
        {
            var entity = new LF2Character();

            Assert.That(
                BattleMatchConfigRuntimeAdapter.PrepareDirectParticipantRegistration(entity, 19),
                Is.True);
            entity.Reset();
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(-1));
        }

        [Test]
        public void DirectParticipantRegistrationConflict_CallerCleanupReturnsOwnerToSentinel()
        {
            const int slot = 19;
            var world = new SimulationWorld();
            var occupied = new LF2Character();
            occupied.SetRequiredRuntimeSlot(slot);
            world.Register(occupied);
            var rejected = new LF2Character();

            Assert.That(
                BattleMatchConfigRuntimeAdapter.PrepareDirectParticipantRegistration(rejected, slot),
                Is.True);
            world.Register(rejected);
            Assert.That(rejected.Runtime.SlotIndex, Is.EqualTo(-1));

            LF2ObjectPointFactory.ReleaseRejectedSpawn(null, rejected);

            Assert.That(rejected.RequiredRuntimeSlot, Is.EqualTo(-1));
            Assert.That(rejected.OwnerEntityIndex, Is.EqualTo(-1));
            Assert.That(world.FindEntityByRuntimeSlotForQuery(slot), Is.SameAs(occupied));
        }

        [TestCase(20)]
        [TestCase(399)]
        public void StageTaskConfigurator_BindsRequiredSlotAsExplicitSelfOwner(int slot)
        {
            var configurator = new StageSpawnTaskConfigurator();
            var task = new OPointCreateTask();
            BattleStageSpawnValue spawn = StageSpawnValue(92000);

            configurator.Configure(task, spawn, 100, -20, 200, "right", slot);

            Assert.That(task.requiredRuntimeSlot, Is.EqualTo(slot));
            Assert.That(task.ownerEntityIndex, Is.EqualTo(slot));

            configurator.Configure(task, spawn, 100, -20, 200, "right");
            Assert.That(task.requiredRuntimeSlot, Is.EqualTo(-1));
            Assert.That(task.ownerEntityIndex, Is.EqualTo(-1));
        }

        [TestCase(20)]
        [TestCase(399)]
        public void LogicFactory_StageCharacterPublishesSelfOwnerInFirstActiveSlotSnapshot(int slot)
        {
            const int oid = 92001;
            LF2CharacterDataWrapper wrapper = CharacterWrapper(oid, LF2ObjectType.Character);
            SimulationWorld world = CreateLogicWorld(oid, LF2ObjectType.Character, wrapper);
            var configurator = new StageSpawnTaskConfigurator();
            OPointCreateTask task = configurator.CreateCold(
                StageSpawnValue(oid),
                100,
                -20,
                200,
                "right",
                slot);

            LF2Entity entity = world.LogicEntityFactory.Create(
                task,
                out BattleLogicEntityCreationFailure failure);

            Assert.That(failure, Is.EqualTo(BattleLogicEntityCreationFailure.None));
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(slot));
            AssertActiveSlotOwnerSnapshot(world, slot, entity, slot);
        }

        [Test]
        public void LogicFactory_ExplicitNonSelfOwnerIsVisibleBeforeNonCharacterRegistrationSnapshot()
        {
            const int oid = 92002;
            const int slot = 50;
            const int owner = 7;
            LF2CharacterDataWrapper wrapper = CharacterWrapper(oid, LF2ObjectType.Other);
            SimulationWorld world = CreateLogicWorld(oid, LF2ObjectType.Other, wrapper);
            var task = new OPointCreateTask
            {
                targetWorld = world,
                requiredRuntimeSlot = slot,
                ownerEntityIndex = owner,
                opoint = new ObjectPoint
                {
                    oid = oid,
                    kind = 0,
                    action = 0,
                },
                preserveActionZero = true,
            };

            LF2Entity entity = world.LogicEntityFactory.Create(
                task,
                out BattleLogicEntityCreationFailure failure);

            Assert.That(failure, Is.EqualTo(BattleLogicEntityCreationFailure.None));
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(slot));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(owner));
            AssertActiveSlotOwnerSnapshot(world, slot, entity, owner);
        }

        [Test]
        public void ResultsReserveEntry_PreservesSelfOwnerAfterPostInitialization()
        {
            const int oid = 92003;
            const int slot = 20;
            LF2CharacterDataWrapper wrapper = CharacterWrapper(oid, LF2ObjectType.Character);
            SimulationWorld world = CreateLogicWorld(oid, LF2ObjectType.Character, wrapper);
            world.Runtime.Stage.SetSceneSnapshot(1000, 180, 350, 0, 0);

            bool spawned = world.TrySpawnResultsReserveEntry(oid, 0, 50, slot);
            LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);

            Assert.That(spawned, Is.True);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(slot));
            AssertActiveSlotOwnerSnapshot(world, slot, entity, slot);
        }

        private static void AssertActiveSlotOwnerSnapshot(
            SimulationWorld world,
            int slot,
            LF2Entity entity,
            int expectedOwner)
        {
            RuntimeSlotTable.ReadOnlySlotView view =
                world.RuntimeSlotTableForModules.GetReadOnlyView(slot);
            Assert.That(view.Claimed, Is.True);
            Assert.That(view.Entity, Is.SameAs(entity));
            Assert.That(view.Entity.Runtime.OwnerSlotIndex, Is.EqualTo(expectedOwner));
            Assert.That(view.RawRuntime, Is.Not.Null);
            Assert.That(
                view.RawRuntime.OwnerSlotIndex,
                Is.EqualTo(-1),
                "Claimed entity state must not be mirrored into the independent raw-slot backing store.");
        }

        private static SimulationWorld CreateLogicWorld(
            int oid,
            LF2ObjectType objectType,
            LF2CharacterDataWrapper wrapper)
        {
            var resolver = new RuntimeCharacterConfigResolver(
                requestedOid => requestedOid == oid ? wrapper : null);
            var world = new SimulationWorld(resolver);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(
                new[] { new ObjectDefinition(oid, (int)objectType, "b0-self-owner.dat") },
                requestedOid => requestedOid == oid ? wrapper : null);
            return world;
        }

        private static LF2CharacterDataWrapper CharacterWrapper(
            int oid,
            LF2ObjectType objectType)
        {
            var data = new LF2CharacterData
            {
                name = $"B0SelfOwner{oid}",
                type_sub = (int)objectType,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.Standing,
                        wait = 10000,
                        next = 0,
                        pic = 999,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            return new LF2CharacterDataWrapper(oid, data);
        }

        private static BattleStageSpawnValue StageSpawnValue(int oid)
        {
            return new BattleStageSpawnValue(
                id: oid,
                act: 0,
                hp: 300,
                times: 1,
                x: 100,
                y: -20,
                ratio: 0.0,
                join: 0);
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B0DirectEntitySelfOwnerRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-DirectEntitySelfOwner-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-DirectEntitySelfOwner-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B0DirectEntitySelfOwnerProductionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B0DirectEntitySelfOwnerRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B0DirectEntitySelfOwnerRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }

            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            File.Delete(RequestPath);

            activeCallbacks = new NTSD28B0DirectEntitySelfOwnerRequestRunner();
            activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            activeApi.RegisterCallbacks(activeCallbacks);
            activeApi.Execute(new ExecutionSettings(
                new Filter
                {
                    testMode = TestMode.EditMode,
                    testNames = new[] { FocusedTestClass },
                })
            {
                runSynchronously = false,
            });
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string text =
                $"state={result.ResultState}\n" +
                $"passed={result.PassCount}\n" +
                $"failed={result.FailCount}\n" +
                $"skipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\n" +
                $"message={result.Message}\n" +
                FailureDetails;
            File.WriteAllText(ResultPath, text, new UTF8Encoding(false));

            activeApi.UnregisterCallbacks(this);
            Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.IsSuite || result.FailCount <= 0)
                return;

            FailureDetails.Append("--- failure ---\n");
            FailureDetails.Append("test=").Append(result.FullName).Append('\n');
            FailureDetails.Append("state=").Append(result.ResultState).Append('\n');
            FailureDetails.Append("message=").Append(result.Message).Append('\n');
            FailureDetails.Append("stack=").Append(result.StackTrace).Append('\n');
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
