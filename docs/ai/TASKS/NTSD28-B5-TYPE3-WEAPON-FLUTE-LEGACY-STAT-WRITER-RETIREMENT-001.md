# Task Contract — NTSD28-B5-TYPE3-WEAPON-FLUTE-LEGACY-STAT-WRITER-RETIREMENT-001

> 状态：`VERIFIED / RED_0_OF_4 / FOCUSED_4_OF_4 / RELATED_B5_777_OF_777 / TARGETED_PLAY_4_CASES / LIVE_COLLISION_MATRIX_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / SCOPED_LEGACY_STATS_RETIRED / INPUT_COMPAT_AUDIT_NEXT`
> 依赖：`NTSD28-B5-STANDARD-REDUCED-KNOCKOUT-PRODUCER-001 / VERIFIED`、
> `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001 / VERIFIED`。

## 目标

退休三个已具备安全边界的legacy stat writer family：type3/other normal hurt与weapon type1/2/4仅保留
Authority exact HP/HPBound/InputHpConsumed；kind10/11 flute仅保留weapon-count、frame、motion/sound等Authority
行为。actual、HitPlan shadow与两套flute compatibility实现必须同步，不改变standard/reduced或held CPoint。

## Authority 与现状

- Authority type3/weapon普通hurt会写vitals与`input_hp_consumed_total_34c`，但不写Unity
  `ComboCountVic`或world `DamageStats`。
- Authority kind10/11 flute分支写weapon-count/动作/运动等，没有`ComboCountAtk += 11`或
  `DamageStats += 11`；Unity concrete/shared actual和HitPlan均有额外镜像。
- Unity type3/weapon exact HP consumed已由`NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001`验证；
  本包删除legacy镜像不会丢失正式累计。

## 允许修改

- `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`
- 新focused test、必要的既有HitPlan/tests/SelfCheck断言。
- 本Task/Change与治理恢复文档。

## 不变量与排除

- type3/other与weapon仍以同一effective injury写HP、HPBound、InputHpConsumed、display、motion、rest、sound。
- flute的accept/reject、WeaponCount、frame182、motion factor、tick/step gate不变；只移除legacy stats。
- standard/reduced的legacy Combo/Kill/world stats继续保留，CPoint/input/recovery完全不动。
- fields、ECS stores、snapshots、checksum/parity与world arrays本身不删，schema仍归route7。
- 不改content、Scene、Prefab、ProjectSettings、Authority、pass order或用户例外。

## Test-first验收

1. type3/other正伤害后exact HP consumed递增，legacy victim/world sentinels保持。
2. weapon type1/2/4同样保持legacy sentinels；type0 standard仍证明legacy writer存在。
3. kind10/11 concrete/shared/HitPlan保持WeaponCount/frame/motion且legacy holder/world sentinels不变。
4. source closure锁定三族实际与projection写入归零，并明确standard/reduced/CPoint排除项仍存在。
5. focused、HitPlan/B5相关、build、full SelfCheck、真实Play及Scene不变。

## 回滚

仅恢复本包type3/weapon/flute legacy assignments与fixture；不得回退exact HP/KO/+0x2F4 producer。
