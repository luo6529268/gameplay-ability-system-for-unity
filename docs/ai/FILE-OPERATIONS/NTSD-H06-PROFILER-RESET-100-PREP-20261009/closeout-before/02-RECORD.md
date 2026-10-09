# NTSD-H06-PROFILER-RESET-100-PREP-20261009 / PLANNED

时间：2026-10-09T19:43:21.601460+08:00。执行者：当前方案评审会话 01a11fe2-bfaa-72f3-a9ed-c8260f63b193。
用户授权：本会话“那就先把旧历史数据删除了，然后重新生成的数据，你觉得如何”及后续“那就开始处理吧，这部分属于准备阶段，不算处理优化代码逻辑，所以你可以直接处理”。仅Profiler内存缓存清空、100对象诊断准备与短采集；不恢复旧自动Goal、其它会话或优化逻辑实施。

Task：docs/ai/TASKS/NTSD-H06-PROFILER-RESET-100-PREP-20261009.md；Change：NTSD-H06-PROFILER-RESET-100-PREP-001。
准确清理：原项目Editor PID74724 / localhost6402，在idle/非Play/无编译/无其它测试且Profiler/deep/GPU关闭时，调用UnityEditorInternal.ProfilerDriver.ClearAllFrames()。旧Profiler内存历史无法完整导出，用户已明确授权清空；不假称有可恢复的完整备份，不再导出旧历史。磁盘文件删除清单为空；所有旧raw/JSON/XML/报告保留。

文件写域及真实dirty备份见before-manifest.json。五个治理文档仅追加/文首新增本任务指针；公共Temp测试XML每次自然替换前保存现存版本并核SHA。唯一新项目脚本H06CollectorPreparationEditorTests.cs及Unity生成同名meta；旧Admission/生产/Suite/Scene/Prefab/资源/ProjectSettings/InputActions/Gen/Plugins不改。保护1192项当前字节见protection-manifest.json；HEAD和staged状态已保存。

拟执行：既有MCP manage_asset(import准确新脚本)、run_tests(EditMode,准确具名Explicit清理/100对象定位)、get_test_job及read_console；先校验新程序集，所有结果独立新UUID。保留76普通/24特殊、50/50阵营、原局部距离和碰撞框；4warm及3个校准preflight样本在详细录制外，最多3次Role收集详细采集，不跑Brute长窗、不改变千人资格门。
Profiler设置及各area事前落盘，try/finally关闭自有录制/释放本次纯逻辑World/恢复本轮已知设置并前后比较。失败仍落盘；不重读活跃Q06方法体。分配栈元数据可以报告符号/行号，不据此打开禁读实现。

早期只读失败：unity status无Pipeline不能当Editor关闭；原MCP确认idle。execute_code只读查询因CodeDom命令长度限制失败，未执行目标代码；不安装/修改第三方包，改用新具名Editor准备测试安全编译。所有正式Unity调用只用原Editor，不启动第二实例。
回滚：本任务新增与文档增量可独立定位；保留全部事前dirty备份，不自动删除/回退。缓存清空后未导出细节不可恢复，已由本次明确授权覆盖。

CODE_WRITTEN：新准备脚本已登记，下一仅import准确路径；新meta由Unity生成。

原Editor新程序集UTC11:46:30晚于新脚本11:46:08，reload完成、idle；开始准确ClearAuthorizedProfilerHistory。公共XML已保存tests-before-reset.xml。

ClearAuthorizedProfilerHistory job a75502e05f5348de91824b657cf82967 1/1 PASS；first/last 0/7→-1/-1，完整设置/各area相同；无磁盘删除。reset-tests.xml保留，开始Capture100AllocationCallsites；公共XML替换前再备份tests-before-capture100.xml。

100对象首轮job ba2d06104500485cafb5486914b1dfea 1/1PASS，新raw 9912324B成功导出，3marker/66event、40B/event，首个解析managed frame LoganKindCatalog.FindEffect line71。发现preflight在recorder.Begin后立即Assert造成额外分配：29计数不可作纯collect结果，调用栈marker内22/call有效。原测试源码/XML保存revision-01-assertion-contamination；下一仅把断言移到scope结束后并补已解析managed栈断言，保留旧产物，不改生产。修订后只重跑本准备项。新缓存已完整raw落盘，可以清空后再复核。

reset2 job e31b17726bf142d4b747b7381b2eabe8 {"total": 1, "passed": 1, "failed": 0, "skipped": 0, "durationSeconds": 5.7383224, "resultState": "Passed"}; XML SHA DB3664AF6A0A346941E6B6698CC95AB8B51173AA27B70E67BC8DB97690A81EE4。

capture100-revision02 job 78eebe0088994b8ead7b77c04fd6eaac {"total": 1, "passed": 1, "failed": 0, "skipped": 0, "durationSeconds": 2.0642197, "resultState": "Passed"}; XML SHA 0C8C9245CD0012BE68A479E005AB8EBF933B41CF78D134E5BA9F6119A51AEB92。
