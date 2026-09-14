<!-- CHANGE-RECORD
id: NTSD28-Q06-TYPE5-UNARMORED-UNITY-001
status: VERIFIED
change-kind: NATIVE_TYPE5_UNARMORED_TRANSACTION
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Hit/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06Type5UnarmoredEditorTests.cs
authority: Current playable type5 unarmored transaction; TYPE5-UNARMORED-SOURCE-WITNESS-001 585 vectors/14048 assertions, repeat SHA c164b073b4ee789df121f18dcffe8253d0771c21c37703eca35b2e73d4acf30e.
evidence: Actual old type5 backend thresholds50/30/10 and cleared80 disagree with native40/20/0 and retained80; Shadow non-character router has no type5 writer prediction.
-->

# Type5普通受击接入

IN_PROGRESS / TEST_FIRST_ONLY。准确只新增Editor测试，复用已验证weapon fixture构造和完整source before/after/extra/rest/spark/audio/RNG/released-hold-finalizer比较，type5共585行×两profile×direct/Shadow。保留每candidate真实prelude/writer一次的要求，不降低旧Bdefend256断言。

源模型先于生产。输入零差异且实际RED后，另追加精确DamageWriter/HitPlan路径与副作用，不能将旧type3或weapon事务直接套用为type5。验收须含完整585/原256、相关回归、自检及必要Play/关闭/回放；未完成不标VERIFIED。当前无框架/Scene/资源/Server/schema更改，禁止computer-use。回滚仅本差量，已有用户和前批修改保持。

生产修改前准确范围：四组585 before0，direct每组2277差异、Shadow2862（额外585 writer观察缺口），headless退出2/四FAIL已归档red。根代理负责BattleDamageWriter：仅type5普通分支、反应/动作/音频/水平垂直/rest/post顺序；复用现有HP/status和native weapon的机械/post helper，必要改名/补timer80 guard但武器语义不变。worker独占BattleEcsHitExecutionPlan：新增type5 CanProject与独立writer预测，复用scalar capture/Native同步cursor、现有HP/rest/spark预测，不读生产输出冒充预测。无新persistent字段/worker/queue/lifecycle模块，无架构扩张。验收585×4、Bdefend256、既有武器2100和源/回归、自检及运行，旧类型3路径不顺改。回滚只本差量，保留其它已验批次和用户修改。

CODE_WRITTEN（待编译）：DamageWriter在type5复用原HP/status后进入独立NativeType5HurtTail，阈值/计数/动作/参考平面/音频/水平垂直/rest/post按当前源。原weapon两个private机械helper改通用名称，水平稳定化显式要求timer80；weapon原timer80保持。仅type5不发旧通用音效，其它type3不改。复杂broken-armor需要进入命中时的selected route上下文，未仅凭runtimeArmorHp=-1猜分支，明确保留为父noncharacter reduced上下文依赖，不声称585覆盖它。Shadow worker正提供只读patch，尚未集成。

CODE_WRITTEN：根代理已审查只读worker原型并重写集成type5 CanProject/独立Projection；以当前DAT5分流，无CLR类型依赖，首次只接受无armor并保留原feedback owner；其它armor上下文仍为父任务。复用现有Native同步scalar观测，不扩schema；585之外的status gain/dx等尚不由该Shadow snapshot完整比较，不以mask0扩大声明。独立reviewer审查DamageWriter中。

独立只读reviewer核对声明无armor普通type5范围：未发现阻断性生产错误；未运行Unity，不能当运行通过。明确额外未验证分支：prev13/snapshot12/非零reference/正向held child rests；source非角色matched3005/3006早返而现Unity仅type3入口的既有差异也未闭合。原weapon破甲-1推断是父上下文风险，不在本新分支复制。根代理后续如关闭本包，只能限定已验证普通事务，相关状态/armor/领域父项保持OPEN。当前GUI Editor已由外部重新打开，本轮预检阻止第二实例启动，已转现有2022 Editor MCP继续。

FOCUSED_TEST_PASS：c6502f38b24b4ac19d8162985d280a13终态22/22，type5四组585 before/after0、Bdefend256四组全PASS（原16观察缺口已清），武器14回归含2100与回放/旧oracle全PASS。当前同一test文件新增14个本地回放场景和Play两factory×585×directShadow=2340的request adapter；只复用既有fixture/assert与pool/scene-checksum guards，生产不再变。新增验证待执行，不将22/22扩大为完整runtime。

第二轮50项终态46PASS/4FAIL，4FAIL仅完整984既有noncharacter reduced108（每组846差异/108case、Shadow额外0）；type5四585、两新local replay（14case/28replayed ticks）、原34、684早期及原前置回放均PASS。完整SelfCheck已fresh请求。

VERIFIED / DECLARED_NO_ARMOR_ORDINARY_TYPE5_SCOPE。完整SelfCheck2026-09-14 14:21:38Z PASS；真实NTSD_Battle Play14:24:37Z两factory×direct/Shadow×585=2340 PASS，before/after0、Scene checksum保持、Renderer2→2。14:24:58Z有序关闭PASS：原位restore4→4，World/slots/logic/render borrowers全0，连续两帧Stopped；Editor退出Play、Scene dirtyfalse/root14/hash bcd1047b…保持、生产hash未变。585之外matched3005/3006、armor/参考平面等依赖未关闭；此状态不得提升成全type5或全对齐完成。
