# NTSD28-B5-RECOVERY-STATUS-CONSUMERS-NO-STATS-001 — Task Contract

> Goal9 / 2026-09-09 / VERIFIED / NO_STATS_THREE_CONSUMERS_ONLY。本合同脚本修改前建立；最终证据见同ID Record。

用户Goal9授权仅无stats/chp/cmp子集的weak positive-status、hp-double fallback、mp-bonus三个consumer。
正式Authority保持B1E13AE1 EXE及对应playable closure；battle_world.cpp:2165-2176、2192-2219、2321、2355-2358。
## 步骤0只读结论

Authority simulation_tick_driver.cpp:983 pre-display resource → 1018 step_frame_slot → 1025 advance_reaction_timers_slot。
battle_world.cpp:2165-2176/2216/2321/2355-2358读取timer；3739/3741递减mp-bonus/hp-double；3756-3757在body未skip且HP>0时递减weak。
Unity BattleLateEntityLifecycleModule.cs:119-143恢复 → 155-173 frame body → 178-181 reaction/status tail；437/439递减bonus/double，450条件递减weak。
结论：同实体本tick消费在递减前，与native一致；timer=1当tick有效、tail后0，focused必须实际证明。保持既有phase派发、body-skip、递减owner不变。

render_phase资格已建模，不标DEFERRED_RENDER_PHASE_ELIGIBILITY：NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001已唯一绑定render_phase_008到Runtime.HitStop/LF2Entity.HitStun（LF2Entity.cs:285-288）；两恢复方法现有HitStun<0早退等价无stats子集render_phase>=0，原样迁移至shared writer并覆盖negative/zero边界。
stats.bound例外继续明确后置，不添加新资格。

自然producer：BattleDamageWriter.ApplyConfirmedInputStatuses:257/278-284通过itr.weak给WeakTimer12C赋正值；另外两字段在Unity production只有载体/reset/copy/checksum及C25h decrement，未找到自然正值writer。Authority core也只见default/read/decrement，正值赋予来自source tests。三分支不存在可完整自然触发的生产链。
PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER：允许Editor定向强制carrier覆盖全部三路径、timer顺序和边界；不声称weak完全没有producer，不为Play添加production timer赋值或新资源。

## 精确文件与符号
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRecoveryStatusWriter.cs（新增）及同名.meta：internal static BattleRecoveryStatusWriter，ApplyHpRecovery / ApplyMpRecovery；无持久状态。
- Assets/NTSD/Scripts/Test/Editor/NTSD28B5RecoveryStatusConsumerEditorTests.cs（新增）及同名.meta：focused matrix和纯测试fixture，类别NTSD28/NTSD28_B5、名称NTSD28B5。
- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs：仅RunPreCollisionRecoveryPhase，把普通HP/PP两段交给shared writer；负environment调用保留原夹层。
- Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs：仅ApplyAuthorityRecovery同样接线；Execute phase/no-op/type路由不变。
- 本Task/Record、CHANGE-LEDGER、STATE、对齐总表和Temp产物。
不改任何其他脚本/Scene/Config/Prefab/ProjectSettings/Authority/生成器或第三方；.meta仅为新增Unity脚本标识。

## 数据与时序不变量
无新runtime字段，不改phase来源、cap500、PP150已修边界、base公式常量、OID51/52、stepWait、render phase门控。
共用HP段：保持既有HP>0且HP<HPBound/周期/stepWait条件；weak>0时不加HP，仅PP<HP3时PP+1；否则hp-double>0加2，非正加1。
共用MP段：保留原资格并增加weak>0抑制；原delta额外加(mp-bonus>0?1:0)。
顺序HP状态恢复 → 原negative-environment writer → 普通MP helper；timer只读，仍由原C25h递减。
不实现stats regen/dhp、chp/cmp、drain、mode rules、stats.bound。边界HP/MP clamp保持原范围，不借本包扩展post-resource。
shutdown声明：writer为无状态同步函数，只在既有恢复pass调用；不创建manager/queue/worker/cache、不接单、不需drain，停止tick即停止调用。十一阶段合同及幂等关闭不变。

## 测试与验收
新增测试先跑一次RED，production未改；通过world.LateEntityUpdateAll驱动真实Legacy/ECS/Derived路径。
覆盖timer0/1/正数/负数、weak抑制HP并MP+1及MP=HP3不增、phase3抑制、HPdouble+2、MPbonus+1、三timer组合优先级、原PP150与render phase资格、当tick消费后timer递减。
全部focused GREEN后，B5名称组原777+新增、NTSD28分类原247+新增完整运行；full SelfCheck PASS；双Assembly build0 error。
指定instance gameplay-ability-system-for-unity@b1b02287 / 2022.3.62f3 / NTSD_Battle；不第二实例；Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11保持。
Play采用上述有据豁免，未进入Play。不把Editor强制字段写为自然生产链证据。
硬停止：步骤0冲突、修正后既有测试失败、范围或Scene违规立即停，不修其他项。
VERIFIED只关闭三consumer；完成停止等待Goal10。

## 风险与回滚
共享提取可能影响既有两路，以timer0与boundary/full回归验证；weak分支必须优先于hp-double，抑制bonus普通helper，timer1顺序不可颠倒。
回滚需用户明确批准，只撤销本包shared writer/调用增量和新增tests/治理；保留Goal8的>150以及所有原用户工作。无持久迁移或外部不可逆动作。

## 最终实现与验收补充

原普通MP资格块（含PP150/render）最终保留在两个调用端，只共享weak抑制、公式和bonus；不改变资格owner或既有源检查。首次新增测试的Assert.Multiple不受当前NUnit支持，已改逐项Assert；0-test请求不计RED，真实RED69执行/至少25实测失败（MCP capped），随后69/69 GREEN。
B5 846/846（777+69）、NTSD28 316/316（247+69）、fresh full SelfCheck14:36:14Z PASS，双build0 error，指定Scene不变；Play使用上述有据豁免。完成停止等待Goal10。
