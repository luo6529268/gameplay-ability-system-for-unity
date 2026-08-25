# D-LIFE-001 — oid7/8→51 dormant、slot、split与cleanup复核

> 日期：2026-08-23  
> 状态：`RUNTIME_PENDING / APPROVED UNITY ADAPTER`  
> Authority：C++ Release live source（只读）

## 1. 当前结论

source、Unity静态复核与fresh自动回归均未发现需要修改production gameplay的差异。Unity没有把合体伙伴真正释放回
slot allocator，而是以`OidMergeDormant`保留同一对象、同一slot和generation；这与C++将该slot中的Entity写
`active=false`、但在拆分前不允许battle-time allocator消费0..19槽位的可观察行为等价。

本轮focused 32/32与full self-check PASS，因此已收口到`RUNTIME_PENDING`；真实production Play与C++ full
trace未取得，不能写runtime `VERIFIED`。

## 2. C++ Release live contract

- `Makefile:32`确认`src/entity/game_tick.cpp`进入release build；
- `game_tick.cpp:1017-1094`只扫描slot0..19，要求self slot<10；成功时记录partner slot到`unk_32C`，
  写`unk_330/334/338`，切换owner为oid51/frame290，并将`partner.active=false`、`object_count--`；
- dormant partner没有执行`free_entity`，其槽位和残留外部rest矩阵并未作为普通free被释放；
- `game_tick.cpp:1098-1154`从`unk_32C`读取同一partner slot，执行`Entity::reset()`，恢复原OID、frame112、
  half HP/HPMax、PP0、位置、速度、相反朝向与relation，并`object_count++`；
- 当前release live battle中stage动态生成从20开始，opoint/effect/broken/random/frame-spawn从50开始；
  slot0起的`GameWorld::spawn`调用仅属于battle bootstrap/character-select，不在merge→split战斗中间态运行。

## 3. Unity crosswalk

| 合同 | Unity实现 | 判定 |
|---|---|---|
| low-slot升序maintenance | `Oid5152RuntimeMaintenanceAll`扫描0..19，跳过dormant/pending | 等价 |
| partner失活 | `OidMergeDormant=true`；active query、ECS membership、ObjectCount、presentation均排除 | 等价adapter |
| partner slot所有权 | slot table不release，partner保留原slot/generation | 等价且防止错误复用 |
| split定位 | `Unk32C` + `FindEntityByRuntimeSlotIncludingDormant` | 等价 |
| reset再写回 | `partner.Reset()`后补formal Entity reset默认，再写OID/frame112/half vital/PP0/position/velocity/facing/relation | 等价 |
| external arest/vrest | reset周围使用preserve-state边界 | 对应C++未调用`reset_cooldowns` |
| partial recovery | owner identity/cooldown先写；invalid partner slot后停止，不写frame112/half vital | 对应C++顺序 |

## 4. 结构差异为什么不构成当前行为差异

C++的inactive partner仍驻留在固定数组slot，只是该slot看起来可被一般“寻找inactive槽”逻辑选中；但本条
merge partner只能在0..19，而正式战斗中的stage/dynamic allocator最低分别为20/50。Unity显式保留claim，
使同一约束由slot table表达。两者在当前live battle调用图上的结果相同：拆分总能回到原partner slot，且不会
被opoint、效果、武器或stage spawn覆盖。

## 5. 重开条件

正式battle-time出现slot0..19 allocator、partner允许slot20+、dormant期间generation推进、old object finalization
影响同槽实体，或未来C++ trace/真实Play显示ObjectCount、slot、reset字段不同，任一成立均重新登记独立Change。

## 6. Fresh evidence

- Unity scripts compile：0 error，Console error entries为0；
- full `BattleRuntimeSelfCheck`：PASS；其入口实际执行七组OID5152 merge/split checks；
- focused EditMode job `04ddfe7fa44b4f92beb0618d0f269a13`：32/32 PASS，覆盖dormant ECS membership、
  presentation reuse、cooldown skip与opoint/slot generation lifecycle；
- 本项production/test脚本修改：0；
- global Change Ledger validator仍被任务外`WEB-CADENCE-001`的non-governed/unrecorded diff阻塞，未擅自修复。
