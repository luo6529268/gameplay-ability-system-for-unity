# R8-INP-01 — physical combo first-difference

> 日期：2026-08-23
> 状态：`FIRST DIFFERENCE FOUND / HANDED TO R3-COMBO-01`

## Goal

在不猜测修改技能或DAT的前提下，定位当前真实按钮/按键组合从InputAction到角色frame的第一个断点，
并把`D-INP-006`从用户观察失败推进到可实施的最小修复合同。

## Scope

- Player_1 W/S/A/D/J/K/L 与用户报告组合；
- InputAction callback、local canonical FrameInputSet、roster routing、worker single-flight边界；
- NTSD key/prev/cooldown/combo/frame消费；
- 只在first difference闭合后，另建Change Record实施最小修复。

## Authority / evidence

- C++：`src/input/input_handler.cpp:1555-1609,2750-2886`及实际调用链；
- Unity：`CharacterInputModule`、`SimulationFrameInputModule`、`LocalSimulationFrameInputProvider`、
  `SimulationTickDriver`、`NTSDInputStateModule`；
- 用户当前Play Mode失败报告；
- `RESEARCH/R8-INP-01-physical-combo-first-difference-preflight-20260823.md`。

## Deliverables

1. 一个不改技能结果的逐tick input probe/fixture；
2. 明确first difference属于physical binding、capture/journal、roster、worker cadence或combo consumer；
3. 最小修复Change Record（若确需脚本改动）；
4. fixed-seed/fixed-input focused test与真实Play Mode复测步骤。

## Verification

- compile 0 error、full self-check PASS；
- provider/input focused tests；
- real Play Mode至少一个普通动作和两个组合键序列；
- worker on/off A/B只用于定位，不能把关闭worker当production修复；
- validator PASS。

## Stop conditions

- first difference指向DAT/技能/opoint等其他模块；
- 修复需要改变C++权威组合窗口或pass ordering；
- 需要以输入插值/预测替代确定性tick input；
- 需要修改C++ authority。

## Out of scope

- AI decision、collision/hit、render、T8、Android、服务器；
- 在未取得first difference前直接调整按键映射或组合窗口。

## Current checkpoint

neutral fresh Play Mode已证明battle/roster/worker/FrameInputSet基础路由存活；尚缺真实按键后的逐tick
观察。非改项目的自动输入注入受桌面隔离和MCP execute-code编译器限制阻塞。下一步若获确认，将先建立
`R8-INP-001A` test/diagnostic-only Change Record，再新增默认关闭的逐tick probe/fixture；不会在同一个
Change ID中直接修production input。

后续authority source审计已找到更早的确定断点`D-INP-010`，因此上述通用probe不再是修复启动前置。
implementation移交`R3-COMBO-01 / R3-COMBO-001`；physical edge/worker probe仍作为D-INP-006独立后续，
不得合并到combo修复。
