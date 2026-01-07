#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.IO;
using NTSD.DatParser;

namespace NTSD.Animation.Editor
{
    /// <summary>
    /// Dat 文件文本编辑器
    /// 类似 LF2 IDE 的文本编辑模式
    /// </summary>
    public class DatFileEditorWindow : OdinEditorWindow
    {
        [MenuItem("NTSD/Animation/Dat File Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<DatFileEditorWindow>("📝 Dat 文件编辑器");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        #region 文件操作

        [TabGroup("Main", "📂 文件")]
        [BoxGroup("Main/📂 文件/文件选择")]
        [Sirenix.OdinInspector.FilePath(Extensions = "dat,txt")]
        [LabelText("Dat 文件路径")]
        public string datFilePath = "";

        [BoxGroup("Main/📂 文件/文件选择")]
        [LabelText("解密密钥")]
        [DetailedInfoBox("LF2/NTSD 标准密钥", "此密钥用于解密和加密 LF2/NTSD 的 DAT 文件。标准密钥为：odBearBecauseHeIsVeryGoodSiuHungIsAGo", InfoMessageType.Info)]
        public string encryptionKey = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";

        [PropertySpace(10)]
        [TabGroup("Main", "📂 文件")]
        [BoxGroup("Main/📂 文件/文件操作")]
        [ResponsiveButtonGroup("Main/📂 文件/文件操作/Buttons")]
        [Button("📥 加载 Dat 文件", ButtonSizes.Large)]
        [GUIColor(0.3f, 1f, 0.3f)]
        private void LoadDatFile()
        {
            if (string.IsNullOrEmpty(datFilePath) || !File.Exists(datFilePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择有效的 dat 文件！", "确定");
                return;
            }

            try
            {
                // 解密并读取文件
                datFileText = Lf2DatDecryptor.DecryptFile(datFilePath, encryptionKey);
                currentLoadedPath = datFilePath;
                isModified = false;

                Debug.Log($"<color=green>✅ 成功加载 Dat 文件: {Path.GetFileName(datFilePath)}</color>");
                Debug.Log($"文本长度: {datFileText.Length} 字符");

                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"加载 Dat 文件失败:\n{e.Message}", "确定");
                Debug.LogError($"加载 Dat 文件失败: {e.Message}\n{e.StackTrace}");
            }
        }

        [ResponsiveButtonGroup("Main/📂 文件/文件操作/Buttons")]
        [Button("💾 保存到 Dat 文件", ButtonSizes.Large)]
        [GUIColor(1f, 0.8f, 0.3f)]
        [EnableIf("@!string.IsNullOrEmpty(currentLoadedPath)")]
        private void SaveDatFile()
        {
            if (string.IsNullOrEmpty(currentLoadedPath))
            {
                EditorUtility.DisplayDialog("错误", "请先加载一个 dat 文件！", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认保存",
                $"确定要保存到文件吗？\n{currentLoadedPath}\n\n这将覆盖原文件！",
                "保存", "取消"))
            {
                return;
            }

            try
            {
                // 加密并保存
                EncryptAndSaveFile(currentLoadedPath, datFileText, encryptionKey);

                isModified = false;
                Debug.Log($"<color=green>✅ 成功保存 Dat 文件: {Path.GetFileName(currentLoadedPath)}</color>");
                EditorUtility.DisplayDialog("成功", "Dat 文件已保存！", "确定");

                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"保存 Dat 文件失败:\n{e.Message}", "确定");
                Debug.LogError($"保存 Dat 文件失败: {e.Message}\n{e.StackTrace}");
            }
        }

        [ResponsiveButtonGroup("Main/📂 文件/文件操作/Buttons")]
        [Button("📄 另存为...", ButtonSizes.Large)]
        [GUIColor(0.3f, 0.8f, 1f)]
        [EnableIf("@!string.IsNullOrEmpty(datFileText)")]
        private void SaveAsFile()
        {
            if (string.IsNullOrEmpty(datFileText))
            {
                EditorUtility.DisplayDialog("错误", "没有可保存的内容！", "确定");
                return;
            }

            string directory = string.IsNullOrEmpty(currentLoadedPath)
                ? Application.dataPath
                : Path.GetDirectoryName(currentLoadedPath);

            string savePath = EditorUtility.SaveFilePanel(
                "另存为 Dat 文件",
                directory,
                "character.dat",
                "dat");

            if (string.IsNullOrEmpty(savePath))
                return;

            try
            {
                EncryptAndSaveFile(savePath, datFileText, encryptionKey);

                currentLoadedPath = savePath;
                isModified = false;
                datFilePath = savePath;

                Debug.Log($"<color=green>✅ 成功另存为: {Path.GetFileName(savePath)}</color>");
                EditorUtility.DisplayDialog("成功", $"文件已保存到:\n{savePath}", "确定");

                Repaint();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"保存文件失败:\n{e.Message}", "确定");
                Debug.LogError($"保存文件失败: {e.Message}");
            }
        }

        #endregion

        #region 文本编辑区域

        [TabGroup("Main", "✏️ 编辑")]
        [BoxGroup("Main/✏️ 编辑/文件状态")]
        [HideLabel]
        [ShowInInspector]
        [DisplayAsString]
        [InfoBox("$StatusInfo", InfoMessageType.None)]
        private string DummyForStatusInfo => "";

        private string StatusInfo
        {
            get
            {
                if (string.IsNullOrEmpty(currentLoadedPath))
                    return "📄 未加载文件";

                string status = isModified ? " ⚠️ (已修改)" : " ✅ (已保存)";
                return $"📁 当前文件: {Path.GetFileName(currentLoadedPath)}{status}  |  📊 行数: {GetLineCount()}  |  📝 字符数: {datFileText.Length}";
            }
        }

        [PropertySpace(5)]
        [TabGroup("Main", "✏️ 编辑")]
        [HideLabel]
        [MultiLineProperty(Lines = 30)]
        [OnValueChanged("OnTextChanged")]
        [ShowInInspector]
        public string datFileText = "";

        private string currentLoadedPath = "";
        private bool isModified = false;

        private void OnTextChanged()
        {
            isModified = true;
        }

        private int GetLineCount()
        {
            if (string.IsNullOrEmpty(datFileText))
                return 0;

            int lines = 1;
            for (int i = 0; i < datFileText.Length; i++)
            {
                if (datFileText[i] == '\n')
                    lines++;
            }
            return lines;
        }

        #endregion

        #region 工具函数

        /// <summary>
        /// 加密并保存文件
        /// </summary>
        private void EncryptAndSaveFile(string filePath, string text, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                // 无密钥，直接保存
                File.WriteAllText(filePath, text);
                return;
            }

            // LF2 加密格式：前 123 字节填充 0，后面加密
            byte[] textBytes = System.Text.Encoding.ASCII.GetBytes(text);
            byte[] encrypted = new byte[123 + textBytes.Length];

            // 前 123 字节填充 0
            for (int i = 0; i < 123; i++)
                encrypted[i] = 0;

            // 加密文本内容
            for (int i = 0; i < textBytes.Length; i++)
            {
                unchecked
                {
                    encrypted[123 + i] = (byte)((byte)textBytes[i] + (byte)key[i % key.Length]);
                }
            }

            File.WriteAllBytes(filePath, encrypted);
        }

        #endregion

        #region 快捷功能

        [TabGroup("Main", "⚡ 快捷")]
        [BoxGroup("Main/⚡ 快捷/快捷操作")]
        [ResponsiveButtonGroup("Main/⚡ 快捷/快捷操作/Buttons")]
        [Button("📤 导出为纯文本", ButtonSizes.Medium)]
        [GUIColor(0.7f, 0.9f, 1f)]
        [EnableIf("@!string.IsNullOrEmpty(datFileText)")]
        private void ExportToTxt()
        {
            if (string.IsNullOrEmpty(datFileText))
                return;

            string directory = string.IsNullOrEmpty(currentLoadedPath)
                ? Application.dataPath
                : Path.GetDirectoryName(currentLoadedPath);

            string savePath = EditorUtility.SaveFilePanel(
                "导出为纯文本",
                directory,
                Path.GetFileNameWithoutExtension(currentLoadedPath ?? "character") + ".txt",
                "txt");

            if (string.IsNullOrEmpty(savePath))
                return;

            try
            {
                File.WriteAllText(savePath, datFileText, System.Text.Encoding.UTF8);
                Debug.Log($"<color=green>✅ 成功导出为纯文本: {savePath}</color>");
                EditorUtility.DisplayDialog("成功", $"文本文件已导出到:\n{savePath}", "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"导出失败:\n{e.Message}", "确定");
            }
        }

        [ResponsiveButtonGroup("Main/⚡ 快捷/快捷操作/Buttons")]
        [Button("🔍 查找/替换", ButtonSizes.Medium)]
        [GUIColor(1f, 0.9f, 0.7f)]
        [EnableIf("@!string.IsNullOrEmpty(datFileText)")]
        private void FindReplace()
        {
            // 简单的查找替换对话框
            string find = EditorUtility.DisplayDialogComplex(
                "查找/替换",
                "此功能需要您使用外部文本编辑器进行复杂的查找替换操作。\n\n建议：先导出为 .txt，用文本编辑器编辑，然后重新加载。",
                "导出为 .txt",
                "取消",
                "确定") == 0 ? "export" : "";

            if (find == "export")
            {
                ExportToTxt();
            }
        }

        #endregion
    }
}
#endif
