> **2026-10-10 H08同Task恢复完成：SCOPED_MEMORY_GAIN_VERIFIED / 默认OFF。** NTSD-H08-NATIVE-SHEET-SHARING-20261010 / Change NTSD-H08-NATIVE-SHEET-SHARING-001 VERIFIED仅限定候选。生产代码冻结；两个Editor夹具修复后8/8，loaded publication OFF/ON实际Unity销毁回调释放40/39 owned对象、borrowed保留；原唯一四组source-worker内存1/1，纹理5→1/2→1/2→1、对象内存降80%/50%/50%，唯一负控1→1/5328B不变，像素/几何与本次精确资源释放全部通过。旧native残留UNKNOWN未清理，非整局/峰值收益；旧176.232MiB仍估计。报告 artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-20261010/resume-01/REPORT.md。下一仅审查限定接线，不自动推广/H10/Goal/队列；H08父项、H07及Android门保持。下方BLOCKED为修复前历史。

> **2026-10-10 H08 源sheet共享候选：BLOCKED_VALIDATION_HARNESS / FINITE_TASK_STOPPED / 默认OFF。** Task NTSD-H08-NATIVE-SHEET-SHARING-20261010；Change NTSD-H08-NATIVE-SHEET-SHARING-001 BLOCKED。原Editor首组8/8通过、补充1/3通过2失败；一次空OFF/ON诊断确认当前EditMode夹具未触达OnDestroy，显式清理不能替代已加载publication销毁验收；另有旧Temp缺图夹具前置缺失。共享算法缺陷尚未证实，完整生命周期/零残留门未过，四组内存比较NOT_STARTED，无收益或准入结论。通过的像素/ownership/取消/lease子证据及失败原件保留，不重测挑PASS。报告 artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-20261010/REPORT.md。下一仅审查同任务验证入口/可追溯清理；不自动推广/转H10/恢复Goal或队列，H08父项及H07/Android等最终门保持。下方为此前时点。

[本候选代码前后、实际验证与阻塞说明](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/IMPLEMENTATION-RECORDS/H08-NATIVE-SHEET-SHARING.md)

> **2026-10-10 NTSD-H08-NATIVE-SHEET-SHARING-20261010 IN_PROGRESS / NTSD-H08-NATIVE-SHEET-SHARING-001 PLANNED.** 三脚本native同publication源sheet共享候选默认OFF；先真实正确性/取消/lease，再唯一有界4组资源内存比较。无资源修改/ATLAS迁移/默认推广/旧Goal恢复。

## 2026-10-10 当前阶段更正：H-08

| 当前阶段 | 当前次序 | 原风险等级 |
|---|---|---|
| P1／当前优先评审 | 2 | 高 |

第2项：先核当前纹理、图集、解码暂存的实际驻留与生命周期；预印/ATLAS资源实施另有准确范围，不等待AB整体迁移。

本表覆盖下方旧评审卡的建议优先级/排程，技术证据与原验收范围保留。当前排序不证明收益、不自动授权跨域实施。[完整34项当前排程](../android-mobile-readiness-priority-risk-register.md) · [唯一进度](../battle-optimization-progress-tracker.md)。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-08 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 内存 / 中 |
| 当前状态 | 专项待批；本轮仅文档，未实施/复测 |
| 依赖的最小范围 | 与H-01先冻结共同Manifest/key/Lease接口；ATLAS预算和资源格式待决 |
| 下一步与具体方向 | 先保守生产者闭包和生命周期，再预印/按局装载；已有共享绑定不是完整ATLAS实现 |
| 完成边界 | 视觉等价、资源完整、批准steady/transition预算及设备生命周期；不得以减少资源漏画换内存 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

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
