# NTSD28-B3-C25A-B-DEFINITION-CLONE-001 — C25a-b definition and clones

<!-- CHANGE-RECORD
id: NTSD28-B3-C25A-B-DEFINITION-CLONE-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25DefinitionCloneEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25DefinitionClonePlayModeProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsLateTailNoOpEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Promoted NTSD 2.8-Logan BattleWorld28 apply_definition_transition_slot then materialize_special_state_clones within C25; exact synchronized callsites; EXE B1E13AE1, closure 39DDDA15.
evidence: RED-CF790519-5-OF-6-EXPECTED / COMPILE-0 / FOCUSED-BEE51C34-11-OF-11 / RELATED-9FB7D4D5-30-OF-30 / NTSD28-AC4C3926-399-OF-399 / SELFCHECK-2026-09-05-07-13-17-PASS / PLAY-PASS-SHA-15285774 / SCENE-UNCHANGED / PLAY-EXITED / CONSOLE-0
-->

> 状态：`VERIFIED / C25A-B-PRODUCTION / TARGETED-PLAY-PASS / B11-DEFINITION-STATS-PENDING`

## 改前事实

- production调用旧`RunStateSpecialPreCollision`，会执行当前Authority C25a没有的state9995和4000 transform，并把8000 transition强制写frame0/render offset140。
- C25b shape/slot/RNG次数近似，但使用Unity legacy `DeterministicRng`，不是2.8 synchronized stream/callsite；newborn spawner与HP/MP初始化也不等价。
- direct self-check仍可保留旧入口用于历史兼容，但不能继续定义production。

## 计划

先写production-level失败测试，再新增C25a native data writer并将C25b切到native synchronized RNG和准确birth字段；随后更新受影响的旧virtual优化断言并执行全套验证。

## 实际改动

- `LF2Entity.TryApplyNativeC25DefinitionTransition`先解析target catalog/wrapper/action再原子提交OID、FrameCache、action、WaitCounter/action-latch、PrevFrame2/tick snapshot与frame counter；清除旧render offset并保留weapon HP。
- `BattleLateEntityLifecycleModule` production只调用native C25a；旧virtual和legacy RNG仅供`RunStateSpecialPreCollisionForSelfCheck` direct compatibility使用。
- C25b的x/y/vertical/z/x/action/facing顺序分别绑定`0x0041F792/7B6/818/8A9|87F/8DD|908|92E/955/96B`；5个成功clone总34次。
- 随机action缺失在消费该候选tuple后拒绝spawn；definition/capacity缺失在RNG前退出。native birth写HP/HPBound/HP3/PP=10、owner/spawner=-1、group0、arest6，weapon HP仍由target DAT初始化。
- 新增11例production focused与真实Play probe；更新旧late no-op测试，只撤销production对state9995/4000及派生virtual的依赖。SelfCheck新增native production合同，同时保留历史direct fixture。

## 验证

- 红灯job `cf790519cf834b27a03cac385b68bd7c`：6例完成，5个预期失败。
- compile0；focused `bee51c34dc724299aa7311a0147574f1` 11/11；related `9fb7d4d5c17f4e689414e04df6e1ec74` 30/30；NTSD28 broad `ac4c392619dc416e8a2173539e53c471` 399/399。
- SelfCheck 07:13:17 PASS。Play artifact PASS，SHA `15285774A6D85A36B908A3B9FDE76648FDE420747A4B1538389320A08C98C903`。
- Play exited、Console0、Scene SHA `0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77` unchanged。

## 剩余边界

C25a的`stats.max_mp/defend/mode_damage_scale`当前无正式Unity内容/运行时载体，留B11；opoint/sound latch与lifecycle code载体也仍属于B7/C25K-P。C25c-p、legacy serial和global post-tail均未因本包关闭。下一包是C25c-e resource/display。
