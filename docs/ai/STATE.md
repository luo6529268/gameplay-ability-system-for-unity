# NTSD 长期项目状态

> 最后更新：2026-08-26
> 状态口径：只记录已检查的事实、明确推断和未知项；不以聊天历史作为唯一项目记忆。


## 当前阶段

- **2026-08-29 WORKER-UNITY-BOUNDARY-001 已关闭当前卡死**：`VERIFIED / SAFE-SYNC-FALLBACK`。Renderer-bound world 现在以原因码 `unity-presentation-bindings-are-still-attached` 禁止启动 dedicated worker并继续同步主线程tick；pure-logic worker合同保留。Unity worker整类job `881e133b32ae4d3f82043dc29ecec66d` 20/20 PASS；真实Play至tick2860为unpaused/failure=null，结构统计已发生66次Free，`Dedicated simulation worker failed`与`EnsureRunningOnMainThread`均0条。该结论不代表Unity-bound worker或其性能已实现；未来重启worker必须独立设计主线程presentation detach/release。
- **2026-08-26 MAPCFG-005 已写入代码与资产**：`CODE_WRITTEN / COMPILE_PENDING`。用户确认删除 `Desert01_Presentation`，将背景图并入 `Desert01_Boundary`，并从地图资产数据中删除 `boundaryName` / 多边形 `name`。当前收敛为单一 Boundary Asset：`mapId/displayName/revision/backgroundSprite/boundaries(polygons/verticesWorld)`；加载到既有 `BoundaryWall` 时仅在内存生成序号名以保留共享运行时兼容接口。已修改 BoundaryDefinition/Catalog/Bootstrap/BoundaryWallManager、四个 MAPCFG focused Editor tests、Desert01/Catalog 资产，并删除独立 Presentation 脚本/资源。静态契约检查、`git diff --check` 和 MAPCFG-005 Ledger 覆盖校验已通过；Unity 正式程序集和 focused test 仍待。
- **2026-08-26 MAPCFG-005 验证边界**：Unity Editor 修复前实际编译日志发现 `BattleMapBoundaryDefinition.cs` 4 个 `CS0122`，现已通过构造函数 deep-copy 修复；随后复用 Unity 生成的 Roslyn 参数完成交叉编译，退出码 `0`，输出仅有工程既有 warnings。当前 Unity Editor PID `37088` 的 `Assembly-CSharp*.dll` 仍停留在 2026-08-25 17:24:38/17:24:39，修复后的正式程序集尚未生成；22:52:28 的 `BattleRuntimeSelfCheck=PASS` 仍是旧程序集结果，不能计入本包。临时 dotnet wrapper 因生成工程所需 `Temp\\bin\\Debug` firstpass/package DLL 缺失而 CS0006，未进入项目源代码编译；该结果不作 compile 结论。不得把 MAPCFG-005 标为 `COMPILE_PASS`、`FOCUSED_TEST_PASS` 或 `VERIFIED`。
- **2026-08-26 MAPCFG-005 范围边界**：不删除共享 `BoundaryData` / `PolygonData` 的历史兼容字段，不改 BoundaryWall 几何、tick、输入、RNG、checksum、Camera、服务器或 C++；删除仅限 Presentation 类型/资源及其 Catalog/runtime 引用，`Desert01_Boundary.asset` 不再保存名称字段。真实 Battle Scene/Play 验收仍需在代码级验证后单独判断。
- **2026-08-25 MAPCFG-004 代码级验证完成**：`FOCUSED_TEST_PASS / RUNTIME_PENDING / DEPLOYMENT INPUT PENDING`。Unity import/compile 完成；P4 focused job `51942ac652474e6c9ba42427a93ba44a` 为4/4 PASS，P1–P4 cross-phase job `50c3e1586f5145e18b6d990662b920b0` 为14/14 PASS，既有 BattleRuntimeSelfCheck result 于17:33:25写入PASS。空配置继续零mutation且不触发 P4 Stage refresh；实际Map prepare才在角色创建前刷新Stage。没有创建真实Map Asset/MapId/Scene/Bg，也未跑Play/Player；当前只缺用户配置资产与引用后的真实Scene验收及本轮final governance。
- **2026-08-25 MAPCFG-004 治理验证**：`RUNTIME_PENDING / DEPLOYMENT INPUT PENDING`。`Tools/Validate-ChangeLedger.ps1` 已通过（105 条 Record、141 个 governed code diff covered），P4 scoped diff也已通过（只有既有 LF→CRLF 提示、无 whitespace error）；当前仅等待用户配置真实 Map Asset/MapId/Inspector 引用后才可进行的 Scene/Play 验收。不得将当前证据写成真实地图已部署。
- **2026-08-25 MAPCFG-004 代码已写**：`CODE_WRITTEN / COMPILE PENDING / DEPLOYMENT INPUT PENDING`。已新增 optional `BattleBootstrap` Catalog+MapId+Boundary manager+同一world Bg renderer 配置、prepare/clear、App/BattleTest 的 fail-close startup gate和四项内存 focused test。空配置仍为零 mutation 的 legacy fallback，且不触发 P4 新增 Stage refresh；只有实际 map prepare 成功时才会在角色创建前刷新 Stage snapshot。未写真实 Asset/MapId/Scene/Bg、Camera/Transform/PPU、DAT、C++、服务器或战斗规则；compile/test/self-check/治理验证均待。正式部署仍必须等待用户配置资产和引用。
- **2026-08-25 MAPCFG-004 预实施**：`IN_PROGRESS / PRE-CODE / DEPLOYMENT INPUT PENDING`。只读发现当前没有生产 `BattleMapBoundaryDefinition`、`BattleMapPresentationDefinition` 或 `BattleMapCatalog` Asset，因此不会猜测正式 MapId、生成默认 Asset 或覆盖当前 Bg/Scene。P4 仅先实现 optional startup config：Catalog+MapId+Bg Renderer 全部配置时，在角色创建/解除暂停前加载；两项均空则保留 legacy fallback；任何半配置或无效依赖 fail-close。Task、Change Record、Ledger、Handoff 和计划已在代码前建立；实际 Map deployment/Play验收等待用户配置。
- **2026-08-25 MAPCFG-003 预实施**：`IN_PROGRESS / PRE-CODE`。P3 只补 Asset ↔ Scene 的 explicit authoring：world X/Y deep copy、MapId 可见、Load/Apply 按钮、Undo/dirty、名称/数量 mismatch fail-close；不会自动保存/覆盖、不会创建/删除用户 walls、不会在 runtime source active 时写 authoring Scene。只读确认现有 JSON export/BoundaryWallEditor/Manager inspector 足够复用。Task、Change Record、Ledger、Handoff 与父计划已在代码前建立；尚未写 MAPCFG-003 C#、Scene、Asset 实例、Bootstrap、C++ 或 battle logic。
- **2026-08-25 MAPCFG-003 代码已写**：`CODE_WRITTEN / VERIFICATION PENDING`。Asset deep-copy replace、wall world X/Y capture、Manager explicit Load/Apply、runtime-source guard、Undo/dirty 和 Inspector MapId/confirmation UI 已写，另有三项 focused Editor tests；没有保存或修改实际 Scene/Asset、没有改几何、Bootstrap、Camera、Bg、C++ 或 battle logic。下一步只做 static/Unity focused 验证与治理校验；P4 integration 仍排除。
- **2026-08-25 MAPCFG-003 focused 结果**：`FOCUSED_TEST_PASS / CROSS-PHASE REGRESSION PENDING`。Unity重编译后 P3 focused job `5e4b965f9e7b4452a5c6e236117b673a` 为3/3 PASS，验证 explicit round trip/deep copy、name mismatch fail-close、runtime carrier active guard；static audit 未找到 SaveAssets/SaveScene，scoped diff通过。P1/P2 shared bridge回归与final governance仍待；P4 integration不在本包。
- **2026-08-25 MAPCFG-003 结果**：`FOCUSED_TEST_PASS / P4 READY / MANUAL-INSPECTOR PENDING`。cross-phase job `63182377db004cb084fc830402bbb878` 为10/10 PASS，覆盖P1/P2/P3所有当前 focused contracts；P3写入后 existing self-check result 于16:26:35为PASS。静态审计无SaveAssets/SaveScene，且没有写Scene/Asset实例；final ledger validator（104 Record / 139 governed diff covered）与scoped diff通过。P3仅完成可测试的Editor authoring bridge；真实用户Inspector点击和MapId/Catalog/Bootstrap/Player集成仍归P4。
- **2026-08-25 MAPCFG-001 结果**：FOCUSED_TEST_PASS / RUNTIME_NOT_CONNECTED。Boundary Asset、Presentation Asset、Catalog 与内存 focused Editor test 已写；真实 Unity import/compile 后，job a0b70302cb314b0cbb0a6b6d3fee0457 为 4/4 PASS，Console 无 MAPCFG C# error；`Tools/Validate-ChangeLedger.ps1` 实际通过（102 条 Record、135 个当前 governed code diff 均有记录覆盖），scoped diff check 也通过。BoundaryWall 几何、Scene、runtime、Camera、Bg、GameConfig、C++、网络和 lockstep 均未改。P1 尚未接入 runtime，因此未运行 BattleRuntimeSelfCheck/Play Mode；下一包为 MAPCFG-002，仅让现有 BoundaryWallManager 加载选中 Boundary Asset 并保持所有现有 API 语义。
- **2026-08-25 MAPCFG-002 结果**：`RUNTIME_PENDING / P3 READY / P4 INTEGRATION PENDING`。首次 Unity compile 的 test-only NUnit collection overload CS1503 已留档并最小替换为 local loop；后续 Unity assembly 已重编译，focused job `850175a9e86141f680f03e2bcb26f7b5` 为 3/3 PASS，覆盖 query parity、union、deterministic random、Stage bounds、failed-load retention 和 explicit clear fallback。现有 `BattleRuntimeSelfCheck` 的项目结果文件于 15:53:47 写入 `PASS`；最终 ledger validator（103 Record / 138 governed diff covered）与scoped diff也通过。没有改现有几何函数、Scene、Asset 实例、Bootstrap、Camera、Bg、C++ 或 battle logic。未运行 Play Mode，真实 MapId/Catalog/Bootstrap/Scene/Player 接线仍只属于 P4。
- **2026-08-25 Map ID + BoundaryWall 配置化计划**：BATTLE-MAP-BOUNDARY-ASSET-001 / IN_PROGRESS / P1 FOCUSED_TEST_PASS / P2 PRE-CODE。用户已澄清可行走区域就是现有 BoundaryWall 与 BoundaryWallManager 已实现的任意多边形，不是矩形，也不是待新增的 C++ battle physics。本任务只把当前多边形 world X/Y 数据按 MapId 保存为 BattleMapBoundaryDefinition，并用同 MapId 的 BattleMapPresentationDefinition 保存背景/表现资源；启动时按 MapId 让现有 BoundaryWallManager 加载边界并保持 IsPointWalkable、IsRectWalkable、随机采样和 legacy Stage bounds 的现有语义。此前 BATTLE-MAP-ASSET-ARCHITECTURE-001 的 C++ audit、矩形首发、StageFingerprint 和 M0 至 M7 范围在未写代码前已 SUPERSEDED。当前规范为 Assets/NTSD/Docs/battle-map-boundary-asset-configuration-plan.md；MAPCFG-002 的独立 Task / Change Record / Ledger / State / Handoff 已完成，下一动作可在其范围内修改 C#。P1 没有改 Scene、Asset 实例、DAT、C++ 或服务器。
- **2026-08-24 外部 Unity compile recovery**：`BUILD-LOCKSTEP-TEST-COMPILE-001 / CODE_WRITTEN`。背景 aspect 修正验证触发全项目编译后，先发现 `InProcessLockstepChecksumWitness.cs` 未被 AssetDatabase 纳入；已通过 Unity 自身 asset refresh 自动生成 `.meta` 并解除该 runtime CS0246。当前仅剩未跟踪的 `InProcessLockstepAuthoritySessionEditorTests.cs` 缺少其 test-local `EmptyController` helper；已补私有无输入 `ILF2Controller` fixture，绝不改 production lockstep、battle runtime或scene。compile / discovery通过后恢复 `CAMERA-PLATFORM-BACKGROUND-001` 的视觉验证。
- **2026-08-25 当前用户授权的 Unity 表现包**：`CAMERA-PLATFORM-BACKGROUND-001 / FOCUSED_TEST_PASS / EDITOR-RUNTIME-WITNESS / RUNTIME_PENDING`。同一 world `Bg (2)` map、固定视觉Camera frame与Android最终黑色 overlay 保持；Edit Mode 可开关实时取景已实际用临时 Sprite A/B replacement→private Update→Camera frame→baseline restore 通过（job `78c18d4f2b3246f99ab4b024dfc1e3f6` 21/21）。`XueYuan` duplicate 仍留在用户 Scene 但 source-owner guard 会 fail closed，避免竞争写相机；当前 hierarchy 的 ScenesCamera/Bg 中心一致。不得删/保存 Scene、改 PPU、背景 Transform、Camera.rect/aspect/follow、安全区、battle/lockstep/input/checksum/Stage writer。用户真实 Bg Inspector 换图、Scene/Game有角色、Desktop/Mobile Player与Android真机仍待。
- **2026-08-24 Server bootstrap 实际完成（覆盖以下旧环境阻塞事实）**：`I:\GitHub\Unity_GAS\NTSD_Server` 已是独立 Git/.NET 10 workspace；`S0-SERVER-BOOTSTRAP-001` 在 Server 自己的 Record/Ledger/State/Handoff 下完成两次 bootstrap、Debug/Release `0 warning/0 error` build、四项 self-hosted tests、架构边界检查、Ledger validator 与 no-network local health run。状态是 `FOCUSED_TEST_PASS / SERVER_CODE_READY / CLIENT_INTEGRATION_PENDING`，不是 S0 `VERIFIED`。Unity Client 未修改、编译、测试或验证，`S0-INPROC-AUTHORITY-001` 继续 `CODE_WRITTEN / USER-DIRECTED HOLD`。bootstrap 的 .NET 10/目录/sandbox blocker 已解除；后续 Server 范围扩大必须新建 Change Record，若需 Client 接入先写 `CLIENT_INTEGRATION_REQUIRED` 并等待用户批准。
- **2026-08-24 Server-only authority-session 实际完成**：`S0-SERVER-INMEMORY-AUTHORITY-001 = FOCUSED_TEST_PASS / SERVER_TESTKERNEL_READY / CLIENT_INTEGRATION_REQUIRED`。generic immutable frame、StartBarrier、authority-first fixed replica sequence、checksum first-difference/fail-closed 与 tests 内 TestKernel 已写；Debug/Release build 0 warning/0 error、四项 self-hosted tests、no-network local run、Ledger 与 static audit 均通过。它不是正式 NTSD BattleKernel、没有 Unity/cross-runtime checksum，也没有启动 S1。
- **2026-08-24 S0 Client validation-only 当前证据**：用户明确允许既有 Unity S0 的读取、编译、focused test 和 `BattleRuntimeSelfCheck`，但禁止 Client 代码/场景/资源/配置修改。现有 Editor 刷新后的 `Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` 晚于 S0 source；Editor.log 未匹配 C# compile-error 模式，`BattleRuntimeSelfCheck` request 于17:07:33写入 `PASS`。用户随后在 EditMode Test Runner 实际运行 S0 Fixture，截图为 5/5 pass、0 fail，并运行 `BattleLockstepSessionEditorTests`，可见九项均通过。状态是 `FOCUSED_TEST_PASS / SELFCHECK_PASS / EXISTING_LOCKSTEP_PASS / WITNESS_IMPLEMENTATION_REQUIRED / RUNTIME_PENDING`，不是 formal S0 或 `VERIFIED`；跨 runtime 是 S5，不是 S0 gate。
- **2026-08-24 S0-WITNESS-001 / CODE_WRITTEN**：用户授权的最小 Client runtime/test 范围内已完成 mismatch-only witness 接线：两个 InProcess runtime 文件、新 witness 文件和既有 S0 Editor fixture。normal tick 继续只走 aggregate checksum；首次 mismatch 才 capture structured snapshots，并锁存固定域序、RNG、slot/generation 和双方 snapshots。新增 RNG、slot reuse/generation、real test-only entity 三 world cases。尚未获得 Unity compile/test/self-check；未改 battle rules、30 Hz、Scene、资源、配置、S1、Socket、DB、transport 或公网。任何额外 authored script 仍须重新说明范围。
- **2026-08-24 S0-WITNESS-001 编译观察**：当前已打开 Unity 仍未导入新增 witness `.cs`（无 `.meta`、ScriptAssemblies 时间早于源码）。以进程级 `DOTNET_CLI_HOME` 的静态 build 只能看到生成 `.csproj` 未包含新文件，继而在两处引用报 `CS0246`；这不是 Unity/source 编译结论。不得手改 generated csproj/meta 或启动第二个 Editor；下一步由当前 Editor 执行正常 refresh/import 后取得真实编译证据。
- **2026-08-24 S0 validation Ledger 外部结果**：`Tools/Validate-ChangeLedger.ps1` 失败于三个非 S0 的 `BattleBackgroundPlatform*` authored script diff 及 `CAMERA-PLATFORM-BACKGROUND-001` 声明不一致。没有修改、清理或收编这些文件；这是 S0 Ledger PASS 的外部治理缺口，不否定已取得的 compile/self-check 证据。
- **2026-08-24 服务器优先顺序（覆盖本条之前的客户端下一步）**：用户要求暂停修改客户端，先专注独立服务端脚本。`S0-INPROC-AUTHORITY-001` 保持 `CODE_WRITTEN / USER-DIRECTED HOLD`；其 Unity 编译、focused、自检和多 world runtime 延后，不得晋升或删除。独立 Server solution、generic authority-session/TestKernel、build/test/run/审计闭环已由上方两条实际证据完成；当前下一门是 `CLIENT_INTEGRATION_REQUIRED`，在恢复 Unity adapter 与跨端 checksum 前不能把 S0 标为 `VERIFIED`。
- **2026-08-24 持续目标已建立**：当前目标线程 `01a0324a-1bc3-7702-9787-b5e1ccff5111 / ACTIVE`。每次上下文恢复都必须先读服务器 progress Resume Card，再按当前服务器 Work Package 继续；每个包持续更新 Task Contract、Change Record、Ledger、STATE 和服务器进度证据。若继续推进必须修改、编译或验证 Unity Client，必须先写 `CLIENT_INTEGRATION_REQUIRED`，随后暂停等待用户明确批准，不得自行跨越。
- **2026-08-24 历史服务器 Work Package（已被上方实际完成证据覆盖）**：`S0-SERVER-BOOTSTRAP-001 / PREIMPLEMENTATION / READY_WHEN_WORKSPACE_WRITABLE` 是目录和 .NET 10 仍缺失时的预实施记录；它不再是当前事实。现已存在独立 Server repository、.NET SDK `10.0.400` 与完成的 bootstrap/in-memory packages；Unity Client 仍冻结。
- **2026-08-24 历史服务器目标阻塞（已解除）**：连续三轮目录/SDK 缺失的原始 blocker 已由当前 Server workspace、.NET 10 与两项完成包解除；不得把这段历史写回当前阻塞。
- **2026-08-24 历史权限重载证据（已解除）**：旧 sandbox access-denied 探测只说明当时任务未重载 writable root；当前 Server 文件、独立 Git 与测试证据已经存在，不能再把它当作当前 blocker。

- **2026-08-23 WEB-CADENCE-001当前状态**：`RUNTIME_PENDING`。已在 `Tools/DatSkillFlowWeb` 新增独立
  `render-cadence.html`、pure sampler、只读 server flag 与专用 launcher；三栏共用当前
  `ntsd_cpp + NTSD 2.4.1` Native preview trace，只插值 presentation position/camera，绝不改
  frame/DAT wait/opoint/hit/logic。build、48项focused、实际OID2 `open→16-tick preview→close` 与 403
  `read-only-mode` 写拦截与Change Ledger validator均通过；Canvas 人工视觉验收仍待。正常 `index.html`、DAT 编辑/保存、C++、Unity、资源均未改。
  全量 `npm test` 为392 pass、2个既有 main.ts 静态正则失败；详见`WEB-CADENCE-001` Change Record 和 handoff。

- **Milestone**：C++ Release → Unity 战斗场景重新对齐。
- **顶层执行目标**：按 `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md` 的 R1～R8
  依赖顺序完成 C++ Release → Unity 战斗场景重新对齐；该目标持续有效。每个实际脚本批次仍必须是
  此目标下独立、可回滚的 Work Package，并先建立 Task Contract 与 Change Record；顶层目标不能
  跳过其分层验收或保护边界。根据 `D-009`，计划内 Work Package 连续推进，不再逐包重复等待确认；
  只有真实范围扩大、authority / 保护边界或用户 Change Request 才停止。
- **当前阶段**：C++→Unity 全量差异盘点的静态 source 阶段已完成；R1-SOURCE-001～007 已建立 COV-001～006 合同、唯一差异总台账、依赖图、future repair batches 与分层验收矩阵。`R2-PASS-01`、`R2-PASS-02` 都处于 **RUNTIME_PENDING**；两者仅取得 source、编译和 focused self-check 证据，不是完整 battle 对齐。
- **2026-08-23 当前R07A结果（覆盖下方旧D-RENDER-002状态）**：`D-SCHED-009 + D-RENDER-002 =
  UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。actual collision/hit→frozen publication→same-tick live
  writeback→formal central materialization→Late幂等→next-tick producer/RNG及no-publication生命周期已由worker
  Play tick843～846通过；compile0、18/18、178/178、13/13、20:25:11 self-check、final Console0与
  ledger82/97均PASS。R07B/R07C/R08未执行。
- **2026-08-22 历史R6结果（已被上条R07A覆盖）**：`D-RENDER-002 / R6-PRES-005 / RUNTIME_PENDING`。
  当时Unity已写入RenderDispatch immediate frozen-cycle finalize、CentralOnly no-publication direct advance及
  `[0,5,38,39]`/unavailable/idempotence matrix，但尚无Play证书；该证据演进见R07A收口记录。
- **2026-08-22 R6 adapter认证收口**：`A-RENDER-002 / R6-PRES-006`与`A-RENDER-003 / R6-PRES-007`均为`RUNTIME_PENDING` no-code certification。1.5×只作用body/held表现几何并由scale-delta保持wpoint重合，不进入逻辑pixel/world；fixed-world tick清零release camera/RenderOffset，safe-area只写presentation camera。两项existing fixture均由19:49:12 fresh full self-check实际覆盖并PASS。真实DAT/atlas held锚点、URP safe-area/scene边缘仍留R8 PlayMode；snapshot restore→PreFrame前直接发布可达性为UNKNOWN。
- **2026-08-22 R6自动证据层结论**：D-RENDER-001～005及A-RENDER-001～003已完成source/现有自动证据闭合，所有相关项仍仅`RUNTIME_PENDING`，不是C++ runtime/PlayMode视觉证书。按主计划现在可进入R7逐项优化重新认证；R8继续承担真实战斗场景验收。
- **2026-08-22 当前R7结果**：`D-PERF-001 / R7-PERF-001 / RUNTIME_PENDING`。death-cleanup private proof producer与T14 cross-pass consumer已删除；同点whole-pass proof、three-pass writer、participant filter及public stress/report schema保持。用户Refresh并恢复UnityMCP session后，fresh Unity DLL为20:16:44/45且晚于20:02 source；focused EditMode job `09948d3e3e314d84ab80791d0d2b2070`为15/15 PASS，实际覆盖same-slot current kind2 oracle与两项warmed 0 B；20:22:37 full `BattleRuntimeSelfCheck=PASS`。`B-R7-PERF-001-01`已解决，旧19:49:12 PASS未复用。Play Mode/C++ runtime trace仍待，不能宣称完整VERIFIED。
- **2026-08-22 R7-LATE结果**：`D-LATE-001 / R7-LATE-001 / RUNTIME_PENDING`。entity三段reload与world-owned 4×217+1×218 writer已落地；missing target frame0/8000 HitStun140、最低空槽、34-call RNG、missing217/218、no-slot、generation与高低cursor矩阵均通过。fresh Unity DLL为20:41:14，20:42:47 full self-check=`PASS`，warmed no-slot 512次为0 B/0 RNG。首次CS0234与第二次6×CS0122均已留在Change Record并最小修复。真实DAT Play Mode、GameObject pool表现与C++ runtime trace仍待，不能宣称完整VERIFIED。
- **2026-08-22 R7 Frame/Recovery只读预检**：已建立`R7-FRAME-01-frame-tick-recovery-soa-recertification-preflight-20260822.md`，没有修改脚本。除既有`D-MOV-005`外未发现新confirmed difference；recovery与FrameTick主要顺序/公式映射闭合，但current DAT oid51/52 identity、invalid DAT jump flag恢复性仍为INFERRED/UNKNOWN。必须在R7-PERF-001验收后独立认证，不能由旧Unity A/B测试升级为C++ VERIFIED。
- **2026-08-22 R7-FRAME认证结果**：`R7-FRAME-001 / RUNTIME_PENDING / NO-CODE`。current DAT inventory确认state2000只在type2/type4 weapon，D-MOV-005继续为exact route `INFERRED not reachable`；exact identity writer inventory未发现OID51/52 shell/current-DAT分离。fresh focused job `7b5d94953fca4cdb8947aaa2350277ca`为22/22 PASS，覆盖Recovery/FrameTick legacy-vs-data-oriented、fallback及warmed 0 B；20:42:47 full self-check仍PASS。无脚本改动；mod DAT、identity未来分离、invalid DAT jump flags、Play Mode与C++ trace仍未关闭。
- **2026-08-22 R7 AI旧测试合同修正完成**：`R7-AI-TEST-001 / VERIFIED / TEST-ONLY`。初始UnityMCP job `6fdd44f773344cffbce04404bfddfd86` 捕获旧dead断言expected 0 / actual 1；C++ `prepare_ai_input`与`R3-AI-LIFE-001`确认active HP=0 AI没有self-HP early return。fixture已拆为dead eligible/applied/context-bind与coordinate zero-attempt；production AI未改。fresh Editor DLL 21:01:39、Console error 0、exact job `8c74d8e0a76e427fac3fd7920f5ac234` 2/2、AI sensing/profile job `5c6bad85dc0b43c2a6949d03cfd256fc` 111/111、21:04:52 full self-check及validator/diff均PASS。该VERIFIED只裁决测试合同，`R3-AI-LIFE-001`与完整AI gameplay仍为`RUNTIME_PENDING`。
- **2026-08-22 R7 AI sensing认证结果**：`R7-AI-01 / RUNTIME_PENDING / NO-PRODUCTION-CODE`。C++ `input_handler.cpp:1209-1235,1615-1898` 的first10 move-mode、ground/air target、ground-derived best/lane、cache retain/refresh、team guard与slot20+ special scan已映射到Unity fallback/SoA/indexed/unified authority；生产profile确认为`DataOrientedCanonical`。除上述stale Editor fixture外未发现新的production source-confirmed difference；exact 2/2、AI sensing/profile 111/111及21:04:52 full self-check PASS。`input_handler.cpp:1900+`完整OID decision tree、真实Play Mode、C++ trace与>399 capacity extension语义仍未关闭，下一包必须独立为`R7-AI-02`。
- **2026-08-22 R7-AI-02 decision chain inventory**：`SOURCE-CONFIRMED DIFFERENCE / NO GAMEPLAY CHANGE`。C++ `input_handler.cpp:2055-2204` 的outer random gate内有39个有序helper/call positions；Unity Legacy与DataOriented均只保留positions1–6，缺失7–27与29–37共30 positions，并把现有28/38/39错误放到gate外。另确认optimized snapshot缺OID11 frame290 side-effect所需current frame`hit_j`。现有decision job `3eaff2c1bb474565b2dd4c66d02c49db` 75/75 PASS只证明两条Unity路径共享缩减oracle，不能关闭C++差异。已登记`D-INP-007A/B`、`D-INP-008`、`D-INP-009`并拆为02A～02F；当前未修改production，下一步必须先做02A authority fixture/dispatcher合同。
- **2026-08-22 R7 broadphase认证结果**：`R7-BROAD-01 / RUNTIME_PENDING / NO-CODE`。C++ slot pair→双方向→ITR/BDY exact顺序与Unity BruteForce、role-aware authority-ordinal sort、双方向exact、degenerate fallback和RNG restore已映射；fresh jobs `b5ea30da3c4e42468977e3ab10868fe6` 9/9、`7798184d88024764971712a9a780029e` 58/58、`201e1b9127004d349b14b06df2aa4e6b` 16/16，合计83/83 PASS。focused suite后full self-check在R3-INP-01连续失败，但无代码变化且domain reload后22:13:06恢复PASS，登记`D-TEST-001`静态测试污染。另登记`D-PERF-002`：production GameConfig为空且resolver默认BruteForce，普通NTSD_Battle尚未部署LooseQuadtree；stress显式backend不等于production接线。Play Mode/C++ trace仍待。
- **2026-08-22 R7 frozen/worker认证结果**：`R7-PRES-WORK-01 / RUNTIME_PENDING / NO-PRODUCTION-CODE`。C++ render observation point、Unity frozen capture、latest/world/generation materialization gate与worker publication/ack single-flight已完成source mapping；fresh positive jobs `ab2811b35d8e42f9b0ce8ed4733ed0ed` 13/13、`26be6db261c54e45ae8c15f5cf1a5a11` 11/11、`7e64d65b61924459b9419fd1d5d4bc34` 6/6、`3789a22c55504027b33b0204c6e5f96e` 16/16，合计46/46 PASS。production确认为CentralOnly+dedicated worker+maxCatchUp=1。完整worker suite及fresh-domain exact test都暴露`D-TEST-002`旧current-key清零断言错误；production current key=1与C++一致。另登记`D-TEST-003` joint driver/central/ack覆盖缺口与`D-PERF-003` single-flight部署边界。本包未改脚本；fresh-domain Unity Console编译错误为0，22:32:29 full self-check PASS。下一步为pool/slot/dynamic capacity只读盘点。
- **2026-08-22 R7 pool/slot/capacity盘点结果**：`R7-CAP-01 / INVENTORIED / D-CAP-001 OPEN / NO CODE CHANGE`。C++ slot50 lowest-free与Unity min-heap/page/generation/pending-release映射闭合；focused job `4cc1de5fb20b49609ee0824cd64c4af4` 44/44 PASS，覆盖Mobile 1000、lowest reuse、generation/snapshot、logic pool families、pooled reset、sealed rejection与warmed 0 B。`PoolMaxSize=200`仅warning。confirmed差异是DesktopExtended seal后拒绝page growth，Windows默认512因此battle-time存在prepared-capacity hard cap，违背文档“动态、无production hard cap”保护条款。必须先做R7-CAP-01A容量/0B/admission合同决策，当前不改代码。fresh-domain Unity Console为0条error/warning，22:45:05 full self-check PASS。R7计划列出的优化组至此已全部完成inventory；下一步先汇总repair WPs，不直接进入实现。
- **2026-08-22 R7完整inventory checkpoint**：R7八组优化盘点全部完成，汇总见`RESEARCH/R7-INVENTORY-SUMMARY-20260822.md`。未关闭的gameplay/data差异为`D-INP-007A/B`、`D-INP-008`；coverage/test为`D-INP-009`、`D-TEST-001/002/003`；deployment/architecture为`D-PERF-002/003`、`D-CAP-001`。修复序列已固定在`TASKS/R7-REPAIR-SEQUENCE-after-complete-inventory.md`。当前开始第一个实施包`R7-AI-02A`，它只能建立C++ source-derived 39-position/gate/RNG test oracle，不允许修改production AI。
- **2026-08-22 R7-AI-02A结果**：`R7-AI-002A / VERIFIED / TEST-ONLY RED-WITNESS CONTRACT`。39-position ordinary contract 1 PASS/2 Explicit skipped；position7 job `ae7bf5e441c845628067227aacd36c81`捕获expected DRJ=3/actual0，position28 job `8ba6322f7a984f669afd80610bad5c5e`捕获expected DUA=0/actual3。首次fixture漏计boundary RNG draw及AssetDatabase未导入的total0请求均已作废留档。fresh Editor DLL 22:57:58、Console 0 error、existing AI job `0417660e5b6440c98d93e2c0fb7c8ae1` 75/75、23:01:13 full self-check PASS。production未改，`D-INP-007A/B`仍open；下一包为`R7-AI-02B` HitJ data contract。
- **2026-08-22 R7-AI-02B结果**：`R7-AI-002B / RUNTIME_PENDING`。C++ `ai_frame_hit_j`读取current logical frame DAT `hit_j`、缺失为0；Unity现已为snapshot与frame-motion canonical store补齐HitJ。DAT只在fallback capture或frame bind/write边界解析，UnifiedAuthority consumer只读SoA projection；Frame pending同点发布HitJ，full/refresh comparison与grow/copy均覆盖。最终focused job `d0670d95986c41e7b115b8a77754d23b` 4/4、AI regression `a525edc1f1c64bfe854f3d3218bd1d4e` 212/212、warmed 0 B、fresh Unity compile 0 error、23:43:36 fresh-domain self-check PASS。23:34:04同domain self-check曾在既有`D-TEST-001`失败并已留档。OID11 helper仍未接，真实DAT PlayMode/02F/C++ trace待后续；下一包为02C。
- **2026-08-23 R7-AI-02C结果**：`R7-AI-002C / RUNTIME_PENDING / UNWIRED MODULE`。positions7–16已实现为非partial实例`AiCharacterDecisionModule`；kernel私有RNG clone已无行为地提取为共享值类型，默认dispatcher未调用module。source-derived focused job `b65adcac443844c183272c984934d061` 19/19、clean AI baseline `12463c4731a24aa4ae9919f96599f720` 212/212、warmed 0 B、fresh Unity compile 0 error、00:10:58 full self-check PASS。组合job `202c6992fb784d219c02d1318f605068`只在两个02A Explicit red witnesses按预期FAIL，证明position7/28默认差异仍存在。下一包为02D；02F前不得接线。
- **2026-08-23 R7-AI-02D结果**：`R7-AI-002D / RUNTIME_PENDING / UNWIRED MODULE`。positions17–28已加入现有非partial实例module；position21使用显式scan域并保持400-slot source fixture、strict-farthest/first-tie，position21/22继续，position24/26保留dynamic-modulus RNG顺序。首轮job `4c3e6f659c384014a0eefa180d6ae5c6`唯一失败是OID19漏计position26 path-B前序draw，留档修正后focused `5d265876f2e24159879cd881e7218d80` 26/26、AI regression `3cd69caca0f546338f1ced0500cb4062` 238/238、warmed 0 B、generated Editor build/Unity compile 0 error、00:33:49 fresh-domain self-check PASS。job `0173e28d95bf44ab9df97facc81193cc`两个02A red witness仍按预期FAIL，证明默认dispatcher与position28 production位置未改；下一包为02E。
- **2026-08-23 R7-AI-02E结果**：`R7-AI-002E / RUNTIME_PENDING / UNWIRED MODULE`。positions29–37已加入现有非partial实例module；position30保留first-20 first-match/no-obj-type filter，position31 frame263/264写jump后继续，position34保留first-100/self-inclusion与gate命中后无目标也return true。首轮job `2c095df1751a442c99d61d7c26a3f1db`两个失败是OID5/14漏计position29前序draw，留档修正后focused `1a2716b9caee4fa8bfd6285fc0c3f738` 31/31、AI regression `0eddc4e7c54840d3b5db41d035b63eb3` 238/238、warmed 0 B、generated Editor build/Unity compile 0 error、00:48:12 fresh-domain self-check PASS。job `7a865915ba984168abe0636da7bac54c`两个02A red witness仍按预期FAIL，证明默认dispatcher与production position28未改；02C～02E unwired模块已齐，下一包为02F联合接线合同。
- **2026-08-23 R7-AI-02F结果**：`R7-AI-002F / RUNTIME_PENDING`。Legacy与DataOriented已原子接入outer-gated positions1–39，positions28/38/39不再gate外重复执行；snapshot持久module、pass级shared rows、构造期预分配fallback、RNG trace与matched-position shadow均闭合。self<400保留400域，extended self使用完整capacity，first20/100不扩展。首次夹具前序RNG、旧shared计数、隐式InputHistory分配与fallback预热问题均已留档修正。final authority 3/3、full dispatcher 5/5、fixed-seed production profile-pair 1/1、AI矩阵286/286、warmed 0 B、Unity compile 0 error。02:03:05同domain self-check复现既有D-TEST-001静态污染；domain reload及最终test-only cleanup后，02:07:58 final fresh full self-check PASS。`D-INP-007A/B`的代码级差异已关闭，`D-INP-008/009`自动证据已闭合；真实AI PlayMode、R8与C++ trace仍待，不能宣称完整AI/battle VERIFIED。
- **2026-08-23 R7-TEST-002结果**：`R7-TEST-002 / VERIFIED / TEST-ONLY`。C++ `InputHandler::poll`确认首次Left+Attack后current key=1、Prev=0；旧worker fixture两条0断言与注释已修为current=1，cooldown/history/publication/ack断言保持。exact job `86e6bddd257f4e18bb37433941f1a916` 1/1、class job `41f7b4803c754635b0d7c16abaf73754` 17/17、compile 0 error、02:14:36 fresh full self-check PASS。production未改；下一包为R7-TEST-003。
- **2026-08-23 R7-TEST-003结果**：`R7-TEST-003 / VERIFIED / TEST-ONLY`。formal driver双tickfixture已联合覆盖`buildPresentation=true` worker frozen publication、CentralOnly exact-tick物化、ack/finalization、next-tick unblock与new frame/generation，并验证host不反写原publication。exact job `8f7e88df654449e38a6ac8df97bb6faa` 1/1、worker+central job `acfb083ac4fc458e999a9715b4f45dca` 31/31、dotnet/Unity compile 0 error；focused后force scripts domain reload，02:27:37 fresh full self-check PASS。production worker/driver/render、single-flight、catch-up和gameplay均未修改；下一包为R7-TEST-001静态污染隔离。
- **2026-08-23 当前执行包状态更新**：`R7-TEST-001 / CODE_WRITTEN / TEST-ONLY`。fresh-domain二分已把D-TEST-001锁到`AiDecisionSoAShadowEditorTests.SharedShadow_BuildsOnceAndRefreshesLowSlotBeforeHighSlotEvaluation`：该owner 1/1 PASS后same-domain self-check于02:52:32在R3-INP-01失败。owner测试现已直接witness missing-frame绑定的静态`LF2FrameCache.EmptyFrame.state`被写14，并用finally恢复原值；production AI/frame/input/scheduler未改。当前等待compile及owner/class/286 matrix→same-domain self-check。
- **2026-08-23 R7-TEST-001隐藏依赖补充**：owner cleanup后的exact owner 1/1→same-domain self-check已于02:57:57 PASS；但完整class首次运行暴露`UnifiedAuthority_AscendingRefreshMakesLowVisibleToHighWithoutReverseEarlyVisibility` expected14/actual0，fresh-domain exact也失败。该fixture过去依赖owner遗留的sentinel14；其自身shared-shadow hook没有形成data-oriented canonical-store refresh witness。仍在同一test-only Record内改用现有character-input mutation override并清理sentinel，production不改。
- **2026-08-23 R7-TEST-001结果**：`VERIFIED / TEST-ONLY`。D-TEST-001已二分到shared-shadow owner对静态`LF2FrameCache.EmptyFrame.state`的未恢复写，并发现unified ascending fixture依赖该污染。owner/dependent现均显式own+finally恢复sentinel；dependent使用character-input mutation override形成canonical post-input full refresh。final dependent exact `b8e926eb862a4c4a83ed3124180f3267` 1/1、class `4a05c94370434bddbd1e2afc38425c9e` 66/66、AI matrix `c90b67cf6eb740dfb2ed2715f56dbaf4` 286/286；class后03:03:54、AI matrix后03:06:15同域self-check均PASS，final fresh compile 0 error与03:07:32 self-check PASS。production未改；下一包为R7-BROAD-02 decision matrix。
- **2026-08-23 当前执行包**：`R7-BROAD-02 / IN_PROGRESS / NO CHANGE`。production broadphase仍为空配置→BruteForce。synthetic 1000 fixture已有499,500→500 pair reduction且候选/RNG一致，但历史真实1000-AI harness本身强制Loose且没有current-build同输入Brute/Loose A/B，因此不授权切默认。先fresh复跑83项parity与same-domain self-check，再形成retain/defer决策；本包不改代码/配置。
- **2026-08-23 R7-BROAD-02决策**：`DECISION COMPLETE / RETAIN BRUTEFORCE / NO CHANGE`。fresh role-aware/formal/Loose/participant job `623e91b88792432a87bccd0969b08ba9` 80/80、AirRole nearest `01bea6cea2e340e088683226c81cf713` 8/8，合计88/88；随后same-domain full self-check于03:13:57 PASS。synthetic pair reduction成立，但current-build真实production Brute/Loose A/B、R8 scene parity和real fallback distribution未闭合；历史1000-AI harness本身已是Loose仍未达30Hz。因此保持GameConfig空→BruteForce，未来切换必须独立配置Record。下一包R7-CAP-01A。
- **2026-08-23 R7-CAP-01A结果**：`DECISION COMPLETE / CURRENT CODE CONFORMS / NO CODE`。合同固定为Desktop无固定产品active cap，但每局在unsealed loading/reset/preflight边界按页准备有限、可配置预算；active battle seal后strict 0 B，超预算deterministic reject，不临时unseal/new。Authority400与Mobile1000不变。fresh jobs `fdf01d6739ac47748158eb42d6d81926` 11/11与`e61ed948fc544caf8cc93b31f7859126` 33/33，合计44/44 PASS；03:19:45同域full self-check PASS。现有production符合，D-CAP-001按合同澄清关闭，R7-CAP-01B不需要实施。R7 orders1–11至此关闭，下一阶段R8。
- **2026-08-23 当前执行包**：`R8-WP01 / IN_PROGRESS / CERTIFICATION-ONLY`。已建立current-worktree R8分层矩阵：01A fresh compile/self-check/EditMode；01B移动/输入；01C关系/opoint/lifecycle；01D CentralOnly可见性；01E 1000 active/0GC/30Hz；01F Windows Mono/IL2CPP；01G汇总。UnityMCP已确认Session Active/Configured、socket6401，非项目探测错误已清空。旧U9只作历史baseline，不作为fresh R8证书；任何脚本修复必须先独立Task/Change Record。
- **2026-08-23 R8-WP01A阻塞与修复准备**：fresh compile 0 error/0 warning、03:28:51 full self-check PASS；但full EditMode job `20fcc884b4114ee9a1a3b7f1667c641c`在1357项后FAILED。至少25条capped failure均为BattleHitExecutionPlan ShadowCompare `writerDiff=0x70000000000000`；fresh exact `8d6f29aa8d8043958b29abcf58096e6e` 2/2 FAILED，排除顺序污染。mask对应TargetHp/HPBound(or PP)/ComboCountVic；production R4-HIT-001/003已写，shadow projection未同步。已建立`R8-TEST-001 / PLANNED`，只允许修诊断投影，不改production damage。
- **2026-08-23 R8-TEST-001代码状态**：`CODE_WRITTEN / DIAGNOSTIC-ONLY`。已在BattleEcsHitExecutionPlan同步normal Light/Heavy/Throw adjusted vital/stat和standard/state-sync/D1/active-D1 type3 kind0 raw vital/stat projection；Drink/non-converted kind9不变，production writer/tests未改。尚未取得compile/test证据。
- **2026-08-23 R8-TEST-001首次验证与范围修订**：fresh compile 0 error/0 warning；converted-kind9 exact job `91c41aff34a746faa4517462e090bda1` 2/2 PASS。整类job `9e09666033394a0b8cdb530135d85da7` 178项仅余`StandardType3DamageSupportsDeadAirTarget` expected HP0/actual-10；R4-HIT-001明确kind0 injury仍写HP，因此这是旧断言。已在同Record增加该test path，只允许把HP期望改为-10，frame/fall/production不变。
- **2026-08-23 R8-TEST-001 focused结果 / R8-TEST-002准备**：dead-air exact `0bbeee8428f8406bb8f8ee06b09ba9c9` 1/1、hit-plan class `69e73f14e34c428eb54803db3327cf85` 178/178 PASS；full job `246be3d87338446ea7a877b13f7f88f5`中原hit failures清零，但1357项最终仅W07 structural一项失败。fresh exact `f453a20619b34ef0afe3716a902d7629` 1/1 FAILED。W07仍期待invalid positive link清TargetSlot/HeldWeapon，而R5-LINK-001/C++只清LinkState。已建立`R8-TEST-002 / PLANNED`，只改W07 test fixture，production不改。
- **2026-08-23 R8-TEST-002代码状态**：`CODE_WRITTEN / TEST-ONLY`。W07 fixture与event assertions已从旧0/-1/-1同步为LinkState/Target/Held=0/1/1，target reverse仍2/0；方法名同步。production link/event producer未改，尚未取得compile/test证据。
- **2026-08-23 R8-TEST-002编译与工具状态**：`COMPILE_PASS / TEST-ONLY`。Editor DLL晚于source，dotnet为0 error/18 existing warnings；validator 56 Records/55 governed files PASS。UnityMCP 6401 listener在本次domain reload后未恢复，Unity PID2880仍存活/响应，Editor log未见error CS但有Unity内部TaskCanceledException。等待用户在MCP面板重新Start Session后继续exact/class/full/self-check；禁止启动第二个Editor。
- **2026-08-23 R8 MCP恢复点（覆盖上条旧PID）**：旧Editor已结束；新国际版Unity 2022.3.62f3 PID36240于07:24打开正确project，Package Manager/AssetDatabase/Tundra 2.23s均成功，当前无C# compiler error。新实例尚未启动MCP Session，6401无listener，故`B-R8-MCP-001`成立。所有离线工作已完成；用户只需在MCP For Unity面板点击`Start Session`，随后直接从R8-TEST-002 exact→class→full→self-check恢复，不重做代码或定位。
- **2026-08-23 R8-WP01A最终自动基线**：MCP恢复后，R8-TEST-002 exact `ad10828ee9d741aa8c2068c1ad7db6c8` 1/1、class `4101eded225e493aa48ad1f4549e6d54` 4/4、full EditMode `6a6336d0e1e94abd9585110358012ca5` 1357/1357 PASS（0 failed/0 skipped，170.1558765s）；同域full self-check 07:31:17、强制域重载后fresh self-check 07:32:39均PASS。R8-TEST-001/002均以diagnostic/test-only VERIFIED关闭，B-R8-MCP-001已解除。下一步进入R8真实Play Mode/central/capacity/Player认证；不得把自动基线扩大为battle已对齐。
- **2026-08-23 R8真实Play Mode首个阻塞**：`R8-PLAY-001 / IN_PROGRESS / EDITOR-DIAGNOSTIC-ONLY`。MCP重新确认Session Active/Configured后进入`NTSD_Battle` Play Mode，Console持续报告`NTSDHitboxGizmos.cs:47`把纯C# `LF2Entity`用于`GetComponentInParent<T>`的`ArgumentException`。已停止Play并在脚本修改前建立Task/Change Record/Ledger/handoff；下一步只通过现有`LF2ObjectRenderer.LogicObject`绑定修复selection解析，再做compile/self-check/Play Mode异常清零。gameplay/CentralOnly/C++ authority均未授权改动。
- **2026-08-23 R8-PLAY-001代码状态（覆盖上条执行状态）**：`CODE_WRITTEN / EDITOR-DIAGNOSTIC-ONLY`。selected gizmo路径已改为先向父级、再向子级读取现有`LF2ObjectRenderer.LogicObject as LF2Entity`，不再把纯C#对象当Unity Component；非selected world snapshot、碰撞盒数学、gameplay和CentralOnly均未改。尚未取得fresh compile/self-check/Play Mode证据。
- **2026-08-23 R8-PLAY-001最终结果（覆盖上条状态）**：`VERIFIED / EDITOR-DIAGNOSTIC-ONLY`。force scripts reload后Console 0 error/warning，07:40:57 fresh self-check PASS；清空Console进入`NTSD_Battle`等待15秒后原`GetComponent<LF2Entity>`异常与全部error/warning均为0。hierarchy确认两个active实体及各自EntityModel renderer binding；validator 57 Records/56 governed files PASS。此结果只关闭gizmo诊断污染，不验证input或CentralOnly可见性。用户随后明确报告实时按钮组合无法释放技能；`D-INP-006 / R3-PHY-01`此前仍是UNKNOWN，现进入R8-WP01B独立first-difference诊断，不得把旧单序列注入成功当作当前物理输入证书。
- **2026-08-23 R8-WP01B输入现状**：`R8-INP-01 / PLANNED / DIAGNOSIS-FIRST`。asset静态确认Player_1为W/S/A/D/J/K/L；C++每game tick按current held生成prev/new-edge。Unity source确认local provider只在tick submission采held、direct callback packet随后被丢弃，dedicated worker single-flight又要求publication/presentation ack后才采下一tick；现有test只覆盖按住跨采样点，不覆盖低帧/in-flight多边沿或真实组合。以上是source风险和coverage gap，不是已证明根因。下一步必须记录InputAction→FrameInputSet→roster→Runtime key/cd/combo/frame的first difference；未闭合前不改技能、DAT或组合窗口。
- **2026-08-23 R8-INP-01 neutral runtime checkpoint**：transition期间的tick0瞬时读数已由`get_editor_state.is_changing=true`证明无效；transition完成后fresh Play为tick681、object8、Roster两名human正确绑定slot0/1、paused=false、dedicated worker active/no failure、LastAppliedFrameInput含player0/1 neutral，CentralOnly UsesCentralPixels=true且Console 0 error。故稳定bootstrap/roster/global pause已排除。非改项目的自动物理按键注入受Codex桌面隔离阻断；MCP execute_code又因Roslyn未安装/CodeDom命令过长不可用。按总计划边界，下一步需用户确认后先建`R8-INP-001A` diagnostic/test-only Change Record，不得直接改production input或技能。
- **2026-08-23 R8-INP-01 first difference（覆盖上条下一步）**：继续只读authority后已排除J/K/L crossed mapping差异：C++ `DEFAULT_P1`本身按internal field order用L/J/K。确定差异为`D-INP-010 / R3-COMBO-001 PLANNED`：C++九combo字段按引用即时持久化；Unity resolver复制local并在`comboDja!=3`、valid/failed DJA、guard、Unk328等绝大多数return前不写回。现有`CheckComboLocalShadowCommitContracts`与`CheckStaggeredNarutoDefendDownJumpInput`还明确把该缺陷写成green oracle。Task/Research/Change Record/Ledger/Decision/handoff均已在脚本修改前建立；尚未改脚本。按当前目标边界等待用户明确实施确认；不得先改physical input、worker、DAT或Naruto专项。
- **2026-08-23 R3-COMBO-001阻塞状态（覆盖上条package状态）**：`BLOCKED / B-R3-COMBO-001-01`。source first difference、最小resolver/test范围、rollback和验收矩阵已完整闭合，validator 58 Records/56 governed files PASS；尚无本包脚本diff。当前顶层目标要求R3+包记录后等待用户确认，连续目标续跑未收到明确批准，继续只读也无新证据价值。恢复条件：用户明确批准实施`R3-COMBO-001`；恢复后直接改resolver并修正stale oracle，不重做定位。
- **2026-08-23 R3-COMBO-001实施恢复（覆盖上条package状态）**：`IN_PROGRESS`。用户已明确回复“同意修改，继续处理”，`B-R3-COMBO-001-01`解除。继续严格按既有Task/Change Record实施resolver九combo字段by-ref即时持久化和source-conflicting测试修正；physical mapping、FrameInputSet、worker、DAT、Naruto专项、opoint与render均不在本包。
- **2026-08-23 R3-COMBO-001代码状态（覆盖上条package状态）**：`CODE_WRITTEN`。resolver现直接`ref input.Combo*`执行八方向与DJA wrapper；self-check已改为C++ source-derived early-return状态和Naruto物理L→S→K跨tick正向触发。尚未编译/运行；不得把此状态表述为修复通过。
- **2026-08-23 R3-COMBO-001编译状态（覆盖上条package状态）**：`COMPILE_PASS`。Unity 2022.3.62f3 fresh Tundra 4.72s成功，目标0 error，Assembly-CSharp晚于源码；既存nullable/unused warnings独立。focused/self-check/Play Mode仍待验。
- **2026-08-23 R3-COMBO-001首次自检反馈（覆盖上条package状态）**：`CODE_WRITTEN`。08:08:18 full self-check在OID51 missing-target stale oracle真实FAIL；C++ trigger path明确在frame-jump调用后清零DJA。missing/valid target两条旧transactional-discard断言已改为private/runtime 0，需重新编译与复跑。
- **2026-08-23 R3-COMBO-001二次编译（覆盖上条package状态）**：`COMPILE_PASS`。OID51两条断言修正后fresh Tundra 2.28s、目标0 error、DLL晚于源码；full self-check复跑与Play Mode仍待。
- **2026-08-23 R3-COMBO-001第二次自检反馈（覆盖上条package状态）**：`CODE_WRITTEN`。08:10:17 full self-check在oid6 guard旧transaction-discard断言FAIL；按C++ wrapper顺序已将guard/release分别改为ordinary0/DJA3与ordinary0/DJA0。需重新编译复跑。
- **2026-08-23 R3-COMBO-001三次编译（覆盖上条package状态）**：`COMPILE_PASS`。oid6 guard/release修正后fresh Tundra 2.47s、目标0 error、DLL晚于源码；full self-check再次复跑待验。
- **2026-08-23 R3-COMBO-001第三次自检反馈（覆盖上条package状态）**：`CODE_WRITTEN`。08:12:09 full self-check在held-right partial旧断言FAIL；实际frame102/combo1/cooldowns0符合C++顺序。同组right/left两条均已改为step1持久化，后续interrupt负向断言保留。需重新编译复跑。
- **2026-08-23 R3-COMBO-001四次编译（覆盖上条package状态）**：`COMPILE_PASS`。right/left partial修正后fresh Tundra 2.06s、目标0 error、DLL晚于源码；full self-check再次复跑待验。
- **2026-08-23 R3-COMBO-001自检通过（覆盖上条package状态）**：`FOCUSED_TEST_PASS`。fresh full `BattleRuntimeSelfCheck`于08:14:02 PASS，覆盖本包跨tick、early branch、guard、missing/valid、same-tick与L→S→K合同；此前三次stale-oracle FAIL均保留在Record。EditMode input regression与真实Play Mode仍待，不能提升为完整对齐。
- **2026-08-23 R3-COMBO-001 EditMode反馈（当前修正）**：job `ab3e2977fee04f888730e1f44464c443`完成47个目标测试，1个FAIL为AI resolver fixture陈旧`ComboDra` expected2、actual3（不是DJA）。测试脚本先纳入Record，再把断言改为source-equivalent 3；需重新编译/复跑。full self-check PASS事实保持，但EditMode回归尚未通过。
- **2026-08-23 R3-COMBO-001 Editor重编译（覆盖当前package状态）**：`COMPILE_PASS`。Editor fixture修正后fresh Tundra 1.49s成功写入Assembly-CSharp-Editor，目标0 error；同一47项input矩阵复跑待验。
- **2026-08-23 R3-COMBO-001 EditMode通过（覆盖当前package状态）**：`FOCUSED_TEST_PASS`。job `135495e273a646539f7b42eca9b8611b`为47/47 PASS、0 failed/skipped，覆盖input pass/store/provider/delayed packet/crossed mapping/warmed allocation回归；08:14:02 full self-check仍为fresh PASS。真实Naruto组合Play Mode待验。
- **2026-08-23 R3-COMBO-001 Play probe前置**：真实`NTSD_Battle` bootstrap已完成并生成两个id2角色；MCP动态CodeDom因引用命令行过长、Roslyn不可用而无法查询纯C#实体。Editor-only显式菜单探针已在修改前加入Record，拟通过真实角色InputBuffer和真实30Hz tick记录L→S→K frame/combo/object-count；尚未写探针代码。
- **2026-08-23 R3-COMBO-001 Play probe代码（覆盖当前package状态）**：`CODE_WRITTEN`。Editor-only显式菜单探针已写，排入真实场景first player的L/S/K语义并记录DDJ/frame/cooldown/object count到Temp JSON；不自动运行、不进入生产pass。尚未编译/运行。
- **2026-08-23 R3-COMBO-001 Play probe编译（覆盖当前package状态）**：`COMPILE_PASS`。probe fresh Tundra 3.62s、目标0 error、Assembly-CSharp-Editor晚于源码；尚未运行真实场景probe。
- **2026-08-23 R3-COMBO-001首个Play probe反馈（覆盖当前package状态）**：`IN_PROGRESS`。08:28:20真实场景probe因直接排SimInputBuffer事件被canonical FrameInputSet边界丢弃而FAIL，tick314–316 cooldown/combos均0；这证明探针绕层，不裁决gameplay。下一版改用Input System Keyboard设备事件走完整生产输入链。
- **2026-08-23 R3-COMBO-001 device probe代码（覆盖当前package状态）**：`CODE_WRITTEN`。probe现按DDJ状态逐步排Input System Keyboard L→S→K，经过action callback与canonical FrameInputSet，并在命中authored hit_Dj后释放。尚未重新编译/运行。
- **2026-08-23 R3-COMBO-001 device probe编译（覆盖当前package状态）**：`COMPILE_PASS`。device-state probe fresh Tundra 1.75s、目标0 error、Assembly-CSharp-Editor晚于源码；真实场景复跑待。
- **2026-08-23 R3-COMBO-001 L/S/K Play通过（覆盖当前package状态）**：`IN_PROGRESS`。08:32:12 real-scene synthetic InputSystem device probe PASS：tick613/614/615为DDJ1/2/3，tick626进入Naruto authored frame271并清零，后续frame272/273/274，objects 8→20。完整callback→FrameInputSet→resolver→skill chain已覆盖；physical L→front→J仍待补跑，用户实体键盘操作仍属最终人工复核。
- **2026-08-23 R3-COMBO-001 forward probe代码（覆盖当前package状态）**：`CODE_WRITTEN`。同一device probe已泛化为按当前朝向L→A/D→J并观察DLA/DRA→authored hit_Fa，使用独立Temp result；尚未重编译/运行。
- **2026-08-23 R3-COMBO-001 forward probe编译（覆盖当前package状态）**：`COMPILE_PASS`。generic forward probe fresh Tundra 1.18s、目标0 error、Assembly-CSharp-Editor晚于源码；真实场景运行待。
- **2026-08-23 R3-COMBO-001 forward Play通过（覆盖当前package状态）**：`RUNTIME_PENDING`。08:35:39 real-scene InputSystem L→D→J PASS：tick496/497/498 DRA1/2/3，tick509进入Naruto authored frame263并清零，后续264→283→284，objects 7→8。DDJ与ordinary direction wrapper均有production-chain Play证据；final self-check/validator/diff待。
- **2026-08-23 R3-COMBO-001关闭（覆盖当前package状态）**：`VERIFIED`。C++ source by-ref合同、Unity compile 0 error、08:37:09 final self-check PASS、input EditMode 47/47、real-scene InputSystem L/S/K→frame271与L/D/J→frame263、validator 58/58、scoped diff均PASS。该结论仅关闭D-INP-010；R1-WP02 full trace仍BLOCKED，用户实体键盘/窗口焦点edge仍属D-INP-006/R8人工复核。
- **2026-08-22 R7只读预检**：已登记`D-PERF-001`。PreInteraction cross-pass proof在death cleanup后缓存neutral结论，但其后collision/held writer可在同slot改变frame/CPoint/link，消费端只检查occupancy/pending epoch，存在confirmed stale-content skip；C++ T14会读取object consume后的当前状态。推荐R7-PERF-001禁用production cross-pass cache、保留T14当点whole-pass proof。R6-PRES-005 fresh自动验收门已关闭，但正式实施前仍须建立独立Task/Change Record，并先完成R6剩余adapter source认证。
- **2026-08-22 R7 late只读预检**：已登记`D-LATE-001`。C++ late state-special同调用支持9995→4000→8000 reload chain，并在最终state9996/attacking1生成4×OID217+1×OID218；Unity transform提前return、没有9996 writer且exact gate错误skip。现有GT-11零RNG/零spawn断言是旧C#结论，后续必须supersede。正式实施必须建立独立world structural Task/Change Record，不能只修改skip gate。
- **2026-08-22 R6-PRES-04保持活跃**：`R6-PRES-04 / RUNTIME_PENDING` no-code adapter certification；CentralOnly fail-closed ownership已通过source/full self-check，真实URP PlayMode/C++ trace仍待。
- **2026-08-22 R6-PRES-003保持活跃**：`R6-PRES-003 / RUNTIME_PENDING` 已修production shadow cache current-DAT identity；fresh compile/full self-check/validator通过，PlayMode/C++ trace仍待。
- **2026-08-22 R6-PRES-002保持活跃**：`R6-PRES-002 / RUNTIME_PENDING` 仍覆盖BuildCommands direct shadow gate的current-DAT修复；R6-PRES-003只补production visibility cache writer，两者均缺PlayMode/C++ trace，不互相替代。
- **2026-08-22 R6-PRES-01结果**：`RUNTIME_PENDING` no-code certification。C++ active slot→stable signed-Z→same-Z slot painter order与Unity CentralOnly slot capture→stable radix/fallback→indexed rank已闭合；per-entity shadow/body/overlay/hit-record及segment顺序保持。command writer job `5561fce764bc4baa8804ae37ca929417`为6/6 PASS；17:49:18 full self-check=`PASS`。没有修改脚本；C++ trace/PlayMode/GPU像素仍待。
- **2026-08-22 R5-LIFE-01B结果**：`RUNTIME_PENDING`。普通PendingFlushDestroy→slot/generation release→old-object finalization已认证为等价adapter；production FirstPresentationTick仅Reset=0；新增`D-LIFE-001`记录oid7/8→51 dormant partner结构差异，因partner<20且正式battle-time allocator从20/50起，暂为`INFERRED safe adapter`。本包没有修改任何production/test脚本。UnityMCP force scripts refresh/compile request完成（无C# diff，DLL保持17:14:38）；focused EditMode job `582b9e9212264d39b4377b72d7e0374d`为19/19 PASS，17:49:18 full self-check=`PASS`。C++ trace、真实Play Mode和R6 visual仍待。
- **2026-08-22 R5-LIFE-001A结果**：`D-SCHED-012 cursor subset / R5-LIFE-001A / RUNTIME_PENDING`。MobileExtended和DesktopExtended-growth的source700→child900 same-pass、source700→child600 next-pass矩阵已通过；production allocator/registry/pass/profile未改，existing lowest-free fixture继续通过。17:10旧程序集PASS已作废；UnityMCP force refresh后fresh Tundra 23.19s、Assembly-CSharp 17:14:38、无error CS，17:15:48 full self-check=`PASS`。
- **2026-08-22 R5-OP-001结果**：`D-OP-001 / R5-OP-001 / RUNTIME_PENDING`。C++ release birth history=0与late spawn order已映射到Unity four initializer/cache adapter；four-type production factory fixture验证birth current=action/history=0、next snapshot history=current。首次16:54 request PASS因Assembly-CSharp仍停在16:05被判定为stale并作废；UnityMCP force refresh后fresh Tundra 23.19s、Assembly-CSharp 17:14:38、无`error CS`，17:15:48 full self-check=`PASS`。C++ trace/real PlayMode待验。
- **R3 代码闭环状态**：R3的可实施纯脚本子包已全部形成最小代码闭环，分别保持 `RUNTIME_PENDING`；D-MOV-005是 current exact route `INFERRED` not reachable，不改代码。R3物理键位 Play Mode、joint scenario与C++ trace仍是独立验收 backlog，但不阻断开始R4 source preflight。`R1-WP02`仍为 **BLOCKED**。
- **R2 验收覆盖审计**：已完成只读审计，见 `RESEARCH/R2-ACCEPTANCE-COVERAGE-AUDIT-20260821.md`。
  现有 self-check 对 empty tick、single-character poll、two-held pass、candidate→CPoint、Z clamp、
  mode2 tail 和 production skill-object 有分散覆盖，但缺一份统一 R2 scheduler joint fixture；
  `R2-VERIFY-01` 仅被建议、尚未授权，也没有创建脚本 Change Record。
- **权威当前队列（2026-08-22，本段优先于下方历史追加记录）**：
- **2026-08-22 当前独立代码包（覆盖下方过期 active 表述）**：`R5-CPT-002 / RUNTIME_PENDING`。
  `R5-CPT-004` 已在 09:27:38 以 full self-check PASS 关闭 code-level phase-owner 前置条件；
  当前只允许在 `BattleCpointWriter.ApplyHeldInjury` 的既有 positive injury branch补 C++
  `weapon.cpp:50-69` 的 valid `Unk344=1/2` global kill/damage stats，并更新
  `BattleRuntimeSelfCheck` 的专用 matrix。C++ global stats 不以 holder 存在为 gate；negative injury、
  already-attacking、invalid index 均不得写。最小 writer及shared lethal + six-case matrix已通过；
  Unity compile error CS=0、full self-check于09:44:35 PASS，final ledger/scoped diff亦PASS。C++ trace / Play Mode仍待；下一步按连续队列
  已完成 `D-CPT-003 / R5-CPT-003` 的 source preflight、Task Contract、Change Record和handoff，状态为
  `RUNTIME_PENDING`；source澄清 active mismatch必须跳过decrease/actions但保留throw tail/dircontrol，
  且throw读取fallback current frame0 geometry。另已登记 `D-CPT-005`：valid decrease-negative escape
  在C++仍可能进入throw tail，而Unity direct return；它不属于当前R5-CPT-003，必须另建合同。Unity compile
  error CS=0、full self-check于09:59:58 PASS，最终ledger/scoped diff待本次文档更新后重跑。现已建立
  `D-CPT-005 / R5-CPT-005 / RUNTIME_PENDING` 的source preflight、Task Contract、Change Record和handoff。
  valid escape现保留hitcount/knockback后以skipActions/fallback-frame继续C++ tail，focused assertions已写；
  Unity 2022.3.62f3 build success、无error CS，request-file full self-check于16:09:37 PASS；C++ trace /
  Play Mode仍待，且不能回改R5-CPT-003 mismatch。`D-CPT-003`、pass order、held/link、
  opoint、input、collision、render、DAT/scene、array capacity、C++ authority、trace及 Play Mode一律排除。
  该包完成后最高只能是 `RUNTIME_PENDING`。
- **2026-08-22 当前执行覆盖说明**：`R5-CPT-001 / PLANNED` 是当前唯一脚本写入包。C++ release
  CPoint relation、decrease escape、aaction/taction/jaction 与 weapon current-frame held-vaction branches
  只写 frame / explicit 字段；Unity `BattleCpointWriter` 的七处对应 callsite额外清
  `Runtime.FrameWaitCounter`。source preflight、Task Contract、Change Record 与最小 existing CPoint
  fixture设计均已建立；允许脚本范围仅为 `BattleCpointWriter` 和 `BattleRuntimeSelfCheck`。
  `D-CPT-002` injury global stats 与新登记的 `D-CPT-003` reciprocal mismatch control flow 必须保持独立，
  未写脚本前不得扩大。R1-WP02 仍只阻塞 C++ full trace，不阻塞本包。
- **2026-08-22 当前执行状态更新（覆盖上条 PLANNED）**：`R5-CPT-001 / CODE_WRITTEN` 已将
  `BattleCpointWriter` 内合同列明的七处 immediate-reset CPoint callsite收窄为已有 raw CPoint writer；
  existing CPoint self-check已改为 FWC sentinel preservation，并补充 missing caught-slot fallback。
  当前只等待 ledger/diff 与 Unity compile/self-check；未改 `D-CPT-002`、`D-CPT-003`、CPoint pass order、
  throw、held/link、opoint、input、collision、render、DAT/scene 或 C++ authority。
- **2026-08-22 当前执行结果（覆盖上条 CODE_WRITTEN）**：`R5-CPT-001 / RUNTIME_PENDING` 已通过
  ledger validator、scoped diff、Unity scripts refresh后的 `error CS=0` 与 full
  `BattleRuntimeSelfCheck`（`Temp/NTSD_BattleRuntimeSelfCheck.result` 于09:08:02为`PASS`）。
  首次自检发现专用CPoint helper会拒绝 C++ 允许的missing positive raw frame133，已在同合同内改用
  raw direct writer而非回退FWC reset。C++ full trace和真实Play Mode未取得；`D-CPT-002`、
  `D-CPT-003`、pass order及其它链路依然独立。
- **2026-08-22 D-CPT-002 预检结论**：global kill/damage stats 的字段映射和条件已由
  `weapon.cpp:50-75`、`entity_collision.cpp:57-61`、Unity `BattleRuntimeState` 3-slot arrays、
  `LF2Entity.Unk344` 与现有 normal-hit writer闭合；但发现 `D-CPT-004`：
  Unity `RunKind1` 在 C++ cpoint pass本不应执行的阶段先调用 `SyncCaughtByCpoint`，later
  `SyncHeldCpoint` 又能再次调用它。action可清attacking，因此存在双伤害/错时点风险。
  所以 `D-CPT-002` 不能直接补 stats；当前开始 `D-CPT-004` 的独立 source preflight，未改脚本。
- **2026-08-22 当前独立代码包**：`R5-CPT-004 / PLANNED` 的 source preflight、Task Contract和
  Change Record已建立。允许范围仅为 `BattleCpointWriter.RunKind1` 的 early
  `SyncCaughtByCpoint` owner transfer及相应 `BattleRuntimeSelfCheck` joint fixture；不能改
  `D-CPT-002` stats、`D-CPT-003` flow或 PreInteraction pass order。 
- **2026-08-22 当前执行状态更新（覆盖上条 PLANNED）**：`R5-CPT-004 / CODE_WRITTEN` 已删除
  `RunKind1` 的唯一 early `SyncCaughtByCpoint` call，且添加 actual `PreInteractionTickAll`
  no-action/action-state9/action-nonstate9 phase fixture。未写 global stats、未改 pass order或其它
  CPoint chain；当前等待 ledger/diff、Unity compile与full self-check。
- **2026-08-22 R5-CPT-004 完成代码级闭环**：`R5-CPT-004 / RUNTIME_PENDING` 已通过
  ledger/scoped diff、Unity `error CS=0` 与full self-check（09:27:38 `PASS`）。现有三个
  `PreInteractionTickAll` case证明 injury owner在current weapon-sync；移除early position后，
  decrease escape的C++ raw-position knockback修正为`-4`并通过。C++ trace / Play Mode未取得；
  `D-CPT-002` stats可现在建立独立合同，`D-CPT-003`仍独立。
  1. `R5-LINK-001 / RUNTIME_PENDING` 已完成当前代码级闭环。它只处理 invalid positive-link 时
     forward holder 字段保持：C++只清`LinkState`，Unity当前还清`TargetSlotIndex`/`HeldWeaponStableId`。Task
     Contract、Change Record和source preflight已经建立；Legacy/DataOriented/shadow expected与focused fixture的
     最小改动已写入，Unity compile `error CS`=0、2026-08-22 07:32:40 full self-check PASS、focused EditMode
     `BattleEcsPositiveLinkValidationPassEditorTests` 8/8 PASS。C++ trace与真实Play Mode仍待，不能写为完整对齐。
  2. `R5-LINK-002 / RUNTIME_PENDING` 已完成当前代码级闭环。它只处理invalid negative-held
     relation时child `HolderStableId`保持：C++两个held pass都只清child `LinkState`，Unity shared invalid branch
     还清`HolderStableId`。two-pass source preflight、Task Contract与Change Record已经建立；single-field writer、
     self-check和focused Editor test已写入，Unity compile `error CS`=0、2026-08-22 07:46:36 full self-check PASS、
     focused EditMode `SimulationQueryAndLinkModuleEditorTests` 2/2 PASS。C++ trace与真实Play Mode仍待，不能写为完整对齐。
   3. `R5-HOLD-001 / RUNTIME_PENDING` 已完成当前代码级闭环。它只处理type2 held throw的
      `FrameDelay`保持：C++两轮pass都先复制holder delay、branch本身不覆盖；Unity generic与real weapon writer
      都在复制后写成1。dual-writer source preflight、Task Contract与Change Record已经建立；two-writer removal与
      generic/real holder-delay fixture已写入，Unity compile `error CS`=0、2026-08-22 08:01:15 full self-check PASS。
      C++ trace与真实Play Mode仍待，不能写为完整对齐。
   4. `R5-HOLD-002 / RUNTIME_PENDING` 已完成当前代码级闭环。C++两轮held throw只在type1/4/6
      写`spawner_slot`，type2没有该写；Unity real weapon shared throw helper却对type2也写
      `SpawnerEntityIndex`。source preflight、Task Contract与Change Record已经建立；type1/4/6 stamp、type2 no-write
      与existing fixture已写入，Unity compile `error CS`=0、2026-08-22 08:25:40 full self-check PASS。C++ trace与真实
      Play Mode仍待，不能写为完整对齐。同一helper的`PickerStableId=holder slot`已另记`D-HOLD-003`，不合并入本包。
   5. `R5-HOLD-003 / RUNTIME_PENDING` 已完成当前代码级闭环。C++ reset、normal pickup和两轮held throw
      都不写`picker_idx`；release-listed frame advance target selection是该字段的合法后续writer。Unity shared throw
      helper却为type1/2/4/6写`PickerStableId=holder slot`。source preflight、Task Contract与Change Record已建立；
      当前已只移除该一处writer并扩展existing held fixture，Unity compile `error CS`=0、2026-08-22 08:39:22 full self-check PASS。
      C++ trace与真实Play Mode仍待，不能写为完整对齐。
   6. `R4-HIT-004 / RUNTIME_PENDING` 不是当前代码写入包，而是仍需在未来关闭 C++ trace / Play Mode 的已写入
      证据包。它只覆盖 normal current-DAT type1/type2/type4/type6 weapon victim 的`HitConfirm2`与
      `RelationTeam`首次写入时点：从 common writer early write 延后至已有 weapon tail。C++ `collision.cpp:559-632`
      source contract、Unity middle-helper read audit、Task Contract和Change Record均已建立；最小 writer 改动与
      four-branch real-hit fixture已通过；UnityMCP refresh后的`error CS`=0，2026-08-22 07:10:20 full self-check=`PASS`。
      首次type2-ground fixture失败（confirm=0/frame=0/flight=100）已定位为oid998命中`data.txt` type5 catalog
      definition；改为02C已用的无catalog-override test OID后通过，未扩大production scope。
   7. `R1-WP02 / BLOCKED` 仅阻塞 C++ full trace 获取；它不阻塞已获 D-009 授权、且已有独立合同的 R4/R5 最小包。
     只有需要扩大到 negative link、CPoint/WeaponSync、held/release、slot/generation、pass ordering 或 C++ authority
     的情况，才停止并建立新的合同。
- **R4-HIT-005 只读结论**：已确认C++按current `char_data->obj_type`分发，Unity shared Character-DAT与
  SpecialAttack的两个target dispatcher仍有CLR shell优先分支；“weapon shell + current type3”已存在于test-only
  adaptation，但正式asset attack-candidate可达性为`UNKNOWN`。因正确修复需通用current-DAT target adapter并跨多个
  dispatcher，当前记为`INFERRED / no gameplay change`，不阻断后续已登记R5最小包。
  CLR weapon shell被shared Character-DAT resolver以non-weapon current DAT分发至`LF2Weapon.Hit`的路径保持
  `UNKNOWN`、独立待处理，严禁合并进本包。计划内常规子包按D-009连续推进；仅真实范围/authority/compile/self-check
  failure才停止。
- **当前执行步骤**：`R4-COL-01 / R4-COL-001`、`R4-COL-02 / R4-COL-002`、`R4-COL-03 / R4-COL-003`、`R4-COL-04A / R4-COL-004A`、`R4-COL-04B / R4-COL-004B`和`R4-COL-05A / R4-COL-005A`均达到 `RUNTIME_PENDING`。05A已把 common writer的kind1/3共同Character gate收窄为kind3，并以 frozen candidate正/负矩阵验证；最终 Unity compile `error CS`=0、2026-08-22 05:05:17 +08:00 full self-check PASS，首次测试 CS0165已留档。`D-COL-005B` 的只读调查已完成：C++ case1=generic grab、pickup=kind2/7、正式input只处理type0，但VDC编码DAT使non-character kind1 asset/key-producer可达性为 `UNKNOWN / no gameplay change`；不阻断主线。`D-HIT-001 / R4-HIT-001` 已完成最小脚本闭环：type3 four-field vital/stat writer位于tail前，lethal focused fixture通过；Unity compile `error CS`=0，full self-check PASS（2026-08-22 05:26:41 +08:00），状态`RUNTIME_PENDING`。`D-HIT-002` 的kind10/11、kind16、weapon-victim和weapon-attacker raw-frame writer已拆为`R4-HIT-02A`～`02D`；`R4-HIT-002A` 已完成two-callsite最小替换与exact/shared focused matrix，Unity compile `error CS`=0、full self-check PASS（2026-08-22 05:43:54 +08:00），状态`RUNTIME_PENDING`。按D-009当前自动进入 `R4-HIT-02B` 的独立合同准备。 
- **当前验证步骤**：`R3-INP-001` 已完成 scripts compile、filtered C# error query 和 request self-check；下一步只能以独立 Record / fixture 关闭 R3 joint input、Play Mode 或可用 C++ trace，不能把现有 PASS 扩大为完整对齐。R2 的 joint fixture 缺口仍记录在 `R2-ACCEPTANCE-COVERAGE-AUDIT`，但不阻断已批准的后续最小包。
- **2026-08-22 连续推进修正（覆盖上方“当前执行步骤”中的过期下一步表述）**：`R4-HIT-002B / PLANNED` 的kind16 source preflight、Task Contract与Change Record已经建立；当前没有脚本写入。下一动作是严格按该合同完成唯一`ApplyKind16` callsite及existing exact/shared fixture的最小改动，而不是停在02A完成处。
- **后续阶段状态**：R2-PASS-01/02 的 joint fixture / Play Mode / trace 验收仍待后续依赖；R3 的 `R3-INP-01`、`R3-INP-02`、`R3-HOLD-INP-01`、`R3-AI-LIFE-01`、`R3-INP-03A`、`R3-INP-04`、`R3-AI-TGT-01`、`R3-FRAME-01A`、`R3-LAND-01`、`R3-SYNC-RESP-01`和`R3-FRAME-02A`均为 `RUNTIME_PENDING`；D-MOV-005为 current exact route `INFERRED` not reachable，R3-PHY-01保持用户Play Mode/asset `UNKNOWN`。R4 已开始，`D-COL-001 / R4-COL-001`、`D-COL-002 / R4-COL-002`、`D-COL-003 / R4-COL-003`、`D-COL-004A / R4-COL-004A`、`D-COL-004B / R4-COL-004B`和`R4-HIT-002A`均为 `RUNTIME_PENDING`；04B的active landing direct-hit已移除，dormant held immediate query仍为 `INFERRED`；D-COL-005B为 `UNKNOWN / no gameplay change`。`D-HIT-002`已有静态writer-family拆分，接下来是`R4-HIT-02B`独立合同；02B～02D、D-HIT-003、R5～R8均未开始。
- **禁止直接开始**：不得启动 C++ instrumentation / 构建 / 配置修改、Unity trace、comparator，或计划外的技能/gameplay / 性能 / Play Mode 扩张。计划内 R2～R8 子包可按 `D-009` 连续推进，但每一包仍须先建立独立 Task Contract 与 Change Record。

## 脚本改动留痕机制

- **机制状态**：已启用；根 `AGENTS.md` 已加入 13.1 规则，`CHANGE-LEDGER.md`、Record 模板和只读 validator 已通过初始自检。
- **最近治理 Change ID**：`OPS-TRACE-001`，状态 `VERIFIED`；范围仅限留痕机制文档与 `Tools/Validate-ChangeLedger.ps1`，未包含 Unity/C++ gameplay。
- **当前活跃 Change ID**：`R4-HIT-002A / RUNTIME_PENDING`。它只覆盖 exact/shared character kind10/11 的 `frame=182` raw-write side effect：两处 resolver已由`ImmediateFrame`换为现有raw writer，self-check已验证PN、attacking、wait、frame-data mirror和既有stats；Unity compile `error CS`=0与05:43:54 full self-check PASS已取得。C++ trace、真实Play Mode和joint frame/presentation仍未关闭。接下来要建立`R4-HIT-02B`的独立Record，不能扩大02A范围。`R4-HIT-001 / RUNTIME_PENDING`已在`BattleDamageWriter.ApplySpecialAttackDamage`的kind0 type3 target route补齐HP、HPBound、ComboCountVic、DamageStats，且位于既有type3 tail之前；未复用type0-only kill/holder-combo score，也未改candidate、weapon、CPoint、held/link、newborn/opoint、scheduler、input、AI或render。`R4-COL-005A / RUNTIME_PENDING`、`R4-COL-004B / RUNTIME_PENDING`、`R4-COL-004A / RUNTIME_PENDING`、`R4-COL-003`、`R4-COL-002`、`R4-COL-001` 与 `R3-FRAME-002A-001`均为已留痕的既有记录。`R3-SYNC-RESP-001 / RUNTIME_PENDING`、`R3-LAND-001 / RUNTIME_PENDING`、`R3-FRAME-001A / RUNTIME_PENDING` 与其余仍为 `RUNTIME_PENDING` 的 `R3-AI-TGT-001`、`R3-INP-004-001`、`R3-INP-003A-001`、`R3-AI-LIFE-001`、`R3-HOLD-INP-001`、`R3-INP-002`、`R3-INP-001`、`R2-SCHED-002`、`R2-SCHED-001`均必须继续留痕。physical binding / C++ trace / joint Play Mode均未关闭，`D-INP-006`、`g_init_stats` / F7与其余R3+均不在既有 Record范围内。
- **2026-08-22 当前活跃 Record 修正（覆盖上方旧 active 表述）**：`R4-HIT-002B / CODE_WRITTEN` 是唯一当前 active Change ID。它已将 `BattleDamageWriter.ApplyKind16` 的implicit `ImmediateFrame(MpDrain)`收窄为raw writer，并扩展existing exact/shared kind16 fixture以验证PN/wait保留与显式`AttackingCounter=0`；当前等待实际Unity compile/self-check。所有其他kind、writer、projection、战斗模块和C++ authority均不在范围内。
- **2026-08-22 02B 最终状态（覆盖上条 active 表述）**：`R4-HIT-002B / RUNTIME_PENDING` 已取得Unity compile `error CS`=0与05:58:02 full self-check `PASS`；actual/shared fixture验证frame/Data mirror=200、PN=71、wait=17、explicit attacking=0和既有vital/stat/vrest/link/held结果。C++ trace与真实Play Mode仍未关闭。按D-009，下一个动作是建立`R4-HIT-02C`的独立source contract/Change Record；不要把02B结论扩展到weapon writer。
- **2026-08-22 02C 当前状态（覆盖上条下一步表述）**：`R4-HIT-002C / PLANNED` 的C++ weapon-victim raw-frame source preflight、Task Contract与Change Record已经建立，尚未改脚本。当前只允许在`ApplyKind0WeaponVictimTail`与focused fixture中处理type1/type4/type6/type2的PN/attacking/wait/RNG合同。
- **2026-08-22 02C 执行中修正（覆盖上条状态）**：`R4-HIT-002C / IN_PROGRESS` 已静态确认同一canonical `ApplyWeaponDamage` 对damageable weapon固定执行raw knockdown（180/186）后再进入weapon-tail raw final-frame；本包只会将该knockdown一处与tail四处的implicit helper替换为raw writer。focused fixture设计覆盖type1、type4、type6、type2-ground、type2-air，锁定PN、attacking、wait和总RNG call count；尚未改脚本或运行Unity验证。
- **2026-08-22 02C 代码已写（覆盖上条状态）**：`R4-HIT-002C / CODE_WRITTEN` 已将canonical `ApplyWeaponDamage`的knockdown一处及`ApplyKind0WeaponVictimTail`的tail四处替换为`DirectWriteRawFramePreserveWaitCounter`，保留C++所需的raw 180/186→raw final-frame顺序。新增五分支真实`LF2Weapon.Hit`夹具，覆盖type1、type4、type6、type2-ground、type2-air并锁定frame/Data、PN、attacking、wait、HitConfirm2、relation、自身vrest与RNG总数；尚未运行Unity compile/self-check，不能写为已对齐。
- **2026-08-22 02C 当前状态（覆盖上条状态）**：`R4-HIT-002C / RUNTIME_PENDING` 已通过UnityMCP script refresh后的compile `error CS`=0与full `BattleRuntimeSelfCheck`（结果文件2026-08-22 06:20:15 +08:00为`PASS`）。五分支real-hit fixture通过；Console仅保留既有rest-binding negative control，不存在C# compiler error或02C fixture失败。C++ trace仍BLOCKED、Play Mode未做，故不得扩大为完整weapon/R4或C++ runtime已对齐。
- **2026-08-22 02D 当前状态**：`R4-HIT-002D / PLANNED` 已只读闭合normal weapon-attacker source contract：state3000必须在generic victim knockdown前处理current-DAT oid209 skipReset，state1002在later位置处理；当前Unity helper顺序相反且漏skip，且两处`ImmediateFrame`均有raw-frame外副作用。独立Task Contract/Change Record/预检已建立，尚未改脚本、未运行Unity验证。
- **2026-08-22 02D 执行中修正（覆盖上条状态）**：`R4-HIT-002D / IN_PROGRESS` 的五类真实`LF2Weapon.Hit`夹具设计已闭合：state1002、state3000 normal、state3000→frame10 state1002 order witness、oid209 Karasu skip、oid209/frame40 skip。下一步仅在Record列明的两份脚本内实现局部writer拆分，仍未改脚本。
- **2026-08-22 02D 代码已写（覆盖上条状态）**：`R4-HIT-002D / CODE_WRITTEN` 已在`ApplyWeaponDamage`内把state3000移动到generic victim knockdown前，并从原attacker-response中拆出state1002的later raw writer。state3000实现只在non-character weapon victim下使用C++ oid209 skipReset，raw10后保留显式attacking/Vx/Vz；state1002 raw random16后保留Vx/Vy/type4 knockback且不清attacking。五类真实`LF2Weapon.Hit`fixture已写，尚未运行Unity compile/self-check，不能写为已对齐。
- **2026-08-22 02D 当前状态（覆盖上条状态）**：`R4-HIT-002D / RUNTIME_PENDING` 已通过UnityMCP script refresh后的compile `error CS`=0与full `BattleRuntimeSelfCheck`（结果文件2026-08-22 06:36:40 +08:00为`PASS`）。五类real-hit fixture通过；Console仅保留两个既有rest-binding negative control，不存在C# compiler error或02D fixture失败。C++ trace仍BLOCKED、Play Mode未做，故不得扩大为完整weapon/R4或C++ runtime已对齐。
- **2026-08-22 R4-HIT-003 当前状态**：`R4-HIT-003 / PLANNED` 已只读闭合normal weapon vital/stat source contract：type1/2/4 full hurt使用FallDamageDiv-adjusted vital/stat后才写raw durability，type6 reaction只写raw durability。type0-only kill/holder score必须排除；early HitConfirm2/RelationTeam另记`D-HIT-004`。独立Task Contract/Change Record/预检已建立，尚未改脚本、未运行Unity验证。
- **2026-08-22 R4-HIT-003 执行中修正（覆盖上条状态）**：`R4-HIT-003 / IN_PROGRESS` 的real-hit fixture设计已闭合：type1/2/4 scaled nonlethal、type2 lethal with holder、type4 bdefend100、type6 reaction control。下一步只改Record列明的weapon writer/local helper与self-check，不碰D-HIT-004或其他模块。
- **2026-08-22 R4-HIT-003 代码已写（覆盖上条状态）**：`R4-HIT-003 / CODE_WRITTEN` 已在`ApplyWeaponDamage`中按C++相对顺序写damage-effect → type1/2/4 scaled vital/stat → raw durability，并新增专用helper排除type0 kill/holder score；type6仍不进vital helper。真实nonlethal/lethal/bdefend100/type6 fixture已写，尚未运行Unity compile/self-check，不能写为已对齐。
- **2026-08-22 R4-HIT-003 当前状态（覆盖上条状态）**：`R4-HIT-003 / RUNTIME_PENDING` 已通过UnityMCP script refresh后的compile `error CS`=0与full `BattleRuntimeSelfCheck`（结果文件2026-08-22 06:50:08 +08:00为`PASS`）。real-hit vital/durability fixture通过；全量Console仅保留两个既有rest-binding negative control。一次filter MCP socket重连错误为tool transport，不是Unity错误。C++ trace仍BLOCKED、Play Mode未做，故不得扩大为完整weapon/R4或C++ runtime已对齐。
- **约束**：从本账本激活后，任何自编写脚本改动都必须先建立 Change Record，再修改代码；修改后必须同步 Ledger、STATE、handoff 和真实验证证据。
- **Git 边界**：未安装或启用 Git hook，未修改 `.git/config`、`.git/hooks` 或 GitHub Desktop 工作流。未来是否启用 pre-commit hook 由用户单独决定。

## VERIFIED

- 唯一行为权威为 `J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 C++ live battle runtime；入口为 `src/entity/game_tick.cpp` 的 `game_tick(...)`。该目录、入口和 `Makefile` 均已在本机读取；Makefile 列入 game tick、frame advance、physics、collision collect/collision、hit、weapon、cpoint、input 与 renderer 模块。
- Unity 实现入口 `SimulationTickDriver`、`NTSDBattleTickSystem`、`SimulationWorld`、`FrameInputSet` 与 `BattleRuntimeSelfCheck` 均在当前工作树中存在。
- 根 `AGENTS.md`、主要对齐 ledger、交接 ledger、中央渲染计划和统一架构计划均已具有或已补齐“C++ release live path 为最终 authority”的治理口径。
- R1-WP01 规划阶段没有修改 `Assets/NTSD/Scripts/` 下的 gameplay、C++ runtime、测试实现、DAT 或资源；
  后续 R2-SCHED-001 的两处 Unity 脚本修改及其证据见本文件第 36 条和对应 Change Record，二者不得混淆。
- 已完成 R1-WP01 的 C++ checkpoint 合同、Unity source-pass 静态 crosswalk、三方 trace schema、固定 fixture/input journal 合同、first-difference 输出格式和后续 R1 工作包拆分，详见 `docs/ai/TASKS/R1-WP01-trace-contract-planning.md`。
- 已静态确认 C++ `game_tick(...)` 的主要边界，以及 Unity `NTSDBattleTickSystem` / `SimulationWorld.Passes.partial.cs` 中的当前 pass 与 CPoint/WeaponSync 调度位置；这是源代码定位事实，不是运行时对齐证据。
- 已静态确认 `Tools/NTSDParity` 的 README、AuthorityTraceCommand、TraceCompareCommand、Authority400 manifest，以及 Unity `BattleParitySnapshot` / `BattleParityTraceEditor` 以 C# / Unity 历史 parity 为前提。它们只能作为格式、夹具、回归或诊断材料。
- 用户已明确 D-006：C++ Release runtime 在 R1 中不可修改；所有 C++ trace 只能通过既有外部只读观察通道获取，采集结果和比较资料必须写在非 authority 目录。
- 已静态确认 Unity 交付边界的实现事实：`BattleRuntimeProfilePolicy` 定义 `Authority400=400`、`MobileExtended=1050 slot / 1000 active`、`DesktopExtended` 的 page-normalized 初始容量（默认 512）和 `int.MaxValue` active 合同；`SimulationWorld.Registry.partial.cs` 只允许 `DesktopExtended` 动态扩容。
- 已静态确认中央表现边界：`CentralOnly` 会抑制 Legacy materializer、以中央已发布 frame/command 为显示来源，并跳过不会贡献中央 command 的逐实体 renderer shell；Legacy `SpriteRenderer` 容量 guard 明确只是临时兼容限制。上述是 Unity 实现边界事实，不是 C++ 行为对齐证书。
- 已修订重新对齐总计划：R1 的必经主线现在是“C++ source behavior contract → Unity source-pass crosswalk → 差异清单 → 子流程验收矩阵”；R1-WP02 的只读 full trace 保持 BLOCKED，但不再被表述为源码盘点的启动门槛。
- R1-SOURCE-001 已完成静态 main-tick contract 与 Unity pass crosswalk：C++ T00–T18、Unity 30 段调度、D-SCHED-001～012 已写入 research 文档。D-SCHED-001～003 是已确认的静态顺序差异；其运行时行为影响仍待后续 R1 模块和联合验收，不得写成已修复或已验证。
- R2-SCHED-001 已只修改 `NTSDBattleTickSystem` 和 `BattleRuntimeSelfCheck`：Unity 调度静态顺序已核验为 first clamp → held#1 → snapshot/pair/candidate → character/random/object consume → candidate cleanup → CPoint/WeaponSync → positive link → second clamp → held#2。通过现有 Unity Editor 的 UnityMCP `refresh_unity(force/scripts/compile)` 完成 domain reload，更新后的 `Assembly-CSharp.dll` 时间戳为 2026-08-21 22:10:02；UnityMCP Console 返回 0 error，之后 `BattleRuntimeSelfCheck` request result 于 22:12:49 返回 PASS。以上是编译和 focused self-check 事实，不是 C++ runtime / joint fixture / Play Mode 对齐证书。
- R1-SOURCE-002 已完成 C++ post-cooldown callback、human/AI input、combo/direct action、F1/F2 gate 与 Unity packet/input-pass crosswalk。`D-SCHED-005` 已由 `R3-INP-001`、`D-SCHED-010` 已由 `R3-INP-002`、`D-INP-001` 已由 `R3-HOLD-INP-001`、`D-INP-002` 已由 `R3-AI-LIFE-001` 写入各自最小 Unity adapter；四者均通过对应 local static、UnityMCP compile（filtered `error CS`=0）与 request self-check。它们仍没有 R3 joint fixture、C++ runtime trace 或 Play Mode 验收，严禁写成已对齐。`D-INP-003`～`006` 仍未处理。
- R1-SOURCE-003 已完成 C++ F00–F09 的 frame/physics/movement/lifecycle crosswalk：state400/401/500/501、frame advance、character/non-character physics、state12/13/18 与武器 landing、state9998、respawn、Z clamp、late frame tick 都有 source mapping。D-MOV-001～005 已登记；均是静态差异或可达性待验，未做 runtime 验收。详见 `R1-SOURCE-003-unity-crosswalk-and-diff.md` 与对应 handoff。
- R1-SOURCE-004 已完成 C++ candidate collect、collision/hit consume、grab/weapon interaction 的静态 source contract 与 Unity crosswalk。D-COL-001～005、D-HIT-001～003 已登记；CPoint、held/link、opoint/lifecycle producer/consumer 的未闭合部分明确移交 R1-SOURCE-005。全部仍为静态结论，未做 runtime 验收。
- R1-SOURCE-005 已完成两轮 negative held、CPoint / weapon sync、positive/negative link、normal late opoint、slot/newborn / free-reset 生命周期的静态 source contract 与 Unity crosswalk。D-SCHED-004、D-LINK-001～002、D-HOLD-001～002、D-CPT-001～002、D-OP-001 已登记；TrackerFlag/TrackerParent 的 C++ auxiliary-field mapping 与 structural lifecycle joint fixture 仍为 UNKNOWN。全部仍未做 runtime 验收。
- R1-SOURCE-006 已完成 C++ release render callback、renderer active/Z painter order、shadow/body/spark side effect、camera/perspective display contract，与 Unity RenderDispatch、BattlePresentation、CentralOnly、Texture2DArray/dynamic Mesh/URP command path 的静态 crosswalk。D-RENDER-001～005 与 A-RENDER-001～004 已登记；中央渲染、1.5× visual scale、fixed-world logic camera 与扩展容量均为保护边界，不构成回退 Legacy 的授权。全部仍未做 runtime/visual 验收。
- R1-SOURCE-007 已完成 COV-001～006 的全量静态收口：所有 D-/A-条目均进入唯一总登记册，UNKNOWN 有最小补证路径，producer->consumer 依赖、future repair batches 与分层验收矩阵均已写入。此结论是“静态盘点完成”，不是 runtime/Play Mode/trace 验收完成。
- 已建立 `R1-SOURCE-ALL-DIFF-REGISTER.md` 作为跨 Work Package 的全量差异总索引；当前收录 D-SCHED、D-INP、D-MOV、D-COL、D-HIT、D-LINK、D-HOLD、D-CPT、D-OP、D-RENDER 和 A-RENDER 条目。它已静态收口；其 UNKNOWN 和待测试项不允许被当前条目数掩盖或伪造为“已对齐”。
- 已建立 `R1-SOURCE-INVENTORY-COVERAGE-MATRIX.md`，并为 R1-SOURCE-005（CPoint / held /
  link / opoint / lifecycle）、R1-SOURCE-006（render handoff）和 R1-SOURCE-007（汇总 /
  依赖图 / 验收矩阵）建立独立 Task Contract。001～007 均已完成静态 source 审计；任何
  gameplay 仍未对齐或验收。
- 已静态确认 `J:\QQFile\NTSD2.4\ntsd_release\ntsd_new.exe` 存在，长度为 957,072 bytes，SHA-256 为 `9F2C56875F6ADC786C159D3483ABD596191D22405F46812D1A3CD286B5E92C5D`，最后写入时间为 2026-06-12 14:38:01。
- 已静态确认现有 source/binary 可见线索：release binary 包含 `NTSD_DEBUG_TICK`、`NTSD_RNG_SEED`、`NTSD_BATTLE_P1_OID`、`NTSD_BATTLE_P2_OID`、`NTSD_BATTLE_STAGE` 和 `diag_auto_result.txt` 字符串；`main.cpp` 的 release 分支静态调用 `bootstrap_direct_battle`，而 `game_tick.cpp` 的 `NTSD_DEBUG_TICK` 路径向 stderr 输出有限 phase/position 信息。
- 已静态确认 `main(int /*argc*/, char* /*argv*/[])` 不消费命令行参数；运行循环使用 SDL keyboard state 与 Windows `GetAsyncKeyState`，不是现成的逐 tick input journal/replay 接口。
- 已静态确认已检查的 source 诊断路径包含相对文件名的 append 写入，例如 `diag_auto_result.txt`；C++ authority 根目录当前已有该诊断文件。未启动 executable，因此本 WP 没有向 C++ authority 目录写入任何内容。

## INFERRED

- 现有 C#/Unity self-check、Authority400 diagnostic trace、fast-path proof 与 1000 AI/0 GC 数据，仍适合作为后续回归、性能或诊断输入；在取得同场景 C++ release trace 前，它们不构成 C++ 行为对齐证书。
- 现有的 C# 基线资料可帮助 R1 对齐命名、夹具和 Unity 对应 pass，但不能缩短 C++ live-path 调用链核验。
- Unity 当前在 `PreInteractionTickAll` 中于 candidate collect 前运行 CPoint/WeaponSync，而 C++ `game_tick(...)` 将其静态放在 object collision 之后；这只是静态时序风险，尚未由同 fixture 的 C++ runtime trace 证明为行为 mismatch。
- Unity fallback / optimized 的现有诊断开关可能可用于 R1 producer，但其完整性、独立性和与 worker 路径的关系尚未验证。
- release binary 中存在的 debug/environment 字符串提示可能有部分外部观察能力，但 source/executable 完整 build identity、运行时开关副作用、输出覆盖范围和外部工作目录兼容性均未运行验证；不能把字符串存在当成可用 trace 的 VERIFIED 结论。

## UNKNOWN / 未完成验证

- 尚未建立 C++/Unity 可比较的同 schema tick trace；尚未取得 first-difference witness。
- 尚未逐条用 C++ release live 调用链审计历史 C# 结论；R0 仅完成证据治理，R1-WP01 仅完成 trace 合同规划。
- R0/R1-WP01 自身没有执行 Unity 编译、`BattleRuntimeSelfCheck`、Play Mode、C++ release 构建或 C++ runtime trace；后续 R2-SCHED-001/002 已各自取得 Unity scripts compile 与 focused self-check PASS，证据见相应 Change Record，但 Play Mode、C++ release 构建与 C++ runtime trace 仍未执行。
- 项目记录版本为 Unity `2022.3.62f3`，而旧测试示例路径使用 `2022.3.4f1c1`；R1 或任何测试任务开始前需确认实际 Editor 可执行路径。
- C++ Release 现有的只读 trace/日志/进程观察通道、其可观测字段、非 authority 输出重定向、可重复输入方式、RNG call-count 与 fixture bootstrap 尚未闭合。
- C++ / Unity 的 DAT 语义 mapping、stage no-data 合同、initial-state digest、Unity fallback/optimized producer 切换边界、C++ camera/perspective 的最终可比较性均尚未闭合。
- 未找到已经文档化且证明可从未修改 `ntsd_new.exe` 输出 R1 full schema（tick/pass/slot/field/candidate/consume/lifecycle/render handoff）的只读采集通道。
- 未确认在不修改 C++、不向 authority 目录写入的前提下，是否能以非 authority working directory 启动 runtime 并保留资源加载；现有 source 有相对诊断写入，不能擅自尝试。
- 未确认可以将每个逻辑 tick 的 held/pressed/released 输入以可重复、可验证方式送入 release runtime；现有入口是实时 SDL/物理键盘轮询。
- `ntsd_new.exe` 的最后写入时间早于 `src/core/main.cpp`；没有找到把当前 source tree / Makefile 与该 executable 精确绑定的 build manifest，因此 source-to-executable identity 未闭合。

## 当前代码与工作树状态

- 当前 Git 分支：`NTSD_2_4_C++`。
- R0 新增/修改仅限工作流与文档治理文件，详见 `docs/ai/HANDOFFS/HANDOFF-R0-bootstrap-authority-migration.md`。
- R1-WP01 仅新增 `docs/ai/TASKS/R1-WP01-trace-contract-planning.md`，并更新本状态、决策与 R1 handoff；未触碰任何 gameplay 或 C++ 文件。
- R1-WP02 只读准备新增 `docs/ai/HANDOFFS/HANDOFF-R1-WP02-readonly-trace-preparation.md`，并只更新 R1 文档/状态/决策；未启动或修改 C++ Release runtime。
- 本次计划修订只更新重新对齐总计划、本状态和决策记录；未触碰任何 Unity/C++ gameplay、测试实现、DAT、场景或资源。
- R1-WP01 开始时工作树已包含与本任务无关的场景、项目设置、资源 meta、文档修改及未跟踪 `.claude/`、`docs/` 内容；本 Work Package 未回退、移动或清理它们。

## 阻塞与下一步

- **2026-08-23 R8-WP01C规划（最新状态）**：`R8-WP01C-production-combat-object-certification.md`
  已建立，状态为 `PLANNED / APPROVAL PENDING / CERTIFICATION-ONLY`。宽泛的对象交互/生命周期
  Play Mode范围已拆为01 opoint/newborn/basic lifecycle、02 pickup/held/throw/landing、03 grab/CPoint/link、
  04 collision/hit/damage、05 death/respawn、06 random weapon/late special/effect、07 synthesis。
  本次仅修改Task/STATE/handoff文档，未修改脚本、场景、资源或C++ authority，也未运行Unity。
  首个可执行包为`R8-WP01C-01`，必须等待用户明确批准；若执行中需要probe/test脚本，必须先建立
  独立Change Record。R1-WP02 full trace继续BLOCKED，R8-WP01D/E/F/G均未开始。
- **2026-08-23 R8-WP01C-01恢复（覆盖上条审批等待）**：用户已明确回复“批准执行 R8-WP01C-01，
  恢复目标”。只读preflight确认既有W05仅为EditMode结构证据，不能提供live `NTSD_Battle` S4；因此
  `R8-OPLIFE-001 / IN_PROGRESS` 已建立，允许新增唯一Editor-only显式Play探针，目标为type0/1/3/5
  production opoint、出生frame/Prev2、slot/generation、high/low scan cursor、release/reuse及cleanup。
  production gameplay/factory/pool/pass/DAT/scene均不在范围。当前未发现运行中的Unity Editor；这只是
  compile/Play运行前置，尚不是BLOCKED或gameplay失败。
- **2026-08-23 R8-OPLIFE-001代码已写（覆盖上条状态）**：Record现为`CODE_WRITTEN`。已新增
  `BattleOpointLifecyclePlayModeProbeEditor.cs`及meta；它只在显式菜单触发，暂停live driver并在worker
  idle边界使用正式catalog/factory/structural writer/slot/pool，记录四类birth、release/generation reuse及
  high/low scan cursor，finally清理probe-owned对象并恢复pause。普通dotnet build为0 error但当前旧Unity
  csproj尚未收录新文件，因此不能作为probe编译证据；fresh Unity compile与Play尚未运行。
- **2026-08-23 R8-OPLIFE-001编译通过（覆盖上条状态）**：UnityMCP socket 6401执行force-all
  refresh后导入新脚本；`Assembly-CSharp-Editor.dll`于09:01:25更新且晚于源码，Editor.log为Tundra
  success/domain reload，直接Console error查询为0。Record提升到`COMPILE_PASS`；active scene确认为
  `NTSD_Battle`，Play probe/self-check/final validator仍待。
- **2026-08-23 R8-OPLIFE-001运行证据（覆盖上条状态）**：Record现为`FOCUSED_TEST_PASS`。
  live production Play result于09:05:09 PASS：worker active，OID33/120/203/999为正确type/CLR、birth
  frame/runtime/Prev2=0，同一slot53 generation=1/3/5/7且release拒绝old handle；high 52→53在tick357
  same-pass执行，low 53→52在tick358保持attacking0并于tick359变1；cleanup后object6/claimed4/
  render-pool2/logic-pool4全部恢复。W05 focused job `3b8e08105d0946bca58d88e5ed6ef990`
  8/8 PASS，09:06:51 full self-check PASS，Play后Console 0 error/warning。final validator/diff待运行；
  C++ full trace和extended>399 real Play仍未关闭。
- **2026-08-23 R8-WP01C-01最终状态（覆盖上条）**：`R8-OPLIFE-001 / VERIFIED`，只裁决01的
  Unity S4。final ledger validator PASS（59 records / 59 governed code files），scoped diff check PASS。
  persistent evidence为`R8-WP01C-01-opoint-lifecycle-runtime-evidence-20260823.md`。WP01C整体仍
  `IN_PROGRESS`；下一独立包是02 pickup/held/throw/landing，状态`APPROVAL PENDING`。不得把01扩大为
  full C++ trace、extended>399 Play、整个R8或完整战斗对齐。
- **2026-08-23 R8-WP01C-02启动（覆盖上条的 approval 状态）**：用户已明确批准并恢复目标。
  `R8-HOLDPLAY-001 / PLANNED` 已建立，唯一允许脚本范围为新增 Editor-only
  `BattleHeldWeaponLifecyclePlayModeProbeEditor.cs`，用于 live pickup→held/wpoint→type1/2/4/6
  throw→landing/no-immediate-hit S4 认证；发现 production first-difference 时必须登记 repair WP并停止，
  不得在认证包顺手修 gameplay。
- **2026-08-23 技能图片用户观察**：新增 `D-RENDER-006 / USER-REPORTED / REPRODUCTION_PENDING`。
  现有 R6 自动证据没有认证真实 DAT 的技能 pic→sheet/slice/UV 内容；该项归尚未开始的 `R8-WP01D`，
  不是 WP01C-01/02 已通过后的遗漏修复。未取得具体角色/技能/tick/frame/pic 重现前，根因保持 UNKNOWN，
  不修改 render。
- **2026-08-23 R8-HOLDPLAY-001代码已写（覆盖上条代码状态）**：Record现为`CODE_WRITTEN`。
  新增Editor-only explicit Play probe及meta；它在live worker idle/driver paused边界，用data.txt真实OID
  120/150/121/122的type身份和确定性wpoint/frame夹具调用production pickup、held/throw、landing writer，
  并加入overlap target no-immediate-hit及best-effort cleanup。尚无fresh Unity compile、Play、focused或
  self-check证据，不得报告02通过。
- **2026-08-23 R8-HOLDPLAY-001首次Play失败**：tick623的type1 pickup在探针入口被拒绝；定位为探针误用
  只服务shared Character-DAT shell的静态resolver，而非真实`LF2Character` production resolver。cleanup完整恢复
  object9/claimed7/render-pool2/logic-pool7。探针已最小改用真实角色resolver并补地面态前置；production未改，
  fresh compile/重跑待执行。
- **2026-08-23 R8-HOLDPLAY-001第二次Play失败**：type1/type2整链已通过，type4反弹的attacking失败
  源于探针sentinel写在`ImmediateFrame`之前，并非landing writer差异；已移到frame初始化之后。同时该次
  触发时仍为tick0/empty world/worker inactive，故不构成有效S4。探针现先等待tick>0且world/claimed非0，
  再暂停并采基线；production未改，fresh compile/第三次Play待执行。
- **2026-08-23 R8-WP01C-02最终状态（覆盖上条）**：`R8-HOLDPLAY-001 / VERIFIED`，只裁决02的
  Unity S4。final Play 09:37:31在tick1/worker active下通过OID120/150/121/122四type的pickup、held
  wpoint、throw、landing与overlap no-immediate-hit；cleanup恢复object4/claimed2/render2/logic2。
  source 09:36:23 < Editor DLL 09:36:40，focused job `36440d545fe64659ae3c73ff1febf03c`
  23/23，09:38:54 full self-check PASS，清空预期负向日志后Console 0 error/warning，validator 60/60与
  diff PASS。WP01C整体仍IN_PROGRESS；03为APPROVAL PENDING。C++ S5、手动具体武器流程和
  D-RENDER-006/WP01D仍未关闭。
- **2026-08-23 R8-WP01D启动 / D-RENDER-006 first-difference**：用户明确批准并要求不以具体
  角色、技能或OID特判。C++ release `game_tick.cpp:352-383`确认state8000 writer是`unk_318=140`，
  `renderer.cpp:581-624`先raw pic999隐藏再做pic+offset；Unity `LF2Entity.ApplyStateDataTransform`
  错写`HitStop=140`，`GetRenderPicIndex`还会先对999加offset，既有self-check错误保护HitStop合同。
  `R8-WP01D-01 / R8-SPRITEMAP-001 / CODE_WRITTEN`已建立；通用writer/raw-hidden和陈旧oracle已最小
  写入，没有新增角色/技能/OID分支，validator PASS，fresh Unity compile/self-check待执行。23个可读DAT range/BMP
  静态矩阵grid mappingDiff=0，故row/col尚不是当前已证明根因；all-loaded-DAT catalog/slice/UV/Play仍待。
- **2026-08-23 R8-SPRITEMAP-001首次自动验证**：fresh `Assembly-CSharp.dll` 10:09:51且Console C# error=0；
  10:11:07 full self-check在另一个既有GT-10“authority HitStop=140”陈旧断言FAIL。全文件检查又找到
  GT-11 chain/missing-target两处同源错误oracle，已统一改为`HitStop=0 + RenderPicOffset=140`并保留其余
  结构断言。当前状态仍`CODE_WRITTEN`，必须重新fresh compile/self-check；首次FAIL已留档，不能报通过。
- **2026-08-23 R8-SPRITEMAP-001自动收口（覆盖上条当前状态）**：source 10:12:16 < fresh
  `Assembly-CSharp.dll` 10:12:32，Console C# error=0；10:13:22 full self-check PASS。自检内两条既有
  negative registry fixture error读取后已清空，最终Console error/warning=0。Record现为`RUNTIME_PENDING`：
  通用字段/raw-hidden已取得source/compile/self-check，但all-loaded-DAT catalog/slice/UV、CentralOnly live
  command、Game/Scene像素与C++ full trace仍未关闭，不能把D-RENDER-006写成VERIFIED。
- **2026-08-23 R8-WP01D-02启动**：为避免单角色/技能样本掩盖通用差异，已建立
  `R8-SPRITEMAP-002 / PLANNED`。唯一允许脚本是新增Editor-only Play probe：枚举全部loaded DAT/frame、
  C++ expected range/rect、catalog entry、central binding/slice/page/UV，并动态从实际state8000 writer选择
  target生成live CentralOnly command；不得硬编码候选OID。production与GPU pixel修复不在本包，当前未改脚本。
- **2026-08-23 R8-SPRITEMAP-002代码写入（覆盖上条代码状态）**：Record现为`CODE_WRITTEN`。
  新增probe/meta已实现全loaded-DAT/catalog/binding矩阵、动态actual-state8000 target、live snapshot/entity
  command/logical key及cleanup JSON；候选由数据排序动态选择，没有固定角色/技能/OID。production脚本0改动；
  validator 62 Records / 61 governed files PASS，fresh Unity compile/Play尚未执行。
- **2026-08-23 R8-SPRITEMAP-002编译（覆盖上条当前状态）**：Record现为`COMPILE_PASS`。Unity
  2022.3.62f3 force scripts reload完成且Editor恢复idle，Console C# error=0；Play全DAT/catalog/binding/
  state8000 command与cleanup尚未执行，不能写成focused/runtime通过。
- **2026-08-23 R8-SPRITEMAP-002菜单传输修正（覆盖上条当前状态）**：Record退回`CODE_WRITTEN`。
  MCP 9.6.9将中文菜单路径传输为乱码，首次`ExecuteMenuItem`未进入探针；这是Editor自动化入口问题，
  不是渲染审计失败。probe新增调用同一入口的ASCII菜单别名，无production改动；必须重新fresh compile。
- **2026-08-23 R8-SPRITEMAP-002首次真实编译失败（覆盖上条验证状态）**：确认主Editor为PID36240/
  port6401，6400/6402为Unity worker；full asset refresh后新script进入Editor csproj，并暴露line558
  CS1061：`BattleSpriteEntry`无`MatchesCommand`。probe现改为按现有公开descriptor合同逐字段比较；
  production未改，Record仍`CODE_WRITTEN`，待重编译。
- **2026-08-23 R8-SPRITEMAP-002编译收口（覆盖上条当前状态）**：Record现为`COMPILE_PASS`。
  修正后source 02:29:06早于`Assembly-CSharp-Editor.dll` 02:29:21，主Editor port6401 idle且Console
  error=0；下一步执行真实Play全DAT audit，尚不能写成runtime通过。
- **2026-08-23 D-RENDER-006第二个通用first difference**：`R8-SPRITEMAP-002`首次真实Play已枚举
  100 loaded definitions、4373 catalog entries、232 ranges与6674 authored frames，累计1301 differences；
  首批均为`CPP_SOURCE_DESCRIPTOR_MISMATCH`，呈横向换行后持续错一格。只读C++确认parser原样保存
  `row`，loading把`sr.row`作为`SpriteSheet.cols`，renderer按`localPic % row`/`localPic / row`取图；
  Unity `ResolveEffectiveGrid`却按BMP尺寸猜测并默认把`col`当横向列。已建立
  `R8-WP01D-03 / R8-SPRITEMAP-003 / PLANNED`，只允许通用row-horizontal repair与同源test修正。
  首次probe cleanup 0->4/0->2是tick0 baseline采集过早，另在002修正；没有角色/技能/OID分支。
- **2026-08-23 R8-SPRITEMAP-003代码写入（覆盖上条003状态）**：003现为`CODE_WRITTEN`。
  production resolver已删除物理尺寸交换heuristic，固定将DAT row映射为horizontal columns、col映射为
  vertical rows；self-check加入非对称row3/col2换行边界并修正旧grid oracle。002 probe同时把baseline移到
  battle-ready/worker-idle，并拆分state8000候选缺失阶段。未增加角色/技能/OID/frame/resource特判；
  尚未fresh compile/self-check/Play复跑。
- **2026-08-23 R8-SPRITEMAP-003首次self-check失败**：fresh compile 0 error后full self-check在
  `CheckSpriteFileRangeParsingContracts`失败，flash fixture仍按`col`建synthetic texture width、按`row`建
  height，因此合法catalog key `(214,0)`成为hole。该fixture现改为C++ row-horizontal尺寸；parser/range/
  overlap职责及production未扩大。两条rest registry error仍为既有负向控制；待重新编译/self-check。
- **2026-08-23 R8-SPRITEMAP-003自动验证收口（覆盖上条当前状态）**：flash fixture修正后fresh
  compile 0 error，10:41:17 full self-check PASS；两条rest registry error是既有负向夹具。003现为
  `FOCUSED_TEST_PASS`，002 baseline/state8000分型也fresh compile通过。下一步必须clean Play重载全部
  production DAT/BMP并复跑4373-entry矩阵，不能仅凭self-check写成runtime通过。
- **2026-08-23 R8-WP01D第二次全DAT Play**：修复后catalog 4933，首次1301条
  `CPP_SOURCE_DESCRIPTOR_MISMATCH`已清零，cleanup从4/2恢复到4/2。剩余229条全为range超出实际网格的
  `VISIBLE_FRAME_CATALOG_ENTRY_MISSING`；样本均是C++ source rect完全落在BMP外，Unity hole与C++无可见
  像素等价。002 probe已改为只有rect仍与source sheet相交才报missing，并统计fully-outside数量；
  state8000为0 authored source，明确SKIPPED而非PASS。002退回`CODE_WRITTEN`待第三次Play；003暂不升级。
- **2026-08-23 R8-SPRITEMAP-002 fully-outside探针首次编译失败**：line424/429因局部变量名与
  同一for scope后续变量冲突产生两条CS0136；已只重命名诊断局部变量，production与判断公式未改。
  Play实际未进入，待重新fresh compile。
- **2026-08-23 R8-WP01D第三次全DAT Play**：probe编译修正后实际运行，60个fully-outside已正确
  排除，剩余169个missing引用仍与纹理边缘相交。只读原BMP检查表明实际C++ rect交集全为黑色
  colorkey；summon的绿色像素只在格间separator列、没有进入任何rect。002现新增全路径通用BMP交集
  像素检查与短生命周期cache：仅非黑交集继续报差异，禁止文件名/OID特判；待重新compile/Play。
- **2026-08-23 D-RENDER-006第三个通用first difference**：第四次全DAT Play在通用BMP交集像素
  过滤后仅余2个非黑可见missing entry，cleanup PASS。C++只以declared range判localPic合法，`row`只作
  横向cols，不用`row*col`限帧，并由blit裁剪partial source；Unity仍以row*col分配且partial为hole。
  003因4933-entry source descriptor mismatch=0升级`RUNTIME_PENDING`；已建立
  `R8-WP01D-04 / R8-SPRITEMAP-004 / PLANNED`，使用range长度、source intersection与通用adjusted pivot；
  证据中的ID/frame/path不得进入实现分支。
- **2026-08-23 R8-SPRITEMAP-004代码写入（覆盖上条004状态）**：004现为`CODE_WRITTEN`。
  rect builder按declared range分配、与source bounds求intersection，并以完整frame锚点和clip offset计算
  adjusted pivot；prewarm Sprite与catalog显式pivot共用合同。self-check锁定range>row*col、79×4 partial与
  negative pivot；002 probe同步expected clipped rect/pivot。Mesh/Shader/gameplay未改，无角色/技能/OID/
  frame/file特判；尚未compile/self-check/Play。
- **2026-08-23 R8-SPRITEMAP-004首次self-check失败**：fresh compile 0 error；P2 partial组合断言仍把
  weapon6/weapon3的2px下一行/42px右列交集当hole，期望count40/7而新C++合同为50/8。已只修
  同源test为精确clipped rect与后续hole；production未改，首次FAIL保留，待重编译/self-check。
- **2026-08-23 R8-SPRITEMAP-004自动与Play收口（覆盖上条004当前状态）**：fresh compile 0 error，
  10:59:06 full self-check PASS；final all-DAT Play枚举100 definitions、232 ranges、6674 frames、
  5537 catalog entries和23 clipped引用，source/path/rect/pivot/binding differences=0，cleanup 4/2→4/2。
  loaded data无authored state8000 source，live witness明确`SKIPPED_NO_AUTHORED_SOURCE`。002与004均升级
  `RUNTIME_PENDING`；未把catalog PASS冒充GPU/Game/Scene或C++ full trace。
- **2026-08-23 R8-WP01D-05启动 / 旧P8-C生产夹具边界**：focused resolver/atlas/mesh job
  `608b9f8515a646fb97ecd2a5c36c4707` 29/29 PASS。P8-C Play GPU矩阵中synthetic legacy-central pixels、
  Texture2DArray UV、透明排序、4097 chunk与missing-resource均PASS；production case因旧harness强制要求
  opoint实体拥有逐对象`LF2ObjectRenderer`而FAIL，和当前批准的logic-only + Central snapshot架构冲突，
  不能裁决production像素。已建立`R8-SPRITEMAP-005 / IN_PROGRESS`，只新增Editor probe，对全部catalog
  source→central binding实际像素及统一GPU command做通用验证；不得恢复Legacy owner或加入专项分支。
- **2026-08-23 R8-SPRITEMAP-005最终状态（覆盖上条005当前状态）**：`VERIFIED`，仅裁决当前
  logic-only CentralOnly架构下的全catalog GPU/binding像素与dynamic Mesh witness。final Play读取232张
  source textures、30个Texture2DArray slices，5537/5537 entries与84,327,319 pixels全部匹配，
  source/central hash均`8ECA0CBA6D4724D1`且同域重复一致，0 differences。final动态可见partial为
  450×5、pivot(0.5,-28)，Legacy/Central 340/340 pixels、mean/max=0/0，cleanup 4/2→4/2。首次全透明partial与第二次负pivot
  视口中心导致witness为0像素，均只修Editor probe且保留失败事实；production代码0改动，无专项分支。
  final editor DLL 11:34:48且compiler error=0，focused job `ecaf8255752e4515bbcc76787c61aba3`
  35/35，11:37:22
  full self-check PASS。D-RENDER-006整体仍`RUNTIME_PENDING`：真实Game/Scene最终可见性/挂点/层级、
  当前loaded data无authored state8000 witness及C++ full trace尚未关闭。
- **2026-08-23 R8-WP01D-06启动 / Game空实体画面**：fresh Game screenshot
  `Temp/R8-WP01D-06/R8-WP01D-06-game.png`显示HUD与背景正常，但无战斗实体；Scene截图因当前
  180×936窄viewport只作环境证据。由于descriptor和84,327,319 GPU binding pixels均0差异，首差范围
  转到正式`snapshot→command→resolver→segment/chunk→URP submission→camera`。已建立
  `R8-SPRITEMAP-006 / IN_PROGRESS`，只新增Editor live diagnostic枚举全部claimed slots，不改production、
  scene/URP asset或任何角色/技能/OID/frame/file分支。
- **2026-08-23 R8-SPRITEMAP-006最终状态（覆盖上条006当前状态）**：`VERIFIED`，仅裁决Editor
  diagnostic与当前Game submission证据。第一次tick1采样的`NO_SNAPSHOT_ENTITIES`在扩展worker/pending/
  immutable-plan字段并延后采样后被证明为过早采样；final tick257报告覆盖3个claimed slots，形成3个
  snapshot、6个source/resolved commands、1 chunk、1 segment和1 draw，plan simulation/display tick均257、
  非stale、无refusal/worker failure，cleanup恢复。fresh Game截图已实际显示角色、武器和阴影；没有
  production first difference，故未修改worker、central renderer、URP、scene或camera。source 11:47:36 <
  Editor DLL 11:47:58，compiler error 0，11:53:16 full self-check PASS。D-RENDER-006整体仍为
  `RUNTIME_PENDING`：当前180×936 Scene View不能裁决logic-only中央实体可观察性，loaded data无authored
  state8000 live witness，C++ full trace继续BLOCKED。
- **2026-08-23 R8-WP01D-07启动**：只读确认production `CanRenderCamera`明确允许Play Mode Base
  `CameraType.SceneView`，focused materialization test也证明最新world publication完成后SceneView可取得
  current lease；当前空Scene截图更可能是viewport/观察坐标问题，不构成renderer差异。已建立
  `R8-SPRITEMAP-007 / PLANNED`，只允许新增Editor-only probe，用真实SceneView camera对齐world camera、
  cullingMask=0与透明RT隔离中央像素，记录gate/lease/pixels/cleanup。未修改production、scene、URP、
  DAT/BMP或C++，若probe发现首差必须另立repair Record。
- **2026-08-23 R8-SPRITEMAP-007代码已写（覆盖上条007状态）**：Record现为`CODE_WRITTEN`。
  Editor-only probe等待current plan与worker idle，记录真实SceneView gate/current lease，暂存camera状态后将
  投影对齐world camera，在960宽白色隔离RT、cullingMask=0下执行实际SceneView render，输出isolated PNG、
  non-clear pixels与hash并恢复camera/driver/world；白底确保黑色阴影不会被黑底证据漏计。production、scene、URP、DAT/BMP、
  Legacy owner与C++均0改动；尚未compile/Play/self-check。
- **2026-08-23 R8-SPRITEMAP-007最终状态（覆盖上条007当前状态）**：`VERIFIED`，仅裁决Play Mode
  SceneView camera/central pixel S4。source 12:03:53 < Editor DLL 12:04:04，compiler error 0；focused job
  `9dfeda6b0663429a9caf20df64048fb9` 13/13。clean Play真实`SceneCamera/CameraType.SceneView`的production
  gate与current lease均true，tick2/generation3 current plan含4 source/resolved commands、1 segment；960×540
  白底isolated render得到575 non-clear pixels、hash `C292967D753744C2`。objects 4→4、claimed 2→2，
  camera/driver恢复，Play Console 0 error，12:05:47 full self-check PASS。首次黑底522像素证据因黑色阴影
  不可观察而由白底final supersede；两次均无production改动。先前空Scene截图是窄viewport和logic-only
  Transform观察方式不足，不是renderer首差。D-RENDER-006现只剩loaded data无authored state8000 live
  witness与C++ full trace两项证据缺口。
- **2026-08-23 R8-WP01C-03获批并启动**：用户明确回复`批准执行 R8-WP01C-03，恢复目标`。
  只读复核确认`R5-CPT-001～005`与`R5-LINK-001～002`的production修复均已写并有source/compile/
  self-check证据，当前缺口是live production world joint S4，没有发现新的静态差异。已建立
  `R8-GRABPLAY-001 / PLANNED`，只允许新增Editor-only Play probe，覆盖valid grab+held injury/global stats、
  reciprocal mismatch throw、negative escape+dircontrol、positive/negative link residue及first-held→
  PreInteraction→positive-link→second-held逐pass表。production、DAT/scene、render与C++禁止修改；发现首差
  必须另拆repair。
- **2026-08-23 R8-GRABPLAY-001代码已写（覆盖上条03状态）**：Record现为`CODE_WRITTEN`。
  Editor-only probe在paused live production world使用正式grab writer和first-held→PreInteraction→positive-link→
  second-held pass，覆盖lethal held injury/global stats/FWC/position、reciprocal mismatch fallback throw、
  negative-duration escape+dircontrol+FramePostProcess、positive/negative residue，并恢复global stats、实体、池与
  pause。production gameplay/scheduler、DAT/scene、render与C++均0改动；尚未compile/Play/focused/self-check。
- **2026-08-23 R8-GRABPLAY-001首次Play**：fresh compile 0、positive-link 8/8、negative-link 2/2；
  clean Play全部行为断言PASS，valid grab四pass、mismatch throw、escape dircontrol和link residue均符合合同，
  cleanup/global stats恢复。发现仅Editor报告取样时点不严谨：postprocess后Knockback已清0，后续negative-held
  又把positive target link从-5清0。已只在probe中提前保存两组观察值，production 0改动；首次PASS保留，
  final evidence必须重新compile/clean Play。
- **2026-08-23 R8-GRABPLAY-001最终状态（覆盖上条03当前状态）**：`VERIFIED`，仅裁决WP01C-03
  Unity production Play S4。final source 12:22:01 < Editor DLL 12:22:18，C# error0；positive-link job
  `2e1446b473a64aef81ca80fd9b69d30d` 8/8、negative-link job
  `aa8d155711ac4ee5a9fc48862bf2fe42` 2/2。clean Play在worker active下tick16→17：valid kind3 grab、
  first-held无damage、PreInteraction唯一lethal injury/stat/position、后续无重复；mismatch fallback throw、
  negative escape+dircontrol+postprocess和正负link residue全部PASS。objects4→4、claimed2→2、pools2→2、
  global stats恢复、Console error0；12:23:59 full self-check PASS。production0改动。C++ full trace继续BLOCKED，
  WP01C-04～07未由03关闭。
- **2026-08-23 R8-WP01C-04获批并启动**：用户明确回复`批准执行 R8-WP01C-04，恢复目标`。
  只读复核确认C++ Release顺序为snapshot/collect→type0 consume→random-weapon boundary→type>0 consume，
  Unity `R4-COL-001～003/005A`与`R4-HIT-001～004`已有production修复和compile/self-check证据，当前
  缺口是同一live production world的character/weapon/special、candidate order、caught/effect21/HitConfirm2
  gate/abort、vital/stat/durability/vrest联合S4。已建立`R8-HITPLAY-001 / IN_PROGRESS`；只允许新增
  Editor-only通用Play probe和治理/证据文档，production、DAT/scene、render与C++禁止修改。发现首差必须
  单独登记repair并停止，不得在认证探针中顺手修复。
- **2026-08-23 R8-HITPLAY-001代码已写（覆盖上条04当前状态）**：Record为`CODE_WRITTEN`。
  Editor-only probe已编码10个frozen candidate，覆盖三类正向命中、HitConfirm2/caught/effect21、kind10 raw
  frame和character→random-weapon→object pass边界，并备份/恢复RNG、stats、sounds、baseline pair rests、
  hit-plan mode、实体/池/pause。production gameplay/scheduler、DAT/scene/render与C++均0改动；尚未compile/
  focused/Play/self-check，不得报告04通过。
- **2026-08-23 R8-HITPLAY-001首次Play（probe-only失败）**：fresh compile 0、hit focused 178/178、
  W06 11/11、role-aware 9/9已通过。首次Play在进入任何gameplay pass前因探针尝试在非reset boundary切
  `ShadowCompare`而由production正确拒绝；实体/池/stats/RNG/sounds/rest均恢复，失败不构成行为首差。
  已只修probe为观察启动时既有hit-plan mode，不再改变mode；需重新compile/clean Play，当前仍非完成。
- **2026-08-23 R8-HITPLAY-001第二次Play（probe-only失败）**：完整passes已运行，所有此前behavior断言
  通过；最后报告读取`DamageStats/KillStats[3]`时因live数组只有0～2而越界。cleanup全恢复，未形成
  gameplay first-difference。probe改为special与character共用合法槽1，按pass验证+10→累计+20且kill
  只增加一次；需第三次compile/clean Play，production仍0改动。
- **2026-08-23 R8-HITPLAY-001最终状态（覆盖上条04当前状态）**：`VERIFIED`，仅裁决WP01C-04
  Unity production Play S4。final compile0；hit focused178/178、W06 11/11、role-aware9/9。clean Play冻结
  10个candidate：character5→-5、weapon100→80/durability100→90、special100→90；HitConfirm2/effect21
  整attacker abort、caught first-only skip、kind10 raw frame182和character→random no-op→object边界均PASS。
  objects4→4、claimed2→2、pools2→2，RNG/stats/sounds/baseline rests/mode/pause恢复，Console0 error；
  13:19:39 self-check与validator PASS。production0改动。当前hit-plan mode=Disabled、worker inactive，故不声称
  本轮ShadowCompare/worker-active或C++ full trace。05～07未由04关闭。
- **2026-08-23 R8-WP01C连续授权（覆盖05 approval pending）**：用户明确要求“直接推进WP01C剩余的
  三项即可，不需要我批准”。因此05→06→07按固定依赖顺序连续执行，不再逐包等待批准；每包仍须独立
  Task/Change Record、fresh compile、focused、Play/self-check与治理。production first-difference、gameplay
  repair、scene/资源/架构变更和任何C++ authority运行/构建/写入仍是停止条件。当前进入05。
- **2026-08-23 R8-WP01C-05启动**：只读source preflight确认AI-before-cleanup、state14 hit-stop、
  no-count/stored-count/free、integer average/RNG与OID998字段合同已闭合；现有Unity production映射未发现
  新静态首差。已建立`R8-DEATHPLAY-001 / PLANNED`，只允许Editor-only live probe和治理/证据文档；
  production、DAT/scene、render与C++均禁止修改。首差必须独立登记并停止。
- **2026-08-23 R8-DEATHPLAY-001代码已写**：Record为`CODE_WRITTEN`。Editor-only probe覆盖HP=0 AI
  input、state14 arm/decrement、no-count stale integer+RNG、stored-count OID998、free和relation/link writer边界，
  并恢复RNG/sounds/entity/slot/pool/pause。production0改动；尚未compile/Play/focused/self-check。
- **2026-08-23 R8-DEATHPLAY-001最终状态（覆盖上条05状态）**：`VERIFIED`，只裁决WP01C-05 Unity
  production Play S4。fresh all-scope compile0；AI85/85，W05 exact1/1与isolated8/8。clean Play完成HP=0
  AI、state14 0→30→4、no-count/stored/free；stale integer平均(130,30)，两次RNG后expected/actual(147,39)；
  stored OID998/action6 slot50字段正确。objects4→4、claimed2→2、pools2→2，RNG/sounds/pause恢复，Console0；
  13:52:04 self-check与validator PASS，production0改动。组合focused一次W05B静态污染失败已由isolated PASS
  复核且未改production。worker inactive、C++ full trace仍BLOCKED；06按连续授权直接进入。
- **2026-08-23 R8-WP01C-06启动**：source preflight闭合natural random、9995→4000→8000→9996、
  4×217+1×218、lowest-slot/RNG/exhaustion；R7旧HitStun140口径已确认由R8 RenderPicOffset140纠正。
  已建立`R8-LATEPLAY-001 / PLANNED`，只允许Editor-only Play probe和治理证据；production/C++ 0改动。
- **2026-08-23 R8-LATEPLAY-001代码已写**：Record为`CODE_WRITTEN`。probe覆盖live catalog natural random、
  live 9996五子、logic-only full chain与authority400 exhaustion；cleanup恢复live baseline，production0改动。
  尚未compile/focused/Play/self-check。
- **2026-08-23 R8-LATEPLAY-001最终状态（覆盖上条06状态）**：`VERIFIED`，只裁决WP01C-06 Unity
  production Play S4。compile0、focused14/14；worker-active final Play中natural 9 candidates选OID122 slot50、
  position(1314,-500,509)、8 RNG；live 9996生成4×217+1×218、34 RNG；synthetic full chain到OID901/
  state9996/offset140；authority400满350动态槽时natural1/late0 RNG、0 spawn。objects/claimed/pools、
  RNG/sounds/pause恢复，Console0，14:08:15 self-check和validator PASS，production0改动。C++ trace BLOCKED；
  按连续授权进入07 synthesis。
- **2026-08-23 R8-WP01C-07 / WP01C最终状态**：`COMPLETE / 01～06 VERIFIED（Unity S4） / 07
  SYNTHESIS COMPLETE`。六包production producer→consumer矩阵、cleanup、fresh compile/focused/full self-check/
  ledger均有持久证据，认证probe对production gameplay改动0。最终汇总见`R8-WP01C-07-synthesis.md`。
  D-COL-004、D-COL-005B、D-HIT未覆盖分支/D-HIT-005、D-LIFE-001、C++ full trace及WP01D/E/F/G仍独立；
  不得把WP01C完成扩大为R8或完整战斗逻辑已对齐。
- **B-R8-WP01C-06-TEARDOWN-01（非阻断战斗S4）**：WP06 probe结束前active object/slot/pools与RNG/sounds
  均恢复、Console0；退出Play后两个AutoCreated manager触发scene cleanup warning。无probe对照Play→Stop为
  0 error，定位为probe新增inactive renderer的Editor teardown hygiene；未修改production/反射pool内部。
- **2026-08-23 R8-WP01D最终边界**：`COMPLETE AT AVAILABLE EVIDENCE / FULL CLOSURE BLOCKED`。
  01～07已取得state8000/row/range修复、5537 catalog、84,327,319 GPU pixels、Game和SceneView限定范围S4；
  `B-R8-WP01D-08-01`为loaded DAT无authored state8000，`B-R8-WP01D-08-02`为R1-WP02 full trace。
  不改DAT、不绕过C++只读；D-RENDER-006保持MAX AVAILABLE S4。WP01E/F/G可继续，当前进入WP01E。
- **2026-08-23 R8-WP01E启动**：已建立`R8-WP01E-current-build-capacity-performance-certification.md`，
  状态`PLANNED / CERTIFICATION-ONLY / NO SCRIPT CHANGE`。现有harness具备1000 production GameObject、
  MobileExtended/capacity、0 B/Gen collection、central draw/pixel、hash与teardown指标；历史报告不作fresh证据。
  顺序固定为fresh工具/容量基线→Dispersed/Combat短样本validity gate→两组各1800 tick正式60秒门→
  DesktopExtended合同复核。任一首差停止认证并另建修复包，本包内不顺手改脚本。
- **2026-08-23 R8-WP01E E-01/E-02结果**：E-01 fresh compile0、focused job
  `2dda595036944c708bfd11f32204ba1e` 290/290、14:25:44 self-check PASS。E-02首次Combat1000在
  0 sampled tick/0 stress entity/无report时失败：processor于Bootstrap初始加载窗口把driver/world partial
  footprint+missing lazy pool误判为managed runtime invalid，两次Play均在ready前重启并fail-closed。该结果
  是harness lifecycle first failure，不是性能/GC/capacity/gameplay结论。WP01E现`BLOCKED AT E-02`；
  `R8-WP01E-R01 / R8-PERFBOOT-001 / PLANNED / APPROVAL PENDING`已登记，只允许修正restart decision
  事实输入及pure-policy tests，不改pool/Bootstrap/gameplay/C++。
- **2026-08-23 R8-PERFBOOT-001获批**：用户明确回复`批准执行 R8-WP01E-R01 / R8-PERFBOOT-001，
  恢复目标`。Record现`IN_PROGRESS`；只允许修改`ProductionEntityStressWindow.cs`的restart decision caller
  和`ProductionEntityStressEditorTests.cs`的pure-policy matrix，不改pool/Bootstrap/gameplay/C++。
- **2026-08-23 R8-PERFBOOT-001代码已写**：Record为`CODE_WRITTEN`。restart policy现在以Bootstrap
  ready而非driver/world partial footprint裁决“服务应已完整”，并允许clean restart后的新Play同样等待初始
  Bootstrap；ready-invalid、previously-healthy invalid与一次retry fail-closed保持。7分支pure-policy matrix
  已写；尚未compile/focused/Play复跑。
- **2026-08-23 R8-PERFBOOT-001代码级验证**：fresh Editor DLL 14:33:57、Console error0；focused job
  `2bcc822ceddb45f9955a3041a3ade51f` 263/263 PASS；14:35:20 full self-check PASS。Record现
  `FOCUSED_TEST_PASS`，等待完全相同Combat1000 capacity-pressure smoke的Play复跑，尚未VERIFIED。
- **2026-08-23 R8-PERFBOOT-001最终状态**：`VERIFIED`。同一Combat1000请求现不再premature restart，
  1000 active、30 warmup、180 sampled完整执行；logic Avg/P95 21.199/23.797ms、0 B/0 collection、capacity
  critical0、central 1 draw/179 pixel frames、hash非空、teardown全恢复。pool/Bootstrap/scene/gameplay/C++
  0改动。WP01E E-02 Combat短样本通过，但visible frame Avg/P95 38.949/39.025ms、frame GC平均7128.94B
  仍是正式60秒门风险；下一步Dispersed短样本，不得宣称30FPS完成。
- **2026-08-23 R8-WP01E E-02短矩阵**：Combat与Dispersed均1000 production objects、180/180 sampled、
  logic0B/0 collection、capacity critical0、central1 draw/SetPass4、hash与teardown PASS。Combat logic
  Avg/P95=21.199/23.797ms，Dispersed=21.432/24.771ms；但visible frame分别38.949/39.025ms与
  38.309/44.265ms，均未达到正式30FPS门。E-02只关闭validity，现进入E-03两组1800-tick completed-frame
  timing正式报告，不先猜优化模块。
- **2026-08-23 R8-WP01E最终状态**：`VERIFIED / UNITY EDITOR CURRENT BUILD`。两组120+1800正式门：
  Dispersed logic/visible/main P95=18.575/25.525/25.286ms；Combat=19.044/33.058/26.901ms；Render/GPU
  P95均低于4ms。两组logic0B、Gen0/1/2=0、capacity critical0、central1 draw/SetPass约4、hash与teardown
  PASS。Desktop/Mobile/pool/slot/generation focused job`f554c03eb363475b808fe622d350c9a3` 299/299；
  fresh Legacy/Data同180-tick的input/RNG/metadata/world/slots/aRest/vRest/stats/events/overall/workload/roster
  12项hash全部相同；14:51:50 final self-check PASS。Editor frame recorder仍有非战斗Editor-side allocations但
  60秒内0 collection且P99<30KB；Player hard gate归WP01F。下一包WP01F Windows Mono/IL2CPP，需先建合同。
- **2026-08-23 R8-WP01F规划**：`R8-WP01F-windows-mono-il2cpp-player-certification.md`已建立，状态
  `PLANNED / APPROVAL PENDING`。国际版2022.3.62f3的WindowsStandaloneSupport/IL2CPP模块存在，当前
  Standalone默认IL2CPP；现有build tool仅旧U9 Mono。`R8-PLAYERBUILD-001 / PLANNED`只允许在该Editor
  build tool提取共享helper、新增Mono/IL2CPP独立输出并保持ProjectSettings/Burst finally恢复；批准前不改脚本、
  不build/run Player。
- **2026-08-23 R8-PLAYERBUILD-001获批**：用户明确回复`批准执行 R8-WP01F / R8-PLAYERBUILD-001，
  恢复目标`。Record现`IN_PROGRESS`；只允许修改现有Editor build tool的双backend入口/独立Temp输出与
  共享finally恢复，不改runtime/scene/ProjectSettings默认值/C++。
- **2026-08-23 R8-PLAYERBUILD-001代码已写**：Record现`CODE_WRITTEN`。现有Editor build tool已新增R8
  Mono/IL2CPP菜单、共享Windows build helper、backend独立Temp输出和明确BuildReport日志，并保留旧U9
  Mono菜单兼容别名；未改runtime/scene/ProjectSettings默认值/C++。fresh compile、两后端build/run和hash
  比较均仍待执行，不能写为认证完成。
- **2026-08-23 R8-PLAYERBUILD-001编译通过**：Record现`COMPILE_PASS`。UnityMCP force refresh后Console
  0 error，scoped diff-check与Change Ledger validator通过；构建前ProjectSettings/Burst hash已登记。Mono与
  IL2CPP均尚未build/run，WP01F仍未认证。
- **2026-08-23 R8-WP01F Mono中间结果**：Mono BuildReport Succeeded；可见D3D11 Player正式运行exit0，
  1000实体、30+180 tick、MobileExtended/DataOrientedCanonical、Player hard四边界0 B、0 collection、
  capacity0、CentralOnly draw1/pixels179与teardown restored均通过。隐藏窗口负控制因SRP不提交按预期exit3，
  不作为正式结果。IL2CPP未build/run，因此WP01F仍在进行中。
- **2026-08-23 R8-WP01F用户停止决定**：用户明确指示`IL2CPP Player 不会有任何问题，不要做相关处理`。
  当前不再构建、运行、诊断、修复或认证IL2CPP，不把Codex沙箱进程结果作为gameplay差异/blocker，也不据此
  修改Unity脚本或配置。`R8-PLAYERBUILD-001`现`ABANDONED`，已写helper与Temp artifacts保持原样；WP01F
  不标VERIFIED，工作返回C++ Release→Unity C#战斗逻辑主线。
- **2026-08-23 R8-WP01G综合完成**：68个D-ID已按20/20/19/7/2无遗漏分类，集合校验register=68、
  synthesis=68、missing0、extra0；scoped diff-check和Change Ledger validator（73 records/73 governed
  code files）PASS。本包未改Unity脚本、scene或config。当前没有可直接修改的未关闭source-confirmed代码
  差异；下一推荐是`R8-WP01G-R01`只读闭合R2的`D-SCHED-006/008`，R3+运行证据/修复包仍须用户批准。
- **2026-08-23 R8-WP01G-R01完成（更正WP01G初次综合）**：`D-SCHED-006`已闭合为
  `SOURCE-CLOSED / EQUIVALENT WITH APPROVED CAPACITY ADAPTER / RUNTIME_PENDING`；两次current-character-DAT
  Z clamp的时点、slot顺序、double/int写入一致，高槽为批准adapter。`D-SCHED-008`已确认
  `SOURCE-CONFIRMED CONDITIONAL DIFFERENCE / UNFIXED`：normal completed tick无consume-end→tail reader，
  但F1/step-wait render后early return跳过tail时，C++保留candidate carrier到下一tick继续append，Unity
  已EndConsumption且下一collect无条件reset，可能改变count/20-cap/order/RNG/consume。当前分类更正为
  A20/B20/C20/D5/E2/F1=68。已建立`R2-CANDIDATE-TAIL-01`预实施Task；本只读包未改/运行Unity或C++。
- **2026-08-23 R2-CANDIDATE-TAIL-01只读实施预检**：精确修复不能只移动End或只清count；StoreOnly/
  LegacyOracle切换、fallback restart、attacker generation、target-slot current occupant与20条ordered entries要求
  一个query-owned fixed-slab retention store，在prebattle capacity阶段预分配，pause时capture、next collect按
  current producer mode seed后继续append、真实tail后清理。该方案跨scheduler/query/store/tail/test，当前状态
  `PLANNED / APPROVAL PENDING`；尚未创建Change Record、未修改脚本。
- **2026-08-23 R8-WP01G-R01B / D-STEP-001 source closure**：`D-STEP-001`从UNKNOWN更新为
  `SOURCE-CONFIRMED DIFFERENCE / POLICY DECISION REQUIRED / UNFIXED`。C++ release main在BATTLE outer
  frame按A→B→C down-edge写process-global flag1/progress3；flag1时F1 wait仍skip完整input callback，但不再
  render后return，会继续postprocess/late/tail。Unity无flag/progress/deterministic command producer，scheduler
  恒走flag0分支。当前68项分类更正为A20/B20/C20/D4/E2/F2。已建立`R3-STEP-01` policy Task，因属于
  R3+ schema/input/architecture选择而停在用户policy批准边界；本包脚本0改动。
- **D-STEP对D-SCHED-008依赖**：candidate retention必须由明确`willSkipPostFrameTail` predicate触发；
  当前Unity未实现unlock时它等价于stepWait，未来flag1+stepWait必须normal clear，不能retain。该合同已写入
  `R2-CANDIDATE-TAIL-01`，避免R2修复硬编码裸stepWait后被R3再次返工。
- **2026-08-23 debug-step范围决定**：用户明确不需要F1/F2战斗调试步进。按`D-015`，`D-STEP-001`与
  仅由debug tail-skip触发的`D-SCHED-008`改列为批准省略的debug-only行为；`R2-CANDIDATE-TAIL-01`与
  `R3-STEP-01`不执行，不再阻塞normal combat主线。
- **2026-08-23 当前执行包**：`R8-WP01G-R02 / IN_PROGRESS / SOURCE-FIRST`。本包只处理
  `D-MOV-005`、`D-COL-005B`、`D-HIT-005`、`D-LIFE-001`，顺序固定；先闭合C++→Unity source与
  production reachability，只有source-confirmed且正式可达差异才在独立Change Record后修改脚本。
- **2026-08-23 D-MOV-005实施开始**：`R8-MOV-005-001 / IN_PROGRESS`。current正式DAT仍仅type2/type4
  weapon state2000走已等价fallback；exact current type0通用pass缺C++ state2000 facing writer。Task/Record/Ledger
  已先建立，下一步只允许补exact branch与正/零/负Vx fixture，尚未写脚本或取得compile证据。
- **2026-08-23 D-MOV-005代码已写**：`R8-MOV-005-001 / CODE_WRITTEN`。exact pass已在C++对应时点
  以通用state2000+Vx规则写朝向，focused Editor fixture覆盖正/零/负Vx与exact ownership；scoped diff-check
  通过。Unity compile/focused/full self-check仍待，不得标完成。
- **2026-08-23 D-MOV-005代码闭环**：`R8-MOV-005-001 / RUNTIME_PENDING`。fresh compile0，focused job
  `e5e283e740cc49e597c99b7ef994c419`为1/1 PASS，16:14:46 full self-check及74/74 validator PASS。正式
  type0 state2000 Play不可达且C++ trace BLOCKED，故不写runtime VERIFIED。`R8-WP01G-R02`进入`D-COL-005B`。
- **2026-08-23 D-COL-005B实施开始**：`R8-COL-005B-001 / IN_PROGRESS`。block-aware全DAT扫描确认
  `itr kind1=0`；但C++ generic runtime-key selector/case1 grab与Unity CLR-key gate/weapon case1 pickup为明确
  代码合同差异。Task/Record/Ledger已建立，只允许修这两个kind1分支与focused self-check，尚未写脚本。
- **2026-08-23 D-COL-005B代码已写**：`R8-COL-005B-001 / CODE_WRITTEN`。kind1 selector现读通用
  entity runtime keys，weapon case1现进generic grab；actual weapon attacker collect→object consume矩阵已加入，
  scoped diff-check通过。Unity compile/full self-check仍待，不得标完成。
- **2026-08-23 D-COL-005B代码闭环**：`R8-COL-005B-001 / RUNTIME_PENDING`。fresh compile0、actual
  weapon attacker self-check、16:21:57 full self-check及75/75 validator PASS。current DAT block-aware inventory
  `itr kind1=0`，故production Play不可得且C++ trace BLOCKED。`R8-WP01G-R02`进入`D-HIT-005`。
- **2026-08-23 D-HIT-005实施开始**：`R8-HIT-005-001 / IN_PROGRESS`。source/crosswalk确认四类attacker
  consumer未共享current-DAT target dispatcher，weapon/type3 helper被历史CLR victim签名限制，type5 mismatch缺
  common kind0入口。Task/Record/Ledger已建立；尚未写脚本或取得compile证据。
- **2026-08-23 D-HIT-005代码已写**：`R8-HIT-005-001 / CODE_WRITTEN`。四attacker consumer现共享
  current-DAT-first dispatcher；generic weapon/type3/type5 writer与三类shell mismatch矩阵已写，scoped diff-check
  通过。Unity compile/focused/full self-check仍待，不得标完成。
- **2026-08-23 D-HIT-005首次自检失败留痕**：fresh compile0后，full self-check在旧
  `BATTLE-AUDIT4-04`失败；其断言要求type3 tail，但fixture类型固定返回current type0，旧CLR SpecialAttack
  priority曾掩盖矛盾。仅把fixture改为真实current type3 shell；production dispatcher不回退，重验待执行。
- **2026-08-23 D-HIT-005第二次自检合同修正**：真实type3 shell仍未进入burning；C++ source确认type3
  effect switch只有current DAT已变成type0时才进入frame203。普通type3应保持frame20/motion reset。旧断言现按
  source改为frame20/HitConfirm2/motion矩阵；production不回退。
- **2026-08-23 D-HIT-005代码闭环**：`R8-HIT-005-001 / RUNTIME_PENDING`。fresh compile0，第三次full
  self-check PASS，focused job `9411895645354ca4a241d2a84d8525a5`为178/178 PASS。四attacker统一
  current-DAT-first dispatch，matching CLR壳保持exact writer，mismatch走generic typed writer。production
  mismatch Play夹具不可得且C++ full trace BLOCKED，故不写runtime VERIFIED。`R8-WP01G-R02`进入`D-LIFE-001`。
- **2026-08-23 D-LIFE-001复核开始**：`R8-LIFE-001 / IN_PROGRESS / NO-GAMEPLAY-CHANGE-EXPECTED`。
  C++ live merge/split与全部battle-time allocator域、Unity dormant/slot/reset/query/presentation crosswalk已重新闭合；
  当前未发现production差异。下一步只运行现有OID5152 focused/full回归并更新证据；若失败才停止并新建Change Record。
- **2026-08-23 D-LIFE-001复核闭环**：`R8-LIFE-001 / RUNTIME_PENDING / APPROVED UNITY ADAPTER`。
  `OidMergeDormant`保留partner原slot/generation与C++ low-slot inactive固定数组行为在当前battle allocator域等价，
  无production脚本修改。focused job `04ddfe7fa44b4f92beb0618d0f269a13`为32/32 PASS；同代码状态full
  self-check PASS并执行七组OID5152矩阵。真实Play/C++ trace未取得，不写VERIFIED。
- **2026-08-23 R8-WP01G-R02四项收口**：四项均已处理到当前最高证据层；MOV/COL/HIT为最小代码修复后
  `RUNTIME_PENDING`，LIFE为no-code approved adapter的`RUNTIME_PENDING`。本包停止，不自动进入后续D-ID。
  global Change Ledger validator被任务外`WEB-CADENCE-001`的non-governed/unrecorded diff阻塞；未修改该用户工作。
- **2026-08-23 R8-WP01G-R03获批并启动**：用户明确批准`physical input → movement → interaction joint
  runtime certification`并恢复总目标。Task/Handoff已建立；当前无脚本Change。执行顺序为真实InputSystem
  DDJ/DRA(DLA)→movement/jump/landing→held/grab/cpoint/collision/hit联合Play。缺少F2 probe时必须先建test-only
  Change Record，不能直接修改脚本；C++、T8、IL2CPP、Android、服务器、F1/F2 debug继续排除。
- **2026-08-23 R03 F1结果与F2探针启动**：fresh Play中DDJ physical L/S/K于tick424/425/426形成
  combo1/2/3并在tick437进入frame271，DRA physical L/D/J于tick1104/1105/1106形成1/2/3并在tick1117
  进入frame263，二者PASS。现有probe不覆盖position/velocity/landing，已在脚本改动前建立
  `R8-JOINTMOVE-PROBE-001 / IN_PROGRESS / TEST-ONLY`，只允许新增Editor-only D/K联合探针。
- **2026-08-23 R03 F2探针代码已写**：`R8-JOINTMOVE-PROBE-001 / CODE_WRITTEN / TEST-ONLY`。
  探针只由ASCII Editor菜单触发，queue physical D/K并读取FrameInputSet/runtime/position/velocity/landing；
  未改production脚本。fresh compile、Play report与self-check尚待。
- **2026-08-23 R03 F2首次探针失败留痕**：新脚本首次未被`scripts` scope导入，改用`scope=all`后
  Editor assembly fresh生成；ready Play随后在neutral tick773后RightQueued超时，FrameInputSet始终neutral。
  同会话existing DRA亦step1=-1，故暂判synthetic首键注入时点问题而非movement production first difference。
  同一test-only Record现把neutral-ready的首个D移到menu调用上下文，并补Action/device诊断，重验待。
- **2026-08-23 R03 F2第二次探针失败更正**：live trace已证明Right与physical K到达runtime：tick1279
  Right edge，tick1283 canonical Defend64/KeyDefend/CdJump5/frame210，tick1286 frame212 Vx8/Vy-16.3，
  tick1287 airborne。失败源于probe误等canonical Jump32；按既有crossed input合同改等Defend64，production不改。
- **2026-08-23 R03 F2真实Play通过**：`R8-JOINTMOVE-PROBE-001 / FOCUSED_TEST_PASS / TEST-ONLY`。
  fresh compile0；tick1080 Right edge，tick1084 physical K对应canonical Defend edge，tick1088 airborne，
  tick1091 release，tick1108 landing。DAT writer写`jump_distance=8`/`jump_height=-16.3`，首个airborne
  样本Vx7/Vy-14.6，X 775→949，对象数8→8，五项checkpoint全部true。F2报告PASS；R03继续F3。
- **2026-08-23 R03 F3联合Play通过**：held weapon报告覆盖type1/2/4/6 pickup/held/throw/landing且无
  immediate hit；grab/CPoint报告覆盖held injury、统计、mismatch throw、escape与link residue；collision/hit
  报告以10 candidates覆盖character/weapon/special、vrest、durability与abort。三者均PASS，world entity、
  claimed slot、object/logic pool及临时全局状态恢复基线。未观察到production gameplay first difference；
  R03进入fresh compile/focused/full self-check/治理收口。
- **2026-08-23 R8-WP01G-R03完成到当前可用证据**：final fresh compile/Console 0 error；8个相关
  EditMode类257/257 PASS（job `a26ba1e3136f4c73b2a17c4bd105a866`）；full `BattleRuntimeSelfCheck`
  17:17:19 PASS；Change Ledger validator PASS（78 records / 93 governed code files）；scoped diff/whitespace
  check PASS。`R8-JOINTMOVE-PROBE-001`升级`VERIFIED / TEST-ONLY`。本包没有production gameplay改动，
  没有观察到first difference；C++ executable/full trace仍BLOCKED，故不宣称全部战斗逻辑完整对齐。
- **2026-08-23 R03证据可重复性更正**：完成后审计发现DRA Temp结果曾被后续首键失败覆盖；fresh Play
  重新运行DDJ与F2也分别在L/D首键进入FrameInputSet前超时。production未变化，两个不同首键共同失败，
  当前首差为一次性`QueueStateEvent`与Editor/InputSystem采样边界，不是已定位的gameplay差异。已在脚本
  修改前建立`R8-JOINTINPUT-PROBE-002 / IN_PROGRESS / TEST-ONLY`，只允许最多8次release→press物理
  状态脉冲并记录attempt；R03临时重开，三份current报告重新PASS前不得维持完成结论。
- **2026-08-23 R03输入证据探针修正已写**：`R8-JOINTINPUT-PROBE-002 / CODE_WRITTEN / TEST-ONLY`。
  DDJ/DRA的L/方向/动作三段与F2的D/D+K两段，现仅在canonical edge未出现时按tick交替release→press，
  每阶段最多8次并输出attempt。未调用InputSystem.Update、未写runtime/buffer/frame/motion；compile/Play待。
- **2026-08-23 R03输入证据fresh重跑PASS**：`R8-JOINTINPUT-PROBE-002 / FOCUSED_TEST_PASS`。
  compile0；F2 D/K attempt2/1于tick1049/1053/1057/1077完成right/jump/air/land；DDJ attempt1/1/1于
  tick1603/1604/1616到frame271；DRA attempt1/1/1于tick2225/2226/2238到frame263。当前三份Temp
  报告均fresh PASS。D-INP-006升级Unity InputSystem S4 PASS；真实人手硬件/窗口焦点edge仍用户待验。
- **2026-08-23 R03证据可重复性最终收口**：`R8-JOINTINPUT-PROBE-002 / VERIFIED / TEST-ONLY`。
  首次focused为256/257且只失败未被本Change触碰的W05B generation；W05隔离8/8后同8类fresh复跑
  257/257 PASS（job `bf16f84db0b346809407bfe7a01dbc83`）。full self-check 17:33:15 PASS、
  Console error0、Change Ledger validator79 records/93 code files PASS。R03重新标记完成；production0改动。
- **2026-08-23 下一包审计**：68项register复核后，`D-INP-006`已由R03提升Unity InputSystem S4；
  当前最早且仍缺正常战斗联合Play证据的完整链是`D-INP-005`与`D-INP-007A/B/008/009`的AI
  sensing→39-position decision/RNG→FrameInputSet→movement/skill/opoint→hit。已建立
  `R8-WP01G-R04-ai-sensing-decision-action-joint-runtime.md / PLANNED / APPROVAL PENDING`；批准前
  不运行R04、不新增probe、不修改production。
- **2026-08-23 AI对齐范围决定**：用户明确不执行R04，未来将AI改成更适配Unity的状态树或行为树。
  `R8-WP01G-R04`现`ABANDONED BY USER / NO EXECUTION`；`D-INP-005/007A/007B/008/009`改列
  `USER-DEFERRED / NOT AN ALIGNMENT BACKLOG`。现有AI代码/测试不回退、不删除；未来AI只保留固定tick
  canonical FrameInputSet接入合同，不要求复刻C++ sensing/39-position/RNG算法。下一步审计非AI剩余项。
- **2026-08-23 非AI剩余审计完成**：当前无新发现的source-confirmed未实现normal-combat代码差异。
  可继续取得Unity运行证据的11个D-ID分4组：G1 candidate/PreInteraction（SCHED-007、PERF-001），
  G2 negative-link/P1P2（INP-001、INP-004），G3 merge/split（LIFE-001），G4 central handoff/writeback
  （SCHED-009、RENDER-001..005）。另有5个current DAT/fixture不可达exact分支、D-INP-006人手硬件待用户、
  F7/F8/F9 function-key debug policy和R1-WP02 full trace blocker。推荐下一包先做G1；尚未建立/执行R05。
- **2026-08-23 R8-WP01G-R05获批启动**：用户明确批准candidate/PreInteraction adapter joint runtime
  certification并恢复目标。Task/Handoff已建立；顺序固定`D-SCHED-007`→`D-PERF-001`。当前只读盘点
  C++/Unity/现有A/B工具，脚本0改动；若缺probe必须先建独立test-only Change Record。
- **2026-08-23 R05联合运行发现诊断误报并建Change**：fresh candidate相关EditMode为9/9、58/58、185/185，
  PreInteraction为15/15；collision与grab/CPoint live Play均PASS。相同seed的50-AI current与forced-legacy
  最终parity hash同为`fdf240f5...bef1`且两侧zero-GC PASS；current侧35/35 store authority、35/35 oracle、
  mismatch/invalid/fallback均0，却因validator把consume期间entry read与consume后的carrier count要求严格相等而
  `SmokeFailed`。已建立`R8-CANDSTORE-DIAG-001 / IN_PROGRESS / TEST-HARNESS ONLY`；不改gameplay。
- **2026-08-23 R05 candidate stress诊断修正已写**：`R8-CANDSTORE-DIAG-001 / CODE_WRITTEN`。
  validator现把consume后carrier candidate sum作为entry reads下界而非精确值，并新增extra/equal/below三段
  回归断言；collector/store/consume/PreInteraction production均未修改。fresh compile与重跑待执行。
- **2026-08-23 R8-WP01G-R05收口**：`D-SCHED-007`与`D-PERF-001`均达`UNITY JOINT S4 PASS /
  C++ FULL TRACE BLOCKED`。candidate focused9/9+58/58、consume185/185、PreInteraction15/15；collision与
  grab/CPoint live Play PASS。相同seed的50-AI current/forced-legacy均SmokePassed，20项parity/lockstep hash
  全等，双方zero-GC与cleanup PASS；current为35/35 store+oracle、mismatch/invalid/fallback0。
  `R8-CANDSTORE-DIAG-001 / VERIFIED / TEST-HARNESS ONLY`；fresh stress Editor256/256、self-check18:35:05、
  Console0、ledger80/94 PASS。production gameplay 0改动；R1-WP02 full trace限制保留。
- **2026-08-23 R06/G2只读预检**：G2必须拆分。`D-INP-001`的自然current-type0 negative-link writer为
  opoint kind2，且C++明确把child设为AI-controlled；按用户AI范围决定，不再作为非AI Play backlog，现有
  source-correct eligibility代码保留。`D-INP-004`发现新的source-confirmed production差异：C++ P2有方向键+
  numpad3/1/2完整输入，Unity `Player_2` action map只有Move，三个action lookup均为null；旧手工packet fixture
  只证明packet后routing，不能关闭physical source。已建立`R8-WP01G-R06-p1p2-physical-input-runtime.md /
  PLANNED / APPROVAL PENDING`；本次脚本/asset/Play 0改动。
- **2026-08-23 G4只读预检**：`D-SCHED-009`与`D-RENDER-001..005`未发现新的source-confirmed
  production实现缺口；现有普通Central Game/SceneView submission已有S4，但special writeback、liveness/
  identity/visibility和fail-closed ownership仍缺联合Play证书。为避免大探针，G4拆为R07A/R07B/R07C。
  已建立`R8-WP01G-R07A-render-writeback-joint-runtime.md / PLANNED / APPROVAL PENDING`，第一包只处理
  `D-SCHED-009 + D-RENDER-002` actual hit producer→frozen spark→same-tick writeback→next-tick capacity/RNG；
  本次脚本/scene/asset/Play 0改动。
- **2026-08-23 G4后续包可行性闭合**：正式`data.txt`包含OID7/8/51/223/224，pending/dormant/
  generation/death/effect/hit-stop均有production producer；CentralOnly已有Editor-only feature registration、
  failure plan与submission lease，可在不改URP asset下设计四态证书。已建立R07B
  `D-RENDER-003/004/005`与R07C`D-RENDER-001`独立Task/Handoff，均为`PLANNED / APPROVAL PENDING /
  NO EXECUTION`。禁止直接写lifecycle/visibility结果字段、改DAT/URP或恢复Legacy制造PASS。
- **2026-08-23 G3真实Play可行性闭合**：`LF2States.Running==2`，正式`data.txt`含OID7/8/51；
  low-slot self/partner、same-team/proximity/HP前置可在测试初始边界配置，merge/dormant/split必须由完整tick的
  OID maintenance产生。已建立`R8-WP01G-R08-oid5152-merge-split-central-runtime.md / PLANNED /
  APPROVAL PENDING`；split优先真实DJA，DJA不可达时完整推进4500 fixed ticks，禁止直接写Unk338=0。
  本次脚本/DAT/scene/Play 0改动。
- **2026-08-23 R06获批启动**：用户明确批准`R8-WP01G-R06`并恢复目标。现状复核确认Player_2只有Move，
  auto-generated wrapper也只有Move；`.inputactions.meta`为`generateWrapperCode:1`。已在写入前建立
  `R8-P2INPUT-001 / IN_PROGRESS`，范围仅为P2 Attack/Jump/Defend+numpad1/2/3、正规wrapper生成、focused
  test与two-player physical Play probe；P1/crossed mapping/8-slot及所有保护边界不变。
- **2026-08-23 R06代码已写**：`R8-P2INPUT-001 / CODE_WRITTEN`。Player_2新增Attack/Jump/Defend及
  numpad1/2/3 exact binding；Unity Input System generator已正规更新wrapper，未手改生成代码。新增asset/action
  聚焦测试与11-case two-player physical Play probe；probe只queue KeyboardState并观察正式FrameInputSet/
  roster/runtime，不直接写packet或runtime。新脚本compile/focused/Play/self-check/validator待执行。
- **2026-08-23 R8-WP01G-R06收口**：`D-INP-004`达到`UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE
  BLOCKED`。fresh compile0；focused2/2、input regression47/47 PASS；未保存two-human Play clone由正式
  bootstrap/object pool/roster创建slot0/1，11/11 physical press/held/release/no-cross PASS，stable100/101保持。
  full self-check 19:37:29 PASS，Play结束前Console error0；Change Ledger validator 81 records/96 governed
  code files与scoped diff-check PASS。`R8-P2INPUT-001 / VERIFIED`；R07A/B/C、R08仍未执行，R1-WP02
  full trace仍BLOCKED。
- **2026-08-23 R8-WP01G-R07A获批启动**：用户明确批准render pass/hit-record writeback联合运行时认证。
  C++只读source与Unity crosswalk复核未发现新的静态production差异；existing exact baseline job
  `7ec88f1aa50f4f93af44990ad9a08dd6`为2/2 PASS（worker lifecycle、10-slot full RNG gate）。现有证据仍缺
  actual collision producer的完整tick Play链，故已在写入前建立`R8-HITWRITEBACK-001`并只新增Editor-only
  probe。probe现已写入；第一次scripts-only refresh的0 error未覆盖无`.meta`的新文件，full asset refresh首次
  实际导入发现probe内CS0102、随后CS0165，均已只在test-only probe中修复；fresh compile现为0 error，
  Change状态为`COMPILE_PASS`。Play联合报告、expanded focused/self-check与validator仍待执行，
  R07B/R07C/R08继续未执行。
- **2026-08-23 R07A首轮Play**：场景就绪后于worker path `startTick=1510`跑到真实hit producer；报告因
  probe错误要求presentation writeback同步更新Unity-only `LastAdvanceTick`而FAIL。source复核确认C++合同与
  `FinalizePublishedHitRecordCycle`只要求age/tail推进；该字段由另一API维护，不能作为本包权威断言。
  `R8-HITWRITEBACK-001`现为`RUNTIME_PENDING`，只删除该test-only越界断言后重跑；production未改。
- **2026-08-23 R07A第二轮Play**：删除`LastAdvanceTick`越界断言后，worker Play已通过actual hit、live age、
  owner/cycle与RNG前置；下一FAIL来自probe误要求纯worker `PublishedFrame`已经materialize commands。既有worker
  contract明确该frame必须保持未物化，中央宿主随后在`CurrentPixelFramePlan.CapturedFrame`物化。下一步只把
  probe检查移到正式central captured frame可用后，不调用self-check materializer，production未改。
- **2026-08-23 R07A第四轮Play**：正式central captured frame等待修正有效；tick1018已完整证明actual hit、
  RNG+2、frozen/live age、central command与Late幂等。tick1019同一pair未重复追加，属于正式hit-rest边界；
  probe不会清rest或直接写hit，下一步只在加载阶段预建独立攻击者并逐tick启用新pair以验下一tick追加。
- **2026-08-23 R07A第五轮Play**：轮换独立attacker但共享victim仍在第二tick保留1条record，受击者侧状态/
  交互资格会抑制立即重复命中。下一步改为4组预建独立pair逐tick启用；旧victim记录仍留在world参与
  publication/no-publication生命周期，不清任何rest/状态，不直接写record。
- **2026-08-23 R8-WP01G-R07A收口**：4组独立pair的production worker Play tick843～846 PASS：published
  owner/record/command为1/2/3，frozen ages`[0]`/`[1,0]`/`[2,1,0]`，live ages`[1]`/`[2,1]`/
  `[3,2,1]`；no-publication保持cycle845并推进`[4,3,2,1]`。每tick exact2 RNG、Late幂等、warmed
  allocation violation delta0，cleanup全恢复。compile0；worker18/18、hit178/178、central13/13、20:25:11
  self-check PASS；final Console0；ledger82/97 PASS。`R8-HITWRITEBACK-001 / VERIFIED`；
  `D-SCHED-009 + D-RENDER-002 = UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。R07B/R07C/R08未执行。
- **2026-08-23 R07B恢复审计/合同纠正**：实施批准仍未获得，未改脚本、未运行专项Play。只读复用盘点
  确认WP01C opoint lifecycle可提供pending/release/generation producer，WP01D central probes可提供
  command/pixel观察；同时发现R07B验收曾要求OID7/8→51 dormant/split却把R08列为out-of-scope。现已纠正：
  R07B只处理`D-RENDER-003` pending/generation/T+1子集与`D-RENDER-004/005`；dormant/split只归R08，
  R08完成前不得整体关闭`D-RENDER-003`。R07B继续`APPROVAL PENDING / NO EXECUTION`。
- **2026-08-23 R07B获批执行**：用户明确批准`R8-WP01G-R07B`并恢复总目标。只读预检确认正式
  FrameLogic pending producer、RenderDispatch→Late opoint的T/T+1边界、正式data.txt OID223/224和production
  opoint factory均可用；现有producer probe与central diagnostic probe不能形成同一handle/generation的联合报告，
  因此已在脚本写入前登记`R8-RENDERLIVE-001 / IN_PROGRESS`，只允许新增Editor-only联合Play probe。
  production gameplay/DAT/scene/renderer保持不改；dormant/split仍只归R08。
- **2026-08-23 R8-WP01G-R07B收口**：`R8-RENDERLIVE-001 / VERIFIED / TEST-ONLY`，production 0改动。
  fresh sync full-tick Play tick202→203 PASS：OID225/frame51使`slot51/gen1` pending/free，Late OID999同槽
  `gen2`；T冻结拒绝旧/新句柄，T+1只接受新generation并恢复body/shadow。正式OID223/224 body均有
  snapshot/command/resource/submission，shadow snapshot存在但current-DAT gate返回`CommandSuppressed`且无
  command/submission；baseline正式角色body/shadow均提交。actual Z 376/375按`ZInt→slot`排序，同Z tie由
  focused覆盖。focused24/24+9/9+worker18/18、21:47:26 self-check、final Console0、ledger83/98 PASS。
  `D-RENDER-003`仅pending/generation/T+1子集达到Unity S4，dormant/split仍归R08；`D-RENDER-004/005 =
  UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。R07C/R08仍未执行。
- **2026-08-23 R8-WP01G-R07C获批启动**：用户明确批准`R8-WP01G-R07C`并恢复总目标。只读预检确认
  exact cold→current→last-good→replacement ownership self-check、Game/SceneView current pixels、Editor-only
  ready/stale publication与submission lease均可复用；尚无已确认production renderer差异。已在脚本写入前建立
  `R8-CENTRALOWN-001 / IN_PROGRESS / TEST-ONLY`，只允许新增Editor-only联合Play probe；不得修改URP asset、
  scene、material asset、production registration、gameplay或Legacy owner。cold若无法安全形成则保留exact
  self-check并诚实标记，不破坏live feature注册制造PASS。
- **2026-08-23 R07C test-only代码已写**：`R8-CENTRALOWN-001 / CODE_WRITTEN`。新Editor-only probe
  在真实current plan上采集isolated Game-camera pixels，通过既有stale self-check boundary保留last-good，
  持旧submission lease后用render-only dispatch发布replacement并检查generation/retire/lease、legacy suppression、
  checksum及feature/material/draw-mode恢复；cold明确只由exact self-check覆盖。production renderer/gameplay/
  URP asset/scene/material均0改动；fresh compile、focused、Play、自检和validator待执行。
- **2026-08-23 R07C Play启动隔离诊断**：两次Play均在probe request消费前被Bootstrap阻塞；根因是禁用
  Domain Reload时EditMode central测试/编辑器URP回调留下active submission，`BeginBattleAllocationSeal`容量预热
  拒绝resize。未改production初始化；只在`R8-CENTRALOWN-001` test-only probe增加prepare-play菜单，调用既有
  `ResetRuntime()`后在同一菜单回调立即进入Play，避免下一个EditMode render重新发布。该reset不作为cold证据。
- **2026-08-23 R07C首次完整导入失败已留痕**：此前scripts-only refresh未导入apply_patch新增文件；full asset
  refresh首次编译发现probe误写`using NTSD.Simulation.Input`，而`FrameInputSet`位于`NTSD.Simulation`。已只删除
  该错误using；production未改，fresh full refresh待重跑。
- **2026-08-23 R07C第二次完整导入失败已留痕**：修正FrameInputSet using后，probe对`NTSDRenderSpace`
  缺少`NTSD.Animation` namespace；已只补正确using，production仍0改动，fresh full refresh继续重跑。
- **2026-08-23 R07C首轮场景Play失败已留痕**：prepare-play成功绕过静态submission预热阻塞，场景运行到
  tick195、4 objects/2 slots、feature/material均注册；但Game视图没有主动消费worker PublishedFrame，probe一直
  等不到current plan。已只在test-only probe于PublishedFrame/feature就绪后调用公开`PrepareFrame(world)`，
  等价于中央宿主消费，再由真实URP Game camera取像素；production未改，待重新编译/Play。
- **2026-08-23 R07C第二轮场景Play失败已留痕**：到tick201仍无current；既有Game Visibility诊断确认当前
  driver由外层手动推进且PublishedFrame为空，PrepareFrame没有输入。probe现只在该空边界调用一次当前tick的
  公开`world.RenderDispatchAll(currentTick)`建立正式表现快照，再PrepareFrame；不推进gameplay、不写实体字段，
  production仍0改动。
- **2026-08-23 R07C第三轮场景Play失败已留痕**：RenderDispatch后仍等不到current，确认PrepareFrame进入
  readiness拒绝但旧probe未记录reason。已只增强test-only失败报告：首次拒绝立即写plan/diagnostic reason、
  PublishedFrame tick、feature/material readiness，不再超时猜测；production未改。
- **2026-08-23 R07C第四轮场景Play失败已留痕**：仍timeout且未进入PrepareFrame拒绝分支，确认停在首次
  BMP/catalog加载前置。原1200 Editor updates低于同项目R07B的12000；已只把R07C test-only等待上限对齐
  到12000，不改变runtime gate或production代码。
- **2026-08-23 R07C第五轮Play启动隔离失败已留痕**：菜单Reset到异步EnterPlay之间仍有Editor URP
  重发布窗口，Bootstrap再次遇到active submission。未改production Bootstrap；test-only入口改为一次性武装
  `playModeStateChanged`，在ExitingEditMode和首帧Start前的EnteredPlayMode各Reset一次，随后立即解除武装。
- **2026-08-23 R07C第六轮Play失败已留痕**：启动隔离已成功，但timeout计数从tick0、BMP/catalog加载前
  开始，并在feature/object刚就绪的update先于readiness分支触发。已只把计数移动到tick/object/CentralOnly/
  feature均ready之后；资源加载不再消耗current-plan等待预算，production未改。
- **2026-08-23 R07C第七轮Play失败已留痕**：scene-ready后仍超时且没有PrepareFrame拒绝报告，确认只剩
  跨update paused门；初始化链会再次SetPaused(false)，probe反复返回。current/stale/replacement在同一Editor
  主线程回调同步完成且worker in-flight单独受控，故只删除该paused前置，pause开始/结束状态仍保持不变。
- **2026-08-23 R07C第八轮Play首差已留痕**：首次进入联合捕获，current owner/tick/gen/lease/Legacy
  suppression与cleanup正确，但source/resolved/segment均0、isolated pixel0，说明实体已注册而sprite/catalog命令
  尚未就绪。已只增加source/resolved/segment>0表现就绪门，再开始四态；production未改。
- **2026-08-23 R07C第九轮Play全链首PASS**：current/stale/replacement各259 isolated pixels且hash一致；
  stale display tick200/gen201，replacement tick201/gen202；checksum不变，old retire/reject/release、feature/material/
  draw-mode和world cleanup均true。replacement lease/draw字段因在Camera.Render前采集显示false/0，已只把该诊断
  移到渲染后重采并增加lease/segment/draw>0断言，待最终重跑；production0改动。
- **2026-08-23 R07C最终收口为BLOCKED**：最终三态Play PASS：current211/211 gen212、stale212/211
  gen212、replacement212/212 gen213；各4/4 commands、1 segment/draw、259px、hash
  `AE3AFF1E932B491E`，lease/retire/release、checksum `7C369C0D79EF47BA`与cleanup全PASS。cold exact
  self-check PASS、Play未运行。final normal Play同时出现`B-R8-R07C-01`：异步加载期间已有active central
  submission，后续`BeginBattleAllocationSeal→PrepareBattleCapacity`拒绝resize。production first-difference
  stop condition已触发；`R8-CENTRALOWN-001 / BLOCKED`，R07C整体BLOCKED。fresh compile0、focused29/29、
  22:45:37 self-check PASS、clear后Console0、ledger84/99 PASS，但均不能覆盖该Play异常。已建立
  `R8-WP01G-R07C-R01 / PLANNED / APPROVAL PENDING / NO EXECUTION`；未批准前不改production、不进R08。
- **2026-08-23 R07C-R01获批启动**：用户明确批准production repair并恢复总目标。caller审计确认直接场景
  由BattleTestBootstrap初始化且会创建AppManager；菜单/additive场景由既有AppManager初始化而test bootstrap
  立即skip，两条入口互斥，不是double-seal。first difference是BattleBootstrap camera可在loading期间发布
  submission，而两条入口都在实体装配后才BeginSeal；BeginSeal本身也缺少already-sealed早退。已在脚本写入前
  建立`R8-CENTRALSEAL-001 / IN_PROGRESS`，计划Awake disable→seal后Enable→unpause，并补strict idempotence；
  不削弱submission lease/resize保护、不改gameplay。
- **2026-08-23 R07C-R01代码已写**：`R8-CENTRALSEAL-001 / CODE_WRITTEN`。BattleBootstrap Awake
  首帧前disable/clear；AppManager与BattleTestBootstrap均改为BeginSeal后、unpause前Enable；BeginSeal对allocation
  gate+runtime capacity均sealed严格no-op。新增Awake lifecycle与active submission后重复seal focused tests。
  submission resize/lease保护、gameplay、URP asset/scene/DAT均未改；compile/runtime证据待执行。
- **2026-08-23 R07C-R01执行中修正**：第一版Awake disable虽取得compile0、focused20/20、self-check PASS、
  normal Play Console0与R07C PASS，但用户观察到Play时Camera被关闭；该副作用不接受。已先更新
  `R8-CENTRALSEAL-001`合同，改为Camera保持启用、首次BeginSeal在presentation capacity prepare前清退旧central
  publication、重复seal仍strict no-op；R07C探针改为等待seal完成。修正版代码与全部验收待执行。
- **2026-08-23 R07C-R01最终收口**：`R8-CENTRALSEAL-001 / VERIFIED`。最终production只在
  `AppManager/BattleTestBootstrap`把Enable移到seal后、在首次`BeginBattleAllocationSeal`的presentation capacity
  prepare前清退旧central publication，并对双sealed重复调用strict no-op；`BattleBootstrap`第一版Awake-disable已
  撤回且无净diff。fresh compile0；focused job `4cd77be4f1664b329a1e6f3b8167cfc9` 20/20；23:13:13
  full self-check PASS；normal Play直接读取`ScenesCamera.enabled=true`且Console0。final R07C为current
  214/214/gen216、stale215/214/gen216、replacement215/215/gen217，三态4/4/1/1、259px、hash
  `AE3AFF1E932B491E`、checksum/cleanup PASS、Console0。Combat1000为30 warmup+180 sample、1000
  entities/slots、Avg/P95/Max 19.121/21.687/23.805ms、0 B/tick、0 collection、cleanup restored。
  `B-R8-R07C-01`关闭，`R8-CENTRALOWN-001 / VERIFIED`；validator 85 records / 103 governed code files PASS。
  C++ full trace仍BLOCKED，R08未启动，T8/AI/IL2CPP/Android/服务器边界不变。
- **2026-08-23 R08获批启动**：用户明确批准`R8-WP01G-R08`并恢复总目标。再次只读核对C++
  `game_tick.cpp:1008-1154`、Unity `Oid5152RuntimeMaintenanceAll`与正式data.txt OID7/8/51，未发现执行前
  必须修改production gameplay的新差异。已在任何脚本写入前建立`R8-MERGESPLIT-001 / IN_PROGRESS /
  TEST-ONLY`；下一步只读确认running/DJA reachability后新增Editor-only production Play probe。禁止直接写
  merge/dormant/split结果或Unk338=0；first difference必须停止并拆repair。R1-WP02、T8、AI、IL2CPP、
  Android和服务器边界不变。
- **2026-08-23 R08只读可达性阻塞**：`B-R8-R08-01`。当前`Assets/NTSD/Config/data.txt`声明
  OID7/8/51，但`Config/chars/rock_lee.dat`、`chiyo.dat`、`sasori.dat`均不存在；正式loader没有fallback并会
  跳过wrapper，无法满足R08 production factory前置。相邻`I:\GitHub\Unity_GAS\ntsd_proto`虽有同名加密DAT，
  但Task禁止新增/修改DAT且其Unity适配来源未确认，未复制/解密。`R8-MERGESPLIT-001 / BLOCKED /
  NO SCRIPT WRITTEN`；probe与专项Play均未开始，production/C++均0改动。恢复条件为用户恢复三份Unity DAT，
  或明确确认合法Unity资产来源与允许部署方式。
- **2026-08-23 B-R8-R08-01资源身份correction**：初次只检索`ntsd_release`源码树而未见DAT；扩大到
  实际运行根目录后，在`J:\QQFile\NTSD2.4\chars`找到OID7/8/51正式runtime DAT。它们与
  `I:\GitHub\Unity_GAS\ntsd_proto\ntsd_assets\chars`同名文件的长度和SHA-256全部不同，故后者不能作为
  权威/Unity适配资源直接补入。两侧均保持只读，未复制/解密/写入；blocker与恢复条件不变：需要用户恢复
  当前Unity适配版三份DAT，或明确其合法来源/部署方式。
- **2026-08-24 type0资源恢复获批**：用户指定DAT源`J:\QQFile\NTSD 2.4.1\chars`、BMP源
  `J:\QQFile\NTSD 2.4.1\sprite`，目标为`Config/Character`和`Sprite/Character/<dat-basename>`，并要求
  同步改data.txt与DAT bmp_begin路径。已在资源写入前建立`R8-CHARASSET-001 / PLANNED / RESOURCE-ONLY`及
  独立Task；先完整预检，禁止覆盖已有资源或改任何DAT战斗字段。此用户资源部署授权取代R08中“DAT零改动”的
  默认限制，但不授权C++、gameplay、T8或其他范围变更。
- **2026-08-24 type0资源恢复与验证完成**：`R8-CHARASSET-001 / VERIFIED / RESOURCE-ONLY`。已恢复37份
  缺失type0 DAT、182个去重BMP；`data.txt` 42个type0全部指向`Assets/NTSD/Config/Character/`，并以运行时同一
  decryptor/parser复核DAT内227条BMP路径全部存在。Unity全资源导入compiler error=0；定向资源测试job
  `34a8a483ff314b82b65e9df5f4aaaf0e` 1/1 PASS；`NTSD_Battle`正常Play 20秒Console error/warning=0。
  `B-R8-R08-01`资源前置关闭、R08变为READY；但merge/dormant/split probe与任何production gameplay改动均尚未执行。
- **2026-08-24 type0资源测试 Change已收口**：`R8-CHARASSET-TEST-001 / VERIFIED`。它只覆盖type0
  DAT/BMP资源可读性，不改gameplay；全资源导入compiler error=0，定向EditMode job
  `34a8a483ff314b82b65e9df5f4aaaf0e` 1/1 PASS，正常`NTSD_Battle` Play Console error/warning=0。其余R08
  merge/dormant/split行为保持未执行。
- **2026-08-24 R08恢复执行**：用户明确恢复目标；`R8-MERGESPLIT-001 / IN_PROGRESS / TEST-ONLY`。
  B-R8-R08-01已关闭；只读DAT确认OID7/8正式state2 frames为9/10/11/19，OID51 frame290为
  `state15/wait2/next999`且无hit_ja。C++与Unity均确认merged DJA完成后走`Unk328==1→Unk338=0`，下一M1
  在frame290自然split；不需要4500 tick或测试直接写cooldown。下一步只允许新增已登记的Editor-only production
  Play probe；production gameplay、DAT、C++、T8、AI及架构边界不变。
- **2026-08-24 R08新first difference停止**：`R8-MERGESPLIT-001 / BLOCKED / B-R8-R08-02`。probe在一次
  probe-only类型修正后fresh compile0，但正式Play尚未创建OID7/8 fixture，production sprite prewarm先抛
  `Duplicate battle sprite key (56,112)`。正式OID56 DAT的106-120与112-200范围重叠；42个type0仅此一组。
  C++ `renderer.cpp:590-606`按声明顺序首匹配后break，Unity当前异步sheet覆盖与catalog duplicate guard不支持该
  first-declared-wins语义。已建立`R8-SPRITERANGE-001 / PLANNED / APPROVAL PENDING`通用repair；未获批准前
  不改production。R08 merge/dormant/DJA/split仍未运行，不能标完成。
- **2026-08-24 B-R8-R08-02全目录补充审计**：已用项目正式DAT解密密钥/减法算法只读解析`data.txt`全部137个
  对象，137/137成功、共347条`file(lo-hi)`范围，只有OID56的106-120与112-200重叠（交集112-120）。这证明
  当前catalog阻塞的已知数据范围是单一输入实例，但production缺口是通用first-declared ownership合同，不能按
  OID56硬编码。`R8-SPRITERANGE-001`仍为`PLANNED / APPROVAL PENDING / NO CODE WRITTEN`；R08保持BLOCKED。
- **2026-08-24 R08-R01获批恢复**：用户明确批准`R8-WP01G-R08-R01 / R8-SPRITERANGE-001`并恢复总目标。
  Change状态改为`IN_PROGRESS / PRE-CODE`；只允许在`CharacterAnimtorManager`实现通用first-declared ownership、
  新增focused test并执行既定回归。DAT、C++、CentralOnly/atlas架构、gameplay和R08探针验收标准保持不改。
- **2026-08-24 R08-R01代码已写**：`R8-SPRITERANGE-001 / CODE_WRITTEN`。`CharacterAnimtorManager`在并行sheet
  调度前按DAT files顺序建立first-owner集合，sheet任务和catalog构建共同使用该集合；later range即使先完成也不能
  覆盖前序owner。新增独立overlap focused test；builder duplicate guard、DAT/C++/CentralOnly/atlas/gameplay/R08
  探针均未改。compile、focused、正常Play、R08重跑和self-check尚未执行，不能标完成。
- **2026-08-24 R08-R01编译通过**：首次完整导入仅新test因NUnit链式`Does.Contain(int)`产生5条CS1503，已
  留痕并改为布尔Contains断言。第二次force-all后Editor DLL晚于source，UnityMCP Console `error CS`=0、全部
  error=0；`R8-SPRITERANGE-001 / COMPILE_PASS`。focused、正常Play、R08和self-check仍待执行。
- **2026-08-24 R08-R01 focused通过**：overlap job `64acdbff4e2f46aeafc519eed0f68d2b`为2/2 PASS；
  existing common-atlas/catalog-resolver/device-policy job `3da7ae8f160a4e7cacf1a6e84a1c1dc5`为29/29 PASS。
  Change推进为`FOCUSED_TEST_PASS`；normal Play、R08 merge/split与full self-check仍未运行。
- **2026-08-24 R08-R01 normal Play与R08恢复**：`NTSD_Battle`正常Play 25秒error/warning0，旧
  duplicate `(56,112)`未再出现。全DAT映射探针审计137 definitions、12487 catalog entries，没有OID56 range/source
  mismatch；整体唯一FAIL为独立state8000 dynamic command witness且`workerPath=false`，不作为本Change PASS。
  R08随后已真正执行OID7/8 fixture：OID7→51/frame290、HP/HPBound/PP/metadata和OID8 dormant正确，cleanup恢复。
- **2026-08-24 R08 probe合同修正中**：首轮probe被另一Editor request poller清pause；已只在当前probe运行期重新
  断言pause。随后旧断言误把Unity `ObjectCount`当纯逻辑实体数；structural delta证明合体tick没有额外spawn/register，
  而既有Unity adapter中每个production character同时贡献logic+shell。probe已改为记录post-fixture count并严格验证
  dormant后减1。最新代码fresh compile0，但当前Unity Play主线程高CPU、7.7GB且所有非ping MCP命令在内部30秒
  timeout，尚未完成该版R08重跑/full self-check；未强杀Editor或启动第二实例。
- **2026-08-24 R08真实split first difference停止**：用户退出Play后恢复目标，R08依次纠正test-only的
  logic+shell ObjectCount、动态stage Z、同tick physics终值与canonical FrameInputSet输入合同。正式OID51 frame290
  `hit_ja=0`，C++/self-check均证明DJA不可提前清cooldown，故按合同分批推进4500完整tick。merge runtime、OID51
  Central body与dormant suppression均通过；final maintenance进入`partner.Reset()`时，relation/link setter向已排除的
  AI unified row发布并抛`stale slot generation after commit`。这是production first difference `B-R8-R08-03`，
  split/cleanup未完成。`R8-MERGESPLIT-001 / BLOCKED`；已建立`R8-WP01G-R08-R02 / R8-AIROWGEN-001 /
  PLANNED / APPROVAL PENDING / NO CODE WRITTEN`。批准前不得改production、削弱ValidateRow或改变slot generation。
- **2026-08-24 R08-R02只读预检闭合**：未修改production。已确认slot/store generation并未被错误推进；异常来自
  CharacterInput激活的unified publisher持续到RuntimeMaintenance，而dormant partner不在当前Included row、四类store
  仍以原generation绑定。推荐最小repair是通用`row-membership invalidation`：merge进入dormant前和split reset前结束
  当前publisher，下一tick因publisher inactive强制full rebuild；不unbind store、不吞ValidateRow、不增generation、
  不release slot。focused矩阵必须验证merge排除、split恢复和next-tick原generation重纳入。Change仍为
  `R8-AIROWGEN-001 / PLANNED / APPROVAL PENDING / NO CODE WRITTEN`。
- **2026-08-24 R08-R02获批恢复**：用户明确批准`R8-WP01G-R08-R02 / R8-AIROWGEN-001`并恢复总目标。
  Change推进为`IN_PROGRESS / USER APPROVED / PRE-CODE`；只允许先写focused旧实现复现，再实现通用merge/split
  row-membership invalidation。generation、allocator、store owner绑定、ValidateRow、AI策略及其他模块保持不改。
- **2026-08-24 R08-R02 focused reproduction已写**：仅修改existing
  `AiDecisionSoAShadowEditorTests`，新增merge不得roll-forward dormant row与split原generation reset/reactivate两条用例；
  production尚未改。下一步先compile并在旧实现上取得预期FAIL，再实施repair。
- **2026-08-24 R08-R02旧实现失败与代码写入**：focused job `aebfc0fa94ad4b3bac8d2b0230aee229`
  的split用例精确复现Play同一stale-row异常；merge fixture前置已修正。production最小repair已写：publisher新增
  row-membership invalidation，occupancy API复用；merge进入dormant前与split partner.Reset前各失效current pass。
  `ValidateRow`、store绑定、generation、allocator、AI策略均未改。Change=`CODE_WRITTEN`。
- **2026-08-24 R08-R02编译外部阻塞**：production写入后的force-all发现S0 HOLD文件
  `InProcessLockstepAuthoritySessionEditorTests.cs:168/175`两条既有CS0019（`int % SimulationInputButtons`）。R02不越权
  修改S0；最新R02尚未进入DLL，compile/focused/self-check/R08均不得标通过。
- **2026-08-24 S0 syntax-only阻塞关闭 / R08-R02编译通过**：在`S0-INPROC-AUTHORITY-001`既有Record下仅为
  `tick % N switch`增加两处括号，不改变S0输入序列或HOLD。force-all后Editor DLL晚于R02 source，Console全部
  error=0；`R8-AIROWGEN-001 / COMPILE_PASS`。focused/self-check/R08仍待。
- **2026-08-24 R08-R02 focused通过**：新增2/2、unified authority21/21、CharacterInput live-slot/0-GC37/37
  均PASS；`R8-AIROWGEN-001 / FOCUSED_TEST_PASS`。整类扩大运行唯一`Position38 predicted-DUA`失败，独立重跑仍
  失败且不经过OID5152/membership路径，保留为独立既有AI fixture问题，不用它否定或证明本Change。下一步full
  self-check与R08 4500-tick Play。
- **2026-08-24 R08-R02 full self-check外部阻塞**：request结果`01:33:25Z`在OID5152检查前由独立
  `R-HC-01 / CheckDeployableResolvedGeometryRisks`失败，来源为恢复DAT中的`bdy h=-999`和`itr w=0`未分类形状。
  本Change不处理DAT/parser/geometry；不能标self-check PASS。继续执行直接R08 4500-tick Play验收。
- **2026-08-24 R08-R02 / R08最终Unity S4通过**：`R8-AIROWGEN-001`关闭了split reset的stale unified-row
  production异常。最终`Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json`于`01:48:32Z`写入PASS：
  4500 tick、OID7/8→51→7/8、dormant、原slot0/10+generation1、当前HP/HPBound各半95/95、tick末
  frame113/state8、Central merged/dormant/split visibility均通过。split局部ObjectCount `14→15`、claimed `8→8`；
  generation-safe cleanup释放5个post-baseline实体，最终world/claimed/object pool/logic pool恢复`2/1/1/1`，RNG恢复、
  cleanup error为空。`R8-AIROWGEN-001`与`R8-MERGESPLIT-001`均为`VERIFIED`，`B-R8-R08-03`关闭。
  full self-check仍被独立`R-HC-01`前置阻塞，R1-WP02 full trace仍BLOCKED；退出Play后有1条Unity scene-close warning，
  故不声明本轮warning0，也不扩大成全C++ runtime完整认证。
  最终`git diff --check`无whitespace error，Change Ledger validator PASS（91 records / 111 governed files）。
- **2026-08-24 post-R08剩余审计与下一包准备**：原11个可执行非AI D-ID现均已达到Unity S4或明确
  source-deferred边界；`D-LIFE-001`与`D-RENDER-003`由R08提升为`UNITY JOINT S4 PASS / C++ FULL TRACE
  BLOCKED`。当前最前置可行动项是验证基础设施`R-HC-01`：正式OID58 frame75/76和OID10 frame75/76/77
  含5个`w21/h-999`倒置body。只读C++与Unity production均保留raw `y2=y1+h`和strict overlap；普通小itr
  不命中，但跨过倒置两端点的大itr仍会命中。旧self-check分类缺失而非已确认gameplay差异。已建立`R8-WP01G-R08-R03`、
  `R8-GEOMETRYCHECK-001 / PLANNED / APPROVAL PENDING / NO SCRIPT CHANGE`与handoff；批准前不改self-check。
- **2026-08-24 R08-R03获批恢复**：用户明确批准`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001`并恢复目标。
  Change推进为`IN_PROGRESS / USER APPROVED / PRE-CODE`；只允许修改self-check的negative-height body分类和
  production collector夹具，DAT/parser/production collision及其他模块保持0改动。
- **2026-08-24 R08-R03 pre-code authority correction**：进一步按strict不等式代入确认negative-height rect并非
  全局inert；大itr若同时跨过其两个倒置端点仍会命中。Task/Record/Handoff已更正为同时验证普通不命中与跨端点
  命中、并覆盖左右朝向；production仍0改动。
- **2026-08-24 R08-R03代码已写**：`R8-GEOMETRYCHECK-001 / CODE_WRITTEN`。仅self-check增加精确5-entry
  negative-height body分类与ordinary/enclosing × right/left production collector四矩阵；其他non-positive geometry
  继续fail closed。production collision、DAT、parser及其他脚本0改动；compile/self-check/回归尚未运行。
- **2026-08-24 R08-R03验证完成 / 新first difference停止**：fresh compile0；full self-check实际越过R-HC-01，
  日志为137 definitions、82200 frames、90 zero-width itr、5 known negative-height body、0 unexpected/other，四个
  ordinary/enclosing × right/left production collector断言全部通过。`R8-GEOMETRYCHECK-001 / VERIFIED`，R-HC-01关闭。
  随后独立`CheckMovementDatLoadingContracts`因仍读取已迁移删除的`AnimationConfig/Mingren/naruto.dat`失败；这是
  test fixture path first difference，不属于几何包。已拆`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001 / PLANNED /
  APPROVAL PENDING / NO SCRIPT CHANGE`；批准前不顺手修改路径。
- **2026-08-24 R08-R04获批恢复**：用户明确批准`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001`并恢复目标。
  Change推进为`IN_PROGRESS / USER APPROVED / PRE-CODE`；只允许让self-check按objectId读取当前
  `ObjectDefinition.file`并替换旧硬编码callsite，production catalog/loader、data.txt、DAT和gameplay保持0改动。
- **2026-08-24 R08-R04代码已写**：`R8-DATFIXTUREPATH-001 / CODE_WRITTEN`。self-check新增objectId→当前
  `ObjectDefinition.file`→production resolver overload，11个production DAT callsite已迁移；旧AnimationConfig与
  FrameConfig clone literal清零。decrypt/parser/converter与字段断言未改，production/资源0改动；验证待执行。
- **2026-08-24 R08-R04验证完成**：fresh compile0；CharacterAssetDeployment job
  `75849d918dec46d88b01f1253cecec63` 1/1 PASS；`Temp/NTSD_BattleRuntimeSelfCheck.result`于`02:27:38Z`
  写入PASS。movement、Naruto DDJ、sprite range、weapon原字段断言均通过；预期负向fixture error日志清空后最终
  Console0。`R8-DATFIXTUREPATH-001 / VERIFIED`，production/资源0改动，没有暴露新的first difference。

- **R1-WP01 阻塞**：无；规划已完成。
- **B-R1-WP02-01 — trace coverage blocker**：未发现能从未修改 release runtime 取得 R1 full schema 的既有外部通道。`NTSD_DEBUG_TICK` / 相对诊断文件是局部日志线索，不覆盖统一 checkpoint 合同。
- **B-R1-WP02-02 — deterministic input blocker**：没有发现现成逐 tick input journal/replay 或 non-interactive CLI；入口忽略 argv 且依赖 SDL/物理键盘。
- **B-R1-WP02-03 — authority write-safety blocker**：已有诊断路径以相对文件 append 写入；非 authority working directory 的资源加载和无写入保证尚未验证，不能为验证而冒险启动。
- **B-R1-WP02-04 — source/executable identity blocker**：当前 source/Makefile 与实际 `ntsd_new.exe` 没有可验证的精确 build identity。
- **停止结论（仅 R1-WP02）**：按照 D-006 和 R1-WP02 Task Contract，该 Work Package 已停止。不得修改 C++、不得改用 debug/diagnostic executable、不得开始 Unity trace 或 comparator 以绕过 blocker。
- **R1 主线边界**：C++ 源码行为合同、Unity 静态 crosswalk、差异登记和验收设计不依赖 R1-WP02 自动 full trace；R1-SOURCE-001～007 已完成静态盘点。历史上 R1 closure 曾要求确认 R2-PASS-01；当前该门槛已由用户的连续执行授权和 `D-009` 取代，R2～R8 仍须遵守各自 Task Contract / Change Record / 分层验收，但不再逐包停止。
- **R1-WP02 恢复条件**：用户提供或确认一个现有的、可从未修改 `ntsd_new.exe` 使用的只读采集/输入方案，并能同时解决非 authority 输出、最小 fixture/run identity 与所需 trace 覆盖；否则 R1-WP02 保持 BLOCKED。

## 2026-08-24 — R8-WP01G-R09 final evidence reconciliation planned

- R08-R04完成后进行只读现状审计：R05～R08可执行非AI联合证据均已到当前允许层级，完整self-check亦恢复PASS；
- 发现父编排、68项D-ID登记册与旧synthesis仍含被R07B/R08/R03/R04取代的历史pending/blocked文本，需独立
  文档包统一校正，不能据此重复修改gameplay；
- 已建立`R8-WP01G-R09-final-evidence-reconciliation.md`与对应handoff；状态为
  `PLANNED / APPROVAL PENDING / DOCUMENT-ONLY / NO SCRIPT CHANGE`；
- 本包批准后只做证据对账、集合校验和文档一致性验证；若发现新source-confirmed gameplay差异，只登记独立
  后续Task/Change，不在R09中修改脚本；
- `R1-WP02` full trace继续BLOCKED，T8默认stage.dat暂缓，AI C++ parity、F1/F2调试步进与IL2CPP保持用户排除。

## 2026-08-24 — R8-WP01G-R09 approved and resumed

- 用户明确批准`R8-WP01G-R09`并恢复目标；
- 状态推进为`IN_PROGRESS / USER APPROVED / DOCUMENT-ONLY / NO SCRIPT CHANGE`；
- 当前开始逐项对账68个D-ID与R05～R08最新证据；不运行Unity/C++，不修改脚本、scene、config、资源或
  已批准的Unity适配边界。

## 2026-08-24 — R8-WP01G-R09 complete at approved Unity evidence

- 68项最终对账完成：43项Unity S4/runtime覆盖、5项exact witness不可得、1项source等价/full trace缺失、
  9项用户排除/未来替换、1项debug-key policy、3项approved adapter/config、6项test/worker/performance；
  合计68、missing0、extra0、duplicate0；
- `D-LIFE-001`与`D-RENDER-003`依据R08/R07B更新为Unity joint S4；F1/F2三项按用户决定退出normal-combat
  backlog但保留source difference；`R8-SPRITERANGE-001`已有七层证据并升级VERIFIED；
- R8父编排、all-diff register、synthesis、post-AI residual audit、总计划、Task和handoff已统一；
- 当前没有新的normal-combat、production-reachable、source-confirmed、Unity-unimplemented脚本差异；
- R09脚本/scene/config/resource/C++改动0，未运行Unity、Player、性能或C++ executable；
- R8只可宣称“批准范围和当前可取得Unity证据层完成”。R1-WP02 full trace仍BLOCKED，T8与其他边界继续保留。
- final verification：68/68、missing0、extra0、duplicate0；Change Ledger validator 93 records / 111 governed
  code files PASS；R09 scoped diff check PASS（仅既有LF→CRLF提示）。

## 2026-08-24 — R11/R12 acceptance and F7/F8/F9 closure authorized

- 用户授权执行仍有意义的验收测试，并新增按模式控制的F7/F8/F9；全部通过后允许结束当前目标；
- 新只读DAT盘点更正旧R09事实：C++与Unity正式DAT现有8个authored state8xxx frame，因此旧
  `authored state8000=0`证据已过期；`R8-WP01G-R11 / R8-AUTHOREDSTATE-PLAY-001`将补production full-tick Play；
- 正式state2000共38帧但没有type0角色，故不再等待不存在的type0样板；使用正式weapon/object样板验收fallback；
- OID999有效body frame399在正式producer/next/opoint不可达，CLR/current-DAT mismatch在C++统一Entity中不存在，
  两者继续以source+synthetic fixture裁决，不伪造production Play；
- `R8-WP01G-R12 / R8-FUNCTIONKEYMODE-001`已获用户授权并进入`IN_PROGRESS / PRE-CODE`：只实现F7/F8/F9，
  由GameConfig的gameModeId+battleGameModeId规则控制，仅LocalFreeRun物理捕获，tick边界消费；
- 当前活跃Change：`R8-AUTHOREDSTATE-PLAY-001`、`R8-FUNCTIONKEYMODE-001`；脚本尚未修改；
- R1-WP02 full trace仍BLOCKED，T8暂缓，F1/F2、AI C++ parity、Android、服务器、IL2CPP保持排除。

## 2026-08-24 — R11/R12 verified and approved goal closed

- `R8-AUTHOREDSTATE-PLAY-001 / VERIFIED`：R11 production Play于12:22:17 PASS。OID150 state2000
  正/负Vx朝向通过；OID32 state8032得到DAT32/frame0/offset140/effective pic140，主线程materialize后的
  Central body command/catalog/UV通过；cleanup恢复基线；production gameplay/DAT/C++零改动；
- `R8-FUNCTIONKEYMODE-001 / VERIFIED`：GameConfig exact 0/1白名单、LocalFreeRun-only physical edge latch、
  tick边界request、F7 postframe、F8/F9 Mode2复用以及checksum/parity/snapshot/restore均已落地；
- R12 production Play于12:26:06 PASS：F7 tick1581四项500，F8 tick1582生成9个，F9 tick1583清理
  7/7个tail时仍合格候选，2个已在此前转换类型；request与cleanup通过；
- focused jobs：`7c4e0d2675f74d12aacca145f75aa302` 4/4 PASS；
  `dca455601f2a4997be98eae4baaa7db8` 18/18 PASS；
- `Temp/NTSD_BattleRuntimeSelfCheck.result`于12:28:41为PASS；fresh compile无C# error；
- Change Ledger validator PASS（95 records / 122 governed code files），scoped `git diff --check` PASS；
- 当前没有活跃的正常战斗脚本Change；批准范围内目标完成。R1-WP02 full trace继续BLOCKED，T8、F1/F2、
  A→B→C、AI C++ parity、Android、服务器、IL2CPP继续排除，不能扩大为C++ executable full-trace认证。

## 2026-08-24 — CAMERA-PRESENTATION-REMOVE-001 verified

- 用户要求移除`BattleCameraSafeArea`的safe-area、viewport布局/视野、follow/边界和调试逻辑；
- 用户追加保留：背景 bounds 驱动的正交尺寸自适应；不改写相机位置；
- Change Record、Task与handoff已在脚本修改前建立；
- 当前只读审计确认该脚本没有外部脚本调用，但`NTSD_Battle`场景保留组件序列化引用；
- 最终实施仅保留同名组件及上述背景尺寸适配；不修改scene、`NTSDRenderSpace`、URP、战斗runtime或C++。

### 中间代码状态（已被用户范围修正取代）

- 脚本曾暂时收缩为无运行逻辑兼容标记；该状态不作为最终交付；
- 最终将保留背景自适应尺寸，其余safe area、viewport布局、follow/边界、camera offset与Editor/GUI/Gizmo仍删除；
- 最终最小代码只按background bounds与aspect更新`orthographicSize`，不改Transform；
- Unity fresh compile（14:44:27）、Scene组件解析、背景 bounds→相机尺寸数值、短Play bootstrap、filtered Console、Ledger validator和scoped diff均已通过；
- 当前Change=`CAMERA-PRESENTATION-REMOVE-001 / VERIFIED`。没有修改scene、`NTSDRenderSpace`、URP、战斗runtime或C++。

## 2026-08-24 — CAMERA-BACKGROUND-FITMODE-001 superseded

- 用户要求新增“背景全面覆盖视野”模式，解决当前相机完整显示宽背景时的上下镂空；
- 已在代码修改前建立 Change Record、Task 与 Handoff；
- 计划仅加入`ContainBackground`与`CoverViewport`的`orthographicSize`公式选择，默认`CoverViewport`；
- 它不拉伸背景，因而会裁切宽背景的左右边缘；不改Transform、viewport、安全区、follow、URP、scene或战斗runtime；
- 已写最小代码：私有`BackgroundFitMode`+序列化下拉字段，`Contain`选择`max`、`Cover`选择`min`；
- fresh compile（15:05:20）、当前场景`backgroundFitMode=CoverViewport`、背景→`orthographicSize=7.05703163`数值、Transform不变与短Play均已通过；
- 用户已确认Cover会裁切原始背景内容，不能作为交付；当前Change=`CAMERA-BACKGROUND-FITMODE-001 / SUPERSEDED`；
- 新Change=`CAMERA-BACKGROUND-NOCROP-FIT-001 / CODE_WRITTEN`已以“完整显示 / 全面覆盖但保留全部内容”的无裁切方案替代；
- 当前已写base-scale捕获/恢复与必要轴向伸缩；compile、无裁切审计、临时Play的Stretch→Contain→Stretch切换、bounds数值、Transform不变和bootstrap均通过；
- 用户新增架构约束：后续联机对战下不得由`BattleCameraSafeArea`写背景Transform/localScale；
- 当前Change=`CAMERA-BACKGROUND-NOCROP-FIT-001 / ROLLED_BACK`；compile、编辑器/Play背景`scale=(1,1,1)`、bootstrap与filtered Console均已通过；
- 无裁切全覆盖必须另行采用纯渲染层方案，本轮不实现。agent未保存scene；现有`NTSD_Battle.unity`的大范围用户diff不含本Change的mode/base-scale/stretch字段。
