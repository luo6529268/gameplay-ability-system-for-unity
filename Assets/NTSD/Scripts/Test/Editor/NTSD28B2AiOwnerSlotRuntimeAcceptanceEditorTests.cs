#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    [Category("NTSD28_B2")]
    public sealed class NTSD28B2AiOwnerSlotRuntimeAcceptanceEditorTests
    {
        private const BindingFlags InstanceMembers =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string GuardCaptureSchema = "ntsd28-b2-ai-owner-guard-v1";
        private const string FormalAuthorityExeSha256 =
            "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
        private const string GuardScenarioId = "b2-ai-owner-slot-guard";
        private const string GuardPrecondition = "hp100/base500/mp100/mode0";
        private const string AuthorityGuardRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlot/authority-ai-owner.json";
        private const string UnityGuardRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlot/unity-ai-owner.json";
        private const string GuardComparisonRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlot/comparison.json";

        [TestCase(122)]
        [TestCase(123)]
        public void ProductionSnapshot_OwnerAloneDrivesSpecialGuard(
            int specialObjectId)
        {
            SimulationWorld world = CreateGuardWorld(
                specialObjectId,
                out LF2Character self);

            AssertGuardResult(
                world,
                self,
                specialObjectId,
                ownerSlot: 0,
                killCount: -1,
                expectedSelectedSlot: 1,
                expectedGuard: true);
            AssertGuardResult(
                world,
                self,
                specialObjectId,
                ownerSlot: -1,
                killCount: 999,
                expectedSelectedSlot: 20,
                expectedGuard: false);
            AssertGuardResult(
                world,
                self,
                specialObjectId,
                ownerSlot: -1,
                killCount: -1,
                expectedSelectedSlot: 20,
                expectedGuard: false);
            AssertGuardResult(
                world,
                self,
                specialObjectId,
                ownerSlot: 37,
                killCount: 999,
                expectedSelectedSlot: 1,
                expectedGuard: true);
        }

        [Test]
        public void AiOwnerGuardTrace_MatchesAuthoritySourceModel()
        {
            string authorityPath = ProjectPath(AuthorityGuardRelativePath);
            Assert.That(File.Exists(authorityPath), Is.True,
                $"Authority AI owner trace is missing: {authorityPath}");
            GuardTraceEnvelope authority = JsonUtility.FromJson<GuardTraceEnvelope>(
                File.ReadAllText(authorityPath, Encoding.UTF8));
            ValidateGuardHeader(authority);

            var records = new List<GuardTraceRecord>(8);
            foreach (int objectId in new[] { 122, 123 })
            {
                SimulationWorld world = CreateGuardWorld(objectId, out LF2Character self);
                records.Add(CaptureGuardResult(world, self, objectId, -1, -1));
                records.Add(CaptureGuardResult(world, self, objectId, -1, 999));
                records.Add(CaptureGuardResult(world, self, objectId, 0, -1));
                records.Add(CaptureGuardResult(world, self, objectId, 37, 999));
            }
            var unity = new GuardTraceEnvelope
            {
                schema = GuardCaptureSchema,
                producer = "UNITY_PRODUCTION_AI_SNAPSHOT",
                evidenceClass = "UNITY_RUNTIME_DIAGNOSTIC_ONLY",
                formalAuthorityExeSha256 = FormalAuthorityExeSha256,
                scenarioId = GuardScenarioId,
                seed = 0x1234,
                precondition = GuardPrecondition,
                records = records.ToArray(),
            };
            string unityPath = ProjectPath(UnityGuardRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(unityPath));
            File.WriteAllText(
                unityPath,
                JsonUtility.ToJson(unity, true),
                new UTF8Encoding(false));

            GuardTraceComparison comparison = CompareGuardTrace(authority, unity);
            File.WriteAllText(
                ProjectPath(GuardComparisonRelativePath),
                JsonUtility.ToJson(comparison, true),
                new UTF8Encoding(false));
            Assert.That(comparison.status, Is.EqualTo("equal-ai-owner-guard"),
                comparison.firstDifference);
            Assert.That(comparison.recordsCompared, Is.EqualTo(8));
            Assert.That(comparison.fieldOccurrencesCompared, Is.EqualTo(48));
        }

        [Test]
        public void ProductionSnapshot_ConsumesClosedB0ProducerValues()
        {
            SimulationWorld directWorld = new SimulationWorld();
            LF2Character direct = CreateCharacter(9500, LF2States.Standing, 500);
            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(direct, 0),
                Is.True);
            direct.KillCount = 401;
            directWorld.Register(direct);
            AssertSnapshotOwner(directWorld, 0, 0);

            const int childOid = 9501;
            LF2CharacterDataWrapper childWrapper = new LF2CharacterDataWrapper(
                childOid,
                Data(
                    "B2OwnerOpointChild",
                    LF2ObjectType.Character,
                    Frame(0, LF2States.Standing)));
            var resolver = new RuntimeCharacterConfigResolver(
                oid => oid == childOid ? childWrapper : null);
            var opointWorld = new SimulationWorld(resolver);
            opointWorld.SetLogicOnlyEntityMaterialization(true);
            opointWorld.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        childOid,
                        (int)LF2ObjectType.Character,
                        "b2-owner-runtime.dat"),
                },
                oid => oid == childOid ? childWrapper : null);
            var root = new ProducerProbeOther(
                9502,
                FrameWithOpoint(childOid));
            root.SetRequiredRuntimeSlot(50);
            root.OwnerEntityIndex = 7;
            opointWorld.Register(root);
            opointWorld.StructuralWriter.ProcessLateOpointSegment(
                opointWorld.LogicObjectPointRuntime,
                root,
                1);
            LF2Entity child = opointWorld.FindEntityByRuntimeSlotForQuery(51);
            Assert.That(child, Is.Not.Null);
            child.KillCount = -300;
            AssertSnapshotOwner(opointWorld, 51, 7);

            const int weaponOid = 150;
            LF2CharacterDataWrapper weaponWrapper = new LF2CharacterDataWrapper(
                weaponOid,
                Data(
                    "B2OwnerF8Weapon",
                    LF2ObjectType.LightWeapon,
                    Frame(0, LF2States.WeaponOnGround),
                    Frame(3, LF2States.WeaponInSky)));
            var f8Resolver = new RuntimeCharacterConfigResolver(
                oid => oid == weaponOid ? weaponWrapper : null);
            var f8World = new SimulationWorld(f8Resolver);
            f8World.SetLogicOnlyEntityMaterialization(true);
            f8World.PrepareRuntimeDataCatalogForBattle(
                new[]
                {
                    new ObjectDefinition(
                        weaponOid,
                        (int)LF2ObjectType.LightWeapon,
                        "b2-owner-f8.dat"),
                },
                oid => oid == weaponOid ? weaponWrapper : null);
            f8World.Runtime.Stage.SetSceneSnapshot(800, 180, 350, 0, 0);
            f8World.Rng.Seed(0x1234);
            f8World.SetMode2Request(1);
            f8World.Mode2RandomWeaponDropTailAll(1);
            LF2Entity f8Entity = f8World.FindEntityByRuntimeSlotForQuery(50);
            Assert.That(f8Entity, Is.Not.Null);
            f8Entity.KillCount = 777;
            AssertSnapshotOwner(f8World, 50, 99);
        }

        private static void AssertGuardResult(
            SimulationWorld world,
            LF2Character self,
            int objectId,
            int ownerSlot,
            int killCount,
            int expectedSelectedSlot,
            bool expectedGuard)
        {
            GuardTraceRecord result = CaptureGuardResult(
                world,
                self,
                objectId,
                ownerSlot,
                killCount);
            Assert.That(result.selectedSlot, Is.EqualTo(expectedSelectedSlot));
            Assert.That(result.guard7A, Is.EqualTo(expectedGuard));
            Assert.That(result.guard7B, Is.EqualTo(expectedGuard));
        }

        private static GuardTraceRecord CaptureGuardResult(
            SimulationWorld world,
            LF2Character self,
            int objectId,
            int ownerSlot,
            int killCount)
        {
            self.Runtime.OwnerSlotIndex = ownerSlot;
            self.Runtime.KillCount = killCount;
            world.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            world.BuildAiInputSlotSnapshot();
            try
            {
                AiSensingSnapshot rows = GetSensingRows(world);
                Assert.That(rows.OwnerSlot[0], Is.EqualTo(ownerSlot));
                Assert.That(AiSensingKernel.TryScanSpecial(
                        rows,
                        0,
                        2,
                        1,
                        100,
                        false,
                        true,
                        out AiSensingSpecialResult result),
                    Is.True);
                return new GuardTraceRecord
                {
                    objectId = objectId,
                    ownerSlot = ownerSlot,
                    legacyKillCountMarker = killCount,
                    selectedSlot = result.SelectedSlot,
                    guard7A =
                        (result.Flags & AiSensingKernel.SpecialGuard7A) != 0,
                    guard7B =
                        (result.Flags & AiSensingKernel.SpecialGuard7B) != 0,
                };
            }
            finally
            {
                world.ClearAiInputSlotSnapshot();
            }
        }

        private static void AssertSnapshotOwner(
            SimulationWorld world,
            int slot,
            int expectedOwner)
        {
            world.AiSensingMode = AiSensingMode.SoAShadowAiSensing;
            world.BuildAiInputSlotSnapshot();
            try
            {
                AiSensingSnapshot rows = GetSensingRows(world);
                Assert.That(rows.Included[slot], Is.True);
                Assert.That(rows.OwnerSlot[slot], Is.EqualTo(expectedOwner));
            }
            finally
            {
                world.ClearAiInputSlotSnapshot();
            }
        }

        private static SimulationWorld CreateGuardWorld(
            int specialObjectId,
            out LF2Character self)
        {
            var world = new SimulationWorld();
            self = CreateCharacter(9510, LF2States.Standing, 500);
            LF2Character target = CreateCharacter(9511, LF2States.Standing, 500);
            LF2OtherObject special = CreateOther(
                specialObjectId,
                1004,
                1);
            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(self, 0),
                Is.True);
            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(target, 1),
                Is.True);
            special.SetRequiredRuntimeSlot(20);
            self.Team = self.RelationTeam = 1;
            target.Team = target.RelationTeam = 2;
            special.Team = special.RelationTeam = 1;
            self.Health.HP = 100;
            self.Health.HPBound = 500;
            self.Health.HP3 = 500;
            self.Runtime.SetPosition(0, 0, 0);
            target.Runtime.SetPosition(80, 0, 0);
            special.Runtime.SetPosition(10, 0, 0);
            self.Runtime.SyncIntegerPosition();
            target.Runtime.SyncIntegerPosition();
            special.Runtime.SyncIntegerPosition();
            world.Register(self);
            world.Register(target);
            world.Register(special);
            Assert.That(special.Runtime.SlotIndex, Is.EqualTo(20));
            return world;
        }

        private static AiSensingSnapshot GetSensingRows(SimulationWorld world)
        {
            FieldInfo runtimeField = typeof(SimulationWorld).GetField(
                "aiRuntime",
                InstanceMembers);
            Assert.That(runtimeField, Is.Not.Null);
            object runtime = runtimeField.GetValue(world);
            PropertyInfo sensingProperty = runtime.GetType().GetProperty(
                "Sensing",
                InstanceMembers);
            Assert.That(sensingProperty, Is.Not.Null);
            object sensing = sensingProperty.GetValue(runtime);
            PropertyInfo rowsProperty = sensing.GetType().GetProperty(
                "Rows",
                InstanceMembers);
            Assert.That(rowsProperty, Is.Not.Null);
            return (AiSensingSnapshot)rowsProperty.GetValue(sensing);
        }

        private static void ValidateGuardHeader(GuardTraceEnvelope authority)
        {
            Assert.That(authority, Is.Not.Null);
            Assert.That(authority.schema, Is.EqualTo(GuardCaptureSchema));
            Assert.That(authority.producer, Is.EqualTo("NTSD28_PLAYABLE_SOURCE_MODEL"));
            Assert.That(authority.evidenceClass,
                Is.EqualTo("SOURCE_MODEL_DIAGNOSTIC_ONLY"));
            Assert.That(authority.formalAuthorityExeSha256,
                Is.EqualTo(FormalAuthorityExeSha256));
            Assert.That(authority.authoritySourceManifestSha256,
                Does.Match("^[0-9A-F]{64}$"));
            Assert.That(authority.captureRunnerSourceSha256,
                Does.Match("^[0-9A-F]{64}$"));
            Assert.That(authority.captureBinarySha256,
                Does.Match("^[0-9A-F]{64}$"));
            Assert.That(authority.scenarioId, Is.EqualTo(GuardScenarioId));
            Assert.That(authority.seed, Is.EqualTo(0x1234));
            Assert.That(authority.precondition, Is.EqualTo(GuardPrecondition));
            Assert.That(authority.records, Has.Length.EqualTo(8));
        }

        private static GuardTraceComparison CompareGuardTrace(
            GuardTraceEnvelope authority,
            GuardTraceEnvelope unity)
        {
            var comparison = new GuardTraceComparison
            {
                schema = "ntsd28-b2-ai-owner-guard-comparison-v1",
                status = "equal-ai-owner-guard",
            };
            if (unity.records == null || authority.records.Length != unity.records.Length)
            {
                comparison.status = "different";
                comparison.firstDifference = "record-count";
                return comparison;
            }
            for (int index = 0; index < authority.records.Length; index++)
            {
                GuardTraceRecord expected = authority.records[index];
                GuardTraceRecord actual = unity.records[index];
                string difference = FirstGuardDifference(expected, actual);
                if (!string.IsNullOrEmpty(difference))
                {
                    comparison.status = "different";
                    comparison.firstDifference = $"record[{index}].{difference}";
                    return comparison;
                }
                comparison.recordsCompared++;
                comparison.fieldOccurrencesCompared += 6;
            }
            return comparison;
        }

        private static string FirstGuardDifference(
            GuardTraceRecord expected,
            GuardTraceRecord actual)
        {
            if (expected.objectId != actual.objectId) return "objectId";
            if (expected.ownerSlot != actual.ownerSlot) return "ownerSlot";
            if (expected.legacyKillCountMarker != actual.legacyKillCountMarker)
                return "legacyKillCountMarker";
            if (expected.selectedSlot != actual.selectedSlot) return "selectedSlot";
            if (expected.guard7A != actual.guard7A) return "guard7A";
            if (expected.guard7B != actual.guard7B) return "guard7B";
            return string.Empty;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }

        private static LF2Character CreateCharacter(
            int objectId,
            int state,
            int hp)
        {
            LF2CharacterData data = Data(
                $"B2OwnerCharacter{objectId}",
                LF2ObjectType.Character,
                Frame(0, state));
            var entity = new LF2Character
            {
                ObjectId = objectId,
                Name = data.name,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = hp;
            entity.Health.HPBound = hp;
            entity.Health.HP3 = hp;
            entity.Health.PP = 100;
            return entity;
        }

        private static LF2OtherObject CreateOther(
            int objectId,
            int state,
            int hp)
        {
            LF2CharacterData data = Data(
                $"B2OwnerOther{objectId}",
                LF2ObjectType.LightWeapon,
                Frame(0, state));
            var entity = new LF2OtherObject
            {
                ObjectId = objectId,
                Name = data.name,
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = hp;
            entity.Health.HPBound = hp;
            entity.Health.HP3 = hp;
            entity.Health.PP = 100;
            return entity;
        }

        private static LF2CharacterData Data(
            string name,
            LF2ObjectType type,
            params LF2FrameData[] frames)
        {
            return new LF2CharacterData
            {
                name = name,
                type_sub = (int)type,
                frames = new List<LF2FrameData>(frames),
            };
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 10000,
                next = id,
                pic = 999,
                centerx = 39,
                centery = 79,
            };
        }

        private static LF2FrameData FrameWithOpoint(int childOid)
        {
            LF2FrameData frame = Frame(0, LF2States.Standing);
            frame.opoint = new ObjectPoint
            {
                oid = childOid,
                kind = 1,
                action = 0,
                x = 39,
                y = 79,
                facing = 0,
            };
            return frame;
        }

        private sealed class ProducerProbeOther : LF2Entity
        {
            public ProducerProbeOther(int objectId, LF2FrameData frame)
            {
                ObjectId = objectId;
                Name = "B2OwnerOpointRoot";
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
                    Data(Name, LF2ObjectType.Other, frame)));
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

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

            internal override bool UsesDynamicRuntimeSlot() => true;

            public override void Reset()
            {
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }

        [Serializable]
        private sealed class GuardTraceEnvelope
        {
            public string schema = string.Empty;
            public string producer = string.Empty;
            public string evidenceClass = string.Empty;
            public string formalAuthorityExeSha256 = string.Empty;
            public string authoritySourceManifestSha256 = string.Empty;
            public string captureRunnerSourceSha256 = string.Empty;
            public string captureBinarySha256 = string.Empty;
            public string scenarioId = string.Empty;
            public int seed;
            public string precondition = string.Empty;
            public GuardTraceRecord[] records = Array.Empty<GuardTraceRecord>();
        }

        [Serializable]
        private sealed class GuardTraceRecord
        {
            public int objectId;
            public int ownerSlot;
            public int legacyKillCountMarker;
            public int selectedSlot;
            public bool guard7A;
            public bool guard7B;
        }

        [Serializable]
        private sealed class GuardTraceComparison
        {
            public string schema = string.Empty;
            public string status = string.Empty;
            public int recordsCompared;
            public int fieldOccurrencesCompared;
            public string firstDifference = string.Empty;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B2AiOwnerSlotRuntimeAcceptanceRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlotRuntime-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlotRuntime-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B2AiOwnerSlotRuntimeAcceptanceEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails = new StringBuilder(2048);
        private static NTSD28B2AiOwnerSlotRuntimeAcceptanceRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B2AiOwnerSlotRuntimeAcceptanceRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode ||
                !File.Exists(RequestPath))
            {
                return;
            }
            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            File.Delete(RequestPath);
            activeCallbacks = new NTSD28B2AiOwnerSlotRuntimeAcceptanceRequestRunner();
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
            UnityEngine.Object.DestroyImmediate(activeApi);
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

    [InitializeOnLoad]
    internal static class NTSD28B2AiOwnerSlotRuntimeAcceptancePlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlotRuntime-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B2-AiOwnerSlotRuntime-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B2AiOwnerSlotRuntimeAcceptancePlayRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (running || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || !File.Exists(RequestPath))
            {
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            running = true;
            File.Delete(RequestPath);
            if (File.Exists(ResultPath))
                File.Delete(ResultPath);
            try
            {
                var tests = new NTSD28B2AiOwnerSlotRuntimeAcceptanceEditorTests();
                tests.ProductionSnapshot_OwnerAloneDrivesSpecialGuard(122);
                tests.ProductionSnapshot_OwnerAloneDrivesSpecialGuard(123);
                tests.ProductionSnapshot_ConsumesClosedB0ProducerValues();
                tests.AiOwnerGuardTrace_MatchesAuthoritySourceModel();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\ncases=4\ntraceRecords=8\ntraceFields=48\n" +
                    "producerOwners=0,7,99\nsceneMutation=none\n",
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                File.WriteAllText(
                    ResultPath,
                    "state=Failed\n" + exception,
                    new UTF8Encoding(false));
            }
            finally
            {
                EditorApplication.delayCall += ExitPlayMode;
            }
        }

        private static void ExitPlayMode()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            running = false;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }
    }
}
#endif
