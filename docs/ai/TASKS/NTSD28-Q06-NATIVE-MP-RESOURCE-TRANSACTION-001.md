> 最新资源owner限定状态：VERIFIED；隐式零帧资格由 NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001 补证闭合。完整tick其它reader迁移未完，不覆盖下游调用链。

> 2026-09-14资格纠正：当前FOCUSED_TEST_PASS / REOPENED_ZERO_FRAME_QUALIFICATION；原有效声明帧公式/phase证据保留，native未声明0..998零帧与Unity HasFrame差异待NTSD28-Q06-NATIVE-ZERO-FRAME-CACHE-CONTRACT-001处理，旧VERIFIED文本为历史。

> 2026-09-14当前状态 VERIFIED / SCOPED_MP_EXIT。用户确认HUDBg变化为其或其他任务所为，已保留。旧来源pending文字仅历史，下一HP Task继续。

> 当前限定状态 FOCUSED_TEST_PASS；最终119/native2028/SelfCheck/scoped Play通过；Scene来源标记见父REPORT，以下启动文字保留历史。

# Q06 完整native MP资源事务及生产接线

IN_PROGRESS / TEST_FIRST；准确五脚本Record已建立。前置RESOURCE-MP-FIRST-DIFFERENCE-AUDIT-001已完成四次native模式原值对照、75source/header及EXE/runner身份复核，确认neutral tick3 MP差异来自Unity漏读模式抑制，而非最大MP初始化。先完整读取同ID审计REPORT及其事务表，不重做Q05或全量Q03审计。

实施必须覆盖native battle_world.cpp:2290-2381完整MP事务：world phase3、有效type0当前frame、frame.cmp/有符号右移、regen_mp各族、stats.bound/weak/阈值、basis/delta/bonus、mode原值==1抑制、F6负向消耗、helper内限幅时点。优化与legacy/派生caller必须共享同一事务；仅改最终MP值或普通+1不是出口。HP/chp、display/post-display和其他生命周期分别按Q06后继Task，不混入此次MP行为。

先声明准确脚本路径/符号、事前hash、测试和回滚，再写代码。候选范围为BattleRecoveryStatusWriter.cs、BattleEcsCharacterRecoveryPass.cs、LF2Entity.RunPreCollisionRecoveryPhase，以及新增focused测试和workspace-owned native resource witness；不得用候选列表替代最终code-path Record。已有Build-AuthoritySourceCapture.ps1支持RunnerSource和独立输出目录，可复用原native成员函数生成分支见证，不改authority源或正式EXE。

模式输入先明确设计：完整事务接收显式不可变规则值，source默认1与Q08正式mode记录注入必须区分；当前World未存28/2C，禁止偷偷新增未校验的可变字段、全局开关或把1当永久所有模式。若必须增加mutable state，先声明schema/identity/restore完整契约，不擅自另开不兼容版本。现有F6输入和status timer、NativeResourcePhase3、NativeMetadata及InputDoubleCost19C应复用。Q08负责正式模式选择投影，Q06先实现可接受任意原值的资源事务及当前默认运行路径，不得形成循环等待。

测试先RED，native实际成员函数覆盖regen值至少-7..14/未知值、mode -1/0/1/2、MP/HP边界、负frame.cmp/奇数右移、F6、弱状态、bound、51/52、phase与tick错相；两caller/fast-fallback一致。同一真实Logan scenario capture中原MP200/201数值差必须闭合，六MISSING继续如实报告；场景正常/特殊MP事务需要现有Editor桥接定向运行，禁止computer-use或把注入输入称物理按键。编译、focused、完整SelfCheck、相关回归、非战斗/Scene/资源保护、账本通过后才交付。过期测试若与当前native矛盾，独立准确Record修夹具，不改规则迎合旧测试。

修改限战斗路径，不改GAS/Mono/Scene/InputActions/资产/Gen/Plugins/外部Server。33ms/3ms与十一阶段shutdown不变，无新runtime manager/queue。回滚需批准仅本包差量。Q06仍包含CPoint/OPoint/+2F8/revival/HP/C25/pieces等后继，本Task完成不能直接进入Q07或关闭总目标。
