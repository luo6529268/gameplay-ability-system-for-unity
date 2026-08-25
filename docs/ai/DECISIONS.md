# NTSD 长期项目决策记录

## D-001 — C++ release live path 是唯一行为权威

- **状态**：VERIFIED
- **日期**：2026-08-20
- **决定**：战斗规则、pass 顺序、输入时点、碰撞/命中、CPoint/held/opoint、生命周期及 render handoff 的最终裁决，均来自 `J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 live path；主入口为 `src/entity/game_tick.cpp::game_tick(...)`。
- **依据**：根 `AGENTS.md`、`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md`；本机已读取 release `Makefile`，其 target 为 `ntsd_new.exe` 且列入相关 live 模块。
- **影响**：C#、Unity self-check、性能 hash、反汇编和旧文档都不能单独签发 C++ 对齐结论。

## D-002 — C# 与旧 Unity 验证资料保留为历史回归，不删除或改写历史事实

- **状态**：VERIFIED
- **日期**：2026-08-20
- **决定**：保留 `ntsd_release_C#`、`BattleRuntimeSelfCheck`、Authority400 diagnostic、历史 alignment/handoff 和优化报告，以供命名定位、夹具复用、回归和性能诊断；所有旧“已对齐/已关闭”结论均须按 C++ release live path 重新审核后才能恢复为当前行为结论。
- **依据**：重新对齐总纲 R0 与根 `AGENTS.md` 的 C# 边界。
- **影响**：R0 只加迁移声明和证据台账，不在旧历史段落中逐行重写当时事实。

## D-003 — R0 只处理工作流、证据和文档，不改变 battle runtime

- **状态**：VERIFIED
- **日期**：2026-08-20
- **决定**：R0 的允许写入范围是长期状态文件与 authority migration 文档；禁止变更 Unity/C++ gameplay、DAT、场景、资源、trace implementation 和 R2+ 调度。
- **依据**：重新对齐总纲 R0 的目的和完成条件。
- **影响**：R1 之前不得借“修文档”实施任何 gameplay 修复。

## D-004 — R1 先建立 C++ 源码行为合同与 Unity 差异清单，再决定任何 gameplay 改动

- **状态**：VERIFIED
- **日期**：2026-08-21
- **决定**：R1 的必经主线是“C++ release live-source behavior contract → Unity source-pass crosswalk → 全量差异清单 → 子流程验收矩阵”。每个条目必须保留 authority/evidence、前置条件、字段/副作用、Unity 映射、依赖、状态和验收方式；R2 才可能修改主调度器 pass 边界。
- **依据**：用户明确的源码优先、先盘点后修复工作流；`Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R1 修订。
- **影响**：R1-WP02 的自动 full trace 是增强证据，而不是开始源码合同或差异盘点的门槛。任何未闭合的 C++ 行为必须保持 `UNKNOWN`；任何无法独立运行的 Unity 子流程必须标记“逻辑已对齐，待测试”，不得提前宣称已验证。

## D-005 — R1 使用新的 C++ 锚定三方 trace 合同；旧 Authority400 格式不升级为 authority

- **状态**：DECIDED（流程/证据合同；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-21
- **决定**：后续 R1 producer 统一采用 `ntsd-r1-cpp-unity-trace-v1`，同时输出 `cpp-release`、`unity-fallback`、`unity-optimized`。三个 producer 必须共享固定 initial state、DAT 语义清单、stage manifest、seed 和 input journal，并以 C++ `game_tick(...)` checkpoint 为比较锚点。first-difference 必须保留 tick、checkpoint/pass、slot、字段、C++ 值、两个 Unity 值和最短已知重现前缀。
- **依据**：`docs/ai/TASKS/R1-WP01-trace-contract-planning.md`；C++ release Makefile 与 `src/entity/game_tick.cpp::game_tick(...)` 的静态 live-path 核验；`Tools/NTSDParity/README.md` 已明确旧 v3/v4 trace 的 C# authority provenance。
- **影响**：历史 C# / Unity parity schema、Authority400、checksum、fast-path proof 可复用为格式、夹具、回归或诊断材料，但不能作为 C++ 行为裁决 producer。未绑定的字段、float 归一化和 presentation policy 保持 UNKNOWN / capture-only，不得被 comparator 或旧 hash 自动判为相等。R1-WP02 当前 BLOCKED 时，本决定的 producer 合同保留为未来自动比较设计，不阻断 D-004 的源码盘点主线。
- **不决定**：本决策不判定任何 Unity pass、CPoint、WeaponSync、held/link、collision、input、opoint、render 或技能已经与 C++ 相同或不同；这些必须等待实际 C++ release trace。

## D-006 — C++ Release runtime 在 R1 中不可修改；WP02 只能做外部只读采集

- **状态**：DECIDED（用户明确范围；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-21
- **决定**：R1-WP02 必须从未修改的 `J:\QQFile\NTSD2.4\ntsd_release` Release runtime 以只读方式取得 trace。不得修改源码、头文件、Makefile、构建产物、可执行文件、DLL、资源、DAT、配置或 C++ 工程内的输出文件；不得新增 C++ instrumentation、trace sink、fixture bootstrap、输入 bridge、CLI 或诊断写入。
- **采集边界**：只允许使用现有 stdout/stderr、既有日志/诊断开关、既有命令行、既有输入/自动化方式或其他证明不写入 C++ runtime 的外部观察通道。采集结果、run manifest 和比较资料必须保存在非 authority 目录。
- **依据**：用户对 R1-WP02 的当前明确要求；`docs/ai/TASKS/R1-WP01-trace-contract-planning.md` 的 R1-WP02 amendment。
- **影响**：若没有安全的只读观察方式、可重复运行环境或必要输入，R1-WP02 必须输出 blocker 并停止；不能以“先插桩再比较”绕过该 blocker。后续 Unity trace、comparator、R2 仍不在本 Work Package 范围内。

## D-007 — Unity 中央表现与扩展容量是不可回退的交付边界

- **状态**：DECIDED（用户明确交付约束；不是 C++ gameplay VERIFIED 结论）
- **日期**：2026-08-21
- **决定**：C++ release 只裁决战斗规则、逻辑 render handoff 和最终可观察行为，不裁决 Unity 的底层 renderer API、Mesh/URP/Texture2DArray 实现或生产容量策略。重新对齐必须保留：
  - `BattleCentralRenderSystem`、中央 command/descriptor、`CentralOnly`、Texture2DArray/atlas、动态 Mesh/quad 与 URP 接入；
  - Legacy `SpriteRenderer` 仅作兼容、fallback 或诊断，不重新成为生产渲染依赖；
  - `Authority400` 为固定 400 slot 的 C++ 同槽对照 profile，不是 Unity 生产全局上限；
  - `MobileExtended` 为 1,050 initial slot、1,000 active runtime entity；
  - `DesktopExtended` 为 page-normalized 初始容量（默认 512）、dynamic growth 和无生产 active 硬上限；
  - `SimulationTickDriver -> NTSDBattleTickSystem -> SimulationWorld`、30 Hz、`FrameInputSet`、slot/generation、SoA/ECS store、对象池、worker 与战斗期间零 GC 目标。
- **依据**：用户当前明确要求；`Assets/NTSD/Scripts/Simulation/BattleRuntimeProfile.cs`、`SimulationWorld.Registry.partial.cs`、`SimulationWorld.StageRender.partial.cs` 与中央渲染计划的静态实现核验。
- **影响**：任何 C++ 行为差异只能通过最小 Unity adapter 修复。若适配可能触及本决策的边界，必须先在差异条目写出 C++ 合同、非回退证明和验收条件；不得通过恢复逐实体生产 `SpriteRenderer`、降低移动端容量、固定桌面上限、取消动态增长或把渲染状态反写回模拟来“对齐”。

## D-008 — 自编写脚本改动必须具有仓库内可恢复审计记录

- **状态**：DECIDED（用户明确流程约束；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-21
- **决定**：每个闭合的自编写脚本行为改动使用唯一 Change ID，并在 `docs/ai/CHANGE-LEDGER.md`、`docs/ai/CHANGE-RECORDS/<ID>.md`、`STATE.md` 与 handoff 中留下可恢复记录。记录必须覆盖 authority/用户需求、Unity 原状、实际代码路径与符号、改前/改后职责、不可回退边界、验证证据、未关闭风险、回滚和 Git 关联。
- **执行规则**：必须先创建 `PLANNED`/`IN_PROGRESS` Record，再修改脚本；每次带脚本 diff 的交付、提交前检查和 handoff 前必须运行 `Tools/Validate-ChangeLedger.ps1`。不能独立验证的项保持“待测试”/`RUNTIME_PENDING`；不得以代码存在、编译通过、聊天内容或 commit message 代替证据。
- **代码注释边界**：不要求把 Change ID 写进每一行源代码。只有不直观的 C++/Unity 时序合同、字段契约或跨 pass 适配点才保留简短 `Alignment contract: <ID>` 注释；详细历史始终留在 Change Record。
- **Git 边界**：validator 是只读仓库工具；未经用户单独批准不得安装 Git hook、修改 `.git/config`、修改 `.git/hooks` 或改变 GitHub Desktop 提交行为。
- **依据**：用户要求每次脚本代码修改必须在上下文压缩和长会话后仍可稳定追溯。
- **影响**：后续 R1/R2 及所有 Unity 工具/脚本变更都必须先登记。未被 Change Record 覆盖的脚本 diff 不得被报告为可交付。

## D-009 — 已批准总计划内的 Work Package 连续推进；只在真实范围门槛停止

- **状态**：DECIDED（用户明确工作方式；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-21
- **决定**：用户已授权按 `cpp-release-vs-unity-battle-realignment-plan.md` 的既定 R1～R8 依赖顺序持续推进。属于该计划、已具备 Task Contract 的常规子 Work Package 不需要在每个 R2/R3/R4 子包开始前重复请求确认；应连续执行“合同 → Change Record → 最小改动 → 分层验证 → 留痕”的闭环。
- **仍须停止并请求方向的情形**：需要扩大到计划外模块或长期架构；需要改变 CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× scale、fixed-world camera、容量、30Hz/FrameInputSet/SoA/ECS/pool/worker/0-GC 等保护边界；需要运行、修改、构建或向 C++ authority 目录写入；C++ source contract 无法闭合；需要改 DAT/scene/resource 以掩盖逻辑差异；或用户明确要求暂停/更改范围。
- **不被取消的约束**：每一项脚本改动仍必须先有独立 Change Record；R1-WP02 full trace 仍保持 BLOCKED；T8 default `stage.dat` 仍暂缓；每个结论仍按 source / compile / focused fixture / joint fixture / Play Mode / trace 分层报告。
- **影响**：当前 R2-VERIFY-01 与 R3-INP-01 可以按计划连续进入其 Change Record 和实施阶段。此前文档中“等待用户确认”的表述只保留为当时历史状态，不再阻断当前已批准总计划内的执行。

## D-010 — R3 输入差异按独立 producer / consumer 边界拆分，不合并为大改动

- **状态**：DECIDED（已批准计划内的最小实施拆分；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-22
- **决定**：将历史上过宽的 `R3-INP-02` 细分为四个独立 Work Package：
  - `R3-INP-02` 只覆盖 `D-SCHED-010` 的默认 F1/F2 step gate 与 Unity battle-entry clear 分离；
  - `R3-HOLD-INP-01` 单独覆盖 `D-INP-001` negative-link / held-caught input；
  - `R3-AI-LIFE-01` 单独覆盖 `D-INP-002` 的 HP=0 / respawn AI caller；
  - `R3-INP-03` 保留 frame packet、P1/P2 extension、AI target equivalence 与 physical binding。
- **依据**：C++ `game_tick.cpp` 的 step gate 只影响 scheduler callback / render-after-return，而
  `input_handler.cpp::apply_input` 的 negative-link 行为和 AI HP prefilter 分别依赖 held/relation 与
  death/respawn producer。把三类问题混入一笔脚本改动会破坏 D-008 的可审计最小范围。
- **影响**：R3-INP-02 可以复用现有 `BattleStepMode` / `BattleStepGate`；不得顺手删除
  `Runtime.LinkState < 0` 或 `HP <= 0` return。每个后续包仍必须分别建立 Task Contract、Change Record
  和分层验收。物理 F1/F2/W/S/A/D/J/K/L binding 和 C++ debug-unlock 不因本决定自动获得实现授权。

## D-011 — 将 R3-INP-03 拆为 packet、容量、AI target 与物理绑定四个可验收包

- **状态**：DECIDED（D-010 的执行级细化；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-22
- **决定**：原 `R3-INP-03` 覆盖的四类证据/风险不共享同一最小改动边界，改按以下连续顺序处理：
  - `R3-INP-03A`：只关闭 `D-INP-003` 的 canonical full-held `FrameInputSet` journal contract，验证
    press / hold / release / same-tick multi-key 对 `key/prev/cd/history` 的 C++ poll 等价；
  - `R3-INP-04`：只验证 `D-INP-004` 的固定 P1/P2 authority fixture 与 Unity 3+ roster extension 边界；
  - `R3-AI-TGT-01`：只验证 `D-INP-005` 的 fallback / indexed AI target equal-distance、cached-target
    与 team/input-phase behavior；
  - `R3-PHY-01`：只处理 `D-INP-006` 的实际 InputAction / Inspector / W-S-A-D-J-K-L Play Mode binding。
- **依据**：C++ `InputHandler::poll`（`src/input/input_handler.cpp:1555-1613`）只读取当前 held state 并在
  固定顺序生成 prev/cooldown/history；P1/P2 caller 在 `src/core/main.cpp:4607-4608`。Unity 的
  `FrameInputSet`/`SimulationFrameInputModule`/`NTSDInputStateModule`、roster extension、AI SoA 以及
  InputAction asset 分别属于不同 consumer 和验收层。
- **影响**：`R3-INP-03A` 可先做 test-only contract，不得顺手改 physical asset、roster capacity、AI
  candidate 或 lockstep protocol。`R3-PHY-01` 保持用户 Play Mode / asset 确认前的 `UNKNOWN`；本次不
  修改 physical binding。

## D-012 — 将 R3-FRAME-01 按 current-key、landing raw write 与 respawn integer sync 分离

- **状态**：DECIDED（D-009 内的执行级最小拆分；不是 gameplay VERIFIED 结论）
- **日期**：2026-08-22
- **决定**：原 `R3-FRAME-01` 覆盖的 `D-MOV-001～003` 不共享同一个安全改动边界，按以下顺序连续处理：
  - `R3-FRAME-01A`：只关闭 `D-MOV-001`，即 current key 从 human/AI producer 保留至 C++ F03/F09
    consumer；不得移动或重写 human/AI producer；
  - `R3-LAND-01`：只处理 `D-MOV-002` 的 landing raw-frame writer subset，并先闭合 frame/Prev/
    Attacking/Transistor 以及 R4/R5 consumer；
  - `R3-SYNC-RESP-01`：只处理 `D-MOV-003` 的 successful-physics integer sync 与 respawn scan时点，
    并先闭合 link/cpoint/structural lifecycle consumer。
- **依据**：C++ `InputHandler::poll` / `prepare_ai_input` 与 `frame_advance.cpp:80-83,941-951,977-980`
  已将 D-MOV-001 的生产者、消费者和最小断言闭合；但 C++ landing和respawn还分别依赖 raw frame-history
  writer以及R5 structural/held/CPoint producer。把三项合入一次改动会违反 D-008 的可恢复审计和最小回滚原则。
- **影响**：`R3-FRAME-001A` 已建立为 `PLANNED` Record；实际脚本改动先只允许其三条路径。D-MOV-002/003
  不因 D-MOV-001 的测试通过而自动获得实现授权，R3-FRAME-02（D-MOV-004/005）仍保持后续独立包。

## D-013 — R3-FRAME-02 按 executable guard 与当前 DAT reachability 再次收缩

- **状态**：DECIDED（scope / evidence decision；不是 gameplay VERIFIED 结论）。
- **决定**：`D-MOV-004` 单独由 `R3-FRAME-02A` 处理 Unity-only `ThrowFrameGuard` readers；`D-MOV-005`
  因当前 authored DAT inventory无法到达 exact-character ECS path而不改代码，作为未来 asset/eligibility watch保留。
- **依据**：C++ release source complete field inventory确认 `throw_frame_guard` 没有 conditional reader、没有
  nonnegative writer；Unity有三处 F03/F07 reader。C++ state2000 facing存在且Unity fallback已有对应；当前 literal
  state2000 DAT全为 type2/type4，而 exact ECS只处理 type0。
- **影响**：不得把“当前不可达”写成删除 state2000 rule的授权，也不得将 D-MOV-005 的静态结论挤入
  R3-FRAME-02A。未来DAT或type eligibility变化必须重开 source / reachability审计。

## D-014 — combo progress以C++ by-reference即时写入为权威，废止Unity local transaction oracle

- **状态**：DECIDED（source authority correction；尚不是代码或runtime VERIFIED）。
- **日期**：2026-08-23
- **决定**：C++ `input_handler.cpp` 的八方向combo和DJA直接以entity字段引用执行；每个wrapper步骤、
  interrupt和early branch的字段修改即时生效。Unity不得继续把九字段复制为local transaction并在大多数
  return路径丢弃，也不得用旧self-check的“staggered L/S/K must not complete”定义权威行为。
- **依据**：`include/input_handler.h:9-16`确认J/K/L到internal storage的交叉映射；
  `input_handler.cpp:2758-2859`确认`run_combo/advance_combo`按引用写入；`Makefile:35`确认release参与性；
  Unity `BattleCharacterInputActionResolver.ApplyComboFrameInput`与现有self-check形成相反合同。
- **影响**：登记`D-INP-010`，由独立`R3-COMBO-01 / R3-COMBO-001`处理resolver和陈旧测试。physical
  binding/FrameInputSet/worker edge仍属D-INP-006，不得与本修复合并；真实Naruto opoint表现继续由R8-WP01C验收。

## D-015 — F1/F2 战斗调试步进不进入正常战斗对齐范围

- **状态**：DECIDED（用户范围决定）。
- **日期**：2026-08-23
- **决定**：Unity 不移植 C++ Release 的 F1/F2 battle debug step、A→B→C debug unlock及其仅在
  debug tail-skip条件下出现的candidate carrier保留行为。`D-STEP-001`与`D-SCHED-008`作为用户批准省略的
  debug-only行为保留证据，不再计为正常战斗主线的未修复 gameplay差异。
- **依据**：用户明确确认“F1/F2 战斗调试步进逻辑不用”；`R8-WP01G-R01/R01B`已证明
  `D-SCHED-008`的差异只在该debug early-return路径出现，normal completed tick无candidate reader差异。
- **影响**：`R2-CANDIDATE-TAIL-01`与`R3-STEP-01`不执行；不得以后因上下文压缩重新把它们作为normal
  gameplay blocker。当前工作转入`R8-WP01G-R02`，只闭合`D-MOV-005`、`D-COL-005B`、`D-HIT-005`、
  `D-LIFE-001`。

## D-016 — 地图逻辑与地图表现资产分离，并以 Map ID 加 Catalog 配对

- **状态**：DECIDED（用户明确的 Unity-native 架构方向；尚无 gameplay 代码或运行时验证）。
- **日期**：2026-08-25
- **决定**：未来多地图系统使用两个独立 ScriptableObject。BattleMapLogicDefinition 保存稳定 Map ID 与会影响战斗模拟的地图数据；BattleMapPresentationDefinition 使用同一 Map ID 保存背景和本地表现资源。BattleMapCatalog 是 Map ID 的唯一配对、校验和选择入口。战斗开始前将 LogicDefinition 冻结为 world-owned BattleMapRuntimeSnapshot；战斗 Tick 内不得扫描 Scene、读取 ScriptableObject 或由 Camera/Bg 推导地图逻辑。
- **逻辑边界**：逻辑资产和 MapFingerprint 可以包含经 C++ Release/Unity 合同确认的 Stage rectangle、出生点、随机区域及 map-specific simulation 字段；不得包含 Asset GUID、路径、Sprite、Texture、Transform、Camera、分辨率、Windows/Android、黑色覆盖或本地表现。MapFingerprint 未来进入现有 LockstepSessionIdentity.StageFingerprint 的正式创建链；mismatch 必须在 Tick 0 前 fail closed。
- **表现边界**：背景、装饰、平台取景、Android 底部黑色覆盖和 Editor preview 只读 PresentationDefinition；它们不改变 SimulationWorld、输入、随机数、checksum、Stage、实体位置或联机身份。不同平台可以不同显示而共享同一逻辑地图。
- **实施边界**：首批只迁入 M0 重新审计确认的矩形 Stage 语义。BoundaryWall polygon 可作为作者/预览数据保存，但在独立 M6 获得 C++ evidence 或用户明确新玩法授权前，必须标为 AUTHORING_ONLY，不能自动成为角色、武器、击退、投掷物、opoint 或 AI 的正式阻挡规则。
- **工程边界**：新地图类型使用独立完整类，不新增 partial。Inspector List 仅用于编辑/加载，tick 使用冻结数组或扁平数据。GameConfig 和 Scene BoundaryWallManager 在过渡期只能是明确 legacy fallback，不能覆盖已选中地图快照。
- **依据**：用户要求“一个 Asset 保存 Map ID 和地图可行走区域，另一个 Asset 保存 Map ID 和地图资源”；现有 BoundaryWallManager 只导出场景联合外接矩形，SimulationWorld 当前从 GameConfig/Scene 取得 Stage，LockstepSessionIdentity 已有 StageFingerprint 但尚未证明有地图资产注入链。
- **影响**：建立 BATTLE-MAP-ASSET-ARCHITECTURE-001 总计划与 M0 至 M7 Work Package。下一步只能先进行 MAP-M0-001 的只读坐标/范围/fingerprint 合同审计；在此之前不得创建 runtime map selection、改 Stage writer、接入 polygon gameplay 或改变背景表现。

## D-017 — 复用现有 BoundaryWall 多边形语义进行 Map ID 配置化

- **状态**：DECIDED（用户澄清后的范围修正；尚无代码实施）。
- **日期**：2026-08-25
- **纠正**：D-016 中“先审计 C++、先使用矩形、polygon 在未来才参与 simulation、先接入 StageFingerprint”的内容不适用于当前任务，现已在写代码前 supersede。用户明确指出可行走区域就是现有 BoundaryWall 与 BoundaryWallManager 正在处理的任意 polygon；本任务不新增或重定义 battle physics。
- **决定**：新增一份按 MapId 保存的 Boundary Asset，数据形状和坐标单位对齐现有 BoundaryExportData、BoundaryData、PolygonData 与 world X/Y vertices；新增另一份按同 MapId 保存背景/表现资源的 Presentation Asset。加载选中 MapId 后，继续使用现有 BoundaryWall 和 BoundaryWallManager 的 polygon union、point contains、rect fully inside、edge epsilon、random walkable point 与 polygon outer-bounds 行为。
- **实施边界**：本计划只改变 boundary 数据来源和 Editor/Bootstrap 配置流程。不得把 polygon 转成矩形，不得新写点包含或碰撞算法，不得改变移动、hit、opoint、AI、tick、Camera、背景表现、lockstep、fingerprint、服务器或 C++ 工程。Scene 顶点编辑和 Asset 保存必须显式 Load/Apply，不能自动互相覆盖。
- **影响**：BATTLE-MAP-ASSET-ARCHITECTURE-001 和其 M0 至 M7 Task/Handoff 全部标为 SUPERSEDED BEFORE CODE。当前唯一执行计划是 BATTLE-MAP-BOUNDARY-ASSET-001，按 MAPCFG-001 至 MAPCFG-004 连续推进。
