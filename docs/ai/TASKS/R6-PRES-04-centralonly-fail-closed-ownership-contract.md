# R6-PRES-04 — CentralOnly fail-closed ownership 合同

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（no-code adapter certification）  
> 对应：`D-RENDER-001`

## Goal

确认CentralOnly success/failure ownership不会改变C++ battle truth，并判断D-RENDER-001是需修复差异
还是必要Unity adapter。

## Scope

- 只读C++ `renderer.cpp` readiness/success path；
- 只读Unity `BattleCentralRenderSystem`、pixel plan/submission、resource diagnostic与existing tests；
- 运行现有自动证据（Editor可用时）；
- 不修改任何production/test脚本、renderer、shader、scene或resource。

## Authority / Evidence

- C++ authority只定义成功battle render handoff和自身surface/sprite readiness；
- CentralOnly/Texture2DArray/dynamic Mesh/URP及禁止Legacy production fallback是用户批准保护边界；
- Unity self-check只证明状态机/诊断合同，不替代真实URP PlayMode或C++ trace。

## Deliverables

1. `RESEARCH/R6-PRES-04-centralonly-fail-closed-ownership-preflight-20260822.md`；
2. 更新总差异登记、STATE、主计划；
3. `HANDOFFS/HANDOFF-R6-PRES-04-centralonly-fail-closed-ownership.md`。

## Verification

- source state table闭合；
- fresh full self-check中的P4/P8/central pixel ownership sections PASS；
- 额外focused job若Editor不可用，明确记录未运行；
- PlayMode/C++ trace未取得时最高`RUNTIME_PENDING`。

## Stop conditions

- 需要改变pixel owner、允许partial frame、恢复Legacy或改renderer/resource architecture；
- 正常ready route出现current snapshot提交差异；
- 用户要求改变fail-closed产品策略。

## Out of scope

D-RENDER-002、camera视觉策略、GPU/Android、performance、T8、C++ executable、任何脚本修改。

