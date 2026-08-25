# BATTLE-MAP-ASSET-ARCHITECTURE-001 — 多地图 Asset 架构总计划合同

> 当前状态：SUPERSEDED BEFORE CODE / NO CODE  
> 创建日期：2026-08-25  
> 关联计划：Assets/NTSD/Docs/battle-map-asset-architecture-plan.md

## Superseded

本合同在没有任何脚本、Scene、Asset、DAT、C++ 或服务器修改前被用户纠正。它错误地把已有 BoundaryWall/BoundaryWallManager 任意 polygon 行为视为需要新增 C++ 物理审计的功能。

不得执行本文 M0 至 M7。当前替代合同为 docs/ai/TASKS/BATTLE-MAP-BOUNDARY-ASSET-001.md。

## Goal

在不把本地背景、Camera、Android 底部黑色覆盖或 Editor 作者坐标混入战斗真相的前提下，建立可分阶段实施的多地图 Asset 架构。

每场战斗在 Tick 0 前选定一个稳定 Map ID，生成不可变的逻辑 RuntimeSnapshot。Windows 与 Android 可以显示不同的本地表现，但必须共享同一逻辑地图和 StageFingerprint。

## 本总计划包的 Scope

本包当前只完成：

- 现状盘点；
- 目标数据合同；
- M0 至 M7 边界；
- Change Record、Ledger、State、Handoff 留痕流程。

本包不修改任何 C#、Editor、test、shader、Scene、Asset、DAT、配置、C++ 或服务器代码。

## Authority / Evidence

- 用户明确需求：一个 Asset 保存 Map ID 与地图可行走/逻辑区域；另一个 Asset 保存同一 Map ID 与地图资源；
- 根 AGENTS.md 的 C++ Release authority 和 Unity 表现隔离规则；
- BoundaryWall、BoundaryWallManager、SimulationWorld Stage、GameConfig、LockstepSessionIdentity 当前源代码；
- 现有背景表现合同 CAMERA-PLATFORM-BACKGROUND-001。

当前没有把 BoundaryWall polygon、GameConfig fallback 或背景表现写成已经获得 C++ gameplay authority 的规则。

## 当前已确定边界

1. Logic Asset 是 simulation / identity 真相；Presentation Asset 不是。
2. Map ID 不能是资源路径、Asset GUID、Instance ID 或 Sprite 名称。
3. Catalog 是唯一配对、校验和选择入口。
4. RuntimeSnapshot 在 Tick 0 前冻结；tick 内不扫描 Scene、不读取 ScriptableObject。
5. 首批正式 runtime 只接入经 M0 重新确认的矩形 Stage。
6. polygon 在 M6 前只能是 AUTHORING_ONLY 作者/预览资料。
7. 新类型使用独立完整类，不新增 partial。
8. logic fingerprint 不含背景、平台、Camera、屏幕、黑色覆盖或本地表现。

## Work Package 划分

| 阶段 | ID | Goal | 进入条件 | 状态 |
|---|---|---|---|---|
| M0 | MAP-M0-001 | 坐标、矩形语义和 fingerprint 合同审计 | 无 | PLANNED |
| M1 | MAP-M1-001 | LogicDefinition、PresentationDefinition、Catalog | M0 闭合 | NOT_STARTED |
| M2 | MAP-M2-001 | BoundaryWall 作者桥、导入导出、资产校验 | M0、M1 | NOT_STARTED |
| M3 | MAP-M3-001 | Map ID 选择、冻结 snapshot、矩形 Stage 接线 | M0、M1 | NOT_STARTED |
| M4 | MAP-M4-001 | 出生、随机区域、reset/preflight | M3 | NOT_STARTED |
| M5 | MAP-M5-001 | StageFingerprint 和 lockstep fail-closed | M3、M4 | NOT_STARTED |
| M6 | MAP-M6-001 | 任意 polygon 成为正式模拟边界 | 独立决定和证据 | DECISION_REQUIRED |
| M7 | MAP-M7-001 | 默认沙漠地图迁移、运行时验收、收口 | M1 至 M5 | NOT_STARTED |

## 后续包合同

### MAP-M0-001

**Goal**：只读闭合 C++ Release 和 Unity 的 stage coordinate、stage rectangle、spawn、origin、rounding、StageFingerprint 输入合同。

**Scope**：读取 C++ Release live path；读取 Unity Stage、BoundaryWall、NTSDRenderSpace、spawn、lockstep source；写 VERIFIED/INFERRED/UNKNOWN 表。

**Files likely involved**：C++ stage/physics/entity movement source；Assets/NTSD/Scripts/LevelEditor；Assets/NTSD/Scripts/Simulation；对应 Task、Handoff、State、Decision。

**Verification**：调用链和字段闭合；首发矩形与 polygon AUTHORING_ONLY 决策明确；不改脚本。

**Stop conditions**：C++ 坐标语义无法闭合；需要改变 tick/pass；或希望 polygon 立即成为 physics 但没有独立授权。

**Out of scope**：Asset 类型、Scene、表现、runtime 注入、spawn、lockstep 接线。

### MAP-M1-001

**Goal**：新增 Logic Asset、Presentation Asset、Catalog 基础类型，尚不接入 battle runtime。

**Scope**：资产类型、canonical fingerprint pure function、Editor validation、focused EditMode tests。

**Verification**：Map ID 唯一；配对一致；logic hash 稳定；presentation 变化不改变 hash；compile、focused tests、ledger。

**Stop conditions**：M0 字段未确定，或必须先改 Stage writer。

**Out of scope**：Tick、Scene、Bg、Camera、BoundaryWall runtime、网络。

### MAP-M2-001

**Goal**：把 BoundaryWall 变成显式作者桥，而不是 runtime 地图真相。

**Scope**：选中 Map ID 的导入、导出、预览、整数转换和校验。

**Verification**：重复导出稳定；非法 polygon/spawn 有错误；无自动 Scene/Asset 写入。

**Stop conditions**：作者和 Map ID 关系不清晰，或需要立即启用 polygon physics。

**Out of scope**：角色阻挡、投掷物、AI、hit/collision。

### MAP-M3-001

**Goal**：Tick 0 前选择 Map ID、冻结 RuntimeSnapshot，并将经确认的矩形 Stage 写入现有 Stage snapshot。

**Scope**：Host、Catalog preflight、snapshot、legacy fallback 优先级。

**Verification**：Map A/B 启动/reset 稳定；战斗中 Scene/Bg/Camera 改动不影响 snapshot；无效 Map ID fail closed。

**Stop conditions**：需要每 tick Scene refresh，或 M0 显示矩形不足。

**Out of scope**：polygon physics、真实网络、背景重构。

### MAP-M4-001

**Goal**：接入已确认的 map-specific spawn/region 数据。

**Scope**：spawn group、确定性区域、RNG 前置、reset/preflight。

**Verification**：同 seed、Map ID、input journal 稳定；presentation 不影响结果。

**Stop conditions**：需要改变 RNG 调用顺序，或规则实为角色/DAT 专有。

**Out of scope**：random weapon、opoint、hit 主逻辑重写。

### MAP-M5-001

**Goal**：把 logic MapFingerprint 接入现有 StageFingerprint/session identity。

**Scope**：start barrier、packet validation、Map mismatch reason。

**Verification**：logic 改动 mismatch，presentation 改动不 mismatch，start barrier fail closed。

**Stop conditions**：生产 identity 创建点无法确定，或需提前实现真实网络业务。

**Out of scope**：Socket、ACK、房间、重连、S0 之后的服务器。

### MAP-M6-001

**Goal**：仅在明确批准后，让整数 polygon 参与正式 simulation boundary。

**Scope**：受影响实体、geometry、movement、knockback、spawn、random 的独立审计与实现。

**Verification**：必须有 C++ Release source/runtime 证据，或用户明确的新玩法规格；所有相关实体经过定向运行时验证。

**Stop conditions**：任一实体规则未知，或会改变 candidate/hit 顺序。

**Out of scope**：把作者 polygon 当成已实现物理。

### MAP-M7-001

**Goal**：迁移第一张沙漠地图并完成验收/文档收口。

**Scope**：默认 logic/presentation pair、Catalog、Battle Scene 选择、legacy 对照、Windows/Android 本地表现下的同一 identity。

**Verification**：compile、focused tests、self-check、定向 Battle Scene、ledger、handoff。

**Stop conditions**：需要保存用户 dirty Scene、擅自部署 stage.dat 或扩大为 polygon gameplay。

## 留痕合同

每个脚本实施包开始前必须创建或更新：

1. 专用 Task Contract；
2. 唯一 Change Record；
3. docs/ai/CHANGE-LEDGER.md；
4. docs/ai/STATE.md；
5. 当前 Handoff；
6. 主计划阶段状态。

代码写入后立即登记实际文件/符号、验证、风险、未知和回滚。阶段关闭前必须运行 Tools/Validate-ChangeLedger.ps1。

## 当前状态

- 已完成文档化，未改任何运行时代码。
- 推荐下一步：MAP-M0-001 只读审计。
- 当前默认边界：矩形 Stage 先落地；polygon 先作为作者数据；是否进入 battle physics 留待 M6。
