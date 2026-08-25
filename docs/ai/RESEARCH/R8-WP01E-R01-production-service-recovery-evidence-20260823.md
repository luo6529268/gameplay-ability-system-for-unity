# R8-WP01E-R01 production service recovery evidence — 2026-08-23

## Change

- `PollRequest`现在以`BattleTestBootstrap.ProductionStressServicesReady`裁决initial services是否应已完整，
  不再把driver/world partial footprint当作runtime expected；
- 初始Play和合法clean restart后的新Play在Bootstrap未ready时均等待；
- ready-invalid、previously-healthy invalid、restart transition和retry limit保持fail-closed；
- pool、Bootstrap、scene、production gameplay、C++ authority均未修改。

## Code verification

- fresh Editor DLL：2026-08-23 14:33:57；Console C# error 0；
- focused job `2bcc822ceddb45f9955a3041a3ade51f`：263/263 PASS；
- full `BattleRuntimeSelfCheck`：2026-08-23 14:35:20 PASS。

## Same-request runtime replay

重新执行与首败完全相同的
`Run 1000 AI Data Oriented Capacity Pressure Smoke`：

- 首败中的premature clean restart未再发生；
- 1000 active production GameObject、1000 world entity、1000 claimed slot；
- 30 warmup + 180 sampled logic ticks完整执行；
- logic tick Avg/P95/Max=`21.199/23.797/24.805 ms`；
- logic tick allocation Avg/Max=`0/0 B`，Gen0/1/2 collection=`0/0/0`；
- capacity critical delta=0，presentation-only voice drop不计capacity fault；
- central source/resolved commands Avg=`1969.838`，1 chunk/segment/draw，179 submitted-pixel frames；
- SetPass=4；final lockstep/parity hash均非空；
- teardown：activeGO 1000→0、world entities 1000→0、claimed 1000→0、pool active 0→0，
  driver/logger恢复，cleanup exception 0；
- terminal result=`PASS`。

报告：`Temp/NTSD_ProductionEntityStress.combat1000.data-oriented-capacity-pressure-smoke.json`。

## Boundary

本证据关闭`R8-PERFBOOT-001`的初始化生命周期首差，并使WP01E E-02 Combat短样本validity gate通过。
它不关闭正式60秒30 FPS门：本短样本Unity visible frame Avg/P95=`38.949/39.025 ms`，且Editor profiler
frame GC allocated平均`7128.94 B/frame`，虽然本窗口没有Gen collection。必须继续Dispersed短样本和正式
1800-tick报告，不能用logic tick 21.2 ms单独宣称实际可见帧稳定30 FPS。
