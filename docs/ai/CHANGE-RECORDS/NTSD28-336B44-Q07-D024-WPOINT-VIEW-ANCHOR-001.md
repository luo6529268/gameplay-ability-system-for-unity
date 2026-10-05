<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs
authority: User D-024 all-entity projection with BattleVisualScale 1.5 preserved; current formal 336B44 BattleWorld28::settle_held_refill_objects and render_snapshot WPoint screen alignment
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001.md
-->

# D-024 持有武器显示挂点补偿

2026-10-05 后继只读插值消费者核查：[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/INTERPOLATION-CONSUMER-AUDIT.md)确认当前正式构建路径与Unity相邻/身份/六关系/连续性/源取整后投影合同对应；挂点补偿只在完整快照物化一次，显示delta只改命令，没有新已证首差。不加“父实体强制跟手”或再除原生速度倍率；本轮未跑测试/Play、未改脚本。R120持有画面/GPU仍未知，本Record状态不晋升，也不自动追加ONE。

2026-10-05 后继原Scene消费者已限定通过：独立 `NTSD28-336B44-Q07-D024-WPOINT-SCENE-PROBE-001 / VERIFIED` 在原Battle Scene自然拾取global5→7的不可变快照→中央命令测得双方WPoint同点X0/Y0；动作115/24、sourceX201/214、Y0/7、互持关系保持，sourceZ481/482由项目地图钳制。[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/SCENE-ACCEPTANCE.md)。这替代本Record“原Scene命令未验”的旧快照；GPU/真人键/其它显示政策等仍未知，生产状态RUNTIME_PENDING不变，不扩大矩阵。

脚本修改前建立。已测[原Editor自然pickup两tick](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/REPORT.md)恒等两点重合、固定视野X差6/Y差4.04657；同正式frame115/24和sourceX/Z成立，尚未测GPU。用户已授权通用比例处理、最终出口修复且禁止DAT修改，并确认保留本体1.5倍显示；本修复属于共用呈现消费者，不改任何源战斗规则。

准确代码范围仅`LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels`：沿用原关系/点存在校验、原纯本地1:1 helper及恒等分支；当共用world projection非恒等时，读取双方已同步视图X/Z整数及源Y，用当前`SourceDeltaToViewY`转高度屏幕差，再将旧补偿纠正为`保留的本地图片WPoint差*visualScale - 实际已投影屏幕位置差`。中央snapshot及Legacy现已共用这个入口，无需各加倍率或改调用者。正式source字段、physics、碰撞、frame、link/holder、RNG、DAT、图片、相机、Scene及非战斗保持；没有新运行时模块或关闭阶段变化。

风险：局部挂点补偿与cover、整数显示位置及朝向耦合，不能把源点全部乘1.5或只用倍率忽略整数误差；非正式初态夹具可能未真正贴齐，恒等分支保持旧行为。前置关系检查保持，release/missing point仍返回0；渲染FrameDelay/RenderOffset等其它效果不由此重新定义。初步验收是原两参数例RED→GREEN、取证CSV除补偿/最终点外源/view字段不变、生成/原Editor0错、Scene clean/SHA保持、Ledger/diff通过。原Scene/GPU、左朝向等未覆盖条件如实保留，不以两例全阶段关闭；新首差才扩邻例。

回滚只手工移除本方法新增投影补偿块，保留已有纵向投影和其它用户修改；先保存实际diff/证据，不使用restore/reset/clean。脚本前PLANNED事实保留；现已CODE_WRITTEN，待编译和同两例GREEN。

实际改动仅声明方法：保留关系校验和纯本地点helper，非恒等world projection按双方已同步XInt/ZInt与源YInt求画面差；旧offset增加本地点差减实际画面差。没有规则/数据写入、额外倍率常量、对象特例或新生命周期模块。

生成工程验证：`dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`，退出0、334 warnings/0 errors、12.35秒；`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot ...`通过1269 Records/19 governed diff files。原Editor MCP已请求刷新；同两例GREEN与原Editor编译核对待验。生成编译不等于运行时对齐。

2026-10-05 后继GREEN：原Editor MCP刷新后Console0error，正式程序集更新；精确同两例job `c4e6daee940d483bbc32735e7242d0ce` 终态succeeded、2/2 PASS、46.7945989秒。固定视野X6/Y4.04656982421875→0/0；恒等CSV所有字段不变，配置CSV仅offset/武器最终画面点/差值六字段改变，所有记录源坐标、frame及view位置不变。[原件与比较](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-VIEW-ANCHOR-20261005/REPORT.md)。一次查询超时后只重取同job，无重复运行；发现8839不是本包执行数。Scene clean/nonPlay/root11/SHA253B2EB...9010保持，Console0error。聚焦几何门通过；原Battle Scene自然拾取画面/GPU、左朝向等未覆盖，状态RUNTIME_PENDING，不能由两例关闭整个阶段。当前无新增强制ONE；真实非例外首差或终验必要时再开单项。
