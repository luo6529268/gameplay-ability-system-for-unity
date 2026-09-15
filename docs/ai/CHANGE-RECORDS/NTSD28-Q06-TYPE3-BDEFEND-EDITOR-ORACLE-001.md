<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE3-BDEFEND-EDITOR-ORACLE-001
status: VERIFIED
change-kind: TEST_ORACLE_FIELD_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointKind2HurtConsumerRetirementEditorTests.cs
authority: Current playable battle_world.cpp unarmored branch writes bdefend_accumulator=45; BDEFEND-FIELD-FAMILY-UNITY-001 maps this to Runtime.Bdefend while preserving legacy HitStateCount.
evidence: CPoint throw regression job4cf4c995960b4b7a8a3cce3f36b46d83 retained two failures: legacy HitStateCount expected45 actual0. Independent source review confirms omitted Editor oracle migration; production tail already writes Runtime.Bdefend=45.
-->

# Type3 旧 Editor 字段断言修正

IN_PROGRESS / TEST_ONLY。准确Task同ID。只修改Type3Tail_DoesNotReadPreviousCpointAsAction(bool)：初始化Bdefend7与legacy HitStateCount241；期望前者覆写45，后者保持241。保留两种朝向及原action/counter/HP/Fall/owner/catch关系和文件其它静态/alias断言。

这是Q06依赖回访，不修改BattleDamageWriter或任何生产行为；没有schema、runtime owner、Scene、资源或非战斗变化。已有SelfCheck oracle Record不含此Editor路径，故单独留痕。原失败XML位于CPOINT-THROW-NATIVE-RAW-BINDING-001/after-first-fix。

验收：Unity编译与本类全部focused实际通过；不以私有tail单测宣称完整命中对齐。回滚仅本测试差量，依规则先批准，不回退其它任务或用户工作。禁止computer-use和提交推送。

已写准确单方法：Bdefend7→45、legacy241保持，其它断言未改。待Unity编译及focused。

VERIFIED / TEST_ORACLE_ONLY。job77b90029625240d7a813667411da2f93本类13/13 PASS，编译CS0；联合另2条投掷完整driver失败明确归属原Task，不是本oracle。XML独立归档。原生产未修改，本结论仅字段观测修正。
