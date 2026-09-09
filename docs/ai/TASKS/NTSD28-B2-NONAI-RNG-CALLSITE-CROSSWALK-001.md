# Task Contract — NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001

> 状态：`VERIFIED / FRESH-50-SYNC-PLUS-2-CRT-CLOSED / DOWNSTREAM-OWNERS-ROUTED / NEXT-FUNCTION-KEY-CROSSWALK`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2 / NON-AI RNG`  
> 建立日期：2026-09-04

## 目标

闭合NTSD 2.8-Logan playable live closure中除native AI外的50个synchronized RNG调用表达式与2个direct
CRT调用：记录函数、call-site、bound、严格条件/短路顺序、同tick顺序、可观察副作用、Unity候选与最终owner
阶段。产出实现拆包，不能仅按文本数量机械替换Unity `world.Rng`。

## Authority 与当前缺口

- 2026-09-04 fresh authority重计：`battle_world.cpp`32、`input_routing.cpp`2、
  `simulation_tick_driver.cpp`1、`game_session.cpp`15，共50个非AI synchronized文本表达式；另有
  `battle_world.cpp`2个direct CRT jitter。旧source-audit的29/1/1/12=43口径已被本次重计纠正。
- 已迁移：direct-battle BGM pre-draw、input action sites0x82/0x83/0x84、native AI live call-sites。
- 未迁移：battle-world hit/spawn/revival/particles、tick-driver drop、GameSession mode/stage/selection等其余owner；
  Unity NativeRandom生产搜索目前只显示bootstrap、AI commit和input action writer。
- 用户保留Unity当前随机武器掉落；其额外RNG必须继续与NTSD28NativeRandom隔离。

## 允许操作

- 只读authority `battle_world.cpp`、`simulation_tick_driver.cpp`、`input_routing.cpp`、`game_session.cpp`及调用链；
  只读Unity RNG消费者和相关测试/记录。
- 新建/更新`docs/ai/MANIFESTS/NTSD28-B2-NONAI-RNG-LIVE-CALLS.md`及本Task、Record、Ledger、STATE、
  handoff、总表。
- `code-path: NONE`；不得修改C#/C++/tool source、Config/DAT、Scene/Prefab、ProjectSettings、Packages或authority。
- 发现实施项后另立Task/Change，禁止在本审计包直接替换RNG调用。

## 验收

- 50个sync表达式和2个CRT表达式逐项入表，无遗漏/重复；动态call-site和zero-bound no-consume必须明确。
- 每项标注当前是否已迁移、用户例外、或归B3/B4/B5/B7/B8/B10/B11；需要B2先建的owner/seam单独列出。
- Unity旧source-audit记录的37个通用RNG候选必须fresh重计并按live owner分类；legacy/exception/重复路径
  不得冒充authority call-site。
- 给出按依赖排序的最小production拆包和第一个实施包验收；validator通过。

## 回滚

仅删除/回退本次manifest与治理路由；production和authority零修改。

## 完成证据

- `docs/ai/MANIFESTS/NTSD28-B2-NONAI-RNG-LIVE-CALLS.md`逐项列出BW-01～32、IN-01～02、
  TD-01、GS-01～15及CRT-01～02，50+2无遗漏；动态site、循环次数、bound1仍消费均明确。
- fresh Unity非Test/非AI候选为87个语法命中（含1个方法声明，实际调用候选86）；已按legacy/optimized、
  user exception、domain owner分类，未按数量机械配对。
- IN-01/02与GS-15已由B2生产迁移；TD-01正式0x92消费未实现，且不能被用户保留的legacy掉落替代。
- BW/GS/TD其余调用分别路由至B3～B8/B11/B12；在pass和single writer未对齐前不得把旧路径直接切到
  NativeRandom。B2下一独立阻断为function-key route/gate crosswalk。
