# R8-WP01G-R07C — CentralOnly fail-closed URP runtime evidence

> 日期：2026-08-23  
> 状态：`CURRENT/STALE/REPLACEMENT PLAY PASS / COLD EXACT SELF-CHECK PASS / PACKAGE BLOCKED`

## 1. Evidence scope

- C++ authority只读：`src/render/renderer.cpp:1300-1438` success-path handoff；
- Unity production renderer/gameplay/URP asset/scene/material均0改动；
- 唯一代码改动是Editor-only `BattleCentralFailClosedOwnershipPlayModeProbeEditor`；
- cold Play未通过破坏live global state强行形成；cold只由exact self-check覆盖；
- current→last-good stale→replacement在真实Play、正式world、正式feature/material和Game camera URP中执行。

## 2. Final structured Play result

报告：`Temp/NTSD_R8_WP01G_R07C_CentralFailClosedOwnership.result.json`

| 状态 | sim/display | generation | commands/resolved/segments/draw | isolated pixels | hash | owner / legacy |
|---|---|---:|---|---:|---|---|
| current | 211/211 | 212 | 4/4/1/1 | 259 | `AE3AFF1E932B491E` | Central / suppressed |
| stale | 212/211 | 212 | 4/4/1/1 | 259 | `AE3AFF1E932B491E` | Central / suppressed |
| replacement | 212/212 | 213 | 4/4/1/1 | 259 | `AE3AFF1E932B491E` | Central / suppressed |

另外：

- 三态submission lease均accepted；
- stale保留current submission/display tick/hash；
- replacement发布新generation，旧submission retired、拒绝新lease，原持有lease释放后count=0；
- checksum `7C369C0D79EF47BA`前后相同；
- ObjectCount `4→4`、claimed slots `2→2`；
- feature instance、texture/array material、OrderedChunks draw mode全部恢复；
- cleanup=true。

## 3. Blocker / production first difference

同一最终Play的Console存在：

`InvalidOperationException: Cannot resize a central submission while it is published or leased.`

调用链：

1. `BattleTestBootstrap.Start:161`；
2. `SimulationTickDriver.BeginBattleAllocationSeal:991`；
3. `PreparePresentationHotPathCapacity:1142`；
4. `BattleCentralRenderSystem.PrepareBattleCapacity:155`；
5. `BattleCentralSubmission.PrepareCapacity:110`。

已观察事实：异步角色/资源加载期间CentralOnly feature已发布submission；随后Bootstrap才调用allocation seal，
capacity prepare要求所有submission reusable而失败。test-only prepare-play reset无法覆盖整个异步加载窗口。

因此：

- 四态renderer ownership链本身通过；
- R07C不能满足“真实Play Console0”验收；
- `D-RENDER-001`不得整体关闭；
- 需要独立repair `R8-WP01G-R07C-R01`，未批准前不改production初始化。

## 4. Fresh validation

- full asset refresh / compile：0 error；
- focused：`BattlePresentationBeginFrameReuseEditorTests + BattleSimulationWorkerBoundaryEditorTests`，job
  `ba8dce7673b2415c8a3278657c2aa451`，29/29 PASS；
- full `BattleRuntimeSelfCheck`：2026-08-23 22:45:37 PASS；
- self-check包含预期负向错误日志；清空后Console error 0，但不能替代上述Play Console first difference；
- Change Ledger validator：84 records / 99 governed code files PASS；
- R1-WP02 C++ full trace仍BLOCKED。

## 5. Classification

- `D-RENDER-001 current/stale/replacement = UNITY JOINT S4 PASS`；
- `D-RENDER-001 cold = EXACT SELF-CHECK PASS / PLAY NOT RUN`；
- `R8-WP01G-R07C = BLOCKED BY B-R8-R07C-01`；

## 2026-08-23 repair后最终证据（取代上方当前状态，不删除历史）

- repair：`R8-WP01G-R07C-R01 / R8-CENTRALSEAL-001`；
- first difference修复：首次allocation seal在central presentation capacity prepare前清退旧publication；
  双seal完成后的重复调用严格no-op；Camera/Canvas保持启用；
- normal Play：`ScenesCamera.enabled=true`，运行20秒Console0，原resize exception未复发；
- final R07C报告时间23:12:30：current `214/214/gen216`、stale `215/214/gen216`、replacement
  `215/215/gen217`；每态source/resolved/segment/draw=`4/4/1/1`，259 pixels，hash
  `AE3AFF1E932B491E`；checksum与cleanup均PASS；Console0；
- cold仍以`BattleRuntimeSelfCheck.CheckCentralPixelOwnershipContracts`为exact证据；full self-check 23:13:13 PASS；
- Combat1000：30 warmup+180 sampled ticks、1000 entities/slots、logic Avg/P95/Max
  `19.121/21.687/23.805ms`、0 B/tick、Gen0/1/2 collection=0、cleanup restored；
- 最终结论：`B-R8-R07C-01 CLOSED`；`R8-WP01G-R07C VERIFIED TO AVAILABLE UNITY S4 EVIDENCE`。
  C++ full trace仍BLOCKED，未被本证据升级。
- 不宣称C++ runtime full-trace或完整战斗对齐。
