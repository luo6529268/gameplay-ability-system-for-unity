# H06 Role record有限诊断：FAILED

Task `NTSD-H06-ROLE-RECORD-COST-DIAG-20261010` 本次有限执行已终止，结果 **FAILED**；Change `NTSD-H06-ROLE-RECORD-COST-DIAG-001` 保持 **BLOCKED / 默认OFF / 未完成实景诊断准入**。不是A生产方案，也不是C“路线不值得”；目前无有效ON数据支持这两类判断。无后继自动任务。

唯一combat1000 OFF窗已跑完120warm＋180sample。校准有效的完整Driver scope发现稳态2次GC.Alloc、首次tick130；独立的逐tick组合观察门另有113次失败。Suite按原合同在OFF结束后停止，ON只存在预写request，没有启动或生成report。没有第三窗、1800窗、其它fixture、Profiler清理或自动分配修复。

## 1. 实测结果与边界

| 门/指标 | 实际结果 | 结论范围 |
|---|---|---|
| 完整Driver GC | begun/ended/accepted各300；120非稳态、180稳态；invalid/rejected/sequence均0 | scope覆盖有效 |
| 前后校准 | 两次已知1MiB正控各1event；empty各0；未饱和/未wrap，capacity256 | 校准有效 |
| 稳态GC | **2event，firstNonzeroSteadyTick=130**；raw900，unit TimeNanoseconds；provenBytes=-1 | **FAIL**。不是900字节，不是回收停顿；managed byte探针0不覆盖此失败 |
| 逐tick观测 | tick1..300连续；steady恰为121..300；完整RuntimeChecksum64与双RNG标量已保存 | OFF原始证据完整；没有ON配对，不能声明双窗一致 |
| 组合实际状态门 | **113次false**，首次104；其中warm17、steady96 | **FAIL**。不可当成13/13或其它局部证据已覆盖 |
| OFF诊断复位 | 300行29计数及七elapsed均0；settingsRestored=true | 新诊断确实OFF，关闭setter/stride恢复 |
| workload | active AI/base roster最小1000；workloadValid、harnessValidity=true | 此受控combat1000有效 |
| 容量压力 | 180scope，violatingTick0，critical/reject/drop delta全0 | 此窗口容量门PASS |
| 关闭 | cleanupExceptions0；activeGO1000→0，worldObjects2000→0，entities/claimed1000→0；object/reference borrower0 | 既有有序关闭与设置恢复PASS |
| retained inactive pool | 10→1050，delta1040 | 保留预热容量，不冒充清零，不是活跃借用残留 |
| tick | average47.615979ms，P95 80.70317ms，P99 101.598721ms，max127.681ms | 从本报告直接读取的180sample统计，不从均值推出P95 |
| backlog | maximum/current backlog0，droppedBacklogTicks361 | 不满足正常dropped0；没有H07通过结论 |
| 原Editor | 返回NTSD_Menu，clean，8roots，idle/nonPlay/noncompile/nontest | 已保存新鲜after-window两个JSON |

组合断言同时检查新gate/stride、旧六timer、84/89/92/94/95/producer/input timing关闭、实际Role、非abort、fallback participant0及steady边界。现有行只保存组合布尔值，**没有保存每个失败子条件**，因此不能准确把113次全部归因某一开关、fallback或规则错误。原report的`broadphaseFallbackParticipantPeak=3`说明本窗口确有fallback参与者；12次Editor轮询的实际collector均Role且无mismatch，但这不替代300tick组合门，也不能证明每个失败tick就是fallback。没有弱化该门或追加观察重跑。

原始文件：`windows/suite-result.json`、`windows/00-combat1000-off/report.json`、其`.logic-gc.json`、`.record-cost.json`、`.final-checksum.json`、`.observation.json`、`.terminal.txt`及两份`.request.json`。Suite原状态PARTIAL/phaseDONE/completedRuns1/runIndex0，error明确为Finite record diagnostic gate failed。共享terminal的PASS仅表示runner干净完成，**不覆盖GC/观察门FAIL**。

## 2. 七段与测时底噪

ON未执行，七段没有实景成本排名、重复覆盖或可消除比例，不能把OFF的零elapsed解释成业务零成本，也不能做OFF→ON收益对照。三个60tick块只报告OFF原始覆盖：

| tick | 行数 | 组合门失败 | 七段计数/elapsed |
|---|---:|---:|---|
| 121—180 | 60 | 15 | 全0，因gate OFF |
| 181—240 | 60 | 46 | 全0，因gate OFF |
| 241—300 | 60 | 35 | 全0，因gate OFF |

Stopwatch frequency=10,000,000。冷端1024次真实空边界raw ticks：median0、nearest-rank P95=1、max17；七边界同构循环wall median4、P95=4、max494；七槽recorded sum median2、P95=3、max17。原始数组已保存；0.1微秒量化使空测量经常为0。这是冷端边界/循环底噪，不等于完整热路径仪器税，不能机械扣除；无ON信号可判是否淹没。没有乘64外推或相加父子计时。

## 3. 实施、验证与证据复用

准确写域仅四文件：

- `BruteForceSceneQuery.cs`：独立默认OFF gate、固定29个long计数、一次共享selector/stride、七互斥phase和finally结束；仅原cached record/FromSnapshot/nearest/select/write helper及采样reset。未修改store实现、sorting或consumer业务逻辑。
- `SimulationWorld.DetailTimingDiagnostics.cs`：尾部54..60七槽，旧0..53不变，Count61；原固定数组冷分配。
- `BattleOptimizationWindowsAiSuiteEditor.cs`：独立menu和固定两request；唯一组合GC+观察owner、冷分配300行/每行7long；复用原CaptureRuntimeChecksum64 schema36（完成Driver后捕获，含原FrameInputSet和runtime输入），另存双RNG标量；原GC scope前后边界不变。旧六timer不开，OFF失败不启动ON，finally恢复。
- `RoleAwareCollisionShadowSelfCheckTests.cs`：原夹具增加诊断矩阵、采样/关闭、容量、kind/RNG、副作用、fallback、store fault与warmed0GC断言。Suite原文件内另有4个request case。未新建代码文件。

所有新增状态owner/capacity/失效/11阶段位置见同Task：query/detail固定标量/计时槽，无新生产cache/worker；阶段1停止tick采样，阶段2原Join门，阶段4原解seal，阶段8原query/suite清理恢复，阶段9解绑。未改十一阶段顺序。原observer拒绝第二owner/迟附着，组合owner直接注册一次Begin/End/Accept；未先Attach后覆盖。观测checksum在Driver scope之后，不当成scope内分配证明。

编译：两处新增测试写法曾CS0117/CS0266失败，原件保留，修为现有state12与显式byte。最新Query源17:20:13 UTC，Assembly-CSharp.dll17:20:19；Editor程序集17:18:04包含最终测试/Suite；最新脚本reload后没有新CS错误，随后focused执行成功。见`compile-evidence.json`。Editor idle本身未被当作编译通过。

| 原Editor测试证据 | 结果/版本 | 复用或重验依据 |
|---|---|---|
| focused01 / 6ba411a79507462bbd2bf067ffc0bd72 | 6/6 PASS：request4＋原GC owner2 | 与最新counter catch位置无关；原首次record过滤类名错，没有冒称record已跑 |
| focused02 / c103224c5ead4193bdf7bf293fb0cd98 | 12/12 PASS：record矩阵4、stride2、容量/group各1、kind1tie2、旧stride2 | 这些普通分支未受随后异常counter位置修复影响；新增晚到case当时未编译不计其中 |
| focused03/04 | 各completed8，唯一报告失败为storeInvalids应>0而为0；MCP result=null | 保留失败原件；其余7仅以进度无其它failure为限，不伪造完整逐case结果或全组PASS |
| focused05 / 4cbea15533db42009b0ecb328efdb5f1 | **2/2 PASS**：store fail-closed＋诊断ON warmed collector0GC | 最新源码重编译/reload后最窄重验；未改原业务fail-closed或弱化断言 |

更正记录：曾推测fault fixture未PrepareBattleCapacity导致计数0，补前置后仍失败；实际是本Task新增counter误插到更早同名异常catch。已删除仅自己误插的一行并加到具名writer catch，原业务逻辑始终未改。中间错误版本/真实dirty备份与修复操作全在同Operation；不把猜测当事实。最新四文件相对真实before的零上下文patch已保存，最终不存在该越界计数行。

未重跑83/91、旧四场景、已通过未受影响的正常record矩阵，也没有重测producer或95。历史Role限定准入/FindEffect证据不因本次失败被改写；同时它们不能豁免本次完整Driver失败。旧ABBA首OFF2event/tick130未知继续独立挂账：本次数字相同只是观察事实，**未证明调用点相同，未证明95/producer/诊断导致，未关闭旧问题**。

## 4. 实际执行命令与保护

通过PowerShell here-string→`python -` 使用已有127.0.0.1:6402 Unity MCP bridge：

- `refresh_unity {scope:scripts, compile:request, wait_for_ready:false}`；最新请求保存在compile-request-07.json。
- `run_tests {mode:EditMode,testNames:[NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests.RecordCost_StoreFaultPreservesOriginalFailClosedPath, NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests.RecordCost_WarmedCollectorZeroAllocation]}`；随后`get_test_job`到终态。早期请求和结果01—04原件保留。
- `get_editor_state {}`、`manage_scene {action:get_loaded_scenes}` 在续办/开窗前/窗口后分别保存。
- **仅一次** `execute_menu_item {menu_path:NTSD/Validation/Optimization/H06 Role Record Cost Diagnostic}`，见window-start.json。fixture固定Sakura combat1000/AI，seed1314149188，warm120/sample180，Role实际接线；没有第二menu或第二实例。
- `git diff --check` exit0，输出只有行尾提示（原件git-diff-check.txt）。
- `pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`：结果在本报告末尾验证追加和validator.txt；未运行前不声称通过。

`pre-window-fingerprints.json`/`post-window-protection.json`证明四脚本在实景前后完全相同，其余2510保护文件无漂移。没有改Scene/Prefab/资源/ProjectSettings/InputActions/Server/Gen/Plugins，没有读取活跃Q06方法体，没有reset/checkout/clean/stash/删除/push。原共享terminal在运行前已备份，每窗另存shared-terminal-before；未清旧诊断。主Operation的before-manifest与close-doc-manifest保留真实dirty字节，回滚只能另登记Operation后定点撤除本Task，不用HEAD覆盖他人内容。

## 5. 结束决定及依赖

本Task以**FAILED / FINITE_DIAGNOSIS_STOPPED**结束。诊断代码保留默认OFF，未提出或实施新生产优化；Change BLOCKED如实表示实景诊断验收未完成，不表示授权自动重试。只同步同一H06-ROLE-EXACT单项/唯一进度及必要依赖，不新增第35父项或旧96包。

若用户决定恢复，前置是明确新的有限范围：定位完整Driver首次tick130分配，并确认这套record观察合同中fallback的真实适用范围及逐子条件证据；不能直接放宽门或复跑同窗口找PASS。本轮不替用户启动该专项，也不自动恢复H07-INPUT/producer。一般H07-INPUT仍是另行评审建议，无固定收益承诺。

Role正常生产默认与Brute回退保持；producer USER_DEFERRED/OFF，95/84/89/92/94生产OFF。H06父项、Windows H07的33ms/无持续backlog/正常dropped0和Android最终设备认证继续开放。此次局部focused或干净关闭都不是H07/Android通过。

最终验证追加：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` **exit0 / PASSED**，1398 Records、当前dirty差异中10个governed code files均覆盖（本Task仅四个；其余是保留工作）。4341条WARNING保留，主要是历史Record声明路径不在当前diff，不是本任务新增改动。`git diff --check`最终exit0。初始真实dirty备份11/11 SHA吻合，四源仍与开窗前一致，实景report恰1份，ON未运行。Task FAILED有限停止/Change BLOCKED默认OFF结论不变。
