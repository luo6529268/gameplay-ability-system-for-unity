# H-08 战斗视觉预烘焙与启动内存方案

> 优先级：高  
> 状态：`OPEN / SOURCE_FALLBACK_EXISTS / PREBAKE_PENDING / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前Logan预热仍遍历全部已加载配置，运行时处理正式PNG/BMP、创建Sprite/Texture并保留CPU atlas source。Auto在预估atlas超预算时可回SourceTexture2D，减少某些图集分配，但不约束源纹理、托管像素、音频和完整加载峰值。旧“408MB源图”属于迁移前统计，不能作当前正式内容数字。本项保留中央渲染，不改变战斗规则；总内存与H-10/H-11/M-12分别记账汇总。

## 解决方案

1. 构建期按当前正式decoder/crop解析DAT与PNG/BMP等源格式，生成不含逐帧Sprite的目录，保存VisualDataId/pic、Pack、Page/Array及slice、UV、Pivot、Size、source hash和UV采样边界。
2. 物理bank按ATLAS M0共现/碎片/字节/segment数据决定；Source/Pages/Array能力选择不预设固定一种。预印页承接已验texel-center clamp，禁止全游戏超级atlas，也不强制一角色一册或跨segment合并。
3. 使用ATLAS支柱一保守生产者全集闭包，覆盖Stage、掉落/碎片、分身/transform/mimic、特殊内建、公共SPARK/Shadow/WORDS/KO等当前可达资源；地图/模式内容遵守D-023排除及项目资源边界。
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

- Android正式战斗路径不直接读源图片/DAT、不创建逐帧Sprite、不运行时排版/组装图集；UI/背景/Editor对照路径保留各自所有权。
- 实际请求属于保守闭包，加载bank集合对应该闭包且物理bank允许含额外帧，不要求恰好最小资源集合；相同实体共享纹理/目录。
- 逐帧rect/UV/pivot/size/flip/Color32/透明、UV采样边界、缺图/超出sheet裁切、排序及可见结果与当前已批准基线一致，不恢复旧红线或裁切错误。
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

- 2026-10-06重扫：`Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:1473,2514,2582,2958-2965`；staging/Sprite与source fallback仍在。atlas policy预算不是整局总预算；UV边界见`BattleDynamicMeshBackend.cs:443-449`及既有红线修复报告。
- 实施时保存：源/产物 Manifest、烘焙报告、逐帧 parity、加载闭包、M0～M8 内存采样和设备纹理格式。
- 2026-09-06：方案文档建立；只记录方案，未开始实现。
- 2026-10-06：同步ATLAS三支柱，补当前输入/crop/公共资源、Source回退、音频及背压交叉引用；bank/预算/格式仍未冻结，未修改资源或启动烘焙。
