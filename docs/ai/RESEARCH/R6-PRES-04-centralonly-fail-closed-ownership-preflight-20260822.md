# R6-PRES-04 — CentralOnly fail-closed ownership 认证

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（no-code adapter certification）  
> 对应：`D-RENDER-001`  
> 脚本改动：无；不创建 Change ID

## 1. C++ success-path authority

- release `renderer.cpp:1300-1438` 从active entity建立当前tick painter stream；
- `draw_shadow`要求shadow surface/current DAT/frame等资源，`draw_entity`要求current DAT/frame/pic与
  sprite sheet/range；资源不存在时相应blit不会发生；
- C++ source没有Unity URP feature、ScriptableRenderer、world Camera、Material或dynamic Mesh backend，
  因而这些Unity基础设施的“缺失状态”不是可机械移植的battle rule。

可对齐合同是：当双方运行前置都满足时，Unity必须提交当前snapshot的同一战斗descriptor/order；
不能把一个无有效Unity renderer的状态当作C++ gameplay行为，再通过恢复Legacy双画来伪造成功。

## 2. Unity ownership state machine

`BattleCentralRenderSystem.PrepareFrameImmediate` 的production状态为：

| 状态 | Unity行为 | 合同判定 |
|---|---|---|
| `LegacyOnly` | 明确发布Legacy owner，不构建central geometry | 诊断/兼容mode；不是CentralOnly生产fallback |
| CentralOnly cold failure | owner仍为Central、submission=null、displayTick=-1、stale=true、Legacy materializer继续抑制 | fail-closed，避免中央/Legacy双owner |
| CentralOnly ready | feature/material/world URP camera/frame/common catalog/backend与全command resource均ready；发布当前tick central submission | C++ success-path handoff对应状态 |
| CentralOnly transient failure after ready | 保留上一份仍可获取的central submission，simulationTick前进、displayTick保持last-good、stale=true并记录reason | Unity-native可靠性adapter；不得当成current tick已显示 |
| replacement ready | 发布新generation/current displayTick，retire上一submission | 恢复到success path |

`TryValidateActiveRenderer`显式验证feature、declared material、active URP world camera及近期feature observation；
frame/common catalog/backend或command resource不完整也会拒绝current generation。`CommitCentralFailurePlan`
不会恢复Legacy pixels。

## 3. 为什么不改代码

1. 用户已批准CentralOnly为production pixel owner，Legacy只保留兼容/诊断；
2. 缺feature/camera/material时不存在可提交的Unity central draw，任何“继续画当前帧”的修复都必须另建
   renderer/resource，并非C++ gameplay移植；
3. 自动fallback到Legacy会重新引入双画、8192 SpriteRenderer capacity依赖和已明确禁止的架构回退；
4. 当前plan把 `simulationTick` 与 `displayTick`、`IsStale`、reason分开，未把stale pixels冒充当前帧；
5. current command中单一resource unresolved会令完整ownership拒绝，并保留明确first unresolved diagnostic，
   避免半帧中央输出与其它owner混合。是否允许“部分command继续画”属于新的长期pixel-ownership设计，
   不能在对齐包中擅自改变。

所以D-RENDER-001的静态结构差异被认证为A-RENDER-001下必要Unity adapter，而非应删除的gameplay
差异。成功路径的descriptor/order仍由R6其它包逐项验收。

## 4. Existing automated evidence

fresh `BattleRuntimeSelfCheck`（DLL `18:33:37.270`，result `18:35:48.011 PASS`）明确调用并通过：

- `CheckBattleCentralMeshAndUrpContracts`：missing feature、Legacy diagnostic mode、CentralOnly cold
  failure/no Legacy materialization；
- `CheckBattleCentralEntityDiagnosticContracts`：missing key、invalid binding、missing texture/material、
  unresolved resource、unsupported state、stale report；
- `CheckCentralPixelOwnershipContracts`：cold → ready → leased double buffer → last-good stale → replacement
  ready、Legacy suppression及warmed zero allocation。

19:09尝试额外运行
`BattleCentralLatestFrameMaterializationEditorTests`与`BattleCentralDiagnosticEditorTests`，但Unity Editor
已关闭、MCP发现0 instance，job未创建；这不是test failure，也不作为通过证据。

## 5. Evidence boundary / reopen conditions

状态最高为`RUNTIME_PENDING`，因为仍缺真实URP PlayMode下：

- active world camera/feature observation；
- cold/ready/stale/recovery的实际Game/Scene像素；
- C++ runtime trace/GPU像素（R1-WP02仍BLOCKED）。

若正常已预加载战斗中出现持续stale、无reason的空像素、current generation却display旧tick、或任何
Legacy production materialization，必须重开D-RENDER-001并以首个reason定位；不得直接回退架构。

