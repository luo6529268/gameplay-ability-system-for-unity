# R8-WP01G-R04 — AI sensing → decision → action → hit joint runtime certification

> 建立日期：2026-08-23  
> 状态：`ABANDONED BY USER / NO EXECUTION / NO CODE CHANGE`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source（只读）

## Goal

在真实`NTSD_Battle` Play Mode中，把AI的target sensing、cache/refresh、special scan、39-position
decision、RNG gate/order、canonical `FrameInputSet`、runtime key/cooldown、移动/技能frame、opoint、
collision/hit/damage串成同一条可观察证据链，推进`D-INP-005`、`D-INP-007A/B`、`D-INP-008/009`
从自动化代码证据到Unity真实场景联合证据。

本包先验证、不预设production修复。发现first difference后必须停止对应fixture，另建独立
Task/Change Record并重新取得授权；不得在R04内顺手改AI production。

## Why this is next

- `D-INP-006`已由R03取得current InputSystem S4，真实人手硬件/窗口焦点由用户验收；
- player movement、interaction/hit、opoint/lifecycle、render和1000实体性能已有各自Unity S4/性能证据；
- AI sensing与39-position dispatcher已有source mapping、286项矩阵、RNG/order与Legacy/DataOriented shadow，
  但真实AI角色Play仍明确`RUNTIME_PENDING`；
- 这是当前最早、仍会直接改变正常战斗手感和技能表现、且能够独立验收的完整 gameplay 链。

## Scope

### 允许

1. 只读复核C++ `src/input/input_handler.cpp:1209-1235,1615+`到39-position decision的caller、
   branch、RNG和input writer顺序；
2. 只读核对Unity `SimulationWorld.AiInput`、unified snapshot/sensing、`AiDecisionKernel`、
   `AiCharacterDecisionModule`、canonical input writer与后续character input/movement/hit passes；
3. 优先复用现有Play probe/diagnostics；若不存在端到端probe，获本包批准后先建立独立test-only
   Change Record，才允许新增Editor-only探针；
4. 使用current loaded DAT/source-derived availability选择普通与OID-specific witness，不为某角色写专项分支；
5. 记录tick、AI slot/stable ID/OID、selected target、ground/air/cache/special facts、decision position、
   RNG state/calls、canonical buttons、runtime key/cd、frame/state、position/velocity、opoint、candidate、
   target HP/frame/stat与cleanup；
6. production `DataOrientedCanonical`为主证据；只允许读取现有Legacy/shadow比较结果，不持久修改profile；
7. fresh compile、AI focused matrix、full self-check、真实Play cleanup和治理校验。

### 禁止

- 不修改、运行、构建、复制或写入C++ authority；
- 不直接写AI target/decision/input/runtime/frame/RNG来制造PASS；
- 不修改DAT、技能frame、AI概率、difficulty、target距离或team；
- 不以1000 AI性能报告代替单AI/多AI行为正确性；
- 不改变30Hz、FrameInputSet、worker、SoA/ECS、容量、CentralOnly或对象池；
- 不处理T8、IL2CPP、Android、服务器、F1/F2 debug或人手硬件输入验收。

## Required fixtures

### A — ordinary target / movement

- 至少两个可攻击team的live character；
- 验证严格低slot tie、ground/air选择、cache retain/refresh与selected target identity；
- 验证AI canonical direction/action进入FrameInputSet，并产生匹配的移动、朝向与frame变化。

### B — RNG-gated decision positions

- 至少一个source-derived early position和一个late position witness；
- 记录outer gate、position number、RNG before/after/call count与input结果；
- 位置未命中时不得额外消费RNG或释放技能。

### C — authored skill / opoint

- 从loaded DAT选择当前可达的OID-specific技能，不硬编码角色名；
- 验证AI decision→canonical buttons→combo/frame→opoint/对象生命周期；
- 若当前DAT没有可达witness，标记`BLOCKED BY FIXTURE AVAILABILITY`，不得改DAT。

### D — hit / lifecycle tail

- 让AI技能真实进入collision/hit/damage；
- 记录candidate order、vrest、HP/frame/stat与AI下一tick资格；
- cleanup后world/slot/pool/RNG/profile恢复基线。

## Deliverables

1. `docs/ai/RESEARCH/R8-WP01G-R04-ai-joint-runtime-evidence-20260823.md`；
2. 结构化Play报告位于`Temp/`；
3. 若需新增probe：独立Task/Change Record/Ledger/STATE/handoff；
4. 若发现production first difference：新的最小修复Work Package，不在R04内直接修改；
5. 更新all-diff register、STATE、总计划与handoff。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | C++ source decision/RNG/input writer顺序闭合；C++目录保持只读。 |
| S1 | Unity sensing→decision→FrameInputSet→movement/skill→hit crosswalk闭合。 |
| S2 | AI相关focused矩阵全部PASS；不得用旧286结果直接替代fresh结果。 |
| S3 | fresh compile0；full `BattleRuntimeSelfCheck` PASS。 |
| S4 | A～D真实Play逐项PASS或诚实记录fixture blocker；cleanup restored。 |
| S5 | C++ full trace仍BLOCKED时明确限制，不升级为C++ runtime VERIFIED。 |

## Stop conditions

- first difference指向production AI/gameplay；
- 需要新增脚本但尚未创建Change Record；
- 需要修改DAT、profile、pass order、30Hz、worker、容量或受保护adapter；
- current loaded DAT无法提供所需decision/skill witness；
- C++规则无法由release live source闭合；
- 用户未批准本Work Package。

## Out of scope

C++ executable/full trace、>399实体的C++语义、1000 AI性能复测、人手硬件输入、render重构、T8、
IL2CPP/Player、Android、服务器、F1/F2 debug。

## Authorization

用户于2026-08-23明确决定不再执行本包：C++ AI sensing、39-position decision与RNG选择不再作为
Unity战斗对齐目标，未来改用Unity状态树或行为树。R04未运行Play、未新增probe、未修改production。

保留的唯一长期边界是：未来Unity AI仍应按固定逻辑tick产生canonical `FrameInputSet`，不得直接把
Transform、Animator或行为树节点状态写成战斗runtime真值。该接口边界不要求复刻C++ AI决策算法。
