# HANDOFF — R5-OP-01 normal opoint child initial Prev2

> 日期：2026-08-22  
> Change ID：R5-OP-001  
> 状态：RUNTIME_PENDING — 最小脚本、Unity编译与full self-check已通过；C++ trace / PlayMode待验。

## 当前结论

C++ Release child reset后`prev_frame2=0`，spawn只写current action；normal opoint在本tick
collision snapshot之后。Unity Character/Weapon/Other三条initializer提前把Prev2写成action；
SpecialAttack的Prev2 id虽为0但Prev2D为null，会使collision reader回退current action data。

## 已完成动作

1. 三条extra writer已把birth Prev2/Prev2D恢复为0/frame0 data，SpecialAttack已补frame0 cache；
2. existing `CheckFrameLifecycleOpointContracts`已加入四类型nonzero-action production factory fixture；
3. materialize时Prev2=0、下一`CaptureCollisionFrameSnapshotsAll`后Prev2=current的矩阵已通过；
4. UnityMCP force refresh后fresh Tundra build success（23.19s）、Assembly-CSharp 17:14:38、无`error CS`，2026-08-22 17:15:48 full self-check=`PASS`；16:54:37 stale-assembly PASS已作废；
5. ledger validator PASS（38 Records / 29 governed code files），scoped diff check PASS；global diff check只报告用户场景既有14处trailing whitespace，本包未修改场景。

## Stop / exclusions

不得修改spawn pass/cursor、current action、action0 adapter、kind2/multiple、slot/generation、pool、
render、C++ authority、R1 trace或其它gameplay。若现有FrameCache无法表达frame0 Prev2D，停止并记录，
不得用null/current action数据伪造。

## 未关闭证据

- C++ full runtime trace：R1-WP02 BLOCKED；
- real PlayMode：PENDING；
- 因此完成代码级闭环后最高只能写`RUNTIME_PENDING`。
