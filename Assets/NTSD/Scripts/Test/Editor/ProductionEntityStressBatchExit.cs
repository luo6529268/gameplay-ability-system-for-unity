#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using NTSD.Animation.Rendering.Editor;
using NTSD.Test;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.EditorTools
{
    internal enum ProductionEntityStressBatchAction
    {
        Wait,
        OpenBattleScene,
        PrepareRequestAndEnterPlayMode,
        EnterPlayMode,
        DispatchRequest,
        MonitorResult,
    }

    [InitializeOnLoad]
    internal static class ProductionEntityStressBatchExit
    {
        private const string CommandLineFlag = "-ntsdProductionStressBatchExit";
        private const string RequestPathArgument = "-ntsdProductionStressRequestPath";
        private const string BattleScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string ResultFile = "Temp/NTSD_ProductionEntityStress.result";
        private const double PollIntervalSeconds = 0.25d;
        private const double TimeoutSeconds = 30d * 60d;
        private const string RequestPathSessionKey =
            "NTSD.ProductionEntityStress.BatchExit.RequestPath";
        private const string RequestPreparedSessionKey =
            "NTSD.ProductionEntityStress.BatchExit.RequestPrepared";
        private const string EnteredPlayModeSessionKey =
            "NTSD.ProductionEntityStress.BatchExit.EnteredPlayMode";
        private const string RequestDispatchedSessionKey =
            "NTSD.ProductionEntityStress.BatchExit.RequestDispatched";
        private const string DeadlineSessionKey =
            "NTSD.ProductionEntityStress.BatchExit.DeadlineRealtime";

        private static string requestSourcePath;
        private static double deadlineRealtimeSeconds;
        private static double nextPollAtSeconds;
        private static bool finished;

        static ProductionEntityStressBatchExit()
        {
            if (!Application.isBatchMode || !HasCommandLineFlag())
                return;

            try
            {
                requestSourcePath = ResolveAndPersistRequestSourcePath();
                deadlineRealtimeSeconds = RestoreOrCreateDeadline();
                nextPollAtSeconds = EditorApplication.timeSinceStartup;
                if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                    RestoreBootstrapSuppressionForPlayDomain();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[ProductionEntityStressBatchExit] Failed to initialize: {exception}");
                EditorApplication.delayCall += ExitAfterInitializationFailure;
                return;
            }

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += UpdateBatchLifecycle;
        }

        private static bool HasCommandLineFlag()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (string.Equals(
                        arguments[i],
                        CommandLineFlag,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ResolveAndPersistRequestSourcePath()
        {
            string requestPath = ReadArgument(RequestPathArgument);
            if (string.IsNullOrWhiteSpace(requestPath))
                requestPath = SessionState.GetString(RequestPathSessionKey, string.Empty);
            if (string.IsNullOrWhiteSpace(requestPath))
            {
                throw new ArgumentException(
                    $"Missing required command-line argument '{RequestPathArgument} <path>'.");
            }

            string absoluteRequestPath = Path.IsPathRooted(requestPath)
                ? Path.GetFullPath(requestPath)
                : ProjectPath(requestPath);
            if (!File.Exists(absoluteRequestPath))
            {
                throw new FileNotFoundException(
                    "The production stress request JSON does not exist.",
                    absoluteRequestPath);
            }

            string persistedPath = SessionState.GetString(RequestPathSessionKey, string.Empty);
            if (!string.Equals(
                    persistedPath,
                    absoluteRequestPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                SessionState.SetString(RequestPathSessionKey, absoluteRequestPath);
                SessionState.SetBool(RequestPreparedSessionKey, false);
                SessionState.SetBool(EnteredPlayModeSessionKey, false);
                SessionState.SetBool(RequestDispatchedSessionKey, false);
                SessionState.EraseString(DeadlineSessionKey);
            }

            return absoluteRequestPath;
        }

        private static double RestoreOrCreateDeadline()
        {
            string persisted = SessionState.GetString(DeadlineSessionKey, string.Empty);
            if (double.TryParse(
                    persisted,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double deadline))
            {
                return deadline;
            }

            deadline = Time.realtimeSinceStartupAsDouble + TimeoutSeconds;
            SessionState.SetString(
                DeadlineSessionKey,
                deadline.ToString("R", CultureInfo.InvariantCulture));
            return deadline;
        }

        private static ProductionEntityStressRequest LoadValidatedRequest()
        {
            string json = File.ReadAllText(requestSourcePath, Encoding.UTF8);
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(json);
            if (request == null)
                throw new InvalidDataException("The production stress request JSON parsed to null.");

            ProductionEntityStressConfig.FromRequest(request, ProjectPath(string.Empty));
            return request;
        }

        private static void ApplyBootstrapSuppression(ProductionEntityStressRequest request)
        {
            BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                ProductionEntityStressRequestProcessor.ShouldSuppressBattleTestBootstrap(
                    request.action);
        }

        private static void RestoreBootstrapSuppressionForPlayDomain()
        {
            ProductionEntityStressRequest request = LoadValidatedRequest();
            ApplyBootstrapSuppression(request);
            SessionState.SetBool(RequestPreparedSessionKey, true);
        }

        private static void PrepareRequestForPlayMode()
        {
            ProductionEntityStressRequest request = LoadValidatedRequest();
            ApplyBootstrapSuppression(request);
            SessionState.SetBool(RequestPreparedSessionKey, true);
        }

        private static void DispatchRequestAfterEnteredPlayMode()
        {
            if (SessionState.GetBool(RequestDispatchedSessionKey, false))
                return;

            ProductionEntityStressRequest request = LoadValidatedRequest();
            ApplyBootstrapSuppression(request);
            ProductionEntityStressRequestProcessor.WriteRequest(request);
            SessionState.SetBool(RequestDispatchedSessionKey, true);
        }

        private static string ReadArgument(string argumentName)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (!string.Equals(
                        arguments[i],
                        argumentName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (i + 1 < arguments.Length &&
                    !string.IsNullOrWhiteSpace(arguments[i + 1]))
                {
                    return arguments[i + 1];
                }

                return string.Empty;
            }

            return string.Empty;
        }

        private static void EnsureBattleSceneIsActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (string.Equals(activeScene.path, BattleScenePath, StringComparison.Ordinal))
                return;

            string absoluteScenePath = ProjectPath(BattleScenePath);
            if (!File.Exists(absoluteScenePath))
            {
                throw new FileNotFoundException(
                    "The production stress battle scene does not exist.",
                    absoluteScenePath);
            }

            Scene openedScene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            if (!openedScene.IsValid() || !openedScene.isLoaded)
                throw new InvalidOperationException($"Unity did not load '{BattleScenePath}'.");

            activeScene = SceneManager.GetActiveScene();
            if (!string.Equals(activeScene.path, BattleScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Active scene is '{activeScene.path}' instead of '{BattleScenePath}'.");
            }
        }

        internal static ProductionEntityStressBatchAction ResolveLifecycleAction(
            bool isPlaying,
            bool isPlayingOrWillChangePlaymode,
            bool isCompiling,
            bool isUpdating,
            bool battleSceneActive,
            bool requestPrepared,
            bool enteredPlayMode,
            bool requestDispatched)
        {
            if (isPlaying)
            {
                if (!enteredPlayMode)
                    return ProductionEntityStressBatchAction.Wait;
                return requestDispatched
                    ? ProductionEntityStressBatchAction.MonitorResult
                    : ProductionEntityStressBatchAction.DispatchRequest;
            }
            if (isPlayingOrWillChangePlaymode || isCompiling || isUpdating)
                return ProductionEntityStressBatchAction.Wait;
            if (!battleSceneActive)
                return ProductionEntityStressBatchAction.OpenBattleScene;
            return requestPrepared
                ? ProductionEntityStressBatchAction.EnterPlayMode
                : ProductionEntityStressBatchAction.PrepareRequestAndEnterPlayMode;
        }

        private static void UpdateBatchLifecycle()
        {
            if (finished)
                return;

            if (Time.realtimeSinceStartupAsDouble >= deadlineRealtimeSeconds)
            {
                Finish(
                    1,
                    "Timed out after 30 minutes waiting for the production stress result.");
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            ProductionEntityStressBatchAction action = ResolveLifecycleAction(
                Application.isPlaying,
                EditorApplication.isPlayingOrWillChangePlaymode,
                EditorApplication.isCompiling,
                EditorApplication.isUpdating,
                string.Equals(activeScene.path, BattleScenePath, StringComparison.Ordinal),
                SessionState.GetBool(RequestPreparedSessionKey, false),
                SessionState.GetBool(EnteredPlayModeSessionKey, false),
                SessionState.GetBool(RequestDispatchedSessionKey, false));

            try
            {
                switch (action)
                {
                    case ProductionEntityStressBatchAction.Wait:
                        return;
                    case ProductionEntityStressBatchAction.OpenBattleScene:
                        EnsureBattleSceneIsActive();
                        return;
                    case ProductionEntityStressBatchAction.PrepareRequestAndEnterPlayMode:
                        PrepareRequestForPlayMode();
                        EnterPlayMode();
                        return;
                    case ProductionEntityStressBatchAction.EnterPlayMode:
                        EnterPlayMode();
                        return;
                    case ProductionEntityStressBatchAction.DispatchRequest:
                        DispatchRequestAfterEnteredPlayMode();
                        return;
                    case ProductionEntityStressBatchAction.MonitorResult:
                        PollResult(EditorApplication.timeSinceStartup);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception exception)
            {
                Finish(1, $"Batch lifecycle failed: {exception}");
            }
        }

        private static void EnterPlayMode()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.EnterPlaymode();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (finished || state != PlayModeStateChange.EnteredPlayMode)
                return;

            SessionState.SetBool(EnteredPlayModeSessionKey, true);
            try
            {
                DispatchRequestAfterEnteredPlayMode();
            }
            catch (Exception exception)
            {
                Finish(1, $"Failed to dispatch the request after EnteredPlayMode: {exception}");
            }
        }

        private static void PollResult(double now)
        {
            if (now < nextPollAtSeconds)
                return;

            nextPollAtSeconds = now + PollIntervalSeconds;
            string resultPath = ProjectPath(ResultFile);
            if (!File.Exists(resultPath))
                return;

            string firstLine;
            try
            {
                using var stream = new FileStream(
                    resultPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true);
                firstLine = reader.ReadLine()?.Trim();
            }
            catch (IOException)
            {
                return;
            }
            catch (UnauthorizedAccessException exception)
            {
                Finish(1, $"Cannot read the production stress result: {exception.Message}");
                return;
            }

            if (string.Equals(firstLine, "RUNNING", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(firstLine))
            {
                return;
            }
            if (string.Equals(firstLine, "PASS", StringComparison.OrdinalIgnoreCase))
            {
                Finish(0, "Production stress reported PASS.");
                return;
            }
            if (string.Equals(firstLine, "FAIL", StringComparison.OrdinalIgnoreCase))
                Finish(1, "Production stress reported FAIL.");
        }

        private static void ExitAfterInitializationFailure()
        {
            EditorApplication.delayCall -= ExitAfterInitializationFailure;
            ClearPersistedState();
            EditorApplication.Exit(1);
        }

        private static void Finish(int exitCode, string message)
        {
            if (finished)
                return;

            finished = true;
            EditorApplication.update -= UpdateBatchLifecycle;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            if (exitCode == 0)
                Debug.Log($"[ProductionEntityStressBatchExit] {message}");
            else
                Debug.LogError($"[ProductionEntityStressBatchExit] {message}");
            ClearPersistedState();
            EditorApplication.Exit(exitCode);
        }

        private static void ClearPersistedState()
        {
            SessionState.EraseString(RequestPathSessionKey);
            SessionState.EraseBool(RequestPreparedSessionKey);
            SessionState.EraseBool(EnteredPlayModeSessionKey);
            SessionState.EraseBool(RequestDispatchedSessionKey);
            SessionState.EraseString(DeadlineSessionKey);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }
    }
}
#endif
