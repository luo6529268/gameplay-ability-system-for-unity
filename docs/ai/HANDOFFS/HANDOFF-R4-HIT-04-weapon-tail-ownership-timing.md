# HANDOFF — R4-HIT-04 normal weapon tail ownership timing

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> Change ID：`R4-HIT-004`  
> 权威：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:559-632`；C++ authority未运行、构建、修改、复制或写入。

## 已闭合的 source contract

- C++ normal weapon path先完成 `apply_hurt` / `apply_hurt_reaction`，随后type1/type4/6/type2 tail写
  `hit_confirm2` 与 `unk_364`；
- Unity common `ApplyWeaponDamage` 原本提前写同两字段，weapon tail 又重复写；
- normal type1/2/4/6从early point到tail的已读 helper均不读取这两个字段；
- shared Character-DAT resolver将非weapon current-DAT CLR weapon shell导向`LF2Weapon.Hit`的路径仍为独立
  `UNKNOWN`，未混入本包。

## 已写入的最小改动

- `BattleDamageWriter.ApplyWeaponDamage` 删除 normal `damageableWeapon` 的early `HitConfirm2=1`；
- 同一writer仅对非damageable current-DAT保留existing early `RelationTeam` fallback；normal type1/2/4/6由
  现有`ApplyKind0WeaponVictimTail`首次写最终relation；
- `BattleRuntimeSelfCheck` 增加 `CheckWeaponTailIdentityTimingContract`，以真实`LF2Weapon.Hit`覆盖type1、type2
  ground、type4、type6，锁定tail后的confirm/relation、frame mirror、PN、attacking、wait、durability与RNG。

## 必须继续的验证

1. `Tools/Validate-ChangeLedger.ps1`；
2. `git diff --check`；
3. 已打开Unity Editor的scripts refresh、`error CS`=0；
4. full `BattleRuntimeSelfCheck`；
5. 将实际时间戳/结果更新回Record、Ledger、STATE、main plan和full diff register。

若compile或self-check失败，保持`CODE_WRITTEN`并记录first failure；不得改current-DAT dispatch或C++ authority来绕过。

## 首次验证结果（2026-08-22 07:06:16 +08:00）

- UnityMCP scripts refresh后的filtered `error CS`=0；
- full self-check `FAIL`，first failure为new type2-ground fixture：`confirm=0`、`relation=23`、`frame=0/0`、
  `flight=100`、`rng=2`；
- 已定位为fixture OID collision：new type2 OID998命中`Config/data.txt:141`的type5 definition，故走
  non-damageable fallback；02C的`990 + weaponType` test OID没有此catalog override。下一步只改new fixture OID，
  不改production writer/dispatcher；随后重跑完整验证。

## 最终本地验证

- 修正fixture OID后，UnityMCP scripts refresh `error CS`=0；
- `Temp/NTSD_BattleRuntimeSelfCheck.result`在2026-08-22 07:10:20 +08:00为`PASS`；
- Console仅有两个既有rest-binding negative-control error-level日志；
- 保持`RUNTIME_PENDING`：C++ full trace BLOCKED，target Play Mode未做，non-weapon current-DAT weapon-shell
  dispatch保持独立UNKNOWN。

## 未关闭 / 禁止扩大

- C++ trace仍由R1-WP02阻塞；Play Mode未做；
- 不改`LF2CharacterDatInteractionResolver`、`LF2WeaponBase`、raw frame、vital/stat、durability、CPoint、held/link、
  candidate、scheduler/input/AI/render/DAT/scene/resource；
- `RUNTIME_PENDING`是最高允许结果，不得称完整weapon/R4/C++对齐。
