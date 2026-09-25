#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_B0")]
    public sealed class NTSD28B0F8Owner99ProductionEditorTests
    {
        private const int WeaponOid = 150;
        private const int FlyFrame = 3;
        private const int F8OwnerSlot = 99;
        private const int StageWidth = 800;
        private const int StageZMin = 180;
        private const int StageZMax = 350;

        [Test]
        public void Mode2Tail_LowestFreeSlotPublishesOwner99WithoutChangingRngFrameOrPosition()
        {
            const int expectedSlot = 50;
            const int seed = 0x1234;
            SimulationWorld world = CreateWorld();

            AssertMode2Spawn(world, expectedSlot, seed);
        }

        [Test]
        public void Mode2Tail_OccupiedPrefixPublishesOwner99InHighSlot()
        {
            const int expectedSlot = 399;
            const int seed = 0x5678;
            SimulationWorld world = CreateWorld();
            for (int slot = 50; slot < expectedSlot; slot++)
            {
                var blocker = new LF2Character();
                blocker.SetRequiredRuntimeSlot(slot);
                world.Register(blocker);
                Assert.That(blocker.Runtime.SlotIndex, Is.EqualTo(slot));
            }

            AssertMode2Spawn(world, expectedSlot, seed);
        }

        [Test]
        public void NormalDrop_PreservesOwnerSentinelAndExistingRngPositionContract()
        {
            const int expectedSlot = 50;
            int seed = FindSeedForFirstRemainder(200, 0);
            SimulationWorld world = CreateWorld();
            var expectedRng = new DeterministicRng(seed);
            Assert.That(expectedRng.NextInt(0, 200), Is.Zero);
            Assert.That(expectedRng.NextInt(0, 1), Is.Zero);
            Vector3 expectedPosition = NextExpectedPosition(expectedRng);

            world.Rng.Seed(seed);
            world.RandomWeaponDropTickAll(1);

            LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(expectedSlot);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.ObjectId, Is.EqualTo(WeaponOid));
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(expectedSlot));
            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.X, Is.EqualTo(expectedPosition.x));
            Assert.That(entity.Runtime.Y, Is.EqualTo(expectedPosition.y));
            Assert.That(entity.Runtime.Z, Is.EqualTo(expectedPosition.z));
            Assert.That(entity.Runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(entity.Runtime.SourceRuleX, Is.EqualTo(expectedPosition.x));
            Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(expectedPosition.z));
            Assert.That(entity.Runtime.SourceRuleXInt, Is.EqualTo((int)expectedPosition.x));
            Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo((int)expectedPosition.z));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(-1));
            Assert.That(world.Rng.CallCount, Is.EqualTo(6UL));
            AssertActiveSlotOwnerSnapshot(world, expectedSlot, entity, -1);
        }

        private static void AssertMode2Spawn(
            SimulationWorld world,
            int expectedSlot,
            int seed)
        {
            var expectedRng = new DeterministicRng(seed);
            Vector3 expectedPosition = NextExpectedPosition(expectedRng);
            expectedPosition.z += 1f;

            world.Rng.Seed(seed);
            world.SetMode2Request(1);
            world.Mode2RandomWeaponDropTailAll(1);

            LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(expectedSlot);
            Assert.That(entity, Is.Not.Null);
            Assert.That(entity.ObjectId, Is.EqualTo(WeaponOid));
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(expectedSlot));
            Assert.That(entity.Frame.N, Is.EqualTo(FlyFrame));
            Assert.That(entity.OwnerEntityIndex, Is.EqualTo(F8OwnerSlot));
            Assert.That(entity.Runtime.X, Is.EqualTo(expectedPosition.x));
            Assert.That(entity.Runtime.Y, Is.EqualTo(expectedPosition.y));
            Assert.That(entity.Runtime.Z, Is.EqualTo(expectedPosition.z));
            Assert.That(entity.Runtime.SourceRulePositionInitialized, Is.True);
            Assert.That(entity.Runtime.SourceRuleX, Is.EqualTo(expectedPosition.x));
            Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(expectedPosition.z));
            Assert.That(entity.Runtime.SourceRuleXInt, Is.EqualTo((int)expectedPosition.x));
            Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo((int)expectedPosition.z));
            Assert.That(world.Rng.CallCount, Is.EqualTo(4UL));
            AssertActiveSlotOwnerSnapshot(
                world,
                expectedSlot,
                entity,
                F8OwnerSlot);
        }

        private static SimulationWorld CreateWorld()
        {
            LF2CharacterDataWrapper wrapper = CreateWeaponWrapper();
            var resolver = new RuntimeCharacterConfigResolver(
                oid => oid == WeaponOid ? wrapper : null);
            var world = new SimulationWorld(resolver);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        WeaponOid,
                        (int)LF2ObjectType.LightWeapon,
                        "b0-f8-owner99.dat"),
                },
                oid => oid == WeaponOid ? wrapper : null);
            world.Runtime.Stage.SetSceneSnapshot(
                StageWidth,
                StageZMin,
                StageZMax,
                0,
                0);
            return world;
        }

        private static LF2CharacterDataWrapper CreateWeaponWrapper()
        {
            var data = new LF2CharacterData
            {
                name = "B0F8Owner99Weapon",
                type_sub = (int)LF2ObjectType.LightWeapon,
                frames = new List<LF2FrameData>
                {
                    new LF2FrameData
                    {
                        frameId = 0,
                        state = LF2States.WeaponOnGround,
                        wait = 10000,
                        next = 0,
                        pic = 999,
                        centerx = 39,
                        centery = 79,
                    },
                    new LF2FrameData
                    {
                        frameId = FlyFrame,
                        state = LF2States.WeaponInSky,
                        wait = 10000,
                        next = FlyFrame,
                        pic = 999,
                        centerx = 39,
                        centery = 79,
                    },
                },
            };
            return new LF2CharacterDataWrapper(WeaponOid, data);
        }

        private static Vector3 NextExpectedPosition(DeterministicRng rng)
        {
            int r1 = rng.NextInt(0, 30);
            int r2 = rng.NextInt(0, 30);
            int r3 = rng.NextInt(0, 30);
            int r4 = rng.NextInt(0, 30);
            int x = r1 * ((StageWidth - 60) / 30) + r2 + 30;
            int z = r3 * ((StageZMax - StageZMin - 60) / 30) +
                    r4 + StageZMin + 30;
            return new Vector3(x, -500f, z);
        }

        private static int FindSeedForFirstRemainder(
            int modulus,
            int expectedRemainder)
        {
            for (int seed = 0; seed < 100000; seed++)
            {
                var rng = new DeterministicRng(seed);
                if (rng.NextInt(0, modulus) == expectedRemainder)
                    return seed;
            }

            Assert.Fail("Could not find deterministic gate seed.");
            return 0;
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
            Assert.That(view.RawRuntime.OwnerSlotIndex, Is.EqualTo(-1));
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B0F8Owner99RequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-F8-Owner99-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-F8-Owner99-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B0F8Owner99ProductionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B0F8Owner99RequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B0F8Owner99RequestRunner()
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

            activeCallbacks = new NTSD28B0F8Owner99RequestRunner();
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
