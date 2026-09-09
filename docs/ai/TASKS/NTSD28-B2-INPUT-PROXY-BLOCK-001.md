# Task Contract — NTSD28-B2-INPUT-PROXY-BLOCK-001

> 状态：`FOCUSED_TEST_PASS / PROXY_BLOCK_READY / PRODUCTION_UNCONNECTED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

建立独立、未接 production entity 的 NTSD 2.8 `Entity+0xBE..0xDE` 精确 0x21-byte 输入 proxy
值合同，冻结字节布局、copy语义与排除字段，为下一包的AI-before-proxy两遍编排提供单一数据边界。

## Authority 布局

- relative 0..2：edge window 0..2；relative 3：defend re-entry cooldown；
- relative 4..7：edge window 3..6；relative 8..14：previous七键；
- relative 15..21：current七键；relative 22..31：combo state 0..9；relative 32：tail DE。
- pending七键、5-key history、run accumulator及DE之后字节不属于copy。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputProxyBlock.cs`（新增）及meta
- `Assets/NTSD/Scripts/Test/Editor/NTSD28InputProxyBlockEditorTests.cs`（新增）及meta
- 本 Task/Change/Ledger/STATE/handoff/总表

## 不变量与验收

- storage在构造时一次预分配；`CopyFrom`与`WriteSerialized` warm path零managed allocation。
- SerializedByteCount必须严格为0x21，布局逐offset验证；copy后source变更不反写destination。
- invalid/null参数fail closed；不提供pending/history/run字段，避免误复制。
- test-first先得到missing type red；final focused全过、compile/Console/Ledger/diff通过。
- 不改 `NTSDEntityRuntime`、AI、人类输入、snapshot/checksum、world pass、DAT、Scene或资源。

## 回滚

删除新增值类型/测试及meta；production无接线需要恢复。

## 实际结果

- test-first missing type `CS0246` 精确命中。
- `NTSD28InputProxyBlock` 构造期预分配7/7/7/10四组bytes，显式保存defend cooldown与tail。
- focused job `9bda6326b2d145fd8e5e78e8d337d161` 5/5 PASS；0x21逐offset、deep copy、
  excluded surface、invalid targets和4096次0 managed allocation均通过。
- Unity compile/Console 0 error；production entity/runtime/pass未接。
