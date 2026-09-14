> 最新资源owner限定状态：VERIFIED；隐式零帧资格由 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 补证闭合。完整tick其它reader迁移未完，不覆盖下游调用链。

> 2026-09-14资格纠正：当前FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION；原有效声明帧公式/phase证据保留，native未声明0..998零帧与Unity HasFrame差异待NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001处理，旧VERIFIED文本为历史。

> 当前状态：VERIFIED / SCOPED_HP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS。证据见 artifacts/diagnostics/NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001/REPORT.md；以下计划/失败记述为历史。下一 NTSD28-Q06-DISPLAY-POST-DISPLAY-RESOURCE-AUDIT-001 / READY_READONLY。

# Q06 HP资源事务与pre-display资格

IN_PROGRESS / TEST_FIRST；准确五脚本Record已建立。先读当前authority和MP事务最终REPORT；MP2028向量、119回归、同源200/201差异消除及真实Play证据保持，不重做MP算法。若HUDBg Scene独立变化尚未获来源确认，保留记录，不阻塞与UI无关的HP源代码/夹具工作；不得覆盖Scene或谎称hash unchanged。

目标为当前native battle_world.cpp:2149-2222的完整HP段及共同pre-display准入：type0、当前frame存在、对象未进入lifecycle resolution；World.NativeResourcePhase12为0、0<HP<effectiveMaxHp；weak>0时只允许当前MP<baseMaxHp的+1并跳过普通HP链；否则按stats.regen_dhp范围/EffectiveMaxRegenDouble1A8更新有效HP上限，stats.regen_hp范围/HpRegenDouble1AC加HP，再按frame.chp增加HP与向0除3后的有效上限，或按stats presence/regen_hp=-1/selected mode+28==1决定fallback增量。注意explicit regen_hp和fallback不是互斥else，不能凭习惯改写分支顺序。

从GameSession28配置默认值和正式mode记录投影追踪模式28，复用MP不可变规则输入做法，Q08负责正式选中记录注入。禁止新增未进snapshot/checksum的可变字段或把默认值误当所有模式。复用Q05 NativeMetadata、chp、已有1A8/1AC/12C/HP/HPBound/HP3载体；源码整数字段和C++向0除法为依据。

候选路径：BattleRecoveryStatusWriter.ApplyHpRecovery、BattleEcsCharacterRecoveryPass、LF2Entity.RunPreCollisionRecoveryPhase以及新focused/native witness。必须先准确Record及符号/副作用/回滚，再动脚本。MP及负环境写入的既有算法保持；但需核对共同type/frame/lifecycle资格，避免HP/MP单独拒绝缺frame而中间负环境仍被执行。若需修资格，只限定caller准入，先独立见证，不重写已验证负环境事务或重排C25骨架。

native成员函数分支向量覆盖有无stats、regen_hp/dhp边界、正负chp与/3舍入、HP/有效上限/baseHp/MP边界、三timer/bonus、mode -1/0/1/2、phase与host tick错相、两caller/缺frame/lifecycle条件。先RED→完整对照→相关MP/弱状态/负环境/快照回放回归→SelfCheck→真实Scene定向HP状态及恢复关闭；明确旧内容Play与实际Logan数据证据区别。若旧预期失效，独立Record修夹具并保留原失败。

本任务只处理HP/pre-display资格，display插值、post-display之前状态/资源尾部、CPoint/OPoint/复活/pieces继续独立Task。33ms/3ms、十一阶段关闭、非战斗/Unity-GAS/Scene/InputActions/资源/Gen/Plugins/外部Server保持；无新manager/queue。回滚须批准仅本包差量，不关闭Q06或总目标，禁止computer-use。
