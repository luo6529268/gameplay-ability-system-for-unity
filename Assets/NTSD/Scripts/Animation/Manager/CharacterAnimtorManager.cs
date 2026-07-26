using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.UI;
using NTSD.Animation.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Sirenix.OdinInspector;
using NTSD.DatParser;


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
        public bool IsPrewarmCompleted { get; private set; }
        public event System.Action PrewarmCompleted;

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
        private Dictionary<int, LF2CharacterDataWrapper> pendingCharacterFrameConfig;
        private int spritePrewarmGeneration;
        private bool spritePrewarmDisposed;
        private HashSet<Sprite> publishedOwnedSprites = new HashSet<Sprite>();
        private HashSet<UnityEngine.Object> publishedOwnedResources = new HashSet<UnityEngine.Object>();
        private readonly Dictionary<BattleSpriteCatalog, SpritePublicationOwnership> retiredSpritePublications =
            new Dictionary<BattleSpriteCatalog, SpritePublicationOwnership>();
        private readonly Dictionary<BattleSpriteCatalog, int> spriteCatalogRendererBindings =
            new Dictionary<BattleSpriteCatalog, int>();

        private sealed class SpritePublicationOwnership
        {
            public readonly HashSet<Sprite> Sprites;
            public readonly HashSet<UnityEngine.Object> Resources;
            public readonly BattleSpriteCatalog Catalog;

            public SpritePublicationOwnership(
                BattleSpriteCatalog catalog,
                HashSet<Sprite> sprites,
                HashSet<UnityEngine.Object> resources)
            {
                Catalog = catalog;
                Sprites = sprites ?? new HashSet<Sprite>();
                Resources = resources ?? new HashSet<UnityEngine.Object>();
            }
        }

        private sealed class SparkPublicationStaging
        {
            public Texture2D Texture;
            public Sprite[] Sprites;
            public string SourcePath;
            public Color32[] ProcessedPixels;
        }

        private sealed class WordsPublicationStaging
        {
            public Texture2D[] Textures;
            public Sprite[][] GlyphSprites;
            public string[] SourcePaths;
            public Color32[][] ProcessedPixels;
        }

        [ShowInInspector, ReadOnly]
        [DictionaryDrawerSettings(
            KeyLabel = "角色ID",
            ValueLabel = "精灵列表",
            DisplayMode = DictionaryDisplayOptions.Foldout
        )]
        [PropertyOrder(-1)]
        private Dictionary<int, List<Sprite>> MergedSprites = new Dictionary<int, List<Sprite>>(10);

        public BattleSpriteCatalog SpriteCatalog { get; private set; } = BattleSpriteCatalog.Empty;
        public BattleCommonVisualCatalog CommonVisualCatalog { get; private set; } = BattleCommonVisualCatalog.Empty;
        public string LastAtlasDiagnostic { get; private set; } = string.Empty;
        public BattleAtlasPolicyDecision LastAtlasPolicyDecision { get; private set; }
        public BattleAtlasDiagnosticInputs LastAtlasDiagnosticInputs { get; private set; }

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
#if UNITY_EDITOR
            EditorApplication.delayCall += async () =>
            {
                try
                {
                    var dataManager = GameDataManager.Instance;
                    if (dataManager == null) { Debug.LogError("GameDataManager.Instance is null"); return; }
                    Debug.Log("<color=cyan>[Editor] start loading configs...</color>");
                    var configs = ParseCharacterFrameConfigs(dataManager, t => Debug.Log($"[Editor] {t}"));
                    ApplyLoadedCharacterConfigs(configs);
                    Debug.Log("<color=cyan>[Editor] configs loaded, loading sprites...</color>");
                    await LoadCharacterSpritesAsync(t => Debug.Log($"[Editor] {t}"));
                    Debug.Log("<color=cyan>[Editor] all data loaded</color>");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Editor] load failed: {e.Message}\n{e.StackTrace}");
                }
            };
#endif
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
#if UNITY_EDITOR
            if (EditorUtility.DisplayDialog("确认清空", "确定要清空所有已加载的数据吗？", "确定", "取消"))
            {
                TotalCharacterFrameConfig.Clear();
                MergedSprites.Clear();
                pendingCharacterFrameConfig = null;
                spritePrewarmGeneration++;
                InvalidateSpriteCatalog();
                Debug.Log("所有数据已清空");
            }
#endif
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

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
        }

        public void ApplyLoadedCharacterConfigs(Dictionary<int, LF2CharacterDataWrapper> configs)
        {
            if (spritePrewarmDisposed || configs == null || configs.Count == 0)
            {
                return;
            }

            var pending = new Dictionary<int, LF2CharacterDataWrapper>(configs.Count);
            foreach (var kvp in configs)
            {
                pending[kvp.Key] = kvp.Value;
            }
            pendingCharacterFrameConfig = pending;
            spritePrewarmGeneration++;
        }

        /// <summary>
        /// 加载所有角色帧配置
        /// </summary>
        public Dictionary<int, LF2CharacterDataWrapper> ParseCharacterFrameConfigs(GameDataManager dataManager, Action<string> onProgressText = null)
        {
            string dataFilePath = Path.Combine(TotalCharacterFrameConfigPath, "../data.txt");
            string fullDataPath = Path.GetFullPath(dataFilePath);

            if (!File.Exists(fullDataPath))
            {
                Debug.LogError($"<color=red>data.txt 文件不存在: {fullDataPath}</color>");
                return null;
            }

            onProgressText?.Invoke("data.txt");

            Dictionary<int, ObjectDefinition> dataObjectMap = new Dictionary<int, ObjectDefinition>();
            dataManager?.LoadDataFile(fullDataPath);
            var allObjects = dataManager?.GetAllObjects();
            if (allObjects != null)
                foreach (var obj in allObjects)
                    dataObjectMap[obj.id] = obj;

            if (dataObjectMap == null || dataObjectMap.Count == 0)
            {
                Debug.LogError("<color=red>data.txt 解析失败或没有对象定义</color>");
                return null;
            }

            int characterCount = 0;
            foreach (var kvp in dataObjectMap)
            {
                if (kvp.Value.type == 0) characterCount++;
            }
            Debug.Log($"<color=cyan>开始加载角色配置，data.txt 中共 {dataObjectMap.Count} 个对象定义，其中 {characterCount} 个角色 (type==0)</color>");

            int loadedCount = 0;
            var result = new Dictionary<int, LF2CharacterDataWrapper>(dataObjectMap.Count);
            foreach (var kvp in dataObjectMap)
            {
                int characterId = kvp.Key;
                ObjectDefinition objectData = kvp.Value;

                try
                {
                    string configDir = Path.GetDirectoryName(fullDataPath);
                    string datFilePath = GameDataManager.ResolveObjectFilePath(configDir, objectData.file);
                    datFilePath = Path.ChangeExtension(datFilePath, ".dat");

                    onProgressText?.Invoke(Path.GetFileName(datFilePath));

                    if (!File.Exists(datFilePath))
                    {
                        Debug.LogWarning($"<color=yellow>DAT文件不存在: ID={characterId}, 文件={datFilePath}</color>");
                        continue;
                    }

                    string datText = Lf2DatDecryptor.DecryptFile(datFilePath, "odBearBecauseHeIsVeryGoodSiuHungIsAGo");

                    if (string.IsNullOrEmpty(datText))
                    {
                        Debug.LogWarning($"<color=yellow>DAT文件解密返回空: ID={characterId}, 文件={Path.GetFileName(datFilePath)}</color>");
                        continue;
                    }

                    Lf2DatParserV2 parser = new Lf2DatParserV2();
                    Lf2DatFile datFile = parser.Parse(datText, datFilePath);

                    if (datFile == null || datFile.Frames.Count == 0)
                    {
                        Debug.LogWarning($"<color=yellow>DAT文件解析失败或无帧数据: ID={characterId}, 文件={Path.GetFileName(datFilePath)}</color>");
                        continue;
                    }

                    string datFileDirectory = Path.GetDirectoryName(datFilePath);
                    LF2CharacterData characterData = BuildCharacterDataFromDat(datFile, datFileDirectory);

                    // C++ release：oid 由调用方传入，不在 dat 文件内容里。
                    // type_sub 字段直接等于 characterId（oid），dat 文件里不存在此字段
                    if (characterData.type_sub == 0)
                        characterData.type_sub = characterId;

                    LF2CharacterDataWrapper wrapper = new LF2CharacterDataWrapper(characterId, characterData);

                    if (wrapper != null && wrapper.characterData != null)
                    {
                        result[characterId] = wrapper;
                        loadedCount++;
                        Debug.Log($"<color=green>加载角色: ID={characterId}, 名称={characterData.name}, 帧数={characterData.frames.Count}</color>");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<color=red>加载失败: ID={characterId}, 文件={objectData.file}\n错误: {e.Message}</color>");
                }
            }

            Debug.Log($"<color=cyan>配置加载完成，成功加载 {loadedCount}/{dataObjectMap.Count} 个角色</color>");
            return result;
        }

        public void SetCharacterSprites(int characterId, List<Sprite> sprites)
        {
            if (sprites == null)
            {
                return;
            }

            MergedSprites[characterId] = sprites;
            InvalidateSpriteCatalog();
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

                // ⬇️ 需要添加这两行 ⬇️
                characterData.head = datFile.Bmp.Head ?? "";
                characterData.small = datFile.Bmp.Small ?? "";

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

            // 4. 提取武器专用参数（weapon_hp, weapon_strength_list 等）
            ExtractWeaponParameters(datFile, characterData);

            return characterData;
        }

        /// <summary>
        /// 提取武器专用顶层参数（weapon_hp, weapon_drop_hurt, weapon_strength_list 等）
        /// </summary>
        private void ExtractWeaponParameters(Lf2DatFile datFile, LF2CharacterData characterData)
        {
            // 从根属性读取 weapon_hp / weapon_drop_hurt / sound 路径
            foreach (var prop in datFile.Properties)
            {
                ApplyWeaponProperty(prop.Key, prop.Value, characterData);
            }
            // weapon_hp 等字段在 <bmp_begin>...<bmp_end> 内，存储在 datFile.Bmp.Properties
            if (datFile.Bmp != null)
            {
                foreach (var prop in datFile.Bmp.Properties)
                {
                    ApplyWeaponProperty(prop.Key, prop.Value, characterData);
                }
            }
            foreach (var block in datFile.Blocks)
            {
                foreach (var prop in block.Properties)
                {
                    ApplyWeaponProperty(prop.Key, prop.Value, characterData);
                }
            }

            // 解析 weapon_strength_list 块（如果存在）
            var wslBlock = datFile.Blocks.Find(b =>
                string.Equals(b.Name, "weapon_strength_list", StringComparison.OrdinalIgnoreCase));
            if (wslBlock == null) return;

            WeaponStrengthEntry current = null;
            foreach (var prop in wslBlock.Properties)
            {
                string k = prop.Key.ToLower();
                string v = prop.Value ?? "";

                if (k == "entry")
                {
                    current = new WeaponStrengthEntry();
                    int.TryParse(v, out current.index);
                    characterData.weapon_strength_list.Add(current);
                    continue;
                }

                if (current == null) continue;

                int intVal = 0;
                int.TryParse(v, out intVal);
                switch (k)
                {
                    case "dvx": current.dvx = intVal; break;
                    case "dvy": current.dvy = intVal; break;
                    case "fall": current.fall = intVal; break;
                    case "vrest": current.vrest = intVal; break;
                    case "arest": current.arest = intVal; break;
                    case "bdefend": current.bdefend = intVal; break;
                    case "injury": current.injury = intVal; break;
                    case "effect": current.effect = intVal; break;
                }
            }
        }

        private void ApplyWeaponProperty(string key, string value, LF2CharacterData data)
        {
            switch (key.ToLower())
            {
                case "weapon_hp":
                    int.TryParse(value, out data.weapon_hp); break;
                case "weapon_drop_hurt":
                    int.TryParse(value, out data.weapon_drop_hurt); break;
                case "weapon_hit_sound":
                    data.weapon_hit_sound = value; break;
                case "weapon_drop_sound":
                    data.weapon_drop_sound = value; break;
                case "weapon_broken_sound":
                    data.weapon_broken_sound = value; break;
            }
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

            if (datFile.Bmp != null)
            {
                foreach (var prop in datFile.Bmp.Properties)
                {
                    ApplyMovementProperty(prop.Key, prop.Value, characterData);
                }
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
            string normalizedKey = key.ToLowerInvariant();
            if (normalizedKey == "walking_frame_rate" || normalizedKey == "running_frame_rate")
            {
                if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int frameRate))
                    return;

                if (normalizedKey == "walking_frame_rate")
                    characterData.walking_frame_rate = frameRate;
                else
                    characterData.running_frame_rate = frameRate;
                return;
            }

            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                return;

            // 处理浮点数类型的参数
            switch (normalizedKey)
            {
                // 行走参数
                case "walking_speed":
                    characterData.walking_speed = floatValue;
                    break;
                case "walking_speedz":
                    characterData.walking_speedz = floatValue;
                    break;

                // 奔跑参数
                case "running_speed":
                    characterData.running_speed = floatValue;
                    break;
                case "running_speedz":
                    characterData.running_speedz = floatValue;
                    break;

                // 负重行走参数
                case "heavy_walking_speed":
                    characterData.heavy_walking_speed = floatValue;
                    break;
                case "heavy_walking_speedz":
                    characterData.heavy_walking_speedz = floatValue;
                    break;

                // 负重奔跑参数
                case "heavy_running_speed":
                    characterData.heavy_running_speed = floatValue;
                    break;
                case "heavy_running_speedz":
                    characterData.heavy_running_speedz = floatValue;
                    break;

                // 跳跃参数
                case "jump_height":
                    characterData.jump_height = floatValue;
                    break;
                case "jump_distance":
                    characterData.jump_distance = floatValue;
                    break;
                case "jump_distancez":
                    characterData.jump_distancez = floatValue;
                    break;

                // 冲刺参数
                case "dash_height":
                    characterData.dash_height = floatValue;
                    break;
                case "dash_distance":
                    characterData.dash_distance = floatValue;
                    break;
                case "dash_distancez":
                    characterData.dash_distancez = floatValue;
                    break;

                // 翻滚参数
                case "rowing_height":
                    characterData.rowing_height = floatValue;
                    break;
                case "rowing_distance":
                    characterData.rowing_distance = floatValue;
                    break;
            }
        }

        /// <summary>
        /// 解析精灵图路径
        /// 支持Unity项目相对路径（Assets/开头）和dat文件相对路径
        /// </summary>
        /// <param name="pathInDat">dat文件中的路径</param>
        /// <param name="datFileDirectory">dat文件所在目录</param>
        /// <returns>绝对路径</returns>
        private string ResolveSpritePath(string pathInDat, string datFileDirectory)
        {
            string normalizedPath = pathInDat.Replace("\\", "/");

            // 检查是否为Unity项目相对路径（以Assets/开头）
            if (normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                return Path.GetFullPath(Path.Combine(projectRoot, normalizedPath));
            }

            // 相对于dat文件所在目录
            return Path.GetFullPath(Path.Combine(datFileDirectory, normalizedPath));
        }

        /// <summary>
        /// 预加载所有角色精灵
        /// 使用并行后台线程处理所有文件的像素数据
        /// </summary>
        public async UniTask LoadCharacterSpritesAsync(Action<string> onProgressText)
        {
            if (spritePrewarmDisposed)
                return;

            int invocation = BeginSpritePrewarmInvocation();
            Dictionary<int, LF2CharacterDataWrapper> configSource =
                pendingCharacterFrameConfig ?? TotalCharacterFrameConfig;
            var stagedConfigs = new Dictionary<int, LF2CharacterDataWrapper>(configSource);
            var stagedSprites = new Dictionary<int, List<Sprite>>(stagedConfigs.Count);
            var stagedCreatedSprites = new HashSet<Sprite>();
            var stagedTextures = new HashSet<Texture2D>();
            var stagedAtlasSources = new List<BattleAtlasSourcePixels>();

            Debug.Log($"<color=cyan>开始加载精灵，角色配置数量: {stagedConfigs.Count}</color>");

            var allFileInfos = new List<(int characterId, SpriteFileInfo fileInfo)>();

            foreach (var config in stagedConfigs.Values)
            {
                int characterId = config.characterId;
                int totalSpriteCount = config.characterData.files.Count > 0
                    ? config.characterData.files.Max(fileInfo => fileInfo.endFrame) + 1
                    : 0;
                stagedSprites[characterId] = new List<Sprite>(new Sprite[totalSpriteCount]);

                foreach (var fileInfo in config.characterData.files)
                {
                    allFileInfos.Add((characterId, fileInfo));
                }
            }

            Debug.Log($"<color=cyan>共 {allFileInfos.Count} 个文件需要处理</color>");

            int totalCreated = 0;
            int failedSheets = 0;
            int concurrentLimit = Mathf.Clamp(System.Environment.ProcessorCount, 1, 4);
            var cpuSemaphore = new System.Threading.SemaphoreSlim(concurrentLimit);
            var uploadSemaphore = new System.Threading.SemaphoreSlim(1);
            var pendingTasks = new List<UniTask>();

            foreach (var (characterId, fileInfo) in allFileInfos)
            {
                await cpuSemaphore.WaitAsync();
                var task = ProcessAndCreateSpritesAsync(
                        characterId,
                        fileInfo,
                        stagedSprites,
                        stagedCreatedSprites,
                        stagedTextures,
                        stagedAtlasSources,
                        onProgressText,
                        cpuSemaphore,
                        uploadSemaphore)
                    .ContinueWith(count =>
                    {
                        if (count < 0)
                            System.Threading.Interlocked.Increment(ref failedSheets);
                        else
                            System.Threading.Interlocked.Add(ref totalCreated, count);
                    });
                pendingTasks.Add(task);
            }

            await UniTask.WhenAll(pendingTasks);

            if (!CanCompleteSpritePrewarmInvocation(invocation))
            {
                await UniTask.SwitchToMainThread();
                DestroyStagedPresentation(stagedCreatedSprites, stagedTextures);
                return;
            }

            if (failedSheets > 0)
            {
                await UniTask.SwitchToMainThread();
                DestroyStagedPresentation(stagedCreatedSprites, stagedTextures);
                throw new InvalidOperationException(
                    $"Battle sprite prewarm failed for {failedSheets} sheet(s); the previous catalog remains published.");
            }

            BattleSpriteCatalog stagedCatalog;
            var stagedResources = new HashSet<UnityEngine.Object>();
            foreach (Texture2D texture in stagedTextures)
                stagedResources.Add(texture);
            string atlasDiagnostic = string.Empty;
            BattleAtlasPolicyDecision atlasPolicyDecision = null;
            BattleAtlasDiagnosticInputs atlasDiagnosticInputs = null;
            SparkPublicationStaging stagedSpark = null;
            WordsPublicationStaging stagedWords = null;
            BattleCommonVisualCatalog commonVisualCatalog = BattleCommonVisualCatalog.Empty;
            try
            {
                stagedCatalog = BuildBattleSpriteCatalog(stagedConfigs, stagedSprites);
                stagedSpark = await BuildSparkPublicationAsync(invocation);
                if (stagedSpark == null || stagedSpark.Texture == null ||
                    stagedSpark.Sprites == null || stagedSpark.ProcessedPixels == null)
                    throw new InvalidOperationException("SPARK.bmp could not be decoded into the common 20-frame publication.");
                stagedTextures.Add(stagedSpark.Texture);
                stagedResources.Add(stagedSpark.Texture);
                foreach (Sprite sparkSprite in stagedSpark.Sprites)
                {
                    if (sparkSprite != null)
                        stagedCreatedSprites.Add(sparkSprite);
                }

                stagedWords = await BuildWordsPublicationAsync(invocation);
                if (stagedWords == null || stagedWords.Textures == null ||
                    stagedWords.GlyphSprites == null || stagedWords.ProcessedPixels == null ||
                    stagedWords.SourcePaths == null)
                {
                    throw new InvalidOperationException(
                        "WORDS0.bmp through WORDS5.bmp could not be decoded into the common glyph publication.");
                }

                foreach (Texture2D wordsTexture in stagedWords.Textures)
                {
                    if (wordsTexture == null)
                        throw new InvalidOperationException("WORDS publication contains a missing texture.");
                    stagedTextures.Add(wordsTexture);
                    stagedResources.Add(wordsTexture);
                }

                foreach (Sprite[] glyphs in stagedWords.GlyphSprites)
                {
                    if (glyphs == null)
                        throw new InvalidOperationException("WORDS publication contains a missing glyph page.");
                    foreach (Sprite glyph in glyphs)
                    {
                        if (glyph != null)
                            stagedCreatedSprites.Add(glyph);
                    }
                }

                await UniTask.SwitchToMainThread();
                if (!CanCompleteSpritePrewarmInvocation(invocation))
                {
                    DestroyStagedPresentation(stagedCreatedSprites, stagedResources);
                    return;
                }

                commonVisualCatalog = BattleCommonVisualCatalog.Build(
                    NTSD.App.GameConfig.Instance?.ShadowPrefab,
                    stagedSpark.Texture,
                    stagedSpark.Sprites,
                    stagedWords.Textures,
                    stagedWords.GlyphSprites);
                if (!commonVisualCatalog.IsComplete)
                    throw new InvalidOperationException(commonVisualCatalog.Diagnostic);

                var commonSourcePaths =
                    new Dictionary<BattleVisualResourceKey, string>(
                        1 + BattleCommonVisualCatalog.SparkFrameCount +
                        BattleCommonVisualCatalog.WordSheetCount *
                        BattleCommonVisualCatalog.WordGlyphsPerSheet);
                var forcedCommonSource2DPaths = new List<string>();
                if (!TryAppendCommonAtlasSources(
                        commonVisualCatalog,
                        stagedSpark,
                        stagedWords,
                        stagedAtlasSources,
                        commonSourcePaths,
                        forcedCommonSource2DPaths,
                        out string commonSourceDiagnostic))
                {
                    throw new InvalidOperationException(commonSourceDiagnostic);
                }

                if (!TryBuildUnifiedCentralAtlasPublication(
                        stagedCatalog,
                        commonVisualCatalog,
                        stagedAtlasSources,
                        commonSourcePaths,
                        forcedCommonSource2DPaths,
                        BattleRenderingDeviceCapabilities.FromSystem(),
                        NTSD.App.GameConfig.Instance,
                        null,
                        out stagedCatalog,
                        out commonVisualCatalog,
                        out HashSet<UnityEngine.Object> atlasResources,
                        out atlasDiagnostic,
                        out atlasPolicyDecision,
                        out atlasDiagnosticInputs))
                {
                    throw new InvalidOperationException($"Battle atlas publication failed: {atlasDiagnostic}");
                }
                stagedResources.UnionWith(atlasResources);
                atlasDiagnostic = CombineAtlasDiagnostics(atlasDiagnostic, commonSourceDiagnostic);
            }
            catch
            {
                await UniTask.SwitchToMainThread();
                DestroyStagedPresentation(stagedCreatedSprites, stagedResources);
                throw;
            }

            await UniTask.SwitchToMainThread();
            if (!TryCommitSpritePrewarmInvocation(
                    invocation,
                    stagedConfigs,
                    stagedSprites,
                    stagedCatalog,
                    stagedCreatedSprites,
                    null,
                    stagedResources,
                    atlasDiagnostic,
                    commonVisualCatalog))
            {
                DestroyStagedPresentation(stagedCreatedSprites, stagedResources);
                return;
            }
            LastAtlasPolicyDecision = atlasPolicyDecision;
            LastAtlasDiagnosticInputs = atlasDiagnosticInputs;
            BattleCentralRenderSystem.ResolveDrawPolicyForPublication(NTSD.App.GameConfig.Instance);

            Debug.Log($"<color=cyan>精灵加载完成，共创建 {totalCreated} 个精灵</color>");

            // Continue UI loading only while this publication is current.
            if (!CanCompleteSpritePrewarmInvocation(invocation))
                return;
            await LoadAllCharacterUISpritesAsync(invocation);

            if (!CanCompleteSpritePrewarmInvocation(invocation))
                return;
            PrewarmCompleted?.Invoke();
        }

        private async UniTask<SparkPublicationStaging> BuildSparkPublicationAsync(int invocation)
        {
            string sparkPath = Path.Combine(
                Application.dataPath,
                "NTSD", "Sprite", "UIPanels", "SPARK.bmp");
            BMPLoader.BmpData bmpData = await UniTask.RunOnThreadPool(() => BMPLoader.LoadBmpData(sparkPath));
            if (bmpData == null || bmpData.Pixels == null ||
                bmpData.Width < 510 || bmpData.Height != 256)
            {
                return null;
            }

            var transparency = new TransparentColorData
            {
                targetColor = Color.black,
                colorTolerance = 0.1f
            };
            Color32[] processedPixels = await UniTask.RunOnThreadPool(() =>
            {
                Color[] colors = RuntimeSpriteProcessor.ProcessColorTransparencyPixels(
                    bmpData.Pixels,
                    transparency,
                    out _);
                return ConvertToColor32(colors);
            });
            if (processedPixels == null || processedPixels.Length != bmpData.Width * bmpData.Height ||
                !CanCompleteSpritePrewarmInvocation(invocation))
            {
                return null;
            }

            await UniTask.SwitchToMainThread();
            if (!CanCompleteSpritePrewarmInvocation(invocation))
                return null;

            Texture2D texture = null;
            Sprite[] sprites = null;
            bool transfersOwnership = false;
            try
            {
                texture = new Texture2D(bmpData.Width, bmpData.Height, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels32(processedPixels);
                texture.Apply(false, true);
                texture.name = "SPARK";

                sprites = new Sprite[BattleCommonVisualCatalog.SparkFrameCount];
                for (int pic = 0; pic < sprites.Length; pic++)
                {
                    if (!CanCompleteSpritePrewarmInvocation(invocation))
                        return null;

                    Rect rect = BattleCommonVisualCatalog.GetSparkPixelRect(pic);
                    Vector2 pivot = BattleCommonVisualCatalog.GetSparkPivotNormalized(pic);
                    Sprite sprite = Sprite.Create(texture, rect, pivot, 100f, 0, SpriteMeshType.FullRect);
                    sprite.name = $"spark_{pic:D2}";
                    sprites[pic] = sprite;
                }

                if (!CanCompleteSpritePrewarmInvocation(invocation))
                    return null;

                transfersOwnership = true;
                return new SparkPublicationStaging
                {
                    Texture = texture,
                    Sprites = sprites,
                    SourcePath = sparkPath,
                    ProcessedPixels = processedPixels,
                };
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to create the common SPARK publication: {exception.Message}");
                return null;
            }
            finally
            {
                if (!transfersOwnership)
                    DestroySparkPublicationStaging(texture, sprites);
            }
        }

        private static Color32[] ConvertToColor32(Color[] colors)
        {
            if (colors == null)
                return null;

            var pixels = new Color32[colors.Length];
            for (int index = 0; index < colors.Length; index++)
                pixels[index] = colors[index];
            return pixels;
        }

        private static void DestroySparkPublicationStaging(Texture2D texture, Sprite[] sprites)
        {
            if (sprites != null)
            {
                for (int pic = 0; pic < sprites.Length; pic++)
                {
                    Sprite sprite = sprites[pic];
                    if (sprite == null)
                        continue;
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(sprite);
                    else
                        UnityEngine.Object.DestroyImmediate(sprite);
                }
            }

            if (texture == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(texture);
            else
                UnityEngine.Object.DestroyImmediate(texture);
        }

        private async UniTask<WordsPublicationStaging> BuildWordsPublicationAsync(int invocation)
        {
            var bmpData = new BMPLoader.BmpData[BattleCommonVisualCatalog.WordSheetCount];
            var sourcePaths = new string[BattleCommonVisualCatalog.WordSheetCount];
            for (int sheetIndex = 0; sheetIndex < bmpData.Length; sheetIndex++)
            {
                int capturedSheetIndex = sheetIndex;
                string wordsPath = Path.Combine(
                    Application.dataPath,
                    "NTSD", "Sprite", "UIPanels", $"WORDS{capturedSheetIndex}.bmp");
                sourcePaths[capturedSheetIndex] = wordsPath;
                bmpData[capturedSheetIndex] = await UniTask.RunOnThreadPool(
                    () => BMPLoader.LoadBmpData(wordsPath));
                if (bmpData[capturedSheetIndex] == null || bmpData[capturedSheetIndex].Pixels == null ||
                    bmpData[capturedSheetIndex].Width != BattleCommonVisualCatalog.WordTextureWidth ||
                    bmpData[capturedSheetIndex].Height != BattleCommonVisualCatalog.WordTextureHeight ||
                    !CanCompleteSpritePrewarmInvocation(invocation))
                {
                    return null;
                }
            }

            var transparency = new TransparentColorData
            {
                targetColor = Color.black,
                colorTolerance = 0f
            };
            var processedPixels = new Color32[BattleCommonVisualCatalog.WordSheetCount][];
            for (int sheetIndex = 0; sheetIndex < processedPixels.Length; sheetIndex++)
            {
                int capturedSheetIndex = sheetIndex;
                processedPixels[capturedSheetIndex] = await UniTask.RunOnThreadPool(() =>
                {
                    Color[] colors = RuntimeSpriteProcessor.ProcessColorTransparencyPixels(
                        bmpData[capturedSheetIndex].Pixels,
                        transparency,
                        out _);
                    return ConvertToColor32(colors);
                });
                if (processedPixels[capturedSheetIndex] == null ||
                    processedPixels[capturedSheetIndex].Length !=
                    bmpData[capturedSheetIndex].Width * bmpData[capturedSheetIndex].Height ||
                    !CanCompleteSpritePrewarmInvocation(invocation))
                {
                    return null;
                }
            }

            await UniTask.SwitchToMainThread();
            if (!CanCompleteSpritePrewarmInvocation(invocation))
                return null;

            Texture2D[] textures = new Texture2D[BattleCommonVisualCatalog.WordSheetCount];
            Sprite[][] glyphSprites = new Sprite[BattleCommonVisualCatalog.WordSheetCount][];
            bool transfersOwnership = false;
            try
            {
                for (int sheetIndex = 0; sheetIndex < textures.Length; sheetIndex++)
                {
                    if (!CanCompleteSpritePrewarmInvocation(invocation))
                        return null;

                    Texture2D texture = new Texture2D(
                        bmpData[sheetIndex].Width,
                        bmpData[sheetIndex].Height,
                        TextureFormat.RGBA32,
                        false);
                    texture.filterMode = FilterMode.Point;
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.SetPixels32(processedPixels[sheetIndex]);
                    texture.Apply(false, true);
                    texture.name = $"WORDS{sheetIndex}";
                    textures[sheetIndex] = texture;

                    var glyphs = new Sprite[BattleCommonVisualCatalog.WordGlyphsPerSheet];
                    for (int charCode = 0; charCode < glyphs.Length; charCode++)
                    {
                        if (!CanCompleteSpritePrewarmInvocation(invocation))
                            return null;

                        Sprite glyph = Sprite.Create(
                            texture,
                            BattleCommonVisualCatalog.GetWordGlyphPixelRect(charCode),
                            BattleCommonVisualCatalog.GetWordGlyphPivotNormalized(),
                            100f,
                            0,
                            SpriteMeshType.FullRect);
                        glyph.name = $"words_{sheetIndex:D1}_{charCode:D3}";
                        glyphs[charCode] = glyph;
                    }

                    glyphSprites[sheetIndex] = glyphs;
                }

                if (!CanCompleteSpritePrewarmInvocation(invocation))
                    return null;

                transfersOwnership = true;
                return new WordsPublicationStaging
                {
                    Textures = textures,
                    GlyphSprites = glyphSprites,
                    SourcePaths = sourcePaths,
                    ProcessedPixels = processedPixels,
                };
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to create the common WORDS publication: {exception.Message}");
                return null;
            }
            finally
            {
                if (!transfersOwnership)
                    DestroyWordsPublicationStaging(textures, glyphSprites);
            }
        }

        private static void DestroyWordsPublicationStaging(Texture2D[] textures, Sprite[][] glyphSprites)
        {
            if (glyphSprites != null)
            {
                foreach (Sprite[] glyphs in glyphSprites)
                {
                    if (glyphs == null)
                        continue;
                    foreach (Sprite glyph in glyphs)
                    {
                        if (glyph == null)
                            continue;
                        if (Application.isPlaying)
                            UnityEngine.Object.Destroy(glyph);
                        else
                            UnityEngine.Object.DestroyImmediate(glyph);
                    }
                }
            }

            if (textures == null)
                return;
            foreach (Texture2D texture in textures)
            {
                if (texture == null)
                    continue;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(texture);
                else
                    UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static bool TryAppendCommonAtlasSources(
            BattleCommonVisualCatalog commonCatalog,
            SparkPublicationStaging spark,
            WordsPublicationStaging words,
            ICollection<BattleAtlasSourcePixels> sources,
            IDictionary<BattleVisualResourceKey, string> sourcePaths,
            ICollection<string> forcedSourceTexture2DPaths,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (commonCatalog == null || !commonCatalog.IsComplete ||
                spark?.Texture == null || spark.ProcessedPixels == null ||
                string.IsNullOrWhiteSpace(spark.SourcePath) ||
                words?.Textures == null || words.GlyphSprites == null ||
                words.ProcessedPixels == null || words.SourcePaths == null ||
                sources == null || sourcePaths == null || forcedSourceTexture2DPaths == null)
            {
                diagnostic = "Complete common staging data is required for unified atlas publication.";
                return false;
            }

            if (spark.ProcessedPixels.Length != spark.Texture.width * spark.Texture.height)
            {
                diagnostic = "SPARK processed pixels do not match the published descriptor texture.";
                return false;
            }

            sources.Add(new BattleAtlasSourcePixels(
                spark.SourcePath,
                spark.Texture.width,
                spark.Texture.height,
                spark.ProcessedPixels));
            for (int pic = 0; pic < BattleCommonVisualCatalog.SparkFrameCount; pic++)
                sourcePaths[BattleVisualResourceKey.CommonSpark(pic)] = spark.SourcePath;

            if (words.Textures.Length != BattleCommonVisualCatalog.WordSheetCount ||
                words.ProcessedPixels.Length != BattleCommonVisualCatalog.WordSheetCount ||
                words.SourcePaths.Length != BattleCommonVisualCatalog.WordSheetCount)
            {
                diagnostic = "WORDS staging arrays do not contain all six source sheets.";
                return false;
            }

            for (int sheetIndex = 0;
                 sheetIndex < BattleCommonVisualCatalog.WordSheetCount;
                 sheetIndex++)
            {
                Texture2D texture = words.Textures[sheetIndex];
                Color32[] pixels = words.ProcessedPixels[sheetIndex];
                string path = words.SourcePaths[sheetIndex];
                if (texture == null || pixels == null || string.IsNullOrWhiteSpace(path) ||
                    pixels.Length != texture.width * texture.height)
                {
                    diagnostic = $"WORDS{sheetIndex} processed pixels do not match the published descriptor texture.";
                    return false;
                }

                sources.Add(new BattleAtlasSourcePixels(path, texture.width, texture.height, pixels));
                for (int charCode = 0;
                     charCode < BattleCommonVisualCatalog.WordGlyphsPerSheet;
                     charCode++)
                {
                    sourcePaths[BattleVisualResourceKey.CommonWordGlyph(sheetIndex, charCode)] = path;
                }
            }

            BattleCommonVisualBinding shadow = commonCatalog.Shadow;
            Texture2D shadowTexture = shadow?.Texture;
            string shadowPath = ResolveCommonShadowAtlasSourcePath(shadowTexture);
            sourcePaths[BattleVisualResourceKey.CommonShadow] = shadowPath;
            try
            {
                Color32[] shadowPixels = shadowTexture != null
                    ? shadowTexture.GetPixels32()
                    : null;
                if (shadowPixels == null ||
                    shadowPixels.Length != shadowTexture.width * shadowTexture.height)
                {
                    throw new InvalidOperationException("pixel count does not match the descriptor texture");
                }

                sources.Add(new BattleAtlasSourcePixels(
                    shadowPath,
                    shadowTexture.width,
                    shadowTexture.height,
                    shadowPixels));
            }
            catch (Exception exception)
            {
                if (shadow?.CentralBinding.Mode != BattleSpriteCentralBindingMode.SourceTexture2D ||
                    !shadow.CentralBinding.IsValid)
                {
                    diagnostic =
                        $"Common shadow cannot be decoded for the atlas and has no valid SourceTexture2D fallback: {exception.Message}";
                    return false;
                }

                forcedSourceTexture2DPaths.Add(shadowPath);
                diagnostic =
                    $"commonShadowSource2DRetained=nonReadable; source='{shadowPath}'; reason='{exception.Message}'.";
            }

            return true;
        }

        private static string ResolveCommonShadowAtlasSourcePath(Texture2D texture)
        {
#if UNITY_EDITOR
            if (texture != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(texture);
                if (!string.IsNullOrWhiteSpace(assetPath))
                {
                    string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ??
                                         Application.dataPath;
                    return Path.GetFullPath(Path.Combine(projectRoot, assetPath));
                }
            }
#endif
            string name = texture != null && !string.IsNullOrWhiteSpace(texture.name)
                ? texture.name
                : "missing";
            foreach (char invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');
            return Path.Combine(
                Application.dataPath,
                "NTSD",
                ".runtime-atlas",
                $"common-shadow-{name}-{(texture != null ? texture.GetInstanceID() : 0)}.rgba32");
        }

        /// <summary>
        /// 异步加载所有角色的UI精灵（head和small）
        /// 在后台线程读取BMP像素数据，在主线程创建Sprite
        /// </summary>
        private async UniTask LoadAllCharacterUISpritesAsync(int invocation)
        {
            if (!CanCompleteSpritePrewarmInvocation(invocation))
                return;

            if (CharacterUIResourceManager.Instance == null)
            {
                Debug.LogWarning("<color=yellow>CharacterUIResourceManager.Instance 为空，跳过UI精灵加载</color>");
                return;
            }

            int loadedCount = 0;

            foreach (var config in TotalCharacterFrameConfig.Values)
            {
                if (!CanCompleteSpritePrewarmInvocation(invocation))
                    return;

                int characterId = config.characterId;
                var characterData = config.characterData;

                Sprite headSprite = null;
                Sprite smallSprite = null;

                // 异步加载head精灵
                if (!string.IsNullOrEmpty(characterData.head))
                {
                    string headPath = ResolveSpritePath(characterData.head, GetDatFileDirectory(characterId));
                    headSprite = await LoadBMPAsSpriteAsync(headPath, $"{characterData.name}_head");
                    if (!CanCompleteSpritePrewarmInvocation(invocation))
                        return;
                }

                // 异步加载small精灵
                if (!string.IsNullOrEmpty(characterData.small))
                {
                    string smallPath = ResolveSpritePath(characterData.small, GetDatFileDirectory(characterId));
                    smallSprite = await LoadBMPAsSpriteAsync(smallPath, $"{characterData.name}_small");
                    if (!CanCompleteSpritePrewarmInvocation(invocation))
                        return;
                }

                // 存入CharacterUIResourceManager
                if (headSprite != null || smallSprite != null)
                {
                    CharacterUIResourceManager.Instance.SetCharacterUISprites(characterId, headSprite, smallSprite);
                    loadedCount++;
                }
            }

            Debug.Log($"<color=cyan>UI精灵加载完成，共加载 {loadedCount} 个角色的头像</color>");
        }

        /// <summary>
        /// 获取角色dat文件所在目录
        /// 用于解析head/small的相对路径
        /// </summary>
        private string GetDatFileDirectory(int characterId)
        {
            // 默认使用配置目录，因为head/small路径通常是Assets/开头的绝对路径
            return TotalCharacterFrameConfigPath;
        }

        /// <summary>
        /// 异步加载BMP文件为Sprite
        /// 在后台线程读取像素数据，在主线程创建Texture2D和Sprite
        /// </summary>
        /// <param name="filePath">BMP文件路径</param>
        /// <param name="spriteName">精灵名称</param>
        /// <returns>加载的Sprite，失败返回null</returns>
        private async UniTask<Sprite> LoadBMPAsSpriteAsync(string filePath, string spriteName)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"<color=yellow>UI精灵文件不存在: {filePath}</color>");
                return null;
            }

            try
            {
                // 在后台线程读取BMP像素数据
                var bmpData = await UniTask.RunOnThreadPool(() => BMPLoader.LoadBmpData(filePath));
                if (bmpData == null || bmpData.Pixels == null)
                {
                    Debug.LogWarning($"<color=yellow>加载BMP数据失败: {filePath}</color>");
                    return null;
                }

                // 切换到主线程创建Texture2D和Sprite
                await UniTask.SwitchToMainThread();

                Texture2D texture = new Texture2D(bmpData.Width, bmpData.Height, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels(bmpData.Pixels);
                texture.Apply();

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
                sprite.name = spriteName;

                return sprite;
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>异步加载UI精灵异常: {filePath}\n{e.Message}</color>");
                return null;
            }
        }

        private async UniTask<int> ProcessAndCreateSpritesAsync(
            int characterId,
            SpriteFileInfo fileInfo,
            Dictionary<int, List<Sprite>> stagedSprites,
            HashSet<Sprite> stagedCreatedSprites,
            HashSet<Texture2D> stagedTextures,
            List<BattleAtlasSourcePixels> stagedAtlasSources,
            Action<string> onProgressText,
            System.Threading.SemaphoreSlim cpuSemaphore,
            System.Threading.SemaphoreSlim uploadSemaphore)
        {
            int created = 0;
            bool cpuSemaphoreHeld = true;
            bool uploadSemaphoreHeld = false;
            try
            {
                string filePath = fileInfo.filePath;
                onProgressText?.Invoke(FormatLoadingResourcePath(filePath));

                var bmpData = await UniTask.RunOnThreadPool(() => BMPLoader.LoadBmpData(filePath));
                if (bmpData == null || bmpData.Pixels == null)
                {
                    return -1;
                }

                int textureWidth = bmpData.Width;
                int textureHeight = bmpData.Height;
                Color[] loadedPixels = bmpData.Pixels;
                var sourcePixels = new Color32[loadedPixels.Length];
                for (int i = 0; i < loadedPixels.Length; i++)
                {
                    sourcePixels[i] = loadedPixels[i];
                }

                ResolveEffectiveGrid(
                    fileInfo,
                    textureWidth,
                    textureHeight,
                    out int actualRow,
                    out int actualCol);

                int spriteWidth = fileInfo.width;
                int spriteHeight = fileInfo.height;
                int row = actualRow;
                int col = actualCol;

                var processedSheet = await UniTask.RunOnThreadPool(() =>
                    RuntimeSpriteProcessor.ProcessSheetPixelsFast(sourcePixels));
                Rect?[] spriteRects = BuildIndexedSpriteRects(
                    fileInfo,
                    textureWidth,
                    textureHeight,
                    row,
                    col);

                if (processedSheet == null || processedSheet.Length == 0 ||
                    spriteRects == null || !spriteRects.Any(rect => rect.HasValue))
                {
                    return -1;
                }

                cpuSemaphore.Release();
                cpuSemaphoreHeld = false;

                await uploadSemaphore.WaitAsync();
                uploadSemaphoreHeld = true;
                await UniTask.SwitchToMainThread();

                var allSprites = stagedSprites[characterId];
                stagedAtlasSources.Add(new BattleAtlasSourcePixels(
                    filePath,
                    textureWidth,
                    textureHeight,
                    processedSheet));
                var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels32(processedSheet);
                texture.Apply(false, true);
                texture.name = System.IO.Path.GetFileNameWithoutExtension(filePath);
                stagedTextures.Add(texture);

                for (int i = 0; i < spriteRects.Length; i++)
                {
                    Rect? spriteRect = spriteRects[i];
                    if (!spriteRect.HasValue)
                        continue;

                    int targetIndex = fileInfo.startFrame + i;
                    if (targetIndex < 0 || targetIndex >= allSprites.Count || targetIndex > fileInfo.endFrame)
                        continue;

                    Sprite sprite = Sprite.Create(
                        texture,
                        spriteRect.Value,
                        new Vector2(0.5f, 0f),
                        100f,
                        0,
                        SpriteMeshType.FullRect
                    );
                    sprite.name = $"sprite_{i / col}_{i % col}";
                    stagedCreatedSprites.Add(sprite);
                    allSprites[targetIndex] = sprite;
                    created++;
                }
                await UniTask.Yield();
                uploadSemaphore.Release();
                uploadSemaphoreHeld = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>处理文件失败: {fileInfo.filePath}\n{e.Message}</color>");
                created = -1;
            }
            finally
            {
                if (cpuSemaphoreHeld)
                    cpuSemaphore?.Release();
                if (uploadSemaphoreHeld)
                    uploadSemaphore?.Release();
            }
            return created;
        }

        internal static BattleSpriteCatalog BuildBattleSpriteCatalog(
            Dictionary<int, LF2CharacterDataWrapper> configs,
            Dictionary<int, List<Sprite>> spritesByVisualDataId)
        {
            var builder = new BattleSpriteCatalogBuilder();

            foreach (var config in configs.Values)
            {
                int visualDataId = config.characterId;
                if (!spritesByVisualDataId.TryGetValue(visualDataId, out List<Sprite> sprites) || sprites == null)
                    continue;

                foreach (SpriteFileInfo fileInfo in config.characterData.files)
                {
                    int firstPic = Mathf.Max(0, fileInfo.startFrame);
                    int lastPic = Mathf.Min(fileInfo.endFrame, sprites.Count - 1);
                    Sprite firstSprite = null;
                    for (int pic = firstPic; pic <= lastPic && firstSprite == null; pic++)
                        firstSprite = sprites[pic];

                    Texture2D texture = firstSprite != null ? firstSprite.texture : null;
                    if (texture == null)
                        continue;

                    ResolveEffectiveGrid(fileInfo, texture.width, texture.height, out int row, out int col);
                    Rect?[] rects = BuildIndexedSpriteRects(
                        fileInfo,
                        texture.width,
                        texture.height,
                        row,
                        col);

                    int firstLocalPic = Mathf.Max(0, firstPic - fileInfo.startFrame);
                    int lastLocalPic = Mathf.Min(
                        rects.Length - 1,
                        lastPic - fileInfo.startFrame);
                    for (int localPic = firstLocalPic; localPic <= lastLocalPic; localPic++)
                    {
                        int effectivePic = fileInfo.startFrame + localPic;
                        if (effectivePic < 0 || effectivePic >= sprites.Count)
                            continue;

                        Sprite legacySprite = sprites[effectivePic];
                        if (legacySprite == null || !rects[localPic].HasValue)
                            continue;

                        builder.Add(
                            visualDataId,
                            effectivePic,
                            fileInfo.filePath,
                            texture,
                            rects[localPic].Value,
                            legacySprite);
                    }
                }
            }

            return builder.Publish();
        }

        internal static bool TryBuildCentralAtlasPublication(
            BattleSpriteCatalog sourceCatalog,
            IReadOnlyList<BattleAtlasSourcePixels> sources,
            BattleRenderingDeviceCapabilities capabilities,
            NTSD.App.GameConfig config,
            string[] commandLineArguments,
            out BattleSpriteCatalog boundCatalog,
            out HashSet<UnityEngine.Object> ownedResources,
            out string diagnostic,
            out BattleAtlasPolicyDecision policyDecision,
            out BattleAtlasDiagnosticInputs diagnosticInputs)
        {
            boundCatalog = sourceCatalog ?? BattleSpriteCatalog.Empty;
            ownedResources = new HashSet<UnityEngine.Object>();
            diagnostic = string.Empty;
            policyDecision = null;
            diagnosticInputs = null;
            if (boundCatalog.Count == 0)
                return true;
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            if (!TryClassifyCentralAtlasSources(
                    sources,
                    capabilities.MaxTextureSize,
                    out List<BattleAtlasSourcePixels> eligibleSources,
                    out List<string> sourceTexture2DExcludedPaths,
                    out string oversizedDiagnostic))
            {
                diagnostic = oversizedDiagnostic;
                return false;
            }

            var descriptors = CreateAtlasDescriptors(eligibleSources);

            BattleAtlasPlanResult planResult = BattleAtlasLayoutPlanner.Plan(descriptors);
            if (!planResult.Succeeded)
            {
                diagnostic = planResult.Diagnostic;
                return false;
            }

            policyDecision = BattleRenderingPolicyResolver.ResolveAtlas(
                capabilities,
                planResult.Plan.PageCount,
                config,
                commandLineArguments);
            if (planResult.Plan.PageCount == 0)
            {
                if (!TryRetainExcludedSourceTexture2DCatalog(
                        boundCatalog,
                        sourceTexture2DExcludedPaths,
                        out string sourceTexture2DDiagnostic))
                {
                    diagnostic = sourceTexture2DDiagnostic;
                    return false;
                }

                diagnostic = oversizedDiagnostic;
                diagnosticInputs = new BattleAtlasDiagnosticInputs(
                    capabilities,
                    policyDecision,
                    0,
                    0,
                    BattleSpriteCentralBindingMode.SourceTexture2D,
                    diagnostic);
                return true;
            }
            if (!BattleAtlasResourceBuilder.TryBuild(
                    planResult.Plan,
                    eligibleSources,
                    policyDecision.CapabilityPolicy,
                    out BattleAtlasResources resources,
                    out diagnostic))
            {
                return false;
            }

            foreach (UnityEngine.Object resource in resources.OwnedObjects)
                ownedResources.Add(resource);
            if (resources.Mode == BattleSpriteCentralBindingMode.AtlasPageTexture2D &&
                policyDecision.EffectiveMode == BattleAtlasPolicyMode.TextureArray)
            {
                string runtimeFallbackReason = string.IsNullOrEmpty(resources.Diagnostic)
                    ? "Texture2DArray allocation/upload failed; ordered pages were published."
                    : resources.Diagnostic;
                policyDecision = new BattleAtlasPolicyDecision(
                    policyDecision.RequestedMode,
                    BattleAtlasPolicyMode.OrderedPages,
                    runtimeFallbackReason,
                    capabilities.ToAtlasCapabilityPolicy(runtimeFallbackReason));
            }
            if (BattleAtlasResourceBuilder.TryBindCatalog(
                    boundCatalog,
                    planResult.Plan,
                    resources,
                    sourceTexture2DExcludedPaths,
                    out BattleSpriteCatalog remapped,
                    out string bindingDiagnostic))
            {
                boundCatalog = remapped;
                diagnostic = CombineAtlasDiagnostics(resources.Diagnostic, oversizedDiagnostic);
                diagnosticInputs = new BattleAtlasDiagnosticInputs(
                    capabilities,
                    policyDecision,
                    planResult.Plan.PageCount,
                    BattleAtlasDiagnosticInputs.EstimateAtlasBytes(planResult.Plan.PageCount),
                    resources.Mode,
                    diagnostic);
                return true;
            }

            DestroyStagedPresentation(null, ownedResources);
            ownedResources.Clear();
            diagnostic = bindingDiagnostic;
            return false;
        }

        internal static bool TryBuildUnifiedCentralAtlasPublication(
            BattleSpriteCatalog sourceCatalog,
            BattleCommonVisualCatalog sourceCommonCatalog,
            IReadOnlyList<BattleAtlasSourcePixels> sources,
            IReadOnlyDictionary<BattleVisualResourceKey, string> commonSourcePaths,
            IReadOnlyCollection<string> forcedCommonSourceTexture2DPaths,
            BattleRenderingDeviceCapabilities capabilities,
            NTSD.App.GameConfig config,
            string[] commandLineArguments,
            out BattleSpriteCatalog boundCatalog,
            out BattleCommonVisualCatalog boundCommonCatalog,
            out HashSet<UnityEngine.Object> ownedResources,
            out string diagnostic,
            out BattleAtlasPolicyDecision policyDecision,
            out BattleAtlasDiagnosticInputs diagnosticInputs)
        {
            boundCatalog = sourceCatalog ?? BattleSpriteCatalog.Empty;
            boundCommonCatalog = sourceCommonCatalog ?? BattleCommonVisualCatalog.Empty;
            ownedResources = new HashSet<UnityEngine.Object>();
            diagnostic = string.Empty;
            policyDecision = null;
            diagnosticInputs = null;
            if (!boundCommonCatalog.IsComplete)
            {
                diagnostic = "Unified atlas publication requires a complete common visual catalog.";
                return false;
            }
            if (commonSourcePaths == null)
            {
                diagnostic = "Unified atlas publication requires common visual source paths.";
                return false;
            }
            if (capabilities == null)
                throw new ArgumentNullException(nameof(capabilities));

            if (!TryClassifyCentralAtlasSources(
                    sources,
                    capabilities.MaxTextureSize,
                    out List<BattleAtlasSourcePixels> eligibleSources,
                    out List<string> sourceTexture2DExcludedPaths,
                    out string oversizedDiagnostic))
            {
                diagnostic = oversizedDiagnostic;
                return false;
            }

            if (forcedCommonSourceTexture2DPaths != null)
            {
                var exclusions = new HashSet<string>(
                    sourceTexture2DExcludedPaths,
                    StringComparer.Ordinal);
                foreach (string path in forcedCommonSourceTexture2DPaths)
                {
                    string normalizedPath = BattleAtlasLayoutPlanner.NormalizePath(path);
                    if (!string.IsNullOrEmpty(normalizedPath))
                        exclusions.Add(normalizedPath);
                }
                sourceTexture2DExcludedPaths = exclusions.ToList();
                sourceTexture2DExcludedPaths.Sort(StringComparer.Ordinal);
            }

            BattleAtlasPlanResult planResult =
                BattleAtlasLayoutPlanner.Plan(CreateAtlasDescriptors(eligibleSources));
            if (!planResult.Succeeded)
            {
                diagnostic = planResult.Diagnostic;
                return false;
            }

            policyDecision = BattleRenderingPolicyResolver.ResolveAtlas(
                capabilities,
                planResult.Plan.PageCount,
                config,
                commandLineArguments);
            if (planResult.Plan.PageCount == 0)
            {
                if (!TryRetainExcludedSourceTexture2DCatalog(
                        boundCatalog,
                        sourceTexture2DExcludedPaths,
                        out diagnostic) ||
                    !TryRetainExcludedSourceTexture2DCommonCatalog(
                        boundCommonCatalog,
                        commonSourcePaths,
                        sourceTexture2DExcludedPaths,
                        out diagnostic))
                {
                    return false;
                }

                diagnostic = oversizedDiagnostic;
                diagnosticInputs = new BattleAtlasDiagnosticInputs(
                    capabilities,
                    policyDecision,
                    0,
                    0,
                    BattleSpriteCentralBindingMode.SourceTexture2D,
                    diagnostic);
                return true;
            }

            if (!BattleAtlasResourceBuilder.TryBuild(
                    planResult.Plan,
                    eligibleSources,
                    policyDecision.CapabilityPolicy,
                    out BattleAtlasResources resources,
                    out diagnostic))
            {
                return false;
            }

            foreach (UnityEngine.Object resource in resources.OwnedObjects)
                ownedResources.Add(resource);
            if (resources.Mode == BattleSpriteCentralBindingMode.AtlasPageTexture2D &&
                policyDecision.EffectiveMode == BattleAtlasPolicyMode.TextureArray)
            {
                string runtimeFallbackReason = string.IsNullOrEmpty(resources.Diagnostic)
                    ? "Texture2DArray allocation/upload failed; ordered pages were published."
                    : resources.Diagnostic;
                policyDecision = new BattleAtlasPolicyDecision(
                    policyDecision.RequestedMode,
                    BattleAtlasPolicyMode.OrderedPages,
                    runtimeFallbackReason,
                    capabilities.ToAtlasCapabilityPolicy(runtimeFallbackReason));
            }

            if (!BattleAtlasResourceBuilder.TryBindCatalog(
                    boundCatalog,
                    planResult.Plan,
                    resources,
                    sourceTexture2DExcludedPaths,
                    out BattleSpriteCatalog remappedCatalog,
                    out string entityBindingDiagnostic))
            {
                DestroyStagedPresentation(null, ownedResources);
                ownedResources.Clear();
                diagnostic = entityBindingDiagnostic;
                return false;
            }
            if (!BattleAtlasResourceBuilder.TryBindCommonCatalog(
                    boundCommonCatalog,
                    planResult.Plan,
                    resources,
                    commonSourcePaths,
                    sourceTexture2DExcludedPaths,
                    out BattleCommonVisualCatalog remappedCommonCatalog,
                    out string commonBindingDiagnostic))
            {
                DestroyStagedPresentation(null, ownedResources);
                ownedResources.Clear();
                diagnostic = commonBindingDiagnostic;
                return false;
            }

            boundCatalog = remappedCatalog;
            boundCommonCatalog = remappedCommonCatalog;
            diagnostic = CombineAtlasDiagnostics(resources.Diagnostic, oversizedDiagnostic);
            diagnosticInputs = new BattleAtlasDiagnosticInputs(
                capabilities,
                policyDecision,
                planResult.Plan.PageCount,
                BattleAtlasDiagnosticInputs.EstimateAtlasBytes(planResult.Plan.PageCount),
                resources.Mode,
                diagnostic);
            return true;
        }

        internal static bool TryBuildCentralAtlasPublication(
            BattleSpriteCatalog sourceCatalog,
            IReadOnlyList<BattleAtlasSourcePixels> sources,
            BattleAtlasCapabilityPolicy policy,
            out BattleSpriteCatalog boundCatalog,
            out HashSet<UnityEngine.Object> ownedResources,
            out string diagnostic)
        {
            boundCatalog = sourceCatalog ?? BattleSpriteCatalog.Empty;
            ownedResources = new HashSet<UnityEngine.Object>();
            diagnostic = string.Empty;
            if (boundCatalog.Count == 0)
                return true;

            if (!TryClassifyCentralAtlasSources(
                    sources,
                    policy.MaxTextureSize,
                    out List<BattleAtlasSourcePixels> eligibleSources,
                    out List<string> sourceTexture2DExcludedPaths,
                    out string oversizedDiagnostic))
            {
                diagnostic = oversizedDiagnostic;
                return false;
            }

            var descriptors = CreateAtlasDescriptors(eligibleSources);

            BattleAtlasPlanResult planResult = BattleAtlasLayoutPlanner.Plan(descriptors);
            if (!planResult.Succeeded)
            {
                diagnostic = planResult.Diagnostic;
                return false;
            }
            if (planResult.Plan.PageCount == 0)
            {
                if (!TryRetainExcludedSourceTexture2DCatalog(
                        boundCatalog,
                        sourceTexture2DExcludedPaths,
                        out string sourceTexture2DDiagnostic))
                {
                    diagnostic = sourceTexture2DDiagnostic;
                    return false;
                }

                diagnostic = oversizedDiagnostic;
                return true;
            }
            if (!BattleAtlasResourceBuilder.TryBuild(
                    planResult.Plan,
                    eligibleSources,
                    policy,
                    out BattleAtlasResources resources,
                    out diagnostic))
            {
                return false;
            }

            foreach (UnityEngine.Object resource in resources.OwnedObjects)
                ownedResources.Add(resource);
            if (BattleAtlasResourceBuilder.TryBindCatalog(
                    boundCatalog,
                    planResult.Plan,
                    resources,
                    sourceTexture2DExcludedPaths,
                    out BattleSpriteCatalog remapped,
                    out string bindingDiagnostic))
            {
                boundCatalog = remapped;
                diagnostic = CombineAtlasDiagnostics(resources.Diagnostic, oversizedDiagnostic);
                return true;
            }

            DestroyStagedPresentation(null, ownedResources);
            ownedResources.Clear();
            diagnostic = bindingDiagnostic;
            return false;
        }

        private static List<BattleAtlasSheetDescriptor> CreateAtlasDescriptors(
            IReadOnlyList<BattleAtlasSourcePixels> sources)
        {
            var descriptors = new List<BattleAtlasSheetDescriptor>(sources?.Count ?? 0);
            if (sources == null)
                return descriptors;

            for (int index = 0; index < sources.Count; index++)
            {
                BattleAtlasSourcePixels source = sources[index];
                if (source != null)
                    descriptors.Add(new BattleAtlasSheetDescriptor(source.Path, source.Width, source.Height));
            }
            return descriptors;
        }

        private static bool TryClassifyCentralAtlasSources(
            IReadOnlyList<BattleAtlasSourcePixels> sources,
            int maxSourceTextureSize,
            out List<BattleAtlasSourcePixels> eligibleSources,
            out List<string> sourceTexture2DExcludedPaths,
            out string diagnostic)
        {
            eligibleSources = new List<BattleAtlasSourcePixels>(sources?.Count ?? 0);
            sourceTexture2DExcludedPaths = new List<string>();
            diagnostic = string.Empty;
            if (!BattleAtlasResourceBuilder.TryValidateSourceSet(sources, out diagnostic))
                return false;

            var oversizedByPath = new Dictionary<string, BattleAtlasSheetDescriptor>(StringComparer.Ordinal);
            var classifiedPaths = new HashSet<string>(StringComparer.Ordinal);
            if (sources != null)
            {
                for (int index = 0; index < sources.Count; index++)
                {
                    BattleAtlasSourcePixels source = sources[index];
                    if (source == null)
                        continue;

                    string path = BattleAtlasLayoutPlanner.NormalizePath(source.Path);
                    if (!classifiedPaths.Add(path))
                        continue;

                    if (BattleAtlasLayoutPlanner.IsPageEligible(source.Width, source.Height))
                    {
                        eligibleSources.Add(source);
                        continue;
                    }

                    oversizedByPath[path] = new BattleAtlasSheetDescriptor(path, source.Width, source.Height);
                }
            }

            if (oversizedByPath.Count == 0)
                return true;

            var unrenderableByPath = new Dictionary<string, BattleAtlasSheetDescriptor>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, BattleAtlasSheetDescriptor> pair in oversizedByPath)
            {
                BattleAtlasSheetDescriptor source = pair.Value;
                if (source.Width > maxSourceTextureSize || source.Height > maxSourceTextureSize)
                    unrenderableByPath.Add(pair.Key, source);
            }
            if (unrenderableByPath.Count > 0)
            {
                diagnostic = $"unrenderableOversized: MaxTextureSize={maxSourceTextureSize}; sources=[{FormatAtlasSourceList(unrenderableByPath)}].";
                return false;
            }

            foreach (string path in oversizedByPath.Keys)
                sourceTexture2DExcludedPaths.Add(path);
            sourceTexture2DExcludedPaths.Sort(StringComparer.Ordinal);
            diagnostic = $"oversizedSource2DRetainedCount={sourceTexture2DExcludedPaths.Count}; oversizedSources=[{FormatAtlasSourceList(oversizedByPath)}].";
            return true;
        }

        private static string FormatAtlasSourceList(
            IReadOnlyDictionary<string, BattleAtlasSheetDescriptor> sources)
        {
            var paths = new List<string>(sources.Keys);
            paths.Sort(StringComparer.Ordinal);
            var values = new List<string>(paths.Count);
            for (int index = 0; index < paths.Count; index++)
            {
                BattleAtlasSheetDescriptor source = sources[paths[index]];
                values.Add($"{source.Path} ({source.Width}x{source.Height})");
            }
            return string.Join(", ", values);
        }

        private static bool TryRetainExcludedSourceTexture2DCatalog(
            BattleSpriteCatalog catalog,
            IReadOnlyCollection<string> sourceTexture2DExcludedPaths,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            var excludedPaths = new HashSet<string>(sourceTexture2DExcludedPaths, StringComparer.Ordinal);
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in catalog.Entries)
            {
                BattleSpriteEntry entry = pair.Value;
                if (!excludedPaths.Contains(BattleAtlasLayoutPlanner.NormalizePath(entry.SourceSheetPath)))
                {
                    diagnostic = $"Catalog entry {pair.Key} references missing atlas source '{entry.SourceSheetPath}'.";
                    return false;
                }
                if (entry.CentralBinding.Mode != BattleSpriteCentralBindingMode.SourceTexture2D ||
                    !entry.CentralBinding.IsValid)
                {
                    diagnostic = $"Catalog entry {pair.Key} cannot retain an invalid SourceTexture2D binding.";
                    return false;
                }
            }
            return true;
        }

        private static bool TryRetainExcludedSourceTexture2DCommonCatalog(
            BattleCommonVisualCatalog catalog,
            IReadOnlyDictionary<BattleVisualResourceKey, string> sourcePaths,
            IReadOnlyCollection<string> sourceTexture2DExcludedPaths,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (catalog == null || !catalog.IsComplete)
            {
                diagnostic = "A complete common visual catalog is required for SourceTexture2D retention.";
                return false;
            }

            var excludedPaths = new HashSet<string>(
                sourceTexture2DExcludedPaths ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            if (!CanRetainCommonSourceTexture2D(
                    catalog.Shadow,
                    sourcePaths,
                    excludedPaths,
                    out diagnostic))
            {
                return false;
            }

            for (int pic = 0; pic < BattleCommonVisualCatalog.SparkFrameCount; pic++)
            {
                if (!catalog.TryGetSpark(pic, out BattleCommonVisualBinding spark) ||
                    !CanRetainCommonSourceTexture2D(
                        spark,
                        sourcePaths,
                        excludedPaths,
                        out diagnostic))
                {
                    return false;
                }
            }

            for (int sheetIndex = 0;
                 sheetIndex < BattleCommonVisualCatalog.WordSheetCount;
                 sheetIndex++)
            {
                for (int charCode = 0;
                     charCode < BattleCommonVisualCatalog.WordGlyphsPerSheet;
                     charCode++)
                {
                    if (!catalog.TryGetWordGlyph(
                            sheetIndex,
                            charCode,
                            out BattleCommonVisualBinding glyph) ||
                        !CanRetainCommonSourceTexture2D(
                            glyph,
                            sourcePaths,
                            excludedPaths,
                            out diagnostic))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool CanRetainCommonSourceTexture2D(
            BattleCommonVisualBinding binding,
            IReadOnlyDictionary<BattleVisualResourceKey, string> sourcePaths,
            ISet<string> excludedPaths,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (binding == null ||
                !sourcePaths.TryGetValue(binding.Key, out string sourcePath))
            {
                diagnostic =
                    $"Common visual {binding?.Key.ToString() ?? "<null>"} has no SourceTexture2D path.";
                return false;
            }
            if (!excludedPaths.Contains(BattleAtlasLayoutPlanner.NormalizePath(sourcePath)))
            {
                diagnostic =
                    $"Common visual {binding.Key} references missing atlas source '{sourcePath}'.";
                return false;
            }
            if (binding.CentralBinding.Mode != BattleSpriteCentralBindingMode.SourceTexture2D ||
                !binding.CentralBinding.IsValid)
            {
                diagnostic =
                    $"Common visual {binding.Key} cannot retain an invalid SourceTexture2D binding.";
                return false;
            }
            return true;
        }

        private static string CombineAtlasDiagnostics(string primary, string secondary)
        {
            if (string.IsNullOrEmpty(primary))
                return secondary ?? string.Empty;
            if (string.IsNullOrEmpty(secondary))
                return primary;
            return primary + " " + secondary;
        }

        internal static void ResolveEffectiveGrid(
            SpriteFileInfo fileInfo,
            int textureWidth,
            int textureHeight,
            out int row,
            out int col)
        {
            row = fileInfo.row;
            col = fileInfo.col;

            bool SizeMatches(int actual, int expected) => Mathf.Abs(actual - expected) <= 1;
            int expectedWidth = fileInfo.col * (fileInfo.width + 1);
            int expectedHeight = fileInfo.row * (fileInfo.height + 1);
            if (SizeMatches(textureWidth, expectedWidth) && SizeMatches(textureHeight, expectedHeight))
                return;

            int swappedExpectedWidth = fileInfo.row * (fileInfo.width + 1);
            int swappedExpectedHeight = fileInfo.col * (fileInfo.height + 1);
            if (SizeMatches(textureWidth, swappedExpectedWidth) &&
                SizeMatches(textureHeight, swappedExpectedHeight))
            {
                row = fileInfo.col;
                col = fileInfo.row;
                return;
            }

            // Production DAT contains intentionally partial sheets. When neither
            // full grid matches, retain the authored row/column interpretation;
            // BuildIndexedSpriteRects leaves each out-of-bounds localPic as a hole.
        }

        internal static Rect?[] BuildIndexedSpriteRects(
            SpriteFileInfo fileInfo,
            int textureWidth,
            int textureHeight,
            int row,
            int col)
        {
            if (fileInfo == null || row <= 0 || col <= 0 ||
                fileInfo.width <= 0 || fileInfo.height <= 0)
                return Array.Empty<Rect?>();

            var rects = new Rect?[checked(row * col)];
            int cellWidth = fileInfo.width + 1;
            int cellHeight = fileInfo.height + 1;
            for (int localPic = 0; localPic < rects.Length; localPic++)
            {
                int rowFromTop = localPic / col;
                int column = localPic % col;
                int x = column * cellWidth;
                int y = textureHeight - (rowFromTop + 1) * cellHeight + 1;
                if (x < 0 || y < 0 ||
                    x + fileInfo.width > textureWidth ||
                    y + fileInfo.height > textureHeight)
                    continue;

                rects[localPic] = new Rect(x, y, fileInfo.width, fileInfo.height);
            }

            return rects;
        }

        internal int BeginSpritePrewarmInvocation()
        {
            return ++spritePrewarmGeneration;
        }

        internal bool CanCompleteSpritePrewarmInvocation(int invocation)
        {
            return !spritePrewarmDisposed && invocation == spritePrewarmGeneration;
        }

        internal void MarkSpritePrewarmDestroyedForSelfCheck()
        {
            spritePrewarmDisposed = true;
            spritePrewarmGeneration++;
        }

        internal bool TryCommitSpritePrewarmInvocation(
            int invocation,
            Dictionary<int, LF2CharacterDataWrapper> configs,
            Dictionary<int, List<Sprite>> sprites,
            BattleSpriteCatalog catalog,
            HashSet<Sprite> ownedSprites = null,
            HashSet<Texture2D> ownedTextures = null,
            HashSet<UnityEngine.Object> ownedResources = null,
            string atlasDiagnostic = "",
            BattleCommonVisualCatalog commonVisualCatalog = null)
        {
            if (!CanCompleteSpritePrewarmInvocation(invocation))
                return false;

            BattleSpriteCatalog previousCatalog = SpriteCatalog;
            HashSet<UnityEngine.Object> resources = ownedResources;
            if (resources == null && ownedTextures != null)
            {
                resources = new HashSet<UnityEngine.Object>();
                foreach (Texture2D texture in ownedTextures)
                    resources.Add(texture);
            }
            bool transfersOwnership = ownedSprites != null || resources != null;
            if (transfersOwnership &&
                (publishedOwnedSprites.Count > 0 || publishedOwnedResources.Count > 0))
            {
                QueueRetiredSpritePublication(
                    previousCatalog,
                    publishedOwnedSprites,
                    publishedOwnedResources);
            }

            TotalCharacterFrameConfig = configs;
            MergedSprites = sprites;
            SpriteCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (commonVisualCatalog != null)
                CommonVisualCatalog = commonVisualCatalog;
            if (transfersOwnership)
            {
                publishedOwnedSprites = ownedSprites ?? new HashSet<Sprite>();
                publishedOwnedResources = resources ?? new HashSet<UnityEngine.Object>();
            }
            pendingCharacterFrameConfig = null;
            IsPrewarmCompleted = true;
            LastAtlasDiagnostic = atlasDiagnostic ?? string.Empty;
            TryRetireCatalogIfUnbound(previousCatalog);
            return true;
        }

        internal void RegisterRendererCatalogBinding(BattleSpriteCatalog catalog)
        {
            if (spritePrewarmDisposed || catalog == null || ReferenceEquals(catalog, BattleSpriteCatalog.Empty))
                return;

            spriteCatalogRendererBindings.TryGetValue(catalog, out int count);
            spriteCatalogRendererBindings[catalog] = count + 1;
        }

        internal void UnregisterRendererCatalogBinding(BattleSpriteCatalog catalog)
        {
            if (spritePrewarmDisposed || catalog == null ||
                !spriteCatalogRendererBindings.TryGetValue(catalog, out int count))
                return;

            if (count <= 1)
                spriteCatalogRendererBindings.Remove(catalog);
            else
                spriteCatalogRendererBindings[catalog] = count - 1;

            TryRetireCatalogIfUnbound(catalog);
        }

        internal int GetRendererCatalogBindingCount(BattleSpriteCatalog catalog)
        {
            return catalog != null && spriteCatalogRendererBindings.TryGetValue(catalog, out int count)
                ? count
                : 0;
        }

        public BattleSpriteCatalogLease AcquireCentralCatalogLease(BattleSpriteCatalog catalog)
        {
            BattleSpriteCatalog leasedCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (spritePrewarmDisposed || ReferenceEquals(leasedCatalog, BattleSpriteCatalog.Empty))
                return new BattleSpriteCatalogLease(leasedCatalog, null);

            RegisterRendererCatalogBinding(leasedCatalog);
            return new BattleSpriteCatalogLease(
                leasedCatalog,
                () => UnregisterRendererCatalogBinding(leasedCatalog));
        }

        internal void QueueRetiredSpritePublication(
            BattleSpriteCatalog catalog,
            HashSet<Sprite> sprites,
            HashSet<Texture2D> textures)
        {
            var resources = new HashSet<UnityEngine.Object>();
            if (textures != null)
            {
                foreach (Texture2D texture in textures)
                    resources.Add(texture);
            }
            QueueRetiredSpritePublication(catalog, sprites, resources);
        }

        internal void QueueRetiredSpritePublication(
            BattleSpriteCatalog catalog,
            HashSet<Sprite> sprites,
            HashSet<UnityEngine.Object> resources)
        {
            if (catalog == null)
                return;

            retiredSpritePublications[catalog] = new SpritePublicationOwnership(
                catalog,
                sprites,
                resources);
            TryRetireCatalogIfUnbound(catalog);
        }

        private int TryRetireCatalogIfUnbound(BattleSpriteCatalog catalog)
        {
            if (catalog == null || GetRendererCatalogBindingCount(catalog) > 0 ||
                !retiredSpritePublications.TryGetValue(catalog, out SpritePublicationOwnership ownership))
                return 0;

            retiredSpritePublications.Remove(catalog);
            return DestroyStagedPresentation(ownership.Sprites, ownership.Resources);
        }

        internal static int DestroyStagedPresentation(
            HashSet<Sprite> stagedCreatedSprites,
            HashSet<Texture2D> stagedTextures)
        {
            var resources = new HashSet<UnityEngine.Object>();
            if (stagedTextures != null)
            {
                foreach (Texture2D texture in stagedTextures)
                    resources.Add(texture);
            }
            return DestroyStagedPresentation(stagedCreatedSprites, resources);
        }

        internal static int DestroyStagedPresentation(
            HashSet<Sprite> stagedCreatedSprites,
            HashSet<UnityEngine.Object> stagedResources)
        {
            int destroyedCount = 0;
            if (stagedCreatedSprites != null)
            {
                foreach (Sprite sprite in stagedCreatedSprites)
                {
                    if (sprite == null)
                        continue;
                    destroyedCount++;
                    DestroyOwnedPresentationResource(sprite);
                }
            }

            if (stagedResources == null)
                return destroyedCount;
            foreach (UnityEngine.Object resource in stagedResources)
            {
                if (resource == null)
                    continue;

                destroyedCount++;
                DestroyOwnedPresentationResource(resource);
            }

            return destroyedCount;
        }

        private static void DestroyOwnedPresentationResource(UnityEngine.Object resource)
        {
            if (resource == null)
                return;

            // These objects are transient prewarm/atlas products, and this
            // method is reached only after the publication has no renderer
            // lease. Retirement must therefore release ownership synchronously
            // even when the editor is currently in Play Mode; deferred Destroy
            // would leave the superseded publication observable until the end
            // of the frame and can retain a large atlas allocation needlessly.
            UnityEngine.Object.DestroyImmediate(resource);
        }

        /// <summary>
        /// Retires superseded publications whose renderer reference count has
        /// reached zero. Bound catalogs remain queued until the final unbind.
        /// </summary>
        public int RetireSupersededSpritePublicationsAfterRefreshBoundary()
        {
            int retiredCount = 0;
            var catalogs = retiredSpritePublications.Keys.ToArray();
            for (int index = 0; index < catalogs.Length; index++)
            {
                retiredCount += TryRetireCatalogIfUnbound(catalogs[index]);
            }

            return retiredCount;
        }

        internal int PendingRetiredSpritePublicationCount => retiredSpritePublications.Count;

        private void OnDestroy()
        {
            // Clear central segments and release their catalog lease before the
            // manager's force-retirement boundary destroys publication resources.
            NTSD.Animation.Rendering.BattleCentralRenderSystem.ResetRuntime();
            MarkSpritePrewarmDestroyedForSelfCheck();
            if (publishedOwnedSprites.Count > 0 || publishedOwnedResources.Count > 0)
            {
                retiredSpritePublications[SpriteCatalog] = new SpritePublicationOwnership(
                    SpriteCatalog,
                    publishedOwnedSprites,
                    publishedOwnedResources);
                publishedOwnedSprites = new HashSet<Sprite>();
                publishedOwnedResources = new HashSet<UnityEngine.Object>();
            }

            // Manager teardown is a force-retirement boundary. No future commit
            // or renderer binding can succeed after the disposed flag is set.
            foreach (SpritePublicationOwnership ownership in retiredSpritePublications.Values)
                DestroyStagedPresentation(ownership.Sprites, ownership.Resources);
            retiredSpritePublications.Clear();
            spriteCatalogRendererBindings.Clear();
        }

        private void InvalidateSpriteCatalog()
        {
            BattleSpriteCatalog previousCatalog = SpriteCatalog;
            if ((publishedOwnedSprites.Count > 0 || publishedOwnedResources.Count > 0) &&
                previousCatalog != null && !ReferenceEquals(previousCatalog, BattleSpriteCatalog.Empty))
            {
                QueueRetiredSpritePublication(
                    previousCatalog,
                    publishedOwnedSprites,
                    publishedOwnedResources);
                publishedOwnedSprites = new HashSet<Sprite>();
                publishedOwnedResources = new HashSet<UnityEngine.Object>();
            }
            SpriteCatalog = BattleSpriteCatalog.Empty;
            CommonVisualCatalog = BattleCommonVisualCatalog.Empty;
            IsPrewarmCompleted = false;
        }

        /// <summary>
        /// 加载 BMP 文件为 Texture2D
        /// 使用增强的 BMP 加载器，支持详细诊断和手动解析
        /// </summary>
        public Texture2D LoadBMPTexture(string filePath)
        {
            return BMPLoader.LoadBMP(filePath);
        }

        private static string FormatLoadingResourcePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            var normalized = filePath.Replace("\\", "/");
            const string marker = "/Sprite/Character/";
            var index = normalized.IndexOf(marker, System.StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return normalized.Substring(index + marker.Length);
            }

            return System.IO.Path.GetFileName(normalized);
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
            return sprites;
        }

        /// <summary>静默查询，不打 LogError，供武器/SA 等可能未加载精灵的对象使用</summary>
        public int GetStartFrame(int id)
        {
            var files = GetCharacterData(id)?.files;
            return (files != null && files.Count > 0) ? files[0].startFrame : 0;
        }

        public bool TryGetSprites(int id, out List<Sprite> sprites)
        {
            return MergedSprites.TryGetValue(id, out sprites);
        }

        public bool TryGetSpriteEntry(int visualDataId, int effectivePic, out BattleSpriteEntry entry)
        {
            return SpriteCatalog.TryGet(visualDataId, effectivePic, out entry);
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
