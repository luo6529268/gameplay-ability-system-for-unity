# Task Contract — NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001

> 状态：`VERIFIED / SURVIVOR_COMMIT / TERMINAL_PATH_PENDING_B7`
> 依赖：`NTSD28-B3-C25M-PREVIOUS-ACTION-OVERRIDE-AUDIT-001 / VERIFIED`

## 目标

对非early-terminal survivor，在C25l后立即提交`Frame.Prev=Frame.N`，并让后续virtual compatibility tail通过仅限调用栈的old-Prev context保持state13/200旧值语义；C25p仍在cleanup/tail之后。

## 修改范围

- `LF2Entity.cs`
- `BattleLateEntityLifecycleModule.cs`
- 新focused test
- 治理文档

## 不变量

- virtual dispatch和LF2Character N30 cleanup不绕过。
- transient override必须try/finally清除，不进runtime/snapshot/checksum。
- direct `RunLateTailBeforePrevFrame`仍读取当前Prev。
- C25l在commit前，cleanup/tail在commit后，C25p在survivor末端。
- terminal early exit、C25k/n/o、内容/Scene/Authority不改。

## 验收

红灯证明缺少commit wrapper；focused覆盖placement、virtual dispatch、old/new双视图和finally清理；相关late/C25 tests、broad、SelfCheck、Scene/Console/Ledger。
