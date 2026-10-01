<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-REDUCED-TEST-CALL-001
status: VERIFIED
change-kind: TEST_FIXTURE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5ReducedState2000AwayDampingEditorTests.cs
authority: current ProjectWriterEffect optional parameter contract; original Editor C051 adjacent job exception
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-REDUCED-TEST-CALL-001.md
-->

# 相邻减伤夹具调用修正

脚本前记录：17项原Editor检查中三个HitPlan调用在行为断言前因参数数量错误失败。实际ProjectWriterEffect有7参数，测试传5参数；计划仅在声明helper补2个Type.Missing以沿用接口默认值。前后职责均为测试投影调用，不改战斗算法、参数语义或断言。范围、风险、验收和回滚见Task。尚未改脚本。

实际修改：仅在ProjectWriterEffect的object[]参数尾部追加两个Type.Missing，保留ref projection索引4，要求CLR使用两个可选参数的既有默认值。尚待编译与原Editor相邻17项重验；原失败结果不删除。

验证：生成Editor工程exit0/0error/266warning；原Editor重新编译后job `0d854bf7635541899e8118c2956706f9` 相邻17/17 PASS，三个HitPlan参数异常消除且原行为断言通过。只有声明helper新增两行，runtime和断言未改，四保护SHA保持。客户端会话关闭报BrokenResourceError，Unity测试终态succeeded及完整结果已落盘；只声明测试fixture修复VERIFIED，不晋升旧B1E13规则到新版权威。[结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/effect-direction-adjacent-result-12.json)。

Change Ledger 1099 records/45 diff代码文件PASS、exit0；本文件diff检查通过，实际差异仅新增两行。
