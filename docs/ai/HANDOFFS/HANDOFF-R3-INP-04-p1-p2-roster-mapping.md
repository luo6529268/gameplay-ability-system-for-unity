# HANDOFF — R3-INP-04 P1/P2 authority fixture and Unity roster-extension boundary

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change Record：`R3-INP-004-001 / RUNTIME_PENDING`

## 已完成的最小闭环

- C++ battle scene 将 P1/P2 固定解析为 runtime object slot0/1，并按 P1 后 P2 的顺序 poll；
- Unity fixed fixture 将 roster player slot0/1绑定到runtime slot0/1的独立 entity；同 tick P1=`Right`、
  P2=`Jump` 后，P1只取得right/cdRight/history6，P2只取得jump/cdAttack/history5；
- roster的runtime slot、stable id和ActiveSlotCount=2都通过断言；
- static、ledger/diff、UnityMCP compile、filtered `error CS`=0和full BattleRuntimeSelfCheck均 PASS；
  最新结果文件为 `Temp/NTSD_BattleRuntimeSelfCheck.result = PASS`（01:28:23 +08:00）。

## 未关闭项 / 不可扩大结论

- Unity 8-slot roster没有被回退、也没有被认证为C++ 3+ player rule；
- 真实双人physical InputAction asset、AppManager生产spawn、scene Play Mode和C++ executable trace均未运行；
- 没有修改production roster/input provider/capacity/pool/physical binding。

## 推荐的连续下一步

按 D-011 进入 `R3-AI-TGT-01`：只对照 C++ target search和Unity fallback/indexed的same-distance、cached
target、team/input-phase behavior，先建立source contract；若fixture需要修改AI decision policy，必须先新建
独立Change Record，不能混入P1/P2或physical binding范围。
