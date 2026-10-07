# 第32批 Brute 空itr guard

当前 SCOPED_GAIN_NOT_ADMITTED / RUNTIME_PENDING：一次两真实1000AI短窗已完成，候选有降耗证据但仍远未达标，默认仍未启用。Goal active、有限交付5of6、父关闭0、新批累计11。RUNTIME_PENDING指整体准入/native/逐tick/完整生产0GC/性能及设备待，不是短窗待运行。

## 实际测量

| 120warm + 180sample | Dispersed1000 | Combat1000 |
| --- | --- | --- |
| 旧26 → 本32逻辑tick mean（ms） | 467.5241 → 231.6160 | 431.0003 → 236.7672 |
| 逻辑tick P95（ms） | 706.7583 → 341.8422 | 691.9200 → 360.1397 |
| CandidateCollect mean（ms） | 429.1872 → 201.0026 | 398.0743 → 206.5606 |
| 本32 collector/tick 占比 | 86.78% | 87.24% |
| 可见Unity帧间隔 mean（ms） | 1116.8773 → 614.4679 | 1037.4795 → 582.8055 |
| dropped backlog ticks | 4701 → 2418 | 4337 → 2272 |
| 实际观测AI / base roster下限 | 1000 / 1000 | 1000 / 1000 |
| 末tick300十hash / 完整snapshot | 同26 | 同26 |
| capacity critical delta | 0 | 0 |
| guard applied / restored | true / true | true / true |
| workloadValid / harnessValidity / terminal | true / true / StoppedCleanly | true / true / StoppedCleanly |
| raw GC / 可靠0GC结论 | raw PASS / UNCALIBRATED_COUNTER · UNKNOWN | raw PASS / UNCALIBRATED_COUNTER · UNKNOWN |

[comparison](comparison-01.json) / [Suite原件](windows-01/suite-result.json)。旧26为非同期同请求基线，不称同步A/B；每套65字段仅outputPath不同，guard由Suite在Configure后且0warm/sample时打开、run结束和shutdown fallback复原，不是request字段。对旧基线logic mean约降50.46%/45.07%、collector约降53.17%/48.11%，但仍远超33ms，可见帧614/583ms。不得称解决不到1FPS、Android或120FPS；H11完整严格FAIL不撤回。

## 代码、测试和实际命令

1. 准确3脚本：BruteForceSceneQuery.cs默认false的空itr前置纯guard/测试计数；RoleAwareCollisionShadowSelfCheckTests.cs的7新case；BattleOptimizationWindowsAiSuiteEditor.cs的新32菜单/2request校验/非法index/owner复原。无默认collector切换、持久cache、新资源、lease、容量或shutdown stage。GetCollisionFrameData当前只是Prev2帧数组读取，Animation域无override；未来实现变更须重评纯读取前提。默认新增分支耗时未单独测量。
2. 原6401 refresh编译，两类EditMode有效RED10具名失败 → job44f1a438b67e467198f1e3e4e856ac42 GREEN实际130/130（10新增+120原回归），10.8421s。首编译错误/旧程序集0实际用例无效RED保留，不能算PASS；catalog9381不等于实际测试数。当前Runtime/Editor程序集时间晚于源修改，编译已完成。
3. 固定1000逻辑参与者（40有itr+960无itr）4warm/8旧新交替sample，payload/顺序/RNG/Kind4断言通过，pair gate999000→39960/轮，291.9617→114.7383ms（约60.7%）。此为collector夹具，不是自然1000AI/FPS/0GC证书。
4. execute_menu_item(menu_path=NTSD/Validation/Optimization/Batch32 Brute Empty Itr Guard)只一次；原两个actual1000AI120+180跑完。原PID19040未重启，无第二Editor、测量期间refresh/额外测试/Assets脚本或文档写入；无Profiler/FrameDebugger/GPUcapture/EXT1专项M0。
5. 两末tick300十hash及完整snapshot JSON字节SHA同26：Dispersed F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA；Combat E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12。仅两个Unity末tick对照，非逐tick/native权威证书。
6. 原11阶段shutdown三残留objects/slots/borrowers0；get_editor_state非Play/idle，manage_scene原Menu8roots clean，Menu/Battle Scene SHA同。238保护/11当前备份/HEAD保持，三个源码SHA与事前冻结同。Validate-ChangeLedger PASS1328Records/11governed C# diff（4248历史warning），tracked diff --check无问题。Suite未跟踪文件no-index check exit1表示差异，未报告空白诊断，不冒充普通exit0 PASS。

## 审计与下一边界

[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH32-BRUTE-EMPTY-ITR-GUARD-20261007.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-H07-BRUTE-EMPTY-ITR-GUARD-032.md) / [文件操作](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH32-BRUTE-EMPTY-ITR-GUARD-20261007/RECORD.md)，最终final-audit-01.json和Operation after.json保留。file-operation VERIFIED仅声明写域/备份保护，不是优化准入VERIFIED。

frozen-request-audit-01的guardOwnedBySuiteStateNotRequest因脚本true未写$true而保存null，原件保留；实际bool证据来自suite-result和observation。comparison分析首次PSObject.Properties.Count被枚举成1数组，保存前改为@(...).Count重读修正65/10，无重新测量。关闭文档首patch因同目标两Update操作被工具拒绝、未应用，合并同目标hunk后正常应用；此前Suite不匹配/拼接和无效RED历史原件保持。

默认false/原Brute选择不变；H07/H11仍OPEN。collector仍约87%，不重复此版相同窗口刷PASS；下一复用既有role候选15ms级collector证据评估必要准入缺口，不新增无证索引架构、不自动切默认。Screenshot CPU主/渲染线程不能单独裁定GPU成本，CPU DrawMesh/segment不等于真实GPUbatch。完整0GC、native逐tick、Android/120FPS和Q06透明排序细节未本批证明。没有读取Q06活跃方法体、改变33ms/3ms/max2或解冻EXT1/ATLAS/Mono；没有删除、reset/restore/push。以下为事前/中间历史。

当前FOCUSED_TEST_PASS / REAL_AI_PENDING：原Editor130/130 PASS/10.8421s，固定40有itr＋960无itr1000逻辑夹具4warm/8旧新交替sample，关系门999000→39960/轮、291.9617→114.7383ms，约60.7%降耗。非真实AI/FPS/0GC；只有后续固定两1000AI短窗可以证明对应工作负载。默认false/原collector不切，H07/H11仍未达成。有效RED10失败与第一次编译错误/旧程序集0用例原件保留。
PLANNED：尚未改代码或测得收益。准确scope/固定矩阵见同名Task，默认不推广、H07/H11/Goal开放；旧混合收益和可靠0GC FAIL保留。
