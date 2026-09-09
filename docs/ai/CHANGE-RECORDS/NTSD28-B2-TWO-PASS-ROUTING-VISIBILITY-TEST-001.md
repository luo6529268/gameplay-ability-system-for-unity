# NTSD28-B2-TWO-PASS-ROUTING-VISIBILITY-TEST-001 — stale routing visibility test correction

<!-- CHANGE-RECORD
id: NTSD28-B2-TWO-PASS-ROUTING-VISIBILITY-TEST-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver all producer/sample first pass then proxy/routing second pass; NTSD28-B2-AI-SAMPLE-PROXY-TWO-PASS-001.
evidence: RELATED-SUITE-296-OF-297-JOB-F29AAAC3AA344DB0AF5851B1EB2FB98B / ISOLATED-FAIL-JOB-225B99098CC0499E952663E1957EC1F9 / ROUTING-MUTATION-MUST-NOT-LEAK-TO-LATER-PRODUCER / FINAL-RELATED-297-OF-297-JOB-B27E33F7CAA84ABFBD66CA6126FB6FCB / SELFCHECK-PASS-2026-09-03T09-50-31 / CONSOLE-0 / PRODUCTION-NO-CHANGE
-->

> 状态：`VERIFIED / TWO_PASS_EXPECTATION_CORRECTED / TEST_ONLY`

旧测试让slot0 non-`LF2Character` character-DAT shell在routing阶段变为frame6/state14，却期望slot1
producer同tick读取该routing结果并改选slot2。2.8两遍合同要求所有producer先完成，routing结果不得回流，
因此slot1保留producer阶段看到的cached slot0才是当前正确期望。只更正测试名称/断言。

测试已更名为`Candidate_EarlierCharacterDatShellRouting_DoesNotLeakToLaterProducer`并期望slot0；
shell最终frame6/state14、零fallback/legacy scan断言保留。最终相关组297/297、SelfCheck PASS、Console0。
