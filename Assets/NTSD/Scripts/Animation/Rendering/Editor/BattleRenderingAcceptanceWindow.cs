#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingAcceptanceWindow : EditorWindow
    {
        private string outputDirectory = "Temp/P8-C-Acceptance";
        private int imageSize = 256;
        private bool exerciseLivePool;
        private int livePoolExtraCount = 1;
        private bool enterPlayMode;
        private int playModeWarmupFrames = 600;
        private bool exitPlayModeAfterRun = true;
        private string status = "Run the deterministic matrix in Edit Mode; enable live pool only in Play Mode.";

        [MenuItem("NTSD/Battle Rendering/P8-C Acceptance")]
        public static void Open()
        {
            GetWindow<BattleRenderingAcceptanceWindow>("P8-C Acceptance");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Central Rendering Correctness Matrix", EditorStyles.boldLabel);
            outputDirectory = EditorGUILayout.TextField("Output Directory", outputDirectory);
            imageSize = EditorGUILayout.IntField("Image Size", imageSize);
            exerciseLivePool = EditorGUILayout.Toggle("Exercise Live Pool", exerciseLivePool);
            using (new EditorGUI.DisabledScope(!exerciseLivePool))
                livePoolExtraCount = EditorGUILayout.IntField("Pool Extra Count", livePoolExtraCount);
            enterPlayMode = EditorGUILayout.Toggle("Enter Play Mode", enterPlayMode);
            using (new EditorGUI.DisabledScope(!enterPlayMode))
            {
                playModeWarmupFrames = EditorGUILayout.IntField("Play Warmup Frames", playModeWarmupFrames);
                exitPlayModeAfterRun = EditorGUILayout.Toggle("Exit Play Mode After Run", exitPlayModeAfterRun);
            }
            EditorGUILayout.HelpBox(status, MessageType.Info);

            if (GUILayout.Button("Write Acceptance Request"))
            {
                try
                {
                    var request = new BattleRenderingAcceptanceRequest
                    {
                        outputDirectory = outputDirectory,
                        imageSize = imageSize,
                        exerciseLivePool = exerciseLivePool,
                        livePoolExtraCount = livePoolExtraCount,
                        enterPlayMode = enterPlayMode,
                        playModeWarmupFrames = playModeWarmupFrames,
                        exitPlayModeAfterRun = exitPlayModeAfterRun,
                    };
                    BattleRenderingAcceptanceRequestProcessor.WriteRequest(request);
                    status = enterPlayMode
                        ? "Request written. The Editor will enter Play Mode, warm up, and run the matrix once."
                        : "Request written. The matrix will run on the next Editor update.";
                }
                catch (Exception exception)
                {
                    status = exception.Message;
                    Debug.LogError($"[P8-C Acceptance] Request write failed: {exception}");
                }
            }
        }
    }

    [InitializeOnLoad]
    internal static class BattleRenderingAcceptanceRequestProcessor
    {
        internal const string RequestFile = "Temp/NTSD_BattleRenderingAcceptance.request.json";
        internal const string ResultFile = "Temp/NTSD_BattleRenderingAcceptance.result";
        internal const int DefaultPlayModeWarmupFrames = 600;
        internal const int MaximumPlayModeWarmupFrames = 600;

        private const string SessionRequestJsonKey =
            "NTSD.BattleRenderingAcceptance.RequestJson";
        private const string SessionWarmupFramesKey =
            "NTSD.BattleRenderingAcceptance.WarmupFrames";
        private const string SessionCompletedKey =
            "NTSD.BattleRenderingAcceptance.Completed";

        private static bool processing;
        private static readonly string projectRoot =
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        private static readonly string RequestAbsolutePath =
            Path.GetFullPath(Path.Combine(projectRoot, RequestFile));
        private static bool requestPending = File.Exists(RequestAbsolutePath);
        private static bool polling;

        internal enum RequestAction
        {
            Execute,
            EnterPlayMode,
            WaitForPlayMode,
            WarmUp,
        }

        static BattleRenderingAcceptanceRequestProcessor()
        {
            if (requestPending)
                StartPolling();
        }

        internal static string ProjectRoot => projectRoot;

        internal static string ProjectPath(string path)
        {
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectRoot, path));
        }

        internal static void WriteRequest(BattleRenderingAcceptanceRequest request)
        {
            BattleRenderingAcceptanceConfig.FromRequest(request, ProjectRoot);
            ValidatePlayModeRequest(request);
            string path = ProjectPath(RequestFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(request), new UTF8Encoding(false));
            requestPending = true;
            StartPolling();
        }

        internal static int GetRequiredWarmupFrames(BattleRenderingAcceptanceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (!request.enterPlayMode)
                return 0;
            return request.playModeWarmupFrames <= 0
                ? DefaultPlayModeWarmupFrames
                : request.playModeWarmupFrames;
        }

        internal static RequestAction DecideRequestAction(
            BattleRenderingAcceptanceRequest request,
            bool isPlaying,
            bool isPlayingOrWillChangePlaymode,
            int completedWarmupFrames)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (!request.enterPlayMode)
                return RequestAction.Execute;
            if (!isPlaying)
            {
                return isPlayingOrWillChangePlaymode
                    ? RequestAction.WaitForPlayMode
                    : RequestAction.EnterPlayMode;
            }
            return completedWarmupFrames < GetRequiredWarmupFrames(request)
                ? RequestAction.WarmUp
                : RequestAction.Execute;
        }

        private static void PollRequest()
        {
            if (processing || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (!requestPending)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                requestPending = File.Exists(RequestAbsolutePath);
                if (!requestPending)
                {
                    ClearSessionRequest();
                    return;
                }
            }

            processing = true;
            try
            {
                string requestJson = File.ReadAllText(RequestAbsolutePath, Encoding.UTF8);
                EnsureSessionRequest(requestJson);
                BattleRenderingAcceptanceRequest request =
                    JsonUtility.FromJson<BattleRenderingAcceptanceRequest>(requestJson);
                BattleRenderingAcceptanceConfig.FromRequest(request, ProjectRoot);
                ValidatePlayModeRequest(request);

                if (SessionState.GetBool(SessionCompletedKey, false))
                {
                    if (TryDelete(RequestAbsolutePath))
                        ClearSessionRequest();
                    return;
                }
                int completedWarmupFrames = SessionState.GetInt(SessionWarmupFramesKey, 0);
                RequestAction action = DecideRequestAction(
                    request,
                    EditorApplication.isPlaying,
                    EditorApplication.isPlayingOrWillChangePlaymode,
                    completedWarmupFrames);
                switch (action)
                {
                    case RequestAction.EnterPlayMode:
                        EditorApplication.EnterPlaymode();
                        return;
                    case RequestAction.WaitForPlayMode:
                        return;
                    case RequestAction.WarmUp:
                        SessionState.SetInt(SessionWarmupFramesKey, completedWarmupFrames + 1);
                        return;
                    case RequestAction.Execute:
                        ProcessRequest(request);
                        SessionState.SetBool(SessionCompletedKey, true);
                        if (TryDelete(RequestAbsolutePath))
                            ClearSessionRequest();
                        if (request.enterPlayMode &&
                            request.exitPlayModeAfterRun &&
                            EditorApplication.isPlaying)
                        {
                            EditorApplication.ExitPlaymode();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception exception)
            {
                WriteRequestFailure(exception);
                SessionState.SetBool(SessionCompletedKey, true);
                if (TryDelete(RequestAbsolutePath))
                    ClearSessionRequest();
            }
            finally
            {
                processing = false;
            }
        }

        private static void ProcessRequest(BattleRenderingAcceptanceRequest request)
        {
            string resultPath = ProjectPath(ResultFile);
            try
            {
                BattleRenderingAcceptanceConfig config =
                    BattleRenderingAcceptanceConfig.FromRequest(request, ProjectRoot);
                BattleRenderingAcceptanceReport report =
                    BattleRenderingAcceptanceHarness.Run(config);
                string reportPath = Path.Combine(config.OutputDirectory, BattleRenderingAcceptanceHarness.ReportFileName);
                string result = (report.passed ? "PASS" : "FAIL") + "\n" + reportPath;
                WriteResult(resultPath, result);
                if (report.passed)
                    Debug.Log($"[P8-C Acceptance] PASS: {reportPath}");
                else
                    Debug.LogError($"[P8-C Acceptance] FAIL: {reportPath}");
            }
            catch (Exception exception)
            {
                WriteResult(resultPath, "FAIL\n" + exception);
                Debug.LogError($"[P8-C Acceptance] Request failed: {exception}");
            }
        }

        private static void ValidatePlayModeRequest(BattleRenderingAcceptanceRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (!request.enterPlayMode)
                return;
            if (request.playModeWarmupFrames > MaximumPlayModeWarmupFrames)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(request.playModeWarmupFrames),
                    $"Play Mode warmup frames must be at most {MaximumPlayModeWarmupFrames}.");
            }
        }

        private static void EnsureSessionRequest(string requestJson)
        {
            string activeRequestJson = SessionState.GetString(SessionRequestJsonKey, string.Empty);
            if (string.Equals(activeRequestJson, requestJson, StringComparison.Ordinal))
                return;
            SessionState.SetString(SessionRequestJsonKey, requestJson);
            SessionState.SetInt(SessionWarmupFramesKey, 0);
            SessionState.SetBool(SessionCompletedKey, false);
        }

        private static void ClearSessionRequest()
        {
            requestPending = false;
            StopPolling();
            SessionState.EraseString(SessionRequestJsonKey);
            SessionState.EraseInt(SessionWarmupFramesKey);
            SessionState.EraseBool(SessionCompletedKey);
        }

        private static void StartPolling()
        {
            if (polling)
                return;

            EditorApplication.update += PollRequest;
            polling = true;
        }

        private static void StopPolling()
        {
            if (!polling)
                return;

            EditorApplication.update -= PollRequest;
            polling = false;
        }

        private static void WriteRequestFailure(Exception exception)
        {
            string resultPath = ProjectPath(ResultFile);
            WriteResult(resultPath, "FAIL\n" + exception);
            Debug.LogError($"[P8-C Acceptance] Request failed: {exception}");
        }

        private static void WriteResult(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static bool TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                return !File.Exists(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[P8-C Acceptance] Could not delete request: {exception.Message}");
                return false;
            }
        }
    }
}
#endif
