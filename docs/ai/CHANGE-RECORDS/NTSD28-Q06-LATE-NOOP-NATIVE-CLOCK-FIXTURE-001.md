<!-- CHANGE-RECORD
id: NTSD28-Q06-LATE-NOOP-NATIVE-CLOCK-FIXTURE-001
status: VERIFIED
change-kind: NATIVE_CONTRACT_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/BattleEcsLateTailNoOpEditorTests.cs
authority: Formal resource-phase eligibility / lifecycle pending-code contract from prior source witnesses.
evidence: Fresh joint64 tests57pass7fail; stack traces captured in parent death-prelude joint-initial.xml.
-->

# 独立旧夹具修正

IN_PROGRESS。两个no-op测试默认NativeResourcePhase12/3均0，实际应执行资源owner，旧tickIndex5不是原版clock。仅Neutral/Warmed两场景显式设phase12=1/phase3=1，保留no-op/零分配断言及其它period测试。

仅已列范围；原失败保留，新focused及相关联合复验，完整SelfCheck范围如实报告。保留正常hit/physics/WPoint及有序关闭，不修改全局架构、资源或Scene。没有新runtime服务/关闭职责；回滚仅本差量且需批准。

上述准确fixture范围已修改，编译0，正在联合复验。原64项57/64及stack traces保留于parent joint-initial.xml。

VERIFIED / TEST_FIXTURE_ONLY，准确Native clock/pending前置修正后64联合全PASS；没有生产规则变化，原失败保留。
