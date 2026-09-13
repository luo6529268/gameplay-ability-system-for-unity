<!-- CHANGE-RECORD
id: NTSD28-Q05-CPOINT27-CONTENT-CONTRACT-001
status: FOCUSED_TEST_PASS
change-kind: CPOINT27_CONTENT_VALUE_AND_DECODER
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointCatalog.cs
code-path: Assets/NTSD/Scripts/Simulation/DataContracts/CatchPoint/BattleCatchPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/LoganCombatRecordDecoder.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05CpointContentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalKernelCatchPointValueSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6CpointThrowAtomicProductionEditorTests.cs
authority: Q05 active goal and Q03 frozen CPoint27 canonical contract; formal Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 playable combat_records.cpp catch_point.
evidence: RED_42_FAIL_1_PASS / FOCUSED_97_PLUS_13_PASS / FULL_SELFCHECK_PASS / SOURCE_INTEGRATION_PENDING / PARENT_Q05_ACTIVE
-->

# Q05-A2 CPoint27内容契约

状态IN_PROGRESS / TEST_FIRST，父Q05-A2及单一联合窗口。事前准确路径见Record；只改CPoint DTO/immutable value/canonical/adapter与新增原版CPoint block decoder。原Legacy Converter继续明确19字段准入与旧alias，改其调用到命名准确的Legacy validator，防止新模型放宽旧caller。新原版decoder从AST读取exact-case last值、strictint/float32，忽略未被原版消费的未知字段，保留原AST不改；其DTO raw只记录实际识别字段，避免把未知字段变成正式payload。

CPoint27 canonical严格依冻结顺序，三个float保存raw bits，Equals/hash一致；兼容19参数构造仍可调用，新8int以默认0扩展，旧数据整数投掷速度转成float32，这是目标类型修正。主运行时消费链BattleCpointWriter/LF2Entity直接把throwfloat提升到double，不应再取整；现有算法本包不改。新增方向/drain/gain等仅保存内容，后继Q06接线，不伪造reader。

原版入口combat_records.cpp catch_point，已核对27项且Q03/正式EXE身份不变。新增decoder仅供新内容构建链后继接入；本包不提前切manager生产入口或发布半迁移candidate。OPoint/BDY/weapon-strength/profile/semantic identity留同父Q05窗口，当前版本不升、不部署Q07。

验证：先RED类型/字段/decoder缺失，再原版37数值见证→DTO/value/canonical bit；27字段顺序与复制、负零相等/hash、unknown/case/repeated key、旧入口保持；既有CPoint内容/throw回归及完整SelfCheck。仅调整受27单元ABI直接影响的旧canonical断言，不删行为测试；必要编译问题先追加准确范围。完整Play与快照版本联合验收仍父Q05出口，不能用本包测试宣布完整运行时对齐。

无新生产队列、服务或生命周期owner；加载期decoder不进入tick。保持GAS/Unity/非战斗功能、33ms/十一阶段及Scene/资源。回滚经批准仅逆本包增量，保留前批未提交工作，不使用破坏性Git。脚本改前记录；改后追加真实证据与限制。

## 实际修改与验证

待RED。


## 事前补充：真实throw consumer回归

追加准确测试路径NTSD28B6CpointThrowAtomicProductionEditorTests.cs，复用原ThrowScope，增加fractional/negativezero与左右朝向实际CPoint throw输入；CreateScope只增加可选CPoint测试参数，不改生产消费算法。目的：验证DTO float提升double无取整，而不是仅断言字段类型。此项在改该文件前登记，原八项回归保留。


## 当前实现证据

RED job6e9be7abbfe940b4b322dfaf8eed355f，43项42失败/1通过，完整RED.xml已保存。实际十脚本均按Record限定修改：CPoint27/float32 DTO与value、27单元canonical、adapter复制与Legacy独立准入、新Logan block decoder、旧canonical测试两处和actual throw三组fractional回归。原版decoder仅内容入口未接manager；旧六DAT blocker保持，后继OPoint/BDY/strength/identity未完成。Unity编译/Green等待，不提前标完成。


## 最终本轮状态

FOCUSED_TEST_PASS / FULL_SELFCHECK_PASS / SOURCE_INTEGRATION_PENDING。实际110项通过（97+精确namespace补验13）、源码链接工具0诊断、旧138加载成功；710处旧throwvz已按native float32位模式分类，不能称旧数值不变。CS0/Scene clean/root14，准确十脚本，保护基线3037/3059不变与22声明变化/0缺失。完整命令/失败请求/证据/限制见本ID artifacts/diagnostics/REPORT.md。本Change保持活跃，待A2来源接线/同窗口身份及Play回访，不宣称VERIFIED或Q05完成。

最终Ledger476/66 PASS；权威72输入无漂移、EXE SHA匹配。人工复核实际十脚本diff，runtime算法未改。

## Source/typed接线回访（NTSD28-Q05-NATIVE-FRAME-TYPED-CONVERSION-001）

当前frame来源已接入实际Logan manager，55348完整typed投影/330构建/557不同focused及SelfCheck通过，同ID REPORT为证据。本Record早先source-pending的帧层子条件现PARTIAL_RETURN；definition头部、semantic identity/联合schema、对应runtime consumer/Play仍待，状态保持FOCUSED_TEST_PASS，不把此回访扩大成整域完成。
