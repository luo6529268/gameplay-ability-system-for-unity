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

        static BattleRuntimeSelfCheckEditor()
        {
            EditorApplication.delayCall += RunIfRequested;
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

        private static void RunIfRequested()
        {
            string requestPath = ProjectPath(RequestFile);
            if (!File.Exists(requestPath))
                return;

            try
            {
                File.Delete(requestPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BattleRuntimeSelfCheckEditor] 删除请求文件失败: {ex.Message}");
            }

            RunAndWriteResult(exitBatchmode: false);
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
            File.WriteAllText(ProjectPath(ResultFile), content);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }
    }
}
#endif
