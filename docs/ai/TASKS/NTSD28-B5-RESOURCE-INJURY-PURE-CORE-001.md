# Task Contract — NTSD28-B5-RESOURCE-INJURY-PURE-CORE-001

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`
> 依赖：`NTSD28-B5-HIT-RESOURCE-PREREQUISITE-AUDIT-001 / VERIFIED`

## 目标

实现`native_hit_resource_injury28`的纯整数核心，不接尚缺的definition/mode production carrier。

## Authority 合同

- injury-double正数时先按32位低位语义乘2。
- definition attacking>=1优先；否则使用active-mode percent；两者均<1或injury==0时返回当前injury。
- injury*multiplier保留32位低位，再按有符号整数除100。
- remainder>=50时quotient+1；负remainder不向上修正。

## 不变量

- pure、无分配、无runtime写入、无RNG。
- 不接production，不新增/猜测definition stats或mode rules carrier。
- 不改Config/DAT/Scene/Authority/资源/snapshot schema。

## 受影响路径

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B5ResourceInjuryPureCoreEditorTests.cs`

## 验收

test-first compile red；double、priority/fallback、49/50 rounding、negative remainder与两个overflow
边界；相关B5、精确NTSD28 broad、SelfCheck、Scene/Console/Ledger。

## 回滚

移除pure helper与focused test；production不受影响。

## 验证结论

- test-first fresh compile red：3个预期`CS0117`。
- fresh compile 0 error；focused `9a7bca7442e1457aa1a0feba8114b497` 9/9。
- B5/hit/carrier related `d0453fbe3ef34274bfbb661384d62d10` 249/249。
- 精确NTSD28 broad `186b6bbafaeb4b6b9d16a60d0935e9e3` 625/625。
- BattleRuntimeSelfCheck `2026-09-05T12:12:35Z` PASS；Scene unchanged。
- production仍等待stats/mode/suppression与world gate carrier，未伪接默认值。
