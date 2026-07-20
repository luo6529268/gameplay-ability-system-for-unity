# 集中式战斗渲染系统方案

## BATTLE-RENDER-PLAN1 状态

- **状态**：方案已确认；R1、R2A、R2B、R2C `GrowTo`、R2C-3A world 实例容量读取、R2C-3B 外部容量边界与 R2C-4 生产 Profile 激活已实施并完成代码层验证，其余阶段未实施。
- **代码状态**：`BattleRuntimeProfile` / `BattleRuntimeProfileResolver`、`Authority400` 三段 indexed binary min-heap + `nextUnused` 分配器、分页 `RuntimeSlotTable` / `RuntimeEntityHandle`、allocator/table 单调 `GrowTo`、world 实例容量读取、special/transition 扩展槽边界，以及生产 Profile、active admission 与桌面自动增长已落地；集中式渲染、Loose Quadtree 与 VRest 解耦未落地，现有 `SpriteRenderer` 未替换。
- **验证状态**：R1、R2A、R2B、R2C、R2C-3A、R2C-3B 与 R2C-4 均已有 fresh Unity 编译、完整 `BattleRuntimeSelfCheck` PASS 与 architect final review PASS。尚未完成 Play Mode、移动端真机、像素级、扩展 replay/checksum schema 或集中式渲染验收。
- **容量说明**：`400` 是 `Authority400` 兼容模式的 C# 权威槽位边界，不是所有 Unity 运行模式的全局容量上限。权威 `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Common\NtsdConstants.cs` 中的 `NtsdConstants.MaxObjects` 定义 `MaxObjects = 400`，`BattleCore\Simulation\SimulationWorld.cs:28-32` 据此创建 `Objects[400]`、`VRest[400,400]` 和 `ARest[400]`；Unity `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:39-44` 以 `MaxRuntimeSlots = 400` 镜像该契约。扩展模式的 active entity 容量与 render command 容量分开管理；每个实体可产生 `Shadow`、`Entity`、`Overlay`、`HitRecord` 等多个命令，Mesh 仍须按实际命令峰值预分配并分 chunk。
- **平台 Profile 说明**：生产解析优先级固定为“命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认值”；平台宏只提供默认 Profile，不进入战斗逻辑、最小堆、Loose Quadtree、VRest 或命中规则。设备能力降级只改变图集、纹理和渲染后端，不得改变已选 Profile 的战斗容量或结果。
- **实施边界**：`SimulationTickDriver` 的 `Awake`、`Recreate`、`ApplyMatchConfig` 共用同一 Profile 解析/创建路径；直接 `BattleTestBootstrap` 会在实体注册前重新协调晚到的 `GameConfig` Profile。生产 Profile、Mobile total active admission 和 Desktop 自动分页增长已接入；Loose Quadtree、VRest 解耦、Extended replay/checksum schema 与集中式渲染仍待后续批次。

### 2026-07-20 R1 第一批实施记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| Profile resolver | **已实施 / 已验证** | 支持显式覆盖 > 配置值 > 平台默认；平台默认由 Unity 条件编译符号选择。Editor/其他平台回落 `Authority400`，Android Player 为 `MobileExtended`，Standalone Player 为 `DesktopExtended` |
| `Authority400` 最低空闲槽分配 | **已实施 / 已验证** | 以 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused` 保留 roster、stage、dynamic band 语义；支持按索引移除、释放回收和最低槽确定性分配 |
| 正式 runtime 接线 | **兼容模式已接入** | `SimulationWorld` 仍显式固定为 `Authority400`，本批不改变 400-slot 行为边界，也不自动启用平台扩展模式 |
| 扩展容量与空间索引 | **基础设施部分已实施 / 生产未启用** | R2A 已实现独立分页 `RuntimeSlotTable` 与 generation handle；`MobileExtended`、`DesktopExtended` 生产接线、桌面动态增长、1000 active admission、AI 迁移和 Loose Quadtree 均保留为后续批次 |

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
| Extended checksum/parity | **边界已实施 / 已验证** | Extended Driver checksum 输出跳过/为空；direct parity capture 仍只接受 `Authority400 + 400`，拒绝 `DesktopExtended/512` 与其他非 authority 组合 |
| 后续阶段 | **未实施 / 未启用** | Loose Quadtree、VRest 解耦、Extended replay/checksum schema 与集中式渲染仍保持后续计划 |

R2C-4 fresh 验证：相关源码时间 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` 结果 `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 42 existing warnings**；architect final review **PASS**。

## Runtime 容量与空间索引阶段决策

**状态：方案已确认 / R1、R2A、R2B、R2C、R2C-3A、R2C-3B 与 R2C-4 runtime 基础设施已实施并验证 / 其余未实施。** 本节是运行时容量与 broadphase 的设计边界，不改变 C# 权威战斗逻辑；当前已落地 Profile resolver、生产 Profile 激活、跨槽区 active admission、桌面自动增长、`Authority400` 分配器、分页槽表、generation 句柄、单调 `GrowTo`、world 按实例容量读取、外部 special/transition 容量边界和 authority parity guard，并已将该槽表接入统一 Driver 路径。Loose Quadtree、VRest 解耦、Extended replay/checksum schema、内存预算和目标设备参数仍需在后续实现阶段验证。

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

- 空间索引使用 X/Z 平面的 **Loose Quadtree**；逻辑实体、AI 范围查询和 itr/bdy 碰撞查询共享空间索引，但查询服务与候选规则分开，不能用 AI 范围结果替代碰撞候选。
- 实体中心点采用严格的**半开区间**归属（左/下含、右/上不含，边界规则全局一致），保证一个中心点只属于一个子节点。
- 实体 AABB 只有在完全被节点的 loose 范围容纳时才留在该节点；超出 loose 范围才迁移到父节点或重新选择的节点。
- 默认参数仅作为 profiling 基准，不能视为最终性能结论：`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 6..8`。目标设备和真实战斗分布 profiling 后再调整。
- 更新采用增量策略：实体只在离开当前节点 loose 范围时迁移；未离开时不重建树。生成、销毁、分页复用和跨边界移动必须在确定的 mutation boundary 更新索引。
- broadphase 每 tick 先按 `RuntimeSlot` 升序遍历 active attacker；各 attacker 查询得到的候选先去重为 `(minSlot, maxSlot)` pair，再在全局按 `(minSlot, maxSlot)` 升序排序后交给现有 narrow phase。保留 C# 的 candidate 截断、距离/类型 tie 顺序和 pair 消费规则；空间索引不得改变命中规则、VRest 计时或最终逻辑结果。

### VRest 与 Parity 边界

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

已确认的设计决策是：保留 `Authority400` 兼容模式；移动端全部槽区合计最多 1000 active 且第 1001 个确定性拒绝；桌面从 512 开始按 256-slot 页自动增长并受技术预算约束；空闲槽使用二叉最小堆 + `nextUnused`；空间 broadphase 使用 X/Z Loose Quadtree；VRest 与 broadphase 解耦；详细 parity snapshot 不进入生产热路径。生产 Profile 优先级为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认，设备能力只降级表现资源/后端；三个 Profile 共用同一套确定性 runtime 算法。

截至 2026-07-20，**R1-R2C-4 的 runtime 容量阶段**达到“已实施 / 编译通过 / self-check 通过 / architect review 通过”：Profile resolver、生产 Driver/Profile 激活、默认容量、Mobile total active admission、Desktop 自动增长、最低槽复用、AI snapshot 扩展、256 槽惰性分页 `RuntimeSlotTable`、generation handle、world/外部实体容量读取，以及 strict authority parity capture guard 均已落地；Extended Driver checksum 当前明确跳过/为空。Loose Quadtree、VRest 解耦、Extended replay/checksum schema 及全部集中式渲染模块仍是 **方案已确认 / 未实施 / 未验证**。具体 API、Shader、装箱算法、内存预算、命令字段、chunk 大小、URP 注入点和最终迁移顺序仍需实施前核验，并持续区分“已确认 / 待确认 / 已实施 / 已验证”。
