# H-08 战斗视觉预烘焙与启动内存方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED`  
> 最后更新：2026-09-06  
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前预热会遍历全部已加载配置，运行时解码 BMP，并同时维护 Sprite、Texture、CPU atlas source 和最终 atlas。约 408 MB 源图不能直接等价为峰值，但多份数据共存构成高可信内存风险。本项保留中央渲染，不改变战斗规则。

## 解决方案

1. Editor/构建期解析 DAT/BMP，生成不含逐帧 `UnityEngine.Sprite` 的 `BattleVisualCatalog`：`VisualDataId + Pic → Pack + Slice/Page + UV + Pivot + Size`。
2. 预先生成有边缘扩展的固定纹理页；正常路径使用有界 Texture2DArray bank，OrderedPages 作为能力回退。禁止一个全游戏巨大 atlas，也不建议每角色一个 array。
3. 按公共、角色 bank、武器、效果、地图分包，并通过 roster + 递归 OPoint/技能依赖闭包只加载本局所需 bank。
4. Android 纹理使用经真机画质/内存验证的 ETC2/ASTC profile，Point、Clamp、MipMap Off、Read/Write Off 作为像素风格起点而非未经验证的最终事实。
5. 先解除 CentralOnly 对 `MergedSprites/List<Sprite>/LegacySprite` 的隐藏绑定，再删除正式战斗路径的逐帧 `Sprite.Create`；背景、UI 和 Editor 对照 Sprite 独立保留。
6. 资源由 H-01 Provider 返回 generation-aware `BattleVisualLease`，在中央 submission 清空后按有序关闭释放。

## 实施步骤

1. B0 inventory：帧数、sheet、尺寸、透明规则、依赖闭包、重复内容、内存估算。
2. 建立帧目录并与当前 `BattleSpriteEntry` 的 rect/pivot/UV 做逐帧对照。
3. 先烘焙 OrderedPages 验证像素，再生成 Android Texture2DArray bank。
4. 接入按局异步加载与 Lease，保留旧路径 shadow compare。
5. 中央目录独立后移除生产 Sprite 兼容桥和运行时全量 atlas 构建。

## 验收条件

- Android 正式角色/武器/飞行物/战斗特效路径不读取源 BMP、不创建逐帧 Sprite、不构建全量运行时 atlas。
- 内容加载集合等于本局依赖闭包；1000 个相同实体共享同一纹理和目录。
- 逐帧 rect、UV、pivot、翻转、透明、排序和可见结果与迁移前基线一致。
- 低内存目标设备连续三次冷启动和进出战斗无 OOM/系统杀进程；退出后内存回到明确稳定区间。
- Central unresolved/unsupported/stale command 为 0；Array 与 Pages 回退均可用。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Catalog parity | 全 OID/pic 静态对照 | rect、pivot、size、ownership 无差异 |
| 像素 A/B | actor/weapon/effect/shadow、翻转/排序 | 无串色、黑边、错帧和偏移 |
| 依赖加载 | 最小 roster、复杂 OPoint roster | 实际加载 bank 与闭包一致，无缺失 |
| 内存阶段 | 冷启动、加载前、加载峰值、稳定、退出 | 每阶段有 CPU/GPU/Native 数据且满足预算 |
| 生命周期 | 取消加载、退出重进、连续三轮 | 无迟到发布、旧 Lease、资源泄漏 |
| Android 格式 | ETC2/ASTC × Array/Pages | 画质、加载、GPU 内存与兼容报告完整 |

## 证据与留痕

- 当前证据：`CharacterAnimtorManager.cs:869-1054` 同时维护多类 staging；移动 atlas policy 256 MiB 不代表全流程峰值。
- 实施时保存：源/产物 Manifest、烘焙报告、逐帧 parity、加载闭包、M0～M8 内存采样和设备纹理格式。
- 2026-09-06：方案文档建立；只记录方案，未开始实现。
