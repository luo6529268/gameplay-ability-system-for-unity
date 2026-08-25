# HANDOFF — R4-HIT-02C normal weapon-victim raw-frame writer

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-002C`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source；未运行、构建、修改、复制或写入C++ authority。

## 已完成的最小改动与验证

- C++ `collision.cpp:583-632` 已只读确认：damageable weapon normal hit保留两次raw frame write，先是common
  knockdown的180/186，随后是type-specific final frame；不允许删去前一次；
- Unity `BattleDamageWriter.ApplyWeaponDamage`的knockdown一处、以及
  `ApplyKind0WeaponVictimTail`的type1、type4/type6、type2-ground、type2-air四处，均已由
  `LF2WeaponBase.SetFrameDirect`改为`DirectWriteRawFramePreserveWaitCounter`；
- 新增`CheckWeaponVictimRawFrameWriterContract`，通过真实
  `LF2Weapon.Hit → ApplyWeaponDamage → ApplyKind0WeaponVictimTail → RecordKind0Hit`路线覆盖type1、type4、
  type6、type2-ground、type2-air，断言final frame/Data、PN、attacking、wait、hit-confirm、relation、self-vrest和
  RNG总数；type1/4/6/2-air的总数应保留一笔final-frame RNG加两笔hit-record RNG，2-ground只保留两笔hit-record RNG。
- UnityMCP刷新脚本后`error CS`=0；full `BattleRuntimeSelfCheck`在2026-08-22 06:20:15 +08:00写入`PASS`；
- self-check后Console只保留`RegistrationRollbackSelfCheckEntity`与mismatched-rest-binding的两条既有negative-control
  error-level日志，未见C# compiler error或02C fixture失败。

## 尚未验证 / 禁止扩大

- Play Mode尚未执行；当前绝非“已对齐”；
- R1-WP02 full C++ trace仍为`BLOCKED`；C++ runtime没有运行、构建或任何写入；
- 不要修改global `SetFrameDirect`、`ImmediateFrame`、weapon attacker response、vital/stat、vrest/held、RNG、
  candidate/scheduler/input/AI/render/DAT，也不要将本包结论扩展至`R4-HIT-02D`。

## 紧接验证步骤

1. 建立`R4-HIT-02D`的独立只读source preflight，先闭合normal weapon attacker raw-frame与显式字段合同；
2. 不得在02D建立Task Contract/Change Record前修改脚本；
3. 02C的C++ trace、真实Play Mode与joint frame/presentation保持未关闭，后续仅在合法证据出现时补记。
