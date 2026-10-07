<!-- CHANGE-RECORD
id: NTSD-OPT-H07-BRUTE-EMPTY-ITR-GUARD-032
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
authority: approved H07 continuation section13; preserve formal336 and default; read-only empty-itr return candidate only
evidence: artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH32-BRUTE-EMPTY-ITR-GUARD-20261007/REPORT.md
-->

# H07 Brute 空攻击框方向前置guard

当前 RUNTIME_PENDING / SCOPED_GAIN_NOT_ADMITTED。固定两真实1000AI120+180短窗已完成、不是待运行；两套request各65字段仅outputPath异于26、Suite guard applied/restored均true。logic mean467.5241→231.6160 / 431.0003→236.7672ms，P95706.7583→341.8422 / 691.9200→360.1397ms；collector429.1872→201.0026 / 398.0743→206.5606ms。旧26为非同期同请求基线，不称同步A/B或准入证书。可见帧614.4679/582.8055ms、collector仍约87%，H07未优化完成；raw GC PASS仍UNCALIBRATED/UNKNOWN，H11完整严格FAIL不撤回。

两末tick300十hash及完整snapshot字节SHA均同26，容量critical delta0，teardown restored/11阶段三残留0，原Menu8roots clean/非Play，双Scene不变；238保护/11当前备份/HEAD、三个源SHA冻结保持。post-measurement及最终audit/after manifest见本32证据目录；Validate-ChangeLedger PASS1328Records/11governed C# diff、4248历史warning、tracked diff --check无诊断。默认false/原Brute选择仍未改，不推广、不追加同构长窗。状态待的是native逐tick准入、完整生产0GC与整体性能/设备，不把限定检查通过晋升VERIFIED。

真实命令原6401 execute_menu_item(menu_path=NTSD/Validation/Optimization/Batch32 Brute Empty Itr Guard)一次；get_editor_state、manage_scene只读核退出，原PID19040未重启，无第二Editor/Profiler/capture/M0/Scene资源写入。实际代码仅本Record三路径；默认新增分支成本未独立测量，不称默认路径零额外耗时。getter未来override/实现变更须重评纯读取前提，未跑当前权威native对照。

审计元数据更正：frozen-request-audit-01的guardOwnedBySuiteStateNotRequest因生成脚本true未写$true保存为null，原件保留，不作为已验证bool；当前suite-result两run与observation均提供实际guard applied/restored true。comparison分析首次PSObject.Properties.Count被PowerShell枚举成1数组，写入前已用@(...).Count重读修正为65/10；未重跑性能窗口或改原报告。

pre-run-audit-01 PASS：238保护/11备份/HEAD与原31terminal保持，validator1328Records/11files PASS（4248历史warning），tracked diff check无问题，原Menu8roots clean/idle/无测试。准备单次32菜单，运行期不编辑C#/Assets文档、refresh/额外测试/重启；GC raw未校准门不放宽，实际收益/末tick/关闭尚待。

GREEN原Editor job44f1a438b67e467198f1e3e4e856ac42实际130/130 PASS（10新增＋120原相关），10.8421225s，新程序集/编译完成。固定1000逻辑参与者4warm/8旧新交替sample，关系门999000→39960/轮、mean291.9617125→114.7382875ms（约60.7%），完整payload/顺序/RNG/Kind4断言通过；这是40有itr＋960无itr的collector夹具，非实际AI/FPS/0GC。原PID一次reload6401暂拒后原PID恢复，未重启。下一原32菜单一次两真实1000AI短窗/末tick300与原26brute对照，默认未启用，暂无真实收益。

Suite准确patch现已应用：新增独立32两brute request/菜单，Configure后且0warm/sample才在同Brute query打开，run末与shutdown fallback复原bool；原6/29/30/31入口/请求保持，owner只新增本32输出归属。两patch匹配失败与一次无效拼接验证均未变更Suite，最终从原patch重建并以实际StartsWith条件匹配。Runtime＋Suite CODE_WRITTEN，下一原两类GREEN/局部成本/一次两真实AI待，不称收益或默认准入。

有效RED job8d35c25fd5f340419b755cf1cece3087有10个具名失败，原件red-01.json。Runtime已写默认false的纯空itr候选与测试计数，Suite仍是request stub，尚未编译/GREEN/实测。Suite文本patch的源码owner格式/hunk顺序/拼接验证失败均未应用，当前按准确源码修正patch，不绕过验证或触及其它文件。下一Suite准确写入后两类GREEN；不得把未实现的menu称已可运行。

首RED派发 e258738fd05f4dc6936acf1b5eb785dc 是0实际用例/旧程序集，NOT_A_TEST_PASS/NOT_VALID_RED，原件invalid-red-zero-tests-01.json保留。原因仅新增Suite test漏用此文件既有NUnit全限定名（5条CS0246/CS0616），已只更正新属性/断言全限定；Runtime仍未改。下一编译错误确认清除后具名10有效RED，不以MCP editor_state idle冒充新程序集可用。

test-first已写7 collector case＋3 request case，Runtime未改；reflection缺少候选flag/counters、request stub NotImplemented作为RED。下一原Editor编译后具名10RED，不跑Scene/性能。准确11当前副本和238保护均已存before.json，原HEAD/PID保持。
PLANNED，脚本尚未改。准确方法、7＋3 RED/GREEN、1000fixture4warm+8sample、一次两个真实1000AI120+180、原末tick300JSON、关闭/保护/validator冻结在[Task](../TASKS/NTSD-OPTIMIZATION-BATCH32-BRUTE-EMPTY-ITR-GUARD-20261007.md)。原默认Brute瓶颈约398–429ms/collector，不用Role短窗代替默认改善。先新诊断false控制，未获native准入/用户默认切换不推广。

读取副作用核实：LF2Entity.GetCollisionFrameData→LF2FrameCache.GetNativeFrameDataById只有Prev2/数组读，Animation域无override。空itr方向既有最终return，提前只省无效pair门；非空、同帧RNG/Kind4/容量/顺序保持。每次重查，无跨tick缓存，新field只是诊断，Suite owner明示打开/结束restore。无新模块或shutdownstage，遵循现有十一阶段。不可回退边界和准确Operation/current-byte恢复、未测native/Android/0GC以及原dirty保护均见Task。需恢复先准确批准，不reset/restore/push。后续追加真实命令/失败/通过，不把待测写收益。
