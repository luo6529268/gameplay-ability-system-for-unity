# R7-CAP-01A Desktop capacity contract

> 日期：2026-08-23
> 状态：`DECISION COMPLETE / CURRENT CODE CONFORMS / NO CODE`

## Selected interpretation

“Desktop无固定生产上限”表示没有512/1000/400这样的产品硬常量，且pre-battle reservation可以按页扩展；
不表示有限内存中的单局能数学无限。strict active-battle 0 B要求本局预算在seal前完全准备。

## Current implementation mapping

- config与CLI可设置Desktop initial capacity；profile按page归一化，active cap为`int.MaxValue`；
- `TryGrowDesktopRuntimeSlots`在unsealed时增长，在sealed时拒绝并计数；
- allocation gate同时seal logic/task/presentation/GameObject families；
- sealed exhaustion已有0 B与deterministic rejection fixtures。

Fresh matrix已经确认以上事实：job `fdf01d6739ac47748158eb42d6d81926` 11/11、
job `e61ed948fc544caf8cc93b31f7859126` 33/33，合计44/44 PASS；随后同域full self-check于
2026-08-23 03:19:45 PASS。

结论：`D-CAP-001`通过澄清delivery合同关闭，不需要`R7-CAP-01B` production改动。Desktop的合同是
“无固定产品级active cap + 每局有限prebattle reservation + active battle seal后strict 0 B + 超预算确定性
admission failure”，不是有限内存中的数学无限，也不是tick内实时扩容。
