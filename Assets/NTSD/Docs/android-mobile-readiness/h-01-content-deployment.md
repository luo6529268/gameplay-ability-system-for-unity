# H-01 Android 生产内容部署方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前生产代码通过项目相对路径、开发机绝对路径和普通 `File.*` API 读取 `data.txt`、DAT、BMP 与 SPARK。Android Player 不具备 Unity 编辑器项目目录语义，因此 APK 构建成功也不代表内容可加载。本项解决“内容如何进入安装包并被稳定定位”，不在内容权威策略确定前覆盖原始 DAT、BMP、WAV、Prefab 或 Scene。

## 解决方案

1. 建立版本化 `BattleContentManifest`，为战斗数据、视觉包、公共 SPARK/Shadow、声音和地图分配稳定资源 key，禁止正式 Player 保存开发机路径。
2. 建立 `IBattleRuntimeContentProvider`：Editor 实现读取生成资产，Android 实现从正式 Unity 资源包异步加载；业务层不直接依赖 Addressables、AssetBundle 或文件路径。
3. 构建期把当前权威 DAT 转为紧凑运行时目录，把 BMP 转为 H-08 定义的预烘焙纹理包；源内容只作为编辑器输入。
4. Manifest 保存源内容哈希、烘焙器版本、产物哈希、依赖和包大小；过期或缺失时构建 fail-close。
5. 本局加载通过 roster、地图和递归 OPoint/技能依赖闭包得到资源包集合，公共包去重并通过显式 Lease 管理生命周期。

## 实施步骤

1. 只读盘点所有生产内容入口和运行时路径调用，冻结资源 key 规范。
2. 建立 Manifest schema、哈希和构建前完整性验证，不先改变运行路径。
3. 建立 Editor/Packaged Provider seam，并以同一目录结果做 shadow compare。
4. 接入预烘焙数据、纹理、声音和地图包；补取消、迟到请求和 session generation 检查。
5. Android 路径验证通过后，移除正式 Player 对项目目录和源 BMP/DAT 的依赖；Editor 工具可保留源读取能力。

## 验收条件

- APK 冷启动不依赖项目根、当前工作目录、外部存储或开发机绝对路径。
- 本局所有直接与递归资源依赖都能由 Manifest 唯一解析，重复依赖只加载一次。
- 缺失、哈希不符或版本不兼容时给出明确 resource key 和失败阶段，不生成空角色。
- 清除应用数据、飞行模式、首次安装和第二次进入战斗均可完成内容加载。
- 战斗退出后非共享 Lease 可回收；迟到异步请求不能污染下一局。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Manifest 单元测试 | 缺失、重复、循环依赖、错误哈希 | 确定性 fail-close，诊断含资源 key |
| Editor/Player 对照 | 同 roster、地图和内容版本 | 解析出的定义、帧数与依赖集合一致 |
| Android 冷启动 | 清数据、飞行模式、ARM64 Development APK | 能进入普通战斗，无 `Assets/...` 文件访问失败 |
| 生命周期 | 连续三次进入/退出、快速取消后重进 | 无旧 session 发布、无资源泄漏和残留 Lease |
| 包闭包检查 | 解包 APK/AAB 并读取构建报告 | Manifest 声明的所有正式包均存在且哈希一致 |

## 证据与留痕

- 当前证据：`Assets/NTSD/Scripts/Animation/GameDataManager.cs:40-53`、`CharacterAnimtorManager.cs:31,444-516,590-618,1068-1075`。
- 实施时必须记录：内容 Manifest、源/产物 SHA-256、BuildReport、APK SHA-256、加载报告和三轮生命周期结果。
- 2026-09-06：方案文档建立；未修改实现，状态保持 `IMPLEMENTATION_NOT_STARTED`。
