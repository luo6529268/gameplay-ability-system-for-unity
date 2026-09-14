<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001
status: FOCUSED_TEST_PASS
change-kind: NATIVE_DISPLAY_PROGRESSION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeDisplayWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeDisplayEditorTests.cs
authority: Formal Logan BattleWorld28::advance_native_display_values_slot, spawn_at and SimulationTickDriver28 C25; source witness980 and post2379.
evidence: DISPLAY-POST-DISPLAY audit source vectors and current birth/late slot caller read; Unity RED pending
-->

# C25d完整显示推进

IN_PROGRESS，当前准确四脚本阶段：新增无状态BattleNativeDisplayWriter.Advance(NTSDEntityRuntime)，四int显示字段按原函数方向/独立step更新，不clamp跨越且不反写HP/累计真值。现有字段/Reset/copy/schema保持，无新服务/queue/关闭职责。

World只新增当前slot显示查询适配：读取当前occupant，排除已注销/待Unregister和OidMergeDormant（Unity不存在于native有效slot的shell），允许PendingFlushDestroy占槽对象执行display。不得放宽全局active predicate或任何其他C25分支。LateEntityLifecycle原slot循环中：正常recovery之后、RefreshNativeComputerState之前调用；既有早退若当前slot仍为可显示的pending实体，只执行显示并保留原continue，不运行其他tail；每slot至多一次。无新全局遍历、无脚本执行顺序更改。

新增Editor测试：980源向量逐值、四不同目标/step、0/负step、两tick跨越修正、无真值写入/分配；真实late production preHP→display→frame顺序，pending对象显示但不frame、不timer；dormant/已注销不显示；不同type/缺frame，以及checksum恢复。先RED后实现，相关C25 timer/HP/MP/selfcheck回归。真实Play程序注入需恢复checksum并既有有序关闭。

完整任务尚包含出生初值。本次查到新硬依赖：两OPoint PostInitLiving忽略point.hp/mp与stats.ohp/omp，仅固定OID5/52，当前Other还以weapon_hp作HP。原版materialize_frame_spawns通过完整出生资源计算产生SpawnRequest.hp，再设置display初值。必须另立原子出生资源Task/Record并核对普通bootstrap、clone与复用，不仅复制Unity旧HP或在setter同步，也不能因本阶段递推通过关闭完整任务。当前Record不授权修改出生/资源脚本；扩展准确范围后才实施。

风险：原pending循环可见性、display过早/重复、误复用HP资格、误接同方向字段、restore遗漏；以原函数/实际loop测试和快照检查约束。回滚须批准仅本四文件差量；保留已验HP/MP、所有非战斗/Unity-GAS/Scene/资源及用户HUDBg x30，13/21/24/2/2和十一阶段不变，禁止computer-use。出生依赖处理后仍需post-display，不关闭Q06/目标。

## RED与实际实现

新鲜Unity RED job a5b754a410eb4660b01be35136b863f7：10项9FAIL/1PASS，失败为writer缺失、pending显示9未到13、生产帧入口display0未到107；red-results.xml已保存。现准确四脚本已写：无状态四字段writer、World专用slot筛选、late原循环三个早退显示处理与正常显示插入、新Editor测试。未调整任何非显示字段或其他tail资格。正在编译及focused；完整出生初始化/OPoint资源依赖尚未处理，不能关闭本Task。

首轮focused job e95fe82ab9314b78af430a44d9132116为10/10 PASS（含980向量），编译CS0。已补两个非type0/缺frame生产用例及pendingUnregister/empty slot用例，共13显示测试；同一已声明Editor文件添加两tick真实Scene请求探针，程序注入独立四字段后验证越界/下一tick修正、真值保持、finally checksum恢复，不改Scene。当前相关合并回归job 5a7a3ed17dce44c49a0d3dece7815fd0仍运行；出生及post-display未完成。

合并181项结束：177PASS/4FAIL；13个新display全PASS，HP/MP/实际Logan/回放通过。四失败由独立C25-POISON-PHASE-FIXTURE（原source World阶段）及C25-DISPLAY-SCHEMA-FIXTURE（Q05旧版本）记录修正；不能把原181写成单次全绿。待定向6项复验与SelfCheck/Play。

当前181不同测试均有通过证据：原177PASS及独立6PASS覆盖4旧FAIL，merged-test-evidence.json逐fullname核对无剩余失败；不是一次181全绿。13显示（含980 native）全部通过，完整SelfCheck新鲜PASS。当前实际Play正在执行；出生前置未完成，本Record不能VERIFIED/Task不能关闭。

## 当前阶段出口及父任务未完成

SCOPED_PLAY_PASS：真实旧内容Scene两个tick显示13/26/499/498→10/32/500/500，source真值保持，checksum恢复4→4；Q05有序关闭四类计数全0、两帧Stopped。CS0/dirtyfalse/root14/用户Scene SHA保持。完整报告同ID artifacts/REPORT.md。当前仅递推与slot适配阶段通过，Record保持FOCUSED_TEST_PASS，出生初始值与OPoint vitals前置未完成，不得VERIFIED。下一唯一Task OPOINT-SPAWN-VITALS-TRANSACTION，再返回本父Task联验。

账本最终PASS：514 records/17 governed code diff；日志在父DISPLAY-PROGRESSION artifact/ledger-final.txt。历史Record不在当前diff的警告保留，不是本次失败。
