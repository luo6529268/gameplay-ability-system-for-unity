#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 JSON读取测试窗口
    /// </summary>
    public class LF2JsonReaderWindow : EditorWindow
    {
        private string jsonFilePath = "";
        private LF2CharacterDataWrapper loadedData = null;
        private Vector2 scrollPosition;
        private int selectedFrameIndex = 0;

        [MenuItem("LF2 Tools/JSON读取器")]
        public static void ShowWindow()
        {
            GetWindow<LF2JsonReaderWindow>("LF2 JSON读取器");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawLoadSection();

            GUILayout.Space(10);

            if (loadedData != null)
            {
                DrawDataDisplay();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制加载区域
        /// </summary>
        private void DrawLoadSection()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("加载JSON文件", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("文件路径:", GUILayout.Width(70));
            jsonFilePath = EditorGUILayout.TextField(jsonFilePath);

            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFilePanel("选择JSON文件", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    jsonFilePath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("加载JSON文件"))
            {
                LoadJsonFile();
            }

            if (GUILayout.Button("从Assets/ExportedDAT加载"))
            {
                LoadFromExportedFolder();
            }

            GUI.enabled = loadedData != null;
            if (GUILayout.Button("清空数据"))
            {
                loadedData = null;
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制数据显示区域
        /// </summary>
        private void DrawDataDisplay()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 基本信息
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"角色ID: {loadedData.characterId}");
            EditorGUILayout.LabelField($"角色名称: {loadedData.characterData.name}");
            EditorGUILayout.LabelField($"头像文件: {loadedData.characterData.head}");
            EditorGUILayout.LabelField($"小图文件: {loadedData.characterData.small}");
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            // 精灵图文件
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("精灵图文件列表", EditorStyles.boldLabel);
            foreach (var file in loadedData.characterData.files)
            {
                EditorGUILayout.LabelField($"  [{file.startFrame}-{file.endFrame}] {file.filePath}");
                EditorGUILayout.LabelField($"    尺寸: {file.width}x{file.height}, 行列: {file.row}x{file.col}");
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            // 移动参数
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("移动参数", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"行走帧率: {loadedData.characterData.walking_frame_rate}");
            EditorGUILayout.LabelField($"行走速度: {loadedData.characterData.walking_speed} (Z: {loadedData.characterData.walking_speedz})");
            EditorGUILayout.LabelField($"奔跑速度: {loadedData.characterData.running_speed} (Z: {loadedData.characterData.running_speedz})");
            EditorGUILayout.LabelField($"跳跃: 高度={loadedData.characterData.jump_height}, 距离={loadedData.characterData.jump_distance}");
            EditorGUILayout.LabelField($"冲刺: 高度={loadedData.characterData.dash_height}, 距离={loadedData.characterData.dash_distance}");
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            // 帧数据
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("帧数据", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"总帧数: {loadedData.characterData.frames.Count}");

            if (loadedData.characterData.frames.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("选择帧查看详情:");

                string[] frameNames = new string[loadedData.characterData.frames.Count];
                for (int i = 0; i < loadedData.characterData.frames.Count; i++)
                {
                    var frame = loadedData.characterData.frames[i];
                    frameNames[i] = $"帧 {frame.frameId}: {frame.frameName}";
                }

                selectedFrameIndex = EditorGUILayout.Popup("帧列表:", selectedFrameIndex, frameNames);

                if (selectedFrameIndex >= 0 && selectedFrameIndex < loadedData.characterData.frames.Count)
                {
                    DrawFrameDetails(loadedData.characterData.frames[selectedFrameIndex]);
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制帧详情
        /// </summary>
        private void DrawFrameDetails(LF2FrameData frame)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField($"帧 {frame.frameId}: {frame.frameName}", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"图片索引: {frame.pic}");
            EditorGUILayout.LabelField($"状态: {frame.state}");
            EditorGUILayout.LabelField($"等待时间: {frame.wait}");
            EditorGUILayout.LabelField($"下一帧: {frame.next}");
            EditorGUILayout.LabelField($"速度: dvx={frame.dvx}, dvy={frame.dvy}");
            EditorGUILayout.LabelField($"中心点: ({frame.centerx}, {frame.centery})");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"碰撞盒数量: {frame.bodies.Count}");
            EditorGUILayout.LabelField($"交互区域数量: {frame.itrs.Count}");
            EditorGUILayout.LabelField($"武器点数量: {frame.wpoints.Count}");

            if (!string.IsNullOrEmpty(frame.sound))
            {
                EditorGUILayout.LabelField($"音效: {frame.sound}");
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 加载JSON文件
        /// </summary>
        private void LoadJsonFile()
        {
            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择JSON文件", "确定");
                return;
            }

            loadedData = LF2CharacterJsonLoader.LoadFromFile(jsonFilePath);

            if (loadedData != null)
            {
                Debug.Log($"成功加载角色数据: {loadedData.characterData.name}");
                LF2CharacterJsonLoader.PrintCharacterInfo(loadedData);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "加载JSON文件失败", "确定");
            }
        }

        /// <summary>
        /// 从导出文件夹加载
        /// </summary>
        private void LoadFromExportedFolder()
        {
            string folderPath = "Assets/ExportedDAT";

            if (!Directory.Exists(folderPath))
            {
                EditorUtility.DisplayDialog("错误", $"文件夹不存在: {folderPath}", "确定");
                return;
            }

            string[] jsonFiles = Directory.GetFiles(folderPath, "character_*_data.json");

            if (jsonFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "未找到任何角色JSON文件", "确定");
                return;
            }

            // 显示选择菜单
            GenericMenu menu = new GenericMenu();
            foreach (string file in jsonFiles)
            {
                string fileName = Path.GetFileName(file);
                menu.AddItem(new GUIContent(fileName), false, () =>
                {
                    jsonFilePath = file;
                    LoadJsonFile();
                });
            }
            menu.ShowAsContext();
        }
    }
}
#endif
