<!-- CHANGE-RECORD
id: NTSD28-Q06-LATE-OPOINT-DEPTH-AND-LIVES-001
status: VERIFIED
change-kind: LATE_OPOINT_DEPTH_AND_BIRTH_LIVES
code-path: Assets/NTSD/Scripts/Simulation/Runtime/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleSpawnVitalsWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06OpointDepthLivesEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: ObjectSpawnPlanner28::plan_frame integer parent.position.z + point.z + 1; BattleWorld28.spawn_at EntityState28 defaults 30C=1/310=0/314=0.
evidence: C25L formal192 combined full-tick comparison has only ordinary OPoint slot50 preciseZ202.5 versus202 and reviveLives0 versus1; source fields and callers traced.
-->

# 普通late OPoint深度和生存计数初始化

精确四脚本、三生产路径。两late position caller改为Runtime.ZInt + opoint.Z + 1的整数结果同时作为初始整数/精确Z；X/Y及motion/facing/多生成/队伍不改。BattleSpawnVitalsWriter.Apply补generic原出生reviveLives1/queuedLives0/queuedHp0，不在HP setter或每tick重置。既有HP/MP/百分比/display初值职责保持，nativeWeaponPieceSpawn旁路已有独立generic初始化不改变。

已有实际formal RED（46885e...）48普通OPoint子向量仅两字段共96差异；先加6个正负父fractionalZ/非零point.Z聚焦RED，再实施。编译、focused、formal192及OPoint vitals/关闭回归；全部C25L联验由父任务继续。不是普通OPoint所有字段已对齐：Dvz/team/multi-spawn精度/高action及render factory普通路径World绑定继续由reader/birth合同后继核验，不以本两项关闭整个生成体系。

复用已快照/校验的Runtime.HP2Orig/HPOrig/RespawnCount，无schema字段增删或语义重命名，不改Unity/GAS/非战斗/Scene/资源/Server。无新增manager/queue/关闭阶段，测试finally按现有World停止与清理；回滚只本差量且先授权，保存用户HUDBg x30/场景哈希。

RED6/6（eca9f824df8a4b5193447cff4c63913b）已实际捕获：非零opoint.Z完全未消费，Z沿父fractional值；正式组合另证HP2Orig0/1差异。开始精确三生产路径修复。

定向17/17 PASS（0062a9974ae24c258a27d813be6e148c，13.5548935秒）：正式192比对全部0差异，18显式delay、保留slot/池失败/Stopping与原owner4、Z/lives6均通过。完整SelfCheck与Play、原大矩阵后继，当前尚非完整VERIFIED。

事前追加第五路径BattleRuntimeSelfCheck.RunAudit7LateOpointPrecisionCase：完整SelfCheck新鲜FAIL仅因旧fixture要求preserve double Z+1，当前source明确整数Z+point.Z+1；改期望用producer.Runtime.ZInt+1（fixture point.Z=0）并保留X/Y/owner/velocity/next-tick全部断言，补reviveLives=1。原失败保留，生产不回退。

正式Play96/96通过（实际Logan999/两factory/完整tick，prototype父与777明确覆写）；随机逐调用和raw47字段一致，Scene checksum保持、Renderer2→2。probe资源前置用现有诊断API为本次峰值24准备26 total，逻辑slot400不变、无配置/资产变更。最终大矩阵/回归/关机验证继续，尚不推广整个父frame或Q06已对齐。

最终回归正在同一job aa6b0c9f033e4b8b83174826936e782c执行；尚不关闭。current-test-progress.json记录当前48 chunk进度；最终SelfCheck（绑定修复后）需紧接其终态运行。

最终联合82中58PASS/24FAIL；完整3021向量执行，direct1507零差异；待处理pending motion/physics上游首差及C17例外guard范围。继续IN_PROGRESS，不能以此前17/Play96关闭整体验收。证据final-regression-82.xml/current-test-progress。

## 最终限定出口（2026-09-14 03:23Z）

VERIFIED / LATE_Z_AND_GENERIC_BIRTH_BASELINE_ONLY。实际证据按改动职责使用，不推广整个B或Q06完成：

- source state18 1550行、formal96，以及weaponHp210重复一致；原正式EXE SHA B1E13AE…9033与75源码manifest07CD47…778F保持。
- 原82测试58PASS/24FAIL；旧non-RNG276实际含252 pending位置和24 weaponHp（此前全归pending的说法已纠正）。pending原6中4FAIL→6PASS，补源实体位置/速度/current/latch/previous/collision/state/pending/code后仍通过；weaponHp原10FAIL→全绿。
- job5a104f86aa1147a3921e27253a5fcc8f：24组22PASS/2FAIL，1514向量只剩weaponHp；修复后jobce0f7a50c7524df990c290e4d9a12831实际30/30 PASS，含weapon10（两caller420原向量、7type真实池复用、手工fallback）、pending6、原SpawnVitals12、两失败chunk5。24组最新合计1514向量0差异/legacy546次恰为批准例外；22组来自前一终态、2组来自修复后，不能称同一run24全绿。direct1507原通过且不受普通OPoint初始化差量影响；full-matrix-final-validation.json记录来源。
- 最终真实Play：pending12含新增源状态；weaponHp84（7type×kind1/2×pointHp负/零/正×两actual factory）；正式Logan999组合96（两factory、原raw47/RNG）。全部PASS，Scene checksum保持，每组Renderer借用2→2。各目录Play-final-pass.json。synthetic父/777及旧内容Scene边界已声明，非图片/物理按键全验收。
- Shutdown-final-pass.json：4→4恢复、World/slots/logic/render borrower全0，两帧Stopped并正常退出。最后SelfCheck请求03:21:56.9990036Z、结果03:22:37Z PASS，归weaponHp目录SelfCheck-final-pass.result。CS error0；Editor idle非Play/非compiling，Scene dirtyfalse/root14/SHA BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6保持。
- Ledger与diff-check已通过（历史未出现在diff的声明路径WARNING不作失败），交付前再运行。无正式资源/Scene/InputActions/非战斗/Unity-GAS/Server/Gen/Plugins修改，无computer-use、commit/push或用户工作清理。

后继边界：普通OPoint的effect/reserve/join/join_reserve/join_pic及type0/5-parent credit门、defend等明确仍由OPoint remaining consumer Task承担；generic出生1/0/0不是最终所有OPoint复活字段。当前frame父事务仍需高动作/成本fallback连续完整driver与真实Play联合，不能用端点2676或本组合覆盖代替。其后回原reader/display-post/Q07。15/23/26/2/2、raw47/3及全部已批准例外保持。
