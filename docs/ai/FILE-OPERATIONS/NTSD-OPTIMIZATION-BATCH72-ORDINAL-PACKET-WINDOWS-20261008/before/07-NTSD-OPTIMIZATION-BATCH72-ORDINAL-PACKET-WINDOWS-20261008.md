# 第72批：原序 packet 真实千人 OFF/ON 收益窗

状态 READY / IMPLEMENTATION_NOT_STARTED / WINDOWS_NOT_STARTED。来源是第70批默认OFF候选的两局部成本信号和第71批8/8完整Driver资格，不是新微候选；本批唯一必要信息是实际1000AI中是否有收益。准备本Task不增加50已执行数，Goal active，H07/H11 OPEN、阶段4/6。有效总授权合同0—8节优先，次数只审计；不重新请求逐批批准、不因窗口结束停止目标。

## 当前证据与精确脚本范围

- 71：有效5RED→8/8新5＋旧3；88配对/176完整step双RNG完整scalar/声明checksum/实体数同，两canonical千人每tickAI1000、真实packet/noFallback/冷数组identity/关闭三0。资格不能代替FPS/GC门，不重跑。
- 70：spacing120/12 local mean减少33.6389%/13.4884%、median减少30.6225%/13.7886%，包括重建；仅1000participant fixture，不重采cost。
- 68：实景性能仍FAIL、ON binding reuse0，PairExactLoop53.094/55.776ms；不重复无覆盖候选，不把CPU DrawMesh/segment当真实GPUbatch。

拟唯一C#：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs（目前SHA71B7C59B74532707D02FE1E90146EC76F67FD73638ECB3A6F3BBC04905D4EB14）。同文件现有BattleOptimizationWindowsAiSuiteRequestTests用于test-first；复用BeginSuite/BuildRequest/StartCurrentRun/ObserveRunningSample/ShutdownAndExit/OnPlayMode及现有完成/退出清理原链，实际符号已重新定位、实施前仍需核当前字节。新增唯一显式ordinal窗口owner、RunState/SuiteState冷字段、四请求routing、apply/observe/complete/restore，不建第二harness、不改旧17参数调用接口或其它窗口行为。正常、异常、外部Play退出必须幂等恢复owned flag并清owner；allocation seal、World/pool/worker和11stage关闭保持。

冻结 Query SHA394EF397D3A5C8C66635855D3D8CC98A0854C1EE9F80BED533C5699DEC33FAD8、Admission SHA88186BA3B1AEFC3F314E8459989A642B13ED403332BFB18871F2735B253EABA0、Role/Feature/GC observer/Harness/Host/Kernel/native和71原件，不修改；Q06只hash/status/已有引用，不读活跃方法体。没有对Scene/Prefab/DAT/资源/importer/Settings/Input/Gen/Plugins/Server的写授权，EXT1/ATLAS/Mono/Role-aware默认专项门不变。

## 先登记与 test-first

脚本前独立 Change NTSD-OPT-H07-ORDINAL-PACKET-WINDOWS-072、Operation，按当前HEAD/dirty重新manifest和准确备份。71执行期间外部HEAD527350af→0e580f7b已记录；不能假设HEAD稳定或复用71备份覆盖当前dirty。先保存公共XML（现71GREEN）、同文件当前Suite和治理文档、两个公共stress文件的准确存在性/身份，再写新tests；不删除未知Temp，不恢复旧缺失。当前07:46Z两个stress文件不存在是准备快照，进入Play前再核。

实施前冻结具名最窄矩阵：
1. 新入口缺失的有效RED，不以compiler error替代；四request与生产基线逐字段相同、只output不同，负index/输入拒绝。
2. 四原生产Brute默认true，packet原默认false；仅显式新owner允许OFF/ON；重入、非默认、其它候选或timer组合拒绝，不改变原owner。
3. OFF/ON apply/observe/complete，flag/default漂移fail-closed；失败和外部退出恢复，重复restore幂等、不接受伪owner。
4. 只读应用/容量观察不将重复last值冒充每tick应用总数；ON实际packet、OFF未应用，无fallback且预热容量不增长；已有冷数组可在冷起止比identity，不新增热分配。
5. 直接受影响的原request/owner/legacy BeginSuite last-param或schema检查。不得放宽旧断言、重跑71Driver/70cost或全部历史测试。新序列字段数只在其本来保护序列化schema的准确检查中更新，不用放宽数量或排除来躲回归。

矩阵实际case数在改脚本前记录；新5类别不等于已冻结或实际执行5case。资格GREEN以后只执行下述四窗一次，不开新微诊断。

## 固定实景矩阵与数据归属

原项目现有Editor确认idle/nonPlay/noncompiling、单一savedclean原Scene且无stress/suite owner；复用旧Suite临时Scene往返、不保存Scene、不启动/重启Unity。没有强制关闭其它任务或第二个写同Library的Editor。71后Menu clean8roots已证；额外Console只读查询两次超时，不能称末轮Console0，8/8当前源实际运行与idle已证；不靠重复Console观察代替实施或测量。

四窗：
- Dispersed1000 packet OFF；
- Dispersed1000 packet ON；
- Combat1000 packet OFF；
- Combat1000 packet ON。

均120warm+180sample，同一编译版本/seed/输入/canonical profile/postcommit0/base roster1000、实际1000AI，ForceBruteForce和现有四admitted defaults true；kind5/eligibility/coarseEnvelope/rejectedBinding/proofReuse/dispatch与branch timing关闭，renderer binding reuse默认OFF，不把其它候选收益掺入。两组配对request除outputPath外完全一致，逻辑33ms/fast3ms/max2、镜头/渲染/音频/worker/插值设置不改。

独立输出artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH72-ORDINAL-PACKET-WINDOWS-20261008/windows-01/，四request/report/final-checksum/logic-gc/terminal/observation与唯一suite-result；进入前核目标不存在，禁止覆盖已生成或旧批archive。两个公共stress路径只由原Suite处理本批owner终态，逐窗先保存准确产物后才下一窗，任何删除/重写必须在Operation逐文件和授权中声明。旧XML在测试前保存，RED/GREEN终态先核Unix UTC新鲜度与actualcase身份后独立ABSENT归档。

记录logic mean/P95、collector/既有phase范围、显示mean/P95、dropped/backlog、AI/baseRoster实际数、capacity/growth/packet实际应用与fallback、中央已解析/拒绝/提交和目前可得的独立unresolved/stale口径（缺则UNKNOWN），完整可靠logicGC observer前后校准与steady事件、末checksum全JSON/声明域、flag恢复和正常11stage三残留0。packet Last计数只报告被实际观察的fresh tick/collection，不能加重复读数当全窗总数；无法证明全tick应用则明确观察覆盖，不猜300次。没有新的Profiler/FrameDebugger/GPUcapture/M0专项、GPUbatch或memory byte测量。

## 裁定与真正完成边界

同场OFF/ON实际收益如实量化，不把不同历史机况/Editor窗口数/测试持续时间当收益；局部信号不保证真场景。无收益/回归或容量/正确性/GC失败则NOT_ADMITTED、默认OFF，保存失败，不追加1800刷PASS或重跑相同四窗；按新证据判断下一有据方向，不因次数停止目标。收益明确也只形成推广候选结论，生产默认改动另准确Task/Change/必要证据，不在窗口里偷偷默认开启。

完整H07阶段门仍为两个真实Windows工作负载120warm+1800sample、实际1000AI、logicP95<33ms/drop0/无持续backlog、可靠fullLogic稳态0GC、容量/central独立门/关闭0/正确性通过。H11完整物化—上传—录制—提交0GC OPEN，43旧12迟发event FAIL不能被logicGC或本短窗消去。只结束候选/子批，不误标H07/H11或Goal完成；34父项关闭数0不变。当前72仅READY、未写脚本、未测试、未进Play或测量。
