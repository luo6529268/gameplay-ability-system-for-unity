#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28C25DefinitionClonePlayModeProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_B3_C25A_B_DefinitionClone.request";
        private const string ResultPath =
            "Temp/NTSD28_B3_C25A_B_DefinitionClone.result.json";

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

            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;

            File.Delete(requestPath);
            string resultPath = ProjectPath(ResultPath);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
            Run();
            EditorApplication.delayCall += ExitPlayModeAfterRequest;
        }

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C25a-b Definition Clone Play Probe")]
        public static void Run()
        {
            var report = new Report();
            try
            {
                Require(EditorApplication.isPlaying,
                    "C25a-b probe requires Play Mode.");
                Execute(report);
                report.status = "PASS";
                report.message =
                    "C25a definition transition and C25b synchronized clone transaction passed in Play Mode.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                Write(report);
            }
        }

        private static void Execute(Report report)
        {
            const int transitionSourceOid = 9000;
            const int cloneSourceOid = 9001;
            const uint seed = 0x13572468u;
            var wrappers = new Dictionary<int, LF2CharacterDataWrapper>();
            var definitions = new List<ObjectDefinition>();

            AddData(
                transitionSourceOid,
                LF2ObjectType.Other,
                Data(
                    "Play_C25a_Source",
                    LF2ObjectType.Other,
                    Frame(5, 8007, 3)));
            AddData(
                7,
                LF2ObjectType.SpecialAttack,
                Data(
                    "Play_C25a_Target",
                    LF2ObjectType.SpecialAttack,
                    Frame(0, 100, 0),
                    Frame(3, 301, 3)));
            AddData(
                cloneSourceOid,
                LF2ObjectType.Character,
                Data(
                    "Play_C25b_Source",
                    LF2ObjectType.Character,
                    Frame(0, 9996, 0)));
            AddWeapon(217);
            AddWeapon(218);

            var resolver = new RuntimeCharacterConfigResolver(oid =>
                wrappers.TryGetValue(oid, out LF2CharacterDataWrapper wrapper)
                    ? wrapper
                    : null);
            var world = new SimulationWorld(
                BattleRuntimeProfile.Authority400,
                SimulationWorld.AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                resolver);
            world.PrepareRuntimeDataCatalogForBattle(
                definitions,
                oid => wrappers.TryGetValue(
                    oid,
                    out LF2CharacterDataWrapper wrapper)
                        ? wrapper
                        : null);
            world.LogicReferencePool.Prewarm(LF2ObjectType.LightWeapon, 8);
            world.LogicReferencePool.PrewarmTasks<OPointCreateTask>(2);
            world.SetLogicOnlyEntityMaterialization(true);
            world.NativeRandom.ResetFromSeed(seed);

            LF2Character transitionSource = CreateCharacter(
                wrappers[transitionSourceOid],
                60,
                5);
            transitionSource.AttackingCounter = 9;
            transitionSource.Runtime.RenderPicOffset = 140;
            transitionSource.Runtime.WeaponFlightCounter = 321;
            world.Register(transitionSource);

            LF2Character cloneSource = CreateCharacter(
                wrappers[cloneSourceOid],
                61,
                0);
            cloneSource.AttackingCounter = 1;
            cloneSource.Runtime.SetPosition(100.75, -20.25, 200.5);
            cloneSource.Runtime.SyncIntegerPosition();
            world.Register(cloneSource);

            ulong nativeCallsBefore = world.NativeRandom
                .CaptureScalarState()
                .SynchronizedCalls;
            uint legacyStateBefore = world.Rng.State;
            ulong legacyCallsBefore = world.Rng.CallCount;
            world.LateEntityUpdateAll(1);

            report.transitionOid = transitionSource.ObjectId;
            report.transitionAction = transitionSource.Frame.N;
            report.transitionType =
                transitionSource.GetCurrentDataObjectTypeForSimulation();
            report.nativeCalls = world.NativeRandom
                .CaptureScalarState()
                .SynchronizedCalls - nativeCallsBefore;
            report.legacyCalls = world.Rng.CallCount - legacyCallsBefore;
            report.cloneCount = 0;
            report.birthDefaults = true;
            for (int index = 0; index < 5; index++)
            {
                LF2Entity child = world.FindEntityByRuntimeSlotIncludingPending(
                    50 + index);
                if (child == null)
                {
                    report.birthDefaults = false;
                    continue;
                }

                report.cloneCount++;
                report.birthDefaults &=
                    child.ObjectId == (index == 4 ? 218 : 217) &&
                    child.Health.HP == 10 &&
                    child.Health.HPBound == 10 &&
                    child.Health.HP3 == 10 &&
                    child.Health.PP == 10 &&
                    child.SpawnerEntityIndex == -1 &&
                    child.OwnerEntityIndex == -1 &&
                    child.AttackExempt == 6;
            }

            Require(transitionSource.ObjectId == 7 &&
                    transitionSource.Frame.N == 3 &&
                    transitionSource.Trans.WaitCounter == 3 &&
                    transitionSource.AttackingCounter == 0 &&
                    transitionSource.Runtime.PrevFrame2 == 3 &&
                    transitionSource.Runtime.RenderPicOffset == 0 &&
                    transitionSource.Runtime.WeaponFlightCounter == 321 &&
                    report.transitionType ==
                        (int)LF2ObjectType.SpecialAttack,
                "C25a Play transaction does not match the native source-next definition transition.");
            Require(report.nativeCalls == 34 &&
                    report.legacyCalls == 0 &&
                    world.Rng.State == legacyStateBefore &&
                    report.cloneCount == 5 &&
                    report.birthDefaults,
                "C25b Play transaction does not match synchronized RNG or native birth defaults.");

            void AddData(int oid, LF2ObjectType type, LF2CharacterData data)
            {
                wrappers.Add(oid, new LF2CharacterDataWrapper(oid, data));
                definitions.Add(new ObjectDefinition(
                    oid,
                    (int)type,
                    $"play-c25-{oid}.dat"));
            }

            void AddWeapon(int oid)
            {
                LF2CharacterData data = Data(
                    $"Play_C25b_Weapon_{oid}",
                    LF2ObjectType.LightWeapon,
                    Frame(0, LF2States.WeaponInSky, 0),
                    Frame(1, LF2States.WeaponInSky, 1),
                    Frame(2, LF2States.WeaponInSky, 2),
                    Frame(3, LF2States.WeaponInSky, 3));
                data.weapon_hp = 700 + oid;
                AddData(oid, LF2ObjectType.LightWeapon, data);
            }
        }

        private static LF2Character CreateCharacter(
            LF2CharacterDataWrapper wrapper,
            int runtimeSlot,
            int frameId)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = wrapper.characterData.name;
            character.ObjectId = wrapper.characterId;
            character.FrameCache.Load(wrapper);
            character.Initialize(500, 500);
            character.Frame.N = frameId;
            character.Frame.PN = frameId;
            character.Frame.D = character.FrameCache.GetFrameDataById(frameId);
            character.Frame.Prev = frameId;
            character.Frame.Prev2 = frameId;
            character.Frame.Prev2D = character.Frame.D;
            character.Trans.SyncDirectFrameData(
                character.Frame.D.wait,
                character.Frame.D.next,
                frameId);
            character.SetRequiredRuntimeSlot(runtimeSlot);
            character.Runtime.SuppressLateFrameTickUntilTick = 100;
            character.RefreshRuntimeSnapshot();
            return character;
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

        private static LF2FrameData Frame(int frameId, int state, int next)
        {
            return new LF2FrameData
            {
                frameId = frameId,
                state = state,
                wait = 100,
                next = next,
                centerx = 39,
                centery = 79,
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            File.WriteAllText(
                ProjectPath(ResultPath),
                JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C25DefinitionClonePlayProbe] PASS");
            else
                Debug.LogError(
                    "[NTSD28C25DefinitionClonePlayProbe] " +
                    report.message);
        }

        private static void ExitPlayModeAfterRequest()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", relativePath));
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int transitionOid;
            public int transitionAction;
            public int transitionType;
            public ulong nativeCalls;
            public ulong legacyCalls;
            public int cloneCount;
            public bool birthDefaults;
        }
    }
}
#endif
