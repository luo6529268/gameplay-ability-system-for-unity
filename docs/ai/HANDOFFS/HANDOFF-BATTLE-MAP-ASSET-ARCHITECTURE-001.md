# Handoff — BATTLE-MAP-ASSET-ARCHITECTURE-001

> 日期：2026-08-25  
> 当前状态：SUPERSEDED BEFORE CODE / NO CODE  
> 下一推荐 Work Package：MAP-M0-001  
> 主计划：Assets/NTSD/Docs/battle-map-asset-architecture-plan.md

## Superseded

本 Handoff 的 M0 至 M7 不能继续执行。用户已澄清任意 polygon 可行走区域就是现有 BoundaryWall/BoundaryWallManager 的既有 Unity 行为；当前只应将该数据按 MapId 配置化，不新增 C++ audit 或 polygon physics。

替代 Handoff：docs/ai/HANDOFFS/HANDOFF-BATTLE-MAP-BOUNDARY-ASSET-001.md。

## 本次完成

已把用户提出的双 Asset 地图模型整理为完整、分阶段、可留痕的实施方案：

1. BattleMapLogicDefinition：Map ID 与战斗逻辑数据；
2. BattleMapPresentationDefinition：Map ID 与背景/表现资源；
3. BattleMapCatalog：唯一配对与 preflight；
4. BattleMapRuntimeSnapshot：Tick 0 前冻结、tick 内只读；
5. M0 至 M7 的目标、范围、解决方案、边界、验证、停止条件和范围外事项。

本次仅改文档：

- Assets/NTSD/Docs/battle-map-asset-architecture-plan.md
- docs/ai/TASKS/BATTLE-MAP-ASSET-ARCHITECTURE-001.md
- docs/ai/STATE.md
- docs/ai/DECISIONS.md
- 本 Handoff

没有修改 Unity C#、Editor、test、Scene、ScriptableObject、资源、DAT、配置、C++ 或服务器。

## 当前事实

| 主题 | 已确认 | 不能误写为 |
|---|---|---|
| BoundaryWall | 已有 Scene polygon 作者与包含判断。 | 已是每个实体的正式 C++ 对齐 physics boundary。 |
| Stage runtime | 当前可从 GameConfig fallback 和 BoundaryWall 外接矩形得到宽度/Z。 | 已具备 Map ID、origin 和独立地图快照。 |
| Lockstep | LockstepSessionIdentity 有 StageFingerprint 字段。 | 已由正式 Map Asset 计算/注入。 |
| 背景 | Bg/Camera/Android 黑色覆盖是本地表现。 | 应进入 Stage 或 network fingerprint。 |
| Polygon | 可保存为作者资料。 | 已阻挡角色、武器、投掷物、击退。 |

## 已登记决策

D-016 已明确：

- Logic 与 Presentation Asset 分离；
- Map ID 加 Catalog 作为唯一配对入口；
- Logic RuntimeSnapshot 才是 simulation / fingerprint 真相；
- 平台视觉不改变战斗逻辑或联机身份；
- 第一阶段只接入已确认的矩形 Stage；
- polygon 只有在 M6 得到独立 C++ evidence 或用户新玩法授权后才能成为正式 simulation boundary。

## 下一包：MAP-M0-001

### Goal

只读闭合 C++ Release 和 Unity 的 stage coordinate、stage rectangle、spawn、origin、rounding 和 StageFingerprint 输入合同。

### 允许

- 读取 C++ Release live path；
- 读取 Unity Stage、BoundaryWall、NTSDRenderSpace、spawn、lockstep source；
- 新建 M0 计划/状态/Handoff 文档；
- 记录 VERIFIED、INFERRED、UNKNOWN。

### 禁止

- 不改 Unity gameplay、Scene、Asset、DAT、背景或 Camera；
- 不改 C++；
- 不启动第二个 Unity Editor；
- 不把 polygon 接入角色移动、hit 或碰撞；
- 不开始 M1/M2/M3 或更后包。

### 必答决策门

1. C++ Release 的 Stage X origin/width/Z range 如何进入实体、边界和出生？
2. Unity Editor world X/Y 与 NTSD logic X/Z 的精确整数转换是什么？
3. 首批地图是否只需要矩形逻辑范围？
4. polygon 是否只是作者资料，还是必须立即改变正式 battle physics？
5. 生产 session identity 的 StageFingerprint 创建点在哪里？

默认建议是第 3 项为是、第 4 项为仅作者资料。若首批即需要任意 polygon 阻挡，必须把它升级为独立 M6 gameplay 包。

## 进度更新协议

每个阶段开始前：更新主计划状态、建立阶段 Task、建立唯一 Change Record、同步 Ledger/State/Handoff，之后才能修改脚本。

每次脚本修改后：立即登记实际符号、验证、风险、未知和回滚；执行最窄测试；阶段关闭前运行 Tools/Validate-ChangeLedger.ps1。没有运行时/C++ 证据的事项只能是 RUNTIME_PENDING 或 BLOCKED。

## 当前未解决项

- C++ stage coordinate/origin 合同尚未重新审计；
- polygon 是否将来进入正式 simulation 尚未决定；
- 默认沙漠 Scene BoundaryWall 与未来 Map ID 的对应关系尚未确认；
- StageFingerprint 的正式生产注入点尚未确认；
- 用户 dirty Scene 不能被本计划自动保存或清理。

## 恢复检查

1. 先读本 Handoff、Task、STATE、DECISIONS 和主计划；
2. 检查 git status，保留全部用户修改；
3. 确认现有 Unity Editor 状态，不启动第二实例；
4. 用户恢复后只开始 MAP-M0-001 的只读审计；
5. 准备任何脚本修改前，先补齐 Change Record 闭环。
