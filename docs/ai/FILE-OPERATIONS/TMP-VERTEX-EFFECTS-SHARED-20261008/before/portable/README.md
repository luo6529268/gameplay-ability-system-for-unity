# 破晓项目内置 TextMeshPro 通用版

这份交付物来自 `Bin/Client/Assets/TextMeshPro` 的项目内置源码（`TMP_Settings.version` 为 `1.4.0`），不是从 `com.unity.textmeshpro` PackageCache 导出的官方 UPM 包，也不是独立重写的文字引擎。它保留了 `TMPro` 命名空间与 `Unity.TextMeshPro` 程序集名称，以便已有 TMP 调用代码迁移。

> **导入前必须检查：** 目标项目不能同时安装官方 `com.unity.textmeshpro`。两套源码都声明 `Unity.TextMeshPro` 和 `Unity.TextMeshPro.Editor`，共存会在脚本编译前报 `Assembly with name ... already exists`。如果官方包是其他包的依赖，应先在 Package Manager 中查明依赖关系；仅修改本副本的 asmdef 名称不能解决重复 `TMPro` 类型。

## 使用

1. 在目标 Unity 项目中确认已安装 **uGUI**（`com.unity.ugui`）。通过 `Window > Package Manager > In Project` 移除官方 `com.unity.textmeshpro`，并确认没有其他定义 `TMPro` / `Unity.TextMeshPro` 的副本。如果 Remove 不可用，先在该包的 Dependencies 中查找依赖它的包，处理依赖后再继续导入。
2. 将此目录中的 `Assets/TextMeshPro` 文件夹连同 `.meta` 文件复制到目标项目的 `Assets` 下。目标项目若已有同名文件夹，应先人工合并并保护原有资源；不要直接覆盖。
3. 等待 Unity 导入与脚本编译。编辑器会自动调用 `Tools > Portable TextMeshPro > Initialize Default Font` 对应的初始化逻辑，基于随包的 OFL Liberation Sans 创建动态 SDF 默认字体，并写入 `TMPSettings.asset`。若首次导入顺序导致字体没有生成，可手动运行同一菜单项。
4. 创建 `TextMeshProUGUI` 或 `TextMeshPro` 组件使用。中文项目需自行添加有相应授权的中文字体资产并设置默认字体或 fallback；Liberation Sans 只提供基本西文字符。

内置设置位于 `Assets/TextMeshPro/Resources/TextMeshPro/TMPSettings.asset`。运行时使用 Unity `Resources` 读取它。富文本中的 `<font>`、`<material>`、`<gradient>`、`<sprite>` 名称会根据设置内的路径查找资源；若需要在 Player 中按名称动态加载，请把对应资产放到 `Assets/**/Resources/TextMeshPro/FontAssets`、`SpriteAssets` 或 `ColorGradientPresets`，路径与设置保持一致。直接在 Inspector 中引用的字体和材质不需要放入 `Resources`。

## 移植范围

- 包含：项目内置的 TMP Runtime、Editor、Shader、设置、默认样式、换行字符文件与 OFL Liberation Sans 字体。
- 通用化：原工程 `TMP_Settings.SyncLoadRes` 对 `Scripts.GameObjectPoolApi` / `Base.TRACE` 的依赖改为 Unity `Resources`；编辑器仍能按项目 `Assets` 路径查找资源。
- 稳定性修正：动态字体批量添加字符时去重，并跳过字体引擎返回的空字形；新增 3D 文本组件时的 `Reset` 会重新创建已销毁的 Mesh。这些都是隔离测试中触发的原内置源码问题。
- 排除：依赖 `rkt.UI` 的 `TextMeshProUV` 逻辑、PSD2UGUI 工具及其 DLL、项目专用字体/字库/图标。原 `TextMeshProUV.cs` 在副本中只留说明注释，不定义该组件。
- `TMP_Settings` 中的默认 Sprite 留空。使用 `<sprite>` 前须创建 Sprite Asset 并在设置中指定。
- 原目录没有 `Editor Resources` 图标和 `Package Resources` 归档。对齐按钮在图标缺失时显示文字，原包的 Essentials/Extras 导入菜单已禁用；旧项目 GUID 转换工具仍属原源码，未作为通用功能验证。

## 适用边界与回滚

按原项目的 Unity `2022.3.62f3` 与 uGUI 组织。不同 Unity 版本、渲染管线、目标平台的兼容性需要分别在目标项目验证；“通用”指去掉破晓项目依赖，并不保证所有 Unity 版本直接兼容。保留了源文件 `.meta` 的 GUID；生成的默认字体在目标项目内有新 GUID。

本交付物只是一份独立副本，不修改破晓项目的 TMP 源码和资源。若导入目标项目后需要回滚，在确认没有其他资源引用之后，移除导入的 `Assets/TextMeshPro` 文件夹，并恢复目标项目原先的 TMP 包配置。

随包的 `Fonts/LiberationSans - OFL.txt` 是 Liberation Sans 的许可文本。若要对外再分发整套 TMP 源码，请另行核对原始 TMP 源码的授权条件。

## 已执行的隔离验证

- 在临时的 Unity `2022.3.62f3` 项目中只安装 `com.unity.ugui` 及 Unity UI 模块，导入此副本，Runtime 与 Editor 脚本编译通过；存在旧源码的编译警告。
- 通过编辑器批处理入口生成了默认字体，`TMPSettings.asset` 已指向生成的字体。`Resources.Load<TMP_Settings>("TextMeshPro/TMPSettings")` 与 `TryAddCharacters("Hello")` 的检查通过。
- 在带图形环境的编辑器批处理里，创建 `TextMeshPro` 组件、调用 `ForceMeshUpdate()` 后检查到 5 个字符，测试通过。真实 Game View 视觉效果、Play Mode、Player Build、其他 Unity 版本及设备未验证。
