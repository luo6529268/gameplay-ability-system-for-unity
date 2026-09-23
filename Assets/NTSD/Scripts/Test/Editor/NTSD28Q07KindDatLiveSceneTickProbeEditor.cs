#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q07KindDatLiveSceneTickProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string Request = "Temp/NTSD28_Q07_KindDatLiveSceneTick.request";
        private const string Result =
            "artifacts/diagnostics/NTSD28-Q07-KIND-DAT-LIVE-SCENE-TICK-001/result.json";

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string scenePath;
            public bool sceneDirtyAtEntry;
            public int startTick;
            public int endTick;
            public int selectedRecords;
            public int selectedEffect;
            public int selectedFrame;
            public int attackerSlot;
            public int targetSlot;
            public int targetObjectId;
            public int targetType;
            public int targetFrame;
            public int targetPreviousFrame;
            public int targetTeam;
            public int targetOwnerSlot;
            public bool targetSpecialHitLatch;
            public bool definitionTransferred;
            public int sceneObjectsBefore;
            public int sceneObjectsAfterCleanup;
            public int poolBorrowersBefore;
            public int poolBorrowersAfter;
            public string scope;
        }

        private sealed class Type3Combatant : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.SpecialAttack;

            internal override bool UsesDynamicRuntimeSlot() => true;
        }

        static NTSD28Q07KindDatLiveSceneTickProbeEditor()
        {
            EditorApplication.update += Poll;
        }

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(ProjectPath(Request)))
                return;

            string request = File.ReadAllText(ProjectPath(Request)).Trim();
            if (request == "run" && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Scene scene = SceneManager.GetActiveScene();
                if (scene.path != ScenePath || scene.isDirty)
                {
                    Write(new Report
                    {
                        status = "FAIL",
                        scenePath = scene.path,
                        sceneDirtyAtEntry = scene.isDirty,
                        error = "Expected the saved NTSD_Battle Scene in EditMode."
                    });
                    File.WriteAllText(ProjectPath(Request), "done");
                    return;
                }

                File.WriteAllText(ProjectPath(Request),
                    "running:" + DateTime.UtcNow.Ticks);
                EditorApplication.EnterPlaymode();
                return;
            }

            if (!request.StartsWith("running:", StringComparison.Ordinal) ||
                !EditorApplication.isPlaying)
                return;

            var report = new Report
            {
                scenePath = SceneManager.GetActiveScene().path,
                scope = "Original saved Battle Scene, selected formal kind catalog, " +
                    "temporary type-3 OID213 to OID206 and one full production driver tick."
            };
            try
            {
                long started = long.Parse(request.Substring("running:".Length));
                if ((DateTime.UtcNow -
                    new DateTime(started, DateTimeKind.Utc)).TotalSeconds > 120)
                    throw new TimeoutException("Q07 kind-dependent Scene probe timed out.");

                SimulationTickDriver driver = SimulationTickDriver.Instance;
                SimulationWorld world = driver?.World;
                if (world == null || driver.CurrentTickIndex < 5 ||
                    !world.IsBattleSnapshotBoundaryReady)
                    return;
                if (!driver.IsPaused)
                {
                    driver.SetPaused(true);
                    return;
                }

                LoganKindCatalog catalog = world.RuntimeDataCatalog?.KindCatalog;
                Require(catalog != null && catalog.IsValid,
                    "Live World has no valid selected kind catalog.");
                report.selectedRecords = catalog.Records.Count;
                LoganKindRecord selected = catalog.FindEffect(209);
                Require(selected != null && selected.Binds(213) &&
                    selected.RespondsTo(206) && selected.Frame == 40,
                    "Live World selected kind record differs from formal effect209/frame40.");
                report.selectedEffect = selected.Effect;
                report.selectedFrame = selected.Frame;

                report.sceneObjectsBefore = world.ObjectCount;
                report.poolBorrowersBefore =
                    LF2ObjectPool.TryGetInstance()?.ActiveObjectCountForAcceptance ?? 0;
                int attackerSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    50, world.RuntimeSlotCapacityForDiagnostics);
                int targetSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    attackerSlot + 1, world.RuntimeSlotCapacityForDiagnostics);
                Require(attackerSlot >= 50 && targetSlot > attackerSlot,
                    "Two free dynamic runtime slots are required.");
                report.attackerSlot = attackerSlot;
                report.targetSlot = targetSlot;

                Type3Combatant attacker = null;
                Type3Combatant target = null;
                try
                {
                    attacker = CreateCombatant(world, attackerSlot, 213, 4, 7, true);
                    target = CreateCombatant(world, targetSlot, 206, 9, 8, false);
                    report.startTick = driver.CurrentTickIndex + 1;
                    Require(driver.StepOneTick(
                        ignorePaused: true, buildPresentation: false),
                        "Production driver rejected the controlled full tick.");
                    report.endTick = driver.CurrentTickIndex;
                    report.targetObjectId = target.ObjectId;
                    report.targetType = target.GetCurrentDataObjectTypeForSimulation();
                    report.targetFrame = target.Frame.N;
                    report.targetPreviousFrame = target.Frame.Prev;
                    report.targetTeam = target.RelationTeam;
                    report.targetOwnerSlot = target.Runtime.OwnerSlotIndex;
                    report.targetSpecialHitLatch = target.Runtime.SpecialHitLatch0EB;
                    report.definitionTransferred =
                        ReferenceEquals(target.FrameCache.Wrapper,
                            attacker.FrameCache.Wrapper);

                    Require(report.endTick == report.startTick &&
                        report.targetObjectId == 213 &&
                        report.targetType == (int)LF2ObjectType.SpecialAttack &&
                        report.targetFrame == 40 &&
                        report.targetPreviousFrame == 40 &&
                        report.targetTeam == 4 && report.targetOwnerSlot == 7 &&
                        report.targetSpecialHitLatch && report.definitionTransferred,
                        "Full tick did not complete the selected kind type-3 transform.");
                    report.status = "PASS";
                }
                finally
                {
                    if (target != null)
                        world.Unregister(target);
                    if (attacker != null)
                        world.Unregister(attacker);
                    report.sceneObjectsAfterCleanup = world.ObjectCount;
                    report.poolBorrowersAfter =
                        LF2ObjectPool.TryGetInstance()?.ActiveObjectCountForAcceptance ?? 0;
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }

            Write(report);
            File.WriteAllText(ProjectPath(Request), "done");
            EditorApplication.ExitPlaymode();
        }

        private static Type3Combatant CreateCombatant(SimulationWorld world,
            int slot, int objectId, int team, int ownerSlot, bool attacker)
        {
            var frame0 = new LF2FrameData
            {
                frameId = 0,
                state = 3000,
                wait = 3,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (attacker)
                frame0.itrs.Add(new InteractionArea
                {
                    kind = 0, x = 0, y = 0, w = 80, h = 80,
                    injury = 10, arest = 1, vrest = 1
                });
            else
            {
                frame0.hit_Uj = 25;
                frame0.bodies.Add(new BattleBodyBoxValue(0, 0, 80, 80));
            }

            var continuation = new LF2FrameData
            {
                frameId = attacker ? 40 : 25,
                state = 3000,
                wait = 3,
                next = attacker ? 40 : 25,
            };
            var data = new LF2CharacterData
            {
                name = "Q07KindLive_" + objectId,
                type_sub = objectId,
                frames = new List<LF2FrameData> { frame0, continuation },
            };
            var entity = new Type3Combatant();
            entity.ModuleInitialize();
            entity.Name = data.name;
            entity.ObjectId = objectId;
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = 0;
            entity.Initialize(500, 500);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.OwnerSlotIndex = ownerSlot;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.HP3 = 500;
            entity.Runtime.SetPosition(200, 0, 300);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private static void Require(bool condition, string error)
        {
            if (!condition)
                throw new InvalidOperationException(error);
        }

        private static void Write(Report report)
        {
            string path = ProjectPath(Result);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }
    }
}
#endif
