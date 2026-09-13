# Q05-A2 CPoint27内容契约

状态FOCUSED_TEST_PASS / SOURCE_INTEGRATION_PENDING，父Q05-A2及单一联合窗口。事前准确路径见Record；只改CPoint DTO/immutable value/canonical/adapter与新增原版CPoint block decoder。原Legacy Converter继续明确19字段准入与旧alias，改其调用到命名准确的Legacy validator，防止新模型放宽旧caller。新原版decoder从AST读取exact-case last值、strictint/float32，忽略未被原版消费的未知字段，保留原AST不改；其DTO raw只记录实际识别字段，避免把未知字段变成正式payload。

CPoint27 canonical严格依冻结顺序，三个float保存raw bits，Equals/hash一致；兼容19参数构造仍可调用，新8int以默认0扩展，旧数据整数投掷速度转成float32，这是目标类型修正。主运行时消费链BattleCpointWriter/LF2Entity直接把throwfloat提升到double，不应再取整；现有算法本包不改。新增方向/drain/gain等仅保存内容，后继Q06接线，不伪造reader。

原版入口combat_records.cpp catch_point，已核对27项且Q03/正式EXE身份不变。新增decoder仅供新内容构建链后继接入；本包不提前切manager生产入口或发布半迁移candidate。OPoint/BDY/weapon-strength/profile/semantic identity留同父Q05窗口，当前版本不升、不部署Q07。

验证：先RED类型/字段/decoder缺失，再原版37数值见证→DTO/value/canonical bit；27字段顺序与复制、负零相等/hash、unknown/case/repeated key、旧入口保持；既有CPoint内容/throw回归及完整SelfCheck。仅调整受27单元ABI直接影响的旧canonical断言，不删行为测试；必要编译问题先追加准确范围。完整Play与快照版本联合验收仍父Q05出口，不能用本包测试宣布完整运行时对齐。

无新生产队列、服务或生命周期owner；加载期decoder不进入tick。保持GAS/Unity/非战斗功能、33ms/十一阶段及Scene/资源。回滚经批准仅逆本包增量，保留前批未提交工作，不使用破坏性Git。脚本改前记录；改后追加真实证据与限制。


本轮110项实际通过、完整SelfCheck PASS；报告同ID。CPoint typed/canonical已改，manager接线与外层身份仍后继，保持未关闭并在A2/Q05出口回访。
