# 真实Play新发现：Preparing关闭owner缺口

结论：Preparing状态已创建4个World对象时，现有App关闭未完成。不是E2发布成功，也不能以正常Running关闭成功覆盖。

- play-cycle-1.json首先记录失败但缺细分阶段。
- play-preparing-diagnostic.json确认失败在PendingObjectPointTasksDiscarded之后：`unity-renderers-remained-without-an-object-pool-owner`，World残留4/slots2。native public入口在该状态正确拒绝切源；未执行候选发布四条检查。
- play-running-cycle-1.json已观察Running真实world，并正常关闭到RuntimeMapCleared/Completed、剩余计数0。随后的fixture日志使用NUnit TestContext而probe无该上下文，导致测试辅助NullReference；已改用Unity Debug.Log。此失败原始报告保留，不算Play通过。

独立回访：docs/ai/TASKS/NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001.md / READY_AUDIT。需要确认首次materialize前的factory/pool owner绑定及关闭后bootstrap continuation，保持现有十一阶段与框架。

当前不修改普通战斗规则或把owner缺失包装成0 borrower。E2实际已通过27/27 EditMode（包括全body/common/atlas/UI、取消/重试、Driver hook、旧SelfCheck子项）；完整运行时出口仍等待本回访及Play重进证据。
