#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q01WordsGameViewRequestEditor
    {
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string RequestGlob = "NTSD28_Q01_WordsGameView.request*.json";
        private const string ResultFolder =
            "artifacts/diagnostics/NTSD28-336B44-Q01-WORDS-GAMEVIEW-001";
        private const string SessionKey = "NTSD.Q01.WordsGameViewRequest";
        private static readonly string[] ProtectedPaths =
        {
            "Assets/NTSD/Scene/NTSD_Battle.unity",
            MenuScene,
            "Assets/NTSD/Config/GameConfig/GameConfig.asset",
            "Assets/NTSD/Resources/ProjectBattleModeConfig.asset"
        };

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool editorIdleConfirmed;
            public string runId;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string reason;
            public int captureTick;
            public int publishedTick;
            public int planTick;
            public bool hasBodyCommand;
            public bool hasLabelCommand;
            public bool hasSelectedWordBinding;
            public int selectedWordSheet;
            public string selectedLabelChar;
            public int screenCaptureWidth;
            public int screenCaptureHeight;
            public string screenCapturePath;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string phase;
            public string status;
            public string reason;
            public string startedUtc;
            public string initialScene;
            public string finalScene;
            public bool initialSceneDirty;
            public bool finalSceneDirty;
            public int initialSceneCount;
            public int finalSceneCount;
            public bool enteredPlay;
            public bool exitedPlay;
            public string probePath;
            public string probeStatus;
            public string screenshotPath;
            public bool screenshotExists;
            public bool protectedHashesStable;
            public string[] beforeHashes;
            public string[] afterHashes;
        }

        private static Report report;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static string ProjectPath(string relative)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));
        }

        private static string ResultPath(string runId)
        {
            return ProjectPath(ResultFolder + "/" + runId + ".json");
        }

        private static string[] HashProtected()
        {
            var hashes = new string[ProtectedPaths.Length];
            for (int index = 0; index < ProtectedPaths.Length; index++)
            {
                using (var sha = SHA256.Create())
                using (var stream = File.OpenRead(ProjectPath(ProtectedPaths[index])))
                {
                    hashes[index] = BitConverter.ToString(sha.ComputeHash(stream))
                        .Replace("-", "");
                }
            }
            return hashes;
        }

        private static void SaveSession()
        {
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        }

        private static void RestoreSession()
        {
            if (report != null)
                return;
            string saved = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            try
            {
                RestoreSession();
                if (report == null)
                {
                    TryStart();
                    return;
                }

                if (DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() >
                    TimeSpan.FromMinutes(6))
                {
                    Fail("The natural WORDS Game View request timed out.");
                    return;
                }

                if (report.phase == "ENTERING")
                {
                    if (!EditorApplication.isPlaying)
                        return;
                    if (SceneManager.GetActiveScene().path != MenuScene)
                        throw new InvalidOperationException("Play did not retain the Menu entry Scene.");
                    NTSD28Q09NameplateNaturalPlayProbeEditor.RunWordsGameViewCapture();
                    FieldInfo pathField = typeof(NTSD28Q09NameplateNaturalPlayProbeEditor)
                        .GetField("activeResultPath", BindingFlags.NonPublic | BindingFlags.Static);
                    report.probePath = pathField?.GetValue(null) as string;
                    if (string.IsNullOrEmpty(report.probePath))
                        throw new InvalidOperationException("The existing nameplate probe path is unavailable.");
                    report.enteredPlay = true;
                    report.phase = "OBSERVING";
                    SaveSession();
                    return;
                }

                if (report.phase == "OBSERVING")
                {
                    if (File.Exists(report.probePath))
                    {
                        ProbeResult probe = JsonUtility.FromJson<ProbeResult>(
                            File.ReadAllText(report.probePath));
                        if (probe != null && probe.status != "RUNNING")
                        {
                            report.probeStatus = probe.status;
                            report.screenshotPath = probe.screenCapturePath;
                            report.phase = "EXITING";
                            SaveSession();
                        }
                    }
                    if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                        report.phase == "OBSERVING")
                        throw new InvalidOperationException("Play ended before the nameplate probe finished.");
                    return;
                }

                if (report.phase == "EXITING" &&
                    !EditorApplication.isPlayingOrWillChangePlaymode)
                    Finish();
            }
            catch (Exception error)
            {
                Fail(error.ToString());
            }
        }

        private static void TryStart()
        {
            string temp = ProjectPath("Temp");
            foreach (string path in Directory.GetFiles(temp, RequestGlob))
            {
                Request request;
                try
                {
                    request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
                }
                catch (IOException)
                {
                    continue;
                }
                if (request == null || !request.requested ||
                    string.IsNullOrEmpty(request.runId) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(
                        request.runId, "^[a-zA-Z0-9-]{1,80}$") ||
                    File.Exists(ResultPath(request.runId)))
                    continue;

                Scene scene = SceneManager.GetActiveScene();
                report = new Report
                {
                    runId = request.runId,
                    phase = "PREFLIGHT",
                    status = "RUNNING",
                    startedUtc = DateTime.UtcNow.ToString("O"),
                    initialScene = scene.path,
                    initialSceneDirty = scene.isDirty,
                    initialSceneCount = SceneManager.sceneCount,
                    beforeHashes = HashProtected()
                };
                SaveSession();
                if (!request.editorIdleConfirmed ||
                    EditorApplication.isPlayingOrWillChangePlaymode ||
                    scene.path != MenuScene || scene.isDirty ||
                    SceneManager.sceneCount != 1)
                {
                    Fail("Preflight requires one clean saved Menu Scene in idle Edit Mode.");
                    return;
                }
                report.phase = "ENTERING";
                SaveSession();
                EditorApplication.EnterPlaymode();
                return;
            }
        }

        private static void Fail(string reason)
        {
            if (report == null)
                return;
            report.status = "FAIL";
            report.reason = reason;
            report.phase = "EXITING";
            SaveSession();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else if (!EditorApplication.isPlayingOrWillChangePlaymode)
                Finish();
        }

        private static void Finish()
        {
            Scene scene = SceneManager.GetActiveScene();
            report.finalScene = scene.path;
            report.finalSceneDirty = scene.isDirty;
            report.finalSceneCount = SceneManager.sceneCount;
            report.exitedPlay = report.enteredPlay && !EditorApplication.isPlaying;
            report.afterHashes = HashProtected();
            report.protectedHashesStable = true;
            for (int index = 0; index < ProtectedPaths.Length; index++)
                report.protectedHashesStable &=
                    report.beforeHashes[index] == report.afterHashes[index];
            report.screenshotExists = !string.IsNullOrEmpty(report.screenshotPath) &&
                File.Exists(report.screenshotPath) &&
                new FileInfo(report.screenshotPath).Length > 24;
            if (report.status != "FAIL")
            {
                ProbeResult probe = File.Exists(report.probePath)
                    ? JsonUtility.FromJson<ProbeResult>(File.ReadAllText(report.probePath))
                    : null;
                bool visualPass = probe != null && probe.status == "PASS" &&
                    probe.hasBodyCommand && probe.hasLabelCommand &&
                    probe.hasSelectedWordBinding &&
                    !string.IsNullOrEmpty(probe.selectedLabelChar) &&
                    probe.captureTick == probe.publishedTick &&
                    probe.publishedTick == probe.planTick &&
                    probe.screenCaptureWidth > 0 &&
                    probe.screenCaptureHeight > 0 && report.screenshotExists;
                report.status = visualPass && report.exitedPlay &&
                    scene.path == MenuScene && !scene.isDirty &&
                    SceneManager.sceneCount == 1 && report.protectedHashesStable
                    ? "PASS" : "FAIL";
                report.reason = report.status == "PASS"
                    ? "Current natural WORDS glyph and same-tick Game View captured."
                    : "Nameplate visual or Editor protection postcondition failed.";
            }
            string path = ResultPath(report.runId);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(report, true));
            SessionState.EraseString(SessionKey);
            report = null;
        }
    }
}
#endif
