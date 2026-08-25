# R4-HIT-02C — normal weapon victim raw-frame contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — Unity compile与full self-check已通过；C++ trace / Play Mode未关闭。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:583-632`。  
> 对应差异：`D-HIT-002` 第三子包；关联 Change ID：`R4-HIT-002C`。

## Goal

让 normal kind0 weapon victim的type1、type4/type6和type2 frame write遵循C++ raw-write合同，不让
`LF2WeaponBase.SetFrameDirect`隐式清attacking或以目标frame覆盖wait。保留原有HitConfirm2、vrest、facing、
relation team和随机数顺序。

## Scope

仅允许修改 `BattleDamageWriter.ApplyWeaponDamage` 的knockdown frame callsite与
`BattleDamageWriter.ApplyKind0WeaponVictimTail` 的四个 `SetFrameDirect` callsite（合计五处），及
`BattleRuntimeSelfCheck` 中的focused weapon-victim fixture。fixture必须覆盖type1、type4、type6、type2-ground和
type2-air五个分支；禁止改global helper、attacker response、weapon
vital/stat、RNG engine、candidate/scheduler/CPoint/held/link/opoint/AI/render/DAT/C++ authority。

## Required behavior

1. type1、type4/type6：保留raw knockdown frame后raw random `0..15`，各恰好一次frame RNG；PN/attacking/wait保留；
2. type2 ground：保留raw knockdown frame后frame20，零frame RNG；PN/attacking/wait保留；
3. type2 random：保留raw knockdown frame后random `0..5`，恰好一次frame RNG；PN/attacking/wait保留；
4. existing hit-confirm/vrest/team/facing合同不回归；不新增alloc或RNG调用。

## Verification

| 层级 | 验收 |
|---|---|
| S0 | C++ `collision.cpp:583-632` 与Unity tail复核。 |
| S1 | 五个current-DAT branch fixture验证raw side effects和RNG count。 |
| S2 | Unity compile=0 error、full self-check PASS、ledger/diff check通过。 |
| S3 | 最高`RUNTIME_PENDING`；C++ trace / Play Mode保持待补。 |

## 本次实际验证

- S0：`PASS`，仅只读核对C++ `collision.cpp:583-632`；未运行、构建、复制或写入authority；
- S1：`PASS`，新增的真实`LF2Weapon.Hit → ApplyWeaponDamage → ApplyKind0WeaponVictimTail → RecordKind0Hit`
  夹具已随full self-check执行，覆盖type1、type4、type6、type2-ground、type2-air；
- S2：`PASS`，已打开Unity Editor通过UnityMCP刷新脚本后`error CS`=0；
  `Temp/NTSD_BattleRuntimeSelfCheck.result`于2026-08-22 06:20:15 +08:00为`PASS`；
  `Tools/Validate-ChangeLedger.ps1`通过，`git diff --check`退出0（仅既有CRLF提示）；
- Console：self-check后仅有两条现有rest-binding negative-control日志，未见C# compiler error或02C fixture失败。

## Stop conditions / Out of scope

需要修改global writer、attacker response、vital/stat、CPoint/held、RNG engine或scope外模块时停止该包；
`R4-HIT-02D`、`D-HIT-003`、R5～R8、T8、C++ executable、Unity Play Mode和性能不在范围内。
