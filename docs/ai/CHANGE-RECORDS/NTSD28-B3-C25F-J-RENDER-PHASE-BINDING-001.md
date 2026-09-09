# NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001 — render phase binding

<!-- CHANGE-RECORD
id: NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001
status: VERIFIED
change-kind: TEST_FIRST_DIAGNOSTIC_BINDING
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Tools/NTSD28Parity/TraceContractSelfTest.cs
code-path: Tools/NTSD28AuthorityTrace/authority_source_capture_main.cpp
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 render_phase_008 C25h/native AI/render snapshot; formal EXE B1E13AE1, playable closure 39DDDA15.
evidence: RED-54F5E3F6-1-OF-3 / RENDER-PHASE-HITSTOP-UNIQUE-BINDING / SOURCE-VALID-3TICKS-6ENTITIES / JOINT-RAW-288-OCCURRENCES-EQUAL-FIELD / TOOL-BUILD0-SELFTEST21-RAW5 / UNITY-FOCUSED15 / NTSD28-BROAD406 / SELFCHECK-08-53-55-PASS / SCENE-UNCHANGED / CONSOLE0 / NO-RUNTIME-BEHAVIOR-CHANGE
-->

> 状态：`VERIFIED / RENDER-PHASE-HITSTOP-BINDING / JOINT-RAW-EQUAL / NO-RUNTIME-BEHAVIOR-CHANGE`

## 改前事实

- Authority `render_phase_008`正负向0移动，并被native AI与render snapshot共同读取。
- Unity `HitStop`具有同一15/20/30 writer、正负移动和显隐阈值/四相公式；但旧B2 AI工作把render phase临时投影为Y。
- 47-field raw contract没有render phase，因此当前无法用Y/phase互异fixture裁决。

## 计划

先红灯，再把raw contract扩为48字段，在workspace source runner和Unity diagnostic exporter中分别读取Authority phase与Unity HitStop；用共同scenario/comparator锁定唯一绑定，不改runtime行为。

## 实际改动

- entity raw contract增加严格字段`combat.renderPhase`，绑定`EntityState28::render_phase_008 -> NTSDEntityRuntime::HitStop/LF2Entity::HitStun`；总数47→48、verified38→39。
- workspace Authority source runner输出真实`render_phase_008`；Unity raw projector输出真实`Runtime.HitStop`。
- Unity scenario exporter增加`renderPhase008`初始化与current formal EXE SHA；新增Y/phase互异scenario和重复性focused test。
- 对旧B2 held-AI Task/Record追加correction：Y临时映射已被新证据取代，历史测试结果保留但不再裁决current binding。

## 验证

- red `54f5e3f642c24fa9a0aa57392400344c`：1/3 expected fail；旧JSON仍47/38。
- source runner build0 warning/0 error；manifest closure`07CD47A0...778F`，runner SHA`FC5F1055...EA60B`，binary SHA`411ACE26...F9260`。
- tool build0/0；trace self-test21/21、raw self-test5/5；Authority capture validator valid-source-model-capture。
- Y/phase互异raw：Authority SHA`C16612641C97DEE39269836327E95ED71D273D92ACCEA0713B0F9F83BBEB00C8`；Unity SHA`31031CF9818129F9BCBF5D8F1E3DCBA2BBDF252A120E3BFCEF02743B78475012`。
- comparator report SHA`092A307C72D9DC3277D940747520AC66EFA75884A95288897D1FF21E3BA6ED29`：3 ticks、6 pairs、288 occurrences、renderPhase全等；其余17差异未归一化。
- Unity focused `1e6c394774864be9a2179633398e0ccf` 15/15；NTSD28 broad `3316c77333374a5ca4ef8d5619b99948` 406/406；compile0。
- final SelfCheck 08:53:55 PASS；未Play，Scene hash/mtime不变，post-clear Console error0。

## 未关闭边界

- source-model trace仍非formal EXE certificate。
- AI kernel/runtime中仍以Y实现的Authority render-phase consumers是已确认行为缺口，必须另包迁移并做AI RNG/decision回归。
- C25h的render phase decrement只是现有Unity writer事实；显式h owner、其他timer bank与status算法仍未实施。
