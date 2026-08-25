# R8-WP01E — current-build 1000 active capacity / zero-GC / 30 Hz certification

> 日期：2026-08-23  
> 状态：`VERIFIED / UNITY EDITOR CURRENT BUILD`

## Goal

在当前工作树、国际版 Unity 2022.3.62f3、production battle 配置和现有中央表现路径上，重新取得
`MobileExtended` 1000 active 与 `DesktopExtended` 容量合同的当前构建证据；历史 U9/P0～P6 报告只作为
对照，不直接晋升当前状态。

本包只认证，不在失败后顺手修改 gameplay、AI、碰撞、对象池、中央渲染、调度或容量策略。首个失败必须
保存报告并转交独立修复 Work Package；任何脚本修改前另建 Task Contract、Change Record 和 Ledger 条目。

## Authority / Evidence

- 行为唯一权威：只读 `J:\QQFile\NTSD2.4\ntsd_release` release live source；本包不运行、构建、修改或
  向 authority 目录写入；
- Unity 交付边界：`MobileExtended = 1050 slot / 1000 active`；`DesktopExtended` 无固定产品 active cap，
  但必须在 unsealed loading/reset/preflight 安排有限页预算，active battle seal 后 strict 0 B，超预算确定性拒绝；
- 当前性能计划：`Assets/NTSD/Docs/singleplayer-1000ai-performance-plan.md`；
- 当前压力工具：`ProductionEntityStressHarness`、`ProductionEntityStressWindow`、capacity focused tests；
- 30 Hz 预算按单逻辑 tick `33.333 ms`；不能用多 tick/frame 追帧、关闭正式表现、simulation-only 或 Deep
  Profile 的结果代替最终门禁。

## Required matrix

### E-01 — fresh tool / capacity baseline

1. Unity fresh scripts compile 0 error；
2. production stress、capacity、runtime profile、central diagnostics 的 focused EditMode tests PASS；
3. full `BattleRuntimeSelfCheck` PASS；
4. 验证当前请求固定 `maxCatchUpTicksPerFrame=1`、`requireZeroGcAfterWarmup=true`、正式表现启用、
   `MobileExtended` 1000 active 和 Loose Quadtree 压力配置实际生效。

### E-02 — short current-build validity gate

先运行短样本，不复用历史报告：

- Dispersed1000：30 warmup + 180 sampled ticks；
- Combat1000：30 warmup + 180 sampled ticks，`data-oriented-canonical`，正式中央表现启用；
- 必须生成 1000 个真实 production GameObject，而不是纯数据行；
- `harnessValidity=true`、sampled tick 数完整、0 B/0 collection/0 capacity fault、central draw/pixel > 0、
  final hash 非空、teardown restored；
- 任一 validity gate 失败即停止正式长跑并登记 first failure。

短样本只证明工具和当前构建可运行，不关闭 30 Hz 正式门禁。

### E-03 — formal 60-second gates

对 Dispersed1000 与 Combat1000 分别执行：

- 120 warmup ticks；
- 1800 sampled logic ticks，即 30 Hz 下 60 秒；
- 每个 Unity frame 最多 1 个完整逻辑 tick；
- `logicTickMilliseconds.average <= 33.333 ms`；
- `logicTickMilliseconds.p95 <= 33.333 ms`；
- 稳态逻辑 tick `0 B`，Gen0/1/2 collection 均为 0；
- capacity critical delta、fault、admission failure 均为 0；
- 1000 active GameObject、world entity、claimed runtime slot 与 pool active 数一致；
- 正式中央表现 source/resolved command、segment、draw 和 submitted pixels 均有有效证据；
- teardown 后 active object/world/slot/pool、driver、logging、RNG 和测试覆盖状态恢复；
- 报告保存 current source identity、Unity version、runtime profile、AI profile、broadphase、seed、final hash。

`Concentrated1000` 只输出极限复杂度报告，不承诺 30 Hz；它不替代也不阻断上述两个正式门。

### E-04 — DesktopExtended capacity contract

- 复跑 DesktopExtended preflight reservation、paged slot、minimum-hole reuse、generation invalidation、sealed
  rejection 和 warmed 0 B focused tests；
- 不把默认初始 512 当作产品 active hard cap；
- 不在 active battle tick 内动态分配页；
- 不为通过测试改变现有容量合同。

## Acceptance

只有 E-01～E-04 全部通过，才可将 WP01E 写为 `VERIFIED`。其中：

- `0 B` 只裁决已测稳态窗口；
- 30 Hz 必须同时满足 Avg 与 P95；
- Editor 当前构建结果不代替 Android 真机，也不代替 C++ runtime full trace；
- 性能通过不证明 gameplay 与 C++ 完整对齐。

## Stop conditions

- fresh compile、focused test 或 self-check 失败；
- short gate 未生成 1000 production GameObject、中央 draw/pixel 无效或 teardown 未恢复；
- 出现任意 steady-tick allocation、GC collection、capacity fault 或 deterministic hash/ownership failure；
- 需要修改 gameplay、AI、collision、pool、worker、render、scene、runtime profile 或长期架构；
- 需要运行、构建、修改或写入 C++ authority；
- Unity Editor/MCP 连接或当前场景前置无法安全恢复。

## Deliverables

1. 当前构建短样本与正式 JSON 报告；
2. `docs/ai/RESEARCH/R8-WP01E-current-build-capacity-performance-evidence-20260823.md`；
3. 更新 WP01 orchestration、STATE、总计划、差异登记和 handoff；
4. 若失败，建立独立修复包合同，保留 first failure，不在本认证包内改代码。

## Out of scope

C++ executable/full trace、Android 真机、T8 默认 stage.dat、Windows Player build、服务器、规则降频、候选/命中
上限、关闭正式表现、simulation-only 代替正式验收，以及任何为达到数字而改变 C++ observable behavior 的优化。

## Current result — 2026-08-23

- E-01：fresh compile 0 error；focused job `2dda595036944c708bfd11f32204ba1e`为290/290 PASS；
  14:25:44 full self-check PASS；
- E-02首次Combat1000短样本：在0 sampled tick、0 stress entity、无report时被initial-service/restart错误阻断；
- terminal state显示driver/world已存在但pool尚未由lazy singleton materialize，processor连续两次过早判定
  managed runtime invalid；
- 分类为harness lifecycle first failure，不是性能失败；证据见
  `RESEARCH/R8-WP01E-first-validity-failure-20260823.md`；
- 已建立`R8-WP01E-R01 / R8-PERFBOOT-001 / PLANNED / APPROVAL PENDING`。认证停止，不在本Task内改脚本。

批准后的`R8-PERFBOOT-001`现已`VERIFIED`。完全相同Combat短样本现为1000 active、180/180 sampled、
logic Avg/P95=`21.199/23.797 ms`、logic 0 B、Gen collection0、capacity critical0、central 1 draw、teardown
restored。E-02 Combat validity gate通过；visible frame Avg/P95=`38.949/39.025 ms`与frame GC平均7128.94 B
仅记录为正式门风险，不能由短样本关闭。下一步是Dispersed1000 current-build短样本。

Dispersed1000短样本亦通过validity：logic Avg/P95/Max=`21.432/24.771/32.177ms`、0 B/0 collection、
capacity critical0、central 1 draw/SetPass4、teardown restored。其visible frame Avg/P95=`38.309/44.265ms`。
E-02短矩阵完成；E-03必须运行两组各120 warmup+1800 sampled并启用completed-frame timing，当前不得宣称
30 FPS完成。证据见`RESEARCH/R8-WP01E-current-build-capacity-performance-evidence-20260823.md`。

E-03正式结果：Dispersed logic/visible/main P95=`18.575/25.525/25.286ms`，Combat=`19.044/33.058/
26.901ms`；两组1800/1800、logic0B/0 collection、capacity critical0、central1 draw、teardown PASS。
E-04 focused 299/299；Legacy/DataOriented同180-tick的12项状态/workload hash全部一致；14:51:50 final
self-check PASS。WP01E在Unity Editor current-build范围`VERIFIED`，Windows Player/Android/C++ trace均不在此结论。
