#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NTSD.Animation.Rendering.Editor
{
    internal static class ProductionEntityStressPlayerBootstrap
    {
        private const string RequestArgument = "--ntsd-production-stress-request";
        private static string requestedPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PrepareWhenRequested()
        {
            requestedPath = ReadArgument(
                Environment.GetCommandLineArgs(),
                RequestArgument);
            if (!string.IsNullOrWhiteSpace(requestedPath))
            {
                NTSD.Test.BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                    true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallWhenRequested()
        {
            if (string.IsNullOrWhiteSpace(requestedPath))
                return;

            var host = new GameObject("NTSD Production Entity Stress Player Bootstrap");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<ProductionEntityStressPlayerBootstrapHost>()
                .Initialize(Path.GetFullPath(requestedPath));
        }

        private static string ReadArgument(string[] arguments, string name)
        {
            if (arguments == null)
                return string.Empty;

            for (int i = 0; i + 1 < arguments.Length; i++)
            {
                if (string.Equals(arguments[i], name, StringComparison.Ordinal))
                    return arguments[i + 1];
            }

            return string.Empty;
        }
    }

    internal sealed class ProductionEntityStressPlayerBootstrapHost : MonoBehaviour
    {
        private const double StartupTimeoutSeconds = 180d;
        private readonly Stopwatch startupStopwatch = new Stopwatch();
        private string requestPath;
        private ProductionEntityStressRequest request;
        private ProductionEntityStressConfig config;
        private bool runStarted;
        private bool quitting;

        internal void Initialize(string absoluteRequestPath)
        {
            requestPath = absoluteRequestPath;
            startupStopwatch.Start();
        }

        private void Update()
        {
            if (quitting)
                return;

            try
            {
                if (!runStarted)
                {
                    TryStartRun();
                    return;
                }

                if (ProductionEntityStressRunner.Active == null)
                    QuitFromFinalReport();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[ProductionEntityStressPlayerBootstrap] Failed: " + exception);
                Quit(2);
            }
        }

        private void TryStartRun()
        {
            if (!ProductionEntityStressRunner.AreProductionServicesReady())
            {
                if (startupStopwatch.Elapsed.TotalSeconds >= StartupTimeoutSeconds)
                {
                    throw new TimeoutException(
                        "Production stress services were not ready within " +
                        StartupTimeoutSeconds + " seconds.");
                }

                return;
            }

            if (!File.Exists(requestPath))
            {
                throw new FileNotFoundException(
                    "Production stress request file was not found.",
                    requestPath);
            }

            request = JsonUtility.FromJson<ProductionEntityStressRequest>(
                File.ReadAllText(requestPath));
            if (request == null)
            {
                throw new InvalidOperationException(
                    "Production stress request JSON could not be parsed: " + requestPath);
            }

            request.autoStopWhenSampled = true;
            config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            ProductionEntityStressRunner.StartRun(config);
            runStarted = true;
            Debug.Log(
                "[ProductionEntityStressPlayerBootstrap] Started " +
                config.Mode + "/" + config.InputMode + ", entities=" +
                config.EntityCount + ", output=" + config.OutputPath + ".");
        }

        private void QuitFromFinalReport()
        {
            if (!File.Exists(config.OutputPath))
            {
                throw new FileNotFoundException(
                    "Production stress runner stopped without a final report.",
                    config.OutputPath);
            }

            ProductionEntityStressReport report =
                JsonUtility.FromJson<ProductionEntityStressReport>(
                    File.ReadAllText(config.OutputPath));
            bool terminalStatus = report != null &&
                                  (string.Equals(
                                       report.status,
                                       "StoppedCleanly",
                                       StringComparison.Ordinal) ||
                                   string.Equals(
                                       report.status,
                                       "SmokePassed",
                                       StringComparison.Ordinal));
            bool passed = terminalStatus &&
                          report.harnessValidity &&
                          report.sampledLogicTicks >= config.SampleTicks &&
                          report.zeroGcGatePassed &&
                          (config.SimulationOnly ||
                           report.centralFrameSubmissionDrawCount != null &&
                           report.centralFrameSubmissionDrawCount.available &&
                           report.centralFrameSubmissionDrawCount.maximum > 0d &&
                           report.centralFrameSubmittedPixelsFrameCount > 0) &&
                          report.teardown != null &&
                          report.teardown.restored;
            if (!passed)
            {
                Debug.LogError(
                    "[ProductionEntityStressPlayerBootstrap] Runtime gate failed: " +
                    "status=" + report?.status +
                    ", failure=" + report?.failure +
                    ", report=" + config.OutputPath + ".");
                Quit(3);
                return;
            }

            Debug.Log(
                "[ProductionEntityStressPlayerBootstrap] PASS: " + config.OutputPath);
            Quit(0);
        }

        private void Quit(int exitCode)
        {
            quitting = true;
            Application.Quit(exitCode);
        }
    }
}
#endif
