# R4-HIT-04 — normal weapon tail ownership timing contract

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING` — Unity compile与full self-check通过；C++ trace / Play Mode未关闭。  
> 对应差异：`D-HIT-004`  
> Change ID：`R4-HIT-004`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp:559-632`。

## Goal

使 Unity normal weapon victim 的 `HitConfirm2` / relation identity 首次可观察写入时点与 C++ type tail 一致，
同时保留已经批准的 current-DAT shell 兼容边界。

## Scope

仅允许修改：

- `Assets/NTSD/Scripts/Simulation/Ecs/BattleDamageWriter.cs` 的 `ApplyWeaponDamage`；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 中一个只覆盖 type1/type2/type4/type6 正常 weapon
  hit tail contract 的 focused fixture。

只对 `damageableWeapon`（current DAT type `1/2/4/6`）延后两字段写入；non-damageable current-DAT fallback
必须保持原样。

## Required behavior

1. type1/type2/type4/type6 在 `ApplyKind0WeaponVictimTail` 前不得由 common writer 写 `HitConfirm2`；
2. 同四类在该尾部各自写最终 `HitConfirm2=1` 和 attacker relation；
3. existing frame, PN, wait, attacking, vrest, durability, vital/stat、RNG和`RecordKind0Hit`契约不改变；
4. 当前 DAT 非 `1/2/4/6` 的 CLR weapon shell 不在本包重分发、不删除其 existing relation fallback。

## Authority / Evidence

- `collision.cpp:559-585`：normal hurt/reaction 先完成；
- `collision.cpp:587-632`：type tail 后写 `hit_confirm2` / `unk_364`；
- `docs/ai/RESEARCH/R4-HIT-04-weapon-tail-ownership-timing-preflight-20260822.md`：Unity call/order/read audit；
- C++ trace remains `BLOCKED` under R1-WP02; no C++ executable, build, source edit, configuration or authority write is allowed.

## Verification

| 层级 | 验收 |
|---|---|
| S0 | Re-read the C++ tail and verify Makefile participation; no authority mutation. |
| S1 | Real `LF2Weapon.Hit` fixture for type1/type2/type4/type6: final `HitConfirm2`, relation, raw frame contract, PN/wait/attacking, durability and RNG remain expected. |
| S2 | `Tools/Validate-ChangeLedger.ps1`, `git diff --check`, Unity script compile `error CS`=0, full `BattleRuntimeSelfCheck` PASS. |
| S3 | Maximum status `RUNTIME_PENDING`; C++ trace and targeted Play Mode remain unclosed. |

## Stop conditions

Stop and record rather than broaden if any of the following is needed:

- modify `LF2CharacterDatInteractionResolver`, `LF2WeaponBase`, generic relation helper or non-weapon current-DAT dispatch;
- modify raw-frame, vital/stat, durability, CPoint, held/link, candidate, scheduler, input, AI, render, DAT/scene/resource;
- C++ source contract becomes ambiguous, Unity compile/self-check fails, or this change exposes a consumer outside the stated writer.

## Out of scope

R1-WP02 trace acquisition, all C++ runtime execution, R5+ modules, T8 default `stage.dat`, performance, server/lockstep, and Play Mode validation.

## 本次实际验证

- S0：`PASS`，只读复核authority `collision.cpp:559-632`和release Makefile参与性；未触碰C++ authority；
- S1：`PASS`，真实 `LF2Weapon.Hit` type1/type2-ground/type4/type6 matrix锁定tail final confirm/relation、
  frame mirror、PN、attacking、wait、raw durability与RNG；
- S2：`PASS`，UnityMCP scripts refresh后`error CS`=0，full `BattleRuntimeSelfCheck`在2026-08-22
  07:10:20 +08:00写入`PASS`；console只保留两个既有rest-binding negative-control日志；
- first fixture failure保留：最初使用oid998，命中`Config/data.txt:141` type5定义而走non-damageable fallback；
  改为existing 02C的`990 + weaponType`无catalog-override test OID后通过；
- S3：保持`RUNTIME_PENDING`，C++ trace与目标Play Mode均未运行。
