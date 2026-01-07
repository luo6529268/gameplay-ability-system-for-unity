#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace NTSD.Animation
{
    public class TextPasteDatWindow : OdinEditorWindow
    {
        #region 窗口打开

        [MenuItem("LF2 Tools/DAT文本粘贴解析器")]
        public static void ShowWindow()
        {
            var window = GetWindow<TextPasteDatWindow>("DAT 转 JSON 工具");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        #endregion

        #region 数据成员

        [Title("输入源", "Input Source", TitleAlignment = TitleAlignments.Centered)]
        [HorizontalGroup("InputSource/Row1")]
        [LabelText("选择方式")]
        [EnumToggleButtons]
        [PropertyOrder(0)]
        public InputSourceType sourceType = InputSourceType.FromJsonFile;

        public enum InputSourceType
        {
            [LabelText("从 JSON 文件")]
            FromJsonFile,
            [LabelText("从 DAT 文本")]
            FromDatText
        }

        [Title("JSON 文件选择", "JSON File Selection", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("sourceType", InputSourceType.FromJsonFile)]
        [HorizontalGroup("JsonFile/Row1")]
        [LabelText("JSON 文件路径")]
        [ReadOnly]
        [PropertyOrder(1)]
        [ShowInInspector]
        private string selectedJsonPath = "";

        [HorizontalGroup("JsonFile/Row1")]
        [Button("选择 JSON 文件", ButtonSizes.Medium)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [PropertyOrder(1)]
        [ShowIf("sourceType", InputSourceType.FromJsonFile)]
        private void SelectJsonFile()
        {
            string path = EditorUtility.OpenFilePanel("选择 JSON 文件", "Assets/NTSD/Config/AnimationConfig", "json");
            if (!string.IsNullOrEmpty(path))
            {
                selectedJsonPath = path;
                LoadJsonFile();
            }
        }

        /// <summary>
        /// 从选择的JSON文件加载数据
        /// </summary>
        private void LoadJsonFile()
        {
            if (string.IsNullOrEmpty(selectedJsonPath))
            {
                Debug.LogWarning("JSON文件路径为空");
                return;
            }

            if (!File.Exists(selectedJsonPath))
            {
                EditorUtility.DisplayDialog("错误", $"文件不存在:\n{selectedJsonPath}", "确定");
                return;
            }

            try
            {
                // 读取JSON文件并移除可能的BOM标记
                string jsonContent = File.ReadAllText(selectedJsonPath, Encoding.UTF8);

                // 移除UTF-8 BOM（如果存在）
                if (jsonContent.Length > 0 && jsonContent[0] == '\uFEFF')
                {
                    jsonContent = jsonContent.Substring(1);
                }

                // 检测文件是否为DAT格式（而不是JSON）
                if (jsonContent.TrimStart().StartsWith("<"))
                {
                    EditorUtility.DisplayDialog("错误",
                        "此文件是 DAT 文本格式，不是 JSON 格式！\n\n" +
                        "请：\n" +
                        "1. 切换到「从 DAT 文本」模式，然后点击「从文件粘贴」\n" +
                        "或\n" +
                        "2. 选择 AnimationConfig 目录下的真正 JSON 文件\n" +
                        "   （例如：character_2_data.json）",
                        "确定");
                    Debug.LogWarning($"<color=yellow>文件 {selectedJsonPath} 是 DAT 格式，不是 JSON 格式</color>");
                    return;
                }

                var wrapper = JsonUtility.FromJson<LF2CharacterDataWrapper>(jsonContent);

                if (wrapper != null && wrapper.characterData != null)
                {
                    characterData = wrapper.characterData;
                    parsedFrames = characterData.frames;
                    exportCharacterId = wrapper.characterId;

                    analysisResult = GenerateFullAnalysisReport(characterData);
                    UpdateJsonPreview();

                    Debug.Log($"<color=green>✅ JSON 文件加载成功: 角色ID={exportCharacterId}, 帧数={parsedFrames.Count}</color>");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "JSON 文件格式不正确或解析后为空", "确定");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("加载失败", $"错误: {e.Message}", "确定");
                Debug.LogError($"加载JSON失败: {e}");
            }
        }

        /// <summary>
        /// 更新JSON预览
        /// </summary>
        private void UpdateJsonPreview()
        {
            if (characterData == null)
            {
                jsonPreview = "";
                return;
            }

            try
            {
                var wrapper = new LF2CharacterDataWrapper(exportCharacterId, characterData);
                jsonPreview = JsonUtility.ToJson(wrapper, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"生成JSON预览失败: {e}");
                jsonPreview = "";
            }
        }

        [Title("DAT 文本粘贴", "DAT Text Paste", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("sourceType", InputSourceType.FromDatText)]
        [HideLabel]
        [MultiLineProperty(10)]
        [PropertyOrder(2)]
        public string pastedContent = "";

        [HorizontalGroup("DatButtons")]
        [Button("从文件粘贴", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [ShowIf("sourceType", InputSourceType.FromDatText)]
        [PropertyOrder(3)]
        private void PasteFromFile()
        {
            string filePath = EditorUtility.OpenFilePanel("选择 DAT 文件", "Assets/NTSD/Config/FrameConfig", "");
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    pastedContent = File.ReadAllText(filePath, Encoding.UTF8);
                    Debug.Log($"<color=green>✅ 已从文件加载 DAT 内容，长度: {pastedContent.Length} 字符</color>");
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("错误", $"读取文件失败: {e.Message}", "确定");
                }
            }
        }

        [HorizontalGroup("DatButtons")]
        [Button("清空内容", ButtonSizes.Large)]
        [GUIColor(1f, 0.8f, 0.3f)]
        [ShowIf("sourceType", InputSourceType.FromDatText)]
        [PropertyOrder(3)]
        private void ClearContent()
        {
            pastedContent = "";
        }

        [HorizontalGroup("DatButtons")]
        [Button("解析内容", ButtonSizes.Large)]
        [GUIColor(0.3f, 1f, 0.3f)]
        [EnableIf("@!string.IsNullOrEmpty(pastedContent) && sourceType == InputSourceType.FromDatText")]
        [ShowIf("sourceType", InputSourceType.FromDatText)]
        [PropertyOrder(3)]
        private void ParseContent()
        {
            try
            {
                characterData = LF2CharacterParser.ParseFullCharacterData(pastedContent);
                parsedFrames = characterData.frames;
                analysisResult = GenerateFullAnalysisReport(characterData);
                UpdateJsonPreview();
                Debug.Log($"解析完成: {parsedFrames.Count} 个帧");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("解析错误", $"解析失败: {e.Message}", "确定");
                Debug.LogError($"解析失败: {e}");
            }
        }

        public int characterId = 2;
        private List<LF2FrameData> parsedFrames = new List<LF2FrameData>();
        private LF2CharacterData characterData = null;
        private string analysisResult = "";
        private string jsonPreview = "";
        private string exportFolder = "Assets/NTSD/Config/AnimationConfig";

        [Title("解析结果", "Parse Result", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("@parsedFrames.Count > 0")]
        [HorizontalGroup("Result/Row1")]
        [LabelText("总帧数")]
        [ReadOnly]
        [ShowInInspector]
        [PropertyOrder(10)]
        private int FrameCount => parsedFrames.Count;

        [HorizontalGroup("Result/Row1")]
        [LabelText("角色名称")]
        [ReadOnly]
        [ShowInInspector]
        [PropertyOrder(10)]
        private string CharacterName => characterData?.name ?? "N/A";

        [FoldoutGroup("Result/分析报告", Expanded = false)]
        [HideLabel]
        [MultiLineProperty(8)]
        [ReadOnly]
        [ShowInInspector]
        [PropertyOrder(11)]
        private string AnalysisReport => analysisResult;

        [Title("JSON 预览", "JSON Preview", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("@!string.IsNullOrEmpty(jsonPreview)")]
        [HideLabel]
        [MultiLineProperty(15)]
        [ReadOnly]
        [ShowInInspector]
        [PropertyOrder(20)]
        private string JsonPreview => jsonPreview;

        [Title("导出设置", "Export Settings", TitleAlignment = TitleAlignments.Centered)]
        [ShowIf("@parsedFrames.Count > 0")]
        [HorizontalGroup("Export/Row1")]
        [LabelText("角色 ID")]
        [PropertyOrder(30)]
        public int exportCharacterId = 2;

        [HorizontalGroup("Export/Row1")]
        [LabelText("导出路径")]
        [FolderPath]
        [PropertyOrder(30)]
        public string outputPath = "Assets/NTSD/Config/AnimationConfig";

        [HorizontalGroup("Export/Row2")]
        [LabelText("文件名")]
        [PropertyOrder(31)]
        public string outputFileName = "character_data.json";

        [ShowIf("@parsedFrames.Count > 0")]
        [HorizontalGroup("Export/Row2")]
        [Button("📁 使用源文件", ButtonSizes.Medium)]
        [GUIColor(0.8f, 0.8f, 1f)]
        [PropertyOrder(31)]
        [ShowIf("@!string.IsNullOrEmpty(selectedJsonPath)")]
        private void UseSourcePath()
        {
            outputPath = Path.GetDirectoryName(selectedJsonPath);
            outputFileName = Path.GetFileName(selectedJsonPath);
        }

        [Title("操作", "Actions", TitleAlignment = TitleAlignments.Centered)]
        [HorizontalGroup("Actions/Row1")]
        [Button("导出为 JSON", ButtonSizes.Large)]
        [GUIColor(0.3f, 1f, 0.3f)]
        [EnableIf("@parsedFrames.Count > 0 && !string.IsNullOrEmpty(jsonPreview)")]
        [PropertyOrder(40)]
        private void ExportAsJson()
        {
            if (characterData == null)
            {
                EditorUtility.DisplayDialog("错误", "没有可导出的数据", "确定");
                return;
            }

            string fullPath = Path.Combine(outputPath, outputFileName);

            // 检查文件是否存在
            bool fileExists = File.Exists(fullPath);
            if (fileExists)
            {
                if (!EditorUtility.DisplayDialog("确认覆盖",
                    $"文件已存在：\n{fullPath}\n\n是否覆盖？",
                    "覆盖", "取消"))
                {
                    return;
                }
            }

            try
            {
                // 确保目录存在
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                // 写入文件
                File.WriteAllText(fullPath, jsonPreview, Encoding.UTF8);
                AssetDatabase.Refresh();

                Debug.Log($"<color=green>✅ JSON 导出成功: {fullPath}</color>");
                EditorUtility.DisplayDialog("导出成功",
                    $"JSON 文件已导出到:\n{fullPath}",
                    "确定");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("导出失败", $"错误: {e.Message}", "确定");
                Debug.LogError($"导出失败: {e}");
            }
        }

        [HorizontalGroup("Actions/Row1")]
        [Button("创建 Unity 资源", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [EnableIf("@parsedFrames.Count > 0")]
        [PropertyOrder(40)]
        private void CreateUnityAsset()
        {
            if (characterData == null)
            {
                EditorUtility.DisplayDialog("错误", "没有可导出的数据", "确定");
                return;
            }

            var asset = CreateInstance<LF2CharacterDataAsset>();
            asset.characterId = exportCharacterId;
            asset.characterData = characterData;

            string assetPath = Path.Combine(outputPath, $"Character_{exportCharacterId}_Data.asset");

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            EditorUtility.DisplayDialog("创建成功", $"Unity 资源已创建: {assetPath}", "确定");
        }

        [HorizontalGroup("Actions/Row1")]
        [Button("验证数据", ButtonSizes.Large)]
        [GUIColor(1f, 0.8f, 0.3f)]
        [EnableIf("@parsedFrames.Count > 0")]
        [PropertyOrder(40)]
        private void ValidateData()
        {
            var parser = new TextPasteDatParser();
            parser.parsedFrames = parsedFrames;
            parser.ValidateParsing();
            DestroyImmediate(parser);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 生成完整的分析报告
        /// </summary>
        private string GenerateFullAnalysisReport(LF2CharacterData data)
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("===== 角色配置解析报告 =====");
            report.AppendLine($"解析时间: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            report.AppendLine("【基本信息】");
            report.AppendLine($"角色名称: {data.name}");
            report.AppendLine($"头像文件: {data.head}");
            report.AppendLine($"小图文件: {data.small}");
            report.AppendLine($"精灵图数量: {data.files.Count}");
            report.AppendLine();

            report.AppendLine("【移动参数】");
            report.AppendLine($"行走帧率: {data.walking_frame_rate}");
            report.AppendLine($"行走速度: {data.walking_speed} (Z轴: {data.walking_speedz})");
            report.AppendLine($"奔跑速度: {data.running_speed} (Z轴: {data.running_speedz})");
            report.AppendLine($"跳跃: 高度={data.jump_height}, 距离={data.jump_distance}");
            report.AppendLine($"冲刺: 高度={data.dash_height}, 距离={data.dash_distance}");
            report.AppendLine();

            report.AppendLine("【帧数据统计】");
            report.AppendLine($"总帧数: {data.frames.Count}");

            int totalBodies = 0;
            int totalItrs = 0;
            int totalWpoints = 0;
            foreach (var frame in data.frames)
            {
                totalBodies += frame.bodies.Count;
                totalItrs += frame.itrs.Count;
                totalWpoints += frame.wpoints.Count;
            }

            report.AppendLine($"总碰撞盒数: {totalBodies}");
            report.AppendLine($"总交互区域数: {totalItrs}");
            report.AppendLine($"总武器点数: {totalWpoints}");

            return report.ToString();
        }

        #endregion
    }
}

#endif