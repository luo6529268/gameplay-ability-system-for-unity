using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.Define;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Sirenix.OdinInspector;
using NTSD.DatParser;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NTSD.Animation
{
    /// <summary>
    /// 角色动画管理器 - Odin Inspector 增强版
    /// 支持在Inspector中查看和调试数据
    /// </summary>
    public class CharacterAnimtorManager : MMSingleton<CharacterAnimtorManager>
    {
        const string TotalCharacterFrameConfigPath = "Assets/NTSD/Config/AnimationConfig";

        #region 数据存储

        [Title("角色配置数据", "Runtime Data", TitleAlignment = TitleAlignments.Centered)]
        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(
            KeyLabel = "角色ID",
            ValueLabel = "配置数据",
            DisplayMode = DictionaryDisplayOptions.Foldout
        )]
        [PropertyOrder(-1)]
        private Dictionary<int, LF2CharacterDataWrapper> TotalCharacterFrameConfig = new Dictionary<int, LF2CharacterDataWrapper>(10);

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(
            KeyLabel = "角色ID",
            ValueLabel = "精灵列表",
            DisplayMode = DictionaryDisplayOptions.Foldout
        )]
        [PropertyOrder(-1)]
        private Dictionary<int, List<Sprite>> MergedSprites = new Dictionary<int, List<Sprite>>(10);

        #endregion

        #region Inspector 调试工具

        [Title("调试工具", "Debug Tools", TitleAlignment = TitleAlignments.Centered)]
        [BoxGroup("查询工具")]
        [LabelText("角色ID")]
        [ValueDropdown("GetLoadedCharacterIds", DropdownTitle = "选择角色ID", DropdownWidth = 200)]
        [PropertyOrder(0)]
        [SerializeField]
        private int selectedCharacterId = 1;

        [BoxGroup("查询工具")]
        [LabelText("帧ID")]
        [PropertyOrder(1)]
        [SerializeField]
        private int selectedFrameId = 0;

        [BoxGroup("查询工具/角色信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("角色名称")]
        [PropertyOrder(2)]
        private string CharacterName => GetCharacterName(selectedCharacterId);

        [BoxGroup("查询工具/角色信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("帧数量")]
        [PropertyOrder(3)]
        private int FrameCount => GetCharacterFrames(selectedCharacterId)?.Count ?? 0;

        [BoxGroup("查询工具/角色信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("精灵数量")]
        [PropertyOrder(4)]
        private int SpriteCount => GetCharacterSpriteByID(selectedCharacterId)?.Count ?? 0;

        [BoxGroup("查询工具/角色信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("行走速度")]
        [PropertyOrder(5)]
        private float WalkSpeed => GetCharacterData(selectedCharacterId)?.walking_speed ?? 0f;

        [BoxGroup("查询工具/角色信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("奔跑速度")]
        [PropertyOrder(6)]
        private float RunSpeed => GetCharacterData(selectedCharacterId)?.running_speed ?? 0f;

        #endregion

        #region Inspector 按钮

        [Title("操作按钮", "Actions", TitleAlignment = TitleAlignments.Centered)]
        [HorizontalGroup("Actions/Row1")]
        [Button("刷新所有数据", ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 1f)]
        [PropertyOrder(10)]
        private void RefreshAllData()
        {
            TotalCharacterFrameConfig.Clear();
            MergedSprites.Clear();
            OnLoadCharacterFrameConfig();
            OnLoadCharacterSprite().Forget();
        }

        [HorizontalGroup("Actions/Row1")]
        [Button("打印角色信息", ButtonSizes.Large)]
        [GUIColor(0.3f, 1f, 0.3f)]
        [PropertyOrder(10)]
        private void PrintSelectedCharacterInfo()
        {
            PrintCharacterInfo(selectedCharacterId);
        }

        [HorizontalGroup("Actions/Row2")]
        [Button("查看角色配置", ButtonSizes.Large)]
        [GUIColor(1f, 0.8f, 0.3f)]
        [PropertyOrder(11)]
        private void ViewCharacterConfig()
        {
            var config = GetCharacterConfig(selectedCharacterId);
            if (config != null)
            {
                Debug.Log($"===== 角色 {config.characterId} 配置 =====\n" +
                         $"名称: {config.characterData.name}\n" +
                         $"帧数: {config.characterData.frames.Count}\n" +
                         $"精灵图: {config.characterData.files.Count}");
            }
            else
            {
                Debug.LogWarning($"未找到角色ID: {selectedCharacterId}");
            }
        }

        [HorizontalGroup("Actions/Row2")]
        [Button("查看指定帧", ButtonSizes.Large)]
        [GUIColor(1f, 0.6f, 1f)]
        [PropertyOrder(11)]
        private void ViewSelectedFrame()
        {
            var frame = GetFrameData(selectedCharacterId, selectedFrameId);
            if (frame != null)
            {
                Debug.Log($"===== 帧 {frame.frameId}: {frame.frameName} =====\n" +
                         $"图片: {frame.pic}, 状态: {frame.state}\n" +
                         $"等待: {frame.wait}, 下一帧: {frame.next}\n" +
                         $"碰撞盒: {frame.bodies.Count}, 交互区域: {frame.itrs.Count}\n" +
                         $"MP: {frame.mp}");
            }
            else
            {
                Debug.LogWarning($"未找到帧: 角色ID={selectedCharacterId}, 帧ID={selectedFrameId}");
            }
        }

        [HorizontalGroup("Actions/Row3")]
        [Button("列出所有角色", ButtonSizes.Medium)]
        [PropertyOrder(12)]
        private void ListAllCharacters()
        {
            var ids = GetAllLoadedCharacterIds();
            Debug.Log($"===== 已加载角色列表 ({ids.Count}个) =====");
            foreach (var id in ids)
            {
                string name = GetCharacterName(id);
                int frameCount = GetCharacterFrames(id)?.Count ?? 0;
                Debug.Log($"ID: {id}, 名称: {name}, 帧数: {frameCount}");
            }
        }

        [HorizontalGroup("Actions/Row3")]
        [Button("清空所有数据", ButtonSizes.Medium)]
        [GUIColor(1f, 0.3f, 0.3f)]
        [PropertyOrder(12)]
        private void ClearAllData()
        {
            if (EditorUtility.DisplayDialog("确认清空", "确定要清空所有已加载的数据吗？", "确定", "取消"))
            {
                TotalCharacterFrameConfig.Clear();
                MergedSprites.Clear();
                Debug.Log("所有数据已清空");
            }
        }

        #endregion

        #region 精灵预览区域

        [Title("精灵预览", "Sprite Preview", TitleAlignment = TitleAlignments.Centered)]

        [FoldoutGroup("精灵列表", Expanded = false)]
        [ShowInInspector, ReadOnly]
        [LabelText("当前角色精灵列表")]
        [ListDrawerSettings(
            ShowIndexLabels = true,
            ShowPaging = true,
            NumberOfItemsPerPage = 20,
            DraggableItems = false,
            ShowItemCount = true
        )]
        [PropertyOrder(20)]
        private List<SpritePreviewItem> CurrentCharacterSpritesPreview
        {
            get
            {
                List<SpritePreviewItem> previewList = new List<SpritePreviewItem>();

                if (selectedCharacterId > 0 && MergedSprites.ContainsKey(selectedCharacterId))
                {
                    var sprites = MergedSprites[selectedCharacterId];
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        previewList.Add(new SpritePreviewItem
                        {
                            Index = i,
                            Sprite = sprites[i]
                        });
                    }
                }

                return previewList;
            }
        }

        [FoldoutGroup("单个精灵", Expanded = true)]
        [ShowInInspector, ReadOnly]
        [PreviewField(200, ObjectFieldAlignment.Center)]
        [LabelText("指定帧精灵预览")]
        [PropertyOrder(21)]
        private Sprite SpecificSpritePreview
        {
            get
            {
                if (selectedCharacterId > 0 && MergedSprites.ContainsKey(selectedCharacterId))
                {
                    var sprites = MergedSprites[selectedCharacterId];
                    if (selectedFrameId >= 0 && selectedFrameId < sprites.Count)
                    {
                        return sprites[selectedFrameId];
                    }
                }
                return null;
            }
        }

        [FoldoutGroup("单个精灵")]
        [ShowInInspector, ReadOnly]
        [LabelText("精灵信息")]
        [PropertyOrder(22)]
        private string SpecificSpriteInfo
        {
            get
            {
                var sprite = SpecificSpritePreview;
                if (sprite != null)
                {
                    return $"索引: {selectedFrameId}\n" +
                           $"名称: {sprite.name}\n" +
                           $"尺寸: {sprite.rect.width} x {sprite.rect.height}\n" +
                           $"纹理: {sprite.texture.width} x {sprite.texture.height}";
                }
                return "未选择精灵";
            }
        }

        #endregion

        #region 精灵预览辅助类

        [System.Serializable]
        private class SpritePreviewItem
        {
            [HorizontalGroup("Row")]
            [LabelText("帧")]
            [ReadOnly]
            public int Index;

            [HorizontalGroup("Row")]
            [PreviewField(80, ObjectFieldAlignment.Center)]
            [HideLabel]
            [ReadOnly]
            public Sprite Sprite;

            [HorizontalGroup("Row")]
            [LabelText("尺寸")]
            [ReadOnly]
            [ShowInInspector]
            public string Size => Sprite != null ? $"{Sprite.rect.width}x{Sprite.rect.height}" : "N/A";
        }

        #endregion

        #region 统计信息

        [Title("统计信息", "Statistics", TitleAlignment = TitleAlignments.Centered)]
        [FoldoutGroup("统计信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("已加载角色数量")]
        [PropertyOrder(30)]
        private int LoadedCharacterCount => TotalCharacterFrameConfig.Count;

        [FoldoutGroup("统计信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("总帧数")]
        [PropertyOrder(30)]
        private int TotalFrameCount
        {
            get
            {
                int count = 0;
                foreach (var config in TotalCharacterFrameConfig.Values)
                {
                    count += config.characterData.frames.Count;
                }
                return count;
            }
        }

        [FoldoutGroup("统计信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("总精灵数量")]
        [PropertyOrder(30)]
        private int TotalSpriteCount
        {
            get
            {
                int count = 0;
                foreach (var sprites in MergedSprites.Values)
                {
                    count += sprites.Count;
                }
                return count;
            }
        }

        [FoldoutGroup("统计信息")]
        [ShowInInspector, ReadOnly]
        [LabelText("配置文件路径")]
        [PropertyOrder(31)]
        private string ConfigPath => TotalCharacterFrameConfigPath;

        #endregion

        #region 原有功能

        protected override async void InitializeSingleton()
        {
            base.InitializeSingleton();

            OnLoadCharacterFrameConfig();
            await OnLoadCharacterSprite();
        }

        /// <summary>
        /// 加载所有角色帧配置
        /// </summary>
        private void OnLoadCharacterFrameConfig()
        {
            // 1. 先解析 data.txt，获取 ID 到文件名的映射
            string dataFilePath = Path.Combine(TotalCharacterFrameConfigPath, "../data.txt");
            string fullDataPath = Path.GetFullPath(dataFilePath);

            if (!File.Exists(fullDataPath))
            {
                Debug.LogError($"<color=red>❌ data.txt 文件不存在: {fullDataPath}</color>");
                Debug.LogError($"<color=red>   请确保 data.txt 在 {Path.GetDirectoryName(fullDataPath)} 目录下</color>");
                return;
            }

            // 解析 data.txt
            Dictionary<int, DataFileParser.ObjectData> dataObjectMap = DataFileParser.ParseDataFile(fullDataPath);

            if (dataObjectMap == null || dataObjectMap.Count == 0)
            {
                Debug.LogError("<color=red>❌ data.txt 解析失败或没有对象定义</color>");
                return;
            }

            Debug.Log($"<color=cyan>开始加载角色配置，data.txt 中共 {dataObjectMap.Count} 个对象定义</color>");

            // 2. 遍历 data.txt 中的对象定义，加载对应的 DAT 文件
            int loadedCount = 0;
            foreach (var kvp in dataObjectMap)
            {
                int characterId = kvp.Key;
                DataFileParser.ObjectData objectData = kvp.Value;

                try
                {
                    // 将 data.txt 中的相对路径转换为 DAT 文件路径
                    string configDir = Path.GetDirectoryName(fullDataPath);
                    string datFilePath = DataFileParser.ResolveObjectFilePath(configDir, objectData.file);

                    // 将扩展名改为 .dat（如果 data.txt 中写的是 .json）
                    datFilePath = Path.ChangeExtension(datFilePath, ".dat");

                    if (!File.Exists(datFilePath))
                    {
                        Debug.LogWarning($"<color=yellow>⚠️ DAT文件不存在: ID={characterId}, 文件={datFilePath}</color>");
                        continue;
                    }

                    // 解密 dat 文件
                    string datText = Lf2DatDecryptor.DecryptFile(datFilePath, "odBearBecauseHeIsVeryGoodSiuHungIsAGo");

                    if (string.IsNullOrEmpty(datText))
                    {
                        Debug.LogWarning($"<color=yellow>⚠️ DAT文件解密返回空: ID={characterId}, 文件={Path.GetFileName(datFilePath)}</color>");
                        continue;
                    }

                    // 解析 dat 文件
                    Lf2DatParserV2 parser = new Lf2DatParserV2();
                    Lf2DatFile datFile = parser.Parse(datText, datFilePath);

                    if (datFile == null || datFile.Frames.Count == 0)
                    {
                        Debug.LogWarning($"<color=yellow>⚠️ DAT文件解析失败或无帧数据: ID={characterId}, 文件={Path.GetFileName(datFilePath)}</color>");
                        continue;
                    }

                    // 构建角色数据（使用 data.txt 中的 ID，并传入 DAT 文件所在目录用于路径解析）
                    string datFileDirectory = Path.GetDirectoryName(datFilePath);
                    LF2CharacterData characterData = BuildCharacterDataFromDat(datFile, datFileDirectory);

                    // 创建 wrapper 并存储
                    LF2CharacterDataWrapper wrapper = new LF2CharacterDataWrapper(characterId, characterData);

                    if (wrapper != null && wrapper.characterData != null)
                    {
                        TotalCharacterFrameConfig[characterId] = wrapper;
                        loadedCount++;
                        Debug.Log($"<color=green>✅ 加载角色: ID={characterId}, 名称={characterData.name}, 帧数={characterData.frames.Count}, 文件={objectData.file}</color>");
                    }
                    else
                    {
                        Debug.LogWarning($"<color=yellow>⚠️ DAT数据转换返回null: ID={characterId}</color>");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<color=red>❌ 加载失败: ID={characterId}, 文件={objectData.file}\n错误: {e.Message}\n{e.StackTrace}</color>");
                }
            }

            Debug.Log($"<color=cyan>配置加载完成，成功加载 {loadedCount}/{dataObjectMap.Count} 个角色</color>");
        }

        /// <summary>
        /// 从解析后的 Dat 文件构建 LF2CharacterData
        /// </summary>
        /// <param name="datFile">解析后的 DAT 文件</param>
        /// <param name="datFileDirectory">DAT 文件所在目录（用于解析相对路径）</param>
        private LF2CharacterData BuildCharacterDataFromDat(Lf2DatFile datFile, string datFileDirectory)
        {
            LF2CharacterData characterData = new LF2CharacterData();

            // 1. 转换所有帧数据
            characterData.frames = new List<LF2FrameData>();
            foreach (var frameBlock in datFile.Frames)
            {
                LF2FrameData frameData = Lf2DatConverter.ConvertToFrameData(frameBlock);
                if (frameData != null)
                {
                    characterData.frames.Add(frameData);
                }
            }

            // 2. 提取 BMP 信息
            if (datFile.Bmp != null)
            {
                // 设置角色名称
                characterData.name = datFile.Bmp.Name ?? "Unknown";

                // 转换精灵文件信息
                characterData.files = new List<SpriteFileInfo>();

                Debug.Log($"<color=cyan>[BMP 解析] 角色={characterData.name}, 找到 {datFile.Bmp.Files.Count} 个精灵文件定义</color>");

                foreach (var fileDef in datFile.Bmp.Files)
                {
                    string pathInDat = fileDef.Path.Replace("\\", "/");
                    string absolutePath;

                    // 检查是否为 Unity 项目相对路径（以 Assets/ 开头）
                    if (pathInDat.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
                    {
                        // 从项目根目录开始解析
                        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                        absolutePath = Path.GetFullPath(Path.Combine(projectRoot, pathInDat));
                    }
                    else
                    {
                        // 相对于 DAT 文件所在目录
                        absolutePath = Path.GetFullPath(Path.Combine(datFileDirectory, pathInDat));
                    }

                    SpriteFileInfo spriteInfo = new SpriteFileInfo
                    {
                        filePath = absolutePath,
                        width = fileDef.Width,
                        height = fileDef.Height,
                        row = fileDef.Row,
                        col = fileDef.Col,
                        startFrame = fileDef.StartIndex,
                        endFrame = fileDef.EndIndex
                    };
                    characterData.files.Add(spriteInfo);

                    Debug.Log($"<color=cyan>[BMP 文件] {pathInDat} -> {absolutePath}, 范围=[{fileDef.StartIndex}-{fileDef.EndIndex}], 尺寸={fileDef.Width}x{fileDef.Height}, 行列={fileDef.Row}x{fileDef.Col}</color>");
                }
            }
            else
            {
                Debug.LogWarning("<color=yellow>⚠️ DAT 文件中没有 BMP 信息（datFile.Bmp 为 null）</color>");
                characterData.files = new List<SpriteFileInfo>(); // 初始化为空列表
            }

            // 3. 提取移动参数（从根级别的 Properties 或 Blocks 中）
            ExtractMovementParameters(datFile, characterData);

            return characterData;
        }

        /// <summary>
        /// 从 Dat 文件中提取移动参数（walking_speed, running_speed 等）
        /// </summary>
        private void ExtractMovementParameters(Lf2DatFile datFile, LF2CharacterData characterData)
        {
            // 先尝试从根级别属性提取
            foreach (var prop in datFile.Properties)
            {
                ApplyMovementProperty(prop.Key, prop.Value, characterData);
            }

            // 再从 blocks 中提取（某些 dat 文件可能将参数放在 <object> 块中）
            foreach (var block in datFile.Blocks)
            {
                foreach (var prop in block.Properties)
                {
                    ApplyMovementProperty(prop.Key, prop.Value, characterData);
                }
            }
        }

        /// <summary>
        /// 应用单个移动参数属性
        /// </summary>
        private void ApplyMovementProperty(string key, string value, LF2CharacterData characterData)
        {
            float floatValue;
            if (!float.TryParse(value, out floatValue))
            {
                return;
            }

            switch (key.ToLower())
            {
                case "walking_speed":
                    characterData.walking_speed = floatValue;
                    break;
                case "running_speed":
                    characterData.running_speed = floatValue;
                    break;
                case "walking_speedz":
                    // 可以添加其他参数的处理
                    break;
                case "running_speedz":
                    // 可以添加其他参数的处理
                    break;
                case "jump_height":
                    // 可以添加其他参数的处理
                    break;
                case "jump_distance":
                    // 可以添加其他参数的处理
                    break;
            }
        }

        /// <summary>
        /// 预加载所有角色精灵
        /// 根据配置文件中的 files 字段的 filePath 加载 BMP 精灵
        /// 自动处理黑色透明并按配置切割精灵
        /// </summary>
        private async UniTask OnLoadCharacterSprite()
        {
            Debug.Log($"<color=cyan>开始加载精灵，角色配置数量: {TotalCharacterFrameConfig.Count}</color>");

            // 创建透明处理配置（黑色变透明）
           TransparentColorData transparentData = new TransparentColorData
            {
                targetColor = new Color(0f, 0f, 0f),
                colorTolerance = 0.031f,
                preserveEdgeColors = true,
                edgeSmoothing = 0.5f,
                borderColor = new Color(0f, 1f, 0f),
                borderTolerance = 0.12f,
                searchRadius = 6,
                useEdgeDetection = true,
                edgeDetectionRadius = 2,
                edgeThreshold = 0.5f
            };

            int frameIndex = 0;
            foreach (var config in TotalCharacterFrameConfig.Values)
            {
                try
                {
                    int characterId = config.characterId;

                    // 计算精灵列表总大小（基于最后一个文件的 endFrame）
                    int totalSpriteCount = 0;
                    if (config.characterData.files.Count > 0)
                    {
                        var lastFile = config.characterData.files[config.characterData.files.Count - 1];
                        totalSpriteCount = lastFile.endFrame + 1;
                    }

                    // 预分配精灵列表（填充 null）
                    List<Sprite> allSprites = new List<Sprite>(new Sprite[totalSpriteCount]);
                    Debug.Log($"<color=cyan>[精灵预分配] 角色ID={characterId}, 预分配大小={totalSpriteCount}</color>");

                    // 遍历角色的所有精灵文件
                    foreach (var fileInfo in config.characterData.files)
                    {
                        frameIndex++;

                        string filePath = fileInfo.filePath;

                        if (frameIndex % 5 == 0)
                            await UniTask.NextFrame();

                        // 加载 BMP 文件
                        Texture2D originalTexture = LoadBMPTexture(filePath);
                        if (originalTexture == null)
                        {
                            Debug.LogWarning($"<color=yellow>⚠️ 无法加载图片: {filePath}</color>");
                            continue;
                        }

                        Debug.Log($"<color=cyan>[精灵裁剪] 文件={Path.GetFileName(filePath)}, 纹理尺寸={originalTexture.width}x{originalTexture.height}, " +
                                 $"配置={fileInfo.row}行x{fileInfo.col}列, 格子={fileInfo.width}x{fileInfo.height}, " +
                                 $"帧范围=[{fileInfo.startFrame}-{fileInfo.endFrame}]</color>");

                        // 检查纹理尺寸是否匹配配置（允许±1像素容差，因为最后一行/列可能没有完整绿框）
                        int expectedWidth = fileInfo.col * (fileInfo.width + 1);
                        int expectedHeight = fileInfo.row * (fileInfo.height + 1);

                        int actualRow = fileInfo.row;
                        int actualCol = fileInfo.col;

                        // 辅助函数：检查尺寸是否接近匹配（允许±1像素误差）
                        bool SizeMatches(int actual, int expected)
                        {
                            return Mathf.Abs(actual - expected) <= 1;
                        }

                        // 如果尺寸不匹配，尝试交换 row 和 col
                        if (!SizeMatches(originalTexture.width, expectedWidth) || !SizeMatches(originalTexture.height, expectedHeight))
                        {
                            Debug.LogWarning($"<color=yellow>[精灵裁剪] 纹理尺寸不匹配！期望={expectedWidth}x{expectedHeight}, 实际={originalTexture.width}x{originalTexture.height}</color>");

                            // 尝试交换 row 和 col
                            int swappedExpectedWidth = fileInfo.row * (fileInfo.width + 1);
                            int swappedExpectedHeight = fileInfo.col * (fileInfo.height + 1);

                            if (SizeMatches(originalTexture.width, swappedExpectedWidth) && SizeMatches(originalTexture.height, swappedExpectedHeight))
                            {
                                Debug.LogWarning($"<color=orange>[精灵裁剪] 检测到 row/col 可能反了，自动交换：{fileInfo.row}行x{fileInfo.col}列 → {fileInfo.col}行x{fileInfo.row}列</color>");
                                actualRow = fileInfo.col;
                                actualCol = fileInfo.row;
                            }
                            else
                            {
                                Debug.LogWarning($"<color=yellow>[精灵裁剪] 尺寸略有差异，继续处理。交换期望={swappedExpectedWidth}x{swappedExpectedHeight}</color>");
                            }
                        }

                        // 先切割精灵（此时还包含绿框）
                        List<Sprite> sprites = RuntimeSpriteProcessor.SliceTextureFromTopLeft(
                            originalTexture,
                            fileInfo.width,
                            fileInfo.height,
                            actualRow,
                            actualCol
                        );

                        Debug.Log($"<color=yellow>[精灵裁剪] 裁剪完成，得到 {sprites.Count} 个精灵</color>");

                        // 对每个精灵单独处理透明（避免绿框颜色渗透）
                        for (int i = 0; i < sprites.Count; i++)
                        {
                            Sprite sprite = sprites[i];

                            // ✅ 记录原始精灵的尺寸（应该是统一的，如 79×79）
                            int originalWidth = (int)sprite.rect.width;
                            int originalHeight = (int)sprite.rect.height;

                            // 获取精灵的纹理区域
                            Texture2D spriteTexture = GetSpriteTexture(sprite);

                            // 处理黑色透明
                            Texture2D processedTexture = RuntimeSpriteProcessor.MakeColorTransparent_Debleeding_AvoidBorder(
                                spriteTexture,
                                transparentData
                            );

                            // ✅ 验证处理后的纹理尺寸是否与原始一致
                            if (processedTexture.width != originalWidth || processedTexture.height != originalHeight)
                            {
                                Debug.LogWarning($"<color=yellow>[透明处理] 纹理尺寸变化：原始={originalWidth}x{originalHeight}, " +
                                                $"处理后={processedTexture.width}x{processedTexture.height}, 精灵={sprite.name}</color>");
                            }

                            // 创建新的精灵（使用处理后的纹理）
                            // ✅ 设置锚点为底部中心（0.5, 0），防止序列帧播放时上下漂移
                            // ✅ 强制使用原始尺寸，确保所有帧统一
                            Sprite newSprite = Sprite.Create(
                                processedTexture,
                                new Rect(0, 0, originalWidth, originalHeight),  // 使用固定的原始尺寸
                                new Vector2(0.5f, 0f),
                                100f
                            );
                            newSprite.name = sprite.name;

                            sprites[i] = newSprite;
                        }

                        // 将精灵放置到指定索引位置（而不是顺序添加）
                        int placedCount = 0;
                        for (int i = 0; i < sprites.Count; i++)
                        {
                            int targetIndex = fileInfo.startFrame + i;
                            if (targetIndex >= 0 && targetIndex < totalSpriteCount && targetIndex <= fileInfo.endFrame)
                            {
                                allSprites[targetIndex] = sprites[i];
                                placedCount++;
                            }
                            else
                            {
                                Debug.LogWarning($"<color=yellow>[精灵放置] 索引超出范围: targetIndex={targetIndex}, 总大小={totalSpriteCount}, endFrame={fileInfo.endFrame}</color>");
                            }
                        }

                        Debug.Log($"<color=green>✅ 加载精灵文件: 角色ID={characterId}, 文件={Path.GetFileName(filePath)}, " +
                                 $"切割={fileInfo.row}x{fileInfo.col}, 数量={sprites.Count}, 已放置={placedCount}, 帧范围=[{fileInfo.startFrame}-{fileInfo.endFrame}]</color>");
                    }

                    // 存储到字典中，使用 characterId 作为 key
                    MergedSprites[characterId] = allSprites;

                    Debug.Log($"<color=cyan>角色 {characterId} ({config.characterData.name}) 精灵加载完成，总数量={allSprites.Count}</color>");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<color=red>❌ 加载角色精灵失败: 角色ID={config.characterId}\n{e.Message}\n{e.StackTrace}</color>");
                }
            }

            Debug.Log($"<color=cyan>精灵加载完成，共 {MergedSprites.Count} 个角色</color>");
        }

        /// <summary>
        /// 加载 BMP 文件为 Texture2D
        /// 使用增强的 BMP 加载器，支持详细诊断和手动解析
        /// </summary>
        private Texture2D LoadBMPTexture(string filePath)
        {
            return BMPLoader.LoadBMP(filePath);
        }

        /// <summary>
        /// 从 Sprite 中提取纹理数据
        /// </summary>
        private Texture2D GetSpriteTexture(Sprite sprite)
        {
            Rect rect = sprite.rect;
            Texture2D sourceTexture = sprite.texture;

            int width = (int)rect.width;
            int height = (int)rect.height;

            // ⭐ 诊断日志：检查源纹理状态
            if (sourceTexture == null)
            {
                Debug.LogError($"<color=red>[GetSpriteTexture] sourceTexture 为 null！sprite={sprite.name}</color>");
                return null;
            }

            Debug.Log($"<color=cyan>[GetSpriteTexture] sprite={sprite.name}, rect=({rect.x},{rect.y},{rect.width}x{rect.height}), " +
                     $"sourceTexture尺寸={sourceTexture.width}x{sourceTexture.height}, 可读={sourceTexture.isReadable}</color>");

            // ⭐ 检查rect是否在纹理范围内
            if (rect.x < 0 || rect.y < 0 || rect.x + rect.width > sourceTexture.width || rect.y + rect.height > sourceTexture.height)
            {
                Debug.LogError($"<color=red>[GetSpriteTexture] rect超出sourceTexture范围！rect=({rect.x},{rect.y},{rect.width}x{rect.height}), " +
                              $"texture=({sourceTexture.width}x{sourceTexture.height})</color>");
            }

            Texture2D newTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // ✅ 设置为像素艺术过滤模式
            newTexture.filterMode = FilterMode.Point;
            newTexture.wrapMode = TextureWrapMode.Clamp;

            try
            {
                // ⭐ 先直接读取sourceTexture的前几个像素，验证texture本身是否有数据
                Color[] sourcePixels = sourceTexture.GetPixels(0, 0, Mathf.Min(10, sourceTexture.width), 1);
                Debug.Log($"<color=magenta>[GetSpriteTexture-诊断] sourceTexture前10个像素样本: " +
                         $"[0]={sourcePixels[0]}, [1]={sourcePixels[1]}, [2]={sourcePixels[2]}</color>");

                // 从源纹理复制像素
                Color[] pixels = sourceTexture.GetPixels(
                    (int)rect.x,
                    (int)rect.y,
                    width,
                    height
                );

                // ⭐ 诊断日志：检查pixel数据
                if (pixels == null || pixels.Length == 0)
                {
                    Debug.LogError($"<color=red>[GetSpriteTexture] GetPixels返回空数据！</color>");
                }
                else
                {
                    // 检查前几个像素是否全黑或全透明
                    int sampleCount = Mathf.Min(10, pixels.Length);
                    bool allBlack = true;
                    bool allTransparent = true;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        if (pixels[i].r > 0.01f || pixels[i].g > 0.01f || pixels[i].b > 0.01f)
                            allBlack = false;
                        if (pixels[i].a > 0.01f)
                            allTransparent = false;
                    }

                    if (allBlack)
                        Debug.LogWarning($"<color=yellow>[GetSpriteTexture] 前{sampleCount}个像素全黑！sprite={sprite.name}</color>");
                    if (allTransparent)
                        Debug.LogWarning($"<color=yellow>[GetSpriteTexture] 前{sampleCount}个像素全透明！sprite={sprite.name}</color>");

                    Debug.Log($"<color=gray>[GetSpriteTexture] 成功提取 {pixels.Length} 个像素，前3个样本: " +
                             $"[0]={pixels[0]}, [1]={(pixels.Length > 1 ? pixels[1].ToString() : "N/A")}, " +
                             $"[2]={(pixels.Length > 2 ? pixels[2].ToString() : "N/A")}</color>");
                }

                newTexture.SetPixels(pixels);
                newTexture.Apply();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>[GetSpriteTexture] GetPixels异常: {e.Message}\n{e.StackTrace}</color>");
                return null;
            }

            return newTexture;
        }


        #endregion

        #region 公开接口

        public LF2CharacterDataWrapper GetCharacterConfig(int id)
        {
            TotalCharacterFrameConfig.TryGetValue(id, out LF2CharacterDataWrapper wrapper);
            return wrapper;
        }

        public LF2CharacterData GetCharacterData(int id)
        {
            return GetCharacterConfig(id)?.characterData;
        }

        public List<LF2FrameData> GetCharacterFrames(int id)
        {
            return GetCharacterData(id)?.frames;
        }

        public LF2FrameData GetFrameData(int characterId, int frameId)
        {
            var frames = GetCharacterFrames(characterId);
            return frames?.FirstOrDefault(f => f.frameId == frameId);
        }

        public string GetCharacterName(int id)
        {
            return GetCharacterData(id)?.name ?? "Unknown";
        }

        public List<Sprite> GetCharacterSpriteByID(int id)
        {
            MergedSprites.TryGetValue(id, out List<Sprite> sprites);
            if(sprites == null)
                Debug.LogError($"未找到ID为{id}的精灵");
            return sprites;
        }

        public bool IsCharacterLoaded(int id)
        {
            return TotalCharacterFrameConfig.ContainsKey(id);
        }

        public List<int> GetAllLoadedCharacterIds()
        {
            return TotalCharacterFrameConfig.Keys.ToList();
        }

        public void PrintCharacterInfo(int id)
        {
            var wrapper = GetCharacterConfig(id);
            if (wrapper != null)
            {
                var data = wrapper.characterData;
                Debug.Log($"<color=cyan>===== 角色信息 =====</color>\n" +
                         $"<color=yellow>ID:</color> {wrapper.characterId}\n" +
                         $"<color=yellow>名称:</color> {data.name}\n" +
                         $"<color=yellow>帧数:</color> {data.frames.Count}\n" +
                         $"<color=yellow>精灵图:</color> {data.files.Count}\n" +
                         $"<color=yellow>行走速度:</color> {data.walking_speed}\n" +
                         $"<color=yellow>奔跑速度:</color> {data.running_speed}");
            }
            else
            {
                Debug.LogWarning($"未找到ID为{id}的角色");
            }
        }

        #endregion

        #region Odin Inspector Helper

        private IEnumerable<int> GetLoadedCharacterIds()
        {
            return TotalCharacterFrameConfig.Keys.OrderBy(x => x);
        }

        #endregion
    }
}
