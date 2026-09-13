# 战斗资源与内存路线图：图集重构 × 低端机适配

> 计划标识：`BATTLE-ATLAS-MEMORY-LOWEND-ROADMAP-001`
>
> 创建日期：2026-09-13
>
> 当前状态：`DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`
>
> 性质：资源管线与内存治理方案。本文件创建时不授权修改任何 C#、Scene、
> Prefab、asmdef、资源、ProjectSettings 或运行行为；所有实施批次获批后须按
> `docs/ai/CHANGE-LEDGER.md` 规则独立立项。
>
> 权威入口：`docs/ai/CURRENT-AUTHORITY.md`；本计划不改变 NTSD 2.8-Logan 战斗
> 行为权威、33 ms 节奏、十一阶段关闭合同与 Direction B 内容权威。
>
> 评审说明：本文件供后续综合评审使用。全文标注证据等级：`[已验证-代码]`、
> `[已验证-文档]`、`[推演-待测]`、`[提案-待批]`。请按等级区分事实与假设。
>
> 姊妹文档：`battle-performance-stats120-roadmap-plan.md`
> （`BATTLE-PERF-STATS120-ROADMAP-001`，治理"每帧时间"）。两计划正交：本计划
> 治理"纹理内存与设备适配"，可独立推进，互不阻塞。

## 1. 目标与不变量

### 1.1 目标定义

| 编号 | 目标 | 判定标准 |
|---|---|---|
| G1 | 常驻纹理内存可控、可预估：战斗装载前能算出本场内存上界，超预算 fail-closed | 装载门禁输出"本场预估字节"且与实测偏差可解释 |
| G2 | 加载期 CPU 瞬态峰值消除或大幅压缩 | 装载期托管堆峰值不随全量内容线性增长 |
| G3 | 低端机可判定、可运行：档位解析 + 预算门禁 + 分级画质 | 档位解析单测矩阵通过；超预算场景被明确拒绝或降档 |
| G4 | 视觉结果分级可控：压缩/降质变体必须通过画质验收后才允许上对应档位 | ASTC/降分辨率变体 A/B 对比通过记录在案 |

### 1.2 不变量

- 战斗模拟行为零变化（本计划不触碰逻辑 tick、pass、RNG、checksum）；
- 中央渲染系统的命令流、segment 合并、fail-closed 门禁语义不变；
- DAT 内容与 Direction B 权威不因打包方式改变数值语义；
- 同一场战斗从装载到结束使用同一套资源变体（装载时锁定，运行中不切换）。

### 1.3 术语澄清

- **整本重印**：现策略为全量角色一次印入单一图集（见 F1），本计划将其改为
  "公共册 + 按角色分册"；
- **动态图集**：运行时增删重排的图集。本计划**明确不做**（见 9 节），注意
  区分"分册装载"（装载期一次性决定）与"动态图集"（运行期变更）；
- **ASTC**：移动端 GPU 原生压缩纹理格式，4x4 档体积为 RGBA32 的 1/4，
  有损，须过画质验收（G4）。

## 2. 当前事实基线

### 2.1 已验证事实

| 编号 | 事实 | 证据 |
|---|---|---|
| F1 | 现状为"全量一本"：Loading 流程加载 data.txt 全部角色配置，统一印入单一图集发布 | `[已验证-代码]` `Animation/UI/LoadingPrewarmController.cs`（CharacterConfig 任务）→ `CharacterAnimtorManager.ParseCharacterFrameConfigs` / `ApplyLoadedCharacterConfigs` / `TryBuildUnifiedCentralAtlasPublication` |
| F2 | 图集页 2048×2048、RGBA32、无 mipmap、Point 过滤；每页 16 MB；`EstimateAtlasBytes = 页数×16MB` | `[已验证-代码]` `BattleAtlasLayoutPlanner.PageSize`、`BattleAtlasDiagnosticInputs.EstimateAtlasBytes` |
| F3 | 平台预算分档：Mobile 256 MB / Desktop 512 MB；能力门禁降级链 `AtlasTextureArray → AtlasPageTexture2D → SourceTexture2D` | `[已验证-代码]` `BattleRenderingPlatformPolicy`、`BattleAtlasCapabilityPolicy.EvaluateArray`、`BattleRenderingPolicyResolver.ResolveAtlas` |
| F4 | 超大图排除：边长 >2046 px 的 sheet 不进图集（保留 SourceTexture2D 直绑）；超设备 `MaxTextureSize` 直接拒绝发布 | `[已验证-代码]` `CharacterAnimtorManager.TryClassifyCentralAtlasSources`、`BattleAtlasLayoutPlanner.IsPageEligible` |
| F5 | 加载期 CPU 全量组装：解码源像素、全量页缓冲（页数×16 MB 托管数组）与 GPU 拷贝瞬态并存；上传后 `Apply(false, makeNoLongerReadable:true)` 释放 CPU 副本 | `[已验证-代码]` `BattleAtlasResourceBuilder.AssemblePages` / `TryBuild` |
| F6 | 回收粒度为整本：仅内容集变更时重印并销毁旧册（`ownedResources` 跟踪）；运行中不增不减不重排；无按时间回收机制（全项目唯一闲置回收为 `LF2ObjectPool` 120 s 对象池，与图集无关） | `[已验证-代码]` `TryCommitSpritePrewarmInvocation` / `DestroyStagedPresentation`；`LF2ObjectPool` |
| F7 | `SourceTexture2D` 绑定模式全链路可用（F4 的超大图即走此模式），中央渲染 segment 按纹理粒度切分 | `[已验证-代码]` `BattleSpriteCentralBindingMode`、`BattleDynamicMeshBackend.IsCompatible` |
| F8 | 设备档位仅有平台二分（Mobile/Desktop 预算不同），无性能档位判定；`allocationGuard` 钩子存在但当前传 null | `[已验证-代码]` `BattleRenderingPlatformPolicy.ResolvePlatformCategory`、`BattleAtlasCapabilityPolicy` 构造参数 |

### 2.2 推演与数据缺口（`[推演-待测]`）

| 编号 | 内容 | 用途 |
|---|---|---|
| C1 | 全量内容总像素/字节未知；单角色分册平均 5~15 MB（RGBA32 口径）为量级估计 | 决定拆册收益与 20 人最坏预算 |
| C2 | 跨角色重复帧（换色/编辑角色、通用特效帧）占比未知 | 决定去重投资是否值得 |
| C3 | 典型战斗参战集合（双方角色 + opoint 可达闭包）占全量比例未知（先验估计约十分之一） | 决定 G1 的实际收益 |
| C4 | ASTC 4x4 在 Point 过滤放大上屏下的画质结论未知（平色块无损、边缘块有伪影风险） | G4 验收前置 |
| C5 | 20 个互不相同角色的最坏配置总字节未知 | 低端机准入门禁的预算数字来源 |

以上缺口由 M0 测量（4.0 节）补齐；C1–C5 任何一个被推翻都正常修正方案，
不视为失败。

## 3. 方案架构：三支柱（全部 `[提案-待批]`）

### 支柱一：拆册——"公共册 + 按角色分册"替代"全量一本"

- **公共册**：影子、SPARK 帧字模、数字、COM 标签等真正全局通用的封闭集合
  （现 `BattleCommonVisualCatalog` 成员），常驻不走；
- **角色分册**：每个 visual data id 的 sheet 独立成册，按"本场参战集合"装载；
- **参战集合 = 静态可达性闭包**：双方选择的角色 → 武器 → opoint 链可生成的
  全部对象（特效实体、召唤物）→ 递归闭合 → 场景对象。该闭包从 DAT 数据
  静态扫描得出，装载前列清单，运行期零变更；
- 装载粒度规则：**参战集合变了才重印；连续相同集合复用现有分册**；
- 换册顺序保持现合同：先印新、验收、瞬切、后毁旧；失败保旧册；
- 归属原则：成员资格永远由装载期静态分析决定，永远不由运行时使用行为决定。

### 支柱二：构建期预印——把打包挪出运行时

- DAT 内容确定性 → 每册可在构建期印好成为资产，运行时只装载不组装；
- 直接消除 F5 的三份并存瞬态峰值（目标 G2：峰值≈0）；
- 为支柱三的压缩变体提供载体（压缩必须在构建期做，运行时不压缩）。

### 支柱三：设备档位 + 预算门禁——低端机的"保证"机制

- **档位解析**（Low/Mid/High）：以 `SystemInfo.systemMemorySize`（RAM，第一
  权重）+ GPU 型号代际 + 图形 API + ASTC 支持做**纯函数启发式评分**；所有
  输入注入、可单测（风格对齐既有 `BattleRenderingPolicyResolver` 测试）；
- **三级兜底**：启发式评分 → 设备型号修正表（可更新配置）→ 用户手选
  （自动/流畅/标准/高清，手选永远覆盖自动）；
- **档位消费时机**：装载时消费一次并锁定全场；只决定纹理变体、render
  scale、内存预算三件事；战斗逻辑对档位完全无感，运行中不切换；
- **预算门禁**：每档一个内存预算，装载前用参战集合预估字节比对；超预算
  走明确分级路径（低压缩比变体/半分辨率变体→仍超则明确拒绝进入），
  延续 fail-closed 脾气。`allocationGuard` 钩子（F8）为现成挂点；
- **先不引入** Unity Adaptive Performance 包与首启跑分校准（依赖与成本，
  留作后续增强）。

## 4. 实施路线（各阶段独立立项、独立验收）

```text
M0 测量统计 ──┬──> P2 拆册装载 ──> P4 压缩变体（依赖画质验收）
              └──> P3 构建期预印 ──┘
P1 设备档位解析器（独立，可与 M0 并行）
```

### 4.0 M0：测量与审计（零战斗代码改动）

1. **内容审计**：用现有解码管线统计——全量总字节、单角色字节分布、跨角色
   重复帧占比（C1/C2）；
2. **参战集合审计**：实现 opoint 可达闭包的静态分析（只读扫描），统计典型
   战斗占比（C3），并验证闭包穷尽性（全数据文件扫描零遗漏，这是后续装载
   门禁的正确性硬指标）；
3. **ASTC 画质 A/B**：同一帧 ASTC 4x4 与 RGBA32 在目标放大倍率下并排对比，
   结论记录在案（C4）。

### 4.1 P1：设备档位解析器

- 纯函数 + 注入式输入 + 设备矩阵单测（模拟 RAM×GPU 组合）；
- 三级兜底结构；档位→预算/变体的映射表。

### 4.2 P2：拆册装载

- 分册数据结构与参战闭包消费；
- 装载路径优先复用既有绑定模式：分册页以 `AtlasPageTexture2D` 或
  `SourceTexture2D` 装载（F7 全链路已可用），不要求先建 Texture2DArray；
- 渲染侧影响评估：segment 改为按页/册粒度切分，draw call 从"理想个位数"
  升至"页数段"（个位到低双位数），仍远低于 legacy——该项在验收中量化对比；
- 双路径期：新分册路径与现有全量图集路径 shadow compare，通过后切换，
  旧路径保留一个回滚周期。

### 4.3 P3：构建期预印管线

- 打包工具链（编辑器脚本性质），产出确定性资产；装载端直载；
- 确定性验证：同 DAT 输入两次构建产物逐字节一致。

### 4.4 P4：压缩变体

- 前置：P3 管线 + M0 的 ASTC A/B 通过记录；
- Low/Mid 档启用 ASTC 4x4；是否增加更高压缩比/半分辨率档由 20 人最坏预算
  数字（C5）决定；
- 不做运行时压缩。

## 5. 与既有计划/讨论的关系

- `BATTLE-PERF-STATS120-ROADMAP-001`：正交。该计划治理帧时间，本计划治理
  内存；可并行推进，互为前置关系为零。唯一交点：两者都建议先做各自的
  M0 测量；
- AGENTS.md 第 14 节（移动端渲染重构，USER_HOLD）：本计划是其延长线的
  资源侧细化，不启动该节其余内容；
- 边界重构计划：无依赖。本计划不要求 Mono/非 Mono 分层先行；
- 此前会话讨论中明确排除项延续有效：运行时动态图集、按时间闲置回收、
  运行时压缩、战斗中途换册。

## 6. 风险与回滚

| 风险 | 表现 | 控制 |
|---|---|---|
| opoint 闭包漏扫 | 战斗中引用未装载分册 → fail-closed 拒画 | M0 第 2 项穷尽性验证作为硬门禁；拒画即报错，不静默 |
| ASTC 画质不达标 | 放大后边缘伪影可见 | G4 A/B 验收前置；不过则该档位维持 RGBA32 并如实报告预算缺口 |
| 分册页碎片浪费 | 页内对齐空隙使总字节高于全量紧排 | M0 统计碎片率；超阈值时调整分册粒度（按角色组合预印） |
| 页粒度 segment 增多 | draw call 上升 | 验收量化；超预期时合并小册或回退该档位 |
| 双路径期行为差异 | 两路径画面不一致 | shadow compare 逐帧对照，first difference 即停 |
| 预算门禁误拒 | 可运行场景被拒 | 门禁数字与实测偏差纳入 G1 判定；修正表机制兜底 |

回滚单位：每阶段独立 Change；P2 回退 = 回全量图集路径（保留至 P4 验收后）。

## 7. 完成定义

仅当以下全部满足才可报告达成：

1. G1–G4 判定全部通过；
2. M0 报告在案且 C1–C5 全部有数字；
3. 战斗模拟 checksum/回放与基线逐位一致（本计划全程零逻辑改动，此项为
   持续门禁）；
4. 双路径 shadow compare 通过记录在案；
5. `BattleRuntimeSelfCheck` 与相关 EditMode 通过；
6. 各阶段 Change Record 状态链完整。

只完成拆册或只完成档位解析不得宣称达成；只能报告阶段性状态。

## 8. 开放决策清单（需用户拍板）

| 编号 | 决策 | 影响 |
|---|---|---|
| D1 | 是否批准 M0 测量统计执行 | 一切数字的前提 |
| D2 | 20 人最坏配置的预算数字与准入策略（拒进 vs 降档优先级） | G3 门禁行为 |
| D3 | ASTC 画质验收的判定人与判定环境（目标放大倍率/机型档位） | G4、P4 启动 |
| D4 | 是否做半分辨率/更高压缩比档位 | P4 范围 |
| D5 | 去重投资：M0 去重率达到多少才值得立专项 | 支柱一范围 |

## 9. 明确不做的事

1. 运行时动态图集（增删重排）；
2. 按"闲置时间"逐张回收（此前讨论结论：GPU 内存按整页分配，逐张回收
   省不出显存；且时间判据破坏可复现性）；
3. 运行时压缩/解压；
4. 战斗中途换册或切档位；
5. 触碰逻辑帧率、pass、RNG、checksum 语义；
6. 未过 A/B 验收即上线任何有损变体。

## 10. 恢复方式

本文档保持 `DOCUMENTED / IMPLEMENTATION_NOT_STARTED / USER_HOLD`。批准启动时：

1. 先读 `docs/ai/CURRENT-AUTHORITY.md` 与当前 `AGENTS.md`；
2. 从 M0 开始，按第 8 节决策清单逐项确认；
3. 立项时重新扫描代码现状，不假设本文引用的文件行号仍有效；
4. 若本文被重命名/移动/拆分，须同步更新姊妹文档与本节路径。
