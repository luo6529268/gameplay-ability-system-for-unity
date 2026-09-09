# H-02 Android Build 与 Scene 闭包方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

`EditorBuildSettings.asset` 当前没有正式 Scene 清单，仓库也没有可复核的 APK/AAB。问题不在 SDK/NDK 缺失，而在可重复的入口 Scene、Additive 依赖、URP、输入和内容闭包尚未形成版本化构建合同。

## 解决方案

1. 建立明确的 Android Build Profile，按 GUID 声明 Bootstrap、加载、战斗与必要 Additive Scene，禁止依赖当前打开的 Scene。
2. 构建前验证 active URP Asset、RendererData、`BattleRenderFeature`、GameConfig、Input Actions、H-01 内容 Manifest 和 H-04 ABI 配置。
3. 建立可重复的 Development APK 与 Release AAB 入口；构建参数和输出目录显式化。
4. 构建后生成成品清单：BuildReport、APK/AAB SHA-256、Scene、ABI、图形 API、内容包与版本指纹。

## 实施步骤

1. 由用户确认正式首 Scene 与菜单/加载边界；不得从当前 Editor 状态猜测。
2. 添加只读 Preflight，先报告缺失项，再建立构建 Profile。
3. 建立 Android Development Build，随后增加 Release/AAB 配置；签名由 L-02 独立处理。
4. 建立 Post-build Inspector，检查 Scene、native library、Manifest 和资源包。

## 验收条件

- 干净环境可用单一、记录化入口重复构建 ARM64 APK，BuildReport 为 `Succeeded`。
- 构建不依赖已打开 Scene、未保存对象或本机绝对内容路径。
- APK 可安装、冷启动、进入战斗、退出并重新进入。
- 产物报告能唯一关联源码/dirty manifest、内容 Manifest 和构建配置。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Preflight 负例 | 移除一个 Scene/Renderer/内容 key | 构建前准确阻断并指出 GUID/key |
| 重复构建 | 相同输入执行两次 Development Build | 配置与闭包一致，差异可解释 |
| 干净启动 | 未打开目标 Scene启动批处理构建 | 成功且使用 Profile 中的 Scene 顺序 |
| 安装运行 | ARM64 设备清数据安装 | 冷启动、进战斗、退出、重进通过 |
| 成品审计 | 解包 APK/AAB | Scene、ABI、内容包与报告一致 |

## 证据与留痕

- 当前证据：`ProjectSettings/EditorBuildSettings.asset` 为 `m_Scenes: []`。
- 实施时保存：Build Profile、完整命令、BuildReport、产物 SHA-256、包内 Scene/ABI/资源清单和安装日志。
- 2026-09-06：方案文档建立；实现未开始。
