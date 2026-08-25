# HANDOFF — R8-WP01G-R07C-R01 active central submission / capacity seal repair

> 日期：2026-08-23  
> 状态：`VERIFIED / COMPLETED / PRODUCTION REPAIR`

## Blocker

`B-R8-R07C-01`：正常Play异步加载期间central submission已经published，之后
`BeginBattleAllocationSeal→PrepareBattleCapacity→Submission.PrepareCapacity`拒绝resize并抛异常。

## What already passed

- R07C current/stale/replacement真实URP Play像素/owner/tick/gen/lease/cleanup/checksum全部PASS；
- cold exact self-check PASS，cold Play未运行；
- compile0、focused29/29、full self-check PASS、ledger84/99 PASS；
- production renderer/gameplay/URP asset 0改动。

## Next action after approval

先只读闭合Bootstrap/AppManager两个caller和capacity high-water，再建立独立production Change Record；不得直接
catch、skip或ResetRuntime。修复后必须重跑normal Play Console0、R07C和1000/0GC容量门。

## Resume phrase

`批准执行 R8-WP01G-R07C-R01，恢复目标`

## Execution start

- 用户已明确批准R07C-R01；
- caller审计确认BattleTestBootstrap与AppManager互斥，不是重复初始化；
- first difference是camera可在loading期间publication，而capacity seal在装配后才执行；
- 已在脚本写入前建立`R8-CENTRALSEAL-001 / IN_PROGRESS`。

## Final handoff

- 最终方案：Camera/Canvas不禁用；首次seal在presentation capacity prepare前调用既有central runtime
  retirement/reset边界；重复seal严格no-op；
- 第一版Awake-disable被用户实测指出副作用后已在同一Change内修正，`BattleBootstrap.cs`最终无净diff；
- compile0、focused20/20、23:13:13 self-check、normal Play Camera enabled/Console0、R07C三态与
  Combat1000 0GC/cleanup均PASS；
- `B-R8-R07C-01`关闭；`R8-CENTRALSEAL-001`与`R8-CENTRALOWN-001`均推进为VERIFIED；
- Change Ledger validator：85 records / 103 governed code files，PASS；
- 不含R08、AI、T8、IL2CPP、Android、服务器或C++改动。
