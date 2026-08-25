# R8-WP01G-R07C-R01 — active central submission / battle capacity seal repair

> 建立日期：2026-08-23  
> 状态：`VERIFIED / COMPLETED / PRODUCTION REPAIR`  
> Blocker：`B-R8-R07C-01`

## Goal

修复Unity-native战斗初始化顺序：异步加载期间CentralOnly已发布submission后，
`BeginBattleAllocationSeal`不得再对published/leased submission执行不安全resize；同时保持预战容量预热、sealed
0GC、CentralOnly ownership和现有战斗逻辑不变，使正常`NTSD_Battle` Play达到Console0。

## Scope

### 批准后允许

1. 只读复核`BattleTestBootstrap`与`AppManager`两条正式初始化入口；
2. 复核`BeginBattleAllocationSeal`、`PreparePresentationHotPathCapacity`、`PrepareBattleCapacity`的调用时点和幂等合同；
3. 先建立独立production Change Record；
4. 选择最小统一修复，必须同时覆盖测试Bootstrap与正式AppManager入口；
5. 增加聚焦初始化顺序/active submission容量测试；
6. fresh compile、focused、self-check、正常Play Console0、R07C probe、1000容量/0GC非回退和ledger验证。

### 禁止

- 不修改C++ authority；
- 不吞掉异常、不catch后继续、不临时ResetRuntime掩盖production顺序；
- 不退休正在使用的submission来强行resize；
- 不取消预战容量预热、sealed capacity或0GC门；
- 不回退Legacy、不改URP asset/scene/material/DAT/gameplay/pass order；
- 不处理R08、AI、T8、IL2CPP、Android或服务器。

## Authority / Evidence

- Unity-native adapter边界；C++ renderer success path不定义Unity容量预热实现；
- final R07C Play first difference及完整调用栈；
- `BattleTestBootstrap.Start:127-163`、`AppManager.InitializeBattleAsync:110-130`；
- `SimulationTickDriver.BeginBattleAllocationSeal:948-1000,1125-1146`；
- `BattleCentralRenderSystem.PrepareBattleCapacity:137-171`；
- `BattleCentralSubmission.PrepareCapacity:102-116`。

## Unknowns

- 最小正确修复应前移presentation capacity prepare、拆分prepare/seal，还是让已足够容量的active submission走
  no-op，需要source/capacity high-water审计后决定；
- Bootstrap与AppManager是否会在同一场景重复初始化，必须先确认caller identity；
- active submission的已有容量是否足以支持最终runtime profile，不能在无诊断数据时直接skip。

## Deliverables

1. source/caller/capacity contract；
2. 独立production Change Record；
3. 最小实现与聚焦测试；
4. normal Play Console0 + R07C四态复验；
5. 1000容量/0GC非回退；
6. evidence、STATE、diff register、main plan和handoff更新。

## Verification

- compile0；
- initialization/capacity focused tests；
- full self-check；
- fresh normal `NTSD_Battle` Play无capacity exception；
- R07C current/stale/replacement证据不退回；
- 1000 profile容量和0GC门不退回；
- validator PASS。

## Stop conditions

- 需要改变长期架构、容量产品合同或CentralOnly owner；
- 无法同时覆盖Bootstrap与AppManager；
- 修复会在battle window内新增分配；
- first difference转移到gameplay/pass order；
- 用户改变范围。

## Out of scope

R08、gameplay parity、AI、T8、IL2CPP、Android、服务器、C++ executable/full trace。

## Authorization

用户已于2026-08-23明确批准执行`R8-WP01G-R07C-R01`并恢复总目标。已在脚本写入前建立
`R8-CENTRALSEAL-001 / IN_PROGRESS`；批准只覆盖本合同，不授权R08或其他production重构。

## Completion evidence（2026-08-23）

- 最终实现保持World/UI Camera与Canvas启用；第一版Awake-disable已在同一Change内废弃；
- 首次`BeginBattleAllocationSeal`在presentation capacity prepare前清退旧central publication；双seal完成后的
  重复调用严格no-op；
- Unity fresh compile 0 error；focused job `4cd77be4f1664b329a1e6f3b8167cfc9`为20/20 PASS；
- `BattleRuntimeSelfCheck`于23:13:13 PASS；
- 普通`NTSD_Battle` Play中`ScenesCamera.enabled=true`、Console0、capacity exception 0；
- R07C current/stale/replacement三态4/4/1/1、259px、hash `AE3AFF1E932B491E`，generation216→217，
  checksum与cleanup PASS，Console0；
- Combat1000为30 warmup+180 sample、1000 entities/slots、0 B/tick、0 collection、cleanup restored；
- `B-R8-R07C-01`已关闭。本合同不授权也未启动R08。
