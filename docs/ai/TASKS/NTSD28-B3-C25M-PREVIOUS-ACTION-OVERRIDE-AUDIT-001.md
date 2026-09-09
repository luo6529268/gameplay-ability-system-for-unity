# Task Contract — NTSD28-B3-C25M-PREVIOUS-ACTION-OVERRIDE-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / TRANSIENT_CONTEXT_DEFINED`

## 目标与结论

审计所有`RunLateTailBeforePrevFrame`覆写与`Frame.Prev`尾段reader。生产覆写仅`LF2Character`，负责N30 cleanup后调用base；其他覆写为诊断/测试探针。C25m可移到C25l后、cleanup前，但后续virtual compatibility tail必须仍被调用，并以仅在调用栈内有效的提交前Prev上下文供state13/200 branch读取。

## 边界

- 不新增snapshot/checksum/runtime carrier，不在tick间保存override。
- `try/finally`保证临时上下文清除。
- direct compatibility调用仍默认读取当前`Frame.Prev`。
- terminal early-exit slot仍待B7/C25o，不在本包伪修。
- 无代码/内容/Scene/Authority写入；实施另用`NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001`。
