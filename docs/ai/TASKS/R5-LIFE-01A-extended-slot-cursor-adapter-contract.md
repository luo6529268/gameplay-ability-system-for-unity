# R5-LIFE-01A — extended slot/newborn cursor adapter contract

> 建立日期：2026-08-22  
> 状态：RUNTIME_PENDING — Extended fixture、fresh Unity编译与full self-check已通过；C++ trace/PlayMode待验。  
> 对应差异：D-SCHED-012（cursor subset）  
> Change ID：R5-LIFE-001A

## Goal

只用focused self-check证明Unity approved Extended容量adapter仍保持C++ Release的slot升序
newborn cursor语义；不修改production runtime。

## Scope

唯一允许脚本：

1. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

允许修改：existing `CheckSimulationWorldLateMutation`、其test-only helper/fixture。

## Required behavior

1. MobileExtended和DesktopExtended-growth都覆盖slot>399；
2. child slot>source cursor：同一LateEntityUpdateAll执行一次；
3. child slot<source cursor：出生pass执行0次，下一pass执行1次；
4. existing Authority400 high/low fixture与lowest-hole allocator/table fixture保持通过；
5. 不改runtime code、profile policy、capacity、generation、pending/free或render。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | C++ allocation/free/late cursor与Unity allocator/registry/late loop reread。 |
| S1 focused | two Extended profile high/low cursor matrix + existing lowest allocator tests。 |
| S2 governance | Record/Ledger/STATE/diff register/main plan/handoff；validator/scoped diff。 |
| S3 Unity | script compile 0 error；full self-check PASS。 |
| S4 honesty | 最高RUNTIME_PENDING；full C++ trace/PlayMode仍未取得。 |

## Stop conditions

- fixture揭示production runtime差异，必须另建gameplay Change Record；
- 需要改allocator/registry/pass order/profile；
- desktop growth或mobile profile无法在test-only范围表达；
- 需要触碰C++ authority或D-RENDER-003。

## Out of scope

R5-LIFE-01B pending/free/generation可观察性、R6 presentation、R1 trace、T8、Android、性能压测。

## 实际结果

- MobileExtended与DesktopExtended-growth的slot700→900 same-pass、slot700→600 next-pass矩阵均通过；
- UnityMCP force refresh触发fresh Tundra 23.19s，Assembly-CSharp更新至17:14:38，无`error CS`；
- 17:15:48 full self-check=`PASS`；17:10旧程序集PASS已作废；
- 没有修改production runtime，状态最高`RUNTIME_PENDING`。
