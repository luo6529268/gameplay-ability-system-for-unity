<!-- CHANGE-RECORD
id: NTSD28-Q06-C25-RENDER-PHASE-20-FIXTURE-001
status: VERIFIED
change-kind: SOURCE_PHASE_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25FHJTimerOwnerEditorTests.cs
authority: Formal BattleWorld28 step_frames_range has no action202 render write; advance_native_revivals alone writes phase20 and action212.
evidence: Parent core passes2676; existing focused19 has one stale action202 producer expectation failure.
-->

# C25h phase20前置夹具修正

修正已观察的旧C25h夹具：单独action202不在正式step_frames_range中写render_phase20；source唯一render_phase_008=20在advance_native_revivals中并选择action212，不能沿用旧C#每tick action202=20的结论。新fixture保留C25h刚写15不递减与已有20递减为19两个断言，并增加无前置写入的action202保持0；仅注入前置phase值测试C25h，不冒称完整revival运行。初轮相关19测试18PASS/1FAIL（旧expected19实际0）保留父artifact/related-19-initial.xml。禁止恢复旧production action202 writer以取悦测试。代码前后仅此方法，未改其它poison/phase断言。验收focused本类/父core；回滚需批准仅本差量，用户工作不回退。
最终VERIFIED_TEST_ONLY：父focused-28-pass.xml中本类全部通过；bare202=0、前置20→19、新15保持均保留。父frame事务仍IN_PROGRESS，未改任何生产以满足此夹具。
