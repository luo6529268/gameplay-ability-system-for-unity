---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Docs\\central-battle-render-system-plan.md"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\BattleCentralRenderTypes.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\BattleDynamicMeshBackend.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\BattleCentralRenderSystem.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\BattleRenderFeature.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\Editor\\BattleRenderFeatureInstaller.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Simulation\\SimulationTickDriver.cs"
timestamp: "2026-07-20T22:08:51.288Z"
---

--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Docs\central-battle-render-system-plan.md ---
# 集中式战斗渲染系统方案

## 2026-07-21 B2C Extended checksum、当前 world 查询与 P1-P3 状态

- **代码已实施 / fresh self-check 已通过 / 最终架构复审待补**：`Authority400` 继续冻结为 `ntsd-battle-trace-v3`，direct parity capture 仍严格拒绝非 `Authority400/400`；`MobileExtended` / `DesktopExtended` 通过通用 checksum API 生成独立 `ntsd-unity-extended-battle-checksum-v1`，旧 `LastFrameSnapshot` 仍只表示 Authority v3。
- Extended metadata 覆盖 profile、logical capacity、claimed/object count 与 tick；slot 域覆盖 slot、claimed、generation、stable ID、current DAT OID、active entity runtime 及已物化但未 claimed 的 raw runtime。读取未物化槽不会创建分页。
- ARest/VRest 使用按 victim/attacker 稳定排序的稀疏投影，不构造 `capacity²` 矩阵；claimed entity 若未绑定当前 world 的 rest store 或 victim slot 不一致，capture 会拒绝生成 checksum。
- focused self-check 覆盖 Extended 的 Mobile `1050` / slot `1049`、Desktop `512 -> 768` / slot `700`、高槽 ARest/VRest、raw runtime、generation/stable-ID reuse、profile separation、稀疏 VRest 与 non-mutating repeat capture；同时覆盖 AI Loose Quadtree 查询与即时 weapon/body current-world 查询的结果/回退契约。最新 full self-check `2026-07-21 00:48:06` **PASS**；`dotnet build` **0 errors / 42 existing warnings**。
- 即时 body/weapon 查询已在显式 `LooseQuadtree` 后端下使用当前 world 实体的空间查询，AI 输入快照已使用 generation-aware Loose Quadtree 查询；索引/几何/映射异常均回退 brute，生产默认仍为 `BruteForce`。
- **P1 排序止血已完成代码层收口**：活跃实体按 `(ZInt, runtime slot)` 排序后分配 dense presentation rank；四个短期子序为 `Shadow=0`、`Entity=1`、`Overlay=2`（仅为 P3 预留，当前没有生产消费者）和 `HitRecord=3`。Shadow、Entity、spark 及其 `SortingGroup` 均统一为 Unity `Object` sorting layer，因此排序层不会先于 compact order 打断实体间交错。旧的 `logicalZ * 4096 + runtimeSlot * 4` 映射已移除。
- **P1 容量边界**：旧 `SpriteRenderer` 后端明确 guard 为最多 `8192` 个 materialized active entities；`8193` 会清晰抛错。移动端 `1000` active 预算在此范围内；`DesktopExtended` 在中央渲染后端完成前仍有这个临时表现上限，不等同于 runtime slot 容量上限。
- **P1 自动验证**：真实双实体四 renderer 的 `ForceRefresh` 检查验证 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5`，并覆盖 generation/高 slot 与 sorting layer/order。fresh 链为 source `2026-07-21 03:00:45` < Unity DLL `03:05:59` < full `BattleRuntimeSelfCheck` `03:07:05` **PASS**；`dotnet build Assembly-CSharp.csproj --no-restore -v:q` 为 **0 errors / 42 existing warnings**；最终 architect review 为 **PASS / no blocker**。
- **P2 immutable Catalog 已完成代码层收口**：`BattleSpriteCatalog` 的唯一 key 为 `(LF2Entity.ResolveCurrentDataObjectId(entity), effectivePic)`；不可变 entry 保存 source sheet、共享 `Texture2D`、Unity bottom-left 像素 rect、归一化 UV、宽高 metrics、pivot 和兼容旧 `SpriteRenderer` 的 legacy `Sprite`。正式 prewarm 使用 invocation-local staging 与 generation/disposed gate，只有本轮所有 sheet 成功且仍为当前 generation 时，才将 configs、`MergedSprites` 与 catalog 原子 publish；失败、过期结果和 teardown 均清理本轮资源。
- **P2 图片索引与生命周期契约**：partial BMP 严格按声明的 row/col 和 `localPic` 建立稀疏 rect，保留未声明图片的 holes；normal/swapped 网格仅在完整匹配时择优，并已覆盖 weapon6、weapon3 等生产矩阵。renderer 对 catalog 持有引用计数屏障，旧 catalog 只有在零引用后才退役，避免异步替换期间释放仍在显示的共享 texture/sprite。
- **P2 生产消费者已迁移**：display、collision、anchor、SpecialAttack point-center 与 shadow metrics 在战斗期不再读取 `Sprite.rect`；`pic=999`、缺 key、current DAT identity 切换和 pool reuse 均会隐藏并清除旧 sprite/catalog 引用。`MergedSprites` 仅保留兼容和预览用途，不再定义战斗期 metrics 真值。
- **P2 自动验证**：focused/full self-check 覆盖双文件边界、normal/swapped row/column、partial holes、rect/UV/pivot/shared texture、current identity replacement、missing/`999`、pool reuse、原子 publish、stale/teardown cleanup、renderer refcount retirement 及全部 metrics 消费者。fresh 链为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full `BattleRuntimeSelfCheck` `04:18:04` **PASS**；fresh dotnet build 为 **0 errors**。不同的自动生成 `.csproj` 刷新视图分别显示 18 或 42 条既有 warnings，因此不把 warning 数量冻结为 P2 契约。最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。
- **P3 shadow-build 已完成代码层收口**：渲染模式明确为默认 `LegacyOnly` 与诊断用 `CentralShadowBuild`；`CentralOnly` 在 P4 后端落地前明确拒绝。每个逻辑 tick 生成 value-only immutable snapshot/commands，按 `(ZInt, runtime slot)` 为每个实体稳定展开 `Shadow -> Entity -> Overlay -> HitRecord`。Overlay 当前标记为 `AuthorityExpectedButLegacyMissing` 诊断，不宣称 P3 已与 legacy overlay 等价。
- **P3 发布与真实 legacy probe**：snapshot/commands 使用 double buffer、几何增长容量和 atomic publish；persistent scratch 保证 steady `RenderDispatch` self-check 为 zero allocation。legacy probe 直接采样真实 renderer 的 sprite、texture、material instance、rect、pivot、position、flip 与 sorting；HitRecord 在 legacy advance 前采样，避免把推进后的 spark 状态错配到当前 tick。
- **P3 catch-up 与 spark 契约**：同一渲染帧追赶多个逻辑 tick 时，无法对中间 tick 取得实际 legacy renderer 状态，因此显式发布 `Incomplete`，记录 incomplete count、first tick 与 last tick；仅最后可观测 tick 进入完整 probe，不宣称所有逻辑 tick 均已实际 legacy parity verified。zero-hit 仍通过 `SparkRenderer.RenderAll` finalize；正式 production pool 路径覆盖 nonzero spark atlas cells、每 tick 只 age 一次，以及 `OnDisable`/`OnDestroy` 归还池。
- **P3 隔离与验证**：P3 snapshot/command/diagnostic 不进入战斗 checksum，也不反写 runtime 真值。fresh 验证链为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full `BattleRuntimeSelfCheck` `05:40:16` **PASS**；dotnet build **0 errors / 18 existing warnings**（root 当前视图）；最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。
- **验收边界**：本轮未执行 Play Mode、真实异步 BMP stress、真实 SPARK BMP/设备验证或性能验收。因此 P1-P3 是“代码、编译/self-check、静态复核完成”，但 P1-P3 的 Play/真实资源/性能门槛尚未全部达成；P3 尤其不能把 catch-up 的 `Incomplete` 中间 tick 写成逐 tick 已验证。未来异步 command consumer 仍须遵守 catalog lease/generation 契约。P4-P7 尚未完成；T8 已排除，不计入本计划完成条件。

本节是当前状态；下方早期阶段中“Extended Driver checksum 跳过/为空”或“Extended schema 尚未实施”的文字仅保留为当时历史边界，不再代表当前实现。

## BATTLE-RENDER-PLAN1 状态

- **状态**：方案已确认；R1-R2C-4、B0、B1-B1.3、B2A、B2B、B2C 与 **P1-P3** 已完成本轮代码层实施；P4-P7 尚未实施。
- **代码状态**：独立 `BruteForce` / `LooseQuadtree` 正式 collision broadphase 后端已具备 generation-aware 增量同步；默认仍为 `BruteForce`。除 fixed-tick candidate collect 外，B2C 已接入即时 weapon/body current-world query 与 AI 输入快照查询；二者均保留失败回退 brute。
- **验证状态**：B2B 的历史 fresh 证据为 `2026-07-20 22:47:04` full `BattleRuntimeSelfCheck` **PASS** 与 architect final review **PASS / no blocker**。B2C 与 P1 的分项证据为 source `2026-07-21 03:00:45` < DLL `03:05:59` < full self-check `03:07:05` **PASS**、dotnet **0 errors**、最终 architect **PASS / no blocker**。P2 的独立 fresh 证据为 source `04:16:00` < DLL `04:17:06` < full self-check `04:18:04` **PASS**、dotnet **0 errors**、最终 architect **PASS / no blocker**、最终 code review **no P0-P2**。P3 的独立 fresh 证据为 source `05:38:38` < DLL `05:39:29` < full self-check `05:40:16` **PASS**、dotnet **0 errors / 18 existing warnings**、最终 architect **PASS / no blocker**、最终 code review **no P0-P2**。各阶段复核只覆盖对应分项；P1-P3 均未完成 Play Mode/性能验收，P3 还未完成真实 SPARK BMP/设备验证。
- **容量说明**：`400` 是 `Authority400` 兼容模式的 C# 权威槽位边界，不是所有 Unity 运行模式的全局容量上限。权威 `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Common\NtsdConstants.cs` 中的 `NtsdConstants.MaxObjects` 定义 `MaxObjects = 400`，`BattleCore\Simulation\SimulationWorld.cs:28-32` 据此创建 `Objects[400]`、`VRest[400,400]` 和 `ARest[400]`；Unity `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:39-44` 以 `MaxRuntimeSlots = 400` 镜像该契约。扩展模式的 active entity 容量与 render command 容量分开管理；每个实体可产生 `Shadow`、`Entity`、`Overlay`、`HitRecord` 等多个命令，Mesh 仍须按实际命令峰值预分配并分 chunk。
- **平台 Profile 说明**：生产解析优先级固定为“命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认值”；平台宏只提供默认 Profile，不进入战斗逻辑、最小堆、Loose Quadtree、VRest 或命中规则。设备能力降级只改变图集、纹理和渲染后端，不得改变已选 Profile 的战斗容量或结果。
- **实施边界**：fixed-tick formal collect 仍在 B2B 边界对当帧 participant 做 batch synchronize，不把 registry mutation 直接写入 collision 索引。B2C 的即时 weapon/body 与 AI 查询各自从当前 world/snapshot 构建查询视图，generation、几何或映射无法验证时回退 brute；它们不改变 fixed-tick pair 的 authority ordinal、RNG 或 candidate 时序。正式 collect 结果仍按 canonical runtime-slot pair 合并、去重，再按原 authority ordinal 双向派发；任何无法证明完整性的情况均 reset 增量索引、整 tick 回退 brute-force，并原子恢复 RNG/candidate 状态。

### 2026-07-20 R1 第一批实施记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| Profile resolver | **已实施 / 已验证** | 支持显式覆盖 > 配置值 > 平台默认；平台默认由 Unity 条件编译符号选择。Editor/其他平台回落 `Authority400`，Android Player 为 `MobileExtended`，Standalone Player 为 `DesktopExtended` |
| `Authority400` 最低空闲槽分配 | **已实施 / 已验证** | 以 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused` 保留 roster、stage、dynamic band 语义；支持按索引移除、释放回收和最低槽确定性分配 |
| 正式 runtime 接线 | **兼容模式已接入** | `SimulationWorld` 仍显式固定为 `Authority400`，本批不改变 400-slot 行为边界，也不自动启用平台扩展模式 |
| 扩展容量与空间索引 | **R1 历史边界，后续已替代** | R1 当时仅有独立分页 `RuntimeSlotTable` 与 generation handle；`MobileExtended`、`DesktopExtended` 生产接线、桌面动态增长、1000 active admission、AI 与 Loose Quadtree 已由后续阶段实施，当前状态以本文件顶部 B2C 节为准 |

fresh 验证：相关源码时间 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` 结果 `12:05:07` **PASS**；分配器另以 **100,000 次随机 claim/release/allocate 操作**与朴素线性扫描模型逐步对照，结果 **PASS**；架构复核 **PASS**。这些证据只关闭 R1 第一批，不代表 Play Mode、扩展容量、四叉树或集中式渲染已经验收。

### 2026-07-20 R2A 分页槽表与 generation 句柄基础记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `RuntimeSlotTable` 分页存储 | **基础设施已实施 / 已验证** | 固定 `PageSize = 256`，按首次访问惰性物化页面；`Authority400` 逻辑容量为 400，`MobileExtended` 设计容量为 1050，最后一页超出各自逻辑尾部的地址均被 guard 拒绝 |
| raw runtime / raw rest 存储 | **基础设施已实施 / 已验证** | 每个 slot 持有独立 `NTSDEntityRuntime` 与 `LF2ItrRestTracker.StateSnapshot` 存储；raw 状态与实体 claim 生命周期分开，不因只读查询隐式占用槽位 |
| 占用计数 | **基础设施已实施 / 已验证** | `ClaimedCount` 由 allocator 契约维护，claim、release 与 reset 后均由 focused self-check 校验 |
| `RuntimeEntityHandle` | **基础设施已实施 / 已验证** | 句柄由 `(slot, generation)` 构成；release、同槽 reuse 与 reset 都推进 generation，使旧句柄无法再 resolve 到新占用者 |
| 生产 runtime 接线 | **未实施 / 未启用** | `SimulationWorld` 仍使用现有 `Authority400` registry/raw arrays，并未切换到 `RuntimeSlotTable`；本批不改变战斗结果或现有 400-slot parity schema |

R2A fresh 验证：相关源码时间 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` 结果 `12:36:53` **PASS**；架构复核 **PASS**。这些证据只验证分页地址、惰性物化、独立 raw 存储、`ClaimedCount` 与 generation 失效契约；不代表 `Extended` 已启用，也不覆盖桌面动态增长、移动端 1000 admission、AI 迁移、Loose Quadtree 或 VRest 改造。

### 2026-07-20 R2B `Authority400` 生产 registry 迁移记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 单一槽位存储后端 | **已实施 / 已验证** | 生产 `SimulationWorld` 的 `_runtimeSlotUsed`、`_rawRuntimeSlots`、`_rawRestSlots` 已由单一 `RuntimeSlotTable` 替代；旧字段检索为 0，registry 不再维护并行槽位真值 |
| 当前占用者查询 | **已实施 / 已验证** | `FindEntityByRuntimeSlotIncludingDormant` 与 current-pass 查询直接通过 slot 地址 O(1) 解析当前 occupant；长期引用仍必须使用带 generation 的 `RuntimeEntityHandle` |
| pass 遍历时序 | **已实施 / 已验证** | 保留 live ascending slot scan：游标以上新生实体可进入本 pass，复用游标以下低槽的实体等待下一 pass，保持既有 high-newborn / low-reuse 时序 |
| release 身份保护 | **已实施 / 已验证** | release 必须同时匹配 slot 与 `expectedEntity`/当前 occupant；过期实体不能释放已被另一实体复用的槽 |
| raw rest 语义 | **已实施 / 已验证** | stage spawn 继续恢复并消费复用槽 raw rest；ordinary spawn 继续按既有语义重置，不把 R2B 存储迁移扩大成 VRest/ARest 规则变更 |
| 对外可观察契约 | **保持不变 / 已验证** | `ObjectCount`、对象 buckets、`SceneQueryHit` 的 runtime-slot 地址语义保持不变；生产 Profile 仍固定为 `Authority400` |

R2B fresh 验证：相关生产源码时间 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` 结果 `12:57:02` **PASS**；fresh `dotnet build` 为 **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索为 **0**。这些证据只关闭 `Authority400` 的生产 registry 存储迁移，不代表 `Extended`、移动端 1000 admission、桌面分页增长、AI、Loose Quadtree、VRest 解耦或集中式渲染已启用。

### 2026-07-20 R2C allocator/table 单调增长记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `RuntimeSlotAllocator.GrowTo` | **基础设施已实施 / 已验证** | 只允许容量单调增加；增长后保留三段边界、dynamic segment 的 indexed binary min-heap、`nextUnused`、已占用槽与 `ClaimedCount`，并继续优先复用增长前的最低空洞，再使用新开放地址 |
| `RuntimeSlotTable.GrowTo` | **基础设施已实施 / 已验证** | 增长时扩展页引用数组但不主动物化新页；保留既有 page object、occupant、generation handle、raw runtime、raw rest 与 claim 状态，新页仍在首次访问时惰性物化 |
| 非增长调用 | **已验证** | 目标容量等于当前容量时成功 no-op；缩容请求返回拒绝，且容量、claims、页面、句柄和 raw 状态保持不变 |
| 移动端地址契约 | **设计边界已修正 / focused 已验证** | `1000 active` 是 admission 预算，不是逻辑地址尾值；保留 `0..49` 后，1000 个动态槽为 `50..1049`，因此逻辑地址容量是 `1050`。`PageSize=256` 时物理数组需要 5 页，但物理尾部 `1050..1279` 必须不可寻址、不可 claim、不可创建 raw runtime |
| 生产接线 | **R2C 时未实施；已由 R2C-4 后续接入** | `SimulationWorld` 在 R2C 时仍固定 `Authority400`；生产 Profile、Mobile total admission 与 Desktop 自动增长已由 R2C-4 接入 |

R2C fresh 验证：相关源码时间 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` 结果 `13:25:34` **PASS**；fresh `dotnet build` 为 **0 errors**；架构复核 **PASS**。这些证据只证明 allocator/table 可在保持既有状态与最低槽语义的前提下单调增长，并验证移动端 `1050` 逻辑地址及物理尾部 guard；不代表 Extended Profile、生产增长、移动端 admission、AI、Loose Quadtree 或集中式渲染已经启用。

### 2026-07-20 R2C-3A `SimulationWorld` 实例容量读取记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| world 容量真值 | **已实施 / 已验证** | `SimulationWorld.RuntimeSlotCapacity` 读取当前 `_runtimeSlots.LogicalCapacity`；registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量循环不再假定固定 400 |
| 默认兼容模式 | **保持不变 / 已验证** | 默认 `SimulationWorld()` 仍创建 `Authority400/400`；现有生产 Driver、400-slot parity 与默认 self-check 不会自动进入扩展模式 |
| focused 扩展契约 | **内部测试入口已实施 / 已验证** | internal 构造以 `DesktopExtended/512` 创建 focused world；slot `511` 可注册、查询并进入 AI 目标扫描，slot `512` 被拒绝，reset 后高槽状态被清理 |
| parity schema | **保持固定 / 已验证** | `BattleParitySnapshot` 继续显式使用 `AuthorityRuntimeSlotCapacity = 400`，没有把历史 400-slot certificate 静默扩展为新 schema |
| 生产与外部边界 | **R2C-3A 时 Profile 未实施；现已由 R2C-4 接入** | `MobileExtended` / `DesktopExtended` Profile 后续已接入生产 Driver；`LF2SpecialAttack` / `LF2Entity` 的外部固定容量边界已在 R2C-3B 按 world capacity 处理 |

R2C-3A fresh 验证：相关源码时间约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` 结果 `13:54:22` **PASS**；fresh `dotnet build` 为 **0 errors / 42 warnings**。这些证据证明默认 400 行为未变，并证明显式 512-slot world 的代码层容量契约可运行；扩展 Profile 当时仍未接入生产 Driver，外部 special/transition 固定边界随后由 R2C-3B 关闭。

### 2026-07-20 R2C-3B 外部容量边界与 parity guard 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| special attack 高槽 holder | **已实施 / 已验证** | `LF2SpecialAttack` 不再用固定 400 拒绝 holder slot；在已绑定 world 时按 `RuntimeSlotCapacity` 验证并解析扩展高槽 holder |
| Karasu 高槽扫描 | **已实施 / 已验证** | Karasu oid209 替换扫描使用当前 world 容量，`DesktopExtended/512` 中的高槽目标不再被 `0..399` 截断 |
| transition effect 容量计数 | **已实施 / 已验证** | `LF2Entity` transition effect 的可用动态槽计数使用当前 world 的 dynamic 起点到逻辑容量尾部，不再固定扫描 `50..399` |
| parity capture guard | **已实施 / 已验证** | 历史 parity capture 必须同时满足 Profile 为 `Authority400` 且逻辑容量为 400；`DesktopExtended/512` 与 `DesktopExtended/400` 均明确拒绝，不能仅凭容量为 400 冒充 authority certificate |
| 生产接线 | **R2C-3B 时未实施；已由 R2C-4 后续接入** | 默认生产 Driver 的 Profile、admission 与桌面自动增长后续已接入；本批仍未实现扩展 parity schema |

R2C-3B fresh 验证：相关源码时间 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` 结果 `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**，warnings 为既有告警。该证据关闭 3A 后遗留的 special attack / transition effect 固定容量边界，并建立严格的 authority parity capture guard；不代表生产 Driver/Profile 接线、admission、桌面自动增长、Loose Quadtree、VRest 或集中式渲染已完成。

### 2026-07-20 R2C-4 生产 Profile 激活记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 生产 Profile 解析优先级 | **已实施 / 已验证** | 命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > Unity 平台宏默认；配置值不再被 `Awake`/重建路径静默覆盖 |
| 默认容量 | **已实施 / 已验证** | `Authority400` 逻辑容量 `400`；`MobileExtended` 逻辑容量 `1050`，`TOTAL active admission = 1000`（跨 roster/stage/dynamic 全部槽区）；`DesktopExtended` 默认初始逻辑容量 `512`，按 `PageSize=256` 规范化并支持自动增长 |
| Driver 生命周期 | **已实施 / 已验证** | `SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析与 world 创建路径；直接 `BattleTestBootstrap` 在实体注册前重新协调晚到的 GameConfig |
| Desktop 增长 | **已实施 / 已验证** | 自动增长保留最低空洞分配顺序，并同步扩展 AI snapshot 容量，避免 world 与 AI 视图分叉 |
| Extended checksum/parity | **历史边界，已由 B2C 替代** | 当时 Extended Driver checksum 输出跳过/为空；当前 B2C 已提供独立 Extended checksum，direct parity capture 仍只接受 `Authority400 + 400` |
| 后续阶段 | **R2C-4 历史边界，后续已替代** | B0 shadow 随后落地；B1-B2B 后续完成 VRest 解耦、增量更新与 formal backend，B2C 已实施即时 weapon/body、AI 查询和 Extended checksum。集中式渲染仍是后续计划，默认 broadphase 仍为 `BruteForce` |

R2C-4 fresh 验证：相关源码时间 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` 结果 `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 42 existing warnings**；architect final review **PASS**。

### 2026-07-20 B0 shadow Loose Quadtree 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 纯数据空间树 | **已实施 / 已验证** | X/Z half-open 归属；`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 8`；不依赖 Transform 或 Unity Physics |
| 构建策略 | **shadow 已实施 / 正式切换未实施** | 每次 collision collect 全量重建；尚未采用增量更新，也未替换正式 brute-force broadphase |
| 诊断边界 | **已实施 / 默认关闭** | 对比 brute AABB pair、tree pair 与正式 accepted subset；诊断关闭时不承担生产结果责任，不据此宣称性能提升 |
| 权威流程保护 | **保持不变 / 已验证** | 正式 `i/j` 遍历、VRest、RNG、candidate 收集/截断/消费顺序继续使用原权威流，shadow 结果不写回战斗真值 |
| 后续接入 | **B0 历史边界，后续已替代** | 即时 weapon/body 与 AI 查询已由 B2C 接入；VRest 解耦、增量更新与 formal broadphase 已由 B1-B2B 接入。生产默认仍为 `BruteForce` |

B0 fresh 验证：相关源码时间不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` 结果 `16:15:43` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。这些证据只证明 shadow 数据结构、pair 诊断和权威流隔离正确，不证明生产 broadphase 已切换或已有性能收益。

### 2026-07-20 B1 `RuntimeRestStore` 基础记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| ARest 存储 | **纯数据基础已实施 / 已验证** | 分页、惰性物化；逻辑容量外地址拒绝，不因只读访问隐式创建页 |
| VRest 存储 | **纯数据基础已实施 / 已验证** | 定向稀疏 `VRest[victim, attacker]`；只保存正值，写零即移除，不把双向 pair 合并 |
| 槽位清理 | **已实施 / 已验证** | `ResetSlot(slot)` 同时清该槽 ARest、VRest victim row 与 attacker column，防止槽复用继承旧 rest |
| 生命周期与扩容 | **已实施 / 已验证** | 支持 `GrowTo`、全局 reset、排序后的 diagnostics/snapshot，以及 snapshot restore；增长保持既有稀疏状态 |
| 差分验证 | **已验证** | 2,000 次随机操作与 dense reference model 逐步 differential，对定向读写、清零移除、slot reset、grow/reset 与 snapshot restore 进行比较 |
| 生产接线 | **B1 时未实施；已由 B1.2 后续接入** | facade lifecycle 与 parity fallback 已由 B1.2 接入；collision pair tick 解耦与正式 quadtree switch 仍 pending |

B1 fresh 验证：相关源码时间 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` 结果 `16:37:13` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**；architect final review **PASS**。这些证据只验证纯数据 store 契约，不代表生产 VRest/ARest owner 已迁移，也不代表 pair tick 已与 collision broadphase 解耦。

### 2026-07-20 B1.1 optional facade 与 victim-row lease 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| optional facade | **已实施 / 已验证 / 未 production-bound** | `LF2ItrRestTracker` 可选择绑定 `RuntimeRestStore`，未绑定时保留既有实现；当前生产 world 尚未启用该绑定 |
| victim-row ownership | **已实施 / 已验证** | facade 获取 exclusive victim-row lease；同一 victim row 不允许多个 facade 并发拥有，释放 lease 后才允许后续 owner 接管 |
| 语义边界 | **保持不变** | facade 只适配现有 ARest/VRest 定向语义，不改变 store 的 positive-only、zero-removal、row/column reset 或排序 snapshot 契约 |
| state import 原子性 | **已修复 / 已验证** | architect 首轮发现 `ReplaceVictimState` 在 mixed-invalid attacker 输入下可能先写入部分合法项再失败；现已先完整预验证，之后原子替换，失败时原状态不变 |
| failed-import 回归 | **已验证** | direct `ReplaceVictimState` 与 facade `Bind` 两条路径均覆盖 mixed-invalid 输入，并断言失败前后的 ARest/VRest 状态完全一致 |
| 非阻塞补强 | **可后续补充** | invalid bound `RestoreState` 的单独断言尚可增加；该路径复用已验证的 atomic replace 入口，不构成当前 blocker |
| 下一批生产接线 | **B1.1 时未实施；已由 B1.2 后续接入** | registration、release、world reset 已按 ordinary 清理与 `StageSpawnAt` retention 分流接入 |

B1.1 修正后 fresh 验证：复跑 `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 18 existing warnings**；相关源码时间 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` 结果 `17:39:07` **PASS**；architect final review **PASS / no blocker**。该批证据本身不代表 production-bound；后续绑定由 B1.2 单独实现和验证。

### 2026-07-20 B1.2 production lifecycle binding 记录（已验证 / architect final PASS）

| 项目 | 当前状态 | 证据 |
|---|---|---|
| store ownership | **已实施 / self-check verified** | `SimulationWorld` 独占 `RuntimeRestStore`，store 生命周期随 world 创建、reset 与 grow 同步 |
| ordinary claim | **已实施 / self-check verified** | claim 成功后先 `ResetSlot(slot)`，再以 `Bind(..., importLegacyState: false)` 绑定 tracker |
| release | **第三个 blocker 已修 / self-check verified** | `ReleaseRuntimeSlot` 返回 bool 并事务传播到全部注销/待销毁调用链；错槽拒绝时不继续半注销，正常 release 保留 store 并解绑 |
| `StageSpawnAt` | **blocker 已修 / self-check verified** | rejected bind 走共享完整 pool 回收；真实 pool counts、lease、slot 与 `KillStats` 均有回归断言 |
| public `Unregister` 故障回归 | **已验证** | 通过公开 `Unregister` 触发错槽 release 拒绝，断言完整 registration context（bucket/slot/lease/store/entity）保持不变 |
| 单一 rest 真值 | **已实施 / self-check verified** | 删除 `RuntimeSlotTable.RawRest`；parity fallback 直接读取 `RuntimeRestStore` |
| world reset/grow | **已实施 / self-check verified** | world reset/grow 与 store 同步 |
| 尚未关闭 | **未实施 / 未验证** | collision pair tick 解耦仍未实施；本批不切换正式 broadphase，且与 T8 无关 |

B1.2 初版证据：`dotnet build` **0 errors**；源码 `2026-07-20 18:11:41` < Unity DLL `18:12:23` < full self-check `18:13:00` **PASS**。architect final review 随后发现上述 2 个 blocker；该证据现只说明初版可编译且旧断言通过，**不构成 B1.2 完成/验证证据**。

B1.2 第一轮 blocker 修复证据：`dotnet build` **0 errors**；源码 `18:21:20` < Unity DLL `18:21:58` < self-check `18:22:59` **PASS**。architect 第二轮随后发现 release 拒绝未向 `Unregister` 调用链传播、可能半注销；因此该 PASS 同样是**非完成证据**。

B1.2 最终 fresh 证据：`dotnet build` **0 errors**；相关源码 `2026-07-20 18:31:25` < Unity DLL `18:33:58` < full self-check `18:34:54` **PASS**。公开 `Unregister` 故障矩阵验证完整注册上下文不变；architect final review **PASS / no blocker**。

### 2026-07-20 B1.3 collision pair VRest tick 解耦记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| pass 顺序 | **已实施 / self-check verified** | 单 tick 固定为 `CaptureSnapshots -> sparse Tick -> Collect`；VRest 递减在候选收集前独立完成 |
| eligible row | **blocker 已修 / self-check verified** | 直接遍历 registered bucket items，筛选 `active + CharData` victim；inactive row 冻结，不扫描 `RuntimeSlotCapacity` |
| pair 内副作用 | **已移除 / self-check verified** | `BruteForceSceneQuery` 不再在 pair 枚举内部 tick VRest；early return、无 pair 与候选截断都不能漏 tick 或重复 tick |
| store 热路径 | **已实施 / self-check verified** | `RuntimeRestStore` 维护 active-positive-row/stamp，scratch 随容量预扩；eligibility 无 capacity scan、无 snapshot 分配 |
| Desktop 稀疏高槽 | **已验证** | 高逻辑容量 world 仅两个 registered eligible items 时访问计数严格为 `visited=2` |
| 验证矩阵 | **已覆盖** | dense differential、registration/release lifecycle、inactive freeze、early-return/no-pair、diagnostics 与 parity fallback 均进入 full self-check |
| broadphase | **未切换** | 正式候选仍由原 brute-force collect 产生；B1.3 不代表 Loose Quadtree 已接管生产 broadphase |

B1.3 初版证据：`dotnet build` **0 errors**；源码 `19:09:44` < DLL `19:10:34` < self-check `19:11:13` **PASS**。architect 随后发现 eligibility 仍为 O(`RuntimeSlotCapacity`) 全扫，该证据因此是**非完成证据**。

B1.3 最终 fresh 证据：`dotnet build` **0 errors**；相关源码 `2026-07-20 19:19:14` < Unity DLL `19:19:47` < full self-check `19:22:50` **PASS**；Desktop sparse high-slot `visited=2`；architect final review **PASS / no blocker**。

### 2026-07-20 B2A formal Loose Quadtree broadphase 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 后端选择 | **已实施 / self-check verified** | 独立 `CollisionBroadphaseBackend` 支持 `BruteForce` 与 `LooseQuadtree`；解析优先级为命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`，平台宏不进入战斗分支 |
| 接管边界 | **B2A 历史边界，已由 B2C 部分替代** | B2A 仅替换 fixed-tick `CollectCollisionCandidates`；B2C 随后接入即时 weapon/body current-world query，失败仍走 brute fallback |
| participant/pair 顺序 | **已实施 / self-check verified** | 收集与 brute outer loop 相同的 eligible participant 并保留 authority ordinal；tree/fallback pair 使用 `(minSlot,maxSlot)` canonical key 全局排序去重，随后按 authority ordinal 以 `a->b`、`b->a` 顺序派发 |
| 无效 AABB | **保守处理 / self-check verified** | 缺失或无效 AABB 的 participant 不被遗漏，而是与全部其他 eligible participant 组成 fallback-all pair；extra formal pair 仍由 narrow phase 过滤 |
| 整 tick 回退 | **已实施 / self-check verified** | runtime slot 缺失/重复/越界、slot-to-entity mapping 不一致、query index/entry count 非法、rebuild/query 异常，或 diagnostics 发现缺少 brute coverage 时，丢弃 formal 部分结果并整 tick 重跑原 brute-force |
| 原子性与确定性 | **已实施 / self-check verified** | formal 失败时恢复进入前 RNG state/call count，清空 candidate carrier/count/distance/cache 后再 brute collect；candidate 20 上限、nearest/type ties、RNG 与消费顺序保持原权威路径 |
| diagnostics | **默认关闭 / self-check verified** | 开启时比较 brute canonical set 与 formal set；缺 pair 强制整 tick brute fallback，extra pair 允许并交 narrow phase；诊断不改变 RNG 或战斗状态 |
| 后续阶段 | **B2A 时未实施；已由 B2B 后续接入** | B2A 当时仍为每 fixed tick full rebuild；generation-aware 增量迁移/更新现已由下节 B2B 接入，生产默认仍未切为 Loose Quadtree |

B2A fresh 证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` **0 errors**；相关源码最新时间 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` 结果 `22:19:28` **PASS**。architect final review **PASS / no blocker**；本批未执行 Play Mode，不能据此扩大为完整场景验收。T8 默认 `stage.dat` 部署继续暂缓。

### 2026-07-20 B2B generation-aware 增量 Loose Quadtree 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 同步边界 | **已实施 / self-check verified** | formal backend 在每次 fixed-tick collision collect 边界批量同步当帧 eligible participant；注册、注销和移动本身不直接改树，避免把 registry mutation 时序引入权威 pass |
| 稳定身份 | **已实施 / self-check verified** | 索引记录与查询结果使用 `(runtime slot, generation)` 的 `RuntimeEntityHandle`；同槽释放再复用时旧 generation 被移除，新 occupant 作为新 handle 插入，不会把旧空间记录解析到新实体 |
| 增量更新 | **已实施 / self-check verified** | 未移动实体保持原记录；AABB 改变但仍在当前节点 loose 容纳范围内时原位更新；越出 loose 范围时才从旧节点移除并重新插入。新增、销毁、invalid-AABB 转换和同槽复用均由同一 batch sync 收口 |
| root escape | **保守重建 / self-check verified** | 当前有效 AABB 超出既有 root 时执行一次全量 rebuild；正常的 loose 内移动与跨 loose 迁移不重建整棵树 |
| live query validation | **已实施 / self-check verified** | quadtree query 返回 handle，派发前必须由当前 `RuntimeSlotTable` generation 成功解析，并再次核对 slot、entity、participant ordinal 与 handle 映射 |
| 原子回退 | **已实施 / self-check verified** | sync/query/invariant/mapping 异常会 reset 增量索引并整 tick 重跑 brute-force；B2A 已有 RNG/candidate rollback 继续包住 formal collect，部分执行不能污染候选、RNG 或消费顺序 |
| world reset | **已实施 / self-check verified** | `SimulationWorld` registry reset 显式清理 formal spatial index，旧 match 的 node、record 与 handle 不会进入下一 world 生命周期 |
| 启用边界 | **B2B 历史边界，已由 B2C 部分替代** | 生产默认仍为 `BruteForce`；只有显式选择 `LooseQuadtree` 才使用 formal backend。B2C 已接入即时 weapon/body 与 AI 查询及 Extended checksum；集中式渲染仍不属于 B2B/B2C |

B2B fresh 证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` **0 errors**；相关源码最新时间 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` 结果 `22:47:04` **PASS**。architect final review **PASS / no blocker**；本批未执行 Play Mode，不能据此扩大为完整场景验收。T8 默认 `stage.dat` 部署继续暂缓。

## Runtime 容量与空间索引阶段决策

**状态：B1-B1.3、B2A 与 B2B 已完成代码层实施 / 编译 / full self-check / architect final review。** B2C 已实现 Extended checksum、AI Loose Quadtree 查询和即时 weapon/body current-world query，并有 `2026-07-21 00:48:06` full self-check PASS；B2C 本身尚无 fresh Architect PASS、Play Mode 或性能验收。生产默认 broadphase 仍是 `BruteForce`。

### RuntimeSlot 容量模式

- **`Authority400` 兼容模式**：保留 C# 的 400 runtime slot、既有特殊槽区和最低空闲槽分配语义，用于现有 self-check、parity 和逐帧对照。该模式的 400 是兼容边界，不代表 render command 上限。
- **移动端扩展模式**：逻辑地址容量为 `1050`，最后有效地址为 `1049`；`TOTAL active admission = 1000`，跨 roster/stage/dynamic 全部槽区计数，第 `1001` 个 active entity 必须确定性拒绝生成，不排队、不替换，也不由设备瞬时内存状态决定。拒绝结果必须进入可重放的结果/日志边界。
- **桌面扩展模式**：默认初始逻辑容量 `512`，按 `PageSize=256` 规范化为整页并在需要时自动增长；不设置玩法层面的 active entity 上限，但仍受明确的地址空间、内存、对象池、逻辑帧和 render command 技术预算约束，不能解释为物理上无限容量。
- 空闲槽使用**二叉最小堆 + `nextUnused`**：R1 第一批已在 `Authority400` 内按 `0..19`、`20..49`、`50..399` 三段实现 indexed binary min-heap；已释放槽进入最小堆，分配时优先取最小空闲槽，堆为空时使用并递增 `nextUnused`。R2A 以 256 槽/页建立惰性分页表并复用该 allocator，R2B-R2C-3B 依次接入槽表、增长、实例容量和外部边界，R2C-4 已将 Desktop 自动增长接入生产。增长前的最低空洞仍优先于新页地址，且 AI snapshot 与 world 容量同步扩展；所有分配、释放和分页增长继续保持最低槽确定性，不依赖 `Dictionary`/`HashSet` 枚举顺序。
- **分层位图**仅作为后续候选优化，不作为本阶段实现前提；若采用，必须保持与最小堆相同的最低槽和回放语义。

### 平台 Profile 与选择边界

**状态：resolver 与生产 Profile 激活已实施并通过 self-check / architect final PASS。** 平台差异通过统一 Profile/能力配置入口表达；不得在战斗 pass、opoint、碰撞、命中、对象生命周期或空间查询内部散布 `#if UNITY_ANDROID` / `#if UNITY_STANDALONE` 分支。Unity 官方条件编译符号仅用于选择平台默认值；`SystemInfo` 等运行时能力 API 留给后续渲染后端降级，不改变战斗 Profile 或逻辑结果。

运行模式固定为：

| Profile | 平台默认与用途 | RuntimeSlot / active 边界 |
|---|---|---|
| `Authority400` | `UNITY_EDITOR` 和未明确支持的平台默认；用于 C# 权威对拍、现有 self-check、历史 parity schema 与兼容诊断 | 固定 400 槽，保留权威特殊槽区和最低空闲槽语义 |
| `MobileExtended` | `UNITY_ANDROID && !UNITY_EDITOR` Player 默认 | 逻辑容量 1050；全部槽区合计最多 1000 active，第 1001 个发布尝试确定性拒绝 |
| `DesktopExtended` | `UNITY_STANDALONE && !UNITY_EDITOR` Player 默认 | 默认初始 512，按 256-slot 页规范化并自动增长；不设玩法层面的 active 上限，但受明确技术预算约束 |

宏边界必须按以下规则实现：

- `UNITY_EDITOR` 优先于当前 Build Target 宏。Editor 即使切到 Android Build Target，也不能仅因同时定义 `UNITY_ANDROID` 就自动进入移动端正式 Profile；Editor 平台默认保持 `Authority400`，测试或配置可显式覆盖为 `MobileExtended` / `DesktopExtended`。
- `UNITY_ANDROID && !UNITY_EDITOR` 只负责给 Android Player 选择 `MobileExtended` 默认值；`UNITY_STANDALONE && !UNITY_EDITOR` 只负责给桌面 Player 选择 `DesktopExtended` 默认值。
- 其他 Player 平台在完成单独设计和验收前默认 `Authority400`，不得根据相似平台经验自动套用 Android 或桌面扩展规则。
- 平台宏只允许出现在默认 Profile 选择和不可避免的平台专属 API 适配入口。核心 runtime 统一读取已解析的 Profile/预算，不直接读取平台宏。

配置解析优先级固定为：

```text
命令行显式覆盖
    > GameConfig.BattleRuntimeProfileName
    > 平台宏默认 Profile
```

- 命令行显式覆盖用于 self-check、parity、回放和 Editor A/B 验证，必须能强制选择 `Authority400`、`MobileExtended` 或 `DesktopExtended`。
- `GameConfig.BattleRuntimeProfileName` 是生产项目配置入口；`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用同一解析路径，直接 `BattleTestBootstrap` 在实体注册前协调晚到配置。
- 运行时设备能力检测发生在 Profile 解析之后。`SystemInfo.supports2DArrayTextures`、纹理尺寸/slice 上限、图形 API、格式支持和目标 GPU 验证结果只用于选择可用的资源与渲染后端。
- 推荐降级链为 `Texture2DArray + OrderedChunks` -> `多 Texture2D + OrderedChunks` -> `LegacySpriteBackend`；任何降级都必须保持原 painter 顺序和相同只读表现输入。
- 设备不支持 `Texture2DArray`、命中设备黑名单或内存预算不足时，不得把 `MobileExtended` 静默改成 `Authority400`，也不得降低 1000 active admission 边界来掩盖渲染预算不足；应通过分 chunk、后端降级、可诊断拒绝或明确启动失败处理。

所有 Profile 必须共用同一份二叉最小堆 + `nextUnused`、分页 slot、generation handle、Loose Quadtree、VRest/ARest、候选排序和 lifecycle 实现。平台可以改变容量、预分配、图集格式、chunk 数和渲染回退策略，但不能改变逻辑 tick、slot 决定性、pair 顺序、VRest 计时、opoint 生成顺序或战斗结果。

### 移动端 1000 active admission 边界

- `1000 active` 与 slot address 容量是两个独立数字：`RuntimeSlotTable.LogicalCapacity = 1050`，最后有效地址是 `1049`，其中 `0..19` 为 roster、`20..49` 为 stage、`50..1049` 为 dynamic 地址。active admission 的 1000 是**全部槽区合计预算**，不是只给 dynamic band 的 1000 个 active 名额；5 个 256-slot 物理页仅是存储实现，尾部 `1050..1279` 不属于逻辑地址空间。
- active 计数以**已发布且尚未完成注销的 runtime entity**为准：已注册的 active、dormant/merge shell 和 `pending-destroy` entity 都计入；尚未发布的 `pending-spawn`、未占用的 raw slot 以及已归还对象池且没有 runtime 注册的 shell 不计入。
- `pending-destroy` 在确定性注销边界完成前仍占用 active 预算和 runtime slot；不能因为已经标记销毁就提前释放容量。分配拒绝必须在发布前判断，不能先发布再回滚。
- 同一 tick 的释放与生成不依赖容器枚举顺序：在既定的 lifecycle mutation boundary 内，先按队列/slot 的确定顺序完成已到期注销，再按既定 producer/pass 顺序逐个进行 spawn admission 和发布；只有前一步已完成注销的 entity 才能为后一步释放容量。若生成发生在注销 boundary 之前，则按当时仍包含 `pending-destroy` 的计数判定并可确定性拒绝。
- 每次 spawn admission 成功后立即增加已发布计数；同一 boundary 后续 spawn 看到更新后的计数。移动端达到 1000 后，后续第 1001 个发布尝试稳定返回拒绝结果；Extended replay/checksum schema 尚未实现，当前 Extended Driver checksum 明确跳过/返回空值。

### X/Z Loose Quadtree Broadphase

**当前状态：B0 shadow 诊断、B2A formal backend 与 B2B generation-aware 增量同步均已实施，并通过 full self-check 与 architect final review；生产默认仍为 `BruteForce`。** `LooseQuadtree` 只有经显式命令行或 `GameConfig` 选择时才接管 formal backend；B2C 已随后接入即时 weapon/body query 与 AI 查询，此处旧“未迁移”结论已替代。

- 空间索引使用 X/Z 平面的 **Loose Quadtree**；逻辑实体、AI 范围查询和 itr/bdy 碰撞查询共享空间索引，但查询服务与候选规则分开，不能用 AI 范围结果替代碰撞候选。
- 实体中心点采用严格的**半开区间**归属（左/下含、右/上不含，边界规则全局一致），保证一个中心点只属于一个子节点。
- 实体 AABB 只有在完全被节点的 loose 范围容纳时才留在该节点；超出 loose 范围才迁移到父节点或重新选择的节点。
- 默认参数仅作为 profiling 基准，不能视为最终性能结论：`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 6..8`。目标设备和真实战斗分布 profiling 后再调整。
- 更新已采用 collect-boundary batch 增量策略：未移动实体保留原记录；AABB 改变但仍处于当前节点 loose 范围时原位更新；离开 loose 范围才迁移。生成、销毁、invalid AABB 和同槽 generation 复用在下一次 collision collect 同步，root escape 才触发全量重建；world reset 显式清空索引。
- broadphase 每 tick 先按 `RuntimeSlot` 升序遍历 active attacker；各 attacker 查询得到的候选先去重为 `(minSlot, maxSlot)` pair，再在全局按 `(minSlot, maxSlot)` 升序排序后交给现有 narrow phase。保留 C# 的 candidate 截断、距离/类型 tie 顺序和 pair 消费规则；空间索引不得改变命中规则、VRest 计时或最终逻辑结果。

### VRest 与 Parity 边界

**当前状态：B1.2 production lifecycle 与 B1.3 sparse tick 已验证；“Extended parity schema 未实施”为 B1.3 历史状态，已由 B2C 独立 Extended checksum 替代。** VRest tick 已移至独立 pass，eligibility 直接遍历 registered bucket items。

- VRest/ARest 的逻辑访问与 broadphase 解耦。空间索引减少候选枚举，不负责 VRest 的递减或过期；VRest 计时必须遍历自己的稀疏活动集合/到期结构，不能因 broadphase 未返回远距离 pair 而停止递减。
- 详细 parity snapshot（完整 slot、ARest/VRest、哈希和诊断字段）退出生产热路径，只在 `Authority400` 对拍、自检、回放或显式诊断模式中生成；生产 tick 不为 parity 预先扫描整页/全容量数据。
- Extended Driver 当前不生成 authority checksum，输出跳过/为空；direct parity capture 继续严格要求 `Authority400` Profile 且容量 400。Extended replay/checksum schema 必须另行设计，不能复用或伪装成旧 400-slot certificate。

## 1. 目标

建立只消费战斗逻辑快照的集中式表现后端，在不改变战斗结果的前提下，逐步替换战斗对象各自持有的 `Sprite` / `SpriteRenderer`：

- Loading 阶段完成 BMP 解码、依赖收集、图集规划和 GPU 资源创建，减少战斗中的资源创建与上传尖峰。
- 使用 source rect / UV 直接绘制，不再为每个图片格创建 `Sprite`。
- 将角色、武器、特殊攻击、其他对象、阴影和火花组织成一条确定顺序的 render command 流。
- 复用持久化 Mesh、顶点缓冲和 Material，避免逐帧 GameObject、Mesh、Sprite、Material 和临时容器分配。
- 通过多页图集和 `Texture2DArray` 减少透明绘制序列中的纹理切换与断批。
- 消除把 `logicalZ * 4096 + runtimeSlot * 4` 塞入 Unity `sortingOrder` 所产生的范围限制。
- 保留旧渲染后端作为迁移期回退，允许逐类切换和结果比对。

## 2. 非目标与边界

- 不改变 30 Hz 战斗逻辑 tick、pass 顺序、碰撞、输入、对象生成、命中结算或实体生命周期。
- 不以 `Transform`、插值位置、Renderer 状态或 GPU 结果反写战斗 runtime。
- 不把渲染帧变成战斗计数来源；参与规则的表现计数仍随逻辑 tick 推进。
- 不在本方案中实现完整联机、回滚、HUD、主菜单或通用场景渲染重构。
- 不以“每角色固定独占一张 2048 图集”作为最终物理布局；角色可以作为依赖收集根，但本局资源应统一装箱以避免空页和跨角色纹理切换。
- 不在本方案中处理或恢复 T8 默认 `stage.dat` 部署；T8 与本渲染方案无关，原暂缓状态不变。
- 第一阶段不承诺单次 draw call；透明正确性优先于极端合批。

## 3. 总体数据流

```text
Loading
data.txt / DAT / BMP
    -> BattleRenderDependencyCollector
    -> BattleAtlasLayoutPlanner
    -> BattleAtlasLoader
    -> Texture2DArray + BattleSpriteCatalog

Runtime（逻辑 tick）
只读 runtime 状态
    -> BattlePresentationSnapshot
    -> BattleRenderCommandBuilder
    -> 权威实体排序 + 实体内命令顺序
    -> BattleDynamicMeshBackend

Render（Unity 渲染帧）
最新完成的 Mesh / command segments
    -> BattleRenderFeature / BattleRenderPass
    -> 背景之后、后处理/UI 之前的目标注入点
```

资源准备、逻辑快照、绘制命令和 Unity 提交必须是明确边界。Loading 只准备表现资源；runtime 只提供只读真值；渲染后端不能成为战斗逻辑 owner。

## 4. 模块划分

| 模块 | 职责 |
|---|---|
| `BattleRenderDependencyCollector` | 从当前对局入口递归收集 DAT/BMP 表现依赖，按规范化路径去重 |
| `BattleAtlasLayoutPlanner` | 统计尺寸，使用确定性装箱算法生成 2048 多页布局 |
| `BattleAtlasLoader` | 解码 BMP，填充 `Texture2DArray`，上传 GPU，并在允许时释放 CPU 可读副本 |
| `BattleSpriteCatalog` | 将视觉对象和有效 pic 映射为 slice、UV、像素尺寸、pivot/中心等表现元数据 |
| `BattlePresentationSnapshot` | 在逻辑 tick 边界捕获渲染所需的只读字段 |
| `BattleRenderCommandBuilder` | 将快照展开为阴影、本体、覆盖物和命中记录等有序命令 |
| `BattleDynamicMeshBackend` | 复用 Mesh/缓冲，将命令写成 quad 顶点并形成连续渲染状态段 |
| `BattleRenderFeature` / `BattleRenderPass` | 在 URP 指定注入点提交有序 Mesh 段 |
| `LegacySpriteBackend` | 迁移期继续使用现有 `SpriteRenderer`，支持回退和 A/B 比对 |

名称只是当前建议，实施时应跟随仓库已有命名和目录边界。

## 5. Loading 依赖闭包

### 5.1 收集入口

`data.txt` 中 `type == 0` 可作为可玩角色 DAT 的资源收集根，但不能当作最终图集边界。一个角色可能通过 opoint、转换、分身、武器、技能体或 stage 生成引用 `type != 0` 的对象；公共阴影、火花、烟雾也可能位于角色 DAT 之外。

当前拟定收集流程：

1. 从本局角色和场景明确入口开始。
2. 读取每个 DAT 的 `LF2CharacterData.files`，收集其全部 BMP。
3. 递归追踪当前对局可达的 opoint、转换对象、武器、特殊攻击和固定表现资源。
4. 按规范化资源路径去重 BMP，而不是按 oid 或 DAT 去重。
5. 对无法静态闭合的动态引用建立明确的预加载清单或受控后备页，不允许在战斗热路径无界创建图集。

依赖闭包的准确规则在实施前仍需结合当前 Unity loader 与 C# 可达对象生成调用链逐项核对。

### 5.2 2048 多页图集

- Loading 阶段先统计本局全部去重 BMP 的尺寸，再运行确定性 MaxRects、Skyline 或等价装箱算法。
- 图集页固定为 `2048 x 2048`；超出一页时增加第二页及后续页面。
- 第一版优先装入完整 BMP sheet，保留 sheet 内格子布局，降低裁剪契约迁移风险。
- 所有同尺寸、同格式页面放入一个 `Texture2DArray`；顶点携带 `atlasSlice`，Shader 以 slice 选择页面。
- `Texture2DArray.depth` 创建后不能无损原地扩展，因此页数应在 Loading 规划结束后确定。
- BMP 大于页面、设备 slice 上限不足、格式不兼容或依赖漏收时必须产生可诊断失败或进入明确 fallback，不能静默显示错误图片。
- 设备不支持 Texture Array 时，回退为多个 `Texture2D`，但仍按原 painter 顺序生成连续纹理段，不按纹理重排对象。

RGBA32 的单张 2048 页面约占 16 MiB GPU 内存；若保持 readable，通常还会保留 CPU 副本。最终应根据目标 Android 格式、mipmap 策略、页数和设备上限制定预算，并在上传完成后按需调用 `Apply(false, true)` 释放 CPU 可读副本。

## 6. 图片索引与格子契约

图片查询使用 frame 的图片编号，不使用动作帧 ID：

```text
effectivePic = LF2FrameData.pic + Runtime.RenderPicOffset
```

然后在 `LF2CharacterData.files` 中找到包含 `effectivePic` 的文件区间：

```text
file.startFrame <= effectivePic <= file.endFrame
localPic = effectivePic - file.startFrame
```

格子按 DAT 现有契约换算：

```text
column     = localPic % columns
rowFromTop = localPic / columns
```

必须在实现前锁定并自动验证以下约束：

- `LF2FrameData.frameId` 是动作状态帧编号，不是图片格子索引。
- `LF2FrameData.pic` 才是图片编号；多个 frame 可以复用同一 pic。
- `RenderPicOffset` 参与最终显示图片查询。
- `pic == 999` 及其他现有无图语义不提交本体命令。
- 当前 DAT 的 `row` / `col` 命名与横纵格数的实际含义必须沿用现有 parser/loader 契约，不能按英文名猜测。
- 格子步长保留当前 sheet 的间隔像素：横纵方向按 `(w + 1, h + 1)` 推进，而不是只用 `(w, h)`。
- BMP 左上角编号与 Unity UV 原点方向不同；Catalog 负责一次性换算，runtime 不重复做易错的 Y 翻转。
- Catalog 同时保存像素宽高、中心/pivot 和必要裁剪元数据，使碰撞/逻辑尺寸不依赖运行时 `Sprite.rect`。

建议 Catalog 的稳定查询键为 `(visualDataId, effectivePic)` 或能唯一定位 DAT file range 的等价结构，结果至少包含 `atlasSlice`、`uvRect`、像素尺寸和 pivot。

## 7. PresentationSnapshot

`BattlePresentationSnapshot` 在逻辑 tick 完成后的稳定边界读取 runtime，只包含表现需要的数据，不持有可变 runtime 引用。候选字段包括：

```text
RuntimeSlot / StableId / Oid
ZInt / XInt / YInt / 表现高度字段
Frame / Pic / RenderPicOffset
Facing / Visible / Alpha / Tint
Shadow 与 overlay/hit-record 所需表现参数
```

最终字段必须从当前实际消费者倒推，不能把整个实体复制进快照。快照生成和消费需要避免逐 tick GC；使用双缓冲或环形缓冲，让 Unity 渲染帧只读取最后一个完整快照。渲染插值只能作用于表现坐标，不改变排序 key，不写回 runtime。

## 8. RenderCommand 与权威顺序

单条 `BattleRenderCommand` 的候选结构：

```text
CommandType
AtlasSlice / UVRect
Position / Size / Pivot
FlipX
Color / Alpha
BlendMode / MaterialVariant
RuntimeSlot / StableId / ZInt
```

全局实体顺序必须沿用 C# 权威可观察绘制顺序：

```text
Runtime.ZInt 升序
相同 ZInt 时 Runtime.SlotIndex 升序
```

对排序后的每个实体，命令按实体内顺序连续追加：

```text
Shadow -> Entity -> Overlay -> HitRecord
```

不得先画全体阴影、再画全体角色；也不得为凑图集或材质批次而跨实体重排透明命令。`YInt`、`displayZ`、`Zz`、shake 和类型专项视觉偏移只能影响顶点位置，不能替换 `(ZInt, RuntimeSlot)` 的全局顺序。

上述“权威”指最终可观察顺序必须与 `J:\QQFile\NTSD2.4\ntsd_release_C#` 对应绘制调用链一致。实施前需重新定位真实调用者、活动 slot 过滤、阴影/本体/覆盖物/命中记录的条件分支，并把证据加入对齐记录；本草案不代替该核验。

## 9. 持久化动态 Quad Mesh

每条可见命令写成一个 quad：4 个顶点、6 个固定索引、2 个三角形。顶点至少包含：

```text
position
uv
color
atlasSlice
```

“持久化”表示以下对象只初始化或扩容时创建，而不是逐帧创建：

- `Mesh`，并调用 `MarkDynamic()`。
- 顶点/索引缓冲和 CPU 侧复用数组或原生容器。
- 固定 quad 索引模板。
- 共享 Material 和 Shader variant。

每个逻辑 tick 或需要重建表现数据时：

1. 将已排序命令顺序写入复用顶点缓冲。
2. 使用 `Mesh.SetVertexBufferData` 或匹配当前 Unity 版本的低分配 API，仅上传活动顶点范围。
3. 更新实际 index count / submesh 或 chunk 范围。
4. 渲染帧重复提交最近完成的数据，不重复推进逻辑计数。

建议以 UInt16 索引限制为边界划分 chunk，例如每 chunk 4096 quad 对应 16384 顶点和 24576 索引；这只是实现候选，不是实体数量上限。命令数可能大于实体数，因为一个实体可以产生阴影、本体和多个附加命令。容量应按命令峰值监测，并在 Loading 预留或按明确策略扩容。

## 10. URP 提交

通过 `ScriptableRendererFeature` / `ScriptableRenderPass` 在战斗相机的确定注入点绘制集中式 Mesh。目标顺序是背景之后、需要参与的世界后处理之前、屏幕 UI 之前；准确 `RenderPassEvent` 需结合当前 URP Renderer 和相机栈验证。

战斗 Mesh 对 Unity 只需要稳定的整体层级。Mesh 内部的对象顺序由 render command 与索引/segment 顺序表达，不再将大范围逻辑 key 编码到 `sortingOrder`。相机裁剪、像素缩放、颜色空间、RenderTexture 和后处理必须在桌面与 Android 目标设备上分别验证。

## 11. 透明绘制与三种模式

默认使用透明混合和 `ZWrite Off`，并按 painter 顺序提交。阴影、烟雾、光效可能含半透明像素，因此不能未经素材和遮挡矩阵验证就统一改为 Alpha Clip 或 `ZWrite On`。

提供三级后端策略：

| 模式 | 说明 | 用途 |
|---|---|---|
| `SingleMesh` | 同一兼容渲染状态尽量由单 Mesh/少量 draw 提交 | 目标性能模式；必须做目标 GPU 像素验证 |
| `OrderedChunks` | 严格保持命令顺序，只把相邻且状态兼容的命令合并为连续段 | 默认稳妥模式；状态变化时断批 |
| `StrictOrderedDraw` | 以更细粒度 draw 保证问题对象或设备的顺序 | 正确性回退和诊断模式 |

Alpha、Additive、Stencil、不同 Shader 或其他 GPU 状态必须断批；只能在原始命令流中切连续段，不能把不相邻的同材质命令抽出合并。Unity/目标 GPU 是否严格按单 Mesh 索引顺序处理所有透明三角形不能只靠桌面推断，必须在目标 Adreno、Mali 等设备用重叠像素场景验证。若结果不稳定，设备配置自动使用 `OrderedChunks` 或 `StrictOrderedDraw`。

## 12. 双后端迁移

迁移期建议保留以下模式：

```text
LegacyOnly
CentralShadowBuild（集中后端生成但不显示，用于命令/排序比对）
CentralOnly
```

切换顺序：

1. 先独立修复现有 `sortingOrder` 越界，使用活动实体紧凑 rank 或其他短期安全映射；不等待整套渲染重构。
2. 建立不依赖 `Sprite.rect` 的 `SpriteMetricsResolver` / Catalog 数据契约。
3. 建立 `BattleSpriteCatalog`，暂时继续由旧 `SpriteRenderer` 消费。
4. 建立 Snapshot 和 RenderCommand，在 shadow-build 下逐 tick 对比对象数量、图片、位置和顺序。
5. 接入持久动态 Quad Mesh 与 URP Pass，先迁移本体。
6. 依次迁移阴影、持有物、overlay、spark/hit record；每类都有旧后端对照。
7. 接入 2048 多页 `Texture2DArray` 和移动端压缩格式。
8. 完成目标 Android GPU 的正确性、内存和性能验收后，才考虑移除战斗 `SpriteRenderer`。

旧后端与新后端不能同时对同一类别实际出图，避免重复显示；shadow-build 只记录/比较，不提交像素。

## 13. 分阶段计划

| 阶段 | 产物 | 进入下一阶段的门槛 |
|---|---|---|
| P0 契约核验 | C# 绘制调用链、Unity 当前消费者、slot/排序/格子契约清单 | 用户确认知识点和总体设计；证据可定位 |
| P1 排序止血 | 当前后端不越界的紧凑排序映射与 focused check | 编译、自检、重叠对象 Play 验证通过 |
| P2 Catalog | BMP/file/pic 到 metrics/UV 的唯一查询层，旧后端消费 | 全部代表性 DAT 的图片索引矩阵通过 |
| P3 Command shadow-build | Snapshot、命令生成、旧/新顺序对比工具 | 多对象、多 Z、同 Z、生成/回收场景逐 tick 等价 |
| P4 Mesh/URP | 持久 Mesh、Shader、URP Pass、OrderedChunks | 桌面像素基线与 Play 场景通过，无逐帧 GC 回归 |
| P5 Atlas Array | 确定性多页装箱、Texture2DArray、fallback | 图集覆盖、内存预算、设备能力与漏依赖处理通过 |
| P6 移动端验收 | Adreno/Mali 真机结果、模式选择与性能报告 | 正确性矩阵通过，性能/内存达到项目预算 |
| P7 收口 | CentralOnly 默认，旧后端移除条件评审 | 回退期完成且长期场景无差异后单独批准 |

每个阶段都应是可回退、可验证的独立提交；不能以最终架构目标跳过中间的可观察行为对比。

## 14. 验收矩阵

| 维度 | 最低检查 |
|---|---|
| 编译 | Unity 2022.3.4f1c1 脚本编译 0 error |
| 自动自检 | 资源索引、file range 边界、`RenderPicOffset`、`pic=999`、row/col、`w+1/h+1`、排序和容量 focused checks |
| 逻辑隔离 | 启用/禁用新后端时 battle checksum 和 runtime 字段完全不变 |
| 图片正确性 | 每个代表性 DAT 的首格、行尾、下一行、file range 首尾、offset、翻面、pivot 像素对照 |
| 层级正确性 | 不同 Z、同 Z 不同 slot、实体交错阴影、持有物、overlay、hit record 的重叠截图/像素断言 |
| 生命周期 | spawn、回收、复用、变身、分身、武器持有/释放后无旧图、错图或残留命令 |
| 透明状态 | Alpha、Additive、Stencil/特殊 Shader 按原命令流断段且不重排 |
| 容量 | 0 实体、常规负载、峰值命令、超过预留容量、跨 UInt16 chunk 边界 |
| 设备兼容 | Texture Array 支持/不支持、slice 上限、Adreno/Mali 的 `SingleMesh` 与 fallback 像素结果 |
| 性能 | Loading 时间、CPU/GPU 内存、上传峰值、draw call、SetPass、主线程耗时、GC alloc |
| 回退 | `LegacyOnly`、shadow-build、`CentralOnly` 可控切换，故障设备可降级 |

最终报告必须分别标记“方案确认”“逻辑已写”“编译通过”“self-check 通过”“Play Mode 通过”“目标 Android 真机通过”，不得互相代替。

## 15. 主要风险与待确认项

- **依赖漏收**：动态 opoint/转换/stage 引用未进入 Loading 闭包，会导致战斗中缺图。需要权威调用链和生产 DAT 扫描共同闭合。
- **内存预算**：2048 RGBA32 页面约 16 MiB；页面过多、CPU readable 副本和 mipmap 会迅速扩大占用。
- **纹理格式**：运行时拼图与 ASTC/ETC2 构建期压缩的组合方式、颜色空间和 alpha 质量尚待技术验证。
- **透明顺序**：单 Mesh 内透明三角形的实际执行顺序需要目标 GPU 像素验证；不能只以 draw call 数量判定正确。
- **状态断批**：不同 blend/stencil/shader 仍会产生 draw；图集只能消除纹理页切换，不能合并不兼容 GPU 状态。
- **页边缘采样**：线性过滤、mipmap 和 atlas bleeding 需要 padding/extrusion 策略；原 BMP 格子的一像素分隔不能直接等同安全 atlas padding。
- **像素坐标与 pivot**：BMP 顶左编号、Unity UV 原点、翻面和中心点若分散换算，容易出现一像素偏移；应集中到 Catalog 并做边界测试。
- **容量误读**：`400` 必须保留为 `Authority400` 的兼容边界，但不能继续解释为所有 Unity 模式的固定 runtime 槽位上限。slot address 容量、active entity 预算和 render command 数是三个不同概念；移动端 1000 active 或桌面分页增长都不代表同数量的绘制命令。每实体可能展开为阴影、本体、覆盖物和命中记录等多条命令，因此 Mesh 容量与 chunk 边界必须按 render command 峰值独立设计。
- **URP 注入点**：相机栈、后处理、RenderTexture 和 UI 的现状需要实际工程验证。
- **API/平台约束**：正式实现前应查阅 Unity 2022.3 对 `Texture2DArray`、`Mesh.SetVertexBufferData`、URP Renderer Feature/Pass 和移动平台纹理格式的官方文档。
- **迁移双维护**：旧/新后端并存会增加短期复杂度，需要清晰的类别 ownership 和移除门槛。

## 16. 当前决策记录

已确认的设计决策是：保留 `Authority400` 兼容模式；移动端全部槽区合计最多 1000 active 且第 1001 个确定性拒绝；桌面从 512 开始按 256-slot 页自动增长并受技术预算约束；空闲槽使用二叉最小堆 + `nextUnused`；B0 先以 X/Z Loose Quadtree shadow 诊断对比，B2A 提供 formal full-rebuild backend，B2B 再以 `(slot, generation)` 身份在 collision collect 边界实施 batch 增量同步，默认仍为 `BruteForce`；VRest 与 broadphase 解耦；详细 parity snapshot 不进入生产热路径。生产 Profile 优先级为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认，broadphase 独立遵循命令行 > `GameConfig` > 默认 `BruteForce`；设备能力只降级表现资源/后端，三个 Profile 共用同一套确定性 runtime 算法。

截至 2026-07-20，R1-R2C-4、B0、B1-B1.3、B2A 与 B2B 已完成代码层实施和既定验证。B2B generation-aware incremental backend 的 fresh chain 为 source `22:43:57` < DLL `22:46:36` < result `22:47:04` **PASS**，dotnet **0 errors**，architect final **PASS / no blocker**。该段“即时 weapon/body query、AI 查询、Extended parity schema 仍是后续任务”为 B2B 历史状态，已由 B2C 替代；B2C 最新 full self-check `2026-07-21 00:48:06` **PASS**、dotnet **0 errors / 42 existing warnings**，但未执行 Play Mode、性能或 fresh Architect PASS。生产默认仍为 `BruteForce`，集中式渲染与 T8 默认 `stage.dat` 部署仍暂缓。


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\BattleCentralRenderTypes.cs ---
using System;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    public enum BattleCentralDrawMode : byte
    {
        OrderedChunks = 0,
        StrictOrderedDraw = 1,
        SingleMeshDiagnosticOnly = 2,
    }

    public enum BattleCentralResourceStatus : byte
    {
        Resolved = 0,
        UnresolvedVisual = 1,
        UnsupportedCategory = 2,
    }

    public readonly struct BattleCentralResolvedResource
    {
        public BattleCentralResolvedResource(
            Texture texture,
            Material material,
            Rect normalizedUv,
            Vector2 pixelSize,
            Vector2 pivot,
            Color32 color,
            int materialVariant = 0,
            int atlasSlice = 0)
        {
            Texture = texture;
            Material = material;
            NormalizedUv = normalizedUv;
            PixelSize = pixelSize;
            Pivot = pivot;
            Color = color;
            MaterialVariant = materialVariant;
            AtlasSlice = atlasSlice;
        }

        public Texture Texture { get; }
        public Material Material { get; }
        public Rect NormalizedUv { get; }
        public Vector2 PixelSize { get; }
        public Vector2 Pivot { get; }
        public Color32 Color { get; }
        public int MaterialVariant { get; }
        public int AtlasSlice { get; }

        internal bool HasDrawableResource => Texture != null && Material != null;
    }

    public interface IBattleCentralResourceResolver
    {
        BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource);
    }

    public readonly struct BattleCentralRenderSegment
    {
        public BattleCentralRenderSegment(
            int chunkIndex,
            int subMeshIndex,
            int firstCommandIndex,
            int commandCount,
            int firstQuad,
            int quadCount,
            Texture texture,
            Material material,
            int materialVariant,
            int atlasSlice)
        {
            ChunkIndex = chunkIndex;
            SubMeshIndex = subMeshIndex;
            FirstCommandIndex = firstCommandIndex;
            CommandCount = commandCount;
            FirstQuad = firstQuad;
            QuadCount = quadCount;
            Texture = texture;
            Material = material;
            MaterialVariant = materialVariant;
            AtlasSlice = atlasSlice;
        }

        public int ChunkIndex { get; }
        public int SubMeshIndex { get; }
        public int FirstCommandIndex { get; }
        public int CommandCount { get; }
        public int FirstQuad { get; }
        public int QuadCount { get; }
        public Texture Texture { get; }
        public Material Material { get; }
        public int MaterialVariant { get; }
        public int AtlasSlice { get; }
    }

    public sealed class BattleCentralBuildDiagnostics
    {
        public int TickIndex { get; internal set; }
        public int SourceCommandCount { get; internal set; }
        public int ResolvedCommandCount { get; internal set; }
        public int UnresolvedCommandCount { get; internal set; }
        public int UnsupportedCategoryCount { get; internal set; }
        public int FirstUnresolvedCommandIndex { get; internal set; } = -1;
        public BattleRenderCommandType FirstUnresolvedCommandType { get; internal set; }
        public int ActiveChunkCount { get; internal set; }
        public int SegmentCount { get; internal set; }
        public int CapacityGrowthCount { get; internal set; }
        public BattleCentralDrawMode DrawMode { get; internal set; }

        internal void Reset(int tickIndex, int sourceCommandCount, BattleCentralDrawMode drawMode)
        {
            TickIndex = tickIndex;
            SourceCommandCount = sourceCommandCount;
            ResolvedCommandCount = 0;
            UnresolvedCommandCount = 0;
            UnsupportedCategoryCount = 0;
            FirstUnresolvedCommandIndex = -1;
            FirstUnresolvedCommandType = default;
            ActiveChunkCount = 0;
            SegmentCount = 0;
            DrawMode = drawMode;
        }
    }

    public sealed class BattleCatalogCentralResourceResolver : IBattleCentralResourceResolver
    {
        private BattleSpriteCatalog catalog = BattleSpriteCatalog.Empty;
        private Material material;

        public void Configure(BattleSpriteCatalog value, Material sharedMaterial)
        {
            catalog = value ?? BattleSpriteCatalog.Empty;
            material = sharedMaterial;
        }

        public BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            if (command.Type != BattleRenderCommandType.Entity)
            {
                resource = default;
                return command.Type == BattleRenderCommandType.OverlayUnsupportedDiagnostic
                    ? BattleCentralResourceStatus.UnsupportedCategory
                    : BattleCentralResourceStatus.UnresolvedVisual;
            }

            if (!catalog.TryGet(command.VisualDataId, command.EffectivePic, out BattleSpriteEntry entry) ||
                entry?.SharedTexture == null)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            resource = new BattleCentralResolvedResource(
                entry.SharedTexture,
                material,
                entry.NormalizedUv,
                new Vector2(entry.PixelWidth, entry.PixelHeight),
                entry.Pivot,
                Color.white);
            return BattleCentralResourceStatus.Resolved;
        }
    }
}


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\BattleDynamicMeshBackend.cs ---
using System;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleDynamicMeshBackend : IDisposable
    {
        public const int QuadsPerChunk = 4096;
        public const int VerticesPerQuad = 4;
        public const int IndicesPerQuad = 6;
        public const int VerticesPerChunk = QuadsPerChunk * VerticesPerQuad;
        public const int IndicesPerChunk = QuadsPerChunk * IndicesPerQuad;
        public const int MaxUInt16VertexIndex = VerticesPerChunk - 1;

        private readonly BattleCentralBuildDiagnostics diagnostics = new BattleCentralBuildDiagnostics();
        private BattleMeshChunk[] chunks = new BattleMeshChunk[1];
        private BattleCentralRenderSegment[] segments = new BattleCentralRenderSegment[16];
        private int activeChunkCount;
        private int segmentCount;
        private bool disposed;

        public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
        public int ActiveChunkCount => activeChunkCount;
        public int SegmentCount => segmentCount;
        public int AllocatedChunkCount => chunks.Length;

        public Mesh GetChunkMesh(int index)
        {
            if ((uint)index >= (uint)activeChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return chunks[index].Mesh;
        }

        public int GetChunkActiveQuadCount(int index)
        {
            if ((uint)index >= (uint)activeChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return chunks[index].ActiveQuadCount;
        }

        internal ushort GetChunkIndexTemplateValue(int chunkIndex, int index)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetIndexTemplateValue(index);
        }

        public BattleCentralRenderSegment GetSegment(int index)
        {
            if ((uint)index >= (uint)segmentCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return segments[index];
        }

        public void Build(
            BattlePresentationFrame frame,
            IBattleCentralResourceResolver resolver,
            BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleDynamicMeshBackend));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            int commandCount = frame?.CommandCount ?? 0;
            diagnostics.Reset(frame?.TickIndex ?? 0, commandCount, drawMode);
            segmentCount = 0;
            int resolvedCount = 0;
            int lastChunkIndex = -1;
            int lastSegmentIndex = -1;

            for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
            {
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                BattleCentralResourceStatus status = resolver.Resolve(command, out BattleCentralResolvedResource resource);
                if (status != BattleCentralResourceStatus.Resolved)
                {
                    if (status == BattleCentralResourceStatus.UnsupportedCategory)
                        diagnostics.UnsupportedCategoryCount++;
                    else
                        diagnostics.UnresolvedCommandCount++;
                    if (diagnostics.FirstUnresolvedCommandIndex < 0)
                    {
                        diagnostics.FirstUnresolvedCommandIndex = commandIndex;
                        diagnostics.FirstUnresolvedCommandType = command.Type;
                    }
                    // An unresolved command still occupies an authoritative position
                    // in the P3 stream. Never batch resolved commands across it.
                    lastSegmentIndex = -1;
                    lastChunkIndex = -1;
                    continue;
                }

                int chunkIndex = resolvedCount / QuadsPerChunk;
                int quadIndex = resolvedCount % QuadsPerChunk;
                EnsureChunk(chunkIndex);
                BattleMeshChunk chunk = chunks[chunkIndex];
                chunk.WriteQuad(quadIndex, command, resource);

                bool strict = drawMode == BattleCentralDrawMode.StrictOrderedDraw;
                bool canAppend = !strict && lastSegmentIndex >= 0 && lastChunkIndex == chunkIndex &&
                                 IsCompatible(segments[lastSegmentIndex], resource) &&
                                 segments[lastSegmentIndex].FirstQuad + segments[lastSegmentIndex].QuadCount == quadIndex;
                if (canAppend)
                {
                    BattleCentralRenderSegment previous = segments[lastSegmentIndex];
                    segments[lastSegmentIndex] = new BattleCentralRenderSegment(
                        previous.ChunkIndex,
                        previous.SubMeshIndex,
                        previous.FirstCommandIndex,
                        commandIndex - previous.FirstCommandIndex + 1,
                        previous.FirstQuad,
                        previous.QuadCount + 1,
                        previous.Texture,
                        previous.Material,
                        previous.MaterialVariant,
                        previous.AtlasSlice);
                }
                else
                {
                    EnsureSegmentCapacity(segmentCount + 1);
                    int subMeshIndex = chunk.PendingSegmentCount;
                    chunk.PendingSegmentCount++;
                    segments[segmentCount] = new BattleCentralRenderSegment(
                        chunkIndex,
                        subMeshIndex,
                        commandIndex,
                        1,
                        quadIndex,
                        1,
                        resource.Texture,
                        resource.Material,
                        resource.MaterialVariant,
                        resource.AtlasSlice);
                    lastSegmentIndex = segmentCount++;
                    lastChunkIndex = chunkIndex;
                }

                resolvedCount++;
            }

            activeChunkCount = resolvedCount == 0 ? 0 : (resolvedCount + QuadsPerChunk - 1) / QuadsPerChunk;
            int segmentCursor = 0;
            for (int chunkIndex = 0; chunkIndex < activeChunkCount; chunkIndex++)
            {
                BattleMeshChunk chunk = chunks[chunkIndex];
                int activeQuads = Math.Min(QuadsPerChunk, resolvedCount - chunkIndex * QuadsPerChunk);
                chunk.Upload(chunkIndex, activeQuads, segments, ref segmentCursor, segmentCount);
            }
            for (int chunkIndex = activeChunkCount; chunkIndex < chunks.Length; chunkIndex++)
                chunks[chunkIndex]?.ClearActive();

            diagnostics.ResolvedCommandCount = resolvedCount;
            diagnostics.ActiveChunkCount = activeChunkCount;
            diagnostics.SegmentCount = segmentCount;
        }

        public void Clear()
        {
            segmentCount = 0;
            activeChunkCount = 0;
            for (int i = 0; i < chunks.Length; i++)
                chunks[i]?.ClearActive();
            diagnostics.Reset(0, 0, BattleCentralDrawMode.OrderedChunks);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            for (int i = 0; i < chunks.Length; i++)
                chunks[i]?.Dispose();
            chunks = Array.Empty<BattleMeshChunk>();
            segments = Array.Empty<BattleCentralRenderSegment>();
            activeChunkCount = 0;
            segmentCount = 0;
        }

        private void EnsureChunk(int chunkIndex)
        {
            if (chunkIndex >= chunks.Length)
            {
                int next = chunks.Length;
                while (next <= chunkIndex)
                    next = checked(next * 2);
                Array.Resize(ref chunks, next);
                diagnostics.CapacityGrowthCount++;
            }
            if (chunks[chunkIndex] == null)
            {
                chunks[chunkIndex] = new BattleMeshChunk(chunkIndex);
                diagnostics.CapacityGrowthCount++;
            }
        }

        private void EnsureSegmentCapacity(int required)
        {
            if (required <= segments.Length)
                return;
            int next = segments.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref segments, next);
            diagnostics.CapacityGrowthCount++;
        }

        private static bool IsCompatible(
            in BattleCentralRenderSegment segment,
            in BattleCentralResolvedResource resource)
        {
            return segment.Texture == resource.Texture &&
                   segment.Material == resource.Material &&
                   segment.MaterialVariant == resource.MaterialVariant &&
                   segment.AtlasSlice == resource.AtlasSlice;
        }

        private struct BattleQuadVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
        }

        private sealed class BattleMeshChunk : IDisposable
        {
            private static readonly VertexAttributeDescriptor[] VertexLayout =
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1),
            };

            private readonly BattleQuadVertex[] vertices = new BattleQuadVertex[VerticesPerChunk];
            private readonly ushort[] indexTemplate = new ushort[IndicesPerChunk];
            private bool hasBounds;
            private Vector3 boundsMin;
            private Vector3 boundsMax;

            public BattleMeshChunk(int index)
            {
                Mesh = new Mesh
                {
                    name = $"NTSD Battle Central Chunk {index}",
                    indexFormat = IndexFormat.UInt16,
                };
                Mesh.MarkDynamic();
                Mesh.SetVertexBufferParams(VerticesPerChunk, VertexLayout);
                Mesh.SetIndexBufferParams(IndicesPerChunk, IndexFormat.UInt16);
                for (int quad = 0; quad < QuadsPerChunk; quad++)
                {
                    int vertex = quad * VerticesPerQuad;
                    int indexOffset = quad * IndicesPerQuad;
                    indexTemplate[indexOffset] = (ushort)vertex;
                    indexTemplate[indexOffset + 1] = (ushort)(vertex + 1);
                    indexTemplate[indexOffset + 2] = (ushort)(vertex + 2);
                    indexTemplate[indexOffset + 3] = (ushort)(vertex + 2);
                    indexTemplate[indexOffset + 4] = (ushort)(vertex + 1);
                    indexTemplate[indexOffset + 5] = (ushort)(vertex + 3);
                }
                Mesh.SetIndexBufferData(
                    indexTemplate,
                    0,
                    0,
                    indexTemplate.Length,
                    MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers);
                ClearActive();
            }

            public Mesh Mesh { get; }
            public int ActiveQuadCount { get; private set; }
            public int PendingSegmentCount { get; set; }

            public ushort GetIndexTemplateValue(int index)
            {
                if ((uint)index >= (uint)indexTemplate.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return indexTemplate[index];
            }

            public void WriteQuad(
                int quadIndex,
                in BattleRenderCommand command,
                in BattleCentralResolvedResource resource)
            {
                if ((uint)quadIndex >= QuadsPerChunk)
                    throw new ArgumentOutOfRangeException(nameof(quadIndex));

                Vector2 pixelSize = resource.PixelSize.sqrMagnitude > 0f ? resource.PixelSize : command.Size;
                Vector2 pivot = resource.Pivot;
                float width = pixelSize.x * NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale;
                float height = pixelSize.y * NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale;
                float left = command.Position.x - pivot.x * width;
                float right = left + width;
                float bottom = command.Position.y - pivot.y * height;
                float top = bottom + height;
                float z = command.Position.z;

                Rect uv = resource.NormalizedUv;
                float u0 = command.FlipX ? uv.xMax : uv.xMin;
                float u1 = command.FlipX ? uv.xMin : uv.xMax;
                int vertex = quadIndex * VerticesPerQuad;
                vertices[vertex] = CreateVertex(left, bottom, z, u0, uv.yMin, resource);
                vertices[vertex + 1] = CreateVertex(left, top, z, u0, uv.yMax, resource);
                vertices[vertex + 2] = CreateVertex(right, bottom, z, u1, uv.yMin, resource);
                vertices[vertex + 3] = CreateVertex(right, top, z, u1, uv.yMax, resource);

                Encapsulate(new Vector3(left, bottom, z));
                Encapsulate(new Vector3(right, top, z));
            }

            public void Upload(
                int chunkIndex,
                int activeQuads,
                BattleCentralRenderSegment[] allSegments,
                ref int segmentCursor,
                int totalSegments)
            {
                ActiveQuadCount = activeQuads;
                int activeVertices = activeQuads * VerticesPerQuad;
                if (activeVertices > 0)
                {
                    Mesh.SetVertexBufferData(
                        vertices,
                        0,
                        0,
                        activeVertices,
                        0,
                        MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                        MeshUpdateFlags.DontNotifyMeshUsers);
                }

                Mesh.subMeshCount = PendingSegmentCount;
                while (segmentCursor < totalSegments &&
                       allSegments[segmentCursor].ChunkIndex == chunkIndex)
                {
                    BattleCentralRenderSegment segment = allSegments[segmentCursor];
                    Mesh.SetSubMesh(
                        segment.SubMeshIndex,
                        new SubMeshDescriptor(
                            segment.FirstQuad * IndicesPerQuad,
                            segment.QuadCount * IndicesPerQuad,
                            MeshTopology.Triangles)
                        {
                            baseVertex = 0,
                            firstVertex = segment.FirstQuad * VerticesPerQuad,
                            vertexCount = segment.QuadCount * VerticesPerQuad,
                            bounds = CurrentBounds(),
                        },
                        MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                        MeshUpdateFlags.DontNotifyMeshUsers);
                    segmentCursor++;
                    if (segmentCursor >= totalSegments ||
                        allSegments[segmentCursor].ChunkIndex != segment.ChunkIndex)
                    {
                        break;
                    }
                }
                Mesh.bounds = CurrentBounds();
                PendingSegmentCount = 0;
                hasBounds = false;
            }

            public void ClearActive()
            {
                ActiveQuadCount = 0;
                PendingSegmentCount = 0;
                Mesh.subMeshCount = 0;
                Mesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
                hasBounds = false;
            }

            public void Dispose()
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(Mesh);
                else
                    UnityEngine.Object.DestroyImmediate(Mesh);
            }

            private static BattleQuadVertex CreateVertex(
                float x,
                float y,
                float z,
                float u,
                float v,
                in BattleCentralResolvedResource resource)
            {
                return new BattleQuadVertex
                {
                    Position = new Vector3(x, y, z),
                    Color = resource.Color,
                    Uv = new Vector2(u, v),
                    AtlasSlice = resource.AtlasSlice,
                };
            }

            private void Encapsulate(Vector3 position)
            {
                if (!hasBounds)
                {
                    boundsMin = position;
                    boundsMax = position;
                    hasBounds = true;
                    return;
                }
                boundsMin = Vector3.Min(boundsMin, position);
                boundsMax = Vector3.Max(boundsMax, position);
            }

            private Bounds CurrentBounds()
            {
                if (!hasBounds)
                    return new Bounds(Vector3.zero, Vector3.zero);
                var bounds = new Bounds();
                bounds.SetMinMax(boundsMin, boundsMax);
                return bounds;
            }
        }
    }
}


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\BattleCentralRenderSystem.cs ---
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleCentralRuntimeDiagnostics
    {
        public BattlePresentationBackendMode RequestedMode { get; internal set; }
        public BattlePresentationBackendMode EffectivePixelMode { get; internal set; }
        public bool FeatureAvailable { get; internal set; }
        public bool MaterialAvailable { get; internal set; }
        public bool FrameAvailable { get; internal set; }
        public bool AllCategoryOwnershipReady { get; internal set; }
        public bool SubmissionReady { get; internal set; }
        public bool SubmittedPixelsLastFrame { get; internal set; }
        public int SubmissionCount { get; internal set; }
        public string RefusalReason { get; internal set; } = string.Empty;
    }

    public static class BattleCentralRenderSystem
    {
        private static readonly BattleDynamicMeshBackend Backend = new BattleDynamicMeshBackend();
        private static readonly BattleCatalogCentralResourceResolver CatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCentralRuntimeDiagnostics RuntimeDiagnostics =
            new BattleCentralRuntimeDiagnostics();

        private static BattleRenderFeature featureOwner;
        private static Material featureMaterial;
        private static BattlePresentationBackendMode requestedMode = BattlePresentationBackendMode.LegacyOnly;
        private static BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks;
        private static bool submissionReady;

        public static BattleDynamicMeshBackend MeshBackend => Backend;
        public static BattleCentralRuntimeDiagnostics Diagnostics => RuntimeDiagnostics;

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            BattleCentralDrawMode mode)
        {
            featureOwner = owner;
            featureMaterial = material;
            drawMode = mode;
            RuntimeDiagnostics.FeatureAvailable = owner != null;
            RuntimeDiagnostics.MaterialAvailable = material != null;
        }

        internal static void UnregisterFeature(BattleRenderFeature owner)
        {
            if (featureOwner != owner)
                return;
            featureOwner = null;
            featureMaterial = null;
            submissionReady = false;
            RuntimeDiagnostics.FeatureAvailable = false;
            RuntimeDiagnostics.MaterialAvailable = false;
            RuntimeDiagnostics.SubmissionReady = false;
        }

        public static void PrepareFrame(SimulationWorld world)
        {
            requestedMode = world?.BattlePresentation?.Mode ?? BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.RequestedMode = requestedMode;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;

            if (requestedMode == BattlePresentationBackendMode.LegacyOnly || world == null)
            {
                Backend.Clear();
                submissionReady = false;
                RuntimeDiagnostics.FrameAvailable = false;
                RuntimeDiagnostics.AllCategoryOwnershipReady = false;
                RuntimeDiagnostics.SubmissionReady = false;
                RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
                RuntimeDiagnostics.RefusalReason = "LegacyOnly does not build or submit central geometry.";
                return;
            }

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            RuntimeDiagnostics.FrameAvailable = frame != null;
            BattleSpriteCatalog catalog = CharacterAnimtorManager.Instance != null
                ? CharacterAnimtorManager.Instance.SpriteCatalog
                : BattleSpriteCatalog.Empty;
            CatalogResolver.Configure(catalog, featureMaterial);
            Backend.Build(frame, CatalogResolver, drawMode);

            // P4 lands the technical backend while P3 still reports Overlay as an
            // unsupported interleaving category. Mixed legacy/central ownership is
            // forbidden, so CentralOnly remains unavailable until every category
            // that can interleave in the command stream has a central resolver.
            bool allCategoryOwnershipReady = frame != null && frame.OverlayUnsupportedCount == 0 &&
                                             Backend.Diagnostics.UnsupportedCategoryCount == 0 &&
                                             Backend.Diagnostics.UnresolvedCommandCount == 0;
            RuntimeDiagnostics.AllCategoryOwnershipReady = allCategoryOwnershipReady;
            submissionReady = requestedMode == BattlePresentationBackendMode.CentralOnly &&
                              featureOwner != null && featureMaterial != null && frame != null &&
                              allCategoryOwnershipReady;
            RuntimeDiagnostics.SubmissionReady = submissionReady;
            RuntimeDiagnostics.EffectivePixelMode = submissionReady
                ? BattlePresentationBackendMode.CentralOnly
                : BattlePresentationBackendMode.LegacyOnly;

            if (requestedMode == BattlePresentationBackendMode.CentralShadowBuild)
                RuntimeDiagnostics.RefusalReason = "CentralShadowBuild uploads geometry for comparison and never submits pixels.";
            else if (featureOwner == null)
                RuntimeDiagnostics.RefusalReason = "BattleRenderFeature is not installed; pixel output falls back to LegacyOnly.";
            else if (featureMaterial == null)
                RuntimeDiagnostics.RefusalReason = "The central battle material is missing; pixel output falls back to LegacyOnly.";
            else if (!allCategoryOwnershipReady)
                RuntimeDiagnostics.RefusalReason = "Not all interleaving presentation categories have central ownership.";
            else
                RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        internal static bool TryGetSubmission(
            Camera camera,
            CameraRenderType renderType,
            out BattleDynamicMeshBackend backend)
        {
            backend = null;
            if (!CanRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera) || !submissionReady ||
                requestedMode != BattlePresentationBackendMode.CentralOnly)
            {
                return false;
            }
            backend = Backend;
            return true;
        }

        public static bool CanRenderCamera(Camera camera, CameraRenderType renderType, Camera worldCamera)
        {
            return camera != null && worldCamera != null && camera == worldCamera &&
                   renderType == CameraRenderType.Base;
        }

        internal static void RecordSubmission(int drawCount)
        {
            RuntimeDiagnostics.SubmittedPixelsLastFrame = drawCount > 0;
            RuntimeDiagnostics.SubmissionCount += drawCount;
        }

        public static void ResetRuntime()
        {
            Backend.Clear();
            requestedMode = BattlePresentationBackendMode.LegacyOnly;
            submissionReady = false;
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.FrameAvailable = false;
            RuntimeDiagnostics.AllCategoryOwnershipReady = false;
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }
    }
}


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\BattleRenderFeature.cs ---
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material material;
        [SerializeField] private BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks;

        private BattleRenderPass pass;

        public Material Material => material;
        public BattleCentralDrawMode DrawMode => drawMode;
        public RenderPassEvent InjectionPoint => RenderPassEvent.AfterRenderingTransparents;

        public void Configure(Material value, BattleCentralDrawMode mode)
        {
            material = value;
            drawMode = mode;
            Create();
        }

        public override void Create()
        {
            pass ??= new BattleRenderPass();
            pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            BattleCentralRenderSystem.RegisterFeature(this, material, drawMode);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderer == null ||
                !BattleCentralRenderSystem.TryGetSubmission(
                    renderingData.cameraData.camera,
                    renderingData.cameraData.renderType,
                    out BattleDynamicMeshBackend backend))
            {
                return;
            }

            pass.Setup(backend);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            BattleCentralRenderSystem.UnregisterFeature(this);
            pass?.Dispose();
            pass = null;
        }

        private sealed class BattleRenderPass : ScriptableRenderPass
        {
            private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
            private static readonly int AtlasSliceId = Shader.PropertyToID("_AtlasSlice");
            private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            private BattleDynamicMeshBackend backend;

            public void Setup(BattleDynamicMeshBackend value)
            {
                backend = value;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (backend == null || backend.SegmentCount == 0)
                    return;

                CommandBuffer commandBuffer = CommandBufferPool.Get("NTSD Central Battle Rendering");
                int drawCount = 0;
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    if (segment.Material == null || segment.Texture == null)
                        continue;
                    propertyBlock.Clear();
                    propertyBlock.SetTexture(MainTexId, segment.Texture);
                    propertyBlock.SetFloat(AtlasSliceId, segment.AtlasSlice);
                    commandBuffer.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        segment.SubMeshIndex,
                        0,
                        propertyBlock);
                    drawCount++;
                }
                context.ExecuteCommandBuffer(commandBuffer);
                CommandBufferPool.Release(commandBuffer);
                BattleCentralRenderSystem.RecordSubmission(drawCount);
            }

            public void Dispose()
            {
                backend = null;
            }
        }
    }
}


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\Editor\BattleRenderFeatureInstaller.cs ---
#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering.Editor
{
    public static class BattleRenderFeatureInstaller
    {
        public const string RendererDataPath =
            "Assets/NTSD/New Universal Render Pipeline Asset_Renderer.asset";
        public const string MaterialPath =
            "Assets/NTSD/Materials/BattleCentralTransparent.mat";
        public const string ShaderName = "NTSD/BattleCentralTransparent";

        [MenuItem("NTSD/Battle Rendering/Install Central Render Feature")]
        public static void Install()
        {
            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
                throw new InvalidOperationException($"UniversalRendererData not found: {RendererDataPath}");

            Material material = LoadOrCreateMaterial();
            BattleRenderFeature[] existing = rendererData.rendererFeatures
                .OfType<BattleRenderFeature>()
                .Where(feature => feature != null)
                .ToArray();
            BattleRenderFeature feature;
            if (existing.Length == 0)
            {
                feature = ScriptableObject.CreateInstance<BattleRenderFeature>();
                feature.name = nameof(BattleRenderFeature);
                feature.Configure(material, BattleCentralDrawMode.OrderedChunks);
                AssetDatabase.AddObjectToAsset(feature, rendererData);
                rendererData.rendererFeatures.Add(feature);
            }
            else
            {
                feature = existing[0];
                feature.Configure(material, BattleCentralDrawMode.OrderedChunks);
                for (int index = 1; index < existing.Length; index++)
                {
                    rendererData.rendererFeatures.Remove(existing[index]);
                    UnityEngine.Object.DestroyImmediate(existing[index], true);
                }
            }

            SynchronizeFeatureMap(rendererData);
            rendererData.SetDirty();
            EditorUtility.SetDirty(rendererData);
            EditorUtility.SetDirty(feature);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(RendererDataPath, ImportAssetOptions.ForceUpdate);
            ValidateOrThrow();
            Debug.Log("[BattleRenderFeatureInstaller] Installed and validated BattleRenderFeature.");
        }

        [MenuItem("NTSD/Battle Rendering/Validate Central Render Feature")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
            Debug.Log("[BattleRenderFeatureInstaller] BattleRenderFeature validation passed.");
        }

        public static void ValidateOrThrow()
        {
            UniversalRendererData rendererData =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);
            if (rendererData == null)
                throw new InvalidOperationException($"UniversalRendererData not found: {RendererDataPath}");

            BattleRenderFeature[] features = rendererData.rendererFeatures
                .OfType<BattleRenderFeature>()
                .Where(feature => feature != null)
                .ToArray();
            if (features.Length != 1)
                throw new InvalidOperationException($"Expected one BattleRenderFeature, found {features.Length}.");
            if (!AssetDatabase.IsSubAsset(features[0]))
                throw new InvalidOperationException("BattleRenderFeature must be serialized as a renderer-data subasset.");
            if (features[0].Material == null || features[0].Material.shader == null ||
                features[0].Material.shader.name != ShaderName)
            {
                throw new InvalidOperationException("BattleRenderFeature material/shader contract is invalid.");
            }
            if (features[0].InjectionPoint != RenderPassEvent.AfterRenderingTransparents)
                throw new InvalidOperationException("BattleRenderFeature injection point must be AfterRenderingTransparents.");

            var serialized = new SerializedObject(rendererData);
            SerializedProperty featureMap = serialized.FindProperty("m_RendererFeatureMap");
            if (featureMap == null || featureMap.arraySize != rendererData.rendererFeatures.Count)
                throw new InvalidOperationException("Renderer feature map is missing or out of sync.");
            for (int index = 0; index < featureMap.arraySize; index++)
            {
                ScriptableRendererFeature feature = rendererData.rendererFeatures[index];
                if (feature == null)
                    throw new InvalidOperationException($"Renderer feature {index} is null.");
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId) ||
                    featureMap.GetArrayElementAtIndex(index).longValue != localId)
                {
                    throw new InvalidOperationException($"Renderer feature map entry {index} is stale.");
                }
            }
        }

        private static Material LoadOrCreateMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
                throw new InvalidOperationException($"Shader not found: {ShaderName}");
            if (material == null)
            {
                material = new Material(shader) { name = "BattleCentralTransparent" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else if (material.shader != shader)
            {
                material.shader = shader;
                EditorUtility.SetDirty(material);
            }
            return material;
        }

        private static void SynchronizeFeatureMap(UniversalRendererData rendererData)
        {
            var serialized = new SerializedObject(rendererData);
            serialized.Update();
            SerializedProperty featureMap = serialized.FindProperty("m_RendererFeatureMap");
            featureMap.arraySize = rendererData.rendererFeatures.Count;
            for (int index = 0; index < rendererData.rendererFeatures.Count; index++)
            {
                ScriptableRendererFeature feature = rendererData.rendererFeatures[index];
                if (feature == null ||
                    !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out _, out long localId))
                {
                    throw new InvalidOperationException($"Cannot resolve renderer feature local id at {index}.");
                }
                featureMap.GetArrayElementAtIndex(index).longValue = localId;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Simulation\SimulationTickDriver.cs ---
﻿using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Presentation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Simulation
{
    public enum SimulationDriveMode
    {
        LocalFreeRun,
        LockstepBuffered,
        Manual
    }

    /// <summary>
    /// 战斗逻辑帧配置。
    /// 逻辑帧长度固定使用 SimulationConstants.SIM_DT；这里的配置只决定外层驱动、追帧和联机预留策略。
    /// </summary>
    [System.Serializable]
    public sealed class LockstepSimulationSettings
    {
        public const int LocalFreeRunMinCatchUpTicks = 4;

        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多追多少个逻辑帧。本地模式必须允许有限追帧，避免渲染帧率低于 30 FPS 时拖慢战斗时钟。")]
        public int maxCatchUpTicksPerFrame = LocalFreeRunMinCatchUpTicks;

        [Tooltip("最多保留多少个逻辑帧的时间积压，超过后丢弃外层积压但不改变单个逻辑帧步长。")]
        public int maxBacklogTicks = 8;

        [Tooltip("联机帧同步预留：本地输入写入未来第 N 帧。当前单机可保持 0。")]
        public int inputDelayTicks = 0;

        [Tooltip("联机帧同步预留：推进前是否要求该逻辑帧的输入已经准备好。")]
        public bool requireInputFrameReady = false;

        [Tooltip("在每个逻辑 tick 尾部生成 canonical battle snapshot 和分域 checksum。")]
        public bool enableFrameChecksum = false;

        public void Normalize()
        {
            int minimumCatchUp = driveMode == SimulationDriveMode.LocalFreeRun
                ? LocalFreeRunMinCatchUpTicks
                : 1;
            if (maxCatchUpTicksPerFrame < minimumCatchUp)
                maxCatchUpTicksPerFrame = minimumCatchUp;
            if (maxBacklogTicks < maxCatchUpTicksPerFrame) maxBacklogTicks = maxCatchUpTicksPerFrame;
            if (inputDelayTicks < 0) inputDelayTicks = 0;
        }
    }

    /// <summary>
    /// 逻辑帧输入源预留接口。
    /// 当前单机输入仍由角色自己的 SimInputBuffer 消费；后续联机可在这里接入输入收齐、预测、回滚和重放。
    /// </summary>
    public interface ISimulationFrameInputProvider
    {
        bool IsFrameInputReady(int tickIndex);
        FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
        void BeforeSimTick(int tickIndex) { }
        void AfterSimTick(int tickIndex) { }
        void Reset() { }
    }

    public sealed class LocalSimulationFrameInputProvider : ISimulationFrameInputProvider
    {
        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
    }

    /// <summary>
    /// 战斗场景模拟时钟。
    /// 负责固定 30Hz 逻辑 tick，并把 C# 权威工程的 pass 顺序交给 NTSDBattleTickSystem。
    /// Unity 的 Update/LateUpdate 只作为外层驱动和表现刷新；战斗逻辑内部不能依赖 deltaTime。
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("记录每个模拟 tick 的开始和结束。")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("启动时暂停，直到 BattleBootstrap 恢复模拟。")]
        [SerializeField] private bool startPaused = true;

        [Header("帧同步时钟")]
        [SerializeField] private LockstepSimulationSettings lockstepSettings = new LockstepSimulationSettings();

        [Header("调试信息（只读）")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;
        [SerializeField][MMReadOnly] private float renderAlpha = 0f;
        [SerializeField][MMReadOnly] private int backlogTickCount = 0;
        [SerializeField][MMReadOnly] private string lastFrameChecksum = string.Empty;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;
        private BattlePresentationBackendMode _presentationBackendMode =
            BattlePresentationBackendMode.LegacyOnly;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;
        private IBattleChecksumSnapshot _lastChecksumSnapshot;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();

            CreateProductionWorld();

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void Update()
        {
            if (paused || _world == null || lockstepSettings.driveMode == SimulationDriveMode.Manual)
            {
                RefreshInspectorState();
                return;
            }

            float delta = lockstepSettings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeAccumulator += delta;

            int maxBacklogTicks = Mathf.Max(lockstepSettings.maxBacklogTicks, lockstepSettings.maxCatchUpTicksPerFrame);
            float maxAccumulator = SimulationConstants.SIM_DT * maxBacklogTicks;
            if (_timeAccumulator > maxAccumulator)
                _timeAccumulator = maxAccumulator;

            int catchUpTicks = 0;
            while (_timeAccumulator >= SimulationConstants.SIM_DT &&
                   catchUpTicks < lockstepSettings.maxCatchUpTicksPerFrame)
            {
                int nextTickIndex = _tickIndex + 1;
                if (!CanAdvanceTick(nextTickIndex))
                    break;

                _timeAccumulator -= SimulationConstants.SIM_DT;
                StepOneTickInternal(nextTickIndex);
                catchUpTicks++;
            }

            RefreshInspectorState();
        }

        private void FixedUpdate()
        {
            // 帧同步逻辑不依赖 Unity FixedUpdate。Unity 物理循环只作为引擎外层回调存在。
        }

        private void LateUpdate()
        {
            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }

            _sparkRenderer.RenderAll(_world);
            BattleCentralRenderSystem.PrepareFrame(_world);
        }

        private bool CanAdvanceTick(int tickIndex)
        {
            if (lockstepSettings.driveMode != SimulationDriveMode.LockstepBuffered &&
                !lockstepSettings.requireInputFrameReady)
            {
                return true;
            }

            return _frameInputProvider == null || _frameInputProvider.IsFrameInputReady(tickIndex);
        }

        private bool StepOneTickInternal(int tickIndex)
        {
            if (_world == null || !CanAdvanceTick(tickIndex))
                return false;

            _tickIndex = tickIndex;
            _sparkRenderFrame = tickIndex;
            if (_world.Runtime?.Flow != null)
            {
                _world.Runtime.Flow.SparkRenderFrame = _sparkRenderFrame;
            }

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            _frameInputProvider?.BeforeSimTick(tickIndex);
            FrameInputSet frameInput = _frameInputProvider?.GetFrameInput(tickIndex) ??
                                       FrameInputSet.Empty(tickIndex);
            if (frameInput.TickIndex != tickIndex)
                frameInput = FrameInputSet.Empty(tickIndex);

            _lastAppliedFrameInput = frameInput;
            _world.ApplyFrameInputSet(frameInput);
            _battleTickSystem?.RunReleaseTick(tickIndex);
            CaptureFrameChecksumIfNeeded(tickIndex, frameInput);
            _frameInputProvider?.AfterSimTick(tickIndex);

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");

            return true;
        }

        private void CaptureFrameChecksumIfNeeded(int tickIndex, FrameInputSet frameInput)
        {
            if (!lockstepSettings.enableFrameChecksum)
            {
                _lastFrameSnapshot = null;
                _lastChecksumSnapshot = null;
                lastFrameChecksum = string.Empty;
                return;
            }

            _lastChecksumSnapshot = CaptureSupportedChecksumSnapshot(_world, tickIndex, frameInput);
            _lastFrameSnapshot = _lastChecksumSnapshot as BattleParityFrameSnapshot;
            lastFrameChecksum = _lastChecksumSnapshot?.OverallChecksum ?? string.Empty;
        }

        internal static bool SupportsAuthorityFrameChecksum(SimulationWorld world)
        {
            return world != null &&
                   world.RuntimeProfileForServices == BattleRuntimeProfile.Authority400 &&
                   world.MaxRuntimeSlotsForServices == SimulationWorld.AuthorityRuntimeSlotCapacity;
        }

        internal static BattleParityFrameSnapshot CaptureSupportedFrameSnapshot(
            SimulationWorld world,
            int tickIndex,
            FrameInputSet frameInput)
        {
            return SupportsAuthorityFrameChecksum(world)
                ? world.CaptureParityFrameSnapshot(tickIndex, frameInput)
                : null;
        }

        internal static bool SupportsFrameChecksum(SimulationWorld world)
        {
            if (world == null)
                return false;

            return SupportsAuthorityFrameChecksum(world) ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended;
        }

        internal static IBattleChecksumSnapshot CaptureSupportedChecksumSnapshot(
            SimulationWorld world,
            int tickIndex,
            FrameInputSet frameInput)
        {
            if (world == null)
                return null;

            if (SupportsAuthorityFrameChecksum(world))
                return world.CaptureParityFrameSnapshot(tickIndex, frameInput);

            return world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended
                ? world.CaptureExtendedChecksumSnapshot(tickIndex, frameInput)
                : null;
        }

        private void RefreshInspectorState()
        {
            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world?.ObjectCount ?? 0;
            renderAlpha = Mathf.Clamp01(_timeAccumulator / SimulationConstants.SIM_DT);
            backlogTickCount = Mathf.FloorToInt(_timeAccumulator / SimulationConstants.SIM_DT);
        }

        public SimulationWorld World => _world;
        public int SparkRenderFrame => _sparkRenderFrame;
        public int CurrentTickIndex => _tickIndex;
        public FrameInputSet LastAppliedFrameInput => _lastAppliedFrameInput;
        public BattleParityFrameSnapshot LastFrameSnapshot => _lastFrameSnapshot;
        public IBattleChecksumSnapshot LastChecksumSnapshot => _lastChecksumSnapshot;
        public bool HasFrameChecksum => _lastChecksumSnapshot != null;
        public string LastFrameChecksum => lastFrameChecksum;
        public BattlePresentationBackendMode PresentationBackendMode => _presentationBackendMode;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void ApplySettings(LockstepSimulationSettings settings)
        {
            if (settings == null)
                return;

            lockstepSettings = settings;
            lockstepSettings.Normalize();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            if (!EnsureRuntimeProfileFromSources())
                return;

            _world.ResetRuntimeState();

            BattleMatchRuntimeState matchState = _world.Runtime?.Match;
            if (matchState != null)
            {
                matchState.LocalGameModeId = config?.gameMode?.gameModeId ?? 0;
                matchState.BattleGameModeId = config?.gameMode?.battleGameModeId ?? 1;
                matchState.BackgroundId = config?.backgroundId ?? -1;
                matchState.Difficulty = config?.difficulty ?? 2;
                matchState.Seed = config?.seed ?? 0;
            }

            _world.Rng?.Seed((uint)(config?.seed ?? 0));
            _world.Runtime?.Roster?.ApplyMatchConfig(config);
            _world.SetNeedClearInput(true);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            _world.ConfigureStageCampaigns(stageCampaigns, config?.stageSeriesId ?? 0, -1);

            _world.SetAiPhaseGate(matchState != null && matchState.BattleGameModeId == 2 ? 1 : 0);
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            _frameInputProvider = provider ?? new LocalSimulationFrameInputProvider();
            _frameInputProvider.Reset();
            _lastAppliedFrameInput = FrameInputSet.Empty(_tickIndex);
        }

        public bool StepOneTick(bool ignorePaused = false)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            CreateProductionWorld();
            _tickIndex = 0;
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            _lastAppliedFrameInput = FrameInputSet.Empty(0);
            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        private void CreateProductionWorld()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            CreateProductionWorld(settings, presentationMode);
        }

        private void CreateProductionWorld(
            BattleRuntimeWorldSettings settings,
            BattlePresentationBackendMode presentationMode)
        {
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            _world = new SimulationWorld(
                settings.Profile,
                settings.InitialRuntimeSlotCapacity,
                settings.CollisionBroadphase);
            _presentationBackendMode = presentationMode;
            _world.SetBattlePresentationBackend(presentationMode);
            _battleTickSystem = new NTSDBattleTickSystem(_world);
        }

        internal bool EnsureRuntimeProfileFromSources()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            if (WorldMatchesRuntimeSettings(_world, settings))
            {
                _presentationBackendMode = presentationMode;
                _world.SetBattlePresentationBackend(presentationMode);
                return true;
            }

            if (_world != null &&
                (_world.ClaimedRuntimeSlotCountForServices > 0 || _world.ObjectCount > 0))
            {
                Debug.LogError(
                    $"[SimulationTickDriver] Runtime profile change rejected while entities are registered. " +
                    $"Current={_world.RuntimeProfileForServices}/{_world.MaxRuntimeSlotsForServices}, " +
                    $"Requested={settings.Profile}/{settings.InitialRuntimeSlotCapacity}");
                return false;
            }

            CreateProductionWorld(settings, presentationMode);
            return true;
        }

        internal static bool WorldMatchesRuntimeSettings(
            SimulationWorld world,
            BattleRuntimeWorldSettings settings)
        {
            if (world == null || world.RuntimeProfileForServices != settings.Profile)
                return false;

            if (world.CollisionBroadphaseForServices != settings.CollisionBroadphase)
                return false;

            return world.MaxRuntimeSlotsForServices == settings.InitialRuntimeSlotCapacity ||
                   (settings.Profile == BattleRuntimeProfile.DesktopExtended &&
                    world.MaxRuntimeSlotsForServices > settings.InitialRuntimeSlotCapacity);
        }

        protected override void OnSingletonDestroyed()
        {
            BattleCentralRenderSystem.ResetRuntime();
            _world = null;
            _battleTickSystem = null;
        }
    }
}


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# P4 final architecture verification

Review the current uncommitted P4 centralized battle rendering implementation in this Unity repository.

Scope:
- Assets/NTSD/Scripts/Animation/Rendering/
- Assets/NTSD/Shaders/BattleCentralTransparent.shader
- Assets/NTSD/Materials/BattleCentralTransparent.mat
- Assets/NTSD/New Universal Render Pipeline Asset_Renderer.asset
- Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs
- P4-related changes in Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
- P3 presentation contracts in Assets/NTSD/Scripts/Simulation/Presentation/

Verify architecture against Assets/NTSD/Docs/central-battle-render-system-plan.md, especially:
- P3 authoritative order is consumed without a second sort.
- 4096 quads/chunk stays within UInt16 index limits.
- persistent mesh/buffer lifetime and stale-frame clearing are safe.
- A,A,B,A remains three contiguous segments; unresolved commands break batching.
- LegacyOnly and CentralShadowBuild never double-render; CentralOnly stays rejected until all categories are centrally owned.
- only the intended Base/world camera submits at AfterRenderingTransparents.
- the renderer feature asset/subasset wiring is valid.
- render state, texture/material ownership, disposal/domain reload, and command buffer use are sound for Unity 2022.3 URP.
- battle logic/runtime truth is not changed.

Fresh local evidence already obtained: P4 source 06:03:01.637 < Assembly-CSharp.dll 06:03:52.534 < full self-check result 06:07:46.001 PASS; dotnet build 0 errors / 42 existing warnings; installer logged Installed and validated BattleRenderFeature. Play Mode, pixel baseline, profiler, and Android are not claimed.

Return PASS only if there is no blocker. Otherwise list exact severity, file/line, failure mode, and minimal correction. Do not edit files.
