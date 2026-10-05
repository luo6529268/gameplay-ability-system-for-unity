<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Core/BattleSpatialProjection.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationMotionSampler.cs
code-path: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationDisplayMotion.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs
code-path: Assets/NTSD/Scripts/Animation/BattleEntityOverlayRenderer.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28D024UnifiedSpatialProjectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePresentationMotionSamplerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5MultiBodyCandidateProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5StandardHitRestProductionIntegrationEditorTests.cs
authority: user D-024 all-entity screen-fraction decision with accepted 1.5 image-size exception; official 336B44 playable render/physics and natural original Battle Scene OID85 first difference
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001.md
-->

# D-024 共用全局 Y 投影与普通 Y 矩形

2026-10-05 R120命令必要出口后继证据：[原Scene单次验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/R120-SCENE-ACCEPTANCE.md)为32tick PASS/DONE，自然alpha0.0156545、source deltaY=-2；本体中间偏移3.156185px符合统一1152/730，Z不变时影子0偏移，全部规则samples与既有GREEN相同。只改原探针Record所有的Editor代码，本生产Record没有新增脚本/资源改动。Console0error、退出Scene clean/保护SHA稳、请求原字节恢复。此前“R120原Scene待”缩为实际GPU/设备120FPS等未证，普通C14命中六例复用；落地画面、原Scene空中命中按总表条件门，父Record仍RUNTIME_PENDING，不升级全面一致或安排重复矩阵。

2026-10-05 命中邻例最终限定通过：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/Y-HIT-RESPONSE.md)及原Editor job `966d8e3d84964d8dad3f3ba3dab55153` 的精确6/6 PASS，检查相交才扣10HP/HitCount1/CRT2，普通角色攻击者hold+3/受击者-3/arest4/vrest1；接触/分离均无副作用，两配置源Y不变。最终生成Editor0错/301warnings，MCP刷新导入后Console0error、Scene clean/nonPlay/SHA不变；validator0/PASSED，当前18个脚本diff均被Record覆盖。C040离地末态不能替代空中命中已在报告说明，错误停顿预期的失败原件保留。实际只新增已预声明的既有标准命中测试方法与专用位置助手，没有生产/DAT/图片/Scene或非战斗改动。这个增量为FOCUSED_TEST_PASS，不是完整Driver/原Scene/正式根空中同态；父Record保持RUNTIME_PENDING，R120原Scene和其他条件门仍开放。必要命中邻例ONE已经完成，不重复该6例。

2026-10-05 停顿断言纠正：追加断言的job `b9cc60e2fcbd426386c4ff058a1026dc` 为4/6 PASS，两个相交例均在攻击者停顿“expected -3 / actual +3”失败，原件完整保留。追当前正式 `battle_world.cpp` 第7044～7068行的实际调用者后确认：正值反转只在target type3分支调用 `release_native_attacker_motion_hold`，普通角色目标保留攻击者+3；第3939行孤立注释“default final -3”不能覆盖实际type分支。Unity+3符合正式规则，此失败是本测试预期错误，不是生产首差。已仅将攻击者预期改+3，受击者-3/arest4/vrest1不变；不修改生产或增加角色特判。待生成工程与原Editor同6例终验，父状态仍RUNTIME_PENDING。

2026-10-05 命中邻例首轮结果：首次生成Editor构建因测试误用 `NativeRandom.CrtCalls` 属性报2个CS1061，已改为既有 `CaptureScalarState().CrtCalls`，没有生产错误或修改。修正后生成工程0错/301warnings；原Editor经MCP强制刷新/域重载、Console0error、Scene clean，精确job `b5d4063ddab746fbb5fe8015b79e044c` 为6/6 PASS（原件 `original-editor-y-hit-response-result-20261005.json`）。随后按当前正式 `battle_world.cpp::apply_standard_hit_rest` 的3tick停顿、公共尾部反转和arest/vrest合同，在同一方法追加双方hold=-3、arest4、vrest1的相交断言及未命中零副作用；生成Editor再次0错/301warnings，该追加断言尚待原Editor最终6例。只重跑这个方法，不扩大suite。

2026-10-05 本轮脚本后：上述单方法及专用位置助手已写入既有标准命中测试类，6个参数实例复用原241帧/500HP夹具，实际先采集生产候选再跑C14提交；断言相交才10HP/一次HitCount/两次CRT、接触与分离无这些副作用，两实体源Y不变。不增加生产代码、场景或资源修改。生成Editor构建已启动但尚无终态，原Editor导入/精确测试尚未运行；本增量仅CODE_WRITTEN，父Record继续RUNTIME_PENDING。

2026-10-05 本轮脚本前范围增量：只在现有 `NTSD28B5StandardHitRestProductionIntegrationEditorTests.cs` 增加 `AirborneVerticalBoundaryPreservesCommittedDamageAcrossViewScale`，复用既有241帧/500HP角色夹具和生产 `CaptureCollisionFrameSnapshotsAll → CollectCollisionCandidatesAll → PostInteractionTickAll → EndCollisionCandidateConsumption`。两实体源Y均在地面上方，攻击者Y=-40，受击者Y=-11/-10/-9分别相交/接触/分离；恒等与2048×1152固定视野各三例，共6例。预期只有相交提交10伤害并写命中计数，接触/分离不扣血；源Y、HP/反应/停顿/RNG应跨视图配置保持，不直接调用DamageWriter绕过候选。权威依据为当前336B44 playable闭包 `hit_candidates.cpp` 的源Y/centery矩形、`collision_geometry.cpp` 的严格相交及 `battle_world.cpp` 的普通命中消费，构建参与性由现有build.ps1第71/72/79行确认。该用例是共用Y修改后的必要伤害提交邻例，非角色矩阵；C040同tick先命中后抓取的Y=-14末态不能替代它。只改本测试，不改生产、DAT、Scene、图片、非战斗。最窄验收为生成Editor0错、原Editor精确6例、Scene clean/hash保持；它不冒充正式EXE或原Battle Scene同输入空中命中证书。回滚只移除本新增测试与其专用助手，保留原测试、当前生产修复和用户工作。状态仍`RUNTIME_PENDING`，测试尚未写/运行。

本记录在任何生产/聚焦测试脚本改动**之前**建立。源证据、当前 Unity 混合坐标、完整受影响路径与符号、预期副作用、保留不变量、最窄验收和手工逆向回滚见 [Task](../TASKS/NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001.md) 及 [消费者审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/Y-DOMAIN-AUDIT.md)。当前状态仅 `PLANNED`；未修改所列脚本、未编译、未跑聚焦或 Play。不得把原 OID85 RED 证据当成本包的 GREEN。

2026-10-05 测试先行增量：已在既有 `NTSD28D024UnifiedSpatialProjectionEditorTests` 增加全局 Y 比率/本体 pivot 2 源像素正例，在 `BattlePresentationMotionSamplerEditorTests` 增加源 `lround` 后仅视图 Y 投影且影子不动的正例。生产尚未改；新断言因待新增 `SourceDeltaToViewY`、`VerticalScale`、`ViewY` 和三轴采样签名目前是预期 RED，尚未运行 Unity Test Runner。改前/改后职责仅涉及本包测试，全部受影响路径仍以前置元数据为准；无 Scene/DAT/图片改动。

RED 证据：`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 于 2026-10-05 退出1，301 warnings/6 errors；6项全部为上述新 API 尚未实现（`SourceDeltaToViewY` 2、`VerticalScale` 2、`ViewY` 1、9参 Sample 1），无其它新编译失败。接下来改生产后重跑；此阶段不称编译通过。

2026-10-05 生产增量：`BattleSpatialProjection` 暴露 Y 距离投影（与视口高比的 Z 倍率同源，默认1）；`LF2ObjectRenderer` 的 Legacy 共用 pivot 与 state9997 owner 全局 Y、`BattlePresentationShadowBuild` 的中央本体/血条/owner 及 frozen frame 的 Projection 快照消费该出口；`BattlePresentationMotionSampler/DisplayMotion` 保留源 Y lround，新增 viewY 且身体插值用 viewY+viewZ，地面仍只用 viewZ，三处生产调用者传同一个 World 高度比例；`BruteForceSceneQuery` 的普通 bdy/itr 源整数端点在 full-height 之后各投影一次，源平台和上一帧 Y 不变。未改 DAT、Scene、图片尺寸、规则 Y/Vy、OPoint、RNG、非战斗代码；kind0 火花和 WORDS 仍按 Task 留在后续出口。生成 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 修复后退出0，334 warnings/0 errors；原 Editor 导入、聚焦 Test Runner、原 Scene Play、落地/命中邻例和退出零残留尚未运行，Record 仅 `COMPILE_PASS`。若出现规则差异，只按本包 diff 逆向回退并保留用户工作。

原 Editor MCP `refresh_unity(force/all/compile=request)` 后 Console 0 error、Battle Scene clean；用精确 `testNames` 跑两个新增投影/插值测试，`get_test_job(931538c555074604970def79d9713154)` 返回 EditMode 2/2 PASS、0 fail。随后在同一既有 D-024 聚焦测试类补普通 `BattleVolume` 两端投影及 full-height sentinel 用例，生成 Editor 工程再次 301 warnings/0 errors；该新增第三例尚待原 Editor 刷新和定向测试。未启动全套 suite，尚未运行 Play。

2026-10-05 有限 GREEN 更新：[验收报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/ACCEPTANCE.md)及[改后唯一原 Scene JSON](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-green-20261005-01.json)。新增第三条原 Editor 精确 `testNames` job `7100f1e72947440b8623ab0a45f039e5` 为1/1 PASS，合计三条聚焦断言3/3；原 Editor Console 0 error。改前/改后自然 OID85 32个规则样本逐项相同，源Y -22→-20同action/Z的本体相对阴影画面移动由1.9999504变为3.1561852视图px，对应目标2×1152/730=3.1561644px。Play `PASS/DONE`、退出非Play/Scene clean、Scene SHA前后`253B2EBA...8F9010`；临时请求经[文件操作记录](../FILE-OPERATIONS/NTSD28-336B44-Q07-D024-SCENE-REQUEST-20261005-001/RECORD.md)逐字节恢复。实际修改文件仅元数据 `code-path` 声明的11个脚本；未改 DAT/图片/Scene。状态 `RUNTIME_PENDING`，因为实际着地、真实 Y 向命中、R120 原 Scene 中间 alpha、kind0 火花仍待；不能宣称 D-024/Q07 已完成。回滚仍限本 Record 代码 diff，用户其它未提交内容保持。

实施边界：共用 `BattleSpatialProjection.SourceDeltaToViewY` 只在实际视图/碰撞矩形出口消费；源 Y/Vy/floor/OPoint/RNG、DAT、图片尺寸、相机/背景、Scene、非战斗框架均保持。正式命中火花仍须另行闭合，不以本包替代；若本包的局部改动无法维持同态，应停在真实证据状态并保留回滚所需原始 diff。

2026-10-05 本轮脚本前范围补充：增加现有 `NTSD28B5MultiBodyCandidateProductionEditorTests.cs` 一处真实候选用例，仅复用其生产 `SimulationWorld`/`BruteForceSceneQuery` 夹具比较 Y 相交、接触、分离在 identity 和 2048×1152 固定视口下的候选个数；不修改既有 X/Z 夹具语义或生产代码。预期副作用只有测试覆盖，最窄验收是生成 Editor 编译及该精确 `testNames` 在原 Editor 通过。若出现 first-difference，再在本 Record 已声明的 `BruteForceSceneQuery.cs` 范围内最小修复并留 RED/GREEN；回滚只移除新增用例，保留其他已通过实现和所有用户工作。

脚本后增量：`NTSD28B5MultiBodyCandidateProductionEditorTests.VerticalBodyBoundaryKeepsSourceCandidateResultsAcrossViewScale` 已增加 2 视口×3 源 Y 边界的一个参数化测试；共用原 production fixture，分别读取直接查询和 `ForceRoleAware` 收集结果，检查相交1、接触0、分离0。未修改生产、Scene、DAT、图片或其他测试。生成 Editor 编译及原 Editor 精确测试尚待验证，状态维持 `RUNTIME_PENDING`。

验证增量：`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly` 退出0（301 warnings/0 errors）；原 Editor MCP refresh 后 Console 0 error、Battle Scene clean/non-Play，精确 EditMode job `4c7b0ca838de4a4c8ed3939629255fa0` 的两个参数实例 2/2 PASS/0 fail。该增量无生产改动，只把直接/收集两条候选路径的 Y 邻接证据补齐；真实命中后续和其它本 Task 限制不变，Record 继续 `RUNTIME_PENDING`。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/ACCEPTANCE.md)。

复用现有普通角色负 floor 落地聚焦 `NTSD28B4Type0OrdinaryLandingEditorTests.ExactCharacterNegativeFloorLandingUsesFrameHitG`，原 Editor 精确 job `c5e986e6501a412eb26f7685dc28e0e0` 1/1 PASS；这是无固定视口的源规则邻例，不冒充原 Scene 落地画面或新版正式根全字段配对。[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/original-editor-landing-neighbor-20261005.json)。

审计：PowerShell 5.1 直接运行 validator 的默认根路径参数为空，显式根路径后又因 Git LF→CRLF stderr warning 被 `$ErrorActionPreference=Stop` 拦截；改用可用的 `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <本仓库绝对路径>` 后退出0、`PASSED`，本轮新增测试路径由本 Record 覆盖。`git diff --check` 退出0（只有 Git 换行提示）。原 Editor 末次为 Battle Scene clean、idle、非Play、Console 0 error。未运行全套测试或新增原 Scene Play。
2026-10-05 后续出口更正：上方 `kind0 火花仍待` 是本 Record 完成时的快照；正式可达自然 C040 第25 tick 的活跃 kind0 火花混合坐标已由独立 [SPARK-VIEW-ANCHOR-001](NTSD28-336B44-Q07-D024-SPARK-VIEW-ANCHOR-001.md) 限定修复并通过原 Scene。此 Record 仍为 `RUNTIME_PENDING`，剩余实际着地、Y 向命中响应及 R120 原 Scene 中间 alpha 等条件门，未改变本 Record 的 11 脚本范围或既有验证原件。

2026-10-05 落地出口更正：[只读复用报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/LANDING-REUSE.md)及[逐字段 JSON](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/landing-reuse-comparison-20261005.json)确认，修复后既有原 Scene 32tick已包含相对27的自然落地 Y0/Vy0/action215和相对28后续action213。源/Unity1690/1690、正式根与Unity所选1658/1658同值，修前/后完整samples相同；LFR独立CRT seed初态差异32项明确排除并保留。原报告“32tick不包含落地后规则状态”是文档遗漏，予以更正，不新增脚本或Play。只关闭本样例的落地规则邻例，落地画面命令/GPU、Y向真实命中及R120原Scene中间alpha仍未验；Record仍`RUNTIME_PENDING`，后续按总表实际首差条件门，不重复已覆盖的落地规则。用户保留1.5倍战斗实体显示尺寸。
