# HANDOFF — R7-CAP-01 pool / slot / dynamic capacity

> 日期：2026-08-22  
> 状态：`INVENTORIED / D-CAP-001 OPEN / NO CODE CHANGE`

## Current

allocator/generation/pending/pool focused matrix 44/44 PASS。C++ slot50 lowest-free与Unity min-heap/page/generation
adapter映射闭合；Mobile 1000/1050和pool family warm 0 B保持。fresh-domain Unity Console为0条
error/warning，2026-08-22 22:45:05 full self-check PASS。

## Open

- `D-CAP-001`：DesktopExtended只能在battle seal前增长；默认Windows 512后fail closed，与文档“动态、无hard cap”冲突；
- 这不是`PoolMaxSize=200`导致，200仅是pre-seal warning阈值；
- 修复前必须先决定desktop capacity、strict battle 0 B与deterministic admission failure的真实合同。

## Next

汇总R7所有新差异并拆repair WPs。不要在未确认容量合同前解除seal或简单增大常量。
