# Q06 MP 首差审计

VERIFIED_AUDIT_ONLY / CAUSE_CONFIRMED。此包没有修改生产或测试脚本、DAT、图片、Scene、框架或非战斗行为。复用当前native source诊断runner，只向本artifact目录写入场景夹具和结果。禁止computer-use。

## 实际因果证据

当前正式EXE SHA为B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。核验现有runner二进制和源码hash，并按原PowerShell构建脚本的Sort-Object顺序重新核验75个C++源/header文件；与07CD47A0…来源manifest一致。Python字典序首次算得不同hash，仅排序规则不同，随后原算法与原顺序复算一致，未把它误认作source变动。

四次实际执行使用相同正式Logan DAT/decoder身份、seed682973786、Naruto OID2与Rock Lee OID7、stage23、HP500/MP200、neutral3ticks，只改变已存在的scenario输入selectedModeMpRegenGate2C：

| 模式字段原值 | tick1 MP（两槽） | tick2 | tick3 |
|---|---|---|---|
| 1 | 200/200 | 200/200 | 200/200 |
| 0 | 200/200 | 200/200 | 201/201 |
| 2 | 200/200 | 200/200 | 201/201 |
| -1 | 200/200 | 200/200 | 201/201 |

证据：mode-gate-causal-comparison.json、各mode-gate-*.raw.jsonl和process.json（真实argv/exitCode0）。它们是SOURCE_MODEL_DIAGNOSTIC_ONLY，不冒称本次执行了正式EXE或物理输入。正式root只读。原场景未声明此字段，scenario28.cpp:772-773明确默认1；GameSession28::initialize复制config，step在4161-4162传入resource_rules。原版data/bg/wha.dat的normal模式也明确regen_mp:1，casual/practice等仍须Q08按正式选择记录接线，不能把1当作所有模式固定值。

OID2/7当前真实stats都只有max_mp:500，没有regen_mp，因此native整数读取为0。Unity的BattleRecoveryStatusWriter.ApplyMpRecovery对HP500计算(500-500)/100+1=1；其两个实际caller没有模式原值判断。原版对非负regen_mp只在selected_mode_mp_regen_gate_2c != 1时加值，因而原冻结场景保持200。这确认了Q05剩余200/201首差的原因；之前的MP最大值初始化问题已经修复，不再重复归因。

## 当前调用链与边界

- 正式playable build.ps1包含core battle_world.cpp/simulation_tick_driver.cpp及playable game_session.cpp；README_SOURCE声明对应发行源。正式source未写入。
- GameSession28::step设置resource_rules及F6对应hit_resource_enabled；SimulationTickDriver28::step在937先推进resource phase，在每slot definition/特殊clone后依次执行pre-display资源（983）、display（989）、post-display资源（993），再进行frame/lifecycle。
- BattleWorld28::advance_native_resources_pre_display_range的MP段在battle_world.cpp:2290-2381；type0、当前frame存在且不是lifecycle_resolution_pending才进入。native资源phase3为0时，先frame.cmp，再完整regen helper，最后此helper内部限幅。
- Unity实际路径：NTSDBattleTickSystem→BattleLateEntityLifecycleModule:120-145→BattleEcsCharacterRecoveryPass.Execute/ApplyAuthorityRecovery；派生/legacy回退走LF2Entity.RunPreCollisionRecoveryPhase:2769-2792。两者最终调用BattleRecoveryStatusWriter.ApplyMpRecovery。必须一起处理，不能只修optimized路径。
- 当前Unity MP writer没有读取NativeMetadata.Stats、frame.cmp或模式恢复值；其caller按tickIndex%3、PP>=500、HitStun<0和旧stepWaitGate早退。HitStun实际映射Runtime.HitStop（raw trace的renderPhase）；不能仅凭名称猜测。
- NativeResourcePhase3/12已经在World提供并进入snapshot/checksum，Q06应消费它们而非把host tick取模重新当作资源真相。当前neutral两者碰巧同相，不能推广到恢复/显式phase夹具。

## 必须整体处理的MP事务

| 顺序/条件 | 当前权威行为 | Unity现有缺口 |
|---|---|---|
| phase3前置 | 只在world资源phase3=0；type0/有效当前frame/活对象 | 当前用host tick取模；须沿用实际实体准入 |
| frame.cmp | 非0先加；InputDoubleCost19C>0时有符号右移1 | 未消费；不能放在helper早退条件之后 |
| stats presence | 有无stats按字段集合是否非空，regen_mp缺省0 | Q05模型已具备，不新增字符串parser |
| helper -1 | regen_mp=-1禁用helper，先前frame.cmp仍有效 | 缺此分支 |
| MP门槛 | 普通gate=-1阈值500，否则150；<=包含等值；regen_mp为-2..-6跳过此门槛 | PP>=500统一早退错误覆盖负向分支 |
| render/weak | renderPhase>=0或stats.bound恰1；weak_timer<=0 | 当前负renderPhase一律拒绝，无stats例外；bmp.bound不是此字段 |
| basis | -2/10→500；-3/1/11→375；-4/2/12→min(HP,500)/2；-5/3/13→/4；-6/4/14→/8；default才对OID51/52减半 | 只实现default公式 |
| delta | (500-basis)/100+1，MP bonus timer>0再+1，整数除法向0 | 保留已存在正确timer职责，补其余basis |
| 正向/负向 | regen>=0受mode原值==1抑制；regen<-6照常增加；-2..-6在MP>0且F6允许时扣除 | 模式/负向族缺失 |
| 限幅时点 | helper eligible后无论是否applied都clamp 0..500；helper被拒绝则本段不额外clamp | 不能把限幅提前到frame.cmp或统一setter，也不能用baseMaxMp替代常数500 |

HP阶段的regen_dhp/regen_hp/chp、模式默认HP门槛、display与post-display之前动作/资源尾部属于后继独立资源包；已完成weak状态、负环境伤害等职责保持。CPoint/OPoint/+2F8/revival/pieces与landing除法精度仍留Q06，不因MP包关闭整组。

## 模式输入与版本约束

Unity现有NativeHitResourceRules有18/1C/34/38/90，没有28/2C恢复模式字段。因此不能在本MP修复中随手新增未进入snapshot/checksum的可变开关，也不能把1写成永远禁用恢复的补丁。Q06基础事务应接受明确的不可变模式规则输入并覆盖全部原值；实际caller选择合同需在事前Record中写清原版默认1与Q08正式模式记录注入的边界。若选择新增可变World载体，必须先独立声明联合schema影响，不能悄悄绕过刚验证的13/21/24/2/2基线。

下一唯一Task为NTSD28-Q06-NATIVE-MP-RESOURCE-TRANSACTION-001。其出口必须包含实际生产两caller、native分支矩阵及same-content neutral MP差异闭合，不能仅交一个未接线helper或跳过非默认模式参数。正式选项/模式记录投影归Q08并触发R07，普通规则本身不能等整个Q08才实现。

本次复用现有Editor测试FormalLoganCaptureUsesActualInputsAndRestoresExistingManagerReferences实际重新采集当前Unity：job f5f8781cd2a6450b85da56921136d1f4，1/1 PASS。fresh-native-unity-mp-comparison.json确认两端完整content头相同，tick1/2都200，tick3仍native200/Unity201。该测试PASS只证明capture和manager恢复成功，不代表MP已经对齐。unity-capture-test-results.xml和unity-current.raw.jsonl已归档，未修改原测试脚本。

本次不重新运行完整Unity SelfCheck/Play：没有脚本变更，readonly-code-scope.json确认Q05出口6个脚本hash保持，Scene旧SHA保持；本包证据是新鲜native四次因果执行、当前Unity实际capture与调用链审计。上一Q05验证仍保留，不冒称本次已修复Unity MP。六个MISSING仍在，总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。
