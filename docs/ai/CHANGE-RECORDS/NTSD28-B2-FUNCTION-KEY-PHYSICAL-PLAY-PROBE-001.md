# NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001 — 真实Play物理功能键验收

<!-- CHANGE-RECORD
id: NTSD28-B2-FUNCTION-KEY-PHYSICAL-PLAY-PROBE-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28NativeFunctionKeyPhysicalPlayModeProbeEditor.cs
authority: NTSD 2.8-Logan formal physical function-key host/session closure; production integration package.
evidence: TASK-CONTRACT-CREATED / COMPILE-0 / REAL-PLAY-NTSD-BATTLE / LIFECYCLE-RUNNING / KEYBOARD-DEVICE-1 / TICK-0-TO-5 / F6-PASS / F7-PASS / F8-F9-PASS / F3-F6-LOCK-PASS / PLAIN-F10-PASS / F11-F12-HELD-RELEASE-PASS / F4-HANDOFF-PASS / CTRL-F10-PASS / RESULT-SHA-881C44CC9F8EFB2FC5F24B91CF86CA3CC2AD457D1F4DE0C954DD9C5D553355E1 / PLAY-EXITED / SCENE-SHA-20984749C3309774A02E8A174EFC2B12ED0268D2A42C7411B647AFBAA028018B-UNCHANGED / FULL-SELFCHECK-2026-09-04-205048-PASS / EXPECTED-NEGATIVE-7-REVIEWED / CONSOLE-0 / LEDGER-157-115-PASS / PRODUCTION-UNCHANGED / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / REAL-PLAY-PHYSICAL-F3-F12-PASS / SCENE-UNCHANGED / PRODUCTION-UNCHANGED`

## 目标

只新增request-driven Editor Play probe，对真实Keyboard/InputSystem→driver production path做F3/F4/F6～F12验收。

## 允许与排除

- 允许新probe与Temp request/result；允许自动进入/退出Play。
- production/Config/Input Actions/Scene/Prefab/DAT/Authority零修改；downstream effects不执行。

## 验证计划

compile→静态/Editor加载→进入Play→写request→等待result→退出Play→Console/SelfCheck/Scene dirty/validator。

## 实际结果

- probe compile0；Play进入`NTSD_Battle`，driver=`Running`，Keyboard device1。
- synthetic hardware-state boundary按计划完成，result为PASS，tick0→5，九组证据全部true；并非pure latch冒充。
- F7没有进入旧InitStats，F8/F9没有进入旧Mode2，F6/F7未触发Bootstrap movement debug；F4/audio仅验typed handoff。
- Play已退出；Scene SHA前后`20984749...018B`一致，result SHA`881C44CC...55E1`。
- 20:50:48 SelfCheck PASS；Console0；validator157/115 PASS。

## 回滚

删除probe与Temp产物；production零改。
