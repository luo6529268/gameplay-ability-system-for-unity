# R8-WP01E current-build capacity / performance evidence — 2026-08-23

> 状态：`VERIFIED / UNITY EDITOR CURRENT BUILD`

## E-01 baseline

- Unity 2022.3.62f3；fresh scripts compile0；
- focused job `2dda595036944c708bfd11f32204ba1e`：290/290 PASS；
- 14:25:44 full self-check PASS；
- initial-service harness first failure由独立`R8-PERFBOOT-001 / VERIFIED`关闭。

## E-02 short current-build matrix

共同配置：1000 production GameObject、MobileExtended、DataOrientedCanonical、LooseQuadtree、正式
CentralOnly表现、30 warmup + 180 sampled ticks、每帧最多1 tick、zero-GC/capacity gate、seed 1314149188。

| Workload | Logic Avg/P95/Max ms | Visible frame Avg/P95 ms | Tick GC / collections | Capacity | Central | Teardown |
|---|---:|---:|---:|---:|---:|---:|
| Combat1000 | 21.199 / 23.797 / 24.805 | 38.949 / 39.025 | 0 B / 0,0,0 | critical 0 | 1 draw / SetPass 4 | restored |
| Dispersed1000 | 21.432 / 24.771 / 32.177 | 38.309 / 44.265 | 0 B / 0,0,0 | critical 0 | 1 draw / SetPass 4 | restored |

两组均为`harnessValidity=true`、180/180 sampled、1000→0 active cleanup、hash非空、179 central pixel
frames。报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`；
- `Temp/NTSD_ProductionEntityStress.dispersed1000.data-oriented-current-build-smoke.json`。

## Current verdict

短样本validity gate通过；logic tick CPU、strict tick 0 B、capacity和central batching满足短样本合同。
正式30 FPS尚未通过：两组visible frame Avg/P95均高于33.333ms，且Editor profiler frame allocation平均约
7.1KB/frame，虽然短窗口没有Gen collection。必须继续120 warmup + 1800 sampled的60秒报告，并启用
completed-frame timing区分main/render/GPU；不能仅凭logic tick 21ms决定修复方向。

## E-03 formal 60-second matrix

共同配置：120 warmup + 1800 sampled ticks、1000 production GameObject、正式CentralOnly、每帧最多1 tick、
DataOrientedCanonical、LooseQuadtree、completed-frame timing开启、phase/presentation/detail diagnostics关闭。

| Workload | Logic Avg/P95/Max ms | Visible frame Avg/P95 ms | Main P95 | Render P95 | GPU P95 | Result |
|---|---:|---:|---:|---:|---:|---:|
| Dispersed1000 | 16.513 / 18.575 / 34.351 | 9.490 / 25.525 | 25.286 | 0.696 | 3.841 | PASS |
| Combat1000 | 16.313 / 19.044 / 36.864 | 19.078 / 33.058 | 26.901 | 1.015 | 2.841 | PASS |

两组均满足：1800/1800 sampled、logic tick 0 B、Gen0/1/2 collection 0、capacity critical 0、central
1 draw、SetPass约4、hash非空、teardown restored。Combat visible-frame P95低于33.333ms约0.275ms，属于通过
但余量较小，应保留后续Player/Android监控，不能把Editor余量直接外推到移动真机。

completed CPU P95分别为29.677ms和34.320ms；正式门要求的main-thread P95分别为25.286ms和26.901ms，
Render/GPU均不是瓶颈。报告：

- `Temp/NTSD_ProductionEntityStress.dispersed1000.current-build-formal60s.json`；
- `Temp/NTSD_ProductionEntityStress.combat1000.current-build-formal60s.json`。

Editor `GC Allocated In Frame` recorder仍含Editor-side分配：Dispersed Avg/P95/P99/Max约
4.92KB/13.34KB/23.38KB/0.36MB，Combat约8.83KB/13.37KB/29.30KB/4.77MB；但两组战斗tick、driver、
presentation和PlayerLoop边界均0 B，正式窗口内Gen0/1/2没有任何collection，P99远低于单次孤立峰值，
没有观察到周期性大GC。Editor envelope是observational；Player hard gate归WP01F，Android归用户真机。

## E-04 capacity and fallback equivalence

- current-build DesktopExtended/Mobile/pool/slot/generation focused job
  `f554c03eb363475b808fe622d350c9a3`：299/299 PASS；
- 覆盖preflight capacity、sealed growth rejection、lowest-hole reuse、generation、pending destroy、runtime
  snapshot、全部logic pool families与warmed 0 B；
- fresh LegacyCanonical Combat 30+180与DataOrientedCanonical同请求逐项比较：input、RNG、metadata、world、
  slots、aRest、vRest、stats、events、overall、workload和roster共12项hash全部相同；
- Legacy logic Avg/P95=`34.335/43.501ms`，DataOriented=`21.199/23.797ms`，两边zero-GC、cleanup PASS；
- final same-domain full self-check：2026-08-23 14:51:50 PASS。

## Final scope verdict

`R8-WP01E VERIFIED / UNITY EDITOR CURRENT BUILD`：MobileExtended 1000 active、DesktopExtended容量合同、
正式60秒0B/0 collection/0 fault、30 FPS、中央提交与fallback/optimized state equivalence均闭合。

不包括Windows Player（WP01F）、Android真机、T8默认stage.dat或C++ executable full trace；性能证书也不替代
C++ gameplay对齐证书。
