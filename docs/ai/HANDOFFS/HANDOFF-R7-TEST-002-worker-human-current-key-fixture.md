# HANDOFF — R7-TEST-002 worker human current-key fixture

> 日期：2026-08-23
> 状态：`VERIFIED / TEST-ONLY`

## Current

C++ source与Unity production都保留本tick current Left+Attack=1；worker exact fixture现已改为1。
exact 1/1、class 17/17、compile 0 error、02:14:36 fresh self-check PASS；production未改。

## Next

按repair sequence进入R7-TEST-003 worker/central/ack joint fixture；不要重开production input。

## Stop

若后续发现current-key差异，必须以C++ poll/source和新fixture定位；不得恢复旧0断言。
