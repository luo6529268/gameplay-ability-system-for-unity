# 子批14生产窗口报告

Task NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006；Change NTSD-OPT-M03-PRODUCTION-WINDOW-014。
最终状态 SCOPED_PRODUCTION_WINDOW_PASS（2026-10-07收尾），两周期已通过。本批是既有生产路径观察/关闭重进验证，
没有新的生产性能算法修改，不含EXT1专项M0、instancing/资源/配置改变或Android认证。

## 范围与身份

- 唯一新增Editor脚本：[观察入口](../../../Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs)及其meta。
- [Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006.md)、[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-PRODUCTION-WINDOW-014.md)、[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH14-PRODUCTION-WINDOW-20261006/RECORD.md)。
- 原Editor6401/PID19040，Unity2022.3.62f3/URP14.0.11，WindowsEditor；仅原saved Battle单场景/自然Driver/原roster。
- 正式EXE SHA336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3，本批新鲜核对不变。
- Production Assembly-CSharp.dll 4215296bytes，23:08:47，SHAB2B46521BE40C2E92F546EF21F73D642E25DDB38BAD00F425EB3B6502F6754CA；本批未编辑生产脚本。
- 33ms/3ms/max2、logic bitwise/input/pass/RNG、publication只读、segment/failclosed/slot/lease、11阶段关闭保持。
- EXT1 PROPOSED / MODIFY_REQUIRED（无专项M0），MONO USER_HOLD；ATLAS bank/预算/格式/合段专项门保持。
- Q06活跃排序方法体未读取/修改；本批不把first-visible/透明重叠或排序内部宣称为最终验收。

## 实际编译与聚焦检查

既有Editor MCP `refresh_unity(mode=force,scope=all,compile=request)`，不启动第二Editor。
新脚本最初meta误为33hex，Editor忽略asset、DLL未更新；修为唯一32hex GUID后成功导入。
仅CS过滤0/Editor idle不足以证明新脚本编译，以新DLL与实际菜单/测试运行共同核对。
MaterializationReport.ToJson现有签名仅无参，探针在导入前已适配；未改生产API。
最终具名测试结果见[去重清单](confirmed-cases.json)，共101个确认通过的case：

| job / 实际入口 | case | 结果 |
|---|---:|---|
| 77e81fe0c9ea4df88701dfb58d070ce1：LatestFrame13、SubMesh6、MaterializationReport58、ManagedMemoryBoundary5 | 82 | Passed，4.3360166s |
| 6609b86402e64b6f9bc9403d40cc5111：CapacitySeal16、三个具名pure DisplayMotion检查 | 19 | Passed，1.3532022s |

空筛选job9db977c...摘要0case不计；0166abb...全DisplayMotion/Capacity筛选经过域重载后缺最终case结果，
Editor回Menu clean/nonPlay、实际Runner inactive而bridge job仍RUNNING/0progress；状态UNCONFIRMED，
不算通过/不猜测生产测试失败。保存[孤儿状态](editmode-motion-capacity-orphan-state.json)和[恢复原件](orphan-session-recovery.json)。
已有run_tests clear_stuck只标记本任务孤儿bridge job并清其SessionState，不Cancel/打断Editor或删除文件。
探针启动额外查本地TestRunnerApi.IsRunActive，metadata清理不等于真实Runner结束。
两个既有UnityTest中一项重写旧play-trace，事前已另存16byte原件；当前仍同字节/SHA，详Operation。
没有将受控旧UnityTest算作本自然窗口。

## 原Scene自然窗口

新探针通过begin/endCameraRendering只读观察已生成plan/publication/captured/backend/native descriptor；
不再Build、不取新lease、不注入输入/替换roster/配置、不GPU capture/readback。
各窗在自然tick>=8且battle allocation window打开后观察96tick，固定最大2048camera记录。
配置显示FPS=120仅是配置，不是实测120FPS或Host cadence证书。
窗口端点、反射delegate绑定、数组预分配、snapshot/JSON/Scene生命周期均在热观察段之外。

第一份corrected-02窗口详[原件](corrected-02/production-window-01.json)、[counter原件](corrected-02/materialization-window-01.json)、[派生摘要](cycle01-derived-summary.json)：

| 指标 | cycle01 |
|---|---:|
| logic tick窗口 | 8→104（96） |
| world-camera观察 | 128 |
| queued materialization Build / 成功vertex API | 256 / 256 |
| publicationChanged / alphaChanged / repeated Build | 96 / 160 / 0 |
| entity Mesh vertex bytes | 180224 |
| 单次end-camera Build payload | 16vertices×44=704bytes |
| 自然实体 / command / physical segment / chunk | 2 / 4 / 4 / 1 |
| 当前RenderPass recorded CPU draw数 | 5（含辅助，不等于GPU batch） |
| 实际观察slot | 2 |
| capacity growth / unresolved / failed / rejected | 0 |
| camera managed envelope / observer | 0B / 0B |
| ordered shutdown objects / slots / borrowers | 0 / 0 / 0 |

publication与captured为不同对象，原publication commands/order未物化、captured两者已物化；
source/display tick对应，segment command范围递增合法、texture/material有效、stride44、submesh bounds有限。
这些是CPU数据与提交侧检查，不是GPU painter order/像素正确性证明。
cycle01 alpha0.1164090913～0.9812393997，32个publication tick有多个显示样本；
首entity表现位置观察到两个值X2.1300344/2.1450346，差0.015，不能扩大为完整运动/跟手验收。
Scene SHA前后253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010，clean。

### 0GC证据边界

已有tick、DriverUpdate、LatePresentation、PlayerLoop分配计数及gen0/1/2端点差值均单独记录；
camera envelope从本begin观察结束到本end观察开始，包含该区间URP与其它callback，但不覆盖区间外工作。
所有计数不相加；诊断准备/导出明确有窗外分配。
Editor collectionControlSupported=false、playerLoopHardGateSupported=false；不证明其他线程/native/GPU/Player、
完整渲染链、1000AI或Android的0GC。CPU read lease归零也不证明GPU consumer完成。

## 新确认的M-03热点候选：同显示帧两个物化入口

本次重新扫描：SimulationTickDriver.cs:491–512 LateUpdate→World.PresentLatestFrame；
SimulationStageRenderModule.cs:438–442→FlushLatestPublishedFrame。
BattleCentralRenderSystem.cs:486–493显式force物化（unityFrame=-1）；
BattleRenderFeature.cs:68–85 AddRenderPasses→MaterializeLatestPublishedFrameForCamera→acquire/enqueue；
BattleCentralRenderSystem.cs:552–565 camera物化用Time.frameCount、force=false。
TryGetReusableBackend:2033–2053排除current并要求slot可复用，不能把两次构建的偶数采样误当只用一个slot。
cycle01全部128样本begin generation→end generation+1、slot0→1、两Mesh身份稳定，
counter256Build=2×128camera样本。初v1亦270Build=2×135样本。
这是当前自然小roster的两物化实证，不是全场/所有配置“一定重复2次”的泛化，
两阶段alpha取样时刻不同，尚不能无条件认为结果相同/直接删除早期刷新或承诺50%收益。
下一候选应先审查早期plan消费者、辅助层、publication age/first-visible/failclosed及legacy分支，
test-first后尝试相机前只排队、不重复几何；不得改逻辑、插值取样语义、segment或直接合并slot。
A1 dirty-chunk跳过仍为未来设计；本批不新增第35项，不将候选等同已实施。

## 失败与修正原件

- 原natural01 probe[FAIL原件](production-window-01.json)：tick8→104/135样本，270Build，生产字段/关闭/Scene通过；
  唯一失败为探针强制end-only观察同时见两slot的断言。每camera两Build使end-only同奇偶slot，非生产RED。
  修正仅补begin/end scalar和实际1～2slot计数，新corrected-02目录，v1源保留，不覆盖FAIL。
- corrected-02 cycle01已PASS；自动第二轮未见Play/输出，Unity delayCall→EnterPlay衔接内部原因待确认。
  v2源备份后只加“Second Cycle When Idle”显式入口；要求first PASS/clean/ordered、second不存在、
  原Scene clean、实际Runner停止，仅接续cycle02 STARTUP或无session，不重跑第一轮。
- 两轮可不同Editor DLL，camera观察逻辑一致；分别记录compiled inputs，不能把后续版本说成此前已运行。

## 当前未关闭门

父M-03及H-11仍OPEN/RUNTIME_PENDING。尚未测子批13优化在真实高segment场景的A/B收益，
未GPU帧/透明重叠/first-visible/完整0GC，未1000AI、Player、Android、持续热稳态。
未做正式EXE同输入全World checksum/Replay、完整SelfCheck；本批不是战斗规则修改，不宣称逻辑重新对齐。
未改PERF/ATLAS/EXT1/MONO正文、bank/预算/格式、资源/Scene/Prefab/Settings、Server、Plugins/Gen。

## 第二周期与最终结论

[cycle02原件](corrected-02/production-window-02.json)、[counter原件](corrected-02/materialization-window-02.json)、
[派生摘要](cycle02-derived-summary.json)实际PASS：tick8→104/96tick，136camera，272Build/成功API，
publication96/alpha176/repeated0，4352vertices/191488bytes；其余2entity/4command/4segment/1chunk与cycle01一致。
alpha0.1108969703～1.0，40个publication tick多显示样本；全部begin/end generation+1、slot0→1，
Mesh身份-2438436/-2438448，两个slot已观察；lease0仅CPU观察，无GPU完成主张。
observer/camera envelope、tick/Update/Late/PlayerLoop端点与gen0/1/2全0，硬门支持false。
所有范围/绑定/stride/bounds/publication隔离检查通过，zero growth/unresolved/fail/reject；
两次关闭三残留0、Scene clean/SHA同，已回原Menu clean/8roots/idle/nonPlay。
确认101个case与两自然Play窗口；原v1 FAIL与自动重进衔接未知不删除、不伪报为生产错误。

最终Editor DLL53AAAC65.../6175232bytes（23:58:13），source8D9A6901...、production DLLB2B46521...不变，
详[最终指纹](final-compiled-inputs.json)；cycle01旧DLL99C80B77...与source91F106E5...证据亦保存。
末次编译后仅增加窗外idle菜单，101回归在此之前运行；没有宣称重新跑101。
本Change VERIFIED仅限定新增Editor入口的验收；父M03/H11及生产高负载收益/全链/设备仍开放。
下一批先闭合早期plan消费者与采样边界，再test-first选择最小物化消重，不引入instancing或新架构。

## 最终静态/留痕验收

实际命令：`Tools/Validate-ChangeLedger.ps1`、`git diff --check`、`git diff --cached --name-only`、
PowerShell `Get-FileHash`与链接解析；已有Editor MCP `refresh_unity`、`run_tests`/`get_test_job`、
具名两菜单、`get_editor_state`/`manage_scene`/CS过滤。没有CLI第二Unity/Build/Profiler/GPU capture。
ChangeLedger exit0/PASSED，1310 Records，7个现有工作树governed script均COVERED，0error；
全库仍4268warning，本Change/新script相关0，未清理其他历史记录，详[原件](change-ledger-validation.json)。
只读静态核对447protected/7准确owned backup全部保持；另trace/v1/v2三个准确备份均同预期SHA。
compiled/source/Scene/GameConfig/正式EXE身份无漂移；HEAD同2cccd597...，staged空。
34条仍高12/中14/低8、父关闭0；链接0缺失、git diff --check exit0、script尾空白0，
详[静态结果](static-validation.json)/[附加审计](supplementary-audit.json)/[Editor最终状态](editor-final.json)。
旧play-trace普通测试时间戳变化但16byte原内容同SHA，单例外事前备份及本批after原件保留；
没有还原、删除、移动或覆盖其他旧证据/用户工作。after.json在全部产物落盘后冻结，不含其自身。
使用unity-cli skill按Unity2022已有连接回退原Editor；2d-pixel-perfect skill约束保留现有URP/UV/stride44，
未调整像素采样/相机/纹理导入设置。本批无生产算法改动，回滚需另授权，不以HEAD丢弃dirty。

