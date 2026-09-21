#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Reflection;
using NTSD.App;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test
{
    public static class NTSD28B11SourceCallerPlaySetup
    {
        [Serializable] private sealed class Request
        {
            public bool requested;
            public bool formalStaged;
            public string runId, mode, root, configAssetPath;
        }

        public static string AttemptedRunId { get; private set; }
        public static string Failure { get; private set; }
        public static GameConfig PreviousConfig { get; private set; }
        public static GameConfig OwnedConfig { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            string path = Path.Combine(root, "Temp/NTSD28_Q07_StagedCaller.request.json");
            if (!File.Exists(path) || !JsonUtility.FromJson<Request>(File.ReadAllText(path)).requested)
                path = Path.Combine(root, "Temp/NTSD28_B11_SourceCaller.request.json");
            if (!File.Exists(path)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            if (request == null || !request.requested) return;
            AttemptedRunId = request.runId;
            Failure = null;
            try
            {
                GameConfig template = AssetDatabase.LoadAssetAtPath<GameConfig>(request.configAssetPath);
                if (template == null || string.IsNullOrEmpty(request.root))
                    throw new InvalidOperationException("Play source setup requires a prepared root and a current GameConfig asset.");
                PreviousConfig = GameConfig.Instance;
                OwnedConfig = UnityEngine.Object.Instantiate(template);
                OwnedConfig.name = "E3_Probe_GameConfig";
                OwnedConfig.BattleContentRuntimeRoot = request.root;
                typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, OwnedConfig);
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = request.mode != "direct";
            }
            catch (Exception error)
            {
                Failure = error.ToString();
            }
        }
    }
}
#endif
