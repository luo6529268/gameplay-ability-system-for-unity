# R1-SOURCE-007 — 全量差异依赖图与后续修复批次

> 状态：COMPLETED（静态 inventory closure；runtime acceptance pending）。  
> 唯一行为 authority：J:\QQFile\NTSD2.4\ntsd_release 的 release live source。  
> 证据口径：本文件只汇总 R1-SOURCE-001～006 已完成的 VERIFIED(source) 合同、INFERRED 风险和 UNKNOWN；未运行 C++ executable 或 Unity。

## 1. 静态盘点闭合结论

### 1.1 已覆盖的 source family

| Coverage | C++ live family | Unity crosswalk | 静态收口 |
|---|---|---|---|
| COV-001 | game_tick(...) 主 pass | NTSDBattleTickSystem / SimulationWorld | 完成 |
| COV-002 | callback、input_handler、human/AI/combo | FrameInputSet / HumanInput / CharacterInput / AI | 完成 |
| COV-003 | frame_advance、physics、state/death/respawn | FrameAdvance / FrameTick / mechanics / stage bounds | 完成 |
| COV-004 | collision_collect、collision、hit、weapon consume | snapshot / broadphase / candidate runner / typed writers | 完成 |
| COV-005 | cpoint、weapon sync、held/link、opoint、reset/free | CPoint/held/link writer、structural writer、registry/pool | 完成 |
| COV-006 | renderer、render callback、visibility/order/shadow | BattlePresentation / CentralOnly / mesh / URP | 完成 |

因此 R1 的源码盘点本身已经闭合：没有遗留“未指定 C++ live family / 未指定 Unity crosswalk”的 COV-001～006 项。

这不等于：

- 43 个 D-ID 都已证实为可见 bug；
- C++ runtime trace 已取得；
- Unity 已编译、self-check、Play Mode 或 1000 AI 验收；
- C++ 与 Unity 已完整对齐。

### 1.2 现有 inventory 的分类

- D-...：当前 C++ source 与 Unity source 已有静态差异，或已有 source-level timing / mapping 风险；其个体状态见总登记册，不能自动升级为 runtime bug。
- A-...：用户已批准的 Unity adapter / 保护边界；后续代码不能通过删除、回退或关闭它们来取得“对齐”。
- UNKNOWN：缺少 C++ source consumer、DAT 可达性、Unity asset/Inspector binding、runtime first-visible timing 或 GPU behavior 的证据。UNKNOWN 不是遗漏，也不是允许猜测补齐的空白。

唯一总索引是 docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md。

## 2. C++ authority flow 与 Unity repair dependency

### 2.1 C++ live battle flow（用于修复顺序）

    FrameInput / post-cooldown callback
            |
            +-- T03 OID maintenance
            +-- state / frame logic / frame advance / physics
            +-- death / respawn / first Z clamp
            +-- T09 negative-held pass #1
            +-- candidate collect -> character consume -> object consume
            +-- T14 CPoint + weapon sync
            +-- T15 positive-link validation
            +-- second Z clamp
            +-- T16 negative-held pass #2
            +-- preframe / stage / immediate stage spawn
            +-- RenderDispatch
            +-- FramePostProcess
            +-- late entity update -> standard late opoint / cleanup / newborn cursor
            +-- tail / postframe

这里的箭头表示 producer -> consumer，不表示未来 Unity 必须复制 C++ 类或 SDL renderer。
Unity 可以保留 FrameInputSet、slot generation、SoA/ECS、worker、pool、CentralOnly 与 URP；
但每一个 adapter 的结果不得改变该链上的逻辑字段、slot/candidate order 或可观察 battle 表现。

### 2.2 需要先于 consumer 处理的依赖图

    R2 pass spine
      +-- D-SCHED-001..004 --> R4 candidate/hit consume
      |                     --> R5 cpoint/held/link/opoint
      |                     --> R6 first-visible / render handoff
      +-- D-SCHED-005/010 --> R3 input/edge/history
                              --> R3 frame advance / movement

    R3 input + frame/physics
      +-- D-INP-001..003, D-MOV-001 --> current-key / combo / late-frame consumers
      +-- D-INP-002, D-MOV-003 -----> death/respawn / AI lifecycle
      +-- D-MOV-002/004/005 --------> R4 raw-frame and R5 relation/newborn consumers

    R4 candidate/hit
      +-- D-COL-001..005 --> D-HIT-001..003
      +-- candidate/result order --> R5 CPoint, held, weapon, opoint consumers

    R5 relation/lifecycle
      +-- D-LINK-001..002, D-HOLD-001..002, D-CPT-001..002, D-OP-001
      +-- slot/free/pending/newborn contracts
      +-- R6 visibility, held attachment, first-visible tick

    R6 central presentation
      +-- D-RENDER-001..005
      +-- preserves A-RENDER-001..004

    R7 performance re-certification
      +-- only after fallback and optimized paths pass corresponding R2-R6 fixtures

## 3. 后续实施批次（尚未授权执行）

下表是 R1 完成后建议的最小闭合实施批次。它们是 future Work Package / Change ID 蓝图，不是当前已执行代码。

| 批次 | Goal | 覆盖 ID | 前置条件 | 允许的主要 Unity 文件域 | 代码后最小验证 | Stop conditions / 禁止扩张 |
|---|---|---|---|---|---|---|
| R2-PASS-01 | 将 Unity scheduler 拆成可明确命名的 C++ pass boundary，并恢复 held#1、candidate、CPoint/weapon sync、positive link、held#2 的相对时点。 | D-SCHED-001～004 | 每个 writer 的读写集合已在 SOURCE-004/005 记录；先建立 Change Record。 | NTSDBattleTickSystem、SimulationWorld.Passes、pass adapter；不得直接改 renderer。 | 静态 pass-order probe + empty / one character / held relation fixture。 | 若必须改 candidate/hit/CPoint 公式才可继续，停在对应 R4/R5；不得顺手修技能。 |
| R2-PASS-02 | 闭合两次 Z clamp、candidate adapter/end lifecycle、tail / slot cursor 的 adapter 是否结果等价。 | D-SCHED-006～009、011～012 | R2-PASS-01 已稳定；DAT/cursor unknown 显式记录。 | Stage bounds、candidate carrier、registry adapter；不动容量 profile。 | Z/newborn/low-high slot fixture + candidate sequence probe。 | 不能证明时保持 UNKNOWN；不得删除 snapshot/broadphase/generation。 |
| R3-INP-01 | 恢复 callback 内 human/AI 与 OID maintenance 的相对顺序。 | D-SCHED-005 | R2 scheduler spine 已明确 callback boundary。 | frame input / human / character input pass adapter。 | same-tick OID7/8/51 + human/AI input journal fixture。 | 不改 Input Action asset；不改变 FrameInputSet API。 |
| R3-INP-02 | 将默认 F1/F2 slow gate 与 battle-entry clear 分离。 | D-SCHED-010 | R3-INP-01；现有 Flow step fields / snapshot 已确认。 | scheduler / focused self-check；默认不改 runtime state、asset 或 renderer。 | F1 wait、F2 one-step、entry-clear fixture。 | 不用 NeedClearInput 重命名替代 F1/F2；`g_dword_449048` 非零 / physical binding 保持 UNKNOWN。 |
| R3-HOLD-INP-01 | 闭合 negative-link / held-caught input gate。 | D-INP-001 | R3-INP-02；R5 relation / held producer-consumer 合同。 | CharacterInput / held relation adapter。 | negative held/caught、combo、holder/target fixture。 | 不得只删除 LinkState early return；须保留每个 local relation gate。 |
| R3-AI-LIFE-01 | 闭合 HP=0 / respawn AI caller。 | D-INP-002 | R3 frame/death/respawn contract、lifecycle consumer 已可观察。 | AI caller / lifecycle adapter。 | HP=0 / respawn key-prev-history-frame fixture。 | 不得把死亡 AI 一律视为 no-op；不改 AI decision kernel。 |
| R3-INP-03A | 固化 canonical full-held frame packet edge 合同。 | D-INP-003 | C++ `InputHandler::poll` source、FrameInputSet / state-module crosswalk；no code before Task/Record。 | FrameInputSet adapter / test-only fixture；不动 physical asset。 | press/hold/release/multi-key journal fixture。 | `PressedButtons` / `ReleasedButtons` 不是 C++ gameplay truth；若完整 packet 条件不成立，停止并单列 protocol adapter。 |
| R3-INP-04 | 固定 P1/P2 authority fixture 与 Unity 3+ roster extension 边界。 | D-INP-004 | R3-INP-03A、roster binding source contract。 | roster/provider mapping；不改 production capacity。 | fixed P1/P2 fixture；3+ extension diagnostic。 | 3+ 不反向定义 C++ rule。 |
| R3-AI-TGT-01 | 闭合 fallback / indexed AI target equal-distance and cached-target behavior。 | D-INP-005 | Ai source contract / seed / profile fixture。 | AI fallback/optimized dispatch；不改 physical input。 | equal ground/air target profile-pair fixture。 | optimized 未等价不得默认启用。 |
| R3-PHY-01 | 核验 W/S/A/D/J/K/L 到 logical key 的实际 Unity binding。 | D-INP-006 | 用户 Play Mode / asset evidence。 | InputAction asset / Inspector；默认不改。 | user Play Mode matrix。 | 未获 asset/Play Mode evidence 不修改 binding。 |
| R3-FRAME-01 | 修 current key 生命周期、landing raw write、integer sync。 | D-MOV-001～003 | R3-INP-01/02；R4/R5 已列 consumers。 | FrameAdvance、physics/landing writer、integer sync adapter。 | walk/run/jump/air momentum/landing/respawn field fixture。 | 不全局替换为 ImmediateFrame；raw writer subset 未闭合即停止。 |
| R3-FRAME-02 | 判定 dormant / DAT-gated guards，必要时做极小修复。 | D-MOV-004～005 | production writer 与 DAT reachability fixture。 | ThrowFrameGuard consumer、exact-character frame tick。 | nonnegative guard reachability、state2000 Vx facing fixture。 | 若没有 writer/DAT 可达性，标为 no-op/dormant，不做猜测性删除。 |
| R4-COL-01 | 恢复 candidate sequence abort、caught gate、effect21 current-state gate。 | D-COL-001～003 | R2 scheduler settled；R5 cpoint relation fields 可观察。 | candidate runner / typed consumer；broadphase仅作 discovery adapter。 | one attacker multi-target/Loop1->Loop2/caught/effect21 fixture。 | 不通过改 quadtree 排序修复；candidate order 必须保留 C++ slot sequence。 |
| R4-COL-02 | 关闭 DAT reachability 的 transition smoke 与 kind1 target-type gate。 | D-COL-004～005 | 真实 DAT data route / fixture data 已确认。 | collect / interaction gate。 | oid999 and multi-type target fixture。 | DAT 不可达则标待测试；不得为绿灯修改 DAT。 |
| R4-HIT-01 | 补齐 type1/2/3/4 normal damage 的 vital/stat contract。 | D-HIT-001、D-HIT-003 | R4-COL-01 candidate result stable。 | typed damage writer / world stats contract。 | nonlethal/lethal HP/HP max/combo/KillStats/DamageStats fixture。 | 不只搬扣血；字段、frame、death/late consumer必须闭合。 |
| R4-HIT-02 | 给 kind10/11、kind16、weapon response 建 raw frame writer subset。 | D-HIT-002 | R3-FRAME-01 raw-frame contract。 | typed hit / weapon writer。 | frame/Prev/Prev2/wait/attacking/next-tick fixture。 | 禁止全局切换 frame helper；每个 C++ writer独立映射。 |
| R5-REL-01 | 对齐 CPoint raw frame/wait 与 injury global stats。 | D-CPT-001～002 | R2 order + R4 stat/frame contract。 | CPointWriter / world stat writer。 | broken/action/duration cpoint + lethal/nonlethal injury fixture。 | 不把 normal hit stat 当作 CPoint 已完成。 |
| R5-REL-02 | 对齐 positive/negative link invalid cleanup 与 type2 held throw fields。 | D-LINK-001～002、D-HOLD-001～002 | R2 two-held-loop schedule。 | link validator / held resolver。 | stale relation/type2 throw follow-up consumer fixture。 | TrackerFlag/Parent C++ mapping未知时不得重写相关 consumer。 |
| R5-LIFE-01 | 对齐 normal opoint child history、pending/free/newborn/slot reuse。 | D-OP-001、D-SCHED-012、D-RENDER-003 的 logic half | R2/R3/R4/R5 preceding writers stable。 | structural writer / factory / registry/pool adapter。 | low/high slot newborn + nonzero action Prev2 + pending destroy fixture。 | 不回退 extended capacity，generation/pool只是 adapter不能成为行为差异来源。 |
| R6-PRES-01 | 将 central fail-closed diagnostic、snapshot visibility 与 C++ active/render contract闭合。 | D-RENDER-001、003、005 | R5-LIFE-01；CentralOnly boundary保留。 | BattlePresentation / central diagnostics / visibility adapter。 | feature/resource/route + hide/death/pool/last-visible command fixture。 | 不回退 Legacy、不要把错误隐藏成 fallback。 |
| R6-PRES-02 | 验证/适配 spark writeback 时点、dynamic identity shadow gate、held 1.5x anchor。 | D-RENDER-002、004；A-RENDER-002/003 | R4-HIT/R5-REL completed；用户 camera policy保持。 | hit record presentation bridge / resource identity adapter / display-only transform。 | spark next-tick, 223/224 identity, held object/weapon/shadow, stationary-object camera fixture。 | 不将 render-frame/Transform 写成 simulation truth；像素差异无法 source 闭合时保留待测试。 |
| R7-PERF-01 | 逐项重新认证 fallback / optimized / worker / central performance path。 | 所有优化 adapter | 各自 R2-R6 fixture已通过。 | profile switches、diagnostics、fast paths。 | fallback/optimized same fixture result；0-GC/容量/性能另报。 | FPS/GC 改善不能覆盖行为分叉。 |

## 4. 必须先解决的跨模块 blocking relationships

| blocker | 为什么不能绕过 | 解除条件 |
|---|---|---|
| B-INV-01：scheduler 与 CPoint/held 二次 scan | D-SCHED-001～004 决定 CPoint、link、held 的 producer->consumer 关系；先修某个技能会把顺序 bug 固化为专项补丁。 | R2-PASS-01 的 pass-boundary fixture 与 static review。 |
| B-INV-02：raw frame writer family | landing、weapon hit、kind16、CPoint 都涉及 frame/Prev/Prev2/wait/attacking，但不是同一 helper 语义。 | R3-FRAME-01 + R4-HIT-02 + R5-REL-01 分别建立 writer subset。 |
| B-INV-03：candidate result 与 relation lifecycle | candidate / hit consume 会读 grab、holder、target、CPoint relation；held/link/opoint 又改变下 tick consumer。 | R4-COL-01 和 R5-REL-02 / R5-LIFE-01 的联合 fixture。 |
| B-INV-04：presentation first-visible | normal late opoint、pending destroy、pool reuse 与 central capture gate共用 slot/active truth。 | R5-LIFE-01 后才进入 R6-PRES-01。 |
| B-INV-05：R1-WP02 full trace | full trace 被 BLOCKED，不能伪造为现有证据；但 source inventory / focused fixtures 仍可推进。 | 用户提供安全、只读、可重复的 external observation/replay 方案后另行恢复；不是 R2 的自动前置。 |

## 5. 不可回退项的批次级约束

所有 R2～R7 code package 必须在 Change Record 中逐条重申：

1. 不恢复 production Legacy SpriteRenderer，不关闭 CentralOnly；
2. 不删除 Texture2DArray/atlas、dynamic Mesh、URP RenderFeature；
3. 不把 C++ 400 slot 变成 MobileExtended/DesktopExtended production cap；
4. 不把 camera/Transform/URP render timing反写进 battle runtime；
5. 不破坏 30 Hz、FrameInputSet、worker、SoA/ECS、pool、zero-GC 战斗期边界；
6. 不通过 DAT、scene 或 prefab 修改掩盖 logic discrepancy；
7. T8 default stage.dat 仍暂缓。

## 6. R1 之后的诚实报告口径

R1 完成后唯一允许的结论是：

> “C++ Release→Unity 的 COV-001～006 已完成静态源码盘点；已建立差异总台账、依赖与验收矩阵。R2 gameplay 修复尚未开始，C++ full trace 仍 BLOCKED，运行时/Play Mode/性能验收待后续。”

禁止把这一结论缩写成“战斗逻辑已对齐”或“所有差异已修复”。
