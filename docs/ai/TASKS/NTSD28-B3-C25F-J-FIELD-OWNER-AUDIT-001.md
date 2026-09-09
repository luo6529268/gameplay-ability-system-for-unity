# Task Contract — NTSD28-B3-C25F-J-FIELD-OWNER-AUDIT-001

> 状态：`VERIFIED / GOVERNANCE_ONLY / FIELD_OWNER_MATRIX_COMPLETE / NEXT_RENDER_PHASE_BINDING`
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 C25f-j`
> 依赖：`NTSD28-B3-C25C-E-ENTITY-CARRIERS-001 / VERIFIED`

## 目标

闭合当前Authority C25f-j的逐字段、逐gate、逐顺序事实，映射Unity已有carrier/writer/schema/content依赖，拆出不会把legacy frame/recovery/timer行为误认成2.8规则的实施包。

## 只读范围

- Authority `simulation_tick_driver.cpp` C25f-j、`BattleWorld28::step_frame_slot`、`advance_reaction_timers_slot`、`advance_armor_recovery_slot`、`decrement_attacker_rest_slot`及直接字段/producer/consumer。
- Unity C25 dynamic late owner、frame tick/ECS writer、reaction/status carrier、armor DAT/runtime、attacker rest、snapshot/checksum/parity及相关tests。
- 仅新增/更新本Task、Record、Ledger、STATE、handoff、总表与C25f-j manifest；不修改C#、Config/DAT、Scene/Prefab、ProjectSettings或Authority。

## 不变量

- C25f→g→h→i→j必须在同一current-occupant slot transaction内保持顺序；高slot newborn同tick、低slot newborn下tick语义不变。
- held/relation skip只冻结Authority明确属于`FUN_0040D000` body的字段；无条件`FUN_004503C5` timer bank不得被一并冻结。
- type3 exception、positive-HP status gate、poison damage时点、armor reload和attacker rest gate必须分别记录，不能合并为通用Cooldown。
- 任何DAT armor/content依赖只记录并路由B11；H项策略未决定前禁止写内容或schema。

## 验收

- C25f-j每项都有Authority字段、gate/order、Unity现状、缺失项和实施路由。
- 明确哪些字段可先独立加carrier，哪些算法可不依赖B11实施，哪些必须等待B5/B10/B11。
- 明确legacy serial/ECS double-writer和现有`AttackExempt--`的所有权风险。
- Authority保持只读，validator通过。

## 回滚

删除本治理包的Task/Record/manifest引用即可；不得触及已验证C25a-b、C25c-e carriers或current_mp纠正。

## 完成证据

- 闭合Authority C25f-j caller和五个slot writer，并核对直接字段、producer/consumer及core tests。
- 核对Unity dynamic late loop、exact-character/fallback frame writer、release counters、OID timer、legacy bdefend recovery、AttackExempt mirror和raw null armor投影。
- 内容只读计数：Authority state7xxx=0、armor block=18（6块hp1、recover均未声明）、poison itr=28、delay itr=16；Unity对应Config/schema均0。
- 发现render-phase绑定冲突、Bdefend速率差、十字段bank/positive-HP status缺owner、armor carrier/content缺失，并拆成五个后续包。
- 详细矩阵：`docs/ai/MANIFESTS/NTSD28-B3-C25F-J-FIELD-OWNER-INVENTORY.md`；未修改C#、content、Scene或Authority。
