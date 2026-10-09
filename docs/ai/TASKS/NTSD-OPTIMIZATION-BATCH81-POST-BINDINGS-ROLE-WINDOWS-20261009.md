# 第81批：共享绑定修复后的现有 Role-aware 候选对照
状态 SCOPED_DIAGNOSTIC_COMPLETED / H07_PERFORMANCE_FAIL / NO_DEFAULT_PROMOTION；仅当前H07受控诊断，完整阶段门与生产准入仍未完成。日期2026-10-09（UTC 2026-10-08T16:12:19.3434521Z）；下文准备/旧筛选状态按时序保留。

## 需求、必要性与不变范围

需求为已批准六项Goal有效§0—8及用户“未优化好继续”；80优先SetPass修复已实测1995→5，逻辑mean87.920/P95112.167/drop725仍FAIL、CandidateCollect57.166ms。现有普通Brute仍按authority序遍历i/j（当前BruteForceSceneQuery:2432—2470），诸缓存不会消除成对遍历；旧29/30 Role-aware资格与性能原件复用，30受旧GC工具影响的FAIL绝不晋升。80之后的current共享绑定/普通缓存/校准scope组合没有当前Role-v-Brute证据，因此不是重复H06评估或旧30长窗找PASS，也不再堆Brute微候选。先验证既有路径收益、完整终态一致性和可靠fullLogic GC，再据结果选择后续；生产默认切换仍需准确授权，未在本包实施。

## 准确写域

唯一C#：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs；新增冷菜单、独立mode/session字段、固定request builder、实际collector观测/失败拒绝、共享terminal覆盖前副本及同文件18个test-first case。不改变原17参数BeginSuite、任何旧request/断言、统计warmup门或退出顺序。
文档只本Task/Change/Operation/REPORT及原五治理入口；公共输出仅原Temp/Goal18_LastTestResults.xml和Temp/NTSD_ProductionEntityStress.result，覆盖前逐次备份。独立新artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH81-POST-BINDINGS-ROLE-WINDOWS-20261009/windows-01，ABSENT gate/不改旧结果。

## 冻结矩阵和验收

唯一四run：Dispersed1000 Brute/Role、Combat1000 Brute/Role；每个120warm+180sample/真实1000AI/OID1 Sakura，同一BuildBruteProductionRequest布局基线，组内仅formalCollectorMode和outputPath不同；全部共享绑定true，不配置任何独立Brute候选或branch timing，不改场景/renderer/cadence/input/seed/spawn/AI。当前默认Brute四flag true、其它flags false保持。不启用instancing/专项M0。
新18：四request完整字段、两越界、一次Session向后兼容、五非法mode/冷配置拒绝、六实际观测必要门；先有效RED再实现；受影响旧中央readback request5（原4为计数笔误，request1/越界2/error往返2）、80request3及既有Role formal Direct/tree/等距RNG/失效退回/例外恢复/degenerate资格按精确名称一次窄复用，不重跑旧cost/全SelfCheck。
run必须beforetick实际query.FormalCollectorMode与请求相符，sample观测LastFormalCollectorMode实际路由相符/非aborted，绑定开关/default/candidate配置无漂移；观测只覆盖已采collection，不凭last值假报所有tick无fallback。沿既有BattleLogicTickGcObserverEditor完整StepOneTick前/后正负校准，记录覆盖300、steady180事件，不从字节未知value虚构0B。完整末tick300 snapshot逐字节与同版基线比；CPU phases/SetPass/CPU Draw各独立实际report，无GPU batch推断。
保持11stage关闭/World对象、slot、pool借用0，保存原Menu sole clean8/双Scene SHA与资源/611旧guard及80原件。指标达标判断仍P95<33/drop0及正式120+1800等未覆盖门；本冒烟不等完整H07/Android/120FPS/透明像素/H11 camera资格。

## 所有权、失败和回滚

runner新诊断World按请求选择既有collector，不修改生产Scene/profile或算法。productionQuery只观察本runner owner，完整逻辑GC沿原Attach/Finish；无新runtime queue/cache/容量或关闭模块。观察数据仅Editor冷/既有Update诊断，不增加模拟内observer或反写publication。现有公共terminal每StartRun先File.Copy到唯一新owned output并拒绝存在；原before副本保留。源码必须通过后才开始窗口，运行中不编辑/import/重启；观察超时核同job而非重发。
真实checksum/配置/清理异常立即保留失败，不能继续推广；性能无收益则不采用、转另一当前热点/必要权限，不重复采样找PASS。回滚另请求精确批准，仅本包hunk/操作副本，不reset/restore旧HEAD或删除其它改动。

## 状态和留痕

Change NTSD-OPT-H07-POST-BINDINGS-ROLE-WINDOWS-081，Operation NTSD-OPTIMIZATION-BATCH81-POST-BINDINGS-ROLE-WINDOWS-20261009。主总表仍唯一；58执行＋81准备、4/6/H07H11 OPEN/34父关闭0/Goal active，79暂未执行。80子项结果保留，不因次数或报告停止Goal；本包无默认推广授权，若候选准入需生产切换会列准确范围另请批准。

GREEN准确窄矩阵：PostBindingsRole_*18；CentralSegmentReadback_(ErrorSessionRoundTrip/RequestOnlyOutput/OutsideMatrix)5；BattleCommonAtlasBindingEditorTests.MatchWindow_*3；RoleFormalRequest_*5；RoleAwareCollisionShadowSelfCheckTests.Formal_(ForcedDirectAndTreeMatchForZeroOneAndManyItrs)3、EqualDistanceKind1TieMatchesLegacyRngAndReplacement2、PreLoopExactCacheInvalidationFallsBackAndRestoresRng1、DegenerateTemplateUsesConservativeIndexAndClampFallsBackWithParity1、ExceptionRestoresRngAndCandidatesThenRunsFullBruteFallback1；预计39，实际XML为准。旧source/测试保持，不改断言。RED18原件已保留；导入新Suite编译后原Editor idle。

GREEN1实际31/31；上述旧formal8因filter误用NTSD.Test.Editor未执行，不称39通过。补测仅准确NTSD.Test.RoleAwareCollisionShadowSelfCheckTests命名空间的同8，GREEN1已备份，不重复31、不增加用例。随后四窗矩阵及所有不变量保持。

更正：该源码包含多个fixture，上述NTSD.Test.RoleAwareCollisionShadowSelfCheckTests只是文件首类，并非formal方法所在类。原job8d511081400f4e69a95de3c70d4ff0c2终态succeeded但summary total0，不计通过。只读get_tests已核实际full_name为NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests.Formal_ForcedDirectAndTreeMatchForZeroOneAndManyItrs(0/1/3)。下一按实际fixture补同8；旧0用例原件保留，不改源码/断言。

2026-10-09 完成追加：准确formal8/8与原31合计去重39具名PASS后，唯一四窗已自然完成；两65字段request配对仅collector/output不同、每窗actualAI1000/120+180，角色输入/33ms/绑定与缓存配置未变。两个完整末tick300文件同，四可靠同步StepOneTick steady180scope0event，四run正常关闭/原Menu clean8/保护保持；详本批REPORT与measurements-summary-01.json。Role收集约-73%/整tick约-47%—48%，但P9550.438/52.710/drop276/271仍FAIL，短窗不代替正式120+1800/native/H11完整camera准入。无Role生产默认推广；当前只SCOPED_DIAGNOSTIC_COMPLETED，不以报告停Goal或关闭H07，59执行/阶段4of6/H07H11 OPEN/79未执行。下一以新鲜热点和准确授权边界推进，不重复本轮窗、旧H06或专项。

