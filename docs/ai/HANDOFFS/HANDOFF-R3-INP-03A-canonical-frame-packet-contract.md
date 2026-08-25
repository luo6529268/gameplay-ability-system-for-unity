# HANDOFF — R3-INP-03A canonical full-held FrameInputSet packet contract

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change Record：`R3-INP-003A-001 / RUNTIME_PENDING`

## 已完成的最小闭环

- C++ `main.cpp:4607-4608` 直接 poll active P1/P2；`input_handler.cpp:1555-1613` 的 truth 是 current
  held state，顺序为 prev copy → current state → cooldown decrement → new-edge history；
- Unity `FrameInputSet.Buttons` 是实际 application source，`PressedButtons` / `ReleasedButtons` 只为
  capture/journal/checksum metadata，未被 `SimulationFrameInputModule.ApplyFrameInputSet` 当成 gameplay
  override；这与 C++ poll contract 一致；
- 新增仅 self-check 的 `CheckCanonicalFramePacketPollContract`。它验证 `Right|Attack|Jump|Defend`
  packet 的 history order 为 `6,9,0,5`，hold 不重放 history，release 的 prev/current / cooldown 与 C++
  poll 顺序一致；
- static order guard、ledger validator、`git diff --check`、UnityMCP scripts compile、filtered
  `error CS`=0 和完整 BattleRuntimeSelfCheck 均 PASS；result 是
  `Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`（2026-08-22 01:20:32 +08:00）。

## 未关闭项 / 不可扩大结论

- 没有修改任何 production input writer、FrameInputSet semantics、buffer、roster、worker、AI 或 physical
  InputAction asset；
- 没有运行 C++ executable；R1-WP02 full trace 仍 BLOCKED；
- W/S/A/D/J/K/L 的实际 binding、2-human scene、3+ Unity extension、AI target tie/cached target 都不在
  本 Record 中；不能由 packet fixture 宣称它们已对齐。

## 推荐的连续下一步

按 D-011 进入 `R3-INP-04` 的只读 P1/P2 capacity / roster mapping preflight。先确认 C++ active
P1/P2 poll identity、Unity two-human roster binding 和 3+ extension 不会反向影响 authority fixture；只有
发现最小独立差异后，才创建新的 Change Record。不要修改 physical asset 或 production capacity。
