# R2-CANDIDATE-TAIL-01 — step-wait candidate carrier retention

> 建立日期：2026-08-23  
> 状态：`PLANNED / APPROVAL PENDING / CHANGE RECORD NOT YET CREATED`  
> D-ID：`D-SCHED-008`

## Goal

修复 `R8-WP01G-R01` 由 C++ Release source 确认的条件性差异：F1/step-wait 在 render 后跳过
entity post-frame tail时，Unity必须与 C++ 一样保留 candidate carrier到下一tick，而不是在
object consume后和下一collect开始时丢弃。

## Scope

- 建立一个明确的 candidate lifecycle state，区分“本tick真正完成tail”和“step-wait提前返回”；
- 让 normal tick 在真实 entity tail后清 carrier/归还store；
- 让 step-wait early-return 保留 ordered entries、count、selection carriers与HitConfirm2；
- 下一tick继续按 C++ count/cap/order/RNG语义收集和消费；
- 增加 focused source-contract/self-check，覆盖 normal、pause、连续pause、resume和fallback/optimized矩阵；
- 保持 warmed 0 B、现有pool/generation/capacity边界。

## Authority / Evidence

- C++唯一权威：`J:\QQFile\NTSD2.4\ntsd_release`；
- `game_tick.cpp:1648-1655,1818-1822,2067-2089`；
- `collision_collect.cpp:123-240,363-372`；
- `collision.cpp:32-47`；
- `include/game_world.h:202-213`；
- source closure：`docs/ai/RESEARCH/R8-WP01G-R01-r2-scheduler-source-reachability-20260823.md`。

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`；
- `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs`；
- `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`；
- candidate store/lifecycle最小必要模块；
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`及必要focused Editor test。

## Unknowns

- 最小安全实现应延后整个`EndCollisionCandidateConsumption`，还是保留visibility end但冻结carrier storage；
- StoreAuthority与LegacyOracle跨pause的最小共享representation；
- 连续paused tick达到20-cap后的exact RNG/tie行为；
- step-wait与results-active/NeedClearInput边界是否共享同一cleanup reason。

## Read-only implementation preflight

进一步读取`CollisionCandidateStore`、`CollisionCandidateRange`与shared candidate runner后，确认不能通过
“延后一个End调用”完整修复。推荐的最小实现是新增一个query-owned、固定slab、prebattle预分配的
`CollisionCandidateRetentionStore`：

1. **capture**：object consume后，如果本tick已知会实际跳过entity post-frame tail，在range仍有效时按attacker
   slot/generation复制count、selection fields、HitConfirm2和最多20条`SceneQueryHit`；
2. **release current adapter**：capture成功后仍可正常invalidate range/归还当前list，避免跨帧持有临时range；
3. **seed next collect**：下一tick正常建立当前producer mode的Legacy list/store row后，把同generation的
   retained rows先seed进去，再让本tickcollector按C++ current count/cap/RNG继续append；
4. **slot semantics**：target只以retained TargetSlot绑定当前occupant，generation只验证attacker row，不得把
   target generation变成consume gate；
5. **fallback**：formal失败重启brute collector时，必须先reset再从同一retention slab重新seed，不能重复append；
6. **tail clear**：真正执行entity post-frame tail时，同时清实际`HitCandidateCount`/selection fields与现有
   HitConfirm2/TransientMp，并清retention pending；
7. **capacity/GC**：slab在`PrepareBattleCapacity`阶段扩容，battle sealed后不得new/resize；Authority400、
   MobileExtended与DesktopExtended共用同一capacity合同。

`D-STEP-001` source closure补充：未来若实现A→B→C unlock，stepWait但flag1的C++ tick不会early-return，
因此不得retain carrier。实现必须依赖明确`willSkipPostFrameTail`/`didSkipPostFrameTail` predicate，而不是裸
`stepWaitGate`；当前flag未实现时该predicate才等价于stepWait。

拒绝的简化方案：

- 只把`EndCollisionCandidateConsumption`移到tail：下一collect仍会reset，且store producer mode切换/slot reuse
  没有闭合；
- 只保留旧Dictionary/List：StoreOnly模式没有Legacy list，oracle interval切换也会丢carrier；
- 只保留count：C++还保留ordered slot/itr-index arrays，count-only会改变consume ordinal；
- battle中临时分配carry list/array：违反0-GC与sealed capacity边界。

这是跨scheduler、query/store、tail与focused test的多文件修改。按根`AGENTS.md`，在用户明确批准前保持
只读，不创建Change Record、不修改脚本。

## Deliverables

1. 实施前独立 Change Record与Ledger/STATE登记；
2. normal/paused/resume一致的candidate lifecycle实现；
3. ordered carrier、cap、RNG、HitConfirm2和tail cleanup focused matrix；
4. fresh Unity compile、focused test、full `BattleRuntimeSelfCheck`；
5. warmed 0 B与Legacy/Store/fallback/optimized等价证据；
6. 更新D-ID、总计划、STATE和handoff。

## Verification

- source：C++与Unity pass/field顺序复核；
- compile：Unity scripts 0 error；
- focused：single normal、single pause、multi-pause、resume、20-cap、HitConfirm2、RNG call-count；
- joint：character/object两consume + CPoint/held后续不读取已失效carrier；
- Play Mode：若现有F1 input路径可安全复现则记录，否则明确`RUNTIME_PENDING`；
- full trace：`R1-WP02 BLOCKED`时不得伪造S5。

## Stop conditions

- 需要修改 C++、输入规则、F1 gate定义、collision/hit writer、CPoint/held/link或render；
- 需要回退candidate store/SoA/ECS/pool/generation/extended capacity/30Hz；
- warmed路径无法维持0 B且需要扩大架构；
- first difference指向R3+或长期架构决策。

## Out of scope

R3+ input/movement、R4 hit公式、R5 lifecycle、R6 render、T8、Android、IL2CPP、服务器、C++运行/构建/写入。
