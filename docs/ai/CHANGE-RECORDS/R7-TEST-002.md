# R7-TEST-002 — worker human current-key fixture correction

<!-- CHANGE-RECORD
id: R7-TEST-002
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSimulationWorkerBoundaryEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp::InputHandler::poll lines 1555-1620
evidence: SOURCE-CONTRACT + EXACT/CLASS TEST + COMPILE + FRESH SELF-CHECK PASS
-->

> 创建日期：2026-08-23
> 类型：test-only stale expectation correction

## 1. 当前状态

`VERIFIED / TEST-ONLY`。production行为与C++ source一致；错误仅在worker Editor fixture，现已修正。

## 2. 原状与预期

- 原测试注释声称frame advance会在消费后清current keys，并断言Left/Attack为0；
- C++ poll当前held state写入`key_left/key_attack=1`，旧值进入Prev；Unity actual=1；
- 最小修正只改两条断言和注释，其他publication/input/cooldown/history断言不动。

## 3. 不可回退边界

- 不改production脚本；
- 不借test修正改变current-key lifetime；
- 不扩入D-TEST-003或D-TEST-001。

## 4. 验收与回滚

按Task Contract验证。若source或exact结果不支持1，应回滚本test-only改动并记录blocker，不得改production迎合测试。

## 5. 实际改动与证据

- 注释明确current key在本tick保持、旧held state进入Prev；
- `KeyLeft`与`KeyAttack`断言由0改为1；Prev、cooldown、history、publication/ack/cleanup断言未动；
- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：0 error；
- Unity fresh scripts compile：Console 0 compiler error；
- exact job `86e6bddd257f4e18bb37433941f1a916`：1/1 PASS；
- class job `41f7b4803c754635b0d7c16abaf73754`：17/17 PASS；
- 2026-08-23 02:14:36 fresh-domain `BattleRuntimeSelfCheck=PASS`；
- production代码、C++ authority、worker/driver/render均未修改。
