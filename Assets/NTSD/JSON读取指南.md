# LF2 JSON 数据读取完整指南

## 📚 目录

1. [快速开始](#快速开始)
2. [读取方法](#读取方法)
3. [使用工具](#使用工具)
4. [数据访问](#数据访问)
5. [常见场景](#常见场景)
6. [最佳实践](#最佳实践)

---

## 🚀 快速开始

### 最简单的读取方式

```csharp
using NTSD.Animation;

// 从文件路径读取
string filePath = "Assets/ExportedDAT/character_1_data.json";
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromFile(filePath);

if (data != null)
{
    Debug.Log($"角色名称: {data.characterData.name}");
    Debug.Log($"总帧数: {data.characterData.frames.Count}");
}
```

---

## 📖 读取方法

### 1. 从文件路径读取

```csharp
// 完整路径
string filePath = "Assets/ExportedDAT/character_1_data.json";
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromFile(filePath);
```

**适用场景**：
- ✅ 编辑器内测试
- ✅ 已知文件路径
- ✅ 动态指定文件

### 2. 根据角色ID读取

```csharp
// 自动构建文件名: character_1_data.json
int characterId = 1;
string folder = "Assets/ExportedDAT";
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadByCharacterId(characterId, folder);
```

**适用场景**：
- ✅ 使用标准命名规则（character_{ID}_data.json）
- ✅ 需要根据ID动态加载
- ✅ 角色选择系统

### 3. 从Resources文件夹读取

```csharp
// 文件位于: Assets/Resources/CharacterData/character_1_data.json
string resourcePath = "CharacterData/character_1_data"; // 不需要.json扩展名
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromResources(resourcePath);
```

**适用场景**：
- ✅ 打包到游戏内
- ✅ 运行时动态加载
- ✅ 简单的资源管理

**注意**：JSON文件必须放在 `Assets/Resources/` 文件夹或其子文件夹内

### 4. 从StreamingAssets文件夹读取

```csharp
// 文件位于: Assets/StreamingAssets/character_1_data.json
string fileName = "character_1_data.json";
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromStreamingAssets(fileName);
```

**适用场景**：
- ✅ 游戏打包后
- ✅ 需要用户修改数据
- ✅ DLC或可下载内容
- ✅ 跨平台兼容

### 5. 从JSON字符串读取

```csharp
string jsonString = File.ReadAllText("path/to/file.json");
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromJsonString(jsonString);
```

**适用场景**：
- ✅ 从网络下载的JSON
- ✅ 从数据库读取的JSON
- ✅ 自定义来源的JSON

---

## 🛠️ 使用工具

### 方式1: 使用编辑器窗口（推荐新手）

1. **打开工具窗口**
   ```
   Unity菜单栏 → LF2 Tools → JSON读取器
   ```

2. **加载JSON文件**
   - 点击"浏览"按钮选择文件
   - 或点击"从Assets/ExportedDAT加载"快速选择

3. **查看数据**
   - 基本信息
   - 精灵图文件列表
   - 移动参数
   - 帧数据详情

**优点**：
- ✅ 可视化界面
- ✅ 无需写代码
- ✅ 方便测试和调试

### 方式2: 使用CharacterManager组件

```csharp
// 1. 在场景中添加组件
LF2CharacterManager manager = gameObject.AddComponent<LF2CharacterManager>();

// 2. 设置文件夹路径
manager.characterDataFolder = "Assets/ExportedDAT";

// 3. 加载角色
manager.LoadCharacter(1);

// 4. 使用数据
LF2CharacterData data = manager.CurrentCharacterData;
Vector2 walkSpeed = manager.GetWalkingSpeed();
LF2FrameData frame = manager.GetFrameData(0);
```

**优点**：
- ✅ 自动管理多个角色
- ✅ 内置缓存机制
- ✅ 提供便捷方法
- ✅ 适合游戏运行时使用

### 方式3: 直接使用Loader类

```csharp
// 最灵活的方式
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromFile(filePath);
```

**优点**：
- ✅ 最大灵活性
- ✅ 最少依赖
- ✅ 适合自定义需求

---

## 📊 数据访问

### 数据结构层次

```
LF2CharacterDataWrapper (根对象)
├── characterId (int)              角色ID
└── characterData (LF2CharacterData)
    ├── name (string)              角色名称
    ├── head (string)              头像文件路径
    ├── small (string)             小图文件路径
    ├── files (List<SpriteFileInfo>) 精灵图文件列表
    │   ├── filePath               文件路径
    │   ├── startFrame             起始帧
    │   ├── endFrame               结束帧
    │   ├── width, height          尺寸
    │   └── row, col               行列数
    ├── walking_speed              行走速度
    ├── running_speed              奔跑速度
    ├── jump_height                跳跃高度
    ├── jump_distance              跳跃距离
    └── frames (List<LF2FrameData>) 帧数据列表
        ├── frameId                帧ID
        ├── frameName              帧名称
        ├── pic                    图片索引
        ├── state                  状态
        ├── wait                   等待时间
        ├── next                   下一帧
        ├── bodies                 碰撞盒列表
        ├── itrs                   交互区域列表
        ├── wpoints                武器点列表
        ├── opoint                 对象点
        └── bpoint                 血点
```

### 访问示例

```csharp
LF2CharacterDataWrapper wrapper = LF2CharacterJsonLoader.LoadFromFile(filePath);

// 访问角色基本信息
string characterName = wrapper.characterData.name;
string headSprite = wrapper.characterData.head;

// 访问移动参数
float walkSpeed = wrapper.characterData.walking_speed;
float jumpHeight = wrapper.characterData.jump_height;

// 访问精灵图信息
foreach (var file in wrapper.characterData.files)
{
    Debug.Log($"{file.filePath}: 帧 {file.startFrame} - {file.endFrame}");
}

// 访问帧数据
LF2FrameData firstFrame = wrapper.characterData.frames[0];
Debug.Log($"第一帧: {firstFrame.frameName}");

// 访问碰撞盒
foreach (var body in firstFrame.bodies)
{
    Debug.Log($"碰撞盒: ({body.x}, {body.y}) 尺寸: {body.w}x{body.h}");
}
```

---

## 🎯 常见场景

### 场景1: 角色选择系统

```csharp
public class CharacterSelector : MonoBehaviour
{
    public int[] availableCharacterIds = { 1, 2, 3, 4, 5 };
    public string dataFolder = "Assets/ExportedDAT";

    public void SelectCharacter(int characterId)
    {
        var data = LF2CharacterJsonLoader.LoadByCharacterId(characterId, dataFolder);

        if (data != null)
        {
            // 显示角色信息
            Debug.Log($"选择角色: {data.characterData.name}");

            // 应用角色数据到游戏对象
            ApplyCharacterData(data.characterData);
        }
    }

    void ApplyCharacterData(LF2CharacterData data)
    {
        // 设置移动速度
        // 加载精灵图
        // 设置动画帧
    }
}
```

### 场景2: 动画系统集成

```csharp
public class AnimationController : MonoBehaviour
{
    private LF2CharacterDataWrapper characterData;
    private int currentFrameId = 0;

    void Start()
    {
        // 加载角色数据
        characterData = LF2CharacterJsonLoader.LoadFromFile("Assets/ExportedDAT/character_1_data.json");
    }

    void Update()
    {
        // 获取当前帧数据
        LF2FrameData frame = LF2CharacterJsonLoader.GetFrameData(characterData, currentFrameId);

        if (frame != null)
        {
            // 应用帧数据
            ApplyFrame(frame);

            // 切换到下一帧
            currentFrameId = frame.next;
        }
    }

    void ApplyFrame(LF2FrameData frame)
    {
        // 设置精灵图
        // 更新碰撞盒
        // 播放音效
    }
}
```

### 场景3: 关卡加载时预加载

```csharp
public class LevelLoader : MonoBehaviour
{
    public int[] levelCharacterIds;
    private Dictionary<int, LF2CharacterDataWrapper> preloadedCharacters;

    IEnumerator PreloadCharacters()
    {
        preloadedCharacters = new Dictionary<int, LF2CharacterDataWrapper>();

        foreach (int id in levelCharacterIds)
        {
            var data = LF2CharacterJsonLoader.LoadByCharacterId(id, "Assets/ExportedDAT");

            if (data != null)
            {
                preloadedCharacters[id] = data;
                Debug.Log($"预加载角色: {data.characterData.name}");
            }

            yield return null; // 分帧加载
        }

        Debug.Log($"预加载完成: {preloadedCharacters.Count} 个角色");
    }
}
```

### 场景4: 运行时修改和导出

```csharp
public class CharacterEditor : MonoBehaviour
{
    private LF2CharacterDataWrapper characterData;

    public void LoadAndModify()
    {
        // 加载数据
        characterData = LF2CharacterJsonLoader.LoadFromFile("Assets/ExportedDAT/character_1_data.json");

        // 修改数据
        characterData.characterData.walking_speed = 5.0f;
        characterData.characterData.jump_height = -20.0f;

        // 保存修改后的数据
        string modifiedJson = JsonUtility.ToJson(characterData, true);
        File.WriteAllText("Assets/ExportedDAT/character_1_modified.json", modifiedJson);

        Debug.Log("修改已保存");
    }
}
```

---

## 💡 最佳实践

### 1. 错误处理

```csharp
LF2CharacterDataWrapper data = LF2CharacterJsonLoader.LoadFromFile(filePath);

// 总是检查null
if (data == null)
{
    Debug.LogError("加载失败");
    return;
}

// 检查数据完整性
if (data.characterData == null || data.characterData.frames.Count == 0)
{
    Debug.LogError("数据不完整");
    return;
}

// 安全访问帧数据
LF2FrameData frame = LF2CharacterJsonLoader.GetFrameData(data, frameId);
if (frame == null)
{
    Debug.LogWarning($"未找到帧: {frameId}");
    return;
}
```

### 2. 性能优化

```csharp
// ✅ 好的做法：缓存数据
private Dictionary<int, LF2CharacterDataWrapper> cache = new Dictionary<int, LF2CharacterDataWrapper>();

LF2CharacterDataWrapper GetCharacterData(int id)
{
    if (!cache.ContainsKey(id))
    {
        cache[id] = LF2CharacterJsonLoader.LoadByCharacterId(id, dataFolder);
    }
    return cache[id];
}

// ❌ 不好的做法：每次都重新加载
void Update()
{
    // 每帧都加载，性能很差！
    var data = LF2CharacterJsonLoader.LoadFromFile(filePath);
}
```

### 3. 路径管理

```csharp
// ✅ 使用常量或配置
public static class GameConfig
{
    public const string CHARACTER_DATA_FOLDER = "Assets/ExportedDAT";
    public const string RESOURCE_PATH = "CharacterData";
}

// 使用
var data = LF2CharacterJsonLoader.LoadByCharacterId(id, GameConfig.CHARACTER_DATA_FOLDER);

// ❌ 硬编码路径
var data = LF2CharacterJsonLoader.LoadFromFile("Assets/ExportedDAT/character_1_data.json");
```

### 4. 调试技巧

```csharp
// 使用提供的打印方法
LF2CharacterJsonLoader.PrintCharacterInfo(data);

// 在Inspector中查看（使用CharacterManager）
[SerializeField] private LF2CharacterManager manager;

// 使用ContextMenu快速测试
[ContextMenu("测试加载角色1")]
void TestLoad()
{
    var data = LF2CharacterJsonLoader.LoadByCharacterId(1, "Assets/ExportedDAT");
    LF2CharacterJsonLoader.PrintCharacterInfo(data);
}
```

### 5. 不同环境的处理

```csharp
public class CharacterDataLoader : MonoBehaviour
{
    public LF2CharacterDataWrapper LoadCharacter(int id)
    {
        #if UNITY_EDITOR
        // 编辑器模式：从Assets文件夹读取
        return LF2CharacterJsonLoader.LoadByCharacterId(id, "Assets/ExportedDAT");

        #elif UNITY_STANDALONE
        // PC平台：从StreamingAssets读取
        return LF2CharacterJsonLoader.LoadFromStreamingAssets($"character_{id}_data.json");

        #elif UNITY_ANDROID || UNITY_IOS
        // 移动平台：从Resources读取（需要提前放入Resources文件夹）
        return LF2CharacterJsonLoader.LoadFromResources($"CharacterData/character_{id}_data");

        #else
        return null;
        #endif
    }
}
```

---

## 📝 总结

### 推荐使用方式

| 场景 | 推荐方法 |
|------|---------|
| **编辑器测试** | 使用 JSON读取器窗口工具 |
| **运行时单个角色** | `LoadFromFile` 或 `LoadByCharacterId` |
| **运行时多个角色** | 使用 `LF2CharacterManager` 组件 |
| **打包后的游戏** | `LoadFromResources` 或 `LoadFromStreamingAssets` |
| **自定义需求** | `LoadFromJsonString` |

### 文件位置

```
Assets/NTSD/
├── Scripts/Animation/
│   ├── LF2CharacterJsonLoader.cs      (核心加载器)
│   ├── LF2CharacterManager.cs         (角色管理器)
│   └── JSON读取使用示例.cs            (10个示例)
│
└── Editor/
    └── LF2JsonReaderWindow.cs          (可视化工具)
```

### 快速参考

```csharp
// 最快上手
var data = LF2CharacterJsonLoader.LoadFromFile("path/to/file.json");

// 游戏运行时
var manager = GetComponent<LF2CharacterManager>();
manager.LoadCharacter(1);

// 可视化测试
// Unity菜单 → LF2 Tools → JSON读取器
```

---

**更多详细示例请查看：**
- `JSON读取使用示例.cs` - 包含10个完整示例
- Unity编辑器：右键点击脚本 → 选择 ContextMenu 项测试

**遇到问题？**
- 检查文件路径是否正确
- 确认JSON格式是否正确
- 查看Console日志的错误信息
- 使用 `LF2CharacterJsonLoader.PrintCharacterInfo()` 调试
