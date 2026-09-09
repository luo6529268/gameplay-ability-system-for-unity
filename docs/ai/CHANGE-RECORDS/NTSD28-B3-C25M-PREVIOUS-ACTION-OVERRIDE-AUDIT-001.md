# NTSD28-B3-C25M-PREVIOUS-ACTION-OVERRIDE-AUDIT-001 — C25m virtual tail audit

<!-- CHANGE-RECORD
id: NTSD28-B3-C25M-PREVIOUS-ACTION-OVERRIDE-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan simulation_tick_driver.cpp C25l->previous_action_078 commit; Unity LF2Entity/LF2Character late-tail override graph; EXE B1E13AE1, closure 39DDDA15.
evidence: OVERRIDE-GRAPH-CLOSED / OLD-PREV-READERS-CLOSED / TRANSIENT-CONTEXT-DEFINED / NO-CODE-WRITE
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / TRANSIENT_CONTEXT_DEFINED`

生产`LF2Character`覆写与所有诊断/测试覆写必须保留virtual dispatch。下一实现先capture old Prev、执行C25l、提交C25m，再通过非持久`try/finally`上下文调用现有virtual tail，使state13/200仍读old Prev；terminal路径留B7/C25o。
