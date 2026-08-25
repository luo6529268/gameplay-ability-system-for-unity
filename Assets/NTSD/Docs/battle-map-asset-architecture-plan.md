# NTSD 多地图 Asset 架构与逻辑边界实施计划

> 计划 ID：BATTLE-MAP-ASSET-ARCHITECTURE-001  
> 版本：0.1 — SUPERSEDED BEFORE CODE  
> 状态：SUPERSEDED / 未开始脚本实施  
> 最后更新：2026-08-25  
> 适用范围：Unity NTSD 战斗场景的地图选择、地图逻辑数据、地图表现资源、关卡边界作者工具与后续联机身份。  
> 不替代：Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md 中的 C++ Release 战斗规则对齐主线。

## 修订说明：本计划已不执行

2026-08-25 用户澄清：所说的可行走区域就是现有 BoundaryWall 和 BoundaryWallManager 已经实现的任意多边形行为。此前版本错误地把“将现有 polygon 数据按 Map ID 配置化”扩大为“新增 polygon battle physics”，因此错误加入了 C++ Release 审计、矩形首发、StageFingerprint 与 M0 至 M7 前置。

上述内容在没有写任何代码前已被撤销。当前唯一执行计划是：

Assets/NTSD/Docs/battle-map-boundary-asset-configuration-plan.md

历史 M0 至 M7 仅保留用于说明被纠正的范围，不得执行。

## 1. 最终目标

一场战斗在 Tick 0 前通过稳定 Map ID 选择一张地图，并生成不可变的逻辑地图快照。

    Map ID
       └─ BattleMapCatalog
          ├─ BattleMapLogicDefinition
          │  └─ BattleMapRuntimeSnapshot
          │     └─ SimulationWorld、Stage、出生、随机区域、StageFingerprint
          └─ BattleMapPresentationDefinition
             └─ Bg、装饰、相机取景和本地平台表现

逻辑地图决定会影响战斗模拟的地图数据。表现地图只决定背景图片、装饰和本地显示资源。Catalog 负责检查同一 Map ID 的逻辑/表现配对。RuntimeSnapshot 在开始战斗前一次性生成，逻辑 Tick 内只读取其紧凑不可变数据。

这套结构要同时满足：

1. Editor 中可以为不同地图编辑不同可行走区域；
2. Windows 与 Android 可以使用不同的本地背景构图或底部黑色覆盖；
3. 平台视觉差异不改变战斗逻辑、位置、碰撞或跨平台联机身份；
4. 每张地图的逻辑身份可进入后续 lockstep 的 StageFingerprint；
5. 可从现有单一场景和全局 Stage fallback 分阶段迁移，不一次性重写战斗。

## 2. 当前事实、推断与未知

### 2.1 已观察事实

| ID | 现有位置 | 当前行为 | 架构影响 |
|---|---|---|---|
| MAP-F-001 | Assets/NTSD/Scripts/LevelEditor/BoundaryWall.cs | 支持多边形、包含测试和 Editor 作者操作。 | 可复用为作者工具候选，不是地图资产本身。 |
| MAP-F-002 | BoundaryWallManager 的 IsRectWalkable / IsPointWalkable | 按已启用的 Scene BoundaryWall 联合区域判断。 | 逻辑真相依赖 Scene 扫描，未绑定 Map ID。 |
| MAP-F-003 | BoundaryWallManager 的 TryGetBattleStageRuntime | 只由 Scene 边界外接矩形导出宽度、Z 最小值和 Z 最大值。 | 丢失多边形、Map ID 和世界 X 原点。 |
| MAP-F-004 | SimulationWorld.StageRender.partial.cs 的 ResolveUnityStageRuntime | 先读 GameConfig，再以 BoundaryWall 场景结果覆盖部分 Stage。 | 没有已选择地图的显式输入。 |
| MAP-F-005 | Assets/NTSD/Scripts/App/GameConfig.cs | BattleStageWidthPx、Z 范围和透视参数是全局 fallback。 | 不能作为多地图最终真相。 |
| MAP-F-006 | LockstepSessionIdentity.cs | 已有 StageFingerprint 并参与 IdentityFingerprint。 | 尚未证明它已由正式地图资产计算并注入。 |
| MAP-F-007 | Bg、BattleBackgroundPlatformPresentation、Camera 表现链 | 负责背景和平台本地表现。 | 不能进入地图逻辑 hash，也不能反写 Stage。 |

### 2.2 合理推断

- 当前 BoundaryWall 多边形能力可作为作者工具或随机采样辅助；没有足够证据证明全部角色移动、击退、投掷物和特殊对象已把它作为正式 C++ 对齐的物理阻挡。
- 当前 Scene 外接矩形无法表达每张地图独立的逻辑身份，也无法保证 Editor 作者坐标与运行时逻辑像素一致。
- 同一逻辑地图在 Windows 和 Android 使用不同相机取景、黑色覆盖或背景显示是安全的，前提是表现层不写回 runtime。

### 2.3 必须保留为 UNKNOWN 的事项

| 事项 | 原因 | 关闭阶段 |
|---|---|---|
| C++ Release 的 stage X 原点、宽度、Z 边界坐标合同 | 不能凭 Unity Scene 世界坐标猜测。 | M0 |
| 正式角色、武器、击退、投掷物是否都应受任意凹多边形阻挡 | 必须区分既有 C++ 规则与 Unity 新玩法。 | M0 / M6 |
| 每组现有 BoundaryWall 对应哪张地图 | 迁移时不能自动猜测。 | M2 / M7 |
| 生产 session 创建点如何提供 StageFingerprint | 不能只看到字段存在就认为接线已完成。 | M0 / M5 |

UNKNOWN 是合法结论；不能用当前 Unity 行为、旧 C# 结论或视觉需要自动补全。

## 3. 架构边界

### 3.1 逻辑地图真相

BattleMapLogicDefinition 与冻结后的 BattleMapRuntimeSnapshot 可以包含：

- 稳定 Map ID、逻辑 schema、逻辑 revision；
- 经确认的 Stage 宽度、Z 范围、透视逻辑参数；
- 经确认的出生点、随机区域与地图专属 deterministic 规则；
- 进入 StageFingerprint 的 canonical payload；
- 经 M6 单独批准后才会参与模拟的 polygon 数据。

以下数据绝不属于逻辑真相：

- Unity Asset GUID、Instance ID、资源路径；
- Sprite、Texture、材质、sorting；
- Transform、Camera size、Camera rect、屏幕比例；
- Windows/Android、本地黑色覆盖、Editor preview；
- Bg bounds 或用户显示配置。

### 3.2 表现地图真相

BattleMapPresentationDefinition 可以包含：

- 与 Map ID 相同的表现身份和表现 revision；
- Background Sprite、装饰 prefab、音效、特效和世界表现锚点；
- 仅表现用 sorting、Windows/Android 显示配置。

它不允许修改 SimulationWorld、Stage、实体位置/速度/朝向、命中、随机数、输入、checksum 或 FrameInputSet。

### 3.3 数据与性能边界

- ScriptableObject 的 Inspector List 只用于加载和编辑。
- 进入 Tick 0 前，逻辑数据必须转换为数组或扁平连续缓冲。
- Tick 热路径不得扫描 Scene、读取 ScriptableObject、使用 AssetDatabase、LINQ 或动态创建 List/Dictionary。
- Catalog 的查找表只允许在加载/preflight 期间存在；战斗中只保存一个选中 RuntimeSnapshot。
- 新增地图类型全部使用独立完整类，不新增 partial class。
- 地图只能在战斗启动或显式 reset/load 边界切换，不能在 tick 中更换。

### 3.4 当前范围外

- 不重写 C++ Release 的 battle logic；
- 不自动实现任意多边形碰撞、障碍物寻路或 AI 行为树；
- 不处理 T8 默认 stage.dat 部署；
- 不实现 Socket、ACK、房间、登录、重连或网络库；
- 不借地图系统改背景 PPU、Bg Transform、相机、输入、ECS/SoA、对象池或集中渲染。

## 4. 建议的数据合同

### 4.1 BattleMapLogicDefinition

建议作为独立 ScriptableObject，字段全部使用确定性、跨平台可复核的逻辑单位。

| 字段组 | 内容 | 约束 |
|---|---|---|
| Identity | MapId、SchemaVersion、LogicRevision | ID 稳定、可读、规范化；不能由路径或 Unity ID 生成。 |
| Stage rectangle | StageWidthPx、ZMinPx、ZMaxPx、已确认透视值 | 只用整数逻辑像素；不由 Camera 或 Sprite bounds 推导。 |
| Coordinate bridge | M0 确认后的作者 world 到逻辑像素转换资料 | 只服务于编辑/加载转换，不在 tick 中读取 Transform。 |
| Spawn | 整数 X/Z 点、spawn group、权重 | 固定排序、固定默认值，进入 hash。 |
| Random regions | 整数矩形或已确认区域 | 只在 C++/Unity 合同闭合后接入正式 RNG。 |
| Polygon authoring | 整数 X/Z 顶点 | M1/M2 可保存和预览；M6 前明确标为 AUTHORING_ONLY。 |
| Rules | 经确认的 map-specific battle rules | 无 C++ 证据或用户新玩法规格时不臆造。 |

建议的 logic MapFingerprint 输入顺序：

1. 逻辑 schema；
2. 规范化 Map ID；
3. LogicRevision；
4. Stage rectangle 与已确认透视值；
5. 固定排序的出生点、随机区域；
6. 只有 polygon 真正参与模拟时，才按 polygon index、vertex index 写入整数 X/Z；
7. 经确认参与模拟的地图规则。

Sprite、Texture、Prefab、Camera、屏幕方向、本地安全区和平台表现不进入 fingerprint。

### 4.2 BattleMapPresentationDefinition

该独立 ScriptableObject 也保存 Map ID，但不进入 logic hash。

| 字段组 | 内容 | 限制 |
|---|---|---|
| Identity | MapId、PresentationRevision | 必须与配对 LogicDefinition 完全一致。 |
| Background | Background Sprite、背景表现 profile | 不能包含逻辑边界或 spawn。 |
| Decoration | 装饰 prefab、音效、特效引用 | 不能成为 SimulationWorld 真相。 |
| Visual anchor | 表现锚点和 sorting | 只能用于渲染，不回写逻辑像素。 |
| Platform presentation | 本地视觉参数 | 不进入 lockstep。 |

### 4.3 BattleMapCatalog

Catalog 是唯一选择入口，至少拒绝：

1. 空或未规范化 Map ID；
2. 重复 Map ID；
3. 无 LogicDefinition 的条目；
4. Map ID 不一致的 presentation；
5. 不可计算 fingerprint 或非法 Stage rectangle；
6. 出生点/随机区域越出已确认逻辑范围；
7. 顶点数不足、重复点、零面积的 authoring polygon。

### 4.4 BattleMapRuntimeSnapshot

选中后一次构建、不可变，持有：

- Map ID 的 canonical identity；
- logic MapFingerprint；
- Stage rectangle 和透视值；
- 已排序的 spawn / region 数组；
- schema、revision；
- 仅 M6 批准后才有扁平化 polygon 运行时数据。

它不持有 UnityEngine.Object、ScriptableObject、GameObject、Transform 或可变 List。

## 5. 迁移策略

| 当前来源 | 当前职责 | 目标角色 | 迁移限制 |
|---|---|---|---|
| GameConfig 全局 Stage 字段 | 无地图时 fallback | 仅保留为迁移期 legacy fallback | 不得覆盖已选择 snapshot。 |
| BoundaryWall / BoundaryWallManager | Scene 作者、联合 polygon、外接矩形导出 | M2 的可选作者桥 | 不能在 tick 中扫描或自动写资产。 |
| SimulationWorld Stage refresh | 从 GameConfig / Scene 读取 | M3 后优先读冻结 snapshot | 只在初始化/reset 边界发生。 |
| LockstepSessionIdentity.StageFingerprint | 已有 identity 字段 | M5 接入 logic fingerprint | 表现字段不得参与。 |
| Bg 与背景表现组件 | 视觉背景 | 读取 PresentationDefinition 或显式映射 | 不改变地图逻辑。 |

## 6. 分阶段 Work Package

状态口径：NOT_STARTED、PLANNED、IN_PROGRESS、CODE_WRITTEN、COMPILE_PASS、FOCUSED_TEST_PASS、RUNTIME_PENDING、VERIFIED、BLOCKED。任何状态都必须有实际证据。

| 阶段 | Work Package | 状态 | 前置 | 最终产出 |
|---|---|---|---|---|
| M0 | MAP-M0-001 坐标、范围、fingerprint 合同 | PLANNED | 无 | 可实施的逻辑坐标/范围/身份定义 |
| M1 | MAP-M1-001 Asset 与 Catalog 基础类型 | NOT_STARTED | M0 | Logic、Presentation、Catalog 资产类型 |
| M2 | MAP-M2-001 BoundaryWall 作者桥 | NOT_STARTED | M0、M1 | 显式导入/导出、预览和校验 |
| M3 | MAP-M3-001 选择、冻结 snapshot、矩形 Stage 接线 | NOT_STARTED | M0、M1 | 单地图逻辑快照 |
| M4 | MAP-M4-001 出生、随机区域、reset/preflight | NOT_STARTED | M3 | map-specific spawn/RNG 输入 |
| M5 | MAP-M5-001 lockstep 地图 identity | NOT_STARTED | M3、M4 | MapFingerprint 到 StageFingerprint |
| M6 | MAP-M6-001 任意 polygon simulation boundary | DECISION_REQUIRED | M0 至 M5 | 仅批准后处理的 gameplay 扩展 |
| M7 | MAP-M7-001 默认地图迁移和验收 | NOT_STARTED | M1 至 M5，M6按决定 | 第一张显式地图收口 |

### M0 — MAP-M0-001：坐标、范围与 fingerprint 合同

**Goal**

闭合 Editor 世界坐标、逻辑像素、Stage rectangle、出生、rounding、origin 与 fingerprint 的语义。

**Scope**

- 只读追踪 C++ Release live stage、physics、boundary、spawn 调用链；
- 只读追踪 Unity Stage、BoundaryWall、NTSDRenderSpace、lockstep identity；
- 输出字段表，逐项标 VERIFIED、INFERRED 或 UNKNOWN；
- 决定矩形 Stage 首发与 polygon 的正式状态。

**解决方案**

- 不预设 world X 原点；先从 C++ / Unity 使用点闭合映射；
- 定义单向整数化作者转换，运行时不反向读 Transform；
- 把可编辑 polygon 与正式 battle boundary 明确分开。

**边界**

不写 C++、Unity gameplay、Scene、Asset、DAT 或背景表现。

**验证**

源码调用链可定位；每字段有证据等级；写出矩形/多边形决策表。

**停止条件**

C++ 坐标语义无法闭合；需要改 tick/pass；或要让 polygon 立即成为 physics 但无授权。

**Out of scope**

所有 Asset 代码、Stage 注入、spawn、网络接线。

### M1 — MAP-M1-001：Logic Asset、Presentation Asset、Catalog

**Goal**

新增数据资产类型和 Editor 验证，不接入 battle runtime。

**Scope**

- BattleMapLogicDefinition；
- BattleMapPresentationDefinition；
- BattleMapCatalog；
- canonical fingerprint pure function；
- focused EditMode validation。

**解决方案**

逻辑资产使用整数和固定排序；表现资产只保存表现引用；Catalog 以显式数组序列化、加载时校验；新增类型均为独立类；polygon 首先是 AUTHORING_ONLY。

**边界**

不写 GameConfig、BoundaryWallManager、SimulationWorld、Scene、Bg、Camera 或 runtime selection。

**验证**

空/重复/不匹配 ID fail closed；hash 重载稳定；presentation 变化不影响 logic hash；compile、focused tests 和 ledger 通过。

**停止条件**

M0 字段未确定，或建立类型必须先改 Stage writer。

### M2 — MAP-M2-001：BoundaryWall 作者桥

**Goal**

让 BoundaryWall 成为显式作者工具，而不是 runtime 地图真相。

**Scope**

- 指定 Map ID 的导入、导出、预览和校验；
- 整数坐标转换；
- AUTHORING_ONLY 可视标记。

**解决方案**

保留 BoundaryWall 为 Editor UI 候选。仅用户点击导入/导出时写资产；运行时从不自动同步 Scene。

**边界**

禁止 OnValidate、Update 或游戏运行时自动写 Asset；禁止从背景 Sprite bounds 推导可行走区域；禁止修改现有 BoundaryWallManager runtime 结果。

**验证**

重复导出得到同一排序和整数数据；非法 polygon/spawn 有错误；不会保存用户 dirty Scene。

**停止条件**

M0 bridge 未确认、必须立即启用 polygon physics，或 BoundaryWall 与 Map ID 无法由用户确认。

### M3 — MAP-M3-001：Map 选择、冻结快照、矩形 Stage 接线

**Goal**

Tick 0 前按 Map ID 构建 RuntimeSnapshot，并让现有 Stage 读取经确认的矩形逻辑数据。

**Scope**

- BattleMapHost 或经审计确认的单一宿主；
- Catalog preflight 与选中 Map ID；
- RuntimeSnapshot 构建、冻结和 reset；
- Stage snapshot 的最小 adapter；
- legacy GameConfig/BoundaryWall fallback 优先级。

**解决方案**

Host 只在 battle-start/reset 边界运行。成功选择后写入 world-owned Stage snapshot，后续 tick 禁止 Scene refresh 覆盖。无 Map ID、catalog mismatch 或非法数据必须在 Tick 0 前 fail closed。

**边界**

不改主 tick、C++ pass、实体物理公式、碰撞顺序、背景相机、PPU、Bg Transform 或 polygon physics。

**验证**

Map A/B 交替启动和 reset 稳定；战斗中 Scene/Bg/Camera 改动不影响冻结 Stage；legacy 无地图路径有显式 fallback 诊断；compile、focused、定向 Battle Scene。

**停止条件**

需要每 tick Scene scan；M0 显示矩形不足；或接线迫使保存用户 Scene。

### M4 — MAP-M4-001：出生、随机区域与 preflight

**Goal**

把已确认 map-specific spawn / region 数据接入冻结 snapshot。

**Scope**

- spawn group、出生点、候选区域；
- deterministic RNG 前置条件；
- reset/pool recycle 后 snapshot 保持；
- 空区域、超界、slot exhaustion 诊断。

**解决方案**

区域使用 snapshot 连续数组；随机仍使用既有 battle deterministic RNG 且保留调用顺序；无有效数据只走明确 fallback，不能随机扫描 Scene。

**边界**

不重写 random weapon、opoint、hit、对象池或部署 stage.dat。

**验证**

同 seed、Map ID、input journal 产生同样结果；表现变化不影响结果；focused map fixture、self-check 和必要 Battle Scene 验证。

**停止条件**

需要改变 RNG 时序，或规则实际是 DAT/角色专有而不是地图规则。

### M5 — MAP-M5-001：lockstep identity 与 fail-closed

**Goal**

把选中地图的 logic MapFingerprint 接入正式 StageFingerprint/session identity。

**Scope**

- RuntimeSnapshot 到 StageFingerprint；
- 正式 session 创建点、start barrier、packet validation；
- 可诊断的 Map mismatch reason。

**解决方案**

复用 LockstepSessionIdentity.StageFingerprint，不另起竞争身份通道。逻辑 payload 改动应 mismatch；表现、屏幕、Camera、Android 黑区不应 mismatch；只在 tick 0 前计算。

**边界**

不实现 Socket、ACK、jitter buffer、房间、重连或网络库。

**验证**

同 logic asset hash 稳定；logic 改动 mismatch；presentation 改动不 mismatch；start barrier fail closed；focused lockstep tests、self-check 和单机回归。

**停止条件**

找不到生产 session 创建点，或接线必须变更协议/服务器业务。

### M6 — MAP-M6-001：可选任意 polygon simulation boundary

**Goal**

仅在用户明确要求和 C++ evidence 足够时，让 polygon 成为正式移动/对象边界。

**为什么单独处理**

它会影响角色移动、跳跃、击退、投掷物、武器、opoint、出生、AI 和随机区域，绝不能作为“能画 polygon”的附带实现。

**前置**

1. C++ Release live source 证据，或用户明确批准 Unity 新玩法；
2. 受影响实体类型和 pass 边界清单；
3. integer geometry、边缘含义、凹 polygon、holes、knockback fallback 合同；
4. 独立 Change Record 和测试矩阵。

**停止条件**

任一实体的规则仍是 UNKNOWN，或需要改 candidate/hit 顺序时立即停止。

### M7 — MAP-M7-001：默认地图迁移与收口

**Goal**

将当前沙漠场景迁成第一张显式 Map ID，并完成 legacy 对照、场景验收和留痕。

**Scope**

- logic/presentation pair、Catalog entry、Battle Scene map selection；
- legacy fallback 与 snapshot 的 Stage、spawn、fingerprint 对照；
- Windows/Android 不同本地显示、相同逻辑 identity 验收。

**边界**

不能保存用户未确认的 NTSD_Battle.unity；不能擅自部署 stage.dat；不能把背景正确显示说成 C++ 全量战斗对齐；M6 未批准时 polygon 仍只是作者数据。

**完成条件**

第一张地图被显式选中、冻结、验证；legacy fallback 保留/废弃状态明确；剩余未验证项如实标为 RUNTIME_PENDING 或 BLOCKED。

## 7. 每次修改与进度更新协议

### 修改脚本前

每个实际实施包必须依次：

1. 更新本计划阶段状态；
2. 创建或更新该阶段 Task Contract；
3. 创建唯一 Change Record，例如 MAP-M3-HOST-001；
4. 同步 docs/ai/CHANGE-LEDGER.md、docs/ai/STATE.md 和当前 Handoff；
5. 之后才允许改 C#、Editor、test、shader 或 build 脚本。

### 修改后

立即记录：

- 实际文件和符号；
- 改前/改后职责；
- 逻辑/表现边界；
- 实际 compile、focused test、自检、Scene 验证；
- 未验证项、风险、依赖和回滚方式；
- 真实状态。

同时更新本计划、State、Ledger 和 Handoff。

### 阶段关闭前

1. 运行 Tools/Validate-ChangeLedger.ps1；
2. 检查 scoped git diff，保留用户 dirty 文件；
3. 运行最窄相关验证；
4. 有运行时行为变更时使用当前 Unity Editor 做指定 Battle Scene 验证；
5. 无 C++ source 或 runtime evidence 时保持 RUNTIME_PENDING/BLOCKED。

## 8. 首个推荐实施包

推荐先执行 M0 的只读合同审计，随后才进入 M1 的数据资产实现。

当前默认产品边界：

- 首批地图先落地经确认的矩形 Stage；
- polygon 可以保存、编辑、预览；
- polygon 在 M6 前不参与正式 battle physics。

该顺序可先完成不同 Map ID、不同矩形可行走范围、不同出生区域、不同背景资源和跨平台不串逻辑，同时避免把关卡作者工具扩张为未经验证的全局物理重写。

## 9. 当前进度

| 日期 | 项目 | 状态 | 证据 |
|---|---|---|---|
| 2026-08-25 | BATTLE-MAP-ASSET-ARCHITECTURE-001 | DOCUMENTED | 本文件、Task、D-016、STATE 和 Handoff 已建立；本次未修改任何 production/test 脚本、Scene、Asset、DAT、配置、C++ 或服务器。 |
