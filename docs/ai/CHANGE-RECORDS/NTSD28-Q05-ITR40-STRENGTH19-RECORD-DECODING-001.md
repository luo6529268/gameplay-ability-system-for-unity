<!-- CHANGE-RECORD
id: NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001
status: FOCUSED_TEST_PASS
change-kind: ITR40_STRENGTH19_RECORD_CONTRACT
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05ItrStrengthContentEditorTests.cs
code-path: Tools/NTSD28Q05ItrStrength/AuthorityItrStrengthWitness.cpp
authority: Active Q05-A2; formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033; playable combat_records.cpp interaction/weapon_strength_interaction and dat_parser.cpp row admission.
evidence: RED_120_FAIL / FINAL_489_PASS / FULL_SELFCHECK_PASS / TABLE_SOURCE_INTEGRATION_PENDING / SCENE_ORIGIN_PENDING
-->

# Q05-A2 ITR40/strength19单记录解码与复制

状态IN_PROGRESS / TEST_FIRST。父ITR-STRENGTH Task的字段与record解码子项，准确五脚本见metadata，完整表语法/manager接线仍后继。Native40/19与严格首整数/普通整数语义取自combat_records.cpp实际playable路径。表头准入1..9/去重/错误拒绝由父Task刚追加的权威更正约束，禁止将lookup可返回0推成DAT允许0。

实际范围：InteractionArea加独立drain/sound/cover及CopyFrom；ItrProjection/两个Fingerprint捕获这三字段，与既有geometry字段保持；旧kind5 replacement不新增这些字段的规则覆写，与actual ShallowCopy路径同保留source值。WeaponStrengthEntry加缺少的11字段，形成index+19，不改_setList/lookup/factory。LoganCombatRecordDecoder新增Interaction完整40项及WeaponStrength(int index,IReadOnlyList<Lf2DatProperty>)单记录19项；新native ITR取first整数，caughtact/catchingact用单元素数组承载，当前kind1/kind3实际reader仅读[0]并按符号控制朝向，不能引入第二元素背向分支。

Interaction decoder复用ApplyInteractionGeometry保留有效性/raw深度，其他native字段strict；未知或confuse别名不生成状态，原AST保留。legacy vaction/throw/kill等非native40字段在新入口保持默认，不通过别名补造；旧Converter保持既有语义。Strength decoder先拒绝非1..9索引，逐字段解码；此函数不承诺整表重复/行边界校验，后继source parser负责。原始entry字段列表由调用方提供，不串接伪造AST参与production解析。

新增C++ witness直接调用actual interaction与weapon_strength_interaction，原版DAT row parser提供合法row；测试分别核对真实ITR AST解码和明确的record字段列表解码，绝不冒充Unity旧table parser已对齐。测试完整40/19字段、strict/first整数、重复/case/未知、copy/clone/projection/fingerprint、非native旧字段保持default、indexed合法边界。新字段纯内容，不接伤害/声音/cover行为，Q06后继；单条strength对象现有list按引用传递，无复制代码须改，后继测试manager语法接线。

无新manager/queue/worker，无非战斗/GAS/Unity框架/Scene/Gen/Plugins/外部包/33ms/十一阶段变化，不升版本/部署资源。先native witness与RED，再写生产字段；若需要新增准确code-path，先补Record。回滚经批准仅本包精确增量，保留前批代码。验收为native逐字段、focused/HitPlan回归、SelfCheck/compile/Scene与Ledger；源表及完整profile仍保持pending。


## 当前进度

RED job8c2b5f706c5d451ca111b3a7948e1483为120/120缺入口/字段失败，XML保存。五脚本已写：ITR三独立字段及copy/projection/hash，strength新增11字段、完整40/19单记录解码、120项测试及native witness。源码链接内容工具build0warning/0error。未改manager或旧table parser；header索引/重复/caption的native见证已保存，不能把单记录解码宣称整表接入。


## 本轮最终证据与未验项

489/489、完整SelfCheck PASS、CS0；初次488/489为authored与native FieldBag输入阶段错误，已用真实raw-field dump更正测试，生产规则未改。138旧加载无新增旧投影差异。详细报告与失败原件同ID工件。Scene文件额外禁用两camera/一canvas，来源未确认，已保存diff并异步询问用户，未回退；不得写Scene unchanged。本Record保持FOCUSED_TEST_PASS/TABLE_SOURCE_INTEGRATION_PENDING/SCENE_ORIGIN_PENDING，完整源表/身份/Play后继。

脚本Ledger最终PASS：480 records /79 governed code files。此校验不认证未归因的Scene文件变化；Scene归属gate继续pending。

## Source/typed接线回访（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001）

当前frame来源已接入实际Logan manager，55348完整typed投影/330构建/557不同focused及SelfCheck通过，同ID REPORT为证据。本Record早先source-pending的帧层子条件现PARTIAL_RETURN；definition头部、semantic identity/联合schema、对应runtime consumer/Play仍待，状态保持FOCUSED_TEST_PASS，不把此回访扩大成整域完成。
