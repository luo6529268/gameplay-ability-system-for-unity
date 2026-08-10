#if UNITY_EDITOR
using NTSD.Test;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NTSD.EditorTools
{
    /// <summary>
    /// 战斗运行时自检的编辑器入口。
    /// 支持菜单执行，也支持 Unity batchmode 的 -executeMethod 调用。
    /// </summary>
    [InitializeOnLoad]
    public static class BattleRuntimeSelfCheckEditor
    {
        private const string RequestFile = "Temp/NTSD_BattleRuntimeSelfCheck.request";
        private const string ResultFile = "Temp/NTSD_BattleRuntimeSelfCheck.result";
        private static bool requestRunInProgress;
        private static bool staleResultDeleteWarningLogged;
        private static readonly string RequestAbsolutePath = ProjectPath(RequestFile);
        private static readonly string ResultAbsolutePath = ProjectPath(ResultFile);

        static BattleRuntimeSelfCheckEditor()
        {
            EditorApplication.update += PollRequest;
        }

        [MenuItem("NTSD/验证/运行战斗运行时自检")]
        public static void RunFromMenu()
        {
            RunAndWriteResult(exitBatchmode: false);
        }

        public static void RunForBatchmode()
        {
            RunAndWriteResult(exitBatchmode: true);
        }

        private static void PollRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (requestRunInProgress)
                return;

            if (!File.Exists(RequestAbsolutePath))
            {
                staleResultDeleteWarningLogged = false;
                return;
            }

            // A pending request invalidates any result left by an earlier run.
            try
            {
                if (File.Exists(ResultAbsolutePath))
                    File.Delete(ResultAbsolutePath);
                staleResultDeleteWarningLogged = false;
            }
            catch (Exception ex)
            {
                if (!staleResultDeleteWarningLogged)
                {
                    Debug.LogWarning($"[BattleRuntimeSelfCheckEditor] Failed to delete stale result file: {ex.Message}");
                    staleResultDeleteWarningLogged = true;
                }
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            requestRunInProgress = true;
            try
            {
                try
                {
                    File.Delete(RequestAbsolutePath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BattleRuntimeSelfCheckEditor] Failed to delete request file: {ex.Message}");
                    return;
                }

                RunAndWriteResult(exitBatchmode: false);
            }
            finally
            {
                requestRunInProgress = false;
            }
        }

        private static void RunAndWriteResult(bool exitBatchmode)
        {
            try
            {
                BattleRuntimeSelfCheck.RunAllChecksStatic();
                WriteResult("PASS");
                Debug.Log("[BattleRuntimeSelfCheckEditor] 自检完成。");
            }
            catch (Exception ex)
            {
                WriteResult("FAIL\n" + ex);
                if (exitBatchmode && Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }

            if (exitBatchmode && Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        private static void WriteResult(string content)
        {
            File.WriteAllText(ResultAbsolutePath, content);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }
    }
}
#endif
