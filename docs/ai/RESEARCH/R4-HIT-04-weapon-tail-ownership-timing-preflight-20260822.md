# R4-HIT-04 — normal weapon tail ownership / `HitConfirm2` and relation timing preflight

> 日期：2026-08-22  
> 状态：`SOURCE_CONTRACT_VERIFIED / PLANNED`  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\collision.cpp`；C++ Release 目录只读。

## 问题

Unity `BattleDamageWriter.ApplyWeaponDamage` 对 normal weapon victim（current DAT type `1/2/4/6`）在
`damage/vital/fall` 之前提前写入 `HitConfirm2=1`，并无条件提前复制 `RelationTeam`；随后
`ApplyKind0WeaponVictimTail` 又为同四种类型写入相同的最终字段。C++ 不存在这两次早写：它先完成
`apply_hurt` / `apply_hurt_reaction`，然后才按 victim DAT type 在 weapon tail 写入 `hit_confirm2` 与
`unk_364`（Unity relation identity）。

## Authority contract

- `collision.cpp:559-585`：type6 走 `apply_hurt_reaction`，其他 normal weapon 走 `apply_hurt`；
- `collision.cpp:587-632`：在上述调用返回后，type1、type4/6、type2 三个 tail 分支分别写
  `hit_confirm2`、frame/vrest/facing，并在各分支末尾写 `unk_364 = atk_core.unk_364`；
- C++ release `Makefile` 已列入 `src/entity/collision.cpp` 与 `src/entity/hit.cpp`，故这是 live-source
  contract，不是历史 C# 推断。

## Unity read-only mapping

- `BattleDamageWriter.ApplyWeaponDamage:198-324`：现有早写位于 `214-223`，canonical weapon tail 位于
  `322-324`；
- `ApplyKind0WeaponVictimTail:1031-1086`：type1、type4/6、type2 都在该尾部写最终
  `HitConfirm2` 与 `RelationTeam`；
- `LF2WeaponBase.TryApplyHit:642-670` 和普通 `LF2CharacterInteractionResolver:67-92` 只将当前 DAT
  type `1/2/4/6` 的 weapon target 导向 `LF2Weapon.Hit`；
- `LF2CharacterDatInteractionResolver:90-110` 对 CLR `LF2WeaponBase` 壳的分发更宽，可能把“当前 DAT
  不是 weapon”的 shell 导向 `LF2Weapon.Hit`。该可达性与正确分发不在本包裁决；不能为了 normal weapon
  timing 修复删除其兼容性 early relation fallback。

## 同 writer consumer 审计

在 early write 与 weapon tail 之间，`ApplyWeaponDamage` 只调用 damage sound、vital/stat、durability、
fall/knockback、state3000、frame/held/vrest、state1002 等局部路径。已逐一阅读
`LF2CharacterDatHitResolver:121-310` 中的 `ResolveStandardDamageKnockbackX`、`ApplyOid100KnockbackTail`、
`ApplyKnockdownHeldPairVrest`、`ApplyActiveHolderFrameDelay`、`RecordDamageEffectSound` 与
`RecordStandardHurtSounds`：它们均不读取 `HitConfirm2` 或 `RelationTeam`。本 writer 的 state3000/state1002
helper也不读取两字段。

因此，对 current DAT type `1/2/4/6`，将两字段的可观察写入推迟至已存在的 tail 不会改变此 writer 中的
中间分支判定；最终字段值继续由同一 tail 写入。

## 最小实现边界

1. 移除 `damageableWeapon` 块中的 early `HitConfirm2=1`；
2. 对 `damageableWeapon` 不再 early 写 `RelationTeam`，保留 `ApplyKind0WeaponVictimTail` 的现有四个 tail
   writer；
3. 对非 `damageableWeapon` 的现有 early `RelationTeam` fallback 保持不变，避免混入 current-DAT shell
   dispatch 差异；
4. 不改 frame/raw writer、vital/stat、durability、RNG、candidate、CPoint、held/link、scheduler、input、AI、
   render 或 C++ authority。

## Evidence classification

- **VERIFIED（source）**：C++ normal weapon tail 的写入顺序，以及 Unity normal type `1/2/4/6` tail
  的最终 writer 存在；
- **VERIFIED（static）**：中间直接 helper 没有读取两字段；
- **UNKNOWN**：shared Character-DAT resolver 将 non-weapon current DAT 的 CLR weapon shell 导向
  `LF2Weapon.Hit` 的 production reachability / C++等价分发；该项另留作后续差异，不得合并进本包；
- **VERIFIED（Unity code）**：normal `damageableWeapon` early confirm已删除、early relation只保留给non-damageable fallback；
  current type1/2/4/6仍由existing tail首次写最终字段；
- **VERIFIED（local regression）**：four real-hit fixture、Unity compile `error CS`=0、2026-08-22 07:10:20 +08:00
  full self-check PASS；
- **PENDING**：target Play Mode与C++ trace。

## First fixture correction retained

首次type2-ground失败不是production writer差异：fixture OID `998`命中`Config/data.txt:141`的type5 definition，
从而正确触发non-damageable fallback。改用existing 02C的`990 + weaponType`未定义test OID后，四正常weapon
类型均通过。该首次失败和最小fixture修复已记录在`R4-HIT-004` Change Record。
