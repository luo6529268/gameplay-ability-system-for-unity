# 接手文档 — NTSD C# → Unity 战斗逻辑对齐（Codex 无缝接手版）

## BATTLE-RENDER-PLAN1 集中式战斗渲染系统方案交接（更新于 2026-07-20）

方案入口：[central-battle-render-system-plan.md](central-battle-render-system-plan.md)。当前状态为 **方案已确认 / R1-R2C-4 runtime 容量阶段与 B0 shadow Loose Quadtree 已实施并验证 / 其余未实施**。

- **已落地**：`BattleRuntimeProfile` / `BattleRuntimeProfileResolver`；生产解析顺序为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认。平台宏只负责默认值：Editor/其他平台为 `Authority400`、Android Player 为 `MobileExtended`、Standalone Player 为 `DesktopExtended`。Unity 条件编译符号不进入战斗 pass；后续设备能力检测只允许选择或降级渲染后端。
- **已接线**：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析/创建路径；直接 `BattleTestBootstrap` 在实体注册前协调晚到的 GameConfig。`Authority400` 使用 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused`；Mobile total active admission 与 Desktop 自动分页增长已接入，Desktop 增长保留最低空洞并同步 AI snapshot。
- **fresh 验证**：相关源码 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` `12:05:07` **PASS**；100,000 次随机 claim/release/allocate 与朴素扫描模型逐步对照 **PASS**；架构复核 **PASS**。
- **R2A 已落地 / 已验证**：独立 `RuntimeSlotTable` 固定 256 槽/页并按需物化；`Authority400` 的 400 逻辑地址、`MobileExtended` 设计所需的 1050 逻辑地址及尾页 guard、每槽独立 raw runtime/rest、`ClaimedCount` 与 `(slot, generation)` 句柄契约均有 focused self-check。release、同槽 reuse 与 reset 后旧句柄均失效。
- **R2A fresh 验证**：相关源码 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` `12:36:53` **PASS**；架构复核 **PASS**。
- **R2B 已落地 / 已验证**：生产 `Authority400` registry 已由单一 `RuntimeSlotTable` 替换 used/raw runtime/raw rest 并行数组；slot 当前 occupant 为 O(1) 查询。live ascending scan 保留 high-newborn / low-reuse 时序；release 以 `expectedEntity`/当前 occupant 防止旧实体释放复用槽；stage/ordinary raw rest 语义、`ObjectCount`、buckets 与 `SceneQueryHit` slot-address 契约保持不变。
- **R2B fresh 验证**：生产源码 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` `12:57:02` **PASS**；fresh `dotnet build` **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索 **0**。
- **R2C 已落地 / 已验证**：`RuntimeSlotAllocator.GrowTo` 与 `RuntimeSlotTable.GrowTo` 只允许单调增长；增长保留 dynamic min-heap、`nextUnused`、claims、既有 pages、occupants、generation handles、raw runtime/rest，并优先复用旧低槽空洞。等容量调用为成功 no-op；缩容拒绝且原状态不变。
- **移动端地址契约修正**：`1000 active` 是 admission 预算，不是最大 slot address。保留 `0..49` 后，1000 个动态槽为 `50..1049`，故逻辑地址容量为 `1050`；`PageSize=256` 时物理需要 5 页，但 `1050..1279` 尾部地址必须不可访问、不可 claim、不可创建 raw runtime。
- **R2C fresh 验证**：相关源码 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` `13:25:34` **PASS**；fresh `dotnet build` **0 errors**；架构复核 **PASS**。
- **R2C-3A 已落地 / 已验证**：`SimulationWorld.RuntimeSlotCapacity` 读取当前 `_runtimeSlots.LogicalCapacity`；registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量循环已改为实例容量。默认 `SimulationWorld()` 仍创建 `Authority400/400`。
- **R2C-3A focused 契约**：internal `DesktopExtended/512` world 仅用于代码层验证；slot `511` 可注册、查询并进入 AI 目标扫描，slot `512` 被拒绝，reset 后高槽被清理。`BattleParitySnapshot` 继续固定 400-slot authority schema。
- **R2C-3A fresh 验证**：相关源码约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` `13:54:22` **PASS**；fresh `dotnet build` **0 errors / 42 warnings**。
- **R2C-3B 已落地 / 已验证**：`LF2SpecialAttack` 的高槽 holder 验证和 Karasu oid209 扫描读取当前 world capacity；`LF2Entity` transition effect 统计当前 dynamic range，不再固定 `50..399`。
- **parity capture guard**：历史 capture 必须同时满足 `Authority400` Profile 与 400 逻辑容量；`DesktopExtended/512`、`DesktopExtended/400` 都被拒绝，现有 400-slot schema 不能用于非 authority Profile。
- **R2C-3B fresh 验证**：相关源码 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors**，warnings 为既有告警。
- **R2C-4 Profile 优先级**：命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认。默认容量为 `Authority400=400`、`MobileExtended=1050 logical / TOTAL active admission 1000`（跨全部槽区）、`DesktopExtended=512 initial`（按 256-slot 页规范化并自动增长）。
- **R2C-4 生产接线**：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析/创建路径；直接 `BattleTestBootstrap` 在实体注册前协调晚到的 GameConfig。Desktop 增长保留最低空洞优先并同步 AI snapshot。
- **R2C-4 checksum 边界**：Extended Driver checksum 跳过/为空；direct parity capture 继续严格拒绝非 `Authority400/400`，Extended replay/checksum schema 尚未实施。
- **R2C-4 fresh 验证**：相关源码 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors / 42 existing warnings**；architect final review **PASS**。
- **B0 shadow Loose Quadtree 已落地 / 已验证**：纯数据 X/Z half-open tree，`looseness=1.5`、`leafCapacity=16`、`maxDepth=8`；每次 collision collect 全量重建，诊断默认关闭。比较 brute AABB pair、tree pair 与 accepted subset，正式 `i/j`、VRest、RNG、candidate flow 不变。
- **B0 fresh 验证**：相关源码不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` `16:15:43` **PASS**；fresh `dotnet build` **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。
- **明确未实施/未启用**：即时 weapon query、AI 查询、VRest 解耦、Loose Quadtree 增量更新、正式 broadphase switch、Extended replay/checksum schema，以及图集、`Texture2DArray`、动态 Mesh、Shader、透明排序、URP Pass 等集中式渲染模块均未实施。B0 不代表性能提升。
- **未验收边界**：本批没有 Play Mode、目标 Android 真机或像素级验收；不能将 B0 shadow PASS 扩大为正式 broadphase、四叉树性能收益、VRest、Extended replay/checksum 或完整渲染方案完成。现有 400-slot parity schema 仍只适用于严格的 `Authority400/400`；本计划不包含 T8。

## BATTLE-AUDIT14 DAT movement 显式值读取回归交接（2026-07-19）

- **最新覆盖结论**：下方 BATTLE-AUDIT13 的“可玩 Naruto `oid2 running_speed=8`”与“`BattleVisualScale=1`”已经失效。生产 Naruto DAT 显式配置为 `running_speed=15`；Unity 实体表现缩放已恢复为项目要求的 `1.5`。
- **回归根因**：DATA-01A 把 `LF2CharacterData` 的兜底值从旧 `15` 改为 C# 权威默认 `8` 本身正确，但 Unity parser 未读取 `<bmp_begin>` 内无冒号的 movement `key value`，使生产显式 `15` 回退到 `8`。这是 Unity loader bug 和对齐回归；此前将慢速主要归因于缩放并不完整。
- **生产修复**：`Lf2DatParserV2` 仅对白名单中的 BMP 顶层 18 个 movement 键接受无冒号 `key value`；`ExtractMovementParameters` 现读取 `Bmp.Properties`；浮点字段和 `frame_rate` 均以 `InvariantCulture` 解析。DAT 缺字段时仍保留 C# 默认 `8`，没有恢复错误的 Unity 默认 `15`。
- **测试矩阵**：生产 DAT 覆盖 Naruto `15`、Kakashi `18`、Sakura `17`、Sasuke `23.9`、clone `15`，并保留 weapon4 冒号语法 guard；synthetic 覆盖全部 18 键、last-wins、frame 隔离与缺省 `8`。
- **同类风险审计**：已审计当前 101 份 DAT。除上述 5 份角色 DAT 的 18 个 movement 字段外，没有第二组当前生产数据触发同类遗漏；weapon/frame/stage/data 当前安全。多词 `name` 是非战斗的潜在表示风险；`catchingact/caughtact` 双值为未来风险，但当前 218 处两值均相等，当前无可观察战斗差异。
- **fresh 验证**：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 14:39:43.992`，晚于相关源码，Console C# error 为 **0**。一次请求因 Editor 误留 Play Mode 未计入结果；退出后 fresh full `BattleRuntimeSelfCheck` 于 `14:44:58.748` 返回 **PASS**。
- **未验收边界**：真实双击 D Play trace 因 UnityMCP 临时注入卡住而未完成，本轮不宣称 Play Mode 通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT13 Naruto 防下攻与跑速缩放交接（2026-07-19）

- 常规战斗逻辑的唯一权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。本项是用户明确指定的例外：Naruto 防下攻以用户已验证表现正确的 `J:\QQFile\NTSD2.4\ntsd_release` C++ 版本作定向参考；该例外不扩展到其他战斗逻辑。
- C++ 行为依据：`oid2 frame286` 的 `centery=79`，opoint 为 `y=80 action=240 dvy=0 oid=33`，child 初始为 `Y=+1, Vy=0`。角色物理落地要求 `new_y > 0.0001 && pre-move Vy > 0.0001`，所以不会立即进入 frame219；后续链为 `240 -> 241 -> 242 -> 243 -> 235 -> 236(dvy=-7) -> 244..247`，真实下降落地后才进入 `219 / AI`。
- Unity 根因与修复：`CharacterMechanics` 的 `landed` 判定缺少 `Vy` 门槛；旧 `LateOpoint + state15` 专项 gate 过宽，并且仍会把 `Y` 钳为 0。现已改为通用 `landed` 条件并移除专项 gate；`CheckLateOpointState15LandingControls` 与 `PH-02` 三向速度矩阵已同步更新。
- 跑速测试状态：按用户要求，`BattleVisualScale` 临时由 `1.5` 改为 `1`，供用户复测奔跑速度体感。可玩 Naruto `oid2` 的逻辑 `running_speed` 仍为 `8`，固定逻辑频率仍为 30 Hz，本轮没有修改逻辑跑速。

fresh 验证链：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 03:21:41.985`，晚于测试时间 `03:20:06.169`；Console C# error 为 **0**；fresh full `BattleRuntimeSelfCheck` result 时间 `03:22:49.668`，结果 **PASS**。本轮没有可复用的真实 Play 自动 trace 入口，因此没有重新运行真实 Play trace；防下攻与 scale 1 奔跑仍需用户手测，当前不宣称 Play Mode 验收通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT12 代码差异修复与 fresh 验证（2026-07-18）

本段是当前交接状态，并覆盖下方 BATTLE-AUDIT9/10/11 的历史冻结措辞。用户负责 4 组 Naruto/武器 Play Mode 场景，本轮不运行也不代替其验收。最新 freshness：相关源码最晚 `BattleRuntimeSelfCheck.cs` `16:44:31.210` < Unity `Assembly-CSharp.dll` `16:45:52.868` < self-check result `16:46:29.080` **PASS**；fresh `dotnet build` 为 **0 errors / 18 warnings**。

- `FW-FLOW-01`：已恢复普通 tick 的 cooldown-before-human-input 顺序，focused check 与 full self-check 通过。
- `LP-03`：typed/generic formal throw 已移除 `Zz=1` 额外层级，release 矩阵通过。
- `LP-05`：formal release、consume、force-clear 的 `TargetIdx/HolderIdx/HeldWeaponSlot/HolderCopy` 写入边界已按 authority 分开，typed/generic 矩阵通过。
- `FW-RESULT-01`：固定 roster slot、inactive/dormant 与 alive/team bucket 矩阵已补齐并通过。
- `UNRES.04`、`DATA-01A-D`：生产修复和对应断言均进入本次 fresh full PASS；此前 transformed landing 阻塞已由 authored-frame gate 修复消除。
- `FW-FLOW-02`：Unity 生产无 writer，authority 仅 Host debug/step 控制，归 dormant/scope-excluded。
- `FW-BOOT-01/02`：旧表误把 rematch-only 写入及普通 reset 后偶合等价字段记成正式差异；普通非-rematch 路径关闭为 equivalent，result rematch 保持 scope-excluded。
- `FW-RESET-01/DEP.RNG.01`：保留 per-world lockstep RNG adapter；算法等价，不迁移为进程静态 owner。

当前 code-only 清单没有未修复的 confirmed item；但这只关闭脚本差异与 self-check 层，不是 Play Mode 结论，也不是完整逐帧 production certificate。T8 默认 `stage.dat` 部署继续暂缓，raw DAT 表示差异继续排除。

## BATTLE-AUDIT11 代码层 12 项待确认项已全部定性（2026-07-18）

本轮只核验脚本/代码层，不进行 Play Mode、资源部署、DAT 文件表示或场景/表现确认；核验后的 Unity 代码修复已落地，但最新 fresh Unity full self-check 仍为 **FAIL**。2026-07-18 最新 fresh run 的 `CheckStateTransformLandingMatrix` transformed landing fixture 断言失败，实际为 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`；这是既有代码契约回归，不是 Play Mode 结论。最终依据为：

- `.omc/research/final-verify-unres-02-05-code-parity-20260718.md`
- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

分类汇总：

- **equivalent / Unity-adapter**：`UNRES.01/02/03/05`、`DEP.INT.01-04`、`DEP.WORLD.01`。
- **confirmed code difference**：`UNRES.04`、`DATA-01A`（`running_speed` 默认值）、`DATA-01B`（frame index 容量）、`DATA-01C`（合法缺帧语义）、`DATA-01D`（cpoint front/back action alias）。首轮修复已落地，但 fresh full self-check 仍被 transformed landing fixture 回归阻塞。
- **Unity-adapter / policy-open**：`DEP.RNG.01`（LCG 算法等价；owner/reset 边界保留为 Unity lockstep 策略待定）。
- **关联确认代码差异**：`FW-RESULT-01`（非正常 roster/lifecycle 下 dormant/inactive 选择与 relation identity alias）。
- `DATA-01E` 为当前 consumer 已屏蔽的 adapter/masked，`DATA-01F` 为 schema-only omission，`DATA-01G` 已在源码闭合，不计为正式 runtime 差异。

在本轮 **code-only scope** 下，原先剩余的 4 个 `authority-unresolved`（`UNRES.02`-`UNRES.05`）现已全部定性，数量为 **0**。这不是修复完成声明：`FW-RESULT-01` 仍是确认差异，且 `UNRES.04`/`DATA-01A-D` 的 fresh full self-check 被 transformed landing fixture 回归阻塞；4 组 Play Mode 场景仍由用户自行验证，本轮不改变 LP 或 Play 验证状态。

## BATTLE-AUDIT10 代码核验结果（2026-07-18）

本轮只处理代码层面的待确认项，不进行 Play Mode、资源部署或场景/表现验证，也未修改任何生产代码。核验报告：

- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

结论与交接状态：

- 已闭合为 **equivalent / Unity-adapter**：`UNRES.01`、`UNRES.02`、`UNRES.03`、`UNRES.05`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01`。
- 已升级为 **confirmed code difference**：`UNRES.04`、`DATA-01A`/`DATA-01B`/`DATA-01C`/`DATA-01D`（DAT parser/runtime contract）；首轮修复已落地，但最新 full self-check 仍被 `CheckStateTransformLandingMatrix` transformed landing fixture 回归阻塞（实际 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`）。
- `DEP.RNG.01` 为 **Unity-adapter / policy-open**（算法等价，owner/reset 边界待策略决定）；`FW-RESULT-01` 仍为未修复的确认差异（非正常 roster/lifecycle 的结果 slot/relation identity）。
- `DATA-01E` 为 **Unity-adapter / masked**，`DATA-01F` 为 **schema-only omission**，`DATA-01G` 为 **closed in source**，不作为正式 runtime 差异计数。
- **BATTLE-AUDIT10 历史中间快照**曾保持 `UNRES.02`-`UNRES.05` 为 authority-unresolved；该状态已被 BATTLE-AUDIT11 取代，当前 code-only scope 下 02/03/05 为 equivalent，04 为 confirmed difference。

**本段为 BATTLE-AUDIT10 历史中间快照，已被 BATTLE-AUDIT11 取代。** 当时统计为剩余 authority-unresolved 4 项（`UNRES.02`-`UNRES.05`）；当前 code-only scope 下已全部定性为 0。BATTLE-AUDIT9 的 LP 项状态和计数保持不变，4 组 Play Mode 场景仍由用户自行验证，本轮不对其下结论。以上仅是代码核验，不是完整战斗逻辑对齐声明。

## BATTLE-AUDIT9 差异盘点冻结（2026-07-18）

当前执行口径已切换为“先完成差异盘点，再按文档集中修复”。本轮只读合并以下报告，**没有按冻结清单修改生产代码**：

- `.omc/research/full-diff-inventory-framework-20260718.md`
- `.omc/research/full-diff-inventory-input-interaction-20260718.md`
- `.omc/research/full-diff-inventory-lifecycle-presentation-20260718.md`
- `.omc/research/reaudit-open-differences-20260718.md`

冻结计数（**BATTLE-AUDIT9 历史快照，已由 BATTLE-AUDIT11 取代**）：9 个正式 runtime 差异、1 个工具/trace 差异、12 个 authority-unresolved 待确认项、4 个 Play Mode 未验证场景。原 12 项现已在 code-only scope 下全部定性；正式差异表保留作历史追踪。

| ID | 权威 C# | Unity 对应 | 触发与预期/实际 | 证据/分类 |
|---|---|---|---|---|
| `FW-FLOW-01` | `BattleCore/Simulation/GameTick.cs:53-67` cooldown/step gate 在 input 前 | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:32-43`、`RunFrameAdvancePhase` | ARest/AttackExempt 与输入边沿同 tick 到期；应先递减再读，Unity 先读 | 静态 confirmed-difference，未修复 |
| `FW-FLOW-02` | `GameTick.cs:56-67` `BattleStepGate44905C` mode=2 转换与抑制 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:272-281`、`NTSDBattleTickSystem.RunReleaseTick` | 单步/慢速模式；应 gate，Unity 无转换/抑制 | 静态 confirmed-difference，生产可达性待确认 |
| `FW-BOOT-01` | `DirectBattleBootstrap.cs:138-140` 写 `Unk344`/`HolderCopy` | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 未显式写两字段 | 初始玩家统计/holder 分支；应 team/slot，实际可能 `0/99` | 静态 confirmed-difference，未修复 |
| `FW-BOOT-02` | `DirectBattleBootstrap.InitializeBattleStats:224-244` 完整 difficulty/HP/PP/respawn/Cd/edge | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 依赖隐式初始化 | 非默认 difficulty、DAT Hp3、复用；应完整字段契约，实际缺显式写入 | 静态 confirmed-difference，未修复 |
| `FW-RESET-01` | `SimulationWorld.Passes.cs:13-70` reset 不播 RNG | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:138-151` reset 播 `0x4E545344` 后再播 config seed | 连续重开/重赛随机序列；应遵循权威播种边界，实际 Unity 增加边界 | 静态 confirmed-difference，播种归属待确认 |
| `LP-01` | `BattleCore/Interaction/WeaponRuntime.cs:169-212,287-303` generic held throw/kind3 写 `ReleaseTick` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` generic throw/kind3 通过 `ClearLinks(..., stampReleaseTick: true)` 写当前 tick | generic DAT held 正式释放；应清 link并写当前 tick | confirmed-difference；**代码已写 / `CheckAudit9GenericHeldReleaseTickContracts` self-check verified / Play-unverified** |
| `LP-02` | `src/Host/SdlBattleRenderer.cs:476-497` 同 Z 按 slot 稳定排序 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` `ComposeRenderSortingOrder`、`Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` `UpdatePosition`；`LF2Sprite.cs` 表现刷新 | 同 Z 实体；应 slot tie-break，Unity 已写入 slot tie-break 排序值，并经 `CheckRenderSortingSlotTieBreak` fresh self-check PASS | confirmed-difference；**代码已写 / self-check verified / Play-unverified** |
| `LP-03` | `BattleCore/Interaction/WeaponRuntime.cs:169-212` 释放不写额外 Zz | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:77-98,391-402` 写 `Zz=1` | 正式投掷；应由 Z/slot 决定，实际额外抬层 | 静态 confirmed-difference，未 Play |
| `LP-04` | `src/Host/SdlBattleRenderer.cs:519-548` 实体/阴影按负 HitStop 阈值与四拍相位隐藏 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:416-448`、`LF2ObjectRenderer.cs:206-243` 已接入 gate | 负 HitStop 闪烁区间；应按实体/阴影各自阈值与四拍相位隐藏 | confirmed-difference；**代码已写 / `CheckHitStopPresentationGates` self-check verified / Play-unverified** |

### BATTLE-AUDIT9 修复进度（LP-01 / LP-04）

Fresh verification: `Assembly-CSharp.dll` `2026-07-18 14:01:27.540`; full `BattleRuntimeSelfCheck` `2026-07-18 14:01:51.078` returned **PASS**.

冻结后仅 `LP-01`、`LP-04` 更新为“**代码已写 / self-check verified / Play-unverified**”，其余冻结状态和计数不变，整个差异清单仍保持开放。`LP-01` 已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` 的 `ThrowHeldObject`、`DropRandomly`、`ClearLinks(..., stampReleaseTick: true)` 补齐 generic held `ReleaseTick`，由 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs:3062` `CheckAudit9GenericHeldReleaseTickContracts` 覆盖；`LP-04` 已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs:206-243` 的 `UpdateSprite`、`ShouldDrawEntityForHitStop`、`ShouldDrawShadowForHitStop` 接入表现门控，由 `BattleRuntimeSelfCheck.cs:1394` `CheckHitStopPresentationGates` 覆盖。

验证证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 warnings**；`Assembly-CSharp.dll` `13:43:59.791` 晚于本轮最新相关源码，fresh Unity full `BattleRuntimeSelfCheck` 于 `2026-07-18 13:44:26.093` 返回 **PASS**。两项仍需 Play Mode 定向验证：generic held 的实际投掷/掉落，以及负 `HitStop` 下实体与阴影的阈值隐藏和四拍闪烁。

`LP-05`（新增 reviewer 候选，只记录、不修复）：权威 `BattleCore/Interaction/WeaponRuntime.cs:289-295` `ReleaseHeldWeaponRuntime` 不清 `holder.TargetIdx`/`held.HolderIdx`；Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs:23-28,39-59` 正式 release 当前清 holder `TargetSlotIndex` 与 held `HolderStableId=-1`，generic `ClearLinks` 也有同类清理。分类保持 **confirmed-candidate / 未修复 / 需 authority 调用链与 Play Mode 复核**，不纳入 `LP-01` 已写结论，也不改变冻结计数。

`RT.CHECK.01` 是 `CharacterSync.cs:796-877,173-317` 内部 snapshot 与 `BattleParitySnapshot.cs:385-542` trace projection 的 schema/alias 差异，分类为 **validator adapter**，不是 runtime 语义差异（见 `reaudit-open-differences-20260718.md:44-56`）。12 个 unresolved 只保持待确认，不得猜测为等价。四个 Play 未验证场景的详细输入与预期见主文档 BATTLE-AUDIT9 详细冻结表：Naruto 防下跳六分身、防前跳螺旋丸、奔跑防跳后续招、投掷武器首击/持续命中。

F1-F7 仅 static/focused self-check 闭合，不能替代 Play Mode；DAT 表示差异不处理，T8 默认 `stage.dat` 部署继续暂缓，fixed-world camera 为批准的 Unity adapter。修复阶段必须从本冻结表开始，逐项取得编译、self-check 和必要 Play Mode 证据后再更新状态。

## BATTLE-AUDIT8 当前交接（2026-07-18，继续开放）

- fresh Unity full `BattleRuntimeSelfCheck` 已于 `2026-07-18 12:46:40.638` 返回 **PASS**；freshness 为 test source `12:45:10.110` < `Assembly-CSharp.dll` `12:46:15.927` < result `12:46:40.638`。
- F6/R1 的生产修复位于 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`：`UpdateLocalInputStateFromControllerBuffer` 先 `SyncFromRuntime`，再轮询 controller buffer；权威对照为 `BattleCore/Input/InputRuntime.cs` 的 human poll/cooldown runtime 真值及 `BattleCore/Simulation/GameTick.cs` 的 results early return。
- `BattleRuntimeSelfCheck.cs` 的 `TemporaryAppManagerRuntimeScope` 只修复 EditMode 下测试 fixture 的 AppManager singleton/Awake 生命周期；生产 `Assets/NTSD/Scripts/App/AppManager.cs` **未修改**。
- Frame/Input 已完成 **237/237** 分类：39 equivalent、181 Unity-adapter、4 confirmed-difference、1 missing、12 authority-unresolved。完整账见 `.omc/research/unity-frame-input-mapping-complete-20260718.md`；`FLOW.05`、`FLOW.09`、`IN.CD.02`、`RT.CHECK.01`、`RT.LINKS.01 / ReleaseTick` 及 12 个 unresolved 在按最新生产代码重新核销前仍开放。
- 该 PASS 只覆盖当前 self-check，不等于全战斗逐帧最终完成；后续仍需静态重审、必要 Play Mode/双端 trace 和最终独立复核。DAT 表示差异不处理；T8 默认 `stage.dat` 部署继续暂缓。

> 生成：2026-07-13 ｜ 供 Codex 或任何接手者直接开工，无需追溯历史会话。
>
> **当前状态（BATTLE-AUDIT7，2026-07-18）**：旧的“完整对齐/无剩余差异”推断已撤销。重新按唯一权威 C# 做完整框架正向映射和 Unity-only 反向审计后，确认 **13 个去重开放根因**：**12 个战斗 runtime/语义差异 + 1 个 trace 投影工具差异**，均为“已确认 / 未修复 / 未运行时验证”。Audit5 的 74/74 与原 15/15 仅表示历史批次已关闭；旧 `01:07:52.834 PASS` 和 Architect `P0/P1/P2=0` 不覆盖本轮新发现。

## 0. 你要做什么

把 **NTSD C# 战斗核心** 里 Unity 尚未对齐的战斗逻辑，逐条补齐 / 修正到 Unity 工程。
T0-T9、Audit2、Audit3、Audit4、Audit5 和 Audit6 只保留为历史实现/定向回归基线。Audit5 的 **74/74** 与原 trace 风险 **15/15** 仍是对应历史批次已关闭的记录，但不能覆盖 BATTLE-AUDIT7 的 13 个新开放根因，也不能作为当前完整对齐证明。C# 与 Unity 的 raw DAT/manifest 差异属于 Unity 适配预期，不是待处理项；T8 默认 `stage.dat` 继续暂缓。

- **唯一 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release_C#`；核心战斗入口位于 `src\BattleCore`。旧工程、反汇编及历史对齐结论不得作为当前实现或验收依据。
- **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
- **完整差异清单（配套读）**：`Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
  - 本文是「行动版」，那份是「全量核实版」。当前状态优先回查其顶部 BATTLE-AUDIT7 章节。

## BATTLE-AUDIT7 全量权威映射交接（2026-07-18，开放）

### 覆盖与结论

- Framework：权威 **172/172 ID** 已映射；独立复核为 13 difference ID，去重为 **7 个 framework 根因**。反向 Unity-only 扫描没有发现这 7 项之外的新 framework 根因。
- Frame/input/physics/runtime：**[历史 BA7 快照，已由 BATTLE-AUDIT8 237/237 分类取代；当前开放项见顶部]**：旧记录曾记权威 **237/237 ID** 集合定位、4 difference ID + 1 missing ID，并注明其余 219 ID 尚未逐项拆分；该旧静态边界不再作为当前状态依据。
- Interaction：权威 **105/105 ID** 集合相等，但独立复核确认 **2 个正式可达差异**；原 0 difference 结论失效。
- 总账：framework 7 + frame/input 新增 3（Results 去重）+ interaction 2 = **12 个战斗 runtime/语义根因**；另有 **1 个 trace 工具根因**，合计 **13**。
- Frame/input 权威 ledger 有两处账本校正：字段组机械相加为 138，而 footer 写 137；`IN.JUMP.03` 曾误写权威成功 jump 清 8 Cd，实际权威与 Unity 都只清 7 个普通 Cd并保留 `CdDefendLock`，因此该 ID 为 equivalent。两者都不是 Unity 差异；`IN.CD.02` 的 AI 递减 ownership 根因仍成立，所以 13 个去重根因不变。

### 13 项开放根因

| 组 | 根因 | 关联 ID | 状态 |
|---|---|---|---|
| Framework | bootstrap 把 `WaveIdx -1 -> 0` | `FW-BS-008`,`FW-LC-004` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 8-slot roster 压缩且 independent team 未规范化 | `FW-BS-008-B1` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 初始出生 X/Z 与 RNG 消耗改用 scene transform | `FW-BS-008-B2` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 初始 `HitStop=75,Vx=Vz=0.1` prime 缺失 | `FW-BS-009` | 已确认 / 未修复 / 未运行时验证 |
| Framework | stage spawn 经通用 Register 误清复用槽 ARest/VRest | `FW-WR-005`,`FW-TK-028`,`FW-H-050`,`FW-H-059`,`FW-LC-004` | 已确认 / 未修复 / 未运行时验证 |
| Framework | Results active 后仍执行普通战斗 pass | `FW-TK-002`,`FW-END-002`,`FLOW.05` | 已确认 / 未修复 / 未运行时验证 |
| Framework | `HitConfirm2` 等 candidate carrier 到下一次 collect 才清 | `FW-TK-034`,`FW-H-042` | 已确认 / 未修复 / 未运行时验证 |
| Frame/input | `CdDefendLock` 错对 AI 递减；成功 jump 双方均清7个普通Cd并保留lock，不是差异 | `IN.CD.02`；`IN.JUMP.03` 已移出差异账 | ownership 已确认 / 未修复 / 未运行时验证 |
| Frame/input | late holder 改帧后再次写 held Frame/位置 | `FLOW.09` | 已确认 / 未修复 / 未运行时验证 |
| Frame/input | `ReleaseTick` storage/writer/hash 缺失 | `RT.LINKS.01` | 已确认 / 未修复 / 未运行时验证 |
| Interaction | IronBall type2 的 dvx/dvy 预处理 gate 错落到 type6 | `INT-HIT-005` | 已确认 / 未修复 / 未运行时验证 |
| Interaction | late opoint child X/Y 使用浮点 `PS`，未按 spawner `XInt/YInt` | `INT-OP-001`,`INT-OP-002` | 已确认 / 未修复 / 未运行时验证 |
| Trace 工具 | `BattleParitySnapshot` 对空槽/category、release、block、transform/weapon/owner 等字段硬编码或错映射 | `RT.CHECK.01` | 已确认 / 未修复 / 未运行时验证 |

每项的权威方法、Unity 对应、可复现前置、预期/实际和依赖见完整差异清单的 BATTLE-AUDIT7 总表。DAT 文件适配不处理；T8 默认 `stage.dat` 部署暂缓，stage runtime 用内存 fixture；fixed-world camera 和不改变逻辑结果的 Unity-native 适配保持。

### 行动顺序

1. 先修 tick/runtime 契约：Results early return、`CdDefendLock`、late held、`ReleaseTick`、candidate carrier；同步补 focused self-check。
2. 修 interaction：IronBall type gate、late opoint 整数 X/Y；覆盖 real/shared、正负和跨零坐标。
3. 修 bootstrap/stage：WaveIdx、8-slot/team、spawn RNG、HitStop/velocity prime、stage rest policy；全部使用内存 fixture，不部署默认 `stage.dat`。
4. **历史行动项（已由 BATTLE-AUDIT8 取代）**：修 trace snapshot 投影，并完成剩余 219 个 frame/input ID 的逐项 equivalent/adapter 分类和反向 Unity-only 零未分类核销；237/237 分类现已完成，但 trace snapshot 等开放差异仍需按最新生产代码重新核销。
5. 最后跑 fresh Unity 编译、full `BattleRuntimeSelfCheck`、normal + hole/independent roster Play Mode、held/opoint/结果态定向场景，再做独立 Architect 复核。证据齐全前不得宣称完整战斗逻辑对齐。

---

## Audit5 全量逐帧审计交接（2026-07-18，风险账已收口）

### 权威与历史结论

- 唯一战斗逻辑权威是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。所有差异定性、修复方向和预期 trace 都必须从该工程的正式 C# 调用链闭合。
- 旧章节中依赖其他来源得出的“已对齐”“已关闭”或“仅作映射参考”结论，在 Audit5 当前状态下全部废止为权威证据。它们只能保留为历史回归基线；未经 C# 重审、fresh 验证和双端 trace，不得恢复完成状态。
- T8 默认 `stage.dat` 部署继续暂缓；默认 trace 使用 `stageFixture=false`，不读取或生成默认资产。

### 当前总账

| 分区 | 报告结果 | 当前实现与 fresh 证据 | 风险账状态 |
|---|---|---|---|
| GameTick / Physics | 21 确认 + 3 风险；正式主干和 Physics 全分支 100% 审计 | `GT-01..15`、`PH-01..06` 共 **21/21 逻辑已写并进入 fresh full PASS** | `R-GP-01..03` **3/3 关闭** |
| HitResolve / CollisionCollect | 33 确认 + 6 风险；两个权威入口全分支审计 | `C-01..33` 共 **33/33 逻辑已写并进入 fresh full PASS** | `R-HC-01..06` **6/6 关闭** |
| Frame / lifecycle | 20 确认 + 6 风险；25/25 方法及 reset/registry/rest 依赖审计 | `FL-01..06`、`FT-01..04`、`OP-01..05`、`LC-01..05` 共 **20/20 生产实现与 focused/full self-check 通过** | `R-FL-01..03`、`R-LC-01..02`、`R-FT-01` **6/6 关闭** |

跨分区原始确认项现为 **74/74 逻辑实现 + focused/full self-check**，原 15 项风险为 **15/15 已关闭**。`BATTLE-AUDIT6-01/02` 是原总账后新增且已关闭；CP-NV1/2/3 与 STEP10 是既有项重开后重新关闭。原 3 个受控 P2 已补强并关闭；最终 freshness 链为 source `2026-07-18 01:06:21.499` < Unity DLL `01:07:21.125` < result `01:07:52.834` **PASS**，Architect `P0/P1/P2=0`。这仍不是任意对局、全输入 production trace certificate。

原 3 个受控 P2 的关闭证据：HC-04 已覆盖真实 step6 `collect -> wrong loop 不消费 -> post consumer 消费` 整链及 current type3 负例；missing-definition 已覆盖 Character/Weapon 的完整错误循环、正确循环与 tail；`LF2CharacterInteractionResolver` 的本地类型 helper 仅单行委托中央 `LF2Entity.ResolveCurrentDataObjectType`，不再存在第二份类型判定维护漂移。

### 原 15 项 trace 风险关闭状态

| 分区 | 风险 | 状态 |
|---|---|---|
| GameTick / Physics | `R-GP-01` | ✅ fresh 2 tick trace 关闭；tick1 slot0=`frame0/wait37/FWC11/HitStop75`，tick2=`frame5/wait37/FWC0/HitStop74`，双方一致 |
| GameTick / Physics | `R-GP-02` | ✅ production `mass > 0` 扫描，static close |
| GameTick / Physics | `R-GP-03` | ✅ central active filter 关闭 |
| Hit / Collision | `R-HC-01` | ✅ 确认差异并修复 zero-width strict overlap；90 项已知非正宽 geometry 纳入权威等价覆盖 |
| Hit / Collision | `R-HC-02` | ✅ oid999 `next` 闭包 14 帧均为零有效 geometry |
| Hit / Collision | `R-HC-03` | ✅ current OID/type 统一与 gate A/B 覆盖 |
| Hit / Collision | `R-HC-04` | ✅ current-DAT pickup 去除 CLR cast；真实 step6 collect、错误循环不消费、post consumer 消费及 current type3 负例均已覆盖 |
| Hit / Collision | `R-HC-05` | ✅ fixed slot/reuse 关闭 |
| Hit / Collision | `R-HC-06` | ✅ 整数路径关闭 |
| Frame / lifecycle | `R-FL-01` | ✅ 四 weapon 矩阵关闭 |
| Frame / lifecycle | `R-FL-02` | ✅ current-DAT boomerang 关闭 |
| Frame / lifecycle | `R-FL-03` | ✅ raw empty slot `CatchTimer`、占槽清理与 reset 关闭 |
| Frame / lifecycle | `R-LC-01` | ✅ pooled snapshot/cache reset 关闭 |
| Frame / lifecycle | `R-LC-02` | ✅ StableId alias/reuse 关闭 |
| Frame / lifecycle | `R-FT-01` | ✅ 已关闭 FT-01 的 trace 验证债；不是重复风险 |

R-GP-01 freshness：authority source `00:11:23` < DLL `00:11:49` < trace `00:12:07`；Unity source `00:11:23` < Editor DLL `00:12:22` < trace `00:13:44`；compare `00:14:02` 为 `equal-diagnostic`、2 ticks、`firstDifference=null`。它关闭 R-GP-01，但不构成任意对局证书。

最终 PASS 前的失败和收口不可省略：`C-05`、`BATTLE-AUDIT3-12`、state8000/type6 fixture、`C-12`、`GT-04/GT-07/PH-02` 与 Weapon C-26/C-27 均已按权威收口；此前 `18:16:36.721 PASS` 与 `21:57:40.670 PASS` 只保留为过期历史证据，当前统一以 `01:07:52.834 PASS` 和 combined Architect `P0/P1/P2=0` 为准。该结论仍不能替代逐 tick 或目标 Play Mode。

### 原始总账后新增确认差异（BATTLE-AUDIT6）

- **BATTLE-AUDIT6-01 / GameTick-Input pass order，已关闭**：Unity 已拆分 human poll 和 unified character input，正式顺序为 poll → cooldown/M-1 → `NeedClearInput`/tick gate → character input。矩阵覆盖 tick1、清输入、oid51 frame85 gate 外延迟 split、AI 顺序；另补 transformed-human P1：CLR character 即使 current DAT 转为 non-character，仍按 roster human 轮询输入，但不会错误执行 character action。
- **BATTLE-AUDIT6-02 / DJA locals persistence，已关闭**：四类 early-return 保留进入的 private/runtime combo locals，只有正常尾路径 commit；缺/有效 target、oid6 guard、`Unk328` 与正常尾路径均有正负覆盖。
- **旧检查已按权威重写**：synthetic fixture 已补 frame85，same-tick 假阳性改为 gate 外延迟 split；不是删除断言求绿。
- **LC-02 最终契约**：plain free 清 pending、注销 slot/bucket 并归池，不触发虚拟 destroy/event/effect/额外 sound；显式 renderer/manual destroy 路径仍保留各自销毁事件。Frame / Lifecycle 20 项已由 combined fresh full PASS 覆盖。

### CP-NV1/2/3 与 STEP10 C# 重审（重开后已关闭）

这批是对原历史 backlog 的重审，不修改原始 74 项分母。旧历史 PASS 不作为当前证据；生产与检查已按 C# 调用链重写，并进入 `21:57:40.670` combined fresh full PASS。

- **CP-NV1 / immediate frame**：real/shared 双壳均清 Runtime FWC，保留 Trans wait/Prev2；最终负向矩阵覆盖 aaction/taction/jaction、负 action、方向、attacking 和双方 carrier。
- **CP-NV2 / throw snapshot/raw**：throw 已使用 source `atkFrame`；transform fixture 为 attacker frame112、victim `(76,-36)`；none/up/down/both 的 victim `Vz` 为 `0/-3/+3/0`，raw carrier 同步覆盖。
- **CP-NV3 / held sync**：`-131/0/131` 分别验证 frame131+翻面+FWC0、保留进入 frame/facing/FWC、frame131+不翻面+FWC0；位置 center/cpoint 均读最终 resolved current frame。
- **STEP10 P0**：state9 首次 sync、mismatch/escape immediate + early return、escape 同 tick `Vx/Vy`、FWC 清零与 entity stats-only 契约均已落地。
- **最终检查**：旧反权威断言已按唯一 C# 权威重写并扩展 real/shared-DAT、负 action、early-return、速度和 world stats 不变覆盖；combined Architect `P0/P1/P2=0`。

### DAT 诊断统计与 trace 证据

- `Temp/NTSDParity/data-audit-v3-required.json`：137 个权威 OID = 34 equal / 66 different / 37 missing Unity / 0 parse error；差异类别计数为 frame 126、geometry 31、sound cue 155。该统计只描述两套 raw DAT 在各自读取/适配前后的结构差异，保留作诊断信息；它不是战斗逻辑阻塞、backlog 或资源缺失清单，不要求把 DAT 文件改成相同。
- raw production battle-logic manifest 当前为 C# `41c088d2...0375`、Unity `6b34e118...332a`。旧 `compare-v3-full-final.json` 因工具按 raw manifest 做 header gate，返回 `different`、`certificateEligible=false`、`ticksCompared=0`。这只说明该次工具运行没有签发 certificate，不代表生产战斗逻辑失败；未来 certificate 应基于双方正式读取/Unity 适配后的语义 runtime 输入与 trace，raw DAT/manifest 相等不得作为前置条件。
- `Tools/NTSDParity` 构建 0 warning / 0 error。最新 `trace-compare-self-test-iter7.json` 为 **20/20 PASS**，覆盖连续 tick、空 trace、body/hash/slot commitment 防篡改、dense human input、diagnostic 显式 opt-in、diagnostic 永不签发 certificate 与 strict/fixed-world camera profile。
- iter7 authority/Unity full-detail diagnostic trace 均已生成。`compare-v3-diagnostic-full-iter7.json` 返回 `status=equal-diagnostic`、`ticksCompared=6`、`firstDifference=null`、`comparisonProfile=fixed-world-camera`、`diagnosticComparison=true`、`certificateEligible=false`、`certificateClass=none`。
- iter7 的 Unity 端使用 `authority-dat-diagnostic` 夹具；该结果只证明这 6 tick 样例的已观察域一致。原 15 项风险由各自证据逐项关闭，不是由 iter7 一次性关闭；iter7 与 R-GP-01 的 2 tick trace 都不能扩大为完整战斗逐帧等价或 production certificate。

### 状态纪律与下一步

必须按“逻辑已写 → isolated/目标编译 → Unity fresh 编译 → full self-check → 逐风险 trace → 必要 Play Mode”逐级报告，任何一级都不能替代后一级。production certificate 可以继续作为聚合对拍证据建设，但当前数量仍为 0，不能冒充已完成，也不能以 raw DAT/manifest 相等作为签发前置。

1. 原 15 项风险账已 15/15 关闭，不再把“关闭 15 风险”列为下一步。
2. 若继续建设 production certificate，扩展双方正式读取/适配后的语义 runtime、真实输入与长时间 full/full trace；保持 source < DLL < trace/result freshness。
3. 不处理 raw DAT 文件或 manifest 差异；T8 默认 `stage.dat` 部署继续暂缓，不读取、生成或私自部署默认资产。

**Audit5/Audit6 历史交接结论（已被顶部 BATTLE-AUDIT7 当前状态取代）：原始确认项曾达到 74/74 逻辑实现 + focused/full self-check，原 15 项 trace 风险曾达到 15/15 已关闭；Audit6 与重开的 CP-NV1/2/3、STEP10 也保持关闭，原 3 个受控 P2 亦已补强关闭。该批 full self-check 为 source `01:06:21.499` < DLL `01:07:21.125` < result `01:07:52.834` PASS，Architect 当时为 `P0/P1/P2=0`。R-GP-01 fresh compare 为 `equal-diagnostic`、2 ticks、无差异，但不能扩大为任意对局、全输入 production certificate，更不能覆盖 BATTLE-AUDIT7 新发现。34 equal / 66 different / 37 missing Unity 只保留为 raw DAT 适配诊断，不是阻塞或 backlog；raw DAT/manifest 相等不是 certificate 前置。T8 默认 `stage.dat` 部署继续独立暂缓。**

完整报告：

- `.omc/research/game-tick-physics-audit-20260717.md`
- `.omc/research/hit-collision-audit-20260717.md`
- `.omc/research/frame-lifecycle-audit-20260717.md`
- `Temp/NTSDParity/authority-v3-full-iter7.jsonl`
- `Temp/NTSDParity/unity-trace-v3-diagnostic-full-iter7.jsonl`
- `Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json`
- `Temp/NTSDParity/trace-compare-self-test-iter7.json`

## 1. 铁律（不可违反）

1. **权威锁死**：任何正式战斗改动必须能在 `ntsd_release_C#` 的真实调用链中找到对应行为；无法确认时标“待确认”，不得以旧工程或历史资料补写规则。
2. **表现效果一致优先**：能逐行对齐就对齐；Unity 框架限制无法同构时，**运行时最终表现必须逐帧等价**（位置/帧号/速度/伤害/时序）。
3. **只新增不误删**：本文的 ❌ 项都是「C# 有 Unity 无」，是**新增**任务，**不是删除**。
4. **架构等价严禁删**：见 §5 清单——Unity 用 resolver/组合/hook 换方式实现的，不算冗余。
5. **排除范围不碰**：bg.dat 可活动范围、相机——不对齐，不改。

## 第三次实战/静态审计交接（2026-07-16，历史记录；已被 Audit4 取代）

旧版“当前没有已确认但未实现的正式战斗逻辑差异”结论已失效。完整编号和双方证据见 `csharp-vs-unity-battle-alignment.md` 的 BATTLE-AUDIT3-01..17。17 项生产修复现已全部落地；10 已完成通用 hit_Fa 重构并补齐 3/4/10/14 直接覆盖，12 已补齐 generic holder、damaged 后继续 dvx/kind3、IronBall `FrameDelay=1` 及 world-level 真实武器覆盖。最新 fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `2026-07-16 18:24:04` < Unity `Assembly-CSharp.dll` `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。该结果包含 M-1/T4 最新矩阵；此前生产 diff 的 Architect 复核结论保留。当前仍不代表 17 项 Play Mode 全完成：真实 `NTSD_Battle` 的 Naruto 防前跳螺旋丸、奔跑防跳命中及防下跳六分身仍待本轮回归，也不能宣称全部战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署仍按用户要求暂缓。

### 分组进度

- **既有候选收集 7 项（03/04/13/14/15/16/17）**：生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；真实 Play Mode 尚未运行，不能标记场景验收完成。
- **本批已落地 8 项（01/02/05/06/07/08/09/11）**：生产修复与针对性 self-check 已通过。01 已补 `RelationTeam`，仍等待真实 bootstrap/螺旋丸 Play Mode；02/05 等 held 表现和攻击链也须在同一场景回归。09 的权威契约是 invalid positive link 只清 holder 的 `LinkState/TargetIdx/HeldWeaponSlot`，不清 inactive/mismatch target 的反向字段。
- **本批新增落地 2 项（10/12）**：10 已将 `hit_Fa1..14` 唯一实现下沉 `LF2Entity`，由 Special/Other/current-DAT shell 共用，并删除旧 TU/重复副本；新增 self-check 覆盖 3/4/10/14，其中 3/14 对 Other、current-DAT Character、Special 三种壳连续两 tick 验证副作用仅执行一次，4 覆盖 catch frame/速度/`CatchTimer`，10 覆盖原路径与落地摩擦防重复。12 的 generic holder、damaged 后继续 dvx/kind3 与 IronBall `FrameDelay=1` 已落地；`CheckWorldLevelRealWeaponStep12Contracts` 经 `SimulationWorld.HeldObjectProcessAll`、generic `LF2Entity` holder 和真实 `LF2Weapon` 覆盖 damaged→dvx、damaged→kind3、IronBall `FrameDelay=1`。新增矩阵 fresh PASS；两项仍未完成真实场景 Play Mode 验收。
- **T8**：默认 `stage.dat` 资产部署继续暂缓，不进入本轮推进。

### 执行顺序

1. **编译与自检已清**：fresh `/m:1` build 为 0 errors / 42 existing warnings；source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full self-check fresh PASS。编译和针对性自检仍不能替代真实场景行为。
2. **关系与 held 前置**：01、09、12 的生产修复和现有 self-check 已通过；09 只清 holder 三字段；12 的 world-level generic holder/真实 weapon 覆盖已补齐。01 仍待 bootstrap Play Mode。
3. **held/坐标相位**：02、05、06、08、11 的生产修复和针对性 self-check 已通过，等待真实 Play Mode。
4. **候选收集**：03、04、07、13、14、15、16、17 的生产修复和对应矩阵已通过，等待真实 Play Mode。
5. **frame logic 分派**：10 的生产重构和 fresh self-check 已通过；`hit_Fa1..14` 唯一实现已下沉 `LF2Entity`，直接覆盖已扩展到 3/4/10/14 及三壳两 tick 单次副作用矩阵。
6. **运行验收**：当前版本防下跳六分身已通过；继续回归 Naruto 防前跳螺旋丸的层级、位置、跟手和攻击路径、奔跑防跳完整后续招，以及投掷武器单次命中/Arest。
7. **Audit3 历史回写状态**：当时可写“生产修复已落地、针对性 self-check 已通过”；该阶段后来被 Audit4 的实现与 Play 验收取代，最终状态以本文后部 Audit4-01..16 为准。

### 验收门槛

- 编译错误必须为 0；“隔离 Roslyn 本轮 0 诊断”不能代替 Unity 编译成功。
- `BattleRuntimeSelfCheck` 已 fresh PASS；该结果只证明现有断言通过，不自动补齐未覆盖分支或真实场景。
- 17 个差异簇的现有针对性矩阵已通过；10 的 3/4/10/14 与三壳两 tick 矩阵、12 的 world-level generic holder/真实 weapon Step12 矩阵均已 fresh PASS。
- `NTSD_Battle` 当前版本的防下跳六分身已通过；仍需回归 Naruto 防前跳螺旋丸、奔跑防跳完整后续招和投掷武器单次命中/Arest。
- T8 只记录逻辑/生产接线状态；默认 `stage.dat` 资产部署继续暂缓。

### Audit3 历史对外措辞（已失效）

**“已发现并记录 17 个战斗逻辑差异簇，生产修复现已全部落地；fresh `/m:1` build 为 0 errors / 42 existing warnings，source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full `BattleRuntimeSelfCheck` PASS；M-1/T4 与 Audit3-10/12 的新增矩阵均已覆盖。但本轮真实 `NTSD_Battle` Naruto 螺旋丸、奔跑防跳和六分身仍待 Play Mode 验收，因此不能把 17 项标成 Play Mode 全完成，也不能宣称 C# 与 Unity 战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。”**

## 第四次战斗命中/技能链审计交接（BATTLE-AUDIT4，2026-07-17 最终状态）

完整双方坐标、影响和逐项状态见 `csharp-vs-unity-battle-alignment.md` 的 BATTLE-AUDIT4-01..16。本批 16 项**生产修复已落地**，Audit4 针对性断言已进入最终 fresh full self-check 并通过，3 项目标 Play Mode 也已全部通过。该结论只关闭本批确认差异，不能关闭完整对局逐帧对拍和 RISK-4。

| 执行组 | 编号 | 内容 | 当前状态 |
|---|---|---|---|
| 核心命中链 | 01、02、03、04、05 | AttackExempt 清理、统一标准命中、weapon candidate 消费、额外 Arest、post-collect 重筛 | 生产修复已落地，针对性矩阵 fresh PASS；投掷武器 Play 09:45:21 PASS |
| 独立实现轨 | 07、08、09、15 | SpecialAttack type3 tail、状态转换 RNG、held raw frame/facing、late frame 后 held pose/presentation | 生产修复已落地，已有断言 fresh PASS；螺旋丸 Play 01:10:34 PASS |
| 命中尾收口 | 06、10、12、14、16 | Naruto kind3、受击方向、命中声音、effect6/23 spark、catching state-exit/full reset | 生产修复已落地，针对性矩阵 fresh PASS；奔跑防跳 Play 09:34:36 PASS |
| opoint/声音生命周期 | 11、13 | first-op/OID5/52 与 frame sound/pic999 生命周期 | 生产修复已落地，针对性矩阵 fresh PASS |

最终 fresh 证据链：Unity Editor PID `11540` fresh script compile 为 **0 C# error**；`BattleRuntimeSelfCheck.cs` source/test `2026-07-17 01:39:46` < `Assembly-CSharp.dll` `09:26:23` < result `09:26:55`，full self-check **PASS**。Architect 最终复核为 **PASS**。Architect 复核后补入的矩阵明确覆盖：SpecialAttack consume 删除 live `Team` gate；collect 后将 attacker `Team=0` 仍按冻结候选连续消费两个目标；显式 oid300 abort 仍停止后续候选；SpecialAttack `PendingSounds` 严格断言单条 Cue/WorldX/Tick，并覆盖下一 tick 与 reset 清理。

`BATTLE-AUDIT4-15` 是 Play 抓出的 held late frame pose/presentation 差异：`HeldObjectProcess` 早于 late `SimFrameTick`，holder 首 tick 切帧后 held 仍读旧挂点。现已在 late frame 变化后执行纯 `SyncHeldPose`，不重复 step12，并按 holder→held 刷新 renderer。focused freshness 链 source `01:05:07` < DLL `01:06:22` < result `01:07:01` **PASS**；Rasengan Play `01:10:34` **PASS**：frame240 / oid434 / link 成立，change runtime/holderVisual/heldVisual=`5/5/5/5`，move=`9/9/9/9`，sorting `526 -> 527`，攻击链 `20 -> 257 -> 258 -> 259`，oid434 `396 -> 397`。

`BATTLE-AUDIT4-16` 是 Play 抓出的 catching state-exit/full reset 差异：Unity 普通 state transition 提前清 catch link，导致 `276 -> 277` 后下一 tick 按 `PrevFrame2=276` cpoint 强制 frame0。现已取消普通 state transition 清 link，完整实体 Reset 仍清。最终 full self-check `09:26:55` **PASS**；Running Play `09:34:36` **PASS**，完整链为 `9 -> 102 -> 295(prev2)/297(pn) -> 298 -> 299 -> 275 -> 276 -> 277 -> 278 -> 279 -> 86 -> 87 -> 88`，victim 保持 frame130/catch；oid33 `current311/pn310` 为 wait0 的正确口径。

Naruto 防下跳六分身的当前版本定向 Play Mode 已通过：真实生产输入链 `L -> L+S -> L+S+K`，tick1 frame271，tick12 frame272 且 PP `500 -> 295`/生成 oid205，tick15 frame273/oid204 展开，tick29-32 出现 6 个 unique oid33/action307，tick38 共有 6 个 renderer 可见；峰值 `max204=11`、`max205=3`、`uniqueClones=6`、`action307=6`、`maxVisible=6`。

投掷武器 Play `09:45:21` **PASS**：使用生产 oid120 / hold / double-D / D+J；HP 只在 tick17 从 `500 -> 489` 下降一次；weapon state1002/frame41 后同 tick 切到 frame7/state1000，`AttackExempt=4`；跨 35 tick 冷却归零并落地，HP 无二次下降。至此三项目标 Play Mode 已全部完成。T8 默认 `stage.dat` 资产部署继续暂缓。

当前 Unity 自动生成的 dotnet `.csproj` 仍包含 35 个已删除历史源文件，最终 `dotnet build` 被 `CS2001` 阻塞。不得把此前的 dotnet 0 error 冒充为 Audit4-16 后的最终编译证据；有效证据是 Unity fresh script compile 0 C# error。

当前对外措辞更新为：**“Audit4-01..16 的生产修复已落地并经 Architect 最终复核 PASS；Unity fresh script compile 为 0 C# error，fresh full `BattleRuntimeSelfCheck` PASS；Naruto 防下跳六分身、螺旋丸、奔跑防跳后续招和投掷武器目标 Play 均通过。本批确认差异已关闭，但完整对局逐帧对拍和 RISK-4 仍在，因此不能宣称 C# 与 Unity 全部战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。”**

非行为性清理债：`WeaponSpawner` 仍有历史非 C# 注释，F9 debug 说明也存在与当前 C# 唯一权威措辞冲突的历史文字。F7-F9/debug 按 `AGENTS.md` 排除正式战斗 backlog，不计为生产逻辑差异。

## 2. 任务清单（按建议顺序，坐标精确到行）

### T0 — 修真 bug：exemptVal 用错变量（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:268` → `int itrArest = itr.Arest < 4 && itr.Vrest == 0 ? 4 : itr.Arest;`
- **Unity 落点**：`LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入已改用 arest/vrest 权威公式。
- **验收**：`CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 Unity 运行时自检。

### T1 — ApplyAlternateDamage（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:629-827` `ShouldUseAlternateHurt` / `ApplyAlternateDamage` 完整方法。
- **Unity 落点**：共享 `LF2AlternateDamageResolver`，由真实角色与 shared-DAT 两入口复用；runtime/stat/运动尾契约已补齐。
- **验收**：alternate trigger/core/motion/character/shared-DAT/heavy/object-pass 针对性检查均通过。

### T2 — 武器命中 spark（M-9，已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1150` `RecordKind0Hit`（timer：`Fall>60 ? sparkPhase*20 : sparkPhase*20+10`），312/320/**506** 三处调用，**武器命中路径（506）也调**。
- **Unity 落点**：`LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径已接入。
- **验收**：`CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 Unity 运行时自检。

### T3 — frame 110/114 → CdDefendLock=3（M-14，已完成，Unity 运行时已验证）
- **C# 权威**：`FrameTick.cs:208-209` → `if (frame==110 || frame==114) CdDefendLock=3;`
- **Unity 落点**：`LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载。
- **验收**：`CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 Unity 运行时自检。

### T4 — oid 7/8 → 51 合体拆分（Audit6 重审已关闭）
- **C# 权威**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:1093-1263` `RunOid5152RuntimeMaintenance/TryMergeOid7Or8Into51/SplitOid51BackToPair`。旧来源结论只保留为历史记录，不能覆盖 C#。
- **历史错误顺序（已被 Audit6 推翻）**：旧实现曾按 `TickCooldowns -> human input -> AI input/combo -> M-1` 提前消费 DJA，并据此要求同 tick 拆分。唯一权威 C# 的正式输入消费在 M-1 与 `NeedClearInput` gate 之后，详见 `BATTLE-AUDIT6-01`。
- **Unity 落点**：`Oid5152RuntimeMaintenanceAll`、merge/split helper 与 runtime 身份/表现维护链已落地；split partner 在 `Reset()` 后恢复正式默认值 `FrameDelay=0`、三轴 knockback=`0.1`、`HolderCopy=99`、prev carriers 清零、`Effect`/`DeadBlink` reset，同时保留 Entity 外部 `ItrRest`。
- **当前验收**：旧 same-tick 期待已按权威改为 frame85 gate 外延迟 split，synthetic fixture 已补 frame85；poll/M-1/apply、tick1/clear gate、human/AI 与 transformed-human 均进入 combined fresh PASS。
- **freshness**：旧 `18:33:00` PASS 已过期；当前统一使用 source `21:55:28` < DLL `21:56:56` < result `21:57:40` PASS。

### T5 — 复活 pass（已完成，Unity 运行时已验证）
- **C# 权威**：`GameTick.cs:839-934` `RunRespawnPass`（tick step10）
  - 门控：state==14 + Hp<=0 + (KillCount>=0 OR Unk364==5 OR slot>=20) + HitStop∈(0,5)
  - 分支A（RespawnCount<=0）：Hp2Orig<2→FreeEntity；否则 Hp2Overlay-1、队友 X/Z 平均+随机、Pp=500、HpMax=Hp3、Hp=HpMax、HitStop=20、Frame=212、YInt=-300
  - 分支B（RespawnCount>0）：Pp=0、HpMax=RespawnCount、Hp3=HpMax、Hp=HpMax、RespawnCount=0、Unk364=1、oid∈[0x1E,0x24]→Unk318=0x8C、Frame=0xDB、FrameDelay=0xA、生成 oid998 复活特效
- **Unity 落点**：`PostFrameAdvanceDeathCleanupAll` 已实现两分支、free gate、队友平均落点、血量/PP/帧字段与 oid998 特效。
- **验收**：无 stored-count、free gate、stored-count + effect 三项检查均通过。

### T6 — kind 15/16 副作用补齐（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1628` `ApplyKind15Or16` + `1737` `ApplyKind15Movement`
  - kind16 完整：Hp-、KillStat++、ComboCountAtk、`RecordSound("SFX_065")`、Frame=200、vrest 写入、LinkState==2 断开
  - kind15 位移：`KnockbackVx = Vx + (±1)`、真实 Vx=KnockbackVx、`KnockbackVz = Vz + (±0.5)`、`YInt=-2`；按对象类型分 vyStep（角色3.0 / 飞行道具3.0 / IronBall2.3）
- **Unity 落点**：真实角色与 shared-DAT resolver 已补 kind15 authority 位移、kind16 统计/vrest/link/SFX 副作用。
- **验收**：`CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过。

### T7 — combo 连招 wrapper（大）
- **C# 权威**：`InputRuntime.cs:740` `RunComboWrappers`（9 组：Dra/Dla/Dld/Dlu/Drd/Dru/Djd/Dja/Daa/Dab + DjaGuard，含 oid6 Sasuke DjaGuard 特判），入口 `InputRuntime.cs:647`。
- **Unity 现状**：已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6 DjaGuard，真实消费路径为 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。
- **本轮新增验证**：`BattleRuntimeSelfCheck` 已补 `CheckComboWrappersCharacterFrameJumps` 与 `CheckOid6DjaGuardComboHold`，覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release。
- **验收现状**：已通过当前打开 Unity 的 request 自检机制，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。**T7 已完成。**
- **Naruto DDJ 完整链补充验收（2026-07-16）**：同 tick held chord 的内部输入 `att + down + def` 先命中 frame271；272 生成 oid205/action98，辅助链经过 99/325/341；273 生成 oid204/action130，展开六分支并各自到 147 生成 `6 x oid33/action307`。clone 在 307 后落地到 frame219 是 authority 行为。
- **本次确认的 5 个根因**：`LF2ReferencePool.Release` 无条件接收外部 synthetic，污染逻辑池类型；factory 角色 opoint 在 `ModuleBind` 注册前过早用 `slot < 0` 拒绝；pending-unregister 对象同 tick 归池复用时，旧 registry bucket 的 `Contains` 拒绝后续递归分支，六 clone 只出 3；pooled `LF2Character.Init` 未重新分配 `StableId`；`SpawnFromOpoint` 缺 `RelationTeam`、`Unk364` 与 holder-copy 继承。
- **修复契约**：`Release` 只归池 active 实例；`Register` 先 finalize pending old lifecycle；slot guard 移至 `ModuleBind + Initialize` 后；character `Init` 重新 `AllocateStableId`；`PostInitLiving` 继承 `Team`、`RelationTeam` 与 holder-copy。专项回归验证 PP 500→295、dynamic slot、6 unique StableId、6 x action307 和 6 visible renderer。
- **真实 Play Mode 生产输入链验收**：在 `NTSD_Battle` Play 中等待 slot0 `CharacterInputModule`/`ActionMap` 就绪，通过 UnityMCP 临时 `InputSystem.Keyboard` 按物理绑定注入 `L (Defend) -> S (Down) -> K (Jump)`。事件完整经过 `InputActionMap -> CharacterInputModule -> SimInputBuffer`，未直接调用技能、帧或 opoint。日志为 `INPUT focused=True buffered=1, attackAction=0, jumpAction=1, defendAction=1, moveY=-1`，crossed internal mapping 符合预期；结果 `frame271=True, max204=11, max205=3, maxClones=6, maxSpriteReady=6, maxVisible=6`。
- **Play Mode 时间线/证据/限制**：clone 数在 `t=0.446/0.473/0.509/0.541` 依次为 `3/4/5/6`，测试窗口无异常，截图 `Temp/naruto-ddj-unitymcp-peak.png`。Win32 `keybd_event` 不被 Unity RawInput 接收，所以这不是物理硬件键盘证明；成功证据是 UnityMCP Input System Keyboard 事件经过完整生产输入链。

### T8 — stage 波次刷敌（M-13，大）
- **C# 权威**：`GameTick.cs:2317` `ApplyCurrentWavePhaseAdvance` + `2350` `ApplyCurrentWaveImmediateStageSpawns` + `2226` `RefillCurrentWavePositiveStageSpawns`（配套 `StageProgression` + `StageSpawnRuntime*` 一整套，见 `SimulationWorld.cs:68-80`），tick step23。
- **Unity 落点**：`BattleRuntimeState` 已补齐 `StageProgression` / `StageSpawnRuntime*`；`SimulationWorld.StageWave.partial.cs` 已实现立即刷敌、正 ratio 并发槽/总量补充、清场推进和 phase bound 写回；`NTSDBattleTickSystem` 在 `PreFrameBounds` 后、`RenderDispatch` 前执行该 pass，匹配权威 step23 顺序。spawn 契约已补 `Unk344=2`、DAT type 0/5 的 character-init `RelationTeam=2/HitStun=20`、其他类型 `RelationTeam=0/HitStun=0`、dynamic slot 50+ 和 action 0 保留。
- **生产接线**：`AppManager.InitializeBattle -> SimulationTickDriver.ApplyMatchConfig -> BattleStageCampaignLoader -> ConfigureStageCampaigns(-1) -> StartInitialStageWave()` 已接通；默认读取 `Application.streamingAssetsPath/NTSD/data/stage.dat`，也可由 `MatchConfig.stageCampaignFilePath` 显式覆盖。仓库当前未纳入二进制 `stage.dat`，缺失时会明确 warning 并保持 `StageProgressionValid=false`。
- **本轮新增验证**：`CheckStageWaveBootstrapAndSpawnContract` 覆盖 stage 文本解析、pre-wave -1→0、bound、type0/type5/非角色身份契约和 action 0；`CheckStageWaveImmediateSpawnAndAdvance` 覆盖真实 direct spawn、dynamic slot 50+、20-49 非 stage 槽隔离、清场推进；`CheckStageWavePositiveSpawnRefill` 覆盖并发槽补位与总量上限。
- **验收现状**：fresh Unity batch self-check 返回 `PASS`。**T8 逻辑与生产接线代码已完成并通过运行时验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。**

### T9 — AI 输入生成器（已完成，Unity 运行时已验证）
- **C# 权威**：`InputRuntime.cs:16` `PrepareAiInputBasic` + 14 辅助函数：
  `AiBetweenX / AiPostCacheCoordinateAllowsSpecial / AiPreUpdateTarget3000SideEffect / AiUpdateOid33_19_16PredictedDuaDecision / AiUpdateOid52_1_2_21PreLabel591Decision / AiUpdateLabel591Oid51_2_18_7Decision / AiUpdateFirstDecision / AiUpdateTeammateGuardDecision / AiUpdateOid1ComboDecision / AiUpdateCloseOid1Decision / AiUpdateOid4ComboDecision / AiUpdateOid5ComboDecision / AiProcessSubOidGroup / AiSpecialOidForSubGate / AiProcessHelper`（行号见差异清单 §6.2）
- **Unity 落点**：`SimulationWorld.AiInput.partial.cs` 已完整承载主入口及直接/间接 helper 闭包，包含 runtime-slot target/cache、coordinate、team/history/held gate、C8/D3/D4/7A/7B 扫描、oid 决策组、move-mode/no-target 和三个 `AiProcessSub*` 尾部分支。
- **历史输入接线（已由 Audit6 修正）**：Unity 曾让 human 与 AI input/combo 在 oid51/52 maintenance 前执行；`BATTLE-AUDIT6-01` 已按唯一权威 C# 拆分 poll 与 apply，并经 tick1/clear gate、human/AI、oid51 延迟 split 和 transformed-human 矩阵 fresh 验证。
- **验收**：fresh dotnet build 为 0 errors / 42 existing warnings；fresh Unity full self-check 返回 `PASS`。自检覆盖 target/cache、coordinate、同 seed 确定性、human 隔离，并由 M-1 full-tick 矩阵覆盖 AI DJA 在 maintenance 前同 tick 拆分。**T9 已完成。**

## 3. 已确认对齐（不要重复处理）

tick 主循环主干（含 `InputPhase`/`FrameMod12`/`FrameToggle` 统一推进）、全局 `ValidatePositiveLinks`、kind 0/4/9 主流程、kind 6/8/10/11/14、oid300、kind5 委托、M-5 死亡弹地、M-7 kind4+WeaponCount 翻转、HP/PP 自然恢复、heal/catch timer、state14 复活与 respawn pass、frame mp turn-around、frame202 HitStun=20、opoint 生成、cpoint 正值主流程、state 400/401/500/501、N30 触发、状态转换特效。

## 4. 确认可不移植

- **M-6 F8 强制掉武器**（`RunF8WeaponDrop`）：调试功能，Unity 不需实现（非冗余）。
- `RunMode2RandomWeaponDrop`、`InitStats`/mode2 postframe 分支：属于 C# 权威工程的 F7-F9/debug 控制路径，不作为正式战斗对齐项。

## 5. 架构等价（🔷 严禁当冗余删除）

| Unity 机制 | 对应 C# | 说明 |
|-----------|---------|------|
| `LF2Character*Resolver` / `LF2Weapon*Resolver` | `NtsdCharacter`/`HitResolve`/`CPointRuntime`/`WeaponRuntime` 各段 | 组合模式拆分 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 角色分发 | 服务 transform 后 wrong-shell 角色 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | 运行时化，字段一一对应 |
| `FrameTransistor` hook | `FrameTick.Tick` 内联步骤 | 拆 hook 供覆写 |
| `SimulationWorld` 动态槽 | `Objects[400]` 固定槽 | 遍历顺序须保持 slot 升序 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023，区别于会清 attacking 的 ImmediateFrame |

## 6. 排除范围（不对齐、不改）

菜单/选人/加载、HUD/结算、bg.dat 的 Z 可活动范围、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施、`src/Host/*`。注意：PreFrame 中改变实体存亡或 X 坐标的逻辑边界仍在战斗范围内。

## 7. 工作流（每个任务照做）

1. **溯源**：打开 C# 权威行号，读懂完整逻辑（含分支/常量/字段读取顺序）。
2. **索要原型**：向 Codex 要 unified diff patch（`sandbox=read-only`，严禁真实改码），作为逻辑参考。
3. **重写**：以原型为参考，写成符合 Unity 架构的生产级代码（用现有 resolver/hook/runtime 字段）。
4. **改码**：用 executor-high（多文件）或 executor（单文件）落地。
5. **Review**：改完立即用 Codex review 或 `code-reviewer-low`。
6. **验收**：按每项的「验收」标准，优先跑 `BattleRuntimeSelfCheck`；无法运行时说明原因，不谎报。
7. **更新清单**：完成一项，去 `csharp-vs-unity-battle-alignment.md` §10 勾选对应行。

## 8. 关键文件速查

| 用途 | 路径 |
|------|------|
| 全量差异清单 | `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md` |
| C# tick 主干 | `ntsd_release_C#/src/BattleCore/Simulation/GameTick.cs` |
| C# 命中结算 | `ntsd_release_C#/src/BattleCore/Interaction/HitResolve.cs` |
| C# 帧推进 | `ntsd_release_C#/src/BattleCore/Frame/FrameTick.cs` |
| C# 输入+AI | `ntsd_release_C#/src/BattleCore/Input/InputRuntime.cs` |
| Unity 角色命中 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs` |
| Unity 武器 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` |
| Unity 帧推进钩子 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` / `LF2Character.cs` |
| Unity pass 调度 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` |
| Unity 候选收集 | `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs` |

## 9. 优先级建议

T0-T9、Audit2/Audit3/Audit4、P1 BOUNDS-X 以及 OPOINT-VIS、STEP10、TRANSFORM-SHELL、FRAME-ADV/FRAME-TICK 的既有 self-check 继续作为回归基线。**当前 Audit5 原始确认总账为 74/74 逻辑实现 + focused/full self-check，Audit6 与重开的 CP-NV/STEP10 也已关闭；freshness 为 source `21:55:28` < DLL `21:56:56` < result `21:57:40` PASS，combined Architect `P0/P1/P2=0`**。这不替代本批目标 Play Mode、完整逐 tick 或 production certificate。

| 优先级 | 当前推进 |
|---|---|
| P0 | ✅ CP-NV1/2/3 与 STEP10 已按唯一权威 C# 重审、修复并重新关闭；immediate FWC、source throw snapshot/Vz、held resolved frame、early-return/即时速度和 entity stats-only 均进入 `21:57:40` combined fresh PASS |
| P1 | ✅ INPUT-1~9 与 INTERACT-1~5 全部修复并通过新增运行时矩阵；既有 OPOINT-VIS、Step10 等 runtime matrix 继续作为回归基线 |
| P2 | ✅ RISK-1/2/3/5 与 NARUTO-DDJ/OPOINT-LIFECYCLE 已修复并运行时验证；后者覆盖 pending 注销、同 tick 归池复用、递归 opoint、StableId 和关系字段继承 |
| P3 | ⚠️ Audit4-01..16 与 3 项目标 Play 已清；继续保留 RISK-4 与完整对局逐帧对拍缺口，不扩大为全战斗完成声明 |

T8 默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。

16 个 Audit4 差异的发现证据与逐项收口状态不在本行动版重复维护，统一见完整差异清单的 Audit4 章节。INPUT-1~9 由 `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖；INTERACT-1~5 由 `CheckInteractionRuntimeSlotContracts` 覆盖；NARUTO-DDJ/OPOINT-LIFECYCLE 由真实 frame271→oid205/204→6 x oid33/action307 完整链覆盖。

P0 的旧 CP-NV 检查曾含覆盖不足或反权威期待，历史 PASS 已废止。当前 `CheckCpointNegativeActionMatrix`、`CheckCpointHeldSyncVactionMatrix`、`CheckCpointThrowRawAndTransformMatrix` 已按 C# 重写并覆盖 real/shared 双壳、负 action、FWC、source snapshot、Vz 和 `-131/0/131`；STEP10 的 mismatch/escape、即时速度和 world stats 不变也已纳入 combined fresh PASS。

本批已验收项：

- OPOINT-VIS：`CheckQueuedObjectPointPassBoundaries` 与 late-mutation 矩阵已验证 pre-advance、natural drop、逐实体 late 发布边界、real factory queue、父回收与高/low slot 可见性；过程修复 pending-destroy active-filter。
- STEP10：state9 首次 sync、mismatch/escape early return、即时速度、real/shared-DAT cpoint 与 entity stats-only/world stats 不变矩阵已通过。
- TRANSFORM-SHELL / FRAME-ADV / FRAME-TICK / LC-02：已验证 character/weapon `PS.BindRuntime`、逐 slot Transit/TU、SpecialAttack 单次 physics/frame_tick/type3 drain、`PpDisplay`、state14、negative next、state4000/8000 WFC/hit-stop 顺序、type1/2/4/6/oid999 current-DAT landing，以及 cross-SimOrder pending plain free 只注销一次且不触发虚拟 destroy/event/effect/额外 sound。
- INPUT-1~9：real character 与 shared-DAT 输入路径均已修复；`CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖 state switch、`YInt` 门、velocity tail、单一 defend-lock 真值、Super Punch、raw frame write、running 和 frame215。
- INTERACT-1~5：dynamic slot、runtime-slot vrest、state3003 双向 vrest 与 non-character kind2 链接均已修复；动态槽 `50..399` 耗尽时直接拒绝生成，并由 `CheckInteractionRuntimeSlotContracts` 断言不遗留 registry 空桶、renderer pool 对象或 reference/logic pool 生命周期残留。
- NARUTO-DDJ / OPOINT-LIFECYCLE：reference pool active-only release、pending lifecycle finalize、factory 注册时机、pooled character StableId 重分配和 opoint Team/RelationTeam/HolderCopy 继承均已修复；真实链验证 6 个 clone 使用 dynamic slot、拥有 unique StableId、到达 action307 且 renderer 可见。
- RISK-1/2/3/5：locomotion 单次推进、raw move frame、held/`TrackerParent` runtime-slot 生命周期和 current-DAT step7/step9 路由均已修复并运行时验证；`CheckHeldReferenceSlotReuseContracts`、`CheckStateTransformInteractionPhaseRouting` 等新增矩阵通过。
- RISK-4 / COLLISION-SNAPSHOT：这是 Audit2 历史风险名，现已由 Audit5 `R-HC-05` 的 fixed-slot/reuse 覆盖关闭，不再是开放项。

## 10. 实施进度（2026-07-16）

> 下表是 Audit4 前的历史实施快照，不代表当前验收已经结束。Audit4 当前状态以本文前部交接段和完整差异清单为准；旧来源记录不得用于当前实现。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState` | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **历史实现/self-check 已通过；待 C# 重审** | 唯一权威为 C# `GameTick.cs:1093-1263`；merge/split 与 pass 顺序需据此重新核验 | 既有 7 项检查只保留为回归基线，不能代替 C# 权威重审 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓 |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；输入 pass 分段；runtime 字段与 roster/opoint bootstrap | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |
| 二次审计 INPUT-1~9 | **全部已修复 / Unity 运行时已验证** | real/shared-DAT input state、raw frame、velocity tail、running/frame215 等契约已按 authority 收口 | `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵通过 |
| 二次审计 INTERACT-1~5 | **全部已修复 / Unity 运行时已验证** | dynamic slot、满槽拒绝、runtime-slot vrest、state3003、non-character kind2 已收口；拒绝路径不遗留 registry 空桶、renderer pool 或 reference/logic pool 生命周期残留 | `CheckInteractionRuntimeSlotContracts` 通过 |
| Naruto DDJ / OPOINT-LIFECYCLE | **已修复 / 当前版本真实 Play Mode 已通过** | active-only reference release；register finalize pending lifecycle；slot guard 后移；pooled character 重分配 StableId；`PostInitLiving` 补 Team/RelationTeam/HolderCopy | 当前回归通过 PP 500→295、dynamic slot、6 unique StableId、6 x oid33/action307、6 visible renderer |
| 二次审计 RISK | **历史 RISK-1..5 均已关闭** | locomotion、raw move frame、held/Tracker slot、current-DAT interaction 与 fixed-slot reuse 已收口 | Audit5 原 15 项风险总账 15/15 关闭 |

Audit3 历史验证（2026-07-16）：fresh `/m:1` build 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。M-1/T4 的 gate、oid8 镜像、identity/presentation、human+AI DJA full-tick、split formal reset 与 `ItrRest` 保留矩阵，以及 Audit3-10/12 的扩展矩阵均包含在该结果中。M-1 runtime self-check 已完成，但仍不能扩大为全部战斗逻辑完全对齐。T8 默认 `stage.dat` 部署继续由用户明确暂缓。

当前版本 `NTSD_Battle` Play Mode 已通过 Input System 的 `L -> L+S -> L+S+K` 完整生产输入链并观测 `frame271=True`、`max204/max205=11/3`、`uniqueClones/action307/maxVisible=6/6/6`。螺旋丸、奔跑防跳和投掷武器三项 Play 也已分别于 `01:10:34`、`09:34:36`、`09:45:21` 通过。上述证据完成本批定向场景验收；历史 RISK-4 已由 Audit5 `R-HC-05` 关闭，但这些定向证据仍不能替代任意对局、全输入 production certificate。T8 默认 `stage.dat` 部署仍暂缓。
