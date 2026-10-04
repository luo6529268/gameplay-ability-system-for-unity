#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q12ReentryLifecycleWitnessEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string SessionKey = "NTSD.Q12.ReentryLifecycleWitness.RunId";
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q12-REENTRY-LIFECYCLE-WITNESS-001";

        [Serializable]
        private sealed class Snapshot
        {
            public string runId;
            public string phase;
            public string utc;
            public string scenePath;
            public string sceneSha256;
            public bool sceneDirty;
            public int driverCount;
            public int driverInstanceId;
            public bool worldPresent;
            public int driverTick;
            public int worldTick;
            public int activeEntities;
            public int activeCharacters;
            public int referencePoolActive;
            public int poolCount;
            public int activePooledObjects;
            public int activePooledSprites;
            public bool poolQuiesced;
            public bool noLiveWorld;
        }

        [MenuItem("NTSD/Diagnostics/Q12 Capture Reentry Live")]
        private static void CaptureLive()
        {
            Require(EditorApplication.isPlaying && !EditorApplication.isCompiling,
                "Capture Live requires a stable Play Mode.");
            Require(string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "The previous Q12 cycle has no exit snapshot.");

            string runId = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") + "-" +
                Guid.NewGuid().ToString("N");
            Snapshot snapshot = Capture(runId, "LIVE");
            Require(snapshot.driverCount == 1 && snapshot.worldPresent &&
                snapshot.driverTick > 0 && snapshot.worldTick > 0 &&
                snapshot.activeCharacters > 0,
                "Live production World, tick, or character precondition failed.");
            Save(snapshot);
            SessionState.SetString(SessionKey, runId);
        }

        [MenuItem("NTSD/Diagnostics/Q12 Capture Reentry Exit")]
        private static void CaptureExit()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling,
                "Capture Exit requires completed Edit Mode.");
            string runId = SessionState.GetString(SessionKey, string.Empty);
            Require(!string.IsNullOrEmpty(runId), "No matching Q12 live snapshot.");

            Snapshot snapshot = Capture(runId, "EXIT");
            Require(snapshot.noLiveWorld && snapshot.activePooledObjects == 0 &&
                snapshot.activePooledSprites == 0 && !snapshot.sceneDirty,
                "Exit retained a live World, pooled object, sprite, or dirty Scene.");
            Save(snapshot);
            SessionState.EraseString(SessionKey);
        }

        private static Snapshot Capture(string runId, string phase)
        {
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene, "The active Scene is not NTSD_Battle.");

            SimulationTickDriver[] drivers = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                .Where(value => value != null && !EditorUtility.IsPersistent(value) &&
                    value.gameObject.scene == scene).ToArray();
            LF2ObjectPool[] pools = Resources.FindObjectsOfTypeAll<LF2ObjectPool>()
                .Where(value => value != null && !EditorUtility.IsPersistent(value) &&
                    value.gameObject.scene == scene).ToArray();
            SimulationTickDriver driver = drivers.Length == 1 ? drivers[0] : null;
            SimulationWorld world = driver?.World;
            int entityCount = 0;
            int characterCount = 0;
            if (world != null)
            {
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (entity == null) continue;
                    entityCount++;
                    if (entity is LF2Character) characterCount++;
                }
            }

            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", BattleScene));
            string sha256;
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);

            return new Snapshot
            {
                runId = runId,
                phase = phase,
                utc = DateTime.UtcNow.ToString("O"),
                scenePath = scene.path,
                sceneSha256 = sha256,
                sceneDirty = scene.isDirty,
                driverCount = drivers.Length,
                driverInstanceId = driver != null ? driver.GetInstanceID() : 0,
                worldPresent = world != null,
                driverTick = driver?.CurrentTickIndex ?? -1,
                worldTick = world?.CurrentTickIndex ?? -1,
                activeEntities = entityCount,
                activeCharacters = characterCount,
                referencePoolActive = world?.LogicReferencePool?.ActiveCount ?? 0,
                poolCount = pools.Length,
                activePooledObjects = pools.Sum(value => value.ActiveObjectCountForAcceptance),
                activePooledSprites = pools.Sum(value => value.ActiveSpriteCountForAcceptance),
                poolQuiesced = pools.Length > 0 && pools.All(value => value.IsQuiescedForDiagnostics),
                noLiveWorld = drivers.All(value => value.World == null)
            };
        }

        private static void Save(Snapshot snapshot)
        {
            string directory = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", OutputRoot));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory,
                "cycle-" + snapshot.runId + "-" + snapshot.phase.ToLowerInvariant() + ".json");
            using (FileStream stream = new FileStream(path, FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(snapshot, true));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
