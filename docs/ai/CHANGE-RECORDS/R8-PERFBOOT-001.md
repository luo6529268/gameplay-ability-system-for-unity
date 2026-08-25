# R8-PERFBOOT-001 — stress initial-service / runtime-invalidation discriminator

<!-- CHANGE-RECORD
id: R8-PERFBOOT-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressWindow.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/Editor/ProductionEntityStressEditorTests.cs
authority: USER approved Unity on-demand service boundary; R8-WP01E current-build certification first failure; no C++ gameplay change
evidence: E-01 compile0/focused290-of-290/self-check PASS; E-02 terminal result at 2026-08-23 14:26:30 before any sampled tick
-->

> 创建日期：2026-08-23  
> 状态：`VERIFIED`  
> 所属：`R8-WP01E-R01`

## 1. 修改前状态

- E-01 fresh compile0、focused 290/290、14:25:44 full self-check PASS；
- E-02 Combat1000 request未生成report，在两次Play initial service窗口均观察到driver/world存在、pool不存在；
- processor把`managedRuntime.HasServiceFootprint`传作`managedRuntimeExpected`，导致初始partial footprint立即
  restart；
- Bootstrap ready前`AreProductionServicesReady()`不会触发lazy pool创建，这是避免早期副作用的现有设计；
- C++ gameplay、Unity pool/runtime/capacity均没有被本失败执行到，不能据此判断性能。

## 2. 允许改动

- 只允许修正`PollRequest`的restart decision事实输入；
- 只允许在existing Editor test class补pure-policy matrix；
- 初始未ready等待、已健康后失效restart、ready-but-invalid restart和retry limit必须同时保留；
- 不新增场景manager，不修改pool或Bootstrap。

## 3. 保护与副作用

- 不改变生产战斗tick、对象生成、容量、RNG、渲染或网络；
- 不让认证工具隐式修复缺失production service；它只等待声明的ready边界；
- 任何后续service缺失必须形成新的first failure，不能在本包扩大修复。

## 4. 验收与回滚

- 验收按Task source→compile→focused→self-check→同请求Play复跑；
- 只有同一请求真正进入采样，才能把本Record升级到`VERIFIED`；
- 回滚仅撤销两个批准文件的本Change diff并更新状态；首次失败证据永久保留。

## 5. 实际改动

用户已于2026-08-23明确批准`R8-WP01E-R01 / R8-PERFBOOT-001`并恢复目标。

- `EvaluatePlayRestartDecision`的第四项语义由partial service footprint改为Bootstrap ready事实；
- 初次Play或合法clean restart后的新Play，在Bootstrap尚未ready时统一等待initial services；
- Bootstrap ready仍无效或先前healthy后失效时继续原有restart/retry-limit；
- `PollRequest`不再用driver/world partial footprint判断service应已完整，改传
  `BattleTestBootstrap.ProductionStressServicesReady`；
- 新增7分支pure-policy matrix，覆盖initial count0/count1 wait、ready-invalid restart、previously-healthy
  restart、transition wait、count1 fail-closed和valid record。

代码写入后fresh Editor DLL为14:33:57，Console C# error0；focused job
`2bcc822ceddb45f9955a3041a3ade51f`为263/263 PASS，包含新增policy matrix与capacity tests；14:35:20
full self-check PASS。当前`FOCUSED_TEST_PASS`，等待同一Combat1000请求Play复跑。

## 6. Runtime verification

完全相同的Combat1000 data-oriented capacity-pressure smoke已复跑并`PASS`：1000 active、30 warmup、
180 sampled ticks、logic tick 0 B/0 collection、capacity critical0、central 1 draw、teardown完整恢复；首败的
premature restart未再出现。logic Avg/P95=`21.199/23.797 ms`。证据：
`RESEARCH/R8-WP01E-R01-production-service-recovery-evidence-20260823.md`。

Record升级`VERIFIED`，只裁决stress initial-service/restart discriminator。visible Unity frame Avg/P95仍为
`38.949/39.025 ms`，正式60秒30 FPS与Editor frame GC仍归WP01E，不被本Record关闭。
