# NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001 — C04 production唯一owner

<!-- CHANGE-RECORD
id: NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDBattleTickSystem.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterInputPass.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28BattleActualPhaseSequenceEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C04FrameMotionProductionOwnerEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan C04 apply_frame_motion global ascending-slot barrier, EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-6-COMPILE-ERRORS / BASIC-DVX-DVY-DVZ-OWNER-ONLY / PRODUCTION-C03-ROUTE-ONLY / PRODUCTION-SERIAL-NO-REPEAT / DIRECT-COMPAT-PRESERVED / UNITY-COMPILE-0 / FOCUSED-11-OF-11-JOB-C7AA85CC / RELATED-211-OF-211-JOB-BABF8E4A / TEST-EMPTYFRAME-POLLUTION-CORRECTED / SELFCHECK-2026-09-05T00-43-10-PASS / REAL-PLAY-KIND0-TICKS-3-6-PASS / RESULT-SHA-5E379F2D / SCENE-SHA-0D74E174-UNCHANGED / PLAY-EXITED / CONSOLE-0 / FULL-30-PARTIAL-4 / NEXT-C05-NATIVE-TELEPORT-VS-COMBINED-EARLY / FULL-C04-B4-PENDING / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / BASIC-C04-OWNER / REAL-PLAY-PASS / FULL-C04-B4-PENDING`

## 改前事实

production exact/shared character在C03 route尾写frame velocity；non-character在后续SimTU中写。没有独立C04
barrier，且同一个概念分散在输入和physics组合入口。

## 实现与验证

- production C03把`applyFrameMotionTail`设为false；direct `CharacterInputAll`默认true，保持combined兼容。
- 新`NativeFrameMotionAll`在C03后/C05前升序遍历active slot，应用现有基础frame velocity并刷新runtime
  snapshot；phase recorder新增`FrameMotion`。
- production `SerialTickAll(..., nativeFrameMotionAlreadyApplied:true)`仅在该调用作用域抑制旧helper，
  `finally`恢复；direct `SerialTickAll`/`SimTU`默认不抑制。
- test-first在抽象夹具修正后得到6条预期缺失phase/seam编译错误；实现后Unity compile0。
- focused `11/11` PASS，job `c7aa85cc7e3d48eaadc7d40ca68b00bd`；输入、frame、late/OID、
  C01/C02/C03、combo、worker与结构相关回归`211/211` PASS，job
  `babf8e4afabc4e2a99e40b3465a0c501`。
- 新测试曾错误修改共享`EmptyFrame`并使SelfCheck出现Vx8；夹具改为先`ImmediateFrame(0)`后再改自有
  frame，domain reload清除污染。最终SelfCheck `2026-09-05 00:43:10 +08:00` PASS。
- 真实kind0 Play tick3～6 PASS，age/RNG/drop/presentation/no-publication/cleanup不变；结果SHA-256
  `5E379F2D002B5466EFC64BD608A293E92F4FCCD0348C4A9E189ABA16AE115E21`。Play退出，Console0；Scene
  SHA-256`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、mtime不变。
- actual full/partial occurrence为30/4；下一首差为C05 native teleport对combined EarlyFrameAdvance。

## 未关闭边界

- linked-platform、delay scale、frame dx/dy/dz仍归B4。
- C05/C06不在本包修改。
