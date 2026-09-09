#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28C12FusionBarrierPlacementPlayModeProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_B3_C12_FusionBarrierPlacement.request";
        private const string ResultPath = "Temp/NTSD28_B3_C12_FusionBarrierPlacement.result.json";

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            string requestPath = ProjectPath(RequestPath);
            if (!File.Exists(requestPath))
                return;
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }
            SimulationTickDriver current = SimulationTickDriver.Instance;
            if (current?.World == null || current.CurrentTickIndex < 5)
                return;
            File.Delete(requestPath);
            string resultPath = ProjectPath(ResultPath);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
            Run();
            EditorApplication.delayCall += ExitPlayModeAfterRequest;
        }

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C12 Fusion Barrier Placement Play Probe")]
        public static void Run()
        {
            var report = new Report();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                report.status = "FAIL";
                report.message = "Play Mode production world is unavailable.";
                Write(report);
                return;
            }

            bool previousPaused = driver.IsPaused;
            int baselineObjects = world.ObjectCount;
            int baselineClaimed = world.ClaimedRuntimeSlotCountForDiagnostics;
            FusionObserver self = null;
            PassiveCharacter partner = null;
            try
            {
                driver.SetPaused(true);
                int selfSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(1, 10);
                int partnerSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(10, 20);
                Require(selfSlot > 0 && selfSlot < 10 && partnerSlot >= 10 && partnerSlot < 20,
                    "No production-compatible OID fusion slots are available.");

                Dictionary<int, LF2CharacterDataWrapper> wrappers = BuildWrappers();
                world.SetRuntimeCharacterConfigResolverForSelfCheck(oid =>
                    wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper) ? wrapper : null);
                self = CreateCharacter<FusionObserver>(world, wrappers[7], 7, selfSlot, 120);
                partner = CreateCharacter<PassiveCharacter>(world, wrappers[8], 8, partnerSlot, 100);
                self.Partner = partner;

                report.initialFrame = self.Frame.N;
                report.initialState = self.Frame.D?.state ?? -1;
                report.initialRuntimeFrame = self.Runtime.Frame;
                report.initialHasFrame10 = self.FrameCache.HasFrame(10);

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(ignorePaused: true, buildPresentation: false),
                    "Production driver rejected the C12 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.selfSlot = selfSlot;
                report.partnerSlot = partnerSlot;
                report.postSerialCount = self.PostSerialCount;
                report.oidSeenInSerial = self.ObservedOidInSerial;
                report.frameSeenInSerial = self.ObservedFrameInSerial;
                report.stateSeenInSerial = self.ObservedStateInSerial;
                report.partnerFrameSeenInSerial = self.ObservedPartnerFrameInSerial;
                report.partnerStateSeenInSerial = self.ObservedPartnerStateInSerial;
                report.frameSeenInProducer = self.ObservedFrameInProducer;
                report.frameSeenInRoute = self.ObservedFrameInRoute;
                report.frameSeenInPhysics = self.ObservedFrameInPhysics;
                report.stateSeenInPhysics = self.ObservedStateInPhysics;
                report.frameSeenAtCollisionSnapshot = self.ObservedFrameAtCollisionSnapshot;
                report.stateSeenAtCollisionSnapshot = self.ObservedStateAtCollisionSnapshot;
                report.finalOid = self.ObjectId;
                report.finalTimer = self.Runtime.Unk338;
                report.partnerDormant = partner.Runtime.OidMergeDormant;
                report.selfFrame = self.Frame.N;
                report.selfState = self.Frame.D?.state ?? -1;
                report.selfHp = self.Health.HP;
                report.selfHpBound = self.Health.HPBound;
                report.selfHp3 = self.Health.HP3;
                report.selfTeam = self.RelationTeam;
                report.selfX = self.Runtime.XInt;
                report.selfZ = self.Runtime.ZInt;
                report.partnerFrame = partner.Frame.N;
                report.partnerState = partner.Frame.D?.state ?? -1;
                report.partnerHp = partner.Health.HP;
                report.partnerHpBound = partner.Health.HPBound;
                report.partnerTeam = partner.RelationTeam;
                report.partnerX = partner.Runtime.XInt;
                report.partnerZ = partner.Runtime.ZInt;
                report.oid51Resolvable = world.RuntimeCharacterConfigs.Resolve(51) != null;
                Require(driver.CurrentTickIndex == expectedTick &&
                        self.PostSerialCount == 1 &&
                        self.ObservedOidInSerial == 51 &&
                        self.ObjectId == 51 &&
                        self.Runtime.Unk338 == 4499 &&
                        partner.Runtime.OidMergeDormant,
                    "Serial remainder did not observe the completed C12 OID7/8 to 51 fusion.");

                report.status = "PASS";
                report.message = "Production C12 fusion completed before serial remainder and C25h decremented afterward.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                world.SetRuntimeCharacterConfigResolverForSelfCheck(null);
                if (partner != null)
                    world.Unregister(partner);
                if (self != null)
                    world.Unregister(self);
                driver.SetPaused(previousPaused);
                report.cleanupPassed = world.ObjectCount == baselineObjects &&
                    world.ClaimedRuntimeSlotCountForDiagnostics == baselineClaimed;
                if (!report.cleanupPassed)
                {
                    report.status = "FAIL";
                    report.message += " Cleanup did not restore object/slot counts.";
                }
                Write(report);
            }
        }

        private static T CreateCharacter<T>(SimulationWorld world, LF2CharacterDataWrapper wrapper, int oid, int slot, double x)
            where T : LF2Character, new()
        {
            var character = new T();
            character.ModuleInitialize();
            character.Name = "C12Play_" + oid;
            character.ObjectId = oid;
            character.RelationTeam = 987654;
            character.FrameCache.Load(wrapper);
            character.Frame.N = 10;
            character.Frame.PN = 10;
            character.Frame.D = character.FrameCache.GetFrameDataById(10);
            character.Initialize(500, 500);
            character.Health.HP = 100;
            character.Health.HPBound = 100;
            character.Health.HP3 = 500;
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            character.Frame.N = 10;
            character.Frame.PN = 10;
            character.Frame.D = character.FrameCache.GetFrameDataById(10);
            character.Runtime.SetPosition(x, 0.0, 300.0);
            character.Runtime.SyncIntegerPosition();
            return character;
        }

        private static Dictionary<int, LF2CharacterDataWrapper> BuildWrappers()
        {
            return new Dictionary<int, LF2CharacterDataWrapper>
            {
                [7] = new LF2CharacterDataWrapper(7, BaseData("C12PlayOid7")),
                [8] = new LF2CharacterDataWrapper(8, BaseData("C12PlayOid8")),
                [51] = new LF2CharacterDataWrapper(51, new LF2CharacterData
                {
                    name = "C12PlayOid51",
                    frames = new List<LF2FrameData> { Frame(0, 0), Frame(290, 2) },
                }),
            };
        }

        private static LF2CharacterData BaseData(string name)
        {
            return new LF2CharacterData
            {
                name = name,
                frames = new List<LF2FrameData>
                {
                    Frame(0, 0),
                    Frame(9, 2),
                    Frame(10, 2),
                    Frame(11, 2),
                    Frame(12, 2),
                },
            };
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
                centerx = 39,
                centery = 79,
                itrs = new List<InteractionArea>(),
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            File.WriteAllText(ProjectPath(ResultPath), JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C12FusionBarrierPlacementPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C12FusionBarrierPlacementPlayProbe] " + report.message);
        }

        private static void ExitPlayModeAfterRequest()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private sealed class FusionObserver : LF2Character
        {
            internal LF2Character Partner { get; set; }
            internal int PostSerialCount { get; private set; }
            internal int ObservedOidInSerial { get; private set; } = -1;
            internal int ObservedFrameInSerial { get; private set; } = -1;
            internal int ObservedStateInSerial { get; private set; } = -1;
            internal int ObservedPartnerFrameInSerial { get; private set; } = -1;
            internal int ObservedPartnerStateInSerial { get; private set; } = -1;
            internal int ObservedFrameInProducer { get; private set; } = -1;
            internal int ObservedFrameInRoute { get; private set; } = -1;
            internal int ObservedFrameInPhysics { get; private set; } = -1;
            internal int ObservedStateInPhysics { get; private set; } = -1;
            internal int ObservedFrameAtCollisionSnapshot { get; private set; } = -1;
            internal int ObservedStateAtCollisionSnapshot { get; private set; } = -1;

            internal override bool RunNativePhysicsForWorldPass(int tickIndex)
            {
                ObservedFrameInPhysics = Frame?.N ?? -1;
                ObservedStateInPhysics = Frame?.D?.state ?? -1;
                return true;
            }

            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(int tickIndex)
            {
                ObservedFrameInProducer = Frame?.N ?? -1;
            }

            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(int tickIndex, bool applyFrameMotionTail = true)
            {
                ObservedFrameInRoute = Frame?.N ?? -1;
            }

            public override void CaptureCollisionFrameSnapshot()
            {
                ObservedFrameAtCollisionSnapshot = Frame?.N ?? -1;
                ObservedStateAtCollisionSnapshot = Frame?.D?.state ?? -1;
                base.CaptureCollisionFrameSnapshot();
            }
            internal override void RunPostNativePhysicsSerialForWorldPass(int tickIndex, bool nativePhysicsCompleted)
            {
                PostSerialCount++;
                ObservedOidInSerial = ObjectId;
                ObservedFrameInSerial = Frame?.N ?? -1;
                ObservedStateInSerial = Frame?.D?.state ?? -1;
                ObservedPartnerFrameInSerial = Partner?.Frame?.N ?? -1;
                ObservedPartnerStateInSerial = Partner?.Frame?.D?.state ?? -1;
            }
        }

        private sealed class PassiveCharacter : LF2Character
        {
            internal override bool RunNativePhysicsForWorldPass(int tickIndex) => true;
            internal override void RunCharacterInputProducerPhaseForKnownCharacterDat(int tickIndex) { }
            internal override void RunCharacterInputRoutingPhaseForKnownCharacterDat(int tickIndex, bool applyFrameMotionTail = true) { }
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int selfSlot;
            public int partnerSlot;
            public int initialFrame;
            public int initialState;
            public int initialRuntimeFrame;
            public bool initialHasFrame10;
            public int postSerialCount;
            public int oidSeenInSerial;
            public int frameSeenInSerial;
            public int stateSeenInSerial;
            public int partnerFrameSeenInSerial;
            public int partnerStateSeenInSerial;
            public int frameSeenInProducer;
            public int frameSeenInRoute;
            public int frameSeenInPhysics;
            public int stateSeenInPhysics;
            public int frameSeenAtCollisionSnapshot;
            public int stateSeenAtCollisionSnapshot;
            public int finalOid;
            public int finalTimer;
            public bool partnerDormant;
            public int selfFrame;
            public int selfState;
            public int selfHp;
            public int selfHpBound;
            public int selfHp3;
            public int selfTeam;
            public int selfX;
            public int selfZ;
            public int partnerFrame;
            public int partnerState;
            public int partnerHp;
            public int partnerHpBound;
            public int partnerTeam;
            public int partnerX;
            public int partnerZ;
            public bool oid51Resolvable;
            public bool cleanupPassed;
        }
    }
}
#endif
