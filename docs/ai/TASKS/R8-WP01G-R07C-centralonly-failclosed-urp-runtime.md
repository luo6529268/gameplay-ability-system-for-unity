# R8-WP01G-R07C — CentralOnly fail-closed URP ownership runtime certification

> 建立日期：2026-08-23  
> 状态：`VERIFIED / UNITY S4 CLOSED TO AVAILABLE EVIDENCE`  
> D-ID：`D-RENDER-001`

## Goal

在真实Unity URP Play中验证CentralOnly的四态pixel ownership：cold failure为空但仍由Central拥有；ready
current提交当前tick；ready之后的transient failure只保留last-good stale submission；replacement ready发布
新generation并退休旧submission。任何状态都不得启动Legacy SpriteRenderer pixel owner或产生双画。

## Scope

### 允许

1. 只读复核C++ render success path与Unity CentralOnly adapter；
2. 复用现有feature registration、central plan、submission lease、diagnostic reason与isolated pixel能力；
3. 批准后先建test-only Change Record；必要时新增Editor-only Play probe；
4. 仅通过Editor-only临时feature/material registration和已有self-check boundary形成cold/failure/replacement；
5. 在`finally`恢复原feature registration、material、draw mode、driver pause、central plan与world状态；
6. 记录requested/effective mode、owner、simulation/display tick、stale、generation、submission lease、reason、
   legacy suppression、draw/segment和isolated pixels；
7. fresh compile、focused ownership tests、full self-check、Play、Console0、ledger validator。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不修改URP renderer asset、scene、shader、material asset或production registration代码；
- 不长期禁用BattleRenderFeature；
- 不恢复Legacy owner、不允许Central+Legacy双画；
- 不把last-good stale写成current tick已显示；
- 不修改gameplay、pass order、camera、visual scale、capacity、worker或0GC边界；
- 不处理R07A/R07B、P1/P2、AI、T8、IL2CPP、Android或服务器。

## Authority / Evidence

- C++ `src/render/renderer.cpp:1300-1438` success-path handoff；
- Unity `BattleCentralRenderSystem` feature registration、Prepare/Materialize、failure plan、submission lease；
- existing `CheckBattleCentralMeshAndUrpContracts`；
- existing `CheckCentralPixelOwnershipContracts` cold→ready→last-good→replacement exact matrix；
- WP01D-06 Game current submission和WP01D-07 SceneView isolated pixels。

## Acceptance

1. cold：Central owner、submission null、displayTick -1、stale true、reason明确、legacy suppressed、0 central pixel；
2. current：Central owner、submission current、simulation/display tick相同、stale false、isolated pixels>0；
3. transient failure：simulation tick前进、display tick保持last-good、stale true、旧submission仍可租用、
   reason明确、legacy仍suppressed、像素等于last-good；
4. replacement：新generation/current display tick/isolated pixels，旧submission正确retire且lease无泄漏；
5. 四态均不启用Legacy materializer或每实体SpriteRenderer；
6. failure/restore不改变battle checksum、world entity、slot、RNG或input；
7. probe异常时`finally`也恢复真实feature/material/draw mode和运行状态；
8. fresh compile/focused/Play/self-check/Console0/ledger全部PASS。

## Files likely involved

- existing central ownership self-check/tests；
- 如确有必要：一个Editor-only fail-closed Play probe及`.meta`；
- Task/Research/Change Record/Ledger/STATE/register/main plan/handoff；
- production renderer、scene和URP asset默认不应修改。

## Unknowns

- live URP feature每camera回调可能立即重新注册，transient failure必须使用现有Editor-only central failure
  boundary而不是改renderer asset；
- cold Play若无法在不Reset真实全局central state的情况下安全形成，只能保留exact self-check并在报告中明确
  cold Play未运行，不能通过破坏当前scene registration强行获取；
- last-good isolated pixel比较必须持有合法submission lease，不能读取retired backend。

## Verification

1. feature registration/restore source audit；
2. exact cold/current/stale/replacement ownership self-check；
3. real Game camera current→failure→replacement isolated pixel Play；
4. lease/generation/legacy suppression/checksum/cleanup；
5. full self-check与ledger validator。

## Stop conditions

- 需要修改URP/scene/material asset或production registration；
- 无法在finally可靠恢复feature/global central state；
- live camera自动重注册使故障状态不可重复；
- 发现production first difference；
- 需要恢复Legacy或改变保护边界；
- 只剩C++ full trace且R1-WP02仍BLOCKED。

## Out of scope

R07A、R07B、gameplay、P1/P2、AI、T8、IL2CPP、Android、服务器、C++ executable/full trace。

## Authorization

用户已于2026-08-23明确批准执行`R8-WP01G-R07C`并恢复总目标。已在脚本写入前建立
`R8-CENTRALOWN-001 / IN_PROGRESS / TEST-ONLY`；实施只允许覆盖本合同，不授权R08或任何production修复。

## Execution result

- current/stale/replacement真实URP Play通过；cold exact self-check通过、cold Play未运行；
- final Play存在`BeginBattleAllocationSeal→PrepareBattleCapacity` active submission resize exception；
- 触发本合同production first-difference stop condition；R07C保持BLOCKED；
- 独立repair：`R8-WP01G-R07C-R01-central-capacity-seal-active-submission-repair.md / APPROVAL PENDING`；
- 证据：`RESEARCH/R8-WP01G-R07C-centralonly-failclosed-urp-runtime-evidence-20260823.md`。

## Repair closure（2026-08-23）

`R8-WP01G-R07C-R01 / R8-CENTRALSEAL-001`已获批、实施并验证。最终方案不关闭Camera；首次battle
allocation seal清退旧central publication后再预热容量，重复seal严格no-op。normal Play Camera enabled/Console0；
R07C三态、cold self-check和Combat1000 0GC均PASS。`B-R8-R07C-01`已关闭，R07C按现有证据收口；C++ full
trace blocker保持独立，不被Unity S4结论覆盖。
