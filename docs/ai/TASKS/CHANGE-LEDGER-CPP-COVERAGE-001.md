# Task Contract — CHANGE-LEDGER-CPP-COVERAGE-001

> 状态：`VERIFIED / GLOBAL-LEDGER-PASS / CPP-NEGATIVE-PASS`  
> 建立日期：2026-09-02

## 目标与范围

将本仓库 `Tools/` 下自编写的 C/C++ source/header 扩展名纳入现有 Change Ledger validator，
保证 B0 authority source capture runner 与后续外部 adapter 不能绕过脚本审计。

允许修改：`Tools/Validate-ChangeLedger.ps1`、本 Task/Change、Ledger、STATE、handoff。
不修改 Unity、authority、Server、Scene、DAT 或资源。

验收：PowerShell parse、模拟未登记 `.cpp` fail、当前已登记 `.cpp` covered、全局 Ledger PASS。

回滚：移除新增扩展名并保留记录；不处理或删除任何用户代码。

实际验证：PowerShell parse PASS；实际 `.cpp` covered；模拟未登记 `.cpp` fail closed；全局
Ledger `67 records / 9 governed diffs / PASS`。
