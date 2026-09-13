# Q05 WeaponState旧载体清理限定交付

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。Q05/总目标ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

## 改动与范围

准确10脚本：5生产、4旧测试/诊断、1新测试。生产仅NTSDEntityRuntime.WeaponState field/copy/reset、WeaponBase reset、ECS fingerprint、checksum、parity旧字段删除。production-scope-stability.json逐文件验证与预变更文本的差量只包含这些删除；实际CurrentFrameState/GetResolvedWeaponStateForExternalUse、FrameLogic/HeldState行为保持。

旧reserved sentinel不再伪造0：旧canonical copy/snapshot/checksum敏感性改用真实WeaponFlightCounter并验证运行时JSON无weaponState；held报告明确观察真实frame状态，并断言实际follow links、throw/drop速度、damaged release；SelfCheck保留动作/速度/durability/cleanup，移除旧假字段断言。旧实际帧状态构造参数weaponState和LF2States.WeaponThrowing保留。没有替换成另一个平行状态机。

## 实际验证

- 使用现有Unity2022.3.62f3桥接refresh_unity/get_editor_state/run_tests/get_test_job/read_console、request文件与manage_editor play；未使用computer-use或第二同项目Editor。
- RED job383ba778b8d84643a23cf84fcba4f6bf：4FAIL/2PASS；字段/三输出存在，实际状态解析两例原本通过，red-results.xml留证。
- GREEN job4f310c2b78d7458daf808b548ba8d200：282/282 PASS、0skipped，17测试类见test-selection.json；包含当前帧与旧并行状态退休、held/throw/类型1落地、copy/snapshot/restore/checksum/ECS与前包回归。
- 完整BattleRuntimeSelfCheck request2026-09-13T10:14:50.268303Z，结果10:15:29Z PASS，mtime晚于新请求；原结果保存为NOT-CURRENT，未混用历史PASS。
- 两次真实NTSD_Battle Play经已有Goal20_R4_CurrentPlay.request=GREEN调用RunCurrentPlayWitness。当前world.RuntimeCharacterConfigs的OID124/action40、OID7/action0，seed424242，两次明确pre-frame-advance调用；state1002/hit_Fa12/vx14。after已没有reservedWeaponState字段；before/after其余ticks字段逐项一致，beforeObjects4/afterObjects4。附带R4四个释放用例也PASS，自动退出Play。
- Play的checksum字段在该入口未填充，比较其默认值不构成完整checksum证据；采用真实state/action/motion/target/RNG和关系/清理证据。没有运行物理按键、完整技能或整tick/native-world checksum对照；此前native authority audit/行为退休结论作前置，本轮没有重写规则。
- Unity完成编译和测试，最终error CS过滤0；SelfCheck中的预期故障注入日志不用于声称Console全无error。Scene最终dirtyfalse/root14，SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f不变。
- 保护3059：2976同/65既有或声明差量/18既有Foot缺失，新增基线差量只有本包LegacyWeaponState测试，无新缺失。当前包10脚本与ReleaseTick前包14合并16，combined-code-scope.json核对一致；用户/前包未提交工作保留。
- git diff --check PASS；Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path PASS：494 Records/16累计差量脚本，最终ledger-final.txt。没有提交/推送、资源部署或非战斗/框架修改。

## 后继

下一NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001，必须覆盖runtime/Entity/OPoint task/ECS/HitPlan所有别名，不能删除真实holder/owner/spawner/target。case-insensitive候选线索在holder-copy-next-all-references.txt。五类退休仅剩HolderCopy；随后继续Q05 identity/双OPoint owner空队列guard、统一13/21/24/2/2、旧版本拒绝/新snapshot回放/Play。当前12/20/23/1/1未发布中间态，R13删除子条件PARTIAL_RETURN而完整R13/R15仍未关，Q07正式DAT/角色图片迁移未执行。

保持Unity/GAS、非战斗、33ms/3ms、十一阶段、Scene/InputActions/Gen/Plugins/外部包、stage.dat USER_HOLD及例外。禁止computer-use。
