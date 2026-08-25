# HANDOFF — R4-HIT-02D normal weapon-attacker raw-frame / ordering writer

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-002D`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改、复制或写入C++ authority。

## 已确认的 C++ / Unity first difference

- C++ `hit.cpp:342-361`的state3000先于weapon victim generic knockdown运行，并以**初始**victim current oid/frame决定
  oid209 skipReset；Unity当前在generic victim writer之后才处理state3000，frame40例外会丢失；
- C++ `hit.cpp:465-482`的state1002在later位置raw写random16，之后写Vx/Vy/type4 knockback，且不清attacking；
- Unity当前将两者合入一个helper，顺序为1002→3000且均用`ImmediateFrame`，会额外改PN、attacking、wait；
- `hit.cpp`已确认在release `Makefile:19`，全部证据来自只读source。

## 已写入的最小改动

- `ApplyWeaponDamage`现于generic victim knockdown之前调用state3000 local writer，并在arest/vrest/holder之后、
  weapon-victim tail之前调用state1002 local writer；未改global pass order；
- state3000使用`DirectWriteRawFramePreserveWaitCounter(10)`、保留显式attacking/Vx/Vz写入与oid209 skipReset；
- state1002使用raw random16、保留C++的Vx/Vy/type4 knockback且不清attacking；
- 新增真实`LF2Weapon.Hit`五类fixture：state1002、type4 state1002、state3000 normal、state3000→frame10 state1002
  order witness、oid209 Karasu/frame40两个skip。
- UnityMCP刷新脚本后`error CS`=0；full `BattleRuntimeSelfCheck`在2026-08-22 06:36:40 +08:00写入`PASS`；
- self-check后Console只保留`RegistrationRollbackSelfCheckEntity`与mismatched-rest-binding的两条既有negative-control
  error-level日志，未见C# compiler error或02D fixture失败。

## 禁止扩大 / 未验证

- Play Mode尚未运行；
- 不改global frame helper、02C、weapon vital/stat、RNG、candidate、CPoint/held/link、scheduler/input/AI/render/DAT；
- C++ trace仍BLOCKED，真实Play Mode仍待补；`RUNTIME_PENDING`不表示任何完整对齐结论。
