# SIMULATION-WORLD-MODULE-EXTRACTION-001 — SimulationWorld 子模块化与 partial 移除

<!-- CHANGE-RECORD
id: SIMULATION-WORLD-MODULE-EXTRACTION-001
status: IN_PROGRESS
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.AiInput.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.AiSoaShadow.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.AiDecisionShadow.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.FrameInput.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.QueryAndLinks.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.StageWave.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationWorld.StageRender.partial.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationStageWaveModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiInputModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiSensingModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiDecisionModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiSensingTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationAiDecisionTypes.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRespawnModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleEarlyFrameAdvanceModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleInteractionPipeline.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleRandomWeaponDropModule.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationPassPipeline.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationWorldModuleArchitectureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/SimulationWorldExtractedPassModuleEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRuntimeSelfCheckEditor.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoAShadowEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/PendingDestroySlotAdmissionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/StableIdDeterminismEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsOutcomeHostWriterSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleResultsReserveTransactionSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRuntimeStructureGuardEditorTests.cs
authority: USER-APPROVED-SIMULATION-WORLD-MODULE-EXTRACTION-2026-09-01; Assets/NTSD/Docs/simulation-world-module-extraction-plan.md; C++ release pass-order invariants
evidence: ARCHITECTURE_BLUEPRINT_FROZEN / M0_PHYSICAL_FILE_GUARD_WRITTEN / M0_RUNTIME_EDITOR_VALIDATION_COMPILE_0 / M1_FOCUSED_15_15_PASS / M2_UNITY_FOCUSED_11_11_PASS / M3_UNITY_FOCUSED_67_67_PASS / M4_UNITY_FOCUSED_242_242_PASS / M5_UNITY_FOCUSED_24_24_PASS / M6_UNITY_FOCUSED_29_29_PASS / M7_UNITY_FOCUSED_112_112_PASS / M8_UNITY_FOCUSED_110_110_PASS / M9_212_PASS_PLUS_1_KNOWN_BASELINE / M10_RUNTIME_EDITOR_COMPILE_0 / M10_AI_REGRESSION_158_158_PASS / M10_CONTRACT_MATRIX_35_35_PASS / M10_STALE_TEST_REFERENCES_3_3_PASS / M10_FULL_EDITMODE_1763_EXECUTED_EXTERNAL_BASELINES / M10_TWO_CLEAN_PLAY_STOP_CYCLES / M10_SCENE_DIRTY_FALSE / PARTIAL_DECLARATIONS_0 / HISTORICAL_PARTIAL_FILES_0 / WORLD_LINES_6040_ALARM_EXPLAINED / FULL_SELFCHECK_P4_RENDER_FEATURE_ASSERTION_FAIL
-->

> 创建日期：2026-09-01
>
> 当前状态：`IN_PROGRESS / M1-M9_FOCUSED_PASS / M10_CODE_COMPLETE / M10_ACCEPTANCE_BLOCKED_BY_TASK_EXTERNAL_BASELINES`

## 1. 用户要求

用户明确认为 `SimulationWorld` 的 partial 共享实现方式不适合长期维护，要求先创建完整文档，再按文档将职责拆成子模块类，由 `SimulationWorld` 主模块持有子模块引用。

实施权威文档：

- `Assets/NTSD/Docs/simulation-world-module-extraction-plan.md`

## 2. 行为边界

本 Change 是纯架构所有权迁移，不修改 gameplay 规则。必须保持：

- C++ release live pass 顺序。
- 固定 30 Hz。
- input、OPoint、register/unregister、slot/generation、deferred mutation 时点。
- RNG 调用顺序。
- snapshot、checksum、worker 和 presentation 可观察结果。
- `BATTLE-RUNTIME-ORDERED-SHUTDOWN-001` 的 11 阶段顺序。

不得借本 Change 修改 Naruto DDA、角色 DAT、Scene、Prefab、Server、C++、Input Actions、URP 或 formal marker。

## 3. 已观察原状

- 相关 `SimulationWorld*` 文件合计约 `22,727` 行。
- 仍属于 `partial class SimulationWorld` 的主体约 `20,130` 行。
- AI Input/Sensing/Decision 三个 partial 合计约 `13,412` 行并共享 private 状态。
- Registry partial 约 `1,672` 行，包含 constructor、slot、registry、reset 和 shutdown。
- Passes partial 约 `3,138` 行，同时包含 pass 编排、具体规则和 probe。
- FrameInput、QueryAndLinks、StageWave、StageRender 已经出现普通 module 迁移先例；主类注释已规定“不再新增 partial”。
- 工作区包含大量用户未提交修改；迁移只能在当前内容上做最小阶段性改动，不得覆盖其他 Change。

## 4. 计划阶段

1. Phase 0：架构规则、allowlist 测试和新鲜基线。
2. Phase 1：收口已提取的 FrameInput、QueryLink、Stage 模块。
3. Phase 2：提取 Registry/RuntimeSlot/Lifecycle。
4. Phase 3：提取 AI Runtime aggregate 及 Input/Sensing/Decision 子模块。
5. Phase 4：提取 PassPipeline 和专项 pass。
6. Phase 5：移除所有 `partial class SimulationWorld` 和历史空文件，收口 diagnostics façade。

每个 Phase 必须独立编译和验证，不允许积累到最后一次性修复。

## 5. 不变量

- 每个可变字段只有一个模块 owner。
- 子模块是普通 C# 对象，不继承 MonoBehaviour，不解析创建型 singleton。
- World 保留兼容 façade，调用者迁移和 API 删除分开。
- 不使用每 tick delegate/event bus/LINQ/反射。
- 不以“文件变小”代替行为等价证据。
- 最终 `rg "partial class SimulationWorld" Assets/NTSD/Scripts` 必须为 0。

## 6. 验收

- Unity compile 0 error。
- architecture/focused/registry/runtime-slot/AI/pass/snapshot/checksum/worker/shutdown/central tests 达到基线。
- 同 seed/input/tick checksum 和 pass trace 无差异。
- 每 tick allocation 不增加。
- 最终代码两轮真实 Play/Stop 无 cleanup warning，Scene 不脏。
- 完整 SelfCheck 实际运行；当前 Naruto DDA 阻塞若仍存在，必须单独报告，不得顺手改战斗规则。
- Change Ledger validator 通过或只报告已记录的无关阻塞。

## 7. 回滚

- 逐 Phase 使用 `apply_patch` 反向迁移当前模块；不使用 `git reset/restore/clean`。
- 保留 ordered shutdown、central render/HP/editor preview、singleton teardown 和所有用户未提交内容。
- 任何阶段出现 checksum/pass-order first difference，停止后续迁移，先恢复该阶段所有权边界。

## 8. 实际实现与验证

### 8.1 已实现检查点

- Phase 0：新增 `SimulationWorldModuleArchitectureEditorTests`，冻结迁移 allowlist；新增 partial 会失败，allowlist 后续只允许缩小。
- Phase 1：精确删除已无类型实现的 `SimulationWorld.FrameInput.partial.cs`、`SimulationWorld.QueryAndLinks.partial.cs` 及 `.meta`；正式实现继续由既有 `SimulationFrameInputModule`、`SimulationQueryAndLinkModule` 提供。
- Phase 2a：新增 `SimulationRegistryModule`，统一创建并拥有 `RuntimeSlotTable`、`RuntimeRestStore`、`SimulationObjectBucketRegistry`；World 暂以只读兼容属性保持现有调用。
- runtime handle resolve、read-only slot view、stable-id search、runtime-slot order 和 runtime snapshot refresh 已迁入 Registry module，World 保留转发 façade。
- 178 行 `SimulationWorld` 组合构造函数已从 Registry partial 移回主 `SimulationWorld.cs`，模块初始化语句和顺序不变；聚合根现在直接展示 composition boundary。
- 此处是早期检查点事实：当时仍有 6 个 allowlisted
  `partial class SimulationWorld` 声明；该事实已被下方 8.5 的后续实现取代。

### 8.2 新鲜证据

- Unity script compile：0 error；移动构造函数首次遗漏的 `NTSD.Extensions` using 已补回后重新编译通过。
- architecture guard：job `b4889c662b4d42769ebb31b8f1643c7e`，`2/2 PASS`。
- runtime slot lifecycle：job `365eb153d61d4d97a95278d76906ac86`，`5/5 PASS`，包含 warmed zero-allocation。
- query/link：job `a2df30eb9d13447fabe15b33cd2e3a80`，`2/2 PASS`。
- runtime-slot snapshot：job `559203f8568b404a8912f0040199eb2a`，`3/3 PASS`，包含 warm capture zero-allocation。
- dedicated worker：job `1d9cc979dbc54d969a146309b25c7967`，`20/20 PASS`。
- ordered shutdown：job `c03b2d21f9dd4ecfbaf3d297cfe60291`，`4/4 PASS`。

### 8.3 下一检查点

- 继续把 register/unregister、deferred mutation、reset/shutdown postconditions 迁入 `SimulationRegistryModule`。
- Registry partial 清空并收缩 allowlist 后，才进入 AI Runtime aggregate；不得跨阶段大爆炸移动 AI/Pass。

### 8.4 2026-09-01 后续检查点

- Registry module 已继续接管 stable-id/profile/capacity、ticking/camera、pending-destroy cache、全部 registry reject counters 和 structural event sink/context/cursor/witness state。
- runtime-slot claim/search、release/rollback、pending-destroy scan、RegisterCore、UnregisterCore、UnregisterImmediate、pending unregister flush 和 pending entity destroy 已迁入 `SimulationRegistryModule`；World 保留 StructuralWriter/public façade。Registry partial 已从约1672行缩至约706行，剩余主要为 root reset/shutdown façade、growth coordination 和兼容转发。
- 新增 `SimulationAiRuntime` aggregate，并由 World readonly 持有；aggregate 已持有 Input/Sensing/Decision 三个普通 C# 子模块。
- `SimulationAiInputModule` 已拥有 slot snapshot、三套 spatial index、phase targets、move-mode arrays、ground/air role arrays、team HP arrays、epoch/cache flags 和 input diagnostics。
- `SimulationAiSensingModule` 已拥有 sensing mode、snapshot epoch/validity、mismatch、execution profile/owned-input mode 和 candidate/remainder flags。
- `SimulationAiDecisionModule` 已拥有 decision/unified mode、oracle interval 和 legacy fallback snapshot；fallback snapshot 创建时点保持在原 `InitializeAiSoASensingRows` 之后。
- 迁移造成两个 reflection fixture 失效：Sensing epoch corruption 已改为从 `aiRuntime.Sensing` 注入；Unified tick-boundary test 改为调用正式 deferred mutation boundary，不再反射 `_ticking`。二者回归通过，没有在 World 保留第二份影子字段。

新鲜证据：

- structural witness job `b12ae183b6fe4a85ac33fadf1f4e7397`：`4/4 PASS`。
- move-mode job `b1f370b748f44a459d956d807c0ec068`：`6/6 PASS`。
- nearest job `4e8a0ee44749423982863aaa8693fdb3`：`3/3 PASS`。
- live-slot job `0b5a0d1bbf744450bd9e1a14c9fe7892`：`37/37 PASS`，含 warmed zero-allocation。
- sensing job `d8aead5be9ff4fdd9efb56bd1970f63c`：`12/12 PASS`。
- decision job `85b893a8c4624666afe3c334f61ff78a`：68项中67通过，唯一 `DataOrientedProfile_MatchesLegacyFullDispatcherForPosition38` 失败。`docs/ai/STATE.md` 与 `R8-AIROWGEN-001` 已证明该用例自2026-08-24扩大整类和独立job均失败，且被正式记录为既有独立AI fixture；本次结果与基线一致，不修改算法。
- architecture job `f6e1356f3e1c4731a50b048978191be3`：`2/2 PASS`。
- worker job `e7e8a2f48f744af3b2c551db70d6db53`：`20/20 PASS`。
- ordered shutdown job `6c3fa2b7d6564848a93db8ce586435c2`：`4/4 PASS`。

该检查点当时仍有6个 allowlisted `partial class SimulationWorld`；该事实已被
下方 8.5 的后续实现取代。AI 算法主体和 PassPipeline 未迁移的边界仍然有效。

### 8.5 2026-09-01 partial 清零检查点

- 将 Registry、Pass、AI Input、AI Sensing、AI Decision 六个尚存 World
  partial 的实现体按原始顺序机械合并到非 partial `SimulationWorld.cs`；算法、
  循环、RNG、pass 顺序和调用点未在该机械步骤中重写。
- `SimulationWorld` 声明现为普通 `public class SimulationWorld`。
- 已删除全部 `SimulationWorld.*.partial.cs` 历史实现文件及对应 `.meta`；其中只
  保留独立顶层类型或已是普通模块的文件改名为
  `SimulationAiSensingTypes.cs`、`SimulationAiDecisionTypes.cs`、
  `SimulationStageWaveModule.cs`、`SimulationStageRenderModule.cs`，并保留原 meta GUID。
- 架构 allowlist 已缩为 0；扫描规则现在匹配任意访问级别的
  `partial class SimulationWorld`，并单独拒绝历史 partial 文件名。
- 静态扫描结果：partial 声明 0，历史 partial 文件名 0，`git diff --check`
  无 whitespace error（只有 Git 的 LF/CRLF 提示）。
- `dotnet build Assembly-CSharp.csproj /m:1 /v:minimal`：92 个既有 warning，
  0 error，证明 production assembly 在最终文件重命名后可编译。
- 复核开始时 Unity Editor 曾被任务外 `FormalContentClosureEditorTests.cs`
  缺少 `WPointExpectation` 类型阻塞；其所有者随后补齐后，fresh
  `dotnet build Assembly-CSharp-Editor.csproj /m:1 /v:minimal` 为34 warning/0 error，
  同时证明新增架构测试可编译。现有 Unity Test Runner 的桌面自动化权限未获批准，
  因此最终重命名后的3项架构测试及 focused 回归仍未实际运行；不得把重命名前的
  2/2 和行为 focused 证据冒充为最终 fresh 测试。
- 此检查点只解决“partial 仍存在”的硬问题。由于尚未迁移的算法体暂时集中在
  约 19,439 行的主类中，AI 算法主体与 PassPipeline 的真实模块抽离仍未完成，
  本 Change 保持 `IN_PROGRESS`，不得报告整个计划完成。
- 当前 Editor 实际消费 `Temp/NTSD_BattleRuntimeSelfCheck.request`；全量检查返回
  `FAIL`，first failure 为任务外
  `CheckUnityBattleCameraRemainsDisabled`：`disabled Unity battle camera check must
  inject a non-zero stale camera state`。本 Change 不修改相机规则，故该结果只记录为
 完整 SelfCheck 未通过，不能替代本 Change 的 focused 回归。

### 8.6 Phase 4a OID 51/52 模块抽离（代码已写、编译通过、focused 待 Editor）

- 目标类型与文件：`BattleOid5152RuntimeModule.cs`。该独立顶层类型已从
  `SimulationWorld.cs` 物理移出；World 文件只保留 readonly 引用、构造和同名
  public façade。
- 只迁移 `Oid5152RuntimeMaintenanceAll`、merge/split、HP gate 与 relation-team
  helper；World 保留同名 public façade。
- 模块通过明确的 World internal capability 读取 dormant/query slot、刷新 runtime
  snapshot、标记 unified row membership 失效并进入/退出 deferred mutation。
- 不更改 slot 扫描顺序、4500/900 timer、HP 合并/二分、frame gate、partner reset
  顺序或 AI unified row invalidation 时点。
- 验收：Runtime/Editor compile 0，OID 51/52 SelfCheck 或 focused fixture 达到迁移前
  基线；失败立即停在该子批次，不继续迁移其他 pass。
- 实际实现：World 新增 readonly `oid5152RuntimeModule`，公开
  `Oid5152RuntimeMaintenanceAll` 只转发；merge/split/HP gate/relation team 算法和
  4500/900 timer 维护均由独立 `BattleOid5152RuntimeModule` 类型执行。World 只提供
  dormant/query slot、runtime snapshot refresh、unified-row invalidation 和 deferred
  mutation 四类 internal capability。
- fresh production/editor build 均为0 error；离线反射调用既有7个 OID self-check
  被 Unity 原生 `ECall methods must be packaged into a system module` 拒绝，不能作为
  测试失败或通过。当前 Test Runner UI 权限不可用，因此该子批次状态是
  `CODE_WRITTEN / COMPILE_PASS / FOCUSED_TEST_PENDING`。
- 当前已打开的 Unity Editor 未自动发现新文件，生成的 `Assembly-CSharp.csproj`
  仍缺该 Compile item；没有修改生成工程。使用从生成工程复制、只追加该模块文件的
  临时 validation project fresh build 为0 error，临时 project 随后已删除。用户需在
  Unity 执行一次 Assets Refresh 后，Editor 才会导入物理模块文件并恢复自身编译。

### 8.7 Phase 4b Respawn 模块抽离（代码已写、编译通过、focused 待 Editor）

- 新增独立 `BattleRespawnModule.cs`，接管 post-frame death gate、无 stored-count
  respawn、stored-count respawn、队友平均坐标/RNG 重生点和 OID998 immediate effect。
- World 新增 readonly `respawnModule`；`PostFrameAdvanceDeathCleanupAll` 只转发。
- respawn scratch list 改由模块单独拥有；World 仅提供 active snapshot fill、active
  判定、runtime snapshot refresh、respawn diagnostic hook、factory/reference pool 能力。
- 扫描顺序、两次 `BattleRandInt` 次序、frame 212/219、HP/PP 写入、OID998 task
  字段和 immediate materialization 顺序未改变。
- 包含 OID 与 Respawn 两个物理模块文件的临时完整 validation project fresh build
  为0 error；临时 project 已删除。当前 Unity Editor 仍需手工 Assets Refresh 后才能
  导入新增文件，focused 尚未运行。

### 8.8 完整模块蓝图与 M0 守卫

- `simulation-world-module-extraction-plan.md` 已补充第11～18节：强制拆分定义、
  当前19,045行事实、最终依赖图、World保留/迁出清单、capability白名单、Registry、
  PassPipeline、OID、Respawn、Early、Late、Interaction、RandomWeapon、AI
  Runtime/Shared/Input/Sensing/Decision、Stage/Snapshot/Diagnostics逐项合同，以及
  M0～M10验证和停止条件。
- 架构测试新增物理文件合同：九个当前正式 module 必须各自存在独立 `.cs`，对应
  文件必须声明该类型，`SimulationWorld.cs` 不得声明子 module class。
- 包含 M1 两个新增 runtime 文件的临时 Runtime + Editor 完整验证工程 fresh build
  为0 error；为保持既有 internal 可见性，Editor 验证程序集名保持
  `Assembly-CSharp-Editor`。两个临时 csproj 已删除。
- 当前 Unity Editor 没有自动导入新增物理文件；按文档停止条件，必须先由用户执行
  `Assets > Refresh` 并跑 M1 architecture/OID/respawn focused，未取得 M1 绿色前
  不进入 M2 EarlyFrameAdvance。
- 新增 `SimulationWorldExtractedPassModuleEditorTests`，以 reflection 逐一调用既有
  7个 OID5152 和4个 Respawn private self-check；不复制规则，也不会触发完整
  SelfCheck 的任务外 camera 前置失败。包含该新 fixture 的临时 Runtime+Editor
  validation project fresh build 0 error，临时 csproj 已删除。
- 继续执行时重新检查当前 Unity：`Assembly-CSharp.csproj` 中 OID/Respawn 均未导入，
  `Assembly-CSharp-Editor.csproj` 中 focused fixture 未导入，三项均为 false；Console
  仍报告 `BattleOid5152RuntimeModule` 未找到。没有可用 Unity MCP resource，Windows
  Computer Use 即使在用户明确授权继续执行后仍由宿主拒绝为
  `Computer Use was not approved to use unity`。因此必须由用户手动 `Ctrl+R`；按
  M1 停止条件，在 Refresh 和两组 focused 绿色前不进入 M2。

### 8.9 M1 Unity 导入与 focused 验收完成

- 用户再次执行 Assets Refresh 后，当前 Unity Editor 已导入
  `BattleOid5152RuntimeModule.cs`、`BattleRespawnModule.cs`、两项架构守卫和
  focused fixture。
- Editor 请求入口实际运行并写出
  `Temp/NTSD_SimulationWorldM1Focused.result`；结果为 `PASS`，其中
  architecture `4/4`、OID5152 `7/7`、Respawn `4/4`，合计 `15/15`。
- 请求文件已被消费，Editor 日志同步给出
  `[SimulationWorldM1Focused] PASS: architecture=4, oid5152=7, respawn=4, total=15.`。
- 该证据满足 M1 停止条件，允许进入 M2；不代表 M2～M10 或整个模块化计划完成。

### 8.10 M2 EarlyFrameAdvance 实施边界（开始前）

- 仅新增 `BattleEarlyFrameAdvanceModule.cs`，迁移 Early teleport、state500/501
  handle snapshot/validate/resolve/special 算法与对应 scratch/diagnostics。
- `SimulationWorld` 只保留 readonly 引用、构造、capacity preparation、public
  façade 与 Registry/refresh/config 的受限 capability；不得保留算法副本。
- 必须保持 active runtime-slot 顺序、handle generation/occupancy proof、teleport
  gate、state500/501 分支、snapshot refresh、RNG 与 OPoint 可见边界不变。
- 验收复用既有 `EarlyFrameAdvanceOptimizationEditorTests`，并补充 physical-file
  ownership guard；任何 compile/focused/zero-allocation 回归立即停止在 M2。

### 8.11 M2 EarlyFrameAdvance 物理提取检查点

- 新增独立 `BattleEarlyFrameAdvanceModule.cs`；该模块单独拥有 active entity
  scratch、state500/state501 handle scratch、legacy diagnostic mode 与全部 Early
  counters。
- `SimulationWorld.EarlyFrameAdvanceSpecialsAll` 现只转发；World 原有 legacy、handle
  snapshot/validate/resolve、state500/state501 special 算法体和同义字段均已移除。
- World 仅新增 readonly module 引用、构造、capacity preparation，以及 occupancy
  epoch/logical capacity/handle resolve 三类窄 capability；existing config、active
  snapshot/check 和 single-entity refresh capability 继续复用。
- 静态扫描：World 中旧 Early private 方法名与两份 handle list 均为0；partial 声明
  仍为0；physical-file/readonly guard 已加入 architecture fixture。
- 从 Unity 生成工程复制、只追加 M2 文件并保留 Editor internal 可见性的临时
  Runtime/Editor validation project 均 fresh build 0 error（Runtime 80个既有 warning，
  Editor 34个既有 warning）；两份临时 csproj 已精确删除。
- 当前 Unity 生成的 `Assembly-CSharp.csproj` 尚未包含新模块，已放置
  `Temp/NTSD_SimulationWorldM2Focused.request`。必须再次 Assets Refresh 后由当前
  Editor 运行 architecture `4` + Early fixture `6` + flow self-check `1`；未取得
  `11/11 PASS` 前不进入 M3。

### 8.12 M2 Unity MCP 验收完成与 M3 开始

- 通过本地 stdio MCP server 直连 Unity 实例
  `gameplay-ability-system-for-unity@b1b02287`（端口6401），执行 full asset refresh
  与 compile；domain reload 断线由 MCP 正常恢复，Editor 返回 ready。
- Unity 已导入 `BattleEarlyFrameAdvanceModule.cs`，Runtime/Editor DLL 于19:46:05
  fresh 生成；M2 request 于19:46:22写出 `PASS`：architecture `4/4`、Early
  `6/6`、flow `1/1`，合计 `11/11`。
- M2 绿色门禁已满足。现只进入 M3 `BattleLateEntityLifecycleModule`；先迁移
  late state special/state9996/OPoint/death/cleanup/tail/snapshot boundary 与其
  scratch/diagnostics，未通过 late/weapon/OPoint/worker/shutdown 前不进入 M4。

### 8.13 M3 LateEntityLifecycle 提取与 Unity 验收完成

- 新增独立 `BattleLateEntityLifecycleModule.cs`，接管 `LateEntityUpdateAll`、late
  state special、state9996五子生成、recovery/frame tick/frame exit、death OPoint、
  frame OPoint、cleanup、tail、queued flush、prev-frame mirror 与 late snapshot
  boundary；模块拥有 late scratch、snapshot mode、legacy switches 和8项 counters。
- World 原 late 算法块已物理删除，只保留 `LateEntityUpdateAll`、self-check seam、
  snapshot mode 和 transition refresh 四个 façade，以及 Registry/ECS/OPoint/resource
  的窄 capability。模块不直接访问 `LF2ObjectPointFactory.Instance` 或
  `GameDataManager.Instance`。
- 首次 MCP compile 只发现模块缺少 `NTSD.Simulation.Ecs` using；仅补该 using 后
  full scripts refresh/compile 的 Unity Console error 为0。
- Unity MCP test job `ffe483395b30479bab79ff173f434b91`：`67/67 PASS`，覆盖
  architecture、late tail/common no-op parity与0B、late snapshot、late OPoint
  capacity、W05 OPoint lifecycle、dedicated worker 和 ordered shutdown。
- MCP `execute_code` 试图额外反射私有 SelfCheck 时被工具自身 Windows
  `mono.exe 文件名或扩展名太长` 阻塞；这是 in-memory compiler transport 限制，
  不是项目编译或测试失败，不取代上述67/67证据。
- M3 绿色门禁已满足，允许进入 M4；M4 只迁移 InteractionPipeline。

### 8.14 M4 InteractionPipeline 提取与 Unity 验收完成

- 新增独立 `BattleInteractionPipeline.cs`，接管 pre-interaction 三段 cpoint/
  mismatch/held sync、character/object hit consumption、empty participant/whole-pass
  proof、data-oriented participant consumption 与 collision consumption end boundary；
  模块拥有 participant scratch、6项 legacy switches 和全部 interaction counters。
- World 原 interaction 算法/helper/diagnostic owner 已删除，只保留三项 pass façade、
  EndCollision façade，以及 SceneQuery/ECS hit plan/Registry proof 的窄 capability。
- Unity MCP full refresh/compile 后 Console error0。
- MCP job `10f149d6300b4d29bad97ee8ab09cf35`：collision/hit/cpoint/
  interaction/structural/checksum/query-link 广义定向矩阵 `238/238 PASS`；job
  `7f414408c43e4840b8e4b5eff755a3dd`：architecture `4/4 PASS`。M4 合计
  `242/242 PASS`。
- M4 绿色门禁已满足，允许进入 M5；M5 只迁移 RandomWeapon 并建立
  `SimulationPassPipeline` 的组合/转发，不重排 pass。

### 8.15 M5 RandomWeapon / PassPipeline 与 Unity 验收完成

- 新增独立 `BattleRandomWeaponDropModule.cs` 与 `SimulationPassPipeline.cs`；前者
  唯一拥有 random-weapon buffer、普通掉落和 Mode2 tail/spawn，后者 readonly
  组合 OID5152、Respawn、Early、Late、Interaction、RandomWeapon 六个 pass module。
- `NTSDBattleTickSystem` 继续作为既有权威 29 phase scheduler；Pipeline 负责业务
  module 组合和 façade，不复制第二份 scheduler，也未改变 RNG 调用、drop slot、
  OPoint materialize 或 input lifetime 边界。
- M5 中两次 focused 失败均来自 `BattleRuntimeSelfCheck` 对已经迁走的 `_buckets`、
  `_cameraX`、`_cameraVel`、`_ticking` 私有字段的旧反射；已改用 Registry/World
  正式 seam，没有修改 production 算法。
- Unity full refresh/compile Console error 0；exact 回归 job
  `217b1f8b6b014072ab5b3033bf88a13b` 为 `1/1 PASS`；最终 M5 RNG/pass-order/
  architecture job `17e6bba03c5a4ae9b3c61492dc8e9ebc` 为 `24/24 PASS`。

### 8.16 M6 Registry remainder 与 Unity 验收完成

- `SimulationRegistryModule` 接管 registry-owned stable comparer/non-entity renderer
  ordered read、registered-object reset、logic-only shutdown sweep 与 shutdown hard
  postcondition；World 只保留 root reset/shutdown façade和跨 Registry/AI/writer 的
  desktop capacity growth 协调。
- 删除 World 中未使用的 stable-id/runtime-slot allocation、rollback、raw reset 等
  同义 private wrapper；新增只读 `TryGetRuntimeSlotReadOnlyViewForDiagnostics`，用于
  测试读取 generation，不暴露 mutable slot table。
- 首轮测试的 2 个失败来自 `PendingDestroySlotAdmissionEditorTests` 和
  `StableIdDeterminismEditorTests` 反射旧 `_runtimeSlots` 字段；两处只改为上述只读
  seam。Unity scripts refresh/compile Console error 0；最终 job
  `acea88d687bd43f2a6f6fd6c971b3819` 覆盖 slot/generation/structural/stable-id/
  ordered shutdown/architecture，结果 `29/29 PASS`。
- M6 绿色门禁满足；当前只进入 M7 AI Input，M8/M9/M10 仍未完成。

### 8.17 M7 AI Input 提取与 Unity 验收完成

- `SimulationAiInputModule` 已接管此前仍由 World 持有的 ground-team partition pool/
  map、team HP summary、nearest slot facts 类型与实例状态；World 对这些状态只保留
  compatibility forwarding properties。
- 已迁入模块的算法包括 snapshot capacity、nearest facts version/capture、
  `BuildSnapshotIndices`、candidate snapshot product、slot snapshot clear、ground-team
  partition prepare/get/invalidate/synchronize、air-role invalidation、move-mode first10
  reset/candidate、same-team summary read，以及四项 legacy input diagnostic counter owner。
- World 只保留与 Sensing/Decision 交叉的 snapshot 顶层编排和窄 capability；slot
  snapshot、spatial indexes、ground/air role、team summary、nearest facts、mutation
  observer 与 quadtree query 均由 `SimulationAiInputModule` 执行。
- 每个子批均经 Unity MCP scripts refresh/compile（Console error 0）后运行同一矩阵；
  最终 job `cf82cf9e643042d7aaf30fc1e76067b1` 覆盖 move-mode、nearest、air/ground
  role、team partition、live-slot、pooled allocation、quadtree allocation 与 architecture，
  结果 `112/112 PASS`。
- M7 绿色门禁满足；随后进入 M8 AI Sensing。

### 8.18 M8 AI Sensing 提取与 Unity 验收完成

- `SimulationAiSensingModule` 现拥有 sensing rows/result 类型、expected result、mode、
  epoch/validity、candidate/remainder gates、shadow mismatch state 和全部 sensing
  diagnostics；World 仅保留 compatibility forwarding properties。
- 模块实际执行 rows initialize/grow/clear/invalidate、shadow snapshot build、candidate
  fused runtime-slot scan、row capture、boundary flags、role index/team summary build、
  occupancy/generation/identity validation、character-input row refresh、nearest/special
  kernel query、candidate handle validation，以及 shadow comparison state/publish。
- `SimulationAiRuntime` 将 Registry 的 `RuntimeSlotTable` 作为窄依赖注入 Sensing；
  Sensing 不持有 `SimulationWorld`。Candidate fused build 仅以方法参数使用 World 已有
  active/read capability，且不拥有或执行 Decision mutation witness；World 在 build
  成功后保持原有 witness begin/record 时点。
- 两个既有 Editor fixture 只更新 reflection seam：从 `aiRuntime.Sensing.Rows` 和
  backing `expected` 读取已迁状态；没有改生产断言或规则。过程中旧 Unity DLL 和
  ref-return reflection 分别产生过基础设施/fixture 失败，均在 fresh DLL 后消除。
- production/editor `dotnet build` 均为 0 error（既有 warning 保留）。Unity MCP
  各切片均 fresh import/compile；最终 job
  `7b942f2152fb4cf8a656e61731111a8a` 覆盖 `AiSensingSoAShadow`、
  `AiSensingSoACandidate`、`AiSensingKernel` 与 architecture，结果 `110/110 PASS`。
- 一次重跑被 MCP bridge 自身 `NetworkStream disposed` Error 日志污染；原样重跑
  `3d0acff623bf4cadb2b34b3b9ed388ba` 为 110/110，证明该失败不是 gameplay 或
  sensing assertion difference。
- M8 绿色门禁满足；当前进入 M9 AI Decision，M10 cleanup/full matrix 尚未完成。

### 8.19 M9 AI Decision 进行中（indexed/unified-authority 子批）

- M9 开始前的完整 decision/unified/worker/checksum/architecture 基线 job
  `946c87340faa447fa8019c3f7cabaf72` 为 `213` 项完成、仅
  `AiDecisionSoAShadowEditorTests.DataOrientedProfile_MatchesLegacyFullDispatcherForPosition38`
  一项失败（预期 predicted-DUA `3`，实际 `0`）；该 exact failure 是计划预先声明的
  known position38 baseline，不作为本次模块提取产生的回归。
- `SimulationAiDecisionModule` 已实际拥有 decision mode/snapshot/shared pass、legacy
  RNG trace、mutation witness、indexed/shadow 诊断和 unified execution state；本子批又
  迁入 shared-owned snapshot capture、indexed-canonical capture/kernel/oracle/commit
  validation/commit/fallback 全闭环，并将对应 counters、first reason 和测试故障注入
  一并迁入模块。
- unified authority 的 fresh build、rolling dirty-row commit、canonical projection
  capture、pre-commit validation、candidate publication/consumer activation、legacy
  buffer restore、failure accounting，以及 decision shared-row post-input refresh 已迁入
  Decision 模块；World 仅保留跨 Input/Sensing/publisher/writer 的窄 capability 和 façade。
- 每个子批均先由 `dotnet build Assembly-CSharp.csproj --no-restore` 验证为 0 error
  （80个既有 warning），再通过 Unity MCP import/refresh/compile。最新 focused job
  `bb24c3927d8449e9a2e67bcbdbc64290` 完成 `105` 项，仍只有上述 position38 exact
  baseline failure，没有新增 compile/test difference；此前同矩阵 job
  `096324d846eb45418ee3b18f5c8d7f5e`、`9f029588e0a14a4ca979f76632369572`
  结果一致。
- M9 尚未完成：unified shadow/refresh comparison 和 legacy decision dispatcher 主体
  仍需从 World 物理迁入 Decision 模块；完成后必须再跑 213 项完整矩阵及可审计的
  position38 单项隔离。M10 尚未开始。
- 后续同批又迁入 unified duplicate-shadow 的 runtime-slot capture、boundary 双编码、
  first10 product、index build/validate、pass availability/exception owner，以及 unified
  execution consumer/probe 与 first10 mutation observer；Unity MCP job
  `be4b2954d0fb476190fad4654f44d6eb` 仍完成105项且只有同一个 position38 基线失败。
  refresh compare 与 legacy dispatcher 仍待迁移，因此 M9 状态保持 `IN_PROGRESS`。
- 该检查点 `SimulationWorld.cs` 约13.9k行、`SimulationAiDecisionModule.cs` 约3.0k行；
  这只是迁移进度，不满足 M10 的≤2,500行架构门禁，禁止据此报告模块化完成。

### 8.20 M9 AI Decision 完成、进入 M10

- 后续已把 unified shadow/refresh comparison、legacy decision dispatcher、legacy
  character-decision RNG/input commit/context builders、decision helper cluster、held-line
  cover、shared/unified rows build/refresh/validate/compare 与 published-state validation
  物理迁入 `SimulationAiDecisionModule`；World 中对应入口均缩为 capability 或 façade。
- Decision remainder 的 mutable state、row lease、bind/validate/fallback/complete，以及
  preflight/post-legacy/exception self-check injection 已从 World/Sensing 归并到 Decision；
  Decision 内部 exception injection 不再反向经过 World。
- `SimulationWorld` 的 `AiAt`、X/Y/Z、HP/PP/team/frame/state/link/target/boundary 等兼容
  访问器已改为转发 Decision 所拥有的行投影视图，不再在 World 重复实现投影算法。
- 本地 `dotnet build Assembly-CSharp.csproj --no-restore` 为 `0 error`；移除迁移产生的
  unreachable 分支后只保留既有 warning。Unity MCP 每个子批均 import/refresh/compile，
  focused jobs `2c96ab22da724259a74bab3a827ec7f2` 与
  `7745d57de4344609b5c3bc19d4e58f75` 均完成105项，仅复现同一 position38 基线。
- M9 完整组合 job `2fab77983798482dbf1985ff424d24cc` 覆盖 decision、unified、
  character-decision、worker、lockstep checksum 与 architecture，完成213项；除已登记
  position38 baseline 外其余212项通过。精确单项 job
  `0b2c3e88d5ae4da592911311353dc457` 1/1 复现“expected 3, actual 0”，完成
  baseline isolation，未发现本 Change 新增差异。
- M9 门禁满足，当前进入 M10。M10 尚未完成：`SimulationWorld.cs` 仍约11k行，需继续
  收缩 AI Input/Sensing remainder 与 verbose diagnostics façade，并完成 full matrix、真实
  Play/Stop/re-enter、SelfCheck、ordered shutdown、Scene dirty 与 Change Ledger 审计。

### 8.21 M10 World cleanup、完整矩阵与运行时验收

- `SimulationAiInputModule` 已接管 nearest-target phase1/best-first/spatial fallback/
  brute oracle/shadow mismatch、move-mode scan、row/full fallback 及对应 self-check；
  `SimulationAiSensingModule` 已接管 candidate nearest/special core、diagnostics、shadow
  comparison 初始化事务及 sensing self-check。`SimulationWorld` 中旧算法副本、52个无引用
  private method、81个无引用 private property alias 与大量纯转发冗余已删除或压缩。
- `SimulationWorld.cs` 最终为 `6040` 行。该值仍高于计划的 `2500` 行架构报警线，但没有
  通过新增 partial、继承 God Object、dynamic 或 source-generated façade 绕过：剩余内容是
  composition constructor/readonly module references、根 lifecycle/reset/shutdown 协调、
  snapshot/restore/bootstrap 服务、既有 public compatibility/diagnostic surface、跨模块窄
  capability，以及 self-check probe 类型。继续压到2500行将要求新的 public API/diagnostic
  迁移批次，超出本 Change 的行为等价清理边界，因此保留并明确登记，不伪报达到目标。
- 结构静态审计：`partial class SimulationWorld` 为0；
  `SimulationWorld.*.partial.cs` 为0；`SimulationWorld.cs` 只声明根 `SimulationWorld`、
  `PendingSoundEvent`、`ISimulationSoundPresentationSink` 与两个私有 self-check probe，未把
  behavior module 重新内嵌。架构 fixture 的4项反射/物理文件守卫均通过。
- 最终本地编译：`dotnet build Assembly-CSharp.csproj --no-restore` 与
  `dotnet build Assembly-CSharp-Editor.csproj --no-restore` 均为 exit0、0 error。
- M10 AI/Input/Sensing/architecture 回归最终 job
  `199505573bec4816a6425f147bf049e3` 为 `158/158 PASS`；关键合同 job
  `63fac12928e54df2b6bb20b4fce40149` 为 `35/35 PASS`，覆盖 checksum 7、
  ordered shutdown 4、dedicated worker 20、module architecture 4。
- 全量 EditMode job `4d26dc2aaed44165807b5da87b4714cf` 已完整执行
  `1763/1763`。它仍因任务外既有基线失败而整体为 failed：已隔离的 position38、共享包
  version 0.6.0/0.8.0、`BattleBloodPointCatalog.Empty` / `BattleCatchPointCatalog.Empty`
  static guard，以及并行 `S0-FORMAL-CONTENT-CLOSURE-001` 正在修改的 WPoint expected/actual。
  本 Change 导致的三个陈旧路径断言已只按真实 owner 更新，并由 jobs
  `63b72c78441847c0b17dc252fe7a7424`、`6b245c3561954b84b9a0b8a712e55c6d`、
  `12e2bf91a3f64beaa5c3d91fae5cd046` 各 `1/1 PASS`；它们不再出现在全量失败集合。
- 完整 `BattleRuntimeSelfCheck` 通过 Unity MCP 菜单 fresh 执行，结果文件时间
  `2026-09-02 03:44:34 +08:00`，但停在任务外 central-render P4 断言：
  `the most recently registered renderer feature must own material and draw-mode selection`。
  本 Change 未修改 RenderFeature/URP/材质所有权，按范围不顺手修复，因此不记 PASS。
- 清理一次由全量测试/SelfCheck 后待处理脚本 refresh 污染的预热轮后，完成两轮干净
  `NTSD_Battle` Play约20秒 → Stop → re-enter。两轮 Stop 后精确过滤
  `Some objects were not cleaned up`、`managed runtime state was invalidated`、
  `LF2ObjectPointFactory_AutoCreated` 均为0；最终 Scene 为 `isDirty=false`、
  `rootCount=13`。MCP domain reload 的连接断开/`disposed object` 只登记为 bridge 噪声，
  不作为项目 cleanup PASS 或 FAIL 的替代。
- 因计划 M10 明确要求 full matrix 与完整 SelfCheck，而两者仍受上述任务外基线阻塞，
  Change 保持 `IN_PROGRESS`，不报告整个模块化计划完成；运行时代码与本 Change focused
  门禁已完成，恢复条件是对应独立 Change 先修复/确认外部基线后重跑全量与 SelfCheck。
- 最终运行 `Tools/Validate-ChangeLedger.ps1`：validator 已扫描本 Change 新增的三个
  test `code-path`，但全局退出1，唯一 error 是任务外
  `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md` 缺少 `code-path` metadata；其余为
  既有 Record 声明路径当前不在 diff 的 warning。按用户范围未修改该治理记录，不能把
  全局 validator 写成 PASS。
- scoped `git diff --check` 为0；新建脚本扫描最初发现 Decision 模块4行行尾空格，
  仅删除空格后重新验证。一次并行调用 runtime/editor `dotnet build` 因两进程争用同一
  `Temp/obj/Assembly-CSharp.dll` 令 Editor build 出现2个 CS2012；改为规定的串行顺序后
  runtime与Editor均 exit0/error0。Unity MCP 再次导入 Decision 模块后，Console
  `error CS` 精确过滤为0；该传输/并发失误不作为项目编译失败，也未隐藏在记录中。
