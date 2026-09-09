# Task Contract — NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / WEAPONCOUNT_WRONG_CARRIER / EXACT_TRANSACTION_REQUIRED / TWO_PACKAGE_ROUTE`
> 依赖：`NTSD28-B5-INPUT-HP-COST-COMPAT-SHARED-TRANSACTION-PRODUCTION-001 / VERIFIED`。

> **2026-09-09 correction：** `NTSD28-B5-NEGATIVE-ENVIRONMENT-CLAMP-CORRECTION-AUDIT-001`
> 已用无漂移 playable source SHA `EB37E8EC...CCB1` 与source tests证实尾部会把HP/HPBound clamp到0。
> 本文下方“no clamp/不clamp”仅保留为原审计历史，已被 supersede；其余transaction合同继续有效。

## 目标

裁决两套`WeaponCount < 0` periodic recovery及`ComboCountVic += 9`的Authority owner，冻结数据先行的
后继包。本审计只读，不修改脚本、content、Scene、Prefab、ProjectSettings或Authority。

## Authority

- playable closure中的`simulation_tick_driver.cpp:931-994`先推进resource phase，再按slot在display前调用
  `advance_native_resources_pre_display_slot`。
- `battle_world.cpp:2224-2275`只在`environment_state_320 < 0 && resource_phase_12 == 0`执行；不读取
  weapon count，也不受Unity step-wait gate。
- rule damage读取active rule `+0x90`，非正值fallback 9；当前正式`GameSession28`未覆盖
  `ResourceSystemRules28`默认9，但nondefault语义仍已确认，不能硬编码成旧字段名。
- actual damage默认等于rule；`IncomingDamageScale340>0`时为固定`900/scale`，不是
  `rule*100/scale`也不是`FallDamageDiv`。
- credit从`CatchSourceSlot90`解码0x2000后最多跟随两次`OwnerSlotIndex`；lethal前置时credit
  `KnockoutCount358++`。victim HP减actual、HPBound减`actual/3`、
  `InputHpConsumedTotal34C += ruleDamage`，credit `InputScoreTotal348 += actualDamage`。
  不写ComboCountVic，不clamp HP/HPBound，不把EnvironmentState320改正值。

## Unity差异与可达性

- `LF2Entity.RunPreCollisionRecoveryPhase`与`BattleEcsCharacterRecoveryPass.ApplyAuthorityRecovery`均以
  `WeaponCount<0`、tickIndex%12、step-wait、FallDamageDiv、clamp与ComboVic实现同一旧分支；两套均错误。
- 当前flute kind10/11会正式写`WeaponCount=-20`，因此错误分支在现有内容可达，会每12 tick造成伪伤害；
  这不是可保留的dead compatibility。
- Unity已有exact `EnvironmentState320/CatchSourceSlot90/ImpactSourceSlot164/IncomingDamageScale340`、
  OwnerSlot、+0x34C/+0x348/+0x358与NativeResourcePhase12，也已有B4两跳credit实现。
- world rules缺`NegativeEnvironmentDamage90`；B6 kind10/11/17/18负environment producer尚未实施，
  但consumer可以synthetic验证，且先移除WeaponCount false positive不依赖该producer。

## 后继顺序

1. `NTSD28-B5-NEGATIVE-ENVIRONMENT-RULE-CARRIER-001`：向既有
   `NTSD28HitResourceRulesRuntimeState`追加default9字段，并完整更新reset/core snapshot/restore/checksum/
   parity与0B测试；不接content producer。
2. `NTSD28-B5-NEGATIVE-ENVIRONMENT-RECOVERY-PRODUCTION-001`：抽单一transaction供legacy/data-oriented
   recovery调用，读取native phase和rule carrier，退休两处WeaponCount/FallDamageDiv/ComboVic writer，
   覆盖scale、credit、KO、slot reuse与flute negative WeaponCount control。
3. B6 impact producer仍独立；B8 KO event feed与legacy schema deletion继续后置。

## 回滚

仅移除本治理记录；后继代码各按独立Change回滚。
