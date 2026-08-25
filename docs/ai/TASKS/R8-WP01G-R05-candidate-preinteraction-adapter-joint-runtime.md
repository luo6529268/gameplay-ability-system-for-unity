# R8-WP01G-R05 — candidate / PreInteraction adapter joint runtime certification

> 建立日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE UNITY EVIDENCE / C++ FULL TRACE BLOCKED`  
> D-ID：`D-SCHED-007`、`D-PERF-001`

## Goal

以C++ Release只读source为唯一规则，确认Unity为性能增加的CollisionSnapshot、PairVRest、candidate
store/broadphase与PreInteraction no-op fast path，在相同逻辑世界和tick下不改变candidate内容/顺序、
RNG、consume、damage、CPoint/held/link或生命周期结果，并保持warmed 0 B。

本包先验证、不预设修复。发现production first difference后必须停止对应fixture，另建独立修复
Task/Change Record；不得在R05中顺手修改candidate或PreInteraction production。

## Fixed order

1. `D-SCHED-007` candidate adapter；
2. `D-PERF-001` PreInteraction no-op fast path。

未证明candidate producer/consume等价前，不运行会依赖该结果裁决的fast-path完成结论。

## Authority / evidence

- C++ `src/entity/game_tick.cpp::game_tick(...)`：snapshot不存在，candidate collect与两类consume顺序；
- C++ `src/entity/collision_collect.cpp`、`collision.cpp`：slot scan、candidate accept/order/cap/RNG/vrest；
- Unity `BruteForceSceneQuery`、`SimulationWorld` collision snapshot/pair-vrest/candidate passes；
- Unity `BattleHitCandidateSequenceRunner`与character/object consume；
- Unity PreInteraction proof/report、CPoint/held/link writers；
- 现有self-check、Editor tests和WP01C-03/04 Play报告只能作为当前Unity证据，不能单独定义C++规则。

## Scope

### 允许

1. 只读核对C++ release live path与Unity当前production路径；
2. 盘点并复用现有LegacyOracle/current store、brute/role-aware collector和PreInteraction proof诊断；
3. 在相同frozen world/snapshot下比较candidate key、ordinal、attacker/target generation、selection、
   HitConfirm2、vrest、RNG state/calls和consume结果；
4. 验证neutral no-op、frame change、CPoint change、positive/negative link、holder、active/dormant/pending、
   same-slot new-generation等PreInteraction矩阵；
5. 运行fresh compile、focused EditMode、full self-check与必要真实Play；
6. 若现有工具缺联合证据，获本包批准后仍须先建独立test-only Change Record，才能新增Editor probe。

### 禁止

- 不修改、运行、构建或写入C++ authority；
- 不改变candidate cap、pass order、collector默认模式、RNG、vrest或consume；
- 不关闭worker、30Hz、FrameInputSet、SoA/ECS、CentralOnly、扩展容量或0-GC边界；
- 不直接写candidate store、hit result、CPoint/link/runtime制造PASS；
- 不处理AI、F1/F2/F7/F8/F9 debug、P1/P2、OID51 merge/split、render G4、T8、IL2CPP、Android或服务器。

## D-SCHED-007 acceptance

- same input world下legacy oracle与current formal producer的candidate count、ordered key与ordinal相同；
- 20-cap边界、nearest/tie、invalid generation、fallback与role-aware/direct/tree边界相同；
- RNG before/after/call count相同；
- character/object consume顺序、abort、HitConfirm2、vrest和damage/stat相同；
- exception/fallback后不保留半成品carrier；cleanup restored。

## D-PERF-001 acceptance

- 真正neutral输入可以跳过且结果等于full path；
- frame/CPoint/link/holder/role/liveness/generation任一变化都不得误skip；
- same-slot new generation不能复用旧proof；
- fast path开/关后的candidate/hit/HP/stat/RNG/hash相同；
- warmed proof和应用路径0 B；capacity不增长；cleanup restored。

## Deliverables

1. `docs/ai/RESEARCH/R8-WP01G-R05-candidate-preinteraction-joint-evidence-20260823.md`；
2. 必要的Temp结构化报告；
3. 更新all-diff register、STATE、总计划和handoff；
4. 若发现first difference：独立最小修复Work Package，不在R05中直接改production。

## Stop conditions

- C++ source顺序或字段无法闭合；
- first difference指向production；
- 需要新增脚本但尚未创建Change Record；
- 需要改变pass order、collector、RNG、capacity、worker或受保护adapter；
- 现有scene/DAT无法提供必要witness且无法用不改production的测试夹具闭合；
- mismatch只存在于C++ full trace层，而R1-WP02仍BLOCKED。

## Out of scope

AI、debug function keys、P1/P2、merge/split、central render G4、不可达exact DAT分支、C++ executable/full
trace、T8、IL2CPP、Android、服务器。

## Completion evidence

- research：`docs/ai/RESEARCH/R8-WP01G-R05-candidate-preinteraction-joint-evidence-20260823.md`；
- candidate focused 9/9 + 58/58 + consume 185/185；PreInteraction 15/15；
- current/forced-legacy 50-AI均SmokePassed、20项hash全等、zero-GC与cleanup PASS；
- collision/hit与grab/CPoint live Play PASS；
- fresh stress Editor 256/256、self-check 18:35:05 PASS、Console0、ledger 80/94 PASS；
- 没有production gameplay修改；唯一脚本Change `R8-CANDSTORE-DIAG-001`为test-harness validator修正。

## Authorization

用户于2026-08-23明确批准：`批准执行 R8-WP01G-R05，恢复目标`。
