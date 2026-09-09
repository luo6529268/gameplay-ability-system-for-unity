#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
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
    [Category("NTSD28_B4")]
    public sealed class NTSD28B4RevivalExitAuditEditorTests
    {
        private const string CaptureSchema = "ntsd28-b4-revival-exit-v1";
        private const string FormalAuthorityExeSha256 =
            "B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033";
        private const string AuthoritySourceManifestSha256 =
            "07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F";
        private const string ScenarioId = "b4-revival-production-exit";
        private const string InputContract = "neutral-zero";
        private const string ObservationUnit = "completed-revival-route";
        private const int SharedSeed = 0x1234;
        private const string AuthorityRelativePath =
            "Temp/NTSD28-B4-RevivalExit/authority-revival.json";
        private const string UnityRelativePath =
            "Temp/NTSD28-B4-RevivalExit/unity-revival.json";
        private const string ComparisonRelativePath =
            "Temp/NTSD28-B4-RevivalExit/comparison.json";

        [Test]
        public void RevivalProductionTrace_MatchesAuthorityAcrossB4ExitCases()
        {
            string authorityPath = ProjectPath(AuthorityRelativePath);
            Assert.That(File.Exists(authorityPath), Is.True,
                $"Authority revival trace is missing: {authorityPath}");

            RevivalTraceEnvelope authority =
                JsonUtility.FromJson<RevivalTraceEnvelope>(
                    File.ReadAllText(authorityPath, Encoding.UTF8));
            ValidateAuthorityHeader(authority);

            RevivalTraceEnvelope unity = CaptureUnityTrace();
            string unityPath = ProjectPath(UnityRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(unityPath));
            File.WriteAllText(
                unityPath,
                JsonUtility.ToJson(unity, true),
                new UTF8Encoding(false));

            RevivalTraceComparison comparison = Compare(authority, unity);
            string comparisonPath = ProjectPath(ComparisonRelativePath);
            File.WriteAllText(
                comparisonPath,
                JsonUtility.ToJson(comparison, true),
                new UTF8Encoding(false));

            Assert.That(comparison.status, Is.EqualTo("equal-revival-trace"),
                comparison.firstDifference);
            Assert.That(comparison.recordsCompared, Is.EqualTo(13));
            Assert.That(comparison.fieldOccurrencesCompared, Is.EqualTo(416));
        }

        private static RevivalTraceEnvelope CaptureUnityTrace()
        {
            var records = new List<RevivalTraceRecord>(13);
            CaptureDirectDefaults(records);
            CaptureC25Cases(records);
            CaptureQueuedCases(records);
            CaptureTerminalAndReuse(records);
            CaptureNormalCases(records);
            Assert.That(records.Count, Is.EqualTo(13));

            return new RevivalTraceEnvelope
            {
                schema = CaptureSchema,
                producer = "UNITY_PRODUCTION_REVIVAL_PATHS",
                evidenceClass = "UNITY_RUNTIME_DIAGNOSTIC_ONLY",
                formalAuthorityExeSha256 = FormalAuthorityExeSha256,
                scenarioId = ScenarioId,
                seed = SharedSeed,
                inputContract = InputContract,
                observationUnit = ObservationUnit,
                records = records.ToArray(),
            };
        }

        private static void CaptureDirectDefaults(
            List<RevivalTraceRecord> records)
        {
            var world = CreateWorld();
            LF2Character entity = CreateUnregisteredCharacter(
                objectId: 9000,
                initialAction: 0,
                relationTeam: 0);
            Assert.That(
                BattleMatchConfigRuntimeAdapter
                    .PrepareDirectParticipantRegistration(entity, 0),
                Is.True);
            world.Register(entity);
            SetPosition(entity, 100, 0, 250, 100, 0, 250);
            entity.RefreshRuntimeSnapshot();
            AppendRecord(records, world, "direct-defaults", 0, 0, 1);
        }

        private static void CaptureC25Cases(
            List<RevivalTraceRecord> records)
        {
            CaptureC25Case(records, "c25-primary-lives2", 0, 2);
            CaptureC25Case(records, "c25-primary-lives1", 0, 1);
            CaptureC25Case(records, "c25-transient-lives2", 20, 2);
        }

        private static void CaptureC25Case(
            List<RevivalTraceRecord> records,
            string caseId,
            int slot,
            int lives)
        {
            SimulationWorld world = CreateWorld();
            LF2Character entity = CreateCharacter(
                world,
                slot,
                9010 + slot + lives,
                5,
                14);
            ConfigureDead(
                entity,
                lives,
                nextLives: 0,
                nextHp: 0,
                renderPhase: 1,
                relationTeam: 5,
                controllerSlot: -1,
                visualId: 0,
                frameCounter: 0,
                motionHold: 0);
            SetPosition(
                entity,
                100 + slot,
                0,
                250 + slot,
                100 + slot,
                0,
                250 + slot);
            entity.RefreshRuntimeSnapshot();

            world.LateEntityUpdateAll(25);

            AppendRecord(records, world, caseId, 25, slot, 1);
        }

        private static void CaptureQueuedCases(
            List<RevivalTraceRecord> records)
        {
            {
                SimulationWorld world = CreateWorld();
                CreateCharacter(world, 2, 9022, 7, 0);
                LF2Character entity = CreateCharacter(world, 0, 30, 5, 14);
                ConfigureDead(entity, 1, 4, 180, 2, 5, 2, 0);
                world.SetRespawnEffectSpawnOverrideForSelfCheck(
                    (_, source) => source);

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(records, world, "queued-explicit", 7, 0, 1);
            }
            {
                SimulationWorld world = CreateWorld();
                LF2Character entity = CreateCharacter(world, 0, 37, 5, 14);
                ConfigureDead(entity, 1, 4, 180, 2, 5, 77, 0);
                world.SetRespawnEffectSpawnOverrideForSelfCheck(
                    (_, source) => source);

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(records, world, "queued-missing", 7, 0, 1);
            }
            {
                SimulationWorld world = CreateWorld();
                LF2Character entity = CreateCharacter(world, 0, 37, 5, 14);
                ConfigureDead(entity, 1, 4, 180, 2, 5, -1, 0);
                world.SetRespawnEffectSpawnOverrideForSelfCheck(
                    (_, source) => source);

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(
                    records,
                    world,
                    "queued-fallback-inactive",
                    7,
                    0,
                    1);
            }
        }

        private static void CaptureTerminalAndReuse(
            List<RevivalTraceRecord> records)
        {
            {
                SimulationWorld world = CreateWorld();
                LF2Character entity = CreateCharacter(
                    world,
                    19,
                    9049,
                    5,
                    14);
                ConfigureDead(entity, 1, 0, 0, 2, 5, -1, 0);

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(records, world, "terminal-primary", 7, 19, 1);
            }
            {
                SimulationWorld world = CreateWorld();
                LF2Character entity = CreateCharacter(
                    world,
                    20,
                    9050,
                    5,
                    14);
                ConfigureDead(entity, 1, 0, 0, 2, 5, -1, 0);

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(
                    records,
                    world,
                    "terminal-transient-removed",
                    7,
                    20,
                    1);
                LF2Character reused = CreateCharacter(
                    world,
                    20,
                    9099,
                    0,
                    0);
                SetPosition(reused, 120, 0, 270, 120, 0, 270);
                reused.RefreshRuntimeSnapshot();
                AppendRecord(
                    records,
                    world,
                    "terminal-slot-reused",
                    8,
                    20,
                    2);
            }
        }

        private static void CaptureNormalCases(
            List<RevivalTraceRecord> records)
        {
            {
                SimulationWorld world = CreateWorld();
                LF2Character dead = CreateCharacter(world, 0, 9060, 5, 14);
                LF2Character peer = CreateCharacter(world, 1, 9061, 5, 0);
                ConfigureDead(dead, 2, 4, 80, 2, 5, 22, 7);
                dead.Runtime.CollisionYReference = -25;
                SetPosition(dead, 123, 8, 345, 111, 8, 333);
                dead.Runtime.SetVelocity(0, 9, 0);
                dead.PS.vy = 9;
                SetPosition(peer, 200, 0, 300, 200, 0, 300);
                dead.RefreshRuntimeSnapshot();
                peer.RefreshRuntimeSnapshot();

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(records, world, "normal-nonzero", 7, 0, 1);
                AppendRecord(
                    records,
                    world,
                    "normal-peer-preserved",
                    7,
                    1,
                    1);
            }
            {
                SimulationWorld world = CreateWorld();
                LF2Character dead = CreateCharacter(world, 0, 9070, 5, 14);
                LF2Character peerA = CreateCharacter(world, 1, 9071, 5, 0);
                LF2Character peerB = CreateCharacter(world, 2, 9072, 5, 0);
                ConfigureDead(dead, 2, 4, 80, 2, 5, 22, 7);
                dead.Runtime.CollisionYReference = 0;
                SetPosition(dead, 123, 8, 345, 111, 8, 333);
                dead.Runtime.SetVelocity(0, 9, 0);
                dead.PS.vy = 9;
                SetPosition(peerA, -50, 0, 100, -50, 0, 100);
                SetPosition(peerB, 50, 0, 200, 50, 0, 200);
                dead.RefreshRuntimeSnapshot();
                peerA.RefreshRuntimeSnapshot();
                peerB.RefreshRuntimeSnapshot();

                world.PostFrameAdvanceDeathCleanupAll(7);

                AppendRecord(
                    records,
                    world,
                    "normal-sumx-zero",
                    7,
                    0,
                    1);
            }
        }

        private static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld();
            world.NativeRandom.ResetFromSeed(SharedSeed);
            return world;
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            int slot,
            int objectId,
            int relationTeam,
            int initialAction)
        {
            LF2Character character = CreateUnregisteredCharacter(
                objectId,
                initialAction,
                relationTeam);
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            SetPosition(
                character,
                100 + slot,
                0,
                250 + slot,
                100 + slot,
                0,
                250 + slot);
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static LF2Character CreateUnregisteredCharacter(
            int objectId,
            int initialAction,
            int relationTeam)
        {
            var data = new LF2CharacterData
            {
                name = "B4RevivalExit_" + objectId,
                type_sub = (int)LF2ObjectType.Character,
                frames = new List<LF2FrameData>
                {
                    Frame(0, LF2States.Standing),
                    Frame(14, LF2States.Lying),
                    Frame(212, LF2States.Jump),
                    Frame(219, LF2States.Standing),
                },
            };
            var character = new LF2Character
            {
                ObjectId = objectId,
                Name = data.name,
            };
            character.ModuleInitialize();
            character.FrameCache.Load(
                new LF2CharacterDataWrapper(objectId, data));
            character.ImmediateFrame(initialAction);
            character.Initialize(500, 500);
            character.RelationTeam = relationTeam;
            character.Team = relationTeam;
            return character;
        }

        private static void ConfigureDead(
            LF2Character entity,
            int lives,
            int nextLives,
            int nextHp,
            int renderPhase,
            int relationTeam,
            int controllerSlot,
            int visualId,
            int frameCounter = 6,
            int motionHold = 3)
        {
            entity.DirectWriteFramePreserveWaitCounter(14);
            entity.Health.HP = 0;
            entity.Health.HPBound = 10;
            entity.Health.HP3 = 180;
            entity.Health.PP = 77;
            entity.HP2Orig = lives;
            entity.HPOrig = nextLives;
            entity.RespawnCount = nextHp;
            entity.RelationTeam = relationTeam;
            entity.Team = relationTeam;
            entity.Runtime.Unk360 = controllerSlot;
            entity.HitStun = renderPhase;
            entity.Runtime.ReviveVisualId184 = visualId;
            entity.Runtime.ReviveVisualRuntime180 = 9;
            entity.Runtime.RenderPicOffset = 8;
            entity.AttackingCounter = frameCounter;
            entity.FrameDelay = motionHold;
            entity.RefreshRuntimeSnapshot();
        }

        private static void SetPosition(
            LF2Entity entity,
            double preciseX,
            double preciseY,
            double preciseZ,
            int integerX,
            int integerY,
            int integerZ)
        {
            entity.Runtime.SetPosition(integerX, integerY, integerZ);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetPosition(preciseX, preciseY, preciseZ);
            entity.PS.x = preciseX;
            entity.PS.y = preciseY;
            entity.PS.z = preciseZ;
        }

        private static LF2FrameData Frame(int frameId, int state)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 10000,
                next = frameId,
                pic = 999,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(),
            };
        }

        private static void AppendRecord(
            List<RevivalTraceRecord> records,
            SimulationWorld world,
            string caseId,
            int observationTick,
            int slot,
            int generationOrdinal)
        {
            NTSD28NativeRandomScalarState random =
                world.NativeRandom.CaptureScalarState();
            var record = new RevivalTraceRecord
            {
                caseId = caseId,
                observationTick = observationTick,
                slot = slot,
                generationOrdinal = generationOrdinal,
                synchronizedCalls = (long)random.SynchronizedCalls,
                lastSynchronizedCallSite = random.LastSynchronizedCallSite,
                crtCalls = (long)random.CrtCalls,
            };
            LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
            if (entity == null)
            {
                records.Add(record);
                return;
            }

            record.active = 1;
            record.oid = entity.ObjectId;
            record.action = entity.Frame.N;
            record.frameCounter = entity.AttackingCounter;
            record.motionHold = entity.FrameDelay;
            record.currentHp = entity.Health.HP;
            record.effectiveMaxHp = entity.Health.HPBound;
            record.baseMaxHp = entity.Health.HP3;
            record.currentMp = entity.Health.PP;
            record.lives = entity.HP2Orig;
            record.nextLives = entity.HPOrig;
            record.nextHp = entity.RespawnCount;
            record.battleGroup = entity.RelationTeam;
            record.controllerSlot = entity.Runtime.Unk360;
            record.renderPhase = entity.HitStun;
            record.visualRuntime180 = entity.Runtime.ReviveVisualRuntime180;
            record.visualId184 = entity.Runtime.ReviveVisualId184;
            record.visualRuntime318 = entity.Runtime.RenderPicOffset;
            record.x = entity.Runtime.XInt;
            record.y = entity.Runtime.YInt;
            record.z = entity.Runtime.ZInt;
            record.preciseXMilli = Milli(entity.Runtime.X);
            record.preciseYMilli = Milli(entity.Runtime.Y);
            record.preciseZMilli = Milli(entity.Runtime.Z);
            record.motionYMilli = Milli(entity.Runtime.Vy);
            records.Add(record);
        }

        private static long Milli(double value)
        {
            return Convert.ToInt64(
                Math.Round(value * 1000.0, MidpointRounding.AwayFromZero));
        }

        private static void ValidateAuthorityHeader(
            RevivalTraceEnvelope authority)
        {
            Assert.That(authority, Is.Not.Null);
            Assert.That(authority.schema, Is.EqualTo(CaptureSchema));
            Assert.That(authority.producer,
                Is.EqualTo("NTSD28_PLAYABLE_SOURCE_MODEL"));
            Assert.That(authority.evidenceClass,
                Is.EqualTo("SOURCE_MODEL_DIAGNOSTIC_ONLY"));
            Assert.That(authority.formalAuthorityExeSha256,
                Is.EqualTo(FormalAuthorityExeSha256));
            Assert.That(authority.authoritySourceManifestSha256,
                Is.EqualTo(AuthoritySourceManifestSha256));
            Assert.That(authority.captureRunnerSourceSha256?.Length,
                Is.EqualTo(64));
            Assert.That(authority.captureBinarySha256?.Length,
                Is.EqualTo(64));
            Assert.That(authority.scenarioId, Is.EqualTo(ScenarioId));
            Assert.That(authority.seed, Is.EqualTo(SharedSeed));
            Assert.That(authority.inputContract, Is.EqualTo(InputContract));
            Assert.That(authority.observationUnit,
                Is.EqualTo(ObservationUnit));
            Assert.That(authority.records, Has.Length.EqualTo(13));
        }

        private static RevivalTraceComparison Compare(
            RevivalTraceEnvelope authority,
            RevivalTraceEnvelope unity)
        {
            var result = new RevivalTraceComparison
            {
                schema = "ntsd28-b4-revival-exit-comparison-v1",
                status = "equal-revival-trace",
                firstDifference = string.Empty,
            };
            if (authority.records == null || unity.records == null ||
                authority.records.Length != unity.records.Length)
            {
                result.status = "different";
                result.firstDifference = "record-count";
                return result;
            }

            for (int index = 0; index < authority.records.Length; index++)
            {
                string difference = FirstDifference(
                    authority.records[index],
                    unity.records[index]);
                if (!string.IsNullOrEmpty(difference))
                {
                    result.status = "different";
                    result.firstDifference = $"record[{index}].{difference}";
                    return result;
                }
                result.recordsCompared++;
                result.fieldOccurrencesCompared += 32;
            }
            return result;
        }

        private static string FirstDifference(
            RevivalTraceRecord expected,
            RevivalTraceRecord actual)
        {
            if (expected.caseId != actual.caseId) return "caseId";
            if (expected.observationTick != actual.observationTick)
                return "observationTick";
            if (expected.slot != actual.slot) return "slot";
            if (expected.generationOrdinal != actual.generationOrdinal)
                return "generationOrdinal";
            if (expected.active != actual.active) return "active";
            if (expected.oid != actual.oid) return "oid";
            if (expected.action != actual.action) return "action";
            if (expected.frameCounter != actual.frameCounter)
                return "frameCounter";
            if (expected.motionHold != actual.motionHold) return "motionHold";
            if (expected.currentHp != actual.currentHp) return "currentHp";
            if (expected.effectiveMaxHp != actual.effectiveMaxHp)
                return "effectiveMaxHp";
            if (expected.baseMaxHp != actual.baseMaxHp) return "baseMaxHp";
            if (expected.currentMp != actual.currentMp) return "currentMp";
            if (expected.lives != actual.lives) return "lives";
            if (expected.nextLives != actual.nextLives) return "nextLives";
            if (expected.nextHp != actual.nextHp) return "nextHp";
            if (expected.battleGroup != actual.battleGroup)
                return "battleGroup";
            if (expected.controllerSlot != actual.controllerSlot)
                return "controllerSlot";
            if (expected.renderPhase != actual.renderPhase)
                return "renderPhase";
            if (expected.visualRuntime180 != actual.visualRuntime180)
                return "visualRuntime180";
            if (expected.visualId184 != actual.visualId184)
                return "visualId184";
            if (expected.visualRuntime318 != actual.visualRuntime318)
                return "visualRuntime318";
            if (expected.x != actual.x) return "x";
            if (expected.y != actual.y) return "y";
            if (expected.z != actual.z) return "z";
            if (expected.preciseXMilli != actual.preciseXMilli)
                return "preciseXMilli";
            if (expected.preciseYMilli != actual.preciseYMilli)
                return "preciseYMilli";
            if (expected.preciseZMilli != actual.preciseZMilli)
                return "preciseZMilli";
            if (expected.motionYMilli != actual.motionYMilli)
                return "motionYMilli";
            if (expected.synchronizedCalls != actual.synchronizedCalls)
                return "synchronizedCalls";
            if (expected.lastSynchronizedCallSite !=
                actual.lastSynchronizedCallSite)
            {
                return "lastSynchronizedCallSite";
            }
            if (expected.crtCalls != actual.crtCalls) return "crtCalls";
            return string.Empty;
        }

        private static string ProjectPath(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.GetFullPath(Path.Combine(root ?? string.Empty, relativePath));
        }

        [Serializable]
        private sealed class RevivalTraceEnvelope
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
            public string inputContract = string.Empty;
            public string observationUnit = string.Empty;
            public RevivalTraceRecord[] records =
                Array.Empty<RevivalTraceRecord>();
        }

        [Serializable]
        private sealed class RevivalTraceRecord
        {
            public string caseId = string.Empty;
            public int observationTick;
            public int slot;
            public int generationOrdinal;
            public int active;
            public int oid = -1;
            public int action = -1;
            public int frameCounter;
            public int motionHold;
            public int currentHp;
            public int effectiveMaxHp;
            public int baseMaxHp;
            public int currentMp;
            public int lives;
            public int nextLives;
            public int nextHp;
            public int battleGroup;
            public int controllerSlot = -1;
            public int renderPhase;
            public int visualRuntime180;
            public int visualId184;
            public int visualRuntime318;
            public int x;
            public int y;
            public int z;
            public long preciseXMilli;
            public long preciseYMilli;
            public long preciseZMilli;
            public long motionYMilli;
            public long synchronizedCalls;
            public long lastSynchronizedCallSite;
            public long crtCalls;
        }

        [Serializable]
        private sealed class RevivalTraceComparison
        {
            public string schema = string.Empty;
            public string status = string.Empty;
            public int recordsCompared;
            public int fieldOccurrencesCompared;
            public string firstDifference = string.Empty;
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28B4RevivalExitAuditRequestRunner : ICallbacks
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalExit-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalExit-v1.result";
        private const string FocusedTestClass =
            "NTSD.Test.Editor.NTSD28B4RevivalExitAuditEditorTests";

        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static readonly StringBuilder FailureDetails =
            new StringBuilder(4096);
        private static NTSD28B4RevivalExitAuditRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;

        static NTSD28B4RevivalExitAuditRequestRunner()
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
            activeCallbacks = new NTSD28B4RevivalExitAuditRequestRunner();
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
    internal static class NTSD28B4RevivalExitAuditPlayRunner
    {
        private const string RequestRelativePath =
            "Temp/NTSD28-B4-RevivalExit-Play-v1.request";
        private const string ResultRelativePath =
            "Temp/NTSD28-B4-RevivalExit-Play-v1.result";
        private static readonly string RequestPath = ProjectPath(RequestRelativePath);
        private static readonly string ResultPath = ProjectPath(ResultRelativePath);
        private static bool running;

        static NTSD28B4RevivalExitAuditPlayRunner()
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
                new NTSD28B4RevivalExitAuditEditorTests()
                    .RevivalProductionTrace_MatchesAuthorityAcrossB4ExitCases();
                File.WriteAllText(
                    ResultPath,
                    "state=Passed\nrecords=13\nfields=416\n" +
                    "firstDifference=\nsceneMutation=none\n",
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
