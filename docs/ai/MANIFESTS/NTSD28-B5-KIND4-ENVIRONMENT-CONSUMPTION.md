# NTSD28 B5 Kind4 Environment Consumption

## Authority chain

- `battle_world.h`：`environment_state_320` 是独立signed scalar；`kind4_source_count_92` 是与低word
  `catch_source_slot_90` 共享native dword的高word16位计数，standalone model必须分字段防止互相破坏。
- `battle_world.cpp` candidate selection：每个通过早期filter的几何kind4，在 `environment_state_320 > 0` 时先将
  `kind4_source_count_92` 按16位递增，再进入低fall与multiple/nearest选择。
- `resolve_standard_damage_interaction`：kind4只在攻击者 `environment_state_320 > 0` 时转kind0；运动方向与朝向相反时反转dvx。
- `resolve_native_standard_hit_attribution`：计数非零时，以 `catch_source_slot_90` 的低16位重定向伤害来源。
- 普通伤害尾部：实际结算后若计数非零则递减一次。

## Unity aligned state

- `NTSDEntityRuntime.EnvironmentState320` 已存在并参与既有carrier治理。
- candidate producer按每个几何重叠BDY、在low-fall/select之前递增独立 `Kind4SourceCount92`。
- `BruteForceSceneQuery`、HitPlan及两个direct character-hit fallback统一读取 `EnvironmentState320 > 0`，并保持dvx反转与heavy-held gate同源。
- 普通伤害以count低16位决定是否从 `(ushort)CatchSourceSlot90` 重定向，再走最多两层owner；type0积分写最终credit，成功damage尾部消耗一次count。
- HitPlan snapshot/project/DifferenceMask覆盖count及最终credit score；focused shadow为0。
- `WeaponCount` 有独立旧用途，不能改名、复用或与 `EnvironmentState320` 双写。
- `AcceptReleaseSelectFlagCandidate` 中最后一条行为中性的kind4/WeaponCount旧赋值已由 `NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001` 退役，并由source guard防止恢复。

## 分包边界

carrier与atomic consumer已分别验证；下一做kind4-specific exit audit，再回到B5 remaining audit。
`EnvironmentState320` 的cpoint正式producer属于B6，现阶段程序化值只验证B5 consumer，不声称B6已完成。
