# Q06 OPoint出生资源事务限定出口

VERIFIED / OPOINT_VITALS_AND_DISPLAY_BIRTH_ONLY，2026-09-14。总目标/Q06/父完整DISPLAY-PROGRESSION仍未完成；下一ZERO-FRAME-CACHE-CONTRACT只读合同，之后返回父display其余出生初始化，再继续post-display。

准确五脚本：BattleSpawnVitalsWriter（新无状态writer）、BattleLogicEntityFactory.PostInitLiving、LF2ObjectPointFactory.PostInitLiving、新Editor测试/Play probe和新native witness。三生产文件只涉及OPoint出生值，不调整parent/link、动作/位置/随机、weapon_hp、队列或关闭顺序。两caller最前同步调用，原旧OID5/52固定覆盖块移除。

## 规则与权威

正式EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033；最终75源/header manifest07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。原函数真实名称是BattleWorld28::materialize_supported_spawns；playable SimulationTickDriver28内materialize_native_frame_zero_entry调用它。此前文档materialize_frame_spawns只是误称，已经纠正，不对应另一个权威函数。

birth选择：point.hp/mp正值优先，否则OID5/52的10/5、其他500/500；stats.ohp/omp正值再int64乘后除100，向0截断。spawn_at把HP/effective/base设为最终HP，当前MP设为最终MP，baseMaxMP取有效stats.max_mp原值（包含0和负值），缺失则最终MP。两个HP display初始化为最终HP，累计display及四step为0。Unity写HP/HPBound/HP3/PP/MPMax和八display字段；保留无明确本事务来源的legacy Health.MP/PPMax/PPBound，不混用current和cap。

## 验证

- native-vitals.tsv：3716原materializer向量，包含正负/默认point值、OID5/52/51/普通、百分比及截断、long乘法、7种type、max_mp缺省/负/0/正。不是runner自算expected。
- native-actual.tsv：330正式目录里329个正OID使用各自实际definition和声明frame，通过原函数生成child；OID0被ObjectSpawnPlanner28原规则忽略，stderr明确记录。首次未处理这一准入假设导致exit3，native-actual-initial-failure.tsv保留；修正诊断迭代后exit0。
- 两组native输出重跑相同，列完整，HP/base/bound/display一致；native-validation.json和native-build-final-manifest.json记录身份及hash。源模型诊断不冒充正式EXE按键验收。
- RED job4af2b7e4d7a34eb4b04d20843dff37bc：10/10 FAIL，原两个caller和完整工厂得到10/500/17而期望50；red-results.xml。
- 首轮10/10 PASS（6618199fc0a64b3cae51388d52a07c5a）；扩展后的合并job57d4683aac0a40fdaf45fa5eb6f97d51为212/212 PASS、144.7277秒。含本包12、3716向量、两profile各329真实Logan metadata对照，catalog3900ECBC509557DB；四type完整logic factory反复创建/释放；两post-init seam；显示/HP/MP/快照回放和owner传播回归。focused-212-pass.xml及两个actual-births.json留证。
- 完整SelfCheck新鲜PASS（SelfCheck-pass.result），Unity编译CS错误0。
- 第一次真实Play两次逻辑出生正确，但误把表现工厂视为独立renderer路线。当前Scene UsesLogicOnlyEntityMaterialization=true，表现工厂正确转发logic，因此Renderer断言失败。World/checksum已恢复，随后关闭全0；原failure/cleanup保留，不修改生产来绕过它。
- 最终真实Play在paused无tick窗口保存模式，使用现有setter选择两路径，finally恢复模式再restore World；两条完整materializer各出生两次，HP101/201、MP103、baseMaxMP103（本Scene旧内容无stats）、display与HP一致，slot50复用，实体返回前检查。child均释放，World4→4，checksum与原模式true均恢复。play-births-pass.json。
- 最终Q05有序关闭：World/slot/logic borrowers/render borrowers全0、两帧Stopped；Editor已退出Play。play-final-cleanup-pass.json。无自然技能、跨slot新生首tick或正式图片表现结论；此前相关owner/回放覆盖继续保留，完整OPoint家族仍未完成。
- scene-final.json：用户HUDBg x30/Scene bcd1047b…不变，dirtyfalse/root14。保护3059中2922未变，较display新增变化路径仅声明LogicEntityFactory，0新增缺失；final-code-scope.json为五脚本hash。

实际入口/命令：Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28Q06SpawnVitals -RunnerSource Tools/NTSD28AuthorityTrace/opoint_spawn_vitals_witness.cpp -ExecutableName opoint_spawn_vitals_witness.exe；exe无参fixture及正式resources/runtime参数actual；Goal13_bridge.py refresh/get_editor_state/read_console/run_tests/get_test_job；SelfCheck请求；manage_editor play和SpawnVitalsPlay/Q05 ReplayPlay请求。没有computer-use、没有第二Editor，也未提交或改资源。

## 新发现零帧资格缺口与后继

native DatDocument.frame在未声明0..998时返回有效零帧；Unity缓存上界857、HasFrame只判声明帧。旧HP/MP native矩阵只将invalid设9999，没有证明未声明7等资格相同；因此两个HP/MP Record已追加纠正并从VERIFIED改为FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION，原有效声明帧、公式/phase及三tick/Play证据保留。

下一NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001 / READY_READONLY。先全reader合同与原函数边界见证，再精确修复资格/缓存，不全局替换HasFrame或擅改schema；本出生值事务的明确声明frame证据仍有效。随后回DISPLAY-PROGRESSION补普通bootstrap/Stage、非OPoint特殊clone与pool初值及联合验收，之后POST-DISPLAY。OPoint其他owner/动作/位置/队列字段、Q07正式资源、Q08 mode及整场仍待。

R05/R07/R09/R16只对已改出生资源/恢复/关闭子条件PARTIAL_RETURN；零帧资格与新出生时序继续独立回访。13/21/24/2/2、33ms/3ms、Unity-GAS/非战斗/所有例外及十一阶段保持。此包不重写原case之外的框架。

最终账本验证PASS：515 records/22 governed code diff；日志ledger-final.txt，历史Record未在当前diff的WARNING保留。总目标继续ACTIVE，无用户输入阻塞。
