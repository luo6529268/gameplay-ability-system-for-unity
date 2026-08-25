# R4-HIT-02D — normal weapon-attacker raw-frame and ordering contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — Unity compile与full self-check已通过；C++ trace / Play Mode未关闭。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\hit.cpp:342-361,465-482`（release `Makefile:19`）。  
> 对应差异：`D-HIT-002`第四子包；关联 Change ID：`R4-HIT-002D`。

## Goal

使normal kind0 weapon victim route中的attacker state3000/state1002 writer同时符合C++的raw-frame副作用、
skipReset与相对顺序；不将state3000的显式attacking清零误扩展到state1002。

## Scope

仅允许修改`Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs`的
`ApplyWeaponDamage`及其现有`ApplyWeaponAttackerResponse`局部拆分，以及
`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`的focused fixture。

禁止修改global frame helper、weapon-victim writer、weapon vital/stat、RNG engine、candidate、CPoint、held/link、
scheduler、input、AI、render、DAT、C++ authority或其他R4包。

## Required behavior

1. state3000在generic weapon-victim knockdown前检查，并以current DAT oid/type与victim frame判断oid209 skipReset；
2. state3000正常分支raw写frame10、保留PN/wait/Vy，并显式写attacking=0、Vx=0、Vz=frame10.dvz；
3. state1002在arest/vrest/holder之后、weapon-victim tail之前raw写random16，保留PN/attacking/wait，写Vx/Vy与type4 knockback；
4. 不新增RNG或alloc，不改变02C的weapon-victim raw-frame/RNG合同；
5. fixture覆盖state1002、state3000 normal、state3000→frame10 state1002 order witness、oid209 Karasu skip和oid209/frame40 skip。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | 只读复核C++ `hit.cpp:342-361,465-482`及Makefile参与性。 |
| S1 | 五类真实`LF2Weapon.Hit`fixture覆盖frame/Data、PN、attacking、wait、Vx/Vy/Vz、skip和RNG。 |
| S2 | Unity compile=0 error、full self-check PASS、ledger/diff check通过。 |
| S3 | 最高`RUNTIME_PENDING`；C++ trace / Play Mode仍待补。 |

## 本次实际验证

- S0：`PASS`，只读复核C++ `hit.cpp:342-361,465-482`及release `Makefile:19`；未运行、构建、复制或写入authority；
- S1：`PASS`，新增真实`LF2Weapon.Hit → ApplyWeaponDamage`夹具已覆盖五类required category（state1002含type4
  subcase、state3000 normal、order witness、oid209 Karasu skip、oid209/frame40 skip）；
- S2：`PASS`，已打开Unity Editor通过UnityMCP刷新脚本后`error CS`=0；
  `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 06:36:40 +08:00为`PASS`；
  `Tools/Validate-ChangeLedger.ps1`与`git diff --check`在写后审计中通过（后者仅既有CRLF提示）；
- Console：self-check后仅有两条既有rest-binding negative-control日志，未见C# compiler error或02D fixture失败。

## Stop conditions / Out of scope

若source contract要求改global helper、02C、vital/stat、CPoint/held/link、pass ordering、RNG engine或scope外模块，
停止本包并记录first difference；R5～R8、T8、C++ executable、Unity Play Mode、性能和服务器不在范围内。
