# Task Contract — CHANGE-LEDGER-GOVERNANCE-ONLY-METADATA-001

> 状态：`VERIFIED / GLOBAL_LEDGER_PASS / NEGATIVE_FIXTURES_PASS`  
> 建立日期：2026-09-02

## 1. 目标

使 `Tools/Validate-ChangeLedger.ps1` 能准确表达“只有治理文档、没有自编写脚本路径”的
Change Record，避免为满足校验器而虚构脚本所有权；随后将既有
`CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001` 按其 Server 同 ID Task/Change 和客户端原记录
已确认的事实，标记为治理专用、无代码路径。

## 2. 允许范围

- `Tools/Validate-ChangeLedger.ps1`
- `docs/ai/CHANGE-RECORDS/CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md`
- 本 Task、同 ID Change Record、Ledger、STATE、当前 handoff

不修改 Unity runtime、测试、Scene、资源、DAT、Server 仓库或 NTSD 2.8 权威目录。

## 3. 合同

- 普通记录仍必须至少声明一个受治理脚本 `code-path`。
- 只有同时声明 `change-kind: GOVERNANCE_ONLY` 与唯一 `code-path: NONE` 的记录可以无脚本路径。
- `NONE` 不进入脚本所有权映射，不能覆盖任何 authored script diff。
- `NONE` 与真实脚本路径混用必须 fail closed。
- 旧记录只补充元数据，不改写其 `VERIFIED / NO_RESOURCE_EDIT` 历史结论。

## 4. 验收

1. PowerShell parser 无语法错误。
2. 当前全局 Ledger validator 通过。
3. 普通缺少 `code-path` 的临时 fixture 仍失败。
4. `GOVERNANCE_ONLY + NONE` fixture 通过元数据阶段，且不能覆盖模拟脚本 diff。
5. 不产生 Unity/runtime/resource 行为变化。

## 5. 回滚

回退 validator 对 `NONE` 的识别，并移除旧记录新增的两条元数据；保留本 Task/Change 及失败
证据，不触碰任何用户代码或资源。

## 6. 实际结果

- PowerShell parser 通过。
- 当前全局 validator 通过：64 Records，5 个当前脚本 diff 全部有真实 Record 覆盖。
- 缺失 `code-path` 的临时负向 fixture 仍 fail closed；fixture 已移除。
- `GOVERNANCE_ONLY + NONE` 不覆盖模拟脚本 diff，模拟路径仍被报告为 unrecorded。
- 无 Unity/runtime/resource/authority 行为变化。
