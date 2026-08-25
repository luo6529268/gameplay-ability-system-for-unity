# R4-HIT-002D — normal weapon-attacker raw-frame / ordering writer

<!-- CHANGE-RECORD
id: R4-HIT-002D
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\hit.cpp:342-361,465-482
evidence: SOURCE-CONTRACT-VERIFIED / UNITY-COMPILE-PASS / FULL-SELFCHECK-PASS / RUNTIME-PENDING
-->

> 创建日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — attacker writer与五类fixture已通过Unity compile/full self-check；C++ trace与Play Mode未关闭。

## 范围与当前差异

仅覆盖normal kind0 weapon-victim route的attacker state3000/state1002。C++ state3000在victim generic knockdown
前按current DAT oid/type和frame40处理skip，再raw写10并显式clear attacking；state1002在later位置raw写random16，
没有attacking clear。Unity原先将两者合入later `ApplyWeaponAttackerResponse`，先state1002后state3000，使用
`ImmediateFrame`并遗漏weapon-victim skipReset；现已拆为state3000 pre-knockdown raw writer与state1002 later raw
writer，且将oid209 skipReset限定在weapon-victim route。

本记录不授权修改02C、global helper、weapon vital/stat、vrest/held算法、RNG、candidate、scheduler、input、AI、
render、DAT、C++ authority或任何其他包。

## 计划代码路径

| 文件 | 方法 | 目标 |
|---|---|---|
| `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs` | `ApplyWeaponDamage` + `ApplyWeaponAttackerState3000PreKnockdown` + `ApplyWeaponAttackerState1002Response` | 分离state3000 pre-knockdown与state1002 later raw writer，保留显式字段与skipReset |
| `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` | `CheckWeaponAttackerRawFrameAndOrderingContract` | 真实命中五类状态/顺序/skip/RNG合同 |

## 验收状态

- C++ source：`PASS`（只读，Makefile已确认参与release）；
- Unity code：`CODE_WRITTEN`；仅包含上述局部writer及五类真实命中fixture；
- Unity compile：`PASS`，UnityMCP刷新后`error CS`=0；
- full self-check：`PASS`，`Temp/NTSD_BattleRuntimeSelfCheck.result`为`PASS`（2026-08-22 06:36:40 +08:00）；
  Console其后仅有两个既有rest-binding negative-control日志，未见C# compiler error或02D fixture失败；
- Play Mode：`PENDING`；
- C++ runtime trace：`BLOCKED`（R1-WP02）。
