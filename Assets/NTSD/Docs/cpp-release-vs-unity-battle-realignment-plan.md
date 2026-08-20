# NTSD C++ Release → Unity 战斗场景重新对齐计划

> 建立日期：2026-08-20  
> 当前状态：**方案已确认，尚未开始改动战斗代码。**  
> 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 中参与 `ntsd_new.exe` release 构建的 live battle runtime。  
> C++ 正式入口：`src/entity/game_tick.cpp` 的 `game_tick(...)`。  
> C# 工程用途：历史移植辅助、命名线索、已有 Unity 自检回归；与 C++ release live path 冲突时不具备裁决权。

## 1. 目标

在**保留** Unity 已完成架构和性能成果的前提下，使 Unity 战斗场景的逻辑时序和最终可观察表现重新对齐 C++ release：

- 保留 `SimulationTickDriver -> NTSDBattleTickSystem -> SimulationWorld`；
- 保留固定 30 Hz、`FrameInputSet`、slot/generation、SoA/ECS store、对象池、中央渲染、Texture2DArray、worker、0 GC 和 1000 AI 性能能力；
- 不将 Unity 改写为 C++，不引入 Unity DOTS，不回退到逐实体 `SpriteRenderer`；
- 不因性能优化而改变 C++ 的 pass 顺序、slot cursor、RNG、输入边沿、命中、opoint、持有/抓取、生命周期或 render handoff；
- 每次只处理一个闭合模块，完成 C++ trace、Unity legacy/fallback、Unity optimized path 和真实 Play Mode 四层验证后才进入下一模块。

这里的“对齐”同时包含：帧号、位置、速度、朝向、命中、伤害、关系字段、对象生成/回收、声音/火花事件时序，以及战斗中实体、阴影、挂点和层级的最终可见结果。

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
4. 不做一次性重写；每次提交只允许一个模块和其必要的 trace/回归夹具。
5. 每个 optimized path 必须有同 tick fallback 或 shadow 对照入口；优化只在证明与 C++ trace 等价后保持默认开启。
6. 任何 fast path 发现首个 C++ observable mismatch 后，立即停止该路径的进一步推广；先保存 witness，再修正对应模块。
7. Unity 的 Transform、Camera、Mesh、Sprite、Animator、URP 回调都只能读取逻辑/表现快照，不能反写战斗真值。
8. `Authority400` 是 C++ 同 slot trace 的首个对照口径。`MobileExtended`、`DesktopExtended` 可继续用于性能，但不用于宣称与 C++ 固定 400 槽逐格相同。

## 4. 分阶段实施

### R0：权威迁移与历史证据分级

**目的**：终止“C# self-check 通过 = C++ 对齐”的误判，不改战斗行为。

- 根 `AGENTS.md`、主要对齐/架构/渲染文档统一声明 C++ release live path 为唯一 authority；
- 旧 C# 结论标记为“历史回归证据”，不删除，不篡改当时事实；
- 建立本文件作为后续唯一执行顺序；
- 盘点所有 Unity `C# authority` 注释、C# trace、fast-path proof 和旧对齐条目，按“可复用回归 / 必须重审 / 已明确与 C++ 冲突”分类。

**完成条件**：未来任务不会再把 C# 作为最终裁决；不涉及 Unity 代码或 C++ 代码改动。

### R1：C++/Unity 双端 tick trace 合同

**目的**：先获得可比较事实，避免凭观察逐个猜技能。

#### R1.1 C++ release trace

只在 release live path 的观察点记录，不改变 gameplay：

- tick index、RNG seed/state、stage bounds、camera/render carrier；
- 每个 slot 的 active、oid、runtime category、frame、prev_frame、prev_frame2；
- `x/y/z`、`x_int/y_int/z_int`、`vx/vy/vz`、facing；
- HP/PP、frame delay、hit stop、arest/vrest、attack/hit counters；
- link/holder/target/caught/catcher；
- candidate collect 与 consume 顺序；
- opoint、随机武器、死亡/回收、slot reuse；
- render handoff 的 entity/shadow/hit-record descriptor。

#### R1.2 Unity trace

在 `Authority400` 下导出同 schema、同阶段名的逻辑 trace。中央渲染只导出 command/descriptor，不把 Mesh/Camera Transform 作为战斗真值。

#### R1.3 对照输入

- 固定 seed、DAT 语义夹具、初始 slot、stage 数据和输入 journal；
- 人类输入只在 `FrameInputSet` 边界进入；
- first difference 必须报告：tick、pass、slot、字段、C++ 值、Unity legacy 值、Unity optimized 值和最短重现步骤。

**完成条件**：能够用同一条输入 journal 逐 tick 找到第一个 C++/Unity 分叉；没有任何 gameplay 修复混入这一阶段。

### R2：主调度器与 pass 边界对齐

**目的**：先解决会放大所有技能差异的顺序问题。

- 以 C++ `game_tick(...)` 的真实顺序重建 `NTSDBattleTickSystem` 的 pass map；
- 重点重审并按 C++ 调整：第一次 Z clamp、step5 held/link、candidate collect、角色/对象 collision loop、随机武器、CPoint、weapon sync、第二轮 held/link；
- `PreInteractionTickAll` 不再因为历史命名而拥有超出 C++ 时点的 CPoint/WeaponSync 副作用；必要时拆为 C++ 同名的无分配子 pass；
- 保留 `SimulationTickDriver`、worker、FrameInputSet 和 phase diagnostics；只改调度时点及对应的 Unity adapter；
- 先运行 legacy/完整扫描路径，再启用 proof/skip path 逐项 A/B。

**完成条件**：普通输入、空场、单角色、角色+武器、角色+技能对象五种夹具在 C++/Unity 主 pass trace 上没有顺序分叉。

### R3：输入、帧推进、移动与物理

**目的**：恢复最直接影响战斗手感的基础链。

- `post_cooldown_input`、human poll、AI input、组合键、按下/按住/释放、输入清理 gate；
- state 400/401/500/501、frame logic、frame advance、wait、next、turn、frame delay；
- 跑步、跳跃水平动量、空中速度保留、地面摩擦、重力、落地、Y/Z/X 整数同步；
- state14、死亡、复活、9998 清理与特殊 OID runtime maintenance；
- 明确哪些 Unity 的 `CharacterInput`/SoA writer 可保留，哪些必须把 C++ slot 时点恢复后才能继续使用。

**完成条件**：走、跑、跳、转向、受击硬直、死亡/复活的 C++ tick trace 与 Unity legacy/optimized trace 一致；真实 Play Mode 的体感和帧号链复核通过。

### R4：碰撞候选、命中、抓取与武器交互

**目的**：恢复“同 tick 谁先命中、命中几次、谁被抓/拾取”的规则。

- C++ `collision_collect` 采集时点、slot 顺序、几何/team/type gate、candidate freeze；
- step7 character loop 与 step9 object loop 的消费顺序；
- kind 0/1/2/3/4/5/6/7/8/9/10/11/14/15/16 的 C++ live side effects；
- arest/vrest、attack exempt、multi-hit、abort、held weapon、拾取、投掷、破武器；
- `BruteForceSceneQuery`、Loose Quadtree 和 role-aware broadphase 只可优化候选发现，不能改变 C++ 最终候选序列。

**完成条件**：同一 attacker 的多目标命中、抓取、拾取、投掷、落地与连续命中均有 C++/Unity trace；优化查询与完整 slot scan 的最终 candidate/consume 序列相同。

### R5：CPoint、held、opoint 与生命周期

**目的**：处理当前最容易体现为“技能出了但后续不对”的链路。

- step5/step10/step12 的 held/link/cpoint 各自职责和时点；
- holder current frame/wpoint、held frame/facing/frame delay、center/wpoint 整数挂点公式；
- release、throw、cover、hit/release、holder/victim link 清理；
- late frame tick、late opoint、递归 opoint、多对象展开、newborn cursor、pending unregister、slot reuse；
- death opoint、state transition effects、N-30、broken weapon、postframe timer。

**优先场景**：Naruto 防下攻分身、Naruto 防前跳螺旋丸、Naruto 奔跑防跳抓取后续、普通武器拾取/投掷、oid122/123 持有物。

**完成条件**：每个场景在 C++ 和 Unity 中的对象数量、slot、frame、位置、速度、link、first visible tick、攻击/命中链均一致。

### R6：C++ render handoff 与 Unity 中央表现收口

**目的**：保持中央渲染性能，同时恢复 C++ 战斗场景可观察表现。

- C++ `renderer.cpp` 的 z sort、同 z slot 稳定顺序、shadow → entity → hit-record 交错顺序；
- `x_int/y_int/z_int`、center、sprite rect、facing、frame delay jitter、render offset、shadow anchor；
- 明确 C++ `camera_x`/perspective 与 Unity 固定逻辑 camera、`BattleCameraSafeArea`、1.5 visual scale 的边界；
- 需要逐项决定：哪些 C++ 镜头行为是 Unity 场景必须复刻的可观察表现，哪些是用户明确保留的 Unity 适配。未作决定前不得把二者混入战斗真值；
- 中央 Mesh、Texture2DArray、atlas、batch segment、URP RenderFeature 仅可改变提交方式，command 顺序和可见结果必须与 C++ descriptor 对齐。

**完成条件**：角色、武器、技能对象、阴影、spark、held object 的 command trace 与 C++ render handoff 一致；Game/Scene 视图不再依赖 Legacy `SpriteRenderer` 回退才能看见实体。

### R7：性能优化逐项重新认证

**目的**：不牺牲性能成果，但让每项优化重新获得 C++ 行为许可证。

对下列路径逐项执行 `C++ trace -> Unity fallback -> Unity optimized` 三向比较：

- PreInteraction no-op proof；
- LateEntityUpdate exact-character skip；
- Frame/AI SoA writer；
- broadphase / Loose Quadtree；
- cached snapshot、frozen presentation、central renderer；
- worker publication 和 presentation acknowledgement；
- pool、slot allocator、dynamic capacity。

**晋升规则**：只有三轮同 seed 的所有 C++ observable 字段一致，并且正式窗口保持 0 B、无 Gen0/1/2 collection、无 capacity fault、无对象丢失时，优化才保留为默认。否则保留为 diagnostics 或回退路径，不能用 FPS 改善覆盖行为分叉。

### R8：完整战斗场景认证

**目的**：形成真正的 C++ release → Unity production certificate。

认证矩阵至少包括：

1. walk/run/jump/turn/landing；
2. 人类输入、组合键、AI 输入；
3. character/weapon/special/effect 四类对象；
4. Naruto 分身、螺旋丸、奔跑防跳；
5. 拾取、持有、投掷、抓取、死亡、复活、随机武器；
6. 多目标 collision、递归 opoint、slot reuse；
7. 400-slot Authority400 完整 trace；
8. 1000 AI 性能矩阵作为**性能附加门**，不替代 C++ 400-slot 行为 certificate；
9. Windows Mono/IL2CPP Player；Android 真机由用户提供结果；T8 默认 `stage.dat` 继续按用户要求暂缓。

只有取得同 seed、同输入、同 tick、同 C++ observable domains 的完整证据，才能再次宣称“Unity 战斗场景与 C++ release 对齐”。

## 5. 首个实际代码批次

在 R1 trace 合同完成前，不改 gameplay。R1 完成后，第一个代码批次只能是 **R2 的主调度器 pass 边界**，优先处理 CPoint/WeaponSync/held 与 candidate/collision 的时序错位。

原因是该错位会同时影响：输入后的首帧技能、抓取、持有武器、投掷、opoint 子对象、命中候选和表现挂点。如果先修单个 Naruto 技能，只会把本应由 tick 顺序保证的行为散落到专项补丁中。

## 6. 不在本计划中做的事

- 不删除现有 ECS、SoA、worker、中央 Mesh 或对象池；
- 不把 Unity 变成 C++ 源码直译；
- 不重新引入每实体 SpriteRenderer 作为生产渲染路径；
- 不用改 DAT 文件来掩盖运行时差异；
- 不以性能压测、Unity self-check、单个 hash 或单个技能成功替代 C++ 全链证据；
- 不在完成 R8 前启动“已完全战斗逻辑对齐”的结论；
- 不在本计划中推进 S0～S9 的服务器代码。
