# H06 现有Role限定生产准入：未通过 / BLOCKED

2026-10-09 收尾结论：**四场景完整Driver逐tick一致性通过；OPointBurst-shaped可靠collector 0GC失败，必要门未齐，生产默认仍Brute。** D1=A授权有效，阻塞不是整tick未达33ms。不能声明“Role限定准入完成”。H06父项/H07/Android仍开放；95默认OFF及其独立GC FAIL保留，未恢复Goal或34项自动队列。

## 诊断清理纠正

调用栈UnityTest因历史导出错误而中断后，未执行到完整finally，留下Profiler.enabled=true。收尾检测2ed7ca384e344d21adb7a3a276e2f36b如实FAIL。显式恢复job0e39de1830774390adb5b145a6d8000e 1/1 PASS，恢复前后见profiler-owned-recovery-b6c0e27ebda04847a89647d40bfb84c7.json：Profiler/driver已false，deep/GPU未启用，profileEditor/allocationCallstacks复位false。初始RequireAvailable只证明录制/deep/GPU关闭，全部Profiler设置未事前落盘，因此不能声称逐项原样恢复（CPU area/memoryRecordMode等联动也已在前后快照记录）。未清空旧历史，部分导出raw继续保留。后续诊断入口保持Explicit并先持久化settings/检查导出错误，本轮不重试调用栈。

## 当前有效证据

| 门 | 本次结果 | 准确范围 |
|---|---|---|
| 四场景强一致 | PASS：每场景32 paired ticks，共128 paired / 256 Driver steps | D/C/Concentrated使用qualification02三PASS；OPoint用qualification03修订后一PASS。每tick输入、extended/lockstep各域hash、native完整RNG、legacy state/calls、ordered candidate handle/payload、实体数/队列/实际collector一致 |
| 候选数量 | D 105751；C 34464；Concentrated 76682；OPoint 34736 | 各场景32tick候选数量之和，不是unique pair或性能单位；Role实际3，Brute实际1，Configured接线 |
| 实际OPoint生成 | PASS | 初态984 Sakura AI，tick1前确认16空slot后经原queue/Driver flush生成至1000；新增slot类型/OID/team/relation/handle、后续每tick两后端一致。受控生成，不冒充自然技能或正式EXE千人trace |
| 容量与关闭 | PASS（本次具名范围） | 三个prepared数组身份/容量逐tick稳定；原helper退出objects/claimed slots/borrowers均0，shutdown非Failed。collector fixture封存后无新拒绝，注销objects/slots0 |
| collector可靠0GC | **FAIL** | 同25四形状，Configured Role、无hook、4warm+8sample，前后1MiB正例/空例校准有效。D/C/Concentrated各8/8零事件；OPointBurst-shaped各234事件，unit=TimeNanoseconds，不可解释为bytes/GC停顿 |
| 分配定位 | snapshot0 / collect234 / end0 | 失败形状单独做Role/Brute同夹具三段；Brute collect也分配且256槽饱和，不能写成精确256。不是Role独有的证明，但仍不能豁免候选必要0GC门 |
| 分配调用栈 | 未取得 | qualification05在导出既有Profiler历史时Unity报frame6>2GB，未获有效目标marker/调用栈。新prior-history.raw为部分失败输出，不能冒充完整备份；未清空历史绕过 |
| 当前同配置实际收益与full Driver0GC | **未运行** | 必要collector GC已失败，未改Suite、未进入120+180窗口或1800正式窗；81/91/93历史结果仅按原范围复用，不能替代本次受影响门 |
| 默认推广/实际新默认 | **未实施** | Profile与Suite SHA保持事前值；Brute回退保持，所有未准入开关仍OFF。无新生产代码 |

## 失败记录与实际调用

- job95e61a1efb0e405482843dd931d382a0：0用例，首次请求先于新程序集加载，保留qualification-00-empty，不计通过。
- jobedb86e0ab3c64c1db570e09f9e9dad33：4FAIL；观察器只读Store而fixture实际LegacyOnly，非空断言拦截。原件qualification-01-observer-fail。修订观察源，不改生产，不删除断言。
- job127b5ae02e5d4fcb9ccb54f31a8895a5：3PASS/1FAIL；三个场景一致性有效。OPoint原tick8空槽假设错误：自然AI已把984填至999，slot984为SpecialAttack。原件qualification-02-opoint-type-fail。
- job6bd9788e9b0f424cbb8855f0ac642b27：4PASS/1FAIL；只跑受影响OPoint及四collector GC；OPoint和三个GC形状通过，混合形状234事件失败。原件qualification-03-gc-fail。未重复已通过的三个Driver场景。
- job6afa239923cb418081ba767ec54141e6：1PASS，仅分配阶段定位成功，不是0GC通过；qualification-04-phase。
- job30d28fef30a6461bb778a62da7934c8b：1FAIL，Profiler历史导出>2GB，qualification-05-profiler-history-fail。未重复同版长窗找PASS。

全部通过原Editor PID74724 / MCP localhost6402调用：`manage_asset(action=import,path=Assets/NTSD/Scripts/Test/Editor/BattleBruteProductionAdmissionEditorTests.cs)`，确认新DLL晚于源码且reload结束，再`run_tests(mode=EditMode,groupNames=具名正则)`；使用`get_test_job`与最终XML双核验。正则依次为`^NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests.H06RoleAdmission_ConfiguredFourSceneDriverParity`、`^NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests.H06RoleAdmission_(FourShapeCollectorZeroGc|ConfiguredFourSceneDriverParity.*OPointBurst)`、`^NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests.H06RoleAdmission_FourShapeCollectorZeroGc.*True`、`^NTSD.Test.Editor.BattleBruteProductionAdmissionEditorTests.H06RoleAdmission_OPointCollectorAllocationCallsites$`。同步测试内MCP查询超时仅作通信记录，没有重启Editor或重复派发。CLI `unity status --project-path ... --format json`无Pipeline实例不代表Editor关闭。

唯一修改C#为BattleBruteProductionAdmissionEditorTests.cs。Task/Change均在脚本前登记；15份真实dirty备份及1150项保护指纹见Operation；无Scene/Prefab/资源/ProjectSettings/InputActions/Server/Gen/Plugins写入，无Git写操作。准确保护复核见closeout-audit.json，最终Editor释放与validator结果另追加。

## 复用与未执行边界

25保留四形状局部candidate/RNG结果，其失效GC计数不复用；82保留有限正式root各12tick、D/C逐tick域与容量证明，不冒充全域。81保留当时同配置收益和full Driver0GC；当前query/Suite指纹差异已单独记录，未用历史均值替代当前P95。86独立表现域三源同SHA，复用既有容量/关闭证据。91/93实际窗口有效GC与95 Manual/headless失败分开保留。fallback关键具名方法字节与81相同的子范围复用，整文件变化不冒充同指纹。详下方原矩阵及batch81-source-fingerprint.json、batch81-symbol-fingerprint.json、fallback-symbol-reuse.json。

本次没有新的正式EXE全域trace、正常物理按键Play验收、实际渲染收益证书、Windows H07阶段PASS、GPU batch或Android认证。33ms、无持续backlog、正常dropped tick0与Android标准不降低。

## 后续建议

H06当前只剩继续定位混合形状共同collect分配并验证原门、再进行实际同配置窗口和默认接线；不要恢复网格/Burst/Jobs/AI降频或读取被排除Q06来绕过边界。Profiler历史无法完整导出的问题保留，若必须清空/替换历史或进入Q06方法体，应先提出准确操作/阅读范围。

下一项H07-INPUT建议不依赖Role已默认：先使用91已有父子计时，选一个重复capture/同步热点，冻结字段owner、失效时点、读写与checksum/RNG不变量、容量/关闭及测试范围；收益无固定承诺，同窗父子计时不相加。95的GC失败约束相关Manual夹具，不能覆盖实际窗口。新实施待用户决定，本轮不自动继续。

边界审计：一次邻近范围读取包含Q06平台方法片段，后续通用CollectCandidatesForPair提取也含Q06历史合同标记；已停止此范围，未据此修改或解冻Q06，详Operation纠正记录。不能把更早“未读Q06”记录当作全程事实。

---
## 事前调查与执行历史（以下按当时时点保留）

# H06 Role 限定生产准入

当前：IN_PROGRESS / DEFAULT_NOT_PROMOTED。用户方案A已批准；整tick33ms不是本子阶段阻塞，H06/H07父项与Android标准保持。旧自动目标不恢复，95默认OFF。

## 只读基线和生产入口

原项目Unity2022.3.62f3，PID74724，MCP6402。CLI status无Pipeline实例，不能据此认定Editor不存在；直接既有MCP确认原Menu唯一8roots、clean、非Play/非Paused、idle、无测试/编译。另一个test项目Editor不是本任务载体，不操作。正式根EXE SHA-256实核336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3。

GameConfig.asset的BattleCollisionBroadphaseName当前为空。BattleRuntimeProfileProductionSource.Resolve读取显式命令行与配置，CollisionBroadphaseBackendResolver.Resolve空值当前BruteForce；SimulationTickDriver创建带该backend的World，BruteForceSceneQuery.ResolveFormalCollectorMode按backend选择现有Role或Brute。LooseQuadtree是配置名，不代表每tick都进tree；Role有Direct/Sweep/tree与平台/特殊保守回退。1050 slots / 1000 active为MobileExtended，不能超额测试却称1000合法资格。

所有dirty已保留，15准确路径before备份、1150保护项及HEAD/staged身份见同IDOperation。无Scene/资源/设置/InputActions/Server/Gen/Plugins写入；活跃Q06方法体未读。

## 复用与缺口矩阵

| 门 | 已有证据 | 可复用范围 / 真实缺口 |
|---|---|---|
| 四场景candidate顺序/handle/itr/RNG | 25四1000-participant局部collector | 局部证据保留；OPointBurst-shaped不是真正生成，不能当完整Driver四场景 |
| 逐tick声明hash/双RNG | 82两root各12、D/C各32；88paired/176steps | 有限正式root字段与D/C诊断资格；没有集中/实际OPoint四场景全域证明，当前新资格补该缺口 |
| 收益 | 81四同版120+180，Role CandidateCollect约53/56→14/15ms，logic约83/86→44ms | 有效历史同配置收益，不能当当前字节精确性能；freeze6路径中manager/harness/App/Driver同，Suite/query不同，见batch81-source-fingerprint.json |
| Full Driver 0GC | 91/93四实际窗口，每个300accepted/180steady，前后正/空校准有效、0event | 真实主线程managed scope，不是GPU/native/全PlayerLoop。93当前Suite SHA同DF63…8013，但94/95当前query/World等变化需受影响核验 |
| CPU表现容量/0GC/重入 | 86两1800camera/3600observer，draw5840、Foot/Health2/2、growth0、两次关闭三0 | 相关三源指纹同；可复用H11独立表现域，不冒充Role或千人0GC |
| 生命周期/关闭 | 81/82/91/93各具名owner后置；95也保留其诊断三零 | 当前配置候选仍需完整shutdown后置；不重跑无关camera域 |
| 95 GC失败 | Manual/headless OFF/ON两千人采样非0；候选默认OFF | 不覆盖91/93实景0GC，反之也不豁免95；本任务不偷偷启用95或修改AI |
| fallback/容量 | 25已有异常/occupancy/RNG回退与generation reuse；82三个prepared数组稳定 | participant conservative fallback是合法路径，不等于whole collection abort。93 group-rejection是阵营过滤计数，不是容量拒绝；不得混写 |

最终准入需要当前受影响正确性、适用完整0GC、容量/关闭及可解释同配置收益。当前没有新性能/设备证书。

## 本次最小资格实验

唯一测试脚本BattleBruteProductionAdmissionEditorTests：新增四场景32tick配对，用现有正式Sakura fixture和完整Manual Driver。backend按World冷配置选择，FormalCollectorMode保持Configured；不强制Direct/Sweep/tree。不改变原82入口及Raw正式校验。D/C实际1000AI；Concentrated1000近距；OPointBurst先984，固定tick8向已有logic OPoint queue提交16受控任务，以实际Driver flush/materialization补缺，不称自然技能或正式EXE千人trace。

每tick记录完整输入、extended/lockstep hashes、完整双RNG、candidate store顺序/handle/完整payload摘要、实体数、fallback、实际collector；检查原三prepared数组身份/容量并沿用原11阶段关闭。只读hook会阻断zero-ITR快速返回，这轮不是无hook性能或0GC证据，须分开判断。无新增runtime状态/缓存；测试观察者finally解绑。

首次job95e61a1efb0e405482843dd931d382a0实际0用例，XML在新程序集加载前生成，保留qualification-00-empty/tests.xml，不计PASS。编译加载后jobedb86e0ab3c64c1db570e09f9e9dad33运行四具名case；最终结果待回收。

## 后继建议及依赖

## 当前资格观察修订（不替换失败记录）

job edb86e0ab3c64c1db570e09f9e9dad33 的最终4FAIL来自探针误读Store-only：实际fixture为LegacyOnly，候选数量全0，非空断言正确拦截；不能称该轮候选顺序通过。原XML/源码已保存qualification-01-observer-fail。修订只读当前实际LegacyLists或Store，每tick严格一次，保留非空断言；另加OPoint队列/新增slot身份和95默认OFF检查。

job 127b5ae02e5d4fcb9ccb54f31a8895a5 正在运行。已落盘D/C/Concentrated各32tick候选总数105751/34464/76682，逐tick input、双RNG、extended/lockstep hashes、候选顺序/载荷/handle、实体数及队列无首差。最终OPoint/XML仍待完成；不先写整体PASS。只读hook有分配，绝非GC或性能数据。

## 后继建议及依赖（原建议保持）

本任务结束后建议H07-INPUT：消费91已有父子计时，冻结一个具体重复capture/同步成本块及字段失效表再决定实施，不重做八子阶段归因，不承诺固定input收益。Role默认不是H07-INPUT硬前置；95 GC只约束所选相关fixture/路径。新路线、AI模式、空间网格、MONO/PERF/ATLAS/EXT-1仍需独立决定，本任务不实施。

## 最终检查（2026-10-09）

原Editor最后程序集19:08:37晚于源码19:08:23，reload完成；job0e10a3458abe4d9eba4b70014af23cab通过1/1（0.596s），H06RoleAdmission_EditorReleasedAndDefaultStillBrute确认Profiler空闲、测试flag释放、空配置resolver仍Brute。最终XML/源码见qualification-08-final-release-pass，Profiler读出见editor-release-and-default UUID JSON。原Menu唯一8roots/clean、非Play/非Paused/无测试/无编译；没有第二个同Library实例，没有保存Scene。

最终closeout-audit-final.json：1150保护项仅Admission测试脚本变化，SHA09B9204AA87BA4956210C10C2B5CBCDF1241A332DDBE1745F952DFB3ABC85C15；15份before备份全同。Profile仍475BD0BF6FF0D83337DB8BB3BEDEF06D30329E4B5118B5662EF4056F79A3F1CB，Suite仍DF63DFB1A4C01BAF32672022B22AA089EE9D59006822EE8C3867CFCD26188013。

实际命令`./Tools/Validate-ChangeLedger.ps1`最终exit0 / PASSED，1391 Records、9 governed diff files、0 errors、4265非阻断warnings（主要历史记录路径不在当前diff，未做无关清理）；原日志change-ledger-validation-final.txt。`git diff --check -- <本任务脚本及治理路径>`exit0，仅LF/CRLF提示，没有空白错误。未执行git add/commit/push、删除/恢复/重置。

GC失败后新增仅阶段/调用栈诊断与释放检查，未改变通过的Driver比较逻辑、collector准入阈值及生产字节，所以未重复三场景/OPoint一致性或已PASS三个GC门。后续收益/default未启动的原因是必要GC门失败，不是33ms。
