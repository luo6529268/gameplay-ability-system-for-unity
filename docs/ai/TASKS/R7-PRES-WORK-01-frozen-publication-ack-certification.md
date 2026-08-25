# R7-PRES-WORK-01 — frozen presentation / worker publication-ack certification

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING / NO-PRODUCTION-CODE`

## Goal

重新认证cached/frozen presentation、CentralOnly latest-frame materialization和dedicated worker
publication/ack，不让性能适配改变C++ RenderDispatch观察时点或逻辑结果。

## Authority / Evidence

- C++ `game_tick.cpp:945-948,2023-2087`；
- C++ `renderer.cpp:1300-1438`；
- Unity `NTSDBattleTickSystem.RenderDispatch`、`BattlePresentationCoordinator`、
  `BattleCentralRenderSystem`、`BattleSimulationWorkerBoundary`、`SimulationTickDriver`；
- focused jobs 13/13、11/11、6/6、16/16 PASS；
- `RESEARCH/R7-PRES-WORK-01-frozen-publication-ack-recertification-20260822.md`。

## Result

- 当前production `CentralOnly + dedicated worker`接线已确认；
- 冻结观察点、latest/world/generation gate与worker single-flight acknowledgement在source和分段自动测试中闭合；
- 未发现新的production behavior difference；
- `D-TEST-002`：human current-key旧测试断言与C++冲突；
- `D-TEST-003`：缺正式driver buildPresentation=true→central materialize→ack→next tick联合夹具；
- `D-PERF-003`：production当前是一帧一tick/single-flight边界，未来pipeline前必须重审。
- fresh-domain Unity Console编译错误为0，2026-08-22 22:32:29 full `BattleRuntimeSelfCheck=PASS`。

## Verification still required

- 修正`D-TEST-002`的独立test-only WP；
- 新增`D-TEST-003` joint fixture的独立test-only WP；
- R8真实URP Play Mode与C++ runtime evidence（若可用）。

## Out of scope / stop

不修改production脚本、C++、渲染架构、worker gate、pass ordering、input、collision、opoint或容量策略。
