<!-- CHANGE-RECORD
id: NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001
status: VERIFIED
change-kind: NATIVE_HP_RESOURCE_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRecoveryStatusWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06NativeHpResourceEditorTests.cs
code-path: Tools/NTSD28AuthorityTrace/resource_hp_contract_witness.cpp
authority: Formal Logan playable GameSession28/SimulationTickDriver28 to BattleWorld28::advance_native_resources_pre_display_range, battle_world.cpp:2149-2222.
evidence: HP3768 native and prior runtime evidence retained;Native zero-frame admission corrected with63 valid cases,223 PASS and scoped resource Play;full reader migration pending.
-->

# Q06 HP资源事务与共同准入

IN_PROGRESS / TEST_FIRST。准确五脚本，保留同文件前一MP包已验证差量。现有HP入口只有weak早返和默认+1/+2，缺stats.regen_dhp/regen_hp、chp/除3、mode28和真实phase；两个caller仍用host tick%12和旧stepWait。当前native完整段已确认；正式playable默认mode28=1（见下方纠正）；模式28的正式选中记录仍由Q08注入，不新增未校验mutable载体或schema。

共享HP入口以(resourcePhase12, selectedModeDefaultHpRegenGate28)不可变值参数消费当前definition/当前action，而非旧Frame.D。依次资格phase0/0<HP<effectiveMaxHp→weak只按MP<baseMaxHp加1并返回→regen_dhp 1..5及1A8双倍→regen_hp 1..4及1AC双倍→非0 chp增加HP和chp/3有效上限，或有无stats/regen_hp=-1/mode恰1控制的默认增量。显式regen_hp和默认增量能叠加；本段无全局clamp，不提前套post-display限制。

抽出与现有MP准入相同的CanEnterNativeResource(entity)（type0/有效当前frame/非dormant/pending），两个caller在HP→既有负环境→MP之前使用；原负环境算法保持，不能对缺frame对象单独执行其夹在中间的伤害。MP算法和参数/phase保持，只复用完全等价共同谓词。HP周期改用World.NativeResourcePhase12；没有World的回退caller不凭host tick捏造资源phase。

新增C++ witness调用原成员函数并用render_phase=-1抑制后续普通MP helper，隔离HP段及weak的MP副作用；不写authority。分支向量覆盖presence/regen范围/chp舍入/双倍/weak/mode/phase/HP-bound-base-MP边界。新Editor测试先RED（反射目标签名和实际两caller），再native逐值/资格阻断负环境/错相/两caller/无新增分配。保留MP2028矩阵与同源raw回归；相关69状态/负环境/恢复回放及SelfCheck要复验。旧fixture失效须独立Record后修。

真实Scene用已声明新Editor探针在明确phase、程序注入HP状态下走生产tick和显式mode28测试，恢复完整World后由既有Q05有序关闭探针回收；不改DAT/Scene或冒称物理技能/正式新版图片。新工具/测试无正式Runtime manager/queue/新关闭职责。当前用户确认HUDBg x30及Scene bcd1047b…受保护。

风险：chp与explicit regen顺序、整型除法/限幅时点、弱状态错走普通链、无stats误被mode抑制、共用资格改变MP或负环境行为。以原函数矩阵及既有回归约束，源证据不足不得猜测。33ms/3ms、十一阶段、GAS/Mono/非战斗/资源/Gen/Plugins/外部Server保持。回滚须批准仅本包差量，不撤销MP/Q05。display/post-display、CPoint/OPoint/复活/pieces及Q08模式仍后继，不能关闭Q06或总目标。


## 默认来源纠正与RED

正式GameSession配置game_session.h:345及scenario28.cpp:809-811默认28均为1，覆盖ResourceSystemRules结构体孤立默认0；生产必须传1。前述恢复入口中的计划默认0为不完整读取，现纠正，未按0修改生产。native矩阵显式传各原值不受影响；新增正式Logan HP100两角色12tick source见证，两槽始终100，用两profile真实Unity测试验证正式默认值。

初始13项RED11FAIL/2PASS：两caller缺完整HP/cmp更新、缺frame/pending对象仍执行负环境伤害、完整入口缺失。native构建通过并生成3768条向量。当前追加实际Logan12tick RED，生产仍未改。

## 生产实现已写

实际Logan两profile追加RED均在tick12观察到Unity101而原版100（31affe9f1322456cb45b46f3b28095cc），源场景mode28默认1已确认。现HP入口改为显式phase12/mode28并完整处理stats/chp/weak，生产默认28=1；共享资格置于两caller最前，MP仅替换等价谓词，负环境算法不动；HP/MP按各自World phase，不再用host tick或stepWait决定HP。未添加runtime字段/manager/schema。当前Unity编译0error，134项候选合并回归正在运行，尚未报告已对齐。

## 134项回归及Play探针实现记录

新鲜Unity合并回归134/134 PASS，job f5b1e641ff4642d2b77da01317640d10，127.385秒，归档focused-134-results.xml；包括3768 native HP向量、2028 MP向量及两profile Logan真实数据12tick HP/HPBound/MP逐值一致。完整SelfCheck首次在GT-06无World夹具失败，已由独立NTSD28-Q06-HP-SELF-CHECK-WORLD-CONTEXT-001修正并保留原失败。

已在声明的新Editor测试文件添加NTSD28Q06HpPlayProbe：请求文件触发，真实Scene等phase12=11后暂停，生产tick验证默认HP，并直接验证显式mode0/weak/双倍；finally恢复完整World/checksum，再由既有Q05探针有序关闭。新增代码编译CS0，真实Play尚待运行，不能提前报告完成。

## 最终限定出口（2026-09-14）

VERIFIED / SCOPED_HP_TRANSACTION_AND_DEFAULT_PRODUCTION_PASS：134/134（含3768 HP/2028 MP）、完整SelfCheck及实际HP Play/恢复/有序关闭全0通过。

详细证据和命令见 artifacts/diagnostics/NTSD28-Q06-NATIVE-HP-RESOURCE-TRANSACTION-001/REPORT.md。用户HUDBg x30/Scene bcd1047b…保留，保护清单无新增缺失；父Q06、Q08模式投影及正式资源迁移仍未完成。初始失败和中间状态为历史，不覆盖本出口。

账本最终验证PASS（510 records/10 governed code diff），详见父artifact/ledger-final.txt；历史Record非当前diff警告未清理。

## 2026-09-14资格覆盖纠正：原隐式零帧尚未对齐

当前native dat_document.cpp:91-116的frame(id)在未声明0..998时返回有效零帧；Unity FrameCache上界857且HasFrame仅判声明帧。本包CanEnterNativeResource调用HasFrame会拒绝这些native合法零帧，故原VERIFIED资格范围过宽，现降为FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION。原有效声明帧、phase/资源公式、越界9999、同源三tick及Play证据保留，不删除历史事实；原native invalid矩阵只使用9999，没有覆盖Unity fixture的未声明7。待独立NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001原函数见证、全reader合同及精确修复/回归后重新裁定完整资格。不得因此回退已正确资源公式或阻塞独立明确声明帧的出生资源任务。

## 2026-09-14资源owner资格补证闭合

原隐式零帧资格缺口已由 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 修复：共同资格/chp/cmp使用独立Native accessor，63合法文档原函数、223联合及实际两owner零帧/999边界、恢复关闭验证通过。本Record恢复VERIFIED / SCOPED_RESOURCE_TRANSACTION；旧拒绝夹具7改9999的独立Record保留。其它frame/input等reader尚未迁移，不能将本owner验证扩大为高位动作完整tick已对齐。
