# NTSD28-B2-SELFCHECK-PRIVATE-MEMBER-SEAM-001 — self-check private member seam

<!-- CHANGE-RECORD
id: NTSD28-B2-SELFCHECK-PRIVATE-MEMBER-SEAM-001
status: VERIFIED
change-kind: TEST_ONLY
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Current Unity SimulationWorld private camera carrier shape and user-approved fixed-world-camera validation contract.
evidence: RED-SELFCHECK-CAMERA-STALE-INJECTION-FAILED / PRIVATE-PROPERTY-CONFIRMED / FIELD-OR-PROPERTY-SEAM / MISSING-FAIL-CLOSED / CAMERA-CHECK-ADVANCED / FULL-SELFCHECK-PASS / PRODUCTION-UNCHANGED
-->

> 状态：`VERIFIED / FULL_SELFCHECK_PASS / TEST_ONLY`

测试反射注入现支持private field/property且missing fail closed；相机check越过，full SelfCheck最终PASS。
production与相机行为不变。
