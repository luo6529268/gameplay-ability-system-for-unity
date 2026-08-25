# R8-WP01G-R01B — D-STEP-001 debug-unlock source/policy closure

> 日期：2026-08-23  
> 状态：`COMPLETE / READ-ONLY / NO CODE CHANGE`  
> D-ID：`D-STEP-001`

## Goal

只读闭合C++ Release `g_dword_449048` / `g_dword_44FD3C` 的writer、lifetime、physical edge与
`game_tick(...)` consumer，判断Unity固定0是否为等价、批准省略或真实未修复差异，并明确它与
`D-SCHED-008` candidate-tail修复的依赖。

## Scope

- 只读C++ Makefile、`main.cpp`、`entity_collision.cpp`、`game_tick.cpp`；
- 只读Unity Flow state、scheduler、snapshot/checksum/restore、input边界；
- 更新差异登记、STATE、总计划、handoff与next policy Task；
- 不修改任何Unity/C++脚本、输入资产、scene或config。

## Authority / Evidence

- 唯一权威：`J:\QQFile\NTSD2.4\ntsd_release` release live source；
- C++只读，不运行、构建、复制、修改或写入；
- Unity现状只用于crosswalk，旧C# parity不能裁决行为；
- R1-WP02 full trace继续BLOCKED。

## Deliverables

1. `RESEARCH/R8-WP01G-R01B-d-step-debug-unlock-source-policy-20260823.md`；
2. `D-STEP-001`明确status；
3. 与`R2-CANDIDATE-TAIL-01`的tail-skip predicate依赖；
4. 后续`R3-STEP-01` policy Task建议；
5. STATE/plan/handoff更新。

## Verification

- Makefile参与性与全部production read/write调用点闭合；
- physical edge、sequence state、flag lifetime和game_tick branch闭合；
- Unity field/schema/producer/consumer inventory闭合；
- docs diff-check与Change Ledger validator通过；
- gameplay/test脚本新增diff为0。

## Stop conditions

- 需要实现raw keyboard、扩展FrameInputSet/lockstep schema或修改scheduler；
- 需要决定是否移植debug功能；
- 需要运行/修改C++或进入R3代码；
- first difference指向candidate-tail以外的R3+模块。

## Out of scope

`R2-CANDIDATE-TAIL-01`脚本实现；physical F1/F2/A/B/C；pause overlay；R3+代码；T8；IL2CPP；Android；服务器。
