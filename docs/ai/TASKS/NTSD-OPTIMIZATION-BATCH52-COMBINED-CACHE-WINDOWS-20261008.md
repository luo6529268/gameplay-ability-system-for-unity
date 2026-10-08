# 第52批：H-07 缓存组合真实 Windows 冒烟
最终：SCOPED_WINDOWS_AB_COMPLETED / MIXED_RESULT / NOT_ADMITTED / PERFORMANCE_FAIL。11有效RED→28/28 GREEN、四120+180有效；两布局终态20hash/snapshot同，flags/0残留/Scene保持。分散变慢/混战小信号，ON P95162.293/113.060ms和drop1181/664未过，GC UNKNOWN，组合保持默认false且不推广。31已执行、4/6阶段条件、5/6产物、父关闭0、Goal active。下方PLANNED是事前合同，实际详REPORT；不重复本窗找PASS。
状态：PLANNED。Change：NTSD-OPT-H07-COMBINED-CACHE-WINDOWS-052。
依据：用户继续未完成优化、有限首阶段合同第0—8节；51已通过组合Driver资格。本包只测组合新增收益，不重跑51/46/49 Driver资格，不切生产默认。
当前前序状态：30已执行子批、六项阶段4/6、限定产物5/6、34父关闭0；H07/H11 OPEN，Goal active。上一用户状态询问不计优化进展。本轮须实现并验证必要组合窗口，而非重复状态报告。

## 范围与预先冻结
唯一C#：Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs。
准确符号：RunState / SuiteState新增组合配置字段；BeginBruteCombinedCache / BeginSuite / BuildCurrentRequest / BuildBruteCombinedCacheRequest；StartCurrentRun / Update的终态与workloadValid；ApplyBruteCombinedCache / CompleteBruteCombinedCache；OnPlayMode / IsOwnedTerminalOrAbsent；同文件RequestTests新增11case。
原四普通Brute默认保持ON；ForceBruteForce不变；eligibility全四窗ON，kind5按OFF/ON；binding48、branchTiming OFF。旧家族Apply/Complete guard不削弱，新增独立组合helper，先Apply eligibility再kind5，终态先验证组合，再恢复kind5，最后沿旧Complete eligibility恢复；异常/外部Play exit恢复两owner引用，再沿既有11阶段关闭，不新建runtime owner。
既有六进度/治理文件：Ledger、STATE、handoff、FILEINDEX、原tracker、原H07方案；7当前dirty文本精确副本＋58只读guards见before.json。Q06只hash，不读方法体。
资源/Scene/Prefab/settings/Input/Gen/Plugins/Server、PERF/ATLAS/Mono/EXT1不写。33ms/3ms/max2、checksum/RNG/pair顺序/AI输入/透排/segment/failclosed不变。

## 测试与实际窗口
test-first 11case：request4＋越界1；组合apply/complete/restore OFF/ON2；elig/kind5 drift2；binding/timing拒绝2。缺helper先有效RED，最小实现后11 GREEN＋受影响旧eligibility12及kind5 request3/原值恢复2（总28），不跑全历史campaign。新增两个旧restore case因组合复用该helper，是影响域，不重跑Driver资格。
四实际窗口根 artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH52-COMBINED-CACHE-WINDOWS-20261008/windows-01：00-dispersed1000-kind5-off、01-dispersed1000-kind5-on、02-combat1000-kind5-off、03-combat1000-kind5-on。每窗只沿BuildBruteProductionRequest(index/2)修改outputPath，全部既有65字段不变：真实1000AI，120 warmup＋180 sample，seed/input/roster/布局/renderer/原GameView配置不改变。source freeze和request文件逐项比较后启动一次菜单，不在Play内编辑/refresh。
门：每窗warm+sample=300、1000active/roster、原四productionDefaults/roster/cache/geometry应用300/fallback0；eligibility300；kind5 OFF0/ON300，flags配置与恢复成功；终态完整snapshot和extended/lockstep20hash每布局同；Scene SHA/dirty不变、11阶段退出objects/slots/borrowers0。完整GC仍用既有UNCALIBRATED_COUNTER/UNKNOWN，raw0不能提升。
同版一次OFF→ON/layout为收益信号，不是统计稳定/Android/120FPS证书，不把kind5省scan频次当时间比例；P95/drop未过则H07保持未达。正式120+1800仍是阶段必需门，短窗不代替。本批不先无效长窗找PASS。

## 文件操作、恢复与限制
先新建Operation/Task/Change、核7dirty副本和Temp/NTSD_ProductionEntityStress.result旧50 PASS副本，再登记索引后改C#。SharedRequest不存在，不删除。既有runner将在4窗覆盖自有SharedResult，每窗完整terminal另存新目录；预先授权范围仅这一个自有temp结果，不覆盖旧artifact。
所有文本编辑apply_patch；副本Copy-Item至新精确路径并SHA核验。回滚仅本包反向hunk且另Operation授权，不reset/restore/delete；HEAD不能恢复dirty。初次只读manifest调用10s返回被脚本误判，未保存句柄/无写入；第二次30s只读成功，before为实际成功原件。
交付前Validate-ChangeLedger.ps1、diff --check；如实区分focused、实景测量、可靠GC与正式门。数值/次数字段不是自动停止边界。

