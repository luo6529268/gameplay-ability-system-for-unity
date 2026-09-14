VERIFIED / TEST_FIXTURE_ONLY，准确Native clock/pending前置修正后64联合全PASS；没有生产规则变化，原失败保留。



# 独立旧夹具修正

IN_PROGRESS。两个no-op测试默认NativeResourcePhase12/3均0，实际应执行资源owner，旧tickIndex5不是原版clock。仅Neutral/Warmed两场景显式设phase12=1/phase3=1，保留no-op/零分配断言及其它period测试。

仅已列范围；原失败保留，新focused及相关联合复验，完整SelfCheck范围如实报告。保留正常hit/physics/WPoint及有序关闭，不修改全局架构、资源或Scene。没有新runtime服务/关闭职责；回滚仅本差量且需批准。
