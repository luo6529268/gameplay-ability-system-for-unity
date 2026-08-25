# R6-PRES-05 — hit-record render writeback 时点预检

> 日期：2026-08-22  
> 状态：`IN_PROGRESS`（脚本修改前）  
> 对应：`D-RENDER-002`  
> Change ID：`R6-PRES-005`

## 1. C++ authority

- `game_tick.cpp:2061-2083` 在PreFrame/stage后调用release render callback，返回后才进入
  FramePostProcess和LateEntityUpdate；
- `renderer.cpp:687-758` 对每个active entity的hit record按slot升序：
  - age可解析为spark pic时，在blit后`hit_record_damage[i]++`；
  - age不可解析且`i == hit_record_count-1`时只递减tail count；
  - 非tail的不可解析record保留且age不增长；
  - 无spark surface时整个owner不推进；
- release entity source中，除renderer外只有collision kind0 writer读取`hit_record_count < 10`并追加，
  且成功追加会消费两次全局`ntsd_rand()`。

因此writeback虽是spark表现生命周期，却会通过“下一个tick是否满10槽、是否消费RNG”间接影响确定性；
不能仅因lockstep core checksum暂时排除hit records就把它视为无关字段。

## 2. Unity current timing

- `BattlePresentationCoordinator.BeginFrameCore`在RenderDispatch冻结cycle/frame；
- 非worker production直到`SimulationTickDriver.LateUpdate`才调用
  `FinalizePublishedHitRecordCycle`；worker则在presentation acknowledgement后调用；
- local automatic policy每个Unity Update最多1 tick，普通单机场景通常在下一tick前得到LateUpdate，
  因而“碰巧next-tick等价”；
- `BattleLockstepSession.TryRestoreAndReplay`默认`buildPresentation=false`并可在一个调用中连续推进；
  Manual/显式step也可在LateUpdate前推进下一tick；CentralOnly no-publication path不会建立新cycle。

confirmed difference：连续显式tick或no-publication replay可让hit-record age/count少推进，满10槽时改变
后续AddHitRecord及RNG消费。

## 3. 最小修复设计

不移动任何battle pass，只把C++ renderer-side writeback放回现有`RenderDispatch`方法内部：

1. scheduler已经capture当前tick presentation时，立即调用existing
   `FinalizePublishedHitRecordCycle(world)`，使用冻结sample/handle/count保护应用一次并释放cycle lease；
2. CentralOnly `buildPresentation=false`没有新cycle时，新增no-allocation direct lifecycle方法，使用sealed
   `RuntimeDataCatalog.HitRecordLifecycleCatalog`扫描当前presentation-active实体并执行同一age/tail规则；
3. LateUpdate/worker acknowledgement保留existing finalizer作为幂等fallback；已在RenderDispatch应用的cycle
   后续调用返回false，不会重复推进；
4. common spark/lifecycle unavailable时不推进，对齐C++ `!spark_surf_` early return。

不改变command snapshot（仍冻结advance前age）、GPU submit、spark sprite选择、RNG实现、candidate/hit writer、
lockstep checksum schema或pass顺序。

## 4. Focused fixture

使用age `[0,5,38,39]`：

- no-publication tick后应为count3 / `[1,5,39]`；证明valid advance、invalid non-tail保留、invalid tail
  单次移除；
- 下一worker-publication tick冻结`[1,5,39]`，RenderDispatch返回后live应为count2 / `[2,5]`；
- 再调用Late/finalizer不得二次推进；
- lifecycle unavailable control保持count/age不变；
- publication snapshot必须仍保留advance前age，证明表现读到C++本次blit使用的age；
- battle checksum/RNG不由本fixture额外改写。

## 5. Evidence boundary

需要fresh compile、full self-check与ledger validator。真实PlayMode/C++ trace仍待，最高
`RUNTIME_PENDING`。若实现需要修改lockstep checksum、worker ownership、GPU resource lifetime或pass order，
立即停止并重新评估，不在本包扩大。

