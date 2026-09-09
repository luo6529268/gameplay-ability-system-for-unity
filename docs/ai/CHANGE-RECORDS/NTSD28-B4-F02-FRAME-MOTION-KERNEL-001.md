# NTSD28-B4-F02-FRAME-MOTION-KERNEL-001 — native frame-motion kernel

<!-- CHANGE-RECORD
id: NTSD28-B4-F02-FRAME-MOTION-KERNEL-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B4FrameMotionKernelEditorTests.cs
authority: NTSD 2.8-Logan frame_motion.cpp FrameMotion28::apply plus input_routing.cpp strict depth intent; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-CS0103-X1 / COMPILE0 / FOCUSED5 / RELATED45 / NTSD28-BROAD455 / SELFCHECK-PASS-2026-09-05T04:12:57Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS
-->

> 状态：`VERIFIED / PRODUCTION_NATIVE_KERNEL / STRICT_DEPTH_INTENT`

## 实际改动

- 新`BattleNativeFrameMotionKernel.Apply`统一X facing-space clamp、Y add/override、Z intent/override；阈值500和偏置550与Authority一致。
- production `ApplyNativeFrameMotionForWorldPass`直接读取current frame和current Up/Down，strict XOR生成`-1/0/+1` intent；双按与双未按均none。
- legacy `ApplyNonCharacterFrameVelocityForFrameAdvance`和direct SimTU未改。

## 验证

- 红灯：1个预期`CS0103 BattleNativeFrameMotionKernel`。
- focused job `58cfc624f55047c18a497a9cc6985341` 5/5；X正负/facing/override、Y加法/override、Z none/正负/override以及production双按/单按均覆盖。
- related job `cfb151ef496f46e7a3c479a744e065df` 45/45；broad job `fa6dc06c9ffc43c18ba2ab02436c1dce` 455/455。
- SelfCheck于`2026-09-05T04:12:57Z` PASS；clear后Console error0。
- Scene SHA/length/mtime仍`0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z`；diff-check无whitespace error；Ledger 211 Records / 190 governed code files PASS。

## 未关闭

physics integration、teleport后坐标、frame step、revival与linked platform不在本包；下一F04 physics audit。
