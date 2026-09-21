<!-- CHANGE-RECORD
id: NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001
status: VERIFIED
change-kind: EFFECT_OVERRIDE_NATIVE_FRAME_SOURCE_WITNESS_FIRST
code-path: Tools/NTSD28AuthorityTrace/effect_override_native_frame_access_witness.cpp
code-path: Tools/NTSD28AuthorityTrace/validate_effect_override_native_frame_access_witness.py
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06EffectOverrideNativeFrameAccessEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
authority: Current playable BattleWorld28 ordinary unarmored hit effect override, native DatDocument::frame and explicit high-frame suppression/previous-state gates.
evidence: Live reader/binder audit plus corrected old getter857 cutoff and formal330 effect8 field inventory; no fresh Unity RED yet.
-->

# NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001

IN_PROGRESS / SOURCE_WITNESS_FIRST。正式playable BattleWorld28::resolve_ordinary_unarmored_standard_hit→resolve_confirmed_unarmored_hit→apply_native_effect_action_override，抑制gate/action_type/pickedact previous-state/post-damage HP与attacker先target后顺序继承已VERIFIED NTSD28-B5-EFFECT-ACTION-OVERRIDE-001。旁边Kind0PostEffectAction及Type3PostHit是独立事务，不混改。

剩余准确访问边界：BattleDamageWriter.ResolveNativeEffectActionOverride两reader、ApplyNativeEffectActionOverride两binder；条件性HitPlan.ProjectNativeEffectActionOverride identity投影分支两reader。旧LF2FrameCache上限857排斥显式857..999，不是只缺implicit：latch900/state602或BDY50会漏抑制，previous900/state12+pickedact12会错误拒绝。审阅初稿声称已声明读取等价已纠正，不沿用该错误scope。隐式state0/null在gate中的等价仅适用未声明帧。

第一写域仅Tools/NTSD28AuthorityTrace/effect_override_native_frame_access_witness.cpp。主16代表：8正动作binder(双方错开低隐式/857/900/998/999声明缺失/1000/显式)，4reader控制(低/高隐式latch、positive picked配implicit拒绝/显式900state12匹配)，2高位抑制(900首BDY50、900state602)，2positive/death控制。kind0/effect8，普通type0双方；真实geometric candidate→resolve_ordinary_unarmored_standard_hit，不直接匿名helper。完整before/after/following，双方descriptor/历史latch/previous/snapshot区分、HP/pending/RNG/calls，记录真实result override/posteffect字段。不得修改权威源码/资源。身份转换source/Unity证据独立后继，普通16不能关闭HitPlan条件分支。

正式330字段审计effect8=801，其余8..16为0，catchingact/pickedact全0，caughtact多个正值及-3；仅静态token域不证明目标可达。报告remaining-reader/FORMAL330-EFFECT-OVERRIDE-FIELDS.json。延用用户代表验证策略，不做全乘积，不重测未变化B5全矩阵。

在源实测后准确新增Unityfixture及最小production符号，当前生产未改。验收compile、source independent/Unity RED、相关回归、闭包出口一次SelfCheck/代表replay/Play/Q05关闭；若发现其他hit事务差异另拆任务，不改expected掩盖。保护schema/Kernel/Scene/资源/非战斗/framework；回滚仅本ID精确diff并保留别的改动。Q06未完/Q07未迁移。

source16首次build/双跑exit0且一致SHA52093e69d3e286ddd5e496ab862efb7e35ec385b63b41472fa5b0dda51696f0d，正常15HP395/死亡HP-4，高位抑制与previousmatch分支实际触发。准确新增独立validator validate_effect_override_native_frame_access_witness.py，仅核对本夹具effectgate/override选择/descriptor与counter/latch/previous/snapshot保持及固定injury5的HP；不声称独立模拟全部hit字段/RNG或following。source全量capture保留用于Unity后继对照。

EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001 source16已build/double-run一致SHA52093e69d3e286ddd5e496ab862efb7e35ec385b63b41472fa5b0dda51696f0d，focused独立365检查PASS；仅gate/override/descriptor/历史字段保持及固定injury5 HP，不模拟全hit sideeffects/RNG/following。初版target显式wait误填37（源41）两失败已保留并纠正。真实普通unarmored source正常HP395/致死-4，高位latch900state602/BDY50抑制及previous900state12匹配已触发。下一准确扩Record单Unity fixture，slot0/70完整before及whole标准hit后/下tick对照，先RED再决定normal两reader/两binder。实际标准hurt支持帧180/186/220已在同源DAT显式声明以隔离别的binder。HitPlan身份投影两reader另需真实转换代表，普通16不关闭它；Kind0PostEffect/Type3PostHit仍独立后继。生产未改；Q06未完/Q07未迁移/总目标ACTIVE，无运行build/job/agent/Play，Editor62860复用，禁computer-use/Scene/资源/非战斗修改，按分支代表验证。

Unityfixture脚本前登记：新NTSD28Q06EffectOverrideNativeFrameAccessEditorTests.cs，主16 Authority/DataOriented+Mobile/Legacy6代表(0,5,10,11,12,15)，两slot0/70正式type0 factory，spawn后恢复源before全部字段。通过现有ApplyStandardCharacterDamage调用完整unarmored writer，after及following分开与source比较，不直接私有effect helper，不据此重验candidate/eligibility。继承pending double精确比较以及明确previousXYZ source-only边界。初态有差异须先修fixture，尚不改生产。身份投影未覆盖。

有效RED jobc13b4d126a0a42608ab961b4d20d99ab两FAIL：主16 before0/immediate40/following61，smoke6 before0/immediate24/following36；产物production-red，含case15 hitReactionTimer0 expected80独立差异。生产修改前准确扩域仅BattleDamageWriter.ResolveNativeEffectActionOverride的latched/previous两读改FrameCache.GetNativeFrameDataById，ApplyNativeEffectActionOverride两写改DirectWriteNativeRawFramePreserveWaitCounter。保护gate/正值HP/attacker先target/计数器和其他尾部。身份projection实际40/40历史低位域由独立审计确认，先不改HitPlan两reader，不伪造projection900；细节下继报告。反应计数80→0保留失败独立跟踪，禁止本包顺手改。回滚仅4访问点并保留此前impact单行。

四访问修复后jobe82c470549de4aa2bbe5becb40e8a89c终态两FAIL，主16/smoke6均before0/即时1/后继1，仅case15 Fall0 expected80，证据after-native-frame-access。NTSD28-Q06-STANDARD-HIT-FALL80-PRESERVATION-001 IN_PROGRESS / SOURCE_WITNESS_FIRST：仅新CPP3代表预声明；effect四访问修后before0/即时1/后继1只余致死Fall80独立clear，生产clear未改，父effect等待本包。

补记原GetFrameDataById委托FrameCache?.查询，新native reader也保留?.空安全，避免新增非空前置。尚未运行后续Unity测试，等待独立fall80 source3。


2026-09-21 联合出口：job d41e706b23d048448436ee57d9f2f0c9 30/30 PASS，实际1.777秒；effect16+smoke6 before/即时/following零差异，标准source3及vertical2真实candidate Shadow/DataOriented通过。完整SelfCheck 01:55:20 UTC PASS已存joint-pass。Ledger603/28 PASS，diffcheck通过。仍IN_PROGRESS，尚待代表Play/回放。
测试扩域预声明（生产不再修改）：同一Effect Editor fixture新增6代表的Authority400/DataOriented same-world snapshot restore后完整伤害+两tick replay；复用既有capture/factory与snapshot API。新增同文件NTSD28Q06EffectPlayProbe请求入口，保护现有Scene checksum/borrower，在一次Play合并effect6 renderer+6 logic和Fall80/vertical真实candidate共10例，结束请求既有Q05关闭。没有场景资产保存、物理键盘或图像一致性声明。

测试扩域已实现：Effect fixture新增Authority6 replay与EffectPlay22 probe；Fall80 RunCapturedHitPlan可选renderer并逐world断言borrowers恢复。当前仅刷新编译，新增replay/Play尚未通过；不重跑既已通过完整SelfCheck。


2026-09-21 限定VERIFIED：Two normal effect latch/previous readers and two raw-action binders. Source16 SHA52093e69d3e286ddd5e496ab862efb7e35ec385b63b41472fa5b0dda51696f0d,focused independent365 checks; only gate/override/descriptor/history/fixedHP model, not full following/RNG independent model. Existing effect gates/order retained. Identity projected readers remain separate low-frame-domain audit.
联合30/30、SelfCheck01:55:20Z、代表回放6/12ticks及Play22/关闭02:00:04Z PASS；Scene hash/dirtyfalse/root14保持。完整证据与限制见artifacts/diagnostics/NTSD28-Q06-EFFECT-OVERRIDE-NATIVE-FRAME-ACCESS-001/ACCEPTANCE.md。上文未运行/生产未改是历史检查点，由本条覆盖；不关闭整体Q06或Q07。
