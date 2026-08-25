# R8-LIFE-001 — oid7/8→51 dormant-slot adapter closure

> 建立日期：2026-08-23  
> 状态：`RUNTIME_PENDING / APPROVED UNITY ADAPTER / NO GAMEPLAY CHANGE`  
> D-ID：`D-LIFE-001`  
> 所属：`R8-WP01G-R02`

## Goal

重新以 C++ Release live `game_tick(...)` 为权威，闭合 oid7/8 合体时伙伴失活、原槽所有权、
oid51 拆分恢复和 reset/cleanup 的完整合同；判断 Unity `OidMergeDormant` 保留对象并占用原槽是否仍是
可观察等价且受保护的 Unity adapter。

## Scope

允许：

1. 只读核对 C++ Release `game_tick.cpp`、正式分配域和 `Entity::reset()`；
2. 只读核对 Unity OID maintenance、registry、slot table、ECS membership、query与presentation gate；
3. 运行已有 OID5152 self-check 与相关 focused EditMode tests；
4. 更新 Task、Research、STATE、register、main plan和handoff。

本合同默认不修改 gameplay。若发现 battle-time slot0..19 writer、generation/slot被提前释放、split恢复字段
或 reset 顺序不一致，必须停止本 no-code closure，并先建立独立 Change Record。

## Authority / Evidence

- C++ build：`J:/QQFile/NTSD2.4/ntsd_release/Makefile:32`；
- C++ live：`src/entity/game_tick.cpp:1017-1154`；
- C++ reset：`include/game_world.h` 的 `Entity::reset()`；
- C++ battle-time allocation：stage从20，opoint/effect/broken/random/frame-spawn从50；
- Unity：`SimulationWorld.Passes.partial.cs::Oid5152RuntimeMaintenanceAll`、
  `TryMergeOid7Or8Into51`、`TrySplitOid51BackToPair`；
- Unity registry：`SimulationWorld.Registry.partial.cs`、`RuntimeSlotTable`、`BattleEcsWorld`；
- regression：`BattleRuntimeSelfCheck` 的七组 OID5152 checks及相关 Editor test classes。

## Acceptance

1. merge成功后 active owner变oid51/frame290，伙伴从active pass/query/presentation消失，ObjectCount减一；
2. partner原slot、OID、stable identity/rest state在dormant期不被动态或stage allocator消费；
3. split按记录slot恢复伙伴，先执行 formal reset语义，再写frame112、HP/HPBound、PP、位置、速度、朝向、team；
4. odd HP整除、失败后的partial recovery、cooldown同tick边界和oid7/8镜像均有断言；
5. fresh compile为0 error，full self-check与相关focused tests PASS；
6. 没有C++ runtime trace或真实Play Mode时最高只能是`RUNTIME_PENDING`。

## Stop conditions

- 发现正式battle-time allocator可写slot0..19；
- merge期间Unity释放partner slot或推进generation；
- split不使用原记录slot，或Reset/写回顺序与C++不同；
- 需要改变容量、pool、render、pass order或任何四项范围外逻辑。

## Out of scope

C++ executable/trace、T8、IL2CPP、Android、服务器、性能架构、F1/F2 debug及其他D-ID。

## Actual result

- C++ Release live source与Unity crosswalk未发现需要修改production gameplay的差异；
- `OidMergeDormant`继续保留原slot/generation是批准的Unity安全adapter，不得为了字段形态一致而释放槽位；
- focused EditMode job `04ddfe7fa44b4f92beb0618d0f269a13`：32/32 PASS；
- 同一代码状态下16:40后的full `BattleRuntimeSelfCheck`为PASS，其中七组OID5152 checks全部实际执行；
- fresh Unity scripts compile为0 error；
- production真实Play与C++ full trace未取得，故最高状态保持`RUNTIME_PENDING`。
