# NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001 — 非AI双RNG生产调用交叉

<!-- CHANGE-RECORD
id: NTSD28-B2-NONAI-RNG-CALLSITE-CROSSWALK-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan playable live non-AI synchronized/direct-CRT RNG closure in battle_world.cpp, input_routing.cpp, simulation_tick_driver.cpp and game_session.cpp.
evidence: TASK-CONTRACT-CREATED / FRESH-AUTHORITY-RECOUNT-BATTLE-WORLD-32-INPUT-2-TICK-1-GAMESESSION-15 / AUTHORITY-NONAI-SYNC-50-TEXT-EXPRESSIONS / OLD-43-INVENTORY-CORRECTED / AUTHORITY-DIRECT-CRT-2 / BW-01-32-IN-01-02-TD-01-GS-01-15-CRT-01-02-MANIFESTED / DYNAMIC-SITE-LOOP-BOUND1-CONTRACTS / UNITY-NONTEST-NONAI-87-SYNTAX-HITS-86-CALL-CANDIDATES / LEGACY-OPTIMIZED-EXCEPTION-CLASSIFIED / INPUT-AND-BGM-MIGRATED / FORMAL-RNG92-MISSING / B3-B8-B11-B12-OWNERS-ROUTED / NEXT-FUNCTION-KEY-CROSSWALK / AUTHORITY-READ-ONLY / NO-PRODUCTION-CHANGE
-->

> 状态：`VERIFIED / FRESH-50-SYNC-PLUS-2-CRT-CLOSED / DOWNSTREAM-OWNERS-ROUTED / NEXT-FUNCTION-KEY-CROSSWALK`

## 计划

- 逐调用读取所在函数与完整分支，不根据call-site数值猜用途。
- 分离已迁移input/bootstrap、用户随机武器例外、B3+下游行为与真正需要B2先建的RNG owner/seam。
- 读取Unity全部生产通用RNG表达式，建立authority↔Unity候选与unmatched/surplus分类。
- 输出manifest、实施拆分和唯一下一包。

## 当前事实

- fresh authority重计为非AI synchronized表达式共50个（32+2+1+15），另有2个direct CRT；旧43口径
  已纠正。当前Unity NativeRandom生产调用没有
  battle/stage/lifecycle consumer。
- input0x82/83/84和direct-battle BGM已由前序B2包迁移，不重复实施。
- 随机武器掉落是用户例外，但必须与正式NativeRandom隔离；Unity当前继续使用legacy `world.Rng`。

## 审计结果

- fresh权威清单已按文本逐项闭合：battle-world32、input2、tick-driver1、GameSession15，合计50；
  direct CRT2。完整条件、顺序、动态site、循环次数、副作用与owner见manifest。
- Unity fresh非Test/非AI搜索得到87个语法命中，其中1个是`BattleRandInt`声明；86个调用候选含多套
  legacy/ECS writer和用户随机武器例外，不能直接切流。
- 已迁移仅为input0x82/83/84与direct-battle BGM；AI另由专项闭合。normal-drop formal 0x92、hit、held、
  revival、stage、story、F8和lifecycle均按B3～B8 owner路由。
- 用户保留Unity随机武器对象效果不等于删除authority 0x92消费；该消费需在B3 pass边界与B8 mode gate
  producer就绪后接入，并与legacy exception stream隔离。
- 下一个不依赖B3行为的B2阻断是完整function-key route/reject crosswalk。

## Git / 交接

- production代码：零修改。
- authority：只读。
- validator：`PASSED / Records 152 / governed code files 104`；本包`code-path: NONE`。
