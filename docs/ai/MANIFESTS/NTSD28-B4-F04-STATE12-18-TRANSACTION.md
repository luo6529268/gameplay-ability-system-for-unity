# NTSD28 B4 F04 state12/18 transaction prerequisites

| Authority字段/职责 | Unity现状 | 结论/路由 |
|---|---|---|
| status_dx_1c0 | 无 | 新carrier，B5 producer |
| status_dy_1c4 | 无 | 新carrier，B5 producer |
| status_dz_1c8 | 无 | 新carrier，B5 producer |
| status_gain_1cc | 无 | 新carrier，soft defer/hard consume |
| status_hit_facing_1d0 | 无 | 新carrier，hard X clamp |
| status_picked_action_1d4=191 | 无 | 新carrier |
| status_picking_action_1d8=185 | 无 | 新carrier |
| environment_state_320 | `EnvironmentState320` | 复用 |
| incoming_damage_scale_340 | `IncomingDamageScale340` | 复用 |
| input_hp_consumed_total | `InputHpConsumedTotal34C` | 复用 |
| catch_source_slot_90 | `CatchSourceSlot90` | 复用，>=0x2000 decode |
| owner_slot | `OwnerSlotIndex` | 复用，最多follow两跳 |
| environment_source_slot_160 | `EnvironmentSourceSlot160` | 复用 |
| input_score_total_348 | `InputScoreTotal348` | 复用 |
| knockout_count_358 | `KnockoutCount358` | 复用 |
| knockout event feed | 无正式sink | B8结果流 |
| contact-side resolution | result仅Landed/Airborne | consumer包新增同调用栈标志 |
