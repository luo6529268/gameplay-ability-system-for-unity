# NTSD C++ Release → Unity 战斗场景重新对齐计划

> 建立日期：2026-08-20  
> 当前状态：**批准的正常战斗对齐目标已完成：R2～R8与R11/R12均达到各自要求的Unity证据层，正常战斗主线暂无新的production可达、source-confirmed且Unity未实现脚本差异；正式state2000/state8xxx残余验收与模式配置F7/F8/F9均已通过。R1-WP02 full trace仍BLOCKED，T8及用户排除项继续保留，不能宣称C++ executable runtime完整动态trace认证。**

> 2026-08-24补充：用户授权对可取得的残余样板执行验收并新增模式配置F7/F8/F9。正式资源恢复后重新盘点确认
> C++/Unity均存在8个authored state8xxx frame，旧“loaded data无authored state8000”结论已过期；由
> `R8-WP01G-R11`补production full-tick witness。`R8-WP01G-R12`只实现F7/F8/F9，按GameConfig模式白名单且仅
> LocalFreeRun物理捕获；F1/F2、A→B→C、AI C++ parity、T8、Android、服务器、IL2CPP仍排除。
> 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 live battle runtime。  
> C++ 正式入口：`src/entity/game_tick.cpp` 的 `game_tick(...)`。  
> C# 工程用途：历史移植辅助、命名线索、已有 Unity 自检回归；与 C++ release live path 冲突时不具备裁决权。
> 证据状态：C++ 自动化 full trace 的只读获取仍为 `R1-WP02：BLOCKED`；它是增强定位能力，**不是**开始 C++ 源码行为合同与 Unity 差异盘点的前置门槛。

## 1. 目标

在**保留** Unity 已完成架构和性能成果的前提下，使 Unity 战斗场景的逻辑时序和最终可观察表现重新对齐 C++ release：

- 保留 `SimulationTickDriver -> NTSDBattleTickSystem -> SimulationWorld`；
- 保留固定 30 Hz、`FrameInputSet`、slot/generation、SoA/ECS store、对象池、中央渲染、Texture2DArray、worker、0 GC 和 1000 AI 性能能力；
- 不将 Unity 改写为 C++，不引入 Unity DOTS，不回退到逐实体 `SpriteRenderer`；
- 不因性能优化而改变 C++ 的 pass 顺序、slot cursor、RNG、输入边沿、命中、opoint、持有/抓取、生命周期或 render handoff；
- 每次只处理一个闭合模块：先完成 C++ 源码行为合同和 Unity 差异盘点，再按可用条件完成代码级、定向运行时、集成和可选 trace 验收；不能独立运行的子流程应标记为“待测试”，不得伪造已验证。

这里的“对齐”同时包含：帧号、位置、速度、朝向、命中、伤害、关系字段、对象生成/回收、声音/火花事件时序，以及战斗中实体、阴影、挂点和层级的最终可见结果。

本计划不要求像素级截图完全相同。C++ 负责裁决战斗规则、逻辑 render handoff 与可观察排序/可见性；Unity 可以保留为实现这些结果所需的 Unity-native 渲染和资源管线。

## 1.1 Unity 已交付边界（对齐中不可回退）

下表是用户已确认的 Unity 交付需求。它们是 C++ 行为对齐的实现边界，不是可被“机械模仿 C++”推翻的历史实现细节。

| 领域 | 必须保留的 Unity 需求 | 对齐中禁止的回退 |
|---|---|---|
| 中央表现 | 保留 `BattleCentralRenderSystem`、集中 command/descriptor、中央 Mesh/批次提交、Texture2DArray/atlas、动态 quad/Mesh 与 URP 接入。`CentralOnly` 必须继续是受支持的生产表现路径。 | 不得因为 C++ 使用不同 renderer API 而恢复逐实体生产用 `SpriteRenderer`，或把中央表现降级为仅性能实验。 |
| Legacy 表现 | Legacy `SpriteRenderer` 仅可保留为兼容、fallback、诊断或对照手段；其存在不能成为中央路径遗漏实体、阴影或挂点的理由。 | 不得把 Legacy fallback 当成 R6/R8 的最终显示依赖，也不得为绕过中央路径问题长期双画。 |
| render 对齐尺度 | 对齐 C++ 的逻辑 render handoff、`(z, slot)` 稳定顺序、entity/shadow/hit-record descriptor、可见性、挂点、阴影和局部 offset。 | 不要求像素级截图一致；不得把 Unity Mesh、材质、Camera、Texture2DArray 或 URP 回调当作 C++ 的逐行替代物。 |
| `Authority400` | 固定 400 runtime slot 仅用于 C++ 同槽对照、诊断和兼容夹具；该 profile 必须保持“恰好 400”的约束。 | 不得把 C++ 的 400-slot 设计推广为 Unity 所有生产 profile 的实体上限。 |
| `MobileExtended` | 保留初始 1,050 runtime slot、最多 1,000 active runtime entity 的移动端容量合同。 | 不得因 C++ 对照或单个性能回归把移动端 active 上限降回 400。 |
| `DesktopExtended` | 保留无固定产品级 active cap（`int.MaxValue`）、按 page 归一化的每局有限容量，以及 unsealed loading/reset/preflight 边界的 generation-aware 预战扩容；默认 512 只是 reservation hint。 | active battle seal 后容量冻结以保证 strict 0 B，超预算时确定性拒绝；不得加入固定产品实体上限，也不得把“无固定产品 cap”误写成 tick 内数学无限增长。 |
| 战斗核心与性能 | 保留 `SimulationTickDriver -> NTSDBattleTickSystem -> SimulationWorld`、30 Hz、`FrameInputSet`、slot/generation、SoA/ECS store、对象池、worker 与战斗期间零 GC 目标。 | 不得用 `Transform`、渲染帧或 Animator 反写战斗真值；不得为了临时行为修复拆除已验证的池、数据导向热路径或容量机制。 |

若某项 C++ 行为合同确实要求 Unity adapter 改动这些边界，必须先明确写出：C++ 可观察规则、现有 Unity 边界、最小适配方式、非回退证明和验收条件；不得直接以大范围渲染/容量回退替代分析。

## 2. 当前已知结构问题

当前 Unity 架构并非整体失效；问题是它的实现和历史验收主要以 C# 基线组织，而 C++ release live `game_tick(...)` 的实际顺序存在关键差异。首先必须重新收口的差异是：

| C++ release live pass | Unity 当前对应位置 | 当前风险 |
|---|---|---|
| cooldown → post-cooldown input | cooldown → HumanInput → CharacterInput | 输入、冷却到期与组合键的同 tick 可见边界需重验 |
| early state 400/401/500/501 → frame logic → frame advance | EarlyFrameAdvance → FrameLogic → FrameAdvance | 看似对应，但需要按 C++ slot scan 与字段 trace 重验 |
| 第一次 Z clamp + step5 held/link | Z clamp → `PreInteraction` | Unity 的 CPoint/WeaponSync 被提前 |
| candidate collect → character collision → random weapon → object collision | candidate collect → character consume → random weapon → object consume | 前置 CPoint/held 改写可能改变采集与消费 |
| CPoint → weapon sync → 第二轮 held/link | `PreInteraction` 与 `HeldObjectProcess` 分散在候选前 | 抓取、持有物挂点、投掷与 release 时点高风险 |
| PreFrame/camera/bg → render handoff → postprocess → late → tail | PreFrame/Stage → RenderDispatch → postprocess → late → tail | 逻辑部分接近；camera/perspective 与中央表现 handoff 需重新对照 |

权威 C++ 源坐标：

- `game_tick.cpp:945`：tick 开始、cooldown、input；
- `game_tick.cpp:1247`：frame logic/advance；
- `game_tick.cpp:1423`：第一次 Z clamp 与 held/link；
- `game_tick.cpp:1645`：候选收集、角色 collision、随机武器；
- `game_tick.cpp:1821`：对象 collision、CPoint、weapon sync；
- `game_tick.cpp:1848`：第二轮 held/link；
- `game_tick.cpp:2021`：PreFrame、render handoff、postprocess、late/tail。

## 3. 全过程硬约束

1. C++ debug/probe 可以记录 live runtime，但不能替代 release 规则；规则只从 release 调用路径和参与构建的模块读取。
2. 不接受“Unity self-check PASS”单独作为 C++ 对齐完成证据。
3. 不接受“1000 AI / 0 GC / checksum 一致”单独作为 C++ 对齐完成证据。
4. 不做一次性重写；每次提交只允许一个已经在 R1 差异清单中闭合的模块，以及它必要的回归夹具。
5. 每个 optimized path 必须保留同 tick fallback 或 shadow 对照入口。若 full trace 不可用，可依据 C++ 源码合同、focused test 和定向运行时验证继续修复，但只能标为相应证据等级，不能把它提升为“已获得 C++ full-trace 证书”。
6. 任何 fast path 发现首个 C++ observable mismatch 后，立即停止该路径的进一步推广；先保存 witness，再修正对应模块。
7. Unity 的 Transform、Camera、Mesh、Sprite、Animator、URP 回调都只能读取逻辑/表现快照，不能反写战斗真值。
8. `Authority400` 是 C++ 同 slot 对照/诊断 profile，必须保持恰好 400 slot；`MobileExtended`（1,050 slot / 1,000 active）和 `DesktopExtended`（无固定产品 active cap、unsealed prebattle page reservation、sealed battle strict 0 B、超预算确定性拒绝）是必须保留的 Unity 交付 profile，不能被 C++ 400-slot 设计回退。

## 4. 分阶段实施

### R0：权威迁移与历史证据分级

**目的**：终止“C# self-check 通过 = C++ 对齐”的误判，不改战斗行为。

- 根 `AGENTS.md`、主要对齐/架构/渲染文档统一声明 C++ release live path 为唯一 authority；
- 旧 C# 结论标记为“历史回归证据”，不删除，不篡改当时事实；
- 建立本文件作为后续唯一执行顺序；
- 盘点所有 Unity `C# authority` 注释、C# trace、fast-path proof 和旧对齐条目，按“可复用回归 / 必须重审 / 已明确与 C++ 冲突”分类。

**完成条件**：未来任务不会再把 C# 作为最终裁决；不涉及 Unity 代码或 C++ 代码改动。

### R1：C++ 源码行为合同与 Unity 全量差异盘点

**目的**：先从 C++ release live source 建立完整、可审计的行为合同，再在 Unity 中逐项映射和登记差异；避免凭旧 C# 结论、单个技能现象或不可用的自动 trace 猜规则。

R1 的必经主线是：

`C++ live source contract → Unity source-pass crosswalk → 差异清单 → 子流程验收矩阵`

这四项完成后，才允许对某个已闭合模块实施 gameplay 修复。自动化 full trace 是加强 first-difference 定位的可选证据，不替代也不阻断该主线。

#### R1.1 C++ release live-source 行为合同（必需）

只读追踪 `J:\QQFile\NTSD2.4\ntsd_release` 中实际参与 `ntsd_new.exe` release 构建的调用链。每个主流程/子流程必须记录：

- release build 参与性、入口、调用者、被调用者和字段定义；
- 前置状态、slot 扫描顺序、分支/early-return 顺序、常量、整数/浮点转换与写回点；
- 读写字段、RNG/slot cursor/统计/生命周期副作用；
- 对象生成、held/link、collision、opoint 与 render handoff 的可观察边界；
- 所属模块、依赖模块、最小重现输入，以及该子流程能否独立运行测试；
- 证据等级：`VERIFIED`（C++ release live source 或实际 release 证据）、`INFERRED`、`UNKNOWN`。

任何未能从 C++ release live path 闭合的规则必须保持 `UNKNOWN`；旧 C#、Unity self-check、Authority400、hash 或性能报告只能辅助定位，不能补写规则。

#### R1.2 Unity source-pass crosswalk 与差异盘点（必需）

对 R1.1 的每条 C++ 合同，在 Unity 中定位对应的：

- `NTSDBattleTickSystem` / `SimulationWorld` pass；
- runtime entity、slot/generation、SoA store、对象池和关系字段；
- 输入、frame/physics、candidate/consume、CPoint/WeaponSync、held/link、opoint/lifecycle adapter；
- 中央表现的 command/descriptor handoff，而非把 Mesh、Camera Transform 或 Legacy `SpriteRenderer` 当作逻辑真值。

每个条目必须写入差异清单，并使用以下状态之一：

| 状态 | 含义 |
|---|---|
| `待盘点` | C++ 调用链或 Unity 映射尚未闭合。 |
| `待处理` | 已确认 C++ 行为合同与 Unity 代码逻辑不同。 |
| `逻辑已对齐，待测试` | 代码级对照已完成，但仍缺依赖模块、夹具或场景验收。 |
| `已验证` | 已具备与风险相称的定向/集成证据。 |
| `UNKNOWN` | 证据不足，禁止把推断升级为规则。 |
| `不适用` | Unity 实现不同，但经合同证明不改变 C++ 要求的逻辑结果或可观察表现。 |

盘点阶段不得混入 gameplay 修复；它的交付物是完整的差异与依赖图，而不是“边找边改”的局部补丁。

#### R1.3 子流程验收与测试合同（必需）

每个待处理差异必须先拆为可闭合的子流程，记录：

- C++ authority/evidence、Unity 映射、前置条件、输入与预期状态变化；
- 代码级验收标准、可独立运行的 focused test、自检和 Play Mode 验收条件；
- 不能独立测试时的依赖流程、联合验收入口及 `待测试` 原因；
- 修复允许影响的 Unity adapter 与禁止触碰的 1.1 交付边界；
- 回归 fixture、失败时应输出的字段/slot/tick 信息。

只有该合同写清后，才可以把对应子流程排入 R2～R6 的实际修改批次。

#### R1.4 C++ release read-only full trace（可选增强，R1-WP02）

从**未修改的 C++ Release runtime** 以只读方式获取 trace，并在**非 authority 目录**中保存采集结果和比较资料。不得修改 C++ 源码、构建文件、可执行文件、资源、配置或 C++ 工程目录中的输出文件。

完整 trace 的目标 schema 可覆盖 tick、RNG、slot、frame、位置/速度、关系字段、candidate/consume、opoint/lifecycle 及 render descriptor，并可报告 `tick / pass / slot / field / C++ value / Unity fallback value / Unity optimized value` 的 first difference。

当前 `R1-WP02：BLOCKED`：尚未确认一个既有、可重复、无 authority 写入、可固定输入且能覆盖 full schema 的外部观察方式。不得以 C++ instrumentation、hook、注入、patch、重建或新增 trace sink 绕过该 blocker。

此 blocker **只阻断自动化 full-trace/comparator 链路**；不阻断 R1.1～R1.3 的 C++ 源码合同、Unity 静态映射、差异登记和验收设计。

**R1 完成条件**：已形成覆盖战斗主流程的 C++ 源码行为合同、Unity 完整差异清单、模块依赖图和分层验收矩阵；没有任何 gameplay 修复混入 R1。自动 full trace 若仍不可用，必须保留为 BLOCKED，而不能虚报为已完成。

#### R1 source inventory 执行状态（2026-08-21）

| Work Package | 范围 | 当前状态 | 关键产物 / 边界 |
|---|---|---|---|
| R1-SOURCE-001 | `game_tick(...)` 主 tick、Unity scheduler crosswalk | 已完成（静态 source） | T00–T18 与 D-SCHED-001～012；无 gameplay 修改。 |
| R1-SOURCE-002 | post-cooldown input、human/AI、combo、F1/F2 gate | 已完成（静态 source） | D-SCHED-005/010、D-INP-001～006；无 runtime 结论。 |
| R1-SOURCE-003 | frame advance、physics、移动、落地、状态/死亡维护 | 已完成（静态 source） | D-MOV-001～005 已登记；无 runtime 结论、不得改 Unity scripts。 |
| R1-SOURCE-004 | candidate collect、collision/hit、抓取/武器交互 | 已完成（静态 source） | D-COL-001～005、D-HIT-001～003 已登记；CPoint/held/lifecycle consumer 移交 005。 |
| R1-SOURCE-005 | CPoint、held/link、opoint、生命周期 | 已完成（静态 source） | D-SCHED-004、D-LINK-001～002、D-HOLD-001～002、D-CPT-001～002、D-OP-001；无 runtime 结论。 |
| R1-SOURCE-006 | C++ render handoff、Unity 中央表现可观察合同 | 已完成（静态 source） | D-RENDER-001～005、A-RENDER-001～004 已登记；保留 CentralOnly/Texture2DArray/Mesh/URP，不回退 Legacy production renderer。 |
| R1-SOURCE-007 | 全量汇总、依赖图、R2–R6 子流程验收矩阵 | 已完成（静态 source inventory closure） | 已形成唯一差异台账、依赖图、future repair batches 与验收矩阵；R2 仍待用户确认。 |

该表表示**盘点进度**，不表示任何 gameplay 已对齐、编译通过或 Play Mode 已验证。

R1-SOURCE-005～007 的 Task Contract 和全路径覆盖矩阵已经预先建立，以防止后续会话只围绕
已暴露技能现象工作；当前 005、006、007 均已完成静态审计，R1 现为“静态差异盘点完成 /
运行时验收待后续”，不是 gameplay 已完成对齐。覆盖范围与完成门槛详见
`docs/ai/RESEARCH/R1-SOURCE-INVENTORY-COVERAGE-MATRIX.md`。

### R2：主调度器与 pass 边界对齐

**目的**：先解决会放大所有技能差异的顺序问题。

**进入条件**：R1 已完成“主调度器”范围的 C++ 源码行为合同、Unity pass crosswalk、待处理差异和子流程验收合同。R1-WP02 的自动 full trace 是否解除 blocker 不作为进入条件。

- 以 C++ `game_tick(...)` 的真实顺序重建 `NTSDBattleTickSystem` 的 pass map；
- 重点重审并按 C++ 调整：第一次 Z clamp、step5 held/link、candidate collect、角色/对象 collision loop、随机武器、CPoint、weapon sync、第二轮 held/link；
- `PreInteractionTickAll` 不再因为历史命名而拥有超出 C++ 时点的 CPoint/WeaponSync 副作用；必要时拆为 C++ 同名的无分配子 pass；
- 保留 `SimulationTickDriver`、worker、FrameInputSet 和 phase diagnostics；只改调度时点及对应的 Unity adapter；
- 先运行 legacy/完整扫描路径，再启用 proof/skip path 逐项 A/B。

**完成条件**：普通输入、空场、单角色、角色+武器、角色+技能对象五种夹具均已按 R1 验收合同完成代码级与可运行测试；不能独立运行的项保持“逻辑已对齐，待测试”，并在依赖链闭合后完成联合验收。若 full trace 已可用，再额外要求其没有主 pass 顺序分叉。

#### R2-PASS-01 执行状态（2026-08-21）

`R2-SCHED-001` 已仅调整 Unity `NTSDBattleTickSystem` 的 T09～T16 调度，并更新同范围
`BattleRuntimeSelfCheck`。已用当前 Unity Editor 的 UnityMCP 强制 scripts refresh/compile，Console
返回 0 error，刷新后 self-check 实际返回 PASS。静态顺序已确认 first clamp → held#1 →
snapshot/pair/candidate → character/random/object consume → candidate cleanup → CPoint/WeaponSync →
positive link → second clamp → held#2。

这只关闭 D-SCHED-001～004 的 scheduler 调整、编译和 focused self-check 层；CPoint/held/link/
candidate 公式、R4/R5 joint fixture、Play Mode 以及 C++ full trace 仍未完成。D-SCHED-005/010
没有修改，继续归 R3 输入批次。

#### R2-PASS-02 执行状态（2026-08-21）

`R2-SCHED-002` 仅处理 D-SCHED-011 的 **mode2 tail reset 时点**。C++ source contract 是
mode2 tail → entity postframe tail → `g_game_mode2=0`；Unity 已把 `Mode2Request` 的 clear 从
`Mode2RandomWeaponDropTailAll` 移至 entity postframe tail 后、results flow 前。现有 Unity
Editor 的 UnityMCP scripts refresh/compile 后，C# `error CS` 过滤结果为 0，request 驱动的
`BattleRuntimeSelfCheck` 于 22:58:30 返回 `PASS`。

这只构成该子边界的代码级证据，状态为 `RUNTIME_PENDING`。`g_init_stats` / F7、D-SCHED-006～010/
012、F8/F9 Play Mode、joint fixture 与 C++ full trace 都没有关闭；中央渲染、容量、1.5× visual
scale、fixed-world camera 等受保护 Unity 适配均未改动。

#### R2 验收覆盖审计（2026-08-21）

已完成现有 `BattleRuntimeSelfCheck` 对 R2 的只读覆盖盘点，详见
`docs/ai/RESEARCH/R2-ACCEPTANCE-COVERAGE-AUDIT-20260821.md`。现有测试确实覆盖 empty tick、
single-character cooldown/human poll、two-held pass、candidate→CPoint、Z clamp、mode2 tail 和一条
production skill-object 回归；但它们是分散夹具，尚未形成同一 tick 的 R2 scheduler joint witness。

因此 R2 不能因多个 focused PASS 写成完整联合验收。后续若授权 test-only `R2-VERIFY-01`，必须先
建立独立 Change Record；不得借测试基础设施修改 gameplay writer，也不得把 R3 的输入改动混入。

### R3：输入、帧推进、移动与物理

**目的**：恢复最直接影响战斗手感的基础链。

- `post_cooldown_input`、human poll、AI input、组合键、按下/按住/释放、输入清理 gate；
- state 400/401/500/501、frame logic、frame advance、wait、next、turn、frame delay；
- 跑步、跳跃水平动量、空中速度保留、地面摩擦、重力、落地、Y/Z/X 整数同步；
- state14、死亡、复活、9998 清理与特殊 OID runtime maintenance；
- 明确哪些 Unity 的 `CharacterInput`/SoA writer 可保留，哪些必须把 C++ slot 时点恢复后才能继续使用。

**完成条件**：走、跑、跳、转向、受击硬直、死亡/复活均满足已闭合的 C++ 源码行为合同，并通过可用的 focused test 与真实 Play Mode 帧号/体感复核；full trace 可用时再要求 Unity fallback/optimized trace 一致。不可独立复现的分支必须保留“待测试”状态。

#### R3-INP-01 执行状态（2026-08-22）

已按 R3 的第一个最小依赖包准备 `D-SCHED-005` 的 source contract：C++ `game_tick(...)` 先完成
完整 `post_cooldown_input` callback（P1/P2 poll、AI prepare、全部 active character 的
`apply_input`），之后才进入 OID 7/8/51 maintenance；Unity 当前 maintenance 位于 human poll 和
`CharacterInput` 之间。详细范围、entry-clear preservation、fixture、停止条件和保护边界见
`docs/ai/TASKS/R3-INP-01-callback-pass-boundary.md`。

`R3-INP-001` 已在脚本修改前建立 Change Record，随后只修改 `NTSDBattleTickSystem` 和
`BattleRuntimeSelfCheck`：Unity normal path 已改为 `HumanInput → CharacterInput → OID maintenance`，
而 `NeedClearInput` branch 保留 `OID maintenance → clear → return`。本地 static order check、
scheduler-driven OID7/8 fixture、UnityMCP compile（filtered `error CS`=0）与 request self-check 已通过；
R3 joint fixture、Play Mode 与 C++ trace 仍待执行。`NeedClearInput` / F1/F2、held/caught、dead AI、packet edge 和物理键位均仍属于
后续独立包，不能混入本次 callback-order 修复。

#### R3-INP-02 计划状态（2026-08-22）

已完成只读 preflight，并以 `docs/ai/TASKS/R3-INP-02-step-gate-vs-entry-clear.md` 固化最小范围。
Unity 已有并会快照 `BattleStepMode` / `BattleStepGate`，但 scheduler 尚未每 tick 生产它们，亦未在
RenderDispatch 后实现 C++ F1 wait return。下一脚本包只会处理 default `g_dword_449048=0` 的
F1/F2 core gate 与 `NeedClearInput` 分离；negative-link input、dead/respawn AI、packet edge / physical
binding 已按 `D-010` 拆入独立后续包。`R3-INP-002` Change Record 已在改动前建立，scheduler / fixture
已写入；local static order、UnityMCP compile（filtered `error CS`=0）与 F1/F2/entry-clear request
self-check 已通过。physical binding、debug-unlock、Play Mode 与 C++ trace 仍待后续。

#### R3-HOLD-INP-01 执行状态（2026-08-22）

`D-INP-001` 的只读 preflight 已完成：C++ `main.cpp` 的 callback 会为每个 active current
character DAT 调用 `apply_input`，而 `input_handler.cpp::apply_input` 没有 `link_state < 0` 的函数级
return；Unity 的 `LF2Entity`、`LF2Character` 与 exact-AI input pass 却有这一总体 skip。已建立
`R3-HOLD-INP-01` Task Contract 与 `R3-HOLD-INP-001 / RUNTIME_PENDING` Change Record。已移除
character input eligibility gate，并新增 valid negative-relation 的 real-character / shared-DAT focused
fixture；local static、UnityMCP scripts compile（filtered `error CS`=0）和 request self-check 均通过。
negative-link frame advance、held/CPoint、link cleanup、collision/opoint/lifecycle、dead AI 与 physical
binding 继续保持独立，不得因本包 PASS 写成完整 relation runtime 对齐。

#### R3-AI-LIFE-01 执行状态（2026-08-22）

`D-INP-002` 已由 `R3-AI-LIFE-01 / R3-AI-LIFE-001` 完成最小代码闭环，状态为
`RUNTIME_PENDING`：C++ `main.cpp` caller 和 `prepare_ai_input` 都没有 AI self-HP overall filter，且
input 在 frame advance / state14 respawn cleanup 前；Unity legacy core、indexed `AiDecisionKernel` 以及
`AiSensingKernel.TryFindNearestCore` 的三个 self `HP <= 0` eligibility gate 均已移除。首次
data-oriented fixture 实际揭示第三 gate 会在 no-target contract 前导致 unified-authority fallback；
Record 先扩展后才改动。source/static、现有 Unity Editor 的 scripts compile（filtered `error CS`=0）和
dual-profile `BattleRuntimeSelfCheck` 均 PASS（结果文件 2026-08-22 01:02:51 +08:00）。target/RNG policy、
death/respawn、frame advance、held/CPoint/link、Play Mode、C++ runtime trace 仍不得被该 PASS 覆盖。

#### R3-INP-03A 执行状态（2026-08-22）

`D-INP-003` 已按 `D-011` 从原本过宽的 R3-INP-03 拆为独立的 full-held packet adapter contract，且
`R3-INP-03A / R3-INP-003A-001` 已到 `RUNTIME_PENDING`：C++ `InputHandler::poll` 只在每 tick 读取
current held state，再固定顺序派生 prev/cooldown/history；Unity `FrameInputSet` 虽保留
pressed/released metadata，但正式 application 同样只消费 `Buttons` 的 complete packet 并重建 edge。
test-only fixture 已验证 multi-key press、same held、full release；source/static、UnityMCP compile
（filtered `error CS`=0）和 full self-check PASS（结果文件 2026-08-22 01:20:32 +08:00）。physical asset、
8-slot extension、AI target、worker/protocol、C++ trace 都保持独立，不能由该结果关闭。

#### R3-INP-04 执行状态（2026-08-22）

`D-INP-004` 已由 `R3-INP-04 / R3-INP-004-001` 完成 fixed P1/P2 authority subset，状态为
`RUNTIME_PENDING`：battle scene 中的 C++ P1/P2 是 runtime object slot0/1，且仅它们进入 physical poll；
Unity 8-slot roster保持保护的 extension，而 player slot0/1→runtime slot0/1 的 same-tick P1 right /
P2 jump fixture 已验证无串键、stable binding正确。source/static、UnityMCP compile（filtered `error CS`=0）
和 full self-check PASS（结果文件 2026-08-22 01:28:23 +08:00）。capacity、pool slot、AppManager、
physical binding、3+ extension或C++ runtime trace均未被该结果关闭。

#### R3-AI-TGT-01 执行状态（2026-08-22）

`D-INP-005` 的只读 source preflight 已闭合，并已先建立
`docs/ai/TASKS/R3-AI-TGT-01-fallback-indexed-target-contract.md` 与
`R3-AI-TGT-001 / RUNTIME_PENDING`。C++ `InputHandler::prepare_ai_input` 的 ground/air target scan、strict
low-slot tie、`unk_360` cache和team/input-phase predicate都已定位；Unity fallback、SoA/indexed与decision
kernel也都有对应静态路径。fixed-seed legacy/data-oriented profile-pair fixture已在
`BattleRuntimeSelfCheck` 通过：同距、air override、cache retain/refresh、team/phase、input signature和RNG
state/call count均一致。中间两次fixture失败完整记录为canonical-store/runtime-mirror initial-state setup，
未修改production AI。final UnityMCP filtered `error CS`=0，self-check result于2026-08-22 01:53:46 +08:00为
PASS。真实AI Play Mode和C++ runtime trace仍未关闭，故本条不是完整AI target对齐证书。

#### R3-FRAME-01A 执行状态（2026-08-22）

原 `R3-FRAME-01 / D-MOV-001～003` 已按 D-012 拆为三个可回滚的子包，避免把 current-key、landing
raw-frame writer和respawn integer-sync混为一次大改。当前已创建
`docs/ai/TASKS/R3-FRAME-01A-current-key-lifetime-contract.md` 与
`R3-FRAME-001A / RUNTIME_PENDING`：C++ `InputHandler::poll` / AI producer、`game_tick(...)` 的 input→frame-advance
顺序，以及 `frame_advance.cpp` F03/F09 current-key consumers已重新阅读；Unity 的
`SimulationWorld.SerialTickAll` 在F03前清 current key是明确静态差异。最小脚本 diff已写，首次全量
self-check暴露一处旧 AUDIT6 current-key clear assertion；已定位为本包同一 test-only fixture并更正。existing
Unity Editor的force scripts refresh后filtered `error CS`=0，重跑full self-check在 **02:22:20** PASS。
实现只在该任务合同的三条脚本路径内进行，也没有把旧 C#-authority self-check当作裁决。

本包只允许恢复 current key 至 F03/F09 的可见性，并将 self-check改为验证 held edge/history、transit
visibility、frame212 direction velocity和battle-entry clear保持。`D-MOV-002`已由独立
`R3-LAND-01 / R3-LAND-001` 完成代码级最小闭环，状态为 `RUNTIME_PENDING`：C++ F04 landing direct-frame →
candidate → F07 late-frame-tick 时序、Unity exact/shared character-DAT `ImmediateFrame` side effect与
branch-specific attacking writer已闭合；两个 landing blocks不再用 `ImmediateFrame`，新16-case self-check
验证 frame/PN/attacking/wait中间态。首次 CS0136变量遮蔽已修复，final existing Unity Editor refresh/compile和
full self-check于 **02:42:40** PASS。真实落地表现、Play Mode与C++ runtime trace仍未关闭。`D-MOV-003`已由
`R3-SYNC-RESP-01 / R3-SYNC-RESP-001`完成代码级最小闭环，状态为 `RUNTIME_PENDING`：C++仅在成功
physics tail同步 integer position，F03 delay/link/kind2 early return不写；F05 no-count respawn随后读取
active same-relation character-DAT的integer x/z。Unity respawn前无条件 global sync已删除；新的 four-path
stale-int fixture验证 early-return integer保留与同一 RNG offset的 respawn average。existing Unity Editor
refresh/compile后full self-check于 **03:01:32** PASS。real respawn Play Mode、C++ runtime trace以及其他
direct-position writer audit仍未关闭，故不得把本条写成完整 movement/respawn对齐。

#### R3-FRAME-02 reachability 状态（2026-08-22）

`D-MOV-004/005` 已先完成 C++ release source、Unity reader/writer以及当前 DAT inventory的只读调查。
`D-MOV-004` 是当前唯一可实施项：C++ compiled source中 `throw_frame_guard` 没有 conditional read也没有
nonnegative writer，Unity却在 F03、exact F07和fallback F07以 matching frame early return；
`R3-FRAME-02A / R3-FRAME-002A-001` 已在改脚本前建立，且只允许移除这三处 reader并加 exact/shared fixture。
`D-MOV-005` 已由`R8-MOV-005-001`重开并完成exact代码闭环：C++ state2000 facing rule与Unity fallback原已
等价；exact current-type0 pass现也在C++对应时点按Vx写朝向。focused正/零/负Vx矩阵1/1、fresh compile、
16:14:46 full self-check及validator均PASS。current literal state2000仍只在type2/type4 fallback可达，故真实
type0 Play与C++ trace未关闭，状态保持`RUNTIME_PENDING`。

`R3-FRAME-02A` 已完成最小代码闭环并保持 `RUNTIME_PENDING`：three readers已删除，matching test-only
value的 exact/shared F03 physics-tail与F07 counter fixture通过；existing Unity Editor refresh/compile后的
full self-check在 **03:15:33** PASS。未做 real held-throw Play Mode，也没有 C++ runtime trace，因此不能把
该 PASS 扩大为完整 held/release或R3验收。

### R4：碰撞候选、命中、抓取与武器交互

**目的**：恢复“同 tick 谁先命中、命中几次、谁被抓/拾取”的规则。

- C++ `collision_collect` 采集时点、slot 顺序、几何/team/type gate、candidate freeze；
- step7 character loop 与 step9 object loop 的消费顺序；
- kind 0/1/2/3/4/5/6/7/8/9/10/11/14/15/16 的 C++ live side effects；
- arest/vrest、attack exempt、multi-hit、abort、held weapon、拾取、投掷、破武器；
- `BruteForceSceneQuery`、Loose Quadtree 和 role-aware broadphase 只可优化候选发现，不能改变 C++ 最终候选序列。

`R4-COL-01 / D-COL-001` 已完成代码级闭环并保持 `RUNTIME_PENDING`：C++ `collision.cpp:57-65` 的
`vrest → hit_confirm2 character-target attacker abort → runtime ITR` 顺序已在 shared candidate runner复现；
exact/shared two-candidate abort/continuation fixture、existing Unity Editor compile和 **03:44:57** full self-check均
通过。该改动同时暴露旧 held-kind5 fixture中 holder 会先合法命中 held weapon并写 carrier的前置；fixture已用
test-only collect-time `AttackExempt` 隔离，生产 gate未加豁免。尚未取得 Play Mode / C++ runtime trace，不得把
本条扩大为整个 R4或完整战斗对齐。

`R4-COL-02 / D-COL-002` 已完成代码级闭环并保持 `RUNTIME_PENDING`：C++ `collision.cpp:69-79` 的
caught-cpoint / `hurtable` gate 已在同一 shared candidate runner按 C07-A之后、runtime ITR replacement之前的
位置复用；其 `return false` 只跳过当前 candidate，而不是中止 attacker。exact/shared kind0 two-candidate
continuation、`hurtable=1` positive control和kind6 direct-writer fixture均通过；现有 Unity Editor compile
的 `error CS`=0，**04:01:04** full self-check PASS。没有 C++ runtime trace或真实 cpoint Play Mode，故不得把
本条扩大为完整 R4或完整 battle alignment。

`R4-COL-03 / D-COL-003` 已完成代码级闭环并保持 `RUNTIME_PENDING`：C++ `collision.cpp:188-194` 在
kind5/4/9 的 local itr转换之后，以 target **current** state18/19执行 whole-attacker abort；Unity已在
shared runner的 runtime itr resolve后、任何 writer前复现该 `return true` sequence break。exact/shared
source-kind0 state18/state19、ordinary control和 source-kind4→runtime kind0/effect21 fixture均通过；现有
Unity Editor compile的 `error CS`=0，**04:16:17** full self-check PASS。没有 C++ runtime trace或真实
effect21 Play Mode，故不得把本条扩大为完整 R4或完整 battle alignment。

`R4-COL-04A / D-COL-004` 的 candidate-collection 子范围已完成代码级闭环并保持 `RUNTIME_PENDING`：C++
`collision_collect.cpp` 没有 oid999 / transition-smoke global filter，Unity normal与role-aware frozen collection的
额外 `IsPureTransitionSmoke`排除已移除。synthetic valid oid999 target/attacker在 brute/role-aware formal collector
均记录同一 candidate / RNG；Unity compile的 `error CS`=0，**04:33:40** full self-check PASS。当前 production
gated frames无有效geometry依然只是DAT可达性事实。`QueryBodyHits` immediate-query 的 helper使用仍是 D-COL-004B
独立未处理子范围；D-COL-005、D-HIT和R5保持独立。

`R4-COL-04B / D-COL-004B` 的 source preflight确认 remaining immediate-query不是 frozen candidate
collection的同类遗留。Unity `LF2Weapon.OnLanded()` state13/high-speed landing会主动按 weapon BDY扫描 target并
直接写 `Hit`；C++ release `physics.cpp:228-320` 的 landing只写自身字段，正式 target interaction仍走
`game_tick.cpp` 后续的 collect/loop consume。04B已移除这条 active direct writer，并以“overlap target可被旧
query看到，但OnLanded不改其HP”的focused fixture验证；weapon自身 -100 HP、`Y=0/Vy=-3.5`仍保留。现有Unity
Editor compile的 `error CS`=0，**04:52:29** full self-check PASS。Unity `ProcessAttack`存在另一条held immediate
scan，但当前静态调用图无 production caller且held `Act` self-check证明不调用它，故仍为未修改的
`INFERRED` dormant。C++ trace与真实 Play Mode未做，04B保持 `RUNTIME_PENDING`，不得扩大为weapon/held 或R4完成。

`R4-COL-05A / D-COL-005A` 的 source preflight确认：C++ `collision_collect.cpp` / `collision.cpp`只限制
kind3和kind8 target为 character，kind1成功后进入通用 Entity `case 1` writer。Unity collector也已只限制
kind3/8，但 `BattleInteractionWriter.TryApplyGrab`额外将kind1/3一起限制为Character target，导致已frozen的
kind1 non-character candidate在consume阶段被错误拒绝。05A已把该 gate收窄为仅kind3，并以 frozen
candidate矩阵验证 kind1 LightWeapon-type target记录/消费后写 action、caught/catcher、duration和fall，kind3同类
target仍不记录。最终 Unity compile的 `error CS`=0，**05:05:17** full self-check PASS；首次测试CS0165已经最小
修复并留档。C++ trace / Play Mode未做，05A保持 `RUNTIME_PENDING`；non-character attacker / weapon kind1的
selector与pickup可达性另拆为 `D-COL-005B`，不在本包修改。

`R4-COL-05B / D-COL-005B` 已完成只读 source / asset-container preflight，结论为
`UNKNOWN / no gameplay change`，不能作为05A的完成扩张。C++ `main.cpp:5505-5523` 的正式 input callback
仅对 type0 character DAT写 human/AI input；`collision_collect.cpp:200-220`虽让kind1 selector直接读取
generic attacker key，而`collision.cpp:921-993`的case1确为generic grab，C++ pickup却在kind2/7。Unity weapon
kind1当前走pickup helper，因此存在潜在语义偏差；但正式 `chars/*.dat` 是VDC编码容器，在不运行、解码、复制或
修改authority的限制下，无法证明有可达的non-character kind1 asset或其key producer。该未知项已固定在
`docs/ai/RESEARCH/R4-COL-05B-noncharacter-attacker-kind1-preflight-20260822.md`，不阻断主线；下一项为
`D-HIT-001` 的独立 source preflight。

2026-08-23 `R8-COL-005B-001`更新：D-COL-005B通用代码差异已闭环。block-aware current Unity DAT扫描
确认`itr kind1=0`；但C++ generic runtime-key selector/case1 grab与Unity CLR-key gate/weapon case1 pickup的
source差异已按通用Entity合同修复。actual weapon attacker collect→ObjectInteraction→grab矩阵、fresh compile、
16:21:57 full self-check与75/75 validator均PASS。current production Play不可达、C++ trace BLOCKED，状态为
`RUNTIME_PENDING`，不得扩大为所有pickup/held已runtime VERIFIED。

`R4-HIT-01 / D-HIT-001` 已完成最小代码闭环并保持 `RUNTIME_PENDING`。C++ normal kind0的
type3 target在`collision.cpp:561-585 → hit.cpp:631-636 → hit.cpp:104-155`先获得公共HP、HP max、
受击combo与damage-stat写入，随后才进入type3 motion tail；kill attribution与holder attack combo有
type0-only guard。Unity `ApplySpecialAttackDamage`现以无分配type3专用vital/stat writer在tail前补齐上述
四项，不会泛化type0 score helper或改动weapon、candidate、CPoint、held/link、input、render。lethal focused
fixture还验证tail读取更新后的HP、且type0-only holder/global score不变；Unity compile `error CS`=0、full
self-check于**05:26:41** PASS。C++ trace、真实Play Mode和type3 lifecycle/identity联合验收仍未完成，故不得
扩大为完整type3或R4对齐。详细合同与证据为
`docs/ai/RESEARCH/R4-HIT-01-type3-normal-vital-stat-preflight-20260822.md`、
`docs/ai/TASKS/R4-HIT-01-type3-normal-vital-stat-contract.md`和`CHANGE-RECORDS/R4-HIT-001.md`。

`D-HIT-002` 的只读source preflight已完成，但**尚未把它写成“已修复”**。C++ kind10/11 character、kind16
character、normal weapon victim和normal weapon attacker都存在direct-frame写入，但它们后续显式字段不同；
Unity的`ImmediateFrame` / `SetFrameDirect`则附带PN、attacking或wait/transistor副作用。因此已建立
`RESEARCH/R4-HIT-02-raw-frame-writer-split-preflight-20260822.md`，并拆成四个不互相覆盖的最小包。当前
`R4-HIT-02A / RUNTIME_PENDING`已处理character kind10/11的`frame=182`：两条Unity resolver均改为保留PN、
attacking和wait counter的raw writer，exact/shared × kind10/11 fixture验证current frame、Frame.Data mirror、PN、
attacking、wait与既有stat。Unity compile `error CS`=0，full self-check在**05:43:54** PASS；C++ trace和真实Play Mode
仍未完成。`02B` kind16已完成唯一writer的raw-frame替换与actual/shared PN/wait/attacking fixture，Unity compile `error CS`=0、full self-check在**05:58:02** PASS，状态`RUNTIME_PENDING`；`02C` weapon victim已完成knockdown一处+tail四处raw-writer最小替换及type1/type4/type6/type2-ground/type2-air真实命中fixture，UnityMCP compile `error CS`=0、full self-check在**06:20:15** PASS，状态`RUNTIME_PENDING`；`02D` weapon attacker已将state3000 pre-knockdown/oid209 skipReset与state1002 later raw-write拆为局部writer，五类真实命中fixture通过，UnityMCP compile `error CS`=0、full self-check在**06:36:40** PASS，状态`RUNTIME_PENDING`。相关合同与审计记录为
`docs/ai/TASKS/R4-HIT-02A-kind10-11-character-raw-frame-contract.md`和
`docs/ai/CHANGE-RECORDS/R4-HIT-002A.md`，以及
`docs/ai/TASKS/R4-HIT-02B-kind16-character-raw-frame-contract.md`、
`docs/ai/CHANGE-RECORDS/R4-HIT-002B.md`，以及
`docs/ai/TASKS/R4-HIT-02C-weapon-victim-raw-frame-contract.md`、
`docs/ai/CHANGE-RECORDS/R4-HIT-002C.md`，以及
`docs/ai/TASKS/R4-HIT-02D-weapon-attacker-raw-frame-contract.md`、
`docs/ai/CHANGE-RECORDS/R4-HIT-002D.md`。

`D-HIT-003`现已进入`R4-HIT-003 / RUNTIME_PENDING`：C++ normal kind0的type1/type2/type4先按FallDamageDiv写
HP/HPBound/Combo/DamageStats、再写raw weapon durability；type6只走reaction durability，weapon lethal不能误写
type0-only kill/holder score。UnityMCP compile `error CS`=0、full self-check在**06:50:08** PASS。另发现Unity common weapon writer提前写`HitConfirm2/RelationTeam`，已登记为独立
`D-HIT-004`，不得和vital/stat改动混在同一包。详见
`docs/ai/RESEARCH/R4-HIT-03-weapon-vital-stat-preflight-20260822.md`、
`docs/ai/TASKS/R4-HIT-03-weapon-vital-stat-contract.md`和`docs/ai/CHANGE-RECORDS/R4-HIT-003.md`。

`D-HIT-004 / R4-HIT-004` 已完成normal weapon timing source preflight并进入`RUNTIME_PENDING`：C++先完成
`apply_hurt`/reaction、随后type1/type2/type4/type6 tail写`hit_confirm2`/relation；Unity已删除normal
`damageableWeapon`的early confirm，且只为non-damageable current-DAT shell保留existing early relation fallback，
normal四类型仍由existing tail首次写最终字段。four real `LF2Weapon.Hit` fixture、Unity compile `error CS`=0和
full self-check（**07:10:20** PASS）已通过。
CLR weapon shell被shared Character-DAT resolver以non-weapon current DAT分发至weapon hit的路径保持独立`UNKNOWN`，
不能借本包改dispatch。详见`docs/ai/RESEARCH/R4-HIT-04-weapon-tail-ownership-timing-preflight-20260822.md`、
`docs/ai/TASKS/R4-HIT-04-weapon-tail-ownership-timing-contract.md`和`docs/ai/CHANGE-RECORDS/R4-HIT-004.md`。

首次Unity验证的type2-ground fixture使用oid998，命中`Config/data.txt`定义的type5，故走non-damageable fallback；
该首次failure已保留，改用02C既有的无catalog-override test OID后通过。C++ trace、target Play Mode与shared dispatcher
差异仍未关闭，不得扩大为完整weapon/R4/C++对齐。

`D-HIT-005 / R8-HIT-005-001` 已完成代码闭环并进入`RUNTIME_PENDING`：C++ unified Entity始终按current
`char_data->obj_type`分发；Unity Character、shared Character-DAT、Weapon与SpecialAttack四类attacker现共享
current-DAT-first dispatcher。matching CLR壳继续使用exact weapon/special `Hit`，shell/current-DAT mismatch按
current type进入generic weapon/type3/type5 writer。fresh compile0、第三次full self-check及focused 178/178 PASS；
两次旧type3断言失败与C++ source修正已留痕。production mismatch Play夹具不可得且C++ full trace BLOCKED，
不得升级为runtime `VERIFIED`。

**完成条件**：同一 attacker 的多目标命中、抓取、拾取、投掷、落地与连续命中均有对应的 C++ 源码行为合同与 Unity 定向验收；优化查询与完整 slot scan 的最终 candidate/consume 序列必须相同。若可获得 full trace，再将其加入同 fixture 的附加证据。

### R5：CPoint、held、opoint 与生命周期

**目的**：处理当前最容易体现为“技能出了但后续不对”的链路。

- step5/step10/step12 的 held/link/cpoint 各自职责和时点；
- holder current frame/wpoint、held frame/facing/frame delay、center/wpoint 整数挂点公式；
- release、throw、cover、hit/release、holder/victim link 清理；
- late frame tick、late opoint、递归 opoint、多对象展开、newborn cursor、pending unregister、slot reuse；
- death opoint、state transition effects、N-30、broken weapon、postframe timer。

**优先夹具**：普通角色/武器/特殊对象的共享 held、throw、opoint、link 清理、slot reuse 链路，以及 oid122/123 持有物。历史上出现过的 Naruto 输入序列只能作为可选回归样本，不构成本计划的角色专项修复目标。

**完成条件**：每个夹具均已按 C++ 源码行为合同核对对象数量、slot、frame、位置、速度、link、first visible tick、攻击/命中链；可运行夹具完成定向验收，无法独立运行的项登记为“待测试”。full trace 可用时再进行自动 first-difference 对照。

#### R5-LINK-01 执行状态（2026-08-22）

`D-LINK-001 / R5-LINK-001` 已按C++ `game_tick.cpp:1828-1845`建立最小合同。C++ T11对invalid positive link只写
holder `link_state=0`；Unity Legacy、DataOriented和ShadowCompare expected原本额外清`TargetSlotIndex`与
`HeldWeaponStableId`。本包已仅在这三个writer/expected路径移除额外clear，并将self-check和focused Editor tests改为
断言前向字段保持、reverse target字段不变、slot顺序/positive-link index/event witness不变。Unity scripts refresh后的
`error CS`=0；2026-08-22 07:32:40 full self-check=`PASS`；focused EditMode job
`edc22b2fd5314fb685c59d1b04f97c7a`为8/8 PASS。因此状态为`RUNTIME_PENDING`：C++ trace与真实Play Mode仍未
取得，不得写为R5或C++ runtime已对齐。negative link、CPoint、held/release、opoint、slot lifecycle与pass ordering保持独立。

`D-LINK-002 / R5-LINK-002` 已完成只读source preflight并建立独立合同：C++ `game_tick.cpp:1441-1457`和
`1860-1872`的两轮negative-held scan在invalid child relation时都只清child `link_state`；Unity shared
`HeldObjectProcessAll`额外清`HolderStableId`。本包已只移除该一个extra clear，并写入out-of-range holder、
active-holder mismatch和second-pass no-reclear测试。Unity scripts refresh后的`error CS`=0；2026-08-22 07:46:36
full self-check=`PASS`；focused EditMode job `161af4674f524a388233e9e89865065c`为2/2 PASS。因此状态为
`RUNTIME_PENDING`：C++ trace与真实Play Mode仍未取得，不得写为R5或C++ runtime已对齐。不得将它与valid held
release/throw、CPoint/WeaponSync、D-SCHED-004或其它R5链路合并。

`D-HOLD-001 / R5-HOLD-001` 已完成dual-writer source preflight：C++两轮held scan都先写child
`frame_delay = holder.frame_delay`，type2 throw只写random frame/velocity/link而不覆盖delay。Unity generic
`BattleHeldObjectWriter`与real `LF2WeaponHeldStateResolver`都先复制holder delay、再写`FrameDelay=1`。本包已
只移除两处extra write，并将既有generic/real type2 throw夹具改为验证holder delay保持；Unity scripts refresh后的
`error CS`=0，2026-08-22 08:01:15 full self-check=`PASS`。因此状态为`RUNTIME_PENDING`：C++ trace与真实Play Mode
仍未取得，不得写为R5或C++ runtime已对齐。type2 spawner、release tick、random/PN/wait、CPoint和pass ordering保持独立。

`D-HOLD-002 / R5-HOLD-002` 已完成只读 source preflight并建立独立合同，状态为`RUNTIME_PENDING`。C++ release
两轮held `dvx` throw只在type1/4/6写`spawner_slot=holder slot`；type2相邻branch不写该字段。Unity real
weapon shared `ThrowHeldWeapon`却无条件写`SpawnerEntityIndex=holder slot`。本包已把当前helper的
spawner stamp显式传为type1/4/6=true、type2=false；type2保留进入branch前的值，未错误地改写为`-1`。existing
real held fixture已覆盖type1/4/6 stamp与type2 preseed sentinel保持；Unity compile `error CS`=0，2026-08-22
08:25:40 full self-check=`PASS`。C++ trace与真实Play Mode仍未取得，不能写为R5或C++ runtime已对齐。
现有type2 random frame、速度、link release、release tick、FrameDelay和render不在本包范围。C++ source表明
`spawner_slot`会进入后续target filtering；Unity也在current-DAT / hit_Fa=12 reader使用它，故该字段不应视为诊断残留。

预检还发现同一Unity helper无条件写`PickerStableId=holder slot`，而C++ normal pickup与两轮type1/2/4/6 held
throw均不写`picker_idx`；release-listed frame-advance target selection才是该字段的合法后续writer。该项已建立
`D-HOLD-003 / R5-HOLD-003 / RUNTIME_PENDING` 已按独立source合同从shared throw helper移除这一处extra write，并以
type1/2/4/6 sentinel fixture验证保持；Unity compile `error CS`=0、2026-08-22 08:39:22 full self-check=`PASS`。
C++ trace与真实Play Mode仍未取得，不能写为R5或C++ runtime已对齐。不得借`D-HOLD-002`一起删除、保留或改写它。

`D-CPT-001 / R5-CPT-001 / RUNTIME_PENDING` 已完成 source preflight、独立 Task Contract / Change Record、
最小 Unity script change、ledger/scoped diff、Unity compile及full self-check。C++ `cpoint.cpp`
relation/decrease/action 与 `weapon.cpp` current-frame held-vaction branches只写 frame / explicit字段；
Unity `BattleCpointWriter` 的七处 extra immediate reset已收窄为支持missing raw frame且保持
`Runtime.FrameWaitCounter` 的 direct writer。state9/action/negative action/held-vaction/decrease/
escape/mismatch fixture都以FWC sentinel通过；Unity compile `error CS`=0、2026-08-22 09:08:02 full
self-check=`PASS`。首次使用受guard的CPoint专用helper导致missing positive frame133被抑制，已在同一
合同内改用raw direct writer并留档。C++ trace与真实Play Mode仍未取得，不能写为R5或C++ runtime已对齐。
`D-CPT-002` injury stats与`D-CPT-003` reciprocal mismatch control flow保持独立，不借本包改动。

`D-CPT-002` global stats 的静态字段条件已进一步核对：C++ `weapon.cpp:50-75` 以victim
`unk_344` 的有效索引1/2写 global kill/damage arrays，Unity现有3-slot `KillStats` /
`DamageStats` 和normal-hit writer可以承载该契约。但预检同时确认新的 `D-CPT-004`：
C++ `cpoint.cpp` 不负责held injury / vaction / position，只有后续 `weapon.cpp` weapon-sync负责；
Unity `RunKind1` 却先调用 `SyncCaughtByCpoint`，later `SyncHeldCpoint` 又可能调用一次。action清
attacking后可使双伤害可达，action后非state9则可提前产生C++没有的伤害。因此先把D-CPT-004作为独立
phase-ownership子包完成；D-CPT-002不得直接在当前writer补统计数组。

`D-CPT-004 / R5-CPT-004 / RUNTIME_PENDING` 已完成独立source合同、最小代码、same-
`PreInteractionTickAll` 的no-action/action→state9/action→non-state9 matrix、ledger/scoped diff、
Unity compile和full self-check。Unity `RunKind1` 的early `SyncCaughtByCpoint` 已删除，current
`SyncHeldCpoint` 保持唯一held vaction/injury/position owner；existing shared-DAT CPoint test已改为
prev-frame无injury。去除early position后，decrease escape按C++ raw-position X=30/10正确为`-4`，
相关assertion已更新。Unity compile `error CS`=0、2026-08-22 09:27:38 full self-check=`PASS`。
C++ trace与真实Play Mode仍未取得；D-CPT-002 stats现可独立处理，D-CPT-003 flow继续排除。

`D-CPT-002 / R5-CPT-002 / RUNTIME_PENDING` 已在脚本改动前建立独立 source contract、Change Record、handoff
和 focused acceptance matrix。C++ `weapon.cpp:50-69` 要求 current held CPoint positive injury 对
valid `victim.unk_344=1/2` 写 global kill/damage stats；lethal kill stat 与 holder-local score并不共享
holder-existence gate，damage stat位于local combo之后。Unity的3-slot arrays和`Unk344` mapping已存在，
但 `BattleCpointWriter.ApplyHeldInjury` 尚未写它们。允许范围只限该 writer 和 CPoint self-check；
negative injury、already-attacking、invalid index不得写global stats，且不得改D-CPT-003、pass order、
held/link/opoint/input/collision/render/DAT/scene/array capacity或C++ authority。最小 writer已在既有
positive branch的 C++相对位置写入，shared lethal与six-case stat matrix已通过；UnityMCP scripts refresh后
error CS=0、full self-check于2026-08-22 09:44:35为`PASS`。C++ trace与真实 Play Mode仍未取得，故状态只能是
`RUNTIME_PENDING`；D-CPT-003 flow继续保持独立。

`D-CPT-003 / R5-CPT-003 / RUNTIME_PENDING` 已完成脚本改动前的独立 source preflight、Task Contract、Change
Record和handoff。C++ `cpoint.cpp:49-58` 对 active reciprocal mismatch或victim previous CPoint invalid
只写attacker frame0并设置skip actions/decrease；后续`throwvx` tail仍执行，且读取fallback current frame0的
geometry/next，最后仍可执行dircontrol。Unity `RunKind1` 目前direct return，故同时错误地跳过tail。允许范围仅为
该 control flow与focused fixture；不得改D-CPT-002/004、valid relation、kind2 validation、throw transform、
held/link/opoint/input/collision/render/DAT/scene/pass order或C++ authority。最小RunKind1 skip/tail writer与
focused mismatch matrix已通过；UnityMCP scripts refresh后error CS=0、full self-check于2026-08-22 09:59:58为
`PASS`。C++ trace与真实Play Mode未取得，D-CPT-005 escape tail也未处理，因此只能是`RUNTIME_PENDING`。

同次 source reread 新登记 `D-CPT-005`：C++ valid-relation decrease-negative escape写frame0/181和
skip-actions后仍可进入unguarded throw tail；Unity当前escape direct return。该项与active mismatch不同，
不得并入 R5-CPT-003 或 R5-CPT-004。现已建立 `D-CPT-005 / R5-CPT-005 / RUNTIME_PENDING` 独立
source contract、Change Record、handoff和focused fixture计划；允许范围只限RunKind1 escape tail与
BattleRuntimeSelfCheck。最小skip-actions/fallback-frame writer与immediate/postprocess/dircontrol fixture已写，
Unity 2022.3.62f3 build success、无error CS，request-file full self-check于2026-08-22 16:09:37为PASS。
C++ trace与真实Play Mode未取得，故不得扩大为完整CPoint或R5对齐。

`D-OP-001 / R5-OP-001 / RUNTIME_PENDING` 已完成只读 source preflight并建立独立Task Contract、Change Record和
handoff。C++ release `Entity::reset()`把`prev_frame2`归零，`spawn_from_opoint()`随后只写current
`frame=op.action`；normal opoint发生在本tick collision snapshot之后。Unity Character、WeaponBase和
OtherObject三条initializer却把`Prev2/Prev2D`提前写为action/current data；SpecialAttack的id虽为0，
但Prev2D为空会使collision reader回退current action data。允许范围只限four initializer/cache adapter与
existing opoint lifecycle self-check，验收为nonzero-action四类型
birth→next collision snapshot矩阵；current action、action0 adapter、spawn pass/cursor、kind2/multiple、
slot/generation、pool、CentralOnly与extended capacity均不得改变。four initializer/cache adapter已最小写入，
production factory的Character/LightWeapon/Other/SpecialAttack nonzero-action birth→next snapshot矩阵通过。
16:54 request PASS因Assembly-CSharp仍停在16:05已作废；UnityMCP force refresh后fresh Tundra 23.19s、
Assembly-CSharp 17:14:38、无`error CS`，2026-08-22 17:15:48 full self-check=`PASS`。C++ trace与真实
PlayMode未取得，故不得扩大为完整opoint、R5或C++ runtime已对齐。

`D-SCHED-012` 的cursor subset已拆为`R5-LIFE-01A / R5-LIFE-001A / RUNTIME_PENDING`。C++ slot50
lowest-free与late升序cursor、Unity allocator/registry/late loop mapping，以及existing Authority400 cursor /
extended lower-hole-first分立fixture均已闭合；当前唯一缺口是MobileExtended与DesktopExtended-growth的
slot>399 high/low newborn joint cursor矩阵。本包只允许补`BattleRuntimeSelfCheck` test helper/fixture，
不得修改production allocator、registry、pass order、profile或批准的扩展容量。现有test helper已最小扩展，
MobileExtended与DesktopExtended-growth的slot700→900 same-pass、slot700→600 next-pass矩阵通过。
17:10旧程序集PASS已作废；fresh Tundra 23.19s、Assembly-CSharp 17:14:38、无error CS，17:15:48
full self-check=`PASS`。pending/free/generation与D-RENDER-003 logic-half保留给R5-LIFE-01B，不能并入01A。

`R5-LIFE-01B` 已完成只读 source/mapping与Unity自动证据层，当前为 `RUNTIME_PENDING`，
且本包不预设 production gameplay 修改。C++ `free_entity` 的 immediate inactive / next-spawn reset已映射到Unity
`PendingFlushDestroy`隐藏、分配前slot/generation释放和按旧对象引用finalization；existing W05 fixture能够验证old handle失效、
same-slot newborn与ghost command隔离。`FirstPresentationTick`当前production writer只有Reset=0，normal late opoint的T+1
首次可见由RenderDispatch-before-late-opoint顺序形成。另登记`D-LIFE-001`：C++ oid7/8合体把pi<20伙伴
inactive，Unity以`OidMergeDormant`保留并占low slot；完整release live battle allocator扫描表明stage从20、opoint/effect
从50开始，slot0起的spawn只用于battle bootstrap/char-select，因此当前battle tick内没有writer会消费该low slot，暂判
`INFERRED safe adapter`。若未来出现battle-time 0..19 allocator、production非零FirstPresentationTick writer或focused/trace
first difference，必须另建gameplay Change Record，不能在01B no-code认证内直接改registry/render架构。UnityMCP
force scripts refresh/compile request完成；本包无C# diff，故Assembly-CSharp保持17:14:38。focused EditMode job
`582b9e9212264d39b4377b72d7e0374d`为19/19 PASS，2026-08-22 17:49:18 full self-check=`PASS`。
C++ runtime trace、真实Play Mode和R6 visual descriptor/order仍未关闭。

2026-08-23 `D-LIFE-001 / R8-LIFE-001`重新复核完成并保持`RUNTIME_PENDING / APPROVED UNITY ADAPTER`。
C++ live merge/split、low-slot固定数组语义、stage20+/dynamic50+分配域与Unity dormant/slot/reset/query/
presentation crosswalk均闭合；未发现需要修改production gameplay的差异。Unity保留dormant partner原slot与
generation，等价于当前C++ live调用图中不会被battle-time allocator消费的inactive low slot。focused job
`04ddfe7fa44b4f92beb0618d0f269a13`为32/32 PASS，同代码状态full self-check PASS并执行七组OID5152矩阵。
真实production Play与C++ full trace未取得，不能升级为runtime VERIFIED。

### R6：C++ render handoff 与 Unity 中央表现收口

**目的**：保持中央渲染性能，同时恢复 C++ 战斗场景可观察表现。

- C++ `renderer.cpp` 的 z sort、同 z slot 稳定顺序、shadow → entity → hit-record 交错顺序；
- `x_int/y_int/z_int`、center、sprite rect、facing、frame delay jitter、render offset、shadow anchor；
- 明确 C++ `camera_x`/perspective 与 Unity 固定逻辑 camera、`BattleCameraSafeArea`、1.5 visual scale 的边界；
- 需要逐项决定：哪些 C++ 镜头行为是 Unity 场景必须复刻的可观察表现，哪些是用户明确保留的 Unity 适配。未作决定前不得把二者混入战斗真值；
- 中央 Mesh、Texture2DArray、atlas、batch segment、URP RenderFeature 仅可改变提交方式，command 顺序和可见结果必须与 C++ descriptor 对齐。
- 不得以 render 对齐为由删除 `CentralOnly`、重建逐实体生产 `SpriteRenderer`、降低 `MobileExtended` 容量或取消 `DesktopExtended` 在unsealed预战边界的page reservation能力；Legacy 只可作为诊断/兼容对照，不能成为最终运行依赖。

**完成条件**：角色、武器、技能对象、阴影、spark、held object 的 descriptor/order/visibility 满足 C++ render handoff 合同；Game/Scene 视图由中央表现路径稳定显示，不依赖 Legacy `SpriteRenderer` 回退。自动 command trace 可用时作为附加 first-difference 证据。

#### R6-PRES-01 执行状态（2026-08-22）

active/Z/slot/per-entity command order已完成no-code source certification，状态为`RUNTIME_PENDING`。C++
`render_world`从active slot升序输入、按signed z_int稳定排序并以slot作为同Z tie；Unity CentralOnly从runtime
slot升序输入，以stable radix只排Z，fallback为Z→slot→stableId，indexed order在BuildCommands按logical
rank解析。shadow/body/overlay/hit-record保持baseOrder+0/+1/+2/+3，dynamic Mesh不按texture跨command流重排。
BeginFrame/order existing tests已随job `582b9e9212264d39b4377b72d7e0374d`通过；command writer job
`5561fce764bc4baa8804ae37ca929417`为6/6 PASS，17:49:18 full self-check=`PASS`。本包无脚本改动，
不宣称C++ trace、Play Mode或GPU像素已验收。下一独立包为D-RENDER-004 shadow OID identity。

`D-RENDER-004 / R6-PRES-002 / RUNTIME_PENDING` 已完成独立source contract、Change Record、最小脚本
修复和fresh自动验证。C++ `draw_shadow`读取current `char_data->oid`；Unity snapshot已有
`CurrentDatObjectId`，但旧BuildCommands使用shell `ObjectId`。shadow 223/224 gate现已改读
`CurrentDatObjectId`；existing P7反向identity matrix同步验证shell223/current7300和shell224/current7300
画shadow、shell7300/current223不画，并保持checksum隔离。source `18:15:26/29` < Assembly-CSharp
`18:16:56` < result `18:18:10 PASS`；Tundra 6.02s、filtered compile errors=0。未做C++ runtime trace或
真实Play Mode/GPU可见验收，不能扩大为完整R6对齐。D-RENDER-001/002/005保持独立。

`D-RENDER-005 / R6-PRES-003 / RUNTIME_PENDING` 已完成脚本修改前writer inventory。C++ body/shadow没有
独立EntityVisible/ShadowVisible truth；Unity EntityVisible当前production false writer均随resource invalid、
destroy/pending/pool或非production legacy TU发生，尚未发现独立可达first difference。另一方面，production
`UpdateShadow/UpdateShadowManagedState`仍以shell ObjectId写ShadowVisible，会在BuildCommands current-DAT
gate之前拦掉shell223/224/current7300的正确shadow，confirmed difference已建立独立Task Contract和Change Record。
两个shadow writer现已改读current DAT，P7已先运行production managed writer并断言cache+command；
首次18:31:32 full self-check因fixture三条entity未绑定LF2Sprite而失败并已留档；最小补rendererless
binding后，test source 18:32:36 < Assembly-CSharp 18:33:37，fresh Tundra 2.66s、filtered errors=0，
18:35:48 full self-check=`PASS`。PlayMode/C++ trace/GPU像素尚未完成；下一独立包为D-RENDER-001。

`D-RENDER-001 / R6-PRES-04 / RUNTIME_PENDING` 已完成no-code adapter certification。C++只定义
renderer success handoff及自身surface/sprite readiness；Unity feature/material/URP camera/catalog/backend
是实现前置。CentralOnly cold empty、ready current、last-good stale、replacement ready均保持Central pixel
owner，simulationTick/displayTick/reason显式，不会恢复Legacy production materializer，因此该结构差异属于
A-RENDER-001必要adapter，不作脚本修改。18:35:48 fresh full self-check覆盖P4/P8/ownership/resource reason
并PASS；19:09额外focused EditMode因Editor已关闭、MCP 0 instance未创建job。真实URP PlayMode/C++ trace
仍待；下一独立包为D-RENDER-002 spark writeback timing。

`D-RENDER-002 / R6-PRES-005 / RUNTIME_PENDING` 已完成脚本修改前source/consumer preflight。C++在每tick
render callback内推进valid hit-record age或移除一个invalid tail；next tick kind0 writer用10槽gate决定
是否追加并消费两次global RNG。Unity LateUpdate/worker acknowledgement在普通LocalFreeRun一帧一tick时
通常next-tick等价，但explicit/manual/replay和CentralOnly buildPresentation=false会少推进，故不是纯视觉
风险。最小合同只在既有RenderDispatch内立即finalize已冻结cycle，无publication时以sealed runtime lifecycle
catalog执行同一规则；不改pass order、checksum、worker protocol、GPU或collision/RNG。Change Record已建立，
最小代码与focused matrix已写；dotnet生成工程0 error、validator/diff PASS。首次Unity batch因licensing
IPC timeout以199退出且当时DLL/result未刷新；随后用户交互Editor完成fresh Tundra 26.11s、`error CS=0`、
Assembly-CSharp 19:41:38及19:49:12 full self-check PASS，`B-R6-PRES-005-01`已解决。PlayMode/C++ trace
仍待，故不得扩大为完整R6或battle已对齐。

2026-08-23 R8-WP01G-R07A已补齐该Play缺口到可用证据上限：4组独立正式collision/hit pair在production
worker tick843～846分别覆盖3次publication与1次CentralOnly no-publication。frozen/live age、owner handle/
generation、central command、每tick exact2 RNG、Late幂等、0 allocation violation delta与cleanup全部PASS；
worker18/18、hit178/178、central13/13及20:25:11 full self-check均PASS。`D-SCHED-009 + D-RENDER-002`
现为`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；不得扩大为整个render/battle已对齐。

`A-RENDER-002 / R6-PRES-006 / RUNTIME_PENDING` 已完成no-code adapter certification。C++ held writer
以center/wpoint写1×逻辑位置；Unity保留`BattleVisualScale=1.5`，但scale只乘body center-to-pivot delta，
逻辑pixel/world不乘1.5。held使用`(scale-1)*(holderDelta-heldDelta)`补偿，Central与Legacy diagnostic
复用同一helper；right/left、invalid/reuse/dormant、central immutable与实际held sample均由19:49:12
fresh full self-check覆盖并PASS。真实DAT/atlas body/weapon/shadow锚点仍待R8 PlayMode。

`A-RENDER-003 / R6-PRES-007 / RUNTIME_PENDING` 已完成no-code adapter certification。C++ camera_x
属于display camera链；Unity按用户批准边界在每tick PreFrame后清零release camera/velocity与entity
RenderOffsetX，`BattleCameraSafeArea`只移动presentation camera、不反写runtime position。stationary
entity/shadow fixture由19:49:12 full self-check实际PASS。URP safe-area/scene左右边缘仍待R8 PlayMode；
snapshot restore后PreFrame前直接发布的可达性暂为UNKNOWN。

至此R6的source/当前自动证据层已经收口，可以进入R7逐项优化重新认证；所有R6条目仍是
`RUNTIME_PENDING`而非C++ runtime/PlayMode完整视觉证书。

### R7：性能优化逐项重新认证

**目的**：不牺牲性能成果，但让每项优化重新获得 C++ 行为许可证。

对下列路径逐项执行“C++ 源码合同 → Unity fallback → Unity optimized”的行为复核；若 R1-WP02 的只读 full trace 以后可用，再叠加三向自动比较：

- PreInteraction no-op proof；
- LateEntityUpdate exact-character skip；
- Frame/AI SoA writer；
- broadphase / Loose Quadtree；
- cached snapshot、frozen presentation、central renderer；
- worker publication 和 presentation acknowledgement；
- pool、slot allocator、dynamic capacity。

**晋升规则**：优化必须不改变其覆盖模块的 C++ 源码行为合同、slot/RNG/输入/候选/生命周期顺序，并在相应 focused/integration 验收中保持等价；正式战斗窗口还必须保持 0 B、无 Gen0/1/2 collection、无 capacity fault、无对象丢失。若 full trace 可用，则以三向一致作为更高等级证据。FPS 改善不能覆盖行为分叉。

`D-PERF-001 / R7-PERF-001 / RUNTIME_PENDING`：只读预检确认
PreInteraction cross-pass proof在death cleanup后缓存neutral结论，但其后held/collision consume可在同slot
改变frame/CPoint/link；现有消费端只验occupancy/pending结构量，不能证明C++ T14 no-op。后续最小包应
停用production cross-pass cache、保留T14当点whole-pass proof与participant filtering。`R6-PRES-005`
fresh自动验收已通过，R6 adapter自动证据层也已收口。private producer/consumer现已删除；同点proof、
three-pass writer、participant filter与public stress/report schema保留；neutral及same-slot current kind2
oracle已写。两份dotnet生成工程已0 error、validator/diff PASS；用户Refresh并恢复UnityMCP session后，
fresh Unity DLL为20:16:44/45且晚于20:02 source，focused job
`09948d3e3e314d84ab80791d0d2b2070`为15/15 PASS并实际覆盖same-slot kind2 oracle与两项warmed 0 B；
20:22:37 full `BattleRuntimeSelfCheck=PASS`。`B-R7-PERF-001-01`已解决，旧19:49:12 PASS未复用。
Play Mode与C++ runtime trace仍未完成，因此本包保持`RUNTIME_PENDING`，不得宣称完整VERIFIED。

`R7-FRAME-001 / RUNTIME_PENDING / NO-CODE CONDITIONAL CERTIFICATION`：已对
C++ recovery + frame_tick与Unity data-oriented recovery/FrameTick pass做逐段复核。除既有D-MOV-005外
没有发现新confirmed difference；period、gate、counter、raw frame、jump212、PP/turn和tail顺序可映射。
current DAT oid51/52与shell identity同步、invalid DAT jump flag恢复性仍只为INFERRED/UNKNOWN；旧Unity
legacy-vs-data-oriented tests不能单独定义C++规则。详情见
`docs/ai/RESEARCH/R7-FRAME-01-frame-tick-recovery-soa-recertification-preflight-20260822.md`。current DAT watch确认
state2000仅存在type2/type4 weapon；exact identity writer watch未发现OID51/52 shell/current-DAT分离。
focused job `7b5d94953fca4cdb8947aaa2350277ca`为22/22 PASS，覆盖Recovery/FrameTick legacy-vs-data-oriented、
fallback与warmed 0 B；20:42:47 full self-check仍PASS。本包未修改脚本，也未创建Change Record。
D-MOV-005仍是conditional `INFERRED not reachable`，invalid/mutable DAT jump flags仍UNKNOWN；Play Mode与
C++ runtime trace未关闭，不能写成FrameTick完整VERIFIED。详情见
`docs/ai/TASKS/R7-FRAME-01-frame-tick-recovery-soa-certification.md`。

`D-LATE-001 / R7-LATE-001 / RUNTIME_PENDING`：exact-character late
skip复核发现C++同一state-special支持9995→4000→8000 reload chain，并在最终state9996/attacking1按
slot50最低空槽生成4×OID217+1×OID218、成功五child共消费34次RNG；Unity transform提前return、无9996
writer且skip gate把9996判为no-op，旧GT-11还错误断言零RNG/零spawn。独立Task/Change Record已在脚本修改前
建立；entity三段reload、world-owned 4×217+1×218 writer与GT-11 full/missing/capacity/chain/cursor/
generation矩阵现已落地。fresh Unity 20:41:14、20:42:47 full self-check PASS，34-call RNG与warmed no-slot
0 B均通过。真实DAT Play Mode、GameObject pool视觉表现与C++ runtime trace仍待，因此保持
`RUNTIME_PENDING`；通用factory/API、allocator、pool/RNG abstraction与pass order均未改。

`D-INP-005 / R7-AI-01 / RUNTIME_PENDING / NO-PRODUCTION-CODE CONDITIONAL CERTIFICATION`：
C++ `input_handler.cpp:1209-1235,1615-1898` 的first10 move-mode、ground/air target、ground-derived
`best_dist/same_z_lane`、cache `%30` retain/refresh、same-team guard与slot20+ special scan已逐段映射到
Unity fallback、SoA/indexed与unified snapshot authority。production GameConfig/scene/profile resolver均确认
`DataOrientedCanonical`。初始AI筛选job发现一条仍把dead self视为ineligible的旧Editor fixture；它与C++和
`R3-AI-LIFE-001`冲突，已由独立`R7-AI-TEST-001 / VERIFIED / TEST-ONLY`拆分修正，production AI未改。
fresh exact job `8c74d8e0a76e427fac3fd7920f5ac234`为2/2 PASS，sensing/profile job
`5c6bad85dc0b43c2a6949d03cfd256fc`为111/111 PASS，21:04:52 full self-check与validator/diff均PASS。
本证书不覆盖`input_handler.cpp:1900+`完整OID decision tree、真实AI Play Mode、C++ runtime trace或Unity
>399 slot extension语义；下一包必须独立执行`R7-AI-02`，不得宣称完整AI已对齐。

`R7-AI-02 / SOURCE-CONFIRMED DIFFERENCE / INVENTORIED / NO GAMEPLAY CHANGE`：进一步逐段核对确认，
C++ `input_handler.cpp:2055-2204` 的outer random gate内固定执行39个有序character/OID helper positions；
Unity Legacy与`AiDecisionKernel`都只保留positions1–6，缺失positions7–27与29–37共30个call positions，
并把已实现的position28/38/39放到outer gate外。该差异会改变角色专项技能、combo/input、RNG调用数/顺序与
early return。optimized `AiSensingSnapshot`还缺OID11 frame290 side-effect所需current frame`hit_j`。
现有decision focused job `3eaff2c1bb474565b2dd4c66d02c49db`为75/75 PASS，但两条Unity路径共享同一缩减链，
因此只是coverage baseline而非C++证书。已登记`D-INP-007A/B`、`D-INP-008`、`D-INP-009`，并按
02A authority fixture、02B HitJ合同、02C～02E分组helper、02F full dispatcher integration拆包；02F前不得
激活部分默认链。详情见`docs/ai/RESEARCH/R7-AI-02-character-decision-chain-preflight-20260822.md`。

`R7-BROAD-01 / RUNTIME_PENDING / NO-CODE CONDITIONAL CERTIFICATION`：C++ slot `i/j` pair、双方向
collect与ITR/BDY exact顺序已映射到Unity BruteForce；role-aware/Loose Quadtree只生成可能的attack→body
unordered pair，按authority ordinal排序后执行同一双方向exact narrow phase；invalid/degenerate geometry
conservative fallback，formal failure恢复RNG后完整回退brute。fresh focused jobs分别9/9、58/58、16/16，
合计83/83 PASS，覆盖strict boundary、candidate order、RNG、fallback、generation、1000 synthetic、
direct/sweep/tree与warmed 0 B。suite后full self-check曾在R3-INP-01连续失败；无代码变化且domain reload后
22:13:06恢复PASS，登记`D-TEST-001`跨测试static污染，而非broadphase gameplay回归。另登记`D-PERF-002`：
production GameConfig broadphase为空且resolver默认BruteForce，普通NTSD_Battle未默认部署LooseQuadtree；
stress显式选择不构成production接线证据。Play Mode/C++ trace仍待，不得宣称完整VERIFIED。

`R7-PRES-WORK-01 / RUNTIME_PENDING / NO-PRODUCTION-CODE CONDITIONAL CERTIFICATION`：C++
RenderDispatch observation point与Unity non-worker/worker frozen capture均位于PreFrame/Stage之后、
FramePostProcess/Late/tail之前；CentralOnly只延迟物化表现顺序/命令，不把Unity表现写回逻辑真值。
latest publication的world/frame/tick/version去重、same-frame gate、world retirement、camera lease generation/tick
校验，以及worker sequence publication→host consume→Late acknowledgement→next tick unblock已完成source mapping。
fresh focused positive jobs为13/13、11/11、6/6、16/16，合计46/46 PASS。production配置确认为
`CentralOnly + useDedicatedSimulationWorker=1 + maxCatchUpTicksPerFrame=1`。新登记`D-TEST-002`：旧worker
human-input fixture错误期待本tickcurrent key清零，actual=1反而与C++ poll一致；另登记`D-TEST-003`缺正式driver
buildPresentation=true→central materialize→ack→next tick联合夹具，以及`D-PERF-003` single-flight部署边界。
本包未改脚本；脚本域重载后Unity Console编译错误为0，2026-08-22 22:32:29 full
`BattleRuntimeSelfCheck=PASS`；真实URP PlayMode和C++ runtime evidence仍待。

`R7-CAP-01 / INVENTORIED / D-CAP-001 OPEN / NO CODE CHANGE`：C++ fixed 400 world的dynamic
spawn按slot50起取最低inactive slot；Unity segmented min-heap、paged slot table、claim/release generation与
pending-destroy admission保持lower-hole-first和旧handle失效。focused job
`4cc1de5fb20b49609ee0824cd64c4af4`为44/44 PASS，覆盖Mobile 1000 saturation、lowest reuse、generation/
snapshot、all concrete logic pool families、pooled reset、sealed rejection及warmed 0 B。`PoolMaxSize=200`只在
pre-seal扩容时warning，不是实体硬上限。新登记`D-CAP-001`：DesktopExtended虽有page growth实现，但
`BeginBattleAllocationSeal`后runtime capacity拒绝任何增长；Windows默认初始512，因此battle-time实际hard cap
等于已准备容量，与本计划保护的“动态增长、无production hard cap”不一致。strict battle 0 B与真正unbounded
growth不能同时绝对保证，必须先由R7-CAP-01A固定preflight reservation / controlled safe-point growth /
deterministic admission fault合同，再允许代码改动。
本包未改脚本；fresh-domain Unity Console为0条error/warning，2026-08-22 22:45:05 full
`BattleRuntimeSelfCheck=PASS`。

**R7完整inventory checkpoint（2026-08-22）**：本节列出的PreInteraction、Late、Frame/AI SoA、
broadphase、frozen/central、worker publication/ack、pool/slot/dynamic capacity均已完成source/test盘点。
当前不再允许“边找边整体修”。未关闭的production gameplay/data差异集中为`D-INP-007A/B`与`D-INP-008`；
验收问题为`D-INP-009`、`D-TEST-001/002/003`；部署/架构问题为`D-PERF-002/003`与`D-CAP-001`。
后续固定按`docs/ai/TASKS/R7-REPAIR-SEQUENCE-after-complete-inventory.md`执行，第一个实施包只能是
test-only `R7-AI-02A`，不得直接整体接入AI 39-position chain。

`R7-AI-02A / R7-AI-002A / VERIFIED TEST-ONLY RED-WITNESS CONTRACT`：新增39-position ordered
source table；普通class job为1 PASS + 2 Explicit skipped。定向position7 witness得到expected DRJ=3 / actual0，
position28 witness得到outer gate miss下expected DUA=0 / actual3，均按预期FAIL并固定`D-INP-007A/B`。
首次fixture漏计boundary RNG draw的无效结果与AssetDatabase未导入导致`total=0`的请求均已留档并纠正。
fresh Editor compile 0 error、existing AI 75/75、23:01:13 full self-check PASS。production未改；下一包为
`R7-AI-02B` HitJ data contract，`D-INP-007A/B`仍open。

`R7-AI-02B / R7-AI-002B / RUNTIME_PENDING`：current-frame DAT `hit_j`已进入snapshot和frame-motion
canonical store；DAT只在fallback capture或frame bind/write边界解析，UnifiedAuthority initial/incremental
consumer不回读Entity/DAT。Frame pending同点发布HitJ，grow/copy与full/refresh comparison已覆盖。最终focused
job `d0670d95986c41e7b115b8a77754d23b`为4/4、AI regression `a525edc1f1c64bfe854f3d3218bd1d4e`
为212/212、warmed 0 B、fresh Unity compile 0 error、23:43:36 fresh-domain self-check PASS。同domain首次
self-check的既有`D-TEST-001`失败已留档。该包没有接OID11 helper；`D-INP-008`仅达到代码/自动证据闭环，
PlayMode/02F/C++ trace仍待。下一包为`R7-AI-02C`。

`R7-AI-02C / R7-AI-002C / RUNTIME_PENDING / UNWIRED MODULE`：C++ positions7–16已实现为非partial
实例module；existing kernel RNG clone已提取为共享allocation-free值类型，LCG/call-count/trace hash不变。
focused job `b65adcac443844c183272c984934d061`为19/19，clean AI baseline
`12463c4731a24aa4ae9919f96599f720`为212/212，warmed 0 B、fresh compile 0 error、00:10:58
fresh-domain self-check PASS。组合job `202c6992fb784d219c02d1318f605068`只有两个02A Explicit red
witness按预期FAIL，证明02C没有提前默认接线。`D-INP-007A`仍是production difference；下一包为02D。

`R7-AI-02D / R7-AI-002D / RUNTIME_PENDING / UNWIRED MODULE`：C++ positions17–28已加入同一个非partial
实例module；position21显式验证400-slot scan、strict-farthest/first-tie与void continuation，position24/26
保留dynamic-modulus RNG顺序。首轮fixture遗漏OID19在position26的前序draw，失败job保留后修正；最终focused
`5d265876f2e24159879cd881e7218d80`为26/26，AI regression `3cd69caca0f546338f1ced0500cb4062`
为238/238，warmed scan 0 B、generated Editor build/Unity compile 0 error、00:33:49 fresh-domain self-check
PASS。02A job `0173e28d95bf44ab9df97facc81193cc`两个red witness仍按预期FAIL，证明position7仍缺且
production position28仍在outer gate外。`D-INP-007A/B`继续open；下一包为02E。

`R7-AI-02E / R7-AI-002E / RUNTIME_PENDING / UNWIRED MODULE`：C++ positions29–37已加入同一个非partial
实例module；position30显式验证first-20 first-match/no-obj-type filter，position31 frame263/264 jump后继续，
position34显式验证first-100/self-inclusion与门命中后无目标仍return true。首轮fixture遗漏OID5/14在
position29的前序draw，失败job保留后修正；最终focused `1a2716b9caee4fa8bfd6285fc0c3f738`为31/31，
AI regression `0eddc4e7c54840d3b5db41d035b63eb3`为238/238，warmed scan 0 B、generated Editor
build/Unity compile 0 error、00:48:12 fresh-domain self-check PASS。02A job
`7a865915ba984168abe0636da7bac54c`两个red witness仍按预期FAIL。02C～02E依赖已齐，但production
仍未接线，`D-INP-007A/B`继续open；下一包为02F full dispatcher integration合同。

`R7-AI-02F / R7-AI-002F / RUNTIME_PENDING`：Legacy与DataOriented已原子接入C++ source-derived
outer-gated positions1–39；positions28/38/39旧gate外重复helper已删除。snapshot持久拥有module，Legacy使用
pass级shared rows与构造期预分配fallback，RNG value stream/trace/matched-position均进入既有shadow合同；
self<400 global scan保持C++ 400域，extended self使用完整Unity capacity，first20/100不扩展。shared row采集
同时修正为只读InputHistory，避免隐式`new int[6]`。final authority chain 3/3、full dispatcher 5/5、
fixed-seed production profile-pair 1/1、AI相关矩阵286/286、warmed 0 B、Unity compile 0 error、
2026-08-23 02:07:58 final fresh-domain full self-check PASS；02:03:05同domain失败是已登记D-TEST-001污染。
`D-INP-007A/B`代码级差异已关闭，`D-INP-008/009`自动证据已闭合；真实角色Play Mode、C++ runtime trace
与R8联合场景仍未完成，故不得宣称完整AI或完整battle VERIFIED。

`R7-TEST-002 / VERIFIED / TEST-ONLY`：worker human-input fixture的两条stale current-key断言已按C++
`InputHandler::poll`从0修为1；Prev仍为0，cooldown/history/publication/ack断言保持。exact 1/1、worker class
17/17、Unity compile 0 error、2026-08-23 02:14:36 fresh-domain full self-check PASS。production input、
worker、driver、render均未修改。

`R7-TEST-003 / VERIFIED / TEST-ONLY`：formal driver双tickfixture已联合覆盖worker
`buildPresentation=true` frozen publication、CentralOnly exact-tick物化、ack/finalization、next-tick unblock及
new frame/generation，并证明host不反写原publication。exact job 1/1、worker+central regression 31/31、
dotnet/Unity compile 0 error，focused后脚本域重载的2026-08-23 02:27:37 full self-check PASS。production
worker、driver、render、single-flight与catch-up均未修改；下一包为R7-TEST-001静态测试污染隔离，
真实URP Play Mode与C++ runtime evidence仍归R8/trace层。

`R7-TEST-001 / VERIFIED / TEST-ONLY`：fresh-domain二分将D-TEST-001锁到shared-shadow owner对static
`LF2FrameCache.EmptyFrame.state`的未恢复写，并暴露unified ascending fixture对该污染的隐藏依赖。
两fixture现均显式own/finally恢复sentinel；dependent使用character-input mutation override触发canonical
post-input full refresh。final dependent exact 1/1、class 66/66、AI matrix 286/286；class/AI后不reload域的
full self-check于03:03:54/03:06:15均PASS，final fresh Unity compile 0 error、03:07:32 self-check PASS。
production AI/frame/input/scheduler均未修改；R7 repair order 1–9 closed，下一包为R7-BROAD-02 decision matrix。

`R7-BROAD-02 / DECISION COMPLETE / RETAIN BRUTEFORCE / NO CHANGE`：fresh role-aware/formal/Loose/
participant job 80/80与AirRole nearest 8/8均PASS，随后same-domain 03:13:57 full self-check PASS。
synthetic 1000的500 vs 499,500 pair reduction继续成立，但current-build真实production Brute/Loose A/B、
R8 scene parity和real fallback distribution未闭合；历史1000-AI harness本身已强制Loose仍未达30Hz。
因此保持`BattleCollisionBroadphaseName`为空并由resolver默认BruteForce；未来切换需独立配置Record。
R7 repair order 1–10 closed，下一包为R7-CAP-01A容量合同决策。

`R7-CAP-01A / DECISION COMPLETE / CURRENT CODE CONFORMS / NO CODE`：Desktop合同已固定为
无固定产品级active cap、每局unsealed loading/reset/preflight边界的有限page reservation、active battle
seal后strict 0 B，以及超预算deterministic admission failure。默认512只是hint；“无固定产品cap”不表示
tick内数学无限增长。fresh capacity/pending/generation 11/11与pool/reuse/pressure 33/33，合计44/44 PASS；
03:19:45同域full self-check PASS。当前production已满足，`D-CAP-001`按合同澄清关闭，不实施R7-CAP-01B。
R7 repair orders 1–11至此全部关闭，下一阶段为R8。

### R8：完整战斗场景认证

**目的**：形成真正的 C++ release → Unity production certificate。

`R8-WP01 / IN_PROGRESS / CERTIFICATION-ONLY`（2026-08-23）：已建立current-worktree分层矩阵，按
fresh compile/self-check/EditMode → movement/input Play Mode → interaction/opoint/lifecycle → CentralOnly
visible rendering → 1000 active/0 GC/30 Hz → Windows Mono/IL2CPP顺序执行。2026-08-20旧U9只作历史baseline，
不能直接关闭当前R8；任何发现的脚本差异必须退出当前认证项并先建立独立Task/Change Record。

`R8-WP01A / AUTOMATED BASELINE PASS`：初次1357项暴露并由R8-TEST-001/002关闭两个陈旧diagnostic/test
合同；production damage/link均未改。最终full EditMode job `6a6336d0e1e94abd9585110358012ca5`
1357/1357 PASS、同域07:31:17与fresh 07:32:39 full self-check PASS、compile 0 error、ledger validator PASS。
该结果只关闭自动基线；R8 Play Mode、CentralOnly可见运行、1000 current-build与Windows Player仍待。

`R8-WP01B / D-INP-006 CURRENT FAILURE`：当前Play Mode由用户确认按钮/按键组合无法释放技能。物理
asset的W/S/A/D/J/K/L静态映射正确；source preflight确认canonical local provider只在tick submission
采held、以相邻held生成edge，direct callback packet在BeforeSimTick丢弃，而dedicated worker single-flight
在publication/presentation ack前不会采下一tick。该组合是高风险first-difference候选，但尚未取得逐tick
证据，不能直接宣称根因或修改技能/DAT。已建立`R8-INP-01`，按InputAction→FrameInputSet→roster→
Runtime key/cd/combo/frame顺序定位后才允许另建Change Record修复。

`R8-WP01B FIRST DIFFERENCE UPDATE`：继续只读C++ header/source后确认，J/K/L crossed internal mapping是
C++ `DEFAULT_P1`正式设计；真正差异为`D-INP-010`。C++八方向+DJA combo字段由引用即时写回，Unity
`BattleCharacterInputActionResolver`却复制九字段到local，并在绝大多数incomplete/DJA/guard/Unk328
return前丢弃，导致跨tick L→方向→J/K组合无法完成。旧self-check还把staggered Naruto L/S/K失败写成
expected。已建立`R3-COMBO-01 / R3-COMBO-001 PLANNED`，尚未改脚本；physical edge/worker仍作为
D-INP-006后续，禁止合并为架构重构。

`R8-WP01B FINAL UPDATE`：`R3-COMBO-001 / VERIFIED` 已关闭by-ref组合状态差异；fresh compile、
full self-check、input EditMode 47/47及两组production InputSystem Play（Naruto L/S/K→frame271、
L/D/J→frame263）均PASS。该状态不替代用户实体键盘/窗口焦点edge的独立D-INP-006人工验证。

`R8-WP01C OBJECT CERTIFICATION UPDATE`：对象认证已拆为01～07。01 opoint birth/newborn/lifecycle与
02 pickup/held/throw/landing现均取得Unity S4 `VERIFIED`；02使用OID120/150/121/122覆盖type1/2/4/6，
验证wpoint integer坐标、FrameDelay、spawner/picker、关系释放、四类landing和no-immediate-hit，cleanup及
fresh compile/focused/full self-check/ledger均PASS。03 grab/CPoint/link及后续包仍未开始，C++ full trace保持
BLOCKED，不得把01/02扩大为完整对象链或完整战斗对齐。

2026-08-23 WP01C-03更新：`R8-GRABPLAY-001`在Unity production Play S4范围`VERIFIED`。worker-active
clean Play以通用source-derived CPoint数据验证valid kind3 grab、first-held无damage、PreInteraction唯一
lethal injury/global stats/position、后续positive-link与second-held无重复；同时验证reciprocal mismatch
fallback throw、negative-duration escape+dircontrol+postprocess和positive/negative link residue。compile0、
focused8/8+2/2、cleanup/Console0、12:23:59 self-check PASS，production0改动。04 collision/hit/damage及
后续仍未执行；C++ full trace继续BLOCKED。

2026-08-23 WP01C-04更新：`R8-HITPLAY-001`在Unity production Play S4范围`VERIFIED`。通用
source-derived live fixture冻结10个candidate并依次执行character consume→random-weapon no-op→object
consume；character/weapon/special正向伤害、HitConfirm2/effect21整attacker abort、caught first-only skip、
kind10 raw frame182、HP/HPBound/stats/durability/vrest均通过。fresh compile、focused178/178+11/11+9/9、
cleanup/Play Console0、13:19:39 self-check和ledger均PASS，production0改动。启动hit-plan mode为Disabled且
worker inactive，故不声明本轮ShadowCompare/worker-active/C++ full trace。05 death/respawn及后续仍未执行。

2026-08-23 WP01C-05更新：`R8-DEATHPLAY-001`在Unity production Play S4范围`VERIFIED`。HP=0 AI
pre-cleanup input、state14 0→30→4、no-count/stored/free、stale integer average+two RNG、relation/link writer
边界与production OID998/action6均PASS；cleanup、compile、focused、13:52:04 self-check和治理通过，
production0改动。worker-active和C++ full trace仍未取得；06按连续授权进入。

2026-08-23 WP01C-06更新：`R8-LATEPLAY-001`在Unity production Play S4范围`VERIFIED`。current
sealed catalog natural random、worker-active live 4×217+1×218、logic-only 9995→4000→8000→9996
full chain和Authority400 exhaustion全部PASS；compile、focused14/14、cleanup/Console0、14:08:15 self-check
与治理通过，production0改动。C++ full trace仍BLOCKED；进入07 synthesis。

2026-08-23 WP01C-07更新：WP01C certification现`COMPLETE`。01～06均取得限定范围Unity S4，07已汇总
每包PASS、D-ID最高证据、probe失败留痕与未关闭边界；六包认证对production gameplay改动0。该完成不关闭
D-COL-004/D-COL-005B/D-HIT未覆盖分支/D-HIT-005/D-LIFE-001，不关闭WP01D/E/F/G或C++ full trace，
不得宣称R8或完整战斗逻辑最终对齐。

`D-RENDER-006 / SOURCE-CONFIRMED DIFFERENCES / IN_PROGRESS`：用户报告部分技能图片错误并明确要求
不得依赖具体角色/技能/OID。R8-WP01D已闭合两个全实体通用差异：state8000应写
`unk_318/RenderPicOffset=140`且raw pic999先隐藏；DAT `row`在C++ parser→loading→SpriteSheet→renderer
链中固定为横向列，而Unity按BMP物理尺寸猜测并可能把`col`当横向列；C++还以declared range允许
localPic并在blit时裁剪source，Unity却以`row*col`限帧且把partial rect留hole。首次真实Play审计覆盖100个
loaded definitions、4373 catalog entries、6674 frames并累计1301 source rect differences；row修复后
4933-entry source mismatch清零，经fully-outside和黑色colorkey像素通用过滤后只余2个非黑visible missing。

2026-08-23覆盖更新：declared-range/partial clip通用修复后final all-DAT Play为5537 catalog entries、
6674 authored frames、23 clipped引用、0 source/path/rect/pivot/binding differences，cleanup PASS；
focused resolver/atlas/mesh 29/29，synthetic GPU matrix PASS。现进入`R8-WP01D-05 / R8-SPRITEMAP-005`，
以适配logic-only + Central snapshot的全catalog source→binding像素和统一GPU command证据替代旧P8-C
逐实体renderer前提；不恢复Legacy owner，不增加角色/技能/OID/frame/file分支。D-RENDER-006仍需
GPU/Game/Scene证据，不能因catalog PASS直接关闭。

2026-08-23 GPU覆盖更新：`R8-SPRITEMAP-005`在其Editor GPU S4范围内`VERIFIED`。final Play对
5537 entries、84,327,319 pixels逐项比较，source/central hash相同且0差异；动态选择可见partial
450×5 / pivot(0.5,-28)，正式resolver+dynamic Mesh与Legacy均340 pixels、mean/max=0/0；cleanup、
focused35/35和11:37:22 self-check PASS。D-RENDER-006整体仍是`RUNTIME_PENDING`，下一证据层是
真实Game/Scene的可见挂点/层级/相机结果；loaded data无authored state8000 source与C++ full trace
必须继续如实标记，不能用GPU catalog证据代替。
修复必须继续通过通用合同完成，不得用Legacy回退或任何角色/技能/OID特判；all-DAT复跑、GPU像素与
真实Game/Scene视觉仍待。

2026-08-23 Game submission覆盖更新：`R8-SPRITEMAP-006`在Editor diagnostic与当前Game证据范围内
`VERIFIED`。首次tick1空snapshot是过早采样；final tick257为3 snapshots、6 source/resolved commands、
1 chunk/segment/draw，immutable plan current、worker/cleanup正常且无refusal。fresh Game截图实际可见
角色、武器和阴影，故没有依据修改worker、central renderer、URP或camera，production 0改动。当前
D-RENDER-006仍为`RUNTIME_PENDING`，只剩可裁决Scene View证据、loaded data无authored state8000
live witness与C++ full trace三项证据缺口；不得把这些未知项写成已确认production差异。

2026-08-23 SceneView覆盖更新：`R8-SPRITEMAP-007`在Play Mode真实SceneView camera S4范围内
`VERIFIED`。production gate/current lease均true，tick2/generation3 current plan为4 source/resolved commands、
1 segment；960×540白底isolated SceneView render产生575 non-clear pixels，hash
`C292967D753744C2`。fresh compile 0、focused13/13、cleanup、Play Console 0 error与12:05:47 self-check均PASS，
production/scene/URP均0改动。先前空Scene截图来自180×936窄viewport与logic-only Transform观察方式不足，
不能作为renderer差异。D-RENDER-006现在只剩loaded data无authored state8000 live witness和C++ full trace。

2026-08-23 WP01D边界更新：01～07已完成当前资源允许的最高证据，状态为`COMPLETE AT AVAILABLE
EVIDENCE / FULL CLOSURE BLOCKED`。loaded DAT中authored state8000 frame为0，不能修改DAT伪造witness；
R1-WP02 full trace继续BLOCKED。没有新的可实施render first-difference，WP01E/F/G可独立继续。

2026-08-23 WP01E启动：已建立current-build容量/性能认证合同，不复用历史U9/P0～P6报告直接晋升。
先复跑工具/容量基线和Dispersed/Combat短样本validity gate，再对两组各运行120 warmup + 1800 sampled
logic ticks；正式门要求1000 production GameObject、每帧最多1 tick、Avg/P95均不超过33.333ms、稳态
0 B且Gen0/1/2 collection为0、0 capacity fault、中央draw/pixel有效、hash存在与teardown restored。
认证发现首差时必须停止并另建修复包，不得在WP01E内顺手改production或改变C++ observable behavior。

2026-08-23 WP01E首次执行更新：E-01 compile0、focused 290/290、full self-check PASS；E-02尚未生成
1000实体或性能样本。pressure request在BattleTestBootstrap初始异步服务加载时观察到driver/world已存在、
lazy LF2ObjectPool尚未创建，request processor将该合法partial footprint误判为runtime invalid，clean restart
一次后再次fail-closed。该first failure属于认证工具lifecycle，不是帧率/GC/gameplay结论。已建立
`R8-WP01E-R01 / R8-PERFBOOT-001 / PLANNED`，只允许修正initial-wait与post-healthy invalidation判别并补
focused policy tests；批准前不改脚本。

2026-08-23 WP01E恢复更新：`R8-PERFBOOT-001`已`VERIFIED`，同一Combat1000请求现以1000 active、
180 sampled、logic Avg/P95 21.199/23.797ms、0B/0 collection、capacity critical0、central 1 draw与完整
teardown通过；pool/Bootstrap/gameplay未改。该短样本的visible frame Avg/P95仍为38.949/39.025ms，故只
关闭初始化首差与Combat短样本validity，不能宣称正式30FPS完成；继续Dispersed短样本和两组60秒门。

2026-08-23 WP01E短矩阵更新：Dispersed1000亦以logic Avg/P95 21.432/24.771ms、0B/0 collection、
capacity critical0、central1 draw/SetPass4、teardown restored通过；但visible frame Avg/P95
38.309/44.265ms。Combat/Dispersed短矩阵只证明current-build validity，不满足正式30FPS。下一步运行
两组各120 warmup+1800 sampled并启用completed-frame timing，再依据main/render/GPU/frame-GC数据建立修复包。

2026-08-23 WP01E最终更新：`VERIFIED / UNITY EDITOR CURRENT BUILD`。Dispersed/Combat正式1800-tick
visible P95=25.525/33.058ms、main P95=25.286/26.901ms、logic P95=18.575/19.044ms；logic0B、
Gen0/1/2=0、capacity critical0、central1 draw、SetPass约4、teardown PASS。Desktop容量focused299/299；
fresh Legacy/Data同请求12项状态/workload hash全等。该结论不包含Player、Android或C++ full trace；下一步
WP01F Windows Mono/IL2CPP。

2026-08-23 WP01F规划：国际版2022.3.62f3的WindowsStandaloneSupport与IL2CPP模块已确认存在；现有
`ProductionEntityStressPlayerBuild`仅支持旧U9 Mono。已建立`R8-WP01F-windows-mono-il2cpp-player-
certification.md`与`R8-PLAYERBUILD-001 / PLANNED / APPROVAL PENDING`：只允许增加双后端独立Temp build
入口并严格恢复backend/frame timing/background/Burst，随后用同一Combat1000 request比较两Player的hard
0B/capacity/central/teardown和12项hash。批准前不改脚本、不build/run。

2026-08-23 WP01F范围更正：用户明确指示`IL2CPP Player 不会有任何问题，不要做相关处理`。因此停止
IL2CPP build/run/诊断/修复/认证，不把Codex沙箱Player结果当作gameplay差异或blocker，也不据此修改Unity
runtime/config。`R8-PLAYERBUILD-001`按用户决定标`ABANDONED`；已写helper与Temp artifacts不擅自回退或删除。
当前返回Unity C#战斗逻辑差异主线，WP01F不标完整双runtime VERIFIED。

2026-08-23 WP01G综合更新：all-diff register的68个D-ID已按20项限定范围关闭/明确Unity证据、20项
代码差异关闭或高层Unity证据但trace/样本受限、19项代码已写/映射而joint/Play待证、7项
source/reachability UNKNOWN/INFERRED、2项批准adapter/未来决策完成无遗漏分类；集合校验missing0/extra0。
当前没有“C++ source-confirmed、Unity尚未修复、production可达性已闭合”的可直接脚本修改项。
`R8-WP01G`只完成证据综合，不宣称R8或C++ runtime完整对齐；下一推荐是先只读闭合R2最早依赖的
`D-SCHED-006/008`，确认真实first difference后再建立修复Change。

2026-08-23 WP01G-R01更正：只读source closure完成后，初次“无未修复source-confirmed差异”的结论已被
部分supersede。`D-SCHED-006`两次character-DAT Z clamp在canonical字段/时点上source等价，高槽扫描保留为
批准容量adapter；状态为`RUNTIME_PENDING`而非C++ runtime VERIFIED。`D-SCHED-008`确认一个F1/step-wait
条件性差异：C++在render后提前return会跳过entity tail并把candidate carrier保留到下一tick，Unity却已在
object consume后失效store/range，并在下一collect开始时重置carrier。该差异可改变下一tick count、20-cap、
ordinal、nearest tie/RNG与consume结果。当前68项分类更正为20/20/20/5/2，另有1项未修复source-confirmed
条件差异。下一实际修复包是`R2-CANDIDATE-TAIL-01`；必须整体处理normal/pause/resume/store/pool/0B，
不得简化为只清count或只删除EndConsumption。

2026-08-23 WP01G-R01B更正：`D-STEP-001`不再UNKNOWN。C++ Release的A→B→C unlock writer在
`main.cpp` BATTLE outer loop无条件参与release；成功后flag1/process进度3保持。flag1的F1 wait仍跳过input，
但不会在render后early-return，会继续postprocess/late/tail。Unity目前没有flag/progress或deterministic
debug-command producer，故为第二项未修复source-confirmed difference；68项现为A20/B20/C20/D4/E2/F2。
是否移植属于`R3-STEP-01`用户policy决定。`R2-CANDIDATE-TAIL-01`必须按actual tail-skip predicate保留
carrier：flag0+stepWait retain，未来flag1+stepWait normal clear，不能硬编码所有stepWait。

2026-08-23 WP01G-R03运行时联合认证更新：真实`NTSD_Battle`中physical DDJ→frame271、DRA→frame263、
D/K→Right→jump→airborne→landing均PASS；held weapon、grab/CPoint/link、collision/hit/damage三组live-world
探针亦PASS并完整恢复world/slot/pool/global基线。本轮未观察到production gameplay first difference；仅新增
Editor-only F2 probe。该结果证明本轮夹具覆盖路径的Unity联合接线，不升级未被DAT/场景触达的特定D-ID为
C++ runtime VERIFIED，R1-WP02 full trace继续BLOCKED。证据见
`docs/ai/RESEARCH/R8-WP01G-R03-joint-runtime-evidence-20260823.md`。

WP01G-R03最终收口：final Unity compile/Console 0 error，8个相关EditMode类257/257 PASS，完整
`BattleRuntimeSelfCheck`于17:17:19 PASS，Change Ledger validator与scoped diff/whitespace检查PASS。
R03状态为`COMPLETE AT AVAILABLE EVIDENCE`；保留R1-WP02 C++ full trace blocker与各exact-unreachable
分支的`RUNTIME_PENDING`，不把联合夹具PASS扩大成全项目完整对齐。

WP01G-R03证据更正：完成后current Temp报告被fresh重跑的一次性首键采样失败覆盖，故R03临时重开。
`R8-JOINTINPUT-PROBE-002`仅给Editor probe增加最多8次release→press物理状态脉冲，不调用
InputSystem.Update、不写runtime。current fresh F2 D/K attempt2/1、DDJ attempt1/1/1→271、
DRA attempt1/1/1→263均PASS，证明修复目标是自动证据采样可重复性；production gameplay未改。

WP01G-R03证据更正最终收口：current focused8类257/257 PASS、full self-check 17:33:15 PASS、
Console error0、Change Ledger validator79/93 PASS。一次W05B瞬时失败在隔离8/8及全组复跑中转绿，且
本Change未触碰W05/slot/pool。R03恢复`COMPLETE AT AVAILABLE EVIDENCE`；真实人手硬件/窗口焦点edge
仍由用户验收，C++ full trace仍BLOCKED。

2026-08-23 下一工作包审计：R03后最早且仍缺正常战斗联合Play证据的完整链为AI sensing、
39-position decision/RNG、canonical FrameInputSet、movement/skill/opoint与hit，对应`D-INP-005`、
`D-INP-007A/B/008/009`。已建立`R8-WP01G-R04-ai-sensing-decision-action-joint-runtime.md`，
状态`PLANNED / APPROVAL PENDING`。本包不以1000 AI性能替代行为证据，不预设production修复；
批准前不运行Play、不新增probe、不修改AI production。

2026-08-23 AI范围更正：用户决定不再执行R04；未来AI使用Unity状态树或行为树，不要求复刻C++
sensing、39-position decision与RNG算法。`D-INP-005/007A/007B/008/009`从对齐backlog移除并保留
历史代码/测试，不回退已有实现。未来AI仍必须按30Hz固定tick产生canonical FrameInputSet，不得绕过
输入边界直接写战斗runtime。当前重新审计非AI正常战斗逻辑的剩余对齐项。

2026-08-23 非AI剩余审计：当前没有新发现的source-confirmed未实现normal-combat代码差异。仍可执行的
Unity运行证据分4组11个D-ID：candidate/PreInteraction、negative-link/P1P2、OID51 merge/split、central
handoff/writeback。另有5个current DAT/fixture不可达exact分支、1个人手硬件输入项、F7/F8/F9功能键
debug policy以及R1-WP02 full trace blocker。详见
`docs/ai/RESEARCH/R8-WP01G-post-ai-non-ai-residual-audit-20260823.md`；下一建议为G1
`D-SCHED-007 + D-PERF-001`，尚未建立或执行R05。

2026-08-23 WP01G-R05启动：用户已批准candidate/PreInteraction adapter joint runtime certification。
固定先验证`D-SCHED-007`同世界candidate内容/顺序/cap/RNG/consume A/B，再验证`D-PERF-001`no-op与
frame/CPoint/link/holder/generation变化矩阵及0B。当前先只读source/crosswalk和现有工具盘点；缺probe时
必须先建test-only Change Record，不预设production修复。

2026-08-23 WP01G-R05收口：`D-SCHED-007`与`D-PERF-001`达到`UNITY JOINT S4 PASS / C++ FULL TRACE
BLOCKED`。candidate focused9/9+58/58、consume185/185、PreInteraction15/15；collision/hit与grab/CPoint
live Play PASS。相同seed的50-AI current/forced-legacy均SmokePassed，20项parity/lockstep hash全等，
zero-GC与cleanup PASS；current35/35 store+oracle且mismatch/invalid/fallback0。唯一脚本Change
`R8-CANDSTORE-DIAG-001`只修stress validator把consume后carrier误当精确entry count的假失败；不改gameplay。
fresh stress Editor256/256、self-check18:35:05、Console0、ledger80/94 PASS。R05不自动进入G2/G3/G4。

2026-08-23 G2/R06只读预检：`D-INP-001`自然type0 negative-link路径仅由opoint kind2产生且child明确为
AI-controlled，按用户AI范围决定不再伪造non-AI Play；现有eligibility修复保留。`D-INP-004`则发现正式差异：
C++ P2默认方向键+numpad3/1/2并完整poll，Unity `Player_2` action map只有Move，Attack/Jump/Defend缺失，
因此P2无法产生动作/组合键canonical packet。已建立`R8-WP01G-R06-p1p2-physical-input-runtime.md`，状态
`PLANNED / APPROVAL PENDING`；批准前不改InputAction asset、生成wrapper或probe。

2026-08-23 G4只读预检：`D-SCHED-009`与`D-RENDER-001..005`当前没有新发现的source-confirmed
production实现缺口；普通CentralOnly Game/SceneView submission已有S4证据，但不能自动关闭hit-record
writeback、pending/dormant/generation、current-DAT shadow identity、visibility cache与fail-closed ownership。
G4按依赖拆为R07A（SCHED-009+RENDER-002）、R07B（RENDER-003/004/005）和R07C（RENDER-001）。
第一包`R8-WP01G-R07A-render-writeback-joint-runtime.md`当时建立为`PLANNED / APPROVAL PENDING`；
该历史状态已被下方2026-08-23收口记录覆盖。

2026-08-23 G4/R07A收口：用户批准后仅新增Editor-only联合probe，production未改。actual kind0 producer→
frozen HitRecord→same-tick live writeback→formal central captured command→Late幂等→next-tick producer/RNG及
no-publication lifecycle均在worker Play通过；最终分类为
`D-SCHED-009 + D-RENDER-002 = UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。R07B、R07C与R08未由本包
执行，仍按各自Task Contract等待独立批准。

2026-08-23 G4后续包可行性闭合：正式`data.txt`含OID7/8/51/223/224；pending/dormant/generation/
death/effect/hit-stop均有production producer。R07B合同要求只由正式producer产生lifecycle/visibility结果，
不得直接写字段；R07C使用现有Editor-only feature registration/failure plan/submission lease，不修改URP asset，
并必须在finally恢复真实feature/material。R07B与R07C Task/Handoff均已建立，状态`PLANNED /
APPROVAL PENDING / NO EXECUTION`。这是当时的历史状态；R07B已被下方收口记录覆盖，R07C仍保持该状态。

2026-08-23 R07B合同纠正：原合同一边要求在R07B中验证OID7/8→51 dormant/split，一边又把独立R08列为
out-of-scope，无法独立收口且会重复同一Play。现R07B只处理`D-RENDER-003`的pending/generation/T+1子集及
`D-RENDER-004/005`；merge/dormant/split与同槽恢复只归R08。R08完成前不得整体关闭`D-RENDER-003`。

2026-08-23 G4/R07B收口：只新增Editor-only联合probe，production gameplay/renderer/DAT/scene/URP 0改动。
sync full-tick Play tick202→203证明pending `slot51/gen1`释放、Late OID999同槽`gen2`、T冻结不受late污染、
T+1新generation body/shadow恢复；正式OID223/224 body正常提交，shadow按current-DAT gate为
`CommandSuppressed`且无命令/提交，baseline正式角色body/shadow正常。focused24/24+9/9+worker18/18、
full self-check、Console0与ledger均PASS。分类为：`D-RENDER-003 pending/generation/T+1 subset = UNITY
JOINT S4 PASS`，`D-RENDER-004/005 = UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`。dormant/split仍只归
R08，R07C仍未执行。证据见
`docs/ai/RESEARCH/R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。

2026-08-23 G3真实Play可行性闭合：state2即正式Running，`data.txt`含OID7/8/51。已建立
`R8-WP01G-R08-oid5152-merge-split-central-runtime.md`，只允许配置初始slot/team/position/HP/running前置，
merge/dormant/split必须由完整tick maintenance产生；split优先正式DJA，若DAT不可达则完整推进4500 fixed
ticks，不得直接写Unk338=0。状态`PLANNED / APPROVAL PENDING / NO EXECUTION`。

2026-08-23 G4/R07C执行结果：仅新增Editor-only probe，production gameplay/renderer/URP asset/scene/material
0改动。真实URP Play中current/stale/replacement均259 isolated pixels且hash一致，Central owner、Legacy
suppressed、tick/gen/lease/retire/checksum/cleanup全部PASS；cold由exact self-check PASS但Play未运行。final Play
同时发现`B-R8-R07C-01`：异步加载期间已有active central submission，随后
`BeginBattleAllocationSeal→PrepareBattleCapacity`尝试resize并抛异常。R07C因此BLOCKED，不得关闭
`D-RENDER-001`。已建立独立repair `R8-WP01G-R07C-R01`为`PLANNED / APPROVAL PENDING / NO EXECUTION`；

2026-08-23 R07C-R01收口：用户批准后实施Camera-preserving repair。最终实现不关闭World/UI Camera或
Canvas；首次`BeginBattleAllocationSeal`在presentation capacity prepare前清退旧central publication，
双seal完成后的重复调用strict no-op。fresh compile0、focused20/20、23:13:13 full self-check、normal Play
`ScenesCamera.enabled=true`/Console0、R07C current/stale/replacement 259px/hash一致及Combat1000 180 sampled
ticks 0 B/0 collection/cleanup restored均PASS。`B-R8-R07C-01`关闭，R07C按现有Unity S4证据收口；C++ full
trace仍BLOCKED，R08未自动启动。上段“未批准前不改production”的表述是repair获批前的历史stop condition，
现已由本收口取代；仍不得在没有新批准时进入R08。

2026-08-23 R08启动后只读阻塞：用户已批准R08，但当前Unity项目缺`data.txt`声明的OID7
`rock_lee.dat`、OID8 `chiyo.dat`、OID51 `sasori.dat`，正式loader会跳过对应wrapper，触发Task stop
condition。已记录`B-R8-R08-01`与`R8-MERGESPLIT-001 / BLOCKED / NO SCRIPT WRITTEN`。相邻`ntsd_proto`
同名加密DAT未被擅自复制；恢复需用户恢复/确认当前Unity适配资产来源。production gameplay、C++、DAT、
probe和专项Play均0改动/未执行。

R08资源调查correction：实际C++运行DAT位于`J:\QQFile\NTSD2.4\chars`，不是`ntsd_release`源码树。
OID7/8/51 runtime DAT与`ntsd_proto\ntsd_assets\chars`同名文件的长度/SHA-256全部不同，后者不得冒充权威
或Unity适配资产。本调查只读，未复制或解密；`B-R8-R08-01`仍需用户恢复当前Unity适配版DAT后解除。

2026-08-24 R08最新first difference：资源与sprite range blocker关闭后，真实Play已通过OID7/8→51 merge runtime、
Central merged body和dormant suppression。OID51 frame290正式`hit_ja=0`，故按C++合同推进4500 fixed ticks；final
maintenance在dormant partner原slot/generation reset期间，AI unified row publisher抛stale generation异常并中断split/
cleanup。已记录`B-R8-R08-03`，拆出`R8-WP01G-R08-R02 / R8-AIROWGEN-001 / PLANNED / APPROVAL PENDING`。
批准前不得修改production、削弱generation fail-fast或宣称R08完成。

R08-R02只读预检已确认generation本身未推进；失配来自dormant row membership与仍active的unified snapshot。
推荐最小方向是通用row-membership invalidation：merge进入dormant与split reset/reactivate前结束当前publisher，令下一
tick强制完整rebuild；保持四类store原generation绑定，不release slot、不吞`ValidateRow`。该方向仍待用户批准后才可
写focused test和production修复。

2026-08-24 R08-R02获批：用户明确批准`R8-WP01G-R08-R02 / R8-AIROWGEN-001`并恢复目标。按先失败focused
reproduction、再通用row-membership invalidation、最后完整R08的顺序执行；批准不扩展到generation/allocator/
AI策略/ValidateRow/DAT/render或T8。

2026-08-24 R08-R02与R08 Unity S4收口：通用row-membership invalidation已关闭dormant split stale-row异常；
fresh compile0，focused merge/split2/2、unified21/21、live-slot37/37 PASS。最终正式Play完成4500 ticks并通过
OID7/8→51→7/8、dormant、current-half HP/HPMax、原slot/generation、Central merged/dormant/split visibility及
generation-safe cleanup；result=`Temp/NTSD_R8_WP01G_R08_Oid5152MergeSplit.result.json`，status PASS，
cleanup恢复world/claimed/object pool/logic pool/RNG。`B-R8-R08-03`关闭，R8-MERGESPLIT与R8-AIROWGEN均VERIFIED。
full self-check仍由独立`R-HC-01`前置阻塞，R1-WP02 full trace仍BLOCKED；因此只宣称该R08 Unity S4行为闭合，
不宣称整个C++ runtime已有动态trace完全对齐。

2026-08-24 post-R08验证基础设施preflight：原11个可执行非AI D-ID均已完成到Unity S4/source-deferred边界。
当前full self-check被`R-HC-01`的5个正式negative-height body阻塞；C++ `hit.cpp/collision_collect.cpp`与Unity
production均按raw `y2=y1+h`和strict overlap处理；`h=-999`形成倒置rect，普通小itr不命中但跨越两个
倒置端点的大itr仍会命中。已建立
`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001 / PLANNED / APPROVAL PENDING`，仅计划修self-check分类与回归，
禁止改DAT/parser/production collision。该包批准并执行前，full self-check仍保持BLOCKED。

2026-08-24 R08-R03获批：用户明确批准`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001`并恢复目标。仅允许
修正`BattleRuntimeSelfCheck`的negative-height body分类与production collector raw strict-overlap回归；production collision、
DAT、parser、角色技能、AI、render、T8和服务器均不改。

2026-08-24 R08-R03收口：fresh compile0；full self-check越过R-HC-01，137 definitions中90个zero-width itr、
5个known negative-height body、0 unexpected/other，ordinary/enclosing × right/left四矩阵均PASS。
`R8-GEOMETRYCHECK-001 / VERIFIED`。随后self-check在独立旧AnimationConfig Naruto硬编码路径失败；已拆
`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001 / PLANNED / APPROVAL PENDING`，不得在R03内顺带修复。

2026-08-24 R08-R04获批：用户明确批准`R8-DATFIXTUREPATH-001`并恢复目标。仅修改self-check按objectId读取
当前`ObjectDefinition.file`及其production DAT test callsites；不修改production loader、catalog、data.txt、DAT或gameplay。

2026-08-24 R08-R04收口：self-check production DAT fixtures现按objectId读取正式`ObjectDefinition.file`；11个callsite
迁移且旧literal清零。fresh compile0、CharacterAssetDeployment1/1、02:27:38 full self-check PASS、最终Console0。
`R8-DATFIXTUREPATH-001 / VERIFIED`；production catalog/loader、data.txt、DAT/BMP和gameplay均0改动。

2026-08-23 WP01G-R06收口：`D-INP-004`的P2 physical source差异已修复。Player_2新增
Attack/Jump/Defend与numpad1/2/3 exact binding，wrapper由Unity Input System正规生成，既有crossed adapter与
8-slot扩展保持。two-human production Play完成11/11 physical press/held/release/no-cross，focused2/2、input
regression47/47、full self-check 19:37:29和Console0均PASS。状态为`UNITY INPUTSYSTEM S4 PASS / C++ FULL
TRACE BLOCKED`；不扩大成C++ executable动态认证或真实人手硬件edge验收。证据见
`docs/ai/RESEARCH/R8-WP01G-R06-p1p2-physical-input-runtime-evidence-20260823.md`。

认证矩阵至少包括：

1. walk/run/jump/turn/landing；
2. 人类输入、组合键、AI 输入；
3. character/weapon/special/effect 四类对象；
4. 拾取、持有、投掷、抓取、死亡、复活、随机武器；
5. 多目标 collision、递归 opoint、slot reuse；
6. `Authority400` 下的 C++ source-contract 对照；若 R1-WP02 已解除 blocker，则追加 400-slot full trace；
7. `MobileExtended` 下 1,000 active 实体，以及 `DesktopExtended` 无固定产品cap、prebattle reservation与sealed overflow合同的非回退验证；它们是 Unity 性能/容量附加门，不替代 C++ 行为证据；
8. 中央表现（包含 `CentralOnly`）下的 entity/shadow/hit-record 可见性与排序验收，不依赖 Legacy `SpriteRenderer`；
9. Windows Mono仅作为已有附加证据；IL2CPP按用户要求排除后续处理；Android真机由用户提供结果；
   T8默认`stage.dat`继续按用户要求暂缓。

只有所有在范围内的模块均具备 C++ release source 合同、对应 Unity 差异已关闭、所需定向/集成验收完成，且保留的 Unity 中央渲染与容量 profile 未被回退时，才能再次宣称“Unity 战斗场景与 C++ release 对齐”。full trace 若可用，会提升证据强度；若仍 BLOCKED，必须在结论中明确该限制，不能把它伪装成已取得。

## 5. 首个实际代码批次

在 R1 的“C++ 源码行为合同、Unity 差异清单、子流程验收合同”完成前，不改 gameplay。自动 full trace 不是代码批次的前置门槛。R1 主线完成后，第一个代码批次只能是 **R2 的主调度器 pass 边界**，优先处理 CPoint/WeaponSync/held 与 candidate/collision 的时序错位。

原因是该错位会同时影响：输入后的首帧技能、抓取、持有武器、投掷、opoint 子对象、命中候选和表现挂点。如果先修单个角色或技能，只会把本应由 tick 顺序保证的行为散落到专项补丁中。

## 6. 不在本计划中做的事

- 不删除现有 ECS、SoA、worker、中央 Mesh 或对象池；
- 不把 Unity 变成 C++ 源码直译；
- 不重新引入每实体 SpriteRenderer 作为生产渲染路径；
- 不回退 `CentralOnly`、Texture2DArray/atlas、动态 Mesh、URP 接入或现有中央 command/descriptor 架构；
- 不把 `Authority400` 变成移动端或桌面端的全局容量上限；不降低 `MobileExtended` 的 1,000 active 合同，也不取消 `DesktopExtended` 的unsealed prebattle page reservation能力；
- 不用改 DAT 文件来掩盖运行时差异；
- 不以性能压测、Unity self-check、单个 hash 或单个技能成功替代 C++ 全链证据；
- 不在完成 R8 前启动“已完全战斗逻辑对齐”的结论；
- 不在本计划中推进 S0～S9 的服务器代码。

## 7. 2026-08-24 R8最终证据对账准备

R08-R04后的只读审计确认：可执行非AI联合证据与完整self-check已到当前允许层级，但父编排、68项D-ID
登记册和旧synthesis仍含被后续包取代的历史pending/blocked文本。已建立
`R8-WP01G-R09-final-evidence-reconciliation.md`与handoff，状态为
`PLANNED / APPROVAL PENDING / DOCUMENT-ONLY / NO SCRIPT CHANGE`。

批准后只统一证据层、剩余边界和R8限定结论；不运行Unity/C++、不改gameplay/资源，也不把Unity S4扩大成
C++ runtime完整对齐。`R1-WP02` full trace继续BLOCKED，T8默认stage.dat继续暂缓。

用户已于2026-08-24明确批准`R8-WP01G-R09`并恢复目标；状态推进为
`IN_PROGRESS / USER APPROVED / DOCUMENT-ONLY / NO SCRIPT CHANGE`。本包开始逐项对账68个D-ID与最新R8证据，
仍不授权任何脚本、资源、运行或架构改动。

### R09 final result

R09已完成：68项最终集合为43项Unity S4/runtime覆盖、5项exact production witness不可得、1项source等价但
full trace缺失、9项用户排除/未来替换、1项调试功能键policy、3项approved adapter/config decision、6项
test/worker/performance事实；总计68、missing0、extra0、duplicate0。

`D-LIFE-001`与`D-RENDER-003`已由R08正式4500-tick Play提升为Unity S4；F1/F2相关`D-SCHED-008`、
`D-SCHED-010`、`D-STEP-001`按用户决定退出正常战斗backlog但保留source差异记录；
`R8-SPRITERANGE-001`在normal Play、R08、完整self-check与validator证据齐全后升级VERIFIED。

R8因此可以表述为“在批准范围和当前可取得Unity证据层完成”。不得表述为“C++ executable runtime full-trace
完整对齐”；R1-WP02、T8、五个exact DAT/fixture不可达分支、人手硬件edge、F7～F9 policy以及未来AI设计仍按
各自边界保留。R09没有修改脚本、scene、config、resource或C++ authority，也没有运行Unity/C++。

R09最终验证：register68、reconciliation68、missing0、extra0、duplicate0；Change Ledger validator
93 records / 111 governed code files PASS；scoped diff check PASS。

## 8. 2026-08-24 post-R09残余验收与功能键最终收口

用户授权对仍有意义的production样板执行验收，并新增按模式控制的F7/F8/F9；全部通过后结束当前目标。

- `R8-WP01G-R11 / R8-AUTHOREDSTATE-PLAY-001 / VERIFIED`：正式OID150 state2000完整tick验证
  正Vx→right、负Vx→left；正式OID32 frame0/state8032验证DAT32/frame0/offset140/effective pic140，
  worker逻辑snapshot经既有主线程materialize后生成18条Central命令，目标body command/catalog/UV一致；
- `R8-WP01G-R12 / R8-FUNCTIONKEYMODE-001 / VERIFIED`：GameConfig exact `gameModeId=0 /
  battleGameModeId=1`显式启用F7/F8/F9；仅LocalFreeRun捕获物理edge并在30Hz tick边界消费；F7写四项500，
  F8/F9复用Mode2生产链，未匹配模式、Manual和LockstepBuffered保持fail closed；
- Play结果：R11于12:22:17 PASS；R12于12:26:06 PASS（F7 tick1581、F8生成9个、F9清7/7个
  tail时仍合格候选，2个此前已转换类型）；两组cleanup均恢复world/slot/pool基线；
- focused：功能键4/4、checksum/snapshot/restore 18/18；full self-check于12:28:41 PASS；
- F1/F2、A→B→C、AI C++ parity、T8、Android、服务器和IL2CPP未进入本轮；C++ authority保持只读，
  R1-WP02 full trace仍BLOCKED。

因此当前目标允许表述为“用户批准范围内的正常战斗对齐与可取得Unity验收已完成”。仍禁止表述为
“已经取得未修改C++ executable的full-trace动态认证”。

## 9. 2026-08-30 formal Kernel Cut C shared-owner 旁证

`CLIENT-FORMAL-KERNEL-SLOT-LIFECYCLE-SHARED-OWNER-001`已在独立Task/Change与用户具名授权范围内关闭为
`FOCUSED_TEST_PASS / SHARED_SLOT_LIFECYCLE_OWNER_READY / S0_NOT_VERIFIED`。三份平台无关的slot allocator、
entity handle和lifecycle state源码/GUID现由Server-owned shared package单一持有，Unity与.NET消费同一物理源码；
`0.3.0` direct/locked artifact、Unity package与slot/lifecycle regressions、S0 witness、existing lockstep、
fresh SelfCheck及Server回归均通过。

该结果是shared ownership与非回退回归旁证，不改变本计划的C++ battle authority、30 Hz、Client adapter、
RuntimeRestStore、战斗规则或R8证据等级，也不把S0/S5提升为`VERIFIED`。后续Cut D先做只读rest/checksum
projection边界审计；任何Client源码移动仍需新的Task/Change和具名授权。
