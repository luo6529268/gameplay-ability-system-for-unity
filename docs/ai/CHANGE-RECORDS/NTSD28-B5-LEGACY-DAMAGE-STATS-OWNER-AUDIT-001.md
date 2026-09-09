# NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan EntityState28 ordinary_credit_gate_2f4, owner_slot, input_hp_consumed_total +0x34C, input_score_total_348, knockout_count_358, combo_hit_count_1e0/combo_hit_last_tick_1e4, revival/lifecycle fields and playable battle_world/native_ai/render consumers; Unity KillCount/ComboCountAtk/Vic/KillStat/DamageStats/KillStats production, AI, ECS, snapshot/checksum/parity closure; EXE B1E13AE1, closure 39DDDA15.
evidence: six-symbol exhaustive scan found 577 unique code-line references, 207 production across 29 files and 370 test/editor; KillCount proven multiplexed across +0x2F4, owner_slot, revival and legacy child propagation; AI reads wrong owner field; three revival gates and 11xx child propagation are non-authority bindings; current state501 count zero and release matches HUD-only; current Itachi action167 next1250 witnesses 11xx reachability; exact score/KO/consumed/native-combo carriers already exist; world arrays have no authority counterpart and no production result consumer beyond diagnostic/shadow/persistence; seven routes plus joint entity12/roster1/full19/checksum22 schema disposition frozen; no code/content/Scene changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_LEGACY_STATS_AUTHORITY / KILLCOUNT_MULTIPLEXED / EARLIER_PHASE_BINDINGS_REOPENED / EXACT_NATIVE_CARRIERS_EXIST / SEVEN_ROUTES_DEFINED / JOINT_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`

`KillCount`不是Authority字段：Unity把`+0x2F4`、AI `owner_slot`、revival资格、state501 owned-child与
11xx/12xx child传播混在同一carrier。Authority exact累计字段是`+0x34C/+0x348/+0x358`，native combo另有
`+0x1E0/+0x1E4`；`ComboCountAtk/Vic`、`KillStat`和world三格数组都是旧并行真相。

扫描得到577行，生产207行/29文件。AI owner、B3 legacy child路径、B4 revival、B5 `+0x2F4`和stats writers
必须依序拆分；B6 impact/held复用已有owner。动态读写归零后，carrier删除加入entity12→13、roster1→2、
full19→20、checksum22→23联合schema方向。详见同ID Task Contract。本轮无code/content/Scene。

## 2026-09-09 route progress

route1～4与HolderCopy前置更正已闭合；route5
`NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001`也已验证完成：damage/recovery只读
exact +0x2F4、type0 OPoint只写exact传播且non-type0不写。下一严格route为
`NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-001`；carrier/schema disposition仍须用户方向。

2026-09-09 readiness更正：全量writer retirement仍被standard/reduced exact KO、B6 held accounting、input compat
与negative recovery阻塞；详见
`NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-READINESS-AUDIT-001`。下一严格包先补standard/reduced KO，
不得把旧stats删除当成exact替代已存在。
