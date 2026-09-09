# NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001 — 功能键正式Host/Session接线

<!-- CHANGE-RECORD
id: NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeyPhysicalLatch.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28NativeFunctionKeySessionState.cs
code-path: Assets/NTSD/Scripts/Simulation/Input/BattleFunctionKeyInputLatch.cs
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyProductionIntegrationEditorTests.cs
authority: NTSD 2.8-Logan playable main/GameSession function-key live closure; verified route and Session carrier packages.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-23-CS0246-CS0103-CS1061 / PHYSICAL-F3-F4-F6-F12-LATCH / F11-F12-CONTINUOUS-F12-WINS / CTRL-F9-F10-MAINTENANCE / B1-F1-F2-F5-ROUTER-ADAPTER / PAUSED-CAPTURE / SESSION-TICK-EXACTLY-ONCE / DEDICATED-WORKER-EXACTLY-ONCE / TYPED-F4-MAINTENANCE-VOLUME-HANDOFF / FORMAL-PHYSICAL-DOES-NOT-ENTER-LEGACY-F789 / BATTLE-BOOTSTRAP-F6-F7-CONFLICT-GUARDED / WARM-4096-ZERO-ALLOC / FIRST-6-OF-9-FIXTURE-PREPARING / FOCUSED-11-OF-11-899D5D81 / RELATED-96-OF-96-5235384A / COMPILE-0 / REAL-PLAY-PHYSICAL-F3-F12-NINE-GROUPS-PASS / PLAY-TICK-0-TO-5 / RESULT-SHA-881C44CC / SCENE-SHA-20984749-UNCHANGED / FULL-SELFCHECK-2026-09-04-205048-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / LEDGER-157-115-PASS / DOWNSTREAM-EFFECTS-EXCLUDED / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / PRODUCTION-CONNECTED / SESSION-EXACTLY-ONCE / LEGACY-PHYSICAL-ISOLATED / REAL-PLAY-PHYSICAL-PASS / EFFECTS-DEFERRED-DOWNSTREAM`

## 改前职责

- B1 F1/F2/F5与旧Config-gated F7/F8/F9由两套physical路径直接产局部command。
- 新router/carrier无production caller；F3/F4/F6/F10/F11/F12没有统一Host/Session handoff。
- tick入口对旧function-key apply存在wrapper+inner重复调用形状，新dispatch不能照搬。

## 目标职责

- 所有正式physical功能键先经同一router；B1继续唯一执行F1/F2/F5 transition。
- Session bit只在逻辑tick入口exactly once dispatch；旧effects不由正式physical入口触发。
- F4/maintenance/volume仅输出typed handoff，等待B8/B10。

## 实际改动

- 新`NTSD28NativeFunctionKeyPhysicalLatch`读取LocalFreeRun Keyboard F3/F4/F6～F12并复用pure router；one-shot以
  held-mask false→true触发，F11/F12每Update刷新。
- driver将F1/F2/F5既有edge通过router filter后交回B1 Host；F3/F6～F9折叠到native event byte并在tick边界
  exactly once dispatch；F4/maintenance/volume只存typed handoff。
- wrapper/inner tick新增显式dispatch ownership，避免direct与dedicated-worker路径二次空dispatch清掉accepted byte。
- 旧`BattleFunctionKeyInputLatch`不再有production physical caller，仅旧diagnostic API可达；正式F7/F8/F9不会写旧
  `InitStatsRequest/Mode2Request`。
- `BattleTestBootstrap`在Running world下禁用旧F6/F7 movement debug热键，避免双副作用。

## 验证计划

1. 新integration tests先取missing seam red。
2. 新physical latch、driver adapter、carrier byte queue与legacy注释最小实现。
3. focused/相关/worker/SelfCheck/Console/validator；Play physical保持诚实边界。

## 实际验证

- red：23条预期missing physical/driver seam。
- 首轮：`6/9`，3项均因fixture停在Preparing而被battle context正确拒绝；让fixture先进入Running后`9/9`。
- final focused：`899d5d81b8574e819261ecba9f25df40 = 11/11`，含dedicated worker与4096 zero allocation。
- final related：`5235384ab4244169889be51446c4b6e5 = 96/96`。
- compile0；SelfCheck 20:41:07 PASS；预期7 negative日志清空后Console0；validator156/114 PASS。

## 未验证与边界

- 后续physical Play probe已PASS并退出Play，故本包runtime gate关闭；证据见
  `NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001`。
- F4 leave、F6 resource consumer、F7 MP tail、F8/F9 object effects、F11/F12 audio/overlay分别留B8/B5/B3+B8/B10；
  typed handoff ready不等于这些效果已对齐。

## 回滚

按Task合同局部恢复旧driver capture/apply；不涉及Config/Scene/asset/Authority。
