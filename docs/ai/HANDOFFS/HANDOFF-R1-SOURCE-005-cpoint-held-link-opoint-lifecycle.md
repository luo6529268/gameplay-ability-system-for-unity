# HANDOFF — R1-SOURCE-005 CPoint / held / link / opoint / 生命周期

> 交接日期：2026-08-21  
> 状态：COMPLETED（静态 source inventory）  
> 不代表：C++ executable trace、Unity 编译、BattleRuntimeSelfCheck、Play Mode、性能验收或 gameplay 已对齐。

## 1. 本包完成范围

已只读检查 C++ Release live source：

- src/entity/game_tick.cpp；
- src/entity/cpoint.cpp；
- src/entity/weapon.cpp；
- src/entity/frame_advance.cpp；
- src/entity/collision.cpp；
- include/game_world.h 与 entity runtime views。

并已映射 Unity：

- NTSDBattleTickSystem；
- SimulationWorld.Passes / Registry / SimulationQueryAndLinkModule；
- BattleCpointWriter、BattleHeldObjectWriter、BattleEcsPositiveLinkValidationPass、
  BattleStructuralWriter；
- BattleLogicObjectPointRuntime / BattleLogicEntityFactory；
- LF2Entity、LF2Character、LF2WeaponBase 与 relation/held resolver。

未修改 C++、Unity gameplay、测试、资源、场景或配置；未运行任何 executable、Unity build、
self-check、Play Mode、trace 或性能测试。

## 2. 已完成的 C++ source 合同

1. C++ 有两轮完整的 negative-link held scan：
   game_tick.cpp:1441-1643 与 1860-2018；两者都是升序 full-slot scan。
2. CPoint 主检查读取 prev_frame2；current CPoint sync 是独立的 weapon sync pass；
   kind 2 validation 又是独立的第二轮。
3. positive link 仅以 target.active + target.holder_idx 验证，并且失效时只清
   holder.link_state。
4. normal late DAT opoint 在 candidate、character/object consume 和 render handoff 后发生，
   C++ child 从 slot 50 起按最低空槽分配；高于 current cursor 的 child 可加入本轮 late
   scan，低于 cursor 的 child 要等下一轮。
5. C++ free_entity 立即去 active，但完整 Entity.reset 在下一次占用 slot 时发生；Unity 的
   PendingFlushDestroy / generation / pool 是必须保持可观察等价性的 adapter。

详见：

- docs/ai/RESEARCH/R1-SOURCE-005-cpp-cpoint-held-link-opoint-lifecycle-contract.md
- docs/ai/RESEARCH/R1-SOURCE-005-unity-crosswalk-and-diff.md

## 3. 新登记或升级的静态差异

| ID | 结论 | 后续最小验收 |
|---|---|---|
| D-SCHED-004 | C++ 两轮 negative-held；Unity 一轮 HeldProcess。 | first/second held mutation fixture。 |
| D-LINK-001 | invalid positive link 的 Unity cleanup 比 C++ 多清 target / held 字段。 | stale target / held fixture。 |
| D-LINK-002 | invalid negative link 的 Unity cleanup 比 C++ 多清 holder 字段。 | invalid child 后 holder consumer fixture。 |
| D-HOLD-001 | type2 held throw 的 Unity FrameDelay=1 没有 C++ 同 branch write。 | type2 throw tick / next-tick fixture。 |
| D-HOLD-002 | type2 held throw 的 Unity SpawnerEntityIndex write 没有 C++ 同 branch write。 | type2 spawner consumer fixture。 |
| D-CPT-001 | CPoint raw frame / wait writer 不同。 | broken relation、action、duration expiry fixture。 |
| D-CPT-002 | CPoint injury 的 Unity global kill/damage stat 写入缺口。 | lethal / non-lethal stat fixture。 |
| D-OP-001 | normal opoint child 的 initial Prev2 字段不同。 | nonzero action child history fixture。 |

这些都是 source-level difference；它们尚未被 C++ runtime evidence 或 Unity joint fixture 验证为
具体用户可见现象，也没有任何一项被修复。

## 4. 显式转交的未知项

- kind2 opoint 的 Unity TrackerFlag / TrackerParent 与 C++ live source 的具体辅助字段、
  kind5 consumer 尚未闭合；
- PendingFlushDestroy / generation / pool 与 C++ active=false + next-reset 的 slot reuse、
  newborn visibility 只有静态 adapter mapping；
- 需要区分 normal late DAT opoint 与 frame-logic hit_Fa / special-object queue 的专项 spawn；
- D-MOV-004 ThrowFrameGuard 在当前 Unity production source 未发现非测试 writer，仍是 dormant
  gate reachability 问题，不能直接删除；
- CentralOnly 中央表现的首帧可见性、layer/order、shadow handoff 交给 SOURCE-006，不回退
  Legacy SpriteRenderer。

## 5. 推荐下一包

**R1-SOURCE-006 — C++ render handoff / Unity central presentation observable contract。**

它必须以 C++ renderer.cpp 的 battle handoff 为 authority，审计：

1. 何时读取 battle logical snapshot；
2. entity / shadow / effect 的位置、layer、顺序与 visibility 规则；
3. normal late opoint child 的 first visible render boundary；
4. CentralOnly / Texture2DArray / dynamic Mesh / URP 如何作为 Unity adapter 保留，而不回退
   legacy production SpriteRenderer；
5. render 只读 logic snapshot，不反写 simulation truth 的证据。

完成 SOURCE-006 后，才允许 SOURCE-007 汇总所有差异、依赖图与分层验收矩阵。

## 6. 持续边界

- C++ authority 继续严格只读：不运行、重建、复制、插桩、hook、patch，且不向其目录写任何文件；
- R1-WP02 full trace 继续 BLOCKED；
- R2、任何 gameplay 修复、Unity compile / self-check / Play Mode、性能测试仍未开始；
- 保持 CentralOnly、Texture2DArray、dynamic Mesh、URP、MobileExtended/DesktopExtended、
  30 Hz、FrameInputSet、SoA/ECS、pool、worker 与 zero-GC 目标；
- T8 默认 stage.dat 持续暂缓。

