# NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-001

状态：RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS；原Editor六项RED五预期首差，修后五新参数＋SelfCheck局部路由6/6、907/190唯一完整Driver一步1/1通过，必要ONE完成按REUSE。两个脚本内只移除共同多余Y cap和提前YInt，不改DAT/非战斗；源规则/保护/运行证据及修前完整方法1902初始化错误的限制见[Record](../CHANGE-RECORDS/NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-001.md)及[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-20261005/REPORT.md)。自然入口/根EXE同初态/GPU未知，父Q及总目标开放。下方为事前合同及保留的更正记录。

## 权威、差异与所有权

正式根EXE仍336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；native_ai.cpp543～559的2/4/12只精确Y顺序±1及Vy除1.4，562～588只X/Z clamp与2选帧，没有Y上限1.4或整数Y写入。tickdriver416→577→600，通常FrameMotion仅写motion；physics_integrator105～108先读旧整数Y做摩擦，121～122才积分精确Y，421尾部同步整数。Unity共同RunHitFa2Or4Or12Or14FrameLogic当前额外Y cap及提前YInt=(int)Y；CharacterMechanics.StepNonCharacterBattleLogic497～512本来同样按旧YInt判断摩擦，LF2Entity6153正常physics尾部已有同步。无平台链接/状态teleport的907/190可显出AI后整数-1误0，继而Vx9误8。独立只读审阅已闭合该链，没有改脚本或运行Unity。

当前catalog907/type3、wind190/hitFa12/state3006/wait1、无Motion/OPoint，正式Genma901/333静态OPoint出生链确认；不把静态可达称自然Play。219/type3/e0/hitFa4是受控正HP邻例，其已知自然hitFa5出生HP0，不能据其自然资源链声称正HP纵向可达。518/1是type3/hitFa2邻例。207/115/hitFa14为上一包保持控制。

## 准确脚本修改清单

- Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs：仅共同2/4/12尾部删除额外Y cap与提前YInt同步；原±1比较顺序、Vy当前等价倍率、14排除、source X/Z、目标/HP、4严格回收、clamp/facing及2选帧不变。只加一行合同注释；不搬动正常physics同步。
- Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs：新增IndexedHitFaVerticalFollowKeepsIntegerYUntilPhysics五参数例及IndexedHitFaVerticalFollowFrictionSurvivesOneFullDriverTick一个必要完整方法。旧14方法的两个2/12控制当前期望整数-99保留了旧错误，改为共同初始整数保持；旧14生产证书及其五正例不撤销、不重跑整组。

## 最窄验收

测试写入前补充：同一预声明完整Driver方法也加入本轮RED，与五参数共六项，在改生产前取得真实Vx9误8的结果；GREEN仍只同五例＋局部SelfCheck，必要完整方法只在修后复验一次，不新增其它初始化/场景。

RED只新增五参数例：fixed2048/1152、当前正式catalog、活跃type0异队目标99/slot0，subject slot50正HP/无平台。907/190初始Y-1.25/Vy-2.8/Vx9、targetY100/sourceZ差4，应AI后Y-0.25/YInt-1/Vy-2，再调用既有mechanics(gravity0)得Vx9/Y-2.25/sourceX+9；907正Y3.75/target100应4.75、整数3保持；518/1正Y3.75/target0应2.75、整数3保持；219/0跨零受控正HP应-0.25/整数-1，整数dy101避开catch；207/115跨零14应Y/Vy/整数/action保持。每例finally注销，不写DAT/Scene。

RED确认后只删除两处共同写者并更正两个旧控制断言。GREEN五例＋既有ExistingLiveHitFaRepresentativeRoutingRemainsValid局部SelfCheck入口；不全SelfCheck/全EditMode/角色矩阵。生成dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly，现有MCP refresh后核新DLL/Editor idle/nonPlay/干净Scene，再精确run_tests。

必要完整Driver仅907/190一个方法，复用旧schema/seed/诊断Stage23/部分target初态/currentLoganRuntime/项目mode Asset wrapper。注销旧875，注册907同slot1、更新Roster新的OID/StableId；catalog Type3及注册后共同type resolver3，frame190 motion0/noOPoint/state3006/wait1/counter0/无平台/参考0。sourceX400/Z600/Y-1.25/Vy-2.8/Vx9，targetX400/Z640/Y100，唯一StepOneTick后action190/Y-2.25/Vy-2/Vx9、NativePreviousY104=-1、sourceX409及view比例、target/count2；Z加速沿用当前float常量，数值用1e-6容差，不称其精确double已对齐。正常wrapper三关闭计数0与发布恢复。前置断言失败保留原件，不改数据来让测试过。

## 风险、边界与恢复

RED后、再次脚本修改前登记：jobfb98a1a4f9624902ad202d48b28f059e终态failed/completed6，四参数预期首差及完整tickVx期望9实8，共五失败，14邻例未报失败。独立只读审阅另发现新完整方法机械复制帧号115→190时误把1152改为1902，而断言仍按1152；这是测试初始化错误。按原Task修回2048/1152，与生产两写者删除及旧两个控制期望一起在本两脚本范围更正。修前完整RED只用于不依赖DepthScale的Y/摩擦首差，不能声称它与修后完整初态逐项相同；五参数始终1152、源Z差40的隔离判据及Y/collision reference不受错误视野高度影响。保留RED原件，不改DAT/规则让测试过。

旧整数Y必须留到正常物理阶段，提前更新会改变摩擦/历史整数及同pass消费者，这是正式规则修复的预期副作用。没有新增module/queue/worker/pool/cache或改变十一阶段关闭合同；所有原dirty、33ms/F5、GAS、Scene/Input Actions、DAT/图片/1.5倍显示/相机/背景/地图/项目模式及非战斗保留。

逐SHA before备份、Git、四保护/五权威/六DAT两端见artifacts/diagnostics/NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-20261005/before-manifest.json及Operation NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-001-EDIT-20261005。本任务不得整体恢复HEAD或丢弃既有增量；回滚只能另有精确授权/新Operation后按本包before逐hunk恢复。失败/超时同handle观察，不自动开第二Editor/重跑。外部build脚本已在上一包观察变化，当前before重新取快照，只证明同版EXE与相应规则文件参与路径，不自动晋升全source树。

自然Genma/219原Scene、根正式EXE同初态/GPU/设备键未知；必要出口完成后REUSE，只有真实首差/相关改动触发，不把RUNTIME_PENDING当必跑角色矩阵。父Q07/Q09/Q12及总目标开放。
