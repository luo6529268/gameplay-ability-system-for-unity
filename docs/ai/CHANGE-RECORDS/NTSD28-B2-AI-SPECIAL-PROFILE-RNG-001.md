# NTSD28-B2-AI-SPECIAL-PROFILE-RNG-001 — sites 0x3C/0x6C and surplus isolation

<!-- CHANGE-RECORD
id: NTSD28-B2-AI-SPECIAL-PROFILE-RNG-001
status: FOCUSED_TEST_PASS
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ai/Kernel/AiDecisionKernel.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28AiSpecialProfileRandomEditorTests.cs
authority: NTSD 2.8-Logan step_special_profile sites 0x3C/0x6C; input router combo index2; Direction B Unity Config current use_ai absence.
evidence: TEST-FIRST-5-EXPECTED-CS0117 / FOCUSED-6-OF-6-JOB-D9FB0EAB9D6747CDA07D4E04AEC0B3BD / RELATED-AI-303-OF-303-JOB-CECC352BFD014C2DAC2DD0CE72577BDC / UNITY-CONFIG-USE-AI-0 / SITES-0X3C-0X6C / LEGACY-PROFILE-SURPLUS-65-ISOLATED-IN-SYNC-MODE / COMBO-INDEX2-HIT-UA / WARM-4096-ZERO-ALLOC / SELFCHECK-PASS-2026-09-03T10-01-37 / CONSOLE-0 / LEDGER-124-RECORDS-72-FILES / LEGACY-PROFILE-TREE-PRESERVED / PRODUCTION-CAPTURE-UNCHANGED
-->

> 状态：`FOCUSED_TEST_PASS / SITES_3C_6C_READY / SURPLUS_65_ISOLATED / PRODUCTION_UNCONNECTED`

本包只在snapshot带synchronized cursor时走native special-profile helper。Unity正式Config `use_ai:`
测量为0处，因此当前缺省0仅按oid33进入6C；legacy CRT与旧角色策略测试保持。

Test-first refresh取得5个预期CS0117，均为`TryApplyNativeSpecialProfile`尚不存在；无其他错误。

## 实际改动与验证

- 新增`TryApplyNativeSpecialProfile`：必先`0x3C`；结果0且当前content-default oid33才消费`0x6C`；
  严格60x7、PP>150、state8/16或roll0与朝向门，命中置legacy `ComboDua`=native combo index2。
- synchronized candidate只调用该helper，不再进入旧66-expression树；其中唯一6C候选被native helper接管，
  因而65个无live ID表达式已隔离。legacy CRT仍完整执行旧树。
- full trace严格为`0x14→0x3C→0x6C`并早退；未出现surplus draw。
- focused job `d9fb0eab9d6747cda07d4e04aec0b3bd`：6/6；全AI相关
  `cecc352bfd014c2dac2dd0ce72577bdc`：303/303；4096 zero-allocation通过。
- SelfCheck 10:01:37 PASS；清理预期负例后Console0；Ledger124/72通过。

## 未关闭项

production cursor仍未接；ordinary movement/held/tail/profiled sites与剩余4个prewrite surplus仍待后续包。
