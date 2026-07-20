using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.UI;
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
            if (configs == null || configs.Count == 0)
            {
                return;
            }

            TotalCharacterFrameConfig.Clear();
            foreach (var kvp in configs)
            {
                TotalCharacterFrameConfig[kvp.Key] = kvp.Value;
            }
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
            Debug.Log($"<color=cyan>开始加载精灵，角色配置数量: {TotalCharacterFrameConfig.Count}</color>");

            var allFileInfos = new List<(int characterId, SpriteFileInfo fileInfo)>();

            foreach (var config in TotalCharacterFrameConfig.Values)
            {
                int characterId = config.characterId;
                int totalSpriteCount = 0;
                if (config.characterData.files.Count > 0)
                {
                    var lastFile = config.characterData.files[config.characterData.files.Count - 1];
                    totalSpriteCount = lastFile.endFrame + 1;
                }
                MergedSprites[characterId] = new List<Sprite>(new Sprite[totalSpriteCount]);

                foreach (var fileInfo in config.characterData.files)
                {
                    allFileInfos.Add((characterId, fileInfo));
                }
            }

            Debug.Log($"<color=cyan>共 {allFileInfos.Count} 个文件需要处理</color>");

            int totalCreated = 0;
            int concurrentLimit = Mathf.Clamp(System.Environment.ProcessorCount, 1, 4);
            var cpuSemaphore = new System.Threading.SemaphoreSlim(concurrentLimit);
            var uploadSemaphore = new System.Threading.SemaphoreSlim(1);
            var pendingTasks = new List<UniTask>();

            foreach (var (characterId, fileInfo) in allFileInfos)
            {
                await cpuSemaphore.WaitAsync();
                var task = ProcessAndCreateSpritesAsync(characterId, fileInfo, onProgressText, cpuSemaphore, uploadSemaphore)
                    .ContinueWith(count => System.Threading.Interlocked.Add(ref totalCreated, count));
                pendingTasks.Add(task);
            }

            await UniTask.WhenAll(pendingTasks);

            Debug.Log($"<color=cyan>精灵加载完成，共创建 {totalCreated} 个精灵</color>");

            // 加载所有角色的UI精灵（head和small）
            await UniTask.SwitchToMainThread();
            await LoadAllCharacterUISpritesAsync();

            IsPrewarmCompleted = true;
            PrewarmCompleted?.Invoke();
        }

        /// <summary>
        /// 异步加载所有角色的UI精灵（head和small）
        /// 在后台线程读取BMP像素数据，在主线程创建Sprite
        /// </summary>
        private async UniTask LoadAllCharacterUISpritesAsync()
        {
            if (CharacterUIResourceManager.Instance == null)
            {
                Debug.LogWarning("<color=yellow>CharacterUIResourceManager.Instance 为空，跳过UI精灵加载</color>");
                return;
            }

            int loadedCount = 0;

            foreach (var config in TotalCharacterFrameConfig.Values)
            {
                int characterId = config.characterId;
                var characterData = config.characterData;

                Sprite headSprite = null;
                Sprite smallSprite = null;

                // 异步加载head精灵
                if (!string.IsNullOrEmpty(characterData.head))
                {
                    string headPath = ResolveSpritePath(characterData.head, GetDatFileDirectory(characterId));
                    headSprite = await LoadBMPAsSpriteAsync(headPath, $"{characterData.name}_head");
                }

                // 异步加载small精灵
                if (!string.IsNullOrEmpty(characterData.small))
                {
                    string smallPath = ResolveSpritePath(characterData.small, GetDatFileDirectory(characterId));
                    smallSprite = await LoadBMPAsSpriteAsync(smallPath, $"{characterData.name}_small");
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

        private async UniTask<int> ProcessAndCreateSpritesAsync(int characterId, SpriteFileInfo fileInfo,
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
                    return 0;
                }

                int textureWidth = bmpData.Width;
                int textureHeight = bmpData.Height;
                Color[] loadedPixels = bmpData.Pixels;
                var sourcePixels = new Color32[loadedPixels.Length];
                for (int i = 0; i < loadedPixels.Length; i++)
                {
                    sourcePixels[i] = loadedPixels[i];
                }

                int expectedWidth = fileInfo.col * (fileInfo.width + 1);
                int expectedHeight = fileInfo.row * (fileInfo.height + 1);
                int actualRow = fileInfo.row;
                int actualCol = fileInfo.col;

                bool SizeMatches(int actual, int expected) => Mathf.Abs(actual - expected) <= 1;

                if (!SizeMatches(textureWidth, expectedWidth) || !SizeMatches(textureHeight, expectedHeight))
                {
                    int swappedExpectedWidth = fileInfo.row * (fileInfo.width + 1);
                    int swappedExpectedHeight = fileInfo.col * (fileInfo.height + 1);

                    if (SizeMatches(textureWidth, swappedExpectedWidth) && SizeMatches(textureHeight, swappedExpectedHeight))
                    {
                        actualRow = fileInfo.col;
                        actualCol = fileInfo.row;
                    }
                }

                int spriteWidth = fileInfo.width;
                int spriteHeight = fileInfo.height;
                int row = actualRow;
                int col = actualCol;

                var processedSheet = await UniTask.RunOnThreadPool(() =>
                    RuntimeSpriteProcessor.ProcessSheetPixelsFast(sourcePixels));
                var spriteRects = RuntimeSpriteProcessor.BuildSpriteRectsFromTopLeft(
                    textureWidth, textureHeight, spriteWidth, spriteHeight, row, col);

                if (processedSheet == null || processedSheet.Length == 0 || spriteRects == null || spriteRects.Count == 0)
                {
                    return 0;
                }

                cpuSemaphore.Release();
                cpuSemaphoreHeld = false;

                await uploadSemaphore.WaitAsync();
                uploadSemaphoreHeld = true;
                await UniTask.SwitchToMainThread();

                var allSprites = MergedSprites[characterId];
                var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels32(processedSheet);
                texture.Apply(false, true);
                texture.name = System.IO.Path.GetFileNameWithoutExtension(filePath);

                for (int i = 0; i < spriteRects.Count; i++)
                {
                    var spriteRect = spriteRects[i];

                    Sprite sprite = Sprite.Create(
                        texture,
                        spriteRect.Rect,
                        new Vector2(0.5f, 0f),
                        100f,
                        0,
                        SpriteMeshType.FullRect
                    );
                    sprite.name = spriteRect.Name;

                    int targetIndex = fileInfo.startFrame + i;
                    if (targetIndex >= 0 && targetIndex < allSprites.Count && targetIndex <= fileInfo.endFrame)
                    {
                        allSprites[targetIndex] = sprite;
                        created++;
                    }
                }
                await UniTask.Yield();
                uploadSemaphore.Release();
                uploadSemaphoreHeld = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>处理文件失败: {fileInfo.filePath}\n{e.Message}</color>");
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
