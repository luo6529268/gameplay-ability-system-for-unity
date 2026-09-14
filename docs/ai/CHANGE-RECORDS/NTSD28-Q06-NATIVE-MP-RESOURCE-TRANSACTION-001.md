<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001
status: VERIFIED
change-kind: NATIVE_MP_RESOURCE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRecoveryStatusWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeMpResourceEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/resource_mp_contract_witness.cpp
authority: Formal Logan playable GameSession28::step / SimulationTickDriver28::step / BattleWorld28::advance_native_resources_pre_display_range, battle_world.cpp:2290-2381; Q06 audited four-mode causal witness.
evidence: MP2028 native and prior mode first-difference closure retained;Native zero-frame admission corrected with63 valid cases,223 PASS and scoped resource Play;full reader migration pending.
-->

# Q06 完整MP资源事务

IN_PROGRESS / TEST_FIRST。准确五脚本。当前writer只有默认HP basis和timer bonus；两caller用host取模/PP>=500/负HitStun/旧stepWait早退，缺frame.cmp、stats族、mode和F6负向路径。原版完整MP段位于C25 pre-display，不修改HP已验行为、负环境恢复、display/post-display或主pass次序。

## 已决定的模式与数据边界

共享MP入口接收不可变值参数(resourcePhase3, selectedModeMpRegenGate2C, negativeMpRegenEnabled)，不新增World字段/manager/队列/全局可变开关或snapshot版本。两个现有生产caller在当前尚无模式投影的入口明确传入source BattleConfig28默认2C=1，以及当前World.FunctionKeys.HitResourceEnabled；默认值具名常量并回链Q08正式模式记录注入任务。该默认只关闭目前已确认normal场景首差，不代表所有模式已接线；事务本身必须支持任意模式原值，Q08正式选择投影不能丢失。禁止以全局禁用普通恢复替代完整事务。

生产caller使用已入snapshot/checksum的World.NativeResourcePhase3；HP暂保留当前12周期及stepWait合同。MP依据当前definition/current action从FrameCache取真实frame和NativeMetadata.Stats，保留有无stats区别；不得用旧Frame.D或bmp.bound代替当前frame/stats.bound。当前type非0、无frame/生命周期待处理时拒绝。使用现有PP/HP/weak/bonus/InputDoubleCost19C/HitStop/OrdinaryCreditGate2F4，不新增实体字段。

共享事务按权威次序：phase0→cmp(必要时有符号>>1)→regen -1/阈值/bound/weak gate→basis分族→bonus→非负mode gate/小于-6正恢复/-2..-6且F6允许扣除→helper内clamp0..500。旧PP>=500等不完整caller gate移入完整事务，恢复阈值包含500/150等值；只在helper eligible后限幅，不额外对拒绝路径限幅。

## 测试、风险和回滚

新增C++ witness通过原source成员函数生成固定MP分支向量，复用Build-AuthoritySourceCapture.ps1的RunnerSource/独立Temp输出，不改authority或build脚本。新增Editor测试先对缺失四参数入口/旧两caller跑RED，再覆盖native regen -7..14/未知值、四模式、51/52、边界、cmp/有符号右移、weak/bound/F6/bonus、phase与host tick错相、有效frame/type及优化/回退一致。测试和Play探针仅使用本文件及已有bootstrap，临时World/资源按原owner退出，无新生命周期职责。

需真实Logan新capture确认currentMp首差闭合、完整SelfCheck/相关恢复测试/编译、必要真实Scene定向MP调用与恢复/有序关闭/零残留、Scene哈希及账本。测试PASS不能代替native对照；六MISSING如实保留。旧测试若期待被权威否定的默认恢复，先独立Record再改预期，不为了兼容保留旧规则。

风险集中于两caller漂移、cmp被早退跳过、模式原值被布尔化、stats和bmp混用、错误限幅/phase、元数据查询每tick分配。warm路径无新增分配，原值/分支向量及真实source检验。没有新服务/queue/pool/cache/关闭阶段，十一阶段、33ms/3ms、GAS/Mono/Scene/资源/UI/InputActions/Gen/Plugins/外部Server保持。回滚须批准仅五脚本本包增量，不逆转Q05。生产模式选择、HP/chp/display/post-display、CPoint/OPoint/+2F8/复活/pieces仍按后继Task推进；此包不关闭Q06或总目标。

## 测试/见证已写，生产尚未修改

新增Editor测试通过反射解析四个不可变参数的目标入口，缺失时明确RED；两实际caller测试直接调用现有路径，覆盖World phase与tick错相、cmp、500处负向消耗、normal gate1。native witness直接调用BattleWorld28原pre-display单slot成员，采用原DatParser构造明确夹具、推进phase且不进入HP12周期。首编译缺少dat_parser.h include已修，原错误日志保留；当前native构建及Unity编译继续，不把桥接观察超时当Editor失败重启。

## RED与完整事务已写

RED job65be3777bfdb40399f84b9adbbbf69cf：11项7FAIL/4PASS（cmp及500处负消耗两caller各失败、完整入口/当前frame/warm入口缺失）。native构建已通过，原成员函数输出2028条向量，formal/source身份不变。已修改三个生产文件：writer四个不可变参数及完整算法、ECS caller以World.ResourcePhase3选择并去旧MP早退、LF2Entity回退caller使用同一入口；HP及负环境调用主体保持。当前FrameCache使用HasFrame检查，防止缺帧EmptyFrame sentinel冒充有效帧。没有新payload/state/queue，Q08模式投影仍明确后继。Unity当前编译0error；实际12项focused/真实Logan capture验证进行中，尚未称对齐。

12/12 focused（job0d1f70a0518e49098fbed0e01cfcaa02）与2028 native vectors已实际通过；same-content-mp-comparison.json证明正式Logan两槽tick3均200，原数值首差闭合。105相关回归71PASS/34旧fixtureFAIL，独立DEFAULT-MP-RECOVERY-FIXTURE-CORRECTION-001处理，未回退生产。新增两mode路径的regen=-7/bonus最后tick测试，保留timer-before-tail正向覆盖。

同一已声明Editor文件加入显式请求驱动的真实Scene探针：待phase3=2暂停，capture全World后注入HP500/MP200中性状态，推进一个真实生产tick，验默认mode1不回、显式mode0回1、weak抑制；最后restore全checksum并保持对象数。该Scene为旧内容，不改DAT/frame metadata、不称物理技能验收。结果检查后由已有Q05真实restore/有序shutdown探针完成清理并复验零残留；禁止在本探针复制关闭架构或改变Scene。


## 当前限定出口

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / SCENE_ORIGIN_PENDING。最终119/119通过，2028 native MP向量/同内容44字段一致且只剩6MISSING、完整SelfCheck及实际MP/恢复/关闭全0通过。完整报告见artifacts/diagnostics/NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001/REPORT.md。Scene独立HUDBg x50→30变化来源待用户确认，未覆盖/不宣称hash unchanged；下一HP独立工作可继续。Q08正式模式投影、其余Q06及总目标未完成。


2026-09-14用户确认Scene HUDBg位置变化是其本人或其他任务修改；保护该现状，不再属于来源未明项。本Record状态提升为VERIFIED / SCOPED_MP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS；Q08模式投影及HP/其他Q06继续。

## 2026-09-14资格覆盖纠正：原隐式零帧尚未对齐

当前native dat_document.cpp:91-116的frame(id)在未声明0..998时返回有效零帧；Unity FrameCache上界857且HasFrame仅判声明帧。本包CanEnterNativeResource调用HasFrame会拒绝这些native合法零帧，故原VERIFIED资格范围过宽，现降为FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION。原有效声明帧、phase/资源公式、越界9999、同源三tick及Play证据保留，不删除历史事实；原native invalid矩阵只使用9999，没有覆盖Unity fixture的未声明7。待独立NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001原函数见证、全reader合同及精确修复/回归后重新裁定完整资格。不得因此回退已正确资源公式或阻塞独立明确声明帧的出生资源任务。

## 2026-09-14资源owner资格补证闭合

原隐式零帧资格缺口已由 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 修复：共同资格/chp/cmp使用独立Native accessor，63合法文档原函数、223联合及实际两owner零帧/999边界、恢复关闭验证通过。本Record恢复VERIFIED / SCOPED_RESOURCE_TRANSACTION；旧拒绝夹具7改9999的独立Record保留。其它frame/input等reader尚未迁移，不能将本owner验证扩大为高位动作完整tick已对齐。
