# 战斗资源与内存路线图：图集重构 × 低端机适配

> 计划标识：`BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001`
>
> 创建日期：2026-09-13；本版：R1（2026-09-14，按外部综合评审修正）
>
> 当前状态：`DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`
>
> 性质：资源管线与内存治理方案。本文件不授权修改任何 C#、Scene、Prefab、
> asmdef、资源、ProjectSettings 或运行行为；实施批次获批后按
> `docs/ai/CHANGE-LEDGER.md` 独立立项。
>
> 权威入口：`docs/ai/CURRENT-AUTHORITY.md`。规则权威为 NTSD 2.8-Logan 正式
> EXE + playable source closure；**DAT 与角色相关图片内容权威按 D-023
> （2026-09-12）采用 NTSD 2.8-Logan 正式 `resources/runtime`，Unity 原
> 138-DAT/旧图片仅为迁移前基线**；音频与非角色图片不因 D-023 自动切换。
> 本计划不改变战斗行为权威、33 ms cadence、十一阶段关闭合同。
>
> 评审说明：全文标注证据等级 `[已验证-代码]`、`[已验证-文档]`、
> `[推演-待测]`、`[提案-待批]`。
>
> 姊妹文档：`battle-performance-stats120-roadmap-plan.md`
> （`BATTLE-PERF-STATS120-ROADMAP-001`）。两计划**可独立实施**，但非零交点：
> 分册/bank 策略影响中央渲染 `SegmentCount` 与 draw，压缩格式影响 GPU 带宽，
> 纹理上传影响主线程/render thread，切场缓存影响内存与帧尖峰。因此共享
> M0 指纹、资源 Manifest 与中央渲染计数，最终设备认证联合执行。
>
> 修订记录：R0（2026-09-13 初版）；R1（2026-09-14）——按 GPT6 综合评审修正，
> 详见第 11 节。

## 1. 目标与不变量

### 1.1 目标定义

| 编号 | 目标 | 判定标准 |
|---|---|---|
| G1 | 常驻纹理内存可控、可预估，**双预算门禁**：SteadyResidentBudget（当前战斗稳定资源上限）与 TransitionPeakBudget（旧 Lease + 新 Lease + 最大单批 staging 上限）；装载前能算出本场两项预估，超预算 fail-closed | 门禁输出与实测偏差可解释；两项预算数字经 M0 冻结 |
| G2 | 加载期托管峰值不随全量内容线性增长：移除全量解码缓冲/全量页数组/运行时排版的线性峰；**切场瞬态峰值受单个 bank/批次上限约束**（R1：不承诺"峰值≈0"——IO 缓冲、解压 staging、Native texture、GPU 上传、驱动临时内存仍存在，按预算实测管理） | 实测峰值 ≤ 对应档位 TransitionPeakBudget |
| G3 | 低端机可判定、可运行：能力判定 → 档位解析 → 预算门禁 → 分级画质，全链 fail-closed | 档位/格式判定单测矩阵通过；超预算场景被明确拒绝或降档 |
| G4 | 视觉分级可控：任何有损/降质变体须通过画质验收并按设备实际格式能力启用 | ASTC/ETC2/降分辨率变体 A/B 记录在案 |

### 1.2 不变量

- 战斗模拟行为零变化（不触碰逻辑 tick、pass、RNG、checksum）；
- 中央渲染命令流、segment 合并语义、fail-closed 门禁不变；
- **内容权威：D-023**——打包/预印/统计的输入是 NTSD 2.8-Logan 正式
  `resources/runtime` 的 DAT 与角色图片（含其 PNG 形态）；Unity 138-DAT/
  旧图片仅作迁移前基线对照；
- 同一场战斗从装载到结束使用同一套资源变体（装载时锁定，运行中不切换）；
- 不做战斗中途换册、换档、重排（见第 9 节）。

### 1.3 术语澄清（R1 强化）

- **预印（Preprint/Bank）**：构建期一次性印好的资源册，运行时只装载；
- **装载（Load/Deploy）**：运行时解析参战闭包 → 选择已存在的预印 bank →
  异步加载 → 绑定中央目录。**装载不包含任何像素解码排版、页面分配或图集
  组装**——这是与"动态图集"的本质区别，也是 R1 修正的核心语义；
- **动态图集**：运行时增删重排图集。本计划明确不做；
- **ASTC**：移动端压缩格式，4x4 = 8bpp（RGBA32 的 1/4），有损；**由设备
  `SystemInfo` 格式能力决定是否可用，不由档位标签决定**（见支柱三）。

## 2. 当前事实基线

### 2.1 已验证事实

| 编号 | 事实 | 证据 |
|---|---|---|
| F1 | 现状为"全量一本"：Loading 流程加载 data.txt 全部角色配置，统一组装单一图集发布 | `[已验证-代码]` `Assets/NTSD/Scripts/UI/LoadingPrewarmController.cs`（CharacterConfig 任务）→ `CharacterAnimtorManager.ParseCharacterFrameConfigs` / `ApplyLoadedCharacterConfigs` / `TryBuildUnifiedCentralAtlasPublication`（R1 修正路径笔误） |
| F2 | 图集页 2048×2048、RGBA32、无 mipmap、Point 过滤；每页 16 MB；`EstimateAtlasBytes = 页数×16MB` | `[已验证-代码]` `BattleAtlasLayoutPlanner.PageSize`、`BattleAtlasDiagnosticInputs.EstimateAtlasBytes` |
| F3 | 平台预算二分（Mobile 256 MB / Desktop 512 MB）；降级链 `AtlasTextureArray → AtlasPageTexture2D → SourceTexture2D`；`allocationGuard` 钩子存在但当前传 null | `[已验证-代码]` `BattleRenderingPlatformPolicy`、`BattleAtlasCapabilityPolicy`、`BattleRenderingPolicyResolver.ResolveAtlas` |
| F4 | 超大图排除：>2046 px sheet 不进图集（SourceTexture2D 直绑）；超设备 `MaxTextureSize` 拒绝发布 | `[已验证-代码]` `TryClassifyCentralAtlasSources`、`IsPageEligible` |
| F5 | 加载期 CPU 全量组装：解码源像素、全量页缓冲（页数×16 MB 托管）与 GPU 拷贝瞬态并存；上传后 `Apply(false, makeNoLongerReadable:true)` 释放 CPU 副本 | `[已验证-代码]` `BattleAtlasResourceBuilder.AssemblePages` / `TryBuild` |
| F6 | 回收粒度为整本：仅内容集变更时重印并销毁旧册；运行中不增不减；无按时间回收（`LF2ObjectPool` 120 s 为对象池，与图集无关） | `[已验证-代码]` `TryCommitSpritePrewarmInvocation` / `DestroyStagedPresentation` |
| F7 | `SourceTexture2D` / `AtlasPageTexture2D` 绑定模式全链路可用；中央渲染 segment 按纹理粒度切分 | `[已验证-代码]` `BattleSpriteCentralBindingMode`、`BattleDynamicMeshBackend.IsCompatible` |
| F8 | 内容权威已切换 D-023：正式输入为 NTSD 2.8-Logan `resources/runtime` 的 DAT 与角色图片；当前正式迁移路径已涉及 PNG | `[已验证-文档]` AGENTS.md D-023 条款、CURRENT-AUTHORITY.md D-023 决定（2026-09-12） |

### 2.2 推演与数据缺口（`[推演-待测]`，M0 输入）

| 编号 | 内容 |
|---|---|
| C1 | 全量内容（D-023 正式口径）总字节、单角色/单 bank 字节分布未知 |
| C2 | 跨角色重复帧占比未知（换色/编辑角色、通用特效帧） |
| C3 | 典型战斗参战闭包占全量比例未知（先验约十分之一） |
| C4 | ASTC 4x4 在 Point 过滤放大上屏下的画质结论未知；**目标设备 ASTC/ETC2 格式支持矩阵未知**——不支持 ASTC 的 Android 设备会在装载时解压回 RGBA32，反而增加内存与加载成本 |
| C5 | 20 个互不相同角色最坏集合的字节未知；**切场双 Lease 并存的过渡峰值**未知 |
| C6 | 预印 bank 的共现频率、碎片率、segment/draw 影响未知（与 PERF 计划共享统计） |

## 3. 方案架构：三支柱（全部 `[提案-待批]`）

### 支柱一：资源组织——逻辑所有权与物理 bank 分离（R1 修正）

- **逻辑依赖单位**：角色 / 武器 / 特效 / 地图（visual data id 及其 pic）；
- **物理预印单位 = Bank**：受字节上限约束的打包单元，例如 Common、
  Fighter-A/B、Weapon、Effect、Map。bank 划分由 M0 的共现频率、典型 roster、
  20 唯一角色最坏集合、页面碎片率、segment 影响、资源更新频率共同决定，
  **不强制"一角色一册"**（避免页面尾部浪费、小册泛滥、segment 与句柄数
  膨胀）；
- **目录**：`VisualDataId + Pic → Bank + Page/Slice + UV + Pivot`；
- **参战闭包 = 静态保守上界**（可多装、不可漏装），生产者全集（R1 补全）：
  双方选择角色 → 武器 → opoint 链递归（特效实体、召唤物）→ **Stage spawn、
  随机掉落武器、武器碎片、分身/transform/mimic、持有/拾取切换 visual
  identity、state 13/18 等专用生成、通用 SPARK/Shadow、地图对象、内建/特殊
  OID 规则（0/999 等）、D-023 正式内容的资源引用关系**；
- 每条依赖边记录来源（source oid/frame、producer kind、target oid、字段/
  内建规则、是否条件性、证据），并配运行时断言：**实际请求必须 ∈ 预计算
  闭包**，违例即 fail-closed 报错；
- **装载语义（R1 关键修正）**：装载 = 解析闭包 → 选择已预印 bank → 异步
  加载 → 绑定中央目录。**装载路径不做任何像素解码排版、页面分配或图集
  组装**；
- 装载触发规则：参战闭包变化导致所需 bank 集变化时，**装载新的预印 bank
  集**（非重印）；连续相同集合复用已装载 bank；
- 资源生命周期（R1 新增）：bank 以 Lease 持有（引用计数），支持取消与迟到
  回调（校验 session epoch/world generation 后丢弃）、缓存上限、切场释放
  顺序。

### 支柱二：构建期预印——打包全部移出运行时

- 预印对象是 D-023 正式内容的 bank（M0 后冻结 bank 划分）；
- **烘焙器源格式无关**（R1）：输入按正式图像源设计（当前含 PNG），不把 BMP
  写成唯一格式；
- 产出：bank 纹理资产 + 确定性目录 Manifest（bank/page/slice/UV/pivot/
  source hash/frame mapping）；
- **确定性分级（R1 修正）**：
  - 必须：同输入 + 同 pinned Unity/工具/目标/配置 → Manifest 与语义 hash
    一致；
  - 尽量：相同固定构建环境内产物二进制一致；
  - 不承诺：跨 Unity/压缩器/平台版本逐字节一致。

### 支柱三：设备能力判定 + 档位 + 双预算门禁（R1 修正判定顺序）

1. **硬能力判定（先于一切档位标签）**：图形 API、`supports2DArrayTextures`、
   `SystemInfo.IsFormatSupported(目标 GraphicsFormat, 采样用途)`、
   `copyTextureSupport`、`maxTextureSize`；
2. **格式选择跟随能力而非档位**：支持 ASTC 且画质验收通过 → ASTC 4x4
   （必要时 6x6）；不支持 ASTC 但 GLES3/Vulkan 支持 ETC2 → ETC2 RGBA8
   候选（仍须画质与真机验证）；均不满足 → 经测量的兼容格式或明确拒绝。
   特别注意：**不支持 ASTC 的设备会在装载时解压回 RGBA32，反而增大内存**，
   因此 Low 档不默认等于 ASTC；
3. **档位解析（Low/Mid/High）**：硬能力 → 实测/系统预算（应用可用内存
   口径，非 `systemMemorySize` 直读）→ 已认证设备覆盖表（可更新配置）→
   用户手选（自动/流畅/标准/高清，手选覆盖自动）。GPU 型号字符串仅作辅助，
   不作为格式能力判断依据；
4. **双预算门禁**：档位对应 SteadyResidentBudget 与 TransitionPeakBudget；
   装载前用参战闭包预估（按唯一 visual dependency set 计，不按实体数）比对；
   超预算的准入顺序：已认证兼容变体降档 → 仍超则低内存切场模式
   （先停旧战斗、清空中央 submission、释放旧 Lease、保持 loading scene、
   分批装载新 bank、验证后启动新 World——牺牲旧画面连续，换取峰值可控，
   按档位启用）→ 仍超则装载界面明确拒绝；
5. **档位消费时机**：装载时消费一次并锁定全场；只决定纹理变体、render
   scale、双预算；战斗逻辑无感，运行中不切换；
6. 先不引入 Unity Adaptive Performance 与首启跑分。

## 4. 实施路线（R1 重排：P3 先于 P2）

```text
M0 测量统计（D-023 正式内容口径）──┬──> P1 设备能力+档位解析器
                                    └──> P3 构建期预印 bank 管线
                                              └──> P2 按局装载（只装载，不组装）
                                                    └──> P4 压缩变体与设备选择
```

顺序变更理由：若 P2 先于 P3 按局组装图集，即构成运行时解码/排版/建图集，
与"不做运行时动态图集"排除项矛盾（R0 措辞缺陷，R1 修正）。过渡期原型可用
既有 `SourceTexture2D`/已存在页面验证装载链路，但须标记为开发过渡路径，
不作为正式低内存方案。

### 4.0 M0：测量与审计（零战斗代码改动）

1. **内容审计（D-023 正式口径）**：全量总字节、单角色/单 bank 分布、跨角色
   重复帧占比。资源迁移完成前可先在现有内容上完善工具，**最终数字必须在
   D-023 正式内容上重跑**；
2. **参战闭包审计**：实现生产者全集静态扫描（3 支柱一清单），统计典型战斗
   占比，并做穷尽性验证（全 catalog 扫描 + 断言机制演练）——闭包漏扫 =
   fail-closed 拒画，穷尽性是硬门禁；
3. **格式能力矩阵**：目标设备档位的 ASTC/ETC2 `IsFormatSupported` 调研与
   解压回退行为确认；
4. **ASTC 画质 A/B**：真机、正式 Point 过滤与放大倍率、覆盖角色/武器/特效/
   透明边缘/翻转/重叠场景，结论记录在案。

### 4.1 P1：能力判定 + 档位解析器

- 能力判定（格式/API/2DArray/CopyTexture/maxTextureSize）与档位解析分离；
- 档位解析为纯函数 + 注入输入 + 设备矩阵单测；GPU 字符串仅辅助。

### 4.2 P3：构建期预印管线（先于 P2）

- bank 划分冻结（依据 M0 C1/C2/C6）；烘焙器源格式无关；产出纹理资产 +
  Manifest；
- 确定性验收按支柱二分级执行。

### 4.3 P2：按局装载（只装载，不组装）

- 参战闭包 → bank 集 → Lease 异步装载 → 绑定中央目录（复用
  `SourceTexture2D`/`AtlasPageTexture2D` 全链路）；
- 资源 Lease/取消/session epoch/缓存上限/切场释放顺序落地；
- 双预算门禁接入装载流程（G1）；
- 双路径期：新装载路径与现有全量图集路径 shadow compare，通过后切换，
  旧路径保留一个回滚周期；
- 渲染侧量化：bank/page 粒度的 segment/draw 变化（与 PERF 共享统计），
  超预期则调整 bank 合并或回退该档位。

### 4.4 P4：压缩变体与设备选择

- 前置：P3 + 格式能力矩阵 + ASTC/ETC2 画质 A/B 通过；
- 变体交付载体（Built-in / AssetBundle / Addressables、离线策略、多格式
  变体构建、包版本与内容 hash）为开放决策 D6；
- 半分辨率/更高压缩比档：首批不做，条件性后置（20 唯一角色最坏集合仍超
  预算时再评估，需单独视觉批准）；
- 不做运行时压缩。

## 5. 与既有计划/合同的关系

- `BATTLE-PERF-STATS120-ROADMAP-001`：可独立实施；共享 M0 指纹、资源
  Manifest、中央渲染计数（segment/draw/GPU/上传）；最终设备认证联合执行；
- AGENTS.md 第 14 节（移动端，USER_HOLD）：本计划是其资源侧延长线细化，
  不启动该节其余内容；
- 边界重构计划：无工程依赖；
- 排除项延续：运行时动态图集、按闲置时间逐张回收、运行时压缩、战斗中途
  换册/换档。

## 6. 风险与回滚

| 风险 | 表现 | 控制 |
|---|---|---|
| opoint/生产者闭包漏扫 | 运行时请求未装载资源 → fail-closed 拒画 | M0 穷尽性验证硬门禁 + 运行时断言；拒画即报错不静默 |
| 低端机不支持 ASTC | 装载时解压回 RGBA32，内存反增 | 能力判定先行（支柱三.1）；ETC2/兼容格式候选路径 |
| 切场双 Lease 峰值 OOM | 低端机换场崩溃 | TransitionPeakBudget + 低内存切场模式（先释放旧再装载新，按档位启用） |
| bank 碎片/小册泛滥 | 字节浪费、segment/handle 膨胀 | bank 划分由 M0 数据决定；验收量化 segment/draw |
| 双路径行为差异 | 两路径画面不一致 | shadow compare 逐帧对照，first difference 即停 |
| 预算门禁误拒 | 可运行场景被拒 | 门禁数字与实测偏差纳入 G1；设备覆盖表兜底 |
| 构建不确定性 | 跨环境产物不一致 | 确定性分级（支柱二）：语义/Manifest 必须一致，二进制仅限 pinned 环境 |
| 性能改造引入分配 | 0GC 合同破坏 | 装载路径 allocation 门禁 |

回滚单位：每阶段独立 Change；P2 回退 = 回全量图集路径（保留至 P4 验收后）。
**回退语义边界（R2）**：回退只代表恢复迁移前功能路径，**不代表低端机
G1/G2 已通过**——全量运行时图集本身即低端机内存风险来源；发生回退时，
该设备档位保持未认证或明确拒绝，不得因路径回退而宣称达标。

## 7. 完成定义

仅当以下全部满足才可报告达成：

1. G1–G4 判定全部通过（含双预算实测、格式能力矩阵、画质 A/B 记录）；
2. M0 报告在案且 C1–C6 全部有数字（D-023 正式口径）；
3. 战斗模拟 checksum/回放与基线逐位一致（持续门禁）；
4. 双路径 shadow compare 通过记录在案；
5. `BattleRuntimeSelfCheck` 与相关 EditMode 通过；
6. 各阶段 Change Record 状态链完整。

## 8. 开放决策清单

| 编号 | 决策 | 建议（源自 GPT6 评审，待用户批准） |
|---|---|---|
| D1 | 是否批准 M0 | **已批准（R2，2026-09-14）**；必须以 D-023 正式内容为最终口径（迁移期间可先完善工具，数字在正式内容上重跑）；旧内容结果标注 `TOOLING_BASELINE_ONLY`，正式内容迁移后结果标注 `FORMAL_ATLAS_M0`，只有后者可冻结 bank/预算/格式决策；只读设计工作（统计 schema、catalog 输入、依赖边分类、报告设计、M0 指标确认）可立即开展，Unity Editor 扫描/导入的实际启动与活跃 NTSD28-Q06 恢复任务协调 |
| D2 | 20 人最坏预算与准入策略 | **变体降档优先，仍超则拒绝**；预算按"唯一 visual dependency set"计；同时冻结 steady 与 transition peak 两数（M0 后定） |
| D3 | ASTC/格式画质判定 | 美术/视觉负责人 + 客户端工程共同签字；目标 Android 真机 + 正式 Point 过滤/放大倍率 + 全场景对比；工程同步核对 GraphicsFormat/内存/加载时间/回退 |
| D4 | 半分辨率/更高压缩档 | 首批不做，条件性后置（见 4.4） |
| D5 | 去重投资阈值 | 量化门槛后立项：典型战斗可驻留字节节省 ≥10%，或 20 人最坏集合节省 ≥32 MiB，且不显著增加 segment/查找/跨包耦合 |
| D6（新增） | 预印交付载体：Built-in / AssetBundle / Addressables；离线策略；多格式变体构建；包版本与内容 hash；Lease/取消/epoch/缓存上限 | P3 设计前锁定 |
| D7（新增） | 切场策略与缓存上限：高档双 Lease 事务切换；低档先释放旧战斗再加载（牺牲断帧） | 按档位写入策略表 |

## 9. 明确不做的事

1. 运行时动态图集（含**装载路径上的任何像素解码排版/页面分配/图集组装**）；
2. 按"闲置时间"逐张回收；
3. 运行时压缩/解压缩为新格式；
4. 战斗中途换册或切档位；
5. 触碰逻辑 cadence、pass、RNG、checksum 语义；
6. 未过 A/B 验收即上线任何有损变体；
7. 以 BMP 为唯一输入假设设计烘焙器（源格式无关，覆盖 D-023 PNG）。

## 10. 恢复方式

保持 `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`。批准启动时：

1. 先读 `docs/ai/CURRENT-AUTHORITY.md`（含 D-023）与当前 `AGENTS.md`；
2. 从 M0 开始，按第 8 节 D1→D3→D6→D7→D2→D5→D4 顺序逐项确认
   （R2：D7 前移——切场策略/双 Lease/缓存上限直接决定 P2 的 Lease API 与
   TransitionPeakBudget，必须在 D2 冻结预算前定型）；
3. 立项时重新扫描代码现状，不假设本文引用的文件行号仍有效；
4. 文件被重命名/移动/拆分时同步更新索引文档与姊妹文档引用。

## 11. 修订记录

| 版本 | 日期 | 内容 |
|---|---|---|
| R0 | 2026-09-13 | 初版 |
| R1 | 2026-09-14 | 按 GPT6 综合评审修正：①内容权威 Direction B → D-023，输入含正式 PNG（阻断级 1）；②P3 预印先于 P2 装载，消除"运行时重印"与动态图集排除项的矛盾（阻断级 7）；③装载语义重定义：只装载不组装（阻断级 7）；④G2 峰值≈0 → 峰值受 bank/批次上限约束（高 13）；⑤新增 Steady/Transition 双预算与低内存切场模式（高 14）；⑥逻辑所有权与物理 bank 分离，bank 由 M0 数据决定（高 15）；⑦闭包生产者全集补全 + 依赖边溯源 + 运行时断言（高 16）；⑧ASTC 由格式能力决定，ETC2 候选，低档不默认 ASTC（高 12）；⑨构建确定性分级（高 17）；⑩资源 Lease/取消/epoch/缓存上限（高 13 附带）；⑪与 PERF 关系改"可独立实施+联合认证"（中 22）；⑫交付载体开放决策 D6、切场策略 D7（中 23）；⑬F1 证据路径修正（中 24）；⑭D1–D5 建议更新 |
| R2 | 2026-09-14 | 按 GPT6 R1 复核修正（通过，附轻微修正）：①恢复顺序改为 D1→D3→D6→D7→D2→D5→D4，D7 前移至预算冻结前——切场策略/双 Lease/缓存上限直接决定 P2 Lease API 与 TransitionPeakBudget；②P2 回滚语义边界：回退≠低端认证，回退时该设备档位保持未认证或明确拒绝；③D1 标记已批准，区分 TOOLING_BASELINE_ONLY / FORMAL_ATLAS_M0，Editor 扫描启动与 NTSD28-Q06 协调 |
