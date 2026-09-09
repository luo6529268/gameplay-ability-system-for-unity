# Task Contract — NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-CARRIER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / THREE_PACKAGE_SPLIT_DEFINED / PRODUCTION_HELD`
> 依赖：`NTSD28-B6-WPOINT-DVX-WEAPON-HP-AUDIT-001 / VERIFIED`

## 目标

冻结held DVX release写入的native `Entity28+0x2F8`身份、默认值、writer/consumer集合，判断
Unity现有`SpawnerSlotIndex`能否作为同一carrier，并为后续确定性接线拆包。

## Authority闭环

- `EntityState28::object_ai_excluded_group_source_slot_2f8`默认`-1`。
- 当前playable closure内唯一writer是
  `BattleWorld28::settle_held_refill_objects()`：仅type1/4/6且WPoint `dvx!=0`时写holder
  physical slot；type2、kind3-only、spawn、respawn和普通lifecycle均不写该字段。
- 唯一consumer是`native_ai.cpp` common non-character target path：若+0x2F8指向active entity，
  读取其`battle_group`并排除同组cached/nearest type0 target。它独立于+0x354 owner/credit及
  +0x3F8 cached target。

## Unity现状与结论

- `NTSDEntityRuntime.SpawnerSlotIndex`默认`-1`，held type1/4/6 DVX当前确实写holder slot；但它
  同时被respawn effect、late lifecycle及其他general-spawner路径写入，并被spawn/weapon逻辑、
  ECS identity、snapshot/checksum/parity作为通用spawner身份消费。
- Unity当前没有独立+0x2F8 runtime field；native AI kernel/SoA也没有该字段。旧
  `LF2WeaponFrameLogicResolver`以`SpawnerEntityIndex`排除team，只能说明旧实现曾混用相似语义，
  不能覆盖新权威唯一writer集合和C02 consumer时点。
- 因writer集合不同，`SpawnerSlotIndex`不能晋级为+0x2F8绑定；继续复用会让非held生成者错误
  参与excluded-group选择，也无法独立验证type2 preserve与slot reuse。
- 当前Unity Config有3条non-kind3 DVX WPoint，release有599条；正式held type1/4/6后续AI选择
  可达该字段，不是纯diagnostic carrier。

## 三包拆分

1. `NTSD28-B6-OBJECT-AI-EXCLUDED-GROUP-CARRIER-001`：新增独立runtime field，闭合
   default/full reset/copy/ECS/snapshot/checksum/full parity/raw projection与slot-reuse sentinel；
   不接writer/consumer。
2. `NTSD28-B6-WPOINT-DVX-EXCLUDED-GROUP-WRITER-001`：type1/4/6 DVX写holder slot，type2和
   其他release preserve；同步real/generic actual，停止以Spawner作为+0x2F8 writer。
3. `NTSD28-B6-OBJECT-AI-EXCLUDED-GROUP-CONSUMER-001`：在当前native non-character AI的正式
   owner/时点读取新field，active source才排除battle group；cached/nearest、invalid/stale source
   和physical slot reuse需focused及joint trace。

consumer包开始前须单独确认Unity当前C02与legacy weapon frame path的唯一生产owner；不得简单
把字段塞进character-only `AiDecisionKernel`。carrier包也不得顺手重命名/删除通用Spawner身份。

## 不变量 / 阻塞

- 不改kind3、weapon HP、DVX motion/action、owner/credit、+0x3F8、content或Scene。
- 现有B6 runtime绿灯缺失；三包均保持held，不叠加行为或schema代码。

## 回滚

仅移除治理记录；没有代码、内容或Scene回滚。

## Current corpus correction（2026-09-08）

current non-kind3 DVX由3纠正为holder-primary184；+0x2F8独立carrier/writer/AI consumer三包不变，current
source/consumer矩阵扩大。详见`NTSD28-B6-DIRECTION-B-MULTILINE-CORPUS-CORRECTION-AUDIT-001`。

## Held relation producer-domain correction（2026-09-08）

完整ITR+OPoint held union的current non-kind3 DVX为186；原184是pickup-only。独立+0x2F8
carrier→writer→AI consumer三包与字段排除结论不变，matrix使用186。

## +0x3F8 target-owner dependency（2026-09-08）

`NTSD28-B6-OBJECT-AI-TARGET-3F8-OWNER-AUDIT-001`确认Unity generic hit_Fa当前把target cache误写进
+0x354 OwnerSlot，而specialized OID124才使用可复用的`PickerStableId` storage。故本Task第三包
`OBJECT-AI-EXCLUDED-GROUP-CONSUMER-001`必须等待+0x3F8 carrier semantic与common/child producer integration；
接+0x2F8时不得再次改写owner/target或继续复用Spawner。
