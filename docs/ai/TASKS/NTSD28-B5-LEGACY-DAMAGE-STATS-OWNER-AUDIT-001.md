# Task Contract — NTSD28-B5-LEGACY-DAMAGE-STATS-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / NO_SINGLE_LEGACY_STATS_AUTHORITY / KILLCOUNT_MULTIPLEXED / EARLIER_PHASE_BINDINGS_REOPENED / EXACT_NATIVE_CARRIERS_EXIST / SEVEN_ROUTES_DEFINED / JOINT_SCHEMA_DIRECTION_GATED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-LEGACY-HOLDERCOPY-MULTIPLEXED-SLOT-OWNER-AUDIT-001`、`NTSD28-B5-UNARMORED-HP-CONSUMPTION-PRODUCTION-001`、`NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001`、`NTSD28-B6-HELD-INJURY-ACCOUNTING-OWNER-AUDIT-001`

## 目标与边界

穷尽 Unity `KillCount`、`ComboCountAtk`、`ComboCountVic`、`KillStat`、world
`DamageStats/KillStats` 的生产读写、AI、lifecycle、ECS、snapshot、checksum与诊断消费，分别映射到当前
NTSD 2.8-Logan Authority 的 exact fields。不得因为旧字段名字含 `Count/Stat` 就把不同所有权合并删除。

本Task只修改治理记录；不修改C#、Config、Scene、Prefab、资源、ProjectSettings、Server或Authority。

## 穷尽扫描基线

对 `Assets/NTSD/Scripts/**/*.cs` 扫描六个符号，共577行唯一命中：207行非test生产代码、29个生产文件，
370行test/editor命中、34个文件。逐符号行数如下；`KillStat`会命中`KillStats`，所以逐符号数字不能相加：

| 符号 | 全部行 | 生产行/文件 | test/editor行/文件 |
|---|---:|---:|---:|
| `KillCount` | 165 | 75 / 25 | 90 / 24 |
| `ComboCountAtk` | 55 | 22 / 10 | 33 / 7 |
| `ComboCountVic` | 101 | 30 / 10 | 71 / 11 |
| `KillStat` | 158 | 51 / 12 | 107 / 10 |
| `DamageStats` | 114 | 32 / 11 | 82 / 12 |
| `KillStats` | 87 | 22 / 9 | 65 / 10 |

Authority production closure中没有这六个Unity legacy名字，也没有world三格damage/kill数组。当前正式字段是：

- `ordinary_credit_gate_2f4`：ordinary KO/score、MP threshold、full-restore以及type0 OPoint child传递所用sentinel；
- `owner_slot`：伤害credit chain、AI avoid OID122/123及对象所有权；
- `input_hp_consumed_total` `+0x34C`、`input_score_total_348`、`knockout_count_358`：累计HP消耗、积分与KO；
- `combo_hit_count_1e0`、`combo_hit_last_tick_1e4`：独立native hit-count显示/expiry，不是累计伤害；
- `revive_lives_30c`、`revive_next_lives_310`、`revive_next_hp_314`、`render_phase_008`和physical slot：revival/terminal participant；
- `runtime_state_code`：11xx/12xx lifecycle code只写当前实体自身。

Unity已经有对应canonical carriers：`OrdinaryCreditGate2F4`、`OwnerSlotIndex`、
`InputHpConsumedTotal34C`、`InputScoreTotal348`、`KnockoutCount358`、
`NativeComboHitCount1E0/NativeComboHitLastTick1E4`和revival fields。因此后继是改写所有者与退休重复镜像，
不是再建一套stats abstraction。

后继`NTSD28-B0-OWNER-SLOT-PRODUCTION-OWNER-AUDIT-001`进一步确认：`OwnerSlotIndex`字段与持久化虽已存在，
formal direct/stage、ordinary OPoint和F8 producer仍不完整，且generic target仍污染它。本审计的“carrier存在”
结论保留，但不得被解释为owner production已闭合。

## `KillCount` 是multiplexed owner，不是一个统计字段

### 1. `+0x2F4`语义

- ordinary/reduced actual、HitPlan和两个legacy resolver仍以`target.KillCount == -1`决定lethal/旧score；
  Authority只读target `ordinary_credit_gate_2f4 == -1`。
- recovery real/ECS以`KillCount != -1`选择150/500 PP门槛；Authority同样只读`+0x2F4`。
- 两个Unity OPoint factory对type0 child执行
  `parent.KillCount > -1 ? parent.KillCount : parent physical slot`；这与Authority type0 child `+0x2F4`
  传播公式同形，但写错了carrier。Authority对non-type0 child没有这项`+0x2F4`写入。
- 已有B6 held-refill代码已正确读child `OrdinaryCreditGate2F4`，证明`KillCount`不能继续作为兼容别名。

### 2. AI语义

`AiSensingKernel`、legacy decision以及unified row把`KillCount > -1`当作OID122/123 avoid guard。
Authority `native_ai.cpp`明确读subject `owner_slot >= 0`。所以AI row/store必须改绑`OwnerSlotIndex`；把
OPoint `+0x2F4`值投到AI会在root、immediate owner、self-owner和non-type0对象上产生错误判断。

### 3. revival/lying语义

`LF2Character`、generic `LF2Entity`和`BattleRespawnModule`以`KillCount`、team 5或slot>=20决定dead-lying
hit-stop/respawn资格。Authority没有该gate：primary terminal hold、multi-life arm与revival分别由physical slot、
object type、state14、HP、`revive_lives_30c/revive_next_hp_314`和`render_phase_008`决定。这会重开B4 revival
binding；不能把`KillCount`机械替换成`+0x2F4`。

### 4. state501与11xx/12xx语义

- EarlyFrame与legacy CPoint transform以`child.KillCount == owner physical slot`批量替换owned child definition。
  当前Direction-B normalized projection有0个battle frame `state=501`；2.8 release decoded corpus仅
  `INKHUD.dat/INKHUD2.dat`各1个state501，当前playable battle source没有这套owned-child transform。
  因此它是current dormant legacy path，须独立退休审计，不能反推`KillCount`为Authority owner field。
- LateLifecycle对11xx/12xx next code除写当前实体外，还扫描`other.KillCount == ownerSlot`并写child HitStun。
  Authority `resolve_pending_lifecycles_range()`只把当前实体action置0并写
  `runtime_state_code=1100-code`，没有child scan。current corpus有1个直接见证：Itachi action167
  `next=1250`；Unity child传播是额外可观察写入，须独立退休。

## 其余legacy stats结论

### Entity累计字段

`ComboCountVic`被ordinary/reduced/type3/held/impact、负environment damage和input HP cost累计；
`ComboCountAtk`由HolderCopy attribution累计；`KillStat`由同一旧attribution在lethal时递增。
Authority对应写入`+0x34C/+0x348/+0x358`，而native combo `+0x1E0/+0x1E4`只按accepted hit次数与tick更新，
不能用伤害和替代。Unity actual/HitPlan多数路径已经同时写canonical值和legacy值，形成重复真相；少数compat
input/recovery路径仍只写`ComboCountVic`，退休前必须补齐exact accumulator。

### World三格数组

`BattleRuntimeState.DamageStats/KillStats`只由legacy hit/CPoint writers和两个legacy impact resolver更新；
生产消费除HitPlan shadow、自身snapshot/checksum/parity外，仅`ProductionEntityStressHarness`汇总诊断。
`BattleWorldRosterResultsSnapshotBuffer.GetDamageStat/GetKillStat`在production无调用者。Authority结果/scoreboard
从每个实体的`+0x34C/+0x348/+0x358`构造，没有world三格并行统计数组。数组应在writers迁移后退休，
不能继续进入deterministic identity。

## Persistence与schema影响

- `KillCount/ComboCountAtk/ComboCountVic/KillStat`进入runtime reset/canonical copy、entity snapshot schema12、
  full snapshot schema19、ECS Vital/Relation store、ECS fingerprint、checksum schema22和parity JSON。
- `DamageStats/KillStats`进入world reset、roster/results snapshot schema1、full snapshot schema19、checksum22
  及parity JSON。
- exact fields已进入canonical copy/checksum/parity；删除legacy carrier仍是协议可观察变化。

所以carrier删除必须等所有behavior reader/writer归零，并与ReleaseTick/WeaponState/Tracker/GrabbedBy/
HolderCopy联合取得用户schema方向。联合迁移除既有entity `12→13`、full `19→20`、checksum `22→23`外，
还必须显式包含roster/results `1→2`；不能只改前三个版本号。

## 后继路由与严格依赖

1. `NTSD28-B2-AI-OWNER-SLOT-KILLCOUNT-CORRECTION-001`：AI sensing/unified/legacy guard改读
   `OwnerSlotIndex`，移除AI row的KillCount语义；slot0/high/self/root!=owner覆盖。
2. `NTSD28-B3-LEGACY-STATE501-OWNED-CHILD-TRANSFORM-RETIREMENT-AUDIT-001`：确认0-current、HUD-only release
   与playable closure后，退休battle transform及其KillCount scan；若EXE出现反证则保持UNKNOWN并重建exact字段。
3. `NTSD28-B3-LEGACY-1100-1299-CHILD-PROPAGATION-RETIREMENT-001`：保留self
   `1100-code`，删除KillCount child scan；覆盖Itachi action167/current child sentinel。
4. `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001`：按physical slot/type/lives/queued HP/
   render phase重绑三个dead/respawn入口，不用`+0x2F4`冒充revival gate。
5. `NTSD28-B5-ORDINARY-CREDIT-GATE-2F4-PRODUCER-CONSUMER-CORRECTION-001`：actual/HitPlan/compat damage与
   PP threshold统一读`OrdinaryCreditGate2F4`；两个factory仅对type0 child写exact传播公式。
6. `NTSD28-B5-LEGACY-DAMAGE-STAT-WRITER-RETIREMENT-001`：在确认每条路径已有exact
   `+0x34C/+0x348/+0x358`或native combo写入后，原子退休actual/HitPlan/compat的entity/world旧stats；
   复用已有B6 impact与held-injury owner包，不重复实现。
7. `NTSD28-B5-LEGACY-DAMAGE-STATS-CARRIER-DISPOSITION-001 / USER_DIRECTION_REQUIRED`：动态读写归零后才删除
   field/property/store/snapshot/checksum/parity/test sentinels，并纳入联合13/20/23及roster2 schema迁移。

发现B2/B3/B4 binding差异后，严格B0→B12顺序要求先处理1～4，再继续5～7或新增B6生产改动。

## 验收矩阵

- AI：owner -1/self/slot0/high/two-hop；OID122/123选择和RNG调用数不变，仅wrong-owner case纠正。
- 11xx：Itachi action167 next1250；self runtime code/action与Authority一致，owned child所有字段bitwise不变。
- revival：primary slot0..19、transient >=20、lives 1/2、queued continuation、team5、`+0x2F4`sentinel交叉矩阵。
- `+0x2F4`：ordinary/reduced/held、lethal/nonlethal、type0/non-type0 OPoint、root!=immediate owner、slot0/high。
- stats：standard/reduced/type3/impact/held/environment/input HP cost；canonical score/KO/consumed/native combo值不变，
  legacy entity/world字段不再写。
- schema：entity/roster/full/checksum旧schema policy、snapshot restore/history/session、ECS layout/fingerprint、parity JSON、
  4096 warm copy/hash 0 B；compile、focused、SelfCheck、Play和same-seed/input/tick trace。

## 不变量与回滚

- 不改damage数值、HP/MP mutation、credit owner-chain深度、native combo、RNG、pass顺序、content、Scene、Prefab、
  shutdown或用户例外。
- 本轮只登记事实、纠正和路由；回滚只移除本Task/Change及摘要，不涉及脚本、content、Scene或Authority。
