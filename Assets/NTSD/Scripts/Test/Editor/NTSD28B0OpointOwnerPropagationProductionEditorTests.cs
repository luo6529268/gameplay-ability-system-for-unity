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
    public sealed class NTSD28B0OpointOwnerPropagationProductionEditorTests
    {
        private const int ParentSlot = 50;

        [TestCase(-1)]
        [TestCase(7)]
        [TestCase(ParentSlot)]
        public void SingleKind1_CharacterChildInheritsParentLiteralOwner(
            int parentOwner)
        {
            const int childOid = 9101;
            SimulationWorld world = CreateWorld(
                Child(childOid, LF2ObjectType.Character));
            ProbeOther parent = CreateParent(
                childOid,
                kind: 1,
                facing: 0,
                parentOwner);
            RegisterParent(world, parent);

            world.LateEntityUpdateAll(1);

            LF2Entity child = world.FindEntityByRuntimeSlotForQuery(51);
            Assert.That(child, Is.TypeOf<LF2Character>());
            Assert.That(child.ObjectId, Is.EqualTo(childOid));
            Assert.That(child.OwnerEntityIndex, Is.EqualTo(parentOwner));
            if (parentOwner != ParentSlot)
                Assert.That(child.OwnerEntityIndex, Is.Not.EqualTo(ParentSlot));
            AssertActiveSlotOwnerSnapshot(world, 51, child, parentOwner);
        }

        [Test]
        public void Kind2_OtherChildSeparatesLiteralOwnerFromHolderPhysicalSlot()
        {
            const int childOid = 9102;
            const int parentOwner = 7;
            SimulationWorld world = CreateWorld(
                Child(childOid, LF2ObjectType.Other));
            ProbeOther parent = CreateParent(
                childOid,
                kind: 2,
                facing: 0,
                parentOwner);
            RegisterParent(world, parent);

            world.LateEntityUpdateAll(2);

            LF2Entity child = world.FindEntityByRuntimeSlotForQuery(51);
            Assert.That(child, Is.TypeOf<LF2OtherObject>());
            Assert.That(child.OwnerEntityIndex, Is.EqualTo(parentOwner));
            Assert.That(child.Runtime.HolderStableId, Is.EqualTo(ParentSlot));
            Assert.That(child.OwnerEntityIndex, Is.Not.EqualTo(child.Runtime.HolderStableId));
            AssertActiveSlotOwnerSnapshot(world, 51, child, parentOwner);
        }

        [Test]
        public void MultiSpawn_AllChildrenInheritOneLiteralOwner()
        {
            const int childOid = 9103;
            const int parentOwner = 11;
            SimulationWorld world = CreateWorld(
                Child(childOid, LF2ObjectType.LightWeapon));
            ProbeOther parent = CreateParent(
                childOid,
                kind: 1,
                facing: 21,
                parentOwner);
            RegisterParent(world, parent);

            world.LateEntityUpdateAll(3);

            for (int slot = 51; slot <= 52; slot++)
            {
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(slot);
                Assert.That(child, Is.InstanceOf<LF2WeaponBase>());
                Assert.That(child.OwnerEntityIndex, Is.EqualTo(parentOwner));
                AssertActiveSlotOwnerSnapshot(world, slot, child, parentOwner);
            }
        }

        [Test]
        public void TwoHop_NewbornProducerKeepsRootLiteralOwnerAcrossPhysicalSlots()
        {
            const int firstOid = 9104;
            const int secondOid = 9105;
            const int rootOwner = 7;
            LF2FrameData firstChildFrame = FrameWithOpoint(
                secondOid,
                kind: 1,
                facing: 0);
            SimulationWorld world = CreateWorld(
                Child(firstOid, LF2ObjectType.Character, firstChildFrame),
                Child(secondOid, LF2ObjectType.SpecialAttack));
            ProbeOther root = CreateParent(
                firstOid,
                kind: 1,
                facing: 0,
                rootOwner);
            RegisterParent(world, root);

            world.LateEntityUpdateAll(4);

            LF2Entity first = world.FindEntityByRuntimeSlotForQuery(51);
            Assert.That(first?.ObjectId, Is.EqualTo(firstOid));
            first.AttackingCounter = 0;
            first.FrameDelay = 0;
            first.Frame.D = firstChildFrame;
            first.Frame.N = 0;
            first.Runtime.Frame = 0;

            world.StructuralWriter.ProcessLateOpointSegment(
                world.LogicObjectPointRuntime,
                first,
                5);

            LF2Entity second = world.FindEntityByRuntimeSlotForQuery(52);
            Assert.That(second?.ObjectId, Is.EqualTo(secondOid));
            Assert.That(first.OwnerEntityIndex, Is.EqualTo(rootOwner));
            Assert.That(second.OwnerEntityIndex, Is.EqualTo(rootOwner));
            Assert.That(second.OwnerEntityIndex, Is.Not.EqualTo(first.Runtime.SlotIndex));
            AssertActiveSlotOwnerSnapshot(world, 51, first, rootOwner);
            AssertActiveSlotOwnerSnapshot(world, 52, second, rootOwner);
        }

        [Test]
        public void BuiltInOid999Fragments_KeepNullParentOwnerSentinel()
        {
            const int fragmentOid = 999;
            SimulationWorld world = CreateWorld(
                Child(fragmentOid, LF2ObjectType.Other));
            var parent = new ProbeOther(
                9000,
                LF2ObjectType.Other,
                FrameWithoutOpoint());
            parent.OwnerEntityIndex = 7;
            RegisterParent(world, parent);

            parent.SpawnBuiltInFragments(100);
            world.FlushQueuedObjectPointTasks();

            for (int slot = 51; slot <= 55; slot++)
            {
                LF2Entity fragment = world.FindEntityByRuntimeSlotForQuery(slot);
                Assert.That(fragment?.ObjectId, Is.EqualTo(fragmentOid));
                Assert.That(fragment.OwnerEntityIndex, Is.EqualTo(-1));
                AssertActiveSlotOwnerSnapshot(world, slot, fragment, -1);
            }
        }

        private static ProbeOther CreateParent(
            int childOid,
            int kind,
            int facing,
            int owner)
        {
            var parent = new ProbeOther(
                9000,
                LF2ObjectType.Other,
                FrameWithOpoint(childOid, kind, facing));
            parent.OwnerEntityIndex = owner;
            return parent;
        }

        private static void RegisterParent(
            SimulationWorld world,
            ProbeOther parent)
        {
            parent.SetRequiredRuntimeSlot(ParentSlot);
            world.Register(parent);
            Assert.That(parent.Runtime.SlotIndex, Is.EqualTo(ParentSlot));
        }

        private static ChildDefinition Child(
            int oid,
            LF2ObjectType type,
            LF2FrameData frame = null)
        {
            return new ChildDefinition(
                oid,
                type,
                frame ?? FrameWithoutOpoint());
        }

        private static SimulationWorld CreateWorld(
            params ChildDefinition[] children)
        {
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            var definitions = new List<ObjectDefinition>(children.Length);
            for (int index = 0; index < children.Length; index++)
            {
                ChildDefinition child = children[index];
                var data = new LF2CharacterData
                {
                    name = $"B0OpointOwnerChild{child.Oid}",
                    type_sub = (int)child.Type,
                    frames = new List<LF2FrameData> { child.Frame },
                };
                wrappers.Add(
                    child.Oid,
                    new LF2CharacterDataWrapper(child.Oid, data));
                definitions.Add(new ObjectDefinition(
                    child.Oid,
                    (int)child.Type,
                    "b0-opoint-owner.dat"));
            }

            var resolver = new RuntimeCharacterConfigResolver(
                oid => wrappers.TryGetValue(
                    oid,
                    out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            var world = new SimulationWorld(resolver);
            world.SetLogicOnlyEntityMaterialization(true);
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(
                    oid,
                    out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            return world;
        }

        private static LF2FrameData FrameWithOpoint(
            int childOid,
            int kind,
            int facing)
        {
            LF2FrameData frame = FrameWithoutOpoint();
            frame.opoint = new ObjectPoint
            {
                oid = childOid,
                kind = kind,
                action = 0,
                x = 10,
                y = 20,
                dvx = 3,
                dvy = -2,
                facing = facing,
            };
            return frame;
        }

        private static LF2FrameData FrameWithoutOpoint()
        {
            return new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 10000,
                next = 0,
                pic = 999,
                centerx = 39,
                centery = 79,
            };
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

        private sealed class ProbeOther : LF2Entity
        {
            private readonly LF2ObjectType dataType;

            public override LF2ObjectType ObjectTypeEnum => dataType;

            internal override bool UsesDynamicRuntimeSlot() => true;

            public ProbeOther(
                int objectId,
                LF2ObjectType dataType,
                LF2FrameData frame)
            {
                this.dataType = dataType;
                Name = $"B0OpointOwnerParent{objectId}";
                ObjectId = objectId;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 500;
                Health.HPBound = 500;
                Health.HP3 = 500;
                Health.PP = 500;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                FrameCache.Load(new LF2CharacterDataWrapper(
                    objectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)dataType,
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.N = 0;
                Frame.PN = 0;
                Frame.Prev = 0;
                Frame.Prev2 = 0;
                Runtime.Frame = 0;
                Runtime.PrevFrame2 = 0;
                Runtime.WeaponFlightCounter = 0;
                Runtime.SetPosition(100, 0, 250);
                Runtime.SyncIntegerPosition();
                PS.dir = "right";
                AttackingCounter = 0;
                FrameDelay = 0;
                HitStun = 0;
            }

            public void SpawnBuiltInFragments(int sourceOid)
            {
                SpawnBrokenWeaponFragments(sourceOid);
            }

            internal override void RunLateTailBeforePrevFrame()
            {
                AttackingCounter = 1;
            }

            public override void Reset()
            {
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }

        private sealed class ChildDefinition
        {
            public ChildDefinition(
                int oid,
                LF2ObjectType type,
                LF2FrameData frame)
            {
                Oid = oid;
                Type = type;
                Frame = frame;
            }

            public int Oid { get; }
            public LF2ObjectType Type { get; }
            public LF2FrameData Frame { get; }
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B0OpointOwnerPropagationRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B0-OpointOwnerPropagation-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B0-OpointOwnerPropagation-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B0OpointOwnerPropagationProductionEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B0OpointOwnerPropagationRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B0OpointOwnerPropagationRequestRunner()
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

            activeCallbacks = new NTSD28B0OpointOwnerPropagationRequestRunner();
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
