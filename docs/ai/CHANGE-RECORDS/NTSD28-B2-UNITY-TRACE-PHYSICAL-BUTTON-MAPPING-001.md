# NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001 — Unity trace physical-button translation

<!-- CHANGE-RECORD
id: NTSD28-B2-UNITY-TRACE-PHYSICAL-BUTTON-MAPPING-001
status: FOCUSED_TEST_PASS
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan physical J/K/L scenario semantics plus the existing Unity crossed FrameInput contract proven by CharacterInputModule and physical-input Play probes.
evidence: TASK-CONTRACT-CREATED / TEST-FIRST-3-CS0117 / FIRST-DIFFERENCE-ACTION-110-VS-210 / PHYSICAL-KL-MISTRANSLATED-AS-NATIVE-JK / EDITOR-ONLY-PARSER-CORRECTED / PHYSICAL-JKL-TO-FRAMEINPUT-JUMP-DEFEND-ATTACK / FIRST-POSTFIX-8-TESTS-ONE-DOMAIN-MASK-FAIL / KL-ACTION-110-STATE7-PASS / CROSSED-INTERNAL-MASK-33-PROJECTED-TO-CANONICAL-17 / COMPILE-0 / FINAL-FOCUSED-8-OF-8 / RELATED-INPUT-20-OF-20 / JOINT-ENTITY-ACTION-STATE-COUNTER-EQUAL / ENTITY-EQUAL-37-DIFFERENCE-10 / FIRST-DIFFERENCE-BASEMAXMP-B11 / SELFCHECK-PASS-20260904-165626 / CONSOLE-0-AFTER-CLEAR / LEDGER-140-94-PASS / PRODUCTION-FRAMEINPUT-BRIDGE-UNCHANGED / AUTHORITY-READ-ONLY / CONFIG-DAT-SCENE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / TRACE_TRANSLATION_CORRECTED / PRODUCTION_UNCHANGED / JOINT_TRACE_RERUN_PASS`

## 改前事实

- existing raw exporter把scenario J/K/L直接映射为FrameInput Attack/Jump/Defend。
- 生产输入合同实际把物理J/K/L承载为FrameInput Jump/Defend/Attack，随后直连legacy crossed runtime fields；
  native proxy再恢复authority attack/jump/defend。
- 因而input-common tick2 slot1物理K+L在Unity诊断路径变成native J+K，产生action110/210首差。
- `SimulationFrameInputModule`与production input已具备真实Play证据，不属于本修正范围。

## 预期改后职责

- raw capture parser只负责将外部物理scenario翻译成现有Unity FrameInput载体；direction不变，J/K/L交叉一次。
- domain exporter继续输出authority canonical applied-input mask，不把内部载体名称当作跨端协议语义。
- 所有production代码和输入资产保持不变。

## 验证记录

- Task Contract与Change Record已在任何测试/诊断脚本修改前建立。
- test-first仅修改测试，Unity编译精确得到3个`CS0117`：新增断言要求的
  `ParsePhysicalInputKeyForTests`尚不存在；红态未触碰production。
- 实际实现只改Editor-only raw exporter：加入internal parser测试seam，方向映射保持，J/K/L改为
  FrameInput Jump/Defend/Attack，并留下本Change ID合同注释。
- 首轮post-fix job `1ead7d4a3b054817be60d299f3afef7b`完成8项；新K+L action110/state7通过，唯一
  failure为旧domain断言expected physical canonical17、actual internal crossed33。这证明运行输入已正确，
  domain trace还需反投影。
- domain输出现将内部Jump/Defend/Attack反投影为physical canonical Attack/Jump/Defend；运行时frameInput
  本身不变。
- final focused job `03b1957078fd49cf860573e9eef49daf`：8/8；包含三键独立映射、K+L action110/state7、
  exact canonical masks、determinism和unknown-key fail-closed。
- related input job `671d04a2e8c3485fb983682ca88e9305`：LocalFrameInput、native producer和native built-in
  production integration共20/20。
- 同一input-common重跑：Unity entity raw SHA `6AFABA89B32AA80294F52EA8C4A2BF1CEB8CC47D9BA57121498DF98CF4D63DD8`；
  domain raw SHA `C28C478B0D45CFDA4FAA6832F363954F3F35F4F572A24B8EA64C5061098DCB08`。
  domain validator valid；domain comparator仍是input/slots/lifecycle equal与RNG topology missing。
- entity comparator SHA `AD963C7376FA6FD2688422FD7334DC6D77AB61F49320BFEF194A807C31E003EA`：
  action/latch/previous/tick snapshot/state/counter均equal，37 equal、10 differences；首差仅为B11内容策略
  `baseMaxMp 500/200`，其余9项均是已诚实标记的missing fields。
- Unity最新编译0 error；SelfCheck 2026-09-04 16:56:26 PASS。自检有7条预期负向错误，期间一次并发
  read_console产生bridge disposed错误；完成后全部清理并确认Console error=0。
- `git diff --check`无whitespace error（仅既有LF/CRLF warning）；Ledger validator通过（140 records、
  94 governed code files）。未修改production输入、Input Actions、Config/DAT、Scene/Prefab或authority。

## 回滚说明

按Task Contract恢复三条测试parser映射并删除新增测试；production runtime无改动。
