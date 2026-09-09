# NTSD28 B5 Weapon Durability Attacking Injury

## Authority chain

- `battle_world.cpp::resolve_confirmed_standard_hit` 在armor/defense分流前通过 `native_attacking_injury28` 计算一次effective attacking injury。
- 正的definition `stats.attacking` 优先；否则使用正的active-mode `+0x1C`；两者均不正或raw injury为0时保留raw值；乘法保留低32位后signed `/100`。
- unarmored `0x0042EA45` 与selected-armor/guarded `0x0043002E` 对target type 1/2/4/6都从 `weapon_hp_31c` 扣该effective值；raw `bdefend == 100` 独立强制写 `-1`。

## Unity aligned state

- `BattleOrdinaryCharacterDamageRouteResolver.ResolveNativeAttackingInjury` 已有相同算术，并已供type1 armor match使用。
- `BattleDamageWriter.ApplyWeaponDamage` 与 `BattleEcsHitExecutionPlan.ProjectStandardObjectDamageWriterEffect` 统一通过共享resolver扣effective attacking injury。
- raw `bdefend==100`仍最终写 `-1`；HP/display/status保持各自原有规则。
- target type 1/2/4/6的正式weapon route均会观察该差异；type0/3/5的HP damage规则不属于此耐久字段差异。

## 下一步

下一做weapon-durability专项exit audit，再回到B5 remaining audit。不改变正式content、Scene或hit-resource MP transaction。
